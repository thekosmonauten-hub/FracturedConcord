using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main UI controller for the Deck Builder scene.
/// Manages card collection display, deck list, filtering, and user interactions.
/// Follows Hearthstone-style deck building interface.
/// </summary>
public class DeckBuilderUI : MonoBehaviour
{
    [Header("Card Collection Panel")]
    [SerializeField] private ScrollRect cardCollectionScrollRect;
    [SerializeField] private GridLayoutGroup cardCollectionGrid;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardCollectionContainer;
    
    [Header("Deck List Panel")]
    [SerializeField] private ScrollRect deckListScrollRect;
    [SerializeField] private Transform deckListContainer;
    [SerializeField] private GameObject deckCardPrefab;
    
    [Header("Deck Info Display")]
    [SerializeField] private TMP_InputField deckNameInputField; // Changed from TextMeshProUGUI to InputField
    [SerializeField] private TextMeshProUGUI deckSizeText;
    [SerializeField] private TMP_InputField deckDescriptionInputField; // Also make description editable
    [SerializeField] private TextMeshProUGUI deckAdditionalEffectsText;
    [SerializeField] private Image deckCategoryIcon;
    [SerializeField] private Sprite defaultCategoryIcon;
    [SerializeField] private Image deckValidityIndicator;
    [SerializeField] private Color validDeckColor = Color.green;
    [SerializeField] private Color invalidDeckColor = Color.red;
    
    [Header("Filter Controls")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private TMP_Dropdown costFilterDropdown;
    [SerializeField] private TMP_Dropdown categoryFilterDropdown;
    [SerializeField] private TMP_Dropdown rarityFilterDropdown;
    [SerializeField] private TMP_Dropdown elementFilterDropdown;
    [SerializeField] private Button clearFiltersButton;
    
    [Header("Deck Preset Controls")]
    [SerializeField] private TMP_Dropdown deckPresetsDropdown;
    [SerializeField] private Button newDeckButton;
    [SerializeField] private Button saveDeckButton;
    [SerializeField] private Button deleteDeckButton;
    [SerializeField] private Button duplicateDeckButton;
    [SerializeField] private Button exportDeckButton;
    [SerializeField] private Button importDeckButton;
    
    [Header("Navigation")]
    [SerializeField] private Button doneButton;
    [SerializeField] private Button backButton;
    
    [Header("Card Database")]
    [SerializeField] private CardDatabase cardDatabase;
    
    [Header("Animation Settings")]
    [SerializeField] private float cardHoverScale = 1.15f;
    [SerializeField] private float cardAnimationDuration = 0.2f;
    
    // Current state
    private DeckPreset currentDeck;
    private List<CardData> filteredCards = new List<CardData>();
    private Dictionary<CardData, DeckBuilderCardUI> cardUIMap = new Dictionary<CardData, DeckBuilderCardUI>();
    private Dictionary<CardData, DeckCardListUI> deckCardUIMap = new Dictionary<CardData, DeckCardListUI>();

    public DeckPreset CurrentDeck => currentDeck;

    public DeckCardEntry GetDeckEntry(CardData card)
    {
        if (currentDeck == null || card == null) return null;
        return currentDeck.GetEntryReference(card);
    }
    
    // Filters
    private string searchFilter = "";
    private int costFilter = -1; // -1 = All
    private CardCategory? categoryFilter = null;
    private CardRarity? rarityFilter = null;
    private CardElement? elementFilter = null;

    private const string AdditionalEffectsFallbackText = "No additional effects recorded.";
    
    #region Unity Lifecycle
    private void Start()
    {
        Initialize();
    }
    
    private void OnEnable()
    {
        // Subscribe to DeckManager events
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnDeckChanged += OnDeckChanged;
            DeckManager.Instance.OnDeckLoaded += OnDeckLoaded;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        if (DeckManager.Instance != null)
        {
            DeckManager.Instance.OnDeckChanged -= OnDeckChanged;
            DeckManager.Instance.OnDeckLoaded -= OnDeckLoaded;
        }
    }
    #endregion
    
    #region Initialization
    private void Initialize()
    {
        ResolveMetadataReferences();
        // Load card database if not assigned
        if (cardDatabase == null)
        {
            cardDatabase = CardDatabase.Instance;
        }
        
        // Configure grid layout padding for hover space
        if (cardCollectionGrid != null)
        {
            cardCollectionGrid.padding = new RectOffset(20, 20, 50, 50); // left, right, top, bottom
        }
        
        // Initialize UI controls
        SetupDeckInfoControls();
        SetupFilterControls();
        SetupPresetControls();
        SetupNavigationControls();
        
        // Load or create initial deck
        LoadInitialDeck();
        
        // Populate card collection
        RefreshCardCollection();
        
        Debug.Log("DeckBuilderUI initialized.");
    }
    
    private void SetupDeckInfoControls()
    {
        // Deck name input field
        if (deckNameInputField != null)
        {
            deckNameInputField.onEndEdit.AddListener(OnDeckNameChanged);
            deckNameInputField.characterLimit = 50; // Max 50 characters
        }
        
        // Deck description input field
        if (deckDescriptionInputField != null)
        {
            deckDescriptionInputField.onEndEdit.AddListener(OnDeckDescriptionChanged);
            deckDescriptionInputField.characterLimit = 200; // Max 200 characters
        }
        
        if (deckAdditionalEffectsText != null)
        {
            deckAdditionalEffectsText.text = AdditionalEffectsFallbackText;
        }
        
        if (deckCategoryIcon != null)
        {
            deckCategoryIcon.sprite = defaultCategoryIcon;
            deckCategoryIcon.gameObject.SetActive(defaultCategoryIcon != null);
        }
    }
    
    private void SetupFilterControls()
    {
        // Search field
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(OnSearchChanged);
        }
        
        // Cost filter dropdown
        if (costFilterDropdown != null)
        {
            costFilterDropdown.ClearOptions();
            List<string> costOptions = new List<string> { "All Costs", "0", "1", "2", "3", "4", "5", "6+" };
            costFilterDropdown.AddOptions(costOptions);
            costFilterDropdown.onValueChanged.AddListener(OnCostFilterChanged);
        }
        
        // Category filter dropdown
        if (categoryFilterDropdown != null)
        {
            categoryFilterDropdown.ClearOptions();
            List<string> categoryOptions = new List<string> { "All Types" };
            categoryOptions.AddRange(System.Enum.GetNames(typeof(CardCategory)));
            categoryFilterDropdown.AddOptions(categoryOptions);
            categoryFilterDropdown.onValueChanged.AddListener(OnCategoryFilterChanged);
        }
        
        // Rarity filter dropdown
        if (rarityFilterDropdown != null)
        {
            rarityFilterDropdown.ClearOptions();
            List<string> rarityOptions = new List<string> { "All Rarities" };
            rarityOptions.AddRange(System.Enum.GetNames(typeof(CardRarity)));
            rarityFilterDropdown.AddOptions(rarityOptions);
            rarityFilterDropdown.onValueChanged.AddListener(OnRarityFilterChanged);
        }
        
        // Element filter dropdown
        if (elementFilterDropdown != null)
        {
            elementFilterDropdown.ClearOptions();
            List<string> elementOptions = new List<string> { "All Elements" };
            elementOptions.AddRange(System.Enum.GetNames(typeof(CardElement)));
            elementFilterDropdown.AddOptions(elementOptions);
            elementFilterDropdown.onValueChanged.AddListener(OnElementFilterChanged);
        }
        
        // Clear filters button
        if (clearFiltersButton != null)
        {
            clearFiltersButton.onClick.AddListener(ClearAllFilters);
        }
    }
    
    private void SetupPresetControls()
    {
        // Preset dropdown
        if (deckPresetsDropdown != null)
        {
            deckPresetsDropdown.onValueChanged.AddListener(OnPresetSelected);
            RefreshPresetDropdown();
        }
        
        // Deck management buttons
        if (newDeckButton != null)
            newDeckButton.onClick.AddListener(OnNewDeck);
        
        if (saveDeckButton != null)
            saveDeckButton.onClick.AddListener(OnSaveDeck);
        
        if (deleteDeckButton != null)
            deleteDeckButton.onClick.AddListener(OnDeleteDeck);
        
        if (duplicateDeckButton != null)
            duplicateDeckButton.onClick.AddListener(OnDuplicateDeck);
        
        if (exportDeckButton != null)
            exportDeckButton.onClick.AddListener(OnExportDeck);
        
        if (importDeckButton != null)
            importDeckButton.onClick.AddListener(OnImportDeck);
    }
    
    private void SetupNavigationControls()
    {
        if (doneButton != null)
        {
            doneButton.onClick.AddListener(OnDoneClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    
    private void LoadInitialDeck()
    {
        // Try to load active deck from DeckManager
        if (DeckManager.Instance.HasActiveDeck())
        {
            currentDeck = DeckManager.Instance.GetActiveDeck();
        }
        else
        {
            // Try to load from character's active deck name
            Character character = CharacterManager.Instance?.GetCurrentCharacter();
            if (character != null && character.deckData != null && !string.IsNullOrEmpty(character.deckData.activeDeckName))
            {
                // Load character's decks
                CharacterManager.Instance.LoadCharacterDecks(character);
                
                // Try again after loading
                if (DeckManager.Instance.HasActiveDeck())
                {
                    currentDeck = DeckManager.Instance.GetActiveDeck();
                }
                else
                {
                    // If still no deck, create a new one
                    currentDeck = DeckManager.Instance.CreateNewDeck("New Deck", character.characterClass);
                }
            }
            else
            {
                // Create a new deck if no character is loaded
                currentDeck = DeckManager.Instance.CreateNewDeck("New Deck");
            }
        }
        
        RefreshDeckDisplay();
    }
    #endregion
    
    #region Card Collection Display
    /// <summary>
    /// Refresh the card collection display with current filters.
    /// </summary>
    public void RefreshCardCollection()
    {
        if (cardDatabase == null || cardCollectionContainer == null)
        {
            Debug.LogWarning("CardDatabase or CardCollectionContainer not assigned.");
            return;
        }
        
        // Apply filters
        ApplyFilters();
        
        // Clear existing card UI
        foreach (Transform child in cardCollectionContainer)
        {
            Destroy(child.gameObject);
        }
        cardUIMap.Clear();
        
        // Create card UI for each filtered card
        foreach (CardData card in filteredCards)
        {
            CreateCardUI(card);
        }
        
        Debug.Log($"Displaying {filteredCards.Count} cards in collection.");
    }
    
    private void CreateCardUI(CardData card)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab not assigned!");
            return;
        }
        
        GameObject cardObj = Instantiate(cardPrefab, cardCollectionContainer);
        DeckBuilderCardUI cardUI = cardObj.GetComponent<DeckBuilderCardUI>();
        
        if (cardUI != null)
        {
            cardUI.Initialize(card, this);
            cardUIMap[card] = cardUI;
            
            // Update card state based on deck
            UpdateCardUIState(card);
        }
        else
        {
            Debug.LogError("CardPrefab must have DeckBuilderCardUI component!");
        }
    }
    
    private void UpdateCardUIState(CardData card)
    {
        if (!cardUIMap.ContainsKey(card)) return;
        
        DeckBuilderCardUI cardUI = cardUIMap[card];
        int currentQuantity = currentDeck.GetCardQuantity(card);
        int ownedCopies = GetOwnedCopies(card);
        int capPerRarity = card.rarity == CardRarity.Unique 
            ? DeckBuilderConstants.MAX_UNIQUE_COPIES 
            : DeckBuilderConstants.MAX_STANDARD_COPIES;
        int maxCopies = Mathf.Min(ownedCopies, capPerRarity);
        
        cardUI.UpdateQuantity(currentQuantity, maxCopies);
        cardUI.SetInteractable(currentQuantity < maxCopies);
    }
    #endregion
    
    #region Deck List Display
    /// <summary>
    /// Refresh the deck list display.
    /// </summary>
    public void RefreshDeckList()
    {
        if (deckListContainer == null || currentDeck == null)
        {
            return;
        }
        
        // Clear existing deck card UI
        foreach (Transform child in deckListContainer)
        {
            Destroy(child.gameObject);
        }
        deckCardUIMap.Clear();
        
        // Sort cards by cost, then by name
        List<DeckCardEntry> sortedEntries = currentDeck.GetCardEntries()
            .OrderBy(e => e.cardData.playCost)
            .ThenBy(e => e.cardData.cardName)
            .ToList();
        
        // Create deck card UI for each entry
        foreach (DeckCardEntry entry in sortedEntries)
        {
            CreateDeckCardUI(entry);
        }
        
        UpdateDeckInfoDisplay();
    }
    
    private void CreateDeckCardUI(DeckCardEntry entry)
    {
        if (deckCardPrefab == null)
        {
            Debug.LogError("Deck card prefab not assigned!");
            return;
        }
        
        GameObject deckCardObj = Instantiate(deckCardPrefab, deckListContainer);
        DeckCardListUI deckCardUI = deckCardObj.GetComponent<DeckCardListUI>();
        
        if (deckCardUI != null)
        {
            deckCardUI.Initialize(entry, this);
            deckCardUIMap[entry.cardData] = deckCardUI;
        }
        else
        {
            Debug.LogError("DeckCardPrefab must have DeckCardListUI component!");
        }
    }
    
    private void UpdateDeckInfoDisplay()
    {
        if (currentDeck == null)
            return;

        ResolveMetadataReferences();
        
        if (deckNameInputField != null)
        {
            deckNameInputField.text = currentDeck.deckName;
        }
        
        if (deckSizeText != null)
        {
            int totalCards = currentDeck.GetTotalCardCount();
            deckSizeText.text = $"{totalCards}/{DeckBuilderConstants.MAX_DECK_SIZE}";
            bool withinBounds = totalCards >= DeckBuilderConstants.MIN_DECK_SIZE && totalCards <= DeckBuilderConstants.MAX_DECK_SIZE;
            deckSizeText.color = withinBounds ? Color.green : Color.red;
        }
        
        if (deckDescriptionInputField != null)
        {
            deckDescriptionInputField.text = currentDeck.description;
        }

        if (deckAdditionalEffectsText != null)
        {
            string summary = !string.IsNullOrWhiteSpace(currentDeck.additionalEffectsText)
                ? currentDeck.additionalEffectsText
                : BuildAdditionalEffectsSummary();
            deckAdditionalEffectsText.text = string.IsNullOrWhiteSpace(summary)
                ? AdditionalEffectsFallbackText
                : summary;
        }

        if (deckCategoryIcon != null)
        {
            Sprite iconToUse = ResolveDeckCategoryIcon(currentDeck);
            deckCategoryIcon.sprite = iconToUse;
            deckCategoryIcon.gameObject.SetActive(iconToUse != null);
        }
        
        if (deckValidityIndicator != null)
        {
            string errorMessage;
            bool isValid = currentDeck.IsValidDeck(out errorMessage);
            deckValidityIndicator.color = isValid ? validDeckColor : invalidDeckColor;
        }
    }
    
    /// <summary>
    /// Called when player finishes editing deck name.
    /// </summary>
    private void OnDeckNameChanged(string newName)
    {
        if (currentDeck == null) return;
        
        // Sanitize the name
        newName = SanitizeDeckName(newName);
        
        // Validate name
        if (string.IsNullOrWhiteSpace(newName))
        {
            ShowMessage("Deck name cannot be empty!");
            deckNameInputField.text = currentDeck.deckName; // Revert
            return;
        }
        
        // Check for duplicate names (excluding current deck)
        List<DeckPreset> allDecks = DeckManager.Instance.GetAllDecks();
        bool nameExists = allDecks.Any(d => d != currentDeck && d.deckName == newName);
        
        if (nameExists)
        {
            ShowMessage($"A deck named '{newName}' already exists!");
            deckNameInputField.text = currentDeck.deckName; // Revert
            return;
        }
        
        // Update deck name
        string oldName = currentDeck.deckName;
        currentDeck.deckName = newName;
        
        Debug.Log($"Deck renamed: '{oldName}' → '{newName}'");
        ShowMessage($"Deck renamed to '{newName}'");
    }
    
    /// <summary>
    /// Called when player finishes editing deck description.
    /// </summary>
    private void OnDeckDescriptionChanged(string newDescription)
    {
        if (currentDeck == null) return;
        
        currentDeck.description = newDescription;
        Debug.Log($"Deck description updated");
    }
    
    /// <summary>
    /// Sanitize deck name for file system compatibility.
    /// </summary>
    private string SanitizeDeckName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Unnamed Deck";
        
        // Remove invalid file name characters
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
        string sanitized = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
        
        // Trim whitespace
        sanitized = sanitized.Trim();
        
        // Limit length
        if (sanitized.Length > 50)
        {
            sanitized = sanitized.Substring(0, 50);
        }
        
        // Default if empty after sanitization
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "Unnamed Deck";
        }
        
        return sanitized;
    }
    
    private void RefreshDeckDisplay()
    {
        RefreshDeckList();
        RefreshCardCollectionStates();
    }
    
    private void RefreshCardCollectionStates()
    {
        foreach (CardData card in cardUIMap.Keys)
        {
            UpdateCardUIState(card);
        }
    }
    #endregion

    #region Metadata Helpers
    private void ResolveMetadataReferences()
    {
        if (deckDescriptionInputField == null)
        {
            deckDescriptionInputField = GetComponentsInChildren<TMP_InputField>(true)
                .FirstOrDefault(input => input != null && input.name.IndexOf("Description", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        if (deckAdditionalEffectsText == null)
        {
            deckAdditionalEffectsText = GetComponentsInChildren<TextMeshProUGUI>(true)
                .FirstOrDefault(text =>
                    text != null &&
                    text.name.IndexOf("Additional", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    text.GetComponentInParent<DeckBuilderCardUI>() == null &&
                    text.transform.GetComponentInParent<DeckCardListUI>() == null);
        }

        if (deckAdditionalEffectsText == null)
        {
            deckAdditionalEffectsText = GetComponentsInChildren<TextMeshProUGUI>(true)
                .FirstOrDefault(text =>
                    text != null &&
                    text.name.IndexOf("DeckDescription", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    text.GetComponentInParent<DeckBuilderCardUI>() == null &&
                    text.transform.GetComponentInParent<DeckCardListUI>() == null);
        }

        if (deckCategoryIcon == null)
        {
            deckCategoryIcon = GetComponentsInChildren<Image>(true)
                .FirstOrDefault(img =>
                    img != null &&
                    (img.name.IndexOf("Category", StringComparison.OrdinalIgnoreCase) >= 0 || img.name.IndexOf("DeckIcon", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    img.GetComponentInParent<DeckBuilderCardUI>() == null &&
                    img.transform.GetComponentInParent<DeckCardListUI>() == null);
        }

        if (deckCategoryIcon == null)
        {
            deckCategoryIcon = GetComponentsInChildren<Image>(true)
                .FirstOrDefault(img =>
                    img != null &&
                    img.name.IndexOf("DiamondShape", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    img.GetComponentInParent<DeckBuilderCardUI>() == null &&
                    img.transform.GetComponentInParent<DeckCardListUI>() == null);
        }
    }

    private string BuildAdditionalEffectsSummary()
    {
        if (currentDeck == null)
        {
            return AdditionalEffectsFallbackText;
        }

        HashSet<string> highlights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (DeckCardEntry entry in currentDeck.GetCardEntries())
        {
            if (entry?.cardData == null)
            {
                continue;
            }

            CardDataExtended extended = entry.cardData as CardDataExtended;
            if (extended != null && !string.IsNullOrWhiteSpace(extended.comboDescription))
            {
                string formatted = $"{extended.cardName}: {extended.comboDescription.Trim()}";
                highlights.Add(formatted);
            }
        }

        if (highlights.Count == 0)
        {
            return AdditionalEffectsFallbackText;
        }

        return string.Join("\n", highlights);
    }

    private Sprite ResolveDeckCategoryIcon(DeckPreset deck)
    {
        if (deck == null)
        {
            return defaultCategoryIcon;
        }

        Sprite resolved = deck.ResolveCategoryIcon();
        if (resolved != null)
        {
            return resolved;
        }

        if (!string.IsNullOrWhiteSpace(deck.categoryIconResourcePath))
        {
            resolved = Resources.Load<Sprite>(deck.categoryIconResourcePath);
            if (resolved != null)
            {
                return resolved;
            }
        }

        return defaultCategoryIcon;
    }
    #endregion
    
    #region Card Add/Remove
    /// <summary>
    /// Add a card to the current deck.
    /// Called by DeckBuilderCardUI when clicked.
    /// </summary>
    public void OnCardAdded(CardData card)
    {
        if (card == null || currentDeck == null) return;
        
        // Check deck size limit
        if (currentDeck.GetTotalCardCount() >= DeckBuilderConstants.MAX_DECK_SIZE)
        {
            ShowMessage($"Deck is full ({DeckBuilderConstants.MAX_DECK_SIZE} cards max)");
            return;
        }
        
        // Check card copy limit
        int currentQuantity = currentDeck.GetCardQuantity(card);
        int ownedCopies = GetOwnedCopies(card);
        int capPerRarity = card.rarity == CardRarity.Unique 
            ? DeckBuilderConstants.MAX_UNIQUE_COPIES 
            : DeckBuilderConstants.MAX_STANDARD_COPIES;
        int maxCopies = Mathf.Min(ownedCopies, capPerRarity);
        
        if (currentQuantity >= maxCopies)
        {
            ShowMessage($"Maximum {maxCopies} copies of {card.cardName} allowed (owned: {ownedCopies})");
            return;
        }
        
        // Add card
        if (currentDeck.AddCard(card, 1))
        {
            // Preserve embossings when adding a card that already exists in deck
            // (AddCard already handles this by using existing entry, but we ensure it's saved)
            SaveCurrentDeck();
            RefreshDeckDisplay();
            RefreshCardCollection(); // Refresh to show updated embossings on cards
            PlayCardAddAnimation(card);
        }
    }
    
    /// <summary>
    /// Remove a card from the current deck.
    /// Called by DeckCardListUI when clicked.
    /// </summary>
    public void OnCardRemoved(CardData card)
    {
        if (card == null || currentDeck == null) return;
        
        if (currentDeck.RemoveCard(card, 1))
        {
            // Save deck to persist embossing changes (if card was completely removed, embossings are cleared)
            SaveCurrentDeck();
            RefreshDeckDisplay();
            RefreshCardCollection(); // Refresh to show updated embossings on cards
            PlayCardRemoveAnimation(card);
        }
    }
    #endregion
    
    #region Filtering
    private void ApplyFilters()
    {
        filteredCards.Clear();
        
        if (cardDatabase == null || cardDatabase.allCards == null)
        {
            return;
        }
        
        foreach (CardData card in cardDatabase.allCards)
        {
            if (PassesFilters(card))
            {
                filteredCards.Add(card);
            }
        }

        // Restrict to owned cards only (starter + acquired duplicates)
        var cm = CharacterManager.Instance;
        if (cm != null && cm.HasCharacter() && cm.GetCurrentCharacter().deckData != null)
        {
            var dd = cm.GetCurrentCharacter().deckData;
            HashSet<string> owned = dd.hasAllCards
                ? new HashSet<string>(cardDatabase.allCards.Select(c => c.cardName))
                : new HashSet<string>(dd.unlockedCards);
            filteredCards = filteredCards.Where(c => owned.Contains(c.cardName)).ToList();
        }
        
        // Sort by cost, then by name
        filteredCards = filteredCards
            .OrderBy(c => c.playCost)
            .ThenBy(c => c.cardName)
            .ToList();
    }
    
    private bool PassesFilters(CardData card)
    {
        // Search filter
        if (!string.IsNullOrEmpty(searchFilter))
        {
            if (!card.cardName.ToLower().Contains(searchFilter.ToLower()) &&
                !card.description.ToLower().Contains(searchFilter.ToLower()))
            {
                return false;
            }
        }
        
        // Cost filter
        if (costFilter >= 0)
        {
            if (costFilter == 6) // "6+"
            {
                if (card.playCost < 6)
                    return false;
            }
            else
            {
                if (card.playCost != costFilter)
                    return false;
            }
        }
        
        // Category filter
        if (categoryFilter.HasValue && card.category != categoryFilter.Value)
        {
            return false;
        }
        
        // Rarity filter
        if (rarityFilter.HasValue && card.rarity != rarityFilter.Value)
        {
            return false;
        }
        
        // Element filter
        if (elementFilter.HasValue && card.element != elementFilter.Value)
        {
            return false;
        }
        
        return true;
    }
    
    private void OnSearchChanged(string searchText)
    {
        searchFilter = searchText;
        RefreshCardCollection();
    }
    
    private void OnCostFilterChanged(int index)
    {
        costFilter = index - 1; // -1 = All, 0-5 = specific costs, 6 = 6+
        RefreshCardCollection();
    }
    
    private void OnCategoryFilterChanged(int index)
    {
        if (index == 0)
        {
            categoryFilter = null;
        }
        else
        {
            categoryFilter = (CardCategory)(index - 1);
        }
        RefreshCardCollection();
    }
    
    private void OnRarityFilterChanged(int index)
    {
        if (index == 0)
        {
            rarityFilter = null;
        }
        else
        {
            rarityFilter = (CardRarity)(index - 1);
        }
        RefreshCardCollection();
    }
    
    private void OnElementFilterChanged(int index)
    {
        if (index == 0)
        {
            elementFilter = null;
        }
        else
        {
            elementFilter = (CardElement)(index - 1);
        }
        RefreshCardCollection();
    }
    
    private void ClearAllFilters()
    {
        searchFilter = "";
        costFilter = -1;
        categoryFilter = null;
        rarityFilter = null;
        elementFilter = null;
        
        // Reset UI controls
        if (searchInputField != null)
            searchInputField.text = "";
        if (costFilterDropdown != null)
            costFilterDropdown.value = 0;
        if (categoryFilterDropdown != null)
            categoryFilterDropdown.value = 0;
        if (rarityFilterDropdown != null)
            rarityFilterDropdown.value = 0;
        if (elementFilterDropdown != null)
            elementFilterDropdown.value = 0;
        
        RefreshCardCollection();
    }
    #endregion
    
    #region Deck Preset Management
    private void RefreshPresetDropdown()
    {
        if (deckPresetsDropdown == null) return;
        
        deckPresetsDropdown.ClearOptions();
        
        List<DeckPreset> allDecks = DeckManager.Instance.GetAllDecks();
        List<string> deckNames = allDecks.Select(d => d.deckName).ToList();
        
        if (deckNames.Count == 0)
        {
            deckNames.Add("No Saved Decks");
        }
        
        deckPresetsDropdown.AddOptions(deckNames);
        
        // Set current deck as selected
        if (currentDeck != null)
        {
            int index = allDecks.IndexOf(currentDeck);
            if (index >= 0)
            {
                deckPresetsDropdown.value = index;
            }
        }
    }
    
    private void OnPresetSelected(int index)
    {
        List<DeckPreset> allDecks = DeckManager.Instance.GetAllDecks();
        
        if (index >= 0 && index < allDecks.Count)
        {
            currentDeck = allDecks[index];
            RefreshDeckDisplay();
        }
    }
    
    private void OnNewDeck()
    {
        currentDeck = DeckManager.Instance.CreateNewDeck("New Deck", GetCurrentCharacterClass());
        RefreshPresetDropdown();
        RefreshDeckDisplay();
    }
    
    private void OnSaveDeck()
    {
        if (currentDeck == null) return;
        
        // Update deck name from input field before saving
        if (deckNameInputField != null)
        {
            string inputName = SanitizeDeckName(deckNameInputField.text);
            
            if (!string.IsNullOrWhiteSpace(inputName))
            {
                currentDeck.deckName = inputName;
            }
        }
        
        // Update description from input field
        if (deckDescriptionInputField != null)
        {
            currentDeck.description = deckDescriptionInputField.text;
        }
        
        // Validate deck
        string errorMessage;
        if (!currentDeck.IsValidDeck(out errorMessage))
        {
            ShowMessage($"Cannot save invalid deck: {errorMessage}");
            return;
        }
        
        if (DeckManager.Instance.SaveDeck(currentDeck))
        {
            ShowMessage($"Deck '{currentDeck.deckName}' saved successfully!");
            RefreshPresetDropdown();
        }
    }
    
    /// <summary>
    /// Save the current deck to persist embossings and other changes.
    /// </summary>
    private void SaveCurrentDeck()
    {
        if (currentDeck == null || DeckManager.Instance == null)
            return;
        
        // Save deck to persist embossings and other changes
        DeckManager.Instance.SaveDeck(currentDeck);
    }
    
    private void OnDeleteDeck()
    {
        if (currentDeck == null) return;
        
        string deletedDeckName = currentDeck.deckName;
        
        // Optional: Show confirmation dialog (if SimpleConfirmationDialog exists in scene)
        // Uncomment this when you add the dialog to your scene:
        // SimpleConfirmationDialog.ShowDeleteConfirmation(deletedDeckName, () => ConfirmDeleteDeck());
        // return;
        
        // For now, delete immediately (no confirmation)
        ConfirmDeleteDeck();
    }
    
    /// <summary>
    /// Actually delete the deck (called after confirmation or immediately).
    /// </summary>
    private void ConfirmDeleteDeck()
    {
        if (currentDeck == null) return;
        
        string deletedDeckName = currentDeck.deckName;
        
        // Delete the deck preset completely (removes file + from saved list)
        if (DeckManager.Instance.DeleteDeck(currentDeck))
        {
            ShowMessage($"Deck '{deletedDeckName}' permanently deleted");
            
            // Switch to a different deck or create new one
            List<DeckPreset> remainingDecks = DeckManager.Instance.GetAllDecks();
            
            if (remainingDecks.Count > 0)
            {
                // Switch to the first remaining deck
                currentDeck = remainingDecks[0];
                DeckManager.Instance.SetActiveDeck(currentDeck);
                RefreshDeckDisplay();
                Debug.Log($"Switched to deck: {currentDeck.deckName}");
            }
            else
            {
                // No decks left, create a new one
                Debug.Log("No decks remaining, creating new deck");
                currentDeck = DeckManager.Instance.CreateNewDeck("New Deck", GetCurrentCharacterClass());
                RefreshDeckDisplay();
            }
            
            // Update the preset dropdown to reflect changes
            RefreshPresetDropdown();
        }
        else
        {
            ShowMessage($"Failed to delete deck");
        }
    }
    
    
    private void OnDuplicateDeck()
    {
        if (currentDeck == null) return;
        
        DeckPreset duplicate = DeckManager.Instance.DuplicateDeck(currentDeck);
        if (duplicate != null)
        {
            currentDeck = duplicate;
            RefreshPresetDropdown();
            RefreshDeckDisplay();
            ShowMessage($"Deck duplicated");
        }
    }
    
    private void OnExportDeck()
    {
        if (currentDeck == null) return;
        
        string json = currentDeck.ExportToJSON();
        GUIUtility.systemCopyBuffer = json;
        ShowMessage("Deck code copied to clipboard!");
    }
    
    private void OnImportDeck()
    {
        string json = GUIUtility.systemCopyBuffer;
        
        if (string.IsNullOrEmpty(json))
        {
            ShowMessage("Clipboard is empty!");
            return;
        }
        
        DeckPreset importedDeck = ScriptableObject.CreateInstance<DeckPreset>();
        if (importedDeck.ImportFromJSON(json, cardDatabase))
        {
            currentDeck = importedDeck;
            RefreshDeckDisplay();
            ShowMessage("Deck imported successfully!");
        }
        else
        {
            ShowMessage("Failed to import deck. Invalid deck code.");
        }
    }
    #endregion
    
    #region Navigation
    private void OnDoneClicked()
    {
        string errorMessage;
        if (!currentDeck.IsValidDeck(out errorMessage))
        {
            ShowMessage($"Deck is invalid: {errorMessage}");
            return;
        }
        
        // Set as active deck in DeckManager (persists across scenes)
        DeckManager.Instance.SetActiveDeck(currentDeck);
        DeckManager.Instance.SaveDeck(currentDeck);
        
        // Also sync to Character.currentDeck for backward compatibility
        SyncDeckToCharacter();
        
        // Return to previous scene (you can customize this)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameUI");
    }
    
    /// <summary>
    /// Sync the current deck to the Character for backward compatibility.
    /// Converts DeckPreset to legacy Deck format.
    /// </summary>
    private void SyncDeckToCharacter()
    {
        if (currentDeck == null) return;
        
        Character character = CharacterManager.Instance?.GetCurrentCharacter();
        if (character == null) return;
        
        // Convert DeckPreset to legacy Deck format
        Deck legacyDeck = new Deck
        {
            deckName = currentDeck.deckName,
            description = currentDeck.description,
            characterClass = currentDeck.characterClass
        };
        
        // Convert CardData entries to Card objects
        foreach (DeckCardEntry entry in currentDeck.GetCardEntries())
        {
            for (int i = 0; i < entry.quantity; i++)
            {
                Card card = ConvertCardDataToCard(entry.cardData);
                if (card != null)
                {
                    ApplyDeckEntryMetadata(card, entry);
                    legacyDeck.AddCard(card);
                }
            }
        }
        
        // Assign to character
        character.currentDeck = legacyDeck;
        CharacterManager.Instance.SaveCharacter();
        
        Debug.Log($"Synced deck '{currentDeck.deckName}' to Character with {legacyDeck.cards.Count} cards");
    }
    
    /// <summary>
    /// Convert CardData to legacy Card format.
    /// </summary>
    private Card ConvertCardDataToCard(CardData cardData)
    {
        if (cardData == null) return null;
        
        Card card = new Card
        {
            cardName = cardData.cardName,
            description = string.IsNullOrWhiteSpace(cardData.description) ? cardData.GetFullDescription() : cardData.description,
            manaCost = cardData.playCost,
            baseDamage = cardData.damage,
            baseGuard = cardData.block
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
    
    private void OnBackClicked()
    {
        // Auto-save if deck is valid
        string errorMessage;
        if (currentDeck != null && currentDeck.IsValidDeck(out errorMessage))
        {
            DeckManager.Instance.SaveDeck(currentDeck);
        }
        
        // Return to previous scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameUI");
    }
    #endregion
    
    #region Event Handlers
    private void OnDeckChanged(DeckPreset deck)
    {
        if (deck == currentDeck)
        {
            RefreshDeckDisplay();
        }
    }
    
    private void OnDeckLoaded(DeckPreset deck)
    {
        currentDeck = deck;
        RefreshDeckDisplay();
    }
    #endregion
    
    #region Animations & Feedback
    private void PlayCardAddAnimation(CardData card)
    {
        // TODO: Implement smooth card add animation (slide/fade)
        // For now, just log
        Debug.Log($"Card added: {card.cardName}");
    }
    
    private void PlayCardRemoveAnimation(CardData card)
    {
        // TODO: Implement smooth card remove animation
        Debug.Log($"Card removed: {card.cardName}");
    }
    
    private void ShowMessage(string message)
    {
        // TODO: Show toast/notification message
        // For now, just log
        Debug.Log($"[Deck Builder] {message}");
    }
    
    // Get how many copies of this card the player owns (count duplicates in unlockedCards)
    private int GetOwnedCopies(CardData card)
    {
        var cm = CharacterManager.Instance;
        if (cm == null || !cm.HasCharacter()) return 0;
        var dd = cm.GetCurrentCharacter().deckData;
        if (dd == null) return 0;
        if (dd.hasAllCards)
        {
            return card.rarity == CardRarity.Unique ? 1 : DeckBuilderConstants.MAX_STANDARD_COPIES;
        }
        if (dd.unlockedCards == null) return 0;
        string name = card.cardName;
        int count = 0;
        for (int i = 0; i < dd.unlockedCards.Count; i++)
        {
            if (dd.unlockedCards[i] == name) count++;
        }
        return count;
    }
    #endregion
    
    #region Utility
    private string GetCurrentCharacterClass()
    {
        // Get character class from CharacterManager if available
        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            return CharacterManager.Instance.GetCurrentCharacter().characterClass;
        }
        
        return "";
    }
    #endregion
}
