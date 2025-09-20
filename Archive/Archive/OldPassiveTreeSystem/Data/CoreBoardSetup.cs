using UnityEngine;
using PassiveTree;
using System.Collections.Generic;

/// <summary>
/// Setup script for creating a complete CoreBoard with all nodes
/// Based on the original TypeScript structure
/// </summary>
public static class CoreBoardSetup
{
    /// <summary>
    /// Create a complete CoreBoard with all nodes
    /// </summary>
    public static PassiveBoard CreateCompleteCoreBoard()
    {
        return CreateCompleteCoreBoard(null, true);
    }
    
    /// <summary>
    /// Create a complete CoreBoard with all nodes and sprite manager settings
    /// </summary>
    public static PassiveBoard CreateCompleteCoreBoard(PassiveTreeSpriteManager spriteManager = null, bool useCustomSprites = true)
    {
        var board = new PassiveBoard
        {
            id = "core_board",
            name = "Core Board",
            description = "The foundation of your character progression",
            theme = BoardTheme.Utility,
            size = new Vector2Int(7, 7),
            maxPoints = 20
        };

        // Set sprite manager settings BEFORE initializing
        if (spriteManager != null)
        {
            board.SetSpriteManager(spriteManager);
            board.SetUseCustomSprites(useCustomSprites);
            Debug.Log($"[CoreBoardSetup] Created CoreBoard with sprite manager: {spriteManager.name}");
        }
        else
        {
            Debug.LogWarning("[CoreBoardSetup] Created CoreBoard without sprite manager");
        }

        // Initialize the board
        board.InitializeBoard();

        // Create all nodes
        CreateAllNodes(board);

        // Create extension points
        CreateExtensionPoints(board);

        // Set up node connections
        SetupNodeConnections(board);

        return board;
    }

    /// <summary>
    /// Create all nodes in the 7x7 grid
    /// </summary>
    private static void CreateAllNodes(PassiveBoard board)
    {
        // Row 0
        CreateNode(board, 0, 0, "core_0_0", "Attributes", "+5 Intelligence, 5 Strength", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "strength", 5 } });
        CreateNode(board, 0, 1, "core_0_1", "Attributes", "+5 Intelligence, 5 Strength", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "strength", 5 } });
        CreateNode(board, 0, 2, "core_0_2", "Attributes", "+10 Intelligence", "small", new Dictionary<string, float> { { "intelligence", 10 } });
        CreateNode(board, 0, 3, "core_ext_top", "Extension Point", "Connect to another board", "extension", new Dictionary<string, float>());
        CreateNode(board, 0, 4, "core_0_4", "Attributes", "+10 Intelligence", "small", new Dictionary<string, float> { { "intelligence", 10 } });
        CreateNode(board, 0, 5, "core_0_5", "Attributes", "+5 Intelligence, 5 Dexterity", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "dexterity", 5 } });
        CreateNode(board, 0, 6, "core_0_6", "Attributes", "+5 Intelligence, 5 Dexterity", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "dexterity", 5 } });

        // Row 1
        CreateNode(board, 1, 0, "core_1_0", "Attributes", "+5 Intelligence, 5 Strength", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "strength", 5 } });
        CreateNode(board, 1, 1, "core_Notable_1_1", "Path of the Warrior", "+8% increased Max Health, +50 Armour, +20 Strength", "notable", new Dictionary<string, float> { { "maxHealthIncrease", 8 }, { "armor", 50 }, { "strength", 20 } });
        CreateNode(board, 1, 2, "core_1_2", "Attributes", "+10 Intelligence", "small", new Dictionary<string, float> { { "intelligence", 10 } });
        CreateNode(board, 1, 3, "core_1_3", "Attributes", "+10 Intelligence", "small", new Dictionary<string, float> { { "intelligence", 10 } });
        CreateNode(board, 1, 4, "core_1_4", "Attributes", "+10 Intelligence", "small", new Dictionary<string, float> { { "intelligence", 10 } });
        CreateNode(board, 1, 5, "core_Notable_1_5", "Path of the Mage", "+16% increased Spell damage, +8% increased Energy shield, +20 Intelligence", "notable", new Dictionary<string, float> { { "spellPowerIncrease", 16 }, { "maxEnergyShield", 8 }, { "intelligence", 20 } });
        CreateNode(board, 1, 6, "core_1_6", "Attributes", "+5 Intelligence, 5 Dexterity", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "dexterity", 5 } });

        // Row 2
        CreateNode(board, 2, 0, "core_2_0", "Attributes", "+10 Strength", "small", new Dictionary<string, float> { { "strength", 10 } });
        CreateNode(board, 2, 1, "core_2_1", "Attributes", "+10 Strength", "small", new Dictionary<string, float> { { "strength", 10 } });
        CreateNode(board, 2, 2, "core_2_2", "Attributes", "+5 Intelligence, 5 Strength", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "strength", 5 } });
        CreateNode(board, 2, 3, "core_2_3", "Attributes", "+10 Intelligence", "small", new Dictionary<string, float> { { "intelligence", 10 } });
        CreateNode(board, 2, 4, "core_2_4", "Attributes", "+5 Intelligence, 5 Dexterity", "small", new Dictionary<string, float> { { "intelligence", 5 }, { "dexterity", 5 } });
        CreateNode(board, 2, 5, "core_2_5", "Attributes", "+10 Dexterity", "small", new Dictionary<string, float> { { "dexterity", 10 } });
        CreateNode(board, 2, 6, "core_2_6", "Attributes", "+10 Dexterity", "small", new Dictionary<string, float> { { "dexterity", 10 } });

        // Row 3
        CreateNode(board, 3, 0, "core_ext_left", "Extension Point", "Connect to another board", "extension", new Dictionary<string, float>());
        CreateNode(board, 3, 1, "core_3_1", "Attributes", "+10 Strength", "small", new Dictionary<string, float> { { "strength", 10 } });
        CreateNode(board, 3, 2, "core_3_2", "Attributes", "+10 Strength", "small", new Dictionary<string, float> { { "strength", 10 } });
        CreateNode(board, 3, 3, "core_main", "START", "Your journey begins here", "main", new Dictionary<string, float>(), 1, 1, 0); // Starting node
        CreateNode(board, 3, 4, "core_3_4", "Attributes", "+10 Dexterity", "small", new Dictionary<string, float> { { "dexterity", 10 } });
        CreateNode(board, 3, 5, "core_3_5", "Attributes", "+10 Dexterity", "small", new Dictionary<string, float> { { "dexterity", 10 } });
        CreateNode(board, 3, 6, "core_ext_right", "Extension Point", "Connect to another board", "extension", new Dictionary<string, float>());

        // Row 4
        CreateNode(board, 4, 0, "core_4_0", "Attributes", "+10 Strength", "small", new Dictionary<string, float> { { "strength", 10 } });
        CreateNode(board, 4, 1, "core_4_1", "Attributes", "+10 Strength", "small", new Dictionary<string, float> { { "strength", 10 } });
        CreateNode(board, 4, 2, "core_4_2", "Max Health", "6% increased Max Health", "small", new Dictionary<string, float> { { "maxHealthIncrease", 6 } });
        CreateNode(board, 4, 3, "core_4_3", "Attributes", "+3 to all attributes", "small", new Dictionary<string, float> { { "strength", 3 }, { "dexterity", 3 }, { "intelligence", 3 } });
        CreateNode(board, 4, 4, "core_4_4", "Evasion", "+25 Evasion", "small", new Dictionary<string, float> { { "evasion", 25 } });
        CreateNode(board, 4, 5, "core_4_5", "Attributes", "+10 Dexterity", "small", new Dictionary<string, float> { { "dexterity", 10 } });
        CreateNode(board, 4, 6, "core_4_6", "Attributes", "+10 Dexterity", "small", new Dictionary<string, float> { { "dexterity", 10 } });

        // Row 5
        CreateNode(board, 5, 0, "core_5_0", "Attributes", "+5 Strength, +5 Dexterity", "small", new Dictionary<string, float> { { "strength", 5 }, { "dexterity", 5 } });
        CreateNode(board, 5, 1, "core_Notable_5_1", "Path of the Sentinel", "+36% increased Armour and Evasion, +10% to all Elemental Resistances", "notable", new Dictionary<string, float> { { "armorIncrease", 36 }, { "increasedEvasion", 36 }, { "elementalResist", 10 } });
        CreateNode(board, 5, 2, "core_5_2", "Attributes", "+5 Strength, +5 Dexterity", "small", new Dictionary<string, float> { { "strength", 5 }, { "dexterity", 5 } });
        CreateNode(board, 5, 3, "core_5_3", "Attributes", "+3 to all attributes", "small", new Dictionary<string, float> { { "strength", 3 }, { "dexterity", 3 }, { "intelligence", 3 } });
        CreateNode(board, 5, 4, "core_5_4", "Attributes", "+5 Strength, +5 Dexterity", "small", new Dictionary<string, float> { { "strength", 5 }, { "dexterity", 5 } });
        CreateNode(board, 5, 5, "core_Notable_5_5", "Path of the Huntress", "+100 to Accuracy rating, +16% increased projectile damage, +20 Dexterity", "notable", new Dictionary<string, float> { { "accuracy", 100 }, { "increasedProjectileDamage", 16 }, { "dexterity", 20 } });
        CreateNode(board, 5, 6, "core_5_6", "Attributes", "+5 Strength, +5 Dexterity", "small", new Dictionary<string, float> { { "strength", 5 }, { "dexterity", 5 } });

        // Row 6
        CreateNode(board, 6, 0, "core_6_0", "Max Health", "4% increased Max Health", "small", new Dictionary<string, float> { { "maxHealthIncrease", 4 } });
        CreateNode(board, 6, 1, "core_6_1", "Max Energy Shield", "4% increased Max Energy Shield", "small", new Dictionary<string, float> { { "maxEnergyShieldIncrease", 4 } });
        CreateNode(board, 6, 2, "core_6_2", "Max Energy Shield", "6% increased Max Energy Shield", "small", new Dictionary<string, float> { { "maxEnergyShieldIncrease", 6 } });
        CreateNode(board, 6, 3, "core_ext_bottom", "Extension Point", "Connect to another board", "extension", new Dictionary<string, float>());
        CreateNode(board, 6, 4, "core_6_4", "Max Energy Shield", "6% increased Max Energy Shield", "small", new Dictionary<string, float> { { "maxEnergyShieldIncrease", 6 } });
        CreateNode(board, 6, 5, "core_6_5", "Increased Evasion", "12% increased Evasion", "small", new Dictionary<string, float> { { "evasionIncrease", 12 } });
        CreateNode(board, 6, 6, "core_6_6", "Increased Evasion", "12% increased Evasion", "small", new Dictionary<string, float> { { "evasionIncrease", 12 } });
    }

    /// <summary>
    /// Create a single node
    /// </summary>
    private static void CreateNode(PassiveBoard board, int row, int col, string id, string name, string description, string typeString, Dictionary<string, float> stats, int currentRank = 0, int maxRank = 1, int cost = 1)
    {
        // Convert string type to NodeType enum
        NodeType nodeType = NodeType.Small; // Default
        switch (typeString.ToLower())
        {
            case "main":
                nodeType = NodeType.Main;
                break;
            case "extension":
                nodeType = NodeType.Extension;
                break;
            case "notable":
                nodeType = NodeType.Notable;
                break;
            case "small":
                nodeType = NodeType.Small;
                break;
            case "travel":
                nodeType = NodeType.Travel;
                break;
            case "keystone":
                nodeType = NodeType.Keystone;
                break;
        }

        var node = new PassiveNode
        {
            id = id,
            name = name,
            description = description,
            position = new Vector2Int(row, col),
            type = nodeType,
            stats = stats,
            maxRank = maxRank,
            currentRank = currentRank,
            cost = cost,
            requirements = new List<string>(),
            connections = new List<string>()
        };

        // Use SetNode instead of AddNode
        board.SetNode(row, col, node);
    }

    /// <summary>
    /// Create extension points
    /// </summary>
    private static void CreateExtensionPoints(PassiveBoard board)
    {
        // Extension points are already created as nodes, but we need to add them to the extensionPoints list
        board.extensionPoints = new List<ExtensionPoint>
        {
            new ExtensionPoint { id = "core_ext_top", position = new Vector2Int(0, 3), availableBoards = new List<string>(), maxConnections = 1, currentConnections = 0 },
            new ExtensionPoint { id = "core_ext_left", position = new Vector2Int(3, 0), availableBoards = new List<string>(), maxConnections = 1, currentConnections = 0 },
            new ExtensionPoint { id = "core_ext_right", position = new Vector2Int(3, 6), availableBoards = new List<string>(), maxConnections = 1, currentConnections = 0 },
            new ExtensionPoint { id = "core_ext_bottom", position = new Vector2Int(6, 3), availableBoards = new List<string>(), maxConnections = 1, currentConnections = 0 }
        };
    }

    /// <summary>
    /// Set up node connections (adjacent nodes are connected)
    /// </summary>
    private static void SetupNodeConnections(PassiveBoard board)
    {
        var allNodes = board.GetAllNodes();
        
        foreach (var node in allNodes)
        {
            var connections = new List<string>();
            
            // Check all 8 adjacent positions
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue; // Skip self
                    
                    int newRow = node.position.x + dr;
                    int newCol = node.position.y + dc;
                    
                    // Check bounds
                    if (newRow >= 0 && newRow < board.size.x && newCol >= 0 && newCol < board.size.y)
                    {
                        var adjacentNode = board.GetNode(newRow, newCol);
                        if (adjacentNode != null)
                        {
                            connections.Add(adjacentNode.id);
                        }
                    }
                }
            }
            
            node.connections = connections;
        }
    }
}
