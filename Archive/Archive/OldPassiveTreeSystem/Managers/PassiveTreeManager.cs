using System.Collections.Generic;
using UnityEngine;
using PassiveTree;

/// <summary>
/// Main manager for the modular passive tree system
/// Handles state management, board connections, and stat calculations
/// </summary>
public class PassiveTreeManager : MonoBehaviour
{
    [Header("Board ScriptableObjects")]
    [Tooltip("Assign your CoreBoard ScriptableObject here")]
    public PassiveBoardScriptableObject coreBoardAsset;
    
    [Tooltip("Assign your Extension Board ScriptableObjects here")]
    public List<PassiveBoardScriptableObject> extensionBoardAssets = new List<PassiveBoardScriptableObject>();
    
    [Tooltip("Assign your Keystone Board ScriptableObjects here")]
    public List<PassiveBoardScriptableObject> keystoneBoardAssets = new List<PassiveBoardScriptableObject>();
    
    [Header("Passive Tree Data (Auto-generated)")]
    [SerializeField] private ModularPassiveTree passiveTree;
    
    /// <summary>
    /// Public access to the passive tree data
    /// </summary>
    public ModularPassiveTree PassiveTree => passiveTree;
    
    /// <summary>
    /// Get the core board for easy access
    /// </summary>
    public PassiveBoard GetCoreBoard()
    {
        return passiveTree?.coreBoard;
    }
    
    [ContextMenu("Toggle Infinite Points")]
    public void ToggleInfinitePoints()
    {
        infinitePoints = !infinitePoints;
        LogDebug($"Infinite points {(infinitePoints ? "ENABLED" : "DISABLED")}");
        
        // Update the UI if needed
        if (infinitePoints)
        {
            LogDebug($"Available points: {GetAvailablePoints()}");
        }
    }
    
    [ContextMenu("Enable Infinite Points")]
    public void EnableInfinitePoints()
    {
        infinitePoints = true;
        LogDebug("Infinite points ENABLED");
        LogDebug($"Available points: {GetAvailablePoints()}");
    }
    
    [ContextMenu("Disable Infinite Points")]
    public void DisableInfinitePoints()
    {
        infinitePoints = false;
        LogDebug("Infinite points DISABLED");
        LogDebug($"Available points: {GetAvailablePoints()}");
    }
    
    [ContextMenu("Add 1000 Points")]
    public void Add1000Points()
    {
        if (playerState != null)
        {
            playerState.AddSkillPoints(1000);
            LogDebug($"Added 1000 points. Total: {GetAvailablePoints()}");
        }
    }
    
    [ContextMenu("Toggle Verbose Logging")]
    public void ToggleVerboseLogging()
    {
        enableVerboseLogging = !enableVerboseLogging;
        LogDebug($"Verbose logging {(enableVerboseLogging ? "ENABLED" : "DISABLED")}");
    }
    
    [ContextMenu("Debug Adjacency Logic")]
    public void DebugAdjacencyLogic()
    {
        if (passiveTree == null || playerState == null)
        {
            LogWarning("PassiveTree or PlayerState is null!");
            return;
        }
        
        if (!enableVerboseLogging)
        {
            LogDebug("Verbose logging is disabled. Enable it to see detailed adjacency information.");
            return;
        }
        
        LogDebug("=== Adjacency Logic Debug ===");
        LogDebug($"Allocated nodes: {string.Join(", ", playerState.GetAllocatedNodeIds())}");
        
        var allNodes = passiveTree.GetAllNodes();
        foreach (var node in allNodes)
        {
            bool canAllocate = CanAllocateNode(node.id);
            LogDebug($"Node '{node.id}' (Pos: {node.position}): Can Allocate = {canAllocate}");
            
            if (!canAllocate && node.id != "core_main")
            {
                // Check why it can't be allocated
                if (node.currentRank >= node.maxRank)
                {
                    LogDebug($"  - Reason: Already maxed out (Rank {node.currentRank}/{node.maxRank})");
                }
                else if (GetAvailablePoints() < node.cost)
                {
                    LogDebug($"  - Reason: Insufficient points (Need {node.cost}, Have {GetAvailablePoints()})");
                }
                else
                {
                    LogDebug($"  - Reason: Not adjacent to allocated nodes");
                    LogDebug($"  - Connected nodes: {string.Join(", ", node.connections)}");
                }
            }
        }
        
        LogDebug("=== End Adjacency Logic Debug ===");
    }
    
    [ContextMenu("Test Orthogonal Adjacency")]
    public void TestOrthogonalAdjacency()
    {
        if (passiveTree == null || playerState == null)
        {
            LogWarning("PassiveTree or PlayerState is null!");
            return;
        }
        
        if (!enableVerboseLogging)
        {
            LogDebug("Verbose logging is disabled. Enable it to see detailed orthogonal adjacency information.");
            return;
        }
        
        LogDebug("=== Orthogonal Adjacency Test ===");
        
        // Find the START node
        var startNode = passiveTree.FindNodeById("core_main");
        if (startNode == null)
        {
            LogWarning("START node (core_main) not found!");
            return;
        }
        
        LogDebug($"START node position: {startNode.position}");
        LogDebug($"START node connections: {string.Join(", ", startNode.connections)}");
        
        // Test each connected node for orthogonal adjacency
        foreach (string connectedId in startNode.connections)
        {
            var connectedNode = passiveTree.FindNodeById(connectedId);
            if (connectedNode != null)
            {
                int rowDiff = Mathf.Abs(startNode.position.x - connectedNode.position.x);
                int colDiff = Mathf.Abs(startNode.position.y - connectedNode.position.y);
                bool isOrthogonal = (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
                
                LogDebug($"Connected node '{connectedId}' (Pos: {connectedNode.position}):");
                LogDebug($"  - Row diff: {rowDiff}, Col diff: {colDiff}");
                LogDebug($"  - Is orthogonal: {isOrthogonal}");
                LogDebug($"  - Can allocate: {CanAllocateNode(connectedId)}");
            }
        }
        
        LogDebug("=== End Orthogonal Adjacency Test ===");
    }
    
    [ContextMenu("Test Node Adjacency Logic")]
    public void TestNodeAdjacencyLogic()
    {
        if (passiveTree == null || playerState == null)
        {
            LogWarning("PassiveTree or PlayerState is null!");
            return;
        }
        
        LogDebug("=== Node Adjacency Logic Test ===");
        
        // Test a specific node's adjacency logic
        var allNodes = passiveTree.GetAllNodes();
        foreach (var node in allNodes)
        {
            if (node.id != "core_main") // Skip START node
            {
                LogDebug($"Testing node '{node.id}' at position {node.position}");
                
                // Call the adjacency check directly
                bool canAllocate = node.CanAllocate(playerState.GetAllocatedNodeIds(), GetAvailablePoints());
                LogDebug($"  - Can allocate: {canAllocate}");
                
                if (!canAllocate)
                {
                    LogDebug($"  - Reason: Adjacency check failed");
                }
            }
        }
        
        LogDebug("=== End Node Adjacency Logic Test ===");
    }
    
    [ContextMenu("Test Grid Adjacency")]
    public void TestGridAdjacency()
    {
        if (passiveTree == null || playerState == null)
        {
            LogWarning("PassiveTree or PlayerState is null!");
            return;
        }
        
        LogDebug("=== Grid Adjacency Test ===");
        
        var coreBoard = passiveTree.coreBoard;
        if (coreBoard == null)
        {
            LogWarning("CoreBoard is null!");
            return;
        }
        
        LogDebug($"CoreBoard size: {coreBoard.size}");
        LogDebug($"Allocated nodes: {string.Join(", ", playerState.GetAllocatedNodeIds())}");
        
        // Test each position in the grid
        for (int row = 0; row < coreBoard.size.x; row++)
        {
            for (int col = 0; col < coreBoard.size.y; col++)
            {
                var node = coreBoard.GetNode(row, col);
                if (node != null)
                {
                    LogDebug($"Node '{node.id}' at ({row}, {col}):");
                    
                    // Check adjacent positions
                    Vector2Int[] adjacentPositions = new Vector2Int[]
                    {
                        new Vector2Int(row - 1, col), // Up
                        new Vector2Int(row + 1, col), // Down
                        new Vector2Int(row, col - 1), // Left
                        new Vector2Int(row, col + 1)  // Right
                    };
                    
                    foreach (Vector2Int adjPos in adjacentPositions)
                    {
                        if (adjPos.x >= 0 && adjPos.x < coreBoard.size.x && adjPos.y >= 0 && adjPos.y < coreBoard.size.y)
                        {
                            var adjacentNode = coreBoard.GetNode(adjPos.x, adjPos.y);
                            if (adjacentNode != null)
                            {
                                bool isAllocated = playerState.GetAllocatedNodeIds().Contains(adjacentNode.id);
                                LogDebug($"  - Adjacent to '{adjacentNode.id}' at ({adjPos.x}, {adjPos.y}): Allocated = {isAllocated}");
                            }
                        }
                    }
                }
            }
        }
        
        LogDebug("=== End Grid Adjacency Test ===");
    }
    
    [ContextMenu("Test Specific Node Allocation")]
    public void TestSpecificNodeAllocation()
    {
        if (passiveTree == null || playerState == null)
        {
            LogWarning("PassiveTree or PlayerState is null!");
            return;
        }
        
        LogDebug("=== Specific Node Allocation Test ===");
        
        var coreBoard = passiveTree.coreBoard;
        if (coreBoard == null)
        {
            LogWarning("CoreBoard is null!");
            return;
        }
        
        var allocatedNodes = playerState.GetAllocatedNodeIds();
        LogDebug($"Current allocated nodes: {string.Join(", ", allocatedNodes)}");
        
        // Test a few specific nodes
        string[] testNodeIds = { "core_main", "node_1", "node_2", "node_3" };
        
        foreach (string nodeId in testNodeIds)
        {
            var node = passiveTree.FindNodeById(nodeId);
            if (node != null)
            {
                bool canAllocate = CanAllocateNode(nodeId);
                LogDebug($"Node '{nodeId}' at position {node.position}: Can Allocate = {canAllocate}");
            }
            else
            {
                LogDebug($"Node '{nodeId}' not found");
            }
        }
        
        LogDebug("=== End Specific Node Allocation Test ===");
    }
    
    [ContextMenu("Debug Sprite Manager Status")]
    public void DebugSpriteManagerStatus()
    {
        LogDebug("=== Sprite Manager Status Debug ===");
        
        // Check current CoreBoard
        if (passiveTree?.coreBoard != null)
        {
            var spriteManager = passiveTree.coreBoard.GetSpriteManager();
            var useCustomSprites = passiveTree.coreBoard.GetUseCustomSprites();
            
            LogDebug($"[PassiveTreeManager] CoreBoard Sprite Manager: {(spriteManager != null ? spriteManager.name : "NULL")}");
            LogDebug($"[PassiveTreeManager] CoreBoard Use Custom Sprites: {useCustomSprites}");
            LogDebug($"[PassiveTreeManager] CoreBoard Node Count: {passiveTree.coreBoard.GetAllNodes().Count}");
        }
        else
        {
            LogWarning("[PassiveTreeManager] CoreBoard is NULL!");
        }
        
        // Check CoreBoard asset
        if (coreBoardAsset != null)
        {
            var assetBoardData = coreBoardAsset.GetBoardData();
            if (assetBoardData != null)
            {
                var assetSpriteManager = assetBoardData.GetSpriteManager();
                var assetUseCustomSprites = assetBoardData.GetUseCustomSprites();
                
                LogDebug($"[PassiveTreeManager] CoreBoard Asset Sprite Manager: {(assetSpriteManager != null ? assetSpriteManager.name : "NULL")}");
                LogDebug($"[PassiveTreeManager] CoreBoard Asset Use Custom Sprites: {assetUseCustomSprites}");
            }
            else
            {
                LogWarning("[PassiveTreeManager] CoreBoard Asset Board Data is NULL!");
            }
        }
        else
        {
            LogWarning("[PassiveTreeManager] CoreBoard Asset is NULL!");
        }
        
        // Check for sprite managers in project
        var allSpriteManagers = Resources.FindObjectsOfTypeAll<PassiveTreeSpriteManager>();
        LogDebug($"[PassiveTreeManager] Found {allSpriteManagers.Length} sprite managers in project:");
        foreach (var sm in allSpriteManagers)
        {
            LogDebug($"  - {sm.name}");
        }
        
        LogDebug("=== End Sprite Manager Status Debug ===");
    }
    
    [Header("Player State")]
    public PlayerPassiveState playerState;
    
    [Header("Settings")]
    public bool autoRecalculateStats = true;
    public bool enableDebugLogging = true;
    
    [Header("Infinite Points (Debug)")]
    [SerializeField] private bool infinitePoints = false;
    [SerializeField] private int debugPoints = 999;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableVerboseLogging = false;
    
    // Singleton pattern
    public static PassiveTreeManager Instance { get; private set; }
    
    // Events
    public System.Action<PassiveNode> OnNodeAllocated;
    public System.Action<PassiveNode> OnNodeDeallocated;
    public System.Action<BoardConnection> OnBoardConnected;
    public System.Action<BoardConnection> OnBoardDisconnected;
    public System.Action OnStatsRecalculated;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize player state if not set
        if (playerState == null)
        {
            playerState = new PlayerPassiveState();
        }
    }
    
    private void Start()
    {
        InitializePassiveTree();
        
        // Ensure CoreBoard is initialized for PassiveTreeScene
        EnsureCoreBoardInitialized();
        
        // Load persistent state for current character
        LoadPersistentState();
    }
    
    private void OnEnable()
    {
        // Subscribe to scene changes to reinitialize when entering PassiveTreeScene
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from scene changes
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Handle scene loading to ensure proper initialization
    /// </summary>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Check if we're in the PassiveTreeScene
        if (scene.name.Contains("PassiveTree") || scene.name.Contains("passive") || scene.name.Contains("tree"))
        {
            LogDebug($"Entered PassiveTreeScene: {scene.name}");
            EnsureCoreBoardInitialized();
        }
    }
    
    /// <summary>
    /// Ensure the CoreBoard is always initialized when needed
    /// </summary>
    private void EnsureCoreBoardInitialized()
    {
        if (passiveTree?.coreBoard != null)
        {
            // Force initialize the core board if it's not already initialized
            if (passiveTree.coreBoard.GetAllNodes().Count == 0)
            {
                LogDebug("CoreBoard nodes array is empty, creating complete board...");
                
                // Get sprite manager from the CoreBoard asset (the source of truth)
                PassiveTreeSpriteManager spriteManagerToUse = null;
                bool useCustomSpritesToUse = true;
                
                if (coreBoardAsset != null)
                {
                    var assetBoardData = coreBoardAsset.GetBoardData();
                    if (assetBoardData != null)
                    {
                        spriteManagerToUse = assetBoardData.GetSpriteManager();
                        useCustomSpritesToUse = assetBoardData.GetUseCustomSprites();
                        LogDebug($"Sprite manager from CoreBoard asset: {(spriteManagerToUse != null ? spriteManagerToUse.name : "NULL")}");
                    }
                }
                
                // If still null, try to find any sprite manager in the project
                if (spriteManagerToUse == null)
                {
                    var allSpriteManagers = Resources.FindObjectsOfTypeAll<PassiveTreeSpriteManager>();
                    if (allSpriteManagers.Length > 0)
                    {
                        spriteManagerToUse = allSpriteManagers[0];
                        LogDebug($"Found sprite manager in project: {spriteManagerToUse.name}");
                    }
                }
                
                LogDebug($"Creating CoreBoard with sprite manager: {(spriteManagerToUse != null ? spriteManagerToUse.name : "NULL")}");
                LogDebug($"Use custom sprites: {useCustomSpritesToUse}");
                
                // Create the complete core board WITH sprite manager settings
                var completeBoard = CoreBoardSetup.CreateCompleteCoreBoard(spriteManagerToUse, useCustomSpritesToUse);
                
                // Assign the new board to the passive tree
                passiveTree.coreBoard = completeBoard;
                
                // Verify the sprite manager was properly assigned
                var finalSpriteManager = passiveTree.coreBoard.GetSpriteManager();
                LogDebug($"Final sprite manager after assignment: {(finalSpriteManager != null ? finalSpriteManager.name : "NULL")}");
                
                LogDebug($"Complete CoreBoard created with {passiveTree.coreBoard.GetAllNodes().Count} nodes");
            }
            else
            {
                LogDebug($"CoreBoard already has {passiveTree.coreBoard.GetAllNodes().Count} nodes");
            }
        }
        else
        {
            LogWarning("CoreBoard is null, cannot initialize!");
        }
    }
    
    /// <summary>
    /// Initialize the passive tree system
    /// </summary>
    public void InitializePassiveTree()
    {
        // Create the passive tree from ScriptableObject assets
        CreatePassiveTreeFromAssets();
        
        if (passiveTree != null)
        {
            passiveTree.Initialize();
            LogDebug("Passive tree initialized");
        }
        else
        {
            LogError("No passive tree data assigned!");
        }
        
        // Initialize player state
        if (playerState != null)
        {
            playerState.Initialize();
            LogDebug($"Player state initialized with {playerState.availablePoints} points");
        }
    }
    
    /// <summary>
    /// Load persistent state for the current character
    /// </summary>
    private void LoadPersistentState()
    {
        if (playerState == null) return;
        
        // Get current character name
        string characterName = GetCurrentCharacterName();
        if (string.IsNullOrEmpty(characterName))
        {
            LogDebug("No character loaded, skipping persistent state load");
            return;
        }
        
        // Check if we have saved state for this character
        if (PlayerPassiveState.HasSavedState(characterName))
        {
            LogDebug($"Loading persistent passive tree state for {characterName}");
            playerState.LoadFromPlayerPrefs(characterName);
            
            // Recalculate stats after loading
            if (autoRecalculateStats)
            {
                RecalculateStats();
            }
        }
        else
        {
            LogDebug($"No saved passive tree state found for {characterName}, using default state");
        }
    }
    
    /// <summary>
    /// Save persistent state for the current character
    /// </summary>
    public void SavePersistentState()
    {
        if (playerState == null) return;
        
        string characterName = GetCurrentCharacterName();
        if (string.IsNullOrEmpty(characterName))
        {
            LogDebug("No character loaded, skipping persistent state save");
            return;
        }
        
        LogDebug($"Saving persistent passive tree state for {characterName}");
        playerState.SaveToPlayerPrefs(characterName);
    }
    
    /// <summary>
    /// Get the current character name
    /// </summary>
    private string GetCurrentCharacterName()
    {
        // Try to get character name from CharacterManager
        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCharacter())
        {
            var character = CharacterManager.Instance.GetCurrentCharacter();
            return character?.characterName;
        }
        
        return null;
    }
    
    /// <summary>
    /// Create the passive tree from assigned ScriptableObject assets
    /// </summary>
    private void CreatePassiveTreeFromAssets()
    {
        passiveTree = new ModularPassiveTree();
        
        // Assign core board
        if (coreBoardAsset != null)
        {
            // Force refresh the board data to ensure we get the latest version
            var boardData = coreBoardAsset.GetBoardData();
            
            // If the board has no nodes, try to create them
            if (boardData.GetAllNodes().Count == 0)
            {
                LogDebug("Core board has no nodes, attempting to create complete board...");
                coreBoardAsset.CreateCompleteCoreBoard();
                boardData = coreBoardAsset.GetBoardData(); // Get the updated data
            }
            
            passiveTree.coreBoard = boardData;
            LogDebug($"Core board assigned: {coreBoardAsset.name} with {boardData.GetAllNodes().Count} nodes");
        }
        else
        {
            LogError("No core board asset assigned!");
        }
        
        // Assign extension boards
        foreach (var boardAsset in extensionBoardAssets)
        {
            if (boardAsset != null)
            {
                var boardData = boardAsset.GetBoardData();
                passiveTree.extensionBoards[boardData.id] = boardData;
                LogDebug($"Extension board assigned: {boardAsset.name} (ID: {boardData.id})");
            }
        }
        
        // Assign keystone boards
        foreach (var boardAsset in keystoneBoardAssets)
        {
            if (boardAsset != null)
            {
                var boardData = boardAsset.GetBoardData();
                passiveTree.keystoneBoards[boardData.id] = boardData;
                LogDebug($"Keystone board assigned: {boardAsset.name} (ID: {boardData.id})");
            }
        }
    }
    
    /// <summary>
    /// Allocate a passive node
    /// </summary>
    public bool AllocateNode(string nodeId)
    {
        if (passiveTree == null || playerState == null)
            return false;
            
        var node = passiveTree.FindNodeById(nodeId);
        if (node == null)
        {
            LogError($"Node not found: {nodeId}");
            return false;
        }
        
        // Check if the node can be allocated (including adjacency validation)
        if (!CanAllocateNode(nodeId))
        {
            LogDebug($"Failed to allocate node: {nodeId} (Cannot allocate - check adjacency, points, or requirements)");
            return false;
        }
        
        // If infinite points is enabled, allocate without consuming points
        if (infinitePoints)
        {
            if (!playerState.IsNodeAllocated(nodeId))
            {
                // Add to allocated nodes without consuming points
                playerState.allocatedNodes.Add(nodeId);
                playerState.statsDirty = true;
                
                LogDebug($"Allocated node with infinite points: {nodeId} (Cost: {node.cost}, but no points consumed)");
                OnNodeAllocated?.Invoke(node);
                
                if (autoRecalculateStats)
                {
                    RecalculateStats();
                }
                
                // Save persistent state after allocation
                SavePersistentState();
                
                return true;
            }
            else
            {
                LogDebug($"Failed to allocate node: {nodeId} (Already allocated)");
                return false;
            }
        }
        else
        {
            // Normal allocation with point consumption
            if (playerState.AllocateNode(nodeId, node.cost))
            {
                LogDebug($"Allocated node: {nodeId} (Cost: {node.cost})");
                OnNodeAllocated?.Invoke(node);
                
                if (autoRecalculateStats)
                {
                    RecalculateStats();
                }
                
                // Save persistent state after allocation
                SavePersistentState();
                
                return true;
            }
            
            LogDebug($"Failed to allocate node: {nodeId} (Insufficient points or already allocated)");
            return false;
        }
    }
    
    /// <summary>
    /// Deallocate a passive node
    /// </summary>
    public bool DeallocateNode(string nodeId)
    {
        if (passiveTree == null || playerState == null)
            return false;
            
        var node = passiveTree.FindNodeById(nodeId);
        if (node == null)
        {
            LogError($"Node not found: {nodeId}");
            return false;
        }
        
        if (playerState.DeallocateNode(nodeId, node.cost))
        {
            LogDebug($"Deallocated node: {nodeId} (Refunded: {node.cost})");
            OnNodeDeallocated?.Invoke(node);
            
            if (autoRecalculateStats)
            {
                RecalculateStats();
            }
            
            // Save persistent state after deallocation
            SavePersistentState();
            
            return true;
        }
        
        LogDebug($"Failed to deallocate node: {nodeId} (Not allocated)");
        return false;
    }
    
    /// <summary>
    /// Connect a board to an extension point
    /// </summary>
    public bool ConnectBoard(string extensionPointId, string boardId)
    {
        if (passiveTree == null || playerState == null)
            return false;
            
        // Check if board can be connected
        if (!passiveTree.CanConnectBoard(boardId))
        {
            LogDebug($"Cannot connect board: {boardId} (Limit reached)");
            return false;
        }
        
        if (playerState.ConnectBoard(extensionPointId, boardId))
        {
            LogDebug($"Connected board: {boardId} to extension point: {extensionPointId}");
            OnBoardConnected?.Invoke(playerState.GetBoardConnections().Find(c => c.boardId == boardId));
            return true;
        }
        
        LogDebug($"Failed to connect board: {boardId} (Already connected)");
        return false;
    }
    
    /// <summary>
    /// Disconnect a board
    /// </summary>
    public bool DisconnectBoard(string boardId)
    {
        if (passiveTree == null || playerState == null)
            return false;
            
        if (playerState.DisconnectBoard(boardId))
        {
            LogDebug($"Disconnected board: {boardId}");
            OnBoardDisconnected?.Invoke(new BoardConnection("", boardId));
            return true;
        }
        
        LogDebug($"Failed to disconnect board: {boardId} (Not connected)");
        return false;
    }
    
    /// <summary>
    /// Recalculate stats from all allocated nodes
    /// </summary>
    public Dictionary<string, float> RecalculateStats()
    {
        if (passiveTree == null || playerState == null)
            return new Dictionary<string, float>();
            
        var totalStats = passiveTree.GetTotalStats();
        playerState.UpdateCachedStats(totalStats);
        
        LogDebug($"Stats recalculated: {totalStats.Count} stat types");
        OnStatsRecalculated?.Invoke();
        
        return totalStats;
    }
    
    /// <summary>
    /// Get current stats (cached or recalculated)
    /// </summary>
    public Dictionary<string, float> GetCurrentStats()
    {
        if (playerState == null)
            return new Dictionary<string, float>();
            
        if (playerState.AreStatsDirty())
        {
            return RecalculateStats();
        }
        
        return playerState.GetCachedStats();
    }
    
    /// <summary>
    /// Add skill points to the player
    /// </summary>
    public void AddSkillPoints(int points)
    {
        if (playerState != null)
        {
            playerState.AddSkillPoints(points);
            LogDebug($"Added {points} skill points. Total: {playerState.availablePoints}");
        }
    }
    
    /// <summary>
    /// Get available skill points
    /// </summary>
    public int GetAvailablePoints()
    {
        if (infinitePoints)
        {
            return debugPoints;
        }
        return playerState?.availablePoints ?? 0;
    }
    
    /// <summary>
    /// Check if a node can be allocated
    /// </summary>
    public bool CanAllocateNode(string nodeId)
    {
        if (passiveTree == null || playerState == null)
            return false;
            
        var node = passiveTree.FindNodeById(nodeId);
        if (node == null)
            return false;
        
        // If infinite points is enabled, use debug points instead of actual points
        int availablePoints = infinitePoints ? debugPoints : playerState.availablePoints;
        return node.CanAllocate(playerState.GetAllocatedNodeIds(), availablePoints);
    }
    
    /// <summary>
    /// Get all allocated nodes
    /// </summary>
    public List<PassiveNode> GetAllocatedNodes()
    {
        if (passiveTree == null)
            return new List<PassiveNode>();
            
        var allocatedNodes = new List<PassiveNode>();
        foreach (var nodeId in playerState?.GetAllocatedNodeIds() ?? new List<string>())
        {
            var node = passiveTree.FindNodeById(nodeId);
            if (node != null)
            {
                allocatedNodes.Add(node);
            }
        }
        
        return allocatedNodes;
    }
    
    /// <summary>
    /// Get all connected boards
    /// </summary>
    public List<PassiveBoard> GetConnectedBoards()
    {
        if (passiveTree == null)
            return new List<PassiveBoard>();
            
        var connectedBoards = new List<PassiveBoard>();
        foreach (var boardId in playerState?.GetConnectedBoardIds() ?? new List<string>())
        {
            var board = passiveTree.FindBoardById(boardId);
            if (board != null)
            {
                connectedBoards.Add(board);
            }
        }
        
        return connectedBoards;
    }
    
    /// <summary>
    /// Reset the player's passive tree
    /// </summary>
    public void ResetPassiveTree()
    {
        if (playerState != null)
        {
            var currentPoints = playerState.availablePoints;
            playerState.Reset();
            LogDebug("Passive tree reset");
        }
    }
    
    /// <summary>
    /// Debug logging
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[PassiveTreeManager] {message}");
        }
    }
    
    /// <summary>
    /// Error logging
    /// </summary>
    private void LogError(string message)
    {
        Debug.LogError($"[PassiveTreeManager] {message}");
    }
    
    /// <summary>
    /// Warning logging
    /// </summary>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[PassiveTreeManager] {message}");
    }
    
    /// <summary>
    /// Force initialize the CoreBoard (for debugging)
    /// </summary>
    [ContextMenu("Force Initialize CoreBoard")]
    public void ForceInitializeCoreBoard()
    {
        LogDebug("Force initializing CoreBoard...");
        EnsureCoreBoardInitialized();
        
        if (passiveTree?.coreBoard != null)
        {
            var nodeCount = passiveTree.coreBoard.GetAllNodes().Count;
            LogDebug($"CoreBoard now has {nodeCount} nodes");
            
            // Trigger UI refresh if any UI components exist
            var boardUIs = FindObjectsByType<PassiveTreeBoardUI>(FindObjectsSortMode.None);
            foreach (var boardUI in boardUIs)
            {
                boardUI.RefreshBoardVisual();
            }
        }
    }
    
    /// <summary>
    /// Debug method to check board assignments
    /// </summary>
    [ContextMenu("Check Board Assignments")]
    public void CheckBoardAssignments()
    {
        Debug.Log("=== Board Assignment Check ===");
        
        if (coreBoardAsset != null)
        {
            Debug.Log($"✅ Core Board: {coreBoardAsset.name}");
        }
        else
        {
            Debug.LogError("❌ Core Board: NOT ASSIGNED");
        }
        
        Debug.Log($"Extension Boards: {extensionBoardAssets.Count}");
        for (int i = 0; i < extensionBoardAssets.Count; i++)
        {
            if (extensionBoardAssets[i] != null)
            {
                Debug.Log($"  ✅ [{i}] {extensionBoardAssets[i].name}");
            }
            else
            {
                Debug.LogError($"  ❌ [{i}] NULL");
            }
        }
        
        Debug.Log($"Keystone Boards: {keystoneBoardAssets.Count}");
        for (int i = 0; i < keystoneBoardAssets.Count; i++)
        {
            if (keystoneBoardAssets[i] != null)
            {
                Debug.Log($"  ✅ [{i}] {keystoneBoardAssets[i].name}");
            }
            else
            {
                Debug.LogError($"  ❌ [{i}] NULL");
            }
        }
        
        if (passiveTree != null)
        {
            Debug.Log($"✅ Passive Tree: Initialized");
            if (passiveTree.coreBoard != null)
            {
                Debug.Log($"  ✅ Core Board Data: {passiveTree.coreBoard.name}");
            }
            else
            {
                Debug.LogError("  ❌ Core Board Data: NULL");
            }
        }
        else
        {
            Debug.LogError("❌ Passive Tree: NOT INITIALIZED");
        }
        
        Debug.Log("=== End Board Assignment Check ===");
    }
    
    /// <summary>
    /// Force reinitialize the passive tree
    /// </summary>
    [ContextMenu("Reinitialize Passive Tree")]
    public void ReinitializePassiveTree()
    {
        Debug.Log("Reinitializing passive tree...");
        CreatePassiveTreeFromAssets();
        if (passiveTree != null)
        {
            passiveTree.Initialize();
            LogDebug("Passive tree reinitialized");
        }
    }
    
    /// <summary>
    /// Force refresh the core board data from the ScriptableObject
    /// </summary>
    [ContextMenu("Force Refresh Core Board Data")]
    public void ForceRefreshCoreBoardData()
    {
        if (coreBoardAsset == null)
        {
            LogError("No core board asset assigned!");
            return;
        }
        
        LogDebug("Force refreshing core board data...");
        
        // Create complete board if needed
        if (coreBoardAsset.GetBoardData().GetAllNodes().Count == 0)
        {
            LogDebug("Core board is empty, creating complete board...");
            coreBoardAsset.CreateCompleteCoreBoard();
        }
        
        // Update the passive tree with fresh data
        if (passiveTree != null)
        {
            passiveTree.coreBoard = coreBoardAsset.GetBoardData();
            LogDebug($"Core board refreshed with {passiveTree.coreBoard.GetAllNodes().Count} nodes");
            
            // Trigger UI refresh
            var boardUIs = FindObjectsByType<PassiveTreeBoardUI>(FindObjectsSortMode.None);
            foreach (var boardUI in boardUIs)
            {
                boardUI.SetBoardData(passiveTree.coreBoard);
            }
        }
        else
        {
            LogError("Passive tree is null!");
        }
    }
    
    [ContextMenu("Save Persistent State")]
    public void SavePersistentStateManual()
    {
        SavePersistentState();
    }
    
    [ContextMenu("Load Persistent State")]
    public void LoadPersistentStateManual()
    {
        LoadPersistentState();
    }
    
    [ContextMenu("Clear Persistent State")]
    public void ClearPersistentState()
    {
        string characterName = GetCurrentCharacterName();
        if (!string.IsNullOrEmpty(characterName))
        {
            PlayerPassiveState.ClearSavedState(characterName);
            LogDebug($"Cleared persistent state for {characterName}");
        }
        else
        {
            LogDebug("No character loaded, cannot clear persistent state");
        }
    }
    
    [ContextMenu("Debug Persistent State")]
    public void DebugPersistentState()
    {
        string characterName = GetCurrentCharacterName();
        LogDebug($"=== Persistent State Debug ===");
        LogDebug($"Current Character: {characterName ?? "None"}");
        
        if (!string.IsNullOrEmpty(characterName))
        {
            bool hasState = PlayerPassiveState.HasSavedState(characterName);
            LogDebug($"Has Saved State: {hasState}");
            
            if (playerState != null)
            {
                LogDebug($"Allocated Nodes: {playerState.GetAllocatedNodeIds().Count}");
                LogDebug($"Available Points: {playerState.GetAvailablePoints()}");
                LogDebug($"Connected Boards: {playerState.GetConnectedBoardIds().Count}");
            }
        }
        
        LogDebug($"=== End Debug ===");
    }
    
}
