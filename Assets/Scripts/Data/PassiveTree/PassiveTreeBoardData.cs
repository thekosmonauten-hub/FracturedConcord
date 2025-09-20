using UnityEngine;
using System.Collections.Generic;
using System;
using PassiveTree;

/// <summary>
/// Master ScriptableObject for generating and managing all passive tree cell data
/// This single asset can generate data for all cells in the passive tree
/// </summary>
[CreateAssetMenu(fileName = "PassiveTreeBoardData", menuName = "Passive Tree/Board Data")]
public class PassiveTreeBoardData : ScriptableObject
{
    [Header("Board Configuration")]
    [SerializeField] private Vector2Int boardSize = new Vector2Int(7, 7);
    [SerializeField] private string boardName = "Core Passive Tree";
    [SerializeField] private string boardDescription = "The main passive tree board";

    [Header("Node Type Definitions")]
    [SerializeField] private NodeTypeData startNodeData;
    [SerializeField] private NodeTypeData travelNodeData;
    [SerializeField] private NodeTypeData smallNodeData;
    [SerializeField] private NodeTypeData notableNodeData;
    [SerializeField] private NodeTypeData extensionNodeData;
    [SerializeField] private NodeTypeData keystoneNodeData;

    [Header("Special Positions")]
    [SerializeField] private Vector2Int startNodePosition = new Vector2Int(3, 3);
    [SerializeField] private List<Vector2Int> extensionPositions = new List<Vector2Int>
    {
        new Vector2Int(0, 3), // South
        new Vector2Int(3, 0), // West
        new Vector2Int(3, 6), // East
        new Vector2Int(6, 3)  // North
    };
    [SerializeField] private List<Vector2Int> travelPositions = new List<Vector2Int>
    {
        new Vector2Int(1, 3), new Vector2Int(2, 3), // South path
        new Vector2Int(3, 1), new Vector2Int(3, 2), // West path
        new Vector2Int(3, 4), new Vector2Int(3, 5), // East path
        new Vector2Int(4, 3), new Vector2Int(5, 3)  // North path
    };
    [SerializeField] private List<Vector2Int> notablePositions = new List<Vector2Int>
    {
        new Vector2Int(1, 1), new Vector2Int(1, 5),
        new Vector2Int(5, 1), new Vector2Int(5, 5)
    };

    [Header("Manual Overrides")]
    [SerializeField] private List<CellDataOverride> manualOverrides = new List<CellDataOverride>();

    [Header("Generation Settings")]
    [SerializeField] private bool autoGenerateOnAwake = true;
    [SerializeField] private bool useRandomDescriptions = false;
    [SerializeField] private bool generateUniqueNames = true;

    // Runtime data
    private Dictionary<Vector2Int, PassiveNodeData> generatedCellData;
    private bool isDataGenerated = false;

    /// <summary>
    /// Get cell data for a specific position
    /// </summary>
    public PassiveNodeData GetCellData(Vector2Int position)
    {
        if (!isDataGenerated)
        {
            GenerateAllCellData();
        }

        if (generatedCellData.TryGetValue(position, out PassiveNodeData data))
        {
            return data;
        }

        return null;
    }

    /// <summary>
    /// Get all generated cell data
    /// </summary>
    public Dictionary<Vector2Int, PassiveNodeData> GetAllCellData()
    {
        if (!isDataGenerated)
        {
            GenerateAllCellData();
        }

        return new Dictionary<Vector2Int, PassiveNodeData>(generatedCellData);
    }

    /// <summary>
    /// Generate all cell data for the board
    /// </summary>
    public void GenerateAllCellData()
    {
        generatedCellData = new Dictionary<Vector2Int, PassiveNodeData>();

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                PassiveNodeData cellData = GenerateCellDataForPosition(position);
                generatedCellData[position] = cellData;
            }
        }

        isDataGenerated = true;
        Debug.Log($"[PassiveTreeBoardData] Generated data for {generatedCellData.Count} cells");
    }

    /// <summary>
    /// Generate cell data for a specific position
    /// </summary>
    private PassiveNodeData GenerateCellDataForPosition(Vector2Int position)
    {
        // Check for manual override first
        CellDataOverride manualOverride = GetManualOverride(position);
        if (manualOverride != null)
        {
            return CreateNodeDataFromOverride(position, manualOverride);
        }

        // Determine node type based on position
        NodeType nodeType = DetermineNodeType(position);
        NodeTypeData typeData = GetNodeTypeData(nodeType);

        // Create the node data
        PassiveNodeData nodeData = ScriptableObject.CreateInstance<PassiveNodeData>();
        
        // Set basic properties
        SetNodeDataField(nodeData, "_nodeName", GenerateNodeName(position, nodeType));
        SetNodeDataField(nodeData, "_description", GenerateNodeDescription(position, nodeType, typeData));
        SetNodeDataField(nodeData, "_nodeType", nodeType);
        SetNodeDataField(nodeData, "_skillPointsCost", typeData.cost);
        SetNodeDataField(nodeData, "_isUnlocked", nodeType == NodeType.Start);
        SetNodeDataField(nodeData, "_nodeSprite", typeData.sprite);

        // Set colors
        SetNodeDataField(nodeData, "_nodeColor", typeData.normalColor);
        SetNodeDataField(nodeData, "_highlightColor", typeData.hoverColor);
        SetNodeDataField(nodeData, "_selectedColor", typeData.selectedColor);

        // Set stats
        SetNodeDataField(nodeData, "_stats", typeData.stats);

        return nodeData;
    }

    /// <summary>
    /// Determine the node type for a given position
    /// </summary>
    private NodeType DetermineNodeType(Vector2Int position)
    {
        if (position == startNodePosition)
            return NodeType.Start;
        
        if (extensionPositions.Contains(position))
            return NodeType.Extension;
        
        if (travelPositions.Contains(position))
            return NodeType.Travel;
        
        if (notablePositions.Contains(position))
            return NodeType.Notable;
        
        return NodeType.Small;
    }

    /// <summary>
    /// Get node type data for a specific node type
    /// </summary>
    private NodeTypeData GetNodeTypeData(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return startNodeData ?? GetDefaultNodeTypeData(NodeType.Start);
            case NodeType.Travel:
                return travelNodeData ?? GetDefaultNodeTypeData(NodeType.Travel);
            case NodeType.Small:
                return smallNodeData ?? GetDefaultNodeTypeData(NodeType.Small);
            case NodeType.Notable:
                return notableNodeData ?? GetDefaultNodeTypeData(NodeType.Notable);
            case NodeType.Extension:
                return extensionNodeData ?? GetDefaultNodeTypeData(NodeType.Extension);
            case NodeType.Keystone:
                return keystoneNodeData ?? GetDefaultNodeTypeData(NodeType.Keystone);
            default:
                return GetDefaultNodeTypeData(NodeType.Small);
        }
    }

    /// <summary>
    /// Get default node type data if none is assigned
    /// </summary>
    private NodeTypeData GetDefaultNodeTypeData(NodeType nodeType)
    {
        NodeTypeData defaultData = new NodeTypeData();
        defaultData.nodeType = nodeType;
        defaultData.cost = GetDefaultCost(nodeType);
        defaultData.normalColor = GetDefaultColor(nodeType);
        defaultData.hoverColor = Color.yellow;
        defaultData.selectedColor = Color.cyan;
        defaultData.sprite = null; // Will use auto-assignment
        defaultData.stats = new Dictionary<string, float>();
        return defaultData;
    }

    /// <summary>
    /// Generate a node name for a position and type
    /// </summary>
    private string GenerateNodeName(Vector2Int position, NodeType nodeType)
    {
        if (!generateUniqueNames)
        {
            return nodeType.ToString();
        }

        switch (nodeType)
        {
            case NodeType.Start:
                return "Starting Point";
            case NodeType.Extension:
                return $"Extension Point ({GetDirectionFromPosition(position)})";
            case NodeType.Notable:
                return $"Notable ({position.x},{position.y})";
            case NodeType.Travel:
                return $"Travel Node ({position.x},{position.y})";
            case NodeType.Small:
                return $"Small Node ({position.x},{position.y})";
            case NodeType.Keystone:
                return $"Keystone ({position.x},{position.y})";
            default:
                return $"{nodeType} ({position.x},{position.y})";
        }
    }

    /// <summary>
    /// Generate a node description for a position and type
    /// </summary>
    private string GenerateNodeDescription(Vector2Int position, NodeType nodeType, NodeTypeData typeData)
    {
        if (!string.IsNullOrEmpty(typeData.description))
        {
            return typeData.description;
        }

        if (useRandomDescriptions)
        {
            return GetRandomDescription(nodeType);
        }

        return GetDefaultDescription(nodeType, position);
    }

    /// <summary>
    /// Get direction string from extension position
    /// </summary>
    private string GetDirectionFromPosition(Vector2Int position)
    {
        if (position == new Vector2Int(0, 3)) return "South";
        if (position == new Vector2Int(3, 0)) return "West";
        if (position == new Vector2Int(3, 6)) return "East";
        if (position == new Vector2Int(6, 3)) return "North";
        return "Unknown";
    }

    /// <summary>
    /// Get default cost for node type
    /// </summary>
    private int GetDefaultCost(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
            case NodeType.Extension:
                return 0;
            case NodeType.Travel:
            case NodeType.Small:
                return 1;
            case NodeType.Notable:
                return 2;
            case NodeType.Keystone:
                return 1;
            default:
                return 1;
        }
    }

    /// <summary>
    /// Get default color for node type
    /// </summary>
    private Color GetDefaultColor(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return Color.green;
            case NodeType.Travel:
                return Color.white;
            case NodeType.Small:
                return Color.gray;
            case NodeType.Notable:
                return Color.blue;
            case NodeType.Extension:
                return Color.magenta;
            case NodeType.Keystone:
                return Color.red;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Get default description for node type
    /// </summary>
    private string GetDefaultDescription(NodeType nodeType, Vector2Int position)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return "Your starting point in the passive tree. This node is automatically allocated.";
            case NodeType.Extension:
                return $"Extension point for connecting other passive tree boards. Direction: {GetDirectionFromPosition(position)}";
            case NodeType.Notable:
                return "A notable passive with significant effects. These nodes provide powerful bonuses.";
            case NodeType.Travel:
                return "A basic travel node that connects different parts of the passive tree.";
            case NodeType.Small:
                return "A small passive node providing minor stat bonuses.";
            case NodeType.Keystone:
                return "A powerful keystone passive that fundamentally changes how your character works.";
            default:
                return "A passive tree node.";
        }
    }

    /// <summary>
    /// Get random description for node type
    /// </summary>
    private string GetRandomDescription(NodeType nodeType)
    {
        // This could be expanded with arrays of random descriptions
        return GetDefaultDescription(nodeType, Vector2Int.zero);
    }

    /// <summary>
    /// Get manual override for a position
    /// </summary>
    private CellDataOverride GetManualOverride(Vector2Int position)
    {
        foreach (var overrideData in manualOverrides)
        {
            if (overrideData.position == position)
            {
                return overrideData;
            }
        }
        return null;
    }

    /// <summary>
    /// Create node data from manual override
    /// </summary>
    private PassiveNodeData CreateNodeDataFromOverride(Vector2Int position, CellDataOverride overrideData)
    {
        PassiveNodeData nodeData = ScriptableObject.CreateInstance<PassiveNodeData>();
        
        SetNodeDataField(nodeData, "_nodeName", overrideData.nodeName);
        SetNodeDataField(nodeData, "_description", overrideData.description);
        SetNodeDataField(nodeData, "_nodeType", overrideData.nodeType);
        SetNodeDataField(nodeData, "_skillPointsCost", overrideData.cost);
        SetNodeDataField(nodeData, "_isUnlocked", overrideData.isUnlocked);
        SetNodeDataField(nodeData, "_nodeSprite", overrideData.sprite);
        SetNodeDataField(nodeData, "_nodeColor", overrideData.normalColor);
        SetNodeDataField(nodeData, "_highlightColor", overrideData.hoverColor);
        SetNodeDataField(nodeData, "_selectedColor", overrideData.selectedColor);
        SetNodeDataField(nodeData, "_stats", overrideData.stats);

        return nodeData;
    }

    /// <summary>
    /// Set a field on PassiveNodeData using reflection
    /// </summary>
    private void SetNodeDataField(PassiveNodeData nodeData, string fieldName, object value)
    {
        var field = typeof(PassiveNodeData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(nodeData, value);
        }
    }

    /// <summary>
    /// Add a manual override for a specific cell
    /// </summary>
    public void AddManualOverride(Vector2Int position, CellDataOverride overrideData)
    {
        // Remove existing override for this position
        manualOverrides.RemoveAll(o => o.position == position);
        
        // Add new override
        overrideData.position = position;
        manualOverrides.Add(overrideData);
        
        // Regenerate data
        isDataGenerated = false;
    }

    /// <summary>
    /// Remove manual override for a specific cell
    /// </summary>
    public void RemoveManualOverride(Vector2Int position)
    {
        manualOverrides.RemoveAll(o => o.position == position);
        isDataGenerated = false;
    }

    /// <summary>
    /// Clear all manual overrides
    /// </summary>
    public void ClearAllManualOverrides()
    {
        manualOverrides.Clear();
        isDataGenerated = false;
    }

    /// <summary>
    /// Force regeneration of all cell data
    /// </summary>
    [ContextMenu("Regenerate All Cell Data")]
    public void RegenerateAllCellData()
    {
        isDataGenerated = false;
        GenerateAllCellData();
    }

    /// <summary>
    /// Get board information
    /// </summary>
    public BoardInfo GetBoardInfo()
    {
        return new BoardInfo
        {
            boardName = boardName,
            boardDescription = boardDescription,
            boardSize = boardSize,
            startNodePosition = startNodePosition,
            extensionPositions = new List<Vector2Int>(extensionPositions),
            travelPositions = new List<Vector2Int>(travelPositions),
            notablePositions = new List<Vector2Int>(notablePositions)
        };
    }
}

/// <summary>
/// Data structure for defining node type properties
/// </summary>
[System.Serializable]
public class NodeTypeData
{
    public NodeType nodeType;
    public string description;
    public int cost;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.cyan;
    public Sprite sprite;
    public Dictionary<string, float> stats = new Dictionary<string, float>();
}

/// <summary>
/// Manual override for specific cell data
/// </summary>
[System.Serializable]
public class CellDataOverride
{
    [HideInInspector] public Vector2Int position;
    public string nodeName;
    [TextArea(3, 5)] public string description;
    public NodeType nodeType;
    public int cost;
    public bool isUnlocked;
    public Sprite sprite;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.cyan;
    public Dictionary<string, float> stats = new Dictionary<string, float>();
}

/// <summary>
/// Board information structure
/// </summary>
[System.Serializable]
public class BoardInfo
{
    public string boardName;
    public string boardDescription;
    public Vector2Int boardSize;
    public Vector2Int startNodePosition;
    public List<Vector2Int> extensionPositions;
    public List<Vector2Int> travelPositions;
    public List<Vector2Int> notablePositions;
}
