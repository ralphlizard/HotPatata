using UnityEngine;
using System.Collections;

public class Child : MonoBehaviour {
	public bool isStanding;
	public BeautifulDissolves.Dissolve dissolve;
	public HeadLookController headLookController;
	Human human;
	AudioSource audio;
	Animator anim;

	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float activeBuffer = 0; //length of time before object activates
	public float deathTimer = 5;

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
	void Update () {
		if (gazeController != null) {
			headLookController.targetObject = gazeController.gameObject;
		}

		if (poppedTime != 0 && 
			Time.time - poppedTime > deathTimer) //countdown to child's death after poking
		{
			Destroy (this.gameObject);
		}
		ControlState();
	}

	void ControlState()
	{
		if (!targeted)
		{
			// stoppped being looked at
			if (Time.time - prevLookTime >= lookBuffer &&
				lookedAt)
			{
				lookedAt = false;
				if (gazeController != null) 
				{
					gazeController.GazeRelease ();
					gazeController = null;
				}
			}

			// increment while being looked at
			if (Time.time - prevLookTime < lookBuffer &&
				lookedAt) 
			{
				print (lookedAtDuration);
				lookedAtDuration = Time.time - startLookedAt;
			}

			// reached maximum look duration
			if (lookedAt && lookedAtDuration >= activeBuffer)
			{
				Activate ();
			}

			Mathf.Clamp (lookedAtDuration, 0, activeBuffer);

			// decrement while not looked at
			if (!lookedAt && lookedAtDuration > 0)
			{
				lookedAtDuration -= Time.deltaTime;
			}
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
		audio.Play();
		anim.SetBool("isScreaming", true);
		GetComponentInChildren<BalloonPop>().Pop();
		dissolve.TriggerDissolve();
	}

	void Activate()
	{
		targeted = true;
		human.AddTarget(transform);
	}

	void AttachGazeController (GazeController newGaze)
	{
		if (gazeController = null)
			gazeController = newGaze;
	}
}