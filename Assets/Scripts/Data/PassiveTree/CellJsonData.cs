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
        nodePosition = new Vector2Int(jsonData.position.column, jsonData.position.row);
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
            position = new JsonPosition { row = nodePosition.y, column = nodePosition.x },
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
                // Add stats to description
                var statsList = new System.Collections.Generic.List<string>();
                
                if (nodeStats.strength != 0) statsList.Add($"+{nodeStats.strength} Strength");
                if (nodeStats.dexterity != 0) statsList.Add($"+{nodeStats.dexterity} Dexterity");
                if (nodeStats.intelligence != 0) statsList.Add($"+{nodeStats.intelligence} Intelligence");
                if (nodeStats.maxHealthIncrease != 0) statsList.Add($"+{nodeStats.maxHealthIncrease} Max Health");
                if (nodeStats.maxEnergyShieldIncrease != 0) statsList.Add($"+{nodeStats.maxEnergyShieldIncrease} Max Energy Shield");
                if (nodeStats.armorIncrease != 0) statsList.Add($"+{nodeStats.armorIncrease} Armor");
                if (nodeStats.increasedEvasion != 0) statsList.Add($"+{nodeStats.increasedEvasion} Evasion");
                if (nodeStats.accuracy != 0) statsList.Add($"+{nodeStats.accuracy} Accuracy");
                if (nodeStats.spellPowerIncrease != 0) statsList.Add($"+{nodeStats.spellPowerIncrease} Spell Power");
                if (nodeStats.increasedProjectileDamage != 0) statsList.Add($"+{nodeStats.increasedProjectileDamage} Projectile Damage");
                if (nodeStats.elementalResist != 0) statsList.Add($"+{nodeStats.elementalResist} Elemental Resistance");

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
        foreach (var node in allNodes)
        {
            if (node != null && node.position.column == position.x && node.position.row == position.y)
            {
                return node;
            }
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

    #endregion
}
