using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    private Enemy currentEnemy;
    private EnemyData enemyData; // Reference to the data used to create this enemy
    private bool isInitialized = false;
    
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
                if (cdm != null && cdm.enemyDisplays != null)
                {
                    for (int i = 0; i < cdm.enemyDisplays.Count; i++)
                    {
                        if (cdm.enemyDisplays[i] == this)
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
        
        if (isInitialized)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// Set enemy from EnemyData (creates Enemy instance automatically).
    /// </summary>
    public void SetEnemyFromData(EnemyData data)
    {
        if (data == null) return;
        
        enemyData = data;
        currentEnemy = data.CreateEnemy();
        
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
            enemyPortrait.sprite = enemyData.enemySprite;
            enemyPortrait.enabled = true;
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
            healthSlider.value = currentEnemy.currentHealth;
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
        }
    }
    
    public void Heal(int amount)
    {
        if (currentEnemy != null)
        {
            currentEnemy.Heal(amount);
            UpdateHealthDisplay();
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
    
    // Animation methods for visual feedback
    public void PlayDamageAnimation()
    {
        // Flash red when taking damage
        StartCoroutine(FlashColor(Color.red, 0.2f));
    }
    
    public void PlayHealAnimation()
    {
        // Flash green when healing
        StartCoroutine(FlashColor(Color.green, 0.2f));
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
}
