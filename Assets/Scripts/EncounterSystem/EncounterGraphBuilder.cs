using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Builds and manages the encounter prerequisite/unlock graph.
/// Handles relationships between encounters.
/// </summary>
public class EncounterGraphBuilder
{
    private Dictionary<int, List<int>> prerequisiteMap = new Dictionary<int, List<int>>();
    private Dictionary<int, List<int>> unlockMap = new Dictionary<int, List<int>>();
    private Dictionary<int, EncounterData> encounterLookup = new Dictionary<int, EncounterData>();
    
    /// <summary>
    /// Build the encounter graph from a list of encounters.
    /// </summary>
    public void BuildGraph(IEnumerable<EncounterData> encounters)
    {
        prerequisiteMap.Clear();
        unlockMap.Clear();
        encounterLookup.Clear();
        
        // First pass: populate lookup and prerequisite map
        foreach (var encounter in encounters)
        {
            if (encounter == null || encounter.encounterID <= 0)
                continue;
            
            encounterLookup[encounter.encounterID] = encounter;
            
            // Initialize prerequisite map
            if (!prerequisiteMap.ContainsKey(encounter.encounterID))
            {
                prerequisiteMap[encounter.encounterID] = new List<int>();
            }
            
            // Add prerequisites
            if (encounter.prerequisiteEncounterIDs != null && encounter.prerequisiteEncounterIDs.Count > 0)
            {
                prerequisiteMap[encounter.encounterID].AddRange(encounter.prerequisiteEncounterIDs);
            }
            
            // Clear unlocked list (will be rebuilt)
            encounter.unlockedEncounterIDs.Clear();
        }
        
        // Second pass: build unlock map (reverse of prerequisites)
        foreach (var kvp in prerequisiteMap)
        {
            int targetId = kvp.Key;
            foreach (int prereqId in kvp.Value)
            {
                if (!encounterLookup.ContainsKey(prereqId))
                    continue;
                
                // Add to unlock map: completing prereqId unlocks targetId
                if (!unlockMap.TryGetValue(prereqId, out var unlockedList))
                {
                    unlockedList = new List<int>();
                    unlockMap[prereqId] = unlockedList;
                }
                
                if (!unlockedList.Contains(targetId))
                {
                    unlockedList.Add(targetId);
                }
                
                // Also update the encounter's unlocked list
                var prereqEncounter = encounterLookup[prereqId];
                if (!prereqEncounter.unlockedEncounterIDs.Contains(targetId))
                {
                    prereqEncounter.unlockedEncounterIDs.Add(targetId);
                }
            }
        }
        
        // Ensure all encounters have entries in unlock map
        foreach (var id in encounterLookup.Keys)
        {
            if (!unlockMap.ContainsKey(id))
            {
                unlockMap[id] = new List<int>();
            }
        }
        
        Debug.Log($"[EncounterGraphBuilder] Built graph with {encounterLookup.Count} encounters, {prerequisiteMap.Count} prerequisite entries, {unlockMap.Count} unlock entries");
    }
    
    /// <summary>
    /// Get all encounters that unlock when the given encounter is completed.
    /// </summary>
    public IReadOnlyList<int> GetUnlockedEncounters(int completedEncounterID)
    {
        if (unlockMap.TryGetValue(completedEncounterID, out var list))
        {
            return list;
        }
        return System.Array.Empty<int>();
    }
    
    /// <summary>
    /// Get all prerequisites for an encounter.
    /// </summary>
    public IReadOnlyList<int> GetPrerequisites(int encounterID)
    {
        if (prerequisiteMap.TryGetValue(encounterID, out var list))
        {
            return list;
        }
        return System.Array.Empty<int>();
    }
    
    /// <summary>
    /// Check if all prerequisites for an encounter are completed.
    /// </summary>
    public bool ArePrerequisitesCompleted(int encounterID, HashSet<int> completedEncounterIDs)
    {
        // Encounter 1 has no prerequisites - it's always available
        if (encounterID == 1)
        {
            return true;
        }
        
        if (!prerequisiteMap.TryGetValue(encounterID, out var prerequisites) || prerequisites.Count == 0)
            return true;
        
        foreach (var prereqId in prerequisites)
        {
            if (!completedEncounterIDs.Contains(prereqId))
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Get encounter data by ID.
    /// </summary>
    public EncounterData GetEncounter(int encounterID)
    {
        encounterLookup.TryGetValue(encounterID, out var encounter);
        return encounter;
    }
    
    /// <summary>
    /// Get all encounters.
    /// </summary>
    public IReadOnlyDictionary<int, EncounterData> GetAllEncounters()
    {
        return encounterLookup;
    }
    
    /// <summary>
    /// Clear all graph data.
    /// </summary>
    public void Clear()
    {
        prerequisiteMap.Clear();
        unlockMap.Clear();
        encounterLookup.Clear();
    }
}

