using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EASING_TYPE { LINEAR, EXPO_EASE_IN, EXPO_EASE_IN_OUT, EXPO_EASE_OUT, CUBIC_EASE_IN, CUBIC_EASE_OUT, CUBIC_EASE_IN_OUT, SINE_EASE_IN, SINE_EASE_OUT, SINE_EASE_IN_OUT };

public class Utils : MonoBehaviour
{
	private static Utils _instance;
	public static Utils shared { get{ return _instance; } }
	
	void Awake()
	{
		_instance = this;	
	}
	
	public static float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax, bool clamp)
	{
		return Map(value, inputMin, inputMax, outputMin, outputMax, clamp, EASING_TYPE.LINEAR);	
	}
	
	public static float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax, bool clamp, EASING_TYPE easingType)
	{
		float ratio = (value - inputMin) / (inputMax - inputMin);
		
		switch(easingType)
		{
		case EASING_TYPE.EXPO_EASE_IN:
			ratio = ExpoEaseIn(ratio);
			break;
		case EASING_TYPE.EXPO_EASE_IN_OUT:
			ratio = ExpoEaseInOut(ratio);
			break;
		case EASING_TYPE.EXPO_EASE_OUT:
			ratio = ExpoEaseOut(ratio);
			break;
		case EASING_TYPE.CUBIC_EASE_IN:
			ratio = CubicEaseIn(ratio);
			break;
		case EASING_TYPE.CUBIC_EASE_OUT:
			ratio = CubicEaseOut(ratio);
			break;
		case EASING_TYPE.CUBIC_EASE_IN_OUT:
			ratio = CubicEaseInOut(ratio);
			break;
		case EASING_TYPE.SINE_EASE_IN:
			ratio = SineEaseIn(ratio);
			break;
		case EASING_TYPE.SINE_EASE_OUT:
			ratio = SineEaseOut(ratio);
			break;
		case EASING_TYPE.SINE_EASE_IN_OUT:
			ratio = SineEaseInOut(ratio);
			break;
		default:
			break;
		}
		
		float outVal = outputMin + (ratio * (outputMax - outputMin));
		
		if(clamp)
		{
			if(outputMax < outputMin)
			{
				if( outVal < outputMax )outVal = outputMax;
				else if( outVal > outputMin )outVal = outputMin;
			}
			else
			{
				if( outVal > outputMax )outVal = outputMax;
				else if( outVal < outputMin )outVal = outputMin;
			}
		}
		
		return outVal;
	}
	
	public static Vector2 GetVectorFromRotation(float degrees)
	{
		float x = Mathf.Sin(-degrees * Mathf.Deg2Rad);
		float y = Mathf.Cos(degrees * Mathf.Deg2Rad);
		return new Vector2(x, y);
	}
	
	public static float GetRotationFromVector(Vector2 vector)
	{
		return (Mathf.Atan2(vector.y, vector.x) - Mathf.Atan2(Vector2.up.y, Vector2.up.x));
	}
	
	public static Color GetColorFromRGB255(int r, int g, int b)
	{
		return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f);
	}
	
	public static Color GetColorFromRGB255(int r, int g, int b, float a)
	{
		return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, a);
	}
	
	public static Color AddColors(Color originalColor, Color newColor)
	{
		// calculate resulting alpha
		float rAlpha = newColor.a + originalColor.a * (1.0f - newColor.a);
		
		// calculate resulting colors
		float rRed = (newColor.r * newColor.a + originalColor.r * originalColor.a * (1.0f - newColor.a)) / rAlpha;
		float rGreen = (newColor.g * newColor.a + originalColor.g * originalColor.a * (1.0f - newColor.a)) / rAlpha;
		float rBlue = (newColor.b * newColor.a + originalColor.b * originalColor.a * (1.0f - newColor.a)) / rAlpha;
		
		return new Color(rRed, rGreen, rBlue, rAlpha);
	}
	
	public static float ExpoEaseIn(float t) { return Mathf.Pow(2.0f, 10.0f * (t - 1.0f)); }
	public static float ExpoEaseOut(float t) { return 1.0f - Mathf.Pow(2.0f, -10.0f * t); }
	public static float ExpoEaseInOut(float t) { return (t < 0.5f) ? ExpoEaseIn(t * 2.0f) * 0.5f : 1 - ExpoEaseIn(2.0f - t * 2.0f) * 0.5f; }
	
	public static float CubicEaseIn(float t) { return t * t * t; }
	public static float CubicEaseOut(float t) { return 1.0f - CubicEaseIn(1.0f - t); }	
	public static float CubicEaseInOut(float t) { return (t < 0.5f) ? CubicEaseIn(t * 2.0f) * 0.5f : 1.0f - CubicEaseIn(2.0f - t * 2.0f) * 0.5f; }
	
	public static float SineEaseIn(float t) { return 1.0f - Mathf.Cos(t * Mathf.PI * 0.5f); }
	public static float SineEaseOut(float t) { return 1.0f - SineEaseIn(1.0f - t); }
	public static float SineEaseInOut(float t) { return (t < 0.5f) ? SineEaseIn(t * 2.0f) * 0.5f : 1.0f - SineEaseIn(2.0f - t * 2.0f) * 0.5f; }
	
	public static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }
}

public static class Extensions
{
	public static void Shuffle<T>(this IList<T> list)  
	{  
		System.Random rng = new System.Random();  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}
}











