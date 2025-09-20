using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Component that automatically assigns grid positions to child cells based on their names
    /// Attach this to a parent GameObject that contains cells named like "Cell_X_Y"
    /// </summary>
    public class CellPositionAutoAssigner : MonoBehaviour
    {
        [Header("Auto Assignment Settings")]
        [SerializeField] private bool assignOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool assignSprites = true;
        
        [Header("Validation")]
        [SerializeField] private bool validateNames = true;
        [SerializeField] private bool onlyAssignValidNames = true;
        
        [Header("Node Type Assignment")]
        [SerializeField] private bool assignNodeTypes = true;
        [SerializeField] private NodeType centerNodeType = NodeType.Start;
        [SerializeField] private NodeType extensionNodeType = NodeType.Extension;
        [SerializeField] private NodeType travelNodeType = NodeType.Travel;
        [SerializeField] private NodeType notableNodeType = NodeType.Notable;
        [SerializeField] private NodeType smallNodeType = NodeType.Small;
        [SerializeField] private bool enableExtensionPoints = true;
        [SerializeField] private Vector2Int[] extensionPointPositions = new Vector2Int[]
        {
            new Vector2Int(0, 3),  // South
            new Vector2Int(3, 0),  // West
            new Vector2Int(3, 6),  // East
            new Vector2Int(6, 3)   // North
        };
        [SerializeField] private Vector2Int[] travelNodePositions = new Vector2Int[]
        {
            new Vector2Int(1, 3),  // Travel nodes
            new Vector2Int(2, 3),
            new Vector2Int(3, 1),
            new Vector2Int(3, 2),
            new Vector2Int(3, 4),
            new Vector2Int(3, 5),
            new Vector2Int(4, 3),
            new Vector2Int(5, 3)
        };
        [SerializeField] private Vector2Int[] notableNodePositions = new Vector2Int[]
        {
            new Vector2Int(1, 1),  // Notable nodes
            new Vector2Int(1, 5),
            new Vector2Int(5, 1),
            new Vector2Int(5, 5)
        };

        void Start()
        {
            if (assignOnStart)
            {
                AssignPositionsToChildren();
            }
        }

        /// <summary>
        /// Manually trigger position assignment
        /// </summary>
        [ContextMenu("Assign Positions to Children")]
        public void AssignPositionsToChildren()
        {
            CellController[] childCells = GetComponentsInChildren<CellController>();
            int assignedCount = 0;
            int errorCount = 0;

            if (showDebugInfo)
                Debug.Log($"[CellPositionAutoAssigner] Found {childCells.Length} child cells");

            foreach (CellController cell in childCells)
            {
                if (AssignPositionToCell(cell))
                {
                    assignedCount++;
                    
                    // Assign node type if enabled
                    if (assignNodeTypes)
                    {
                        AssignNodeTypeToCell(cell);
                    }
                    
                    // Assign sprite if enabled
                    if (assignSprites)
                    {
                        AssignSpriteToCell(cell);
                    }
                }
                else
                {
                    errorCount++;
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"[CellPositionAutoAssigner] Assignment complete: {assignedCount} successful, {errorCount} errors");
            }
        }

        /// <summary>
        /// Assign position to a specific cell based on its name
        /// </summary>
        private bool AssignPositionToCell(CellController cell)
        {
            if (cell == null)
            {
                if (showDebugInfo)
                    Debug.LogError("[CellPositionAutoAssigner] Cell is null");
                return false;
            }

            Vector2Int? position = ParseCellName(cell.name);
            if (position.HasValue)
            {
                // Use reflection to set the private field
                var field = typeof(CellController).GetField("gridPosition", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(cell, position.Value);
                    
                    if (showDebugInfo)
                        Debug.Log($"[CellPositionAutoAssigner] Assigned position {position.Value} to {cell.name}");
                    
                    return true;
                }
                else
                {
                    if (showDebugInfo)
                        Debug.LogError($"[CellPositionAutoAssigner] Could not find gridPosition field in CellController");
                    return false;
                }
            }
            else
            {
                if (showDebugInfo && validateNames)
                    Debug.LogWarning($"[CellPositionAutoAssigner] Could not parse position from name: {cell.name}");
                return false;
            }
        }

        /// <summary>
        /// Parse cell name to extract grid position
        /// Expected format: Cell_X_Y (e.g., Cell_1_3)
        /// </summary>
        private Vector2Int? ParseCellName(string cellName)
        {
            if (string.IsNullOrEmpty(cellName))
            {
                return null;
            }

            // Remove any whitespace
            cellName = cellName.Trim();

            // Check if name starts with "Cell_"
            if (!cellName.StartsWith("Cell_"))
            {
                return null;
            }

            // Remove "Cell_" prefix
            string positionPart = cellName.Substring(5); // "Cell_".Length = 5

            // Split by underscore
            string[] parts = positionPart.Split('_');
            
            if (parts.Length != 2)
            {
                return null;
            }

            // Try to parse X and Y coordinates
            if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            {
                return new Vector2Int(x, y);
            }

            return null;
        }

        /// <summary>
        /// Validate all child cell names
        /// </summary>
        [ContextMenu("Validate Child Cell Names")]
        public void ValidateChildCellNames()
        {
            CellController[] childCells = GetComponentsInChildren<CellController>();
            int validCount = 0;
            int invalidCount = 0;

            foreach (CellController cell in childCells)
            {
                Vector2Int? position = ParseCellName(cell.name);
                if (position.HasValue)
                {
                    validCount++;
                    if (showDebugInfo)
                        Debug.Log($"[CellPositionAutoAssigner] Valid: {cell.name} -> {position.Value}");
                }
                else
                {
                    invalidCount++;
                    if (showDebugInfo)
                        Debug.LogWarning($"[CellPositionAutoAssigner] Invalid: {cell.name}");
                }
            }

            Debug.Log($"[CellPositionAutoAssigner] Validation complete: {validCount} valid, {invalidCount} invalid");
        }
        
        /// <summary>
        /// Assign node type to a cell based on its position
        /// </summary>
        private void AssignNodeTypeToCell(CellController cell)
        {
            Vector2Int? position = ParseCellName(cell.name);
            if (position.HasValue)
            {
                NodeType nodeType = DetermineNodeType(position.Value);
                cell.SetNodeType(nodeType);
                
                if (showDebugInfo)
                    Debug.Log($"[CellPositionAutoAssigner] Assigned {nodeType} to {cell.name}");
            }
        }
        
        /// <summary>
        /// Assign sprite to a cell based on its node type
        /// </summary>
        private void AssignSpriteToCell(CellController cell)
        {
            // The CellController will handle sprite assignment automatically
            // when the node type is set, if autoAssignSprite is enabled
            cell.UpdateSpriteForNodeType();
            
            if (showDebugInfo)
                Debug.Log($"[CellPositionAutoAssigner] Updated sprite for {cell.name}");
        }
        
        /// <summary>
        /// Determine node type based on grid position
        /// </summary>
        private NodeType DetermineNodeType(Vector2Int position)
        {
            // Center cell (3,3) for a 7x7 grid becomes the start node
            if (position.x == 3 && position.y == 3)
            {
                return centerNodeType;
            }
            
            // Check if this position is an extension point
            if (enableExtensionPoints && IsExtensionPoint(position))
            {
                return extensionNodeType;
            }
            
            // Check if this position is a travel node
            if (IsTravelNode(position))
            {
                return travelNodeType;
            }
            
            // Check if this position is a notable node
            if (IsNotableNode(position))
            {
                return notableNodeType;
            }
            
            // All remaining positions are small nodes
            return smallNodeType;
        }
        
        /// <summary>
        /// Check if a position is an extension point
        /// </summary>
        private bool IsExtensionPoint(Vector2Int position)
        {
            if (extensionPointPositions == null || extensionPointPositions.Length == 0)
                return false;
                
            foreach (Vector2Int extensionPos in extensionPointPositions)
            {
                if (position == extensionPos)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a position is a travel node
        /// </summary>
        private bool IsTravelNode(Vector2Int position)
        {
            if (travelNodePositions == null || travelNodePositions.Length == 0)
                return false;
                
            foreach (Vector2Int travelPos in travelNodePositions)
            {
                if (position == travelPos)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a position is a notable node
        /// </summary>
        private bool IsNotableNode(Vector2Int position)
        {
            if (notableNodePositions == null || notableNodePositions.Length == 0)
                return false;
                
            foreach (Vector2Int notablePos in notableNodePositions)
            {
                if (position == notablePos)
                    return true;
            }
            
            return false;
        }
    }
}
