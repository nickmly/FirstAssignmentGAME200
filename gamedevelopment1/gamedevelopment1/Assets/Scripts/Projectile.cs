using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	[SerializeField] private float destroyTime;
	[SerializeField] private bool isGrenade;
	[SerializeField] private GameObject explosionPrefab;
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
			if(isGrenade){
				GameObject newExplosion = Instantiate(explosionPrefab,transform.position,transform.rotation) as GameObject;
				newExplosion.GetComponent<Explosion>().damage = damage;
			}
			Destroy(gameObject);
		}
	}

	void OnCollisionEnter(Collision col){
		if (col.gameObject.tag != "Enemy" && col.gameObject.tag != "Bullet" && !isGrenade) {
			//GameObject newExplosion = Instantiate(explosionPrefab,transform.position,transform.rotation) as GameObject;
			//newExplosion.GetComponent<Explosion>().damage = damage;
			Destroy (gameObject);
		}
	}
}
