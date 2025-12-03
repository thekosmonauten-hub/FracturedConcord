using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Serializable dictionary for storing flexible parameters for ModifierAction and ModifierCondition
/// </summary>
[System.Serializable]
public class SerializableParameterDictionary
{
    [System.Serializable]
    public class ParameterValue
    {
        public enum ValueType
        {
            String = 0,
            Int = 1,
            Float = 2,
            Bool = 3
        }
        
        public ValueType type;
        public string stringValue;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        
        public object GetValue()
        {
            switch (type)
            {
                case ValueType.String: return stringValue;
                case ValueType.Int: return intValue;
                case ValueType.Float: return floatValue;
                case ValueType.Bool: return boolValue;
                default: return null;
            }
        }
        
        public void SetValue(object value)
        {
            if (value is string str)
            {
                type = ValueType.String;
                stringValue = str;
            }
            else if (value is int i)
            {
                type = ValueType.Int;
                intValue = i;
            }
            else if (value is float f)
            {
                type = ValueType.Float;
                floatValue = f;
            }
            else if (value is bool b)
            {
                type = ValueType.Bool;
                boolValue = b;
            }
        }
    }
    
    [System.Serializable]
    public class ParameterEntry
    {
        public string key;
        public ParameterValue value;
    }
    
    public List<ParameterEntry> entries = new List<ParameterEntry>();
    
    private Dictionary<string, object> runtimeDict;
    
    /// <summary>
    /// Get a parameter value by key
    /// </summary>
    public object GetParameter(string key)
    {
        if (runtimeDict == null)
        {
            BuildRuntimeDict();
        }
        
        if (runtimeDict.TryGetValue(key, out object value))
        {
            return value;
        }
        return null;
    }
    
    /// <summary>
    /// Get a parameter as a specific type
    /// </summary>
    public T GetParameter<T>(string key)
    {
        object value = GetParameter(key);
        if (value != null && value is T)
        {
            return (T)value;
        }
        return default(T);
    }
    
    /// <summary>
    /// Check if a parameter exists
    /// </summary>
    public bool ContainsKey(string key)
    {
        if (runtimeDict == null)
        {
            BuildRuntimeDict();
        }
        
        return runtimeDict.ContainsKey(key);
    }
    
    /// <summary>
    /// Build the runtime dictionary from serialized entries
    /// </summary>
    private void BuildRuntimeDict()
    {
        runtimeDict = new Dictionary<string, object>();
        
        foreach (var entry in entries)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.key) && entry.value != null)
            {
                runtimeDict[entry.key] = entry.value.GetValue();
            }
        }
    }
    
    /// <summary>
    /// Get all keys
    /// </summary>
    public IEnumerable<string> Keys
    {
        get
        {
            if (runtimeDict == null)
            {
                BuildRuntimeDict();
            }
            return runtimeDict.Keys;
        }
    }
}

