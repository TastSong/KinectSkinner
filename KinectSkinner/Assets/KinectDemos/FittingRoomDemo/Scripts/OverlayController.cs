using UnityEngine;
using System.Collections;

public class OverlayController : MonoBehaviour 
{
//	[Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
//	public GUITexture backgroundImage;

	[Tooltip("Camera used to display the 1st scene background.")]
	public Camera backgroundCamera;

	[Tooltip("Camera used to display the 2nd scene background (users).")]
	public Camera backgroundCamera2;

	[Tooltip("Camera used to display the clothing models from the Kinect point of view.")]
	public Camera foregroundCamera;

//	[Tooltip("Use this setting to minimize the offset between the image and the model overlay.")]
//	[Range(-0.1f, 0.1f)]
//	public float adjustedCameraOffset = 0f;
//
//
//	// variable to track the current camera offset
//	private float currentCameraOffset = 0f;


	void Start () 
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			KinectInterop.SensorData sensorData = manager.GetSensorData();

			if(foregroundCamera != null && sensorData != null && sensorData.sensorInterface != null)
			{
//				foregroundCamera.transform.position = new Vector3(sensorData.depthCameraOffset + adjustedCameraOffset, 
//				                                                  manager.sensorHeight, 0f);
				foregroundCamera.transform.position = new Vector3(0f, manager.sensorHeight, 0f);
				foregroundCamera.transform.rotation = Quaternion.Euler(-manager.sensorAngle, 0f, 0f);
//				currentCameraOffset = adjustedCameraOffset;

//				foregroundCamera.fieldOfView = sensorData.colorCameraFOV;
			}

			if(backgroundCamera != null && sensorData != null && sensorData.sensorInterface != null)
			{
				backgroundCamera.transform.position = new Vector3(0f, manager.sensorHeight, 0f);
				backgroundCamera.transform.rotation = Quaternion.Euler(-manager.sensorAngle, 0f, 0f);
			}

			if(backgroundCamera2 != null && sensorData != null && sensorData.sensorInterface != null)
			{
				backgroundCamera2.transform.position = new Vector3(0f, manager.sensorHeight, 0f);
				backgroundCamera2.transform.rotation = Quaternion.Euler(-manager.sensorAngle, 0f, 0f);
			}
		}
	}

	void Update () 
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			KinectInterop.SensorData sensorData = manager.GetSensorData();
			
			if(manager.autoHeightAngle == KinectManager.AutoHeightAngle.AutoUpdate || 
				manager.autoHeightAngle == KinectManager.AutoHeightAngle.AutoUpdateAndShowInfo) // ||
			   //currentCameraOffset != adjustedCameraOffset)
			{
				// update the cameras automatically, according to the current sensor height and angle
				if(foregroundCamera != null && sensorData != null)
				{
//					foregroundCamera.transform.position = new Vector3(sensorData.depthCameraOffset + adjustedCameraOffset, 
//					                                                  manager.sensorHeight, 0f);
					foregroundCamera.transform.position = new Vector3(0f, manager.sensorHeight, 0f);
					foregroundCamera.transform.rotation = Quaternion.Euler(-manager.sensorAngle, 0f, 0f);
//					currentCameraOffset = adjustedCameraOffset;
				}
				
				if(backgroundCamera != null && sensorData != null)
				{
					backgroundCamera.transform.position = new Vector3(0f, manager.sensorHeight, 0f);
					backgroundCamera.transform.rotation = Quaternion.Euler(-manager.sensorAngle, 0f, 0f);
				}
				
				if(backgroundCamera2 != null && sensorData != null)
				{
					backgroundCamera2.transform.position = new Vector3(0f, manager.sensorHeight, 0f);
					backgroundCamera2.transform.rotation = Quaternion.Euler(-manager.sensorAngle, 0f, 0f);
				}
			}
			
//			if(backgroundImage)
//			{
//				if(backgroundImage.texture == null)
//				{
//					backgroundImage.texture = manager.GetUsersClrTex();
//					//backgroundImage.texture = BackgroundRemovalManager.Instance.GetForegroundTex();
//				}
//			}
		}

	}

}
