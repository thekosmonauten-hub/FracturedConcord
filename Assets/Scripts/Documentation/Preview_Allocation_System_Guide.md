# Preview Allocation System Guide

## Overview

The Preview Allocation System allows users to preview their node allocations before committing to them. This provides better user experience by preventing accidental allocations and allowing users to plan their passive tree builds more effectively.

**NEW: The preview system now uses the same validation logic as the regular allocation system, ensuring consistency and proper orthographic allocation rules.**

**ENHANCED: The preview system now supports chaining preview selections, allowing users to preview nodes that are adjacent to other previewed nodes, not just allocated nodes.**

**PROTECTED: The preview system now prevents removing branch nodes that would break tree connectivity, ensuring only leaf nodes can be safely removed.**

## How It Works

### **Preview Allocation Process**

1. **Click a node** - Adds it to the preview allocation (turns yellow) **if it passes validation**
2. **Click again** - Removes it from preview allocation (turns white) **if it's a leaf node**
3. **Click "Allocate"** - Commits all valid previewed nodes (turns green)
4. **Click "Clear Preview"** - Removes all nodes from preview

### **Visual States**

| State | Color | Description |
|-------|-------|-------------|
| **Unallocated** | White | Node is available but not selected |
| **Previewed** | Yellow | Node is selected for allocation |
| **Allocated** | Green | Node has been allocated |

## Features

### **✅ Smart Validation**
- **Point Cost Checking**: Validates if you have enough points for previewed nodes
- **Max Rank Checking**: Prevents previewing nodes that are already at max rank
- **Orthographic Allocation**: Only allows previewing nodes adjacent to allocated nodes
- **Preview Chaining**: Allows previewing nodes adjacent to other previewed nodes
- **Tree Connectivity**: Prevents removing branch nodes that would break tree connectivity
- **Requirement Validation**: Checks node prerequisites before allowing preview
- **Batch Allocation**: Allocates multiple nodes at once for efficiency

### **✅ Visual Feedback**
- **Real-time Updates**: Grid updates immediately when nodes are previewed
- **Cost Calculation**: Shows total cost of all previewed nodes
- **Status Indicators**: Clear visual feedback for allocation status
- **Validation Status**: Shows which nodes are valid/invalid for allocation
- **Node Type Indicators**: Shows leaf nodes vs branch nodes

### **✅ Information Display**
- **Preview Summary**: Shows number of previewed nodes and total cost
- **Validation Count**: Displays how many previewed nodes are valid
- **Point Validation**: Indicates if you have enough points to allocate
- **Node Details**: Lists all previewed nodes with their individual costs, validation status, and node type
- **Validation Issues**: Shows specific reasons why nodes cannot be allocated
- **Tree Connectivity**: Indicates which nodes are safe to remove (leaf nodes) vs branch nodes

## Allocation Rules

### **Orthographic Allocation**
The preview system respects the same orthographic allocation rules as the regular system:

- **Adjacent Nodes Only**: Nodes must be orthogonally adjacent (up, down, left, right) to allocated nodes
- **Preview Chaining**: Nodes can also be adjacent to other previewed nodes
- **No Diagonal Connections**: Diagonal connections are not allowed
- **Tree Integrity**: Maintains the tree structure by requiring adjacency

### **Tree Connectivity**
The preview system maintains tree connectivity by preventing removal of branch nodes:

- **Leaf Nodes**: Nodes that can be safely removed without breaking tree connectivity
- **Branch Nodes**: Nodes that serve as bridges between different parts of the tree
- **Safe Removal**: Only leaf nodes can be removed from preview
- **Protected Removal**: Branch nodes cannot be removed to prevent tree disconnection

### **Validation Checks**
Each node must pass these validation checks to be previewed:

1. **Sufficient Points**: Must have enough points to allocate the node
2. **Not Maxed Out**: Node must not already be at maximum rank
3. **Adjacent to Allocated or Previewed**: Must be orthogonally adjacent to an allocated node OR a previewed node
4. **Requirements Met**: Must have all prerequisite nodes allocated
5. **Tree Integrity**: Must not break the tree structure

## Usage

### **Basic Preview Allocation**

1. **Click on a valid node** to add it to preview (turns yellow)
2. **Click on another valid node** to add more nodes to preview
3. **Click "Allocate Previewed Nodes"** to commit all valid previewed nodes
4. **Nodes turn green** when successfully allocated

### **Preview Chaining**

1. **Select a node adjacent to allocated nodes** (turns yellow)
2. **Select nodes adjacent to the previewed node** (also turn yellow)
3. **Continue chaining** to build complex preview paths
4. **Allocate all at once** when ready

### **Safe Node Removal**

1. **Leaf Nodes**: Click on a yellow leaf node to remove it from preview
2. **Branch Nodes**: Cannot be removed (system prevents it)
3. **Tree Integrity**: System maintains connected tree structure
4. **Visual Feedback**: Info panel shows which nodes are leaf vs branch

### **Managing Preview**

- **Remove Leaf Nodes**: Click on a yellow leaf node to remove it
- **Protected Branch Nodes**: Branch nodes cannot be removed (prevents tree breakage)
- **Clear All Preview**: Use "Clear Preview" button to remove all previewed nodes
- **Batch Operations**: Preview multiple nodes before allocating them all at once
- **Validation Feedback**: Invalid nodes are automatically removed from preview

### **Validation**

The system automatically validates:
- **Available Points**: Ensures you have enough points for all previewed nodes
- **Node Limits**: Prevents previewing nodes that are already at maximum rank
- **Orthographic Rules**: Ensures nodes are adjacent to allocated or previewed nodes
- **Tree Connectivity**: Prevents removing nodes that would break tree structure
- **Cost Calculation**: Shows total cost and compares with available points
- **Requirement Checking**: Validates all node prerequisites

## UI Integration

### **UI Setup Guide**

To test the preview allocation system, set up these buttons in Unity:

#### **Required Buttons**
1. **Allocate Node Button** - Click to preview/allocate selected node
2. **Allocate Previewed Button** - Allocate all previewed nodes at once
3. **Clear Preview Button** - Remove all nodes from preview
4. **Add Points Button** - Add 10 test points for testing

#### **Button Configuration**
1. **Select PassiveTreeTestController** in the scene
2. **In Inspector**, find the "Preview Allocation UI" section
3. **Drag and drop** your UI buttons to the corresponding fields:
   - `_allocatePreviewedButton` → "Allocate Previewed" button
   - `_clearPreviewButton` → "Clear Preview" button  
   - `_addPointsButton` → "Add Points" button

#### **Testing Workflow**
1. **Add Points**: Click "Add Points" to get test points
2. **Preview Nodes**: Click nodes to add them to preview (yellow)
3. **Chain Preview**: Click adjacent nodes to build preview paths
4. **Allocate All**: Click "Allocate Previewed" to commit all previewed nodes
5. **Clear Preview**: Click "Clear Preview" to reset selection

### **Info Panel Display**

When nodes are previewed, the info panel shows:

```
Preview Allocation:
Previewed Nodes: 3
Valid Nodes: 3/3
Total Preview Cost: 6
✅ Can allocate all previewed nodes

Previewed Nodes:
  • Strength Node (Cost: 2) ✅ - Leaf Node (safe to remove)
  • Dexterity Node (Cost: 2) ✅ - Branch Node (cannot remove - would break tree)
  • Intelligence Node (Cost: 2) ✅ - Leaf Node (safe to remove)

Validation Issues:
  • (None - all nodes are valid)
```

### **Status Indicators**

- **✅ Can allocate all previewed nodes** - All nodes are valid and you have enough points
- **⚠️ Not enough points! Need X, have Y** - You need more points
- **⚠️ Some nodes cannot be allocated** - Some previewed nodes fail validation
- **🟡 This node is previewed for allocation** - Node is in preview state
- **🍃 Leaf Node - Click again to remove from preview** - Safe to remove
- **🌿 Branch Node - Cannot remove (would break tree connectivity)** - Protected from removal

## Technical Implementation

### **Core Components**

#### **PassiveTreeTestController**
- **`_previewedNodes`**: List of nodes currently in preview
- **`HandlePreviewAllocation()`**: Manages adding/removing nodes from preview with validation
- **`AllocatePreviewedNodes()`**: Commits all valid previewed nodes
- **`ClearPreviewedNodes()`**: Removes all nodes from preview
- **`WouldBreakTreeConnectivity()`**: Checks if removing a node would break tree connectivity
- **`IsTreeConnected()`**: Validates tree connectivity using BFS
- **`IsLeafNode()`**: Determines if a node is a leaf node (safe to remove)

#### **GridManager**
- **`GetNodeColor()`**: Determines node color based on state
- **`IsNodePreviewed()`**: Checks if a node is in preview state
- **`UpdateNodeVisuals()`**: Updates all node colors

#### **PassiveNode**
- **`CanAllocate()`**: Validates if a node can be allocated
- **`IsAdjacentToAllocatedNode()`**: Checks orthographic adjacency
- **Validation Logic**: Ensures tree integrity and allocation rules

### **State Management**

```csharp
// Preview state tracking with validation
private List<PassiveNode> _previewedNodes = new List<PassiveNode>();

// Enhanced validation using allocated AND previewed nodes
var validAdjacentNodes = new List<string>(allocatedNodeIds);
foreach (var previewedNode in _previewedNodes)
{
    if (!validAdjacentNodes.Contains(previewedNode.id))
    {
        validAdjacentNodes.Add(previewedNode.id);
    }
}

// Tree connectivity validation
if (_previewedNodes.Contains(node))
{
    if (WouldBreakTreeConnectivity(node))
    {
        // Cannot remove - branch node
        Debug.Log("Cannot remove - would break tree connectivity");
    }
    else
    {
        // Safe to remove - leaf node
        _previewedNodes.Remove(node);
    }
}

// Color determination
private Color GetNodeColor(PassiveNode node) {
    if (node.currentRank > 0) return _allocatedNodeColor;     // Green
    if (IsNodePreviewed(node)) return _selectedNodeColor;     // Yellow
    return _nodeColor;                                        // White
}
```

## Testing and Debugging

### **Context Menu Options**

Right-click on `PassiveTreeTestController` for testing:

- **"Test Preview Allocation"** - Shows current preview state
- **"Clear All Previewed Nodes"** - Removes all previewed nodes
- **"Allocate All Previewed Nodes"** - Commits all previewed nodes

### **Console Logging**

The system provides detailed logging:

```
[PassiveTreeTestController] Added Strength Node to preview allocation (adjacent to allocated nodes)
[PassiveTreeTestController] Added Dexterity Node to preview allocation (adjacent to previewed nodes)
[PassiveTreeTestController] Cannot remove Dexterity Node from preview - would break tree connectivity (branch node)
[PassiveTreeTestController] Removed Intelligence Node from preview allocation (leaf node)
[PassiveTreeTestController] Allocating 2 previewed nodes...
[PassiveTreeTestController] Successfully allocated 2 of 2 valid nodes
```

## Best Practices

### **User Experience**

1. **Preview Before Allocating**: Always preview nodes before committing
2. **Use Preview Chaining**: Build complex paths by chaining from previewed nodes
3. **Respect Tree Connectivity**: Only remove leaf nodes to maintain tree integrity
4. **Check Validation**: Verify that previewed nodes are valid before allocating
5. **Batch Operations**: Preview multiple nodes for efficient allocation
6. **Clear Preview**: Use clear preview to reset selection

### **Development**

1. **Test Preview States**: Verify all visual states work correctly
2. **Validate Point Logic**: Ensure point calculations are accurate
3. **Test Orthographic Rules**: Verify adjacency validation works correctly
4. **Test Preview Chaining**: Verify nodes can be chained from previewed nodes
5. **Test Tree Connectivity**: Verify branch nodes cannot be removed
6. **Test Edge Cases**: Test with insufficient points and max rank nodes
7. **Performance**: Preview system should be responsive

## Future Enhancements

### **Planned Features**

- **Preview Undo**: Undo last preview action
- **Preview History**: Track preview changes
- **Smart Suggestions**: Suggest optimal node combinations
- **Preview Templates**: Save and load preview configurations
- **Visual Connections**: Show preview connections between nodes

### **Advanced Features**

- **Dependency Checking**: Validate node prerequisites
- **Path Optimization**: Suggest optimal allocation paths
- **Build Sharing**: Share preview configurations
- **Simulation Mode**: Test builds without committing

## Troubleshooting

### **Common Issues**

#### **"Node not turning yellow when clicked"**
- **Cause**: Node may be at max rank, insufficient points, or not adjacent to allocated/previewed nodes
- **Solution**: Check node rank, available points, and adjacency

#### **"Preview not clearing"**
- **Cause**: Grid not updating properly
- **Solution**: Use "Clear All Previewed Nodes" context menu option

#### **"Can't allocate previewed nodes"**
- **Cause**: Some nodes fail validation or insufficient points
- **Solution**: Check validation issues in info panel and remove invalid nodes

#### **"Nodes not adjacent to allocated nodes"**
- **Cause**: Orthographic allocation rules require adjacency
- **Solution**: Allocate nodes in a connected path from existing allocated nodes

#### **"Can't chain preview selections"**
- **Cause**: Preview chaining may not be working correctly
- **Solution**: Ensure nodes are orthogonally adjacent to other previewed nodes

#### **"Cannot remove node from preview"**
- **Cause**: Node is a branch node that would break tree connectivity
- **Solution**: Only remove leaf nodes, or remove nodes in the correct order

### **Debug Steps**

1. **Check Console Logs** for preview allocation messages
2. **Use "Test Preview Allocation"** to verify preview state
3. **Verify Point Count** in info panel
4. **Check Validation Issues** in info panel
5. **Test Individual Nodes** to isolate issues
6. **Test Preview Chaining** by selecting adjacent nodes
7. **Test Tree Connectivity** by trying to remove branch nodes

---

*For more information about the Passive Tree system, see `Core_Board_Initialization_Guide.md`*
