using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarrantBoardGraphBuilder : MonoBehaviour
{
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private WarrantBoardGraphDefinition graphDefinition;
    [SerializeField] private WarrantNodeView socketPrefab;
    [SerializeField] private WarrantNodeView effectPrefab;
    [SerializeField] private WarrantBoardStateController boardStateController;
    [SerializeField] private WarrantIconLibrary iconLibrary;
    [SerializeField] private WarrantLockerGrid lockerGrid;
    [SerializeField] private ItemTooltipManager tooltipManager;
    [SerializeField] private Vector2 nodeSize = new Vector2(42f, 42f);
    [SerializeField] private Color socketColor = new Color(0.67f, 0.13f, 0.15f);
    [SerializeField] private Color specialSocketColor = new Color(0.87f, 0.52f, 0.16f);
    [SerializeField] private Color effectColor = new Color(0.35f, 0.94f, 0.35f);
    [SerializeField] private Color anchorColor = Color.black;
    [SerializeField] private bool buildOnStart = true;
    [SerializeField, Min(0)] private int effectNodesPerEdge = 3;
    [SerializeField] private bool clampEffectNodesToAxis = false;

    private readonly List<WarrantNodeView> nodeViews = new List<WarrantNodeView>();
    private readonly Dictionary<string, WarrantBoardRuntimeGraph.Node> runtimeNodeLookup = new Dictionary<string, WarrantBoardRuntimeGraph.Node>();
    private readonly WarrantBoardRuntimeGraph runtimeGraph = new WarrantBoardRuntimeGraph();

    public WarrantBoardRuntimeGraph RuntimeGraph => runtimeGraph;
    public WarrantBoardGraphDefinition GraphDefinition => graphDefinition;

    private void Awake()
    {
        if (contentRoot == null)
        {
            contentRoot = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        if (buildOnStart)
        {
            BuildGraph();
        }
    }

    public void BuildGraph()
    {
        ClearVisuals();
        runtimeGraph.Clear();
        runtimeNodeLookup.Clear();

        if (graphDefinition == null)
        {
            Debug.LogWarning("[WarrantBoardGraphBuilder] Missing graph definition. Assign one in the inspector.");
            return;
        }

        // Create nodes
        foreach (var nodeDef in graphDefinition.Nodes)
        {
            if (string.IsNullOrWhiteSpace(nodeDef.id))
                continue;

            Vector2 position = (nodeDef.position + graphDefinition.PositionOffset) * graphDefinition.PositionScale;
            var node = runtimeGraph.AddNode(null, nodeDef.id, nodeDef.nodeType, position);
            runtimeNodeLookup[nodeDef.id] = node;
            CreateVisual(node, position);
        }

        // Create edges
        foreach (var edge in graphDefinition.Edges)
        {
            if (string.IsNullOrEmpty(edge.fromNodeId) || string.IsNullOrEmpty(edge.toNodeId))
                continue;

            if (!runtimeNodeLookup.TryGetValue(edge.fromNodeId, out var fromNode))
            {
                Debug.LogWarning($"[WarrantBoardGraphBuilder] Edge references unknown node '{edge.fromNodeId}'.");
                continue;
            }
            if (!runtimeNodeLookup.TryGetValue(edge.toNodeId, out var toNode))
            {
                Debug.LogWarning($"[WarrantBoardGraphBuilder] Edge references unknown node '{edge.toNodeId}'.");
                continue;
            }

            // If effect nodes are already defined in the graph, chain through them
            // Otherwise, auto-generate them along the edge
            if (effectNodesPerEdge > 0)
            {
                var intermediateNodes = GetIntermediateNodes(edge.fromNodeId, edge.toNodeId);
                if (intermediateNodes.Count > 0)
                {
                    // Chain through existing intermediate nodes
                    LinkNodes(fromNode, intermediateNodes[0]);
                    for (int i = 0; i < intermediateNodes.Count - 1; i++)
                    {
                        LinkNodes(intermediateNodes[i], intermediateNodes[i + 1]);
                    }
                    LinkNodes(intermediateNodes[intermediateNodes.Count - 1], toNode);

                    if (edge.bidirectional)
                    {
                        LinkNodes(toNode, intermediateNodes[intermediateNodes.Count - 1]);
                        for (int i = intermediateNodes.Count - 1; i > 0; i--)
                        {
                            LinkNodes(intermediateNodes[i], intermediateNodes[i - 1]);
                        }
                        LinkNodes(intermediateNodes[0], fromNode);
                    }
                }
                else
                {
                    // Auto-generate effect nodes
                    CreateEffectNodesAlongEdge(fromNode, toNode, edge.bidirectional);
                }
            }
            else
            {
                LinkNodes(fromNode, toNode);
                if (edge.bidirectional)
                {
                    LinkNodes(toNode, fromNode);
                }
            }
        }
        
        // Notify state controller that graph is built so it can refresh effect views
        if (boardStateController != null)
        {
            boardStateController.OnGraphBuilt();
        }
    }

    private List<WarrantBoardRuntimeGraph.Node> GetIntermediateNodes(string fromId, string toId)
    {
        var result = new List<WarrantBoardRuntimeGraph.Node>();
        // Find all intermediate nodes between from and to (pattern: "{from}_to_{to}_effect{i}")
        for (int i = 1; i <= 10; i++) // Check up to 10 intermediate nodes
        {
            string nodeId = $"{fromId}_to_{toId}_effect{i}";
            if (runtimeNodeLookup.TryGetValue(nodeId, out var node))
            {
                result.Add(node);
            }
            else
            {
                break; // Nodes should be sequential, stop if gap found
            }
        }
        // Also check reverse pattern in case of bidirectional
        if (result.Count == 0)
        {
            for (int i = 1; i <= 10; i++)
            {
                string nodeId = $"{toId}_to_{fromId}_effect{i}";
                if (runtimeNodeLookup.TryGetValue(nodeId, out var node))
                {
                    result.Insert(0, node); // Insert at beginning for reverse order
                }
                else
                {
                    break;
                }
            }
        }
        return result;
    }

    private void LinkNodes(WarrantBoardRuntimeGraph.Node a, WarrantBoardRuntimeGraph.Node b)
    {
        if (a == null || b == null) return;
        if (a == b) return;

        if (!a.Connections.Contains(b))
        {
            a.Connections.Add(b);
        }
    }

    private void CreateVisual(WarrantBoardRuntimeGraph.Node node, Vector2 anchoredPosition)
    {
        WarrantNodeView viewInstance = null;
        Color nodeColor = effectColor;

        switch (node.NodeType)
        {
            case WarrantNodeType.Socket:
                viewInstance = socketPrefab != null ? Instantiate(socketPrefab, contentRoot) : CreateFallbackView(node.NodeType);
                nodeColor = socketColor;
                break;
            case WarrantNodeType.SpecialSocket:
                viewInstance = socketPrefab != null ? Instantiate(socketPrefab, contentRoot) : CreateFallbackView(node.NodeType);
                nodeColor = specialSocketColor;
                break;
            case WarrantNodeType.Anchor:
                viewInstance = CreateFallbackView(node.NodeType);
                nodeColor = anchorColor;
                break;
            default:
                viewInstance = effectPrefab != null ? Instantiate(effectPrefab, contentRoot) : CreateFallbackView(node.NodeType);
                nodeColor = effectColor;
                break;
        }

        // Ensure the view instance is active
        if (viewInstance != null && viewInstance.gameObject != null)
        {
            viewInstance.gameObject.SetActive(true);
        }
        
        viewInstance.Initialize(node.Id, node.NodeType, nodeColor);
        viewInstance.BindRuntimeNode(node);
            if (viewInstance is WarrantSocketView socketView)
            {
                socketView.Configure(boardStateController, iconLibrary, lockerGrid, tooltipManager, runtimeGraph);
            }
            else if (viewInstance is WarrantEffectView effectView)
            {
                effectView.Configure(boardStateController, lockerGrid, tooltipManager, runtimeGraph);
            }
        var rect = viewInstance.RectTransform;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = nodeSize;
        nodeViews.Add(viewInstance);
    }

    private WarrantNodeView CreateFallbackView(WarrantNodeType type)
    {
        var go = new GameObject($"{type}Node", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(WarrantNodeView));
        go.transform.SetParent(contentRoot, false);
        return go.GetComponent<WarrantNodeView>();
    }

    private void ClearVisuals()
    {
        foreach (var view in nodeViews)
        {
            if (view != null)
            {
                Destroy(view.gameObject);
            }
        }
        nodeViews.Clear();
    }

    private void CreateEffectNodesAlongEdge(WarrantBoardRuntimeGraph.Node fromNode, WarrantBoardRuntimeGraph.Node toNode, bool bidirectional)
    {
        var tempNodes = new List<WarrantBoardRuntimeGraph.Node>();
        Vector2 start = fromNode.Position;
        Vector2 end = toNode.Position;

        for (int i = 0; i < effectNodesPerEdge; i++)
        {
            float t = (float)(i + 1) / (effectNodesPerEdge + 1);
            Vector2 position = Vector2.Lerp(start, end, t);
            if (clampEffectNodesToAxis)
            {
                if (Mathf.Abs(start.x - end.x) > Mathf.Abs(start.y - end.y))
                {
                    position.y = Mathf.Lerp(start.y, end.y, 0.5f);
                }
                else
                {
                    position.x = Mathf.Lerp(start.x, end.x, 0.5f);
                }
            }

            string nodeId = $"EdgeNode_{fromNode.Id}_{toNode.Id}_{i}";
            var newNode = runtimeGraph.AddNode(null, nodeId, WarrantNodeType.Effect, position);
            CreateVisual(newNode, position);
            tempNodes.Add(newNode);
        }

        var sequence = new List<WarrantBoardRuntimeGraph.Node> { fromNode };
        sequence.AddRange(tempNodes);
        sequence.Add(toNode);

        for (int i = 0; i < sequence.Count - 1; i++)
        {
            LinkNodes(sequence[i], sequence[i + 1]);
            if (bidirectional)
            {
                LinkNodes(sequence[i + 1], sequence[i]);
            }
        }
    }
}

