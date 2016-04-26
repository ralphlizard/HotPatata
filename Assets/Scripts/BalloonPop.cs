using UnityEngine;
using System.Collections;

public class BalloonPop : MonoBehaviour {
	Rigidbody[] rigidBodies;

	// Use this for initialization
	void Start () {
		rigidBodies = transform.GetComponentsInChildren<Rigidbody>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
	}

	public void Pop()
	{
		transform.parent = null;
		foreach (Rigidbody rigidBody in rigidBodies)
			rigidBody.isKinematic = false;
//		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<AudioSource>().Play();
	}
}
