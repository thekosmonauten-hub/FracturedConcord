using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject representing an Embossing effect that can be applied to cards
/// Embossings increase mana cost based on flat cost + percentage multiplier
/// </summary>
[CreateAssetMenu(fileName = "New Embossing", menuName = "Card System/Embossing Effect")]
public class EmbossingEffect : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Display name (e.g., 'of Ferocity')")]
    public string embossingName = "of Unnamed";
    
    [Tooltip("Unique ID for this embossing (auto-generated from name)")]
    public string embossingId;
    
    [TextArea(2, 4)]
    [Tooltip("Description shown to player")]
    public string description = "";
    
    [Header("Visual")]
    public Sprite embossingIcon;
    public Color embossingColor = Color.white;
    
    [Header("Category")]
    public EmbossingCategory category = EmbossingCategory.Damage;
    public EmbossingRarity rarity = EmbossingRarity.Common;
    
    [Header("Mana Cost Impact")]
    [Tooltip("Mana cost multiplier (0.25 = +25%, 0.35 = +35%)")]
    [Range(0f, 2f)]
    public float manaCostMultiplier = 0.25f;
    
    [Tooltip("Additional flat mana cost (applied after multiplier)")]
    public int flatManaCostIncrease = 0;
    
    [Header("Requirements")]
    [Tooltip("Minimum character level required to apply this embossing")]
    public int minimumLevel = 1;
    
    [Tooltip("Minimum Strength required (0 = no requirement)")]
    public int minimumStrength = 0;
    
    [Tooltip("Minimum Dexterity required (0 = no requirement)")]
    public int minimumDexterity = 0;
    
    [Tooltip("Minimum Intelligence required (0 = no requirement)")]
    public int minimumIntelligence = 0;
    
    [Header("Effect Mechanics")]
    [Tooltip("Type of effect this embossing applies")]
    public EmbossingEffectType effectType = EmbossingEffectType.DamageMultiplier;
    
    [Tooltip("Primary effect value (usage depends on effect type)")]
    public float effectValue = 0f;
    
    [Tooltip("Secondary effect value (for complex effects)")]
    public float secondaryEffectValue = 0f;
    
    [Tooltip("Element type for conversion/damage effects")]
    public DamageType elementType = DamageType.Physical;
    
    [Tooltip("Status effect to apply (if applicable, leave as Poison if not using)")]
    public StatusEffectType statusEffect = StatusEffectType.Poison;
    
    [Tooltip("Chance to apply status effect (0-1, where 1 = 100%)")]
    [Range(0f, 1f)]
    public float statusEffectChance = 0f;
    
    [Header("Special Flags")]
    [Tooltip("Can only be applied once per card")]
    public bool unique = false;
    
    [Tooltip("Mutually exclusive with other embossings in this group")]
    public string exclusivityGroup = "";
    
    [Tooltip("Custom effect logic identifier (for special effects)")]
    public string customEffectId = "";
    
    [Header("Modifier Definitions")]
    [Tooltip("List of modifier IDs that define this embossing's effects (auto-linked from EmbossingModifierDefinition assets)")]
    public List<string> modifierIds = new List<string>();
    
    /// <summary>
    /// Auto-generate ID from name on validation
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(embossingId) || embossingId == "")
        {
            embossingId = GenerateIdFromName(embossingName);
        }
    }
    
    /// <summary>
    /// Generate a unique ID from the embossing name
    /// </summary>
    private string GenerateIdFromName(string name)
    {
        return name.ToLower()
            .Replace(" ", "_")
            .Replace("of_", "")
            .Replace("the_", "");
    }
    
    /// <summary>
    /// Check if character meets requirements to apply this embossing
    /// </summary>
    public bool MeetsRequirements(Character character)
    {
        if (character == null) return false;
        
        // Level check
        if (character.level < minimumLevel)
            return false;
        
        // Stat checks
        if (character.strength < minimumStrength)
            return false;
        
        if (character.dexterity < minimumDexterity)
            return false;
        
        if (character.intelligence < minimumIntelligence)
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Get formatted requirement text
    /// </summary>
    public string GetRequirementsText()
    {
        string text = "";
        
        if (minimumLevel > 1)
            text += $"Level {minimumLevel}\n";
        
        if (minimumStrength > 0)
            text += $"Strength {minimumStrength}\n";
        
        if (minimumDexterity > 0)
            text += $"Dexterity {minimumDexterity}\n";
        
        if (minimumIntelligence > 0)
            text += $"Intelligence {minimumIntelligence}\n";
        
        return text.TrimEnd('\n');
    }
    
    /// <summary>
    /// Calculate the mana cost increase for a card with this embossing
    /// Formula: (Base + N_embossings) × (1 + Σ Multipliers) + Flat increases
    /// </summary>
    public int CalculateManaCostIncrease(int baseManaCost, int totalEmbossings, float totalMultiplier)
    {
        // Calculate multiplied cost
        float multipliedCost = (baseManaCost + totalEmbossings) * (1 + totalMultiplier);
        
        // Add flat increase
        return Mathf.CeilToInt(multipliedCost) + flatManaCostIncrease - baseManaCost;
    }
    
    /// <summary>
    /// Get color based on embossing category
    /// </summary>
    public Color GetTypeColor()
    {
        switch (category)
        {
            case EmbossingCategory.Damage: return new Color(0.8f, 0.2f, 0.2f); // Red
            case EmbossingCategory.Scaling: return new Color(0.2f, 0.8f, 0.2f); // Green
            case EmbossingCategory.Utility: return new Color(0.2f, 0.5f, 0.8f); // Blue
            case EmbossingCategory.Defensive: return new Color(0.5f, 0.5f, 0.8f); // Purple
            case EmbossingCategory.Combo: return new Color(0.8f, 0.6f, 0.2f); // Orange
            case EmbossingCategory.Ailment: return new Color(0.6f, 0.2f, 0.8f); // Violet
            case EmbossingCategory.Chaos: return new Color(0.8f, 0.2f, 0.6f); // Magenta
            case EmbossingCategory.Conversion: return new Color(0.2f, 0.8f, 0.8f); // Cyan
            default: return Color.gray;
        }
    }
    
    /// <summary>
    /// Get color based on embossing rarity
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case EmbossingRarity.Common: return Color.white;
            case EmbossingRarity.Uncommon: return new Color(0.3f, 1f, 0.3f); // Light green
            case EmbossingRarity.Rare: return new Color(0.3f, 0.5f, 1f); // Light blue
            case EmbossingRarity.Epic: return new Color(0.8f, 0.3f, 1f); // Purple
            case EmbossingRarity.Legendary: return new Color(1f, 0.6f, 0f); // Orange/gold
            default: return Color.gray;
        }
    }
    
    /// <summary>
    /// Alias for embossingIcon (for compatibility with UI code)
    /// </summary>
    public Sprite icon
    {
        get { return embossingIcon; }
        set { embossingIcon = value; }
    }
    
    /// <summary>
    /// Get formatted requirements text with color coding based on character stats
    /// </summary>
    public string GetRequirementsTextColored(Character character)
    {
        if (character == null)
            return GetRequirementsText();
        
        string text = "";
        
        if (minimumLevel > 1)
        {
            bool meetsLevel = character.level >= minimumLevel;
            string color = meetsLevel ? "green" : "red";
            text += $"<color={color}>Level {minimumLevel}";
            if (!meetsLevel)
            {
                int needed = minimumLevel - character.level;
                text += $" (Need {needed} more)";
            }
            text += "</color>\n";
        }
        
        if (minimumStrength > 0)
        {
            bool meetsStr = character.strength >= minimumStrength;
            string color = meetsStr ? "green" : "red";
            text += $"<color={color}>Strength {minimumStrength}";
            if (!meetsStr)
            {
                int needed = minimumStrength - character.strength;
                text += $" (Need {needed} more)";
            }
            text += "</color>\n";
        }
        
        if (minimumDexterity > 0)
        {
            bool meetsDex = character.dexterity >= minimumDexterity;
            string color = meetsDex ? "green" : "red";
            text += $"<color={color}>Dexterity {minimumDexterity}";
            if (!meetsDex)
            {
                int needed = minimumDexterity - character.dexterity;
                text += $" (Need {needed} more)";
            }
            text += "</color>\n";
        }
        
        if (minimumIntelligence > 0)
        {
            bool meetsInt = character.intelligence >= minimumIntelligence;
            string color = meetsInt ? "green" : "red";
            text += $"<color={color}>Intelligence {minimumIntelligence}";
            if (!meetsInt)
            {
                int needed = minimumIntelligence - character.intelligence;
                text += $" (Need {needed} more)";
            }
            text += "</color>\n";
        }
        
        if (string.IsNullOrEmpty(text))
        {
            return "<color=green>No requirements</color>";
        }
        
        return text.TrimEnd('\n');
    }
    
    /// <summary>
    /// Get human-readable effect description based on effect type and values
    /// If description field is set and seems more detailed, use that instead
    /// </summary>
    public string GetEffectDescription()
    {
        // For CustomEffect, prioritize the description field if available
        if (effectType == EmbossingEffectType.CustomEffect && !string.IsNullOrEmpty(description))
        {
            return description;
        }
        
        // Generate description based on effect type
        string generatedDesc = GetGeneratedEffectDescription();
        
        // If description field is set and longer/more detailed, use it instead
        if (!string.IsNullOrEmpty(description) && description.Length > generatedDesc.Length)
        {
            return description;
        }
        
        return generatedDesc;
    }
    
    /// <summary>
    /// Generate effect description from effect type and values
    /// </summary>
    private string GetGeneratedEffectDescription()
    {
        switch (effectType)
        {
            // Damage types
            case EmbossingEffectType.DamageMultiplier:
                return $"+{(effectValue * 100):F0}% more damage";
            
            case EmbossingEffectType.PhysicalDamageMultiplier:
                return $"+{(effectValue * 100):F0}% more physical damage";
            
            case EmbossingEffectType.ElementalDamageMultiplier:
                return $"+{(effectValue * 100):F0}% more elemental damage";
            
            case EmbossingEffectType.SpellDamageMultiplier:
                return $"+{(effectValue * 100):F0}% more spell damage";
            
            case EmbossingEffectType.FlatDamageBonus:
                return $"+{effectValue:F0} flat damage";
            
            // Conversion types
            case EmbossingEffectType.PhysicalToFireConversion:
                return $"Convert {(effectValue * 100):F0}% of physical damage to fire";
            
            case EmbossingEffectType.PhysicalToColdConversion:
                return $"Convert {(effectValue * 100):F0}% of physical damage to cold";
            
            case EmbossingEffectType.PhysicalToLightningConversion:
                return $"Convert {(effectValue * 100):F0}% of physical damage to lightning";
            
            case EmbossingEffectType.ElementalToChaosConversion:
                return $"Convert {(effectValue * 100):F0}% of elemental damage to chaos";
            
            // Scaling types
            case EmbossingEffectType.StrengthScaling:
                return $"+{(effectValue * 100):F1}% damage per point of Strength";
            
            case EmbossingEffectType.DexterityScaling:
                return $"+{(effectValue * 100):F1}% damage per point of Dexterity";
            
            case EmbossingEffectType.IntelligenceScaling:
                return $"+{(effectValue * 100):F1}% damage per point of Intelligence";
            
            case EmbossingEffectType.EmbossingCountScaling:
                return $"+{(effectValue * 100):F0}% damage per embossing on this card";
            
            // Utility types
            case EmbossingEffectType.ManaCostReduction:
                return $"-{(effectValue * 100):F0}% mana cost";
            
            case EmbossingEffectType.CriticalChance:
                return $"+{(effectValue * 100):F0}% critical strike chance";
            
            case EmbossingEffectType.CriticalMultiplier:
                return $"+{(effectValue * 100):F0}% critical strike multiplier";
            
            case EmbossingEffectType.LifeOnHit:
                return $"Gain {effectValue:F0} life on hit";
            
            case EmbossingEffectType.LifeLeech:
                return $"Leech {(effectValue * 100):F1}% of damage as life";
            
            case EmbossingEffectType.CardDuplication:
                return $"{(effectValue * 100):F0}% chance to create a copy of this card when played";
            
            case EmbossingEffectType.PrepareCharges:
                return $"Card prepares with {effectValue:F0} charges";
            
            case EmbossingEffectType.Persistence:
                return "Card stays in hand after being played";
            
            // Status effects
            case EmbossingEffectType.ApplyBleed:
                return $"{(statusEffectChance * 100):F0}% chance to apply Bleed";
            
            case EmbossingEffectType.ApplyIgnite:
                return $"{(statusEffectChance * 100):F0}% chance to apply Ignite";
            
            case EmbossingEffectType.ApplyPoison:
                return $"{(statusEffectChance * 100):F0}% chance to apply Poison";
            
            case EmbossingEffectType.ApplyShock:
                return $"{(statusEffectChance * 100):F0}% chance to apply Shock";
            
            case EmbossingEffectType.ApplyFreeze:
                return $"{(statusEffectChance * 100):F0}% chance to apply Freeze";
            
            case EmbossingEffectType.ApplyChill:
                return $"{(statusEffectChance * 100):F0}% chance to apply Chill";
            
            // Defensive types
            case EmbossingEffectType.GuardOnPlay:
                return $"Gain {effectValue:F0} Guard when played";
            
            case EmbossingEffectType.DamageReflection:
                return $"Reflect {(effectValue * 100):F0}% of damage taken";
            
            case EmbossingEffectType.GuardEffectiveness:
                return $"+{(effectValue * 100):F0}% Guard effectiveness";
            
            // Special
            case EmbossingEffectType.ConditionalDamage:
                return $"+{(effectValue * 100):F0}% damage under specific conditions";
            
            case EmbossingEffectType.ComboScaling:
                return $"+{(effectValue * 100):F0}% damage per combo point";
            
            case EmbossingEffectType.CustomEffect:
                if (!string.IsNullOrEmpty(customEffectId))
                {
                    return $"Custom effect: {customEffectId}";
                }
                else if (!string.IsNullOrEmpty(description))
                {
                    return description;
                }
                else
                {
                    return "Custom effect";
                }
            
            default:
                return !string.IsNullOrEmpty(description) ? description : "Unknown effect";
        }
    }
    
    /// <summary>
    /// Calculate the new mana cost for a card after applying this embossing
    /// </summary>
    public int CalculateNewManaCost(Card card)
    {
        if (card == null) return 0;
        
        // Get base mana cost
        int baseCost = card.manaCost;
        
        // Calculate total multiplier from all embossings (including this one)
        float totalMultiplier = manaCostMultiplier;
        if (card.appliedEmbossings != null)
        {
            foreach (var instance in card.appliedEmbossings)
            {
                // Get the embossing effect from database
                EmbossingEffect existingEffect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (existingEffect != null)
                {
                    totalMultiplier += existingEffect.manaCostMultiplier;
                }
            }
        }
        
        // Calculate new cost
        int embossingCount = card.appliedEmbossings != null ? card.appliedEmbossings.Count : 0;
        float multipliedCost = (baseCost + embossingCount + 1) * (1 + totalMultiplier);
        
        // Add flat increase
        int totalFlatIncrease = flatManaCostIncrease;
        if (card.appliedEmbossings != null)
        {
            foreach (var instance in card.appliedEmbossings)
            {
                EmbossingEffect existingEffect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (existingEffect != null)
                {
                    totalFlatIncrease += existingEffect.flatManaCostIncrease;
                }
            }
        }
        
        return Mathf.CeilToInt(multipliedCost) + totalFlatIncrease;
    }
}

/// <summary>
/// Category of embossing effect
/// </summary>
public enum EmbossingCategory
{
    Damage,          // Pure damage increases
    Scaling,         // Stat scaling effects
    Utility,         // Special mechanics
    Defensive,       // Guard/protection effects
    Combo,           // Discard/combo synergies
    Ailment,         // Status effect focused
    Chaos,           // High risk/reward
    Conversion       // Element conversion
}

/// <summary>
/// Rarity tier of embossing
/// </summary>
public enum EmbossingRarity
{
    Common,      // Low requirements, low power
    Uncommon,    // Moderate requirements
    Rare,        // Higher requirements
    Epic,        // Significant requirements
    Legendary    // Extreme requirements, very powerful
}

/// <summary>
/// Type of effect the embossing provides
/// </summary>
public enum EmbossingEffectType
{
    // Damage types
    DamageMultiplier,           // +X% more damage
    PhysicalDamageMultiplier,   // +X% more physical damage
    ElementalDamageMultiplier,  // +X% more elemental damage
    SpellDamageMultiplier,      // +X% more spell damage
    FlatDamageBonus,            // +X flat damage
    
    // Conversion types
    PhysicalToFireConversion,       // Convert X% physical to fire
    PhysicalToColdConversion,       // Convert X% physical to cold
    PhysicalToLightningConversion,  // Convert X% physical to lightning
    ElementalToChaosConversion,     // Convert X% elemental to chaos
    
    // Scaling types
    StrengthScaling,      // +X% damage per strength
    DexterityScaling,     // +X% damage per dexterity
    IntelligenceScaling,  // +X% damage per intelligence
    EmbossingCountScaling, // +X% per embossing on card
    
    // Utility types
    ManaCostReduction,    // -X% mana cost
    CriticalChance,       // +X% crit chance
    CriticalMultiplier,   // +X% crit damage
    LifeOnHit,           // Gain X life on hit
    LifeLeech,           // Leech X% of damage
    CardDuplication,     // Create copy of card
    PrepareCharges,      // Card prepares with charges
    Persistence,         // Card stays in hand
    
    // Status effects
    ApplyBleed,          // Chance to bleed
    ApplyIgnite,         // Chance to ignite
    ApplyPoison,         // Chance to poison
    ApplyShock,          // Chance to shock
    ApplyFreeze,         // Chance to freeze
    ApplyChill,          // Chance to chill
    
    // Defensive types
    GuardOnPlay,         // Gain guard when played
    DamageReflection,    // Reflect X% damage
    GuardEffectiveness,  // +X% guard effectiveness
    
    // Special/Custom
    ConditionalDamage,   // Damage based on conditions
    ComboScaling,        // Scales with combos
    CustomEffect         // Uses customEffectId for unique logic
}

// Note: StatusEffectType is defined in Assets/Scripts/CombatSystem/StatusEffect.cs
// Note: ElementType/DamageType is defined in Assets/Scripts/Cards/Card.cs

