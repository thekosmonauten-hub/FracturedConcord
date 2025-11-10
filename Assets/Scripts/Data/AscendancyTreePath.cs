using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper class to define branching paths in an Ascendancy tree.
/// Creates the Start -> Minor -> Major -> Minor -> Major structure.
/// </summary>
[System.Serializable]
public class AscendancyTreePath
{
    [Tooltip("Name of this path (e.g., 'Left Branch', 'Right Branch')")]
    public string pathName = "";
    
    [Tooltip("Nodes in this path, in order from start to end")]
    public List<string> nodeSequence = new List<string>();
    
    /// <summary>
    /// Get the number of nodes in this path
    /// </summary>
    public int GetNodeCount()
    {
        return nodeSequence != null ? nodeSequence.Count : 0;
    }
    
    /// <summary>
    /// Check if a node is in this path
    /// </summary>
    public bool ContainsNode(string nodeName)
    {
        return nodeSequence != null && nodeSequence.Contains(nodeName);
    }
    
    /// <summary>
    /// Get the index of a node in this path
    /// </summary>
    public int GetNodeIndex(string nodeName)
    {
        return nodeSequence != null ? nodeSequence.IndexOf(nodeName) : -1;
    }
}

/// <summary>
/// Helper to generate standard Ascendancy tree structures
/// </summary>
public static class AscendancyTreeBuilder
{
    /// <summary>
    /// Create a standard 2-branch tree with Minor/Major pattern
    /// Pattern: Start -> (Minor -> Major -> Minor -> Major) x2 branches
    /// </summary>
    public static List<AscendancyPassive> CreateStandardTree(
        string startName,
        string[] leftMinorNames, string[] leftMajorNames,
        string[] rightMinorNames, string[] rightMajorNames)
    {
        List<AscendancyPassive> nodes = new List<AscendancyPassive>();
        
        // Start node
        nodes.Add(new AscendancyPassive
        {
            name = startName,
            nodeType = AscendancyNodeType.Start,
            unlockedByDefault = true,
            pointCost = 0,
            treePosition = Vector2.zero
        });
        
        // Left branch
        if (leftMinorNames != null && leftMajorNames != null)
        {
            for (int i = 0; i < Mathf.Max(leftMinorNames.Length, leftMajorNames.Length); i++)
            {
                // Minor node
                if (i < leftMinorNames.Length)
                {
                    var minor = new AscendancyPassive
                    {
                        name = leftMinorNames[i],
                        nodeType = AscendancyNodeType.Minor,
                        pointCost = 1,
                        treePosition = new Vector2(-100, -100 - (i * 100))
                    };
                    
                    // Set prerequisite
                    if (i == 0)
                        minor.prerequisitePassives.Add(startName);
                    else
                        minor.prerequisitePassives.Add(leftMajorNames[i - 1]);
                    
                    nodes.Add(minor);
                }
                
                // Major node
                if (i < leftMajorNames.Length)
                {
                    var major = new AscendancyPassive
                    {
                        name = leftMajorNames[i],
                        nodeType = AscendancyNodeType.Major,
                        pointCost = 1,
                        nodeScale = 1.3f,
                        treePosition = new Vector2(-100, -150 - (i * 100))
                    };
                    
                    // Set prerequisite
                    if (i < leftMinorNames.Length)
                        major.prerequisitePassives.Add(leftMinorNames[i]);
                    
                    nodes.Add(major);
                }
            }
        }
        
        // Right branch (similar pattern)
        if (rightMinorNames != null && rightMajorNames != null)
        {
            for (int i = 0; i < Mathf.Max(rightMinorNames.Length, rightMajorNames.Length); i++)
            {
                // Minor node
                if (i < rightMinorNames.Length)
                {
                    var minor = new AscendancyPassive
                    {
                        name = rightMinorNames[i],
                        nodeType = AscendancyNodeType.Minor,
                        pointCost = 1,
                        treePosition = new Vector2(100, -100 - (i * 100))
                    };
                    
                    // Set prerequisite
                    if (i == 0)
                        minor.prerequisitePassives.Add(startName);
                    else
                        minor.prerequisitePassives.Add(rightMajorNames[i - 1]);
                    
                    nodes.Add(minor);
                }
                
                // Major node
                if (i < rightMajorNames.Length)
                {
                    var major = new AscendancyPassive
                    {
                        name = rightMajorNames[i],
                        nodeType = AscendancyNodeType.Major,
                        pointCost = 1,
                        nodeScale = 1.3f,
                        treePosition = new Vector2(100, -150 - (i * 100))
                    };
                    
                    // Set prerequisite
                    if (i < rightMinorNames.Length)
                        major.prerequisitePassives.Add(rightMinorNames[i]);
                    
                    nodes.Add(major);
                }
            }
        }
        
        return nodes;
    }
}


