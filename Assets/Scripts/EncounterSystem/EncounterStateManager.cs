using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages encounter state (unlocked/completed) as a single source of truth.
/// Derives state from character progression and encounter data.
/// </summary>
public class EncounterStateManager
{
    private Dictionary<int, EncounterState> encounterStates = new Dictionary<int, EncounterState>();
    private EncounterGraphBuilder graphBuilder;
    
    public struct EncounterState
    {
        public bool isUnlocked;
        public bool isCompleted;
        public bool isAvailable; // unlocked AND prerequisites met
        public string lockReason; // why it's locked (if locked)
        
        public override string ToString()
        {
            return $"Unlocked: {isUnlocked}, Completed: {isCompleted}, Available: {isAvailable}, LockReason: {lockReason}";
        }
    }
    
    public EncounterStateManager(EncounterGraphBuilder graphBuilder)
    {
        this.graphBuilder = graphBuilder;
    }
    
    /// <summary>
    /// Refresh all encounter states from character progression.
    /// </summary>
    public void RefreshStates(Dictionary<int, EncounterProgressionData> progression)
    {
        encounterStates.Clear();
        
        var allEncounters = graphBuilder.GetAllEncounters();
        var completedSet = new HashSet<int>();
        
        // Build completed set from progression
        foreach (var kvp in progression)
        {
            if (kvp.Value.isCompleted)
            {
                completedSet.Add(kvp.Key);
            }
        }
        
        // Ensure encounter 1 is always in progression and unlocked
        if (!progression.ContainsKey(1))
        {
            var encounter1Prog = new EncounterProgressionData(1);
            encounter1Prog.MarkUnlocked();
            progression[1] = encounter1Prog;
        }
        else if (!progression[1].isUnlocked)
        {
            progression[1].MarkUnlocked();
        }
        
        // Calculate state for each encounter
        foreach (var kvp in allEncounters)
        {
            int encounterID = kvp.Key;
            EncounterData encounter = kvp.Value;
            
            EncounterState state = CalculateState(encounterID, encounter, progression, completedSet);
            encounterStates[encounterID] = state;
        }
        
        // Explicitly ensure encounter 1 state is correct
        if (encounterStates.ContainsKey(1))
        {
            var state1 = encounterStates[1];
            state1.isUnlocked = true;
            state1.isAvailable = true;
            state1.lockReason = string.Empty;
            encounterStates[1] = state1;
        }
    }
    
    /// <summary>
    /// Calculate the state for a single encounter.
    /// </summary>
    private EncounterState CalculateState(
        int encounterID, 
        EncounterData encounter, 
        Dictionary<int, EncounterProgressionData> progression,
        HashSet<int> completedSet)
    {
        EncounterState state = new EncounterState();
        
        // Encounter 1 is always unlocked
        if (encounterID == 1)
        {
            state.isUnlocked = true;
            state.isAvailable = true;
            state.lockReason = string.Empty;
        }
        else
        {
            // Check if unlocked in progression
            if (progression.TryGetValue(encounterID, out var progData))
            {
                state.isUnlocked = progData.isUnlocked;
            }
            else
            {
                // Check if prerequisites are met
                state.isUnlocked = graphBuilder.ArePrerequisitesCompleted(encounterID, completedSet);
            }
            
            // Available if unlocked AND prerequisites met
            state.isAvailable = state.isUnlocked && 
                               graphBuilder.ArePrerequisitesCompleted(encounterID, completedSet);
            
            // Determine lock reason
            if (!state.isUnlocked)
            {
                var prereqs = graphBuilder.GetPrerequisites(encounterID);
                if (prereqs.Count > 0)
                {
                    List<int> missingPrereqs = new List<int>();
                    foreach (var prereqId in prereqs)
                    {
                        if (!completedSet.Contains(prereqId))
                        {
                            missingPrereqs.Add(prereqId);
                        }
                    }
                    if (missingPrereqs.Count > 0)
                    {
                        state.lockReason = $"Complete prerequisites: {string.Join(", ", missingPrereqs)}";
                    }
                }
                else
                {
                    state.lockReason = "Not unlocked";
                }
            }
        }
        
        // Check completion state
        if (progression.TryGetValue(encounterID, out var prog))
        {
            state.isCompleted = prog.isCompleted;
        }
        else
        {
            state.isCompleted = false;
        }
        
        return state;
    }
    
    /// <summary>
    /// Get the state for an encounter.
    /// </summary>
    public EncounterState GetState(int encounterID)
    {
        if (encounterStates.TryGetValue(encounterID, out var state))
        {
            return state;
        }
        
        // Return default state if not found
        return new EncounterState
        {
            isUnlocked = encounterID == 1, // Only encounter 1 is unlocked by default
            isCompleted = false,
            isAvailable = encounterID == 1,
            lockReason = encounterID == 1 ? string.Empty : "Encounter not found"
        };
    }
    
    /// <summary>
    /// Check if an encounter is unlocked.
    /// </summary>
    public bool IsUnlocked(int encounterID)
    {
        // Encounter 1 is always unlocked - bypass all checks
        if (encounterID == 1)
        {
            Debug.Log($"[EncounterStateManager] IsUnlocked({encounterID}) - Encounter 1, returning TRUE (always unlocked)");
            return true;
        }
        var state = GetState(encounterID);
        Debug.Log($"[EncounterStateManager] IsUnlocked({encounterID}) - State: {state}");
        return state.isUnlocked;
    }
    
    /// <summary>
    /// Check if an encounter is completed.
    /// </summary>
    public bool IsCompleted(int encounterID)
    {
        return GetState(encounterID).isCompleted;
    }
    
    /// <summary>
    /// Check if an encounter is available (unlocked and prerequisites met).
    /// </summary>
    public bool IsAvailable(int encounterID)
    {
        // Encounter 1 is always available - bypass all checks
        if (encounterID == 1)
        {
            return true;
        }
        return GetState(encounterID).isAvailable;
    }
    
    /// <summary>
    /// Get the lock reason for an encounter.
    /// </summary>
    public string GetLockReason(int encounterID)
    {
        return GetState(encounterID).lockReason;
    }
    
    /// <summary>
    /// Clear all states.
    /// </summary>
    public void Clear()
    {
        encounterStates.Clear();
    }
}

