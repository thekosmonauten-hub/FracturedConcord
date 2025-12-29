using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Dexiled.Data.Items;

[Serializable]
public class Character
{
    [Header("Basic Information")]
    public string characterName;
    public string characterClass;
    public int level = 1;
    public int experience = 0;
    public int skillPoints = 0;
    public int act = 1;
    
    [Header("Core Attributes (PoE Style)")]
    public int strength = 14;
    public int dexterity = 14;
    public int intelligence = 14;
    
    [Header("Resources")]
    public int mana = 30;
    public int maxMana = 30;
    public int manaRecoveryPerTurn = 30; // Base mana recovery per turn
    public int cardsDrawnPerTurn = 1; // Base cards drawn per turn
    public int cardsDrawnPerWave = 1; // Base cards drawn when a new wave starts (in addition to turn draw)
    public int reliance = 200;
    public int maxReliance = 200;
    
    [Header("Reliance Auras")]
    [Tooltip("List of Reliance Aura names that the character owns (can activate)")]
    public List<string> ownedRelianceAuras = new List<string>();
    
    [Tooltip("List of Reliance Aura names that are currently active (persistent buffs)")]
    public List<string> activeRelianceAuras = new List<string>();
    
    [Tooltip("Experience and level data for each owned aura")]
    public List<AuraExperienceData> auraExperienceData = new List<AuraExperienceData>();
    public float currentGuard = 0f; // Current guard amount
    public float maxGuard = 0f; // Maximum guard (based on max health)
    public float guardPersistenceFraction = 0.25f; // Fraction of guard retained between turns
    public int maxEnergyShield = 0; // Maximum energy shield
    public int currentEnergyShield = 0; // Current energy shield
    
    [Header("Stagger System")]
    public float staggerThreshold = 100f; // Amount of stagger needed to trigger stun
    public float currentStagger = 0f; // Current stagger meter value
    public float staggerDecayPerTurn = 3f; // How much stagger decays per turn (reduced decay to allow stagger buildup)
    
    [Header("Combat-Wide Modifiers")]
    [Tooltip("Bonus momentum gained from all sources (e.g., Berserker's Fury: +1 per momentum gain)")]
    public int momentumGainBonus = 0;
    
    [Header("Momentum System")]
    [System.NonSerialized]
    private MomentumManager momentumManager = new MomentumManager();
    
    /// <summary>
    /// Legacy MomentumManager property - now uses StackSystem under the hood for consistency
    /// </summary>
    [System.Obsolete("Use StackSystem.Instance.GetStacks(StackType.Momentum) instead. This wrapper is maintained for backward compatibility.")]
    public MomentumManager Momentum
    {
        get
        {
            if (momentumManager == null)
            {
                momentumManager = new MomentumManager();
            }
            // Sync with StackSystem
            if (StackSystem.Instance != null)
            {
                momentumManager.currentMomentum = StackSystem.Instance.GetStacks(StackType.Momentum);
            }
            return momentumManager;
        }
    }
    
    /// <summary>
    /// Get current momentum from StackSystem
    /// </summary>
    public int GetMomentum()
    {
        if (StackSystem.Instance != null)
        {
            return StackSystem.Instance.GetStacks(StackType.Momentum);
        }
        return 0;
    }
    
    /// <summary>
    /// Gain momentum stacks (uses StackSystem)
    /// </summary>
    public int GainMomentum(int amount)
    {
        if (StackSystem.Instance != null && amount > 0)
        {
            int before = StackSystem.Instance.GetStacks(StackType.Momentum);
            StackSystem.Instance.AddStacks(StackType.Momentum, amount);
            int after = StackSystem.Instance.GetStacks(StackType.Momentum);
            return after - before;
        }
        return 0;
    }
    
    /// <summary>
    /// Spend momentum stacks (uses StackSystem)
    /// </summary>
    public int SpendMomentum(int amount)
    {
        if (StackSystem.Instance != null && amount > 0)
        {
            int before = StackSystem.Instance.GetStacks(StackType.Momentum);
            int toSpend = Mathf.Min(amount, before);
            StackSystem.Instance.RemoveStacks(StackType.Momentum, toSpend);
            return toSpend;
        }
        return 0;
    }
    
    /// <summary>
    /// Spend all momentum stacks (uses StackSystem)
    /// </summary>
    public int SpendAllMomentum()
    {
        if (StackSystem.Instance != null)
        {
            int current = StackSystem.Instance.GetStacks(StackType.Momentum);
            if (current > 0)
            {
                StackSystem.Instance.ClearStacks(StackType.Momentum);
                return current;
            }
        }
        return 0;
    }
    
    /// <summary>
    /// Check if momentum is at or above a threshold (uses StackSystem)
    /// </summary>
    public bool HasMomentum(int threshold)
    {
        if (StackSystem.Instance != null)
        {
            return StackSystem.Instance.GetStacks(StackType.Momentum) >= threshold;
        }
        return false;
    }
    
    [Header("Delayed Actions")]
    [Tooltip("Cards queued for future turns (for Temporal Savant, etc.)")]
    public List<DelayedAction> delayedActions = new List<DelayedAction>();
    
    [Header("Derived Stats")]
    public int maxHealth;
    public int currentHealth;
    public int attackPower;
    public int defense;
    public float criticalChance;
    public float criticalMultiplier;
    
    // Derived secondary stats from attributes
    public float increasedMeleePhysicalDamage = 0f; // Additive increased (e.g., 0.15 = +15%)
    public float increasedSpellDamage = 0f; // Additive increased spell damage from Intelligence (e.g., 0.15 = +15%)
    public float increasedEnergyShieldPercent = 0f; // % increased Energy Shield from Intelligence (e.g., 0.10 = +10%)
    public float accuracyRating = 0f;
    public float increasedEvasion = 0f; // Additive increased (e.g., 0.10 = +10%)
    public float baseEvasionRating = 0f; // baseline before increased modifiers
    
    [Header("Utility & Effigy Stats")]
    public float dodgeChance = 0f;
    public float guardEffectivenessPercent = 0f;
    public float toleranceEffectivenessPercent = 0f; // Increased effectiveness of Tolerance stacks
    public float maxGuardMultiplier = 1f; // Multiplier for max guard (default 1x, Wall of War sets to 2x)
    public bool attackCannotMiss = false; // Iron Will: Attacks cannot miss
    public float attackDamageIncreasedPercent = 0f; // Increased attack damage (for Crumbling Earth)
    public float crumbleMagnitudeIncreasedPercent = 0f; // Increased Crumble magnitude (additive)
    public float crumbleMagnitudeMorePercent = 0f; // More Crumble magnitude (multiplicative)
    public float crumbleDurationIncreasedPercent = 0f; // Increased Crumble duration
    public float crumbleExplosionHealPercent = 0f; // Heal percent from Crumble explosions
    public float maxManaPercentIncrease = 0f; // Percent increase to max mana
    public bool immuneToBleed = false; // Stoneskin: Immune to Bleed
    public bool immuneToIgnite = false; // Stoneskin: Immune to Ignite
    public float buffDurationIncreasedPercent = 0f;
    public float randomAilmentChancePercent = 0f;
    public float increasedDamageAfterGuardPercent = 0f;
    public float increasedDamagePercent = 0f; // General increased damage (for Disciple of War and Profane Vessel nodes)
    public float criticalStrikeChance = 0f; // General critical strike chance (for Disciple of War nodes)
    
    [Header("Profane Vessel Modifiers")]
    public float chaosResistancePercent = 0f; // Chaos resistance (stored separately from damageStats for modifiers)
    public float reducedSelfInflictedDamagePercent = 0f; // Reduced self-inflicted damage
    public float increasedSelfInflictedDamagePercent = 0f; // Increased self-inflicted damage
    public float increasedChaosDamagePercent = 0f; // Increased Chaos damage
    public float corruptionGainRatePercent = 0f; // Corruption gain rate multiplier
    
    [Header("Archanum Bladeweaver Modifiers")]
    public float increasedElementalDamageWithAttacksPercent = 0f; // Increased elemental damage with attacks
    public float weaponAttacksScaleWithSpellPowerPercent = 0f; // Weapon attacks scale with % of Spell Power
    public float spellsGainDamageFromWeaponPercent = 0f; // Spells gain damage equal to % of weapon damage
    
    [Header("Temporal Savant Modifiers")]
    public float temporalCardDrawChancePercent = 0f; // Chance to draw Temporal cards
    // Note: increasedEnergyShieldPercent is defined in "Derived secondary stats from attributes" section (line 167)
    public float increasedTemporalCardEffectivenessPercent = 0f; // Increased effectiveness of Temporal cards
    public float temporalCardEffectBonusPercent = 0f; // Bonus effect for Temporal cards (Borrowed Power)
    public int temporalCardManaCostIncrease = 0; // Mana cost increase for Temporal cards (Borrowed Power)
    
    [Header("Speed Modifiers")]
    [Tooltip("Total % increased attack speed from items/passives (additive).")]
    public float attackSpeedIncreasedPercent = 0f;
    [Tooltip("Total % increased cast speed from items/passives (additive).")]
    public float castSpeedIncreasedPercent = 0f;
    [Tooltip("Total % increased movement speed from items/passives (additive).")]
    public float movementSpeedIncreasedPercent = 0f;
    
    [Header("Damage Modifiers")]
    public DamageModifiers damageModifiers = new DamageModifiers();
    public float increasedDamage = 0f; // Total increased damage modifier
    public float moreDamage = 1f; // Total more damage multiplier
    
    [Header("Warrant Stat Modifiers")]
    [Tooltip("Non-damage warrant modifiers (e.g., evasionIncreased, maxHealthIncreased). Applied to CharacterStatsData when creating stat snapshots.")]
    public Dictionary<string, float> warrantStatModifiers = new Dictionary<string, float>();
    
    [Tooltip("Flat warrant modifiers (e.g., maxHealthFlat, maxManaFlat). Applied before percentage modifiers.")]
    public Dictionary<string, float> warrantFlatModifiers = new Dictionary<string, float>();
    
    [Tooltip("Warrant attribute bonuses (strength, dexterity, intelligence). Tracked separately so they can be properly removed when warrants are unequipped.")]
    public Dictionary<string, int> warrantAttributeBonuses = new Dictionary<string, int>();
    
    [Header("Base Defense Values from Items")]
    [Tooltip("Base defense values from equipped items (before increased modifiers). Used for scaling with evasionIncreased, armourIncreased, energyShieldIncreased.")]
    public float baseEvasionFromItems = 0f;
    public float baseArmourFromItems = 0f;
    public float baseEnergyShieldFromItems = 0f;
    
    [Header("Damage Stats")]
    public DamageStats damageStats = new DamageStats();
    
    [Header("Game State")]
    public Vector3 lastPosition = Vector3.zero;
    public string currentScene = "MainGameUI";
    public DateTime lastSaveTime;
    public bool isNewCharacter = true;
    
    [Header("Inventory & Equipment")]
    public List<string> inventory = new List<string>();
    public Dictionary<string, string> equippedItems = new Dictionary<string, string>();
    public List<Effigy> ownedEffigies = new List<Effigy>();
    public List<Effigy> equippedEffigies = new List<Effigy>();
    
    [Header("Warrants")]
    [Tooltip("List of warrant instances owned by this character. Persisted across scenes.")]
    public List<WarrantInstanceData> ownedWarrants = new List<WarrantInstanceData>();
    
    [Header("Forge Materials")]
    [Tooltip("Materials salvaged from items, used for crafting.")]
    public List<ForgeMaterialData> forgeMaterials = new List<ForgeMaterialData>();
    
    [Header("Progression")]
    public List<string> unlockedAbilities = new List<string>();
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    public List<int> completedEncounterIDs = new List<int>();
    public List<int> unlockedEncounterIDs = new List<int>();
    public List<int> enteredEncounterIDs = new List<int>(); // Tracks encounters that have been entered (not necessarily completed)
    public List<string> completedQuestIDs = new List<string>();
    public List<string> completedTutorialIDs = new List<string>(); // Tracks completed tutorials (e.g., "warrant_tutorial")
    
    [Header("Weapons")]
    public CharacterWeapons weapons = new CharacterWeapons();
    
    [Header("Deck & Cards")]
    public Deck currentDeck; // Legacy - for backward compatibility
    public CharacterDeckData deckData = new CharacterDeckData(); // New: deck management & card collection
    
    [Header("Ascendancy")]
    public CharacterAscendancyProgress ascendancyProgress = new CharacterAscendancyProgress();
    
    [System.NonSerialized]
    private ChannelingTracker channelingTracker = new ChannelingTracker();

    public ChannelingTracker Channeling
    {
        get
        {
            if (channelingTracker == null)
            {
                channelingTracker = new ChannelingTracker();
            }
            return channelingTracker;
        }
    }
    
    // Constructor
    public Character(string name, string characterClass)
    {
        this.characterName = name;
        this.characterClass = characterClass;
        this.lastSaveTime = DateTime.Now;
        InitializeStats();
        ResetRuntimeState();
    }
    
    // Initialize stats based on Path of Exile-style class system
    private void InitializeStats()
    {
        ResetAttributesToBase();
        
        // All classes start with 30 Mana and 200 Reliance
        mana = 30;
        maxMana = 30;
        manaRecoveryPerTurn = 30; // Base mana recovery per turn
        cardsDrawnPerTurn = 1; // Base cards drawn per turn
        cardsDrawnPerWave = 1; // Base cards drawn per wave
        reliance = 200;
        maxReliance = 200;

        guardPersistenceFraction = 0.25f;

        CalculateDerivedStats();
    }
    
    /// <summary>
    /// Reset attributes to their base values based on character class and level
    /// Used when clearing warrant modifiers to restore base attributes (including level-up gains)
    /// </summary>
    private void ResetAttributesToBase()
    {
        // Get base attributes for this class
        int baseStr, baseDex, baseInt;
        switch (characterClass.ToLower())
        {
            // Primary Classes (Single Attribute Focus)
            case "marauder":
                baseStr = 32;
                baseDex = 14;
                baseInt = 14;
                break;
            case "ranger":
                baseStr = 14;
                baseDex = 32;
                baseInt = 14;
                break;
            case "witch":
                baseStr = 14;
                baseDex = 14;
                baseInt = 32;
                break;
            
            // Hybrid Classes (Dual Attribute Focus)
            case "brawler":
                baseStr = 23;
                baseDex = 23;
                baseInt = 14;
                break;
            case "thief":
                baseStr = 14;
                baseDex = 23;
                baseInt = 23;
                break;
            case "apostle":
                baseStr = 23;
                baseDex = 14;
                baseInt = 23;
                break;
            default:
                // Default fallback (shouldn't happen, but safety)
                baseStr = 14;
                baseDex = 14;
                baseInt = 14;
                break;
        }
        
        // Add level-up gains (level 1 is base, so we gain (level - 1) times)
        var (strGain, dexGain, intGain) = GetLevelUpGains();
        int levelsGained = Mathf.Max(0, level - 1);
        
        strength = baseStr + (strGain * levelsGained);
        dexterity = baseDex + (dexGain * levelsGained);
        intelligence = baseInt + (intGain * levelsGained);
    }

    public void ResetRuntimeState()
    {
        Channeling.Reset();
        // Reset momentum using StackSystem
        if (StackSystem.Instance != null)
        {
            StackSystem.Instance.ClearStacks(StackType.Momentum);
        }
        // Also reset legacy MomentumManager for backward compatibility
        if (momentumManager != null)
        {
            momentumManager.Reset();
        }
        guardPersistenceFraction = Mathf.Clamp01(guardPersistenceFraction);
        momentumGainBonus = 0; // Reset combat-wide momentum gain bonus
    }
    
    // Calculate derived stats based on core attributes
    public void CalculateDerivedStats()
    {
        dodgeChance = 0f;
        guardEffectivenessPercent = 0f;
        buffDurationIncreasedPercent = 0f;
        randomAilmentChancePercent = 0f;
        increasedDamageAfterGuardPercent = 0f;
        
        // Attribute breakpoints
        int str = Mathf.Max(0, strength);
        int dex = Mathf.Max(0, dexterity);
        int intel = Mathf.Max(0, intelligence);

        // Strength → Life and Melee Physical Increased
        // Every 2 STR: +1 Life; Every 10 STR: +5 additional Life
        // Every 5 STR: +1% inc melee phys; Every 10 STR: +2% additional inc melee phys
        int lifeFromStr = (str / 2) + 5 * (str / 10);
        float incMeleeFromStr = 0.01f * (str / 5) + 0.02f * (str / 10);

        // Base life stays 100 unless overridden by content systems
        maxHealth = 100 + lifeFromStr;
        currentHealth = maxHealth; // Start with full health
        increasedMeleePhysicalDamage = incMeleeFromStr;
        
        // Guard calculation (guard cannot exceed max health)
        UpdateMaxGuard();
        
        // Dexterity → Accuracy and Evasion Increased
        // Every 1 DEX: +2 Accuracy; Every 10 DEX: +20 additional Accuracy
        // Every 5 DEX: +1% inc Evasion; Every 10 DEX: +2% additional inc Evasion
        accuracyRating = (dex * 2) + 20 * (dex / 10);
        increasedEvasion = 0.01f * (dex / 5) + 0.02f * (dex / 10);
        baseEvasionRating = 0f; // can be set by gear; character sheets can display final via a helper if needed

        // Intelligence → Increased Spell Damage and % Increased Energy Shield
        // Spell Damage: Same formula as Strength's melee physical damage
        // Every 5 INT: +1% inc spell damage; Every 10 INT: +2% additional inc spell damage
        float incSpellFromInt = 0.01f * (intel / 5) + 0.02f * (intel / 10);
        increasedSpellDamage = incSpellFromInt;
        
        // Add Intelligence's spell damage to damageModifiers list (so it's included in CharacterStatsData)
        // Clear any existing Intelligence-based spell damage first (to avoid duplicates on recalculation)
        damageModifiers.increasedSpellDamage.RemoveAll(x => Mathf.Approximately(x, incSpellFromInt));
        if (incSpellFromInt > 0f)
        {
            damageModifiers.increasedSpellDamage.Add(incSpellFromInt);
        }
        
        // Energy Shield: % Increased (not flat)
        // Every 3 INT: +1% increased Energy Shield
        increasedEnergyShieldPercent = 0.01f * (intel / 3);
        
        // Base Energy Shield stays 0 unless overridden by content systems (equipment, etc.)
        // The % increased will be applied when calculating final ES from equipment/base values
        // For now, we don't set maxEnergyShield here - it should come from equipment or other sources
        // maxEnergyShield will be calculated elsewhere with the increasedEnergyShieldPercent applied
        
        // Attack power (based on Strength and Dexterity)
        attackPower = strength * 2 + dexterity * 1;
        
        // Defense (based on Dexterity and Intelligence)
        defense = dexterity * 1 + intelligence * 1;
        
        // Critical chance (NOT scaled by attributes - only from equipment/buffs)
        // criticalChance = dexterity * 0.5f; // REMOVED - attributes don't affect crit chance
        // Critical chance comes ONLY from equipment and temporary buffs
        
        // Critical multiplier (NOT scaled by attributes - only from equipment/buffs)  
        // criticalMultiplier = 1.5f + (intelligence * 0.02f); // REMOVED - attributes don't affect crit multi
        // Critical multiplier comes ONLY from equipment and temporary buffs
    }
    
    // Get level-up attribute gains based on class
    private (int str, int dex, int intel) GetLevelUpGains()
    {
        switch (characterClass.ToLower())
        {
            // Primary Classes
            case "marauder":
                return (3, 1, 1); // +3 STR, +1 DEX, +1 INT
            case "ranger":
                return (1, 3, 1); // +1 STR, +3 DEX, +1 INT
            case "witch":
                return (1, 1, 3); // +1 STR, +1 DEX, +3 INT
            
            // Hybrid Classes
            case "brawler":
                return (2, 2, 1); // +2 STR, +2 DEX, +1 INT
            case "thief":
                return (1, 2, 2); // +1 STR, +2 DEX, +2 INT
            case "apostle":
                return (2, 1, 2); // +2 STR, +1 DEX, +2 INT
            
            default:
                return (1, 1, 1); // Default fallback
        }
    }
    
    // Level up the character with PoE-style attribute gains
    public void LevelUp()
    {
        level++;
        skillPoints += 1; // +1 skill point per level
        
        // Get attribute gains for this class
        var (strGain, dexGain, intGain) = GetLevelUpGains();
        
        // Apply attribute gains
        strength += strGain;
        dexterity += dexGain;
        intelligence += intGain;
        
        // Recalculate derived stats
        CalculateDerivedStats();
        
        Debug.Log($"{characterName} leveled up to level {level}! " +
                  $"Gained +{strGain} STR, +{dexGain} DEX, +{intGain} INT");
        
        // Auto-save after level up
        if (CharacterManager.Instance != null && CharacterManager.Instance.GetCurrentCharacter() == this)
        {
            CharacterManager.Instance.SaveCharacter();
            Debug.Log($"[Auto-Save] Character saved after leveling up to level {level}.");
        }
    }
    
    // Calculate required experience for next level (PoE-style exponential scaling)
    public int GetRequiredExperience()
    {
        return Mathf.FloorToInt(100 * Mathf.Pow(level, 1.5f));
    }
    
    // Add experience and check for level up
    public void AddExperience(int exp)
    {
        experience += exp;
        // Allow multiple level-ups with carryover
        bool leveled = false;
        while (true)
        {
            int requiredExp = GetRequiredExperience();
            if (experience >= requiredExp)
            {
                experience -= requiredExp; // carryover
                LevelUp();
                leveled = true;
                continue;
            }
            break;
        }
        if (leveled)
        {
            Debug.Log($"{characterName} is now level {level}. Skill Points: {skillPoints}. XP towards next level: {experience}/{GetRequiredExperience()}");
        }
    }
    
    // Get total attributes at a specific level (for display/debugging)
    public (int str, int dex, int intel) GetTotalAttributesAtLevel(int targetLevel)
    {
        if (targetLevel <= 1) return (strength, dexterity, intelligence);
        
        var (strGain, dexGain, intGain) = GetLevelUpGains();
        int levelsGained = targetLevel - 1;
        
        int totalStr = strength - (strGain * (level - 1)) + (strGain * levelsGained);
        int totalDex = dexterity - (dexGain * (level - 1)) + (dexGain * levelsGained);
        int totalInt = intelligence - (intGain * (level - 1)) + (intGain * levelsGained);
        
        return (totalStr, totalDex, totalInt);
    }
    
    // Take damage
    public void TakeDamage(float damage, DamageType damageType = DamageType.Physical)
    {
        // Apply resistances based on damage type
        float resistance = damageStats.GetResistance(damageType);
        float actualDamage;
        
        if (damageType == DamageType.Physical)
        {
            // Physical resistance (armor) reduces damage by flat amount
            actualDamage = Mathf.Max(1, damage - resistance);
        }
        else
        {
            // Elemental resistances reduce damage by percentage
            float reduction = 1f - (resistance / 100f);
            actualDamage = damage * Mathf.Max(0.1f, reduction); // Minimum 10% damage
        }

        if (StackSystem.Instance != null)
        {
            actualDamage *= Mathf.Max(0f, StackSystem.Instance.GetToleranceDamageMultiplier());
        }
        
        // Apply Bolster (less damage taken per stack: 2%, max 10 stacks)
        try
        {
            var playerDisplay = GameObject.FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                var statusManager = playerDisplay.GetStatusEffectManager();
                if (statusManager != null)
                {
                    float bolsterStacks = Mathf.Min(10f, statusManager.GetTotalMagnitude(StatusEffectType.Bolster));
                    if (bolsterStacks > 0f)
                    {
                        float lessMultiplier = Mathf.Clamp01(1f - (0.02f * bolsterStacks));
                        actualDamage *= lessMultiplier;
                    }
                }
            }
        }
        catch { /* safe guard */ }

        // Apply guard first
        if (currentGuard > 0)
        {
            if (currentGuard >= actualDamage)
            {
                currentGuard -= actualDamage;
                actualDamage = 0;
            }
            else
            {
                actualDamage -= currentGuard;
                currentGuard = 0;
            }
        }
        
        // Apply energy shield next (if any damage remains)
        if (actualDamage > 0 && currentEnergyShield > 0)
        {
            if (currentEnergyShield >= actualDamage)
            {
                currentEnergyShield -= Mathf.RoundToInt(actualDamage);
                actualDamage = 0;
            }
            else
            {
                actualDamage -= currentEnergyShield;
                currentEnergyShield = 0;
            }
        }
        
        // Apply remaining damage to health
        currentHealth = Mathf.Max(0, currentHealth - Mathf.RoundToInt(actualDamage));
        
        if (currentHealth <= 0)
        {
            Debug.Log($"{characterName} has been defeated!");
        }
    }
    
    // Heal the character
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.RoundToInt(amount));
    }
    
    // Restore energy shield
    public void RestoreEnergyShield(int amount)
    {
        currentEnergyShield = Mathf.Min(maxEnergyShield, currentEnergyShield + amount);
    }
    
    // Use mana
    public bool UseMana(int amount)
    {
        if (mana >= amount)
        {
            mana -= amount;
            return true;
        }
        return false;
    }
    
    // Restore mana
    public void RestoreMana(int amount)
    {
        mana = Mathf.Min(maxMana, mana + amount);
    }
    
    // Use reliance
    public bool UseReliance(int amount)
    {
        if (reliance >= amount)
        {
            reliance -= amount;
            return true;
        }
        return false;
    }
    
    // Restore reliance
    public void RestoreReliance(int amount)
    {
        reliance = Mathf.Min(maxReliance, reliance + amount);
    }
    
    // Add mana recovery per turn (for items/abilities)
    public void AddManaRecoveryPerTurn(int amount)
    {
        manaRecoveryPerTurn += amount;
        Debug.Log($"{characterName} mana recovery increased by {amount}. Now recovers {manaRecoveryPerTurn} mana per turn.");
    }
    
    // Set mana recovery per turn (for items/abilities)
    public void SetManaRecoveryPerTurn(int amount)
    {
        manaRecoveryPerTurn = amount;
        Debug.Log($"{characterName} mana recovery set to {manaRecoveryPerTurn} per turn.");
    }
    
    // Get mana recovery per turn
    public int GetManaRecoveryPerTurn()
    {
        return manaRecoveryPerTurn;
    }
    
    // Add cards drawn per turn (for passives/equipment)
    public void AddCardsDrawnPerTurn(int amount)
    {
        cardsDrawnPerTurn += amount;
        Debug.Log($"{characterName} cards drawn per turn increased by {amount}. Now draws {cardsDrawnPerTurn} cards per turn.");
    }
    
    // Set cards drawn per turn (for passives/equipment)
    public void SetCardsDrawnPerTurn(int amount)
    {
        cardsDrawnPerTurn = amount;
        Debug.Log($"{characterName} cards drawn per turn set to {cardsDrawnPerTurn} per turn.");
    }
    
    // Get cards drawn per turn
    public int GetCardsDrawnPerTurn()
    {
        return cardsDrawnPerTurn;
    }
    
    // Add cards drawn per wave (for passives/equipment)
    public void AddCardsDrawnPerWave(int amount)
    {
        cardsDrawnPerWave += amount;
        Debug.Log($"{characterName} cards drawn per wave increased by {amount}. Now draws {cardsDrawnPerWave} cards per wave.");
    }
    
    // Set cards drawn per wave (for passives/equipment)
    public void SetCardsDrawnPerWave(int amount)
    {
        cardsDrawnPerWave = amount;
        Debug.Log($"{characterName} cards drawn per wave set to {cardsDrawnPerWave} per wave.");
    }
    
    // Get cards drawn per wave
    public int GetCardsDrawnPerWave()
    {
        return Mathf.Max(1, cardsDrawnPerWave); // Always return at least 1
    }
    
    // Get character info as string
    public string GetCharacterInfo()
    {
        return $"Name: {characterName}\n" +
               $"Class: {characterClass}\n" +
               $"Level: {level}\n" +
               $"Experience: {experience}/{GetRequiredExperience()}\n" +
               $"Health: {currentHealth}/{maxHealth}\n" +
               $"Energy Shield: {currentEnergyShield}/{maxEnergyShield}\n" +
               $"Mana: {mana}/{maxMana} (+{manaRecoveryPerTurn}/turn)\n" +
               $"Cards: +{cardsDrawnPerTurn}/turn\n" +
               $"Reliance: {reliance}/{maxReliance}\n" +
               $"Strength: {strength}\n" +
               $"Dexterity: {dexterity}\n" +
               $"Intelligence: {intelligence}";
    }
    
    // Get detailed level progression info
    public string GetLevelProgressionInfo()
    {
        var (strGain, dexGain, intGain) = GetLevelUpGains();
        var (totalStr, totalDex, totalInt) = GetTotalAttributesAtLevel(level);
        
        return $"Level {level} {characterClass}\n" +
               $"Next Level: {GetRequiredExperience()} XP needed\n" +
               $"Level Gains: +{strGain} STR, +{dexGain} DEX, +{intGain} INT\n" +
               $"Total Attributes: {totalStr} STR, {totalDex} DEX, {totalInt} INT";
    }
    
    // Convert to CharacterData for save system compatibility
    public CharacterData ToCharacterData()
    {
        var data = new CharacterData(characterName, characterClass, level, act);
        
        // Core Stats
        data.experience = experience;
        data.skillPoints = skillPoints;
        data.strength = strength;
        data.dexterity = dexterity;
        data.intelligence = intelligence;
        
        // Resources
        data.currentHealth = currentHealth;
        data.maxHealth = maxHealth;
        data.currentEnergyShield = currentEnergyShield;
        data.maxEnergyShield = maxEnergyShield;
        data.mana = mana;
        data.maxMana = maxMana;
        data.manaRecoveryPerTurn = manaRecoveryPerTurn;
        data.cardsDrawnPerTurn = cardsDrawnPerTurn;
        data.cardsDrawnPerWave = cardsDrawnPerWave;
        data.reliance = reliance;
        data.maxReliance = maxReliance;
        
        // Game State
        data.currentScene = currentScene;
        data.lastPosX = lastPosition.x;
        data.lastPosY = lastPosition.y;
        data.lastPosZ = lastPosition.z;
        
        data.completedEncounterIDs = new List<int>(completedEncounterIDs);
        data.unlockedEncounterIDs = new List<int>(unlockedEncounterIDs);
        data.enteredEncounterIDs = new List<int>(enteredEncounterIDs);
        data.completedQuestIDs = new List<string>(completedQuestIDs);
        data.completedTutorialIDs = new List<string>(completedTutorialIDs);
        
        // Warrants
        data.ownedWarrants = new List<WarrantInstanceData>(ownedWarrants);
        
        // Warrant Board State (NEW) - Save socket assignments and unlocked nodes
        // First try to get from WarrantsManager singleton (persists across scenes)
        var warrantsManager = WarrantsManager.Instance;
        if (warrantsManager != null)
        {
            string stateJson = warrantsManager.GetWarrantBoardStateJson();
            if (!string.IsNullOrEmpty(stateJson))
            {
                data.warrantBoardStateJson = stateJson;
                Debug.Log($"[Character] Saved warrant board state from WarrantsManager ({data.warrantBoardStateJson.Length} chars)");
            }
            else
            {
                // Fallback: Try to get from WarrantBoardStateController if it's in the scene
                var boardState = UnityEngine.Object.FindFirstObjectByType<WarrantBoardStateController>();
                if (boardState != null)
                {
                    data.warrantBoardStateJson = boardState.ToJson(false);
                    // Also sync to WarrantsManager for next time
                    warrantsManager.SetWarrantBoardStateJson(data.warrantBoardStateJson);
                    Debug.Log($"[Character] Saved warrant board state from WarrantBoardStateController ({data.warrantBoardStateJson.Length} chars)");
                }
                else
                {
                    // Final fallback: Try PlayerPrefs
                    string playerPrefsKey = $"WarrantBoardState_{this.characterName}";
                    if (PlayerPrefs.HasKey(playerPrefsKey))
                    {
                        data.warrantBoardStateJson = PlayerPrefs.GetString(playerPrefsKey);
                        Debug.Log($"[Character] Loaded warrant board state from PlayerPrefs fallback ({data.warrantBoardStateJson.Length} chars)");
                    }
                    else if (PlayerPrefs.HasKey("WarrantBoardState"))
                    {
                        data.warrantBoardStateJson = PlayerPrefs.GetString("WarrantBoardState");
                        Debug.Log($"[Character] Loaded warrant board state from legacy PlayerPrefs key ({data.warrantBoardStateJson.Length} chars)");
                    }
                    else
                    {
                        data.warrantBoardStateJson = "";
                        Debug.LogWarning("[Character] No warrant board state found in WarrantsManager, WarrantBoardStateController, or PlayerPrefs");
                    }
                }
            }
        }
        else
        {
            // WarrantsManager not available - try direct access to controller
            var boardState = UnityEngine.Object.FindFirstObjectByType<WarrantBoardStateController>();
            if (boardState != null)
            {
                data.warrantBoardStateJson = boardState.ToJson(false);
                Debug.Log($"[Character] Saved warrant board state from WarrantBoardStateController (WarrantsManager unavailable) ({data.warrantBoardStateJson.Length} chars)");
            }
            else
            {
                data.warrantBoardStateJson = "";
                Debug.LogWarning("[Character] WarrantsManager and WarrantBoardStateController both unavailable - warrant board state not saved");
            }
        }
        
        // Currencies (NEW)
        var lootManager = LootManager.Instance;
        if (lootManager != null)
        {
            var currencies = lootManager.GetAllCurrencies();
            foreach (var kvp in currencies)
            {
                data.currencyTypes.Add(kvp.Key.ToString());
                data.currencyAmounts.Add(kvp.Value);
            }
            Debug.Log($"[Character] Saved {currencies.Count} currency types to CharacterData");
        }
        
        // Inventory (NEW)
        var charManager = CharacterManager.Instance;
        if (charManager != null && charManager.inventoryItems != null)
        {
            foreach (var item in charManager.inventoryItems)
            {
                if (item == null) continue;
                
                SerializedItemData itemData = SerializeItem(item);
                if (itemData != null)
                {
                    data.inventoryItems.Add(itemData);
                }
            }
            Debug.Log($"[Character] Saved {data.inventoryItems.Count} inventory items to CharacterData");
        }
        
        // Aura Experience Data
        data.auraExperienceData = auraExperienceData != null ? new List<AuraExperienceData>(auraExperienceData) : new List<AuraExperienceData>();
        
        // Forge Materials
        data.forgeMaterials = forgeMaterials != null ? new List<ForgeMaterialData>(forgeMaterials) : new List<ForgeMaterialData>();
        
        return data;
    }
    
    /// <summary>
    /// Serialize a BaseItem for saving
    /// </summary>
    private static SerializedItemData SerializeItem(BaseItem item)
    {
        if (item == null) return null;
        
        var data = new SerializedItemData
        {
            itemName = item.itemName,
            rarity = item.rarity
        };
        
        // Determine item type and save type-specific data
        if (item is WeaponItem weapon)
        {
            data.itemType = "WeaponItem";
            data.rolledBaseDamage = weapon.rolledBaseDamage;
            
            // Save affixes with rolled values
            if (weapon.prefixes != null)
            {
                foreach (var affix in weapon.prefixes)
                {
                    data.affixes.Add(SerializeAffix(affix));
                }
            }
            if (weapon.suffixes != null)
            {
                foreach (var affix in weapon.suffixes)
                {
                    data.affixes.Add(SerializeAffix(affix));
                }
            }
            if (weapon.implicitModifiers != null)
            {
                foreach (var affix in weapon.implicitModifiers)
                {
                    data.affixes.Add(SerializeAffix(affix));
                }
            }
        }
        else if (item is Armour armour)
        {
            data.itemType = "Armour";
            
            // Save affixes
            if (armour.prefixes != null)
            {
                foreach (var affix in armour.prefixes)
                {
                    data.affixes.Add(SerializeAffix(affix));
                }
            }
            if (armour.suffixes != null)
            {
                foreach (var affix in armour.suffixes)
                {
                    data.affixes.Add(SerializeAffix(affix));
                }
            }
            if (armour.implicitModifiers != null)
            {
                foreach (var affix in armour.implicitModifiers)
                {
                    data.affixes.Add(SerializeAffix(affix));
                }
            }
        }
        else if (item is Effigy effigy)
        {
            data.itemType = "Effigy";
            // Effigies don't have affixes, just save basic data
        }
        else
        {
            data.itemType = "BaseItem";
        }
        
        return data;
    }
    
    /// <summary>
    /// Serialize an Affix for saving
    /// </summary>
    private static SerializedAffix SerializeAffix(Affix affix)
    {
        if (affix == null) return null;
        
        var data = new SerializedAffix
        {
            affixName = affix.name,
            description = affix.description,
            affixType = affix.affixType,
            isRolled = affix.isRolled,
            rolledValue = affix.rolledValue
        };
        
        // Serialize modifiers
        if (affix.modifiers != null)
        {
            foreach (var modifier in affix.modifiers)
            {
                if (modifier == null) continue;
                
                data.modifiers.Add(new SerializedAffixModifier
                {
                    modifierType = modifier.modifierType,
                    minValue = modifier.minValue,
                    maxValue = modifier.maxValue,
                    isRolled = modifier.isRolled,
                    rolledValue = modifier.rolledValue,
                    isDualRange = modifier.isDualRange,
                    rolledFirstValue = modifier.rolledFirstValue,
                    rolledSecondValue = modifier.rolledSecondValue
                });
            }
        }
        
        return data;
    }
    
    // Create from CharacterData
    public static Character FromCharacterData(CharacterData data)
    {
        Character character = new Character(data.characterName, data.characterClass);
        character.level = data.level;
        character.act = data.act;
        
        // Core Stats
        character.experience = data.experience;
        character.skillPoints = data.skillPoints;
        character.strength = data.strength;
        character.dexterity = data.dexterity;
        character.intelligence = data.intelligence;
        
        // Resources
        character.currentHealth = data.currentHealth;
        character.maxHealth = data.maxHealth;
        character.currentEnergyShield = data.currentEnergyShield;
        character.maxEnergyShield = data.maxEnergyShield;
        character.mana = data.mana;
        character.maxMana = data.maxMana;
        character.manaRecoveryPerTurn = data.manaRecoveryPerTurn;
        character.cardsDrawnPerTurn = data.cardsDrawnPerTurn;
        character.cardsDrawnPerWave = data.cardsDrawnPerWave;
        character.reliance = data.reliance;
        character.maxReliance = data.maxReliance;
        
        // Aura Experience Data
        character.auraExperienceData = data.auraExperienceData != null ? new List<AuraExperienceData>(data.auraExperienceData) : new List<AuraExperienceData>();
        
        // Forge Materials
        character.forgeMaterials = data.forgeMaterials != null ? new List<ForgeMaterialData>(data.forgeMaterials) : new List<ForgeMaterialData>();
        
        character.completedEncounterIDs = data.completedEncounterIDs != null ? new List<int>(data.completedEncounterIDs) : new List<int>();
        character.unlockedEncounterIDs = data.unlockedEncounterIDs != null ? new List<int>(data.unlockedEncounterIDs) : new List<int>();
        character.enteredEncounterIDs = data.enteredEncounterIDs != null ? new List<int>(data.enteredEncounterIDs) : new List<int>();
        character.completedQuestIDs = data.completedQuestIDs != null ? new List<string>(data.completedQuestIDs) : new List<string>();
        character.completedTutorialIDs = data.completedTutorialIDs != null ? new List<string>(data.completedTutorialIDs) : new List<string>();
        
        // Warrants
        character.ownedWarrants = data.ownedWarrants != null ? new List<WarrantInstanceData>(data.ownedWarrants) : new List<WarrantInstanceData>();
        
        // Warrant Board State (NEW) - Load socket assignments and unlocked nodes
        // Note: This will be applied when the warrant board scene is loaded
        // Store the JSON string temporarily so it can be loaded later
        if (!string.IsNullOrEmpty(data.warrantBoardStateJson))
        {
            // First, sync to WarrantsManager (persists across scenes)
            var warrantsManager = WarrantsManager.Instance;
            if (warrantsManager != null)
            {
                warrantsManager.SetWarrantBoardStateJson(data.warrantBoardStateJson);
            }
            
            // Try to load immediately if board state controller exists
            var boardState = UnityEngine.Object.FindFirstObjectByType<WarrantBoardStateController>();
            if (boardState != null)
            {
                boardState.LoadFromJson(data.warrantBoardStateJson);
                Debug.Log($"[Character] Loaded warrant board state from CharacterData into WarrantsManager and WarrantBoardStateController ({data.warrantBoardStateJson.Length} chars)");
            }
            else
            {
                // Board state controller not available yet - state is in WarrantsManager and will be loaded when warrant board scene opens
                Debug.Log($"[Character] Loaded warrant board state from CharacterData into WarrantsManager (WarrantBoardStateController not in scene) ({data.warrantBoardStateJson.Length} chars)");
            }
        }
        
        // Currencies (NEW)
        var lootManager = LootManager.Instance;
        if (lootManager != null && data.currencyTypes != null && data.currencyAmounts != null)
        {
            for (int i = 0; i < data.currencyTypes.Count && i < data.currencyAmounts.Count; i++)
            {
                try
                {
                    CurrencyType type = (CurrencyType)System.Enum.Parse(typeof(CurrencyType), data.currencyTypes[i]);
                    int amount = data.currencyAmounts[i];
                    lootManager.SetCurrency(type, amount);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[Character] Failed to load currency type '{data.currencyTypes[i]}': {e.Message}");
                }
            }
            Debug.Log($"[Character] Loaded {data.currencyTypes.Count} currency types from CharacterData");
        }
        
        // Inventory (NEW)
        var charManager = CharacterManager.Instance;
        if (charManager != null && data.inventoryItems != null)
        {
            charManager.inventoryItems.Clear();
            
            foreach (var itemData in data.inventoryItems)
            {
                BaseItem item = DeserializeItem(itemData);
                if (item != null)
                {
                    charManager.inventoryItems.Add(item);
                }
            }
            Debug.Log($"[Character] Loaded {charManager.inventoryItems.Count} inventory items from CharacterData");
        }
        
        // Recalculate derived stats based on loaded attributes
        character.CalculateDerivedStats();
        
        return character;
    }
    
    /// <summary>
    /// Deserialize a saved item back to BaseItem
    /// NOTE: This creates runtime instances from serialized data
    /// </summary>
    private static BaseItem DeserializeItem(SerializedItemData data)
    {
        if (data == null) return null;
        
        BaseItem item = null;
        
        // Create appropriate item type
        if (data.itemType == "WeaponItem")
        {
            item = ScriptableObject.CreateInstance<WeaponItem>();
            var weapon = item as WeaponItem;
            weapon.rolledBaseDamage = data.rolledBaseDamage;
        }
        else if (data.itemType == "Armour")
        {
            item = ScriptableObject.CreateInstance<Armour>();
        }
        else if (data.itemType == "Effigy")
        {
            item = ScriptableObject.CreateInstance<Effigy>();
        }
        else
        {
            item = ScriptableObject.CreateInstance<BaseItem>();
        }
        
        // Set basic properties
        item.itemName = data.itemName;
        item.rarity = data.rarity;
        
        // Deserialize affixes
        var prefixes = new List<Affix>();
        var suffixes = new List<Affix>();
        var implicits = new List<Affix>();
        
        foreach (var affixData in data.affixes)
        {
            Affix affix = DeserializeAffix(affixData);
            if (affix != null)
            {
                switch (affix.affixType)
                {
                    case AffixType.Prefix:
                        prefixes.Add(affix);
                        break;
                    case AffixType.Suffix:
                        suffixes.Add(affix);
                        break;
                    default:
                        implicits.Add(affix);
                        break;
                }
            }
        }
        
        // Apply affixes to item
        if (item is WeaponItem weapon2)
        {
            weapon2.prefixes = prefixes;
            weapon2.suffixes = suffixes;
            weapon2.implicitModifiers = implicits;
        }
        else if (item is Armour armour)
        {
            armour.prefixes = prefixes;
            armour.suffixes = suffixes;
            armour.implicitModifiers = implicits;
        }
        
        return item;
    }
    
    /// <summary>
    /// Deserialize a saved affix
    /// </summary>
    private static Affix DeserializeAffix(SerializedAffix data)
    {
        if (data == null) return null;
        
        // Create affix with required constructor parameters
        var affix = new Affix(
            data.affixName,
            data.description,
            data.affixType,
            AffixTier.Tier1 // Default tier, may need to be saved/loaded if important
        )
        {
            isRolled = data.isRolled,
            rolledValue = (int)data.rolledValue // Cast float to int
        };
        
        // Deserialize modifiers
        foreach (var modData in data.modifiers)
        {
            // Use 4-parameter constructor: statName, minValue, maxValue, modifierType
            var modifier = new AffixModifier(
                modData.modifierType.ToString(), // statName
                modData.minValue,
                modData.maxValue,
                modData.modifierType
            )
            {
                isRolled = modData.isRolled,
                rolledValue = modData.rolledValue,
                isDualRange = modData.isDualRange,
                rolledFirstValue = modData.rolledFirstValue,
                rolledSecondValue = modData.rolledSecondValue
            };
            
            affix.modifiers.Add(modifier);
        }
        
        return affix;
    }

    public DamageModifiers GetDamageModifiers()
    {
        return damageModifiers;
    }
    
    /// <summary>
    /// Clear all warrant modifiers (both damage and stat modifiers)
    /// </summary>
    private void ClearAllWarrantModifiers()
    {
        // First, restore base attributes by subtracting tracked warrant bonuses
        if (warrantAttributeBonuses != null && warrantAttributeBonuses.Count > 0)
        {
            if (warrantAttributeBonuses.ContainsKey("strength"))
            {
                strength -= warrantAttributeBonuses["strength"];
            }
            if (warrantAttributeBonuses.ContainsKey("dexterity"))
            {
                dexterity -= warrantAttributeBonuses["dexterity"];
            }
            if (warrantAttributeBonuses.ContainsKey("intelligence"))
            {
                intelligence -= warrantAttributeBonuses["intelligence"];
            }
            warrantAttributeBonuses.Clear();
        }
        else
        {
            // If warrantAttributeBonuses is empty (e.g., after loading from save),
            // we need to reset attributes to their base values by recalculating from class
            // This handles the case where saved attributes already include warrant bonuses
            // but the tracking dictionary wasn't saved/loaded
            ResetAttributesToBase();
        }
        
        // Clear damage modifiers
        damageModifiers.increasedPhysicalDamage.Clear();
        damageModifiers.increasedFireDamage.Clear();
        damageModifiers.increasedColdDamage.Clear();
        damageModifiers.increasedLightningDamage.Clear();
        damageModifiers.increasedChaosDamage.Clear();
        damageModifiers.increasedAttackDamage.Clear();
        damageModifiers.increasedSpellDamage.Clear();
        damageModifiers.morePhysicalDamage.Clear();
        damageModifiers.moreFireDamage.Clear();
        damageModifiers.moreColdDamage.Clear();
        damageModifiers.moreLightningDamage.Clear();
        damageModifiers.moreChaosDamage.Clear();
        
        damageModifiers.addedPhysicalDamage = 0f;
        damageModifiers.addedFireDamage = 0f;
        damageModifiers.addedColdDamage = 0f;
        damageModifiers.addedLightningDamage = 0f;
        damageModifiers.addedChaosDamage = 0f;
        
        damageModifiers.criticalStrikeChance = 0f;
        damageModifiers.criticalStrikeMultiplier = 1.5f; // Base multiplier
        
        // Clear warrant stat modifiers
        if (warrantStatModifiers != null)
        {
            warrantStatModifiers.Clear();
        }
        
        // Clear warrant flat modifiers
        if (warrantFlatModifiers != null)
        {
            warrantFlatModifiers.Clear();
        }
    }
    
    /// <summary>
    /// Refresh warrant modifiers from the warrant board
    /// Should be called when warrants are allocated/deallocated or when character is loaded
    /// This clears all modifiers and re-applies both equipment and warrant modifiers
    /// </summary>
    public void RefreshWarrantModifiers()
    {
        // Re-apply equipment modifiers first (these should always be available)
        var equipmentManager = EquipmentManager.Instance;
        if (equipmentManager != null)
        {
            equipmentManager.ApplyEquipmentStats();
        }
        
        // Find WarrantBoardStateController and WarrantLockerGrid
        // Use FindObjectsByType with FindObjectsInactive.Include to find inactive components
        // This ensures we can refresh modifiers even when the locker panel is inactive
        var boardStates = UnityEngine.Object.FindObjectsByType<WarrantBoardStateController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var lockerGrids = UnityEngine.Object.FindObjectsByType<WarrantLockerGrid>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var graphBuilders = UnityEngine.Object.FindObjectsByType<WarrantBoardGraphBuilder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        WarrantBoardStateController boardState = boardStates != null && boardStates.Length > 0 ? boardStates[0] : null;
        WarrantLockerGrid lockerGrid = lockerGrids != null && lockerGrids.Length > 0 ? lockerGrids[0] : null;
        
        // WarrantBoardRuntimeGraph is not a MonoBehaviour, get it from WarrantBoardGraphBuilder
        WarrantBoardRuntimeGraph runtimeGraph = null;
        if (graphBuilders != null && graphBuilders.Length > 0 && graphBuilders[0] != null)
        {
            runtimeGraph = graphBuilders[0].RuntimeGraph;
        }
        
        if (boardState != null && lockerGrid != null && runtimeGraph != null)
        {
            // Only clear warrant modifiers if we can successfully re-apply them
            // This ensures modifiers persist across scenes where warrant board components aren't available
            ClearAllWarrantModifiers();
            
            WarrantModifierCollector.ApplyWarrantModifiersToCharacter(this, boardState, lockerGrid, runtimeGraph);
            Debug.Log($"[Character] ✅ Refreshed warrant modifiers. Total increased Physical Damage entries: {damageModifiers.increasedPhysicalDamage.Count}");
        }
        else
        {
            // If warrant board components aren't available, try lightweight method from saved state
            // This ensures modifiers are applied even when WarrantScene isn't loaded
            ClearAllWarrantModifiers();
            WarrantModifierCollector.ApplyWarrantModifiersFromSavedState(this);
            
            string missing = "";
            if (boardState == null) missing += "WarrantBoardStateController ";
            if (lockerGrid == null) missing += "WarrantLockerGrid ";
            if (runtimeGraph == null) missing += "WarrantBoardRuntimeGraph ";
            Debug.Log($"[Character] Warrant board components not available ({missing.Trim()}). Applied modifiers from saved state instead.");
        }
        
        // Re-apply attribute-based modifiers (Strength's melee physical, Intelligence's spell damage)
        // This ensures attribute bonuses are preserved after clearing modifiers
        CalculateDerivedStats();
    }

    public void ApplyEquipmentModifiers(DamageModifiers equipmentModifiers)
    {
        // Add equipment modifiers to character modifiers
        damageModifiers.addedPhysicalDamage += equipmentModifiers.addedPhysicalDamage;
        damageModifiers.addedFireDamage += equipmentModifiers.addedFireDamage;
        damageModifiers.addedColdDamage += equipmentModifiers.addedColdDamage;
        damageModifiers.addedLightningDamage += equipmentModifiers.addedLightningDamage;
        damageModifiers.addedChaosDamage += equipmentModifiers.addedChaosDamage;
        
        // Add increased damage modifiers
        damageModifiers.increasedPhysicalDamage.AddRange(equipmentModifiers.increasedPhysicalDamage);
        damageModifiers.increasedFireDamage.AddRange(equipmentModifiers.increasedFireDamage);
        damageModifiers.increasedColdDamage.AddRange(equipmentModifiers.increasedColdDamage);
        damageModifiers.increasedLightningDamage.AddRange(equipmentModifiers.increasedLightningDamage);
        damageModifiers.increasedChaosDamage.AddRange(equipmentModifiers.increasedChaosDamage);
        
        // Add more damage modifiers
        damageModifiers.morePhysicalDamage.AddRange(equipmentModifiers.morePhysicalDamage);
        damageModifiers.moreFireDamage.AddRange(equipmentModifiers.moreFireDamage);
        damageModifiers.moreColdDamage.AddRange(equipmentModifiers.moreColdDamage);
        damageModifiers.moreLightningDamage.AddRange(equipmentModifiers.moreLightningDamage);
        damageModifiers.moreChaosDamage.AddRange(equipmentModifiers.moreChaosDamage);
        
        // Update critical strike stats
        damageModifiers.criticalStrikeChance += equipmentModifiers.criticalStrikeChance;
        damageModifiers.criticalStrikeMultiplier += equipmentModifiers.criticalStrikeMultiplier;
    }

    public bool HasWeaponType(WeaponType weaponType)
    {
        return weapons.HasWeaponType(weaponType);
    }

    public float GetWeaponDamage(WeaponType weaponType)
    {
        return weapons.GetWeaponDamage(weaponType);
    }

    // Add this method to assign starter deck
    public void AssignStarterDeck(StarterDeckManager deckManager)
    {
        currentDeck = deckManager.GetStarterDeck(characterClass);
        if (currentDeck == null)
        {
            Debug.LogError($"No starter deck found for class: {characterClass}");
        }
        else
        {
            Debug.Log($"Assigned {currentDeck.deckName} to {characterName}");
        }
    }

    // Add this method to get usable cards
    public List<Card> GetUsableCards()
    {
        return currentDeck?.GetUsableCards(this) ?? new List<Card>();
    }

    // Add this method to get deck statistics
    public DeckStatistics GetDeckStatistics()
    {
        return currentDeck?.GetDeckStatistics() ?? new DeckStatistics();
    }

    #region Speed Helpers

    public float GetAttackSpeedMultiplier()
    {
        float baseSpeed = GetBaseWeaponAttackSpeed();
        float totalPercent = attackSpeedIncreasedPercent;
        if (StackSystem.Instance != null)
        {
            totalPercent += StackSystem.Instance.GetSpeedBonuses().attack;
        }
        float multiplier = baseSpeed * (1f + totalPercent / 100f);
        return Mathf.Clamp(multiplier, 0.05f, 10f);
    }

    public float GetCastSpeedMultiplier()
    {
        float totalPercent = castSpeedIncreasedPercent;
        if (StackSystem.Instance != null)
        {
            totalPercent += StackSystem.Instance.GetSpeedBonuses().cast;
        }
        float multiplier = 1.5f * (1f + totalPercent / 100f);
        return Mathf.Clamp(multiplier, 0.05f, 10f);
    }

    public float GetMovementSpeedMultiplier()
    {
        float totalPercent = movementSpeedIncreasedPercent;
        if (StackSystem.Instance != null)
        {
            totalPercent += StackSystem.Instance.GetSpeedBonuses().move;
        }
        float multiplier = 1f * (1f + totalPercent / 100f);
        return Mathf.Clamp(multiplier, 0.05f, 10f);
    }

    public void SetAttackSpeedIncreasePercent(float percent) => attackSpeedIncreasedPercent = percent;
    public void AddAttackSpeedIncreasePercent(float delta) => attackSpeedIncreasedPercent += delta;
    public void SetCastSpeedIncreasePercent(float percent) => castSpeedIncreasedPercent = percent;
    public void AddCastSpeedIncreasePercent(float delta) => castSpeedIncreasedPercent += delta;
    public void SetMovementSpeedIncreasePercent(float percent) => movementSpeedIncreasedPercent = percent;
    public void AddMovementSpeedIncreasePercent(float delta) => movementSpeedIncreasedPercent += delta;

    #endregion

    private float GetBaseWeaponAttackSpeed()
    {
        float bestSpeed = 0f;
        if (weapons != null)
        {
            if (weapons.meleeWeapon != null && weapons.meleeWeapon.attackSpeed > bestSpeed)
                bestSpeed = weapons.meleeWeapon.attackSpeed;
            if (weapons.projectileWeapon != null && weapons.projectileWeapon.attackSpeed > bestSpeed)
                bestSpeed = weapons.projectileWeapon.attackSpeed;
            if (weapons.spellWeapon != null && weapons.spellWeapon.attackSpeed > bestSpeed)
                bestSpeed = weapons.spellWeapon.attackSpeed;
        }
        return bestSpeed > 0f ? bestSpeed : 1f;
    }
    
    // Add guard to the character (capped at max guard)
    public void AddGuard(float guardAmount)
    {
        // Guard cannot exceed max guard (which is maxHealth * maxGuardMultiplier)
        float effectivenessMultiplier = 1f + Mathf.Max(0f, guardEffectivenessPercent) / 100f;
        float adjustedGuard = guardAmount * effectivenessMultiplier;
        float newGuard = currentGuard + adjustedGuard;
        UpdateMaxGuard(); // Ensure maxGuard is up to date
        currentGuard = Mathf.Min(newGuard, maxGuard);
        
        Debug.Log($"{characterName} gained {adjustedGuard} guard (base {guardAmount}). Total guard: {currentGuard}/{maxGuard}");
        
        // Boss Ability Handler - guard gained
        if (adjustedGuard > 0)
        {
            BossAbilityHandler.OnPlayerGainedGuard(adjustedGuard);
        }
    }
    
    // Set max guard based on max health and maxGuardMultiplier (call this when max health or maxGuardMultiplier changes)
    public void UpdateMaxGuard()
    {
        maxGuard = maxHealth * maxGuardMultiplier;
        // Ensure current guard doesn't exceed new max
        if (currentGuard > maxGuard)
        {
            currentGuard = maxGuard;
        }
    }
    
    // Add reliance to the character
    public void AddReliance(int relianceAmount)
    {
        reliance += relianceAmount;
        Debug.Log($"{characterName} gained {relianceAmount} reliance. Total reliance: {reliance}");
    }

    public void SetGuardPersistenceFraction(float fraction)
    {
        guardPersistenceFraction = Mathf.Clamp01(fraction);
    }

    public void ModifyGuardPersistenceFraction(float delta)
    {
        guardPersistenceFraction = Mathf.Clamp01(guardPersistenceFraction + delta);
    }

    public float GetGuardPersistenceMultiplier()
    {
        return Mathf.Clamp01(guardPersistenceFraction);
    }

    public bool IsEncounterCompleted(int encounterId)
    {
        return completedEncounterIDs.Contains(encounterId);
    }

    public void MarkEncounterCompleted(int encounterId)
    {
        if (!completedEncounterIDs.Contains(encounterId))
        {
            completedEncounterIDs.Add(encounterId);
        }
    }

    public void MarkEncounterUnlocked(int encounterId)
    {
        if (!unlockedEncounterIDs.Contains(encounterId))
        {
            unlockedEncounterIDs.Add(encounterId);
        }
    }

    public bool HasEnteredEncounter(int encounterId)
    {
        return enteredEncounterIDs.Contains(encounterId);
    }

    public void MarkEncounterEntered(int encounterId)
    {
        if (!enteredEncounterIDs.Contains(encounterId))
        {
            enteredEncounterIDs.Add(encounterId);
        }
    }
    
    /// <summary>
    /// Check if a tutorial has been completed
    /// </summary>
    public bool HasCompletedTutorial(string tutorialId)
    {
        return completedTutorialIDs.Contains(tutorialId);
    }
    
    /// <summary>
    /// Mark a tutorial as completed
    /// </summary>
    public void MarkTutorialCompleted(string tutorialId)
    {
        if (!completedTutorialIDs.Contains(tutorialId))
        {
            completedTutorialIDs.Add(tutorialId);
            Debug.Log($"[Character] Character '{characterName}' completed tutorial: {tutorialId}");
        }
    }
    
    #region Stagger System
    
    /// <summary>
    /// Add stagger to the player. Returns true if stagger threshold was reached and player should be stunned.
    /// </summary>
    public bool AddStagger(float amount, float staggerEffectiveness = 1f)
    {
        if (amount <= 0f) return false;
        
        float effectiveStagger = amount * staggerEffectiveness;
        float previousStagger = currentStagger;
        currentStagger += effectiveStagger;
        
        Debug.Log($"[Stagger] {characterName} gained {effectiveStagger:F1} stagger ({previousStagger:F1} -> {currentStagger:F1}/{staggerThreshold:F1})");
        
        // Check if stagger threshold was reached
        if (previousStagger < staggerThreshold && currentStagger >= staggerThreshold)
        {
            Debug.Log($"[Stagger] {characterName} reached stagger threshold! ({currentStagger:F1}/{staggerThreshold:F1})");
            return true; // Signal that stagger threshold was reached
        }
        
        return false;
    }
    
    /// <summary>
    /// Reduce stagger meter (decay over time)
    /// </summary>
    public void DecayStagger()
    {
        if (staggerDecayPerTurn > 0f && currentStagger > 0f)
        {
            float previousStagger = currentStagger;
            currentStagger = Mathf.Max(0f, currentStagger - staggerDecayPerTurn);
            
            if (!Mathf.Approximately(previousStagger, currentStagger))
            {
                Debug.Log($"[Stagger] {characterName} stagger decayed ({previousStagger:F1} -> {currentStagger:F1})");
            }
        }
    }
    
    /// <summary>
    /// Reset stagger meter (called when player is stunned)
    /// </summary>
    public void ResetStagger()
    {
        if (currentStagger > 0f)
        {
            Debug.Log($"[Stagger] {characterName} stagger reset ({currentStagger:F1} -> 0)");
            currentStagger = 0f;
        }
    }
    
    /// <summary>
    /// Get stagger percentage (0-1)
    /// </summary>
    public float GetStaggerPercentage()
    {
        if (staggerThreshold <= 0f) return 0f;
        return Mathf.Clamp01(currentStagger / staggerThreshold);
    }
    
    #endregion
}

[Serializable]
public class ChannelingTracker
{
    [Serializable]
    public struct ChannelingState
    {
        public bool isChanneling;
        public string activeGroupKey;
        public int consecutiveCasts;
        public bool startedThisCast;
        public bool stoppedThisCast;

        public ChannelingState(bool isChanneling, string activeGroupKey, int consecutiveCasts, bool startedThisCast, bool stoppedThisCast)
        {
            this.isChanneling = isChanneling;
            this.activeGroupKey = activeGroupKey ?? string.Empty;
            this.consecutiveCasts = consecutiveCasts;
            this.startedThisCast = startedThisCast;
            this.stoppedThisCast = stoppedThisCast;
        }
    }

    private string activeGroupKey = string.Empty;
    private int consecutiveCastCount = 0;

    public bool IsChanneling => consecutiveCastCount >= 2;
    public string ActiveGroupKey => activeGroupKey;
    public int ConsecutiveCastCount => consecutiveCastCount;

    public ChannelingState RegisterCast(string groupKey)
    {
        string normalizedKey = string.IsNullOrEmpty(groupKey) ? string.Empty : groupKey;
        bool stoppedThisCast = false;

        if (string.IsNullOrEmpty(normalizedKey))
        {
            stoppedThisCast = IsChanneling || consecutiveCastCount > 0;
            Reset();
            return new ChannelingState(false, string.Empty, 0, false, stoppedThisCast);
        }

        if (!string.Equals(normalizedKey, activeGroupKey, StringComparison.Ordinal))
        {
            stoppedThisCast = IsChanneling || consecutiveCastCount > 0;
            activeGroupKey = normalizedKey;
            consecutiveCastCount = 1;
        }
        else
        {
            consecutiveCastCount++;
        }

        bool isChannelingNow = IsChanneling;
        bool startedThisCast = isChannelingNow && consecutiveCastCount == 2;
        bool stoppedFlag = stoppedThisCast && (!isChannelingNow || startedThisCast);

        return new ChannelingState(isChannelingNow, activeGroupKey, consecutiveCastCount, startedThisCast, stoppedFlag);
    }

    public ChannelingState BreakChannel()
    {
        bool stopped = IsChanneling || consecutiveCastCount > 0;
        Reset();
        return new ChannelingState(false, string.Empty, 0, false, stopped);
    }

    public ChannelingState GetState()
    {
        return new ChannelingState(IsChanneling, activeGroupKey, consecutiveCastCount, false, false);
    }

    public void Reset()
    {
        activeGroupKey = string.Empty;
        consecutiveCastCount = 0;
    }
}
