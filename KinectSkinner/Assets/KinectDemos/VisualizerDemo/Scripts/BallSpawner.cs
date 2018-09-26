using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Prefab used to instantiate balls in the scene.")]
    public Transform ballPrefab;

	[Tooltip("Prefab used to instantiate cubes in the scene.")]
	public Transform cubePrefab;
	
	[Tooltip("How many objects do we want to spawn.")]
	public int numberOfObjects = 20;

    private float nextSpawnTime = 0.0f;
    private float spawnRate = 1.5f;
	private int ballsCount = 0;
 	

	void Update () 
	{
        if (nextSpawnTime < Time.time)
        {
            SpawnBalls();
            nextSpawnTime = Time.time + spawnRate;

			spawnRate = Random.Range(0f, 1f);
			//numberOfBalls = Mathf.RoundToInt(Random.Range(1f, 10f));
        }
	}

    void SpawnBalls()
    {
		KinectManager manager = KinectManager.Instance;

		if(ballPrefab && cubePrefab && ballsCount < numberOfObjects &&
			manager && manager.IsInitialized() && manager.IsUserDetected(playerIndex))
		{
			long userId = manager.GetUserIdByIndex(playerIndex);
			Vector3 posUser = manager.GetUserPosition(userId);

			float xPos = Random.Range(-1.5f, 1.5f);
			float zPos = Random.Range(-1.5f, 1.5f);
			Vector3 spawnPos = new Vector3(posUser.x + xPos, posUser.y, posUser.z + zPos);

			int ballOrCube = Mathf.RoundToInt(Random.Range(0f, 1f));

			Transform ballTransform = Instantiate(ballOrCube > 0 ? ballPrefab : cubePrefab, spawnPos, Quaternion.identity) as Transform;
			ballTransform.GetComponent<Renderer>().material.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
			ballTransform.parent = transform;

			ballsCount++;
		}
    }

}
