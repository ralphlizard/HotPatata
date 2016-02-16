using UnityEngine;
using System.Collections;

public class Balloon : MonoBehaviour {
	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float popTime;
	public float maxSize;
	public float scaleFactor = 0.05f;

	Animator anim;
	GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;
	float blendWeight1;
	float blendWeight2;
	Rigidbody[] rigidBodies;
	bool popped;

	// Use this for initialization
	void Start () {
		gazeController = GameObject.FindGameObjectWithTag("Player").GetComponent<GazeController>();
		anim = GetComponentInChildren<Animator> ();
		rigidBodies = transform.GetComponentsInChildren<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		ControlState ();
	}
	
	void ControlState()
	{
		lookedAtDuration = Time.time - startLookedAt;

		// stoppped being looked at
		if (Time.time - prevLookTime >= lookBuffer &&
		    lookedAt)
		{
			lookedAt = false;
			gazeController.GazeRelease();
		}

		else if (lookedAt)
		{
			Activate ();
		}
	}

	void LookedAt() 
	{
		prevLookTime = Time.time;
		// is being looked at
		if (!lookedAt) 
		{
			lookedAt = true;
			startLookedAt = Time.time;
		}
	}

	void Activate()
	{
		if (lookedAtDuration >= popTime &&
		    rigidBodies[0].isKinematic &&
		    !popped)
		{
			foreach (Rigidbody rigidBody in rigidBodies)
				rigidBody.isKinematic = false;
			popped = true;
			if (gazeController.solved == 1)
				gazeController.solved++;
			GetComponent<Rigidbody>().isKinematic = false;
			GetComponent<AudioSource>().Play();
		}
		float scale = Time.deltaTime * scaleFactor;
		if (!popped && transform.localScale.x < 2f)
			transform.localScale += new Vector3(scale, scale, scale);
	}
}