using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCombatDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Image characterPortrait;
    public TextMeshProUGUI characterNameText;
    
    [Header("Vertical Bar Components")]
    public VerticalHealthBar healthBar;
    public VerticalHealthBar manaBar; // Using VerticalHealthBar with BarType.Mana
    public VerticalEnergyShieldBar energyShieldBar;
    public VerticalGuardBar guardBar;
    
    [Header("Bar GameObject References")]
    [Tooltip("The entire EnergyShield bar GameObject to enable/disable")]
    public GameObject energyShieldBarGameObject;
    [Tooltip("The entire Guard bar GameObject to enable/disable")]
    public GameObject guardBarGameObject;
    
    [Header("Bar Text Displays")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI energyShieldText;
    public TextMeshProUGUI guardText;
    [Header("Attribute Readouts (optional)")]
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI evasionText;
    
    [Header("Status Effects Display")]
    public Transform statusEffectsContainer;
    public GameObject statusEffectIconPrefab;
    
    [Header("Player Sprite Animation")]
    public PlayerSpriteAnimator playerSpriteAnimator;
    
    [Header("Speed Meter UI")]
    [SerializeField] private SpeedMeterRing speedMeterRing;
    
    [Header("Colors")]
    // Note: Mana color is now handled by VerticalHealthBar with BarType.Mana
    
    [Header("Bar Settings")]
    [Tooltip("If true, health bar color changes based on health percentage. If false, uses fixed color from VerticalHealthBar.")]
    public bool useDynamicHealthColor = false;
    
    private Character currentCharacter;
    private CharacterManager characterManager;
    private StatusEffectManager statusEffectManager;
    
    private void Start()
    {
        InitializeDisplay();
        SetupCharacterManager();
        HookSpeedMeterRing();
    }

    private void OnEnable()
    {
        HookSpeedMeterRing();
    }

    private void OnDisable()
    {
        if (CombatDeckManager.Instance != null)
        {
            CombatDeckManager.Instance.OnSpeedMeterChanged -= HandleSpeedMeterChanged;
        }
    }
    
    private void InitializeDisplay()
    {
        // Initialize StatusEffectManager
        statusEffectManager = GetComponent<StatusEffectManager>();
        if (statusEffectManager == null)
        {
            statusEffectManager = gameObject.AddComponent<StatusEffectManager>();
        }
        
        // Set up status effects container
        if (statusEffectsContainer == null)
        {
            // Create status effects container if it doesn't exist
            GameObject container = new GameObject("PlayerStatusEffectsContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = new Vector3(0, 150, 0); // Above the player display
            statusEffectsContainer = container.transform;
        }
        
        // Set up status effect manager
        statusEffectManager.statusEffectContainer = statusEffectsContainer;
        if (statusEffectIconPrefab != null)
        {
            statusEffectManager.statusEffectIconPrefab = statusEffectIconPrefab;
        }
        
        // Auto-find components if not assigned
        if (characterNameText == null)
            characterNameText = transform.Find("CharacterName")?.GetComponent<TextMeshProUGUI>();
            
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.GetComponent<VerticalHealthBar>();
            
        if (manaBar == null)
            manaBar = transform.Find("ManaBar")?.GetComponent<VerticalHealthBar>();
            
        if (energyShieldBar == null)
            energyShieldBar = transform.Find("EnergyShieldBar")?.GetComponent<VerticalEnergyShieldBar>();
            
        if (energyShieldBarGameObject == null)
            energyShieldBarGameObject = transform.Find("EnergyShieldBar")?.gameObject;
            
        if (guardBar == null)
            guardBar = transform.Find("GuardBar")?.GetComponent<VerticalGuardBar>();
            
        if (guardBarGameObject == null)
            guardBarGameObject = transform.Find("GuardBar")?.gameObject;
            
        if (healthText == null)
            healthText = transform.Find("HealthBar/HealthText")?.GetComponent<TextMeshProUGUI>();
            
        if (manaText == null)
            manaText = transform.Find("ManaBar/ManaText")?.GetComponent<TextMeshProUGUI>();
            
        if (energyShieldText == null)
            energyShieldText = transform.Find("EnergyShieldBar/EnergyShieldText")?.GetComponent<TextMeshProUGUI>();
            
        if (guardText == null)
            guardText = transform.Find("GuardBar/GuardText")?.GetComponent<TextMeshProUGUI>();
            
        if (playerSpriteAnimator == null)
        {
            playerSpriteAnimator = GetComponentInChildren<PlayerSpriteAnimator>();
            if (playerSpriteAnimator == null)
            {
                // Fallback: dynamic player prefab may not be a child of this display
                playerSpriteAnimator = FindFirstObjectByType<PlayerSpriteAnimator>();
                if (playerSpriteAnimator == null)
                {
                    Debug.LogWarning("PlayerSpriteAnimator not found in children or scene. Attack/Guard animations will be skipped until available.");
                }
            }
        }

        if (speedMeterRing == null)
        {
            speedMeterRing = GetComponentInChildren<SpeedMeterRing>(includeInactive: true);
        }
    }
    
    private void SetupCharacterManager()
    {
        characterManager = CharacterManager.Instance;
        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found! Make sure it exists in the scene.");
            return;
        }
        // Ensure a character is loaded (handles scene transitions)
        characterManager.EnsureCharacterLoadedFromPrefs();
        
        // Subscribe to character events
        characterManager.OnCharacterDamaged += OnCharacterDamaged;
        characterManager.OnCharacterHealed += OnCharacterHealed;
        characterManager.OnManaUsed += OnManaUsed;
        
        // Load initial character data
        LoadCharacterData();
    }
    
    private void LoadCharacterData()
    {
        currentCharacter = characterManager.GetCurrentCharacter();
        if (currentCharacter == null)
        {
            Debug.LogWarning("No character loaded! Creating test character for display.");
            CreateTestCharacter();
        }
        
        UpdateDisplay();
    }
    
    private void CreateTestCharacter()
    {
        // Create a test character for display purposes
        currentCharacter = new Character("Test Player", "Marauder");
        currentCharacter.currentHealth = 80;
        currentCharacter.mana = 2;
        currentCharacter.reliance = 150;
        currentCharacter.currentGuard = 10f;
    }
    
    private void UpdateDisplay()
    {
        if (currentCharacter == null) return;
        
        // Update basic info
        if (characterNameText != null)
            characterNameText.text = currentCharacter.characterName;
        
        // Update health
        UpdateHealthDisplay();
        
        // Update energy shield
        UpdateEnergyShieldDisplay();
        
        // Update mana
        UpdateManaDisplay();
        
        // Update guard
        UpdateGuardDisplay();

        // Update attribute readouts if present
        if (accuracyText != null) accuracyText.text = $"Acc: {currentCharacter.accuracyRating:F0}";
        if (evasionText != null) evasionText.text = $"Ev%: {currentCharacter.increasedEvasion * 100f:F0}";

        RefreshSpeedMeterImmediately();
    }
    
    private void UpdateHealthDisplay()
    {
        if (healthBar != null)
        {
            float healthPercentage = (float)currentCharacter.currentHealth / currentCharacter.maxHealth;
            healthBar.SetFillAmount(healthPercentage);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentCharacter.currentHealth}/{currentCharacter.maxHealth}";
        }
    }
    
    private void UpdateEnergyShieldDisplay()
    {
        // Show/hide energy shield bar based on whether energy shield exists
        bool hasEnergyShield = currentCharacter.currentEnergyShield > 0;
        
        // Disable the entire energy shield bar GameObject when no energy shield
        if (energyShieldBarGameObject != null)
        {
            energyShieldBarGameObject.SetActive(hasEnergyShield);
            Debug.Log($"Energy Shield Bar GameObject active: {hasEnergyShield} (Value: {currentCharacter.currentEnergyShield})");
        }
        
        if (energyShieldBar != null && hasEnergyShield)
        {
            float energyShieldPercentage = currentCharacter.maxEnergyShield > 0 ? 
                (float)currentCharacter.currentEnergyShield / currentCharacter.maxEnergyShield : 0f;
            energyShieldBar.SetFillAmount(energyShieldPercentage);
        }
        
        if (energyShieldText != null)
        {
            if (hasEnergyShield)
            {
                energyShieldText.text = $"{currentCharacter.currentEnergyShield}/{currentCharacter.maxEnergyShield}";
            }
            else
            {
                energyShieldText.text = "";
            }
        }
    }
    
    public void UpdateManaDisplay()
    {
        if (manaBar != null)
        {
            float manaPercentage = (float)currentCharacter.mana / currentCharacter.maxMana;
            manaBar.SetFillAmount(manaPercentage);
        }
        
        if (manaText != null)
        {
            manaText.text = $"{currentCharacter.mana}/{currentCharacter.maxMana}";
        }
    }
    
    public void UpdateGuardDisplay()
    {
        // Show/hide guard bar based on whether guard exists
        bool hasGuard = currentCharacter.currentGuard > 0;
        
        Debug.Log($"<color=cyan>UpdateGuardDisplay called:</color> currentGuard={currentCharacter.currentGuard}, hasGuard={hasGuard}");
        
        // Disable the entire guard bar GameObject when no guard
        if (guardBarGameObject != null)
        {
            guardBarGameObject.SetActive(hasGuard);
            Debug.Log($"Guard Bar GameObject active: {hasGuard} (Value: {currentCharacter.currentGuard})");
        }
        else
        {
            Debug.LogWarning($"<color=red>guardBarGameObject is NULL!</color>");
        }
        
        if (guardBar != null && hasGuard)
        {
            // Guard percentage is based on max health (guard cannot exceed max health)
            float guardPercentage = currentCharacter.currentGuard / currentCharacter.maxHealth;
            guardBar.SetFillAmount(guardPercentage);
        }
        
        if (guardText != null)
        {
            if (hasGuard)
            {
                guardText.text = $"{currentCharacter.currentGuard:F0}/{currentCharacter.maxHealth}";
            }
            else
            {
                guardText.text = "";
            }
        }
    }

    private void HookSpeedMeterRing()
    {
        if (CombatDeckManager.Instance == null) return;

        CombatDeckManager.Instance.OnSpeedMeterChanged -= HandleSpeedMeterChanged;
        CombatDeckManager.Instance.OnSpeedMeterChanged += HandleSpeedMeterChanged;
        RefreshSpeedMeterImmediately();
    }

    private void HandleSpeedMeterChanged(CombatDeckManager.SpeedMeterState state)
    {
        if (speedMeterRing != null)
        {
            speedMeterRing.UpdateMeterState(state);
        }
    }

    private void RefreshSpeedMeterImmediately()
    {
        if (speedMeterRing != null && CombatDeckManager.Instance != null)
        {
            speedMeterRing.UpdateMeterState(CombatDeckManager.Instance.CurrentSpeedMeterState);
        }
    }
    
    // Event handlers
    private void OnCharacterDamaged(Character character)
    {
        UpdateHealthDisplay();
        UpdateEnergyShieldDisplay();
        UpdateGuardDisplay();
        
        // Play damage shake animation
        if (playerSpriteAnimator != null)
        {
            playerSpriteAnimator.PlayDamageShake();
        }
    }
    
    private void OnCharacterHealed(Character character)
    {
        UpdateHealthDisplay();
        
        // Play heal glow animation
        if (playerSpriteAnimator != null)
        {
            playerSpriteAnimator.PlayHealGlow();
        }
    }
    
    /// <summary>
    /// Add a status effect to the player
    /// </summary>
    public void AddStatusEffect(StatusEffect effect)
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.AddStatusEffect(effect);
        }
    }
    
    /// <summary>
    /// Remove a status effect from the player
    /// </summary>
    public void RemoveStatusEffect(StatusEffectType effectType)
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.RemoveStatusEffect(effectType);
        }
    }
    
    /// <summary>
    /// Check if the player has a specific status effect
    /// </summary>
    public bool HasStatusEffect(StatusEffectType effectType)
    {
        if (statusEffectManager != null)
        {
            return statusEffectManager.HasStatusEffect(effectType);
        }
        return false;
    }
    
    /// <summary>
    /// Get the player's status effect manager
    /// </summary>
    public StatusEffectManager GetStatusEffectManager()
    {
        return statusEffectManager;
    }
    
    /// <summary>
    /// Trigger attack nudge animation
    /// </summary>
    public void TriggerAttackNudge()
    {
        if (playerSpriteAnimator == null)
        {
            // Attempt late binding for dynamic prefab cases
            playerSpriteAnimator = GetComponentInChildren<PlayerSpriteAnimator>();
            if (playerSpriteAnimator == null)
                playerSpriteAnimator = FindFirstObjectByType<PlayerSpriteAnimator>();
        }
        if (playerSpriteAnimator != null)
            playerSpriteAnimator.PlayAttackNudge();
    }
    
    /// <summary>
    /// Trigger guard sheen animation
    /// </summary>
    public void TriggerGuardSheen()
    {
        if (playerSpriteAnimator == null)
        {
            // Attempt late binding for dynamic prefab cases
            playerSpriteAnimator = GetComponentInChildren<PlayerSpriteAnimator>();
            if (playerSpriteAnimator == null)
                playerSpriteAnimator = FindFirstObjectByType<PlayerSpriteAnimator>();
        }
        if (playerSpriteAnimator != null)
            playerSpriteAnimator.PlayGuardSheen();
    }
    
    private void OnManaUsed(Character character)
    {
        UpdateManaDisplay();
        
        // Update card usability when mana changes
        CombatDeckManager deckManager = CombatDeckManager.Instance;
        if (deckManager != null)
        {
            deckManager.UpdateCardUsability();
        }
    }
    
    // Debug methods for testing buff/debuff system
    [ContextMenu("Add Strength Buff")]
    public void TestAddStrengthBuff()
    {
        StatusEffect strength = new StatusEffect(StatusEffectType.Strength, "Strength", 3f, 5, false);
        AddStatusEffect(strength);
    }
    
    [ContextMenu("Add Poison Debuff")]
    public void TestAddPoisonDebuff()
    {
        StatusEffect poison = new StatusEffect(StatusEffectType.Poison, "Poison", 2f, 3, true);
        AddStatusEffect(poison);
    }
    
    [ContextMenu("Add Regeneration Buff")]
    public void TestAddRegenerationBuff()
    {
        StatusEffect regen = new StatusEffect(StatusEffectType.Regeneration, "Regeneration", 5f, 4, false);
        AddStatusEffect(regen);
    }
    
    [ContextMenu("Clear All Status Effects")]
    public void TestClearAllStatusEffects()
    {
        if (statusEffectManager != null)
        {
            statusEffectManager.ClearAllStatusEffects();
        }
    }
    
    // Public methods for external updates
    public void RefreshDisplay()
    {
        LoadCharacterData();
    }
    
    public void SetCharacter(Character character)
    {
        currentCharacter = character;
        UpdateDisplay();
    }
    
    // Debug methods
    [ContextMenu("Test Damage")]
    public void TestDamage()
    {
        if (characterManager != null)
        {
            characterManager.TakeDamage(10);
        }
    }
    
    [ContextMenu("Test Heal")]
    public void TestHeal()
    {
        if (characterManager != null)
        {
            characterManager.Heal(10);
        }
    }
    
    [ContextMenu("Test Mana Use")]
    public void TestManaUse()
    {
        if (characterManager != null)
        {
            characterManager.UseMana(1);
        }
    }
    
    [ContextMenu("Test Set Energy Shield")]
    private void TestSetEnergyShield()
    {
        if (currentCharacter != null)
        {
            currentCharacter.currentEnergyShield = 50;
            currentCharacter.maxEnergyShield = 100;
            UpdateEnergyShieldDisplay();
            Debug.Log("Energy Shield set to 50/100");
        }
    }
    
    [ContextMenu("Test Clear Energy Shield")]
    private void TestClearEnergyShield()
    {
        if (currentCharacter != null)
        {
            currentCharacter.currentEnergyShield = 0;
            currentCharacter.maxEnergyShield = 0;
            UpdateEnergyShieldDisplay();
            Debug.Log("Energy Shield cleared");
        }
    }
    
    [ContextMenu("Test Set Guard")]
    private void TestSetGuard()
    {
        if (currentCharacter != null)
        {
            currentCharacter.currentGuard = 25;
            UpdateGuardDisplay();
            Debug.Log("Guard set to 25");
        }
    }
    
    [ContextMenu("Test Clear Guard")]
    private void TestClearGuard()
    {
        if (currentCharacter != null)
        {
            currentCharacter.currentGuard = 0;
            UpdateGuardDisplay();
            Debug.Log("Guard cleared");
        }
    }
    
    [ContextMenu("Test Attack Nudge Animation")]
    private void TestAttackNudgeAnimation()
    {
        if (playerSpriteAnimator != null)
        {
            playerSpriteAnimator.PlayAttackNudge();
            Debug.Log("Playing attack nudge animation");
        }
        else
        {
            Debug.LogWarning("PlayerSpriteAnimator not found!");
        }
    }
    
    [ContextMenu("Test Guard Sheen Animation")]
    private void TestGuardSheenAnimation()
    {
        if (playerSpriteAnimator != null)
        {
            playerSpriteAnimator.PlayGuardSheen();
            Debug.Log("Playing guard sheen animation");
        }
        else
        {
            Debug.LogWarning("PlayerSpriteAnimator not found!");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (characterManager != null)
        {
            characterManager.OnCharacterDamaged -= OnCharacterDamaged;
            characterManager.OnCharacterHealed -= OnCharacterHealed;
            characterManager.OnManaUsed -= OnManaUsed;
        }
    }
}
