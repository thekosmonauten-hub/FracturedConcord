using UnityEngine;
using UnityEditor;
using PassiveTree;

/// <summary>
/// Editor script to swap X and Y values in GridPosition for CellController components
/// This fixes reversed grid positions on the CoreBoard prefab
/// </summary>
public class GridPositionSwapper : EditorWindow
{
    private GameObject targetBoard;
    private bool showPreview = true;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Grid Position Swapper")]
    public static void ShowWindow()
    {
        GetWindow<GridPositionSwapper>("Grid Position Swapper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Position Swapper", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This tool swaps X and Y values in GridPosition for CellController components.");
        GUILayout.Label("Use this to fix reversed grid positions on the CoreBoard prefab.");
        GUILayout.Space(10);

        // Target board selection
        GUILayout.Label("Target Board:", EditorStyles.label);
        targetBoard = (GameObject)EditorGUILayout.ObjectField(targetBoard, typeof(GameObject), true);

        GUILayout.Space(10);

        // Show preview option
        showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);

        GUILayout.Space(10);

        // Preview section
        if (showPreview && targetBoard != null)
        {
            ShowPreview();
        }

        GUILayout.Space(10);

        // Action buttons
        EditorGUI.BeginDisabledGroup(targetBoard == null);
        
        if (GUILayout.Button("Swap Grid Positions", GUILayout.Height(30)))
        {
            SwapGridPositions();
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);

        // Help section
        EditorGUILayout.HelpBox(
            "Instructions:\n" +
            "1. Select the CoreBoard prefab or GameObject\n" +
            "2. Click 'Swap Grid Positions' to fix reversed coordinates\n" +
            "3. The script will swap X and Y values for all CellController components\n" +
            "4. This will fix issues where grid positions are reversed",
            MessageType.Info
        );
    }

    private void ShowPreview()
    {
        GUILayout.Label("Preview:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        
        CellController[] cells = targetBoard.GetComponentsInChildren<CellController>();
        
        if (cells.Length == 0)
        {
            EditorGUILayout.LabelField("No CellController components found in children.");
        }
        else
        {
            EditorGUILayout.LabelField($"Found {cells.Length} CellController components:");
            GUILayout.Space(5);

            foreach (CellController cell in cells)
            {
                Vector2Int currentPos = cell.GridPosition;
                Vector2Int swappedPos = new Vector2Int(currentPos.y, currentPos.x);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"• {cell.gameObject.name}:", GUILayout.Width(150));
                EditorGUILayout.LabelField($"({currentPos.x},{currentPos.y})", GUILayout.Width(80));
                EditorGUILayout.LabelField("→", GUILayout.Width(20));
                EditorGUILayout.LabelField($"({swappedPos.x},{swappedPos.y})", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void SwapGridPositions()
    {
        if (targetBoard == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a target board first.", "OK");
            return;
        }

        CellController[] cells = targetBoard.GetComponentsInChildren<CellController>();
        
        if (cells.Length == 0)
        {
            EditorUtility.DisplayDialog("No Cells Found", "No CellController components found in the target board.", "OK");
            return;
        }

        // Show confirmation dialog
        bool confirmed = EditorUtility.DisplayDialog(
            "Confirm Grid Position Swap",
            $"This will swap X and Y values for {cells.Length} CellController components.\n\n" +
            "This action cannot be undone easily.\n\n" +
            "Do you want to continue?",
            "Yes, Swap Positions",
            "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        // Record undo operation
        Undo.RecordObjects(cells, "Swap Grid Positions");

        int swappedCount = 0;
        
            foreach (CellController cell in cells)
            {
                Vector2Int currentPos = cell.GridPosition;
                Vector2Int swappedPos = new Vector2Int(currentPos.y, currentPos.x);
                
                // Swap the positions using the SetGridPosition method
                cell.SetGridPosition(swappedPos);
                swappedCount++;
                
                Debug.Log($"[GridPositionSwapper] Swapped {cell.gameObject.name}: ({currentPos.x},{currentPos.y}) → ({swappedPos.x},{swappedPos.y})");
            }

        // Mark the scene as dirty
        EditorUtility.SetDirty(targetBoard);
        
        // Refresh the preview
        Repaint();

        // Show completion message
        EditorUtility.DisplayDialog(
            "Grid Position Swap Complete",
            $"Successfully swapped grid positions for {swappedCount} CellController components.\n\n" +
            "The changes have been applied to the GameObject.",
            "OK"
        );

        Debug.Log($"[GridPositionSwapper] ✅ Grid position swap complete: {swappedCount} cells updated");
    }
}
