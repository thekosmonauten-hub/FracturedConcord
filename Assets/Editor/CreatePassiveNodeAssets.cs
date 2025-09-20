using UnityEngine;
using UnityEditor;
using PassiveTree;

/// <summary>
/// Editor script to create PassiveNodeData assets for different node types
/// </summary>
public class CreatePassiveNodeAssets : EditorWindow
{
    [MenuItem("Passive Tree/Create Node Assets")]
    public static void ShowWindow()
    {
        GetWindow<CreatePassiveNodeAssets>("Create Passive Node Assets");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Create Passive Tree Node Assets", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will create ScriptableObject assets for different node types.");
        GUILayout.Label("You can then assign sprites and customize the properties.");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create START Node Asset"))
        {
            CreateNodeAsset("START_Node", NodeType.Start, Color.green, true);
        }
        
        if (GUILayout.Button("Create Extension Point Asset"))
        {
            CreateNodeAsset("Extension_Point", NodeType.Extension, Color.blue, false);
        }
        
        if (GUILayout.Button("Create Notable Node Asset"))
        {
            CreateNodeAsset("Notable_Node", NodeType.Notable, Color.yellow, false);
        }
        
        if (GUILayout.Button("Create Travel Node Asset"))
        {
            CreateNodeAsset("Travel_Node", NodeType.Travel, Color.white, false);
        }
        
        if (GUILayout.Button("Create Keystone Asset"))
        {
            CreateNodeAsset("Keystone_Node", NodeType.Keystone, Color.red, false);
        }
        
        if (GUILayout.Button("Create All Node Assets"))
        {
            CreateAllNodeAssets();
        }
        
        GUILayout.Space(20);
        
        GUILayout.Label("After creating assets:", EditorStyles.boldLabel);
        GUILayout.Label("1. Select each asset in the Project window");
        GUILayout.Label("2. Assign appropriate sprites in the Inspector");
        GUILayout.Label("3. Customize colors, descriptions, and effects");
        GUILayout.Label("4. Set up connections between nodes");
    }
    
    /// <summary>
    /// Create a single node asset
    /// </summary>
    private void CreateNodeAsset(string assetName, NodeType nodeType, Color baseColor, bool isStartNode)
    {
        // Create the asset
        PassiveNodeData nodeData = ScriptableObject.CreateInstance<PassiveNodeData>();
        
        // Set default values based on node type
        SetDefaultValues(nodeData, assetName, nodeType, baseColor, isStartNode);
        
        // Save the asset
        string path = $"Assets/ScriptableObjects/PassiveTree/{assetName}.asset";
        
        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        AssetDatabase.CreateAsset(nodeData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select the created asset
        Selection.activeObject = nodeData;
        EditorGUIUtility.PingObject(nodeData);
        
        Debug.Log($"Created {nodeType} node asset: {path}");
    }
    
    /// <summary>
    /// Create all node assets at once
    /// </summary>
    private void CreateAllNodeAssets()
    {
        CreateNodeAsset("START_Node", NodeType.Start, Color.green, true);
        CreateNodeAsset("Extension_Point", NodeType.Extension, Color.blue, false);
        CreateNodeAsset("Notable_Node", NodeType.Notable, Color.yellow, false);
        CreateNodeAsset("Travel_Node", NodeType.Travel, Color.white, false);
        CreateNodeAsset("Keystone_Node", NodeType.Keystone, Color.red, false);
        
        Debug.Log("Created all passive node assets!");
    }
    
    /// <summary>
    /// Set default values for a node asset
    /// </summary>
    private void SetDefaultValues(PassiveNodeData nodeData, string name, NodeType nodeType, Color baseColor, bool isStartNode)
    {
        // Use reflection to set private fields
        var nameField = typeof(PassiveNodeData).GetField("_nodeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var descField = typeof(PassiveNodeData).GetField("_description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var typeField = typeof(PassiveNodeData).GetField("_nodeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var colorField = typeof(PassiveNodeData).GetField("_nodeColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var highlightField = typeof(PassiveNodeData).GetField("_highlightColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var selectedField = typeof(PassiveNodeData).GetField("_selectedColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var unlockedField = typeof(PassiveNodeData).GetField("_isUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var startField = typeof(PassiveNodeData).GetField("_isStartNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var costField = typeof(PassiveNodeData).GetField("_skillPointsCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var effectField = typeof(PassiveNodeData).GetField("_effectDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (nameField != null) nameField.SetValue(nodeData, name);
        if (descField != null) descField.SetValue(nodeData, GetDefaultDescription(nodeType));
        if (typeField != null) typeField.SetValue(nodeData, nodeType);
        if (colorField != null) colorField.SetValue(nodeData, baseColor);
        if (highlightField != null) highlightField.SetValue(nodeData, GetHighlightColor(baseColor));
        if (selectedField != null) selectedField.SetValue(nodeData, GetSelectedColor(baseColor));
        if (unlockedField != null) unlockedField.SetValue(nodeData, isStartNode);
        if (startField != null) startField.SetValue(nodeData, isStartNode);
        if (costField != null) costField.SetValue(nodeData, GetDefaultCost(nodeType));
        if (effectField != null) effectField.SetValue(nodeData, GetDefaultEffect(nodeType));
    }
    
    /// <summary>
    /// Get default description for node type
    /// </summary>
    private string GetDefaultDescription(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return "Starting point of your passive tree journey";
            case NodeType.Extension:
                return "Connection point to other boards";
            case NodeType.Notable:
                return "Powerful passive effect";
            case NodeType.Travel:
                return "Basic attribute node";
            case NodeType.Keystone:
                return "Build-defining keystone passive";
            default:
                return "Passive tree node";
        }
    }
    
    /// <summary>
    /// Get default cost for node type
    /// </summary>
    private int GetDefaultCost(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return 0;
            case NodeType.Extension:
                return 0;
            case NodeType.Notable:
                return 2;
            case NodeType.Travel:
                return 1;
            case NodeType.Keystone:
                return 5;
            default:
                return 1;
        }
    }
    
    /// <summary>
    /// Get default effect description for node type
    /// </summary>
    private string GetDefaultEffect(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Start:
                return "Unlocks the passive tree";
            case NodeType.Extension:
                return "Allows connection to other boards";
            case NodeType.Notable:
                return "Provides significant bonuses";
            case NodeType.Travel:
                return "Small attribute bonus";
            case NodeType.Keystone:
                return "Major build-altering effect";
            default:
                return "No effect";
        }
    }
    
    /// <summary>
    /// Get highlight color based on base color
    /// </summary>
    private Color GetHighlightColor(Color baseColor)
    {
        return Color.Lerp(baseColor, Color.white, 0.3f);
    }
    
    /// <summary>
    /// Get selected color based on base color
    /// </summary>
    private Color GetSelectedColor(Color baseColor)
    {
        return Color.Lerp(baseColor, Color.cyan, 0.5f);
    }
}
