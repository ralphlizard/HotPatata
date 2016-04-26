// LookTargetController.cs
// Tore Knabe
// Copyright 2015 ioccam@ioccam.com


using UnityEngine;
using Random = UnityEngine.Random;


namespace RealisticEyeMovements {

	public class LookTargetController : MonoBehaviour
	{
		#region fields

			[Tooltip("Drag objects here for the actor to look at. If empty, actor will look in random directions.")]
			public Transform[] pointsOfInterest;

			[Tooltip("Ratio of how often to look at player vs elsewhere. 0: never, 1: always")]
			[Range(0,1)]
			public float lookAtPlayerRatio = 0.1f;

			[Tooltip("How likely the actor is to look back at the player when player stares at actor.")]
			[Range(0,1)]
			public float stareBackFactor = 0;

			[Tooltip("If player is closer than this, notice him")]
			[Range(0, 100)]
			public float noticePlayerDistance = 0;

			[Tooltip("If player is closer than this, look away (overrides noticing him)")]
			[Range(0, 4)]
			public float personalSpaceDistance = 0;

			[Tooltip("Minimum time to look at a target")]
			[Range(1f, 100f)]
			public float minLookTime = 3f;

			[Tooltip("Maximum time to look at a target")]
			[Range(1f, 100f)]
			public float maxLookTime = 10f;

			[Tooltip("For 3rd person games, set this to the player's eye center transform")]
			public Transform playerEyeCenter;

			[Tooltip("Keep trying to track target even when it moves out of sight")]
			public bool keepTargetEvenWhenLost = true;


			EyeAndHeadAnimator eyeAndHeadAnimator;

			const float minLookAtMeTimeToReact = 4;

			Transform targetPOI;
		
			Transform playerEyeCenterXform;
			Transform playerLeftEyeXform;
			Transform playerRightEyeXform;

			GameObject createdVRParentGO;
			GameObject createdPlayerEyeCenterGO;
			GameObject createdPlayerLeftEyeGO;
			GameObject createdPlayerRightEyeGO;
		
			float lastDistanceToPlayer = -1;
			float playerLookingAtMeTime;
			float nextChangePOITime;
			float stareBackDeadtime;	
			float timeOfLastNoticeCheck = -1000;
			float timeOfLastLookBackCheck = -1000;
			float timeOutsideOfAwarenessZone = 1000;
			float timeInsidePersonalSpace;

			bool useNativeVRSupport;
			bool useVR;

			bool isInitialized;
			bool hasRegisteredTargetLostFunction;

			enum State
			{
				LookingAtPlayer,
				LookingAroundIdly,
				LookingAtPoiDirectly,
				LookingAwayFromPlayer
			}
			State state;

		#endregion
	
	

		public void Blink()
		{
			eyeAndHeadAnimator.Blink();
		}



		Vector3 ChooseNextHeadTargetPoint()
		{
			bool hasBoneEyelidControl = eyeAndHeadAnimator.controlData.eyelidControl == ControlData.EyelidControl.Bones;
			float angleVert = Random.Range(-0.5f * (hasBoneEyelidControl ? 6f : 3f), hasBoneEyelidControl ? 6f : 4f);
			float angleHoriz = Random.Range(-10f, 10f);

			Vector3 point = eyeAndHeadAnimator.GetOwnEyeCenter() + eyeAndHeadAnimator.eyeDistanceScale * Random.Range(3.0f, 5.0f) *
																						eyeAndHeadAnimator.headParentXform.TransformDirection((Quaternion.Euler(	angleVert, angleHoriz, 0) *  Vector3.forward));

			return point;
		}



		Transform ChooseNextHeadTargetPOI()
		{
			if ( pointsOfInterest == null || pointsOfInterest.Length == 0 )
				return null;

			int numPOIsInView = 0;
			for (int i=0;  i<pointsOfInterest.Length;  i++)
			{
				if  ( pointsOfInterest[i] != targetPOI && eyeAndHeadAnimator.CanGetIntoView(pointsOfInterest[i].position) )
					numPOIsInView++;
			}
			if ( numPOIsInView == 0 )
				return targetPOI;
			
			
			int targetVisibleIndex = Random.Range(0, numPOIsInView);
			int visibleIndex = 0;
			for (int i=0;  i<pointsOfInterest.Length;  i++)
			{
				if  ( pointsOfInterest[i] != targetPOI && eyeAndHeadAnimator.CanGetIntoView(pointsOfInterest[i].position) )
				{
					if ( visibleIndex == targetVisibleIndex )
						return pointsOfInterest[i];

					visibleIndex++;
				}
			}
			
			return null;
		}



		public void ClearLookTarget()
		{
			eyeAndHeadAnimator.ClearLookTarget();
			nextChangePOITime = -1;
		}



		public void Initialize()
		{
			if ( isInitialized )
				return;

			eyeAndHeadAnimator = GetComponent<EyeAndHeadAnimator>();
			eyeAndHeadAnimator.Initialize();

			//*** Player eyes: either user main camera or VR cameras
			{
				#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
					useNativeVRSupport = useVR = UnityEngine.VR.VRDevice.isPresent && UnityEngine.VR.VRSettings.enabled;
				#else
					useNativeVRSupport = false;
				#endif

				if ( useNativeVRSupport )
				{
					GameObject ovrRigGO = GameObject.Find("OVRCameraRig");
					if ( ovrRigGO != null )
					{
						useVR = true;

						playerLeftEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "LeftEyeAnchor").transform;
						playerRightEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "RightEyeAnchor").transform;
						playerEyeCenterXform = Utils.FindChildInHierarchy(ovrRigGO, "CenterEyeAnchor").transform;
					}
					else if ( Camera.main == null && playerEyeCenter == null)
					{
						Debug.LogError("Main camera not found. Please set the main camera's tag to 'MainCamera' or set Player Eye Center.");
						useVR = false;
						useNativeVRSupport = false;
						lookAtPlayerRatio = 0;
					}
					else
					{
						createdPlayerEyeCenterGO = new GameObject("CreatedPlayerCenterVREye") { hideFlags = HideFlags.HideInHierarchy };
						createdPlayerLeftEyeGO = new GameObject("CreatedPlayerLeftVREye") { hideFlags = HideFlags.HideInHierarchy };
						createdPlayerRightEyeGO = new GameObject("CreatedPlayerRightVREye") { hideFlags = HideFlags.HideInHierarchy };

						playerEyeCenterXform = createdPlayerEyeCenterGO.transform;
						playerLeftEyeXform = createdPlayerLeftEyeGO.transform;
						playerRightEyeXform = createdPlayerRightEyeGO.transform;

						Transform playerXform = (playerEyeCenter != null) ? playerEyeCenter : Camera.main.transform;
						createdVRParentGO = new GameObject("Parent");
						createdVRParentGO.transform.position = playerXform.position;
						createdVRParentGO.transform.rotation = playerXform.rotation;
						createdVRParentGO.transform.parent = playerXform.parent;

						playerEyeCenterXform.parent = createdVRParentGO.transform;
						playerLeftEyeXform.parent = createdVRParentGO.transform;
						playerRightEyeXform.parent = createdVRParentGO.transform;

						UpdateNativeVRTransforms();
					}
				}
				else
				{
					//*** Check for Oculus
					{
						GameObject ovrRigGO = GameObject.Find("OVRCameraRig");
						if ( ovrRigGO != null )
						{
							useVR = true;

							playerLeftEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "LeftEyeAnchor").transform;
							playerRightEyeXform = Utils.FindChildInHierarchy(ovrRigGO, "RightEyeAnchor").transform;
							playerEyeCenterXform = Utils.FindChildInHierarchy(ovrRigGO, "CenterEyeAnchor").transform;
						}
					}
				}

				if ( false == useVR )
				{
					if ( playerEyeCenter != null )
						playerEyeCenterXform = playerEyeCenter;
					else if ( Camera.main != null )
						playerEyeCenterXform = Camera.main.transform;
					else
					{
						Debug.LogError("Main camera not found. Please set the main camera's tag to 'MainCamera' or set Player Eye Center.");
						lookAtPlayerRatio = 0;
					}
				}
			}

			nextChangePOITime = 0;
			isInitialized = true;
		}



		public bool IsPlayerInView()
		{
			UpdateCameraTransformIfNecessary();
			return (playerEyeCenterXform != null) && eyeAndHeadAnimator.IsInView( playerEyeCenterXform.position );
		}



		void LateUpdate()
		{
			if ( false == isInitialized )
				return;

			if (useNativeVRSupport)
				UpdateNativeVRTransforms();

			bool shouldLookBackAtPlayer = false;
			bool shouldNoticePlayer = false;
			bool shouldLookAwayFromPlayer = false;

			Vector3 playerTargetPos = playerEyeCenterXform.position;
			float distanceToPlayer = Vector3.Distance(eyeAndHeadAnimator.GetOwnEyeCenter(), playerTargetPos);
			bool isPlayerInView = eyeAndHeadAnimator.IsInView( playerEyeCenterXform.position );
			bool isPlayerInAwarenessZone = isPlayerInView && distanceToPlayer < noticePlayerDistance;
			bool isPlayerInPersonalSpace = isPlayerInView && distanceToPlayer < personalSpaceDistance;

			//*** Awareness zone
			{
				if ( isPlayerInAwarenessZone )
				{
					if ( Time.time - timeOfLastNoticeCheck > 0.1f && state != State.LookingAtPlayer )
					{
						timeOfLastNoticeCheck = Time.time;
					
						bool isPlayerApproaching = lastDistanceToPlayer > distanceToPlayer;
						float closenessFactor01 = (noticePlayerDistance - distanceToPlayer)/noticePlayerDistance;
						float noticeProbability = Mathf.Lerp (0.1f, 0.5f, closenessFactor01);
						shouldNoticePlayer = isPlayerApproaching && timeOutsideOfAwarenessZone > 1 && Random.value < noticeProbability; 
					}
				}
				else
					timeOutsideOfAwarenessZone += Time.deltaTime;
			}


			//*** Personal space
			{
				 if ( isPlayerInPersonalSpace )
				 {
					timeInsidePersonalSpace += Time.deltaTime * Mathf.Clamp01((personalSpaceDistance - distanceToPlayer)/(0.5f * personalSpaceDistance));
					const float kMinTimeInPersonalSpaceToLookAway = 1;
					if ( timeInsidePersonalSpace >= kMinTimeInPersonalSpaceToLookAway )
						shouldLookAwayFromPlayer = true;
				 }
				 else
					timeInsidePersonalSpace = 0;
			}


			//*** Look away from player?
			{
				if ( shouldLookAwayFromPlayer && state != State.LookingAwayFromPlayer )
				{
					LookAwayFromPlayer();

					return;
				}
			}


			//*** Finished looking at current target?
			{
				if ( nextChangePOITime >= 0 && Time.time >= nextChangePOITime && eyeAndHeadAnimator.CanChangePointOfAttention() )
				{
					if ( Random.value <= lookAtPlayerRatio && IsPlayerInView() )
						LookAtPlayer(Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime)));
					else
						LookAroundIdly();

					return;
				}
			}


			//*** If the player keeps staring at us, stare back?		
			{
				if ( stareBackFactor > 0 && playerEyeCenterXform != null )
				{
					float playerLookingAtMeAngle = eyeAndHeadAnimator.GetStareAngleTargetAtMe( playerEyeCenterXform );
					bool isPlayerLookingAtMe = playerLookingAtMeAngle < 15;
		
					playerLookingAtMeTime = (isPlayerInView && isPlayerLookingAtMe	)	? Mathf.Min(10, playerLookingAtMeTime + Mathf.Cos(Mathf.Deg2Rad * playerLookingAtMeAngle) * Time.deltaTime)
																														: Mathf.Max(0, playerLookingAtMeTime - Time.deltaTime);
			
					if ( false == eyeAndHeadAnimator.IsLookingAtFace() )
					{
						if ( stareBackDeadtime > 0 )
							stareBackDeadtime -= Time.deltaTime;
						
						if (	stareBackDeadtime <= 0  &&
							Time.time - timeOfLastLookBackCheck > 0.1f &&
							playerLookingAtMeTime > minLookAtMeTimeToReact  &&
							eyeAndHeadAnimator.CanChangePointOfAttention() &&
							isPlayerLookingAtMe )
						{
							timeOfLastLookBackCheck = Time.time;
							
							float lookTimeProbability = stareBackFactor * 2 * (Mathf.Min(10, playerLookingAtMeTime) - minLookAtMeTimeToReact) / (10-minLookAtMeTimeToReact);
							shouldLookBackAtPlayer = Random.value < lookTimeProbability;
						}
					}
				}
			}

			if ( shouldLookBackAtPlayer || shouldNoticePlayer )
				LookAtPlayer(Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime)));

			lastDistanceToPlayer = distanceToPlayer;

		}



		// To keep looking at the player until new command, set duration to -1
		public void LookAtPlayer(float duration=-1, float headLatency=0.075f)
		{
			UpdateCameraTransformIfNecessary();

			if ( playerLeftEyeXform != null && playerRightEyeXform	!= null )
				eyeAndHeadAnimator.LookAtFace( playerLeftEyeXform, playerRightEyeXform, headLatency );
			else
				eyeAndHeadAnimator.LookAtFace( playerEyeCenterXform, headLatency );
			
			nextChangePOITime = (duration >= 0) ? (Time.time + duration) : -1;

			targetPOI = null;
			timeOutsideOfAwarenessZone = 0;

			state = State.LookingAtPlayer;
		}
	
	
	
		public void LookAroundIdly()
		{
			if ( state == State.LookingAtPlayer )
				stareBackDeadtime = Random.Range(10.0f, 30.0f);
			
			targetPOI = ChooseNextHeadTargetPOI();

			if ( targetPOI != null )
				eyeAndHeadAnimator.LookAtAreaAround( targetPOI );
			else
				eyeAndHeadAnimator.LookAtAreaAround(ChooseNextHeadTargetPoint());

			nextChangePOITime = Time.time + Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime));
					
			state = State.LookingAroundIdly;
		}



		// To keep looking at the poi until new command, set duration to -1
		public void LookAtPoiDirectly( Transform poiXform, float duration=-1, float headLatency=0.075f )
		{
			eyeAndHeadAnimator.LookAtSpecificThing( poiXform, headLatency );
			nextChangePOITime = (duration >= 0) ? (Time.time + duration) : -1;
			state = State.LookingAtPoiDirectly;
		}
	
	
	
		// To keep looking at the poi until new command, set duration to -1
		public void LookAtPoiDirectly( Vector3 poi, float duration=-1, float headLatency=0.075f )
		{
			eyeAndHeadAnimator.LookAtSpecificThing( poi, headLatency: headLatency );
			nextChangePOITime = (duration >= 0) ? (Time.time + duration) : -1;
			state = State.LookingAtPoiDirectly;
		}
	
	
	
		void LookAwayFromPlayer()
		{
			stareBackDeadtime = Random.Range(5.0f, 10.0f);
			
			bool isPlayerOnMyLeft = eyeAndHeadAnimator.headParentXform.InverseTransformPoint( playerEyeCenterXform.position ).x < 0;
			Vector3 awayPoint = eyeAndHeadAnimator.headParentXform.TransformPoint( eyeAndHeadAnimator.GetOwnEyeCenter() + 10 * (Quaternion.Euler(0, isPlayerOnMyLeft ? 50 : -50, 0 ) * Vector3.forward));
			eyeAndHeadAnimator.LookAtAreaAround( awayPoint );

			nextChangePOITime = Time.time + Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime));

			state = State.LookingAwayFromPlayer;
		}



		void OnDestroy()
		{
			if ( createdVRParentGO != null )
				Destroy(createdVRParentGO);
		}



		void OnEnable()
		{
			if (false == isInitialized)
				Initialize();
		}



		void OnTargetLost()
		{
			if ( eyeAndHeadAnimator.CanChangePointOfAttention() )
			{
				float r = Random.value;
				if ( r <= lookAtPlayerRatio && IsPlayerInView() )
					LookAtPlayer(Random.Range(Mathf.Min(minLookTime, maxLookTime), Mathf.Max(minLookTime, maxLookTime)));
				else
					LookAroundIdly();
			}
		}



		void Start()
		{
			if ( false == isInitialized )
				Initialize();
		}


		void Update()
		{
			if ( false == isInitialized )
				return;

			if ( false == keepTargetEvenWhenLost && false == hasRegisteredTargetLostFunction )
			{
				eyeAndHeadAnimator.OnTargetLost += OnTargetLost;
				hasRegisteredTargetLostFunction = true;
			}
			else if ( keepTargetEvenWhenLost && hasRegisteredTargetLostFunction )
			{
				eyeAndHeadAnimator.OnTargetLost -= OnTargetLost;
				hasRegisteredTargetLostFunction = false;
			}
			
			UpdateCameraTransformIfNecessary();
		}



		void UpdateCameraTransformIfNecessary()
		{
			if ( false == useVR )
				playerEyeCenterXform = (playerEyeCenter != null) ? playerEyeCenter : Camera.main.transform;
		}



		void UpdateNativeVRTransforms()
		{
			#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
				if (useNativeVRSupport)
				{
					playerEyeCenterXform.localPosition = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye);
					playerLeftEyeXform.localPosition = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.LeftEye);
					playerRightEyeXform.localPosition = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.RightEye);

					playerEyeCenterXform.localRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
					playerLeftEyeXform.localRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.LeftEye);
					playerRightEyeXform.localRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.RightEye);
				}
			#endif
		}

	}




}