using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCombatDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Image characterPortrait;
    public TextMeshProUGUI characterNameText;
    
    [Header("Health Display")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image healthFillImage;
    
    [Header("Energy Shield Display")]
    public Slider energyShieldSlider;
    public TextMeshProUGUI energyShieldText;
    public Image energyShieldFillImage;
    
    [Header("Mana Display")]
    public Slider manaSlider;
    public TextMeshProUGUI manaText;
    public Image manaFillImage;
    
    [Header("Guard Display")]
    public GameObject guardContainer;
    public TextMeshProUGUI guardText;
    
    [Header("Colors")]
    public Color healthColor = Color.red;
    public Color energyShieldColor = new Color(0.5f, 0.8f, 1f, 0.8f); // Light blue with transparency
    public Color manaColor = Color.blue;
    public Color guardColor = new Color(1f, 1f, 0f, 0.7f); // Yellow with transparency
    
    private Character currentCharacter;
    private CharacterManager characterManager;
    
    private void Start()
    {
        InitializeDisplay();
        SetupCharacterManager();
    }
    
    private void InitializeDisplay()
    {
        // Auto-find components if not assigned
        if (characterNameText == null)
            characterNameText = transform.Find("CharacterName")?.GetComponent<TextMeshProUGUI>();
            
        if (healthSlider == null)
            healthSlider = transform.Find("HealthBar")?.GetComponent<Slider>();
            
        if (healthText == null)
            healthText = transform.Find("HealthBar/HealthText")?.GetComponent<TextMeshProUGUI>();
            
        if (manaSlider == null)
            manaSlider = transform.Find("ManaBar")?.GetComponent<Slider>();
            
        if (energyShieldSlider == null)
            energyShieldSlider = transform.Find("HealthBar/EnergyShieldBar")?.GetComponent<Slider>();
            
        if (energyShieldText == null)
            energyShieldText = transform.Find("HealthBar/EnergyShieldText")?.GetComponent<TextMeshProUGUI>();
            
        if (manaText == null)
            manaText = transform.Find("ManaBar/ManaText")?.GetComponent<TextMeshProUGUI>();
            
        if (guardContainer == null)
            guardContainer = transform.Find("GuardContainer")?.gameObject;
            
        if (guardText == null)
            guardText = transform.Find("GuardContainer/GuardText")?.GetComponent<TextMeshProUGUI>();
        
        // Set up colors
        if (healthFillImage != null)
            healthFillImage.color = healthColor;
            
        if (energyShieldFillImage != null)
            energyShieldFillImage.color = energyShieldColor;
            
        if (manaFillImage != null)
            manaFillImage.color = manaColor;
    }
    
    private void SetupCharacterManager()
    {
        characterManager = CharacterManager.Instance;
        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found! Make sure it exists in the scene.");
            return;
        }
        
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
    }
    
    private void UpdateHealthDisplay()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = currentCharacter.maxHealth;
            healthSlider.value = currentCharacter.currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentCharacter.currentHealth}/{currentCharacter.maxHealth}";
        }
    }
    
    private void UpdateEnergyShieldDisplay()
    {
        if (energyShieldSlider != null)
        {
            // Energy Shield appears as an overlay on top of health
            energyShieldSlider.maxValue = currentCharacter.maxHealth;
            energyShieldSlider.value = currentCharacter.currentHealth + currentCharacter.currentEnergyShield;
        }
        
        if (energyShieldText != null)
        {
            if (currentCharacter.currentEnergyShield > 0)
            {
                energyShieldText.text = $"+{currentCharacter.currentEnergyShield}";
            }
            else
            {
                energyShieldText.text = "";
            }
        }
    }
    
    private void UpdateManaDisplay()
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = currentCharacter.maxMana;
            manaSlider.value = currentCharacter.mana;
        }
        
        if (manaText != null)
        {
            manaText.text = $"{currentCharacter.mana}/{currentCharacter.maxMana}";
        }
    }
    
    private void UpdateGuardDisplay()
    {
        if (guardContainer != null)
        {
            guardContainer.SetActive(currentCharacter.currentGuard > 0);
        }
        
        if (guardText != null && currentCharacter.currentGuard > 0)
        {
            guardText.text = $"Guard: {currentCharacter.currentGuard:F0}";
        }
    }
    
    // Event handlers
    private void OnCharacterDamaged(Character character)
    {
        UpdateHealthDisplay();
        UpdateEnergyShieldDisplay();
        UpdateGuardDisplay();
    }
    
    private void OnCharacterHealed(Character character)
    {
        UpdateHealthDisplay();
    }
    
    private void OnManaUsed(Character character)
    {
        UpdateManaDisplay();
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
