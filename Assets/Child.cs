using UnityEngine;
using System.Collections;

public class Child : MonoBehaviour {
	public bool isStanding;
	public BeautifulDissolves.Dissolve dissolve;
	public HeadLookController headLookController;
	public SkinnedMeshRenderer kidMaterial;
	Human human;
	AudioSource audio;
	Animator anim;

	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float activeBuffer = 0; //length of time before object activates
	public float deathTimer = 5;
	public float ragTimer = 2;

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
		human = GameObject.FindGameObjectWithTag("Human").GetComponent<Human>();
		audio = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		if (isStanding)
		{
			anim.SetInteger("initialPos", 0);
		}
		else
		{
			int rand = Random.Range(1,3);
			anim.SetInteger("initialPos", rand);
		}
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
			kidMaterial.material.color = new Color(newColor,newColor,newColor);
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
		anim.SetBool("isScreaming", true);
		GetComponentInChildren<BalloonPop>().Pop();
		dissolve.TriggerDissolve();
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
		if (gazeController == null)
			gazeController = newGaze;
	}
}