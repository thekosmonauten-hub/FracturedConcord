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
        
        // Store initial values - apply delay bonus if card is delayed
        float baseValue = card.damage;
        bool isDelayed = IsCardDelayed(card, player);
        
        if (isDelayed)
        {
            // Apply delay bonus BEFORE preparation multipliers
            // Attack cards: +25% damage
            // Guard cards: +30% guard (stored as damage for guard cards)
            CardType cardTypeEnum = card.GetCardTypeEnum();
            
            if (cardTypeEnum == CardType.Attack || (cardTypeEnum == CardType.Skill && card.damage > 0))
            {
                baseValue *= 1.25f; // +25% for delayed attacks
                Debug.Log($"<color=cyan>[Preparation+Delay] Attack card gets +25% base damage: {card.damage} → {baseValue:F1}</color>");
            }
            else if (cardTypeEnum == CardType.Guard || (cardTypeEnum == CardType.Skill && card.block > 0))
            {
                // For guard cards, we store guard as "damage" for calculation purposes
                // But the actual guard value will be calculated separately
                baseValue *= 1.30f; // +30% for delayed guard
                Debug.Log($"<color=cyan>[Preparation+Delay] Guard card gets +30% base guard: {card.damage} → {baseValue:F1}</color>");
            }
        }
        
        storedBaseDamage = baseValue;
        storedSpellPower = player.intelligence * 0.1f; // Spell power from INT
        
        // Store current stats for scaling calculations
        storedStats["strength"] = player.strength;
        storedStats["dexterity"] = player.dexterity;
        storedStats["intelligence"] = player.intelligence;
        
        // Determine unleash condition
        unleashCondition = ParseUnleashCondition(card.unleashCondition);
        
        string delayInfo = isDelayed ? " (Delayed: +25%/+30% applied to base)" : "";
        Debug.Log($"<color=cyan>[Preparation] Created PreparedCard: {card.cardName} | Max Turns: {maxTurns} | Multiplier/Turn: {multiplierPerTurn} | Base: {storedBaseDamage:F1}{delayInfo}</color>");
    }
    
    /// <summary>
    /// Check if a card is delayed (has delay turns or is marked as delayed)
    /// </summary>
    private bool IsCardDelayed(CardDataExtended card, Character player)
    {
        if (card == null) return false;
        
        // Check card's delayTurns property
        if (card.delayTurns > 0)
        {
            return true;
        }
        
        // Check if card has "Delayed" tag
        if (card.tags != null && card.tags.Contains("Delayed"))
        {
            return true;
        }
        
        // Check if card is marked as delayed (set by ascendancy or effect)
        if (card.isDelayed)
        {
            return true;
        }
        
        // TODO: Check ascendancy effects (Temporal Savant)
        // This would check if player has Temporal Savant ascendancy and apply delay
        
        return false;
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
        float baseDamage = storedBaseDamage;
        
        // Check for preparation bonus (e.g., Twin Strike: "deal 7 damage (+Dex/2)" instead of base 5 + Dex/4)
        if (PreparationBonusParser.HasPreparationBonus(sourceCard.description))
        {
            var (prepBaseDamage, prepDexDivisor) = PreparationBonusParser.ParsePreparationDamage(sourceCard.description);
            
            if (prepBaseDamage > 0f)
            {
                // Use preparation bonus damage instead of stored base damage
                baseDamage = prepBaseDamage;
                
                // Add dexterity scaling if specified
                if (prepDexDivisor > 0f && owner != null)
                {
                    float dexBonus = owner.dexterity / prepDexDivisor;
                    baseDamage += dexBonus;
                    Debug.Log($"<color=cyan>[Preparation Bonus] {sourceCard.cardName} uses preparation damage: {prepBaseDamage} + Dex/{prepDexDivisor} = {baseDamage:F1}</color>");
                }
                else
                {
                    Debug.Log($"<color=cyan>[Preparation Bonus] {sourceCard.cardName} uses preparation damage: {prepBaseDamage}</color>");
                }
            }
        }
        
        // Base damage * (1 + currentMultiplier) + flat bonuses - decay penalty
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
    /// Get current calculated damage value (for UI display)
    /// Rounds up as per user's example
    /// </summary>
    public int GetCurrentDamage()
    {
        float damage = CalculateUnleashDamage();
        return Mathf.CeilToInt(damage); // Round up
    }
    
    /// <summary>
    /// Get current calculated guard value (for UI display)
    /// For guard cards, calculates guard amount with preparation multiplier
    /// </summary>
    public int GetCurrentGuard()
    {
        if (sourceCard == null) return 0;
        
        // Check if this is a guard card
        CardType cardTypeEnum = sourceCard.GetCardTypeEnum();
        if (cardTypeEnum == CardType.Guard || (cardTypeEnum == CardType.Skill && sourceCard.block > 0))
        {
            // Calculate guard with preparation multiplier
            float baseGuard = storedBaseDamage; // Guard is stored as damage for calculation
            float multipliedGuard = baseGuard * (1f + currentMultiplier);
            float totalGuard = multipliedGuard + flatStrengthBonus;
            
            // Apply decay penalty if any
            if (decayAmount > 0f)
            {
                totalGuard *= (1f - decayAmount);
            }
            
            return Mathf.CeilToInt(totalGuard); // Round up
        }
        
        return 0;
    }
    
    /// <summary>
    /// Get a temporary CardDataExtended with updated values for UI display
    /// This allows the card UI to show the current prepared values
    /// </summary>
    public CardDataExtended GetDisplayCardData()
    {
        if (sourceCard == null) return null;
        
        // Create a copy for display (we'll modify the damage/block values)
        // Note: We can't modify ScriptableObjects directly, so we'll need to pass this info differently
        // For now, return the original and let the UI component handle the display
        return sourceCard;
    }
    
    /// <summary>
    /// Calculate energy cost to manually unleash this card
    /// Uses the base card cost (preparation doesn't increase cost)
    /// </summary>
    public int CalculateManualUnleashCost()
    {
        return sourceCard.playCost; // Base cost only - preparation doesn't increase cost
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

