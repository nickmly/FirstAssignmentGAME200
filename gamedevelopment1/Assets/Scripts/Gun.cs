using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	[SerializeField] private float fireRate;
	[SerializeField] private float reloadTime;
	[SerializeField] private Vector3 stillPos;
	[SerializeField] private Vector3 sightPos;
	[SerializeField] private Vector3 stillRotation;
	[SerializeField] private float recoilAmount;
	[SerializeField] private float damage;
	[SerializeField] private bool shotgun;
	[SerializeField] private float lerpSpeed;

	private float sprintTimer = 0.333f;
	private float maxFireRate;
	private float maxReloadTime;
	private float bulletSpeed = 200f;
	private float flashDuration = 0.015f;
	private float maxFlashDuration = 0.015f;
	private Vector3 fakeMousePos;
	private bool canShoot, pumping;
	private Animator anim;
	private Vector3 recoilPos, currentPos;
	
	private bool reloading;
	private bool isADS;

	public Rigidbody bulletPrefab, shellPrefab;
	public Transform bulletExitPoint, shellExitPoint;
	public SpriteRenderer flash;

	public int ammo;
	private int maxAmmo;


#region CROSSHAIR
	public Texture2D crosshair;
	private float centerX = Screen.width / 2;
	private float centerY = Screen.height / 2;
	private float cursorW = 32;
	private float cursorH = 32;
#endregion


	void OnGUI(){
		if (!isADS) {
			GUI.DrawTexture (new Rect (centerX - cursorW / 2, centerY - cursorH / 2, cursorW, cursorH), crosshair);
		}
	}

	// Use this for initialization
	void Start () {
		canShoot = true; // We can shoot
		reloading = false; // Not reloading
		pumping = false; // Not pumping
		isADS = false; // Not aiming
		maxAmmo = ammo; //Set up our max variables for resetting them later
		maxFireRate = fireRate;//////
		maxReloadTime = reloadTime;//////
		fakeMousePos = new Vector3 (Screen.width / 2, Screen.height / 2, Input.mousePosition.z); // Fake mouse pos used is placed in center of screen
		anim = GetComponent<Animator> ();
		transform.localPosition = stillPos;
		currentPos = transform.localPosition;
		transform.localRotation = new Quaternion (stillRotation.x, stillRotation.y, stillRotation.z, transform.localRotation.w);
		//stillPos = new Vector3 (transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
		recoilPos = new Vector3 (stillPos.x, stillPos.y, stillPos.z - 0.2f);
		ResetGun ();
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = Vector3.Lerp (transform.localPosition, currentPos, lerpSpeed);

		if (Input.GetKey (KeyCode.LeftShift)) {
			canShoot = false;
			anim.applyRootMotion = false;
			anim.SetBool ("sprinting",true);
		}

		if (Input.GetKeyUp(KeyCode.LeftShift)) {
			canShoot = true;
			anim.SetBool ("sprinting",false);
		}

		if (Input.GetKeyDown (KeyCode.Mouse1)) {
			currentPos = sightPos;
			isADS = true;
		}

		if (Input.GetKeyUp (KeyCode.Mouse1)) {
			currentPos = stillPos;
			isADS = false;
		}

		if (Input.GetKey (KeyCode.Mouse0) && canShoot && ammo > 0) { //Fire the gun with left mouse button
			FireGun();
		}

		if ((Input.GetKeyDown (KeyCode.R) || ammo <= 0) && !reloading) { // Reload gun with R
			ReloadGun();
		}

		if (reloading) {
			PerformReloadAnim ();
		} else {
			if (!canShoot) {
				PrepareGunForFiring ();
			}
		}
	}

	private void FireGun(){
		ammo--;
		recoilPos = new Vector3 (transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - recoilAmount);
		currentPos = recoilPos;
		flash.enabled = true;
		flash.gameObject.GetComponent<Light> ().enabled = true;
		flashDuration = maxFlashDuration;
		canShoot = false;
		Ray mousePos = Camera.main.ScreenPointToRay (fakeMousePos); // Point at center of screen
		Rigidbody newBullet = Instantiate (bulletPrefab, bulletExitPoint.transform.position, transform.rotation) as Rigidbody; // Create bullet at bullet exit point
		newBullet.transform.LookAt (mousePos.GetPoint (350f)); // Aim bullet at center of screen
		newBullet.velocity = transform.parent.forward * bulletSpeed; // Move bullet towards center with speed of bulletSpeed
		newBullet.GetComponent<Projectile> ().damage = damage;
//		if(shotgun){
//			Rigidbody newBullet1 = Instantiate (bulletPrefab, bulletExitPoint.transform.position, transform.rotation) as Rigidbody; // Create bullet at bullet exit point
//			newBullet1.transform.LookAt (mousePos.GetPoint (700f)); // Aim bullet at center of screen
//			newBullet1.velocity = transform.parent.forward * bulletSpeed; // Move bullet towards center with speed of bulletSpeed
//			newBullet1.GetComponent<Projectile> ().damage = damage;
//
//			Rigidbody newBullet2 = Instantiate (bulletPrefab, bulletExitPoint.transform.position, transform.rotation) as Rigidbody; // Create bullet at bullet exit point
//			newBullet2.transform.LookAt (mousePos.GetPoint (125f)); // Aim bullet at center of screen
//			newBullet2.velocity = transform.parent.forward * bulletSpeed; // Move bullet towards center with speed of bulletSpeed
//			newBullet2.GetComponent<Projectile> ().damage = damage;
//		}//TODO: fix rotation on these bullets

		if (!shotgun) {
			CreateShell (mousePos);
		}

	}


	private void PumpGun(){
		canShoot = true;
		fireRate = maxFireRate;
		anim.SetBool ("pumping", false);
		anim.applyRootMotion = true;
	}

	private void CreateShell(){
		Ray mousePos = Camera.main.ScreenPointToRay (fakeMousePos); // Point at center of screen
		CreateShell (mousePos);
	}

	private void CreateShell(Ray mousePos){
		Rigidbody newShell = Instantiate (shellPrefab, shellExitPoint.transform.position, shellExitPoint.transform.rotation) as Rigidbody; // Create shell at shell exit point
		//newShell.AddForce (40f, 10f, 0f, ForceMode.Impulse);
		newShell.transform.GetChild(0).Rotate(new Vector3(Random.Range(0,360),0,Random.Range(0,360)));
		newShell.velocity = transform.parent.right * 10f;
		newShell.transform.LookAt (mousePos.GetPoint (30f));
	}

	private void ReloadGun(){
		if(isADS){
			currentPos = sightPos;
		} else{ 
			currentPos = stillPos;
		}
		flash.enabled = false;
		flash.gameObject.GetComponent<Light> ().enabled = false;
		anim.applyRootMotion = false;
		canShoot = false;
		anim.SetBool ("reloading", true);
		reloading = true;
		ammo = 0;
	}

	private void ResetGun(){
		anim.applyRootMotion = true;
		if (!isADS) {
			currentPos = stillPos;
		} else {
			currentPos = sightPos;
		}

		transform.localRotation = new Quaternion (stillRotation.x, stillRotation.y, stillRotation.z, transform.localRotation.w);

	}

	private void PrepareGunForFiring(){
		if (flashDuration > 0) {
			flashDuration -= Time.deltaTime;
		} else {
			if(isADS){
				currentPos = sightPos;
			} else{ 
				currentPos = stillPos;
			}
			flashDuration = maxFlashDuration;
			flash.enabled = false;
			flash.gameObject.GetComponent<Light> ().enabled = false;
		}
		if (fireRate > 0) {
			fireRate -= Time.deltaTime;
		} else {
			if(!shotgun){
				canShoot = true;
			} else {
				pumping = true;
				anim.SetBool ("pumping", true);
				anim.applyRootMotion = false;
			}

			fireRate = maxFireRate;
			//anim.SetBool ("shooting", false);
		}
	}

	private void PerformReloadAnim(){
		if(reloadTime > 0){
			reloadTime -= Time.deltaTime;
		} else {
			ammo = maxAmmo;
			canShoot = true;
			reloading = false;
			reloadTime = maxReloadTime;
			anim.SetBool("reloading",false);
			anim.applyRootMotion = true;
			transform.localRotation = new Quaternion (stillRotation.x, stillRotation.y, stillRotation.z, transform.localRotation.w);
		}
	}
}
