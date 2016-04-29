using UnityEngine;
using System.Collections;

public class Purgatory : MonoBehaviour {
	public bool useGravity;
	public float startingVel;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ApplyGravity (GameObject newObject)
	{
//		GetComponent<
		Rigidbody[] rigidBodies = newObject.GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody rigidBody in rigidBodies) {
//			rigidBody.velocity = new Vector3 (0,0,0);
			rigidBody.isKinematic = false;
			rigidBody.useGravity = true;
//			rigidBody.AddForce(new Vector3 (0,startingVel,0));
		}
	}

	public void ApplyGravityAll () 
	{
		useGravity = true;
		Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody rigidBody in rigidBodies) {
//			rigidBody.AddForce(new Vector3 (0,startingVel,0));
			rigidBody.isKinematic = false;
			rigidBody.useGravity = true;
		}
	}
}
