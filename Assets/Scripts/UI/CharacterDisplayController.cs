using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controller for CharacterDisplayUI scene.
/// Displays selected class information and starter deck cards.
/// </summary>
public class CharacterDisplayController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI pathOfTheArchetypeText; // "Path of the X" title
    [SerializeField] private TextMeshProUGUI classNameText;
    [SerializeField] private TextMeshProUGUI classDescriptionText;
    [SerializeField] private Image classIconImage;
    
    [Header("Attribute Display")]
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI dexterityText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    
    [Header("Resource Display (Optional)")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI relianceText;
    
    [Header("Card Preview")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardGridContainer;
    [SerializeField] private int cardsPerRow = 6;
    [SerializeField] private float cardSpacing = 10f;
    
    [Header("Full Card Preview (Hover)")]
    [Tooltip("Prefab with DeckBuilderCardUI for showing full card details on hover")]
    [SerializeField] private GameObject fullCardPreviewPrefab;
    [Tooltip("Container where the full card preview will be displayed")]
    [SerializeField] private Transform fullCardPreviewContainer;
    [Tooltip("Scale of the preview card")]
    [SerializeField] private float previewScale = 1.0f;
    [SerializeField] private bool showHoverDebugLogs = false;
    
    [Header("Ascendancy Display")]
    [Tooltip("The 3 Ascendancy button GameObjects (should have AscendancyButton component)")]
    [SerializeField] private GameObject ascendancy1Button;
    [SerializeField] private GameObject ascendancy2Button;
    [SerializeField] private GameObject ascendancy3Button;
    
    [Tooltip("Panel that opens to show full Ascendancy tree")]
    [SerializeField] private AscendancyDisplayPanel ascendancyDisplayPanel;
    
    [Header("Class Splash Art")]
    [Tooltip("Image to display the base class splash art")]
    [SerializeField] private Image classSplashArtImage;
    [Tooltip("Path to class splash art in Resources (e.g., 'Art/CharCreation/SplashArt/Class/')")]
    [SerializeField] private string classSplashArtResourcePath = "Art/CharCreation/SplashArt/Class";
    [Tooltip("Manual override: assign specific sprites for each class")]
    [SerializeField] private ClassSplashArtOverride[] classSplashArtOverrides;
    
    [Header("Character Name Input")]
    [SerializeField] private TMP_InputField characterNameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    
    [Header("Class Info (Optional - Override Defaults)")]
    [SerializeField] private ClassInfoScriptableObject[] classInfoOverrides;
    
    [Header("Test Mode (Editor Only)")]
    [Tooltip("If enabled, loads Marauder by default for testing. Only works in Editor.")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private string testClass = "Marauder";
    
    // References to persistent managers
    private StarterDeckManager starterDeckManager;
    private ClassSelectionData classSelectionData;
    
    private string selectedClass;
    private List<GameObject> spawnedCards = new List<GameObject>();
    private GameObject currentPreviewCard;
    
    void Start()
    {
        // Get persistent managers
        starterDeckManager = StarterDeckManager.Instance;
        classSelectionData = ClassSelectionData.Instance;
        
        // Get selected class
        selectedClass = classSelectionData.SelectedClass;
        
        // Test mode: Load test class if no class selected (Editor only)
        if (string.IsNullOrEmpty(selectedClass) && testMode)
        {
            #if UNITY_EDITOR
            Debug.LogWarning($"[CharacterDisplayController] Test mode enabled! Loading test class: {testClass}");
            selectedClass = testClass;
            classSelectionData.SetSelectedClass(testClass);
            #else
            Debug.LogError("[CharacterDisplayController] No class selected! Returning to character creation.");
            SceneManager.LoadScene("CharacterCreation");
            return;
            #endif
        }
        
        if (string.IsNullOrEmpty(selectedClass))
        {
            Debug.LogError("[CharacterDisplayController] No class selected! Returning to character creation.");
            SceneManager.LoadScene("CharacterCreation");
            return;
        }
        
        Debug.Log($"[CharacterDisplayController] Displaying class: {selectedClass}");
        
        // Setup UI
        DisplayClassSplashArt();
        DisplayClassInfo();
        DisplayStartingAttributes();
        DisplayStarterDeck();
        DisplayAscendancies();
        SetupButtons();
        
        // Auto-fill character name if already set
        if (characterNameInput != null && !string.IsNullOrEmpty(classSelectionData.CharacterName))
        {
            characterNameInput.text = classSelectionData.CharacterName;
        }
    }
    
    void DisplayClassInfo()
    {
        // Update "Path of the X" title
        if (pathOfTheArchetypeText != null)
        {
            pathOfTheArchetypeText.text = $"Path of the {selectedClass}";
        }
        
        // Try to get class info from overrides first
        ClassInfoScriptableObject classInfo = GetClassInfo(selectedClass);
        
        if (classInfo != null)
        {
            if (classNameText != null)
                classNameText.text = classInfo.className;
            
            if (classDescriptionText != null)
                classDescriptionText.text = classInfo.description;
            
            if (classIconImage != null && classInfo.classIcon != null)
                classIconImage.sprite = classInfo.classIcon;
        }
        else
        {
            // Fallback to defaults
            if (classNameText != null)
                classNameText.text = selectedClass;
            
            if (classDescriptionText != null)
                classDescriptionText.text = GetDefaultClassDescription(selectedClass);
        }
    }
    
    void DisplayStartingAttributes()
    {
        // Get starting attributes for selected class
        var attributes = GetStartingAttributes(selectedClass);
        
        if (strengthText != null)
            strengthText.text = $"Strength: {attributes.strength}";
        
        if (dexterityText != null)
            dexterityText.text = $"Dexterity: {attributes.dexterity}";
        
        if (intelligenceText != null)
            intelligenceText.text = $"Intelligence: {attributes.intelligence}";
        
        if (healthText != null)
            healthText.text = $"Health: {attributes.health}";
        
        if (manaText != null)
            manaText.text = $"Mana: {attributes.mana}";
        
        if (relianceText != null)
            relianceText.text = $"Reliance: {attributes.reliance}";
        
        Debug.Log($"[CharacterDisplayController] Displaying attributes for {selectedClass}: STR {attributes.strength}, DEX {attributes.dexterity}, INT {attributes.intelligence}");
    }
    
    void DisplayStarterDeck()
    {
        if (cardPrefab == null || cardGridContainer == null)
        {
            Debug.LogError("[CharacterDisplayController] Card prefab or container not assigned!");
            Debug.LogError($"  - cardPrefab: {(cardPrefab != null ? "✅ Assigned" : "❌ NULL")}");
            Debug.LogError($"  - cardGridContainer: {(cardGridContainer != null ? "✅ Assigned" : "❌ NULL")}");
            return;
        }
        
        // Clear existing cards
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
                Destroy(card);
        }
        spawnedCards.Clear();
        
        // Get starter deck for selected class
        if (starterDeckManager == null)
        {
            Debug.LogError("[CharacterDisplayController] StarterDeckManager is null!");
            return;
        }
        
        StarterDeckDefinition deckDef = starterDeckManager.GetDefinitionForClass(selectedClass);
        
        if (deckDef == null)
        {
            Debug.LogError($"[CharacterDisplayController] No starter deck found for {selectedClass}");
            Debug.LogError($"  - Check if StarterDeckManager has a StarterDeckDefinition with characterClass = '{selectedClass}'");
            Debug.LogError($"  - Check if StarterDeckDefinition asset is in Resources/StarterDecks folder");
            return;
        }
        
        if (deckDef.cards == null || deckDef.cards.Count == 0)
        {
            Debug.LogWarning($"[CharacterDisplayController] Starter deck for {selectedClass} has no cards!");
            return;
        }
        
        Debug.Log($"[CharacterDisplayController] Displaying {deckDef.cards.Count} card types for {selectedClass}");
        
        // Spawn cards (one per unique card type, not duplicates)
        int cardIndex = 0;
        foreach (var cardEntry in deckDef.cards)
        {
            if (cardEntry.card == null)
            {
                Debug.LogWarning($"[CharacterDisplayController] Card entry {cardIndex} has null card!");
                continue;
            }
            
            // Spawn ONE card per card type (show quantity in the card UI if supported)
            GameObject cardObj = Instantiate(cardPrefab, cardGridContainer);
            cardObj.name = $"Card_{cardIndex}_{cardEntry.card.cardName}";
            
            // Try to set card data using available components
            bool cardSet = false;
            
            // Try DeckBuilderCardUI first (preferred for full card display)
            DeckBuilderCardUI deckBuilderCard = cardObj.GetComponent<DeckBuilderCardUI>();
            if (deckBuilderCard != null)
            {
                // Initialize with CardDataExtended (no conversion needed!)
                deckBuilderCard.Initialize(cardEntry.card, null, null);
                cardSet = true;
                Debug.Log($"[CharacterDisplayController] ✓ Displayed card via DeckBuilderCardUI: {cardEntry.card.cardName} (x{cardEntry.count})");
            }
            
                // Try DeckCardListUI (for simplified card rows)
                if (!cardSet)
                {
                    DeckCardListUI deckListCard = cardObj.GetComponent<DeckCardListUI>();
                    if (deckListCard != null)
                    {
                        // DeckCardListUI expects DeckCardEntry, so create one using constructor
                        // Pass the count so it can display "x6", "x4", etc.
                        DeckCardEntry deckEntry = new DeckCardEntry(cardEntry.card, cardEntry.count);
                        deckListCard.Initialize(deckEntry, null);
                        cardSet = true;
                        Debug.Log($"[CharacterDisplayController] ✓ Displayed card via DeckCardListUI: {cardEntry.card.cardName} (x{cardEntry.count})");
                        
                        // Add hover component for full card preview
                        CharacterScreenCardHover hoverComponent = cardObj.GetComponent<CharacterScreenCardHover>();
                        if (hoverComponent == null)
                        {
                            hoverComponent = cardObj.AddComponent<CharacterScreenCardHover>();
                        }
                        hoverComponent.SetCardData(cardEntry.card);
                        hoverComponent.showDebugLogs = showHoverDebugLogs;
                        
                        // Subscribe to hover events
                        hoverComponent.OnCardHoverEnter += OnCardHoverEnter;
                        hoverComponent.OnCardHoverExit += OnCardHoverExit;
                    }
                }
            
            // Fallback to CardDisplay if available
            if (!cardSet)
            {
                CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
                if (cardDisplay != null)
                {
                    // Convert CardDataExtended to runtime Card
                    Card runtimeCard = cardEntry.card.ToCard();
                    cardDisplay.SetCard(runtimeCard);
                    cardSet = true;
                    Debug.Log($"[CharacterDisplayController] ✓ Displayed card via CardDisplay: {cardEntry.card.cardName} (x{cardEntry.count})");
                }
            }
            
            if (!cardSet)
            {
                Debug.LogWarning($"[CharacterDisplayController] Card prefab '{cardPrefab.name}' has no compatible component!");
                Debug.LogWarning($"  - Expected: DeckBuilderCardUI, DeckCardListUI, or CardDisplay");
                Debug.LogWarning($"  - Card will be spawned but not display data");
            }
            
            // Position card in grid
            PositionCardInGrid(cardObj, cardIndex);
            
            spawnedCards.Add(cardObj);
            cardIndex++;
        }
        
        Debug.Log($"[CharacterDisplayController] ✓ Spawned {spawnedCards.Count} total cards for {selectedClass}");
    }
    
    void PositionCardInGrid(GameObject cardObj, int index)
    {
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        if (rect == null) return;
        
        int row = index / cardsPerRow;
        int col = index % cardsPerRow;
        
        float xPos = col * (rect.sizeDelta.x + cardSpacing);
        float yPos = -row * (rect.sizeDelta.y + cardSpacing);
        
        rect.anchoredPosition = new Vector2(xPos, yPos);
    }
    
    void SetupButtons()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
        
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.AddListener(OnNameChanged);
        }
        
        UpdateConfirmButtonState();
    }
    
    void OnNameChanged(string newName)
    {
        classSelectionData.CharacterName = newName;
        UpdateConfirmButtonState();
    }
    
    void UpdateConfirmButtonState()
    {
        if (confirmButton != null && characterNameInput != null)
        {
            confirmButton.interactable = !string.IsNullOrWhiteSpace(characterNameInput.text);
        }
    }
    
    void OnConfirmClicked()
    {
        string characterName = characterNameInput != null ? characterNameInput.text : "Hero";
        
        if (string.IsNullOrWhiteSpace(characterName))
        {
            Debug.LogWarning("[CharacterDisplayController] Character name is empty!");
            return;
        }
        
        Debug.Log($"[CharacterDisplayController] Creating character: {characterName} ({selectedClass})");
        
        // Create the character using CharacterManager
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager != null)
        {
            characterManager.CreateCharacter(characterName, selectedClass);
        }
        
        // Clear selection data
        classSelectionData.Clear();
        
        // Load Main Game UI with TransitionManager curtain effect fallback to direct load
        TransitionManager transitionManager = TransitionManager.Instance;
        if (transitionManager != null)
        {
            transitionManager.TransitionToSceneWithCurtain("MainGameUI");
        }
        else
        {
            SceneManager.LoadScene("MainGameUI");
        }
    }
    
    void OnBackClicked()
    {
        Debug.Log("[CharacterDisplayController] Back button clicked - playing reverse transition");
        
        // Find VideoTransitionManager
        VideoTransitionManager transitionManager = VideoTransitionManager.Instance;
        
        if (transitionManager != null)
        {
            // Play the same transition video in reverse at 1.5x speed
            // This will use the same video that was used to get here
            transitionManager.PlayTransitionAndLoadScene(
                "CharacterCreation",    // Scene to load
                null,                    // Use same video (default)
                1.5f,                    // 1.5x speed
                true                     // Play in reverse
            );
        }
        else
        {
            Debug.LogWarning("[CharacterDisplayController] VideoTransitionManager not found! Loading scene directly.");
            SceneManager.LoadScene("CharacterCreation");
        }
        
        // Clear selection data
        classSelectionData.Clear();
    }
    
    ClassInfoScriptableObject GetClassInfo(string className)
    {
        if (classInfoOverrides == null) return null;
        
        foreach (var info in classInfoOverrides)
        {
            if (info != null && info.className == className)
            {
                return info;
            }
        }
        
        return null;
    }
    
    string GetDefaultClassDescription(string className)
    {
        switch (className)
        {
            case "Witch":
                return "Intelligence-based spellcaster specializing in arcane magic.";
            case "Marauder":
                return "Strength-based warrior who excels in close combat.";
            case "Ranger":
                return "Dexterity-based archer with ranged expertise.";
            case "Thief":
                return "Dexterity/Intelligence hybrid focusing on stealth and precision.";
            case "Apostle":
                return "Wisdom-based cleric with divine powers.";
            case "Brawler":
                return "Strength-based fighter who uses martial prowess.";
            default:
                return "A brave adventurer.";
        }
    }
    
    StartingAttributes GetStartingAttributes(string className)
    {
        StartingAttributes attributes = new StartingAttributes();
        
        switch (className.ToLower())
        {
            // Primary Classes (Single Attribute Focus)
            case "marauder":
                attributes.strength = 32;
                attributes.dexterity = 14;
                attributes.intelligence = 14;
                break;
            case "ranger":
                attributes.strength = 14;
                attributes.dexterity = 32;
                attributes.intelligence = 14;
                break;
            case "witch":
                attributes.strength = 14;
                attributes.dexterity = 14;
                attributes.intelligence = 32;
                break;
            
            // Hybrid Classes (Dual Attribute Focus)
            case "brawler":
                attributes.strength = 23;
                attributes.dexterity = 23;
                attributes.intelligence = 14;
                break;
            case "thief":
                attributes.strength = 14;
                attributes.dexterity = 23;
                attributes.intelligence = 23;
                break;
            case "apostle":
                attributes.strength = 23;
                attributes.dexterity = 14;
                attributes.intelligence = 23;
                break;
            
            default:
                attributes.strength = 14;
                attributes.dexterity = 14;
                attributes.intelligence = 14;
                break;
        }
        
        // Calculate derived stats (same as Character class)
        attributes.health = CalculateStartingHealth(attributes.strength);
        attributes.mana = 3; // All classes start with 3 mana
        attributes.reliance = 200; // All classes start with 200 reliance
        
        return attributes;
    }
    
    int CalculateStartingHealth(int strength)
    {
        // Same formula as Character.CalculateDerivedStats()
        // Health: 100 base + (4 per STR)
        return 100 + (strength * 4);
    }
    
    #region Class Splash Art Display
    
    /// <summary>
    /// Display the base class splash art
    /// </summary>
    void DisplayClassSplashArt()
    {
        if (classSplashArtImage == null)
        {
            Debug.Log("[CharacterDisplayController] No class splash art image assigned - skipping");
            return;
        }
        
        Sprite splashArt = GetClassSplashArt(selectedClass);
        
        if (splashArt != null)
        {
            classSplashArtImage.sprite = splashArt;
            classSplashArtImage.gameObject.SetActive(true);
            Debug.Log($"[CharacterDisplayController] ✓ Set class splash art for {selectedClass}: {splashArt.name}");
        }
        else
        {
            classSplashArtImage.gameObject.SetActive(false);
            Debug.LogWarning($"[CharacterDisplayController] No splash art found for class: {selectedClass}");
        }
    }
    
    /// <summary>
    /// Get class splash art sprite
    /// </summary>
    Sprite GetClassSplashArt(string className)
    {
        // First, check manual overrides
        if (classSplashArtOverrides != null && classSplashArtOverrides.Length > 0)
        {
            foreach (var ovr in classSplashArtOverrides)
            {
                if (ovr.className.Equals(className, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (ovr.splashArt != null)
                    {
                        Debug.Log($"[CharacterDisplayController] Using override splash art for {className}");
                        return ovr.splashArt;
                    }
                }
            }
        }
        
        // Try to load from Resources
        if (!string.IsNullOrEmpty(classSplashArtResourcePath))
        {
            // Try common naming patterns
            string[] possibleNames = new string[]
            {
                $"{classSplashArtResourcePath}/{className}SplashArt",      // MarauderSplashArt
                $"{classSplashArtResourcePath}/{className}",                // Marauder
                $"{classSplashArtResourcePath}/Class{className}SplashArt",  // ClassMarauderSplashArt
                $"{classSplashArtResourcePath}/{className.ToLower()}",      // marauder
            };
            
            foreach (string path in possibleNames)
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    Debug.Log($"[CharacterDisplayController] Loaded class splash art from Resources: {path}");
                    return sprite;
                }
            }
            
            Debug.LogWarning($"[CharacterDisplayController] Could not find splash art in Resources. Tried paths:");
            foreach (string path in possibleNames)
            {
                Debug.LogWarning($"  - Resources/{path}");
            }
        }
        
        return null;
    }
    
    #endregion
    
    #region Ascendancy Display
    
    /// <summary>
    /// Display the 3 Ascendancy options for the selected class
    /// </summary>
    void DisplayAscendancies()
    {
        // Get Ascendancy database
        AscendancyDatabase ascendancyDB = AscendancyDatabase.Instance;
        
        if (ascendancyDB == null)
        {
            Debug.LogWarning("[CharacterDisplayController] AscendancyDatabase not found! Ascendancies will not be displayed.");
            return;
        }
        
        // Get Ascendancies for this class
        var ascendancies = ascendancyDB.GetAscendanciesForClass(selectedClass);
        
        if (ascendancies == null || ascendancies.Count == 0)
        {
            Debug.LogWarning($"[CharacterDisplayController] No Ascendancies found for class: {selectedClass}");
            
            // Hide all Ascendancy buttons
            if (ascendancy1Button != null) ascendancy1Button.SetActive(false);
            if (ascendancy2Button != null) ascendancy2Button.SetActive(false);
            if (ascendancy3Button != null) ascendancy3Button.SetActive(false);
            return;
        }
        
        Debug.Log($"[CharacterDisplayController] Displaying {ascendancies.Count} Ascendancies for {selectedClass}");
        
        // Assign Ascendancies to buttons
        GameObject[] ascendancyButtons = { ascendancy1Button, ascendancy2Button, ascendancy3Button };
        
        for (int i = 0; i < ascendancyButtons.Length; i++)
        {
            GameObject buttonObj = ascendancyButtons[i];
            
            if (buttonObj == null)
            {
                Debug.LogWarning($"[CharacterDisplayController] Ascendancy button {i + 1} is not assigned!");
                continue;
            }
            
            // If we have an Ascendancy for this slot
            if (i < ascendancies.Count && ascendancies[i] != null)
            {
                buttonObj.SetActive(true);
                
                // Get or add AscendancyButton component
                AscendancyButton ascButton = buttonObj.GetComponent<AscendancyButton>();
                if (ascButton == null)
                {
                    ascButton = buttonObj.AddComponent<AscendancyButton>();
                    Debug.Log($"[CharacterDisplayController] Added AscendancyButton component to {buttonObj.name}");
                }
                
                // Initialize with Ascendancy data
                // Locked = true initially (unlocked in-game)
                ascButton.Initialize(ascendancies[i], locked: true, lockReason: $"Unlocks at Level {ascendancies[i].requiredLevel}");
                
                // Subscribe to click event (for future: show detailed info panel)
                ascButton.OnAscendancyClicked += OnAscendancyClicked;
                
                Debug.Log($"[CharacterDisplayController] ✓ Displayed Ascendancy {i + 1}: {ascendancies[i].ascendancyName}");
            }
            else
            {
                // No Ascendancy for this slot, hide button
                buttonObj.SetActive(false);
                Debug.Log($"[CharacterDisplayController] Hid Ascendancy button {i + 1} (no data)");
            }
        }
    }
    
    /// <summary>
    /// Handle Ascendancy button click - Opens detailed tree panel
    /// </summary>
    void OnAscendancyClicked(AscendancyData ascendancy)
    {
        Debug.Log($"━━━ [CharacterDisplayController] Ascendancy clicked: {ascendancy.ascendancyName} ━━━");
        
        // Show detailed Ascendancy panel
        if (ascendancyDisplayPanel != null)
        {
            // Create progression (for character creation, they have 0 points)
            // In-game, this would be loaded from CharacterManager
            CharacterAscendancyProgress progress = new CharacterAscendancyProgress();
            // progress.AwardPoints(8); // Uncomment for testing unlocks
            
            ascendancyDisplayPanel.ShowAscendancy(ascendancy, progress);
            
            Debug.Log($"[CharacterDisplayController] ✓ Opened Ascendancy panel: {ascendancy.ascendancyName}");
        }
        else
        {
            Debug.LogWarning("[CharacterDisplayController] AscendancyDisplayPanel not assigned! Logging info to Console instead:");
            
            // Fallback: Log the info
            Debug.Log($"<color=cyan>Tagline:</color> {ascendancy.tagline}");
            Debug.Log($"<color=cyan>Description:</color> {ascendancy.description}");
            Debug.Log($"<color=cyan>Keywords:</color> {ascendancy.GetKeywordsString()}");
            
            Debug.Log($"\n<color=yellow>━━ Core Mechanic: {ascendancy.coreMechanicName} ━━</color>");
            Debug.Log(ascendancy.coreMechanicDescription);
            
            Debug.Log($"\n<color=lime>━━ Passive Abilities ({ascendancy.GetPassiveCount()}) ━━</color>");
            Debug.Log(ascendancy.GetPassivesSummary());
            
            if (ascendancy.signatureCard != null && !string.IsNullOrEmpty(ascendancy.signatureCard.cardName))
            {
                Debug.Log($"\n<color=magenta>━━ Signature Card ━━</color>");
                Debug.Log(ascendancy.signatureCard.GetCardInfo());
            }
            
            Debug.Log($"\n<color=orange>Unlock Requirements:</color> Level {ascendancy.requiredLevel}");
            Debug.Log($"<color=orange>Max Ascendancy Points:</color> {ascendancy.maxAscendancyPoints}");
        }
    }
    
    #endregion
    
    #region Card Hover Preview
    
    /// <summary>
    /// Show full card preview when hovering over a simplified card
    /// </summary>
    void OnCardHoverEnter(CardDataExtended cardData)
    {
        if (fullCardPreviewPrefab == null || fullCardPreviewContainer == null)
        {
            if (showHoverDebugLogs)
                Debug.LogWarning("[CharacterDisplayController] Full card preview prefab or container not assigned!");
            return;
        }
        
        // Hide previous preview if any
        OnCardHoverExit();
        
        // Instantiate full card preview
        currentPreviewCard = Instantiate(fullCardPreviewPrefab, fullCardPreviewContainer);
        currentPreviewCard.name = $"FullPreview_{cardData.cardName}";
        
        // Setup the card using DeckBuilderCardUI
        DeckBuilderCardUI previewUI = currentPreviewCard.GetComponent<DeckBuilderCardUI>();
        if (previewUI != null)
        {
            // Create a temporary character for stat display
            Character tempChar = new Character("Preview", selectedClass);
            previewUI.Initialize(cardData, null, tempChar);
            
            // Disable interaction on preview (it's just for display)
            previewUI.enabled = false;
            
            if (showHoverDebugLogs)
                Debug.Log($"[CharacterDisplayController] Showing full preview for: {cardData.cardName}");
        }
        else
        {
            Debug.LogWarning("[CharacterDisplayController] Full card preview prefab missing DeckBuilderCardUI component!");
        }
        
        // Setup category icon (Attack, Guard, Skill, Power icons)
        SetupCategoryIcon(currentPreviewCard, cardData);
        SetupComboDescription(currentPreviewCard, cardData);
        
        // Disable raycasting on preview to prevent blocking other UI
        var allGraphics = currentPreviewCard.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in allGraphics)
        {
            graphic.raycastTarget = false;
        }
        
        // Position and scale
        RectTransform previewRect = currentPreviewCard.GetComponent<RectTransform>();
        if (previewRect != null)
        {
            previewRect.anchoredPosition = Vector2.zero; // Center in container
            previewRect.localScale = Vector3.one * previewScale;
        }
        
        currentPreviewCard.SetActive(true);
    }
    
    /// <summary>
    /// Hide full card preview when mouse leaves card
    /// </summary>
    void OnCardHoverExit()
    {
        if (currentPreviewCard != null)
        {
            Destroy(currentPreviewCard);
            currentPreviewCard = null;
            
            if (showHoverDebugLogs)
                Debug.Log("[CharacterDisplayController] Hid card preview");
        }
    }
    
    /// <summary>
    /// Setup category icon on a card preview
    /// </summary>
    void SetupCategoryIcon(GameObject cardObj, CardDataExtended cardData)
    {
        if (cardObj == null || cardData == null) return;
        
        // Search for category icon by name
        Image targetIcon = null;
        var images = cardObj.GetComponentsInChildren<Image>(true);
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
            if (showHoverDebugLogs)
                Debug.LogWarning($"[CharacterDisplayController] CategoryIcon not found on {cardObj.name}");
            return;
        }
        
        // Load CardVisualAssets
        var visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
        if (visualAssets == null)
        {
            Debug.LogWarning("[CharacterDisplayController] CardVisualAssets not found in Resources");
            return;
        }
        
        // Get category sprite
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
            
            if (showHoverDebugLogs)
                Debug.Log($"[CharacterDisplayController] ✓ Set category icon for {cardData.cardName}: {cardData.category}");
        }
        else
        {
            targetIcon.gameObject.SetActive(false);
        }
    }
    
    void SetupComboDescription(GameObject cardObj, CardDataExtended cardData)
    {
        if (cardObj == null || cardData == null) return;

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
            if (showHoverDebugLogs)
                Debug.LogWarning($"[CharacterDisplayController] AdditionalEffectText not found on {cardObj.name}");
            return;
        }

        Character tempChar = new Character("Preview", selectedClass);
        string dynamicCombo = cardData.GetDynamicComboDescription(tempChar);
        string comboText = string.IsNullOrEmpty(dynamicCombo) ? cardData.comboDescription : dynamicCombo;

        bool hasText = !string.IsNullOrWhiteSpace(comboText);
        additionalEffectText.text = hasText ? comboText : string.Empty;
        additionalEffectText.gameObject.SetActive(hasText);

        if (showHoverDebugLogs)
        {
            Debug.Log(hasText
                ? $"[CharacterDisplayController] AdditionalEffectText set to: {comboText}"
                : "[CharacterDisplayController] AdditionalEffectText hidden (empty)");
        }
    }

    #endregion
    
    void OnDestroy()
    {
        // Clean up hover preview
        if (currentPreviewCard != null)
            Destroy(currentPreviewCard);
        
        // Clean up hover event listeners
        foreach (GameObject cardObj in spawnedCards)
        {
            if (cardObj != null)
            {
                CharacterScreenCardHover hoverComponent = cardObj.GetComponent<CharacterScreenCardHover>();
                if (hoverComponent != null)
                {
                    hoverComponent.OnCardHoverEnter -= OnCardHoverEnter;
                    hoverComponent.OnCardHoverExit -= OnCardHoverExit;
                }
            }
        }
        
        // Clean up button listeners
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
        
        if (characterNameInput != null)
            characterNameInput.onValueChanged.RemoveListener(OnNameChanged);
    }
}

/// <summary>
/// Manual override for class splash art
/// </summary>
[System.Serializable]
public class ClassSplashArtOverride
{
    public string className;
    public Sprite splashArt;
}

/// <summary>
/// Optional: ScriptableObject to define class information
/// </summary>
[System.Serializable]
public class ClassInfoScriptableObject : ScriptableObject
{
    public string className;
    [TextArea(3, 5)]
    public string description;
    public Sprite classIcon;
}

/// <summary>
/// Helper struct for starting attributes display
/// </summary>
[System.Serializable]
public struct StartingAttributes
{
    public int strength;
    public int dexterity;
    public int intelligence;
    public int health;
    public int mana;
    public int reliance;
}

