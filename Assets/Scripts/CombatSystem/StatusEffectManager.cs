using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages status effects for characters and enemies
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    [Header("Status Effect Lists")]
    public List<StatusEffect> activeStatusEffects = new List<StatusEffect>();
    
    [Header("Visual Settings")]
    public Transform statusEffectContainer;
    public GameObject statusEffectIconPrefab;
    
    [Header("Max Effects")]
    public int maxStatusEffects = 10;
    
    // Events
    public System.Action<StatusEffect> OnStatusEffectAdded;
    public System.Action<StatusEffect> OnStatusEffectRemoved;
    public System.Action<StatusEffect> OnStatusEffectTick;
    
    private void Update()
    {
        UpdateStatusEffects();
    }
    
    /// <summary>
    /// Advance all status effects by one turn (call this at the start of each new turn)
    /// </summary>
    public void AdvanceAllEffectsOneTurn()
    {
        var effectsToAdvance = activeStatusEffects.ToList();
        foreach (var effect in effectsToAdvance)
        {
            try
            {
                // Process DoT damage BEFORE advancing turn (damage happens at turn end)
                // This ensures damage is applied before duration is reduced
                // For turn-based mode, we check if it's a DoT effect type rather than tickInterval
                if (effect.isActive && IsDamageOverTimeEffect(effect.effectType))
                {
                    // This is a damage-over-time effect - process damage now
                    ProcessStatusEffectTick(effect);
                }
                
                // Now advance the turn (reduces duration)
                effect.AdvanceTurn();
                
                // Handle Crumble expiration (deal stored damage)
                if (effect.effectType == StatusEffectType.Crumble && !effect.isActive && effect.magnitude > 0f)
                {
                    float storedDamage = effect.magnitude;
                    Debug.Log($"[Crumble] Expired, dealing {storedDamage} stored physical damage");
                    ApplyDamageToEntity(storedDamage, DamageType.Physical);
                }
                
                // Handle DelayedDamage trigger (damage triggers when duration reaches 0)
                if (effect.effectType == StatusEffectType.DelayedDamage && !effect.isActive && effect.magnitude > 0f)
                {
                    float delayedDamage = effect.magnitude;
                    Debug.Log($"<color=orange>[DelayedDamage] {effect.effectName} triggered! Dealing {delayedDamage} damage!</color>");
                    
                    // Show combat message
                    var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
                    if (combatUI != null)
                    {
                        combatUI.LogMessage($"<color=orange>{effect.effectName}!</color> Delayed damage triggers for {delayedDamage}!");
                    }
                    
                    ApplyDamageToEntity(delayedDamage, DamageType.Physical);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffect] Error processing {effect.effectName}: {ex.Message}\n{ex.StackTrace}");
                // Continue processing other effects even if one fails
            }
        }
        
        // Remove expired effects
        RemoveExpiredEffects();
    }
    
    /// <summary>
    /// Add a status effect to this entity
    /// Handles special stacking rules for different effect types
    /// </summary>
    public bool AddStatusEffect(StatusEffect newEffect)
    {
        if (newEffect == null) return false;
        
        // Check for BuffDenial status effect
        if (!newEffect.isDebuff) // Only check for buffs
        {
            if (HasStatusEffect(StatusEffectType.BuffDenial))
            {
                Debug.Log($"<color=red>[BuffDenial] {newEffect.effectName} was DENIED!</color>");
                
                var combatUI = Object.FindFirstObjectByType<AnimatedCombatUI>();
                if (combatUI != null)
                {
                    combatUI.LogMessage($"<color=red>Buff Denied!</color> {newEffect.effectName} was negated.");
                }
                
                // Remove BuffDenial (consume it after use)
                RemoveStatusEffect(StatusEffectType.BuffDenial);
                
                return false; // Buff was denied, don't add it
            }
        }
        
        // Special handling for effects with unique stacking rules
        switch (newEffect.effectType)
        {
            case StatusEffectType.Bleed:
                return AddBleedingEffect(newEffect);
            
            case StatusEffectType.Poison:
                return AddPoisonEffect(newEffect);
            
            case StatusEffectType.Bolster:
                return AddBolsterEffect(newEffect);
            
            default:
                // Standard handling for other effects
                return AddStandardStatusEffect(newEffect);
        }
    }
    
    /// <summary>
    /// Add Bleeding effect: Only one instance, refreshes if higher magnitude
    /// </summary>
    private bool AddBleedingEffect(StatusEffect newEffect)
    {
        StatusEffect existing = activeStatusEffects.FirstOrDefault(e => e.effectType == StatusEffectType.Bleed);
        
        if (existing != null)
        {
            // If new bleed is higher, replace it and refresh duration
            if (newEffect.magnitude > existing.magnitude)
            {
                existing.magnitude = newEffect.magnitude;
                existing.timeRemaining = newEffect.duration;
                existing.duration = newEffect.duration;
                existing.sourcePhysicalDamage = newEffect.sourcePhysicalDamage;
                Debug.Log($"Bleeding refreshed with higher magnitude: {existing.magnitude} (Duration: {existing.duration})");
                UpdateStatusEffectVisuals();
                return true;
            }
            else
            {
                // Lower bleed, just refresh duration
                existing.timeRemaining = Mathf.Max(existing.timeRemaining, newEffect.duration);
                existing.duration = Mathf.Max(existing.duration, newEffect.duration);
                Debug.Log($"Bleeding duration refreshed: {existing.timeRemaining}");
                UpdateStatusEffectVisuals();
                return true;
            }
        }
        
        // No existing bleed, add new one
        return AddStandardStatusEffect(newEffect);
    }
    
    /// <summary>
    /// Add Poison effect: Stacks independently, each with own duration
    /// </summary>
    private bool AddPoisonEffect(StatusEffect newEffect)
    {
        // Poison stacks independently - always add as new instance
        if (activeStatusEffects.Count >= maxStatusEffects)
        {
            Debug.LogWarning($"Cannot add Poison: Maximum effects reached ({maxStatusEffects})");
            return false;
        }
        
        StatusEffect effect = newEffect.Clone();
        activeStatusEffects.Add(effect);
        ApplyRuntimeStatEffect(effect, true);
        
        Debug.Log($"Added Poison stack: {effect.magnitude} damage/turn (Duration: {effect.duration} turns)");
        UpdateStatusEffectVisuals();
        OnStatusEffectAdded?.Invoke(effect);
        
        return true;
    }
    
    /// <summary>
    /// Add Bolster effect: Stacks up to 10 (20% max damage reduction)
    /// </summary>
    private bool AddBolsterEffect(StatusEffect newEffect)
    {
        StatusEffect existing = activeStatusEffects.FirstOrDefault(e => e.effectType == StatusEffectType.Bolster);
        
        if (existing != null)
        {
            // Stack magnitude, cap at 10 stacks (20% damage reduction)
            float newMagnitude = existing.magnitude + newEffect.magnitude;
            if (newMagnitude > 10f)
            {
                newMagnitude = 10f;
                Debug.Log($"Bolster capped at 10 stacks (20% damage reduction)");
            }
            existing.magnitude = newMagnitude;
            existing.timeRemaining = Mathf.Max(existing.timeRemaining, newEffect.duration);
            existing.duration = Mathf.Max(existing.duration, newEffect.duration);
            Debug.Log($"Bolster stacked: {existing.magnitude} stacks ({existing.magnitude * 2f}% damage reduction)");
            UpdateStatusEffectVisuals();
            return true;
        }
        
        // Cap new bolster at 10 stacks
        if (newEffect.magnitude > 10f)
        {
            newEffect.magnitude = 10f;
        }
        
        return AddStandardStatusEffect(newEffect);
    }
    
    /// <summary>
    /// Add standard status effect (handles most effects)
    /// </summary>
    private bool AddStandardStatusEffect(StatusEffect newEffect)
    {
        // Check if we already have this type of effect
        StatusEffect existingEffect = activeStatusEffects.FirstOrDefault(e => e.effectType == newEffect.effectType);
        
        if (existingEffect != null)
        {
            AccumulateStackAdjustments(existingEffect, newEffect);
            RefreshStatusEffect(existingEffect, newEffect);
            return true;
        }
        
        // Check if we're at the limit
        if (activeStatusEffects.Count >= maxStatusEffects)
        {
            Debug.LogWarning($"Cannot add status effect {newEffect.effectName}: Maximum effects reached ({maxStatusEffects})");
            return false;
        }
        
        // Add the new effect
        StatusEffect effect = newEffect.Clone();
        activeStatusEffects.Add(effect);
        
        ApplyRuntimeStatEffect(effect, true);

        Debug.Log($"Added status effect: {effect.effectName} (Duration: {effect.duration}, Magnitude: {effect.magnitude})");
        
        // Update visuals
        UpdateStatusEffectVisuals();
        
        // Trigger event
        OnStatusEffectAdded?.Invoke(effect);
        
        return true;
    }
    
    /// <summary>
    /// Remove a status effect by type
    /// </summary>
    public bool RemoveStatusEffect(StatusEffectType effectType)
    {
        StatusEffect effect = activeStatusEffects.FirstOrDefault(e => e.effectType == effectType);
        if (effect != null)
        {
            return RemoveStatusEffect(effect);
        }
        return false;
    }
    
    /// <summary>
    /// Remove a specific status effect
    /// </summary>
    public bool RemoveStatusEffect(StatusEffect effect)
    {
        if (effect != null && activeStatusEffects.Contains(effect))
        {
            ApplyRuntimeStatEffect(effect, false);
            activeStatusEffects.Remove(effect);
            Debug.Log($"Removed status effect: {effect.effectName}");
            
            // Update visuals
            UpdateStatusEffectVisuals();
            
            // Trigger event
            OnStatusEffectRemoved?.Invoke(effect);
            
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Remove all status effects
    /// </summary>
    public void ClearAllStatusEffects()
    {
        var effectsToRemove = new List<StatusEffect>(activeStatusEffects);
        foreach (var effect in effectsToRemove)
        {
            RemoveStatusEffect(effect);
        }
    }
    
    
    /// <summary>
    /// Gets a summary of all active status effects for display in UI.
    /// </summary>
    public string GetStatusEffectSummary()
    {
        if (activeStatusEffects.Count == 0)
        {
            return "None";
        }
        
        List<string> effectSummaries = new List<string>();
        foreach (var effect in activeStatusEffects)
        {
            string summary = $"{effect.effectName}";
            if (effect.magnitude > 0)
            {
                summary += $"({effect.magnitude:F0})";
            }
            if (effect.duration > 0)
            {
                summary += $"[{effect.duration}]";
            }
            effectSummaries.Add(summary);
        }
        
        return string.Join(", ", effectSummaries);
    }
    
    /// <summary>
    /// Remove all debuffs
    /// </summary>
    public void ClearAllDebuffs()
    {
        var debuffsToRemove = activeStatusEffects.Where(e => e.isDebuff).ToList();
        foreach (var effect in debuffsToRemove)
        {
            RemoveStatusEffect(effect);
        }
    }
    
    /// <summary>
    /// Remove all buffs
    /// </summary>
    public void ClearAllBuffs()
    {
        var buffsToRemove = activeStatusEffects.Where(e => !e.isDebuff).ToList();
        foreach (var effect in buffsToRemove)
        {
            RemoveStatusEffect(effect);
        }
    }
    
    /// <summary>
    /// Check if this entity has a specific status effect
    /// </summary>
    public bool HasStatusEffect(StatusEffectType effectType)
    {
        return activeStatusEffects.Any(e => e.effectType == effectType && e.isActive);
    }
    
    /// <summary>
    /// Get a specific status effect by type
    /// </summary>
    public StatusEffect GetStatusEffect(StatusEffectType effectType)
    {
        return activeStatusEffects.FirstOrDefault(e => e.effectType == effectType && e.isActive);
    }
    
    /// <summary>
    /// Get the total magnitude of a status effect type (for stacking effects)
    /// </summary>
    public float GetTotalMagnitude(StatusEffectType effectType)
    {
        return activeStatusEffects.Where(e => e.effectType == effectType && e.isActive)
                                  .Sum(e => e.GetCurrentMagnitude());
    }
    
    /// <summary>
    /// Check if a status effect is a damage-over-time effect
    /// </summary>
    private bool IsDamageOverTimeEffect(StatusEffectType effectType)
    {
        return effectType == StatusEffectType.Bleed ||
               effectType == StatusEffectType.Poison ||
               effectType == StatusEffectType.Burn ||
               effectType == StatusEffectType.ChaosDot;
    }
    
    /// <summary>
    /// Notify combat manager that an enemy died from DoT damage
    /// This triggers the same death animation and cleanup as regular combat deaths
    /// </summary>
    private void NotifyCombatManagerOfEnemyDeath(EnemyCombatDisplay enemyDisplay, Enemy enemy)
    {
        if (enemyDisplay == null || enemy == null) return;
        
        // Find the combat manager
        var combatManager = FindFirstObjectByType<CombatDisplayManager>();
        if (combatManager == null)
        {
            Debug.LogWarning("[StatusEffect DoT] CombatDisplayManager not found - cannot trigger death animation");
            return;
        }
        
        // Notify combat manager to handle death (loot, experience, animation, cleanup)
        combatManager.OnEnemyDefeatedByDoT(enemyDisplay, enemy);
    }
    
    /// <summary>
    /// Update all active status effects (called every frame)
    /// For turn-based games, DoT damage is processed in AdvanceAllEffectsOneTurn() instead
    /// </summary>
    private void UpdateStatusEffects()
    {
        var effectsToRemove = new List<StatusEffect>();
        
        foreach (var effect in activeStatusEffects)
        {
            effect.UpdateEffect(Time.deltaTime);
            
            // TURN-BASED MODE: Skip DoT damage ticks in Update loop
            // DoT effects (Bleed, Poison, Burn, etc.) are processed in AdvanceAllEffectsOneTurn()
            // Only process non-damage ticks here (buffs, visual effects, etc.)
            bool isDamageOverTimeEffect = IsDamageOverTimeEffect(effect.effectType);
            
            if (effect.ShouldTick() && !isDamageOverTimeEffect)
            {
                ProcessStatusEffectTick(effect);
                effect.ResetTick();
            }
            
            // Mark for removal if expired
            if (!effect.isActive)
            {
                effectsToRemove.Add(effect);
            }
        }
        
        // Remove expired effects
        foreach (var effect in effectsToRemove)
        {
            RemoveStatusEffect(effect);
        }
    }
    
    /// <summary>
    /// Remove effects that have expired (timeRemaining <= 0)
    /// </summary>
    private void RemoveExpiredEffects()
    {
        var expiredEffects = activeStatusEffects.Where(e => !e.isActive).ToList();
        foreach (var effect in expiredEffects)
        {
            RemoveStatusEffect(effect);
        }
    }
    
    /// <summary>
    /// Process a tick from a status effect
    /// </summary>
    private void ProcessStatusEffectTick(StatusEffect effect)
    {
        Debug.Log($"Status effect tick: {effect.effectName} - {effect.magnitude} {effect.effectType}");
        
        // Trigger event for other systems to handle
        OnStatusEffectTick?.Invoke(effect);
        
        // Handle specific effect types
        switch (effect.effectType)
        {
            case StatusEffectType.Bleed:
                // Bleeding: 70% of physical damage per turn
                ApplyBleedingDamage(effect);
                break;
            case StatusEffectType.Poison:
                // Poison: 30% of (physical + chaos) damage per turn
                ApplyPoisonDamage(effect);
                break;
            case StatusEffectType.Burn:
                // Ignite: 70% of fire damage per turn
                ApplyIgniteDamage(effect);
                break;
            case StatusEffectType.ChaosDot:
                // Chaos DoT: Uses magnitude directly
                ApplyDamageOverTime(effect);
                break;
            case StatusEffectType.Regeneration:
                // Apply healing over time
                ApplyHealingOverTime(effect);
                break;
            case StatusEffectType.ManaRegen:
                // Apply mana regeneration
                ApplyManaRegeneration(effect);
                break;
            case StatusEffectType.Crumble:
                // Crumble does not tick; it's consumed by a shout or expires
                break;
        }
    }
    
    /// <summary>
    /// Apply Bleeding damage: 70% of source physical damage per turn
    /// </summary>
    private void ApplyBleedingDamage(StatusEffect effect)
    {
        // Magnitude is already calculated as 70% of source physical damage
        float damage = effect.magnitude;
        Debug.Log($"Applying Bleeding: {damage} physical damage (70% of {effect.sourcePhysicalDamage} source damage)");
        ApplyDamageToEntity(damage, DamageType.Physical);
    }
    
    /// <summary>
    /// Apply Poison damage: 30% of (physical + chaos) damage per turn
    /// </summary>
    private void ApplyPoisonDamage(StatusEffect effect)
    {
        // Magnitude is already calculated as 30% of (physical + chaos) damage
        float damage = effect.magnitude;
        float totalSource = effect.sourcePhysicalDamage + effect.sourceChaosDamage;
        
        // Get total poison magnitude for display
        float totalPoisonMagnitude = GetTotalMagnitude(StatusEffectType.Poison);
        int poisonStacks = activeStatusEffects.Count(e => e.effectType == StatusEffectType.Poison && e.isActive);
        
        Debug.Log($"Applying Poison stack: {damage} chaos damage (30% of {totalSource} source). Total Poison: {totalPoisonMagnitude} from {poisonStacks} stacks");
        ApplyDamageToEntity(damage, DamageType.Chaos);
    }
    
    /// <summary>
    /// Apply Ignite damage: 70% of fire damage per turn
    /// </summary>
    private void ApplyIgniteDamage(StatusEffect effect)
    {
        // Magnitude is already calculated as 70% of source fire damage
        float damage = effect.magnitude;
        Debug.Log($"Applying Ignite: {damage} fire damage (70% of {effect.sourceFireDamage} source damage)");
        ApplyDamageToEntity(damage, DamageType.Fire);
    }
    
    /// <summary>
    /// Apply damage to the entity (Character or Enemy)
    /// </summary>
    private void ApplyDamageToEntity(float damage, DamageType damageType)
    {
        // Check if this is on PlayerCombatDisplay
        var playerDisplay = GetComponent<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            // Get the character from CharacterManager
            if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
            {
                Character character = CharacterManager.Instance.GetCurrentCharacter();
                if (character != null)
                {
                    character.TakeDamage(damage, damageType);
                    // Display will update automatically via CharacterManager.OnCharacterDamaged event
                    Debug.Log($"[StatusEffect DoT] Applied {damage} {damageType} damage to player");
                    return;
                }
            }
            else
            {
                Debug.LogWarning($"[StatusEffect DoT] PlayerCombatDisplay found but CharacterManager or Character is null!");
            }
        }
        
        // Check if this is on EnemyCombatDisplay
        var enemyDisplay = GetComponent<EnemyCombatDisplay>();
        if (enemyDisplay != null)
        {
            Enemy enemy = enemyDisplay.GetCurrentEnemy();
            if (enemy != null)
            {
                string enemyName = enemy.enemyName;
                float healthBefore = enemy.currentHealth;
                
                // Use the display's TakeDamage method to ensure UI updates properly
                enemyDisplay.TakeDamage(damage);
                
                Debug.Log($"[StatusEffect DoT] Applied {damage} {damageType} damage to {enemyName} (HP: {healthBefore} → {enemy.currentHealth})");
                
                // Check if enemy died from DoT damage
                if (enemy.currentHealth <= 0)
                {
                    Debug.Log($"[StatusEffect DoT] {enemyName} defeated by {damageType} damage!");
                    // Notify the combat manager to handle death properly
                    NotifyCombatManagerOfEnemyDeath(enemyDisplay, enemy);
                }
            }
            return;
        }
        
        Debug.LogWarning($"[StatusEffect DoT] Could not find PlayerCombatDisplay or EnemyCombatDisplay to apply {damage} {damageType} damage!");
    }
    
    /// <summary>
    /// Apply damage over time from a status effect
    /// </summary>
    private void ApplyDamageOverTime(StatusEffect effect)
    {
        // This integrates with the character/enemy damage system
        Debug.Log($"Applying {effect.magnitude} {effect.damageType} DoT from {effect.effectName}");
        
        // Example: Get the character component and apply damage
        Character character = GetComponent<Character>();
        if (character != null)
        {
            character.TakeDamage(effect.magnitude, effect.damageType);
        }
        
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(effect.magnitude);
        }
    }
    
    /// <summary>
    /// Apply healing over time from a status effect
    /// </summary>
    private void ApplyHealingOverTime(StatusEffect effect)
    {
        Debug.Log($"Applying {effect.magnitude} healing from {effect.effectName}");
        
        Character character = GetComponent<Character>();
        if (character != null)
        {
            character.Heal(effect.magnitude);
        }
        
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Heal(Mathf.RoundToInt(effect.magnitude));
        }
    }
    
    /// <summary>
    /// Apply mana regeneration from a status effect
    /// </summary>
    private void ApplyManaRegeneration(StatusEffect effect)
    {
        Debug.Log($"Applying {effect.magnitude} mana regeneration from {effect.effectName}");
        
        Character character = GetComponent<Character>();
        if (character != null)
        {
            character.RestoreMana(Mathf.RoundToInt(effect.magnitude));
        }
    }
    
    /// <summary>
    /// Refresh an existing status effect with new data
    /// </summary>
    private void RefreshStatusEffect(StatusEffect existing, StatusEffect newEffect)
    {
        // Extend duration
        existing.timeRemaining = Mathf.Max(existing.timeRemaining, newEffect.duration);
        
        // Update magnitude (could be additive or multiplicative based on effect type)
        switch (existing.effectType)
        {
            case StatusEffectType.Burn:
                // Ignite stacks magnitude
                existing.magnitude += newEffect.magnitude;
                existing.sourceFireDamage += newEffect.sourceFireDamage;
                break;
            case StatusEffectType.Strength:
            case StatusEffectType.Dexterity:
            case StatusEffectType.Intelligence:
                // Stack stat buffs
                existing.magnitude += newEffect.magnitude;
                break;
            case StatusEffectType.Shield:
                // Stack shield effects
                existing.magnitude += newEffect.magnitude;
                break;
            case StatusEffectType.TempMaxMana:
            case StatusEffectType.TempEvasion:
            {
                var deltaEffect = new StatusEffect(existing.effectType, existing.effectName, newEffect.magnitude, newEffect.duration, existing.isDebuff);
                ApplyRuntimeStatEffect(deltaEffect, true);
                existing.magnitude += newEffect.magnitude;
                break;
            }
            case StatusEffectType.Chill:
                // Update energy gain reduction based on cold damage
                existing.sourceColdDamage = Mathf.Max(existing.sourceColdDamage, newEffect.sourceColdDamage);
                existing.energyGainReduction = CalculateChilledEnergyReduction(existing.sourceColdDamage);
                break;
            case StatusEffectType.Shocked:
                // Update damage multiplier based on lightning damage
                existing.sourceLightningDamage = Mathf.Max(existing.sourceLightningDamage, newEffect.sourceLightningDamage);
                existing.damageMultiplier = CalculateShockedDamageMultiplier(existing.sourceLightningDamage);
                break;
            default:
                // For most effects, just refresh duration
                break;
        }
        
        Debug.Log($"Refreshed status effect: {existing.effectName} (New duration: {existing.timeRemaining}, New magnitude: {existing.magnitude})");
    }
    
    /// <summary>
    /// Calculate Bleeding magnitude: 70% of physical damage
    /// </summary>
    public static float CalculateBleedingMagnitude(float physicalDamage)
    {
        return physicalDamage * 0.7f;
    }
    
    /// <summary>
    /// Calculate Poison magnitude: 30% of (physical + chaos) damage
    /// </summary>
    public static float CalculatePoisonMagnitude(float physicalDamage, float chaosDamage)
    {
        return (physicalDamage + chaosDamage) * 0.3f;
    }
    
    /// <summary>
    /// Calculate Ignite magnitude: 70% of fire damage
    /// </summary>
    public static float CalculateIgniteMagnitude(float fireDamage)
    {
        return fireDamage * 0.7f;
    }
    
    /// <summary>
    /// Calculate Chilled energy gain reduction: up to 30% based on cold damage
    /// </summary>
    public static float CalculateChilledEnergyReduction(float coldDamage)
    {
        // Scale from 0% to 30% based on cold damage
        // This is a placeholder - you may want to adjust the scaling formula
        float reduction = Mathf.Clamp(coldDamage * 0.01f, 0f, 0.3f);
        return reduction;
    }
    
    /// <summary>
    /// Calculate Frozen duration: 1-2 turns based on cold damage as percentage of target's max HP
    /// Only considers the COLD damage portion, not total damage
    /// </summary>
    public static int CalculateFrozenDuration(float coldDamage, float targetMaxHP)
    {
        if (targetMaxHP <= 0f) return 1; // Safety check
        
        // Calculate cold damage as percentage of max HP
        float coldDamagePercent = (coldDamage / targetMaxHP) * 100f;
        
        // Base 1 turn, +1 turn if cold damage >= 10% of max HP
        if (coldDamagePercent >= 10f)
        {
            return 2;
        }
        return 1;
    }
    
    /// <summary>
    /// Calculate Shocked damage multiplier: up to 1.5x (50% increased) based on lightning damage
    /// </summary>
    public static float CalculateShockedDamageMultiplier(float lightningDamage)
    {
        // Scale from 1.0x to 1.5x based on lightning damage
        // This is a placeholder - you may want to adjust the scaling formula
        float multiplier = 1f + Mathf.Clamp(lightningDamage * 0.01f, 0f, 0.5f);
        return multiplier;
    }
    
    /// <summary>
    /// Update the visual representation of status effects
    /// </summary>
    private void UpdateStatusEffectVisuals()
    {
        if (statusEffectContainer == null || statusEffectIconPrefab == null) return;
        
        // Clear existing icons
        foreach (Transform child in statusEffectContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Group effects by type for stacking display
        var effectsByType = activeStatusEffects.Where(e => e.isActive)
                                               .GroupBy(e => e.effectType);
        
        foreach (var group in effectsByType)
        {
            StatusEffectType effectType = group.Key;
            
            // For Poison, create a single icon showing total magnitude
            if (effectType == StatusEffectType.Poison)
            {
                // Get the shortest duration among all Poison stacks (soonest to expire)
                int shortestDuration = group.Min(e => Mathf.CeilToInt(e.timeRemaining));
                float totalMagnitude = group.Sum(e => e.magnitude);
                int stackCount = group.Count();
                
                // Create a combined display effect
                StatusEffect displayEffect = group.First().Clone();
                displayEffect.magnitude = totalMagnitude;
                displayEffect.timeRemaining = shortestDuration;
                displayEffect.effectName = $"Poison ({stackCount})"; // Show stack count
                
                CreateStatusEffectIcon(displayEffect);
                Debug.Log($"[Poison Display] Showing {stackCount} stacks with total magnitude {totalMagnitude} (shortest duration: {shortestDuration})");
            }
            else
            {
                // For non-stacking effects, show each one individually
                foreach (var effect in group)
                {
                    CreateStatusEffectIcon(effect);
                }
            }
        }

        // If a layout group is present, let it control positioning; otherwise, fallback spacing
        var hasLayout = statusEffectContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>() != null
                        || statusEffectContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>() != null;
        if (!hasLayout)
        {
            int i = 0;
            foreach (Transform child in statusEffectContainer)
            {
                child.localPosition = new Vector3(i * 40f, 0f, 0f);
                i++;
            }
        }
    }
    
    /// <summary>
    /// Create a visual icon for a status effect
    /// </summary>
    private void CreateStatusEffectIcon(StatusEffect effect)
    {
        GameObject iconObj = Instantiate(statusEffectIconPrefab, statusEffectContainer);
        StatusEffectIcon icon = iconObj.GetComponent<StatusEffectIcon>();
        
        if (icon != null)
        {
            icon.SetupStatusEffect(effect);
        }
        
        // Positioning is handled by layout group when present
    }
    
    /// <summary>
    /// Get all active status effects as a list
    /// </summary>
    public List<StatusEffect> GetActiveStatusEffects()
    {
        return activeStatusEffects.Where(e => e.isActive).ToList();
    }

    private void ApplyRuntimeStatEffect(StatusEffect effect, bool applying)
    {
        if (effect == null) return;

        ApplyStackAdjustments(effect, applying);
        ApplyOtherRuntimeEffects(effect, applying);
    }

    private void ApplyOtherRuntimeEffects(StatusEffect effect, bool applying)
    {
        if (effect == null) return;

        float sign = applying ? 1f : -1f;

        var enemyDisplay = GetComponent<EnemyCombatDisplay>();
        if (enemyDisplay != null)
        {
            Enemy enemy = enemyDisplay.GetCurrentEnemy();
            if (enemy != null)
            {
                switch (effect.effectType)
                {
                    case StatusEffectType.Bolster:
                        enemy.ModifyBolsterStacks(effect.magnitude * sign);
                        break;
                    case StatusEffectType.Strength:
                        enemy.ModifyStrengthStacks(effect.magnitude * sign);
                        break;
                    case StatusEffectType.Dexterity:
                        enemy.ModifyDexterityStacks(effect.magnitude * sign);
                        break;
                    case StatusEffectType.Intelligence:
                        enemy.ModifyIntelligenceStacks(effect.magnitude * sign);
                        break;
                    case StatusEffectType.Chill:
                        if (applying)
                        {
                            // Chilled: Reduce energy gain up to 30% based on cold damage
                            effect.energyGainReduction = CalculateChilledEnergyReduction(effect.sourceColdDamage);
                            enemy.ApplyEnergyDrainFromStatus(effect.effectType, effect.energyGainReduction);
                        }
                        // Note: Energy drain is automatically cleared when effect is removed
                        break;
                    case StatusEffectType.Slow:
                        if (applying)
                        {
                            enemy.ApplyEnergyDrainFromStatus(effect.effectType, effect.magnitude);
                        }
                        // Note: Energy drain is automatically cleared when effect is removed
                        break;
                    case StatusEffectType.Freeze:
                    case StatusEffectType.Stagger:
                        // Frozen and Stagger prevent actions - handled in CanAct() method
                        break;
                    case StatusEffectType.Shocked:
                        // Shocked increases damage taken - handled in damage calculation
                        break;
                }
            }
        }

        var playerDisplay = GetComponent<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            var characterManager = CharacterManager.Instance;
            if (characterManager != null && characterManager.HasCharacter())
            {
                var character = characterManager.GetCurrentCharacter();
                if (character != null)
                {
                    switch (effect.effectType)
                    {
                        case StatusEffectType.TempMaxMana:
                        {
                            int delta = Mathf.RoundToInt(effect.magnitude * sign);
                            character.maxMana = Mathf.Max(0, character.maxMana + delta);
                            if (applying)
                            {
                                character.mana = Mathf.Min(character.maxMana, character.mana + delta);
                            }
                            else
                            {
                                character.mana = Mathf.Min(character.mana, character.maxMana);
                            }
                            playerDisplay.UpdateManaDisplay();
                            break;
                        }
                        case StatusEffectType.TempEvasion:
                        {
                            float delta = (effect.magnitude / 100f) * sign;
                            character.increasedEvasion = Mathf.Max(0f, character.increasedEvasion + delta);
                            playerDisplay.RefreshDisplay();
                            break;
                        }
                    }
                }
            }
        }
    }

    private void ApplyStackAdjustments(StatusEffect effect, bool applying)
    {
        ApplyStackAdjustment(effect.stackAdjustment, applying);
    }

    public void ApplyStackAdjustment(StackAdjustmentDefinition adjustment, bool applying)
    {
        if (adjustment == null) return;

        int signedMultiplier = applying ? 1 : -1;

        var enemyDisplay = GetComponent<EnemyCombatDisplay>();
        if (enemyDisplay != null)
        {
            Enemy enemy = enemyDisplay.GetCurrentEnemy();
            if (enemy != null)
            {
                ApplyStackDeltaToEnemy(enemy, StackType.Agitate, CalculateAdjustedDelta(adjustment.agitateStacks, adjustment.agitateMoreMultiplier, adjustment.agitateIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToEnemy(enemy, StackType.Tolerance, CalculateAdjustedDelta(adjustment.toleranceStacks, adjustment.toleranceMoreMultiplier, adjustment.toleranceIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToEnemy(enemy, StackType.Potential, CalculateAdjustedDelta(adjustment.potentialStacks, adjustment.potentialMoreMultiplier, adjustment.potentialIncreasedPercent, signedMultiplier));
                enemyDisplay.UpdateStackDisplay();
            }
        }

        var playerDisplay = GetComponent<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            var stackSystem = StackSystem.Instance;
            if (stackSystem != null)
            {
                ApplyStackDeltaToStackSystem(stackSystem, StackType.Agitate, CalculateAdjustedDelta(adjustment.agitateStacks, adjustment.agitateMoreMultiplier, adjustment.agitateIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToStackSystem(stackSystem, StackType.Tolerance, CalculateAdjustedDelta(adjustment.toleranceStacks, adjustment.toleranceMoreMultiplier, adjustment.toleranceIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToStackSystem(stackSystem, StackType.Potential, CalculateAdjustedDelta(adjustment.potentialStacks, adjustment.potentialMoreMultiplier, adjustment.potentialIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToStackSystem(stackSystem, StackType.Momentum, CalculateAdjustedDelta(adjustment.momentumStacks, adjustment.momentumMoreMultiplier, adjustment.momentumIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToStackSystem(stackSystem, StackType.Flow, CalculateAdjustedDelta(adjustment.flowStacks, adjustment.flowMoreMultiplier, adjustment.flowIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToStackSystem(stackSystem, StackType.Corruption, CalculateAdjustedDelta(adjustment.corruptionStacks, adjustment.corruptionMoreMultiplier, adjustment.corruptionIncreasedPercent, signedMultiplier));
                ApplyStackDeltaToStackSystem(stackSystem, StackType.BattleRhythm, CalculateAdjustedDelta(adjustment.battleRhythmStacks, adjustment.battleRhythmMoreMultiplier, adjustment.battleRhythmIncreasedPercent, signedMultiplier));
            }
        }
    }

    private void ApplyStackDeltaToEnemy(Enemy enemy, StackType type, int delta)
    {
        if (enemy == null || delta == 0) return;

        if (delta > 0)
        {
            enemy.AddStacks(type, delta);
        }
        else if (delta < 0)
        {
            enemy.RemoveStacks(type, Mathf.Abs(delta));
        }
    }

    private void ApplyStackDeltaToStackSystem(StackSystem system, StackType type, int delta)
    {
        if (system == null || delta == 0) return;

        if (delta > 0)
        {
            system.AddStacks(type, delta);
        }
        else if (delta < 0)
        {
            system.RemoveStacks(type, Mathf.Abs(delta));
        }
    }

    private int CalculateAdjustedDelta(int baseStacks, float moreMultiplier, float increasedPercent, int directionSign)
    {
        if (baseStacks == 0 && Mathf.Approximately(moreMultiplier, 0f) && Mathf.Approximately(increasedPercent, 0f))
        {
            return 0;
        }

        float value = baseStacks;
        if (!Mathf.Approximately(moreMultiplier, 0f))
        {
            value += value * moreMultiplier;
        }

        if (!Mathf.Approximately(increasedPercent, 0f))
        {
            value += value * (increasedPercent / 100f);
        }

        int finalAmount = Mathf.RoundToInt(value);
        return finalAmount * directionSign;
    }

    private void AccumulateStackAdjustments(StatusEffect existingEffect, StatusEffect newEffect)
    {
        if (existingEffect == null || newEffect == null) return;

        var adjustment = newEffect.stackAdjustment;
        if (adjustment == null)
        {
            return;
        }

        if (existingEffect.stackAdjustment == null)
        {
            existingEffect.stackAdjustment = adjustment.Clone();
        }
        else
        {
            existingEffect.stackAdjustment.MergeFrom(adjustment);
        }

        ApplyStackAdjustments(newEffect, true);
        ApplyOtherRuntimeEffects(newEffect, true);
    }
    
    /// <summary>
    /// Get all active buffs
    /// </summary>
    public List<StatusEffect> GetActiveBuffs()
    {
        return activeStatusEffects.Where(e => e.isActive && !e.isDebuff).ToList();
    }
    
    /// <summary>
    /// Get all active debuffs
    /// </summary>
    public List<StatusEffect> GetActiveDebuffs()
    {
        return activeStatusEffects.Where(e => e.isActive && e.isDebuff).ToList();
    }
    
    /// <summary>
    /// Check if this entity can act (not Frozen or Staggered)
    /// </summary>
    public bool CanAct()
    {
        bool isFrozen = HasStatusEffect(StatusEffectType.Freeze);
        bool isStaggered = HasStatusEffect(StatusEffectType.Stagger);
        return !isFrozen && !isStaggered;
    }
    
    /// <summary>
    /// Get total damage multiplier from Shocked status effect
    /// </summary>
    public float GetShockedDamageMultiplier()
    {
        StatusEffect shocked = GetStatusEffect(StatusEffectType.Shocked);
        if (shocked != null && shocked.isActive)
        {
            return shocked.damageMultiplier;
        }
        return 1f;
    }
    
    /// <summary>
    /// Get damage multiplier from Vulnerability (20% more damage, consumed after one instance)
    /// </summary>
    public float GetVulnerabilityDamageMultiplier()
    {
        StatusEffect vulnerable = GetStatusEffect(StatusEffectType.Vulnerable);
        if (vulnerable != null && vulnerable.isActive && !vulnerable.vulnerabilityConsumed)
        {
            return 1.2f; // 20% more damage
        }
        return 1f;
    }
    
    /// <summary>
    /// Consume Vulnerability after damage is dealt
    /// </summary>
    public void ConsumeVulnerability()
    {
        StatusEffect vulnerable = GetStatusEffect(StatusEffectType.Vulnerable);
        if (vulnerable != null && !vulnerable.vulnerabilityConsumed)
        {
            vulnerable.vulnerabilityConsumed = true;
            RemoveStatusEffect(vulnerable); // Remove after consumption
            Debug.Log("[Vulnerability] Consumed after damage instance");
        }
    }
    
    /// <summary>
    /// Get total damage reduction from Bolster (2% per stack, max 20% at 10 stacks)
    /// </summary>
    public float GetBolsterDamageReduction()
    {
        StatusEffect bolster = GetStatusEffect(StatusEffectType.Bolster);
        if (bolster != null && bolster.isActive)
        {
            float stacks = Mathf.Min(bolster.magnitude, 10f); // Cap at 10
            return stacks * 0.02f; // 2% per stack
        }
        return 0f;
    }
    
    /// <summary>
    /// Get energy gain reduction from Chilled (up to 30% based on cold damage)
    /// </summary>
    public float GetChilledEnergyReduction()
    {
        StatusEffect chilled = GetStatusEffect(StatusEffectType.Chill);
        if (chilled != null && chilled.isActive)
        {
            return chilled.energyGainReduction;
        }
        return 0f;
    }
    
    /// <summary>
    /// Apply or stack Crumble: add magnitude only, do not reset duration (use max remaining).
    /// </summary>
    public void ApplyOrStackCrumble(float addedMagnitude, int baseDuration)
    {
        var existing = activeStatusEffects.FirstOrDefault(e => e.effectType == StatusEffectType.Crumble);
        if (existing != null)
        {
            // Stack magnitude; keep the higher remaining duration (do not reset lower)
            existing.magnitude += addedMagnitude;
            existing.timeRemaining = Mathf.Max(existing.timeRemaining, baseDuration);
            Debug.Log($"[Crumble] Stacked. Mag={existing.magnitude}, Time={existing.timeRemaining}");
            UpdateStatusEffectVisuals();
            return;
        }
        
        // Create new crumble effect
        var crumble = new StatusEffect(StatusEffectType.Crumble, "Crumble", addedMagnitude, baseDuration, true);
        AddStatusEffect(crumble);
        Debug.Log($"[Crumble] Applied. Mag={addedMagnitude}, Time={baseDuration}");
    }
    
    /// <summary>
    /// Consume Crumble immediately: deal stored damage and remove the debuff.
    /// </summary>
    public void ConsumeCrumble()
    {
        var effect = activeStatusEffects.FirstOrDefault(e => e.effectType == StatusEffectType.Crumble);
        if (effect == null || !effect.isActive || effect.magnitude <= 0f)
        {
            Debug.Log("[Crumble] No active crumble to consume.");
            return;
        }
        
        float storedDamage = effect.magnitude;
        Debug.Log($"[Crumble] Consuming: dealing {storedDamage} stored damage");
        
        // Apply to the owning entity (Character or Enemy)
        Character character = GetComponent<Character>();
        if (character != null)
        {
            character.TakeDamage(storedDamage, DamageType.Physical);
        }
        
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(storedDamage);
        }
        
        // Remove the effect after consuming
        RemoveStatusEffect(effect);
    }

    #region Debug Methods
    
    [ContextMenu("Test Add Poison Effect")]
    private void TestAddPoisonEffect()
    {
        var poisonEffect = new StatusEffect(StatusEffectType.Poison, "Poison", 3f, 3, true); // 3 turns duration
        AddStatusEffect(poisonEffect);
    }
    
    [ContextMenu("Test Add Vulnerability Effect")]
    private void TestAddVulnerabilityEffect()
    {
        var vulnerabilityEffect = new StatusEffect(StatusEffectType.Vulnerable, "Vulnerability", 2f, 2, true); // 2 turns duration
        AddStatusEffect(vulnerabilityEffect);
    }
    
    [ContextMenu("Test Add Strength Buff")]
    private void TestAddStrengthBuff()
    {
        var strengthEffect = new StatusEffect(StatusEffectType.Strength, "TempStrength", 3f, 3, false); // 3 turns duration
        AddStatusEffect(strengthEffect);
    }
    
    [ContextMenu("Test Add Ignite Effect")]
    private void TestAddIgniteEffect()
    {
        var igniteEffect = new StatusEffect(StatusEffectType.Burn, "Ignite", 2f, 2, true); // 2 turns duration
        AddStatusEffect(igniteEffect);
    }
    
    [ContextMenu("Advance One Turn (Test)")]
    private void TestAdvanceOneTurn()
    {
        AdvanceAllEffectsOneTurn();
    }
    
    [ContextMenu("Clear All Effects")]
    private void TestClearAllEffects()
    {
        var effectsToRemove = activeStatusEffects.ToList();
        foreach (var effect in effectsToRemove)
        {
            RemoveStatusEffect(effect);
        }
    }
    
    [ContextMenu("Debug Status Effect Setup")]
    private void DebugStatusEffectSetup()
    {
        Debug.Log($"=== StatusEffectManager Debug ===");
        Debug.Log($"Container: {(statusEffectContainer != null ? statusEffectContainer.name : "NULL")}");
        Debug.Log($"Icon Prefab: {(statusEffectIconPrefab != null ? statusEffectIconPrefab.name : "NULL")}");
        Debug.Log($"Active Effects: {activeStatusEffects.Count}");
        
        if (statusEffectIconPrefab != null)
        {
            var iconComponent = statusEffectIconPrefab.GetComponent<StatusEffectIcon>();
            Debug.Log($"Prefab has StatusEffectIcon: {iconComponent != null}");
        }
        
        // Test sprite loading
        Sprite poisonSprite = Resources.Load<Sprite>("StatusEffectIcons/Poison");
        Debug.Log($"Poison sprite found: {poisonSprite != null}");
        
        Sprite igniteSprite = Resources.Load<Sprite>("StatusEffectIcons/Ignite");
        Debug.Log($"Ignite sprite found: {igniteSprite != null}");
        
        Sprite vulnerabilitySprite = Resources.Load<Sprite>("StatusEffectIcons/Vulnerability");
        Debug.Log($"Vulnerability sprite found: {vulnerabilitySprite != null}");
    }
    
    #endregion
}
