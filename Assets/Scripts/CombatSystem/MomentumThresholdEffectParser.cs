using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Parses momentum threshold effects from card momentumEffectDescription.
/// Handles patterns like "3+ Momentum:", "5+ Momentum:", "8+ Momentum:", etc.
/// </summary>
public static class MomentumThresholdEffectParser
{
    /// <summary>
    /// Represents a single momentum threshold effect
    /// </summary>
    public class MomentumThresholdEffect
    {
        public int threshold; // e.g., 3, 5, 8, 10
        public bool isExact; // true if "10 Momentum" (exact), false if "10+ Momentum" (at least)
        public string effectText; // The effect description after the threshold
        
        public MomentumThresholdEffect(int threshold, bool isExact, string effectText)
        {
            this.threshold = threshold;
            this.isExact = isExact;
            this.effectText = effectText;
        }
    }
    
    /// <summary>
    /// Parse all momentum threshold effects from a description
    /// </summary>
    public static List<MomentumThresholdEffect> ParseThresholdEffects(string description)
    {
        var effects = new List<MomentumThresholdEffect>();
        if (string.IsNullOrEmpty(description)) return effects;
        
        // Split by newlines to handle multiple effects
        string[] lines = description.Split('\n');
        
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            
            // Pattern 1: "X+ Momentum:" or "X Momentum:" (exact)
            // Pattern 2: "If you have X+ Momentum:" or "If you have X Momentum:"
            // Pattern 3: "If you spend X+ Momentum:" or "If you spend X Momentum:"
            var match = Regex.Match(
                trimmed,
                @"(?:If\s+you\s+(?:have|spend)\s+)?(\d+)(\+)?\s+Momentum:\s*(.+)",
                RegexOptions.IgnoreCase
            );
            
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int threshold))
                {
                    bool isExact = string.IsNullOrEmpty(match.Groups[2].Value); // No "+" means exact
                    string effectText = match.Groups[3].Value.Trim();
                    
                    effects.Add(new MomentumThresholdEffect(threshold, isExact, effectText));
                }
            }
            else
            {
                // Alternative pattern: "Gain +1 Guard per Momentum." (no threshold, just "Momentum")
                // Also handle patterns like "Gain +1 Guard per Momentum" at the start of description
                if (trimmed.Contains("Momentum", System.StringComparison.OrdinalIgnoreCase) && 
                    (trimmed.Contains("Guard per Momentum") || trimmed.Contains("gain") && trimmed.Contains("Guard")))
                {
                    // This is a base effect that applies when you have any momentum
                    effects.Add(new MomentumThresholdEffect(1, false, trimmed));
                }
            }
        }
        
        // Sort by threshold (ascending) so we can check highest applicable threshold
        effects.Sort((a, b) => a.threshold.CompareTo(b.threshold));
        
        return effects;
    }
    
    /// <summary>
    /// Get the highest applicable threshold effect for a given momentum amount
    /// </summary>
    public static MomentumThresholdEffect GetHighestApplicableEffect(List<MomentumThresholdEffect> effects, int currentMomentum)
    {
        MomentumThresholdEffect bestEffect = null;
        
        foreach (var effect in effects)
        {
            bool applies = false;
            
            if (effect.isExact)
            {
                applies = (currentMomentum == effect.threshold);
            }
            else
            {
                applies = (currentMomentum >= effect.threshold);
            }
            
            if (applies)
            {
                // Keep the highest threshold that applies
                if (bestEffect == null || effect.threshold >= bestEffect.threshold)
                {
                    bestEffect = effect;
                }
            }
        }
        
        return bestEffect;
    }
    
    /// <summary>
    /// Get all applicable threshold effects for a given momentum amount
    /// </summary>
    public static List<MomentumThresholdEffect> GetAllApplicableEffects(List<MomentumThresholdEffect> effects, int currentMomentum)
    {
        var applicable = new List<MomentumThresholdEffect>();
        
        foreach (var effect in effects)
        {
            bool applies = false;
            
            if (effect.isExact)
            {
                applies = (currentMomentum == effect.threshold);
            }
            else
            {
                applies = (currentMomentum >= effect.threshold);
            }
            
            if (applies)
            {
                applicable.Add(effect);
            }
        }
        
        return applicable;
    }
    
    /// <summary>
    /// Parse effect type from effect text
    /// </summary>
    public static MomentumEffectType ParseEffectType(string effectText)
    {
        if (string.IsNullOrEmpty(effectText)) return MomentumEffectType.None;
        
        string lower = effectText.ToLower();
        
        // Cost reduction
        if (lower.Contains("costs") && (lower.Contains("less") || lower.Contains("0")))
            return MomentumEffectType.CostReduction;
        
        // AoE conversion
        if (lower.Contains("all enemies") || lower.Contains("deal damage to all"))
            return MomentumEffectType.ConvertToAoE;
        
        // Random targets
        if (lower.Contains("random enemies") || lower.Contains("2 random"))
            return MomentumEffectType.RandomTargets;
        
        // Additional momentum gain
        if (lower.Contains("gain") && lower.Contains("momentum"))
            return MomentumEffectType.AdditionalMomentum;
        
        // Draw cards
        if (lower.Contains("draw"))
            return MomentumEffectType.DrawCards;
        
        // Temporary stat boost
        if (lower.Contains("temporary") && (lower.Contains("strength") || lower.Contains("dexterity") || lower.Contains("intelligence")))
            return MomentumEffectType.TemporaryStatBoost;
        
        // Energy gain
        if (lower.Contains("energy") || lower.Contains("gain +1 energy"))
            return MomentumEffectType.EnergyGain;
        
        // Apply ailment
        if (lower.Contains("apply") && (lower.Contains("bleed") || lower.Contains("poison") || lower.Contains("burn")))
            return MomentumEffectType.ApplyAilment;
        
        // Double damage
        if (lower.Contains("double") && lower.Contains("damage"))
            return MomentumEffectType.DoubleDamage;
        
        // Additional guard per momentum (e.g., "Gain +1 Guard per Momentum" or "Gain +1 guard per Momentum")
        if (lower.Contains("guard per momentum") || (lower.Contains("gain") && lower.Contains("guard") && lower.Contains("per momentum")))
            return MomentumEffectType.GuardPerMomentum;
        
        // Additional guard flat
        if (lower.Contains("additional guard") || lower.Contains("gain") && lower.Contains("guard"))
            return MomentumEffectType.AdditionalGuard;
        
        // Special effects (Adrenaline Burst, etc.)
        if (lower.Contains("trigger") || lower.Contains("burst"))
            return MomentumEffectType.SpecialEffect;
        
        return MomentumEffectType.Unknown;
    }
    
    /// <summary>
    /// Parse numeric value from effect text (e.g., "Draw 2 cards" -> 2)
    /// </summary>
    public static int ParseNumericValue(string effectText)
    {
        if (string.IsNullOrEmpty(effectText)) return 0;
        
        var match = Regex.Match(effectText, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int value))
            return value;
        
        return 0;
    }
}

/// <summary>
/// Types of momentum threshold effects
/// </summary>
public enum MomentumEffectType
{
    None,
    CostReduction,
    ConvertToAoE,
    RandomTargets,
    AdditionalMomentum,
    DrawCards,
    TemporaryStatBoost,
    EnergyGain,
    ApplyAilment,
    DoubleDamage,
    GuardPerMomentum,
    AdditionalGuard,
    SpecialEffect,
    Unknown
}

