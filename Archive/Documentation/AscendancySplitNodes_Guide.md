# Ascendancy Split Nodes Guide

## Overview

The Ascendancy system now supports **split nodes** - nodes that can spawn multiple branches. This allows you to create complex tree structures like:

```
Start → 3 nodes (3 branches) → Merge to 1 node → Split node with 3 paths
```

## Key Features

### 1. Branches Starting from Any Node

Branches can now start from any node, not just the start node. Use the `sourceNodeName` field in `AscendancyBranch`:

```csharp
// Branch starting from start node (default behavior)
branch.sourceNodeName = ""; // or leave empty

// Branch starting from a specific node
branch.sourceNodeName = "Merge Node Name";
```

### 2. Split Nodes

Split nodes are nodes that spawn multiple branches. Define them in `AscendancyData.splitNodes`:

```csharp
[System.Serializable]
public class AscendancySplitNode
{
    public string nodeName = "";           // Name of the node that splits
    public List<string> branchNames;     // Branches that spawn from it
    public List<float> branchAngles;      // Optional: angles for each branch
}
```

## How to Create Your Desired Structure

### Example: Start → 3 nodes → Merge → Split (3 paths)

#### Step 1: Create 3 Initial Branches from Start

```csharp
// Branch 1
branch1.branchName = "Left Path";
branch1.sourceNodeName = ""; // Empty = starts from start node
branch1.branchAngle = 150f; // Up-left
branch1.branchNodes = [node1, node2, mergeNode];

// Branch 2
branch2.branchName = "Center Path";
branch2.sourceNodeName = ""; // Empty = starts from start node
branch2.branchAngle = 90f; // Up
branch2.branchNodes = [node3, node4, mergeNode];

// Branch 3
branch3.branchName = "Right Path";
branch3.sourceNodeName = ""; // Empty = starts from start node
branch3.branchAngle = 30f; // Up-right
branch3.branchNodes = [node5, node6, mergeNode];
```

**Important:** All three branches should have `mergeNode` as their last node to create the merge effect.

#### Step 2: Create the Split Node Configuration

```csharp
AscendancySplitNode splitNode = new AscendancySplitNode
{
    nodeName = "Merge Node Name", // The node that splits
    branchNames = new List<string> 
    { 
        "Split Branch 1", 
        "Split Branch 2", 
        "Split Branch 3" 
    },
    branchAngles = new List<float> 
    { 
        120f,  // Up-left
        90f,   // Up
        60f    // Up-right
    }
};
```

#### Step 3: Create the Split Branches

```csharp
// Split Branch 1
splitBranch1.branchName = "Split Branch 1";
splitBranch1.sourceNodeName = "Merge Node Name"; // Must match splitNode.nodeName
splitBranch1.branchAngle = 120f;
splitBranch1.branchNodes = [splitNode1, splitNode2];

// Split Branch 2
splitBranch2.branchName = "Split Branch 2";
splitBranch2.sourceNodeName = "Merge Node Name"; // Must match splitNode.nodeName
splitBranch2.branchAngle = 90f;
splitBranch2.branchNodes = [splitNode3, splitNode4];

// Split Branch 3
splitBranch3.branchName = "Split Branch 3";
splitBranch3.sourceNodeName = "Merge Node Name"; // Must match splitNode.nodeName
splitBranch3.branchAngle = 60f;
splitBranch3.branchNodes = [splitNode5, splitNode6];
```

#### Step 4: Add to AscendancyData

```csharp
ascendancyData.branches = [branch1, branch2, branch3, splitBranch1, splitBranch2, splitBranch3];
ascendancyData.splitNodes = [splitNode];
```

## Angle Guide

Use the angle guide for positioning:

```
         ↑     90° (Up)
180° ←───●───→ 0° (Right)         
         ↓     270° (Down)
```

Common angles:
- `0°` = Right
- `60°` = Up-right (diagonal)
- `90°` = Up
- `120°` = Up-left (diagonal)
- `150°` = Up-left (diagonal)
- `180°` = Left
- `210°` = Down-left (diagonal)
- `270°` = Down
- `330°` = Down-right (diagonal)

## Auto-Spacing

If you don't provide `branchAngles` in the split node, the system will automatically space branches evenly:

- 2 branches: 0°, 180°
- 3 branches: 0°, 120°, 240°
- 4 branches: 0°, 90°, 180°, 270°

## Important Notes

1. **Node Names Must Match:** The `sourceNodeName` in branches must exactly match the `nodeName` in the split node configuration.

2. **Generation Order:** The system generates branches in two phases:
   - Phase 1: Branches from start node (or with empty `sourceNodeName`)
   - Phase 2: Branches from split nodes

3. **Merge Nodes:** To create a merge effect, simply have multiple branches end at the same node. The prerequisite system will handle the connections.

4. **Split Nodes:** Split nodes must exist in the tree before their branches are generated. Make sure the split node is part of a branch that's generated in Phase 1.

## Example Structure Visualization

```
        [Node 1]
           |
        [Node 2]
           |
    [Merge Node] ←─── [Node 3] ←─── [Node 4]
           |              |
        [Node 5]       [Node 6]
           |
    [Split Node]
      /    |    \
[Path1] [Path2] [Path3]
```

This structure requires:
- 3 branches from start → merge node
- 1 split node configuration
- 3 branches from split node

