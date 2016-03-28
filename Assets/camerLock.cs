using UnityEngine;
using System.Collections;

public class camerLock : MonoBehaviour {
	Quaternion rot;

	// Use this for initialization
	void Start () {
		rot = transform.rotation;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.rotation = rot;
	}
}
