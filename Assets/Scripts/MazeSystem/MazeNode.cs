using UnityEngine;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Represents a single node on a maze floor minimap.
    /// </summary>
    [System.Serializable]
    public class MazeNode
    {
        public string nodeId;
        public MazeNodeType nodeType;
        public Vector2Int position; // Grid position
        public bool visited = false;
        public bool revealed = false; // Visible on minimap (fog of war)
        public bool isSelectable = false; // Can be clicked (adjacent to current position)
        
        // Node-specific data (expanded based on type)
        public MazeNodeData nodeData;
        
        public MazeNode(string id, MazeNodeType type, Vector2Int pos)
        {
            nodeId = id;
            nodeType = type;
            position = pos;
            visited = false;
            revealed = false;
            isSelectable = false;
        }
        
        public void MarkVisited()
        {
            visited = true;
        }
        
        public void Reveal()
        {
            revealed = true;
        }
        
        public void SetSelectable(bool selectable)
        {
            isSelectable = selectable;
        }
    }
    
    /// <summary>
    /// Node-specific data container (combat encounter, shrine type, loot, etc.)
    /// </summary>
    [System.Serializable]
    public class MazeNodeData
    {
        // Combat node data
        public EnemyTier enemyTier = EnemyTier.Normal;
        public int enemyCount = 2;
        
        // Shrine node data
        public string shrineType; // "Cohesion", "Echoes", "Anchored Form", etc.
        public float buffStrength = 1f;
        
        // Chest node data
        public int lootTier = 1;
        
        // Boss node data
        public string bossId;
        public float retreatThreshold = 0.45f; // Boss retreats at this HP %
        
        // Trap node data
        public string trapType;
        public int trapDifficulty = 12;
    }
    
    /// <summary>
    /// Types of nodes available in the maze.
    /// </summary>
    public enum MazeNodeType
    {
        Start,      // Entry point (only one per floor)
        Combat,     // Enemy encounter
        Chest,      // Treasure/loot
        Shrine,     // Run-wide buff
        Trap,       // Entropic anomaly/curse
        Forge,      // Card fusion/upgrade
        Special,    // Unique nodes (Market, Still-Point, etc.)
        Stairs,     // Next floor entrance (triggers boss on first visit)
        Boss        // Boss encounter (spawned before stairs)
    }
}

