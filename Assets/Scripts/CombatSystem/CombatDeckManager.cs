using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

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
    [SerializeField] private DiscardPanel discardPanel;
    
    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI deckCountText;
    [SerializeField] private TMPro.TextMeshProUGUI discardCountText;
    
    [Header("Deck Positions (For Animations)")]
    [SerializeField] private Transform deckPileTransform;
    [Tooltip("Visual position where draw pile sits - cards animate from here")]
    [SerializeField] private Transform discardPileTransform;
    [Tooltip("Visual position where discard pile sits - cards animate to here when played")]
    
    [Header("Speed Meter Settings")]
    [SerializeField] private float aggressionChargeThreshold = 100f;
    [SerializeField] private float aggressionBaseGainPercent = 100f / 30f;
    [SerializeField] private float focusChargeThreshold = 100f;
    [SerializeField] private float focusBaseGainPercent = 100f / 20f;
    [SerializeField] private bool showSpeedMeterDebug = false;
    private const float DefaultAggressionHitsBaseline = 30f;
    private const float DefaultFocusHitsBaseline = 20f;
    private const float LegacyBaseGainPercent = 25f;
    
    [Header("Debug Info (Read-Only)")]
    [SerializeField] private int debugDrawPileCount = 0;
    [SerializeField] private int debugHandCount = 0;
    [SerializeField] private int debugDiscardCount = 0;
    [SerializeField] private int debugTotalCardCount = 0;
    [SerializeField] private int debugInitialDeckSize = 0;
    
    // Deck state tracking for debugging card loss bug
    private int initialDeckSize = 0; // Track expected total card count
    private bool deckInitialized = false; // Prevent mid-combat deck reloads
    [Header("Card Loss Debugging")]
    [Tooltip("Enable detailed card count validation logging")]
    [SerializeField] private bool enableCardCountValidation = true;
    
    // Deck piles - NOW USING CardDataExtended (no conversion!)
    private List<CardDataExtended> drawPile = new List<CardDataExtended>();
    private List<CardDataExtended> hand = new List<CardDataExtended>();
    private List<CardDataExtended> discardPile = new List<CardDataExtended>();
    private List<GameObject> handVisuals = new List<GameObject>();
    
    // Track cards currently being played (to prevent interference)
    private HashSet<GameObject> cardsBeingPlayed = new HashSet<GameObject>();
    private Dictionary<GameObject, CardPlayState> cardPlayStates = new Dictionary<GameObject, CardPlayState>();
    
    // Action Queue System (Phase 1: Best Practice Architecture)
    private Queue<CardAction> actionQueue = new Queue<CardAction>();
    private bool isProcessingQueue = false;
    
    // Phase 2: Micro Input Lock - prevents visual chaos from rapid clicking
    [Header("Phase 2: Input Handling")]
    [Tooltip("Minimum time between card plays (in seconds). Prevents visual chaos while maintaining soft queue.")]
    [SerializeField] private float inputLockDuration = 0.05f; // 50ms - feels instant but prevents spam
    private float lastCardPlayTime = -1f;
    
    /// <summary>
    /// Represents a queued card play action - logic is resolved immediately, animations are separate
    /// </summary>
    private class CardAction
    {
        public CardDataExtended card;
        public GameObject cardVisual; // Visual representation (clone or original) - null for unleashed cards
        public int handIndex; // -1 for unleashed cards
        public Vector3 targetPosition;
        public Enemy targetEnemy;
        public Character playerCharacter;
        public ComboSystem.ComboApplication comboApp;
        public bool resolved; // Track if logic has been resolved
        public float queueTime;
        
        // Unleash-specific data (null for regular card plays)
        public float? unleashDamage; // Pre-calculated unleash damage (includes preparation multiplier)
        public float? unleashMultiplier; // Preparation multiplier for momentum-based cards
        public bool isUnleash; // Flag to indicate this is an unleashed card
        
        public CardAction(CardDataExtended card, GameObject cardVisual, int handIndex, Vector3 targetPosition, Enemy targetEnemy, Character playerCharacter, ComboSystem.ComboApplication comboApp)
        {
            this.card = card;
            this.cardVisual = cardVisual;
            this.handIndex = handIndex;
            this.targetPosition = targetPosition;
            this.targetEnemy = targetEnemy;
            this.playerCharacter = playerCharacter;
            this.comboApp = comboApp;
            this.resolved = false;
            this.queueTime = Time.time;
            this.isUnleash = false;
            this.unleashDamage = null;
            this.unleashMultiplier = null;
        }
        
        // Constructor for unleashed cards
        public CardAction(CardDataExtended card, Vector3 targetPosition, Enemy targetEnemy, Character playerCharacter, float unleashDamage, float unleashMultiplier)
        {
            this.card = card;
            this.cardVisual = null; // Unleashed cards don't have visuals in hand
            this.handIndex = -1; // Not from hand
            this.targetPosition = targetPosition;
            this.targetEnemy = targetEnemy;
            this.playerCharacter = playerCharacter;
            this.comboApp = null;
            this.resolved = false;
            this.queueTime = Time.time;
            this.isUnleash = true;
            this.unleashDamage = unleashDamage;
            this.unleashMultiplier = unleashMultiplier;
        }
    }
    
    /// <summary>
    /// Card state for tracking - follows best practice state model
    /// Phase 2: Enhanced with validation and explicit transitions
    /// </summary>
    private enum CardState
    {
        InHand,
        Queued,      // Action queued, waiting to resolve
        Resolving,   // Effects being applied
        Resolved,    // Effects applied, waiting for cleanup
        Discarded,   // In discard pile
        Exhausted    // Removed from combat
    }
    
    /// <summary>
    /// Phase 2: Validate if a state transition is allowed
    /// </summary>
    private bool IsValidStateTransition(CardState from, CardState to)
    {
        // Valid transitions (following best practice state model)
        switch (from)
        {
            case CardState.InHand:
                return to == CardState.Queued || to == CardState.Discarded || to == CardState.Exhausted;
            
            case CardState.Queued:
                return to == CardState.Resolving || to == CardState.Discarded || to == CardState.Exhausted;
            
            case CardState.Resolving:
                return to == CardState.Resolved || to == CardState.Discarded || to == CardState.Exhausted;
            
            case CardState.Resolved:
                return to == CardState.Discarded || to == CardState.Exhausted;
            
            case CardState.Discarded:
                return to == CardState.InHand; // Card can be drawn back into hand
            
            case CardState.Exhausted:
                return false; // Exhausted is terminal - no transitions allowed
            
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Phase 2: Safely transition card state with validation
    /// </summary>
    private bool TransitionCardState(GameObject cardObj, CardState newState, string context = "")
    {
        if (cardObj == null) return false;
        
        if (!cardPlayStates.ContainsKey(cardObj))
        {
            Debug.LogWarning($"[State Transition] Card {cardObj.name} not found in cardPlayStates. Context: {context}");
            return false;
        }
        
        CardState oldState = cardPlayStates[cardObj].state;
        
        // Validate transition
        if (!IsValidStateTransition(oldState, newState))
        {
            Debug.LogError($"[State Transition] INVALID transition for {cardPlayStates[cardObj].card?.cardName ?? cardObj.name}: {oldState} → {newState}. Context: {context}");
            return false;
        }
        
        // Perform transition
        cardPlayStates[cardObj].state = newState;
        
        // Log transition (only if enabled to avoid spam)
        if (logStateTransitions || showDebugLogs)
        {
            Debug.Log($"[State Transition] {cardPlayStates[cardObj].card?.cardName ?? cardObj.name}: {oldState} → {newState}. Context: {context}");
        }
        
        return true;
    }
    
    [Header("Debug Logging Settings")]
    [Tooltip("Enable detailed state transition logging")]
    [SerializeField] private bool showDebugLogs = false;
    
    [Tooltip("Log card draw operations")]
    [SerializeField] private bool logCardDraws = true;
    
    [Tooltip("Log card play/discard operations")]
    [SerializeField] private bool logCardOperations = true;
    
    [Tooltip("Log animation events (card play animations, discard animations, etc.)")]
    [SerializeField] private bool logAnimations = false;
    
    [Tooltip("Log card effect processing (channeling, combos, damage calculations)")]
    [SerializeField] private bool logCardEffects = false;
    
    [Tooltip("Log card state transitions (Queued -> Resolving -> Resolved, etc.)")]
    [SerializeField] private bool logStateTransitions = false;
    
    [Tooltip("Always log critical errors and card loss detection (recommended: always enabled)")]
    [SerializeField] private bool logCriticalErrors = true;
    
    [Tooltip("Log detailed card click debug information")]
    [SerializeField] private bool logCardClickDebug = false;
    
    [Tooltip("Log card hover operations (raise to top, restore layer)")]
    [SerializeField] private bool logCardHover = false;
    
    [Tooltip("Log card adapter initialization (card setting, image checks)")]
    [SerializeField] private bool logCardAdapter = false;
    
    [Tooltip("Log combo glow state changes")]
    [SerializeField] private bool logComboGlow = false;
    
    [Tooltip("Log discard check operations")]
    [SerializeField] private bool logDiscardCheck = false;
    
    /// <summary>
    /// Public getter for damage calculation logging (used by DamageCalculator)
    /// </summary>
    public bool ShouldLogCardEffects => logCardEffects;
    
    /// <summary>
    /// Phase 2: Validate card state integrity - ensures no orphaned or invalid states
    /// Call this periodically or when suspicious state detected
    /// </summary>
    private void ValidateCardStates()
    {
        // Check for cards in cardsBeingPlayed that aren't in cardPlayStates
        foreach (var cardObj in cardsBeingPlayed.ToList())
        {
            if (cardObj == null)
            {
                Debug.LogWarning("[State Validation] Found null cardObj in cardsBeingPlayed, removing...");
                cardsBeingPlayed.Remove(cardObj);
                continue;
            }
            
            if (!cardPlayStates.ContainsKey(cardObj))
            {
                Debug.LogWarning($"[State Validation] Card {cardObj.name} in cardsBeingPlayed but not in cardPlayStates! Adding default state...");
                // Recover by creating default state (shouldn't happen, but better than crashing)
                cardPlayStates[cardObj] = new CardPlayState
                {
                    card = null, // Can't recover card reference
                    cardObj = cardObj,
                    effectsApplied = false,
                    playStartTime = Time.time,
                    state = CardState.Queued
                };
            }
        }
        
        // Check for cards in cardPlayStates that are null or destroyed
        var keysToRemove = new List<GameObject>();
        foreach (var kvp in cardPlayStates)
        {
            if (kvp.Key == null)
            {
                Debug.LogWarning("[State Validation] Found null key in cardPlayStates, marking for removal...");
                keysToRemove.Add(kvp.Key);
                continue;
            }
            
            // Check for stuck states (cards that have been resolving for too long)
            if (kvp.Value.state == CardState.Resolving || kvp.Value.state == CardState.Resolved)
            {
                float timeSincePlay = Time.time - kvp.Value.playStartTime;
                if (timeSincePlay > 10f) // 10 seconds is way too long
                {
                    Debug.LogError($"[State Validation] Card {kvp.Value.card?.cardName ?? kvp.Key.name} stuck in {kvp.Value.state} state for {timeSincePlay:F1}s! This indicates a bug.");
                }
            }
        }
        
        // Remove invalid entries
        foreach (var key in keysToRemove)
        {
            cardPlayStates.Remove(key);
            cardsBeingPlayed.Remove(key);
        }
    }
    
    private class CardPlayState
    {
        public CardDataExtended card;
        public GameObject cardObj;
        public bool effectsApplied;
        public float playStartTime;
        public CardState state = CardState.InHand; // Track state explicitly
    }
    
    // Events
    public System.Action<CardDataExtended> OnCardDrawn;
    public System.Action<CardDataExtended> OnCardPlayed;
    public System.Action<CardDataExtended> OnCardDiscarded;
    public System.Action OnDeckShuffled;
    public System.Action<Character, ChannelingTracker.ChannelingState> OnChannelingStateChanged;
    public System.Action<SpeedMeterState> OnSpeedMeterChanged;
    private ChannelingTracker.ChannelingState lastChannelingState = default;
    
    // Divine Favor state (next card applies discarded effect)
    private bool nextCardAppliesDiscardedEffect = false;
    
    // Boss Ability System - track cards played this turn
    private int cardsPlayedThisTurn = 0;
    
    public ChannelingTracker.ChannelingState CurrentChannelingState => lastChannelingState;
    public SpeedMeterState CurrentSpeedMeterState => new SpeedMeterState
    {
        aggressionProgress = aggressionMeterValue / Mathf.Max(1f, aggressionChargeThreshold),
        aggressionCharges = aggressionCharges,
        focusProgress = focusMeterValue / Mathf.Max(1f, focusChargeThreshold),
        focusCharges = focusCharges
    };
    public SpeedMeterState GetSpeedMeterState() => CurrentSpeedMeterState;
    private static readonly Color ChannelingStartColor = new Color(0.25f, 0.85f, 1f);
    private static readonly Color ChannelingEndColor = new Color(1f, 0.5f, 0.3f);
    private PlayerCombatDisplay cachedPlayerDisplay;
    private float aggressionMeterValue = 0f;
    private int aggressionCharges = 0;
    private float focusMeterValue = 0f;
    private int focusCharges = 0;

    #region Charge Modifier State

    public enum AggressionChargeEffect
    {
        None,
        HitsTwice,
        HitsAllEnemies,
        AlwaysCrit,
        IgnoresGuardArmor
    }

    public enum FocusChargeEffect
    {
        None,
        DoubleDamage,
        HalfManaCost
    }

    [Header("Charge Modifier State")]
    [SerializeField] private AggressionChargeEffect selectedAggressionEffect = AggressionChargeEffect.None;
    [SerializeField] private FocusChargeEffect selectedFocusEffect = FocusChargeEffect.None;
    private bool aggressionModifierConsumed = false;
    private bool focusModifierConsumed = false;

    public event System.Action<AggressionChargeEffect> OnAggressionModifierSelected;
    public event System.Action<FocusChargeEffect> OnFocusModifierSelected;
    public event System.Action OnChargeModifiersCleared;

    #endregion
    
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
        
        // Initialize Ascendancy Modifier Event Processor
        var modifierProcessor = AscendancyModifierEventProcessor.Instance;
        if (modifierProcessor != null)
        {
            Debug.Log("[CombatDeckManager] Ascendancy Modifier Event Processor initialized");
        }
        
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

        EnsureSpeedMeterDefaults();
        RaiseSpeedMeterChanged();
        ResetChargeModifiers();
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
            ResetChannelingState();
            ResetSpeedMeters();
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
            ResetChannelingState();
            ResetSpeedMeters();
        }
    }

    private void OnValidate()
    {
        EnsureSpeedMeterDefaults();
    }
    
    private void Update()
    {
        // Update debug counts (visible in Inspector)
        debugDrawPileCount = drawPile.Count;
        debugHandCount = hand.Count;
        debugDiscardCount = discardPile.Count;
        debugTotalCardCount = GetTotalCardCount();
        debugInitialDeckSize = initialDeckSize;
        
        // Validate card count if enabled (check every 60 frames to reduce log spam)
        if (enableCardCountValidation && deckInitialized && Time.frameCount % 60 == 0)
        {
            ValidateCardCount("Periodic Update Check");
        }
        
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
        // SAFETY: Prevent deck reload during active combat (would cause card loss bug)
        if (deckInitialized)
        {
            Debug.LogWarning($"<color=red>[CARD LOSS PREVENTION] LoadDeckForClass called but deck already initialized! " +
                           $"This would reset the deck mid-combat and cause card loss. Blocking reload.</color>");
            Debug.LogWarning($"Current state - Draw: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}, Total: {GetTotalCardCount()}");
            return;
        }
        
        Debug.Log($"<color=yellow>=== Loading deck for: {characterClass} ===</color>");
        
        // Log state before clearing (for debugging)
        int cardsBeforeClear = GetTotalCardCount();
        if (cardsBeforeClear > 0)
        {
            Debug.LogWarning($"<color=yellow>[DECK LOAD] Clearing {cardsBeforeClear} existing cards before loading new deck</color>");
        }
        
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
        bool deckLoadedFromPreset = false;
        
        if (cardDataDeck == null || cardDataDeck.Count == 0)
        {
            cardDataDeck = LoadDeckFromActivePreset(characterClass);
            deckLoadedFromPreset = cardDataDeck != null && cardDataDeck.Count > 0;
        }
        
        if (cardDataDeck != null && cardDataDeck.Count > 0)
        {
            if (deckLoadedFromPreset)
            {
                drawPile.AddRange(cardDataDeck);
                Debug.Log($"<color=green>✓ Loaded {characterClass} deck from active DeckBuilder preset:</color> {drawPile.Count} cards");
            }
            else
            {
                foreach (CardDataExtended card in cardDataDeck)
                {
                    int copies = GetCardCopies(card.cardName);
                    for (int i = 0; i < copies; i++)
                    {
                        drawPile.Add(card);
                    }
                    Debug.Log($"[CardDataExtended] Added {copies} copies of {card.cardName} to deck");
                }
                Debug.Log($"<color=green>✓ Loaded {characterClass} deck from CardDatabase:</color> {drawPile.Count} cards");
            }
            
            Debug.Log($"Draw pile now contains: {drawPile.Count} cards");
            
            if (drawPile.Count > 0)
            {
                Debug.Log($"First card in draw pile: {drawPile[0].cardName}");
            }
            
            // Track initial deck size for validation
            initialDeckSize = drawPile.Count;
            deckInitialized = true;
            
            Debug.Log($"<color=green>[DECK INITIALIZED] Initial deck size set to: {initialDeckSize} cards</color>");
            
            LogDeckComposition();
            ValidateCardCount("After LoadDeckForClass");
        }
        else
        {
            Debug.LogError($"<color=red>✗ Failed to load deck for {characterClass}!</color>");
        }
    }
    
    /// <summary>
    /// Get total card count across all piles (for validation)
    /// Includes prepared cards since they're temporarily removed from hand
    /// </summary>
    private int GetTotalCardCount()
    {
        int preparedCount = 0;
        var prepManager = PreparationManager.Instance;
        if (prepManager != null)
        {
            var preparedCards = prepManager.GetPreparedCards();
            preparedCount = preparedCards != null ? preparedCards.Count : 0;
        }
        
        return drawPile.Count + hand.Count + discardPile.Count + preparedCount;
    }
    
    /// <summary>
    /// Validate that card count matches expected initial size (debugging card loss bug)
    /// </summary>
    private void ValidateCardCount(string context)
    {
        if (!deckInitialized || initialDeckSize == 0)
            return; // Can't validate if deck hasn't been initialized yet
        
        int currentTotal = GetTotalCardCount();
        
        if (currentTotal != initialDeckSize)
        {
            int preparedCount = 0;
            var prepManager = PreparationManager.Instance;
            if (prepManager != null)
            {
                var preparedCards = prepManager.GetPreparedCards();
                preparedCount = preparedCards != null ? preparedCards.Count : 0;
            }
            
            Debug.LogError($"<color=red>[CARD LOSS DETECTED] Card count mismatch during {context}! " +
                         $"Expected: {initialDeckSize}, Actual: {currentTotal}, Lost: {initialDeckSize - currentTotal} cards</color>");
            Debug.LogError($"  Draw Pile: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}, Prepared: {preparedCount}");
            Debug.LogError($"  Stack trace: {System.Environment.StackTrace}");
        }
        else if (enableCardCountValidation)
        {
            // Only log validation success in verbose mode (every 10th validation to reduce spam)
            if (Time.frameCount % 600 == 0)
            {
                int preparedCount = 0;
                var prepManager = PreparationManager.Instance;
                if (prepManager != null)
                {
                    var preparedCards = prepManager.GetPreparedCards();
                    preparedCount = preparedCards != null ? preparedCards.Count : 0;
                }
                
                Debug.Log($"<color=green>[CARD COUNT VALID] {context}: {currentTotal} cards (Draw: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}, Prepared: {preparedCount})</color>");
            }
        }
    }
    
    /// <summary>
    /// Log current deck state for debugging
    /// </summary>
    private void LogDeckState(string context)
    {
        int total = GetTotalCardCount();
        int preparedCount = 0;
        var prepManager = PreparationManager.Instance;
        if (prepManager != null)
        {
            var preparedCards = prepManager.GetPreparedCards();
            preparedCount = preparedCards != null ? preparedCards.Count : 0;
        }
        
        Debug.Log($"[DECK STATE] {context} | Draw: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}, Prepared: {preparedCount}, Total: {total}" +
                 (deckInitialized ? $" (Expected: {initialDeckSize})" : " (Not initialized)"));
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
    
    private List<CardDataExtended> LoadDeckFromActivePreset(string characterClass)
    {
        DeckManager deckManager = DeckManager.Instance;
        if (deckManager == null || !deckManager.HasActiveDeck())
        {
            Debug.LogWarning("[CombatDeckManager] No active deck preset available. Falling back to CardDatabase.");
            return null;
        }
        
        DeckPreset activeDeck = deckManager.GetActiveDeck();
        if (activeDeck == null)
        {
            Debug.LogWarning("[CombatDeckManager] Active deck preset is null.");
            return null;
        }
        
        if (!string.IsNullOrEmpty(activeDeck.characterClass) && !string.Equals(activeDeck.characterClass, characterClass, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[CombatDeckManager] Active deck preset class '{activeDeck.characterClass}' does not match expected class '{characterClass}'. Loading anyway.");
        }
        
        List<CardDataExtended> result = new List<CardDataExtended>();
        foreach (DeckCardEntry entry in activeDeck.GetCardEntries())
        {
            if (entry == null || entry.cardData == null || entry.quantity <= 0)
                continue;
            CardDataExtended extended = entry.cardData as CardDataExtended;
            if (extended == null)
            {
                extended = LoadCardDataExtendedDirectly(entry.cardData.cardName);
            }
            if (extended == null)
            {
                Debug.LogWarning($"[CombatDeckManager] Could not resolve CardDataExtended for '{entry.cardData.cardName}'.");
                continue;
            }
            for (int i = 0; i < entry.quantity; i++)
            {
                result.Add(extended);
            }
        }
        
        Debug.Log($"[CombatDeckManager] Loaded {result.Count} cards from active deck preset '{activeDeck.deckName}'.");
        return result;
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
    /// Reset turn-based counters (called at start of player turn)
    /// </summary>
    public void ResetTurnCounters()
    {
        cardsPlayedThisTurn = 0;
        Debug.Log($"<color=cyan>[CombatDeckManager] Turn counters reset</color>");
    }
    
    /// <summary>
    /// Get number of cards played this turn (for boss abilities)
    /// </summary>
    public int GetCardsPlayedThisTurn()
    {
        return cardsPlayedThisTurn;
    }
    
    /// <summary>
    /// Draw a specific number of cards
    /// </summary>
    public void DrawCards(int count)
    {
        // Check for DrawReduction status effect
        var playerDisplay = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            var statusManager = playerDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                float drawReduction = statusManager.GetTotalMagnitude(StatusEffectType.DrawReduction);
                if (drawReduction > 0)
                {
                    int reduction = Mathf.FloorToInt(drawReduction);
                    count = Mathf.Max(0, count - reduction);
                    Debug.Log($"<color=yellow>[DrawReduction] Card draw reduced by {reduction}! Drawing {count} cards instead.</color>");
                    
                    var combatUI = UnityEngine.Object.FindFirstObjectByType<AnimatedCombatUI>();
                    if (combatUI != null)
                    {
                        combatUI.LogMessage($"<color=grey>Draw Reduction!</color> Drawing {count} card(s) instead.");
                    }
                }
            }
        }
        
        if (logCardDraws)
        {
            Debug.Log($"<color=yellow>=== DrawCards called: Drawing {count} cards ===</color>");
            LogDeckState("Before DrawCards");
        }
        ValidateCardCount("Before DrawCards");
        
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
                    int discardCountBefore = discardPile.Count;
                    if (logCardDraws)
                    {
                        Debug.Log($"<color=cyan>Draw pile empty! Reshuffling {discardCountBefore} cards from discard pile...</color>");
                        LogDeckState("Before Reshuffle");
                    }
                    
                    drawPile.AddRange(discardPile);
                    discardPile.Clear();
                    
                    // Validate cards weren't lost during transfer (always check, even if logging disabled)
                    if (drawPile.Count != discardCountBefore)
                    {
                        if (logCriticalErrors)
                        {
                            Debug.LogError($"<color=red>[RESHUFFLE BUG] Card count mismatch! Expected {discardCountBefore} cards in draw pile after transfer, got {drawPile.Count}</color>");
                        }
                    }
                    
                    ShuffleDeck();
                    if (logCardDraws)
                    {
                        LogDeckState("After Reshuffle");
                    }
                    ValidateCardCount("After Reshuffle");
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>No cards left to draw! Draw: {drawPile.Count}, Hand: {hand.Count}, Discard: {discardPile.Count}</color>");
                    ValidateCardCount("Draw Pile Empty");
                    break;
                }
            }
            
            if (drawPile.Count > 0)
            {
                // Draw card from deck - NOW USING CardDataExtended!
                CardDataExtended drawnCard = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(drawnCard);
                
                if (logCardDraws)
                {
                    Debug.Log($"<color=cyan>Drawing card #{i+1}: {drawnCard.cardName}</color>");
                }
                
                // Create visual card with draw animation - NO CONVERSION!
                GameObject cardObj = CreateAnimatedCard(drawnCard, player, i);
                
                if (cardObj != null)
                {
                    if (logCardDraws)
                    {
                        Debug.Log($"<color=green>✓ Card GameObject created: {cardObj.name}</color>");
                    }
                    
                    // Verify parent is correct (only log if verbose logging enabled)
                    if (showDebugLogs && cardRuntimeManager.cardHandParent != null)
                    {
                        if (cardObj.transform.parent != cardRuntimeManager.cardHandParent)
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
                    if (logCriticalErrors)
                    {
                        Debug.LogError($"<color=red>✗ Failed to create GameObject for: {drawnCard.cardName}</color>");
                    }
                }
                
                OnCardDrawn?.Invoke(drawnCard);
            }
        }
        
        if (logCardDraws)
        {
            Debug.Log($"<color=yellow>=== Draw complete: {hand.Count} cards in hand, {handVisuals.Count} visuals created ===</color>");
            LogDeckState("After DrawCards");
        }
        ValidateCardCount("After DrawCards");
        
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
                if (logAnimations)
                {
                    Debug.Log($"<color=green>All cards drawn! Repositioning {handVisuals.Count} cards to final positions...</color>");
                }
                cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
                
                // IMPORTANT: Ensure ALL cards are interactable after draw completes
                foreach (GameObject cardObj in handVisuals)
                {
                    if (cardObj != null)
                    {
                        SetCardInteractable(cardObj, true);
                        
                        // Get button component once and reuse
                        UnityEngine.UI.Button button = cardObj.GetComponent<UnityEngine.UI.Button>();
                        
                        // Only log interaction details if verbose logging enabled
                        if (showDebugLogs)
                        {
                            int siblingIndex = cardObj.transform.GetSiblingIndex();
                            bool buttonInteractable = button != null ? button.interactable : false;
                            Debug.Log($"  Re-enabled interaction for {cardObj.name} [Sibling: {siblingIndex}, Button: {buttonInteractable}]");
                        }
                        
                        // Double-check: Force button interactable if it's still false
                        if (button != null && !button.interactable)
                        {
                            if (logCriticalErrors)
                            {
                                Debug.LogWarning($"  ⚠️ Button still disabled for {cardObj.name}! Force-enabling...");
                            }
                            button.interactable = true;
                        }
                    }
                }
                
                // Update combo glow states after draw completes
                UpdateComboHighlights();
            }
        });
    }
    
    #region Action Queue System (Phase 1: Best Practice Architecture)
    
    /// <summary>
    /// Process the next action in the queue immediately (logic resolution, not animation)
    /// </summary>
    private void ProcessActionQueue()
    {
        if (isProcessingQueue || actionQueue.Count == 0)
            return;
        
        isProcessingQueue = true;
        
        while (actionQueue.Count > 0)
        {
            CardAction action = actionQueue.Dequeue();
            if (action.resolved)
                continue; // Skip already resolved actions (safety check)
            
            ResolveCardAction(action);
        }
        
        isProcessingQueue = false;
    }
    
    /// <summary>
    /// Resolve a card action - apply all game logic immediately, independent of animations
    /// </summary>
    private void ResolveCardAction(CardAction action)
    {
        if (action.resolved)
        {
            Debug.LogWarning($"[ActionQueue] Action for {action.card.cardName} already resolved!");
            return;
        }
        
        if (logCardOperations)
        {
            Debug.Log($"<color=cyan>[ActionQueue] Resolving {action.card.cardName} (logic only, animations separate){(action.isUnleash ? " [UNLEASHED]" : "")}</color>");
        }
        
        // Phase 2: Update state to Resolving with validation (skip for unleashed cards - they don't have visuals)
        if (action.cardVisual != null)
        {
            TransitionCardState(action.cardVisual, CardState.Resolving, "ResolveCardAction");
        }
        
        // Handle unleashed cards differently - they use the preparation system's effect pipeline
        if (action.isUnleash)
        {
            ApplyUnleashEffects(action);
        }
        else
        {
            // Apply all card effects immediately (this handles channeling, combos, damage, etc.)
            // OnCardPlayed event is fired inside ApplyCardEffectsInternal
            ApplyCardEffectsInternal(action.card, action.cardVisual, action.targetEnemy, action.playerCharacter, action.targetPosition, action.comboApp);
        }
        
        // Add card to discard pile (logical state - card is played, it goes to discard)
        // Note: Visual animation to discard pile happens separately
        if (!discardPile.Contains(action.card))
        {
            discardPile.Add(action.card);
            LogDeckState($"After Adding Card to Discard in ResolveCardAction: {action.card.cardName}");
            ValidateCardCount($"After ResolveCardAction - card added to discard: {action.card.cardName}");
            // OnCardDiscarded is invoked later during visual cleanup, not here
        }
        else
        {
            Debug.LogWarning($"[ResolveCardAction] Card {action.card.cardName} already in discard pile! (duplicate add prevented)");
        }
        
        // Mark as resolved
        action.resolved = true;
        
        // Phase 2: Update state to Resolved with validation (skip for unleashed cards)
        if (action.cardVisual != null && cardPlayStates.ContainsKey(action.cardVisual))
        {
            TransitionCardState(action.cardVisual, CardState.Resolved, "ResolveCardAction - effects applied");
            cardPlayStates[action.cardVisual].effectsApplied = true;
        }
        
        if (logCardOperations)
        {
            Debug.Log($"<color=green>[ActionQueue] {action.card.cardName} resolved - effects applied, card in discard pile, animation can now proceed independently</color>");
        }
    }
    
    /// <summary>
    /// Clean up card after play is complete (visual only - logic already resolved)
    /// </summary>
    private void CleanupCardAfterPlay(CardAction action)
    {
        if (action == null || action.cardVisual == null)
            return;
        
        CleanupCardAfterPlay(action.cardVisual, action.card);
    }
    
    /// <summary>
    /// Clean up card after play is complete (visual only - logic already resolved)
    /// </summary>
    private void CleanupCardAfterPlay(GameObject cardObj, CardDataExtended card)
    {
        if (cardObj == null || card == null)
            return;
        
        // Ensure card is in discard pile (should already be there, but safety check)
        if (!discardPile.Contains(card))
        {
            Debug.LogWarning($"[CleanupCardAfterPlay] Card {card.cardName} not in discard pile! Adding now (possible card loss bug)");
            discardPile.Add(card);
            OnCardDiscarded?.Invoke(card);
            ValidateCardCount($"After CleanupCardAfterPlay - card added to discard: {card.cardName}");
        }
        
        // Return card to pool
        if (cardRuntimeManager != null)
        {
            cardRuntimeManager.ReturnCardToPool(cardObj);
        }
        else
        {
            Destroy(cardObj);
        }
        
        // Phase 2: Clean up tracking with validated state transition
        cardsBeingPlayed.Remove(cardObj);
        if (cardPlayStates.ContainsKey(cardObj))
        {
            TransitionCardState(cardObj, CardState.Discarded, "CleanupCardAfterPlay");
            cardPlayStates.Remove(cardObj);
        }
        
        if (logCardOperations)
        {
            Debug.Log($"<color=green>[Cleanup] {card.cardName} cleaned up and returned to pool</color>");
        }
        
        // Phase 2: Validate states after cleanup to catch any inconsistencies
        ValidateCardStates();
    }
    
    #endregion
    
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
        
        // Calculate base mana cost (percentage for Skill cards if set, otherwise flat)
        int baseManaCost = card.GetCurrentManaCost(null, player);
        
        // Apply charge modifiers to mana cost
        int effectiveManaCost = ApplyManaCostModifier(card, baseManaCost);
        
        if (player.mana < effectiveManaCost) // Use effective mana cost after modifiers
        {
            Debug.Log($"<color=red>Cannot play {card.cardName}: Not enough mana! Required: {effectiveManaCost}, Available: {player.mana}</color>");
            
            // Animate the card to show it can't be played
            AnimateInsufficientManaCard(cardObj);
            
            // Flash the End Turn button to indicate player can't afford cards
            FlashEndTurnButton();
            return;
        }
        
        // Check for Bind status effect (prevents Guard cards)
        var playerDisplayForBind = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplayForBind != null)
        {
            var statusManager = playerDisplayForBind.GetStatusEffectManager();
            if (statusManager != null && statusManager.HasStatusEffect(StatusEffectType.Bind))
            {
                if (card.cardType == "Guard")
                {
                    Debug.Log($"<color=red>Cannot play {card.cardName}: You are Bound! Cannot play Guard cards.</color>");
                    
                    // Show feedback
                    var combatUI = UnityEngine.Object.FindFirstObjectByType<AnimatedCombatUI>();
                    if (combatUI != null)
                    {
                        combatUI.LogMessage($"<color=red>Bound!</color> Cannot play Guard cards.");
                    }
                    
                    AnimateInsufficientManaCard(cardObj); // Reuse animation for feedback
                    return;
                }
            }
        }
        
        // Get target enemy early (used in both delayed and normal play paths)
        Enemy targetEnemy = GetTargetEnemy();
        
        // Check if card should be delayed (Temporal Savant, etc.)
        int delayTurns = GetCardDelayTurns(card, player);
        if (delayTurns > 0)
        {
            // Spend mana NOW when delaying (mana is spent when queuing, not when executing)
            bool manaSpent = player.UseMana(effectiveManaCost);
            if (!manaSpent)
            {
                Debug.LogError($"Failed to spend mana for delayed card {card.cardName}!");
                return;
            }
            
            Debug.Log($"<color=green>Spent {effectiveManaCost} mana to queue {card.cardName} (delayed). Remaining mana: {player.mana}</color>");
            
            // Update mana display
            var playerDisplayMana = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplayMana != null)
            {
                playerDisplayMana.UpdateManaDisplay();
            }
            
            // Queue as delayed action instead of playing immediately
            DelayedAction delayedAction = new DelayedAction(card, delayTurns, targetEnemy, targetPosition);
            player.delayedActions.Add(delayedAction);
            
            // Remove from hand and visuals
            hand.RemoveAt(handIndex);
            handVisuals.RemoveAt(handIndex);
            
            // Reposition remaining cards
            if (cardRuntimeManager != null)
            {
                cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
            }
            
            // Return card visual to pool (don't animate it)
            if (cardRuntimeManager != null && cardObj != null)
            {
                cardRuntimeManager.ReturnCardToPool(cardObj);
            }
            
            Debug.Log($"<color=cyan>[Delayed] {card.cardName} queued for {delayTurns} turn(s) later! (Mana already spent)</color>");
            
            // Update UI
            UpdateCardUsability();
            var combatUIUpdate = FindFirstObjectByType<CombatUI>();
            if (combatUIUpdate != null)
            {
                combatUIUpdate.FlashEndTurnButton(); // Update UI feedback
            }
            
            return; // Don't play the card immediately
        }
        
        // Deduct mana cost before playing the card (using effective cost after modifiers)
        bool manaSpentNormal = player.UseMana(effectiveManaCost);
        if (!manaSpentNormal)
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
                if (logCardEffects)
                {
                    Debug.Log($"<color=yellow>[Combo] Combo will apply to {card.cardName}: Logic={comboApp.logic}, +Atk={comboApp.attackIncrease}, +Guard={comboApp.guardIncrease}, AoE={comboApp.isAoEOverride}, ManaRefund={comboApp.manaRefund}</color>");
                }
            }
        }
        
        // Update mana display and card usability
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            playerDisplay.UpdateManaDisplay();
        }
        UpdateCardUsability();
        
        // APOSTLE: Check if card requires discarding other cards
        // Check isDiscardCard property first, then fall back to description parsing for backwards compatibility
        bool hasIsDiscardCardFlag = card.isDiscardCard;
        bool hasDiscardInDescription = DiscardCardParser.RequiresDiscard(card.description);
        bool isDiscardCard = hasIsDiscardCardFlag || hasDiscardInDescription;
        
        if (logDiscardCheck)
        {
            Debug.Log($"[Discard Check] Card: {card.cardName}");
            Debug.Log($"[Discard Check]   - isDiscardCard property: {hasIsDiscardCardFlag}");
            Debug.Log($"[Discard Check]   - Has discard in description: {hasDiscardInDescription}");
            Debug.Log($"[Discard Check]   - Final isDiscardCard result: {isDiscardCard}");
        }
        
        if (isDiscardCard)
        {
            int discardCount = DiscardCardParser.ParseDiscardCount(card.description);
            // Default to 1 if parsing fails
            if (discardCount <= 0)
            {
                discardCount = 1;
            }
            Debug.Log($"<color=orange>[Discard Card] {card.cardName} requires discarding {discardCount} card(s) (isDiscardCard={card.isDiscardCard})</color>");
            
            // Show discard panel and wait for selection
            if (discardPanel == null)
            {
                discardPanel = FindFirstObjectByType<DiscardPanel>();
                Debug.Log($"[Discard Card] DiscardPanel lookup: {(discardPanel != null ? "FOUND" : "NOT FOUND")}");
            }
            
            if (discardPanel != null)
            {
                Debug.Log($"<color=green>[Discard Card] Showing DiscardPanel for {card.cardName}</color>");
                
                // Don't remove card from hand yet - wait for discard selection
                // Store references for callback
                int storedHandIndex = handIndex;
                GameObject storedCardObj = cardObj;
                Vector3 storedTargetPosition = targetPosition;
                Character storedPlayer = player;
                
                // Show discard panel
                discardPanel.ShowDiscardSelection(discardCount, card, (discardedCard) => {
                    Debug.Log($"<color=green>[Discard Card] Discard selection complete for {card.cardName}. Processing effect...</color>");
                    // Discard selection complete - now process the card effect
                    ProcessDiscardCardEffect(card, discardedCard, storedHandIndex, storedCardObj, storedTargetPosition, storedPlayer);
                });
                return; // Exit early - will continue in callback
            }
            else
            {
                Debug.LogError("[Discard Card] DiscardPanel not found! Cannot show discard selection.");
                // Fall through to normal card play (without discard)
            }
        }
        else
        {
            if (logDiscardCheck)
            {
                Debug.Log($"[Discard Check] Card {card.cardName} is NOT a discard card - proceeding with normal play");
            }
        }
        
        // Phase 2: Update input lock time (micro input lock for visual stability)
        lastCardPlayTime = Time.time;
        
        // PHASE 1 REFACTOR: Action Queue System
        // Remove from hand and visuals IMMEDIATELY
        // Note: Validation is intentionally skipped here because the card is in a transitional state
        // (removed from hand but not yet added to discard). Validation happens later after the card
        // is added to discard in ResolveCardAction.
        LogDeckState($"Before Removing Card from Hand: {card.cardName}");
        hand.RemoveAt(handIndex);
        handVisuals.RemoveAt(handIndex);
        LogDeckState($"After Removing Card from Hand: {card.cardName} (transitional state - card not yet in discard)");

        if (logCardOperations)
        {
            Debug.Log($"<color=yellow>[ActionQueue] Queueing card: {card.cardName}</color>");
        }
        
        // Get player character BEFORE queuing (targetEnemy already obtained above)
        Character playerCharacter = characterManager != null && characterManager.HasCharacter() ? 
            characterManager.GetCurrentCharacter() : null;
        
        // Create action and queue it
        CardAction action = new CardAction(card, cardObj, handIndex, targetPosition, targetEnemy, playerCharacter, comboApp);
        actionQueue.Enqueue(action);
        
        // Track this card as being played (update state to Queued)
        if (!cardsBeingPlayed.Contains(cardObj))
        {
            cardsBeingPlayed.Add(cardObj);
            cardPlayStates[cardObj] = new CardPlayState
            {
                card = card,
                cardObj = cardObj,
                effectsApplied = false,
                playStartTime = Time.time,
                state = CardState.Queued
            };
        }
        else
        {
            // Phase 2: Update existing state with validation
            TransitionCardState(cardObj, CardState.Queued, "PlayCard - card already being played");
        }
        
        // Process action queue IMMEDIATELY (resolve logic before animations)
        ProcessActionQueue();
        
        // Effects are now applied! Animation can proceed independently
        if (logCardOperations)
        {
            Debug.Log($"<color=green>[ActionQueue] {card.cardName} logic resolved. Starting animations (fire-and-forget)...</color>");
        }
        
        // Reposition remaining cards
        if (cardRuntimeManager != null)
        {
            List<GameObject> cardsToReposition = handVisuals.Where(v => v != null && !cardsBeingPlayed.Contains(v)).ToList();
            if (cardsToReposition.Count > 0)
            {
                cardRuntimeManager.RepositionCards(cardsToReposition, animated: true, duration: 0.3f);
            }
        }
        
        // CRITICAL: Cancel ALL LeanTween animations on this card ONLY!
        LeanTween.cancel(cardObj);
        LeanTween.cancel(cardObj, false);
        
        // Reset any transform changes that might be stuck
        cardObj.transform.localScale = Vector3.one;
        cardObj.transform.localRotation = Quaternion.identity;
        
        // Disable CardHoverEffect and interaction
        CardHoverEffect hoverEffect = cardObj.GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.enabled = false;
        }
        SetCardInteractable(cardObj, false);
        
        // ANIMATIONS ARE NOW FIRE-AND-FORGET - logic is already resolved!
        // Validate card object is still valid before starting animation
        if (cardObj == null || !cardObj.activeInHierarchy)
        {
            Debug.LogWarning($"[ActionQueue] Card object invalid for {card.cardName}! CardObj: {(cardObj != null ? "exists but inactive" : "null")}. Cleaning up...");
            CleanupCardAfterPlay(action);
            return;
        }
        
        // Check if we have animation support
        if (animationManager == null)
        {
            Debug.LogError("CombatAnimationManager is NULL! Cannot animate card play!");
            // Even without animation, card is already resolved - just clean up
            CleanupCardAfterPlay(action);
            return;
        }
        
        // Phase 2: Animation Kill Switch support
        // If CombatAnimationManager.disableAllAnimations is true, animations are skipped
        // and callbacks fire immediately. This validates that logic is independent of visuals.
        
        if (cardRuntimeManager != null && animationManager != null)
        {
            // Add safety timeout to ensure card is cleaned up even if animation fails
            StartCoroutine(SafetyTimeoutForCardAnimationCleanup(cardObj, card, action, 5.0f));
            
            // Start play animation (fire-and-forget - effects already applied!)
            // Note: If disableAllAnimations is true, this will immediately invoke the callback
            animationManager.AnimateCardPlay(cardObj, targetPosition, () => {
                // Animation complete callback - only handle visual cleanup, NO LOGIC
                if (logAnimations)
                {
                    Debug.Log($"  <color=cyan>[Animation] Card play animation complete for {card.cardName} - effects already applied</color>");
                }
                
                // Card is already in discard pile (added during ResolveCardAction)
                // Just invoke discard event for UI updates
                OnCardDiscarded?.Invoke(card);
                
                // Animate to discard pile (visual only)
                float effectDuration = 0.3f; // Brief pause to show effect
                LeanTween.delayedCall(gameObject, effectDuration, () => {
                    if (cardObj != null && discardPileTransform != null)
                    {
                        AnimateToDiscardPile(cardObj, card);
                    }
                    else if (cardObj != null)
                    {
                        // No discard animation - just clean up
                        CleanupCardAfterPlay(action);
                    }
                });
            });
        }
        else
        {
            // No animation support - action already processed, just clean up
            if (logCardOperations)
            {
                Debug.Log($"<color=yellow>[ActionQueue] No animation support - {card.cardName} effects already applied, cleaning up...</color>");
            }
            CleanupCardAfterPlay(action);
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
                
                // Validate card was properly discarded
                ValidateCardCount($"After Discard: {card.cardName}");
                
                // Reposition remaining cards with smooth animation
                cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
            });
        }
        else
        {
            // Immediate discard
            discardPile.Add(card);
            OnCardDiscarded?.Invoke(card);
            
            ValidateCardCount($"After Immediate Discard: {card.cardName}");
            
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
        }
        
        if (logCardOperations)
        {
            Debug.Log($"Discarded: {card.cardName}");
            LogDeckState($"After Discard: {card.cardName}");
        }
        
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

    #region Channeling

    private ChannelingTracker.ChannelingState UpdateChannelingState(Character owner, CardDataExtended cardData)
    {
        if (owner == null || cardData == null)
            return lastChannelingState;

        var state = owner.Channeling.RegisterCast(cardData.GetGroupKey());
        lastChannelingState = state;
        OnChannelingStateChanged?.Invoke(owner, state);
        return state;
    }

    private void ApplyChannelingMetadata(Card runtimeCard, ChannelingTracker.ChannelingState state, CardDataExtended sourceCard, bool bonusApplied)
    {
        if (runtimeCard == null)
            return;

        runtimeCard.channelingActive = state.isChanneling;
        runtimeCard.channelingStacks = state.consecutiveCasts;
        runtimeCard.channelingStartedThisCast = state.startedThisCast;
        runtimeCard.channelingStoppedThisCast = state.stoppedThisCast;
        runtimeCard.channelingGroupKey = sourceCard != null ? sourceCard.GetGroupKey() : string.Empty;
        runtimeCard.channelingBonusEnabled = sourceCard != null && sourceCard.channelingBonusEnabled;
        runtimeCard.channelingMinStacks = sourceCard != null ? Mathf.Max(1, sourceCard.channelingMinStacks) : runtimeCard.channelingMinStacks;
        runtimeCard.channelingAdditionalGuard = sourceCard != null ? sourceCard.channelingAdditionalGuard : runtimeCard.channelingAdditionalGuard;
        runtimeCard.channelingDamageIncreasedPercent = sourceCard != null ? sourceCard.channelingDamageIncreasedPercent : runtimeCard.channelingDamageIncreasedPercent;
        runtimeCard.channelingDamageMorePercent = sourceCard != null ? sourceCard.channelingDamageMorePercent : runtimeCard.channelingDamageMorePercent;
        runtimeCard.channelingGuardIncreasedPercent = sourceCard != null ? sourceCard.channelingGuardIncreasedPercent : runtimeCard.channelingGuardIncreasedPercent;
        runtimeCard.channelingGuardMorePercent = sourceCard != null ? sourceCard.channelingGuardMorePercent : runtimeCard.channelingGuardMorePercent;
        runtimeCard.channelingBonusApplied = bonusApplied;
    }

    public void ResetChannelingState(Character owner = null)
    {
        if (owner == null)
        {
            owner = characterManager != null && characterManager.HasCharacter()
                ? characterManager.GetCurrentCharacter()
                : null;
        }

        if (owner == null)
            return;

        lastChannelingState = owner.Channeling.BreakChannel();
        OnChannelingStateChanged?.Invoke(owner, lastChannelingState);
    }

    public void ResetSpeedMeters()
    {
        aggressionMeterValue = 0f;
        aggressionCharges = 0;
        focusMeterValue = 0f;
        focusCharges = 0;
        RaiseSpeedMeterChanged();
    }

    private void EnsureSpeedMeterDefaults()
    {
        if (aggressionChargeThreshold <= 0f)
            aggressionChargeThreshold = 100f;
        if (focusChargeThreshold <= 0f)
            focusChargeThreshold = 100f;

        if (aggressionBaseGainPercent <= 0f || Mathf.Approximately(aggressionBaseGainPercent, LegacyBaseGainPercent))
        {
            aggressionBaseGainPercent = aggressionChargeThreshold / DefaultAggressionHitsBaseline;
        }

        if (focusBaseGainPercent <= 0f || Mathf.Approximately(focusBaseGainPercent, LegacyBaseGainPercent))
        {
            focusBaseGainPercent = focusChargeThreshold / DefaultFocusHitsBaseline;
        }
    }

    private static readonly string SpellTagName = "Spell";

    private bool CardHasTag(CardDataExtended card, string tagName)
    {
        if (card == null || string.IsNullOrEmpty(tagName) || card.tags == null)
            return false;

        return card.tags.Any(t => string.Equals(t, tagName, StringComparison.OrdinalIgnoreCase));
    }

    private void ProcessSpeedMeters(CardDataExtended card, Character player)
    {
        if (card == null || player == null) return;

        if (CardHasTag(card, SpellTagName))
        {
            AddFocusProgress(player.GetCastSpeedMultiplier());
            return;
        }

        CardType type = card.GetCardTypeEnum();
        switch (type)
        {
            case CardType.Attack:
                AddAggressionProgress(player.GetAttackSpeedMultiplier());
                break;
            case CardType.Skill:
            case CardType.Power:
                AddFocusProgress(player.GetCastSpeedMultiplier());
                break;
        }
    }

    private void AddAggressionProgress(float speedMultiplier)
    {
        float gain = aggressionBaseGainPercent * Mathf.Max(0f, speedMultiplier);
        aggressionMeterValue += gain;
        while (aggressionMeterValue >= aggressionChargeThreshold)
        {
            aggressionMeterValue -= aggressionChargeThreshold;
            aggressionCharges++;
        }
        aggressionMeterValue = Mathf.Clamp(aggressionMeterValue, 0f, aggressionChargeThreshold);
        if (showSpeedMeterDebug)
        {
            Debug.Log($"[SpeedMeter] Aggression progress -> value {aggressionMeterValue:F1}/{aggressionChargeThreshold}, charges {aggressionCharges}");
        }
        RaiseSpeedMeterChanged();
    }

    private void AddFocusProgress(float speedMultiplier)
    {
        float gain = focusBaseGainPercent * Mathf.Max(0f, speedMultiplier);
        focusMeterValue += gain;
        while (focusMeterValue >= focusChargeThreshold)
        {
            focusMeterValue -= focusChargeThreshold;
            focusCharges++;
        }
        focusMeterValue = Mathf.Clamp(focusMeterValue, 0f, focusChargeThreshold);
        if (showSpeedMeterDebug)
        {
            Debug.Log($"[SpeedMeter] Focus progress -> value {focusMeterValue:F1}/{focusChargeThreshold}, charges {focusCharges}");
        }
        RaiseSpeedMeterChanged();
    }

    /// <summary>
    /// Consume one Aggression charge (called when a charge effect is used).
    /// </summary>
    public void ConsumeAggressionCharge()
    {
        if (aggressionCharges > 0)
        {
            aggressionCharges--;
            RaiseSpeedMeterChanged();
            Debug.Log($"[SpeedMeter] Consumed 1 Aggression charge. Remaining: {aggressionCharges}");
        }
        else
        {
            Debug.LogWarning("[SpeedMeter] Attempted to consume Aggression charge but none available!");
        }
    }

    /// <summary>
    /// Consume one Focus charge (called when a charge effect is used).
    /// </summary>
    public void ConsumeFocusCharge()
    {
        if (focusCharges > 0)
        {
            focusCharges--;
            RaiseSpeedMeterChanged();
            Debug.Log($"[SpeedMeter] Consumed 1 Focus charge. Remaining: {focusCharges}");
        }
        else
        {
            Debug.LogWarning("[SpeedMeter] Attempted to consume Focus charge but none available!");
        }
    }

    #region Charge Modifier Helpers

    private void ResetChargeModifiers()
    {
        selectedAggressionEffect = AggressionChargeEffect.None;
        selectedFocusEffect = FocusChargeEffect.None;
        aggressionModifierConsumed = false;
        focusModifierConsumed = false;
        OnChargeModifiersCleared?.Invoke();
    }

    public AggressionChargeEffect GetAggressionChargeEffect()
    {
        return aggressionModifierConsumed ? AggressionChargeEffect.None : selectedAggressionEffect;
    }

    public FocusChargeEffect GetFocusChargeEffect()
    {
        return focusModifierConsumed ? FocusChargeEffect.None : selectedFocusEffect;
    }

    public bool SelectAggressionChargeEffect(AggressionChargeEffect effect)
    {
        if (effect == AggressionChargeEffect.None)
        {
            ClearAggressionChargeEffect();
            return true;
        }

        if (aggressionCharges <= 0)
        {
            Debug.LogWarning("[ChargeModifiers] No Aggression charges available to assign effect.");
            return false;
        }

        selectedAggressionEffect = effect;
        aggressionModifierConsumed = false;
        OnAggressionModifierSelected?.Invoke(effect);
        Debug.Log($"[ChargeModifiers] Selected Aggression effect: {effect}");
        return true;
    }

    public bool SelectFocusChargeEffect(FocusChargeEffect effect)
    {
        if (effect == FocusChargeEffect.None)
        {
            ClearFocusChargeEffect();
            return true;
        }

        if (focusCharges <= 0)
        {
            Debug.LogWarning("[ChargeModifiers] No Focus charges available to assign effect.");
            return false;
        }

        selectedFocusEffect = effect;
        focusModifierConsumed = false;
        OnFocusModifierSelected?.Invoke(effect);
        if (logCardEffects)
        {
            Debug.Log($"[ChargeModifiers] Selected Focus effect: {effect}");
        }
        return true;
    }

    public void ClearAggressionChargeEffect()
    {
        selectedAggressionEffect = AggressionChargeEffect.None;
        aggressionModifierConsumed = false;
        OnAggressionModifierSelected?.Invoke(selectedAggressionEffect);
    }

    public void ClearFocusChargeEffect()
    {
        selectedFocusEffect = FocusChargeEffect.None;
        focusModifierConsumed = false;
        OnFocusModifierSelected?.Invoke(selectedFocusEffect);
    }

    public void ConsumeChargeModifiers()
    {
        if (selectedAggressionEffect != AggressionChargeEffect.None && !aggressionModifierConsumed)
        {
            aggressionModifierConsumed = true;
            ConsumeAggressionCharge();
        }

        if (selectedFocusEffect != FocusChargeEffect.None && !focusModifierConsumed)
        {
            focusModifierConsumed = true;
            ConsumeFocusCharge();
        }

        ResetChargeModifiers();
    }

    private int ApplyManaCostModifierInternal(CardDataExtended card, int baseManaCost)
    {
        int modifiedCost = baseManaCost;
        
        // Apply Focus charge modifiers
        if (GetFocusChargeEffect() == FocusChargeEffect.HalfManaCost)
        {
            modifiedCost = Mathf.Max(0, Mathf.RoundToInt(modifiedCost * 0.5f));
            if (logCardEffects)
            {
                Debug.Log($"[ChargeModifiers] Half Mana Cost applied: {baseManaCost} -> {modifiedCost}");
            }
        }
        
        // Apply momentum threshold cost reduction (e.g., "6+ Momentum: This card costs 1 less")
        if (card != null && !string.IsNullOrEmpty(card.momentumEffectDescription))
        {
            Character player = characterManager != null && characterManager.HasCharacter() ? 
                characterManager.GetCurrentCharacter() : null;
            
            if (player != null)
            {
                int currentMomentum = player.GetMomentum();
                var thresholdEffects = MomentumThresholdEffectParser.ParseThresholdEffects(card.momentumEffectDescription);
                
                foreach (var effect in thresholdEffects)
                {
                    bool applies = effect.isExact ? (currentMomentum == effect.threshold) : (currentMomentum >= effect.threshold);
                    if (applies)
                    {
                        var effectType = MomentumThresholdEffectParser.ParseEffectType(effect.effectText);
                        if (effectType == MomentumEffectType.CostReduction)
                        {
                            int reduction = MomentumThresholdEffectParser.ParseNumericValue(effect.effectText);
                            if (reduction > 0)
                            {
                                modifiedCost = Mathf.Max(0, modifiedCost - reduction);
                                Debug.Log($"<color=cyan>[Momentum Threshold] {card.cardName}: Cost reduced by {reduction} (had {currentMomentum} momentum). New cost: {modifiedCost}</color>");
                            }
                        }
                    }
                }
            }
        }
        
        return modifiedCost;
    }

    private float ApplyDamageModifierInternal(float baseDamage)
    {
        if (GetFocusChargeEffect() == FocusChargeEffect.DoubleDamage)
        {
            float modified = baseDamage * 2f;
            if (logCardEffects)
            {
                Debug.Log($"[ChargeModifiers] Double Damage applied: {baseDamage} -> {modified}");
            }
            return modified;
        }
        return baseDamage;
    }

    private void ApplyCardModifiersInternal(Card card, CardDataExtended cardData)
    {
        var effect = GetAggressionChargeEffect();
        switch (effect)
        {
            case AggressionChargeEffect.HitsAllEnemies:
                card.isAoE = true;
                if (logCardEffects)
                {
                    Debug.Log("[ChargeModifiers] Aggression effect: Hits All Enemies");
                }
                break;
            case AggressionChargeEffect.AlwaysCrit:
                if (logCardEffects)
                {
                    Debug.Log("[ChargeModifiers] Aggression effect: Always Crit");
                }
                break;
            case AggressionChargeEffect.IgnoresGuardArmor:
                if (logCardEffects)
                {
                    Debug.Log("[ChargeModifiers] Aggression effect: Ignores Guard/Armor");
                }
                break;
        }
    }

    public static int ApplyManaCostModifier(CardDataExtended card, int baseManaCost)
    {
        return Instance != null ? Instance.ApplyManaCostModifierInternal(card, baseManaCost) : baseManaCost;
    }
    
    /// <summary>
    /// Get the display cost for a card (includes all modifiers)
    /// For Skill cards: calculates percentage-based cost first, then applies modifiers
    /// For Attack cards: applies modifiers to flat cost
    /// </summary>
    public static int GetDisplayCost(CardDataExtended card, int baseManaCost, Character character)
    {
        if (card == null) return baseManaCost;
        
        // For Skill cards with percentage cost, calculate percentage-based cost first
        int calculatedCost = baseManaCost;
        if (string.Equals(card.cardType, "Skill", System.StringComparison.OrdinalIgnoreCase) && character != null && card.percentageManaCost > 0)
        {
            // percentageManaCost is a percentage (e.g., 10 = 10% of maxMana)
            // Use CeilToInt to round up (e.g., 4.5 -> 5, not 4)
            float percentageCost = card.percentageManaCost / 100.0f;
            calculatedCost = Mathf.CeilToInt(character.maxMana * percentageCost);
        }
        
        // Apply mana cost modifiers (Focus charge effects, momentum reductions, etc.)
        int displayCost = ApplyManaCostModifier(card, calculatedCost);
        
        return displayCost;
    }
    
    /// <summary>
    /// Add a card directly to the discard pile (without animation)
    /// Used when unleashed cards need to be added to discard
    /// </summary>
    public void AddCardToDiscardPile(CardDataExtended card)
    {
        if (card == null)
        {
            Debug.LogWarning("[CombatDeckManager] Cannot add null card to discard pile");
            return;
        }
        
        // Prevent duplicates
        if (discardPile.Contains(card))
        {
            Debug.LogWarning($"[CombatDeckManager] Card {card.cardName} already in discard pile, skipping duplicate add");
            return;
        }
        
        LogDeckState($"Before Adding Unleashed Card to Discard: {card.cardName}");
        discardPile.Add(card);
        LogDeckState($"After Adding Unleashed Card to Discard: {card.cardName}");
        ValidateCardCount($"After Adding Unleashed Card to Discard: {card.cardName}");
        
        OnCardDiscarded?.Invoke(card);
        Debug.Log($"[CombatDeckManager] Added {card.cardName} to discard pile (count: {discardPile.Count})");
    }

    public static float ApplyDamageModifier(float baseDamage)
    {
        return Instance != null ? Instance.ApplyDamageModifierInternal(baseDamage) : baseDamage;
    }

    public static void ApplyCardModifiers(Card card, CardDataExtended cardData)
    {
        if (Instance != null)
        {
            Instance.ApplyCardModifiersInternal(card, cardData);
        }
    }

    public static bool ShouldHitTwice()
    {
        return Instance != null && Instance.GetAggressionChargeEffect() == AggressionChargeEffect.HitsTwice;
    }

    public static bool ShouldAlwaysCrit()
    {
        return Instance != null && Instance.GetAggressionChargeEffect() == AggressionChargeEffect.AlwaysCrit;
    }

    public static bool ShouldIgnoreGuardArmor()
    {
        return Instance != null && Instance.GetAggressionChargeEffect() == AggressionChargeEffect.IgnoresGuardArmor;
    }

    public static bool HasAggressionModifier()
    {
        return Instance != null && Instance.GetAggressionChargeEffect() != AggressionChargeEffect.None;
    }

    public static bool HasFocusModifier()
    {
        return Instance != null && Instance.GetFocusChargeEffect() != FocusChargeEffect.None;
    }

    #endregion

    private void RaiseSpeedMeterChanged()
    {
        OnSpeedMeterChanged?.Invoke(CurrentSpeedMeterState);
    }

    private void ShowChannelingPopup(string message, Color color)
    {
        CombatAnimationManager anim = CombatAnimationManager.Instance;
        if (anim == null) return;

        Vector3 position = GetPlayerPopupPosition();
        anim.ShowFloatingText(message, position, color, 38);
    }

    private Vector3 GetPlayerPopupPosition()
    {
        if (cachedPlayerDisplay == null)
        {
            cachedPlayerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        }

        if (cachedPlayerDisplay != null)
        {
            RectTransform rect = cachedPlayerDisplay.GetComponent<RectTransform>();
            Vector3 basePosition = rect != null ? rect.position : cachedPlayerDisplay.transform.position;
            float offsetY = 0f;
            if (rect != null)
            {
                offsetY = rect.rect.height * cachedPlayerDisplay.transform.lossyScale.y * 0.5f + 80f;
            }
            else
            {
                offsetY = 120f;
            }
            return basePosition + new Vector3(0f, offsetY, 0f);
        }

        return new Vector3(Screen.width * 0.25f, Screen.height * 0.5f, 0f);
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
        
        // Remove existing click handler if any
        CardClickHandler existingHandler = cardObj.GetComponent<CardClickHandler>();
        if (existingHandler != null)
        {
            Destroy(existingHandler);
        }
        
        // Add our custom click handler that supports right-click and shift-click
        CardClickHandler clickHandler = cardObj.AddComponent<CardClickHandler>();
        clickHandler.Initialize(this, cardObj);
        
        // Also keep Button for left-click compatibility
        UnityEngine.UI.Button button = cardObj.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnCardClicked(cardObj, false, false));
        }
        else
        {
            Debug.LogWarning($"<color=red>{cardObj.name} has NO Button component!</color>");
        }
    }
    
    public void OnCardClicked(GameObject clickedCardObj, bool isRightClick = false, bool isShiftClick = false)
    {
        // Phase 2: Micro Input Lock - prevent rapid clicking from causing visual chaos
        // Note: This is a VERY short lock (50ms default) - feels instant but prevents spam
        // Actions are still queued, so logic remains unaffected - this is purely for visual stability
        float currentTime = Time.time;
        if (lastCardPlayTime >= 0f && (currentTime - lastCardPlayTime) < inputLockDuration)
        {
            float timeSinceLastPlay = currentTime - lastCardPlayTime;
            Debug.Log($"<color=orange>[Input Lock] Ignoring rapid click (last play {timeSinceLastPlay * 1000:F1}ms ago, lock: {inputLockDuration * 1000}ms)</color>");
            return; // Ignore this click - too soon after last one
        }
        
        if (logCardClickDebug)
        {
            Debug.Log($"<color=cyan>═══ CARD CLICK DEBUG ═══</color>");
            Debug.Log($"Clicked card GameObject: {clickedCardObj.name}");
            Debug.Log($"Hand visuals count: {handVisuals.Count}");
            Debug.Log($"Hand data count: {hand.Count}");
            Debug.Log($"Right-click: {isRightClick}, Shift-click: {isShiftClick}");
        }
        
        // Find the index of this card in the hand visuals
        int handIndex = handVisuals.IndexOf(clickedCardObj);
        
        if (logCardClickDebug)
        {
            Debug.Log($"Card index in handVisuals: {handIndex}");
        }
        
        if (handIndex < 0)
        {
            if (logCriticalErrors)
            {
                Debug.LogWarning($"Clicked card not found in hand visuals!");
            }
            if (logCardClickDebug)
            {
                Debug.Log($"Card active: {clickedCardObj.activeInHierarchy}");
                Debug.Log($"Card parent: {clickedCardObj.transform.parent?.name}");
            }
            return;
        }
        
        if (handIndex >= hand.Count)
        {
            if (logCriticalErrors)
            {
                Debug.LogWarning($"Hand index out of range! Index: {handIndex}, Hand count: {hand.Count}");
            }
            return;
        }
        
        // Get the card data - NOW USING CardDataExtended!
        CardDataExtended clickedCard = hand[handIndex];
        
        // Check if this is a preparation click (right-click or shift-click)
        if (isRightClick || isShiftClick)
        {
            PrepareCard(handIndex);
            return;
        }
        
        // Get target position from first available enemy
        Vector3 targetPos = GetTargetScreenPosition();
        
        if (logCardClickDebug || logCardOperations)
        {
            Debug.Log($"<color=yellow>Card clicked: {clickedCard.cardName} (Index: {handIndex}), Target pos: {targetPos}</color>");
        }
        if (logCardClickDebug)
        {
            Debug.Log($"About to call PlayCard({handIndex}, {targetPos})");
        }
        
        // Play the card (this will update lastCardPlayTime)
        PlayCard(handIndex, targetPos);
        
        if (logCardClickDebug)
        {
            Debug.Log($"<color=cyan>═══ END CLICK DEBUG ═══</color>");
        }
    }
    
    /// <summary>
    /// Prepare a card instead of playing it (called on right-click or shift-click)
    /// </summary>
    private void PrepareCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"Invalid hand index for preparation: {handIndex}");
            return;
        }
        
        CardDataExtended card = hand[handIndex];
        Character player = CharacterManager.Instance?.GetCurrentCharacter();
        
        if (player == null)
        {
            Debug.LogError("No player character found for preparation!");
            return;
        }
        
        // Check if we have space for preparation
        var prepManager = PreparationManager.Instance;
        if (prepManager == null)
        {
            Debug.LogError("PreparationManager not found!");
            return;
        }
        
        if (!prepManager.CanPrepareCard(card))
        {
            Debug.LogWarning($"Cannot prepare {card.cardName}: No space or invalid card!");
            return;
        }
        
        // Prepare the card
        // Note: Card is removed from hand but NOT added to discard - it's now in "prepared" state
        // Card count validation includes prepared cards, so no mismatch should occur
        LogDeckState($"Before Preparing Card: {card.cardName}");
        bool success = prepManager.PrepareCard(card, player);
        
        if (success)
        {
            // Remove card from hand and visuals
            hand.RemoveAt(handIndex);
            GameObject cardObj = handVisuals[handIndex];
            handVisuals.RemoveAt(handIndex);
            
            // Return card visual to pool
            if (cardRuntimeManager != null && cardObj != null)
            {
                cardRuntimeManager.ReturnCardToPool(cardObj);
            }
            
            // Reposition remaining cards
            if (cardRuntimeManager != null)
            {
                cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
            }
            
            LogDeckState($"After Preparing Card: {card.cardName} (card now in prepared state, not in discard)");
            ValidateCardCount($"After Preparing Card: {card.cardName}");
            
            Debug.Log($"<color=green>[Preparation] {card.cardName} prepared successfully!</color>");
            
            // Update UI
            UpdateCardUsability();
            var combatUIUpdate = FindFirstObjectByType<CombatUI>();
            if (combatUIUpdate != null)
            {
                combatUIUpdate.FlashEndTurnButton(); // Update UI feedback
            }
        }
        else
        {
            Debug.LogWarning($"Failed to prepare {card.cardName}!");
        }
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
                if (logComboGlow)
                {
                    Debug.Log($"[ComboGlow] {cardData.cardName}: GlowEffect set to {(eligible ? "ACTIVE" : "INACTIVE")}");
                }
                
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
    
    /// <summary>
    /// Phase 2: Safety timeout for card animation cleanup - ensures card is cleaned up even if animation fails
    /// Enhanced with state validation
    /// </summary>
    private IEnumerator SafetyTimeoutForCardAnimationCleanup(GameObject cardObj, CardDataExtended card, CardAction action, float timeout)
    {
        yield return new WaitForSeconds(timeout);
        
        // Check if card is still in cardsBeingPlayed (means animation didn't complete)
        if (cardObj != null && cardsBeingPlayed.Contains(cardObj))
        {
            Debug.LogWarning($"[Safety Timeout] Card {card.cardName} animation timeout after {timeout}s! Animation likely failed. Force cleaning up...");
            
            // Phase 2: Validate state before cleanup
            if (cardPlayStates.ContainsKey(cardObj))
            {
                CardState currentState = cardPlayStates[cardObj].state;
                Debug.LogWarning($"[Safety Timeout] Card state at timeout: {currentState}");
                
                // If card is stuck in Resolving or Resolved, log it as a bug
                if (currentState == CardState.Resolving || currentState == CardState.Resolved)
                {
                    Debug.LogError($"[Safety Timeout] Card {card.cardName} stuck in {currentState} state - this indicates logic was applied but cleanup failed!");
                }
            }
            
            // Cancel any animations on this card
            LeanTween.cancel(cardObj);
            
            // Force cleanup using the action
            CleanupCardAfterPlay(action);
        }
    }
    
    /// <summary>
    /// Safety timeout for stuck card animations - ensures effects are applied and card is cleaned up
    /// </summary>
    private IEnumerator SafetyTimeoutForCardAnimation(GameObject cardObj, CardDataExtended card, Enemy targetEnemy, Character playerCharacter, Vector3 targetPosition, ComboSystem.ComboApplication comboApp, float timeout)
    {
        yield return new WaitForSeconds(timeout);
        
        // Check if card is still active (animation might be stuck)
        if (cardObj != null && cardObj.activeInHierarchy)
        {
            Debug.LogWarning($"[Safety Timeout] Card animation stuck for {card.cardName}! Force applying effects and cleaning up...");
            
            // Cancel all animations
            LeanTween.cancel(cardObj);
            
            // Check if effects have been applied - if not, apply them now
            if (!cardPlayStates.ContainsKey(cardObj) || !cardPlayStates[cardObj].effectsApplied)
            {
                Debug.LogWarning($"[Safety Timeout] Effects NOT applied for {card.cardName}! Applying now...");
                ApplyCardEffectsInternal(card, cardObj, targetEnemy, playerCharacter, targetPosition, comboApp);
            }
            
            // Ensure card is added to discard pile if not already
            if (!discardPile.Contains(card))
            {
                discardPile.Add(card);
                OnCardDiscarded?.Invoke(card);
                Debug.Log($"[Safety Timeout] Added {card.cardName} to discard pile");
            }
            
            // Force return card to pool - this ensures it's cleaned up
            if (cardRuntimeManager != null)
            {
                var activeCardsList = cardRuntimeManager.GetActiveCards();
                if (activeCardsList.Contains(cardObj))
                {
                    cardRuntimeManager.RemoveCard(cardObj);
                }
                else
                {
                    cardRuntimeManager.ReturnCardToPool(cardObj);
                }
                Debug.Log($"[Safety Timeout] Force returned {card.cardName} to pool");
            }
            else
            {
                Destroy(cardObj);
                Debug.Log($"[Safety Timeout] Destroyed {card.cardName} (no pool manager)");
            }
            
            // Clean up tracking
            if (cardsBeingPlayed.Contains(cardObj))
            {
                cardsBeingPlayed.Remove(cardObj);
            }
            if (cardPlayStates.ContainsKey(cardObj))
            {
                cardPlayStates.Remove(cardObj);
            }
        }
    }
    
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
            if (!button.enabled)
            {
                button.enabled = true;
            }
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
    /// <summary>
    /// Get the number of turns to delay a card (checks card property, tags, and ascendancy)
    /// </summary>
    private int GetCardDelayTurns(CardDataExtended card, Character player)
    {
        if (card == null) return 0;
        
        // Check card's delayTurns property
        if (card.delayTurns > 0)
        {
            return card.delayTurns;
        }
        
        // Check if card has "Delayed" tag
        if (card.tags != null && card.tags.Contains("Delayed"))
        {
            return 1; // Default 1 turn delay for Delayed tag
        }
        
        // Check if card is marked as delayed (set by ascendancy or effect)
        if (card.isDelayed)
        {
            return 1; // Default 1 turn delay
        }
        
        // TODO: Check ascendancy effects (Temporal Savant)
        // This would check if player has Temporal Savant ascendancy and apply delay
        
        return 0; // No delay
    }
    
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
    
    #region Unleash Card Handling (Best Practice Pipeline)
    
    /// <summary>
    /// Queue an unleashed card action for processing through the action queue system
    /// Follows the same pipeline as regular card plays: Queue → Resolve → Discard
    /// </summary>
    public void QueueUnleashCard(CardDataExtended card, Vector3 targetPosition, Enemy targetEnemy, Character playerCharacter, float unleashDamage, float unleashMultiplier)
    {
        if (card == null || playerCharacter == null)
        {
            Debug.LogError("[CombatDeckManager] Cannot queue unleash: Invalid card or player!");
            return;
        }
        
        if (logCardOperations)
        {
            Debug.Log($"<color=yellow>[ActionQueue] Queueing unleashed card: {card.cardName}</color>");
        }
        
        // Create action for unleashed card (no cardVisual since it's not in hand)
        CardAction action = new CardAction(card, targetPosition, targetEnemy, playerCharacter, unleashDamage, unleashMultiplier);
        actionQueue.Enqueue(action);
        
        // Process immediately (same as regular cards)
        ProcessActionQueue();
        
        if (logCardOperations)
        {
            Debug.Log($"<color=green>[ActionQueue] {card.cardName} (unleashed) logic resolved. Effects applied through action queue.</color>");
        }
    }
    
    /// <summary>
    /// Apply unleash-specific effects (preparation multipliers, etc.) through the same pipeline as regular cards
    /// </summary>
    private void ApplyUnleashEffects(CardAction action)
    {
        if (!action.isUnleash || !action.unleashDamage.HasValue)
        {
            Debug.LogError("[CombatDeckManager] ApplyUnleashEffects called on non-unleash action!");
            return;
        }
        
        CardDataExtended card = action.card;
        Character player = action.playerCharacter;
        float unleashDamage = action.unleashDamage.Value;
        float unleashMultiplier = action.unleashMultiplier ?? 0f;
        
        if (logCardEffects)
        {
            Debug.Log($"<color=cyan>Applying unleash effects for {card.cardName} (damage: {unleashDamage:F1}, multiplier: {unleashMultiplier:F2})...</color>");
        }
        
        // Get CardEffectProcessor to use the full card processing pipeline
        if (cardEffectProcessor == null)
        {
            Debug.LogError("[CombatDeckManager] CardEffectProcessor not found! Cannot apply unleash effects.");
            return;
        }
        
        // Convert CardDataExtended to Card for processing
        #pragma warning disable CS0618 // Type or member is obsolete
        Card cardForProcessor = card.ToCard();
        #pragma warning restore CS0618
        
        // Apply preparation multipliers to the card before processing
        // This mirrors the logic from PreparationManager.DealUnleashDamage
        // Check if card has momentum-based effects using MomentumEffectParser
        bool isMomentumBased = MomentumEffectParser.HasPerMomentumSpent(card.description);
        if (!isMomentumBased)
        {
            // For non-momentum cards, use the calculated unleash damage directly
            cardForProcessor.baseDamage = unleashDamage;
        }
        else
        {
            // For momentum-based cards, apply preparation multiplier to baseDamage per momentum
            float originalBaseDamage = cardForProcessor.baseDamage;
            float preparationMultiplier = 1f + unleashMultiplier;
            cardForProcessor.baseDamage = originalBaseDamage * preparationMultiplier;
            if (logCardEffects)
            {
                Debug.Log($"<color=cyan>[Preparation] Momentum-based card {card.cardName} - applying multiplier: {originalBaseDamage} * {preparationMultiplier:F2} = {cardForProcessor.baseDamage:F1} per momentum</color>");
            }
        }
        
        // Apply charge modifiers (same as regular cards)
        ApplyCardModifiers(cardForProcessor, card);
        
        // Process speed meters (same as regular cards)
        ProcessSpeedMeters(card, player);
        
        // Check card type - Guard cards apply guard, others deal damage
        if (card.GetCardTypeEnum() == CardType.Guard)
        {
            // Guard cards: Apply base guard using CardEffectProcessor
            cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, null, player, Vector3.zero, false);
            
            // Apply prepared card guard bonus (handled here instead of PreparationManager for pipeline consistency)
            if (card.preparedCardGuardBase > 0f || 
                (card.preparedCardGuardScaling != null && 
                 (card.preparedCardGuardScaling.strengthDivisor > 0f || 
                  card.preparedCardGuardScaling.dexterityDivisor > 0f || 
                  card.preparedCardGuardScaling.intelligenceDivisor > 0f ||
                  card.preparedCardGuardScaling.strengthScaling > 0f ||
                  card.preparedCardGuardScaling.dexterityScaling > 0f ||
                  card.preparedCardGuardScaling.intelligenceScaling > 0f)))
            {
                // Calculate guard per prepared card (including scaling)
                float guardPerCard = card.preparedCardGuardBase;
                if (card.preparedCardGuardScaling != null)
                {
                    guardPerCard += card.preparedCardGuardScaling.CalculateScalingBonus(player);
                }
                
                // Get the number of prepared cards (this card was already removed from the list in ExecuteUnleash)
                var prepManager = PreparationManager.Instance;
                int preparedCount = 1; // This card itself
                if (prepManager != null)
                {
                    var remainingPrepared = prepManager.GetPreparedCards();
                    preparedCount += (remainingPrepared != null ? remainingPrepared.Count : 0);
                }
                
                // Apply guard with preparation multiplier
                float bonusGuard = guardPerCard * preparedCount * (1f + unleashMultiplier);
                player.AddGuard(bonusGuard);
                
                if (logCardEffects)
                {
                    Debug.Log($"<color=cyan>[Preparation Bonus] {card.cardName} gained {bonusGuard:F1} bonus guard from preparation (base: {card.preparedCardGuardBase} per card, scaling: {card.preparedCardGuardScaling?.CalculateScalingBonus(player) ?? 0f}, prepared count: {preparedCount}, multiplier: {1f + unleashMultiplier:F2})</color>");
                }
                
                // Update guard display
                PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplay != null)
                {
                    playerDisplay.UpdateGuardDisplay();
                }
            }
        }
        else
        {
            // Attack/Spell cards: Deal damage using CardEffectProcessor (handles AoE, targeting, etc.)
            if (cardForProcessor.isAoE)
            {
                cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, null, player, action.targetPosition);
            }
            else if (action.targetEnemy != null)
            {
                cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, action.targetEnemy, player, action.targetPosition);
            }
            else
            {
                Debug.LogWarning($"Cannot apply unleash {card.cardName}: No target enemy for single-target card!");
                return;
            }
        }
        
        // Play combat effects (same as regular cards)
        if (combatEffectManager != null)
        {
            PlayCardEffects(card, action.targetEnemy, player);
        }
        
        // Apply card abilities (same as regular cards)
        CardAbilityRouter.ApplyCardPlay(card, cardForProcessor, player, action.targetEnemy, action.targetPosition);
        
        // Trigger "hits twice" effect if applicable (same as regular cards)
        if (ShouldHitTwice() && action.targetEnemy != null && !cardForProcessor.isAoE)
        {
            if (logCardEffects)
            {
                Debug.Log($"<color=yellow>[ChargeModifier] HitsTwice: Applying unleash effect again!</color>");
            }
            cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, action.targetEnemy, player, action.targetPosition);
        }
        
        // Boss Ability Handler - card played (same as regular cards)
        cardsPlayedThisTurn++;
        BossAbilityHandler.OnPlayerCardPlayed(cardForProcessor, cardsPlayedThisTurn);
        
        // Fire OnCardPlayed event (same as regular cards) - pass CardDataExtended, not Card
        OnCardPlayed?.Invoke(card);
    }
    
    #endregion
    
    /// <summary>
    /// Apply card effects - extracted method that can be called independently of animation callbacks
    /// </summary>
    private void ApplyCardEffectsInternal(CardDataExtended card, GameObject cardObj, Enemy targetEnemy, Character playerCharacter, Vector3 targetPosition, ComboSystem.ComboApplication comboApp)
    {
        // Check if effects have already been applied for this card
        if (cardPlayStates.ContainsKey(cardObj) && cardPlayStates[cardObj].effectsApplied)
        {
            Debug.LogWarning($"  → [CardPlay] Effects already applied for {card.cardName}! Skipping duplicate application.");
            return;
        }
        
        if (logCardEffects)
        {
            Debug.Log($"<color=cyan>Applying card effects for {card.cardName}...</color>");
            Debug.Log($"  CardEffectProcessor: {(cardEffectProcessor != null ? "Found" : "NULL")}");
            Debug.Log($"  TargetingManager: {(targetingManager != null ? "Found" : "NULL")}");
            Debug.Log($"  Target Enemy: {(targetEnemy != null ? targetEnemy.enemyName : "NULL")}");
            Debug.Log($"  Player Character: {(playerCharacter != null ? playerCharacter.characterName : "NULL")}");
            Debug.Log($"  Is AoE Card: {card.isAoE}");
        }
        
        // Mark effects as applied
        if (cardPlayStates.ContainsKey(cardObj))
        {
            cardPlayStates[cardObj].effectsApplied = true;
        }
        
        // DIVINE FAVOR: Check if next card should apply discarded effect
        if (nextCardAppliesDiscardedEffect && !string.IsNullOrEmpty(card.ifDiscardedEffect))
        {
            if (logCardEffects)
            {
                Debug.Log($"<color=yellow>[Divine Favor] {card.cardName} applies its discarded effect!</color>");
            }
            ProcessIfDiscardedEffect(card, playerCharacter);
            nextCardAppliesDiscardedEffect = false; // Consume the effect
        }
        
        var channelingState = UpdateChannelingState(playerCharacter, card);
        bool channelingBonusApplied = false;
        if (playerCharacter != null)
        {
            if (channelingState.startedThisCast)
            {
                if (logCardEffects)
                {
                    Debug.Log($"<color=cyan>[Channeling]</color> {playerCharacter.characterName} began channeling {channelingState.activeGroupKey} ({channelingState.consecutiveCasts} casts).");
                }
                ShowChannelingPopup("Channeling!", ChannelingStartColor);
            }
            else if (channelingState.isChanneling)
            {
                if (logCardEffects)
                {
                    Debug.Log($"<color=cyan>[Channeling]</color> Channeling continues ({channelingState.consecutiveCasts} casts).");
                }
            }
            else if (channelingState.stoppedThisCast && !channelingState.startedThisCast)
            {
                if (logCardEffects)
                {
                    Debug.Log($"<color=cyan>[Channeling]</color> Channeling ended before playing {card.cardName}.");
                }
                ShowChannelingPopup("Channeling Broken", ChannelingEndColor);
            }
        }

        ProcessSpeedMeters(card, playerCharacter);
        
        if (cardEffectProcessor != null)
        {
            // TEMPORARY: Convert CardDataExtended to Card for CardEffectProcessor
            #pragma warning disable CS0618 // Type or member is obsolete
            Card cardForProcessor = card.ToCard();
            #pragma warning restore CS0618
            
            // Apply charge modifiers to the card
            ApplyCardModifiers(cardForProcessor, card);
            int requiredStacks = Mathf.Max(1, card.channelingMinStacks);
            if (card.channelingBonusEnabled && channelingState.consecutiveCasts >= requiredStacks)
            {
                if (!Mathf.Approximately(card.channelingAdditionalGuard, 0f))
                {
                    cardForProcessor.baseGuard += card.channelingAdditionalGuard;
                }
                if (!Mathf.Approximately(card.channelingDamageIncreasedPercent, 0f))
                {
                    cardForProcessor.baseDamage = Mathf.Max(0f, cardForProcessor.baseDamage * (1f + card.channelingDamageIncreasedPercent / 100f));
                }
                if (!Mathf.Approximately(card.channelingDamageMorePercent, 0f))
                {
                    cardForProcessor.baseDamage = Mathf.Max(0f, cardForProcessor.baseDamage * (1f + card.channelingDamageMorePercent / 100f));
                }
                if (!Mathf.Approximately(card.channelingGuardIncreasedPercent, 0f) || !Mathf.Approximately(card.channelingGuardMorePercent, 0f))
                {
                    float guardValue = cardForProcessor.baseGuard;
                    guardValue *= (1f + card.channelingGuardIncreasedPercent / 100f);
                    guardValue *= (1f + card.channelingGuardMorePercent / 100f);
                    cardForProcessor.baseGuard = guardValue;
                }
                channelingBonusApplied = true;
                if (logCardEffects)
                {
                    Debug.Log($"<color=cyan>[Channeling]</color> Bonus applied → Damage +{card.channelingDamageIncreasedPercent}% inc, +{card.channelingDamageMorePercent}% more, Guard +{card.channelingGuardIncreasedPercent}% inc / +{card.channelingGuardMorePercent}% more, +{card.channelingAdditionalGuard} flat");
                }
            }
            ApplyChannelingMetadata(cardForProcessor, channelingState, card, channelingBonusApplied);
            
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
            
            CardAbilityRouter.ApplyCardPlay(card, cardForProcessor, playerCharacter, targetEnemy, targetPosition);

            // Apply "hits twice" effect if Aggression charge is active
            if (ShouldHitTwice() && targetEnemy != null && !cardForProcessor.isAoE)
            {
                if (logCardEffects)
                {
                    Debug.Log($"<color=yellow>[ChargeModifier] HitsTwice: Applying card effect again!</color>");
                }
                cardEffectProcessor.ApplyCardToEnemy(cardForProcessor, targetEnemy, playerCharacter, targetPosition);
            }
            
            // Boss Ability Handler - card played
            cardsPlayedThisTurn++;
            BossAbilityHandler.OnPlayerCardPlayed(cardForProcessor, cardsPlayedThisTurn);
        }
        else
        {
            Debug.LogWarning($"Cannot apply {card.cardName}: No effect processor!");
        }
        
        // Trigger event for other systems
        OnCardPlayed?.Invoke(card);
        Debug.Log($"  → Card effect triggered: {card.cardName}");
        
        // Consume charge modifiers after card is played
        ConsumeChargeModifiers();
        
        // Apply combo post-effects: mana refund, draw, ailments, buffs
        if (comboApp != null)
        {
            // Mana refund
            if (comboApp.manaRefund > 0 && playerCharacter != null)
            {
                playerCharacter.RestoreMana(comboApp.manaRefund);
                if (logCardEffects)
                {
                    Debug.Log($"<color=green>[Combo] Refunded {comboApp.manaRefund} mana</color>");
                }
                var playerDisplayRefund = FindFirstObjectByType<PlayerCombatDisplay>();
                if (playerDisplayRefund != null) playerDisplayRefund.UpdateManaDisplay();
            }
            
            // Draw cards on combo
            if (comboApp.comboDrawCards > 0)
            {
                if (logCardEffects)
                {
                    Debug.Log($"<color=green>[Combo] Draw {comboApp.comboDrawCards} card(s)</color>");
                }
                DrawCards(comboApp.comboDrawCards);
            }
            
            // Ailment (hook into status/effect manager)
            if (!string.IsNullOrEmpty(comboApp.ailmentId) && targetEnemy != null)
            {
                if (logCardEffects)
                {
                    Debug.Log($"[Combo] Apply ailment: {comboApp.ailmentId} to {targetEnemy.enemyName}");
                }
                // TODO: integrate with StatusEffectManager once ailment mapping is defined
            }
            
            // Buffs on player (list)
            if (comboApp.buffIds != null && comboApp.buffIds.Count > 0)
            {
                if (logCardEffects)
                {
                    Debug.Log($"[Combo] Apply buffs to player: {string.Join(", ", comboApp.buffIds)}");
                }
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
                            if (logCardEffects)
                            {
                                Debug.Log($"[Combo] Applied Bolster ({stacks} stack(s), 2 turns)");
                            }
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
            ApplyChannelingMetadata(cardForCombo, channelingState, card, channelingBonusApplied);
            comboSystem.OnCardPlayed(cardForCombo);
        }
        if (ComboSystem.Instance != null)
        {
            string groupKey = string.IsNullOrEmpty(card.groupKey) ? card.cardName : card.groupKey;
            ComboSystem.Instance.RegisterLastPlayed(card.GetCardTypeEnum(), card.cardName, groupKey);
            // Refresh highlights after registering last played
            UpdateComboHighlights();
        }
    }
    
    /// <summary>
    /// Animate card flying to discard pile and disappearing.
    /// PURELY VISUAL - card logic is already resolved (fire-and-forget)
    /// </summary>
    private void AnimateToDiscardPile(GameObject cardObj, CardDataExtended card)
    {
        if (cardObj == null)
        {
            Debug.LogError($"Cannot animate discard - cardObj is null for {card.cardName}!");
            // Card already added to discard pile in caller
            return;
        }
        
        if (discardPileTransform == null)
        {
            Debug.LogWarning("DiscardPileTransform not set! Using instant discard.");
            // Card already added to discard pile in caller
            
            // Remove from activeCards if still there
            if (cardRuntimeManager != null)
            {
                var activeCardsList = cardRuntimeManager.GetActiveCards();
                if (activeCardsList.Contains(cardObj))
                {
                    cardRuntimeManager.RemoveCard(cardObj);
                }
                else
                {
                    cardRuntimeManager.ReturnCardToPool(cardObj);
                }
            }
            else
            {
                Destroy(cardObj);
            }
            return;
        }
        
        Vector3 discardPosWorld = discardPileTransform.position;
        Vector3 startPos = cardObj.transform.position;
        
        if (logAnimations)
        {
            Debug.Log($"  → [Discard] Animating {card.cardName} from {startPos} to discard pile at {discardPosWorld}...");
        }
        
        // Verify this is actually the discard pile (not draw pile) - check if deckPileTransform is different
        if (deckPileTransform != null)
        {
            float distanceToDiscard = Vector3.Distance(discardPileTransform.position, startPos);
            float distanceToDraw = Vector3.Distance(deckPileTransform.position, startPos);
            if (showDebugLogs)
            {
                Debug.Log($"  → [Discard] Distance to discard pile: {distanceToDiscard}, Distance to draw pile: {distanceToDraw}");
            }
            
            if (discardPileTransform == deckPileTransform)
            {
                if (logCriticalErrors)
                {
                    Debug.LogError($"  → [Discard] ERROR: DiscardPileTransform and DeckPileTransform are the SAME! Cards will go to wrong pile!");
                }
            }
        }
        
        // Cancel any existing animations on this card
        LeanTween.cancel(cardObj);
        
        // Get RectTransform for UI elements - use it for move animation if available
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        Vector3 discardPos = discardPosWorld; // Default to world position
        
        // If using RectTransform, convert discard pile world position to local space
        if (cardRect != null && cardRect.parent != null)
        {
            RectTransform parentRect = cardRect.parent as RectTransform;
            if (parentRect != null)
            {
                // Get the canvas for coordinate conversion
                Canvas canvas = cardObj.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    Camera eventCamera = canvas.worldCamera;
                    if (eventCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    {
                        eventCamera = Camera.main;
                    }
                    
                    // Convert world position to screen point
                    Vector2 screenPoint;
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        screenPoint = new Vector2(discardPosWorld.x, discardPosWorld.y);
                    }
                    else
                    {
                        screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, discardPosWorld);
                    }
                    
                    // Convert screen point to local position relative to card's parent
                    Vector2 localPoint;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentRect, 
                        screenPoint,
                        eventCamera,
                        out localPoint))
                    {
                        discardPos = localPoint;
                        if (showDebugLogs)
                        {
                            Debug.Log($"  → [Discard] Converted discard position to local space: {discardPos}");
                        }
                    }
                    else
                    {
                        if (logCriticalErrors)
                        {
                            Debug.LogWarning($"  → [Discard] Failed to convert discard position to local space, using world position");
                        }
                    }
                }
            }
        }
        
        // Use a counter to ensure all animations complete
        int animationsCompleted = 0;
        int totalAnimations = 3; // move, scale, rotate
        
        System.Action OnAnimationComplete = () => {
            animationsCompleted++;
            if (logAnimations)
            {
                Debug.Log($"  → [Discard] Animation step {animationsCompleted}/{totalAnimations} complete for {card.cardName}");
            }
            
            // Only trigger cleanup when ALL animations complete
            if (animationsCompleted >= totalAnimations)
            {
                if (logAnimations)
                {
                    Debug.Log($"  → [Discard] <color=green>All 3 animations complete for {card.cardName}, returning to pool...</color>");
                }
                
                // Safety check
                if (cardObj == null)
                {
                    if (logCriticalErrors)
                    {
                        Debug.LogError($"  → [Discard] Card GameObject became null during discard animation!");
                    }
                    // Card already added to discard pile in caller
                    return;
                }
                
                if (showDebugLogs)
                {
                    Debug.Log($"  → [Discard] Card position before pool return: {cardObj.transform.position}");
                    Debug.Log($"  → [Discard] Card scale before pool return: {cardObj.transform.localScale}");
                    Debug.Log($"  → [Discard] Card active: {cardObj.activeInHierarchy}");
                }
                
                // Card already added to discard pile in caller, just return to pool
                
                // Remove from activeCards and return to pool
                if (cardRuntimeManager != null)
                {
                    var activeCardsList = cardRuntimeManager.GetActiveCards();
                    if (activeCardsList.Contains(cardObj))
                    {
                        cardRuntimeManager.RemoveCard(cardObj);
                    }
                    else
                    {
                        cardRuntimeManager.ReturnCardToPool(cardObj);
                    }
                    if (logAnimations)
                    {
                        Debug.Log($"  → [Discard] <color=green>✓✓✓ {card.cardName} RETURNED TO POOL! ✓✓✓</color>");
                    }
                }
                else
                {
                    Destroy(cardObj);
                    Debug.Log($"  → [Discard] {card.cardName} destroyed (no pool manager)");
                }
                
                // Clean up tracking - card is done being played
                if (cardsBeingPlayed.Contains(cardObj))
                {
                    cardsBeingPlayed.Remove(cardObj);
                    Debug.Log($"  → [CardPlay] Removed {card.cardName} from cardsBeingPlayed set");
                }
                if (cardPlayStates.ContainsKey(cardObj))
                {
                    cardPlayStates.Remove(cardObj);
                }
            }
        };
        
        // Animate to discard pile (position) - use RectTransform if available for UI elements
        if (cardRect != null)
        {
            if (showDebugLogs)
            {
                Debug.Log($"  → [Discard] Using RectTransform.move for {card.cardName} to position {discardPos}");
            }
            LeanTween.move(cardRect, discardPos, 0.4f)
                .setEase(LeanTweenType.easeInQuad)
                .setOnComplete(OnAnimationComplete);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log($"  → [Discard] Using Transform.move for {card.cardName} to position {discardPos}");
            }
            LeanTween.move(cardObj, discardPos, 0.4f)
                .setEase(LeanTweenType.easeInQuad)
                .setOnComplete(OnAnimationComplete);
        }
        
        // Shrink while moving
        LeanTween.scale(cardObj, Vector3.one * 0.2f, 0.4f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(OnAnimationComplete);
        
        // Rotate while moving
        LeanTween.rotate(cardObj, new Vector3(0, 0, Random.Range(-30f, 30f)), 0.4f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(OnAnimationComplete);
        
        // Safety timeout for discard animation (visual cleanup only - logic already resolved)
        StartCoroutine(SafetyTimeoutForDiscardAnimation(cardObj, card, 1.5f));
    }
    
    /// <summary>
    /// Safety timeout coroutine for discard animation - forces cleanup if animation gets stuck
    /// </summary>
    private IEnumerator SafetyTimeoutForDiscardAnimation(GameObject cardObj, CardDataExtended card, float timeout)
    {
        yield return new WaitForSeconds(timeout);
        
        if (cardObj != null && cardObj.activeInHierarchy)
        {
            if (logCriticalErrors)
            {
                Debug.LogWarning($"  → [Discard] [Safety Timeout] Discard animation stuck for {card.cardName}! Force completing...");
            }
            LeanTween.cancel(cardObj);
            
            // Force return to pool
            if (cardRuntimeManager != null)
            {
                var activeCardsList = cardRuntimeManager.GetActiveCards();
                if (activeCardsList.Contains(cardObj))
                {
                    cardRuntimeManager.RemoveCard(cardObj);
                }
                else
                {
                    cardRuntimeManager.ReturnCardToPool(cardObj);
                }
                if (logAnimations)
                {
                    Debug.Log($"  → [Discard] [Safety Timeout] Returned {card.cardName} to pool.");
                }
            }
            else
            {
                Destroy(cardObj);
                Debug.Log($"  → [Discard] [Safety Timeout] Destroyed {card.cardName} (no pool manager)");
            }
        }
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
    
    /// <summary>
    /// Exhaust a card from hand (permanently remove it from combat)
    /// </summary>
    public void ExhaustCardFromHand(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning($"[ExhaustCard] Invalid hand index: {handIndex}");
            return;
        }
        
        CardDataExtended exhaustedCard = hand[handIndex];
        LogDeckState($"Before Exhaust: {exhaustedCard.cardName}");
        
        hand.RemoveAt(handIndex);
        
        // Remove visual
        if (handIndex < handVisuals.Count)
        {
            GameObject cardObj = handVisuals[handIndex];
            handVisuals.RemoveAt(handIndex);
            
            if (cardRuntimeManager != null && cardObj != null)
            {
                cardRuntimeManager.ReturnCardToPool(cardObj);
            }
            
            // Reposition remaining cards
            if (cardRuntimeManager != null)
            {
                cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
            }
        }
        
        Debug.Log($"<color=orange>[ExhaustCard] Exhausted {exhaustedCard.cardName} from hand</color>");
        LogDeckState($"After Exhaust: {exhaustedCard.cardName}");
        
        // Note: Exhausted cards are intentionally removed - validation will show mismatch, which is expected
        // We track exhausts separately rather than modifying initialDeckSize to maintain integrity checks
        ValidateCardCount($"After Exhaust: {exhaustedCard.cardName} (Expected -1 due to exhaust)");
    }
    
    /// <summary>
    /// Exhaust a card from discard pile (used by ExhaustCardEffect)
    /// This is called externally, so we track it for debugging
    /// </summary>
    public void ExhaustCardFromDiscardPile(int discardIndex)
    {
        if (discardIndex < 0 || discardIndex >= discardPile.Count)
        {
            Debug.LogWarning($"[ExhaustCard] Invalid discard index: {discardIndex}");
            return;
        }
        
        CardDataExtended exhaustedCard = discardPile[discardIndex];
        LogDeckState($"Before Exhaust from Discard: {exhaustedCard.cardName}");
        
        discardPile.RemoveAt(discardIndex);
        
        Debug.Log($"<color=orange>[ExhaustCard] Exhausted {exhaustedCard.cardName} from discard pile</color>");
        LogDeckState($"After Exhaust from Discard: {exhaustedCard.cardName}");
        ValidateCardCount($"After Exhaust from Discard: {exhaustedCard.cardName} (Expected -1 due to exhaust)");
    }
    
    /// <summary>
    /// Reset deck state tracking (call when combat ends)
    /// </summary>
    public void ResetDeckStateTracking()
    {
        deckInitialized = false;
        initialDeckSize = 0;
        Debug.Log("[CombatDeckManager] Deck state tracking reset");
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
            if (handVisuals[i] == null) continue;
            
            GameObject cardObj = handVisuals[i];
            
            // Check if we have card data for this visual
            if (i < hand.Count && hand[i] != null)
            {
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
                
                // CRITICAL: Ensure button interactability is set based on mana affordability
                // This prevents buttons from being disabled incorrectly
                UnityEngine.UI.Button button = cardObj.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    bool canAfford = player.mana >= card.playCost;
                    button.interactable = canAfford;
                    
                    // Debug logging for high-index cards
                    int siblingIndex = cardObj.transform.GetSiblingIndex();
                    if (siblingIndex >= 12)
                    {
                        Debug.Log($"<color=orange>[UpdateCardUsability {siblingIndex}] {cardObj.name} - Mana: {player.mana}/{card.playCost}, CanAfford: {canAfford}, Button.interactable: {button.interactable}</color>");
                    }
                }
            }
            else
            {
                // Card visual exists but no card data - ensure it's still interactable
                // This handles cases where handVisuals.Count > hand.Count
                UnityEngine.UI.Button button = cardObj.GetComponent<UnityEngine.UI.Button>();
                if (button != null && !button.interactable)
                {
                    int siblingIndex = cardObj.transform.GetSiblingIndex();
                    Debug.LogWarning($"<color=red>[UpdateCardUsability] Card at index {i} (sibling {siblingIndex}) has no card data but button is disabled! Force-enabling...</color>");
                    button.interactable = true;
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
    /// Process a discard card effect after discard selection is complete
    /// </summary>
    private void ProcessDiscardCardEffect(CardDataExtended card, CardDataExtended discardedCard, int handIndex, GameObject cardObj, Vector3 targetPosition, Character player)
    {
        if (card == null)
        {
            Debug.LogError("[Discard Card] Card is null!");
            return;
        }
        
        Debug.Log($"<color=orange>[Discard Card] Processing {card.cardName} after discarding {(discardedCard != null ? discardedCard.cardName : "nothing")}</color>");
        
        // Remove the discard card from hand now
        if (handIndex >= 0 && handIndex < hand.Count && hand[handIndex] == card)
        {
            hand.RemoveAt(handIndex);
        }
        if (handIndex >= 0 && handIndex < handVisuals.Count && handVisuals[handIndex] == cardObj)
        {
            handVisuals.RemoveAt(handIndex);
        }
        
        // Process the discarded card's ifDiscardedEffect if it exists
        if (discardedCard != null && !string.IsNullOrEmpty(discardedCard.ifDiscardedEffect))
        {
            ProcessIfDiscardedEffect(discardedCard, player);
        }
        
        // Calculate discard power for effects that use it
        float discardPower = discardedCard != null 
            ? DiscardPowerCalculator.CalculateDiscardPower(discardedCard, player) 
            : 0f;
        
        // Process the discard card's main effect
        ProcessDiscardCardMainEffect(card, discardedCard, discardPower, targetPosition, player);
        
        // Continue with normal card play animation and effects
        // (Skip the discard check since we already handled it)
        ContinueCardPlayAfterDiscard(card, cardObj, targetPosition, player);
    }
    
    /// <summary>
    /// Continue normal card play flow after discard selection is complete
    /// </summary>
    private void ContinueCardPlayAfterDiscard(CardDataExtended card, GameObject cardObj, Vector3 targetPosition, Character player)
    {
        // Reposition remaining cards
        if (cardRuntimeManager != null)
        {
            cardRuntimeManager.RepositionCards(handVisuals, animated: true, duration: 0.3f);
        }
        
        // Get target enemy and build combo
        Enemy targetEnemy = GetTargetEnemy();
        Character playerCharacter = characterManager != null && characterManager.HasCharacter() ?
            characterManager.GetCurrentCharacter() : null;
        
        ComboSystem.ComboApplication comboApp = null;
        if (ComboSystem.Instance != null)
        {
            comboApp = ComboSystem.Instance.BuildComboApplication(card, playerCharacter);
        }
        
        // Use action queue system (same as normal PlayCard flow)
        int handIndex = -1; // Not in hand anymore, but action needs a value
        CardAction action = new CardAction(card, cardObj, handIndex, targetPosition, targetEnemy, playerCharacter, comboApp);
        actionQueue.Enqueue(action);
        
        // Track this card as being played
        if (!cardsBeingPlayed.Contains(cardObj))
        {
            cardsBeingPlayed.Add(cardObj);
            cardPlayStates[cardObj] = new CardPlayState
            {
                card = card,
                cardObj = cardObj,
                effectsApplied = false,
                playStartTime = Time.time,
                state = CardState.Queued
            };
        }
        
        // Process action queue immediately (resolve logic)
        ProcessActionQueue();
        
        Debug.Log($"<color=green>[ActionQueue] {card.cardName} (discard card) logic resolved. Starting animations...</color>");
        
        // Cancel any existing animations and prepare for play animation
        LeanTween.cancel(cardObj);
        cardObj.transform.localScale = Vector3.one;
        cardObj.transform.localRotation = Quaternion.identity;
        
        CardHoverEffect hoverEffect = cardObj.GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
        {
            hoverEffect.enabled = false;
        }
        SetCardInteractable(cardObj, false);
        
        // Start play animation (fire-and-forget - effects already applied!)
        if (animationManager != null && cardRuntimeManager != null)
        {
            animationManager.AnimateCardPlay(cardObj, targetPosition, () => {
                if (logAnimations)
                {
                    Debug.Log($"  <color=cyan>[Animation] Card play animation complete for {card.cardName} (discard card) - effects already applied</color>");
                }
                
                // Card already in discard pile and effects applied, just invoke event
                OnCardDiscarded?.Invoke(card);
                
                // Animate to discard pile (visual only)
                float effectDuration = 0.3f;
                LeanTween.delayedCall(gameObject, effectDuration, () => {
                    if (cardObj != null && discardPileTransform != null)
                    {
                        AnimateToDiscardPile(cardObj, card);
                    }
                    else if (cardObj != null)
                    {
                        CleanupCardAfterPlay(cardObj, card);
                    }
                });
            });
        }
        else
        {
            // No animation - just clean up
            CleanupCardAfterPlay(cardObj, card);
        }
    }
    
    /// <summary>
    /// Process the main effect of a discard card (e.g., "draw 3 cards", "Gain 6 Guard")
    /// </summary>
    private void ProcessDiscardCardMainEffect(CardDataExtended card, CardDataExtended discardedCard, float discardPower, Vector3 targetPosition, Character player)
    {
        if (card == null || player == null) return;
        
        string description = card.description ?? "";
        
        // Forbidden Prayer: "Discard 1 card: draw 3 cards."
        if (card.cardName.Contains("Forbidden Prayer") || (description.Contains("draw") && description.Contains("card")))
        {
            var drawMatch = System.Text.RegularExpressions.Regex.Match(description, @"draw\s+(\d+)\s+card", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (drawMatch.Success)
            {
                int drawCount = int.Parse(drawMatch.Groups[1].Value);
                DrawCards(drawCount);
                Debug.Log($"<color=green>[Discard Card] {card.cardName} drew {drawCount} cards</color>");
            }
        }
        
        // Scripture Burn: "Discard 1 card. Gain 6 Guard (+Int/2 + Discard Power/2) and 3 temporary Intelligence."
        if (card.cardName.Contains("Scripture Burn"))
        {
            ProcessScriptureBurnEffect(card, player, discardPower);
        }
    }
    
    /// <summary>
    /// Process ifDiscardedEffect when a card is discarded
    /// </summary>
    private void ProcessIfDiscardedEffect(CardDataExtended discardedCard, Character player)
    {
        if (discardedCard == null || player == null || string.IsNullOrEmpty(discardedCard.ifDiscardedEffect)) return;
        
        Debug.Log($"<color=cyan>[If Discarded] Processing effect for {discardedCard.cardName}: {discardedCard.ifDiscardedEffect}</color>");
        
        // Calculate discard power
        float discardPower = DiscardPowerCalculator.CalculateDiscardPower(discardedCard, player);
        
        // Process the ifDiscardedEffect text
        ProcessIfDiscardedEffectText(discardedCard.ifDiscardedEffect, discardedCard, player, discardPower);
    }
    
    /// <summary>
    /// Process ifDiscardedEffect text and apply effects
    /// </summary>
    private void ProcessIfDiscardedEffectText(string effectText, CardDataExtended card, Character player, float discardPower)
    {
        if (string.IsNullOrEmpty(effectText)) return;
        
        // Replace {discardPower} placeholder
        effectText = effectText.Replace("{discardPower}", discardPower.ToString("F0"));
        
        // Parse and apply effects
        // Pattern: "Deal {discardPower} chaos damage to all enemies."
        if (System.Text.RegularExpressions.Regex.IsMatch(effectText, @"Deal.*damage.*all enemies", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            ProcessIfDiscardedDamage(effectText, card, player, discardPower);
        }
        // Pattern: "Gain {discardPower} Guard."
        else if (System.Text.RegularExpressions.Regex.IsMatch(effectText, @"Gain.*Guard", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            ProcessIfDiscardedGuard(effectText, card, player, discardPower);
        }
        // Pattern: "Gain {discardPower} Temporary Intelligence."
        else if (System.Text.RegularExpressions.Regex.IsMatch(effectText, @"Gain.*Temporary Intelligence", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            ProcessIfDiscardedTempIntelligence(effectText, card, player, discardPower);
        }
        // Pattern: "Increase spell power by {intelligence/8}."
        else if (System.Text.RegularExpressions.Regex.IsMatch(effectText, @"Increase.*spell power", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            ProcessIfDiscardedSpellPower(effectText, card, player);
        }
        // Pattern: "Gain X Guard (+Int/Y + Discard Power/Z) and draw 1 card."
        else if (System.Text.RegularExpressions.Regex.IsMatch(effectText, @"Gain.*Guard.*draw", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            ProcessIfDiscardedGuardAndDraw(effectText, card, player, discardPower);
        }
    }
    
    private void ProcessIfDiscardedDamage(string effectText, CardDataExtended card, Character player, float discardPower)
    {
        // Parse damage type (chaos, physical, etc.)
        bool isChaos = effectText.ToLower().Contains("chaos");
        DamageType damageType = isChaos ? DamageType.Chaos : DamageType.Physical;
        
        // Deal damage to all enemies
        var combatDisplay = FindFirstObjectByType<CombatDisplayManager>();
        if (combatDisplay != null)
        {
            var enemySpawner = combatDisplay.enemySpawner;
            if (enemySpawner != null)
            {
                var activeEnemies = enemySpawner.GetActiveEnemies();
                if (activeEnemies != null)
                {
                    for (int i = 0; i < activeEnemies.Count; i++)
                    {
                        combatDisplay.PlayerAttackEnemy(i, discardPower);
                    }
                    Debug.Log($"<color=orange>[If Discarded] {card.cardName} dealt {discardPower:F0} {damageType} damage to all enemies</color>");
                }
            }
        }
    }
    
    private void ProcessIfDiscardedGuard(string effectText, CardDataExtended card, Character player, float discardPower)
    {
        player.AddGuard(discardPower);
        Debug.Log($"<color=cyan>[If Discarded] {card.cardName} gained {discardPower:F0} guard</color>");
        
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null) playerDisplay.UpdateGuardDisplay();
    }
    
    private void ProcessIfDiscardedTempIntelligence(string effectText, CardDataExtended card, Character player, float discardPower)
    {
        int tempInt = Mathf.RoundToInt(discardPower);
        // Get StatusEffectManager from PlayerCombatDisplay
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            var statusMgr = playerDisplay.GetStatusEffectManager();
            if (statusMgr != null)
            {
                var intEffect = new StatusEffect(StatusEffectType.Intelligence, "TempIntelligence", tempInt, 3, false);
                statusMgr.AddStatusEffect(intEffect);
                Debug.Log($"<color=cyan>[If Discarded] {card.cardName} gained {tempInt} temporary Intelligence</color>");
            }
        }
    }
    
    private void ProcessIfDiscardedSpellPower(string effectText, CardDataExtended card, Character player)
    {
        // Parse intelligence divisor (e.g., {intelligence/8})
        var match = System.Text.RegularExpressions.Regex.Match(effectText, @"intelligence\s*/\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            float divisor = float.Parse(match.Groups[1].Value);
            float spellPowerIncrease = player.intelligence / divisor;
            
            // Apply spell power as a status effect (each point = 1% increased spell damage)
            var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                var statusMgr = playerDisplay.GetStatusEffectManager();
                if (statusMgr != null)
                {
                    // Spell power lasts for the duration of combat (999 turns = effectively entire combat)
                    // Stacks additively with other spell power sources
                    var spellPowerEffect = new StatusEffect(StatusEffectType.SpellPower, "Spell Power", spellPowerIncrease, 999, false);
                    statusMgr.AddStatusEffect(spellPowerEffect);
                    Debug.Log($"<color=cyan>[If Discarded] {card.cardName} increased spell power by {spellPowerIncrease:F1} (Int/{divisor}) for the duration of combat</color>");
                }
            }
        }
    }
    
    private void ProcessIfDiscardedGuardAndDraw(string effectText, CardDataExtended card, Character player, float discardPower)
    {
        // Parse guard amount
        var guardMatch = System.Text.RegularExpressions.Regex.Match(effectText, @"Gain\s+(\d+)\s+Guard", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (guardMatch.Success)
        {
            float baseGuard = float.Parse(guardMatch.Groups[1].Value);
            
            // Parse Int divisor
            float intDivisor = 0f;
            var intMatch = System.Text.RegularExpressions.Regex.Match(effectText, @"Int\s*/\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (intMatch.Success)
            {
                intDivisor = float.Parse(intMatch.Groups[1].Value);
            }
            
            // Parse Discard Power divisor
            float discardDivisor = 0f;
            var discardMatch = System.Text.RegularExpressions.Regex.Match(effectText, @"Discard Power\s*/\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (discardMatch.Success)
            {
                discardDivisor = float.Parse(discardMatch.Groups[1].Value);
            }
            
            float totalGuard = baseGuard;
            if (intDivisor > 0) totalGuard += player.intelligence / intDivisor;
            if (discardDivisor > 0) totalGuard += discardPower / discardDivisor;
            
            player.AddGuard(totalGuard);
            Debug.Log($"<color=cyan>[If Discarded] {card.cardName} gained {totalGuard:F1} guard (base: {baseGuard}, Int/{intDivisor}, Discard/{discardDivisor})</color>");
            
            var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null) playerDisplay.UpdateGuardDisplay();
        }
        
        // Draw 1 card
        DrawCards(1);
        Debug.Log($"<color=green>[If Discarded] {card.cardName} drew 1 card</color>");
    }
    
    private void ProcessScriptureBurnEffect(CardDataExtended card, Character player, float discardPower)
    {
        if (card == null || player == null) return;
        
        // Parse: "Gain 6 Guard (+Int/2 + Discard Power/2) and 3 temporary Intelligence."
        float baseGuard = 6f;
        float intDivisor = 2f;
        float discardDivisor = 2f;
        int tempInt = 3;
        
        float totalGuard = baseGuard + (player.intelligence / intDivisor) + (discardPower / discardDivisor);
        player.AddGuard(totalGuard);
        
        Debug.Log($"<color=cyan>[Discard Card] Scripture Burn gained {totalGuard:F1} guard (base: {baseGuard}, Int/{intDivisor}, Discard/{discardDivisor})</color>");
        
        var playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null) playerDisplay.UpdateGuardDisplay();
        
        // Apply temporary Intelligence
        if (playerDisplay != null)
        {
            var statusMgr = playerDisplay.GetStatusEffectManager();
            if (statusMgr != null)
            {
                var intEffect = new StatusEffect(StatusEffectType.Intelligence, "TempIntelligence", tempInt, 3, false);
                statusMgr.AddStatusEffect(intEffect);
                Debug.Log($"<color=cyan>[Discard Card] Scripture Burn gained {tempInt} temporary Intelligence</color>");
            }
        }
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
                    // Skip impact effect for projectile cards - they play impact when projectile hits
                    // Projectile cards are handled by CardEffectProcessor.PlayCardEffect which plays
                    // the projectile and triggers impact when it arrives
                    bool isProjectile = IsProjectileCard(card);
                    if (!isProjectile)
                    {
                    // Check for critical hit (10% chance)
                    bool isCritical = Random.Range(0f, 1f) < 0.1f;
                    
                    // Get damage type from card effects
                    DamageType damageType = GetCardDamageType(card);
                    
                        // Play elemental damage effect (only for non-projectile cards)
                    combatEffectManager.PlayElementalDamageEffectOnTarget(targetDisplay.transform, damageType, isCritical);
                    }
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
    /// Determine if a card should use projectile effects (works with CardDataExtended)
    /// </summary>
    private bool IsProjectileCard(CardDataExtended card)
    {
        if (card == null) return false;
        
        // Check scalesWithProjectileWeapon
        if (card.scalesWithProjectileWeapon)
        {
            return true;
        }
        
        // Check tags for projectile/ranged indicators
        if (card.tags != null)
        {
            foreach (string tag in card.tags)
            {
                if (tag != null && (
                    tag.Equals("Projectile", System.StringComparison.OrdinalIgnoreCase) ||
                    tag.Equals("Ranged", System.StringComparison.OrdinalIgnoreCase) ||
                    tag.Equals("Bow", System.StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
        }
        
        // Check card name for common projectile cards
        string cardName = card.cardName ?? "";
        if (cardName.Contains("Fireball", System.StringComparison.OrdinalIgnoreCase) ||
            cardName.Contains("Arrow", System.StringComparison.OrdinalIgnoreCase) ||
            cardName.Contains("Bolt", System.StringComparison.OrdinalIgnoreCase) ||
            cardName.Contains("Shot", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        return false;
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

    [System.Serializable]
    public struct SpeedMeterState
    {
        public float aggressionProgress;
        public int aggressionCharges;
        public float focusProgress;
        public int focusCharges;

        public bool IsAggressionReady => aggressionCharges > 0;
        public bool IsFocusReady => focusCharges > 0;
    }
}

