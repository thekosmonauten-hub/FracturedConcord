using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject representing a saved deck preset.
/// Stores deck configuration with card references and quantities.
/// Can be saved to disk as JSON for import/export functionality.
/// </summary>
[CreateAssetMenu(fileName = "New Deck Preset", menuName = "Dexiled/Decks/Deck Preset")]
public class DeckPreset : ScriptableObject
{
    [Header("Deck Information")]
    public string deckName = "New Deck";
    [TextArea(2, 4)]
    public string description = "Custom deck description";
    public string characterClass = ""; // Optional class restriction
    public Sprite deckIcon; // Optional deck icon
    
    [Header("Deck Configuration")]
    [SerializeField] private List<DeckCardEntry> cardEntries = new List<DeckCardEntry>();
    
    [Header("Metadata")]
    public string createdDate;
    public string lastModifiedDate;
    public int version = 1; // For future compatibility
    
    /// <summary>
    /// Add a card to the deck preset.
    /// Returns true if successful, false if deck limit reached.
    /// </summary>
    public bool AddCard(CardData card, int quantity = 1)
    {
        if (card == null) return false;
        
        // Find existing entry
        DeckCardEntry existingEntry = cardEntries.Find(e => e.cardData == card);
        
        if (existingEntry != null)
        {
            // Enforce copy limits
            int maxCopies = GetMaxCopiesAllowed(card);
            int newQuantity = existingEntry.quantity + quantity;
            
            if (newQuantity > maxCopies)
            {
                Debug.LogWarning($"Cannot add more copies of {card.cardName}. Max: {maxCopies}");
                return false;
            }
            
            existingEntry.quantity = newQuantity;
        }
        else
        {
            // Check deck size limit before adding new card
            if (GetTotalCardCount() + quantity > DeckBuilderConstants.MAX_DECK_SIZE)
            {
                Debug.LogWarning($"Deck size limit reached ({DeckBuilderConstants.MAX_DECK_SIZE} cards)");
                return false;
            }
            
            cardEntries.Add(new DeckCardEntry(card, quantity));
        }
        
        lastModifiedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return true;
    }
    
    /// <summary>
    /// Remove a card from the deck preset.
    /// Removes one copy by default.
    /// </summary>
    public bool RemoveCard(CardData card, int quantity = 1)
    {
        if (card == null) return false;
        
        DeckCardEntry entry = cardEntries.Find(e => e.cardData == card);
        
        if (entry != null)
        {
            entry.quantity -= quantity;
            
            if (entry.quantity <= 0)
            {
                cardEntries.Remove(entry);
            }
            
            lastModifiedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get the quantity of a specific card in this deck.
    /// </summary>
    public int GetCardQuantity(CardData card)
    {
        DeckCardEntry entry = cardEntries.Find(e => e.cardData == card);
        return entry?.quantity ?? 0;
    }
    
    /// <summary>
    /// Get total number of cards in deck (accounting for quantities).
    /// </summary>
    public int GetTotalCardCount()
    {
        return cardEntries.Sum(e => e.quantity);
    }
    
    /// <summary>
    /// Get all unique cards in this deck.
    /// </summary>
    public List<CardData> GetUniqueCards()
    {
        return cardEntries.Select(e => e.cardData).ToList();
    }
    
    /// <summary>
    /// Get all card entries (card + quantity).
    /// </summary>
    public List<DeckCardEntry> GetCardEntries()
    {
        return new List<DeckCardEntry>(cardEntries);
    }
    
    /// <summary>
    /// Clear all cards from the deck.
    /// </summary>
    public void ClearDeck()
    {
        cardEntries.Clear();
        lastModifiedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// Check if deck is valid for play.
    /// </summary>
    public bool IsValidDeck(out string errorMessage)
    {
        errorMessage = "";
        
        int totalCards = GetTotalCardCount();
        
        if (totalCards < DeckBuilderConstants.MIN_DECK_SIZE)
        {
            errorMessage = $"Deck must have at least {DeckBuilderConstants.MIN_DECK_SIZE} cards. Current: {totalCards}";
            return false;
        }
        
        if (totalCards > DeckBuilderConstants.MAX_DECK_SIZE)
        {
            errorMessage = $"Deck cannot exceed {DeckBuilderConstants.MAX_DECK_SIZE} cards. Current: {totalCards}";
            return false;
        }
        
        // Validate each card's quantity
        foreach (DeckCardEntry entry in cardEntries)
        {
            int maxCopies = GetMaxCopiesAllowed(entry.cardData);
            if (entry.quantity > maxCopies)
            {
                errorMessage = $"{entry.cardData.cardName} exceeds max copies ({entry.quantity}/{maxCopies})";
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get maximum allowed copies based on card rarity.
    /// </summary>
    private int GetMaxCopiesAllowed(CardData card)
    {
        if (card.rarity == CardRarity.Unique)
            return DeckBuilderConstants.MAX_UNIQUE_COPIES;
        
        return DeckBuilderConstants.MAX_STANDARD_COPIES;
    }
    
    /// <summary>
    /// Create a deep copy of this deck preset.
    /// </summary>
    public DeckPreset Clone()
    {
        DeckPreset clone = CreateInstance<DeckPreset>();
        clone.deckName = this.deckName + " (Copy)";
        clone.description = this.description;
        clone.characterClass = this.characterClass;
        clone.deckIcon = this.deckIcon;
        clone.createdDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        clone.lastModifiedDate = clone.createdDate;
        clone.version = this.version;
        
        foreach (DeckCardEntry entry in cardEntries)
        {
            clone.cardEntries.Add(new DeckCardEntry(entry.cardData, entry.quantity));
        }
        
        return clone;
    }
    
    /// <summary>
    /// Export deck as JSON string for sharing.
    /// </summary>
    public string ExportToJSON()
    {
        DeckPresetData data = new DeckPresetData(this);
        return JsonUtility.ToJson(data, true);
    }
    
    /// <summary>
    /// Import deck from JSON string.
    /// </summary>
    public bool ImportFromJSON(string json, CardDatabase cardDatabase)
    {
        try
        {
            DeckPresetData data = JsonUtility.FromJson<DeckPresetData>(json);
            
            deckName = data.deckName;
            description = data.description;
            characterClass = data.characterClass;
            version = data.version;
            cardEntries.Clear();
            
            // Reconstruct card entries from card names
            foreach (DeckCardEntryData entryData in data.cardEntries)
            {
                CardData card = cardDatabase.allCards.Find(c => c.cardName == entryData.cardName);
                
                if (card != null)
                {
                    cardEntries.Add(new DeckCardEntry(card, entryData.quantity));
                }
                else
                {
                    Debug.LogWarning($"Card not found in database: {entryData.cardName}");
                }
            }
            
            lastModifiedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to import deck from JSON: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get deck statistics for display.
    /// </summary>
    public DeckStatistics GetStatistics()
    {
        return new DeckStatistics
        {
            totalCards = GetTotalCardCount(),
            uniqueCards = cardEntries.Count,
            attackCards = cardEntries.Where(e => e.cardData.category == CardCategory.Attack)
                                     .Sum(e => e.quantity),
            skillCards = cardEntries.Where(e => e.cardData.category == CardCategory.Skill)
                                    .Sum(e => e.quantity),
            guardCards = cardEntries.Where(e => e.cardData.category == CardCategory.Guard)
                                    .Sum(e => e.quantity),
            powerCards = cardEntries.Where(e => e.cardData.category == CardCategory.Power)
                                    .Sum(e => e.quantity),
            auraCards = 0, // CardData uses CardCategory enum which doesn't have Aura (legacy Card class only)
            averageManaCost = cardEntries.Count > 0 
                ? (float)cardEntries.Sum(e => e.cardData.playCost * e.quantity) / GetTotalCardCount()
                : 0f,
            totalManaCost = cardEntries.Sum(e => e.cardData.playCost * e.quantity)
        };
    }
}

/// <summary>
/// Individual card entry in a deck (card reference + quantity).
/// </summary>
[System.Serializable]
public class DeckCardEntry
{
    public CardData cardData;
    public int quantity;
    
    public DeckCardEntry(CardData card, int qty)
    {
        cardData = card;
        quantity = qty;
    }
}

/// <summary>
/// Serializable deck data for JSON export/import.
/// Uses card names instead of references for portability.
/// </summary>
[System.Serializable]
public class DeckPresetData
{
    public string deckName;
    public string description;
    public string characterClass;
    public int version;
    public List<DeckCardEntryData> cardEntries = new List<DeckCardEntryData>();
    
    public DeckPresetData(DeckPreset preset)
    {
        deckName = preset.deckName;
        description = preset.description;
        characterClass = preset.characterClass;
        version = preset.version;
        
        foreach (DeckCardEntry entry in preset.GetCardEntries())
        {
            cardEntries.Add(new DeckCardEntryData
            {
                cardName = entry.cardData.cardName,
                quantity = entry.quantity
            });
        }
    }
}

[System.Serializable]
public class DeckCardEntryData
{
    public string cardName;
    public int quantity;
}

/// <summary>
/// Constants for deck building rules.
/// Centralized for easy balancing adjustments.
/// </summary>
public static class DeckBuilderConstants
{
    public const int MIN_DECK_SIZE = 18; // allow starter decks of 18
    public const int MAX_DECK_SIZE = 40;
    public const int MAX_STANDARD_COPIES = 6;
    public const int MAX_UNIQUE_COPIES = 1;
    public const int STARTING_HAND_SIZE = 5;
}
