using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stores character-specific deck and card collection data.
/// Links decks to specific characters and tracks card unlocks.
/// Saved with character data for persistence.
/// </summary>
[System.Serializable]
public class CharacterDeckData
{
    [Header("Active Deck")]
    public string activeDeckName = "Default Deck"; // Name of currently active deck
    
    [Header("Saved Decks")]
    public List<string> savedDeckNames = new List<string>(); // Names of saved deck presets
    
    [Header("Card Collection")]
    public List<string> unlockedCards = new List<string>(); // Card names the character owns
    public bool hasAllCards = false; // Debug: unlock all cards
    
    [Header("Deck History")]
    public string lastUsedDeck = ""; // For quick-switching
    public int deckSlotsUnlocked = 5; // How many deck presets can be saved
    
    /// <summary>
    /// Check if character owns a specific card.
    /// </summary>
    public bool OwnsCard(string cardName)
    {
        if (hasAllCards) return true;
        return unlockedCards.Contains(cardName);
    }
    
    /// <summary>
    /// Check if character owns a card by CardData reference.
    /// </summary>
    public bool OwnsCard(CardData card)
    {
        if (card == null) return false;
        return OwnsCard(card.cardName);
    }
    
    /// <summary>
    /// Unlock a card for this character.
    /// </summary>
    public void UnlockCard(string cardName)
    {
        if (!unlockedCards.Contains(cardName))
        {
            unlockedCards.Add(cardName);
            Debug.Log($"Unlocked card: {cardName}");
        }
    }
    
    /// <summary>
    /// Unlock a card by CardData reference.
    /// </summary>
    public void UnlockCard(CardData card)
    {
        if (card != null)
        {
            UnlockCard(card.cardName);
        }
    }
    
    /// <summary>
    /// Unlock multiple cards at once.
    /// </summary>
    public void UnlockCards(List<string> cardNames)
    {
        foreach (string cardName in cardNames)
        {
            UnlockCard(cardName);
        }
    }
    
    /// <summary>
    /// Check if character has a saved deck with this name.
    /// </summary>
    public bool HasDeck(string deckName)
    {
        return savedDeckNames.Contains(deckName);
    }
    
    /// <summary>
    /// Add a deck to this character's saved decks.
    /// </summary>
    public bool AddDeck(string deckName)
    {
        if (savedDeckNames.Count >= deckSlotsUnlocked)
        {
            Debug.LogWarning($"Cannot save deck: Maximum {deckSlotsUnlocked} deck slots reached");
            return false;
        }
        
        if (!savedDeckNames.Contains(deckName))
        {
            savedDeckNames.Add(deckName);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Remove a deck from this character's saved decks.
    /// </summary>
    public void RemoveDeck(string deckName)
    {
        savedDeckNames.Remove(deckName);
        
        if (activeDeckName == deckName)
        {
            activeDeckName = savedDeckNames.Count > 0 ? savedDeckNames[0] : "";
        }
    }
    
    /// <summary>
    /// Set the active deck for combat.
    /// </summary>
    public void SetActiveDeck(string deckName)
    {
        if (savedDeckNames.Contains(deckName))
        {
            lastUsedDeck = activeDeckName; // Store previous for quick-switch
            activeDeckName = deckName;
        }
    }
    
    /// <summary>
    /// Get all cards this character can use (unlocked + owns).
    /// </summary>
    public List<CardData> GetAvailableCards(CardDatabase database)
    {
        if (hasAllCards)
        {
            return database.allCards;
        }
        
        return database.allCards
            .Where(card => OwnsCard(card))
            .ToList();
    }
    
    /// <summary>
    /// Initialize with starter cards.
    /// </summary>
    public void InitializeStarterCollection(List<string> starterCardNames)
    {
        unlockedCards.Clear();
        unlockedCards.AddRange(starterCardNames);
        Debug.Log($"Initialized with {starterCardNames.Count} starter cards");
    }
    
    /// <summary>
    /// Unlock all cards (debug/cheat).
    /// </summary>
    public void UnlockAllCards()
    {
        hasAllCards = true;
        Debug.Log("Unlocked all cards for this character");
    }
}








