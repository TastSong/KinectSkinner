using UnityEngine;
using System.Collections;

public class BallMover : MonoBehaviour 
{
	void Update () 
	{
        if (transform.position.y < -2f)
        {
            Destroy(gameObject);
        }
	}
}
