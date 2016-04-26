using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlpacaSound.RetroPixelPro
{

	public class ColorMapCalculator
	{
#if UNITY_EDITOR
		public Texture3D colormap;
		public float progress;

		ColorMapPrecision precision;
		Color[] palette;
		bool[] usedColors;
		int numColors;
		System.Action doneCallback;
		Color32[] pixelBuffer;
		int colorsteps;
		int totalPixels;
		int pixelProgress;



		public ColorMapCalculator(ColorMapPrecision precision, Color[] palette, bool[] usedColors, int numColors, System.Action doneCallback)
		{
			this.precision = precision;
			this.palette = palette;
			this.usedColors = usedColors;
			this.doneCallback = doneCallback;
			this.numColors = numColors;
			progress = 0;
			pixelProgress = 0;
			SetupPixelBuffer();
		}


		void SetupPixelBuffer()
		{
			colorsteps = ColormapUtils.GetPrecisionColorsteps(precision);
			totalPixels = colorsteps * colorsteps * colorsteps;
			pixelBuffer = new Color32[totalPixels];
		}


		public void CalculateChunk()
		{
			double frameStartTime = EditorApplication.timeSinceStartup;

			while (EditorApplication.timeSinceStartup < frameStartTime + (1.0 / 30.0))
			{
				CalculateNextPixel();
			}
		}


		void CalculateNextPixel()
		{
			if (pixelProgress < totalPixels)
			{
				int temp = pixelProgress;

				int r = temp % colorsteps;
				temp /= colorsteps;
				
				int g = temp % colorsteps;
				temp /= colorsteps;
				
				int b = temp % colorsteps;

				CalculatePixel(r, g, b);

				++pixelProgress;
				progress = (float) pixelProgress / (float) totalPixels;
			}
			else
			{
				colormap = new Texture3D(colorsteps, colorsteps, colorsteps, TextureFormat.Alpha8, false);
				colormap.filterMode = FilterMode.Point;
				colormap.wrapMode = TextureWrapMode.Clamp;
				colormap.SetPixels32(pixelBuffer);
				colormap.Apply();

				doneCallback.Invoke();
			}
		}


		void CalculatePixel(int r, int g, int b)
		{
			byte paletteIndex = GetClosestPaletteIndex(r, g, b);
			pixelBuffer[pixelProgress] = new Color32(0, 0, 0, paletteIndex);
		}


		byte GetClosestPaletteIndex(int r, int g, int b)
		{
			float closestDistance = float.MaxValue;
			int closestIndex = 0;
			Vector3 rgb = new Vector3(r, g, b);
			rgb = rgb / (colorsteps-1);
			
			for (int i = 0; i < numColors; ++i)
			{
				if (usedColors[i])
				{
					Vector3 paletteRGB = new Vector3(palette[i].r, palette[i].g, palette[i].b);
					float distance = Vector3.Distance(rgb, paletteRGB);
					if (distance < closestDistance)
					{
						closestDistance = distance;
						closestIndex = i;
					}
				}
			}
			
			return (byte) closestIndex;
		}

#endif
	}

}


