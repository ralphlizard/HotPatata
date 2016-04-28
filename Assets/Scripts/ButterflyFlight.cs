using UnityEngine;
using System.Collections;

public class ButterflyFlight : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	void ResetGame () {
		Application.LoadLevel (Application.loadedLevel);		
	}
}
