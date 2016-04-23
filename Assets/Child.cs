using UnityEngine;
using System.Collections;

public class Child : MonoBehaviour {
	public bool isStanding;
	public bool isScreaming;
	public GameObject balloon;
	AudioSource audio;
	Animator anim;

	// Use this for initialization
	void Start () {
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
			balloon.GetComponent<BalloonPop>().Pop();
		}
	}
}