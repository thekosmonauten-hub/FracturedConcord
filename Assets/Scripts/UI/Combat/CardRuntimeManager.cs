using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages card GameObjects at runtime for combat.
/// Handles creation, pooling, display, and lifecycle of card visual instances.
/// 
/// Workflow: Card Data → GameObject → Display in Hand → Animate → Pool/Destroy
/// </summary>
public class CardRuntimeManager : MonoBehaviour
{
    public static CardRuntimeManager Instance { get; private set; }
    
    [Header("Card Prefab")]
    [Tooltip("The prefab used to create card GameObjects")]
    [SerializeField] private GameObject cardPrefab;
    
    [Header("Card Display Settings")]
    [Tooltip("Parent transform where cards in hand are displayed")]
    [SerializeField] public Transform cardHandParent; // Public so other systems can check parent
    
    [Header("Card Positioning (Fine-Tune Here!)")]
    [Tooltip("Spacing between cards in hand")]
    [SerializeField] private float cardSpacing = 140f;
    
    [Tooltip("Horizontal offset - shift entire hand left (negative) or right (positive)")]
    [SerializeField] private float handXOffset = 0f;
    
    [Tooltip("Vertical offset from hand parent")]
    [SerializeField] private float cardYOffset = 0f;
    
    [Tooltip("Scale of cards")]
    [SerializeField] private Vector3 cardScale = Vector3.one;
    
    // Public getters for other systems to access values
    public float CardSpacing => cardSpacing;
    public float HandXOffset => handXOffset;
    public float CardYOffset => cardYOffset;
    
    [Header("Object Pooling")]
    [Tooltip("Pre-create this many cards for pooling")]
    [SerializeField] private int poolSize = 15;
    
    [Tooltip("Parent for pooled cards")]
    [SerializeField] private Transform poolParent;
    
    [Header("Deck Positions")]
    [Tooltip("Where cards animate from when drawn")]
    [SerializeField] private Transform deckPosition;
    
    [Tooltip("Where cards animate to when discarded")]
    [SerializeField] private Transform discardPosition;
    
    // Card pool
    private Queue<GameObject> cardPool = new Queue<GameObject>();
    private List<GameObject> activeCards = new List<GameObject>();
    
    // References
    private CombatAnimationManager animManager;
    
    #region Initialization
    
    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        animManager = CombatAnimationManager.Instance;
        
        InitializePool();
    }
    
    private void InitializePool()
    {
        // Create pool parent if not assigned
        if (poolParent == null)
        {
            GameObject poolObj = new GameObject("CardPool");
            poolObj.transform.SetParent(transform);
            poolParent = poolObj.transform;
        }
        
        // Pre-instantiate cards
        if (cardPrefab != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject card = Instantiate(cardPrefab, poolParent);
                card.name = $"PooledCard_{i}";
                card.SetActive(false);
                cardPool.Enqueue(card);
            }
            
            Debug.Log($"Card pool initialized with {poolSize} cards");
        }
        else
        {
            Debug.LogWarning("Card prefab not assigned! Cards will be created procedurally.");
        }
    }
    
    #endregion
    
    #region Card Creation
    
    /// <summary>
    /// Create a card GameObject from Card data
    /// </summary>
    public GameObject CreateCardFromData(Card cardData, Character ownerCharacter)
    {
        GameObject cardObj = GetCardFromPool();
        if (cardObj == null) return null;
        
        // IMPORTANT: Reparent to hand (not pool!)
        if (cardHandParent != null)
        {
            cardObj.transform.SetParent(cardHandParent, false); // false = keep world position
        }
        
        // Try to use CombatCardAdapter (works with DeckBuilderCardUI prefabs)
        CombatCardAdapter adapter = cardObj.GetComponent<CombatCardAdapter>();
        if (adapter != null)
        {
            adapter.SetCard(cardData, ownerCharacter);
        }
        else
        {
            // Fallback to CardVisualizer
            CardVisualizer visualizer = cardObj.GetComponent<CardVisualizer>();
            if (visualizer == null)
            {
                visualizer = cardObj.AddComponent<CardVisualizer>();
            }
            visualizer.SetCard(cardData, ownerCharacter);
        }
        
        // Setup hover effect (if not already on prefab)
        CardHoverEffect hover = cardObj.GetComponent<CardHoverEffect>();
        if (hover == null)
        {
            hover = cardObj.AddComponent<CardHoverEffect>();
        }
        hover.animationManager = animManager;
        hover.enabled = true;
        // Ensure tooltip component exists (can be disabled per prefab)
        if (cardObj.GetComponent<CardHoverTooltip>() == null)
        {
            cardObj.AddComponent<CardHoverTooltip>();
        }
        
        // Scale
        cardObj.transform.localScale = cardScale;
        
        // IMPORTANT: Store base scale for hover effect AFTER setting scale
        hover.StoreBaseScale();
        
        // Add to active cards list
        if (!activeCards.Contains(cardObj))
        {
            activeCards.Add(cardObj);
        }
        
        return cardObj;
    }
    
    /// <summary>
    /// Create a card GameObject from CardData ScriptableObject
    /// </summary>
    public GameObject CreateCardFromCardData(CardData cardData)
    {
        GameObject cardObj = GetCardFromPool();
        if (cardObj == null) return null;
        
        // IMPORTANT: Reparent to hand (not pool!)
        if (cardHandParent != null)
        {
            cardObj.transform.SetParent(cardHandParent, false);
        }
        
        // Try to use CombatCardAdapter (works with DeckBuilderCardUI prefabs)
        CombatCardAdapter adapter = cardObj.GetComponent<CombatCardAdapter>();
        if (adapter != null)
        {
            adapter.SetCardData(cardData);
        }
        else
        {
            // Fallback to CardDataVisualizer
            CardDataVisualizer visualizer = cardObj.GetComponent<CardDataVisualizer>();
            if (visualizer == null)
            {
                visualizer = cardObj.AddComponent<CardDataVisualizer>();
            }
            visualizer.SetCardData(cardData);
        }
        
        // Setup hover effect (if not already on prefab)
        CardHoverEffect hover = cardObj.GetComponent<CardHoverEffect>();
        if (hover == null)
        {
            hover = cardObj.AddComponent<CardHoverEffect>();
        }
        hover.animationManager = animManager;
        hover.enabled = true;
        // Ensure tooltip component exists (can be disabled per prefab)
        if (cardObj.GetComponent<CardHoverTooltip>() == null)
        {
            cardObj.AddComponent<CardHoverTooltip>();
        }
        
        // Scale
        cardObj.transform.localScale = cardScale;
        
        // IMPORTANT: Store base scale for hover effect AFTER setting scale
        hover.StoreBaseScale();
        
        // Add to active cards list
        if (!activeCards.Contains(cardObj))
        {
            activeCards.Add(cardObj);
        }
        
        return cardObj;
    }
    
    /// <summary>
    /// Create a card GameObject from CardDataExtended ScriptableObject (PREFERRED METHOD - NO CONVERSION!)
    /// </summary>
    public GameObject CreateCardFromCardDataExtended(CardDataExtended cardData, Character character)
    {
        GameObject cardObj = GetCardFromPool();
        if (cardObj == null) return null;
        
        // IMPORTANT: Reparent to hand (not pool!)
        if (cardHandParent != null)
        {
            cardObj.transform.SetParent(cardHandParent, false);
        }
        
        Debug.Log($"<color=cyan>[CardDataExtended] Creating card: {cardData.cardName}</color>");
        Debug.Log($"<color=cyan>[CardDataExtended]   - Card Image: {(cardData.cardImage != null ? "✅ LOADED" : "❌ NULL")}</color>");
        
        // Try to use CombatCardAdapter first (handles conversion internally)
        CombatCardAdapter adapter = cardObj.GetComponent<CombatCardAdapter>();
        if (adapter != null)
        {
            adapter.SetCardDataExtended(cardData, character);
            Debug.Log($"<color=green>[CardDataExtended] ✓ Set card via CombatCardAdapter</color>");
        }
        else
        {
            // Fallback to DeckBuilderCardUI directly
            DeckBuilderCardUI deckBuilderCard = cardObj.GetComponent<DeckBuilderCardUI>();
            if (deckBuilderCard != null)
            {
                // Pass character for dynamic descriptions!
                deckBuilderCard.Initialize(cardData, null, character);
                Debug.Log($"<color=green>[CardDataExtended] ✓ Set card via DeckBuilderCardUI with character</color>");
            }
            else
            {
                Debug.LogWarning($"[CardDataExtended] ⚠ No CombatCardAdapter or DeckBuilderCardUI found on card prefab!");
            }
        }
        
        // Setup hover effect (if not already on prefab)
        CardHoverEffect hover = cardObj.GetComponent<CardHoverEffect>();
        if (hover == null)
        {
            hover = cardObj.AddComponent<CardHoverEffect>();
        }
        hover.animationManager = animManager;
        hover.enabled = true;
        // Ensure tooltip component exists (can be disabled per prefab)
        if (cardObj.GetComponent<CardHoverTooltip>() == null)
        {
            cardObj.AddComponent<CardHoverTooltip>();
        }
        
        // Scale
        cardObj.transform.localScale = cardScale;
        
        // IMPORTANT: Store base scale for hover effect AFTER setting scale
        hover.StoreBaseScale();
        
        // Add to active cards list
        if (!activeCards.Contains(cardObj))
        {
            activeCards.Add(cardObj);
        }
        
        return cardObj;
    }
    
    /// <summary>
    /// Create multiple cards and display in hand
    /// </summary>
    public List<GameObject> CreateHandFromCards(List<Card> cards, Character ownerCharacter)
    {
        List<GameObject> cardObjects = new List<GameObject>();
        
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject cardObj = CreateCardFromData(cards[i], ownerCharacter);
            if (cardObj != null)
            {
                cardObjects.Add(cardObj);
                activeCards.Add(cardObj);
                EnsureCardParent(cardObj);
                PositionCardInHand(cardObj, i, cards.Count);
                
                if (animManager != null && deckPosition != null)
                {
                    float delay = i * 0.1f;
                    AnimateCardDraw(cardObj, delay);
                }
            }
        }
        
        return cardObjects;
    }
    
    #endregion
    
    #region Card Positioning
    
    /// <summary>
    /// Position a card in the hand based on index
    /// </summary>
    public void PositionCardInHand(GameObject cardObj, int index, int totalCards)
    {
        if (cardHandParent == null || cardObj == null) return;

        EnsureCardParent(cardObj);

        Vector3 localPosition = CalculateCardPosition(index, totalCards);

        var rect = cardObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition3D = localPosition;
            rect.localRotation = Quaternion.identity;
        }
        else
        {
            cardObj.transform.localPosition = localPosition;
            cardObj.transform.localRotation = Quaternion.identity;
        }

        cardObj.transform.localScale = cardScale;
        SetSiblingIndex(cardObj.transform, index);

        CardHoverEffect hoverEffect = cardObj.GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.StoreOriginalPosition();
        }
    }

    /// <summary>
    /// Calculate position for a card in hand (public so other systems can use same logic)
    /// </summary>
    public Vector3 CalculateCardPosition(int index, int totalCards)
    {
        if (totalCards <= 1)
        {
            return new Vector3(handXOffset, cardYOffset, 0f);
        }

        float totalWidth = (totalCards - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;
        float xPos = startX + (index * cardSpacing) + handXOffset;

        return new Vector3(xPos, cardYOffset, 0f);
    }
    
    /// <summary>
    /// Reposition all active cards in hand with smooth animation
    /// </summary>
    public void RepositionAllCards(bool animated = true, float duration = 0.3f)
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            if (activeCards[i] != null && activeCards[i].activeInHierarchy)
            {
                if (animated)
                {
                    PositionCardInHandAnimated(activeCards[i], i, activeCards.Count, duration);
                }
                else
                {
                    PositionCardInHand(activeCards[i], i, activeCards.Count);
                }
            }
        }
    }
    
    /// <summary>
    /// Reposition a specific list of cards (for CombatDeckManager's handVisuals)
    /// </summary>
    public void RepositionCards(List<GameObject> cardList, bool animated = true, float duration = 0.3f)
    {
        Debug.Log($"<color=magenta>Repositioning {cardList.Count} cards (animated: {animated})...</color>");
        
        // Additional safety check: Remove any null cards from the list
        cardList.RemoveAll(card => card == null);
        
        // Additional safety check: Verify all cards are properly parented
        foreach (GameObject card in cardList)
        {
            if (card != null && card.transform.parent != cardHandParent)
            {
                Debug.LogWarning($"<color=yellow>Card {card.name} has wrong parent! Reparenting to hand...</color>");
                card.transform.SetParent(cardHandParent, false);
            }
        }
        
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] != null && cardList[i].activeInHierarchy)
            {
                Debug.Log($"  Repositioning card {i}: {cardList[i].name}");
                
                // Additional safety: Cancel any existing position tweens before repositioning
                LeanTween.cancel(cardList[i], false);
                
                if (animated)
                {
                    PositionCardInHandAnimated(cardList[i], i, cardList.Count, duration);
                }
                else
                {
                    PositionCardInHand(cardList[i], i, cardList.Count);
                }
            }
            else if (cardList[i] == null)
            {
                Debug.LogWarning($"<color=red>Card at index {i} is NULL during reposition!</color>");
            }
        }
        
        Debug.Log($"<color=green>✓ Reposition complete for {cardList.Count} cards</color>");
    }
    
    /// <summary>
    /// Position a card in hand with smooth animation (for "squeeze together" effect)
    /// </summary>
    private void PositionCardInHandAnimated(GameObject cardObj, int index, int totalCards, float duration)
    {
        if (cardHandParent == null || cardObj == null) return;

        EnsureCardParent(cardObj);
        Vector3 targetLocalPos = CalculateCardPosition(index, totalCards);
        SetSiblingIndex(cardObj.transform, index);

        LeanTween.cancel(cardObj, false);

        var rect = cardObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            LeanTween.move(rect, targetLocalPos, duration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    rect.anchoredPosition3D = targetLocalPos;
                    rect.localRotation = Quaternion.identity;
                    FinalizeCardPlacement(cardObj);
                });
        }
        else
        {
            LeanTween.moveLocal(cardObj, targetLocalPos, duration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    cardObj.transform.localPosition = targetLocalPos;
                    cardObj.transform.localRotation = Quaternion.identity;
                    FinalizeCardPlacement(cardObj);
                });
        }
    }
    
    #endregion
    
    #region Card Animations
    
    /// <summary>
    /// Animate card being drawn from deck
    /// </summary>
    private void AnimateCardDraw(GameObject cardObj, float delay = 0f)
    {
        if (animManager == null || deckPosition == null) return;

        Vector3 startPos = deckPosition.position;
        Vector3 endPos = cardObj.transform.position;

        cardObj.transform.position = startPos;

        if (delay > 0f)
        {
            LeanTween.delayedCall(delay, () =>
            {
                animManager.AnimateCardDraw(cardObj, startPos, endPos);
            });
        }
        else
        {
            animManager.AnimateCardDraw(cardObj, startPos, endPos);
        }
    }
    
    /// <summary>
    /// Animate card being played
    /// </summary>
    public void AnimateCardPlay(GameObject cardObj, Vector3 targetPosition, System.Action onComplete = null)
    {
        if (animManager != null)
        {
            animManager.AnimateCardPlay(cardObj, targetPosition, () => {
                // Remove from active cards
                activeCards.Remove(cardObj);
                ReturnCardToPool(cardObj);
                onComplete?.Invoke();
            });
        }
        else
        {
            // Immediate
            activeCards.Remove(cardObj);
            ReturnCardToPool(cardObj);
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Animate card being discarded
    /// </summary>
    public void AnimateCardDiscard(GameObject cardObj, System.Action onComplete = null)
    {
        if (animManager != null && discardPosition != null)
        {
            animManager.AnimateCardDiscard(cardObj, discardPosition.position, () => {
                activeCards.Remove(cardObj);
                ReturnCardToPool(cardObj);
                onComplete?.Invoke();
            });
        }
        else
        {
            activeCards.Remove(cardObj);
            ReturnCardToPool(cardObj);
            onComplete?.Invoke();
        }
    }
    
    #endregion
    
    #region Object Pooling
    
    /// <summary>
    /// Get a card from the pool or create new one
    /// </summary>
    private GameObject GetCardFromPool()
    {
        GameObject card;
        
        if (cardPool.Count > 0)
        {
            card = cardPool.Dequeue();
        }
        else
        {
            // Pool exhausted - create new card
            if (cardPrefab != null)
            {
                card = Instantiate(cardPrefab, poolParent);
                Debug.LogWarning($"Card pool exhausted! Created new card. Consider increasing pool size.");
            }
            else
            {
                // Create basic card GameObject
                card = CreateBasicCardObject();
            }
        }
        
        card.SetActive(true);
        return card;
    }
    
    /// <summary>
    /// Return a card to the pool
    /// </summary>
    public void ReturnCardToPool(GameObject card)
    {
        if (card == null) return;
        
        card.SetActive(false);
        card.transform.SetParent(poolParent);
        cardPool.Enqueue(card);
    }
    
    /// <summary>
    /// Create a basic card GameObject procedurally
    /// </summary>
    private GameObject CreateBasicCardObject()
    {
        GameObject card = new GameObject("BasicCard");
        card.transform.SetParent(poolParent);
        
        // Add RectTransform
        RectTransform rect = card.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 180);
        
        // Add Image for background
        Image background = card.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f);
        
        // Add CanvasGroup for fading
        card.AddComponent<CanvasGroup>();
        
        // Add Button for clicking
        card.AddComponent<Button>();
        
        Debug.Log("Created procedural card GameObject");
        return card;
    }
    
    /// <summary>
    /// Clear all active cards
    /// </summary>
    public void ClearAllCards()
    {
        foreach (GameObject card in activeCards)
        {
            if (card != null)
            {
                ReturnCardToPool(card);
            }
        }
        
        activeCards.Clear();
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Get count of active cards
    /// </summary>
    public int GetActiveCardCount()
    {
        return activeCards.Count;
    }
    
    /// <summary>
    /// Get all active card GameObjects
    /// </summary>
    public List<GameObject> GetActiveCards()
    {
        return new List<GameObject>(activeCards);
    }
    
    /// <summary>
    /// Remove a specific card from display
    /// </summary>
    public void RemoveCard(GameObject cardObj)
    {
        if (activeCards.Contains(cardObj))
        {
            activeCards.Remove(cardObj);
            ReturnCardToPool(cardObj);
            RepositionAllCards();
        }
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Apply Positioning Changes (Update All Cards)")]
    public void ApplyPositioningChanges()
    {
        Debug.Log($"<color=yellow>Applying new positioning settings:</color>");
        Debug.Log($"  Card Spacing: {cardSpacing}");
        Debug.Log($"  Hand X Offset: {handXOffset}");
        Debug.Log($"  Card Y Offset: {cardYOffset}");
        Debug.Log($"  Card Scale: {cardScale}");
        
        RepositionAllCards(animated: false);
        Debug.Log($"✓ Repositioned {activeCards.Count} cards");
    }
    
    [ContextMenu("Create Test Card")]
    private void CreateTestCard()
    {
        // Create a test card
        Card testCard = new Card
        {
            cardName = "Test Strike",
            description = "Deal 10 damage",
            cardType = CardType.Attack,
            manaCost = 1,
            baseDamage = 10f,
            primaryDamageType = DamageType.Physical
        };
        
        CharacterManager charManager = CharacterManager.Instance;
        Character player = charManager != null && charManager.HasCharacter() ? charManager.GetCurrentCharacter() : null;
        
        GameObject cardObj = CreateCardFromData(testCard, player);
        if (cardObj != null)
        {
            activeCards.Add(cardObj);
            cardObj.transform.SetParent(cardHandParent);
            PositionCardInHand(cardObj, activeCards.Count - 1, activeCards.Count);
            
            Debug.Log("Created test card");
        }
    }
    
    [ContextMenu("Create Test Hand (5 cards)")]
    private void CreateTestHand()
    {
        List<Card> testCards = new List<Card>
        {
            new Card { cardName = "Strike", cardType = CardType.Attack, manaCost = 1, baseDamage = 8f },
            new Card { cardName = "Block", cardType = CardType.Guard, manaCost = 1, baseGuard = 5f },
            new Card { cardName = "Fireball", cardType = CardType.Attack, manaCost = 2, baseDamage = 12f, primaryDamageType = DamageType.Fire },
            new Card { cardName = "Heal", cardType = CardType.Skill, manaCost = 2 },
            new Card { cardName = "Power Up", cardType = CardType.Power, manaCost = 1 }
        };
        
        CharacterManager charManager = CharacterManager.Instance;
        Character player = charManager != null && charManager.HasCharacter() ? charManager.GetCurrentCharacter() : null;
        
        CreateHandFromCards(testCards, player);
        
        Debug.Log("Created test hand with 5 cards");
    }
    
    [ContextMenu("Clear All Cards")]
    private void DebugClearAllCards()
    {
        ClearAllCards();
        Debug.Log("Cleared all cards");
    }
    
    [ContextMenu("Show Pool Stats")]
    private void ShowPoolStats()
    {
        Debug.Log($"Card Pool Stats:\n" +
                  $"- Pool Size: {poolSize}\n" +
                  $"- Available in Pool: {cardPool.Count}\n" +
                  $"- Active Cards: {activeCards.Count}\n" +
                  $"- Total Created: {poolSize - cardPool.Count + activeCards.Count}");
    }
    
    #endregion

    private void EnsureCardParent(GameObject cardObj)
    {
        if (cardHandParent != null && cardObj.transform.parent != cardHandParent)
        {
            cardObj.transform.SetParent(cardHandParent, false);
        }
    }

    private void FinalizeCardPlacement(GameObject cardObj)
    {
        CardHoverEffect hoverEffect = cardObj.GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.StoreOriginalPosition();
        }
    }

    private void SetSiblingIndex(Transform cardTransform, int index)
    {
        if (cardTransform == null || cardTransform.parent == null) return;
        int maxIndex = cardTransform.parent.childCount - 1;
        int target = Mathf.Clamp(index, 0, maxIndex);
        cardTransform.SetSiblingIndex(target);
    }
}

