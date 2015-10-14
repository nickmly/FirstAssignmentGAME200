﻿using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	[SerializeField] private GameObject target;

	public enum State
	{
		Idle,
		Chasing,
		Looking,
		Attacking,
	}

	private float moveSpeed = 0.1f;
	private bool alive = true;
	private float health = 100f;

	// Use this for initialization
	void Start () {
		target = GameObject.Find ("FPSController"); //temporary
	}
	
	// Update is called once per frame
	void Update () {
		if (alive) {	
			Vector3 targetPos = new Vector3(target.transform.position.x,transform.position.y,target.transform.position.z);
			transform.LookAt (targetPos);
			if (Vector3.Distance (target.transform.position, transform.position) > 10) {
				transform.position += transform.forward * moveSpeed;
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