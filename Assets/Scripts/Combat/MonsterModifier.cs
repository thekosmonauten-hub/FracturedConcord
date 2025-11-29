using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines a monster modifier (visible modifier for Magic/Rare enemies).
/// Examples: "Hasted", "Armored", "Resistant", etc.
/// </summary>
[CreateAssetMenu(fileName = "New Monster Modifier", menuName = "Dexiled/Combat/Monster Modifier")]
public class MonsterModifier : ScriptableObject
{
    [Header("Basic Info")]
    public string modifierName = "New Modifier";
    [TextArea(2, 4)]
    public string description = "Modifier description";
    
    [Header("Visual")]
    public Color modifierColor = Color.yellow; // Color for UI display
    public Sprite modifierIcon; // Optional icon
    
    [Header("Loot Modifiers")]
    [Tooltip("Multiplier for quantity of items dropped (e.g., 1.5 = 50% more items). Stacks with rarity multipliers.")]
    public float quantityMultiplier = 1f; // Default 1x (no change)
    
    [Tooltip("Multiplier for rarity of items dropped (e.g., 2.0 = 2x better rarity). Stacks with rarity multipliers.")]
    public float rarityMultiplier = 1f; // Default 1x (no change)
    
    [Header("Modifier Weight")]
    [Tooltip("Weight for rarity-based selection. Higher weight = more common. Lower weight = rarer but more rewarding. Default: 100")]
    [Min(1)]
    public int weight = 100; // Default weight (common modifier)
    
    [Header("Modifier Effects")]
    [Tooltip("Simple effect types that trigger automatically (e.g., Shock on hit, No crit damage)")]
    public List<ModifierEffectType> effectTypes = new List<ModifierEffectType>();
    
    [Tooltip("Full EnemyAbility reference for complex behaviors (optional, overrides simple effects)")]
    public EnemyAbility abilityReference;
    
    [Header("Effect Parameters")]
    [Tooltip("Status effect to apply on hit (if effectTypes includes ShockOnHit, etc.)")]
    public StatusEffectType onHitStatusEffect = StatusEffectType.Vulnerable; // Default to Vulnerable (Shock doesn't exist, use Vulnerable as proxy)
    [Tooltip("Magnitude for on-hit status effects")]
    public float onHitStatusMagnitude = 1f;
    [Tooltip("Duration for on-hit status effects")]
    public int onHitStatusDuration = 1;
    
    [Tooltip("Stack type to grant on turn start (if effectTypes includes GrantStackOnTurnStart)")]
    public StackType turnStartStackType = StackType.Tolerance;
    [Tooltip("Number of stacks to grant")]
    public int turnStartStackAmount = 1;
    [Tooltip("Only grant stacks if hit recently (within last turn)")]
    public bool turnStartStackRequiresHitRecently = false;
    
    [Header("Delayed Action Parameters")]
    [Tooltip("Number of turns to delay actions (for DelayedActions effect)")]
    public int delayedActionTurns = 1;
    [Tooltip("Damage bonus per delayed action queued (percentage, for Time-Lagged)")]
    public float damageBonusPerDelayedAction = 10f;
    
    [Header("Other Effect Parameters")]
    [Tooltip("Chance to avoid damage and heal (for TemporalSlip, 0-1)")]
    [Range(0f, 1f)]
    public float temporalSlipChance = 0.1f;
    [Tooltip("Percentage of damage to heal when TemporalSlip triggers")]
    public float temporalSlipHealPercent = 0.1f;
    [Tooltip("Percentage of damage to steal as life (for BloodTither)")]
    public float bloodTitherPercent = 0.05f;
    [Tooltip("Damage bonus per damage taken (for GrudgeForged, percentage)")]
    public float grudgeForgedDamageBonus = 1f;
    [Tooltip("Speed/damage bonus per turn (for OverclockedCore, percentage)")]
    public float overclockedCoreBonusPerTurn = 2f;
    [Tooltip("Extra damage taken (for Threadbare, percentage)")]
    public float threadbareExtraDamage = 20f;
    [Tooltip("Attack efficiency for second attack (for Threadbare, percentage)")]
    public float threadbareAttackEfficiency = 50f;
    
    [Header("Stat Modifications")]
    [Tooltip("Percentage increase to max health (e.g., 50 = +50% max health)")]
    public float healthMultiplier = 0f;
    
    [Tooltip("Percentage increase to damage (e.g., 30 = +30% damage)")]
    public float damageMultiplier = 0f;
    
    [Tooltip("Percentage increase to accuracy rating")]
    public float accuracyMultiplier = 0f;
    
    [Tooltip("Percentage increase to evasion rating")]
    public float evasionMultiplier = 0f;
    
    [Tooltip("Percentage increase to critical chance")]
    public float critChanceMultiplier = 0f;
    
    [Tooltip("Percentage increase to critical multiplier")]
    public float critMultiplierMultiplier = 0f;
    
    [Header("Resistance Modifications")]
    [Tooltip("Flat resistance values (e.g., 25 = +25% resistance)")]
    public float physicalResistance = 0f;
    public float fireResistance = 0f;
    public float coldResistance = 0f;
    public float lightningResistance = 0f;
    public float chaosResistance = 0f;
    
    [Header("Special Effects")]
    [Tooltip("If true, enemy regenerates health each turn")]
    public bool regeneratesHealth = false;
    [Tooltip("Health regenerated per turn (if regeneratesHealth is true)")]
    public float healthRegenPerTurn = 0f;
    
    [Tooltip("If true, enemy has increased movement/action speed")]
    public bool isHasted = false;
    
    [Tooltip("If true, enemy has increased defenses")]
    public bool isArmored = false;
    
    [Tooltip("If true, enemy reflects damage back to attacker")]
    public bool reflectsDamage = false;
    [Tooltip("Percentage of damage reflected (if reflectsDamage is true)")]
    public float reflectPercentage = 0f;
    
    /// <summary>
    /// Get display name for UI
    /// </summary>
    public string GetDisplayName()
    {
        return modifierName;
    }
    
    /// <summary>
    /// Get full description including stat changes
    /// </summary>
    public string GetFullDescription()
    {
        string desc = description;
        
        if (healthMultiplier > 0f)
            desc += $"\n+{healthMultiplier:F0}% Maximum Life";
        if (damageMultiplier > 0f)
            desc += $"\n+{damageMultiplier:F0}% Damage";
        if (accuracyMultiplier > 0f)
            desc += $"\n+{accuracyMultiplier:F0}% Accuracy";
        if (evasionMultiplier > 0f)
            desc += $"\n+{evasionMultiplier:F0}% Evasion";
        if (critChanceMultiplier > 0f)
            desc += $"\n+{critChanceMultiplier:F0}% Critical Strike Chance";
        if (critMultiplierMultiplier > 0f)
            desc += $"\n+{critMultiplierMultiplier:F0}% Critical Strike Multiplier";
        
        if (physicalResistance > 0f)
            desc += $"\n+{physicalResistance:F0}% Physical Resistance";
        if (fireResistance > 0f)
            desc += $"\n+{fireResistance:F0}% Fire Resistance";
        if (coldResistance > 0f)
            desc += $"\n+{coldResistance:F0}% Cold Resistance";
        if (lightningResistance > 0f)
            desc += $"\n+{lightningResistance:F0}% Lightning Resistance";
        if (chaosResistance > 0f)
            desc += $"\n+{chaosResistance:F0}% Chaos Resistance";
        
        if (regeneratesHealth && healthRegenPerTurn > 0f)
            desc += $"\nRegenerates {healthRegenPerTurn:F0} Life per Turn";
        if (isHasted)
            desc += "\nHasted";
        if (isArmored)
            desc += "\nArmored";
        if (reflectsDamage && reflectPercentage > 0f)
            desc += $"\nReflects {reflectPercentage:F0}% of Damage";
        
        if (quantityMultiplier > 1f)
            desc += $"\n{((quantityMultiplier - 1f) * 100f):F0}% increased Quantity of Items Dropped";
        if (rarityMultiplier > 1f)
            desc += $"\n{((rarityMultiplier - 1f) * 100f):F0}% increased Rarity of Items Dropped";
        
        return desc;
    }
}

