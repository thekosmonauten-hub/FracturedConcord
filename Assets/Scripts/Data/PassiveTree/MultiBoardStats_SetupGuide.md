# MultiBoardStatsManager Setup Guide

## ✅ **Fixed: Extension Board Stats Issue!**

The `MultiBoardStatsManager` now handles stats from **both core board AND extension boards**, ensuring all allocated nodes are included in the stats summary.

## 🎯 **What It Does**

- **Consolidates stats** from core board + all extension boards
- **Real-time updates** when nodes are allocated/deallocated on ANY board
- **Single source of truth** for all passive tree stats
- **Automatic detection** of all boards in the scene

## 🚀 **Setup Instructions**

### 1. Add MultiBoardStatsManager

1. **Create an empty GameObject** in your scene (e.g., "StatsManager")
2. **Add the `MultiBoardStatsManager` component** to this GameObject
3. **Configure the settings**:
   ```csharp
   [SerializeField] private bool autoUpdateOnStart = true;          // Auto-refresh on Start()
   [SerializeField] private bool enableDebugLogging = true;        // Enable detailed logging
   [SerializeField] private float updateInterval = 0.1f;          // Check for changes every 0.1 seconds
   ```

### 2. Keep Your Existing BoardJSONData

- **Keep `BoardJSONData`** on your core board (it will be automatically detected)
- **Extension boards** don't need `BoardJSONData` - they're handled directly

### 3. StatsSummaryPanel Integration

The `StatsSummaryPanel` will automatically:
- **Find `MultiBoardStatsManager`** (preferred method)
- **Fall back to `BoardJSONData`** if not found
- **Subscribe to global stats updates**
- **Display consolidated stats from all boards**

## 🔧 **How It Works**

### 1. **Automatic Board Detection**
```
MultiBoardStatsManager finds:
├── Core Board (with BoardJSONData)
├── Extension Board 1 (Cold)
├── Extension Board 2 (Fire)
└── Extension Board 3 (Lightning)
```

### 2. **Stats Consolidation**
- **Core board stats** from `BoardJSONData`
- **Extension board stats** from direct `CellJsonData` access
- **Global consolidation** of all stats
- **Real-time updates** when any node changes

### 3. **Event Flow**
```
Node Clicked → PassiveTreeManager Event → MultiBoardStatsManager → StatsSummaryPanel
```

## 🧪 **Testing Steps**

### 1. Setup Verification
Check console for these messages on Start():
```
[MultiBoardStatsManager] Found BoardJSONData: CoreBoard
[MultiBoardStatsManager] Found ExtensionBoard: ColdBoard
[MultiBoardStatsManager] Multi-board system initialized - 2 boards, X allocated nodes
[StatsSummaryPanel] ✅ Found MultiBoardStatsManager
```

### 2. Test Core Board
1. **Click a node on the core board**
2. **Check console** for:
   ```
   [MultiBoardStatsManager] Board stats updated: X stat types
   [StatsSummaryPanel] Global stats updated from MultiBoardStatsManager: X stat types
   ```

### 3. Test Extension Boards
1. **Click a node on an extension board**
2. **Check console** for:
   ```
   [MultiBoardStatsManager] PassiveTreeManager node allocated at (X, Y)
   [MultiBoardStatsManager] Refreshing extension board stats
   [StatsSummaryPanel] Global stats updated from MultiBoardStatsManager: X stat types
   ```

## 🛠️ **Debug Tools**

### On `MultiBoardStatsManager`:
- **"Force Immediate Refresh"** - Forces refresh of all boards
- **"Debug All Boards"** - Shows all detected boards
- **"Debug Global Stats"** - Shows consolidated stats from all boards

### On `StatsSummaryPanel`:
- **"Force Show and Update Stats"** - Shows panel and forces update
- **"Debug Stats Calculation"** - Tests stats calculation

## 📊 **Expected Results**

With this system, your stats should now include:
- **Core board stats** ✅
- **Extension board stats** ✅ (Cold, Fire, Lightning, etc.)
- **Real-time updates** when clicking nodes on ANY board ✅
- **Consolidated totals** from all boards ✅

## 🐛 **Troubleshooting**

### Issue: Extension board stats not showing
**Check:**
1. Is `MultiBoardStatsManager` in the scene?
2. Are extension boards being detected?
3. Are extension board nodes being found as allocated?

**Solution:**
```csharp
// Use "Debug All Boards" context menu on MultiBoardStatsManager
// Check if extension boards are listed
```

### Issue: Stats not updating in real-time
**Check:**
1. Are `PassiveTreeManager` events firing?
2. Is `MultiBoardStatsManager` subscribed to events?
3. Is `StatsSummaryPanel` subscribed to global stats updates?

**Solution:**
```csharp
// Use "Force Immediate Refresh" on MultiBoardStatsManager
// This will force refresh all boards and trigger events
```

### Issue: Wrong stats displayed
**Check:**
1. Are all boards being detected?
2. Are allocated nodes being found correctly?
3. Are stats being consolidated properly?

**Solution:**
```csharp
// Use "Debug Global Stats" on MultiBoardStatsManager
// This shows the final consolidated stats from all boards
```

## 🚀 **Quick Test**

1. **Add `MultiBoardStatsManager`** to a GameObject in your scene
2. **Click "Force Immediate Refresh"** on `MultiBoardStatsManager`
3. **Check console** for board detection and stats consolidation
4. **Click nodes on both core and extension boards**
5. **Stats should update automatically** from all boards!

## 🔄 **Migration from Single Board**

If you were using `BoardJSONData` only:
1. **Keep your existing `BoardJSONData`** on the core board
2. **Add `MultiBoardStatsManager`** to the scene
3. **`StatsSummaryPanel`** will automatically use `MultiBoardStatsManager` (preferred)
4. **Extension boards** will now be included automatically!

The system now supports **unlimited extension boards** and will automatically detect and include stats from all of them! 🎉

