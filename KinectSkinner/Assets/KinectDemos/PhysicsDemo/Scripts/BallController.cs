using UnityEngine;
using System.Collections;
//using Windows.Kinect;


public class BallController : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("The ball game object.")]
	public Transform ballObject;

	[Tooltip("Minimum movement distance, used to consider the ball being thrown.")]
	public float minThrowDistance = 0.3f;

	[Tooltip("Maximum time in seconds, used to consider the ball being thrown.")]
	public float timeThrowLimit = 0.3f;

	[Tooltip("Velocity scale.")]
	public float velocityScale = 5f;
	
	[Tooltip("UI-Text to display information messages.")]
	public UnityEngine.UI.Text infoText;

	public enum BallState : int { Hidden, HandRaise, BallThrow, BallWait }
	[Tooltip("Current state of the ball.")]
	public BallState currentState = BallState.Hidden;

//	public Transform ballP1;
//	public Transform ballP2;

	//public UnityEngine.UI.Text debugText;

	private KinectManager manager;
	private Quaternion initialRotation = Quaternion.identity;

	private long userId = 0;
    private int jointIndex = -1;

	// variables used for throwing
	private Vector3 lowestPos;
	private Vector3 handPos1, handPos2;
	private float handTime1, handTime2;

	// number of hits
	private int hitPoints = 0;


    void Start()
	{
        if (ballObject)
		{
			initialRotation = ballObject.rotation;
		}

		if (infoText) 
		{
			infoText.text = "Raise hand, throw the ball and try to hit the barrel.";
		}
	}
	
	void Update () 
	{
		manager = KinectManager.Instance;

		if(manager && manager.IsInitialized())
		{
			userId = manager.GetUserIdByIndex(playerIndex);

			switch (currentState) 
			{
			case BallState.Hidden:
				UpdateBallHide();

				// try to catch
				currentState = BallState.HandRaise;
				break;

			case BallState.HandRaise:
				UpdateHandRaise();
				break;

			case BallState.BallThrow:
				UpdateBallThrow();
               	break;

			case BallState.BallWait:
				break;
			}
		}

		if (infoText) 
		{
			//infoText.text = currentState.ToString();
		}
	}


	private void UpdateBallHide()
	{
		if (ballObject) 
		{
			ballObject.position = new Vector3 (0, 0, -10);
			ballObject.rotation = initialRotation;

			ballObject.GetComponent<Rigidbody>().isKinematic = true;
		}
	}


    private void UpdateHandRaise()
    {
        jointIndex = -1;
		Vector3 vLowestPos = Vector3.zero;
		Vector3 vHandPos = Vector3.zero;

        // check for left hand raise
		if (manager.IsJointTracked(userId, (int)KinectInterop.JointType.ShoulderRight) && manager.IsJointTracked(userId, (int)KinectInterop.JointType.HandLeft))
        {
			vLowestPos = GetJointPositionInv(userId, (int)KinectInterop.JointType.ShoulderRight);
			vHandPos = GetJointPositionInv(userId, (int)KinectInterop.JointType.HandLeft);

			if (vHandPos.y > vLowestPos.y) 
			{
				jointIndex = (int)KinectInterop.JointType.HandLeft;

				lowestPos = vLowestPos;
				handPos1 = vHandPos;
				handTime1 = Time.realtimeSinceStartup;
			}
        }

        // check for right hand raise
		if (manager.IsJointTracked(userId, (int)KinectInterop.JointType.ShoulderLeft) && manager.IsJointTracked(userId, (int)KinectInterop.JointType.HandRight))
        {
			vLowestPos = GetJointPositionInv(userId, (int)KinectInterop.JointType.ShoulderLeft);
			vHandPos = GetJointPositionInv(userId, (int)KinectInterop.JointType.HandRight);

			if (vHandPos.y > vLowestPos.y) 
			{
				jointIndex = (int)KinectInterop.JointType.HandRight;

				lowestPos = vLowestPos;
				handPos1 = vHandPos;
				handTime1 = Time.realtimeSinceStartup;
			}
        }

		if (jointIndex >= 0 && ballObject)
        {
			ballObject.position = vHandPos;
			currentState = BallState.BallThrow;
        }
    }


	private void UpdateBallThrow()
	{
		// check for push
		if (jointIndex >= 0 && manager.IsJointTracked (userId, jointIndex) && GetJointPositionInv(userId, jointIndex).y >= lowestPos.y) 
		{
			handPos2 = GetJointPositionInv(userId, jointIndex);
			handTime2 = Time.realtimeSinceStartup;

			ballObject.position = handPos2;

			Vector3 throwDir = handPos2 - handPos1;
			float throwDist = throwDir.magnitude;
			float throwTime = handTime2 - handTime1;

			if ((throwTime <= timeThrowLimit) && (throwDist >= minThrowDistance) && (handPos2.z > handPos1.z)) 
			{
				// test succeeded - ball was thrown
				float velocity = throwDist / throwTime;
				Debug.Log(string.Format("Dist: {0:F3}; Time: {1:F3}; Velocity: {2:F3}", throwDist, throwTime, velocity));

//				if (ballP1)
//					ballP1.position = handPos1;
//				if (ballP2) 
//				{
//					ballP2.position = handPos2;
//					ballP2.forward = throwDir.normalized;
//				}

				if (ballObject) 
				{
					ballObject.forward = throwDir.normalized;
					Rigidbody rb = ballObject.GetComponent<Rigidbody>();

					if (rb) 
					{
						rb.velocity = throwDir * velocity * velocityScale;
						rb.isKinematic = false;
					}

					currentState = BallState.BallWait;
					StartCoroutine (WaitForBall());
				}
			}
			else if ((handTime2 - handTime1) > timeThrowLimit) 
			{
				// too slow, start new test
				handPos1 = handPos2;
				handTime1 = handTime2;
			}
		} 
		else 
		{
			// throw was cancelled
			currentState = BallState.Hidden;
		}
	}


	private Vector3 GetJointPositionInv(long userId, int jointIndex)
	{
		if (manager)
		{
			Vector3 userPos = manager.GetUserPosition(userId);
			Vector3 jointPos = manager.GetJointPosition(userId, jointIndex);

			Vector3 jointDiff = jointPos - userPos;
			jointDiff.z = -jointDiff.z;
			jointPos = userPos + jointDiff;

			return jointPos;
		}

		return Vector3.zero;
	}


	private IEnumerator WaitForBall()
	{
		// wait 3 seconds
		yield return new WaitForSeconds(3f);

//		if (ballP1)
//			ballP1.position = new Vector3(0, 0, -10);
//		if (ballP2)
//			ballP2.position = new Vector3(0, 0, -10);

		// start over
		currentState = BallState.Hidden;
	}


	// invoked by BarrelTrigger-script, when the barrel was hit by the ball
	public void BarrelWasHit()
	{
		hitPoints++;

		if (infoText) 
		{
			infoText.text = "Barrel hits: " + hitPoints;
		}
	}

}
