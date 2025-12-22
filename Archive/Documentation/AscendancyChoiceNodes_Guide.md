# Ascendancy Choice Nodes Guide

## Overview

Choice nodes are special nodes that display **sub-nodes in a circle around them**. When you click a sub-node, its sprite is applied to the main node, representing your selection. This creates a visual choice system where players can pick from multiple options.

## How It Works

1. **Main Node**: The choice node itself (unlocked normally)
2. **Sub-Nodes**: Options that circle around the main node
3. **Selection**: Clicking a sub-node applies its sprite to the main node
4. **Respecc**: When the main node is refunded, it returns to showing sub-nodes

## Setup

### Step 1: Mark Node as Choice Node

In your `AscendancyPassive`:
```csharp
passive.isChoiceNode = true;
passive.subNodeRadius = 100f; // Distance of sub-nodes from main node
```

### Step 2: Add Sub-Nodes

```csharp
// Create sub-nodes
AscendancySubNode subNode1 = new AscendancySubNode
{
    name = "Fire Specialization",
    icon = fireIconSprite,
    description = "Gain +20% Fire damage",
    angleOffset = 0f // 0° = right, 90° = up, etc.
};

AscendancySubNode subNode2 = new AscendancySubNode
{
    name = "Cold Specialization",
    icon = coldIconSprite,
    description = "Gain +20% Cold damage",
    angleOffset = 120f // Positioned at 120°
};

AscendancySubNode subNode3 = new AscendancySubNode
{
    name = "Lightning Specialization",
    icon = lightningIconSprite,
    description = "Gain +20% Lightning damage",
    angleOffset = 240f // Positioned at 240°
};

// Add to choice node
passive.subNodes = new List<AscendancySubNode> { subNode1, subNode2, subNode3 };
```

### Step 3: Auto-Positioning

If you don't set `angleOffset` for sub-nodes, they'll be automatically spaced evenly:
- 2 sub-nodes: 0°, 180°
- 3 sub-nodes: 0°, 120°, 240°
- 4 sub-nodes: 0°, 90°, 180°, 270°

## Visual Behavior

### Before Selection
- Main node displays normally
- Sub-nodes circle around it (smaller, slightly transparent)
- Player can click any sub-node

### After Selection
- Main node displays the selected sub-node's sprite
- Sub-nodes are hidden
- Main node shows the chosen option

### After Respecc
- Main node returns to normal state
- Sub-nodes reappear in circle
- Player can select a different option

## Angle Guide

Use the angle guide for manual positioning:

```
         ↑     90° (Up)
180° ←───●───→ 0° (Right)         
         ↓     270° (Down)
```

Common angles:
- `0°` = Right
- `60°` = Up-right
- `90°` = Up
- `120°` = Up-left
- `180°` = Left
- `240°` = Down-left
- `270°` = Down
- `300°` = Down-right

## Example Use Cases

1. **Elemental Specialization**: Choose Fire, Cold, or Lightning focus
2. **Combat Style**: Choose Aggressive, Defensive, or Balanced
3. **Resource Generation**: Choose Mana, Energy, or Momentum focus
4. **Weapon Type**: Choose Melee, Ranged, or Spell focus

## Technical Details

### Data Structure
- `AscendancyPassive.isChoiceNode` - Marks node as choice node
- `AscendancyPassive.subNodes` - List of sub-node options
- `AscendancyPassive.subNodeRadius` - Distance from main node
- `AscendancyPassive.selectedSubNodeIndex` - Currently selected index (-1 = no selection)

### Progression Tracking
- `CharacterAscendancyProgress.choiceNodeSelections` - Stores selections
- `SelectSubNode(nodeName, subNodeIndex, ascendancy)` - Selects a sub-node
- `GetSelectedSubNodeIndex(nodeName)` - Gets current selection
- `ClearSubNodeSelection(nodeName)` - Clears selection (on respecc)

### UI Behavior
- Sub-nodes spawn automatically when choice node is unlocked
- Sub-nodes are hidden when a selection is made
- Main node sprite updates to show selection
- On respecc, sub-nodes reappear

## Best Practices

1. **Clear Icons**: Use distinct icons for each sub-node option
2. **Descriptive Names**: Make sub-node names clear about what they do
3. **Balanced Options**: Ensure all sub-nodes are equally viable choices
4. **Visual Distinction**: Sub-nodes are smaller (75% scale) and slightly transparent
5. **Radius Tuning**: Adjust `subNodeRadius` based on node size and screen space

## Notes

- Sub-nodes don't have their own point costs (main node cost covers selection)
- Only one sub-node can be selected per choice node
- Selection persists until respecc
- Sub-nodes are automatically positioned in a circle
- Manual `angleOffset` overrides auto-positioning
























