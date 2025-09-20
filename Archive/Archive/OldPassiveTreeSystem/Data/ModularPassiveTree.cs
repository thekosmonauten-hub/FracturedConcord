using System.Collections.Generic;
using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Main class that manages the entire modular passive tree system
    /// </summary>
    [System.Serializable]
    public class ModularPassiveTree
    {
        [Header("Core Board")]
        public PassiveBoard coreBoard;
        
        [Header("Extension Boards")]
        public Dictionary<string, PassiveBoard> extensionBoards = new Dictionary<string, PassiveBoard>();
        
        [Header("Keystone Boards")]
        public Dictionary<string, PassiveBoard> keystoneBoards = new Dictionary<string, PassiveBoard>();
        
        [Header("Board Limits")]
        public BoardLimits boardLimits = new BoardLimits();
        
        /// <summary>
        /// Initialize the passive tree system
        /// </summary>
        public void Initialize()
        {
            if (coreBoard != null)
            {
                coreBoard.InitializeBoard();
            }
            
            foreach (var board in extensionBoards.Values)
            {
                board.InitializeBoard();
            }
            
            foreach (var board in keystoneBoards.Values)
            {
                board.InitializeBoard();
            }
        }
        
        /// <summary>
        /// Get all boards (core + extension + keystone)
        /// </summary>
        public List<PassiveBoard> GetAllBoards()
        {
            var allBoards = new List<PassiveBoard>();
            
            if (coreBoard != null)
                allBoards.Add(coreBoard);
                
            allBoards.AddRange(extensionBoards.Values);
            allBoards.AddRange(keystoneBoards.Values);
            
            return allBoards;
        }
        
        /// <summary>
        /// Get all nodes from all boards
        /// </summary>
        public List<PassiveNode> GetAllNodes()
        {
            var allNodes = new List<PassiveNode>();
            
            foreach (var board in GetAllBoards())
            {
                allNodes.AddRange(board.GetAllNodes());
            }
            
            return allNodes;
        }
        
        /// <summary>
        /// Get all allocated nodes from all boards
        /// </summary>
        public List<PassiveNode> GetAllAllocatedNodes()
        {
            var allAllocatedNodes = new List<PassiveNode>();
            
            foreach (var board in GetAllBoards())
            {
                allAllocatedNodes.AddRange(board.GetAllocatedNodes());
            }
            
            return allAllocatedNodes;
        }
        
        /// <summary>
        /// Get total stats from all allocated nodes
        /// </summary>
        public Dictionary<string, float> GetTotalStats()
        {
            var totalStats = new Dictionary<string, float>();
            
            foreach (var board in GetAllBoards())
            {
                var boardStats = board.GetTotalStats();
                foreach (var stat in boardStats)
                {
                    if (totalStats.ContainsKey(stat.Key))
                    {
                        totalStats[stat.Key] += stat.Value;
                    }
                    else
                    {
                        totalStats[stat.Key] = stat.Value;
                    }
                }
            }
            
            return totalStats;
        }
        
        /// <summary>
        /// Get total points spent across all boards
        /// </summary>
        public int GetTotalPointsSpent()
        {
            int totalPoints = 0;
            
            foreach (var board in GetAllBoards())
            {
                totalPoints += board.GetTotalPointsSpent();
            }
            
            return totalPoints;
        }
        
        /// <summary>
        /// Find a node by ID across all boards
        /// </summary>
        public PassiveNode FindNodeById(string nodeId)
        {
            foreach (var board in GetAllBoards())
            {
                foreach (var node in board.GetAllNodes())
                {
                    if (node.id == nodeId)
                    {
                        return node;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Find a board by ID
        /// </summary>
        public PassiveBoard FindBoardById(string boardId)
        {
            if (coreBoard != null && coreBoard.id == boardId)
                return coreBoard;
                
            if (extensionBoards.ContainsKey(boardId))
                return extensionBoards[boardId];
                
            if (keystoneBoards.ContainsKey(boardId))
                return keystoneBoards[boardId];
                
            return null;
        }
        
        /// <summary>
        /// Check if a board can be connected based on limits
        /// </summary>
        public bool CanConnectBoard(string boardId)
        {
            var board = FindBoardById(boardId);
            if (board == null)
                return false;
                
            // Check extension board limits
            if (extensionBoards.ContainsKey(boardId))
            {
                if (extensionBoards.Count >= boardLimits.maxExtensionBoards)
                    return false;
                    
                var themeCount = GetBoardCountByTheme(board.theme);
                if (themeCount >= boardLimits.maxBoardsPerTheme.GetValueOrDefault(board.theme, int.MaxValue))
                    return false;
            }
            
            // Check keystone board limits
            if (keystoneBoards.ContainsKey(boardId))
            {
                if (keystoneBoards.Count >= boardLimits.maxKeystoneBoards)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get the count of boards by theme
        /// </summary>
        private int GetBoardCountByTheme(BoardTheme theme)
        {
            int count = 0;
            
            foreach (var board in extensionBoards.Values)
            {
                if (board.theme == theme)
                    count++;
            }
            
            return count;
        }
    }
    
    /// <summary>
    /// Board limits configuration
    /// </summary>
    [System.Serializable]
    public class BoardLimits
    {
        [Header("General Limits")]
        public int maxExtensionBoards = 8;
        public int maxKeystoneBoards = 16;
        
        [Header("Theme Limits")]
        public Dictionary<BoardTheme, int> maxBoardsPerTheme = new Dictionary<BoardTheme, int>()
        {
            { BoardTheme.Fire, 3 },
            { BoardTheme.Cold, 3 },
            { BoardTheme.Lightning, 3 },
            { BoardTheme.Chaos, 3 },
            { BoardTheme.Physical, 3 },
            { BoardTheme.Life, 3 },
            { BoardTheme.Armor, 3 },
            { BoardTheme.Evasion, 3 },
            { BoardTheme.Critical, 3 },
            { BoardTheme.Speed, 3 },
            { BoardTheme.Utility, 3 },
            { BoardTheme.Elemental, 3 },
            { BoardTheme.Keystone, 16 }
        };
    }
}
