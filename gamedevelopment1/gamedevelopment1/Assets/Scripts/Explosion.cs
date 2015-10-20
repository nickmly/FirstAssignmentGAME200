using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

	private ParticleSystem particle;
	[SerializeField] private float radius;
	public float damage;

	void Start () {
		transform.Rotate(new Vector3(285f,350f,355f));
		particle = GetComponent<ParticleSystem>();
		particle.Play();
		Invoke ("Delete",particle.startLifetime);
	}
	
	void OnDrawGizmos(){
		
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position,radius);
	}
	
	void FixedUpdate(){
		Collider[] enemiesHit = Physics.OverlapSphere(transform.position,radius,1<<15);
		if(enemiesHit.Length > 0){
			for(int i = 0; i < enemiesHit.Length; i++){
				enemiesHit[i].GetComponent<Rigidbody>().AddExplosionForce(500f,transform.position,radius);
				if(enemiesHit[i].GetComponent<Enemy>() != null){
					enemiesHit[i].GetComponent<Enemy>().Death();
				}
				if(enemiesHit[i].GetComponent<DestructableObject>() != null){
					enemiesHit[i].GetComponent<DestructableObject>().Destruct();
				}
			}
		}
	}
	
	void Delete(){
		Destroy(gameObject);
	}
}
