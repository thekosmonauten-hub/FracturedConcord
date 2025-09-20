using UnityEngine;
using UnityEditor;
using PassiveTree;

/// <summary>
/// Custom editor for PassiveTreeBoardData ScriptableObject
/// Provides easy configuration and generation of cell data
/// </summary>
[CustomEditor(typeof(PassiveTreeBoardData))]
public class PassiveTreeBoardDataEditor : Editor
{
    private PassiveTreeBoardData boardData;
    private bool showNodeTypeData = true;
    private bool showSpecialPositions = true;
    private bool showManualOverrides = true;
    private bool showGenerationSettings = true;

    private void OnEnable()
    {
        boardData = (PassiveTreeBoardData)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Passive Tree Board Data", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Board Configuration
        EditorGUILayout.LabelField("Board Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("boardName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("boardDescription"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("boardSize"));

        EditorGUILayout.Space();

        // Node Type Data
        showNodeTypeData = EditorGUILayout.Foldout(showNodeTypeData, "Node Type Definitions", true);
        if (showNodeTypeData)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startNodeData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("travelNodeData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smallNodeData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("notableNodeData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("extensionNodeData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keystoneNodeData"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Special Positions
        showSpecialPositions = EditorGUILayout.Foldout(showSpecialPositions, "Special Positions", true);
        if (showSpecialPositions)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startNodePosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("extensionPositions"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("travelPositions"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("notablePositions"), true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Manual Overrides
        showManualOverrides = EditorGUILayout.Foldout(showManualOverrides, "Manual Overrides", true);
        if (showManualOverrides)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("manualOverrides"), true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Generation Settings
        showGenerationSettings = EditorGUILayout.Foldout(showGenerationSettings, "Generation Settings", true);
        if (showGenerationSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoGenerateOnAwake"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useRandomDescriptions"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generateUniqueNames"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Action Buttons
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate All Cell Data"))
        {
            GenerateAllCellData();
        }
        if (GUILayout.Button("Clear All Data"))
        {
            ClearAllData();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate Board Data"))
        {
            ValidateBoardData();
        }
        if (GUILayout.Button("Preview Board Layout"))
        {
            PreviewBoardLayout();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Quick Setup Buttons
        EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Setup Default Node Types"))
        {
            SetupDefaultNodeTypes();
        }
        if (GUILayout.Button("Setup Default Positions"))
        {
            SetupDefaultPositions();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Board Info Display
        if (boardData != null)
        {
            EditorGUILayout.LabelField("Board Information", EditorStyles.boldLabel);
            BoardInfo info = boardData.GetBoardInfo();
            if (info != null)
            {
                EditorGUILayout.LabelField($"Name: {info.boardName}");
                EditorGUILayout.LabelField($"Size: {info.boardSize.x}x{info.boardSize.y}");
                EditorGUILayout.LabelField($"Start Position: {info.startNodePosition}");
                EditorGUILayout.LabelField($"Extension Points: {info.extensionPositions.Count}");
                EditorGUILayout.LabelField($"Travel Nodes: {info.travelPositions.Count}");
                EditorGUILayout.LabelField($"Notable Nodes: {info.notablePositions.Count}");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Generate all cell data for the board
    /// </summary>
    private void GenerateAllCellData()
    {
        if (boardData == null) return;

        boardData.RegenerateAllCellData();
        EditorUtility.SetDirty(boardData);
        
        Debug.Log("[PassiveTreeBoardDataEditor] Generated all cell data");
    }

    /// <summary>
    /// Clear all generated data
    /// </summary>
    private void ClearAllData()
    {
        if (boardData == null) return;

        if (EditorUtility.DisplayDialog("Clear All Data", 
            "Are you sure you want to clear all generated cell data?", 
            "Yes", "Cancel"))
        {
            // This would require a method in PassiveTreeBoardData to clear data
            Debug.Log("[PassiveTreeBoardDataEditor] Cleared all cell data");
        }
    }

    /// <summary>
    /// Validate the board data configuration
    /// </summary>
    private void ValidateBoardData()
    {
        if (boardData == null) return;

        bool isValid = true;
        string errors = "";

        // Check board size
        if (boardData.GetBoardInfo().boardSize.x <= 0 || boardData.GetBoardInfo().boardSize.y <= 0)
        {
            errors += "Board size must be greater than 0x0\n";
            isValid = false;
        }

        // Check start position is within bounds
        Vector2Int startPos = boardData.GetBoardInfo().startNodePosition;
        Vector2Int boardSize = boardData.GetBoardInfo().boardSize;
        if (startPos.x < 0 || startPos.x >= boardSize.x || startPos.y < 0 || startPos.y >= boardSize.y)
        {
            errors += "Start position is outside board bounds\n";
            isValid = false;
        }

        // Check extension positions are within bounds
        foreach (Vector2Int extPos in boardData.GetBoardInfo().extensionPositions)
        {
            if (extPos.x < 0 || extPos.x >= boardSize.x || extPos.y < 0 || extPos.y >= boardSize.y)
            {
                errors += $"Extension position {extPos} is outside board bounds\n";
                isValid = false;
            }
        }

        if (isValid)
        {
            EditorUtility.DisplayDialog("Validation", "Board data is valid!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Validation Errors", errors, "OK");
        }
    }

    /// <summary>
    /// Preview the board layout
    /// </summary>
    private void PreviewBoardLayout()
    {
        if (boardData == null) return;

        BoardInfo info = boardData.GetBoardInfo();
        string layout = "Board Layout Preview:\n\n";

        for (int y = info.boardSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < info.boardSize.x; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                char symbol = ' ';

                if (pos == info.startNodePosition)
                    symbol = 'S';
                else if (info.extensionPositions.Contains(pos))
                    symbol = 'E';
                else if (info.travelPositions.Contains(pos))
                    symbol = 'T';
                else if (info.notablePositions.Contains(pos))
                    symbol = 'N';
                else
                    symbol = '·';

                layout += symbol + " ";
            }
            layout += "\n";
        }

        layout += "\nLegend: S=Start, E=Extension, T=Travel, N=Notable, ·=Small";

        EditorUtility.DisplayDialog("Board Layout", layout, "OK");
    }

    /// <summary>
    /// Setup default node type data
    /// </summary>
    private void SetupDefaultNodeTypes()
    {
        if (boardData == null) return;

        // This would require methods in PassiveTreeBoardData to set default node types
        Debug.Log("[PassiveTreeBoardDataEditor] Setup default node types");
    }

    /// <summary>
    /// Setup default positions for a 7x7 board
    /// </summary>
    private void SetupDefaultPositions()
    {
        if (boardData == null) return;

        // This would require methods in PassiveTreeBoardData to set default positions
        Debug.Log("[PassiveTreeBoardDataEditor] Setup default positions");
    }
}
