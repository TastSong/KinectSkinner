using UnityEngine;
using System.Collections;


public class UserBodyBlender : MonoBehaviour 
{
	[Tooltip("Allowed depth distance between the user and the clothing model, in meters.")]
	[Range(-0.5f, 0.5f)]
	public float depthThreshold = 0.1f;

	[Tooltip("RawImage used to display the scene background.")]
	public UnityEngine.UI.RawImage backgroundImage;

	[Tooltip("Camera used to render the 1st scene background. This background camera gets disabled, when this component is enabled.")]
	public Camera backrgoundCamera;

	[Tooltip("Camera used to render the 2nd scene background (users). This background camera gets disabled, when this component is enabled.")]
	public Camera backgroundCamera2;

	private Material userBlendMat;
	private KinectManager kinectManager;
	private BackgroundRemovalManager backManager;
	private long lastDepthFrameTime;

	private Vector2[] color2DepthCoords;
	private ComputeBuffer color2DepthBuffer;

	private float[] depthImageBufferData;
	private ComputeBuffer depthImageBuffer;

	private Rect shaderUvRect = new Rect(0, 0, 1, 1);
	private bool shaderRectInited = false;

	//private float depthFactor = 1f;

	private RenderTexture copyToTex;


	// sets texture to copy to
	/// <summary>
	/// Sets the texture to copy the camera image to.
	/// </summary>
	/// <param name="tex">The target texture</param>
	public void SetCopyToTexture(RenderTexture tex)
	{
		copyToTex = tex;
	}


	void OnEnable()
	{
		// set camera to clear the background
		Camera thisCamera = gameObject.GetComponent<Camera>();
		if(thisCamera) 
		{
			thisCamera.clearFlags = CameraClearFlags.SolidColor;
		}

		// disable the background cameras
		if (backrgoundCamera) 
		{
			backrgoundCamera.gameObject.SetActive(false);
		}

		if (backgroundCamera2) 
		{
			backgroundCamera2.gameObject.SetActive(false);
		}
	}


	void OnDisable()
	{
		// set camera to clear the depth buffser only
		Camera thisCamera = gameObject.GetComponent<Camera>();
		if(thisCamera) 
		{
			thisCamera.clearFlags = CameraClearFlags.Depth;
		}

		// enable the background cameras
		if (backrgoundCamera) 
		{
			backrgoundCamera.gameObject.SetActive(true);
		}

		if (backgroundCamera2) 
		{
			backgroundCamera2.gameObject.SetActive(true);
		}
	}


	void Start () 
	{
		kinectManager = KinectManager.Instance;
		backManager = BackgroundRemovalManager.Instance;

		if(kinectManager && kinectManager.IsInitialized() &&
			kinectManager.GetSensorData().sensorIntPlatform == KinectInterop.DepthSensorPlatform.KinectSDKv2)
		{
			Shader userBlendShader = Shader.Find("Custom/UserBlendShader");
			KinectInterop.SensorData sensorData = kinectManager.GetSensorData();

			if(userBlendShader != null && sensorData != null)
			{
				userBlendMat = new Material(userBlendShader);
				
				userBlendMat.SetFloat("_ColorResX", (float)sensorData.colorImageWidth);
				userBlendMat.SetFloat("_ColorResY", (float)sensorData.colorImageHeight);
				userBlendMat.SetFloat("_DepthResX", (float)sensorData.depthImageWidth);
				userBlendMat.SetFloat("_DepthResY", (float)sensorData.depthImageHeight);

				//depthFactor = 1f + Mathf.Sin (Mathf.Abs (kinectManager.sensorAngle) * Mathf.Deg2Rad);
				//userBlendMat.SetFloat("_DepthFactor", depthFactor);

				color2DepthCoords = new Vector2[sensorData.colorImageWidth * sensorData.colorImageHeight];

				color2DepthBuffer = new ComputeBuffer(sensorData.colorImageWidth * sensorData.colorImageHeight, sizeof(float) * 2);
				userBlendMat.SetBuffer("_DepthCoords", color2DepthBuffer);

				depthImageBufferData = new float[sensorData.depthImage.Length];

				depthImageBuffer = new ComputeBuffer(sensorData.depthImage.Length, sizeof(float));
				userBlendMat.SetBuffer("_DepthBuffer", depthImageBuffer);

				if (backgroundImage) 
				{
					userBlendMat.SetTexture("_BackTex", backgroundImage.texture);
				}

				// color camera texture
				//userBlendMat.SetTexture("_ColorTex", sensorData.colorImageTexture);
//				Texture colorTex = backManager && sensorData.color2DepthTexture ? (Texture)sensorData.color2DepthTexture : (Texture)sensorData.colorImageTexture;
//				userBlendMat.SetTexture("_ColorTex", colorTex);
			}
		}
		else
		{
			// disable the component
			gameObject.GetComponent<UserBodyBlender>().enabled = false;
		}
	}

	void OnDestroy()
	{
		if(color2DepthBuffer != null)
		{
			color2DepthBuffer.Release();
			color2DepthBuffer = null;
		}
		
		if(depthImageBuffer != null)
		{
			depthImageBuffer.Release();
			depthImageBuffer = null;
		}

		color2DepthCoords = null;
		depthImageBufferData = null;
	}

	void Update()
	{
		if (!shaderRectInited) 
		{
			PortraitBackground portraitBack = PortraitBackground.Instance;
			if(portraitBack && portraitBack.IsInitialized())
			{
				shaderUvRect = portraitBack.GetShaderUvRect();
			}

			if (userBlendMat != null) 
			{
				userBlendMat.SetFloat("_ColorOfsX", shaderUvRect.x);
				userBlendMat.SetFloat("_ColorMulX", shaderUvRect.width);
				userBlendMat.SetFloat("_ColorOfsY", shaderUvRect.y);
				userBlendMat.SetFloat("_ColorMulY", shaderUvRect.height);
			}

			shaderRectInited = true;
		}

		if(kinectManager && kinectManager.IsInitialized())
		{
			KinectInterop.SensorData sensorData = kinectManager.GetSensorData();
			
			if(sensorData != null && sensorData.depthImage != null && 
				(sensorData.colorImageTexture2D || sensorData.colorImageTexture) &&
				userBlendMat != null && lastDepthFrameTime != sensorData.lastDepthFrameTime)
			{
				lastDepthFrameTime = sensorData.lastDepthFrameTime;
				//userBlendMat.SetTexture("_ColorTex", sensorData.colorImageTexture);

				//if (kinectManager.autoHeightAngle == KinectManager.AutoHeightAngle.AutoUpdate || kinectManager.autoHeightAngle == KinectManager.AutoHeightAngle.AutoUpdateAndShowInfo) 
				//{
				//	depthFactor = 1f + Mathf.Sin (Mathf.Abs (kinectManager.sensorAngle) * Mathf.Deg2Rad);
				//	userBlendMat.SetFloat("_DepthFactor", depthFactor);
				//}

				if(KinectInterop.MapColorFrameToDepthCoords(sensorData, ref color2DepthCoords))
				{
					color2DepthBuffer.SetData(color2DepthCoords);
				}

				// buffer for depths
				for (int i = 0; i < sensorData.depthImage.Length; i++)
				{
					ushort depth = sensorData.depthImage[i];
					depthImageBufferData[i] = (float)depth;
				}
				
				depthImageBuffer.SetData(depthImageBufferData);

				// color camera texture
				Texture colorTex = backManager && sensorData.color2DepthTexture ? (Texture)sensorData.color2DepthTexture : 
					(Texture)(sensorData.colorImageTexture2D ? sensorData.colorImageTexture2D : sensorData.colorImageTexture);
				userBlendMat.SetTexture("_ColorTex", colorTex);
			}
		}
	}
	
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		if(userBlendMat != null)
		{
			userBlendMat.SetFloat("_Threshold", depthThreshold);
			Graphics.Blit(source, destination, userBlendMat);

			if (copyToTex != null) 
			{
				Graphics.Blit(destination, copyToTex);
			}
		}
	}
}
