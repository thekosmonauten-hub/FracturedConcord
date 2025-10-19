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
}



