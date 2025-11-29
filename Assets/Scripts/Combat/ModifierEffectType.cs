using UnityEngine;

/// <summary>
/// Types of effects that modifiers can apply
/// </summary>
public enum ModifierEffectType
{
    None,
    
    // On-Hit Effects
    ShockOnHit,              // Apply Shock status on hit
    ChillOnHit,               // Apply Chill status on hit
    BurnOnHit,                // Apply Burn status on hit
    PoisonOnHit,              // Apply Poison status on hit
    VulnerableOnHit,          // Apply Vulnerable status on hit
    
    // Defensive Effects
    NoExtraCritDamage,        // Take no extra damage from critical strikes
    ReducedCritDamage,        // Take reduced damage from critical strikes (percentage)
    
    // Turn-Based Effects
    GrantStackOnTurnStart,    // Grant stacks (Tolerance, Agitate, etc.) at turn start
    GrantStackOnHit,          // Grant stacks when hit
    
    // Special Effects
    ReflectDamage,            // Reflect damage back (already exists as bool, but can be effect type)
    ImmuneToStatus,          // Immune to specific status effects
    
    // Delayed Actions
    DelayedActions,          // Actions occur 1 turn after declared (Time-Lagged)
    
    // Damage Modifiers
    TemporalSlip,            // Chance to avoid damage and heal instead
    InvertedHarmonics,       // Damage below threshold heals, above deals 20% more
    BloodTither,             // Steals life equal to % of damage taken
    GrudgeForged,            // Damage dealt increases enemy's damage next turn
    
    // Retaliation Effects
    SmolderingArmor,         // Applies fire damage to attackers
    Scrapstorm,              // Chance to retaliate with shards on damage
    VolatilePulse,           // AoE lightning on crit hit
    
    // Turn-Based Effects
    OverclockedCore,         // Gains % speed/damage every turn
    ShiftingPlates,          // Alternates between Armored/Evasive
    ErraticPulse,            // Random action at turn start
    
    // Health Threshold Effects
    Despairmonger,           // Applies mental affliction at health thresholds
    FracturedEcho,           // Spawns echo copy at 50% HP
    
    // Death Effects
    WailOfTheFallen,         // On death, applies vulnerable
    
    // Stack Tracking
    ParadoxBound,            // Gains resistance if hit twice with same damage type
    EchoOfMissteps,          // Gains buff when player plays same card twice
    
    // Player Interaction
    ContractLeech,           // Gains buff when player gains buff
    InsightDrinker,          // Gains buff when player draws outside normal draw
    WhisperWrapped,          // Adds junk card to player deck every N turns
    IceboundRoots,           // First attack each turn costs more mana
    Threadbare,              // Takes extra damage, attacks twice at 50% efficiency
    UnravelingAura,          // Chance to remove player buff when attacking
    LawTwisted,              // Randomly changes damage type each turn
    Hexweaver,               // Every N turns, casts strong curse
    BoundInSilence,          // Temporarily silences player skills every N turns
    ContractHoarder,         // Drops double loot but gains stats when player draws
    Clausebreaker,           // Every Nth hit applies random debuff
    SoulLinked,              // Damage shared between linked enemies
}
