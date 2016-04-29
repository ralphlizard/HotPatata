using UnityEngine;
using System.Collections;

public class Child : MonoBehaviour {
	public bool isStanding;
	public BeautifulDissolves.Dissolve dissolve;
	public SkinnedMeshRenderer kidMaterial;
	public Transform teleDestination;
	public Human human;
	AudioSource[] audios;
	Animator anim;

	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float activeBuffer = 0; //length of time before object activates
	public float deathTimer = 5;
	public float ragTimer = 2;
	Color origColor;
	public Color targetColor;

	float poppedTime;
	public GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;
	bool targeted;
	bool fullyActive;
	bool teleported;

	// Use this for initialization
	void Start () {
		origColor = kidMaterial.material.color;
		if (GameObject.FindGameObjectWithTag ("Human") != null) {
			human = GameObject.FindGameObjectWithTag ("Human").GetComponent<Human> ();
		}
		audios = GetComponents<AudioSource>();
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
		if (human == null &&
			GameObject.FindGameObjectWithTag ("Human") != null) {
			human = GameObject.FindGameObjectWithTag ("Human").GetComponent<Human> ();
		}
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
			Time.time - poppedTime > deathTimer &&
			!teleported) //countdown to child's death after poking
		{
			Teleport ();
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

//			Mathf.Clamp (lookedAtDuration, 0, activeBuffer);
				
			// reached maximum look duration
			if (lookedAtDuration >= activeBuffer)
			{
				Activate ();
			}
			float newR = origColor.r + (lookedAtDuration/activeBuffer) * (targetColor.r - origColor.r);
			float newG = origColor.g + (lookedAtDuration/activeBuffer) * (targetColor.g - origColor.g);
			float newB = origColor.b + (lookedAtDuration/activeBuffer) * (targetColor.b - origColor.b);
			if (kidMaterial != null)
				kidMaterial.material.color = new Color(newR,newG,newB);
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
		audios[Random.Range(0,5)].Play(); //scream
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

	void Teleport ()
	{
		teleported = true;
		transform.position = teleDestination.position;
		Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody> ();
		foreach (Rigidbody rigidBody in rigidBodies) {
			rigidBody.isKinematic = true;
			rigidBody.velocity  = new Vector3 (0, 0, 0);
		}
		GameObject purgatory = GameObject.FindGameObjectWithTag ("Purgatory");
		transform.parent = purgatory.transform;
		if (purgatory.GetComponent<Purgatory> ().useGravity) {
			purgatory.GetComponent<Purgatory> ().ApplyGravity (gameObject);
		}
//		purgatory.GetComponent<Purgatory> ().ApplyGravityAll ();
	}
}