using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages Reliance Aura experience and leveling system.
/// Tracks experience per aura name (all instances of the same aura share experience).
/// Persists with character save data.
/// Similar to CardExperienceManager but for auras.
/// </summary>
public class AuraExperienceManager : MonoBehaviour
{
    #region Singleton
    private static AuraExperienceManager _instance;
    public static AuraExperienceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AuraExperienceManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AuraExperienceManager");
                    _instance = go.AddComponent<AuraExperienceManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion
    
    // Experience data per aura name
    private Dictionary<string, AuraExperienceData> auraExperience = new Dictionary<string, AuraExperienceData>();
    
    // Events
    public System.Action<string, int> OnAuraLevelUp; // auraName, newLevel
    public System.Action<string, int> OnAuraGainExperience; // auraName, amount
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Add experience to an aura.
    /// All instances of the same aura name share experience.
    /// </summary>
    public void AddAuraExperience(string auraName, int amount)
    {
        if (string.IsNullOrEmpty(auraName) || amount <= 0) return;
        
        // Get or create experience data
        if (!auraExperience.ContainsKey(auraName))
        {
            auraExperience[auraName] = new AuraExperienceData
            {
                auraName = auraName,
                level = 1,
                experience = 0
            };
        }
        
        AuraExperienceData data = auraExperience[auraName];
        data.experience += amount;
        
        OnAuraGainExperience?.Invoke(auraName, amount);
        
        // Check for level ups
        bool leveledUp = false;
        while (data.CanLevelUp())
        {
            int requiredXP = data.GetRequiredExperienceForNextLevel();
            data.experience -= requiredXP;
            data.level++;
            leveledUp = true;
            
            Debug.Log($"[Aura Level Up] {auraName} reached level {data.level}!");
            OnAuraLevelUp?.Invoke(auraName, data.level);
        }
    }
    
    /// <summary>
    /// Get the level of an aura.
    /// </summary>
    public int GetAuraLevel(string auraName)
    {
        if (auraExperience.ContainsKey(auraName))
        {
            return auraExperience[auraName].level;
        }
        return 1; // Default level
    }
    
    /// <summary>
    /// Get the experience of an aura.
    /// </summary>
    public int GetAuraExperience(string auraName)
    {
        if (auraExperience.ContainsKey(auraName))
        {
            return auraExperience[auraName].experience;
        }
        return 0;
    }
    
    /// <summary>
    /// Get experience data for an aura.
    /// </summary>
    public AuraExperienceData GetAuraExperienceData(string auraName)
    {
        if (auraExperience.ContainsKey(auraName))
        {
            return auraExperience[auraName];
        }
        
        // Return default data
        return new AuraExperienceData
        {
            auraName = auraName,
            level = 1,
            experience = 0
        };
    }
    
    /// <summary>
    /// Load aura experience data from character save.
    /// </summary>
    public void LoadFromCharacter(Character character)
    {
        if (character == null) return;
        
        auraExperience.Clear();
        
        if (character.auraExperienceData != null)
        {
            foreach (var data in character.auraExperienceData)
            {
                auraExperience[data.auraName] = data;
            }
            
            Debug.Log($"[AuraExperience] Loaded experience data for {auraExperience.Count} auras");
        }
    }
    
    /// <summary>
    /// Save aura experience data to character.
    /// </summary>
    public void SaveToCharacter(Character character)
    {
        if (character == null) return;
        
        character.auraExperienceData = new List<AuraExperienceData>(auraExperience.Values);
        
        Debug.Log($"[AuraExperience] Saved experience data for {auraExperience.Count} auras");
    }
    
    /// <summary>
    /// Get all aura experience data (for save system).
    /// </summary>
    public List<AuraExperienceData> GetAllAuraExperienceData()
    {
        return new List<AuraExperienceData>(auraExperience.Values);
    }
    
    /// <summary>
    /// Apply experience gains from combat.
    /// Distributes experience to all active auras.
    /// </summary>
    public void ApplyCombatExperience(int baseExperience)
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter()) return;
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null || character.activeRelianceAuras == null) return;
        
        // Get unique aura names (active auras gain experience)
        HashSet<string> processedAuras = new HashSet<string>();
        
        foreach (string auraName in character.activeRelianceAuras)
        {
            if (!string.IsNullOrEmpty(auraName) && !processedAuras.Contains(auraName))
            {
                AddAuraExperience(auraName, baseExperience);
                processedAuras.Add(auraName);
            }
        }
        
        if (processedAuras.Count > 0)
        {
            Debug.Log($"[AuraExperience] Applied {baseExperience} XP to {processedAuras.Count} active auras");
        }
    }
}

/// <summary>
/// Stores experience and level data for a Reliance Aura.
/// All instances of the same aura name share this data.
/// </summary>
[System.Serializable]
public class AuraExperienceData
{
    public string auraName;
    public int level = 1;
    public int experience = 0;
    
    /// <summary>
    /// Get experience required for next level.
    /// Uses same exponential curve as cards: 100 * (1.15^(level-1))
    /// </summary>
    public int GetRequiredExperienceForNextLevel()
    {
        if (level >= 20) return 0; // Max level
        return Mathf.RoundToInt(100f * Mathf.Pow(1.15f, level - 1));
    }
    
    /// <summary>
    /// Check if ready to level up.
    /// </summary>
    public bool CanLevelUp()
    {
        return level < 20 && experience >= GetRequiredExperienceForNextLevel();
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
}

