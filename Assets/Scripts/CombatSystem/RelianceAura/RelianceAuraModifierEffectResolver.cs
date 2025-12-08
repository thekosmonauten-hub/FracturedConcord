using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Dexiled.CombatSystem;

/// <summary>
/// Resolves and executes Reliance Aura modifier actions
/// Handles complex aura effects that need special logic
/// </summary>
public static class RelianceAuraModifierEffectResolver
{
    /// <summary>
    /// Resolve a Reliance Aura modifier action and execute it
    /// </summary>
    public static void ResolveAction(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (action == null || character == null)
        {
            Debug.LogWarning("[RelianceAuraModifierEffectResolver] Cannot resolve action - null action or character");
            return;
        }

        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();

        switch (action.actionType)
        {
            // Complex Reliance Aura effects
            case ModifierActionType.SpreadStatusOnKill:
                HandleSpreadStatusOnKill(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.ShatterOnKill:
                HandleShatterOnKill(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.CastSpellOnKill:
                HandleCastSpellOnKill(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.ApplyCrumble:
                HandleApplyCrumble(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.DealCrumbleDamage:
                HandleDealCrumbleDamage(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.AddPoisonStacks:
                HandleAddPoisonStacks(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.RollDamagePerTurn:
                HandleRollDamagePerTurn(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.StackDamagePerTurn:
                HandleStackDamagePerTurn(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.ModifyAilmentDurationAndEffect:
                HandleModifyAilmentDurationAndEffect(action, eventContext, character, modifierState, definition);
                break;
            case ModifierActionType.ScaleDiscardPower:
                HandleScaleDiscardPower(action, eventContext, character, modifierState, definition);
                break;
            
            // Fall back to standard resolver for other action types
            default:
                // Check if it's a standard action type
                if (action.actionType < ModifierActionType.SpreadStatusOnKill)
                {
                    ModifierEffectResolver.ResolveAction(action, eventContext, character, modifierState, null);
                }
                else
                {
                    Debug.LogWarning($"[RelianceAuraModifierEffectResolver] Unhandled Reliance Aura action type: {action.actionType}");
                }
                break;
        }
    }
    
    /// <summary>
    /// Spread Status on Kill: Spread ignite/shock/etc. to random enemy when killing an enemy with that status
    /// Used by: Emberwake Echo, Law of Ember
    /// </summary>
    private static void HandleSpreadStatusOnKill(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (eventContext == null || !eventContext.ContainsKey("defeatedEnemy"))
            return;
        
        Enemy defeatedEnemy = eventContext["defeatedEnemy"] as Enemy;
        if (defeatedEnemy == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        string statusType = parameters.ContainsKey("statusType") ? parameters["statusType"].ToString() : "Ignite";
        int spreadCount = parameters.ContainsKey("spreadCount") ? Convert.ToInt32(parameters["spreadCount"]) : 1;
        
        // Check if defeated enemy had the status
        bool hadStatus = false;
        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in enemyDisplays)
        {
            if (display != null && display.GetCurrentEnemy() == defeatedEnemy)
            {
                var statusManager = display.GetStatusEffectManager();
                if (statusManager != null)
                {
                    StatusEffectType status = ParseStatusEffectType(statusType);
                    hadStatus = statusManager.HasStatusEffect(status);
                    
                    if (hadStatus)
                    {
                        // Get the status effect magnitude to spread
                        StatusEffect statusEffect = statusManager.GetStatusEffect(status);
                        float statusMagnitude = statusEffect != null ? statusEffect.magnitude : 0f;
                        
                        // Find random target(s) to spread to
                        var allEnemies = GetAllActiveEnemies();
                        if (allEnemies.Count > 0)
                        {
                            // Remove the defeated enemy from the list
                            allEnemies.Remove(defeatedEnemy);
                            
                            if (allEnemies.Count > 0)
                            {
                                // Shuffle and pick random targets
                                var shuffled = new List<Enemy>(allEnemies);
                                for (int i = 0; i < shuffled.Count; i++)
                                {
                                    var temp = shuffled[i];
                                    int randomIndex = UnityEngine.Random.Range(i, shuffled.Count);
                                    shuffled[i] = shuffled[randomIndex];
                                    shuffled[randomIndex] = temp;
                                }
                                
                                int targetsToSpread = Mathf.Min(spreadCount, shuffled.Count);
                                
                                for (int i = 0; i < targetsToSpread; i++)
                                {
                                    Enemy targetEnemy = shuffled[i];
                                    ApplyStatusToEnemy(targetEnemy, status, statusMagnitude);
                                    Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Spread {statusType} (magnitude: {statusMagnitude}) to {targetEnemy.enemyName}");
                                }
                            }
                        }
                    }
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// Shatter on Kill: Deal damage to adjacent enemies when killing a Chilled/Frozen enemy
    /// Used by: Law of Permafrost
    /// </summary>
    private static void HandleShatterOnKill(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (eventContext == null || !eventContext.ContainsKey("defeatedEnemy"))
            return;
        
        Enemy defeatedEnemy = eventContext["defeatedEnemy"] as Enemy;
        if (defeatedEnemy == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        float damagePercent = parameters.ContainsKey("damagePercent") ? Convert.ToSingle(parameters["damagePercent"]) : 0.03f; // 3% default
        
        // Check if defeated enemy was Chilled or Frozen
        bool wasChilledOrFrozen = false;
        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        EnemyCombatDisplay defeatedDisplay = null;
        
        foreach (var display in enemyDisplays)
        {
            if (display != null && display.GetCurrentEnemy() == defeatedEnemy)
            {
                defeatedDisplay = display;
                var statusManager = display.GetStatusEffectManager();
                if (statusManager != null)
                {
                    wasChilledOrFrozen = statusManager.HasStatusEffect(StatusEffectType.Chill) || 
                                        statusManager.HasStatusEffect(StatusEffectType.Freeze);
                }
                break;
            }
        }
        
        if (wasChilledOrFrozen && defeatedEnemy != null)
        {
            // Get max life of defeated enemy
            float maxLife = defeatedEnemy.maxHealth;
            float shatterDamage = maxLife * damagePercent;
            
            // Find adjacent enemies (enemies in nearby positions)
            var allEnemies = GetAllActiveEnemies();
            allEnemies.Remove(defeatedEnemy);
            
            // For now, damage all remaining enemies (can be enhanced to only damage "adjacent" ones)
            foreach (var enemy in allEnemies)
            {
                if (enemy != null)
                {
                    DealDamageToEnemy(enemy, shatterDamage, DamageType.Cold);
                    Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Shatter dealt {shatterDamage} Cold damage to {enemy.enemyName} ({damagePercent * 100}% of {defeatedEnemy.enemyName}'s max life)");
                }
            }
        }
    }
    
    /// <summary>
    /// Cast Spell on Kill: Cast a spell (lightning bolt) on random enemy when killing a Shocked enemy
    /// Used by: Law of Tempest
    /// </summary>
    private static void HandleCastSpellOnKill(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (eventContext == null || !eventContext.ContainsKey("defeatedEnemy"))
            return;
        
        Enemy defeatedEnemy = eventContext["defeatedEnemy"] as Enemy;
        if (defeatedEnemy == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        float damage = parameters.ContainsKey("damage") ? Convert.ToSingle(parameters["damage"]) : 80f;
        string spellName = parameters.ContainsKey("spellName") ? parameters["spellName"].ToString() : "Lightning Bolt";
        
        // Check if defeated enemy was Shocked
        bool wasShocked = false;
        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        
        foreach (var display in enemyDisplays)
        {
            if (display != null && display.GetCurrentEnemy() == defeatedEnemy)
            {
                var statusManager = display.GetStatusEffectManager();
                if (statusManager != null)
                {
                    wasShocked = statusManager.HasStatusEffect(StatusEffectType.Shocked);
                }
                break;
            }
        }
        
        if (wasShocked)
        {
            // Find random target
            var allEnemies = GetAllActiveEnemies();
            allEnemies.Remove(defeatedEnemy);
            
            if (allEnemies.Count > 0)
            {
                Enemy randomTarget = allEnemies[UnityEngine.Random.Range(0, allEnemies.Count)];
                DealDamageToEnemy(randomTarget, damage, DamageType.Lightning);
                Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Cast {spellName} dealing {damage} Lightning damage to {randomTarget.enemyName}");
            }
        }
    }
    
    /// <summary>
    /// Apply Crumble: Apply Crumble stack to enemy on hit
    /// Used by: Law of Force
    /// </summary>
    private static void HandleApplyCrumble(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (eventContext == null || !eventContext.ContainsKey("targetEnemy"))
            return;
        
        Enemy targetEnemy = eventContext["targetEnemy"] as Enemy;
        if (targetEnemy == null)
            return;
        
        // Apply Crumble stack - track in modifier state using enemy name as key
        if (modifierState != null)
        {
            string crumbleKey = $"Crumble_{targetEnemy.enemyName}";
            int currentStacks = modifierState.ContainsKey(crumbleKey) ? Convert.ToInt32(modifierState[crumbleKey]) : 0;
            modifierState[crumbleKey] = currentStacks + 1;
            
            Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Applied Crumble to {targetEnemy.enemyName} (stacks: {currentStacks + 1})");
        }
    }
    
    /// <summary>
    /// Deal Crumble Damage: Deal extra damage based on stored Crumble value
    /// Used by: Law of Force
    /// </summary>
    private static void HandleDealCrumbleDamage(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (eventContext == null || !eventContext.ContainsKey("targetEnemy"))
            return;
        
        Enemy targetEnemy = eventContext["targetEnemy"] as Enemy;
        if (targetEnemy == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        float damagePercent = parameters.ContainsKey("damagePercent") ? Convert.ToSingle(parameters["damagePercent"]) : 0.10f; // 10% default
        
        // Get Crumble stacks for this enemy
        int crumbleStacks = GetCrumbleStacks(targetEnemy, modifierState);
        
        if (crumbleStacks > 0)
        {
            // Calculate damage based on stored Crumble value
            // For now, using a simple calculation - can be enhanced
            float crumbleDamage = crumbleStacks * damagePercent * 100f; // Example: 10 stacks * 10% * 100 = 100 damage
            
            DealDamageToEnemy(targetEnemy, crumbleDamage, DamageType.Physical);
            Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Dealt {crumbleDamage} extra Physical damage to {targetEnemy.enemyName} (from {crumbleStacks} Crumble stacks)");
        }
    }
    
    /// <summary>
    /// Add Poison Stacks: Apply additional poison stacks when poisoning
    /// Used by: Law of Pale Venin
    /// </summary>
    private static void HandleAddPoisonStacks(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (eventContext == null || !eventContext.ContainsKey("targetEnemy"))
            return;
        
        Enemy targetEnemy = eventContext["targetEnemy"] as Enemy;
        if (targetEnemy == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        int additionalStacks = parameters.ContainsKey("additionalStacks") ? Convert.ToInt32(parameters["additionalStacks"]) : 1;
        
        // This would be called when poison is applied, adding extra stacks
        // The actual poison application happens elsewhere, this just adds bonus stacks
        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in enemyDisplays)
        {
            if (display != null && display.GetCurrentEnemy() == targetEnemy)
            {
                var statusManager = display.GetStatusEffectManager();
                if (statusManager != null && statusManager.HasStatusEffect(StatusEffectType.Poison))
                {
                    // Add additional poison stacks by creating a new poison effect
                    // The StatusEffectManager.AddPoisonEffect will handle stacking automatically
                    StatusEffect poisonStack = new StatusEffect
                    {
                        effectType = StatusEffectType.Poison,
                        magnitude = (float)additionalStacks, // Total magnitude for all stacks
                        duration = 3, // Default duration
                        effectName = "Poison Stack"
                    };
                    statusManager.AddStatusEffect(poisonStack);
                    Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Added {additionalStacks} additional Poison stacks to {targetEnemy.enemyName}");
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// Roll Damage Per Turn: Roll new damage value each turn
    /// Used by: Tempest Flux
    /// </summary>
    private static void HandleRollDamagePerTurn(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (modifierState == null || character == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        float minDamage = parameters.ContainsKey("minDamage") ? Convert.ToSingle(parameters["minDamage"]) : 1f;
        float maxDamage = parameters.ContainsKey("maxDamage") ? Convert.ToSingle(parameters["maxDamage"]) : 140f;
        string damageType = parameters.ContainsKey("damageType") ? parameters["damageType"].ToString() : "Lightning";
        
        // Roll new damage value
        float rolledDamage = UnityEngine.Random.Range(minDamage, maxDamage + 1);
        modifierState["rolledDamage"] = rolledDamage;
        modifierState["damageType"] = damageType;
        
        Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Rolled {rolledDamage} {damageType} damage for this turn ({minDamage}-{maxDamage})");
    }
    
    /// <summary>
    /// Stack Damage Per Turn: Increase damage multiplier each turn (capped)
    /// Used by: Iron Ascent
    /// </summary>
    private static void HandleStackDamagePerTurn(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (modifierState == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        float percentPerTurn = parameters.ContainsKey("percentPerTurn") ? Convert.ToSingle(parameters["percentPerTurn"]) : 1f;
        int maxTurns = parameters.ContainsKey("maxTurns") ? Convert.ToInt32(parameters["maxTurns"]) : 10;
        string damageType = parameters.ContainsKey("damageType") ? parameters["damageType"].ToString() : "Physical";
        
        // Get current turn count
        int currentTurns = modifierState.ContainsKey("turnCount") ? Convert.ToInt32(modifierState["turnCount"]) : 0;
        currentTurns = Mathf.Min(currentTurns + 1, maxTurns);
        modifierState["turnCount"] = currentTurns;
        
        // Calculate damage multiplier
        float damageMultiplier = 1f + (currentTurns * percentPerTurn / 100f);
        modifierState["damageMultiplier"] = damageMultiplier;
        modifierState["damageType"] = damageType;
        
        Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Physical damage multiplier is now {damageMultiplier * 100}% (+{percentPerTurn}% per turn, {currentTurns}/{maxTurns} turns)");
    }
    
    /// <summary>
    /// Modify Ailment Duration and Effect: Increase duration but reduce effectiveness
    /// Used by: Woundweaver Rite
    /// </summary>
    private static void HandleModifyAilmentDurationAndEffect(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (modifierState == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        float durationMultiplier = parameters.ContainsKey("durationMultiplier") ? Convert.ToSingle(parameters["durationMultiplier"]) : 2f; // 100% increased = 2x
        float effectReduction = parameters.ContainsKey("effectReduction") ? Convert.ToSingle(parameters["effectReduction"]) : 0.5f; // 50% reduced = 0.5x
        
        modifierState["ailmentDurationMultiplier"] = durationMultiplier;
        modifierState["ailmentEffectMultiplier"] = effectReduction;
        
        Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Ailments last {durationMultiplier * 100}% longer but deal {effectReduction * 100}% damage");
    }
    
    /// <summary>
    /// Scale Discard Power: Increase discard power based on discarded cards this combat
    /// Used by: Echo of the Unburdened
    /// </summary>
    private static void HandleScaleDiscardPower(ModifierAction action, Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState, RelianceAuraModifierDefinition definition)
    {
        if (modifierState == null || character == null)
            return;
        
        Dictionary<string, object> parameters = action.parameters ?? new Dictionary<string, object>();
        float powerPerDiscard = parameters.ContainsKey("powerPerDiscard") ? Convert.ToSingle(parameters["powerPerDiscard"]) : 1f;
        
        // Track discarded cards (this would need to be tracked elsewhere in the combat system)
        int discardedCount = modifierState.ContainsKey("discardedCount") ? Convert.ToInt32(modifierState["discardedCount"]) : 0;
        
        // Calculate discard power bonus
        float discardPowerBonus = discardedCount * powerPerDiscard;
        modifierState["discardPowerBonus"] = discardPowerBonus;
        
        Debug.Log($"[RelianceAura] {definition?.linkedAuraName}: Discard power bonus is {discardPowerBonus} (from {discardedCount} discarded cards)");
    }
    
    // Helper methods
    
    private static StatusEffectType ParseStatusEffectType(string statusType)
    {
        switch (statusType.ToLower())
        {
            case "ignite": case "burn": return StatusEffectType.Burn;
            case "chill": return StatusEffectType.Chill;
            case "freeze": return StatusEffectType.Freeze;
            case "shock": case "shocked": return StatusEffectType.Shocked;
            case "poison": return StatusEffectType.Poison;
            default: return StatusEffectType.Poison;
        }
    }
    
    private static void ApplyStatusToEnemy(Enemy enemy, StatusEffectType statusType, float magnitude)
    {
        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in enemyDisplays)
        {
            if (display != null && display.GetCurrentEnemy() == enemy)
            {
                var statusManager = display.GetStatusEffectManager();
                if (statusManager != null)
                {
                    StatusEffect effect = new StatusEffect
                    {
                        effectType = statusType,
                        magnitude = magnitude,
                        duration = 3 // Default duration
                    };
                    statusManager.AddStatusEffect(effect);
                }
                break;
            }
        }
    }
    
    private static void DealDamageToEnemy(Enemy enemy, float damage, DamageType damageType)
    {
        // Find CombatDisplayManager or CombatManager to deal damage
        CombatDisplayManager displayManager = UnityEngine.Object.FindFirstObjectByType<CombatDisplayManager>();
        if (displayManager != null)
        {
            var allEnemies = GetAllActiveEnemies();
            int enemyIndex = allEnemies.IndexOf(enemy);
            if (enemyIndex >= 0)
            {
                displayManager.PlayerAttackEnemy(enemyIndex, damage, null);
            }
        }
        else
        {
            // Fallback to direct damage
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
    
    private static List<Enemy> GetAllActiveEnemies()
    {
        List<Enemy> enemies = new List<Enemy>();
        var enemyDisplays = UnityEngine.Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in enemyDisplays)
        {
            if (display != null && display.gameObject.activeInHierarchy)
            {
                Enemy enemy = display.GetCurrentEnemy();
                if (enemy != null && enemy.currentHealth > 0)
                {
                    enemies.Add(enemy);
                }
            }
        }
        return enemies;
    }
    
    private static int GetCrumbleStacks(Enemy enemy, Dictionary<string, object> modifierState)
    {
        if (enemy == null || modifierState == null)
            return 0;
        
        string crumbleKey = $"Crumble_{enemy.enemyName}";
        if (modifierState.ContainsKey(crumbleKey))
        {
            return Convert.ToInt32(modifierState[crumbleKey]);
        }
        return 0;
    }
}

