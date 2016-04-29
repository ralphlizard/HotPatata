using UnityEngine;
using System.Collections;

public class LoopBg : MonoBehaviour {
	static float speed = 10;
	public float planeL = 110.705f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.MoveTowards (transform.position, new Vector3(transform.position.x, transform.position.y, -100), speed*Time.deltaTime);
		if (transform.position.z < -83)
			transform.position = new Vector3 (transform.position.x, transform.position.y, transform.position.z + planeL * 2);	
	}
}
