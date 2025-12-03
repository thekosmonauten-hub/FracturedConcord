using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CharacterStatsController : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect statsScrollView;
    public Transform statsContainer;
    public GameObject statRowPrefab;
    
    [Header("Character Info")]
    public Image characterPortrait;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterClassText;
    public TextMeshProUGUI characterLevelText;
    public TextMeshProUGUI experienceText;
    public Slider experienceSlider;
    
    [Header("Resource Bars")]
    // Only experience slider is used - other sliders removed
    
    [Header("Attribute Section")]
    public TextMeshProUGUI StrengthText;
    public TextMeshProUGUI DexterityText;
    public TextMeshProUGUI IntelligenceText;
    
    [Header("Resource Section")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI relianceText;
    
    [Header("Damage Section - Physical")]
    public TextMeshProUGUI PhysicalFlatValue;
    public TextMeshProUGUI PhysicalIncreaseValue;
    public TextMeshProUGUI PhysicalMoreValue;
    
    [Header("Damage Section - Fire")]
    public TextMeshProUGUI FireFlatValue;
    public TextMeshProUGUI FireIncreaseValue;
    public TextMeshProUGUI FireMoreValue;
    
    [Header("Damage Section - Cold")]
    public TextMeshProUGUI ColdFlatValue;
    public TextMeshProUGUI ColdIncreaseValue;
    public TextMeshProUGUI ColdMoreValue;
    
    [Header("Damage Section - Lightning")]
    public TextMeshProUGUI LightningFlatValue;
    public TextMeshProUGUI LightningIncreaseValue;
    public TextMeshProUGUI LightningMoreValue;
    
    [Header("Damage Section - Chaos")]
    public TextMeshProUGUI ChaosFlatValue;
    public TextMeshProUGUI ChaosIncreaseValue;
    public TextMeshProUGUI ChaosMoreValue;
    
    [Header("Defense Section")]
    public TextMeshProUGUI ArmourText;
    public TextMeshProUGUI EvasionText;
    public TextMeshProUGUI AccuracyText;
    public TextMeshProUGUI EvasionIncText;
    public TextMeshProUGUI EnergyShieldText;
    
    [Header("Resistance Section")]
    public TextMeshProUGUI PhysicalResistanceText;
    public TextMeshProUGUI FireResistanceText;
    public TextMeshProUGUI ColdResistanceText;
    public TextMeshProUGUI LightningResistanceText;
    public TextMeshProUGUI ChaosResistanceText;
    
    [Header("Card Mechanics Section")]
    public TextMeshProUGUI DiscardPowerText;
    public TextMeshProUGUI ManaPerTurnText;
    public TextMeshProUGUI DrawPerTurnText;
    public TextMeshProUGUI HandSizeText;
    
    [Header("Ascendancy Button")]
    [Tooltip("Button to open the Ascendancy panel")]
    public Button openAscendancyButton;
    [Tooltip("Reference to the AscendancyDisplayPanel (auto-found if not assigned)")]
    public AscendancyDisplayPanel ascendancyDisplayPanel;
    
    [Header("Colors")]
    public Color positiveStatColor = Color.green;
    public Color negativeStatColor = Color.red;
    public Color neutralStatColor = Color.white;
    public Color healthColor = Color.red;
    public Color energyShieldColor = new Color(0.5f, 0.8f, 1f, 1f);
    public Color manaColor = Color.blue;
    public Color relianceColor = Color.yellow;
    
    private CharacterStatsData currentStats;
    private Character currentCharacter;
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        RefreshDisplay();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        // Clean up button listener
        if (openAscendancyButton != null)
        {
            openAscendancyButton.onClick.RemoveAllListeners();
        }
    }
    
    private void InitializeUI()
    {
        // Set up experience slider (only slider used)
        if (experienceSlider != null)
        {
            experienceSlider.minValue = 0f;
            experienceSlider.maxValue = 1f;
        }
        
        // Ensure this GameObject and its parents are enabled before searching for children
        if (!gameObject.activeInHierarchy)
        {
            EnableGameObjectAndParents(gameObject);
        }
        
        // Auto-find AscendancyDisplayPanel if not assigned
        if (ascendancyDisplayPanel == null)
        {
            ascendancyDisplayPanel = FindFirstObjectByType<AscendancyDisplayPanel>();
            if (ascendancyDisplayPanel == null)
            {
                Debug.LogWarning("[CharacterStatsController] AscendancyDisplayPanel not found. Ascendancy button will not work.");
            }
        }
        
        // Setup Ascendancy button
        if (openAscendancyButton != null)
        {
            openAscendancyButton.onClick.RemoveAllListeners();
            openAscendancyButton.onClick.AddListener(OnOpenAscendancyClicked);
        }
        else
        {
            // Try to auto-find the button (only works if panel is enabled)
            if (gameObject.activeInHierarchy)
            {
                openAscendancyButton = GetComponentInChildren<Button>(true); // Include inactive children
                if (openAscendancyButton != null && openAscendancyButton.gameObject.name.Contains("Ascendancy"))
                {
                    openAscendancyButton.onClick.RemoveAllListeners();
                    openAscendancyButton.onClick.AddListener(OnOpenAscendancyClicked);
                    Debug.Log("[CharacterStatsController] Auto-found Ascendancy button");
                }
            }
        }
    }
    
    /// <summary>
    /// Recursively enable a GameObject and all its parents
    /// </summary>
    private void EnableGameObjectAndParents(GameObject obj)
    {
        if (obj == null) return;
        
        // Enable this object
        obj.SetActive(true);
        
        // Recursively enable parents
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            parent.gameObject.SetActive(true);
            parent = parent.parent;
        }
    }
    

    
    private void SubscribeToEvents()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharacterLoaded += OnCharacterLoaded;
            CharacterManager.Instance.OnCharacterDamaged += OnCharacterDamaged;
            CharacterManager.Instance.OnCharacterHealed += OnCharacterHealed;
            CharacterManager.Instance.OnManaUsed += OnManaUsed;
            CharacterManager.Instance.OnRelianceUsed += OnRelianceUsed;
        }
        
        // PassiveTreeManager references removed - passive tree system deleted
    }
    
    private void UnsubscribeFromEvents()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCharacterLoaded -= OnCharacterLoaded;
            CharacterManager.Instance.OnCharacterDamaged -= OnCharacterDamaged;
            CharacterManager.Instance.OnCharacterHealed -= OnCharacterHealed;
            CharacterManager.Instance.OnManaUsed -= OnManaUsed;
            CharacterManager.Instance.OnRelianceUsed -= OnRelianceUsed;
        }
        
        // PassiveTreeManager references removed - passive tree system deleted
    }
    
    private void OnCharacterLoaded(Character character)
    {
        currentCharacter = character;
        currentStats = new CharacterStatsData(character);
        RefreshDisplay();
    }
    
    private void OnCharacterDamaged(Character character)
    {
        UpdateResourceDisplays();
    }
    
    private void OnCharacterHealed(Character character)
    {
        UpdateResourceDisplays();
    }
    
    private void OnManaUsed(Character character)
    {
        UpdateResourceDisplays();
    }
    
    private void OnRelianceUsed(Character character)
    {
        UpdateResourceDisplays();
    }
    
    // OnPassiveTreeChanged method removed - passive tree system deleted
    
    /// <summary>
    /// Update the panel with character data (called by CharacterStatsPanelManager)
    /// </summary>
    public void UpdateCharacterStats(Character character)
    {
        if (character == null)
        {
            Debug.LogWarning("[CharacterStatsController] Character is null in UpdateCharacterStats");
            return;
        }
        
        currentCharacter = character;
        currentStats = new CharacterStatsData(character);
        RefreshDisplay();
        
        // Ensure button is set up now that panel is active
        SetupAscendancyButton();
        
        Debug.Log($"[CharacterStatsController] Updated stats for {character.characterName}");
    }
    
    /// <summary>
    /// Setup the Ascendancy button (called when panel becomes active)
    /// </summary>
    private void SetupAscendancyButton()
    {
        // Only setup if panel is now active and button wasn't found before
        if (!gameObject.activeInHierarchy) return;
        
        // Auto-find AscendancyDisplayPanel if not assigned
        if (ascendancyDisplayPanel == null)
        {
            ascendancyDisplayPanel = FindFirstObjectByType<AscendancyDisplayPanel>();
        }
        
        // Setup Ascendancy button if not already set up
        if (openAscendancyButton == null)
        {
            // Try to find the button now that panel is active
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (Button btn in allButtons)
            {
                if (btn.gameObject.name.Contains("Ascendancy") || 
                    btn.gameObject.name.Contains("ascendancy"))
                {
                    openAscendancyButton = btn;
                    break;
                }
            }
        }
        
        // Setup button listener if button was found
        if (openAscendancyButton != null)
        {
            openAscendancyButton.onClick.RemoveAllListeners();
            openAscendancyButton.onClick.AddListener(OnOpenAscendancyClicked);
            Debug.Log("[CharacterStatsController] Ascendancy button set up");
        }
    }
    
    public void RefreshDisplay()
    {
        if (currentCharacter == null)
        {
            currentCharacter = CharacterManager.Instance?.GetCurrentCharacter();
            if (currentCharacter == null) return;
        }
        
        if (currentStats == null)
        {
            currentStats = new CharacterStatsData(currentCharacter);
        }
        
        UpdateCharacterInfo();
        UpdateResourceDisplays();
        UpdateStatSections();
    }
    
    private void UpdateCharacterInfo()
    {
        if (characterNameText != null)
            characterNameText.text = currentCharacter.characterName;
        
        if (characterClassText != null)
            characterClassText.text = currentCharacter.characterClass;
        
        if (characterLevelText != null)
            characterLevelText.text = $"Level {currentCharacter.level}";
        
        if (experienceText != null)
        {
            int requiredExp = currentCharacter.GetRequiredExperience();
            experienceText.text = $"Experience: {currentCharacter.experience} / {requiredExp}";
        }
        
        if (experienceSlider != null)
        {
            int requiredExp = currentCharacter.GetRequiredExperience();
            experienceSlider.value = requiredExp > 0 ? (float)currentCharacter.experience / requiredExp : 0f;
        }
    }
    
    private void UpdateResourceDisplays()
    {
        // Update specific text fields (no sliders except experience)
        if (healthText != null)
            healthText.text = $"{currentStats.currentHealth} / {currentStats.maxHealth}";
        
        if (manaText != null)
            manaText.text = $"{currentStats.currentMana} / {currentStats.maxMana}";
        
        if (relianceText != null)
            relianceText.text = $"{currentStats.currentReliance} / {currentStats.maxReliance}";
    }
    
    private void UpdateStatSections()
    {
        UpdateAttributeSection();
        UpdateDamageSection();
        UpdateDefenseSection();
        UpdateResistanceSection();
        UpdateCardMechanicsSection();
    }
    
    private void UpdateAttributeSection()
    {
        if (StrengthText != null)
            StrengthText.text = currentStats.strength.ToString();
        
        if (DexterityText != null)
            DexterityText.text = currentStats.dexterity.ToString();
        
        if (IntelligenceText != null)
            IntelligenceText.text = currentStats.intelligence.ToString();
    }
    
    private void UpdateDamageSection()
    {
        // Physical Damage
        if (PhysicalFlatValue != null)
            PhysicalFlatValue.text = currentStats.addedPhysicalDamage.ToString("F1");
        
        if (PhysicalIncreaseValue != null)
            PhysicalIncreaseValue.text = currentStats.increasedPhysicalDamage.ToString("F1") + "%";
        
        if (PhysicalMoreValue != null)
            PhysicalMoreValue.text = currentStats.morePhysicalDamage.ToString("F2") + "x";
        
        // Fire Damage
        if (FireFlatValue != null)
            FireFlatValue.text = currentStats.addedFireDamage.ToString("F1");
        
        if (FireIncreaseValue != null)
            FireIncreaseValue.text = currentStats.increasedFireDamage.ToString("F1") + "%";
        
        if (FireMoreValue != null)
            FireMoreValue.text = currentStats.moreFireDamage.ToString("F2") + "x";
        
        // Cold Damage
        if (ColdFlatValue != null)
            ColdFlatValue.text = currentStats.addedColdDamage.ToString("F1");
        
        if (ColdIncreaseValue != null)
            ColdIncreaseValue.text = currentStats.increasedColdDamage.ToString("F1") + "%";
        
        if (ColdMoreValue != null)
            ColdMoreValue.text = currentStats.moreColdDamage.ToString("F2") + "x";
        
        // Lightning Damage
        if (LightningFlatValue != null)
            LightningFlatValue.text = currentStats.addedLightningDamage.ToString("F1");
        
        if (LightningIncreaseValue != null)
            LightningIncreaseValue.text = currentStats.increasedLightningDamage.ToString("F1") + "%";
        
        if (LightningMoreValue != null)
            LightningMoreValue.text = currentStats.moreLightningDamage.ToString("F2") + "x";
        
        // Chaos Damage
        if (ChaosFlatValue != null)
            ChaosFlatValue.text = currentStats.addedChaosDamage.ToString("F1");
        
        if (ChaosIncreaseValue != null)
            ChaosIncreaseValue.text = currentStats.increasedChaosDamage.ToString("F1") + "%";
        
        if (ChaosMoreValue != null)
            ChaosMoreValue.text = currentStats.moreChaosDamage.ToString("F2") + "x";
    }
    
    private void UpdateDefenseSection()
    {
        if (ArmourText != null)
            ArmourText.text = currentStats.armour.ToString();
        
        if (EvasionText != null)
            EvasionText.text = currentStats.evasion.ToString("F0");
        if (AccuracyText != null)
            AccuracyText.text = currentStats.accuracy.ToString("F0");
        if (EvasionIncText != null)
            EvasionIncText.text = (currentStats.evasionIncreased * 100f).ToString("F0") + "%";
        
        if (EnergyShieldText != null)
            EnergyShieldText.text = $"{currentStats.currentEnergyShield} / {currentStats.maxEnergyShield}";
    }
    
    private void UpdateResistanceSection()
    {
        if (PhysicalResistanceText != null)
            PhysicalResistanceText.text = currentStats.physicalResistance.ToString("F1") + "%";
        
        if (FireResistanceText != null)
            FireResistanceText.text = currentStats.fireResistance.ToString("F1") + "%";
        
        if (ColdResistanceText != null)
            ColdResistanceText.text = currentStats.coldResistance.ToString("F1") + "%";
        
        if (LightningResistanceText != null)
            LightningResistanceText.text = currentStats.lightningResistance.ToString("F1") + "%";
        
        if (ChaosResistanceText != null)
            ChaosResistanceText.text = currentStats.chaosResistance.ToString("F1") + "%";
    }
    
    private void UpdateCardMechanicsSection()
    {
        if (DiscardPowerText != null)
            DiscardPowerText.text = currentStats.discardPower.ToString("F1");
        
        if (ManaPerTurnText != null)
            ManaPerTurnText.text = currentStats.manaPerTurn.ToString("F1");
        
        if (DrawPerTurnText != null)
            DrawPerTurnText.text = currentStats.cardsDrawnPerTurn.ToString();
        
        if (HandSizeText != null)
            HandSizeText.text = currentStats.maxHandSize.ToString();
    }
    
    // Public methods for external updates
    public void UpdateStats(CharacterStatsData newStats)
    {
        currentStats = newStats;
        RefreshDisplay();
    }
    
    /// <summary>
    /// Get the current character stats data
    /// </summary>
    public CharacterStatsData GetCharacterStats()
    {
        return currentStats;
    }
    
    // Context menu methods for testing
    [ContextMenu("Refresh Display")]
    private void DebugRefreshDisplay()
    {
        RefreshDisplay();
    }
    
    [ContextMenu("Update All Stats")]
    private void DebugUpdateAllStats()
    {
        if (currentCharacter != null)
        {
            UpdateCharacterStats(currentCharacter);
        }
    }
    
    /// <summary>
    /// Called when the Ascendancy button is clicked
    /// </summary>
    private void OnOpenAscendancyClicked()
    {
        if (currentCharacter == null)
        {
            currentCharacter = CharacterManager.Instance?.GetCurrentCharacter();
            if (currentCharacter == null)
            {
                Debug.LogWarning("[CharacterStatsController] Cannot open Ascendancy panel - no character found");
                return;
            }
        }
        
        // Ensure character has ascendancyProgress
        if (currentCharacter.ascendancyProgress == null)
        {
            currentCharacter.ascendancyProgress = new CharacterAscendancyProgress();
        }
        
        // Get AscendancyDatabase
        var ascendancyDB = AscendancyDatabase.Instance;
        if (ascendancyDB == null)
        {
            Debug.LogError("[CharacterStatsController] AscendancyDatabase not found!");
            return;
        }
        
        // Get the character's selected Ascendancy
        string ascendancyName = currentCharacter.ascendancyProgress.selectedAscendancy;
        if (string.IsNullOrEmpty(ascendancyName))
        {
            Debug.LogWarning("[CharacterStatsController] Character has no Ascendancy selected. Cannot open panel.");
            return;
        }
        
        // Get Ascendancy data
        var ascendancy = ascendancyDB.GetAscendancy(ascendancyName);
        if (ascendancy == null)
        {
            Debug.LogError($"[CharacterStatsController] Ascendancy '{ascendancyName}' not found in database!");
            return;
        }
        
        // Show the Ascendancy panel with point allocation enabled
        if (ascendancyDisplayPanel != null)
        {
            // Show the panel with point allocation enabled (from stats panel, user can allocate points)
            ascendancyDisplayPanel.ShowAscendancy(ascendancy, currentCharacter.ascendancyProgress, allowAllocation: true);
            
            Debug.Log($"[CharacterStatsController] Opened Ascendancy panel for {ascendancyName} with point allocation enabled");
        }
        else
        {
            Debug.LogError("[CharacterStatsController] AscendancyDisplayPanel not found! Cannot open Ascendancy panel.");
        }
    }
}
