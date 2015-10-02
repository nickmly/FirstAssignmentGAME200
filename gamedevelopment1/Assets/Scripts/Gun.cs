using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {

	[SerializeField] private float fireRate;
	[SerializeField] private float maxFireRate;
	[SerializeField] private float reloadTime;
	[SerializeField] private float maxReloadTime;
	[SerializeField] private Vector3 sightPos;

	private float bulletSpeed = 75f;
	private float flashDuration = 0.015f;
	private float maxFlashDuration = 0.015f;
	private Vector3 fakeMousePos;
	private bool canShoot;
	private Animator anim;
	private Vector3 stillPos, recoilPos;

	private bool reloading;

	public Rigidbody bulletPrefab;
	public Transform bulletExitPoint, shellExitPoint;
	public SpriteRenderer flash;



	// Use this for initialization
	void Start () {
		canShoot = true; // We can shoot
		reloading = false; // Not reloading
		fakeMousePos = new Vector3 (Screen.width / 2, Screen.height / 2, Input.mousePosition.z); // Fake mouse pos used is placed in center of screen
		anim = GetComponent<Animator> ();
		stillPos = new Vector3 (transform.position.x, transform.position.y, transform.position.z);
		recoilPos = new Vector3 (stillPos.x, stillPos.y, stillPos.z + 0.2f);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Mouse1)) {
			transform.position = sightPos;
		}

		if (Input.GetKeyUp (KeyCode.Mouse1)) {
			transform.position = stillPos;
		}

		if (Input.GetKey (KeyCode.Mouse0) && canShoot) { //Fire the gun with left mouse button
			FireGun();
		}

		if (Input.GetKeyDown (KeyCode.R) && !reloading) { // Reload gun with R
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

		//transform.position = recoilPos;
		flash.enabled = true;
		flashDuration = maxFlashDuration;
		anim.SetBool ("shooting", true);
		canShoot = false;
		Ray mousePos = Camera.main.ScreenPointToRay (fakeMousePos); // Point at center of screen
		Rigidbody newBullet = Instantiate (bulletPrefab, bulletExitPoint.transform.position, new Quaternion(0,0,0,0)) as Rigidbody; // Create bullet at bullet exit point
		newBullet.transform.LookAt (mousePos.GetPoint (500f)); // Aim bullet at center of screen
		newBullet.velocity = transform.forward * bulletSpeed; // Move bullet towards center with speed of bulletSpeed
		Quaternion newRot = newBullet.transform.rotation;
		newRot.eulerAngles = new Vector3 (0, 90, 0);
		newBullet.rotation = newRot;
		//CreateShell ();
	}

	private void CreateShell(){
		Rigidbody newShell = Instantiate (bulletPrefab, shellExitPoint.transform.position, shellExitPoint.transform.rotation) as Rigidbody; // Create shell at shell exit point
		newShell.AddForce (20, 0, -20f, ForceMode.Impulse);
	}

	private void ReloadGun(){
		if (anim.GetBool ("shooting")) {
			anim.SetBool ("shooting", false);
		}
		canShoot = false;
		anim.SetBool ("reloading", true);
		reloading = true;
	}

	private void PrepareGunForFiring(){
		if (flashDuration > 0) {
			flashDuration -= Time.deltaTime;
		} else {
			//transform.position = stillPos;
			flashDuration = maxFlashDuration;
			flash.enabled = false;
		}
		if (fireRate > 0) {
			fireRate -= Time.deltaTime;
		} else {
			canShoot = true;
			fireRate = maxFireRate;
			anim.SetBool ("shooting", false);
		}
	}

	private void PerformReloadAnim(){
		if(reloadTime > 0){
			reloadTime -= Time.deltaTime;
		} else {
			canShoot = true;
			reloading = false;
			reloadTime = maxReloadTime;
			anim.SetBool("reloading",false);
		}
	}
}
