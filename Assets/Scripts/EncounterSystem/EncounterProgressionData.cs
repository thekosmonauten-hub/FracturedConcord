using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rich progression data for a single encounter.
/// Tracks completion statistics, unlock state, and custom metadata.
/// </summary>
[System.Serializable]
public class EncounterProgressionData
{
    [Header("Basic State")]
    public int encounterID;
    public bool isCompleted = false;
    public bool isUnlocked = false;
    
    [Header("Statistics")]
    public int completionCount = 0;
    public int attemptCount = 0;
    public float bestCompletionTime = float.MaxValue; // seconds
    public int highestScore = 0; // if scoring system is added
    
    [Header("Timestamps")]
    public DateTime firstCompletedDate = default;
    public DateTime lastCompletedDate = default;
    public DateTime firstAttemptDate = default;
    public DateTime lastAttemptDate = default;
    
    [Header("Extensible Data")]
    public Dictionary<string, object> customStats = new Dictionary<string, object>();
    
    public EncounterProgressionData(int encounterID)
    {
        this.encounterID = encounterID;
    }
    
    /// <summary>
    /// Mark this encounter as attempted.
    /// </summary>
    public void MarkAttempted()
    {
        attemptCount++;
        lastAttemptDate = DateTime.Now;
        
        if (firstAttemptDate == default)
        {
            firstAttemptDate = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Mark this encounter as completed.
    /// </summary>
    public void MarkCompleted(float completionTime = 0f, int score = 0)
    {
        if (!isCompleted)
        {
            isCompleted = true;
            firstCompletedDate = DateTime.Now;
        }
        
        completionCount++;
        lastCompletedDate = DateTime.Now;
        
        if (completionTime > 0 && (bestCompletionTime == float.MaxValue || completionTime < bestCompletionTime))
        {
            bestCompletionTime = completionTime;
        }
        
        if (score > highestScore)
        {
            highestScore = score;
        }
    }
    
    /// <summary>
    /// Mark this encounter as unlocked.
    /// </summary>
    public void MarkUnlocked()
    {
        if (!isUnlocked)
        {
            isUnlocked = true;
        }
    }
    
    /// <summary>
    /// Mark this encounter as locked.
    /// </summary>
    public void MarkLocked()
    {
        isUnlocked = false;
    }
    
    /// <summary>
    /// Get a custom stat value.
    /// </summary>
    public T GetCustomStat<T>(string key, T defaultValue = default(T))
    {
        if (customStats.TryGetValue(key, out object value) && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Set a custom stat value.
    /// </summary>
    public void SetCustomStat<T>(string key, T value)
    {
        customStats[key] = value;
    }
    
    /// <summary>
    /// Reset all progression data for this encounter.
    /// </summary>
    public void Reset()
    {
        isCompleted = false;
        isUnlocked = false;
        completionCount = 0;
        attemptCount = 0;
        bestCompletionTime = float.MaxValue;
        highestScore = 0;
        firstCompletedDate = default;
        lastCompletedDate = default;
        firstAttemptDate = default;
        lastAttemptDate = default;
        customStats.Clear();
    }
}

