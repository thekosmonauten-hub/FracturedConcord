using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "Dexiled/Enemies/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public DamageType damageType = DamageType.Physical;
    public int flatDamage = 10;
    [Range(0f,1f)] public float percentOfMaxHp = 0f;
    [Tooltip("When targeting allied enemies, include the caster in the damage list.")]
    public bool includeCasterWhenTargetingAllies = false;

    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null) return;

        CharacterManager characterManager = ctx.characterManager ?? CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
            return;

        int damage = CalculateDamage(characterManager);

        switch (ctx.target)
        {
            case AbilityTarget.Self:
                DamageEnemyDisplay(ctx.display, ctx.enemyRuntime, damage);
                break;

            case AbilityTarget.AllEnemies:
                DamageAllEnemyDisplays(ctx, damage, includeCasterWhenTargetingAllies);
                break;

            case AbilityTarget.RandomEnemy:
                DamageRandomEnemyDisplay(ctx, damage, includeCasterWhenTargetingAllies);
                break;

            case AbilityTarget.Player:
            case AbilityTarget.AllPlayers:
            default:
                DamagePlayer(characterManager, damage, ctx);
                break;
        }
    }

    private int CalculateDamage(CharacterManager characterManager)
    {
        var player = characterManager.GetCurrentCharacter();
        return GetPredictedDamage(player);
    }

    public int GetPredictedDamage(Character player)
    {
        int dmg = flatDamage;
        if (player != null && percentOfMaxHp > 0f)
        {
            dmg += Mathf.CeilToInt(player.maxHealth * percentOfMaxHp);
        }
        return dmg;
    }

    private void DamagePlayer(CharacterManager cm, int damage, AbilityContext ctx)
    {
        cm.TakeDamage(damage);
        if (ctx.effects != null && ctx.display != null)
        {
            ctx.effects.PlayElementalDamageEffectOnTarget(ctx.display.transform, damageType);
        }
    }

    private void DamageEnemyDisplay(EnemyCombatDisplay display, Enemy runtimeEnemy, int damage)
    {
        if (display != null)
        {
            display.TakeDamage(damage);
            return;
        }

        if (runtimeEnemy == null) return;
        var displays = Object.FindObjectsByType<EnemyCombatDisplay>(FindObjectsSortMode.None);
        foreach (var d in displays)
        {
            if (d != null && d.GetCurrentEnemy() == runtimeEnemy)
            {
                d.TakeDamage(damage);
                return;
            }
        }
    }

    private void DamageAllEnemyDisplays(AbilityContext ctx, int damage, bool includeCaster)
    {
        if (ctx.combat == null) return;
        List<EnemyCombatDisplay> displays = ctx.combat.GetActiveEnemyDisplays();
        if (displays == null || displays.Count == 0) return;

        foreach (var display in displays)
        {
            if (display == null) continue;
            if (!includeCaster && IsSameDisplay(display, ctx)) continue;
            display.TakeDamage(damage);
        }
    }

    private void DamageRandomEnemyDisplay(AbilityContext ctx, int damage, bool includeCaster)
    {
        if (ctx.combat == null) return;
        List<EnemyCombatDisplay> displays = ctx.combat.GetActiveEnemyDisplays();
        if (displays == null || displays.Count == 0) return;

        var eligible = new List<EnemyCombatDisplay>();
        foreach (var display in displays)
        {
            if (display == null) continue;
            if (!includeCaster && IsSameDisplay(display, ctx)) continue;
            eligible.Add(display);
        }

        if (eligible.Count == 0) return;

        int index = Random.Range(0, eligible.Count);
        eligible[index].TakeDamage(damage);
    }

    private bool IsSameDisplay(EnemyCombatDisplay candidate, AbilityContext ctx)
    {
        if (candidate == null) return false;
        if (ctx.display != null && candidate == ctx.display) return true;
        if (ctx.enemyRuntime != null && candidate.GetCurrentEnemy() == ctx.enemyRuntime) return true;
        return false;
    }
}

