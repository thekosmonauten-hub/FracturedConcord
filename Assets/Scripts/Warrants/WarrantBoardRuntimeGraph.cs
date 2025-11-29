using System.Collections.Generic;
using UnityEngine;

public class WarrantBoardRuntimeGraph
{
    public IReadOnlyDictionary<string, Node> Nodes => nodes;
    public IReadOnlyList<Row> Rows => rows;

    private readonly Dictionary<string, Node> nodes = new Dictionary<string, Node>();
    private readonly List<Row> rows = new List<Row>();

    public void Clear()
    {
        nodes.Clear();
        rows.Clear();
    }

    public Row AddRow(WarrantRowType rowType)
    {
        var row = new Row(rowType);
        rows.Add(row);
        return row;
    }

    public Node AddNode(Row row, string nodeId, WarrantNodeType type, Vector2 position)
    {
        var node = new Node(nodeId, type, position);
        nodes[nodeId] = node;
        if (row != null)
        {
            row.Nodes.Add(node);
        }
        return node;
    }

    public class Row
    {
        public WarrantRowType RowType { get; }
        public List<Node> Nodes { get; } = new List<Node>();

        public Row(WarrantRowType type)
        {
            RowType = type;
        }
    }

    public class Node
    {
        public string Id { get; }
        public WarrantNodeType NodeType { get; }
        public Vector2 Position { get; }
        public List<Node> Connections { get; } = new List<Node>();

        public Node(string id, WarrantNodeType nodeType, Vector2 position)
        {
            Id = id;
            NodeType = nodeType;
            Position = position;
        }

        public override string ToString() => $"{Id} ({NodeType})";
    }
}

