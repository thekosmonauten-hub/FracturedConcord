using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Condition that must be met for a modifier effect to trigger
/// </summary>
[System.Serializable]
public class ModifierCondition
{
    public ModifierConditionType conditionType;
    public SerializableParameterDictionary parameterDict = new SerializableParameterDictionary();
    public bool invert = false; // If true, invert the condition result
    
    /// <summary>
    /// Get parameters dictionary (for compatibility)
    /// </summary>
    public SerializableParameterDictionary parameters => parameterDict;
    
    /// <summary>
    /// Check if this condition is met
    /// </summary>
    public bool Evaluate(Dictionary<string, object> eventContext, Character character, Dictionary<string, object> modifierState)
    {
        // Implementation would go here - for now, return true to allow all conditions
        // This should be implemented in ModifierConditionResolver
        bool result = true;
        
        if (invert)
        {
            result = !result;
        }
        
        return result;
    }
}

