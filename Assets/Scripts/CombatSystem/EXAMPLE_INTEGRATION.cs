using UnityEngine;
using UIImage = UnityEngine.UI.Image;

/// <summary>
/// EXAMPLE: How to integrate CombatAnimationManager with your existing combat system.
/// This file shows practical examples - copy what you need!
/// 
/// IMPORTANT NOTE: Enemy and Character are data classes (not MonoBehaviours),
/// so they don't have transform properties. Pass screen positions from your UI instead.
/// </summary>
public class EXAMPLE_INTEGRATION : MonoBehaviour
{
    // =====================================================
    // EXAMPLE 1: Basic Damage Number When Enemy Takes Damage
    // =====================================================
    
    public void Example_ShowDamageOnEnemy(Enemy enemy, Vector3 enemyScreenPosition, float damage)
    {
        // Get the animation manager
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Show floating damage number at enemy screen position
        // Note: Enemy is a data class, not a MonoBehaviour, so pass position from UI
        animManager.ShowDamageNumber(
            damage,
            enemyScreenPosition,  // Get from UI element position
            DamageNumberType.Normal
        );
        
        // Add screen shake for impact
        float shakeIntensity = Mathf.Clamp(damage / 50f, 0.3f, 1.5f);
        animManager.ShakeCamera(shakeIntensity);
    }
    
    // =====================================================
    // EXAMPLE 2: Critical Hit with Extra Flair
    // =====================================================
    
    public void Example_ShowCriticalHit(Enemy enemy, Vector3 enemyScreenPosition, float damage)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Show CRITICAL damage with special type
        animManager.ShowDamageNumber(
            damage,
            enemyScreenPosition,
            DamageNumberType.Critical  // Yellow color, bigger, pulse effect
        );
        
        // Bigger screen shake for crits
        animManager.ShakeCamera(1.5f);
        
        // Optional: Zoom effect for emphasis
        animManager.ZoomCamera(0.95f, 0.4f);
    }
    
    // =====================================================
    // EXAMPLE 3: Healing Animation
    // =====================================================
    
    public void Example_ShowHealing(Character player, Vector3 playerScreenPosition, float healAmount)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Show green healing number
        // Note: Character is a data class, so pass screen position from UI
        animManager.ShowDamageNumber(
            healAmount,
            playerScreenPosition,
            DamageNumberType.Heal  // Green color for heals
        );
    }
    
    // =====================================================
    // EXAMPLE 4: Smooth Health Bar Update
    // =====================================================
    
    public void Example_UpdateHealthBar(UIImage healthBarFill, float currentHealth, float maxHealth)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null)
        {
            // Fallback: instant update
            healthBarFill.fillAmount = currentHealth / maxHealth;
            return;
        }
        
        // Smooth animated health bar
        animManager.AnimateHealthBar(healthBarFill, currentHealth, maxHealth);
    }
    
    // =====================================================
    // EXAMPLE 5: Card Play Animation
    // =====================================================
    
    public void Example_PlayCard(GameObject cardObject, Vector3 targetPosition, Card card)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Animate card flying to target
        animManager.AnimateCardPlay(
            cardObject,
            targetPosition,
            onComplete: () => {
                // After animation completes, apply card effect
                ApplyCardEffect(card);
            }
        );
    }
    
    // =====================================================
    // EXAMPLE 6: Turn Transition
    // =====================================================
    
    public void Example_TransitionToPlayerTurn(GameObject turnIndicator)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Animate turn indicator
        animManager.AnimateTurnTransition(
            turnIndicator,
            "YOUR TURN",
            Color.yellow,
            onComplete: () => {
                // Start player turn after animation
                Debug.Log("Player turn started!");
            }
        );
    }
    
    // =====================================================
    // EXAMPLE 7: AoE Damage with Staggered Numbers
    // =====================================================
    
    public void Example_AoEDamage(Enemy[] enemies, Vector3[] enemyScreenPositions, float damagePerEnemy)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Show damage on each enemy with a delay
        for (int i = 0; i < enemies.Length; i++)
        {
            int index = i; // Capture for closure
            float delay = i * 0.1f; // Stagger by 0.1s each
            
            // Use coroutine or LeanTween delay
            LeanTween.delayedCall(delay, () => {
                animManager.ShowDamageNumber(
                    damagePerEnemy,
                    enemyScreenPositions[index],  // Get from UI element
                    DamageNumberType.Fire  // Fire AoE attack
                );
            });
        }
        
        // Big screen shake for AoE
        animManager.ShakeCamera(2f);
    }
    
    // =====================================================
    // EXAMPLE 8: Complete Combat Turn with All Animations
    // =====================================================
    
    public void Example_CompleteAttackSequence(Card card, GameObject cardObject, Enemy targetEnemy, Vector3 enemyScreenPosition)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Step 1: Play card animation
        animManager.AnimateCardPlay(
            cardObject,
            enemyScreenPosition,  // Animate to enemy UI position
            onComplete: () => {
                // Step 2: Calculate and apply damage
                float damage = CalculateDamage(card);
                targetEnemy.TakeDamage(damage);
                
                // Step 3: Show damage number
                bool isCrit = Random.value > 0.8f;
                animManager.ShowDamageNumber(
                    damage,
                    enemyScreenPosition,
                    isCrit ? DamageNumberType.Critical : DamageNumberType.Normal
                );
                
                // Step 4: Screen shake
                animManager.ShakeCamera(isCrit ? 1.5f : 0.8f);
                
                // Step 5: Update health bar
                UIImage enemyHealthBar = GetEnemyHealthBar(targetEnemy);
                if (enemyHealthBar != null)
                {
                    animManager.AnimateHealthBar(
                        enemyHealthBar,
                        targetEnemy.currentHealth,
                        targetEnemy.maxHealth
                    );
                }
            }
        );
    }
    
    // =====================================================
    // EXAMPLE 9: Hovering Over Card
    // =====================================================
    
    public void Example_CardHoverEffect(GameObject cardObject, bool isHovering)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Scale and lift card when hovering
        animManager.AnimateCardHover(cardObject, isHovering);
    }
    
    // =====================================================
    // EXAMPLE 10: Drawing Cards from Deck
    // =====================================================
    
    public void Example_DrawCards(int count, Vector3 deckPosition, Vector3[] handPositions)
    {
        var animManager = CombatAnimationManager.Instance;
        if (animManager == null) return;
        
        // Draw each card with a delay
        for (int i = 0; i < count; i++)
        {
            int index = i;
            float delay = i * 0.15f; // Stagger card draws
            
            LeanTween.delayedCall(delay, () => {
                GameObject cardObject = CreateCardObject();
                
                animManager.AnimateCardDraw(
                    cardObject,
                    deckPosition,
                    handPositions[index],
                    onComplete: () => {
                        Debug.Log($"Card {index} drawn!");
                    }
                );
            });
        }
    }
    
    // =====================================================
    // HELPER METHODS (Placeholders - implement your own)
    // =====================================================
    
    private void ApplyCardEffect(Card card)
    {
        Debug.Log($"Applying card effect: {card.cardName}");
    }
    
    private float CalculateDamage(Card card)
    {
        return card.baseDamage + Random.Range(0, 10);
    }
    
    private UIImage GetEnemyHealthBar(Enemy enemy)
    {
        // Your code to get the health bar UI element
        // Example: Find it via CombatUI or store references
        return null;
    }
    
    // =====================================================
    // HELPER: Get Enemy Screen Position from UI
    // =====================================================
    
    private Vector3 GetEnemyScreenPosition(int enemyIndex)
    {
        // Example: Get position from your CombatUI
        // You would get this from the actual UI element
        
        // Option 1: From RectTransform of UI element
        // RectTransform enemyUI = GetEnemyUIElement(enemyIndex);
        // return enemyUI.position;
        
        // Option 2: Calculate based on layout
        Vector3 screenCenter = new Vector3(Screen.width * 0.7f, Screen.height * 0.7f, 0);
        float spacing = Screen.width * 0.15f;
        return new Vector3(
            screenCenter.x + (enemyIndex - 1) * spacing,
            screenCenter.y,
            0
        );
    }
    
    private Vector3 GetPlayerScreenPosition()
    {
        // Example: Get from your player UI element
        // RectTransform playerUI = GetPlayerUIElement();
        // return playerUI.position;
        
        // Fallback calculation
        return new Vector3(Screen.width * 0.25f, Screen.height * 0.3f, 0);
    }
    
    private GameObject CreateCardObject()
    {
        // Your code to instantiate a card
        return new GameObject("Card");
    }
}

