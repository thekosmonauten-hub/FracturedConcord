# Passive Tree Tooltip and Auto-Load Fixes

## Overview
This document explains the fixes implemented for tooltip behavior and automatic node loading in the passive tree system.

## Issues Fixed

### 1. Tooltip Visibility Issue
**Problem**: Tooltips were showing immediately on hover, making the interface cluttered.

**Solution**: 
- Added tooltip delay system with configurable delay (default: 0.5 seconds)
- Tooltips now only appear after hovering for the specified delay
- Added `showTooltipOnHover` toggle to enable/disable tooltips entirely

### 2. Auto-Loading Issue
**Problem**: Nodes weren't automatically loading when entering the scene, requiring manual "Set Board Data from Manager" action.

**Solution**:
- Improved auto-initialization logic to always run on Start()
- Added better timing to ensure manager is fully initialized
- Added fallback to use existing board data if manager isn't available
- Added "Force Auto Initialize" context menu for debugging

## Implementation Details

### Tooltip System Changes

#### New Fields in PassiveTreeNodeUI
```csharp
[Header("Tooltip Settings")]
[SerializeField] private float tooltipDelay = 0.5f;
[SerializeField] private bool showTooltipOnHover = true;

// Runtime variables
private float hoverTimer = 0f;
private bool tooltipShown = false;
```

#### Tooltip Behavior
1. **On Hover Start**: Reset timer and hide any existing tooltip
2. **During Hover**: Increment timer in Update()
3. **After Delay**: Show tooltip if still hovering
4. **On Hover End**: Hide tooltip immediately

#### New Methods
- `ShowTooltip()`: Displays tooltip after delay
- `HideTooltip()`: Hides tooltip immediately
- `Update()`: Handles tooltip delay logic

### Auto-Loading System Changes

#### Improved Start() Method
```csharp
private void Start()
{
    // Always try to auto-initialize, even if boardData is set
    // This ensures the board loads properly when entering the scene
    StartCoroutine(AutoInitializeBoard());
}
```

#### Enhanced AutoInitializeBoard() Method
1. **Event Subscription**: Subscribe to manager events
2. **Timing**: Wait 0.1 seconds for manager initialization
3. **Priority**: Try manager data first, then existing board data
4. **Fallback**: Create visual with existing data if manager unavailable

#### New Context Menu
- `Force Auto Initialize`: Manually trigger auto-initialization for debugging

## Configuration Options

### Tooltip Settings
- **Tooltip Delay**: How long to wait before showing tooltip (0.5s default)
- **Show Tooltip On Hover**: Enable/disable tooltip system entirely
- **Tooltip Panel**: Reference to the tooltip UI panel
- **Tooltip Text**: Reference to the tooltip text component

### Auto-Load Settings
- **Show Debug Info**: Enable detailed logging for troubleshooting
- **Board Data**: Manual assignment (fallback if auto-load fails)
- **Node Prefab**: Reference to node prefab for creation
- **Connection Line Prefab**: Reference to connection line prefab

## Usage Instructions

### For Tooltips
1. **Configure Delay**: Set `tooltipDelay` in inspector (0.5s recommended)
2. **Enable/Disable**: Toggle `showTooltipOnHover` as needed
3. **Test**: Hover over nodes to verify delay behavior

### For Auto-Loading
1. **Automatic**: Should work automatically when entering scene
2. **Manual Trigger**: Use "Force Auto Initialize" context menu if needed
3. **Debug**: Enable `showDebugInfo` to see initialization logs

## Troubleshooting

### Tooltips Not Showing
1. Check `showTooltipOnHover` is enabled
2. Verify `tooltipPanel` and `tooltipText` are assigned
3. Adjust `tooltipDelay` if needed
4. Check console for errors

### Nodes Not Loading
1. Ensure `PassiveTreeManager` exists in scene
2. Verify `CoreBoard` asset is assigned to manager
3. Use "Force Auto Initialize" context menu
4. Check console for initialization logs
5. Verify `nodePrefab` is assigned

### Performance Issues
1. Reduce `tooltipDelay` for faster response
2. Disable `showTooltipOnHover` if not needed
3. Check for multiple Update() calls in tooltip system

## Future Enhancements

### Tooltip System
- **Fade Animation**: Smooth fade in/out for tooltips
- **Positioning**: Smart positioning to avoid screen edges
- **Rich Content**: Support for images and formatting in tooltips
- **Custom Styling**: Theme-aware tooltip appearance

### Auto-Load System
- **Scene Persistence**: Remember board state across scene changes
- **Loading States**: Visual feedback during initialization
- **Error Recovery**: Automatic retry on initialization failure
- **Configuration**: Save/load user preferences for auto-load behavior

## Testing Checklist

### Tooltip Testing
- [ ] Tooltips appear after delay
- [ ] Tooltips hide immediately on exit
- [ ] Multiple tooltips don't conflict
- [ ] Tooltip content is correct
- [ ] Tooltip positioning is appropriate

### Auto-Load Testing
- [ ] Nodes appear automatically on scene load
- [ ] Manager events are properly subscribed
- [ ] Fallback to existing data works
- [ ] Context menu functions correctly
- [ ] Debug logs provide useful information

### Integration Testing
- [ ] Tooltips work with node interactions
- [ ] Auto-load doesn't interfere with manual setup
- [ ] Performance is acceptable
- [ ] No console errors or warnings
