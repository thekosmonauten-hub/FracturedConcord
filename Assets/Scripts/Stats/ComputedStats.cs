using UnityEngine;

public struct ComputedStats
{
	public int maxHealth;
	public int maxMana;
	public int energyShield;
	public float armour;
	public float evasion;
	
	// Guard/Defense utilities
	public float guardEffectivenessMult;	// 1 + guardEffectivenessIncreased
	public float eliteLessMult;				// 1 - lessDamageFromElites
	public float statusAvoidChance;			// 0..1
	
	// Resource gains
	public float aggressionGainMult;		// 1 + aggressionGainIncreased
	public float focusGainMult;				// 1 + focusGainIncreased
}


