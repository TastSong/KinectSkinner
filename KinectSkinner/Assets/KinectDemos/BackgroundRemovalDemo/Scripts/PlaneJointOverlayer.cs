using UnityEngine;
using System.Collections;
using System;
//using Windows.Kinect;


public class PlaneJointOverlayer : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Kinect joint that is going to be overlayed.")]
	public KinectInterop.JointType trackedJoint = KinectInterop.JointType.HandRight;

	[Tooltip("The Plane game object.")]
	public Transform planeObject;

	[Tooltip("Game object used to overlay the joint over the plane.")]
	public Transform overlayObject;

	[Tooltip("Smoothing factor used for joint rotation.")]
	public float smoothFactor = 10f;

	//private Quaternion initialRotation = Quaternion.identity;
	//private bool objFlipped = false;

	private Rect planeRect = new Rect();
	private bool planeRectSet = false;

	
	public void Start()
	{
		if(overlayObject)
		{
			// always mirrored
			//initialRotation = overlayObject.rotation; // Quaternion.Euler(new Vector3(0f, 180f, 0f));
			//objFlipped = (Vector3.Dot(overlayObject.forward, Vector3.forward) < 0);

			overlayObject.rotation = Quaternion.identity;
		}
	}
	
	void Update () 
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			// get the plane rectangle to be used for object overlay
			if (!planeRectSet && planeObject) 
			{
				planeRectSet = true;

				planeRect.width = 10f * Mathf.Abs(planeObject.localScale.x);
				planeRect.height = 10f * Mathf.Abs(planeObject.localScale.z);
				planeRect.x = planeObject.position.x - planeRect.width / 2f;
				planeRect.y = planeObject.position.y - planeRect.height / 2f;
			}

			// overlay the object
			long userId = manager.GetUserIdByIndex(playerIndex);
			
			int iJointIndex = (int)trackedJoint;
			if (planeObject && manager.IsJointTracked (userId, iJointIndex)) 
			{
				//Vector3 posJoint = manager.GetJointPosColorOverlay(userId, iJointIndex, foregroundCamera, backgroundRect);
				Vector3 posJoint = manager.GetJointPosColorOverlay(userId, iJointIndex, planeRect);
				posJoint.z = planeObject.position.z;

				if (posJoint != Vector3.zero) 
				{
					if (overlayObject) 
					{
						overlayObject.position = posJoint;

//						Quaternion rotJoint = manager.GetJointOrientation(userId, iJointIndex, !objFlipped);
//						rotJoint = initialRotation * rotJoint;
//
//						overlayObject.rotation = Quaternion.Slerp (overlayObject.rotation, rotJoint, smoothFactor * Time.deltaTime);
					}
				}
			} 
			else 
			{
				// make the overlay object invisible
				if (overlayObject && overlayObject.position.z > 0f) 
				{
					Vector3 posJoint = overlayObject.position;
					posJoint.z = -10f;
					overlayObject.position = posJoint;
				}
			}
				
		}
	}


}
