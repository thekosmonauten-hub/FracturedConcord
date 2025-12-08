# Ascendancy Node Groups Guide

## Overview

Node groups allow you to create **mutually exclusive** choices in your Ascendancy tree. When nodes are in the same group, only **one** node from that group can be unlocked at a time.

This is perfect for creating branching choices where players must pick one path over another.

## How to Use

### Step 1: Set the Group ID

In your `AscendancyPassive` node, set the `nodeGroup` field to a unique group identifier:

```csharp
// Example: Three nodes in the same group
node1.nodeGroup = "SpecializationChoice";
node2.nodeGroup = "SpecializationChoice";
node3.nodeGroup = "SpecializationChoice";
```

### Step 2: How It Works

- **Empty group ID** (`""`): Node is not in any group, can be unlocked freely
- **Same group ID**: Nodes with the same group ID are mutually exclusive
- **Different group IDs**: Nodes in different groups don't affect each other

### Example: Specialization Choice

You want players to choose between three specializations:

```
[Node A] - nodeGroup = "Specialization"
[Node B] - nodeGroup = "Specialization"  
[Node C] - nodeGroup = "Specialization"
```

**Result:**
- Player can unlock Node A, B, or C
- Once one is unlocked, the others become unavailable
- Player can only have ONE specialization active

### Example: Multiple Independent Groups

You can have multiple groups that don't interfere with each other:

```
[Node A] - nodeGroup = "OffenseChoice"
[Node B] - nodeGroup = "OffenseChoice"
[Node C] - nodeGroup = "DefenseChoice"
[Node D] - nodeGroup = "DefenseChoice"
```

**Result:**
- Player can unlock ONE from "OffenseChoice" (A or B)
- Player can unlock ONE from "DefenseChoice" (C or D)
- These choices are independent - unlocking A doesn't prevent unlocking C

## Validation

The system automatically:
1. **Prevents unlocking** if another node in the same group is already unlocked
2. **Shows appropriate warnings** in the console
3. **Makes nodes unavailable** in the UI if a group conflict exists

## UI Behavior

When a node is in a group:
- If another node in the group is unlocked, this node becomes **unavailable** (grayed out)
- The tooltip/UI should indicate why the node is unavailable
- Players must unlock nodes in the group one at a time

## Best Practices

1. **Use descriptive group IDs**: `"OffenseSpecialization"` is better than `"Group1"`
2. **Group related choices**: Only put nodes in the same group if they're truly mutually exclusive
3. **Consider prerequisites**: Group nodes can still have prerequisites - they just can't be unlocked if another group member is already unlocked
4. **Visual indication**: Consider adding visual indicators (colors, borders) to show which nodes are in groups

## Technical Details

- Group checking happens in:
  - `CharacterAscendancyProgress.UnlockPassive()` - Prevents unlocking
  - `AscendancyTreeDisplay.CanUnlockPassive()` - Makes nodes unavailable in UI
- Helper methods:
  - `AscendancyData.GetNodesInGroup(groupId)` - Get all nodes in a group
  - `AscendancyData.IsAnyNodeInGroupUnlocked(groupId, progression)` - Check if any group member is unlocked

## Example Use Cases

1. **Class Specializations**: Choose between "Melee Focus", "Ranged Focus", or "Spell Focus"
2. **Elemental Affinities**: Choose Fire, Cold, or Lightning specialization
3. **Playstyle Choices**: Choose between "Aggressive", "Defensive", or "Balanced"
4. **Resource Management**: Choose between different resource generation methods











