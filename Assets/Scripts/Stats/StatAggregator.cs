using UnityEngine;

public static class StatAggregator
{
	public static ComputedStats BuildTotals(CharacterStatsData s, float flatHealth = 0f, float flatMana = 0f, float flatES = 0f, float flatArmour = 0f, float flatEvasion = 0f)
	{
		var totals = new ComputedStats();
		
		// Base + flats, then apply additive "increased%"
		totals.maxHealth = Mathf.RoundToInt((s.maxHealth + flatHealth) * (1f + Mathf.Max(0f, s.maxHealthIncreased)));
		totals.maxMana = Mathf.RoundToInt((s.maxMana + flatMana) * (1f + Mathf.Max(0f, s.maxManaIncreased)));
		totals.energyShield = Mathf.RoundToInt((s.maxEnergyShield + flatES) * (1f + Mathf.Max(0f, s.energyShieldIncreased)));
		totals.armour = (s.armour + flatArmour) * (1f + Mathf.Max(0f, s.armourIncreased));
		totals.evasion = (s.evasion + flatEvasion) * (1f + Mathf.Max(0f, s.evasionIncreased));
		
		// Guard/Defense utilities
		totals.guardEffectivenessMult = 1f + Mathf.Max(0f, s.guardEffectivenessIncreased);
		totals.eliteLessMult = Mathf.Clamp01(1f - Mathf.Max(0f, s.lessDamageFromElites));
		totals.statusAvoidChance = Mathf.Clamp01(Mathf.Max(0f, s.statusAvoidance));
		
		// Resource gains
		totals.aggressionGainMult = 1f + Mathf.Max(0f, s.aggressionGainIncreased);
		totals.focusGainMult = 1f + Mathf.Max(0f, s.focusGainIncreased);
		
		return totals;
	}
}


