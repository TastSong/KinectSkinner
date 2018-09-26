using UnityEngine;
using System.Collections;

public class BarrelTrigger : MonoBehaviour 
{
	[Tooltip("Reference to the BallController-component to be invoked, when the barrel gets hit by a ball.")]
	public BallController ballController;


	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Player") 
		{
			if (ballController) 
			{
				ballController.BarrelWasHit();
			}
		}
	}

}
