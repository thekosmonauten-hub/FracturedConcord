using UnityEngine;

public struct DamageContext
{
	public string damageType; // "physical","fire","cold","lightning","chaos","poison"
	public bool isAttack, isSpell, isProjectile, isArea, isMelee, isRanged, isDot;
	public string weaponType; // "axe","bow","mace","sword","wand","onehanded","twohanded"
	public bool targetChilled, targetShocked, targetIgnited, targetIsElite;
}

public static class StatsDamageCalculator
{
	public static float ComputeIncreased(CharacterStatsData s, DamageContext ctx)
	{
		float inc = 0f;
		switch (ctx.damageType)
		{
			case "physical": inc += s.increasedPhysicalDamage; break;
			case "fire": inc += s.increasedFireDamage; break;
			case "cold": inc += s.increasedColdDamage; break;
			case "lightning": inc += s.increasedLightningDamage; break;
			case "chaos": inc += s.increasedChaosDamage; break;
		}
		
		// Elemental umbrella
		if (ctx.damageType == "fire" || ctx.damageType == "cold" || ctx.damageType == "lightning")
		{
			inc += s.increasedElementalDamage;
		}
		
		if (ctx.isAttack) inc += s.increasedAttackDamage;
		if (ctx.isSpell) inc += s.increasedSpellDamage;
		if (ctx.isProjectile) inc += s.increasedProjectileDamage;
		if (ctx.isArea) inc += s.increasedAreaDamage;
		if (ctx.isMelee) inc += s.increasedMeleeDamage;
		if (ctx.isRanged) inc += s.increasedRangedDamage;
		
		// Weapon/type specifics
		switch (ctx.weaponType)
		{
			case "axe": inc += s.increasedAxeDamage; break;
			case "bow": inc += s.increasedBowDamage; break;
			case "mace": inc += s.increasedMaceDamage; break;
			case "sword": inc += s.increasedSwordDamage; break;
			case "wand": inc += s.increasedWandDamage; break;
			case "dagger": inc += s.increasedDaggerDamage; break;
			case "onehanded": inc += s.increasedOneHandedDamage; break;
			case "twohanded": inc += s.increasedTwoHandedDamage; break;
		}
		
		// Situational vs target state
		if (ctx.targetChilled) inc += s.increasedDamageVsChilled;
		if (ctx.targetShocked) inc += s.increasedDamageVsShocked;
		if (ctx.targetIgnited) inc += s.increasedDamageVsIgnited;
		
		// DoT/Poison
		if (ctx.isDot) inc += s.increasedDamageOverTime;
		if (ctx.damageType == "poison") inc += s.increasedPoisonDamage;
		
		return Mathf.Max(0f, inc);
	}
	
	public static float ComputeMore(CharacterStatsData s, DamageContext ctx)
	{
		float more = 1f;
		if (ctx.damageType == "physical") more *= s.morePhysicalDamage;
		if (ctx.damageType == "fire") more *= s.moreFireDamage;
		if (ctx.damageType == "cold") more *= s.moreColdDamage;
		if (ctx.damageType == "lightning") more *= s.moreLightningDamage;
		if (ctx.damageType == "chaos") more *= s.moreChaosDamage;
		
		more *= s.moreElementalDamage;
		if (ctx.isSpell) more *= s.moreSpellDamage;
		if (ctx.isAttack) more *= s.moreAttackDamage;
		if (ctx.isProjectile) more *= s.moreProjectileDamage;
		if (ctx.isArea) more *= s.moreAreaDamage;
		if (ctx.isMelee) more *= s.moreMeleeDamage;
		if (ctx.isRanged) more *= s.moreRangedDamage;
		
		return Mathf.Max(0f, more);
	}
	
	public static float ComputeFinalDamage(float baseDamage, CharacterStatsData s, ComputedStats totals, DamageContext ctx)
	{
		var inc = ComputeIncreased(s, ctx);
		var more = ComputeMore(s, ctx);
		float mult = (1f + inc) * more;
		float dmg = baseDamage * mult;
		
		// Apply elite reductions on the target if applicable (incoming damage side)
		if (ctx.targetIsElite)
		{
			dmg *= totals.eliteLessMult;
		}
		
		return Mathf.Max(0f, dmg);
	}
}


