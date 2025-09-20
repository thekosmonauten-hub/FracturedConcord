using System.Collections.Generic;
using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Represents a complete passive board with nodes and extension points
    /// </summary>
    [System.Serializable]
    public class PassiveBoard
    {
        [Header("Board Identity")]
        public string id;
        public string name;
        public string description;
        public BoardTheme theme;
        
        [Header("Board Size")]
        public Vector2Int size = new Vector2Int(7, 7); // rows, columns
        
        [Header("Board Content")]
        public PassiveNode[,] nodes; // 2D grid of nodes
        public List<ExtensionPoint> extensionPoints = new List<ExtensionPoint>();
        
        [Header("Limits")]
        public int maxPoints = 15;
        
        [Header("Visual")]
        public Color boardColor = Color.white;
        public Sprite boardBackground;
        
        [Header("Sprite Management")]
        [SerializeField] private PassiveTreeSpriteManager spriteManager;
        [SerializeField] private bool useCustomSprites = true;
        
        /// <summary>
        /// Initialize the board with proper dimensions
        /// </summary>
        public void InitializeBoard()
        {
            nodes = new PassiveNode[size.x, size.y];
        }
        
        /// <summary>
        /// Get a node at a specific position
        /// </summary>
        public PassiveNode GetNode(int row, int column)
        {
            // Check if nodes array is initialized
            if (nodes == null)
            {
                Debug.LogWarning($"[PassiveBoard] Nodes array is null for board '{name}'. Initializing board...");
                InitializeBoard();
            }
            
            if (row >= 0 && row < size.x && column >= 0 && column < size.y)
            {
                return nodes[row, column];
            }
            return null;
        }
        
        /// <summary>
        /// Set a node at a specific position
        /// </summary>
        public void SetNode(int row, int column, PassiveNode node)
        {
            if (row >= 0 && row < size.x && column >= 0 && column < size.y)
            {
                nodes[row, column] = node;
                if (node != null)
                {
                    node.position = new Vector2Int(row, column);
                }
            }
        }
        
        /// <summary>
        /// Get all nodes as a flat list
        /// </summary>
        public List<PassiveNode> GetAllNodes()
        {
            var allNodes = new List<PassiveNode>();
            
            // Check if nodes array is initialized
            if (nodes == null)
            {
                Debug.LogWarning($"[PassiveBoard] Nodes array is null for board '{name}'. Initializing board...");
                InitializeBoard();
            }
            
            for (int row = 0; row < size.x; row++)
            {
                for (int column = 0; column < size.y; column++)
                {
                    if (nodes[row, column] != null)
                    {
                        allNodes.Add(nodes[row, column]);
                    }
                }
            }
            
            return allNodes;
        }
        
        /// <summary>
        /// Find a node by ID within this board
        /// </summary>
        public PassiveNode FindNodeById(string nodeId)
        {
            foreach (var node in GetAllNodes())
            {
                if (node.id == nodeId)
                {
                    return node;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Get all allocated nodes
        /// </summary>
        public List<PassiveNode> GetAllocatedNodes()
        {
            var allocatedNodes = new List<PassiveNode>();
            
            foreach (var node in GetAllNodes())
            {
                if (node.currentRank > 0)
                {
                    allocatedNodes.Add(node);
                }
            }
            
            return allocatedNodes;
        }
        
        /// <summary>
        /// Get total stats from all allocated nodes
        /// </summary>
        public Dictionary<string, float> GetTotalStats()
        {
            var totalStats = new Dictionary<string, float>();
            
            foreach (var node in GetAllocatedNodes())
            {
                var nodeStats = node.GetCurrentStats();
                foreach (var stat in nodeStats)
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
        /// Get the total points spent on this board
        /// </summary>
        public int GetTotalPointsSpent()
        {
            int totalPoints = 0;
            
            foreach (var node in GetAllocatedNodes())
            {
                totalPoints += node.currentRank * node.cost;
            }
            
            return totalPoints;
        }
        
        /// <summary>
        /// Check if the board has available extension points
        /// </summary>
        public List<ExtensionPoint> GetAvailableExtensionPoints()
        {
            var availablePoints = new List<ExtensionPoint>();
            
            foreach (var extensionPoint in extensionPoints)
            {
                if (extensionPoint.CanConnect())
                {
                    availablePoints.Add(extensionPoint);
                }
            }
            
            return availablePoints;
        }
        
        /// <summary>
        /// Get the board container sprite for this board's theme
        /// </summary>
        public Sprite GetBoardContainerSprite()
        {
            if (useCustomSprites && spriteManager != null)
            {
                return spriteManager.GetBoardContainerSprite(theme);
            }
            
            return boardBackground;
        }
        
        /// <summary>
        /// Get the cell sprite for this board's theme
        /// </summary>
        public Sprite GetCellSprite()
        {
            if (useCustomSprites && spriteManager != null)
            {
                return spriteManager.GetCellSprite(theme);
            }
            
            return null;
        }
        
        /// <summary>
        /// Get the node sprite for a specific node type
        /// </summary>
        public Sprite GetNodeSprite(NodeType nodeType)
        {
            if (useCustomSprites && spriteManager != null)
            {
                return spriteManager.GetNodeSprite(nodeType);
            }
            
            return null;
        }
        
        /// <summary>
        /// Set the sprite manager for this board
        /// </summary>
        public void SetSpriteManager(PassiveTreeSpriteManager manager)
        {
            spriteManager = manager;
        }
        
        /// <summary>
        /// Enable or disable custom sprite usage
        /// </summary>
        public void SetUseCustomSprites(bool useCustom)
        {
            useCustomSprites = useCustom;
        }
        
        /// <summary>
        /// Get the sprite manager for this board
        /// </summary>
        public PassiveTreeSpriteManager GetSpriteManager()
        {
            return spriteManager;
        }
        
        /// <summary>
        /// Get whether custom sprites are enabled for this board
        /// </summary>
        public bool GetUseCustomSprites()
        {
            return useCustomSprites;
        }
        
        /// <summary>
        /// Log the current sprite manager status for debugging
        /// </summary>
        public void LogSpriteManagerStatus()
        {
            if (spriteManager != null)
            {
                Debug.Log($"[PassiveBoard] '{name}' has sprite manager: {spriteManager.name} (Use Custom Sprites: {useCustomSprites})");
            }
            else
            {
                Debug.LogWarning($"[PassiveBoard] '{name}' has NO sprite manager assigned (Use Custom Sprites: {useCustomSprites})");
            }
        }
    }
    
    /// <summary>
    /// Board themes for different types of passives
    /// </summary>
    public enum BoardTheme
    {
        Fire,
        Cold,
        Lightning,
        Chaos,
        Physical,
        Life,
        Armor,
        Evasion,
        Critical,
        Speed,
        Utility,
        Elemental,
        Keystone
    }
}
