using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks a character's Ascendancy progression:
/// - Which Ascendancy they've chosen
/// - Which passives they've unlocked
/// - Available Ascendancy points
/// </summary>
[System.Serializable]
public class CharacterAscendancyProgress
{
    [Header("Ascendancy Selection")]
    public string selectedAscendancy = ""; // Empty = not chosen yet
    public bool ascendancyUnlocked = false;
    
    [Header("Point Progression")]
    public int totalAscendancyPoints = 0;      // Total points earned
    public int spentAscendancyPoints = 0;      // Points spent on passives
    public int availableAscendancyPoints = 0;  // Points available to spend
    
    [Header("Unlocked Passives")]
    public List<string> unlockedPassives = new List<string>();
    
    [Header("Signature Card")]
    public bool signatureCardUnlocked = false;
    
    /// <summary>
    /// Choose an Ascendancy (can only be done once)
    /// </summary>
    public bool ChooseAscendancy(string ascendancyName)
    {
        if (!string.IsNullOrEmpty(selectedAscendancy))
        {
            Debug.LogWarning($"[AscendancyProgress] Already chose Ascendancy: {selectedAscendancy}");
            return false;
        }
        
        selectedAscendancy = ascendancyName;
        ascendancyUnlocked = true;
        signatureCardUnlocked = true; // Signature card unlocks immediately
        
        Debug.Log($"[AscendancyProgress] Chose Ascendancy: {ascendancyName}");
        return true;
    }
    
    /// <summary>
    /// Award Ascendancy points (from completing challenges/quests)
    /// </summary>
    public void AwardPoints(int points)
    {
        totalAscendancyPoints += points;
        availableAscendancyPoints += points;
        Debug.Log($"[AscendancyProgress] Awarded {points} Ascendancy Points (Total: {totalAscendancyPoints}, Available: {availableAscendancyPoints})");
    }
    
    /// <summary>
    /// Unlock a passive ability
    /// </summary>
    public bool UnlockPassive(AscendancyPassive passive, AscendancyData ascendancy)
    {
        if (passive == null)
        {
            Debug.LogError("[AscendancyProgress] Cannot unlock null passive!");
            return false;
        }
        
        // Check if already unlocked
        if (unlockedPassives.Contains(passive.name))
        {
            Debug.LogWarning($"[AscendancyProgress] Passive already unlocked: {passive.name}");
            return false;
        }
        
        // Check point cost
        if (availableAscendancyPoints < passive.pointCost)
        {
            Debug.LogWarning($"[AscendancyProgress] Not enough points! Need {passive.pointCost}, have {availableAscendancyPoints}");
            return false;
        }
        
        // Check prerequisites
        if (passive.prerequisitePassives != null && passive.prerequisitePassives.Count > 0)
        {
            if (passive.requireAllPrerequisites)
            {
                foreach (string prereq in passive.prerequisitePassives)
                {
                    if (!unlockedPassives.Contains(prereq))
                    {
                        Debug.LogWarning($"[AscendancyProgress] Missing prerequisite: {prereq}");
                        return false;
                    }
                }
            }
            else
            {
                bool anyUnlocked = false;
                foreach (string prereq in passive.prerequisitePassives)
                {
                    if (unlockedPassives.Contains(prereq))
                    {
                        anyUnlocked = true;
                        break;
                    }
                }

                if (!anyUnlocked)
                {
                    Debug.LogWarning($"[AscendancyProgress] Requires any prerequisite unlocked: {string.Join(", ", passive.prerequisitePassives)}");
                    return false;
                }
            }
        }
        
        // Unlock passive
        unlockedPassives.Add(passive.name);
        spentAscendancyPoints += passive.pointCost;
        availableAscendancyPoints -= passive.pointCost;
        
        Debug.Log($"[AscendancyProgress] Unlocked passive: {passive.name} (Cost: {passive.pointCost}, Remaining: {availableAscendancyPoints})");
        return true;
    }
    
    /// <summary>
    /// Check if a passive is unlocked
    /// </summary>
    public bool IsPassiveUnlocked(string passiveName)
    {
        return unlockedPassives.Contains(passiveName);
    }
    
    /// <summary>
    /// Get number of unlocked passives
    /// </summary>
    public int GetUnlockedPassiveCount()
    {
        return unlockedPassives.Count;
    }
    
    /// <summary>
    /// Refund all points (for testing or respec)
    /// </summary>
    public void RefundAllPoints()
    {
        availableAscendancyPoints = totalAscendancyPoints;
        spentAscendancyPoints = 0;
        unlockedPassives.Clear();
        Debug.Log($"[AscendancyProgress] Refunded all points. Available: {availableAscendancyPoints}");
    }
    
    /// <summary>
    /// Get summary of progression
    /// </summary>
    public string GetProgressSummary()
    {
        if (!ascendancyUnlocked)
            return "No Ascendancy chosen";
        
        return $"{selectedAscendancy}\n" +
               $"Points: {spentAscendancyPoints}/{totalAscendancyPoints} spent ({availableAscendancyPoints} available)\n" +
               $"Passives: {unlockedPassives.Count} unlocked";
    }
}

