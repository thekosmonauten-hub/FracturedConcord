using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class CharacterStatsData
{
    [Header("Core Attributes")]
    public int strength;
    public int dexterity;
    public int intelligence;
    
    [Header("Combat Resources")]
    public int maxHealth;
    public int currentHealth;
    public int maxEnergyShield;
    public int currentEnergyShield;
    public int maxMana;
    public int currentMana;
    public int maxReliance;
    public int currentReliance;
    public float currentGuard;
    
    [Header("Combat Stats")]
    public int attackPower;
    public int defense;
    public float criticalChance;
    public float criticalMultiplier;
    public float accuracy;
    public float evasionIncreased;
    
    [Header("Critical Strike Modifiers (Increased %)")]
    public float increasedCriticalStrikeChance = 0f;
    public float increasedCriticalStrikeMultiplier = 0f;
    public float increasedCriticalChanceWithDaggers = 0f;
    public float increasedCriticalChanceWithSwords = 0f;
    public float increasedCriticalMultiplierWithDaggers = 0f;
    public float increasedCriticalMultiplierWithSwords = 0f;
    public float increasedCriticalChanceWithColdSkills = 0f;
    public float increasedCriticalChanceWithLightningCards = 0f;
    public float increasedCriticalChanceVsFullLife = 0f;
    public float increasedCriticalChanceVsCursed = 0f;
    public float increasedCriticalChanceWithProjectileCards = 0f;
    public float increasedCriticalChanceWithPreparedCards = 0f;
    public float increasedSpellCriticalChance = 0f;
    public float increasedSpellCriticalMultiplier = 0f;
    public float increasedCriticalDamageMultiplier = 0f;
    
    [Header("Damage Modifiers")]
    public float increasedPhysicalDamage;
    public float increasedFireDamage;
    public float increasedColdDamage;
    public float increasedLightningDamage;
    public float increasedChaosDamage;
    public float increasedElementalDamage;
    public float increasedElementalAttackDamage; // Elemental damage with attack skills specifically
    public float increasedSpellDamage;
    public float increasedAttackDamage;
    public float increasedProjectileDamage;
    public float increasedAreaDamage;
    public float increasedMeleeDamage;
    public float increasedRangedDamage;
    
    [Header("More Damage Multipliers")]
    public float morePhysicalDamage = 1f;
    public float moreFireDamage = 1f;
    public float moreColdDamage = 1f;
    public float moreLightningDamage = 1f;
    public float moreChaosDamage = 1f;
    public float moreElementalDamage = 1f;
    public float moreSpellDamage = 1f;
    public float moreAttackDamage = 1f;
    public float moreProjectileDamage = 1f;
    public float moreAreaDamage = 1f;
    public float moreMeleeDamage = 1f;
    public float moreRangedDamage = 1f;
    
    [Header("Added Damage")]
    public float addedPhysicalDamage;
    public float addedFireDamage;
    public float addedColdDamage;
    public float addedLightningDamage;
    public float addedChaosDamage;
    public float addedElementalDamage;
    public float addedSpellDamage;
    public float addedAttackDamage;
    public float addedProjectileDamage;
    public float addedAreaDamage;
    public float addedMeleeDamage;
    public float addedRangedDamage;
    
    [Header("Resistances")]
    public float physicalResistance;
    public float fireResistance;
    public float coldResistance;
    public float lightningResistance;
    public float chaosResistance;
    public float elementalResistance;
    public float allResistance;
    
    [Header("Resistance Modifiers (Increased %)")]
    public float physicalResistanceIncreased = 0f;
    public float allElementalResistancesIncreased = 0f; // Applies to fire/cold/lightning simultaneously
    public float fireResistancePenetration = 0f; // % penetration of fire resistance (damage calc modifier)
    
    [Header("Defense Stats")]
    public int armour;
    public float evasion;
    public int energyShield;
    public float blockChance;
    public float dodgeChance;
    public float spellDodgeChance;
    public float spellBlockChance;
    
    [Header("Block & Dodge Modifiers (Increased %)")]
    public float blockChanceIncreased = 0f;
    public float blockEffectivenessIncreased = 0f;
    public float dodgeChanceIncreased = 0f;
    public float criticalStrikeAvoidance = 0f; // % chance to avoid critical strikes
    public float debuffExpirationRateIncreased = 0f; // % faster debuff expiration
	
	[Header("Defense % Increased")]
	public float maxHealthIncreased; // % increased Max Health
	public float maxManaIncreased; // % increased Max Mana
	public float energyShieldIncreased; // % increased Energy Shield
	public float armourIncreased; // % increased Armour
    
    [Header("Ailments")]
    // Non-damaging ailments chance
    public float chanceToShock;
    public float chanceToChill;
    public float chanceToFreeze;
    
    // Damaging ailments chance
    public float chanceToIgnite;
    public float chanceToBleed;
    public float chanceToPoison;
    
    // Increased ailment magnitude/effect
    public float increasedIgniteMagnitude;
    public float increasedShockMagnitude;
    public float increasedChillMagnitude;
    public float increasedFreezeMagnitude;
    public float increasedBleedMagnitude;
    public float increasedPoisonMagnitude;
	public float increasedDamageOverTime; // % increased DoT
	public float increasedPoisonDamage; // % increased Poison Damage
	public float increasedPoisonDuration; // % increased Poison Duration
	
	[Header("Ailment & Status Effect Modifiers")]
	public float ailmentDurationIncreased = 0f; // % increased ailment duration (generic)
	public float ailmentApplicationChanceIncreased = 0f; // % increased chance to apply ailments
	public float slowChance = 0f; // Chance to slow enemies
	public float freezeChance = 0f; // Chance to freeze enemies (separate from chanceToFreeze for consistency)
	public float curseApplicationChance = 0f; // Chance to apply curses
	public float curseDurationIncreased = 0f; // % increased curse duration
	public float curseEffectivenessIncreased = 0f; // % increased curse effect
	public float randomAilmentChance = 0f; // Chance to apply random ailment
	public float chillEffectivenessIncreased = 0f; // % increased chill effectiveness
	public float shockEffectivenessIncreased = 0f; // % increased shock effectiveness
	
	[Header("Situational Damage")]
	public float increasedDamageVsChilled;
	public float increasedDamageVsShocked;
	public float increasedDamageVsIgnited;
	
	[Header("Conditional Damage Modifiers (Increased %)")]
	public float increasedDamageVsCursed = 0f;
	public float increasedDamageVsBlocked = 0f;
	public float increasedDamageVsSlowed = 0f;
	public float increasedDamageWhileInShadow = 0f;
	public float increasedDamageOnCriticalStrikes = 0f;
	public float increasedDamageAfterDiscard = 0f; // Temporary damage boost after discarding
	public float increasedDamageWhenBelowLifeThreshold = 0f; // Conditional damage (threshold-based)
	public float increasedDamageVsRareAndUnique = 0f; // % increased damage vs elite enemies
	public float increasedDamageWithConsecutiveAttacks = 0f; // Scaling damage with combo
	public float increasedDamageOnEveryNthAttack = 0f; // Conditional damage (e.g., every 3rd attack)
	public float increasedDamageVsMarked = 0f; // % increased damage vs marked enemies
    
    [Header("Recovery Stats")]
    public float lifeRegeneration;
    public float energyShieldRegeneration;
    public float manaRegeneration;
    public float relianceRegeneration;
    public float lifeSteal; // Renamed from lifeLeech for consistency
    public float manaSteal; // Renamed from manaLeech for consistency
    public float energyShieldLeech;
    
    [Header("Recovery Modifiers (Increased %)")]
    public float lifeRegenerationIncreased = 0f;
    public float lifeRecoveryRateIncreased = 0f;
    public float manaRegenerationIncreased = 0f;
    public float lifeStealOnHitIncreased = 0f; // % increased life steal
    public float manaLeechChance = 0f; // Chance to leech mana on hit
    public float lifeOnKill = 0f; // Flat or % life gained on kill
    public float lifeCostEfficiencyIncreased = 0f; // % increased life cost efficiency (reduces life costs)
    
    [Header("Combat Mechanics")]
    public float attackSpeed;
    public float castSpeed;
    public float movementSpeed;
    public float attackRange;
    public float projectileSpeed;
    public float areaOfEffect;
    public float skillEffectDuration;
    public float statusEffectDuration;
    
    [Header("Movement & Mobility (Increased %)")]
    public float movementSpeedIncreased = 0f;
    
    [Header("Stun & Crowd Control")]
    public float stunDurationIncreased = 0f; // % increased stun duration on enemies
    public float staggerEffectivenessIncreased = 0f; // % increased stagger effectiveness
    public float increasedDamageToStaggered = 0f; // % increased damage to staggered enemies
    public float reducedEnemyStaggerThreshold = 0f; // % reduced enemy stagger threshold (effectively increases stagger damage)
    public float increasedStaggerDuration = 0f; // Flat addition to stagger stun duration (in turns, base is 1 turn)
	
	[Header("Charge/Resource Gains")]
	public float aggressionGainIncreased;
	public float focusGainIncreased;
    
    [Header("Card System Stats")]
    public int cardsDrawnPerTurn;
    public int cardsDrawnPerWave;
    public int maxHandSize;
    public int discardPileSize;
    public int exhaustPileSize;
    public float cardDrawChance;
    public float cardRetentionChance;
    public float cardUpgradeChance;
    public float discardPower; // Added: Discard power stat
    public float manaPerTurn; // Added: Mana per turn stat
    
    [Header("Card System Modifiers (Increased %)")]
    public float cardDrawChanceIncreased = 0f;
    public int handSizeIncreased = 0; // Flat increase to hand size
    public float manaCostEfficiencyIncreased = 0f; // % reduced mana costs
    public float manaRefundChance = 0f; // Chance to refund mana on card play
    public float cardCycleSpeedIncreased = 0f; // % increased card cycle/draw speed
    public float preparedCardEffectivenessIncreased = 0f; // % increased effect of prepared cards
    public int discardCostReduction = 0; // Flat reduction to discard costs
    public float discardPowerIncreased = 0f; // % increased discard effect
    public float delayedCardEffectivenessIncreased = 0f; // % increased delayed card effect
    public float skillCardEffectivenessIncreased = 0f; // % increased skill card effect
    public float spellPowerIncreased = 0f; // % increased spell power/effectiveness
    public float echoCardEffectivenessIncreased = 0f; // % increased echo card effectiveness
    public float chainChance = 0f; // Chance for spells to chain
    public float spellEffectVsAilmentIncreased = 0f; // % increased spell effect vs any ailment
	
	[Header("Weapon/Type Damage Modifiers")]
	public float increasedAxeDamage;
	public float increasedBowDamage;
	public float increasedMaceDamage;
	public float increasedSwordDamage;
	public float increasedWandDamage;
	public float increasedDaggerDamage; // Added: Dagger damage
	public float increasedOneHandedDamage;
	public float increasedTwoHandedDamage;
	
	[Header("Guard/Defense Utilities")]
	public float guardEffectiveness = 0f; // Base stat: additive to Guard provided by cards
	public float guardEffectivenessIncreased = 0f; // % increased to guardEffectiveness stat
	public float guardRetentionIncreased = 0f; // % more guard retained next turn
	public float damageReductionWhileGuarding = 0f; // % damage reduction when guarding
	public float guardBreakChance = 0f; // Chance to break enemy guard
	public float lessDamageFromElites;
	public float statusAvoidance;
	
	[Header("Damage Reduction & Mitigation (Less/Multiplicative)")]
	public float damageReductionIncreased = 0f; // Generic % damage reduction (less damage taken)
	public float damageReductionFromSpells = 0f; // % reduced damage from spells
	public float damageReductionWhenStunned = 0f; // Conditional damage reduction
	public float physicalDamageReductionIncreased = 0f; // % increased physical damage reduction
	public float physicalReductionIncreased = 0f; // % increased physical reduction (shorthand)
    
    // Passive Tree Stats removed - passive tree system deleted
    
    [Header("Equipment Stats")]
    public Dictionary<string, float> equipmentStats = new Dictionary<string, float>();
    public List<string> equippedItems = new List<string>();
    public Dictionary<string, int> equipmentLevels = new Dictionary<string, int>();
    
    [Header("Temporary Buffs")]
    public Dictionary<string, float> temporaryBuffs = new Dictionary<string, float>();
    public Dictionary<string, float> temporaryDebuffs = new Dictionary<string, float>();
    
    [Header("Calculated Totals")]
    public Dictionary<string, float> calculatedTotals = new Dictionary<string, float>();
    
    // Constructor
    public CharacterStatsData()
    {
        InitializeDefaultStats();
    }
    
    // Initialize with character data
    public CharacterStatsData(Character character)
    {
        InitializeFromCharacter(character);
    }
    
    private void InitializeDefaultStats()
    {
        // Set all stats to 0 or default values
        strength = 0;
        dexterity = 0;
        intelligence = 0;
        
        maxHealth = 100;
        currentHealth = 100;
        maxEnergyShield = 0;
        currentEnergyShield = 0;
        maxMana = 3;
        currentMana = 3;
        maxReliance = 200;
        currentReliance = 200;
        currentGuard = 0f;
        
        attackPower = 0;
        defense = 0;
        criticalChance = 5f;
        criticalMultiplier = 1.5f;
        accuracy = 100f;
        evasion = 0f;
        evasionIncreased = 0f;
        
        // Initialize critical strike modifiers
        increasedCriticalStrikeChance = 0f;
        increasedCriticalStrikeMultiplier = 0f;
        increasedCriticalChanceWithDaggers = 0f;
        increasedCriticalChanceWithSwords = 0f;
        increasedCriticalMultiplierWithDaggers = 0f;
        increasedCriticalMultiplierWithSwords = 0f;
        increasedCriticalChanceWithColdSkills = 0f;
        increasedCriticalChanceWithLightningCards = 0f;
        increasedCriticalChanceVsFullLife = 0f;
        increasedCriticalChanceVsCursed = 0f;
        increasedCriticalChanceWithProjectileCards = 0f;
        increasedCriticalChanceWithPreparedCards = 0f;
        increasedSpellCriticalChance = 0f;
        increasedSpellCriticalMultiplier = 0f;
        increasedCriticalDamageMultiplier = 0f;
        
        blockChance = 0f;
        dodgeChance = 0f;
        blockChanceIncreased = 0f;
        blockEffectivenessIncreased = 0f;
        dodgeChanceIncreased = 0f;
        criticalStrikeAvoidance = 0f;
        debuffExpirationRateIncreased = 0f;
        
        // Initialize all damage modifiers to 0
        increasedPhysicalDamage = 0f;
        increasedFireDamage = 0f;
        increasedColdDamage = 0f;
        increasedLightningDamage = 0f;
        increasedChaosDamage = 0f;
        increasedElementalDamage = 0f;
        increasedSpellDamage = 0f;
        increasedAttackDamage = 0f;
        increasedProjectileDamage = 0f;
        increasedAreaDamage = 0f;
        increasedMeleeDamage = 0f;
        increasedRangedDamage = 0f;
        
        // Initialize all resistances to 0
        physicalResistance = 0f;
        fireResistance = 0f;
        coldResistance = 0f;
        lightningResistance = 0f;
        chaosResistance = 0f;
        elementalResistance = 0f;
        allResistance = 0f;
        physicalResistanceIncreased = 0f;
        allElementalResistancesIncreased = 0f;
        fireResistancePenetration = 0f;
        
        // Initialize other stats
        armour = 0;
        evasion = 0f;
        energyShield = 0;
        blockChance = 0f;
        dodgeChance = 0f;
        spellDodgeChance = 0f;
        spellBlockChance = 0f;
		maxHealthIncreased = 0f;
		maxManaIncreased = 0f;
		energyShieldIncreased = 0f;
		armourIncreased = 0f;
        
        // Initialize ailment stats
        chanceToShock = 0f;
        chanceToChill = 0f;
        chanceToFreeze = 0f;
        chanceToIgnite = 0f;
        chanceToBleed = 0f;
        chanceToPoison = 0f;
        increasedIgniteMagnitude = 0f;
        increasedShockMagnitude = 0f;
        increasedChillMagnitude = 0f;
        increasedFreezeMagnitude = 0f;
        increasedBleedMagnitude = 0f;
        increasedPoisonMagnitude = 0f;
		increasedDamageOverTime = 0f;
		increasedPoisonDamage = 0f;
		increasedPoisonDuration = 0f;
		increasedDamageVsChilled = 0f;
		increasedDamageVsShocked = 0f;
		increasedDamageVsIgnited = 0f;
		ailmentDurationIncreased = 0f;
		ailmentApplicationChanceIncreased = 0f;
		slowChance = 0f;
		freezeChance = 0f;
		curseApplicationChance = 0f;
		curseDurationIncreased = 0f;
		curseEffectivenessIncreased = 0f;
		randomAilmentChance = 0f;
		chillEffectivenessIncreased = 0f;
		shockEffectivenessIncreased = 0f;
		increasedDamageVsCursed = 0f;
		increasedDamageVsBlocked = 0f;
		increasedDamageVsSlowed = 0f;
		increasedDamageWhileInShadow = 0f;
		increasedDamageOnCriticalStrikes = 0f;
		increasedDamageAfterDiscard = 0f;
		increasedDamageWhenBelowLifeThreshold = 0f;
		increasedDamageVsRareAndUnique = 0f;
		increasedDamageWithConsecutiveAttacks = 0f;
		increasedDamageOnEveryNthAttack = 0f;
        
        lifeRegeneration = 0f;
        energyShieldRegeneration = 0f;
        manaRegeneration = 3f;
        relianceRegeneration = 0f;
        lifeSteal = 0f; // Renamed from lifeLeech
        manaSteal = 0f; // Renamed from manaLeech
        energyShieldLeech = 0f;
        lifeRegenerationIncreased = 0f;
        lifeRecoveryRateIncreased = 0f;
        manaRegenerationIncreased = 0f;
        lifeStealOnHitIncreased = 0f;
        manaLeechChance = 0f;
        lifeOnKill = 0f;
        lifeCostEfficiencyIncreased = 0f;
        
        attackSpeed = 1f;
        castSpeed = 1f;
        movementSpeed = 1f;
        movementSpeedIncreased = 0f;
        attackRange = 1f;
        projectileSpeed = 1f;
        areaOfEffect = 1f;
        skillEffectDuration = 1f;
        statusEffectDuration = 1f;
		aggressionGainIncreased = 0f;
		focusGainIncreased = 0f;
        
        cardsDrawnPerTurn = 1;
        cardsDrawnPerWave = 1;
        maxHandSize = 10;
        discardPileSize = 0;
        exhaustPileSize = 0;
        cardDrawChance = 0f;
        cardRetentionChance = 0f;
        cardUpgradeChance = 0f;
        discardPower = 0f;
        manaPerTurn = 3f;
        cardDrawChanceIncreased = 0f;
        handSizeIncreased = 0;
        manaCostEfficiencyIncreased = 0f;
        manaRefundChance = 0f;
        cardCycleSpeedIncreased = 0f;
        preparedCardEffectivenessIncreased = 0f;
        discardCostReduction = 0;
        discardPowerIncreased = 0f;
        delayedCardEffectivenessIncreased = 0f;
        skillCardEffectivenessIncreased = 0f;
        spellPowerIncreased = 0f;
        echoCardEffectivenessIncreased = 0f; 
        
		// Weapon/Type Damage Modifiers
		increasedAxeDamage = 0f;
		increasedBowDamage = 0f;
		increasedMaceDamage = 0f;
		increasedSwordDamage = 0f;
		increasedWandDamage = 0f;
		increasedDaggerDamage = 0f;
		increasedOneHandedDamage = 0f;
		increasedTwoHandedDamage = 0f;
		
		// Guard/Defense Utilities
		guardEffectiveness = 0f;
		guardEffectivenessIncreased = 0f;
		guardRetentionIncreased = 0f;
		damageReductionWhileGuarding = 0f;
		guardBreakChance = 0f;
		lessDamageFromElites = 0f;
		statusAvoidance = 0f;
		damageReductionIncreased = 0f;
		damageReductionFromSpells = 0f;
		damageReductionWhenStunned = 0f;
		physicalDamageReductionIncreased = 0f;
		physicalReductionIncreased = 0f;
		stunDurationIncreased = 0f;
		staggerEffectivenessIncreased = 0f;
		increasedDamageToStaggered = 0f;
		reducedEnemyStaggerThreshold = 0f;
		increasedStaggerDuration = 0f;
		increasedDamageVsMarked = 0f;
		chainChance = 0f;
		spellEffectVsAilmentIncreased = 0f;
		
        
    }
    
    private void InitializeFromCharacter(Character character)
    {
        if (character == null) return;
        
        // Copy core attributes
        strength = character.strength;
        dexterity = character.dexterity;
        intelligence = character.intelligence;
        
        // Copy combat resources
        maxHealth = character.maxHealth;
        currentHealth = character.currentHealth;
        maxEnergyShield = character.maxEnergyShield;
        currentEnergyShield = character.currentEnergyShield;
        maxMana = character.maxMana;
        currentMana = character.mana;
        maxReliance = character.maxReliance;
        currentReliance = character.reliance;
        currentGuard = character.currentGuard;
        
        // Copy combat stats
        attackPower = character.attackPower;
        defense = character.defense;
        criticalChance = character.criticalChance;
        criticalMultiplier = character.criticalMultiplier;
        
        // Copy damage modifiers from character
        if (character.damageModifiers != null)
        {
            increasedPhysicalDamage = character.damageModifiers.increasedPhysicalDamage.Sum();
            increasedFireDamage = character.damageModifiers.increasedFireDamage.Sum();
            increasedColdDamage = character.damageModifiers.increasedColdDamage.Sum();
            increasedLightningDamage = character.damageModifiers.increasedLightningDamage.Sum();
            increasedChaosDamage = character.damageModifiers.increasedChaosDamage.Sum();
            
            addedPhysicalDamage = character.damageModifiers.addedPhysicalDamage;
            addedFireDamage = character.damageModifiers.addedFireDamage;
            addedColdDamage = character.damageModifiers.addedColdDamage;
            addedLightningDamage = character.damageModifiers.addedLightningDamage;
            addedChaosDamage = character.damageModifiers.addedChaosDamage;
        }
        
        // Copy resistances
        if (character.damageStats != null)
        {
            physicalResistance = character.damageStats.physicalResistance;
            fireResistance = character.damageStats.fireResistance;
            coldResistance = character.damageStats.coldResistance;
            lightningResistance = character.damageStats.lightningResistance;
            chaosResistance = character.damageStats.chaosResistance;
        }
        
        // Copy card system stats
        cardsDrawnPerTurn = character.cardsDrawnPerTurn;
        cardsDrawnPerWave = character.cardsDrawnPerWave;
        manaRegeneration = character.manaRecoveryPerTurn;
        manaPerTurn = character.manaRecoveryPerTurn; // Copy mana per turn
        discardPower = 0f; // Default discard power (can be modified by equipment/effects)

        // Speed stats
        attackSpeed = character.GetAttackSpeedMultiplier();
        castSpeed = character.GetCastSpeedMultiplier();
        movementSpeed = character.GetMovementSpeedMultiplier();
        
        // Calculate elemental damage totals
        increasedElementalDamage = increasedFireDamage + increasedColdDamage + increasedLightningDamage;
        addedElementalDamage = addedFireDamage + addedColdDamage + addedLightningDamage;
        elementalResistance = (fireResistance + coldResistance + lightningResistance) / 3f;
        
        // Calculate all resistance
        allResistance = (physicalResistance + fireResistance + coldResistance + lightningResistance + chaosResistance) / 5f;
    }
    
    // Get a stat value by name (for compatibility with affix system)
    public float GetStatValue(string statName)
    {
        switch (statName.ToLower())
        {
            // Core attributes
            case "strength": return strength;
            case "dexterity": return dexterity;
            case "intelligence": return intelligence;
            
            // Combat resources
            case "maxhealth": return maxHealth;
            case "currenthealth": return currentHealth;
            case "maxenergyshield": return maxEnergyShield;
            case "currentenergyshield": return currentEnergyShield;
            case "maxmana": return maxMana;
            case "currentmana": return currentMana;
            case "maxreliance": return maxReliance;
            case "currentreliance": return currentReliance;
            case "currentguard": return currentGuard;
            
            // Combat stats
            case "attackpower": return attackPower;
            case "defense": return defense;
            case "criticalchance": return criticalChance;
            case "criticalmultiplier": return criticalMultiplier;
            case "accuracy": return accuracy;
            case "evasion": return evasion;
            case "evasionincreased": return evasionIncreased;
            case "blockchance": return blockChance;
            case "dodgechance": return dodgeChance;
			case "maxhealthincreased": return maxHealthIncreased;
			case "maxmanaincreased": return maxManaIncreased;
			case "energyshieldincreased": return energyShieldIncreased;
			case "armourincreased": return armourIncreased;
            
            // Damage modifiers
            case "increasedphysicaldamage": return increasedPhysicalDamage;
            case "increasedfiredamage": return increasedFireDamage;
            case "increasedcolddamage": return increasedColdDamage;
            case "increasedlightningdamage": return increasedLightningDamage;
            case "increasedchaosdamage": return increasedChaosDamage;
            case "increasedelementaldamage": return increasedElementalDamage;
            case "increasedspelldamage": return increasedSpellDamage;
            case "increasedattackdamage": return increasedAttackDamage;
            
            // Added damage
            case "addedphysicaldamage": return addedPhysicalDamage;
            case "addedfiredamage": return addedFireDamage;
            case "addedcolddamage": return addedColdDamage;
            case "addedlightningdamage": return addedLightningDamage;
            case "addedchaosdamage": return addedChaosDamage;
            case "addedelementaldamage": return addedElementalDamage;
            case "addedspelldamage": return addedSpellDamage;
            case "addedattackdamage": return addedAttackDamage;
            
            // Resistances
            case "physicalresistance": return physicalResistance;
            case "fireresistance": return fireResistance;
            case "coldresistance": return coldResistance;
            case "lightningresistance": return lightningResistance;
            case "chaosresistance": return chaosResistance;
            case "elementalresistance": return elementalResistance;
            case "allresistance": return allResistance;
            
            // Defense stats
            case "armour": return armour;
            case "energyshield": return energyShield;
            case "spelldodgechance": return spellDodgeChance;
            case "spellblockchance": return spellBlockChance;
            
            // Ailment stats
            case "chancetoshock": return chanceToShock;
            case "chancetochill": return chanceToChill;
            case "chancetofreeze": return chanceToFreeze;
            case "chancetoignite": return chanceToIgnite;
            case "chancetobleed": return chanceToBleed;
            case "chancetopoison": return chanceToPoison;
            case "increasedignitemagnitude": return increasedIgniteMagnitude;
            case "increasedshockmagnitude": return increasedShockMagnitude;
            case "increasedchillmagnitude": return increasedChillMagnitude;
            case "increasedfreezemagnitude": return increasedFreezeMagnitude;
            case "increasedbleedmagnitude": return increasedBleedMagnitude;
            case "increasedpoisonmagnitude": return increasedPoisonMagnitude;
			case "increaseddamageovertime": return increasedDamageOverTime;
			case "increasedpoisondamage": return increasedPoisonDamage;
			case "increasedpoisonduration": return increasedPoisonDuration;
			case "increaseddamagevschilled": return increasedDamageVsChilled;
			case "increaseddamagevsshocked": return increasedDamageVsShocked;
			case "increaseddamagevsignited": return increasedDamageVsIgnited;
			case "increaseddamagevscursed": return increasedDamageVsCursed;
			case "increaseddamagevsblocked": return increasedDamageVsBlocked;
			case "increaseddamagevsslowed": return increasedDamageVsSlowed;
			case "increaseddamagewhileinshadow": return increasedDamageWhileInShadow;
			case "increaseddamageoncriticalstrikes": return increasedDamageOnCriticalStrikes;
			case "increaseddamageafterdiscard": return increasedDamageAfterDiscard;
			case "increaseddamagewhenbelowlifethreshold": return increasedDamageWhenBelowLifeThreshold;
			case "increaseddamagevsrareandunique": return increasedDamageVsRareAndUnique;
			case "increaseddamagewithconsecutiveattacks": return increasedDamageWithConsecutiveAttacks;
			case "increaseddamageoneverynthattack": return increasedDamageOnEveryNthAttack;
			case "increaseddamagevsmarked": return increasedDamageVsMarked;
			
			// Ailment & Status Effect Modifiers
			case "ailmentdurationincreased": return ailmentDurationIncreased;
			case "ailmentapplicationchanceincreased": return ailmentApplicationChanceIncreased;
			case "slowchance": return slowChance;
			case "freezechance": return freezeChance;
			case "curseapplicationchance": return curseApplicationChance;
			case "cursedurationincreased": return curseDurationIncreased;
			case "curseeffectivenessincreased": return curseEffectivenessIncreased;
			case "randomailmentchance": return randomAilmentChance;
			case "chilleffectivenessincreased": return chillEffectivenessIncreased;
			case "shockeffectivenessincreased": return shockEffectivenessIncreased;
            
            // Recovery stats
            case "liferegeneration": return lifeRegeneration;
            case "energyshieldregeneration": return energyShieldRegeneration;
            case "manaregeneration": return manaRegeneration;
            case "relianceregeneration": return relianceRegeneration;
            case "lifesteal": return lifeSteal;
            case "manasteal": return manaSteal;
            case "lifeleech": return lifeSteal; // Legacy support
            case "manaleech": return manaSteal; // Legacy support
            case "energyshieldleech": return energyShieldLeech;
            
            // Combat mechanics
            case "attackspeed": return attackSpeed;
            case "castspeed": return castSpeed;
            case "movementspeed": return movementSpeed;
            case "attackrange": return attackRange;
            case "projectilespeed": return projectileSpeed;
            case "areaofeffect": return areaOfEffect;
            case "skilleffectduration": return skillEffectDuration;
            case "statuseffectduration": return statusEffectDuration;
			case "aggressiongainincreased": return aggressionGainIncreased;
			case "focusgainincreased": return focusGainIncreased;
            
            // Card system stats
            case "cardsdrawnperturn": return cardsDrawnPerTurn;
            case "maxhandsize": return maxHandSize;
            case "discardpilesize": return discardPileSize;
            case "exhaustpilesize": return exhaustPileSize;
            case "carddrawchance": return cardDrawChance;
            case "cardretentionchance": return cardRetentionChance;
            case "cardupgradechance": return cardUpgradeChance;
            
            // Card System Modifiers
            case "carddrawchanceincreased": return cardDrawChanceIncreased;
            case "handsizeincreased": return handSizeIncreased;
            case "manacostefficiencyincreased": return manaCostEfficiencyIncreased;
            case "manarefundchance": return manaRefundChance;
            case "cardcyclespeedincreased": return cardCycleSpeedIncreased;
            case "preparedcardeffectivenessincreased": return preparedCardEffectivenessIncreased;
            case "discardcostreduction": return discardCostReduction;
            case "discardpowerincreased": return discardPowerIncreased;
            case "delayedcardeffectivenessincreased": return delayedCardEffectivenessIncreased;
            case "skillcardeffectivenessincreased": return skillCardEffectivenessIncreased;
			case "spellpowerincreased": return spellPowerIncreased;
			case "echocardeffectivenessincreased": return echoCardEffectivenessIncreased;
			case "chainchance": return chainChance;
			case "spelleffectvsailmentincreased": return spellEffectVsAilmentIncreased;
            
            // Passive tree stats removed - passive tree system deleted
			
			// Critical Strike Modifiers
			case "increasedcriticalstrikechance": return increasedCriticalStrikeChance;
			case "increasedcriticalstrikemultiplier": return increasedCriticalStrikeMultiplier;
			case "increasedcriticalchancewithdaggers": return increasedCriticalChanceWithDaggers;
			case "increasedcriticalchancewithswords": return increasedCriticalChanceWithSwords;
			case "increasedcriticalmultiplierwithdaggers": return increasedCriticalMultiplierWithDaggers;
			case "increasedcriticalmultiplierwithswords": return increasedCriticalMultiplierWithSwords;
			case "increasedcriticalchancewithcoldskills": return increasedCriticalChanceWithColdSkills;
			case "increasedcriticalchancewithlightningcards": return increasedCriticalChanceWithLightningCards;
			case "increasedcriticalchancevsfulllife": return increasedCriticalChanceVsFullLife;
			case "increasedcriticalchancevscursed": return increasedCriticalChanceVsCursed;
			case "increasedcriticalchancewithprojectilecards": return increasedCriticalChanceWithProjectileCards;
			case "increasedcriticalchancewithpreparedcards": return increasedCriticalChanceWithPreparedCards;
			case "increasedspellcriticalchance": return increasedSpellCriticalChance;
			case "increasedspellcriticalmultiplier": return increasedSpellCriticalMultiplier;
			case "increasedcriticaldamagemultiplier": return increasedCriticalDamageMultiplier;
			
			// Recovery Modifiers
			case "liferegenerationincreased": return lifeRegenerationIncreased;
			case "liferecoveryrateincreased": return lifeRecoveryRateIncreased;
			case "manaregenerationincreased": return manaRegenerationIncreased;
			case "lifestealonhitincreased": return lifeStealOnHitIncreased;
			case "manaleechchance": return manaLeechChance;
			case "lifeonkill": return lifeOnKill;
			case "lifecostefficiencyincreased": return lifeCostEfficiencyIncreased;
			
			// Block & Dodge Modifiers
			case "blockchanceincreased": return blockChanceIncreased;
			case "blockeffectivenessincreased": return blockEffectivenessIncreased;
			case "dodgechanceincreased": return dodgeChanceIncreased;
			case "criticalstrikeavoidance": return criticalStrikeAvoidance;
			case "debuffexpirationrateincreased": return debuffExpirationRateIncreased;
			
			// Resistance Modifiers
			case "physicalresistanceincreased": return physicalResistanceIncreased;
			case "allelementalresistancesincreased": return allElementalResistancesIncreased;
			case "fireresistancepenetration": return fireResistancePenetration;
			
			// Movement & Mobility
			case "movementspeedincreased": return movementSpeedIncreased;
			
			// Weapon/Type Damage Modifiers
			case "increasedaxedamage": return increasedAxeDamage;
			case "increasedbowdamage": return increasedBowDamage;
			case "increasedmacedamage": return increasedMaceDamage;
			case "increasedsworddamage": return increasedSwordDamage;
			case "increasedwanddamage": return increasedWandDamage;
			case "increaseddaggerdamage": return increasedDaggerDamage;
			case "increasedonehandeddamage": return increasedOneHandedDamage;
			case "increasedtwohandeddamage": return increasedTwoHandedDamage;
			
			// Guard/Defense Utilities
			case "guardeffectiveness": return guardEffectiveness;
			case "guardeffectivenessincreased": return guardEffectivenessIncreased;
			case "guardretentionincreased": return guardRetentionIncreased;
			case "damagereductionwhileguarding": return damageReductionWhileGuarding;
			case "guardbreakchance": return guardBreakChance;
			case "lessdamagefromelites": return lessDamageFromElites;
			case "statusavoidance": return statusAvoidance;
			
			// Damage Reduction & Mitigation
			case "damagereductionincreased": return damageReductionIncreased;
			case "damagereductionfromspells": return damageReductionFromSpells;
			case "damagereductionwhenstunned": return damageReductionWhenStunned;
			case "physicaldamagereductionincreased": return physicalDamageReductionIncreased;
			case "physicalreductionincreased": return physicalReductionIncreased;
			
			// Stun & Crowd Control
			case "stundurationincreased": return stunDurationIncreased;
			case "staggereffectivenessincreased": return staggerEffectivenessIncreased;
			case "increaseddamagetostaggered": return increasedDamageToStaggered;
			case "reducedenemystaggerthreshold": return reducedEnemyStaggerThreshold;
			case "increasedstaggerduration": return increasedStaggerDuration;
            
            // Check equipment stats
            case var s when equipmentStats.ContainsKey(s): return equipmentStats[s];
            
            // Check temporary buffs
            case var s when temporaryBuffs.ContainsKey(s): return temporaryBuffs[s];
            
            // Check calculated totals
            case var s when calculatedTotals.ContainsKey(s): return calculatedTotals[s];
            
            default: return 0f;
        }
    }
    
    // Set a stat value by name
    public void SetStatValue(string statName, float value)
    {
        switch (statName.ToLower())
        {
            case "strength": strength = Mathf.RoundToInt(value); break;
            case "dexterity": dexterity = Mathf.RoundToInt(value); break;
            case "intelligence": intelligence = Mathf.RoundToInt(value); break;
            case "maxhealth": maxHealth = Mathf.RoundToInt(value); break;
            case "currenthealth": currentHealth = Mathf.RoundToInt(value); break;
            case "maxenergyshield": maxEnergyShield = Mathf.RoundToInt(value); break;
            case "currentenergyshield": currentEnergyShield = Mathf.RoundToInt(value); break;
            case "maxmana": maxMana = Mathf.RoundToInt(value); break;
            case "currentmana": currentMana = Mathf.RoundToInt(value); break;
            case "maxreliance": maxReliance = Mathf.RoundToInt(value); break;
            case "currentreliance": currentReliance = Mathf.RoundToInt(value); break;
            case "currentguard": currentGuard = value; break;
            case "attackpower": attackPower = Mathf.RoundToInt(value); break;
            case "defense": defense = Mathf.RoundToInt(value); break;
            case "criticalchance": criticalChance = value; break;
            case "criticalmultiplier": criticalMultiplier = value; break;
            case "accuracy": accuracy = value; break;
            case "evasion": evasion = value; break;
            case "evasionincreased": evasionIncreased = value; break;
			case "maxhealthincreased": maxHealthIncreased = value; break;
			case "maxmanaincreased": maxManaIncreased = value; break;
			case "energyshieldincreased": energyShieldIncreased = value; break;
			case "armourincreased": armourIncreased = value; break;
            case "increasedphysicaldamage": increasedPhysicalDamage = value; break;
            case "increasedfiredamage": increasedFireDamage = value; break;
            case "increasedcolddamage": increasedColdDamage = value; break;
            case "increasedlightningdamage": increasedLightningDamage = value; break;
            case "increasedchaosdamage": increasedChaosDamage = value; break;
            case "increasedelementaldamage": increasedElementalDamage = value; break;
            case "increasedspelldamage": increasedSpellDamage = value; break;
            case "increasedattackdamage": increasedAttackDamage = value; break;
            case "addedphysicaldamage": addedPhysicalDamage = value; break;
            case "addedfiredamage": addedFireDamage = value; break;
            case "addedcolddamage": addedColdDamage = value; break;
            case "addedlightningdamage": addedLightningDamage = value; break;
            case "addedchaosdamage": addedChaosDamage = value; break;
            case "addedelementaldamage": addedElementalDamage = value; break;
            case "addedspelldamage": addedSpellDamage = value; break;
            case "addedattackdamage": addedAttackDamage = value; break;
            case "physicalresistance": physicalResistance = value; break;
            case "fireresistance": fireResistance = value; break;
            case "coldresistance": coldResistance = value; break;
            case "lightningresistance": lightningResistance = value; break;
            case "chaosresistance": chaosResistance = value; break;
            case "elementalresistance": elementalResistance = value; break;
            case "allresistance": allResistance = value; break;
            case "armour": armour = Mathf.RoundToInt(value); break;
            case "energyshield": energyShield = Mathf.RoundToInt(value); break;
            case "blockchance": blockChance = value; break;
            case "dodgechance": dodgeChance = value; break;
            case "spelldodgechance": spellDodgeChance = value; break;
            case "spellblockchance": spellBlockChance = value; break;
            
            // Ailment stats
            case "chancetoshock": chanceToShock = value; break;
            case "chancetochill": chanceToChill = value; break;
            case "chancetofreeze": chanceToFreeze = value; break;
            case "chancetoignite": chanceToIgnite = value; break;
            case "chancetobleed": chanceToBleed = value; break;
            case "chancetopoison": chanceToPoison = value; break;
            case "increasedignitemagnitude": increasedIgniteMagnitude = value; break;
            case "increasedshockmagnitude": increasedShockMagnitude = value; break;
            case "increasedchillmagnitude": increasedChillMagnitude = value; break;
            case "increasedfreezemagnitude": increasedFreezeMagnitude = value; break;
            case "increasedbleedmagnitude": increasedBleedMagnitude = value; break;
            case "increasedpoisonmagnitude": increasedPoisonMagnitude = value; break;
			case "increaseddamageovertime": increasedDamageOverTime = value; break;
			case "increasedpoisondamage": increasedPoisonDamage = value; break;
			case "increasedpoisonduration": increasedPoisonDuration = value; break;
			case "increaseddamagevschilled": increasedDamageVsChilled = value; break;
			case "increaseddamagevsshocked": increasedDamageVsShocked = value; break;
			case "increaseddamagevsignited": increasedDamageVsIgnited = value; break;
			case "increaseddamagevscursed": increasedDamageVsCursed = value; break;
			case "increaseddamagevsblocked": increasedDamageVsBlocked = value; break;
			case "increaseddamagevsslowed": increasedDamageVsSlowed = value; break;
			case "increaseddamagewhileinshadow": increasedDamageWhileInShadow = value; break;
			case "increaseddamageoncriticalstrikes": increasedDamageOnCriticalStrikes = value; break;
			case "increaseddamageafterdiscard": increasedDamageAfterDiscard = value; break;
			case "increaseddamagewhenbelowlifethreshold": increasedDamageWhenBelowLifeThreshold = value; break;
			case "increaseddamagevsrareandunique": increasedDamageVsRareAndUnique = value; break;
			case "increaseddamagewithconsecutiveattacks": increasedDamageWithConsecutiveAttacks = value; break;
			case "increaseddamageoneverynthattack": increasedDamageOnEveryNthAttack = value; break;
			case "increaseddamagevsmarked": increasedDamageVsMarked = value; break;
			
			// Ailment & Status Effect Modifiers
			case "ailmentdurationincreased": ailmentDurationIncreased = value; break;
			case "ailmentapplicationchanceincreased": ailmentApplicationChanceIncreased = value; break;
			case "slowchance": slowChance = value; break;
			case "freezechance": freezeChance = value; break;
			case "curseapplicationchance": curseApplicationChance = value; break;
			case "cursedurationincreased": curseDurationIncreased = value; break;
			case "curseeffectivenessincreased": curseEffectivenessIncreased = value; break;
			case "randomailmentchance": randomAilmentChance = value; break;
			case "chilleffectivenessincreased": chillEffectivenessIncreased = value; break;
			case "shockeffectivenessincreased": shockEffectivenessIncreased = value; break;
            
            case "liferegeneration": lifeRegeneration = value; break;
            case "energyshieldregeneration": energyShieldRegeneration = value; break;
            case "manaregeneration": manaRegeneration = value; break;
            case "relianceregeneration": relianceRegeneration = value; break;
            case "lifesteal": lifeSteal = value; break;
            case "manasteal": manaSteal = value; break;
            case "lifeleech": lifeSteal = value; break; // Legacy support
            case "manaleech": manaSteal = value; break; // Legacy support
            case "energyshieldleech": energyShieldLeech = value; break;
            case "attackspeed": attackSpeed = value; break;
            case "castspeed": castSpeed = value; break;
            case "movementspeed": movementSpeed = value; break;
            case "attackrange": attackRange = value; break;
            case "projectilespeed": projectileSpeed = value; break;
            case "areaofeffect": areaOfEffect = value; break;
            case "skilleffectduration": skillEffectDuration = value; break;
            case "statuseffectduration": statusEffectDuration = value; break;
			case "aggressiongainincreased": aggressionGainIncreased = value; break;
			case "focusgainincreased": focusGainIncreased = value; break;
            case "cardsdrawnperturn": cardsDrawnPerTurn = Mathf.RoundToInt(value); break;
            case "maxhandsize": maxHandSize = Mathf.RoundToInt(value); break;
            case "discardpilesize": discardPileSize = Mathf.RoundToInt(value); break;
            case "exhaustpilesize": exhaustPileSize = Mathf.RoundToInt(value); break;
            case "carddrawchance": cardDrawChance = value; break;
            case "cardretentionchance": cardRetentionChance = value; break;
            case "cardupgradechance": cardUpgradeChance = value; break;
            
            // Card System Modifiers
            case "carddrawchanceincreased": cardDrawChanceIncreased = value; break;
            case "handsizeincreased": handSizeIncreased = Mathf.RoundToInt(value); break;
            case "manacostefficiencyincreased": manaCostEfficiencyIncreased = value; break;
            case "manarefundchance": manaRefundChance = value; break;
            case "cardcyclespeedincreased": cardCycleSpeedIncreased = value; break;
            case "preparedcardeffectivenessincreased": preparedCardEffectivenessIncreased = value; break;
            case "discardcostreduction": discardCostReduction = Mathf.RoundToInt(value); break;
            case "discardpowerincreased": discardPowerIncreased = value; break;
            case "delayedcardeffectivenessincreased": delayedCardEffectivenessIncreased = value; break;
            case "skillcardeffectivenessincreased": skillCardEffectivenessIncreased = value; break;
			case "spellpowerincreased": spellPowerIncreased = value; break;
			case "echocardeffectivenessincreased": echoCardEffectivenessIncreased = value; break;
			case "chainchance": chainChance = value; break;
			case "spelleffectvsailmentincreased": spellEffectVsAilmentIncreased = value; break;
            
            // Passive tree points removed - passive tree system deleted
			
			// Critical Strike Modifiers
			case "increasedcriticalstrikechance": increasedCriticalStrikeChance = value; break;
			case "increasedcriticalstrikemultiplier": increasedCriticalStrikeMultiplier = value; break;
			case "increasedcriticalchancewithdaggers": increasedCriticalChanceWithDaggers = value; break;
			case "increasedcriticalchancewithswords": increasedCriticalChanceWithSwords = value; break;
			case "increasedcriticalmultiplierwithdaggers": increasedCriticalMultiplierWithDaggers = value; break;
			case "increasedcriticalmultiplierwithswords": increasedCriticalMultiplierWithSwords = value; break;
			case "increasedcriticalchancewithcoldskills": increasedCriticalChanceWithColdSkills = value; break;
			case "increasedcriticalchancewithlightningcards": increasedCriticalChanceWithLightningCards = value; break;
			case "increasedcriticalchancevsfulllife": increasedCriticalChanceVsFullLife = value; break;
			case "increasedcriticalchancevscursed": increasedCriticalChanceVsCursed = value; break;
			case "increasedcriticalchancewithprojectilecards": increasedCriticalChanceWithProjectileCards = value; break;
			case "increasedcriticalchancewithpreparedcards": increasedCriticalChanceWithPreparedCards = value; break;
			case "increasedspellcriticalchance": increasedSpellCriticalChance = value; break;
			case "increasedspellcriticalmultiplier": increasedSpellCriticalMultiplier = value; break;
			case "increasedcriticaldamagemultiplier": increasedCriticalDamageMultiplier = value; break;
			
			// Recovery Modifiers
			case "liferegenerationincreased": lifeRegenerationIncreased = value; break;
			case "liferecoveryrateincreased": lifeRecoveryRateIncreased = value; break;
			case "manaregenerationincreased": manaRegenerationIncreased = value; break;
			case "lifestealonhitincreased": lifeStealOnHitIncreased = value; break;
			case "manaleechchance": manaLeechChance = value; break;
			case "lifeonkill": lifeOnKill = value; break;
			case "lifecostefficiencyincreased": lifeCostEfficiencyIncreased = value; break;
			
			// Block & Dodge Modifiers
			case "blockchanceincreased": blockChanceIncreased = value; break;
			case "blockeffectivenessincreased": blockEffectivenessIncreased = value; break;
			case "dodgechanceincreased": dodgeChanceIncreased = value; break;
			case "criticalstrikeavoidance": criticalStrikeAvoidance = value; break;
			case "debuffexpirationrateincreased": debuffExpirationRateIncreased = value; break;
			
			// Resistance Modifiers
			case "physicalresistanceincreased": physicalResistanceIncreased = value; break;
			case "allelementalresistancesincreased": allElementalResistancesIncreased = value; break;
			case "fireresistancepenetration": fireResistancePenetration = value; break;
			
			// Movement & Mobility
			case "movementspeedincreased": movementSpeedIncreased = value; break;
			
			// Weapon/Type Damage Modifiers
			case "increasedaxedamage": increasedAxeDamage = value; break;
			case "increasedbowdamage": increasedBowDamage = value; break;
			case "increasedmacedamage": increasedMaceDamage = value; break;
			case "increasedsworddamage": increasedSwordDamage = value; break;
			case "increasedwanddamage": increasedWandDamage = value; break;
			case "increaseddaggerdamage": increasedDaggerDamage = value; break;
			case "increasedonehandeddamage": increasedOneHandedDamage = value; break;
			case "increasedtwohandeddamage": increasedTwoHandedDamage = value; break;
			
			// Guard/Defense Utilities
			case "guardeffectiveness": guardEffectiveness = value; break;
			case "guardeffectivenessincreased": guardEffectivenessIncreased = value; break;
			case "guardretentionincreased": guardRetentionIncreased = value; break;
			case "damagereductionwhileguarding": damageReductionWhileGuarding = value; break;
			case "guardbreakchance": guardBreakChance = value; break;
			case "lessdamagefromelites": lessDamageFromElites = value; break;
			case "statusavoidance": statusAvoidance = value; break;
			
			// Damage Reduction & Mitigation
			case "damagereductionincreased": damageReductionIncreased = value; break;
			case "damagereductionfromspells": damageReductionFromSpells = value; break;
			case "damagereductionwhenstunned": damageReductionWhenStunned = value; break;
			case "physicaldamagereductionincreased": physicalDamageReductionIncreased = value; break;
			case "physicalreductionincreased": physicalReductionIncreased = value; break;
			
			// Stun & Crowd Control
			case "stundurationincreased": stunDurationIncreased = value; break;
			case "staggereffectivenessincreased": staggerEffectivenessIncreased = value; break;
			case "increaseddamagetostaggered": increasedDamageToStaggered = value; break;
			case "reducedenemystaggerthreshold": reducedEnemyStaggerThreshold = value; break;
			case "increasedstaggerduration": increasedStaggerDuration = value; break;
            default:
                // For custom stats, store in equipment stats
                equipmentStats[statName] = value;
                break;
        }
        
        // Recalculate derived stats
        RecalculateDerivedStats();
    }
    
    // Add to a stat value
    public void AddToStat(string statName, float value)
    {
        float currentValue = GetStatValue(statName);
        SetStatValue(statName, currentValue + value);
    }
    
    // Multiply a stat value
    public void MultiplyStat(string statName, float multiplier)
    {
        float currentValue = GetStatValue(statName);
        SetStatValue(statName, currentValue * multiplier);
    }
    
    // Recalculate all derived stats
    public void RecalculateDerivedStats()
    {
        // Recalculate elemental totals
        increasedElementalDamage = increasedFireDamage + increasedColdDamage + increasedLightningDamage;
        addedElementalDamage = addedFireDamage + addedColdDamage + addedLightningDamage;
        elementalResistance = (fireResistance + coldResistance + lightningResistance) / 3f;
        allResistance = (physicalResistance + fireResistance + coldResistance + lightningResistance + chaosResistance) / 5f;
        
        // Recalculate available passive points
        // Passive tree points calculation removed - passive tree system deleted
        
        // Store calculated totals
        calculatedTotals["totalincreaseddamage"] = increasedPhysicalDamage + increasedElementalDamage + increasedChaosDamage;
        calculatedTotals["totaladdeddamage"] = addedPhysicalDamage + addedElementalDamage + addedChaosDamage;
        calculatedTotals["totalresistance"] = allResistance;
        calculatedTotals["totaldefense"] = armour + evasion + energyShield;
    }
    
    // Apply equipment stats
    public void ApplyEquipmentStats(Dictionary<string, float> equipmentStats)
    {
        foreach (var stat in equipmentStats)
        {
            AddToStat(stat.Key, stat.Value);
        }
    }
    
    // Apply temporary buff
    public void ApplyTemporaryBuff(string statName, float value, float duration)
    {
        temporaryBuffs[statName] = value;
        AddToStat(statName, value);
        
        // TODO: Implement duration system with coroutines
        Debug.Log($"Applied temporary buff: {statName} +{value} for {duration} seconds");
    }
    
    // Remove temporary buff
    public void RemoveTemporaryBuff(string statName)
    {
        if (temporaryBuffs.ContainsKey(statName))
        {
            float value = temporaryBuffs[statName];
            AddToStat(statName, -value);
            temporaryBuffs.Remove(statName);
            Debug.Log($"Removed temporary buff: {statName} -{value}");
        }
    }
    
    // Get formatted stat display string
    public string GetStatDisplayString(string statName)
    {
        float value = GetStatValue(statName);
        
        // Format based on stat type
        if (statName.ToLower().Contains("chance") || statName.ToLower().Contains("resistance"))
        {
            return $"{value:F1}%";
        }
        else if (statName.ToLower().Contains("multiplier") || statName.ToLower().Contains("speed"))
        {
            return $"{value:F2}x";
        }
        else if (statName.ToLower().Contains("damage") || statName.ToLower().Contains("health") || 
                 statName.ToLower().Contains("mana") || statName.ToLower().Contains("reliance"))
        {
            return $"{value:F0}";
        }
        else
        {
            return $"{value:F1}";
        }
    }
    
    // Get all stats as a dictionary for easy iteration
    public Dictionary<string, float> GetAllStats()
    {
        var allStats = new Dictionary<string, float>();
        
        // Core attributes
        allStats["Strength"] = strength;
        allStats["Dexterity"] = dexterity;
        allStats["Intelligence"] = intelligence;
        
        // Combat resources
        allStats["Max Health"] = maxHealth;
        allStats["Current Health"] = currentHealth;
        allStats["Max Energy Shield"] = maxEnergyShield;
        allStats["Current Energy Shield"] = currentEnergyShield;
        allStats["Max Mana"] = maxMana;
        allStats["Current Mana"] = currentMana;
        allStats["Max Reliance"] = maxReliance;
        allStats["Current Reliance"] = currentReliance;
        allStats["Current Guard"] = currentGuard;
        
        // Combat stats
        allStats["Attack Power"] = attackPower;
        allStats["Defense"] = defense;
        allStats["Critical Chance"] = criticalChance;
        allStats["Critical Multiplier"] = criticalMultiplier;
        allStats["Accuracy"] = accuracy;
        allStats["Evasion"] = evasion;
        allStats["Evasion Increased %"] = evasionIncreased * 100f;
        allStats["Block Chance"] = blockChance;
        allStats["Dodge Chance"] = dodgeChance;
        
        // Add equipment stats
        foreach (var stat in equipmentStats)
        {
            allStats[stat.Key] = stat.Value;
        }
        
        return allStats;
    }
    
    // Clone this stats data
    public CharacterStatsData Clone()
    {
        var clone = new CharacterStatsData();
        
        // Copy all fields
        clone.strength = this.strength;
        clone.dexterity = this.dexterity;
        clone.intelligence = this.intelligence;
        
        clone.maxHealth = this.maxHealth;
        clone.currentHealth = this.currentHealth;
        clone.maxEnergyShield = this.maxEnergyShield;
        clone.currentEnergyShield = this.currentEnergyShield;
        clone.maxMana = this.maxMana;
        clone.currentMana = this.currentMana;
        clone.maxReliance = this.maxReliance;
        clone.currentReliance = this.currentReliance;
        clone.currentGuard = this.currentGuard;
        
        clone.attackPower = this.attackPower;
        clone.defense = this.defense;
        clone.criticalChance = this.criticalChance;
        clone.criticalMultiplier = this.criticalMultiplier;
        clone.accuracy = this.accuracy;
        clone.evasion = this.evasion;
        clone.blockChance = this.blockChance;
        clone.dodgeChance = this.dodgeChance;
        
        // Copy damage modifiers
        clone.increasedPhysicalDamage = this.increasedPhysicalDamage;
        clone.increasedFireDamage = this.increasedFireDamage;
        clone.increasedColdDamage = this.increasedColdDamage;
        clone.increasedLightningDamage = this.increasedLightningDamage;
        clone.increasedChaosDamage = this.increasedChaosDamage;
        clone.increasedElementalDamage = this.increasedElementalDamage;
        clone.increasedSpellDamage = this.increasedSpellDamage;
        clone.increasedAttackDamage = this.increasedAttackDamage;
        
        clone.addedPhysicalDamage = this.addedPhysicalDamage;
        clone.addedFireDamage = this.addedFireDamage;
        clone.addedColdDamage = this.addedColdDamage;
        clone.addedLightningDamage = this.addedLightningDamage;
        clone.addedChaosDamage = this.addedChaosDamage;
        clone.addedElementalDamage = this.addedElementalDamage;
        clone.addedSpellDamage = this.addedSpellDamage;
        clone.addedAttackDamage = this.addedAttackDamage;
        
        // Copy resistances
        clone.physicalResistance = this.physicalResistance;
        clone.fireResistance = this.fireResistance;
        clone.coldResistance = this.coldResistance;
        clone.lightningResistance = this.lightningResistance;
        clone.chaosResistance = this.chaosResistance;
        clone.elementalResistance = this.elementalResistance;
        clone.allResistance = this.allResistance;
        
        // Copy other stats
        clone.armour = this.armour;
        clone.energyShield = this.energyShield;
        clone.spellDodgeChance = this.spellDodgeChance;
        clone.spellBlockChance = this.spellBlockChance;
        
        // Copy ailment stats
        clone.chanceToShock = this.chanceToShock;
        clone.chanceToChill = this.chanceToChill;
        clone.chanceToFreeze = this.chanceToFreeze;
        clone.chanceToIgnite = this.chanceToIgnite;
        clone.chanceToBleed = this.chanceToBleed;
        clone.chanceToPoison = this.chanceToPoison;
        clone.increasedIgniteMagnitude = this.increasedIgniteMagnitude;
        clone.increasedShockMagnitude = this.increasedShockMagnitude;
        clone.increasedChillMagnitude = this.increasedChillMagnitude;
        clone.increasedFreezeMagnitude = this.increasedFreezeMagnitude;
        clone.increasedBleedMagnitude = this.increasedBleedMagnitude;
        clone.increasedPoisonMagnitude = this.increasedPoisonMagnitude;
        
        clone.lifeRegeneration = this.lifeRegeneration;
        clone.energyShieldRegeneration = this.energyShieldRegeneration;
        clone.manaRegeneration = this.manaRegeneration;
        clone.relianceRegeneration = this.relianceRegeneration;
        clone.lifeSteal = this.lifeSteal;
        clone.manaSteal = this.manaSteal;
        clone.energyShieldLeech = this.energyShieldLeech;
        
        clone.attackSpeed = this.attackSpeed;
        clone.castSpeed = this.castSpeed;
        clone.movementSpeed = this.movementSpeed;
        clone.attackRange = this.attackRange;
        clone.projectileSpeed = this.projectileSpeed;
        clone.areaOfEffect = this.areaOfEffect;
        clone.skillEffectDuration = this.skillEffectDuration;
        clone.statusEffectDuration = this.statusEffectDuration;
        
        clone.cardsDrawnPerTurn = this.cardsDrawnPerTurn;
        clone.maxHandSize = this.maxHandSize;
        clone.discardPileSize = this.discardPileSize;
        clone.exhaustPileSize = this.exhaustPileSize;
        clone.cardDrawChance = this.cardDrawChance;
        clone.cardRetentionChance = this.cardRetentionChance;
        clone.cardUpgradeChance = this.cardUpgradeChance;
        
        // Passive tree data removed - passive tree system deleted
        
        // Copy collections
        clone.equipmentStats = new Dictionary<string, float>(this.equipmentStats);
        clone.equippedItems = new List<string>(this.equippedItems);
        clone.equipmentLevels = new Dictionary<string, int>(this.equipmentLevels);
        clone.temporaryBuffs = new Dictionary<string, float>(this.temporaryBuffs);
        clone.temporaryDebuffs = new Dictionary<string, float>(this.temporaryDebuffs);
        clone.calculatedTotals = new Dictionary<string, float>(this.calculatedTotals);
        
        return clone;
    }
}
