using UnityEngine;

public static class GuardSystem
{
	public static float ComputeGuardGain(float baseGuard, ComputedStats totals)
	{
		return Mathf.Max(0f, baseGuard) * Mathf.Max(0f, totals.guardEffectivenessMult);
	}
	
	public static float ApplyIncomingDamageModifiers(float incoming, bool sourceIsElite, ComputedStats totals)
	{
		float result = Mathf.Max(0f, incoming);
		if (sourceIsElite)
		{
			result *= Mathf.Clamp01(totals.eliteLessMult);
		}
		return result;
	}
}


