#if (UNITY_STANDALONE_WIN)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Kinect.Face;


public class FacePointOverlayer : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Tracked face point.")]
	public HighDetailFacePoints facePoint = HighDetailFacePoints.NoseTip;

	[Tooltip("Transform used to show the selected face point in space.")]
	public Transform facePointTransform;
	
	[Tooltip("Camera used to overlay face point transform over the color background.")]
	public Camera foregroundCamera;

	[Tooltip("UI-Text to display face-information messages.")]
	public UnityEngine.UI.Text faceInfoText;

	private KinectManager manager = null;
	private FacetrackingManager faceManager = null;

	private Vector3[] faceVertices;


	void Update () 
	{
		if (!manager) 
		{
			manager = KinectManager.Instance;
		}

		if (!faceManager) 
		{
			faceManager = FacetrackingManager.Instance;
		}

		// get the face points
		if(manager != null && manager.IsInitialized() && faceManager && faceManager.IsFaceTrackingInitialized())
		{
			long userId = manager.GetUserIdByIndex(playerIndex);
			
			if (faceVertices == null) 
			{
				int iVertCount = faceManager.GetUserFaceVertexCount(userId);

				if (iVertCount > 0) 
				{
					faceVertices = new Vector3[iVertCount];
				}
			}

			if (faceVertices != null) 
			{
				if (faceManager.GetUserFaceVertices(userId, ref faceVertices)) 
				{
					if(faceVertices != null && faceVertices[(int)facePoint] != Vector3.zero)
					{
						Vector3 facePointPos = faceVertices [(int)facePoint];
						if (foregroundCamera) 
						{
							facePointPos = GetOverlayPosition(facePointPos);
						}

						if (facePointTransform) 
						{
							facePointTransform.position = facePointPos;
						}

						if(faceInfoText)
						{
							string sStatus = string.Format("{0}: {1}", facePoint, facePointPos);
							faceInfoText.text = sStatus;
						}
					}
				}
			}

		}

	}

	// returns the color-camera overlay position for the given face point
	private Vector3 GetOverlayPosition(Vector3 facePointPos)
	{
		// get the background rectangle (use the portrait background, if available)
		Rect backgroundRect = foregroundCamera.pixelRect;
		PortraitBackground portraitBack = PortraitBackground.Instance;

		if(portraitBack && portraitBack.enabled)
		{
			backgroundRect = portraitBack.GetBackgroundRect();
		}

		Vector3 posColorOverlay = Vector3.zero;
		if(manager && facePointPos != Vector3.zero)
		{
			// 3d position to depth
			Vector2 posDepth = manager.MapSpacePointToDepthCoords(facePointPos);
			ushort depthValue = manager.GetDepthForPixel((int)posDepth.x, (int)posDepth.y);

			if(posDepth != Vector2.zero && depthValue > 0)
			{
				// depth pos to color pos
				Vector2 posColor = manager.MapDepthPointToColorCoords(posDepth, depthValue);

				if(!float.IsInfinity(posColor.x) && !float.IsInfinity(posColor.y))
				{
					float xScaled = (float)posColor.x * backgroundRect.width / manager.GetColorImageWidth();
					float yScaled = (float)posColor.y * backgroundRect.height / manager.GetColorImageHeight();

					float xScreen = backgroundRect.x + xScaled;
					float yScreen = backgroundRect.y + backgroundRect.height - yScaled;

					Plane cameraPlane = new Plane(foregroundCamera.transform.forward, foregroundCamera.transform.position);
					float zDistance = cameraPlane.GetDistanceToPoint(facePointPos);

					posColorOverlay = foregroundCamera.ScreenToWorldPoint(new Vector3(xScreen, yScreen, zDistance));
				}
			}
		}

		return posColorOverlay;
	}


}
#endif
