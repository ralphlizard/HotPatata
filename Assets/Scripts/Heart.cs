using UnityEngine;
using System.Collections;

public class Heart : MonoBehaviour {
	
	AudioSource music;
	Camera camera;
	Vector3 targetDir;
	Vector3 camDir;

	// Use this for initialization
	void Start () {
		music = GetComponent<AudioSource>();
		camera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		camDir = camera.transform.forward;
		targetDir = transform.position - camera.transform.position;
		music.volume = map(Vector3.Angle(camDir, targetDir), 180, 0, 0, 1);
//		print (music.volume);
	}	

	float map(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s-a1)*(b2-b1)/(a2-a1);
	}
}