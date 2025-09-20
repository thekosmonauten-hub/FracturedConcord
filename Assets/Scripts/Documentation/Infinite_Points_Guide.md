# Infinite Points Guide

## Overview

The Infinite Points feature allows you to bypass point restrictions in the passive tree system, enabling unlimited node allocation for testing, development, and debugging purposes.

## Features

- **Infinite Points Mode**: Allocate nodes without consuming points
- **Debug Points**: Set a high point value for testing
- **Easy Toggle**: Quick enable/disable through context menus
- **Preserved Functionality**: All other passive tree features work normally

## How to Use

### Method 1: Inspector Settings

1. **Select PassiveTreeManager** in the scene
2. **In the Inspector**, find the "Infinite Points (Debug)" section
3. **Check "Infinite Points"** to enable infinite mode
4. **Set "Debug Points"** to your desired value (default: 999)
5. **Press Play** to test

### Method 2: Context Menu (Recommended)

1. **Select PassiveTreeManager** in the scene
2. **Right-click** on the PassiveTreeManager component
3. **Choose one of these options**:
   - **"Toggle Infinite Points"**: Switch between enabled/disabled
   - **"Enable Infinite Points"**: Turn on infinite points
   - **"Disable Infinite Points"**: Turn off infinite points
   - **"Add 1000 Points"**: Add 1000 points to normal mode

## How It Works

### When Infinite Points is ENABLED:
- **Node Allocation**: Nodes can be allocated without consuming points
- **Point Display**: Shows the debug points value (e.g., 999)
- **Node Requirements**: Still respects node connection requirements
- **Cost Display**: Shows node costs but doesn't consume them

### When Infinite Points is DISABLED:
- **Normal Operation**: Standard point consumption applies
- **Point Limits**: Respects maxPoints and available points
- **Cost Enforcement**: Nodes consume points when allocated

## Use Cases

### 1. Testing and Development
- **Test all nodes** without grinding for points
- **Debug node connections** and requirements
- **Verify stat calculations** with full tree allocation

### 2. Game Balance Testing
- **Test different builds** quickly
- **Verify node interactions** and synergies
- **Balance node costs** and effects

### 3. Player Experience Testing
- **Test UI responsiveness** with many allocated nodes
- **Verify performance** with full tree allocation
- **Test save/load** functionality

### 4. Debugging
- **Isolate issues** from point restrictions
- **Test edge cases** with unlimited allocation
- **Verify system stability** under load

## Configuration Options

### Infinite Points Settings

```csharp
[Header("Infinite Points (Debug)")]
[SerializeField] private bool infinitePoints = false;    // Enable/disable infinite points
[SerializeField] private int debugPoints = 999;         // Point value to display
```

### Debug Points Value
- **999**: Default value, shows as "999 points available"
- **9999**: Higher value for extensive testing
- **Custom**: Set any value you prefer

## Context Menu Commands

### Quick Access Commands
- **Toggle Infinite Points**: Switch between enabled/disabled
- **Enable Infinite Points**: Turn on infinite mode
- **Disable Infinite Points**: Turn off infinite mode
- **Add 1000 Points**: Add points to normal mode

### Usage
1. **Select PassiveTreeManager** in the scene
2. **Right-click** on the component
3. **Choose** the desired command
4. **Check Console** for confirmation messages

## Integration with Existing Systems

### Passive Tree Manager
- **Fully Integrated**: Works with all existing passive tree features
- **Event System**: Triggers normal allocation events
- **State Management**: Updates player state correctly
- **Persistence**: Saves allocated nodes normally

### UI Systems
- **Point Display**: Shows debug points when enabled
- **Node States**: Updates visual states correctly
- **Allocation Feedback**: Provides normal user feedback

### Stat Calculation
- **Full Integration**: All allocated nodes contribute to stats
- **Real-time Updates**: Stats update as nodes are allocated
- **Cache Management**: Properly manages stat cache

## Best Practices

### Development Workflow
1. **Enable infinite points** for testing
2. **Test all features** thoroughly
3. **Disable infinite points** for final testing
4. **Verify normal operation** works correctly

### Testing Strategy
1. **Test with infinite points** to verify functionality
2. **Test with limited points** to verify restrictions
3. **Test point consumption** in normal mode
4. **Test save/load** in both modes

### Performance Considerations
- **Large Trees**: Infinite points can create very large allocations
- **Memory Usage**: Monitor memory usage with full tree allocation
- **UI Performance**: Test UI responsiveness with many nodes

## Troubleshooting

### Infinite Points Not Working
1. **Check PassiveTreeManager**: Ensure it's selected in the scene
2. **Verify Settings**: Check "Infinite Points" is enabled in Inspector
3. **Check Console**: Look for confirmation messages
4. **Restart Scene**: Try restarting the scene if needed

### Points Not Displaying Correctly
1. **Check Debug Points Value**: Ensure it's set to a reasonable number
2. **Verify UI Updates**: Check if UI is refreshing properly
3. **Check Console Logs**: Look for point-related debug messages

### Nodes Still Requiring Points
1. **Verify Infinite Points**: Ensure the feature is actually enabled
2. **Check Node Requirements**: Some nodes may have other requirements
3. **Check Console**: Look for allocation messages
4. **Test Simple Nodes**: Try allocating basic nodes first

## Safety Features

### Data Protection
- **No Data Loss**: Infinite points doesn't affect saved data
- **Reversible**: Can be disabled at any time
- **Isolated**: Only affects point consumption, not other systems

### Validation
- **Node Requirements**: Still validates node connection requirements
- **State Integrity**: Maintains proper state management
- **Event System**: Triggers proper events and callbacks

## Future Enhancements

### Potential Features
- **Per-Board Infinite Points**: Enable for specific boards only
- **Point Multipliers**: Set custom point multipliers
- **Conditional Infinite Points**: Enable based on conditions
- **Point Refund Testing**: Test point refund scenarios

### Integration Ideas
- **Cheat System**: Integrate with broader cheat/debug system
- **Testing Framework**: Automated testing with infinite points
- **Balance Tools**: Tools for balancing node costs and effects

## Conclusion

The Infinite Points feature provides a powerful tool for testing and development while maintaining the integrity of the passive tree system. Use it responsibly and always test with normal point restrictions before releasing features.
