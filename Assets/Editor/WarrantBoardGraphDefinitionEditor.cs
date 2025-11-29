using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WarrantBoardGraphDefinition))]
public class WarrantBoardGraphDefinitionEditor : Editor
{
    private float width = 240f;
    private float height = 160f;
    private int nodesPerEdge = 3;
    private Vector2 anchorPosition = new Vector2(0f, -120f);

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Generation", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Click 'Generate Rectangle Frame' to create a complete working tree instantly.", MessageType.Info);

        EditorGUILayout.Space();
        width = EditorGUILayout.FloatField("Rectangle Width", width);
        height = EditorGUILayout.FloatField("Rectangle Height", height);
        nodesPerEdge = EditorGUILayout.IntField("Effect Nodes Per Edge", nodesPerEdge);
        anchorPosition = EditorGUILayout.Vector2Field("Anchor Position", anchorPosition);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Rectangle Frame", GUILayout.Height(30)))
        {
            var graph = (WarrantBoardGraphDefinition)target;
            Undo.RecordObject(graph, "Generate Rectangle Frame");
            graph.GenerateRectangleFrame(width, height, nodesPerEdge, anchorPosition);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            Debug.Log($"Generated rectangle frame with {graph.Nodes.Count} nodes and {graph.Edges.Count} edges!");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Current: {((WarrantBoardGraphDefinition)target).Nodes.Count} nodes, {((WarrantBoardGraphDefinition)target).Edges.Count} edges", EditorStyles.miniLabel);
    }
}














