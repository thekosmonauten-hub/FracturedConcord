using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all prepared cards in combat.
/// Handles preparation, turn tracking, unleashing, and stat synergies.
/// Modular system similar to MomentumManager.
/// </summary>
public class PreparationManager : MonoBehaviour
{
    private static PreparationManager _instance;
    public static PreparationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PreparationManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PreparationManager");
                    _instance = go.AddComponent<PreparationManager>();
                }
            }
            return _instance;
        }
    }
    
    [Header("Preparation Settings")]
    [Tooltip("Maximum number of cards that can be prepared at once")]
    public int maxPreparedSlots = 3;
    
    [Tooltip("Default maximum turns a card can be prepared")]
    public int defaultMaxTurns = 3;
    
    [Tooltip("Can player unleash multiple cards per turn?")]
    public bool allowMultipleUnleashPerTurn = false;
    
    [Header("State Tracking")]
    [SerializeField] private List<PreparedCard> preparedCards = new List<PreparedCard>();
    private int unleashedThisTurn = 0;
    
    [Header("References")]
    public PreparedCardsUI preparedCardsUI;
    
    // Events
    public delegate void PreparedCardEvent(PreparedCard card);
    public event PreparedCardEvent OnCardPrepared;
    public event PreparedCardEvent OnCardUnleashed;
    public event PreparedCardEvent OnCardDecayed;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Debug.Log("<color=cyan>[PreparationManager] Initialized</color>");
    }
    
    /// <summary>
    /// Prepare a card for future unleashing
    /// </summary>
    public bool PrepareCard(CardDataExtended card, Character player)
    {
        // Check if we have space
        if (preparedCards.Count >= maxPreparedSlots)
        {
            Debug.LogWarning($"[PreparationManager] Cannot prepare {card.cardName}: All {maxPreparedSlots} slots full!");
            return false;
        }
        
        // Check if card can be prepared
        if (!card.canPrepare)
        {
            Debug.LogWarning($"[PreparationManager] Card {card.cardName} does not have Prepare capability!");
            return false;
        }
        
        // Create prepared card
        PreparedCard prepared = new PreparedCard(card, player);
        preparedCards.Add(prepared);
        
        Debug.Log($"<color=green>[PreparationManager] Prepared card: {card.cardName} ({preparedCards.Count}/{maxPreparedSlots} slots)</color>");
        
        // Notify UI
        OnCardPrepared?.Invoke(prepared);
        
        // Update UI
        if (preparedCardsUI != null)
        {
            preparedCardsUI.AddPreparedCard(prepared);
        }
        
        return true;
    }
    
    /// <summary>
    /// Manually unleash a prepared card (costs energy)
    /// </summary>
    public bool UnleashCardManually(PreparedCard prepared, Character player)
    {
        if (prepared == null || !preparedCards.Contains(prepared))
        {
            Debug.LogWarning("[PreparationManager] Cannot unleash: Invalid prepared card!");
            return false;
        }
        
        // Check turn limit
        if (!allowMultipleUnleashPerTurn && unleashedThisTurn > 0)
        {
            Debug.LogWarning("[PreparationManager] Cannot unleash: Already unleashed a card this turn!");
            return false;
        }
        
        // Calculate and check energy cost
        int energyCost = prepared.CalculateManualUnleashCost();
        if (player.mana < energyCost)
        {
            Debug.LogWarning($"[PreparationManager] Cannot unleash: Need {energyCost} energy, have {player.mana}!");
            return false;
        }
        
        // Pay energy cost
        if (!player.UseMana(energyCost))
        {
            Debug.LogWarning($"[PreparationManager] Failed to spend {energyCost} mana for unleash!");
            return false;
        }
        
        Debug.Log($"<color=yellow>[PreparationManager] Manual unleash: {prepared.sourceCard.cardName} (Cost: {energyCost} energy)</color>");
        
        // Execute unleash
        return ExecuteUnleash(prepared, player, false);
    }
    
    /// <summary>
    /// Unleash a prepared card for free (via Unleash card or auto-decay)
    /// </summary>
    public bool UnleashCardFree(PreparedCard prepared, Character player, bool isDecay = false)
    {
        if (prepared == null || !preparedCards.Contains(prepared))
        {
            Debug.LogWarning("[PreparationManager] Cannot unleash: Invalid prepared card!");
            return false;
        }
        
        Debug.Log($"<color=yellow>[PreparationManager] Free unleash: {prepared.sourceCard.cardName} (Decay: {isDecay})</color>");
        
        return ExecuteUnleash(prepared, player, isDecay);
    }
    
    /// <summary>
    /// Execute the unleash effect and cleanup
    /// </summary>
    private bool ExecuteUnleash(PreparedCard prepared, Character player, bool isDecay)
    {
        // Calculate final damage
        float unleashDamage = prepared.CalculateUnleashDamage();
        
        // Apply the card's effect with stored/modified values
        ApplyUnleashEffect(prepared, player, unleashDamage);
        
        // Track unleash count
        unleashedThisTurn++;
        
        // Remove from prepared list
        preparedCards.Remove(prepared);
        
        // Notify events
        if (isDecay)
        {
            OnCardDecayed?.Invoke(prepared);
        }
        else
        {
            OnCardUnleashed?.Invoke(prepared);
        }
        
        // Update UI
        if (preparedCardsUI != null)
        {
            preparedCardsUI.RemovePreparedCard(prepared);
        }
        
        Debug.Log($"<color=green>[PreparationManager] ✓ Unleashed: {prepared.sourceCard.cardName} (Damage: {unleashDamage:F1})</color>");
        
        return true;
    }
    
    /// <summary>
    /// Apply the unleash effect based on card type and stored values
    /// </summary>
    private void ApplyUnleashEffect(PreparedCard prepared, Character player, float damage)
    {
        CardDataExtended card = prepared.sourceCard;
        
        // Get unleash effect type from card
        string unleashEffect = card.unleashEffect;
        
        switch (unleashEffect?.ToLower())
        {
            case "deal_stored_damage":
            case "deal_damage":
            default:
                // Deal damage to all enemies (or specific targets based on card)
                DealUnleashDamage(prepared, player, damage);
                break;
                
            case "apply_buffs":
                // Apply stored buffs to player
                ApplyUnleashBuffs(prepared, player);
                break;
                
            case "hybrid":
                // Both damage and buffs
                DealUnleashDamage(prepared, player, damage);
                ApplyUnleashBuffs(prepared, player);
                break;
        }
        
        // Trigger any combo effects
        TriggerUnleashComboEffects(prepared, player);
    }
    
    /// <summary>
    /// Deal damage from unleashed card
    /// </summary>
    private void DealUnleashDamage(PreparedCard prepared, Character player, float damage)
    {
        // For now, deal to all enemies
        // TODO: Integrate with actual targeting system
        var combatManager = FindFirstObjectByType<CombatManager>();
        var combatDisplay = FindFirstObjectByType<CombatDisplayManager>();
        
        if (combatDisplay != null)
        {
            // Use CombatDisplayManager if available
            var activeEnemies = combatDisplay.GetActiveEnemies();
            if (activeEnemies != null)
            {
                foreach (var enemyDisplay in activeEnemies)
                {
                    if (enemyDisplay != null && enemyDisplay.currentHealth > 0)
                    {
                        // Apply damage type modifiers
                        float finalDamage = damage;
                        
                        // Add elemental/physical modifiers based on card tags
                        if (prepared.sourceCard.tags.Contains("Fire"))
                        {
                            float spellPower = player.intelligence * 0.1f;
                            finalDamage *= (1f + spellPower / 100f);
                        }
                        
                        enemyDisplay.TakeDamage(Mathf.RoundToInt(finalDamage));
                        
                        Debug.Log($"<color=orange>[Preparation] {prepared.sourceCard.cardName} dealt {finalDamage:F1} damage to {enemyDisplay.enemyName}</color>");
                    }
                }
            }
        }
        else if (combatManager != null && combatManager.enemies != null)
        {
            // Fallback to CombatManager enemies list
            foreach (var enemy in combatManager.enemies)
            {
                if (enemy != null && enemy.IsAlive())
                {
                    // Apply damage type modifiers
                    float finalDamage = damage;
                    
                    // Add elemental/physical modifiers based on card tags
                    if (prepared.sourceCard.tags.Contains("Fire"))
                    {
                        float spellPower = player.intelligence * 0.1f;
                        finalDamage *= (1f + spellPower / 100f);
                    }
                    
                    enemy.TakeDamage(finalDamage);
                    
                    Debug.Log($"<color=orange>[Preparation] {prepared.sourceCard.cardName} dealt {finalDamage:F1} damage to {enemy.enemyName}</color>");
                }
            }
        }
    }
    
    /// <summary>
    /// Apply buffs from unleashed card
    /// </summary>
    private void ApplyUnleashBuffs(PreparedCard prepared, Character player)
    {
        // Apply stored buffs based on card definition
        // TODO: Implement based on your buff system
        Debug.Log($"<color=cyan>[Preparation] Applied buffs from {prepared.sourceCard.cardName}</color>");
    }
    
    /// <summary>
    /// Trigger combo effects when unleashing
    /// </summary>
    private void TriggerUnleashComboEffects(PreparedCard prepared, Character player)
    {
        // Check for combo tags or special effects
        CardDataExtended card = prepared.sourceCard;
        
        // Example: If unleashed after 2+ turns, apply special effect
        if (prepared.turnsPrepared >= 2 && card.tags.Contains("Combo"))
        {
            Debug.Log($"<color=yellow>[Preparation] Combo bonus triggered for {card.cardName}!</color>");
            // Apply combo bonus (e.g., crit, additional damage, etc.)
        }
        
        // Momentum synergy example (optional - only if MomentumManager exists in your project)
        // Uncomment if you have MomentumManager:
        /*
        var momentumManager = FindFirstObjectByType<MomentumManager>();
        if (momentumManager != null && card.tags.Contains("Momentum"))
        {
            int momentumGain = prepared.turnsPrepared;
            momentumManager.GainMomentum(momentumGain);
            Debug.Log($"<color=cyan>[Preparation] Gained {momentumGain} Momentum from unleash!</color>");
        }
        */
    }
    
    /// <summary>
    /// Unleash all cards matching a specific tag or condition (for "Unleash" cards)
    /// </summary>
    public void UnleashByTag(string tag, Character player)
    {
        var toUnleash = preparedCards.Where(p => p.sourceCard.tags.Contains(tag)).ToList();
        
        Debug.Log($"<color=yellow>[PreparationManager] Unleashing {toUnleash.Count} cards with tag: {tag}</color>");
        
        foreach (var prepared in toUnleash)
        {
            UnleashCardFree(prepared, player, false);
        }
    }
    
    /// <summary>
    /// Called at the end of each turn to update all prepared cards
    /// </summary>
    public void OnTurnEnd()
    {
        Debug.Log($"<color=cyan>[PreparationManager] Turn end: Updating {preparedCards.Count} prepared cards</color>");
        
        // Reset unleash counter
        unleashedThisTurn = 0;
        
        // Update each prepared card
        List<PreparedCard> toAutoUnleash = new List<PreparedCard>();
        
        foreach (var prepared in preparedCards.ToList())
        {
            // Increment turn and apply bonuses
            prepared.OnTurnEnd();
            
            // Check for auto-unleash conditions
            if (prepared.ShouldAutoUnleash())
            {
                toAutoUnleash.Add(prepared);
            }
            
            // Update UI
            if (preparedCardsUI != null)
            {
                preparedCardsUI.UpdatePreparedCard(prepared);
            }
        }
        
        // Auto-unleash cards that reached their condition
        if (toAutoUnleash.Count > 0)
        {
            Debug.Log($"<color=yellow>[PreparationManager] Auto-unleashing {toAutoUnleash.Count} cards</color>");
            
            foreach (var prepared in toAutoUnleash)
            {
                bool isDecay = prepared.decayAmount >= 1f;
                UnleashCardFree(prepared, prepared.owner, isDecay);
            }
        }
    }
    
    /// <summary>
    /// Clear all prepared cards (e.g., on combat end)
    /// </summary>
    public void ClearAllPreparedCards()
    {
        Debug.Log($"<color=red>[PreparationManager] Clearing all {preparedCards.Count} prepared cards</color>");
        
        preparedCards.Clear();
        unleashedThisTurn = 0;
        
        if (preparedCardsUI != null)
        {
            preparedCardsUI.ClearAll();
        }
    }
    
    /// <summary>
    /// Get all currently prepared cards
    /// </summary>
    public List<PreparedCard> GetPreparedCards()
    {
        return new List<PreparedCard>(preparedCards);
    }
    
    /// <summary>
    /// Get number of prepared card slots available
    /// </summary>
    public int GetAvailableSlots()
    {
        return maxPreparedSlots - preparedCards.Count;
    }
    
    /// <summary>
    /// Check if a card can be prepared right now
    /// </summary>
    public bool CanPrepareCard(CardDataExtended card)
    {
        return card.canPrepare && preparedCards.Count < maxPreparedSlots;
    }
}

