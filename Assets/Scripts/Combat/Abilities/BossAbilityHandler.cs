using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Centralized handler for complex boss abilities that require special logic
/// beyond standard AbilityEffect scriptable objects.
/// Similar pattern to ModifierEffectHandler for Ascendancy modifiers.
/// </summary>
public static class BossAbilityHandler
{
    // ====================
    // EVENT HOOKS
    // ====================
    
    /// <summary>
    /// Called when player plays a card. Triggers reactive abilities like Sundering Echo, Judgment Loop, etc.
    /// </summary>
    /// <param name="card">The card that was played</param>
    /// <param name="turnCardCount">Number of cards played this turn (including this one)</param>
    public static void OnPlayerCardPlayed(Card card, int turnCardCount)
    {
        Debug.Log($"<color=cyan>[BossAbilityHandler] OnPlayerCardPlayed: {card.cardName}, Type: {card.cardType}, Count: {turnCardCount}</color>");
        
        var activeEnemies = GetActiveEnemies();
        Debug.Log($"<color=cyan>[BossAbilityHandler] Found {activeEnemies.Count} active enemies</color>");
        
        foreach (var enemyDisplay in activeEnemies)
        {
            Enemy enemy = enemyDisplay.GetCurrentEnemy();
            if (enemy == null) continue;
            
            Debug.Log($"<color=cyan>[BossAbilityHandler] Checking enemy: {enemy.enemyName}</color>");
            
            // Sundering Echo - duplicate first card with corruption
            if (turnCardCount == 1 && HasAbility(enemy, BossAbilityType.SunderingEcho))
            {
                ProcessSunderingEcho(card, enemy);
            }
            
            // Judgment Loop - repeat attack on card type repeat
            if (HasAbility(enemy, BossAbilityType.JudgmentLoop))
            {
                Card previousCard = GetPreviousCard(enemy);
                if (previousCard != null)
                {
                    ProcessJudgmentLoop(card, previousCard, enemy, enemyDisplay);
                }
            }
            
            // Retaliation on Skill
            bool hasRetaliation = HasAbility(enemy, BossAbilityType.RetaliationOnSkill);
            Debug.Log($"<color=yellow>[BossAbilityHandler] {enemy.enemyName} has RetaliationOnSkill: {hasRetaliation}</color>");
            
            // Debug: Check what abilities are registered
            var abilities = GetCustomData(enemy, "bossAbilities") as List<BossAbilityType>;
            if (abilities != null)
            {
                Debug.Log($"<color=magenta>[BossAbilityHandler] {enemy.enemyName} registered abilities: {string.Join(", ", abilities)}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>[BossAbilityHandler] {enemy.enemyName} has NO boss abilities registered!</color>");
            }
            
            if (hasRetaliation)
            {
                Debug.Log($"<color=yellow>[BossAbilityHandler] Processing Echo of Breaking for {card.cardName} (type: {card.cardType})</color>");
                ProcessEchoOfBreaking(card, enemy);
            }
            else
            {
                Debug.Log($"<color=grey>[BossAbilityHandler] {enemy.enemyName} does not have RetaliationOnSkill</color>");
            }
            
            // Track cards hit for Cloak of Dusk
            if (HasAbility(enemy, BossAbilityType.ConditionalStealth))
            {
                IncrementCardsHitCount(enemy);
            }
        }
        
        // Store as previous card for all enemies
        foreach (var enemyDisplay in activeEnemies)
        {
            Enemy enemy = enemyDisplay.GetCurrentEnemy();
            if (enemy != null)
            {
                SetCustomData(enemy, "previousCard", card);
            }
        }
    }
    
    /// <summary>
    /// Called when player gains Guard. Triggers reactive abilities like Seep, Bloom of Ruin.
    /// </summary>
    public static void OnPlayerGainedGuard(float guardAmount)
    {
        var activeEnemies = GetActiveEnemies();
        
        foreach (var enemyDisplay in activeEnemies)
        {
            Enemy enemy = enemyDisplay.GetCurrentEnemy();
            if (enemy == null) continue;
            
            // Reactive Seep - damage on guard gain
            if (HasAbility(enemy, BossAbilityType.ReactiveSeep))
            {
                ProcessSeep(guardAmount, enemy);
            }
            
            // Bloom of Ruin - trigger zone
            if (HasAbility(enemy, BossAbilityType.BloomOfRuin))
            {
                ProcessBloomOfRuin(guardAmount, enemy);
            }
        }
    }
    
    /// <summary>
    /// Called at the start of enemy's turn. Used for abilities with turn-based triggers.
    /// </summary>
    public static void OnEnemyTurnStart(Enemy enemy, EnemyCombatDisplay enemyDisplay)
    {
        if (enemy == null) return;
        
        // Reset first attack tracker for Empty Footfalls
        if (HasAbility(enemy, BossAbilityType.AvoidFirstAttack))
        {
            SetCustomData(enemy, "attacksThisTurn", 0);
        }
        
        // Reset hit tracker for Cooling Regret
        if (HasAbility(enemy, BossAbilityType.ConditionalHeal))
        {
            SetCustomData(enemy, "hitThisTurn", false);
        }
        
        // Check Barrier of Dissent cycle
        if (HasAbility(enemy, BossAbilityType.BarrierOfDissent))
        {
            int currentTurn = GetCombatTurnCount();
            CheckBarrierOfDissent(enemy, enemyDisplay, currentTurn);
        }
    }
    
    /// <summary>
    /// Called at the end of enemy's turn.
    /// </summary>
    public static void OnEnemyTurnEnd(Enemy enemy, EnemyCombatDisplay enemyDisplay)
    {
        if (enemy == null) return;
        
        // Check Cooling Regret heal condition
        if (HasAbility(enemy, BossAbilityType.ConditionalHeal))
        {
            CheckCoolingRegret(enemy);
        }
        
        // Check Cloak of Dusk stealth condition
        if (HasAbility(enemy, BossAbilityType.ConditionalStealth))
        {
            CheckCloakOfDusk(enemy, enemyDisplay);
        }
    }
    
    /// <summary>
    /// Called when an enemy is damaged. Used for tracking hits.
    /// </summary>
    public static void OnEnemyDamaged(Enemy enemy, float damageAmount)
    {
        if (enemy == null || damageAmount <= 0) return;
        
        // Track hit for Cooling Regret
        if (HasAbility(enemy, BossAbilityType.ConditionalHeal))
        {
            SetCustomData(enemy, "hitThisTurn", true);
        }
    }
    
    /// <summary>
    /// Called before damage is applied to check if enemy should evade.
    /// </summary>
    public static bool ShouldEvadeAttack(Enemy enemy)
    {
        if (!HasAbility(enemy, BossAbilityType.AvoidFirstAttack)) return false;
        
        int attacksThisTurn = (int)(GetCustomData(enemy, "attacksThisTurn") ?? 0);
        
        if (attacksThisTurn == 0)
        {
            SetCustomData(enemy, "attacksThisTurn", 1);
            Debug.Log($"[Empty Footfalls] {enemy.enemyName} evades the first attack!");
            return true;
        }
        
        return false;
    }
    
    // ====================
    // ABILITY PROCESSORS
    // ====================
    
    /// <summary>
    /// Sundering Echo - Duplicate first card with corrupted values
    /// </summary>
    private static void ProcessSunderingEcho(Card firstCard, Enemy boss)
    {
        if (firstCard == null) return;
        
        // TODO: Implement card cloning and corruption
        // This requires CombatDeckManager integration
        Debug.Log($"[Sundering Echo] {boss.enemyName} corrupts {firstCard.cardName}!");
        
        // Future implementation:
        // Card corruptedCard = firstCard.Clone();
        // corruptedCard.baseDamage = Mathf.FloorToInt(firstCard.baseDamage * 0.5f);
        // corruptedCard.manaCost += 1;
        // corruptedCard.cardName = "Corrupted " + firstCard.cardName;
        // CombatDeckManager.Instance.AddCardToHand(corruptedCard);
    }
    
    /// <summary>
    /// Judgment Loop - Repeat attack if same card type played twice
    /// </summary>
    private static void ProcessJudgmentLoop(Card currentCard, Card previousCard, Enemy boss, EnemyCombatDisplay enemyDisplay)
    {
        if (currentCard.cardType == previousCard.cardType)
        {
            int repeatDamage = (int)(GetCustomData(boss, "lastAttackDamage") ?? boss.baseDamage);
            
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
            {
                // Use CharacterManager.TakeDamage() to properly update UI
                CharacterManager.Instance.TakeDamage(repeatDamage);
                Debug.Log($"[Judgment Loop] {boss.enemyName} repeats attack for {repeatDamage} damage!");
                
                // Show floating damage on player
                var floatingDamageManager = Object.FindFirstObjectByType<FloatingDamageManager>();
                var playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
                if (floatingDamageManager != null && playerDisplay != null)
                {
                    floatingDamageManager.ShowDamage(repeatDamage, false, playerDisplay.transform);
                }
                
                // Log to combat UI
                var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
                if (combatUI != null)
                {
                    combatUI.LogMessage($"<color=red>Judgment Loop!</color> {boss.enemyName} strikes again!");
                }
            }
        }
    }
    
    /// <summary>
    /// Reactive Seep - Damage when player gains guard
    /// </summary>
    private static void ProcessSeep(float guardAmount, Enemy boss)
    {
        int damage = Mathf.FloorToInt(guardAmount * 0.5f); // 50% of guard as damage
        
        if (damage > 0 && CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            // Use CharacterManager.TakeDamage() to properly update UI
            CharacterManager.Instance.TakeDamage(damage);
            Debug.Log($"[Seep] {boss.enemyName} deals {damage} damage (you gained {guardAmount} guard)");
            
            // Show floating damage on player
            var floatingDamageManager = Object.FindFirstObjectByType<FloatingDamageManager>();
            var playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
            if (floatingDamageManager != null && playerDisplay != null)
            {
                floatingDamageManager.ShowDamage(damage, false, playerDisplay.transform);
            }
            
            var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
            if (combatUI != null)
            {
                combatUI.LogMessage($"<color=green>Seep!</color> {damage} damage from gaining guard.");
            }
        }
    }
    
    /// <summary>
    /// Echo of Breaking - Retaliation when Skill played
    /// </summary>
    private static void ProcessEchoOfBreaking(Card playedCard, Enemy boss)
    {
        Debug.Log($"<color=magenta>[Echo of Breaking] Checking card: {playedCard.cardName}, CardType: {playedCard.cardType}, isSkill: {playedCard.cardType == CardType.Skill}</color>");
        
        if (playedCard.cardType == CardType.Skill)
        {
            int damage = boss.baseDamage;
            
            Debug.Log($"<color=red>[Echo of Breaking] TRIGGERED! {boss.enemyName} retaliates for {damage} damage!</color>");
            
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
            {
                CharacterManager.Instance.GetCurrentCharacter().TakeDamage(damage, DamageType.Physical);
                Debug.Log($"<color=red>[Echo of Breaking] Damage applied to player!</color>");
                
                var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
                if (combatUI != null)
                {
                    combatUI.LogMessage($"<color=yellow>Echo of Breaking!</color> Skill retaliation: {damage} damage.");
                }
            }
            else
            {
                Debug.LogError($"[Echo of Breaking] CharacterManager or Character is NULL!");
            }
        }
        else
        {
            Debug.Log($"<color=grey>[Echo of Breaking] Card is not Skill type, no retaliation.</color>");
        }
    }
    
    /// <summary>
    /// Bloom of Ruin - Trigger zone when guard gained
    /// </summary>
    private static void ProcessBloomOfRuin(float guardAmount, Enemy boss)
    {
        bool hasActiveBloom = (bool)(GetCustomData(boss, "bloomActive") ?? false);
        
        if (hasActiveBloom)
        {
            int damage = 20; // Fixed damage from zone trigger
            
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
            {
                // Use CharacterManager.TakeDamage() to properly update UI
                CharacterManager.Instance.TakeDamage(damage);
                SetCustomData(boss, "bloomActive", false);
                Debug.Log($"[Bloom of Ruin] Zone triggered! {damage} damage dealt.");
                
                // Show floating damage on player
                var floatingDamageManager = Object.FindFirstObjectByType<FloatingDamageManager>();
                var playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
                if (floatingDamageManager != null && playerDisplay != null)
                {
                    floatingDamageManager.ShowDamage(damage, false, playerDisplay.transform);
                }
                
                var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
                if (combatUI != null)
                {
                    combatUI.LogMessage($"<color=purple>Bloom of Ruin!</color> The zone erupts for {damage} damage!");
                }
            }
        }
    }
    
    /// <summary>
    /// Cooling Regret - Heal if not hit this turn
    /// </summary>
    private static void CheckCoolingRegret(Enemy boss)
    {
        bool wasHitThisTurn = (bool)(GetCustomData(boss, "hitThisTurn") ?? false);
        
        if (!wasHitThisTurn)
        {
            int healAmount = Mathf.FloorToInt(boss.maxHealth * 0.15f); // 15% max HP
            boss.currentHealth = Mathf.Min(boss.currentHealth + healAmount, boss.maxHealth);
            
            Debug.Log($"[Cooling Regret] {boss.enemyName} heals for {healAmount}.");
            
            var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
            if (combatUI != null)
            {
                combatUI.LogMessage($"<color=green>Cooling Regret!</color> {boss.enemyName} heals {healAmount} HP.");
            }
            
            // Update display
            var enemyDisplay = FindEnemyDisplay(boss);
            if (enemyDisplay != null)
            {
                enemyDisplay.RefreshDisplay();
            }
        }
    }
    
    /// <summary>
    /// Cloak of Dusk - Stealth after hit by 3 cards
    /// </summary>
    private static void CheckCloakOfDusk(Enemy boss, EnemyCombatDisplay enemyDisplay)
    {
        int cardsHitThisTurn = (int)(GetCustomData(boss, "cardsHitCount") ?? 0);
        
        if (cardsHitThisTurn >= 3)
        {
            // Apply Invisible status
            var statusEffect = new StatusEffect(
                StatusEffectType.Invisible,
                "Cloak of Dusk",
                1f,
                1
            );
            
            if (enemyDisplay != null)
            {
                enemyDisplay.AddStatusEffect(statusEffect);
            }
            
            Debug.Log($"[Cloak of Dusk] {boss.enemyName} becomes untargetable!");
            
            var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
            if (combatUI != null)
            {
                combatUI.LogMessage($"<color=grey>Cloak of Dusk!</color> {boss.enemyName} vanishes into shadow!");
            }
            
            // Reset counter
            SetCustomData(boss, "cardsHitCount", 0);
        }
    }
    
    /// <summary>
    /// Barrier of Dissent - Invulnerability cycle with health loss
    /// </summary>
    private static void CheckBarrierOfDissent(Enemy boss, EnemyCombatDisplay enemyDisplay, int currentTurn)
    {
        if (currentTurn % 3 == 0)
        {
            // Apply invulnerability
            var barrierEffect = new StatusEffect(
                StatusEffectType.Invulnerable,
                "Barrier of Dissent",
                1f,
                1
            );
            
            if (enemyDisplay != null)
            {
                enemyDisplay.AddStatusEffect(barrierEffect);
            }
            
            // Lose health
            int healthLoss = Mathf.FloorToInt(boss.maxHealth * 0.10f);
            boss.currentHealth = Mathf.Max(1, boss.currentHealth - healthLoss); // Don't die from this
            
            Debug.Log($"[Barrier of Dissent] {boss.enemyName} becomes invulnerable but loses {healthLoss} HP!");
            
            var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
            if (combatUI != null)
            {
                combatUI.LogMessage($"<color=cyan>Barrier of Dissent!</color> Invulnerable but loses {healthLoss} HP.");
            }
            
            // Update display
            if (enemyDisplay != null)
            {
                enemyDisplay.RefreshDisplay();
            }
        }
    }
    
    // ====================
    // HELPER METHODS
    // ====================
    
    /// <summary>
    /// Check if enemy has a specific boss ability
    /// </summary>
    public static bool HasAbility(this Enemy enemy, BossAbilityType abilityType)
    {
        if (enemy == null)
        {
            Debug.LogWarning("[BossAbilityHandler] HasAbility called with null enemy");
            return false;
        }
        
        // Check if this ability type is stored in custom data
        var abilities = GetCustomData(enemy, "bossAbilities") as List<BossAbilityType>;
        
        Debug.Log($"<color=cyan>[BossAbilityHandler] HasAbility check for {enemy.enemyName}: abilities={abilities?.Count ?? 0}, looking for {abilityType}</color>");
        
        if (abilities != null)
        {
            foreach (var ability in abilities)
            {
                Debug.Log($"  - Has: {ability}");
            }
        }
        
        return abilities != null && abilities.Contains(abilityType);
    }
    
    /// <summary>
    /// Register a boss ability on an enemy
    /// </summary>
    public static void RegisterAbility(this Enemy enemy, BossAbilityType abilityType)
    {
        if (enemy == null || abilityType == BossAbilityType.None)
        {
            Debug.LogWarning($"[BossAbilityHandler] RegisterAbility failed - enemy null: {enemy == null}, abilityType: {abilityType}");
            return;
        }
        
        var abilities = GetCustomData(enemy, "bossAbilities") as List<BossAbilityType>;
        if (abilities == null)
        {
            abilities = new List<BossAbilityType>();
            SetCustomData(enemy, "bossAbilities", abilities);
            Debug.Log($"<color=green>[BossAbilityHandler] Created new abilities list for {enemy.enemyName}</color>");
        }
        
        if (!abilities.Contains(abilityType))
        {
            abilities.Add(abilityType);
            Debug.Log($"<color=green>[BossAbilityHandler] ✓ Registered {abilityType} on {enemy.enemyName} (Total abilities: {abilities.Count})</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>[BossAbilityHandler] {abilityType} already registered on {enemy.enemyName}</color>");
        }
    }
    
    /// <summary>
    /// Set custom data on enemy
    /// </summary>
    public static void SetCustomData(this Enemy enemy, string key, object value)
    {
        if (enemy == null) return;
        enemy.SetBossData(key, value);
    }
    
    /// <summary>
    /// Get custom data from enemy
    /// </summary>
    public static object GetCustomData(this Enemy enemy, string key)
    {
        if (enemy == null) return null;
        return enemy.GetBossData(key);
    }
    
    /// <summary>
    /// Increment cards hit counter for Cloak of Dusk
    /// </summary>
    private static void IncrementCardsHitCount(Enemy enemy)
    {
        int count = (int)(GetCustomData(enemy, "cardsHitCount") ?? 0);
        SetCustomData(enemy, "cardsHitCount", count + 1);
    }
    
    /// <summary>
    /// Get previous card played (for Judgment Loop)
    /// </summary>
    private static Card GetPreviousCard(Enemy enemy)
    {
        return GetCustomData(enemy, "previousCard") as Card;
    }
    
    /// <summary>
    /// Get all active enemy displays
    /// </summary>
    private static List<EnemyCombatDisplay> GetActiveEnemies()
    {
        var allDisplays = Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        return allDisplays.Where(d => d != null && d.gameObject.activeInHierarchy && d.GetCurrentEnemy() != null).ToList();
    }
    
    /// <summary>
    /// Find enemy display for a specific enemy
    /// </summary>
    private static EnemyCombatDisplay FindEnemyDisplay(Enemy enemy)
    {
        var displays = Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in displays)
        {
            if (display.GetCurrentEnemy() == enemy)
            {
                return display;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get current combat turn count
    /// </summary>
    private static int GetCombatTurnCount()
    {
        var combatManager = Object.FindFirstObjectByType<CombatManager>();
        if (combatManager != null)
        {
            // Assuming CombatManager has a turn counter
            // You may need to add this if it doesn't exist
            return 1; // Placeholder
        }
        return 1;
    }
}

