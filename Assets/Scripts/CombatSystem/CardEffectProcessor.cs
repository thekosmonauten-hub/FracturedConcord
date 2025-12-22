using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Damage breakdown by type for status effect calculations
/// </summary>
public struct DamageBreakdown
{
    public float physical;
    public float fire;
    public float cold;
    public float lightning;
    public float chaos;
    public float total;
    
    public DamageBreakdown(float phys, float f, float c, float l, float ch, float tot)
    {
        physical = phys;
        fire = f;
        cold = c;
        lightning = l;
        chaos = ch;
        total = tot;
    }
}

/// <summary>
/// Processes card effects and applies them to targets (enemies/player).
/// Handles damage calculation, guard, status effects, etc.
/// </summary>
public class CardEffectProcessor : MonoBehaviour
{
    public static CardEffectProcessor Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private CombatDisplayManager combatManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private CombatAnimationManager animationManager;
    
    [Header("Settings")]
    [SerializeField] private bool showDetailedLogs = true;
    
    // Effect system reference
    private CombatEffectManager effectManager;
    
    // Track temporary stat boosts for removal
    private class TemporaryStatBoostTracker
    {
        public Character character;
        public string statName;
        public float value;
        public int remainingTurns;
    }
    private List<TemporaryStatBoostTracker> activeStatBoosts = new List<TemporaryStatBoostTracker>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find references
        if (combatManager == null)
            combatManager = FindFirstObjectByType<CombatDisplayManager>();
        
        if (characterManager == null)
            characterManager = CharacterManager.Instance;
        
        if (animationManager == null)
            animationManager = CombatAnimationManager.Instance;
        
        // Find effect manager
        effectManager = FindFirstObjectByType<CombatEffectManager>();
    }
    
    /// <summary>
    /// Apply a card's effect to a target enemy.
    /// </summary>
    public void ApplyCardToEnemy(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition, bool isDelayed = false)
    {
        // Debug logs commented out to prevent memory leaks from string allocations
        // Uncomment only when debugging card application issues:
        // Debug.Log($"<color=magenta>╔══════════════════════════════════╗</color>");
        // Debug.Log($"<color=magenta>║ APPLYING CARD EFFECT DEBUG      ║</color>");
        // Debug.Log($"<color=magenta>╚══════════════════════════════════╝</color>");
        
        if (card == null)
        {
            Debug.LogError("✗ Card is NULL! Cannot apply!");
            return;
        }
        
        if (targetEnemy == null && !card.isAoE)
        {
            Debug.LogError("✗ Target enemy is NULL and card is not AoE! Cannot apply!");
            return;
        }
        
        // Debug.Log($"✓ Card: {card.cardName} (Type: {card.cardType})");
        // if (targetEnemy != null)
        // {
        //     Debug.Log($"✓ Target: {targetEnemy.enemyName}");
        //     Debug.Log($"✓ Target HP BEFORE: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}");
        // }
        // else if (card.isAoE)
        // {
        //     Debug.Log($"✓ AoE Target: All enemies");
        // }
        
        // if (showDetailedLogs)
        // {
        //     string targetName = targetEnemy != null ? targetEnemy.enemyName : "All enemies";
        //     Debug.Log($"<color=cyan>═══ Applying {card.cardName} to {targetName} ═══</color>");
        // }
        
        // Check if this is an AoE card
        if (card.isAoE)
        {
            // Debug log removed to prevent memory leaks:
            // Debug.Log($"<color=orange>⚡ AoE Card detected: {card.cardName} will hit all enemies!</color>");
            ApplyAoECard(card, player, targetScreenPosition, isDelayed);
        }
        else
        {
            // Single target card
            switch (card.cardType)
            {
                case CardType.Attack:
                    ApplyAttackCard(card, targetEnemy, player, targetScreenPosition, isDelayed);
                    break;
                
                case CardType.Guard:
                    ApplyGuardCard(card, player, isDelayed);
                    break;
                
                case CardType.Skill:
                    ApplySkillCard(card, targetEnemy, player, targetScreenPosition, isDelayed);
                    break;
                
                case CardType.Power:
                    ApplyPowerCard(card, player, isDelayed);
                    break;
                
                default:
                    Debug.LogWarning($"Unknown card type: {card.cardType}");
                    break;
            }
        }
    }
    
    /// <summary>
    /// Apply an AoE card - affects all enemies
    /// </summary>
    private void ApplyAoECard(Card card, Character player, Vector3 targetScreenPosition, bool isDelayed = false)
    {
        // Debug log removed to prevent memory leaks:
        // Debug.Log($"<color=orange>🎯 Applying AoE card: {card.cardName}</color>");
        
        // Get all active enemies and their display indices BEFORE applying damage
        // This prevents index mismatches when enemies are killed during AoE
        var combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatDisplayManager == null)
        {
            Debug.LogWarning("CombatDisplayManager not found for AoE card!");
            return;
        }
        
        // Get current active enemy displays and their indices
        var activeDisplays = combatDisplayManager.GetActiveEnemyDisplays();
        List<(Enemy enemy, int displayIndex)> validTargets = new List<(Enemy, int)>();
        
        // Debug log removed to prevent memory leaks:
        // Debug.Log($"<color=cyan>🔍 AoE Debug: Found {activeDisplays.Count} active displays</color>");
        
        for (int i = 0; i < activeDisplays.Count; i++)
        {
            var display = activeDisplays[i];
            if (display != null)
            {
                Debug.Log($"<color=cyan>  Display {i}: Active={display.gameObject.activeInHierarchy}</color>");
                if (display.gameObject.activeInHierarchy)
                {
                    var enemy = display.GetEnemy();
                    if (enemy != null)
                    {
                        Debug.Log($"<color=cyan>    Enemy: {enemy.enemyName}, HP: {enemy.currentHealth}/{enemy.maxHealth}</color>");
                        if (enemy.currentHealth > 0)
                        {
                            validTargets.Add((enemy, i));
                            Debug.Log($"<color=green>    ✓ Added to valid targets list</color>");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"<color=red>    ✗ Display has no enemy!</color>");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"<color=red>  Display {i} is NULL!</color>");
            }
        }
        
        if (validTargets.Count == 0)
        {
            Debug.LogWarning("No valid enemies found for AoE card!");
            return;
        }
        
        Debug.Log($"<color=orange>🎯 AoE will target {validTargets.Count} valid enemies from {activeDisplays.Count} displays</color>");
        
        // Apply damage to each valid target using their stable display indices
        // If aoeTargets <= 0, hit ALL enemies. Otherwise, hit up to aoeTargets
        int maxTargets = (card.aoeTargets <= 0) ? validTargets.Count : Mathf.Min(card.aoeTargets, validTargets.Count);
        Debug.Log($"<color=orange>🎯 AoE targeting {maxTargets} enemies (card.aoeTargets: {card.aoeTargets}, hit all: {card.aoeTargets <= 0})</color>");
        
        // Play Area effect for AoE card (plays once at targeted enemy's Default point)
        if (effectManager != null && validTargets.Count > 0)
        {
            // Get the targeted enemy transform (first enemy in the list, or use EnemyTargetingManager if available)
            Transform targetEnemyTransform = null;
            var targetingManager = EnemyTargetingManager.Instance;
            if (targetingManager != null)
            {
                int targetIndex = targetingManager.GetTargetedEnemyIndex();
                if (targetIndex >= 0 && targetIndex < activeDisplays.Count && activeDisplays[targetIndex] != null)
                {
                    targetEnemyTransform = activeDisplays[targetIndex].transform;
                }
            }
            
            // Fallback to first valid target if targeting manager didn't provide one
            if (targetEnemyTransform == null && validTargets.Count > 0)
            {
                var (firstEnemy, firstIndex) = validTargets[0];
                if (firstIndex >= 0 && firstIndex < activeDisplays.Count && activeDisplays[firstIndex] != null)
                {
                    targetEnemyTransform = activeDisplays[firstIndex].transform;
                }
            }
            
            if (targetEnemyTransform != null)
            {
                effectManager.PlayAreaEffectForCard(card, targetEnemyTransform, false);
            }
            else
            {
                Debug.LogWarning("[CardEffectProcessor] Could not find target enemy transform for Area effect!");
            }
        }
        
        // BATCH PROCESSING: Apply all AoE damage in one batch to prevent cascading defeat handlers
        // This prevents multiple simultaneous CheckWaveCompletion calls that can cause freezes
        
        // Mark AoE attack as in progress to defer wave completion checks
        combatDisplayManager.StartAoEAttack();
        
        // Check for momentum-based damage scaling ("per Momentum spent")
        float totalDamage = 0f;
        int momentumSpent = 0;
        
        if (player != null && MomentumEffectParser.HasPerMomentumSpent(card.description))
        {
            // Handle "per Momentum spent" damage
            string desc = card.description ?? "";
            
            // Spend momentum first
            int spendAmount = MomentumEffectParser.ParseSpendMomentum(desc);
            if (spendAmount == -1) // Spend all
            {
                momentumSpent = player.SpendAllMomentum();
            }
            else if (spendAmount > 0)
            {
                momentumSpent = player.SpendMomentum(spendAmount);
            }
            
            if (momentumSpent > 0)
            {
                // Calculate damage per momentum with attribute scaling
                // Pass card parameter so it can use card.baseDamage if description uses {damage} placeholder
                totalDamage = MomentumEffectParser.CalculatePerMomentumDamage(desc, momentumSpent, player, card);
                Debug.Log($"<color=cyan>[Momentum] Spent {momentumSpent} momentum, dealing {totalDamage:F1} total damage (per momentum scaling)</color>");
            }
            else
            {
                Debug.LogWarning($"[Momentum] {card.cardName} requires momentum but player has none!");
                combatDisplayManager.EndAoEAttack();
                return;
            }
        }
        else
        {
            // Standard damage calculation (AoE hits all enemies, so pass null for targetEnemy)
            totalDamage = (card.cardType == CardType.Attack || (card.cardType == CardType.Skill && card.baseDamage > 0))
                ? DamageCalculator.CalculateCardDamage(card, player, null, null)
                : 0f;
        }

        // Apply delayed card bonus: +25% damage for delayed attack cards
        if (isDelayed && totalDamage > 0f)
        {
            totalDamage *= 1.25f;
            Debug.Log($"<color=cyan>[Delayed Bonus] AoE attack card gains +25% damage: {totalDamage:F1}</color>");
        }

        if (totalDamage > 0f)
        {
            totalDamage = CombatDeckManager.ApplyDamageModifier(totalDamage);
        }
        
        Debug.Log($"<color=green>🎯 AoE will hit all {validTargets.Count} enemies (aoeTargets: {card.aoeTargets}, damage: {totalDamage})</color>");
        
        // Calculate damage breakdown for status effects (same for all targets)
        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
        
        // Apply all damage in batch
        for (int n = 0; n < maxTargets; n++)
        {
            var (enemy, displayIndex) = validTargets[n];
            
            // Double-check enemy is still alive before applying
            if (enemy == null || enemy.currentHealth <= 0)
            {
                Debug.Log($"<color=orange>⚠️ Enemy at index {displayIndex} already dead, skipping</color>");
                continue;
            }
            
            Debug.Log($"<color=yellow>💥 Dealing {totalDamage} damage to {enemy.enemyName} at display index {displayIndex} ({n+1}/{maxTargets})</color>");
            
            // Apply Vulnerability multiplier per enemy (AoE)
            float enemyDamage = totalDamage;
            try
            {
                var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                foreach (var d in enemyDisplays)
                {
                    if (d != null && d.GetCurrentEnemy() == enemy)
                    {
                        var statusManager = d.GetComponent<StatusEffectManager>();
                        if (statusManager != null)
                        {
                            // Use GetVulnerabilityDamageMultiplier() which returns 1.2f (20% more) and checks if consumed
                            float vulnMultiplier = statusManager.GetVulnerabilityDamageMultiplier();
                            if (vulnMultiplier > 1f)
                            {
                                Debug.Log($"  [Vulnerability] Applying multiplier to {enemy.enemyName}: x{vulnMultiplier:F2} (20% more damage)");
                                enemyDamage *= vulnMultiplier;
                            }
                            // Apply Bolster (less damage taken per stack: 2%, max 10 stacks)
                            float bolsterStacks = Mathf.Min(10f, statusManager.GetTotalMagnitude(StatusEffectType.Bolster));
                            if (bolsterStacks > 0f)
                            {
                                float lessMultiplier = Mathf.Clamp01(1f - (0.02f * bolsterStacks));
                                Debug.Log($"  Bolster stacks: {bolsterStacks}, less dmg multiplier: x{lessMultiplier:F2}");
                                enemyDamage *= lessMultiplier;
                            }
                        }
                        break;
                    }
                }
            }
            catch { /* safe guard */ }
            
            // Play visual effect before damage (AoE)
            // Note: If card has Area effect, PlayCardEffect will skip per-enemy effects
            // (Area effect is already played once at AoEAreaIndicator location)
            PlayCardEffect(card, enemy, displayIndex, true);
            
            // Apply damage with vulnerability multiplier
            combatDisplayManager.PlayerAttackEnemy(displayIndex, enemyDamage, card);
            
            // Consume Vulnerability after damage is dealt (AoE)
            try
            {
                var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                foreach (var d in enemyDisplays)
                {
                    if (d != null && d.GetCurrentEnemy() == enemy)
                    {
                        var statusManager = d.GetComponent<StatusEffectManager>();
                        if (statusManager != null)
                        {
                            statusManager.ConsumeVulnerability();
                        }
                        break;
                    }
                }
            }
            catch { /* safe guard */ }
            
            // Trigger embossing modifier event for damage dealt (AoE)
            // Note: PlayerAttackEnemy already triggers this for single-target, but for AoE we need to trigger it here
            // since PlayerAttackEnemy is called for each enemy in the loop
            Character playerCharacter = null;
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
            {
                playerCharacter = CharacterManager.Instance.GetCurrentCharacter();
            }
            if (card != null && playerCharacter != null && Dexiled.CombatSystem.Embossing.EmbossingModifierEventProcessor.Instance != null)
            {
                // Determine primary damage type from breakdown
                DamageType primaryDamageType = card.primaryDamageType;
                if (damageBreakdown.fire > 0 && damageBreakdown.fire >= damageBreakdown.physical) primaryDamageType = DamageType.Fire;
                else if (damageBreakdown.cold > 0 && damageBreakdown.cold >= damageBreakdown.physical) primaryDamageType = DamageType.Cold;
                else if (damageBreakdown.lightning > 0 && damageBreakdown.lightning >= damageBreakdown.physical) primaryDamageType = DamageType.Lightning;
                
                Dexiled.CombatSystem.Embossing.EmbossingModifierEventProcessor.Instance.OnDamageDealt(
                    card, playerCharacter, enemy, totalDamage, primaryDamageType
                );
                
                // Check if enemy was killed by this damage
                if (enemy != null && enemy.currentHealth <= 0)
                {
                    Dexiled.CombatSystem.Embossing.EmbossingModifierEventProcessor.Instance.OnEnemyKilled(
                        card, playerCharacter, enemy
                    );
                }
            }
            
            // Apply automatic status effects based on damage types
            ApplyAutomaticStatusEffects(enemy, damageBreakdown, card);
            
            // Process CardEffects that target enemies (e.g., ApplyStatus effects with targetsAllEnemies)
            // This must be done AFTER damage calculation so status effects can use the damage breakdown
            if (card.effects != null && card.effects.Count > 0)
            {
                ProcessCardEffectsForEnemy(card, enemy, player, totalDamage);
            }
            
            Debug.Log($"<color=yellow>  After damage: {enemy.enemyName} HP is now {enemy.currentHealth}/{enemy.maxHealth}</color>");
        }
        
        // After all damage is applied, check for defeated enemies once
        // This prevents multiple CheckWaveCompletion calls from causing issues
        Debug.Log($"<color=green>✅ AoE batch complete. Deferring wave completion check...</color>");
        StartCoroutine(DelayedWaveCompletionCheck(combatDisplayManager));
        
        Debug.Log($"<color=green>✅ AoE card {card.cardName} completed targeting {maxTargets} enemies</color>");
    }

    /// <summary>
    /// Delayed wave completion check to avoid race conditions during AoE
    /// </summary>
    private IEnumerator DelayedWaveCompletionCheck(CombatDisplayManager displayManager)
    {
        // Wait for all defeat handlers to complete
        yield return null;
        yield return null;
        
        // Now check wave completion
        if (displayManager != null)
        {
            displayManager.EndAoEAttack();
        }
    }

    private List<Enemy> TryGetActiveEnemiesList()
    {
        var cdm = combatManager != null ? combatManager : FindFirstObjectByType<CombatDisplayManager>();
        if (cdm == null) return null;
        var field = typeof(CombatDisplayManager).GetField("activeEnemies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null) return null;
        return field.GetValue(cdm) as List<Enemy>;
    }

    private int GetActiveEnemyCount(CombatDisplayManager cdm)
    {
        var field = typeof(CombatDisplayManager).GetField("activeEnemies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var list = field.GetValue(cdm) as List<Enemy>;
            if (list != null) return list.Count;
        }
        return 0;
    }

    private CombatManager.EnemyRow GetRowForActiveIndex(int zeroBasedIndex, int totalEnemies)
    {
        if (zeroBasedIndex < 0) return CombatManager.EnemyRow.Both;
        return CombatManager.GetEnemyRow(zeroBasedIndex, totalEnemies);
    }

    private int FindActiveEnemyIndex(Enemy enemy)
    {
        if (enemy == null) return -1;
        // Try activeEnemies list first (order used by PlayerAttackEnemy)
        var cdm = combatManager != null ? combatManager : FindFirstObjectByType<CombatDisplayManager>();
        if (cdm != null)
        {
            var field = typeof(CombatDisplayManager).GetField("activeEnemies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var list = field.GetValue(cdm) as List<Enemy>;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == enemy) return i;
                    }
                }
            }
        }

        // Fallback: enemyDisplays order
        if (combatManager != null && combatManager.GetActiveEnemyDisplays() != null)
        {
            for (int i = 0; i < combatManager.GetActiveEnemyDisplays().Count; i++)
            {
                var d = combatManager.GetActiveEnemyDisplays()[i];
                if (d != null && d.GetCurrentEnemy() == enemy) return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Get all active enemies in the combat
    /// </summary>
    private List<Enemy> GetAllActiveEnemies()
    {
        List<Enemy> activeEnemies = new List<Enemy>();
        
        // Try to get enemies from CombatDisplayManager
        var combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatDisplayManager != null)
        {
            // Access the activeEnemies list if it's public
            var activeEnemiesField = typeof(CombatDisplayManager).GetField("activeEnemies", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (activeEnemiesField != null)
            {
                var enemies = activeEnemiesField.GetValue(combatDisplayManager) as List<Enemy>;
                if (enemies != null)
                {
                    foreach (var enemy in enemies)
                    {
                        if (enemy != null && enemy.currentHealth > 0)
                        {
                            activeEnemies.Add(enemy);
                        }
                    }
                }
            }
        }
        
        // Fallback: find enemies through EnemyCombatDisplay components
        if (activeEnemies.Count == 0)
        {
            // Find EnemyCombatDisplay components which contain Enemy data
            EnemyCombatDisplay[] enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var enemyDisplay in enemyDisplays)
            {
                if (enemyDisplay != null && enemyDisplay.gameObject.activeInHierarchy)
                {
                    // Get the enemy data from the display component
                    Enemy enemy = enemyDisplay.GetCurrentEnemy();
                    if (enemy != null && enemy.currentHealth > 0)
                    {
                        activeEnemies.Add(enemy);
                    }
                }
            }
        }
        
        return activeEnemies;
    }
    
    /// <summary>
    /// Get screen position for an enemy (for AoE targeting)
    /// </summary>
    private Vector3 GetEnemyScreenPosition(Enemy enemy, int index)
    {
        // Try to find the enemy's display component
        var enemyDisplay = FindFirstObjectByType<EnemyCombatDisplay>();
        if (enemyDisplay != null)
        {
            // Calculate a position based on the enemy index
            // This spreads out the target positions for visual variety
            Vector3 basePosition = enemyDisplay.transform.position;
            basePosition.x += (index - 1) * 50f; // Spread horizontally
            return basePosition;
        }
        
        // Fallback: use the original target position
        return Vector3.zero;
    }

    private void ApplyStatusEffectToEnemy(Enemy targetEnemy, StatusEffect effect)
    {
        if (targetEnemy == null || effect == null) return;

        // Apply status effect duration modifier from character stats (before applying effect)
        // Duration needs to be calculated before the status effect is applied to the enemy (Rounding up to closest whole)
        // Get player character from CharacterManager
        Character playerCharacter = null;
        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            playerCharacter = CharacterManager.Instance.GetCurrentCharacter();
        }
        
        if (playerCharacter != null && effect.duration > 0) // Only apply to non-permanent effects (duration > 0)
        {
            var statsData = new CharacterStatsData(playerCharacter);
            float statusEffectDuration = statsData.statusEffectDuration;
            
            if (statusEffectDuration > 0f)
            {
                // Apply as increased modifier: finalDuration = baseDuration * (1 + statusEffectDuration / 100)
                float durationMultiplier = 1f + (statusEffectDuration / 100f);
                float modifiedDuration = effect.duration * durationMultiplier;
                
                // Round up to nearest whole number
                effect.duration = Mathf.CeilToInt(modifiedDuration);
                effect.timeRemaining = effect.duration; // Update time remaining to match new duration
                
                Debug.Log($"[Status Effect Duration] Applied {statusEffectDuration}% duration modifier. Base={effect.duration / durationMultiplier:F1}, Final={effect.duration}");
            }
        }

        var displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in displays)
        {
            if (display != null && display.GetCurrentEnemy() == targetEnemy)
            {
                var statusManager = display.GetComponent<StatusEffectManager>();
                if (statusManager != null)
                {
                    statusManager.AddStatusEffect(effect);
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// Calculate damage breakdown by type for a card
    /// This is needed for status effects that scale with specific damage types
    /// </summary>
    private DamageBreakdown CalculateDamageBreakdown(Card card, Character player, float totalDamage)
    {
        // For now, we'll use a simplified approach:
        // If card has a primary damage type, assign all damage to that type
        // TODO: In the future, this could be enhanced to track actual damage by type from modifiers
        
        DamageBreakdown breakdown = new DamageBreakdown(0f, 0f, 0f, 0f, 0f, totalDamage);
        
        if (card == null) return breakdown;
        
        // Assign damage based on primary damage type
        // This is a simplification - ideally we'd track actual damage by type
        switch (card.primaryDamageType)
        {
            case DamageType.Physical:
                breakdown.physical = totalDamage;
                break;
            case DamageType.Fire:
                breakdown.fire = totalDamage;
                break;
            case DamageType.Cold:
                breakdown.cold = totalDamage;
                break;
            case DamageType.Lightning:
                breakdown.lightning = totalDamage;
                break;
            case DamageType.Chaos:
                breakdown.chaos = totalDamage;
                break;
        }
        
        return breakdown;
    }
    
    /// <summary>
    /// Apply automatic status effects based on damage types
    /// - Cold damage always inflicts Chilled (and Freeze if threshold met)
    /// - Fire damage can inflict Ignite
    /// - Lightning damage can inflict Shocked
    /// - Physical damage from attacks can inflict Bleeding
    /// </summary>
    private void ApplyAutomaticStatusEffects(Enemy targetEnemy, DamageBreakdown damageBreakdown, Card card)
    {
        if (targetEnemy == null) return;
        
        // Cold damage always inflicts Chilled
        if (damageBreakdown.cold > 0f)
        {
            StatusEffect chilledEffect = StatusEffectFactory.CreateChilled(damageBreakdown.cold, 2);
            ApplyStatusEffectToEnemy(targetEnemy, chilledEffect);
            Debug.Log($"[Auto Status] Applied Chilled from {damageBreakdown.cold} cold damage");
            
            // Check for Freeze: cold damage >= 10% of enemy max HP
            float coldDamagePercent = (damageBreakdown.cold / targetEnemy.maxHealth) * 100f;
            if (coldDamagePercent >= 10f)
            {
                StatusEffect frozenEffect = StatusEffectFactory.CreateFrozen(damageBreakdown.cold, targetEnemy.maxHealth);
                ApplyStatusEffectToEnemy(targetEnemy, frozenEffect);
                Debug.Log($"[Auto Status] Applied Frozen from {damageBreakdown.cold} cold damage ({coldDamagePercent:F1}% of max HP)");
            }
        }
        
        // Fire damage can inflict Ignite
        if (damageBreakdown.fire > 0f)
        {
            StatusEffect igniteEffect = StatusEffectFactory.CreateIgnite(damageBreakdown.fire, 4);
            ApplyStatusEffectToEnemy(targetEnemy, igniteEffect);
            Debug.Log($"[Auto Status] Applied Ignite from {damageBreakdown.fire} fire damage");
        }
        
        // Lightning damage can inflict Shocked
        if (damageBreakdown.lightning > 0f)
        {
            StatusEffect shockedEffect = StatusEffectFactory.CreateShocked(damageBreakdown.lightning, 2);
            ApplyStatusEffectToEnemy(targetEnemy, shockedEffect);
            Debug.Log($"[Auto Status] Applied Shocked from {damageBreakdown.lightning} lightning damage");
        }
        
        // Physical damage from attacks can inflict Bleeding (with chance check)
        if (damageBreakdown.physical > 0f && card != null && card.cardType == CardType.Attack)
        {
            // Get player character to check bleed chance
            Character playerCharacter = null;
            if (characterManager != null && characterManager.HasCharacter())
            {
                playerCharacter = characterManager.GetCurrentCharacter();
            }
            
            if (playerCharacter != null)
            {
                var statsData = new CharacterStatsData(playerCharacter);
                float bleedChance = statsData.chanceToBleed;
                
                // Apply ailment application chance increased modifier
                bleedChance += statsData.ailmentApplicationChanceIncreased;
                
                // Roll chance (0-100)
                float roll = UnityEngine.Random.Range(0f, 100f);
                bool shouldBleed = roll < bleedChance;
                
                if (shouldBleed)
                {
                    StatusEffect bleedEffect = StatusEffectFactory.CreateBleeding(damageBreakdown.physical, 5);
                    ApplyStatusEffectToEnemy(targetEnemy, bleedEffect);
                    Debug.Log($"[Auto Status] Applied Bleeding from {damageBreakdown.physical} physical attack damage (Chance: {bleedChance:F1}%, Roll: {roll:F1}%)");
                }
                else
                {
                    Debug.Log($"[Auto Status] Bleed failed: Chance={bleedChance:F1}%, Roll={roll:F1}%");
                }
            }
            else
            {
                // Fallback: if no player character found, don't apply bleed
                Debug.LogWarning("[Auto Status] Cannot check bleed chance: Player character not found");
            }
        }
        
        // Chaos damage can inflict Poison if card has "Poison" tag
        if (damageBreakdown.chaos > 0f && card != null && card.tags != null && card.tags.Contains("Poison"))
        {
            // Poison stacks independently - apply 3 stacks for Chaos Bolt
            // Each stack is 30% of (physical + chaos) damage per turn for 3 turns
            int poisonStacks = 3; // Chaos Bolt applies 3 stacks
            
            for (int i = 0; i < poisonStacks; i++)
            {
                StatusEffect poisonEffect = StatusEffectFactory.CreatePoison(damageBreakdown.physical, damageBreakdown.chaos, 3);
                ApplyStatusEffectToEnemy(targetEnemy, poisonEffect);
            }
            
            float poisonDamagePerStack = StatusEffectManager.CalculatePoisonMagnitude(damageBreakdown.physical, damageBreakdown.chaos);
            Debug.Log($"[Auto Status] Applied {poisonStacks} Poison stacks ({poisonDamagePerStack} damage/turn each, {poisonDamagePerStack * poisonStacks} total/turn) from {damageBreakdown.chaos} chaos damage");
        }
    }
    
    /// <summary>
    /// Apply an attack card - deal damage to enemy.
    /// </summary>
    private void ApplyAttackCard(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition, bool isDelayed = false)
    {
        Debug.Log($"<color=yellow>→ Attack card detected!</color>");
        
        // Process momentum threshold effects BEFORE damage calculation
        MomentumThresholdResult momentumEffects = ProcessMomentumThresholdEffects(card, player, CardType.Attack);
        
        // Check if card should be AoE based on description ("to all enemies")
        // This handles cases where isAoE is false but description says "to all enemies"
        string desc = card.description ?? "";
        bool shouldBeAoE = card.isAoE || 
                          desc.Contains("to all enemies", System.StringComparison.OrdinalIgnoreCase) ||
                          desc.Contains("all enemies", System.StringComparison.OrdinalIgnoreCase);
        
        // If card has "per Momentum spent" AND "to all enemies", route to AoE path
        // ApplyAoECard already handles "per Momentum spent" damage calculation
        if (shouldBeAoE && player != null && MomentumEffectParser.HasPerMomentumSpent(desc))
        {
            Debug.Log($"<color=yellow>[Momentum + AoE] {card.cardName} has 'per Momentum spent' and 'to all enemies' - routing to AoE path</color>");
            ApplyAoECard(card, player, targetScreenPosition, isDelayed);
            
            // Process generic on-play effects (e.g., Draw, Momentum gain)
            ApplyOnPlayEffects(card, player, momentumEffects);
            
            // Apply momentum threshold effects
            if (momentumEffects != null)
            {
                ApplyMomentumThresholdResult(momentumEffects, card, player, targetEnemy);
            }
            return; // AoE path handles everything
        }
        
        // Check for momentum-based damage scaling ("per Momentum spent")
        float totalDamage = 0f;
        int momentumSpent = 0;
        
        if (player != null && MomentumEffectParser.HasPerMomentumSpent(desc))
        {
            // Handle "per Momentum spent" damage (single target)
            // Spend momentum first
            int spendAmount = MomentumEffectParser.ParseSpendMomentum(desc);
            if (spendAmount == -1) // Spend all
            {
                momentumSpent = player.SpendAllMomentum();
            }
            else if (spendAmount > 0)
            {
                momentumSpent = player.SpendMomentum(spendAmount);
            }
            
            if (momentumSpent > 0)
            {
                // Calculate damage per momentum with attribute scaling
                // Pass card parameter so it can use card.baseDamage if description uses {damage} placeholder
                totalDamage = MomentumEffectParser.CalculatePerMomentumDamage(desc, momentumSpent, player, card);
                Debug.Log($"<color=cyan>[Momentum] Spent {momentumSpent} momentum, dealing {totalDamage:F1} total damage (per momentum scaling)</color>");
            }
            else
            {
                Debug.LogWarning($"[Momentum] {card.cardName} requires momentum but player has none!");
                return;
            }
        }
        else
        {
            // Standard damage calculation (pass targetEnemy for conditional damage modifiers)
            totalDamage = DamageCalculator.CalculateCardDamage(card, player, null, targetEnemy);
        }
        
        // Multi-hit attack: Check card's isMultiHit property EARLY (before momentum effects that might return early)
        bool isMultiHit = card.isMultiHit;
        int hitCount = isMultiHit ? Mathf.Max(1, card.hitCount) : 1; // Ensure at least 1 hit
        
        // Debug logs removed to prevent memory leaks from string allocations
        // Uncomment only when debugging multi-hit issues:
        // Debug.Log($"[Multi-Hit Debug] Card: {card.cardName}, isMultiHit: {card.isMultiHit}, hitCount: {card.hitCount}, calculated hits: {hitCount}");
        // Debug.Log($"[Multi-Hit Debug] Card type: {card.cardType}, baseDamage: {card.baseDamage}");
        
        // if (isMultiHit)
        // {
        //     Debug.Log($"<color=cyan>[Multi-Hit] {card.cardName} will hit {hitCount} times!</color>");
        // }
        
        // Apply momentum threshold effects that modify damage/targeting
        // NOTE: Multi-hit cards should still apply multi-hit even if momentum converts to AoE/random targets
        if (momentumEffects != null)
        {
            // Convert to AoE if threshold met
            if (momentumEffects.convertToAoE)
            {
                // For multi-hit AoE, we need to handle it specially
                if (isMultiHit && hitCount > 1)
                {
                    Debug.Log($"<color=yellow>[Multi-Hit + AoE] {card.cardName} is multi-hit AND AoE - applying {hitCount} hits to all enemies</color>");
                    // Apply multi-hit AoE (each enemy gets hit multiple times)
                    StartCoroutine(ApplyMultiHitAoE(card, player, targetScreenPosition, hitCount, totalDamage, isDelayed));
                }
                else
                {
                    // Convert single-target attack to AoE
                    ApplyAoECard(card, player, targetScreenPosition, isDelayed);
                }
                // Apply other momentum effects
                ApplyMomentumThresholdResult(momentumEffects, card, player, targetEnemy);
                return; // AoE path handles everything
            }
            
            // Random targets instead of single target
            if (momentumEffects.randomTargetCount > 0)
            {
                // For multi-hit random targets, apply multi-hit to each random target
                var allEnemies = GetAllActiveEnemies();
                if (allEnemies.Count > 0)
                {
                    // Shuffle and pick random targets
                    var shuffled = new List<Enemy>(allEnemies);
                    for (int i = 0; i < shuffled.Count; i++)
                    {
                        var temp = shuffled[i];
                        int randomIndex = Random.Range(i, shuffled.Count);
                        shuffled[i] = shuffled[randomIndex];
                        shuffled[randomIndex] = temp;
                    }
                    
                    int targetsToHit = Mathf.Min(momentumEffects.randomTargetCount, shuffled.Count);
                    
                    if (isMultiHit && hitCount > 1)
                    {
                        Debug.Log($"<color=yellow>[Multi-Hit + Random] {card.cardName} is multi-hit AND random targets - applying {hitCount} hits to {targetsToHit} random enemies</color>");
                        // Apply multi-hit to each random target
                        StartCoroutine(ApplyMultiHitRandomTargets(shuffled, targetsToHit, totalDamage, hitCount, card, targetScreenPosition));
                    }
                    else
                    {
                        // Single hit to random targets
                        for (int i = 0; i < targetsToHit; i++)
                        {
                            Enemy randomEnemy = shuffled[i];
                            int enemyIdx = FindActiveEnemyIndex(randomEnemy);
                            if (enemyIdx >= 0 && combatManager != null)
                            {
                                combatManager.PlayerAttackEnemy(enemyIdx, totalDamage, card);
                            }
                        }
                    }
                    Debug.Log($"<color=cyan>[Momentum Effect] Hit {targetsToHit} random enemies instead of single target</color>");
                    
                    // Apply other momentum effects
                    ApplyMomentumThresholdResult(momentumEffects, card, player, targetEnemy);
                    return;
                }
            }
        }
        
        // Apply delayed card bonus: +25% damage for delayed attack cards
        if (isDelayed)
        {
            totalDamage *= 1.25f;
            Debug.Log($"<color=cyan>[Delayed Bonus] Attack card gains +25% damage: {totalDamage:F1}</color>");
        }
        
        // THIEF CARD EFFECTS: Check for prepared card interactions and dual wield
        CardDataExtended extendedCard = GetCardDataExtended(card);
        if (extendedCard != null && player != null)
        {
            // Check for prepared card count bonuses (Ambush, Poisoned Blade)
            int preparedCount = ThiefCardEffects.GetPreparedCardCount();
            if (preparedCount > 0)
            {
                // Use the new configurable fields from CardDataExtended
                // Ambush: +preparedCardDamageBase (+preparedCardDamageScaling) damage per prepared card
                if (extendedCard.preparedCardDamageBase > 0f || 
                    (extendedCard.preparedCardDamageScaling != null && 
                     (extendedCard.preparedCardDamageScaling.strengthDivisor > 0f || 
                      extendedCard.preparedCardDamageScaling.dexterityDivisor > 0f || 
                      extendedCard.preparedCardDamageScaling.intelligenceDivisor > 0f ||
                      extendedCard.preparedCardDamageScaling.strengthScaling > 0f ||
                      extendedCard.preparedCardDamageScaling.dexterityScaling > 0f ||
                      extendedCard.preparedCardDamageScaling.intelligenceScaling > 0f)))
                {
                    // Check if dual wielding for enhanced effect
                    bool isDualWielding = ThiefCardEffects.IsDualWielding(player);
                    float bonusPerCard = extendedCard.preparedCardDamageBase;
                    
                    // Add attribute scaling
                    if (extendedCard.preparedCardDamageScaling != null)
                    {
                        bonusPerCard += extendedCard.preparedCardDamageScaling.CalculateScalingBonus(player);
                    }
                    
                    // Dual wield doubles the base bonus (but not the scaling)
                    if (isDualWielding && extendedCard.preparedCardDamageBase > 0f)
                    {
                        bonusPerCard = (extendedCard.preparedCardDamageBase * 2f) + 
                                       (extendedCard.preparedCardDamageScaling != null ? extendedCard.preparedCardDamageScaling.CalculateScalingBonus(player) : 0f);
                    }
                    
                    float bonusDamage = bonusPerCard * preparedCount;
                    totalDamage += bonusDamage;
                    Debug.Log($"<color=cyan>[Thief] {card.cardName} gains +{bonusDamage:F1} damage from {preparedCount} prepared cards (base: {extendedCard.preparedCardDamageBase}, scaling: {extendedCard.preparedCardDamageScaling?.CalculateScalingBonus(player) ?? 0f}, dual wield: {isDualWielding})</color>");
                }
                
                // Poisoned Blade: Apply +preparedCardPoisonBase Poison per prepared card
                if (extendedCard.preparedCardPoisonBase > 0 && targetEnemy != null)
                {
                    int totalPoisonStacks = extendedCard.preparedCardPoisonBase * preparedCount;
                    
                    // Find enemy's StatusEffectManager via EnemyCombatDisplay
                    var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                    foreach (var display in enemyDisplays)
                    {
                        if (display != null && display.GetCurrentEnemy() == targetEnemy)
                        {
                            var statusMgr = display.GetComponent<StatusEffectManager>();
                            if (statusMgr != null)
                            {
                                // Calculate damage for poison (use card's damage breakdown)
                                DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
                                float physDmg = damageBreakdown.physical > 0f ? damageBreakdown.physical : 10f; // Fallback
                                float chaosDmg = damageBreakdown.chaos > 0f ? damageBreakdown.chaos : 0f;
                                
                                // Apply poison for each prepared card (each is a separate stack)
                                for (int i = 0; i < totalPoisonStacks; i++)
                                {
                                    StatusEffect poisonEffect = StatusEffectFactory.CreatePoison(physDmg, chaosDmg, 3);
                                    statusMgr.AddStatusEffect(poisonEffect);
                                }
                                Debug.Log($"<color=green>[Thief] Poisoned Blade applied {totalPoisonStacks} Poison stacks from {preparedCount} prepared cards (base: {extendedCard.preparedCardPoisonBase} per card)</color>");
                            }
                            break;
                        }
                    }
                }
            }
            
            // Perfect Strike: Consume all prepared cards for bonus damage
            if (card.cardName.Contains("Perfect Strike") || card.description.Contains("Consume all prepared"))
            {
                int consumedCount = ThiefCardEffects.ConsumeAllPreparedCards(player);
                if (consumedCount > 0)
                {
                    bool isDualWielding = ThiefCardEffects.IsDualWielding(player);
                    float bonusPerCard = isDualWielding ? 4f : 2f; // Dual: +4, Normal: +2
                    
                    // Parse dexterity scaling from description
                    float dexDivisor = ParseDexterityDivisor(card.description);
                    float dexBonus = dexDivisor > 0 ? player.dexterity / dexDivisor : 0f;
                    
                    float bonusDamage = (bonusPerCard + dexBonus) * consumedCount;
                    totalDamage += bonusDamage;
                    // Debug log removed to prevent memory leaks - uncomment only when debugging:
                    // Debug.Log($"<color=orange>[Thief] Perfect Strike consumed {consumedCount} prepared cards, gained +{bonusDamage:F1} damage (dual wield: {isDualWielding})</color>");
                }
            }
            
            // Process dual wield effects
            if (!string.IsNullOrEmpty(extendedCard.dualWieldEffect) && ThiefCardEffects.IsDualWielding(player))
            {
                ThiefCardEffects.ProcessDualWieldEffect(extendedCard.dualWieldEffect, card, player, targetEnemy);
            }
        }
        
        // Apply CardEffects that target enemies (e.g., ApplyStatus effects)
        // This must be done AFTER damage calculation so status effects can use the damage breakdown
        if (targetEnemy != null && card.effects != null && card.effects.Count > 0)
        {
            ProcessCardEffectsForEnemy(card, targetEnemy, player, totalDamage);
        }
        
        // Apply charge modifiers to damage
        totalDamage = CombatDeckManager.ApplyDamageModifier(totalDamage); // CardDataExtended not available here, but damage modifier doesn't need it

        // Prefer CombatDisplayManager routing if we can resolve the enemy index
        int idx = FindActiveEnemyIndex(targetEnemy);
        if (idx >= 0 && combatManager != null)
        {
            // For multi-hit attacks, use coroutine to space out hits with animations
            if (isMultiHit && hitCount > 1)
            {
                StartCoroutine(ApplyMultiHitAttack(combatManager, idx, totalDamage, hitCount, card, targetScreenPosition));
            }
            else
            {
                // Play visual effect before damage
                PlayCardEffect(card, targetEnemy, idx, false);
                
                // Single hit - use normal path
                // Note: combatManager is CombatDisplayManager which has PlayerAttackEnemy
                combatManager.PlayerAttackEnemy(idx, totalDamage, card);
                
                // Trigger nudge animation for single hit
                var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplay != null)
                {
                    playerDisplay.TriggerAttackNudge();
                }
            }
            
            // Gain Aggression charges from Attack cards (affected by attackSpeed and aggressionGainIncreased)
            if (player != null && StackSystem.Instance != null)
            {
                var statsData = new CharacterStatsData(player);
                float baseGain = 1f; // Base: 1 Aggression charge per Attack card
                float speedMultiplier = 1f + (statsData.attackSpeed / 100f);
                float gainMultiplier = 1f + (statsData.aggressionGainIncreased / 100f);
                float finalGain = baseGain * speedMultiplier * gainMultiplier;
                int aggressionGain = Mathf.RoundToInt(finalGain);
                
                if (aggressionGain > 0)
                {
                    StackSystem.Instance.AddStacks(StackType.Aggression, aggressionGain);
                    Debug.Log($"[Aggression] Gained {aggressionGain} Aggression charge(s) from {card.cardName} (Attack Speed: {statsData.attackSpeed}%, Aggression Gain: {statsData.aggressionGainIncreased}%)");
                }
            }
            
            // Process generic on-play effects (e.g., Draw, Momentum gain) BEFORE applying momentum threshold effects
            // NOTE: momentumEffects was already processed at the start of this method (line 445)
            // IMPORTANT: This must be called BEFORE the early return so card effects (like GainMomentum) are processed
            ApplyOnPlayEffects(card, player, momentumEffects);
            
            // Apply momentum threshold effects after damage
            if (momentumEffects != null)
            {
                ApplyMomentumThresholdResult(momentumEffects, card, player, targetEnemy);
            }
            return;
        }

        // Apply Vulnerability multiplier using proper method (20% more damage, consumed after one instance)
        try
        {
            var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var d in enemyDisplays)
            {
                if (d != null && d.GetCurrentEnemy() == targetEnemy)
                {
                    var statusManager = d.GetComponent<StatusEffectManager>();
                    if (statusManager != null)
                    {
                        // Use GetVulnerabilityDamageMultiplier() which returns 1.2f (20% more) and checks if consumed
                        float vulnMultiplier = statusManager.GetVulnerabilityDamageMultiplier();
                        if (vulnMultiplier > 1f)
                        {
                            Debug.Log($"  [Vulnerability] Applying multiplier: x{vulnMultiplier:F2} (20% more damage)");
                            totalDamage *= vulnMultiplier;
                        }
                        // Apply Bolster (less damage taken per stack: 2%, max 10 stacks)
                        float bolsterStacks = Mathf.Min(10f, statusManager.GetTotalMagnitude(StatusEffectType.Bolster));
                        if (bolsterStacks > 0f)
                        {
                            float lessMultiplier = Mathf.Clamp01(1f - (0.02f * bolsterStacks));
                            Debug.Log($"  Bolster stacks: {bolsterStacks}, less dmg multiplier: x{lessMultiplier:F2}");
                            totalDamage *= lessMultiplier;
                        }
                    }
                    break;
                }
            }
        }
        catch { /* safe guard */ }
        
        Debug.Log($"  Base damage: {card.baseDamage}");
        Debug.Log($"  Total calculated damage: {totalDamage}");
        
        // Note: isMultiHit and hitCount are already set above (before early return check)
        // If we reach here, the early return didn't happen, so use the fallback path with multi-hit support
        
        // Apply damage to enemy (with charge modifier: ignore guard/armor)
        bool ignoreGuardArmor = CombatDeckManager.ShouldIgnoreGuardArmor();
        
        // Determine if this is a projectile card (used in multiple places)
        bool isProjectile = IsProjectileCard(card);
        
        // For multi-hit in fallback path, use coroutine to space out hits
        if (isMultiHit && hitCount > 1)
        {
            StartCoroutine(ApplyMultiHitFallback(targetEnemy, totalDamage, hitCount, ignoreGuardArmor, card, targetScreenPosition));
        }
        else
        {
            
            // Play visual effect before damage (fallback path)
            int fallbackIdx = FindActiveEnemyIndex(targetEnemy);
            
            // Create callback for projectile cards that applies damage when projectile hits
            System.Action onProjectileHit = null;
            if (isProjectile)
            {
                // Store values needed for damage application
                float damageToApply = totalDamage;
                bool ignoreGuard = ignoreGuardArmor;
                Vector3 damagePos = targetScreenPosition;
                
                onProjectileHit = () => {
                    ApplyProjectileDamage(card, targetEnemy, damageToApply, ignoreGuard, damagePos, player);
                };
                
                PlayCardEffect(card, targetEnemy, fallbackIdx, false, totalDamage, targetScreenPosition, false, onProjectileHit);
            }
            else
            {
                // Non-projectile: apply damage immediately
                PlayCardEffect(card, targetEnemy, fallbackIdx, false);
                
            // Single hit - apply damage directly
            targetEnemy.TakeDamage(totalDamage, ignoreGuardArmor);
            
            Debug.Log($"<color=red>  ⚔️ Dealt {totalDamage:F0} damage to {targetEnemy.enemyName}</color>");
            Debug.Log($"<color=red>  💔 Target HP AFTER: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}</color>");
            
            // Consume Vulnerability after damage is dealt
            try
            {
                var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                foreach (var d in enemyDisplays)
                {
                    if (d != null && d.GetCurrentEnemy() == targetEnemy)
                    {
                        var statusManager = d.GetComponent<StatusEffectManager>();
                        if (statusManager != null)
                        {
                            statusManager.ConsumeVulnerability();
                        }
                        break;
                    }
                }
            }
            catch { /* safe guard */ }
            
            // Calculate damage breakdown for status effects
            DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
            
            // Apply automatic status effects based on damage types
            ApplyAutomaticStatusEffects(targetEnemy, damageBreakdown, card);
            
                // Show damage number immediately for non-projectile cards
            if (animationManager != null)
            {
                DamageNumberType damageNumberType = ConvertDamageType(card.primaryDamageType);
                animationManager.ShowDamageNumber(totalDamage, targetScreenPosition, damageNumberType);
            }
            
            // Trigger nudge animation
            var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                playerDisplay.TriggerAttackNudge();
            }
            
            // Update enemy display
            UpdateEnemyDisplay(targetEnemy);
            }
        }
        
        // Apply guard if this attack grants any
        if (player != null && card.baseGuard > 0f)
        {
            float guardAmount = CalculateGuard(card, player);
            if (guardAmount > 0f)
            {
                player.AddGuard(guardAmount);
                Debug.Log($"  🛡️ Player gained {guardAmount:F0} guard from {card.cardName} (Total: {player.currentGuard}/{player.maxHealth})");
                PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplay != null)
                {
                    playerDisplay.UpdateGuardDisplay();
                }
            }
        }
        
        // Process generic on-play effects (e.g., Draw, Momentum gain)
        // NOTE: momentumEffects was already processed at the start of this method (line 445)
        ApplyOnPlayEffects(card, player, momentumEffects);
        
        // Apply momentum threshold effects (draw cards, stat boosts, etc.)
        // Note: Additional momentum is handled in ApplyOnPlayEffects
        if (momentumEffects != null)
        {
            // Don't apply additional momentum here - it's handled in ApplyOnPlayEffects
            var effectsToApply = new MomentumThresholdResult
            {
                drawCards = momentumEffects.drawCards,
                tempStrength = momentumEffects.tempStrength,
                tempDexterity = momentumEffects.tempDexterity,
                tempIntelligence = momentumEffects.tempIntelligence,
                energyGain = momentumEffects.energyGain,
                applyBleed = momentumEffects.applyBleed,
                doubleNextAttack = momentumEffects.doubleNextAttack,
                triggerAdrenalineBurst = momentumEffects.triggerAdrenalineBurst
            };
            ApplyMomentumThresholdResult(effectsToApply, card, player, targetEnemy);
        }
        
        // NEW: Hook - apply structured combo ailment if present
        // Note: For projectile cards, this is handled in ApplyProjectileDamage when the projectile hits
        // For non-projectile cards, apply immediately
        if (!isProjectile && card.comboAilmentId != AilmentId.None)
        {
            switch (card.comboAilmentId)
            {
                case AilmentId.Crumble:
                    if (card.primaryDamageType == DamageType.Physical && card.comboAilmentPortion > 0f)
                    {
                        // Find the display for the specific target enemy to apply Crumble correctly
                        var displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                        foreach (var d in displays)
                        {
                            if (d != null && d.GetCurrentEnemy() == targetEnemy)
                            {
                                var statusManager = d.GetComponent<StatusEffectManager>();
                                if (statusManager != null)
                                {
                                    int dur = card.comboAilmentDuration > 0 ? card.comboAilmentDuration : 5;
                                    float stored = totalDamage * card.comboAilmentPortion;
                                    statusManager.ApplyOrStackCrumble(stored, dur);
                                    Debug.Log($"[Crumble] Stored {stored:F0} damage for {dur} turns (structured)");
                                }
                                break;
                            }
                        }
                    }
                    break;
                case AilmentId.Chill:
                    if (targetEnemy != null)
                    {
                        // Calculate damage breakdown to get cold damage
                        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
                        int chillDuration = card.comboAilmentDuration > 0 ? card.comboAilmentDuration : 2;
                        
                        // Chilled always applies when cold damage is dealt
                        // Use actual cold damage from breakdown, or fallback to portion if no cold damage
                        float coldDmg = damageBreakdown.cold > 0f ? damageBreakdown.cold : 
                                       (Mathf.Approximately(card.comboAilmentPortion, 0f) ? 20f : card.comboAilmentPortion);
                        
                        StatusEffect chilledEffect = StatusEffectFactory.CreateChilled(coldDmg, chillDuration);
                        ApplyStatusEffectToEnemy(targetEnemy, chilledEffect);
                        Debug.Log($"[Chill] Applied Chill (cold damage: {coldDmg}, dur {chillDuration}) to {targetEnemy.enemyName}");
                    }
                    break;
            }
        }
        
        // Check if enemy is defeated (only for non-projectile cards - projectile cards check in ApplyProjectileDamage)
        if (!isProjectile && targetEnemy.currentHealth <= 0)
        {
            Debug.Log($"<color=yellow>💀 {targetEnemy.enemyName} has been defeated!</color>");
            
                // XP grant per enemy kill using area level and rarity multipliers with overlevel penalties
                TryGrantKillExperience(targetEnemy);
                TryGenerateLoot(targetEnemy);
                
                if (combatManager != null)
                {
                    combatManager.OnEnemyDefeated?.Invoke(targetEnemy);
                }
        }
    }

    private void TryGenerateLoot(Enemy enemy)
    {
        // Simple loot: roll a weapon from ItemDatabase matching enemy tier → rarity
        var itemDb = ItemDatabase.Instance;
        if (itemDb == null) return;

        ItemRarity rarity = ItemRarity.Normal;
        try
        {
            var displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var d in displays)
            {
                if (d != null && d.GetCurrentEnemy() == enemy)
                {
                    var tierField = typeof(EnemyCombatDisplay).GetField("enemyData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var ed = tierField != null ? tierField.GetValue(d) as EnemyData : null;
                    if (ed != null)
                    {
                        switch (ed.tier)
                        {
                            case EnemyTier.Normal: rarity = ItemRarity.Normal; break;
                            case EnemyTier.Elite: rarity = ItemRarity.Magic; break;
                            case EnemyTier.Miniboss: rarity = ItemRarity.Rare; break;
                            case EnemyTier.Boss: rarity = ItemRarity.Rare; break;
                        }
                    }
                    break;
                }
            }
        }
        catch { rarity = ItemRarity.Normal; }

        var candidates = itemDb.GetWeaponsByRarity(rarity);
        if (candidates == null || candidates.Count == 0)
        {
            candidates = itemDb.weapons; // fallback to any weapon
        }
        if (candidates == null || candidates.Count == 0) return;

        var dropped = candidates[Random.Range(0, candidates.Count)];

        // Add to inventory via CharacterManager
        var cm = CharacterManager.Instance;
        if (cm != null)
        {
            cm.AddItem(dropped);
        }

        // Log to combat UI with rarity color and hover tooltip
        var animatedUI = FindFirstObjectByType<AnimatedCombatUI>();
        if (animatedUI != null)
        {
            animatedUI.AddLootToken(dropped, enemy.enemyName);
        }
        else
        {
            string color = GetRarityHexColor(dropped.rarity);
            string itemText = $"<color=#{color}>[{dropped.itemName}]</color>";
            string message = $"{enemy.enemyName} dropped {itemText}";
            var simpleUI = FindFirstObjectByType<CombatUI>();
            if (simpleUI != null)
            {
                simpleUI.SetCombatLogMessage(message);
            }
        }
    }

    private string GetRarityHexColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return "FFFFFF";
            case ItemRarity.Magic: return "4AA3FF"; // blue
            case ItemRarity.Rare: return "FFD700"; // gold
            case ItemRarity.Unique: return "FF7F27"; // orange
            default: return "FFFFFF";
        }
    }

    private void TryShowHoverTooltipForDrop(BaseItem item)
    {
        // Basic implementation: create a transient GameObject with TooltipTrigger under the root canvas
        try
        {
            var canvas = GameObject.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;
            var go = new GameObject("LootTooltipProxy");
            go.transform.SetParent(canvas.transform, false);
            var trigger = go.AddComponent<TooltipTrigger>();
            trigger.title = item.GetDisplayName();
            trigger.content = item.GetFullDescription();
            // position near mouse; the trigger script reads Input.mousePosition
            // auto-destroy shortly after to avoid clutter
            LeanTween.delayedCall(go, 2f, () => { if (go != null) GameObject.Destroy(go); });
        }
        catch { /* best-effort */ }
    }

    /// <summary>
    /// Find the UI/display index for a given Enemy, using CombatDisplayManager if available.
    /// </summary>
    private int FindEnemyIndex(Enemy enemy)
    {
        if (enemy == null) return -1;

        // Try direct match against enemyDisplays
        if (combatManager != null && combatManager.GetActiveEnemyDisplays() != null)
        {
            for (int i = 0; i < combatManager.GetActiveEnemyDisplays().Count; i++)
            {
                var d = combatManager.GetActiveEnemyDisplays()[i];
                if (d != null && d.GetCurrentEnemy() == enemy)
                {
                    return i;
                }
            }
        }

        // Try activeEnemies list via reflection on CombatDisplayManager
        var cdm = combatManager != null ? combatManager : FindFirstObjectByType<CombatDisplayManager>();
        if (cdm != null)
        {
            var field = typeof(CombatDisplayManager).GetField("activeEnemies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var list = field.GetValue(cdm) as List<Enemy>;
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == enemy) return i;
                    }
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Compute XP for a kill using area level, rarity multipliers, and overlevel penalties.
    /// </summary>
    private int ComputeKillXp(Enemy enemy)
    {
        int areaLevel = 1;
        int playerLevel = 1;
        var encounterMgr = EncounterManager.Instance;
        if (encounterMgr != null)
        {
            var enc = encounterMgr.GetCurrentEncounter();
            if (enc != null)
            {
                areaLevel = Mathf.Max(1, enc.areaLevel);
            }
        }

        var cm = CharacterManager.Instance;
        if (cm != null && cm.HasCharacter())
        {
            playerLevel = Mathf.Max(1, cm.GetCurrentCharacter().level);
        }

        float baseXP = 5f;
        float areaMultiplier = 1f + 0.1f * (areaLevel - 1);

        // Use enemy's experienceMultiplier from rarity modifiers (hidden base modifiers)
        float rarityMultiplier = 1f;
        if (enemy != null)
        {
            // Prefer the hidden experienceMultiplier if available (from MonsterRarityModifiers)
            if (enemy.experienceMultiplier > 1f)
            {
                rarityMultiplier = enemy.experienceMultiplier;
            }
            else
            {
                // Fallback to old hardcoded values if experienceMultiplier not set
                switch (enemy.rarity)
                {
                    case EnemyRarity.Magic: rarityMultiplier = 1.4f; break;
                    case EnemyRarity.Rare: rarityMultiplier = 2.0f; break;
                    case EnemyRarity.Unique: rarityMultiplier = 3.0f; break;
                    default: rarityMultiplier = 1f; break;
                }
            }
        }

        // Fallback: use EnemyData tier or label if needed
        try
        {
            var displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var d in displays)
            {
                if (d != null && d.GetCurrentEnemy() == enemy)
                {
                    var tierField = typeof(EnemyCombatDisplay).GetField("enemyData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var ed = tierField != null ? tierField.GetValue(d) as EnemyData : null;
                    if (ed != null)
                    {
                        switch (ed.tier)
                        {
                            case EnemyTier.Normal: rarityMultiplier = Mathf.Max(rarityMultiplier, 1f); break;
                            case EnemyTier.Elite: rarityMultiplier = Mathf.Max(rarityMultiplier, 1.6f); break;
                            case EnemyTier.Miniboss: rarityMultiplier = Mathf.Max(rarityMultiplier, 2.0f); break;
                            case EnemyTier.Boss: rarityMultiplier = Mathf.Max(rarityMultiplier, 3.0f); break;
                        }
                    }
                    else
                    {
                        string typeText = d.enemyTypeText != null ? d.enemyTypeText.text.ToLower() : string.Empty;
                        if (typeText.Contains("unique")) rarityMultiplier = Mathf.Max(rarityMultiplier, 3.0f);
                        else if (typeText.Contains("rare")) rarityMultiplier = Mathf.Max(rarityMultiplier, 2.0f);
                        else if (typeText.Contains("magic")) rarityMultiplier = Mathf.Max(rarityMultiplier, 1.4f);
                    }
                    break;
                }
            }
        }
        catch { rarityMultiplier = Mathf.Max(rarityMultiplier, 1f); }

        float xp = baseXP * areaMultiplier * rarityMultiplier;
        int diff = playerLevel - areaLevel;
        if (diff >= 4 && diff <= 8)
        {
            float[] penalties = { 0.8f, 0.6f, 0.4f, 0.2f, 0.1f };
            xp *= penalties[diff - 4];
        }
        else if (diff >= 9)
        {
            xp = 0f;
        }
        return Mathf.RoundToInt(xp);
    }

    private void TryGrantKillExperience(Enemy enemy)
    {
        var cm = CharacterManager.Instance;
        if (cm == null || !cm.HasCharacter()) return;
        int xp = ComputeKillXp(enemy);
        if (xp <= 0) return;
        cm.AddExperience(xp);
        Debug.Log($"[XP] Gained {xp} XP for killing {enemy.enemyName}. Level {cm.GetCurrentCharacter().level} XP {cm.GetCurrentCharacter().experience}/{cm.GetCurrentCharacter().GetRequiredExperience()}");
    }
    
    /// <summary>
    /// Update enemy display to show current HP.
    /// </summary>
    private void UpdateEnemyDisplay(Enemy enemy)
    {
        if (combatManager == null || combatManager.GetActiveEnemyDisplays() == null) return;
        
        // Find the display for this enemy
        foreach (var enemyDisplay in combatManager.GetActiveEnemyDisplays())
        {
            if (enemyDisplay.GetCurrentEnemy() == enemy)
            {
                enemyDisplay.RefreshDisplay();
                Debug.Log($"  → Updated {enemyDisplay.name} health display");
                return;
            }
        }
    }
    
    /// <summary>
    /// Apply a guard card - add block to player.
    /// </summary>
    private void ApplyGuardCard(Card card, Character player, bool isDelayed = false)
    {
        float guardAmount = 0f;
        int momentumSpent = 0;
        
        // Check for momentum-based guard scaling ("Guard per Momentum spent")
        if (player != null && MomentumEffectParser.HasGuardPerMomentumSpent(card.description))
        {
            // Handle "Guard per Momentum spent"
            string desc = card.description ?? "";
            
            // Calculate base guard first (this will be added to momentum-based guard)
            float baseGuard = CalculateGuard(card, player);
            
            // Spend momentum first
            int spendAmount = MomentumEffectParser.ParseSpendMomentum(desc);
            if (spendAmount == -1) // Spend all
            {
                momentumSpent = player.SpendAllMomentum();
            }
            else if (spendAmount > 0)
            {
                momentumSpent = player.SpendMomentum(spendAmount);
            }
            
            if (momentumSpent > 0)
            {
                // Calculate guard per momentum (uses card's base guard + guard scaling)
                float momentumGuard = MomentumEffectParser.CalculatePerMomentumGuard(desc, momentumSpent, card, player);
                // Add base guard to momentum-based guard
                guardAmount = baseGuard + momentumGuard;
                Debug.Log($"<color=cyan>[Momentum] Spent {momentumSpent} momentum, gaining {baseGuard:F1} base guard + {momentumGuard:F1} momentum guard = {guardAmount:F1} total</color>");
            }
            else
            {
                Debug.LogWarning($"[Momentum] {card.cardName} requires momentum but player has none!");
                // Still apply base guard if any
                guardAmount = baseGuard;
            }
        }
        else
        {
            // Standard guard calculation
            guardAmount = CalculateGuard(card, player);
        }
        
        // Apply delayed card bonus: +30% guard for delayed guard cards
        if (isDelayed)
        {
            guardAmount *= 1.30f;
            Debug.Log($"<color=cyan>[Delayed Bonus] Guard card gains +30% guard: {guardAmount:F1}</color>");
        }
        
        // THIEF CARD EFFECTS: Check for dual wield and preparation bonuses
        CardDataExtended extendedCard = GetCardDataExtended(card);
        if (extendedCard != null && player != null)
        {
            // Shadow Step: Preparation bonus (+2 Guard + 1 temp Dex when unleashed)
            // This is handled in PreparationManager when card is unleashed
            
            // Process dual wield effects
            if (!string.IsNullOrEmpty(extendedCard.dualWieldEffect) && ThiefCardEffects.IsDualWielding(player))
            {
                ThiefCardEffects.ProcessDualWieldEffect(extendedCard.dualWieldEffect, card, player);
            }
        }
        
        // Process momentum threshold effects BEFORE applying guard
        MomentumThresholdResult momentumEffects = ProcessMomentumThresholdEffects(card, player, CardType.Guard);
        
        // Apply momentum-based guard bonuses
        if (momentumEffects != null)
        {
            // Guard per momentum (e.g., "Momentum, gain +1 Guard per Momentum")
            if (momentumEffects.guardPerMomentum > 0)
            {
                int currentMomentum = player.GetMomentum();
                float bonusGuard = momentumEffects.guardPerMomentum * currentMomentum;
                guardAmount += bonusGuard;
                Debug.Log($"<color=cyan>[Momentum Effect] Gained {bonusGuard:F0} guard from {currentMomentum} momentum ({momentumEffects.guardPerMomentum} per momentum)</color>");
            }
            
            // Additional guard flat amount
            if (momentumEffects.additionalGuard > 0)
            {
                guardAmount += momentumEffects.additionalGuard;
                Debug.Log($"<color=cyan>[Momentum Effect] Gained {momentumEffects.additionalGuard} additional guard</color>");
            }
        }
        
        // Apply guard to player
        if (player != null)
        {
            player.AddGuard(guardAmount);
            Debug.Log($"  🛡️ Player gained {guardAmount:F0} guard (Total: {player.currentGuard}/{player.maxHealth})");
            
            // Update the guard display UI
            PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                playerDisplay.UpdateGuardDisplay();
                Debug.Log($"  🛡️ Guard display updated");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ PlayerCombatDisplay not found - guard UI won't update");
            }
        }
        
        // Process generic on-play effects (e.g., Draw, Momentum gain)
        ApplyOnPlayEffects(card, player);
        
        // Apply momentum threshold effects (draw cards, stat boosts, etc.)
        if (momentumEffects != null)
        {
            ApplyMomentumThresholdResult(momentumEffects, card, player);
        }
    }
    
    /// <summary>
    /// Apply temporary evasion buff to player from Skill card.
    /// Supports both flat evasion (baseEvasion > 0) and percentage-based evasion (baseEvasion = 0, percentage in description).
    /// </summary>
    private void ApplyEvasionBuff(CardDataExtended card, Character player, bool isDelayed = false)
    {
        if (card == null || player == null) return;
        
        // Check if this is percentage-based evasion (Focus card pattern: "Gain X% increased evasion")
        // Priority: Check description first, then fall back to baseEvasion
        bool isPercentageBased = card.description.ToLower().Contains("% increased evasion");
        float evasionAmount = 0f;
        
        if (isPercentageBased)
        {
            // Extract percentage from description (e.g., "Gain 20% increased evasion" -> 20)
            // For Focus: "Gain 20% increased evasion for 2 turns"
            var match = System.Text.RegularExpressions.Regex.Match(card.description, @"(\d+)%\s*increased\s*evasion", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && float.TryParse(match.Groups[1].Value, out float percentage))
            {
                evasionAmount = percentage; // TempEvasion uses magnitude/100, so 20 = 20% increased
            }
            else
            {
                Debug.LogWarning($"[CardEffectProcessor] Could not parse percentage from Focus card description: {card.description}");
                return;
            }
        }
        else
        {
            // Flat evasion: base + scaling
            evasionAmount = card.baseEvasion;
            evasionAmount += card.evasionScaling.CalculateScalingBonus(player);
        }
        
        // Apply delayed card bonus: +30% for delayed skill effects
        if (isDelayed)
        {
            if (isPercentageBased)
            {
                evasionAmount *= 1.30f; // 30% more percentage
            }
            else
            {
                evasionAmount *= 1.30f; // 30% more flat evasion
            }
            Debug.Log($"<color=cyan>[Delayed Bonus] Evasion card gains +30% evasion: {evasionAmount:F1}</color>");
        }
        
        // Find player's StatusEffectManager
        PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay == null)
        {
            Debug.LogWarning("[CardEffectProcessor] PlayerCombatDisplay not found - cannot apply evasion buff");
            return;
        }
        
        StatusEffectManager statusManager = playerDisplay.GetStatusEffectManager();
        if (statusManager == null)
        {
            Debug.LogWarning("[CardEffectProcessor] StatusEffectManager not found on player display - cannot apply evasion buff");
            return;
        }
        
        // Create and apply TempEvasion status effect
        // Note: TempEvasion applies magnitude/100 as increasedEvasion percentage
        int duration = card.evasionDuration < 0 ? -1 : card.evasionDuration; // -1 = rest of combat
        StatusEffect evasionBuff = new StatusEffect(
            StatusEffectType.TempEvasion,
            $"{card.cardName}: Evasion",
            evasionAmount,
            duration,
            false // Not a debuff
        );
        
        statusManager.AddStatusEffect(evasionBuff);
        string evasionType = isPercentageBased ? "% increased" : "flat";
        Debug.Log($"  🟢 Player gained {evasionAmount:F0} {evasionType} evasion for {(duration < 0 ? "rest of combat" : $"{duration} turns")} from {card.cardName}");
    }
    
    /// <summary>
    /// Apply a skill card - various effects.
    /// </summary>
    private void ApplySkillCard(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition, bool isDelayed = false)
    {
        // Skills can have both damage and other effects
        if (card.baseDamage > 0)
        {
            ApplyAttackCard(card, targetEnemy, player, targetScreenPosition, isDelayed);
        }
        
        // Check for guard granting (either baseGuard > 0 OR "Guard per Momentum spent" pattern)
        bool hasGuardEffect = card.baseGuard > 0 || (player != null && MomentumEffectParser.HasGuardPerMomentumSpent(card.description));
        
        if (hasGuardEffect)
        {
            // Handle momentum-based guard for Skill cards
            if (MomentumEffectParser.HasGuardPerMomentumSpent(card.description))
            {
                string desc = card.description ?? "";
                int momentumSpent = 0;
                
                // Spend momentum first
                int spendAmount = MomentumEffectParser.ParseSpendMomentum(desc);
                if (spendAmount == -1) // Spend all
                {
                    momentumSpent = player.SpendAllMomentum();
                }
                else if (spendAmount > 0)
                {
                    momentumSpent = player.SpendMomentum(spendAmount);
                }
                
                if (momentumSpent > 0)
                {
                    // Calculate guard per momentum (uses card's base guard + guard scaling)
                    float guardAmount = MomentumEffectParser.CalculatePerMomentumGuard(desc, momentumSpent, card, player);
                    
                    // Apply delayed card bonus: +30% guard for delayed skill cards
                    if (isDelayed)
                    {
                        guardAmount *= 1.30f;
                        Debug.Log($"<color=cyan>[Delayed Bonus] Skill guard card gains +30% guard: {guardAmount:F1}</color>");
                    }
                    
                    // Apply guard to player
                    player.AddGuard(guardAmount);
                    Debug.Log($"<color=cyan>[Momentum] Spent {momentumSpent} momentum, gained {guardAmount:F1} guard from {card.cardName}</color>");
                    Debug.Log($"  🛡️ Player gained {guardAmount:F0} guard (Total: {player.currentGuard}/{player.maxHealth})");
                    
                    // Update the guard display UI
                    PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
                    if (playerDisplay != null)
                    {
                        playerDisplay.UpdateGuardDisplay();
                    }
                }
                else
                {
                    Debug.LogWarning($"[Momentum] {card.cardName} requires momentum but player has none!");
                }
            }
            else
            {
                // Standard guard card processing
                ApplyGuardCard(card, player, isDelayed);
            }
        }
        
        // Apply evasion buffs (for Skill cards that grant evasion)
        // Try to get CardDataExtended from card's source reference or lookup by name
        CardDataExtended extendedCard = null;
        
        // Check if Card has sourceCardData reference (CardDataExtended)
        if (card is Card cardObj && cardObj.sourceCardData != null)
        {
            extendedCard = cardObj.sourceCardData;
        }
        else
        {
            // Fallback: Look up by card name from Resources
            var allCards = Resources.LoadAll<CardDataExtended>("Cards");
            foreach (var cardAsset in allCards)
            {
                if (cardAsset != null && cardAsset.cardName == card.cardName)
                {
                    extendedCard = cardAsset;
                    break;
                }
            }
        }
        
        if (extendedCard != null && (extendedCard.baseEvasion > 0 || extendedCard.evasionScaling.CalculateScalingBonus(player) > 0 || extendedCard.description.ToLower().Contains("% increased evasion")))
        {
            ApplyEvasionBuff(extendedCard, player, isDelayed);
        }
        
        // THIEF CARD EFFECTS: Check for prepared card interactions and dual wield
        if (extendedCard != null && player != null)
        {
            // Feint: Advance prepared cards charge by 1 (or 2 if dual wielding)
            if (card.cardName.Contains("Feint") || card.description.Contains("Advance prepared"))
            {
                bool isDualWielding = ThiefCardEffects.IsDualWielding(player);
                int advanceAmount = isDualWielding ? 2 : 1;
                ThiefCardEffects.AdvancePreparedCardCharges(advanceAmount);
            }
            
            // Poisoned Blade: Apply +1 Poison per prepared card
            if (card.cardName.Contains("Poisoned Blade") && targetEnemy != null)
            {
                int preparedCount = ThiefCardEffects.GetPreparedCardCount();
                if (preparedCount > 0)
                {
                    // Find enemy's StatusEffectManager via EnemyCombatDisplay
                    var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                    foreach (var display in enemyDisplays)
                    {
                        if (display != null && display.GetCurrentEnemy() == targetEnemy)
                        {
                            var statusMgr = display.GetComponent<StatusEffectManager>();
                            if (statusMgr != null)
                            {
                                // For Poisoned Blade, calculate damage if card deals damage
                                // Otherwise use a base value for poison calculation
                                float skillDamage = card.baseDamage > 0 ? DamageCalculator.CalculateCardDamage(card, player) : 10f;
                                DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, skillDamage);
                                
                                // For Poisoned Blade, we apply poison per prepared card
                                // Use actual damage breakdown, but ensure we have some damage to work with
                                float physDmg = damageBreakdown.physical > 0f ? damageBreakdown.physical : 10f; // Fallback
                                float chaosDmg = damageBreakdown.chaos > 0f ? damageBreakdown.chaos : 0f;
                                
                                // Apply poison for each prepared card (each is a separate stack)
                                for (int i = 0; i < preparedCount; i++)
                                {
                                    StatusEffect poisonEffect = StatusEffectFactory.CreatePoison(physDmg, chaosDmg, 3);
                                    statusMgr.AddStatusEffect(poisonEffect);
                                }
                                Debug.Log($"<color=green>[Thief] Poisoned Blade applied {preparedCount} Poison stacks from {preparedCount} prepared cards</color>");
                            }
                            break;
                        }
                    }
                }
            }
            
            // Process dual wield effects
            if (!string.IsNullOrEmpty(extendedCard.dualWieldEffect) && ThiefCardEffects.IsDualWielding(player))
            {
                ThiefCardEffects.ProcessDualWieldEffect(extendedCard.dualWieldEffect, card, player, targetEnemy);
            }
        }
        
        // Process CardEffects for Skill cards (e.g., ApplyStatus effects)
        // This handles Skill cards that don't deal damage but apply status effects
        if (card.effects != null && card.effects.Count > 0)
        {
            // For single-target Skill cards, process effects on the target enemy
            if (targetEnemy != null)
            {
                // Calculate damage breakdown (may be 0 for non-damage Skill cards)
                float skillDamage = card.baseDamage > 0 ? DamageCalculator.CalculateCardDamage(card, player, null, targetEnemy) : 0f;
                ProcessCardEffectsForEnemy(card, targetEnemy, player, skillDamage);
            }
            // For AoE Skill cards, effects are processed in ApplyAoECard
        }
        
        // Apply delayed card bonus for skill effects: +1 stack/effect or +30% duration
        if (isDelayed)
        {
            ApplyDelayedSkillBonuses(card, targetEnemy, player);
        }
        
        // Gain Focus charges from Skill cards (affected by castSpeed and focusGainIncreased)
        if (player != null && StackSystem.Instance != null)
        {
            var statsData = new CharacterStatsData(player);
            float baseGain = 1f; // Base: 1 Focus charge per Skill card
            float speedMultiplier = 1f + (statsData.castSpeed / 100f);
            float gainMultiplier = 1f + (statsData.focusGainIncreased / 100f);
            float finalGain = baseGain * speedMultiplier * gainMultiplier;
            int focusGain = Mathf.RoundToInt(finalGain);
            
            if (focusGain > 0)
            {
                StackSystem.Instance.AddStacks(StackType.Focus, focusGain);
                Debug.Log($"[Focus] Gained {focusGain} Focus charge(s) from {card.cardName} (Cast Speed: {statsData.castSpeed}%, Focus Gain: {statsData.focusGainIncreased}%)");
            }
        }
        
        // Process momentum threshold effects
        MomentumThresholdResult momentumEffects = ProcessMomentumThresholdEffects(card, player, CardType.Skill);
        
        // Process generic on-play effects (e.g., Draw, Momentum gain)
        // NOTE: If momentum threshold modifies momentum gain, we need to handle it specially
        ApplyOnPlayEffects(card, player, momentumEffects);
        
        // Apply momentum threshold effects (draw cards, stat boosts, etc.)
        // Note: Additional momentum is handled in ApplyOnPlayEffects
        if (momentumEffects != null)
        {
            // Don't apply additional momentum here - it's handled in ApplyOnPlayEffects
            var effectsToApply = new MomentumThresholdResult
            {
                drawCards = momentumEffects.drawCards,
                tempStrength = momentumEffects.tempStrength,
                tempDexterity = momentumEffects.tempDexterity,
                tempIntelligence = momentumEffects.tempIntelligence,
                energyGain = momentumEffects.energyGain,
                applyBleed = momentumEffects.applyBleed,
                doubleNextAttack = momentumEffects.doubleNextAttack,
                triggerAdrenalineBurst = momentumEffects.triggerAdrenalineBurst
            };
            ApplyMomentumThresholdResult(effectsToApply, card, player, targetEnemy);
        }
        
        // NEW: If this skill is a Shout, consume Crumble on targets
        // Or if the card explicitly consumes an ailment (per-card)
        bool shouldConsume = card.cardName.ToLower().Contains("shout") || card.consumeAilmentEnabled;
        if (shouldConsume)
        {
            AilmentId consumeId = card.consumeAilmentEnabled ? card.consumeAilmentId : AilmentId.Crumble;
            if (card.isAoE)
            {
                foreach (var enemy in GetAllActiveEnemies())
                {
                    var displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                    foreach (var d in displays)
                    {
                        if (d != null && d.GetCurrentEnemy() == enemy)
                        {
                            var statusManager = d.GetComponent<StatusEffectManager>();
                            if (statusManager != null)
                            {
                                if (consumeId == AilmentId.Crumble) statusManager.ConsumeCrumble();
                                else if (consumeId == AilmentId.Chill) statusManager.RemoveStatusEffect(StatusEffectType.Chill);
                                // Additional ailments can be added here with specific consume logic
                            }
                            break;
                        }
                    }
                }
            }
            else if (targetEnemy != null)
            {
                var displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                foreach (var d in displays)
                {
                    if (d != null && d.GetCurrentEnemy() == targetEnemy)
                    {
                        var statusManager = d.GetComponent<StatusEffectManager>();
                        if (statusManager != null)
                        {
                            if (consumeId == AilmentId.Crumble) statusManager.ConsumeCrumble();
                            else if (consumeId == AilmentId.Chill) statusManager.RemoveStatusEffect(StatusEffectType.Chill);
                            // Additional ailments can be added here with specific consume logic
                        }
                        break;
                    }
                }
            }
        }
        
        Debug.Log($"  ✨ Skill effect applied: {card.cardName}");
    }
    
    /// <summary>
    /// Apply a power card - buff player.
    /// </summary>
    private void ApplyPowerCard(Card card, Character player, bool isDelayed = false)
    {
        Debug.Log($"  💪 Power card applied: {card.cardName}");
        
        // DIVINE FAVOR: "The next card you play applies their 'discarded' effect."
        if (card.cardName.Contains("Divine Favor") || (card.description != null && card.description.Contains("next card") && card.description.Contains("discarded")))
        {
            var deckManager = CombatDeckManager.Instance;
            if (deckManager != null)
            {
                // Use reflection to set the flag
                var field = typeof(CombatDeckManager).GetField("nextCardAppliesDiscardedEffect", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(deckManager, true);
                    Debug.Log($"<color=yellow>[Divine Favor] Next card played will apply its discarded effect!</color>");
                }
            }
        }
        
        // BERSERKER'S FURY: "For the rest of combat, when you gain Momentum, gain 1 additional."
        if (player != null && (card.cardName == "Berserker's Fury" || card.groupKey == "Berserkers_Fury"))
        {
            player.momentumGainBonus += 1;
            Debug.Log($"<color=orange>[Berserker's Fury] Activated! All momentum gains now grant +1 additional (Total bonus: {player.momentumGainBonus})</color>");
        }
        
        // Apply delayed card bonus for power effects: +1 stack/effect or +30% duration
        if (isDelayed)
        {
            ApplyDelayedPowerBonuses(card, player);
        }
        
        // Process momentum threshold effects
        MomentumThresholdResult momentumEffects = ProcessMomentumThresholdEffects(card, player, CardType.Power);
        
        // Process generic on-play effects (e.g., Draw, Momentum gain)
        ApplyOnPlayEffects(card, player);
        
        // Apply momentum threshold effects (draw cards, stat boosts, etc.)
        if (momentumEffects != null)
        {
            ApplyMomentumThresholdResult(momentumEffects, card, player);
        }
    }
    
    /// <summary>
    /// Apply delayed bonuses for skill cards: +1 stack/effect or +30% duration
    /// </summary>
    private void ApplyDelayedSkillBonuses(Card card, Enemy targetEnemy, Character player)
    {
        if (card == null || card.effects == null) return;
        
        Debug.Log($"<color=cyan>[Delayed Bonus] Applying skill bonuses for {card.cardName}</color>");
        
        // Apply bonuses to status effects when they are created
        // We'll modify the effects before they're processed
        foreach (var effect in card.effects)
        {
            if (effect == null) continue;
            
            if (effect.effectType == EffectType.ApplyStatus)
            {
                // Increase magnitude by 1 (extra stack) or duration by 30%
                if (effect.value > 0f)
                {
                    // If it's a stack-based effect, add 1 stack
                    float originalValue = effect.value;
                    effect.value += 1f;
                    Debug.Log($"  → Status effect magnitude increased by 1: {originalValue} → {effect.value}");
                }
                else if (effect.duration > 0)
                {
                    // If it's duration-based, increase by 30%
                    int originalDuration = effect.duration;
                    effect.duration = Mathf.RoundToInt(effect.duration * 1.30f);
                    Debug.Log($"  → Status effect duration increased by 30%: {originalDuration} → {effect.duration} turns");
                }
            }
        }
    }
    
    /// <summary>
    /// Apply delayed bonuses for power cards: +1 stack/effect or +30% duration
    /// </summary>
    private void ApplyDelayedPowerBonuses(Card card, Character player)
    {
        if (card == null || card.effects == null) return;
        
        Debug.Log($"<color=cyan>[Delayed Bonus] Applying power bonuses for {card.cardName}</color>");
        
        // Apply bonuses to buffs/effects
        foreach (var effect in card.effects)
        {
            if (effect == null) continue;
            
            if (effect.effectType == EffectType.ApplyStatus || effect.effectType == EffectType.TemporaryStatBoost)
            {
                // Increase magnitude by 1 (extra stack) or duration by 30%
                if (effect.value > 0f)
                {
                    // If it's a stack-based effect, add 1 stack
                    float originalValue = effect.value;
                    effect.value += 1f;
                    Debug.Log($"  → Effect magnitude increased by 1: {originalValue} → {effect.value}");
                }
                else if (effect.duration > 0)
                {
                    // If it's duration-based, increase by 30%
                    int originalDuration = effect.duration;
                    effect.duration = Mathf.RoundToInt(effect.duration * 1.30f);
                    Debug.Log($"  → Effect duration increased by 30%: {originalDuration} → {effect.duration} turns");
                }
            }
        }
    }

    /// <summary>
    /// Handle generic on-play effects that should apply regardless of card type (e.g., Draw, Momentum gain).
    /// </summary>
    private void ApplyOnPlayEffects(Card card, Character player = null, MomentumThresholdResult momentumEffects = null)
    {
        if (card == null)
        {
            Debug.LogWarning("[ApplyOnPlayEffects] Card is null!");
            return;
        }
        
        if (card.effects == null)
        {
            Debug.LogWarning($"[ApplyOnPlayEffects] Card '{card.cardName}' has null effects list!");
            return;
        }
        
        // Get player if not provided
        if (player == null)
        {
            var charMgr = CharacterManager.Instance;
            if (charMgr != null && charMgr.HasCharacter())
            {
                player = charMgr.GetCurrentCharacter();
            }
        }
        
        int drawTotal = 0;
        int momentumGain = 0;
        
        Debug.Log($"[ApplyOnPlayEffects] Processing {card.cardName} with {card.effects.Count} effect(s)");
        
        foreach (var eff in card.effects)
        {
            if (eff == null)
            {
                Debug.LogWarning($"[ApplyOnPlayEffects] Null effect found in card '{card.cardName}'");
                continue;
            }
            
            Debug.Log($"[ApplyOnPlayEffects] Effect: {eff.effectType}, Value: {eff.value}, Name: {eff.effectName}");
            
            if (eff.effectType == EffectType.Draw)
            {
                drawTotal += Mathf.RoundToInt(eff.value);
            }
            else if (eff.effectType == EffectType.GainMomentum)
            {
                // GainMomentum grants momentum stacks via StackSystem
                int amount = Mathf.RoundToInt(eff.value);
                momentumGain += amount;
                Debug.Log($"[ApplyOnPlayEffects] Found GainMomentum effect: {amount} momentum");
            }
            else if (eff.effectType == EffectType.TemporaryStatBoost)
            {
                // Apply temporary stat boost to player
                if (player != null && eff.targetsSelf)
                {
                    // Use the CardEffect's ApplyEffect method which handles warrantStatModifiers
                    eff.ApplyEffect(player, player);
                    
                    // Track for removal at end of turn if duration is specified
                    if (eff.duration > 0)
                    {
                        var tracker = new TemporaryStatBoostTracker
                        {
                            character = player,
                            statName = eff.effectName,
                            value = eff.value,
                            remainingTurns = eff.duration
                        };
                        activeStatBoosts.Add(tracker);
                        Debug.Log($"[ApplyOnPlayEffects] Tracked temporary stat boost: {eff.effectName} = +{eff.value}% for {eff.duration} turn(s)");
                    }
                }
            }
        }
        
        // Apply momentum threshold modifications to momentum gain
        if (momentumEffects != null && momentumEffects.additionalMomentum > 0)
        {
            // Check if this is a replacement ("instead of") or additional gain
            CardDataExtended extendedCard = GetCardDataExtended(card);
            bool isReplacement = false;
            if (extendedCard != null && !string.IsNullOrEmpty(extendedCard.momentumEffectDescription))
            {
                // Check if effect text says "instead of"
                isReplacement = extendedCard.momentumEffectDescription.ToLower().Contains("instead of");
            }
            
            if (isReplacement && momentumGain > 0)
            {
                // Replace base momentum gain with the threshold amount
                int oldGain = momentumGain;
                momentumGain = momentumEffects.additionalMomentum;
                Debug.Log($"<color=cyan>[Momentum Threshold] Replaced momentum gain: {oldGain} → {momentumGain} (from threshold effect)</color>");
            }
            else if (!isReplacement)
            {
                // Add to base momentum gain
                momentumGain += momentumEffects.additionalMomentum;
                Debug.Log($"<color=cyan>[Momentum Threshold] Added {momentumEffects.additionalMomentum} momentum to base gain (Total: {momentumGain})</color>");
            }
        }
        
        if (drawTotal > 0)
        {
            var deckMgr = CombatDeckManager.Instance;
            if (deckMgr != null)
            {
                Debug.Log($"[Effect] Draw {drawTotal} card(s)");
                deckMgr.DrawCards(drawTotal);
            }
        }
        
        if (momentumGain > 0 && player != null)
        {
            // Apply combat-wide momentum gain bonus (e.g., Berserker's Fury)
            int bonus = player.momentumGainBonus;
            int totalMomentumGain = momentumGain + bonus;
            
            // Grant momentum stacks via StackSystem
            var stackSystem = StackSystem.Instance;
            if (stackSystem != null)
            {
                stackSystem.AddStacks(StackType.Momentum, totalMomentumGain);
                if (bonus > 0)
                {
                    Debug.Log($"[Effect] Gained {momentumGain} Momentum stack(s) from {card.cardName} + {bonus} bonus = {totalMomentumGain} total (Total: {player.GetMomentum()})");
                }
                else
                {
                    Debug.Log($"[Effect] Gained {momentumGain} Momentum stack(s) from {card.cardName} (Total: {player.GetMomentum()})");
                }
            }
            else
            {
                Debug.LogWarning("[Effect] StackSystem.Instance is null - cannot grant momentum stacks");
            }
        }
    }
    
    // REMOVED: Old CalculateDamage() method
    // Now using DamageCalculator.CalculateCardDamage() for consistent damage calculation
    // with proper character modifiers, embossing effects, and debug logging
    
    /// <summary>
    /// Process CardEffects that target enemies (e.g., ApplyStatus effects like Poison)
    /// </summary>
    private void ProcessCardEffectsForEnemy(Card card, Enemy targetEnemy, Character player, float totalDamage)
    {
        if (card == null || card.effects == null || targetEnemy == null)
            return;
        
        // Find enemy's StatusEffectManager via EnemyCombatDisplay
        var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        EnemyCombatDisplay targetDisplay = null;
        foreach (var display in enemyDisplays)
        {
            if (display != null && display.GetCurrentEnemy() == targetEnemy)
            {
                targetDisplay = display;
                break;
            }
        }
        
        if (targetDisplay == null)
        {
            Debug.LogWarning($"[ProcessCardEffectsForEnemy] Could not find EnemyCombatDisplay for {targetEnemy.enemyName}");
            return;
        }
        
        var statusMgr = targetDisplay.GetComponent<StatusEffectManager>();
        if (statusMgr == null)
        {
            Debug.LogWarning($"[ProcessCardEffectsForEnemy] EnemyCombatDisplay has no StatusEffectManager");
            return;
        }
        
        // Calculate damage breakdown for status effects that scale with damage
        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
        
        // Process each CardEffect
        foreach (var effect in card.effects)
        {
            if (effect == null) continue;
            
            // Only process effects that target enemies
            if (!effect.targetsEnemy && !effect.targetsAllEnemies)
                continue;
            
            if (effect.effectType == EffectType.ApplyStatus)
            {
                // Determine status effect type from effect name
                StatusEffectType statusType = GetStatusEffectTypeFromName(effect.effectName);
                
                // For Poison, calculate damage from card's damage breakdown
                if (statusType == StatusEffectType.Poison)
                {
                    float physDmg = damageBreakdown.physical > 0f ? damageBreakdown.physical : 10f; // Fallback
                    float chaosDmg = damageBreakdown.chaos > 0f ? damageBreakdown.chaos : 0f;
                    
                    // Apply poison stacks (value is the number of stacks)
                    int poisonStacks = Mathf.RoundToInt(effect.value);
                    for (int i = 0; i < poisonStacks; i++)
                    {
                        StatusEffect poisonEffect = StatusEffectFactory.CreatePoison(physDmg, chaosDmg, effect.duration > 0 ? effect.duration : 3);
                        statusMgr.AddStatusEffect(poisonEffect);
                    }
                    Debug.Log($"<color=green>[CardEffectProcessor] {card.cardName} applied {poisonStacks} Poison stacks via CardEffect</color>");
                }
                else
                {
                    // For other status effects, use the CardEffect's ApplyEffect method
                    // But we need to pass the Enemy as a Character (this might need adjustment)
                    // For now, create the status effect directly
                    StatusEffect statusEffect = new StatusEffect(statusType, effect.effectName, effect.value, effect.duration > 0 ? effect.duration : 1);
                    statusMgr.AddStatusEffect(statusEffect);
                    Debug.Log($"<color=green>[CardEffectProcessor] {card.cardName} applied {effect.effectName} via CardEffect</color>");
                }
            }
        }
    }
    
    /// <summary>
    /// Get StatusEffectType from effect name (helper for CardEffect processing)
    /// </summary>
    private StatusEffectType GetStatusEffectTypeFromName(string effectName)
    {
        if (string.IsNullOrEmpty(effectName))
            return StatusEffectType.Poison; // Default fallback
        
        switch (effectName.ToLower())
        {
            case "poison":
            case "poisoned":
                return StatusEffectType.Poison;
            case "burn":
            case "burning":
            case "ignite":
            case "ignited":
                return StatusEffectType.Burn;
            case "chill":
            case "chilled":
                return StatusEffectType.Chill;
            case "freeze":
            case "frozen":
                return StatusEffectType.Freeze;
            case "bleed":
            case "bleeding":
                return StatusEffectType.Bleed;
            case "shock":
            case "shocked":
                return StatusEffectType.Shocked;
            case "vulnerable":
                return StatusEffectType.Vulnerable;
            case "weak":
                return StatusEffectType.Weak;
            default:
                return StatusEffectType.Poison; // Default fallback
        }
    }
    
    /// <summary>
    /// Get CardDataExtended from a Card object
    /// </summary>
    private CardDataExtended GetCardDataExtended(Card card)
    {
        if (card == null) return null;
        
        // Check if Card has sourceCardData reference (CardDataExtended)
        if (card is Card cardObj && cardObj.sourceCardData != null)
        {
            return cardObj.sourceCardData;
        }
        
        // Fallback: Look up by card name from Resources
        var allCards = Resources.LoadAll<CardDataExtended>("Cards");
        foreach (var cardAsset in allCards)
        {
            if (cardAsset != null && cardAsset.cardName == card.cardName)
            {
                return cardAsset;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Process momentum threshold effects for a card.
    /// Returns a MomentumThresholdResult containing any modifications to apply.
    /// </summary>
    private MomentumThresholdResult ProcessMomentumThresholdEffects(Card card, Character player, CardType cardType)
    {
        var result = new MomentumThresholdResult();
        if (card == null || player == null) return result;
        
        // Get CardDataExtended to access momentumEffectDescription
        CardDataExtended extendedCard = GetCardDataExtended(card);
        if (extendedCard == null || string.IsNullOrEmpty(extendedCard.momentumEffectDescription))
        {
            return result;
        }
        
        int currentMomentum = player.GetMomentum();
        if (currentMomentum <= 0) return result;
        
        // Parse threshold effects
        var thresholdEffects = MomentumThresholdEffectParser.ParseThresholdEffects(extendedCard.momentumEffectDescription);
        if (thresholdEffects.Count == 0) return result;
        
        // Get all applicable effects (sorted by threshold, highest first)
        var applicableEffects = MomentumThresholdEffectParser.GetAllApplicableEffects(thresholdEffects, currentMomentum);
        applicableEffects.Sort((a, b) => b.threshold.CompareTo(a.threshold)); // Highest threshold first
        
        Debug.Log($"<color=orange>[Momentum Threshold] {card.cardName} has {currentMomentum} momentum, checking {applicableEffects.Count} applicable effects</color>");
        
        // Process each applicable effect
        foreach (var effect in applicableEffects)
        {
            var effectType = MomentumThresholdEffectParser.ParseEffectType(effect.effectText);
            int numericValue = MomentumThresholdEffectParser.ParseNumericValue(effect.effectText);
            
            Debug.Log($"<color=yellow>[Momentum Threshold] {effect.threshold}+ Momentum: {effect.effectText} (Type: {effectType}, Value: {numericValue})</color>");
            
            switch (effectType)
            {
                case MomentumEffectType.CostReduction:
                    result.costReduction = numericValue > 0 ? numericValue : 1; // Default to 1 if not specified
                    Debug.Log($"<color=green>[Momentum] Cost reduced by {result.costReduction}</color>");
                    break;
                    
                case MomentumEffectType.ConvertToAoE:
                    result.convertToAoE = true;
                    Debug.Log($"<color=green>[Momentum] Card converted to AoE</color>");
                    break;
                    
                case MomentumEffectType.RandomTargets:
                    result.randomTargetCount = numericValue > 0 ? numericValue : 2; // Default to 2
                    Debug.Log($"<color=green>[Momentum] Will hit {result.randomTargetCount} random enemies</color>");
                    break;
                    
                case MomentumEffectType.AdditionalMomentum:
                    result.additionalMomentum = numericValue > 0 ? numericValue : 1; // Default to 1
                    Debug.Log($"<color=green>[Momentum] Will gain {result.additionalMomentum} additional momentum</color>");
                    break;
                    
                case MomentumEffectType.DrawCards:
                    result.drawCards = numericValue > 0 ? numericValue : 1; // Default to 1
                    Debug.Log($"<color=green>[Momentum] Will draw {result.drawCards} cards</color>");
                    break;
                    
                case MomentumEffectType.TemporaryStatBoost:
                    // Parse which stat and amount
                    string lowerText = effect.effectText.ToLower();
                    if (lowerText.Contains("strength"))
                    {
                        result.tempStrength = numericValue > 0 ? numericValue : 1;
                        Debug.Log($"<color=green>[Momentum] Will gain {result.tempStrength} temporary Strength</color>");
                    }
                    else if (lowerText.Contains("dexterity"))
                    {
                        result.tempDexterity = numericValue > 0 ? numericValue : 1;
                        Debug.Log($"<color=green>[Momentum] Will gain {result.tempDexterity} temporary Dexterity</color>");
                    }
                    else if (lowerText.Contains("intelligence"))
                    {
                        result.tempIntelligence = numericValue > 0 ? numericValue : 1;
                        Debug.Log($"<color=green>[Momentum] Will gain {result.tempIntelligence} temporary Intelligence</color>");
                    }
                    break;
                    
                case MomentumEffectType.EnergyGain:
                    result.energyGain = numericValue > 0 ? numericValue : 1; // Default to 1
                    Debug.Log($"<color=green>[Momentum] Will gain {result.energyGain} Energy</color>");
                    break;
                    
                case MomentumEffectType.ApplyAilment:
                    // Parse ailment type and amount
                    string ailmentText = effect.effectText.ToLower();
                    if (ailmentText.Contains("bleed"))
                    {
                        result.applyBleed = numericValue > 0 ? numericValue : 1;
                        Debug.Log($"<color=green>[Momentum] Will apply {result.applyBleed} Bleed to all enemies</color>");
                    }
                    // Add other ailments as needed
                    break;
                    
                case MomentumEffectType.DoubleDamage:
                    result.doubleNextAttack = true;
                    Debug.Log($"<color=green>[Momentum] Next attack will deal double damage</color>");
                    break;
                    
                case MomentumEffectType.GuardPerMomentum:
                    result.guardPerMomentum = numericValue > 0 ? numericValue : 1; // Default to 1 per momentum
                    Debug.Log($"<color=green>[Momentum] Will gain {result.guardPerMomentum} Guard per Momentum</color>");
                    break;
                    
                case MomentumEffectType.AdditionalGuard:
                    result.additionalGuard = numericValue > 0 ? numericValue : 1; // Default to 1
                    Debug.Log($"<color=green>[Momentum] Will gain {result.additionalGuard} additional Guard</color>");
                    break;
                    
                case MomentumEffectType.SpecialEffect:
                    // Handle special effects like "Adrenaline Burst"
                    // Match patterns like "Trigger Adrenaline Burst" or "Adrenaline Burst"
                    string effectLower = effect.effectText.ToLower();
                    if (effectLower.Contains("adrenaline burst") || 
                        (effectLower.Contains("trigger") && effectLower.Contains("adrenaline")))
                    {
                        result.triggerAdrenalineBurst = true;
                        Debug.Log($"<color=green>[Momentum] Will trigger Adrenaline Burst from: {effect.effectText}</color>");
                    }
                    break;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Result of processing momentum threshold effects
    /// </summary>
    private class MomentumThresholdResult
    {
        public int costReduction = 0;
        public bool convertToAoE = false;
        public int randomTargetCount = 0;
        public int additionalMomentum = 0;
        public int drawCards = 0;
        public int tempStrength = 0;
        public int tempDexterity = 0;
        public int tempIntelligence = 0;
        public int energyGain = 0;
        public int applyBleed = 0;
        public bool doubleNextAttack = false;
        public int guardPerMomentum = 0;
        public int additionalGuard = 0;
        public bool triggerAdrenalineBurst = false;
    }
    
    /// <summary>
    /// Apply momentum threshold result effects
    /// </summary>
    private void ApplyMomentumThresholdResult(MomentumThresholdResult result, Card card, Character player, Enemy targetEnemy = null)
    {
        if (result == null || player == null) return;
        
        // Draw cards
        if (result.drawCards > 0)
        {
            var deckMgr = CombatDeckManager.Instance;
            if (deckMgr != null)
            {
                deckMgr.DrawCards(result.drawCards);
                Debug.Log($"<color=cyan>[Momentum Effect] Drew {result.drawCards} card(s)</color>");
            }
        }
        
        // Additional momentum gain (only if not already handled in ApplyOnPlayEffects)
        // This is for cases where momentum is gained separately from card effects
        // If momentum threshold says "Gain X instead of Y", that's handled in ApplyOnPlayEffects
        // This is for pure additional momentum (not replacing base gain)
        if (result.additionalMomentum > 0)
        {
            // Check if this is a replacement ("instead of") or additional gain
            CardDataExtended extendedCard = GetCardDataExtended(card);
            bool isReplacement = false;
            if (extendedCard != null && !string.IsNullOrEmpty(extendedCard.momentumEffectDescription))
            {
                // Check if effect text says "instead of"
                isReplacement = extendedCard.momentumEffectDescription.ToLower().Contains("instead of");
            }
            
            if (!isReplacement)
            {
                var stackSystem = StackSystem.Instance;
                if (stackSystem != null)
                {
                    int bonus = player.momentumGainBonus;
                    int totalGain = result.additionalMomentum + bonus;
                    stackSystem.AddStacks(StackType.Momentum, totalGain);
                    Debug.Log($"<color=cyan>[Momentum Effect] Gained {result.additionalMomentum} additional momentum (Total: {player.GetMomentum()})</color>");
                }
            }
            else
            {
                Debug.Log($"<color=cyan>[Momentum Effect] Momentum gain replacement handled in ApplyOnPlayEffects</color>");
            }
        }
        
        // Temporary stat boosts
        PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            StatusEffectManager statusManager = playerDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                if (result.tempStrength > 0)
                {
                    var strengthEffect = new StatusEffect(StatusEffectType.Strength, "TempStrength", result.tempStrength, -1, false);
                    statusManager.AddStatusEffect(strengthEffect);
                    Debug.Log($"<color=cyan>[Momentum Effect] Gained {result.tempStrength} temporary Strength</color>");
                }
                
                if (result.tempDexterity > 0)
                {
                    var dexEffect = new StatusEffect(StatusEffectType.Dexterity, "TempDexterity", result.tempDexterity, -1, false);
                    statusManager.AddStatusEffect(dexEffect);
                    Debug.Log($"<color=cyan>[Momentum Effect] Gained {result.tempDexterity} temporary Dexterity</color>");
                }
                
                if (result.tempIntelligence > 0)
                {
                    var intEffect = new StatusEffect(StatusEffectType.Intelligence, "TempIntelligence", result.tempIntelligence, -1, false);
                    statusManager.AddStatusEffect(intEffect);
                    Debug.Log($"<color=cyan>[Momentum Effect] Gained {result.tempIntelligence} temporary Intelligence</color>");
                }
            }
        }
        
        // Energy gain (next turn)
        if (result.energyGain > 0)
        {
            // Store energy gain for next turn
            // This would need to be tracked in Character or CombatDeckManager
            // For now, we'll add it directly to current energy
            player.RestoreMana(result.energyGain);
            Debug.Log($"<color=cyan>[Momentum Effect] Gained {result.energyGain} Energy</color>");
        }
        
        // Apply ailments to all enemies
        if (result.applyBleed > 0)
        {
            var allEnemies = GetAllActiveEnemies();
            // For momentum effects, use applyBleed as the physical damage amount
            // This represents the physical damage that would cause bleeding
            float physicalDmg = result.applyBleed;
            foreach (var enemy in allEnemies)
            {
                StatusEffect bleedEffect = StatusEffectFactory.CreateBleeding(physicalDmg, 5);
                ApplyStatusEffectToEnemy(enemy, bleedEffect);
            }
            Debug.Log($"<color=cyan>[Momentum Effect] Applied Bleed to all enemies (physical damage: {physicalDmg})</color>");
        }
        
        // Double next attack (store flag on player)
        if (result.doubleNextAttack)
        {
            // This would need to be tracked in Character or CombatDeckManager
            // For now, we'll apply it immediately to the current attack
            // TODO: Implement "next attack" tracking
            Debug.Log($"<color=cyan>[Momentum Effect] Next attack will deal double damage (TODO: implement next attack tracking)</color>");
        }
        
        // Adrenaline Burst special effect
        if (result.triggerAdrenalineBurst)
        {
            // Parse damage from effect text: "deal 3 (+dex/6) damage to all enemies"
            CardDataExtended extendedCard = GetCardDataExtended(card);
            if (extendedCard != null && !string.IsNullOrEmpty(extendedCard.momentumEffectDescription))
            {
                string desc = extendedCard.momentumEffectDescription;
                var match = System.Text.RegularExpressions.Regex.Match(desc, @"deal\s+(\d+)\s*\([^)]*dex/(\d+)[^)]*\)\s*damage\s+to\s+all\s+enemies", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int baseDmg) && int.TryParse(match.Groups[2].Value, out int dexDiv))
                    {
                        float dexBonus = player.dexterity / (float)dexDiv;
                        float totalDmg = baseDmg + dexBonus;
                        
                        var allEnemies = GetAllActiveEnemies();
                        foreach (var enemy in allEnemies)
                        {
                            int idx = FindActiveEnemyIndex(enemy);
                            if (idx >= 0 && combatManager != null)
                            {
                                combatManager.PlayerAttackEnemy(idx, totalDmg, card);
                            }
                        }
                        Debug.Log($"<color=orange>[Momentum Effect] Adrenaline Burst: Dealt {totalDmg:F1} damage to all enemies</color>");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Parse dexterity divisor from card description (e.g., "+Dex/4" -> 4)
    /// </summary>
    private float ParseDexterityDivisor(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0f;
        
        var match = System.Text.RegularExpressions.Regex.Match(description, @"\([^)]*Dex\s*/\s*(\d+)[^)]*\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return float.Parse(match.Groups[1].Value);
        }
        return 0f;
    }
    
    /// <summary>
    /// Calculate total guard from a card + player stats.
    /// </summary>
    private float CalculateGuard(Card card, Character player)
    {
        float baseGuard = card.baseGuard;
        
        if (player == null)
        {
            return baseGuard;
        }
        
        // Add attribute scaling for guard (includes both multiplicative and divisor scaling)
        if (card.guardScaling != null)
        {
            float scalingBonus = card.guardScaling.CalculateScalingBonus(player);
            baseGuard += scalingBonus;
            
            Debug.Log($"[Guard Calculation] {card.cardName}: Base={card.baseGuard}, ScalingBonus={scalingBonus:F2}, Total={baseGuard:F2}");
        }
        
        // Apply guard effectiveness increased modifier from warrants/character stats
        // Only apply if card has "Guard" tag or is a Guard card type
        if (card.cardType == CardType.Guard || (card.tags != null && (card.tags.Contains("Guard") || card.tags.Contains("guard"))))
        {
            var statsData = new CharacterStatsData(player);
            float guardEffectivenessIncreased = statsData.guardEffectivenessIncreased;
            
            if (guardEffectivenessIncreased > 0f)
            {
                // Apply as increased modifier: finalGuard = baseGuard * (1 + guardEffectivenessIncreased / 100)
                float effectivenessMultiplier = 1f + (guardEffectivenessIncreased / 100f);
                baseGuard *= effectivenessMultiplier;
                
                Debug.Log($"[Guard Calculation] {card.cardName}: Applied {guardEffectivenessIncreased}% guard effectiveness. Final={baseGuard:F2}");
            }
        }
        
        return baseGuard;
    }
    
    /// <summary>
    /// Get target position for enemy (for animation target).
    /// </summary>
    public Vector3 GetEnemyScreenPosition(Enemy enemy)
    {
        if (combatManager == null || combatManager.GetActiveEnemyDisplays() == null)
        {
            // Fallback position
            return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
        }
        
        // Find the display for this enemy
        foreach (var enemyDisplay in combatManager.GetActiveEnemyDisplays())
        {
            if (enemyDisplay.GetCurrentEnemy() == enemy)
            {
                // Return the screen position of the enemy display
                RectTransform rectTransform = enemyDisplay.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    return rectTransform.position;
                }
            }
        }
        
        // Fallback
        return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
    }
    
    /// <summary>
    /// Get the first available enemy (for auto-targeting).
    /// </summary>
    public Enemy GetFirstAvailableEnemy()
    {
        if (combatManager == null)
            return null;
        
        List<Enemy> activeEnemies = combatManager.GetActiveEnemies();
        if (activeEnemies != null && activeEnemies.Count > 0)
        {
            return activeEnemies[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// Convert DamageType to DamageNumberType for animation.
    /// </summary>
    private DamageNumberType ConvertDamageType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                return DamageNumberType.Normal;
            case DamageType.Fire:
                return DamageNumberType.Fire;
            case DamageType.Cold:
                return DamageNumberType.Cold;
            case DamageType.Lightning:
                return DamageNumberType.Lightning;
            case DamageType.Chaos:
                return DamageNumberType.Normal; // Chaos uses Normal for now
            default:
                return DamageNumberType.Normal;
        }
    }
    
    /// <summary>
    /// Coroutine to apply multi-hit attack via CombatDisplayManager with delays between hits.
    /// </summary>
    private IEnumerator ApplyMultiHitAttack(CombatDisplayManager combatMgr, int enemyIndex, float damage, int hits, Card card, Vector3 targetPosition)
    {
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        Character player = characterManager != null && characterManager.HasCharacter() ? characterManager.GetCurrentCharacter() : null;
        
        // Calculate damage breakdown for status effects (same for all hits)
        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, damage);
        
        // Get enemy from index for status effects
        Enemy targetEnemy = null;
        var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        if (enemyIndex >= 0 && enemyIndex < enemyDisplays.Length && enemyDisplays[enemyIndex] != null)
        {
            targetEnemy = enemyDisplays[enemyIndex].GetCurrentEnemy();
        }
        
        Debug.Log($"<color=cyan>[Multi-Hit Coroutine] Starting {hits} hits with 0.3s delay between each</color>");
        
        // Apply Vulnerability multiplier only to first hit (will be consumed after)
        float firstHitDamage = damage;
        bool vulnerabilityConsumed = false;
        if (targetEnemy != null)
        {
            try
            {
                // Reuse existing enemyDisplays variable
                foreach (var d in enemyDisplays)
                {
                    if (d != null && d.GetCurrentEnemy() == targetEnemy)
                    {
                        var statusManager = d.GetComponent<StatusEffectManager>();
                        if (statusManager != null)
                        {
                            float vulnMultiplier = statusManager.GetVulnerabilityDamageMultiplier();
                            if (vulnMultiplier > 1f)
                            {
                                firstHitDamage *= vulnMultiplier;
                                Debug.Log($"  [Vulnerability] Applying multiplier to first hit: x{vulnMultiplier:F2} (20% more damage)");
                            }
                        }
                        break;
                    }
                }
            }
            catch { /* safe guard */ }
        }
        
        for (int hit = 0; hit < hits; hit++)
        {
            Debug.Log($"<color=yellow>[Multi-Hit Coroutine] Processing hit {hit + 1}/{hits}</color>");
            
            // Trigger nudge animation FIRST (before damage) so it's visible
            if (playerDisplay != null)
            {
                playerDisplay.TriggerAttackNudge();
                Debug.Log($"<color=green>[Multi-Hit] Triggered nudge animation for hit {hit + 1}</color>");
            }
            
            // Small delay to let nudge animation start
            yield return new WaitForSeconds(0.05f);
            
            // Use vulnerability multiplier only for first hit
            float hitDamage = (hit == 0) ? firstHitDamage : damage;
            
            // Apply damage via CombatDisplayManager (handles floating text internally)
            combatMgr.PlayerAttackEnemy(enemyIndex, hitDamage, card);
            Debug.Log($"<color=red>  ⚔️ Hit {hit + 1}/{hits}: Dealt {hitDamage:F0} damage via CombatDisplayManager</color>");
            
            // Consume Vulnerability after first hit
            if (hit == 0 && targetEnemy != null && !vulnerabilityConsumed)
            {
                try
                {
                    // Reuse existing enemyDisplays variable
                    foreach (var d in enemyDisplays)
                    {
                        if (d != null && d.GetCurrentEnemy() == targetEnemy)
                        {
                            var statusManager = d.GetComponent<StatusEffectManager>();
                            if (statusManager != null)
                            {
                                statusManager.ConsumeVulnerability();
                                vulnerabilityConsumed = true;
                            }
                            break;
                        }
                    }
                }
                catch { /* safe guard */ }
            }
            
            // Apply automatic status effects (only on first hit to avoid stacking issues)
            if (hit == 0 && targetEnemy != null)
            {
                ApplyAutomaticStatusEffects(targetEnemy, damageBreakdown, card);
            }
            
            // Wait before next hit (except for the last one)
            // Use 0.3s delay to allow nudge animation (0.3s) to complete before next hit
            if (hit < hits - 1)
            {
                Debug.Log($"<color=cyan>[Multi-Hit] Waiting 0.3s before next hit...</color>");
                yield return new WaitForSeconds(0.3f); // 0.3 second delay between hits (matches nudge duration)
            }
        }
        
        Debug.Log($"<color=cyan>[Multi-Hit Coroutine] Completed all {hits} hits</color>");
    }
    
    /// <summary>
    /// Coroutine to apply multi-hit AoE (each enemy gets hit multiple times)
    /// </summary>
    private IEnumerator ApplyMultiHitAoE(Card card, Character player, Vector3 targetScreenPosition, int hitCount, float totalDamage, bool isDelayed)
    {
        Debug.Log($"<color=yellow>[Multi-Hit AoE] Starting {hitCount} hits on all enemies (damage per hit: {totalDamage})</color>");
        
        var allEnemies = GetAllActiveEnemies();
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        var combatDisplayManager = combatManager != null ? combatManager : FindFirstObjectByType<CombatDisplayManager>();
        
        if (combatDisplayManager == null)
        {
            Debug.LogError("[Multi-Hit AoE] CombatDisplayManager not found!");
            yield break;
        }
        
        // Calculate damage breakdown for status effects (same for all hits)
        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
        
        // Get all enemy displays once to avoid repeated lookups
        var allEnemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        
        for (int hit = 0; hit < hitCount; hit++)
        {
            Debug.Log($"<color=yellow>[Multi-Hit AoE] Hit {hit + 1}/{hitCount} on all enemies</color>");
            
            // Trigger nudge animation
            if (playerDisplay != null)
            {
                playerDisplay.TriggerAttackNudge();
            }
            
            yield return new WaitForSeconds(0.05f);
            
            // Apply damage to each enemy for this hit
            foreach (var enemy in allEnemies)
            {
                if (enemy == null || enemy.currentHealth <= 0) continue;
                
                int enemyIdx = FindActiveEnemyIndex(enemy);
                if (enemyIdx >= 0)
                {
                    // Apply Vulnerability multiplier only to first hit per enemy
                    float enemyDamage = totalDamage;
                    if (hit == 0)
                    {
                        try
                        {
                            foreach (var d in allEnemyDisplays)
                            {
                                if (d != null && d.GetCurrentEnemy() == enemy)
                                {
                                    var statusManager = d.GetComponent<StatusEffectManager>();
                                    if (statusManager != null)
                                    {
                                        float vulnMultiplier = statusManager.GetVulnerabilityDamageMultiplier();
                                        if (vulnMultiplier > 1f)
                                        {
                                            enemyDamage *= vulnMultiplier;
                                            Debug.Log($"  [Vulnerability] Applying multiplier to {enemy.enemyName} first hit: x{vulnMultiplier:F2} (20% more damage)");
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        catch { /* safe guard */ }
                    }
                    
                    combatDisplayManager.PlayerAttackEnemy(enemyIdx, enemyDamage, card);
                    
                    // Consume Vulnerability after first hit
                    if (hit == 0)
                    {
                        try
                        {
                            foreach (var d in allEnemyDisplays)
                            {
                                if (d != null && d.GetCurrentEnemy() == enemy)
                                {
                                    var statusManager = d.GetComponent<StatusEffectManager>();
                                    if (statusManager != null)
                                    {
                                        statusManager.ConsumeVulnerability();
                                    }
                                    break;
                                }
                            }
                        }
                        catch { /* safe guard */ }
                    }
                    
                    // Apply automatic status effects (only on first hit to avoid stacking issues)
                    if (hit == 0)
                    {
                        ApplyAutomaticStatusEffects(enemy, damageBreakdown, card);
                    }
                }
            }
            
            // Wait before next hit (except for the last one)
            if (hit < hitCount - 1)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        Debug.Log($"<color=yellow>[Multi-Hit AoE] Completed all {hitCount} hits on all enemies</color>");
        
        // End AoE attack
        StartCoroutine(DelayedWaveCompletionCheck(combatDisplayManager));
    }
    
    /// <summary>
    /// Coroutine to apply multi-hit to random targets (each random target gets hit multiple times)
    /// </summary>
    private IEnumerator ApplyMultiHitRandomTargets(List<Enemy> shuffledEnemies, int targetsToHit, float totalDamage, int hitCount, Card card, Vector3 targetScreenPosition)
    {
        Debug.Log($"<color=yellow>[Multi-Hit Random] Starting {hitCount} hits on {targetsToHit} random enemies</color>");
        
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        Character player = characterManager != null && characterManager.HasCharacter() ? characterManager.GetCurrentCharacter() : null;
        
        // Calculate damage breakdown for status effects (same for all hits)
        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
        
        // Get all enemy displays once to avoid repeated lookups
        var allEnemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        
        for (int hit = 0; hit < hitCount; hit++)
        {
            Debug.Log($"<color=yellow>[Multi-Hit Random] Hit {hit + 1}/{hitCount} on {targetsToHit} random enemies</color>");
            
            // Trigger nudge animation
            if (playerDisplay != null)
            {
                playerDisplay.TriggerAttackNudge();
            }
            
            yield return new WaitForSeconds(0.05f);
            
            // Apply damage to each random target for this hit
            for (int i = 0; i < targetsToHit; i++)
            {
                Enemy randomEnemy = shuffledEnemies[i];
                if (randomEnemy == null || randomEnemy.currentHealth <= 0) continue;
                
                int enemyIdx = FindActiveEnemyIndex(randomEnemy);
                if (enemyIdx >= 0 && combatManager != null)
                {
                    // Apply Vulnerability multiplier only to first hit per enemy
                    float enemyDamage = totalDamage;
                    if (hit == 0)
                    {
                        try
                        {
                            foreach (var d in allEnemyDisplays)
                            {
                                if (d != null && d.GetCurrentEnemy() == randomEnemy)
                                {
                                    var statusManager = d.GetComponent<StatusEffectManager>();
                                    if (statusManager != null)
                                    {
                                        float vulnMultiplier = statusManager.GetVulnerabilityDamageMultiplier();
                                        if (vulnMultiplier > 1f)
                                        {
                                            enemyDamage *= vulnMultiplier;
                                            Debug.Log($"  [Vulnerability] Applying multiplier to {randomEnemy.enemyName} first hit: x{vulnMultiplier:F2} (20% more damage)");
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        catch { /* safe guard */ }
                    }
                    
                    combatManager.PlayerAttackEnemy(enemyIdx, enemyDamage, card);
                    
                    // Consume Vulnerability after first hit
                    if (hit == 0)
                    {
                        try
                        {
                            foreach (var d in allEnemyDisplays)
                            {
                                if (d != null && d.GetCurrentEnemy() == randomEnemy)
                                {
                                    var statusManager = d.GetComponent<StatusEffectManager>();
                                    if (statusManager != null)
                                    {
                                        statusManager.ConsumeVulnerability();
                                    }
                                    break;
                                }
                            }
                        }
                        catch { /* safe guard */ }
                    }
                    
                    // Apply automatic status effects (only on first hit to avoid stacking issues)
                    if (hit == 0)
                    {
                        ApplyAutomaticStatusEffects(randomEnemy, damageBreakdown, card);
                    }
                }
            }
            
            // Wait before next hit (except for the last one)
            if (hit < hitCount - 1)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        Debug.Log($"<color=yellow>[Multi-Hit Random] Completed all {hitCount} hits on {targetsToHit} random enemies</color>");
    }
    
    /// <summary>
    /// Coroutine to apply multi-hit attack in fallback path with delays between hits.
    /// </summary>
    private IEnumerator ApplyMultiHitFallback(Enemy targetEnemy, float damage, int hits, bool ignoreGuardArmor, Card card, Vector3 targetPosition)
    {
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        DamageNumberType damageNumberType = ConvertDamageType(card.primaryDamageType);
        Character player = characterManager != null && characterManager.HasCharacter() ? characterManager.GetCurrentCharacter() : null;
        
        // Calculate damage breakdown for status effects (same for all hits)
        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, damage);
        
        for (int hit = 0; hit < hits; hit++)
        {
            // Apply damage for each hit
            targetEnemy.TakeDamage(damage, ignoreGuardArmor);
            
            Debug.Log($"<color=red>  ⚔️ Hit {hit + 1}/{hits}: Dealt {damage:F0} damage to {targetEnemy.enemyName}</color>");
            Debug.Log($"<color=red>  💔 Target HP AFTER: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}</color>");
            
            // Apply automatic status effects (only on first hit to avoid stacking issues)
            if (hit == 0)
            {
                ApplyAutomaticStatusEffects(targetEnemy, damageBreakdown, card);
            }
            
            if (showDetailedLogs)
            {
                Debug.Log($"  ⚔️ Hit {hit + 1}/{hits}: Dealt {damage:F0} damage to {targetEnemy.enemyName}");
                Debug.Log($"  💔 {targetEnemy.enemyName} HP: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}");
            }
            
            // Show damage number for each hit (with slight offset for visual clarity)
            if (animationManager != null)
            {
                Vector3 hitPosition = targetPosition;
                if (hits > 1)
                {
                    // Offset each hit slightly to the right for visual separation
                    hitPosition.x += hit * 20f;
                }
                animationManager.ShowDamageNumber(damage, hitPosition, damageNumberType);
            }
            
            // Trigger nudge animation for each hit
            if (playerDisplay != null)
            {
                playerDisplay.TriggerAttackNudge();
            }
            
            // Update enemy display after each hit
            UpdateEnemyDisplay(targetEnemy);
            
            // Wait before next hit (except for the last one)
            if (hit < hits - 1)
            {
                yield return new WaitForSeconds(0.2f); // 0.2 second delay between hits
            }
        }
    }
    
    /// <summary>
    /// Advance turn for temporary stat boosts (called at end of player turn)
    /// </summary>
    public void AdvanceTurn()
    {
        if (activeStatBoosts.Count == 0) return;
        
        bool anyRemoved = false;
        for (int i = activeStatBoosts.Count - 1; i >= 0; i--)
        {
            var tracker = activeStatBoosts[i];
            if (tracker.character == null)
            {
                activeStatBoosts.RemoveAt(i);
                anyRemoved = true;
                continue;
            }
            
            tracker.remainingTurns--;
            if (tracker.remainingTurns <= 0)
            {
                // Remove the stat boost
                if (tracker.character.warrantStatModifiers != null && 
                    tracker.character.warrantStatModifiers.ContainsKey(tracker.statName))
                {
                    tracker.character.warrantStatModifiers[tracker.statName] -= tracker.value;
                    
                    // Remove the key if it reaches zero or below
                    if (tracker.character.warrantStatModifiers[tracker.statName] <= 0f)
                    {
                        tracker.character.warrantStatModifiers.Remove(tracker.statName);
                    }
                    
                    Debug.Log($"[CardEffectProcessor] Removed temporary stat boost: {tracker.statName} = -{tracker.value}% (expired)");
                }
                
                activeStatBoosts.RemoveAt(i);
                anyRemoved = true;
            }
        }
        
        if (anyRemoved)
        {
            Debug.Log($"[CardEffectProcessor] Advanced turn for temporary stat boosts. Remaining: {activeStatBoosts.Count}");
        }
    }
    
    /// <summary>
    /// Clear all temporary stat boosts (called when combat ends)
    /// </summary>
    public void ClearAllTemporaryStatBoosts()
    {
        foreach (var tracker in activeStatBoosts)
        {
            if (tracker.character != null && tracker.character.warrantStatModifiers != null &&
                tracker.character.warrantStatModifiers.ContainsKey(tracker.statName))
            {
                tracker.character.warrantStatModifiers[tracker.statName] -= tracker.value;
                if (tracker.character.warrantStatModifiers[tracker.statName] <= 0f)
                {
                    tracker.character.warrantStatModifiers.Remove(tracker.statName);
                }
            }
        }
        activeStatBoosts.Clear();
        Debug.Log("[CardEffectProcessor] Cleared all temporary stat boosts");
    }
    
    /// <summary>
    /// Play visual effect for a card (projectile or impact)
    /// </summary>
    private void PlayCardEffect(Card card, Enemy targetEnemy, int enemyDisplayIndex, bool isAoE, 
        float? damageAmount = null, Vector3? damagePosition = null, bool isCritical = false,
        System.Action onProjectileHit = null)
    {
        if (effectManager == null)
        {
            Debug.LogWarning("[CardEffectProcessor] CombatEffectManager not found! Effects will not play.");
            return;
        }
        
        if (card == null)
        {
            Debug.LogWarning("[CardEffectProcessor] Card is null! Cannot play effect.");
            return;
        }
        
        if (targetEnemy == null)
        {
            Debug.LogWarning($"[CardEffectProcessor] Target enemy is null for card {card.cardName}! Cannot play effect.");
            return;
        }
        
        Debug.Log($"[CardEffectProcessor] Playing effect for card: '{card.cardName}' (isProjectile: {IsProjectileCard(card)}, isAoE: {isAoE})");
        
        // For AoE cards, check if this card has an Area effect - if so, skip per-enemy effects
        // (Area effect is already played once at AoEAreaIndicator location)
        if (isAoE)
        {
            if (effectManager == null)
            {
                Debug.LogWarning($"[CardEffectProcessor] effectManager is null - cannot check for Area effects");
            }
            else if (effectManager.GetEffectsDatabase() == null)
            {
                Debug.LogWarning($"[CardEffectProcessor] EffectsDatabase is null - cannot check for Area effects");
            }
            else
            {
                var effectsDatabase = effectManager.GetEffectsDatabase();
                
                // Priority 1: Check for card-specific Area effect
                EffectData cardEffect = effectsDatabase.FindEffectByCardName(card.cardName);
                Debug.Log($"[CardEffectProcessor] Checking for Area effect - card name: '{card.cardName}', found effect: {(cardEffect != null ? cardEffect.effectName : "NULL")}, effectType: {(cardEffect != null ? cardEffect.effectType.ToString() : "N/A")}");
                
                if (cardEffect != null && cardEffect.effectType == VisualEffectType.Area)
                {
                    Debug.Log($"[CardEffectProcessor] ✓ Card '{card.cardName}' has Area effect '{cardEffect.effectName}' - skipping per-enemy effect (Area effect already played at AoEAreaIndicator)");
                    return; // Area effect was already played once at AoEAreaIndicator location
                }
                
                // Priority 2: Check if damage type has Area effect
                DamageType primaryDamageType = card.primaryDamageType;
                if (primaryDamageType == DamageType.None)
                {
                    primaryDamageType = DamageType.Physical;
                }
                
                var query = new EffectQuery
                {
                    effectType = VisualEffectType.Area,
                    damageType = primaryDamageType,
                    isCritical = false
                };
                
                EffectData areaEffect = effectsDatabase.FindEffect(query);
                Debug.Log($"[CardEffectProcessor] Checking for Area effect by damage type - damageType: {primaryDamageType}, found effect: {(areaEffect != null ? areaEffect.effectName : "NULL")}");
                
                if (areaEffect != null)
                {
                    Debug.Log($"[CardEffectProcessor] ✓ Card '{card.cardName}' damage type ({primaryDamageType}) has Area effect '{areaEffect.effectName}' - skipping per-enemy effect (Area effect already played at AoEAreaIndicator)");
                    return; // Area effect was already played once at AoEAreaIndicator location
                }
                
                Debug.Log($"[CardEffectProcessor] No Area effect found for card '{card.cardName}' - will play per-enemy effects");
            }
        }
        
        // Get enemy display transform
        Transform enemyTransform = null;
        if (combatManager != null)
        {
            var activeDisplays = combatManager.GetActiveEnemyDisplays();
            if (enemyDisplayIndex >= 0 && enemyDisplayIndex < activeDisplays.Count)
            {
                var enemyDisplay = activeDisplays[enemyDisplayIndex];
                if (enemyDisplay != null && enemyDisplay.GetCurrentEnemy() == targetEnemy)
                {
                    enemyTransform = enemyDisplay.transform;
                }
            }
        }
        
        // Fallback: find enemy display by enemy
        if (enemyTransform == null)
        {
            var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var display in enemyDisplays)
            {
                if (display != null && display.GetCurrentEnemy() == targetEnemy)
                {
                    enemyTransform = display.transform;
                    break;
                }
            }
        }
        
        if (enemyTransform == null)
        {
            Debug.LogWarning($"[CardEffectProcessor] Could not find enemy display for {targetEnemy.enemyName}");
            return;
        }
        
        // Determine if card is projectile or impact
        bool isProjectile = IsProjectileCard(card);
        
        if (isProjectile)
        {
            // Get player icon transform
            Transform playerIcon = effectManager.FindPlayerCharacterIcon();
            Debug.Log($"[CardEffectProcessor] Player icon found: {playerIcon != null} (name: {(playerIcon != null ? playerIcon.name : "NULL")})");
            Debug.Log($"[CardEffectProcessor] Enemy transform: {enemyTransform != null} (name: {(enemyTransform != null ? enemyTransform.name : "NULL")})");
            
            if (playerIcon != null)
            {
                // Play projectile effect with callback (will be called when projectile hits)
                Debug.Log($"[CardEffectProcessor] Calling PlayProjectileForCard with playerIcon={playerIcon.name}, enemy={enemyTransform.name}");
                effectManager.PlayProjectileForCard(
                    card,
                    playerIcon,
                    enemyTransform,
                    "Weapon",  // Start point (player)
                    "Default", // End point (enemy always uses "Default")
                    isCritical, // isCritical
                    onProjectileHit // Callback for damage application (includes damage numbers)
                );
            }
            else
            {
                Debug.LogWarning("[CardEffectProcessor] Could not find player icon for projectile effect - falling back to impact");
                // Fallback to impact effect
                effectManager.PlayEffectForCard(card, enemyTransform, false);
            }
        }
        else
        {
            // Play impact effect
            effectManager.PlayEffectForCard(card, enemyTransform, false);
        }
    }
    
    /// <summary>
    /// Determine if a card should use projectile or impact effects
    /// </summary>
    private bool IsProjectileCard(Card card)
    {
        if (card == null) return false;
        
        // Check CardDataExtended for scalesWithProjectileWeapon
        CardDataExtended extendedCard = GetCardDataExtended(card);
        if (extendedCard != null && extendedCard.scalesWithProjectileWeapon)
        {
            return true;
        }
        
        // Check tags for projectile/ranged indicators
        if (card.tags != null)
        {
            foreach (string tag in card.tags)
            {
                if (tag != null && (
                    tag.Equals("Projectile", System.StringComparison.OrdinalIgnoreCase) ||
                    tag.Equals("Ranged", System.StringComparison.OrdinalIgnoreCase) ||
                    tag.Equals("Bow", System.StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
        }
        
        // Check card name for common projectile cards
        string cardName = card.cardName ?? "";
        if (cardName.Contains("Fireball", System.StringComparison.OrdinalIgnoreCase) ||
            cardName.Contains("Arrow", System.StringComparison.OrdinalIgnoreCase) ||
            cardName.Contains("Bolt", System.StringComparison.OrdinalIgnoreCase) ||
            cardName.Contains("Shot", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Apply damage and related effects when a projectile hits the target
    /// This is called from the projectile hit callback
    /// </summary>
    private void ApplyProjectileDamage(Card card, Enemy targetEnemy, float totalDamage, bool ignoreGuardArmor, 
        Vector3 targetScreenPosition, Character player)
    {
        if (targetEnemy == null || card == null)
        {
            Debug.LogError("[CardEffectProcessor] ApplyProjectileDamage called with null target or card!");
            return;
        }
        
        // Apply Vulnerability multiplier for projectile damage
        float finalDamage = totalDamage;
        try
        {
            var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var d in enemyDisplays)
            {
                if (d != null && d.GetCurrentEnemy() == targetEnemy)
                {
                    var statusManager = d.GetComponent<StatusEffectManager>();
                    if (statusManager != null)
                    {
                        // Use GetVulnerabilityDamageMultiplier() which returns 1.2f (20% more) and checks if consumed
                        float vulnMultiplier = statusManager.GetVulnerabilityDamageMultiplier();
                        if (vulnMultiplier > 1f)
                        {
                            Debug.Log($"  [Vulnerability] Applying multiplier to {targetEnemy.enemyName}: x{vulnMultiplier:F2} (20% more damage)");
                            finalDamage *= vulnMultiplier;
                        }
                        // Apply Bolster (less damage taken per stack: 2%, max 10 stacks)
                        float bolsterStacks = Mathf.Min(10f, statusManager.GetTotalMagnitude(StatusEffectType.Bolster));
                        if (bolsterStacks > 0f)
                        {
                            float lessMultiplier = Mathf.Clamp01(1f - (0.02f * bolsterStacks));
                            Debug.Log($"  Bolster stacks: {bolsterStacks}, less dmg multiplier: x{lessMultiplier:F2}");
                            finalDamage *= lessMultiplier;
                        }
                    }
                    break;
                }
            }
        }
        catch { /* safe guard */ }
        
        Debug.Log($"[CardEffectProcessor] ⚡ Projectile hit! Applying damage: {finalDamage:F0} to {targetEnemy.enemyName}");
        
        // Apply damage to enemy
        targetEnemy.TakeDamage(finalDamage, ignoreGuardArmor);
        
        Debug.Log($"<color=red>  ⚔️ Dealt {finalDamage:F0} damage to {targetEnemy.enemyName}</color>");
        Debug.Log($"<color=red>  💔 Target HP AFTER: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}</color>");
        
        // Consume Vulnerability after damage is dealt
        try
        {
            var enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var d in enemyDisplays)
            {
                if (d != null && d.GetCurrentEnemy() == targetEnemy)
                {
                    var statusManager = d.GetComponent<StatusEffectManager>();
                    if (statusManager != null)
                    {
                        statusManager.ConsumeVulnerability();
                    }
                    break;
                }
            }
        }
        catch { /* safe guard */ }
        
        // Calculate damage breakdown for status effects
        DamageBreakdown damageBreakdown = CalculateDamageBreakdown(card, player, finalDamage);
        
        // Apply automatic status effects based on damage types
        ApplyAutomaticStatusEffects(targetEnemy, damageBreakdown, card);
        
        // Show damage number
        if (animationManager != null)
        {
            DamageNumberType damageNumberType = ConvertDamageType(card.primaryDamageType);
            animationManager.ShowDamageNumber(totalDamage, targetScreenPosition, damageNumberType);
        }
        
        // Trigger nudge animation
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            playerDisplay.TriggerAttackNudge();
        }
        
        // Update enemy display
        UpdateEnemyDisplay(targetEnemy);
        
        // Apply combo ailments if present
        if (card.comboAilmentId != AilmentId.None)
        {
            switch (card.comboAilmentId)
            {
                case AilmentId.Crumble:
                    if (card.primaryDamageType == DamageType.Physical && card.comboAilmentPortion > 0f)
                    {
                        // Find the display for the specific target enemy to apply Crumble correctly
                        var displays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
                        foreach (var d in displays)
                        {
                            if (d != null && d.GetCurrentEnemy() == targetEnemy)
                            {
                                var statusManager = d.GetComponent<StatusEffectManager>();
                                if (statusManager != null)
                                {
                                    int dur = card.comboAilmentDuration > 0 ? card.comboAilmentDuration : 5;
                                    float stored = totalDamage * card.comboAilmentPortion;
                                    statusManager.ApplyOrStackCrumble(stored, dur);
                                    Debug.Log($"[Crumble] Stored {stored:F0} damage for {dur} turns (structured)");
                                }
                                break;
                            }
                        }
                    }
                    break;
                case AilmentId.Chill:
                    if (targetEnemy != null)
                    {
                        // Calculate damage breakdown to get cold damage
                        DamageBreakdown chillBreakdown = CalculateDamageBreakdown(card, player, totalDamage);
                        int chillDuration = card.comboAilmentDuration > 0 ? card.comboAilmentDuration : 2;
                        
                        // Chilled always applies when cold damage is dealt
                        // Use actual cold damage from breakdown, or fallback to portion if no cold damage
                        float coldDmg = chillBreakdown.cold > 0f ? chillBreakdown.cold : 
                                       (Mathf.Approximately(card.comboAilmentPortion, 0f) ? 20f : card.comboAilmentPortion);
                        
                        StatusEffect chilledEffect = StatusEffectFactory.CreateChilled(coldDmg, chillDuration);
                        ApplyStatusEffectToEnemy(targetEnemy, chilledEffect);
                        Debug.Log($"[Chill] Applied Chill (cold damage: {coldDmg}, dur {chillDuration}) to {targetEnemy.enemyName}");
                    }
                    break;
            }
        }
        
        // Check if enemy is defeated
        if (targetEnemy.currentHealth <= 0)
        {
            Debug.Log($"<color=yellow>💀 {targetEnemy.enemyName} has been defeated!</color>");
            
            // XP grant per enemy kill using area level and rarity multipliers with overlevel penalties
            TryGrantKillExperience(targetEnemy);
            TryGenerateLoot(targetEnemy);
            
            if (combatManager != null)
            {
                combatManager.OnEnemyDefeated?.Invoke(targetEnemy);
            }
        }
    }
}

