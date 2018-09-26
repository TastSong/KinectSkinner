using UnityEngine;
using System.Collections;
using System.Text;

public class WebcamSource : MonoBehaviour 
{
	[Tooltip("Selected web-camera name. If left empty, the first available web camera will be selected.")]
	public string webcamName;

	[Tooltip("Whether the webcam image needs to be flipped horizontally.")]
	public bool flipHorizontally = true;


	// the web-camera texture
	private WebCamTexture webcamTex;
	private bool bTexAspectSet = false;

	void Start () 
	{
		if(string.IsNullOrEmpty(webcamName))
		{
			// get available webcams
			WebCamDevice[] devices = WebCamTexture.devices;
			
			if(devices != null && devices.Length > 0)
			{
				// print available webcams
				StringBuilder sbWebcams = new StringBuilder();
				sbWebcams.Append("Available webcams:").AppendLine();

				foreach(WebCamDevice device in devices)
				{
					sbWebcams.Append(device.name).AppendLine();
				}

				Debug.Log(sbWebcams.ToString());

				// get the 1st webcam name
				webcamName = devices[0].name;
			}
		}

		// create the texture
		if(!string.IsNullOrEmpty(webcamName))
		{
			webcamTex = new WebCamTexture(webcamName.Trim());
		}
		
		// set the texture
		Renderer renderer = GetComponent<Renderer>();
		if(renderer)
		{
			renderer.material.mainTexture = webcamTex;
		}
		
		if(webcamTex && !string.IsNullOrEmpty(webcamTex.deviceName))
		{
			webcamTex.Play();
		}
	}

	void Update()
	{
		if(!bTexAspectSet)
		{
			Vector3 localScale = transform.localScale;
			localScale.x = (float)webcamTex.width * localScale.y / (float)webcamTex.height;
			localScale.x = (!flipHorizontally ? localScale.x : -localScale.x);
			transform.localScale = localScale;

			bTexAspectSet = true;
		}
	}
	
}
