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
            
            // Ensure this GameObject is at root level (required for DontDestroyOnLoad)
            if (transform.parent != null)
            {
                Debug.LogWarning($"[EmbossingDatabase] GameObject '{gameObject.name}' is not at root level. Moving to root for DontDestroyOnLoad.");
                transform.SetParent(null);
            }
            
            // Mark as DontDestroyOnLoad so it persists across all scenes
            DontDestroyOnLoad(gameObject);
            
            // Load all embossings immediately
            LoadAllEmbossings();
            
            Debug.Log($"[EmbossingDatabase] Initialized and set to persist across scenes. Loaded {allEmbossings.Count} embossings.");
        }
        else if (Instance != this)
        {
            // Another instance already exists, destroy this duplicate
            Debug.Log($"[EmbossingDatabase] Duplicate instance detected. Destroying '{gameObject.name}' and using existing instance.");
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // Only clear instance if this is the actual singleton instance
        if (Instance == this)
        {
            Debug.LogWarning("[EmbossingDatabase] Singleton instance is being destroyed! This should not happen if DontDestroyOnLoad is working correctly.");
            Instance = null;
        }
    }
    
    /// <summary>
    /// Ensure the database is loaded (lazy initialization)
    /// </summary>
    private void EnsureLoaded()
    {
        if (embossingsByID.Count == 0)
        {
            Debug.LogWarning("[EmbossingDatabase] Database was empty, reloading embossings...");
            LoadAllEmbossings();
        }
    }
    
    /// <summary>
    /// Static method to ensure database exists (creates if needed)
    /// This is a fallback - ideally the database should be in the MainGameUI scene
    /// </summary>
    public static void EnsureInstance()
    {
        if (Instance == null)
        {
            GameObject dbObject = new GameObject("EmbossingDatabase");
            Instance = dbObject.AddComponent<EmbossingDatabase>();
            // Awake() will handle DontDestroyOnLoad, but ensure it's at root
            dbObject.transform.SetParent(null);
            DontDestroyOnLoad(dbObject);
            Debug.Log("[EmbossingDatabase] Created instance dynamically (fallback - should be in MainGameUI scene)");
        }
    }
    
    /// <summary>
    /// Load all embossing effects from Resources
    /// </summary>
    void LoadAllEmbossings()
    {
        embossingsByID.Clear();
        allEmbossings.Clear();
        
        Debug.Log($"[EmbossingDatabase] Loading embossings from Resources/{embossingResourcePath}");
        EmbossingEffect[] loadedEmbossings = Resources.LoadAll<EmbossingEffect>(embossingResourcePath);
        
        if (loadedEmbossings == null || loadedEmbossings.Length == 0)
        {
            Debug.LogError($"[EmbossingDatabase] No embossings found at Resources/{embossingResourcePath}! Check that embossing assets are in the correct folder.");
            return;
        }
        
        Debug.Log($"[EmbossingDatabase] Found {loadedEmbossings.Length} embossing assets in Resources");
        
        foreach (EmbossingEffect embossing in loadedEmbossings)
        {
            if (embossing == null)
            {
                Debug.LogWarning("[EmbossingDatabase] Encountered null embossing in Resources, skipping");
                continue;
            }
            
            string id = embossing.embossingId;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[EmbossingDatabase] Embossing '{embossing.embossingName}' has no ID, skipping");
                continue;
            }
            
            // Normalize ID (trim whitespace)
            id = id.Trim();
            
            if (embossingsByID.ContainsKey(id))
            {
                Debug.LogWarning($"[EmbossingDatabase] Duplicate embossing ID '{id}', skipping '{embossing.embossingName}' (already have '{embossingsByID[id].embossingName}')");
                continue;
            }
            
            embossingsByID[id] = embossing;
            allEmbossings.Add(embossing);
        }
        
        Debug.Log($"[EmbossingDatabase] Successfully loaded {allEmbossings.Count} embossing effects");
        
        // Debug: Log all loaded IDs (first 20)
        if (embossingsByID.Count > 0)
        {
            var ids = embossingsByID.Keys.Take(20).ToList();
            Debug.Log($"[EmbossingDatabase] Sample IDs loaded: {string.Join(", ", ids)}");
            
            // Check specifically for 'brutality'
            if (embossingsByID.ContainsKey("brutality"))
            {
                Debug.Log($"[EmbossingDatabase] ✓ 'brutality' embossing is loaded: {embossingsByID["brutality"].embossingName}");
            }
            else
            {
                var brutalityVariants = embossingsByID.Keys.Where(k => k.ToLower().Contains("brutal")).ToList();
                if (brutalityVariants.Count > 0)
                {
                    Debug.LogWarning($"[EmbossingDatabase] 'brutality' not found, but found similar: {string.Join(", ", brutalityVariants)}");
                }
            }
        }
    }
    
    /// <summary>
    /// Get embossing by ID (case-insensitive lookup)
    /// </summary>
    public EmbossingEffect GetEmbossing(string embossingId)
    {
        if (string.IsNullOrEmpty(embossingId))
        {
            Debug.LogWarning("[EmbossingDatabase] GetEmbossing called with null or empty ID");
            return null;
        }
        
        // Ensure database is loaded
        EnsureLoaded();
        
        // Try exact match first (fast path)
        if (embossingsByID.TryGetValue(embossingId, out EmbossingEffect embossing))
        {
            return embossing;
        }
        
        // Try case-insensitive lookup
        string lowerId = embossingId.ToLower().Trim();
        foreach (var kvp in embossingsByID)
        {
            if (kvp.Key.ToLower().Trim() == lowerId)
            {
                Debug.Log($"[EmbossingDatabase] Found embossing '{kvp.Key}' via case-insensitive lookup for '{embossingId}'");
                return kvp.Value;
            }
        }
        
        // Debug: Log available IDs if not found
        if (embossingsByID.Count > 0)
        {
            var sampleIds = embossingsByID.Keys.Take(20).ToList();
            Debug.LogWarning($"[EmbossingDatabase] Embossing not found: '{embossingId}'. Total loaded: {embossingsByID.Count}. Sample IDs: {string.Join(", ", sampleIds)}");
            
            // Check if there's a similar ID (for debugging)
            var similarIds = embossingsByID.Keys.Where(k => k.ToLower().Contains(lowerId) || lowerId.Contains(k.ToLower())).Take(5).ToList();
            if (similarIds.Count > 0)
            {
                Debug.LogWarning($"[EmbossingDatabase] Similar IDs found: {string.Join(", ", similarIds)}");
            }
        }
        else
        {
            Debug.LogError($"[EmbossingDatabase] Database is empty! No embossings loaded. Looking for: '{embossingId}'");
        }
        
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

