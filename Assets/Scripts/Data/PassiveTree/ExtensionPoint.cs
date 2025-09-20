using System.Collections.Generic;
using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Represents an extension point where boards can be connected
    /// </summary>
    [System.Serializable]
    public class ExtensionPoint
    {
        [Header("Identity")]
        public string id;
        public Vector2Int position; // row, column in 2D grid
        public string boardId => id; // Compatibility property
        
        [Header("World Position")]
        public Vector3Int worldPosition = Vector3Int.zero;
        
        [Header("Connection Settings")]
        public List<string> availableBoards = new List<string>();
        public int maxConnections = 1;
        public int currentConnections = 0;
        
        [Header("Preview Allocation")]
        public bool isPreviewAllocated = false;
        public string previewBoardId = "";
        public Vector2Int previewBoardPosition = Vector2Int.zero;
        
        [Header("Visual")]
        public Color connectionColor = Color.yellow;
        public Color previewAllocatedColor = Color.cyan;
        public Sprite connectionIcon;
        
        /// <summary>
        /// Check if this extension point can accept a connection
        /// </summary>
        public bool CanConnect()
        {
            return currentConnections < maxConnections;
        }
        
        /// <summary>
        /// Check if a specific board can be connected to this point
        /// </summary>
        public bool CanConnectBoard(string boardId)
        {
            // If availableBoards is empty, allow all board types
            if (availableBoards.Count == 0)
            {
                return CanConnect();
            }
            
            return availableBoards.Contains(boardId) && CanConnect();
        }
        
        /// <summary>
        /// Add a connection to this extension point
        /// </summary>
        public bool AddConnection()
        {
            if (CanConnect())
            {
                currentConnections++;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Remove a connection from this extension point
        /// </summary>
        public bool RemoveConnection()
        {
            if (currentConnections > 0)
            {
                currentConnections--;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Set preview allocation for this extension point
        /// </summary>
        public void SetPreviewAllocation(string boardId, Vector2Int boardPosition)
        {
            isPreviewAllocated = true;
            previewBoardId = boardId;
            previewBoardPosition = boardPosition;
        }
        
        /// <summary>
        /// Clear preview allocation from this extension point
        /// </summary>
        public void ClearPreviewAllocation()
        {
            isPreviewAllocated = false;
            previewBoardId = "";
            previewBoardPosition = Vector2Int.zero;
        }
        
        /// <summary>
        /// Commit preview allocation to actual allocation
        /// </summary>
        public bool CommitPreviewAllocation()
        {
            if (!isPreviewAllocated) return false;
            
            if (AddConnection())
            {
                isPreviewAllocated = false;
                previewBoardId = "";
                previewBoardPosition = Vector2Int.zero;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get the direction of this extension point
        /// </summary>
        public Vector2Int GetDirection()
        {
            // TODO: Implement direction logic based on position
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// Get the opposing direction of this extension point
        /// </summary>
        public Vector2Int GetOpposingDirection()
        {
            // TODO: Implement opposing direction logic
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// Get visual color for this extension point
        /// </summary>
        public Color GetVisualColor()
        {
            if (isPreviewAllocated)
                return previewAllocatedColor;
            return connectionColor;
        }
        
        /// <summary>
        /// Get the world position of this extension point
        /// </summary>
        public Vector3Int GetWorldPosition()
        {
            return worldPosition;
        }
        
        /// <summary>
        /// Set the world position of this extension point
        /// </summary>
        public void SetWorldPosition(Vector3Int position)
        {
            worldPosition = position;
        }
        
        /// <summary>
        /// Get a string representation of this extension point
        /// </summary>
        public override string ToString()
        {
            return $"ExtensionPoint '{id}' at {position} (world: {worldPosition}) - {currentConnections}/{maxConnections} connections";
        }
    }
}
