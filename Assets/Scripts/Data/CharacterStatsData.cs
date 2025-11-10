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
    
    [Header("Defense Stats")]
    public int armour;
    public float evasion;
    public int energyShield;
    public float blockChance;
    public float dodgeChance;
    public float spellDodgeChance;
    public float spellBlockChance;
    
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
    
    [Header("Recovery Stats")]
    public float lifeRegeneration;
    public float energyShieldRegeneration;
    public float manaRegeneration;
    public float relianceRegeneration;
    public float lifeLeech;
    public float manaLeech;
    public float energyShieldLeech;
    
    [Header("Combat Mechanics")]
    public float attackSpeed;
    public float castSpeed;
    public float movementSpeed;
    public float attackRange;
    public float projectileSpeed;
    public float areaOfEffect;
    public float skillEffectDuration;
    public float statusEffectDuration;
    
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
        blockChance = 0f;
        dodgeChance = 0f;
        
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
        
        // Initialize other stats
        armour = 0;
        evasion = 0f;
        energyShield = 0;
        blockChance = 0f;
        dodgeChance = 0f;
        spellDodgeChance = 0f;
        spellBlockChance = 0f;
        
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
        
        lifeRegeneration = 0f;
        energyShieldRegeneration = 0f;
        manaRegeneration = 3f;
        relianceRegeneration = 0f;
        lifeLeech = 0f;
        manaLeech = 0f;
        energyShieldLeech = 0f;
        
        attackSpeed = 1f;
        castSpeed = 1f;
        movementSpeed = 1f;
        attackRange = 1f;
        projectileSpeed = 1f;
        areaOfEffect = 1f;
        skillEffectDuration = 1f;
        statusEffectDuration = 1f;
        
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
            
            // Recovery stats
            case "liferegeneration": return lifeRegeneration;
            case "energyshieldregeneration": return energyShieldRegeneration;
            case "manaregeneration": return manaRegeneration;
            case "relianceregeneration": return relianceRegeneration;
            case "lifeleech": return lifeLeech;
            case "manaleech": return manaLeech;
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
            
            // Card system stats
            case "cardsdrawnperturn": return cardsDrawnPerTurn;
            case "maxhandsize": return maxHandSize;
            case "discardpilesize": return discardPileSize;
            case "exhaustpilesize": return exhaustPileSize;
            case "carddrawchance": return cardDrawChance;
            case "cardretentionchance": return cardRetentionChance;
            case "cardupgradechance": return cardUpgradeChance;
            
            // Passive tree stats removed - passive tree system deleted
            
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
            
            case "liferegeneration": lifeRegeneration = value; break;
            case "energyshieldregeneration": energyShieldRegeneration = value; break;
            case "manaregeneration": manaRegeneration = value; break;
            case "relianceregeneration": relianceRegeneration = value; break;
            case "lifeleech": lifeLeech = value; break;
            case "manaleech": manaLeech = value; break;
            case "energyshieldleech": energyShieldLeech = value; break;
            case "attackspeed": attackSpeed = value; break;
            case "castspeed": castSpeed = value; break;
            case "movementspeed": movementSpeed = value; break;
            case "attackrange": attackRange = value; break;
            case "projectilespeed": projectileSpeed = value; break;
            case "areaofeffect": areaOfEffect = value; break;
            case "skilleffectduration": skillEffectDuration = value; break;
            case "statuseffectduration": statusEffectDuration = value; break;
            case "cardsdrawnperturn": cardsDrawnPerTurn = Mathf.RoundToInt(value); break;
            case "maxhandsize": maxHandSize = Mathf.RoundToInt(value); break;
            case "discardpilesize": discardPileSize = Mathf.RoundToInt(value); break;
            case "exhaustpilesize": exhaustPileSize = Mathf.RoundToInt(value); break;
            case "carddrawchance": cardDrawChance = value; break;
            case "cardretentionchance": cardRetentionChance = value; break;
            case "cardupgradechance": cardUpgradeChance = value; break;
            // Passive tree points removed - passive tree system deleted
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
        clone.lifeLeech = this.lifeLeech;
        clone.manaLeech = this.manaLeech;
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
