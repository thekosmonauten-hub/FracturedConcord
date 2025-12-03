using UnityEngine;

/// <summary>
/// Enum defining unique boss ability mechanics that require special handling
/// beyond standard AbilityEffect scriptable objects.
/// Used by BossAbilityHandler for complex, reactive abilities.
/// </summary>
public enum BossAbilityType
{
    None = 0,
    
    // ====================
    // TIER 3 - COMPLEX REACTIVE ABILITIES
    // ====================
    
    /// <summary>
    /// Sundering Echo (The First to Fall)
    /// Duplicates the first card played each turn with corrupted values (reduced damage, increased cost).
    /// </summary>
    SunderingEcho,
    
    /// <summary>
    /// Judgment Loop (Bridge Warden Remnant)
    /// Repeats boss's last attack if player plays two cards of the same type consecutively.
    /// </summary>
    JudgmentLoop,
    
    /// <summary>
    /// Add Curse Cards (River-Sworn Rotmass: Bile Torrent, Entropic Traveler: Dust Trail)
    /// Adds curse cards (Rot, Wither, etc.) to player's deck or hand.
    /// </summary>
    AddCurseCards,
    
    /// <summary>
    /// Reactive Seep (River-Sworn Rotmass)
    /// Deals damage to the player whenever they gain Guard.
    /// </summary>
    ReactiveSeep,
    
    /// <summary>
    /// Retaliation on Skill (Weeper-of-Bark: Echo of Breaking)
    /// Deals retribution damage when player plays a Skill-type card.
    /// </summary>
    RetaliationOnSkill,
    
    /// <summary>
    /// Avoid First Attack (Entropic Traveler: Empty Footfalls)
    /// Boss evades the first attack each turn (100% evasion).
    /// </summary>
    AvoidFirstAttack,
    
    /// <summary>
    /// Conditional Heal (Charred Homesteader: Cooling Regret)
    /// Boss heals if a certain condition is met (e.g., not hit this turn).
    /// </summary>
    ConditionalHeal,
    
    /// <summary>
    /// Conditional Stealth (Shadow Shepherd: Cloak of Dusk)
    /// Boss gains stealth/untargetable status after meeting condition (e.g., hit by 3 cards).
    /// </summary>
    ConditionalStealth,
    
    /// <summary>
    /// Negate Strongest Buff (Gate Warden of Vassara: Last Article)
    /// Removes the player's strongest buff (highest magnitude).
    /// </summary>
    NegateStrongestBuff,
    
    /// <summary>
    /// Barrier of Dissent (Gate Warden of Vassara)
    /// Boss becomes invulnerable but loses health on a cyclical pattern (every N turns).
    /// </summary>
    BarrierOfDissent,
    
    /// <summary>
    /// Bloom of Ruin (Fieldborn Aberrant)
    /// Places a reactive zone that triggers when player gains Guard.
    /// </summary>
    BloomOfRuin,
    
    /// <summary>
    /// Learn Player Card (Bandit Reclaimer: Predation Lineage)
    /// Boss copies a card from player's discard pile and can cast it.
    /// </summary>
    LearnPlayerCard,
    
    /// <summary>
    /// Buff Cancellation (Bridge Warden Remnant: Crossing Denied)
    /// Negates the player's next buff application.
    /// </summary>
    BuffCancellation,
    
    /// <summary>
    /// Add Temporary Curse (Entropic Traveler: Dust Trail)
    /// Adds temporary curse cards directly to player's hand (vanish at turn end).
    /// </summary>
    AddTemporaryCurse,
}

