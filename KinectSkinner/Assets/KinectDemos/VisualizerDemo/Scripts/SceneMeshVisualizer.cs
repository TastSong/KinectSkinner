using UnityEngine;
using System.Collections;


public class SceneMeshVisualizer : MonoBehaviour
{
	[Tooltip("Minimum tracked distance from the sensor, in meters.")]
	public float minDistance = 1f;
	
	[Tooltip("Maximum tracked distance from the sensor, in meters.")]
	public float maxDistance = 3f;
	
	[Tooltip("Maximum left and right distance from the sensor, in meters.")]
	public float maxLeftRight = 2f;
	
	[Tooltip("Whether to include the detected players to the scene mesh or not.")]
	public bool includePlayers = false;

	[Tooltip("Time interval between scene mesh updates, in seconds.")]
	public float updateMeshInterval = 0.1f;

	[Tooltip("Whether to update the mesh, only when there are no players detected.")]
	public bool updateWhenNoPlayers = false;

	[Tooltip("Whether the mesh is facing the player or not.")]
	private bool mirroredScene = true;
	
	[Tooltip("Camera used to overlay the mesh over the color background.")]
	public Camera foregroundCamera;

	[Tooltip("Whether to update the mesh collider as well, when the user mesh changes.")]
	public bool updateMeshCollider = false;

	[Tooltip("Number of pixels per direction in a sample.")]
	private const int sampleSize = 2;
	

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

	private KinectManager manager = null;

	private KinectInterop.SensorData sensorData = null;
	//private Vector3[] spaceCoords = null;
	private long lastSpaceCoordsTime = 0;
	private Matrix4x4 kinectToWorld = Matrix4x4.identity;

	private float lastMeshUpdateTime = 0f;

	private int colorWidth = 0;
	private int colorHeight = 0;
	
	private int depthWidth = 0;
	private int depthHeight = 0;

	private int sampledWidth = 0;
	private int sampledHeight = 0;

	private int minDepth = 0;
	private int maxDepth = 0;

	private Vector3 sceneMeshPos = Vector3.zero;

	private byte[] vertexType;
	private int[] vertexIndex;


    void Start()
    {
		if (foregroundCamera == null) 
		{
			foregroundCamera = Camera.main;
		}

		manager = KinectManager.Instance;
		if (manager != null && manager.IsInitialized())
        {
			sensorData = manager.GetSensorData();

			minDepth = Mathf.RoundToInt(minDistance * 1000f);
			maxDepth = Mathf.RoundToInt(maxDistance * 1000f);

			colorWidth = manager.GetColorImageWidth();
			colorHeight = manager.GetColorImageHeight();
			
			depthWidth = manager.GetDepthImageWidth();
			depthHeight = manager.GetDepthImageHeight();
			
			sampledWidth = depthWidth / sampleSize;
			sampledHeight = depthHeight / sampleSize;

			if(sensorData.depth2SpaceCoords == null)
			{
				sensorData.depth2SpaceCoords = new Vector3[depthWidth * depthHeight];
			}

			sceneMeshPos = transform.position;
			if(!mirroredScene)
			{
				sceneMeshPos.x = -sceneMeshPos.x;
			}

			vertexType = new byte[sampledWidth * sampledHeight];
			vertexIndex = new int[sampledWidth * sampledHeight];

			CreateMesh(sampledWidth, sampledHeight);
        }
    }

    private void CreateMesh(int width, int height)
    {
        mesh = new Mesh();
		mesh.name = "SceneMesh";

        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void Update()
    {
		if (manager == null || !manager.IsInitialized())
			return;

		// get user texture
		Renderer renderer = GetComponent<Renderer>();
		if(renderer && renderer.material && renderer.material.mainTexture == null)
		{
			renderer.material.mainTexture = manager.GetUsersClrTex();
			renderer.material.SetTextureScale("_MainTex", manager.GetColorImageScale());
		}

		// update the mesh position
		sceneMeshPos = transform.position;
		if(!mirroredScene)
		{
			sceneMeshPos.x = -sceneMeshPos.x;
		}

		// get kinect-to-world matrix
		kinectToWorld = manager.GetKinectToWorldMatrix();

		// update the mesh
		UpdateMesh();
    }
    
    private void UpdateMesh()
    {
		if(sensorData.depthImage != null && sensorData.depth2ColorCoords != null && 
			sensorData.depth2SpaceCoords != null && lastSpaceCoordsTime != sensorData.lastDepth2SpaceCoordsTime)
		{
			if ((Time.time - lastMeshUpdateTime) >= updateMeshInterval &&
				(!updateWhenNoPlayers || !manager.IsUserDetected())) 
			{
				int vCount = 0, tCount = 0;
				EstimateSceneVertices(out vCount, out tCount);

				vertices = new Vector3[vCount];
				uvs = new Vector2[vCount];
				triangles = new int[6 * tCount];

				int index = 0, vIndex = 0, tIndex = 0, xyIndex = 0;
				for (int y = 0; y < depthHeight; y += sampleSize)
				{
					int xyStartIndex = xyIndex;

					for (int x = 0; x < depthWidth; x += sampleSize)
					{
						Vector3 vSpacePos = sensorData.depth2SpaceCoords[xyIndex];

						if(vertexType[index] != 0 &&
							!float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
						{
							Vector2 vColorPos = sensorData.depth2ColorCoords[xyIndex];
							if(!float.IsInfinity(vColorPos.x) && !float.IsInfinity(vColorPos.y))
							{
								uvs[vIndex] = new Vector2(Mathf.Clamp01(vColorPos.x / colorWidth), Mathf.Clamp01(vColorPos.y / colorHeight));
							}

							// check for color overlay
							if (foregroundCamera) 
							{
								// get the background rectangle (use the portrait background, if available)
								Rect backgroundRect = foregroundCamera.pixelRect;
								PortraitBackground portraitBack = PortraitBackground.Instance;

								if(portraitBack && portraitBack.enabled)
								{
									backgroundRect = portraitBack.GetBackgroundRect();
								}

								ushort depthValue = sensorData.depthImage[xyIndex];

								if(!float.IsInfinity(vColorPos.x) && !float.IsInfinity(vColorPos.y) && depthValue > 0)
								{
									float xScaled = (float)vColorPos.x * backgroundRect.width / sensorData.colorImageWidth;
									float yScaled = (float)vColorPos.y * backgroundRect.height / sensorData.colorImageHeight;

									float xScreen = backgroundRect.x + xScaled;
									float yScreen = backgroundRect.y + backgroundRect.height - yScaled;
									float zDistance = (float)depthValue / 1000f;

									vSpacePos = foregroundCamera.ScreenToWorldPoint(new Vector3(xScreen, yScreen, zDistance));
								}
							}

							if(!mirroredScene)
							{
								vSpacePos.x = -vSpacePos.x;
							}

							if(foregroundCamera == null) 
							{
								// convert space to world coords, when there is no color overlay
								vSpacePos = kinectToWorld.MultiplyPoint3x4(vSpacePos);
							}

							vertices[vIndex] = vSpacePos - sceneMeshPos;
							vIndex++;

							if(vertexType[index] == 3)
							{
								if(mirroredScene)
								{
									triangles[tIndex++] = vertexIndex[index];  // top left
									triangles[tIndex++] = vertexIndex[index + 1];  // top right
									triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom left

									triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom left
									triangles[tIndex++] = vertexIndex[index + 1];  // top right
									triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom right
								}
								else
								{
									triangles[tIndex++] = vertexIndex[index + 1];  // top left
									triangles[tIndex++] = vertexIndex[index];  // top right
									triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom left

									triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom left
									triangles[tIndex++] = vertexIndex[index];  // top right
									triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom right
								}
							}
						}

						index++;
						xyIndex += sampleSize;
					}

					xyIndex = xyStartIndex + sampleSize * depthWidth;
				}

				// buffer is released
				lastSpaceCoordsTime = sensorData.lastDepth2SpaceCoordsTime;

//				lock(sensorData.spaceCoordsBufferLock)
//				{
//					sensorData.spaceCoordsBufferReady = false;
//				}

				// update the mesh
				mesh.Clear();
				mesh.vertices = vertices;
				mesh.uv = uvs;
				//mesh.normals = normals;
				mesh.triangles = triangles;
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();

				if (updateMeshCollider) 
				{
					MeshCollider meshCollider = GetComponent<MeshCollider>();

					if (meshCollider) 
					{
						meshCollider.sharedMesh = null;
						meshCollider.sharedMesh = mesh;
					}
				}

				// save update time
				lastMeshUpdateTime = Time.time;
			}
		}
    }

	// estimates which and how many sampled vertices are valid
	private void EstimateSceneVertices(out int count1, out int count3)
	{
		System.Array.Clear(vertexType, 0, vertexType.Length);

		Vector3[] vSpacePos = new Vector3[4];
		int rowIndex = 0;

		for (int y = 0; y < sampledHeight - 1; y++)
		{
			int pixIndex = rowIndex;

			for (int x = 0; x < sampledWidth - 1; x++)
			{
				if(IsSceneSampleValid(x, y, ref vSpacePos[0]) && IsSceneSampleValid(x + 1, y, ref vSpacePos[1]) &&
				   IsSceneSampleValid(x, y + 1, ref vSpacePos[2]) && IsSceneSampleValid(x + 1, y + 1, ref vSpacePos[3]))
				{
					if(IsSpacePointsClose(vSpacePos, 0.01f))
					{
						vertexType[pixIndex] = 3;
						
						vertexType[pixIndex + 1] = 1;
						vertexType[pixIndex + sampledWidth] = 1;
						vertexType[pixIndex + sampledWidth + 1] = 1;
					}
				}

				pixIndex++;
			}

			rowIndex += sampledWidth;
		}

		// estimate counts
		count1 = 0;
		count3 = 0;
		
		for(int i = 0; i < vertexType.Length; i++)
		{
			if(vertexType[i] != 0)
			{
				vertexIndex[i] = count1;
				count1++;
			}
			else
			{
				vertexIndex[i] = 0;
			}

			if(vertexType[i] == 3)
			{
				count3++;
			}
		}
	}

	// checks if the space points are closer to each other than the minimum squared distance
	private bool IsSpacePointsClose(Vector3[] vSpacePos, float fMinDistSquared)
	{
		int iPosLength = vSpacePos.Length;

		for(int i = 0; i < iPosLength; i++)
		{
			for(int j = i + 1; j < iPosLength; j++)
			{
				Vector3 vDist = vSpacePos[j] - vSpacePos[i];
				if(vDist.sqrMagnitude > fMinDistSquared)
					return false;
			}
		}

		return true;
	}

	// checks whether this sample block is valid for the scene
	private bool IsSceneSampleValid(int x, int y, ref Vector3 vSpacePos)
	{
		int pixelIndex = y * sampleSize * depthWidth + x * sampleSize;

		int depth = sensorData.depthImage[pixelIndex];
		vSpacePos = sensorData.depth2SpaceCoords[pixelIndex];

		// check for valid scene or body pixel
		bool isValidScenePixel = !includePlayers ? sensorData.bodyIndexImage[pixelIndex] == 255 : true;

		if(isValidScenePixel && depth >= minDepth && depth <= maxDepth &&
		   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z) &&
		   (maxLeftRight < 0f || (vSpacePos.x >= -maxLeftRight && vSpacePos.x <= maxLeftRight)))
		{
			return true;
		} 

		return false;
	}

}
