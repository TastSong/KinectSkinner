using UnityEngine;
using System.Collections;

public class ModelHatController : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Camera used to estimate the overlay positions of 3D-objects over the background.")]
	public Camera foregroundCamera;
	
	[Tooltip("Vertical offset of the hat above the head position (in meters).")]
	public float verticalOffset = 0f;

	[Tooltip("Scale factor for the hat model.")]
	[Range(0.1f, 2.0f)]
	public float modelScaleFactor = 1f;
	
	[Tooltip("Smooth factor used for hat-model rotation.")]
	public float smoothFactorRotation = 10f;

	[Tooltip("Smooth factor used for hat-model movement.")]
	public float smoothFactorMovement = 0f;
	
	private KinectManager kinectManager;
	private FacetrackingManager faceManager;
	private Quaternion initialRotation;


	void Start () 
	{
		initialRotation = transform.rotation;
		transform.localScale = new Vector3(modelScaleFactor, modelScaleFactor, modelScaleFactor);
	}
	
	void Update () 
	{
		// get the face-tracking manager instance
		if(faceManager == null)
		{
			kinectManager = KinectManager.Instance;
			faceManager = FacetrackingManager.Instance;
		}

		// get user-id by user-index
		long userId = kinectManager ? kinectManager.GetUserIdByIndex(playerIndex) : 0;

		if(kinectManager && kinectManager.IsInitialized() && userId != 0 &&
			faceManager && faceManager.IsTrackingFace(userId) && foregroundCamera)
		{
			// get head position
			Vector3 newPosition = faceManager.GetHeadPosition(userId, true);
			
			// get head rotation
			Quaternion newRotation = initialRotation * faceManager.GetHeadRotation(userId, true);

			// rotational fix, provided by Richard Borys:
			// The added rotation fixes rotational error that occurs when person is not centered in the middle of the kinect
			Vector3 addedRotation = newPosition.z != 0f ? new Vector3(Mathf.Rad2Deg * (Mathf.Tan(newPosition.y) / newPosition.z),
				Mathf.Rad2Deg * (Mathf.Tan(newPosition.x) / newPosition.z), 0) : Vector3.zero;
			
			addedRotation.x = newRotation.eulerAngles.x + addedRotation.x;
			addedRotation.y = newRotation.eulerAngles.y + addedRotation.y;
			addedRotation.z = newRotation.eulerAngles.z + addedRotation.z;
			
			newRotation = Quaternion.Euler(addedRotation.x, addedRotation.y, addedRotation.z);
			// end of rotational fix

			if(smoothFactorRotation != 0f)
				transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, smoothFactorRotation * Time.deltaTime);
			else
				transform.rotation = newRotation;

			// get the background rectangle (use the portrait background, if available)
			Rect backgroundRect = foregroundCamera.pixelRect;
			PortraitBackground portraitBack = PortraitBackground.Instance;
			
			if(portraitBack && portraitBack.enabled)
			{
				backgroundRect = portraitBack.GetBackgroundRect();
			}
			
			// model position
			newPosition = kinectManager.GetJointPosColorOverlay(userId, (int)KinectInterop.JointType.Head, foregroundCamera, backgroundRect);
			if(newPosition == Vector3.zero)
			{
				// hide the model behind the camera
				newPosition.z = -10f;
			}
			
			if(verticalOffset != 0f)
			{
				// add the vertical offset
				Vector3 dirHead = new Vector3(0, verticalOffset, 0);
				dirHead = transform.InverseTransformDirection(dirHead);
				newPosition += dirHead;
			}

			// go to the new position
			if(smoothFactorMovement != 0f && transform.position.z >= 0f)
				transform.position = Vector3.Lerp(transform.position, newPosition, smoothFactorMovement * Time.deltaTime);
			else
				transform.position = newPosition;

			// scale the model if needed
			if(transform.localScale.x != modelScaleFactor)
			{
				transform.localScale = new Vector3(modelScaleFactor, modelScaleFactor, modelScaleFactor);
			}
		}
		else
		{
			// hide the model behind the camera
			if(transform.position.z >= 0f)
			{
				transform.position = new Vector3(0f, 0f, -10f);
			}
		}
	}

}
