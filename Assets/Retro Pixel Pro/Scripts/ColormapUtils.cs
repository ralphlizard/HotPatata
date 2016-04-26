using UnityEngine;
using System.Collections;


namespace AlpacaSound.RetroPixelPro
{

	public class ColormapUtils
	{

		public static int GetPrecisionColorsteps(ColorMapPrecision precision)
		{
			switch (precision)
			{
			case ColorMapPrecision.Low: return 16;
			case ColorMapPrecision.Medium: return 32;
			case ColorMapPrecision.High: return 64;
			case ColorMapPrecision.Overkill: return 128;
			case ColorMapPrecision.StupidOverkill: return 256;
			default: return 16;
			}
		}


	}
}
