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
        
        [Header("Connection Settings")]
        public List<string> availableBoards = new List<string>();
        public int maxConnections = 1;
        public int currentConnections = 0;
        
        [Header("Visual")]
        public Color connectionColor = Color.yellow;
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
    }
}
