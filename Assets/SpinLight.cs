using UnityEngine;
using System.Collections;

public class SpinLight : MonoBehaviour {
	public float rotSpeed = 10;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y + rotSpeed * Time.deltaTime, transform.eulerAngles.z);
	}
}
