// EyeAndHeadAnimator.cs
// Tore Knabe
// Copyright 2016 ioccam@ioccam.com

// If you use FinalIK to move the head, uncomment the next line:
// #define USE_FINAL_IK

#if UNITY_4_6
	#if !UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_METRO
		#define SUPPORTS_SERIALIZATION
	#endif
#else
	#if !UNITY_WP8 && !UNITY_WP_8_1 && !UNITY_WSA
		#define SUPPORTS_SERIALIZATION
	#endif
#endif

using System;
#if SUPPORTS_SERIALIZATION
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace RealisticEyeMovements {

	[Serializable]
	public class EyeAndHeadAnimatorForExport
	{
		public float headSpeedModifier;
		public float headWeight;
		public bool useMicroSaccades;
		public bool useMacroSaccades;
		public bool kDrawSightlinesInEditor;
		public ControlData.ControlDataForExport controlData;
		public float kMinNextBlinkTime;
		public float kMaxNextBlinkTime;
		public bool eyelidsFollowEyesVertically;
		public float maxEyeHorizAngle;
		public float maxEyeHorizAngleTowardsNose;
		public float crossEyeCorrection;
		public float nervousness;
		public float limitHeadAngle;
	}


	public class EyeAndHeadAnimator : MonoBehaviour
	{
		#region fields

			const float kMaxLimitedHorizontalHeadAngle = 55;
			const float kMaxLimitedVerticalHeadAngle = 40;
			const float kMaxHorizViewAngle = 100;
			const float kMaxVertViewAngle = 60;

			public event System.Action OnTargetLost;


			[Tooltip("Increases or decreases head movement speed (1: normal)")]
			[Range(0.1f, 5)]
			public float headSpeedModifier = 1;

			[Tooltip("How much this component controls head direction.")]
			[Range(0, 1)]
			public float headWeight = 1;

			public bool useMicroSaccades = true;
			public bool useMacroSaccades = true;
			public bool kDrawSightlinesInEditor;

			[HideInInspector]
			public ControlData controlData = new ControlData();

			#region eye lids
				[Tooltip("Minimum seconds until next blink")]
				public float kMinNextBlinkTime = 3.0f;
				[Tooltip("Maximum seconds until next blink")]
				public float kMaxNextBlinkTime = 15.0f;
				
				[Tooltip("Whether the eyelids move up a bit when looking up and down when looking down.")]
				public bool eyelidsFollowEyesVertically = true;

				public float blink01 { get; private set; }		
	
				bool useUpperEyelids;
				bool useLowerEyelids;

				float timeOfNextBlink;
			
				enum BlinkState {
					Idle,
					Closing,
					KeepingClosed,
					Opening
				}
				BlinkState blinkState = BlinkState.Idle;
				float blinkStateTime;
				float blinkDuration;
				bool isShortBlink;
			
				const float kBlinkCloseTimeShort = 0.035f;
				const float kBlinkOpenTimeShort = 0.055f;
				const float kBlinkCloseTimeLong = 0.07f;
				const float kBlinkOpenTimeLong = 0.1f;
				const float kBlinkKeepingClosedTime = 0.008f;
			#endregion

			[Tooltip("Maximum horizontal eye angle (away from nose)")]
			public float maxEyeHorizAngle = 35;

			[Tooltip("Maximum horizontal eye angle towards nose")]
			public float maxEyeHorizAngleTowardsNose = 35;

			[Tooltip("Cross eye correction factor")]
			[Range(0, 5)]
			public float crossEyeCorrection = 1.0f;

			[Tooltip("The more nervous, the more often you do micro-and macrosaccades.")]
			[Range(0,10)]
			public float nervousness;

			[Tooltip("Limits the angle for the head movement")]
			[Range(0,1)]
			public float limitHeadAngle;

			public float eyeDistance { get; private set; }
			public float eyeDistanceScale { get; private set; }

			Transform leftEyeAnchor;
			Transform rightEyeAnchor;

			float leftMaxSpeedHoriz;
			float leftHorizDuration;
			float leftMaxSpeedVert;
			float leftVertDuration;
			float leftCurrentSpeedX;
			float leftCurrentSpeedY;

			float rightMaxSpeedHoriz;
			float rightHorizDuration;
			float rightMaxSpeedVert;
			float rightVertDuration;
			float rightCurrentSpeedX;
			float rightCurrentSpeedY;

			float startLeftEyeHorizDuration;
			float startLeftEyeVertDuration;
			float startLeftEyeMaxSpeedHoriz;
			float startLeftEyeMaxSpeedVert;

			float startRightEyeHorizDuration;
			float startRightEyeVertDuration;
			float startRightEyeMaxSpeedHoriz;
			float startRightEyeMaxSpeedVert;

			float timeOfEyeMovementStart;

			const float kMaxHeadVelocity = 2.9f;
			const float kHeadOmega = 3.5f;
			CritDampTweenQuaternion critDampTween;
			Vector3 headEulerSpeed;
			Vector3 lastHeadEuler;
			float maxHeadHorizSpeedSinceSaccadeStart;
			float maxHeadVertSpeedSinceSaccadeStart;
			bool isHeadTracking;
			float headTrackingFactor = 1;

			float headLatency;
			float eyeLatency;

			float ikWeight = 1;

			Animator animator;
			#if USE_FINAL_IK
				RootMotion.FinalIK.LookAtIK lookAtIK;
			#endif


			#region Transforms for target
				Transform currentHeadTargetPOI;
				Transform currentEyeTargetPOI;
				Transform nextHeadTargetPOI;
				Transform nextEyeTargetPOI;
				Transform currentTargetLeftEyeXform;
				Transform currentTargetRightEyeXform;
				Transform nextTargetLeftEyeXform;
				Transform nextTargetRightEyeXform;
				readonly Transform[] createdTargetXforms = new Transform[2];
				int createdTargetXformIndex;
			#endregion


			Transform eyesRootXform;
			public Transform headParentXform { get; private set; }
			Transform headTargetPivotXform;

			Quaternion leftEyeRootFromAnchorQ;
			Quaternion rightEyeRootFromAnchorQ;
			Quaternion leftAnchorFromEyeRootQ;
			Quaternion rightAnchorFromEyeRootQ;
			Vector3 currentLeftEyeLocalEuler;
			Vector3 currentRightEyeLocalEuler;
			Quaternion originalLeftEyeLocalQ;
			Quaternion originalRightEyeLocalQ;
			Quaternion lastLeftEyeLocalRotation;
			Quaternion lastRightEyeLocalQ;

			Vector3 macroSaccadeTargetLocal;
			Vector3 microSaccadeTargetLocal;

			float timeOfEnteringClearingPhase;
			float timeOfLastMacroSaccade = -100;
			float timeToMicroSaccade;
			float timeToMacroSaccade;

			bool isInitialized;
		
			public enum HeadSpeed
			{
				Slow,
				Fast
			}
			HeadSpeed headSpeed = HeadSpeed.Slow;

			public enum EyeDelay
			{
				Simultaneous,
				EyesFirst,
				HeadFirst
			}

			enum LookTarget
			{
				StraightAhead,
				ClearingTargetPhase1,
				ClearingTargetPhase2,
				GeneralDirection,
				SpecificThing,
				Face
			}
			LookTarget lookTarget = LookTarget.StraightAhead;

			enum FaceLookTarget
			{
				EyesCenter,
				LeftEye,
				RightEye,
				Mouth
			}
			FaceLookTarget faceLookTarget = FaceLookTarget.EyesCenter;

		#endregion



		void Awake()
		{
			if ( false == isInitialized )
				Initialize();
		}



		public void Blink( bool isShortBlink =true)
		{
			if ( blinkState != BlinkState.Idle )
				return;

			this.isShortBlink = isShortBlink;
			blinkState = BlinkState.Closing;
			blinkStateTime = 0;
			blinkDuration = isShortBlink ? kBlinkCloseTimeShort : kBlinkCloseTimeLong;
		}



		public bool CanGetIntoView(Vector3 point)
		{
			Vector3 targetLocalAngles = Quaternion.LookRotation( headParentXform.InverseTransformPoint( point ) ).eulerAngles;

			float x = Mathf.Abs(Utils.NormalizedDegAngle(targetLocalAngles.x));
			float y = Mathf.Abs(Utils.NormalizedDegAngle(targetLocalAngles.y));

			bool horizOk = y < (LimitHorizontalHeadAngle(kMaxLimitedHorizontalHeadAngle) + maxEyeHorizAngle + 0.2f * kMaxHorizViewAngle);

			float clampedEyeVertAngle = controlData.ClampRightVertEyeAngle(targetLocalAngles.x);
			bool vertOk = x < (LimitVerticalHeadAngle(kMaxLimitedVerticalHeadAngle) + Mathf.Abs(clampedEyeVertAngle) + 0.2f * kMaxVertViewAngle);
			
			return horizOk && vertOk;
		}



		public bool CanChangePointOfAttention()
		{
			return Time.time-timeOfLastMacroSaccade >= 2f * 1f/(1 + nervousness);
		}



		#if SUPPORTS_SERIALIZATION
			public bool CanImportFromFile(string filename)
			{
				EyeAndHeadAnimatorForExport import;
				using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					import = (EyeAndHeadAnimatorForExport) new BinaryFormatter().Deserialize(stream);
				}

				return controlData.CanImport(import.controlData, transform);
			}
		#endif



		// If eye latency is greater than zero, the head starts turning towards new target and the eyes keep looking at the old target for a while.
		// If head latency is greater than zero, the eyes look at the new target first and the head turns later.
		void CheckLatencies()
		{
			if ( eyeLatency > 0 )
			{
				eyeLatency -= Time.deltaTime;
				if ( eyeLatency <= 0 )
				{
					currentEyeTargetPOI = nextEyeTargetPOI;
					currentTargetLeftEyeXform = nextTargetLeftEyeXform;
					currentTargetRightEyeXform = nextTargetRightEyeXform;
					StartEyeMovement(currentEyeTargetPOI);
				}
			}
			else if ( headLatency > 0 )
			{
				headLatency -= Time.deltaTime;
				if ( headLatency <= 0 )
					StartHeadMovement( nextHeadTargetPOI );
			}
		}



		void CheckMacroSaccades()
		{
			if ( lookTarget == LookTarget.SpecificThing )
				return;

			if ( controlData.eyeControl == ControlData.EyeControl.None )
				return;

			if ( eyeLatency > 0 )
				return;

			timeToMacroSaccade -= Time.deltaTime;
			if ( timeToMacroSaccade <= 0 )
			{
				if ( lookTarget == LookTarget.GeneralDirection && useMacroSaccades)
				{
							const float kMacroSaccadeAngle = 10;
							bool hasBoneEyelidControl = controlData.eyelidControl == ControlData.EyelidControl.Bones;
							float angleVert = Random.Range(-kMacroSaccadeAngle * (hasBoneEyelidControl ? 0.65f : 0.3f), kMacroSaccadeAngle * (hasBoneEyelidControl ? 0.65f : 0.4f));
							float angleHoriz = Random.Range(-kMacroSaccadeAngle,kMacroSaccadeAngle);
					SetMacroSaccadeTarget( eyesRootXform.TransformPoint(	Quaternion.Euler( angleVert, angleHoriz, 0)
																												* eyesRootXform.InverseTransformPoint( GetCurrentEyeTargetPos() )));

					timeToMacroSaccade = Random.Range(5.0f, 8.0f);
					timeToMacroSaccade *= 1.0f/(1.0f + nervousness);
				}
				else if ( lookTarget == LookTarget.Face )
				{
					if ( currentEyeTargetPOI == null )
					{
						//*** Social triangle: saccade between eyes and mouth (or chest, if actor isn't looking back)
						{
							switch( faceLookTarget )
							{
								case FaceLookTarget.LeftEye:
									faceLookTarget = Random.value < 0.75f ? FaceLookTarget.RightEye : FaceLookTarget.Mouth;
									break;
								case FaceLookTarget.RightEye:
									faceLookTarget = Random.value < 0.75f ? FaceLookTarget.LeftEye : FaceLookTarget.Mouth;
									break;
								case FaceLookTarget.Mouth:
								case FaceLookTarget.EyesCenter:
									faceLookTarget = Random.value < 0.5f ? FaceLookTarget.LeftEye : FaceLookTarget.RightEye;
									break;
							}
							SetMacroSaccadeTarget( GetLookTargetPosForSocialTriangle( faceLookTarget ) );
							timeToMacroSaccade = (faceLookTarget == FaceLookTarget.Mouth)	? Random.Range(0.4f, 0.9f)
																																: Random.Range(1.0f, 3.0f);
							timeToMacroSaccade *= 1.0f/(1.0f + nervousness);
						}
					}
				}																																				
			}
		}



		void CheckMicroSaccades()
		{
			if ( false == useMicroSaccades )
				return;

			if ( controlData.eyeControl == ControlData.EyeControl.None )
				return;

			if ( eyeLatency > 0 )
				return;

			if ( lookTarget == LookTarget.GeneralDirection || lookTarget == LookTarget.SpecificThing || (lookTarget == LookTarget.Face && currentEyeTargetPOI != null) )
			{
				timeToMicroSaccade -= Time.deltaTime;
				if ( timeToMicroSaccade <= 0 )
				{
					const float kMicroSaccadeAngle = 3;
					bool hasBoneEyelidControl = controlData.eyelidControl == ControlData.EyelidControl.Bones;
					float angleVert = Random.Range(-kMicroSaccadeAngle * (hasBoneEyelidControl ? 0.8f : 0.5f), kMicroSaccadeAngle * (hasBoneEyelidControl ? 0.85f : 0.6f));
					float angleHoriz = Random.Range(-kMicroSaccadeAngle,kMicroSaccadeAngle);
					if ( lookTarget == LookTarget.Face )
					{
						angleVert *= 0.5f;
						angleHoriz *= 0.5f;
					}

					SetMicroSaccadeTarget ( eyesRootXform.TransformPoint(	Quaternion.Euler(angleVert, angleHoriz, 0)
																												* eyesRootXform.InverseTransformPoint( currentEyeTargetPOI.TransformPoint(macroSaccadeTargetLocal) )));
				}
			}
		}


		
		float ClampLeftHorizEyeAngle( float angle )
		{
			float normalizedAngle = Utils.NormalizedDegAngle(angle);
			bool isTowardsNose = normalizedAngle > 0;
			float maxAngle = isTowardsNose ? maxEyeHorizAngleTowardsNose : maxEyeHorizAngle;
			return Mathf.Clamp(normalizedAngle, -maxAngle, maxAngle);
		}



		float ClampRightHorizEyeAngle( float angle )
		{
			float normalizedAngle = Utils.NormalizedDegAngle(angle);
			bool isTowardsNose = normalizedAngle < 0;
			float maxAngle = isTowardsNose ? maxEyeHorizAngleTowardsNose : maxEyeHorizAngle;
			return Mathf.Clamp(normalizedAngle, -maxAngle, maxAngle);
		}



		//float ClampLeftVertEyeAngle( float angle )
		//{
		//	//if ( controlData.eyeControl == ControlData.EyeControl.MecanimEyeBones )
		//	//	return controlData.leftBoneEyeRotationLimiter.ClampAngle( angle );
		//	//else if ( controlData
		//	//return Mathf.Clamp(Utils.NormalizedDegAngle(angle), -controlData.maxEyeUpAngle, controlData.maxEyeDownAngle);
		//}



		public void ClearLookTarget()
		{
			LookAtAreaAround( GetOwnEyeCenter() + transform.forward * 1000 * eyeDistance );
			lookTarget = LookTarget.ClearingTargetPhase1;
			timeOfEnteringClearingPhase = Time.time;
		}



		void DrawSightlinesInEditor()
		{
			if ( controlData.eyeControl != ControlData.EyeControl.None )
			{
				Vector3 leftDirection = (leftEyeAnchor.parent.rotation * leftEyeAnchor.localRotation * leftAnchorFromEyeRootQ) * Vector3.forward;
				Vector3 rightDirection = (rightEyeAnchor.parent.rotation * rightEyeAnchor.localRotation * rightAnchorFromEyeRootQ) * Vector3.forward;
				Debug.DrawLine(leftEyeAnchor.position, leftEyeAnchor.position + leftDirection * 10 * eyeDistanceScale);
				Debug.DrawLine(rightEyeAnchor.position, rightEyeAnchor.position + rightDirection * 10 * eyeDistanceScale);
			}

			// Debug.DrawLine(eyesRootXform.position, eyesRootXform.position + GetOwnLookDirection() * 10  );
		}



		#if SUPPORTS_SERIALIZATION
			public void ExportToFile(string filename)
			{
				EyeAndHeadAnimatorForExport export = new EyeAndHeadAnimatorForExport
				{
					headSpeedModifier = headSpeedModifier,
					headWeight = headWeight,
					useMicroSaccades = useMicroSaccades,
					useMacroSaccades = useMacroSaccades,
					kDrawSightlinesInEditor = kDrawSightlinesInEditor,
					controlData = controlData.GetExport(transform),
					kMaxNextBlinkTime = kMaxNextBlinkTime,
					eyelidsFollowEyesVertically = eyelidsFollowEyesVertically,
					maxEyeHorizAngle = maxEyeHorizAngle,
					maxEyeHorizAngleTowardsNose = maxEyeHorizAngleTowardsNose,
					crossEyeCorrection = crossEyeCorrection,
					nervousness = nervousness,
					limitHeadAngle = limitHeadAngle,
				};

				FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
				new BinaryFormatter().Serialize(stream, export);
				stream.Close();
			}
		#endif



		Vector3 GetCurrentEyeTargetPos()
		{
			return ( currentEyeTargetPOI != null )	?	currentEyeTargetPOI.position
																	:	0.5f * ( currentTargetLeftEyeXform.position + currentTargetRightEyeXform.position );
		}



		Vector3 GetCurrentHeadTargetPos()
		{
			return ( currentHeadTargetPOI != null )	?	currentHeadTargetPOI.position
																		:	0.5f * ( currentTargetLeftEyeXform.position + currentTargetRightEyeXform.position );
		}



		Vector3 GetLookTargetPosForSocialTriangle( FaceLookTarget playerFaceLookTarget )
		{
			if ( currentTargetLeftEyeXform == null || currentTargetRightEyeXform == null )
				return currentEyeTargetPOI.position;

			Vector3 faceTargetPos = Vector3.zero;

			Vector3 eyeCenter = 0.5f * (currentTargetLeftEyeXform.position + currentTargetRightEyeXform.position);

			switch( playerFaceLookTarget )
			{
				case FaceLookTarget.EyesCenter:
					faceTargetPos = GetCurrentEyeTargetPos();
					break;
				case FaceLookTarget.LeftEye:
					faceTargetPos = Vector3.Lerp(eyeCenter, currentTargetLeftEyeXform.position, 0.75f);
					break;
				case FaceLookTarget.RightEye:
					faceTargetPos = Vector3.Lerp(eyeCenter, currentTargetRightEyeXform.position, 0.75f);
					break;
				case FaceLookTarget.Mouth:
					Vector3 eyeUp = 0.5f * (currentTargetLeftEyeXform.up + currentTargetRightEyeXform.up);
					faceTargetPos = eyeCenter - eyeUp * 0.4f * Vector3.Distance( currentTargetLeftEyeXform.position, currentTargetRightEyeXform.position );
					break;
			}

			return faceTargetPos;
		}



		public Vector3 GetOwnEyeCenter()
		 {
			return eyesRootXform.position;
		 }



		Vector3 GetOwnLookDirection()
		{
			return ( leftEyeAnchor != null && rightEyeAnchor != null )	?  (Quaternion.Slerp(	leftEyeAnchor.rotation * leftAnchorFromEyeRootQ,
																																rightEyeAnchor.rotation * rightAnchorFromEyeRootQ, 0.5f)) * Vector3.forward
																								:	eyesRootXform.forward;
		}



		public float GetStareAngleMeAtTarget( Vector3 target )
		{
			return Vector3.Angle(GetOwnLookDirection(), target - eyesRootXform.position);
		}



		public float GetStareAngleTargetAtMe( Transform targetXform )
		{
			return Vector3.Angle(targetXform.forward, GetOwnEyeCenter() - targetXform.position);
		}


	
		#if SUPPORTS_SERIALIZATION
			public void ImportFromFile(string filename)
			{
				if ( false == CanImportFromFile(filename) )
				{
					Debug.LogError(name + " cannot import from file");
					return;
				}

				EyeAndHeadAnimatorForExport import;
				using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					import = (EyeAndHeadAnimatorForExport) new BinaryFormatter().Deserialize(stream);
				}

				headSpeedModifier = import.headSpeedModifier;
				headWeight = import.headWeight;
				useMicroSaccades = import.useMicroSaccades;
				useMacroSaccades = import.useMacroSaccades;
				kDrawSightlinesInEditor = import.kDrawSightlinesInEditor;
				controlData.Import(import.controlData, transform);
				headSpeedModifier = import.headSpeedModifier;
				kMaxNextBlinkTime = import.kMaxNextBlinkTime;
				eyelidsFollowEyesVertically = import.eyelidsFollowEyesVertically;
				maxEyeHorizAngle = import.maxEyeHorizAngle;
				maxEyeHorizAngleTowardsNose = import.maxEyeHorizAngleTowardsNose;
				if ( maxEyeHorizAngleTowardsNose <= 0 )
					maxEyeHorizAngleTowardsNose = maxEyeHorizAngle;
				crossEyeCorrection = import.crossEyeCorrection;
				nervousness = import.nervousness;
				limitHeadAngle = import.limitHeadAngle;

				isInitialized = false;
			}
		#endif


		
		public void Initialize()
		{
			if ( isInitialized )
				return;

			if ( controlData == null )
				return;

			eyeDistance = 0.064f;
			animator = GetComponentInChildren<Animator>();
			#if USE_FINAL_IK
				lookAtIK = GetComponentInChildren<RootMotion.FinalIK.LookAtIK>();
			#endif

			controlData.CheckConsistency( animator, this );
			controlData.Initialize( );

			if ( createdTargetXforms[0] == null )
			{
				createdTargetXforms[0] = new GameObject(name + "_createdEyeTarget_1").transform;
				DestroyNotifier destroyNotifer = createdTargetXforms[0].gameObject.AddComponent<DestroyNotifier>();
				destroyNotifer.OnDestroyedEvent += OnCreatedXformDestroyed;
				DontDestroyOnLoad(createdTargetXforms[0].gameObject);
				createdTargetXforms[0].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}

			if ( createdTargetXforms[1] == null )
			{
				createdTargetXforms[1] = new GameObject(name + "_createdEyeTarget_2").transform;
				DestroyNotifier destroyNotifer = createdTargetXforms[1].gameObject.AddComponent<DestroyNotifier>();
				destroyNotifer.OnDestroyedEvent += OnCreatedXformDestroyed;
				DontDestroyOnLoad(createdTargetXforms[1].gameObject);
				createdTargetXforms[1].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}


			Transform headXform=null;
			if (animator != null)
				headXform = animator.GetBoneTransform(HumanBodyBones.Head);
			#if USE_FINAL_IK
				if ( headXform == null && lookAtIK != null )
					headXform = lookAtIK.solver.head.transform;
			#endif
			if ( headXform == null )
				headXform = transform;

			Transform spineXform = null;
			if ( animator != null )
			{
				spineXform = animator.GetBoneTransform(HumanBodyBones.Chest);
				if ( spineXform == null )
					spineXform = animator.GetBoneTransform(HumanBodyBones.Spine);
			}
			if ( spineXform == null )
				spineXform = transform;

			if ( headParentXform == null )
			{
				headParentXform = new GameObject(name + " head parent").transform;
				headParentXform.gameObject.hideFlags = HideFlags.HideInHierarchy;
				headParentXform.parent = spineXform;
				headParentXform.position = headXform.position;
				headParentXform.rotation = transform.rotation;
			}

			if ( headTargetPivotXform == null )
			{
				headTargetPivotXform = new GameObject(name + " head target").transform;
				headTargetPivotXform.gameObject.hideFlags = HideFlags.HideInHierarchy;
				headTargetPivotXform.parent = headParentXform;
				headTargetPivotXform.localPosition = Vector3.zero;
				headTargetPivotXform.localRotation = Quaternion.identity;

				critDampTween = new CritDampTweenQuaternion(headTargetPivotXform.localRotation, kHeadOmega, kMaxHeadVelocity);
 				lastHeadEuler = headTargetPivotXform.localEulerAngles;
			}

			//*** Eyes
			{
				if ( controlData.eyeControl == ControlData.EyeControl.MecanimEyeBones || controlData.eyeControl == ControlData.EyeControl.SelectedObjects )
				{
					if ( controlData.eyeControl == ControlData.EyeControl.MecanimEyeBones )
					{
						Transform leftEyeBoneXform = animator.GetBoneTransform(HumanBodyBones.LeftEye);
						Transform rightEyeBoneXform = animator.GetBoneTransform(HumanBodyBones.RightEye);
						leftEyeAnchor = leftEyeBoneXform;
						rightEyeAnchor = rightEyeBoneXform;
						if ( leftEyeAnchor == null )
							Debug.LogError("Left eye bone not found in Mecanim rig");
						if ( rightEyeAnchor == null )
							Debug.LogError("Right eye bone not found in Mecanim rig");
					}
					else if ( controlData.eyeControl == ControlData.EyeControl.SelectedObjects )
					{
						leftEyeAnchor = controlData.leftEye;
						rightEyeAnchor = controlData.rightEye;
					}
				}

				if ( eyesRootXform == null )
				{
					eyesRootXform = new GameObject(name + "_eyesRoot").transform;
					eyesRootXform.gameObject.hideFlags = HideFlags.HideInHierarchy;
					eyesRootXform.rotation = transform.rotation;
				}

				if ( leftEyeAnchor != null && rightEyeAnchor != null )
				{
					eyeDistance = Vector3.Distance( leftEyeAnchor.position, rightEyeAnchor.position );
					eyeDistanceScale = eyeDistance/0.064f;
					controlData.RestoreDefault();
					Quaternion inverse = Quaternion.Inverse(eyesRootXform.rotation);
					leftEyeRootFromAnchorQ = inverse * leftEyeAnchor.rotation;
					rightEyeRootFromAnchorQ = inverse * rightEyeAnchor.rotation;
					leftAnchorFromEyeRootQ = Quaternion.Inverse(leftEyeRootFromAnchorQ);
					rightAnchorFromEyeRootQ = Quaternion.Inverse(rightEyeRootFromAnchorQ);

					originalLeftEyeLocalQ = leftEyeAnchor.localRotation;
					originalRightEyeLocalQ = rightEyeAnchor.localRotation;

					eyesRootXform.position = 0.5f * (leftEyeAnchor.position + rightEyeAnchor.position);
					Transform commonAncestorXform = Utils.GetCommonAncestor( leftEyeAnchor, rightEyeAnchor );
					eyesRootXform.parent =  (commonAncestorXform != null) ? commonAncestorXform : leftEyeAnchor.parent;
				}
				else if ( animator != null )
				{
					if ( headXform != null )
					{
						eyesRootXform.position = headXform.position;
						eyesRootXform.parent = headXform;
					}
					else
					{
						eyesRootXform.position = transform.position;
						eyesRootXform.parent = transform;
					}
				}
				else
				{
					eyesRootXform.position = transform.position;
					eyesRootXform.parent = transform;
				}
			}


			//*** Eye lids
			{
				if ( controlData.eyelidControl == ControlData.EyelidControl.Bones )
				{
					if ( controlData.upperEyeLidLeft != null && controlData.upperEyeLidRight != null )
						useUpperEyelids = true;

					if ( controlData.lowerEyeLidLeft != null && controlData.lowerEyeLidRight != null )
						useLowerEyelids = true;
				}

				blink01 = 0;
				timeOfNextBlink = Time.time + Random.Range(3f, 6f);
				ikWeight = headWeight;
			}

			isInitialized = true;
		}



		public bool IsInView( Vector3 target )
		{
			if ( leftEyeAnchor == null || rightEyeAnchor == null )
			{
							Vector3 localAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection(target - GetOwnEyeCenter())).eulerAngles;
							float vertAngle = Utils.NormalizedDegAngle(localAngles.x);
							float horizAngle = Utils.NormalizedDegAngle(localAngles.y);
				bool seesTarget = Mathf.Abs(vertAngle) <= kMaxVertViewAngle && Mathf.Abs(horizAngle) <= kMaxHorizViewAngle;

				return seesTarget;
			}
			else
			{
							Vector3 localAnglesLeft = (leftEyeRootFromAnchorQ * Quaternion.Inverse(leftEyeAnchor.rotation) * Quaternion.LookRotation(target - leftEyeAnchor.position, leftEyeAnchor.up)).eulerAngles;
							float vertAngleLeft = Utils.NormalizedDegAngle(localAnglesLeft.x);
							float horizAngleLeft = Utils.NormalizedDegAngle(localAnglesLeft.y);
				bool leftEyeSeesTarget = Mathf.Abs(vertAngleLeft) <= kMaxVertViewAngle && Mathf.Abs(horizAngleLeft) <= kMaxHorizViewAngle;

							Vector3 localAnglesRight = (rightEyeRootFromAnchorQ * Quaternion.Inverse(rightEyeAnchor.rotation) * Quaternion.LookRotation(target - rightEyeAnchor.position, rightEyeAnchor.up)).eulerAngles;
							float vertAngleRight = Utils.NormalizedDegAngle(localAnglesRight.x);
							float horizAngleRight = Utils.NormalizedDegAngle(localAnglesRight.y);
				bool rightEyeSeesTarget = Mathf.Abs(vertAngleRight) <= kMaxVertViewAngle && Mathf.Abs(horizAngleRight) <= kMaxHorizViewAngle;

				return leftEyeSeesTarget || rightEyeSeesTarget;
			}
		}



		public bool IsLookingAtFace()
		{
			return lookTarget == LookTarget.Face;
		}
	
	
	
		void LateUpdate()
		{
			if ( false == isInitialized )
				return;

			if ( lookTarget == LookTarget.StraightAhead )
				return;


			#if USE_FINAL_IK
				if ( lookAtIK != null )
				{
					float targetIKWeight = (lookTarget == LookTarget.StraightAhead || lookTarget == LookTarget.ClearingTargetPhase2 ||lookTarget == LookTarget.ClearingTargetPhase1 ) ? 0 : headWeight;
					ikWeight = Mathf.Lerp( ikWeight, targetIKWeight, Time.deltaTime);
					lookAtIK.solver.IKPositionWeight = ikWeight;
					lookAtIK.solver.IKPosition = headTargetPivotXform.TransformPoint( eyeDistanceScale * Vector3.forward );
				}
			#endif

			if ( controlData.eyeControl != ControlData.EyeControl.None )
			{
				CheckMicroSaccades();
				CheckMacroSaccades();

				Transform trans = (currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform;
				if (trans != null && OnTargetLost != null && false == CanGetIntoView(trans.TransformPoint(macroSaccadeTargetLocal)) && eyeLatency <= 0)
					OnTargetLost();
			}

			UpdateHeadMovement();
			if ( controlData.eyeControl != ControlData.EyeControl.None )
				UpdateEyeMovement();
			UpdateBlinking();
			UpdateEyelids();

			if ( kDrawSightlinesInEditor )
				DrawSightlinesInEditor();
		}



		float LimitHorizontalHeadAngle( float headAngle )
		{
			const float kMaxUnlimitedHeadAngle = 90;
			float maxLimitedHeadAngle = Mathf.Lerp(kMaxLimitedHorizontalHeadAngle, 0, limitHeadAngle);
			const float kExponent = 1.5f;

			headAngle = Utils.NormalizedDegAngle(headAngle);
			float absAngle = Mathf.Abs(headAngle);
			float limitedAngle = Mathf.Sign(headAngle) *
										(absAngle - (kMaxUnlimitedHeadAngle-maxLimitedHeadAngle)/Mathf.Pow(kMaxUnlimitedHeadAngle, kExponent) * Mathf.Pow(absAngle, kExponent));

			return limitedAngle;
		}



		float LimitVerticalHeadAngle( float headAngle )
		{
			const float kMaxUnlimitedHeadAngle = 50;
			float maxLimitedHeadAngle = Mathf.Lerp(kMaxLimitedVerticalHeadAngle, 0, limitHeadAngle);
			const float kExponent = 1.5f;

			headAngle = Utils.NormalizedDegAngle(headAngle);
			float absAngle = Mathf.Abs(headAngle);
			float limitedAngle = Mathf.Sign(headAngle) *
										(absAngle - (kMaxUnlimitedHeadAngle-maxLimitedHeadAngle)/Mathf.Pow(kMaxUnlimitedHeadAngle, kExponent) * Mathf.Pow(absAngle, kExponent));

			return limitedAngle;
		}



		public void LookAtFace( Transform eyeCenterXform, float headLatency=0.075f )
		{
			lookTarget = LookTarget.Face;
			headSpeed = HeadSpeed.Fast;
			faceLookTarget = FaceLookTarget.EyesCenter;
			nextHeadTargetPOI = eyeCenterXform;
			this.headLatency = headLatency;
			currentTargetLeftEyeXform = currentTargetRightEyeXform = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			StartEyeMovement( eyeCenterXform );
		}



		public void LookAtFace(	Transform leftEyeXform,
											Transform rightEyeXform,
											float headLatency=0.075f )
		{
			lookTarget = LookTarget.Face;
			headSpeed = HeadSpeed.Fast;
			faceLookTarget = FaceLookTarget.EyesCenter;
			this.headLatency = headLatency;
			currentTargetLeftEyeXform = leftEyeXform;
			currentTargetRightEyeXform = rightEyeXform;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;
			nextHeadTargetPOI = null;

			StartEyeMovement( );
		}



		public void LookAtSpecificThing( Transform poi, float headLatency=0.075f )
		{
			lookTarget = LookTarget.SpecificThing;
			headSpeed = HeadSpeed.Fast;
			this.headLatency = headLatency;
			nextHeadTargetPOI = poi;
			currentTargetLeftEyeXform = currentTargetRightEyeXform = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			StartEyeMovement( poi );
		}



		public void LookAtSpecificThing( Vector3 point, float headLatency=0.075f )
		{
			createdTargetXformIndex = (createdTargetXformIndex+1) % createdTargetXforms.Length;
			createdTargetXforms[createdTargetXformIndex].position = point;
			LookAtSpecificThing( createdTargetXforms[createdTargetXformIndex], headLatency );
		}



		public void LookAtAreaAround( Transform poi )
		{
			lookTarget = LookTarget.GeneralDirection;
			headSpeed = HeadSpeed.Slow;
			eyeLatency = Random.Range(0.05f, 0.1f);

			nextEyeTargetPOI = poi;
			currentTargetLeftEyeXform = currentTargetRightEyeXform = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			StartHeadMovement( poi );
		}



		public void LookAtAreaAround( Vector3 point )
		{
			createdTargetXformIndex = (createdTargetXformIndex+1) % createdTargetXforms.Length;
			createdTargetXforms[createdTargetXformIndex].position = point;
			LookAtAreaAround( createdTargetXforms[createdTargetXformIndex] );
		}



		void OnAnimatorIK()
		{
			#if USE_FINAL_IK
				if ( lookAtIK != null )
					return;
			#endif

			if ( headWeight <= 0 )
				return;

						float targetIKWeight = (lookTarget == LookTarget.StraightAhead || lookTarget == LookTarget.ClearingTargetPhase2 || lookTarget == LookTarget.ClearingTargetPhase1 ) ? 0 : headWeight;
					ikWeight = Mathf.Lerp( ikWeight, targetIKWeight, Time.deltaTime);		
			animator.SetLookAtWeight(1, 0.01f, ikWeight);
			animator.SetLookAtPosition(headTargetPivotXform.TransformPoint( eyeDistanceScale * Vector3.forward ));
		}



		void OnCreatedXformDestroyed( DestroyNotifier destroyNotifer )
		{
			Transform destroyedXform = destroyNotifer.GetComponent<Transform>();

			for (int i=0;  i<createdTargetXforms.Length; i++)
				if ( createdTargetXforms[i] == destroyedXform )
					createdTargetXforms[i] = null;
		}



		void OnDestroy()
		{
			foreach ( Transform createdXform in createdTargetXforms )
			{
				if ( createdXform != null )
				{
					createdXform.GetComponent<DestroyNotifier>().OnDestroyedEvent -= OnCreatedXformDestroyed;
					Destroy( createdXform.gameObject );
				}
			}
		}



		void OnEnable()
		{
			if ( false == isInitialized )
				Initialize();
		}



		void SetMacroSaccadeTarget( Vector3 targetGlobal )
		{	
			macroSaccadeTargetLocal = ((currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform).InverseTransformPoint( targetGlobal );

			timeOfLastMacroSaccade = Time.time;

			SetMicroSaccadeTarget( targetGlobal );
			timeToMicroSaccade += 0.75f;
		}



		void SetMicroSaccadeTarget( Vector3 targetGlobal )
		{
			microSaccadeTargetLocal = ((currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform).InverseTransformPoint( targetGlobal );

			Vector3 targetLeftEyeLocalAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( targetGlobal - leftEyeAnchor.position)).eulerAngles;

					targetLeftEyeLocalAngles = new Vector3(	controlData.ClampLeftVertEyeAngle(targetLeftEyeLocalAngles.x),
																				ClampLeftHorizEyeAngle(targetLeftEyeLocalAngles.y),
																				targetLeftEyeLocalAngles.z);

			float leftHorizDistance = Mathf.Abs(Mathf.DeltaAngle(currentLeftEyeLocalEuler.y, targetLeftEyeLocalAngles.y));

					// From "Realistic Avatar and Head Animation Using a Neurobiological Model of Visual Attention", Itti, Dhavale, Pighin
			leftMaxSpeedHoriz = 473 * (1 - Mathf.Exp(-leftHorizDistance/7.8f));

					// From "Eyes Alive", Lee, Badler
					const float D0 = 0.025f;
					const float d = 0.00235f;
			leftHorizDuration = D0 + d * leftHorizDistance;

			float leftVertDistance = Mathf.Abs(Mathf.DeltaAngle(currentLeftEyeLocalEuler.x, targetLeftEyeLocalAngles.x));
			leftMaxSpeedVert = 473 * (1 - Mathf.Exp(-leftVertDistance/7.8f));
			leftVertDuration = D0 + d * leftVertDistance;


			Vector3 targetRightEyeLocalAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( targetGlobal - rightEyeAnchor.position)).eulerAngles;

						targetRightEyeLocalAngles = new Vector3(	controlData.ClampRightVertEyeAngle(targetRightEyeLocalAngles.x),
																						ClampRightHorizEyeAngle(targetRightEyeLocalAngles.y),
																						targetRightEyeLocalAngles.z);

			float rightHorizDistance = Mathf.Abs(Mathf.DeltaAngle(currentRightEyeLocalEuler.y, targetRightEyeLocalAngles.y));
			rightMaxSpeedHoriz = 473 * (1 - Mathf.Exp(-rightHorizDistance/7.8f));
			rightHorizDuration = D0 + d * rightHorizDistance;

			float rightVertDistance = Mathf.Abs(Mathf.DeltaAngle(currentRightEyeLocalEuler.x, targetRightEyeLocalAngles.x));
			rightMaxSpeedVert = 473 * (1 - Mathf.Exp(-rightVertDistance/7.8f));
			rightVertDuration = D0 + d * rightVertDistance;

			leftMaxSpeedHoriz = rightMaxSpeedHoriz = Mathf.Max( leftMaxSpeedHoriz, rightMaxSpeedHoriz );
			leftMaxSpeedVert = rightMaxSpeedVert = Mathf.Max( leftMaxSpeedVert, rightMaxSpeedVert );
			leftHorizDuration = rightHorizDuration = Mathf.Max( leftHorizDuration, rightHorizDuration );
			leftVertDuration = rightVertDuration = Mathf.Max( leftVertDuration, rightVertDuration );

			timeToMicroSaccade = Random.Range(0.8f, 1.75f);
			timeToMicroSaccade *= 1.0f/(1.0f + 0.4f * nervousness);

			//*** Blink if eyes move enough
			{
				if ( useUpperEyelids || useLowerEyelids || controlData.eyelidControl == ControlData.EyelidControl.Blendshapes )
				{
					float distance = Mathf.Max(leftHorizDistance, Mathf.Max(rightHorizDistance, Mathf.Max(leftVertDistance, rightVertDistance)));
					const float kMinBlinkDistance = 25.0f;
					if ( distance >= kMinBlinkDistance )
						Blink( isShortBlink: false );
				}
			}

			//*** For letting the eyes keep tracking the target after they saccaded to it
			{
				startLeftEyeHorizDuration = leftHorizDuration;
				startLeftEyeVertDuration = leftVertDuration;
				startLeftEyeMaxSpeedHoriz = leftMaxSpeedHoriz;
				startLeftEyeMaxSpeedVert = leftMaxSpeedVert;

				startRightEyeHorizDuration = rightHorizDuration;
				startRightEyeVertDuration = rightVertDuration;
				startRightEyeMaxSpeedHoriz = rightMaxSpeedHoriz;
				startRightEyeMaxSpeedVert = rightMaxSpeedVert;

				timeOfEyeMovementStart = Time.time;
			}

		}



		void StartEyeMovement( Transform targetXform=null )
		{
			eyeLatency = 0;
			currentEyeTargetPOI = targetXform;
			nextEyeTargetPOI = null;
			nextTargetLeftEyeXform = nextTargetRightEyeXform = null;

			if ( controlData.eyeControl != ControlData.EyeControl.None )
			{
				SetMacroSaccadeTarget ( GetCurrentEyeTargetPos() );
				timeToMacroSaccade = Random.Range(1.5f, 2.5f);
				timeToMacroSaccade *= 1.0f/(1.0f + nervousness);
			}

			if ( currentHeadTargetPOI == null )
				currentHeadTargetPOI = currentEyeTargetPOI;
		}



		void StartHeadMovement( Transform targetXform=null )
		{
			headLatency = 0;
			currentHeadTargetPOI = targetXform;
			nextHeadTargetPOI = null;

			//*** For letting the head keep tracking the target after orienting towards it
			{
				maxHeadHorizSpeedSinceSaccadeStart = maxHeadVertSpeedSinceSaccadeStart = 0;
				isHeadTracking = false;
				headTrackingFactor = 1;
			}

			if ( currentEyeTargetPOI == null && currentTargetLeftEyeXform == null )
				currentEyeTargetPOI = currentHeadTargetPOI;
		}




		void Update()
		{
			if ( false == isInitialized )
				return;

			CheckLatencies();
		}



		void UpdateBlinking()
		{
			if ( blinkState != BlinkState.Idle )
			{
				blinkStateTime += Time.deltaTime;
			
				if ( blinkStateTime >= blinkDuration )
				{
					blinkStateTime = 0;

					if ( blinkState == BlinkState.Closing )
					{
						if ( isShortBlink )
						{
							blinkState = BlinkState.Opening;
							blinkDuration = isShortBlink ? kBlinkOpenTimeShort : kBlinkOpenTimeLong;
							blink01 = 1;
						}
						else
						{
							blinkState = BlinkState.KeepingClosed;
							blinkDuration = kBlinkKeepingClosedTime;
							blink01 = 1;
						}
					}
					else if ( blinkState == BlinkState.KeepingClosed )
					{
						blinkState = BlinkState.Opening;
						blinkDuration = isShortBlink ? kBlinkOpenTimeShort : kBlinkOpenTimeLong;
					}
					else if ( blinkState == BlinkState.Opening )
					{
						blinkState = BlinkState.Idle;
						float minTime = Mathf.Max( 0.1f, Mathf.Min(kMinNextBlinkTime, kMaxNextBlinkTime));
						float maxTime = Mathf.Max( 0.1f, Mathf.Max(kMinNextBlinkTime, kMaxNextBlinkTime));
						timeOfNextBlink = Time.time + Random.Range( minTime, maxTime);
						blink01 = 0;
					}
				}
				else
					blink01 = Utils.EaseSineIn(	blinkStateTime,
																blinkState == BlinkState.Closing ? 0 : 1,
																blinkState == BlinkState.Closing ? 1 : -1,
																blinkDuration );
			}
		
		
			if ( Time.time >= timeOfNextBlink && blinkState == BlinkState.Idle )
				Blink();
		}



		void UpdateEyelids()
		{
			if ( controlData.eyelidControl == ControlData.EyelidControl.Bones || controlData.eyelidControl == ControlData.EyelidControl.Blendshapes )
				controlData.UpdateEyelids( currentLeftEyeLocalEuler.x, currentRightEyeLocalEuler.x, blink01, eyelidsFollowEyesVertically );
		}



		void UpdateEyeMovement()
		{
			if ( lookTarget == LookTarget.ClearingTargetPhase2 )
			{
				if ( Time.time - timeOfEnteringClearingPhase >= 1 )
					lookTarget = LookTarget.StraightAhead;
				else
				{
					leftEyeAnchor.localRotation = lastLeftEyeLocalRotation = Quaternion.Slerp(lastLeftEyeLocalRotation, originalLeftEyeLocalQ, Time.deltaTime);
					rightEyeAnchor.localRotation = lastRightEyeLocalQ = Quaternion.Slerp(lastRightEyeLocalQ, originalRightEyeLocalQ, Time.deltaTime);
				}

				return;
			}

			if ( lookTarget == LookTarget.ClearingTargetPhase1 )
			{
				if ( Time.time - timeOfEnteringClearingPhase >= 2 )
				{
					lookTarget = LookTarget.ClearingTargetPhase2;
					timeOfEnteringClearingPhase = Time.time;
				}
			}
		
			bool isLookingAtFace = lookTarget == LookTarget.Face;
			bool shouldDoSocialTriangle =		isLookingAtFace &&
															faceLookTarget != FaceLookTarget.EyesCenter;
			Transform trans = (currentEyeTargetPOI != null) ? currentEyeTargetPOI : currentTargetLeftEyeXform;
			Vector3 eyeTargetGlobal = shouldDoSocialTriangle	? GetLookTargetPosForSocialTriangle( faceLookTarget )
																						: trans.TransformPoint(microSaccadeTargetLocal);

			//*** Prevent cross-eyes
			{
				Vector3 ownEyeCenter = GetOwnEyeCenter();
				Vector3 eyeCenterToTarget = eyeTargetGlobal - ownEyeCenter;
				float distance = eyeCenterToTarget.magnitude / eyeDistanceScale;
				float corrDistMax = isLookingAtFace ? 2f : 0.6f;
				float corrDistMin = isLookingAtFace ? 1.5f : 0.2f;
						
				if ( distance < corrDistMax )
				{
					float modifiedDistance = corrDistMin + distance * (corrDistMax-corrDistMin)/corrDistMax;
					modifiedDistance = crossEyeCorrection * (modifiedDistance-distance) + distance;
					eyeTargetGlobal = ownEyeCenter + eyeDistanceScale * modifiedDistance * (eyeCenterToTarget/distance);
				}
			}

					
			//*** After the eyes saccaded to the new POI, adjust eye duration and speed so they keep tracking the target quickly enough.
			{
				const float kEyeDurationForTracking = 0.005f;
				const float kEyeMaxSpeedForTracking = 600;

				float timeSinceLeftEyeHorizInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startLeftEyeHorizDuration);
				if ( timeSinceLeftEyeHorizInitiatedMovementStop > 0 )
				{
					leftHorizDuration = kEyeDurationForTracking + startLeftEyeHorizDuration/(1 + timeSinceLeftEyeHorizInitiatedMovementStop);
					leftMaxSpeedHoriz = kEyeMaxSpeedForTracking - startLeftEyeMaxSpeedHoriz/(1 + timeSinceLeftEyeHorizInitiatedMovementStop);
				}

				float timeSinceLeftEyeVertInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startLeftEyeVertDuration);
				if ( timeSinceLeftEyeVertInitiatedMovementStop > 0 )
				{
					leftVertDuration = kEyeDurationForTracking + startLeftEyeVertDuration/(1 + timeSinceLeftEyeVertInitiatedMovementStop);
					leftMaxSpeedVert = kEyeMaxSpeedForTracking - startLeftEyeMaxSpeedVert/(1 + timeSinceLeftEyeVertInitiatedMovementStop);
				}

				float timeSinceRightEyeHorizInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startRightEyeHorizDuration);
				if ( timeSinceRightEyeHorizInitiatedMovementStop > 0 )
				{
					rightHorizDuration = kEyeDurationForTracking + startRightEyeHorizDuration/(1 + timeSinceRightEyeHorizInitiatedMovementStop);
					rightMaxSpeedHoriz = kEyeMaxSpeedForTracking - startRightEyeMaxSpeedHoriz/(1 + timeSinceRightEyeHorizInitiatedMovementStop);
				}

				float timeSinceRightEyeVertInitiatedMovementStop = Time.time-(timeOfEyeMovementStart + 1.5f * startRightEyeVertDuration);
				if ( timeSinceRightEyeVertInitiatedMovementStop > 0 )
				{
					rightVertDuration = kEyeDurationForTracking + startRightEyeVertDuration/(1 + timeSinceRightEyeVertInitiatedMovementStop);
					rightMaxSpeedVert = kEyeMaxSpeedForTracking - startRightEyeMaxSpeedVert/(1 + timeSinceRightEyeVertInitiatedMovementStop);
				}
			}

					Vector3 leftEyeTargetAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( eyeTargetGlobal - leftEyeAnchor.position )).eulerAngles;
					leftEyeTargetAngles = new Vector3(controlData.ClampLeftVertEyeAngle(leftEyeTargetAngles.x),
																		ClampLeftHorizEyeAngle(leftEyeTargetAngles.y),
																		0);
					float deltaTime = Mathf.Max(0.0001f, Time.deltaTime);
					float headMaxSpeedHoriz = 4*maxHeadHorizSpeedSinceSaccadeStart * Mathf.Sign(headEulerSpeed.y);
					float headMaxSpeedVert = 4*maxHeadVertSpeedSinceSaccadeStart * Mathf.Sign(headEulerSpeed.x);

			currentLeftEyeLocalEuler = new Vector3(	controlData.ClampLeftVertEyeAngle(Mathf.SmoothDampAngle(	currentLeftEyeLocalEuler.x,
																																			leftEyeTargetAngles.x,
																																			ref leftCurrentSpeedX,
																																			leftVertDuration,
																																			Mathf.Max(headMaxSpeedVert, leftMaxSpeedVert),
																																			deltaTime)),
																		ClampLeftHorizEyeAngle(Mathf.SmoothDampAngle(	currentLeftEyeLocalEuler.y,
																																					leftEyeTargetAngles.y,
																																					ref leftCurrentSpeedY,
																																					leftHorizDuration,
																																					Mathf.Max(headMaxSpeedHoriz, leftMaxSpeedHoriz),
																																					deltaTime)),
																		leftEyeTargetAngles.z);

			leftEyeAnchor.localRotation = Quaternion.Inverse(leftEyeAnchor.parent.rotation) * eyesRootXform.rotation * Quaternion.Euler( currentLeftEyeLocalEuler ) * leftEyeRootFromAnchorQ;

					Vector3 rightEyeTargetAngles = Quaternion.LookRotation(eyesRootXform.InverseTransformDirection( eyeTargetGlobal - rightEyeAnchor.position)).eulerAngles;
					rightEyeTargetAngles = new Vector3(	controlData.ClampRightVertEyeAngle(rightEyeTargetAngles.x),
																			ClampRightHorizEyeAngle(rightEyeTargetAngles.y),
																			0);
			currentRightEyeLocalEuler= new Vector3( controlData.ClampRightVertEyeAngle(Mathf.SmoothDampAngle(	currentRightEyeLocalEuler.x,
																																			rightEyeTargetAngles.x,
																																			ref rightCurrentSpeedX,
																																			rightVertDuration,
																																			Mathf.Max(headMaxSpeedVert, rightMaxSpeedVert),
																																			deltaTime)),
																		ClampRightHorizEyeAngle(Mathf.SmoothDampAngle(currentRightEyeLocalEuler.y,
																																					rightEyeTargetAngles.y,
																																					ref rightCurrentSpeedY,
																																					rightHorizDuration,
																																					Mathf.Max(headMaxSpeedHoriz, rightMaxSpeedHoriz),
																																					deltaTime)),
																		rightEyeTargetAngles.z);

			rightEyeAnchor.localRotation = Quaternion.Inverse(rightEyeAnchor.parent.rotation) * eyesRootXform.rotation * Quaternion.Euler( currentRightEyeLocalEuler ) * rightEyeRootFromAnchorQ;

			lastLeftEyeLocalRotation = leftEyeAnchor.localRotation;
			lastRightEyeLocalQ = rightEyeAnchor.localRotation;
		}



		void UpdateHeadMovement()
		{
			if ( ikWeight <= 0 )
				return;

					Vector3 targetLocalAngles = Quaternion.LookRotation( headParentXform.InverseTransformPoint( GetCurrentHeadTargetPos() ) ).eulerAngles;
			targetLocalAngles = new Vector3(	LimitVerticalHeadAngle(targetLocalAngles.x),
																LimitHorizontalHeadAngle(targetLocalAngles.y),
																0 );

			//*** After the head moved to the new POI, adjust head duration so the head keeps tracking the target quickly enough.
			{
				const float kMinAngleToStartTracking = 2;
				if ( false == isHeadTracking )
				{
					Vector3 tweenEuler = critDampTween.rotation.eulerAngles;
					isHeadTracking =	Mathf.Abs(Mathf.DeltaAngle(tweenEuler.x, targetLocalAngles.x)) < kMinAngleToStartTracking &&
												Mathf.Abs(Mathf.DeltaAngle(tweenEuler.y, targetLocalAngles.y)) < kMinAngleToStartTracking;
				}

				float headSpeedFactor = (headSpeed == HeadSpeed.Slow) ? 0.5f : 1.0f;
				float targetHeadTrackingFactor = isHeadTracking ? 5 : 1;
				headTrackingFactor = Mathf.Lerp(headTrackingFactor, targetHeadTrackingFactor, Time.deltaTime * 3);
				critDampTween.omega = headSpeedFactor * headSpeedModifier * headTrackingFactor * kHeadOmega;
			}

			critDampTween.Step(Quaternion.Euler(targetLocalAngles));

			float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
			headTargetPivotXform.localEulerAngles = critDampTween.rotation.eulerAngles;
			headEulerSpeed = (headTargetPivotXform.localEulerAngles - lastHeadEuler)/deltaTime;
			lastHeadEuler = headTargetPivotXform.localEulerAngles;

			maxHeadHorizSpeedSinceSaccadeStart = Mathf.Max(maxHeadHorizSpeedSinceSaccadeStart, Mathf.Abs(headEulerSpeed.y));
			maxHeadVertSpeedSinceSaccadeStart = Mathf.Max(maxHeadHorizSpeedSinceSaccadeStart, Mathf.Abs(headEulerSpeed.x));
		}
	}
}