using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	[SerializeField] private float speed;
	[SerializeField] private float jumpHeight;
	[SerializeField] private float sensitivity;
	[SerializeField] private Camera cam;
	[SerializeField] private GameObject primaryWeapon;
	[SerializeField] private GameObject secondaryWeapon;
	[SerializeField] private HUDManager hud;
	[SerializeField] private BoxCollider collider;

	private GameObject currentWeapon;
	private CharacterController cc;
	private float normalSpeed;
	private Rigidbody rb;
	private Vector3 xVelocity, zVelocity, velocity, rotation, cameraRotation;
	private float xRotationVel = 0.0f;
	private float yRotationVel = 0.0f;
	private float currentRotationX = 0.0f;
	private float currentRotationY = 0.0f;
	
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rotation = Vector3.zero;
		velocity = Vector3.zero;
		cameraRotation = Vector3.zero;
		normalSpeed = speed;
		cc = GetComponent<CharacterController> ();
		currentWeapon = GameObject.FindGameObjectWithTag ("Gun");
	}

	void Update () {
		HandleInput ();
		Move (velocity);
		Rotate (rotation);
		RotateCamera (cameraRotation);

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
			cam.transform.eulerAngles = clampedEuler;
			cam.transform.Rotate(-newRotation);
		}
	}

	private bool IsGrounded(){
		Collider[] collisions = Physics.OverlapSphere (new Vector3 (collider.transform.position.x,collider.transform.position.y - (collider.bounds.extents.y+collider.bounds.size.y/8), collider.transform.position.z), 0.1f);
		Debug.Log (collisions.Length);
		if (collisions.Length > 0) {
			return true;
		} else {
			return false;
		}
	}

#region DEBUGGING
	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(new Vector3 (collider.transform.position.x,collider.transform.position.y - (collider.bounds.extents.y+collider.bounds.size.y/48), collider.transform.position.z), 0.1f);
	}
#endregion

	private void HandleInput(){
		float x = Input.GetAxis ("Horizontal");
		float z = Input.GetAxis ("Vertical");

		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			speed *= 1.5f;
		}
		if (Input.GetKeyUp (KeyCode.LeftShift)) {
			speed = normalSpeed;
		}

		if (Input.GetKeyDown (KeyCode.Space) && IsGrounded()) {
			rb.AddForce(new Vector3(0,jumpHeight,0));
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
			currentWeapon.transform.parent = transform.GetChild(0);
			hud.gun = currentWeapon.GetComponent<Gun>();
		}
		
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			Destroy (currentWeapon);
			currentWeapon = Instantiate(secondaryWeapon,transform.position,transform.rotation) as GameObject;
			currentWeapon.transform.parent = transform.GetChild(0);
			hud.gun = currentWeapon.GetComponent<Gun>();
		}
	}
}
