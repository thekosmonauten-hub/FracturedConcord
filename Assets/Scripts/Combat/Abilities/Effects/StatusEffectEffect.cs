using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectEffect", menuName = "Dexiled/Enemies/Effects/Status")]
public class StatusEffectEffect : AbilityEffect
{
    [Header("Effect Settings")]
    public StatusEffectType statusType = StatusEffectType.Vulnerable;
    public float magnitude = 1f;
    public int durationTurns = 2;
    [Tooltip("Optional override for the effect name shown in tooltips. Leave empty to use the StatusEffectType name.")]
    public string overrideEffectName = "";
    [Tooltip("Marks the applied status as a debuff, affecting tooltip coloring and filters.")]
    public bool isDebuff = false;

[Header("Stack Adjustments")]
[Tooltip("Optional stack adjustment to apply alongside the status.")]
public StackAdjustmentDefinition stackAdjustment;
    [Tooltip("If disabled, only the stack adjustment is applied; no status effect is created.")]
    public bool applyStatusEffect = true;

    [Header("Inline Stack Overrides")]
    public bool useInlineStackSettings = false;
    public int inlineAgitateStacks;
    public int inlineToleranceStacks;
    public int inlinePotentialStacks;
    [Tooltip("Additive percentage (e.g., 50 = 50% increased).")]
    public float inlineAgitateIncreasedPercent;
    public float inlineToleranceIncreasedPercent;
    public float inlinePotentialIncreasedPercent;
    [Tooltip("Multiplicative bonus (e.g., 0.5 = 50% more).")]
    public float inlineAgitateMoreMultiplier;
    public float inlineToleranceMoreMultiplier;
    public float inlinePotentialMoreMultiplier;

    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null) return;

        StackAdjustmentDefinition adjustmentInstance = BuildEffectiveAdjustment();

        if (!applyStatusEffect)
        {
            ApplyStackOnly(adjustmentInstance, ctx);
            return;
        }

        string finalName = string.IsNullOrWhiteSpace(overrideEffectName) ? statusType.ToString() : overrideEffectName.Trim();
        var effect = new StatusEffect(statusType, finalName, magnitude, durationTurns, isDebuff)
        {
            stackAdjustment = adjustmentInstance
        };

        switch (ctx.target)
        {
            case AbilityTarget.Self:
                ApplyToEnemy(effect, ctx);
                break;
            case AbilityTarget.Player:
            case AbilityTarget.AllPlayers:
            default:
                ApplyToPlayer(effect, ctx);
                break;
        }
    }

    private void ApplyToEnemy(StatusEffect effect, AbilityContext ctx)
    {
        if (ctx.display != null)
        {
            ctx.display.AddStatusEffect(effect);
            return;
        }

        // Fallback: attempt to locate the display that owns the runtime enemy
        if (ctx.enemyRuntime != null)
        {
            var displays = Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var display in displays)
            {
                if (display != null && display.GetCurrentEnemy() == ctx.enemyRuntime)
                {
                    display.AddStatusEffect(effect);
                    return;
                }
            }
        }

        Debug.LogWarning($"[StatusEffectEffect] Unable to locate enemy display to apply {effect.effectName}.");
    }

    private void ApplyToPlayer(StatusEffect effect, AbilityContext ctx)
    {
        PlayerCombatDisplay playerDisplay = null;

        if (ctx.combat != null && ctx.combat.playerDisplay != null)
        {
            playerDisplay = ctx.combat.playerDisplay;
        }
        else
        {
            playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
        }

        if (playerDisplay == null)
        {
            Debug.LogWarning($"[StatusEffectEffect] Unable to locate PlayerCombatDisplay to apply {effect.effectName}.");
            return;
        }

        playerDisplay.AddStatusEffect(effect);
}

    private StackAdjustmentDefinition BuildEffectiveAdjustment()
    {
        bool hasInlineStacks = useInlineStackSettings &&
                               (inlineAgitateStacks != 0 || inlineToleranceStacks != 0 || inlinePotentialStacks != 0 ||
                                !Mathf.Approximately(inlineAgitateIncreasedPercent, 0f) ||
                                !Mathf.Approximately(inlineToleranceIncreasedPercent, 0f) ||
                                !Mathf.Approximately(inlinePotentialIncreasedPercent, 0f) ||
                                !Mathf.Approximately(inlineAgitateMoreMultiplier, 0f) ||
                                !Mathf.Approximately(inlineToleranceMoreMultiplier, 0f) ||
                                !Mathf.Approximately(inlinePotentialMoreMultiplier, 0f));

        if (stackAdjustment == null && !hasInlineStacks)
        {
            return null;
        }

        StackAdjustmentDefinition result = stackAdjustment != null
            ? stackAdjustment.Clone()
            : ScriptableObject.CreateInstance<StackAdjustmentDefinition>();

        if (hasInlineStacks)
        {
            result.agitateStacks += inlineAgitateStacks;
            result.toleranceStacks += inlineToleranceStacks;
            result.potentialStacks += inlinePotentialStacks;
            result.agitateIncreasedPercent += inlineAgitateIncreasedPercent;
            result.toleranceIncreasedPercent += inlineToleranceIncreasedPercent;
            result.potentialIncreasedPercent += inlinePotentialIncreasedPercent;
            result.agitateMoreMultiplier += inlineAgitateMoreMultiplier;
            result.toleranceMoreMultiplier += inlineToleranceMoreMultiplier;
            result.potentialMoreMultiplier += inlinePotentialMoreMultiplier;
        }

        return result;
    }

    private void ApplyStackOnly(StackAdjustmentDefinition adjustment, AbilityContext ctx)
    {
        if (adjustment == null) return;

        switch (ctx.target)
        {
            case AbilityTarget.Self:
                ApplyStackToEnemy(adjustment, ctx);
                break;
            case AbilityTarget.Player:
            case AbilityTarget.AllPlayers:
            default:
                ApplyStackToPlayer(adjustment, ctx);
                break;
        }
    }

    private void ApplyStackToEnemy(StackAdjustmentDefinition adjustment, AbilityContext ctx)
    {
        if (ctx.display != null)
        {
            ctx.display.ApplyStackAdjustment(adjustment);
            return;
        }

        if (ctx.enemyRuntime != null)
        {
            var displays = Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
            foreach (var display in displays)
            {
                if (display != null && display.GetCurrentEnemy() == ctx.enemyRuntime)
                {
                    display.ApplyStackAdjustment(adjustment);
                    return;
                }
            }
        }

        Debug.LogWarning("[StatusEffectEffect] Unable to locate enemy display to apply stack adjustment.");
    }

    private void ApplyStackToPlayer(StackAdjustmentDefinition adjustment, AbilityContext ctx)
    {
        PlayerCombatDisplay playerDisplay = ctx.combat != null ? ctx.combat.playerDisplay : null;
        if (playerDisplay == null)
        {
            playerDisplay = Object.FindFirstObjectByType<PlayerCombatDisplay>();
        }

        if (playerDisplay == null)
        {
            Debug.LogWarning("[StatusEffectEffect] Unable to locate PlayerCombatDisplay to apply stack adjustment.");
            return;
        }

        playerDisplay.ApplyStackAdjustment(adjustment);
    }
}
