using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class EnemyCombatDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Image enemyPortrait;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyTypeText;
    
    [Header("Health Display")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image healthFillImage;
    
    [Header("Stagger Display")]
    [Tooltip("Stagger bar overlay that appears on top of the health bar")]
    public Image staggerBarOverlay;
    [Tooltip("Optional text to display stagger percentage")]
    public TextMeshProUGUI staggerText;
    
    [Header("Intent Display")]
    public GameObject intentContainer;
    public Image intentIcon;
    public TextMeshProUGUI intentText;
    public TextMeshProUGUI intentDamageText;
    
    [Header("Status Effects")]
    public Transform statusEffectsContainer;
    public GameObject statusEffectPrefab;
    
    [Header("Energy Display")]
    [SerializeField] private GameObject energyContainer;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Color energyReadyColor = new Color(0.3f, 0.85f, 1f);
    [SerializeField] private Color energyDepletedColor = new Color(0.1f, 0.25f, 0.4f);
    
    [Header("Guard Display")]
    [SerializeField] private GameObject guardContainer;
    [SerializeField] private Image guardFillImage;
    [SerializeField] private TextMeshProUGUI guardText;
    [SerializeField] private Color guardColor = new Color(0.2f, 0.6f, 1f);
    
    [Header("Stacks")]
    public Transform stacksContainer;
    public GameObject stackIconPrefab;
    [Tooltip("If true, stack icons are hidden while their value is zero.")]
    public bool hideZeroStacks = true;
    
    private StatusEffectManager statusEffectManager;
    
    [Header("Colors")]
    public static readonly Color DefaultAbilityIntentColor = new Color(0.6f, 0.3f, 1f, 1f);
    public Color healthColor = Color.red;
    public Color attackIntentColor = Color.red;
    public Color defendIntentColor = Color.blue;
    public Color abilityIntentColor = DefaultAbilityIntentColor;
    
    [Header("Intent Icons")]
    public Sprite attackIcon;
    public Sprite defendIcon;
    public Sprite abilityIcon;
    
    [Header("Animation")]
    public Animator enemyAnimator; // Animator for enemy sprite animations
    private RuntimeAnimatorController enemyAnimatorController; // Store the enemy's specific animator controller
    
    private Enemy currentEnemy;
    private EnemyData enemyData; // Reference to the data used to create this enemy
    private EnemyAbilityRunner abilityRunner;
    private bool deathNotified = false;
    private bool showingAbilityIntent = false;
    private string activeAbilityIntentName = null;
    
    private Vector2 baseHealthAnchoredPos;
    private Vector2 baseIntentAnchoredPos;
    private Vector2 baseNameAnchoredPos;
    private bool cachedBaseAnchoredPositions = false;
    private bool isInitialized = false;
    private Enemy subscribedEnemyForStacks;
    private Enemy energySubscribedEnemy;
    
    private class StackIconElements
    {
        public GameObject root;
        public Image icon;
        public TextMeshProUGUI value;
    }

    private readonly Dictionary<StackType, StackIconElements> stackIconLookup = new Dictionary<StackType, StackIconElements>();
    private readonly Dictionary<StackType, Sprite> stackSpriteCache = new Dictionary<StackType, Sprite>();
    
    /// <summary>
    /// Get the current Enemy instance
    /// </summary>
    public Enemy GetEnemy()
    {
        return currentEnemy;
    }
    
    /// <summary>
    /// Get the EnemyData for this display (for loot tracking)
    /// </summary>
    public EnemyData GetEnemyData()
    {
        return enemyData;
    }
    
    private void Start()
    {
        InitializeDisplay();
    }
    
    private void InitializeDisplay()
    {
        EnsureAbilityRunner();

        // Initialize StatusEffectManager
        statusEffectManager = GetComponent<StatusEffectManager>();
        if (statusEffectManager == null)
        {
            statusEffectManager = gameObject.AddComponent<StatusEffectManager>();
        }
        
        // Set up status effect container
        if (statusEffectsContainer == null)
        {
            // Defensive: avoid orphaned transforms if our display gets destroyed during scene load
            if (this == null || gameObject == null) return;
            GameObject container = new GameObject("StatusEffectsContainer");
            if (container == null) return;
            container.transform.SetParent(transform, false);
            container.transform.localPosition = new Vector3(0, 120, 0);
            statusEffectsContainer = container.transform;
        }
        
        // Set up status effect manager
        statusEffectManager.statusEffectContainer = statusEffectsContainer;
        if (statusEffectPrefab != null)
        {
            statusEffectManager.statusEffectIconPrefab = statusEffectPrefab;
        }
        
        // Ensure a simple horizontal layout for the status effect bar
        if (statusEffectsContainer != null)
        {
            var layout = statusEffectsContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = statusEffectsContainer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                layout.spacing = 6f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }
            var fitter = statusEffectsContainer.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = statusEffectsContainer.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            }
        }
        
        EnsureEnergyUI();
        EnsureGuardUI();
        
        // Provide a default runtime status-effect icon prefab if none assigned
        if (statusEffectPrefab == null)
        {
            var prefab = new GameObject("StatusEffectIconPrefab_Runtime", typeof(RectTransform));
            var iconImage = prefab.AddComponent<UnityEngine.UI.Image>();
            var icon = prefab.AddComponent<StatusEffectIcon>();
            
            // Background child
            var bgGO = new GameObject("Background", typeof(RectTransform));
            bgGO.transform.SetParent(prefab.transform, false);
            var bgImg = bgGO.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.5f);
            
            // Duration text child
            var durGO = new GameObject("DurationText", typeof(RectTransform));
            durGO.transform.SetParent(prefab.transform, false);
            var durText = durGO.AddComponent<TMPro.TextMeshProUGUI>();
            durText.alignment = TMPro.TextAlignmentOptions.BottomRight;
            durText.fontSize = 18f;
            
            // Magnitude text child
            var magGO = new GameObject("MagnitudeText", typeof(RectTransform));
            magGO.transform.SetParent(prefab.transform, false);
            var magText = magGO.AddComponent<TMPro.TextMeshProUGUI>();
            magText.alignment = TMPro.TextAlignmentOptions.TopRight;
            magText.fontSize = 18f;
            
            statusEffectPrefab = prefab;
            statusEffectManager.statusEffectIconPrefab = statusEffectPrefab;
        }
        
        // Auto-find components if not assigned
        if (enemyNameText == null)
        {
            enemyNameText = transform.Find("EnemyName")?.GetComponent<TextMeshProUGUI>();
            if (enemyNameText == null)
            {
                // Fallback: search any TMP child that contains "Name"
                var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (t != null && t.name.IndexOf("name", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        enemyNameText = t; break;
                    }
                }
                if (enemyNameText == null && tmps.Length > 0) enemyNameText = tmps[0];
            }
        }
            
        if (enemyTypeText == null)
        {
            enemyTypeText = transform.Find("EnemyType")?.GetComponent<TextMeshProUGUI>();
            if (enemyTypeText == null)
            {
                var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (t != null && t.name.IndexOf("type", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    { enemyTypeText = t; break; }
                }
            }
        }
            
        if (healthSlider == null)
            healthSlider = transform.Find("HealthBar")?.GetComponent<Slider>();
            
        if (healthText == null)
            healthText = transform.Find("HealthBar/HealthText")?.GetComponent<TextMeshProUGUI>();
            
        if (intentContainer == null)
            intentContainer = transform.Find("IntentContainer")?.gameObject;
            
        if (intentIcon == null)
            intentIcon = transform.Find("IntentContainer/IntentIcon")?.GetComponent<Image>();
            
        if (intentText == null)
            intentText = transform.Find("IntentContainer/IntentText")?.GetComponent<TextMeshProUGUI>();
            
        if (intentDamageText == null)
            intentDamageText = transform.Find("IntentContainer/IntentDamageText")?.GetComponent<TextMeshProUGUI>();
            
        if (statusEffectsContainer == null)
            statusEffectsContainer = transform.Find("StatusEffectsContainer")?.transform;
        
        if (stacksContainer == null)
            stacksContainer = transform.Find("StacksContainer");
        
        EnsureStacksUI();
        
        // Set up colors
        if (healthFillImage != null)
            healthFillImage.color = healthColor;
        
        CacheBaseAnchoredPositions();
        
        // Auto-find enemy animator if not assigned
        if (enemyAnimator == null && enemyPortrait != null)
        {
            // Try multiple locations for the Animator component
            // 1. On the portrait Image itself
            enemyAnimator = enemyPortrait.GetComponent<Animator>();
            
            // 2. On the parent of the portrait
            if (enemyAnimator == null)
            {
                enemyAnimator = enemyPortrait.transform.parent?.GetComponent<Animator>();
            }
            
            // 3. On any child of the portrait (common if portrait is a container)
            if (enemyAnimator == null)
            {
                enemyAnimator = enemyPortrait.GetComponentInChildren<Animator>(true);
            }
            
            // 4. Search siblings (if portrait is just the Image and animator is on another sibling)
            if (enemyAnimator == null && enemyPortrait.transform.parent != null)
            {
                enemyAnimator = enemyPortrait.transform.parent.GetComponentInChildren<Animator>(true);
            }
            
            if (enemyAnimator != null)
            {
                Debug.Log($"✓ Auto-found Animator component on: {enemyAnimator.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"⚠ No Animator component found for Enemy Portrait. Animations will not play. Add an Animator component to the Enemy Portrait GameObject or its children.");
            }
        }
        
        // Set up dynamic animator controller from enemy data
        SetupEnemyAnimator();
        
        isInitialized = true;
        // If an enemy was assigned before Start(), ensure the UI updates now
        if (currentEnemy != null)
        {
            UpdateDisplay();
        }
        
        // Ensure enemy panel is clickable for targeting
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<UnityEngine.UI.Button>();
            button.transition = UnityEngine.UI.Selectable.Transition.None;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            var targeting = EnemyTargetingManager.Instance;
            if (targeting != null)
            {
                // Find my index in CombatDisplayManager
                var cdm = FindFirstObjectByType<CombatDisplayManager>();
                if (cdm != null)
                {
                    var activeDisplays = cdm.GetActiveEnemyDisplays();
                    for (int i = 0; i < activeDisplays.Count; i++)
                    {
                        if (activeDisplays[i] == this)
                        {
                            targeting.SelectEnemy(i);
                            break;
                        }
                    }
                }
            }
        });
        
        // DON'T auto-create test enemy - let CombatDisplayManager assign real enemies
        // If you want a placeholder, CombatDisplayManager will handle it
    }
    
    private void CreateTestEnemy()
    {
        // DEPRECATED: This is now handled by CombatDisplayManager
        // Keeping method for backward compatibility but not auto-called
        currentEnemy = new Enemy("Test Goblin", 50, 8);
        currentEnemy.SetIntent();
        UpdateDisplay();
    }
    
    public void SetEnemy(Enemy enemy, EnemyData data = null)
    {
        UnsubscribeFromEnemyStacks();
        UnsubscribeFromEnemyEnergy();
        currentEnemy = enemy;
        enemyData = data;
        deathNotified = false;
        InitializeAbilityRunner();
        SubscribeToEnemyStacks();
        SubscribeToEnemyEnergy();
        
        // Set up animator when enemy data changes
        SetupEnemyAnimator();
        
        // Always update display when enemy is set, even if not fully initialized yet
        // This ensures spawned enemies show immediately without waiting for turn advance
        UpdateDisplay();
    }
    
    /// <summary>
    /// Clear enemy data from this display (for wave resets)
    /// </summary>
    public void ClearEnemy()
    {
        UnsubscribeFromEnemyStacks();
        UnsubscribeFromEnemyEnergy();
        currentEnemy = null;
        enemyData = null;
        deathNotified = false;
        
        // Clear visual elements
        if (enemyNameText != null)
            enemyNameText.text = "";
        
        if (enemyTypeText != null)
            enemyTypeText.text = "";
        
        if (enemyPortrait != null)
        {
            // COMPLETE RESET: Clear sprite and reset to default state
            enemyPortrait.sprite = null;
            enemyPortrait.color = Color.white;
            
            // Keep Image component enabled - we'll disable the whole GameObject instead
            // This prevents Unity Canvas rendering issues when re-enabling
            enemyPortrait.enabled = true;
            
            Debug.Log($"[Clear Enemy] Reset portrait sprite for {gameObject.name}");
        }
        
        // Animator is handled by SetupEnemyAnimator when new enemy is assigned
        
        if (healthSlider != null)
            healthSlider.value = 0;
        
        if (healthText != null)
            healthText.text = "0 / 0";
        
        if (intentContainer != null)
            intentContainer.SetActive(false);
        
        // Clear status effects
        if (statusEffectManager != null)
        {
            statusEffectManager.ClearAllStatusEffects();
        }
        
        ClearStackDisplay();
        
        // Force Canvas update to clear any cached rendering
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"[EnemyCombatDisplay] Cleared enemy data from {gameObject.name}");
    }
    
    /// <summary>
    /// Set enemy from EnemyData (creates Enemy instance automatically).
    /// </summary>
    public void SetEnemyFromData(EnemyData data)
    {
        if (data == null) return;
        
        UnsubscribeFromEnemyStacks();
        UnsubscribeFromEnemyEnergy();
        enemyData = data;
        
        // Get area level for scaling (from EncounterManager or maze context)
        int areaLevel = GetAreaLevel();
        
        // Create enemy with area level scaling
        currentEnemy = data.CreateEnemy(areaLevel);
        deathNotified = false;
        InitializeAbilityRunner();
        SubscribeToEnemyStacks();
        SubscribeToEnemyEnergy();
        
        // Set up animator when enemy data changes
        SetupEnemyAnimator();
        
        if (isInitialized)
        {
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (currentEnemy == null) return;
        
        // Update basic info
        if (enemyNameText != null)
        {
            enemyNameText.text = currentEnemy.enemyName;
            // Set color based on rarity and tier
            enemyNameText.color = GetEnemyNameColor(currentEnemy.rarity, enemyData?.tier ?? EnemyTier.Normal);
        }
        
        // Update sprite from EnemyData if available
        if (enemyData != null && enemyPortrait != null && enemyData.enemySprite != null)
        {
            // FORCE COMPLETE RESET: Clear any previous state that might be "stuck"
            enemyPortrait.sprite = null;
            enemyPortrait.enabled = false;
            enemyPortrait.color = Color.white;
            enemyPortrait.raycastTarget = false;
            
            // Force Canvas update to clear any cached state
            Canvas.ForceUpdateCanvases();
            
            // Ensure portrait GameObject is active
            if (!enemyPortrait.gameObject.activeInHierarchy)
            {
                enemyPortrait.gameObject.SetActive(true);
                Debug.Log($"[Portrait Fix] Activated portrait GameObject for {enemyData.enemyName}");
            }
            
            // Check if enemy uses animations or static sprite
            bool hasAnimations = enemyData.animatorController != null && enemyAnimator != null && enemyAnimator.enabled;
            
            if (hasAnimations)
            {
                // Animator will handle sprite updates
                enemyPortrait.sprite = enemyData.enemySprite; // Set initial sprite
                enemyPortrait.enabled = true;
                Debug.Log($"✓ Set initial sprite for {enemyData.enemyName} (animations enabled)");
            }
            else
            {
                // No animations - use static sprite
                enemyPortrait.sprite = enemyData.enemySprite;
                enemyPortrait.enabled = true;
                Debug.Log($"✓ Set static sprite for {enemyData.enemyName}: {enemyData.enemySprite.name}");
            }
            
            // Force sprite to render by setting color to fully opaque
            enemyPortrait.color = Color.white;
            
            // Ensure portrait order/layering
            EnsureUILayering();
            
            // Check for CanvasGroup that might block visibility
            CanvasGroup cg = enemyPortrait.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
            }
            
            // Force Canvas to rebuild immediately
            Canvas.ForceUpdateCanvases();
            
            // Apply display scale to the portrait
            ApplyDisplayScale();
            
            // Adjust UI element positions based on portrait size
            AdjustUILayoutForPortraitScale();
            
            // Additional: Set raycast target to ensure it's "visible" to Unity
            enemyPortrait.raycastTarget = true;
        }
        else
        {
            Debug.LogWarning($"⚠ Cannot set sprite for {enemyData?.enemyName ?? "unknown"}: enemyData={enemyData != null}, enemyPortrait={enemyPortrait != null}, enemySprite={enemyData?.enemySprite != null}");
        }
        
        // Update type/category from EnemyData
        if (enemyTypeText != null)
        {
            if (enemyData != null)
            {
                // Prefer rarity naming (Normal/Magic/Rare/Unique) with category
                enemyTypeText.text = $"{enemyData.rarity} {enemyData.category}";

                // Color code by rarity
                switch (enemyData.rarity)
                {
                    case EnemyRarity.Normal:
                        enemyTypeText.color = Color.white;
                        break;
                    case EnemyRarity.Magic:
                        enemyTypeText.color = new Color(0.29f, 0.64f, 1f); // Blue
                        break;
                    case EnemyRarity.Rare:
                        enemyTypeText.color = new Color(1f, 0.84f, 0f); // Gold
                        break;
                    case EnemyRarity.Unique:
                        enemyTypeText.color = new Color(1f, 0.5f, 0.15f); // Orange
                        break;
                }
            }
            else
            {
                enemyTypeText.text = "Enemy"; // Fallback
            }
        }
        
        // Update health
        UpdateHealthDisplay();
        
        // Update stagger display
        UpdateStaggerDisplay();
        
        // Update intent
        UpdateIntentDisplay();
        
        // Update status effects
        UpdateStatusEffects();
        UpdateEnergyDisplay();
        UpdateGuardDisplay();

        // Apply layout scaling if EnemyData provides displayScale/basePanelHeight
        if (enemyData != null)
        {
            var le = GetComponent<UnityEngine.UI.LayoutElement>();
            if (le == null) le = gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            float scaled = Mathf.Clamp(enemyData.basePanelHeight * Mathf.Max(0.25f, enemyData.displayScale), 100f, 1200f);
            le.preferredHeight = scaled;
        }

        UpdateStackDisplay();
    }
    
    private void UpdateHealthDisplay()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = currentEnemy.maxHealth;
            float target = Mathf.Clamp(currentEnemy.currentHealth, 0, currentEnemy.maxHealth);
            if (Application.isPlaying)
            {
                LeanTween.cancel(healthSlider.gameObject);
                LeanTween.value(healthSlider.gameObject, healthSlider.value, target, 0.25f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnUpdate((float v) => { if (healthSlider != null) healthSlider.value = v; });
            }
            else
            {
                healthSlider.value = target;
            }
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentEnemy.currentHealth}/{currentEnemy.maxHealth}";
        }
    }
    
    /// <summary>
    /// Update the stagger bar overlay on the health bar
    /// </summary>
    public void UpdateStaggerDisplay()
    {
        if (currentEnemy == null) return;
        
        // Show/hide stagger bar based on whether enemy can be staggered
        bool canStagger = currentEnemy.staggerThreshold > 0f;
        
        if (staggerBarOverlay != null)
        {
            // Only show if enemy can be staggered and has some stagger
            bool shouldShow = canStagger && currentEnemy.currentStagger > 0f;
            staggerBarOverlay.gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                // Calculate stagger percentage (0-1)
                float staggerPercentage = currentEnemy.GetStaggerPercentage();
                
                // Update fill amount (assuming Image Type is Filled)
                if (staggerBarOverlay.type == Image.Type.Filled)
                {
                    // Animate the fill smoothly
                    if (Application.isPlaying)
                    {
                        LeanTween.cancel(staggerBarOverlay.gameObject);
                        LeanTween.value(staggerBarOverlay.gameObject, staggerBarOverlay.fillAmount, staggerPercentage, 0.3f)
                            .setEase(LeanTweenType.easeOutQuad)
                            .setOnUpdate((float v) => {
                                if (staggerBarOverlay != null)
                                    staggerBarOverlay.fillAmount = v;
                            });
                    }
                    else
                    {
                        staggerBarOverlay.fillAmount = staggerPercentage;
                    }
                }
                else
                {
                    // If not using filled, adjust scale or width
                    RectTransform rect = staggerBarOverlay.GetComponent<RectTransform>();
                    if (rect != null && healthSlider != null)
                    {
                        // Match the health bar width and scale horizontally
                        RectTransform healthRect = healthSlider.GetComponent<RectTransform>();
                        if (healthRect != null)
                        {
                            float healthBarWidth = healthRect.rect.width;
                            rect.sizeDelta = new Vector2(healthBarWidth * staggerPercentage, rect.sizeDelta.y);
                        }
                    }
                }
            }
        }
        
        // Update stagger text if available
        if (staggerText != null)
        {
            if (canStagger && currentEnemy.currentStagger > 0f)
            {
                float staggerPercent = currentEnemy.GetStaggerPercentage() * 100f;
                staggerText.text = $"Stagger: {staggerPercent:F0}%";
                staggerText.gameObject.SetActive(true);
            }
            else
            {
                staggerText.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateIntentDisplay()
    {
        if (intentContainer != null)
        {
            intentContainer.SetActive(true);
        }
        
        // Check for crowd control status effects (Frozen, Stunned) that prevent actions
        bool isFrozen = statusEffectManager != null && statusEffectManager.HasStatusEffect(StatusEffectType.Freeze);
        bool isStunned = statusEffectManager != null && statusEffectManager.HasStatusEffect(StatusEffectType.Stun);
        
        // Display crowd control status instead of normal intent
        if (isFrozen)
        {
            if (intentText != null)
            {
                intentText.text = "FROZEN!";
                intentText.color = new Color(0.6f, 0.9f, 1f); // Light blue
            }
            if (intentDamageText != null)
            {
                intentDamageText.text = "";
            }
            if (intentIcon != null)
            {
                intentIcon.sprite = null; // Or use a frozen icon if available
            }
            return;
        }
        
        if (isStunned)
        {
            if (intentText != null)
            {
                intentText.text = "STAGGERED!";
                intentText.color = Color.yellow;
            }
            if (intentDamageText != null)
            {
                intentDamageText.text = "";
            }
            if (intentIcon != null)
            {
                intentIcon.sprite = null; // Or use a stunned icon if available
            }
            return;
        }
        
        // Normal intent display (if not frozen/stunned)
        if (showingAbilityIntent)
        {
            intentText.text = activeAbilityIntentName;
            return;
        }
        
        if (intentText != null)
        {
            intentText.text = currentEnemy.GetIntentDescription();
            intentText.color = Color.white; // Reset to default color
        }
        
        if (intentDamageText != null)
        {
            if (currentEnemy.currentIntent == EnemyIntent.Attack)
            {
                intentDamageText.text = currentEnemy.intentDamage.ToString();
                intentDamageText.color = attackIntentColor;
            }
            else
            {
                intentDamageText.text = "";
            }
        }
        
        if (intentIcon != null)
        {
            switch (currentEnemy.currentIntent)
            {
                case EnemyIntent.Attack:
                    intentIcon.sprite = attackIcon;
                    intentIcon.color = attackIntentColor;
                    break;
                case EnemyIntent.Defend:
                    intentIcon.sprite = defendIcon;
                    intentIcon.color = defendIntentColor;
                    break;
                default:
                    intentIcon.sprite = null;
                    break;
            }
        }
    }
    
    private void UpdateStatusEffects()
    {
        // Status effects are now managed by StatusEffectManager
        // This method is kept for compatibility but the actual work is done by StatusEffectManager
        if (statusEffectManager != null)
        {
            // The StatusEffectManager handles all the visual updates automatically
            // This method can be used for additional custom logic if needed
        }
    }

    private void EnsureStacksUI()
    {
        if (stacksContainer == null)
        {
            stacksContainer = transform.Find("StacksContainer");
            if (stacksContainer == null)
            {
                GameObject container = new GameObject("StacksContainer", typeof(RectTransform));
                container.transform.SetParent(transform, false);
                var rect = container.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(119.6f, -20f);
                rect.sizeDelta = new Vector2(239.2f, 40f);
                stacksContainer = rect;
            }
        }

        if (stacksContainer != null)
        {
            var layout = stacksContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = stacksContainer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                layout.spacing = 6f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
            }

            var fitter = stacksContainer.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = stacksContainer.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        EnsureStackIcons();
    }

    private void EnsureStackIcons()
    {
        if (stacksContainer == null) return;

        foreach (StackType type in Enum.GetValues(typeof(StackType)))
        {
            if (stackIconLookup.ContainsKey(type))
                continue;

            StackIconElements elements = stackIconPrefab != null
                ? ExtractStackIconElements(Instantiate(stackIconPrefab, stacksContainer))
                : CreateDefaultStackIconElements();

            elements.root.name = $"Stack_{type}";
            stackIconLookup[type] = elements;

            UpdateStackIconSprite(type, elements);

            if (elements.value != null)
            {
                elements.value.text = "0";
            }

            if (hideZeroStacks)
            {
                elements.root.SetActive(false);
            }
            else
            {
                elements.root.SetActive(true);
            }
        }
    }

    private StackIconElements CreateDefaultStackIconElements()
    {
        var root = new GameObject("StackIcon", typeof(RectTransform));
        root.transform.SetParent(stacksContainer, false);
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(48f, 48f);

        var background = root.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.4f);
        background.raycastTarget = false;

        var iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(root.transform, false);
        var iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(36f, 36f);
        var iconImage = iconGO.AddComponent<Image>();
        iconImage.raycastTarget = false;

        var valueGO = new GameObject("Value", typeof(RectTransform));
        valueGO.transform.SetParent(root.transform, false);
        var valueRect = valueGO.GetComponent<RectTransform>();
        valueRect.anchorMin = Vector2.zero;
        valueRect.anchorMax = Vector2.one;
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;
        var valueText = valueGO.AddComponent<TextMeshProUGUI>();
        valueText.alignment = TextAlignmentOptions.BottomRight;
        valueText.fontSize = 26f;
        valueText.raycastTarget = false;
        if (enemyNameText != null)
        {
            valueText.font = enemyNameText.font;
        }

        return new StackIconElements
        {
            root = root,
            icon = iconImage,
            value = valueText
        };
    }

    private StackIconElements ExtractStackIconElements(GameObject instance)
    {
        var elements = new StackIconElements
        {
            root = instance,
            icon = FindIconImage(instance),
            value = FindValueLabel(instance)
        };

        if (elements.icon == null)
        {
            elements.icon = instance.GetComponent<Image>();
            if (elements.icon == null)
            {
                elements.icon = instance.AddComponent<Image>();
            }
            elements.icon.raycastTarget = false;
        }

        if (elements.value == null)
        {
            var valueGO = new GameObject("Value", typeof(RectTransform));
            valueGO.transform.SetParent(instance.transform, false);
            var valueRect = valueGO.GetComponent<RectTransform>();
            valueRect.anchorMin = Vector2.zero;
            valueRect.anchorMax = Vector2.one;
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            var valueText = valueGO.AddComponent<TextMeshProUGUI>();
            valueText.alignment = TextAlignmentOptions.BottomRight;
            valueText.fontSize = 26f;
            valueText.raycastTarget = false;
            if (enemyNameText != null)
            {
                valueText.font = enemyNameText.font;
            }
            elements.value = valueText;
        }

        return elements;
    }

    private Image FindIconImage(GameObject root)
    {
        if (root == null) return null;

        Transform iconTransform = root.transform.Find("Icon");
        if (iconTransform != null)
        {
            var icon = iconTransform.GetComponent<Image>();
            if (icon != null) return icon;
        }

        var directImage = root.GetComponent<Image>();
        if (directImage != null && directImage.sprite != null)
        {
            return directImage;
        }

        foreach (var image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject == root) continue;
            return image;
        }

        return null;
    }

    private TextMeshProUGUI FindValueLabel(GameObject root)
    {
        if (root == null) return null;

        Transform valueTransform = root.transform.Find("Value");
        if (valueTransform != null)
        {
            var label = valueTransform.GetComponent<TextMeshProUGUI>();
            if (label != null) return label;
        }

        return root.GetComponentInChildren<TextMeshProUGUI>();
    }

    private Sprite LoadStackSprite(StackType type)
    {
        if (stackSpriteCache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        Sprite sprite = Resources.Load<Sprite>($"UI/Stacks/{type}");
        stackSpriteCache[type] = sprite;
        return sprite;
    }

    private void UpdateStackIconSprite(StackType type, StackIconElements elements)
    {
        if (elements == null || elements.icon == null) return;

        Sprite sprite = LoadStackSprite(type);
        if (sprite != null)
        {
            elements.icon.sprite = sprite;
            elements.icon.enabled = true;
        }
        else
        {
            elements.icon.enabled = false;
        }
    }

    private void EnsureEnergyUI()
    {
        if (energyContainer == null)
        {
            var existing = transform.Find("EnergyContainer");
            if (existing != null)
            {
                energyContainer = existing.gameObject;
                energyFillImage = energyContainer.transform.Find("EnergyFill")?.GetComponent<Image>();
                energyText = energyContainer.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (energyContainer == null)
        {
            var container = new GameObject("EnergyContainer", typeof(RectTransform));
            container.transform.SetParent(transform, false);
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -24f);
            rect.sizeDelta = new Vector2(200f, 22f);
            energyContainer = container;

            var bgGO = new GameObject("EnergyBackground", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(container.transform, false);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGO.GetComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.35f);

            var fillGO = new GameObject("EnergyFill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(container.transform, false);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            energyFillImage = fillGO.GetComponent<Image>();
            energyFillImage.type = Image.Type.Filled;
            energyFillImage.fillMethod = Image.FillMethod.Horizontal;
            energyFillImage.color = energyReadyColor;

            var textGO = new GameObject("EnergyText", typeof(RectTransform));
            textGO.transform.SetParent(container.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            energyText = textGO.AddComponent<TextMeshProUGUI>();
            energyText.alignment = TextAlignmentOptions.Center;
            energyText.fontSize = 22f;
            energyText.color = Color.white;
            energyText.raycastTarget = false;
        }

        ConfigureEnergyFillImage();

        if (energyContainer != null)
        {
            energyContainer.SetActive(false);
        }
    }

    private void ConfigureEnergyFillImage()
    {
        if (energyFillImage == null)
            return;

        energyFillImage.type = Image.Type.Filled;
        energyFillImage.fillMethod = Image.FillMethod.Horizontal;
        energyFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        energyFillImage.fillAmount = Mathf.Clamp01(currentEnemy != null && currentEnemy.maxEnergy > 0f
            ? currentEnemy.currentEnergy / currentEnemy.maxEnergy
            : 1f);
    }

    private void UpdateEnergyDisplay()
    {
        if (energyContainer == null || currentEnemy == null)
            return;

        bool shouldShow = currentEnemy.usesEnergy && currentEnemy.maxEnergy > 0f;
        energyContainer.SetActive(shouldShow);
        if (!shouldShow)
            return;

        // Force read the latest values directly from the enemy
        float current = currentEnemy.currentEnergy;
        float max = currentEnemy.maxEnergy;
        float percent = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        
        if (energyFillImage != null)
        {
            if (Application.isPlaying)
            {
                LeanTween.cancel(energyFillImage.gameObject);
                float startFill = energyFillImage.fillAmount;
                LeanTween.value(energyFillImage.gameObject, startFill, percent, 0.35f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnUpdate((float value) =>
                    {
                        if (energyFillImage == null) return;
                        energyFillImage.fillAmount = value;
                        energyFillImage.color = value <= 0.05f ? energyDepletedColor : energyReadyColor;
                    });
            }
            else
            {
                energyFillImage.fillAmount = percent;
                energyFillImage.color = percent <= 0.05f ? energyDepletedColor : energyReadyColor;
            }
        }

        if (energyText != null)
        {
            energyText.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
        }
    }

    private void SubscribeToEnemyEnergy()
    {
        if (currentEnemy == null)
            return;

        UnsubscribeFromEnemyEnergy();
        currentEnemy.OnEnergyChanged += HandleEnemyEnergyChanged;
        energySubscribedEnemy = currentEnemy;
        HandleEnemyEnergyChanged(currentEnemy.currentEnergy, currentEnemy.maxEnergy);
    }

    private void UnsubscribeFromEnemyEnergy()
    {
        if (energySubscribedEnemy != null)
        {
            energySubscribedEnemy.OnEnergyChanged -= HandleEnemyEnergyChanged;
            energySubscribedEnemy = null;
        }

        if (energyContainer != null)
        {
            energyContainer.SetActive(false);
        }
    }

    private void HandleEnemyEnergyChanged(float current, float max)
    {
        // Ensure we update the display with the latest values
        if (currentEnemy != null)
        {
            UpdateEnergyDisplay();
        }
    }
    
    private void EnsureGuardUI()
    {
        if (guardContainer == null)
        {
            var existing = transform.Find("GuardContainer");
            if (existing != null)
            {
                guardContainer = existing.gameObject;
                guardFillImage = guardContainer.transform.Find("GuardFill")?.GetComponent<Image>();
                guardText = guardContainer.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (guardContainer == null)
        {
            var container = new GameObject("GuardContainer", typeof(RectTransform));
            container.transform.SetParent(transform, false);
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -48f); // Position below energy bar
            rect.sizeDelta = new Vector2(200f, 22f);
            guardContainer = container;

            var bgGO = new GameObject("GuardBackground", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(container.transform, false);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGO.GetComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.35f);

            var fillGO = new GameObject("GuardFill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(container.transform, false);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            guardFillImage = fillGO.GetComponent<Image>();
            guardFillImage.type = Image.Type.Filled;
            guardFillImage.fillMethod = Image.FillMethod.Horizontal;
            guardFillImage.color = guardColor;

            var textGO = new GameObject("GuardText", typeof(RectTransform));
            textGO.transform.SetParent(container.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            guardText = textGO.AddComponent<TextMeshProUGUI>();
            guardText.alignment = TextAlignmentOptions.Center;
            guardText.fontSize = 22f;
            guardText.color = Color.white;
            guardText.raycastTarget = false;
        }

        ConfigureGuardFillImage();

        if (guardContainer != null)
        {
            guardContainer.SetActive(false);
        }
    }

    private void ConfigureGuardFillImage()
    {
        if (guardFillImage == null)
            return;

        guardFillImage.type = Image.Type.Filled;
        guardFillImage.fillMethod = Image.FillMethod.Horizontal;
        guardFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        guardFillImage.fillAmount = Mathf.Clamp01(currentEnemy != null && currentEnemy.maxGuard > 0f
            ? currentEnemy.currentGuard / currentEnemy.maxGuard
            : 0f);
    }

    public void UpdateGuardDisplay()
    {
        if (guardContainer == null || currentEnemy == null)
            return;

        bool shouldShow = currentEnemy.currentGuard > 0f;
        guardContainer.SetActive(shouldShow);
        if (!shouldShow)
            return;

        // Force read the latest values directly from the enemy
        float current = currentEnemy.currentGuard;
        float max = currentEnemy.maxGuard;
        float percent = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        
        if (guardFillImage != null)
        {
            if (Application.isPlaying)
            {
                LeanTween.cancel(guardFillImage.gameObject);
                float startFill = guardFillImage.fillAmount;
                LeanTween.value(guardFillImage.gameObject, startFill, percent, 0.35f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnUpdate((float value) =>
                    {
                        if (guardFillImage == null) return;
                        guardFillImage.fillAmount = value;
                    });
            }
            else
            {
                guardFillImage.fillAmount = percent;
            }
        }

        if (guardText != null)
        {
            guardText.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
        }
    }

    public void UpdateStackDisplay()
    {
        EnsureStacksUI();
        if (stackIconLookup.Count == 0)
        {
            EnsureStackIcons();
        }

        if (stackIconLookup.Count == 0)
            return;

        if (currentEnemy == null)
        {
            ClearStackDisplay();
            return;
        }

        foreach (StackType type in Enum.GetValues(typeof(StackType)))
        {
            if (!stackIconLookup.TryGetValue(type, out var elements) || elements.root == null)
                continue;

            int stackCount = currentEnemy.GetStacks(type);

            if (elements.value != null)
            {
                elements.value.text = stackCount.ToString();
            }

            UpdateStackIconSprite(type, elements);

            bool shouldShow = hideZeroStacks ? stackCount > 0 : true;
            elements.root.SetActive(shouldShow);
        }
    }

    private void ClearStackDisplay()
    {
        foreach (var kvp in stackIconLookup)
        {
            var elements = kvp.Value;
            if (elements == null) continue;

            if (elements.value != null)
            {
                elements.value.text = "0";
            }

            if (elements.root != null)
            {
                elements.root.SetActive(!hideZeroStacks);
                if (hideZeroStacks)
                {
                    elements.root.SetActive(false);
                }
            }
        }
    }

    private void SubscribeToEnemyStacks()
    {
        if (currentEnemy == null) return;
        if (subscribedEnemyForStacks == currentEnemy) return;

        UnsubscribeFromEnemyStacks();
        currentEnemy.OnStacksChanged += HandleEnemyStacksChanged;
        subscribedEnemyForStacks = currentEnemy;
        UpdateStackDisplay();
    }

    private void UnsubscribeFromEnemyStacks()
    {
        if (subscribedEnemyForStacks != null)
        {
            subscribedEnemyForStacks.OnStacksChanged -= HandleEnemyStacksChanged;
            subscribedEnemyForStacks = null;
        }
    }

    private void HandleEnemyStacksChanged(StackType type, int value)
    {
        UpdateStackDisplay();
    }
    
    /// <summary>
    /// Add a status effect to this enemy
    /// </summary>
    public void AddStatusEffect(StatusEffect effect)
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.AddStatusEffect(effect);
        }
    }

    public void ApplyStackAdjustment(StackAdjustmentDefinition adjustment)
    {
        if (statusEffectManager != null && adjustment != null)
        {
            statusEffectManager.ApplyStackAdjustment(adjustment, true);
            UpdateStackDisplay();
        }
    }
    
    /// <summary>
    /// Remove a status effect from this enemy
    /// </summary>
    public void RemoveStatusEffect(StatusEffectType effectType)
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.RemoveStatusEffect(effectType);
        }
    }
    
    /// <summary>
    /// Check if this enemy has a specific status effect
    /// </summary>
    public bool HasStatusEffect(StatusEffectType effectType)
    {
        if (statusEffectManager != null)
        {
            return statusEffectManager.HasStatusEffect(effectType);
        }
        return false;
    }
    
    // Public methods for external updates
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
    
    public void UpdateIntent()
    {
        if (currentEnemy != null)
        {
            currentEnemy.SetIntent();
            UpdateIntentDisplay();
        }
    }
    
    public void ShowAbilityIntent(string abilityName, int? previewDamage = null)
    {
        showingAbilityIntent = true;
        activeAbilityIntentName = abilityName;
        if (intentContainer != null)
        {
            intentContainer.SetActive(true);
        }
        if (intentText != null)
        {
            intentText.text = abilityName;
        }
        if (intentDamageText != null)
        {
            if (previewDamage.HasValue && previewDamage.Value > 0)
            {
                intentDamageText.text = previewDamage.Value.ToString();
                intentDamageText.color = attackIntentColor;
            }
            else
            {
                intentDamageText.text = string.Empty;
            }
        }
        if (intentIcon != null)
        {
            if (abilityIcon != null)
            {
                intentIcon.sprite = abilityIcon;
                intentIcon.enabled = true;
                intentIcon.color = abilityIntentColor;
            }
            else
            {
                intentIcon.sprite = null;
            }
        }
    }
    
    public void ClearAbilityIntent()
    {
        showingAbilityIntent = false;
        activeAbilityIntentName = null;
        UpdateIntentDisplay();
    }
    
    public void TakeDamage(float damage, bool ignoreGuardArmor = false)
    {
        if (currentEnemy != null)
        {
            // Check for DamageReflection status effect BEFORE taking damage
            var statusManager = GetStatusEffectManager();
            if (statusManager != null && statusManager.HasStatusEffect(StatusEffectType.DamageReflection))
            {
                float reflectionPercent = statusManager.GetTotalMagnitude(StatusEffectType.DamageReflection);
                float reflectedDamage = damage * (reflectionPercent / 100f);
                
                if (reflectedDamage > 0f)
                {
                    Debug.Log($"<color=cyan>[DamageReflection] {currentEnemy.enemyName} reflects {reflectedDamage:F1} damage back ({reflectionPercent}%)!</color>");
                    
                    // Reflect damage back to player
                    if (CharacterManager.Instance != null)
                    {
                        CharacterManager.Instance.TakeDamage(Mathf.RoundToInt(reflectedDamage));
                        
                        // Show floating damage on player
                        var floatingDamageManager = UnityEngine.Object.FindFirstObjectByType<FloatingDamageManager>();
                        var playerDisplay = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
                        if (floatingDamageManager != null && playerDisplay != null)
                        {
                            floatingDamageManager.ShowDamage(reflectedDamage, false, playerDisplay.transform);
                        }
                        
                        var combatUI = UnityEngine.Object.FindFirstObjectByType<AnimatedCombatUI>();
                        if (combatUI != null)
                        {
                            combatUI.LogMessage($"<color=cyan>Reflected!</color> {reflectedDamage:F0} damage returned.");
                        }
                    }
                    
                    // Remove reflection after use (consumed on hit)
                    statusManager.RemoveStatusEffect(StatusEffectType.DamageReflection);
                }
            }
            
            currentEnemy.TakeDamage(damage, ignoreGuardArmor);
            abilityRunner?.OnDamaged();
            UpdateHealthDisplay();
            UpdateStaggerDisplay(); // Update stagger when damage is taken (stagger may have been applied)
            UpdateGuardDisplay(); // Update guard display when damage is taken (guard may have been reduced)
            PlayDamageAnimation();
            if (currentEnemy.currentHealth <= 0)
            {
                NotifyAbilityRunnerDeath();
            }
        }
    }
    
    public void Heal(int amount)
    {
        if (currentEnemy != null)
        {
            currentEnemy.Heal(amount);
            UpdateHealthDisplay();
            PlayHealAnimation();
        }
    }
    
    // Debug methods
    [ContextMenu("Test Damage")]
    public void TestDamage()
    {
        TakeDamage(10);
    }
    
    [ContextMenu("Test Heal")]
    public void TestHeal()
    {
        Heal(5);
    }
    
    [ContextMenu("Update Intent")]
    public void TestUpdateIntent()
    {
        UpdateIntent();
    }
    
    [ContextMenu("Add Poison Effect")]
    public void TestAddPoisonEffect()
    {
        StatusEffect poison = new StatusEffect(StatusEffectType.Poison, "Poison", 5f, 3, true);
        AddStatusEffect(poison);
    }
    
    [ContextMenu("Add Burn Effect")]
    public void TestAddBurnEffect()
    {
        StatusEffect burn = new StatusEffect(StatusEffectType.Burn, "Burn", 3f, 2, true);
        AddStatusEffect(burn);
    }
    
    [ContextMenu("Add Strength Buff")]
    public void TestAddStrengthBuff()
    {
        StatusEffect strength = new StatusEffect(StatusEffectType.Strength, "Strength", 2f, 5, false);
        AddStatusEffect(strength);
    }
    
    [ContextMenu("Clear All Status Effects")]
    public void TestClearAllStatusEffects()
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.ClearAllStatusEffects();
        }
    }
    
    /// <summary>
    /// Get the StatusEffectManager for this enemy
    /// </summary>
    public StatusEffectManager GetStatusEffectManager()
    {
        return statusEffectManager;
    }

    public EnemyAbilityRunner GetAbilityRunner()
    {
        EnsureAbilityRunner();
        return abilityRunner;
    }

    public void NotifyAbilityRunnerDeath()
    {
        if (deathNotified)
            return;

        EnsureAbilityRunner();
        abilityRunner?.OnDeath();
        deathNotified = true;
    }

    private void EnsureAbilityRunner()
    {
        if (abilityRunner == null)
        {
            abilityRunner = GetComponent<EnemyAbilityRunner>();
            if (abilityRunner == null)
            {
                abilityRunner = gameObject.AddComponent<EnemyAbilityRunner>();
            }
        }
    }

    private void InitializeAbilityRunner()
    {
        if (currentEnemy == null && enemyData == null)
            return;

        EnsureAbilityRunner();
        abilityRunner?.Initialize(currentEnemy, enemyData);
    }
    
    [ContextMenu("Create Test Enemy")]
    public void TestCreateEnemy()
    {
        CreateTestEnemy();
    }
    
    [ContextMenu("Validate Enemy Display Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== Enemy Display Validation ===");
        
        // Check UI References
        Debug.Log($"Enemy Portrait: {(enemyPortrait != null ? "✓ Assigned" : "✗ MISSING")}");
        if (enemyPortrait != null)
        {
            Debug.Log($"  - GameObject: {enemyPortrait.gameObject.name}");
            Debug.Log($"  - Active: {enemyPortrait.gameObject.activeInHierarchy}");
            Debug.Log($"  - Current Sprite: {(enemyPortrait.sprite != null ? enemyPortrait.sprite.name : "none")}");
        }
        
        Debug.Log($"Enemy Name Text: {(enemyNameText != null ? "✓ Assigned" : "⚠ Missing (auto-find will attempt)")}");
        Debug.Log($"Enemy Type Text: {(enemyTypeText != null ? "✓ Assigned" : "⚠ Missing (auto-find will attempt)")}");
        Debug.Log($"Health Slider: {(healthSlider != null ? "✓ Assigned" : "⚠ Missing (auto-find will attempt)")}");
        
        // Check Animator
        if (enemyAnimator != null)
        {
            Debug.Log($"✓ Animator: Found on '{enemyAnimator.gameObject.name}'");
            Debug.Log($"  - Enabled: {enemyAnimator.enabled}");
            Debug.Log($"  - Culling Mode: {enemyAnimator.cullingMode}");
            if (enemyAnimator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
            {
                Debug.LogWarning($"  ⚠ Culling Mode should be 'AlwaysAnimate' for UI! Currently: {enemyAnimator.cullingMode}");
            }
            
            if (enemyAnimator.runtimeAnimatorController != null)
            {
                Debug.Log($"  ✓ Animator Controller: {enemyAnimator.runtimeAnimatorController.name}");
                Debug.Log($"  - Parameter Count: {enemyAnimator.parameterCount}");
                
                // List parameters
                foreach (var param in enemyAnimator.parameters)
                {
                    Debug.Log($"    • {param.name} ({param.type})");
                }
            }
            else
            {
                Debug.LogWarning($"  ⚠ Animator has NO controller assigned (will be set from EnemyData at runtime)");
            }
        }
        else
        {
            Debug.LogWarning($"⚠ NO Animator component found! Add Animator to:");
            Debug.LogWarning($"   - The Enemy Portrait Image GameObject, OR");
            Debug.LogWarning($"   - A parent/child/sibling of the Enemy Portrait");
        }
        
        // Check EnemyData
        if (enemyData != null)
        {
            Debug.Log($"✓ Enemy Data: {enemyData.enemyName}");
            Debug.Log($"  - Sprite: {(enemyData.enemySprite != null ? "✓ " + enemyData.enemySprite.name : "✗ MISSING")}");
            Debug.Log($"  - Display Scale: {enemyData.displayScale}");
            Debug.Log($"  - Panel Height: {enemyData.basePanelHeight}");
            Debug.Log($"  - Animator Controller: {(enemyData.animatorController != null ? "✓ " + enemyData.animatorController.name : "✗ NOT ASSIGNED - ANIMATIONS WILL NOT WORK!")}");
        }
        else
        {
            Debug.LogWarning($"⚠ No EnemyData assigned (normal if not yet initialized)");
        }
        
        // Check Current Enemy
        if (currentEnemy != null)
        {
            Debug.Log($"✓ Current Enemy: {currentEnemy.enemyName} ({currentEnemy.currentHealth}/{currentEnemy.maxHealth} HP)");
        }
        else
        {
            Debug.LogWarning($"⚠ No current enemy set (normal if not yet initialized)");
        }
        
        Debug.Log("=== Validation Complete ===");
    }
    
    /// <summary>
    /// Gets the area level for enemy scaling (from EncounterManager or maze context).
    /// </summary>
    private int GetAreaLevel()
    {
        // Check if this is maze combat
        string mazeContext = PlayerPrefs.GetString("MazeCombatContext", "");
        bool isMazeCombat = !string.IsNullOrEmpty(mazeContext);
        
        if (isMazeCombat && Dexiled.MazeSystem.MazeRunManager.Instance != null)
        {
            var run = Dexiled.MazeSystem.MazeRunManager.Instance.GetCurrentRun();
            if (run != null)
            {
                // Use floor number as area level for maze combat
                return run.currentFloor;
            }
        }
        
        // Check EncounterManager for regular encounters
        if (EncounterManager.Instance != null)
        {
            var encounter = EncounterManager.Instance.GetCurrentEncounter();
            if (encounter != null)
            {
                return Mathf.Max(1, encounter.areaLevel);
            }
        }
        
        // Default fallback
        return 1;
    }
    
    [ContextMenu("Test Attack Animation NOW")]
    public void TestAttackAnimationNow()
    {
        Debug.Log("=== Testing Attack Animation ===");
        
        if (enemyAnimator == null)
        {
            Debug.LogError("✗ Cannot test - No Animator component found!");
            return;
        }
        
        if (enemyAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("✗ Cannot test - No Animator Controller assigned!");
            return;
        }
        
        Debug.Log($"Playing attack animation on {enemyAnimator.gameObject.name}...");
        PlayAttackAnimation();
        Debug.Log("=== Test Complete - Check if sprite changed ===");
    }
    
    // Animation methods for visual feedback
    public void PlayDamageAnimation()
    {
        // Flash red when taking damage
        StartCoroutine(FlashColor(Color.red, 0.2f));
        
        // Trigger hit animation if animator is set up
        PlayHitAnimation();
    }
    
    public void PlayHealAnimation()
    {
        // Flash green when healing
        StartCoroutine(FlashColor(Color.green, 0.2f));
    }
    
    /// <summary>
    /// Play the attack animation (called when enemy attacks)
    /// </summary>
    public void PlayAttackAnimation()
    {
        Debug.Log($"[PlayAttackAnimation] Called for {currentEnemy?.enemyName}");
        
        if (enemyAnimator == null)
        {
            Debug.LogWarning($"{currentEnemy?.enemyName}: enemyAnimator is NULL - cannot play attack animation");
            return;
        }
        
        if (enemyAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{currentEnemy?.enemyName}: enemyAnimator.runtimeAnimatorController is NULL - animator controller not assigned. Check EnemyData.animatorController field.");
            return;
        }
        
        // Check if Attack parameter exists
        bool hasAttackTrigger = false;
        foreach (var param in enemyAnimator.parameters)
        {
            if (param.name == "Attack" && param.type == AnimatorControllerParameterType.Trigger)
            {
                hasAttackTrigger = true;
                break;
            }
        }
        
        if (!hasAttackTrigger)
        {
            Debug.LogError($"{currentEnemy?.enemyName}: Animator Controller '{enemyAnimator.runtimeAnimatorController.name}' does NOT have an 'Attack' Trigger parameter! Add it in the Animator Controller.");
            return;
        }
        
        enemyAnimator.SetTrigger("Attack");
        Debug.Log($"✓ {currentEnemy?.enemyName} playing attack animation with controller: {enemyAnimator.runtimeAnimatorController.name}");
        
        // Additional debugging
        var currentState = enemyAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"Current state: {currentState.shortNameHash} (IsName: {enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")})");
        
        // Check if animator is actually playing
        StartCoroutine(CheckAnimationState());
    }
    
    /// <summary>
    /// Play the hit animation (called when enemy takes damage)
    /// </summary>
    public void PlayHitAnimation()
    {
        if (enemyAnimator != null && enemyAnimator.runtimeAnimatorController != null)
        {
            // Check if Hit trigger exists
            foreach (var param in enemyAnimator.parameters)
            {
                if (param.name == "Hit" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    enemyAnimator.SetTrigger("Hit");
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// Play the death animation (called when enemy dies)
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (enemyAnimator != null && enemyAnimator.runtimeAnimatorController != null)
        {
            // Check if IsDead bool exists
            foreach (var param in enemyAnimator.parameters)
            {
                if (param.name == "IsDead" && param.type == AnimatorControllerParameterType.Bool)
                {
                    enemyAnimator.SetBool("IsDead", true);
                    return;
                }
            }
        }
    }
    
    private System.Collections.IEnumerator FlashColor(Color flashColor, float duration)
    {
        if (enemyPortrait != null)
        {
            Color originalColor = enemyPortrait.color;
            enemyPortrait.color = flashColor;
            
            yield return new WaitForSeconds(duration);
            
            enemyPortrait.color = originalColor;
        }
    }
    
    private System.Collections.IEnumerator RestorePositionAfterFrame(Vector3 originalPosition)
    {
        yield return null; // Wait one frame
        
        if (enemyPortrait != null)
        {
            enemyPortrait.rectTransform.anchoredPosition = originalPosition;
            Debug.Log($"[Visibility Test] Restored portrait position: {originalPosition}");
        }
    }

    public void StartDeathFadeOut(Action onComplete)
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, cg.alpha, 0f, 0.35f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float a) => { if (cg != null) cg.alpha = a; })
            .setOnComplete(() => {
                onComplete?.Invoke();
                gameObject.SetActive(false);
            });
    }
    
    // Getter for current enemy
    public Enemy GetCurrentEnemy()
    {
        return currentEnemy;
    }
    
    // Check if enemy is alive
    public bool IsAlive()
    {
        return currentEnemy != null && currentEnemy.currentHealth > 0;
    }
    
    /// <summary>
    /// Check animation state for debugging
    /// </summary>
    private System.Collections.IEnumerator CheckAnimationState()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (enemyAnimator != null)
        {
            var stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Animation State Check - IsPlaying: {enemyAnimator.IsInTransition(0)}, State: {stateInfo.shortNameHash}, NormalizedTime: {stateInfo.normalizedTime}");
            
            // Check if we're in Attack state
            if (enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                Debug.Log("✓ Successfully in Attack state!");
            }
            else
            {
                Debug.LogWarning("⚠ Not in Attack state - transition may have failed");
            }
        }
    }
    
    /// <summary>
    /// Apply display scale from EnemyData to the portrait sprite
    /// </summary>
    private void ApplyDisplayScale()
    {
        if (enemyData == null || enemyPortrait == null) return;
        
        RectTransform portraitRect = enemyPortrait.GetComponent<RectTransform>();
        if (portraitRect == null) return;
        
        // Apply scale to the local scale (multiplicative with any existing scale)
        // This preserves any manually set scale while applying the data-driven scale
        Vector3 baseScale = Vector3.one;
        portraitRect.localScale = baseScale * enemyData.displayScale;
        
        Debug.Log($"V Applied display scale {enemyData.displayScale} to {enemyData.enemyName} portrait");
    }
    
    private void CacheBaseAnchoredPositions()
    {
        if (cachedBaseAnchoredPositions)
            return;
        
        if (healthSlider != null)
        {
            RectTransform rect = healthSlider.GetComponent<RectTransform>();
            if (rect != null)
            {
                baseHealthAnchoredPos = rect.anchoredPosition;
            }
        }
        
        if (intentContainer != null)
        {
            RectTransform rect = intentContainer.GetComponent<RectTransform>();
            if (rect != null)
            {
                baseIntentAnchoredPos = rect.anchoredPosition;
            }
        }
        
        if (enemyNameText != null)
        {
            RectTransform rect = enemyNameText.GetComponent<RectTransform>();
            if (rect != null)
            {
                baseNameAnchoredPos = rect.anchoredPosition;
            }
        }
        
        cachedBaseAnchoredPositions = true;
    }
    
    private void EnsureUILayering()
    {
        int nextIndex = 0;
        
        if (enemyPortrait != null)
        {
            enemyPortrait.transform.SetSiblingIndex(nextIndex++);
        }
        
        if (healthSlider != null)
        {
            healthSlider.transform.SetSiblingIndex(nextIndex++);
        }
        
        if (intentContainer != null)
        {
            intentContainer.transform.SetSiblingIndex(nextIndex++);
        }
        
        if (statusEffectsContainer != null)
        {
            statusEffectsContainer.SetSiblingIndex(nextIndex++);
        }
        
        if (enemyNameText != null)
        {
            enemyNameText.transform.SetSiblingIndex(transform.childCount - 1);
        }
    }
    
    /// <summary>
    /// Adjust UI element positions to create a proper stacked layout:
    /// Name -> Portrait -> Health -> Intent
    /// This ensures elements are properly spaced below the scaled portrait
    /// </summary>
    private void AdjustUILayoutForPortraitScale()
    {
        CacheBaseAnchoredPositions();

        RectTransform portraitRect = enemyPortrait != null ? enemyPortrait.GetComponent<RectTransform>() : null;
        if (portraitRect == null)
            return;

        float scale = enemyData != null ? Mathf.Max(0.25f, enemyData.displayScale) : 1f;
        float extraHeight = (scale - 1f) * portraitRect.rect.height * portraitRect.pivot.y;

        if (healthSlider != null)
        {
            RectTransform healthRect = healthSlider.GetComponent<RectTransform>();
            if (healthRect != null)
            {
                healthRect.anchoredPosition = baseHealthAnchoredPos - new Vector2(0f, extraHeight);
            }
        }

        if (intentContainer != null)
        {
            RectTransform intentRect = intentContainer.GetComponent<RectTransform>();
            if (intentRect != null)
            {
                intentRect.anchoredPosition = baseIntentAnchoredPos - new Vector2(0f, extraHeight);
            }
        }

        if (enemyNameText != null)
        {
            RectTransform nameRect = enemyNameText.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                nameRect.anchoredPosition = baseNameAnchoredPos + new Vector2(0f, extraHeight);
            }
        }
    }
    
    /// <summary>
    /// Set up the animator controller dynamically based on enemy data
    /// </summary>
    private void SetupEnemyAnimator()
    {
        if (enemyData == null)
        {
            Debug.LogWarning($"SetupEnemyAnimator: enemyData is NULL. Cannot setup animator.");
            return;
        }
        
        // Check if this enemy has an animator controller assigned
        if (enemyData.animatorController != null)
        {
            // Enemy has animations - setup animator
            if (enemyAnimator == null)
            {
                Debug.LogWarning($"[{enemyData.enemyName}] Has animator controller but no Animator component found. Using static sprite instead.");
                // Use static sprite fallback
                UseStaticSprite();
                return;
            }
            
            // Assign animator controller
            if (enemyAnimator.runtimeAnimatorController != enemyData.animatorController)
            {
                enemyAnimatorController = enemyData.animatorController;
                enemyAnimator.runtimeAnimatorController = enemyData.animatorController;
                enemyAnimator.enabled = true;
                Debug.Log($"✓ Set animator controller for {enemyData.enemyName}: {enemyData.animatorController.name}");
                
                // Ensure sprite is set after animator controller assignment
                if (enemyPortrait != null && enemyData.enemySprite != null)
                {
                    enemyPortrait.sprite = enemyData.enemySprite;
                    enemyPortrait.enabled = true;
                    Debug.Log($"✓ Re-assigned sprite for {enemyData.enemyName}: {enemyData.enemySprite.name}");
                }
            }
            else
            {
                Debug.Log($"✓ Animator controller already set for {enemyData.enemyName}: {enemyData.animatorController.name}");
            }
        }
        else
        {
            // No animator controller - use static sprite
            Debug.Log($"✓ {enemyData.enemyName} has no animator controller. Using static sprite.");
            
            // Disable animator if it exists to prevent interference
            if (enemyAnimator != null)
            {
                enemyAnimator.enabled = false;
                Debug.Log($"✓ Disabled animator for {enemyData.enemyName} (static sprite mode)");
            }
            
            // Ensure static sprite is displayed
            UseStaticSprite();
        }
    }
    
    /// <summary>
    /// Display static sprite (for enemies without animations)
    /// </summary>
    private void UseStaticSprite()
    {
        if (enemyData != null && enemyPortrait != null && enemyData.enemySprite != null)
        {
            enemyPortrait.sprite = enemyData.enemySprite;
            enemyPortrait.enabled = true;
            enemyPortrait.color = Color.white;
            Debug.Log($"✓ Using static sprite for {enemyData.enemyName}: {enemyData.enemySprite.name}");
        }
        else
        {
            Debug.LogWarning($"⚠ Cannot display static sprite for {enemyData?.enemyName ?? "unknown"}: sprite is null");
        }
    }

    /// <summary>
    /// Gets the color for enemy name based on rarity and tier.
    /// Common (Normal) - White, Magic - Blue, Rare - Yellow, Unique/Boss/Mini-boss - Orange
    /// </summary>
    public Color GetEnemyNameColor(EnemyRarity rarity, EnemyTier tier)
    {
        // Boss and Mini-boss always use Orange
        if (tier == EnemyTier.Boss || tier == EnemyTier.Miniboss)
        {
            return new Color(1f, 0.65f, 0f); // Orange
        }
        
        // Otherwise use rarity-based colors
        switch (rarity)
        {
            case EnemyRarity.Normal:
                return Color.white;
            case EnemyRarity.Magic:
                return new Color(0.3f, 0.6f, 1f); // Blue
            case EnemyRarity.Rare:
                return new Color(1f, 0.9f, 0.2f); // Yellow
            case EnemyRarity.Unique:
                return new Color(1f, 0.65f, 0f); // Orange
            default:
                return Color.white;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEnemyStacks();
        UnsubscribeFromEnemyEnergy();
    }
}
