using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Database for all monster modifiers that can be applied to Magic/Rare enemies.
/// </summary>
[CreateAssetMenu(fileName = "Monster Modifier Database", menuName = "Dexiled/Combat/Monster Modifier Database")]
public class MonsterModifierDatabase : ScriptableObject
{
    [Header("All Modifiers")]
    public List<MonsterModifier> allModifiers = new List<MonsterModifier>();
    
    [Header("Resources Loading")]
    [Tooltip("Folders under any Resources/ to scan for MonsterModifier assets. Leave empty to scan all.")]
    public string[] resourcePaths = new string[] { "MonsterModifiers" };
    
    private static MonsterModifierDatabase _instance;
    public static MonsterModifierDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<MonsterModifierDatabase>("MonsterModifierDatabase");
                if (_instance == null)
                {
                    Debug.LogWarning("MonsterModifierDatabase not found in Resources folder! Creating empty database.");
                    _instance = ScriptableObject.CreateInstance<MonsterModifierDatabase>();
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Get a random modifier from the database
    /// </summary>
    public MonsterModifier GetRandomModifier()
    {
        if (allModifiers == null || allModifiers.Count == 0)
        {
            Debug.LogWarning("[MonsterModifierDatabase] No modifiers available!");
            return null;
        }
        
        return allModifiers[Random.Range(0, allModifiers.Count)];
    }
    
    /// <summary>
    /// Get multiple random modifiers (ensures no duplicates)
    /// Uses uniform random selection (no weights)
    /// </summary>
    public List<MonsterModifier> GetRandomModifiers(int count)
    {
        List<MonsterModifier> result = new List<MonsterModifier>();
        
        if (allModifiers == null || allModifiers.Count == 0)
        {
            Debug.LogWarning("[MonsterModifierDatabase] No modifiers available!");
            return result;
        }
        
        // Create a shuffled copy of available modifiers
        List<MonsterModifier> available = new List<MonsterModifier>(allModifiers);
        
        // Shuffle
        for (int i = 0; i < available.Count; i++)
        {
            MonsterModifier temp = available[i];
            int randomIndex = Random.Range(i, available.Count);
            available[i] = available[randomIndex];
            available[randomIndex] = temp;
        }
        
        // Take up to count modifiers
        int takeCount = Mathf.Min(count, available.Count);
        for (int i = 0; i < takeCount; i++)
        {
            result.Add(available[i]);
        }
        
        return result;
    }
    
    /// <summary>
    /// Get multiple random modifiers using weighted selection (ensures no duplicates)
    /// Higher weight = more common, lower weight = rarer but more rewarding
    /// </summary>
    public List<MonsterModifier> GetRandomModifiersWeighted(int count)
    {
        List<MonsterModifier> result = new List<MonsterModifier>();
        
        if (allModifiers == null || allModifiers.Count == 0)
        {
            Debug.LogWarning("[MonsterModifierDatabase] No modifiers available!");
            return result;
        }
        
        // Filter out null modifiers and calculate total weight
        List<MonsterModifier> validModifiers = new List<MonsterModifier>();
        int totalWeight = 0;
        
        foreach (var modifier in allModifiers)
        {
            if (modifier != null)
            {
                validModifiers.Add(modifier);
                totalWeight += Mathf.Max(1, modifier.weight); // Ensure weight is at least 1
            }
        }
        
        if (validModifiers.Count == 0)
        {
            Debug.LogWarning("[MonsterModifierDatabase] No valid modifiers available!");
            return result;
        }
        
        // Select modifiers using weighted random (without replacement)
        List<MonsterModifier> available = new List<MonsterModifier>(validModifiers);
        int currentTotalWeight = totalWeight;
        
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            // Roll weighted random
            int roll = Random.Range(0, currentTotalWeight);
            int accumulatedWeight = 0;
            MonsterModifier selected = null;
            int selectedIndex = -1;
            
            for (int j = 0; j < available.Count; j++)
            {
                accumulatedWeight += Mathf.Max(1, available[j].weight);
                if (roll < accumulatedWeight)
                {
                    selected = available[j];
                    selectedIndex = j;
                    break;
                }
            }
            
            if (selected != null)
            {
                result.Add(selected);
                currentTotalWeight -= Mathf.Max(1, selected.weight);
                available.RemoveAt(selectedIndex);
            }
            else
            {
                // Fallback: just take first available
                if (available.Count > 0)
                {
                    selected = available[0];
                    result.Add(selected);
                    currentTotalWeight -= Mathf.Max(1, selected.weight);
                    available.RemoveAt(0);
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get modifier by name
    /// </summary>
    public MonsterModifier GetModifierByName(string name)
    {
        return allModifiers.FirstOrDefault(m => m != null && m.modifierName == name);
    }
}

