using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Card carousel for embossing screen
/// Displays deck cards in a smooth, snapping carousel
/// One card centered, others at edges
/// </summary>
public class CardCarouselUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Transform cardContainer; // Content transform in ScrollRect
    [SerializeField] private GameObject cardPrefab; // Prefab for card display
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private ScrollRect scrollRect;
    
    [Header("Carousel Settings")]
    [SerializeField] private float cardSpacing = 150f; // Space between cards (150-175 for multi-card view)
    [SerializeField] private float centerCardScale = 1f; // Scale of centered card
    [SerializeField] private float firstSideCardScale = 0.9f; // Scale of first card on each side (0.9)
    [SerializeField] private float secondSideCardScale = 0.8f; // Scale of second card on each side (0.8)
    [SerializeField] private float snapSpeed = 0.2f; // Time to snap to card (0.2 for snappy feel)
    [SerializeField] private float swipeThreshold = 25f; // Minimum drag distance to trigger swipe (25-35 recommended)
    
    [Header("Visual Effects")]
    [SerializeField] private float sideCardAlpha = 0.6f; // Alpha of side cards
    [SerializeField] private Color centerCardHighlight = Color.white;
    [SerializeField] private Color sideCardColor = new Color(0.7f, 0.7f, 0.7f);
    
    [Header("Testing")]
    [SerializeField] private bool useTestCards = false; // Enable test cards when DeckManager not available (disabled by default)
    [SerializeField] private int testCardCount = 5; // Number of test cards to generate
    
    [Header("Selection Header")]
    [SerializeField] private TextMeshProUGUI selectedCardNameText; // Text in EmbossingBackground/Header/CardName
    
    private List<CardDisplay> cardDisplays = new List<CardDisplay>();
    private List<Card> deckCards = new List<Card>();
    private Dictionary<string, int> cardCounts = new Dictionary<string, int>(); // Track how many copies of each card
    private int currentCardIndex = 0;
    private bool isSnapping = false;
    private Vector2 dragStartPos;
    private float[] cardPositions; // X positions of each card for snapping
    private bool isButtonClick = false; // Prevent drag detection during button clicks
    
    void Start()
    {
        // Setup buttons
        if (previousButton != null)
            previousButton.onClick.AddListener(() => NavigateCard(-1));
        
        if (nextButton != null)
            nextButton.onClick.AddListener(() => NavigateCard(1));
        
        // Auto-find selected card name text if not assigned
        if (selectedCardNameText == null)
        {
            GameObject embossingBg = GameObject.Find("EmbossingBackground");
            if (embossingBg != null)
            {
                Transform header = embossingBg.transform.Find("Header");
                if (header != null)
                {
                    Transform cardNameTransform = header.Find("CardName");
                    if (cardNameTransform != null)
                    {
                        selectedCardNameText = cardNameTransform.GetComponent<TextMeshProUGUI>();
                        Debug.Log("[CardCarousel] Auto-assigned selectedCardNameText from EmbossingBackground/Header/CardName");
                    }
                }
            }
        }
        
        // Load deck
        LoadDeckCards();
        
        // Initial snap will happen after position calculation completes
        // (see DelayedCalculatePositions coroutine)
    }
    
    /// <summary>
    /// Load cards from DeckManager
    /// </summary>
    void LoadDeckCards()
    {
        List<Card> allCards = new List<Card>();
        
        // Use singleton instance if available
        if (DeckManager.Instance != null)
        {
            // Try to get active deck
            DeckPreset activeDeck = DeckManager.Instance.GetActiveDeck();
            
            if (activeDeck == null)
            {
                // Try to load from character's active deck name
                Debug.Log("[CardCarousel] No active deck in DeckManager, trying to load from character...");
                
                Character character = CharacterManager.Instance?.GetCurrentCharacter();
                if (character != null && character.deckData != null && !string.IsNullOrEmpty(character.deckData.activeDeckName))
                {
                    // Load character's decks
                    CharacterManager.Instance.LoadCharacterDecks(character);
                    
                    // Try again after loading
                    activeDeck = DeckManager.Instance.GetActiveDeck();
                }
            }
            
            if (activeDeck != null)
            {
                // Get cards from deck preset
                allCards = DeckManager.Instance.GetActiveDeckAsCards();
                Debug.Log($"[CardCarousel] ✓ Loaded {allCards.Count} total cards from active deck '{activeDeck.deckName}'");
            }
            else
            {
                Debug.LogWarning("[CardCarousel] No active deck available after loading attempt!");
                
                // Use test cards if enabled
                if (useTestCards)
                {
                    allCards = CreateTestCards();
                    Debug.Log($"[CardCarousel] ⚠️ Using {allCards.Count} TEST cards (no active deck)");
                }
                else
                {
                    allCards = new List<Card>();
                }
            }
        }
        else
        {
            Debug.LogWarning("[CardCarousel] No DeckManager instance available!");
            
            // Use test cards if enabled
            if (useTestCards)
            {
                allCards = CreateTestCards();
                Debug.Log($"[CardCarousel] ⚠️ Using {allCards.Count} TEST cards (DeckManager not found)");
            }
            else
            {
                allCards = new List<Card>();
            }
        }
        
        // Filter to unique cards by cardName (GroupKey)
        // This ensures embossing affects all copies of the same card
        deckCards = GetUniqueCards(allCards);
        Debug.Log($"[CardCarousel] Displaying {deckCards.Count} unique cards (grouped by name)");
        
        CreateCardDisplays();
    }
    
    /// <summary>
    /// Get unique cards grouped by groupKey.
    /// Returns one card per unique groupKey, preserving card data.
    /// Also populates cardCounts dictionary for embossing system.
    /// </summary>
    List<Card> GetUniqueCards(List<Card> allCards)
    {
        Dictionary<string, Card> uniqueCards = new Dictionary<string, Card>();
        cardCounts.Clear();
        
        foreach (Card card in allCards)
        {
            if (card == null || string.IsNullOrEmpty(card.cardName)) continue;
            
            // Use groupKey (falls back to cardName if groupKey is empty)
            string key = card.GetGroupKey();
            
            if (!uniqueCards.ContainsKey(key))
            {
                uniqueCards[key] = card;
                cardCounts[key] = 1;
            }
            else
            {
                // Increment count for duplicate cards
                cardCounts[key]++;
            }
        }
        
        return new List<Card>(uniqueCards.Values);
    }
    
    /// <summary>
    /// Get the number of copies of a card in the deck by groupKey.
    /// Used by embossing system to know how many cards will be affected.
    /// </summary>
    public int GetCardCount(string groupKey)
    {
        return cardCounts.ContainsKey(groupKey) ? cardCounts[groupKey] : 0;
    }
    
    /// <summary>
    /// Get the number of copies of a card by its Card object.
    /// </summary>
    public int GetCardCountByCard(Card card)
    {
        return card != null ? GetCardCount(card.GetGroupKey()) : 0;
    }
    
    /// <summary>
    /// Get the number of copies of the currently selected card.
    /// </summary>
    public int GetSelectedCardCount()
    {
        Card selectedCard = GetSelectedCard();
        return selectedCard != null ? GetCardCountByCard(selectedCard) : 0;
    }
    
    /// <summary>
    /// Get the groupKey of the currently selected card.
    /// Used by embossing system to identify which card group to modify.
    /// </summary>
    public string GetSelectedCardGroupKey()
    {
        Card selectedCard = GetSelectedCard();
        return selectedCard != null ? selectedCard.GetGroupKey() : "";
    }
    
    /// <summary>
    /// Get the current card index
    /// </summary>
    public int GetCurrentCardIndex()
    {
        return currentCardIndex;
    }
    
    /// <summary>
    /// Get card at specific index
    /// </summary>
    public Card GetCardAtIndex(int index)
    {
        if (index < 0 || index >= deckCards.Count)
            return null;
        
        return deckCards[index];
    }
    
    /// <summary>
    /// Refresh the current card display (for embossing slot updates)
    /// </summary>
    public void RefreshCurrentCard()
    {
        if (currentCardIndex < 0 || currentCardIndex >= cardDisplays.Count)
            return;
        
        CardDisplay display = cardDisplays[currentCardIndex];
        Card card = deckCards[currentCardIndex];
        
        if (display != null && card != null)
        {
            display.SetCard(card);
            Debug.Log($"[CardCarousel] Refreshed card display for: {card.cardName}");
        }
    }

    /// <summary>
    /// Reload deck data from DeckManager and preserve the current selection if possible.
    /// </summary>
    public void ReloadDeckPreservingSelection()
    {
        string selectedGroupKey = GetSelectedCardGroupKey();
        int previousIndex = currentCardIndex;

        LoadDeckCards();

        if (deckCards.Count == 0)
        {
            return;
        }

        int targetIndex = Mathf.Clamp(previousIndex, 0, deckCards.Count - 1);

        if (!string.IsNullOrEmpty(selectedGroupKey))
        {
            for (int i = 0; i < deckCards.Count; i++)
            {
                if (string.Equals(deckCards[i].GetGroupKey(), selectedGroupKey, System.StringComparison.OrdinalIgnoreCase))
                {
                    targetIndex = i;
                    break;
                }
            }
        }

        currentCardIndex = Mathf.Clamp(targetIndex, 0, deckCards.Count - 1);
        SnapToCard(currentCardIndex, false);
    }
    
    /// <summary>
    /// Create test cards for development/testing
    /// </summary>
    List<Card> CreateTestCards()
    {
        List<Card> testCards = new List<Card>();
        
        string[] testNames = { 
            "Flame Strike", 
            "Ice Barrier", 
            "Thunder Bolt", 
            "Shadow Step", 
            "Holy Light",
            "Poison Dart",
            "Earth Shield",
            "Wind Slash"
        };
        
        string[] testDescriptions = {
            "Deals fire damage to target",
            "Creates a protective ice shield",
            "Strikes with lightning",
            "Teleport through shadows",
            "Heals and protects allies",
            "Poisons the enemy",
            "Summons earth defense",
            "Cuts with wind blades"
        };
        
        for (int i = 0; i < Mathf.Min(testCardCount, testNames.Length); i++)
        {
            Card testCard = new Card
            {
                cardName = testNames[i],
                description = testDescriptions[i],
                cardType = CardType.Power,
                manaCost = 2,
                baseDamage = 10f,
                baseGuard = 0f
                // cardArt will be null - CardDisplay will use default sprite
            };
            
            testCards.Add(testCard);
        }
        
        return testCards;
    }
    
    /// <summary>
    /// Setup CardDisplay component references for CardPrefab_combat
    /// </summary>
    void SetupCardDisplayReferences(CardDisplay cardDisplay, GameObject cardObj)
    {
        // Find VisualRoot (the parent of all card elements)
        Transform visualRoot = cardObj.transform.Find("VisualRoot ");
        if (visualRoot == null)
        {
            visualRoot = cardObj.transform.Find("VisualRoot");
        }
        if (visualRoot == null)
        {
            Debug.LogWarning("[CardCarousel] Could not find VisualRoot in card prefab");
            return;
        }
        
        // Use reflection to set private fields since CardDisplay has serialized private fields
        var cardDisplayType = typeof(CardDisplay);
        var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        
        // Find and assign cardImage
        Transform cardImageTransform = visualRoot.Find("CardImage");
        if (cardImageTransform != null)
        {
            var cardImageField = cardDisplayType.GetField("cardImage", bindingFlags);
            cardImageField?.SetValue(cardDisplay, cardImageTransform.GetComponent<UnityEngine.UI.Image>());
        }
        
        // Find and assign categoryIcon
        Transform categoryIconTransform = visualRoot.Find("CategoryIcon");
        if (categoryIconTransform != null)
        {
            var categoryIconField = cardDisplayType.GetField("categoryIcon", bindingFlags);
            categoryIconField?.SetValue(cardDisplay, categoryIconTransform.GetComponent<UnityEngine.UI.Image>());
        }
        
        // Find and assign cardNameText
        Transform cardNameTransform = visualRoot.Find("CardName");
        if (cardNameTransform != null)
        {
            var cardNameField = cardDisplayType.GetField("cardNameText", bindingFlags);
            cardNameField?.SetValue(cardDisplay, cardNameTransform.GetComponent<TMPro.TextMeshProUGUI>());
        }
        
        // Find and assign cardDescriptionText
        Transform descriptionTransform = visualRoot.Find("DescriptionText");
        if (descriptionTransform != null)
        {
            var descriptionField = cardDisplayType.GetField("cardDescriptionText", bindingFlags);
            descriptionField?.SetValue(cardDisplay, descriptionTransform.GetComponent<TMPro.TextMeshProUGUI>());
        }
        
        // Find and assign additionalEffectText
        Transform additionalEffectTransform = visualRoot.Find("AdditionalEffectText");
        if (additionalEffectTransform != null)
        {
            var additionalEffectField = cardDisplayType.GetField("additionalEffectText", bindingFlags);
            additionalEffectField?.SetValue(cardDisplay, additionalEffectTransform.GetComponent<TMPro.TextMeshProUGUI>());
        }
        
        // Find and assign cardLevelText (optional - in CardLevelContainer)
        Transform levelContainer = visualRoot.Find("CardLevelContainer");
        if (levelContainer != null)
        {
            Transform levelTextTransform = levelContainer.Find("Text (TMP)");
            if (levelTextTransform != null)
            {
                var cardLevelField = cardDisplayType.GetField("cardLevelText", bindingFlags);
                cardLevelField?.SetValue(cardDisplay, levelTextTransform.GetComponent<TMPro.TextMeshProUGUI>());
            }
        }
        
        // Find and assign cardXPSlider (optional - in separate CardXpSlider container)
        Transform sliderContainer = visualRoot.Find("CardXpSlider");
        if (sliderContainer != null)
        {
            // Try to get Slider component from container itself
            UnityEngine.UI.Slider slider = sliderContainer.GetComponent<UnityEngine.UI.Slider>();
            if (slider != null)
            {
                var sliderField = cardDisplayType.GetField("cardXPSlider", bindingFlags);
                sliderField?.SetValue(cardDisplay, slider);
            }
            else
            {
                // Try to find Slider child
                Transform sliderChild = sliderContainer.Find("Slider");
                if (sliderChild != null)
                {
                    var sliderField = cardDisplayType.GetField("cardXPSlider", bindingFlags);
                    sliderField?.SetValue(cardDisplay, sliderChild.GetComponent<UnityEngine.UI.Slider>());
                }
            }
        }
        
        // Find and assign embossingSlotContainer
        Transform embossingContainer = visualRoot.Find("EmbossingSlots");
        if (embossingContainer != null)
        {
            var embossingField = cardDisplayType.GetField("embossingSlotContainer", bindingFlags);
            embossingField?.SetValue(cardDisplay, embossingContainer);

            var configureMethod = cardDisplayType.GetMethod("ConfigureEmbossingSlot", bindingFlags);
            if (configureMethod != null)
            {
                for (int slotIndex = 0; slotIndex < 5; slotIndex++)
                {
                    Transform slotContainer = embossingContainer.Find($"Slot{slotIndex + 1}Container");
                    if (slotContainer == null)
                    {
                        continue;
                    }

                    Transform emptySlot = slotContainer.Find($"Slot{slotIndex + 1}Embossing");
                    Transform filledSlot = slotContainer.Find($"Slot{slotIndex + 1}Filled");

                    Image emptyImage = emptySlot != null ? emptySlot.GetComponent<Image>() : null;
                    Image filledImage = filledSlot != null ? filledSlot.GetComponent<Image>() : null;
                    Image iconImage = null;

                    if (filledSlot != null)
                    {
                        string[] candidateNames = { "EmbossingIcon", "Icon" };
                        for (int i = 0; i < candidateNames.Length; i++)
                        {
                            Transform iconChild = filledSlot.Find(candidateNames[i]);
                            if (iconChild != null)
                            {
                                iconImage = iconChild.GetComponent<Image>();
                                if (iconImage != null)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    configureMethod.Invoke(cardDisplay, new object[]
                    {
                        slotIndex,
                        slotContainer,
                        emptyImage,
                        filledImage,
                        iconImage
                    });
                }
            }
        }

        var visualRootFieldInfo = cardDisplayType.GetField("visualRoot", bindingFlags);
        if (visualRootFieldInfo != null)
        {
            visualRootFieldInfo.SetValue(cardDisplay, visualRoot as RectTransform);
        }
        
        // Load visual assets from Resources
        var visualAssets = Resources.Load<CardVisualAssets>("CardVisualAssets");
        if (visualAssets != null)
        {
            var visualAssetsField = cardDisplayType.GetField("visualAssets", bindingFlags);
            visualAssetsField?.SetValue(cardDisplay, visualAssets);
        }
        
        Debug.Log($"[CardCarousel] Configured CardDisplay for {cardObj.name}");
    }
    
    /// <summary>
    /// Create visual displays for all cards
    /// </summary>
    void CreateCardDisplays()
    {
        // Clear existing
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        cardDisplays.Clear();
        
        if (deckCards.Count == 0)
        {
            Debug.LogWarning("[CardCarousel] No cards to display!");
            return;
        }
        
        // Setup horizontal layout
        HorizontalLayoutGroup layoutGroup = cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = cardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        
        layoutGroup.spacing = cardSpacing;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        
        // Add padding to center first card
        float viewportWidth = scrollRect.viewport.rect.width;
        float cardWidth = 200f; // Approximate card width (adjust based on your prefab)
        float sidePadding = (viewportWidth - cardWidth) / 2f;
        layoutGroup.padding = new RectOffset((int)sidePadding, (int)sidePadding, 0, 0);
        
        // Create card displays
        cardPositions = new float[deckCards.Count];
        
        for (int i = 0; i < deckCards.Count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            cardObj.name = $"Card_{i}_{deckCards[i].cardName}";
            
            CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
            if (cardDisplay == null)
            {
                cardDisplay = cardObj.AddComponent<CardDisplay>();
            }

            // Auto-wire the references from CardPrefab_combat's structure
            SetupCardDisplayReferences(cardDisplay, cardObj);
            
            cardDisplay.SetCard(deckCards[i]);
            cardDisplays.Add(cardDisplay);
            
            // Add click listener to select this card
            int cardIndex = i; // Capture index for closure
            UnityEngine.UI.Button cardButton = cardObj.GetComponent<UnityEngine.UI.Button>();
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(() => OnCardClicked(cardIndex));
            }
            
            // Calculate position for snapping (will be updated after layout)
            Canvas.ForceUpdateCanvases();
        }
        
        // Force layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer.GetComponent<RectTransform>());
        
        // Update visuals first (apply scaling)
        UpdateCardVisuals();
        
        // Calculate positions after a frame to ensure layout is final
        StartCoroutine(DelayedCalculatePositions());
        
        Debug.Log($"[CardCarousel] Created {cardDisplays.Count} card displays");
    }
    
    /// <summary>
    /// Delay position calculation to ensure layout is finalized
    /// </summary>
    IEnumerator DelayedCalculatePositions()
    {
        // Wait for end of frame to ensure layout is complete
        yield return new WaitForEndOfFrame();
        
        // Force another canvas update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer.GetComponent<RectTransform>());
        
        // Now calculate positions
        CalculateCardPositions();
        
        Debug.Log($"[CardCarousel] Card positions calculated and ready");
        
        // Now snap to first card (positions are properly calculated)
        if (cardDisplays.Count > 0)
        {
            SnapToCard(0, false); // Instant snap to first card
            Debug.Log($"[CardCarousel] Initial snap to first card completed");
        }
    }
    
    /// <summary>
    /// Calculate snap positions for each card (accounting for proper centering)
    /// </summary>
    void CalculateCardPositions()
    {
        if (cardDisplays.Count == 0) return;
        
        RectTransform contentRect = scrollRect.content;
        float viewportWidth = scrollRect.viewport.rect.width;
        
        for (int i = 0; i < cardDisplays.Count; i++)
        {
            RectTransform cardRect = cardDisplays[i].GetComponent<RectTransform>();
            
            // Get the card's position in the content
            float cardLocalX = cardRect.anchoredPosition.x;
            
            // Calculate the content position needed to center this card in the viewport
            // We want the card's center to align with the viewport's center
            float targetContentX = -cardLocalX + (viewportWidth / 2f);
            
            // Clamp to valid scroll range
            float contentWidth = contentRect.rect.width;
            float minContentX = -(contentWidth - viewportWidth);
            float maxContentX = 0;
            targetContentX = Mathf.Clamp(targetContentX, minContentX, maxContentX);
            
            // Convert to normalized position (0 to 1)
            if (contentWidth > viewportWidth)
            {
                cardPositions[i] = Mathf.Clamp01(-targetContentX / (contentWidth - viewportWidth));
            }
            else
            {
                cardPositions[i] = 0f;
            }
        }
    }
    
    /// <summary>
    /// Navigate to previous/next card
    /// </summary>
    public void NavigateCard(int direction)
    {
        if (isSnapping || cardDisplays.Count == 0) return;
        
        Debug.Log($"[CardCarousel] NavigateCard called: direction={direction}, currentIndex={currentCardIndex}");
        
        int newIndex = currentCardIndex + direction;
        newIndex = Mathf.Clamp(newIndex, 0, cardDisplays.Count - 1);
        
        Debug.Log($"[CardCarousel] Navigating from {currentCardIndex} to {newIndex}");
        
        if (newIndex != currentCardIndex)
        {
            isButtonClick = true; // Flag to prevent drag interference
            SnapToCard(newIndex, true);
        }
    }
    
    /// <summary>
    /// Snap to a specific card
    /// </summary>
    void SnapToCard(int index, bool animated)
    {
        if (index < 0 || index >= cardDisplays.Count) return;
        
        currentCardIndex = index;
        float targetPosition = cardPositions[index];
        
        if (animated)
        {
            StartCoroutine(SnapToCardCoroutine(targetPosition));
        }
        else
        {
            scrollRect.horizontalNormalizedPosition = targetPosition;
            UpdateCardVisuals();
            OnCardSelected(currentCardIndex);
        }
        
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Coroutine for smooth snapping animation
    /// </summary>
    IEnumerator SnapToCardCoroutine(float targetPosition)
    {
        isSnapping = true;
        float startPosition = scrollRect.horizontalNormalizedPosition;
        float elapsed = 0f;
        
        while (elapsed < snapSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapSpeed;
            
            // Ease out cubic curve for smooth deceleration
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, t);
            UpdateCardVisuals();
            
            yield return null;
        }
        
        scrollRect.horizontalNormalizedPosition = targetPosition;
        UpdateCardVisuals();
        isSnapping = false;
        isButtonClick = false; // Clear button click flag
        OnCardSelected(currentCardIndex);
    }
    
    /// <summary>
    /// Update button enabled states
    /// </summary>
    void UpdateButtonStates()
    {
        if (previousButton != null)
            previousButton.interactable = currentCardIndex > 0;
        
        if (nextButton != null)
            nextButton.interactable = currentCardIndex < cardDisplays.Count - 1;
    }
    
    /// <summary>
    /// Update visual appearance of all cards based on position
    /// </summary>
    void UpdateCardVisuals()
    {
        if (cardDisplays.Count == 0) return;
        
        for (int i = 0; i < cardDisplays.Count; i++)
        {
            RectTransform cardRect = cardDisplays[i].GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.localScale = Vector3.one;
            }
            
            // Calculate offset from center card (in card positions)
            int offsetFromCenter = Mathf.Abs(i - currentCardIndex);
            
            // Apply progressive scaling based on position
            float scale = centerCardScale;
            float alpha = 1f;
            
            if (offsetFromCenter == 0)
            {
                // Center card
                scale = centerCardScale;
                alpha = 1f;
            }
            else if (offsetFromCenter == 1)
            {
                // First card on each side
                scale = firstSideCardScale;
                alpha = 0.8f; // Slightly less opaque than center
            }
            else if (offsetFromCenter == 2)
            {
                // Second card on each side
                scale = secondSideCardScale;
                alpha = sideCardAlpha; // Use configured side alpha
            }
            else
            {
                // Cards beyond second position (fade out more)
                scale = secondSideCardScale * 0.95f; // Slightly smaller
                alpha = sideCardAlpha * 0.8f; // Even more faded
            }
            
            // Apply scale via CardDisplay root
            cardDisplays[i].ApplyScale(scale);
            
            // Apply alpha to card's CanvasGroup
            CanvasGroup canvasGroup = cardDisplays[i].GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = cardDisplays[i].gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = alpha;
            
            // Apply color tint based on offset
            Image cardImage = cardDisplays[i].GetComponent<Image>();
            if (cardImage != null)
            {
                float tintLerp = Mathf.Clamp01(offsetFromCenter / 2f); // 0-1 based on 0-2 offset
                cardImage.color = Color.Lerp(centerCardHighlight, sideCardColor, tintLerp);
            }
        }
    }
    
    /// <summary>
    /// Called when a card is selected (centered)
    /// </summary>
    void OnCardSelected(int index)
    {
        if (index < 0 || index >= deckCards.Count) return;
        
        Card selectedCard = deckCards[index];
        string groupKey = selectedCard.GetGroupKey();
        int copyCount = GetCardCount(groupKey);
        
        Debug.Log($"[CardCarousel] Card selected: {selectedCard.cardName} (GroupKey: {groupKey})");
        Debug.Log($"[CardCarousel] → Embossing this card will affect {copyCount} {(copyCount == 1 ? "copy" : "copies")}");
        
        // Update header with selected card name
        UpdateSelectedCardHeader(selectedCard, copyCount);
        
        // TODO: Notify embossing UI that this card is selected
        // You can add an event here like: OnCardSelected?.Invoke(selectedCard, groupKey, copyCount);
    }
    
    /// <summary>
    /// Called when a card is clicked (user interaction)
    /// </summary>
    void OnCardClicked(int index)
    {
        if (index < 0 || index >= deckCards.Count) return;
        
        Debug.Log($"[CardCarousel] Card clicked: {deckCards[index].cardName} at index {index}");
        
        // Snap to the clicked card
        SnapToCard(index, true);
    }
    
    /// <summary>
    /// Update the header text with selected card information
    /// </summary>
    void UpdateSelectedCardHeader(Card card, int copyCount)
    {
        if (selectedCardNameText == null || card == null) return;
        
        // Display card name with copy count
        string displayText = $"{card.cardName}";
        
        // Add level if card is leveled
        if (card.cardLevel > 1)
        {
            displayText += $" (Lv. {card.cardLevel})";
        }
        
        // Add copy count
        if (copyCount > 1)
        {
            displayText += $" - x{copyCount} in deck";
        }
        
        selectedCardNameText.text = displayText;
        
        Debug.Log($"[CardCarousel] Updated header: {displayText}");
    }
    
    // Drag/Swipe handlers
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSnapping || isButtonClick) return;
        dragStartPos = eventData.position;
        Debug.Log($"[CardCarousel] OnBeginDrag at {dragStartPos}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isSnapping || isButtonClick) return;
        UpdateCardVisuals();
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isSnapping || isButtonClick) 
        {
            Debug.Log($"[CardCarousel] OnEndDrag ignored (isSnapping={isSnapping}, isButtonClick={isButtonClick})");
            return;
        }
        
        Vector2 dragDelta = eventData.position - dragStartPos;
        Debug.Log($"[CardCarousel] OnEndDrag - dragDelta: {dragDelta.x}");
        
        // Detect swipe
        if (Mathf.Abs(dragDelta.x) > swipeThreshold)
        {
            if (dragDelta.x > 0)
            {
                // Swiped right = previous card
                Debug.Log("[CardCarousel] Swipe RIGHT detected - navigating to previous card");
                NavigateCard(-1);
            }
            else
            {
                // Swiped left = next card
                Debug.Log("[CardCarousel] Swipe LEFT detected - navigating to next card");
                NavigateCard(1);
            }
        }
        else
        {
            // Small drag = snap to nearest
            Debug.Log("[CardCarousel] Small drag - snapping to nearest card");
            SnapToNearestCard();
        }
    }
    
    /// <summary>
    /// Snap to the nearest card based on current scroll position
    /// </summary>
    void SnapToNearestCard()
    {
        if (isButtonClick)
        {
            Debug.Log("[CardCarousel] SnapToNearestCard ignored - button click in progress");
            return;
        }
        
        float currentPos = scrollRect.horizontalNormalizedPosition;
        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;
        
        for (int i = 0; i < cardPositions.Length; i++)
        {
            float distance = Mathf.Abs(cardPositions[i] - currentPos);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        Debug.Log($"[CardCarousel] SnapToNearestCard - current: {currentCardIndex}, nearest: {nearestIndex}");
        SnapToCard(nearestIndex, true);
    }
    
    void Update()
    {
        // Continuous visual updates while scrolling
        if (!isSnapping && cardDisplays.Count > 0)
        {
            UpdateCardVisuals();
        }
    }
    
    /// <summary>
    /// Get the currently selected card
    /// </summary>
    public Card GetSelectedCard()
    {
        if (currentCardIndex >= 0 && currentCardIndex < deckCards.Count)
        {
            return deckCards[currentCardIndex];
        }
        return null;
    }
    
    /// <summary>
    /// Refresh the carousel (call when deck changes)
    /// </summary>
    public void RefreshCarousel()
    {
        LoadDeckCards();
        if (cardDisplays.Count > 0)
        {
            SnapToCard(0, false);
        }
    }
}

