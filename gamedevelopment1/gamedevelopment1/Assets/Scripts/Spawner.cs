using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public GameObject objectToSpawn;
	public float timeToSpawn;
	public bool spawnGrid;
	public float gridSize;
	private float maxTimeToSpawn;
	private float currentX, currentY;

	// Use this for initialization
	void Start () {
		maxTimeToSpawn = timeToSpawn;
		if (spawnGrid) {
			currentX = objectToSpawn.GetComponent<BoxCollider>().size.x/1.5f;
			currentY = objectToSpawn.GetComponent<BoxCollider>().size.y/2f;
			for(int i = 0; i < gridSize; i++){//columns
				for(int j = 0; j < gridSize; j++){//rows
					GameObject newObject = Instantiate (objectToSpawn, new Vector3(transform.localPosition.x+currentX*i,transform.localPosition.y+currentY*j,transform.position.z), transform.rotation) as GameObject;
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!spawnGrid) {
			if (timeToSpawn > 0) {
				timeToSpawn -= Time.deltaTime;
			} else {
				timeToSpawn = maxTimeToSpawn;
				GameObject newObject = Instantiate (objectToSpawn, transform.position, transform.rotation) as GameObject;
			}
		}
	}
}
