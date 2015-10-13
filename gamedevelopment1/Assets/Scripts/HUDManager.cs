using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDManager : MonoBehaviour {

	public Text ammo, use;
	public Gun gun;
	// Use this for initialization
	void Start () {
	
	}

	public void ShowUseText(string key, string action){
		use.enabled = true;
		use.text = "Press " + key + " to " + action;
	}

	// Update is called once per frame
	void Update () {
		if (gun != null) {
			ammo.text = gun.ammo.ToString ();
		} else {
			ammo.text = "--";
		}
	}
}
