using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages card experience and leveling system.
/// Tracks experience per groupKey (not per individual card instance).
/// All cards with the same groupKey share experience and level.
/// Persists with character save data.
/// </summary>
public class CardExperienceManager : MonoBehaviour
{
    #region Singleton
    private static CardExperienceManager _instance;
    public static CardExperienceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CardExperienceManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CardExperienceManager");
                    _instance = go.AddComponent<CardExperienceManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion
    
    // Experience data per groupKey
    private Dictionary<string, CardExperienceData> cardExperience = new Dictionary<string, CardExperienceData>();
    
    // Events
    public System.Action<string, int> OnCardLevelUp; // groupKey, newLevel
    public System.Action<string, int> OnCardGainExperience; // groupKey, amount
    
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
    /// Add experience to a card group.
    /// All cards with this groupKey gain experience together.
    /// </summary>
    public void AddCardExperience(string groupKey, int amount)
    {
        if (string.IsNullOrEmpty(groupKey) || amount <= 0) return;
        
        // Get or create experience data
        if (!cardExperience.ContainsKey(groupKey))
        {
            cardExperience[groupKey] = new CardExperienceData
            {
                groupKey = groupKey,
                level = 1,
                experience = 0
            };
        }
        
        CardExperienceData data = cardExperience[groupKey];
        data.experience += amount;
        
        OnCardGainExperience?.Invoke(groupKey, amount);
        
        // Check for level ups
        bool leveledUp = false;
        while (data.CanLevelUp())
        {
            int requiredXP = data.GetRequiredExperienceForNextLevel();
            data.experience -= requiredXP;
            data.level++;
            leveledUp = true;
            
            Debug.Log($"[Card Level Up] {groupKey} reached level {data.level}! Bonus: {data.GetLevelBonusMultiplier():P1}");
            OnCardLevelUp?.Invoke(groupKey, data.level);
        }
        
        if (leveledUp)
        {
            // Update all cards with this groupKey in active deck
            SyncLevelToActiveCards(groupKey, data.level, data.experience);
        }
    }
    
    /// <summary>
    /// Get the level of a card group.
    /// </summary>
    public int GetCardLevel(string groupKey)
    {
        if (cardExperience.ContainsKey(groupKey))
        {
            return cardExperience[groupKey].level;
        }
        return 1; // Default level
    }
    
    /// <summary>
    /// Get the experience of a card group.
    /// </summary>
    public int GetCardExperience(string groupKey)
    {
        if (cardExperience.ContainsKey(groupKey))
        {
            return cardExperience[groupKey].experience;
        }
        return 0;
    }
    
    /// <summary>
    /// Get experience data for a card group.
    /// </summary>
    public CardExperienceData GetCardExperienceData(string groupKey)
    {
        if (cardExperience.ContainsKey(groupKey))
        {
            return cardExperience[groupKey];
        }
        
        // Return default data
        return new CardExperienceData
        {
            groupKey = groupKey,
            level = 1,
            experience = 0
        };
    }
    
    /// <summary>
    /// Sync card level/experience to all cards in active deck with matching groupKey.
    /// Called after a card levels up.
    /// </summary>
    private void SyncLevelToActiveCards(string groupKey, int level, int experience)
    {
        if (DeckManager.Instance == null || !DeckManager.Instance.HasActiveDeck()) return;
        
        List<Card> deckCards = DeckManager.Instance.GetActiveDeckAsCards();
        
        foreach (Card card in deckCards)
        {
            if (card.GetGroupKey() == groupKey)
            {
                card.cardLevel = level;
                card.cardExperience = experience;
            }
        }
    }
    
    /// <summary>
    /// Load card experience data from character save.
    /// </summary>
    public void LoadFromCharacter(Character character)
    {
        if (character == null || character.deckData == null) return;
        
        cardExperience.Clear();
        
        if (character.deckData.cardExperienceData != null)
        {
            foreach (var data in character.deckData.cardExperienceData)
            {
                cardExperience[data.groupKey] = data;
            }
            
            Debug.Log($"[CardExperience] Loaded experience data for {cardExperience.Count} card groups");
        }
    }
    
    /// <summary>
    /// Save card experience data to character.
    /// </summary>
    public void SaveToCharacter(Character character)
    {
        if (character == null || character.deckData == null) return;
        
        character.deckData.cardExperienceData = new List<CardExperienceData>(cardExperience.Values);
        
        Debug.Log($"[CardExperience] Saved experience data for {cardExperience.Count} card groups");
    }
    
    /// <summary>
    /// Get all card experience data (for save system).
    /// </summary>
    public List<CardExperienceData> GetAllCardExperienceData()
    {
        return new List<CardExperienceData>(cardExperience.Values);
    }
    
    /// <summary>
    /// Apply experience gains from combat.
    /// Distributes experience to all cards in the active deck.
    /// </summary>
    public void ApplyCombatExperience(int baseExperience)
    {
        if (DeckManager.Instance == null || !DeckManager.Instance.HasActiveDeck()) return;
        
        List<Card> deckCards = DeckManager.Instance.GetActiveDeckAsCards();
        
        // Get unique groupKeys
        HashSet<string> processedGroups = new HashSet<string>();
        
        foreach (Card card in deckCards)
        {
            string groupKey = card.GetGroupKey();
            
            if (!processedGroups.Contains(groupKey))
            {
                AddCardExperience(groupKey, baseExperience);
                processedGroups.Add(groupKey);
            }
        }
        
        Debug.Log($"[CardExperience] Applied {baseExperience} XP to {processedGroups.Count} unique card groups");
    }
}

/// <summary>
/// Stores experience and level data for a card group.
/// All cards with the same groupKey share this data.
/// </summary>
[System.Serializable]
public class CardExperienceData
{
    public string groupKey;
    public int level = 1;
    public int experience = 0;
    
    /// <summary>
    /// Calculate the bonus multiplier for this card level.
    /// </summary>
    public float GetLevelBonusMultiplier()
    {
        // Level 1 = 1.00x (no bonus)
        // Level 20 = 1.10x (+10% bonus)
        return 1.0f + ((level - 1) * 0.005263f);
    }
    
    /// <summary>
    /// Get experience required for next level.
    /// </summary>
    public int GetRequiredExperienceForNextLevel()
    {
        if (level >= 20) return 0;
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

