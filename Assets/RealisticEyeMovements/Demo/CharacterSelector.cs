using RealisticEyeMovements;
using UnityEngine;
using UnityEngine.UI;



public class CharacterSelector : MonoBehaviour
{
	#region fields

		public float orbitSpeed = 10;
		public float upDownAmplitude = 0.1f;
		public float upDownSpeed = 10;

		public GameObject makeHumanGO;
		public GameObject autodeskGO;
		public GameObject mixamoGO;

		public Button makeHumanButton;
		public Button autodeskButton;
		public Button mixamoButton;

		GameObject activeGameObject;
		LookTargetController lookTargetController;

		Transform sphereAnchorXform;
		Transform sphereXform;
		Transform leftCubeXform;
		Transform rightCubeXform;

		float timeToChangeLeftRightTarget;
		Transform leftRightTargetXform;

		float sphereUpDownTime;

		bool isSphereOrbiting;
		bool isSphereUpAndDown;
		bool isLookingLeftRight;

	#endregion



	void Awake()
	{
		sphereAnchorXform = transform.Find("Sphere anchor");
		if ( sphereAnchorXform != null )
			sphereXform = sphereAnchorXform.Find("Sphere");
		leftCubeXform = transform.Find("Left cube");
		rightCubeXform = transform.Find("Right cube");

		activeGameObject = autodeskGO;

		UpdateCharacters();
	}



	public void OnAutodeskSelected()
	{
		activeGameObject = autodeskGO;
		UpdateCharacters();
	}



	public void OnLookAtPlayerSelected()
	{
		isLookingLeftRight = false;
		lookTargetController.LookAtPlayer();
	}



	public void OnLookAtSphereSelected()
	{
		isLookingLeftRight = false;
		lookTargetController.LookAtPoiDirectly(sphereXform);
	}



	public void OnLookIdlySelected()
	{
		isLookingLeftRight = false;
		lookTargetController.LookAroundIdly();
	}



	public void OnMakeHumanSelected()
	{
		activeGameObject = makeHumanGO;
		UpdateCharacters();
	}



	public void OnMixamoSelected()
	{
		activeGameObject = mixamoGO;
		UpdateCharacters();
	}



	public void OnLeftRightSelected()
	{
		isLookingLeftRight = true;

		timeToChangeLeftRightTarget = 4;
		leftRightTargetXform = (leftRightTargetXform == leftCubeXform) ? rightCubeXform : leftCubeXform;
		lookTargetController.LookAtPoiDirectly(leftRightTargetXform);
	}



	public void OnToggleSphereOribiting(bool toggle)
	{
		isSphereOrbiting = !isSphereOrbiting;
	}



	public void OnToggleSphereUpAndDown(bool toggle)
	{
		isSphereUpAndDown = !isSphereUpAndDown;
	}



	void Update()
	{
		if ( isLookingLeftRight )
		{
			timeToChangeLeftRightTarget -= Time.deltaTime;
			if ( timeToChangeLeftRightTarget < 0 )
			{
				timeToChangeLeftRightTarget = 4;
				leftRightTargetXform = (leftRightTargetXform == leftCubeXform) ? rightCubeXform : leftCubeXform;
				lookTargetController.LookAtPoiDirectly(leftRightTargetXform);
			}
		}

		if ( isSphereOrbiting )
			sphereAnchorXform.Rotate(Vector3.up, Time.deltaTime * orbitSpeed);

		if ( isSphereUpAndDown )
		{
			sphereUpDownTime += Time.deltaTime;
			sphereAnchorXform.localPosition = upDownAmplitude * Mathf.Sin(sphereUpDownTime * upDownSpeed) * Vector3.up;
		}
	}



	void UpdateCharacters()
	{
		mixamoGO.SetActive(activeGameObject == mixamoGO);
		autodeskGO.SetActive(activeGameObject == autodeskGO);
		makeHumanGO.SetActive(activeGameObject == makeHumanGO);

		makeHumanButton.interactable = activeGameObject != makeHumanGO;
		autodeskButton.interactable = activeGameObject != autodeskGO;
		mixamoButton.interactable = activeGameObject != mixamoGO;

		lookTargetController = activeGameObject.GetComponent<LookTargetController>();
	}

}
