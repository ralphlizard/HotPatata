// EyeAndHeadAnimatorEditor.cs
// Tore Knabe
// Copyright 2016 ioccam@ioccam.com


using UnityEngine;
using UnityEditor;

namespace RealisticEyeMovements {

	[CustomEditor( typeof (EyeAndHeadAnimator))]
	public class EyeAndHeadAnimatorEditor : Editor
	{
		#region fields
			readonly string[] eyeControlStringList = { "None", "Mecanim eye bones", "Eye gameobjects" };
			Animator animator;

			const float kLineBuffer = 2;
			ControlData.EyeControl eyeControl;
			ControlData.EyelidControl eyelidControl;
			ControlData.EyelidBoneMode eyelidBoneMode;
			GUIStyle redTextStyle;
			readonly string[] eyelidControlStringList = { "None", "Eyelid bones", "Blendshapes" };
			readonly string[] eyelidBoneModeStringList = {"Rotation and Position", "Rotation", "Position"};
		#endregion


		public override void OnInspectorGUI()
		{
			serializedObject.Update ();

			EyeAndHeadAnimator eyeAndHeadAnimator = (EyeAndHeadAnimator) target;

			DrawDefaultInspector ();
			DrawControlData(eyeAndHeadAnimator);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Export"))
			{
				string filename = EditorUtility.SaveFilePanel("Export settings", "", "REMsettings.dat", "dat");
				if (false == string.IsNullOrEmpty(filename))
					eyeAndHeadAnimator.ExportToFile(filename);
			}
			if (GUILayout.Button("Import"))
			{
				string filename = EditorUtility.OpenFilePanel("Import settings", "", "dat");
				if (false == string.IsNullOrEmpty(filename))
				{
					if (eyeAndHeadAnimator.CanImportFromFile(filename))
					{
						eyeAndHeadAnimator.ImportFromFile(filename);
					}
					else
						EditorUtility.DisplayDialog("Cannot import", "Settings don't match target model", "Ok");
				}
			}
			GUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties ();
		}



		void DrawControlData(EyeAndHeadAnimator eyeAndHeadAnimator)
		{
			if ( redTextStyle == null )
				redTextStyle = new GUIStyle (GUI.skin.label) {normal = {textColor = Color.red}};

			ControlData controlData = eyeAndHeadAnimator.controlData;

			EditorGUI.indentLevel = 0;

			//*** Combobox for eye control
			{
				eyeControl = controlData.eyeControl;
				int selectedIndex = EditorGUILayout.Popup ("Eye control", (int) eyeControl, eyeControlStringList);
				eyeControl = controlData.eyeControl = (ControlData.EyeControl) selectedIndex;
			}


			//*** For eyeball control, slots to assign eye objects
			{
				if ( eyeControl == ControlData.EyeControl.SelectedObjects )
				{
					controlData.leftEye = EditorGUILayout.ObjectField("Left eye", controlData.leftEye, typeof(Transform), true ) as Transform;
					controlData.rightEye = EditorGUILayout.ObjectField("Right eye", controlData.rightEye, typeof(Transform), true ) as Transform;
				}
			}

			//*** bools for eye control
			bool isEyeballControl = eyeControl == ControlData.EyeControl.SelectedObjects;
			bool isEyeBoneControl = eyeControl == ControlData.EyeControl.MecanimEyeBones;

			if ( isEyeBoneControl && false == eyeAndHeadAnimator.gameObject.activeSelf )
			{
				EditorGUILayout.LabelField("Gameobject inactive; activate to show controls.");

				return; // prevent missing animator error message, because the animator cannot be found on a non-active gameobject
			}
			if ( isEyeBoneControl && animator == null )
				animator = eyeAndHeadAnimator.GetComponent<Animator>();
			bool isAnimatorMissing = isEyeBoneControl && animator == null;
			bool areEyeTransformsMissing = isEyeballControl && ( controlData.leftEye == null || controlData.rightEye == null );
			bool areEyeBonesMissing = isEyeBoneControl && animator != null &&
														(null == animator.GetBoneTransform(HumanBodyBones.LeftEye) || null == animator.GetBoneTransform(HumanBodyBones.LeftEye) );

			//*** Error message if any data for eye control is missing
			{
				if ( isEyeBoneControl || isEyeballControl )
				{
					if ( areEyeTransformsMissing )
						EditorGUILayout.LabelField("The eyeballs need to be assigned.", redTextStyle);
					
					if ( isAnimatorMissing )
						EditorGUILayout.LabelField("No Animator found.", redTextStyle);
					
					if ( areEyeBonesMissing )
						EditorGUILayout.LabelField("Eye bones not found; is the Mecanim rig set up correctly?", redTextStyle);

					if ( areEyeTransformsMissing || isAnimatorMissing || areEyeBonesMissing )
					{
						return;
					}
				}
				else
				{
					return;
				}
			}


			//*** Combobox for eyelid control
			{
				eyelidControl = controlData.eyelidControl;
				int selectedIndex = EditorGUILayout.Popup ("Eyelid control", (int) eyelidControl, eyelidControlStringList);
				eyelidControl = controlData.eyelidControl = (ControlData.EyelidControl) selectedIndex;
			}


			//*** Eyelid bone control: assign transforms for the four bones
			{
				if ( eyelidControl == ControlData.EyelidControl.Bones )
				{
					controlData.upperEyeLidLeft = EditorGUILayout.ObjectField("Upper left eyelid", controlData.upperEyeLidLeft, typeof(Transform), true ) as Transform;
					controlData.lowerEyeLidLeft = EditorGUILayout.ObjectField("Lower left eyelid", controlData.lowerEyeLidLeft, typeof(Transform), true ) as Transform;
					controlData.upperEyeLidRight = EditorGUILayout.ObjectField("Upper right eyelid", controlData.upperEyeLidRight, typeof(Transform), true ) as Transform;
					controlData.lowerEyeLidRight = EditorGUILayout.ObjectField("Lower right eyelid", controlData.lowerEyeLidRight, typeof(Transform), true ) as Transform;
				}
			}


			//*** bools for eyelid control
			bool isEyelidBoneControl = eyelidControl == ControlData.EyelidControl.Bones;
			bool isEyelidBlendshapeControl = eyelidControl == ControlData.EyelidControl.Blendshapes;
			bool areEyelidTransformsMissing = isEyelidBoneControl && (controlData.upperEyeLidLeft == null || controlData.upperEyeLidRight == null );

			//*** Error message if eyelid transforms are missing
			{
				if ( areEyelidTransformsMissing )
				{
					EditorGUILayout.LabelField("At least the upper eyelid bones need to be assigned", redTextStyle);

					return;
				}
			}

			//*** Error message if only one of the lower eyelids is assigned
			{
				if ( isEyelidBoneControl && ((controlData.lowerEyeLidLeft == null) != (controlData.lowerEyeLidRight == null) ))
				{
					EditorGUILayout.LabelField("Only one of the lower eyelid bones is assigned", redTextStyle);

					return;
				}
			}

			//*** Error message if an eyelid bone is assigned twice
			{
				if ( eyelidControl == ControlData.EyelidControl.Bones )
				{
					if ( controlData.upperEyeLidLeft != null && controlData.upperEyeLidLeft == controlData.upperEyeLidRight )
					{
						EditorGUILayout.LabelField("Upper Left eyelid bone cannot be the same as Upper Right", redTextStyle);
						return;
					}
					if ( controlData.upperEyeLidLeft != null && controlData.upperEyeLidLeft == controlData.lowerEyeLidLeft )
					{
						EditorGUILayout.LabelField("Upper Left eyelid bone cannot be the same as Lower Left", redTextStyle);
						return;
					}
					if ( controlData.upperEyeLidLeft != null && controlData.upperEyeLidLeft == controlData.lowerEyeLidRight )
					{
						EditorGUILayout.LabelField("Upper Left eyelid bone cannot be the same as Lower Right", redTextStyle);
						return;
					}

					if ( controlData.upperEyeLidRight != null && controlData.upperEyeLidRight == controlData.lowerEyeLidLeft )
					{
						EditorGUILayout.LabelField("Upper Right eyelid bone cannot be the same as Lower Left", redTextStyle);
						return;
					}
					if ( controlData.upperEyeLidRight != null && controlData.upperEyeLidRight == controlData.lowerEyeLidRight )
					{
						EditorGUILayout.LabelField("Upper Right eyelid bone cannot be the same as Lower Right", redTextStyle);
						return;
					}

					if ( controlData.lowerEyeLidLeft != null && controlData.lowerEyeLidLeft == controlData.lowerEyeLidRight )
					{
						EditorGUILayout.LabelField("Lower Left eyelid bone cannot be the same as Lower Right", redTextStyle);
						return;
					}
				}
			}

			//*** Combobox for eyelid bone mode (RotationAndPosition, Rotation, or Position)
			{
				if ( eyelidControl == ControlData.EyelidControl.Bones )
				{
					eyelidBoneMode = controlData.eyelidBoneMode;
					int selectedIndex = EditorGUILayout.Popup ("Eyelid bone mode", (int) eyelidBoneMode, eyelidBoneModeStringList);
					eyelidBoneMode = controlData.eyelidBoneMode = (ControlData.EyelidBoneMode) selectedIndex;
				}
			}

			bool isDefaultSet = false;
			bool isClosedSet = false;
			bool isLookUpSet = false;
			bool isLookDownSet = false;

			//*** Default eye opening
			{
				if ( isEyeBoneControl || isEyeballControl )
				{
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Eyes open, looking straight");
						if ( GUILayout.Button("Save") )
							controlData.SaveDefault( eyeAndHeadAnimator );

						isDefaultSet = true;
						if ( isEyeballControl )
							isDefaultSet &= controlData.isEyeBallDefaultSet;
						if ( isEyeBoneControl )
							isDefaultSet &= controlData.isEyeBoneDefaultSet;
						if ( isEyelidBoneControl )
							isDefaultSet &= controlData.isEyelidBonesDefaultSet;
						if ( isEyelidBlendshapeControl )
							isDefaultSet &= controlData.isEyelidBlendshapeDefaultSet;

						if ( isDefaultSet )
						{
							if ( GUILayout.Button( "Load") )
								controlData.RestoreDefault();
						}
						else
							EditorGUILayout.LabelField( "Not saved yet", redTextStyle);
					EditorGUILayout.EndHorizontal();
				}
			}


			if ( isDefaultSet )
			{
				//*** Closed
				{
					if ( isEyelidBoneControl || isEyelidBlendshapeControl )
					{
						EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("Eyes closed, looking straight");
							if ( GUILayout.Button("Save") )
								controlData.SaveClosed( eyeAndHeadAnimator );

							isClosedSet = true;
							if ( isEyelidBoneControl )
								isClosedSet &= controlData.isEyelidBonesClosedSet;
							if ( isEyelidBlendshapeControl )
								isClosedSet &= controlData.isEyelidBlendshapeClosedSet;

							if ( isClosedSet )
							{
								if ( GUILayout.Button("Load") )
									controlData.RestoreClosed();
							}
							else
								EditorGUILayout.LabelField("Not saved yet", redTextStyle);
						EditorGUILayout.EndHorizontal();
					}
				}

				//*** Looking up
				{
					EditorGUILayout.BeginHorizontal();

								string tooltip = "Rotate " + (isEyeBoneControl ? "eyebones" : "eyes") + " to look up maximally";
								if ( isEyelidBoneControl || isEyelidBlendshapeControl )
									tooltip += ", and adjust eyelid " + (isEyelidBoneControl ? "bone rotation" : "blendshapes") + " for that position";
						EditorGUILayout.LabelField(new GUIContent("Looking up", tooltip));
						if ( GUILayout.Button("Save") )
							controlData.SaveLookUp( eyeAndHeadAnimator );

						isLookUpSet = true;
						if ( isEyeballControl )
							isLookUpSet &= controlData.isEyeBallLookUpSet;
						if ( isEyeBoneControl )
							isLookUpSet &= controlData.isEyeBoneLookUpSet;
						if ( isEyelidBoneControl )
							isLookUpSet &= controlData.isEyelidBonesLookUpSet;
						if ( isEyelidBlendshapeControl )
							isLookUpSet &= controlData.isEyelidBlendshapeLookUpSet;

						if ( isLookUpSet )
						{
							if ( GUILayout.Button("Load") )
								controlData.RestoreLookUp();
						}
						else
							EditorGUILayout.LabelField("Not saved yet", redTextStyle);
					EditorGUILayout.EndHorizontal();
				}

				//*** Looking down
				{
					EditorGUILayout.BeginHorizontal();

								string tooltip = "Rotate " + (isEyeBoneControl ? "eyebones" : "eyes") + " to look down maximally";
								if ( isEyelidBoneControl || isEyelidBlendshapeControl )
									tooltip += ", and adjust eyelid " + (isEyelidBoneControl ? "bone rotation" : "blendshapes") + " for that position";
						EditorGUILayout.LabelField(new GUIContent("Looking down", tooltip));
						if ( GUILayout.Button("Save") )
							controlData.SaveLookDown( eyeAndHeadAnimator );

						isLookDownSet = true;
						if ( isEyeballControl )
							isLookDownSet &= controlData.isEyeBallLookDownSet;
						if ( isEyeBoneControl )
							isLookDownSet &= controlData.isEyeBoneLookDownSet;
						if ( isEyelidBoneControl )
							isLookDownSet &= controlData.isEyelidBonesLookDownSet;
						if ( isEyelidBlendshapeControl )
							isLookDownSet &= controlData.isEyelidBlendshapeLookDownSet;

						if (isLookDownSet)
						{
							if (GUILayout.Button("Load"))
								controlData.RestoreLookDown();
						}
						else
							EditorGUILayout.LabelField("Not saved yet", redTextStyle);

					EditorGUILayout.EndHorizontal();
				}
			}

			if (eyelidControl == ControlData.EyelidControl.Bones)
			{
				const string tooltip = "0: normal. 1: max widened, -1: max squint";
				controlData.eyeWidenOrSquint = EditorGUILayout.Slider(new GUIContent("Eye widen or squint", tooltip), controlData.eyeWidenOrSquint, -1, 1);
			}

		}

	}

}
