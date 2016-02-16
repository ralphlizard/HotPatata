using UnityEngine;
using System.Collections;

public class GazeAware : MonoBehaviour {
	public float lookBuffer; //length of time that needs to pass until object decides it's not being looked at

	GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;

	// Use this for initialization
	void Start () {
		gazeController = GameObject.FindGameObjectWithTag("Player").GetComponent<GazeController>();
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
	}
}