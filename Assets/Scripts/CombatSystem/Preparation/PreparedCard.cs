using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a card that has been prepared and is waiting to be unleashed.
/// Tracks turn count, accumulated bonuses, and stored effects.
/// </summary>
[System.Serializable]
public class PreparedCard
{
    // Core card reference
    public CardDataExtended sourceCard;
    public Character owner;
    
    // Timing tracking
    public int turnsPrepared = 0;
    public int maxTurns = 3;
    public int chargesThisTurn = 0; // For DEX-based multi-charging
    
    // Stored values
    public float storedBaseDamage = 0f;
    public float storedSpellPower = 0f;
    public Dictionary<string, float> storedStats = new Dictionary<string, float>();
    
    // Accumulation settings (from card definition)
    public float multiplierPerTurn = 0.5f; // 50% per turn by default
    public float currentMultiplier = 0f;
    
    // Flat bonuses from stats
    public float flatStrengthBonus = 0f;
    
    // State tracking
    public bool isOvercharged = false; // Charged beyond max turns via DEX
    public float decayAmount = 0f; // Tracks decay if past max turns without overcharge
    
    // Unleash conditions
    public enum UnleashCondition
    {
        Manual,         // Click to unleash
        AutoOnMax,      // Automatically unleashes when max turns reached
        Triggered,      // Requires specific trigger card
        Decay          // Auto-unleashes after max, but with penalty
    }
    public UnleashCondition unleashCondition = UnleashCondition.Manual;
    
    // Visual state
    public GameObject cardVisualObject; // Reference to UI card object
    
    /// <summary>
    /// Constructor for creating a prepared card
    /// </summary>
    public PreparedCard(CardDataExtended card, Character player)
    {
        sourceCard = card;
        owner = player;
        
        // Initialize from card data
        maxTurns = card.maxPrepTurns > 0 ? card.maxPrepTurns : 3;
        multiplierPerTurn = card.multiplierPerTurn > 0 ? card.multiplierPerTurn : 0.5f;
        
        // Store initial values
        storedBaseDamage = card.damage;
        storedSpellPower = player.intelligence * 0.1f; // Spell power from INT
        
        // Store current stats for scaling calculations
        storedStats["strength"] = player.strength;
        storedStats["dexterity"] = player.dexterity;
        storedStats["intelligence"] = player.intelligence;
        
        // Determine unleash condition
        unleashCondition = ParseUnleashCondition(card.unleashCondition);
        
        Debug.Log($"<color=cyan>[Preparation] Created PreparedCard: {card.cardName} | Max Turns: {maxTurns} | Multiplier/Turn: {multiplierPerTurn}</color>");
    }
    
    /// <summary>
    /// Increment turn counter and apply bonuses based on stats
    /// </summary>
    public void OnTurnEnd()
    {
        // Calculate charge rate based on Dexterity
        int chargeRate = CalculateChargeRate();
        chargesThisTurn = chargeRate;
        
        // Apply charges
        for (int i = 0; i < chargeRate; i++)
        {
            if (turnsPrepared < maxTurns)
            {
                // Normal charging
                turnsPrepared++;
                currentMultiplier += multiplierPerTurn;
                
                Debug.Log($"<color=green>[Preparation] {sourceCard.cardName} charged: {turnsPrepared}/{maxTurns} (Multiplier: {currentMultiplier:F2}x)</color>");
            }
            else
            {
                // Overcharging (DEX-based)
                if (CanOvercharge())
                {
                    isOvercharged = true;
                    turnsPrepared++;
                    currentMultiplier += multiplierPerTurn * 0.5f; // Half bonus for overcharge
                    
                    Debug.Log($"<color=yellow>[Preparation] {sourceCard.cardName} OVERCHARGED: {turnsPrepared}/{maxTurns} (Multiplier: {currentMultiplier:F2}x)</color>");
                }
                else
                {
                    // Decay starts
                    decayAmount += 0.1f; // 10% decay per turn over max
                    Debug.Log($"<color=red>[Preparation] {sourceCard.cardName} decaying: {decayAmount:F2} penalty</color>");
                }
            }
        }
        
        // Update stat-based flat bonuses
        UpdateStatBonuses();
    }
    
    /// <summary>
    /// Calculate charge rate based on Dexterity
    /// Base: 1/turn, High DEX: 2-3/turn
    /// </summary>
    private int CalculateChargeRate()
    {
        float dex = owner.dexterity;
        
        if (dex >= 50) return 3;      // 3 charges/turn at 50+ DEX
        if (dex >= 30) return 2;      // 2 charges/turn at 30+ DEX
        return 1;                     // 1 charge/turn base
    }
    
    /// <summary>
    /// Check if this card can overcharge based on Dexterity
    /// </summary>
    private bool CanOvercharge()
    {
        // Requires high DEX to prevent decay
        return owner.dexterity >= 30;
    }
    
    /// <summary>
    /// Update flat bonuses from character stats
    /// </summary>
    private void UpdateStatBonuses()
    {
        // STR adds flat damage per turn
        flatStrengthBonus = owner.strength * 0.5f * turnsPrepared;
        
        // INT increases max turns
        int intBonus = Mathf.FloorToInt(owner.intelligence / 20f);
        maxTurns = sourceCard.maxPrepTurns + intBonus;
        
        Debug.Log($"<color=cyan>[Preparation] {sourceCard.cardName} stat bonuses: STR +{flatStrengthBonus:F1} dmg, Max turns: {maxTurns}</color>");
    }
    
    /// <summary>
    /// Calculate final damage/effect when unleashed
    /// </summary>
    public float CalculateUnleashDamage()
    {
        // Base damage * (1 + currentMultiplier) + flat bonuses - decay penalty
        float baseDamage = storedBaseDamage;
        float multipliedDamage = baseDamage * (1f + currentMultiplier);
        float totalDamage = multipliedDamage + flatStrengthBonus;
        
        // Apply decay penalty if any
        if (decayAmount > 0f)
        {
            totalDamage *= (1f - decayAmount);
        }
        
        Debug.Log($"<color=yellow>[Preparation] {sourceCard.cardName} Unleash Damage: Base={baseDamage}, Multiplier={currentMultiplier:F2}, STR Bonus={flatStrengthBonus:F1}, Decay={decayAmount:F2}, Total={totalDamage:F1}</color>");
        
        return totalDamage;
    }
    
    /// <summary>
    /// Calculate energy cost to manually unleash this card
    /// Base energy + 1 per turn prepared
    /// </summary>
    public int CalculateManualUnleashCost()
    {
        return sourceCard.playCost + turnsPrepared;
    }
    
    /// <summary>
    /// Check if this card should auto-unleash this turn
    /// </summary>
    public bool ShouldAutoUnleash()
    {
        switch (unleashCondition)
        {
            case UnleashCondition.AutoOnMax:
                return turnsPrepared >= maxTurns && !isOvercharged;
                
            case UnleashCondition.Decay:
                return decayAmount >= 1f; // Fully decayed, forced unleash
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Parse unleash condition from string
    /// </summary>
    private UnleashCondition ParseUnleashCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition)) return UnleashCondition.Manual;
        
        switch (condition.ToLower())
        {
            case "manual": return UnleashCondition.Manual;
            case "auto_on_max": return UnleashCondition.AutoOnMax;
            case "triggered": return UnleashCondition.Triggered;
            case "decay": return UnleashCondition.Decay;
            default: return UnleashCondition.Manual;
        }
    }
}

