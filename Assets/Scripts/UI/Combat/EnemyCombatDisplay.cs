using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
    
    [Header("Intent Display")]
    public GameObject intentContainer;
    public Image intentIcon;
    public TextMeshProUGUI intentText;
    public TextMeshProUGUI intentDamageText;
    
    [Header("Status Effects")]
    public Transform statusEffectsContainer;
    public GameObject statusEffectPrefab;
    
    private StatusEffectManager statusEffectManager;
    
    [Header("Colors")]
    public Color healthColor = Color.red;
    public Color attackIntentColor = Color.red;
    public Color defendIntentColor = Color.blue;
    
    [Header("Intent Icons")]
    public Sprite attackIcon;
    public Sprite defendIcon;
    
    [Header("Animation")]
    public Animator enemyAnimator; // Animator for enemy sprite animations
    private RuntimeAnimatorController enemyAnimatorController; // Store the enemy's specific animator controller
    
    private Enemy currentEnemy;
    private EnemyData enemyData; // Reference to the data used to create this enemy
    private bool isInitialized = false;
    
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
        
        // Set up colors
        if (healthFillImage != null)
            healthFillImage.color = healthColor;
        
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
        currentEnemy = enemy;
        enemyData = data;
        
        // Set up animator when enemy data changes
        SetupEnemyAnimator();
        
        if (isInitialized)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// Clear enemy data from this display (for wave resets)
    /// </summary>
    public void ClearEnemy()
    {
        currentEnemy = null;
        enemyData = null;
        
        // Clear visual elements
        if (enemyNameText != null)
            enemyNameText.text = "";
        
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
        
        enemyData = data;
        currentEnemy = data.CreateEnemy();
        
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
            enemyNameText.text = currentEnemy.enemyName;
        
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
            
            // Ensure portrait is in front of siblings
            enemyPortrait.transform.SetAsLastSibling();
            
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
        
        // Update intent
        UpdateIntentDisplay();
        
        // Update status effects
        UpdateStatusEffects();

        // Apply layout scaling if EnemyData provides displayScale/basePanelHeight
        if (enemyData != null)
        {
            var le = GetComponent<UnityEngine.UI.LayoutElement>();
            if (le == null) le = gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            float scaled = Mathf.Clamp(enemyData.basePanelHeight * Mathf.Max(0.25f, enemyData.displayScale), 100f, 1200f);
            le.preferredHeight = scaled;
        }
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
    
    private void UpdateIntentDisplay()
    {
        if (intentContainer != null)
        {
            intentContainer.SetActive(true);
        }
        
        if (intentText != null)
        {
            intentText.text = currentEnemy.GetIntentDescription();
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
    
    public void TakeDamage(float damage)
    {
        if (currentEnemy != null)
        {
            currentEnemy.TakeDamage(damage);
            UpdateHealthDisplay();
            PlayDamageAnimation();
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
        
        Debug.Log($"✓ Applied display scale {enemyData.displayScale} to {enemyData.enemyName} portrait");
    }
    
    /// <summary>
    /// Adjust UI element positions to create a proper stacked layout:
    /// Name -> Portrait -> Health -> Intent
    /// This ensures elements are properly spaced below the scaled portrait
    /// </summary>
    private void AdjustUILayoutForPortraitScale()
    {
        if (enemyData == null || enemyPortrait == null) return;
        
        RectTransform portraitRect = enemyPortrait.GetComponent<RectTransform>();
        if (portraitRect == null) return;
        
        // Calculate the actual displayed height of the scaled portrait
        float portraitHeight = portraitRect.rect.height * enemyData.displayScale;
        float portraitCenterY = portraitRect.anchoredPosition.y;
        float portraitBottom = portraitCenterY - (portraitHeight / 2f);
        
        // Spacing between UI elements (in pixels)
        const float spacingBetweenElements = 15f;
        
        // Start positioning elements right below the portrait
        float currentY = portraitBottom;
        
        // 1. Health Bar - Position directly below portrait
        if (healthSlider != null)
        {
            RectTransform healthRect = healthSlider.GetComponent<RectTransform>();
            if (healthRect != null)
            {
                // Position health bar directly below portrait
                currentY -= spacingBetweenElements;
                float healthHeight = healthRect.rect.height;
                float healthY = currentY - (healthHeight / 2f);
                
                Vector2 currentHealthPos = healthRect.anchoredPosition;
                healthRect.anchoredPosition = new Vector2(currentHealthPos.x, healthY);
                
                // Move current position down for next element
                currentY -= healthHeight;
            }
        }
        
        // 2. Intent Container - Position directly below health bar (or portrait if no health bar)
        if (intentContainer != null)
        {
            RectTransform intentRect = intentContainer.GetComponent<RectTransform>();
            if (intentRect != null)
            {
                currentY -= spacingBetweenElements;
                float intentHeight = intentRect.rect.height;
                float intentY = currentY - (intentHeight / 2f);
                
                Vector2 currentIntentPos = intentRect.anchoredPosition;
                intentRect.anchoredPosition = new Vector2(currentIntentPos.x, intentY);
            }
        }
        
        // 3. Enemy Name - Keep at top (above portrait) but ensure it doesn't overlap
        // Only adjust name if portrait scale would cause overlap
        if (enemyNameText != null && enemyData.displayScale > 1.0f)
        {
            RectTransform nameRect = enemyNameText.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                Vector2 currentNamePos = nameRect.anchoredPosition;
                float nameBottom = currentNamePos.y - (nameRect.rect.height / 2f);
                float portraitTop = portraitCenterY + (portraitHeight / 2f);
                
                // If name overlaps with scaled portrait, move it up
                if (nameBottom > portraitTop)
                {
                    float nameHeight = nameRect.rect.height;
                    float newNameY = portraitTop + (nameHeight / 2f) + spacingBetweenElements;
                    nameRect.anchoredPosition = new Vector2(currentNamePos.x, newNameY);
                }
            }
        }
        
        Debug.Log($"✓ Stacked UI layout for {enemyData.enemyName}: Portrait (scale: {enemyData.displayScale}x) -> Health -> Intent");
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
}
