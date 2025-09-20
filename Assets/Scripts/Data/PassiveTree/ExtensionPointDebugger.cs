using UnityEngine;
using PassiveTree;

/// <summary>
/// Debug script to help identify extension point sprite loading issues
/// </summary>
public class ExtensionPointDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool debugOnStart = true;
    [SerializeField] private bool showDetailedInfo = true;
    
    [Header("Extension Point Positions")]
    [SerializeField] private Vector2Int[] expectedExtensionPoints = new Vector2Int[]
    {
        new Vector2Int(3, 0), // Cell_ext_bottom
        new Vector2Int(0, 3), // Cell_ext_left
        new Vector2Int(6, 3), // Cell_ext_right
        new Vector2Int(3, 6)  // Cell_ext_top
    };
    
    void Start()
    {
        if (debugOnStart)
        {
            DebugExtensionPoints();
        }
    }
    
    /// <summary>
    /// Debug extension points
    /// </summary>
    [ContextMenu("Debug Extension Points")]
    public void DebugExtensionPoints()
    {
        Debug.Log("🔍 [ExtensionPointDebugger] Starting extension point debug...");
        
        // Find JsonBoardDataManager
        var jsonManager = FindFirstObjectByType<JsonBoardDataManager>();
        if (jsonManager == null)
        {
            Debug.LogError("❌ [ExtensionPointDebugger] No JsonBoardDataManager found!");
            return;
        }
        
        Debug.Log($"✅ [ExtensionPointDebugger] Found JsonBoardDataManager: {jsonManager.name}");
        Debug.Log($"📊 [ExtensionPointDebugger] Data loaded: {jsonManager.IsDataLoaded}");
        
        // Check each expected extension point
        foreach (var position in expectedExtensionPoints)
        {
            DebugExtensionPoint(position, jsonManager);
        }
        
        // Also check all cells to see what we have
        DebugAllCells();
    }
    
    /// <summary>
    /// Debug a specific extension point
    /// </summary>
    private void DebugExtensionPoint(Vector2Int position, JsonBoardDataManager jsonManager)
    {
        Debug.Log($"\n🔍 [ExtensionPointDebugger] Checking extension point at {position}:");
        
        // Check JSON data
        var jsonData = jsonManager.GetNodeData(position);
        if (jsonData != null)
        {
            Debug.Log($"  ✅ JSON Data Found:");
            Debug.Log($"    - ID: '{jsonData.id}'");
            Debug.Log($"    - Name: '{jsonData.name}'");
            Debug.Log($"    - Type: '{jsonData.type}'");
            Debug.Log($"    - Position: ({jsonData.position.column}, {jsonData.position.row})");
        }
        else
        {
            Debug.LogWarning($"  ❌ No JSON data found for position {position}");
        }
        
        // Check CellController
        var cellController = FindCellControllerAtPosition(position);
        if (cellController != null)
        {
            Debug.Log($"  ✅ CellController Found:");
            Debug.Log($"    - GameObject: {cellController.gameObject.name}");
            Debug.Log($"    - Node Type: {cellController.GetNodeType()}");
            Debug.Log($"    - Node Name: '{cellController.GetNodeName()}'");
            Debug.Log($"    - Auto Assign Sprite: {cellController.GetAutoAssignSprite()}");
            
            // Check sprite assignment
            var spriteRenderer = cellController.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Debug.Log($"    - Current Sprite: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "NULL")}");
            }
            
            // Check if extension point sprite is assigned
            var extensionSprite = cellController.GetExtensionPointSprite();
            Debug.Log($"    - Extension Point Sprite: {(extensionSprite != null ? extensionSprite.name : "NULL")}");
        }
        else
        {
            Debug.LogWarning($"  ❌ No CellController found for position {position}");
        }
    }
    
    /// <summary>
    /// Debug all cells to see what we have
    /// </summary>
    private void DebugAllCells()
    {
        Debug.Log("\n📊 [ExtensionPointDebugger] All cells in scene:");
        
        var allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        Debug.Log($"  Found {allCells.Length} CellController components");
        
        foreach (var cell in allCells)
        {
            if (cell != null)
            {
                var position = cell.GetGridPosition();
                var nodeType = cell.GetNodeType();
                var nodeName = cell.GetNodeName();
                
                Debug.Log($"  - {cell.gameObject.name} at {position}: {nodeType} - '{nodeName}'");
            }
        }
    }
    
    /// <summary>
    /// Find CellController at specific position
    /// </summary>
    private CellController FindCellControllerAtPosition(Vector2Int position)
    {
        var allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        
        foreach (var cell in allCells)
        {
            if (cell != null && cell.GetGridPosition() == position)
            {
                return cell;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Force refresh extension point sprites
    /// </summary>
    [ContextMenu("Force Refresh Extension Point Sprites")]
    public void ForceRefreshExtensionPointSprites()
    {
        Debug.Log("🔄 [ExtensionPointDebugger] Force refreshing extension point sprites...");
        
        foreach (var position in expectedExtensionPoints)
        {
            var cellController = FindCellControllerAtPosition(position);
            if (cellController != null)
            {
                // Force update the sprite
                cellController.UpdateSpriteForJsonData();
                Debug.Log($"  ✅ Refreshed sprite for cell at {position}");
            }
        }
    }
    
    /// <summary>
    /// Check if extension point sprites are assigned
    /// </summary>
    [ContextMenu("Check Extension Point Sprite Assignments")]
    public void CheckExtensionPointSpriteAssignments()
    {
        Debug.Log("🎨 [ExtensionPointDebugger] Checking extension point sprite assignments...");
        
        var allCells = FindObjectsByType<CellController>(FindObjectsSortMode.None);
        int extensionCellsFound = 0;
        int extensionSpritesAssigned = 0;
        
        foreach (var cell in allCells)
        {
            if (cell != null && cell.GetNodeType() == NodeType.Extension)
            {
                extensionCellsFound++;
                var extensionSprite = cell.GetExtensionPointSprite();
                if (extensionSprite != null)
                {
                    extensionSpritesAssigned++;
                }
                
                Debug.Log($"  - {cell.gameObject.name}: Extension sprite = {(extensionSprite != null ? extensionSprite.name : "NULL")}");
            }
        }
        
        Debug.Log($"📊 [ExtensionPointDebugger] Summary:");
        Debug.Log($"  - Extension cells found: {extensionCellsFound}");
        Debug.Log($"  - Extension sprites assigned: {extensionSpritesAssigned}");
        
        if (extensionCellsFound > 0 && extensionSpritesAssigned == 0)
        {
            Debug.LogWarning("⚠️ [ExtensionPointDebugger] Extension cells found but no extension sprites assigned!");
            Debug.LogWarning("   Please assign extension point sprites in the CellController Inspector.");
        }
    }
}

