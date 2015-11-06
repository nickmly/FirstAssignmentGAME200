using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	public enum State
	{
		Idle,
		Chasing,
		Looking,
		Attacking,
		Dead
	}
	public State currentState;
	[SerializeField] private Transform player;
	public bool alive = true;
	public Vector3 target;	
	
	private int direction = 1;
	private BoxCollider collider;
	private Rigidbody rb;
	private float moveSpeed = 0.1f;	
	private float health = 100f;
	private float patrolRadius = 15f;
	private float visionRadius = 30f;
	private float patrolTimer = 5f;
	private float maxPatrolTimer;
	private float lookTimer = 0.4f;
	private float maxLookTimer;
	private float vaultTimer = 0.3f;
	private float maxVaultTimer;
	private bool isVaulting = false;
	private Quaternion currentRot;
	// Use this for initialization
	void Start () {
		maxLookTimer = lookTimer;
		maxPatrolTimer = patrolTimer;
		maxVaultTimer = vaultTimer;
		currentState = State.Looking;
		rb = GetComponent<Rigidbody>();
		collider = GetComponent<BoxCollider>();
	}
	
	void Patrol(){
		transform.rotation = Quaternion.Slerp (transform.rotation, currentRot, 0.5f);
		if(patrolTimer > 0){
			patrolTimer -= Time.deltaTime;
			Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
			currentRot = Quaternion.LookRotation(targetPos - transform.position);
			if (Vector3.Distance (target, transform.position) > 1) {
				transform.position += (transform.forward * direction) * moveSpeed;
			} else {
				patrolTimer = 0;
			}
		} else {
			patrolTimer = maxPatrolTimer;
			Vector3 randomPos = new Vector3(transform.position.x + Random.Range(-patrolRadius,+patrolRadius),transform.position.y,transform.position.z + Random.Range(-patrolRadius,+patrolRadius));
			target = randomPos;
			if(Physics.CheckSphere(transform.position,visionRadius,1<<11)){
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
		transform.rotation = Quaternion.Slerp (transform.rotation, currentRot, 0.5f);
		if(Physics.CheckSphere(transform.position,visionRadius,1<<11)){
			Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
			currentRot = Quaternion.LookRotation(targetPos - transform.position);
			if (Vector3.Distance (target, transform.position) > 20) {
				transform.position += transform.forward * moveSpeed;
			} else {
				currentState = State.Attacking;
				rb.velocity = Vector3.zero;
			}
		} else {
			currentState = State.Looking;
			rb.velocity = Vector3.zero;
		}
	}
	
	void Attack(){
		transform.rotation = Quaternion.Slerp (transform.rotation, currentRot, 0.5f);
		if(Physics.CheckSphere(transform.position,patrolRadius,1<<11)){
			if(lookTimer > 0){
				lookTimer -= Time.deltaTime;
			} else {
				lookTimer = Random.Range(0,maxLookTimer);
				target = player.position;
				Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
				currentRot = Quaternion.LookRotation(targetPos - transform.position);
			}
		} else {
			target = player.position;
			currentState = State.Chasing;
		}
	
	}
	
	private bool IsColliding(string side, int layerID){
		Vector3 fwd = transform.TransformDirection (Vector3.forward);
		Vector3 bkd = transform.TransformDirection (-Vector3.forward);
		
		Vector3 left = transform.TransformDirection (-Vector3.right);
		Vector3 right = transform.TransformDirection (Vector3.right);
		
		float dist = 1.5f;
		
		switch (side) {
		case "forward":
			if(Physics.Raycast(collider.transform.position,fwd,dist,1 << layerID)){
				return Physics.Raycast(collider.transform.position,fwd,dist,1 << layerID);
			} else {
				return Physics.Raycast(new Vector3(collider.transform.position.x,collider.transform.position.y-collider.bounds.extents.y/2,collider.transform.position.z),fwd,dist,1 << layerID);
			}				
			break;
		case "backward":
			return Physics.Raycast(collider.transform.position,bkd,dist,1 << layerID);
			break;
		case "left":
			return Physics.Raycast(collider.transform.position,left,dist,1 << layerID);
			break;
		case "right":
			return Physics.Raycast(collider.transform.position,right,dist,1 << layerID);
			break;
		default:
			return false;
			break;
		}
	}
	
	void Update () {
		if (alive) {	
			switch (currentState) {
			case State.Looking:
				Patrol ();
				break;
			case State.Chasing:
				Chase ();
				break;
				
			case State.Attacking:
				Attack ();
				break;
				
			}

			if (health <= 0) {
				Death ();
			}
		} else {
			if(currentState != State.Dead){
				currentState = State.Dead;
			}
		}
	}

	public void Hit(float dmg){
		if (health > 0) {
			health -= dmg;
		} else {
			Death ();
		}
	}
	
	public void Death(){
		rb.freezeRotation = false;
		alive = false;
	}

	void OnCollisionEnter(Collision col){
		switch (col.gameObject.tag) {
			case "Bullet":
				Hit(col.gameObject.GetComponent<Projectile>().damage);
				Destroy (col.gameObject);
			break;
			case "Wall":
				direction *= -1;
			break;
		}
	}
}
