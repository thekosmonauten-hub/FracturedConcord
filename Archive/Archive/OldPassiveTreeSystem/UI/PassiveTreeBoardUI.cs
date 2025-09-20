using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PassiveTree;
using TMPro;
using System.Linq;

/// <summary>
/// UI component for displaying a passive tree board visually
/// Handles node positioning, visual states, and interactions
/// </summary>
public class PassiveTreeBoardUI : MonoBehaviour
{
    [Header("Board Configuration")]
    [SerializeField] private PassiveBoard boardData;
    [SerializeField] private Vector2 nodeSpacing = new Vector2(80f, 80f);
    [SerializeField] private Vector2 boardOffset = Vector2.zero;
    
    [Header("UI References")]
    [SerializeField] private RectTransform boardContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject cellPrefab; // New CellPrefab for cell backgrounds
    [SerializeField] private GameObject connectionLinePrefab;
    
    [Header("Sprite Management")]
    [SerializeField] private PassiveTreeSpriteManager spriteManager;
    [SerializeField] private bool useCustomSprites = true;
    
    [Header("Cell Settings")]
    [SerializeField] private float cellSize = 60f; // Configurable cell size
    [SerializeField] private bool autoCalculateCellSize = false; // Auto-calculate based on container (disabled by default)
    [SerializeField] private float cellPadding = 20f; // Padding around the grid
    
    // Public properties for debugging
    public PassiveBoard BoardData => boardData;
    public GameObject NodePrefab => nodePrefab;
    public GameObject ConnectionLinePrefab => connectionLinePrefab;
    
    /// <summary>
    /// Get the sprite manager for this board
    /// </summary>
    public PassiveTreeSpriteManager GetSpriteManager()
    {
        if (spriteManager == null)
        {
            Debug.LogWarning($"[PassiveTreeBoardUI] Sprite manager is NULL for board '{boardData?.name}'");
        }
        else
        {
    
        }
        return spriteManager;
    }
    
    [Header("Visual Settings")]
    [SerializeField] private Color allocatedNodeColor = Color.green;
    [SerializeField] private Color unallocatedNodeColor = Color.gray;
    [SerializeField] private Color unavailableNodeColor = Color.red;
    [SerializeField] private Color connectionLineColor = Color.white;
    [SerializeField] private float connectionLineWidth = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Runtime data
    private Dictionary<string, PassiveTreeNodeUI> nodeUIs = new Dictionary<string, PassiveTreeNodeUI>();
    private List<GameObject> connectionLines = new List<GameObject>();
    private PassiveTreeManager treeManager;
    
    private void Awake()
    {
        // Get or create board container
        if (boardContainer == null)
        {
            var containerGO = new GameObject("BoardContainer");
            containerGO.transform.SetParent(transform);
            boardContainer = containerGO.AddComponent<RectTransform>();
            boardContainer.anchorMin = Vector2.zero;
            boardContainer.anchorMax = Vector2.one;
            boardContainer.offsetMin = Vector2.zero;
            boardContainer.offsetMax = Vector2.zero;
            
            // Ensure the container is properly set up
    
        }
        else
        {
    
        }
        
        // Get tree manager
        treeManager = PassiveTreeManager.Instance;
    }
    
    private void Start()
    {
        // Always try to auto-initialize, even if boardData is set
        // This ensures the board loads properly when entering the scene
        StartCoroutine(AutoInitializeBoard());
    }
    
    /// <summary>
    /// Auto-initialize the board when entering the scene
    /// </summary>
    private System.Collections.IEnumerator AutoInitializeBoard()
    {
        // Wait a frame to ensure everything is loaded
        yield return null;
        
        // Try to get board data from manager
        if (treeManager == null)
        {
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
        }
        
        // Subscribe to events if we have a manager
        if (treeManager != null)
        {
            treeManager.OnNodeAllocated += OnNodeAllocated;
            treeManager.OnNodeDeallocated += OnNodeDeallocated;
            treeManager.OnStatsRecalculated += OnStatsRecalculated;
        }
        
        // Wait a bit more to ensure the manager is fully initialized
        yield return new WaitForSeconds(0.1f);
        
        // Try to get board data from manager
        if (treeManager != null && treeManager.PassiveTree != null && treeManager.PassiveTree.coreBoard != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("[PassiveTreeBoardUI] Auto-initializing board from manager...");
            }
            
            SetBoardData(treeManager.PassiveTree.coreBoard);
        }
        else if (boardData != null)
        {
            // If we already have board data, just create the visual
            if (showDebugInfo)
            {
                Debug.Log("[PassiveTreeBoardUI] Creating visual with existing board data...");
            }
            CreateBoardVisual();
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("[PassiveTreeBoardUI] Could not auto-initialize board - no manager or core board found");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (treeManager != null)
        {
            // Unsubscribe from events
            treeManager.OnNodeAllocated -= OnNodeAllocated;
            treeManager.OnNodeDeallocated -= OnNodeDeallocated;
            treeManager.OnStatsRecalculated -= OnStatsRecalculated;
        }
    }
    
    /// <summary>
    /// Set the board data and create the visual representation
    /// </summary>
    public void SetBoardData(PassiveBoard board)
    {
        boardData = board;
        CreateBoardVisual();
    }
    
    /// <summary>
    /// Create the visual representation of the board
    /// </summary>
    public void CreateBoardVisual()
    {
        if (boardData == null)
        {
            Debug.LogError("[PassiveTreeBoardUI] No board data assigned!");
            return;
        }
        
        ClearBoardVisual();
        
        // Create cells first (if using CellPrefab)
        if (cellPrefab != null)
        {
            CreateCells();
        }
        
        // Create nodes
        var allNodes = boardData.GetAllNodes();
        foreach (var node in allNodes)
        {
            CreateNodeUI(node);
        }
        
        // Create connections
        CreateConnectionLines();
        
        // Update board background sprite
        UpdateBoardBackgroundSprite();
        
        // Update visual states
        UpdateAllNodeStates();
        
        // Update connection lines based on initial allocation state
        UpdateConnectionLines();
        
        if (showDebugInfo)
        {
    
        }
    }
    
    /// <summary>
    /// Create cells for the entire board grid
    /// </summary>
    private void CreateCells()
    {
        if (cellPrefab == null || boardData == null) return;
        

        
        for (int row = 0; row < boardData.size.x; row++)
        {
            for (int col = 0; col < boardData.size.y; col++)
            {
                CreateCell(new Vector2Int(row, col));
            }
        }
    }
    
    /// <summary>
    /// Create a single cell at the specified grid position
    /// </summary>
    private void CreateCell(Vector2Int position)
    {
        if (cellPrefab == null) return;
        
        // Get the effective cell size
        float effectiveCellSize = GetEffectiveCellSize();
        
        // Calculate cell position (edge-to-edge, not overlapping)
        Vector2 cellPos = CalculateCellPosition(position);
        
        // Instantiate cell
        GameObject cellGO = Instantiate(cellPrefab, boardContainer);
        cellGO.name = $"Cell_{position.x}_{position.y}";
        
        // Set position and size
        RectTransform cellRect = cellGO.GetComponent<RectTransform>();
        if (cellRect != null)
        {
            cellRect.anchoredPosition = cellPos;
            
            // Set the cell size to match the calculated size
            cellRect.sizeDelta = new Vector2(effectiveCellSize, effectiveCellSize);
        }
        
        // Initialize cell UI component
        PassiveTreeCellUI cellUI = cellGO.GetComponent<PassiveTreeCellUI>();
        if (cellUI != null)
        {
            cellUI.InitializeCell(position, boardData.theme, spriteManager);
            cellUI.SetUseCustomCellSprites(useCustomSprites);
        }
        else
        {
            Debug.LogWarning($"[PassiveTreeBoardUI] CellPrefab missing PassiveTreeCellUI component at position {position}");
        }
        

    }
    
    /// <summary>
    /// Calculate cell position for edge-to-edge placement
    /// </summary>
    private Vector2 CalculateCellPosition(Vector2Int gridPosition)
    {
        // Get the effective cell size
        float effectiveCellSize = GetEffectiveCellSize();
        
        // Calculate position with no spacing (edge-to-edge)
        float x = (gridPosition.y - boardData.size.y / 2f) * effectiveCellSize;
        float y = (boardData.size.x / 2f - gridPosition.x) * effectiveCellSize;
        
        // Apply board offset
        x += boardOffset.x;
        y += boardOffset.y;
        
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// Get the effective cell size based on settings
    /// </summary>
    private float GetEffectiveCellSize()
    {
        // Get the node size to base cell size on
        float nodeSize = GetNodeSize();
        float cellPadding = 10f; // Padding around the node
        
        // If auto-calculate is enabled, calculate based on container size
        if (autoCalculateCellSize && boardContainer != null && boardData != null)
        {
            float containerWidth = boardContainer.rect.width;
            float containerHeight = boardContainer.rect.height;
            
            // Calculate cell size to fit the grid with some padding
            float availableWidth = containerWidth - (this.cellPadding * 2);
            float availableHeight = containerHeight - (this.cellPadding * 2);
            
            // Calculate cell size to fit the grid
            float widthPerCell = availableWidth / boardData.size.y;
            float heightPerCell = availableHeight / boardData.size.x;
            
            // Use the smaller dimension to ensure cells fit
            float autoCellSize = Mathf.Min(widthPerCell, heightPerCell);
            
            // Clamp to reasonable bounds
            autoCellSize = Mathf.Clamp(autoCellSize, 30f, 200f);
            
            Debug.Log($"[PassiveTreeBoardUI] Auto-calculated cell size: {autoCellSize} (container: {containerWidth}x{containerHeight}, available: {availableWidth}x{availableHeight}, grid: {boardData.size})");
            return autoCellSize;
        }
        
        // Otherwise, calculate cell size based on node size
        float calculatedCellSize = nodeSize + (cellPadding * 2);
        
        // Use the configured cell size if it's larger than the calculated size
        float finalCellSize = Mathf.Max(calculatedCellSize, cellSize);
        
        Debug.Log($"[PassiveTreeBoardUI] Cell size: {finalCellSize} (node size: {nodeSize}, padding: {cellPadding}, configured: {cellSize})");
        return finalCellSize;
    }
    
    /// <summary>
    /// Get the size of the node prefab
    /// </summary>
    private float GetNodeSize()
    {
        if (nodePrefab != null)
        {
            RectTransform nodeRect = nodePrefab.GetComponent<RectTransform>();
            if (nodeRect != null)
            {
                float nodeSize = Mathf.Max(nodeRect.rect.width, nodeRect.rect.height);
                if (nodeSize > 0)
                {
                    return nodeSize;
                }
            }
        }
        
        // Default node size if prefab not found
        return 40f;
    }
    
    /// <summary>
    /// Create a visual node UI
    /// </summary>
    private void CreateNodeUI(PassiveNode node)
    {
        if (nodePrefab == null)
        {
            Debug.LogError("[PassiveTreeBoardUI] Node prefab not assigned!");
            return;
        }
        
        // Ensure boardContainer exists
        if (boardContainer == null)
        {
            Debug.LogError("[PassiveTreeBoardUI] boardContainer is null! Cannot create node.");
            return;
        }
        
        // Create node GameObject as child of boardContainer
        var nodeGO = Instantiate(nodePrefab, boardContainer);
        var nodeUI = nodeGO.GetComponent<PassiveTreeNodeUI>();
        
        if (nodeUI == null)
        {
            Debug.LogError("[PassiveTreeBoardUI] Node prefab must have PassiveTreeNodeUI component!");
            DestroyImmediate(nodeGO);
            return;
        }
        
        // Position the node
        var nodeRect = nodeGO.GetComponent<RectTransform>();
        var position = GetNodePosition(node.position);
        nodeRect.anchoredPosition = position;
        
        // Initialize the node UI
        nodeUI.Initialize(node, this);
        nodeUIs[node.id] = nodeUI;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreeBoardUI] Created node UI for '{node.name}' at position {position} as child of {boardContainer.name}");
        }
    }
    
    /// <summary>
    /// Update the board background sprite based on theme
    /// </summary>
    private void UpdateBoardBackgroundSprite()
    {
        if (boardData == null || !useCustomSprites || spriteManager == null) return;
        
        // Get the board container sprite from the board data
        Sprite boardSprite = boardData.GetBoardContainerSprite();
        if (boardSprite != null)
        {
            // Find the board background image component
            var boardBackgroundImage = boardContainer?.GetComponent<Image>();
            if (boardBackgroundImage != null)
            {
                boardBackgroundImage.sprite = boardSprite;
                Debug.Log($"[PassiveTreeBoardUI] Updated board background sprite for '{boardData.name}' to {boardSprite.name}");
            }
        }
    }
    
    /// <summary>
    /// Get the UI position for a grid position
    /// </summary>
    private Vector2 GetNodePosition(Vector2Int gridPos)
    {
        // Center the grid around (0,0) for a 7x7 board
        // Grid positions: (0,0) to (6,6)
        // Center should be at (3,3)
        var centeredX = (gridPos.x - 3) * nodeSpacing.x;
        var centeredY = (gridPos.y - 3) * nodeSpacing.y;
        
        var position = new Vector2(centeredX, centeredY) + boardOffset;
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreeBoardUI] Node at grid {gridPos} positioned at {position} (centered: {centeredX}, {centeredY}, offset: {boardOffset})");
        }
        
        return position;
    }
    
    /// <summary>
    /// Create connection lines between connected nodes
    /// </summary>
    private void CreateConnectionLines()
    {
        if (connectionLinePrefab == null)
        {
            Debug.LogWarning("[PassiveTreeBoardUI] Connection line prefab not assigned - skipping connections");
            return;
        }
        
        var allNodes = boardData.GetAllNodes();
        
        foreach (var node in allNodes)
        {
            foreach (var connectionId in node.connections)
            {
                var connectedNode = boardData.GetAllNodes().Find(n => n.id == connectionId);
                if (connectedNode != null)
                {
                    CreateConnectionLine(node, connectedNode);
                }
            }
        }
    }
    
    /// <summary>
    /// Create a connection line between two nodes
    /// </summary>
    private void CreateConnectionLine(PassiveNode node1, PassiveNode node2)
    {
        // Check if we already have this connection
        var connectionKey = GetConnectionKey(node1.id, node2.id);
        if (connectionLines.Exists(line => line.name == connectionKey))
        {
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeBoardUI] Connection line {connectionKey} already exists, skipping");
            }
            return;
        }
        
        // Ensure boardContainer exists
        if (boardContainer == null)
        {
            Debug.LogWarning("[PassiveTreeBoardUI] boardContainer is null, skipping connection line creation");
            return;
        }
        
        // Skip if no connection line prefab
        if (connectionLinePrefab == null)
        {
            Debug.LogWarning("[PassiveTreeBoardUI] Connection line prefab is null, skipping connection line creation");
            return;
        }
        
        var lineGO = Instantiate(connectionLinePrefab, boardContainer);
        lineGO.name = connectionKey;
        
        var lineRenderer = lineGO.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            var pos1 = GetNodePosition(node1.position);
            var pos2 = GetNodePosition(node2.position);
            
            // Convert to world space for LineRenderer
            var worldPos1 = boardContainer.TransformPoint(pos1);
            var worldPos2 = boardContainer.TransformPoint(pos2);
            
            lineRenderer.SetPosition(0, worldPos1);
            lineRenderer.SetPosition(1, worldPos2);
            lineRenderer.startColor = connectionLineColor;
            lineRenderer.endColor = connectionLineColor;
            lineRenderer.startWidth = connectionLineWidth;
            lineRenderer.endWidth = connectionLineWidth;
            
            // Fix the material for line rendering
            FixLineRendererMaterial(lineRenderer);
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeBoardUI] Created connection line {connectionKey} from {node1.id} ({pos1}) to {node2.id} ({pos2})");
                Debug.Log($"[PassiveTreeBoardUI] World positions: {worldPos1} to {worldPos2}");
                Debug.Log($"[PassiveTreeBoardUI] LineRenderer enabled: {lineRenderer.enabled}, Color: {connectionLineColor}, Width: {connectionLineWidth}");
            }
        }
        else
        {
            Debug.LogError($"[PassiveTreeBoardUI] Connection line prefab missing LineRenderer component!");
        }
        
        connectionLines.Add(lineGO);
    }
    
    /// <summary>
    /// Get a unique key for a connection between two nodes
    /// </summary>
    private string GetConnectionKey(string node1Id, string node2Id)
    {
        // Sort IDs to ensure consistent key regardless of order
        var sortedIds = new List<string> { node1Id, node2Id };
        sortedIds.Sort();
        return $"Connection_{sortedIds[0]}_{sortedIds[1]}";
    }
    
    /// <summary>
    /// Fix the LineRenderer material to use proper line rendering
    /// </summary>
    private void FixLineRendererMaterial(LineRenderer lineRenderer)
    {
        if (lineRenderer == null) return;
        
        // Check if the current material is the sprite material (which won't work for lines)
        if (lineRenderer.sharedMaterial != null && lineRenderer.sharedMaterial.name.Contains("Sprites-Default"))
        {
            // Create a simple white material for line rendering
            var lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = Color.white;
            lineRenderer.material = lineMaterial;
            
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeBoardUI] Fixed LineRenderer material from Sprites-Default to proper line material");
            }
        }
        
        // Ensure the line renderer is set up for UI rendering
        lineRenderer.useWorldSpace = true;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.allowOcclusionWhenDynamic = false;
        lineRenderer.generateLightingData = false;
    }
    
    /// <summary>
    /// Fix all existing connection line materials
    /// </summary>
    private void FixAllConnectionLineMaterials()
    {
        Debug.Log($"[PassiveTreeBoardUI] Fixing materials for {connectionLines.Count} connection lines");
        
        foreach (var lineGO in connectionLines)
        {
            if (lineGO == null) continue;
            
            var lineRenderer = lineGO.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                FixLineRendererMaterial(lineRenderer);
            }
        }
        
        Debug.Log($"[PassiveTreeBoardUI] Finished fixing connection line materials");
    }
    
    /// <summary>
    /// Update connection line visibility based on node allocation state
    /// </summary>
    private void UpdateConnectionLines()
    {
        // Try to find tree manager if it's null
        if (treeManager == null)
        {
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
            if (treeManager == null)
            {
                Debug.LogWarning("[PassiveTreeBoardUI] Cannot update connection lines - no PassiveTreeManager found");
                return;
            }
        }
        
        if (boardData == null) return;
        
        var allocatedNodeIds = treeManager.GetAllocatedNodes().Select(n => n.id).ToList();
        
        if (showDebugInfo)
        {
            Debug.Log($"[PassiveTreeBoardUI] UpdateConnectionLines called with {connectionLines.Count} connection lines");
            Debug.Log($"[PassiveTreeBoardUI] Allocated nodes: {string.Join(", ", allocatedNodeIds)}");
        }
        
        foreach (var lineGO in connectionLines)
        {
            if (lineGO == null) continue;
            
            // Extract node IDs from the connection line name
            var connectionName = lineGO.name;
            if (!connectionName.StartsWith("Connection_"))
                continue;
                
            var parts = connectionName.Split('_');
            if (parts.Length != 3)
                continue;
                
            string node1Id = parts[1];
            string node2Id = parts[2];
            
            // Check if both connected nodes are allocated
            bool bothAllocated = allocatedNodeIds.Contains(node1Id) && allocatedNodeIds.Contains(node2Id);
            
            // Show/hide the connection line based on allocation state
            var lineRenderer = lineGO.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                bool wasEnabled = lineRenderer.enabled;
                lineRenderer.enabled = bothAllocated;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[PassiveTreeBoardUI] Connection line {connectionName}: {(bothAllocated ? "SHOWN" : "HIDDEN")} (Node1: {node1Id}, Node2: {node2Id}, Was: {wasEnabled}, Now: {lineRenderer.enabled})");
                }
            }
            else
            {
                Debug.LogError($"[PassiveTreeBoardUI] Connection line {connectionName} missing LineRenderer component!");
            }
        }
    }
    
    /// <summary>
    /// Test connection line functionality (Context Menu)
    /// </summary>
    [ContextMenu("Test Connection Lines")]
    public void TestConnectionLines()
    {
        Debug.Log("=== Connection Lines Test ===");
        
        // Try to find tree manager if it's null
        if (treeManager == null)
        {
            Debug.LogWarning("Tree Manager is null, trying to find it...");
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
            
            if (treeManager == null)
            {
                Debug.LogError("Could not find PassiveTreeManager in scene!");
                return;
            }
            else
            {
                Debug.Log("Found PassiveTreeManager, continuing with test...");
            }
        }
        
        var allocatedNodes = treeManager.GetAllocatedNodes();
        Debug.Log($"Currently allocated nodes: {string.Join(", ", allocatedNodes.Select(n => n.id))}");
        
        Debug.Log($"Total connection lines: {connectionLines.Count}");
        
        foreach (var lineGO in connectionLines)
        {
            if (lineGO == null) continue;
            
            var connectionName = lineGO.name;
            Debug.Log($"Connection line: {connectionName}");
            
            if (connectionName.StartsWith("Connection_"))
            {
                var parts = connectionName.Split('_');
                if (parts.Length == 3)
                {
                    string node1Id = parts[1];
                    string node2Id = parts[2];
                    bool node1Allocated = allocatedNodes.Any(n => n.id == node1Id);
                    bool node2Allocated = allocatedNodes.Any(n => n.id == node2Id);
                    
                    Debug.Log($"  - Node1: {node1Id} (Allocated: {node1Allocated})");
                    Debug.Log($"  - Node2: {node2Id} (Allocated: {node2Allocated})");
                    Debug.Log($"  - Line should be: {(node1Allocated && node2Allocated ? "VISIBLE" : "HIDDEN")}");
                    
                    // Check actual LineRenderer state
                    var lineRenderer = lineGO.GetComponent<LineRenderer>();
                    if (lineRenderer != null)
                    {
                        Debug.Log($"  - LineRenderer enabled: {lineRenderer.enabled}");
                        Debug.Log($"  - LineRenderer positions: {lineRenderer.GetPosition(0)} to {lineRenderer.GetPosition(1)}");
                        Debug.Log($"  - LineRenderer color: {lineRenderer.startColor}");
                        Debug.Log($"  - LineRenderer width: {lineRenderer.startWidth}");
                    }
                    else
                    {
                        Debug.LogError($"  - LineRenderer component missing!");
                    }
                }
            }
        }
        
        Debug.Log("=== End Connection Lines Test ===");
    }
    
    /// <summary>
    /// Force update connection lines (Context Menu)
    /// </summary>
    [ContextMenu("Force Update Connection Lines")]
    public void ForceUpdateConnectionLines()
    {
        Debug.Log("=== Force Update Connection Lines ===");
        UpdateConnectionLines();
        Debug.Log("=== End Force Update ===");
    }
    
    /// <summary>
    /// Check connection line prefab configuration (Context Menu)
    /// </summary>
    [ContextMenu("Check Connection Line Prefab")]
    public void CheckConnectionLinePrefab()
    {
        Debug.Log("=== Connection Line Prefab Check ===");
        
        if (connectionLinePrefab == null)
        {
            Debug.LogError("Connection line prefab is NULL!");
            return;
        }
        
        Debug.Log($"Connection line prefab: {connectionLinePrefab.name}");
        
        var lineRenderer = connectionLinePrefab.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            Debug.Log($"LineRenderer component found:");
            Debug.Log($"  - Enabled: {lineRenderer.enabled}");
            Debug.Log($"  - Shared Material: {lineRenderer.sharedMaterial}");
            Debug.Log($"  - Positions count: {lineRenderer.positionCount}");
            Debug.Log($"  - Start color: {lineRenderer.startColor}");
            Debug.Log($"  - End color: {lineRenderer.endColor}");
            Debug.Log($"  - Start width: {lineRenderer.startWidth}");
            Debug.Log($"  - End width: {lineRenderer.endWidth}");
            Debug.Log($"  - Use world space: {lineRenderer.useWorldSpace}");
        }
        else
        {
            Debug.LogError("Connection line prefab missing LineRenderer component!");
        }
        
        Debug.Log("=== End Prefab Check ===");
    }
    
    /// <summary>
    /// Force show all connection lines for testing (Context Menu)
    /// </summary>
    [ContextMenu("Force Show All Connection Lines")]
    public void ForceShowAllConnectionLines()
    {
        Debug.Log("=== Force Show All Connection Lines ===");
        
        foreach (var lineGO in connectionLines)
        {
            if (lineGO == null) continue;
            
            var lineRenderer = lineGO.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                Debug.Log($"Forced enabled connection line: {lineGO.name}");
            }
        }
        
        Debug.Log("=== End Force Show ===");
    }
    
    /// <summary>
    /// Manually assign tree manager (Context Menu)
    /// </summary>
    [ContextMenu("Assign Tree Manager")]
    public void AssignTreeManager()
    {
        Debug.Log("=== Assign Tree Manager ===");
        
        treeManager = FindFirstObjectByType<PassiveTreeManager>();
        
        if (treeManager != null)
        {
            Debug.Log($"Successfully assigned tree manager: {treeManager.name}");
            
            // Subscribe to events
            treeManager.OnNodeAllocated += OnNodeAllocated;
            treeManager.OnNodeDeallocated += OnNodeDeallocated;
            treeManager.OnStatsRecalculated += OnStatsRecalculated;
            
            Debug.Log("Subscribed to tree manager events");
        }
        else
        {
            Debug.LogError("Could not find PassiveTreeManager in scene!");
        }
        
        Debug.Log("=== End Assign Tree Manager ===");
    }
    
    /// <summary>
    /// Fix connection line materials (Context Menu)
    /// </summary>
    [ContextMenu("Fix Connection Line Materials")]
    public void FixConnectionLineMaterials()
    {
        Debug.Log("=== Fix Connection Line Materials ===");
        FixAllConnectionLineMaterials();
        Debug.Log("=== End Fix Materials ===");
    }
    
    /// <summary>
    /// Update the visual state of all nodes
    /// </summary>
    public void UpdateAllNodeStates()
    {
        if (treeManager == null) return;
        
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            var node = nodeUI.Node;
            
            // Check if node is allocated
            bool isAllocated = treeManager.GetAllocatedNodes().Exists(n => n.id == node.id);
            
            // Check if node can be allocated
            bool canAllocate = treeManager.CanAllocateNode(node.id);
            
            // Update visual state
            if (isAllocated)
            {
                nodeUI.SetState(NodeVisualState.Allocated);
            }
            else if (canAllocate)
            {
                nodeUI.SetState(NodeVisualState.Available);
            }
            else
            {
                nodeUI.SetState(NodeVisualState.Unavailable);
            }
        }
    }
    
    /// <summary>
    /// Clear all visual elements
    /// </summary>
    private void ClearBoardVisual()
    {
        // Clear nodes
        foreach (var nodeUI in nodeUIs.Values)
        {
            if (nodeUI != null)
            {
                DestroyImmediate(nodeUI.gameObject);
            }
        }
        nodeUIs.Clear();
        
        // Clear connection lines
        foreach (var line in connectionLines)
        {
            if (line != null)
            {
                DestroyImmediate(line);
            }
        }
        connectionLines.Clear();
    }
    
    /// <summary>
    /// Handle node click from PassiveTreeNodeUI
    /// </summary>
    public void OnNodeClicked(PassiveNode node)
    {
        if (treeManager == null) return;
        
        // CRITICAL: Prevent clicking on START node
        if (node.id == "core_main")
        {
            if (showDebugInfo)
            {
                Debug.Log("[PassiveTreeBoardUI] START node cannot be clicked!");
            }
            return;
        }
        
        if (treeManager.GetAllocatedNodes().Exists(n => n.id == node.id))
        {
            // Node is allocated - try to deallocate
            bool success = treeManager.DeallocateNode(node.id);
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeBoardUI] Deallocation attempt for '{node.name}': {(success ? "SUCCESS" : "FAILED")}");
            }
        }
        else
        {
            // Node is not allocated - try to allocate
            bool success = treeManager.AllocateNode(node.id);
            if (showDebugInfo)
            {
                Debug.Log($"[PassiveTreeBoardUI] Allocation attempt for '{node.name}': {(success ? "SUCCESS" : "FAILED")}");
            }
        }
    }
    
    // Event handlers
    private void OnNodeAllocated(PassiveNode node)
    {
        if (nodeUIs.ContainsKey(node.id))
        {
            nodeUIs[node.id].SetState(NodeVisualState.Allocated);
        }
        UpdateConnectionLines(); // Update connection lines when node is allocated
    }
    
    private void OnNodeDeallocated(PassiveNode node)
    {
        if (nodeUIs.ContainsKey(node.id))
        {
            UpdateAllNodeStates(); // Recalculate availability
        }
        UpdateConnectionLines(); // Update connection lines when node is deallocated
    }
    
    private void OnStatsRecalculated()
    {
        UpdateAllNodeStates();
    }
    
    /// <summary>
    /// Refresh the board visual (for editor use)
    /// </summary>
    [ContextMenu("Refresh Board Visual")]
    public void RefreshBoardVisual()
    {
        if (boardData != null)
        {
            CreateBoardVisual();
        }
        else
        {
            Debug.LogWarning("[PassiveTreeBoardUI] No board data to refresh!");
        }
    }
    
    /// <summary>
    /// Set board data from ScriptableObject
    /// </summary>
    [ContextMenu("Set Board Data from Manager")]
    public void SetBoardDataFromManager()
    {
        // Try to find the manager if not assigned
        if (treeManager == null)
        {
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
            if (treeManager == null)
            {
                Debug.LogError("[PassiveTreeBoardUI] No PassiveTreeManager found in scene!");
                return;
            }
        }
        
        // Check if the manager has a passive tree
        if (treeManager.PassiveTree == null)
        {
            Debug.LogWarning("[PassiveTreeBoardUI] PassiveTreeManager has no passive tree data!");
            return;
        }
        
        // Check if the core board exists
        if (treeManager.PassiveTree.coreBoard == null)
        {
            Debug.LogWarning("[PassiveTreeBoardUI] No core board found in passive tree!");
            return;
        }
        
        // Set the board data
        SetBoardData(treeManager.PassiveTree.coreBoard);
        Debug.Log($"[PassiveTreeBoardUI] Successfully set board data from manager: {treeManager.PassiveTree.coreBoard.name}");
    }
    
    /// <summary>
    /// Check UI setup and connections
    /// </summary>
    [ContextMenu("Check UI Setup")]
    public void CheckUISetup()
    {
        Debug.Log("=== PassiveTreeBoardUI Setup Check ===");
        
        // Check manager reference
        if (treeManager == null)
        {
            Debug.LogWarning("❌ Tree Manager: NULL (will try to find automatically)");
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
            if (treeManager == null)
            {
                Debug.LogError("❌ No PassiveTreeManager found in scene!");
                return;
            }
        }
        
        // Check board data
        if (boardData == null)
        {
            Debug.LogWarning("❌ Board Data: NULL");
        }
        
        // Check prefabs
        if (nodePrefab == null)
        {
            Debug.LogError("❌ Node Prefab: NULL");
        }
        
        if (connectionLinePrefab == null)
        {
            Debug.LogWarning("❌ Connection Line Prefab: NULL");
        }
        
        // Check if nodes are created
        Debug.Log($"📊 Created Nodes: {nodeUIs.Count}");
        
        // Check if nodes are positioned off-screen
        if (nodeUIs.Count > 0)
        {
            var firstNode = nodeUIs.Values.First();
            var nodePos = firstNode.GetComponent<RectTransform>().anchoredPosition;
            
            if (Mathf.Abs(nodePos.x) > 1000 || Mathf.Abs(nodePos.y) > 1000)
            {
                Debug.LogWarning("⚠️ Node positioned far from center - might be off-screen!");
            }
        }
        
        Debug.Log("=== End UI Setup Check ===");
    }
    
    [ContextMenu("Force Center All Nodes")]
    public void ForceCenterAllNodes()
    {
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            var node = nodeUI.Node;
            var rectTransform = nodeUI.GetComponent<RectTransform>();
            
            // Position using the proper grid layout
            var position = GetNodePosition(node.position);
            rectTransform.anchoredPosition = position;
            
            // Force visible color for testing
            var image = nodeUI.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = Color.red;
            }
        }
        
        Debug.Log($"✅ Repositioned {nodeUIs.Count} nodes in proper grid layout");
    }
    
    [ContextMenu("Fix Node Hierarchy")]
    public void FixNodeHierarchy()
    {
        Debug.Log("🔧 Fixing node hierarchy...");
        
        // Ensure boardContainer exists
        if (boardContainer == null)
        {
            Debug.LogError("[PassiveTreeBoardUI] boardContainer is null! Cannot fix hierarchy.");
            return;
        }
        
        // Find all PassiveTreeNodeUI components in the scene
        var allNodeUIs = FindObjectsByType<PassiveTreeNodeUI>(FindObjectsSortMode.None);
        int movedCount = 0;
        
        foreach (var nodeUI in allNodeUIs)
        {
            // Check if this node is not a child of boardContainer
            if (nodeUI.transform.parent != boardContainer)
            {
                Debug.Log($"[PassiveTreeBoardUI] Moving {nodeUI.name} to BoardContainer");
                nodeUI.transform.SetParent(boardContainer, false);
                movedCount++;
            }
        }
        
        Debug.Log($"✅ Moved {movedCount} nodes to BoardContainer");
        
        // Update the nodeUIs dictionary to reflect the new hierarchy
        nodeUIs.Clear();
        var boardContainerNodes = boardContainer.GetComponentsInChildren<PassiveTreeNodeUI>();
        foreach (var nodeUI in boardContainerNodes)
        {
            if (nodeUI.Node != null)
            {
                nodeUIs[nodeUI.Node.id] = nodeUI;
            }
        }
        
        Debug.Log($"✅ Updated nodeUIs dictionary with {nodeUIs.Count} nodes");
    }
    
    [ContextMenu("Set Development Size")]
    public void SetDevelopmentSize()
    {
        Debug.Log("🎯 Setting to development size (large and clear)...");
        nodeSpacing = new Vector2(150f, 150f);
        
        // Reposition all nodes
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            var node = nodeUI.Node;
            var rectTransform = nodeUI.GetComponent<RectTransform>();
            var position = GetNodePosition(node.position);
            rectTransform.anchoredPosition = position;
        }
        Debug.Log("✅ Set to development size (150x150 spacing)");
    }
    
    [ContextMenu("Set Production Size")]
    public void SetProductionSize()
    {
        Debug.Log("🎯 Setting to production size (balanced)...");
        nodeSpacing = new Vector2(100f, 100f);
        
        // Reposition all nodes
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            var node = nodeUI.Node;
            var rectTransform = nodeUI.GetComponent<RectTransform>();
            var position = GetNodePosition(node.position);
            rectTransform.anchoredPosition = position;
        }
        Debug.Log("✅ Set to production size (100x100 spacing)");
    }
    
    [ContextMenu("Set Large Size")]
    public void SetLargeSize()
    {
        Debug.Log("🎯 Setting to large size (extra clear)...");
        nodeSpacing = new Vector2(200f, 200f);
        
        // Reposition all nodes
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            var node = nodeUI.Node;
            var rectTransform = nodeUI.GetComponent<RectTransform>();
            var position = GetNodePosition(node.position);
            rectTransform.anchoredPosition = position;
        }
        Debug.Log("✅ Set to large size (200x200 spacing)");
    }
    
    [ContextMenu("Optimize Node Layout")]
    public void OptimizeNodeLayout()
    {
        Debug.Log("🎯 Optimizing node layout for better visibility...");
        
        // Adjust spacing for better visibility
        nodeSpacing = new Vector2(100f, 100f); // Increased from 80f
        boardOffset = Vector2.zero; // Center the board
        
        Debug.Log($"📍 Updated spacing to {nodeSpacing}, offset to {boardOffset}");
        
        // Reposition all nodes with new settings
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            var node = nodeUI.Node;
            var rectTransform = nodeUI.GetComponent<RectTransform>();
            
            var position = GetNodePosition(node.position);
            rectTransform.anchoredPosition = position;
        }
        
        Debug.Log($"✅ Repositioned {nodeUIs.Count} nodes with optimized layout");
    }
    
    [ContextMenu("Force Auto Initialize")]
    public void ForceAutoInitialize()
    {
        Debug.Log("🔄 Force auto-initializing board...");
        StartCoroutine(AutoInitializeBoard());
    }
    
    [ContextMenu("Show Grid Layout")]
    public void ShowGridLayout()
    {
        Debug.Log("📐 Showing current grid layout...");
        
        if (nodeUIs.Count == 0)
        {
            Debug.LogWarning("No nodes to display!");
            return;
        }
        
        // Group nodes by their grid position for easier visualization
        var gridLayout = new Dictionary<string, List<string>>();
        
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            var node = nodeUI.Node;
            var gridKey = $"({node.position.x},{node.position.y})";
            
            if (!gridLayout.ContainsKey(gridKey))
            {
                gridLayout[gridKey] = new List<string>();
            }
            
            gridLayout[gridKey].Add(node.name);
        }
        
        Debug.Log("📊 Current Grid Layout:");
        for (int row = 0; row < 7; row++)
        {
            var rowString = "";
            for (int col = 0; col < 7; col++)
            {
                var gridKey = $"({row},{col})";
                if (gridLayout.ContainsKey(gridKey))
                {
                    var nodeNames = gridLayout[gridKey];
                    var nodeName = nodeNames.Count > 0 ? nodeNames[0] : "EMPTY";
                    rowString += $"[{nodeName.PadRight(15)}]";
                }
                else
                {
                    rowString += "[EMPTY         ]";
                }
            }
            Debug.Log($"Row {row}: {rowString}");
        }
    }
    
    [ContextMenu("Toggle Auto-Calculate Cell Size")]
    public void ToggleAutoCalculateCellSize()
    {
        autoCalculateCellSize = !autoCalculateCellSize;
        Debug.Log($"[PassiveTreeBoardUI] Auto-calculate cell size: {autoCalculateCellSize}");
        
        // Recreate the board to apply the new setting
        if (boardData != null)
        {
            CreateBoardVisual();
        }
    }
    
    [ContextMenu("Set Fixed Cell Size (60)")]
    public void SetFixedCellSize60()
    {
        autoCalculateCellSize = false;
        cellSize = 60f;
        Debug.Log("[PassiveTreeBoardUI] Set fixed cell size to 60");
        
        if (boardData != null)
        {
            CreateBoardVisual();
        }
    }
    
    [ContextMenu("Set Fixed Cell Size (80)")]
    public void SetFixedCellSize80()
    {
        autoCalculateCellSize = false;
        cellSize = 80f;
        Debug.Log("[PassiveTreeBoardUI] Set fixed cell size to 80");
        
        if (boardData != null)
        {
            CreateBoardVisual();
        }
    }
    
    [ContextMenu("Set Cell Size to Fit Node")]
    public void SetCellSizeToFitNode()
    {
        autoCalculateCellSize = false;
        float nodeSize = GetNodeSize();
        cellSize = nodeSize + 20f; // Add 10px padding on each side
        Debug.Log($"[PassiveTreeBoardUI] Set cell size to fit node: {cellSize} (node size: {nodeSize})");
        
        if (boardData != null)
        {
            CreateBoardVisual();
        }
    }
    
    [ContextMenu("Set Cell Size to Fit Node (Tight)")]
    public void SetCellSizeToFitNodeTight()
    {
        autoCalculateCellSize = false;
        float nodeSize = GetNodeSize();
        cellSize = nodeSize + 10f; // Add 5px padding on each side
        Debug.Log($"[PassiveTreeBoardUI] Set cell size to fit node (tight): {cellSize} (node size: {nodeSize})");
        
        if (boardData != null)
        {
            CreateBoardVisual();
        }
    }
    
    [ContextMenu("Debug Cell Positioning")]
    public void DebugCellPositioning()
    {
        Debug.Log("=== Cell Positioning Debug ===");
        
        if (cellPrefab == null)
        {
            Debug.LogError("CellPrefab is not assigned!");
            return;
        }
        
        // Show effective cell size
        float effectiveSize = GetEffectiveCellSize();
        float nodeSize = GetNodeSize();
        Debug.Log($"Effective cell size: {effectiveSize}, Node size: {nodeSize}, Auto-calculate: {autoCalculateCellSize}");
        
        if (boardData != null)
        {
            Debug.Log($"Board size: {boardData.size}, Offset: {boardOffset}");
        }
        
        Debug.Log("=== End Cell Positioning Debug ===");
    }
    
    [ContextMenu("Debug Sprite Assignments")]
    public void DebugSpriteAssignments()
    {
        Debug.Log($"[PassiveTreeBoardUI] === Sprite Assignment Debug ===");
        Debug.Log($"[PassiveTreeBoardUI] Board: {(boardData != null ? boardData.name : "NULL")}, Use Custom: {useCustomSprites}");
        Debug.Log($"[PassiveTreeBoardUI] Sprite Manager: {(spriteManager != null ? spriteManager.name : "NULL")}");
        Debug.Log($"[PassiveTreeBoardUI] Node UIs Count: {nodeUIs.Count}");
        Debug.Log($"[PassiveTreeBoardUI] === End Sprite Debug ===");
    }
    
    [ContextMenu("Force Update All Node Sprites")]
    public void ForceUpdateAllNodeSprites()
    {
        Debug.Log($"[PassiveTreeBoardUI] === Force Updating All Node Sprites ===");
        
        foreach (var kvp in nodeUIs)
        {
            var nodeUI = kvp.Value;
            nodeUI.SetState(nodeUI.CurrentState); // This will trigger UpdateVisualState()
        }
        
        Debug.Log($"[PassiveTreeBoardUI] === End Force Update ===");
    }
}

/// <summary>
/// Visual states for nodes
/// </summary>
public enum NodeVisualState
{
    Available,      // Can be allocated
    Allocated,      // Currently allocated
    Unavailable     // Cannot be allocated (requirements not met)
}
