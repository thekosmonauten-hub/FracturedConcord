using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    }
    
    /// <summary>
    /// Apply a card's effect to a target enemy.
    /// </summary>
    public void ApplyCardToEnemy(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition)
    {
        Debug.Log($"<color=magenta>╔══════════════════════════════════╗</color>");
        Debug.Log($"<color=magenta>║ APPLYING CARD EFFECT DEBUG      ║</color>");
        Debug.Log($"<color=magenta>╚══════════════════════════════════╝</color>");
        
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
        
        Debug.Log($"✓ Card: {card.cardName} (Type: {card.cardType})");
        if (targetEnemy != null)
        {
            Debug.Log($"✓ Target: {targetEnemy.enemyName}");
            Debug.Log($"✓ Target HP BEFORE: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}");
        }
        else if (card.isAoE)
        {
            Debug.Log($"✓ AoE Target: All enemies");
        }
        
        if (showDetailedLogs)
        {
            string targetName = targetEnemy != null ? targetEnemy.enemyName : "All enemies";
            Debug.Log($"<color=cyan>═══ Applying {card.cardName} to {targetName} ═══</color>");
        }
        
        // Check if this is an AoE card
        if (card.isAoE)
        {
            Debug.Log($"<color=orange>⚡ AoE Card detected: {card.cardName} will hit all enemies!</color>");
            ApplyAoECard(card, player, targetScreenPosition);
        }
        else
        {
            // Single target card
            switch (card.cardType)
            {
                case CardType.Attack:
                    ApplyAttackCard(card, targetEnemy, player, targetScreenPosition);
                    break;
                
                case CardType.Guard:
                    ApplyGuardCard(card, player);
                    break;
                
                case CardType.Skill:
                    ApplySkillCard(card, targetEnemy, player, targetScreenPosition);
                    break;
                
                case CardType.Power:
                    ApplyPowerCard(card, player);
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
    private void ApplyAoECard(Card card, Character player, Vector3 targetScreenPosition)
    {
        Debug.Log($"<color=orange>🎯 Applying AoE card: {card.cardName}</color>");
        
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
        
        Debug.Log($"<color=cyan>🔍 AoE Debug: Found {activeDisplays.Count} active displays</color>");
        
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
        
        // BATCH PROCESSING: Apply all AoE damage in one batch to prevent cascading defeat handlers
        // This prevents multiple simultaneous CheckWaveCompletion calls that can cause freezes
        
        // Mark AoE attack as in progress to defer wave completion checks
        combatDisplayManager.StartAoEAttack();
        
        // Calculate damage once for all targets
        float totalDamage = (card.cardType == CardType.Attack || (card.cardType == CardType.Skill && card.baseDamage > 0))
            ? DamageCalculator.CalculateCardDamage(card, player)
            : 0f;
        
        Debug.Log($"<color=green>🎯 AoE will hit all {validTargets.Count} enemies (aoeTargets: {card.aoeTargets}, damage: {totalDamage})</color>");
        
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
            
            // Apply damage
            combatDisplayManager.PlayerAttackEnemy(displayIndex, totalDamage);
            
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
    /// Apply an attack card - deal damage to enemy.
    /// </summary>
    private void ApplyAttackCard(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition)
    {
        Debug.Log($"<color=yellow>→ Attack card detected!</color>");
        
        // Calculate total damage
        float totalDamage = DamageCalculator.CalculateCardDamage(card, player);

        // Prefer CombatDisplayManager routing if we can resolve the enemy index
        int idx = FindActiveEnemyIndex(targetEnemy);
        if (idx >= 0 && combatManager != null)
        {
            combatManager.PlayerAttackEnemy(idx, totalDamage);
            return;
        }

        // Apply Vulnerability multiplier: +15% damage taken per stack
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
                        float stacks = statusManager.GetTotalMagnitude(StatusEffectType.Vulnerable);
                        if (stacks > 0f)
                        {
                            float vulnMultiplier = 1f + (0.15f * stacks);
                            Debug.Log($"  Vulnerable stacks: {stacks}, multiplier: x{vulnMultiplier:F2}");
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
        Debug.Log($"  Calling Enemy.TakeDamage({totalDamage})...");
        
        // Apply damage to enemy
        targetEnemy.TakeDamage(totalDamage);
        
        Debug.Log($"<color=red>  ⚔️ Dealt {totalDamage:F0} damage to {targetEnemy.enemyName}</color>");
        Debug.Log($"<color=red>  💔 Target HP AFTER: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}</color>");
        
        if (showDetailedLogs)
        {
            Debug.Log($"  ⚔️ Dealt {totalDamage:F0} damage to {targetEnemy.enemyName}");
            Debug.Log($"  💔 {targetEnemy.enemyName} HP: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}");
        }
        
        // Show damage number
        if (animationManager != null)
        {
            // Convert DamageType to DamageNumberType
            DamageNumberType damageNumberType = ConvertDamageType(card.primaryDamageType);
            animationManager.ShowDamageNumber(totalDamage, targetScreenPosition, damageNumberType);
        }
        
        // Update enemy display to show new HP
        UpdateEnemyDisplay(targetEnemy);
        
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
        
        // Process generic on-play effects (e.g., Draw)
        ApplyOnPlayEffects(card);
        
        // NEW: Hook - apply structured combo ailment if present
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
                        int chillDuration = card.comboAilmentDuration > 0 ? card.comboAilmentDuration : 2;
                        float chillMagnitude = Mathf.Approximately(card.comboAilmentPortion, 0f) ? 20f : card.comboAilmentPortion;
                        var chillEffect = new StatusEffect(StatusEffectType.Chill, "Chilled", chillMagnitude, chillDuration, true);
                        ApplyStatusEffectToEnemy(targetEnemy, chillEffect);
                        Debug.Log($"[Chill] Applied Chill (mag {chillMagnitude}, dur {chillDuration}) to {targetEnemy.enemyName}");
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
            var canvas = GameObject.FindObjectOfType<Canvas>();
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

        // Prefer rolled rarity on Enemy for XP
        float rarityMultiplier = 1f;
        if (enemy != null)
        {
            switch (enemy.rarity)
            {
                case EnemyRarity.Magic: rarityMultiplier = 1.4f; break;
                case EnemyRarity.Rare: rarityMultiplier = 2.0f; break;
                case EnemyRarity.Unique: rarityMultiplier = 3.0f; break;
                default: rarityMultiplier = 1f; break;
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
    private void ApplyGuardCard(Card card, Character player)
    {
        float guardAmount = CalculateGuard(card, player);
        
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
        
        // Process generic on-play effects (e.g., Draw)
        ApplyOnPlayEffects(card);
    }
    
    /// <summary>
    /// Apply a skill card - various effects.
    /// </summary>
    private void ApplySkillCard(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition)
    {
        // Skills can have both damage and other effects
        if (card.baseDamage > 0)
        {
            ApplyAttackCard(card, targetEnemy, player, targetScreenPosition);
        }
        
        if (card.baseGuard > 0)
        {
            ApplyGuardCard(card, player);
        }
        
        // Process generic on-play effects (e.g., Draw)
        ApplyOnPlayEffects(card);
        
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
    private void ApplyPowerCard(Card card, Character player)
    {
        // TODO: Implement buff system
        Debug.Log($"  💪 Power card applied: {card.cardName}");
        
        // Process generic on-play effects (e.g., Draw)
        ApplyOnPlayEffects(card);
    }

    /// <summary>
    /// Handle generic on-play effects that should apply regardless of card type (e.g., Draw).
    /// </summary>
    private void ApplyOnPlayEffects(Card card)
    {
        if (card == null || card.effects == null) return;
        int drawTotal = 0;
        foreach (var eff in card.effects)
        {
            if (eff != null && eff.effectType == EffectType.Draw)
            {
                drawTotal += Mathf.RoundToInt(eff.value);
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
    }
    
    // REMOVED: Old CalculateDamage() method
    // Now using DamageCalculator.CalculateCardDamage() for consistent damage calculation
    // with proper character modifiers, embossing effects, and debug logging
    
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
        
        // Add attribute scaling for guard
        if (card.guardScaling != null)
        {
            float strBonus = player.strength * card.guardScaling.strengthScaling;
            float dexBonus = player.dexterity * card.guardScaling.dexterityScaling;
            float intBonus = player.intelligence * card.guardScaling.intelligenceScaling;
            
            baseGuard += strBonus + dexBonus + intBonus;
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
}

