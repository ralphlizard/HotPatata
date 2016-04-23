using UnityEngine;
using System.Collections;

public class Human : MonoBehaviour {
	public Transform[] kids;
	public bool havePoked;
	int curIndex = 0;
	Transform curKid;
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
		curKid = kids[curIndex];

		if (curKid != null) //a target was acquired
		{
			agent.destination = curKid.position;
			if (agent.remainingDistance > minDistance) //pursue target
			{
				anim.SetBool("isWalking", true);
				if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.walk")) //check if getup is done
				{
					agent.Resume();
					/*
					transform.position = Vector3.MoveTowards(transform.position, curKid.position, Time.deltaTime * moveSpeed);
					Vector3 targetDir = new Vector3(curKid.position.x, 0, curKid.position.z) - 
						new Vector3(transform.position.x, 0, transform.position.z);
					Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, Time.deltaTime * rotSpeed, 0);
					transform.rotation = Quaternion.LookRotation(newDir);
					*/
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
					curIndex++; //pursue next kid
					curKid.GetComponent<Child>().isScreaming = true;
				}
			}
		}
	}
}
