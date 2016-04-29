using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Human : NetworkBehaviour {

	public Transform targetSlug;
	public bool havePoked;
	public float turnSpeed;
	int targetIndex = 0;
	int targetsSize = 0;
	float minDistance = 0;
	bool arrived;
	Animator anim;
	NavMeshAgent agent;
	public Transform[] targets;
	Vector3 startPos;
	Quaternion startRot;

	[SyncVar]
	Transform curTarget;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this);
		startPos = transform.position;
		startRot = transform.rotation;
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		minDistance = agent.stoppingDistance;
		agent.Stop();
	}

	public void Initialize()
	{
		transform.position = startPos;
		transform.rotation = startRot;
		anim.SetTrigger ("reset");
	}
	
	// Update is called once per frame
	void Update () {
//		if (targets [targetIndex] != null) {
		curTarget = targets[targetIndex];
//		}

		if (curTarget != null) //a target was acquired
		{
			agent.destination = curTarget.GetComponentInChildren<BalloonPop>().transform.position;
			if (agent.remainingDistance > minDistance) //pursue target
			{
				arrived = false;
//				transform.eulerAngles = Vector3.RotateTowards (transform.eulerAngles, curTarget.GetComponentInChildren<BalloonPop>().transform.position - transform.position, turnSpeed * Time.deltaTime,1);
				anim.SetBool("isWalking", true);
				if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.walk")) //check if getup is done
				{
					agent.Resume();
				}
			}
			else //arrived at target
			{
				agent.Stop();
//				anim.SetBool("isWalking", false);
//				anim.SetBool ("isIdle", false);

				if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.walk") &&
					!anim.GetCurrentAnimatorStateInfo(0).IsName("human.idleStand") &&
					!havePoked) //check if walking is done
					anim.SetBool("isPoking", true); //poke the taraget

				if (!arrived)
				{
					arrived = true;
				}

				if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.poke")) //check if poking is in progress
				{
					havePoked = true;
					anim.SetBool("isPoking", false);
					anim.SetBool("isIdle", true); //stand idle
				}
				else if (havePoked) //check if poking is done
				{
					havePoked = false;
					targetIndex++; //pursue next taraget
					print("poke");
					curTarget.SendMessageUpwards("ReceivePoke");
				}
			}
		}
	}

	public void AddTarget (Transform targetTransform)
	{
		targets[targetsSize] = targetTransform;
		targetsSize++;
	}
}