using UnityEngine;
using System.Collections;

public class Slug : MonoBehaviour {
	public bool isStanding;
	BeautifulDissolves.Dissolve[] dissolves;
	public SkinnedMeshRenderer slugMaterial1;
	public MeshRenderer slugMaterial2;
	Human human;
	AudioSource audio;
	Animator anim;

	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float activeBuffer = 3; //length of time before object activates
	public float deathTimer = 5;
	public float ragTimer = 0;

	float poppedTime;
	public GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;
	bool targeted;
	bool fullyActive;

	// Use this for initialization
	void Start () {
		dissolves = GetComponentsInChildren<BeautifulDissolves.Dissolve> ();
		human = GameObject.FindGameObjectWithTag("Human").GetComponent<Human>();
		audio = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void LateUpdate () {
		/* look at current gazer
		if (gazeController != null) 
		{
			headLookController.targetObject = gazeController.gameObject;
		}
		*/

		HandlePostPoke ();
		ControlState();
	}

	void HandlePostPoke()
	{
		if (poppedTime != 0 &&
			anim.enabled &&
			Time.time - poppedTime > ragTimer) //countdown to ragdolling
		{
			anim.enabled = false;
		}

		if (poppedTime != 0 && 
			Time.time - poppedTime > deathTimer) //countdown to child's death after poking
		{
//			teleport here
			Destroy (this.gameObject);
		}
	}

	void ControlState()
	{
		if (!targeted)
		{
			// increment while being looked at
			if (lookedAt) 
			{
				lookedAtDuration = Time.time - startLookedAt;
			}

			// stoppped being looked at
			if (Time.time - prevLookTime >= lookBuffer &&
				lookedAt)
			{
				lookedAt = false;
				gazeController.GazeRelease ();
				gazeController = null;
			}

			// decrement while not looked at
			if (!lookedAt && lookedAtDuration > 0)
			{
				lookedAtDuration -= Time.deltaTime;
			}

			Mathf.Clamp (lookedAtDuration, 0, activeBuffer);

			// reached maximum look duration
			if (lookedAtDuration >= activeBuffer)
			{
				Activate ();
			}
			float newColor = lookedAtDuration/activeBuffer;
			if (slugMaterial1 != null)
				slugMaterial1.material.color = new Color(newColor,newColor,newColor);
			if (slugMaterial2 != null)
				slugMaterial2.material.color = new Color(newColor,newColor,newColor);
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

	void ReceivePoke()
	{
		poppedTime = Time.time;
		audio.Play(); //scream
//		anim.SetBool("isScreaming", true);
		GetComponentInChildren<BalloonPop>().Pop();
		foreach (BeautifulDissolves.Dissolve dissolve in dissolves)
			dissolve.TriggerDissolve ();
	}

	void Activate()
	{
		this.tag = "Untagged";
		gazeController.GazeRelease ();
		gazeController = null;
		targeted = true;
		human.AddTarget(transform);
	}

	void AttachGazeController (GazeController newGaze)
	{
		if (newGaze == this)
			return;
		if (gazeController == null)
			gazeController = newGaze;
	}
}