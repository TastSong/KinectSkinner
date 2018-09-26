using UnityEngine;
using System.Collections;
using RoomAliveToolkit;

public class ProjectorCamera : MonoBehaviour 
{
	[Tooltip("XML file in Resources, containing calibration data.")]
	public TextAsset calibrationXml = null;

	[Tooltip("Projector name in the RoomAlive calibration file.")]
	public string projNameInConfig = "0";

	[Tooltip("Whether to set the camera's target display to be the same, as in the RoomAlive calibration file.")]
	public bool useProjectorDisplay = false;

//	[Tooltip("Whether to flip the camera image horizontally, or not.")]
//	public bool flipCameraX = false;


	private int imageWidth = 1280;
	private int imageHeight = 800;
	//public Vector4 lensDist;

	private int displayIndex = -1;

	private ProjectorCameraEnsemble ensemble;
	private ProjectorCameraEnsemble.Projector projConfig;

	private Camera cam;
	private Material camFlipMat;


	void Start () 
	{
		LoadCalibrationData();

//		Shader camFlipShader = Shader.Find("Custom/CameraFlipShader");
//		if(camFlipShader)
//		{
//			camFlipMat = new Material(camFlipShader);
//		}

	}

	void Update()
	{
		KinectManager kinectManager = KinectManager.Instance;

		if(cam != null && projConfig != null && kinectManager != null && kinectManager.IsInitialized())
		{
			if(kinectManager.autoHeightAngle == KinectManager.AutoHeightAngle.AutoUpdate || 
				kinectManager.autoHeightAngle == KinectManager.AutoHeightAngle.AutoUpdateAndShowInfo)
			{
				Matrix4x4 worldToLocal = RAT2Unity.Convert(projConfig.pose);
				worldToLocal = UnityUtilities.ConvertRHtoLH(worldToLocal);

				this.transform.localPosition = worldToLocal.ExtractTranslation() + new Vector3(0f, kinectManager.sensorHeight, 0f);
			}
		}
	}

//	void OnRenderImage (RenderTexture source, RenderTexture destination)
//	{
//		if(flipCameraX && camFlipMat)
//		{
//			Graphics.Blit(source, destination, camFlipMat);
//		}
//	}

	private void LoadCalibrationData()
	{
		projConfig = null;
		cam = GetComponent<Camera>();

		if(!cam)
		{
			Debug.LogError("Please add the ProjectorCamera-component to a camera in the scene.");
			return;
		}

		if (calibrationXml)
		{
			ensemble = ProjectorCameraEnsemble.ReadCalibration(calibrationXml.text);

			foreach (ProjectorCameraEnsemble.Projector pc in ensemble.projectors)
			{
				if (pc.name == projNameInConfig)
				{
					projConfig = pc;
				}
			}
		}
		else
		{
			projConfig = null;
		}

		if (projConfig != null)
		{
			if (displayIndex < 0)
			{
				displayIndex = projConfig.displayIndex;
				Debug.Log("ProjCam target display: " + displayIndex);

				if(useProjectorDisplay && displayIndex >= 0)
				{
//#if !UNITY_EDITOR					
					cam.targetDisplay = displayIndex;
//#endif
				}
			}
			
			//Debug.Log("Projective Rendering - Loading projector calibration information.");
			imageWidth = projConfig.width;
			imageHeight = projConfig.height;

			if(imageWidth > 0 && imageHeight > 0 && projConfig.cameraMatrix != null)
			{
				cam.aspect = (float)imageWidth / imageHeight;

				// this is the vertical field of view - fy
				float fieldOfViewRad = 2.0f * (float)System.Math.Atan((((double)(imageHeight)) / 2.0) / projConfig.cameraMatrix[1, 1]);
				float fieldOfViewDeg = fieldOfViewRad / 3.14159265359f * 180.0f;

				cam.fieldOfView = fieldOfViewDeg;
				Debug.Log("ProjCam FOV: " + fieldOfViewDeg);

				Matrix4x4 opencvProjMat = GetProjectionMatrix(projConfig.cameraMatrix, cam.nearClipPlane, cam.farClipPlane);
				cam.projectionMatrix = UnityUtilities.ConvertRHtoLH(opencvProjMat);
			}

			if(projConfig.lensDistortion != null)
			{
				//var irCoef = projConfig.lensDistortion.AsFloatArray();
				//! jolaur -- looks like this is not being used and is now 2 elements instead of four in the new xml format
				//! lensDist = new Vector4(irCoef[0], irCoef[1], irCoef[2], irCoef[3]); 
				//lensDist = new Vector4();
			}

			if(projConfig.pose != null)
			{
				Matrix4x4 worldToLocal = RAT2Unity.Convert(projConfig.pose);
				worldToLocal = UnityUtilities.ConvertRHtoLH(worldToLocal);

				KinectManager kinectManager = KinectManager.Instance;

				this.transform.localPosition = worldToLocal.ExtractTranslation() + new Vector3(0f, kinectManager.sensorHeight, 0f);
				this.transform.localRotation = worldToLocal.ExtractRotation();
				Debug.Log("ProjCam position: " + transform.localPosition + ", rotation: " + transform.localRotation.eulerAngles);
			}
		}
		else
		{
			Debug.LogError("Make sure the 'Calibration Xml' && 'Proj name in config'-settings are correct.");
			//lensDist = new Vector4();
		}
	}


	private Matrix4x4 GetProjectionMatrix(RoomAliveToolkit.Matrix intrinsics, float zNear, float zFar)
	{
		float c_x = (float)intrinsics[0, 2];
		float c_y = (float)intrinsics[1, 2];

		//the intrinsics are in Kinect coordinates: X - left, Y - up, Z, forward
		//we need the coordinates to be: X - right, Y - down, Z - forward
		c_x = imageWidth - c_x;
		c_y = imageHeight - c_y;

		// http://spottrlabs.blogspot.com/2012/07/opencv-and-opengl-not-always-friends.html
		// http://opencv.willowgarage.com/wiki/Posit
		Matrix4x4 projMat = new Matrix4x4();
		projMat[0, 0] = (float)(2.0 * intrinsics[0, 0] / imageWidth);
		projMat[1, 1] = (float)(2.0 * intrinsics[1, 1] / imageHeight);
		projMat[2, 0] = (float)(-1.0f + 2 * c_x / imageWidth);
		projMat[2, 1] = (float)(-1.0f + 2 * c_y / imageHeight);

		// Note this changed from previous code
		// see here: http://www.songho.ca/opengl/gl_projectionmatrix.html
		projMat[2, 2] = -(zFar + zNear) / (zFar - zNear);
		projMat[3, 2] = -2.0f * zNear * zFar / (zFar - zNear);
		projMat[2, 3] = -1;

		// Transpose tp fit Unity's column major matrix (in contrast to vision raw major ones).
		projMat = projMat.transpose;

		return projMat;
	}

}
