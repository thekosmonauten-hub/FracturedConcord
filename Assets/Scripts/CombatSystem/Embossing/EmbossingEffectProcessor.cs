using UnityEngine;
using System.Collections.Generic;

namespace Dexiled.CombatSystem.Embossing
{
    /// <summary>
    /// Core processor for embossing effects in combat
    /// Handles damage multipliers, scaling, conversions, and special effects
    /// </summary>
    public static class EmbossingEffectProcessor
    {
        // Debug logging
        private static bool debugMode = true;
        
        /// <summary>
        /// Apply all damage multiplier embossings to base damage
        /// Phase 1: Damage multipliers only
        /// </summary>
        public static float ApplyDamageMultipliers(Card card, float baseDamage, DamageType damageType, Character character = null)
        {
            if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
            {
                return baseDamage;
            }
            
            float additiveMultiplier = 0f; // All multipliers add together, then multiply once
            float flatBonus = 0f;
            
            if (debugMode)
            {
                Debug.Log($"[EmbossingEffectProcessor] Processing {card.appliedEmbossings.Count} embossings for {card.cardName}");
                Debug.Log($"[EmbossingEffectProcessor] Base damage: {baseDamage}, Damage type: {damageType}");
            }
            
            foreach (var instance in card.appliedEmbossings)
            {
                if (instance == null) continue;
                
                // Get embossing effect data
                EmbossingEffect effect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (effect == null)
                {
                    Debug.LogWarning($"[EmbossingEffectProcessor] Embossing not found in database: {instance.embossingId}");
                    continue;
                }
                
                // Get level bonus multiplier (1.0 at level 1, up to 1.2 at level 20)
                float levelBonus = instance.GetLevelBonusMultiplier();
                
                // Process effect based on type
                float embossingValue = effect.effectValue * levelBonus;
                
                switch (effect.effectType)
                {
                    case EmbossingEffectType.DamageMultiplier:
                        // Generic damage increase (affects all damage)
                        additiveMultiplier += embossingValue;
                        if (debugMode)
                        {
                            Debug.Log($"  [{effect.embossingName}] +{(embossingValue * 100):F1}% damage (generic)");
                        }
                        break;
                    
                    case EmbossingEffectType.PhysicalDamageMultiplier:
                        // Only applies to physical damage
                        if (damageType == DamageType.Physical)
                        {
                            additiveMultiplier += embossingValue;
                            if (debugMode)
                            {
                                Debug.Log($"  [{effect.embossingName}] +{(embossingValue * 100):F1}% physical damage");
                            }
                        }
                        break;
                    
                    case EmbossingEffectType.ElementalDamageMultiplier:
                        // Applies to fire, cold, lightning
                        if (damageType == DamageType.Fire || damageType == DamageType.Cold || damageType == DamageType.Lightning)
                        {
                            additiveMultiplier += embossingValue;
                            if (debugMode)
                            {
                                Debug.Log($"  [{effect.embossingName}] +{(embossingValue * 100):F1}% elemental damage");
                            }
                        }
                        break;
                    
                    case EmbossingEffectType.SpellDamageMultiplier:
                        // Applies to spell cards (check tags)
                        if (card.tags != null && (card.tags.Contains("Spell") || card.tags.Contains("spell")))
                        {
                            additiveMultiplier += embossingValue;
                            if (debugMode)
                            {
                                Debug.Log($"  [{effect.embossingName}] +{(embossingValue * 100):F1}% spell damage");
                            }
                        }
                        break;
                    
                    case EmbossingEffectType.FlatDamageBonus:
                        // Flat damage added
                        flatBonus += embossingValue * levelBonus;
                        if (debugMode)
                        {
                            Debug.Log($"  [{effect.embossingName}] +{(embossingValue * levelBonus):F1} flat damage");
                        }
                        break;
                }
            }
            
            // Apply multipliers (additive stacking)
            float finalDamage = (baseDamage + flatBonus) * (1f + additiveMultiplier);
            
            if (debugMode)
            {
                Debug.Log($"[EmbossingEffectProcessor] Total multiplier: +{(additiveMultiplier * 100):F1}%");
                Debug.Log($"[EmbossingEffectProcessor] Flat bonus: +{flatBonus}");
                Debug.Log($"[EmbossingEffectProcessor] Final damage: {baseDamage} → {finalDamage}");
            }
            
            return finalDamage;
        }
        
        /// <summary>
        /// Apply stat scaling embossings (Strength/Dexterity/Intelligence)
        /// Phase 3: Will be implemented later
        /// </summary>
        public static float ApplyScalingBonuses(Card card, Character character, float baseDamage)
        {
            if (card == null || character == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
            {
                return baseDamage;
            }
            
            float totalScalingBonus = 0f;
            
            foreach (var instance in card.appliedEmbossings)
            {
                if (instance == null) continue;
                
                EmbossingEffect effect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (effect == null) continue;
                
                float levelBonus = instance.GetLevelBonusMultiplier();
                float scalingValue = effect.effectValue * levelBonus;
                
                switch (effect.effectType)
                {
                    case EmbossingEffectType.StrengthScaling:
                        totalScalingBonus += character.strength * scalingValue;
                        if (debugMode)
                        {
                            Debug.Log($"  [{effect.embossingName}] +{(character.strength * scalingValue):F1} from STR scaling");
                        }
                        break;
                    
                    case EmbossingEffectType.DexterityScaling:
                        totalScalingBonus += character.dexterity * scalingValue;
                        if (debugMode)
                        {
                            Debug.Log($"  [{effect.embossingName}] +{(character.dexterity * scalingValue):F1} from DEX scaling");
                        }
                        break;
                    
                    case EmbossingEffectType.IntelligenceScaling:
                        totalScalingBonus += character.intelligence * scalingValue;
                        if (debugMode)
                        {
                            Debug.Log($"  [{effect.embossingName}] +{(character.intelligence * scalingValue):F1} from INT scaling");
                        }
                        break;
                    
                    case EmbossingEffectType.EmbossingCountScaling:
                        int embossingCount = card.appliedEmbossings.Count;
                        totalScalingBonus += baseDamage * scalingValue * embossingCount;
                        if (debugMode)
                        {
                            Debug.Log($"  [{effect.embossingName}] +{(baseDamage * scalingValue * embossingCount):F1} from {embossingCount} embossings");
                        }
                        break;
                }
            }
            
            if (debugMode && totalScalingBonus > 0)
            {
                Debug.Log($"[EmbossingEffectProcessor] Total scaling bonus: +{totalScalingBonus}");
            }
            
            return baseDamage + totalScalingBonus;
        }
        
        /// <summary>
        /// Check if card has a specific embossing effect type
        /// </summary>
        public static bool HasEmbossingOfType(Card card, EmbossingEffectType type)
        {
            if (card == null || card.appliedEmbossings == null) return false;
            
            foreach (var instance in card.appliedEmbossings)
            {
                if (instance == null) continue;
                
                EmbossingEffect effect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (effect != null && effect.effectType == type)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Get all embossings of a specific type on a card
        /// </summary>
        public static List<EmbossingEffect> GetEmbossingsByType(Card card, EmbossingEffectType type)
        {
            List<EmbossingEffect> results = new List<EmbossingEffect>();
            
            if (card == null || card.appliedEmbossings == null) return results;
            
            foreach (var instance in card.appliedEmbossings)
            {
                if (instance == null) continue;
                
                EmbossingEffect effect = EmbossingDatabase.Instance?.GetEmbossing(instance.embossingId);
                if (effect != null && effect.effectType == type)
                {
                    results.Add(effect);
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// Enable/disable debug logging
        /// </summary>
        public static void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
        }
    }
}


