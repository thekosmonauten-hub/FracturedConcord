using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Extension board specific cell controller
    /// Handles mouse interaction and visual state for extension board cells only
    /// Optimized for extension board behavior without core board dependencies
    /// Inherits from CellController for tooltip system compatibility
    /// Data-driven: Uses CellJsonData as primary data source
    /// </summary>
    public class CellController_EXT : CellController
    {
        // Extension board specific references
        public ExtensionBoardController extensionBoardController;
        
        // Data source reference
        private CellJsonData cellJsonData;
        
        // Data-driven properties that delegate to CellJsonData
        // Using 'new' to hide base class properties and provide data-driven implementations
        public new Vector2Int GridPosition => cellJsonData?.NodePosition ?? base.GridPosition;
        public new string NodeName => cellJsonData?.NodeName ?? base.NodeName;
        public new string NodeDescription => cellJsonData?.NodeDescription ?? base.NodeDescription;
        public new NodeType NodeType => cellJsonData?.GetNodeTypeEnum() ?? base.NodeType;
        
        // Cost and rank properties from JSON data
        public int NodeCost => cellJsonData?.NodeCost ?? 0;
        public int MaxRank => cellJsonData?.MaxRank ?? 1;
        public int CurrentRank => cellJsonData?.CurrentRank ?? 0;
        public JsonStats NodeStats => cellJsonData?.NodeStats;
        
        
        protected override void Awake()
        {
            // Call base class Awake first
            base.Awake();
            
            // Get extension board controller reference
            extensionBoardController = GetComponentInParent<ExtensionBoardController>();
            
            // Get CellJsonData reference for data-driven behavior
            cellJsonData = GetComponent<CellJsonData>();
            
            if (cellJsonData == null && showDebugInfo)
            {
                Debug.LogWarning($"[CellController_EXT] No CellJsonData component found on {gameObject.name} - will use base class data");
            }
            
            // Disable the base class button click handler for extension board cells
            // Extension board cells use ExtensionBoardCellClickHandler instead
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (showDebugInfo)
                {
                    Debug.Log($"[CellController_EXT] Disabled base class button click handler for {gameObject.name} - using ExtensionBoardCellClickHandler instead");
                }
            }
        }
        
        void Start()
        {
            // Initialize data-driven state
            InitializeDataDrivenState();
            
            // Set initial visual state
            UpdateVisualState();
            
            // Only assign sprite if none is currently set (preserve prefab sprites)
            if (autoAssignSprite && spriteRenderer != null && spriteRenderer.sprite == null)
            {
                AssignSpriteBasedOnNodeType();
            }
        }
        
        /// <summary>
        /// Initialize the cell using data from CellJsonData component
        /// </summary>
        private void InitializeDataDrivenState()
        {
            if (cellJsonData != null && cellJsonData.HasJsonData())
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[CellController_EXT] Initializing data-driven state for {gameObject.name}: '{NodeName}' ({NodeType})");
                }
                
                // Update base class fields to match JSON data (for compatibility)
                // This ensures the base class has the correct data for any legacy code
                UpdateBaseClassDataFromJson();
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[CellController_EXT] No JSON data available for {gameObject.name}, using base class defaults");
                }
            }
        }
        
        /// <summary>
        /// Update base class data fields from JSON data for backward compatibility
        /// </summary>
        private void UpdateBaseClassDataFromJson()
        {
            if (cellJsonData == null || !cellJsonData.HasJsonData()) return;
            
            // Update base class fields to match JSON data
            // Note: These are protected fields in the base class, so we need to use reflection or public setters
            // For now, we'll rely on the overridden properties to provide the correct data
            
            if (showDebugInfo)
            {
                Debug.Log($"[CellController_EXT] Updated base class data from JSON: {NodeName} at {GridPosition}");
            }
        }
        
        /// <summary>
        /// Initialize the extension board cell
        /// </summary>
        public void Initialize(ExtensionBoardController controller)
        {
            extensionBoardController = controller;
            
            // Set initial state based on node type
            SetInitialState();
            
        }
        
        /// <summary>
        /// Set initial state based on node type for extension boards
        /// Extension boards never have Start nodes - they connect to existing boards
        /// </summary>
        public void SetInitialState()
        {
            switch (nodeType)
            {
                case NodeType.Extension:
                    // Extension points start locked - only the connecting extension point will be made available by BoardPositioningManager
                    isUnlocked = false;
                    isAvailable = false;
                    isPurchased = false;
                    isExtensionPoint = true;
                    break;
                default:
                    // All other nodes start locked and unavailable
                    isUnlocked = false;
                    isAvailable = false;
                    isPurchased = false;
                    break;
            }
        }
        
        /// <summary>
        /// Override button click for extension board specific behavior
        /// </summary>
        public override void OnButtonClick()
        {
            if (!CanBeInteractedWith()) 
            {
                return;
            }
            
            // Use the data-driven GridPosition instead of base class gridPosition
            Vector2Int correctPosition = GridPosition;
            
            // Route to the extension board controller for proper node type handling
            if (extensionBoardController != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[CellController_EXT] Routing to extension board controller with correct position: {correctPosition} (was using base gridPosition: {gridPosition})");
                }
                extensionBoardController.OnCellClicked(correctPosition);
            }
            else
            {
                // Fallback: Route through the main PassiveTreeManager
                Debug.Log($"[CellController_EXT] No extension board controller found, routing through PassiveTreeManager for cell {correctPosition} (was using base gridPosition: {gridPosition})");
                RouteThroughPassiveTreeManager();
            }
        }
        
        /// <summary>
        /// Route the cell click through the main PassiveTreeManager
        /// </summary>
        private void RouteThroughPassiveTreeManager()
        {
            // Find the PassiveTreeManager in the scene
            PassiveTreeManager treeManager = FindFirstObjectByType<PassiveTreeManager>();
            if (treeManager != null)
            {
                Vector2Int correctPosition = GridPosition;
                Debug.Log($"[CellController_EXT] Routing cell click through PassiveTreeManager: {correctPosition} (was using base gridPosition: {gridPosition})");
                // Use the extension board cell handler for proper processing
                treeManager.OnExtensionBoardCellClicked(this);
            }
            else
            {
                Vector2Int correctPosition = GridPosition;
                Debug.LogError($"[CellController_EXT] No PassiveTreeManager found in scene for cell {correctPosition} (was using base gridPosition: {gridPosition})");
            }
        }
        
        /// <summary>
        /// Handle purchase for extension board cells
        /// </summary>
        public void HandleExtensionBoardCellPurchase()
        {
            // Check if we can purchase this cell
            if (!CanBeInteractedWith())
            {
                return;
            }
            
            // Purchase the cell
            SetPurchased(true);
            
            // Notify the extension board controller
            if (extensionBoardController != null)
            {
                extensionBoardController.OnCellPurchased(gridPosition);
            }
            else
            {
                Debug.LogWarning($"[CellController_EXT] No extension board controller found for cell {gridPosition}");
            }
        }
        
        /// <summary>
        /// Handle initial allocation for extension board cells
        /// Called when extension board is first created and connected
        /// </summary>
        public void HandleInitialAllocation()
        {
            if (nodeType == NodeType.Extension)
            {
                // Extension points are automatically allocated when board is created
                SetPurchased(true);
                isUnlocked = true;
                isAvailable = true;
                isExtensionPoint = true;
            }
        }
        
        /// <summary>
        /// Unlock this cell for purchasing (called when adjacent cell is purchased)
        /// </summary>
        public void UnlockForPurchasing()
        {
            if (!isPurchased && !isUnlocked)
            {
                isUnlocked = true;
                isAvailable = true;
                UpdateVisualState();
            }
        }
        
        /// <summary>
        /// Load JSON data for this extension board cell
        /// Called when the extension board JSON data is loaded
        /// Now data-driven: just refresh the state since data comes from CellJsonData
        /// Uses Y_X naming convention logic
        /// </summary>
        public void LoadJsonDataForExtensionBoard()
        {
            // Refresh the CellJsonData reference in case it was added after Awake
            if (cellJsonData == null)
            {
                cellJsonData = GetComponent<CellJsonData>();
            }
            
            if (cellJsonData != null && cellJsonData.HasJsonData())
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[CellController_EXT] Refreshing data-driven state for {gameObject.name}: '{NodeName}' ({NodeType})");
                }
                
                // Apply Y_X naming convention logic for position consistency
                if (gameObject.name.StartsWith("Cell_") && gameObject.name.Contains("_"))
                {
                    // Extract Y and X from GameObject name (Cell_Y_X format)
                    string[] nameParts = gameObject.name.Split('_');
                    if (nameParts.Length >= 3)
                    {
                        if (int.TryParse(nameParts[1], out int nameY) && int.TryParse(nameParts[2], out int nameX))
                        {
                            Vector2Int namePosition = new Vector2Int(nameX, nameY); // Convert to X,Y format
                            Vector2Int jsonPosition = cellJsonData.NodePosition;
                            
                            if (namePosition != jsonPosition)
                            {
                                Debug.Log($"[CellController_EXT] Y_X naming convention (Load JSON): GameObject {gameObject.name} expects position {namePosition}, JSON has {jsonPosition}");
                                // Note: NodePosition is read-only, so we'll use the position from the GameObject name
                                // The GridPosition property will handle the conversion automatically
                            }
                        }
                    }
                }
                
                // Update base class data for backward compatibility
                UpdateBaseClassDataFromJson();
                
                // Update visual state based on new data
                UpdateVisualState();
                
                // Only assign sprite if none is currently set (preserve prefab sprites)
                if (autoAssignSprite && spriteRenderer != null && spriteRenderer.sprite == null)
                {
                    AssignSpriteBasedOnNodeType();
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[CellController_EXT] No JSON data available for {gameObject.name}");
                }
            }
        }
        
        /// <summary>
        /// Override button interactable to include adjacency validation for extension points
        /// </summary>
        protected override void UpdateButtonInteractable()
        {
            if (button != null)
            {
                // For extension points, we need to check both basic interaction and adjacency validation
                if (nodeType == NodeType.Extension)
                {
                    // Check basic interaction first
                    bool canInteract = CanBeInteractedWith();
                    
                    if (canInteract)
                    {
                        // For extension points, also check if they have adjacent purchased nodes
                        // This requires access to the ExtensionBoardController
                        if (extensionBoardController != null)
                        {
                            // Use the extension board controller's adjacency validation
                            canInteract = extensionBoardController.HasAdjacentPurchasedNode(gridPosition);
                        }
                    }
                    
                    button.interactable = canInteract;
                }
                else
                {
                    // For non-extension nodes, use the base class logic
                    button.interactable = CanBeInteractedWith();
                }
            }
        }
        
        /// <summary>
        /// Refresh the data source reference (useful if CellJsonData is added at runtime)
        /// </summary>
        public void RefreshDataSource()
        {
            cellJsonData = GetComponent<CellJsonData>();
            if (cellJsonData != null && cellJsonData.HasJsonData())
            {
                InitializeDataDrivenState();
                UpdateVisualState();
            }
        }
        
        /// <summary>
        /// Check if this cell has JSON data available
        /// </summary>
        public bool HasJsonData()
        {
            return cellJsonData != null && cellJsonData.HasJsonData();
        }
        
        /// <summary>
        /// Get the CellJsonData component (for external access)
        /// </summary>
        public CellJsonData GetCellJsonData()
        {
            return cellJsonData;
        }
        
        /// <summary>
        /// Sync grid position from CellJsonData to CellController_EXT
        /// Uses Y_X naming convention logic
        /// </summary>
        [ContextMenu("Sync Grid Position from JSON Data")]
        public void SyncGridPositionFromJsonData()
        {
            if (cellJsonData == null)
            {
                Debug.LogWarning($"[CellController_EXT] No CellJsonData component found on {gameObject.name}");
                return;
            }
            
            if (!cellJsonData.HasJsonData())
            {
                Debug.LogWarning($"[CellController_EXT] No JSON data available on {gameObject.name}");
                return;
            }
            
            Vector2Int jsonPosition = cellJsonData.NodePosition;
            Vector2Int currentPosition = base.GridPosition;
            
            // Apply Y_X naming convention logic
            // If the GameObject name follows Cell_Y_X format, we need to ensure consistency
            if (gameObject.name.StartsWith("Cell_") && gameObject.name.Contains("_"))
            {
                // Extract Y and X from GameObject name (Cell_Y_X format)
                string[] nameParts = gameObject.name.Split('_');
                if (nameParts.Length >= 3)
                {
                    if (int.TryParse(nameParts[1], out int nameY) && int.TryParse(nameParts[2], out int nameX))
                    {
                        Vector2Int namePosition = new Vector2Int(nameX, nameY); // Convert to X,Y format
                        if (namePosition != jsonPosition)
                        {
                            Debug.Log($"[CellController_EXT] Y_X naming convention: GameObject {gameObject.name} expects position {namePosition}, JSON has {jsonPosition}");
                            // Use the position from the GameObject name for consistency
                            jsonPosition = namePosition;
                        }
                    }
                }
            }
            
            if (jsonPosition == currentPosition)
            {
                Debug.Log($"[CellController_EXT] Grid position already synchronized: {jsonPosition}");
                return;
            }
            
            // Update the base class grid position
            SetGridPosition(jsonPosition);
            
            Debug.Log($"[CellController_EXT] ✅ Synced grid position from JSON (Y_X logic): {currentPosition} → {jsonPosition}");
        }
        
        /// <summary>
        /// Sync grid position from CellJsonData to CellController_EXT (for prefab editing)
        /// This version works directly with the components on the same GameObject
        /// Uses Y_X naming convention logic
        /// </summary>
        [ContextMenu("Sync Grid Position from JSON (Prefab Safe)")]
        public void SyncGridPositionFromJsonPrefabSafe()
        {
            // Get CellJsonData component directly from this GameObject
            CellJsonData jsonData = GetComponent<CellJsonData>();
            if (jsonData == null)
            {
                Debug.LogWarning($"[CellController_EXT] No CellJsonData component found on {gameObject.name}");
                return;
            }
            
            if (!jsonData.HasJsonData())
            {
                Debug.LogWarning($"[CellController_EXT] No JSON data available on {gameObject.name}");
                return;
            }
            
            Vector2Int jsonPosition = jsonData.NodePosition;
            Vector2Int currentPosition = base.GridPosition;
            
            // Apply Y_X naming convention logic
            // If the GameObject name follows Cell_Y_X format, we need to ensure consistency
            if (gameObject.name.StartsWith("Cell_") && gameObject.name.Contains("_"))
            {
                // Extract Y and X from GameObject name (Cell_Y_X format)
                string[] nameParts = gameObject.name.Split('_');
                if (nameParts.Length >= 3)
                {
                    if (int.TryParse(nameParts[1], out int nameY) && int.TryParse(nameParts[2], out int nameX))
                    {
                        Vector2Int namePosition = new Vector2Int(nameX, nameY); // Convert to X,Y format
                        if (namePosition != jsonPosition)
                        {
                            Debug.Log($"[CellController_EXT] Y_X naming convention (Prefab Safe): GameObject {gameObject.name} expects position {namePosition}, JSON has {jsonPosition}");
                            // Use the position from the GameObject name for consistency
                            jsonPosition = namePosition;
                        }
                    }
                }
            }
            
            if (jsonPosition == currentPosition)
            {
                Debug.Log($"[CellController_EXT] Grid position already synchronized: {jsonPosition}");
                return;
            }
            
            // Update the base class grid position
            SetGridPosition(jsonPosition);
            
            Debug.Log($"[CellController_EXT] ✅ Synced grid position from JSON (Y_X logic, Prefab Safe): {currentPosition} → {jsonPosition}");
        }
        
        /// <summary>
        /// Set the grid position (for internal use)
        /// </summary>
        private void SetGridPosition(Vector2Int newPosition)
        {
            // Use reflection to set the protected gridPosition field
            var field = typeof(CellController).GetField("gridPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(this, newPosition);
            }
            else
            {
                Debug.LogError($"[CellController_EXT] Could not access gridPosition field in base class");
            }
        }
        
        /// <summary>
        /// Sync all data from CellJsonData to CellController_EXT
        /// </summary>
        [ContextMenu("Sync All Data from JSON")]
        public void SyncAllDataFromJson()
        {
            if (cellJsonData == null)
            {
                Debug.LogWarning($"[CellController_EXT] No CellJsonData component found on {gameObject.name}");
                return;
            }
            
            if (!cellJsonData.HasJsonData())
            {
                Debug.LogWarning($"[CellController_EXT] No JSON data available on {gameObject.name}");
                return;
            }
            
            Debug.Log($"[CellController_EXT] Syncing all data from JSON for {gameObject.name}...");
            
            // Sync grid position
            SyncGridPositionFromJsonData();
            
            // Update visual state
            UpdateVisualState();
            
            Debug.Log($"[CellController_EXT] ✅ All data synced from JSON: {NodeName} at {GridPosition}");
        }
        
        /// <summary>
        /// Add CellJsonData components to all cells in the board
        /// </summary>
        [ContextMenu("Add CellJsonData to All Cells in Board")]
        public void AddCellJsonDataToAllCellsInBoard()
        {
            Debug.Log($"[CellController_EXT] Adding CellJsonData components to all cells in board...");
            
            // Find the ExtensionBoardCells parent
            Transform extensionBoardCells = transform.parent;
            while (extensionBoardCells != null && !extensionBoardCells.name.Contains("ExtensionBoardCells"))
            {
                extensionBoardCells = extensionBoardCells.parent;
            }
            
            if (extensionBoardCells == null || !extensionBoardCells.name.Contains("ExtensionBoardCells"))
            {
                Debug.LogError($"[CellController_EXT] Could not find ExtensionBoardCells parent for {gameObject.name}");
                return;
            }
            
            Debug.Log($"[CellController_EXT] Found ExtensionBoardCells: {extensionBoardCells.name}");
            
            // Get all CellController_EXT components in the board
            CellController_EXT[] allCells = extensionBoardCells.GetComponentsInChildren<CellController_EXT>();
            Debug.Log($"[CellController_EXT] Found {allCells.Length} CellController_EXT components");
            
            int addedCount = 0;
            int alreadyExistsCount = 0;
            
            foreach (CellController_EXT cell in allCells)
            {
                // Check if CellJsonData already exists
                CellJsonData existingJsonData = cell.GetComponent<CellJsonData>();
                if (existingJsonData != null)
                {
                    alreadyExistsCount++;
                    continue;
                }
                
                // Add CellJsonData component
                CellJsonData newJsonData = cell.gameObject.AddComponent<CellJsonData>();
                
                // Set up the component with basic data
                newJsonData.SetBoardId(extensionBoardCells.parent.name);
                
                // Try to find a JSON file for this board
                TextAsset jsonFile = FindJsonFileForBoard(extensionBoardCells.parent.name);
                if (jsonFile != null)
                {
                    newJsonData.SetJsonFile(jsonFile);
                    Debug.Log($"[CellController_EXT] Added CellJsonData to {cell.gameObject.name} with JSON file: {jsonFile.name}");
                }
                else
                {
                    Debug.LogWarning($"[CellController_EXT] No JSON file found for board {extensionBoardCells.parent.name}");
                }
                
                addedCount++;
            }
            
            Debug.Log($"[CellController_EXT] ✅ CellJsonData addition complete: {addedCount} added, {alreadyExistsCount} already existed");
        }
        
        /// <summary>
        /// Find JSON file for a specific board
        /// </summary>
        private TextAsset FindJsonFileForBoard(string boardName)
        {
            // Try to find JSON files that match the board name
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                
                // Check if the file name matches the board name or contains relevant keywords
                if (fileName.ToLower().Contains(boardName.ToLower()) || 
                    fileName.ToLower().Contains("t1") ||
                    fileName.ToLower().Contains("physical") ||
                    fileName.ToLower().Contains("cold") ||
                    fileName.ToLower().Contains("fire") ||
                    fileName.ToLower().Contains("lightning"))
                {
                    TextAsset jsonFile = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (jsonFile != null)
                    {
                        Debug.Log($"[CellController_EXT] Found JSON file: {fileName} for board {boardName}");
                        return jsonFile;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Sync grid positions for all cells in the parent board
        /// </summary>
        [ContextMenu("Sync All Cells in Board from JSON")]
        public void SyncAllCellsInBoardFromJson()
        {
            Debug.Log($"[CellController_EXT] Syncing all cells in board from JSON...");
            
            // Find the ExtensionBoardCells parent
            Transform extensionBoardCells = transform.parent;
            while (extensionBoardCells != null && !extensionBoardCells.name.Contains("ExtensionBoardCells"))
            {
                extensionBoardCells = extensionBoardCells.parent;
            }
            
            if (extensionBoardCells == null || !extensionBoardCells.name.Contains("ExtensionBoardCells"))
            {
                Debug.LogError($"[CellController_EXT] Could not find ExtensionBoardCells parent for {gameObject.name}");
                return;
            }
            
            Debug.Log($"[CellController_EXT] Found ExtensionBoardCells: {extensionBoardCells.name}");
            
            // Get all CellController_EXT components in the board
            CellController_EXT[] allCells = extensionBoardCells.GetComponentsInChildren<CellController_EXT>();
            Debug.Log($"[CellController_EXT] Found {allCells.Length} CellController_EXT components");
            
            int syncedCount = 0;
            int skippedCount = 0;
            
            foreach (CellController_EXT cell in allCells)
            {
                if (cell.cellJsonData != null && cell.cellJsonData.HasJsonData())
                {
                    cell.SyncGridPositionFromJsonData();
                    syncedCount++;
                }
                else
                {
                    Debug.LogWarning($"[CellController_EXT] Skipping {cell.gameObject.name} - no JSON data");
                    skippedCount++;
                }
            }
            
            Debug.Log($"[CellController_EXT] ✅ Board sync complete: {syncedCount} synced, {skippedCount} skipped");
        }
        
        /// <summary>
        /// Sync all cells in the prefab from JSON data (prefab-safe version)
        /// This works by finding all CellController_EXT components in the scene
        /// </summary>
        [ContextMenu("Sync All Cells in Prefab from JSON")]
        public void SyncAllCellsInPrefabFromJson()
        {
            Debug.Log($"[CellController_EXT] Syncing all cells in prefab from JSON...");
            
            // Find all CellController_EXT components in the scene
            CellController_EXT[] allCells = FindObjectsByType<CellController_EXT>(FindObjectsSortMode.None);
            Debug.Log($"[CellController_EXT] Found {allCells.Length} CellController_EXT components in scene");
            
            int syncedCount = 0;
            int skippedCount = 0;
            int noJsonDataCount = 0;
            
            foreach (CellController_EXT cell in allCells)
            {
                // Get CellJsonData component directly from this GameObject
                CellJsonData jsonData = cell.GetComponent<CellJsonData>();
                if (jsonData == null)
                {
                    noJsonDataCount++;
                    continue;
                }
                
                if (!jsonData.HasJsonData())
                {
                    noJsonDataCount++;
                    continue;
                }
                
                // Sync the grid position
                Vector2Int jsonPosition = jsonData.NodePosition;
                Vector2Int currentPosition = cell.GridPosition;
                
                if (jsonPosition == currentPosition)
                {
                    skippedCount++;
                    continue;
                }
                
                // Update the grid position
                cell.SetGridPosition(jsonPosition);
                syncedCount++;
                
                Debug.Log($"[CellController_EXT] ✅ Synced {cell.gameObject.name}: {currentPosition} → {jsonPosition}");
            }
            
            Debug.Log($"[CellController_EXT] ✅ Prefab sync complete: {syncedCount} synced, {skippedCount} already synced, {noJsonDataCount} no JSON data");
        }
        
        /// <summary>
        /// Complete setup: Add CellJsonData components and sync all data
        /// </summary>
        [ContextMenu("Complete Board Setup (Add JSON + Sync)")]
        public void CompleteBoardSetup()
        {
            Debug.Log($"[CellController_EXT] Starting complete board setup...");
            
            // Step 1: Add CellJsonData components
            AddCellJsonDataToAllCellsInBoard();
            
            // Step 2: Load JSON data for all cells
            LoadJsonDataForAllCellsInBoard();
            
            // Step 3: Sync all data
            SyncAllCellsInBoardFromJson();
            
            Debug.Log($"[CellController_EXT] ✅ Complete board setup finished!");
        }
        
        /// <summary>
        /// Load JSON data for all cells in the board
        /// </summary>
        private void LoadJsonDataForAllCellsInBoard()
        {
            Debug.Log($"[CellController_EXT] Loading JSON data for all cells in board...");
            
            // Find the ExtensionBoardCells parent
            Transform extensionBoardCells = transform.parent;
            while (extensionBoardCells != null && !extensionBoardCells.name.Contains("ExtensionBoardCells"))
            {
                extensionBoardCells = extensionBoardCells.parent;
            }
            
            if (extensionBoardCells == null || !extensionBoardCells.name.Contains("ExtensionBoardCells"))
            {
                Debug.LogError($"[CellController_EXT] Could not find ExtensionBoardCells parent for {gameObject.name}");
                return;
            }
            
            // Get all CellJsonData components in the board
            CellJsonData[] allJsonData = extensionBoardCells.GetComponentsInChildren<CellJsonData>();
            Debug.Log($"[CellController_EXT] Found {allJsonData.Length} CellJsonData components");
            
            int loadedCount = 0;
            int skippedCount = 0;
            
            foreach (CellJsonData jsonData in allJsonData)
            {
                if (jsonData.HasJsonData())
                {
                    // JSON data already loaded
                    skippedCount++;
                    continue;
                }
                
                // Try to load JSON data for this cell
                jsonData.LoadJsonDataForThisCell();
                loadedCount++;
            }
            
            Debug.Log($"[CellController_EXT] ✅ JSON data loading complete: {loadedCount} loaded, {skippedCount} already had data");
        }
        
        /// <summary>
        /// Debug method to show extension board cell state
        /// </summary>
        [ContextMenu("Debug Extension Cell State")]
        public void DebugExtensionCellState()
        {
            Debug.Log($"[CellController_EXT] Extension Board Cell State:");
            Debug.Log($"  - GameObject: {gameObject.name}");
            Debug.Log($"  - Base Grid Position: {base.GridPosition}");
            Debug.Log($"  - Data-Driven Grid Position: {GridPosition}");
            Debug.Log($"  - Data Source: {(cellJsonData != null ? "CellJsonData" : "Base Class")}");
            Debug.Log($"  - Node Name: {NodeName}");
            Debug.Log($"  - Node Type: {NodeType}");
            Debug.Log($"  - Available: {isAvailable}");
            Debug.Log($"  - Unlocked: {isUnlocked}");
            Debug.Log($"  - Purchased: {isPurchased}");
            Debug.Log($"  - IsExtensionPoint: {isExtensionPoint}");
            if (cellJsonData != null && cellJsonData.HasJsonData())
            {
                Debug.Log($"  - JSON Position: {cellJsonData.NodePosition}");
                Debug.Log($"  - JSON Data: {cellJsonData.NodeName} (Cost: {NodeCost}, Rank: {CurrentRank}/{MaxRank})");
                Debug.Log($"  - Position Match: {base.GridPosition == cellJsonData.NodePosition}");
            }
        }
    }
}
