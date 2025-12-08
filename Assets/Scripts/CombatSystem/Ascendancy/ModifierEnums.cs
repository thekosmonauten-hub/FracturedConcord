/// <summary>
/// Event types that trigger modifier effects
/// </summary>
public enum ModifierEventType
{
    OnCombatStart = 0,
    OnTurnStart = 1,
    OnTurnEnd = 2,
    OnCardPlayed = 3,
    OnAttackUsed = 4,
    OnSpellCast = 5,
    OnDamageDealt = 6,
    OnDamageTaken = 7,
    OnKill = 8,
    OnBlock = 9,
    OnDodge = 10,
    OnCriticalHit = 11,
    OnStatusApplied = 12,
    OnEnemyKilled = 13,
    OnDamageDealtToEnemy = 14,
    OnHeal = 15,
    OnGuardGained = 16,
    OnManaSpent = 17,
    OnCardDrawn = 18,
    OnEnemyDefeated = 19
}

/// <summary>
/// Action types that modifiers can perform
/// </summary>
public enum ModifierActionType
{
    AddStack = 0,
    RemoveStack = 1,
    ConsumeStack = 2,
    SetStack = 3,
    ClearStack = 4,
    ModifyMaxStacks = 5,
    AddFlatDamage = 6,
    AddPercentDamage = 7,
    AddDamageMorePercent = 8,
    ModifyDamageMultiplier = 9,
    ConvertDamageType = 10,
    AddElementalDamage = 11,
    AddExtraHit = 12,
    ApplyStatus = 13,
    RemoveStatus = 14,
    ModifyStat = 15,
    AddGuard = 16,
    AddMana = 17,
    DrawCard = 18,
    DiscardCard = 19,
    ApplyStatusToEnemy = 20,
    ModifyHealthPercent = 21, // Modify health by percentage (for Unstable Corruption)
    MarkCardAsTemporal = 22, // Mark a card as Temporal (for Temporal Threads)
    EchoTemporalCards = 23, // Echo Temporal cards at turn start (for Temporal Threads)
    TriggerChaosSurge = 24, // Trigger Chaos Surge effect (for Unstable Corruption)
    
    // Reliance Aura Complex Effects
    SpreadStatusOnKill = 25, // Spread status effect to random enemy on kill (e.g., spread ignite)
    ShatterOnKill = 26, // Deal damage to adjacent enemies on killing chilled/frozen enemy
    CastSpellOnKill = 27, // Cast a spell on random enemy when killing shocked enemy
    ApplyCrumble = 28, // Apply Crumble stack to enemy on hit
    DealCrumbleDamage = 29, // Deal extra damage based on Crumble stacks
    AddPoisonStacks = 30, // Add additional poison stacks on application
    RollDamagePerTurn = 31, // Roll new damage value each turn (for Tempest Flux)
    StackDamagePerTurn = 32, // Stack damage multiplier per turn (for Iron Ascent)
    ModifyAilmentDurationAndEffect = 33, // Modify ailment duration and effectiveness (for Woundweaver Rite)
    ScaleDiscardPower = 34 // Scale discard power based on discarded cards (for Echo of the Unburdened)
}

/// <summary>
/// Condition types for modifier effects
/// </summary>
public enum ModifierConditionType
{
    HasStack = 0,
    StackCount = 1,
    HasStatus = 2,
    HealthPercent = 3,
    ManaPercent = 4,
    TurnNumber = 5,
    CardType = 6,
    DamageType = 7,
    TargetType = 8,
    RandomChance = 9
}

