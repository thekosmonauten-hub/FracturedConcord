using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages encounter progression (unlocking, completion, statistics).
/// Handles the business logic for encounter progression.
/// </summary>
public class EncounterProgressionManager
{
    private Dictionary<int, EncounterProgressionData> progression = new Dictionary<int, EncounterProgressionData>();
    private EncounterGraphBuilder graphBuilder;
    private EncounterStateManager stateManager;
    
    public EncounterProgressionManager(EncounterGraphBuilder graphBuilder, EncounterStateManager stateManager)
    {
        this.graphBuilder = graphBuilder;
        this.stateManager = stateManager;
    }
    
    /// <summary>
    /// Load progression from character data.
    /// </summary>
    public void LoadProgression(Dictionary<int, EncounterProgressionData> characterProgression)
    {
        Debug.Log($"[EncounterProgressionManager] LoadProgression - Input has {characterProgression?.Count ?? 0} encounters");
        progression.Clear();
        
        if (characterProgression != null)
        {
            foreach (var kvp in characterProgression)
            {
                progression[kvp.Key] = kvp.Value;
                Debug.Log($"[EncounterProgressionManager] Loaded encounter {kvp.Key}: Unlocked={kvp.Value.isUnlocked}, Completed={kvp.Value.isCompleted}");
            }
        }
        
        // Ensure encounter 1 is always unlocked
        EnsureEncounter1Unlocked();
        
        // Verify encounter 1 after ensuring
        if (progression.TryGetValue(1, out var encounter1Data))
        {
            Debug.Log($"[EncounterProgressionManager] After EnsureEncounter1Unlocked, encounter 1: Unlocked={encounter1Data.isUnlocked}, Completed={encounter1Data.isCompleted}");
        }
        else
        {
            Debug.LogError($"[EncounterProgressionManager] Encounter 1 NOT FOUND in progression after EnsureEncounter1Unlocked!");
        }
        
        RefreshStates();
    }
    
    /// <summary>
    /// Mark an encounter as attempted.
    /// </summary>
    public void MarkAttempted(int encounterID)
    {
        var data = GetOrCreateProgression(encounterID);
        data.MarkAttempted();
        
        EncounterEvents.InvokeEncounterAttempted(encounterID);
    }
    
    /// <summary>
    /// Mark an encounter as completed.
    /// </summary>
    public void MarkCompleted(int encounterID, float completionTime = 0f, int score = 0)
    {
        var data = GetOrCreateProgression(encounterID);
        bool wasCompleted = data.isCompleted;
        
        data.MarkCompleted(completionTime, score);
        
        if (!wasCompleted)
        {
            EncounterEvents.InvokeEncounterCompleted(encounterID);
            EncounterEvents.InvokeCompletionStateChanged(encounterID, true);
        }
        
        // Auto-unlock next encounters
        UnlockNextEncounters(encounterID);
        
        RefreshStates();
    }
    
    /// <summary>
    /// Mark an encounter as unlocked.
    /// </summary>
    public void MarkUnlocked(int encounterID)
    {
        var data = GetOrCreateProgression(encounterID);
        bool wasUnlocked = data.isUnlocked;
        
        data.MarkUnlocked();
        
        if (!wasUnlocked)
        {
            EncounterEvents.InvokeEncounterUnlocked(encounterID);
            EncounterEvents.InvokeLockStateChanged(encounterID, true);
        }
        
        RefreshStates();
    }
    
    /// <summary>
    /// Unlock all encounters that should be unlocked when the given encounter is completed.
    /// </summary>
    private void UnlockNextEncounters(int completedEncounterID)
    {
        var unlockedList = graphBuilder.GetUnlockedEncounters(completedEncounterID);
        var completedSet = GetCompletedSet();
        
        foreach (int nextId in unlockedList)
        {
            // Check if prerequisites are met
            if (graphBuilder.ArePrerequisitesCompleted(nextId, completedSet))
            {
                MarkUnlocked(nextId);
            }
        }
    }
    
    /// <summary>
    /// Get or create progression data for an encounter.
    /// </summary>
    private EncounterProgressionData GetOrCreateProgression(int encounterID)
    {
        if (!progression.TryGetValue(encounterID, out var data))
        {
            data = new EncounterProgressionData(encounterID);
            progression[encounterID] = data;
        }
        return data;
    }
    
    /// <summary>
    /// Get progression data for an encounter.
    /// </summary>
    public EncounterProgressionData GetProgression(int encounterID)
    {
        if (progression.TryGetValue(encounterID, out var data))
        {
            return data;
        }
        return null;
    }
    
    /// <summary>
    /// Get all progression data.
    /// </summary>
    public Dictionary<int, EncounterProgressionData> GetAllProgression()
    {
        return new Dictionary<int, EncounterProgressionData>(progression);
    }
    
    /// <summary>
    /// Get set of completed encounter IDs.
    /// </summary>
    private HashSet<int> GetCompletedSet()
    {
        HashSet<int> completed = new HashSet<int>();
        foreach (var kvp in progression)
        {
            if (kvp.Value.isCompleted)
            {
                completed.Add(kvp.Key);
            }
        }
        return completed;
    }
    
    /// <summary>
    /// Ensure encounter 1 is always unlocked.
    /// </summary>
    private void EnsureEncounter1Unlocked()
    {
        var data = GetOrCreateProgression(1);
        if (!data.isUnlocked)
        {
            data.MarkUnlocked();
        }
    }
    
    /// <summary>
    /// Refresh state manager with current progression.
    /// </summary>
    private void RefreshStates()
    {
        // Ensure encounter 1 is always unlocked before refreshing states
        EnsureEncounter1Unlocked();
        stateManager.RefreshStates(progression);
    }
    
    /// <summary>
    /// Check if an encounter is unlocked.
    /// </summary>
    public bool IsUnlocked(int encounterID)
    {
        // Encounter 1 is always unlocked
        if (encounterID == 1)
        {
            Debug.Log($"[EncounterProgressionManager] IsUnlocked({encounterID}) - Encounter 1, returning TRUE (always unlocked)");
            // Double-check: ensure it's in progression
            var encounter1Data = GetOrCreateProgression(1);
            if (!encounter1Data.isUnlocked)
            {
                Debug.LogWarning($"[EncounterProgressionManager] Encounter 1 was not unlocked in progression! Fixing now...");
                encounter1Data.MarkUnlocked();
            }
            return true;
        }
        
        if (progression.TryGetValue(encounterID, out var data))
        {
            Debug.Log($"[EncounterProgressionManager] IsUnlocked({encounterID}) - Found in progression: Unlocked={data.isUnlocked}, Completed={data.isCompleted}");
            return data.isUnlocked;
        }
        
        // Check if prerequisites are met
        var completedSet = GetCompletedSet();
        bool prereqsMet = graphBuilder.ArePrerequisitesCompleted(encounterID, completedSet);
        Debug.Log($"[EncounterProgressionManager] IsUnlocked({encounterID}) - Not in progression, checking prerequisites: {prereqsMet}");
        return prereqsMet;
    }
    
    /// <summary>
    /// Check if an encounter is completed.
    /// </summary>
    public bool IsCompleted(int encounterID)
    {
        if (progression.TryGetValue(encounterID, out var data))
        {
            return data.isCompleted;
        }
        return false;
    }
    
    /// <summary>
    /// Clear all progression.
    /// </summary>
    public void Clear()
    {
        progression.Clear();
    }
}

