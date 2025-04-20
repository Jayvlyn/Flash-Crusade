using UnityEngine;

public static class Utilities
{
	public static int RoundHalfUp(float value)
	{
		return (int)Mathf.Floor(value + 0.5f);
	}
}
