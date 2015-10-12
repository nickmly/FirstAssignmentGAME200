using UnityEngine;
using System.Collections;


//TODO: bug in jump not always working related to rigidbody stuck in wall; make function that prevents this
public class Player : MonoBehaviour {

	[SerializeField] private float speed;
	[SerializeField] private float jumpHeight;
	[SerializeField] private float sensitivity;
	[SerializeField] private Camera cam;
	[SerializeField] private GameObject primaryWeapon;
	[SerializeField] private GameObject secondaryWeapon;
	[SerializeField] private HUDManager hud;
	[SerializeField] private BoxCollider collider;
	[SerializeField] private Gun gun;

	public bool isRunning = false;

	private GameObject currentWeapon;
	private Animator gunAnim;
	private CharacterController cc;
	private float normalSpeed;
	private Rigidbody rb;
	private Vector3 xVelocity, zVelocity, velocity, rotation, cameraRotation;
	private float xRotationVel = 0.0f;
	private float yRotationVel = 0.0f;
	private float currentRotationX = 0.0f;
	private float currentRotationY = 0.0f;
	private float vaultTimer,maxVaultTimer;
	private bool isVaulting = false;
	private bool hasJumped = false;
	
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rotation = Vector3.zero;
		velocity = Vector3.zero;
		cameraRotation = Vector3.zero;
		normalSpeed = speed;
		cc = GetComponent<CharacterController> ();
		currentWeapon = GameObject.FindGameObjectWithTag ("Gun");
		vaultTimer = 0.3f;
		maxVaultTimer = vaultTimer;
		gun = currentWeapon.GetComponent<Gun> ();
		hud.gun = gun;
		gunAnim = gun.GetComponent<Animator> ();
	}

	void FixedUpdate(){
		HandleInput ();

		if (hasJumped) {
			if(CanVault()){
				isVaulting = true;
				gunAnim.applyRootMotion = false;
				gunAnim.SetBool("vaulting",true);
			}
		}
		if (isVaulting) {
			Vault ();
		}
	
		Move (velocity);
	}

	void Update () {

		Rotate (rotation);
		RotateCamera (cameraRotation);
		if (IsGrounded ()) {
			hasJumped = false;
		}
	}

	private void GetNewGun(){
		currentWeapon.transform.parent = transform.GetChild(0);
		hud.gun = currentWeapon.GetComponent<Gun>();
		gun = currentWeapon.GetComponent<Gun> ();
		gunAnim = gun.GetComponent<Animator> ();
	}

	private void Move(Vector3 newVelocity){
		rb.MovePosition (rb.position + newVelocity * Time.fixedDeltaTime);
	}

	private void Rotate(Vector3 newRotation){
		//rb.MoveRotation (rb.rotation * Quaternion.Euler(newRotation));//Rotates around newRotation by converting it to a quaternion using euler
		transform.Rotate(newRotation);
	}

	private void RotateCamera(Vector3 newRotation){
		if (cam != null) {
			Vector3 clampedEuler = cam.transform.eulerAngles;
			clampedEuler.x = Mathf.Clamp (clampedEuler.x,-90,90);
			//cam.transform.eulerAngles = clampedEuler;
			cam.transform.Rotate(-newRotation);
		}
	}

	private bool IsGrounded(){
		Collider[] collisions = Physics.OverlapSphere (new Vector3 (collider.transform.position.x,collider.transform.position.y - (collider.bounds.extents.y+collider.bounds.size.y/8), collider.transform.position.z), 0.1f);
		if (collisions.Length > 0) {
			return true;
		} else {
			return false;
		}
	}

	private bool CanVault(){
		int layerMask = 1 << 8;
		Collider[] collisions = Physics.OverlapSphere (new Vector3 (collider.transform.position.x,collider.transform.position.y - (collider.bounds.extents.y+collider.bounds.size.y/8), collider.transform.position.z), 2f,layerMask);
		if (collisions.Length > 0) {
			return true;
		} else {
			return false;
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
				return Physics.Raycast(collider.transform.position,fwd,dist,1 << layerID);
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

	private void Vault(){
		zVelocity = Vector3.zero;
		if (vaultTimer > 0) {
			vaultTimer -= Time.deltaTime;
		} else {
			gunAnim.SetBool("vaulting",false);
			isVaulting = false;
			vaultTimer = maxVaultTimer;
			if(!isRunning){
				rb.AddForce(transform.forward * jumpHeight/2);
			} else {
				rb.AddForce(transform.forward * jumpHeight/2);
			}

			//velocity = transform.forward * 2.5f;
		}
	}

#region DEBUGGING
	void OnDrawGizmos(){


		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(new Vector3 (collider.transform.position.x,collider.transform.position.y - (collider.bounds.extents.y+collider.bounds.size.y/8), collider.transform.position.z), 0.1f);
		//Gizmos.DrawWireSphere(transform.forward * 2.5f, 1f);
	}
#endregion

	private void HandleInput(){
		float x = 0;
		float z = 0;
		if (Input.GetKey (KeyCode.W) && !IsColliding("forward",8) && !isVaulting) {
			z = 1;
		}
		if (Input.GetKey (KeyCode.S) && !IsColliding("backward",8)) {
			z = -1;
		}
		if (Input.GetKey (KeyCode.D) && !IsColliding("right",8)) {
			x = 1;
		}
		if (Input.GetKey (KeyCode.A) && !IsColliding("left",8)) {
			x = -1;
		}


		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			speed = normalSpeed * 1.5f;
			isRunning = true;
		}
		if (Input.GetKeyUp (KeyCode.LeftShift)) {
			speed = normalSpeed;
			isRunning = false;
		}

		if (Input.GetKeyDown (KeyCode.Space) && IsGrounded ()) {
			if (!isVaulting && !hasJumped) {
				rb.AddForce (new Vector3 (0, jumpHeight, 0));
				hasJumped = true;
			}
		}

		xVelocity = transform.right * x;
		zVelocity = transform.forward * z;

		velocity = (xVelocity + zVelocity).normalized * speed;// Combine the vectors and normalize them, multiply by speed

		float mouseY = Input.GetAxisRaw ("Mouse X") * sensitivity; //These lines calculate the turning rotation of the player on the x-axis
		//mouseY = Mathf.SmoothDamp (currentRotationX, mouseY, ref xRotationVel, 0.01f);
		rotation = new Vector3 (0, mouseY, 0);


		float mouseX = Input.GetAxis ("Mouse Y") * sensitivity; //These lines calculate the turning rotation of the camera on the y-axis
		//mouseX = Mathf.SmoothDamp (currentRotationY, mouseX, ref yRotationVel, 0.01f);
		cameraRotation = new Vector3 (mouseX, 0, 0);


		//INVENTORY MANAGEMENT
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			Destroy (currentWeapon);
			currentWeapon = Instantiate(primaryWeapon,transform.position,transform.rotation) as GameObject;
			GetNewGun();
		}
		
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			Destroy (currentWeapon);
			currentWeapon = Instantiate(secondaryWeapon,transform.position,transform.rotation) as GameObject;
			GetNewGun();
		}
	}
}
