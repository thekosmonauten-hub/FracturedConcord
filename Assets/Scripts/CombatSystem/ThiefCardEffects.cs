using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Helper class for processing Thief-specific card effects:
/// - Dual Wield effects
/// - Prepared card interactions
/// - Preparation bonuses
/// </summary>
public static class ThiefCardEffects
{
    /// <summary>
    /// Check if player is dual wielding (has multiple weapons of same type or specific dual wield setup)
    /// </summary>
    public static bool IsDualWielding(Character player)
    {
        if (player == null || player.weapons == null) return false;
        
        // Check if player has multiple weapons equipped
        // Dual wielding typically means having 2 melee weapons, or a melee + off-hand
        int weaponCount = 0;
        if (player.weapons.meleeWeapon != null) weaponCount++;
        if (player.weapons.projectileWeapon != null) weaponCount++;
        if (player.weapons.spellWeapon != null) weaponCount++;
        
        // For now, consider dual wielding if player has 2+ weapons
        // TODO: More specific dual wield detection (e.g., melee + off-hand, or specific weapon tags)
        return weaponCount >= 2;
    }
    
    /// <summary>
    /// Get count of prepared cards
    /// </summary>
    public static int GetPreparedCardCount()
    {
        var prepManager = PreparationManager.Instance;
        if (prepManager == null) return 0;
        
        // Use reflection to access private preparedCards list
        var field = typeof(PreparationManager).GetField("preparedCards", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var preparedCards = field.GetValue(prepManager) as List<PreparedCard>;
            if (preparedCards != null) return preparedCards.Count;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Consume all prepared cards and return the count consumed
    /// </summary>
    public static int ConsumeAllPreparedCards(Character player)
    {
        var prepManager = PreparationManager.Instance;
        if (prepManager == null) return 0;
        
        // Prevent recursive consumption - if we're already unleashing a card, don't consume again
        // (e.g., if Perfect Strike is being unleashed and tries to consume all prepared cards)
        if (prepManager.IsUnleashing)
        {
            Debug.LogWarning("<color=red>[Thief] Cannot consume prepared cards: Already unleashing a card (prevents infinite loop)</color>");
            return 0;
        }
        
        // Use reflection to access private preparedCards list
        var field = typeof(PreparationManager).GetField("preparedCards", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var preparedCards = field.GetValue(prepManager) as List<PreparedCard>;
            if (preparedCards != null && preparedCards.Count > 0)
            {
                // Create a copy of the list to avoid modification during iteration
                // (cards are removed from the list when unleashed)
                var cardsToUnleash = new List<PreparedCard>(preparedCards);
                int count = cardsToUnleash.Count;
                
                // Remove all prepared cards
                foreach (var prepared in cardsToUnleash)
                {
                    // Check if card is still in the list (might have been removed by previous unleash)
                    if (preparedCards.Contains(prepared))
                    {
                        prepManager.UnleashCardFree(prepared, player);
                    }
                }
                
                // Debug log removed to prevent memory leaks - uncomment only when debugging:
                // Debug.Log($"<color=orange>[Thief] Consumed {count} prepared cards</color>");
                return count;
            }
        }
        
        return 0;
    }
    
    /// <summary>
    /// Advance prepared card charges by specified amount
    /// </summary>
    public static void AdvancePreparedCardCharges(int amount)
    {
        var prepManager = PreparationManager.Instance;
        if (prepManager == null) return;
        
        // Use reflection to access private preparedCards list
        var field = typeof(PreparationManager).GetField("preparedCards", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var preparedCards = field.GetValue(prepManager) as List<PreparedCard>;
            if (preparedCards != null)
            {
                foreach (var prepared in preparedCards)
                {
                    // Advance charges by calling OnTurnEnd multiple times
                    for (int i = 0; i < amount; i++)
                    {
                        prepared.OnTurnEnd();
                    }
                }
                
                Debug.Log($"<color=cyan>[Thief] Advanced all prepared cards by {amount} charge(s)</color>");
                
                // Update UI - update each prepared card individually (reuse existing preparedCards variable)
                if (prepManager.preparedCardsUI != null && preparedCards != null)
                {
                    foreach (var prepared in preparedCards)
                    {
                        prepManager.preparedCardsUI.UpdatePreparedCard(prepared);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Process dual wield effect text and apply it
    /// </summary>
    public static void ProcessDualWieldEffect(string dualWieldEffect, Card card, Character player, Enemy targetEnemy = null)
    {
        if (string.IsNullOrEmpty(dualWieldEffect) || card == null || player == null) return;
        
        Debug.Log($"<color=yellow>[Dual Wield] Processing: {dualWieldEffect}</color>");
        
        // Parse and apply dual wield effects
        // Pattern: "Deal X damage (+Dex/Y) to all enemies"
        if (Regex.IsMatch(dualWieldEffect, @"Deal.*damage.*all enemies", RegexOptions.IgnoreCase))
        {
            ProcessDualWieldDamage(dualWieldEffect, card, player);
        }
        // Pattern: "Gain X Guard (+Dex/Y) and Z(+Dex*W) evasion"
        else if (Regex.IsMatch(dualWieldEffect, @"Gain.*Guard.*evasion", RegexOptions.IgnoreCase))
        {
            ProcessDualWieldGuardAndEvasion(dualWieldEffect, card, player);
        }
        // Pattern: "Apply X Poison and gain Y temporary Dexterity"
        else if (Regex.IsMatch(dualWieldEffect, @"Apply.*Poison.*Dexterity", RegexOptions.IgnoreCase))
        {
            ProcessDualWieldPoisonAndDexterity(dualWieldEffect, card, player, targetEnemy);
        }
        // Pattern: "Advance prepared cards charge by X"
        else if (Regex.IsMatch(dualWieldEffect, @"Advance.*prepared.*charge", RegexOptions.IgnoreCase))
        {
            int amount = ParseAdvanceAmount(dualWieldEffect);
            AdvancePreparedCardCharges(amount);
        }
        // Pattern: "Deal +X (+dex/Y) damage per consumed card instead"
        else if (Regex.IsMatch(dualWieldEffect, @"Deal.*damage per consumed card", RegexOptions.IgnoreCase))
        {
            // This is handled in the main card effect (Perfect Strike)
            Debug.Log($"<color=yellow>[Dual Wield] Perfect Strike dual wield effect will be handled in main effect</color>");
        }
        // Pattern: "Deal +X (+Dex/Y) damage per prepared card instead"
        else if (Regex.IsMatch(dualWieldEffect, @"Deal.*damage per prepared card", RegexOptions.IgnoreCase))
        {
            // This is handled in the main card effect (Ambush)
            Debug.Log($"<color=yellow>[Dual Wield] Ambush dual wield effect will be handled in main effect</color>");
        }
    }
    
    private static void ProcessDualWieldDamage(string effect, Card card, Character player)
    {
        // Parse: "Deal 1(+Dex/4) off-hand damage to all enemies."
        var match = Regex.Match(effect, @"Deal\s+(\d+)(?:\([^)]+\))?\s+.*damage.*all enemies", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            float baseDamage = float.Parse(match.Groups[1].Value);
            
            // Parse dexterity scaling: (+Dex/4)
            float dexDivisor = ParseDexterityDivisor(effect);
            float dexBonus = dexDivisor > 0 ? player.dexterity / dexDivisor : 0f;
            
            float totalDamage = baseDamage + dexBonus;
            
            Debug.Log($"<color=orange>[Dual Wield] Dealing {totalDamage:F1} off-hand damage to all enemies</color>");
            
            // Deal damage to all enemies
            var combatDisplay = Object.FindFirstObjectByType<CombatDisplayManager>();
            if (combatDisplay != null)
            {
                var enemySpawner = combatDisplay.enemySpawner;
                if (enemySpawner != null)
                {
                    var activeEnemies = enemySpawner.GetActiveEnemies();
                    if (activeEnemies != null)
                    {
                        for (int i = 0; i < activeEnemies.Count; i++)
                        {
                            combatDisplay.PlayerAttackEnemy(i, totalDamage);
                        }
                    }
                }
            }
        }
    }
    
    private static void ProcessDualWieldGuardAndEvasion(string effect, Card card, Character player)
    {
        // Parse: "Gain 6 Guard (+Dex/6) and 250(+Dex*2) evasion"
        var guardMatch = Regex.Match(effect, @"Gain\s+(\d+)\s+Guard", RegexOptions.IgnoreCase);
        var evasionMatch = Regex.Match(effect, @"(\d+)\([^)]*Dex[^)]*\)\s+evasion", RegexOptions.IgnoreCase);
        
        if (guardMatch.Success)
        {
            float baseGuard = float.Parse(guardMatch.Groups[1].Value);
            float dexDivisor = ParseDexterityDivisor(effect);
            float dexBonus = dexDivisor > 0 ? player.dexterity / dexDivisor : 0f;
            float totalGuard = baseGuard + dexBonus;
            
            player.AddGuard(totalGuard);
            Debug.Log($"<color=cyan>[Dual Wield] Gained {totalGuard:F1} guard</color>");
            
            var playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null) playerDisplay.UpdateGuardDisplay();
        }
        
        if (evasionMatch.Success)
        {
            float baseEvasion = float.Parse(evasionMatch.Groups[1].Value);
            // Parse: (+Dex*2) - multiplier instead of divisor
            var dexMultMatch = Regex.Match(effect, @"\([^)]*Dex\s*\*\s*(\d+)[^)]*\)", RegexOptions.IgnoreCase);
            float dexMultiplier = dexMultMatch.Success ? float.Parse(dexMultMatch.Groups[1].Value) : 0f;
            float dexBonus = dexMultiplier > 0 ? player.dexterity * dexMultiplier : 0f;
            float totalEvasion = baseEvasion + dexBonus;
            
            // Apply temporary evasion
            var playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                var statusMgr = playerDisplay.GetStatusEffectManager();
                if (statusMgr != null)
                {
                    var evasionEffect = new StatusEffect(StatusEffectType.TempEvasion, "TempEvasion", totalEvasion, -1, false);
                    statusMgr.AddStatusEffect(evasionEffect);
                    Debug.Log($"<color=cyan>[Dual Wield] Gained {totalEvasion:F1} temporary evasion</color>");
                }
            }
        }
    }
    
    private static void ProcessDualWieldPoisonAndDexterity(string effect, Card card, Character player, Enemy targetEnemy)
    {
        // Parse: "Apply 3 Poison and gain 1 temporary Dexterity"
        var poisonMatch = Regex.Match(effect, @"Apply\s+(\d+)\s+Poison", RegexOptions.IgnoreCase);
        var dexMatch = Regex.Match(effect, @"gain\s+(\d+)\s+temporary\s+Dexterity", RegexOptions.IgnoreCase);
        
        if (poisonMatch.Success && targetEnemy != null)
        {
            int poisonStacks = int.Parse(poisonMatch.Groups[1].Value);
            // Find enemy's StatusEffectManager via EnemyCombatDisplay
            var enemyDisplays = Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var display in enemyDisplays)
            {
                if (display != null && display.GetCurrentEnemy() == targetEnemy)
                {
                    var statusMgr = display.GetComponent<StatusEffectManager>();
                    if (statusMgr != null)
                    {
                        var poisonEffect = new StatusEffect(StatusEffectType.Poison, "Poison", poisonStacks, 3, true);
                        statusMgr.AddStatusEffect(poisonEffect);
                        Debug.Log($"<color=green>[Dual Wield] Applied {poisonStacks} Poison to {targetEnemy.enemyName}</color>");
                    }
                    break;
                }
            }
        }
        
        if (dexMatch.Success)
        {
            int dexGain = int.Parse(dexMatch.Groups[1].Value);
            // Get StatusEffectManager from PlayerCombatDisplay
            var playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                var statusMgr = playerDisplay.GetStatusEffectManager();
                if (statusMgr != null)
                {
                    var dexEffect = new StatusEffect(StatusEffectType.Dexterity, "TempDexterity", dexGain, 3, false);
                    statusMgr.AddStatusEffect(dexEffect);
                    Debug.Log($"<color=cyan>[Dual Wield] Gained {dexGain} temporary Dexterity</color>");
                }
            }
        }
    }
    
    private static int ParseAdvanceAmount(string effect)
    {
        var match = Regex.Match(effect, @"by\s+(\d+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        return 1; // Default to 1
    }
    
    private static float ParseDexterityDivisor(string text)
    {
        var match = Regex.Match(text, @"\([^)]*Dex\s*/\s*(\d+)[^)]*\)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return float.Parse(match.Groups[1].Value);
        }
        return 0f;
    }
}

