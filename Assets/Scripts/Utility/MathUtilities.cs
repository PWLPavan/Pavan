using UnityEngine;
using System.Collections;

public static class MathUtilities
{
    public static float Remap(float n, float oldMin, float oldMax, float newMin, float newMax) {
        return (n - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }

    public static int Round(float value, int roundIncrement = 1)
	{
		return ((int) Mathf.Round((float) value / roundIncrement)) * roundIncrement;
	}

	/**
	 * Clamp if possible, but if beyond both values at once, return the averga eof the values. 
	 */
	public static float AverageClamp(float value, float min, float max)
	{
		var a = value < min;
		var b = value > max;

		if (a && b)
			return (min + max) / 2f;

		if (a)
			return min;

		if (b)
			return max;

		return value;
	}

	/**
	 * Ensures that value is at least distance from min or max. In the case that both distance constraints can't
	 * be satisfied at once, the average of the two is returned.
	 */
	public static float MinDistanceClamp(float value, float distance, float min, float max)
	{
		var a = Mathf.Abs(value - min) < distance;
		var b = Mathf.Abs(value - max) < distance;

		if (a && b || (Mathf.Abs(min - max) < distance * 2))
			return (min + max) / 2f;

		if (a)
			return min + distance;

		if (b)
			return max - distance;

		return value;
	}
	
	/**
	 * Round to the nearest 0.5f
	 */
	public static float RoundHalf(float a)
	{
		return Mathf.Round(a * 2) / 2;
	}
	
	/**
	 * Round to the nearest 0.5f.
	 */
	public static Vector3 RoundHalf(Vector3 a)
	{
		return new Vector3(RoundHalf(a.x), RoundHalf(a.y), RoundHalf(a.z));
	}
	
	/**
	 * Round only x and y to nearest .5f
	 */
	public static Vector3 RoundHalfXY(Vector3 a)
	{
		return new Vector3(RoundHalf(a.x), RoundHalf(a.y), a.z);
	}
	
	/**
	 * Round all V3 fields to nearest int.
	 */
	public static Vector3 RoundToInt(Vector3 a)
	{
		return new Vector3(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(a.z));
	}
	
	/**
	 * Get 1D unit of x. If x > 0  return 1, else if x < 0 -1.
	 */
	public static int Unit(float x)
	{
		if (x == 0) return 0;
		return x > 0 ? 1 : -1;
	}

	/**
	 * Rotate a vector PI/2 in some direction
	 */
	public static Vector2 Rot90(this Vector2 v, bool CCW=true)
	{
		if (CCW)
			return new Vector2 (-v.y, v.x);
		else
			return new Vector2(v.y, -v.x);
	}

    public static float TimeIndependentLerp(float inLerpSpeed, float inLerpPeriod = 1.0f)
    {
        return TimeIndependentLerp(inLerpSpeed, inLerpPeriod, Time.deltaTime);
    }

    /// <summary>
    /// Returns a time-independent interpolation value.
    /// Example: LerpSpeed = 0.25 and LerpPeriod = 1.0 means travel 25% of the distance over 1 second.
    /// </summary>
    /// <param name="inLerpSpeed">The percentage distance to travel.  Must be less than 1.</param>
    /// <param name="inLerpPeriod">The time over which the distance is traveled.</param>
    /// <param name="inDeltaTime">The current timestep.</param>
    /// <returns></returns>
    public static float TimeIndependentLerp(float inLerpSpeed, float inLerpPeriod, float inDeltaTime)
    {
        return 1 - Mathf.Pow(1 - inLerpSpeed, inDeltaTime / inLerpPeriod);
    }
}
