using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleCombatUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas combatCanvas;
    public Transform cardHandParent;
    public Text deckCountText;
    public Text discardCountText;
    public Button drawCardButton;
    public Button endTurnButton;
    
    [Header("Card Display")]
    public GameObject cardPrefab;
    public Vector2 cardSize = new Vector2(120, 180);
    public float cardSpacing = 10f;
    public int maxCardsInHand = 5;
    
    [Header("Card Scaling")]
    [Range(0.1f, 5f)]
    public float scaleX = 1f;
    [Range(0.1f, 5f)]
    public float scaleY = 1f;
    [SerializeField] private bool autoUpdateScaling = true;
    
    [Header("Test Configuration")]
    public bool loadTestCardsOnStart = true;
    public bool shuffleDeckOnStart = true;
    
    // Card collections
    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    
    // Card instances in hand
    private List<GameObject> cardInstances = new List<GameObject>();
    
    private void Start()
    {
        InitializeUI();
        
        if (loadTestCardsOnStart)
        {
            LoadTestDeck();
        }
        
        if (shuffleDeckOnStart)
        {
            ShuffleDeck();
        }
        
        // Draw initial hand
        DrawInitialHand();
    }
    
    // Called when values are changed in the inspector
    private void OnValidate()
    {
        if (autoUpdateScaling && Application.isPlaying)
        {
            // Update cardSize based on scale values
            cardSize = new Vector2(120f * scaleX, 180f * scaleY);
            
            // Apply scaling to existing cards if any
            if (cardInstances.Count > 0)
            {
                ApplyScalingToAllCards();
            }
        }
    }
    
    private void InitializeUI()
    {
        // Auto-find components if not assigned
        if (combatCanvas == null)
            combatCanvas = FindFirstObjectByType<Canvas>();
            
        if (cardHandParent == null)
        {
            GameObject handParent = new GameObject("CardHandParent");
            handParent.transform.SetParent(combatCanvas.transform);
            handParent.transform.localPosition = Vector3.zero;
            cardHandParent = handParent.transform;
        }
        
        // Load card prefab if not assigned
        if (cardPrefab == null)
        {
            cardPrefab = Resources.Load<GameObject>("CardPrefab");
            if (cardPrefab == null)
            {
                Debug.LogWarning("CardPrefab not found in Resources folder! Using fallback card creation.");
            }
            else
            {
                Debug.Log("CardPrefab loaded successfully from Resources.");
            }
        }
        
        // Set up button events
        if (drawCardButton != null)
        {
            drawCardButton.onClick.AddListener(OnDrawCardClicked);
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }
    }
    
    private void LoadTestDeck()
    {
        deck.Clear();
        
        // Always create test cards for development/testing
        Debug.Log("Creating test cards for development...");
        CreateTestCards();
        
        UpdateDeckCount();
    }
    
    private void CreateTestCards()
    {
        // Create test cards with multiple copies for a proper deck
        CardData strike = CreateTestCard("Strike", "Attack", 1, "Deal 6 damage.", CardRarity.Common, CardElement.Basic, CardCategory.Attack, 6, 0);
        CardData defend = CreateTestCard("Defend", "Skill", 1, "Gain 5 block.", CardRarity.Common, CardElement.Basic, CardCategory.Guard, 0, 5);
        CardData fireball = CreateTestCard("Fireball", "Attack", 2, "Deal 12 fire damage.", CardRarity.Magic, CardElement.Fire, CardCategory.Attack, 12, 0);
        CardData iceShield = CreateTestCard("Ice Shield", "Skill", 1, "Gain 8 block.", CardRarity.Magic, CardElement.Cold, CardCategory.Guard, 0, 8);
        CardData lightningBolt = CreateTestCard("Lightning Bolt", "Attack", 1, "Deal 8 lightning damage.", CardRarity.Magic, CardElement.Lightning, CardCategory.Attack, 8, 0);
        
        // Add multiple copies of each card to create a proper deck
        for (int i = 0; i < 3; i++) deck.Add(strike);    // 3 copies
        for (int i = 0; i < 3; i++) deck.Add(defend);    // 3 copies
        for (int i = 0; i < 2; i++) deck.Add(fireball);  // 2 copies
        for (int i = 0; i < 2; i++) deck.Add(iceShield); // 2 copies
        for (int i = 0; i < 2; i++) deck.Add(lightningBolt); // 2 copies
        
        Debug.Log($"Created {deck.Count} test cards (12 total cards in deck)");
    }
    
    private CardData CreateTestCard(string name, string type, int cost, string description, CardRarity rarity, CardElement element, CardCategory category, int damage, int block)
    {
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.cardName = name;
        cardData.cardType = type;
        cardData.playCost = cost;
        cardData.description = description;
        cardData.rarity = rarity;
        cardData.element = element;
        cardData.category = category;
        cardData.damage = damage;
        cardData.block = block;
        return cardData;
    }
    
    private void ShuffleDeck()
    {
        // Fisher-Yates shuffle
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardData temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
        
        Debug.Log("Deck shuffled");
    }
    
    private void DrawInitialHand()
    {
        // Draw initial cards
        for (int i = 0; i < Mathf.Min(3, maxCardsInHand); i++)
        {
            DrawCard();
        }
    }
    
    private void DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.Log("Deck is empty! Cannot draw card.");
            return;
        }
        
        if (hand.Count >= maxCardsInHand)
        {
            Debug.Log("Hand is full! Cannot draw more cards.");
            return;
        }
        
        // Draw card from deck
        CardData drawnCard = deck[0];
        deck.RemoveAt(0);
        hand.Add(drawnCard);
        
        // Create card instance
        GameObject cardInstance = CreateCardInstance(drawnCard);
        if (cardInstance != null)
        {
            cardInstances.Add(cardInstance);
            RepositionCards();
            
            Debug.Log($"Drew card: {drawnCard.cardName}");
        }
        
        UpdateDeckCount();
    }
    
    private GameObject CreateCardInstance(CardData cardData)
    {
        // Use the actual card prefab if available
        if (cardPrefab != null)
        {
            GameObject cardInstance = Instantiate(cardPrefab, cardHandParent);
            cardInstance.name = $"Card_{cardData.cardName}";
            
            Debug.Log($"Created card instance: {cardInstance.name} for card: {cardData.cardName} (Element: {cardData.element}, Rarity: {cardData.rarity})");
            
            // Scale the card prefab using localScale only
            RectTransform cardRectTransform = cardInstance.GetComponent<RectTransform>();
            if (cardRectTransform != null)
            {
                // Log the original size before scaling
                Vector2 originalSize = cardRectTransform.sizeDelta;
                Debug.Log($"Card {cardData.cardName} original size: {originalSize}");
                
                // Set up proper anchoring and pivot
                cardRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                cardRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                cardRectTransform.pivot = new Vector2(0.5f, 0.5f);
                
                // Disable any layout components that might interfere
                LayoutElement layoutElement = cardInstance.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.enabled = false;
                    Debug.Log($"Disabled LayoutElement on {cardData.cardName}");
                }
                
                // Disable ContentSizeFitter if present
                ContentSizeFitter contentSizeFitter = cardInstance.GetComponent<ContentSizeFitter>();
                if (contentSizeFitter != null)
                {
                    contentSizeFitter.enabled = false;
                    Debug.Log($"Disabled ContentSizeFitter on {cardData.cardName}");
                }
                
                // Scale the entire card using localScale (don't change sizeDelta)
                ScaleCardWithLocalScale(cardInstance, cardSize);
                
                // Log the final scale after scaling
                Vector3 finalScale = cardInstance.transform.localScale;
                Debug.Log($"Card {cardData.cardName} final scale: {finalScale} (target size: {cardSize})");
            }
            else
            {
                Debug.LogError($"No RectTransform found on card instance for {cardData.cardName}");
            }
            
            // Set the card data on the CardVisualManager
            CardVisualManager visualManager = cardInstance.GetComponent<CardVisualManager>();
            if (visualManager != null)
            {
                Debug.Log($"Found CardVisualManager, updating visuals for {cardData.cardName}");
                visualManager.UpdateCardVisuals(cardData);
                
                // Check if CardVisualManager changed the size
                Vector2 sizeAfterVisualUpdate = cardRectTransform.sizeDelta;
                if (sizeAfterVisualUpdate != cardSize)
                {
                    Debug.LogWarning($"CardVisualManager changed size from {cardSize} to {sizeAfterVisualUpdate}");
                    // Force the size again after visual update
                    cardRectTransform.sizeDelta = cardSize;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(cardRectTransform);
                }
            }
            else
            {
                Debug.LogWarning($"No CardVisualManager found on card instance for {cardData.cardName}");
            }
            
            // Add click handler
            Button cardButton = cardInstance.GetComponent<Button>();
            if (cardButton == null)
            {
                cardButton = cardInstance.AddComponent<Button>();
            }
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => OnCardClicked(cardData));
            
            return cardInstance;
        }
        else
        {
            // Fallback to creating a simple card GameObject
            GameObject cardInstance = new GameObject($"Card_{cardData.cardName}");
            cardInstance.transform.SetParent(cardHandParent);
            
            // Add RectTransform for UI positioning
            RectTransform rectTransform = cardInstance.AddComponent<RectTransform>();
            rectTransform.sizeDelta = cardSize;
            
            // Add Image component for background
            Image backgroundImage = cardInstance.AddComponent<Image>();
            backgroundImage.color = GetRarityColor(cardData.rarity);
            
            // Add border
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(cardInstance.transform);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = Color.white;
            borderImage.sprite = null;
            
            // Add text elements
            AddCardText(cardInstance, "Name", cardData.cardName, 12, Color.white, new Vector2(0, 60));
            AddCardText(cardInstance, "Type", cardData.cardType, 10, Color.yellow, new Vector2(0, 40));
            AddCardText(cardInstance, "Cost", $"Cost: {cardData.playCost}", 10, Color.cyan, new Vector2(0, 20));
            AddCardText(cardInstance, "Description", cardData.description, 8, Color.white, new Vector2(0, -20));
            
            // Add click handler
            Button cardButton = cardInstance.AddComponent<Button>();
            cardButton.onClick.AddListener(() => OnCardClicked(cardData));
            
            return cardInstance;
        }
    }
    
    private void AddCardText(GameObject parent, string name, string text, int fontSize, Color color, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(cardSize.x - 10, 20);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
    
    private Color GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
            case CardRarity.Magic: return new Color(0.3f, 0.3f, 0.8f);
            case CardRarity.Rare: return new Color(0.8f, 0.6f, 0.2f);
            case CardRarity.Unique: return new Color(0.8f, 0.2f, 0.8f);
            default: return Color.gray;
        }
    }
    
    private void OnCardClicked(CardData cardData)
    {
        PlayCard(cardData);
    }
    
    public void PlayCard(CardData cardData)
    {
        if (hand.Contains(cardData))
        {
            // Remove from hand
            int cardIndex = hand.IndexOf(cardData);
            hand.RemoveAt(cardIndex);
            
            // Remove card instance
            if (cardIndex < cardInstances.Count)
            {
                Destroy(cardInstances[cardIndex]);
                cardInstances.RemoveAt(cardIndex);
            }
            
            // Add to discard pile
            discardPile.Add(cardData);
            
            // Reposition remaining cards
            RepositionCards();
            
            // Update UI
            UpdateDiscardCount();
            
            Debug.Log($"Played card: {cardData.cardName}");
            
            // Apply card effects (placeholder for now)
            ApplyCardEffects(cardData);
        }
    }
    
    private void ApplyCardEffects(CardData cardData)
    {
        // Placeholder for card effects
        Debug.Log($"Applying effects for {cardData.cardName}:");
        if (cardData.damage > 0)
        {
            Debug.Log($"- Deal {cardData.damage} damage");
        }
        if (cardData.block > 0)
        {
            Debug.Log($"- Gain {cardData.block} block");
        }
    }
    
    private void RepositionCards()
    {
        if (cardInstances.Count == 0) return;
        
        // Calculate the actual scaled width for positioning using the new scale values
        float actualCardWidth = 120f * this.scaleX; // Base card width * X scale
        
        // Calculate total width needed for all cards
        float totalWidth = (cardInstances.Count - 1) * (actualCardWidth + cardSpacing);
        float startX = -totalWidth / 2f;
        
        Debug.Log($"Repositioning {cardInstances.Count} cards. Total width: {totalWidth}, Start X: {startX}, Actual card width: {actualCardWidth} (Scale X: {this.scaleX})");
        
        for (int i = 0; i < cardInstances.Count; i++)
        {
            if (cardInstances[i] != null)
            {
                RectTransform rectTransform = cardInstances[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float xPos = startX + i * (actualCardWidth + cardSpacing);
                    rectTransform.anchoredPosition = new Vector2(xPos, 0);
                    
                    // Ensure the card is visible and properly positioned
                    rectTransform.localRotation = Quaternion.identity;
                    
                    Debug.Log($"Card {i}: {cardInstances[i].name} positioned at X: {xPos}");
                }
                else
                {
                    Debug.LogError($"Card {i} has no RectTransform component!");
                }
            }
        }
    }
    
    private void OnDrawCardClicked()
    {
        DrawCard();
    }
    
    private void OnEndTurnClicked()
    {
        Debug.Log("End turn clicked - shuffling discard pile back into deck");
        
        // Move discard pile back to deck
        deck.AddRange(discardPile);
        discardPile.Clear();
        
        // Clear hand
        foreach (var cardInstance in cardInstances)
        {
            if (cardInstance != null)
            {
                Destroy(cardInstance);
            }
        }
        cardInstances.Clear();
        hand.Clear();
        
        // Shuffle deck
        ShuffleDeck();
        
        // Draw new hand
        DrawInitialHand();
        
        UpdateDeckCount();
        UpdateDiscardCount();
    }
    
    private void UpdateDeckCount()
    {
        if (deckCountText != null)
        {
            deckCountText.text = deck.Count.ToString();
        }
    }
    
    private void UpdateDiscardCount()
    {
        if (discardCountText != null)
        {
            discardCountText.text = discardPile.Count.ToString();
        }
    }
    
    // Public methods for external access
    public void ShuffleDeckPublic()
    {
        ShuffleDeck();
    }
    
    public void DrawCardPublic()
    {
        DrawCard();
    }
    
    public int GetDeckCount()
    {
        return deck.Count;
    }
    
    public int GetHandCount()
    {
        return hand.Count;
    }
    
    public int GetDiscardCount()
    {
        return discardPile.Count;
    }
    
    // Public method to resize all cards in hand
    public void ResizeCards(Vector2 newSize)
    {
        cardSize = newSize;
        
        // Resize all existing cards in hand
        foreach (var cardInstance in cardInstances)
        {
            if (cardInstance != null)
            {
                RectTransform rectTransform = cardInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Set up proper anchoring and pivot
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    
                    // Disable layout components
                    LayoutElement layoutElement = cardInstance.GetComponent<LayoutElement>();
                    if (layoutElement != null)
                    {
                        layoutElement.enabled = false;
                    }
                    
                    // Scale the entire card using localScale (don't change sizeDelta)
                    ScaleCardWithLocalScale(cardInstance, cardSize);
                }
            }
        }
        
        // Reposition cards with new size
        RepositionCards();
        
        Debug.Log($"Resized all cards to: {cardSize}");
    }
    
    // Method to force refresh card sizes (call this if cards aren't updating)
    public void ForceRefreshCardSizes()
    {
        Debug.Log("Force refreshing card sizes...");
        ResizeCards(cardSize);
    }
    
    // Public method to test different card sizes (for debugging)
    [ContextMenu("Test Small Cards (0.5x)")]
    public void TestSmallCards()
    {
        scaleX = 0.5f;
        scaleY = 0.5f;
        ApplyScalingToAllCards();
    }
    
    [ContextMenu("Test Large Cards (2x)")]
    public void TestLargeCards()
    {
        scaleX = 2f;
        scaleY = 2f;
        ApplyScalingToAllCards();
    }
    
    [ContextMenu("Test Huge Cards (3x)")]
    public void TestHugeCards()
    {
        scaleX = 3f;
        scaleY = 3f;
        ApplyScalingToAllCards();
    }
    
    [ContextMenu("Reset Card Size (1x)")]
    public void ResetCardSize()
    {
        scaleX = 1f;
        scaleY = 1f;
        ApplyScalingToAllCards();
    }
    
    [ContextMenu("Test Wide Cards (2x X, 1x Y)")]
    public void TestWideCards()
    {
        scaleX = 2f;
        scaleY = 1f;
        ApplyScalingToAllCards();
    }
    
    [ContextMenu("Test Tall Cards (1x X, 2x Y)")]
    public void TestTallCards()
    {
        scaleX = 1f;
        scaleY = 2f;
        ApplyScalingToAllCards();
    }
    
    [ContextMenu("Apply Current Scaling")]
    public void ApplyCurrentScaling()
    {
        ApplyScalingToAllCards();
    }
    
    // Method to scale the entire card using localScale
    private void ScaleCardWithLocalScale(GameObject cardInstance, Vector2 targetSize)
    {
        // Use the inspector scale values for more control
        Vector3 scale = new Vector3(this.scaleX, this.scaleY, 1f);
        
        // Apply the scale to the entire card
        cardInstance.transform.localScale = scale;
        
        Debug.Log($"Scaled card {cardInstance.name} with localScale: {scale} (target size: {targetSize})");
    }
    
    // Apply scaling to all existing cards
    private void ApplyScalingToAllCards()
    {
        foreach (GameObject cardInstance in cardInstances)
        {
            if (cardInstance != null)
            {
                ScaleCardWithLocalScale(cardInstance, cardSize);
            }
        }
        
        // Reposition cards after scaling
        RepositionCards();
        
        Debug.Log($"Applied scaling to all cards: X={scaleX}, Y={scaleY}");
    }
    
    // Debug method to check all RectTransforms in a card
    [ContextMenu("Debug Card Sizes")]
    public void DebugCardSizes()
    {
        if (cardInstances.Count == 0)
        {
            Debug.Log("No cards in hand to debug");
            return;
        }
        
        GameObject firstCard = cardInstances[0];
        if (firstCard != null)
        {
            Debug.Log($"=== Debugging card: {firstCard.name} ===");
            
            // Check main RectTransform
            RectTransform mainRect = firstCard.GetComponent<RectTransform>();
            if (mainRect != null)
            {
                Debug.Log($"Main RectTransform - Size: {mainRect.sizeDelta}, Anchors: {mainRect.anchorMin}-{mainRect.anchorMax}, Pivot: {mainRect.pivot}");
            }
            
            // Check all child RectTransforms
            RectTransform[] childRects = firstCard.GetComponentsInChildren<RectTransform>();
            Debug.Log($"Found {childRects.Length} RectTransforms in card hierarchy:");
            
            for (int i = 0; i < childRects.Length; i++)
            {
                RectTransform child = childRects[i];
                Debug.Log($"  {i}: {child.name} - Size: {child.sizeDelta}, Anchors: {child.anchorMin}-{child.anchorMax}");
            }
            
            // Check for layout components
            LayoutElement[] layoutElements = firstCard.GetComponentsInChildren<LayoutElement>();
            ContentSizeFitter[] contentSizeFitters = firstCard.GetComponentsInChildren<ContentSizeFitter>();
            
            Debug.Log($"Layout Elements: {layoutElements.Length}, ContentSizeFitters: {contentSizeFitters.Length}");
        }
    }
}
