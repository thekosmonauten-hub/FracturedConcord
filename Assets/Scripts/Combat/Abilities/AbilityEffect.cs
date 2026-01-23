using UnityEngine;

public abstract class AbilityEffect : ScriptableObject
{
    public abstract void Execute(AbilityContext ctx);
}

public class AbilityContext
{
    public Enemy enemyRuntime;
    public EnemyData enemyData;
    public CombatDisplayManager combat;
    public CharacterManager characterManager;
    public CombatEffectManager effects;
    public EnemyCombatDisplay display;
    public AbilityTarget target;
    public float effectMultiplier = 1f;
    public bool isCharged = false;
    public bool hasOverrideDamageType = false;
    public DamageType overrideDamageType = DamageType.Physical;
    public bool isVolatile = false;
    public bool isTerminal = false;
}



