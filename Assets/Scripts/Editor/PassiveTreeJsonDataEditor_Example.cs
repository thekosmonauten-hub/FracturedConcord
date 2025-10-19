using UnityEngine;
using UnityEditor;

namespace PassiveTreeEditor
{
    /// <summary>
    /// Example script showing how to use the PassiveTreeJsonDataEditor programmatically
    /// </summary>
    public class PassiveTreeJsonDataEditor_Example : MonoBehaviour
    {
        [Header("Example Usage")]
        [SerializeField] private GameObject targetBoard;
        
        [ContextMenu("Open JSON Data Editor")]
        public void OpenJsonDataEditor()
        {
            // Open the editor window
            PassiveTreeJsonDataEditor.ShowWindow();
        }
        
        [ContextMenu("Find All Boards in Scene")]
        public void FindAllBoards()
        {
            // Find all boards with CellJsonData
            var boards = FindObjectsOfType<CellJsonData>();
            Debug.Log($"Found {boards.Length} cells with JSON data in the scene");
            
            foreach (var cell in boards)
            {
                Debug.Log($"Cell: {cell.gameObject.name} - Board: {cell.transform.parent.name}");
            }
        }
        
        [ContextMenu("Quick Stats Check")]
        public void QuickStatsCheck()
        {
            if (targetBoard == null)
            {
                Debug.LogWarning("No target board assigned!");
                return;
            }
            
            var cells = targetBoard.GetComponentsInChildren<CellJsonData>();
            int cellsWithStats = 0;
            int totalStats = 0;
            
            foreach (var cell in cells)
            {
                if (cell.NodeStats != null)
                {
                    var stats = cell.NodeStats;
                    int cellStats = 0;
                    
                    // Count non-zero stats
                    if (stats.strength != 0) cellStats++;
                    if (stats.dexterity != 0) cellStats++;
                    if (stats.intelligence != 0) cellStats++;
                    if (stats.increasedPhysicalDamage != 0) cellStats++;
                    if (stats.addedPhysicalAsFire != 0) cellStats++;
                    if (stats.armorIncrease != 0) cellStats++;
                    
                    if (cellStats > 0)
                    {
                        cellsWithStats++;
                        totalStats += cellStats;
                    }
                }
            }
            
            Debug.Log($"Board: {targetBoard.name}");
            Debug.Log($"Total cells: {cells.Length}");
            Debug.Log($"Cells with stats: {cellsWithStats}");
            Debug.Log($"Total stats: {totalStats}");
        }
    }
}
