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
    
    [Header("Colors")]
    public Color healthColor = Color.red;
    public Color attackIntentColor = Color.red;
    public Color defendIntentColor = Color.blue;
    
    [Header("Intent Icons")]
    public Sprite attackIcon;
    public Sprite defendIcon;
    
    private Enemy currentEnemy;
    private bool isInitialized = false;
    
    private void Start()
    {
        InitializeDisplay();
    }
    
    private void InitializeDisplay()
    {
        // Auto-find components if not assigned
        if (enemyNameText == null)
            enemyNameText = transform.Find("EnemyName")?.GetComponent<TextMeshProUGUI>();
            
        if (enemyTypeText == null)
            enemyTypeText = transform.Find("EnemyType")?.GetComponent<TextMeshProUGUI>();
            
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
        
        // Create test enemy if none assigned
        if (currentEnemy == null)
        {
            CreateTestEnemy();
        }
    }
    
    private void CreateTestEnemy()
    {
        // Create a test enemy for display purposes
        currentEnemy = new Enemy("Test Goblin", 50, 8);
        currentEnemy.SetIntent();
        UpdateDisplay();
    }
    
    public void SetEnemy(Enemy enemy)
    {
        currentEnemy = enemy;
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
            
        if (enemyTypeText != null)
            enemyTypeText.text = "Enemy"; // Could be expanded to show enemy type/class
        
        // Update health
        UpdateHealthDisplay();
        
        // Update intent
        UpdateIntentDisplay();
        
        // Update status effects
        UpdateStatusEffects();
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
        // Clear existing status effects
        if (statusEffectsContainer != null)
        {
            foreach (Transform child in statusEffectsContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Add status effects here when implemented
        // For now, this is a placeholder for future status effect system
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
