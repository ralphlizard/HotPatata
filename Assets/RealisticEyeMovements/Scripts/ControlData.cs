// ControlData.cs
// Tore Knabe
// Copyright 2015 ioccam@ioccam.com


using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;


namespace RealisticEyeMovements {

	[System.Serializable]
	public class EyeRotationLimiter
	{

		[System.Serializable]
		public class EyeRotationLimiterForExport
		{
			public string transformPath;
			public SerializableQuaternion defaultQ;
			public SerializableQuaternion lookUpQ;
			public SerializableQuaternion lookDownQ;

			public bool isLookUpSet;
			public bool isLookDownSet;
		}

		#region fields

			[SerializeField] Transform transform;
			[SerializeField] Quaternion defaultQ;
			[SerializeField] Quaternion lookUpQ;
			[SerializeField] Quaternion lookDownQ;

			public float maxUpAngle;
			public float maxDownAngle;

			[SerializeField] bool isLookUpSet;
			[SerializeField] bool isLookDownSet;

		#endregion


		public bool CanImport(EyeRotationLimiterForExport import, Transform startXform)
		{
			return Utils.CanGetTransformFromPath(startXform, import.transformPath);
		}

		public float ClampAngle( float angle )
		{
			return Mathf.Clamp( Utils.NormalizedDegAngle(angle), -maxUpAngle, maxDownAngle );
		}



		public EyeRotationLimiterForExport GetExport(Transform startXform)
		{
			EyeRotationLimiterForExport export = new EyeRotationLimiterForExport
			{
				transformPath = Utils.GetPathForTransform(startXform, transform),
				defaultQ = defaultQ,
				lookUpQ = lookUpQ,
				lookDownQ = lookDownQ,
				isLookUpSet = isLookUpSet,
				isLookDownSet = isLookDownSet
			};

			return export;
		}


		public float GetEyeUp01( float angle )
		{
			return angle >= 0 ? 0 : Mathf.InverseLerp(0, maxUpAngle, -angle);
		}


		public float GetEyeDown01( float angle )
		{
			return angle <= 0 ? 0 : Mathf.InverseLerp(0, maxDownAngle, angle);
		}


		public void Import( EyeRotationLimiterForExport import, Transform startXform )
		{
			transform = Utils.GetTransformFromPath(startXform, import.transformPath);
			defaultQ = import.defaultQ;
			lookUpQ = import.lookUpQ;
			lookDownQ = import.lookDownQ;
			isLookUpSet = import.isLookUpSet;
			isLookDownSet = import.isLookDownSet;

			UpdateMaxAngles();
		}



		public void RestoreDefault()
		{
			transform.localRotation = defaultQ;
		}


		public void RestoreLookDown()
		{
			transform.localRotation = lookDownQ;
		}


		public void RestoreLookUp()
		{
			transform.localRotation = lookUpQ;
		}


		public void SaveDefault( Transform t )
		{
			transform = t;
			defaultQ = t.localRotation;
			if (false == isLookDownSet)
				lookDownQ = defaultQ * Quaternion.Euler(20, 0, 0);
			if (false == isLookUpSet)
				lookUpQ = defaultQ * Quaternion.Euler(-8, 0, 0);
			UpdateMaxAngles();
		}


		public void SaveLookDown()
		{
			lookDownQ = transform.localRotation;
			UpdateMaxAngles();
			isLookDownSet = true;
		}

		
		public void SaveLookUp()
		{
			lookUpQ = transform.localRotation;
			UpdateMaxAngles();
			isLookUpSet = true;
		}
		

		void UpdateMaxAngles()
		{
			Vector3 lookUpEulerInDefaultSpace = (Quaternion.Inverse( defaultQ ) * lookUpQ).eulerAngles;
			maxUpAngle = Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookUpEulerInDefaultSpace.x)),
													Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookUpEulerInDefaultSpace.y)),
																		Mathf.Abs(Utils.NormalizedDegAngle(lookUpEulerInDefaultSpace.z))));

			Vector3 lookDownEulerInDefaultSpace = (Quaternion.Inverse( defaultQ ) * lookDownQ).eulerAngles;
			maxDownAngle = Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookDownEulerInDefaultSpace.x)),
													Mathf.Max(	Mathf.Abs(Utils.NormalizedDegAngle(lookDownEulerInDefaultSpace.y)),
																		Mathf.Abs(Utils.NormalizedDegAngle(lookDownEulerInDefaultSpace.z))));
		}

	}



	[System.Serializable]
	public class EyelidRotationLimiter
	{
		[System.Serializable]
		public class EyelidRotationLimiterForExport
		{
			public  string transformPath;
			public  SerializableQuaternion defaultQ;
			public  SerializableQuaternion closedQ;
			public  SerializableQuaternion lookUpQ;
			public  SerializableQuaternion lookDownQ;
			public  float eyeMaxDownAngle;
			public  float eyeMaxUpAngle;

			public  SerializableVector3 defaultPos;
			public  SerializableVector3 closedPos;
			public  SerializableVector3 lookUpPos;
			public  SerializableVector3 lookDownPos;

			public  bool isLookUpSet;
			public  bool isLookDownSet;
			public  bool isDefaultPosSet;
			public  bool isClosedPosSet;
			public  bool isLookUpPosSet;
			public  bool isLookDownPosSet;
		}



		#region fields

			[SerializeField] Transform transform;
			[SerializeField] Quaternion defaultQ;
			[SerializeField] Quaternion closedQ;
			[SerializeField] Quaternion lookUpQ;
			[SerializeField] Quaternion lookDownQ;
			[SerializeField] float eyeMaxDownAngle;
			[SerializeField] float eyeMaxUpAngle;

			[SerializeField] Vector3 defaultPos;
			[SerializeField] Vector3 closedPos;
			[SerializeField] Vector3 lookUpPos;
			[SerializeField] Vector3 lookDownPos;

			[SerializeField] bool isLookUpSet;
			[SerializeField] bool isLookDownSet;
			[SerializeField] bool isDefaultPosSet;
			[SerializeField] bool isClosedPosSet;
			[SerializeField] bool isLookUpPosSet;
			[SerializeField] bool isLookDownPosSet;

		#endregion


		public bool CanImport(EyelidRotationLimiterForExport import, Transform startXform)
		{
			return Utils.CanGetTransformFromPath(startXform, import.transformPath);
		}


		public EyelidRotationLimiterForExport GetExport(Transform startXform)
		{
			EyelidRotationLimiterForExport export = new EyelidRotationLimiterForExport()
			{
					transformPath = Utils.GetPathForTransform(startXform, transform),
					defaultQ = defaultQ,
					closedQ = closedQ,
					lookUpQ = lookUpQ,
					lookDownQ = lookDownQ,
					eyeMaxDownAngle = eyeMaxDownAngle,
					eyeMaxUpAngle = eyeMaxUpAngle,
					defaultPos = defaultPos,
					closedPos = closedPos,
					lookUpPos = lookUpPos,
					lookDownPos = lookDownPos,
					isLookUpSet = isLookUpSet,
					isLookDownSet = isLookDownSet,
					isDefaultPosSet = isDefaultPosSet,
					isClosedPosSet = isClosedPosSet,
					isLookUpPosSet = isLookUpPosSet,
					isLookDownPosSet = isLookDownPosSet
			};

			return export;
		}



		public void GetRotationAndPosition( float eyeAngle, float blink01, float eyeWidenOrSquint, bool isUpper, out Quaternion rotation, ref Vector3 position, ControlData.EyelidBoneMode eyelidBoneMode )
		{
			bool isLookingDown = eyeAngle > 0;
			float angle01 = Mathf.InverseLerp(0, isLookingDown	? eyeMaxDownAngle : -eyeMaxUpAngle, eyeAngle);
					
			if ( eyeWidenOrSquint < 0 )
				blink01 = Mathf.Lerp(blink01, 1, -eyeWidenOrSquint);

			//*** Rotation
			{
				if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Rotation )
				{
					rotation = Quaternion.Slerp(defaultQ, isLookingDown ? lookDownQ : lookUpQ, angle01);
					rotation = Quaternion.Slerp(rotation, closedQ, blink01);
					if ( eyeWidenOrSquint > 0 )
						rotation = Quaternion.Slerp(rotation, isUpper ? lookUpQ : lookDownQ, eyeWidenOrSquint);
				}
				else
					rotation = Quaternion.identity;
			}

			//*** Position
			{
				if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Position )
				{
					if ( isLookingDown )
					{
						if ( isDefaultPosSet && isLookDownPosSet )
							position = Vector3.Lerp(defaultPos, lookDownPos, angle01);
					}
					else
					{
						if ( isDefaultPosSet && isLookUpPosSet )
							position = Vector3.Lerp(defaultPos, lookUpPos, angle01);
					}

					if ( isDefaultPosSet && isClosedPosSet )
						position = Vector3.Lerp(position, closedPos, blink01);
					if ( eyeWidenOrSquint > 0 )
						position = Vector3.Lerp(position, isUpper ? lookUpPos : lookDownPos, eyeWidenOrSquint);
				}
			}			
		}



		public void Import(EyelidRotationLimiterForExport import, Transform startXform)
		{
			transform = Utils.GetTransformFromPath(startXform, import.transformPath);
			defaultQ = import.defaultQ;
			closedQ = import.closedQ;
			lookUpQ = import.lookUpQ;
			lookDownQ = import.lookDownQ;
			eyeMaxDownAngle = import.eyeMaxDownAngle;
			eyeMaxUpAngle = import.eyeMaxUpAngle;
			defaultPos = import.defaultPos;
			closedPos = import.closedPos;
			lookUpPos = import.lookUpPos;
			lookDownPos = import.lookDownPos;
			isLookUpSet = import.isLookUpSet;
			isLookDownPosSet = import.isLookDownPosSet;
			isDefaultPosSet = import.isDefaultPosSet;
			isClosedPosSet = import.isClosedPosSet;
			isLookUpPosSet = import.isLookUpPosSet;
			isLookDownPosSet = import.isLookDownPosSet;
		}



		public void RestoreClosed(ControlData.EyelidBoneMode eyelidBoneMode)
		{
			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Rotation )
				transform.localRotation = closedQ;

			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Position )
				if ( isClosedPosSet )
					transform.localPosition = closedPos;
		}


		public void RestoreDefault(ControlData.EyelidBoneMode eyelidBoneMode)
		{
			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Rotation )
				transform.localRotation = defaultQ;

			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Position )
				if ( isDefaultPosSet )
					transform.localPosition = defaultPos;
		}


		public void RestoreLookDown(ControlData.EyelidBoneMode eyelidBoneMode)
		{
			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Rotation )
				transform.localRotation = lookDownQ;

			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Position )
				if ( isLookDownPosSet )
					transform.localPosition = lookDownPos;
		}


		public void RestoreLookUp(ControlData.EyelidBoneMode eyelidBoneMode)
		{
			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Rotation )
				transform.localRotation = lookUpQ;

			if ( eyelidBoneMode == ControlData.EyelidBoneMode.RotationAndPosition || eyelidBoneMode == ControlData.EyelidBoneMode.Position )
				if ( isLookUpPosSet)
					transform.localPosition = lookUpPos;
		}


		public void SaveClosed()
		{
			closedQ = transform.localRotation;
			closedPos = transform.localPosition;
			isClosedPosSet = true;
		}


		public void SaveDefault( Transform t )
		{
			transform = t;

			defaultQ = t.localRotation;
					if (false == isLookDownSet)
						lookDownQ = defaultQ * Quaternion.Euler(20, 0, 0);
					if (false == isLookUpSet)
						lookUpQ = defaultQ * Quaternion.Euler(-8, 0, 0);

			defaultPos = t.localPosition;
					isDefaultPosSet = true;
					if ( false == isLookUpPosSet )
						lookUpPos = defaultPos;
					if ( false == isLookDownPosSet )
						lookDownPos = defaultPos;
					if ( false == isClosedPosSet )
						closedPos = defaultPos;
		}


		public void SaveLookDown(float eyeMaxDownAngle)
		{
			this.eyeMaxDownAngle = eyeMaxDownAngle;
			lookDownQ = transform.localRotation;
			lookDownPos = transform.localPosition;
			isLookDownSet = true;
			isLookDownPosSet = true;
		}

		
		public void SaveLookUp(float eyeMaxUpAngle)
		{
			this.eyeMaxUpAngle = eyeMaxUpAngle;
			lookUpQ = transform.localRotation;
			lookUpPos = transform.localPosition;
			isLookUpSet = true;
			isLookUpPosSet = true;
		}


	}


	[System.Serializable]
	public class ControlData
	{	

		[System.Serializable]
		public class ControlDataForExport
		{
			public EyeControl eyeControl;
			public EyelidBoneMode eyelidBoneMode;

			public string leftEyePath;
			public string rightEyePath;

			public float maxEyeUpBoneAngle;
			public float maxEyeDownBoneAngle;

			public float maxEyeUpEyeballAngle;
			public float maxEyeDownEyeballAngle;

			public bool isEyeBallDefaultSet;
			public bool isEyeBoneDefaultSet;
			public bool isEyeBallLookUpSet;
			public bool isEyeBoneLookUpSet;
			public bool isEyeBallLookDownSet;
			public bool isEyeBoneLookDownSet;

			public EyeRotationLimiter.EyeRotationLimiterForExport leftBoneEyeRotationLimiter;
			public EyeRotationLimiter.EyeRotationLimiterForExport rightBoneEyeRotationLimiter;
			public EyeRotationLimiter.EyeRotationLimiterForExport leftEyeballEyeRotationLimiter;
			public EyeRotationLimiter.EyeRotationLimiterForExport rightEyeballEyeRotationLimiter;

			public EyelidControl eyelidControl;
			public bool eyelidsFollowEyesVertically;

			public string upperEyeLidLeftPath;
			public string upperEyeLidRightPath;
			public string lowerEyeLidLeftPath;
			public string lowerEyeLidRightPath;

			public bool isEyelidBonesDefaultSet;
			public bool isEyelidBonesClosedSet;
			public bool isEyelidBonesLookUpSet;
			public bool isEyelidBonesLookDownSet;

			public EyelidRotationLimiter.EyelidRotationLimiterForExport upperLeftLimiter;
			public EyelidRotationLimiter.EyelidRotationLimiterForExport upperRightLimiter;
			public EyelidRotationLimiter.EyelidRotationLimiterForExport lowerLeftLimiter;
			public EyelidRotationLimiter.EyelidRotationLimiterForExport lowerRightLimiter;

			public float eyeWidenOrSquint;

			public EyelidPositionBlendshapeForExport[] blendshapesForBlinking;
			public EyelidPositionBlendshapeForExport[] blendshapesForLookingUp;
			public EyelidPositionBlendshapeForExport[] blendshapesForLookingDown;
			public BlendshapesConfigForExport[] blendshapesConfigs;

			public bool isEyelidBlendshapeDefaultSet;
			public bool isEyelidBlendshapeClosedSet;
			public bool isEyelidBlendshapeLookUpSet;
			public bool isEyelidBlendshapeLookDownSet;
	}



		#region fields

			#region Eyes

				public enum EyeControl
				{
					None,
					MecanimEyeBones,
					SelectedObjects
				}
				public EyeControl eyeControl = EyeControl.None;

				public Transform leftEye;
				public Transform rightEye;

				public float maxEyeUpBoneAngle = 20;
				public float maxEyeDownBoneAngle = 20;

				public float maxEyeUpEyeballAngle = 20;
				public float maxEyeDownEyeballAngle = 20;

				public bool isEyeBallDefaultSet;
				public bool isEyeBoneDefaultSet;
				public bool isEyeBallLookUpSet;
				public bool isEyeBoneLookUpSet;
				public bool isEyeBallLookDownSet;
				public bool isEyeBoneLookDownSet;

				[SerializeField] EyeRotationLimiter leftBoneEyeRotationLimiter = new EyeRotationLimiter();
				[SerializeField] EyeRotationLimiter rightBoneEyeRotationLimiter = new EyeRotationLimiter();
				[SerializeField] EyeRotationLimiter leftEyeballEyeRotationLimiter = new EyeRotationLimiter();
				[SerializeField] EyeRotationLimiter rightEyeballEyeRotationLimiter = new EyeRotationLimiter();

			#endregion


			#region Eyelids

				public enum EyelidControl
				{
					None,
					Bones,
					Blendshapes
				}
				public EyelidControl eyelidControl = EyelidControl.None;

				public enum EyelidBoneMode
				{
					RotationAndPosition,
					Rotation,
					Position
				}
				public EyelidBoneMode eyelidBoneMode = EyelidBoneMode.RotationAndPosition;

				public bool eyelidsFollowEyesVertically;
				#region Eyelid Bones

					public Transform upperEyeLidLeft;
					public Transform upperEyeLidRight;
					public Transform lowerEyeLidLeft;
					public Transform lowerEyeLidRight;

					public bool isEyelidBonesDefaultSet;
					public bool isEyelidBonesClosedSet;
					public bool isEyelidBonesLookUpSet;
					public bool isEyelidBonesLookDownSet;

					[SerializeField] EyelidRotationLimiter upperLeftLimiter = new EyelidRotationLimiter();
					[SerializeField] EyelidRotationLimiter upperRightLimiter = new EyelidRotationLimiter();
					[SerializeField] EyelidRotationLimiter lowerLeftLimiter = new EyelidRotationLimiter();
					[SerializeField] EyelidRotationLimiter lowerRightLimiter = new EyelidRotationLimiter();

					[Tooltip("0: normal. 1: max widened, -1: max squint")]
					[Range(-1, 1)]
					public float eyeWidenOrSquint;


				#endregion


				#region Eyelid Blendshapes

					[System.Serializable]
					public class EyelidPositionBlendshapeForExport
					{
						public string skinnedMeshRendererPath;
						public float defaultWeight;
						public float positionWeight;
						public int index;
						public string name;
						public bool isUsedInEalierConfig;
					}


					[System.Serializable]
					public class EyelidPositionBlendshape
					{
						public SkinnedMeshRenderer skinnedMeshRenderer;
						public float defaultWeight;
						public float positionWeight;
						public int index;
						public string name;
						public bool isUsedInEalierConfig;

						public static bool CanImport(EyelidPositionBlendshapeForExport import, Transform startXform)
						{
							Transform t = Utils.GetTransformFromPath(startXform, import.skinnedMeshRendererPath);

							if ( t == null )
								return false;

							SkinnedMeshRenderer meshRenderer = t.GetComponent<SkinnedMeshRenderer>();

							if ( meshRenderer == null )
								return false;

							if ( import.index >= meshRenderer.sharedMesh.blendShapeCount )
								return false;

							if ( false == string.IsNullOrEmpty(import.name) )
							{
								bool containsName = false;
								for ( int i=0;  i<meshRenderer.sharedMesh.blendShapeCount;  i++ )
									if ( meshRenderer.sharedMesh.GetBlendShapeName(i).Equals( import.name ) )
									{
										containsName = true;
										break;
									}

								if ( false == containsName )
									return false;
							}									

							return true;
						}



						public EyelidPositionBlendshapeForExport GetExport(Transform startXform)
						{
							EyelidPositionBlendshapeForExport export = new EyelidPositionBlendshapeForExport
							{
								skinnedMeshRendererPath = Utils.GetPathForTransform(startXform, skinnedMeshRenderer.transform),
								defaultWeight = defaultWeight,
								positionWeight = positionWeight,
								index = index,
								name = name,
								isUsedInEalierConfig = isUsedInEalierConfig
							};

							return export;
						}



						public void Import(EyelidPositionBlendshapeForExport export, Transform startXform)
						{
							skinnedMeshRenderer = Utils.GetTransformFromPath(startXform, export.skinnedMeshRendererPath).GetComponent<SkinnedMeshRenderer>();
							defaultWeight = export.defaultWeight;
							positionWeight = export.positionWeight;
							index = export.index;
							name = export.name;
							isUsedInEalierConfig = export.isUsedInEalierConfig;

							//*** If we imported a name for the blendshape, find the correct index, because during runtime we use the index to manipulate blendshapes
							{
								if ( false == string.IsNullOrEmpty(name) )
									for ( int i=0;  i<skinnedMeshRenderer.sharedMesh.blendShapeCount;  i++ )
										if ( skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i).Equals( name ) )
										{
											index = i;
											break;
										}
							}
						}

					}

					[SerializeField] EyelidPositionBlendshape[] blendshapesForBlinking;
					[SerializeField] EyelidPositionBlendshape[] blendshapesForLookingUp;
					[SerializeField] EyelidPositionBlendshape[] blendshapesForLookingDown;


					[System.Serializable]
					public class BlendshapesConfigForExport
					{
						public string skinnedMeshRendererPath;
						public int blendShapeCount;
						public string[] blendshapeNames;
						public float[] blendshapeWeights;
					}

					[System.Serializable]
					public class BlendshapesConfig
					{
						public SkinnedMeshRenderer skinnedMeshRenderer;
						public int blendShapeCount;
						public string[] blendshapeNames;
						public float[] blendshapeWeights;

						public static bool CanImport(BlendshapesConfigForExport import, Transform startXform)
						{
							Transform t = Utils.GetTransformFromPath(startXform, import.skinnedMeshRendererPath);
							if ( t == null )
								return false;

							SkinnedMeshRenderer skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
							if ( skinnedMeshRenderer == null )
								return false;

							if ( import.blendShapeCount != skinnedMeshRenderer.sharedMesh.blendShapeCount )
								return false;

							if ( import.blendshapeNames != null && import.blendshapeNames.Length > 0 )
								for ( int i=0;  i<import.blendShapeCount;  i++ )
									if ( false == import.blendshapeNames[i].Equals(skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i)) )
										return false;

							return true;
						}

						public BlendshapesConfigForExport GetExport(Transform startXform)
						{
							BlendshapesConfigForExport export = new BlendshapesConfigForExport
							{
								skinnedMeshRendererPath = Utils.GetPathForTransform(startXform, skinnedMeshRenderer.transform),
								blendShapeCount = blendShapeCount,
								blendshapeNames = blendshapeNames,
								blendshapeWeights = blendshapeWeights
							};

							return export;
						}

						public void Import(Transform startXform, BlendshapesConfigForExport import)
						{
							skinnedMeshRenderer = Utils.GetTransformFromPath(startXform, import.skinnedMeshRendererPath).GetComponent<SkinnedMeshRenderer>();
							blendShapeCount = import.blendShapeCount;
							blendshapeNames = import.blendshapeNames;
							blendshapeWeights = import.blendshapeWeights;
						}
					}

					[SerializeField] BlendshapesConfig[] blendshapesConfigs;

					public bool isEyelidBlendshapeDefaultSet;
					public bool isEyelidBlendshapeClosedSet;
					public bool isEyelidBlendshapeLookUpSet;
					public bool isEyelidBlendshapeLookDownSet;

				#endregion
			#endregion
		#endregion




		public bool CanImport(ControlDataForExport import, Transform startXform)
		{
			bool canImport =
						Utils.CanGetTransformFromPath(startXform, import.leftEyePath) &&
						Utils.CanGetTransformFromPath(startXform, import.rightEyePath) &&
						Utils.CanGetTransformFromPath(startXform, import.upperEyeLidLeftPath) &&
						Utils.CanGetTransformFromPath(startXform, import.upperEyeLidRightPath) &&
						Utils.CanGetTransformFromPath(startXform, import.lowerEyeLidLeftPath) &&
						Utils.CanGetTransformFromPath(startXform, import.lowerEyeLidRightPath) &&

						leftBoneEyeRotationLimiter.CanImport(import.leftBoneEyeRotationLimiter, startXform) &&
						rightBoneEyeRotationLimiter.CanImport(import.rightBoneEyeRotationLimiter, startXform) &&
						leftEyeballEyeRotationLimiter.CanImport(import.leftEyeballEyeRotationLimiter, startXform) &&
						rightEyeballEyeRotationLimiter.CanImport(import.rightEyeballEyeRotationLimiter, startXform) &&

						upperLeftLimiter.CanImport(import.upperLeftLimiter, startXform) &&
						upperRightLimiter.CanImport(import.upperRightLimiter, startXform) &&
						lowerLeftLimiter.CanImport(import.lowerLeftLimiter, startXform) &&
						lowerRightLimiter.CanImport(import.lowerRightLimiter, startXform);

			if ( false == canImport )
				return false;
			
			if ( import.blendshapesForBlinking != null )
				foreach ( EyelidPositionBlendshapeForExport blendShapeImport in import.blendshapesForBlinking )
					if ( false == EyelidPositionBlendshape.CanImport(blendShapeImport, startXform) )
						return false;

			if ( import.blendshapesForLookingUp != null )
				foreach ( EyelidPositionBlendshapeForExport blendShapeImport in import.blendshapesForLookingUp)
					if ( false == EyelidPositionBlendshape.CanImport(blendShapeImport, startXform) )
						return false;

			if ( import.blendshapesForLookingDown != null )
				foreach ( EyelidPositionBlendshapeForExport blendShapeImport in import.blendshapesForLookingDown)
					if ( false == EyelidPositionBlendshape.CanImport(blendShapeImport, startXform) )
						return false;

			//if ( import.blendshapesConfigs != null )
			//	foreach ( BlendshapesConfigForExport blendshapeConfigImport in import.blendshapesConfigs)
			//		if ( false == BlendshapesConfig.CanImport(blendshapeConfigImport, startXform) )
			//			return false;

			return true;
		}




		public void CheckConsistency( Animator animator, EyeAndHeadAnimator eyeAndHeadAnimator )
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
			{
				if ( null == animator )
					throw new System.Exception("No Animator found.");
				if ( null == animator.GetBoneTransform(HumanBodyBones.LeftEye) || null == animator.GetBoneTransform(HumanBodyBones.LeftEye) )
					throw new System.Exception("Mecanim humanoid eye bones not found.");

				if ( false == isEyeBoneDefaultSet )
					SaveDefault( eyeAndHeadAnimator );
			}
			else if ( eyeControl == EyeControl.SelectedObjects )
			{
				if ( null == leftEye )
					throw new System.Exception("The left eye object hasn't been assigned.");
				if ( null == rightEye )
					throw new System.Exception("The right eye object hasn't been assigned.");

				if ( false == isEyeBallDefaultSet )
					SaveDefault( eyeAndHeadAnimator );
			}

			if ( eyelidControl == EyelidControl.Bones )
			{
				if ( upperEyeLidLeft == null || upperEyeLidRight == null )
					throw new System.Exception("The upper eyelid bones haven't been assigned.");
				if ( false == isEyelidBonesDefaultSet )
					throw new System.Exception("The default eyelid position hasn't been saved.");
				if ( false == isEyelidBonesClosedSet )
					throw new System.Exception("The eyes closed eyelid position hasn't been saved.");
				if ( false == isEyelidBonesLookUpSet )
					throw new System.Exception("The eyes look up eyelid position hasn't been saved.");
				if ( false == isEyelidBonesLookDownSet )
					throw new System.Exception("The eyes look down eyelid position hasn't been saved.");
			 }
			 else if ( eyelidControl == EyelidControl.Blendshapes )
			 {
				if ( false == isEyelidBlendshapeDefaultSet )
					throw new System.Exception("The default eyelid position hasn't been saved.");
				if ( false == isEyelidBlendshapeClosedSet )
					throw new System.Exception("The eyes closed eyelid position hasn't been saved.");
				if ( false == isEyelidBlendshapeLookUpSet )
					throw new System.Exception("The eyes look up eyelid position hasn't been saved.");
				if ( false == isEyelidBlendshapeLookDownSet )
					throw new System.Exception("The eyes look down eyelid position hasn't been saved.");
			}

		}



		public float ClampLeftVertEyeAngle( float angle )
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
				return leftBoneEyeRotationLimiter.ClampAngle( angle );
			
			if ( eyeControl == EyeControl.SelectedObjects )
				return leftEyeballEyeRotationLimiter.ClampAngle( angle );
			
			return angle;
		}


				 
		public float ClampRightVertEyeAngle( float angle )
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
				return rightBoneEyeRotationLimiter.ClampAngle( angle );
			
			if ( eyeControl == EyeControl.SelectedObjects )
				return rightEyeballEyeRotationLimiter.ClampAngle( angle );

			return angle;
		}



		public ControlDataForExport GetExport(Transform startXform)
		{
			ControlDataForExport export = new ControlDataForExport
			{
				eyeControl = eyeControl,
				eyelidBoneMode =  eyelidBoneMode,
				leftEyePath = Utils.GetPathForTransform(startXform, leftEye),
				rightEyePath = Utils.GetPathForTransform(startXform, rightEye),
				maxEyeUpBoneAngle = maxEyeUpBoneAngle,
				maxEyeDownBoneAngle = maxEyeDownBoneAngle,
				maxEyeUpEyeballAngle = maxEyeUpEyeballAngle,
				maxEyeDownEyeballAngle = maxEyeDownEyeballAngle,
				isEyeBallDefaultSet = isEyeBallDefaultSet,
				isEyeBoneDefaultSet = isEyeBoneDefaultSet,
				isEyeBallLookUpSet = isEyeBallLookUpSet,
				isEyeBoneLookUpSet = isEyeBoneLookUpSet,
				isEyeBallLookDownSet = isEyeBallLookDownSet,
				isEyeBoneLookDownSet = isEyeBoneLookDownSet,
				leftBoneEyeRotationLimiter = leftBoneEyeRotationLimiter.GetExport(startXform),
				rightBoneEyeRotationLimiter = rightBoneEyeRotationLimiter.GetExport(startXform),
				leftEyeballEyeRotationLimiter = leftEyeballEyeRotationLimiter.GetExport(startXform),
				rightEyeballEyeRotationLimiter = rightEyeballEyeRotationLimiter.GetExport(startXform),
				eyelidControl = eyelidControl,
				eyelidsFollowEyesVertically = eyelidsFollowEyesVertically,
				upperEyeLidLeftPath = Utils.GetPathForTransform(startXform, upperEyeLidLeft),
				upperEyeLidRightPath = Utils.GetPathForTransform(startXform, upperEyeLidRight),
				lowerEyeLidLeftPath = Utils.GetPathForTransform(startXform, lowerEyeLidLeft),
				lowerEyeLidRightPath = Utils.GetPathForTransform(startXform, lowerEyeLidRight),
				isEyelidBonesDefaultSet = isEyelidBonesDefaultSet,
				isEyelidBonesClosedSet = isEyelidBonesClosedSet,
				isEyelidBonesLookUpSet = isEyelidBonesLookUpSet,
				isEyelidBonesLookDownSet = isEyelidBonesLookDownSet,
				upperLeftLimiter = upperLeftLimiter.GetExport(startXform),
				upperRightLimiter = upperRightLimiter.GetExport(startXform),
				lowerLeftLimiter = lowerLeftLimiter.GetExport(startXform),
				lowerRightLimiter = lowerRightLimiter.GetExport(startXform),
				eyeWidenOrSquint = eyeWidenOrSquint,
				isEyelidBlendshapeDefaultSet = isEyelidBlendshapeDefaultSet,
				isEyelidBlendshapeClosedSet = isEyelidBlendshapeClosedSet,
				isEyelidBlendshapeLookUpSet = isEyelidBlendshapeLookUpSet,
				isEyelidBlendshapeLookDownSet = isEyelidBlendshapeLookDownSet
			};

			export.blendshapesForBlinking = new EyelidPositionBlendshapeForExport[blendshapesForBlinking.Length];
			for ( int i=0;  i<blendshapesForBlinking.Length;  i++ )
				export.blendshapesForBlinking[i] = blendshapesForBlinking[i].GetExport(startXform);

			export.blendshapesForLookingUp = new EyelidPositionBlendshapeForExport[blendshapesForLookingUp.Length];
			for ( int i=0;  i<blendshapesForLookingUp.Length;  i++ )
				export.blendshapesForLookingUp[i] = blendshapesForLookingUp[i].GetExport(startXform);

			export.blendshapesForLookingDown = new EyelidPositionBlendshapeForExport[blendshapesForLookingDown.Length];
			for ( int i=0;  i<blendshapesForLookingDown.Length;  i++ )
				export.blendshapesForLookingDown[i] = blendshapesForLookingDown[i].GetExport(startXform);

			export.blendshapesConfigs = new BlendshapesConfigForExport[blendshapesConfigs.Length];
			for ( int i=0;  i<blendshapesConfigs.Length;  i++ )
				export.blendshapesConfigs[i] = blendshapesConfigs[i].GetExport(startXform);

			return export;
		}



		public void Import(ControlDataForExport import, Transform startXform)
		{
			eyeControl = import.eyeControl;
			eyelidBoneMode = import.eyelidBoneMode;
			leftEye = Utils.GetTransformFromPath(startXform, import.leftEyePath);
			rightEye = Utils.GetTransformFromPath(startXform, import.rightEyePath);
			maxEyeUpBoneAngle = import.maxEyeUpBoneAngle;
			maxEyeDownBoneAngle = import.maxEyeDownBoneAngle;
			maxEyeUpEyeballAngle = import.maxEyeUpEyeballAngle;
			maxEyeDownEyeballAngle = import.maxEyeDownEyeballAngle;
			isEyeBallDefaultSet = import.isEyeBallDefaultSet;
			isEyeBoneDefaultSet = import.isEyeBoneDefaultSet;
			isEyeBallLookUpSet = import.isEyeBallLookUpSet;
			isEyeBoneLookUpSet = import.isEyeBoneLookUpSet;
			isEyeBallLookDownSet = import.isEyeBallLookDownSet;
			isEyeBoneLookDownSet = import.isEyeBoneLookDownSet;
			eyelidControl = import.eyelidControl;
			eyelidsFollowEyesVertically = import.eyelidsFollowEyesVertically;
			upperEyeLidLeft = Utils.GetTransformFromPath(startXform, import.upperEyeLidLeftPath);
			upperEyeLidRight = Utils.GetTransformFromPath(startXform, import.upperEyeLidRightPath);
			lowerEyeLidLeft = Utils.GetTransformFromPath(startXform, import.lowerEyeLidLeftPath);
			lowerEyeLidRight = Utils.GetTransformFromPath(startXform, import.lowerEyeLidRightPath);
			isEyelidBonesDefaultSet = import.isEyelidBonesDefaultSet;
			isEyelidBonesClosedSet = import.isEyelidBonesClosedSet;
			isEyelidBonesLookUpSet = import.isEyelidBonesLookUpSet;
			isEyelidBonesLookDownSet = import.isEyelidBonesLookDownSet;
			eyeWidenOrSquint = import.eyeWidenOrSquint;

			isEyelidBlendshapeDefaultSet = import.isEyelidBlendshapeDefaultSet;
			isEyelidBlendshapeClosedSet = import.isEyelidBlendshapeClosedSet;
			isEyelidBlendshapeLookUpSet = import.isEyelidBlendshapeLookUpSet;
			isEyelidBlendshapeLookDownSet = import.isEyelidBlendshapeLookDownSet;

			leftBoneEyeRotationLimiter.Import(import.leftBoneEyeRotationLimiter, startXform);
			rightBoneEyeRotationLimiter.Import(import.rightBoneEyeRotationLimiter, startXform);
			leftEyeballEyeRotationLimiter.Import(import.leftEyeballEyeRotationLimiter, startXform);
			rightEyeballEyeRotationLimiter.Import(import.rightEyeballEyeRotationLimiter, startXform);

			upperLeftLimiter.Import(import.upperLeftLimiter, startXform);
			upperRightLimiter.Import(import.upperRightLimiter, startXform);
			lowerLeftLimiter.Import(import.lowerLeftLimiter, startXform);
			lowerRightLimiter.Import(import.lowerRightLimiter, startXform);

			if ( import.blendshapesForBlinking != null )
			{
				blendshapesForBlinking = new EyelidPositionBlendshape[import.blendshapesForBlinking.Length];
				for ( int i=0;  i<import.blendshapesForBlinking.Length;  i++ )
				{
					EyelidPositionBlendshape eyelidPositionBlendshape = new EyelidPositionBlendshape();
					eyelidPositionBlendshape.Import(import.blendshapesForBlinking[i], startXform);
					blendshapesForBlinking[i] = eyelidPositionBlendshape;
				}
			}

			if ( import.blendshapesForLookingUp != null )
			{
				blendshapesForLookingUp = new EyelidPositionBlendshape[import.blendshapesForLookingUp.Length];
				for ( int i=0;  i<import.blendshapesForLookingUp.Length;  i++ )
				{
					EyelidPositionBlendshape eyelidPositionBlendshape = new EyelidPositionBlendshape();
					eyelidPositionBlendshape.Import(import.blendshapesForLookingUp[i], startXform);
					blendshapesForLookingUp[i] = eyelidPositionBlendshape;
				}
			}

			if ( import.blendshapesForLookingDown != null )
			{
				blendshapesForLookingDown = new EyelidPositionBlendshape[import.blendshapesForLookingDown.Length];
				for ( int i=0;  i<import.blendshapesForLookingDown.Length;  i++ )
				{
					EyelidPositionBlendshape eyelidPositionBlendshape = new EyelidPositionBlendshape();
					eyelidPositionBlendshape.Import(import.blendshapesForLookingDown[i], startXform);
					blendshapesForLookingDown[i] = eyelidPositionBlendshape;
				}
			}


			bool canImportBlendshapeConfigs = false;
			SkinnedMeshRenderer[] skinnedMeshRenderers = startXform.GetComponentsInChildren<SkinnedMeshRenderer>();
			if (import.blendshapesConfigs != null)
				if ( skinnedMeshRenderers.Length == import.blendshapesConfigs.Length )
				{
					canImportBlendshapeConfigs = true;
					foreach (BlendshapesConfigForExport blendshapeConfigImport in import.blendshapesConfigs)
						if (false == BlendshapesConfig.CanImport(blendshapeConfigImport, startXform))
						{
							canImportBlendshapeConfigs = false;
							break;
						}
				}

			if ( canImportBlendshapeConfigs )
			{
				blendshapesConfigs = new BlendshapesConfig[import.blendshapesConfigs.Length];
				for ( int i=0;  i<import.blendshapesConfigs.Length;  i++ )
				{	
					BlendshapesConfig blendshapesConfig = new BlendshapesConfig();
					blendshapesConfig.Import(startXform, import.blendshapesConfigs[i]);
					blendshapesConfigs[i] = blendshapesConfig;
				}
			}
			else
				blendshapesConfigs = null;
		}



		public void Initialize( )
		{
			if ( eyelidControl == EyelidControl.Blendshapes )
			{
				//*** For each blinking blendshape save in "isUsedInEalierConfig" whether it is used in lookingUp or lookingDown
				{
					foreach ( EyelidPositionBlendshape blendShapeForBlinking in blendshapesForBlinking )
					{
						blendShapeForBlinking.isUsedInEalierConfig = false;

						foreach ( EyelidPositionBlendshape blendShapeForLookingUp in blendshapesForLookingUp )
						{
							if ( blendShapeForBlinking.skinnedMeshRenderer == blendShapeForLookingUp.skinnedMeshRenderer &&
								blendShapeForBlinking.index == blendShapeForLookingUp.index )
							{
								blendShapeForBlinking.isUsedInEalierConfig = true;
								break;
							}
						}

						if ( false == blendShapeForBlinking.isUsedInEalierConfig )
							foreach ( EyelidPositionBlendshape blendShapeForLookingDown in blendshapesForLookingDown )
							{
								if ( blendShapeForBlinking.skinnedMeshRenderer == blendShapeForLookingDown.skinnedMeshRenderer &&
									blendShapeForBlinking.index == blendShapeForLookingDown.index )
								{
									blendShapeForBlinking.isUsedInEalierConfig = true;
									break;
								}
							}
					}
				}
			}
		}



		void LerpBlendshapeConfig( EyelidPositionBlendshape[] blendshapes, float lerpValue, bool relativeToCurrentValueIfUsedInOtherConfig=false )
		{
			foreach ( EyelidPositionBlendshape blendShape in blendshapes )
				blendShape.skinnedMeshRenderer
					.SetBlendShapeWeight( blendShape.index,
									Mathf.Lerp( (blendShape.isUsedInEalierConfig && relativeToCurrentValueIfUsedInOtherConfig) ? blendShape.skinnedMeshRenderer.GetBlendShapeWeight(blendShape.index)
																																											: blendShape.defaultWeight,
													blendShape.positionWeight, lerpValue) );
		}



		public bool NeedsSaveDefaultBlendshapeConfig()
		{
			return blendshapesConfigs == null || blendshapesConfigs.Length == 0;
		}



		void ResetBlendshapeConfig( EyelidPositionBlendshape[] blendshapes )
		{
			if ( blendshapes == null )
				return;

			foreach ( EyelidPositionBlendshape blendShape in blendshapes )
				blendShape.skinnedMeshRenderer.SetBlendShapeWeight( blendShape.index, blendShape.defaultWeight );
		}



		void ResetAllBlendshapesToDefault()
		{
			ResetBlendshapeConfig( blendshapesForBlinking );
			ResetBlendshapeConfig( blendshapesForLookingDown );
			ResetBlendshapeConfig( blendshapesForLookingUp );
		}



		public void RestoreClosed()
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
			{
				leftBoneEyeRotationLimiter.RestoreDefault( );
				rightBoneEyeRotationLimiter.RestoreDefault( );
			}
			else if ( eyeControl == EyeControl.SelectedObjects )
			{
				leftEyeballEyeRotationLimiter.RestoreDefault( );
				rightEyeballEyeRotationLimiter.RestoreDefault( );
			}

			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.RestoreClosed(eyelidBoneMode);
				upperRightLimiter.RestoreClosed(eyelidBoneMode);
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.RestoreClosed(eyelidBoneMode);
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.RestoreClosed(eyelidBoneMode);
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				ResetAllBlendshapesToDefault();

				if ( blendshapesForBlinking != null )
				{
					foreach ( EyelidPositionBlendshape blendShapeForBlinking in blendshapesForBlinking )
						blendShapeForBlinking.skinnedMeshRenderer.
							SetBlendShapeWeight(	blendShapeForBlinking.index,
																blendShapeForBlinking.positionWeight );
				}
			}
		}



		public void RestoreDefault()
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
			{
				leftBoneEyeRotationLimiter.RestoreDefault( );
				rightBoneEyeRotationLimiter.RestoreDefault( );
			}
			else if ( eyeControl == EyeControl.SelectedObjects )
			{
				leftEyeballEyeRotationLimiter.RestoreDefault( );
				rightEyeballEyeRotationLimiter.RestoreDefault( );
			}


			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.RestoreDefault(eyelidBoneMode);
				upperRightLimiter.RestoreDefault(eyelidBoneMode);
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.RestoreDefault(eyelidBoneMode);
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.RestoreDefault(eyelidBoneMode);
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
				ResetAllBlendshapesToDefault();
		}



		public void RestoreLookDown()
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
			{
				leftBoneEyeRotationLimiter.RestoreLookDown( );
				rightBoneEyeRotationLimiter.RestoreLookDown( );
			}
			else if ( eyeControl == EyeControl.SelectedObjects )
			{
				leftEyeballEyeRotationLimiter.RestoreLookDown( );
				rightEyeballEyeRotationLimiter.RestoreLookDown( );
			}

			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.RestoreLookDown(eyelidBoneMode);
				upperRightLimiter.RestoreLookDown(eyelidBoneMode);
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.RestoreLookDown(eyelidBoneMode);
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.RestoreLookDown(eyelidBoneMode);
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				ResetAllBlendshapesToDefault();

				foreach ( EyelidPositionBlendshape blendshapeForLookingDown in blendshapesForLookingDown )
					blendshapeForLookingDown.skinnedMeshRenderer.
						SetBlendShapeWeight(	blendshapeForLookingDown.index,
															blendshapeForLookingDown.positionWeight );
			}
		}



		public void RestoreLookUp()
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
			{
				leftBoneEyeRotationLimiter.RestoreLookUp( );
				rightBoneEyeRotationLimiter.RestoreLookUp( );
			}
			else if ( eyeControl == EyeControl.SelectedObjects )
			{
				leftEyeballEyeRotationLimiter.RestoreLookUp( );
				rightEyeballEyeRotationLimiter.RestoreLookUp( );
			}

			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.RestoreLookUp(eyelidBoneMode);
				upperRightLimiter.RestoreLookUp(eyelidBoneMode);
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.RestoreLookUp(eyelidBoneMode);
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.RestoreLookUp(eyelidBoneMode);
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				ResetAllBlendshapesToDefault();

				foreach ( EyelidPositionBlendshape blendshapeForLookingUp in blendshapesForLookingUp )
					blendshapeForLookingUp.skinnedMeshRenderer.
						SetBlendShapeWeight(	blendshapeForLookingUp.index,
															blendshapeForLookingUp.positionWeight );
			}
		}



		void SaveBlendshapesForEyelidPosition(ref EyelidPositionBlendshape[] blendshapesForPosition, Object rootObject, string positionName)
		{
			SkinnedMeshRenderer[] skinnedMeshRenderers = (rootObject as MonoBehaviour).GetComponentsInChildren<SkinnedMeshRenderer>();
			List<EyelidPositionBlendshape> blendshapeForPositionList = new List<EyelidPositionBlendshape>();
			if ( skinnedMeshRenderers.Length != blendshapesConfigs.Length )
			{
				Debug.LogError("The saved data for open eyelids is invalid. Please reset to open eyelids and resave 'Eyes open, looking straight'.");
				isEyelidBlendshapeDefaultSet = false;
				isEyelidBlendshapeClosedSet = false;
				isEyelidBlendshapeLookDownSet = false;
				isEyelidBlendshapeLookUpSet = false;
			}
			else
			{
				for ( int i=0;  i<skinnedMeshRenderers.Length;  i++ )
				{
					SkinnedMeshRenderer skinnedMeshRenderer = skinnedMeshRenderers[i];
					BlendshapesConfig blendshapesConfig = blendshapesConfigs[i];
					if ( skinnedMeshRenderer != blendshapesConfig.skinnedMeshRenderer || skinnedMeshRenderer.sharedMesh.blendShapeCount != blendshapesConfig.blendShapeCount )
					{
						Debug.LogError("The saved data for open eyelids is invalid. Please reset to open eyelids and resave 'Eyes open, looking straight'.");
						isEyelidBlendshapeDefaultSet = false;
						isEyelidBlendshapeClosedSet = false;
						isEyelidBlendshapeLookDownSet = false;
						isEyelidBlendshapeLookUpSet = false;
					}
					else
					{
						for ( int j=0;  j<blendshapesConfig.blendShapeCount;  j++ )
						{
							const float kEpsilon = 0.01f;
							if ( Mathf.Abs(blendshapesConfig.blendshapeWeights[j] - skinnedMeshRenderer.GetBlendShapeWeight(j)) >= kEpsilon )
							{
								EyelidPositionBlendshape eyePositionBlendshape = new EyelidPositionBlendshape
								{
									skinnedMeshRenderer = skinnedMeshRenderer,
									index = j,
									defaultWeight = blendshapesConfig.blendshapeWeights[j],
									positionWeight = skinnedMeshRenderer.GetBlendShapeWeight(j)
								};
								blendshapeForPositionList.Add( eyePositionBlendshape );
							}
						}
					}
				}
				blendshapesForPosition = blendshapeForPositionList.ToArray();
				Debug.Log("Found " + blendshapesForPosition.Length + " blend shapes for " + positionName + ":");
				foreach ( EyelidPositionBlendshape blendshapeForPosition in blendshapesForPosition )
					Debug.Log(blendshapeForPosition.skinnedMeshRenderer.name + " --> "
							+ blendshapeForPosition.skinnedMeshRenderer.sharedMesh.GetBlendShapeName( blendshapeForPosition.index)
							+ " open: " + blendshapeForPosition.defaultWeight + " closed: " + blendshapeForPosition.positionWeight);
			}
		}



		public void SaveClosed(Object rootObject )
		{
			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.SaveClosed();
				upperRightLimiter.SaveClosed();
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.SaveClosed();
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.SaveClosed();

				isEyelidBonesClosedSet = true;
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				isEyelidBlendshapeClosedSet = true;
				SaveBlendshapesForEyelidPosition(ref blendshapesForBlinking, rootObject, "closed eyes");
			}
		}



		public void SaveDefault( Object rootObject )
		{
			if ( eyeControl == EyeControl.MecanimEyeBones )
			{
				Animator animator = (rootObject as MonoBehaviour).GetComponent<Animator>();
				Transform leftEyeBoneTransform = animator.GetBoneTransform(HumanBodyBones.LeftEye);
				Transform rightEyeBoneTransform = animator.GetBoneTransform(HumanBodyBones.RightEye);

				leftBoneEyeRotationLimiter.SaveDefault( leftEyeBoneTransform );
				rightBoneEyeRotationLimiter.SaveDefault( rightEyeBoneTransform );

				isEyeBoneDefaultSet = true;
			}
			else if ( eyeControl == EyeControl.SelectedObjects )
			{
				leftEyeballEyeRotationLimiter.SaveDefault( leftEye );
				rightEyeballEyeRotationLimiter.SaveDefault( rightEye );

				isEyeBallDefaultSet = true;
			}


			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.SaveDefault(upperEyeLidLeft);
				upperRightLimiter.SaveDefault(upperEyeLidRight);
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.SaveDefault(lowerEyeLidLeft);
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.SaveDefault(lowerEyeLidRight);

				isEyelidBonesDefaultSet = true;
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				SkinnedMeshRenderer[] skinnedMeshRenderers = (rootObject as MonoBehaviour).GetComponentsInChildren<SkinnedMeshRenderer>();
				blendshapesConfigs = new BlendshapesConfig[ skinnedMeshRenderers.Length ];
				for ( int i=0;  i<skinnedMeshRenderers.Length;  i++ )
				{
					BlendshapesConfig blendshapesConfig = new BlendshapesConfig {skinnedMeshRenderer = skinnedMeshRenderers[i]};
					blendshapesConfig.blendShapeCount = blendshapesConfig.skinnedMeshRenderer.sharedMesh.blendShapeCount;
					blendshapesConfig.blendshapeWeights = new float[blendshapesConfig.blendShapeCount ];
					for ( int j=0;  j<blendshapesConfig.blendShapeCount;  j++ )
						blendshapesConfig.blendshapeWeights[j] = blendshapesConfig.skinnedMeshRenderer.GetBlendShapeWeight(j);
					blendshapesConfigs[i] = blendshapesConfig;
				}

				isEyelidBlendshapeDefaultSet = true;
			}
		}



		public void SaveLookDown( Object rootObject )
		{
			bool isEyeBonesControl = eyeControl == EyeControl.MecanimEyeBones;
			bool isEyeballsControl = eyeControl == EyeControl.SelectedObjects;

			if ( isEyeBonesControl )
			{
				leftBoneEyeRotationLimiter.SaveLookDown( );
				rightBoneEyeRotationLimiter.SaveLookDown( );

				isEyeBoneLookDownSet= true;
			}
			else if ( isEyeballsControl )
			{
				leftEyeballEyeRotationLimiter.SaveLookDown();
				rightEyeballEyeRotationLimiter.SaveLookDown();

				isEyeBallLookDownSet = true;
			}

			float leftMaxDownAngle = isEyeBonesControl ? leftBoneEyeRotationLimiter.maxDownAngle : leftEyeballEyeRotationLimiter.maxDownAngle;
			float rightMaxDownAngle = isEyeBonesControl ? rightBoneEyeRotationLimiter.maxDownAngle : rightEyeballEyeRotationLimiter.maxDownAngle;

			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.SaveLookDown(leftMaxDownAngle);
				upperRightLimiter.SaveLookDown(rightMaxDownAngle);
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.SaveLookDown(leftMaxDownAngle);
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.SaveLookDown(rightMaxDownAngle);

				isEyelidBonesLookDownSet = true;
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				isEyelidBlendshapeLookDownSet = true;
				SaveBlendshapesForEyelidPosition(ref blendshapesForLookingDown, rootObject, "looking down");
			}
		}



		public void SaveLookUp( Object rootObject )
		{
			bool isEyeBonesControl = eyeControl == EyeControl.MecanimEyeBones;
			bool isEyeballsControl = eyeControl == EyeControl.SelectedObjects;

			if ( isEyeBonesControl )
			{
				leftBoneEyeRotationLimiter.SaveLookUp( );
				rightBoneEyeRotationLimiter.SaveLookUp( );

				isEyeBoneLookUpSet= true;
			}
			else if ( isEyeballsControl )
			{
				leftEyeballEyeRotationLimiter.SaveLookUp();
				rightEyeballEyeRotationLimiter.SaveLookUp();

				isEyeBallLookUpSet = true;
			}

			float leftMaxUpAngle = isEyeBonesControl ? leftBoneEyeRotationLimiter.maxUpAngle : leftEyeballEyeRotationLimiter.maxUpAngle;
			float rightMaxUpAngle = isEyeBonesControl ? rightBoneEyeRotationLimiter.maxUpAngle : rightEyeballEyeRotationLimiter.maxUpAngle;

			if ( eyelidControl == EyelidControl.Bones )
			{
				upperLeftLimiter.SaveLookUp(leftMaxUpAngle);
				upperRightLimiter.SaveLookUp(rightMaxUpAngle);
				if ( lowerEyeLidLeft != null )
					lowerLeftLimiter.SaveLookUp(leftMaxUpAngle);
				if ( lowerEyeLidRight != null )
					lowerRightLimiter.SaveLookUp(rightMaxUpAngle);

				isEyelidBonesLookUpSet = true;
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				isEyelidBlendshapeLookUpSet = true;
				SaveBlendshapesForEyelidPosition(ref blendshapesForLookingUp, rootObject, "looking up");
			}
		}



		public void UpdateEyelids( float leftEyeAngle, float rightEyeAngle, float blink01, bool eyelidsFollowEyesVertically )
		{
			leftEyeAngle = Utils.NormalizedDegAngle( leftEyeAngle );
			rightEyeAngle = Utils.NormalizedDegAngle( rightEyeAngle );

			if ( eyelidControl == EyelidControl.Bones )
			{
				Quaternion rotation;

				Vector3 position = upperEyeLidLeft.localPosition;
				upperLeftLimiter.GetRotationAndPosition(leftEyeAngle, blink01, eyeWidenOrSquint, true, out rotation, ref position, eyelidBoneMode);
				if ( eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Rotation )
					upperEyeLidLeft.localRotation = rotation;
				if (eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Position)
					upperEyeLidLeft.localPosition = position;

				position = upperEyeLidRight.localPosition;
				upperRightLimiter.GetRotationAndPosition(rightEyeAngle, blink01, eyeWidenOrSquint, true, out rotation, ref position, eyelidBoneMode);
				if ( eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Rotation )
					upperEyeLidRight.localRotation = rotation;
				if (eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Position)
					upperEyeLidRight.localPosition = position;

				if ( lowerEyeLidLeft != null )
				{
					position = lowerEyeLidLeft.localPosition;
					lowerLeftLimiter.GetRotationAndPosition(leftEyeAngle, blink01, eyeWidenOrSquint, false, out rotation, ref position, eyelidBoneMode);
					if ( eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Rotation )
						lowerEyeLidLeft.localRotation = rotation;
					if (eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Position)
						lowerEyeLidLeft.localPosition = position;
				}
				if ( lowerEyeLidRight != null )
				{
					position = lowerEyeLidRight.localPosition;
					lowerRightLimiter.GetRotationAndPosition(rightEyeAngle, blink01, eyeWidenOrSquint, false, out rotation, ref position, eyelidBoneMode);
					if ( eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Rotation )
						lowerEyeLidRight.localRotation = rotation;
					if (eyelidBoneMode == EyelidBoneMode.RotationAndPosition || eyelidBoneMode == EyelidBoneMode.Position)
						lowerEyeLidRight.localPosition = position;
				}
			}
			else if ( eyelidControl == EyelidControl.Blendshapes )
			{
				// TODO , eyeWidenOrSquint for blendshapes
				bool isLookingDown = leftEyeAngle > 0;
				float eyeUp01 = isLookingDown	? 0
																: ((eyeControl == EyeControl.MecanimEyeBones)	? leftBoneEyeRotationLimiter.GetEyeUp01( leftEyeAngle )
																																		: leftEyeballEyeRotationLimiter.GetEyeUp01( leftEyeAngle ));
				float eyeDown01 = !isLookingDown	? 0
																	: ((eyeControl == EyeControl.MecanimEyeBones)	? leftBoneEyeRotationLimiter.GetEyeDown01( leftEyeAngle )
																																			: leftEyeballEyeRotationLimiter.GetEyeDown01( leftEyeAngle ));

				if ( this.eyelidsFollowEyesVertically )
					ResetAllBlendshapesToDefault();
				else
					ResetBlendshapeConfig(blendshapesForBlinking);

				if ( eyelidsFollowEyesVertically )
				{
					if ( isLookingDown )
						LerpBlendshapeConfig( blendshapesForLookingDown, eyeDown01 );
					else
						LerpBlendshapeConfig( blendshapesForLookingUp, eyeUp01 );
				}

				LerpBlendshapeConfig( blendshapesForBlinking, blink01, relativeToCurrentValueIfUsedInOtherConfig: eyelidsFollowEyesVertically );					
			}

			this.eyelidsFollowEyesVertically = eyelidsFollowEyesVertically;
		}

	}

}
