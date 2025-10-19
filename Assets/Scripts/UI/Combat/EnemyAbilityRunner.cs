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

    public void OnSpawn() { TryAbilities(AbilityTrigger.OnSpawn); }
    public void OnTurnStart() { TickCooldowns(); TryAbilities(AbilityTrigger.OnTurnStart); CheckPhaseGate(); }
    public void OnTurnEnd() { TryAbilities(AbilityTrigger.OnTurnEnd); CheckPhaseGate(); }
    public void OnAttack() { TryAbilities(AbilityTrigger.OnAttack); }
    public void OnDamaged() { TryAbilities(AbilityTrigger.OnDamaged); }
    public void OnDeath() { TryAbilities(AbilityTrigger.OnDeath); }

    private void TickCooldowns()
    {
        var keys = new List<string>(cooldowns.Keys);
        foreach (var k in keys)
        {
            if (cooldowns[k] > 0) cooldowns[k]--;
        }
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
                TryCast(a);
            }
        }
    }

    private void TryAbilities(AbilityTrigger trigger)
    {
        if (data == null || data.abilities == null) return;
        foreach (var a in data.abilities)
        {
            if (a == null || a.trigger != trigger) continue;
            TryCast(a);
        }
    }

    private void TryCast(EnemyAbility a)
    {
        if (a == null || string.IsNullOrEmpty(a.id)) return;
        if (!cooldowns.TryGetValue(a.id, out var cd)) cooldowns[a.id] = 0;
        if (cooldowns[a.id] > 0) return;

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

        if (a.effects != null)
        {
            foreach (var eff in a.effects)
            {
                eff?.Execute(ctx);
            }
        }

        cooldowns[a.id] = Mathf.Max(0, a.cooldownTurns);
    }
}



