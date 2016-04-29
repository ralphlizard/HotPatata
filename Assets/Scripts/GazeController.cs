using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GazeController : NetworkBehaviour {

	public Camera mainCamera;
	public GameObject headedSlug;
	public GameObject headlessSlug;
	public GameObject eye_L;
	public GameObject eye_R;
	public GameObject heart;
	public AudioListener listener;
	public OVRCameraRig OculusCameraRig;
	public int solved;
	public GameObject slugBalloon;
	public Transform rightEye;

	private ButterflyFlight butterfly;
	private GameObject gazeTarget;
	private bool activeExists;
	private RaycastHit hit;
	private Vector3 fwd;
	private Transform spawn;
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this.gameObject);
		GameObject.FindGameObjectWithTag ("NetworkManager").GetComponent<InitializeNetwork> ().AttachPlayer (this);

		if (isLocalPlayer)
		{
			OculusCameraRig.enabled = true;
			eye_L.SetActive(false);
			eye_R.SetActive(false);
			listener.enabled = true;
			headedSlug.SetActive(false);
			heart.SetActive(false);
			headedSlug.SetActive(false);
			mainCamera.enabled = true;
//			mainCamera.tag = "MainCamera";
//			this.gameObject.tag = "Player";
		}
		else
		{
			headlessSlug.SetActive (false);
//			mainCamera.tag = "Untagged";		
		}
	}
	
	// Update is called once per frame
	void Update () {
		//raycast hits object
		fwd = mainCamera.transform.TransformDirection (Vector3.forward);
		if (gazeTarget == null &&
		    Physics.Raycast (mainCamera.transform.position, fwd, out hit, 100) && 
			hit.collider.tag == "GazeAware" &&
			hit.collider != GetComponent<Collider>())
		{
			gazeTarget = hit.collider.gameObject;
			gazeTarget.SendMessageUpwards ("LookedAt");
			gazeTarget.SendMessageUpwards ("AttachGazeController", this);
			
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

	public void ResetBalloon ()
	{
		if (rightEye.childCount == 0) //give new balloon
		{
			Instantiate (slugBalloon);
			Vector3 tempPos = slugBalloon.transform.localPosition;
			Quaternion tempRot = slugBalloon.transform.localRotation;
			Vector3 tempScale = slugBalloon.transform.localScale;
			slugBalloon.transform.parent = rightEye;
			slugBalloon.transform.localPosition = tempPos;
			slugBalloon.transform.localRotation = tempRot;
			slugBalloon.transform.localScale = tempScale;
		}
	}
}