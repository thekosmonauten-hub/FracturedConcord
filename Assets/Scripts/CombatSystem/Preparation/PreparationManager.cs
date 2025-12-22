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
    private bool isUnleashing = false; // Flag to prevent recursive consumption
    
    /// <summary>
    /// Check if a card is currently being unleashed (prevents recursive consumption)
    /// </summary>
    public bool IsUnleashing => isUnleashing;
    
    /// <summary>
    /// Check if a PreparedCard is still valid (exists in the prepared cards list)
    /// </summary>
    public bool IsPreparedCardValid(PreparedCard prepared)
    {
        if (prepared == null) return false;
        return preparedCards.Contains(prepared);
    }
    
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
        
        // Allow all cards to be prepared (canPrepare check removed for universal preparation)
        // The canPrepare flag is now optional - all cards can be prepared via right-click/shift-click
        
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
        
        // Check if card has at least 1 charge (cannot unleash same turn it's prepared)
        if (prepared.turnsPrepared < 1)
        {
            Debug.LogWarning($"[PreparationManager] Cannot unleash: Card needs at least 1 charge (currently {prepared.turnsPrepared}/{prepared.maxTurns})!");
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
        
        // Pay energy cost - use CharacterManager to trigger events
        bool manaSpent = false;
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager != null)
        {
            manaSpent = charManager.UseMana(energyCost);
        }
        else
        {
            // Fallback to direct character UseMana if CharacterManager not available
            manaSpent = player.UseMana(energyCost);
        }
        
        if (!manaSpent)
        {
            Debug.LogWarning($"[PreparationManager] Failed to spend {energyCost} mana for unleash!");
            return false;
        }
        
        Debug.Log($"<color=yellow>[PreparationManager] Manual unleash: {prepared.sourceCard.cardName} (Cost: {energyCost} energy)</color>");
        
        // Update UI after mana consumption (ensures all UI systems are updated)
        UpdateManaUI();
        
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
        
        // Remove from prepared list BEFORE applying effect to prevent recursive consumption
        // (e.g., if Perfect Strike is prepared and gets unleashed, it shouldn't consume itself)
        preparedCards.Remove(prepared);
        
        // Set flag to prevent recursive consumption during unleash
        bool wasUnleashing = isUnleashing;
        isUnleashing = true;
        
        try
        {
            // Apply the card's effect with stored/modified values
            ApplyUnleashEffect(prepared, player, unleashDamage);
        }
        finally
        {
            // Restore flag
            isUnleashing = wasUnleashing;
        }
        
        // Track unleash count
        unleashedThisTurn++;
        
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
        
        // Add unleashed card to discard pile
        var combatDeckManager = FindFirstObjectByType<CombatDeckManager>();
        if (combatDeckManager != null)
        {
            combatDeckManager.AddCardToDiscardPile(prepared.sourceCard);
            Debug.Log($"<color=cyan>[PreparationManager] Added {prepared.sourceCard.cardName} to discard pile</color>");
        }
        else
        {
            Debug.LogWarning("[PreparationManager] CombatDeckManager not found - cannot add unleashed card to discard pile");
        }
        
        return true;
    }
    
    /// <summary>
    /// Updates the mana UI display after mana consumption
    /// </summary>
    private void UpdateManaUI()
    {
        // Update PlayerCombatDisplay if it exists
        PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            playerDisplay.UpdateManaDisplay();
            Debug.Log("[PreparationManager] Updated PlayerCombatDisplay mana UI");
        }
        
        // Update AnimatedCombatUI if it exists
        AnimatedCombatUI animatedUI = FindFirstObjectByType<AnimatedCombatUI>();
        if (animatedUI != null)
        {
            animatedUI.AnimatePlayerMana();
            Debug.Log("[PreparationManager] Updated AnimatedCombatUI mana UI");
        }
        
        // Update CombatUI (UIElements-based) if it exists
        CombatUI combatUI = FindFirstObjectByType<CombatUI>();
        if (combatUI != null)
        {
            combatUI.UpdateCombatUI();
            Debug.Log("[PreparationManager] Updated CombatUI");
        }
    }
    
    /// <summary>
    /// Apply the unleash effect based on card type and stored values
    /// </summary>
    private void ApplyUnleashEffect(PreparedCard prepared, Character player, float damage)
    {
        CardDataExtended card = prepared.sourceCard;
        
        // Check card type first - Guard cards should apply guard, not deal damage
        if (card.GetCardTypeEnum() == CardType.Guard)
        {
            // Guard cards: Apply base guard + prepared card guard bonus
            ApplyUnleashGuard(prepared, player);
            return;
        }
        
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
    /// Apply guard from unleashed Guard card (base guard + prepared card guard bonus)
    /// </summary>
    private void ApplyUnleashGuard(PreparedCard prepared, Character player)
    {
        if (prepared == null || prepared.sourceCard == null || player == null) return;
        
        CardDataExtended card = prepared.sourceCard;
        
        // Get CardEffectProcessor to use the full guard calculation pipeline
        var cardEffectProcessor = FindFirstObjectByType<CardEffectProcessor>();
        if (cardEffectProcessor == null)
        {
            Debug.LogError("[PreparationManager] CardEffectProcessor not found! Cannot apply guard.");
            return;
        }
        
        // Convert CardDataExtended to Card for processing
        Card cardObj = card.ToCard();
        
        // Apply base guard using CardEffectProcessor (handles all the normal guard calculation)
        // This will apply base guard, scaling, momentum effects, etc.
        cardEffectProcessor.ApplyCardToEnemy(cardObj, null, player, Vector3.zero, false);
        
        // Now apply the prepared card guard bonus on top of the base guard
        // This is the bonus that scales with the number of prepared cards
        if (card.preparedCardGuardBase > 0f || 
            (card.preparedCardGuardScaling != null && 
             (card.preparedCardGuardScaling.strengthDivisor > 0f || 
              card.preparedCardGuardScaling.dexterityDivisor > 0f || 
              card.preparedCardGuardScaling.intelligenceDivisor > 0f ||
              card.preparedCardGuardScaling.strengthScaling > 0f ||
              card.preparedCardGuardScaling.dexterityScaling > 0f ||
              card.preparedCardGuardScaling.intelligenceScaling > 0f)))
        {
            // Calculate guard per prepared card (including scaling)
            float guardPerCard = card.preparedCardGuardBase;
            if (card.preparedCardGuardScaling != null)
            {
                guardPerCard += card.preparedCardGuardScaling.CalculateScalingBonus(player);
            }
            
            // Get the number of prepared cards (this card was already removed from the list)
            // So we need to add 1 to account for this card itself
            int preparedCount = (preparedCards != null ? preparedCards.Count : 0) + 1;
            
            // Apply guard with preparation multiplier
            float bonusGuard = guardPerCard * preparedCount * (1f + prepared.currentMultiplier);
            player.AddGuard(bonusGuard);
            
            Debug.Log($"<color=cyan>[Preparation Bonus] {card.cardName} gained {bonusGuard:F1} bonus guard from preparation (base: {card.preparedCardGuardBase} per card, scaling: {card.preparedCardGuardScaling?.CalculateScalingBonus(player) ?? 0f}, prepared count: {preparedCount}, multiplier: {1f + prepared.currentMultiplier:F2})</color>");
            
            // Update guard display
            PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                playerDisplay.UpdateGuardDisplay();
            }
        }
        
        // Apply temporary Dexterity bonus if specified in description
        int tempDex = PreparationBonusParser.ParsePreparationTempDexterity(card.description);
        if (tempDex > 0)
        {
            var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                var statusMgr = playerDisplay.GetStatusEffectManager();
                if (statusMgr != null)
                {
                    var dexEffect = new StatusEffect(StatusEffectType.Dexterity, "TempDexterity", tempDex, 3, false);
                    statusMgr.AddStatusEffect(dexEffect);
                    Debug.Log($"<color=cyan>[Preparation Bonus] {card.cardName} gained {tempDex} temporary Dexterity from preparation bonus</color>");
                }
            }
        }
        
        // Trigger any combo effects
        TriggerUnleashComboEffects(prepared, player);
    }
    
    /// <summary>
    /// Deal damage from unleashed card
    /// Uses CardEffectProcessor to ensure momentum effects (AoE, multi-hit, random targets, etc.) are preserved
    /// </summary>
    private void DealUnleashDamage(PreparedCard prepared, Character player, float damage)
    {
        if (prepared == null || prepared.sourceCard == null || player == null)
        {
            Debug.LogError("[PreparationManager] Cannot deal unleash damage: Invalid prepared card or player!");
            return;
        }
        
        // Get CardEffectProcessor to use the full card processing pipeline
        var cardEffectProcessor = FindFirstObjectByType<CardEffectProcessor>();
        if (cardEffectProcessor == null)
        {
            Debug.LogError("[PreparationManager] CardEffectProcessor not found! Cannot process momentum effects.");
            return;
        }
        
        // Convert CardDataExtended to Card for processing
        Card card = prepared.sourceCard.ToCard();
        
        // For momentum-based cards ("per Momentum spent"), apply preparation multiplier to baseDamage
        // CardEffectProcessor will handle momentum spending and damage calculation
        // For other cards, use the calculated unleash damage
        bool isMomentumBased = MomentumEffectParser.HasPerMomentumSpent(prepared.sourceCard.description);
        if (!isMomentumBased)
        {
            // Update card's damage to the calculated unleash damage
            // This preserves the preparation multiplier while allowing momentum effects to modify targeting
            card.baseDamage = damage;
        }
        else
        {
            // For momentum-based cards, apply preparation multiplier to baseDamage per momentum
            // This way, when CalculatePerMomentumDamage uses card.baseDamage, it will use the multiplied value
            float originalBaseDamage = card.baseDamage;
            float preparationMultiplier = 1f + prepared.currentMultiplier;
            card.baseDamage = originalBaseDamage * preparationMultiplier;
            Debug.Log($"<color=cyan>[Preparation] Momentum-based card {prepared.sourceCard.cardName} - applying multiplier: {originalBaseDamage} * {preparationMultiplier:F2} = {card.baseDamage:F1} per momentum</color>");
        }
        
        // Get target enemy using the same targeting system as regular cards
        // This ensures unleashed cards follow the same targeting rules (selected enemy, AoE, etc.)
        Enemy targetEnemy = null;
        Vector3 targetScreenPosition = Vector3.zero;
        
        // For single-target cards, use the selected enemy from EnemyTargetingManager
        // For AoE cards, targetEnemy should be null (CardEffectProcessor handles AoE targeting)
        if (!card.isAoE)
        {
            // Use EnemyTargetingManager to get the currently selected enemy (same as regular cards)
            if (EnemyTargetingManager.Instance != null)
            {
                targetEnemy = EnemyTargetingManager.Instance.GetTargetedEnemy();
                targetScreenPosition = EnemyTargetingManager.Instance.GetTargetedEnemyPosition();
                
                // If no valid target found, fall back to first available enemy
                if (targetEnemy == null)
                {
                    var combatDisplay = FindFirstObjectByType<CombatDisplayManager>();
                    if (combatDisplay != null && combatDisplay.enemySpawner != null)
                    {
                        var activeDisplays = combatDisplay.enemySpawner.GetActiveEnemies();
                        if (activeDisplays != null && activeDisplays.Count > 0)
                        {
                            var firstEnemyDisplay = activeDisplays[0];
                            if (firstEnemyDisplay != null)
                            {
                                targetEnemy = firstEnemyDisplay.GetEnemy();
                                if (targetEnemy != null && firstEnemyDisplay.transform != null)
                                {
                                    targetScreenPosition = firstEnemyDisplay.transform.position;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Fallback if EnemyTargetingManager not available
                var combatDisplay = FindFirstObjectByType<CombatDisplayManager>();
                if (combatDisplay != null && combatDisplay.enemySpawner != null)
                {
                    var activeDisplays = combatDisplay.enemySpawner.GetActiveEnemies();
                    if (activeDisplays != null && activeDisplays.Count > 0)
                    {
                        var firstEnemyDisplay = activeDisplays[0];
                        if (firstEnemyDisplay != null)
                        {
                            targetEnemy = firstEnemyDisplay.GetEnemy();
                            if (targetEnemy != null && firstEnemyDisplay.transform != null)
                            {
                                targetScreenPosition = firstEnemyDisplay.transform.position;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // For AoE cards, get position from first enemy for animation reference
            // CardEffectProcessor will handle actual AoE targeting
            if (EnemyTargetingManager.Instance != null)
            {
                targetScreenPosition = EnemyTargetingManager.Instance.GetTargetedEnemyPosition();
            }
            else
            {
                var combatDisplay = FindFirstObjectByType<CombatDisplayManager>();
                if (combatDisplay != null && combatDisplay.enemySpawner != null)
                {
                    var activeDisplays = combatDisplay.enemySpawner.GetActiveEnemies();
                    if (activeDisplays != null && activeDisplays.Count > 0)
                    {
                        var firstEnemyDisplay = activeDisplays[0];
                        if (firstEnemyDisplay != null && firstEnemyDisplay.transform != null)
                        {
                            targetScreenPosition = firstEnemyDisplay.transform.position;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"<color=orange>[Preparation] Unleashing {prepared.sourceCard.cardName} with {damage:F1} damage (will process momentum effects)</color>");
        
        // Use CardEffectProcessor to apply the card - this will process momentum threshold effects
        // (AoE conversion, multi-hit, random targets, etc.)
        cardEffectProcessor.ApplyCardToEnemy(card, targetEnemy, player, targetScreenPosition, false);
    }
    
    /// <summary>
    /// Apply buffs from unleashed card
    /// </summary>
    private void ApplyUnleashBuffs(PreparedCard prepared, Character player)
    {
        if (prepared == null || prepared.sourceCard == null || player == null) return;
        
        CardDataExtended card = prepared.sourceCard;
        
        // Use the new configurable fields from CardDataExtended for guard bonuses
        // This is more reliable than parsing description text
        if (card.preparedCardGuardBase > 0f || 
            (card.preparedCardGuardScaling != null && 
             (card.preparedCardGuardScaling.strengthDivisor > 0f || 
              card.preparedCardGuardScaling.dexterityDivisor > 0f || 
              card.preparedCardGuardScaling.intelligenceDivisor > 0f ||
              card.preparedCardGuardScaling.strengthScaling > 0f ||
              card.preparedCardGuardScaling.dexterityScaling > 0f ||
              card.preparedCardGuardScaling.intelligenceScaling > 0f)))
        {
            // Calculate guard per prepared card (including scaling)
            float guardPerCard = card.preparedCardGuardBase;
            if (card.preparedCardGuardScaling != null)
            {
                guardPerCard += card.preparedCardGuardScaling.CalculateScalingBonus(player);
            }
            
            // Get the number of prepared cards (including this one)
            int preparedCount = preparedCards != null ? preparedCards.Count : 0;
            
            // Apply guard with preparation multiplier
            float totalGuard = guardPerCard * preparedCount * (1f + prepared.currentMultiplier);
            player.AddGuard(totalGuard);
            
            Debug.Log($"<color=cyan>[Preparation Bonus] {card.cardName} gained {totalGuard:F1} guard from preparation bonus (base: {card.preparedCardGuardBase} per card, scaling: {card.preparedCardGuardScaling?.CalculateScalingBonus(player) ?? 0f}, prepared count: {preparedCount}, multiplier: {1f + prepared.currentMultiplier:F2})</color>");
            
            // Update guard display
            PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                playerDisplay.UpdateGuardDisplay();
            }
        }
        else
        {
            // Fallback: Check for preparation bonuses in description (legacy support)
            if (PreparationBonusParser.HasPreparationBonus(card.description))
            {
                // Parse preparation guard bonus
                var (prepGuard, prepGuardDexDivisor) = PreparationBonusParser.ParsePreparationGuard(card.description);
                if (prepGuard > 0f)
                {
                    float guardAmount = prepGuard;
                    
                    // Add dexterity scaling if specified
                    if (prepGuardDexDivisor > 0f)
                    {
                        float dexBonus = player.dexterity / prepGuardDexDivisor;
                        guardAmount += dexBonus;
                    }
                    
                    // Apply guard with preparation multiplier
                    float multipliedGuard = guardAmount * (1f + prepared.currentMultiplier);
                    player.AddGuard(multipliedGuard);
                    
                    Debug.Log($"<color=cyan>[Preparation Bonus] {card.cardName} gained {multipliedGuard:F1} guard from preparation bonus (base: {prepGuard} + Dex/{prepGuardDexDivisor}, multiplier: {1f + prepared.currentMultiplier:F2})</color>");
                    
                    // Update guard display
                    PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
                    if (playerDisplay != null)
                    {
                        playerDisplay.UpdateGuardDisplay();
                    }
                }
            }
        }
        
        // Parse preparation temporary Dexterity bonus (still from description for now)
        int tempDex = PreparationBonusParser.ParsePreparationTempDexterity(card.description);
        if (tempDex > 0)
        {
            // Get StatusEffectManager from PlayerCombatDisplay
            var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                var statusMgr = playerDisplay.GetStatusEffectManager();
                if (statusMgr != null)
                {
                    // Use Dexterity status effect type for temporary dexterity boosts
                    var dexEffect = new StatusEffect(StatusEffectType.Dexterity, "TempDexterity", tempDex, 3, false);
                    statusMgr.AddStatusEffect(dexEffect);
                    Debug.Log($"<color=cyan>[Preparation Bonus] {card.cardName} gained {tempDex} temporary Dexterity from preparation bonus</color>");
                }
            }
        }
        
        // Apply any other stored buffs based on card definition
        Debug.Log($"<color=cyan>[Preparation] Applied buffs from {card.cardName}</color>");
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
        
        // Update all card scales after turn end (charges changed, queue order may have changed)
        if (preparedCardsUI != null)
        {
            preparedCardsUI.UpdateAllCardScales();
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
        // Allow all cards to be prepared (canPrepare check removed for universal preparation)
        return preparedCards.Count < maxPreparedSlots;
    }
}

