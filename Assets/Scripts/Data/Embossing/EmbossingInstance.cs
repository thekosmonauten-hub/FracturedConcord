using UnityEngine;

/// <summary>
/// Represents a specific embossing applied to a card.
/// Tracks the embossing effect, level, and experience.
/// Each embossing can level up independently (max level 20, +20% bonus).
/// </summary>
[System.Serializable]
public class EmbossingInstance
{
    [Tooltip("ID of the embossing effect (references EmbossingEffect asset name)")]
    public string embossingId = "";
    
    [Tooltip("Current level of this embossing (1-20)")]
    [Range(1, 20)]
    public int level = 1;
    
    [Tooltip("Current experience points for this embossing")]
    public int experience = 0;
    
    [Tooltip("Slot index where this embossing is applied (0-4 for slots 1-5)")]
    public int slotIndex = 0;
    
    /// <summary>
    /// Calculate the bonus multiplier for this embossing level.
    /// Max level (20) provides +20% bonus (1.20x multiplier).
    /// </summary>
    public float GetLevelBonusMultiplier()
    {
        // Level 1 = 1.00x (no bonus)
        // Level 20 = 1.20x (+20% bonus)
        // Linear scaling: ~1.053% per level
        return 1.0f + ((level - 1) * 0.010526f); // 0.010526 * 19 levels = ~0.20
    }
    
    /// <summary>
    /// Get experience required for next level.
    /// Uses same curve as cards but scaled for embossing importance.
    /// </summary>
    public int GetRequiredExperienceForNextLevel()
    {
        if (level >= 20) return 0; // Max level
        
        // Base XP: 150 (50% more than cards, since embossings are more impactful)
        // Scaling: 1.15x per level (exponential growth)
        return Mathf.RoundToInt(150f * Mathf.Pow(1.15f, level - 1));
    }
    
    /// <summary>
    /// Check if embossing is ready to level up.
    /// </summary>
    public bool CanLevelUp()
    {
        return level < 20 && experience >= GetRequiredExperienceForNextLevel();
    }
    
    /// <summary>
    /// Add experience to this embossing and handle level ups.
    /// Returns true if embossing leveled up.
    /// </summary>
    public bool AddExperience(int amount)
    {
        if (level >= 20) return false; // Max level
        
        experience += amount;
        bool leveledUp = false;
        
        // Check for level ups (handle multiple levels if enough XP)
        while (CanLevelUp())
        {
            int requiredXP = GetRequiredExperienceForNextLevel();
            experience -= requiredXP;
            level++;
            leveledUp = true;
            
            Debug.Log($"[Embossing Level Up] {embossingId} reached level {level}! Bonus: {GetLevelBonusMultiplier():P1}");
        }
        
        return leveledUp;
    }
    
    /// <summary>
    /// Get progress to next level (0.0 to 1.0).
    /// </summary>
    public float GetLevelProgress()
    {
        if (level >= 20) return 1.0f;
        
        int required = GetRequiredExperienceForNextLevel();
        if (required == 0) return 1.0f;
        
        return Mathf.Clamp01((float)experience / required);
    }
    
    /// <summary>
    /// Create a new embossing instance.
    /// </summary>
    public EmbossingInstance(string id, int slot = 0)
    {
        embossingId = id;
        slotIndex = slot;
        level = 1;
        experience = 0;
    }
    
    /// <summary>
    /// Default constructor for serialization.
    /// </summary>
    public EmbossingInstance()
    {
        embossingId = "";
        slotIndex = 0;
        level = 1;
        experience = 0;
    }
}

