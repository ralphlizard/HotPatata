using UnityEngine;
using System.Collections;

public class Human : MonoBehaviour {
	public Transform[] kids;
	int curIndex = 0;
	Transform curKid;
	public float minDistance = 0;
	public float moveSpeed = 1;
	public float rotSpeed = 1;
	Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		curKid = kids[curIndex];

		if (curKid != null) //a target was acquired
		{
			if (Vector3.Distance(transform.position, curKid.position) > minDistance) //pursue target
			{
				anim.SetTrigger("walk");
				anim.SetTrigger("walk");
				if (anim.GetCurrentAnimatorStateInfo(0).IsName("human.walk")) //check if getup is done
				{
					transform.position = Vector3.MoveTowards(transform.position, curKid.position, Time.deltaTime * moveSpeed);
					Vector3 targetDir = new Vector3(curKid.position.x, 0, curKid.position.z) - 
						new Vector3(transform.position.x, 0, transform.position.z);
					Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, Time.deltaTime * rotSpeed, 0);
					transform.rotation = Quaternion.LookRotation(newDir);
				}
			}
			else //arrived at target
			{
				anim.SetTrigger("poke"); //poke the kid
				if (!anim.GetCurrentAnimatorStateInfo(0).IsName("human.poke")) //check if poking is done
				{
					curIndex++; //pursue next kid
					anim.SetTrigger("idle");
				}
			}
		}
	}
}
