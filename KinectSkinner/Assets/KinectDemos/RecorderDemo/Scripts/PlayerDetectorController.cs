using UnityEngine;
using System.Collections;

public class PlayerDetectorController : MonoBehaviour 
{
	public float userLostMaxTime = 2f;

	private KinectRecorderPlayer saverPlayer;
	private KinectInterop.SensorData sensorData;
	private KinectInterop.BodyFrameData bodyFrame;
	private Matrix4x4 kinectToWorld;

	private float lastUserTime = 0f;


	void Start()
	{
		saverPlayer = KinectRecorderPlayer.Instance;

		sensorData = KinectManager.Instance.GetSensorData();
		kinectToWorld = KinectManager.Instance.GetKinectToWorldMatrix();
		bodyFrame = new KinectInterop.BodyFrameData(sensorData.bodyCount, KinectInterop.Constants.MaxJointCount);
	}

	void Update () 
	{
		if (!saverPlayer)
			return;
		
		bool bPlayerActive = saverPlayer.IsPlaying();

		if (bPlayerActive) 
		{
			if (KinectInterop.PollBodyFrame (sensorData, ref bodyFrame, ref kinectToWorld, false)) 
			{
				for (int i = 0; i < sensorData.bodyCount; i++) 
				{
					if (bodyFrame.bodyData [i].bIsTracked != 0) 
					{
						lastUserTime = Time.realtimeSinceStartup;
						break;
					}
				}

				lock (sensorData.bodyFrameLock) 
				{
					sensorData.bodyFrameReady = false;
				}
			}
		} 
		else 
		{
			if (KinectManager.Instance.GetUsersCount () > 0) 
			{
				lastUserTime = Time.realtimeSinceStartup;
			}
		}

		bool bUserFound = (Time.realtimeSinceStartup - lastUserTime) < userLostMaxTime;

		if(!bPlayerActive && !bUserFound) 
		{
			saverPlayer.StartPlaying();
		}
		else if(bPlayerActive && bUserFound)
		{
			saverPlayer.StopRecordingOrPlaying();
			KinectManager.Instance.ClearKinectUsers();
		}
	}

}
