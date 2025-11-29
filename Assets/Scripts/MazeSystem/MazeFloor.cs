using System.Collections.Generic;
using UnityEngine;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Represents a single floor/minimap in the maze run.
    /// </summary>
    [System.Serializable]
    public class MazeFloor
    {
        public int floorNumber;
        public Vector2Int playerPosition; // Current position on this floor
        public Dictionary<Vector2Int, MazeNode> nodes = new Dictionary<Vector2Int, MazeNode>();
        public List<Vector2Int> nodePositions = new List<Vector2Int>(); // Ordered list for iteration
        public Vector2Int startPosition;
        public Vector2Int stairsPosition;
        public Vector2Int? bossPosition; // Null if boss not yet spawned
        
        public MazeFloor(int floorNum)
        {
            floorNumber = floorNum;
            nodes = new Dictionary<Vector2Int, MazeNode>();
            nodePositions = new List<Vector2Int>();
        }
        
        public void AddNode(MazeNode node)
        {
            nodes[node.position] = node;
            if (!nodePositions.Contains(node.position))
            {
                nodePositions.Add(node.position);
            }
        }
        
        public MazeNode GetNode(Vector2Int pos)
        {
            return nodes.TryGetValue(pos, out var node) ? node : null;
        }
        
        public void SetPlayerPosition(Vector2Int pos)
        {
            playerPosition = pos;
            UpdateSelectableNodes();
        }
        
        /// <summary>
        /// Updates which nodes are selectable based on player position.
        /// Allows free navigation between visited nodes.
        /// For unvisited nodes, only reveals those on paths leading to the stairs.
        /// </summary>
        public void UpdateSelectableNodes()
        {
            // First, mark all nodes as not selectable
            foreach (var kvp in nodes)
            {
                var node = kvp.Value;
                
                // Check if adjacent (Manhattan distance = 1)
                int distance = Mathf.Abs(node.position.x - playerPosition.x) + 
                              Mathf.Abs(node.position.y - playerPosition.y);
                
                bool isAdjacent = distance == 1;
                
                if (node.visited)
                {
                    // Allow free navigation between visited nodes
                    if (isAdjacent)
                    {
                        node.Reveal(); // Ensure visited nodes are always revealed
                        node.SetSelectable(true); // Allow navigation between visited nodes
                    }
                    else
                    {
                        // Not adjacent - can't move there (but it's still revealed if visited)
                        node.SetSelectable(false);
                    }
                    continue;
                }
                
                // For unvisited nodes, only reveal and make selectable those on paths to stairs
                if (isAdjacent)
                {
                    // Only reveal and make selectable nodes that lead to stairs (are on a path to stairs)
                    bool leadsToStairs = HasPathToStairs(node.position, playerPosition);
                    if (leadsToStairs)
                    {
                        node.Reveal();
                        node.SetSelectable(true);
                    }
                    else
                    {
                        // Adjacent but dead end/branch - keep hidden until visited
                        node.SetSelectable(false);
                        // Don't reveal it yet - let it remain hidden
                    }
                }
                else
                {
                    node.SetSelectable(false);
                }
            }
        }
        
        /// <summary>
        /// Checks if there's a path from the given position to the stairs.
        /// Uses BFS to find if stairs is reachable from this position.
        /// </summary>
        private bool HasPathToStairs(Vector2Int fromPos, Vector2Int excludePos)
        {
            if (!nodes.ContainsKey(fromPos) || !nodes.ContainsKey(stairsPosition))
                return false;
            
            // If already at stairs, return true
            if (fromPos == stairsPosition)
                return true;
            
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            
            queue.Enqueue(fromPos);
            visited.Add(fromPos);
            visited.Add(excludePos); // Exclude current player position from pathfinding
            
            Vector2Int[] directions = {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0)
            };
            
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                if (current == stairsPosition)
                    return true;
                
                foreach (var dir in directions)
                {
                    Vector2Int next = current + dir;
                    if (nodes.ContainsKey(next) && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Reveals nodes adjacent to visited nodes, but only those on paths leading to stairs.
        /// This creates a linear path visualization, hiding branches until explored.
        /// </summary>
        public void RevealAdjacentNodes(Vector2Int centerPos)
        {
            Vector2Int[] directions = {
                new Vector2Int(0, 1),  // Up
                new Vector2Int(0, -1), // Down
                new Vector2Int(1, 0),  // Right
                new Vector2Int(-1, 0)  // Left
            };
            
            foreach (var dir in directions)
            {
                Vector2Int adjPos = centerPos + dir;
                if (nodes.TryGetValue(adjPos, out var node))
                {
                    // Only reveal nodes that lead to stairs (on the main path)
                    // This hides branches and dead ends
                    if (HasPathToStairs(adjPos, centerPos))
                    {
                        node.Reveal();
                    }
                    // Don't reveal branches/dead ends - keep them hidden
                }
            }
        }
        
        /// <summary>
        /// Validates that there's a path from start to stairs using BFS.
        /// </summary>
        public bool ValidateConnectivity()
        {
            if (!nodes.ContainsKey(startPosition) || !nodes.ContainsKey(stairsPosition))
                return false;
            
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            
            queue.Enqueue(startPosition);
            visited.Add(startPosition);
            
            Vector2Int[] directions = {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0)
            };
            
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                
                if (current == stairsPosition)
                    return true;
                
                foreach (var dir in directions)
                {
                    Vector2Int next = current + dir;
                    if (nodes.ContainsKey(next) && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
            
            return false;
        }
    }
}

