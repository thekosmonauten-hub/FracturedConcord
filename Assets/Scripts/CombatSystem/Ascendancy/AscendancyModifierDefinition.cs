using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Definition of an Ascendancy modifier - ScriptableObject that defines what happens when a passive is unlocked
/// </summary>
[CreateAssetMenu(fileName = "New Ascendancy Modifier", menuName = "Dexiled/Ascendancy/Modifier Definition")]
public class AscendancyModifierDefinition : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Unique identifier for this modifier (e.g., 'Tolerance_Start')")]
    public string modifierId;
    
    [Tooltip("Name of the passive this modifier is linked to")]
    public string linkedPassiveName;
    
    [TextArea(3, 5)]
    [Tooltip("Description of what this modifier does")]
    public string description;
    
    [Header("Effects")]
    [Tooltip("List of effects that trigger on various events")]
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
}

