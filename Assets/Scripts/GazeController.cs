using UnityEngine;
using System.Collections;

public class GazeController : MonoBehaviour {

	private Camera mainCamera;
	private GameObject gazeTarget;
	private bool activeExists;
	private RaycastHit hit;
	private Vector3 fwd;
	public int solved;

	// Use this for initialization
	void Start () {
		mainCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.R))
			Application.LoadLevel(Application.loadedLevelName);

		//raycast hits object
		fwd = mainCamera.transform.TransformDirection (Vector3.forward);
		if (gazeTarget == null &&
		    Physics.Raycast (mainCamera.transform.position, fwd, out hit, 100) && 
			hit.collider.tag == "GazeAware")
		{
			gazeTarget = hit.collider.gameObject;
			gazeTarget.SendMessageUpwards ("LookedAt");
		}
		else if (gazeTarget != null &&
		         Physics.Raycast (mainCamera.transform.position, fwd, out hit, 100) &&
		         hit.collider.gameObject.name == gazeTarget.name)
		{
			gazeTarget.SendMessageUpwards ("LookedAt");
		}
//		print ("Gazing " + gazeTarget != null);
	}

	public void GazeRelease ()
	{
		gazeTarget = null;
	}

	public void Solve ()
	{
		print ("Solved" + solved);
		solved++;
	}
}
