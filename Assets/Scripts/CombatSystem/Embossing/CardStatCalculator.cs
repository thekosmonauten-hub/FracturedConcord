using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Calculates card stats (damage, etc.) with embossings applied for preview/display purposes
/// Works outside of combat to show real-time stat updates when embossings are applied
/// </summary>
public static class CardStatCalculator
{
    /// <summary>
    /// Calculated damage breakdown for a card with embossings
    /// </summary>
    public class DamageBreakdown
    {
        public float baseDamage;
        public float attributeScaling;
        public float weaponDamage;
        public float embossingFlatBonus;
        public float embossingMultiplier;
        public Dictionary<DamageType, float> embossingAddedDamage = new Dictionary<DamageType, float>(); // Extra damage from conversions
        public float totalDamage;
        public string breakdownText;
        
        // Per-damage-type breakdowns (for multi-element cards)
        public Dictionary<DamageType, float> damageByType = new Dictionary<DamageType, float>();
    }
    
    /// <summary>
    /// Calculate total damage for a card with embossings applied (for preview/display)
    /// This is a simplified version that works outside combat
    /// </summary>
    public static DamageBreakdown CalculateCardDamage(Card card, Character character)
    {
        DamageBreakdown breakdown = new DamageBreakdown();
        
        if (card == null || character == null)
        {
            breakdown.totalDamage = 0f;
            breakdown.breakdownText = "No card or character";
            return breakdown;
        }
        
        // 1. Base damage
        breakdown.baseDamage = card.baseDamage;
        
        // 2. Attribute scaling
        if (card.damageScaling != null)
        {
            breakdown.attributeScaling = card.damageScaling.CalculateScalingBonus(character);
        }
        
        // 3. Weapon scaling
        if (card.scalesWithMeleeWeapon)
        {
            breakdown.weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Melee);
        }
        if (card.scalesWithProjectileWeapon)
        {
            breakdown.weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Projectile);
        }
        if (card.scalesWithSpellWeapon)
        {
            breakdown.weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Spell);
        }
        
        // 4. Base damage with scaling
        float baseWithScaling = breakdown.baseDamage + breakdown.attributeScaling + breakdown.weaponDamage;
        
        // 5. Apply embossing stat scaling (flat bonuses)
        if (card.appliedEmbossings != null && card.appliedEmbossings.Count > 0)
        {
            baseWithScaling = Dexiled.CombatSystem.Embossing.EmbossingEffectProcessor.ApplyScalingBonuses(
                card, 
                character, 
                baseWithScaling
            );
        }
        
        // 6. Apply embossing damage multipliers and conversions
        float embossingMultiplier = 0f;
        float embossingFlat = 0f;
        
        if (card.appliedEmbossings != null && card.appliedEmbossings.Count > 0)
        {
            // First, process modifiers from the new modifier system
            var modifierRegistry = EmbossingModifierRegistry.Instance;
            if (modifierRegistry != null)
            {
                // Get all active effects (already scaled by level)
                var activeEffects = modifierRegistry.GetActiveEffects(card, character);
                
                if (activeEffects != null && activeEffects.Count > 0)
                {
                    Debug.Log($"[CardStatCalculator] Processing {activeEffects.Count} active modifier effects for {card.cardName}");
                }
                
                foreach (var effect in activeEffects)
                {
                    // Only process OnCardPlayed effects for immediate stat calculation
                    if (effect.eventType != ModifierEventType.OnCardPlayed) continue;
                    
                    if (effect.actions != null)
                    {
                        foreach (var action in effect.actions)
                        {
                            // Handle AddFlatDamage action
                            if (action.actionType == ModifierActionType.AddFlatDamage)
                            {
                                float damageValue = action.parameterDict.GetParameter<float>("value");
                                DamageType damageType = action.parameterDict.ContainsKey("damageType") 
                                    ? action.parameterDict.GetParameter<DamageType>("damageType") 
                                    : DamageType.Physical;
                                
                                if (damageValue > 0)
                                {
                                    if (breakdown.embossingAddedDamage.ContainsKey(damageType))
                                        breakdown.embossingAddedDamage[damageType] += damageValue;
                                    else
                                        breakdown.embossingAddedDamage[damageType] = damageValue;
                                    
                                    Debug.Log($"[CardStatCalculator] Added {damageValue} {damageType} damage from AddFlatDamage modifier");
                                }
                            }
                            // Handle ModifyStat action (for chaos damage, etc.)
                            else if (action.actionType == ModifierActionType.ModifyStat)
                            {
                                // Try to parse damage from description or parameters
                                string customEffect = action.parameterDict.ContainsKey("customEffect") 
                                    ? action.parameterDict.GetParameter<string>("customEffect") 
                                    : "";
                                    
                                if (!string.IsNullOrEmpty(customEffect))
                                {
                                    Debug.Log($"[CardStatCalculator] Processing ModifyStat action with customEffect: {customEffect}");
                                    
                                    // Parse "Adds X Chaos damage" or similar patterns
                                    float damageValue = ParseDamageFromDescription(customEffect, out DamageType parsedDamageType);
                                    if (damageValue > 0)
                                    {
                                        if (breakdown.embossingAddedDamage.ContainsKey(parsedDamageType))
                                            breakdown.embossingAddedDamage[parsedDamageType] += damageValue;
                                        else
                                            breakdown.embossingAddedDamage[parsedDamageType] = damageValue;
                                        
                                        Debug.Log($"[CardStatCalculator] Added {damageValue} {parsedDamageType} damage from ModifyStat modifier (parsed from: {customEffect})");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"[CardStatCalculator] Failed to parse damage value from: {customEffect}");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning($"[CardStatCalculator] ModifyStat action has no customEffect parameter");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CardStatCalculator] EmbossingModifierRegistry.Instance is null!");
            }
            
            // Also process old embossing effect types for backwards compatibility
            foreach (var instance in card.appliedEmbossings)
            {
                if (instance == null) continue;
                
                EmbossingEffect effect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (effect == null) continue;
                
                float levelBonus = instance.GetLevelBonusMultiplier();
                float embossingValue = effect.effectValue * levelBonus;
                
                // Damage multipliers
                switch (effect.effectType)
                {
                    case EmbossingEffectType.DamageMultiplier:
                        embossingMultiplier += embossingValue;
                        break;
                    
                    case EmbossingEffectType.PhysicalDamageMultiplier:
                        if (card.primaryDamageType == DamageType.Physical)
                            embossingMultiplier += embossingValue;
                        break;
                    
                    case EmbossingEffectType.ElementalDamageMultiplier:
                        if (card.primaryDamageType == DamageType.Fire || 
                            card.primaryDamageType == DamageType.Cold || 
                            card.primaryDamageType == DamageType.Lightning)
                            embossingMultiplier += embossingValue;
                        break;
                    
                    case EmbossingEffectType.SpellDamageMultiplier:
                        if (card.tags != null && (card.tags.Contains("Spell") || card.tags.Contains("spell")))
                            embossingMultiplier += embossingValue;
                        break;
                    
                    case EmbossingEffectType.FlatDamageBonus:
                        embossingFlat += embossingValue;
                        break;
                    
                    // Conversions (reduce source, add as new type) - Note: These reduce the original damage
                    case EmbossingEffectType.PhysicalToFireConversion:
                        if (card.primaryDamageType == DamageType.Physical)
                        {
                            // Conversion reduces physical and adds fire
                            float converted = baseWithScaling * embossingValue;
                            breakdown.embossingAddedDamage[DamageType.Fire] = 
                                (breakdown.embossingAddedDamage.ContainsKey(DamageType.Fire) ? breakdown.embossingAddedDamage[DamageType.Fire] : 0f) + converted;
                            // Note: Physical damage should be reduced, but for preview we'll show both
                        }
                        break;
                    
                    case EmbossingEffectType.PhysicalToColdConversion:
                        if (card.primaryDamageType == DamageType.Physical)
                        {
                            float converted = baseWithScaling * embossingValue;
                            breakdown.embossingAddedDamage[DamageType.Cold] = 
                                (breakdown.embossingAddedDamage.ContainsKey(DamageType.Cold) ? breakdown.embossingAddedDamage[DamageType.Cold] : 0f) + converted;
                        }
                        break;
                    
                    case EmbossingEffectType.PhysicalToLightningConversion:
                        if (card.primaryDamageType == DamageType.Physical)
                        {
                            float converted = baseWithScaling * embossingValue;
                            breakdown.embossingAddedDamage[DamageType.Lightning] = 
                                (breakdown.embossingAddedDamage.ContainsKey(DamageType.Lightning) ? breakdown.embossingAddedDamage[DamageType.Lightning] : 0f) + converted;
                        }
                        break;
                    
                    // Note: "Extra" damage effects (like "30% of Physical as Extra Fire") 
                    // For now, check description for "Extra" keywords or use elementType
                    default:
                        // Check if this is an "Extra" damage effect
                        // Pattern: "X% of [SourceType] as Extra [TargetType] damage"
                        if (effect.description != null && 
                            (effect.description.ToLower().Contains("extra") || effect.description.ToLower().Contains("as extra")))
                        {
                            // If elementType is set and different from card's primary type, treat as extra damage
                            if (effect.elementType != DamageType.None && 
                                effect.elementType != card.primaryDamageType &&
                                embossingValue > 0)
                            {
                                // Calculate extra damage based on current physical damage
                                float extraDamage = baseWithScaling * embossingValue;
                                breakdown.embossingAddedDamage[effect.elementType] = 
                                    (breakdown.embossingAddedDamage.ContainsKey(effect.elementType) ? 
                                     breakdown.embossingAddedDamage[effect.elementType] : 0f) + extraDamage;
                            }
                        }
                        break;
                }
            }
        }
        
        breakdown.embossingFlatBonus = embossingFlat;
        breakdown.embossingMultiplier = embossingMultiplier;
        
        // 7. Calculate final damage
        float damageAfterMultiplier = (baseWithScaling + embossingFlat) * (1f + embossingMultiplier);
        
        // Add converted damage
        breakdown.damageByType[card.primaryDamageType] = damageAfterMultiplier;
        foreach (var kvp in breakdown.embossingAddedDamage)
        {
            if (breakdown.damageByType.ContainsKey(kvp.Key))
                breakdown.damageByType[kvp.Key] += kvp.Value;
            else
                breakdown.damageByType[kvp.Key] = kvp.Value;
        }
        
        // Total is sum of all damage types
        breakdown.totalDamage = breakdown.damageByType.Values.Sum();
        
        // 8. Generate breakdown text
        breakdown.breakdownText = GenerateBreakdownText(breakdown, card);
        
        return breakdown;
    }
    
    /// <summary>
    /// Generate human-readable breakdown text for tooltip
    /// </summary>
    private static string GenerateBreakdownText(DamageBreakdown breakdown, Card card)
    {
        List<string> parts = new List<string>();
        
        // Calculate base damage (before embossings)
        float baseBeforeEmbossings = breakdown.baseDamage + breakdown.attributeScaling + breakdown.weaponDamage;
        
        // Base damage line
        if (baseBeforeEmbossings > 0)
        {
            string baseLine = $"{baseBeforeEmbossings:F0} {card.primaryDamageType}";
            if (breakdown.baseDamage > 0 && (breakdown.attributeScaling > 0 || breakdown.weaponDamage > 0))
            {
                baseLine += $" ({breakdown.baseDamage:F0} base";
                if (breakdown.attributeScaling > 0)
                    baseLine += $" + {breakdown.attributeScaling:F0} attributes";
                if (breakdown.weaponDamage > 0)
                    baseLine += $" + {breakdown.weaponDamage:F0} weapon";
                baseLine += ")";
            }
            parts.Add(baseLine);
        }
        
        // Embossing flat bonus
        if (breakdown.embossingFlatBonus > 0)
            parts.Add($"+{breakdown.embossingFlatBonus:F0} flat from embossings");
        
        // Embossing multiplier
        if (breakdown.embossingMultiplier > 0)
            parts.Add($"+{breakdown.embossingMultiplier * 100:F0}% more from embossings");
        
        // Extra damage from conversions/extra effects
        foreach (var kvp in breakdown.embossingAddedDamage)
        {
            if (kvp.Value > 0)
            {
                string typeName = kvp.Key.ToString();
                parts.Add($"+{kvp.Value:F0} {typeName} (from embossings)");
            }
        }
        
        if (parts.Count == 0)
            return $"{breakdown.totalDamage:F0} damage";
        
        // Format as multi-line breakdown
        return string.Join("\n", parts) + $"\n= {breakdown.totalDamage:F0} total damage";
    }
    
    /// <summary>
    /// Get formatted damage text for tooltip (shows breakdown on hover)
    /// </summary>
    public static string GetDamageTextWithBreakdown(Card card, Character character, bool showBreakdown = false)
    {
        DamageBreakdown breakdown = CalculateCardDamage(card, character);
        
        if (showBreakdown && breakdown.embossingFlatBonus > 0 || breakdown.embossingMultiplier > 0 || breakdown.embossingAddedDamage.Count > 0)
        {
            return breakdown.breakdownText;
        }
        
        return breakdown.totalDamage.ToString("F0");
    }
    
    /// <summary>
    /// Parse damage value and type from description string (e.g., "Adds 26 Chaos damage")
    /// </summary>
    private static float ParseDamageFromDescription(string description, out DamageType damageType)
    {
        damageType = DamageType.Physical;
        if (string.IsNullOrEmpty(description)) return 0f;
        
        description = description.ToLower();
        
        // Try to extract numeric value
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(\d+(?:\.\d+)?)");
        var match = regex.Match(description);
        if (!match.Success) return 0f;
        
        if (!float.TryParse(match.Groups[1].Value, out float damageValue))
            return 0f;
        
        // Determine damage type from description
        if (description.Contains("chaos"))
            damageType = DamageType.Chaos;
        else if (description.Contains("fire"))
            damageType = DamageType.Fire;
        else if (description.Contains("cold"))
            damageType = DamageType.Cold;
        else if (description.Contains("lightning"))
            damageType = DamageType.Lightning;
        else if (description.Contains("physical"))
            damageType = DamageType.Physical;
        // Default to Physical if not specified
        
        return damageValue;
    }
}

