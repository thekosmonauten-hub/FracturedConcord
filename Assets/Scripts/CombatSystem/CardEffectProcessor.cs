using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Processes card effects and applies them to targets (enemies/player).
/// Handles damage calculation, guard, status effects, etc.
/// </summary>
public class CardEffectProcessor : MonoBehaviour
{
    public static CardEffectProcessor Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private CombatDisplayManager combatManager;
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private CombatAnimationManager animationManager;
    
    [Header("Settings")]
    [SerializeField] private bool showDetailedLogs = true;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find references
        if (combatManager == null)
            combatManager = FindFirstObjectByType<CombatDisplayManager>();
        
        if (characterManager == null)
            characterManager = CharacterManager.Instance;
        
        if (animationManager == null)
            animationManager = CombatAnimationManager.Instance;
    }
    
    /// <summary>
    /// Apply a card's effect to a target enemy.
    /// </summary>
    public void ApplyCardToEnemy(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition)
    {
        Debug.Log($"<color=magenta>╔══════════════════════════════════╗</color>");
        Debug.Log($"<color=magenta>║ APPLYING CARD EFFECT DEBUG      ║</color>");
        Debug.Log($"<color=magenta>╚══════════════════════════════════╝</color>");
        
        if (card == null)
        {
            Debug.LogError("✗ Card is NULL! Cannot apply!");
            return;
        }
        
        if (targetEnemy == null)
        {
            Debug.LogError("✗ Target enemy is NULL! Cannot apply!");
            return;
        }
        
        Debug.Log($"✓ Card: {card.cardName} (Type: {card.cardType})");
        Debug.Log($"✓ Target: {targetEnemy.enemyName}");
        Debug.Log($"✓ Target HP BEFORE: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}");
        
        if (showDetailedLogs)
        {
            Debug.Log($"<color=cyan>═══ Applying {card.cardName} to {targetEnemy.enemyName} ═══</color>");
        }
        
        // Calculate damage based on card type
        switch (card.cardType)
        {
            case CardType.Attack:
                ApplyAttackCard(card, targetEnemy, player, targetScreenPosition);
                break;
            
            case CardType.Guard:
                ApplyGuardCard(card, player);
                break;
            
            case CardType.Skill:
                ApplySkillCard(card, targetEnemy, player, targetScreenPosition);
                break;
            
            case CardType.Power:
                ApplyPowerCard(card, player);
                break;
            
            default:
                Debug.LogWarning($"Unknown card type: {card.cardType}");
                break;
        }
    }
    
    /// <summary>
    /// Apply an attack card - deal damage to enemy.
    /// </summary>
    private void ApplyAttackCard(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition)
    {
        Debug.Log($"<color=yellow>→ Attack card detected!</color>");
        
        // Calculate total damage
        float totalDamage = CalculateDamage(card, player);
        
        Debug.Log($"  Base damage: {card.baseDamage}");
        Debug.Log($"  Total calculated damage: {totalDamage}");
        Debug.Log($"  Calling Enemy.TakeDamage({totalDamage})...");
        
        // Apply damage to enemy
        targetEnemy.TakeDamage(totalDamage);
        
        Debug.Log($"<color=red>  ⚔️ Dealt {totalDamage:F0} damage to {targetEnemy.enemyName}</color>");
        Debug.Log($"<color=red>  💔 Target HP AFTER: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}</color>");
        
        if (showDetailedLogs)
        {
            Debug.Log($"  ⚔️ Dealt {totalDamage:F0} damage to {targetEnemy.enemyName}");
            Debug.Log($"  💔 {targetEnemy.enemyName} HP: {targetEnemy.currentHealth}/{targetEnemy.maxHealth}");
        }
        
        // Show damage number
        if (animationManager != null)
        {
            // Convert DamageType to DamageNumberType
            DamageNumberType damageNumberType = ConvertDamageType(card.primaryDamageType);
            animationManager.ShowDamageNumber(totalDamage, targetScreenPosition, damageNumberType);
        }
        
        // Update enemy display to show new HP
        UpdateEnemyDisplay(targetEnemy);
        
        // Check if enemy is defeated
        if (targetEnemy.currentHealth <= 0)
        {
            Debug.Log($"<color=yellow>💀 {targetEnemy.enemyName} has been defeated!</color>");
            
            if (combatManager != null)
            {
                combatManager.OnEnemyDefeated?.Invoke(targetEnemy);
            }
        }
    }
    
    /// <summary>
    /// Update enemy display to show current HP.
    /// </summary>
    private void UpdateEnemyDisplay(Enemy enemy)
    {
        if (combatManager == null || combatManager.enemyDisplays == null) return;
        
        // Find the display for this enemy
        foreach (var enemyDisplay in combatManager.enemyDisplays)
        {
            if (enemyDisplay.GetCurrentEnemy() == enemy)
            {
                enemyDisplay.RefreshDisplay();
                Debug.Log($"  → Updated {enemyDisplay.name} health display");
                return;
            }
        }
    }
    
    /// <summary>
    /// Apply a guard card - add block to player.
    /// </summary>
    private void ApplyGuardCard(Card card, Character player)
    {
        float guardAmount = CalculateGuard(card, player);
        
        // Apply guard to player (you'll need to implement this in Character class)
        if (player != null)
        {
            // TODO: Add guard/block system to Character
            Debug.Log($"  🛡️ Player gained {guardAmount:F0} guard");
            
            // For now, just log it
            // Later: player.AddGuard(guardAmount);
        }
    }
    
    /// <summary>
    /// Apply a skill card - various effects.
    /// </summary>
    private void ApplySkillCard(Card card, Enemy targetEnemy, Character player, Vector3 targetScreenPosition)
    {
        // Skills can have both damage and other effects
        if (card.baseDamage > 0)
        {
            ApplyAttackCard(card, targetEnemy, player, targetScreenPosition);
        }
        
        if (card.baseGuard > 0)
        {
            ApplyGuardCard(card, player);
        }
        
        // TODO: Apply additional effects (draw cards, heal, etc.)
        Debug.Log($"  ✨ Skill effect applied: {card.cardName}");
    }
    
    /// <summary>
    /// Apply a power card - buff player.
    /// </summary>
    private void ApplyPowerCard(Card card, Character player)
    {
        // TODO: Implement buff system
        Debug.Log($"  💪 Power card applied: {card.cardName}");
    }
    
    /// <summary>
    /// Calculate total damage from a card + player stats.
    /// </summary>
    private float CalculateDamage(Card card, Character player)
    {
        float baseDamage = card.baseDamage;
        
        if (player == null)
        {
            return baseDamage;
        }
        
        // Add weapon damage if card scales with weapons
        // TODO: Implement weapon system
        // For now, weapon scaling is skipped
        
        // Add attribute scaling
        if (card.damageScaling != null)
        {
            float strBonus = player.strength * card.damageScaling.strengthScaling;
            float dexBonus = player.dexterity * card.damageScaling.dexterityScaling;
            float intBonus = player.intelligence * card.damageScaling.intelligenceScaling;
            
            baseDamage += strBonus + dexBonus + intBonus;
            
            if (showDetailedLogs && (strBonus + dexBonus + intBonus > 0))
            {
                Debug.Log($"    • Attribute scaling: +{strBonus + dexBonus + intBonus:F1}");
            }
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// Calculate total guard from a card + player stats.
    /// </summary>
    private float CalculateGuard(Card card, Character player)
    {
        float baseGuard = card.baseGuard;
        
        if (player == null)
        {
            return baseGuard;
        }
        
        // Add attribute scaling for guard
        if (card.guardScaling != null)
        {
            float strBonus = player.strength * card.guardScaling.strengthScaling;
            float dexBonus = player.dexterity * card.guardScaling.dexterityScaling;
            float intBonus = player.intelligence * card.guardScaling.intelligenceScaling;
            
            baseGuard += strBonus + dexBonus + intBonus;
        }
        
        return baseGuard;
    }
    
    /// <summary>
    /// Get target position for enemy (for animation target).
    /// </summary>
    public Vector3 GetEnemyScreenPosition(Enemy enemy)
    {
        if (combatManager == null || combatManager.enemyDisplays == null)
        {
            // Fallback position
            return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
        }
        
        // Find the display for this enemy
        foreach (var enemyDisplay in combatManager.enemyDisplays)
        {
            if (enemyDisplay.GetCurrentEnemy() == enemy)
            {
                // Return the screen position of the enemy display
                RectTransform rectTransform = enemyDisplay.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    return rectTransform.position;
                }
            }
        }
        
        // Fallback
        return new Vector3(Screen.width * 0.7f, Screen.height * 0.6f, 0);
    }
    
    /// <summary>
    /// Get the first available enemy (for auto-targeting).
    /// </summary>
    public Enemy GetFirstAvailableEnemy()
    {
        if (combatManager == null)
            return null;
        
        List<Enemy> activeEnemies = combatManager.GetActiveEnemies();
        if (activeEnemies != null && activeEnemies.Count > 0)
        {
            return activeEnemies[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// Convert DamageType to DamageNumberType for animation.
    /// </summary>
    private DamageNumberType ConvertDamageType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                return DamageNumberType.Normal;
            case DamageType.Fire:
                return DamageNumberType.Fire;
            case DamageType.Cold:
                return DamageNumberType.Cold;
            case DamageType.Lightning:
                return DamageNumberType.Lightning;
            case DamageType.Chaos:
                return DamageNumberType.Normal; // Chaos uses Normal for now
            default:
                return DamageNumberType.Normal;
        }
    }
}

