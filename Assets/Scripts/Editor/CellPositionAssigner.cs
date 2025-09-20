using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using PassiveTree;

/// <summary>
/// Editor script to automatically assign grid positions based on cell names
/// Parses names like "Cell_1_3" to get position (1,3)
/// </summary>
public class CellPositionAssigner : EditorWindow
{
    [MenuItem("Tools/Passive Tree/Assign Cell Positions")]
    public static void ShowWindow()
    {
        GetWindow<CellPositionAssigner>("Cell Position Assigner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cell Position Assigner", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This tool automatically assigns grid positions to cells based on their names.");
        GUILayout.Label("Expected naming pattern: Cell_X_Y (e.g., Cell_1_3 for position 1,3)");
        GUILayout.Space(10);

        if (GUILayout.Button("Assign Positions to All Cells"))
        {
            AssignAllCellPositions();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Assign Positions to Selected Cells"))
        {
            AssignSelectedCellPositions();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Validate Cell Names"))
        {
            ValidateCellNames();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Assign Positions + Sprites + Node Types"))
        {
            AssignAllCellPositionsAndSprites();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Complete Setup (Positions + Components + Sprites)"))
        {
            CompleteCellSetup();
        }
    }

    /// <summary>
    /// Assign positions to all cells in the scene
    /// </summary>
    private void AssignAllCellPositions()
    {
        CellController[] allCells = FindObjectsOfType<CellController>();
        int assignedCount = 0;
        int errorCount = 0;

        Debug.Log($"[CellPositionAssigner] Found {allCells.Length} cells in scene");

        foreach (CellController cell in allCells)
        {
            if (AssignPositionToCell(cell))
            {
                assignedCount++;
            }
            else
            {
                errorCount++;
            }
        }

        Debug.Log($"[CellPositionAssigner] Assignment complete: {assignedCount} successful, {errorCount} errors");
        
        if (assignedCount > 0)
        {
            EditorUtility.SetDirty(FindObjectOfType<PassiveTreeManager>());
        }
    }

    /// <summary>
    /// Assign positions, sprites, and node types to all cells
    /// </summary>
    private void AssignAllCellPositionsAndSprites()
    {
        CellController[] allCells = FindObjectsOfType<CellController>();
        int assignedCount = 0;
        int errorCount = 0;

        Debug.Log($"[CellPositionAssigner] Found {allCells.Length} cells in scene");

        foreach (CellController cell in allCells)
        {
            if (AssignPositionToCell(cell))
            {
                assignedCount++;
                
                // Auto-detect and assign components
                AssignComponentsToCell(cell);
                
                // Assign node type based on position
                AssignNodeTypeToCell(cell);
                
                // Update sprite based on node type
                cell.UpdateSpriteForNodeType();
            }
            else
            {
                errorCount++;
            }
        }

        Debug.Log($"[CellPositionAssigner] Complete assignment: {assignedCount} successful, {errorCount} errors");
        
        if (assignedCount > 0)
        {
            EditorUtility.SetDirty(FindObjectOfType<PassiveTreeManager>());
        }
    }

    /// <summary>
    /// Complete setup: positions, components, sprites, and node types
    /// </summary>
    private void CompleteCellSetup()
    {
        CellController[] allCells = FindObjectsOfType<CellController>();
        int assignedCount = 0;
        int errorCount = 0;
        int componentCount = 0;

        Debug.Log($"[CellPositionAssigner] Starting complete setup for {allCells.Length} cells");

        foreach (CellController cell in allCells)
        {
            if (AssignPositionToCell(cell))
            {
                assignedCount++;
                
                // Auto-detect and assign components
                if (AssignComponentsToCell(cell))
                {
                    componentCount++;
                }
                
                // Assign node type based on position
                AssignNodeTypeToCell(cell);
                
                // Update sprite based on node type
                cell.UpdateSpriteForNodeType();
            }
            else
            {
                errorCount++;
            }
        }

        Debug.Log($"[CellPositionAssigner] Complete setup finished: {assignedCount} positions, {componentCount} components, {errorCount} errors");
        
        if (assignedCount > 0)
        {
            EditorUtility.SetDirty(FindObjectOfType<PassiveTreeManager>());
        }
    }

    /// <summary>
    /// Assign positions to selected cells only
    /// </summary>
    private void AssignSelectedCellPositions()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int assignedCount = 0;
        int errorCount = 0;

        Debug.Log($"[CellPositionAssigner] Processing {selectedObjects.Length} selected objects");

        foreach (GameObject obj in selectedObjects)
        {
            CellController cell = obj.GetComponent<CellController>();
            if (cell != null)
            {
                if (AssignPositionToCell(cell))
                {
                    assignedCount++;
                }
                else
                {
                    errorCount++;
                }
            }
        }

        Debug.Log($"[CellPositionAssigner] Assignment complete: {assignedCount} successful, {errorCount} errors");
    }

    /// <summary>
    /// Validate all cell names in the scene
    /// </summary>
    private void ValidateCellNames()
    {
        CellController[] allCells = FindObjectsOfType<CellController>();
        List<string> validNames = new List<string>();
        List<string> invalidNames = new List<string>();

        foreach (CellController cell in allCells)
        {
            if (IsValidCellName(cell.name))
            {
                validNames.Add(cell.name);
            }
            else
            {
                invalidNames.Add(cell.name);
            }
        }

        Debug.Log($"[CellPositionAssigner] Validation Results:");
        Debug.Log($"Valid names ({validNames.Count}): {string.Join(", ", validNames)}");
        
        if (invalidNames.Count > 0)
        {
            Debug.LogWarning($"Invalid names ({invalidNames.Count}): {string.Join(", ", invalidNames)}");
        }
    }

    /// <summary>
    /// Assign position to a specific cell based on its name
    /// </summary>
    private bool AssignPositionToCell(CellController cell)
    {
        if (cell == null)
        {
            Debug.LogError("[CellPositionAssigner] Cell is null");
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
                EditorUtility.SetDirty(cell);
                Debug.Log($"[CellPositionAssigner] Assigned position {position.Value} to {cell.name}");
                return true;
            }
            else
            {
                Debug.LogError($"[CellPositionAssigner] Could not find gridPosition field in CellController");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"[CellPositionAssigner] Could not parse position from name: {cell.name}");
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
    /// Check if a cell name follows the expected format
    /// </summary>
    private bool IsValidCellName(string cellName)
    {
        return ParseCellName(cellName).HasValue;
    }

    /// <summary>
    /// Auto-detect and assign SpriteRenderer and Button components to a cell
    /// </summary>
    private bool AssignComponentsToCell(CellController cell)
    {
        if (cell == null)
        {
            Debug.LogError("[CellPositionAssigner] Cell is null");
            return false;
        }

        bool componentsAssigned = false;

        // Auto-detect and assign SpriteRenderer
        SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Use reflection to set the private field
            var spriteField = typeof(CellController).GetField("spriteRenderer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (spriteField != null)
            {
                spriteField.SetValue(cell, spriteRenderer);
                Debug.Log($"[CellPositionAssigner] Auto-assigned SpriteRenderer to {cell.name}");
                componentsAssigned = true;
            }
        }
        else
        {
            Debug.LogWarning($"[CellPositionAssigner] No SpriteRenderer found on {cell.name}");
        }

        // Auto-detect and assign Button
        Button button = cell.GetComponent<Button>();
        if (button != null)
        {
            // Use reflection to set the private field
            var buttonField = typeof(CellController).GetField("button", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (buttonField != null)
            {
                buttonField.SetValue(cell, button);
                Debug.Log($"[CellPositionAssigner] Auto-assigned Button to {cell.name}");
                componentsAssigned = true;
            }
        }
        else
        {
            Debug.LogWarning($"[CellPositionAssigner] No Button found on {cell.name}");
        }

        if (componentsAssigned)
        {
            EditorUtility.SetDirty(cell);
        }

        return componentsAssigned;
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
            Debug.Log($"[CellPositionAssigner] Assigned {nodeType} to {cell.name}");
        }
    }
    
    /// <summary>
    /// Determine node type based on grid position
    /// </summary>
    private NodeType DetermineNodeType(Vector2Int position)
    {
        // Center cell (3,3) for a 7x7 grid becomes the start node
        if (position.x == 3 && position.y == 3)
        {
            return NodeType.Start;
        }
        
        // Check if this position is an extension point (edge positions)
        if (IsExtensionPoint(position))
        {
            return NodeType.Extension;
        }
        
        // Check if this position is a travel node
        if (IsTravelNode(position))
        {
            return NodeType.Travel;
        }
        
        // Check if this position is a notable node
        if (IsNotableNode(position))
        {
            return NodeType.Notable;
        }
        
        // All remaining positions are small nodes
        return NodeType.Small;
    }
    
    /// <summary>
    /// Check if a position is an extension point (edge positions)
    /// </summary>
    private bool IsExtensionPoint(Vector2Int position)
    {
        // Default extension points at the edges of a 7x7 grid
        Vector2Int[] extensionPoints = new Vector2Int[]
        {
            new Vector2Int(0, 3),  // South
            new Vector2Int(3, 0),  // West
            new Vector2Int(3, 6),  // East
            new Vector2Int(6, 3)   // North
        };
        
        foreach (Vector2Int extensionPos in extensionPoints)
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
        // Travel nodes at specific positions
        Vector2Int[] travelNodes = new Vector2Int[]
        {
            new Vector2Int(1, 3),
            new Vector2Int(2, 3),
            new Vector2Int(3, 1),
            new Vector2Int(3, 2),
            new Vector2Int(3, 4),
            new Vector2Int(3, 5),
            new Vector2Int(4, 3),
            new Vector2Int(5, 3)
        };
        
        foreach (Vector2Int travelPos in travelNodes)
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
        // Notable nodes at corner positions
        Vector2Int[] notableNodes = new Vector2Int[]
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, 5),
            new Vector2Int(5, 1),
            new Vector2Int(5, 5)
        };
        
        foreach (Vector2Int notablePos in notableNodes)
        {
            if (position == notablePos)
                return true;
        }
        
        return false;
    }
}
