using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	[SerializeField] private float destroyTime;
	public float damage;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (destroyTime > 0) {
			// TODO: replace this with object pooling
			destroyTime -= Time.deltaTime;
		} else {
			Destroy(gameObject);
		}
	}

	void OnCollisionEnter(Collision col){
		if (gameObject.tag != "Enemy") {
			Destroy (gameObject);
		}
	}
}
