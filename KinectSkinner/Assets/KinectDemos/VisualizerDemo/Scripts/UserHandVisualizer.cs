using UnityEngine;
using System.Collections;


public class UserHandVisualizer : MonoBehaviour
{
	[Tooltip("Index of the player, tracked by this component. 0 - the 1st player, 1 - the 2nd player, 2 - the 3rd player, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Whether the mesh is facing the player or not.")]
	public bool mirroredMovement = true;
	
	[Tooltip("Kinect position in the world, used as origin for user movements.")]
	public Vector3 originPosition = Vector3.zero;
	
	[Tooltip("Whether the z-movement should be inverted or not.")]
	public bool invertedZMovement = false;
	
	[Tooltip("Smooth factor used for user movements.")]
	public float smoothFactor = 0f;
	
	[Tooltip("Camera used to overlay the mesh over the color background.")]
	public Camera foregroundCamera;

	[Tooltip("Whether to update the mesh collider as well, when the user mesh changes.")]
	public bool updateMeshCollider = false;

	[Tooltip("Number of pixels per direction in a sample.")]
	private const int sampleSize = 1;


    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

	private KinectManager manager = null;

	private KinectInterop.SensorData sensorData = null;
	//private Vector3[] spaceCoords = null;
	private long lastSpaceCoordsTime = 0;
	private Matrix4x4 kinectToWorld = Matrix4x4.identity;

	private int depthWidth = 0;
	private int depthHeight = 0;

	private int sampledWidth = 0;
	private int sampledHeight = 0;

	private long userId = 0;
	private byte userBodyIndex = 255;
	private Vector3 userMeshPos = Vector3.zero;

	private Vector3 leftHandPos = Vector3.zero;
	private Vector3 rightHandPos = Vector3.zero;
	private Vector3 leftFingerPos = Vector3.zero;
	private Vector3 rightFingerPos = Vector3.zero;
	private Vector3 leftThumbPos = Vector3.zero;
	private Vector3 rightThumbPos = Vector3.zero;

	private byte[] vertexType;
	private int[] vertexIndex;


    void Start()
    {
		manager = KinectManager.Instance;

		if (manager != null)
        {
			sensorData = manager.GetSensorData();

			depthWidth = manager.GetDepthImageWidth();
			depthHeight = manager.GetDepthImageHeight();
			
			sampledWidth = depthWidth / sampleSize;
			sampledHeight = depthHeight / sampleSize;

			//spaceCoords = new Vector3[depthWidth * depthHeight];

			if(sensorData.depth2SpaceCoords == null)
			{
				sensorData.depth2SpaceCoords = new Vector3[depthWidth * depthHeight];
			}

			vertexType = new byte[sampledWidth * sampledHeight];
			vertexIndex = new int[sampledWidth * sampledHeight];

			CreateMesh(sampledWidth, sampledHeight);
        }
    }

    private void CreateMesh(int width, int height)
    {
        mesh = new Mesh();
		mesh.name = "UserMesh";

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
			BackgroundRemovalManager backManager = BackgroundRemovalManager.Instance;
			renderer.material.mainTexture = backManager ? (Texture)sensorData.depth2ColorTexture : (Texture)manager.GetUsersLblTex();
		}

		// get kinect-to-world matrix
		kinectToWorld = manager.GetKinectToWorldMatrix();

		if(playerIndex >= 0)
		{
			long lastUserId = userId;
			userId = manager.GetUserIdByIndex(playerIndex);

			userBodyIndex = (byte)manager.GetBodyIndexByUserId(userId);
			if(userBodyIndex == 255)
				userBodyIndex = 222;

			// hand positions
			leftHandPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.HandLeft);
			rightHandPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.HandRight);
			leftFingerPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.HandTipLeft);
			rightFingerPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.HandTipRight);
			leftThumbPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.ThumbLeft);
			rightThumbPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.ThumbRight);

			// check for color overlay
			if (foregroundCamera) 
			{
				// get the background rectangle (use the portrait background, if available)
				Rect backgroundRect = foregroundCamera.pixelRect;
				PortraitBackground portraitBack = PortraitBackground.Instance;

				if (portraitBack && portraitBack.enabled) 
				{
					backgroundRect = portraitBack.GetBackgroundRect ();
				}

				// get user position
				userMeshPos = manager.GetJointPosColorOverlay(userId, (int)KinectInterop.JointType.SpineBase, foregroundCamera, backgroundRect);
			}
			else
			{
				// get user position
				userMeshPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.SpineBase);
			}

			if(!mirroredMovement)
			{
				//userMeshPos.x = -userMeshPos.x;
				userMeshPos.x = 0f;
			}

			if (foregroundCamera == null) 
			{
				// convert kinect pos to world coords, when there is no color overlay
				userMeshPos = kinectToWorld.MultiplyPoint3x4(userMeshPos);
			}

			// set transform position
			Vector3 newUserPos = userMeshPos + originPosition; // manager.GetJointPosition(userId, (int)KinectInterop.JointType.SpineBase) + originPosition;
			
			if(invertedZMovement)
			{
				newUserPos.z = -newUserPos.z;
			}
			
			transform.position = lastUserId != 0 && smoothFactor != 0f ? Vector3.Lerp(transform.position, newUserPos, smoothFactor * Time.deltaTime) : newUserPos;
		}
		else
		{
			userId = 0;
			userBodyIndex = 255;
			userMeshPos = Vector3.zero;

			leftHandPos = rightHandPos = Vector3.zero;
			leftFingerPos = rightFingerPos = Vector3.zero;
			leftThumbPos = rightThumbPos = Vector3.zero;
		}

		// update the mesh
		UpdateMesh();
    }
    
    private void UpdateMesh()
    {
		if(sensorData.depthImage != null && sensorData.bodyIndexImage != null &&
			sensorData.depth2SpaceCoords != null && lastSpaceCoordsTime != sensorData.lastDepth2SpaceCoordsTime)
		{
			int vCount = 0, tCount = 0;
			EstimateUserVertices(out vCount, out tCount);

			vertices = new Vector3[vCount];
			uvs = new Vector2[vCount];
			triangles = new int[6 * tCount];

			int index = 0, vIndex = 0, tIndex = 0, xyIndex = 0;
			for (int y = 0; y < depthHeight; y += sampleSize)
			{
				int xyStartIndex = xyIndex;

				for (int x = 0; x < depthWidth; x += sampleSize)
				{
					//Vector3 vSpacePos = spaceCoords[xyIndex];
					Vector3 vSpacePos = sensorData.depth2SpaceCoords[xyIndex];

					if(vertexType[index] != 0 &&
					   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
					{
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

							Vector2 vColorPos = sensorData.depth2ColorCoords[xyIndex];
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

						if(!mirroredMovement)
						{
							vSpacePos.x = -vSpacePos.x;
						}

						if(foregroundCamera == null) 
						{
							// convert space to world coords, when there is no color overlay
							vSpacePos = kinectToWorld.MultiplyPoint3x4(vSpacePos);
						}

						vertices[vIndex] = vSpacePos - userMeshPos;
						uvs[vIndex] = new Vector2((float)x / depthWidth, (float)y / depthHeight);
						vIndex++;

						if(vertexType[index] == 3)
						{
							if(mirroredMovement)
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

//			lock(sensorData.spaceCoordsBufferLock)
//			{
//				sensorData.spaceCoordsBufferReady = false;
//			}

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
		}
    }

	// estimates which and how many sampled vertices are valid
	private void EstimateUserVertices(out int count1, out int count3)
	{
		System.Array.Clear(vertexType, 0, vertexType.Length);

		Vector3[] vSpacePos = new Vector3[4];
		int rowIndex = 0;

		for (int y = 0; y < sampledHeight - 1; y++)
		{
			int pixIndex = rowIndex;

			for (int x = 0; x < sampledWidth - 1; x++)
			{
				if(IsUserSampleValid(x, y, ref vSpacePos[0]) && IsUserSampleValid(x + 1, y, ref vSpacePos[1]) &&
				   IsUserSampleValid(x, y + 1, ref vSpacePos[2]) && IsUserSampleValid(x + 1, y + 1, ref vSpacePos[3]))
				{
					if(IsSpacePointsCloseToEachOther(vSpacePos, 0.01f) && IsSpacePointsCloseToHands(vSpacePos, 0.01f))
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
	private bool IsSpacePointsCloseToEachOther(Vector3[] vSpacePos, float fMinDistSquared)
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

	// checks if the space points are closer to hand, finger or thumb joints than the minimum squared distance
	private bool IsSpacePointsCloseToHands(Vector3[] vSpacePos, float fMinDistSquared)
	{
		if(IsSpacePointsCloseToJoint(vSpacePos, leftHandPos, fMinDistSquared) || IsSpacePointsCloseToJoint(vSpacePos, rightHandPos, fMinDistSquared) ||
			IsSpacePointsCloseToJoint(vSpacePos, leftFingerPos, fMinDistSquared) || IsSpacePointsCloseToJoint(vSpacePos, rightFingerPos, fMinDistSquared) ||
			IsSpacePointsCloseToJoint(vSpacePos, leftThumbPos, fMinDistSquared) || IsSpacePointsCloseToJoint(vSpacePos, rightThumbPos, fMinDistSquared)) 
		{
			return true;
		}

		return false;
	}

	// checks if the space points are closer to joint position than the minimum squared distance
	private bool IsSpacePointsCloseToJoint(Vector3[] vSpacePos, Vector3 vJointPos, float fMinDistSquared)
	{
		int iPosLength = vSpacePos.Length;

		for(int i = 0; i < iPosLength; i++)
		{
			Vector3 vDist = vSpacePos[i] - vJointPos;
			if(vDist.sqrMagnitude > fMinDistSquared)
				return false;
		}

		return true;
	}

	// checks whether this sample block is valid for this user
	private bool IsUserSampleValid(int x, int y, ref Vector3 vSpacePos)
	{
		int startIndex = y * sampleSize * depthWidth + x * sampleSize;

		//for (int y1 = 0; y1 < SampleSize; y1++)
		{
			int pixelIndex = startIndex;
			//vSpacePos = spaceCoords[pixelIndex];
			vSpacePos = sensorData.depth2SpaceCoords[pixelIndex];

			//for (int x1 = 0; x1 < SampleSize; x1++)
			{
				if(userBodyIndex != 255)
				{
					if(sensorData.bodyIndexImage[pixelIndex] == userBodyIndex &&
					   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
					{
						return true;
					}
				}
//				else
//				{
//					if(sensorData.bodyIndexImage[pixelIndex] != 255 &&
//					   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
//					{
//						return true;
//					}
//				}

				pixelIndex++;
			}

			startIndex += depthWidth;
		}
		
		return false;
	}

}
