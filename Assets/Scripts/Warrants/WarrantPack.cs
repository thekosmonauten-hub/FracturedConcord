using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A collection of warrants that can be given to players together (e.g., starter pack, reward pack)
/// </summary>
[CreateAssetMenu(fileName = "New Warrant Pack", menuName = "Dexiled/Warrants/Warrant Pack", order = 1)]
public class WarrantPack : ScriptableObject
{
    [Header("Pack Information")]
    [Tooltip("Unique identifier for this pack (e.g., 'warrant_starter_pack')")]
    public string packId;
    
    [Tooltip("Display name for this pack")]
    public string displayName;
    
    [TextArea(2, 4)]
    [Tooltip("Description of what this pack contains")]
    public string description;
    
    [Header("Warrants in Pack")]
    [Tooltip("List of warrant blueprints that are included in this pack. Each warrant will be rolled and given to the player.")]
    public List<WarrantDefinition> warrantBlueprints = new List<WarrantDefinition>();
    
    [Header("Rolling Settings")]
    [Tooltip("Minimum affixes when rolling warrants from this pack")]
    [Range(0, 5)]
    public int minAffixes = 1;
    
    [Tooltip("Maximum affixes when rolling warrants from this pack")]
    [Range(1, 5)]
    public int maxAffixes = 3;
    
    /// <summary>
    /// Get the number of warrants in this pack
    /// </summary>
    public int GetWarrantCount()
    {
        return warrantBlueprints != null ? warrantBlueprints.Count : 0;
    }
    
    /// <summary>
    /// Check if this pack is valid (has an ID and at least one warrant)
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(packId) || warrantBlueprints == null || warrantBlueprints.Count == 0)
            return false;
        
        // Check that all blueprints are non-null
        foreach (var blueprint in warrantBlueprints)
        {
            if (blueprint == null)
                return false;
        }
        
        return true;
    }
}

