using UnityEngine;
using System.Collections;

public class Slug : MonoBehaviour {
	BeautifulDissolves.Dissolve[] dissolves;
	public SkinnedMeshRenderer slugMaterial1;
	public MeshRenderer slugMaterial2;
	Color origColor;
	public Color targetColor;
	Human human;
	AudioSource[] audios;
	Animator anim;

	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float activeBuffer = 3; //length of time before object activates
	public float deathTimer = 5;
	public float ragTimer = 0;
	public float gameOverTime = 12;

	float poppedTime;
	public GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;
	bool targeted;
	bool fullyActive;
	bool teleported;
	float teleportTime;
	Vector3 startPos;
	Quaternion startRot;

	// Use this for initialization
	void Start () {
//		Initialize ();
		origColor = slugMaterial1.material.color;
		anim = GetComponent<Animator> ();
		dissolves = GetComponentsInChildren<BeautifulDissolves.Dissolve> ();
		human = GameObject.FindGameObjectWithTag("Human").GetComponent<Human>();
		audios = GetComponents<AudioSource>();
		startPos = transform.position;
		startRot = transform.rotation;
	}

	public void Initialize ()
	{
		poppedTime = 0;
		gazeController = null;
		lookedAtDuration = 0;
		startLookedAt = 0;
		prevLookTime = 0;
		lookedAt = false;
		targeted = false;
		fullyActive = false;
		teleported = false;
		transform.position = startPos;
		transform.rotation = startRot;
		teleportTime = 0;
		GetComponent<GazeController> ().ResetBalloon();
	}

	// Update is called once per frame
	void LateUpdate () {
		/* look at current gazer
		if (gazeController != null) 
		{
			headLookController.targetObject = gazeController.gameObject;
		}
		*/
		if (teleported &&
			Time.time - teleportTime >= gameOverTime)
		{
			GameObject networkManager = GameObject.FindGameObjectWithTag ("NetworkManager");
			networkManager.GetComponent<InitializeNetwork> ().ResetGame ();
		}

		HandlePostPoke ();
		ControlState();
	}

	void OnCollisionEnter ()
	{
		audios [1].Play ();
		//reset the game here
	}

	void HandlePostPoke()
	{
		if (poppedTime != 0 &&
			anim.enabled &&
			Time.time - poppedTime > ragTimer) //countdown to ragdolling
		{
			anim.enabled = false;
			GetComponent<Rigidbody> ().isKinematic = false;
		}

		if (poppedTime != 0 && 
			Time.time - poppedTime > deathTimer &&
			!teleported) //countdown to child's death after poking
		{
//			teleport here
			Teleport();
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
			if (slugMaterial1 != null)
				slugMaterial1.material.color = new Color(newR,newG,newB);
			if (slugMaterial2 != null)
				slugMaterial2.material.color = new Color(newR,newG,newB);
		}
	}

	void LookedAt() 
	{		
		prevLookTime = Time.time;
		// is being looked at
		if (!lookedAt) 
		{
			audios[0].Play(); //call when looked at 
			lookedAt = true;
			startLookedAt = Time.time;
		}
	}

	void ReceivePoke()
	{
		poppedTime = Time.time;
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

	void Teleport ()
	{
		teleportTime = Time.time;
		teleported = true;
		GameObject purgatory = GameObject.FindGameObjectWithTag ("Purgatory");
		GetComponent<Rigidbody> ().isKinematic = true;
		GetComponent<Rigidbody> ().velocity = new Vector3 (0, 0, 0);
		transform.eulerAngles = new Vector3 (0,0,0);
		transform.position = purgatory.transform.position;
//		transform.parent = purgatory.transform;
		purgatory.GetComponent<Purgatory> ().ApplyGravity (gameObject);
		purgatory.GetComponent<Purgatory> ().ApplyGravityAll ();
	}
}