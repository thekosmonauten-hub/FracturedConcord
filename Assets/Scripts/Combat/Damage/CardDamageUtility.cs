using UnityEngine;

public static class CardDamageUtility
{
	/// <summary>
	/// Compute final card damage using pre-aggregated totals.
	/// Use this overload when you already have a cached ComputedStats for the character.
	/// </summary>
	public static float ComputeCardDamage(float baseDamage, CharacterStatsData stats, ComputedStats totals, DamageContext context)
	{
		return StatsDamageCalculator.ComputeFinalDamage(baseDamage, stats, totals, context);
	}
	
	/// <summary>
	/// Compute final card damage, aggregating totals on the fly.
	/// Prefer caching totals per turn/when stats change and using the other overload for performance.
	/// </summary>
	public static float ComputeCardDamage(float baseDamage, CharacterStatsData stats, DamageContext context)
	{
		var totals = StatAggregator.BuildTotals(stats);
		return StatsDamageCalculator.ComputeFinalDamage(baseDamage, stats, totals, context);
	}
}


