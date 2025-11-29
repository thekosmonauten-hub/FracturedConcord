using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates rolled effigy affixes using the shared AffixDatabase
/// </summary>
public static class EffigyAffixGenerator
{
    private const float EffigyScalingFactor = 0.1f;
    private const int ExplicitAffixCount = Effigy.ExplicitAffixTarget;
    private static readonly ItemType[] SourceItemTypes = { ItemType.Weapon, ItemType.Armour, ItemType.Accessory };

    public static void RollAffixes(Effigy effigy, AffixDatabase database, ItemRarity? forcedRarity = null, int? seed = null)
    {
        if (effigy == null)
        {
            Debug.LogWarning("[EffigyAffixGenerator] Effigy is null. No affixes rolled.");
            return;
        }

        if (database == null)
        {
            Debug.LogWarning("[EffigyAffixGenerator] Affix database is null. No affixes rolled.");
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

        var prefixesPool = BuildPrefixPool(database);
        var suffixesPool = BuildSuffixPool(database);

        if (prefixesPool.Count == 0 && suffixesPool.Count == 0)
        {
            Debug.LogWarning("[EffigyAffixGenerator] No affixes available in database.");
            return;
        }

        (int prefixCount, int suffixCount) = AllocatePrefixSuffixCounts(targetAffixCount, rng, prefixesPool.Count, suffixesPool.Count);

        List<Affix> rolledPrefixes = RollAffixesFromPool(prefixesPool, prefixCount, rng);
        List<Affix> rolledSuffixes = RollAffixesFromPool(suffixesPool, suffixCount, rng);

        int totalRolled = rolledPrefixes.Count + rolledSuffixes.Count;
        if (totalRolled < targetAffixCount)
        {
            FillRemainingAffixes(prefixesPool, suffixesPool, rolledPrefixes, rolledSuffixes, targetAffixCount - totalRolled, rng);
        }

        // Ensure we do not exceed the target count
        TrimToTargetCount(rolledPrefixes, rolledSuffixes, targetAffixCount);

        ApplyScalingAndGlobalScope(rolledPrefixes);
        ApplyScalingAndGlobalScope(rolledSuffixes);

        effigy.prefixes.AddRange(rolledPrefixes.Take(targetAffixCount));
        int remainingSlots = Mathf.Max(0, targetAffixCount - effigy.prefixes.Count);
        if (remainingSlots > 0)
        {
            effigy.suffixes.AddRange(rolledSuffixes.Take(remainingSlots));
        }
        else
        {
            // If prefixes already filled all slots, ensure suffixes remain empty for consistency
            effigy.suffixes.Clear();
        }

        effigy.rarity = effigy.isUnique ? effigy.GetCalculatedRarity() : ItemRarity.Rare;
    }

    private static (int prefix, int suffix) AllocatePrefixSuffixCounts(int total, System.Random rng, int availablePrefixes, int availableSuffixes)
    {
        if (total == 0)
            return (0, 0);

        int prefixCount = rng.Next(0, total + 1);
        int suffixCount = total - prefixCount;

        prefixCount = Mathf.Min(prefixCount, availablePrefixes);
        suffixCount = Mathf.Min(suffixCount, availableSuffixes);

        while (prefixCount + suffixCount < total)
        {
            if (prefixCount < availablePrefixes)
                prefixCount++;
            else if (suffixCount < availableSuffixes)
                suffixCount++;
            else
                break;
        }

        return (prefixCount, suffixCount);
    }

    private static List<Affix> BuildPrefixPool(AffixDatabase database)
    {
        List<Affix> pool = new List<Affix>();

        pool.AddRange(database.weaponPrefixCategories.SelectMany(c => c.GetAllAffixes()));
        pool.AddRange(database.armourPrefixCategories.SelectMany(c => c.GetAllAffixes()));
        pool.AddRange(database.jewelleryPrefixCategories.SelectMany(c => c.GetAllAffixes()));

        return pool;
    }

    private static List<Affix> BuildSuffixPool(AffixDatabase database)
    {
        List<Affix> pool = new List<Affix>();

        pool.AddRange(database.weaponSuffixCategories.SelectMany(c => c.GetAllAffixes()));
        pool.AddRange(database.armourSuffixCategories.SelectMany(c => c.GetAllAffixes()));
        pool.AddRange(database.jewellerySuffixCategories.SelectMany(c => c.GetAllAffixes()));

        return pool;
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

    private static void FillRemainingAffixes(
        List<Affix> prefixPool,
        List<Affix> suffixPool,
        List<Affix> rolledPrefixes,
        List<Affix> rolledSuffixes,
        int remaining,
        System.Random rng)
    {
        if (remaining <= 0)
            return;

        List<Affix> combinedPool = new List<Affix>();
        if (prefixPool != null) combinedPool.AddRange(prefixPool);
        if (suffixPool != null) combinedPool.AddRange(suffixPool);

        while (remaining > 0 && combinedPool.Count > 0)
        {
            Affix template = WeightedRandomPick(combinedPool, rng);
            if (template == null)
                break;

            Affix rolled = template.GenerateRolledAffix(rng.Next());
            if (template.affixType == AffixType.Prefix)
            {
                rolledPrefixes.Add(rolled);
            }
            else
            {
                rolledSuffixes.Add(rolled);
            }

            remaining--;
        }
    }

    private static void TrimToTargetCount(List<Affix> prefixes, List<Affix> suffixes, int targetCount)
    {
        int total = prefixes.Count + suffixes.Count;
        if (total <= targetCount)
            return;

        // Prioritize removing extras from the larger group while keeping balance
        while (total > targetCount && (prefixes.Count > 0 || suffixes.Count > 0))
        {
            if (prefixes.Count > suffixes.Count && prefixes.Count > 0)
            {
                prefixes.RemoveAt(prefixes.Count - 1);
            }
            else if (suffixes.Count > 0)
            {
                suffixes.RemoveAt(suffixes.Count - 1);
            }
            else if (prefixes.Count > 0)
            {
                prefixes.RemoveAt(prefixes.Count - 1);
            }

            total = prefixes.Count + suffixes.Count;
        }
    }

    private static void ApplyScalingAndGlobalScope(List<Affix> affixes)
    {
        foreach (var affix in affixes)
        {
            if (affix == null)
                continue;

            affix.minValue = Mathf.RoundToInt(affix.minValue * EffigyScalingFactor);
            affix.maxValue = Mathf.RoundToInt(affix.maxValue * EffigyScalingFactor);
            affix.rolledValue = Mathf.RoundToInt(affix.rolledValue * EffigyScalingFactor);

            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null)
                    continue;

                modifier.scope = ModifierScope.Global;

                modifier.minValue = RoundToTwoDecimals(modifier.minValue * EffigyScalingFactor);
                modifier.maxValue = RoundToTwoDecimals(modifier.maxValue * EffigyScalingFactor);
                modifier.originalMinValue = RoundToTwoDecimals(modifier.originalMinValue * EffigyScalingFactor);
                modifier.originalMaxValue = RoundToTwoDecimals(modifier.originalMaxValue * EffigyScalingFactor);

                if (modifier.isDualRange)
                {
                    modifier.firstRangeMin = RoundToTwoDecimals(modifier.firstRangeMin * EffigyScalingFactor);
                    modifier.firstRangeMax = RoundToTwoDecimals(modifier.firstRangeMax * EffigyScalingFactor);
                    modifier.secondRangeMin = RoundToTwoDecimals(modifier.secondRangeMin * EffigyScalingFactor);
                    modifier.secondRangeMax = RoundToTwoDecimals(modifier.secondRangeMax * EffigyScalingFactor);
                    modifier.rolledFirstValue = RoundToTwoDecimals(modifier.rolledFirstValue * EffigyScalingFactor);
                    modifier.rolledSecondValue = RoundToTwoDecimals(modifier.rolledSecondValue * EffigyScalingFactor);
                }

                modifier.description = BuildModifierDescription(modifier);
            }

            affix.description = BuildAffixDescription(affix);
        }
    }

    private static float RoundToTwoDecimals(float value)
    {
        return (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static string BuildModifierDescription(AffixModifier modifier)
    {
        if (modifier.isDualRange)
        {
            return $"{modifier.statName}: +{modifier.rolledFirstValue:0.##} to +{modifier.rolledSecondValue:0.##}";
        }

        float displayValue = modifier.minValue != 0 ? modifier.minValue : modifier.maxValue;
        string sign = displayValue >= 0 ? "+" : "";
        return $"{modifier.statName}: {sign}{displayValue:0.##}";
    }

    private static string BuildAffixDescription(Affix affix)
    {
        if (affix.modifiers == null || affix.modifiers.Count == 0)
            return affix.description;

        return string.Join("\n", affix.modifiers.Select(m => m.description));
    }
}






