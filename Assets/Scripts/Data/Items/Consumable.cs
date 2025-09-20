using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Dexiled/Items/Consumables/Consumable")]
public class Consumable : BaseItem
{
    [Header("Consumable Properties")]
    public ConsumableType consumableType;
    public ConsumableEffectType effectType;
    
    [Header("Consumable Effects")]
    public float effectValue = 0f;
    public float effectDuration = 0f; // 0 = instant effect
    public DamageType effectDamageType = DamageType.None;
    
    [Header("Consumable Requirements")]
    public new int requiredLevel = 1;
    public bool requiresTarget = false;
    public float castTime = 0f;
    public float cooldown = 0f;
    
    [Header("Consumable Modifiers")]
    public List<ConsumableModifier> modifiers = new List<ConsumableModifier>();
    
    [Header("Stack Properties")]
    public new bool isStackable = true;
    public new int maxStackSize = 10;
    public int currentStackSize = 1;
    
    public override string GetDisplayName()
    {
        string stackInfo = isStackable && currentStackSize > 1 ? $" ({currentStackSize})" : "";
        string rarityPrefix = rarity != ItemRarity.Normal ? $"[{rarity}] " : "";
        return rarityPrefix + itemName + stackInfo;
    }
    
    public override string GetFullDescription()
    {
        string desc = description;
        
        // Add consumable type
        desc += $"\n\n{consumableType}";
        
        // Add effect information
        if (effectValue > 0)
        {
            desc += $"\nEffect: {effectValue}";
            if (effectDamageType != DamageType.None)
            {
                desc += $" {effectDamageType}";
            }
            
            if (effectDuration > 0)
            {
                desc += $" for {effectDuration} seconds";
            }
        }
        
        // Add requirements
        if (requiredLevel > 1)
        {
            desc += $"\n\nRequired Level: {requiredLevel}";
        }
        
        if (castTime > 0)
        {
            desc += $"\nCast Time: {castTime:F1}s";
        }
        
        if (cooldown > 0)
        {
            desc += $"\nCooldown: {cooldown:F1}s";
        }
        
        // Add modifiers
        if (modifiers.Count > 0)
        {
            desc += "\n\nEffects:";
            foreach (var modifier in modifiers)
            {
                desc += $"\n{modifier.description}";
            }
        }
        
        // Add stack information
        if (isStackable)
        {
            desc += $"\n\nStack Size: {currentStackSize}/{maxStackSize}";
        }
        
        return desc;
    }
    
    public bool CanUse(Character character)
    {
        return character.level >= requiredLevel;
    }
    
    public void Use(Character character)
    {
        if (!CanUse(character)) return;
        
        // Apply effects
        ApplyEffects(character);
        
        // Reduce stack size
        if (isStackable && currentStackSize > 1)
        {
            currentStackSize--;
        }
    }
    
    private void ApplyEffects(Character character)
    {
        // Apply base effect
        switch (effectType)
        {
            case ConsumableEffectType.Heal:
                character.Heal(effectValue);
                break;
            case ConsumableEffectType.RestoreMana:
                character.RestoreMana((int)effectValue);
                break;
            case ConsumableEffectType.Buff:
                // Apply buff effect
                break;
            case ConsumableEffectType.Debuff:
                // Apply debuff effect
                break;
        }
        
        // Apply modifiers
        foreach (var modifier in modifiers)
        {
            ApplyModifier(character, modifier);
        }
    }
    
    private void ApplyModifier(Character character, ConsumableModifier modifier)
    {
        // Apply modifier effects to character
        // This would integrate with the character's stat system
    }
}

[System.Serializable]
public class ConsumableModifier
{
    public string description;
    public ConsumableModifierType modifierType;
    public float value;
    public float duration = 0f;
    public StatType statType = StatType.Flat;
}

public enum ConsumableType
{
    HealthPotion,
    ManaPotion,
    HybridPotion,
    LifePotion,
    Scroll,
    Gem,
    Orb,
    Fragment,
    Essence,
    Catalyst
}

public enum ConsumableEffectType
{
    Heal,
    RestoreMana,
    Buff,
    Debuff,
    Transform,
    Identify,
    Portal,
    Other
}

public enum ConsumableModifierType
{
    IncreasedLife,
    IncreasedMana,
    IncreasedStrength,
    IncreasedDexterity,
    IncreasedIntelligence,
    IncreasedDamage,
    IncreasedAttackSpeed,
    IncreasedCastSpeed,
    IncreasedMovementSpeed,
    IncreasedResistance,
    ReducedDamage,
    ReducedSpeed,
    ReducedResistance,
    Other
}
