using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Action that a modifier effect performs
/// </summary>
[System.Serializable]
public class ModifierAction
{
    public ModifierActionType actionType;
    public SerializableParameterDictionary parameterDict = new SerializableParameterDictionary();
    public int executionOrder = 0;
    
    /// <summary>
    /// Get a parameter value
    /// </summary>
    public object GetParameter(string key)
    {
        return parameterDict?.GetParameter(key);
    }
    
    /// <summary>
    /// Check if a parameter exists
    /// </summary>
    public bool HasParameter(string key)
    {
        return parameterDict != null && parameterDict.ContainsKey(key);
    }
    
    /// <summary>
    /// Get all parameters as a dictionary (for compatibility with ModifierEffectResolver)
    /// </summary>
    public Dictionary<string, object> GetParameters()
    {
        var dict = new Dictionary<string, object>();
        if (parameterDict != null)
        {
            foreach (var key in parameterDict.Keys)
            {
                dict[key] = parameterDict.GetParameter(key);
            }
        }
        return dict;
    }
    
    /// <summary>
    /// Property for compatibility with ModifierEffectResolver (expects action.parameters)
    /// </summary>
    public Dictionary<string, object> parameters
    {
        get { return GetParameters(); }
    }
}

