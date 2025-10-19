using UnityEngine;
using System.Collections;

/// <summary>
/// Extension class to add animation support to existing CombatManager.
/// Extends CombatManager functionality without modifying the original file.
/// </summary>
public static class CombatManagerAnimationExtensions
{
    /// <summary>
    /// Play card with animations
    /// </summary>
    public static void PlayCardAnimated(this CombatManager combatManager, Card card, GameObject cardObject, Vector3 targetPosition)
    {
        if (!combatManager.CanPlayCard(card))
        {
            Debug.LogWarning($"Cannot play card {card.cardName}");
            return;
        }
        
        // Get animation manager
        CombatAnimationManager animManager = CombatAnimationManager.Instance;
        
        if (animManager != null && cardObject != null)
        {
            // Animate card being played
            animManager.AnimateCardPlay(cardObject, targetPosition, () => {
                // Execute card logic after animation
                ExecuteCardLogic(combatManager, card);
            });
        }
        else
        {
            // Fallback to normal play if no animation manager
            combatManager.PlayCard(card);
        }
    }
    
    private static void ExecuteCardLogic(CombatManager combatManager, Card card)
    {
        // Remove from hand
        combatManager.GetCurrentHand().Remove(card);
        
        // Spend mana
        combatManager.playerCharacter.UseMana(card.manaCost);
        
        // Calculate damage
        float damage = DamageCalculator.CalculateCardDamage(card, combatManager.playerCharacter, combatManager.playerCharacter.weapons.meleeWeapon);
        
        CombatAnimationManager animManager = CombatAnimationManager.Instance;
        
        // Handle AoE vs single target
        if (card.isAoE)
        {
            // AoE card hits multiple enemies
            PlayAoECardAnimated(combatManager, card, damage);
        }
        else
        {
            // Single target card
            PlaySingleTargetCardAnimated(combatManager, card, damage);
        }
        
        // Add to discard pile
        combatManager.discardPile.Add(card);
        
        // Check if any enemies are defeated
        CheckEnemyDefeatsAnimated(combatManager);
        
        // Update UI through CombatUI if available
        CombatUI combatUI = GameObject.FindFirstObjectByType<CombatUI>();
        if (combatUI != null)
        {
            combatUI.UpdateCombatUI();
        }
    }
    
    private static void PlaySingleTargetCardAnimated(CombatManager combatManager, Card card, float damage)
    {
        if (combatManager.selectedEnemyIndex < combatManager.enemies.Count && combatManager.enemies[combatManager.selectedEnemyIndex] != null)
        {
            Enemy target = combatManager.enemies[combatManager.selectedEnemyIndex];
            Vector3 targetPosition = GetEnemyWorldPosition(combatManager, combatManager.selectedEnemyIndex);
            
            // Apply damage
            target.TakeDamage(damage);
            
            // Show damage number
            CombatAnimationManager animManager = CombatAnimationManager.Instance;
            if (animManager != null)
            {
                DamageNumberType damageType = GetDamageNumberType(card.primaryDamageType);
                animManager.ShowDamageNumber(damage, targetPosition, damageType);
                
                // Screen shake for impact
                float shakeIntensity = Mathf.Clamp(damage / 50f, 0.3f, 1.5f);
                animManager.ShakeCamera(shakeIntensity);
            }
            
            Debug.Log($"{combatManager.playerCharacter.characterName} played {card.cardName} for {damage:F1} damage on {target.enemyName}!");
        }
    }
    
    private static void PlayAoECardAnimated(CombatManager combatManager, Card card, float damage)
    {
        int targetsHit = 0;
        int maxTargets = Mathf.Min(card.aoeTargets, combatManager.enemies.Count);
        
        CombatAnimationManager animManager = CombatAnimationManager.Instance;
        
        for (int i = 0; i < maxTargets; i++)
        {
            if (combatManager.enemies[i] != null && combatManager.enemies[i].IsAlive())
            {
                Enemy target = combatManager.enemies[i];
                Vector3 targetPosition = GetEnemyWorldPosition(combatManager, i);
                
                // Apply damage
                target.TakeDamage(damage);
                
                // Show damage number with delay for readability
                if (animManager != null)
                {
                    float delay = i * 0.1f; // Stagger damage numbers
                    StartDelayedDamageNumber(animManager, damage, targetPosition, GetDamageNumberType(card.primaryDamageType), delay);
                }
                
                targetsHit++;
                Debug.Log($"{combatManager.playerCharacter.characterName} played {card.cardName} for {damage:F1} damage on {target.enemyName}!");
            }
        }
        
        if (targetsHit > 0)
        {
            // Screen shake for AoE impact
            if (animManager != null)
            {
                float shakeIntensity = Mathf.Clamp((damage * targetsHit) / 100f, 0.5f, 2f);
                animManager.ShakeCamera(shakeIntensity);
            }
            
            Debug.Log($"{card.cardName} hit {targetsHit} enemies for {damage:F1} damage each!");
        }
    }
    
    private static void CheckEnemyDefeatsAnimated(CombatManager combatManager)
    {
        for (int i = combatManager.enemies.Count - 1; i >= 0; i--)
        {
            if (!combatManager.enemies[i].IsAlive())
            {
                Debug.Log($"{combatManager.enemies[i].enemyName} has been defeated!");
                
                // TODO: Add defeat animation here (fade out, fall, etc.)
                
                combatManager.enemies.RemoveAt(i);
                
                // Adjust selected enemy index if needed
                if (combatManager.selectedEnemyIndex >= combatManager.enemies.Count)
                {
                    combatManager.selectedEnemyIndex = Mathf.Max(0, combatManager.enemies.Count - 1);
                }
                
                // Update current enemy for backward compatibility
                if (combatManager.enemies.Count > 0)
                {
                    combatManager.currentEnemy = combatManager.enemies[combatManager.selectedEnemyIndex];
                }
                else
                {
                    combatManager.currentEnemy = null;
                }
            }
        }
        
        // Check if all enemies are defeated
        if (combatManager.enemies.Count == 0)
        {
            // End combat with victory animation
            // combatManager.EndCombat(true);
        }
    }
    
    private static void StartDelayedDamageNumber(CombatAnimationManager animManager, float damage, Vector3 position, DamageNumberType type, float delay)
    {
        // Use coroutine for delay
        animManager.StartCoroutine(DelayedDamageNumberCoroutine(animManager, damage, position, type, delay));
    }
    
    private static IEnumerator DelayedDamageNumberCoroutine(CombatAnimationManager animManager, float damage, Vector3 position, DamageNumberType type, float delay)
    {
        yield return new WaitForSeconds(delay);
        animManager.ShowDamageNumber(damage, position, type);
    }
    
    private static DamageNumberType GetDamageNumberType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Fire:
                return DamageNumberType.Fire;
            case DamageType.Cold:
                return DamageNumberType.Cold;
            case DamageType.Lightning:
                return DamageNumberType.Lightning;
            case DamageType.Physical:
            default:
                return DamageNumberType.Normal;
        }
    }
    
    private static Vector3 GetEnemyWorldPosition(CombatManager combatManager, int enemyIndex)
    {
        // Try to get enemy position from UI
        // For now, return a calculated position based on screen layout
        // You'll want to adjust this based on your actual UI layout
        
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.65f, 0);
        float spacing = Screen.width * 0.2f;
        float startX = screenCenter.x - spacing;
        
        Vector3 enemyScreenPos = new Vector3(
            startX + (enemyIndex * spacing),
            screenCenter.y,
            0
        );
        
        // Convert to world position
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            return mainCam.ScreenToWorldPoint(new Vector3(enemyScreenPos.x, enemyScreenPos.y, 10f));
        }
        
        return enemyScreenPos;
    }
}

