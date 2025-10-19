using UnityEngine;
using UnityEngine.UIElements;
using UIImage = UnityEngine.UI.Image;  // Alias to avoid conflict with UIElements.Image
using UIText = UnityEngine.UI.Text;

/// <summary>
/// Integrates CombatAnimationManager with CombatUI for smooth animated updates.
/// Attach this to the same GameObject as CombatUI.
/// </summary>
[RequireComponent(typeof(CombatUI))]
public class CombatUIAnimationIntegration : MonoBehaviour
{
    private CombatUI combatUI;
    private CombatAnimationManager animManager;
    
    [Header("UI Element References (Old UI System)")]
    [Tooltip("Reference to player health bar Image (for old UI system)")]
    public UIImage playerHealthBarImage;
    
    [Tooltip("Reference to player mana bar Image (for old UI system)")]
    public UIImage playerManaBarImage;
    
    [Tooltip("References to enemy health bar Images")]
    public UIImage[] enemyHealthBarImages = new UIImage[3];
    
    [Header("Turn Indicator")]
    public GameObject turnIndicatorObject;
    
    private void Awake()
    {
        combatUI = GetComponent<CombatUI>();
        animManager = CombatAnimationManager.Instance;
        
        if (animManager == null)
        {
            Debug.LogWarning("CombatAnimationManager not found in scene! Animations will not play.");
        }
    }
    
    /// <summary>
    /// Animate player health bar update
    /// </summary>
    public void AnimatePlayerHealth(float currentHealth, float maxHealth)
    {
        if (animManager != null && playerHealthBarImage != null)
        {
            animManager.AnimateHealthBar(playerHealthBarImage, currentHealth, maxHealth);
        }
    }
    
    /// <summary>
    /// Animate player mana bar update
    /// </summary>
    public void AnimatePlayerMana(float currentMana, float maxMana)
    {
        if (animManager != null && playerManaBarImage != null)
        {
            animManager.AnimateManaBar(playerManaBarImage, currentMana, maxMana);
        }
    }
    
    /// <summary>
    /// Animate enemy health bar update
    /// </summary>
    public void AnimateEnemyHealth(int enemyIndex, float currentHealth, float maxHealth)
    {
        if (animManager != null && enemyIndex >= 0 && enemyIndex < enemyHealthBarImages.Length)
        {
            UIImage healthBar = enemyHealthBarImages[enemyIndex];
            if (healthBar != null)
            {
                animManager.AnimateHealthBar(healthBar, currentHealth, maxHealth);
            }
        }
    }
    
    /// <summary>
    /// Animate turn transition
    /// </summary>
    public void AnimateTurnTransition(bool isPlayerTurn)
    {
        if (animManager != null && turnIndicatorObject != null)
        {
            string turnText = isPlayerTurn ? "YOUR TURN" : "ENEMY TURN";
            Color turnColor = isPlayerTurn ? Color.yellow : new Color(1f, 0.5f, 0.5f);
            
            animManager.AnimateTurnTransition(turnIndicatorObject, turnText, turnColor);
        }
    }
    
    /// <summary>
    /// Show damage number at enemy position
    /// </summary>
    public void ShowEnemyDamage(int enemyIndex, float damage, DamageNumberType type = DamageNumberType.Normal)
    {
        if (animManager != null)
        {
            Vector3 enemyPosition = GetEnemyScreenPosition(enemyIndex);
            animManager.ShowDamageNumber(damage, enemyPosition, type);
        }
    }
    
    /// <summary>
    /// Show heal number at player position
    /// </summary>
    public void ShowPlayerHeal(float healAmount)
    {
        if (animManager != null)
        {
            Vector3 playerPosition = GetPlayerScreenPosition();
            animManager.ShowDamageNumber(healAmount, playerPosition, DamageNumberType.Heal);
        }
    }
    
    /// <summary>
    /// Get enemy screen position for damage numbers
    /// </summary>
    private Vector3 GetEnemyScreenPosition(int enemyIndex)
    {
        // Try to get position from UI element
        if (enemyIndex >= 0 && enemyIndex < enemyHealthBarImages.Length && enemyHealthBarImages[enemyIndex] != null)
        {
            RectTransform rectTransform = enemyHealthBarImages[enemyIndex].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform.position;
            }
        }
        
        // Fallback: Calculate position based on screen layout
        Vector3 screenCenter = new Vector3(Screen.width * 0.7f, Screen.height * 0.7f, 0);
        float spacing = Screen.width * 0.15f;
        
        return new Vector3(
            screenCenter.x + (enemyIndex - 1) * spacing,
            screenCenter.y,
            0
        );
    }
    
    /// <summary>
    /// Get player screen position for heal numbers
    /// </summary>
    private Vector3 GetPlayerScreenPosition()
    {
        // Try to get position from UI element
        if (playerHealthBarImage != null)
        {
            RectTransform rectTransform = playerHealthBarImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform.position;
            }
        }
        
        // Fallback: Calculate position
        return new Vector3(Screen.width * 0.25f, Screen.height * 0.3f, 0);
    }
}

/// <summary>
/// Helper component to trigger animations when character stats change.
/// Attach to Character GameObject or use as a system component.
/// </summary>
public class CharacterAnimationTrigger : MonoBehaviour
{
    private Character character;
    private CombatUIAnimationIntegration uiAnimations;
    private float lastHealth;
    private float lastMana;
    
    private void Start()
    {
        character = GetComponent<Character>();
        uiAnimations = FindFirstObjectByType<CombatUIAnimationIntegration>();
        
        if (character != null)
        {
            lastHealth = character.currentHealth;
            lastMana = character.mana;
        }
    }
    
    private void Update()
    {
        if (character == null || uiAnimations == null) return;
        
        // Check for health changes
        if (character.currentHealth != lastHealth)
        {
            uiAnimations.AnimatePlayerHealth(character.currentHealth, character.maxHealth);
            
            // Show heal or damage number
            float difference = character.currentHealth - lastHealth;
            if (difference > 0)
            {
                uiAnimations.ShowPlayerHeal(difference);
            }
            
            lastHealth = character.currentHealth;
        }
        
        // Check for mana changes
        if (character.mana != lastMana)
        {
            uiAnimations.AnimatePlayerMana(character.mana, character.maxMana);
            lastMana = character.mana;
        }
    }
}

