using UnityEngine;
using System.Collections;

public class SetFaceTexture : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Whether to use the face-rectangle provided by the FacetrackingManager, or not.")]
	public bool useTrackedFaceRect = false;

	[Tooltip("Width of the rectangle around the head joint, in meters.")]
	public float faceWidth = 0.3f;
	[Tooltip("Height of the rectangle around the head joint, in meters.")]
	public float faceHeight = 0.3f;

	[Tooltip("Game object renderer, used to display the face image as 2D texture.")]
	public Renderer targetObject;


	//private Renderer targetRenderer;
	private Rect faceRect;
	private Texture2D colorTex, faceTex;
	private KinectManager kinectManager;
	private FacetrackingManager faceManager;

	private BackgroundRemovalManager backManager;
	private RenderTexture foregroundTex = null;

	//private const int pixelAlignment = -2;  // must be negative power of 2

	/// <summary>
	/// Determines whether the face rectange and face texture are valid.
	/// </summary>
	/// <returns><c>true</c> if the face rectangle is valid; otherwise, <c>false</c>.</returns>
	public bool IsFaceRectValid()
	{
		return faceRect.width > 0 && faceRect.height > 0;
	}


	/// <summary>
	/// Gets the face rectangle, in pixels.
	/// </summary>
	/// <returns>The face rectangle.</returns>
	public Rect GetFaceRect()
	{
		return faceRect;
	}

	/// <summary>
	/// Gets the tracked face texture.
	/// </summary>
	/// <returns>The face texture.</returns>
	public Texture GetFaceTex()
	{
		return faceTex;
	}


	void Start () 
	{
		kinectManager = KinectManager.Instance;
		faceTex = new Texture2D(100, 100, TextureFormat.ARGB32, false);

		if (!targetObject) 
		{
			targetObject = GetComponent<Renderer>();
		}

		if(targetObject && targetObject.material)
		{
			targetObject.material.SetTextureScale("_MainTex", new Vector2(1, -1));
		}
	}
	
	void Update () 
	{
		if(faceManager == null)
		{
			faceManager = FacetrackingManager.Instance;
		}

		if(!kinectManager || !kinectManager.IsInitialized())
			return;
		if(!faceManager || !faceManager.IsFaceTrackingInitialized())
			return;

		long userId = kinectManager.GetUserIdByIndex(playerIndex);
		if (userId == 0) 
		{
			if(targetObject && targetObject.material && targetObject.material.mainTexture != null)
			{
				targetObject.material.mainTexture = null;
			}

			return;
		}

		if (!backManager) 
		{
			backManager = BackgroundRemovalManager.Instance;

			if (backManager) 
			{
				// re-initialize the texture
				colorTex = null;
			}
		}

		if (!foregroundTex && backManager && backManager.IsBackgroundRemovalInitialized()) 
		{
			// use foreground image
			foregroundTex = (RenderTexture)backManager.GetForegroundTex();

//			if (!colorTex) 
//			{
//				colorTex = new Texture2D(kinectManager.GetColorImageWidth(), kinectManager.GetColorImageHeight(), TextureFormat.ARGB32, false);
//			}
//
//			try 
//			{
//				foregroundTex = (RenderTexture)backManager.GetForegroundTex ();
//				//KinectInterop.RenderTex2Tex2D (foregroundTex, ref colorTex);
//			} 
//			catch (System.Exception) 
//			{
//				colorTex = (Texture2D)backManager.GetForegroundTex ();
//			}
		}
		else 
		{
			// use color camera image
			if (!colorTex) 
			{
				colorTex = kinectManager.GetUsersClrTex2D();
			}
		}

		//faceRect = faceManager.GetFaceColorRect(userId);
		faceRect = GetHeadJointFaceRect(userId);

		if (faceRect.width > 0 && faceRect.height > 0) 
		{
			int faceX = (int)faceRect.x;
			int faceY = (int)faceRect.y;
			int faceW = (int)faceRect.width;
			int faceH = (int)faceRect.height;

			if(faceX < 0) faceX = 0;
			if(faceY < 0) faceY = 0;

			if (foregroundTex) 
			{
				if((faceX + faceW) > foregroundTex.width) faceW = foregroundTex.width - faceX;
				if((faceY + faceH) > foregroundTex.height) faceH = foregroundTex.height - faceY;
			} 
			else if (colorTex) 
			{
				if((faceX + faceW) > colorTex.width) faceW = colorTex.width - faceX;
				if((faceY + faceH) > colorTex.height) faceH = colorTex.height - faceY;
			}

			if(faceTex.width != faceW || faceTex.height != faceH)
			{
				faceTex.Resize(faceW, faceH);
			}

			if (foregroundTex) 
			{
				KinectInterop.RenderTex2Tex2D(foregroundTex, faceX, foregroundTex.height - faceY - faceH, faceW, faceH, ref faceTex);
			}
			else if(colorTex)
			{
				Color[] colorPixels = colorTex.GetPixels(faceX, faceY, faceW, faceH, 0);
				faceTex.SetPixels(colorPixels);
				faceTex.Apply();
			}

			if (targetObject && !targetObject.gameObject.activeSelf) 
			{
				targetObject.gameObject.SetActive(true);
			}

			if(targetObject && targetObject.material)
			{
				targetObject.material.mainTexture = faceTex;
			}

			// don't rotate the transform - mesh follows the head rotation
			if (targetObject.transform.rotation != Quaternion.identity) 
			{
				targetObject.transform.rotation = Quaternion.identity;
			}
		} 
		else 
		{
			if (targetObject && targetObject.gameObject.activeSelf) 
			{
				targetObject.gameObject.SetActive(false);
			}

			if(targetObject && targetObject.material && targetObject.material.mainTexture != null)
			{
				targetObject.material.mainTexture = null;
			}
		}
	}

	private Rect GetHeadJointFaceRect(long userId)
	{
		Rect faceJointRect = new Rect();

		if (useTrackedFaceRect && faceManager && 
			faceManager.IsFaceTrackingInitialized () && faceManager.IsTrackingFace (userId)) 
		{
			faceJointRect = faceManager.GetFaceColorRect(userId);
			return faceJointRect;
		}

		if(kinectManager.IsJointTracked(userId, (int)KinectInterop.JointType.Head))
		{
			Vector3 posHeadRaw = kinectManager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.Head);
			
			if(posHeadRaw != Vector3.zero)
			{
				Vector2 posDepthHead = kinectManager.MapSpacePointToDepthCoords(posHeadRaw);
				ushort depthHead = kinectManager.GetDepthForPixel((int)posDepthHead.x, (int)posDepthHead.y);
				
				Vector3 sizeHalfFace = new Vector3(faceWidth / 2f, faceHeight / 2f, 0f);
				Vector3 posFaceRaw1 = posHeadRaw - sizeHalfFace;
				Vector3 posFaceRaw2 = posHeadRaw + sizeHalfFace;
				
				Vector2 posDepthFace1 = kinectManager.MapSpacePointToDepthCoords(posFaceRaw1);
				Vector2 posDepthFace2 = kinectManager.MapSpacePointToDepthCoords(posFaceRaw2);

				if(posDepthFace1 != Vector2.zero && posDepthFace2 != Vector2.zero && depthHead > 0)
				{
					Vector2 posColorFace1 = kinectManager.MapDepthPointToColorCoords(posDepthFace1, depthHead);
					Vector2 posColorFace2 = kinectManager.MapDepthPointToColorCoords(posDepthFace2, depthHead);
					
					if(!float.IsInfinity(posColorFace1.x) && !float.IsInfinity(posColorFace1.y) &&
					   !float.IsInfinity(posColorFace2.x) && !float.IsInfinity(posColorFace2.y))
					{
						faceJointRect.x = posColorFace1.x;
						faceJointRect.y = posColorFace2.y;
						faceJointRect.width = Mathf.Abs(posColorFace2.x - posColorFace1.x);
						faceJointRect.height = Mathf.Abs(posColorFace2.y - posColorFace1.y);
					}
				}
			}
		}

		return faceJointRect;
	}

}
