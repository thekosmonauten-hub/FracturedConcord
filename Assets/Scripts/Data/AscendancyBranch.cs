using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Direction change point in a branch
/// </summary>
[System.Serializable]
public class BranchDirectionChange
{
    [Tooltip("Node index where direction changes (0 = first node after start)")]
    public int atNodeIndex = 2;
    
    [Tooltip("New angle from this point onward (0 = right, 90 = up, 180 = left, 270 = down)")]
    public float newAngle = 0f;
    
    [Tooltip("Spacing for nodes after this point (optional, 0 = use default)")]
    public float newSpacing = 0f;
}

/// <summary>
/// Cross-branch connection (connects a node to a node in another branch)
/// </summary>
[System.Serializable]
public class CrossBranchConnection
{
    [Tooltip("Node index in THIS branch")]
    public int fromNodeIndex = 2;
    
    [Tooltip("Name of node in OTHER branch to connect to")]
    public string toNodeName = "";
    
    [Tooltip("Connection type: Prerequisite (must unlock other first) or Optional (either works)")]
    public bool isPrerequisite = false;
}

/// <summary>
/// Represents a single branch in an Ascendancy tree.
/// Contains nodes in sequential order: Minor -> Major -> Minor -> Major
/// Supports direction changes for curved/bent branches.
/// </summary>
[System.Serializable]
public class AscendancyBranch
{
    [Header("Branch Info")]
    [Tooltip("Branch identifier (e.g., 'Left', 'Right', 'Center')")]
    public string branchName = "";
    
    [Tooltip("Theme/description of this branch's playstyle")]
    public string branchTheme = "";
    
    [Header("Branch Nodes (In Sequential Order)")]
    [Tooltip("Nodes in this branch from first to last. System auto-connects them.")]
    public List<AscendancyPassive> branchNodes = new List<AscendancyPassive>();
    
    [Header("Branch Layout")]
    [Tooltip("Name of the node this branch starts from (empty = start node). Allows branches to start from any node, enabling split nodes.")]
    public string sourceNodeName = "";
    
    [Tooltip("Starting angle from source node (0 = right, 90 = up, 180 = left, 270 = down)")]
    public float branchAngle = 0f;
    
    [Tooltip("Horizontal offset from center for this branch (legacy, use branchAngle instead)")]
    public float horizontalOffset = 150f;
    
    [Header("Advanced: Direction Changes")]
    [Tooltip("Define points where the branch changes direction (for curved/bent paths)")]
    public List<BranchDirectionChange> directionChanges = new List<BranchDirectionChange>();
    
    [Header("Advanced: Cross-Branch Connections")]
    [Tooltip("Define connections to nodes in other branches (for intertwined paths)")]
    public List<CrossBranchConnection> crossBranchConnections = new List<CrossBranchConnection>();
    
    /// <summary>
    /// Get the number of nodes in this branch
    /// </summary>
    public int GetNodeCount()
    {
        return branchNodes != null ? branchNodes.Count : 0;
    }
    
    /// <summary>
    /// Get node at index
    /// </summary>
    public AscendancyPassive GetNode(int index)
    {
        if (branchNodes == null || index < 0 || index >= branchNodes.Count)
            return null;
        
        return branchNodes[index];
    }
    
    /// <summary>
    /// Auto-generate prerequisites and positions for nodes in this branch
    /// </summary>
    public void GenerateBranchStructure(string startNodeName, Vector2 startNodePosition = default, AscendancyData ascendancyData = null)
    {
        if (branchNodes == null || branchNodes.Count == 0) return;
        
        if (crossBranchConnections != null && crossBranchConnections.Count > 0)
        {
            Debug.LogWarning($"[AscendancyBranch] Cross-branch connections detected on '{branchName}'. Consider migrating to floating nodes.");
        }

        // Determine source node (the node this branch starts from)
        string actualSourceNodeName = string.IsNullOrEmpty(sourceNodeName) ? startNodeName : sourceNodeName;
        Vector2 sourceNodePosition = startNodePosition;
        
        // If source is not the start node, find its position
        if (!string.IsNullOrEmpty(sourceNodeName) && ascendancyData != null)
        {
            AscendancyPassive sourceNode = ascendancyData.FindPassiveByName(actualSourceNodeName);
            if (sourceNode != null)
            {
                sourceNodePosition = sourceNode.treePosition;
                
                // Validate that source node position is set (not zero or uninitialized)
                // Note: Vector2.zero is valid for start node, but for other nodes it indicates uninitialized
                if (sourceNodePosition == Vector2.zero && actualSourceNodeName != startNodeName)
                {
                    Debug.LogWarning($"[AscendancyBranch] Source node '{actualSourceNodeName}' has zero position for branch '{branchName}'. This may cause overlapping nodes. Ensure source node is generated before this branch. Falling back to start node position.");
                    sourceNodePosition = startNodePosition;
                    actualSourceNodeName = startNodeName;
                }
            }
            else
            {
                Debug.LogWarning($"[AscendancyBranch] Source node '{actualSourceNodeName}' not found for branch '{branchName}'. Using start node position.");
                actualSourceNodeName = startNodeName;
                sourceNodePosition = startNodePosition;
            }
        }

        // Track current position and direction
        // Start from source node position - first node will be spaced from source
        float currentAngle = branchAngle;
        float baseSpacing = 80f; // Default spacing between nodes
        
        for (int i = 0; i < branchNodes.Count; i++)
        {
            AscendancyPassive node = branchNodes[i];
            if (node == null) continue;
            
            // Determine the position to start from (previous node or source)
            Vector2 startPosition;
            if (i == 0)
            {
                // First node: start from source node position
                startPosition = sourceNodePosition;
            }
            else
            {
                // Subsequent nodes: start from the ACTUAL position of the previous node
                // This ensures we're always spacing from the real previous node position
                AscendancyPassive previousNode = branchNodes[i - 1];
                if (previousNode != null)
                {
                    startPosition = previousNode.treePosition;
                }
                else
                {
                    // Fallback to source if previous node is null (shouldn't happen)
                    startPosition = sourceNodePosition;
                    Debug.LogWarning($"[AscendancyBranch] Previous node is null at index {i}, using source position");
                }
            }
            
            // Check for direction changes at this node
            BranchDirectionChange directionChange = GetDirectionChangeAtIndex(i);
            if (directionChange != null)
            {
                currentAngle = directionChange.newAngle;
                if (directionChange.newSpacing > 0)
                    baseSpacing = directionChange.newSpacing;
                
                Debug.Log($"[AscendancyBranch] Direction change at node {i}: new angle = {currentAngle}°");
            }
            
            // Calculate direction vector from current angle
            float angleRad = currentAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            
            // Calculate position: space from start position (previous node or source)
            // This ensures consistent spacing between all nodes in the branch
            Vector2 offset = direction * baseSpacing;
            Vector2 newNodePosition = startPosition + offset;
            node.treePosition = newNodePosition;
            
            // Set prerequisite (previous node or source)
            node.prerequisitePassives.Clear();
            if (i == 0)
            {
                // First node connects to source node
                node.prerequisitePassives.Add(actualSourceNodeName);
            }
            else
            {
                // Connect to previous node in branch
                node.prerequisitePassives.Add(branchNodes[i - 1].name);
            }

            node.requireAllPrerequisites = true;
            
            // Add cross-branch connections if defined
            AddCrossBranchPrerequisites(node, i);
            
            Debug.Log($"[AscendancyBranch] {node.name} positioned at {node.treePosition} (branch: {branchName}, source: {actualSourceNodeName} at {sourceNodePosition}, angle: {currentAngle}°, spacing: {baseSpacing}, index: {i})");
        }
    }
    
    /// <summary>
    /// Get direction change at a specific node index
    /// </summary>
    BranchDirectionChange GetDirectionChangeAtIndex(int index)
    {
        if (directionChanges == null || directionChanges.Count == 0)
            return null;
        
        foreach (var change in directionChanges)
        {
            if (change.atNodeIndex == index)
                return change;
        }
        
        return null;
    }
    
    /// <summary>
    /// Add cross-branch prerequisites to a node
    /// </summary>
    void AddCrossBranchPrerequisites(AscendancyPassive node, int nodeIndex)
    {
        if (crossBranchConnections == null || crossBranchConnections.Count == 0)
            return;
        
        foreach (var connection in crossBranchConnections)
        {
            if (connection.fromNodeIndex == nodeIndex && !string.IsNullOrEmpty(connection.toNodeName))
            {
                // Add as prerequisite (requires other branch's node first)
                if (connection.isPrerequisite && !node.prerequisitePassives.Contains(connection.toNodeName))
                {
                    node.prerequisitePassives.Add(connection.toNodeName);
                    Debug.Log($"[AscendancyBranch] Added cross-branch prerequisite: {node.name} requires {connection.toNodeName}");
                }
                // Note: Connection line will be drawn automatically by prerequisite system
            }
        }
    }
}

