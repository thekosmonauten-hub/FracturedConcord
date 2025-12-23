using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

/// <summary>
/// Handles the Seer's card vendor functionality - displays and sells pre-determined cards
/// based on the player's class. Cards from other classes are available for purchase.
/// </summary>
public class SeerCardVendor : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Container for vendor card slots (Grid Layout Group)")]
    public Transform cardGridContainer;
    
    [Tooltip("Prefab for displaying cards in the vendor")]
    public GameObject cardSlotPrefab;
    
    [Tooltip("Button to purchase selected card")]
    public Button purchaseButton;
    
    [Tooltip("Text displaying selected card info")]
    public TextMeshProUGUI selectedCardText;
    
    [Header("Currency Display")]
    [Tooltip("Text displaying current currency (legacy - used if currencyDisplayPrefab not set)")]
    public TextMeshProUGUI currencyDisplayText;
    
    [Tooltip("Container for currency display prefab (optional - if set, will use prefab instead of text)")]
    public Transform currencyDisplayContainer;
    
    [Tooltip("Prefab for displaying currency (e.g., CurrencyListPrefab)")]
    public GameObject currencyDisplayPrefab;
    
    [Tooltip("Hide plus/minus buttons in currency prefab if present")]
    public bool hideCurrencyButtons = true;
    
    [Header("Vendor Configuration")]
    [Tooltip("Currency type used for purchasing cards")]
    public CurrencyType purchaseCurrencyType = CurrencyType.OrbOfGeneration;
    
    [Tooltip("Base price for cards")]
    [Range(1, 10)]
    public int baseCardPrice = 1;
    
    // Class distribution mapping: Each class can buy cards from these classes
    private static readonly Dictionary<string, string[]> ClassDistributionMap = new Dictionary<string, string[]>
    {
        { "Marauder", new[] { "Apostle", "Brawler" } },
        { "Ranger", new[] { "Brawler", "Thief" } },
        { "Witch", new[] { "Apostle", "Thief" } },
        { "Apostle", new[] { "Witch", "Marauder" } },
        { "Brawler", new[] { "Ranger", "Marauder" } },
        { "Thief", new[] { "Ranger", "Witch" } }
    };
    
    private List<CardDataExtended> availableCards = new List<CardDataExtended>();
    private List<SeerCardVendorSlot> cardSlots = new List<SeerCardVendorSlot>();
    private SeerCardVendorSlot selectedSlot = null;
    private CharacterManager characterManager;
    private LootManager lootManager;
    private CurrencyDatabase currencyDatabase;
    private GameObject currentCurrencyDisplayInstance;
    
    private void Awake()
    {
        characterManager = CharacterManager.Instance;
        lootManager = LootManager.Instance;
        currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
    }
    
    private void Start()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
            purchaseButton.interactable = false;
        }
    }
    
    private void OnEnable()
    {
        LoadAvailableCards();
        DisplayVendorCards();
        SetupCurrencyDisplay();
        UpdateCurrencyDisplay();
    }
    
    private void OnDisable()
    {
        CleanupCurrencyDisplay();
    }
    
    /// <summary>
    /// Loads available cards based on the player's class distribution rules
    /// </summary>
    private void LoadAvailableCards()
    {
        availableCards.Clear();
        
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[SeerCardVendor] No character loaded! Cannot determine available cards.");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        string playerClass = character.characterClass;
        
        if (string.IsNullOrEmpty(playerClass))
        {
            Debug.LogWarning("[SeerCardVendor] Character class is empty! Cannot determine available cards.");
            return;
        }
        
        // Get classes to load cards from
        if (!ClassDistributionMap.ContainsKey(playerClass))
        {
            Debug.LogWarning($"[SeerCardVendor] No distribution mapping found for class: {playerClass}");
            return;
        }
        
        string[] sourceClasses = ClassDistributionMap[playerClass];
        Debug.Log($"[SeerCardVendor] Loading cards for {playerClass} from classes: {string.Join(", ", sourceClasses)}");
        
        // Load cards from each source class
        foreach (string sourceClass in sourceClasses)
        {
            List<CardDataExtended> classCards = LoadStarterCardsFromClass(sourceClass);
            availableCards.AddRange(classCards);
        }
        
        // Remove duplicates (in case same card appears in multiple starter decks)
        availableCards = availableCards.Distinct().ToList();
        
        Debug.Log($"[SeerCardVendor] Loaded {availableCards.Count} unique cards for {playerClass}");
    }
    
    /// <summary>
    /// Loads starter cards from a specific class folder
    /// </summary>
    private List<CardDataExtended> LoadStarterCardsFromClass(string className)
    {
        List<CardDataExtended> cards = new List<CardDataExtended>();
        
        // Try multiple possible paths for starter deck definition
        string[] possiblePaths = {
            $"Cards/{className}/{className}Start",
            $"Cards/{className}/{className}StarterDeck",
            $"Cards/{className}/{className}Starter"
        };
        
        StarterDeckDefinition starterDeck = null;
        string loadedPath = "";
        
        foreach (string path in possiblePaths)
        {
            starterDeck = Resources.Load<StarterDeckDefinition>(path);
            if (starterDeck != null)
            {
                loadedPath = path;
                break;
            }
        }
        
        if (starterDeck == null)
        {
            Debug.LogWarning($"[SeerCardVendor] Could not find starter deck for {className}. Tried paths: {string.Join(", ", possiblePaths)}");
            return cards;
        }
        
        // Get unique cards from the starter deck
        HashSet<CardDataExtended> uniqueCards = new HashSet<CardDataExtended>();
        foreach (var entry in starterDeck.cards)
        {
            if (entry.card != null)
            {
                uniqueCards.Add(entry.card);
            }
        }
        
        cards.AddRange(uniqueCards);
        Debug.Log($"[SeerCardVendor] Loaded {cards.Count} unique cards from {className} starter deck");
        
        return cards;
    }
    
    /// <summary>
    /// Displays available cards in the vendor grid
    /// </summary>
    private void DisplayVendorCards()
    {
        if (cardGridContainer == null || cardSlotPrefab == null)
        {
            Debug.LogError("[SeerCardVendor] Card grid container or prefab not assigned!");
            return;
        }
        
        // Clear existing slots
        ClearVendorCards();
        cardSlots.Clear();
        selectedSlot = null;
        
        // Create slots for each card
        foreach (CardDataExtended card in availableCards)
        {
            if (card == null) continue;
            
            GameObject slotObj = Instantiate(cardSlotPrefab, cardGridContainer);
            
            // Setup vendor card slot
            SeerCardVendorSlot slot = slotObj.GetComponent<SeerCardVendorSlot>();
            if (slot == null)
            {
                slot = slotObj.AddComponent<SeerCardVendorSlot>();
            }
            
            // Initialize slot
            slot.Initialize(card, baseCardPrice, purchaseCurrencyType, OnCardSelected);
            
            cardSlots.Add(slot);
        }
        
        UpdatePurchaseButton();
    }
    
    /// <summary>
    /// Clears all vendor card slots
    /// </summary>
    private void ClearVendorCards()
    {
        if (cardGridContainer == null) return;
        
        foreach (Transform child in cardGridContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    /// <summary>
    /// Handles card selection
    /// </summary>
    private void OnCardSelected(SeerCardVendorSlot slot)
    {
        if (slot == null || slot.IsSoldOut())
        {
            return;
        }
        
        // Deselect previous slot
        if (selectedSlot != null && selectedSlot != slot)
        {
            selectedSlot.SetSelected(false);
        }
        
        // Select new slot
        selectedSlot = slot;
        selectedSlot.SetSelected(true);
        
        // Update UI
        UpdateSelectedCardDisplay();
        UpdatePurchaseButton();
    }
    
    /// <summary>
    /// Handles purchase button click
    /// </summary>
    private void OnPurchaseClicked()
    {
        if (selectedSlot == null || selectedSlot.IsSoldOut())
        {
            Debug.LogWarning("[SeerCardVendor] No card selected or card is sold out!");
            return;
        }
        
        CardDataExtended card = selectedSlot.GetCard();
        int price = selectedSlot.GetPrice();
        
        if (card == null)
        {
            Debug.LogError("[SeerCardVendor] Cannot purchase null card!");
            return;
        }
        
        // Check currency
        if (lootManager == null || lootManager.GetCurrencyAmount(purchaseCurrencyType) < price)
        {
            Debug.LogWarning($"[SeerCardVendor] Insufficient currency! Need {price} {purchaseCurrencyType}");
            // TODO: Show error message to player
            return;
        }
        
        // Check if player already has the card (informational only - allow purchase of duplicates)
        if (characterManager != null && characterManager.HasCharacter())
        {
            Character character = characterManager.GetCurrentCharacter();
            if (character.deckData.OwnsCard(card.cardName))
            {
                Debug.Log($"[SeerCardVendor] Player already owns card: {card.cardName} (allowing duplicate purchase)");
            }
        }
        
        // Spend currency
        if (!lootManager.RemoveCurrency(purchaseCurrencyType, price))
        {
            Debug.LogWarning("[SeerCardVendor] Failed to spend currency!");
            return;
        }
        
        // Unlock card for character
        if (characterManager != null && characterManager.HasCharacter())
        {
            characterManager.UnlockCard(card.cardName);
            Debug.Log($"[SeerCardVendor] Purchased and unlocked card: {card.cardName} for {price} {purchaseCurrencyType}");
        }
        else
        {
            Debug.LogWarning("[SeerCardVendor] Cannot unlock card: No character loaded!");
        }
        
        // Mark slot as sold out (optional - could allow multiple purchases)
        // selectedSlot.MarkAsSoldOut();
        
        // Clear selection
        selectedSlot.SetSelected(false);
        selectedSlot = null;
        UpdateSelectedCardDisplay();
        UpdatePurchaseButton();
        UpdateCurrencyDisplay();
    }
    
    /// <summary>
    /// Updates the selected card display
    /// </summary>
    private void UpdateSelectedCardDisplay()
    {
        if (selectedCardText == null) return;
        
        if (selectedSlot != null && !selectedSlot.IsSoldOut())
        {
            CardDataExtended card = selectedSlot.GetCard();
            selectedCardText.text = $"Selected: {card.cardName} - {selectedSlot.GetPrice()} {purchaseCurrencyType}";
        }
        else
        {
            selectedCardText.text = "No card selected";
        }
    }
    
    /// <summary>
    /// Updates the purchase button state
    /// </summary>
    private void UpdatePurchaseButton()
    {
        if (purchaseButton == null) return;
        
        bool canPurchase = selectedSlot != null && 
                          !selectedSlot.IsSoldOut() && 
                          lootManager != null &&
                          lootManager.GetCurrencyAmount(purchaseCurrencyType) >= selectedSlot.GetPrice();
        
        purchaseButton.interactable = canPurchase;
    }
    
    /// <summary>
    /// Sets up the currency display (prefab or text)
    /// </summary>
    private void SetupCurrencyDisplay()
    {
        if (currencyDisplayContainer != null && currencyDisplayPrefab != null)
        {
            // Clear existing display
            CleanupCurrencyDisplay();
            
            // Create currency display instance
            GameObject displayItem = Instantiate(currencyDisplayPrefab, currencyDisplayContainer);
            currentCurrencyDisplayInstance = displayItem;
            
            // Setup the display item
            SetupCurrencyDisplayItem(displayItem);
        }
    }
    
    /// <summary>
    /// Sets up a currency display item (similar to SeerCardGenerationUI)
    /// </summary>
    private void SetupCurrencyDisplayItem(GameObject displayItem)
    {
        // Find components (trying multiple naming patterns)
        TMP_Text countText = FindChildComponent<TMP_Text>(displayItem, "CountText", "Count", "QuantityText");
        TMP_Text nameText = FindChildComponent<TMP_Text>(displayItem, "NameText", "Name", "LabelText");
        Image iconImage = FindChildComponent<Image>(displayItem, "IconImage", "Icon", "SpriteImage");
        
        // Hide buttons if requested (for read-only display)
        if (hideCurrencyButtons)
        {
            Button plusButton = FindChildComponent<Button>(displayItem, "PlusButton", "+Button", "AddButton");
            Button minusButton = FindChildComponent<Button>(displayItem, "MinusButton", "-Button", "RemoveButton");
            
            if (plusButton != null)
                plusButton.gameObject.SetActive(false);
            if (minusButton != null)
                minusButton.gameObject.SetActive(false);
        }
        
        // Get currency data from CurrencyDatabase
        if (currencyDatabase != null)
        {
            CurrencyData currencyData = currencyDatabase.GetCurrency(purchaseCurrencyType);
            if (currencyData != null)
            {
                if (nameText != null)
                    nameText.text = currencyData.currencyName;
                if (iconImage != null && currencyData.currencySprite != null)
                    iconImage.sprite = currencyData.currencySprite;
            }
            else
            {
                Debug.LogWarning($"[SeerCardVendor] CurrencyData not found for {purchaseCurrencyType} in CurrencyDatabase!");
            }
        }
        else
        {
            Debug.LogWarning("[SeerCardVendor] CurrencyDatabase is null! Cannot populate currency display.");
        }
        
        // Store reference for updates
        CurrencyDisplayData displayData = displayItem.GetComponent<CurrencyDisplayData>();
        if (displayData == null)
            displayData = displayItem.AddComponent<CurrencyDisplayData>();
        
        displayData.countText = countText;
    }
    
    /// <summary>
    /// Helper method to find a component in child objects, trying multiple name patterns
    /// </summary>
    private T FindChildComponent<T>(GameObject parent, params string[] names) where T : Component
    {
        // First, check if there's a "Content" child - if so, search within that
        Transform contentChild = parent.transform.Find("Content");
        if (contentChild == null)
        {
            contentChild = parent.transform.Find("content");
        }
        
        // Determine the root to search from
        Transform searchRoot = contentChild != null ? contentChild : parent.transform;
        
        foreach (string name in names)
        {
            Transform child = searchRoot.Find(name);
            if (child != null)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                    return component;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Cleans up the currency display instance
    /// </summary>
    private void CleanupCurrencyDisplay()
    {
        if (currentCurrencyDisplayInstance != null)
        {
            Destroy(currentCurrencyDisplayInstance);
            currentCurrencyDisplayInstance = null;
        }
    }
    
    /// <summary>
    /// Updates the currency display
    /// </summary>
    public void UpdateCurrencyDisplay()
    {
        if (lootManager == null)
            return;
        
        int currencyAmount = lootManager.GetCurrencyAmount(purchaseCurrencyType);
        
        // Update prefab display if available
        if (currentCurrencyDisplayInstance != null)
        {
            CurrencyDisplayData displayData = currentCurrencyDisplayInstance.GetComponent<CurrencyDisplayData>();
            if (displayData != null && displayData.countText != null)
            {
                displayData.countText.text = currencyAmount.ToString();
            }
        }
        // Fallback to text display
        else if (currencyDisplayText != null)
        {
            currencyDisplayText.text = $"{purchaseCurrencyType}: {currencyAmount}";
        }
    }
}

/// <summary>
/// Helper component to store references for currency display (similar to GenerationOrbDisplayData)
/// </summary>
public class CurrencyDisplayData : MonoBehaviour
{
    public TMP_Text countText;
}

