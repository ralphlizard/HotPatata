using UnityEngine;
using System.Collections;

public class StayUpright : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.eulerAngles = new Vector3(0,0,0); //keep upright
	}
}
