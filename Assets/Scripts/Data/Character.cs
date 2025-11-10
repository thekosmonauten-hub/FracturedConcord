using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public int mana = 3;
    public int maxMana = 3;
    public int manaRecoveryPerTurn = 3; // Base mana recovery per turn
    public int cardsDrawnPerTurn = 1; // Base cards drawn per turn
    public int cardsDrawnPerWave = 1; // Base cards drawn when a new wave starts (in addition to turn draw)
    public int reliance = 200;
    public int maxReliance = 200;
    public float currentGuard = 0f; // Current guard amount
    public float maxGuard = 0f; // Maximum guard (based on max health)
    public int maxEnergyShield = 0; // Maximum energy shield
    public int currentEnergyShield = 0; // Current energy shield
    
    [Header("Derived Stats")]
    public int maxHealth;
    public int currentHealth;
    public int attackPower;
    public int defense;
    public float criticalChance;
    public float criticalMultiplier;
    
    // Derived secondary stats from attributes
    public float increasedMeleePhysicalDamage = 0f; // Additive increased (e.g., 0.15 = +15%)
    public float accuracyRating = 0f;
    public float increasedEvasion = 0f; // Additive increased (e.g., 0.10 = +10%)
    public float baseEvasionRating = 0f; // baseline before increased modifiers
    
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
    
    [Header("Progression")]
    public List<string> unlockedAbilities = new List<string>();
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    
    [Header("Weapons")]
    public CharacterWeapons weapons = new CharacterWeapons();
    
    [Header("Deck & Cards")]
    public Deck currentDeck; // Legacy - for backward compatibility
    public CharacterDeckData deckData = new CharacterDeckData(); // New: deck management & card collection
    
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
        switch (characterClass.ToLower())
        {
            // Primary Classes (Single Attribute Focus)
            case "marauder":
                strength = 32;
                dexterity = 14;
                intelligence = 14;
                break;
            case "ranger":
                strength = 14;
                dexterity = 32;
                intelligence = 14;
                break;
            case "witch":
                strength = 14;
                dexterity = 14;
                intelligence = 32;
                break;
            
            // Hybrid Classes (Dual Attribute Focus)
            case "brawler":
                strength = 23;
                dexterity = 23;
                intelligence = 14;
                break;
            case "thief":
                strength = 14;
                dexterity = 23;
                intelligence = 23;
                break;
            case "apostle":
                strength = 23;
                dexterity = 14;
                intelligence = 23;
                break;
        }
        
        // All classes start with 3 Mana and 200 Reliance
        mana = 3;
        maxMana = 3;
        manaRecoveryPerTurn = 3; // Base mana recovery per turn
        cardsDrawnPerTurn = 1; // Base cards drawn per turn
        cardsDrawnPerWave = 1; // Base cards drawn per wave
        reliance = 200;
        maxReliance = 200;
        
        CalculateDerivedStats();
    }

    public void ResetRuntimeState()
    {
        Channeling.Reset();
    }
    
    // Calculate derived stats based on core attributes
    public void CalculateDerivedStats()
    {
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

        // Intelligence → Energy Shield and Increased Max ES
        // Every 2 INT: +1 ES; Every 10 INT: +5 additional ES
        // Every 5 INT: +1% inc max ES; Every 10 INT: +2% additional inc max ES
        int esFromInt = (intel / 2) + 5 * (intel / 10);
        float incESPct = 0.01f * (intel / 5) + 0.02f * (intel / 10);
        maxEnergyShield = Mathf.FloorToInt(esFromInt * (1f + incESPct));
        currentEnergyShield = maxEnergyShield; // Start with full energy shield
        
        // Attack power (based on Strength and Dexterity)
        attackPower = strength * 2 + dexterity * 1;
        
        // Defense (based on Dexterity and Intelligence)
        defense = dexterity * 1 + intelligence * 1;
        
        // Critical chance (based on Dexterity)
        criticalChance = dexterity * 0.5f; // 0.5% per dexterity point
        
        // Critical multiplier (based on Intelligence)
        criticalMultiplier = 1.5f + (intelligence * 0.02f); // Base 1.5x + 0.02x per int point
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
        
        // Game State
        character.currentScene = data.currentScene;
        character.lastPosition = new Vector3(data.lastPosX, data.lastPosY, data.lastPosZ);
        
        // Recalculate derived stats based on loaded attributes
        character.CalculateDerivedStats();
        
        return character;
    }

    public DamageModifiers GetDamageModifiers()
    {
        return damageModifiers;
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
    
    // Add guard to the character (capped at max health)
    public void AddGuard(float guardAmount)
    {
        // Guard cannot exceed max health
        float newGuard = currentGuard + guardAmount;
        currentGuard = Mathf.Min(newGuard, maxHealth);
        
        Debug.Log($"{characterName} gained {guardAmount} guard. Total guard: {currentGuard}/{maxHealth}");
    }
    
    // Set max guard based on max health (call this when max health changes)
    public void UpdateMaxGuard()
    {
        maxGuard = maxHealth;
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
