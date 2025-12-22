using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Manages the discard panel UI for selecting cards to discard.
/// When a discard card is played, cards fly to center screen for selection.
/// </summary>
public class DiscardPanel : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Button cancelButton;
    
    [Header("Animation Targets")]
    [Tooltip("Parent transform for card animation targets (cards fly here)")]
    [SerializeField] private Transform cardTargetParent;
    [Tooltip("Prefab for creating animation target positions")]
    [SerializeField] private GameObject targetPositionPrefab;
    
    [Header("Animation Settings")]
    [Tooltip("Duration for cards to fly to center")]
    [SerializeField] private float flyInDuration = 0.5f;
    [Tooltip("Duration for cards to return to hand")]
    [SerializeField] private float returnDuration = 0.3f;
    [Tooltip("Spacing between cards in discard selection")]
    [SerializeField] private float cardSpacing = 150f;
    [Tooltip("Scale of cards when in discard selection")]
    [SerializeField] private Vector3 discardScale = new Vector3(1.2f, 1.2f, 1.2f);
    
    [Header("References")]
    [SerializeField] private CombatDeckManager deckManager;
    [SerializeField] private CardRuntimeManager cardRuntimeManager;
    
    // State
    private bool isActive = false;
    private int cardsToDiscard = 1;
    private List<GameObject> cardsInSelection = new List<GameObject>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Vector3> originalScales = new List<Vector3>();
    private System.Action<CardDataExtended> onDiscardComplete;
    private CardDataExtended sourceDiscardCard; // The card that triggered the discard
    
    private void Awake()
    {
        // Ensure panel is hidden on start
        EnsurePanelHidden();
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }
        
        // Auto-find references if not set
        if (deckManager == null)
        {
            deckManager = FindFirstObjectByType<CombatDeckManager>();
        }
        
        if (cardRuntimeManager == null)
        {
            cardRuntimeManager = FindFirstObjectByType<CardRuntimeManager>();
        }
    }
    
    private void Start()
    {
        // Double-check panel is hidden on start (safety check)
        EnsurePanelHidden();
    }
    
    private void OnEnable()
    {
        // Ensure panel stays hidden unless explicitly shown via ShowDiscardSelection
        if (!isActive)
        {
            EnsurePanelHidden();
        }
    }
    
    /// <summary>
    /// Ensure the panel root is hidden. If panelRoot is not assigned, disable this GameObject.
    /// </summary>
    private void EnsurePanelHidden()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        else
        {
            // If panelRoot is not assigned, try to find it
            // Look for a child GameObject with "Panel" in the name
            Transform panelTransform = transform.Find("Panel");
            if (panelTransform == null)
            {
                panelTransform = transform.Find("DiscardPanel");
            }
            if (panelTransform != null)
            {
                panelRoot = panelTransform.gameObject;
                panelRoot.SetActive(false);
            }
            // NOTE: We do NOT disable the GameObject itself here
            // The GameObject must stay active so coroutines can run
            // Only the panelRoot child should be hidden
        }
        
        isActive = false;
    }
    
    /// <summary>
    /// Show discard panel and animate cards to center for selection
    /// </summary>
    public void ShowDiscardSelection(int count, CardDataExtended sourceCard, System.Action<CardDataExtended> onComplete)
    {
        if (isActive)
        {
            Debug.LogWarning("[DiscardPanel] Already showing discard selection!");
            return;
        }
        
        // Ensure panelRoot is set
        if (panelRoot == null)
        {
            // Try to find panel root
            Transform panelTransform = transform.Find("Panel");
            if (panelTransform == null)
            {
                panelTransform = transform.Find("DiscardPanel");
            }
            if (panelTransform != null)
            {
                panelRoot = panelTransform.gameObject;
            }
            else
            {
                // If no child found, use this GameObject as the panel root
                panelRoot = gameObject;
                Debug.LogWarning("[DiscardPanel] panelRoot not assigned, using this GameObject as panel root. Please assign panelRoot in inspector.");
            }
        }
        
        // Ensure this GameObject is active
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            Debug.Log("[DiscardPanel] Activated DiscardPanel GameObject");
        }
        
        cardsToDiscard = count;
        sourceDiscardCard = sourceCard;
        onDiscardComplete = onComplete;
        
        // Get all cards currently in hand
        List<GameObject> handCards = GetHandCards();
        
        if (handCards.Count == 0)
        {
            Debug.LogWarning("[DiscardPanel] No cards in hand to discard!");
            onDiscardComplete?.Invoke(null);
            return;
        }
        
        Debug.Log($"[DiscardPanel] Showing discard selection for {count} card(s). Found {handCards.Count} cards in hand.");
        
        // Store original positions and scales
        originalPositions.Clear();
        originalScales.Clear();
        cardsInSelection.Clear();
        
        foreach (var cardObj in handCards)
        {
            if (cardObj != null)
            {
                originalPositions.Add(cardObj.transform.position);
                originalScales.Add(cardObj.transform.localScale);
                cardsInSelection.Add(cardObj);
            }
        }
        
        // Enable panel
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            Debug.Log($"[DiscardPanel] Enabled panel root: {panelRoot.name}");
        }
        else
        {
            Debug.LogError("[DiscardPanel] panelRoot is null! Cannot show discard panel!");
            onDiscardComplete?.Invoke(null);
            return;
        }
        
        // Update instruction text
        if (instructionText != null)
        {
            instructionText.text = $"Select {count} card{(count > 1 ? "s" : "")} to discard";
        }
        
        isActive = true;
        
        // CRITICAL: Ensure GameObject is active before starting coroutine
        // Coroutines can only run on active GameObjects
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log("[DiscardPanel] Force-activated GameObject before starting coroutine");
        }
        
        // Double-check it's active in hierarchy (parent chain must also be active)
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[DiscardPanel] GameObject or parent is inactive! Cannot start coroutine.");
            // Try to activate parent if it exists
            if (transform.parent != null && !transform.parent.gameObject.activeSelf)
            {
                transform.parent.gameObject.SetActive(true);
                Debug.Log("[DiscardPanel] Activated parent GameObject");
            }
            // Re-check after parent activation
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("[DiscardPanel] Still inactive after parent activation. Aborting.");
                onDiscardComplete?.Invoke(null);
                return;
            }
        }
        
        // Animate cards to center
        StartCoroutine(AnimateCardsToCenter(handCards));
    }
    
    /// <summary>
    /// Get all card GameObjects currently in hand
    /// </summary>
    private List<GameObject> GetHandCards()
    {
        List<GameObject> cards = new List<GameObject>();
        
        // First, try to get cards from CombatDeckManager (preferred method)
        if (deckManager != null)
        {
            // Use reflection to access private handVisuals list
            var field = typeof(CombatDeckManager).GetField("handVisuals", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var handVisuals = field.GetValue(deckManager) as List<GameObject>;
                if (handVisuals != null && handVisuals.Count > 0)
                {
                    // Filter out null entries and return only active cards
                    cards.AddRange(handVisuals.Where(go => go != null && go.activeInHierarchy));
                    if (cards.Count > 0)
                    {
                        Debug.Log($"[DiscardPanel] Found {cards.Count} cards from CombatDeckManager.handVisuals");
                        return cards;
                    }
                }
            }
        }
        
        // Fallback: Try CardRuntimeManager
        if (cardRuntimeManager != null)
        {
            // Use reflection to access private activeCards list
            var field = typeof(CardRuntimeManager).GetField("activeCards", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var activeCards = field.GetValue(cardRuntimeManager) as List<GameObject>;
                if (activeCards != null && activeCards.Count > 0)
                {
                    cards.AddRange(activeCards.Where(go => go != null && go.activeInHierarchy));
                    if (cards.Count > 0)
                    {
                        Debug.Log($"[DiscardPanel] Found {cards.Count} cards from CardRuntimeManager.activeCards");
                        return cards;
                    }
                }
            }
        }
        
        // Final fallback: Find cards by tag or component
        if (cards.Count == 0)
        {
            var cardObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go != null && go.activeInHierarchy && 
                       (go.GetComponent<DeckBuilderCardUI>() != null || go.GetComponent<CombatCardAdapter>() != null || go.name.Contains("Card")))
                .ToList();
            cards.AddRange(cardObjects);
            if (cards.Count > 0)
            {
                Debug.Log($"[DiscardPanel] Found {cards.Count} cards via fallback search");
            }
        }
        
        return cards;
    }
    
    /// <summary>
    /// Animate cards flying to center screen
    /// </summary>
    private IEnumerator AnimateCardsToCenter(List<GameObject> cards)
    {
        if (cards.Count == 0) yield break;
        
        // Create target positions
        List<Vector3> targetPositions = CalculateTargetPositions(cards.Count);
        
        // Animate each card
        List<Coroutine> animations = new List<Coroutine>();
        
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                // Disable card button temporarily to prevent playing while animating
                Button cardButton = cards[i].GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.interactable = false;
                }
                
                // Start animation
                Coroutine anim = StartCoroutine(AnimateCardToPosition(
                    cards[i], 
                    targetPositions[i], 
                    discardScale, 
                    flyInDuration
                ));
                animations.Add(anim);
            }
        }
        
        // Wait for all animations to complete
        foreach (var anim in animations)
        {
            yield return anim;
        }
        
        // Re-enable buttons for selection
        foreach (var card in cards)
        {
            if (card != null)
            {
                Button cardButton = card.GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.interactable = true;
                    // Replace click handler with discard selection
                    cardButton.onClick.RemoveAllListeners();
                    cardButton.onClick.AddListener(() => OnCardSelectedForDiscard(card));
                }
            }
        }
        
        Debug.Log($"[DiscardPanel] Cards animated to center. Ready for selection.");
    }
    
    /// <summary>
    /// Calculate target positions for cards in center screen
    /// </summary>
    private List<Vector3> CalculateTargetPositions(int cardCount)
    {
        List<Vector3> positions = new List<Vector3>();
        
        float totalWidth = (cardCount - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;
        
        // Use center of screen or cardTargetParent position
        Vector3 centerPos = cardTargetParent != null 
            ? cardTargetParent.position 
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        
        for (int i = 0; i < cardCount; i++)
        {
            Vector3 pos = centerPos;
            pos.x += startX + (i * cardSpacing);
            positions.Add(pos);
        }
        
        return positions;
    }
    
    /// <summary>
    /// Animate a single card to a target position
    /// </summary>
    private IEnumerator AnimateCardToPosition(GameObject card, Vector3 targetPos, Vector3 targetScale, float duration)
    {
        if (card == null) yield break;
        
        Vector3 startPos = card.transform.position;
        Vector3 startScale = card.transform.localScale;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth easing
            t = Mathf.SmoothStep(0f, 1f, t);
            
            card.transform.position = Vector3.Lerp(startPos, targetPos, t);
            card.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }
        
        // Ensure final position
        card.transform.position = targetPos;
        card.transform.localScale = targetScale;
    }
    
    /// <summary>
    /// Called when a card is selected for discard
    /// </summary>
    private void OnCardSelectedForDiscard(GameObject cardObj)
    {
        if (!isActive || cardObj == null) return;
        
        // Get CardDataExtended from the card object
        CardDataExtended cardData = GetCardDataFromObject(cardObj);
        
        if (cardData == null)
        {
            Debug.LogWarning($"[DiscardPanel] Could not get card data from {cardObj.name}");
            return;
        }
        
        Debug.Log($"[DiscardPanel] Card selected for discard: {cardData.cardName}");
        
        // Complete discard selection
        StartCoroutine(CompleteDiscardSelection(cardData));
    }
    
    /// <summary>
    /// Get CardDataExtended from a card GameObject
    /// </summary>
    private CardDataExtended GetCardDataFromObject(GameObject cardObj)
    {
        // Try CardVisualizer first
        CardVisualizer visualizer = cardObj.GetComponent<CardVisualizer>();
        if (visualizer != null)
        {
            // CardVisualizer has a Card, need to get CardDataExtended
            var cardField = typeof(CardVisualizer).GetField("card", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (cardField != null)
            {
                Card card = cardField.GetValue(visualizer) as Card;
                if (card != null && card.sourceCardData != null)
                {
                    return card.sourceCardData;
                }
            }
        }
        
        // Try finding by name in hand
        if (deckManager != null)
        {
            var handField = typeof(CombatDeckManager).GetField("hand", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (handField != null)
            {
                var hand = handField.GetValue(deckManager) as List<CardDataExtended>;
                if (hand != null)
                {
                    // Try to match by name (not perfect but works)
                    string cardName = cardObj.name;
                    foreach (var card in hand)
                    {
                        if (card != null && cardName.Contains(card.cardName))
                        {
                            return card;
                        }
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Complete the discard selection and return cards to hand
    /// </summary>
    private IEnumerator CompleteDiscardSelection(CardDataExtended discardedCard)
    {
        // Disable panel
        isActive = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        
        // Actually discard the selected card
        if (discardedCard != null && deckManager != null)
        {
            // Remove from hand
            var handField = typeof(CombatDeckManager).GetField("hand", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var handVisualsField = typeof(CombatDeckManager).GetField("handVisuals", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var discardPileField = typeof(CombatDeckManager).GetField("discardPile", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (handField != null && discardPileField != null)
            {
                var hand = handField.GetValue(deckManager) as List<CardDataExtended>;
                var discardPile = discardPileField.GetValue(deckManager) as List<CardDataExtended>;
                var handVisuals = handVisualsField?.GetValue(deckManager) as List<GameObject>;
                
                if (hand != null && discardPile != null)
                {
                    // Find and remove from hand
                    int discardIndex = hand.IndexOf(discardedCard);
                    if (discardIndex >= 0)
                    {
                        hand.RemoveAt(discardIndex);
                        discardPile.Add(discardedCard);
                        
                        // Remove visual
                        if (handVisuals != null && discardIndex < handVisuals.Count)
                        {
                            GameObject discardVisual = handVisuals[discardIndex];
                            handVisuals.RemoveAt(discardIndex);
                            
                            // Animate discarded card to discard pile
                            if (cardRuntimeManager != null && discardVisual != null)
                            {
                                var discardPileTransformField = typeof(CombatDeckManager).GetField("discardPileTransform", 
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                Transform discardPileTransform = discardPileTransformField?.GetValue(deckManager) as Transform;
                                
                                if (discardPileTransform != null)
                                {
                                    // Animate to discard pile
                                    LeanTween.move(discardVisual, discardPileTransform.position, 0.5f)
                                        .setEase(LeanTweenType.easeInOutQuad)
                                        .setOnComplete(() => {
                                            if (cardRuntimeManager != null && discardVisual != null)
                                            {
                                                cardRuntimeManager.ReturnCardToPool(discardVisual);
                                            }
                                        });
                                }
                                else
                                {
                                    cardRuntimeManager.ReturnCardToPool(discardVisual);
                                }
                            }
                        }
                        
                        Debug.Log($"<color=orange>[Discard] Discarded {discardedCard.cardName}</color>");
                        
                        // Trigger discard event
                        var onCardDiscardedField = typeof(CombatDeckManager).GetField("OnCardDiscarded", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (onCardDiscardedField != null)
                        {
                            var onCardDiscarded = onCardDiscardedField.GetValue(deckManager) as System.Action<CardDataExtended>;
                            onCardDiscarded?.Invoke(discardedCard);
                        }
                    }
                }
            }
        }
        
        // Return all remaining cards to hand
        List<GameObject> cardsToReturn = new List<GameObject>();
        List<int> returnIndices = new List<int>();
        
        for (int i = 0; i < cardsInSelection.Count; i++)
        {
            var cardObj = cardsInSelection[i];
            if (cardObj != null)
            {
                CardDataExtended cardData = GetCardDataFromObject(cardObj);
                if (cardData != discardedCard)
                {
                    cardsToReturn.Add(cardObj);
                    returnIndices.Add(i);
                }
            }
        }
        
        // Animate cards back to hand
        yield return StartCoroutine(ReturnCardsToHand(cardsToReturn, returnIndices));
        
        // Call completion callback
        onDiscardComplete?.Invoke(discardedCard);
        
        // Cleanup
        cardsInSelection.Clear();
        originalPositions.Clear();
        originalScales.Clear();
    }
    
    /// <summary>
    /// Return cards to their original hand positions
    /// </summary>
    private IEnumerator ReturnCardsToHand(List<GameObject> cards, List<int> indices)
    {
        List<Coroutine> animations = new List<Coroutine>();
        
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null && i < indices.Count)
            {
                int originalIndex = indices[i];
                Vector3 targetPos = originalIndex < originalPositions.Count 
                    ? originalPositions[originalIndex] 
                    : cards[i].transform.position;
                Vector3 targetScale = originalIndex < originalScales.Count 
                    ? originalScales[originalIndex] 
                    : Vector3.one;
                
                // Re-enable button and restore original click handler
                Button cardButton = cards[i].GetComponent<Button>();
                if (cardButton != null)
                {
                    cardButton.interactable = true;
                    // Restore original click handler (will be handled by CombatDeckManager)
                }
                
                Coroutine anim = StartCoroutine(AnimateCardToPosition(
                    cards[i], 
                    targetPos, 
                    targetScale, 
                    returnDuration
                ));
                animations.Add(anim);
            }
        }
        
        // Wait for all animations
        foreach (var anim in animations)
        {
            yield return anim;
        }
        
        Debug.Log("[DiscardPanel] Cards returned to hand.");
    }
    
    /// <summary>
    /// Cancel discard selection
    /// </summary>
    private void OnCancelClicked()
    {
        if (!isActive) return;
        
        Debug.Log("[DiscardPanel] Discard selection cancelled.");
        
        // Return all cards to hand without discarding
        StartCoroutine(CompleteDiscardSelection(null));
    }
    
    /// <summary>
    /// Check if discard panel is currently active
    /// </summary>
    public bool IsActive => isActive;
}

