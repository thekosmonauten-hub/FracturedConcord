using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Modern Combat UI system designed to work seamlessly with CombatAnimationManager.
/// Handles card display, health/mana bars, enemy panels, and turn flow with animations.
/// Uses Old UI System (UnityEngine.UI) for consistency and simplicity.
/// </summary>
public class AnimatedCombatUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDisplayManager combatDisplayManager;
    [SerializeField] private CombatAnimationManager animationManager;
    
    // Support for legacy CombatManager (if it exists)
    [SerializeField] private CombatManager legacyCombatManager;
    
    [Header("Player UI")]
    [SerializeField] private Image playerHealthBarFill;
    [SerializeField] private Text playerHealthText;
    [SerializeField] private Image playerManaBarFill;
    [SerializeField] private Text playerManaText;
    [SerializeField] private Text playerNameText;
    
    [Header("Enemy UI")]
    [SerializeField] private EnemyPanel[] enemyPanels = new EnemyPanel[3];
    
    [Header("Card Hand UI")]
    [SerializeField] private Transform cardHandParent;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private float cardSpacing = 140f;
    [SerializeField] private float cardYPosition = -300f;
    [SerializeField] private Vector3 cardScale = Vector3.one;
    
    [Header("Deck/Discard UI")]
    [SerializeField] private TextMeshProUGUI deckCountText;
    [SerializeField] private TextMeshProUGUI discardCountText;
    [SerializeField] private Transform deckPosition;
    [SerializeField] private Transform discardPosition;
    
    [Header("Loot Log")]
    [SerializeField] private Transform lootLogContainer;
    [SerializeField] private int maxLootEntries = 20;
    
    [Header("Turn UI")]
    [SerializeField] private GameObject turnIndicator;
    [SerializeField] private Text turnIndicatorText;
    [SerializeField] private Button endTurnButton;
    
    [Header("Wave UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    
    /// <summary>
    /// Flash the End Turn button to indicate player can't afford any cards
    /// </summary>
    public void FlashEndTurnButton()
    {
        if (endTurnButton != null)
        {
            // Get the button's image component
            var buttonImage = endTurnButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Store original color
                Color originalColor = buttonImage.color;
                
                // Flash red twice using LeanTween
                LeanTween.sequence()
                    .append(LeanTween.color(endTurnButton.gameObject, Color.red, 0.2f).setEase(LeanTweenType.easeOutQuad))
                    .append(LeanTween.color(endTurnButton.gameObject, originalColor, 0.2f).setEase(LeanTweenType.easeOutQuad))
                    .append(LeanTween.color(endTurnButton.gameObject, Color.red, 0.2f).setEase(LeanTweenType.easeOutQuad))
                    .append(LeanTween.color(endTurnButton.gameObject, originalColor, 0.2f).setEase(LeanTweenType.easeOutQuad));
            }
        }
        
        Debug.Log("<color=orange>End Turn button flashed - player cannot afford any cards!</color>");
    }
    
    [Header("Combat Log")]
    [SerializeField] private Text combatLogText;
    
    // Card management
    private List<CardInstance> activeCards = new List<CardInstance>();
    private Queue<GameObject> cardPool = new Queue<GameObject>();
    
    // State
    private float lastPlayerHealth;
    private float lastPlayerMana;
    
    #region Initialization
    
    private void Awake()
    {
        // Auto-find references if not set
        if (combatDisplayManager == null)
            combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        
        if (legacyCombatManager == null)
            legacyCombatManager = FindFirstObjectByType<CombatManager>();
        
        if (animationManager == null)
            animationManager = CombatAnimationManager.Instance;
        
        InitializeCardPool();
    }
    
    private void Start()
    {
        SetupEventListeners();
        InitializeUI();
        UpdateAllUI();
    }
    
    private void InitializeCardPool()
    {
        if (cardPrefab == null)
        {
            Debug.LogWarning("Card prefab not assigned! Card animations will not work.");
            return;
        }
        
        // Pre-instantiate card pool
        int poolSize = 10;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject card = Instantiate(cardPrefab, cardHandParent);
            card.SetActive(false);
            cardPool.Enqueue(card);
        }
        
        Debug.Log($"Card pool initialized with {poolSize} cards");
    }
    
    private void SetupEventListeners()
    {
        // Subscribe to CombatDisplayManager events if available
        if (combatDisplayManager != null)
        {
            combatDisplayManager.OnCombatStateChanged += OnCombatStateChanged;
            combatDisplayManager.OnTurnChanged += OnTurnChanged;
            combatDisplayManager.OnTurnTypeChanged += OnTurnTypeChanged;
            combatDisplayManager.OnEnemyDefeated += OnEnemyDefeated;
            combatDisplayManager.OnCombatEnded += OnCombatEnded;
            combatDisplayManager.OnWaveChanged += OnWaveChanged;
        }
        // Fallback to legacy CombatManager if available
        else if (legacyCombatManager != null)
        {
            legacyCombatManager.OnCardPlayed += OnCardPlayed;
            legacyCombatManager.OnTurnEnded += OnTurnEnded;
            legacyCombatManager.OnCombatEnded += OnCombatEnded;
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }
        
        // Setup enemy panel click listeners
        for (int i = 0; i < enemyPanels.Length; i++)
        {
            int index = i; // Capture for closure
            if (enemyPanels[i] != null && enemyPanels[i].clickArea != null)
            {
                enemyPanels[i].clickArea.onClick.AddListener(() => SelectEnemy(index));
            }
        }
    }
    
    private void InitializeUI()
    {
        // Store initial values from CharacterManager
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager != null && charManager.HasCharacter())
        {
            Character player = charManager.GetCurrentCharacter();
            lastPlayerHealth = player.currentHealth;
            lastPlayerMana = player.mana;
        }
        
        // Hide all enemy panels initially
        foreach (var panel in enemyPanels)
        {
            if (panel != null && panel.panelObject != null)
            {
                panel.panelObject.SetActive(false);
            }
        }
    }
    
    #endregion
    
    #region Update Methods
    
    private void Update()
    {
        // Monitor for changes and trigger animations
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter()) return;
        
        Character player = charManager.GetCurrentCharacter();
        
        // Check for health changes
        if (player.currentHealth != lastPlayerHealth)
        {
            AnimatePlayerHealth();
            lastPlayerHealth = player.currentHealth;
        }
        
        // Check for mana changes
        if (player.mana != lastPlayerMana)
        {
            AnimatePlayerMana();
            lastPlayerMana = player.mana;
        }
    }
    
    public void UpdateAllUI()
    {
        UpdatePlayerUI();
        UpdateEnemyUI();
        UpdateCardHandUI();
        UpdateDeckUI();
        UpdateTurnUI();
        UpdateWaveUI();
    }
    
    private void UpdatePlayerUI()
    {
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter()) return;
        
        Character player = charManager.GetCurrentCharacter();
        
        // Update name
        if (playerNameText != null)
            playerNameText.text = player.characterName;
        
        // Update health (animated)
        AnimatePlayerHealth();
        
        // Update mana (animated)
        AnimatePlayerMana();
    }
    
    private void UpdateEnemyUI()
    {
        List<Enemy> enemies = GetActiveEnemies();
        
        for (int i = 0; i < enemyPanels.Length; i++)
        {
            if (enemyPanels[i] == null) continue;
            
            if (i < enemies.Count)
            {
                // Show and update enemy panel
                Enemy enemy = enemies[i];
                UpdateEnemyPanel(i, enemy);
            }
            else
            {
                // Hide unused panel
                if (enemyPanels[i].panelObject != null)
                    enemyPanels[i].panelObject.SetActive(false);
            }
        }
    }
    
    private List<Enemy> GetActiveEnemies()
    {
        if (combatDisplayManager != null)
        {
            return combatDisplayManager.GetActiveEnemies();
        }
        else if (legacyCombatManager != null)
        {
            return legacyCombatManager.enemies;
        }
        
        return new List<Enemy>();
    }
    
    private void UpdateEnemyPanel(int index, Enemy enemy)
    {
        EnemyPanel panel = enemyPanels[index];
        if (panel == null) return;
        
        // Show panel
        if (panel.panelObject != null)
            panel.panelObject.SetActive(true);
        
        // Update name
        if (panel.nameText != null)
            panel.nameText.text = enemy.enemyName;
        
        // Update health text
        if (panel.healthText != null)
            panel.healthText.text = $"{enemy.currentHealth}/{enemy.maxHealth}";
        
        // Update intent
        if (panel.intentText != null)
            panel.intentText.text = $"Intent: {enemy.GetIntentDescription()}";
        
        // Animate health bar
        if (panel.healthBarFill != null && animationManager != null)
        {
            animationManager.AnimateHealthBar(
                panel.healthBarFill,
                enemy.currentHealth,
                enemy.maxHealth
            );
        }
        
        // Update selection indicator (if using selection system)
        bool isSelected = false; // TODO: Implement selection tracking
        UpdateSelectionIndicator(panel, isSelected);
    }
    
    private void UpdateSelectionIndicator(EnemyPanel panel, bool isSelected)
    {
        if (panel.selectionIndicator != null)
        {
            panel.selectionIndicator.SetActive(isSelected);
        }
        
        // Also update border color if available
        if (panel.border != null)
        {
            panel.border.color = isSelected ? Color.yellow : Color.white;
        }
    }
    
    private void UpdateCardHandUI()
    {
        // Get current hand from combat manager - NOW USING CardDataExtended!
        List<CardDataExtended> hand = GetCurrentHand();
        if (hand == null) return;
        
        // Remove excess cards
        while (activeCards.Count > hand.Count)
        {
            RemoveCardAt(activeCards.Count - 1);
        }
        
        // Add missing cards
        while (activeCards.Count < hand.Count)
        {
            int index = activeCards.Count;
            AddCardToHand(hand[index], index);
        }
        
        // Update card positions
        RepositionCards();
    }
    
    private List<CardDataExtended> GetCurrentHand()
    {
        // Try CombatDeckManager first (current system) - NOW RETURNS CardDataExtended!
        CombatDeckManager deckManager = CombatDeckManager.Instance;
        if (deckManager != null)
        {
            return deckManager.GetHand();
        }
        
        // Fallback to legacy CombatManager (convert if needed)
        if (legacyCombatManager != null)
        {
            // Legacy returns List<Card>, need to convert
            Debug.LogWarning("Using legacy CombatManager - card features may be limited");
            return new List<CardDataExtended>(); // Empty list for now (legacy not supported)
        }
        
        // No card manager found
        Debug.LogWarning("No card manager found! Cards will not be displayed. Make sure CombatDeckManager is in the scene.");
        return new List<CardDataExtended>();
    }
    
    private void UpdateDeckUI()
    {
        // Try CombatDeckManager first (current system)
        CombatDeckManager deckManager = CombatDeckManager.Instance;
        if (deckManager != null)
        {
            if (deckCountText != null)
                deckCountText.text = deckManager.GetDrawPileCount().ToString();
            
            if (discardCountText != null)
                discardCountText.text = deckManager.GetDiscardPileCount().ToString();
        }
        // Fallback to legacy CombatManager
        else if (legacyCombatManager != null)
        {
            if (deckCountText != null)
                deckCountText.text = legacyCombatManager.GetDrawPileCount().ToString();
            
            if (discardCountText != null)
                discardCountText.text = legacyCombatManager.GetDiscardPileCount().ToString();
        }
        else
        {
            // Default values when no combat manager
            if (deckCountText != null)
                deckCountText.text = "0";
            
            if (discardCountText != null)
                discardCountText.text = "0";
        }
    }
    
    private void UpdateTurnUI()
    {
        bool isPlayerTurn = GetIsPlayerTurn();
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = isPlayerTurn ? "YOUR TURN" : "ENEMY TURN";
        }
        if (endTurnButton != null)
        {
            endTurnButton.interactable = isPlayerTurn;
        }
    }

    private void UpdateWaveUI()
    {
        if (waveText == null) return;
        if (combatDisplayManager == null)
        {
            waveText.text = "";
            return;
        }
        int cur = Mathf.Max(0, combatDisplayManager.currentWave);
        int total = Mathf.Max(1, combatDisplayManager.totalWaves);
        waveText.text = cur > 0 ? $"Wave {cur}/{total}" : "";
    }

    private void OnWaveChanged(int cur, int total)
    {
        UpdateWaveUI();
        if (waveText != null)
        {
            // Cancel any existing animations on this object
            LeanTween.cancel(waveText.gameObject);
            
            // Reset to normal scale first
            waveText.rectTransform.localScale = Vector3.one;
            
            // Enhanced pulse animation: Scale up to 1.3x, then back down to 1.0x
            LeanTween.scale(waveText.rectTransform, Vector3.one * 1.3f, 0.3f)
                     .setEase(LeanTweenType.easeOutBack)
                     .setOnComplete(() => 
                     {
                         LeanTween.scale(waveText.rectTransform, Vector3.one, 0.25f)
                                  .setEase(LeanTweenType.easeInOutQuad);
                     });
            
            // Optional: Add a subtle color flash
            if (waveText is TextMeshProUGUI tmp)
            {
                Color originalColor = tmp.color;
                Color flashColor = new Color(1f, 0.9f, 0.3f); // Gold flash
                
                LeanTween.value(waveText.gameObject, 0f, 1f, 0.3f)
                         .setOnUpdate((float t) => 
                         {
                             tmp.color = Color.Lerp(flashColor, originalColor, t);
                         });
            }
            
            Debug.Log($"[Wave Animation] Playing pulse animation for Wave {cur}/{total}");
        }
    }
    
    private bool GetIsPlayerTurn()
    {
        if (combatDisplayManager != null)
        {
            return combatDisplayManager.IsPlayerTurn();
        }
        else if (legacyCombatManager != null)
        {
            return legacyCombatManager.isPlayerTurn;
        }
        
        return true; // Default to player turn
    }
    
    #endregion
    
    #region Card Management
    
    private void AddCardToHand(CardDataExtended cardData, int index)
    {
        GameObject cardObj = GetCardFromPool();
        if (cardObj == null) return;
        
        // Get player character
        CharacterManager charManager = CharacterManager.Instance;
        Character player = charManager != null && charManager.HasCharacter() ? charManager.GetCurrentCharacter() : null;
        
        // Setup card - TEMPORARY: Convert to Card for CardVisualizer
        // (CardVisualizer can be updated to accept CardDataExtended in the future)
        #pragma warning disable CS0618 // Type or member is obsolete
        Card cardForVisualizer = cardData.ToCard();
        #pragma warning restore CS0618
        
        CardVisualizer visualizer = cardObj.GetComponent<CardVisualizer>();
        if (visualizer == null)
        {
            visualizer = cardObj.AddComponent<CardVisualizer>();
        }
        visualizer.SetCard(cardForVisualizer, player);
        
        // Setup button
        Button cardButton = cardObj.GetComponent<Button>();
        if (cardButton == null)
        {
            cardButton = cardObj.AddComponent<Button>();
        }
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(() => OnCardClicked(cardForVisualizer, cardObj));
        
        // Add hover effects
        CardHoverEffect hover = cardObj.GetComponent<CardHoverEffect>();
        if (hover == null)
        {
            hover = cardObj.AddComponent<CardHoverEffect>();
        }
        hover.animationManager = animationManager;
        
        // Create card instance
        CardInstance instance = new CardInstance
        {
            cardObject = cardObj,
            cardData = cardForVisualizer,
            handIndex = index
        };
        
        activeCards.Add(instance);
        
        // Animate card draw if manager available
        if (animationManager != null && deckPosition != null)
        {
            Vector3 startPos = deckPosition.position;
            Vector3 endPos = GetCardPosition(index);
            
            animationManager.AnimateCardDraw(cardObj, startPos, endPos);
        }
        else
        {
            // Instant positioning
            cardObj.transform.position = GetCardPosition(index);
        }
    }
    
    private void RemoveCardAt(int index)
    {
        if (index < 0 || index >= activeCards.Count) return;
        
        CardInstance instance = activeCards[index];
        ReturnCardToPool(instance.cardObject);
        activeCards.RemoveAt(index);
    }
    
    private void RepositionCards()
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            if (activeCards[i].cardObject != null)
            {
                Vector3 targetPos = GetCardPosition(i);
                activeCards[i].cardObject.transform.position = targetPos;
                activeCards[i].handIndex = i;
            }
        }
    }
    
    private Vector3 GetCardPosition(int index)
    {
        int cardCount = activeCards.Count;
        float totalWidth = (cardCount - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f;
        
        Vector3 position = cardHandParent.position;
        position.x += startX + (index * cardSpacing);
        position.y += cardYPosition;
        
        return position;
    }
    
    #endregion
    
    #region Card Pooling
    
    private GameObject GetCardFromPool()
    {
        GameObject card;
        
        if (cardPool.Count > 0)
        {
            card = cardPool.Dequeue();
        }
        else
        {
            // Create new if pool is empty
            if (cardPrefab == null) return null;
            card = Instantiate(cardPrefab, cardHandParent);
        }
        
        card.SetActive(true);
        card.transform.localScale = cardScale;
        return card;
    }
    
    private void ReturnCardToPool(GameObject card)
    {
        if (card == null) return;
        
        card.SetActive(false);
        cardPool.Enqueue(card);
    }
    
    #endregion
    
    #region Animation Helpers
    
    private void AnimatePlayerHealth()
    {
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter()) return;
        
        Character player = charManager.GetCurrentCharacter();
        
        // Update text immediately
        if (playerHealthText != null)
            playerHealthText.text = $"{player.currentHealth}/{player.maxHealth}";
        
        // Animate bar
        if (playerHealthBarFill != null && animationManager != null)
        {
            animationManager.AnimateHealthBar(
                playerHealthBarFill,
                player.currentHealth,
                player.maxHealth
            );
        }
    }
    
    private void AnimatePlayerMana()
    {
        CharacterManager charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter()) return;
        
        Character player = charManager.GetCurrentCharacter();
        
        // Update text immediately
        if (playerManaText != null)
            playerManaText.text = $"{player.mana}/{player.maxMana}";
        
        // Animate bar
        if (playerManaBarFill != null && animationManager != null)
        {
            animationManager.AnimateManaBar(
                playerManaBarFill,
                player.mana,
                player.maxMana
            );
        }
    }
    
    public Vector3 GetEnemyScreenPosition(int enemyIndex)
    {
        if (enemyIndex >= 0 && enemyIndex < enemyPanels.Length && enemyPanels[enemyIndex] != null)
        {
            Transform panelTransform = enemyPanels[enemyIndex].healthBarFill?.transform;
            if (panelTransform != null)
            {
                return panelTransform.position;
            }
        }
        
        // Fallback calculation
        return new Vector3(Screen.width * 0.7f, Screen.height * 0.7f, 0);
    }
    
    public Vector3 GetPlayerScreenPosition()
    {
        if (playerHealthBarFill != null)
        {
            return playerHealthBarFill.transform.position;
        }
        
        // Fallback
        return new Vector3(Screen.width * 0.25f, Screen.height * 0.3f, 0);
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnCardClicked(Card cardData, GameObject cardObj)
    {
        // Get target position (first enemy by default)
        Vector3 targetPos = GetEnemyScreenPosition(0);
        
        // Remove from active cards
        CardInstance instance = activeCards.Find(c => c.cardObject == cardObj);
        if (instance != null)
        {
            activeCards.Remove(instance);
        }
        
        // Animate card play
        if (animationManager != null)
        {
            animationManager.AnimateCardPlay(cardObj, targetPos, () => {
                // Play card logic after animation
                PlayCard(cardData);
                ReturnCardToPool(cardObj);
                UpdateAllUI();
            });
        }
        else
        {
            // Immediate play
            PlayCard(cardData);
            ReturnCardToPool(cardObj);
            UpdateAllUI();
        }
    }
    
    private void PlayCard(Card cardData)
    {
        // Use legacy combat manager if available
        if (legacyCombatManager != null)
        {
            legacyCombatManager.PlayCard(cardData);
        }
        else
        {
            // Handle card play through CombatDisplayManager
            // This would need to be implemented based on your card system
            Debug.Log($"Played card: {cardData.cardName}");
        }
    }
    
    private void SelectEnemy(int enemyIndex)
    {
        List<Enemy> enemies = GetActiveEnemies();
        if (enemyIndex < 0 || enemyIndex >= enemies.Count) return;
        
        // Select enemy through legacy manager if available
        if (legacyCombatManager != null)
        {
            legacyCombatManager.SelectEnemy(enemyIndex);
        }
        
        UpdateEnemyUI();
        
        LogMessage($"Targeted: {enemies[enemyIndex].enemyName}");
    }
    
    private void OnEndTurnClicked()
    {
        // End player turn through appropriate manager
        if (combatDisplayManager != null)
        {
            combatDisplayManager.EndPlayerTurn();
        }
        else if (legacyCombatManager != null)
        {
            legacyCombatManager.EndTurn();
        }
        
        // Animate turn transition
        if (animationManager != null && turnIndicator != null)
        {
            animationManager.AnimateTurnTransition(
                turnIndicator,
                "ENEMY TURN",
                new Color(1f, 0.5f, 0.5f)
            );
        }
    }
    
    private void OnCardPlayed(Card card)
    {
        LogMessage($"Played: {card.cardName}");
    }
    
    private void OnTurnEnded()
    {
        UpdateAllUI();
        
        // Animate turn transition
        if (animationManager != null && turnIndicator != null)
        {
            animationManager.AnimateTurnTransition(
                turnIndicator,
                "YOUR TURN",
                Color.yellow
            );
        }
    }
    
    private void OnCombatEnded()
    {
        List<Enemy> enemies = GetActiveEnemies();
        bool victory = enemies.Count == 0;
        LogMessage(victory ? "VICTORY!" : "DEFEAT!");
    }
    
    // Event handlers for CombatDisplayManager
    private void OnCombatStateChanged(CombatDisplayManager.CombatState state)
    {
        UpdateTurnUI();
    }
    
    private void OnTurnChanged(int turnNumber)
    {
        LogMessage($"Turn {turnNumber}");
    }
    
    private void OnTurnTypeChanged(bool isPlayerTurn)
    {
        UpdateTurnUI();
    }
    
    private void OnEnemyDefeated(Enemy enemy)
    {
        LogMessage($"{enemy.enemyName} defeated!");
    }
    
    #endregion
    
    #region Combat Log
    
    public void LogMessage(string message)
    {
        if (combatLogText != null)
        {
            combatLogText.text = message;
        }
        
        Debug.Log($"[Combat] {message}");
    }
    
    public void AddLootToken(BaseItem item, string enemyName)
    {
        if (item == null) return;
        EnsureLootLogContainer();
        if (lootLogContainer == null) return;
        
        // Trim old entries
        while (lootLogContainer.childCount >= maxLootEntries)
        {
            Destroy(lootLogContainer.GetChild(0).gameObject);
        }
        
        // Create an entry with two texts: prefix and item
        var entry = new GameObject("LootEntry");
        entry.transform.SetParent(lootLogContainer, false);
        var hlg = entry.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        
        // Prefix text
        var prefixGO = new GameObject("Prefix");
        prefixGO.transform.SetParent(entry.transform, false);
        var prefixText = prefixGO.AddComponent<Text>();
        prefixText.text = $"{enemyName} dropped ";
        prefixText.font = GetDefaultFont();
        prefixText.color = Color.white;
        prefixText.supportRichText = true;
        
        // Item text (colored)
        var itemGO = new GameObject("Item");
        itemGO.transform.SetParent(entry.transform, false);
        var itemText = itemGO.AddComponent<Text>();
        itemText.text = $"[{item.itemName}]";
        itemText.font = GetDefaultFont();
        itemText.color = GetRarityColor(item.rarity);
        itemText.supportRichText = true;
        
        // Tooltip on item token
        var trigger = itemGO.AddComponent<TooltipTrigger>();
        trigger.title = item.GetDisplayName();
        trigger.content = item.GetFullDescription();
    }
    
    private void EnsureLootLogContainer()
    {
        if (lootLogContainer != null) return;
        // Create a simple vertical container under this UI root
        var container = new GameObject("LootLogContainer");
        container.transform.SetParent(transform, false);
        var vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        lootLogContainer = container.transform;
    }
    
    private Font GetDefaultFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal: return Color.white;
            case ItemRarity.Magic: return new Color(0.29f, 0.64f, 1f); // blue
            case ItemRarity.Rare: return new Color(1f, 0.84f, 0f); // gold
            case ItemRarity.Unique: return new Color(1f, 0.5f, 0.15f); // orange
            default: return Color.white;
        }
    }
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        // Remove event listeners
        if (combatDisplayManager != null)
        {
            combatDisplayManager.OnCombatStateChanged -= OnCombatStateChanged;
            combatDisplayManager.OnTurnChanged -= OnTurnChanged;
            combatDisplayManager.OnTurnTypeChanged -= OnTurnTypeChanged;
            combatDisplayManager.OnEnemyDefeated -= OnEnemyDefeated;
            combatDisplayManager.OnCombatEnded -= OnCombatEnded;
        }
        
        if (legacyCombatManager != null)
        {
            legacyCombatManager.OnCardPlayed -= OnCardPlayed;
            legacyCombatManager.OnTurnEnded -= OnTurnEnded;
            legacyCombatManager.OnCombatEnded -= OnCombatEnded;
        }
        
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveAllListeners();
        }
    }
    
    #endregion
}

#region Helper Classes

/// <summary>
/// Represents an enemy panel in the UI
/// </summary>
[System.Serializable]
public class EnemyPanel
{
    public GameObject panelObject;
    public Text nameText;
    public Image healthBarFill;
    public Text healthText;
    public Text intentText;
    public GameObject selectionIndicator;
    public Image border;
    public Button clickArea;
}

/// <summary>
/// Represents a card instance in the hand
/// </summary>
public class CardInstance
{
    public GameObject cardObject;
    public Card cardData;
    public int handIndex;
}

#endregion

