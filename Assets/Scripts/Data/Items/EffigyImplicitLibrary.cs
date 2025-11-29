using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EffigyImplicitLibrary
{
    public static void EnsureImplicitModifiers(Effigy effigy, System.Random rng)
    {
        if (effigy == null)
            return;

        // Preserve handcrafted implicit modifiers for uniques
        if (effigy.isUnique && effigy.implicitModifiers != null && effigy.implicitModifiers.Count > 0)
            return;

        EffigyShapeCategory shape = effigy.GetShapeCategory();
        if (shape == EffigyShapeCategory.Unknown)
            return;

        if (rng == null)
            rng = new System.Random();

        Affix implicitAffix = CreateImplicitAffix(effigy, shape, rng);
        if (implicitAffix == null)
            return;

        effigy.implicitModifiers = new List<Affix> { implicitAffix };
    }

    private static Affix CreateImplicitAffix(Effigy effigy, EffigyShapeCategory shape, System.Random rng)
    {
        switch (shape)
        {
            case EffigyShapeCategory.Cross:
                return CreateCrossImplicit(effigy);
            case EffigyShapeCategory.LShape:
                return CreateLifeImplicit(effigy);
            case EffigyShapeCategory.Line:
                return CreateDamageImplicit(effigy);
            case EffigyShapeCategory.SShape:
                return CreateEvasionImplicit(effigy);
            case EffigyShapeCategory.Single:
                return CreateRandomAttributeImplicit(effigy, rng);
            case EffigyShapeCategory.Square:
                return CreateGuardImplicit(effigy);
            case EffigyShapeCategory.SmallL:
                return CreateDamageAfterGuardImplicit(effigy);
            case EffigyShapeCategory.TShape:
                return CreateBuffDurationImplicit(effigy);
            case EffigyShapeCategory.ZShape:
                return CreateAilmentImplicit(effigy);
            default:
                return null;
        }
    }

    private static Affix CreateCrossImplicit(Effigy effigy)
    {
        float value = GetValueBySize(effigy, 3f, 5f, 7f);
        var affix = CreateAffixShell("Sanctified Resonance");
        AddFlatStat(affix, "Strength", value, $"+{value:0} Strength");
        AddFlatStat(affix, "Dexterity", value, $"+{value:0} Dexterity");
        AddFlatStat(affix, "Intelligence", value, $"+{value:0} Intelligence");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateLifeImplicit(Effigy effigy)
    {
        float value = GetValueBySize(effigy, 4f, 6f, 8f);
        var affix = CreateAffixShell("Vital Engraving");
        AddIncreasedStat(affix, "IncreasedMaxLifePercent", value, $"+{value:0}% increased Maximum Life");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateDamageImplicit(Effigy effigy)
    {
        float value = GetValueBySize(effigy, 6f, 8f, 10f);
        var affix = CreateAffixShell("Focused Momentum");
        AddIncreasedStat(affix, "IncreasedDamagePercent", value, $"+{value:0}% increased Damage");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateEvasionImplicit(Effigy effigy)
    {
        float evasion = GetValueBySize(effigy, 6f, 8f, 10f);
        float dodge = GetValueBySize(effigy, 3f, 4f, 5f);
        var affix = CreateAffixShell("Elusive Pattern");
        AddIncreasedStat(affix, "IncreasedEvasionPercent", evasion, $"+{evasion:0}% increased Evasion");
        AddFlatStat(affix, "DodgeChancePercent", dodge, $"+{dodge:0}% Dodge Chance");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateRandomAttributeImplicit(Effigy effigy, System.Random rng)
    {
        string[] stats = { "Strength", "Dexterity", "Intelligence" };
        string chosenStat = stats[rng.Next(stats.Length)];
        int value = rng.Next(1, 4); // 1-3 inclusive

        var affix = CreateAffixShell("Adaptive Core");
        AddFlatStat(affix, chosenStat, value, $"+{value} {chosenStat}");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateGuardImplicit(Effigy effigy)
    {
        float value = GetValueBySize(effigy, 6f, 8f, 10f);
        var affix = CreateAffixShell("Guardian Sigil");
        AddIncreasedStat(affix, "GuardEffectivenessPercent", value, $"+{value:0}% increased Guard Effectiveness");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateDamageAfterGuardImplicit(Effigy effigy)
    {
        float value = GetValueBySize(effigy, 10f, 15f, 20f);
        var affix = CreateAffixShell("Aggressive Riposte");
        AddFlatStat(affix, "DamageAfterGuardPercent", value, $"+{value:0}% increased Damage after Guarding or Attacking");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateBuffDurationImplicit(Effigy effigy)
    {
        float value = GetValueBySize(effigy, 5f, 7f, 10f);
        var affix = CreateAffixShell("Lingering Echo");
        AddIncreasedStat(affix, "BuffDurationPercent", value, $"+{value:0}% increased Buff Duration");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateAilmentImplicit(Effigy effigy)
    {
        float value = GetValueBySize(effigy, 5f, 7f, 10f);
        var affix = CreateAffixShell("Chaotic Surge");
        AddFlatStat(affix, "RandomAilmentChancePercent", value, $"+{value:0}% chance to inflict a Random Ailment on Hit");
        FinaliseDescription(affix);
        return affix;
    }

    private static Affix CreateAffixShell(string name)
    {
        return new Affix(name, string.Empty, AffixType.Prefix, AffixTier.Tier1)
        {
            weight = 0f,
            minLevel = 1,
            hasRange = false,
            rolledValue = 0,
            isRolled = true
        };
    }

    private static void AddFlatStat(Affix affix, string statName, float value, string description)
    {
        var modifier = new AffixModifier(statName, value, value, ModifierType.Flat, ModifierScope.Global)
        {
            description = description,
            minValue = value,
            maxValue = value,
            originalMinValue = value,
            originalMaxValue = value
        };
        affix.modifiers.Add(modifier);
    }

    private static void AddIncreasedStat(Affix affix, string statName, float value, string description)
    {
        var modifier = new AffixModifier(statName, value, value, ModifierType.Increased, ModifierScope.Global)
        {
            description = description,
            minValue = value,
            maxValue = value,
            originalMinValue = value,
            originalMaxValue = value
        };
        affix.modifiers.Add(modifier);
    }

    private static void FinaliseDescription(Affix affix)
    {
        if (affix.modifiers == null || affix.modifiers.Count == 0)
        {
            affix.description = string.Empty;
            return;
        }

        affix.description = string.Join("\n", affix.modifiers.Select(m => m.description));
    }

    private static float GetValueBySize(Effigy effigy, float tiny, float medium, float large)
    {
        switch (effigy.sizeTier)
        {
            case EffigySizeTier.Tiny:
                return tiny;
            case EffigySizeTier.Medium:
                return medium;
            case EffigySizeTier.Large:
                return large;
            default:
                return medium;
        }
    }
}


