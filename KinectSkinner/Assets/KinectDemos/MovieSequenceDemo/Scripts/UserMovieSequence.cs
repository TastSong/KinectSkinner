using UnityEngine;
using System;

class UserMovieSequence : MonoBehaviour
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("How far left or right from the camera may be the user, in meters.")]
	public float limitLeftRight = 1.2f;

	[Tooltip("RawImage to display the movie frames.")]
	public UnityEngine.UI.RawImage movieGuiTexture = null;

	[Tooltip("List of frames in the movie sequence.")]
	public Texture[] frameTextures = null;

	[Tooltip("Smooth factor used for frame interpolation.")]
	public float smoothFactor = 10f;

	[Tooltip("Current frame number (as estimated).")]
	public int currentFrame = 0;

	[Tooltip("UI-Text to display status messages.")]
	public UnityEngine.UI.Text statusText = null;


	private KinectManager kinectManager;
	private int numberOfFrames;
	private float fCurrentFrame;



	void Start()
	{
		kinectManager = KinectManager.Instance;
		numberOfFrames = frameTextures != null ? frameTextures.Length : 0;
		fCurrentFrame = 0f;
	}

	void Update()
	{
		if (kinectManager && kinectManager.IsInitialized()) 
		{
			long userId = kinectManager.GetUserIdByIndex(playerIndex);

			if (kinectManager.IsUserTracked (userId) && kinectManager.IsJointTracked (userId, (int)KinectInterop.JointType.SpineBase)) 
			{
				Vector3 userPos = kinectManager.GetJointPosition (userId, (int)KinectInterop.JointType.SpineBase);

				if (userPos.x >= -limitLeftRight && userPos.x <= limitLeftRight) 
				{
					// calculate the relative position in the movie
					float relPos = (userPos.x + limitLeftRight) / (2f * limitLeftRight);
					fCurrentFrame = (fCurrentFrame != 0f) ? Mathf.Lerp (fCurrentFrame, relPos * (numberOfFrames - 1), smoothFactor * Time.deltaTime) : (relPos * (numberOfFrames - 1));

					// current frame index
					currentFrame = Mathf.RoundToInt(fCurrentFrame);

					if (statusText) 
					{
						statusText.text = string.Format ("X-Pos: {0:F2}, RelPos: {1:F3}, Frame: {2}", userPos.x, relPos, currentFrame);
					}
				}
			} 
//			else 
//			{
//				fCurrentFrame = 0f;
//			}

			// display the frame with 'currentFrame' index
			if(frameTextures != null && currentFrame >= 0 && currentFrame < frameTextures.Length) 
			{
				Texture tex = frameTextures[currentFrame];

				if (movieGuiTexture) 
				{
					movieGuiTexture.texture = tex;
					movieGuiTexture.color = Color.white;
				}
			}

		}
	}

}
