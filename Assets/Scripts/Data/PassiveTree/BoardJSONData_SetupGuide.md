# BoardJSONData Setup Guide

## Overview

The `BoardJSONData` script consolidates all stats from allocated `CellJsonData` components in a board, providing a single source of truth for board-level statistics. This solves the issue where `StatsSummaryPanel` couldn't find allocated nodes through the `PassiveTreeManager`.

## Key Features

- **Consolidated Stats**: Automatically sums all stats from allocated cells
- **Real-time Updates**: Events fire when stats change
- **Performance Optimized**: Caches results and only updates when necessary
- **Debug Tools**: Comprehensive logging and debugging methods
- **Event System**: Integrates with `StatsSummaryPanel` for automatic UI updates

## Setup Instructions

### 1. Add BoardJSONData to Your Board

1. **Find your board GameObject** (the parent of all cell prefabs)
2. **Add the `BoardJSONData` component** to the board GameObject
3. **Configure the board ID** (e.g., "core_board", "fire_board", etc.)

### 2. Configure BoardJSONData

```csharp
[Header("Board Configuration")]
[SerializeField] private string boardId = "core_board";           // Unique identifier for this board
[SerializeField] private bool autoUpdateOnStart = true;          // Auto-refresh on Start()
[SerializeField] private bool enableDebugLogging = true;        // Enable detailed logging
```

### 3. Integration with StatsSummaryPanel

The `StatsSummaryPanel` will automatically:
- Find the `BoardJSONData` component
- Subscribe to stats update events
- Use consolidated stats instead of searching for nodes manually

### 4. Manual Integration (if needed)

If you need to manually integrate with other systems:

```csharp
// Get the BoardJSONData component
BoardJSONData boardData = FindObjectOfType<BoardJSONData>();

// Get consolidated stats
Dictionary<string, float> stats = boardData.GetConsolidatedStats();

// Subscribe to updates
boardData.OnStatsUpdated += OnStatsChanged;

// Force refresh
boardData.ForceRefreshAllData();
```

## How It Works

### 1. Initialization
- `BoardJSONData` finds all `CellJsonData` components in its children
- Identifies which cells are allocated (purchased)
- Consolidates stats from all allocated cells

### 2. Stat Consolidation
- Extracts stats from each `CellJsonData.NodeStats`
- Maps JSON stat fields to display names
- Sums duplicate stat types (e.g., multiple +5 Dexterity nodes = +10 Dexterity)

### 3. Real-time Updates
- Listens for node allocation/deallocation events
- Automatically refreshes when nodes change
- Fires `OnStatsUpdated` event for UI updates

### 4. Performance
- Caches consolidated stats
- Only recalculates when nodes change
- Compares old vs new stats to avoid unnecessary updates

## Debug Tools

### Context Menu Methods

1. **"Force Refresh All Data"** - Manually refresh all stats
2. **"Debug Allocated Cells"** - Show all allocated cells
3. **"Debug Consolidated Stats"** - Show all consolidated stats

### Debug Logging

Enable `enableDebugLogging` to see:
- Which cells are found and allocated
- How many stats are consolidated
- When stats are updated
- Performance metrics

## Expected Results

With this system, your `StatsSummaryPanel` should now display:
- **45 Dexterity** (from multiple nodes)
- **25 Intelligence** (from multiple nodes)  
- **16% increased Spell damage** (from spell damage nodes)
- **8% Increased energy shield** (from energy shield nodes)

## Troubleshooting

### Issue: "No stats allocated" still showing
**Solution**: 
1. Check that `BoardJSONData` is on the board GameObject
2. Run "Force Refresh All Data" context menu
3. Check debug logs for allocated cells

### Issue: Stats not updating in real-time
**Solution**:
1. Ensure `StatsSummaryPanel` is subscribed to `BoardJSONData` events
2. Check that `PassiveTreeManager` events are firing
3. Verify `BoardJSONData.OnNodeAllocated/Deallocated` are being called

### Issue: Wrong stats displayed
**Solution**:
1. Check that `CellJsonData` components have correct JSON data
2. Verify that allocated cells have `IsPurchased = true`
3. Use "Debug Consolidated Stats" to see what's being calculated

## Integration with Existing Systems

### PassiveTreeStatsIntegration
The `BoardJSONData` works alongside `PassiveTreeStatsIntegration`:
- `PassiveTreeStatsIntegration` applies stats to character stats
- `BoardJSONData` provides stats for UI display
- Both use the same `CellJsonData` source

### Multiple Boards
For extension boards:
- Each board should have its own `BoardJSONData` component
- `StatsSummaryPanel` will find and use the first one found
- Consider creating a `MultiBoardStatsManager` for multiple boards

## Performance Considerations

- **Startup**: Initial consolidation happens once on Start()
- **Runtime**: Only updates when nodes change (not every frame)
- **Memory**: Caches results to avoid repeated calculations
- **Events**: Minimal overhead for real-time updates

## Future Enhancements

1. **Multi-Board Support**: Handle multiple boards in one system
2. **Stat Categories**: Group stats by type (Attributes, Damage, etc.)
3. **Stat History**: Track stat changes over time
4. **Export/Import**: Save/load consolidated stats
5. **Visualization**: Show stat sources and calculations

