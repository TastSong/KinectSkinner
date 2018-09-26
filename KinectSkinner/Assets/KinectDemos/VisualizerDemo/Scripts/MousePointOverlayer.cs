using UnityEngine;
using System.Collections;

public class MousePointOverlayer : MonoBehaviour 
{
	[Tooltip("Whether to wait for mouse click or not.")]
	public bool waitForClick = true;

	[Tooltip("Game object used to overlay the mouse pointed position.")]
	public Transform overlayObject;

	[Tooltip("Camera used to convert the screen point to world point.")]
	public Camera foregroundCamera;

	[Tooltip("UI-Text used to display information messages.")]
	public UnityEngine.UI.Text infoText;


	// reference to the singleton instance of KM
	private KinectManager kinectManager;

	// sensor color-image width & height
	private int colorWidth = 0;
	private int colorHeight = 0;

	// last mouse position
	private Vector2 lastMousePos;
	// screen rectangle
	private Rect backgroundRect;


	void Start () 
	{
		// by default set the main camera as foreground-camera
		if (foregroundCamera == null) 
		{
			foregroundCamera = Camera.main;
		}

		// get the singleton instance of KM
		kinectManager = KinectManager.Instance;

		// get color-image resolution
		if (kinectManager && kinectManager.IsInitialized()) 
		{
			colorWidth = kinectManager.GetColorImageWidth();
			colorHeight = kinectManager.GetColorImageHeight();
		}

		// estimate the background rectangle
		backgroundRect = foregroundCamera ? foregroundCamera.pixelRect : new Rect(0, 0, Screen.width, Screen.height);
	}
	
	void Update () 
	{
		// get mouse button state and position
		bool bMouseClicked = waitForClick ? Input.GetMouseButtonDown(0) : true;
		Vector2 mousePos = Input.mousePosition;

		if (kinectManager && kinectManager.IsInitialized() && overlayObject &&
			bMouseClicked && mousePos.x >= 0 && mousePos.y >= 0 && mousePos.x < Screen.width && mousePos.y < Screen.height
			&& mousePos != lastMousePos) 
		{
			lastMousePos = mousePos;

			// screen position
			Vector2 screenPos = mousePos;

			// update the background rectangle with the portrait background, if available
			PortraitBackground portraitBack = PortraitBackground.Instance;
			if(portraitBack && portraitBack.enabled)
			{
				backgroundRect = portraitBack.GetBackgroundRect();
			}

			// convert to color image rectangle
			float colorX = (screenPos.x - backgroundRect.x) * (float)colorWidth / backgroundRect.width;
			float colorY = (backgroundRect.y + (backgroundRect.height - screenPos.y)) * (float)colorHeight / backgroundRect.height;
			Vector2 colorPos = new Vector2(colorX, colorY);

			// get the respective depth image pos
			Vector2 depthPos = kinectManager.MapColorPointToDepthCoords(colorPos, true);

			if (depthPos != Vector2.zero) 
			{
				// get the depth in mm
				ushort depthValue = kinectManager.GetDepthForPixel((int)depthPos.x, (int)depthPos.y);

				// get the space position in world coordinates
				Vector3 worldPos = foregroundCamera ? foregroundCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, (float)depthValue / 1000f)) :
					kinectManager.MapDepthPointToSpaceCoords(depthPos, depthValue, true);

				// set the overlay object's position
				if (!float.IsNaN(worldPos.x) && !float.IsNaN(worldPos.y) && !float.IsNaN(worldPos.z)) 
				{
					overlayObject.position = worldPos;

					if (infoText) 
					{
						infoText.text = string.Format("Pos: ({0:F2}, {1:F2}, {2:F2})", worldPos.x, worldPos.y, worldPos.z);
					}
				}
			}

		}
	}

}
