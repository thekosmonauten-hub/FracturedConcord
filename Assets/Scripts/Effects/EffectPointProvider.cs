using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Provides effect point transforms for visual effects.
/// Can be attached to any GameObject (character icons, enemies, etc.)
/// </summary>
public class EffectPointProvider : MonoBehaviour
{
    [Header("Effect Points")]
    [Tooltip("Transform where head effects should appear")]
    public Transform effectPoint_Head;
    
    [Tooltip("Transform where chest/torso effects should appear")]
    public Transform effectPoint_Chest;
    
    [Tooltip("Transform where weapon/hand effects should appear")]
    public Transform effectPoint_Weapon;
    
    [Tooltip("Default fallback point if specific point not found")]
    public Transform effectPoint_Default;
    
    /// <summary>
    /// Get effect point by name, or return default/self if not found
    /// </summary>
    public Transform GetEffectPoint(string pointName = "Default")
    {
        switch (pointName.ToLower())
        {
            case "head": 
                return effectPoint_Head ?? effectPoint_Default ?? transform;
            case "chest": 
                return effectPoint_Chest ?? effectPoint_Default ?? transform;
            case "weapon": 
                return effectPoint_Weapon ?? effectPoint_Default ?? transform;
            default: 
                return effectPoint_Default ?? transform;
        }
    }
    
    /// <summary>
    /// Get all available effect points
    /// </summary>
    public List<Transform> GetAllEffectPoints()
    {
        List<Transform> points = new List<Transform>();
        if (effectPoint_Head != null) points.Add(effectPoint_Head);
        if (effectPoint_Chest != null) points.Add(effectPoint_Chest);
        if (effectPoint_Weapon != null) points.Add(effectPoint_Weapon);
        if (effectPoint_Default != null) points.Add(effectPoint_Default);
        if (points.Count == 0) points.Add(transform);
        return points;
    }
}

