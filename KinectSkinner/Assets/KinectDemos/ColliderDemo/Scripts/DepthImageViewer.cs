using UnityEngine;
using System.Collections;

public class DepthImageViewer : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Camera used to estimate the overlay positions of 3D-objects over the background. By default it is the main camera.")]
	public Camera foregroundCamera;
	
	// radius of the created capsule colliders
	private const float colliderRadius = 0.3f;

	// the KinectManager instance
	private KinectManager manager;

	// the foreground texture
	private Texture foregroundTex;
	
	// rectangle taken by the foreground texture (in pixels)
	private Rect foregroundGuiRect;
	private Rect foregroundImgRect;

	// game objects to contain the joint colliders
	private GameObject[] jointColliders = null;
	private int numColliders = 0;

	private int depthImageWidth;
	private int depthImageHeight;
	

	void Start () 
	{
		if (foregroundCamera == null) 
		{
			// by default use the main camera
			foregroundCamera = Camera.main;
		}

		manager = KinectManager.Instance;
		if(manager && manager.IsInitialized())
		{
			KinectInterop.SensorData sensorData = manager.GetSensorData();

			if(sensorData != null && sensorData.sensorInterface != null && foregroundCamera != null)
			{
				// get depth image size
				depthImageWidth = sensorData.depthImageWidth;
				depthImageHeight = sensorData.depthImageHeight;

				// calculate the foreground rectangles
				Rect cameraRect = foregroundCamera.pixelRect;
				float rectHeight = cameraRect.height;
				float rectWidth = cameraRect.width;
				
				if(rectWidth > rectHeight)
					rectWidth = rectHeight * depthImageWidth / depthImageHeight;
				else
					rectHeight = rectWidth * depthImageHeight / depthImageWidth;
				
				float foregroundOfsX = (cameraRect.width - rectWidth) / 2;
				float foregroundOfsY = (cameraRect.height - rectHeight) / 2;
				foregroundImgRect = new Rect(foregroundOfsX, foregroundOfsY, rectWidth, rectHeight);
				foregroundGuiRect = new Rect(foregroundOfsX, cameraRect.height - foregroundOfsY, rectWidth, -rectHeight);
				
				// create joint colliders
				numColliders = sensorData.jointCount;
				jointColliders = new GameObject[numColliders];
				
				for(int i = 0; i < numColliders; i++)
				{
					string sColObjectName = ((KinectInterop.JointType)i).ToString() + "Collider";
					jointColliders[i] = new GameObject(sColObjectName);
					jointColliders[i].transform.parent = transform;
					
					if (i == 0) 
					{
						// sphere collider for body center
						SphereCollider collider = jointColliders[i].AddComponent<SphereCollider>();
						collider.radius = colliderRadius;
					} 
					else 
					{
						// capsule collider for bones
						CapsuleCollider collider = jointColliders[i].AddComponent<CapsuleCollider>();
						collider.radius = colliderRadius;
					}
				}
			}
		}

	}
	
	void Update () 
	{
		// get the users texture
		if(manager && manager.IsInitialized())
		{
			foregroundTex = manager.GetUsersLblTex();
		}

		if(manager && manager.IsUserDetected(playerIndex) && foregroundCamera)
		{
			long userId = manager.GetUserIdByIndex(playerIndex);  // manager.GetPrimaryUserID();

			// update colliders
			for(int i = 0; i < numColliders; i++)
			{
				bool bActive = false;

				if(manager.IsJointTracked(userId, i))
				{
					Vector3 posJoint = manager.GetJointPosDepthOverlay(userId, i, foregroundCamera, foregroundImgRect);

					if (i == 0) 
					{
						// sphere collider for body center
						jointColliders[i].transform.position = posJoint;

						Quaternion rotCollider = manager.GetJointOrientation(userId, i, true);
						jointColliders[i].transform.rotation = rotCollider;

						bActive = true;
					} 
					else 
					{
						int p = (int)manager.GetParentJoint((KinectInterop.JointType)i);

						if (manager.IsJointTracked (userId, p)) 
						{
							// capsule collider for bones
							Vector3 posParent = manager.GetJointPosDepthOverlay(userId, p, foregroundCamera, foregroundImgRect);

							Vector3 posCollider = (posJoint + posParent) / 2f;
							jointColliders[i].transform.position = posCollider;

							Quaternion rotCollider = Quaternion.FromToRotation (Vector3.up, (posJoint - posParent).normalized);
							jointColliders[i].transform.rotation = rotCollider;

							CapsuleCollider collider = jointColliders [i].GetComponent<CapsuleCollider>();
							collider.height = (posJoint - posParent).magnitude;

							bActive = true;
						}
					}
				}

				if (jointColliders[i].activeSelf != bActive) 
				{
					// change collider activity
					jointColliders[i].SetActive(bActive);
				}
			}
		}

	}

	void OnGUI()
	{
		if(foregroundTex)
		{
			GUI.DrawTexture(foregroundGuiRect, foregroundTex);
		}
	}

}
