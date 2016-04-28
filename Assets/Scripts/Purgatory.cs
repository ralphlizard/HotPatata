using UnityEngine;
using System.Collections;

public class Purgatory : MonoBehaviour {
	public bool useGravity;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ApplyGravity (GameObject newObject)
	{
		Rigidbody[] rigidBodies = newObject.GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody rigidBody in rigidBodies) {
			rigidBody.isKinematic = false;
		}
	}

	public void ApplyGravityAll () 
	{
		useGravity = true;
		Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody rigidBody in rigidBodies) {
			rigidBody.isKinematic = false;
		}
	}
}
