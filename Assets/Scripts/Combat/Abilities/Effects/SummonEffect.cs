using UnityEngine;

[CreateAssetMenu(fileName = "SummonEffect", menuName = "Dexiled/Enemies/Effects/Summon")]
public class SummonEffect : AbilityEffect
{
    [Tooltip("Optional explicit enemy to summon. If null, uses caster's summonPool.")]
    public EnemyData enemyToSummon;
    public int minCount = 1;
    public int maxCount = 2;

    public override void Execute(AbilityContext ctx)
    {
        if (ctx == null || ctx.combat == null) return;

        int count = Mathf.Clamp(Random.Range(minCount, maxCount + 1), 1, 99);

        // Choose pool
        System.Collections.Generic.List<EnemyData> pool = null;
        if (enemyToSummon != null)
        {
            pool = new System.Collections.Generic.List<EnemyData> { enemyToSummon };
        }
        else if (ctx.enemyData != null && ctx.enemyData.summonPool != null && ctx.enemyData.summonPool.Count > 0)
        {
            pool = ctx.enemyData.summonPool;
        }

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("SummonEffect has no valid summon pool or target. Skipping.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var pick = pool[Random.Range(0, pool.Count)];
            ctx.combat.SpawnSpecificEnemy(pick, EnemyRarity.Normal);
        }
    }
}



