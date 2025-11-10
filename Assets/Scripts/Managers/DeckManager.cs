using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Singleton manager for deck persistence across scenes.
/// Handles active deck selection, deck preset save/load, and deck validation.
/// Persists using DontDestroyOnLoad pattern.
/// </summary>
public class DeckManager : MonoBehaviour
{
    #region Singleton
    private static DeckManager _instance;
    public static DeckManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DeckManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DeckManager");
                    _instance = go.AddComponent<DeckManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion
    
    [Header("Deck Configuration")]
    [SerializeField] private DeckPreset activeDeck;
    [SerializeField] private List<DeckPreset> savedDecks = new List<DeckPreset>();
    
    [Header("Card Database")]
    [SerializeField] private CardDatabase cardDatabase;
    
    [Header("Save Settings")]
    [SerializeField] private string saveFolder = "DeckPresets";
    [SerializeField] private bool autoSaveEnabled = true;
    
    // Events
    public System.Action<DeckPreset> OnDeckLoaded;
    public System.Action<DeckPreset> OnDeckSaved;
    public System.Action<DeckPreset> OnDeckChanged;
    public System.Action<DeckPreset> OnDeckDeleted;
    
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFolder);
    
    #region Unity Lifecycle
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnApplicationQuit()
    {
        if (autoSaveEnabled && activeDeck != null)
        {
            SaveDeck(activeDeck);
        }
    }
    #endregion
    
    #region Initialization
    private void Initialize()
    {
        // Load card database if not assigned
        if (cardDatabase == null)
        {
            cardDatabase = CardDatabase.Instance;
        }
        
        // Create save directory if it doesn't exist
        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
            Debug.Log($"Created deck preset folder: {SavePath}");
        }
        
        // Load all saved decks
        LoadAllDecks();
        
        Debug.Log($"DeckManager initialized. Loaded {savedDecks.Count} deck presets.");
    }
    #endregion
    
    #region Active Deck Management
    /// <summary>
    /// Set the active deck for combat.
    /// </summary>
    public void SetActiveDeck(DeckPreset deck)
    {
        if (deck == null)
        {
            Debug.LogWarning("Cannot set null deck as active.");
            return;
        }
        
        string errorMessage;
        if (!deck.IsValidDeck(out errorMessage))
        {
            Debug.LogError($"Cannot activate invalid deck: {errorMessage}");
            return;
        }
        
        activeDeck = deck;
        OnDeckLoaded?.Invoke(activeDeck);
        Debug.Log($"Active deck set to: {deck.deckName}");
    }
    
    /// <summary>
    /// Get the current active deck.
    /// </summary>
    public DeckPreset GetActiveDeck()
    {
        return activeDeck;
    }
    
    /// <summary>
    /// Check if there's an active deck.
    /// </summary>
    public bool HasActiveDeck()
    {
        return activeDeck != null && activeDeck.GetTotalCardCount() > 0;
    }
    
    public void NotifyDeckUpdatedExternally()
    {
        if (activeDeck != null)
        {
            OnDeckChanged?.Invoke(activeDeck);
        }
    }
    
    /// <summary>
    /// Get all cards from active deck as Card objects for combat.
    /// Converts CardData to legacy Card format.
    /// </summary>
    public List<Card> GetActiveDeckAsCards()
    {
        if (!HasActiveDeck())
        {
            Debug.LogWarning("No active deck available.");
            return new List<Card>();
        }
        
        List<Card> cards = new List<Card>();
        
        foreach (DeckCardEntry entry in activeDeck.GetCardEntries())
        {
            for (int i = 0; i < entry.quantity; i++)
            {
                Card card = ConvertCardDataToCard(entry.cardData);
                if (card != null)
                {
                    ApplyDeckEntryMetadata(card, entry);
                    cards.Add(card);
                }
            }
        }
        
        return cards;
    }
    #endregion
    
    #region Deck Preset Management
    /// <summary>
    /// Create a new empty deck preset.
    /// </summary>
    public DeckPreset CreateNewDeck(string deckName, string characterClass = "")
    {
        DeckPreset newDeck = ScriptableObject.CreateInstance<DeckPreset>();
        newDeck.deckName = deckName;
        newDeck.characterClass = characterClass;
        newDeck.createdDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        newDeck.lastModifiedDate = newDeck.createdDate;
        
        savedDecks.Add(newDeck);
        OnDeckChanged?.Invoke(newDeck);
        
        Debug.Log($"Created new deck: {deckName}");
        return newDeck;
    }
    
    /// <summary>
    /// Save a deck preset to disk.
    /// </summary>
    public bool SaveDeck(DeckPreset deck, string customFileName = "")
    {
        if (deck == null)
        {
            Debug.LogError("Cannot save null deck.");
            return false;
        }
        
        try
        {
            string fileName = string.IsNullOrEmpty(customFileName) 
                ? SanitizeFileName(deck.deckName) + ".json"
                : customFileName;
            
            string filePath = Path.Combine(SavePath, fileName);
            string json = deck.ExportToJSON();
            
            File.WriteAllText(filePath, json);
            
            // Add to saved decks if not already present
            if (!savedDecks.Contains(deck))
            {
                savedDecks.Add(deck);
            }
            
            OnDeckSaved?.Invoke(deck);
            Debug.Log($"Deck saved: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save deck '{deck.deckName}': {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Load a deck preset from disk.
    /// </summary>
    public DeckPreset LoadDeck(string fileName)
    {
        try
        {
            string filePath = Path.Combine(SavePath, fileName);
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Deck file not found: {filePath}");
                return null;
            }
            
            string json = File.ReadAllText(filePath);
            DeckPreset deck = ScriptableObject.CreateInstance<DeckPreset>();
            
            if (deck.ImportFromJSON(json, cardDatabase))
            {
                if (!savedDecks.Contains(deck))
                {
                    savedDecks.Add(deck);
                }
                
                OnDeckLoaded?.Invoke(deck);
                Debug.Log($"Deck loaded: {deck.deckName}");
                return deck;
            }
            
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load deck '{fileName}': {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Load all deck presets from disk.
    /// </summary>
    public void LoadAllDecks()
    {
        savedDecks.Clear();
        
        if (!Directory.Exists(SavePath))
        {
            return;
        }
        
        string[] files = Directory.GetFiles(SavePath, "*.json");
        
        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            DeckPreset deck = LoadDeck(fileName);
            
            if (deck != null && !savedDecks.Contains(deck))
            {
                savedDecks.Add(deck);
            }
        }
        
        Debug.Log($"Loaded {savedDecks.Count} deck presets from disk.");
    }
    
    /// <summary>
    /// Delete a deck preset from disk and memory.
    /// </summary>
    public bool DeleteDeck(DeckPreset deck)
    {
        if (deck == null)
        {
            Debug.LogError("Cannot delete null deck.");
            return false;
        }
        
        try
        {
            string fileName = SanitizeFileName(deck.deckName) + ".json";
            string filePath = Path.Combine(SavePath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            savedDecks.Remove(deck);
            
            // Clear active deck if it was deleted
            if (activeDeck == deck)
            {
                activeDeck = null;
            }
            
            OnDeckDeleted?.Invoke(deck);
            Debug.Log($"Deck deleted: {deck.deckName}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete deck '{deck.deckName}': {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get all saved deck presets.
    /// </summary>
    public List<DeckPreset> GetAllDecks()
    {
        return new List<DeckPreset>(savedDecks);
    }
    
    /// <summary>
    /// Clone/duplicate an existing deck.
    /// </summary>
    public DeckPreset DuplicateDeck(DeckPreset sourceDeck)
    {
        if (sourceDeck == null)
        {
            Debug.LogError("Cannot duplicate null deck.");
            return null;
        }
        
        DeckPreset clone = sourceDeck.Clone();
        savedDecks.Add(clone);
        OnDeckChanged?.Invoke(clone);
        
        Debug.Log($"Duplicated deck: {clone.deckName}");
        return clone;
    }
    #endregion
    
    #region Deck Validation
    /// <summary>
    /// Validate deck and return detailed error messages.
    /// </summary>
    public bool ValidateDeck(DeckPreset deck, out List<string> errors)
    {
        errors = new List<string>();
        
        if (deck == null)
        {
            errors.Add("Deck is null");
            return false;
        }
        
        int totalCards = deck.GetTotalCardCount();
        
        if (totalCards < DeckBuilderConstants.MIN_DECK_SIZE)
        {
            errors.Add($"Deck must have at least {DeckBuilderConstants.MIN_DECK_SIZE} cards (current: {totalCards})");
        }
        
        if (totalCards > DeckBuilderConstants.MAX_DECK_SIZE)
        {
            errors.Add($"Deck cannot exceed {DeckBuilderConstants.MAX_DECK_SIZE} cards (current: {totalCards})");
        }
        
        // Validate card quantities
        foreach (DeckCardEntry entry in deck.GetCardEntries())
        {
            if (entry.cardData == null)
            {
                errors.Add("Deck contains invalid card reference");
                continue;
            }
            
            int maxCopies = entry.cardData.rarity == CardRarity.Unique 
                ? DeckBuilderConstants.MAX_UNIQUE_COPIES 
                : DeckBuilderConstants.MAX_STANDARD_COPIES;
            
            if (entry.quantity > maxCopies)
            {
                errors.Add($"{entry.cardData.cardName}: Too many copies ({entry.quantity}/{maxCopies})");
            }
        }
        
        return errors.Count == 0;
    }
    #endregion
    
    #region Card Conversion (CardData -> Card)
    /// <summary>
    /// Convert CardData ScriptableObject to legacy Card class for combat.
    /// This bridges the two card systems in the project.
    /// </summary>
    private void ApplyDeckEntryMetadata(Card card, DeckCardEntry entry)
    {
        if (card == null || entry == null || entry.cardData == null)
            return;

        card.embossingSlots = entry.cardData.embossingSlots;
        card.appliedEmbossings = DeckCardEntry.CopyEmbossings(entry.embossings);
        card.groupKey = ResolveGroupKey(entry.cardData);

        if (entry.cardData is CardDataExtended extended)
        {
            card.sourceCardData = extended;
        }
    }

    private static string ResolveGroupKey(CardData cardData)
    {
        if (cardData is CardDataExtended extended && !string.IsNullOrEmpty(extended.groupKey))
            return extended.groupKey;

        return cardData != null ? cardData.cardName : string.Empty;
    }

    private Card ConvertCardDataToCard(CardData cardData)
    {
        if (cardData == null) return null;
        
        Card card = new Card
        {
            cardName = cardData.cardName,
            description = string.IsNullOrWhiteSpace(cardData.description) ? cardData.GetFullDescription() : cardData.description,
            manaCost = cardData.playCost,
            baseDamage = cardData.damage,
            baseGuard = cardData.block,
            
            // ✅ FIX: Copy visual assets from CardData
            cardArt = cardData.cardImage,
            cardArtName = cardData.cardImage != null ? cardData.cardImage.name : ""
        };
        
        // Map category to CardType
        switch (cardData.category)
        {
            case CardCategory.Attack:
                card.cardType = CardType.Attack;
                break;
            case CardCategory.Guard:
                card.cardType = CardType.Guard;
                break;
            case CardCategory.Skill:
                card.cardType = CardType.Skill;
                break;
            case CardCategory.Power:
                card.cardType = CardType.Power;
                break;
        }
        
        // Add tags based on element and properties
        card.tags = new List<string>();
        if (cardData.element != CardElement.Basic)
        {
            card.tags.Add(cardData.element.ToString());
        }
        
        if (cardData.isDualWield)
        {
            card.tags.Add("Dual");
        }
        
        if (cardData.isDiscardCard)
        {
            card.tags.Add("Discard");
        }
        
        return card;
    }
    #endregion
    
    #region Utility
    /// <summary>
    /// Sanitize file name by removing invalid characters.
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitized = new string(fileName
            .Where(c => !invalidChars.Contains(c))
            .ToArray());
        
        return string.IsNullOrEmpty(sanitized) ? "deck" : sanitized;
    }
    #endregion
}








