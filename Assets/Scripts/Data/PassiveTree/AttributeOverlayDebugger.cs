using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Debug tool for attribute overlay issues on extension boards
    /// </summary>
    public class AttributeOverlayDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = true;
        
        [ContextMenu("Debug Attribute Overlays on Extension Boards")]
        public void DebugAttributeOverlaysOnExtensionBoards()
        {
            Debug.Log("[AttributeOverlayDebugger] 🔍 Debugging attribute overlays on extension boards...");
            
            // Find all extension board controllers
            ExtensionBoardController[] extensionBoards = FindObjectsOfType<ExtensionBoardController>();
            Debug.Log($"[AttributeOverlayDebugger] Found {extensionBoards.Length} extension boards");
            
            foreach (var board in extensionBoards)
            {
                Debug.Log($"[AttributeOverlayDebugger] 🔍 Checking board: {board.name}");
                
                // Find all CellController_EXT components in this board
                CellController_EXT[] cells = board.GetComponentsInChildren<CellController_EXT>();
                Debug.Log($"[AttributeOverlayDebugger] Found {cells.Length} cells in board {board.name}");
                
                foreach (var cell in cells)
                {
                    DebugCellAttributeOverlay(cell);
                }
            }
        }
        
        [ContextMenu("Debug All Attribute Overlays")]
        public void DebugAllAttributeOverlays()
        {
            Debug.Log("[AttributeOverlayDebugger] 🔍 Debugging all attribute overlays...");
            
            // Find all CellController components (including CellController_EXT)
            CellController[] allCells = FindObjectsOfType<CellController>();
            Debug.Log($"[AttributeOverlayDebugger] Found {allCells.Length} total cells");
            
            foreach (var cell in allCells)
            {
                DebugCellAttributeOverlay(cell);
            }
        }
        
        [ContextMenu("Force Refresh All Attribute Overlays")]
        public void ForceRefreshAllAttributeOverlays()
        {
            Debug.Log("[AttributeOverlayDebugger] 🔄 Force refreshing all attribute overlays...");
            
            CellController[] allCells = FindObjectsOfType<CellController>();
            
            foreach (var cell in allCells)
            {
                if (cell != null)
                {
                    cell.RefreshAttributeOverlay();
                }
            }
            
            Debug.Log($"[AttributeOverlayDebugger] ✅ Refreshed {allCells.Length} cells");
        }
        
        [ContextMenu("Enable Attribute Overlays on All Cells")]
        public void EnableAttributeOverlaysOnAllCells()
        {
            Debug.Log("[AttributeOverlayDebugger] 🔄 Enabling attribute overlays on all cells...");
            
            CellController[] allCells = FindObjectsOfType<CellController>();
            
            foreach (var cell in allCells)
            {
                if (cell != null)
                {
                    cell.SetAttributeOverlaysEnabled(true);
                }
            }
            
            Debug.Log($"[AttributeOverlayDebugger] ✅ Enabled overlays on {allCells.Length} cells");
        }
        
        [ContextMenu("Force Refresh Extension Board Overlays")]
        public void ForceRefreshExtensionBoardOverlays()
        {
            Debug.Log("[AttributeOverlayDebugger] 🔄 Force refreshing extension board overlays...");
            
            // Find all extension board controllers
            ExtensionBoardController[] extensionBoards = FindObjectsOfType<ExtensionBoardController>();
            
            foreach (var board in extensionBoards)
            {
                Debug.Log($"[AttributeOverlayDebugger] 🔄 Refreshing board: {board.name}");
                
                // Find all CellController_EXT components in this board
                CellController_EXT[] cells = board.GetComponentsInChildren<CellController_EXT>();
                
                foreach (var cell in cells)
                {
                    if (cell != null)
                    {
                        cell.ForceRefreshAttributeOverlay();
                    }
                }
            }
            
            Debug.Log($"[AttributeOverlayDebugger] ✅ Force refreshed extension board overlays");
        }
        
        private void DebugCellAttributeOverlay(CellController cell)
        {
            if (cell == null) return;
            
            string cellName = cell.gameObject.name;
            Vector2Int position = cell.GridPosition;
            
            // Check if this is an extension board cell
            bool isExtensionCell = cell is CellController_EXT;
            string cellType = isExtensionCell ? "EXT" : "CORE";
            
            // Check overlay status
            bool isShowingOverlay = cell.IsShowingAttributeOverlay();
            string overlayInfo = cell.GetOverlayInfo();
            
            // Check JSON data
            var cellJsonData = cell.GetComponent<CellJsonData>();
            bool hasJsonData = cellJsonData != null && cellJsonData.HasJsonData();
            string jsonDataInfo = hasJsonData ? "Yes" : "No";
            
            // Check stats
            string statsInfo = "No stats";
            if (hasJsonData && cellJsonData.NodeStats != null)
            {
                var stats = cellJsonData.NodeStats;
                int strength = stats.strength;
                int dexterity = stats.dexterity;
                int intelligence = stats.intelligence;
                statsInfo = $"Str:{strength}, Dex:{dexterity}, Int:{intelligence}";
            }
            
            Debug.Log($"[AttributeOverlayDebugger] {cellType} Cell {cellName} at {position}:");
            Debug.Log($"  - Has JSON Data: {jsonDataInfo}");
            Debug.Log($"  - Stats: {statsInfo}");
            Debug.Log($"  - Showing Overlay: {isShowingOverlay}");
            Debug.Log($"  - Overlay Info: {overlayInfo}");
            
            // Check overlay sprite renderer
            var overlayRenderer = cell.GetComponentInChildren<SpriteRenderer>();
            if (overlayRenderer != null && overlayRenderer.name.Contains("AttributeOverlay"))
            {
                Debug.Log($"  - Overlay Renderer: {overlayRenderer.name} (Active: {overlayRenderer.gameObject.activeInHierarchy})");
                Debug.Log($"  - Overlay Sprite: {(overlayRenderer.sprite != null ? overlayRenderer.sprite.name : "None")}");
            }
            else
            {
                Debug.Log($"  - Overlay Renderer: Not found or not named 'AttributeOverlay'");
            }
        }
    }
}
