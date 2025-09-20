# Passive Tree Tooltip Prefab Integration Guide

## Overview
This guide explains how to use custom tooltip prefabs with passive tree nodes, giving you full control over tooltip appearance and behavior.

## Features

### Tooltip Prefab System
- ✅ **Custom Prefab Support**: Assign any UI prefab as a tooltip
- ✅ **Automatic Positioning**: Tooltip positioned relative to node with offset
- ✅ **Dynamic Content**: Tooltip content automatically populated from node data
- ✅ **Fallback System**: Falls back to built-in tooltip if prefab not assigned
- ✅ **Easy Testing**: Context menu methods for testing tooltip behavior

## Setup Instructions

### 1. Create a Tooltip Prefab

#### Basic Tooltip Prefab Structure
```
TooltipPrefab (GameObject)
├── Background (Image)
├── Content (TextMeshProUGUI) ← REQUIRED
└── Border (Image) [Optional]
```

#### Required Components
- **TextMeshProUGUI**: Must contain a TextMeshProUGUI component for displaying tooltip content
- **RectTransform**: For proper positioning
- **Canvas Group** [Optional]: For fade effects

#### Recommended Settings
- **Pivot**: Set to bottom-center for positioning above nodes
- **Anchors**: Set to middle-center for flexible positioning
- **Size**: Adjust based on your content needs

### 2. Configure the Node Prefab

#### Inspector Settings
1. **Open your NodePrefab** in the scene or prefab editor
2. **Select the PassiveTreeNodeUI component**
3. **In the "Tooltip Prefab" section:**
   - **Tooltip Prefab**: Drag your tooltip prefab here
   - **Tooltip Offset**: Set positioning offset (default: 0, 50)
   - **Use Prefab Tooltip**: Check this to enable prefab tooltips

#### Example Settings
```
Tooltip Prefab: [YourTooltipPrefab]
Tooltip Offset: (0, 50)  // 50 pixels above the node
Use Prefab Tooltip: ✓    // Enable prefab tooltips
```

### 3. Tooltip Content Format

The system automatically generates tooltip content in this format:

```
Node Name
Node Description

Cost: X points
Rank: X/Y (if applicable)

Stats:
• Stat1: +Value1
• Stat2: +Value2
• ...

Requirements:
• Requirement1
• Requirement2
• ...
```

## Implementation Details

### Automatic Initialization
```csharp
// Tooltip prefab is automatically initialized in Awake()
if (usePrefabTooltip && tooltipPrefab != null)
{
    InitializePrefabTooltip();
}
```

### Prefab Tooltip Creation
```csharp
private void InitializePrefabTooltip()
{
    // Create tooltip instance as child of the node
    tooltipInstance = Instantiate(tooltipPrefab, transform);
    tooltipInstance.name = "NodeTooltip";
    
    // Find the TextMeshProUGUI component in the prefab
    prefabTooltipText = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
    
    // Set up the tooltip content
    SetupPrefabTooltip();
    
    // Hide the tooltip initially
    tooltipInstance.SetActive(false);
}
```

### Dynamic Positioning
```csharp
private void PositionPrefabTooltip()
{
    if (tooltipInstance == null) return;
    
    var tooltipRect = tooltipInstance.GetComponent<RectTransform>();
    if (tooltipRect != null)
    {
        // Position the tooltip above the node with offset
        tooltipRect.anchoredPosition = tooltipOffset;
    }
}
```

## Usage Examples

### Basic Tooltip Prefab
Create a simple tooltip with just text:

1. **Create a new UI GameObject**
2. **Add an Image component** for background
3. **Add a TextMeshProUGUI component** for content
4. **Set the background color** and **text styling**
5. **Save as prefab** in your UI folder

### Advanced Tooltip Prefab
Create a tooltip with multiple sections:

```
AdvancedTooltipPrefab
├── Background (Image)
├── Header (TextMeshProUGUI)
├── Description (TextMeshProUGUI)
├── StatsContainer (VerticalLayoutGroup)
│   ├── Stat1 (TextMeshProUGUI)
│   ├── Stat2 (TextMeshProUGUI)
│   └── Stat3 (TextMeshProUGUI)
├── RequirementsContainer (VerticalLayoutGroup)
│   ├── Req1 (TextMeshProUGUI)
│   └── Req2 (TextMeshProUGUI)
└── CostDisplay (TextMeshProUGUI)
```

## Context Menu Methods

### Debug and Testing
- **Debug Tooltip Visibility**: Shows detailed tooltip status
- **Initialize Prefab Tooltip**: Manually initialize prefab tooltip
- **Test Show Prefab Tooltip**: Test tooltip display
- **Test Hide Prefab Tooltip**: Test tooltip hiding
- **Force Hide Tooltip**: Force hide any active tooltip

### Usage
1. **Right-click on a node** in the scene
2. **Select the desired context menu option**
3. **Check console for debug information**

## Troubleshooting

### Common Issues

#### Tooltip Not Showing
**Problem**: Tooltip prefab assigned but not showing on hover
**Solution**:
1. Check "Use Prefab Tooltip" is enabled
2. Verify prefab contains TextMeshProUGUI component
3. Use "Debug Tooltip Visibility" to check status
4. Try "Initialize Prefab Tooltip" context menu

#### Tooltip Positioned Incorrectly
**Problem**: Tooltip appears in wrong position
**Solution**:
1. Adjust "Tooltip Offset" in inspector
2. Check prefab's pivot point (recommend bottom-center)
3. Verify prefab's anchor settings
4. Use "Test Show Prefab Tooltip" to see positioning

#### Tooltip Content Not Updating
**Problem**: Tooltip shows old or incorrect content
**Solution**:
1. Check if node data is properly set
2. Verify TextMeshProUGUI component is found
3. Use "Debug Tooltip Visibility" to check content
4. Reinitialize the tooltip if needed

#### Performance Issues
**Problem**: Tooltips causing performance problems
**Solution**:
1. Use object pooling for tooltips
2. Limit tooltip complexity
3. Use Canvas Groups for fade effects
4. Consider disabling tooltips on mobile

### Debug Information
The "Debug Tooltip Visibility" context menu shows:
- Use Prefab Tooltip status
- Tooltip Prefab assignment
- Tooltip Instance status
- Prefab Tooltip Text component
- Built-in tooltip status
- Hover and timing information
- Tooltip offset values

## Best Practices

### Tooltip Design
1. **Keep it Simple**: Don't overload tooltips with too much information
2. **Consistent Styling**: Use consistent colors and fonts across all tooltips
3. **Readable Text**: Ensure text is readable against the background
4. **Appropriate Size**: Make tooltip large enough for content but not overwhelming

### Performance
1. **Object Pooling**: Consider pooling tooltip instances for better performance
2. **Lazy Loading**: Only create tooltips when needed
3. **Efficient Updates**: Update tooltip content only when necessary
4. **Mobile Optimization**: Simplify tooltips for mobile devices

### Positioning
1. **Screen Boundaries**: Ensure tooltips don't go off-screen
2. **Dynamic Positioning**: Consider different positions based on node location
3. **Offset Consistency**: Use consistent offsets across similar nodes
4. **Z-Order**: Ensure tooltips appear above other UI elements

## Advanced Features

### Custom Tooltip Content
You can extend the tooltip system by modifying the `SetupPrefabTooltip()` method:

```csharp
private void SetupPrefabTooltip()
{
    if (prefabTooltipText == null) return;
    
    // Custom tooltip content
    string tooltip = $"<b>{node.name}</b>\n";
    tooltip += $"<i>{node.description}</i>\n\n";
    
    if (node.cost > 0)
    {
        tooltip += $"<color=yellow>Cost: {node.cost} points</color>\n";
    }
    
    // Add custom formatting
    if (node.stats.Count > 0)
    {
        tooltip += "\n<color=green>Stats:</color>\n";
        foreach (var stat in node.stats)
        {
            tooltip += $"• <color=cyan>{stat.Key}</color>: +{stat.Value}\n";
        }
    }
    
    prefabTooltipText.text = tooltip;
}
```

### Multiple Tooltip Types
You can create different tooltip prefabs for different node types:

```csharp
[Header("Tooltip Prefabs")]
[SerializeField] private GameObject standardTooltipPrefab;
[SerializeField] private GameObject notableTooltipPrefab;
[SerializeField] private GameObject keystoneTooltipPrefab;

private void InitializePrefabTooltip()
{
    // Choose tooltip prefab based on node type
    GameObject selectedPrefab = GetTooltipPrefabForNodeType(node.type);
    
    if (selectedPrefab != null)
    {
        tooltipInstance = Instantiate(selectedPrefab, transform);
        // ... rest of initialization
    }
}

private GameObject GetTooltipPrefabForNodeType(NodeType type)
{
    switch (type)
    {
        case NodeType.Notable: return notableTooltipPrefab;
        case NodeType.Keystone: return keystoneTooltipPrefab;
        default: return standardTooltipPrefab;
    }
}
```

### Animation Support
Add animations to your tooltip prefab:

1. **Create an Animator** on your tooltip prefab
2. **Add fade-in/fade-out animations**
3. **Use Canvas Group** for smooth transitions
4. **Trigger animations** in ShowTooltip/HideTooltip methods

## Migration from Built-in Tooltips

### Step-by-Step Migration
1. **Create your tooltip prefab**
2. **Assign it to the Tooltip Prefab field**
3. **Enable "Use Prefab Tooltip"**
4. **Test with context menu methods**
5. **Adjust positioning and styling**
6. **Remove old tooltip panel references** (optional)

### Backward Compatibility
- Built-in tooltips still work if prefab is not assigned
- You can switch between prefab and built-in tooltips
- No existing functionality is broken

## Testing Checklist

### Basic Functionality
- [ ] Tooltip prefab assigned correctly
- [ ] "Use Prefab Tooltip" enabled
- [ ] Tooltip shows on hover
- [ ] Tooltip hides when not hovering
- [ ] Tooltip content is correct
- [ ] Tooltip positioning is appropriate

### Advanced Features
- [ ] Tooltip offset works correctly
- [ ] Tooltip appears above other UI
- [ ] Tooltip doesn't go off-screen
- [ ] Performance is acceptable
- [ ] Context menu methods work
- [ ] Debug information is helpful

### Integration
- [ ] Works with existing node system
- [ ] Works with persistence system
- [ ] Works with START node protection
- [ ] No conflicts with other systems
- [ ] Console shows no errors

## Future Enhancements

### Planned Features
- **Smart Positioning**: Automatic positioning to avoid screen edges
- **Animation System**: Built-in fade and scale animations
- **Rich Text Support**: Better formatting and styling options
- **Tooltip Templates**: Pre-made tooltip designs
- **Performance Optimization**: Object pooling and caching

### Custom Extensions
- **Custom Content Providers**: Plug-in system for custom content
- **Event System**: Events for tooltip show/hide
- **Styling System**: Theme-based tooltip styling
- **Accessibility**: Screen reader support and keyboard navigation







