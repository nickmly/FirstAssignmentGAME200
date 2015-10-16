using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	[SerializeField] private float fireRate;
	[SerializeField] private float reloadTime;
	[SerializeField] private Vector3 stillPos;
	[SerializeField] private Vector3 sightPos;
	[SerializeField] private Vector3 stillRotation;
	[SerializeField] private float recoilAmount, walkIntensity;
	[SerializeField] private float damage;
	[SerializeField] private bool shotgun;
	[SerializeField] private float lerpSpeed;
	[SerializeField] private bool hasScope = false;


	public int mags = 5;
	public string gunType;
	private float sprintTimer = 0.333f;
	private float maxFireRate;
	private float maxReloadTime;
	private float walkTimer = 0.2f;
	private float maxWalkTimer;
	private int walkDecision = 0;
	private float normalRecoilAmount;
	private float bulletSpeed = 100f;
	private float flashDuration = 0.015f;
	private float maxFlashDuration = 0.015f;
	private Vector3 fakeMousePos;
	private bool canShoot, pumping;
	private Animator anim;
	private Vector3 recoilPos, currentPos;
	private float swayX, swayY;
	
	private bool reloading;
	private bool isADS;

	public Rigidbody bulletPrefab, shellPrefab;
	public Transform bulletExitPoint, shellExitPoint;
	public SpriteRenderer flash;
	public Player player;

	public int ammo;
	public int maxAmmo;


#region CROSSHAIR
	public Texture2D crosshair, sight;
	private float centerX = Screen.width / 2;
	private float centerY = Screen.height / 2;
	private float cursorW = 32;
	private float cursorH = 32;
#endregion


	void OnGUI(){
		if (!isADS) {
			GUI.DrawTexture (new Rect (centerX - cursorW / 2, centerY - cursorH / 2, cursorW, cursorH), crosshair);
		} else if (isADS && hasScope) {
			GUI.DrawTexture (new Rect (centerX - cursorW / 2, centerY - cursorH / 2, cursorW, cursorH), sight);
		}
	}

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		canShoot = true; // We can shoot
		reloading = false; // Not reloading
		pumping = false; // Not pumping
		isADS = false; // Not aiming
		maxAmmo = ammo; //Set up our max variables for resetting them later
		maxFireRate = fireRate;//////////
		maxReloadTime = reloadTime;//////
		maxWalkTimer = walkTimer;////////
		normalRecoilAmount = recoilAmount;//////
		fakeMousePos = new Vector3 (Screen.width / 2, Screen.height / 2, Input.mousePosition.z); // Fake mouse pos used is placed in center of screen
		anim = GetComponent<Animator> ();
		transform.localPosition = new Vector3 (stillPos.x, stillPos.y - 2, stillPos.z);
		currentPos = transform.localPosition;
		transform.localRotation = new Quaternion (stillRotation.x, stillRotation.y, stillRotation.z, transform.localRotation.w);
		//stillPos = new Vector3 (transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
		recoilPos = new Vector3 (stillPos.x, stillPos.y, stillPos.z - 0.2f);
		ResetGun ();
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = Vector3.Lerp (transform.localPosition, currentPos, lerpSpeed);

		float mouseY = Input.GetAxis ("Mouse X") * player.sensitivity;
		float mouseX = Input.GetAxis ("Mouse Y") * player.sensitivity;

		swayX = Mathf.SmoothDamp (swayX, mouseX, ref player.velocity.x, 2f * Time.deltaTime);
		swayY = Mathf.SmoothDamp (swayY, mouseY, ref player.velocity.y, 2f * Time.deltaTime);

		Quaternion rotValue = Quaternion.Euler (new Vector3 (swayX, swayY, 0));

		transform.localRotation = Quaternion.Lerp (transform.localRotation, rotValue, 2.5f * Time.deltaTime);
		if (player.isWalking) {
			if(walkTimer > 0){
				walkTimer -= Time.deltaTime;
			} else {
				walkTimer = maxWalkTimer;
				Walk ();
			}
		}
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
		
		if(isADS){
			recoilAmount = normalRecoilAmount/2;
		} else {
			recoilAmount = normalRecoilAmount;
		}
		
		if (Input.GetKey (KeyCode.Mouse0) && canShoot && ammo > 0 && mags >= 0) { //Fire the gun with left mouse button
			FireGun();
		}

		if ((Input.GetKeyDown (KeyCode.R) || ammo <= 0) && !reloading) { // Reload gun with R
			if(mags > 0){
				ReloadGun();
				mags--;
			}
		}

		if (reloading) {
			PerformReloadAnim ();
		} else {
			if (!canShoot) {
				PrepareGunForFiring ();
			}
		}
	}

	private void Walk(){//TODO: move this to player and affect the camera
		if (walkDecision == 0) {
			if(!isADS){
				currentPos = new Vector3(currentPos.x,stillPos.y-walkIntensity,currentPos.z);
			} else {
				currentPos = new Vector3(currentPos.x,sightPos.y-walkIntensity/2,currentPos.z);
			}
			walkDecision = 1;
		} else {
			if(!isADS){
				currentPos = new Vector3(currentPos.x,stillPos.y+walkIntensity,currentPos.z);
			} else {
				currentPos = new Vector3(currentPos.x,sightPos.y+walkIntensity/2,currentPos.z);
			}
			walkDecision = 0;
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

		if (shotgun) {
			Rigidbody newBullet1 = Instantiate (bulletPrefab, bulletExitPoint.transform.position, transform.rotation) as Rigidbody; // Create bullet at bullet exit point
			newBullet.transform.Rotate(new Vector3(0,15,0));
			newBullet1.transform.LookAt (mousePos.GetPoint (350f)); // Aim bullet at center of screen
			newBullet1.velocity = newBullet1.transform.forward * bulletSpeed; // Move bullet towards center with speed of bulletSpeed
			newBullet1.GetComponent<Projectile> ().damage = damage;

			Rigidbody newBullet2 = Instantiate (bulletPrefab, bulletExitPoint.transform.position, transform.rotation) as Rigidbody; // Create bullet at bullet exit point
			newBullet.transform.Rotate(new Vector3(0,-15,0));
			newBullet2.transform.LookAt (mousePos.GetPoint (350f)); // Aim bullet at center of screen
			newBullet2.velocity = newBullet2.transform.forward * bulletSpeed; // Move bullet towards center with speed of bulletSpeed
			newBullet2.GetComponent<Projectile> ().damage = damage;
		}

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

	public void ResetGun(){
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
				if(!anim.GetBool("sprinting") && !anim.GetBool("vaulting")){
					pumping = true;
					anim.SetBool ("pumping", true);
					anim.applyRootMotion = false;
				}
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
