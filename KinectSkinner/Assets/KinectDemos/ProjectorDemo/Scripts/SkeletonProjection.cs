using UnityEngine;
using System.Collections;
//using Windows.Kinect;


public class SkeletonProjection : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Whether to flip left and right, relative to the sensor.")]
	public bool flipLeftRight = false;

	[Tooltip("Game object used to overlay the joints.")]
	public GameObject jointPrefab;

	[Tooltip("Line object used to overlay the bones.")]
	public LineRenderer linePrefab;


	private GameObject[] joints = null;
	private LineRenderer[] lines = null;

	private Quaternion initialRotation = Quaternion.identity;


	void Start()
	{
		KinectManager manager = KinectManager.Instance;

		if(manager && manager.IsInitialized())
		{
			int jointsCount = manager.GetJointCount();

			if(jointPrefab)
			{
				// array holding the skeleton joints
				joints = new GameObject[jointsCount];
				
				for(int i = 0; i < joints.Length; i++)
				{
					joints[i] = Instantiate(jointPrefab) as GameObject;
					joints[i].transform.parent = transform;
					joints[i].name = ((KinectInterop.JointType)i).ToString();
					joints[i].SetActive(false);
				}
			}

			// array holding the skeleton lines
			lines = new LineRenderer[jointsCount];
		}

		// always mirrored
		initialRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
	}
	
	void Update () 
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			// overlay all joints in the skeleton
			if(manager.IsUserDetected(playerIndex))
			{
				long userId = manager.GetUserIdByIndex(playerIndex);
				int jointsCount = manager.GetJointCount();

				for(int i = 0; i < jointsCount; i++)
				{
					int joint = i;

					if(manager.IsJointTracked(userId, joint))
					{
						Vector3 posJoint = manager.GetJointPosition(userId, joint);
						if(flipLeftRight)
							posJoint.x = -posJoint.x;

						if(joints != null)
						{
							// overlay the joint
							if(posJoint != Vector3.zero)
							{
								joints[i].SetActive(true);
								joints[i].transform.position = posJoint;

								Quaternion rotJoint = manager.GetJointOrientation(userId, joint, false);
								rotJoint = initialRotation * rotJoint;
								joints[i].transform.rotation = rotJoint;
							}
							else
							{
								joints[i].SetActive(false);
							}
						}

						if(lines[i] == null && linePrefab != null)
						{
							lines[i] = Instantiate(linePrefab) as LineRenderer;
							lines[i].transform.parent = transform;
							lines[i].gameObject.SetActive(false);
						}

						if(lines[i] != null)
						{
							// overlay the line to the parent joint
							int jointParent = (int)manager.GetParentJoint((KinectInterop.JointType)joint);

							Vector3 posParent = manager.GetJointPosition(userId, jointParent);
							if(flipLeftRight)
								posParent.x = -posParent.x;

							if(posJoint != Vector3.zero && posParent != Vector3.zero)
							{
								lines[i].gameObject.SetActive(true);
								
								//lines[i].SetVertexCount(2);
								lines[i].SetPosition(0, posParent);
								lines[i].SetPosition(1, posJoint);
							}
							else
							{
								lines[i].gameObject.SetActive(false);
							}
						}
						
					}
					else
					{
						if(joints != null)
						{
							joints[i].SetActive(false);
						}
						
						if(lines[i] != null)
						{
							lines[i].gameObject.SetActive(false);
						}
					}
				}

			}
		}
	}

}
