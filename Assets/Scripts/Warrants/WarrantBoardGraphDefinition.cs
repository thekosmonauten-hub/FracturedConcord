using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WarrantBoardGraph", menuName = "Dexiled/Warrants/Board Graph Definition")]
public class WarrantBoardGraphDefinition : ScriptableObject
{
    [SerializeField] private float positionScale = 1f;
    [SerializeField] private Vector2 positionOffset = Vector2.zero;
    [SerializeField] private List<NodeDefinition> nodes = new List<NodeDefinition>();
    [SerializeField] private List<EdgeDefinition> edges = new List<EdgeDefinition>();

    public float PositionScale => Mathf.Max(0.01f, positionScale);
    public Vector2 PositionOffset => positionOffset;
    public IReadOnlyList<NodeDefinition> Nodes => nodes;
    public IReadOnlyList<EdgeDefinition> Edges => edges;

    public NodeDefinition FindNode(string id) => nodes.Find(n => n.id == id);

    /// <summary>
    /// Generate a rectangular frame with center hub pattern:
    /// - Anchor at bottom
    /// - Three branches (left, center, right) from anchor
    /// - Branches form a rectangular frame
    /// - Center hub connects to all corners and midpoints
    /// </summary>
    public void GenerateRectangleFrame(
        float width = 220f,
        float height = 140f,
        int nodesPerEdge = 3,
        Vector2 anchorPosition = default)
    {
        nodes.Clear();
        edges.Clear();

        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        if (anchorPosition == default)
            anchorPosition = new Vector2(0f, -halfHeight - 40f);

        // Anchor node
        AddNode("Anchor", WarrantNodeType.Anchor, anchorPosition);

        // Corners
        Vector2 topLeft = new Vector2(-halfWidth, halfHeight);
        Vector2 topRight = new Vector2(halfWidth, halfHeight);
        Vector2 bottomLeft = new Vector2(-halfWidth, -halfHeight);
        Vector2 bottomRight = new Vector2(halfWidth, -halfHeight);
        AddNode("TopLeft", WarrantNodeType.Socket, topLeft);
        AddNode("TopRight", WarrantNodeType.Socket, topRight);
        AddNode("BottomLeft", WarrantNodeType.Socket, bottomLeft);
        AddNode("BottomRight", WarrantNodeType.Socket, bottomRight);

        // Midpoints
        Vector2 midTop = new Vector2(0f, halfHeight);
        Vector2 midBottom = new Vector2(0f, -halfHeight);
        Vector2 midLeft = new Vector2(-halfWidth, 0f);
        Vector2 midRight = new Vector2(halfWidth, 0f);
        AddNode("MidTop", WarrantNodeType.Socket, midTop);
        AddNode("MidBottom", WarrantNodeType.Socket, midBottom);
        AddNode("MidLeft", WarrantNodeType.Socket, midLeft);
        AddNode("MidRight", WarrantNodeType.Socket, midRight);

        // Edge sockets (between corners and midpoints)
        AddNode("TopLeft_MidTop_Socket", WarrantNodeType.Socket, Vector2.Lerp(topLeft, midTop, 0.5f));
        AddNode("MidTop_TopRight_Socket", WarrantNodeType.Socket, Vector2.Lerp(midTop, topRight, 0.5f));
        AddNode("BottomLeft_MidBottom_Socket", WarrantNodeType.Socket, Vector2.Lerp(bottomLeft, midBottom, 0.5f));
        AddNode("MidBottom_BottomRight_Socket", WarrantNodeType.Socket, Vector2.Lerp(midBottom, bottomRight, 0.5f));
        AddNode("TopLeft_MidLeft_Socket", WarrantNodeType.Socket, Vector2.Lerp(topLeft, midLeft, 0.5f));
        AddNode("MidLeft_BottomLeft_Socket", WarrantNodeType.Socket, Vector2.Lerp(midLeft, bottomLeft, 0.5f));
        AddNode("TopRight_MidRight_Socket", WarrantNodeType.Socket, Vector2.Lerp(topRight, midRight, 0.5f));
        AddNode("MidRight_BottomRight_Socket", WarrantNodeType.Socket, Vector2.Lerp(midRight, bottomRight, 0.5f));

        // Center hub
        AddNode("Center", WarrantNodeType.Socket, Vector2.zero);

        // Connect anchor to three bottom points with two ()ooo()ooo sections
        CreateSectionedPathWithEffects("Anchor", "BottomLeft", 2, nodesPerEdge, "Anchor_BottomLeft");
        CreateSectionedPathWithEffects("Anchor", "MidBottom", 2, nodesPerEdge, "Anchor_MidBottom");
        CreateSectionedPathWithEffects("Anchor", "BottomRight", 2, nodesPerEdge, "Anchor_BottomRight");

        // Rectangle frame edges (with effect nodes + intermediate sockets)
        CreateEdgeChainWithEffects(new[]
        {
            "TopLeft",
            "TopLeft_MidTop_Socket",
            "MidTop",
            "MidTop_TopRight_Socket",
            "TopRight"
        }, nodesPerEdge);

        CreateEdgeChainWithEffects(new[]
        {
            "BottomLeft",
            "BottomLeft_MidBottom_Socket",
            "MidBottom",
            "MidBottom_BottomRight_Socket",
            "BottomRight"
        }, nodesPerEdge);

        CreateEdgeChainWithEffects(new[]
        {
            "TopLeft",
            "TopLeft_MidLeft_Socket",
            "MidLeft",
            "MidLeft_BottomLeft_Socket",
            "BottomLeft"
        }, nodesPerEdge);

        CreateEdgeChainWithEffects(new[]
        {
            "TopRight",
            "TopRight_MidRight_Socket",
            "MidRight",
            "MidRight_BottomRight_Socket",
            "BottomRight"
        }, nodesPerEdge);

        // Center hub connections with ()ooo()ooo diagonals
        var centerTargets = new[]
        {
            "TopLeft",
            "TopRight",
            "MidRight",
            "BottomRight",
            "BottomLeft",
            "MidLeft"
        };

        foreach (var target in centerTargets)
        {
            CreateSectionedPathWithEffects("Center", target, 2, nodesPerEdge, $"Center_{target}");
        }

        float specialBranchHorizontalOffset = width * 0.18f;
        float specialBranchVerticalOffset = height * 0.25f;

        CreateSpecialBranches("MidTop", new[]
        {
            new Vector2(-specialBranchHorizontalOffset, -specialBranchVerticalOffset),
            new Vector2(specialBranchHorizontalOffset, -specialBranchVerticalOffset)
        }, nodesPerEdge, "MidTopSpecial");

        CreateSpecialBranches("MidBottom", new[]
        {
            new Vector2(-specialBranchHorizontalOffset, specialBranchVerticalOffset),
            new Vector2(specialBranchHorizontalOffset, specialBranchVerticalOffset)
        }, nodesPerEdge, "MidBottomSpecial");
    }

    private void CreateEdgeChainWithEffects(IReadOnlyList<string> nodeIds, int effectCount)
    {
        if (nodeIds == null || nodeIds.Count < 2) return;
        for (int i = 0; i < nodeIds.Count - 1; i++)
        {
            CreateEdgeWithEffects(nodeIds[i], nodeIds[i + 1], effectCount);
        }
    }

    private void CreateEdgeWithEffects(string from, string to, int effectCount)
    {
        var fromNode = FindNode(from);
        var toNode = FindNode(to);
        if (fromNode == null || toNode == null) return;

        Vector2 fromPos = fromNode.position;
        Vector2 toPos = toNode.position;
        Vector2 dir = (toPos - fromPos) / (effectCount + 1f);

        string prevId = from;
        for (int i = 1; i <= effectCount; i++)
        {
            string effectId = $"{from}_to_{to}_effect{i}";
            Vector2 pos = fromPos + dir * i;
            AddNode(effectId, WarrantNodeType.Effect, pos);
            AddEdge(prevId, effectId, true);
            prevId = effectId;
        }
        AddEdge(prevId, to, true);
    }

    private void AddNode(string id, WarrantNodeType nodeType, Vector2 position)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        var existing = FindNode(id);
        if (existing != null)
        {
            existing.nodeType = nodeType;
            existing.position = position;
            return;
        }

        nodes.Add(new NodeDefinition
        {
            id = id,
            nodeType = nodeType,
            position = position
        });
    }

    private void AddEdge(string fromId, string toId, bool bidirectional)
    {
        if (string.IsNullOrWhiteSpace(fromId) || string.IsNullOrWhiteSpace(toId))
            return;

        edges.Add(new EdgeDefinition
        {
            fromNodeId = fromId,
            toNodeId = toId,
            bidirectional = bidirectional
        });
    }

    private void CreateSectionedPathWithEffects(string startId, string endId, int sectionCount, int effectsPerSection, string pathName)
    {
        if (sectionCount < 1) sectionCount = 1;
        var startNode = FindNode(startId);
        var endNode = FindNode(endId);
        if (startNode == null || endNode == null)
            return;

        Vector2 startPos = startNode.position;
        Vector2 endPos = endNode.position;

        string previousSocketId = startId;
        for (int sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
        {
            float sectionStartT = sectionIndex / (float)sectionCount;
            float sectionEndT = (sectionIndex + 1f) / sectionCount;
            Vector2 sectionStartPos = Vector2.Lerp(startPos, endPos, sectionStartT);
            Vector2 sectionEndPos = Vector2.Lerp(startPos, endPos, sectionEndT);

            string nextSocketId;
            if (sectionIndex == sectionCount - 1)
            {
                nextSocketId = endId;
            }
            else
            {
                nextSocketId = $"{pathName}_Section{sectionIndex + 1}_Socket";
                AddNode(nextSocketId, WarrantNodeType.Socket, sectionEndPos);
            }

            CreateEdgeWithEffects(previousSocketId, nextSocketId, effectsPerSection);
            previousSocketId = nextSocketId;
        }
    }

    private void CreateSpecialBranches(string midpointId, IReadOnlyList<Vector2> offsets, int effectsPerSection, string branchPrefix)
    {
        if (offsets == null || offsets.Count == 0)
            return;

        var midpoint = FindNode(midpointId);
        if (midpoint == null)
            return;

        for (int i = 0; i < offsets.Count; i++)
        {
            string socketId = $"{branchPrefix}_{i + 1}_SpecialSocket";
            Vector2 position = midpoint.position + offsets[i];
            AddNode(socketId, WarrantNodeType.SpecialSocket, position);
            CreateSectionedPathWithEffects(midpointId, socketId, 1, effectsPerSection, $"{branchPrefix}_{i + 1}");
        }
    }

#if UNITY_EDITOR
    public List<NodeDefinition> EditorNodes => nodes;
    public List<EdgeDefinition> EditorEdges => edges;
#endif

    [Serializable]
    public class NodeDefinition
    {
        public string id;
        public WarrantNodeType nodeType = WarrantNodeType.Effect;
        public Vector2 position;
    }

    [Serializable]
    public class EdgeDefinition
    {
        public string fromNodeId;
        public string toNodeId;
        public bool bidirectional = true;
    }
}

