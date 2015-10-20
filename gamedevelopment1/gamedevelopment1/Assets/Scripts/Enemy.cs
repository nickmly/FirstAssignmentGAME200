using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	public enum State
	{
		Idle,
		Chasing,
		Looking,
		Attacking,
	}
	public State currentState;
	[SerializeField] private Transform player;
	public bool alive = true;
	public Vector3 target;	
	
	private int direction = 1;
	private BoxCollider collider;
	private Rigidbody rb;
	private NavMeshAgent agent;	
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

	// Use this for initialization
	void Start () {
		//target = GameObject.Find ("FPSController"); //temporary
		maxLookTimer = lookTimer;
		maxPatrolTimer = patrolTimer;
		maxVaultTimer = vaultTimer;
		currentState = State.Looking;
		agent = GetComponent<NavMeshAgent>();
		rb = GetComponent<Rigidbody>();
		collider = GetComponent<BoxCollider>();
	}
	
	void Patrol(){
		if(patrolTimer > 0){
			patrolTimer -= Time.deltaTime;
			Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
			transform.LookAt (targetPos);
			if (Vector3.Distance (target, transform.position) > 1) {
				//agent.SetDestination(
				transform.position += (transform.forward * direction) * moveSpeed;
			} else {
				patrolTimer = 0;
			}
		} else {
			patrolTimer = maxPatrolTimer;
			Vector3 randomPos = new Vector3(transform.position.x + Random.Range(-patrolRadius,+patrolRadius),transform.position.y,transform.position.z + Random.Range(-patrolRadius,+patrolRadius));
			target = randomPos;
			if(Physics.CheckSphere(transform.position,visionRadius,1<<11)){
				target = player.position;
				agent.enabled = true;
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
			//Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
			//transform.LookAt (targetPos);
			if (Vector3.Distance (target, transform.position) > 20) {
				if(!isVaulting){
					agent.SetDestination(target);
				}
				//agent.SetDestination(targetPos);
				//transform.position += transform.forward * moveSpeed;
			} else {
				currentState = State.Attacking;
				agent.enabled = false;
				rb.velocity = Vector3.zero;
			}
		} else {
			currentState = State.Looking;
			agent.enabled = false;
			rb.velocity = Vector3.zero;
		}
	}
	
	void Attack(){
		if(Physics.CheckSphere(transform.position,patrolRadius,1<<11)){
			if(lookTimer > 0){
				lookTimer -= Time.deltaTime;
			} else {
				lookTimer = Random.Range(0,maxLookTimer);
				target = player.position;
				Vector3 targetPos = new Vector3(target.x,transform.position.y,target.z);
				transform.LookAt (targetPos);
			}
		} else {
			target = player.position;
			agent.enabled = true;
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
	
	private bool CanVault(){
		int layerMask = 1 << 8;
		Collider[] collisions = Physics.OverlapSphere (new Vector3 (collider.transform.position.x,
		                                                            collider.transform.position.y - 
		                                                            (collider.bounds.extents.y+collider.bounds.size.y/8),
		                                                            collider.transform.position.z), 2f,layerMask);
		if (collisions.Length > 0) {
			return true;
		} else {
			return false;
		}
	}
	
	private void Vault(){
		rb.velocity = Vector3.zero;
		if (vaultTimer > 0) {
			vaultTimer -= Time.deltaTime;
		} else {
			isVaulting = false;
			vaultTimer = maxVaultTimer;
			if(!IsColliding("forward",8)){			
				rb.AddForce(transform.forward * 350f/2);				
			}
		}
	}
	

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
			
//			if (IsColliding("forward",8)) {
//				if(CanVault()){
//					if(currentState == State.Chasing){
//						agent.enabled = false;
//						currentState = State.Looking;
//					}
//					rb.AddForce(new Vector3(0,350f,0));
//					isVaulting = true;
//				}
//			}
//			if (isVaulting) {
//				Vault ();
//			}

			if (health <= 0) {
				Death ();
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
		agent.enabled = false;
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
