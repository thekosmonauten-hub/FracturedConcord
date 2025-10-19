using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper component to integrate CircularHealthBar with AnimatedCombatUI.
/// Attach this alongside AnimatedCombatUI to use circular bars instead of standard fill bars.
/// </summary>
public class CircularBarIntegration : MonoBehaviour
{
    [Header("Circular Bars")]
    [SerializeField] private CircularHealthBar playerHealthCircular;
    [SerializeField] private CircularHealthBar playerManaCircular;
    [SerializeField] private CircularHealthBar[] enemyHealthCircular = new CircularHealthBar[3];
    
    [Header("References")]
    [SerializeField] private CombatManager combatManager;
    
    [Header("Effects")]
    [SerializeField] private bool flashOnDamage = true;
    [SerializeField] private bool shakeOnDamage = true;
    [SerializeField] private bool flashOnHeal = true;
    
    // State tracking
    private float lastPlayerHealth;
    private float lastPlayerMana;
    private float[] lastEnemyHealth = new float[3];
    
    private void Start()
    {
        if (combatManager == null)
            combatManager = FindFirstObjectByType<CombatManager>();
        
        InitializeState();
    }
    
    private void InitializeState()
    {
        if (combatManager == null || combatManager.playerCharacter == null) return;
        
        Character player = combatManager.playerCharacter;
        lastPlayerHealth = player.currentHealth;
        lastPlayerMana = player.mana;
        
        for (int i = 0; i < combatManager.enemies.Count && i < 3; i++)
        {
            lastEnemyHealth[i] = combatManager.enemies[i].currentHealth;
        }
    }
    
    private void Update()
    {
        if (combatManager == null || combatManager.playerCharacter == null) return;
        
        UpdatePlayerBars();
        UpdateEnemyBars();
    }
    
    private void UpdatePlayerBars()
    {
        Character player = combatManager.playerCharacter;
        
        // Update health
        if (player.currentHealth != lastPlayerHealth)
        {
            if (playerHealthCircular != null)
            {
                playerHealthCircular.SetFillAmount(player.currentHealth, player.maxHealth);
                
                // Flash on change
                if (player.currentHealth < lastPlayerHealth && flashOnDamage)
                {
                    playerHealthCircular.Flash(Color.red, 0.2f);
                }
                else if (player.currentHealth > lastPlayerHealth && flashOnHeal)
                {
                    playerHealthCircular.Flash(Color.green, 0.3f);
                }
                
                // Shake on damage
                if (player.currentHealth < lastPlayerHealth && shakeOnDamage)
                {
                    float damage = lastPlayerHealth - player.currentHealth;
                    float intensity = Mathf.Clamp(damage / 5f, 5f, 15f);
                    playerHealthCircular.Shake(intensity);
                }
            }
            
            lastPlayerHealth = player.currentHealth;
        }
        
        // Update mana
        if (player.mana != lastPlayerMana)
        {
            if (playerManaCircular != null)
            {
                playerManaCircular.SetFillAmount(player.mana, player.maxMana);
            }
            
            lastPlayerMana = player.mana;
        }
    }
    
    private void UpdateEnemyBars()
    {
        for (int i = 0; i < combatManager.enemies.Count && i < enemyHealthCircular.Length; i++)
        {
            Enemy enemy = combatManager.enemies[i];
            
            if (enemy.currentHealth != lastEnemyHealth[i])
            {
                if (enemyHealthCircular[i] != null)
                {
                    enemyHealthCircular[i].SetFillAmount(enemy.currentHealth, enemy.maxHealth);
                    
                    // Flash on damage
                    if (enemy.currentHealth < lastEnemyHealth[i] && flashOnDamage)
                    {
                        enemyHealthCircular[i].Flash(Color.white, 0.15f);
                    }
                    
                    // Shake on damage
                    if (enemy.currentHealth < lastEnemyHealth[i] && shakeOnDamage)
                    {
                        float damage = lastEnemyHealth[i] - enemy.currentHealth;
                        float intensity = Mathf.Clamp(damage / 3f, 8f, 20f);
                        enemyHealthCircular[i].Shake(intensity);
                    }
                }
                
                lastEnemyHealth[i] = enemy.currentHealth;
            }
        }
    }
    
    /// <summary>
    /// Manually trigger health bar update (call after instantaneous health changes)
    /// </summary>
    public void ForceUpdateBars()
    {
        if (combatManager == null || combatManager.playerCharacter == null) return;
        
        Character player = combatManager.playerCharacter;
        
        if (playerHealthCircular != null)
            playerHealthCircular.SetFillAmount(player.currentHealth, player.maxHealth);
        
        if (playerManaCircular != null)
            playerManaCircular.SetFillAmount(player.mana, player.maxMana);
        
        for (int i = 0; i < combatManager.enemies.Count && i < enemyHealthCircular.Length; i++)
        {
            if (enemyHealthCircular[i] != null)
            {
                Enemy enemy = combatManager.enemies[i];
                enemyHealthCircular[i].SetFillAmount(enemy.currentHealth, enemy.maxHealth);
            }
        }
    }
}

