using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WarrantBoardGraphBuilder))]
public class WarrantBoardGraphBuilderEditor : Editor
{
    private WarrantBoardGraphBuilder builder;
    private bool sceneEditEnabled;
    private int edgeFromIndex;
    private int edgeToIndex;

    private void OnEnable()
    {
        builder = (WarrantBoardGraphBuilder)target;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (builder == null || builder.GraphDefinition == null)
        {
            EditorGUILayout.HelpBox("Assign a WarrantBoardGraphDefinition to edit nodes visually.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graph Editing", EditorStyles.boldLabel);

        sceneEditEnabled = EditorGUILayout.Toggle("Scene Editing", sceneEditEnabled);

        var graph = builder.GraphDefinition;
#if UNITY_EDITOR
        var nodeList = graph.EditorNodes;
#else
        var nodeList = new List<WarrantBoardGraphDefinition.NodeDefinition>();
#endif

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Node At Origin"))
        {
            Undo.RecordObject(graph, "Add Warrant Node");
            string newId = $"Node_{nodeList.Count}";
            nodeList.Add(new WarrantBoardGraphDefinition.NodeDefinition
            {
                id = newId,
                nodeType = WarrantNodeType.Effect,
                position = Vector2.zero
            });
            EditorUtility.SetDirty(graph);
        }

        if (nodeList.Count >= 2)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add Edge", EditorStyles.boldLabel);

            string[] names = nodeList.ConvertAll(n => n.id).ToArray();
            edgeFromIndex = Mathf.Clamp(edgeFromIndex, 0, names.Length - 1);
            edgeToIndex = Mathf.Clamp(edgeToIndex, 0, names.Length - 1);

            edgeFromIndex = EditorGUILayout.Popup("From", edgeFromIndex, names);
            edgeToIndex = EditorGUILayout.Popup("To", edgeToIndex, names);
            bool bidirectional = true;

            if (GUILayout.Button("Create Edge"))
            {
                if (edgeFromIndex != edgeToIndex)
                {
                    Undo.RecordObject(graph, "Add Warrant Edge");
                    graph.EditorEdges.Add(new WarrantBoardGraphDefinition.EdgeDefinition
                    {
                        fromNodeId = names[edgeFromIndex],
                        toNodeId = names[edgeToIndex],
                        bidirectional = bidirectional
                    });
                    EditorUtility.SetDirty(graph);
                }
                else
                {
                    Debug.LogWarning("Cannot create an edge from a node to itself.");
                }
            }
        }
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        if (!sceneEditEnabled || builder == null || builder.GraphDefinition == null)
            return;

        var graph = builder.GraphDefinition;
#if UNITY_EDITOR
        var nodes = graph.EditorNodes;
        var edges = graph.EditorEdges;
#else
        return;
#endif

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Color socketColor = new Color(0.9f, 0.2f, 0.2f);
        Color effectColor = new Color(0.2f, 0.95f, 0.3f);
        Color anchorColor = Color.black;

        // Draw edges first
        Handles.color = Color.white;
        foreach (var edge in edges)
        {
            var fromNode = graph.FindNode(edge.fromNodeId);
            var toNode = graph.FindNode(edge.toNodeId);
            if (fromNode == null || toNode == null) continue;

            Vector3 fromPos = ToWorld(builder.transform, fromNode.position);
            Vector3 toPos = ToWorld(builder.transform, toNode.position);
            Handles.DrawLine(fromPos, toPos);
        }

        // Draw node handles
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            Vector3 worldPos = ToWorld(builder.transform, node.position);
            float size = HandleUtility.GetHandleSize(worldPos) * 0.07f;

            Handles.color = node.nodeType switch
            {
                WarrantNodeType.Socket => socketColor,
                WarrantNodeType.Anchor => anchorColor,
                _ => effectColor
            };

            EditorGUI.BeginChangeCheck();
            var fmh_140_68_638987520853225025 = Quaternion.identity; Vector3 newWorldPos = Handles.FreeMoveHandle(worldPos, size, Vector3.zero, Handles.SphereHandleCap);
            Handles.Label(worldPos + Vector3.up * size * 1.5f, node.id);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(graph, "Move Warrant Node");
                Vector3 local = builder.transform.InverseTransformPoint(newWorldPos);
                node.position = new Vector2(local.x, local.y);
                EditorUtility.SetDirty(graph);
            }
        }
    }

    private static Vector3 ToWorld(Transform root, Vector2 point)
    {
        return root != null
            ? root.TransformPoint(new Vector3(point.x, point.y, 0f))
            : new Vector3(point.x, point.y, 0f);
    }
}

