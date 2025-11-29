using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CombatUI : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    
    [Header("Combat Manager")]
    public CombatManager combatManager;
    public CombatDisplayManager combatDisplayManager;
    
    // UI Elements
    private VisualElement root;
    private Label playerNameLabel;
    private VisualElement playerHealthFill;
    private Label playerHealthText;
    private VisualElement playerManaFill;
    private Label playerManaText;
    
    // Multiple enemy elements
    private List<VisualElement> enemyHealthFills = new List<VisualElement>();
    private List<Label> enemyHealthTexts = new List<Label>();
    private List<Label> enemyNameLabels = new List<Label>();
    private List<Label> enemyIntentTexts = new List<Label>();
    
    private Label turnIndicator;
    private Label combatLog;
    
    private ScrollView cardHand;
    private Button endTurnButton;
    private Button drawCardButton;
    
    private Label deckCountLabel;
    private Label discardCountLabel;
    private Button returnToMapButton;
    
    // Buff/Debuff display elements
    private VisualElement buffBar;
    private Label playerBuffsLabel;
    private Label enemyBuffsLabel;
    
    private void Start()
    {
        InitializeUI();
        
        // Subscribe to combat events
        if (combatManager != null)
        {
            combatManager.OnCardPlayed += OnCardPlayed;
            combatManager.OnTurnEnded += OnTurnEnded;
            combatManager.OnCombatEnded += OnCombatEnded;
        }
        
        // Also subscribe to CombatDisplayManager events
        if (combatDisplayManager == null)
        {
            combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        }
        
        if (combatDisplayManager != null)
        {
            combatDisplayManager.OnCombatStateChanged += OnCombatStateChanged;
            combatDisplayManager.OnTurnChanged += OnTurnChanged;
            combatDisplayManager.OnTurnTypeChanged += OnTurnTypeChanged;
        }
    }
    
    private void InitializeUI()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        root = uiDocument.rootVisualElement;
        
        // Load card template stylesheet
        var cardTemplateStyle = Resources.Load<StyleSheet>("UI/Combat/CardTemplate");
        if (cardTemplateStyle != null)
        {
            root.styleSheets.Add(cardTemplateStyle);
        }
        
        // Get UI elements from new layout
        playerNameLabel = root.Q<Label>("PlayerName");
        playerHealthFill = root.Q<VisualElement>("PlayerHealthFill");
        playerHealthText = root.Q<Label>("PlayerHealthText");
        playerManaFill = root.Q<VisualElement>("PlayerManaFill");
        playerManaText = root.Q<Label>("PlayerManaText");
        
        // Multiple enemy elements
        enemyHealthFills.Add(root.Q<VisualElement>("Enemy1HealthFill"));
        enemyHealthTexts.Add(root.Q<Label>("Enemy1HealthText"));
        enemyNameLabels.Add(root.Q<Label>("Enemy1Name"));
        enemyIntentTexts.Add(root.Q<Label>("Enemy1Intent"));
        
        enemyHealthFills.Add(root.Q<VisualElement>("Enemy2HealthFill"));
        enemyHealthTexts.Add(root.Q<Label>("Enemy2HealthText"));
        enemyNameLabels.Add(root.Q<Label>("Enemy2Name"));
        enemyIntentTexts.Add(root.Q<Label>("Enemy2Intent"));
        
        enemyHealthFills.Add(root.Q<VisualElement>("Enemy3HealthFill"));
        enemyHealthTexts.Add(root.Q<Label>("Enemy3HealthText"));
        enemyNameLabels.Add(root.Q<Label>("Enemy3Name"));
        enemyIntentTexts.Add(root.Q<Label>("Enemy3Intent"));
        
        // Set up enemy click events for targeting
        SetupEnemyTargeting();
        
        turnIndicator = root.Q<Label>("TurnIndicator");
        combatLog = root.Q<Label>("CombatLog");
        
        cardHand = root.Q<ScrollView>("CardHand");
        endTurnButton = root.Q<Button>("EndTurnButton");
        drawCardButton = root.Q<Button>("DrawCardButton");
        
        deckCountLabel = root.Q<Label>("DeckCount");
        discardCountLabel = root.Q<Label>("DiscardCount");
        returnToMapButton = root.Q<Button>("ReturnToMapButton");
        
        // Get buff/debuff display elements
        buffBar = root.Q<VisualElement>("BuffBar");
        playerBuffsLabel = root.Q<Label>("PlayerBuffs");
        enemyBuffsLabel = root.Q<Label>("EnemyBuffs");
        
        // Set up button events
        endTurnButton.clicked += OnEndTurnClicked;
        drawCardButton.clicked += OnDrawCardClicked;
        returnToMapButton.clicked += OnReturnToMapClicked;
        
        // Initial UI update
        UpdateCombatUI();
    }
    
    private void SetupEnemyTargeting()
    {
        // Get enemy containers and set up click events
        for (int i = 0; i < 3; i++)
        {
            VisualElement enemyContainer = root.Q<VisualElement>($"Enemy{i + 1}");
            int enemyIndex = i; // Capture the index for the lambda
            
            enemyContainer.RegisterCallback<ClickEvent>(evt => OnEnemyClicked(enemyIndex));
        }
    }
    
    private void OnEnemyClicked(int enemyIndex)
    {
        if (combatManager != null)
        {
            combatManager.SelectEnemy(enemyIndex);
        }
    }
    
    public void UpdateCombatUI()
    {
        if (combatManager == null) return;
        
        UpdatePlayerInfo();
        UpdateEnemyInfo();
        UpdateCombatStatus();
        UpdateDeckInfo();
        UpdateHand();
        UpdateBuffDisplay();
    }
    
    private void UpdatePlayerInfo()
    {
        if (combatManager.playerCharacter == null) return;
        
        Character player = combatManager.playerCharacter;
        
        playerNameLabel.text = player.characterName;
        
        // Update health bar
        float healthPercentage = (float)player.currentHealth / player.maxHealth;
        playerHealthFill.style.width = Length.Percent(healthPercentage * 100f);
        
        // NOTE: Health color management is now handled by PlayerCombatDisplay.cs
        // This was causing conflicts with the red color set in editor mode
        // The PlayerCombatDisplay component now has a useDynamicHealthColor setting
        
        playerHealthText.text = $"{player.currentHealth}/{player.maxHealth}";
        
        // Update mana bar
        float manaPercentage = (float)player.mana / player.maxMana;
        playerManaFill.style.width = Length.Percent(manaPercentage * 100f);
        
        playerManaText.text = $"Mana: {player.mana}/{player.maxMana} (+{player.manaRecoveryPerTurn}/turn)";
    }
    
    private void UpdateEnemyInfo()
    {
        if (combatManager == null) return;
        
        // Update all enemies
        for (int i = 0; i < 3; i++)
        {
            if (i < combatManager.enemies.Count)
            {
                // Show this enemy
                enemyHealthFills[i].parent.style.display = DisplayStyle.Flex;
                
                Enemy enemy = combatManager.enemies[i];
                enemyNameLabels[i].text = enemy.enemyName;
                // Set color based on rarity and tier
                EnemyData enemyData = GetEnemyDataForEnemy(enemy);
                EnemyTier tier = enemyData != null ? enemyData.tier : EnemyTier.Normal;
                enemyNameLabels[i].style.color = GetEnemyNameColor(enemy.rarity, tier);
                
                // Update health bar
                float healthPercentage = enemy.GetHealthPercentage();
                enemyHealthFills[i].style.width = Length.Percent(healthPercentage * 100f);
                
                // Update health color based on percentage
                if (healthPercentage > 0.5f)
                    enemyHealthFills[i].style.backgroundColor = new Color(1f, 0.4f, 0.4f);
                else if (healthPercentage > 0.25f)
                    enemyHealthFills[i].style.backgroundColor = new Color(1f, 0.8f, 0f);
                else
                    enemyHealthFills[i].style.backgroundColor = new Color(1f, 0.2f, 0.2f);
                
                enemyHealthTexts[i].text = $"{enemy.currentHealth}/{enemy.maxHealth}";
                enemyIntentTexts[i].text = $"Intent: {enemy.GetIntentDescription()}";
                
                // Show targeting indicator
                if (i == combatManager.selectedEnemyIndex)
                {
                    enemyHealthFills[i].parent.style.borderLeftColor = new Color(1f, 1f, 0f); // Yellow border for selected
                    enemyHealthFills[i].parent.style.borderLeftWidth = 3;
                    enemyHealthFills[i].parent.style.borderRightColor = new Color(1f, 1f, 0f);
                    enemyHealthFills[i].parent.style.borderRightWidth = 3;
                    enemyHealthFills[i].parent.style.borderTopColor = new Color(1f, 1f, 0f);
                    enemyHealthFills[i].parent.style.borderTopWidth = 3;
                    enemyHealthFills[i].parent.style.borderBottomColor = new Color(1f, 1f, 0f);
                    enemyHealthFills[i].parent.style.borderBottomWidth = 3;
                }
                else
                {
                    enemyHealthFills[i].parent.style.borderLeftColor = new Color(1f, 0.4f, 0.4f); // Normal red border
                    enemyHealthFills[i].parent.style.borderLeftWidth = 2;
                    enemyHealthFills[i].parent.style.borderRightColor = new Color(1f, 0.4f, 0.4f);
                    enemyHealthFills[i].parent.style.borderRightWidth = 2;
                    enemyHealthFills[i].parent.style.borderTopColor = new Color(1f, 0.4f, 0.4f);
                    enemyHealthFills[i].parent.style.borderTopWidth = 2;
                    enemyHealthFills[i].parent.style.borderBottomColor = new Color(1f, 0.4f, 0.4f);
                    enemyHealthFills[i].parent.style.borderBottomWidth = 2;
                }
            }
            else
            {
                // Hide this enemy slot
                enemyHealthFills[i].parent.style.display = DisplayStyle.None;
            }
        }
    }
    
    private void UpdateCombatStatus()
    {
        if (combatManager == null) return;
        
        if (combatManager.isPlayerTurn)
        {
            turnIndicator.text = "Your Turn";
            turnIndicator.style.color = new Color(1f, 1f, 0f);
        }
        else
        {
            turnIndicator.text = "Enemy Turn";
            turnIndicator.style.color = new Color(1f, 0.5f, 0.5f);
        }
    }
    
    private void UpdateDeckInfo()
    {
        if (combatManager == null) return;
        
        deckCountLabel.text = $"Deck: {combatManager.GetDrawPileCount()}";
        discardCountLabel.text = $"Discard: {combatManager.GetDiscardPileCount()}";
    }
    
    public void UpdateHand()
    {
        if (combatManager == null || cardHand == null) return;
        
        // Clear existing cards
        cardHand.Clear();
        
        // Get current hand
        var currentHand = combatManager.GetCurrentHand();
        
        // Add cards to hand
        for (int i = 0; i < currentHand.Count; i++)
        {
            CustomCard cardElement = CreateCustomCard(currentHand[i]);
            
            // Apply fanning transform based on position
            ApplyFanningTransform(cardElement, i, currentHand.Count);
            
            cardHand.Add(cardElement);
        }
    }
    
    private void ApplyFanningTransform(CustomCard cardElement, int index, int totalCards)
    {
        if (totalCards <= 1)
        {
            // Single card - no rotation
            cardElement.RemoveFromClassList("card-fanned");
            cardElement.RemoveFromClassList("card-fan-left");
            cardElement.RemoveFromClassList("card-fan-center");
            cardElement.RemoveFromClassList("card-fan-right");
            cardElement.AddToClassList("card-single");
            
            // Reset transform using individual style properties
            cardElement.style.marginLeft = 0;
            cardElement.style.marginTop = 0;
            cardElement.style.rotate = new StyleRotate(new Angle(0));
            cardElement.style.scale = new StyleScale(new Vector2(1, 1));
            return;
        }
        
        // Remove all fanning classes first
        cardElement.RemoveFromClassList("card-single");
        cardElement.RemoveFromClassList("card-fan-left");
        cardElement.RemoveFromClassList("card-fan-center");
        cardElement.RemoveFromClassList("card-fan-right");
        
        // Add fanning class
        cardElement.AddToClassList("card-fanned");
        
        // Calculate position in the arc (0 to 1)
        float position = (float)index / (totalCards - 1);
        
        // Calculate transform values
        float maxRotation = 15f;
        float maxOffset = 60f;
        float arcPosition = (position * 2f) - 1f; // Convert to -1 to 1
        float rotation = arcPosition * maxRotation;
        float offset = arcPosition * maxOffset;
        
        // Apply transform using individual style properties
        cardElement.style.marginLeft = offset;
        cardElement.style.marginTop = 0;
        cardElement.style.rotate = new StyleRotate(new Angle(rotation));
        cardElement.style.scale = new StyleScale(new Vector2(1, 1));
        
        // Apply position-specific classes for styling
        if (position < 0.25f)
        {
            cardElement.AddToClassList("card-fan-left");
        }
        else if (position > 0.75f)
        {
            cardElement.AddToClassList("card-fan-right");
        }
        else
        {
            cardElement.AddToClassList("card-fan-center");
        }
    }
    
    private CustomCard CreateCustomCard(Card card)
    {
        CustomCard cardElement = new CustomCard();
        
        // Set up the card data
        cardElement.SetCardData(card, combatManager.playerCharacter);
        
        // Set up click event
        cardElement.OnCardClicked += OnCustomCardClicked;
        
        return cardElement;
    }
    
    private void OnCustomCardClicked(CustomCard cardElement)
    {
        if (cardElement.isUsable)
        {
            combatManager.PlayCard(cardElement.cardData);
            UpdateCombatUI();
        }
        else
        {
            Debug.Log($"Cannot play {cardElement.cardData.cardName}: Not enough mana or requirements not met");
        }
    }
    
    private string GetCardDescription(Card card)
    {
        string description = "";
        
        if (card.damageScaling.strengthScaling > 0)
            description += $"Scales with STR\n";
        if (card.damageScaling.dexterityScaling > 0)
            description += $"Scales with DEX\n";
        if (card.damageScaling.intelligenceScaling > 0)
            description += $"Scales with INT\n";
        
        if (card.scalesWithMeleeWeapon)
            description += "Melee Weapon\n";
        if (card.scalesWithProjectileWeapon)
            description += "Projectile Weapon\n";
        if (card.scalesWithSpellWeapon)
            description += "Spell Weapon\n";
        
        if (card.isAoE)
            description += $"AoE: {card.aoeTargets} targets\n";
        
        return description.TrimEnd('\n');
    }
    
    private void OnCardClicked(Card card)
    {
        if (combatManager.CanPlayCard(card))
        {
            combatManager.PlayCard(card);
            UpdateCombatUI();
        }
        else
        {
            Debug.Log($"Cannot play {card.cardName}: Not enough mana or requirements not met");
        }
    }
    
    private void OnEndTurnClicked()
    {
        // Use CombatDisplayManager if available, otherwise fall back to CombatManager
        if (combatDisplayManager != null)
        {
            combatDisplayManager.EndPlayerTurn();
        }
        else if (combatManager != null)
        {
            combatManager.EndTurn();
        }
        else
        {
            Debug.LogWarning("No combat manager found to end turn!");
        }
    }

    // Allow external systems to set a combat log message
    public void SetCombatLogMessage(string message)
    {
        if (combatLog != null)
        {
            combatLog.text = message;
        }
        Debug.Log($"[CombatUI] {message}");
    }
    
    private void OnDrawCardClicked()
    {
        combatManager.DrawCard();
    }
    
    private void OnReturnToMapClicked()
    {
        // Find the CombatSceneManager and trigger the return to map
        CombatSceneManager sceneManager = FindFirstObjectByType<CombatSceneManager>();
        if (sceneManager != null)
        {
            sceneManager.ReturnToMap();
        }
        else
        {
            Debug.LogWarning("CombatSceneManager not found in scene");
        }
    }
    
    private void OnCardPlayed(Card card)
    {
        combatLog.text = $"{combatManager.playerCharacter.characterName} played {card.cardName}!";
    }
    
    private void OnTurnEnded()
    {
        combatLog.text = "Turn ended.";
    }
    
    private void OnCombatEnded()
    {
        if (combatManager.enemies.Count > 0)
        {
            combatLog.text = "You have been defeated!";
        }
        else
        {
            combatLog.text = "Victory! All enemies defeated!";
        }
    }
    
    public void AddCombatLog(string message)
    {
        combatLog.text = message;
    }
    
    // CombatDisplayManager event handlers
    private void OnCombatStateChanged(CombatDisplayManager.CombatState state)
    {
        Debug.Log($"Combat state changed to: {state}");
        UpdateCombatStatus();
    }
    
    private void OnTurnChanged(int turnNumber)
    {
        Debug.Log($"Turn changed to: {turnNumber}");
        // Update turn indicator if needed
    }
    
    private void OnTurnTypeChanged(bool isPlayerTurn)
    {
        Debug.Log($"Turn type changed - Player turn: {isPlayerTurn}");
        UpdateCombatStatus();
    }
    
    private void UpdateBuffDisplay()
    {
        if (playerBuffsLabel == null || enemyBuffsLabel == null) return;
        
        // Get player status effects
        string playerBuffText = GetStatusEffectText(true);
        playerBuffsLabel.text = $"Player Buffs: {playerBuffText}";
        
        // Get enemy status effects
        string enemyBuffText = GetStatusEffectText(false);
        enemyBuffsLabel.text = $"Enemy Buffs: {enemyBuffText}";
    }
    
    private string GetStatusEffectText(bool isPlayer)
    {
        string statusText = "None";
        
        if (isPlayer)
        {
            // Get player status effects
            PlayerCombatDisplay playerDisplay = FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                StatusEffectManager statusManager = playerDisplay.GetStatusEffectManager();
                if (statusManager != null)
                {
                    statusText = statusManager.GetStatusEffectSummary();
                }
            }
        }
        else
        {
            // Get enemy status effects
            EnemyCombatDisplay[] enemyDisplays = FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            if (enemyDisplays.Length > 0)
            {
                List<string> enemyStatuses = new List<string>();
                foreach (var enemyDisplay in enemyDisplays)
                {
                    if (enemyDisplay != null)
                    {
                        StatusEffectManager statusManager = enemyDisplay.GetComponent<StatusEffectManager>();
                        if (statusManager != null)
                        {
                            string enemyStatus = statusManager.GetStatusEffectSummary();
                            if (!string.IsNullOrEmpty(enemyStatus) && enemyStatus != "None")
                            {
                                enemyStatuses.Add(enemyStatus);
                            }
                        }
                    }
                }
                
                if (enemyStatuses.Count > 0)
                {
                    statusText = string.Join(", ", enemyStatuses);
                }
            }
        }
        
        return statusText;
    }
    
    /// <summary>
    /// Flash the End Turn button to indicate player can't afford any cards
    /// </summary>
    public void FlashEndTurnButton()
    {
        if (endTurnButton == null) return;
        
        // Flash the button with a pulsing red effect
        var buttonElement = endTurnButton.Q<VisualElement>(className: "unity-button__text");
        if (buttonElement != null)
        {
            // Store original background color
            Color originalColor = buttonElement.style.backgroundColor.value;
            
            // Create flash sequence
            LeanTween.sequence()
                .append(LeanTween.value(0f, 1f, 0.2f).setOnUpdate((float t) => {
                    buttonElement.style.backgroundColor = Color.Lerp(originalColor, Color.red, t);
                }))
                .append(LeanTween.value(1f, 0f, 0.2f).setOnUpdate((float t) => {
                    buttonElement.style.backgroundColor = Color.Lerp(Color.red, originalColor, t);
                }))
                .append(LeanTween.value(0f, 1f, 0.2f).setOnUpdate((float t) => {
                    buttonElement.style.backgroundColor = Color.Lerp(originalColor, Color.red, t);
                }))
                .append(LeanTween.value(1f, 0f, 0.2f).setOnUpdate((float t) => {
                    buttonElement.style.backgroundColor = Color.Lerp(Color.red, originalColor, t);
                }));
        }
        
        Debug.Log("<color=orange>End Turn button flashed - player cannot afford any cards!</color>");
    }
    
    /// <summary>
    /// Gets the EnemyData for an Enemy instance by looking it up in the database or finding its display.
    /// </summary>
    private EnemyData GetEnemyDataForEnemy(Enemy enemy)
    {
        if (enemy == null) return null;
        
        // Try to find the EnemyCombatDisplay that has this enemy
        if (combatDisplayManager != null)
        {
            var activeDisplays = combatDisplayManager.GetActiveEnemyDisplays();
            foreach (var display in activeDisplays)
            {
                if (display != null && display.GetEnemy() == enemy)
                {
                    return display.GetEnemyData();
                }
            }
        }
        
        // Fallback: Look up by name in EnemyDatabase (strip rarity suffix if present)
        string baseName = enemy.enemyName;
        if (baseName.Contains(" (Magic)") || baseName.Contains(" (Rare)") || baseName.Contains(" (Unique)"))
        {
            baseName = baseName.Substring(0, baseName.LastIndexOf(" ("));
        }
        
        if (EnemyDatabase.Instance != null)
        {
            return EnemyDatabase.Instance.GetEnemyByName(baseName);
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the color for enemy name based on rarity and tier.
    /// Common (Normal) - White, Magic - Blue, Rare - Yellow, Unique/Boss/Mini-boss - Orange
    /// </summary>
    private UnityEngine.Color GetEnemyNameColor(EnemyRarity rarity, EnemyTier tier)
    {
        // Boss and Mini-boss always use Orange
        if (tier == EnemyTier.Boss || tier == EnemyTier.Miniboss)
        {
            return new UnityEngine.Color(1f, 0.65f, 0f); // Orange
        }
        
        // Otherwise use rarity-based colors
        switch (rarity)
        {
            case EnemyRarity.Normal:
                return UnityEngine.Color.white;
            case EnemyRarity.Magic:
                return new UnityEngine.Color(0.3f, 0.6f, 1f); // Blue
            case EnemyRarity.Rare:
                return new UnityEngine.Color(1f, 0.9f, 0.2f); // Yellow
            case EnemyRarity.Unique:
                return new UnityEngine.Color(1f, 0.65f, 0f); // Orange
            default:
                return UnityEngine.Color.white;
        }
    }
}
