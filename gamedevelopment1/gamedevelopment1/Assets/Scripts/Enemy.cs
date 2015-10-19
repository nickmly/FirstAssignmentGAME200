using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	[SerializeField] private Vector3 target;
	[SerializeField] private Transform player;
	
	public enum State
	{
		Idle,
		Chasing,
		Looking,
		Attacking,
	}
	
	public State currentState;
	private float moveSpeed = 0.1f;
	public bool alive = true;
	private float health = 100f;
	private float patrolRadius = 15f;
	private float visionRadius = 30f;
	private float patrolTimer = 5f;
	private float maxPatrolTimer;
	private float lookTimer = 0.4f;
	private float maxLookTimer;

	// Use this for initialization
	void Start () {
		//target = GameObject.Find ("FPSController"); //temporary
		maxLookTimer = lookTimer;
		maxPatrolTimer = patrolTimer;
		currentState = State.Looking;
	}
	
	void Patrol(){
		if(patrolTimer > 0){
			patrolTimer -= Time.deltaTime;
			Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
			transform.LookAt (targetPos);
			if (Vector3.Distance (target, transform.position) > 1) {
				transform.position += transform.forward * moveSpeed;
			} else {
				patrolTimer = 0;
			}
		} else {
			patrolTimer = maxPatrolTimer;
			Vector3 randomPos = new Vector3(transform.position.x + Random.Range(-patrolRadius,+patrolRadius),transform.position.y,transform.position.z + Random.Range(-patrolRadius,+patrolRadius));
			target = randomPos;
			if(Physics.CheckSphere(transform.position,visionRadius,1<<11)){
				target = player.position;
				currentState = State.Chasing;
				return;
			}
		}
	}
	
	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position,visionRadius);
	}
	
	void Chase(){
		if(Physics.CheckSphere(transform.position,visionRadius,1<<11)){
			target = player.position;
			Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
			transform.LookAt (targetPos);
			if (Vector3.Distance (target, transform.position) > 10) {
				transform.position += transform.forward * moveSpeed;
			} else {
				currentState = State.Attacking;
			}
		} else {
			currentState = State.Looking;
		}
	}
	
	void Attack(){
		if(Physics.CheckSphere(transform.position,visionRadius,1<<11)){
			if(lookTimer > 0){
				lookTimer -= Time.deltaTime;
			} else {
				lookTimer = Random.Range(0,maxLookTimer);
				target = player.position;
				Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
				transform.LookAt (targetPos);
			}
		} else {
			currentState = State.Looking;
		}
	
	}
	
	// Update is called once per frame
	void Update () {
		if (alive) {	
			switch(currentState){
				case State.Looking:
					Patrol();
				break;
				case State.Chasing:
					Chase ();
				break;
				
				case State.Attacking:
					Attack();
				break;
				
			}

			if (health <= 0) {
				Death ();
			}
		}
	}

	void Hit(float dmg){
		if (health > 0) {
			health -= dmg;
		} else {
			Death ();
		}
	}

	void Death(){
		alive = false;
	}

	void OnCollisionEnter(Collision col){
		switch (col.gameObject.tag) {
			case "Bullet":
				Hit(col.gameObject.GetComponent<Projectile>().damage);
				Destroy (col.gameObject);
			break;
		}
	}
}
