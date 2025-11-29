using UnityEngine;
using System.Collections.Generic;

public class EnemyAbilityRunner : MonoBehaviour
{
    private CombatDisplayManager combat;
    private CharacterManager cm;
    private CombatEffectManager vfx;
    private EnemyCombatDisplay display;
    private EnemyData data;
    private Enemy runtimeEnemy;

    private readonly Dictionary<string, int> cooldowns = new Dictionary<string, int>();
    private float currentPhase = 1f;
    private EnemyAbility queuedAbility;
    private string lastExecutedAbilityName;
    private bool lastExecutionConsumedTurn;
    private int lastPreviewDamage;

    public int LastPreviewDamage => lastPreviewDamage;

    public bool HasQueuedAbility => queuedAbility != null;
    public bool LastExecutionConsumedTurn => lastExecutionConsumedTurn;
    public string GetLastExecutedAbilityName() => lastExecutedAbilityName;

    private void Awake()
    {
        combat = FindFirstObjectByType<CombatDisplayManager>();
        cm = CharacterManager.Instance;
        vfx = CombatEffectManager.Instance;
        display = GetComponent<EnemyCombatDisplay>();
    }

    public void Initialize(Enemy runtime, EnemyData source)
    {
        runtimeEnemy = runtime;
        data = source;
        cooldowns.Clear();
        queuedAbility = null;
        if (data != null && data.abilities != null)
        {
            foreach (var a in data.abilities)
            {
                if (a == null || string.IsNullOrEmpty(a.id)) continue;
                cooldowns[a.id] = Mathf.Max(0, a.initialCooldown);
            }
        }
        OnSpawn();
    }

    public void OnSpawn() { TryAbilities(AbilityTrigger.OnSpawn); CheckPhaseGate(); }
    public void OnTurnStart()
    {
        TickCooldowns();
        RegenerateEnergy();
        TryAbilities(AbilityTrigger.OnTurnStart);
        CheckPhaseGate();
    }
    public void OnTurnEnd() { TryAbilities(AbilityTrigger.OnTurnEnd); CheckPhaseGate(); }
    public void OnAttack() { TryAbilities(AbilityTrigger.OnAttack); }
    public void OnDamaged() { TryAbilities(AbilityTrigger.OnDamaged); CheckPhaseGate(); }
    public void OnDeath() { TryAbilities(AbilityTrigger.OnDeath); queuedAbility = null; display?.ClearAbilityIntent(); }

    private void TickCooldowns()
    {
        var keys = new List<string>(cooldowns.Keys);
        foreach (var k in keys)
        {
            if (cooldowns[k] > 0 && cooldowns[k] < int.MaxValue) cooldowns[k]--;
        }
    }

    private void RegenerateEnergy()
    {
        if (runtimeEnemy == null || !runtimeEnemy.usesEnergy) return;
        if (runtimeEnemy.energyRegenPerTurn <= 0f) return;
        runtimeEnemy.RegenerateEnergy(runtimeEnemy.energyRegenPerTurn);
    }

    private void CheckPhaseGate()
    {
        if (runtimeEnemy == null || data == null || data.abilities == null) return;
        currentPhase = Mathf.Clamp01(runtimeEnemy.GetHealthPercentage());
        foreach (var a in data.abilities)
        {
            if (a == null || a.trigger != AbilityTrigger.PhaseGate) continue;
            if (a.phaseThreshold > 0f && currentPhase <= a.phaseThreshold)
            {
                QueueAbility(a);
            }
        }
    }

    private void TryAbilities(AbilityTrigger trigger)
    {
        if (data == null || data.abilities == null) return;
        foreach (var a in data.abilities)
        {
            if (a == null || a.trigger != trigger) continue;
            TryCast(a, trigger);
        }
    }

    private void TryCast(EnemyAbility a, AbilityTrigger triggerContext)
    {
        if (a == null || string.IsNullOrEmpty(a.id)) return;
        if (!cooldowns.TryGetValue(a.id, out var cd)) cooldowns[a.id] = 0;
        if (cooldowns[a.id] > 0) return;
        if (!HasRequiredEnergy(a)) return;

        if (ShouldQueueAbility(a) || triggerContext == AbilityTrigger.OnTurnStart || triggerContext == AbilityTrigger.OnTurnEnd || a.consumesTurn)
        {
            QueueAbility(a);
            return;
        }

        CastAbility(a, true);
    }

    private void QueueAbility(EnemyAbility a)
    {
        if (a == null) return;
        if (!cooldowns.TryGetValue(a.id, out var cd)) cooldowns[a.id] = 0;
        if (cd == int.MaxValue) return; // already consumed for this combat
        if (queuedAbility != null)
        {
            if (queuedAbility.id == a.id) return;
            Debug.LogWarning($"[EnemyAbilityRunner] Attempted to queue '{a.displayName}' while '{queuedAbility.displayName}' already queued. Keeping first.");
            return;
        }

        queuedAbility = a;
        ApplyAbilityIntentPreview(a, true);

        if (a.trigger != AbilityTrigger.OnAttack)
        {
            Transform targetTransform = display != null ? display.transform : null;
            Color abilityColor = display != null ? display.abilityIntentColor : EnemyCombatDisplay.DefaultAbilityIntentColor;
            FloatingDamageManager.Instance?.ShowAbilityName(a.displayName, targetTransform, abilityColor);
        }
    }

    private bool ShouldQueueAbility(EnemyAbility ability)
    {
        if (ability == null) return false;

        if (ability.trigger == AbilityTrigger.PhaseGate)
            return true;

        if (ability.trigger == AbilityTrigger.OnTurnStart || ability.trigger == AbilityTrigger.OnTurnEnd)
        {
            // Queue turn-based abilities that target the player so they telegraph before execution
            return ability.target == AbilityTarget.Player || ability.target == AbilityTarget.AllPlayers;
        }

        return false;
    }

    private void CastAbility(EnemyAbility a, bool showIntent)
    {
        var ctx = new AbilityContext
        {
            enemyRuntime = runtimeEnemy,
            enemyData = data,
            combat = combat,
            characterManager = cm,
            effects = vfx,
            display = display,
            target = a.target
        };

        ApplyAbilityIntentPreview(a, showIntent);

        if (showIntent && a.trigger != AbilityTrigger.OnAttack)
        {
            var targetTransform = ctx.display != null ? ctx.display.transform : null;
            Color abilityColor = ctx.display != null ? ctx.display.abilityIntentColor : EnemyCombatDisplay.DefaultAbilityIntentColor;
            FloatingDamageManager.Instance?.ShowAbilityName(a.displayName, targetTransform, abilityColor);
        }

        SpendAbilityEnergy(a);

        if (a.effects != null)
        {
            foreach (var eff in a.effects)
            {
                eff?.Execute(ctx);
            }
        }

        if (a.trigger == AbilityTrigger.PhaseGate)
        {
            cooldowns[a.id] = int.MaxValue;
        }
        else
        {
            cooldowns[a.id] = Mathf.Max(0, a.cooldownTurns);
        }

        if (showIntent)
        {
            ctx.display?.ClearAbilityIntent();
        }

        lastExecutedAbilityName = a.displayName;
        lastExecutionConsumedTurn = a.consumesTurn;
    }

    public EnemyAbility ExecuteQueuedAbility()
    {
        if (queuedAbility == null) return null;
        var ability = queuedAbility;
        if (!HasRequiredEnergy(ability))
        {
            // Keep it queued until energy recovers
            return null;
        }

        queuedAbility = null;
        CastAbility(ability, false);
        display?.ClearAbilityIntent();
        lastExecutionConsumedTurn = ability.consumesTurn;
        lastExecutedAbilityName = ability.displayName;
        return ability;
    }

    private bool HasRequiredEnergy(EnemyAbility ability)
    {
        if (ability == null || runtimeEnemy == null) return true;
        if (!runtimeEnemy.usesEnergy || ability.energyCost <= 0f) return true;
        if (runtimeEnemy.currentEnergy >= ability.energyCost - 0.01f) return true;

        Debug.Log($"[EnemyAbilityRunner] {runtimeEnemy.enemyName} lacks energy for {ability.displayName}. Current: {runtimeEnemy.currentEnergy:F1}, Cost: {ability.energyCost:F1}");
        return false;
    }

    private void SpendAbilityEnergy(EnemyAbility ability)
    {
        if (ability == null || runtimeEnemy == null) return;
        if (!runtimeEnemy.usesEnergy || ability.energyCost <= 0f) return;
        runtimeEnemy.DrainEnergy(ability.energyCost, ability.displayName);
    }


    public void ClearIntentTelegraph()
    {
        display?.ClearAbilityIntent();
    }

    private void ApplyAbilityIntentPreview(EnemyAbility ability, bool updateDisplay)
    {
        int previewDamage = CalculatePlayerDamagePreview(ability);
        lastPreviewDamage = previewDamage;
        if (runtimeEnemy != null)
        {
            runtimeEnemy.intentDamage = previewDamage;
        }

        if (updateDisplay && display != null)
        {
            display.ShowAbilityIntent(ability.displayName, previewDamage > 0 ? previewDamage : (int?)null);
        }
    }

    private int CalculatePlayerDamagePreview(EnemyAbility ability)
    {
        if (ability == null || ability.effects == null || ability.effects.Count == 0)
            return 0;

        if (!TargetsPlayer(ability.target))
            return 0;

        if (cm == null || !cm.HasCharacter())
            return 0;

        Character player = cm.GetCurrentCharacter();
        if (player == null)
            return 0;

        int total = 0;
        foreach (var effect in ability.effects)
        {
            if (effect is DamageEffect damageEffect)
            {
                total += damageEffect.GetPredictedDamage(player);
            }
        }
        return total;
    }

    private bool TargetsPlayer(AbilityTarget target)
    {
        return target == AbilityTarget.Player || target == AbilityTarget.AllPlayers;
    }
}



