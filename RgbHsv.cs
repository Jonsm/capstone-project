using UnityEngine;
using System.Collections;

public static class RgbHsv {
	//HSVToRGB thanks to Bunny83 on UnityAnswers
	//RGBToHSV thanks to tanoshimi on UnityAnswers	

	public static Color HSVToRGB(Vector3 hsv)
	{
		float H = hsv.x;
		float S = hsv.y;
		float V = hsv.z;

		if (S == 0f)
			return new Color(V,V,V);
		else if (V == 0f)
			return Color.black;
		else
		{
			Color col = Color.black;
			float Hval = H * 6f;
			int sel = Mathf.FloorToInt(Hval);
			float mod = Hval - sel;
			float v1 = V * (1f - S);
			float v2 = V * (1f - S * mod);
			float v3 = V * (1f - S * (1f - mod));
			switch (sel + 1)
			{
			case 0:
				col.r = V;
				col.g = v1;
				col.b = v2;
				break;
			case 1:
				col.r = V;
				col.g = v3;
				col.b = v1;
				break;
			case 2:
				col.r = v2;
				col.g = V;
				col.b = v1;
				break;
			case 3:
				col.r = v1;
				col.g = V;
				col.b = v3;
				break;
			case 4:
				col.r = v1;
				col.g = v2;
				col.b = V;
				break;
			case 5:
				col.r = v3;
				col.g = v1;
				col.b = V;
				break;
			case 6:
				col.r = V;
				col.g = v1;
				col.b = v2;
				break;
			case 7:
				col.r = V;
				col.g = v3;
				col.b = v1;
				break;
			}
			col.r = Mathf.Clamp(col.r, 0f, 1f);
			col.g = Mathf.Clamp(col.g, 0f, 1f);
			col.b = Mathf.Clamp(col.b, 0f, 1f);
			return col;
		}
	}

	public static Vector3 RGBToHSV(Color rgbColor)
	{
		Vector3 hsv = new Vector3 ();
		if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
		{
			RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out hsv);
		}
		else
		{
			if (rgbColor.g > rgbColor.r)
			{
				RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out hsv);
			}
			else
			{
				RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out hsv);
			}
		}

		return hsv;
	}

	private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out Vector3 hsv)
	{
		float H = -1, S = -1, V = -1;

		V = dominantcolor;
		if (V != 0f)
		{
			float num = 0f;
			if (colorone > colortwo)
			{
				num = colortwo;
			}
			else
			{
				num = colorone;
			}
			float num2 = V - num;
			if (num2 != 0f)
			{
				S = num2 / V;
				H = offset + (colorone - colortwo) / num2;
			}
			else
			{
				S = 0f;
				H = offset + (colorone - colortwo);
			}
			H /= 6f;
			if (H < 0f)
			{
				H += 1f;
			}
		}
		else
		{
			S = 0f;
			H = 0f;
		}

		hsv = new Vector3 (H, S, V);
	}
}
