# Passive Tree Canvas Size Optimization Guide

## Overview
This guide explains how to optimize the canvas size for your passive tree to make it more visible and easier to work with during development.

## Recommended Canvas Settings

### For Development (Larger, Easier to See)
```
Canvas Scaler Settings:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 x 1080 (or 2560 x 1440)
- Screen Match Mode: Match Width Or Height
- Match: 0.5 (or adjust as needed)
- Reference Pixels Per Unit: 100
```

### For Production (Flexible, User-Friendly)
```
Canvas Scaler Settings:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 x 1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5
- Reference Pixels Per Unit: 100
```

## Step-by-Step Setup

### Step 1: Adjust Canvas Scaler
1. **Select your Canvas GameObject**
2. **Find the Canvas Scaler component**
3. **Set UI Scale Mode to "Scale With Screen Size"**
4. **Set Reference Resolution to 1920 x 1080**
5. **Set Screen Match Mode to "Match Width Or Height"**
6. **Set Match to 0.5** (adjust between 0.0 and 1.0)

### Step 2: Adjust Node Spacing
1. **Select your PassiveTreeBoardUI component**
2. **Increase Node Spacing**:
   - Current: 80f x 80f
   - Recommended: 120f x 120f (or higher)
3. **Test the layout** and adjust as needed

### Step 3: Adjust Node Prefab Size
1. **Select your Node Prefab**
2. **Increase the RectTransform size**:
   - Current: ~50x50 pixels
   - Recommended: ~80x80 pixels (or larger)
3. **Adjust text size** to match the new node size

## Canvas Size Recommendations

### Development Phase (Large and Clear)
```
Reference Resolution: 2560 x 1440
Node Spacing: 150f x 150f
Node Size: 100x100 pixels
Text Size: 16-18pt
```

### Testing Phase (Medium)
```
Reference Resolution: 1920 x 1080
Node Spacing: 120f x 120f
Node Size: 80x80 pixels
Text Size: 14-16pt
```

### Production Phase (Flexible)
```
Reference Resolution: 1920 x 1080
Node Spacing: 100f x 100f
Node Size: 70x70 pixels
Text Size: 12-14pt
+ Zoom System for user control
```

## Quick Setup Script

Add this context menu to your PassiveTreeBoardUI for quick size adjustments:

```csharp
[ContextMenu("Set Development Size")]
public void SetDevelopmentSize()
{
    nodeSpacing = new Vector2(150f, 150f);
    // Reposition all nodes
    foreach (var kvp in nodeUIs)
    {
        var nodeUI = kvp.Value;
        var node = nodeUI.Node;
        var rectTransform = nodeUI.GetComponent<RectTransform>();
        var position = GetNodePosition(node.position);
        rectTransform.anchoredPosition = position;
    }
    Debug.Log("Set to development size (150x150 spacing)");
}

[ContextMenu("Set Production Size")]
public void SetProductionSize()
{
    nodeSpacing = new Vector2(100f, 100f);
    // Reposition all nodes
    foreach (var kvp in nodeUIs)
    {
        var nodeUI = kvp.Value;
        var node = nodeUI.Node;
        var rectTransform = nodeUI.GetComponent<RectTransform>();
        var position = GetNodePosition(node.position);
        rectTransform.anchoredPosition = position;
    }
    Debug.Log("Set to production size (100x100 spacing)");
}
```

## Screen Size Considerations

### Desktop (1920x1080 and up)
- **Large Canvas**: 2560x1440 reference
- **Node Spacing**: 120-150f
- **Node Size**: 80-100 pixels

### Laptop (1366x768 to 1920x1080)
- **Medium Canvas**: 1920x1080 reference
- **Node Spacing**: 100-120f
- **Node Size**: 70-80 pixels

### Tablet (1024x768 to 1366x768)
- **Small Canvas**: 1366x768 reference
- **Node Spacing**: 80-100f
- **Node Size**: 60-70 pixels

### Mobile (320x568 to 1024x768)
- **Mobile Canvas**: 1024x768 reference
- **Node Spacing**: 60-80f
- **Node Size**: 50-60 pixels
- **Touch-friendly spacing**

## Performance Considerations

### Large Canvas Sizes
- **Pros**: Better visibility, easier development
- **Cons**: Higher memory usage, potential performance impact
- **Recommendation**: Use for development, test performance

### Optimal Balance
- **Reference Resolution**: 1920x1080 (good balance)
- **Node Spacing**: 100-120f (readable but not excessive)
- **Node Count**: 49 nodes (7x7 grid) - manageable

## Testing Different Sizes

### Quick Test Method
1. **Duplicate your scene** for testing
2. **Try different Canvas Scaler settings**
3. **Test on different screen resolutions**
4. **Check performance impact**
5. **Get user feedback**

### Automated Testing
```csharp
[ContextMenu("Test Different Sizes")]
public void TestDifferentSizes()
{
    var sizes = new Vector2[] {
        new Vector2(80f, 80f),   // Small
        new Vector2(100f, 100f), // Medium
        new Vector2(120f, 120f), // Large
        new Vector2(150f, 150f)  // Extra Large
    };
    
    foreach (var size in sizes)
    {
        Debug.Log($"Testing size: {size}");
        nodeSpacing = size;
        // Reposition nodes
        // Test performance
        // Wait for user input
    }
}
```

## Integration with Zoom System

### Best Practice: Large Canvas + Zoom
1. **Set Canvas to larger size** for development
2. **Keep zoom system** for user flexibility
3. **Default zoom to 0.8** to show more content
4. **Allow users to zoom in** for detail

### Canvas Size + Zoom Settings
```
Canvas Reference: 1920x1080
Default Zoom: 0.8 (shows more of the tree)
Min Zoom: 0.5 (shows entire tree)
Max Zoom: 2.0 (detailed view)
```

## Troubleshooting

### Nodes Too Small
- Increase Canvas reference resolution
- Increase node spacing
- Increase node prefab size
- Increase text size

### Nodes Too Large
- Decrease Canvas reference resolution
- Decrease node spacing
- Decrease node prefab size
- Decrease text size

### Performance Issues
- Reduce Canvas reference resolution
- Reduce node spacing
- Optimize node prefabs
- Use object pooling for large trees

### Text Not Readable
- Increase text size
- Use better fonts
- Add text shadows/outlines
- Ensure proper contrast
