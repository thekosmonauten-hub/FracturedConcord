using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject database containing all embossing effects.
/// This allows for faster loading via preloading instead of Resources.LoadAll.
/// Note: This is the ScriptableObject asset. The MonoBehaviour manager is EmbossingDatabase (in Managers folder).
/// </summary>
[CreateAssetMenu(fileName = "EmbossingDatabase", menuName = "FracturedConcord/EmbossingDatabase", order = 1)]
public class EmbossingDatabaseAsset : ScriptableObject
{
    [Header("Embossing Effects")]
    [Tooltip("All embossing effects in the game")]
    [SerializeField] private List<EmbossingEffect> allEmbossings = new List<EmbossingEffect>();
    
    [Header("Metadata")]
    [Tooltip("Path where embossings are stored in Resources (for reference)")]
    [SerializeField] private string sourceResourcePath = "Embossings";
    
    /// <summary>
    /// Get all embossing effects
    /// </summary>
    public List<EmbossingEffect> GetAllEmbossings()
    {
        return new List<EmbossingEffect>(allEmbossings);
    }
    
    /// <summary>
    /// Get embossing by ID (case-insensitive lookup)
    /// </summary>
    public EmbossingEffect GetEmbossing(string embossingId)
    {
        if (string.IsNullOrEmpty(embossingId))
        {
            return null;
        }
        
        // Try exact match first (fast path)
        var exactMatch = allEmbossings.FirstOrDefault(e => e != null && e.embossingId == embossingId);
        if (exactMatch != null)
        {
            return exactMatch;
        }
        
        // Try case-insensitive lookup
        string lowerId = embossingId.ToLower().Trim();
        return allEmbossings.FirstOrDefault(e => 
            e != null && 
            !string.IsNullOrEmpty(e.embossingId) && 
            e.embossingId.ToLower().Trim() == lowerId);
    }
    
    /// <summary>
    /// Get embossings by category
    /// </summary>
    public List<EmbossingEffect> GetEmbossingsByCategory(EmbossingCategory category)
    {
        return allEmbossings.Where(e => e != null && e.category == category).ToList();
    }
    
    /// <summary>
    /// Get embossings by rarity
    /// </summary>
    public List<EmbossingEffect> GetEmbossingsByRarity(EmbossingRarity rarity)
    {
        return allEmbossings.Where(e => e != null && e.rarity == rarity).ToList();
    }
    
    /// <summary>
    /// Get embossings that the character can apply (meets requirements)
    /// </summary>
    public List<EmbossingEffect> GetAvailableEmbossings(Character character)
    {
        return allEmbossings.Where(e => e != null && e.MeetsRequirements(character)).ToList();
    }
    
    /// <summary>
    /// Get embossings available for a specific card
    /// </summary>
    public List<EmbossingEffect> GetEmbossingsForCard(Card card, Character character)
    {
        List<EmbossingEffect> available = new List<EmbossingEffect>();
        
        foreach (EmbossingEffect embossing in allEmbossings)
        {
            if (embossing == null) continue;
            
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
    
    /// <summary>
    /// Set the embossings list (used by editor script)
    /// </summary>
    public void SetEmbossings(List<EmbossingEffect> embossings)
    {
        allEmbossings = new List<EmbossingEffect>(embossings);
    }
    
    /// <summary>
    /// Get count of embossings
    /// </summary>
    public int Count => allEmbossings.Count;
}

