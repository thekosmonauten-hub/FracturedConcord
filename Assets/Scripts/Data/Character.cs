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
    public int reliance = 200;
    public int maxReliance = 200;
    public float currentGuard = 0f; // Current guard amount
    public int maxEnergyShield = 0; // Maximum energy shield
    public int currentEnergyShield = 0; // Current energy shield
    
    [Header("Derived Stats")]
    public int maxHealth;
    public int currentHealth;
    public int attackPower;
    public int defense;
    public float criticalChance;
    public float criticalMultiplier;
    
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
    
    // Add this field to the Character class
    [Header("Deck")]
    public Deck currentDeck;
    
    // Constructor
    public Character(string name, string characterClass)
    {
        this.characterName = name;
        this.characterClass = characterClass;
        this.lastSaveTime = DateTime.Now;
        InitializeStats();
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
        reliance = 200;
        maxReliance = 200;
        
        CalculateDerivedStats();
    }
    
    // Calculate derived stats based on core attributes
    public void CalculateDerivedStats()
    {
        // Health calculation (based on Strength and Vitality)
        maxHealth = strength * 10 + 100; // Base 100 + 10 per strength
        currentHealth = maxHealth; // Start with full health
        
        // Energy Shield calculation (based on Intelligence)
        maxEnergyShield = intelligence * 2; // 2 ES per intelligence point
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
        experience = 0; // Reset experience for next level
        
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
        int requiredExp = GetRequiredExperience();
        
        if (experience >= requiredExp)
        {
            LevelUp();
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
        return new CharacterData(characterName, characterClass, level, act);
    }
    
    // Create from CharacterData
    public static Character FromCharacterData(CharacterData data)
    {
        Character character = new Character(data.characterName, data.characterClass);
        character.level = data.level;
        character.act = data.act;
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
    
    // Add guard to the character
    public void AddGuard(float guardAmount)
    {
        currentGuard += guardAmount;
        Debug.Log($"{characterName} gained {guardAmount} guard. Total guard: {currentGuard}");
    }
    
    // Add reliance to the character
    public void AddReliance(int relianceAmount)
    {
        reliance += relianceAmount;
        Debug.Log($"{characterName} gained {relianceAmount} reliance. Total reliance: {reliance}");
    }
}
