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
            effect.AdvanceTurn();
        }
        
        // Remove expired effects
        RemoveExpiredEffects();
    }
    
    /// <summary>
    /// Add a status effect to this entity
    /// </summary>
    public bool AddStatusEffect(StatusEffect newEffect)
    {
        if (newEffect == null) return false;
        
        // Check if we already have this type of effect
        StatusEffect existingEffect = activeStatusEffects.FirstOrDefault(e => e.effectType == newEffect.effectType);
        
        if (existingEffect != null)
        {
            // Refresh existing effect (extend duration, update magnitude, etc.)
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
    /// Update all active status effects
    /// </summary>
    private void UpdateStatusEffects()
    {
        var effectsToRemove = new List<StatusEffect>();
        
        foreach (var effect in activeStatusEffects)
        {
            effect.UpdateEffect(Time.deltaTime);
            
            // Check if effect should tick
            if (effect.ShouldTick())
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
            case StatusEffectType.Poison:
            case StatusEffectType.Burn:
            case StatusEffectType.ChaosDot:
                // Apply damage over time
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
            case StatusEffectType.Poison:
            case StatusEffectType.Burn:
                // Stack damage over time effects
                existing.magnitude += newEffect.magnitude;
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
            default:
                // For most effects, just refresh duration
                break;
        }
        
        Debug.Log($"Refreshed status effect: {existing.effectName} (New duration: {existing.timeRemaining}, New magnitude: {existing.magnitude})");
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
        
        // Create new icons for active effects
        foreach (var effect in activeStatusEffects.Where(e => e.isActive))
        {
            CreateStatusEffectIcon(effect);
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
