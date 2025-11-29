using UnityEngine;

public static class AilmentSystem
{
	public static float ComputeAilmentDuration(CharacterStatsData s, string ailmentId, float baseDurationSec)
	{
		float inc = Mathf.Max(0f, s.statusEffectDuration);
		if (ailmentId == "poison")
		{
			inc += Mathf.Max(0f, s.increasedPoisonDuration);
		}
		// Extend here for per-ailment duration keys if added
		return Mathf.Max(0f, baseDurationSec) * (1f + inc);
	}
	
	public static float ComputeAilmentMagnitude(CharacterStatsData s, string ailmentId, float baseMagnitude)
	{
		float inc = 0f;
		switch (ailmentId)
		{
			case "ignite": inc += Mathf.Max(0f, s.increasedIgniteMagnitude); break;
			case "shock": inc += Mathf.Max(0f, s.increasedShockMagnitude); break;
			case "chill": inc += Mathf.Max(0f, s.increasedChillMagnitude); break;
			case "freeze": inc += Mathf.Max(0f, s.increasedFreezeMagnitude); break;
			case "bleed": inc += Mathf.Max(0f, s.increasedBleedMagnitude); break;
			case "poison": inc += Mathf.Max(0f, s.increasedPoisonMagnitude); break;
		}
		return Mathf.Max(0f, baseMagnitude) * (1f + Mathf.Max(0f, inc));
	}
}


