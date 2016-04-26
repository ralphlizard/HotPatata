using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlpacaSound.RetroPixelPro
{
	[ExecuteInEditMode]
	[RequireComponent (typeof(Camera))]
	[AddComponentMenu("Image Effects/Custom/Retro Pixel Pro")]
	public class RetroPixelPro : MonoBehaviour
	{
		/// <summary>
		/// The horizontal resolution.
		/// Clamped in the range [1, 16384]
		/// </summary>
		public int horizontalResolution;

		/// <summary>
		/// The vertical resolution.
		/// Clamped in the range [1, 16384]
		/// </summary>
		public int verticalResolution;

		/// <summary>
		/// Alpha of the colorization.
		/// Clamped in the range [0, 1]
		/// </summary>
		public float strength;

		/// <summary>
		/// Don't change this at runtime.
		/// </summary>
		public int numberOfColors;

		/// <summary>
		/// Don't change this at runtime.
		/// </summary>
		public PalettePresets.PresetName preset;

		/// <summary>
		/// Don't change this at runtime.
		/// </summary>
		public Color[] palette;

		/// <summary>
		/// Don't change this at runtime.
		/// </summary>
		public bool[] usedColors;

		/// <summary>
		/// Don't change this at runtime.
		/// </summary>
		public ColorMapPrecision colormapPrecision;

		/// <summary>
		/// Don't change this at runtime.
		/// </summary>
		public Texture3D colormap;

		/// <summary>
		/// Don't change this at runtime.
		/// </summary>
		public Texture2D paletteTexture;

#if UNITY_EDITOR

		public bool autoUpdateColormap;
		public bool isUpdatingColormap;
		ColorMapCalculator calculator;

#endif

		Material m_material;
		Material material
		{
			get
			{
				if (m_material == null)
				{
					string shaderName = "AlpacaSound/RetroPixelPro";
					Shader shader = Shader.Find (shaderName);

					if (shader == null)
					{
						Debug.LogError ("Shader \'" + shaderName + "\' not found. Was it deleted?");
						enabled = false;
						return null;
					}

					m_material = new Material (shader);
					m_material.hideFlags = HideFlags.DontSave;
				}

				return m_material;
			} 
		}


		void Start ()
		{
			if (!SystemInfo.supportsImageEffects)
			{
				Debug.LogWarning("This system does not support image effects.");
				enabled = false;
			}
		}


#if UNITY_EDITOR
		void Reset()
		{
			horizontalResolution = 320;
			verticalResolution = 200;
			palette = new Color[256];
			numberOfColors = 16;
			usedColors = new bool[256];
			colormapPrecision = ColorMapPrecision.Medium;
			preset = PalettePresets.PresetName.Classic1;
			strength = 1;
			autoUpdateColormap = true;

			ApplyPreset();
			UpdateColormap();
		}
#endif


		void OnEnable()
		{
			if (Application.isPlaying)
			{
				Apply ();
			}
			else
			{
#if UNITY_EDITOR
				EditorApplication.update += OnEditorUpdate;

				if (colormap == null)
				{
					UpdateColormap();
				}
				else
				{
					Apply();
				}
#endif
			}
		}
		
		
		void OnDisable()
		{
			if (m_material)
			{
				Material.DestroyImmediate (m_material);
			}

#if UNITY_EDITOR
			isUpdatingColormap = false;
			EditorApplication.update -= OnEditorUpdate;
#endif
		}
		
		
#if UNITY_EDITOR
		void OnEditorUpdate()
		{
			if (Application.isPlaying) 
			{
				return;
			}

			if (isUpdatingColormap)
			{
				if (calculator != null)
				{
					calculator.CalculateChunk();
				}
			}
		}


		public float GetProgress()
		{
			return calculator.progress;
		}


		public void UpdateColormap()
		{
			isUpdatingColormap = true;
			calculator = new ColorMapCalculator(colormapPrecision, palette, usedColors, numberOfColors, DoneUpdatingColormap);
		}


		void DoneUpdatingColormap()
		{
			isUpdatingColormap = false;
			colormap = calculator.colormap;
			Apply();
		}


		public void CancelColormapUpdate()
		{
			isUpdatingColormap = false;
		}


		public void ApplyPreset()
		{
			PalettePresets.ApplyPalette(preset, this);
		}
#endif


		void Apply()
		{
			ApplyColormap();
			GeneratePaletteTexture();
			ApplyPalette();
		}


		void ApplyColormap()
		{
			if (colormap == null)
			{
				Debug.LogWarning("Colormap was null.");
			}
			else
			{
				material.SetTexture("_ColorMap", colormap);
			}
		}


		void ApplyPalette()
		{
			if (palette == null)
			{
				Debug.LogWarning("Palette was null.");
			}
			else
			{
				material.SetTexture("_Palette", paletteTexture);
			}
		}


		void GeneratePaletteTexture()
		{
			paletteTexture = new Texture2D(256, 1, TextureFormat.RGB24, false);
			paletteTexture.filterMode = FilterMode.Point;
			paletteTexture.wrapMode = TextureWrapMode.Clamp;

			for (int i = 0; i < numberOfColors; ++i)
			{
				paletteTexture.SetPixel(i, 0, palette[i]);
			}

			paletteTexture.Apply();
		}
		
		
		public void OnRenderImage (RenderTexture src, RenderTexture dest)
		{
			horizontalResolution = Mathf.Clamp(horizontalResolution, 1, 16384);
			verticalResolution = Mathf.Clamp(verticalResolution, 1, 16384);
			strength = Mathf.Clamp01(strength);

			if (material != null)
			{
				material.SetFloat("_Strength", strength);
				RenderTexture scaled = RenderTexture.GetTemporary (horizontalResolution, (int) verticalResolution);
				scaled.filterMode = FilterMode.Point;
				Graphics.Blit (src, scaled, material);
				//Graphics.Blit (src, scaled);
				Graphics.Blit (scaled, dest);
				RenderTexture.ReleaseTemporary (scaled);
			}
			else
			{
				Graphics.Blit (src, dest);
			}
		}
		
	}
}



