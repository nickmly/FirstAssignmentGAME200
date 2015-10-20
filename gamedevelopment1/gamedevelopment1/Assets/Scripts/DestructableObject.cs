using UnityEngine;
using System.Collections;

public class DestructableObject : MonoBehaviour {
	
	private Rigidbody rb;
	void Start(){
		rb = GetComponent<Rigidbody>();
	}
	
	public void Destruct(){
		rb.isKinematic = false;
	}
}
