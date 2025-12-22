using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Definition of an Embossing modifier - ScriptableObject that defines what happens when an embossing is applied to a card
/// Similar to RelianceAuraModifierDefinition but for embossings
/// Supports level scaling (embossing level 1-20)
/// </summary>
[CreateAssetMenu(fileName = "New Embossing Modifier", menuName = "Dexiled/Embossing/Modifier Definition")]
public class EmbossingModifierDefinition : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Unique identifier for this modifier (e.g., 'of_ferocity_damage')")]
    public string modifierId;
    
    [Tooltip("ID of the embossing effect this modifier is linked to (references EmbossingEffect.embossingId)")]
    public string linkedEmbossingId;
    
    [TextArea(3, 5)]
    [Tooltip("Description of what this modifier does")]
    public string description;
    
    [Header("Effects")]
    [Tooltip("List of effects that trigger on various events (OnCardPlayed, OnDamageDealt, etc.)")]
    public List<ModifierEffect> effects = new List<ModifierEffect>();
    
    [Header("Variables (Optional)")]
    [Tooltip("Variable keys for dynamic values")]
    public List<string> variableKeys = new List<string>();
    
    [Tooltip("Variable values (corresponds to variableKeys). Note: Unity cannot serialize List<object>, so this is stored as a workaround.")]
    [System.NonSerialized]
    public List<object> variableValues = new List<object>();
    
    [Header("Tags (Optional)")]
    [Tooltip("Tags for categorizing modifiers")]
    public List<string> tags = new List<string>();
    
    [Header("Level Scaling")]
    [Tooltip("Minimum embossing level for this modifier to be active")]
    public int minLevel = 1;
    
    [Tooltip("Maximum embossing level for this modifier (0 = no max, scales to level 20)")]
    public int maxLevel = 0;
    
    [Header("Requirements (Optional)")]
    [Tooltip("Card tags required for this modifier to apply. If multiple tags are listed, the card needs at least ONE of them (OR logic). Example: ['Attack', 'Melee'] means the card must have either 'Attack' OR 'Melee' tag.")]
    public List<string> requiredCardTags = new List<string>();
    
    [Tooltip("If true, ALL requiredCardTags must be present (AND logic). If false, at least ONE tag must be present (OR logic - default).")]
    public bool requireAllTags = false;
    
    [Tooltip("Damage types this modifier applies to (empty = all types)")]
    public List<DamageType> applicableDamageTypes = new List<DamageType>();
}

