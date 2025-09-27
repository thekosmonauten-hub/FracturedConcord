using UnityEngine;
using PassiveTree;

/// <summary>
/// Component that holds JSON data for a specific cell
/// This allows each cell to have its own JSON data directly attached
/// </summary>
public class CellJsonData : MonoBehaviour
{
    [Header("JSON Data Source")]
    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private string boardId = "core_board"; // For future extension boards
    
    [Header("JSON Data")]
    [SerializeField] private string nodeId;
    [SerializeField] private string nodeName;
    [SerializeField] private string nodeDescription;
    [SerializeField] private string nodeType;
    [SerializeField] private Vector2Int nodePosition;
    [SerializeField] private int nodeCost;
    [SerializeField] private int maxRank;
    [SerializeField] private int currentRank;
    
    [Header("Stats")]
    [SerializeField] private JsonStats nodeStats;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Properties for easy access
    public string NodeId => nodeId;
    public string NodeName => nodeName;
    public string NodeDescription => nodeDescription;
    public string NodeType => nodeType;
    public Vector2Int NodePosition => nodePosition;
    public int NodeCost => nodeCost;
    public int MaxRank => maxRank;
    public int CurrentRank => currentRank;
    public JsonStats NodeStats => nodeStats;

    /// <summary>
    /// Set the JSON data for this cell
    /// </summary>
    public void SetJsonData(JsonNodeData jsonData)
    {
        if (jsonData == null)
        {
            Debug.LogWarning($"[CellJsonData] SetJsonData called with null data on {gameObject.name}");
            return;
        }

        nodeId = jsonData.id;
        nodeName = jsonData.name;
        nodeDescription = jsonData.description;
        nodeType = jsonData.type;
        nodePosition = new Vector2Int(jsonData.position.row, jsonData.position.column);
        nodeCost = jsonData.cost;
        maxRank = jsonData.maxRank;
        currentRank = jsonData.currentRank;
        nodeStats = jsonData.stats;

        if (showDebugInfo)
        {
            Debug.Log($"[CellJsonData] Set JSON data for {gameObject.name}:");
            Debug.Log($"  - ID: '{nodeId}'");
            Debug.Log($"  - Name: '{nodeName}'");
            Debug.Log($"  - Description: '{nodeDescription}'");
            Debug.Log($"  - Type: '{nodeType}'");
            Debug.Log($"  - Position: {nodePosition}");
            Debug.Log($"  - Cost: {nodeCost}");
            Debug.Log($"  - Max Rank: {maxRank}");
            Debug.Log($"  - Current Rank: {currentRank}");
            Debug.Log($"  - Stats: {(nodeStats != null ? "Available" : "None")}");
        }
        
        // Update the cell's sprite based on the new JSON data
        var cellController = GetComponent<CellController>();
        if (cellController != null)
        {
            cellController.UpdateSpriteForJsonData();
        }
    }
    
    /// <summary>
    /// Set the JSON file for this cell
    /// </summary>
    public void SetJsonFile(TextAsset jsonFile)
    {
        this.jsonFile = jsonFile;
        if (showDebugInfo)
        {
            Debug.Log($"[CellJsonData] Set JSON file for {gameObject.name}: {jsonFile?.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// Set the board ID for this cell
    /// </summary>
    public void SetBoardId(string boardId)
    {
        this.boardId = boardId;
        if (showDebugInfo)
        {
            Debug.Log($"[CellJsonData] Set board ID for {gameObject.name}: {boardId}");
        }
    }

    /// <summary>
    /// Get the JSON data as a JsonNodeData object
    /// </summary>
    public JsonNodeData GetJsonNodeData()
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            return null;
        }

        return new JsonNodeData
        {
            id = nodeId,
            name = nodeName,
            description = nodeDescription,
            type = nodeType,
            position = new JsonPosition { row = nodePosition.x, column = nodePosition.y },
            cost = nodeCost,
            maxRank = maxRank,
            currentRank = currentRank,
            stats = nodeStats
        };
    }

    /// <summary>
    /// Check if this cell has JSON data
    /// </summary>
    public bool HasJsonData()
    {
        return !string.IsNullOrEmpty(nodeId) && !string.IsNullOrEmpty(nodeName);
    }

    /// <summary>
    /// Get a formatted description for tooltips
    /// </summary>
    public string GetFormattedDescription()
    {
        if (!HasJsonData())
        {
            return "No data available";
        }

        string description = nodeDescription;
        
        if (nodeStats != null)
        {
            // Add stats to description with categories
            var statsList = new System.Collections.Generic.List<string>();
            
            // Core Attributes
            var coreAttributes = new System.Collections.Generic.List<string>();
            if (nodeStats.strength != 0) coreAttributes.Add($"+{nodeStats.strength} Strength");
            if (nodeStats.dexterity != 0) coreAttributes.Add($"+{nodeStats.dexterity} Dexterity");
            if (nodeStats.intelligence != 0) coreAttributes.Add($"+{nodeStats.intelligence} Intelligence");
            if (coreAttributes.Count > 0)
            {
                statsList.Add("Core Attributes:");
                statsList.AddRange(coreAttributes);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Combat Resources (Only Max values - Current values are runtime only)
            var combatResources = new System.Collections.Generic.List<string>();
            if (nodeStats.maxHealthIncrease != 0) combatResources.Add($"+{nodeStats.maxHealthIncrease} Max Health");
            if (nodeStats.maxEnergyShieldIncrease != 0) combatResources.Add($"+{nodeStats.maxEnergyShieldIncrease} Max Energy Shield");
            if (nodeStats.maxMana != 0) combatResources.Add($"+{nodeStats.maxMana} Max Mana");
            if (nodeStats.maxReliance != 0) combatResources.Add($"+{nodeStats.maxReliance} Max Reliance");
            if (combatResources.Count > 0)
            {
                statsList.Add("Combat Resources:");
                statsList.AddRange(combatResources);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Combat Stats
            var combatStats = new System.Collections.Generic.List<string>();
            if (nodeStats.attackPower != 0) combatStats.Add($"+{nodeStats.attackPower} Attack Power");
            if (nodeStats.defense != 0) combatStats.Add($"+{nodeStats.defense} Defense");
            if (nodeStats.criticalChance != 0) combatStats.Add($"+{nodeStats.criticalChance}% Critical Chance");
            if (nodeStats.criticalMultiplier != 0) combatStats.Add($"+{nodeStats.criticalMultiplier}% Critical Multiplier");
            if (nodeStats.accuracy != 0) combatStats.Add($"+{nodeStats.accuracy} Accuracy");
            if (combatStats.Count > 0)
            {
                statsList.Add("Combat Stats:");
                statsList.AddRange(combatStats);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Damage Modifiers (Increased)
            var damageModifiers = new System.Collections.Generic.List<string>();
            if (nodeStats.increasedPhysicalDamage != 0) damageModifiers.Add($"+{nodeStats.increasedPhysicalDamage}% Physical Damage");
            if (nodeStats.increasedFireDamage != 0) damageModifiers.Add($"+{nodeStats.increasedFireDamage}% Fire Damage");
            if (nodeStats.increasedColdDamage != 0) damageModifiers.Add($"+{nodeStats.increasedColdDamage}% Cold Damage");
            if (nodeStats.increasedLightningDamage != 0) damageModifiers.Add($"+{nodeStats.increasedLightningDamage}% Lightning Damage");
            if (nodeStats.increasedChaosDamage != 0) damageModifiers.Add($"+{nodeStats.increasedChaosDamage}% Chaos Damage");
            if (nodeStats.increasedElementalDamage != 0) damageModifiers.Add($"+{nodeStats.increasedElementalDamage}% Elemental Damage");
            if (nodeStats.increasedSpellDamage != 0) damageModifiers.Add($"+{nodeStats.increasedSpellDamage}% Spell Damage");
            if (nodeStats.increasedAttackDamage != 0) damageModifiers.Add($"+{nodeStats.increasedAttackDamage}% Attack Damage");
            if (nodeStats.increasedProjectileDamage != 0) damageModifiers.Add($"+{nodeStats.increasedProjectileDamage}% Projectile Damage");
            if (nodeStats.increasedAreaDamage != 0) damageModifiers.Add($"+{nodeStats.increasedAreaDamage}% Area Damage");
            if (nodeStats.increasedMeleeDamage != 0) damageModifiers.Add($"+{nodeStats.increasedMeleeDamage}% Melee Damage");
            if (nodeStats.increasedRangedDamage != 0) damageModifiers.Add($"+{nodeStats.increasedRangedDamage}% Ranged Damage");
            if (damageModifiers.Count > 0)
            {
                statsList.Add("Damage Modifiers:");
                statsList.AddRange(damageModifiers);
                statsList.Add(""); // Empty line for spacing
            }
            
            // More Damage Multipliers
            var moreDamageMultipliers = new System.Collections.Generic.List<string>();
            if (nodeStats.morePhysicalDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.morePhysicalDamage:F1}x Physical Damage");
            if (nodeStats.moreFireDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreFireDamage:F1}x Fire Damage");
            if (nodeStats.moreColdDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreColdDamage:F1}x Cold Damage");
            if (nodeStats.moreLightningDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreLightningDamage:F1}x Lightning Damage");
            if (nodeStats.moreChaosDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreChaosDamage:F1}x Chaos Damage");
            if (nodeStats.moreElementalDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreElementalDamage:F1}x Elemental Damage");
            if (nodeStats.moreSpellDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreSpellDamage:F1}x Spell Damage");
            if (nodeStats.moreAttackDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreAttackDamage:F1}x Attack Damage");
            if (nodeStats.moreProjectileDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreProjectileDamage:F1}x Projectile Damage");
            if (nodeStats.moreAreaDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreAreaDamage:F1}x Area Damage");
            if (nodeStats.moreMeleeDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreMeleeDamage:F1}x Melee Damage");
            if (nodeStats.moreRangedDamage != 1f) moreDamageMultipliers.Add($"{nodeStats.moreRangedDamage:F1}x Ranged Damage");
            if (moreDamageMultipliers.Count > 0)
            {
                statsList.Add("More Damage Multipliers:");
                statsList.AddRange(moreDamageMultipliers);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Added Damage
            var addedDamage = new System.Collections.Generic.List<string>();
            if (nodeStats.addedPhysicalDamage != 0) addedDamage.Add($"+{nodeStats.addedPhysicalDamage} Physical Damage");
            if (nodeStats.addedFireDamage != 0) addedDamage.Add($"+{nodeStats.addedFireDamage} Fire Damage");
            if (nodeStats.addedColdDamage != 0) addedDamage.Add($"+{nodeStats.addedColdDamage} Cold Damage");
            if (nodeStats.addedLightningDamage != 0) addedDamage.Add($"+{nodeStats.addedLightningDamage} Lightning Damage");
            if (nodeStats.addedChaosDamage != 0) addedDamage.Add($"+{nodeStats.addedChaosDamage} Chaos Damage");
            if (nodeStats.addedElementalDamage != 0) addedDamage.Add($"+{nodeStats.addedElementalDamage} Elemental Damage");
            if (nodeStats.addedSpellDamage != 0) addedDamage.Add($"+{nodeStats.addedSpellDamage} Spell Damage");
            if (nodeStats.addedAttackDamage != 0) addedDamage.Add($"+{nodeStats.addedAttackDamage} Attack Damage");
            if (nodeStats.addedProjectileDamage != 0) addedDamage.Add($"+{nodeStats.addedProjectileDamage} Projectile Damage");
            if (nodeStats.addedAreaDamage != 0) addedDamage.Add($"+{nodeStats.addedAreaDamage} Area Damage");
            if (nodeStats.addedMeleeDamage != 0) addedDamage.Add($"+{nodeStats.addedMeleeDamage} Melee Damage");
            if (nodeStats.addedRangedDamage != 0) addedDamage.Add($"+{nodeStats.addedRangedDamage} Ranged Damage");
            if (addedDamage.Count > 0)
            {
                statsList.Add("Added Damage:");
                statsList.AddRange(addedDamage);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Resistances
            var resistances = new System.Collections.Generic.List<string>();
            if (nodeStats.physicalResistance != 0) resistances.Add($"+{nodeStats.physicalResistance}% Physical Resistance");
            if (nodeStats.fireResistance != 0) resistances.Add($"+{nodeStats.fireResistance}% Fire Resistance");
            if (nodeStats.coldResistance != 0) resistances.Add($"+{nodeStats.coldResistance}% Cold Resistance");
            if (nodeStats.lightningResistance != 0) resistances.Add($"+{nodeStats.lightningResistance}% Lightning Resistance");
            if (nodeStats.chaosResistance != 0) resistances.Add($"+{nodeStats.chaosResistance}% Chaos Resistance");
            if (nodeStats.elementalResistance != 0) resistances.Add($"+{nodeStats.elementalResistance}% Elemental Resistance");
            if (nodeStats.allResistance != 0) resistances.Add($"+{nodeStats.allResistance}% All Resistance");
            if (resistances.Count > 0)
            {
                statsList.Add("Resistances:");
                statsList.AddRange(resistances);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Defense Stats
            var defenseStats = new System.Collections.Generic.List<string>();
            if (nodeStats.armour != 0) defenseStats.Add($"+{nodeStats.armour} Armour");
            if (nodeStats.evasion != 0) defenseStats.Add($"+{nodeStats.evasion} Evasion");
            if (nodeStats.energyShield != 0) defenseStats.Add($"+{nodeStats.energyShield} Energy Shield");
            if (nodeStats.blockChance != 0) defenseStats.Add($"+{nodeStats.blockChance}% Block Chance");
            if (nodeStats.dodgeChance != 0) defenseStats.Add($"+{nodeStats.dodgeChance}% Dodge Chance");
            if (nodeStats.spellDodgeChance != 0) defenseStats.Add($"+{nodeStats.spellDodgeChance}% Spell Dodge Chance");
            if (nodeStats.spellBlockChance != 0) defenseStats.Add($"+{nodeStats.spellBlockChance}% Spell Block Chance");
            if (defenseStats.Count > 0)
            {
                statsList.Add("Defense Stats:");
                statsList.AddRange(defenseStats);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Ailments
            var ailmentStats = new System.Collections.Generic.List<string>();
            // Non-damaging ailments chance
            if (nodeStats.chanceToShock != 0) ailmentStats.Add($"+{nodeStats.chanceToShock}% Chance to Shock");
            if (nodeStats.chanceToChill != 0) ailmentStats.Add($"+{nodeStats.chanceToChill}% Chance to Chill");
            if (nodeStats.chanceToFreeze != 0) ailmentStats.Add($"+{nodeStats.chanceToFreeze}% Chance to Freeze");
            
            // Damaging ailments chance
            if (nodeStats.chanceToIgnite != 0) ailmentStats.Add($"+{nodeStats.chanceToIgnite}% Chance to Ignite");
            if (nodeStats.chanceToBleed != 0) ailmentStats.Add($"+{nodeStats.chanceToBleed}% Chance to Bleed");
            if (nodeStats.chanceToPoison != 0) ailmentStats.Add($"+{nodeStats.chanceToPoison}% Chance to Poison");
            
            // Increased ailment magnitude/effect
            if (nodeStats.increasedIgniteMagnitude != 0) ailmentStats.Add($"+{nodeStats.increasedIgniteMagnitude}% Ignite Magnitude");
            if (nodeStats.increasedShockMagnitude != 0) ailmentStats.Add($"+{nodeStats.increasedShockMagnitude}% Shock Magnitude");
            if (nodeStats.increasedChillMagnitude != 0) ailmentStats.Add($"+{nodeStats.increasedChillMagnitude}% Chill Magnitude");
            if (nodeStats.increasedFreezeMagnitude != 0) ailmentStats.Add($"+{nodeStats.increasedFreezeMagnitude}% Freeze Magnitude");
            if (nodeStats.increasedBleedMagnitude != 0) ailmentStats.Add($"+{nodeStats.increasedBleedMagnitude}% Bleed Magnitude");
            if (nodeStats.increasedPoisonMagnitude != 0) ailmentStats.Add($"+{nodeStats.increasedPoisonMagnitude}% Poison Magnitude");
            if (ailmentStats.Count > 0)
            {
                statsList.Add("Ailments:");
                statsList.AddRange(ailmentStats);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Recovery Stats
            var recoveryStats = new System.Collections.Generic.List<string>();
            if (nodeStats.lifeRegeneration != 0) recoveryStats.Add($"+{nodeStats.lifeRegeneration} Life Regeneration");
            if (nodeStats.energyShieldRegeneration != 0) recoveryStats.Add($"+{nodeStats.energyShieldRegeneration} Energy Shield Regeneration");
            if (nodeStats.manaRegeneration != 0) recoveryStats.Add($"+{nodeStats.manaRegeneration} Mana Regeneration");
            if (nodeStats.relianceRegeneration != 0) recoveryStats.Add($"+{nodeStats.relianceRegeneration} Reliance Regeneration");
            if (nodeStats.lifeLeech != 0) recoveryStats.Add($"+{nodeStats.lifeLeech}% Life Leech");
            if (nodeStats.manaLeech != 0) recoveryStats.Add($"+{nodeStats.manaLeech}% Mana Leech");
            if (nodeStats.energyShieldLeech != 0) recoveryStats.Add($"+{nodeStats.energyShieldLeech}% Energy Shield Leech");
            if (recoveryStats.Count > 0)
            {
                statsList.Add("Recovery Stats:");
                statsList.AddRange(recoveryStats);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Combat Mechanics
            var combatMechanics = new System.Collections.Generic.List<string>();
            if (nodeStats.attackSpeed != 0) combatMechanics.Add($"+{nodeStats.attackSpeed}% Attack Speed");
            if (nodeStats.castSpeed != 0) combatMechanics.Add($"+{nodeStats.castSpeed}% Cast Speed");
            if (nodeStats.movementSpeed != 0) combatMechanics.Add($"+{nodeStats.movementSpeed}% Movement Speed");
            if (nodeStats.attackRange != 0) combatMechanics.Add($"+{nodeStats.attackRange}% Attack Range");
            if (nodeStats.projectileSpeed != 0) combatMechanics.Add($"+{nodeStats.projectileSpeed}% Projectile Speed");
            if (nodeStats.areaOfEffect != 0) combatMechanics.Add($"+{nodeStats.areaOfEffect}% Area of Effect");
            if (nodeStats.skillEffectDuration != 0) combatMechanics.Add($"+{nodeStats.skillEffectDuration}% Skill Effect Duration");
            if (nodeStats.statusEffectDuration != 0) combatMechanics.Add($"+{nodeStats.statusEffectDuration}% Status Effect Duration");
            if (combatMechanics.Count > 0)
            {
                statsList.Add("Combat Mechanics:");
                statsList.AddRange(combatMechanics);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Card System Stats
            var cardSystemStats = new System.Collections.Generic.List<string>();
            if (nodeStats.cardsDrawnPerTurn != 0) cardSystemStats.Add($"+{nodeStats.cardsDrawnPerTurn} Cards Drawn Per Turn");
            if (nodeStats.maxHandSize != 0) cardSystemStats.Add($"+{nodeStats.maxHandSize} Max Hand Size");
            if (nodeStats.cardDrawChance != 0) cardSystemStats.Add($"+{nodeStats.cardDrawChance}% Card Draw Chance");
            if (nodeStats.cardRetentionChance != 0) cardSystemStats.Add($"+{nodeStats.cardRetentionChance}% Card Retention Chance");
            if (nodeStats.cardUpgradeChance != 0) cardSystemStats.Add($"+{nodeStats.cardUpgradeChance}% Card Upgrade Chance");
            if (nodeStats.discardPower != 0) cardSystemStats.Add($"+{nodeStats.discardPower} Discard Power");
            if (nodeStats.manaPerTurn != 0) cardSystemStats.Add($"+{nodeStats.manaPerTurn} Mana Per Turn");
            if (cardSystemStats.Count > 0)
            {
                statsList.Add("Card System Stats:");
                statsList.AddRange(cardSystemStats);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Legacy/Backward Compatibility Fields (for existing JSON files)
            var legacyStats = new System.Collections.Generic.List<string>();
            if (nodeStats.armorIncrease != 0) legacyStats.Add($"+{nodeStats.armorIncrease} Armor");
            if (nodeStats.increasedEvasion != 0) legacyStats.Add($"+{nodeStats.increasedEvasion}% Evasion");
            if (nodeStats.elementalResist != 0) legacyStats.Add($"+{nodeStats.elementalResist}% Elemental Resistance");
            if (nodeStats.spellPowerIncrease != 0) legacyStats.Add($"+{nodeStats.spellPowerIncrease} Spell Power");
            if (nodeStats.critChanceIncrease != 0) legacyStats.Add($"+{nodeStats.critChanceIncrease}% Critical Strike Chance");
            if (nodeStats.critMultiplierIncrease != 0) legacyStats.Add($"+{nodeStats.critMultiplierIncrease}% Critical Strike Multiplier");
            if (legacyStats.Count > 0)
            {
                statsList.Add("Legacy Stats:");
                statsList.AddRange(legacyStats);
                statsList.Add(""); // Empty line for spacing
            }
            
            // Elemental-specific stats (Legacy naming for backward compatibility)
            var elementalLegacyStats = new System.Collections.Generic.List<string>();
            if (nodeStats.fireIncrease != 0) elementalLegacyStats.Add($"+{nodeStats.fireIncrease}% Fire Damage");
            if (nodeStats.fire != 0) elementalLegacyStats.Add($"+{nodeStats.fire} Fire Damage");
            if (nodeStats.chanceToIgnite != 0) elementalLegacyStats.Add($"+{nodeStats.chanceToIgnite}% Chance to Ignite");
            if (nodeStats.addedPhysicalAsFire != 0) elementalLegacyStats.Add($"+{nodeStats.addedPhysicalAsFire}% Physical Damage as Fire");
            if (nodeStats.increasedIgniteMagnitude != 0) elementalLegacyStats.Add($"+{nodeStats.increasedIgniteMagnitude}% Ignite Magnitude");
            if (nodeStats.addedFireAsCold != 0) elementalLegacyStats.Add($"+{nodeStats.addedFireAsCold}% Fire Damage as Cold");
            
            if (nodeStats.coldIncrease != 0) elementalLegacyStats.Add($"+{nodeStats.coldIncrease}% Cold Damage");
            if (nodeStats.cold != 0) elementalLegacyStats.Add($"+{nodeStats.cold} Cold Damage");
            if (nodeStats.chanceToFreeze != 0) elementalLegacyStats.Add($"+{nodeStats.chanceToFreeze}% Chance to Freeze");
            if (nodeStats.addedPhysicalAsCold != 0) elementalLegacyStats.Add($"+{nodeStats.addedPhysicalAsCold}% Physical Damage as Cold");
            if (nodeStats.increasedFreezeMagnitude != 0) elementalLegacyStats.Add($"+{nodeStats.increasedFreezeMagnitude}% Freeze Magnitude");
            if (nodeStats.addedColdAsFire != 0) elementalLegacyStats.Add($"+{nodeStats.addedColdAsFire}% Cold Damage as Fire");
            
            if (nodeStats.lightningIncrease != 0) elementalLegacyStats.Add($"+{nodeStats.lightningIncrease}% Lightning Damage");
            if (nodeStats.lightning != 0) elementalLegacyStats.Add($"+{nodeStats.lightning} Lightning Damage");
            if (nodeStats.chanceToShock != 0) elementalLegacyStats.Add($"+{nodeStats.chanceToShock}% Chance to Shock");
            if (nodeStats.addedPhysicalAsLightning != 0) elementalLegacyStats.Add($"+{nodeStats.addedPhysicalAsLightning}% Physical Damage as Lightning");
            if (nodeStats.increasedShockMagnitude != 0) elementalLegacyStats.Add($"+{nodeStats.increasedShockMagnitude}% Shock Magnitude");
            if (nodeStats.addedLightningAsFire != 0) elementalLegacyStats.Add($"+{nodeStats.addedLightningAsFire}% Lightning Damage as Fire");
            
            if (nodeStats.physicalIncrease != 0) elementalLegacyStats.Add($"+{nodeStats.physicalIncrease}% Physical Damage");
            if (nodeStats.physical != 0) elementalLegacyStats.Add($"+{nodeStats.physical} Physical Damage");
            if (nodeStats.chanceToBleed != 0) elementalLegacyStats.Add($"+{nodeStats.chanceToBleed}% Chance to Bleed");
            if (nodeStats.increasedBleedMagnitude != 0) elementalLegacyStats.Add($"+{nodeStats.increasedBleedMagnitude}% Bleed Magnitude");
            
            if (nodeStats.chaosIncrease != 0) elementalLegacyStats.Add($"+{nodeStats.chaosIncrease}% Chaos Damage");
            if (nodeStats.chaos != 0) elementalLegacyStats.Add($"+{nodeStats.chaos} Chaos Damage");
            if (nodeStats.chanceToPoison != 0) elementalLegacyStats.Add($"+{nodeStats.chanceToPoison}% Chance to Poison");
            if (nodeStats.increasedPoisonMagnitude != 0) elementalLegacyStats.Add($"+{nodeStats.increasedPoisonMagnitude}% Poison Magnitude");
            if (elementalLegacyStats.Count > 0)
            {
                statsList.Add("Elemental Legacy Stats:");
                statsList.AddRange(elementalLegacyStats);
                statsList.Add(""); // Empty line for spacing
            }

            if (statsList.Count > 0)
            {
                description += "\n\nStats:\n" + string.Join("\n", statsList);
            }
        }

        return description;
    }

    /// <summary>
    /// Convert node type string to NodeType enum
    /// </summary>
    public NodeType GetNodeTypeEnum()
    {
        if (string.IsNullOrEmpty(nodeType))
        {
            return PassiveTree.NodeType.Travel;
        }

        switch (nodeType.ToLower())
        {
            case "main": return PassiveTree.NodeType.Start;
            case "extension": return PassiveTree.NodeType.Extension;
            case "notable": return PassiveTree.NodeType.Notable;
            case "small": return PassiveTree.NodeType.Small;
            default: return PassiveTree.NodeType.Travel;
        }
    }

    void Start()
    {
        if (showDebugInfo && HasJsonData())
        {
            Debug.Log($"[CellJsonData] {gameObject.name} has JSON data: '{nodeName}' ({nodeType})");
        }
    }

    void OnValidate()
    {
        // Update the name in the inspector to show the node name
        if (!string.IsNullOrEmpty(nodeName))
        {
            gameObject.name = $"Cell_{nodePosition.x}_{nodePosition.y}_{nodeName}";
        }
    }

    #region Context Menu Methods

    /// <summary>
    /// Load JSON data for this specific cell based on its position
    /// </summary>
    [ContextMenu("Load JSON Data for This Cell")]
    public void LoadJsonDataForThisCell()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"[CellJsonData] No JSON file assigned to {gameObject.name}. Please assign a JSON file first.");
            return;
        }

        // Get the cell position from the CellController component (more reliable than name parsing)
        Vector2Int cellPosition = GetCellPositionFromController();
        if (cellPosition == new Vector2Int(-1, -1))
        {
            // Fallback to name parsing
            cellPosition = GetCellPositionFromName();
            if (cellPosition == new Vector2Int(-1, -1))
            {
                Debug.LogError($"[CellJsonData] Could not determine cell position from controller or name: {gameObject.name}");
                return;
            }
        }

        // Load JSON data using custom parser for jagged arrays
        var allNodes = ParseJaggedJsonNodes(jsonFile.text);
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError($"[CellJsonData] Failed to parse JSON data from {jsonFile.name}");
            return;
        }

        // Find the matching node data
        JsonNodeData matchingNode = FindNodeDataByPosition(allNodes, cellPosition);
        if (matchingNode != null)
        {
            SetJsonData(matchingNode);
            Debug.Log($"[CellJsonData] Successfully loaded JSON data for {gameObject.name}: '{matchingNode.name}'");
            
            // Update the cell name to include the node name
            UpdateCellName();
            
            // Save changes to prefab if this is a prefab instance
            SaveChangesToPrefab();
        }
        else
        {
            Debug.LogWarning($"[CellJsonData] No JSON data found for position {cellPosition} in {jsonFile.name}");
        }
    }

    /// <summary>
    /// Load JSON data for this cell by matching the expected ID pattern
    /// </summary>
    [ContextMenu("Load JSON Data by ID Pattern")]
    public void LoadJsonDataByIdPattern()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"[CellJsonData] No JSON file assigned to {gameObject.name}. Please assign a JSON file first.");
            return;
        }

        // Get the cell position from the game object name
        Vector2Int cellPosition = GetCellPositionFromName();
        if (cellPosition == new Vector2Int(-1, -1))
        {
            Debug.LogError($"[CellJsonData] Could not determine cell position from name: {gameObject.name}");
            return;
        }

        // Generate expected ID pattern
        string expectedId = GenerateExpectedId(cellPosition);
        
        // Load JSON data using custom parser for jagged arrays
        var allNodes = ParseJaggedJsonNodes(jsonFile.text);
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError($"[CellJsonData] Failed to parse JSON data from {jsonFile.name}");
            return;
        }

        // Find the matching node data by ID
        JsonNodeData matchingNode = FindNodeDataById(allNodes, expectedId);
        if (matchingNode != null)
        {
            SetJsonData(matchingNode);
            Debug.Log($"[CellJsonData] Successfully loaded JSON data for {gameObject.name} with ID '{expectedId}': '{matchingNode.name}'");
            
            // Update the cell name to include the node name
            UpdateCellName();
            
            // Save changes to prefab if this is a prefab instance
            SaveChangesToPrefab();
        }
        else
        {
            Debug.LogWarning($"[CellJsonData] No JSON data found with ID '{expectedId}' in {jsonFile.name}");
            Debug.Log($"[CellJsonData] Available IDs: {GetAvailableIds(allNodes)}");
        }
    }

    /// <summary>
    /// Show available JSON data IDs for debugging
    /// </summary>
    [ContextMenu("Show Available JSON IDs")]
    public void ShowAvailableJsonIds()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"[CellJsonData] No JSON file assigned to {gameObject.name}");
            return;
        }

        var allNodes = ParseJaggedJsonNodes(jsonFile.text);
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError($"[CellJsonData] Failed to parse JSON data from {jsonFile.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Available JSON IDs in {jsonFile.name}:");
        for (int i = 0; i < allNodes.Count; i++)
        {
            var node = allNodes[i];
            if (node != null)
            {
                Debug.Log($"  [{i}] ID: '{node.id}' | Name: '{node.name}' | Position: ({node.position.column}, {node.position.row})");
            }
        }
    }

    /// <summary>
    /// Test JSON parsing to verify the structure
    /// </summary>
    [ContextMenu("Test JSON Parsing")]
    public void TestJsonParsing()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"[CellJsonData] No JSON file assigned to {gameObject.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Testing JSON parsing for {jsonFile.name}...");
        Debug.Log($"[CellJsonData] JSON file size: {jsonFile.text.Length} characters");
        
        // Show first 500 characters of JSON
        string preview = jsonFile.text.Length > 500 ? jsonFile.text.Substring(0, 500) + "..." : jsonFile.text;
        Debug.Log($"[CellJsonData] JSON preview: {preview}");

        // Check if "nodes" exists in the JSON
        if (jsonFile.text.Contains("\"nodes\""))
        {
            Debug.Log($"[CellJsonData] ✅ Found 'nodes' in JSON");
        }
        else
        {
            Debug.LogError($"[CellJsonData] ❌ 'nodes' not found in JSON");
        }

        var allNodes = ParseJaggedJsonNodes(jsonFile.text);
        if (allNodes != null && allNodes.Count > 0)
        {
            Debug.Log($"[CellJsonData] ✅ Successfully parsed {allNodes.Count} nodes");
            
            // Show first few nodes
            for (int i = 0; i < Mathf.Min(3, allNodes.Count); i++)
            {
                var node = allNodes[i];
                if (node != null)
                {
                    Debug.Log($"[CellJsonData] Sample node [{i}]: ID='{node.id}', Name='{node.name}', Type='{node.type}', Position=({node.position.column},{node.position.row})");
                }
            }
        }
        else
        {
            Debug.LogError($"[CellJsonData] ❌ Failed to parse JSON nodes");
        }
    }

    /// <summary>
    /// Clear all JSON data from this cell
    /// </summary>
    [ContextMenu("Clear JSON Data")]
    public void ClearJsonData()
    {
        nodeId = "";
        nodeName = "";
        nodeDescription = "";
        nodeType = "";
        nodePosition = Vector2Int.zero;
        nodeCost = 0;
        maxRank = 0;
        currentRank = 0;
        nodeStats = null;

        Debug.Log($"[CellJsonData] Cleared JSON data from {gameObject.name}");
    }

    /// <summary>
    /// Debug method to show JSON content around nodes section
    /// </summary>
    [ContextMenu("Debug JSON Content")]
    public void DebugJsonContent()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"[CellJsonData] No JSON file assigned to {gameObject.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Debugging JSON content for {jsonFile.name}...");
        
        // Find the nodes section
        int nodesIndex = jsonFile.text.IndexOf("\"nodes\"");
        if (nodesIndex != -1)
        {
            // Show content around the nodes section
            int start = Mathf.Max(0, nodesIndex - 100);
            int end = Mathf.Min(jsonFile.text.Length, nodesIndex + 200);
            string context = jsonFile.text.Substring(start, end - start);
            Debug.Log($"[CellJsonData] JSON context around 'nodes': {context}");
        }
        else
        {
            Debug.LogError($"[CellJsonData] 'nodes' not found in JSON");
            // Show first 300 characters
            string preview = jsonFile.text.Length > 300 ? jsonFile.text.Substring(0, 300) + "..." : jsonFile.text;
            Debug.Log($"[CellJsonData] JSON preview: {preview}");
        }
    }

    /// <summary>
    /// Load JSON data for all cells in the board by position
    /// This method finds all CellJsonData components in the board and loads their data
    /// </summary>
    [ContextMenu("Load JSON Data for All Cells in Board")]
    public void LoadJsonDataForAllCellsInBoard()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"[CellJsonData] No JSON file assigned to {gameObject.name}. Please assign a JSON file first.");
            return;
        }

        Debug.Log($"[CellJsonData] Loading JSON data for all cells in board using file: {jsonFile.name}");

        // Find the board GameObject (go up the hierarchy to find the board)
        GameObject board = FindBoardGameObject();
        if (board == null)
        {
            Debug.LogError($"[CellJsonData] Could not find board GameObject for {gameObject.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Found board: {board.name}");

        // Find all CellJsonData components in the board
        CellJsonData[] allCellJsonData = board.GetComponentsInChildren<CellJsonData>();
        Debug.Log($"[CellJsonData] Found {allCellJsonData.Length} CellJsonData components in board");

        // Parse JSON data once
        var allNodes = ParseJaggedJsonNodes(jsonFile.text);
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError($"[CellJsonData] Failed to parse JSON data from {jsonFile.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Parsed {allNodes.Count} nodes from JSON");

        int successCount = 0;
        int failCount = 0;

        // Load data for each cell
        foreach (CellJsonData cellJsonData in allCellJsonData)
        {
            if (cellJsonData == null) continue;

            // Get cell position from the CellController component
            Vector2Int cellPosition = GetCellPositionFromController(cellJsonData.gameObject);
            if (cellPosition == new Vector2Int(-1, -1))
            {
                // Fallback to name parsing
                cellPosition = GetCellPositionFromName(cellJsonData.gameObject);
                if (cellPosition == new Vector2Int(-1, -1))
                {
                    Debug.LogWarning($"[CellJsonData] Could not determine position for cell: {cellJsonData.gameObject.name}");
                    failCount++;
                    continue;
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[CellJsonData] Processing cell: {cellJsonData.gameObject.name} at position ({cellPosition.x}, {cellPosition.y})");
            }

            // Find matching node data by position
            JsonNodeData matchingNode = FindNodeDataByPosition(allNodes, cellPosition);
            if (matchingNode != null)
            {
                // Set the JSON file reference first
                cellJsonData.SetJsonFile(jsonFile);
                cellJsonData.SetBoardId(board.name);
                
                // Set the JSON data
                cellJsonData.SetJsonData(matchingNode);
                
                // Update the cell name to include the node name
                cellJsonData.UpdateCellName();
                
                successCount++;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[CellJsonData] ✅ Loaded data for {cellJsonData.gameObject.name}: '{matchingNode.name}' at {cellPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"[CellJsonData] ❌ No JSON data found for position {cellPosition} in cell: {cellJsonData.gameObject.name}");
                failCount++;
            }
        }

        Debug.Log($"[CellJsonData] Bulk loading complete: {successCount} successful, {failCount} failed");
    }

    /// <summary>
    /// Alternative bulk loading method that works with the current cell's parent hierarchy
    /// </summary>
    [ContextMenu("Load JSON Data for All Cells in Parent Board")]
    public void LoadJsonDataForAllCellsInParentBoard()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"[CellJsonData] No JSON file assigned to {gameObject.name}. Please assign a JSON file first.");
            return;
        }

        Debug.Log($"[CellJsonData] Loading JSON data for all cells in parent board using file: {jsonFile.name}");

        // Find the ExtensionBoardCells parent (this should be more reliable)
        Transform extensionBoardCells = transform.parent;
        if (extensionBoardCells == null || !extensionBoardCells.name.Contains("ExtensionBoardCells"))
        {
            Debug.LogError($"[CellJsonData] Could not find ExtensionBoardCells parent for {gameObject.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Found ExtensionBoardCells: {extensionBoardCells.name}");

        // Find all CellJsonData components in the ExtensionBoardCells
        CellJsonData[] allCellJsonData = extensionBoardCells.GetComponentsInChildren<CellJsonData>();
        Debug.Log($"[CellJsonData] Found {allCellJsonData.Length} CellJsonData components in ExtensionBoardCells");

        // Parse JSON data once
        var allNodes = ParseJaggedJsonNodes(jsonFile.text);
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError($"[CellJsonData] Failed to parse JSON data from {jsonFile.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Parsed {allNodes.Count} nodes from JSON");

        int successCount = 0;
        int failCount = 0;

        // Load data for each cell
        foreach (CellJsonData cellJsonData in allCellJsonData)
        {
            if (cellJsonData == null) continue;

            // Get cell position from the CellController component
            Vector2Int cellPosition = GetCellPositionFromController(cellJsonData.gameObject);
            if (cellPosition == new Vector2Int(-1, -1))
            {
                // Fallback to name parsing
                cellPosition = GetCellPositionFromName(cellJsonData.gameObject);
                if (cellPosition == new Vector2Int(-1, -1))
                {
                    Debug.LogWarning($"[CellJsonData] Could not determine position for cell: {cellJsonData.gameObject.name}");
                    failCount++;
                    continue;
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[CellJsonData] Processing cell: {cellJsonData.gameObject.name} at position ({cellPosition.x}, {cellPosition.y})");
            }

            // Find matching node data by position
            JsonNodeData matchingNode = FindNodeDataByPosition(allNodes, cellPosition);
            if (matchingNode != null)
            {
                // Set the JSON file reference first
                cellJsonData.SetJsonFile(jsonFile);
                cellJsonData.SetBoardId(extensionBoardCells.parent.name);
                
                // Set the JSON data
                cellJsonData.SetJsonData(matchingNode);
                
                // Update the cell name to include the node name
                cellJsonData.UpdateCellName();
                
                successCount++;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[CellJsonData] ✅ Loaded data for {cellJsonData.gameObject.name}: '{matchingNode.name}' at {cellPosition}");
                }
            }
            else
            {
                Debug.LogWarning($"[CellJsonData] ❌ No JSON data found for position {cellPosition} in cell: {cellJsonData.gameObject.name}");
                failCount++;
            }
        }

        Debug.Log($"[CellJsonData] Bulk loading complete: {successCount} successful, {failCount} failed");
    }

    /// <summary>
    /// Find the board GameObject by going up the hierarchy
    /// </summary>
    private GameObject FindBoardGameObject()
    {
        Transform current = transform;
        
        if (showDebugInfo)
        {
            Debug.Log($"[CellJsonData] Starting hierarchy search from: {current.name}");
        }
        
        // Go up the hierarchy to find the board
        while (current != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[CellJsonData] Checking: {current.name}");
                
                // Log all components on this GameObject
                Component[] components = current.GetComponents<Component>();
                string componentNames = "";
                foreach (var comp in components)
                {
                    if (comp != null)
                    {
                        componentNames += comp.GetType().Name + ", ";
                    }
                }
                Debug.Log($"[CellJsonData] Components on {current.name}: {componentNames}");
            }
            
            // Check if this is a board (has ExtensionBoardController or similar)
            if (current.GetComponent<ExtensionBoardController>() != null || 
                current.name.Contains("Board") || 
                current.name.Contains("ExtensionBoard"))
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[CellJsonData] ✅ Found board: {current.name}");
                }
                return current.gameObject;
            }
            current = current.parent;
        }
        
        if (showDebugInfo)
        {
            Debug.LogError($"[CellJsonData] ❌ No board found in hierarchy for {gameObject.name}");
        }
        return null;
    }

    /// <summary>
    /// Get cell position from CellController component on the given GameObject
    /// </summary>
    private Vector2Int GetCellPositionFromController(GameObject cellGameObject)
    {
        CellController cellController = cellGameObject.GetComponent<CellController>();
        if (cellController != null)
        {
            return cellController.GridPosition;
        }
        
        CellController_EXT cellControllerExt = cellGameObject.GetComponent<CellController_EXT>();
        if (cellControllerExt != null)
        {
            return cellControllerExt.GridPosition;
        }
        
        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// Get cell position from GameObject name
    /// </summary>
    private Vector2Int GetCellPositionFromName(GameObject cellGameObject)
    {
        string name = cellGameObject.name;
        
        if (name.StartsWith("Cell_"))
        {
            string[] parts = name.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                return new Vector2Int(x, y);
            }
        }
        
        return new Vector2Int(-1, -1);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get cell position from the CellController component (more reliable)
    /// </summary>
    private Vector2Int GetCellPositionFromController()
    {
        CellController cellController = GetComponent<CellController>();
        if (cellController != null)
        {
            Vector2Int position = cellController.GridPosition;
            Debug.Log($"[CellJsonData] Got position from CellController: ({position.x}, {position.y})");
            return position;
        }
        
        Debug.LogWarning($"[CellJsonData] No CellController component found on {gameObject.name}");
        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// Get cell position from the game object name (e.g., "Cell_2_3" -> (2, 3))
    /// </summary>
    private Vector2Int GetCellPositionFromName()
    {
        string name = gameObject.name;
        Debug.Log($"[CellJsonData] GetCellPositionFromName called for: {name}");
        
        if (name.StartsWith("Cell_"))
        {
            string[] parts = name.Split('_');
            if (parts.Length >= 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                Debug.Log($"[CellJsonData] Parsed position from Cell_ format: ({x}, {y})");
                return new Vector2Int(x, y);
            }
        }
        
        Debug.LogWarning($"[CellJsonData] Could not parse position from name: {name}");
        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// Generate expected ID based on cell position and board ID
    /// </summary>
    private string GenerateExpectedId(Vector2Int position)
    {
        // Both GameObject names and JSON IDs use "Cell_X_Y" format
        return $"Cell_{position.x}_{position.y}";
    }

    /// <summary>
    /// Parse JSON with jagged array structure using a simpler approach
    /// </summary>
    private System.Collections.Generic.List<JsonNodeData> ParseJaggedJsonNodes(string jsonText)
    {
        try
        {
            var allNodes = new System.Collections.Generic.List<JsonNodeData>();
            
            // First, find the "nodes" array in the JSON (with flexible spacing)
            int nodesStart = jsonText.IndexOf("\"nodes\"");
            if (nodesStart == -1)
            {
                Debug.LogError("[CellJsonData] Could not find 'nodes' in JSON");
                return null;
            }
            
            // Find the colon after "nodes"
            int colonIndex = jsonText.IndexOf(':', nodesStart);
            if (colonIndex == -1)
            {
                Debug.LogError("[CellJsonData] Could not find colon after 'nodes'");
                return null;
            }
            
            // Find the start of the actual array content (after the opening bracket)
            int arrayStart = jsonText.IndexOf('[', colonIndex);
            if (arrayStart == -1)
            {
                Debug.LogError("[CellJsonData] Could not find opening bracket for nodes array");
                return null;
            }
            
            Debug.Log($"[CellJsonData] Found nodes at position {nodesStart}, colon at {colonIndex}, array starts at {arrayStart}");
            
            // Now search within the nodes array for individual node objects
            int searchIndex = arrayStart + 1; // Start after the opening bracket
            int nodeCount = 0;
            
            while (searchIndex < jsonText.Length)
            {
                // Look for the start of a node object within the array
                int nodeStart = jsonText.IndexOf('{', searchIndex);
                if (nodeStart == -1) break;
                
                // Find the matching closing brace
                int braceCount = 0;
                int nodeEnd = -1;
                bool inString = false;
                bool escaped = false;
                
                for (int i = nodeStart; i < jsonText.Length; i++)
                {
                    char c = jsonText[i];
                    
                    if (c == '"' && !escaped)
                    {
                        inString = !inString;
                    }
                    else if (c == '\\' && inString)
                    {
                        escaped = !escaped;
                    }
                    else if (!inString)
                    {
                        if (c == '{')
                        {
                            braceCount++;
                        }
                        else if (c == '}')
                        {
                            braceCount--;
                            if (braceCount == 0)
                            {
                                nodeEnd = i;
                                break;
                            }
                        }
                    }
                    
                    if (escaped) escaped = false;
                }
                
                if (nodeEnd == -1) break;
                
                // Extract the node JSON
                string nodeJson = jsonText.Substring(nodeStart, nodeEnd - nodeStart + 1);
                
                // Debug: Log what we found (only for valid nodes)
                if (nodeJson.Contains("\"id\"") && nodeJson.Contains("\"position\""))
                {
                    Debug.Log($"[CellJsonData] Found valid node: {nodeJson.Substring(0, Mathf.Min(100, nodeJson.Length))}...");
                }
                
                // Check if this looks like a valid node (contains "id" and "position")
                if (nodeJson.Contains("\"id\"") && nodeJson.Contains("\"position\""))
                {
                    try
                    {
                        JsonNodeData node = JsonUtility.FromJson<JsonNodeData>(nodeJson);
                        if (node != null && !string.IsNullOrEmpty(node.id))
                        {
                            allNodes.Add(node);
                            nodeCount++;
                            Debug.Log($"[CellJsonData] Parsed node {nodeCount}: ID='{node.id}', Name='{node.name}'");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[CellJsonData] Failed to parse node JSON: {e.Message}");
                    }
                }
                
                searchIndex = nodeEnd + 1;
            }
            
            Debug.Log($"[CellJsonData] Successfully parsed {allNodes.Count} nodes from JSON");
            return allNodes;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CellJsonData] Error parsing JSON: {e.Message}");
            return null;
        }
    }




    /// <summary>
    /// Find node data by position
    /// </summary>
    private JsonNodeData FindNodeDataByPosition(System.Collections.Generic.List<JsonNodeData> allNodes, Vector2Int position)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[CellJsonData] Looking for node at position ({position.x}, {position.y})");
        }
        
        foreach (var node in allNodes)
        {
            if (node != null && node.position != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[CellJsonData] Checking node '{node.name}' at JSON position (column: {node.position.column}, row: {node.position.row})");
                }
                
                // Match: JSON column = Unity X, JSON row = Unity Y
                if (node.position.column == position.x && node.position.row == position.y)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"[CellJsonData] ✅ Found match: '{node.name}' at ({position.x}, {position.y})");
                    }
                    return node;
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.LogWarning($"[CellJsonData] ❌ No node found at position ({position.x}, {position.y})");
        }
        return null;
    }

    /// <summary>
    /// Find node data by ID
    /// </summary>
    private JsonNodeData FindNodeDataById(System.Collections.Generic.List<JsonNodeData> allNodes, string id)
    {
        foreach (var node in allNodes)
        {
            if (node != null && node.id == id)
            {
                return node;
            }
        }
        return null;
    }

    /// <summary>
    /// Get all available IDs for debugging
    /// </summary>
    private string GetAvailableIds(System.Collections.Generic.List<JsonNodeData> allNodes)
    {
        var ids = new System.Collections.Generic.List<string>();
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                ids.Add(node.id);
            }
        }
        return string.Join(", ", ids);
    }

    /// <summary>
    /// Update the GameObject name based on the current JSON data
    /// </summary>
    [ContextMenu("Update Cell Name")]
    public void UpdateCellName()
    {
        if (!string.IsNullOrEmpty(nodeName) && nodePosition != Vector2Int.zero)
        {
            string newName = $"Cell_{nodePosition.x}_{nodePosition.y}_{nodeName}";
            if (gameObject.name != newName)
            {
                gameObject.name = newName;
                Debug.Log($"[CellJsonData] Updated cell name to: {newName}");
                
                // Save changes to prefab if this is a prefab instance
                SaveChangesToPrefab();
            }
            else
            {
                Debug.Log($"[CellJsonData] Cell name is already correct: {newName}");
            }
        }
        else
        {
            Debug.LogWarning($"[CellJsonData] Cannot update cell name - missing nodeName or nodePosition. NodeName: '{nodeName}', Position: {nodePosition}");
        }
    }

    /// <summary>
    /// Manually save the board prefab to ensure all changes are persisted
    /// </summary>
    [ContextMenu("Save Board Prefab")]
    public void SaveBoardPrefabManually()
    {
        SaveBoardPrefab();
    }

    /// <summary>
    /// Update cell names for all cells in the parent board
    /// </summary>
    [ContextMenu("Update All Cell Names in Board")]
    public void UpdateAllCellNamesInBoard()
    {
        // Find the ExtensionBoardCells parent
        Transform extensionBoardCells = transform.parent;
        while (extensionBoardCells != null && !extensionBoardCells.name.Contains("ExtensionBoardCells"))
        {
            extensionBoardCells = extensionBoardCells.parent;
        }

        if (extensionBoardCells == null || !extensionBoardCells.name.Contains("ExtensionBoardCells"))
        {
            Debug.LogError($"[CellJsonData] Could not find ExtensionBoardCells parent for {gameObject.name}");
            return;
        }

        Debug.Log($"[CellJsonData] Updating cell names for all cells in: {extensionBoardCells.name}");

        // Get all CellJsonData components in the board
        var allCellJsonData = extensionBoardCells.GetComponentsInChildren<CellJsonData>();
        int updatedCount = 0;
        int skippedCount = 0;

        foreach (var cellJsonData in allCellJsonData)
        {
            if (!string.IsNullOrEmpty(cellJsonData.nodeName) && cellJsonData.nodePosition != Vector2Int.zero)
            {
                string newName = $"Cell_{cellJsonData.nodePosition.x}_{cellJsonData.nodePosition.y}_{cellJsonData.nodeName}";
                if (cellJsonData.gameObject.name != newName)
                {
                    cellJsonData.gameObject.name = newName;
                    updatedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                Debug.LogWarning($"[CellJsonData] Skipping {cellJsonData.gameObject.name} - missing nodeName or nodePosition");
                skippedCount++;
            }
        }

        Debug.Log($"[CellJsonData] Cell name update complete: {updatedCount} updated, {skippedCount} skipped");
        
        // Save changes to prefab if this is a prefab instance
        SaveChangesToPrefab();
        
        // Also save the entire board prefab
        SaveBoardPrefab();
    }

    /// <summary>
    /// Save changes to the prefab if this is a prefab instance
    /// </summary>
    private void SaveChangesToPrefab()
    {
#if UNITY_EDITOR
        // Try to save the individual cell first (if it's a prefab instance)
        var prefabInstance = UnityEditor.PrefabUtility.GetPrefabInstanceHandle(gameObject);
        if (prefabInstance != null)
        {
            // Mark the prefab as dirty so Unity knows it has changes
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(gameObject);
            
            // Apply the changes to the prefab
            UnityEditor.PrefabUtility.ApplyPrefabInstance(gameObject, UnityEditor.InteractionMode.AutomatedAction);
            
            // Force Unity to save the prefab asset
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"[CellJsonData] Changes saved to prefab asset for {gameObject.name}");
        }
        else
        {
            // Individual cell is not a prefab instance, try to save at the board level
            SaveBoardPrefab();
        }
#endif
    }

    /// <summary>
    /// Save the entire board prefab to ensure all changes are persisted
    /// </summary>
    private void SaveBoardPrefab()
    {
#if UNITY_EDITOR
        // Find the root board GameObject - look for various board naming patterns
        Transform boardRoot = transform;
        while (boardRoot.parent != null)
        {
            string name = boardRoot.name.ToLower();
            if (name.Contains("board") || name.Contains("t1") || name.Contains("fire") || 
                name.Contains("cold") || name.Contains("lightning") || name.Contains("physical"))
            {
                break;
            }
            boardRoot = boardRoot.parent;
        }

        if (boardRoot != null)
        {
            // Check if the board root is a prefab instance
            var boardPrefabInstance = UnityEditor.PrefabUtility.GetPrefabInstanceHandle(boardRoot.gameObject);
            if (boardPrefabInstance != null)
            {
                // Mark the entire board as dirty
                UnityEditor.EditorUtility.SetDirty(boardRoot.gameObject);
                
                // Apply changes to the board prefab
                UnityEditor.PrefabUtility.ApplyPrefabInstance(boardRoot.gameObject, UnityEditor.InteractionMode.AutomatedAction);
                
                // Force Unity to save the prefab asset
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                
                Debug.Log($"[CellJsonData] Board prefab saved: {boardRoot.name}");
            }
            else
            {
                // Try to find the prefab asset and save it directly
                var prefabAsset = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(boardRoot.gameObject);
                if (prefabAsset != null)
                {
                    UnityEditor.EditorUtility.SetDirty(prefabAsset);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    Debug.Log($"[CellJsonData] Board prefab asset saved: {prefabAsset.name}");
                }
                else
                {
                    Debug.Log($"[CellJsonData] Board root {boardRoot.name} is not a prefab instance and no prefab asset found");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[CellJsonData] Could not find board root for {gameObject.name}");
        }
#endif
    }

    #endregion
}
