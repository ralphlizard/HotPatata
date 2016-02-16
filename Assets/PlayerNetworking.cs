using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerNetworking : NetworkBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnStartLocalPlayer() {

		// disable my mesh
		// enable camera
		// enable inputs (or just check isLocalPlayer in update)
	}
}
