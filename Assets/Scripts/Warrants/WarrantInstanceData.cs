using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable data structure for storing warrant instances in Character save data.
/// Used to persist rolled warrant instances across scenes.
/// </summary>
[System.Serializable]
public class WarrantInstanceData
{
    public string warrantId;
    public string displayName;
    public WarrantRarity rarity;
    public bool isKeystone;
    public bool isBlueprint;
    
    // Range & Behavior
    public WarrantRangeDirection rangeDirection;
    public int rangeDepth;
    public bool affectDiagonals;
    
    // Modifiers (already serializable)
    public List<WarrantModifier> modifiers = new List<WarrantModifier>();
    
    // Notable
    public string notableId;
    
    // Original blueprint ID (if rolled from blueprint)
    public string blueprintId;
    
    /// <summary>
    /// Create WarrantInstanceData from a WarrantDefinition
    /// </summary>
    public static WarrantInstanceData FromWarrantDefinition(WarrantDefinition warrant, string blueprintId = null)
    {
        if (warrant == null) return null;
        
        var data = new WarrantInstanceData
        {
            warrantId = warrant.warrantId,
            displayName = warrant.displayName,
            rarity = warrant.rarity,
            isKeystone = warrant.isKeystone,
            isBlueprint = warrant.isBlueprint,
            rangeDirection = warrant.rangeDirection,
            rangeDepth = warrant.rangeDepth,
            affectDiagonals = warrant.affectDiagonals,
            notableId = warrant.notableId,
            blueprintId = blueprintId
        };
        
        // Copy modifiers
        if (warrant.modifiers != null)
        {
            data.modifiers = new List<WarrantModifier>();
            foreach (var mod in warrant.modifiers)
            {
                if (mod != null)
                {
                    data.modifiers.Add(new WarrantModifier
                    {
                        modifierId = mod.modifierId,
                        displayName = mod.displayName,
                        operation = mod.operation,
                        value = mod.value,
                        description = mod.description
                    });
                }
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// Create a WarrantDefinition ScriptableObject instance from this data
    /// </summary>
    public WarrantDefinition ToWarrantDefinition(WarrantDatabase database)
    {
        if (database == null)
        {
            Debug.LogError("[WarrantInstanceData] Cannot create WarrantDefinition without WarrantDatabase");
            return null;
        }
        
        // Create a runtime ScriptableObject instance
        WarrantDefinition instance = ScriptableObject.CreateInstance<WarrantDefinition>();
        
        instance.warrantId = this.warrantId;
        instance.displayName = this.displayName;
        instance.rarity = this.rarity;
        instance.isKeystone = this.isKeystone;
        instance.isBlueprint = this.isBlueprint;
        instance.rangeDirection = this.rangeDirection;
        instance.rangeDepth = this.rangeDepth;
        instance.affectDiagonals = this.affectDiagonals;
        instance.notableId = this.notableId;
        
        // Copy modifiers
        instance.modifiers = new List<WarrantModifier>();
        if (this.modifiers != null)
        {
            foreach (var mod in this.modifiers)
            {
                if (mod != null)
                {
                    instance.modifiers.Add(new WarrantModifier
                    {
                        modifierId = mod.modifierId,
                        displayName = mod.displayName,
                        operation = mod.operation,
                        value = mod.value,
                        description = mod.description
                    });
                }
            }
        }
        
        return instance;
    }
}

