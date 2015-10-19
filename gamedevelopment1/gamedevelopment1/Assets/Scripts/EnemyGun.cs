using UnityEngine;
using System.Collections;

public class EnemyGun : MonoBehaviour {

	[SerializeField] private float fireRate;
	[SerializeField] private float reloadTime;
	[SerializeField] private float recoilAmount;
	[SerializeField] private float damage;
	
	
	
	public GameObject bulletPrefab, shellPrefab;
	public Transform bulletExitPoint,shellExitPoint;
	public GameObject flash;
	public int ammo;
	private float maxFireRate, maxReloadTime;
	private int maxAmmo;
	
	private float lerpSpeed = 0.5f;
	private bool canShoot;
	private float bulletSpeed = 100f;
	private float flashDuration = 0.015f;
	private float maxFlashDuration;
	
	private Vector3 stillPos, currentPos, recoilPos;
	
	private Enemy enemy;

	void Start () {
		canShoot = true;
		maxFireRate = fireRate;
		maxReloadTime = reloadTime;
		maxAmmo = ammo;
		maxFlashDuration = flashDuration;
		stillPos = transform.localPosition;
		recoilPos = new Vector3(stillPos.x,stillPos.y,stillPos.z-recoilAmount);
		currentPos = stillPos;
		enemy = transform.parent.GetComponent<Enemy>();
	}
	
	void Update(){
		transform.localPosition = Vector3.Lerp (transform.localPosition, currentPos, lerpSpeed);
		
		if(enemy.currentState == Enemy.State.Attacking && enemy.alive){
			if(!canShoot){
				PrepareToFire();
			} else {
				if(ammo > 0){
					FireGun();
				} else {
					Reload();					
				}
			}
			
			
		}
	}
	
	void FireGun(){
		ammo--;
		currentPos = recoilPos;
		flash.SetActive(true);
		flashDuration = maxFlashDuration;
		canShoot = false;
		GameObject bullet = Instantiate (bulletPrefab, bulletExitPoint.transform.position, transform.rotation) as GameObject; // Create bullet at bullet exit point
		bullet.GetComponent<Rigidbody>().velocity = transform.parent.forward * 100f;
		bullet.GetComponent<Projectile> ().damage = damage;
		CreateShell();		
	}
	
	void CreateShell(){
		Rigidbody newShell = Instantiate (shellPrefab, shellExitPoint.transform.position, shellExitPoint.transform.rotation) as Rigidbody; // Create shell at shell exit point
		Vector3 random = new Vector3(0,Random.Range(0,360),Random.Range(0,360));
		//newShell.transform.Rotate(random);
		//newShell.velocity = transform.parent.right * 10f;
	}
	
	void PrepareToFire(){
		if (flashDuration > 0) {
			flashDuration -= Time.deltaTime;
		} else {
			currentPos = stillPos;
			flashDuration = maxFlashDuration;
			flash.SetActive(false);
		}
		if(fireRate > 0){
			fireRate -= Time.deltaTime;
		} else {
			fireRate = maxFireRate;
			canShoot = true;
		}
	}
	
	void Reload(){
		if(reloadTime > 0){
			reloadTime -= Time.deltaTime;
		} else {
			ammo = maxAmmo;
			canShoot = true;
			reloadTime = maxReloadTime;
		}
	}

}
