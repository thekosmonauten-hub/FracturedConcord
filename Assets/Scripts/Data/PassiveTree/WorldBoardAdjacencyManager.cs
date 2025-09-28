using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Manages world board adjacency logic for seamless pathing between adjacent boards
    /// When a player allocates an extension point on one board, automatically allocates
    /// the corresponding extension point on adjacent boards to ensure full board pathing
    /// </summary>
    public class WorldBoardAdjacencyManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Singleton instance
        private static WorldBoardAdjacencyManager _instance;
        public static WorldBoardAdjacencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<WorldBoardAdjacencyManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("WorldBoardAdjacencyManager");
                        _instance = go.AddComponent<WorldBoardAdjacencyManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // Board position tracking
        private Dictionary<Vector2Int, GameObject> worldBoardPositions = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, string> boardNames = new Dictionary<Vector2Int, string>();
        
        // Adjacency directions (North, South, East, West, and diagonals)
        private readonly Vector2Int[] adjacencyDirections = new Vector2Int[]
        {
            Vector2Int.up,    // North
            Vector2Int.down,  // South
            Vector2Int.right, // East
            Vector2Int.left,  // West
            Vector2Int.up + Vector2Int.right,    // North-East
            Vector2Int.up + Vector2Int.left,     // North-West
            Vector2Int.down + Vector2Int.right, // South-East
            Vector2Int.down + Vector2Int.left    // South-West
        };
        
        // Cardinal directions only (for extension point adjacency)
        private readonly Vector2Int[] cardinalDirections = new Vector2Int[]
        {
            Vector2Int.up,    // North
            Vector2Int.down,  // South
            Vector2Int.right, // East
            Vector2Int.left   // West
        };
        
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
        
        /// <summary>
        /// Register a board at a specific world grid position
        /// </summary>
        public void RegisterBoard(Vector2Int worldPosition, GameObject boardObject, string boardName)
        {
            worldBoardPositions[worldPosition] = boardObject;
            boardNames[worldPosition] = boardName;
            
            if (showDebugInfo)
            {
                Debug.Log($"[WorldBoardAdjacencyManager] 🌍 Registered board '{boardName}' (GameObject: '{boardObject.name}') at world position {worldPosition}");
            }
        }
        
        /// <summary>
        /// Unregister a board from the world grid
        /// </summary>
        public void UnregisterBoard(Vector2Int worldPosition)
        {
            if (worldBoardPositions.ContainsKey(worldPosition))
            {
                string boardName = boardNames[worldPosition];
                worldBoardPositions.Remove(worldPosition);
                boardNames.Remove(worldPosition);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[WorldBoardAdjacencyManager] Unregistered board '{boardName}' from world position {worldPosition}");
                }
            }
        }
        
        /// <summary>
        /// Get all adjacent boards to a given world position
        /// </summary>
        public List<Vector2Int> GetAdjacentBoardPositions(Vector2Int worldPosition)
        {
            return GetAdjacentBoardPositions(worldPosition, false);
        }
        
        /// <summary>
        /// Get all adjacent boards to a given world position
        /// </summary>
        public List<Vector2Int> GetAdjacentBoardPositions(Vector2Int worldPosition, bool cardinalOnly)
        {
            List<Vector2Int> adjacentPositions = new List<Vector2Int>();
            
            Debug.Log($"[WorldBoardAdjacencyManager] 🔍 Checking adjacency for position {worldPosition} (cardinal only: {cardinalOnly})");
            Debug.Log($"[WorldBoardAdjacencyManager] 🔍 Available positions: {string.Join(", ", worldBoardPositions.Keys)}");
            
            Vector2Int[] directionsToCheck = cardinalOnly ? cardinalDirections : adjacencyDirections;
            
            foreach (Vector2Int direction in directionsToCheck)
            {
                Vector2Int adjacentPosition = worldPosition + direction;
                Debug.Log($"[WorldBoardAdjacencyManager] 🔍 Checking direction {direction} -> position {adjacentPosition} (exists: {worldBoardPositions.ContainsKey(adjacentPosition)})");
                if (worldBoardPositions.ContainsKey(adjacentPosition))
                {
                    // Skip if it's the same board (prevent self-adjacency)
                    if (adjacentPosition != worldPosition)
                    {
                        adjacentPositions.Add(adjacentPosition);
                        Debug.Log($"[WorldBoardAdjacencyManager] ✅ Found adjacent board at {adjacentPosition}");
                    }
                    else
                    {
                        Debug.LogWarning($"[WorldBoardAdjacencyManager] ⚠️ Skipping self-adjacency at {adjacentPosition}");
                    }
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[WorldBoardAdjacencyManager] Found {adjacentPositions.Count} adjacent boards for position {worldPosition}");
                foreach (Vector2Int pos in adjacentPositions)
                {
                    Debug.Log($"  - Adjacent board '{boardNames[pos]}' at {pos}");
                }
            }
            
            return adjacentPositions;
        }
        
        /// <summary>
        /// Get the direction from one board to another
        /// </summary>
        public Vector2Int GetDirectionToAdjacentBoard(Vector2Int fromPosition, Vector2Int toPosition)
        {
            Vector2Int direction = toPosition - fromPosition;
            
            // Normalize to unit direction
            if (direction.x != 0) direction.x = direction.x / Mathf.Abs(direction.x);
            if (direction.y != 0) direction.y = direction.y / Mathf.Abs(direction.y);
            
            return direction;
        }
        
        /// <summary>
        /// Handle extension point allocation with adjacency logic
        /// When an extension point is allocated on one board, automatically allocate
        /// the corresponding extension point on adjacent boards
        /// </summary>
        public void HandleExtensionPointAllocation(Vector2Int boardWorldPosition, Vector2Int cellPosition, string extensionPointName)
        {
            Debug.Log($"[WorldBoardAdjacencyManager] 🌍 ADJACENCY: Handling extension point allocation: {extensionPointName} at cell {cellPosition} on board at {boardWorldPosition}");
            
            if (showDebugInfo)
            {
                Debug.Log($"[WorldBoardAdjacencyManager] Handling extension point allocation: {extensionPointName} at cell {cellPosition} on board at {boardWorldPosition}");
            }
            
            // Get all adjacent boards (use cardinal directions only for extension points)
            List<Vector2Int> adjacentPositions = GetAdjacentBoardPositions(boardWorldPosition, true);
            
            Debug.Log($"[WorldBoardAdjacencyManager] 🌍 ADJACENCY: Found {adjacentPositions.Count} adjacent boards for position {boardWorldPosition}");
            
            // Show all registered boards for debugging
            Debug.Log($"[WorldBoardAdjacencyManager] 🌍 ADJACENCY: All registered boards:");
            foreach (var kvp in worldBoardPositions)
            {
                Debug.Log($"  - Board '{boardNames[kvp.Key]}' at world position {kvp.Key}");
            }
            
            foreach (Vector2Int adjacentPosition in adjacentPositions)
            {
                // Get the direction from current board to adjacent board
                Vector2Int direction = GetDirectionToAdjacentBoard(boardWorldPosition, adjacentPosition);
                
                // Calculate the corresponding cell position on the adjacent board
                Vector2Int correspondingCellPosition = GetCorrespondingCellPosition(cellPosition, direction);
                
                // Get the corresponding extension point name
                string correspondingExtensionPointName = GetCorrespondingExtensionPointName(extensionPointName, direction);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[WorldBoardAdjacencyManager] 🌍 ADJACENCY: Processing adjacent board '{boardNames[adjacentPosition]}' at {adjacentPosition}");
                    Debug.Log($"  - Original cell position: {cellPosition}");
                    Debug.Log($"  - Direction: {direction}");
                    Debug.Log($"  - Corresponding cell position: {correspondingCellPosition}");
                    Debug.Log($"  - Original extension point: {extensionPointName}");
                    Debug.Log($"  - Corresponding extension point: {correspondingExtensionPointName}");
                }
                
                // Allocate the corresponding extension point on the adjacent board
                AllocateCorrespondingExtensionPoint(adjacentPosition, correspondingCellPosition, correspondingExtensionPointName);
            }
        }
        
        /// <summary>
        /// Calculate the corresponding cell position on an adjacent board
        /// This should return the position that corresponds to the original cell position
        /// on the opposite edge of the adjacent board
        /// </summary>
        private Vector2Int GetCorrespondingCellPosition(Vector2Int originalCellPosition, Vector2Int direction)
        {
            // For a 7x7 board (0-6), we need to map the original position to the opposite edge
            // The key insight: if we're going in a direction, we need the same relative position
            // on the opposite edge of the target board
            
            if (direction == Vector2Int.up) // North -> South edge
            {
                // If original is at (0,3), target should be at (0,3) on South edge
                // But South edge is at Y=0, so we keep X the same
                return new Vector2Int(originalCellPosition.x, 0);
            }
            else if (direction == Vector2Int.down) // South -> North edge
            {
                // If original is at (0,3), target should be at (0,3) on North edge
                // But North edge is at Y=6, so we keep X the same
                return new Vector2Int(originalCellPosition.x, 6);
            }
            else if (direction == Vector2Int.right) // East -> West edge
            {
                // If original is at (0,3), target should be at (0,3) on West edge
                // But West edge is at X=0, so we keep Y the same
                return new Vector2Int(0, originalCellPosition.y);
            }
            else if (direction == Vector2Int.left) // West -> East edge
            {
                // If original is at (0,3), target should be at (0,3) on East edge
                // But East edge is at X=6, so we keep Y the same
                return new Vector2Int(6, originalCellPosition.y);
            }
            // Handle diagonal directions - use the primary direction
            else if (direction == Vector2Int.up + Vector2Int.right) // North-East -> South-West
            {
                // For diagonal, we need to consider both X and Y
                // If going North-East, we need the South edge (Y=0) and West edge (X=0)
                return new Vector2Int(0, 0);
            }
            else if (direction == Vector2Int.up + Vector2Int.left) // North-West -> South-East
            {
                // If going North-West, we need the South edge (Y=0) and East edge (X=6)
                return new Vector2Int(6, 0);
            }
            else if (direction == Vector2Int.down + Vector2Int.right) // South-East -> North-West
            {
                // If going South-East, we need the North edge (Y=6) and West edge (X=0)
                return new Vector2Int(0, 6);
            }
            else if (direction == Vector2Int.down + Vector2Int.left) // South-West -> North-East
            {
                // If going South-West, we need the North edge (Y=6) and East edge (X=6)
                return new Vector2Int(6, 6);
            }
            
            return originalCellPosition; // Fallback
        }
        
        /// <summary>
        /// Get the corresponding extension point name for the adjacent board
        /// </summary>
        private string GetCorrespondingExtensionPointName(string originalExtensionPointName, Vector2Int direction)
        {
            // Map extension point names based on direction
            // When we go in a direction, we need the opposite extension point on the target board
            if (direction == Vector2Int.up) // North
            {
                return "Extension_South"; // Target board needs South to connect to our North
            }
            else if (direction == Vector2Int.down) // South
            {
                return "Extension_North"; // Target board needs North to connect to our South
            }
            else if (direction == Vector2Int.right) // East
            {
                return "Extension_West"; // Target board needs West to connect to our East
            }
            else if (direction == Vector2Int.left) // West
            {
                return "Extension_East"; // Target board needs East to connect to our West
            }
            // Handle diagonal directions - use the primary direction
            else if (direction == Vector2Int.up + Vector2Int.right) // North-East
            {
                return "Extension_South"; // Use South as primary for North-East
            }
            else if (direction == Vector2Int.up + Vector2Int.left) // North-West
            {
                return "Extension_South"; // Use South as primary for North-West
            }
            else if (direction == Vector2Int.down + Vector2Int.right) // South-East
            {
                return "Extension_North"; // Use North as primary for South-East
            }
            else if (direction == Vector2Int.down + Vector2Int.left) // South-West
            {
                return "Extension_North"; // Use North as primary for South-West
            }
            
            return originalExtensionPointName; // Fallback
        }
        
        /// <summary>
        /// Allocate the corresponding extension point on an adjacent board
        /// </summary>
        private void AllocateCorrespondingExtensionPoint(Vector2Int boardWorldPosition, Vector2Int cellPosition, string extensionPointName)
        {
            if (!worldBoardPositions.ContainsKey(boardWorldPosition))
            {
                Debug.LogWarning($"[WorldBoardAdjacencyManager] Board not found at world position {boardWorldPosition}");
                return;
            }
            
            GameObject boardObject = worldBoardPositions[boardWorldPosition];
            string boardName = boardNames[boardWorldPosition];
            
            if (showDebugInfo)
            {
                Debug.Log($"[WorldBoardAdjacencyManager] Allocating corresponding extension point '{extensionPointName}' at cell {cellPosition} on board '{boardName}'");
                Debug.Log($"[WorldBoardAdjacencyManager] Board GameObject name: '{boardObject.name}'");
                Debug.Log($"[WorldBoardAdjacencyManager] Board world position: {boardWorldPosition}");
                Debug.Log($"[WorldBoardAdjacencyManager] Board GameObject transform position: {boardObject.transform.position}");
                
                // Show all registered boards for debugging
                Debug.Log($"[WorldBoardAdjacencyManager] All registered boards:");
                foreach (var kvp in worldBoardPositions)
                {
                    Debug.Log($"  - Board '{boardNames[kvp.Key]}' (GameObject: '{kvp.Value.name}') at world position {kvp.Key}");
                }
            }
            
            // Find the cell controller for the corresponding position
            CellController[] allCells = boardObject.GetComponentsInChildren<CellController>();
            CellController targetCell = null;
            
            if (showDebugInfo)
            {
                Debug.Log($"[WorldBoardAdjacencyManager] Found {allCells.Length} cells on board '{boardName}'");
            }
            
            foreach (CellController cell in allCells)
            {
                if (cell.GridPosition == cellPosition)
                {
                    targetCell = cell;
                    if (showDebugInfo)
                    {
                        Debug.Log($"[WorldBoardAdjacencyManager] Found target cell at position {cellPosition} on board '{boardName}'");
                    }
                    break;
                }
            }
            
            if (targetCell != null)
            {
                // Check if this is the Coreboard (doesn't have ExtensionBoardController)
                bool isCoreBoard = boardName.StartsWith("CoreBoard") || boardObject.name.StartsWith("CoreBoard");
                
                if (isCoreBoard)
                {
                    // Handle Coreboard allocation directly through PassiveTreeManager
                    PassiveTreeManager treeManager = FindObjectOfType<PassiveTreeManager>();
                    if (treeManager != null)
                    {
                        // For Coreboard, we need to allocate the extension point directly
                        // The Coreboard uses the main PassiveTreeManager for extension point allocation
                        treeManager.AllocateExtensionPoint(targetCell);
                        
                        if (showDebugInfo)
                        {
                            Debug.Log($"[WorldBoardAdjacencyManager] ✅ Successfully allocated extension point '{extensionPointName}' at cell {cellPosition} on Coreboard '{boardName}' using PassiveTreeManager");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[WorldBoardAdjacencyManager] ❌ PassiveTreeManager not found for Coreboard allocation");
                    }
                }
                else
                {
                    // Handle Extension Board allocation through ExtensionBoardController
                    ExtensionBoardController boardController = boardObject.GetComponent<ExtensionBoardController>();
                    if (boardController != null)
                    {
                        // Find the PassiveTreeManager and call the proper allocation method
                        PassiveTreeManager treeManager = FindObjectOfType<PassiveTreeManager>();
                        if (treeManager != null)
                        {
                            // Call the public allocation method that sets IsAdjacent
                            treeManager.AllocateExtensionPoint(targetCell);
                            
                            if (showDebugInfo)
                            {
                                Debug.Log($"[WorldBoardAdjacencyManager] ✅ Successfully allocated extension point '{extensionPointName}' at cell {cellPosition} on extension board '{boardName}' using PassiveTreeManager");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[WorldBoardAdjacencyManager] ❌ PassiveTreeManager not found");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[WorldBoardAdjacencyManager] ❌ Extension board '{boardName}' does not have ExtensionBoardController component");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[WorldBoardAdjacencyManager] ❌ Could not find cell at position {cellPosition} on board '{boardName}'");
            }
        }
        
        /// <summary>
        /// Get all registered boards
        /// </summary>
        public Dictionary<Vector2Int, GameObject> GetAllBoards()
        {
            return new Dictionary<Vector2Int, GameObject>(worldBoardPositions);
        }
        
        /// <summary>
        /// Get board name at a specific world position
        /// </summary>
        public string GetBoardName(Vector2Int worldPosition)
        {
            return boardNames.ContainsKey(worldPosition) ? boardNames[worldPosition] : "Unknown";
        }
        
        /// <summary>
        /// Check if a board exists at a specific world position
        /// </summary>
        public bool HasBoardAt(Vector2Int worldPosition)
        {
            return worldBoardPositions.ContainsKey(worldPosition);
        }
    }
}
