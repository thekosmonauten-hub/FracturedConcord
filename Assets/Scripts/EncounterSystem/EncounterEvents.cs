using System;

/// <summary>
/// Centralized event system for encounter-related events.
/// Provides decoupled communication between encounter system components.
/// </summary>
public static class EncounterEvents
{
    // Progression Events
    public static event Action<int> OnEncounterUnlocked;
    public static event Action<int> OnEncounterCompleted;
    public static event Action<int> OnEncounterEntered;
    public static event Action<int> OnEncounterFailed;
    public static event Action<int> OnEncounterAttempted;
    
    // State Change Events
    public static event Action<int, bool> OnEncounterLockStateChanged;
    public static event Action<int, bool> OnEncounterCompletionStateChanged;
    
    // Graph Events
    public static event Action OnEncounterGraphBuilt;
    public static event Action OnProgressionApplied;
    public static event Action OnEncounterDataLoaded;
    
    // System Events
    public static event Action OnEncounterSystemInitialized;
    public static event Action OnEncounterSystemReset;

    /// <summary>
    /// Invoke when an encounter is unlocked.
    /// </summary>
    public static void InvokeEncounterUnlocked(int encounterID)
    {
        OnEncounterUnlocked?.Invoke(encounterID);
    }

    /// <summary>
    /// Invoke when an encounter is completed.
    /// </summary>
    public static void InvokeEncounterCompleted(int encounterID)
    {
        OnEncounterCompleted?.Invoke(encounterID);
    }

    /// <summary>
    /// Invoke when an encounter is entered.
    /// </summary>
    public static void InvokeEncounterEntered(int encounterID)
    {
        OnEncounterEntered?.Invoke(encounterID);
    }

    /// <summary>
    /// Invoke when an encounter attempt fails.
    /// </summary>
    public static void InvokeEncounterFailed(int encounterID)
    {
        OnEncounterFailed?.Invoke(encounterID);
    }

    /// <summary>
    /// Invoke when an encounter is attempted (entered but not yet completed).
    /// </summary>
    public static void InvokeEncounterAttempted(int encounterID)
    {
        OnEncounterAttempted?.Invoke(encounterID);
    }

    /// <summary>
    /// Invoke when an encounter's lock state changes.
    /// </summary>
    public static void InvokeLockStateChanged(int encounterID, bool isUnlocked)
    {
        OnEncounterLockStateChanged?.Invoke(encounterID, isUnlocked);
    }

    /// <summary>
    /// Invoke when an encounter's completion state changes.
    /// </summary>
    public static void InvokeCompletionStateChanged(int encounterID, bool isCompleted)
    {
        OnEncounterCompletionStateChanged?.Invoke(encounterID, isCompleted);
    }

    /// <summary>
    /// Invoke when the encounter graph is built.
    /// </summary>
    public static void InvokeGraphBuilt()
    {
        OnEncounterGraphBuilt?.Invoke();
    }

    /// <summary>
    /// Invoke when character progression is applied.
    /// </summary>
    public static void InvokeProgressionApplied()
    {
        OnProgressionApplied?.Invoke();
    }

    /// <summary>
    /// Invoke when encounter data is loaded.
    /// </summary>
    public static void InvokeDataLoaded()
    {
        OnEncounterDataLoaded?.Invoke();
    }

    /// <summary>
    /// Invoke when the encounter system is initialized.
    /// </summary>
    public static void InvokeSystemInitialized()
    {
        OnEncounterSystemInitialized?.Invoke();
    }

    /// <summary>
    /// Invoke when the encounter system is reset.
    /// </summary>
    public static void InvokeSystemReset()
    {
        OnEncounterSystemReset?.Invoke();
    }

    /// <summary>
    /// Clear all event subscriptions. Useful for cleanup.
    /// </summary>
    public static void ClearAllEvents()
    {
        OnEncounterUnlocked = null;
        OnEncounterCompleted = null;
        OnEncounterEntered = null;
        OnEncounterFailed = null;
        OnEncounterAttempted = null;
        OnEncounterLockStateChanged = null;
        OnEncounterCompletionStateChanged = null;
        OnEncounterGraphBuilt = null;
        OnProgressionApplied = null;
        OnEncounterDataLoaded = null;
        OnEncounterSystemInitialized = null;
        OnEncounterSystemReset = null;
    }
}

