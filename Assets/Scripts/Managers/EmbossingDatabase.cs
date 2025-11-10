using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton manager for all embossing effects in the game
/// Loads and provides access to embossing definitions
/// </summary>
public class EmbossingDatabase : MonoBehaviour
{
    public static EmbossingDatabase Instance { get; private set; }
    
    [Header("Database")]
    [Tooltip("Folder path to load embossings from (relative to Resources)")]
    [SerializeField] private string embossingResourcePath = "Embossings";
    
    [Tooltip("All loaded embossing effects")]
    private Dictionary<string, EmbossingEffect> embossingsByID = new Dictionary<string, EmbossingEffect>();
    
    private List<EmbossingEffect> allEmbossings = new List<EmbossingEffect>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllEmbossings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load all embossing effects from Resources
    /// </summary>
    void LoadAllEmbossings()
    {
        embossingsByID.Clear();
        allEmbossings.Clear();
        
        EmbossingEffect[] loadedEmbossings = Resources.LoadAll<EmbossingEffect>(embossingResourcePath);
        
        foreach (EmbossingEffect embossing in loadedEmbossings)
        {
            if (embossing == null) continue;
            
            string id = embossing.embossingId;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[EmbossingDatabase] Embossing '{embossing.embossingName}' has no ID, skipping");
                continue;
            }
            
            if (embossingsByID.ContainsKey(id))
            {
                Debug.LogWarning($"[EmbossingDatabase] Duplicate embossing ID '{id}', skipping '{embossing.embossingName}'");
                continue;
            }
            
            embossingsByID[id] = embossing;
            allEmbossings.Add(embossing);
        }
        
        Debug.Log($"[EmbossingDatabase] Loaded {allEmbossings.Count} embossing effects");
    }
    
    /// <summary>
    /// Get embossing by ID
    /// </summary>
    public EmbossingEffect GetEmbossing(string embossingId)
    {
        if (embossingsByID.TryGetValue(embossingId, out EmbossingEffect embossing))
        {
            return embossing;
        }
        
        Debug.LogWarning($"[EmbossingDatabase] Embossing not found: {embossingId}");
        return null;
    }
    
    /// <summary>
    /// Get all embossings
    /// </summary>
    public List<EmbossingEffect> GetAllEmbossings()
    {
        return new List<EmbossingEffect>(allEmbossings);
    }
    
    /// <summary>
    /// Get embossings by category
    /// </summary>
    public List<EmbossingEffect> GetEmbossingsByCategory(EmbossingCategory category)
    {
        return allEmbossings.Where(e => e.category == category).ToList();
    }
    
    /// <summary>
    /// Get embossings by rarity
    /// </summary>
    public List<EmbossingEffect> GetEmbossingsByRarity(EmbossingRarity rarity)
    {
        return allEmbossings.Where(e => e.rarity == rarity).ToList();
    }
    
    /// <summary>
    /// Get embossings that the character can apply (meets requirements)
    /// </summary>
    public List<EmbossingEffect> GetAvailableEmbossings(Character character)
    {
        return allEmbossings.Where(e => e.MeetsRequirements(character)).ToList();
    }
    
    /// <summary>
    /// Get embossings available for a specific card
    /// Filters by character requirements and card compatibility
    /// </summary>
    public List<EmbossingEffect> GetEmbossingsForCard(Card card, Character character)
    {
        List<EmbossingEffect> available = new List<EmbossingEffect>();
        
        foreach (EmbossingEffect embossing in allEmbossings)
        {
            // Check character requirements
            if (!embossing.MeetsRequirements(character))
                continue;
            
            // Check if card has embossing slots
            if (!card.HasEmptyEmbossingSlot())
                continue;
            
            // Check if embossing is unique and already applied
            if (embossing.unique && card.appliedEmbossings.Any(ei => ei.embossingId == embossing.embossingId))
                continue;
            
            // Check exclusivity group
            if (!string.IsNullOrEmpty(embossing.exclusivityGroup))
            {
                bool hasConflict = card.appliedEmbossings.Any(ei =>
                {
                    EmbossingEffect applied = GetEmbossing(ei.embossingId);
                    return applied != null && applied.exclusivityGroup == embossing.exclusivityGroup;
                });
                
                if (hasConflict)
                    continue;
            }
            
            available.Add(embossing);
        }
        
        return available;
    }
    
    /// <summary>
    /// Calculate total mana cost increase for a card
    /// </summary>
    public int CalculateCardManaCost(Card card, int baseManaCost)
    {
        if (card.appliedEmbossings.Count == 0)
            return baseManaCost;
        
        // Count embossings
        int embossingCount = card.appliedEmbossings.Count;
        
        // Sum all multipliers
        float totalMultiplier = 0f;
        int totalFlatIncrease = 0;
        
        foreach (EmbossingInstance instance in card.appliedEmbossings)
        {
            EmbossingEffect embossing = GetEmbossing(instance.embossingId);
            if (embossing == null) continue;
            
            totalMultiplier += embossing.manaCostMultiplier;
            totalFlatIncrease += embossing.flatManaCostIncrease;
        }
        
        // Formula: (Base + N_embossings) × (1 + Σ Multipliers) + Flat increases
        float multipliedCost = (baseManaCost + embossingCount) * (1 + totalMultiplier);
        int finalCost = Mathf.CeilToInt(multipliedCost) + totalFlatIncrease;
        
        return finalCost;
    }
    
    /// <summary>
    /// Get formatted mana cost breakdown for display
    /// </summary>
    public string GetManaCostBreakdown(Card card, int baseManaCost)
    {
        if (card.appliedEmbossings.Count == 0)
            return $"Base Cost: {baseManaCost}";
        
        int finalCost = CalculateCardManaCost(card, baseManaCost);
        int embossingCount = card.appliedEmbossings.Count;
        
        float totalMultiplier = 0f;
        foreach (EmbossingInstance instance in card.appliedEmbossings)
        {
            EmbossingEffect embossing = GetEmbossing(instance.embossingId);
            if (embossing != null)
                totalMultiplier += embossing.manaCostMultiplier;
        }
        
        string breakdown = $"Base: {baseManaCost}\n";
        breakdown += $"Embossings: +{embossingCount} flat\n";
        breakdown += $"Multiplier: +{(totalMultiplier * 100):F0}%\n";
        breakdown += $"Final Cost: {finalCost}";
        
        return breakdown;
    }
}

