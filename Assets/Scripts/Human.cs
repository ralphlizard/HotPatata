using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Human : NetworkBehaviour {

	public Transform targetSlug;
	public bool havePoked;
	int targetIndex = 0;
	int targetsSize = 0;
	float minDistance = 0;
	Animator anim;
	NavMeshAgent agent;
	public Transform[] targets;

	[SyncVar]
	Transform curTarget;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		minDistance = agent.stoppingDistance;
		agent.Stop();
	}
	
	// Update is called once per frame
	void Update () {
		curTarget = targets[targetIndex];
		print (targetIndex);

		if (curTarget != null) //a target was acquired
		{
			print (curTarget);
			agent.destination = curTarget.GetComponentInChildren<BalloonPop>().transform.position;
			if (agent.remainingDistance > minDistance) //pursue target
			{
				anim.SetBool("isWalking", true);
				if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.walk")) //check if getup is done
				{
					agent.Resume();
				}
			}
			else //arrived at target
			{
				agent.Stop();
				anim.SetBool("isWalking", false);
				if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.walk") &&
					!anim.GetCurrentAnimatorStateInfo(0).IsName("human.idleStand") &&
					!havePoked) //check if walking is done
				{
					anim.SetBool("isPoking", true); //poke the taraget
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