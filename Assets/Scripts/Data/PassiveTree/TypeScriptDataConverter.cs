using UnityEngine;
using System.Collections.Generic;
using System;
using PassiveTree;

/// <summary>
/// Converts TypeScript passive tree data to C# ScriptableObject format
/// Translates CoreBoard.ts data structure to PassiveTreeBoardData
/// </summary>
public static class TypeScriptDataConverter
{
    /// <summary>
    /// Convert TypeScript board data to C# PassiveTreeBoardData
    /// </summary>
    public static PassiveTreeBoardData ConvertCoreBoardData()
    {
        PassiveTreeBoardData boardData = ScriptableObject.CreateInstance<PassiveTreeBoardData>();
        
        // Set basic board information
        SetBoardDataField(boardData, "boardName", "Core Board");
        SetBoardDataField(boardData, "boardDescription", "The foundation of your character progression");
        SetBoardDataField(boardData, "boardSize", new Vector2Int(7, 7));
        
        // Set start node position (from TypeScript: row 3, column 3)
        SetBoardDataField(boardData, "startNodePosition", new Vector2Int(3, 3));
        
        // Set extension positions (from TypeScript extension points)
        List<Vector2Int> extensionPositions = new List<Vector2Int>
        {
            new Vector2Int(0, 3), // core_ext_bottom (row 0, column 3)
            new Vector2Int(3, 0), // core_ext_left (row 3, column 0)
            new Vector2Int(3, 6), // core_ext_right (row 3, column 6)
            new Vector2Int(6, 3)  // core_ext_top (row 6, column 3)
        };
        SetBoardDataField(boardData, "extensionPositions", extensionPositions);
        
        // Set travel positions (nodes connecting to start)
        List<Vector2Int> travelPositions = new List<Vector2Int>
        {
            new Vector2Int(1, 3), new Vector2Int(2, 3), // South path
            new Vector2Int(3, 1), new Vector2Int(3, 2), // West path
            new Vector2Int(3, 4), new Vector2Int(3, 5), // East path
            new Vector2Int(4, 3), new Vector2Int(5, 3)  // North path
        };
        SetBoardDataField(boardData, "travelPositions", travelPositions);
        
        // Set notable positions (from TypeScript notable nodes)
        List<Vector2Int> notablePositions = new List<Vector2Int>
        {
            new Vector2Int(1, 1), // Path of the Warrior
            new Vector2Int(1, 5), // Path of the Mage
            new Vector2Int(5, 1), // Path of the Polyglot
            new Vector2Int(5, 5)  // Path of the Huntress
        };
        SetBoardDataField(boardData, "notablePositions", notablePositions);
        
        // Create manual overrides for all specific nodes from TypeScript
        List<CellDataOverride> manualOverrides = CreateManualOverridesFromTypeScript();
        SetBoardDataField(boardData, "manualOverrides", manualOverrides);
        
        // Set generation settings
        SetBoardDataField(boardData, "autoGenerateOnAwake", true);
        SetBoardDataField(boardData, "useRandomDescriptions", false);
        SetBoardDataField(boardData, "generateUniqueNames", true);
        
        return boardData;
    }
    
    /// <summary>
    /// Create manual overrides for all nodes from TypeScript data
    /// </summary>
    private static List<CellDataOverride> CreateManualOverridesFromTypeScript()
    {
        List<CellDataOverride> overrides = new List<CellDataOverride>();
        
        // Row 0
        overrides.Add(CreateNodeOverride(0, 0, "Max health", "4% increased max health", NodeType.Small, 1, new List<string> { "maxHealthIncrease: 4" }));
        overrides.Add(CreateNodeOverride(0, 1, "Max health", "4% Increased Max health", NodeType.Small, 1, new List<string> { "maxHealthIncrease: 4", "strength: 5" }));
        overrides.Add(CreateNodeOverride(0, 2, "Intelligence", "+10 Intelligence", NodeType.Small, 1, new List<string> { "intelligence: 10" }));
        overrides.Add(CreateNodeOverride(0, 3, "Extension Point", "Connect to another board", NodeType.Extension, 0, new List<string>()));
        overrides.Add(CreateNodeOverride(0, 4, "Intelligence", "+10 Intelligence", NodeType.Small, 1, new List<string> { "intelligence: 10" }));
        overrides.Add(CreateNodeOverride(0, 5, "Increased energy shield", "6% increased energy shield", NodeType.Small, 1, new List<string> { "maxEnergyShieldIncrease: 6" }));
        overrides.Add(CreateNodeOverride(0, 6, "Increased energy shield", "6% increased energy shield", NodeType.Small, 1, new List<string> { "maxEnergyShieldIncrease: 6" }));
        
        // Row 1
        overrides.Add(CreateNodeOverride(1, 0, "Max health", "4% increased max health", NodeType.Small, 1, new List<string> { "maxHealthIncrease: 4" }));
        overrides.Add(CreateNodeOverride(1, 1, "Path of the Warrior", "+8% increased Max Health, +50 Armour, +20 Strength", NodeType.Notable, 1, new List<string> { "maxHealthIncrease: 8", "armor: 50", "strength: 20" }));
        overrides.Add(CreateNodeOverride(1, 2, "Intelligence", "+10 Intelligence", NodeType.Small, 1, new List<string> { "intelligence: 10" }));
        overrides.Add(CreateNodeOverride(1, 3, "Intelligence", "+10 Intelligence", NodeType.Small, 1, new List<string> { "intelligence: 10" }));
        overrides.Add(CreateNodeOverride(1, 4, "Intelligence", "+10 Intelligence", NodeType.Small, 1, new List<string> { "intelligence: 10" }));
        overrides.Add(CreateNodeOverride(1, 5, "Path of the Mage", "+16% increased Spell damage, +8% increased Energy shield, +20 Intelligence", NodeType.Notable, 1, new List<string> { "spellPowerIncrease: 16", "maxEnergyShield: 8", "intelligence: 20" }));
        overrides.Add(CreateNodeOverride(1, 6, "Increased energy shield", "6% increased energy shield", NodeType.Small, 1, new List<string> { "maxEnergyShieldIncrease: 6" }));
        
        // Row 2
        overrides.Add(CreateNodeOverride(2, 0, "Strength", "+10 Strength", NodeType.Small, 1, new List<string> { "strength: 10" }));
        overrides.Add(CreateNodeOverride(2, 1, "Strength", "+10 Strength", NodeType.Small, 1, new List<string> { "strength: 10" }));
        overrides.Add(CreateNodeOverride(2, 2, "Intelligence & Strength", "+5 Intelligence, 5 Strength", NodeType.Small, 1, new List<string> { "intelligence: 5", "strength: 5" }));
        overrides.Add(CreateNodeOverride(2, 3, "Intelligence", "+10 Intelligence", NodeType.Small, 1, new List<string> { "intelligence: 10" }));
        overrides.Add(CreateNodeOverride(2, 4, "Intelligence & Dexterity", "+5 Intelligence, 5 Dexterity", NodeType.Small, 1, new List<string> { "intelligence: 5", "dexterity: 5" }));
        overrides.Add(CreateNodeOverride(2, 5, "Dexterity", "+10 Dexterity", NodeType.Small, 1, new List<string> { "dexterity: 10" }));
        overrides.Add(CreateNodeOverride(2, 6, "Dexterity", "+10 Dexterity", NodeType.Small, 1, new List<string> { "dexterity: 10" }));
        
        // Row 3
        overrides.Add(CreateNodeOverride(3, 0, "Extension Point", "Connect to another board", NodeType.Extension, 0, new List<string>()));
        overrides.Add(CreateNodeOverride(3, 1, "Strength", "+10 Strength", NodeType.Small, 1, new List<string> { "strength: 10" }));
        overrides.Add(CreateNodeOverride(3, 2, "Strength", "+10 Strength", NodeType.Small, 1, new List<string> { "strength: 10" }));
        overrides.Add(CreateNodeOverride(3, 3, "START", "Your journey begins here", NodeType.Start, 0, new List<string>()));
        overrides.Add(CreateNodeOverride(3, 4, "Dexterity", "+10 Dexterity", NodeType.Small, 1, new List<string> { "dexterity: 10" }));
        overrides.Add(CreateNodeOverride(3, 5, "Dexterity", "+10 Dexterity", NodeType.Small, 1, new List<string> { "dexterity: 10" }));
        overrides.Add(CreateNodeOverride(3, 6, "Extension Point", "Connect to another board", NodeType.Extension, 0, new List<string>()));
        
        // Row 4
        overrides.Add(CreateNodeOverride(4, 0, "Strength", "+10 Strength", NodeType.Small, 1, new List<string> { "strength: 10" }));
        overrides.Add(CreateNodeOverride(4, 1, "Strength", "+10 Strength", NodeType.Small, 1, new List<string> { "strength: 10" }));
        overrides.Add(CreateNodeOverride(4, 2, "Strength", "+10 Strength", NodeType.Small, 1, new List<string> { "strength: 10" }));
        overrides.Add(CreateNodeOverride(4, 3, "All attributes", "+3 to all attributes", NodeType.Small, 1, new List<string> { "strength: 3", "dexterity: 3", "intelligence: 3" }));
        overrides.Add(CreateNodeOverride(4, 4, "Flat Evasion", "+25 Evasion", NodeType.Small, 1, new List<string> { "evasion: 25" }));
        overrides.Add(CreateNodeOverride(4, 5, "Dexterity", "+10 Dexterity", NodeType.Small, 1, new List<string> { "dexterity: 10" }));
        overrides.Add(CreateNodeOverride(4, 6, "Dexterity", "+10 Dexterity", NodeType.Small, 1, new List<string> { "dexterity: 10" }));
        
        // Row 5
        overrides.Add(CreateNodeOverride(5, 0, "All attributes", "+3 to all attributes", NodeType.Small, 1, new List<string> { "strength: 5", "dexterity: 5" }));
        overrides.Add(CreateNodeOverride(5, 1, "Path of the Polyglot", "+36% increased Armour, Evasion and energy shield, +10% to all Elemental Resistances, +5 to all attributes", NodeType.Notable, 1, new List<string> { "armorIncrease: 36", "increasedEvasion: 36", "elementalResist: 10", "strength: 5", "dexterity: 5", "intelligence: 5" }));
        overrides.Add(CreateNodeOverride(5, 2, "Strength & Dexterity", "+5 Strength, +5 Dexterity", NodeType.Small, 1, new List<string> { "strength: 5", "dexterity: 5" }));
        overrides.Add(CreateNodeOverride(5, 3, "All attributes", "+3 to all attributes", NodeType.Small, 1, new List<string> { "strength: 3", "dexterity: 3", "intelligence: 3" }));
        overrides.Add(CreateNodeOverride(5, 4, "Strength & Dexterity", "+5 Strength, +5 Dexterity", NodeType.Small, 1, new List<string> { "strength: 5", "dexterity: 5" }));
        overrides.Add(CreateNodeOverride(5, 5, "Path of the Huntress", "+250 to Accuracy rating, +16% increased projectile damage, +20 Dexterity", NodeType.Notable, 1, new List<string> { "accuracy: 250", "increasedProjectileDamage: 16", "dexterity: 20" }));
        overrides.Add(CreateNodeOverride(5, 6, "Strength & Dexterity", "+5 Strength, +5 Dexterity", NodeType.Small, 1, new List<string> { "strength: 5", "dexterity: 5" }));
        
        // Row 6
        overrides.Add(CreateNodeOverride(6, 0, "All attributes", "+3 to all attributes", NodeType.Small, 1, new List<string> { "strength: 3", "dexterity: 3", "intelligence: 3" }));
        overrides.Add(CreateNodeOverride(6, 1, "All attributes", "+3 to all attributes", NodeType.Small, 1, new List<string> { "strength: 3", "dexterity: 3", "intelligence: 3" }));
        overrides.Add(CreateNodeOverride(6, 2, "All attributes", "+3 to all attributes", NodeType.Small, 1, new List<string> { "strength: 3", "dexterity: 3", "intelligence: 3" }));
        overrides.Add(CreateNodeOverride(6, 3, "Extension Point", "Connect to another board", NodeType.Extension, 0, new List<string>()));
        overrides.Add(CreateNodeOverride(6, 4, "Increased Evasion", "12% increased Evasion", NodeType.Small, 1, new List<string> { "evasionIncrease: 12" }));
        overrides.Add(CreateNodeOverride(6, 5, "Increased Evasion", "12% increased Evasion", NodeType.Small, 1, new List<string> { "evasionIncrease: 12" }));
        overrides.Add(CreateNodeOverride(6, 6, "Increased Evasion", "12% increased Evasion", NodeType.Small, 1, new List<string> { "evasionIncrease: 12" }));
        
        return overrides;
    }
    
    /// <summary>
    /// Create a node override from TypeScript data
    /// </summary>
    private static CellDataOverride CreateNodeOverride(int x, int y, string name, string description, NodeType nodeType, int cost, List<string> stats)
    {
        CellDataOverride nodeOverride = new CellDataOverride();
        nodeOverride.position = new Vector2Int(x, y);
        nodeOverride.nodeName = name;
        nodeOverride.description = description;
        nodeOverride.nodeType = nodeType;
        nodeOverride.cost = cost;
        nodeOverride.isUnlocked = (nodeType == NodeType.Start);
        nodeOverride.sprite = null; // Will use auto-assignment
        
        // Map TypeScript stats to Unity CharacterStatsData stat names
        // Convert List<string> to Dictionary<string, float> format
        Dictionary<string, float> statsDict = new Dictionary<string, float>();
        List<string> mappedStats = PassiveTreeStatMapper.ConvertStatFormat(stats);
        
        foreach (string stat in mappedStats)
        {
            if (string.IsNullOrEmpty(stat))
                continue;

            // Parse stat format: "statName: value"
            string[] parts = stat.Split(':');
            if (parts.Length == 2)
            {
                string statName = parts[0].Trim();
                string valueString = parts[1].Trim();

                if (float.TryParse(valueString, out float value))
                {
                    statsDict[statName] = value;
                }
            }
        }
        
        nodeOverride.stats = statsDict;
        
        // Set colors based on node type
        switch (nodeType)
        {
            case NodeType.Start:
                nodeOverride.normalColor = Color.green;
                break;
            case NodeType.Extension:
                nodeOverride.normalColor = Color.magenta;
                break;
            case NodeType.Notable:
                nodeOverride.normalColor = Color.blue;
                break;
            case NodeType.Small:
                nodeOverride.normalColor = Color.gray;
                break;
            default:
                nodeOverride.normalColor = Color.white;
                break;
        }
        
        nodeOverride.hoverColor = Color.yellow;
        nodeOverride.selectedColor = Color.cyan;
        
        return nodeOverride;
    }
    
    /// <summary>
    /// Set a field on PassiveTreeBoardData using reflection
    /// </summary>
    private static void SetBoardDataField(PassiveTreeBoardData boardData, string fieldName, object value)
    {
        var field = typeof(PassiveTreeBoardData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(boardData, value);
        }
        else
        {
            Debug.LogWarning($"[TypeScriptDataConverter] Could not find field: {fieldName}");
        }
    }
}
