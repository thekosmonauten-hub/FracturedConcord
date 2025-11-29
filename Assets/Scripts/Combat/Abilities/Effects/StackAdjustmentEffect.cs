using UnityEngine;

/// <summary>
/// Ability effect that applies a StackAdjustmentDefinition without attaching a status effect.
/// Useful for enemies or passives that only modify Agitate/Tolerance/Potential stacks.
/// </summary>
[CreateAssetMenu(fileName = "StackAdjustmentEffect", menuName = "Dexiled/Enemies/Effects/Stack Adjustment")]
public class StackAdjustmentEffect : AbilityEffect
{
    [Header("Stack Adjustment")]
    public StackAdjustmentDefinition adjustment;

    [Header("Targeting Overrides")]
    [Tooltip("Force the adjustment to apply to the caster even if the ability target is the player.")]
    public bool forceApplyToSelf = false;
    [Tooltip("Apply to both self and player targets when true.")]
    public bool applyToSelfAndPlayer = false;

    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null || adjustment == null) return;

        bool applyToSelf = forceApplyToSelf || ctx.target == AbilityTarget.Self || applyToSelfAndPlayer;
        bool applyToPlayer = (!forceApplyToSelf && (ctx.target == AbilityTarget.Player || ctx.target == AbilityTarget.AllPlayers)) || applyToSelfAndPlayer;

        if (applyToSelf)
        {
            ApplyToEnemy(ctx);
        }

        if (applyToPlayer)
        {
            ApplyToPlayer(ctx);
        }
    }

    private void ApplyToEnemy(AbilityContext ctx)
    {
        StackAdjustmentDefinition runtimeAdjustment = adjustment.Clone();

        if (ctx.display != null)
        {
            ctx.display.ApplyStackAdjustment(runtimeAdjustment);
            return;
        }

        if (ctx.enemyRuntime != null)
        {
            var displays = Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var display in displays)
            {
                if (display != null && display.GetCurrentEnemy() == ctx.enemyRuntime)
                {
                    display.ApplyStackAdjustment(runtimeAdjustment);
                    return;
                }
            }
        }

        Debug.LogWarning("[StackAdjustmentEffect] Unable to locate enemy display to apply stack adjustment.");
    }

    private void ApplyToPlayer(AbilityContext ctx)
    {
        StackAdjustmentDefinition runtimeAdjustment = adjustment.Clone();

        PlayerCombatDisplay playerDisplay = ctx.combat != null ? ctx.combat.playerDisplay : null;
        if (playerDisplay == null)
        {
            playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
        }

        if (playerDisplay == null)
        {
            Debug.LogWarning("[StackAdjustmentEffect] Unable to locate PlayerCombatDisplay to apply stack adjustment.");
            return;
        }

        playerDisplay.ApplyStackAdjustment(runtimeAdjustment);
    }
}

