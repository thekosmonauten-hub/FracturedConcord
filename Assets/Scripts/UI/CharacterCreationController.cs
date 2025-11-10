using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UIToolkitButton = UnityEngine.UIElements.Button;
using UGUIButton = UnityEngine.UI.Button;

public enum CardAlignment
{
    Left,
    Center,
    Right
}

public class CharacterCreationController : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    
    [Header("Card Prefabs")]
    [Tooltip("Small card prefab for deck preview (DeckCardPrefab.prefab)")]
    public GameObject deckCardPrefab;
    
    [Tooltip("Full card prefab for hover preview (CardPrefab.prefab)")]
    public GameObject fullCardPrefab;
    
    [Header("UGUI References")]
    [Tooltip("Canvas for rendering UGUI card previews on top of UI Toolkit")]
    public Canvas cardPreviewCanvas;
    
    [Tooltip("Parent transform for the full card preview")]
    public Transform cardHoverPreviewParent;
    
    [Header("Deck Preview Layout Settings")]
    [Tooltip("Maximum number of cards per row before wrapping to next row")]
    [Range(1, 10)]
    public int maxCardsPerRow = 6;
    
    [Tooltip("Horizontal spacing between cards")]
    [Range(0f, 50f)]
    public float cardSpacingX = 10f;
    
    [Tooltip("Vertical spacing between card rows")]
    [Range(0f, 50f)]
    public float cardSpacingY = 10f;
    
    [Tooltip("Fixed width for each card display")]
    [Range(80f, 200f)]
    public float cardWidth = 120f;
    
    [Tooltip("Fixed height for each card display")]
    [Range(30f, 80f)]
    public float cardHeight = 40f;
    
    [Tooltip("How to align cards within the preview panel")]
    public CardAlignment cardAlignment = CardAlignment.Center;
    
    [Header("Card Hover Settings")]
    [Tooltip("Vertical offset above the hovered card (pixels)")]
    [Range(-200f, 200f)]
    public float hoverOffsetY = 100f;
    
    [Tooltip("Horizontal offset from the hovered card (pixels)")]
    [Range(-200f, 200f)]
    public float hoverOffsetX = 0f;
    
    [Tooltip("Scale of the hover card preview")]
    [Range(0.5f, 2.0f)]
    public float hoverCardScale = 1.2f;
    
    // UI Elements - these will be connected to your UXML elements
    private TextField characterNameInput;
    private UIToolkitButton createCharacterButton;
    private UIToolkitButton backButton;
    private VisualElement characterPreview;
    private ScrollView deckPreviewScroll;
    private VisualElement deckCardGrid;
    
    // Class Buttons - connected to your class selection buttons
    private UIToolkitButton witchButton;
    private UIToolkitButton marauderButton;
    private UIToolkitButton rangerButton;
    private UIToolkitButton thiefButton;
    private UIToolkitButton apostleButton;
    private UIToolkitButton brawlerButton;
    
    // Current selection state
    private string selectedClass = "";
    private string characterName = "";
    
    // Deck Management
    [Header("Deck Management")]
    public StarterDeckManager starterDeckManager;
    
    // Card hover system
    private GameObject currentHoverCard = null;
    private List<GameObject> deckPreviewCards = new List<GameObject>();
    
    private void Start()
    {
        SetupUI();
        SetupButtonEvents();
        UpdateCreateButtonState();
    }
    
    private void SetupUI()
    {
        // Get UIDocument if not assigned
        if (uiDocument == null)
        {
            uiDocument = GetComponentInParent<UIDocument>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogError("CharacterCreationController: No UIDocument found!");
            return;
        }
        
        var root = uiDocument.rootVisualElement;
        
        // Connect to UXML elements by name
        characterNameInput = root.Q<TextField>("CharacterNameInput");
        createCharacterButton = root.Q<UIToolkitButton>("CreateCharacterButton");
        backButton = root.Q<UIToolkitButton>("BackButton");
        characterPreview = root.Q<VisualElement>("CharacterPreview");
        deckPreviewScroll = root.Q<ScrollView>("StarterDeckPreview");
        deckCardGrid = root.Q<VisualElement>("DeckCardGrid");
        
        // Connect to class buttons
        witchButton = root.Q<UIToolkitButton>("WitchButton");
        marauderButton = root.Q<UIToolkitButton>("MarauderButton");
        rangerButton = root.Q<UIToolkitButton>("RangerButton");
        thiefButton = root.Q<UIToolkitButton>("ThiefButton");
        apostleButton = root.Q<UIToolkitButton>("ApostleButton");
        brawlerButton = root.Q<UIToolkitButton>("BrawlerButton");
        
        // Verify connections
        if (characterNameInput == null) Debug.LogError("CharacterNameInput not found!");
        if (createCharacterButton == null) Debug.LogError("CreateCharacterButton not found!");
        if (backButton == null) Debug.LogError("BackButton not found!");
        if (deckCardGrid == null) Debug.LogError("DeckCardGrid not found!");
        
        // Setup card preview canvas if not assigned
        if (cardPreviewCanvas == null)
        {
            cardPreviewCanvas = FindFirstObjectByType<Canvas>();
        }
        
        // Create hover preview parent if needed
        if (cardHoverPreviewParent == null && cardPreviewCanvas != null)
        {
            GameObject hoverParent = new GameObject("CardHoverPreviewParent");
            hoverParent.transform.SetParent(cardPreviewCanvas.transform, false);
            cardHoverPreviewParent = hoverParent.transform;
            
            // Position it at a good location for preview
            RectTransform rt = hoverParent.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(300f, 0f); // To the right of center
            rt.sizeDelta = new Vector2(160, 220);
        }
    }
    
    private void SetupButtonEvents()
    {
        // Name input event
        if (characterNameInput != null)
        {
            characterNameInput.RegisterValueChangedCallback(OnNameChanged);
        }
        
        // Action buttons
        if (createCharacterButton != null)
        {
            createCharacterButton.clicked += OnCreateCharacter;
        }
        
        if (backButton != null)
        {
            backButton.clicked += OnBackToMainMenu;
        }
        
        // Class selection buttons
        if (witchButton != null)
        {
            witchButton.clicked += () => OnClassSelected("Witch");
        }
        
        if (marauderButton != null)
        {
            marauderButton.clicked += () => OnClassSelected("Marauder");
        }
        
        if (rangerButton != null)
        {
            rangerButton.clicked += () => OnClassSelected("Ranger");
        }
        
        if (thiefButton != null)
        {
            thiefButton.clicked += () => OnClassSelected("Thief");
        }
        
        if (apostleButton != null)
        {
            apostleButton.clicked += () => OnClassSelected("Apostle");
        }
        
        if (brawlerButton != null)
        {
            brawlerButton.clicked += () => OnClassSelected("Brawler");
        }
    }
    
    // Event handlers
    private void OnNameChanged(ChangeEvent<string> evt)
    {
        characterName = evt.newValue;
        UpdateCreateButtonState();
    }
    
    private void OnClassSelected(string className)
    {
        selectedClass = className;
        UpdateClassSelection();
        UpdateCreateButtonState();
        UpdateCharacterPreview();
    }
    
    private void UpdateClassSelection()
    {
        // Reset all button styles
        ResetClassButtonStyles();
        
        // Highlight selected button
        UIToolkitButton selectedButton = GetClassButton(selectedClass);
        if (selectedButton != null)
        {
            selectedButton.AddToClassList("selected");
        }
    }
    
    private void ResetClassButtonStyles()
    {
        UIToolkitButton[] classButtons = { witchButton, marauderButton, rangerButton, 
                                 thiefButton, apostleButton, brawlerButton };
        
        foreach (var button in classButtons)
        {
            if (button != null)
            {
                button.RemoveFromClassList("selected");
            }
        }
    }
    
    private UIToolkitButton GetClassButton(string className)
    {
        switch (className)
        {
            case "Witch": return witchButton;
            case "Marauder": return marauderButton;
            case "Ranger": return rangerButton;
            case "Thief": return thiefButton;
            case "Apostle": return apostleButton;
            case "Brawler": return brawlerButton;
            default: return null;
        }
    }
    
    private void UpdateCharacterPreview()
    {
        if (characterPreview != null)
        {
            // Update preview based on selected class
            // You can add preview images or styling here
            characterPreview.style.backgroundColor = GetClassColor(selectedClass);
        }

        // Update starter deck preview
        UpdateStarterDeckPreview();
    }

    private void UpdateStarterDeckPreview()
    {
        if (deckCardGrid == null) return;
        
        // Clear existing preview
        deckCardGrid.Clear();
        ClearDeckPreviewCards();

        if (string.IsNullOrEmpty(selectedClass)) return;

        var sdm = starterDeckManager != null ? starterDeckManager : StarterDeckManager.Instance;
        var def = sdm != null ? sdm.GetDefinitionForClass(selectedClass) : null;
        if (def == null || def.cards == null || def.cards.Count == 0) 
        {
            Debug.LogWarning($"No starter deck definition found for class: {selectedClass}");
            return;
        }

        Debug.Log($"<color=cyan>Building deck preview for {selectedClass} with {def.cards.Count} unique cards</color>");

        // Decide whether to use UGUI card prefabs or UI Toolkit visual elements
        bool useVisualCards = deckCardPrefab != null && cardPreviewCanvas != null;
        bool useUIToolkitHover = fullCardPrefab != null && !useVisualCards;

        if (useVisualCards)
        {
            // OPTION 1: Use actual UGUI card prefabs for visual fidelity
            // Make UI Toolkit elements ignore pointer events so UGUI cards can receive hover
            SetDeckPreviewPickingMode(PickingMode.Ignore);
            CreateVisualDeckPreview(def);
        }
        else if (useUIToolkitHover)
        {
            // OPTION 2: Use UI Toolkit with UGUI hover preview
            SetDeckPreviewPickingMode(PickingMode.Position);
            CreateUIToolkitDeckPreviewWithHover(def);
        }
        else
        {
            // OPTION 3: Use simple text rows (fallback)
            SetDeckPreviewPickingMode(PickingMode.Position);
            CreateTextDeckPreview(def);
        }
        
        Debug.Log($"<color=green>✓ Deck preview created with {def.cards.Count} card types (visual: {useVisualCards})</color>");
    }
    
    /// <summary>
    /// Set picking mode for UI Toolkit deck preview elements to control pointer event handling
    /// </summary>
    private void SetDeckPreviewPickingMode(PickingMode mode)
    {
        if (deckPreviewScroll != null)
        {
            deckPreviewScroll.pickingMode = mode;
            Debug.Log($"<color=cyan>Set deck preview scroll picking mode to: {mode}</color>");
        }
        
        if (deckCardGrid != null)
        {
            deckCardGrid.pickingMode = mode;
            Debug.Log($"<color=cyan>Set deck card grid picking mode to: {mode}</color>");
        }
    }
    
    /// <summary>
    /// Create deck preview using visual UGUI card prefabs embedded in UI Toolkit
    /// </summary>
    private void CreateVisualDeckPreview(StarterDeckDefinition def)
    {
        // Create a parent GameObject for the deck preview cards
        GameObject deckPreviewParent = new GameObject("DeckPreviewCards");
        
        // If we have a canvas, use it; otherwise create a temporary one
        if (cardPreviewCanvas != null)
        {
            deckPreviewParent.transform.SetParent(cardPreviewCanvas.transform, false);
        }
        else
        {
            // Create a temporary canvas for the cards
            GameObject tempCanvas = new GameObject("TempCardCanvas");
            Canvas canvas = tempCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 101; // Above UI Toolkit (-100), below hover preview (200)
            
            CanvasScaler scaler = tempCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            tempCanvas.AddComponent<GraphicRaycaster>();
            
            deckPreviewParent.transform.SetParent(tempCanvas.transform, false);
            deckPreviewCards.Add(tempCanvas); // Track for cleanup
        }
        
        // Setup RectTransform for positioning
        RectTransform parentRT = deckPreviewParent.AddComponent<RectTransform>();
        parentRT.anchorMin = new Vector2(0.5f, 0.5f);
        parentRT.anchorMax = new Vector2(0.5f, 0.5f);
        parentRT.anchoredPosition = new Vector2(0f, -200f); // Below class buttons
        // Calculate dynamic height based on number of cards and rows
        int totalCards = def.cards.Count;
        int rows = Mathf.CeilToInt((float)totalCards / maxCardsPerRow);
        float dynamicHeight = Mathf.Max(100f, (rows * cardHeight) + ((rows - 1) * cardSpacingY) + 40f); // 40f for padding
        
        parentRT.sizeDelta = new Vector2(800f, dynamicHeight);
        
        Debug.Log($"<color=cyan>Deck Preview Layout: {totalCards} cards, {rows} rows, {maxCardsPerRow} per row, height: {dynamicHeight}, alignment: {cardAlignment}</color>");
        
        // Add grid layout group for card arrangement with wrapping
        UnityEngine.UI.GridLayoutGroup layout = deckPreviewParent.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        layout.cellSize = new Vector2(cardWidth, cardHeight); // Configurable card size
        layout.spacing = new Vector2(cardSpacingX, cardSpacingY); // Configurable spacing
        layout.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
        
        // Set alignment based on configuration
        switch (cardAlignment)
        {
            case CardAlignment.Left:
                layout.childAlignment = UnityEngine.TextAnchor.UpperLeft;
                break;
            case CardAlignment.Center:
                layout.childAlignment = UnityEngine.TextAnchor.UpperCenter;
                break;
            case CardAlignment.Right:
                layout.childAlignment = UnityEngine.TextAnchor.UpperRight;
                break;
        }
        
        layout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = maxCardsPerRow; // Configurable max cards per row
        
        deckPreviewCards.Add(deckPreviewParent);
        
        foreach (var entry in def.cards)
        {
            if (entry == null || entry.card == null || entry.count <= 0) continue;
            
            // Instantiate the compact deck card prefab
            GameObject cardObj = Instantiate(deckCardPrefab, deckPreviewParent.transform);
            cardObj.name = $"DeckCard_{entry.card.cardName}";
            
            // Setup the card visual using DeckCardListUI
            DeckCardListUI cardUI = cardObj.GetComponent<DeckCardListUI>();
            if (cardUI != null)
            {
                // Create a DeckCardEntry using the constructor
                CardData cardData = ConvertToCardData(entry.card);
                DeckCardEntry deckEntry = new DeckCardEntry(cardData, entry.count);
                
                // Initialize with null DeckBuilderUI since we're in character creation
                cardUI.Initialize(deckEntry, null);
            }
            else
            {
                Debug.LogWarning($"DeckCardPrefab missing DeckCardListUI component!");
            }
            
            // GridLayoutGroup will handle sizing automatically
            
            // Setup hover events using EventTrigger for UGUI
            AddCardHoverEvents(cardObj, entry.card);
            
            deckPreviewCards.Add(cardObj);
            Debug.Log($"  Added visual card: x{entry.count} {entry.card.cardName}");
        }
    }
    
    /// <summary>
    /// Create deck preview using UI Toolkit with UGUI hover preview
    /// </summary>
    private void CreateUIToolkitDeckPreviewWithHover(StarterDeckDefinition def)
    {
        Debug.Log($"<color=cyan>Creating UI Toolkit deck preview with hover for {def.characterClass}</color>");
        
        foreach (var entry in def.cards)
        {
            if (entry == null || entry.card == null || entry.count <= 0) continue;
            
            // Create a visual card row using UI Toolkit
            VisualElement cardRow = CreateCardVisualElement(entry);
            deckCardGrid.Add(cardRow);
            
            Debug.Log($"  Added UI Toolkit card with hover: x{entry.count} {entry.card.cardName}");
        }
    }
    
    /// <summary>
    /// Create a single card visual element with proper styling and hover
    /// </summary>
    private VisualElement CreateCardVisualElement(StarterDeckDefinition.CardEntry entry)
    {
        // Create main container
        VisualElement cardContainer = new VisualElement();
        cardContainer.AddToClassList("deck-preview-row");
        cardContainer.name = $"CardRow_{entry.card.cardName}";
        
        // Create count label
        Label countLabel = new Label($"x{entry.count}");
        countLabel.AddToClassList("deck-card-count");
        cardContainer.Add(countLabel);
        
        // Create card name label
        Label nameLabel = new Label(entry.card.cardName);
        nameLabel.AddToClassList("deck-card-name");
        cardContainer.Add(nameLabel);
        
        // Add hover events
        cardContainer.RegisterCallback<MouseEnterEvent>(evt => OnCardRowHoverEnter(entry.card, cardContainer));
        cardContainer.RegisterCallback<MouseLeaveEvent>(evt => OnCardRowHoverExit());
        
        return cardContainer;
    }
    
    /// <summary>
    /// Create deck preview using simple text labels (fallback)
    /// </summary>
    private void CreateTextDeckPreview(StarterDeckDefinition def)
    {
        foreach (var entry in def.cards)
        {
            if (entry == null || entry.card == null || entry.count <= 0) continue;
            
            // Create a container for this card entry
            VisualElement cardRow = new VisualElement();
            cardRow.AddToClassList("deck-preview-row");
            
            // Add count label
            Label countLabel = new Label($"x{entry.count}");
            countLabel.AddToClassList("deck-card-count");
            cardRow.Add(countLabel);
            
            // Add card name label
            Label nameLabel = new Label(entry.card.cardName);
            nameLabel.AddToClassList("deck-card-name");
            cardRow.Add(nameLabel);
            
            // Register hover events to show full card preview
            CardDataExtended cardData = entry.card;
            cardRow.RegisterCallback<MouseEnterEvent>(evt => OnCardRowHoverEnter(cardData, cardRow));
            cardRow.RegisterCallback<MouseLeaveEvent>(evt => OnCardRowHoverExit());
            
            deckCardGrid.Add(cardRow);
            
            Debug.Log($"  Added text row: x{entry.count} {entry.card.cardName}");
        }
    }
    
    /// <summary>
    /// Add hover event handlers to a UGUI card GameObject
    /// </summary>
    private void AddCardHoverEvents(GameObject cardObj, CardDataExtended cardData)
    {
        // Add EventTrigger component for hover events
        UnityEngine.EventSystems.EventTrigger trigger = cardObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = cardObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // Add pointer enter event (hover start)
        UnityEngine.EventSystems.EventTrigger.Entry enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { 
            Debug.Log($"<color=cyan>Hover ENTER: {cardData.cardName}</color>");
            OnCardPrefabHoverEnter(cardData, cardObj); 
        });
        trigger.triggers.Add(enterEntry);
        
        // Add pointer exit event (hover end)
        UnityEngine.EventSystems.EventTrigger.Entry exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { 
            Debug.Log($"<color=cyan>Hover EXIT: {cardData.cardName}</color>");
            OnCardPrefabHoverExit(); 
        });
        trigger.triggers.Add(exitEntry);
    }
    
    /// <summary>
    /// Show full card preview when hovering over a UGUI deck card prefab
    /// </summary>
    private void OnCardPrefabHoverEnter(CardDataExtended cardData, GameObject cardObj)
    {
        if (fullCardPrefab == null || cardData == null)
        {
            Debug.LogWarning($"Cannot show card preview: Missing prefab ({fullCardPrefab == null}) or card data ({cardData == null})");
            return;
        }
        
        // Auto-create hover preview parent if not assigned
        if (cardHoverPreviewParent == null)
        {
            GameObject hoverParent = new GameObject("CardHoverPreviewParent");
            Canvas hoverCanvas = hoverParent.AddComponent<Canvas>();
            hoverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hoverCanvas.sortingOrder = 200; // Above deck preview cards (101) and UI Toolkit (-100)
            
            CanvasScaler scaler = hoverParent.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            hoverParent.AddComponent<GraphicRaycaster>();
            cardHoverPreviewParent = hoverParent.transform;
            
            Debug.Log("<color=yellow>Auto-created card hover preview parent</color>");
        }
        
        // Hide previous hover card if any
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
            currentHoverCard = null;
        }
        
        // Instantiate full card prefab
        currentHoverCard = Instantiate(fullCardPrefab, cardHoverPreviewParent);
        currentHoverCard.name = $"HoverPreview_{cardData.cardName}";
        
        Debug.Log($"<color=green>Created hover card GameObject: {currentHoverCard.name}</color>");
        
        // Setup the card visual using DeckBuilderCardUI
        DeckBuilderCardUI cardUI = currentHoverCard.GetComponent<DeckBuilderCardUI>();
        if (cardUI != null)
        {
            // Create a temporary character for stat display
            Character tempChar = new Character("Preview", selectedClass);
            cardUI.Initialize(cardData, null, tempChar);
            Debug.Log($"<color=yellow>Showing hover preview for: {cardData.cardName}</color>");
        }
        else
        {
            Debug.LogWarning($"<color=red>Full card prefab missing DeckBuilderCardUI component!</color>");
        }
        
        // Add CombatCardAdapter functionality for category icons and combo descriptions
        SetupCharacterCreationCardFeatures(currentHoverCard, cardData);
        
        // Position the card relative to the hovered card
        RectTransform rt = currentHoverCard.GetComponent<RectTransform>();
        RectTransform hoveredCardRT = cardObj.GetComponent<RectTransform>();
        
        if (rt != null && hoveredCardRT != null)
        {
            // Get the world position of the hovered card
            Vector3 hoveredCardWorldPos = hoveredCardRT.position;
            
            // Convert to screen space
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, hoveredCardWorldPos);
            
            // Convert screen position to local position in the hover canvas
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cardHoverPreviewParent.GetComponent<RectTransform>(),
                screenPos,
                null,
                out localPos
            );
            
            // Position relative to the hovered card with configurable offset
            rt.localPosition = new Vector3(localPos.x + hoverOffsetX, localPos.y + hoverOffsetY, 0f);
            rt.localScale = Vector3.one * hoverCardScale;
            
            Debug.Log($"<color=cyan>Positioned hover card at: {rt.localPosition} (hovered card at: {hoveredCardWorldPos})</color>");
        }
        else
        {
            // Fallback to center if positioning fails
            if (rt != null)
            {
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one * hoverCardScale;
                Debug.LogWarning("<color=yellow>Could not position relative to hovered card, using center position</color>");
            }
        }
        
        currentHoverCard.SetActive(true);
        currentHoverCard.transform.SetAsLastSibling();
    }
    
    /// <summary>
    /// Hide full card preview when mouse leaves UGUI deck card
    /// </summary>
    private void OnCardPrefabHoverExit()
    {
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
            currentHoverCard = null;
        }
    }
    
    /// <summary>
    /// Show full card preview when hovering over a deck row
    /// </summary>
    private void OnCardRowHoverEnter(CardDataExtended cardData, VisualElement cardRow)
    {
        if (fullCardPrefab == null || cardHoverPreviewParent == null || cardData == null)
        {
            Debug.LogWarning("Cannot show card preview: Missing prefab, parent, or card data");
            return;
        }
        
        // Hide previous hover card if any
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
            currentHoverCard = null;
        }
        
        // Instantiate full card prefab
        currentHoverCard = Instantiate(fullCardPrefab, cardHoverPreviewParent);
        currentHoverCard.name = $"HoverPreview_{cardData.cardName}";
        
        // Setup the card visual using DeckBuilderCardUI
        DeckBuilderCardUI cardUI = currentHoverCard.GetComponent<DeckBuilderCardUI>();
        if (cardUI != null)
        {
            // Create a temporary character for stat display (use selected class)
            Character tempChar = new Character("Preview", selectedClass);
            cardUI.Initialize(cardData, null, tempChar);
            Debug.Log($"<color=yellow>Showing hover preview for: {cardData.cardName}</color>");
        }
        else
        {
            Debug.LogWarning($"Full card prefab missing DeckBuilderCardUI component!");
        }
        
        // Add CombatCardAdapter functionality for category icons and combo descriptions
        SetupCharacterCreationCardFeatures(currentHoverCard, cardData);
        
        // Position the card (will be centered on the parent transform)
        RectTransform rt = currentHoverCard.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one * 1.2f; // Slightly larger for visibility
        }
        
        // Ensure it's visible and on top
        currentHoverCard.SetActive(true);
        currentHoverCard.transform.SetAsLastSibling();
    }
    
    /// <summary>
    /// Hide full card preview when mouse leaves deck row
    /// </summary>
    private void OnCardRowHoverExit()
    {
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
            currentHoverCard = null;
            Debug.Log($"<color=yellow>Hidden hover preview</color>");
        }
    }
    
    /// <summary>
    /// Setup character creation card features (category icons and combo descriptions)
    /// This replicates CombatCardAdapter functionality for character creation
    /// </summary>
    private void SetupCharacterCreationCardFeatures(GameObject cardObj, CardDataExtended cardData)
    {
        if (cardObj == null || cardData == null) return;
        
        // Setup category icon
        SetupCategoryIcon(cardObj, cardData);
        
        // Setup combo description (AdditionalEffectText)
        SetupComboDescription(cardObj, cardData);
    }
    
    /// <summary>
    /// Setup category icon for character creation cards
    /// </summary>
    private void SetupCategoryIcon(GameObject cardObj, CardDataExtended cardData)
    {
        // Find category icon image
        var categoryIcon = cardObj.GetComponentInChildren<UnityEngine.UI.Image>();
        UnityEngine.UI.Image targetIcon = null;
        
        // Search for category icon by name
        var images = cardObj.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        foreach (var img in images)
        {
            if (img.gameObject.name == "CategoryIcon" || 
                img.gameObject.name == "Category" ||
                img.gameObject.name == "CardCategory")
            {
                targetIcon = img;
                break;
            }
        }
        
        if (targetIcon == null)
        {
            Debug.LogWarning($"[CharacterCreation] CategoryIcon not found on {cardObj.name}");
            return;
        }
        
        // Load CardVisualAssets
        var visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
        if (visualAssets == null)
        {
            Debug.LogWarning("[CharacterCreation] CardVisualAssets not found in Resources");
            return;
        }
        
        // Get category sprite using the same logic as CombatCardAdapter
        Sprite categorySprite = null;
        switch (cardData.category)
        {
            case CardCategory.Attack: categorySprite = visualAssets.attackIcon; break;
            case CardCategory.Guard: categorySprite = visualAssets.guardIcon; break;
            case CardCategory.Skill: categorySprite = visualAssets.skillIcon; break;
            case CardCategory.Power: categorySprite = visualAssets.powerIcon; break;
            default: categorySprite = null; break;
        }
        
        if (categorySprite != null)
        {
            targetIcon.sprite = categorySprite;
            targetIcon.gameObject.SetActive(true);
            Debug.Log($"[CharacterCreation] Set category icon for {cardData.cardName}: {cardData.category}");
        }
        else
        {
            targetIcon.gameObject.SetActive(false);
            Debug.Log($"[CharacterCreation] No category sprite found for {cardData.category}");
        }
    }
    
    /// <summary>
    /// Setup combo description for character creation cards
    /// </summary>
    private void SetupComboDescription(GameObject cardObj, CardDataExtended cardData)
    {
        // Find AdditionalEffectText
        var textObjects = cardObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        TMPro.TextMeshProUGUI additionalEffectText = null;
        
        foreach (var textComp in textObjects)
        {
            if (textComp.gameObject.name == "AdditionalEffectText" || 
                textComp.gameObject.name == "Additional Effects" || 
                textComp.gameObject.name == "AdditionalEffect")
            {
                additionalEffectText = textComp;
                break;
            }
        }
        
        if (additionalEffectText == null)
        {
            Debug.LogWarning($"[CharacterCreation] AdditionalEffectText not found on {cardObj.name}");
            return;
        }
        
        // Get combo description
        Character tempChar = new Character("Preview", selectedClass);
        string dynamicCombo = cardData.GetDynamicComboDescription(tempChar);
        string comboText = string.IsNullOrEmpty(dynamicCombo) ? cardData.comboDescription : dynamicCombo;
        
        if (string.IsNullOrWhiteSpace(comboText))
        {
            additionalEffectText.text = "";
            additionalEffectText.gameObject.SetActive(false);
            Debug.Log($"[CharacterCreation] No combo description for {cardData.cardName}");
        }
        else
        {
            additionalEffectText.text = comboText;
            additionalEffectText.gameObject.SetActive(true);
            Debug.Log($"[CharacterCreation] Set combo description for {cardData.cardName}: '{comboText}'");
        }
    }
    
    /// <summary>
    /// Clear all deck preview card GameObjects
    /// </summary>
    private void ClearDeckPreviewCards()
    {
        foreach (GameObject cardObj in deckPreviewCards)
        {
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
        }
        deckPreviewCards.Clear();
        
        // Also clear hover card if any
        if (currentHoverCard != null)
        {
            Destroy(currentHoverCard);
            currentHoverCard = null;
        }
    }
    
    /// <summary>
    /// Convert CardDataExtended to legacy CardData for compatibility
    /// </summary>
    private CardData ConvertToCardData(CardDataExtended extended)
    {
        if (extended == null) return null;
        
        CardData data = ScriptableObject.CreateInstance<CardData>();
        data.cardName = extended.cardName;
        data.cardImage = extended.cardImage;
        data.playCost = extended.playCost;
        data.rarity = extended.rarity;
        
        // Copy other relevant fields if needed
        return data;
    }
    
    private Color GetClassColor(string className)
    {
        switch (className)
        {
            case "Witch": return new Color(0.5f, 0.2f, 0.8f, 0.3f); // Purple
            case "Marauder": return new Color(0.8f, 0.2f, 0.2f, 0.3f); // Red
            case "Ranger": return new Color(0.2f, 0.8f, 0.2f, 0.3f); // Green
            case "Thief": return new Color(0.2f, 0.2f, 0.8f, 0.3f); // Blue
            case "Apostle": return new Color(0.8f, 0.8f, 0.2f, 0.3f); // Yellow
            case "Brawler": return new Color(0.8f, 0.5f, 0.2f, 0.3f); // Orange
            default: return new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray
        }
    }
    
    private void UpdateCreateButtonState()
    {
        if (createCharacterButton != null)
        {
            bool canCreate = !string.IsNullOrEmpty(characterName) && 
                           !string.IsNullOrEmpty(selectedClass);
            
            createCharacterButton.SetEnabled(canCreate);
            
            if (canCreate)
            {
                createCharacterButton.style.opacity = 1f;
            }
            else
            {
                createCharacterButton.style.opacity = 0.5f;
            }
        }
    }
    
    private void OnCreateCharacter()
    {
        if (string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(selectedClass))
        {
            Debug.LogWarning("Cannot create character: Missing name or class selection");
            return;
        }
        
        // Check if character name already exists
        if (CharacterSaveSystem.Instance.CharacterExists(characterName))
        {
            Debug.LogWarning($"Character name '{characterName}' already exists!");
            // TODO: Show error message to user
            return;
        }
        
        // Create new character using CharacterManager
        // (CharacterManager.CreateCharacter now handles starter deck initialization)
        CharacterManager.Instance.CreateCharacter(characterName, selectedClass);
        
        Debug.Log($"Created new character: {characterName} ({selectedClass})");
        
        // Persist last character for scene continuity
        PlayerPrefs.SetString("LastCharacterName", characterName);
        PlayerPrefs.Save();
        // Transition to main game
        SceneManager.LoadScene("MainGameUI");
    }
    
    private void OnBackToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        // Use direct scene loading instead of transition
        SceneManager.LoadScene("MainMenu");
    }
    
    private void OnDestroy()
    {
        // Clean up card preview objects
        ClearDeckPreviewCards();
        
        // Clean up event listeners to prevent memory leaks
        if (characterNameInput != null)
        {
            characterNameInput.UnregisterValueChangedCallback(OnNameChanged);
        }
        
        if (createCharacterButton != null)
        {
            createCharacterButton.clicked -= OnCreateCharacter;
        }
        
        if (backButton != null)
        {
            backButton.clicked -= OnBackToMainMenu;
        }
        
        // Clean up class button events
        if (witchButton != null) witchButton.clicked -= () => OnClassSelected("Witch");
        if (marauderButton != null) marauderButton.clicked -= () => OnClassSelected("Marauder");
        if (rangerButton != null) rangerButton.clicked -= () => OnClassSelected("Ranger");
        if (thiefButton != null) thiefButton.clicked -= () => OnClassSelected("Thief");
        if (apostleButton != null) apostleButton.clicked -= () => OnClassSelected("Apostle");
        if (brawlerButton != null) brawlerButton.clicked -= () => OnClassSelected("Brawler");
    }
}
