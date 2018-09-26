using UnityEngine;
using System.Collections;

public class UserPlaneMover : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 - the 1st player, 1 - the 2nd player, etc.")]
	public int playerIndex = 0;

//	[Tooltip("Whether the user position is calculated relative to the initial user position.")]
//	public bool relToInitialPos = false;

	[Tooltip("Smooth factor used for the transform movement.")]
	public float smoothFactor = 20f;


	private KinectManager manager = null;
	private long lastUserId = 0;

	private long userId = 0;
	private Vector3 initialPlanePos = Vector3.zero;
//	private Vector3 initialUserPos = Vector3.zero;
	private Vector3 currentUserPos = Vector3.zero;


	void Start () 
	{
		manager = KinectManager.Instance;
		initialPlanePos = transform.position;
	}
	
	void Update () 
	{
		if (manager == null || !manager.IsInitialized())
			return;

		userId = manager.GetUserIdByIndex(playerIndex);
		currentUserPos = manager.GetUserPosition(userId);

		if (userId != 0 && userId != lastUserId) 
		{
			lastUserId = userId;
//			initialUserPos = currentUserPos;
		}

		if (userId != 0) 
		{
			Vector3 deltaUserPos = currentUserPos; // relToInitialPos ? (currentUserPos - initialUserPos) : currentUserPos;
			Vector3 newPlanePos = initialPlanePos + new Vector3(0f, 0f, deltaUserPos.z);

			transform.position = Vector3.Lerp(transform.position, newPlanePos, smoothFactor * Time.deltaTime);
		} 
		else 
		{
			lastUserId = 0;
//			initialUserPos = Vector3.zero;

			transform.position = initialPlanePos;
		}
	
	}

}
