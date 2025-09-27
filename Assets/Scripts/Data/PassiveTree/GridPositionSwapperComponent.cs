using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Component to swap X and Y values in GridPosition for CellController components
    /// This fixes reversed grid positions on the CoreBoard prefab
    /// Can be used as a component or called programmatically
    /// </summary>
    public class GridPositionSwapperComponent : MonoBehaviour
    {
        [Header("Grid Position Swapping")]
        [SerializeField] private bool autoSwapOnStart = false;
        [SerializeField] private bool showDebugLogs = true;

        private void Start()
        {
            if (autoSwapOnStart)
            {
                SwapGridPositions();
            }
        }

        /// <summary>
        /// Swap X and Y values in GridPosition for all CellController components in children
        /// </summary>
        [ContextMenu("Swap Grid Positions")]
        public void SwapGridPositions()
        {
            if (showDebugLogs)
            {
                Debug.Log($"[GridPositionSwapperComponent] Starting grid position swap for {gameObject.name}");
            }

            CellController[] cells = GetComponentsInChildren<CellController>();
            
            if (cells.Length == 0)
            {
                Debug.LogWarning($"[GridPositionSwapperComponent] No CellController components found in {gameObject.name}");
                return;
            }

            int swappedCount = 0;
            
            foreach (CellController cell in cells)
            {
                Vector2Int currentPos = cell.GridPosition;
                Vector2Int swappedPos = new Vector2Int(currentPos.y, currentPos.x);
                
                // Swap the positions using the SetGridPosition method
                // This works for both CellController and CellController_EXT
                cell.SetGridPosition(swappedPos);
                swappedCount++;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[GridPositionSwapperComponent] Swapped {cell.gameObject.name}: ({currentPos.x},{currentPos.y}) → ({swappedPos.x},{swappedPos.y})");
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[GridPositionSwapperComponent] ✅ Grid position swap complete: {swappedCount} cells updated on {gameObject.name}");
            }
        }

        /// <summary>
        /// Swap grid positions for a specific CellController
        /// </summary>
        public void SwapGridPosition(CellController cell)
        {
            if (cell == null)
            {
                Debug.LogWarning("[GridPositionSwapperComponent] Cannot swap grid position - CellController is null");
                return;
            }

            Vector2Int currentPos = cell.GridPosition;
            Vector2Int swappedPos = new Vector2Int(currentPos.y, currentPos.x);
            
            cell.SetGridPosition(swappedPos);
            
            if (showDebugLogs)
            {
                Debug.Log($"[GridPositionSwapperComponent] Swapped {cell.gameObject.name}: ({currentPos.x},{currentPos.y}) → ({swappedPos.x},{swappedPos.y})");
            }
        }

        /// <summary>
        /// Swap grid positions for multiple CellControllers
        /// </summary>
        public void SwapGridPositions(CellController[] cells)
        {
            if (cells == null || cells.Length == 0)
            {
                Debug.LogWarning("[GridPositionSwapperComponent] Cannot swap grid positions - no cells provided");
                return;
            }

            int swappedCount = 0;
            
            foreach (CellController cell in cells)
            {
                if (cell != null)
                {
                    Vector2Int currentPos = cell.GridPosition;
                    Vector2Int swappedPos = new Vector2Int(currentPos.y, currentPos.x);
                    
                    cell.SetGridPosition(swappedPos);
                    swappedCount++;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[GridPositionSwapperComponent] Swapped {cell.gameObject.name}: ({currentPos.x},{currentPos.y}) → ({swappedPos.x},{swappedPos.y})");
                    }
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[GridPositionSwapperComponent] ✅ Grid position swap complete: {swappedCount} cells updated");
            }
        }

        /// <summary>
        /// Preview what the grid position swap would do (read-only)
        /// </summary>
        [ContextMenu("Preview Grid Position Swap")]
        public void PreviewGridPositionSwap()
        {
            Debug.Log($"[GridPositionSwapperComponent] === GRID POSITION SWAP PREVIEW for {gameObject.name} ===");

            CellController[] cells = GetComponentsInChildren<CellController>();
            
            if (cells.Length == 0)
            {
                Debug.LogWarning($"[GridPositionSwapperComponent] No CellController components found in {gameObject.name}");
                return;
            }

            Debug.Log($"[GridPositionSwapperComponent] Found {cells.Length} CellController components:");
            
            foreach (CellController cell in cells)
            {
                Vector2Int currentPos = cell.GridPosition;
                Vector2Int swappedPos = new Vector2Int(currentPos.y, currentPos.x);
                
                Debug.Log($"[GridPositionSwapperComponent] • {cell.gameObject.name}: ({currentPos.x},{currentPos.y}) → ({swappedPos.x},{swappedPos.y})");
            }
            
            Debug.Log($"[GridPositionSwapperComponent] === PREVIEW COMPLETE ===");
        }

        /// <summary>
        /// Reset all CellController grid positions to their original values
        /// Note: This only works if you haven't saved the scene after swapping
        /// </summary>
        [ContextMenu("Reset Grid Positions (Undo)")]
        public void ResetGridPositions()
        {
            Debug.LogWarning("[GridPositionSwapperComponent] Reset functionality not implemented. Use Unity's Undo system or manually restore from backup.");
        }
    }
}
