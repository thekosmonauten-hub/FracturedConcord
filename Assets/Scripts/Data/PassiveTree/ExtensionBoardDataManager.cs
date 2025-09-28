using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Centralized data manager for extension boards and their extension points
    /// Provides quick access to board data, extension point states, and allocation management
    /// </summary>
    [System.Serializable]
    public class ExtensionBoardData
    {
        public string boardId;
        public string boardName;
        public Vector2Int gridPosition;
        public Vector3 worldPosition;
        public bool isActive;
        public List<ExtensionPointData> extensionPoints;
        public System.DateTime createdTime;
        
        public ExtensionBoardData(string id, string name, Vector2Int gridPos, Vector3 worldPos)
        {
            boardId = id;
            boardName = name;
            gridPosition = gridPos;
            worldPosition = worldPos;
            isActive = true;
            extensionPoints = new List<ExtensionPointData>();
            createdTime = System.DateTime.Now;
        }
    }
    
    /// <summary>
    /// Data structure for extension point information
    /// </summary>
    [System.Serializable]
    public class ExtensionPointData
    {
        public string pointId;
        public string pointName; // e.g., "Extension_East", "Extension_West"
        public Vector2Int gridPosition;
        public Vector3 worldPosition;
        public bool isPurchased;
        public bool isUnlocked;
        public bool isAvailable;
        public string parentBoardId;
        
        public ExtensionPointData(string id, string name, Vector2Int gridPos, Vector3 worldPos, string parentBoard)
        {
            pointId = id;
            pointName = name;
            gridPosition = gridPos;
            worldPosition = worldPos;
            isPurchased = false;
            isUnlocked = false;
            isAvailable = true;
            parentBoardId = parentBoard;
        }
    }
    
    /// <summary>
    /// Centralized manager for extension board data
    /// </summary>
    public class ExtensionBoardDataManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Data storage
        private Dictionary<string, ExtensionBoardData> extensionBoards = new Dictionary<string, ExtensionBoardData>();
        private Dictionary<string, ExtensionPointData> extensionPoints = new Dictionary<string, ExtensionPointData>();
        
        // Singleton pattern
        private static ExtensionBoardDataManager _instance;
        public static ExtensionBoardDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ExtensionBoardDataManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ExtensionBoardDataManager");
                        _instance = go.AddComponent<ExtensionBoardDataManager>();
                    }
                }
                return _instance;
            }
        }
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        #region Board Management
        
        /// <summary>
        /// Register a new extension board
        /// </summary>
        public void RegisterExtensionBoard(string boardId, string boardName, Vector2Int gridPosition, Vector3 worldPosition)
        {
            if (extensionBoards.ContainsKey(boardId))
            {
                Debug.LogWarning($"[ExtensionBoardDataManager] Board {boardId} already exists, updating data");
                extensionBoards[boardId].boardName = boardName;
                extensionBoards[boardId].gridPosition = gridPosition;
                extensionBoards[boardId].worldPosition = worldPosition;
                extensionBoards[boardId].isActive = true;
            }
            else
            {
                ExtensionBoardData boardData = new ExtensionBoardData(boardId, boardName, gridPosition, worldPosition);
                extensionBoards[boardId] = boardData;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardDataManager] ✅ Registered extension board: {boardName} at {gridPosition}");
                }
            }
            
            // Notify the board selection tracker about the created board
            NotifyBoardSelectionTracker(boardName, boardId);
        }
        
        /// <summary>
        /// Notify the board selection tracker about a created board
        /// </summary>
        private void NotifyBoardSelectionTracker(string boardName, string boardId)
        {
            var tracker = BoardSelectionTracker.Instance;
            if (tracker != null)
            {
                // Try to find the corresponding BoardData for this board
                BoardData boardData = FindBoardDataByName(boardName);
                if (boardData != null)
                {
                    // Extract tier and theme from board name or use defaults
                    int tier = ExtractTierFromBoardName(boardName);
                    BoardTheme theme = ExtractThemeFromBoardName(boardName);
                    
                    tracker.RegisterCreatedBoard(boardName, boardData, tier, theme);
                }
                else if (showDebugInfo)
                {
                    Debug.LogWarning($"[ExtensionBoardDataManager] Could not find BoardData for board: {boardName}");
                }
            }
        }
        
        /// <summary>
        /// Find BoardData by board name
        /// </summary>
        private BoardData FindBoardDataByName(string boardName)
        {
            // Search for BoardData assets in the project
            BoardData[] allBoardData = Resources.FindObjectsOfTypeAll<BoardData>();
            
            foreach (BoardData boardData in allBoardData)
            {
                if (boardData != null && boardData.BoardName.Equals(boardName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return boardData;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Extract tier from board name (e.g., "T1_Fire_Board" -> 1)
        /// </summary>
        private int ExtractTierFromBoardName(string boardName)
        {
            // Look for T1, T2, T3, etc. in the board name
            if (boardName.ToLower().Contains("t1")) return 1;
            if (boardName.ToLower().Contains("t2")) return 2;
            if (boardName.ToLower().Contains("t3")) return 3;
            if (boardName.ToLower().Contains("t4")) return 4;
            if (boardName.ToLower().Contains("t5")) return 5;
            
            // Default to tier 1 if no tier found
            return 1;
        }
        
        /// <summary>
        /// Extract theme from board name
        /// </summary>
        private BoardTheme ExtractThemeFromBoardName(string boardName)
        {
            string lowerName = boardName.ToLower();
            
            // Check for specific theme keywords
            if (lowerName.Contains("fire")) return BoardTheme.Fire;
            if (lowerName.Contains("cold") || lowerName.Contains("frozen")) return BoardTheme.Cold;
            if (lowerName.Contains("lightning")) return BoardTheme.Lightning;
            if (lowerName.Contains("chaos")) return BoardTheme.Chaos;
            if (lowerName.Contains("physical")) return BoardTheme.Physical;
            if (lowerName.Contains("life")) return BoardTheme.Life;
            if (lowerName.Contains("armor")) return BoardTheme.Armor;
            if (lowerName.Contains("evasion")) return BoardTheme.Evasion;
            if (lowerName.Contains("critical")) return BoardTheme.Critical;
            if (lowerName.Contains("speed")) return BoardTheme.Speed;
            if (lowerName.Contains("utility")) return BoardTheme.Utility;
            if (lowerName.Contains("elemental")) return BoardTheme.Elemental;
            if (lowerName.Contains("keystone")) return BoardTheme.Keystone;
            
            // Check for mastery themes (common in passive trees)
            if (lowerName.Contains("mastery")) 
            {
                // Try to determine the mastery type
                if (lowerName.Contains("frozen") || lowerName.Contains("cold")) return BoardTheme.Cold;
                if (lowerName.Contains("fire") || lowerName.Contains("burning")) return BoardTheme.Fire;
                if (lowerName.Contains("lightning") || lowerName.Contains("electric")) return BoardTheme.Lightning;
                if (lowerName.Contains("chaos") || lowerName.Contains("void")) return BoardTheme.Chaos;
                if (lowerName.Contains("physical") || lowerName.Contains("strength")) return BoardTheme.Physical;
                if (lowerName.Contains("life") || lowerName.Contains("vitality")) return BoardTheme.Life;
                if (lowerName.Contains("armor") || lowerName.Contains("defense")) return BoardTheme.Armor;
                if (lowerName.Contains("evasion") || lowerName.Contains("dodge")) return BoardTheme.Evasion;
                if (lowerName.Contains("critical") || lowerName.Contains("crit")) return BoardTheme.Critical;
                if (lowerName.Contains("speed") || lowerName.Contains("haste")) return BoardTheme.Speed;
                if (lowerName.Contains("utility") || lowerName.Contains("support")) return BoardTheme.Utility;
                if (lowerName.Contains("elemental")) return BoardTheme.Elemental;
                if (lowerName.Contains("keystone")) return BoardTheme.Keystone;
            }
            
            // Default to General if no theme found
            return BoardTheme.General;
        }
        
        /// <summary>
        /// Unregister an extension board
        /// </summary>
        public void UnregisterExtensionBoard(string boardId)
        {
            if (extensionBoards.ContainsKey(boardId))
            {
                string boardName = extensionBoards[boardId].boardName;
                
                // Remove all extension points for this board
                var pointsToRemove = extensionPoints.Values.Where(p => p.parentBoardId == boardId).ToList();
                foreach (var point in pointsToRemove)
                {
                    extensionPoints.Remove(point.pointId);
                }
                
                extensionBoards.Remove(boardId);
                
                // Notify the board selection tracker about the removed board
                var tracker = BoardSelectionTracker.Instance;
                if (tracker != null)
                {
                    tracker.UnregisterBoard(boardName);
                }
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardDataManager] ✅ Unregistered extension board: {boardId} ({boardName})");
                }
            }
        }
        
        /// <summary>
        /// Get all extension boards
        /// </summary>
        public Dictionary<string, ExtensionBoardData> GetAllExtensionBoards()
        {
            return new Dictionary<string, ExtensionBoardData>(extensionBoards);
        }
        
        /// <summary>
        /// Get a specific extension board
        /// </summary>
        public ExtensionBoardData GetExtensionBoard(string boardId)
        {
            return extensionBoards.ContainsKey(boardId) ? extensionBoards[boardId] : null;
        }
        
        #endregion
        
        #region Extension Point Management
        
        /// <summary>
        /// Register a new extension point
        /// </summary>
        public void RegisterExtensionPoint(string pointId, string pointName, Vector2Int gridPosition, Vector3 worldPosition, string parentBoardId)
        {
            ExtensionPointData pointData = new ExtensionPointData(pointId, pointName, gridPosition, worldPosition, parentBoardId);
            extensionPoints[pointId] = pointData;
            
            // Add to parent board's extension points list
            if (extensionBoards.ContainsKey(parentBoardId))
            {
                extensionBoards[parentBoardId].extensionPoints.Add(pointData);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardDataManager] ✅ Registered extension point: {pointName} at {gridPosition} on board {parentBoardId}");
            }
        }
        
        /// <summary>
        /// Update extension point state
        /// </summary>
        public void UpdateExtensionPointState(string pointId, bool isPurchased, bool isUnlocked, bool isAvailable)
        {
            if (extensionPoints.ContainsKey(pointId))
            {
                extensionPoints[pointId].isPurchased = isPurchased;
                extensionPoints[pointId].isUnlocked = isUnlocked;
                extensionPoints[pointId].isAvailable = isAvailable;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardDataManager] ✅ Updated extension point {pointId}: Purchased={isPurchased}, Unlocked={isUnlocked}, Available={isAvailable}");
                }
            }
            else
            {
                Debug.LogWarning($"[ExtensionBoardDataManager] ❌ Extension point {pointId} not found");
            }
        }
        
        /// <summary>
        /// Get all extension points
        /// </summary>
        public Dictionary<string, ExtensionPointData> GetAllExtensionPoints()
        {
            return new Dictionary<string, ExtensionPointData>(extensionPoints);
        }
        
        /// <summary>
        /// Get extension points for a specific board
        /// </summary>
        public List<ExtensionPointData> GetExtensionPointsForBoard(string boardId)
        {
            return extensionPoints.Values.Where(p => p.parentBoardId == boardId).ToList();
        }
        
        /// <summary>
        /// Find extension point by position
        /// </summary>
        public ExtensionPointData FindExtensionPointByPosition(Vector2Int gridPosition)
        {
            return extensionPoints.Values.FirstOrDefault(p => p.gridPosition == gridPosition);
        }
        
        #endregion
        
        #region Allocation Management
        
        /// <summary>
        /// Check if a position can be allocated (has adjacent purchased nodes)
        /// </summary>
        public bool CanAllocatePosition(Vector2Int gridPosition, string boardId)
        {
            // Check if there's a purchased node adjacent to this position
            Vector2Int[] adjacentPositions = {
                gridPosition + Vector2Int.up,
                gridPosition + Vector2Int.down,
                gridPosition + Vector2Int.left,
                gridPosition + Vector2Int.right
            };
            
            foreach (Vector2Int adjPos in adjacentPositions)
            {
                // Check core board for adjacent purchased nodes
                if (IsPositionPurchasedOnCoreBoard(adjPos))
                {
                    return true;
                }
                
                // Check extension boards for adjacent purchased nodes
                if (IsPositionPurchasedOnExtensionBoards(adjPos))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a position is purchased on the core board
        /// </summary>
        private bool IsPositionPurchasedOnCoreBoard(Vector2Int gridPosition)
        {
            // This would need to be implemented with PassiveTreeManager integration
            // For now, return false as placeholder
            return false;
        }
        
        /// <summary>
        /// Check if a position is purchased on extension boards
        /// </summary>
        private bool IsPositionPurchasedOnExtensionBoards(Vector2Int gridPosition)
        {
            var point = FindExtensionPointByPosition(gridPosition);
            return point != null && point.isPurchased;
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// Debug all extension boards and their extension points
        /// </summary>
        [ContextMenu("Debug All Extension Board Data")]
        public void DebugAllExtensionBoardData()
        {
            Debug.Log("=== EXTENSION BOARD DATA DEBUG ===");
            Debug.Log($"Total Extension Boards: {extensionBoards.Count}");
            Debug.Log($"Total Extension Points: {extensionPoints.Count}");
            
            foreach (var board in extensionBoards.Values)
            {
                Debug.Log($"Board: {board.boardName} (ID: {board.boardId})");
                Debug.Log($"  Grid Position: {board.gridPosition}");
                Debug.Log($"  World Position: {board.worldPosition}");
                Debug.Log($"  Is Active: {board.isActive}");
                Debug.Log($"  Extension Points: {board.extensionPoints.Count}");
                
                foreach (var point in board.extensionPoints)
                {
                    Debug.Log($"    Point: {point.pointName} at {point.gridPosition}");
                    Debug.Log($"      Purchased: {point.isPurchased}, Unlocked: {point.isUnlocked}, Available: {point.isAvailable}");
                }
            }
            
            Debug.Log("=== EXTENSION BOARD DATA DEBUG COMPLETE ===");
        }
        
        /// <summary>
        /// Clear all extension board data
        /// </summary>
        [ContextMenu("Clear All Extension Board Data")]
        public void ClearAllExtensionBoardData()
        {
            extensionBoards.Clear();
            extensionPoints.Clear();
            Debug.Log("[ExtensionBoardDataManager] ✅ Cleared all extension board data");
        }
        
        /// <summary>
        /// Test extension board data management
        /// </summary>
        [ContextMenu("Test Extension Board Data Management")]
        public void TestExtensionBoardDataManagement()
        {
            Debug.Log("=== TESTING EXTENSION BOARD DATA MANAGEMENT ===");
            
            // Test board registration
            RegisterExtensionBoard("test_board_1", "Test Board 1", new Vector2Int(1, 0), new Vector3(100, 0, 0));
            RegisterExtensionBoard("test_board_2", "Test Board 2", new Vector2Int(-1, 0), new Vector3(-100, 0, 0));
            
            // Test extension point registration
            RegisterExtensionPoint("test_point_1", "Extension_East", new Vector2Int(3, 6), new Vector3(300, 0, 600), "test_board_1");
            RegisterExtensionPoint("test_point_2", "Extension_West", new Vector2Int(0, 3), new Vector3(0, 0, 300), "test_board_2");
            
            // Test state updates
            UpdateExtensionPointState("test_point_1", true, true, true);
            UpdateExtensionPointState("test_point_2", false, true, true);
            
            // Debug the data
            DebugAllExtensionBoardData();
            
            Debug.Log("=== EXTENSION BOARD DATA MANAGEMENT TEST COMPLETE ===");
        }
        
        #endregion
    }
}



