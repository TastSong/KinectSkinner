using UnityEngine;
using System.Collections;

public class ForegroundToImage : MonoBehaviour 
{
	private GUITexture guiTex;

	void Start()
	{
		guiTex = GetComponent<GUITexture>();
	}

	void Update () 
	{
		if (guiTex && guiTex.texture == null) 
		{
			BackgroundRemovalManager backManager = BackgroundRemovalManager.Instance;
			KinectManager kinectManager = KinectManager.Instance;

			if (kinectManager && backManager && backManager.enabled /**&& backManager.IsBackgroundRemovalInitialized()*/) 
			{
				guiTex.texture = backManager.GetForegroundTex();  // user's foreground texture
				guiTex.transform.localScale = kinectManager.GetColorImageScale();
			} 
			else if(kinectManager /**&& kinectManager.IsInitialized()*/)
			{
				guiTex.texture = kinectManager.GetUsersClrTex();  // color camera texture
				guiTex.transform.localScale = kinectManager.GetColorImageScale();
			}
		}
//		else if(guiTex && guiTex != null)
//		{
//			KinectManager kinectManager = KinectManager.Instance;
//			if(kinectManager == null)
//			{
//				guiTex.texture = null;
//			}
//		}
	}


	void OnApplicationPause(bool isPaused)
	{
		// fix for app pause & restore (UWP)
		if(isPaused && guiTex && guiTex.texture != null)
		{
			guiTex.texture = null;
		}
	}

}
