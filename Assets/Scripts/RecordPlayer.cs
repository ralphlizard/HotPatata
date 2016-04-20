using UnityEngine;
using System.Collections;

public class RecordPlayer : MonoBehaviour {
	public float lookBuffer = 0.5f; //length of time that needs to pass until object decides it's not being looked at
	public float activeBuffer = 0; //length of time before object activates
	public float volumeAmpStep = 1; 
	
	GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;
	public bool displayText;
	AudioSource music;

	// Use this for initialization
	void Start () {
		gazeController = GameObject.FindGameObjectWithTag("Player").GetComponent<GazeController>();
		music = GetComponent<AudioSource>();
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

		if (!lookedAt && music.volume > 0.1f)
		{
			music.volume -= Time.deltaTime / volumeAmpStep;
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
		if (music.volume < 1 && lookedAtDuration > activeBuffer)
		{
			music.volume += Time.deltaTime / volumeAmpStep;
		}
		if (lookedAtDuration > 5 && gazeController.solved >= 3)
		{
			if (gazeController.solved == 3)
				gazeController.solved++;
		}
	}
}