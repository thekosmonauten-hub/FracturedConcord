using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyAbility", menuName = "Dexiled/Enemies/Ability")]
public class EnemyAbility : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("Timing")]
    public AbilityTrigger trigger = AbilityTrigger.OnTurnStart;
    [Tooltip("Turns after cast before this ability can be used again")] public int cooldownTurns = 0;
    [Tooltip("Starting cooldown when the enemy spawns")] public int initialCooldown = 0;
    [Tooltip("Phase gate threshold (0..1). Used only when trigger = PhaseGate")] [Range(0f,1f)] public float phaseThreshold = 0f;
    [Tooltip("If true, using this ability consumes the enemy's action for the turn (no attack/defend).")]
    public bool consumesTurn = true;

    [Header("Targeting")]
    public AbilityTarget target = AbilityTarget.Player;

[Header("Energy")]
[Tooltip("Energy cost paid when this ability fires. Ignored if the enemy has no energy pool.")]
[Min(0f)] public float energyCost = 0f;

    [Header("Effects")] 
    public List<AbilityEffect> effects = new List<AbilityEffect>();

    public bool ConsumesTurn() => consumesTurn;
}

public enum AbilityTrigger { OnSpawn, OnTurnStart, OnTurnEnd, OnAttack, OnDamaged, OnDeath, PhaseGate }
public enum AbilityTarget { Self, Player, AllEnemies, RandomEnemy, AllPlayers }
