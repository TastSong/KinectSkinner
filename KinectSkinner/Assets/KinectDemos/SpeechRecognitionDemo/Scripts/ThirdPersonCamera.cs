using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	[Tooltip("Smoothing factor of camera motion.")]
	public float smooth = 3f;		// a public variable to adjust smoothing of camera motion

	private Transform standardPos;			// the usual position for the camera, specified by a transform in the game
	private Transform lookAtPos;			// the position to move the camera to when using head look


	void Start()
	{
		// initialising references
		standardPos = GameObject.Find ("CamPos").transform;
		
		if(GameObject.Find ("LookAtPos"))
			lookAtPos = GameObject.Find ("LookAtPos").transform;
	}
	
	void FixedUpdate ()
	{
		// if we hold Alt
		if(Input.GetButton("Fire2") && lookAtPos)
		{
			// lerp the camera position to the look at position, and lerp its forward direction to match 
			transform.position = Vector3.Lerp(transform.position, lookAtPos.position, smooth > 0f ? Time.deltaTime * smooth : 1f);
			transform.forward = Vector3.Lerp(transform.forward, lookAtPos.forward, smooth > 0f ? Time.deltaTime * smooth : 1f);
		}
		else
		{	
			// return the camera to standard position and direction
			transform.position = Vector3.Lerp(transform.position, standardPos.position, smooth > 0f ? Time.deltaTime * smooth : 1f);	
			transform.forward = Vector3.Lerp(transform.forward, standardPos.forward, smooth > 0f ? Time.deltaTime * smooth : 1f);
		}
		
	}
}
