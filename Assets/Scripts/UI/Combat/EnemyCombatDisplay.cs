using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
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
    [SerializeField] private bool enableIntentSlide = false;
    [SerializeField] private float intentSlideDistance = 12f;
    [SerializeField] private float intentSlideDuration = 0.12f;
    [SerializeField] private bool enableIntentSlotScroll = true;
    [SerializeField] private float intentSlotScrollDuration = 0.12f;
    [SerializeField] private int intentQueueSlotCount = 3;
    [SerializeField] private float intentSlotSpacing = 18f;
    
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
    public Color threatLabelColor = new Color(1f, 0.85f, 0.45f, 1f);
    public bool showThreatTextFallback = true;
    
    [Header("Intent Icons")]
    public Sprite attackIcon;
    public Sprite defendIcon;
    public Sprite abilityIcon;

    [Header("Enemy Threat Icons")]
    public Image enemyThreatIconPrimary;
    public Image enemyThreatIconSecondary;
    
    [Header("Animation")]
    public Animator enemyAnimator; // Animator for enemy sprite animations
    private RuntimeAnimatorController enemyAnimatorController; // Store the enemy's specific animator controller
    
    private Enemy currentEnemy;
    private EnemyData enemyData; // Reference to the data used to create this enemy
    private EnemyAbilityRunner abilityRunner;
    private bool deathNotified = false;
    private bool showingAbilityIntent = false;
    private string activeAbilityIntentName = null;
    private int? abilityPreviewDamage = null;
    private string lastIntentHeadKey = null;
    private Coroutine intentSlideRoutine;
    private Coroutine intentScrollRoutine;
    private bool isAnimatingQueue = false;
    
    private Vector2 baseHealthAnchoredPos;
    private Vector2 baseIntentAnchoredPos;
    private Vector2 baseNameAnchoredPos;
    private bool cachedBaseAnchoredPositions = false;
    private bool isInitialized = false;
    private Enemy subscribedEnemyForStacks;
    private Enemy energySubscribedEnemy;
    private Enemy intentChangedSubscribedEnemy;
    private class IntentSlot
    {
        public GameObject root;
        public Image icon;
        public TextMeshProUGUI text;
        public TextMeshProUGUI value;
        public TextMeshProUGUI threatText;
        public Image threatIconPrimary;
        public Image threatIconSecondary;
        public Image abilityBoundBadge;
    }

    private readonly List<IntentSlot> intentSlots = new List<IntentSlot>(3);
    private readonly List<Vector2> intentSlotBasePositions = new List<Vector2>(3);
    private bool intentSlotPositionsCached = false;
    private StatusEffectManager statusSubscribedManager;
    
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
        SubscribeToStatusEffects();
        
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
        EnsureEnemyThreatIcons();
        
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
        EnsureIntentQueueSlots();
        
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
        
        // Setup clickable area for targeting
        // Make the visual elements (portrait, health bar, etc.) clickable instead of just the root GameObject
        SetupClickableArea();
        
        // DON'T auto-create test enemy - let CombatDisplayManager assign real enemies
        // If you want a placeholder, CombatDisplayManager will handle it
    }

    private void EnsureEnemyThreatIcons()
    {
        if (enemyThreatIconPrimary == null)
        {
            enemyThreatIconPrimary = FindThreatIconByName("EnemyThreatIconPrimary", "ThreatIconPrimary", "ThreatIcon1");
        }
        if (enemyThreatIconSecondary == null)
        {
            enemyThreatIconSecondary = FindThreatIconByName("EnemyThreatIconSecondary", "ThreatIconSecondary", "ThreatIcon2");
        }
    }

    private Image FindThreatIconByName(params string[] names)
    {
        foreach (var name in names)
        {
            Transform t = transform.Find(name);
            if (t != null)
                return t.GetComponent<Image>();
        }
        return null;
    }
    
    /// <summary>
    /// Setup clickable areas on visual elements (portrait, health bar, etc.) for accurate targeting
    /// </summary>
    private void SetupClickableArea()
    {
        // Remove old button from root if it exists (we'll use visual elements instead)
        UnityEngine.UI.Button rootButton = GetComponent<UnityEngine.UI.Button>();
        if (rootButton != null)
        {
            DestroyImmediate(rootButton);
        }
        
        // Helper method to add click handler to a UI element
        System.Action<GameObject> addClickHandler = (GameObject targetObj) =>
        {
            if (targetObj == null) return;
            
            // Add Image component if missing (needed for raycast detection)
            Image img = targetObj.GetComponent<Image>();
            if (img == null)
            {
                img = targetObj.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f); // Transparent - just for raycast
            }
            img.raycastTarget = true;
            
            // Add Button component for click handling
            UnityEngine.UI.Button btn = targetObj.GetComponent<UnityEngine.UI.Button>();
            if (btn == null)
            {
                btn = targetObj.AddComponent<UnityEngine.UI.Button>();
                btn.transition = UnityEngine.UI.Selectable.Transition.None;
            }
            
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                HandleEnemyClick();
            });
        };
        
        // Make enemy portrait clickable (main visual element)
        if (enemyPortrait != null)
        {
            enemyPortrait.raycastTarget = true;
            addClickHandler(enemyPortrait.gameObject);
        }
        
        // Make health slider clickable
        if (healthSlider != null)
        {
            addClickHandler(healthSlider.gameObject);
        }
        
        // Make health fill image clickable
        if (healthFillImage != null)
        {
            healthFillImage.raycastTarget = true;
            addClickHandler(healthFillImage.gameObject);
        }
        
        // Make enemy name text clickable (if it has a background)
        if (enemyNameText != null)
        {
            // TextMeshProUGUI can be clickable if it has raycastTarget enabled
            enemyNameText.raycastTarget = true;
            
            // Add a transparent Image behind the text for better click detection
            Image textBg = enemyNameText.GetComponent<Image>();
            if (textBg == null)
            {
                textBg = enemyNameText.gameObject.AddComponent<Image>();
                textBg.color = new Color(1f, 1f, 1f, 0f); // Transparent
            }
            textBg.raycastTarget = true;
            addClickHandler(enemyNameText.gameObject);
        }
        
        // Make intent container clickable
        if (intentContainer != null)
        {
            addClickHandler(intentContainer);
        }
        
        // If we have a RectTransform on the root, ensure it has an Image for fallback clicking
        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect != null)
        {
            Image rootImage = GetComponent<Image>();
            if (rootImage == null)
            {
                rootImage = gameObject.AddComponent<Image>();
                rootImage.color = new Color(1f, 1f, 1f, 0f); // Transparent - just for raycast
            }
            rootImage.raycastTarget = true;
            
            // Add click handler to root as fallback
            UnityEngine.UI.Button rootBtn = GetComponent<UnityEngine.UI.Button>();
            if (rootBtn == null)
            {
                rootBtn = gameObject.AddComponent<UnityEngine.UI.Button>();
                rootBtn.transition = UnityEngine.UI.Selectable.Transition.None;
            }
            rootBtn.onClick.RemoveAllListeners();
            rootBtn.onClick.AddListener(() => {
                HandleEnemyClick();
            });
        }
    }
    
    /// <summary>
    /// Handle click on this enemy display
    /// </summary>
    private void HandleEnemyClick()
    {
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
        UnsubscribeFromIntentChanged();
        currentEnemy = enemy;
        enemyData = data;
        deathNotified = false;
        InitializeAbilityRunner();
        SubscribeToEnemyStacks();
        SubscribeToEnemyEnergy();
        SubscribeToIntentChanged();
        
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
        UnsubscribeFromIntentChanged();
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
        UnsubscribeFromIntentChanged();
        enemyData = data;
        
        // Get area level for scaling (from EncounterManager or maze context)
        int areaLevel = GetAreaLevel();
        
        // Create enemy with area level scaling
        currentEnemy = data.CreateEnemy(areaLevel);
        deathNotified = false;
        InitializeAbilityRunner();
        SubscribeToEnemyStacks();
        SubscribeToEnemyEnergy();
        SubscribeToIntentChanged();
        
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
            // Keep raycastTarget enabled so clicks work on the portrait
            enemyPortrait.raycastTarget = true;
            
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
        UpdateEnemyThreatIcons();

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

        EnsureIntentQueueSlots();
        
        if (isAnimatingQueue)
            return;

        // Check for crowd control status effects (Frozen, Stunned) that prevent actions
        bool isFrozen = statusEffectManager != null && statusEffectManager.HasStatusEffect(StatusEffectType.Freeze);
        bool isStunned = statusEffectManager != null && statusEffectManager.HasStatusEffect(StatusEffectType.Stun);
        
        // Display crowd control status instead of normal intent
        if (isFrozen)
        {
            if (intentSlots.Count > 0 && intentSlots[0]?.text != null)
            {
                intentSlots[0].text.text = "FROZEN!";
                intentSlots[0].text.color = new Color(0.6f, 0.9f, 1f, 1f); // Light blue
            }
            ClearIntentSlotsFromIndex(1);
            SetAbilityIntentVisible(false);
            return;
        }
        
        if (isStunned)
        {
            if (intentSlots.Count > 0 && intentSlots[0]?.text != null)
            {
                intentSlots[0].text.text = "STAGGERED!";
                intentSlots[0].text.color = new Color(1f, 1f, 0f, 1f);
            }
            ClearIntentSlotsFromIndex(1);
            SetAbilityIntentVisible(false);
            return;
        }
        
        if (showingAbilityIntent)
        {
            SetAbilityIntentVisible(true);
            return;
        }

        SetAbilityIntentVisible(false);

        string headKey = GetIntentHeadKey();
        bool headChanged = !string.IsNullOrEmpty(headKey) && headKey != lastIntentHeadKey;

        if (headChanged && enableIntentSlotScroll && !isAnimatingQueue && intentSlots.Count > 1)
        {
            StartQueueScrollAnimation();
            lastIntentHeadKey = headKey;
            return;
        }

        UpdateIntentQueueSlots();
        if (headChanged && enableIntentSlide)
            PlayIntentSlide();

        if (headChanged)
            lastIntentHeadKey = headKey;
    }

    private string GetIntentHeadKey()
    {
        if (currentEnemy == null || currentEnemy.intentQueue == null || currentEnemy.intentQueue.IsEmpty)
            return null;

        var head = currentEnemy.intentQueue.Peek();
        if (!head.HasValue)
            return null;

        if (head.Value.IsAbility)
            return $"{head.Value.AbilityId}:{head.Value.AbilityValue}";

        return $"{head.Value.Type}:{head.Value.Damage}";
    }

    private void PlayIntentSlide()
    {
        if (intentContainer == null) return;
        CacheBaseAnchoredPositions();

        RectTransform rect = intentContainer.GetComponent<RectTransform>();
        if (rect == null) return;

        if (intentSlideRoutine != null)
            StopCoroutine(intentSlideRoutine);

        intentSlideRoutine = StartCoroutine(AnimateIntentSlide(rect));
    }

    private IEnumerator AnimateIntentSlide(RectTransform rect)
    {
        Vector2 start = baseIntentAnchoredPos - new Vector2(0f, intentSlideDistance);
        Vector2 end = baseIntentAnchoredPos;
        float elapsed = 0f;

        rect.anchoredPosition = start;
        while (elapsed < intentSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / intentSlideDuration);
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
        rect.anchoredPosition = end;
        intentSlideRoutine = null;
    }

    private void StartQueueScrollAnimation()
    {
        if (!enableIntentSlotScroll || intentSlots.Count == 0)
            return;

        CacheIntentSlotPositions();
        if (intentScrollRoutine != null)
            StopCoroutine(intentScrollRoutine);

        intentScrollRoutine = StartCoroutine(AnimateIntentQueueScroll());
    }

    private IEnumerator AnimateIntentQueueScroll()
    {
        isAnimatingQueue = true;

        Vector2 delta = intentSlotBasePositions.Count > 1
            ? intentSlotBasePositions[0] - intentSlotBasePositions[1]
            : new Vector2(0f, intentSlotSpacing);

        float elapsed = 0f;
        while (elapsed < intentSlotScrollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / intentSlotScrollDuration);

            for (int i = 0; i < intentSlots.Count; i++)
            {
                IntentSlot slot = intentSlots[i];
                RectTransform rect = slot?.root != null ? slot.root.GetComponent<RectTransform>() : null;
                if (rect == null) continue;

                Vector2 start = intentSlotBasePositions[i];
                Vector2 target = i == 0 ? start + delta : intentSlotBasePositions[i - 1];
                rect.anchoredPosition = Vector2.Lerp(start, target, t);

                float targetAlpha = i == 0 ? 0f : (i == 1 ? 1f : 0.6f);
                float currentAlpha = Mathf.Lerp(1f, targetAlpha, t);
                SetSlotAlpha(slot, currentAlpha);
            }

            yield return null;
        }

        RestoreIntentSlotPositions();
        UpdateIntentQueueSlots();

        isAnimatingQueue = false;
        intentScrollRoutine = null;
    }

    private void EnsureIntentQueueSlots()
    {
        if (intentSlots.Count > 0)
            return;

        Transform root = intentContainer != null ? intentContainer.transform : transform;
        for (int i = 1; i <= Mathf.Max(1, intentQueueSlotCount); i++)
        {
            string parentName = $"Intent{i}";
            Transform parent = root.Find(parentName);
            if (parent == null)
                parent = transform.Find(parentName);

            if (parent == null)
                continue;

            IntentSlot slot = new IntentSlot
            {
                root = parent.gameObject,
                icon = parent.Find($"Intent{i}Icon")?.GetComponent<Image>(),
                text = parent.Find($"Intent{i}Text")?.GetComponent<TextMeshProUGUI>(),
                value = parent.Find($"Intent{i}Value")?.GetComponent<TextMeshProUGUI>(),
                threatText = parent.Find($"Intent{i}Threat")?.GetComponent<TextMeshProUGUI>()
                             ?? parent.Find($"Intent{i}ThreatText")?.GetComponent<TextMeshProUGUI>(),
                threatIconPrimary = FindThreatIcon(parent, i, true),
                threatIconSecondary = FindThreatIcon(parent, i, false),
                abilityBoundBadge = FindAbilityBoundBadge(parent, i)
            };
            intentSlots.Add(slot);
        }

        if (intentSlots.Count > 0)
        {
            if (intentText != null)
                intentText.gameObject.SetActive(false);
            if (intentDamageText != null)
                intentDamageText.gameObject.SetActive(false);
            if (intentIcon != null && intentIcon.gameObject != null)
                intentIcon.gameObject.SetActive(false);

            CacheIntentSlotPositions();
            for (int i = 0; i < intentSlots.Count; i++)
            {
                SetSlotActive(intentSlots[i], true);
            }
        }

        // Fallback to legacy single intent elements if no slots were found.
        if (intentSlots.Count == 0 && intentText != null)
        {
            intentSlots.Add(new IntentSlot
            {
                root = intentText.transform.parent != null ? intentText.transform.parent.gameObject : null,
                icon = intentIcon,
                text = intentText,
                value = intentDamageText
            });
        }
    }

    private void CacheIntentSlotPositions()
    {
        if (intentSlotPositionsCached || intentSlots.Count == 0)
            return;

        intentSlotBasePositions.Clear();
        for (int i = 0; i < intentSlots.Count; i++)
        {
            RectTransform rect = intentSlots[i]?.root != null ? intentSlots[i].root.GetComponent<RectTransform>() : null;
            intentSlotBasePositions.Add(rect != null ? rect.anchoredPosition : Vector2.zero);
        }

        intentSlotPositionsCached = true;
    }

    private void RestoreIntentSlotPositions()
    {
        if (!intentSlotPositionsCached) return;
        for (int i = 0; i < intentSlots.Count; i++)
        {
            RectTransform rect = intentSlots[i]?.root != null ? intentSlots[i].root.GetComponent<RectTransform>() : null;
            if (rect != null && i < intentSlotBasePositions.Count)
                rect.anchoredPosition = intentSlotBasePositions[i];
        }
    }

    private void UpdateIntentQueueSlots()
    {
        if (currentEnemy == null || intentSlots.Count == 0)
            return;

        var entries = currentEnemy.intentQueue != null ? currentEnemy.intentQueue.All : null;
        int count = entries != null ? Mathf.Min(entries.Count, intentSlots.Count) : 0;

        for (int i = 0; i < intentSlots.Count; i++)
        {
            IntentSlot slot = intentSlots[i];
            if (slot == null) continue;

            if (i < count)
            {
                SetSlotFromEntry(slot, entries[i], i);
            }
            else
            {
                ClearSlot(slot);
            }
        }
    }

    private void SetSlotFromEntry(IntentSlot slot, EnemyIntentEntry entry, int index)
    {
        float alpha = index == 0 ? 1f : (index == 1 ? 0.6f : 0.3f);
        string label = GetIntentLabel(entry);
        string value = GetIntentValue(entry, currentEnemy);
        Sprite icon = GetIntentIcon(entry, out Color iconColor);
        Color textColor = GetIntentTextColor(entry);

        SetSlotActive(slot, true);

        if (slot.text != null)
        {
            slot.text.text = label;
            slot.text.color = textColor;
            SetAlpha(slot.text, alpha);
        }

        if (slot.value != null)
        {
            slot.value.text = value;
            slot.value.color = textColor;
            SetAlpha(slot.value, alpha);
        }

        if (slot.icon != null)
        {
            slot.icon.sprite = icon;
            slot.icon.color = iconColor;
            SetAlpha(slot.icon, alpha);
            slot.icon.enabled = icon != null;
        }

        if (slot.threatText != null)
        {
            slot.threatText.text = showThreatTextFallback ? GetThreatLabel(entry) : string.Empty;
            slot.threatText.color = threatLabelColor;
            SetAlpha(slot.threatText, alpha);
        }

        if (slot.threatIconPrimary != null)
        {
            SetThreatIcon(slot.threatIconPrimary, GetDisplayThreat(entry.PrimaryThreat, entry.IsAbility), alpha);
        }
        if (slot.threatIconSecondary != null)
        {
            SetThreatIcon(slot.threatIconSecondary, GetDisplayThreat(entry.SecondaryThreat, entry.IsAbility), alpha);
        }

        if (slot.abilityBoundBadge != null)
        {
            SetAbilityBoundBadge(slot.abilityBoundBadge, entry, alpha);
        }
    }

    private void ClearSlot(IntentSlot slot)
    {
        SetSlotActive(slot, true);
        if (slot.text != null) slot.text.text = string.Empty;
        if (slot.value != null) slot.value.text = string.Empty;
        if (slot.threatText != null) slot.threatText.text = string.Empty;
        if (slot.threatIconPrimary != null)
        {
            slot.threatIconPrimary.sprite = null;
            slot.threatIconPrimary.enabled = false;
        }
        if (slot.threatIconSecondary != null)
        {
            slot.threatIconSecondary.sprite = null;
            slot.threatIconSecondary.enabled = false;
        }
        if (slot.abilityBoundBadge != null)
        {
            slot.abilityBoundBadge.sprite = null;
            slot.abilityBoundBadge.enabled = false;
        }
        if (slot.icon != null)
        {
            slot.icon.sprite = null;
            slot.icon.enabled = false;
        }
    }

    private void ClearIntentSlotsFromIndex(int startIndex)
    {
        for (int i = startIndex; i < intentSlots.Count; i++)
        {
            ClearSlot(intentSlots[i]);
        }
    }

    private string GetIntentLabel(EnemyIntentEntry entry)
    {
        string prefix = entry.IsCharged ? "Charging " : string.Empty;
        if (entry.IsAbility)
        {
            string name = string.IsNullOrEmpty(entry.AbilityName) ? "Ability" : entry.AbilityName;
            return $"{prefix}{name}";
        }

        switch (entry.Type)
        {
            case EnemyIntent.Attack:
                return $"{prefix}Attack";
            case EnemyIntent.Defend:
                return "Defend";
            default:
                return "Intent";
        }
    }

    private string GetIntentValue(EnemyIntentEntry entry, Enemy enemy)
    {
        if (entry.IsAbility)
            return entry.AbilityValue > 0 ? entry.AbilityValue.ToString() : string.Empty;

        switch (entry.Type)
        {
            case EnemyIntent.Attack:
                return entry.Damage > 0 ? entry.Damage.ToString() : string.Empty;
            case EnemyIntent.Defend:
                if (enemy != null)
                {
                    int guardAmount = Mathf.RoundToInt(enemy.maxHealth * enemy.defendGuardPercent);
                    return guardAmount > 0 ? guardAmount.ToString() : string.Empty;
                }
                return string.Empty;
            default:
                return string.Empty;
        }
    }

    private Sprite GetIntentIcon(EnemyIntentEntry entry, out Color color)
    {
        color = Color.white;
        if (entry.IsAbility)
        {
            color = abilityIntentColor;
            return entry.AbilityIcon != null ? entry.AbilityIcon : abilityIcon;
        }

        switch (entry.Type)
        {
            case EnemyIntent.Attack:
                color = attackIntentColor;
                return attackIcon;
            case EnemyIntent.Defend:
                color = defendIntentColor;
                return defendIcon;
            default:
                return null;
        }
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    private void SetSlotActive(IntentSlot slot, bool active)
    {
        if (slot == null) return;
        if (slot.root != null) slot.root.SetActive(active);
        if (slot.text != null)
        {
            slot.text.gameObject.SetActive(active);
            slot.text.enabled = active;
        }
        if (slot.value != null)
        {
            slot.value.gameObject.SetActive(active);
            slot.value.enabled = active;
        }
        if (slot.icon != null)
        {
            slot.icon.gameObject.SetActive(active);
            slot.icon.enabled = active;
        }
        if (slot.threatText != null)
        {
            slot.threatText.gameObject.SetActive(active);
            slot.threatText.enabled = active;
        }
        if (slot.threatIconPrimary != null)
        {
            slot.threatIconPrimary.gameObject.SetActive(active);
            slot.threatIconPrimary.enabled = active;
        }
        if (slot.threatIconSecondary != null)
        {
            slot.threatIconSecondary.gameObject.SetActive(active);
            slot.threatIconSecondary.enabled = active;
        }
        if (slot.abilityBoundBadge != null)
        {
            slot.abilityBoundBadge.gameObject.SetActive(active);
            slot.abilityBoundBadge.enabled = active;
        }
    }

    private void SetSlotAlpha(IntentSlot slot, float alpha)
    {
        if (slot == null) return;
        SetAlpha(slot.text, alpha);
        SetAlpha(slot.value, alpha);
        SetAlpha(slot.icon, alpha);
        SetAlpha(slot.threatText, alpha);
        SetAlpha(slot.threatIconPrimary, alpha);
        SetAlpha(slot.threatIconSecondary, alpha);
        SetAlpha(slot.abilityBoundBadge, alpha);
    }

    private Color GetIntentTextColor(EnemyIntentEntry entry)
    {
        if (entry.IsAbility)
            return abilityIntentColor;

        switch (entry.Type)
        {
            case EnemyIntent.Attack:
                return attackIntentColor;
            case EnemyIntent.Defend:
                return defendIntentColor;
            default:
                return Color.white;
        }
    }

    private string GetThreatLabel(EnemyIntentEntry entry)
    {
        ThreatWord primaryWord = GetDisplayThreat(entry.PrimaryThreat, entry.IsAbility);
        ThreatWord secondaryWord = GetDisplayThreat(entry.SecondaryThreat, entry.IsAbility);
        string primary = primaryWord != ThreatWord.None ? primaryWord.ToString() : string.Empty;
        string secondary = secondaryWord != ThreatWord.None ? secondaryWord.ToString() : string.Empty;

        if (string.IsNullOrEmpty(primary))
            return secondary;
        if (string.IsNullOrEmpty(secondary))
            return primary;
        return $"{primary} + {secondary}";
    }

    [Serializable]
    public class ThreatIconMapping
    {
        public ThreatWord word;
        public Sprite icon;
    }

    [Header("Threat Icons")]
    public List<ThreatIconMapping> threatIcons = new List<ThreatIconMapping>();

    private Dictionary<ThreatWord, Sprite> threatIconLookup;

    private Image FindThreatIcon(Transform parent, int index, bool primary)
    {
        string suffix = primary ? "Primary" : "Secondary";
        Transform t = parent.Find($"Intent{index}ThreatIcon{suffix}");
        if (t == null) t = parent.Find($"Intent{index}ThreatIcon{(primary ? "1" : "2")}");
        if (t == null) t = parent.Find($"Intent{index}ThreatIcon{(primary ? "A" : "B")}");
        if (t == null) t = parent.Find(primary ? "ThreatIconPrimary" : "ThreatIconSecondary");
        if (t == null) t = parent.Find(primary ? "ThreatIcon1" : "ThreatIcon2");
        return t != null ? t.GetComponent<Image>() : null;
    }

    private Image FindAbilityBoundBadge(Transform parent, int index)
    {
        Transform t = parent.Find($"Intent{index}AbilityBoundBadge");
        if (t == null) t = parent.Find("AbilityBoundBadge");
        if (t == null) t = parent.Find("AbilityBound");
        return t != null ? t.GetComponent<Image>() : null;
    }

    private void EnsureThreatIconLookup()
    {
        if (threatIconLookup != null) return;
        threatIconLookup = new Dictionary<ThreatWord, Sprite>();
        foreach (var mapping in threatIcons)
        {
            if (mapping == null || mapping.word == ThreatWord.None || mapping.icon == null)
                continue;
            threatIconLookup[mapping.word] = mapping.icon;
        }

        // Auto-load from Resources/ThreatIcons/<ThreatWord> if missing.
        foreach (ThreatWord word in Enum.GetValues(typeof(ThreatWord)))
        {
            if (word == ThreatWord.None || threatIconLookup.ContainsKey(word))
                continue;
            Sprite sprite = Resources.Load<Sprite>($"ThreatIcons/{word}");
            if (sprite != null)
                threatIconLookup[word] = sprite;
        }
    }

    private void SetThreatIcon(Image icon, ThreatWord word, float alpha)
    {
        if (icon == null) return;
        EnsureThreatIconLookup();
        if (word == ThreatWord.None || threatIconLookup == null || !threatIconLookup.TryGetValue(word, out var sprite) || sprite == null)
        {
            icon.sprite = null;
            icon.enabled = false;
            return;
        }

        icon.sprite = sprite;
        icon.color = Color.white;
        icon.enabled = true;
        SetAlpha(icon, alpha);
    }

    private void SetAbilityBoundBadge(Image icon, EnemyIntentEntry entry, float alpha)
    {
        if (icon == null)
            return;

        if (!entry.IsAbility)
        {
            icon.sprite = null;
            icon.enabled = false;
            return;
        }

        ThreatWord primary = GetDisplayThreat(entry.PrimaryThreat, true);
        ThreatWord secondary = GetDisplayThreat(entry.SecondaryThreat, true);
        ThreatWord badgeWord = primary != ThreatWord.None ? primary : secondary;
        if (badgeWord == ThreatWord.None)
        {
            icon.sprite = null;
            icon.enabled = false;
            return;
        }

        EnsureThreatIconLookup();
        if (threatIconLookup == null || !threatIconLookup.TryGetValue(badgeWord, out var sprite) || sprite == null)
        {
            icon.sprite = null;
            icon.enabled = false;
            return;
        }

        icon.sprite = sprite;
        icon.color = Color.white;
        icon.enabled = true;
        SetAlpha(icon, alpha);
    }

    private void UpdateEnemyThreatIcons()
    {
        if (currentEnemy == null)
            return;

        ThreatWord primary = GetDisplayThreat(currentEnemy.primaryThreat, isAbility: false);
        ThreatWord secondary = GetDisplayThreat(currentEnemy.secondaryThreat, isAbility: false);

        if (primary == ThreatWord.None && secondary != ThreatWord.None)
        {
            primary = secondary;
            secondary = ThreatWord.None;
        }

        SetThreatIcon(enemyThreatIconPrimary, primary, 1f);
        SetThreatIcon(enemyThreatIconSecondary, secondary, 1f);
    }

    public void FlashThreatIcons(float duration = 0.12f)
    {
        StartCoroutine(FlashThreatIconsRoutine(duration));
    }

    private IEnumerator FlashThreatIconsRoutine(float duration)
    {
        var icons = new List<Image>
        {
            enemyThreatIconPrimary,
            enemyThreatIconSecondary
        };

        foreach (var slot in intentSlots)
        {
            if (slot == null) continue;
            if (slot.threatIconPrimary != null) icons.Add(slot.threatIconPrimary);
            if (slot.threatIconSecondary != null) icons.Add(slot.threatIconSecondary);
        }

        var originals = new List<Color>(icons.Count);
        foreach (var icon in icons)
        {
            if (icon == null || !icon.enabled)
            {
                originals.Add(Color.clear);
                continue;
            }
            originals.Add(icon.color);
            Color c = icon.color;
            c.a = 1f;
            icon.color = c;
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < icons.Count; i++)
        {
            var icon = icons[i];
            if (icon == null) continue;
            icon.color = originals[i];
        }
    }

    private ThreatWord GetDisplayThreat(ThreatWord word, bool isAbility)
    {
        if (word == ThreatWord.None)
            return ThreatWord.None;

        var def = ThreatBehaviorTable.Get(word);
        if (def.Binding == ThreatBindingScope.Removed)
            return ThreatWord.None;

        if (isAbility)
        {
            if (def.Binding == ThreatBindingScope.EnemyBound)
                return ThreatWord.None;
        }
        else
        {
            if (def.Binding == ThreatBindingScope.AbilityBound)
                return ThreatWord.None;
        }

        return word;
    }

    private void SetAbilityIntentVisible(bool visible)
    {
        if (!visible || string.IsNullOrEmpty(activeAbilityIntentName))
        {
            abilityPreviewDamage = null;
            return;
        }

        if (intentSlots.Count == 0)
            return;

        IntentSlot slot = intentSlots[0];
        if (slot == null) return;

        if (slot.text != null)
        {
            slot.text.text = activeAbilityIntentName;
            slot.text.color = abilityIntentColor;
        }
        if (slot.value != null)
        {
            slot.value.text = abilityPreviewDamage.HasValue && abilityPreviewDamage.Value > 0
                ? abilityPreviewDamage.Value.ToString()
                : string.Empty;
            slot.value.color = abilityIntentColor;
        }
        if (slot.icon != null)
        {
            slot.icon.sprite = abilityIcon;
            slot.icon.color = abilityIntentColor;
            slot.icon.enabled = abilityIcon != null;
        }

        ClearIntentSlotsFromIndex(1);
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

    private void SubscribeToStatusEffects()
    {
        if (statusEffectManager == null || statusSubscribedManager == statusEffectManager)
            return;

        UnsubscribeFromStatusEffects();
        statusEffectManager.OnStatusEffectAdded += HandleStatusEffectChanged;
        statusEffectManager.OnStatusEffectRemoved += HandleStatusEffectChanged;
        statusSubscribedManager = statusEffectManager;
    }

    private void UnsubscribeFromStatusEffects()
    {
        if (statusSubscribedManager == null)
            return;

        statusSubscribedManager.OnStatusEffectAdded -= HandleStatusEffectChanged;
        statusSubscribedManager.OnStatusEffectRemoved -= HandleStatusEffectChanged;
        statusSubscribedManager = null;
    }

    private void HandleStatusEffectChanged(StatusEffect effect)
    {
        UpdateIntentDisplay();
    }

    private void SubscribeToIntentChanged()
    {
        if (currentEnemy == null) return;
        UnsubscribeFromIntentChanged();
        currentEnemy.OnIntentChanged += HandleIntentChanged;
        intentChangedSubscribedEnemy = currentEnemy;
    }

    private void UnsubscribeFromIntentChanged()
    {
        if (intentChangedSubscribedEnemy != null)
        {
            intentChangedSubscribedEnemy.OnIntentChanged -= HandleIntentChanged;
            intentChangedSubscribedEnemy = null;
        }
    }

    private void HandleIntentChanged()
    {
        RefreshIntentDisplay();
    }

    /// <summary>Refresh intent UI only (no SetIntent). Use when intent changed externally, e.g. OnIntentChanged.</summary>
    public void RefreshIntentDisplay()
    {
        if (currentEnemy != null)
            UpdateIntentDisplay();
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
    
    /// <summary>Refresh intent UI from currentEnemy. Does not call SetIntent; caller must set intent when changing it.</summary>
    public void UpdateIntent()
    {
        RefreshIntentDisplay();
    }
    
    public void ShowAbilityIntent(string abilityName, int? previewDamage = null)
    {
        showingAbilityIntent = true;
        activeAbilityIntentName = abilityName;
        abilityPreviewDamage = previewDamage;
        if (intentContainer != null)
        {
            intentContainer.SetActive(true);
        }
        SetAbilityIntentVisible(true);
        UpdateIntentDisplay();
    }
    
    public void ClearAbilityIntent()
    {
        showingAbilityIntent = false;
        activeAbilityIntentName = null;
        abilityPreviewDamage = null;
        UpdateIntentDisplay();
    }
    
    public void TakeDamage(float damage, bool ignoreGuardArmor = false)
    {
        if (currentEnemy != null)
        {
            var combatManager = UnityEngine.Object.FindFirstObjectByType<CombatDisplayManager>();
            if (combatManager != null && !combatManager.IsAnchoringShareInProgress() &&
                (currentEnemy.primaryThreat == ThreatWord.Anchoring || currentEnemy.secondaryThreat == ThreatWord.Anchoring
                 || combatManager.HasActiveEnemyThreat(ThreatWord.Anchoring)))
            {
                combatManager.ApplyAnchoringSharedDamage(this, damage, ignoreGuardArmor);
                return;
            }

            float preGuard = currentEnemy.currentGuard;
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
            bool threatChanged = ThreatBehaviorProcessor.OnDamaged(currentEnemy, damage, preGuard);
            if (currentEnemy.HasThreat(ThreatWord.Shielded) && preGuard > 0f && currentEnemy.currentGuard <= 0f)
            {
                int stunTurns = Mathf.Max(0, currentEnemy.shieldedStunTurns);
                if (stunTurns > 0)
                {
                    var stun = new StatusEffect(StatusEffectType.Stun, "Stunned", 1f, stunTurns, true);
                    AddStatusEffect(stun);
                }
                FlashThreatIcons(0.2f);
            }
            UpdateHealthDisplay();
            UpdateStaggerDisplay(); // Update stagger when damage is taken (stagger may have been applied)
            UpdateGuardDisplay(); // Update guard display when damage is taken (guard may have been reduced)
            if (threatChanged)
            {
                UpdateIntent();
            }
            PlayDamageAnimation();
            
            // Check if enemy died and notify combat manager to handle death properly
            // This ensures enemies disappear even if death happens outside of PlayerAttackEnemy
            if (currentEnemy.currentHealth <= 0)
            {
                NotifyAbilityRunnerDeath();
                
                // Notify combat manager to handle death (if not already handled)
                // This catches cases where TakeDamage is called directly (e.g., from status effects)
                var combatManager = UnityEngine.Object.FindFirstObjectByType<CombatDisplayManager>();
                if (combatManager != null && combatManager.IsEnemyStillActive(currentEnemy))
                {
                    // Let combat manager find the display index - it has access to the spawner
                    combatManager.HandleEnemyDeathIfNeeded(this, currentEnemy, -1);
                }
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
        
        // Cancel any existing tweens on this object
        LeanTween.cancel(gameObject);
        
        // Store reference to onComplete in case object gets disabled
        bool callbackFired = false;
        System.Action safeCallback = () => {
            if (!callbackFired)
            {
                callbackFired = true;
                onComplete?.Invoke();
                if (gameObject != null)
                {
                    gameObject.SetActive(false);
                }
            }
        };
        
        // Fade out animation
        float fadeDuration = 0.35f;
        LeanTween.value(gameObject, cg.alpha, 0f, fadeDuration)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float a) => { 
                if (cg != null && gameObject != null) 
                    cg.alpha = a; 
            })
            .setOnComplete(() => {
                safeCallback();
            });
        
        // Safety timeout - ensure callback fires even if animation fails
        StartCoroutine(ForceDeathCallbackAfterDelay(safeCallback, fadeDuration + 0.1f));
    }
    
    private System.Collections.IEnumerator ForceDeathCallbackAfterDelay(System.Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
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
        UnsubscribeFromIntentChanged();
        UnsubscribeFromStatusEffects();
    }
}
