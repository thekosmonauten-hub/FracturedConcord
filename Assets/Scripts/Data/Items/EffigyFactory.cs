using System;
using System.Collections.Generic;
using UnityEngine;

public static class EffigyFactory
{
    /// <summary>
    /// Creates a runtime instance of an effigy blueprint, deep copying implicit modifiers and rolling new affixes.
    /// </summary>
    public static Effigy CreateInstance(Effigy blueprint, EffigyAffixDatabase effigyAffixDatabase, ItemRarity? forcedRarity = null, int? seed = null, int? itemLevel = null)
    {
        if (blueprint == null)
        {
            Debug.LogWarning("[EffigyFactory] Cannot create instance from null blueprint.");
            return null;
        }

        Effigy runtimeEffigy = ScriptableObject.Instantiate(blueprint);
        runtimeEffigy.name = $"{blueprint.name}_Instance";
        runtimeEffigy.itemName = blueprint.itemName;
        runtimeEffigy.effigyName = blueprint.effigyName;
        runtimeEffigy.itemName = string.IsNullOrEmpty(runtimeEffigy.displayAlias)
            ? runtimeEffigy.effigyName
            : runtimeEffigy.displayAlias;

        // Don't clone implicitModifiers from blueprint - they will be generated based on shape
        // This prevents duplicates if the blueprint already has an implicit stored
        runtimeEffigy.implicitModifiers = new List<Affix>();
        runtimeEffigy.prefixes = new List<Affix>();
        runtimeEffigy.suffixes = new List<Affix>();
        runtimeEffigy.itemTags = new List<string>(blueprint.itemTags);
        
        // Set item level (for affix tier gating)
        if (itemLevel.HasValue)
        {
            runtimeEffigy.itemLevel = itemLevel.Value;
        }
        else
        {
            // Default to requiredLevel if itemLevel not specified
            runtimeEffigy.itemLevel = runtimeEffigy.requiredLevel;
        }

        int implicitSeed = seed.HasValue
            ? seed.Value ^ (blueprint.effigyName?.GetHashCode() ?? 0)
            : Environment.TickCount ^ runtimeEffigy.GetHashCode();
        EffigyImplicitLibrary.EnsureImplicitModifiers(runtimeEffigy, new System.Random(implicitSeed));

        ItemRarity effectiveRarity = forcedRarity ?? ItemRarity.Rare;

        if (effigyAffixDatabase != null)
        {
            int affixSeed = seed ?? implicitSeed;
            EffigyAffixGenerator.RollAffixes(runtimeEffigy, effigyAffixDatabase, effectiveRarity, affixSeed, runtimeEffigy.itemLevel);
        }
        else
        {
            Debug.LogWarning("[EffigyFactory] EffigyAffixDatabase was null. Effigy will not receive rolled affixes.");
        }

        runtimeEffigy.rarity = runtimeEffigy.isUnique ? runtimeEffigy.GetCalculatedRarity() : ItemRarity.Rare;
        return runtimeEffigy;
    }

    private static List<Affix> CloneAffixList(List<Affix> source)
    {
        List<Affix> clones = new List<Affix>();
        if (source == null)
            return clones;

        foreach (var affix in source)
        {
            if (affix == null)
                continue;

            clones.Add(CloneAffix(affix));
        }

        return clones;
    }

    public static Affix CloneAffix(Affix source)
    {
        if (source == null)
            return null;

        Affix clone = new Affix(source.name, source.description, source.affixType, source.tier)
        {
            requiredTags = new List<string>(source.requiredTags),
            compatibleTags = new List<string>(source.compatibleTags),
            weight = source.weight,
            handedness = source.handedness,
            minLevel = source.minLevel,
            minValue = source.minValue,
            maxValue = source.maxValue,
            hasRange = source.hasRange,
            rolledValue = source.rolledValue,
            isRolled = source.isRolled
        };

        foreach (var mod in source.modifiers)
        {
            if (mod == null)
                continue;

            clone.modifiers.Add(CloneModifier(mod));
        }

        return clone;
    }

    public static AffixModifier CloneModifier(AffixModifier source)
    {
        AffixModifier clone;

        if (source.isDualRange)
        {
            clone = new AffixModifier(
                source.statName,
                source.firstRangeMin,
                source.firstRangeMax,
                source.secondRangeMin,
                source.secondRangeMax,
                source.modifierType,
                source.scope
            );
        }
        else
        {
            clone = new AffixModifier(source.statName, source.minValue, source.maxValue, source.modifierType, source.scope);
        }

        clone.damageType = source.damageType;
        clone.description = source.description;
        clone.originalMinValue = source.originalMinValue;
        clone.originalMaxValue = source.originalMaxValue;
        clone.isDualRange = source.isDualRange;
        clone.firstRangeMin = source.firstRangeMin;
        clone.firstRangeMax = source.firstRangeMax;
        clone.secondRangeMin = source.secondRangeMin;
        clone.secondRangeMax = source.secondRangeMax;
        clone.rolledFirstValue = source.rolledFirstValue;
        clone.rolledSecondValue = source.rolledSecondValue;
        clone.minValue = source.minValue;
        clone.maxValue = source.maxValue;

        return clone;
    }
}

