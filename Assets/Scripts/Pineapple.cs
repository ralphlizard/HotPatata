using UnityEngine;
using System.Collections;

public class Pineapple : MonoBehaviour {
	public float lookBuffer; //length of time that needs to pass until object decides it's not being looked at
	public int rotType;
	public bool rotten;

	Animator anim;
	GazeController gazeController;
	float lookedAtDuration;
	float startLookedAt;
	float prevLookTime;
	bool lookedAt;
	SkinnedMeshRenderer skinnedMeshRenderer;
	float blendWeight1;
	float blendWeight2;

	// Use this for initialization
	void Start () {
		gazeController = GameObject.FindGameObjectWithTag("Player").GetComponent<GazeController>();
		anim = GetComponentInChildren<Animator> ();
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
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
		if (blendWeight1 < 100)
		{
			blendWeight1 += 10 * Time.deltaTime;
			skinnedMeshRenderer.SetBlendShapeWeight(rotType, blendWeight1);
		}
		else
		{
			rotten = true;
			if (gazeController.solved == 2)
				gazeController.solved++;
		}
	}
}