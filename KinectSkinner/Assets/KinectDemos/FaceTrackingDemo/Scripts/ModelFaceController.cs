using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModelFaceController : MonoBehaviour 
{
	public enum AxisEnum { X, Y, Z };
	
	[Tooltip("Transform of the joint, used to move and rotate the head.")]
	public Transform HeadTransform;

	[Tooltip("Whether the model's head is facing the player or not.")]
	public bool mirroredHeadMovement = true;

	[Tooltip("Camera used to estimate the overlay position of the head over the background.")]
	public Camera foregroundCamera;

	// for testing purposes
	//public Transform overlayObj;
	
	[Tooltip("Vertical offset of the model above the head position.")]
	public float verticalOffset = 0f;
	
	[Tooltip("Scale factor for the head model.")]
	[Range(0.1f, 2.0f)]
	public float modelScaleFactor = 1f;
	
	[Tooltip("Smooth factor used for head movement and head-joint rotations.")]
	public float smoothFactor = 10f;
	
	// Upper Lip Left
	[Tooltip("Left upper lip transform.")]
	public Transform UpperLipLeft;
	[Tooltip("Left upper lip axis of rotation.")]
	public AxisEnum UpperLipLeftAxis;
	[Tooltip("Maximum up-value for the left upper lip, down-value is the opposite one.")]
	public float UpperLipLeftUp;

	// Upper Lip Right
	[Tooltip("Right upper lip transform.")]
	public Transform UpperLipRight;
	[Tooltip("Right upper lip axis of rotation.")]
	public AxisEnum UpperLipRightAxis;
	[Tooltip("Maximum up-value for the right upper lip, down-value is the opposite one.")]
	public float UpperLipRightUp;

	// Jaw
	[Tooltip("Jaw (mouth) transform.")]
	public Transform Jaw;
	[Tooltip("Jaw axis of rotation.")]
	public AxisEnum JawAxis;
	[Tooltip("Maximum down-value for the jaw, up-value is the opposite one.")]
	public float JawDown;
	
	// Lip Left
	[Tooltip("Left lip transform.")]
	public Transform LipLeft;
	[Tooltip("Left lip axis of rotation.")]
	public AxisEnum LipLeftAxis;
	[Tooltip("Maximum stretched-value for the left lip, rounded-value is the opposite one.")]
	public float LipLeftStretched;

	// Lip Right
	[Tooltip("Right lip transform.")]
	public Transform LipRight;
	[Tooltip("Right lip axis of rotation.")]
	public AxisEnum LipRightAxis;
	[Tooltip("Maximum stretched-value for the right lip, rounded-value is the opposite one.")]
	public float LipRightStretched;

	// Eyebrow Left
	[Tooltip("Left eyebrow transform.")]
	public Transform EyebrowLeft;
	[Tooltip("Left eyebrow axis of rotation.")]
	public AxisEnum EyebrowLeftAxis;
	[Tooltip("Maximum lowered-value for the left eyebrow, raised-value is the opposite one.")]
	public float EyebrowLeftLowered;

	// Eyebrow Right
	[Tooltip("Right eyebrow transform.")]
	public Transform EyebrowRight;
	[Tooltip("Right eyebrow axis of rotation.")]
	public AxisEnum EyebrowRightAxis;
	[Tooltip("Maximum lowered-value for the right eyebrow, raised-value is the opposite one.")]
	public float EyebrowRightLowered;
	
	// Lip Corner Left
	[Tooltip("Left lip-corner transform.")]
	public Transform LipCornerLeft;
	[Tooltip("Left lip-corner axis of rotation.")]
	public AxisEnum LipCornerLeftAxis;
	[Tooltip("Maximum depressed-value for the left lip-corner, smile-value is the opposite one.")]
	public float LipCornerLeftDepressed;

	// Lip Corner Right
	[Tooltip("Right lip-corner transform.")]
	public Transform LipCornerRight;
	[Tooltip("Right lip-corner axis of rotation.")]
	public AxisEnum LipCornerRightAxis;
	[Tooltip("Maximum depressed-value for the right lip-corner, smile-value is the opposite one.")]
	public float LipCornerRightDepressed;

	// Upper Eyelid Left
	[Tooltip("Left upper eyelid transform.")]
	public Transform UpperEyelidLeft;
	[Tooltip("Left upper eyelid axis of rotation.")]
	public AxisEnum UpperEyelidLeftAxis;
	[Tooltip("Maximum lowered-value for the left upper eyelid, raised-value is the opposite one.")]
	public float UpperEyelidLeftLowered;

	// Upper Eyelid Right
	[Tooltip("Right upper eyelid transform.")]
	public Transform UpperEyelidRight;
	[Tooltip("Right upper eyelid axis of rotation.")]
	public AxisEnum UpperEyelidRightAxis;
	[Tooltip("Maximum lowered-value for the right upper eyelid, raised-value is the opposite one.")]
	public float UpperEyelidRightLowered;
	
	// Lower Eyelid Left
	[Tooltip("Left lower eyelid transform.")]
	public Transform LowerEyelidLeft;
	[Tooltip("Left lower eyelid axis of rotation.")]
	public AxisEnum LowerEyelidLeftAxis;
	[Tooltip("Maximum raised-value for the left lower eyelid, lowered-value is the opposite one.")]
	public float LowerEyelidLeftRaised;

	// Lower Eyelid Right
	[Tooltip("Right lower eyelid transform.")]
	public Transform LowerEyelidRight;
	[Tooltip("Right lower eyelid axis of rotation.")]
	public AxisEnum LowerEyelidRightAxis;
	[Tooltip("Maximum raised-value for the right lower eyelid, lowered-value is the opposite one.")]
	public float LowerEyelidRightRaised;
	
	
	private FacetrackingManager manager;
	private KinectInterop.DepthSensorPlatform platform;
	
	private Vector3 HeadInitialPosition;
	private Quaternion HeadInitialRotation;
	
	private float UpperLipLeftNeutral;
	private float UpperLipRightNeutral;
	private float JawNeutral;
	private float LipLeftNeutral;
	private float LipRightNeutral;
	private float EyebrowLeftNeutral;
	private float EyebrowRightNeutral;
	private float LipCornerLeftNeutral;
	private float LipCornerRightNeutral;
	private float UpperEyelidLeftNeutral;
	private float UpperEyelidRightNeutral;
	private float LowerEyelidLeftNeutral;
	private float LowerEyelidRightNeutral;

	
	void Start()
	{
		if(HeadTransform != null)
		{
			HeadInitialPosition = HeadTransform.position;
			//HeadInitialPosition.z = 0;
			HeadInitialRotation = HeadTransform.rotation;
		}
		
		UpperLipLeftNeutral = GetJointRotation(UpperLipLeft, UpperLipLeftAxis);
		UpperLipRightNeutral = GetJointRotation(UpperLipRight, UpperLipRightAxis);
		
		JawNeutral = GetJointRotation(Jaw, JawAxis);
		
		LipLeftNeutral = GetJointRotation(LipLeft, LipLeftAxis);
		LipRightNeutral = GetJointRotation(LipRight, LipRightAxis);
		
		EyebrowLeftNeutral = GetJointRotation(EyebrowLeft, EyebrowLeftAxis);
		EyebrowRightNeutral = GetJointRotation(EyebrowRight, EyebrowRightAxis);
		
		LipCornerLeftNeutral = GetJointRotation(LipCornerLeft, LipCornerLeftAxis);
		LipCornerRightNeutral = GetJointRotation(LipCornerRight, LipCornerRightAxis);
		
		UpperEyelidLeftNeutral = GetJointRotation(UpperEyelidLeft, UpperEyelidLeftAxis);
		UpperEyelidRightNeutral = GetJointRotation(UpperEyelidRight, UpperEyelidRightAxis);

		LowerEyelidLeftNeutral = GetJointRotation(LowerEyelidLeft, LowerEyelidLeftAxis);
		LowerEyelidRightNeutral = GetJointRotation(LowerEyelidRight, LowerEyelidRightAxis);

		KinectManager kinectManager = KinectManager.Instance;
		if(kinectManager && kinectManager.IsInitialized())
		{
			platform = kinectManager.GetSensorPlatform();
		}
	}
	
	void Update() 
	{
		// get the face-tracking manager instance
		if(manager == null)
		{
			manager = FacetrackingManager.Instance;
		}

		if(manager && manager.GetFaceTrackingID() != 0)
		{
			// set head position & rotation
			if(HeadTransform != null)
			{
				// head position
				Vector3 newPosition = manager.GetHeadPosition(mirroredHeadMovement);
				
				// head rotation
				Quaternion newRotation = HeadInitialRotation * manager.GetHeadRotation(mirroredHeadMovement);
				
				// rotational fix, provided by Richard Borys:
				// The added rotation fixes rotational error that occurs when person is not centered in the middle of the kinect
				Vector3 addedRotation = newPosition.z != 0f ? new Vector3(Mathf.Rad2Deg * (Mathf.Tan(newPosition.y) / newPosition.z),
				                                                          Mathf.Rad2Deg * (Mathf.Tan(newPosition.x) / newPosition.z), 0) : Vector3.zero;
				
				addedRotation.x = newRotation.eulerAngles.x + addedRotation.x;
				addedRotation.y = newRotation.eulerAngles.y + addedRotation.y;
				addedRotation.z = newRotation.eulerAngles.z + addedRotation.z;
				
				newRotation = Quaternion.Euler(addedRotation.x, addedRotation.y, addedRotation.z);
				// end of rotational fix
				
				if(smoothFactor != 0f)
					HeadTransform.rotation = Quaternion.Slerp(HeadTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
				else
					HeadTransform.rotation = newRotation;

				// check for head pos overlay
				if(foregroundCamera)
				{
					// get the background rectangle (use the portrait background, if available)
					Rect backgroundRect = foregroundCamera.pixelRect;
					PortraitBackground portraitBack = PortraitBackground.Instance;
					
					if(portraitBack && portraitBack.enabled)
					{
						backgroundRect = portraitBack.GetBackgroundRect();
					}

					KinectManager kinectManager = KinectManager.Instance;

					if(kinectManager)
					{
						long userId = kinectManager.GetUserIdByIndex(manager.playerIndex);
						Vector3 posColorOverlay = kinectManager.GetJointPosColorOverlay(userId, (int)KinectInterop.JointType.Head, foregroundCamera, backgroundRect);
						
						if(posColorOverlay != Vector3.zero)
						{
							newPosition = posColorOverlay;

//							if(overlayObj)
//							{
//								overlayObj.position = newPosition;
//							}
						}
					}
				}
				else
				{
					// move around the initial position
					newPosition += HeadInitialPosition;
				}

				// vertical offet
				if(verticalOffset != 0f)
				{
					// add the vertical offset
					Vector3 dirHead = new Vector3(0, verticalOffset, 0);
					dirHead = HeadTransform.InverseTransformDirection(dirHead);
					newPosition += dirHead;
				}

				// set the position
				if(smoothFactor != 0f)
					HeadTransform.position = Vector3.Lerp(HeadTransform.position, newPosition, smoothFactor * Time.deltaTime);
				else
					HeadTransform.position = newPosition;

				// scale factor
				if(HeadTransform.localScale.x != modelScaleFactor)
				{
					HeadTransform.localScale = new Vector3(modelScaleFactor, modelScaleFactor, modelScaleFactor);
				}
			}
			
			// apply animation units

			// AU0 - Upper Lip Raiser
			// 0=neutral, covering teeth; 1=showing teeth fully; -1=maximal possible pushed down lip
			float fAU0 = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.LipPucker);
			SetJointRotation(UpperLipLeft, UpperLipLeftAxis, fAU0, UpperLipLeftNeutral, UpperLipLeftUp);
			SetJointRotation(UpperLipRight, UpperLipRightAxis, fAU0, UpperLipRightNeutral, UpperLipRightUp);
			
			// AU1 - Jaw Lowerer
			// 0=closed; 1=fully open; -1= closed, like 0
			float fAU1 = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.JawOpen);
			SetJointRotation(Jaw, JawAxis, fAU1, JawNeutral, JawDown);
			
			// AU2 – Lip Stretcher
			// 0=neutral; 1=fully stretched (joker’s smile); -1=fully rounded (kissing mouth)
			float fAU2_left = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.LipStretcherLeft);
			fAU2_left = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU2_left * 2 - 1) : fAU2_left;
			SetJointRotation(LipLeft, LipLeftAxis, fAU2_left, LipLeftNeutral, LipLeftStretched);

			float fAU2_right = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.LipStretcherRight);
			fAU2_right = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU2_right * 2 - 1) : fAU2_right;
			SetJointRotation(LipRight, LipRightAxis, fAU2_right, LipRightNeutral, LipRightStretched);
			
			// AU3 – Brow Lowerer
			// 0=neutral; -1=raised almost all the way; +1=fully lowered (to the limit of the eyes)
			float fAU3_left = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.LefteyebrowLowerer);
			fAU3_left = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU3_left * 2 - 1) : fAU3_left;
			SetJointRotation(EyebrowLeft, EyebrowLeftAxis, fAU3_left, EyebrowLeftNeutral, EyebrowLeftLowered);

			float fAU3_right = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.RighteyebrowLowerer);
			fAU3_right = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU3_right * 2 - 1) : fAU3_right;
			SetJointRotation(EyebrowRight, EyebrowRightAxis, fAU3_right, EyebrowRightNeutral, EyebrowRightLowered);
			
			// AU4 – Lip Corner Depressor
			// 0=neutral; -1=very happy smile; +1=very sad frown
			float fAU4_left = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.LipCornerDepressorLeft);
			fAU4_left = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU4_left * 2) : fAU4_left;
			SetJointRotation(LipCornerLeft, LipCornerLeftAxis, fAU4_left, LipCornerLeftNeutral, LipCornerLeftDepressed);

			float fAU4_right = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.LipCornerDepressorRight);
			fAU4_right = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU4_right * 2) : fAU4_right;
			SetJointRotation(LipCornerRight, LipCornerRightAxis, fAU4_right, LipCornerRightNeutral, LipCornerRightDepressed);

			// AU6, AU7 – Eyelid closed
			// 0=neutral; -1=raised; +1=fully lowered
			float fAU6_left = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.LefteyeClosed);
			fAU6_left = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU6_left * 2 - 1) : fAU6_left;
			SetJointRotation(UpperEyelidLeft, UpperEyelidLeftAxis, fAU6_left, UpperEyelidLeftNeutral, UpperEyelidLeftLowered);
			SetJointRotation(LowerEyelidLeft, LowerEyelidLeftAxis, fAU6_left, LowerEyelidLeftNeutral, LowerEyelidLeftRaised);

			float fAU6_right = manager.GetAnimUnit(KinectInterop.FaceShapeAnimations.RighteyeClosed);
			fAU6_right = (platform == KinectInterop.DepthSensorPlatform.KinectSDKv2) ? (fAU6_right * 2 - 1) : fAU6_right;
			SetJointRotation(UpperEyelidRight, UpperEyelidRightAxis, fAU6_right, UpperEyelidRightNeutral, UpperEyelidRightLowered);
			SetJointRotation(LowerEyelidRight, LowerEyelidRightAxis, fAU6_right, LowerEyelidRightNeutral, LowerEyelidRightRaised);
		}
		else
		{
			// hide the model behind the camera
			if(HeadTransform && HeadTransform.position.z >= 0f)
			{
				HeadTransform.position = new Vector3(0f, 0f, -10f);
			}
		}
	}
	
	private float GetJointRotation(Transform joint, AxisEnum axis)
	{
		float fJointRot = 0.0f;
		
		if(joint == null)
			return fJointRot;
		
		Vector3 jointRot = joint.localRotation.eulerAngles;
		
		switch(axis)
		{
			case AxisEnum.X:
				fJointRot = jointRot.x;
				break;
			
			case AxisEnum.Y:
				fJointRot = jointRot.y;
				break;
			
			case AxisEnum.Z:
				fJointRot = jointRot.z;
				break;
		}
		
		return fJointRot;
	}
	
	private void SetJointRotation(Transform joint, AxisEnum axis, float fAU, float fMin, float fMax)
	{
		if(joint == null)
			return;
		
//		float fSign = 1.0f;
//		if(fMax < fMin)
//			fSign = -1.0f;
		
		// [-1, +1] -> [0, 1]
		//fAUnorm = (fAU + 1f) / 2f;
		float fValue = fMin + (fMax - fMin) * fAU;
		
		Vector3 jointRot = joint.localRotation.eulerAngles;
		
		switch(axis)
		{
			case AxisEnum.X:
				jointRot.x = fValue;
				break;
			
			case AxisEnum.Y:
				jointRot.y = fValue;
				break;
			
			case AxisEnum.Z:
				jointRot.z = fValue;
				break;
		}

		if(smoothFactor != 0f)
			joint.localRotation = Quaternion.Slerp(joint.localRotation, Quaternion.Euler(jointRot), smoothFactor * Time.deltaTime);
		else
			joint.localRotation = Quaternion.Euler(jointRot);
	}
	
	
}
