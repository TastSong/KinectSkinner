using UnityEngine;
using System.Collections;

public class ForegroundToRenderer : MonoBehaviour 
{
	private Renderer thisRenderer;

	void Start()
	{
		thisRenderer = GetComponent<Renderer>();

		KinectManager kinectManager = KinectManager.Instance;
		if (kinectManager && kinectManager.IsInitialized ()) 
		{
			Vector3 localScale = transform.localScale;
			localScale.z = localScale.x * kinectManager.GetColorImageHeight () / kinectManager.GetColorImageWidth ();
			localScale.x = -localScale.x;

			transform.localScale = localScale;
		}
	}


	void Update () 
	{
		if(thisRenderer && thisRenderer.sharedMaterial.mainTexture == null)
		{
			KinectManager kinectManager = KinectManager.Instance;
			BackgroundRemovalManager backManager = BackgroundRemovalManager.Instance;

			if(kinectManager && backManager && backManager.enabled /**&& backManager.IsBackgroundRemovalInitialized()*/)
			{
				thisRenderer.sharedMaterial.mainTexture = backManager.GetForegroundTex();
			}
		}
//		else if(thisRenderer && thisRenderer.sharedMaterial.mainTexture != null)
//		{
//			KinectManager kinectManager = KinectManager.Instance;
//			if(kinectManager == null)
//			{
//				thisRenderer.sharedMaterial.mainTexture = null;
//			}
//		}
	}


	void OnApplicationPause(bool isPaused)
	{
		// fix for app pause & restore (UWP)
		if(isPaused && thisRenderer && thisRenderer.sharedMaterial.mainTexture != null)
		{
			thisRenderer.sharedMaterial.mainTexture = null;
		}
	}

}
