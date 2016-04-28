using UnityEngine;
using System.Collections;

public class Human : MonoBehaviour {
	public Transform[] kids;
	public Transform targetSlug;
	public bool havePoked;
	int kidIndex = 0;
	int kidsSize = 0;
	Transform curTarget;
	float minDistance = 0;
	Animator anim;
	NavMeshAgent agent;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		minDistance = agent.stoppingDistance;
		agent.Stop();
	}
	
	// Update is called once per frame
	void Update () {
		if (kidIndex < kids.Length)
		{
			curTarget = kids[kidIndex];
		}
		else //exhausted kids list
		{
			curTarget = targetSlug;
		}

		if (curTarget != null) //a target was acquired
		{
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
					anim.SetBool("isPoking", true); //poke the kid
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
					kidIndex++; //pursue next kid
					curTarget.SendMessageUpwards("ReceivePoke");
				}
			}
		}
	}

	public void AddTarget (Transform targetTransform)
	{
		kids[kidsSize] = targetTransform;
		kidsSize++;
	}
}