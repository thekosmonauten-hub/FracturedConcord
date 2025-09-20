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
    }
    
    private void InitializeUI()
    {
        // Set up experience slider (only slider used)
        if (experienceSlider != null)
        {
            experienceSlider.minValue = 0f;
            experienceSlider.maxValue = 1f;
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
        
        Debug.Log($"[CharacterStatsController] Updated stats for {character.characterName}");
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
}
