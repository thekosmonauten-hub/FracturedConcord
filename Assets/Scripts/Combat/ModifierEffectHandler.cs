using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles modifier effects for enemies during combat
/// </summary>
public static class ModifierEffectHandler
{
    /// <summary>
    /// Check if enemy should queue a delayed action instead of executing immediately
    /// </summary>
    public static bool ShouldDelayAction(Enemy enemy, out int delayTurns)
    {
        delayTurns = 0;
        if (enemy == null || enemy.modifiers == null) return false;
        
        foreach (var modifier in enemy.modifiers)
        {
            if (modifier == null || modifier.effectTypes == null) continue;
            
            if (modifier.effectTypes.Contains(ModifierEffectType.DelayedActions))
            {
                delayTurns = modifier.delayedActionTurns;
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Get damage bonus from delayed actions (for Time-Lagged modifier)
    /// </summary>
    public static float GetDelayedActionDamageBonus(Enemy enemy)
    {
        if (enemy == null || enemy.modifiers == null || enemy.delayedActions == null) return 0f;
        
        float totalBonus = 0f;
        foreach (var modifier in enemy.modifiers)
        {
            if (modifier == null || modifier.effectTypes == null) continue;
            
            if (modifier.effectTypes.Contains(ModifierEffectType.DelayedActions))
            {
                // Bonus per delayed action queued
                totalBonus += enemy.delayedActions.Count * modifier.damageBonusPerDelayedAction;
            }
        }
        return totalBonus;
    }
    
    /// <summary>
    /// Process modifier effects when enemy hits a target
    /// </summary>
    public static void ProcessOnHitEffects(Enemy enemy, EnemyCombatDisplay enemyDisplay, Character target = null, PlayerCombatDisplay playerDisplay = null)
    {
        if (enemy == null || enemy.modifiers == null) return;
        
        foreach (var modifier in enemy.modifiers)
        {
            if (modifier == null || modifier.effectTypes == null) continue;
            
            foreach (var effectType in modifier.effectTypes)
            {
                switch (effectType)
                {
                    case ModifierEffectType.ShockOnHit:
                        ApplyOnHitStatusEffect(enemy, modifier, StatusEffectType.Vulnerable, enemyDisplay, target, playerDisplay);
                        break;
                    case ModifierEffectType.ChillOnHit:
                        ApplyOnHitStatusEffect(enemy, modifier, StatusEffectType.Chill, enemyDisplay, target, playerDisplay);
                        break;
                    case ModifierEffectType.BurnOnHit:
                        ApplyOnHitStatusEffect(enemy, modifier, StatusEffectType.Burn, enemyDisplay, target, playerDisplay);
                        break;
                    case ModifierEffectType.PoisonOnHit:
                        ApplyOnHitStatusEffect(enemy, modifier, StatusEffectType.Poison, enemyDisplay, target, playerDisplay);
                        break;
                    case ModifierEffectType.VulnerableOnHit:
                        ApplyOnHitStatusEffect(enemy, modifier, StatusEffectType.Vulnerable, enemyDisplay, target, playerDisplay);
                        break;
                    case ModifierEffectType.GrantStackOnHit:
                        GrantStacksOnHit(enemy, modifier, enemyDisplay);
                        break;
                }
            }
        }
    }
    
    /// <summary>
    /// Process modifier effects at the start of enemy's turn
    /// </summary>
    public static void ProcessOnTurnStartEffects(Enemy enemy, EnemyCombatDisplay enemyDisplay)
    {
        if (enemy == null || enemy.modifiers == null) return;
        
        foreach (var modifier in enemy.modifiers)
        {
            if (modifier == null || modifier.effectTypes == null) continue;
            
            foreach (var effectType in modifier.effectTypes)
            {
                if (effectType == ModifierEffectType.GrantStackOnTurnStart)
                {
                    GrantStacksOnTurnStart(enemy, modifier, enemyDisplay);
                }
            }
        }
    }
    
    /// <summary>
    /// Check if enemy should take reduced/no extra damage from critical strikes
    /// </summary>
    public static float GetCritDamageMultiplier(Enemy enemy, bool isCritical)
    {
        if (!isCritical || enemy == null || enemy.modifiers == null) return 1f;
        
        float multiplier = 1f;
        bool hasNoExtraCritDamage = false;
        
        foreach (var modifier in enemy.modifiers)
        {
            if (modifier == null || modifier.effectTypes == null) continue;
            
            foreach (var effectType in modifier.effectTypes)
            {
                if (effectType == ModifierEffectType.NoExtraCritDamage)
                {
                    hasNoExtraCritDamage = true;
                    break;
                }
                else if (effectType == ModifierEffectType.ReducedCritDamage)
                {
                    // Could add a parameter for reduction percentage, but for now just use 50% reduction
                    multiplier *= 0.5f;
                }
            }
        }
        
        // If "No Extra Crit Damage", critical hits deal normal damage (1x instead of crit multiplier)
        if (hasNoExtraCritDamage)
        {
            return 1f; // Normal damage, no crit bonus
        }
        
        return multiplier;
    }
    
    private static void ApplyOnHitStatusEffect(Enemy enemy, MonsterModifier modifier, StatusEffectType statusType, 
        EnemyCombatDisplay enemyDisplay, Character target, PlayerCombatDisplay playerDisplay)
    {
        // Use modifier's onHitStatusEffect if it's set, otherwise use the default from effectType
        StatusEffectType effectToApply = modifier.onHitStatusEffect != StatusEffectType.Vulnerable ? modifier.onHitStatusEffect : statusType;
        float magnitude = modifier.onHitStatusMagnitude;
        int duration = modifier.onHitStatusDuration;
        
        // Apply to player (enemy hits player)
        if (target != null && playerDisplay != null)
        {
            var statusManager = playerDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                var effect = new StatusEffect(effectToApply, $"{modifier.modifierName} Effect", magnitude, duration, true);
                statusManager.AddStatusEffect(effect);
                Debug.Log($"[Modifier] {enemy.enemyName} ({modifier.modifierName}) applied {effectToApply} to player!");
            }
        }
    }
    
    private static void GrantStacksOnHit(Enemy enemy, MonsterModifier modifier, EnemyCombatDisplay enemyDisplay)
    {
        if (enemyDisplay == null) return;
        
        // Grant stacks to the enemy when they hit
        if (modifier.turnStartStackAmount > 0)
        {
            var adjustment = ScriptableObject.CreateInstance<StackAdjustmentDefinition>();
            if (modifier.turnStartStackType == StackType.Tolerance)
                adjustment.toleranceStacks = modifier.turnStartStackAmount;
            else if (modifier.turnStartStackType == StackType.Agitate)
                adjustment.agitateStacks = modifier.turnStartStackAmount;
            else if (modifier.turnStartStackType == StackType.Potential)
                adjustment.potentialStacks = modifier.turnStartStackAmount;
            
            enemyDisplay.ApplyStackAdjustment(adjustment);
            Debug.Log($"[Modifier] {enemy.enemyName} ({modifier.modifierName}) gained {modifier.turnStartStackAmount} {modifier.turnStartStackType} stacks on hit!");
        }
    }
    
    private static void GrantStacksOnTurnStart(Enemy enemy, MonsterModifier modifier, EnemyCombatDisplay enemyDisplay)
    {
        if (enemyDisplay == null) return;
        
        // Check if "hit recently" requirement is needed
        if (modifier.turnStartStackRequiresHitRecently)
        {
            // TODO: Track if enemy was hit recently (within last turn)
            // For now, we'll assume they were hit if they have any damage taken
            // This is a simplified check - you may want to add a "hitRecently" flag to Enemy
            if (enemy.currentHealth >= enemy.maxHealth)
            {
                // Enemy at full health, probably wasn't hit recently
                return;
            }
        }
        
        // Grant stacks at turn start
        if (modifier.turnStartStackAmount > 0)
        {
            var adjustment = ScriptableObject.CreateInstance<StackAdjustmentDefinition>();
            if (modifier.turnStartStackType == StackType.Tolerance)
                adjustment.toleranceStacks = modifier.turnStartStackAmount;
            else if (modifier.turnStartStackType == StackType.Agitate)
                adjustment.agitateStacks = modifier.turnStartStackAmount;
            else if (modifier.turnStartStackType == StackType.Potential)
                adjustment.potentialStacks = modifier.turnStartStackAmount;
            
            enemyDisplay.ApplyStackAdjustment(adjustment);
            Debug.Log($"[Modifier] {enemy.enemyName} ({modifier.modifierName}) gained {modifier.turnStartStackAmount} {modifier.turnStartStackType} stacks at turn start!");
        }
    }
}
