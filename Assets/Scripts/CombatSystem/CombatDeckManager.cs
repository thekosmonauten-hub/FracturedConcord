using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the card deck during combat.
/// Handles loading, shuffling, drawing, and discarding cards.
/// Integrates with CardRuntimeManager for visual display.
/// </summary>
public class CombatDeckManager : MonoBehaviour
{
    public static CombatDeckManager Instance { get; private set; }
    
    [Header("Deck Settings")]
    [SerializeField] private bool loadDeckOnStart = true;
    [SerializeField] private int initialHandSize = 5;
    [SerializeField] private bool autoShuffleOnStart = true;
    
    [Header("Testing (Quick Test Mode)")]
    [SerializeField] private bool testLoadMarauderDeckOnStart = false;
    [Tooltip("When checked, loads Marauder starter deck on play (ignores character)")]
    
    [Header("References")]
    [SerializeField] private CardRuntimeManager cardRuntimeManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private CombatAnimationManager animationManager;
    [SerializeField] private CardEffectProcessor cardEffectProcessor;
    [SerializeField] private EnemyTargetingManager targetingManager;
    
    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI deckCountText;
    [SerializeField] private TMPro.TextMeshProUGUI discardCountText;
    
    [Header("Deck Positions (For Animations)")]
    [SerializeField] private Transform deckPileTransform;
    [Tooltip("Visual position where draw pile sits - cards animate from here")]
    [SerializeField] private Transform discardPileTransform;
    [Tooltip("Visual position where discard pile sits - cards animate to here when played")]
    
    [Header("Debug Info (Read-Only)")]
    [SerializeField] private int debugDrawPileCount = 0;
    [SerializeField] private int debugHandCount = 0;
    [SerializeField] private int debugDiscardCount = 0;
    
    // Deck piles
    private List<Card> drawPile = new List<Card>();
    private List<Card> hand = new List<Card>();
    private List<Card> discardPile = new List<Card>();
    private List<GameObject> handVisuals = new List<GameObject>();
    
    // Events
    public System.Action<Card> OnCardDrawn;
    public System.Action<Card> OnCardPlayed;
    public System.Action<Card> OnCardDiscarded;
    public System.Action OnDeckShuffled;
    
    #region Initialization
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find references
        if (cardRuntimeManager == null)
            cardRuntimeManager = CardRuntimeManager.Instance;
        
        if (characterManager == null)
            characterManager = CharacterManager.Instance;
        
        if (animationManager == null)
            animationManager = CombatAnimationManager.Instance;
        
        if (cardEffectProcessor == null)
            cardEffectProcessor = CardEffectProcessor.Instance;
        
        if (targetingManager == null)
            targetingManager = EnemyTargetingManager.Instance;
    }
    
    private void Start()
    {
        // TEST MODE: Load Marauder deck directly for quick testing
        if (testLoadMarauderDeckOnStart)
        {
            Debug.Log("<color=yellow>TEST MODE: Loading Marauder starter deck...</color>");
            LoadDeckForClass("Marauder");
            ShuffleDeck();
            DrawInitialHand();
            return; // Skip normal loading
        }
        
        // NORMAL MODE: Load deck based on character
        if (loadDeckOnStart)
        {
            LoadDeckForCurrentCharacter();
            
            if (autoShuffleOnStart)
            {
                ShuffleDeck();
            }
            
            DrawInitialHand();
        }
    }
    
    private void Update()
    {
        // Update debug counts (visible in Inspector)
        debugDrawPileCount = drawPile.Count;
        debugHandCount = hand.Count;
        debugDiscardCount = discardPile.Count;
        
        // Update UI text every frame (simple and works)
        UpdateDeckCountUI();
    }
    
    /// <summary>
    /// Update deck and discard count UI texts.
    /// </summary>
    private void UpdateDeckCountUI()
    {
        if (deckCountText != null)
        {
            deckCountText.text = drawPile.Count.ToString();
        }
        
        if (discardCountText != null)
        {
            discardCountText.text = discardPile.Count.ToString();
        }
    }
    
    #endregion
    
    #region Deck Loading
    
    /// <summary>
    /// Load deck for the current character's class
    /// </summary>
    public void LoadDeckForCurrentCharacter()
    {
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogError("No character loaded! Cannot load deck.");
            return;
        }
        
        Character player = characterManager.GetCurrentCharacter();
        LoadDeckForClass(player.characterClass);
    }
    
    /// <summary>
    /// Load deck for a specific class
    /// </summary>
    public void LoadDeckForClass(string characterClass)
    {
        Debug.Log($"<color=yellow>=== Loading deck for: {characterClass} ===</color>");
        
        // Clear existing deck
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        
        // Clear visual cards
        if (cardRuntimeManager != null)
        {
            cardRuntimeManager.ClearAllCards();
        }
        handVisuals.Clear();
        
        // Load deck from JSON
        List<Card> loadedDeck = DeckLoader.LoadStarterDeck(characterClass);
        
        if (loadedDeck != null && loadedDeck.Count > 0)
        {
            // Add cards to draw pile (don't reassign reference!)
            drawPile.AddRange(loadedDeck);
            Debug.Log($"<color=green>✓ Loaded {characterClass} deck:</color> {drawPile.Count} cards");
            Debug.Log($"Draw pile now contains: {drawPile.Count} cards");
            
            // Verify cards are actually in the list
            if (drawPile.Count > 0)
            {
                Debug.Log($"First card in draw pile: {drawPile[0].cardName}");
            }
            
            LogDeckComposition();
        }
        else
        {
            Debug.LogError($"<color=red>✗ Failed to load deck for {characterClass}!</color>");
        }
    }
    
    private void LogDeckComposition()
    {
        Dictionary<string, int> cardCounts = new Dictionary<string, int>();
        
        foreach (Card card in drawPile)
        {
            if (!cardCounts.ContainsKey(card.cardName))
            {
                cardCounts[card.cardName] = 0;
            }
            cardCounts[card.cardName]++;
        }
        
        string composition = "Deck Composition:\n";
        foreach (var kvp in cardCounts)
        {
            composition += $"  - {kvp.Key} x{kvp.Value}\n";
        }
        
        Debug.Log(composition);
    }
    
    #endregion
    
    #region Deck Management
    
    /// <summary>
    /// Shuffle the draw pile
    /// </summary>
    public void ShuffleDeck()
    {
        // Fisher-Yates shuffle
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card temp = drawPile[i];
            drawPile[i] = drawPile[j];
            drawPile[j] = temp;
        }
        
        OnDeckShuffled?.Invoke();
        Debug.Log("Deck shuffled");
    }
    
    /// <summary>
    /// Draw initial hand at combat start
    /// </summary>
    public void DrawInitialHand()
    {
        DrawCards(initialHandSize);
    }
    
    /// <summary>
    /// Draw a specific number of cards
    /// </summary>
    public void DrawCards(int count)
    {
        Debug.Log($"<color=yellow>=== DrawCards called: Drawing {count} cards ===</color>");
        Debug.Log($"Current state - Draw pile: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}");
        
        // Check CardRuntimeManager
        if (cardRuntimeManager == null)
        {
            Debug.LogError("CardRuntimeManager is NULL! Cannot create card visuals.");
            Debug.Log("Looking for CardRuntimeManager...");
            cardRuntimeManager = CardRuntimeManager.Instance;
            if (cardRuntimeManager == null)
            {
                Debug.LogError("Still couldn't find CardRuntimeManager! Make sure it exists in scene.");
                return;
            }
            else
            {
                Debug.Log("Found CardRuntimeManager!");
            }
        }
        
        Character player = characterManager != null && characterManager.HasCharacter() ? 
            characterManager.GetCurrentCharacter() : null;
        
        if (player == null)
        {
            Debug.LogWarning("No character found! Cards will show without character-specific values.");
        }
        
        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count == 0)
            {
                // Reshuffle discard pile into draw pile
                if (discardPile.Count > 0)
                {
                    Debug.Log("Draw pile empty! Reshuffling discard pile...");
                    drawPile.AddRange(discardPile);
                    discardPile.Clear();
                    ShuffleDeck();
                }
                else
                {
                    Debug.LogWarning("No cards left to draw!");
                    break;
                }
            }
            
            if (drawPile.Count > 0)
            {
                // Draw card from deck
                Card drawnCard = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(drawnCard);
                
                Debug.Log($"<color=cyan>Drawing card #{i+1}: {drawnCard.cardName}</color>");
                
                // Create visual card with draw animation!
                Debug.Log($"Creating visual for: {drawnCard.cardName}...");
                GameObject cardObj = CreateAnimatedCard(drawnCard, player, i);
                
                if (cardObj != null)
                {
                    Debug.Log($"<color=green>✓ Card GameObject created: {cardObj.name}</color>");
                    Debug.Log($"  Position: {cardObj.transform.position} (Local: {cardObj.transform.localPosition})");
                    Debug.Log($"  Active: {cardObj.activeInHierarchy}");
                    Debug.Log($"  Parent: {cardObj.transform.parent?.name ?? "NULL"}");
                    
                    // Verify parent is correct
                    if (cardRuntimeManager.cardHandParent != null)
                    {
                        if (cardObj.transform.parent == cardRuntimeManager.cardHandParent)
                        {
                            Debug.Log($"  <color=green>✓ Correctly parented to CardHandParent</color>");
                        }
                        else
                        {
                            Debug.LogWarning($"  <color=yellow>⚠ Wrong parent! Expected: {cardRuntimeManager.cardHandParent.name}</color>");
                        }
                    }
                    
                    handVisuals.Add(cardObj);
                    
                    // Setup click handler (don't pass index - look it up dynamically!)
                    SetupCardClickHandler(cardObj);
                }
                else
                {
                    Debug.LogError($"<color=red>✗ Failed to create GameObject for: {drawnCard.cardName}</color>");
                }
                
                OnCardDrawn?.Invoke(drawnCard);
            }
        }
        
        Debug.Log($"<color=yellow>=== Draw complete: {hand.Count} cards in hand, {handVisuals.Count} visuals created ===</color>");
        
        // IMPORTANT: Wait for all draw animations to complete, then reposition all cards together
        // This ensures all cards use the same "total cards" in their position calculation
        float lastCardDelay = (count - 1) * 0.15f; // Delay of the last card
        
        // Get actual animation duration from config (don't hardcode!)
        float drawAnimDuration = 0.6f; // Default fallback
        if (animationManager != null && animationManager.config != null)
        {
            drawAnimDuration = animationManager.config.cardDrawDuration;
        }
        
        float totalWaitTime = lastCardDelay + drawAnimDuration + 0.2f; // Extra buffer to be safe
        
        LeanTween.delayedCall(gameObject, totalWaitTime, () => {
            if (cardRuntimeManager != null && handVisuals.Count > 0)
            {
                Debug.Log($"<color=green>All cards drawn! Repositioning {handVisuals.Count} cards to final positions...</color>");
                cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
                
                // IMPORTANT: Ensure ALL cards are interactable after draw completes
                foreach (GameObject cardObj in handVisuals)
                {
                    if (cardObj != null)
                    {
                        SetCardInteractable(cardObj, true);
                        Debug.Log($"  Re-enabled interaction for {cardObj.name}");
                    }
                }
            }
        });
    }
    
    /// <summary>
    /// Play a card from hand
    /// </summary>
    public void PlayCard(int handIndex, Vector3 targetPosition)
    {
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"Invalid hand index: {handIndex}");
            return;
        }
        
        Card card = hand[handIndex];
        GameObject cardObj = handVisuals[handIndex];
        
        // Remove from hand and visuals IMMEDIATELY
        // This prevents the card from being repositioned while animating
        hand.RemoveAt(handIndex);
        handVisuals.RemoveAt(handIndex);
        
        Debug.Log($"<color=yellow>Playing card: {card.cardName}</color>");
        Debug.Log($"  Card GameObject: {cardObj.name}");
        Debug.Log($"  Current position: {cardObj.transform.position}");
        
        // Reposition remaining cards with SMOOTH ANIMATION (squeeze together effect!)
        // Use handVisuals list (not activeCards) to ensure correct cards are repositioned
        if (cardRuntimeManager != null)
        {
            cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
        }
        
        Debug.Log($"  Repositioning complete. Starting play animation to {targetPosition}...");
        
        // CRITICAL: Cancel ALL LeanTween animations on this card!
        // There may be stale hover/draw/reposition animations still running
        LeanTween.cancel(cardObj);
        Debug.Log($"  ✓ Cancelled all LeanTween animations on {cardObj.name}");
        
        // ALSO disable CardHoverEffect and ALL interaction to prevent interference
        CardHoverEffect hoverEffect = cardObj.GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.enabled = false;
            Debug.Log($"  ✓ Disabled CardHoverEffect on {cardObj.name}");
        }
        
        // Disable button to prevent any click interference
        UnityEngine.UI.Button cardButton = cardObj.GetComponent<UnityEngine.UI.Button>();
        if (cardButton != null)
        {
            cardButton.enabled = false;
        }
        
        // Set CanvasGroup to block raycasts completely
        CanvasGroup cg = cardObj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
        
        // ANIMATION SEQUENCE:
        // 1. Fly to target (enemy/player)
        // 2. Perform card effect
        // 3. Fly to discard pile
        // 4. Disappear (return to pool)
        
        // Check if we have animation support
        if (animationManager == null)
        {
            Debug.LogError("CombatAnimationManager is NULL! Cannot animate card play!");
        }
        
        if (cardRuntimeManager != null && animationManager != null)
        {
            // Get target enemy for the card
            Enemy targetEnemy = GetTargetEnemy();
            Character player = characterManager != null && characterManager.HasCharacter() ? 
                characterManager.GetCurrentCharacter() : null;
            
            Debug.Log($"  Animation manager found. Flying card to enemy...");
            
            // Step 1: Animate to target position (play effect)
            // Use animationManager directly (not cardRuntimeManager.AnimateCardPlay which returns to pool!)
            animationManager.AnimateCardPlay(cardObj, targetPosition, () => {
                
                Debug.Log($"  <color=green>Card reached target! Applying effects...</color>");
                Debug.Log($"  Card still exists? {cardObj != null}, Active? {(cardObj != null ? cardObj.activeInHierarchy.ToString() : "null")}");
                
                // Step 2: Apply card effect (DEAL DAMAGE!)
                if (cardEffectProcessor != null && targetEnemy != null)
                {
                    cardEffectProcessor.ApplyCardToEnemy(card, targetEnemy, player, targetPosition);
                }
                else
                {
                    Debug.LogWarning($"Cannot apply {card.cardName}: No target enemy or effect processor!");
                }
                
                // Trigger event for other systems
                OnCardPlayed?.Invoke(card);
                Debug.Log($"  → Card effect triggered: {card.cardName}");
                
                // Step 3: After a brief pause, animate to discard pile
                float effectDuration = 0.3f; // Time to show the card effect
                
                // IMPORTANT: Capture cardObj in local variable to keep strong reference
                GameObject cardToDiscard = cardObj;
                
                LeanTween.delayedCall(gameObject, effectDuration, () => {
                    
                    Debug.Log($"  Delayed callback fired after {effectDuration}s. Checking card object...");
                    
                    // Safety check - make sure card object still exists
                    if (cardToDiscard == null)
                    {
                        Debug.LogError($"Card GameObject was destroyed before discard animation!");
                        Debug.Log($"  This means something else destroyed/pooled the card during the {effectDuration}s delay!");
                        discardPile.Add(card);
                        OnCardDiscarded?.Invoke(card);
                        return;
                    }
                    
                    Debug.Log($"  Card still exists: {cardToDiscard.name}, Active: {cardToDiscard.activeInHierarchy}");
                    
                    Debug.Log($"  → Starting discard animation for {card.cardName}...");
                    
                    if (discardPileTransform != null)
                    {
                        // Animate to discard pile
                        AnimateToDiscardPile(cardToDiscard, card);
                    }
                    else
                    {
                        // No discard pile position - just return to pool immediately
                        Debug.LogWarning("DiscardPileTransform not set! Card will disappear without animation.");
                        discardPile.Add(card);
                        OnCardDiscarded?.Invoke(card);
                        
                        if (cardRuntimeManager != null)
                        {
                            cardRuntimeManager.ReturnCardToPool(cardToDiscard);
                        }
                        else
                        {
                            Destroy(cardToDiscard);
                        }
                        
                        Debug.Log($"  → {card.cardName} discarded (no animation)");
                    }
                    
                    // NOTE: Remaining cards already repositioned earlier!
                });
            });
        }
        else
        {
            // No animation - immediate play
            discardPile.Add(card);
            OnCardPlayed?.Invoke(card);
            OnCardDiscarded?.Invoke(card);
            
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
            
            // Reposition remaining cards
            if (cardRuntimeManager != null)
            {
                cardRuntimeManager.RepositionAllCards();
            }
            
            Debug.Log($"Played (instant): {card.cardName}");
        }
    }
    
    /// <summary>
    /// Discard a card from hand
    /// </summary>
    public void DiscardCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"Invalid hand index: {handIndex}");
            return;
        }
        
        Card card = hand[handIndex];
        GameObject cardObj = handVisuals[handIndex];
        
        // Remove from hand
        hand.RemoveAt(handIndex);
        handVisuals.RemoveAt(handIndex);
        
        // Animate discard
        if (cardRuntimeManager != null)
        {
            cardRuntimeManager.AnimateCardDiscard(cardObj, () => {
                // Move to discard pile
                discardPile.Add(card);
                OnCardDiscarded?.Invoke(card);
                
                // Reposition remaining cards with smooth animation
                cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
            });
        }
        else
        {
            // Immediate discard
            discardPile.Add(card);
            OnCardDiscarded?.Invoke(card);
            
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
        }
        
        Debug.Log($"Discarded: {card.cardName}");
    }
    
    /// <summary>
    /// Discard entire hand
    /// </summary>
    public void DiscardHand()
    {
        while (hand.Count > 0)
        {
            DiscardCard(0);
        }
    }
    
    #endregion
    
    #region Card Interaction
    
    private void SetupCardClickHandler(GameObject cardObj)
    {
        // IMPORTANT: Disable DeckBuilderCardUI if it exists (it's for DeckBuilder scene, not Combat!)
        DeckBuilderCardUI deckBuilderUI = cardObj.GetComponent<DeckBuilderCardUI>();
        if (deckBuilderUI != null)
        {
            deckBuilderUI.enabled = false;
            Debug.Log($"Disabled DeckBuilderCardUI on {cardObj.name} for combat use");
        }
        
        UnityEngine.UI.Button button = cardObj.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnCardClicked(cardObj));
        }
    }
    
    private void OnCardClicked(GameObject clickedCardObj)
    {
        Debug.Log($"<color=cyan>═══ CARD CLICK DEBUG ═══</color>");
        Debug.Log($"Clicked card GameObject: {clickedCardObj.name}");
        Debug.Log($"Hand visuals count: {handVisuals.Count}");
        Debug.Log($"Hand data count: {hand.Count}");
        
        // Find the index of this card in the hand visuals
        int handIndex = handVisuals.IndexOf(clickedCardObj);
        
        Debug.Log($"Card index in handVisuals: {handIndex}");
        
        if (handIndex < 0)
        {
            Debug.LogWarning($"Clicked card not found in hand visuals!");
            Debug.Log($"Card active: {clickedCardObj.activeInHierarchy}");
            Debug.Log($"Card parent: {clickedCardObj.transform.parent?.name}");
            return;
        }
        
        if (handIndex >= hand.Count)
        {
            Debug.LogWarning($"Hand index out of range! Index: {handIndex}, Hand count: {hand.Count}");
            return;
        }
        
        // Get the card data
        Card clickedCard = hand[handIndex];
        
        // Get target position from first available enemy
        Vector3 targetPos = GetTargetScreenPosition();
        
        Debug.Log($"<color=yellow>Card clicked: {clickedCard.cardName} (Index: {handIndex}), Target pos: {targetPos}</color>");
        Debug.Log($"About to call PlayCard({handIndex}, {targetPos})");
        
        // Play the card
        PlayCard(handIndex, targetPos);
        
        Debug.Log($"<color=cyan>═══ END CLICK DEBUG ═══</color>");
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Create a card with draw animation from deck pile.
    /// </summary>
    private GameObject CreateAnimatedCard(Card cardData, Character player, int cardIndex)
    {
        // Create the card visual
        GameObject cardObj = cardRuntimeManager.CreateCardFromData(cardData, player);
        if (cardObj == null) return null;
        
        // IMMEDIATELY disable interaction BEFORE positioning
        // This prevents hover from triggering if mouse is already at deck pile
        SetCardInteractable(cardObj, false);
        
        // ALSO cancel any active tweens on this card (in case of rapid creation)
        LeanTween.cancel(cardObj);
        
        // Animate from deck pile if available
        if (animationManager != null && deckPileTransform != null)
        {
            Vector3 startPos = deckPileTransform.position;
            
            // IMPORTANT: Calculate the FINAL position in hand (where card should end up)
            // We need to know total number of cards that will be in hand
            int totalCardsInHand = hand.Count; // hand.Count already includes this card
            int thisCardIndex = hand.Count - 1; // This card's index in the hand
            
            // Calculate where this card SHOULD be positioned
            Vector3 finalPosition = CalculateFinalCardPosition(thisCardIndex, totalCardsInHand);
            
            // Calculate staggered delay for multiple cards
            float delay = cardIndex * 0.15f; // 0.15s between each card
            
            // Capture the FINAL scale BEFORE we modify it (this was set by CreateCardFromData)
            Vector3 targetScale = cardObj.transform.localScale;
            
            // Start card at deck position, small and rotated
            cardObj.transform.position = startPos;
            cardObj.transform.localScale = targetScale * 0.3f; // Start at 30% of final scale
            cardObj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
            
            // Animate after delay, then ENABLE interaction when done
            if (delay > 0f)
            {
                LeanTween.delayedCall(delay, () => {
                    animationManager.AnimateCardDraw(cardObj, startPos, finalPosition, targetScale, () => {
                        // Animation complete - enable interaction!
                        SetCardInteractable(cardObj, true);
                    });
                });
            }
            else
            {
                animationManager.AnimateCardDraw(cardObj, startPos, finalPosition, targetScale, () => {
                    // Animation complete - enable interaction!
                    SetCardInteractable(cardObj, true);
                });
            }
        }
        else
        {
            // No animation - enable immediately
            SetCardInteractable(cardObj, true);
        }
        
        return cardObj;
    }
    
    /// <summary>
    /// Calculate the final position for a card in hand.
    /// Uses CardRuntimeManager's calculation directly to ensure perfect match!
    /// </summary>
    private Vector3 CalculateFinalCardPosition(int cardIndex, int totalCards)
    {
        if (cardRuntimeManager == null)
        {
            return Vector3.zero;
        }
        
        // Use CardRuntimeManager's calculation directly - guaranteed to match!
        return cardRuntimeManager.CalculateCardPosition(cardIndex, totalCards);
    }
    
    /// <summary>
    /// Enable or disable card interaction (button, hover effects).
    /// </summary>
    private void SetCardInteractable(GameObject cardObj, bool interactable)
    {
        if (cardObj == null) return;
        
        // Use CanvasGroup to completely block raycasts when disabled
        // This prevents ANY pointer events from reaching the card
        CanvasGroup canvasGroup = cardObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = cardObj.AddComponent<CanvasGroup>();
        }
        
        if (interactable)
        {
            // Enable - allow raycasts and full interaction
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        else
        {
            // Disable - BLOCK all raycasts (no hover, no clicks)
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        // Also disable/enable button
        UnityEngine.UI.Button button = cardObj.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.interactable = interactable;
        }
        
        // Also disable/enable hover effect
        CardHoverEffect hoverEffect = cardObj.GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.enabled = interactable;
        }
    }
    
    /// <summary>
    /// Get the current target enemy from targeting system.
    /// </summary>
    private Enemy GetTargetEnemy()
    {
        if (targetingManager != null)
        {
            return targetingManager.GetTargetedEnemy();
        }
        
        // Fallback: Use first available
        if (cardEffectProcessor != null)
        {
            return cardEffectProcessor.GetFirstAvailableEnemy();
        }
        
        return null;
    }
    
    /// <summary>
    /// Get screen position for target enemy (for animation).
    /// </summary>
    private Vector3 GetTargetScreenPosition()
    {
        if (targetingManager != null)
        {
            return targetingManager.GetTargetedEnemyPosition();
        }
        
        // Fallback: Use first enemy
        Enemy targetEnemy = GetTargetEnemy();
        if (targetEnemy != null && cardEffectProcessor != null)
        {
            return cardEffectProcessor.GetEnemyScreenPosition(targetEnemy);
        }
        
        // Final fallback position
        return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
    }
    
    /// <summary>
    /// Animate card flying to discard pile and disappearing.
    /// </summary>
    private void AnimateToDiscardPile(GameObject cardObj, Card card)
    {
        if (cardObj == null)
        {
            Debug.LogError($"Cannot animate discard - cardObj is null for {card.cardName}!");
            discardPile.Add(card);
            OnCardDiscarded?.Invoke(card);
            return;
        }
        
        if (discardPileTransform == null)
        {
            Debug.LogWarning("DiscardPileTransform not set! Using instant discard.");
            discardPile.Add(card);
            OnCardDiscarded?.Invoke(card);
            
            if (cardRuntimeManager != null)
            {
                cardRuntimeManager.ReturnCardToPool(cardObj);
            }
            else
            {
                Destroy(cardObj);
            }
            return;
        }
        
        Vector3 discardPos = discardPileTransform.position;
        Vector3 startPos = cardObj.transform.position;
        
        Debug.Log($"  → Animating {card.cardName} from {startPos} to discard pile at {discardPos}...");
        
        // Cancel any existing animations on this card
        LeanTween.cancel(cardObj);
        
        // Use a counter to ensure all animations complete
        int animationsCompleted = 0;
        int totalAnimations = 3; // move, scale, rotate
        
        System.Action OnAnimationComplete = () => {
            animationsCompleted++;
            Debug.Log($"  → Animation step {animationsCompleted}/{totalAnimations} complete for {card.cardName}");
            
            // Only trigger cleanup when ALL animations complete
            if (animationsCompleted >= totalAnimations)
            {
                Debug.Log($"  → <color=green>All 3 animations complete for {card.cardName}, returning to pool...</color>");
                
                // Safety check
                if (cardObj == null)
                {
                    Debug.LogError($"Card GameObject became null during discard animation!");
                    discardPile.Add(card);
                    OnCardDiscarded?.Invoke(card);
                    return;
                }
                
                Debug.Log($"  → Card position before pool return: {cardObj.transform.position}");
                Debug.Log($"  → Card scale before pool return: {cardObj.transform.localScale}");
                Debug.Log($"  → Card active: {cardObj.activeInHierarchy}");
                
                // Add to discard pile
                discardPile.Add(card);
                OnCardDiscarded?.Invoke(card);
                
                // Return to pool
                if (cardRuntimeManager != null)
                {
                    cardRuntimeManager.ReturnCardToPool(cardObj);
                    Debug.Log($"  → <color=green>✓✓✓ {card.cardName} RETURNED TO POOL! ✓✓✓</color>");
                }
                else
                {
                    Destroy(cardObj);
                    Debug.Log($"  → {card.cardName} destroyed (no pool manager)");
                }
            }
        };
        
        // Animate to discard pile (position)
        LeanTween.move(cardObj, discardPos, 0.4f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(OnAnimationComplete);
        
        // Shrink while moving
        LeanTween.scale(cardObj, Vector3.one * 0.2f, 0.4f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(OnAnimationComplete);
        
        // Rotate while moving
        LeanTween.rotate(cardObj, new Vector3(0, 0, Random.Range(-30f, 30f)), 0.4f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(OnAnimationComplete);
    }
    
    #endregion
    
    #region Getters
    
    public List<Card> GetHand()
    {
        return new List<Card>(hand);
    }
    
    public List<Card> GetDrawPile()
    {
        return new List<Card>(drawPile);
    }
    
    public List<Card> GetDiscardPile()
    {
        return new List<Card>(discardPile);
    }
    
    public int GetHandCount()
    {
        return hand.Count;
    }
    
    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }
    
    public int GetDiscardPileCount()
    {
        return discardPile.Count;
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Load Marauder Deck")]
    private void LoadMarauderDeck()
    {
        LoadDeckForClass("Marauder");
    }
    
    [ContextMenu("Load Witch Deck")]
    private void LoadWitchDeck()
    {
        LoadDeckForClass("Witch");
    }
    
    [ContextMenu("Load Ranger Deck")]
    private void LoadRangerDeck()
    {
        LoadDeckForClass("Ranger");
    }
    
    [ContextMenu("Load Brawler Deck")]
    private void LoadBrawlerDeck()
    {
        LoadDeckForClass("Brawler");
    }
    
    [ContextMenu("Load Thief Deck")]
    private void LoadThiefDeck()
    {
        LoadDeckForClass("Thief");
    }
    
    [ContextMenu("Load Apostle Deck")]
    private void LoadApostleDeck()
    {
        LoadDeckForClass("Apostle");
    }
    
    [ContextMenu("Shuffle Deck")]
    private void DebugShuffleDeck()
    {
        ShuffleDeck();
    }
    
    [ContextMenu("Draw Initial Hand")]
    private void DebugDrawInitialHand()
    {
        DrawInitialHand();
    }
    
    [ContextMenu("Play First Card")]
    private void DebugPlayFirstCard()
    {
        if (hand.Count == 0)
        {
            Debug.LogWarning("No cards in hand to play!");
            return;
        }
        
        Debug.Log($"<color=yellow>Testing card play: {hand[0].cardName}</color>");
        Vector3 targetPos = GetTargetScreenPosition();
        PlayCard(0, targetPos);
    }
    
    [ContextMenu("Check Discard Setup")]
    private void DebugCheckDiscardSetup()
    {
        Debug.Log("=== DISCARD PILE SETUP CHECK ===");
        Debug.Log($"Discard Pile Transform: {(discardPileTransform != null ? discardPileTransform.name : "NOT SET!")}");
        if (discardPileTransform != null)
        {
            Debug.Log($"  Position: {discardPileTransform.position}");
        }
        Debug.Log($"Card Runtime Manager: {(cardRuntimeManager != null ? "Found" : "NULL!")}");
        Debug.Log($"Animation Manager: {(animationManager != null ? "Found" : "NULL!")}");
    }
    
    [ContextMenu("Reposition Cards (Test Settings)")]
    private void DebugRepositionCards()
    {
        if (cardRuntimeManager != null)
        {
            Debug.Log("<color=yellow>Repositioning cards with current settings...</color>");
            cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.5f);
        }
        else
        {
            Debug.LogError("CardRuntimeManager not found!");
        }
    }
    
    [ContextMenu("Check Card Interactability")]
    private void DebugCheckCardInteractability()
    {
        Debug.Log("=== CARD INTERACTABILITY CHECK ===");
        Debug.Log($"Total cards in hand: {handVisuals.Count}");
        
        for (int i = 0; i < handVisuals.Count; i++)
        {
            GameObject cardObj = handVisuals[i];
            if (cardObj != null)
            {
                UnityEngine.UI.Button button = cardObj.GetComponent<UnityEngine.UI.Button>();
                CardHoverEffect hover = cardObj.GetComponent<CardHoverEffect>();
                CanvasGroup canvasGroup = cardObj.GetComponent<CanvasGroup>();
                
                Debug.Log($"\nCard {i}: {cardObj.name}");
                Debug.Log($"  Active: {cardObj.activeInHierarchy}");
                Debug.Log($"  Position: {cardObj.transform.position}");
                Debug.Log($"  Button interactable: {(button != null ? button.interactable.ToString() : "No button")}");
                Debug.Log($"  Hover enabled: {(hover != null ? hover.enabled.ToString() : "No hover")}");
                Debug.Log($"  CanvasGroup blocks raycasts: {(canvasGroup != null ? (!canvasGroup.blocksRaycasts).ToString() : "No CanvasGroup")}");
                
                if (canvasGroup != null && !canvasGroup.blocksRaycasts)
                {
                    Debug.LogWarning($"  ⚠ Card {i} has raycasts BLOCKED! Might be unclickable!");
                }
            }
        }
    }
    
    [ContextMenu("Draw 1 Card")]
    private void DebugDrawCard()
    {
        DrawCards(1);
    }
    
    [ContextMenu("Clear Hand")]
    private void DebugClearHand()
    {
        cardRuntimeManager?.ClearAllCards();
        hand.Clear();
        handVisuals.Clear();
    }
    
    [ContextMenu("Show Deck Stats")]
    private void ShowDeckStats()
    {
        Debug.Log($"<color=cyan>Deck Statistics:</color>\n" +
                  $"Draw Pile: {drawPile.Count} cards\n" +
                  $"Hand: {hand.Count} cards\n" +
                  $"Discard Pile: {discardPile.Count} cards\n" +
                  $"Total: {drawPile.Count + hand.Count + discardPile.Count} cards");
    }
    
    #endregion
}

