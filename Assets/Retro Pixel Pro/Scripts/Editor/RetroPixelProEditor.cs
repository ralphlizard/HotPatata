
//#define RETROPIXEL_DEBUG

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AlpacaSound.RetroPixelPro
{


	[CustomEditor(typeof(RetroPixelPro))]
	public class RetroPixelProEditor : Editor
	{
		RetroPixelPro _target;
		SerializedProperty horizontalResolution;
		SerializedProperty verticalResolution;
		SerializedProperty preset;
		SerializedProperty numberOfColors;
		SerializedProperty colormapPrecision;
		SerializedProperty strength;
		SerializedProperty palette;
		SerializedProperty usedColors;
		SerializedProperty autoUpdateColormap;

		bool colormapNeedsUpdating;

		float debugPaletteChecker;
		Vector3 debugColormapChecker;

		void OnEnable()
		{
			_target = target as RetroPixelPro;

			horizontalResolution = serializedObject.FindProperty("horizontalResolution");
			verticalResolution = serializedObject.FindProperty("verticalResolution");
			preset = serializedObject.FindProperty("preset");
			numberOfColors = serializedObject.FindProperty("numberOfColors");
			colormapPrecision = serializedObject.FindProperty("colormapPrecision");
			strength = serializedObject.FindProperty("strength");
			palette = serializedObject.FindProperty("palette");
			usedColors = serializedObject.FindProperty("usedColors");
			autoUpdateColormap = serializedObject.FindProperty("autoUpdateColormap");

			colormapNeedsUpdating = false;

			Undo.undoRedoPerformed += OnUndoRedo;
		}


		void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndoRedo;
		}


		void OnUndoRedo()
		{
			colormapNeedsUpdating = true;
		}


		public override void OnInspectorGUI()
		{
			if (autoUpdateColormap.boolValue && colormapNeedsUpdating)
			{
				UpdateColormap();
			}

			serializedObject.Update ();

			DrawDebugStuff();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("RUNTIME PROPERTIES", EditorStyles.boldLabel);
			++EditorGUI.indentLevel;

			horizontalResolution.intValue = EditorGUILayout.IntField("Horizontal Resolution", horizontalResolution.intValue);
			verticalResolution.intValue = EditorGUILayout.IntField("Vertical Resolution", verticalResolution.intValue);
			EditorGUILayout.Slider(strength, 0, 1, "Strength");

			EditorGUILayout.Space();
			--EditorGUI.indentLevel;

			if (!Application.isPlaying)
			{
				DrawStaticProperties();
			}

			serializedObject.ApplyModifiedProperties ();

		}


		void DrawStaticProperties()
		{
			EditorGUILayout.LabelField("STATIC PROPERTIES", EditorStyles.boldLabel);
			++EditorGUI.indentLevel;

			PalettePresets.PresetName oldPalettePreset = _target.preset;
			PalettePresets.PresetName newPalettePreset = (PalettePresets.PresetName) EditorGUILayout.EnumPopup("Palette Preset", _target.preset);
			preset.enumValueIndex = (int) newPalettePreset;
			if (newPalettePreset != oldPalettePreset)
			{
				serializedObject.ApplyModifiedProperties();
				_target.ApplyPreset();
				serializedObject.Update();
				colormapNeedsUpdating = true;
			}

			int oldNumberOfColors = numberOfColors.intValue;
			numberOfColors.intValue = EditorGUILayout.IntField("Number Of Colors", numberOfColors.intValue);

			if (numberOfColors.intValue != oldNumberOfColors)
			{
				colormapNeedsUpdating = true;
				if (numberOfColors.intValue < oldNumberOfColors)
				{
					preset.enumValueIndex = (int) PalettePresets.PresetName.Custom;
				}
			}

			int oldPrecision = colormapPrecision.enumValueIndex;
			colormapPrecision.enumValueIndex = (int) (ColorMapPrecision) EditorGUILayout.EnumPopup("Colormap Precision", _target.colormapPrecision);

			if (oldPrecision != colormapPrecision.enumValueIndex)
			{
				colormapNeedsUpdating = true;
			}

			autoUpdateColormap.boolValue = EditorGUILayout.Toggle("Auto Update Colormap", autoUpdateColormap.boolValue);
		
			--EditorGUI.indentLevel;

			EditorGUI.BeginDisabledGroup(autoUpdateColormap.boolValue);
			if (!_target.isUpdatingColormap)
			{
				if (GUILayout.Button("Update Colormap", GUILayout.Width(130), GUILayout.Height(20)))
				{
					UpdateColormap();
				}
			}
			EditorGUI.EndDisabledGroup();

			if (_target.isUpdatingColormap)
			{
				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button("Cancel", GUILayout.Width(130), GUILayout.Height(20)))
				{
					_target.CancelColormapUpdate();
				}

				Rect progressRect = GUILayoutUtility.GetRect (0, 26, GUILayout.ExpandWidth (true));
				EditorGUI.ProgressBar(progressRect, _target.GetProgress(), "Updating Colormap");
				EditorUtility.SetDirty(target);

				EditorGUILayout.EndHorizontal();
			}

			DrawColors();

			EditorGUILayout.Space();
		}


		void DrawDebugStuff()
		{
#if RETROPIXEL_DEBUG
			if (_target.paletteTexture != null && _target.colormap != null)
			{
				EditorGUILayout.LabelField("DEBUG PROPERTIES", EditorStyles.boldLabel);
				++EditorGUI.indentLevel;

				debugPaletteChecker = EditorGUILayout.Slider("Palette Checker", debugPaletteChecker, 0, 1);
				int palettePixel = Mathf.RoundToInt(debugPaletteChecker * (_target.numberOfColors-1));
				EditorGUILayout.LabelField("Palette Pixel", "" + palettePixel);
				EditorGUILayout.ColorField("Palette Color", _target.paletteTexture.GetPixel(palettePixel, 0));

				debugColormapChecker = EditorGUILayout.Vector3Field("ColormapChecker", debugColormapChecker);
				debugColormapChecker = new Vector3(Mathf.Clamp01(debugColormapChecker.x), Mathf.Clamp01(debugColormapChecker.y), Mathf.Clamp01(debugColormapChecker.z));
				int colorsteps = ColormapUtils.GetPrecisionColorsteps(_target.colormapPrecision);
				Vector3 colormapPixel = new Vector3(Mathf.RoundToInt(debugColormapChecker.x * (colorsteps-1)),
				                                    Mathf.RoundToInt(debugColormapChecker.y * (colorsteps-1)),
				                                    Mathf.RoundToInt(debugColormapChecker.z * (colorsteps-1)));
				int pixelInArray = Mathf.FloorToInt(colormapPixel.x + colormapPixel.y * colorsteps + colormapPixel.z * colorsteps * colorsteps);
				EditorGUILayout.LabelField("Colormap Pixel", "" + colormapPixel + " :: " + pixelInArray);
				int alpha = _target.colormap.GetPixels32()[pixelInArray].a;
				EditorGUILayout.LabelField("Palette Index (Alpha)", "" + alpha);
				EditorGUILayout.ColorField("Final Color", _target.paletteTexture.GetPixel(alpha, 1));

				--EditorGUI.indentLevel;
			}
#endif
		}


		void DrawColors()
		{
			for (int i = 0; i < numberOfColors.intValue; i+=4)
			{
				EditorGUILayout.BeginHorizontal ();
				
				for (int j = 0; j < 4; ++j)
				{
					if (i+j < numberOfColors.intValue)
					{
						SerializedProperty used = usedColors.GetArrayElementAtIndex(i+j);
						bool newUsed = EditorGUILayout.Toggle(used.boolValue, GUILayout.Width(15));

						if (newUsed != used.boolValue)
						{
							preset.enumValueIndex = (int) PalettePresets.PresetName.Custom;
							colormapNeedsUpdating = true;
						}

						used.boolValue = newUsed;

						SerializedProperty color = palette.GetArrayElementAtIndex(i+j);
						
						if (used.boolValue)
						{
							Color newColor = EditorGUILayout.ColorField (GUIContent.none, color.colorValue, false, false, false, null, GUILayout.Width(40), GUILayout.Height(25));
							
							if (newColor != color.colorValue)
							{
								preset.enumValueIndex = (int) PalettePresets.PresetName.Custom;
								colormapNeedsUpdating = true;
							}
							
							color.colorValue = newColor;
						}
						else
						{
							EditorGUI.BeginDisabledGroup(true);
							EditorGUILayout.ColorField (GUIContent.none, DisabledColor(color.colorValue), false, false, false, null, GUILayout.Width(40), GUILayout.Height(25));
							EditorGUI.EndDisabledGroup();
						}
					}
					else
					{
						GUILayout.Space(67);
					}
					
					EditorGUILayout.Space();
				}
				
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space();
			}
		}


		Color DisabledColor(Color color)
		{
			return Color.Lerp(Color.white, color, 0.5f);
		}


		void UpdateColormap()
		{
			_target.UpdateColormap();
			colormapNeedsUpdating = false;
		}





	}

}
