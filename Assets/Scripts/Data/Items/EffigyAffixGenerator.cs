using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates rolled effigy affixes using the shape-specific EffigyAffixDatabase
/// Affixes use their full values (no scaling) since they're designed specifically for effigies
/// </summary>
public static class EffigyAffixGenerator
{
    private const int ExplicitAffixCount = Effigy.ExplicitAffixTarget;

    public static void RollAffixes(Effigy effigy, EffigyAffixDatabase database, ItemRarity? forcedRarity = null, int? seed = null, int? itemLevel = null)
    {
        if (effigy == null)
        {
            Debug.LogWarning("[EffigyAffixGenerator] Effigy is null. No affixes rolled.");
            return;
        }

        if (database == null)
        {
            Debug.LogWarning("[EffigyAffixGenerator] EffigyAffixDatabase is null. No affixes rolled.");
            return;
        }

        if (effigy.isUnique)
        {
            // Unique effigies rely on predefined modifiers.
            return;
        }

        effigy.ClearAffixes();

        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        ItemRarity targetRarity = forcedRarity ?? ItemRarity.Rare;
        int targetAffixCount = ExplicitAffixCount;

        // Get shape-specific affix pools and combine them (effigies don't distinguish between prefix/suffix)
        EffigyShapeCategory shape = effigy.GetShapeCategory();
        int effectiveItemLevel = itemLevel ?? effigy.itemLevel;
        var combinedPool = BuildCombinedAffixPool(database, shape, effectiveItemLevel);

        if (combinedPool.Count == 0)
        {
            Debug.LogWarning($"[EffigyAffixGenerator] No affixes available in database for shape: {shape}");
            return;
        }

        // Roll affixes from the combined pool (no prefix/suffix distinction)
        List<Affix> rolledAffixes = RollAffixesFromPool(combinedPool, targetAffixCount, rng);

        // Apply global scope to all modifiers (no scaling - affixes are designed for effigies)
        ApplyGlobalScope(rolledAffixes);

        // Store all rolled affixes in prefixes list (suffixes remain empty for effigies)
        effigy.prefixes.AddRange(rolledAffixes);
        effigy.suffixes.Clear(); // Effigies don't use suffixes - all affixes go in prefixes

        effigy.rarity = effigy.isUnique ? effigy.GetCalculatedRarity() : ItemRarity.Rare;
    }

    /// <summary>
    /// Build the affix pool for a specific shape, filtered by item level.
    /// Effigies use a unified pool - no prefix/suffix distinction.
    /// Only affixes with minLevel <= itemLevel can be rolled.
    /// </summary>
    private static List<Affix> BuildCombinedAffixPool(EffigyAffixDatabase database, EffigyShapeCategory shape, int itemLevel)
    {
        if (database == null)
            return new List<Affix>();

        // Get unified affix pool for this shape and filter by item level
        var allAffixes = database.GetAllAffixes(shape);
        return allAffixes.Where(a => a != null && a.minLevel <= itemLevel).ToList();
    }

    private static List<Affix> RollAffixesFromPool(List<Affix> pool, int count, System.Random rng)
    {
        List<Affix> rolled = new List<Affix>();
        if (count <= 0 || pool.Count == 0)
            return rolled;

        for (int i = 0; i < count; i++)
        {
            Affix template = WeightedRandomPick(pool, rng);
            if (template == null)
                break;

            Affix rolledAffix = template.GenerateRolledAffix(rng.Next());
            rolled.Add(rolledAffix);
        }

        return rolled;
    }

    private static Affix WeightedRandomPick(List<Affix> pool, System.Random rng)
    {
        if (pool == null || pool.Count == 0)
            return null;

        float totalWeight = pool.Sum(a => a.weight);
        if (totalWeight <= 0f)
            return pool[rng.Next(pool.Count)];

        double roll = rng.NextDouble() * totalWeight;
        double cumulative = 0;

        foreach (var affix in pool)
        {
            cumulative += affix.weight;
            if (roll <= cumulative)
                return affix;
        }

        return pool.Last();
    }


    /// <summary>
    /// Apply global scope to all modifiers. No scaling is applied - affixes are designed at full power for effigies.
    /// </summary>
    private static void ApplyGlobalScope(List<Affix> affixes)
    {
        foreach (var affix in affixes)
        {
            if (affix == null)
                continue;

            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null)
                    continue;

                // Set all modifiers to global scope (effigies affect the character globally)
                modifier.scope = ModifierScope.Global;
            }
        }
    }
}






