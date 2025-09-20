using UnityEngine;
using UnityEditor;
using PassiveTree;

/// <summary>
/// Editor tool to convert TypeScript passive tree data to C# ScriptableObject
/// Provides easy conversion and application of CoreBoard.ts data
/// </summary>
public class TypeScriptDataConverterEditor : EditorWindow
{
    private PassiveTreeBoardData targetBoardData;
    private Vector2 scrollPosition;
    private bool showPreview = true;
    private bool showStats = false;

    [MenuItem("Tools/Passive Tree/TypeScript Data Converter")]
    public static void ShowWindow()
    {
        GetWindow<TypeScriptDataConverterEditor>("TS Data Converter");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("TypeScript to C# Data Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Target board data selection
        EditorGUILayout.LabelField("Target Board Data", EditorStyles.boldLabel);
        targetBoardData = (PassiveTreeBoardData)EditorGUILayout.ObjectField("Board Data", targetBoardData, typeof(PassiveTreeBoardData), false);

        EditorGUILayout.Space();

        // Conversion buttons
        EditorGUILayout.LabelField("Conversion Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Convert CoreBoard.ts Data"))
        {
            ConvertCoreBoardData();
        }
        if (GUILayout.Button("Create New Board Data"))
        {
            CreateNewBoardData();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Preview section
        showPreview = EditorGUILayout.Foldout(showPreview, "Data Preview", true);
        if (showPreview && targetBoardData != null)
        {
            EditorGUI.indentLevel++;
            ShowDataPreview();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Stats section
        showStats = EditorGUILayout.Foldout(showStats, "Conversion Statistics", true);
        if (showStats)
        {
            EditorGUI.indentLevel++;
            ShowConversionStats();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Help section
        EditorGUILayout.LabelField("Help", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool converts TypeScript passive tree data from CoreBoard.ts to C# ScriptableObject format.\n\n" +
            "1. Select or create a PassiveTreeBoardData asset\n" +
            "2. Click 'Convert CoreBoard.ts Data' to apply the conversion\n" +
            "3. The tool will create manual overrides for all 49 nodes\n" +
            "4. All node names, descriptions, costs, and stats will be preserved",
            MessageType.Info
        );
    }

    /// <summary>
    /// Convert CoreBoard.ts data to the target board data
    /// </summary>
    private void ConvertCoreBoardData()
    {
        if (targetBoardData == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a target board data asset first.", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Convert Data", 
            "This will overwrite the current board data with TypeScript data. Continue?", 
            "Yes", "Cancel"))
        {
            // Create converted data
            PassiveTreeBoardData convertedData = TypeScriptDataConverter.ConvertCoreBoardData();
            
            // Copy the converted data to the target
            CopyBoardData(convertedData, targetBoardData);
            
            // Mark as dirty and save
            EditorUtility.SetDirty(targetBoardData);
            AssetDatabase.SaveAssets();
            
            Debug.Log("[TypeScriptDataConverterEditor] Successfully converted CoreBoard.ts data to C# ScriptableObject");
            EditorUtility.DisplayDialog("Success", "TypeScript data has been converted and applied to the board data asset!", "OK");
        }
    }

    /// <summary>
    /// Create a new board data asset with converted data
    /// </summary>
    private void CreateNewBoardData()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Board Data Asset",
            "CorePassiveTreeData",
            "asset",
            "Choose where to save the converted board data asset"
        );

        if (!string.IsNullOrEmpty(path))
        {
            // Create converted data
            PassiveTreeBoardData convertedData = TypeScriptDataConverter.ConvertCoreBoardData();
            
            // Create asset
            AssetDatabase.CreateAsset(convertedData, path);
            AssetDatabase.SaveAssets();
            
            // Select the new asset
            Selection.activeObject = convertedData;
            EditorGUIUtility.PingObject(convertedData);
            
            targetBoardData = convertedData;
            
            Debug.Log($"[TypeScriptDataConverterEditor] Created new board data asset: {path}");
            EditorUtility.DisplayDialog("Success", $"New board data asset created at: {path}", "OK");
        }
    }

    /// <summary>
    /// Copy board data from source to target
    /// </summary>
    private void CopyBoardData(PassiveTreeBoardData source, PassiveTreeBoardData target)
    {
        // Copy all fields using reflection
        var fields = typeof(PassiveTreeBoardData).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            // Check if field has SerializeField attribute
            if (field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
            {
                object value = field.GetValue(source);
                field.SetValue(target, value);
            }
        }
    }

    /// <summary>
    /// Show preview of the board data
    /// </summary>
    private void ShowDataPreview()
    {
        if (targetBoardData == null) return;

        BoardInfo info = targetBoardData.GetBoardInfo();
        if (info == null) return;

        EditorGUILayout.LabelField($"Name: {info.boardName}");
        EditorGUILayout.LabelField($"Description: {info.boardDescription}");
        EditorGUILayout.LabelField($"Size: {info.boardSize.x}x{info.boardSize.y}");
        EditorGUILayout.LabelField($"Start Position: {info.startNodePosition}");
        EditorGUILayout.LabelField($"Extension Points: {info.extensionPositions.Count}");
        EditorGUILayout.LabelField($"Travel Nodes: {info.travelPositions.Count}");
        EditorGUILayout.LabelField($"Notable Nodes: {info.notablePositions.Count}");

        // Show some sample nodes
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sample Nodes:", EditorStyles.boldLabel);
        
        var allData = targetBoardData.GetAllCellData();
        int count = 0;
        foreach (var kvp in allData)
        {
            if (count >= 5) break; // Show only first 5 nodes
            
            EditorGUILayout.LabelField($"  {kvp.Key}: {kvp.Value.NodeName} ({kvp.Value.NodeType})");
            count++;
        }
        
        if (allData.Count > 5)
        {
            EditorGUILayout.LabelField($"  ... and {allData.Count - 5} more nodes");
        }
    }

    /// <summary>
    /// Show conversion statistics
    /// </summary>
    private void ShowConversionStats()
    {
        EditorGUILayout.LabelField("TypeScript Data Structure:");
        EditorGUILayout.LabelField("  • 7x7 grid (49 total nodes)");
        EditorGUILayout.LabelField("  • 1 Start node (center)");
        EditorGUILayout.LabelField("  • 4 Extension points (edges)");
        EditorGUILayout.LabelField("  • 4 Notable nodes (corners)");
        EditorGUILayout.LabelField("  • 40 Small/Travel nodes");
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Conversion Features:");
        EditorGUILayout.LabelField("  • All node names preserved");
        EditorGUILayout.LabelField("  • All descriptions preserved");
        EditorGUILayout.LabelField("  • All costs preserved");
        EditorGUILayout.LabelField("  • All stats preserved");
        EditorGUILayout.LabelField("  • Proper node type mapping");
        EditorGUILayout.LabelField("  • Color coding by type");
    }
}
