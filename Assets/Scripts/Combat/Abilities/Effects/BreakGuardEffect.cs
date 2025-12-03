using UnityEngine;

/// <summary>
/// Removes all Guard from the target and optionally prevents Guard gain
/// Used for abilities like Shatterflail: "Breaks armor-type defenses"
/// </summary>
[CreateAssetMenu(fileName = "BreakGuardEffect", menuName = "Dexiled/Enemies/Effects/Break Guard")]
public class BreakGuardEffect : AbilityEffect
{
    [Header("Break Guard Settings")]
    [Tooltip("If true, also prevents Guard gain next turn")]
    public bool preventGuardNextTurn = true;
    
    [Tooltip("Duration in turns to prevent Guard gain (only if preventGuardNextTurn is true)")]
    public int preventionDuration = 1;
    
    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null) return;
        
        // Target player by default
        var playerDisplay = UnityEngine.Object.FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay == null)
        {
            Debug.LogWarning("[BreakGuardEffect] PlayerCombatDisplay not found!");
            return;
        }
        
        var characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[BreakGuardEffect] CharacterManager or Character not found!");
            return;
        }
        
        Character player = characterManager.GetCurrentCharacter();
        
        // Remove all current Guard
        float guardRemoved = player.currentGuard;
        player.currentGuard = 0f;
        
        Debug.Log($"<color=orange>[BreakGuard] Shattered all Guard! Removed {guardRemoved:F1} guard.</color>");
        
        // Update display
        playerDisplay.UpdateGuardDisplay();
        
        // Show combat message
        var combatUI = UnityEngine.Object.FindFirstObjectByType<AnimatedCombatUI>();
        if (combatUI != null)
        {
            combatUI.LogMessage($"<color=orange>Guard Shattered!</color> All {guardRemoved:F0} guard destroyed!");
        }
        
        // Optionally prevent Guard gain next turn
        if (preventGuardNextTurn)
        {
            var statusManager = playerDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                // Apply a custom "Shattered Defense" debuff that prevents Guard
                StatusEffect shatteredDefense = new StatusEffect(
                    StatusEffectType.Bind, // Reuse Bind for now, or could create new type
                    "Shattered Defense",
                    1f,
                    preventionDuration
                );
                
                statusManager.AddStatusEffect(shatteredDefense);
                Debug.Log($"[BreakGuard] Applied Shattered Defense - cannot gain Guard for {preventionDuration} turn(s)");
                
                if (combatUI != null)
                {
                    combatUI.LogMessage($"<color=grey>Defense Shattered!</color> Cannot gain guard next turn.");
                }
            }
        }
    }
}

