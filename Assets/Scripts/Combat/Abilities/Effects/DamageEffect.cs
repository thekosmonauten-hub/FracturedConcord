using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "Dexiled/Enemies/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public DamageType damageType = DamageType.Physical;
    public int flatDamage = 10;
    [Range(0f,1f)] public float percentOfMaxHp = 0f;

    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null || ctx.characterManager == null || !ctx.characterManager.HasCharacter()) return;
        var player = ctx.characterManager.GetCurrentCharacter();
        int dmg = flatDamage;
        if (percentOfMaxHp > 0f) dmg += Mathf.CeilToInt(player.maxHealth * percentOfMaxHp);

        ctx.characterManager.TakeDamage(dmg);
        if (ctx.effects != null && ctx.display != null)
        {
            ctx.effects.PlayElementalDamageEffectOnTarget(ctx.display.transform, damageType);
        }
    }
}



