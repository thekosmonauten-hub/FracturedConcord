using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the card deck during combat.
/// Handles loading, shuffling, drawing, and discarding cards.
/// Integrates with CardRuntimeManager for visual display.
/// Now uses CardData ScriptableObjects instead of JSON for better reliability.
/// </summary>
public class CombatDeckManager : MonoBehaviour
{
    public static CombatDeckManager Instance { get; private set; }
    
    [Header("Deck Settings")]
    [SerializeField] private bool loadDeckOnStart = true;
    [SerializeField] private int initialHandSize = 5;
    private bool hasDrawnInitialHand = false; // Track if initial hand has been drawn
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
    [SerializeField] private CombatEffectManager combatEffectManager;
    
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
    
    // Deck piles - NOW USING CardDataExtended (no conversion!)
    private List<CardDataExtended> drawPile = new List<CardDataExtended>();
    private List<CardDataExtended> hand = new List<CardDataExtended>();
    private List<CardDataExtended> discardPile = new List<CardDataExtended>();
    private List<GameObject> handVisuals = new List<GameObject>();
    
    // Events
    public System.Action<CardDataExtended> OnCardDrawn;
    public System.Action<CardDataExtended> OnCardPlayed;
    public System.Action<CardDataExtended> OnCardDiscarded;
    public System.Action OnDeckShuffled;
    
    // Combat Manager Integration
    private CombatDisplayManager combatDisplayManager;
    private ComboSystem comboSystem;
    
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
            
        if (combatEffectManager == null)
            combatEffectManager = CombatEffectManager.Instance;
        
        if (cardEffectProcessor == null)
            cardEffectProcessor = CardEffectProcessor.Instance;
        
        if (targetingManager == null)
            targetingManager = EnemyTargetingManager.Instance;
        
        // Debug manager initialization
        Debug.Log($"<color=yellow>CombatDeckManager Initialization:</color>");
        Debug.Log($"  CardEffectProcessor: {(cardEffectProcessor != null ? "Found" : "NULL")}");
        Debug.Log($"  TargetingManager: {(targetingManager != null ? "Found" : "NULL")}");
        Debug.Log($"  CombatEffectManager: {(combatEffectManager != null ? "Found" : "NULL")}");
        
        // Auto-find CombatDisplayManager
        combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        
        // Auto-find ComboSystem
        comboSystem = ComboSystem.Instance;
        if (comboSystem == null)
        {
            // Create ComboSystem if it doesn't exist
            GameObject comboSystemObj = new GameObject("ComboSystem");
            comboSystem = comboSystemObj.AddComponent<ComboSystem>();
        }
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
        
        // Load deck from CardDatabase as CardDataExtended - NO CONVERSION!
        List<CardDataExtended> cardDataDeck = LoadDeckFromCardDatabase(characterClass);
        
        if (cardDataDeck != null && cardDataDeck.Count > 0)
        {
            // NO CONVERSION NEEDED! Use CardDataExtended directly!
            foreach (CardDataExtended card in cardDataDeck)
            {
                // Add multiple copies based on typical deck composition
                int copies = GetCardCopies(card.cardName);
                for (int i = 0; i < copies; i++)
                {
                    drawPile.Add(card);
                }
                Debug.Log($"[CardDataExtended] Added {copies} copies of {card.cardName} to deck");
            }
            
            Debug.Log($"<color=green>✓ Loaded {characterClass} deck from CardDatabase:</color> {drawPile.Count} cards");
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
        
        foreach (CardDataExtended card in drawPile)
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
    
    /// <summary>
    /// Load deck from CardDatabase as CardDataExtended (no conversion!)
    /// </summary>
    private List<CardDataExtended> LoadDeckFromCardDatabase(string characterClass)
    {
        Debug.Log($"<color=cyan>[CardDataExtended] Loading {characterClass} deck from CardDatabase...</color>");
        
        CardDatabase database = CardDatabase.Instance;
        if (database == null)
        {
            Debug.LogError($"[CardDataExtended] CardDatabase not found! Cannot load {characterClass} deck.");
            return null;
        }
        
        Debug.Log($"[CardDataExtended] CardDatabase loaded with {database.allCards.Count} total cards");
        
        // Get class cards from database as CardDataExtended
        List<CardDataExtended> classCards = GetClassCardsFromDatabase(database, characterClass);
        
        if (classCards.Count == 0)
        {
            Debug.LogWarning($"[CardDataExtended] No {characterClass} cards found in CardDatabase. Run migration tool: Tools > Cards > Migrate to CardDataExtended");
            return null;
        }
        
        Debug.Log($"[CardDataExtended] Found {classCards.Count} {characterClass} cards in CardDatabase");
        return classCards;
    }
    
    /// <summary>
    /// Get class cards from database as CardDataExtended
    /// </summary>
    private List<CardDataExtended> GetClassCardsFromDatabase(CardDatabase database, string characterClass)
    {
        List<CardDataExtended> classCards = new List<CardDataExtended>();
        
        // Get card names for this class
        string[] cardNames = GetCardNamesForClass(characterClass);
        
        foreach (string cardName in cardNames)
        {
            // Try to find CardDataExtended first (preferred)
            CardData cardData = database.allCards.Find(c => c.cardName == cardName);
            
            if (cardData != null)
            {
                // Check if it's already CardDataExtended
                CardDataExtended cardExtended = cardData as CardDataExtended;
                
                if (cardExtended != null)
                {
                    classCards.Add(cardExtended);
                    Debug.Log($"[CardDataExtended] ✓ Found extended card: {cardExtended.cardName}");
                }
                else
                {
                    // It's a regular CardData - log warning that migration is needed
                    Debug.LogWarning($"[CardDataExtended] ⚠ Card '{cardName}' is CardData, not CardDataExtended! Run migration tool.");
                    
                    // For backward compatibility during migration, try to load the _Extended version directly
                    CardDataExtended migratedCard = LoadCardDataExtendedDirectly(cardName);
                    if (migratedCard != null)
                    {
                        classCards.Add(migratedCard);
                        Debug.Log($"[CardDataExtended] ✓ Found migrated version: {cardName}_Extended");
                    }
                }
            }
            else
            {
                // Try direct load as fallback
                CardDataExtended directCard = LoadCardDataExtendedDirectly(cardName);
                if (directCard != null)
                {
                    classCards.Add(directCard);
                    Debug.Log($"[CardDataExtended] ✓ Loaded directly: {cardName}");
                }
                else
                {
                    Debug.LogError($"[CardDataExtended] ✗ Card '{cardName}' not found in database or Resources!");
                }
            }
        }
        
        return classCards;
    }
    
    /// <summary>
    /// Get card names for a specific class
    /// </summary>
    private string[] GetCardNamesForClass(string characterClass)
    {
        switch (characterClass)
        {
            case "Marauder":
                return new string[] {
                    "Heavy Strike",
                    "Brace",
                    "Ground Slam",
                    "Cleave",
                    "Endure",
                    "Intimidating Shout"
                };
            case "Witch":
                return new string[] { 
                    // TODO: Add Witch cards
                };
            case "Ranger":
                return new string[] { 
                    // TODO: Add Ranger cards
                };
            case "Brawler":
                return new string[] {
                    // TODO: Add Brawler cards
                };
            case "Thief":
                return new string[] {
                    // TODO: Add Thief cards
                };
            case "Apostle":
                return new string[] {
                    // TODO: Add Apostle cards
                };
            default:
                Debug.LogWarning($"No card list defined for class: {characterClass}");
                return new string[] { };
        }
    }
    
    /// <summary>
    /// Load CardDataExtended directly from Resources folder (fallback method)
    /// </summary>
    private CardDataExtended LoadCardDataExtendedDirectly(string cardName)
    {
        // Try different possible paths, including _Extended suffix
        string[] possiblePaths = {
            $"Cards/Marauder/{cardName.Replace(" ", "")}_Extended",
            $"Cards/Marauder/{cardName}_Extended",
            $"Cards/Marauder/{cardName.Replace(" ", "")}",
            $"Cards/Marauder/{cardName}",
            $"Cards/{cardName.Replace(" ", "")}_Extended",
            $"Cards/{cardName}_Extended",
            $"Cards/{cardName.Replace(" ", "")}",
            $"Cards/{cardName}"
        };
        
        foreach (string path in possiblePaths)
        {
            CardDataExtended card = Resources.Load<CardDataExtended>(path);
            if (card != null)
            {
                Debug.Log($"[CardDataExtended] Successfully loaded card from: {path}");
                return card;
            }
        }
        
        Debug.LogWarning($"[CardDataExtended] Could not find CardDataExtended at any path for: {cardName}");
        return null;
    }
    
    /// <summary>
    /// Get number of copies for each card (replicating JSON deck composition)
    /// </summary>
    private int GetCardCopies(string cardName)
    {
        switch (cardName)
        {
            case "Heavy Strike": return 6;  // 6 copies in JSON
            case "Brace": return 4;         // 4 copies in JSON
            case "Ground Slam": return 2;   // 2 copies in JSON
            case "Cleave": return 2;        // 2 copies in JSON
            case "Endure": return 2;        // 2 copies in JSON
            case "Intimidating Shout": return 2; // 2 copies in JSON
            default: return 1; // Default to 1 copy
        }
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
            CardDataExtended temp = drawPile[i]; // NOW USING CardDataExtended!
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
        if (!hasDrawnInitialHand)
        {
            DrawCards(initialHandSize);
            hasDrawnInitialHand = true;
            Debug.Log($"<color=green>Initial hand drawn: {initialHandSize} cards</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>Initial hand already drawn, skipping</color>");
        }
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
                // Draw card from deck - NOW USING CardDataExtended!
                CardDataExtended drawnCard = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(drawnCard);
                
                Debug.Log($"<color=cyan>Drawing card #{i+1}: {drawnCard.cardName}</color>");
                
                // Create visual card with draw animation - NO CONVERSION!
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
                
                // Update combo glow states after draw completes
                UpdateComboHighlights();
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
        
        CardDataExtended card = hand[handIndex]; // NOW USING CardDataExtended!
        GameObject cardObj = handVisuals[handIndex];
        
        // Check if player has enough mana to play this card
        Character player = CharacterManager.Instance?.GetCurrentCharacter();
        if (player == null)
        {
            Debug.LogError("No player character found!");
            return;
        }
        
        if (player.mana < card.playCost) // Use playCost from CardDataExtended
        {
            Debug.Log($"<color=red>Cannot play {card.cardName}: Not enough mana! Required: {card.playCost}, Available: {player.mana}</color>");
            
            // Animate the card to show it can't be played
            AnimateInsufficientManaCard(cardObj);
            
            // Flash the End Turn button to indicate player can't afford cards
            FlashEndTurnButton();
            return;
        }
        
        // Deduct mana cost before playing the card
        bool manaSpent = player.UseMana(card.playCost); // Use playCost from CardDataExtended
        if (!manaSpent)
        {
            Debug.LogError($"Failed to spend mana for {card.cardName}!");
            return;
        }
        
        Debug.Log($"<color=green>Spent {card.playCost} mana to play {card.cardName}. Remaining mana: {player.mana}</color>");
        
        // Build combo application (if any) BEFORE applying effects
        ComboSystem.ComboApplication comboApp = null;
        if (ComboSystem.Instance != null)
        {
            Character comboPlayer = characterManager != null && characterManager.HasCharacter() ? 
                characterManager.GetCurrentCharacter() : null;
            comboApp = ComboSystem.Instance.BuildComboApplication(card, comboPlayer);
            if (comboApp != null)
            {
                Debug.Log($"<color=yellow>[Combo] Combo will apply to {card.cardName}: Logic={comboApp.logic}, +Atk={comboApp.attackIncrease}, +Guard={comboApp.guardIncrease}, AoE={comboApp.isAoEOverride}, ManaRefund={comboApp.manaRefund}</color>");
            }
        }
        
        // Update mana display and card usability
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            playerDisplay.UpdateManaDisplay();
        }
        UpdateCardUsability();
        
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
            Character playerCharacter = characterManager != null && characterManager.HasCharacter() ? 
                characterManager.GetCurrentCharacter() : null;
            
            Debug.Log($"  Animation manager found. Flying card to enemy...");
            
            // Step 1: Animate to target position (play effect)
            // Use animationManager directly (not cardRuntimeManager.AnimateCardPlay which returns to pool!)
            animationManager.AnimateCardPlay(cardObj, targetPosition, () => {
                
                Debug.Log($"  <color=green>Card reached target! Applying effects...</color>");
                Debug.Log($"  Card still exists? {cardObj != null}, Active? {(cardObj != null ? cardObj.activeInHierarchy.ToString() : "null")}");
                
                // Step 2: Apply card effect (DEAL DAMAGE!)
                Debug.Log($"<color=cyan>Applying card effects for {card.cardName}...</color>");
                Debug.Log($"  CardEffectProcessor: {(cardEffectProcessor != null ? "Found" : "NULL")}");
                Debug.Log($"  TargetingManager: {(targetingManager != null ? "Found" : "NULL")}");
                Debug.Log($"  Target Enemy: {(targetEnemy != null ? targetEnemy.enemyName : "NULL")}");
                Debug.Log($"  Player Character: {(playerCharacter != null ? playerCharacter.characterName : "NULL")}");
                Debug.Log($"  Is AoE Card: {card.isAoE}");
                
                if (cardEffectProcessor != null)
                {
                    // TEMPORARY: Convert CardDataExtended to Card for CardEffectProcessor
                    // (CardEffectProcessor will be updated to use CardDataExtended in the future)
                    #pragma warning disable CS0618 // Type or member is obsolete
                    Card cardForProcessor = card.ToCard();
                    #pragma warning restore CS0618
                    
                    // Apply combo logic modifications to the runtime Card passed to processor
                    if (comboApp != null)
                    {
                        if (comboApp.logic == ComboLogicType.Instead)
                        {
                            // Replace base values entirely
                            cardForProcessor.baseDamage = Mathf.Max(0f, comboApp.attackIncrease);
                            cardForProcessor.baseGuard = Mathf.Max(0f, comboApp.guardIncrease);
                            cardForProcessor.isAoE = comboApp.isAoEOverride;
                        }
                        else // Additive
                        {
                            cardForProcessor.baseDamage = Mathf.Max(0f, cardForProcessor.baseDamage + comboApp.attackIncrease);
                            cardForProcessor.baseGuard = Mathf.Max(0f, cardForProcessor.baseGuard + comboApp.guardIncrease);
                            // Only override AoE if true, otherwise keep original
                            cardForProcessor.isAoE = comboApp.isAoEOverride || cardForProcessor.isAoE;
                        }
                        
                        // Pass ailment fields from combo override
                        cardForProcessor.comboAilmentId = comboApp.comboAilmentId;
                        cardForProcessor.comboAilmentPortion = comboApp.comboAilmentPortion;
                        cardForProcessor.comboAilmentDuration = comboApp.comboAilmentDuration;
                        
                        // If combo-specific consume enabled on asset, propagate it for this play
                        if (card.comboConsumeAilment && card.comboConsumeAilmentId != AilmentId.None)
                        {
                            cardForProcessor.consumeAilmentEnabled = true;
                            cardForProcessor.consumeAilmentId = card.comboConsumeAilmentId;
                        }
                    }
                    else
                    {
                        // No combo override: allow per-card ailment application on play
                        if (card.comboAilment != AilmentId.None && card.comboAilmentPortion > 0f && card.comboAilmentDuration > 0)
                        {
                            cardForProcessor.comboAilmentId = card.comboAilment;
                            cardForProcessor.comboAilmentPortion = card.comboAilmentPortion;
                            cardForProcessor.comboAilmentDuration = card.comboAilmentDuration;
                        }
                    }
                    
                    // Always pass consume fields directly from the asset (not combo-only)
                    cardForProcessor.consumeAilmentEnabled = card.consumeAilment;
                    cardForProcessor.consumeAilmentId = card.consumeAilmentId;
                    
                    // For AoE cards, we don't need a specific target enemy
                    if (cardForProcessor.isAoE)
                    {
                        Debug.Log($"  → Calling cardEffectProcessor.ApplyCardToEnemy for AoE card...");
                        cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, null, playerCharacter, targetPosition);
                    }
                    else if (targetEnemy != null)
                    {
                        Debug.Log($"  → Calling cardEffectProcessor.ApplyCardToEnemy for single target...");
                        cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, targetEnemy, playerCharacter, targetPosition);
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot apply {card.cardName}: No target enemy for single-target card!");
                        return;
                    }
                    
                    // Play combat effects based on card type
                    if (combatEffectManager != null)
                    {
                        Debug.Log($"  → Calling PlayCardEffects...");
                        // Pass CardDataExtended to visual effects (legacy Card not needed here)
                        PlayCardEffects(card, targetEnemy, playerCharacter);
                    }
                    else
                    {
                        Debug.LogWarning($"  → CombatEffectManager is NULL!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Cannot apply {card.cardName}: No effect processor!");
                }
                
                // Trigger event for other systems
                OnCardPlayed?.Invoke(card);
                Debug.Log($"  → Card effect triggered: {card.cardName}");
                
                // Apply combo post-effects: mana refund, draw, ailments, buffs
                if (comboApp != null)
                {
                    // Mana refund
                    if (comboApp.manaRefund > 0 && playerCharacter != null)
                    {
                        playerCharacter.RestoreMana(comboApp.manaRefund);
                        Debug.Log($"<color=green>[Combo] Refunded {comboApp.manaRefund} mana</color>");
                        var playerDisplayRefund = FindFirstObjectByType<PlayerCombatDisplay>();
                        if (playerDisplayRefund != null) playerDisplayRefund.UpdateManaDisplay();
                    }
                    
                    // Draw cards on combo
                    if (comboApp.comboDrawCards > 0)
                    {
                        Debug.Log($"<color=green>[Combo] Draw {comboApp.comboDrawCards} card(s)</color>");
                        DrawCards(comboApp.comboDrawCards);
                    }
                    
                    // Ailment (hook into status/effect manager)
                    if (!string.IsNullOrEmpty(comboApp.ailmentId) && targetEnemy != null)
                    {
                        Debug.Log($"[Combo] Apply ailment: {comboApp.ailmentId} to {targetEnemy.enemyName}");
                        // TODO: integrate with StatusEffectManager once ailment mapping is defined
                    }
                    
                    // Buffs on player (list)
                    if (comboApp.buffIds != null && comboApp.buffIds.Count > 0)
                    {
                        Debug.Log($"[Combo] Apply buffs to player: {string.Join(", ", comboApp.buffIds)}");
                        // Simple integration: support Bolster stacks (2-turn default)
                        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
                        var statusManager = playerDisplay != null ? playerDisplay.GetStatusEffectManager() : null;
                        if (statusManager != null)
                        {
                            foreach (var buff in comboApp.buffIds)
                            {
                                if (string.Equals(buff, "Bolster", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    int stacks = 1; // base stack
                                    if (playerCharacter != null && card != null)
                                    {
                                        switch (card.comboScaling)
                                        {
                                            case ComboScalingType.Strength:
                                                if (card.comboScalingDivisor > 0f)
                                                {
                                                    stacks += Mathf.FloorToInt(playerCharacter.strength / card.comboScalingDivisor);
                                                }
                                                break;
                                            case ComboScalingType.Dexterity:
                                                if (card.comboScalingDivisor > 0f)
                                                {
                                                    stacks += Mathf.FloorToInt(playerCharacter.dexterity / card.comboScalingDivisor);
                                                }
                                                break;
                                            case ComboScalingType.Intelligence:
                                                if (card.comboScalingDivisor > 0f)
                                                {
                                                    stacks += Mathf.FloorToInt(playerCharacter.intelligence / card.comboScalingDivisor);
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    // Clamp effective benefit elsewhere, but ensure at least 1
                                    stacks = Mathf.Max(1, stacks);
                                    var effect = new StatusEffect(StatusEffectType.Bolster, "Bolster", stacks, 2, false);
                                    statusManager.AddStatusEffect(effect);
                                    Debug.Log($"[Combo] Applied Bolster ({stacks} stack(s), 2 turns)");
                                }
                            }
                        }
                    }
                }
                
                // Trigger player sprite animations based on card type
                TriggerPlayerAnimation(card);
                
                // Notify combat manager that a card was played
                if (combatDisplayManager != null)
                {
                    combatDisplayManager.OnCardPlayed();
                }
                
                // Notify combo system that a card was played (legacy), and register last played for new system
                if (comboSystem != null)
                {
                    // TEMPORARY: Convert CardDataExtended to Card for ComboSystem
                    #pragma warning disable CS0618
                    Card cardForCombo = card.ToCard();
                    #pragma warning restore CS0618
                    comboSystem.OnCardPlayed(cardForCombo);
                }
                if (ComboSystem.Instance != null)
                {
                    ComboSystem.Instance.RegisterLastPlayed(card.GetCardTypeEnum(), card.cardName);
                    // Refresh highlights after registering last played
                    UpdateComboHighlights();
                }
                
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
            
            // Notify combat manager that a card was played
            if (combatDisplayManager != null)
            {
                combatDisplayManager.OnCardPlayed();
            }
            
            // Notify combo system that a card was played
            if (comboSystem != null)
            {
                // TEMPORARY: Convert CardDataExtended to Card for ComboSystem
                #pragma warning disable CS0618 // Type or member is obsolete
                Card cardForCombo = card.ToCard();
                #pragma warning restore CS0618
                comboSystem.OnCardPlayed(cardForCombo);
            }
            
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
        UpdateComboHighlights(); // Update highlights after playing
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
        
        CardDataExtended card = hand[handIndex]; // NOW USING CardDataExtended!
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
        
        // Refresh highlights after discard
        UpdateComboHighlights();
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
        
        // Get the card data - NOW USING CardDataExtended!
        CardDataExtended clickedCard = hand[handIndex];
        
        // Get target position from first available enemy
        Vector3 targetPos = GetTargetScreenPosition();
        
        Debug.Log($"<color=yellow>Card clicked: {clickedCard.cardName} (Index: {handIndex}), Target pos: {targetPos}</color>");
        Debug.Log($"About to call PlayCard({handIndex}, {targetPos})");
        
        // Play the card
        PlayCard(handIndex, targetPos);
        
        Debug.Log($"<color=cyan>═══ END CLICK DEBUG ═══</color>");
    }
    
    /// <summary>
    /// Update combo highlight glow for all cards in hand.
    /// Requires each card visual prefab to contain a child Image named "GlowEffect" or "ComboGlow".
    /// </summary>
    private void UpdateComboHighlights()
    {
        if (ComboSystem.Instance == null) return;
        Character player = characterManager != null && characterManager.HasCharacter() ? characterManager.GetCurrentCharacter() : null;
        
        int count = Mathf.Min(hand.Count, handVisuals.Count);
        for (int i = 0; i < count; i++)
        {
            var cardData = hand[i];
            var cardObj = handVisuals[i];
            if (cardData == null || cardObj == null) continue;
            
            var app = ComboSystem.Instance.BuildComboApplication(cardData, player);
            bool eligible = app != null;
            
            // Find GlowEffect child recursively (it's nested under VisualRoot)
            Transform t = FindChildRecursive(cardObj.transform, "GlowEffect");
            if (t == null)
            {
                t = FindChildRecursive(cardObj.transform, "ComboGlow");
            }
            
            if (t != null)
            {
                var img = t.GetComponent<UnityEngine.UI.Image>();
                if (img != null && ComboSystem.Instance != null)
                {
                    img.color = ComboSystem.Instance.comboHighlightColor;
                }
                t.gameObject.SetActive(eligible);
                Debug.Log($"[ComboGlow] {cardData.cardName}: GlowEffect set to {(eligible ? "ACTIVE" : "INACTIVE")}");
                
                // Attach pulse effect if present
                var pulse = t.GetComponent<ComboGlowPulse>();
                if (pulse == null)
                {
                    pulse = t.gameObject.AddComponent<ComboGlowPulse>();
                }
                // Component will handle enable/disable
                pulse.enabled = eligible;
            }
            else
            {
                Debug.LogWarning($"[ComboGlow] GlowEffect not found on {cardObj.name} for card {cardData.cardName}");
            }
        }
    }
    
    /// <summary>
    /// Recursively search for a child transform by name
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Create a card with draw animation from deck pile.
    /// </summary>
    private GameObject CreateAnimatedCard(CardDataExtended cardData, Character player, int cardIndex)
    {
        // Create the card visual - NO CONVERSION! Use CardDataExtended directly
        GameObject cardObj = cardRuntimeManager.CreateCardFromCardDataExtended(cardData, player);
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
    private void AnimateToDiscardPile(GameObject cardObj, CardDataExtended card)
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
    
    public List<CardDataExtended> GetHand()
    {
        return new List<CardDataExtended>(hand);
    }
    
    public List<CardDataExtended> GetDrawPile()
    {
        return new List<CardDataExtended>(drawPile);
    }
    
    public List<CardDataExtended> GetDiscardPile()
    {
        return new List<CardDataExtended>(discardPile);
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
    
    public List<GameObject> GetHandVisuals()
    {
        return new List<GameObject>(handVisuals);
    }
    
    /// <summary>
    /// Get the current character for tooltip calculations
    /// </summary>
    public Character GetCurrentCharacter()
    {
        if (characterManager != null && characterManager.HasCharacter())
        {
            return characterManager.GetCurrentCharacter();
        }
        return null;
    }
    
    /// <summary>
    /// Update card visuals when mana changes
    /// </summary>
    public void UpdateCardUsability()
    {
        Character player = characterManager != null && characterManager.HasCharacter() ? 
            characterManager.GetCurrentCharacter() : null;
        
        if (player == null) return;
        
        // Update each card visual's usability
        for (int i = 0; i < handVisuals.Count; i++)
        {
            if (i < hand.Count && handVisuals[i] != null)
            {
                GameObject cardObj = handVisuals[i];
                CardDataExtended card = hand[i]; // NOW USING CardDataExtended!
                
                // Update card visual components - use new CardDataExtended methods
                CombatCardAdapter adapter = cardObj.GetComponent<CombatCardAdapter>();
                if (adapter != null)
                {
                    adapter.SetCardDataExtended(card, player);
                }
                else
                {
                    // Fallback to DeckBuilderCardUI directly
                    DeckBuilderCardUI deckBuilderCard = cardObj.GetComponent<DeckBuilderCardUI>();
                    if (deckBuilderCard != null)
                    {
                        // Pass player for dynamic descriptions!
                        deckBuilderCard.Initialize(card, null, player);
                        Debug.Log($"Updated DeckBuilderCardUI for {card.cardName}");
                    }
                }
            }
        }
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
    
    [ContextMenu("Test Mana Cost Validation")]
    private void TestManaCostValidation()
    {
        Character player = characterManager != null && characterManager.HasCharacter() ?
            characterManager.GetCurrentCharacter() : null;

        if (player == null)
        {
            Debug.LogWarning("No character found for mana cost validation test!");
            return;
        }

        Debug.Log($"<color=yellow>=== MANA COST VALIDATION TEST ===</color>");
        Debug.Log($"Player mana: {player.mana}/{player.maxMana}");
        Debug.Log($"Cards in hand: {hand.Count}");

        for (int i = 0; i < hand.Count; i++)
        {
            CardDataExtended card = hand[i]; // NOW USING CardDataExtended!
            bool canAfford = player.mana >= card.playCost; // Use playCost from CardDataExtended
            string status = canAfford ? "✓ Can afford" : "✗ Cannot afford";
            Debug.Log($"  Card {i + 1}: {card.cardName} (Cost: {card.playCost}) - {status}");
        }

        // Update card visuals
        UpdateCardUsability();
        
        // Check if player can afford any cards
        bool canAffordAnyCard = false;
        for (int i = 0; i < hand.Count; i++)
        {
            if (player.mana >= hand[i].playCost) // Use playCost from CardDataExtended
            {
                canAffordAnyCard = true;
                break;
            }
        }
        
        // Flash End Turn button if player can't afford any cards
        if (!canAffordAnyCard && hand.Count > 0)
        {
            FlashEndTurnButton();
        }
        
        Debug.Log("Card visuals updated with mana cost validation");
    }
    
    /// <summary>
    /// Flash the End Turn button to indicate player can't afford any cards
    /// </summary>
    private void FlashEndTurnButton()
    {
        // Find the End Turn button in the UI
        var combatUI = FindFirstObjectByType<CombatUI>();
        if (combatUI != null)
        {
            combatUI.FlashEndTurnButton();
        }
        else
        {
            // Fallback: try to find AnimatedCombatUI
            var animatedUI = FindFirstObjectByType<AnimatedCombatUI>();
            if (animatedUI != null)
            {
                animatedUI.FlashEndTurnButton();
            }
        }
    }
    
    /// <summary>
    /// Animate a card when player has insufficient mana
    /// </summary>
    private void AnimateInsufficientManaCard(GameObject cardObj)
    {
        if (cardObj == null) return;
        
        // Cancel any existing animations on this card
        LeanTween.cancel(cardObj);
        
        // Get the card's Image component for color changes
        UnityEngine.UI.Image cardImage = cardObj.GetComponent<UnityEngine.UI.Image>();
        if (cardImage == null) return;
        
        // Store original color
        Color originalColor = cardImage.color;
        
        // Create animation sequence: Flash red + wiggle
        LeanTween.sequence()
            // Flash red
            .append(LeanTween.color(cardObj, Color.red, 0.15f).setEase(LeanTweenType.easeOutQuad))
            .append(LeanTween.color(cardObj, originalColor, 0.15f).setEase(LeanTweenType.easeOutQuad))
            // Wiggle left
            .append(LeanTween.moveLocalX(cardObj, cardObj.transform.localPosition.x - 10f, 0.1f).setEase(LeanTweenType.easeInOutQuad))
            // Wiggle right
            .append(LeanTween.moveLocalX(cardObj, cardObj.transform.localPosition.x + 10f, 0.1f).setEase(LeanTweenType.easeInOutQuad))
            // Return to center
            .append(LeanTween.moveLocalX(cardObj, cardObj.transform.localPosition.x, 0.1f).setEase(LeanTweenType.easeInOutQuad))
            // Final flash red
            .append(LeanTween.color(cardObj, Color.red, 0.1f).setEase(LeanTweenType.easeOutQuad))
            .append(LeanTween.color(cardObj, originalColor, 0.1f).setEase(LeanTweenType.easeOutQuad));
        
        Debug.Log($"<color=orange>Animated insufficient mana for {cardObj.name}</color>");
    }
    
    #region Context Menu Debug Methods
    
    [ContextMenu("Test Insufficient Mana Animation")]
    private void TestInsufficientManaAnimation()
    {
        if (handVisuals.Count > 0 && handVisuals[0] != null)
        {
            AnimateInsufficientManaCard(handVisuals[0]);
            Debug.Log("Testing insufficient mana animation on first card in hand");
        }
        else
        {
            Debug.LogWarning("No cards in hand to test animation");
        }
    }
    
    #endregion
    
    /// <summary>
    /// Flash a Unity UI Button with a pulsing effect
    /// </summary>
    private void FlashButton(UnityEngine.UI.Button button)
    {
        if (button == null) return;
        
        // Get the button's image component
        var buttonImage = button.GetComponent<UnityEngine.UI.Image>();
        if (buttonImage == null) return;
        
        // Store original color
        Color originalColor = buttonImage.color;
        
        // Flash red twice
        LeanTween.sequence()
            .append(LeanTween.color(buttonImage.rectTransform, Color.red, 0.2f).setEase(LeanTweenType.easeOutQuad))
            .append(LeanTween.color(buttonImage.rectTransform, originalColor, 0.2f).setEase(LeanTweenType.easeOutQuad))
            .append(LeanTween.color(buttonImage.rectTransform, Color.red, 0.2f).setEase(LeanTweenType.easeOutQuad))
            .append(LeanTween.color(buttonImage.rectTransform, originalColor, 0.2f).setEase(LeanTweenType.easeOutQuad));
    }
    
    /// <summary>
    /// Play combat effects based on card type and effects
    /// </summary>
    private void PlayCardEffects(CardDataExtended card, Enemy targetEnemy, Character player)
    {
        if (combatEffectManager == null) return;
        
        // Get target enemy display for positioning
        EnemyCombatDisplay targetDisplay = null;
        EnemyCombatDisplay[] enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var display in enemyDisplays)
        {
            if (display.GetCurrentEnemy() == targetEnemy)
            {
                targetDisplay = display;
                break;
            }
        }
        
        // Play effects based on card category
        CardType cardType = card.GetCardTypeEnum(); // Get CardType from CardDataExtended
        
        switch (cardType)
        {
            case CardType.Attack:
                if (targetDisplay != null)
                {
                    // Check for critical hit (10% chance)
                    bool isCritical = Random.Range(0f, 1f) < 0.1f;
                    
                    // Get damage type from card effects
                    DamageType damageType = GetCardDamageType(card);
                    
                    // Play elemental damage effect
                    combatEffectManager.PlayElementalDamageEffectOnTarget(targetDisplay.transform, damageType, isCritical);
                }
                break;
                
            case CardType.Guard:
                // Guard effect on player
                PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplay != null)
                {
                    combatEffectManager.PlayGuardEffectOnTarget(playerDisplay.transform);
                }
                break;
                
            case CardType.Skill:
                // Check if it's a heal skill
                if (card.cardName.ToLower().Contains("heal"))
                {
                    PlayerCombatDisplay playerDisplay2 = FindFirstObjectByType<PlayerCombatDisplay>();
                    if (playerDisplay2 != null)
                    {
                        combatEffectManager.PlayHealEffectOnTarget(playerDisplay2.transform);
                    }
                }
                else if (targetDisplay != null)
                {
                    // Other skill effects on target
                    DamageType damageType = GetCardDamageType(card);
                    combatEffectManager.PlayElementalDamageEffectOnTarget(targetDisplay.transform, damageType);
                }
                break;
                
            case CardType.Power:
            case CardType.Aura:
                // Status effect or buff
                PlayerCombatDisplay playerDisplay3 = FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplay3 != null)
                {
                    combatEffectManager.PlayStatusEffectOnTarget(playerDisplay3.transform, card.cardName);
                }
                break;
        }
        
        Debug.Log($"Played combat effects for {card.cardName} ({cardType})");
    }
    
    /// <summary>
    /// Get the damage type from a card's effects
    /// </summary>
    private DamageType GetCardDamageType(CardDataExtended card)
    {
        // Check if card has effects
        if (card.effects != null && card.effects.Count > 0)
        {
            // Look for damage effects and get their damage type
            foreach (var effect in card.effects)
            {
                if (effect.effectType == EffectType.Damage)
                {
                    return effect.damageType;
                }
            }
        }
        
        // Check card name for elemental keywords
        string cardName = card.cardName.ToLower();
        if (cardName.Contains("fire") || cardName.Contains("burn"))
            return DamageType.Fire;
        else if (cardName.Contains("ice") || cardName.Contains("frost") || cardName.Contains("freeze") || cardName.Contains("cold"))
            return DamageType.Cold;
        else if (cardName.Contains("lightning") || cardName.Contains("thunder") || cardName.Contains("shock"))
            return DamageType.Lightning;
        else if (cardName.Contains("poison") || cardName.Contains("toxic") || cardName.Contains("venom") || cardName.Contains("chaos"))
            return DamageType.Chaos;
        else if (cardName.Contains("magic") || cardName.Contains("arcane") || cardName.Contains("spell"))
            return DamageType.Chaos; // Use Chaos as magic equivalent
        else if (cardName.Contains("dark") || cardName.Contains("shadow") || cardName.Contains("void"))
            return DamageType.Chaos; // Use Chaos as dark equivalent
        else if (cardName.Contains("light") || cardName.Contains("holy") || cardName.Contains("divine"))
            return DamageType.Lightning; // Use Lightning as light equivalent
        
        // Default to physical damage
        return DamageType.Physical;
    }
    
    /// <summary>
    /// Trigger player sprite animations based on card type
    /// </summary>
    private void TriggerPlayerAnimation(CardDataExtended card)
    {
        PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay == null)
        {
            Debug.LogWarning("PlayerCombatDisplay not found for sprite animation!");
            return;
        }
        
        // Trigger animations based on card category
        CardType cardType = card.GetCardTypeEnum(); // Get CardType from CardDataExtended
        
        switch (cardType)
        {
            case CardType.Attack:
                playerDisplay.TriggerAttackNudge();
                Debug.Log($"<color=red>Triggered attack nudge for {card.cardName}</color>");
                break;
                
            case CardType.Guard:
                playerDisplay.TriggerGuardSheen();
                Debug.Log($"<color=blue>Triggered guard sheen for {card.cardName}</color>");
                break;
                
            case CardType.Skill:
            case CardType.Power:
            case CardType.Aura:
                // No specific animation for these types yet
                Debug.Log($"<color=purple>Played {cardType} card: {card.cardName}</color>");
                break;
        }
    }
    
    #endregion
}

