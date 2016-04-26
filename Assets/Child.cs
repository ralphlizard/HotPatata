using UnityEngine;
using System.Collections;

public class Child : MonoBehaviour {
	public bool isStanding;
	public bool isScreaming;
	public BeautifulDissolves.Dissolve dissolve;
	Human human;
	AudioSource audio;
	Animator anim;

	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float activeBuffer = 0; //length of time before object activates
	public float volumeAmpStep = 1; 

	public GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;
	bool targeted;

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
		if (isScreaming)
		{
			isScreaming = false;
			audio.Play();
			anim.SetBool("isScreaming", true);
			GetComponentInChildren<BalloonPop>().Pop();
			dissolve.TriggerDissolve();
		}
		ControlState();
	}

	void ControlState()
	{
		if (!targeted)
		{
			lookedAtDuration = Time.time - startLookedAt;

			// stoppped being looked at
			if (Time.time - prevLookTime >= lookBuffer &&
				lookedAt)
			{
				lookedAt = false;
				gazeController.GazeRelease();
				gazeController = null;
			}

			else if (lookedAt && lookedAtDuration >= activeBuffer)
			{
				Activate ();
			}

			/*
			if (!lookedAt && music.volume > 0.1f)
			{
				music.volume -= Time.deltaTime / volumeAmpStep;
			}
			*/
		}
	}

	void LookedAt() 
	{
		print("Nipples");
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
		isScreaming = true;
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