# Real-Time Stats Debug Guide

## ✅ Fixed: Automatic Refresh Issue

The `BoardJSONData` now automatically subscribes to `PassiveTreeManager` events and refreshes when nodes are allocated/deallocated.

## 🔧 How It Works Now

1. **`BoardJSONData`** automatically subscribes to `PassiveTreeManager.OnNodeAllocated/Deallocated` events
2. **When a node is clicked**, the `PassiveTreeManager` fires the event
3. **`BoardJSONData`** receives the event and refreshes its data
4. **`BoardJSONData`** fires `OnStatsUpdated` event
5. **`StatsSummaryPanel`** receives the update and refreshes the display

## 🧪 Testing Steps

### 1. Setup Verification
1. **Add `BoardJSONData`** component to your board GameObject
2. **Check the console** for these messages on Start():
   ```
   [BoardJSONData] Subscribing to PassiveTreeManager events
   [StatsSummaryPanel] ✅ Found BoardJSONData
   ```

### 2. Test Real-Time Updates
1. **Click on a passive node** to allocate it
2. **Check the console** for these messages:
   ```
   [PassiveTreeManager] Node allocated event triggered
   [BoardJSONData] PassiveTreeManager node allocated at (X, Y)
   [BoardJSONData] Node allocated: [NodeName]
   [BoardJSONData] Stats updated from BoardJSONData: X stat types
   ```

### 3. Debug Tools

#### On `BoardJSONData` component:
- **"Force Immediate Refresh"** - Forces refresh and triggers events
- **"Debug Allocated Cells"** - Shows all allocated cells
- **"Debug Consolidated Stats"** - Shows all consolidated stats

#### On `StatsSummaryPanel` component:
- **"Force Show and Update Stats"** - Shows panel and forces update
- **"Debug Stats Calculation"** - Tests stats calculation

## 🐛 Troubleshooting

### Issue: No automatic refresh when clicking nodes
**Check:**
1. Is `BoardJSONData` on the board GameObject?
2. Are you seeing the subscription messages in console?
3. Are `PassiveTreeManager` events firing?

**Solution:**
```csharp
// Add this to BoardJSONData Start() method for debugging
Debug.Log($"[BoardJSONData] Event subscription status: {PassiveTree.PassiveTreeManager.OnNodeAllocated != null}");
```

### Issue: Stats not updating in UI
**Check:**
1. Is `StatsSummaryPanel` subscribed to `BoardJSONData.OnStatsUpdated`?
2. Is the panel visible (`isVisible = true`)?

**Solution:**
```csharp
// Force show the panel and update
statsSummaryPanel.ForceShowAndUpdateStats();
```

### Issue: Events not firing
**Check:**
1. Are `PassiveTreeManager` events being triggered?
2. Is `BoardJSONData` finding the `CellJsonData` component?

**Solution:**
```csharp
// Add this to PassiveTreeManager when triggering events
Debug.Log($"[PassiveTreeManager] Triggering OnNodeAllocated for {position}");
OnNodeAllocated?.Invoke(position, cell);
```

## 📊 Expected Console Output

When you click a node, you should see:
```
[PassiveTreeManager] Cell clicked: (X, Y)
[PassiveTreeManager] ✅ Node allocated event triggered
[BoardJSONData] PassiveTreeManager node allocated at (X, Y)
[BoardJSONData] Node allocated: [NodeName]
[BoardJSONData] Found X allocated cells in board core_board
[BoardJSONData] Consolidated X unique stat types
[StatsSummaryPanel] Stats updated from BoardJSONData: X stat types
[StatsSummaryPanel] UpdateStatsDisplay called - isVisible: True
```

## 🚀 Quick Test

1. **Add `BoardJSONData`** to your board
2. **Click "Force Immediate Refresh"** on `BoardJSONData`
3. **Check if stats appear** in the `StatsSummaryPanel`
4. **Click a passive node** and watch the console
5. **Stats should update automatically** without manual refresh

## 🔄 Manual Override

If automatic updates still don't work, you can manually trigger updates:

```csharp
// Get the BoardJSONData component
BoardJSONData boardData = FindObjectOfType<BoardJSONData>();

// Force refresh
boardData.ForceImmediateRefresh();

// Or just refresh stats
boardData.RefreshAllocatedCells();
boardData.ConsolidateStats();
```

The system should now work automatically! 🎉

