# EquipmentScreen Preloading Optimization

## Overview

The EquipmentScreen was experiencing long load times due to heavy resource loading during initialization. This document outlines the optimizations implemented to preload critical assets in the Bootstrap scene.

## Heavy Dependencies Identified

### Critical (Loaded Immediately on EquipmentScreen Load)

1. **CurrencyDatabase** ⚠️ **MOST CRITICAL**
   - Loaded in: `CurrencyManager.cs` (EquipmentScreen)
   - Also loaded in: `LootManager`, `SeerCardVendor`, `MazeCurrencyManager`, etc.
   - Impact: High - Used immediately for currency display
   - Status: ✅ Preloaded in Bootstrap

2. **CardVisualAssets** ⚠️ **CRITICAL**
   - Loaded in: `CardCarouselUI.cs`, `CardDisplay.cs` (multiple times)
   - Impact: High - Used for all card displays in EquipmentScreen
   - Status: ✅ Preloaded in Bootstrap

3. **ItemDatabase**
   - Loaded in: `ItemDatabase.cs` (singleton)
   - Impact: Medium - Used by inventory grids
   - Status: ✅ Preloaded in Bootstrap

### Important (Loaded on Demand)

4. **WarrantIconLibrary**
   - Loaded in: `WarrantFusionUI.cs`
   - Impact: Medium - Used when viewing warrant fusion
   - Status: ✅ Preloaded in Bootstrap

5. **EmbossingEffect** (via Resources.LoadAll)
   - Loaded in: `EmbossingGridUI.cs`
   - Impact: Medium - Loaded when embossing grid is accessed
   - Status: ⚠️ Not preloaded (uses LoadAll, harder to preload)

## Changes Made

### 1. AssetPreloader Updates

**File**: `Assets/Scripts/SceneManagement/AssetPreloader.cs`

**Added to Preload List**:
- `CurrencyDatabase` (moved to top priority)
- `CardVisualAssets` (new)
- `WarrantIconLibrary` (new)
- `ItemDatabase` (already present, verified)
- `StatusDatabase` (new)
- `EffectsDatabase` (new)

**Priority Order**:
1. EquipmentScreen critical dependencies first
2. Warrant system databases
3. Affix databases
4. Other common databases

### 2. Code Updates to Use Preloader

**Files Updated**:
- ✅ `CurrencyManager.cs` - Uses preloader for CurrencyDatabase
- ✅ `CardCarouselUI.cs` - Uses preloader for CardVisualAssets
- ✅ `CardDisplay.cs` - Uses preloader for CardVisualAssets (2 locations)
- ✅ `WarrantFusionUI.cs` - Uses preloader for WarrantIconLibrary
- ✅ `LootManager.cs` - Uses preloader for CurrencyDatabase

**Pattern Used**:
```csharp
// Use preloader if available
if (AssetPreloader.Instance != null)
{
    asset = AssetPreloader.Instance.GetPreloadedAsset<AssetType>("AssetName");
}
if (asset == null)
{
    asset = Resources.Load<AssetType>("AssetName"); // Fallback
}
```

## Expected Performance Improvements

### Before Optimization:
- EquipmentScreen load: ~3-5 seconds
- Multiple Resources.Load calls blocking initialization
- Currency display delayed
- Card displays delayed

### After Optimization:
- EquipmentScreen load: ~1-2 seconds (estimated 40-60% reduction)
- Critical assets already loaded in Bootstrap
- Currency displays immediately
- Card displays ready instantly

## Remaining Optimizations

### 1. EmbossingEffect Preloading
**Challenge**: Uses `Resources.LoadAll<EmbossingEffect>(path)` which loads multiple assets
**Solution Options**:
- Create an `EmbossingDatabase` ScriptableObject that references all embossings
- Preload the database instead of individual assets
- Or: Load embossings in background after EquipmentScreen appears

### 2. AreaLootTable Preloading
**Location**: `AreaLootManager.cs`
**Impact**: Medium - Used when accessing loot tables
**Status**: Not yet preloaded

### 3. Additional Databases
Consider preloading:
- `ShopDatabase` (used in DialogueManager)
- `WarrantPackDatabase` (used in DialogueManager)
- `MonsterModifierDatabase` (used in combat)

## Testing Recommendations

1. **Profile EquipmentScreen Load**:
   - Use Unity Profiler to measure load time
   - Check for remaining Resources.Load calls
   - Verify assets are loaded from cache

2. **Test on Low-End Hardware**:
   - Measure actual load times
   - Verify smooth transitions
   - Check for frame spikes

3. **Verify Asset Caching**:
   - Check console logs for preload messages
   - Verify assets are found in preloader cache
   - Ensure fallback to Resources.Load works if needed

## Monitoring

### Console Logs to Watch:
```
[AssetPreloader] Starting asset preload...
[AssetPreloader] Preloaded: CurrencyDatabase
[AssetPreloader] Preloaded: CardVisualAssets
[AssetPreloader] ✅ Preload complete! Loaded X assets.
```

### Performance Metrics:
- EquipmentScreen load time (before/after)
- Number of Resources.Load calls during load
- Frame time during initialization

## Files Modified

### New/Modified Files:
- `Assets/Scripts/SceneManagement/AssetPreloader.cs` - Added critical assets
- `Assets/Scripts/UI/EquipmentScreen/UnityUI/CurrencyManager.cs` - Uses preloader
- `Assets/Scripts/UI/EquipmentScreen/UnityUI/CardCarouselUI.cs` - Uses preloader
- `Assets/Scripts/UI/EquipmentScreen/UnityUI/CardDisplay.cs` - Uses preloader
- `Assets/Scripts/Warrants/WarrantFusionUI.cs` - Uses preloader
- `Assets/Scripts/LootSystem/LootManager.cs` - Uses preloader

## Next Steps

1. **Test the improvements** - Verify EquipmentScreen loads faster
2. **Profile remaining bottlenecks** - Identify any remaining heavy loads
3. **Consider EmbossingEffect optimization** - Create database or defer loading
4. **Expand preloading** - Add more databases as needed

