using System.Collections.Generic;
using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Tracks the player's passive tree state and progress
    /// </summary>
    [System.Serializable]
    public class PlayerPassiveState
    {
        [Header("Allocated Nodes")]
        public List<string> allocatedNodes = new List<string>();        // Array of allocated node IDs
        
        [Header("Connected Boards")]
        public List<string> connectedBoards = new List<string>();       // Array of connected board IDs
        public List<BoardConnection> boardConnections = new List<BoardConnection>(); // Connection metadata
        
        [Header("Resources")]
        public int availablePoints = 0;
        
        [Header("Stats Cache")]
        public Dictionary<string, float> cachedStats = new Dictionary<string, float>();
        public bool statsDirty = true; // Flag to indicate if stats need recalculation
        
        /// <summary>
        /// Initialize the passive state
        /// </summary>
        public void Initialize(int startingPoints = 0)
        {
            allocatedNodes.Clear();
            connectedBoards.Clear();
            boardConnections.Clear();
            availablePoints = startingPoints;
            statsDirty = true;
            
            // CRITICAL: Always allocate the START node by default
            EnsureStartNodeAllocated();
        }
        
        /// <summary>
        /// Ensure the START node is always allocated
        /// </summary>
        private void EnsureStartNodeAllocated()
        {
            const string START_NODE_ID = "core_main";
            
            if (!allocatedNodes.Contains(START_NODE_ID))
            {
                allocatedNodes.Add(START_NODE_ID);
                Debug.Log("[PlayerPassiveState] START node automatically allocated");
            }
        }
        
        /// <summary>
        /// Check if a node is allocated
        /// </summary>
        public bool IsNodeAllocated(string nodeId)
        {
            return allocatedNodes.Contains(nodeId);
        }
        
        /// <summary>
        /// Allocate a node
        /// </summary>
        public bool AllocateNode(string nodeId, int cost)
        {
            if (availablePoints >= cost && !IsNodeAllocated(nodeId))
            {
                allocatedNodes.Add(nodeId);
                availablePoints -= cost;
                statsDirty = true;
                
                // Ensure START node stays allocated
                EnsureStartNodeAllocated();
                
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Deallocate a node
        /// </summary>
        public bool DeallocateNode(string nodeId, int cost)
        {
            // CRITICAL: Prevent deallocating the START node
            const string START_NODE_ID = "core_main";
            if (nodeId == START_NODE_ID)
            {
                Debug.LogWarning("[PlayerPassiveState] Cannot deallocate START node!");
                return false;
            }
            
            if (IsNodeAllocated(nodeId))
            {
                allocatedNodes.Remove(nodeId);
                availablePoints += cost;
                statsDirty = true;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Check if a board is connected
        /// </summary>
        public bool IsBoardConnected(string boardId)
        {
            return connectedBoards.Contains(boardId);
        }
        
        /// <summary>
        /// Connect a board
        /// </summary>
        public bool ConnectBoard(string extensionPointId, string boardId)
        {
            if (!IsBoardConnected(boardId))
            {
                var connection = new BoardConnection(extensionPointId, boardId);
                boardConnections.Add(connection);
                connectedBoards.Add(boardId);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Disconnect a board
        /// </summary>
        public bool DisconnectBoard(string boardId)
        {
            if (IsBoardConnected(boardId))
            {
                boardConnections.RemoveAll(c => c.boardId == boardId);
                connectedBoards.Remove(boardId);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get all allocated node IDs
        /// </summary>
        public List<string> GetAllocatedNodeIds()
        {
            return new List<string>(allocatedNodes);
        }
        
        /// <summary>
        /// Get all connected board IDs
        /// </summary>
        public List<string> GetConnectedBoardIds()
        {
            return new List<string>(connectedBoards);
        }
        
        /// <summary>
        /// Get all board connections
        /// </summary>
        public List<BoardConnection> GetBoardConnections()
        {
            return new List<BoardConnection>(boardConnections);
        }
        
        /// <summary>
        /// Add skill points
        /// </summary>
        public void AddSkillPoints(int points)
        {
            availablePoints += points;
        }
        
        /// <summary>
        /// Get available skill points
        /// </summary>
        public int GetAvailablePoints()
        {
            return availablePoints;
        }
        
        /// <summary>
        /// Mark stats as dirty (need recalculation)
        /// </summary>
        public void MarkStatsDirty()
        {
            statsDirty = true;
        }
        
        /// <summary>
        /// Check if stats need recalculation
        /// </summary>
        public bool AreStatsDirty()
        {
            return statsDirty;
        }
        
        /// <summary>
        /// Update cached stats
        /// </summary>
        public void UpdateCachedStats(Dictionary<string, float> newStats)
        {
            cachedStats.Clear();
            foreach (var stat in newStats)
            {
                cachedStats[stat.Key] = stat.Value;
            }
            statsDirty = false;
        }
        
        /// <summary>
        /// Get cached stats
        /// </summary>
        public Dictionary<string, float> GetCachedStats()
        {
            return new Dictionary<string, float>(cachedStats);
        }
        
        /// <summary>
        /// Reset the passive state
        /// </summary>
        public void Reset()
        {
            Initialize(availablePoints);
        }
        
        /// <summary>
        /// Create a deep copy of this state
        /// </summary>
        public PlayerPassiveState Clone()
        {
            var clone = new PlayerPassiveState();
            clone.allocatedNodes = new List<string>(allocatedNodes);
            clone.connectedBoards = new List<string>(connectedBoards);
            clone.boardConnections = new List<BoardConnection>(boardConnections);
            clone.availablePoints = availablePoints;
            clone.cachedStats = new Dictionary<string, float>(cachedStats);
            clone.statsDirty = statsDirty;
            return clone;
        }
        
        #region Persistence Methods
        
        /// <summary>
        /// Save the passive state to PlayerPrefs for a specific character
        /// </summary>
        public void SaveToPlayerPrefs(string characterName)
        {
            string prefix = $"PassiveTree_{characterName}_";
            
            // Save allocated nodes as JSON array
            string allocatedNodesJson = JsonUtility.ToJson(new StringListWrapper { items = allocatedNodes });
            PlayerPrefs.SetString(prefix + "AllocatedNodes", allocatedNodesJson);
            
            // Save connected boards as JSON array
            string connectedBoardsJson = JsonUtility.ToJson(new StringListWrapper { items = connectedBoards });
            PlayerPrefs.SetString(prefix + "ConnectedBoards", connectedBoardsJson);
            
            // Save board connections as JSON array
            string boardConnectionsJson = JsonUtility.ToJson(new BoardConnectionListWrapper { items = boardConnections });
            PlayerPrefs.SetString(prefix + "BoardConnections", boardConnectionsJson);
            
            // Save available points
            PlayerPrefs.SetInt(prefix + "AvailablePoints", availablePoints);
            
            // Save cached stats as JSON
            string cachedStatsJson = JsonUtility.ToJson(new StatsWrapper { stats = cachedStats });
            PlayerPrefs.SetString(prefix + "CachedStats", cachedStatsJson);
            
            PlayerPrefs.Save();
            Debug.Log($"[PlayerPassiveState] Saved passive tree state for {characterName}");
        }
        
        /// <summary>
        /// Load the passive state from PlayerPrefs for a specific character
        /// </summary>
        public void LoadFromPlayerPrefs(string characterName)
        {
            string prefix = $"PassiveTree_{characterName}_";
            
            // Load allocated nodes
            string allocatedNodesJson = PlayerPrefs.GetString(prefix + "AllocatedNodes", "");
            if (!string.IsNullOrEmpty(allocatedNodesJson))
            {
                var wrapper = JsonUtility.FromJson<StringListWrapper>(allocatedNodesJson);
                allocatedNodes = wrapper.items ?? new List<string>();
            }
            
            // Load connected boards
            string connectedBoardsJson = PlayerPrefs.GetString(prefix + "ConnectedBoards", "");
            if (!string.IsNullOrEmpty(connectedBoardsJson))
            {
                var wrapper = JsonUtility.FromJson<StringListWrapper>(connectedBoardsJson);
                connectedBoards = wrapper.items ?? new List<string>();
            }
            
            // Load board connections
            string boardConnectionsJson = PlayerPrefs.GetString(prefix + "BoardConnections", "");
            if (!string.IsNullOrEmpty(boardConnectionsJson))
            {
                var wrapper = JsonUtility.FromJson<BoardConnectionListWrapper>(boardConnectionsJson);
                boardConnections = wrapper.items ?? new List<BoardConnection>();
            }
            
            // Load available points
            availablePoints = PlayerPrefs.GetInt(prefix + "AvailablePoints", 0);
            
            // Load cached stats
            string cachedStatsJson = PlayerPrefs.GetString(prefix + "CachedStats", "");
            if (!string.IsNullOrEmpty(cachedStatsJson))
            {
                var wrapper = JsonUtility.FromJson<StatsWrapper>(cachedStatsJson);
                cachedStats = wrapper.stats ?? new Dictionary<string, float>();
            }
            
            // Ensure START node is always allocated
            EnsureStartNodeAllocated();
            
            statsDirty = true; // Mark stats as dirty to force recalculation
            Debug.Log($"[PlayerPassiveState] Loaded passive tree state for {characterName}");
        }
        
        /// <summary>
        /// Check if passive state exists for a character
        /// </summary>
        public static bool HasSavedState(string characterName)
        {
            string prefix = $"PassiveTree_{characterName}_";
            return PlayerPrefs.HasKey(prefix + "AllocatedNodes");
        }
        
        /// <summary>
        /// Clear saved passive state for a character
        /// </summary>
        public static void ClearSavedState(string characterName)
        {
            string prefix = $"PassiveTree_{characterName}_";
            PlayerPrefs.DeleteKey(prefix + "AllocatedNodes");
            PlayerPrefs.DeleteKey(prefix + "ConnectedBoards");
            PlayerPrefs.DeleteKey(prefix + "BoardConnections");
            PlayerPrefs.DeleteKey(prefix + "AvailablePoints");
            PlayerPrefs.DeleteKey(prefix + "CachedStats");
            PlayerPrefs.Save();
            Debug.Log($"[PlayerPassiveState] Cleared passive tree state for {characterName}");
        }
        
        #endregion
    }
    
    // Helper classes for JSON serialization
    [System.Serializable]
    public class StringListWrapper
    {
        public List<string> items = new List<string>();
    }
    
    [System.Serializable]
    public class BoardConnectionListWrapper
    {
        public List<BoardConnection> items = new List<BoardConnection>();
    }
    
    [System.Serializable]
    public class StatsWrapper
    {
        public Dictionary<string, float> stats = new Dictionary<string, float>();
    }
}
