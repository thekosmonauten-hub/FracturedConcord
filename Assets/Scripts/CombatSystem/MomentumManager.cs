using UnityEngine;
using System;

/// <summary>
/// Manages the Momentum resource system for Brawler and other classes.
/// Momentum is a combat resource that:
/// - Can be gained/spent by cards
/// - Persists across turns (with optional decay)
/// - Triggers threshold effects at 3+, 5+, 7+, 8+, 10+
/// - Can be displayed in UI
/// </summary>
[Serializable]
public class MomentumManager
{
    [Header("Momentum State")]
    [Tooltip("Current momentum value")]
    public int currentMomentum = 0;
    
    [Tooltip("Maximum momentum cap (0 = no cap)")]
    public int maxMomentum = 10;
    
    [Tooltip("Momentum decay per turn (0 = no decay)")]
    public int momentumDecayPerTurn = 0;
    
    [Tooltip("Multiplier for momentum gain (e.g., 1.5 = +50% more momentum gained)")]
    public float momentumGainMultiplier = 1f;
    
    [Header("Threshold Tracking")]
    [Tooltip("Was at 3+ momentum last check?")]
    public bool wasAtThreshold3 = false;
    
    [Tooltip("Was at 5+ momentum last check?")]
    public bool wasAtThreshold5 = false;
    
    [Tooltip("Was at 7+ momentum last check?")]
    public bool wasAtThreshold7 = false;
    
    [Tooltip("Was at 8+ momentum last check?")]
    public bool wasAtThreshold8 = false;
    
    [Tooltip("Was at 10 momentum last check?")]
    public bool wasAtThreshold10 = false;
    
    /// <summary>
    /// Gain momentum (respects multiplier and cap)
    /// </summary>
    public int GainMomentum(int amount)
    {
        if (amount <= 0) return 0;
        
        int actualGain = Mathf.RoundToInt(amount * momentumGainMultiplier);
        int previousMomentum = currentMomentum;
        
        currentMomentum += actualGain;
        
        if (maxMomentum > 0)
        {
            currentMomentum = Mathf.Min(currentMomentum, maxMomentum);
        }
        
        int gained = currentMomentum - previousMomentum;
        
        if (gained > 0)
        {
            UpdateThresholdFlags();
            Debug.Log($"<color=cyan>[Momentum] Gained {gained} momentum (total: {currentMomentum})</color>");
        }
        
        return gained;
    }
    
    /// <summary>
    /// Spend momentum (returns amount actually spent)
    /// </summary>
    public int SpendMomentum(int amount)
    {
        if (amount <= 0) return 0;
        
        int spent = Mathf.Min(amount, currentMomentum);
        currentMomentum -= spent;
        
        UpdateThresholdFlags();
        
        if (spent > 0)
        {
            Debug.Log($"<color=orange>[Momentum] Spent {spent} momentum (remaining: {currentMomentum})</color>");
        }
        
        return spent;
    }
    
    /// <summary>
    /// Spend all momentum (returns amount spent)
    /// </summary>
    public int SpendAllMomentum()
    {
        int spent = currentMomentum;
        currentMomentum = 0;
        UpdateThresholdFlags();
        
        if (spent > 0)
        {
            Debug.Log($"<color=orange>[Momentum] Spent all {spent} momentum</color>");
        }
        
        return spent;
    }
    
    /// <summary>
    /// Set momentum to a specific value
    /// </summary>
    public void SetMomentum(int value)
    {
        int previousMomentum = currentMomentum;
        currentMomentum = Mathf.Max(0, value);
        
        if (maxMomentum > 0)
        {
            currentMomentum = Mathf.Min(currentMomentum, maxMomentum);
        }
        
        if (currentMomentum != previousMomentum)
        {
            UpdateThresholdFlags();
        }
    }
    
    /// <summary>
    /// Check if momentum is at or above a threshold
    /// </summary>
    public bool HasMomentum(int threshold)
    {
        return currentMomentum >= threshold;
    }
    
    /// <summary>
    /// Get current momentum value
    /// </summary>
    public int GetMomentum()
    {
        return currentMomentum;
    }
    
    /// <summary>
    /// Get momentum percentage (0-1) for UI display
    /// </summary>
    public float GetMomentumPercentage()
    {
        if (maxMomentum <= 0) return 0f;
        return Mathf.Clamp01((float)currentMomentum / maxMomentum);
    }
    
    /// <summary>
    /// Apply decay at turn start
    /// </summary>
    public void OnTurnStart()
    {
        if (momentumDecayPerTurn > 0 && currentMomentum > 0)
        {
            int previousMomentum = currentMomentum;
            currentMomentum = Mathf.Max(0, currentMomentum - momentumDecayPerTurn);
            
            if (currentMomentum != previousMomentum)
            {
                UpdateThresholdFlags();
                Debug.Log($"<color=yellow>[Momentum] Decayed {previousMomentum - currentMomentum} momentum (now: {currentMomentum})</color>");
            }
        }
    }
    
    /// <summary>
    /// Reset momentum to 0 (for combat start/end)
    /// </summary>
    public void Reset()
    {
        currentMomentum = 0;
        UpdateThresholdFlags();
    }
    
    /// <summary>
    /// Update threshold flags (for detecting threshold crossings)
    /// </summary>
    private void UpdateThresholdFlags()
    {
        wasAtThreshold3 = currentMomentum >= 3;
        wasAtThreshold5 = currentMomentum >= 5;
        wasAtThreshold7 = currentMomentum >= 7;
        wasAtThreshold8 = currentMomentum >= 8;
        wasAtThreshold10 = currentMomentum >= 10;
    }
    
    /// <summary>
    /// Check if momentum crossed a threshold (went from below to at/above)
    /// </summary>
    public bool CrossedThreshold(int threshold)
    {
        switch (threshold)
        {
            case 3: return wasAtThreshold3 && currentMomentum >= 3;
            case 5: return wasAtThreshold5 && currentMomentum >= 5;
            case 7: return wasAtThreshold7 && currentMomentum >= 7;
            case 8: return wasAtThreshold8 && currentMomentum >= 8;
            case 10: return wasAtThreshold10 && currentMomentum >= 10;
            default: return currentMomentum >= threshold;
        }
    }
}

