using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// JSON-based Board Data Manager for passive tree
    /// Much simpler and more efficient than ScriptableObject approach
    /// </summary>
    public class JsonBoardDataManager : MonoBehaviour
    {
        [Header("JSON Data Source")]
        [SerializeField] private TextAsset boardDataJson;
        
        [Header("Settings")]
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private bool debugMode = true;

        // Runtime data - much simpler structure
        private Dictionary<Vector2Int, JsonNodeData> nodeDataMap = new Dictionary<Vector2Int, JsonNodeData>();
        private Dictionary<Vector2Int, CellController> cellMap = new Dictionary<Vector2Int, CellController>();
        private bool isDataLoaded = false;

        // Events
        public System.Action<Vector2Int, JsonNodeData> OnNodeDataLoaded;
        public System.Action OnBoardDataLoaded;

        void Start()
        {
            if (autoLoadOnStart)
            {
                LoadBoardData();
            }
        }

        /// <summary>
        /// Load board data from JSON and assign to cells
        /// </summary>
        [ContextMenu("Load Board Data from JSON")]
        public void LoadBoardData()
        {
            PassiveTreeLogger.LogInfo("Loading board data from JSON...", "json");
            
            if (debugMode)
            {
                PassiveTreeLogger.LogCategory($"JSON file assigned: {boardDataJson != null}", "json");
                if (boardDataJson != null)
                {
                    PassiveTreeLogger.LogCategory($"JSON file: {boardDataJson.name} ({boardDataJson.text.Length} chars)", "json");
                }
                else
                {
                    PassiveTreeLogger.LogError("No JSON file assigned! Please assign CoreBoardData.json in the inspector.");
                }
            }

            // Clear existing data
            nodeDataMap.Clear();
            cellMap.Clear();

            // Find CellController components only on this board (children of this GameObject)
            CellController[] cells = GetComponentsInChildren<CellController>();
            PassiveTreeLogger.LogCategory($"Found {cells.Length} CellController components on board '{gameObject.name}'", "json");

            // Build cell map
            foreach (CellController cell in cells)
            {
                cellMap[cell.GridPosition] = cell;
                if (debugMode)
                {
                    PassiveTreeLogger.LogCategory($"Mapped cell at {cell.GridPosition}", "json");
                }
            }

            // Load data from JSON
            if (boardDataJson != null)
            {
                LoadDataFromJson();
            }
            else
            {
                Debug.LogWarning("[JsonBoardDataManager] No JSON file assigned!");
                return;
            }

            // Assign data to cells
            AssignDataToCells();

            isDataLoaded = true;
            OnBoardDataLoaded?.Invoke();

            PassiveTreeLogger.LogInfo($"Board data loading complete. {nodeDataMap.Count} nodes loaded.", "json");
            
            // Debug: Show all loaded positions
            if (debugMode)
            {
                Debug.Log($"[JsonBoardDataManager] Loaded positions:");
                foreach (var kvp in nodeDataMap)
                {
                    Debug.Log($"  - Position {kvp.Key}: '{kvp.Value.name}' ({kvp.Value.type})");
                }
            }
        }

        /// <summary>
        /// Load node data from JSON file
        /// </summary>
        private void LoadDataFromJson()
        {
            try
            {
                if (debugMode)
                {
                    Debug.Log($"[JsonBoardDataManager] Parsing JSON text...");
                    Debug.Log($"[JsonBoardDataManager] JSON text preview: {boardDataJson.text.Substring(0, Mathf.Min(200, boardDataJson.text.Length))}...");
                }
                
                // Parse JSON data using custom parser for jagged array structure
                var allNodes = ParseJaggedJsonNodes(boardDataJson.text);
                
                if (debugMode)
                {
                    Debug.Log($"[JsonBoardDataManager] JSON parsing result:");
                    Debug.Log($"  - allNodes: {allNodes != null}");
                    if (allNodes != null)
                    {
                        Debug.Log($"  - allNodes.Count: {allNodes.Count}");
                        for (int i = 0; i < Mathf.Min(3, allNodes.Count); i++)
                        {
                            var node = allNodes[i];
                            Debug.Log($"    - Node {i}: {node?.name ?? "null"} at ({node?.position.column ?? -1}, {node?.position.row ?? -1})");
                        }
                    }
                }
                
                if (allNodes != null)
                {
                    foreach (var node in allNodes)
                    {
                        if (node != null)
                        {
                            Vector2Int position = new Vector2Int(node.position.column, node.position.row);
                            nodeDataMap[position] = node;
                            
                            if (debugMode)
                            {
                                Debug.Log($"[JsonBoardDataManager] Loaded node: '{node.name}' at {position}");
                                Debug.Log($"  - Description: '{node.description}'");
                                Debug.Log($"  - Type: {node.type}");
                                Debug.Log($"  - Stats: {(node.stats != null ? "Available" : "None")}");
                                if (node.stats != null)
                                {
                                    // Log some key stats if they have values
                                    if (node.stats.strength != 0) Debug.Log($"    - Strength: {node.stats.strength}");
                                    if (node.stats.dexterity != 0) Debug.Log($"    - Dexterity: {node.stats.dexterity}");
                                    if (node.stats.intelligence != 0) Debug.Log($"    - Intelligence: {node.stats.intelligence}");
                                    if (node.stats.maxHealthIncrease != 0) Debug.Log($"    - Max Health: +{node.stats.maxHealthIncrease}");
                                    if (node.stats.armorIncrease != 0) Debug.Log($"    - Armor: +{node.stats.armorIncrease}");
                                }
                            }
                            
                            OnNodeDataLoaded?.Invoke(position, node);
                        }
                    }
                }
                
                // Parse extension points from JSON
                var extensionPoints = ParseExtensionPointsFromJson(boardDataJson.text);
                if (extensionPoints != null && extensionPoints.Count > 0)
                {
                    if (debugMode)
                    {
                        Debug.Log($"[JsonBoardDataManager] Loaded {extensionPoints.Count} extension points from JSON");
                    }
                    
                    // Process extension points to ensure they have proper node data
                    foreach (var extPoint in extensionPoints)
                    {
                        Vector2Int position = new Vector2Int(extPoint.position.column, extPoint.position.row);
                        
                        // Check if we already have node data for this position
                        if (!nodeDataMap.ContainsKey(position))
                        {
                            // Create a JsonNodeData for this extension point
                            var extensionNodeData = new JsonNodeData
                            {
                                id = extPoint.id,
                                name = "Extension Point",
                                description = "Connect to another board",
                                position = extPoint.position,
                                type = "extension",
                                stats = new JsonStats(),
                                maxRank = 1,
                                currentRank = 0,
                                cost = 0,
                                requirements = new string[0],
                                connections = new string[0]
                            };
                            
                            nodeDataMap[position] = extensionNodeData;
                            
                            if (debugMode)
                            {
                                Debug.Log($"[JsonBoardDataManager] Created extension point node data: '{extPoint.id}' at {position}");
                            }
                        }
                        else
                        {
                            // Ensure existing node data is marked as extension type
                            var existingNode = nodeDataMap[position];
                            if (existingNode.type != "extension")
                            {
                                if (debugMode)
                                {
                                    Debug.Log($"[JsonBoardDataManager] Converting existing node at {position} to extension type");
                                }
                                existingNode.type = "extension";
                            }
                        }
                    }
                }
                
                Debug.Log($"[JsonBoardDataManager] Loaded {nodeDataMap.Count} nodes from JSON");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JsonBoardDataManager] Failed to parse JSON: {e.Message}");
                Debug.LogError($"[JsonBoardDataManager] Stack trace: {e.StackTrace}");
                if (debugMode)
                {
                    Debug.LogError($"[JsonBoardDataManager] JSON text that failed to parse:");
                    Debug.LogError($"[JsonBoardDataManager] {boardDataJson.text}");
                }
            }
        }

        /// <summary>
        /// Assign loaded data to cells
        /// </summary>
        private void AssignDataToCells()
        {
            Debug.Log($"[JsonBoardDataManager] Assigning data to {nodeDataMap.Count} cells...");
            Debug.Log($"[JsonBoardDataManager] Available cells: {cellMap.Count}");
            
            if (debugMode)
            {
                Debug.Log($"[JsonBoardDataManager] Available cell positions:");
                foreach (var kvp in cellMap)
                {
                    Debug.Log($"  - Cell at {kvp.Key}: {kvp.Value?.name ?? "null"}");
                }
                
                Debug.Log($"[JsonBoardDataManager] Available node positions:");
                foreach (var kvp in nodeDataMap)
                {
                    Debug.Log($"  - Node at {kvp.Key}: {kvp.Value?.name ?? "null"}");
                }
            }
            
            foreach (var kvp in nodeDataMap)
            {
                Vector2Int position = kvp.Key;
                JsonNodeData nodeData = kvp.Value;
                
                if (debugMode)
                {
                    Debug.Log($"[JsonBoardDataManager] Processing node at {position}: {nodeData?.name ?? "null"}");
                }
                
                if (cellMap.ContainsKey(position))
                {
                    CellController cell = cellMap[position];
                    AssignNodeDataToCell(cell, nodeData);
                }
                else
                {
                    Debug.LogWarning($"[JsonBoardDataManager] No cell found at position {position} for node {nodeData?.name ?? "null"}");
                }
            }
        }

        /// <summary>
        /// Assign JSON node data to a cell
        /// </summary>
        private void AssignNodeDataToCell(CellController cell, JsonNodeData nodeData)
        {
            if (cell == null || nodeData == null) 
            {
                Debug.LogWarning($"[JsonBoardDataManager] AssignNodeDataToCell called with null cell or nodeData");
                return;
            }

            if (debugMode)
            {
                Debug.Log($"[JsonBoardDataManager] Assigning data to cell at {cell.GridPosition}:");
                Debug.Log($"  - nodeData.id: '{nodeData.id}'");
                Debug.Log($"  - nodeData.name: '{nodeData.name}'");
                Debug.Log($"  - nodeData.description: '{nodeData.description}'");
                Debug.Log($"  - nodeData.type: '{nodeData.type}'");
                Debug.Log($"  - nodeData.position: ({nodeData.position?.column ?? -1}, {nodeData.position?.row ?? -1})");
                Debug.Log($"  - nodeData.cost: {nodeData.cost}");
                Debug.Log($"  - nodeData.currentRank: {nodeData.currentRank}");
            }

            // Convert JSON node type to Unity NodeType
            NodeType nodeType = ConvertJsonNodeType(nodeData.type);
            
            if (debugMode)
            {
                Debug.Log($"[JsonBoardDataManager] Setting node type for cell at {cell.GridPosition}: '{nodeData.type}' -> {nodeType}");
            }
            
            // Set cell properties
            cell.SetNodeType(nodeType);
            cell.SetNodeDescription(nodeData.description);
            cell.SetNodeName(nodeData.name);
            cell.SetSkillPointsCost(nodeData.cost);
            cell.SetUnlocked(nodeData.currentRank > 0);
            cell.SetPurchased(nodeData.currentRank > 0);
            
            // Set stats if available
            if (nodeData.stats != null)
            {
                Dictionary<string, float> statsDict = ConvertStatsToDictionary(nodeData.stats);
                cell.SetNodeStats(statsDict);
            }

            if (debugMode)
            {
                Debug.Log($"[JsonBoardDataManager] Assigned {nodeData.name} to cell at {cell.GridPosition}");
            }
        }

        /// <summary>
        /// Convert JSON node type to Unity NodeType
        /// </summary>
        private NodeType ConvertJsonNodeType(string jsonType)
        {
            if (string.IsNullOrEmpty(jsonType))
            {
                // Don't log warning for every cell - this is expected for many cells
                return NodeType.Travel;
            }
            
            if (debugMode)
            {
                Debug.Log($"[JsonBoardDataManager] Converting node type: '{jsonType}' -> '{jsonType.ToLower()}'");
            }
            
            switch (jsonType.ToLower())
            {
                case "main": 
                    if (debugMode) Debug.Log($"[JsonBoardDataManager] Converted '{jsonType}' to NodeType.Start");
                    return NodeType.Start;
                case "extension": 
                    if (debugMode) Debug.Log($"[JsonBoardDataManager] Converted '{jsonType}' to NodeType.Extension");
                    return NodeType.Extension;
                case "notable": 
                    if (debugMode) Debug.Log($"[JsonBoardDataManager] Converted '{jsonType}' to NodeType.Notable");
                    return NodeType.Notable;
                case "small": 
                    if (debugMode) Debug.Log($"[JsonBoardDataManager] Converted '{jsonType}' to NodeType.Small");
                    return NodeType.Small;
                default: 
                    Debug.LogWarning($"[JsonBoardDataManager] Unknown node type '{jsonType}', defaulting to Travel");
                    return NodeType.Travel;
            }
        }

        /// <summary>
        /// Convert JSON stats to Dictionary
        /// </summary>
        private Dictionary<string, float> ConvertStatsToDictionary(JsonStats jsonStats)
        {
            Dictionary<string, float> statsDict = new Dictionary<string, float>();
            
            // Use reflection to get all properties from JsonStats
            var properties = typeof(JsonStats).GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float))
                {
                    object value = prop.GetValue(jsonStats);
                    if (value != null)
                    {
                        float floatValue = System.Convert.ToSingle(value);
                        if (floatValue != 0) // Only add non-zero values
                        {
                            // Map TypeScript stat names to Unity stat names
                            string unityStatName = PassiveTreeStatMapper.MapStatName(prop.Name);
                            statsDict[unityStatName] = floatValue;
                        }
                    }
                }
            }
            
            return statsDict;
        }

        /// <summary>
        /// Get node data for a specific position
        /// </summary>
        public JsonNodeData GetNodeData(Vector2Int position)
        {
            if (debugMode)
            {
                Debug.Log($"[JsonBoardDataManager] GetNodeData called for position {position}");
                Debug.Log($"[JsonBoardDataManager] nodeDataMap contains {nodeDataMap.Count} entries");
                Debug.Log($"[JsonBoardDataManager] Contains key {position}: {nodeDataMap.ContainsKey(position)}");
                
                if (!nodeDataMap.ContainsKey(position))
                {
                    Debug.LogWarning($"[JsonBoardDataManager] No data found for position {position}. Available positions:");
                    foreach (var kvp in nodeDataMap)
                    {
                        Debug.Log($"  - {kvp.Key}: '{kvp.Value.name}'");
                    }
                }
            }
            
            return nodeDataMap.ContainsKey(position) ? nodeDataMap[position] : null;
        }

        /// <summary>
        /// Get all node data
        /// </summary>
        public Dictionary<Vector2Int, JsonNodeData> GetAllNodeData()
        {
            return new Dictionary<Vector2Int, JsonNodeData>(nodeDataMap);
        }

        /// <summary>
        /// Check if data is loaded
        /// </summary>
        public bool IsDataLoaded => isDataLoaded;
        public TextAsset BoardDataJson => boardDataJson;

        /// <summary>
        /// Set the JSON data source
        /// </summary>
        public void SetJsonDataSource(TextAsset jsonFile)
        {
            boardDataJson = jsonFile;
        }

        /// <summary>
        /// Get the loaded board data (for compatibility)
        /// </summary>
        public JsonBoardData GetBoardData()
        {
            if (boardDataJson != null && isDataLoaded)
            {
                try
                {
                    return JsonUtility.FromJson<JsonBoardData>(boardDataJson.text);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[JsonBoardDataManager] Failed to get board data: {e.Message}");
                }
            }
            return null;
        }

        /// <summary>
        /// Parse JSON with jagged array structure for nodes
        /// </summary>
        private System.Collections.Generic.List<JsonNodeData> ParseJaggedJsonNodes(string jsonText)
        {
            try
            {
                var allNodes = new System.Collections.Generic.List<JsonNodeData>();
                
                // First, find the "nodes" array in the JSON
                int nodesStart = jsonText.IndexOf("\"nodes\"");
                if (nodesStart == -1)
                {
                    Debug.LogError("[JsonBoardDataManager] Could not find 'nodes' in JSON");
                    return null;
                }
                
                // Find the colon after "nodes"
                int colonIndex = jsonText.IndexOf(':', nodesStart);
                if (colonIndex == -1)
                {
                    Debug.LogError("[JsonBoardDataManager] Could not find colon after 'nodes'");
                    return null;
                }
                
                // Find the start of the actual array content (after the opening bracket)
                int arrayStart = jsonText.IndexOf('[', colonIndex);
                if (arrayStart == -1)
                {
                    Debug.LogError("[JsonBoardDataManager] Could not find opening bracket for nodes array");
                    return null;
                }
                
                Debug.Log($"[JsonBoardDataManager] Found nodes at position {nodesStart}, colon at {colonIndex}, array starts at {arrayStart}");
                
                // Now search within the nodes array for individual node objects
                int searchIndex = arrayStart + 1; // Start after the opening bracket
                int nodeCount = 0;
                int totalObjectsFound = 0;
                
                Debug.Log($"[JsonBoardDataManager] Starting to search for node objects from position {searchIndex}");
                
                while (searchIndex < jsonText.Length)
                {
                    // Look for the start of a node object within the array
                    int nodeStart = jsonText.IndexOf('{', searchIndex);
                    if (nodeStart == -1) break;
                    
                    totalObjectsFound++;
                    
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
                    
                    // Debug: Log what we found
                    if (debugMode)
                    {
                        Debug.Log($"[JsonBoardDataManager] Found object {totalObjectsFound}: {nodeJson.Substring(0, Mathf.Min(100, nodeJson.Length))}...");
                    }
                    
                    // Check if this looks like a valid node (contains "id" and position info)
                    if (nodeJson.Contains("\"id\"") && nodeJson.Contains("\"position\""))
                    {
                        try
                        {
                            JsonNodeData node = JsonUtility.FromJson<JsonNodeData>(nodeJson);
                            if (node != null && !string.IsNullOrEmpty(node.id))
                            {
                                allNodes.Add(node);
                                nodeCount++;
                                if (debugMode)
                                {
                                    Debug.Log($"[JsonBoardDataManager] Parsed node {nodeCount}: ID='{node.id}', Name='{node.name}'");
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"[JsonBoardDataManager] Failed to parse node JSON: {e.Message}");
                            if (debugMode)
                            {
                                Debug.LogWarning($"[JsonBoardDataManager] Failed JSON: {nodeJson}");
                            }
                        }
                    }
                    else
                    {
                        if (debugMode)
                        {
                            Debug.Log($"[JsonBoardDataManager] Skipping object {totalObjectsFound} - missing id or position: {nodeJson.Substring(0, Mathf.Min(50, nodeJson.Length))}...");
                        }
                    }
                    
                    searchIndex = nodeEnd + 1;
                }
                
                Debug.Log($"[JsonBoardDataManager] Parsing complete: Found {totalObjectsFound} total objects, successfully parsed {allNodes.Count} nodes from JSON");
                return allNodes;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JsonBoardDataManager] Error parsing JSON: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse extension points from JSON text
        /// </summary>
        private System.Collections.Generic.List<JsonExtensionPoint> ParseExtensionPointsFromJson(string jsonText)
        {
            try
            {
                var extensionPoints = new System.Collections.Generic.List<JsonExtensionPoint>();
                
                // First, find the "extensionPoints" array in the JSON
                int extensionPointsStart = jsonText.IndexOf("\"extensionPoints\"");
                if (extensionPointsStart == -1)
                {
                    if (debugMode)
                    {
                        Debug.Log($"[JsonBoardDataManager] No 'extensionPoints' found in JSON - this is normal for some boards");
                    }
                    return extensionPoints; // Return empty list, not null
                }
                
                // Find the colon after "extensionPoints"
                int colonIndex = jsonText.IndexOf(':', extensionPointsStart);
                if (colonIndex == -1)
                {
                    Debug.LogError("[JsonBoardDataManager] Could not find colon after 'extensionPoints'");
                    return extensionPoints;
                }
                
                // Find the start of the actual array content (after the opening bracket)
                int arrayStart = jsonText.IndexOf('[', colonIndex);
                if (arrayStart == -1)
                {
                    Debug.LogError("[JsonBoardDataManager] Could not find opening bracket for extensionPoints array");
                    return extensionPoints;
                }
                
                if (debugMode)
                {
                    Debug.Log($"[JsonBoardDataManager] Found extensionPoints at position {extensionPointsStart}, colon at {colonIndex}, array starts at {arrayStart}");
                }
                
                // Now search within the extensionPoints array for individual extension point objects
                int searchIndex = arrayStart + 1; // Start after the opening bracket
                int extPointCount = 0;
                int totalObjectsFound = 0;
                
                if (debugMode)
                {
                    Debug.Log($"[JsonBoardDataManager] Starting to search for extension point objects from position {searchIndex}");
                }
                
                while (searchIndex < jsonText.Length)
                {
                    // Look for the start of an extension point object within the array
                    int extPointStart = jsonText.IndexOf('{', searchIndex);
                    if (extPointStart == -1) break;
                    
                    totalObjectsFound++;
                    
                    // Find the matching closing brace
                    int braceCount = 0;
                    int extPointEnd = -1;
                    bool inString = false;
                    bool escaped = false;
                    
                    for (int i = extPointStart; i < jsonText.Length; i++)
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
                                    extPointEnd = i;
                                    break;
                                }
                            }
                        }
                        
                        if (escaped) escaped = false;
                    }
                    
                    if (extPointEnd == -1) break;
                    
                    // Extract the extension point JSON
                    string extPointJson = jsonText.Substring(extPointStart, extPointEnd - extPointStart + 1);
                    
                    // Debug: Log what we found
                    if (debugMode)
                    {
                        Debug.Log($"[JsonBoardDataManager] Found extension point object {totalObjectsFound}: {extPointJson.Substring(0, Mathf.Min(100, extPointJson.Length))}...");
                    }
                    
                    // Check if this looks like a valid extension point (contains "id" and position info)
                    if (extPointJson.Contains("\"id\"") && extPointJson.Contains("\"position\""))
                    {
                        try
                        {
                            // Parse the extension point JSON
                            JsonExtensionPoint extPoint = JsonUtility.FromJson<JsonExtensionPoint>(extPointJson);
                            if (extPoint != null && extPoint.position != null)
                            {
                                extensionPoints.Add(extPoint);
                                extPointCount++;
                                
                                if (debugMode)
                                {
                                    Debug.Log($"[JsonBoardDataManager] Successfully parsed extension point: '{extPoint.id}' at ({extPoint.position.column}, {extPoint.position.row})");
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"[JsonBoardDataManager] Failed to parse extension point JSON: {e.Message}");
                            if (debugMode)
                            {
                                Debug.LogError($"[JsonBoardDataManager] Extension point JSON that failed: {extPointJson}");
                            }
                        }
                    }
                    
                    searchIndex = extPointEnd + 1;
                }
                
                if (debugMode)
                {
                    Debug.Log($"[JsonBoardDataManager] Extension points parsing complete: Found {totalObjectsFound} total objects, successfully parsed {extensionPoints.Count} extension points from JSON");
                }
                return extensionPoints;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JsonBoardDataManager] Error parsing extension points from JSON: {e.Message}");
                return new System.Collections.Generic.List<JsonExtensionPoint>(); // Return empty list, not null
            }
        }
    }

    /// <summary>
    /// JSON data structures matching TypeScript format
    /// </summary>
    [System.Serializable]
    public class JsonBoardData
    {
        public string id;
        public string name;
        public string description;
        public string theme;
        public JsonSize size;
        public JsonNodeData[] nodes;
        public JsonExtensionPoint[] extensionPoints;
        public int maxPoints;
    }

    [System.Serializable]
    public class JsonSize
    {
        public int rows;
        public int columns;
    }

    [System.Serializable]
    public class JsonNodeData
    {
        public string id;
        public string name;
        public string description;
        public JsonPosition position;
        public string type;
        public JsonStats stats;
        public int maxRank;
        public int currentRank;
        public int cost;
        public string[] requirements;
        public string[] connections;
    }

    [System.Serializable]
    public class JsonPosition
    {
        public int row;
        public int column;
    }

    [System.Serializable]
    public class JsonStats
    {
        // Core attributes
        public int strength;
        public int dexterity;
        public int intelligence;
        
        // Health and resources
        public int maxHealthIncrease;
        public int maxEnergyShieldIncrease;
        public int maxEnergyShield;
        
        // Combat stats
        public int armor;
        public int armorIncrease;
        public int evasion;
        public int increasedEvasion;
        public int elementalResist;
        public int accuracy;
        
        // Damage stats
        public int spellPowerIncrease;
        public int increasedProjectileDamage;
        
        // Fire-specific stats
        public int fireIncrease;
        public int fire;
        public int chanceToIgnite;
        public int addedPhysicalAsFire;
        public int increasedIgniteMagnitude;
        public int addedFireAsCold;
        
        // Add more stats as needed from TypeScript
    }

    [System.Serializable]
    public class JsonExtensionPoint
    {
        public string id;
        public JsonPosition position;
        public string[] availableBoards;
        public int maxConnections;
        public int currentConnections;
    }
}
