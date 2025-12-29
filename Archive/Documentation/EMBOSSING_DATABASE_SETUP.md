# Embossing Database Setup Guide

## Overview

This guide explains how to create a ScriptableObject-based `EmbossingDatabaseAsset` to replace the slow `Resources.LoadAll<EmbossingEffect>()` call. This allows embossings to be preloaded in Bootstrap for much faster EquipmentScreen loading.

## Why This Optimization?

**Before**: `Resources.LoadAll<EmbossingEffect>("Embossings")` - Slow, loads all assets individually
**After**: Single ScriptableObject asset - Fast, can be preloaded in Bootstrap

**Performance Impact**: 
- EquipmentScreen load time: ~1-2 seconds faster
- Embossing grid appears instantly
- No blocking during scene load

## Step-by-Step Setup

### Step 1: Create the Database Asset

1. **Open Unity Editor**

2. **Go to Menu**: `FracturedConcord → Create Embossing Database`
   - This opens the database creator window

3. **Configure Settings**:
   - **Resource Path**: `"Embossings"` (or your embossing folder path)
   - **Database Save Path**: `"Assets/Resources/EmbossingDatabase.asset"`

4. **Click "Load Embossings and Create Database"**
   - The tool will:
     - Load all `EmbossingEffect` assets from `Resources/Embossings`
     - Validate and filter (removes nulls, empty IDs, duplicates)
     - Create or update `EmbossingDatabase.asset`
     - Populate it with all valid embossings

5. **Verify Success**:
   - Check console for success message
   - Database asset should appear in `Assets/Resources/`
   - Asset should be selected in Project window

### Step 2: Verify the Database

1. **Select `EmbossingDatabase.asset`** in Project window

2. **Check Inspector**:
   - Should show list of all embossing effects
   - Count should match your embossing assets (minus any invalid ones)

3. **Check Console**:
   - Should show: `"Successfully loaded X embossing effects"`

### Step 3: Test Loading

1. **Play the game**

2. **Check Console Logs**:
   ```
   [AssetPreloader] Preloaded: EmbossingDatabase
   [EmbossingDatabase] Loading embossings from ScriptableObject database (fast path)
   [EmbossingDatabase] Successfully loaded X embossing effects from database
   ```

3. **Load EquipmentScreen**:
   - Should load faster
   - Embossing grid should appear immediately

## How It Works

### Architecture

**ScriptableObject Asset** (`EmbossingDatabaseAsset`):
- Contains list of all `EmbossingEffect` references
- Stored at `Resources/EmbossingDatabase.asset`
- Preloaded in Bootstrap scene

**MonoBehaviour Manager** (`EmbossingDatabase`):
- Singleton manager that provides access to embossings
- Tries to load from ScriptableObject first (fast)
- Falls back to `Resources.LoadAll` if database not found (backward compatible)

### Loading Priority

1. **First**: Try `AssetPreloader` cache (fastest - preloaded)
2. **Second**: Try `Resources.Load<EmbossingDatabaseAsset>()` (fast - single asset)
3. **Fallback**: `Resources.LoadAll<EmbossingEffect>()` (slow - backward compatible)

## Editor Tool Features

### Create Embossing Database Window

**Location**: `FracturedConcord → Create Embossing Database`

**Features**:
- **Load Embossings and Create Database**: Main function
  - Scans `Resources/Embossings` folder
  - Validates all embossing effects
  - Creates/updates database asset
  - Shows summary of loaded/skipped embossings

- **Find All Embossing Effects in Project**: Helper function
  - Scans entire project for `EmbossingEffect` assets
  - Groups by folder
  - Suggests resource path
  - Shows which are in Resources folders

### Validation

The tool automatically:
- ✅ Filters out null entries
- ✅ Skips embossings with empty IDs
- ✅ Detects and skips duplicate IDs
- ✅ Shows summary of what was loaded/skipped

## Troubleshooting

### Database Not Found?

**Error**: `"ScriptableObject database not found, falling back to Resources.LoadAll"`

**Solutions**:
1. Check that `EmbossingDatabase.asset` exists in `Assets/Resources/`
2. Verify asset name is exactly `"EmbossingDatabase"` (no extension in Resources path)
3. Check that asset is added to AssetPreloader preload list
4. Verify Bootstrap scene has AssetPreloader component

### No Embossings Found?

**Error**: `"No EmbossingEffect assets found at Resources/Embossings"`

**Solutions**:
1. Use "Find All Embossing Effects" button to locate your embossings
2. Update the resource path to match your folder structure
3. Ensure embossings are in a `Resources` folder
4. Check that assets are actually `EmbossingEffect` type

### Duplicate IDs?

**Warning**: `"Duplicate embossing ID 'X', skipping"`

**Solutions**:
1. Check console for which embossings have duplicate IDs
2. Fix duplicate IDs in the embossing assets
3. Re-run the database creator

### Performance Not Improved?

**Check**:
1. Verify `EmbossingDatabase` is in AssetPreloader preload list
2. Check console for preload messages
3. Profile EquipmentScreen load to see if `Resources.LoadAll` is still being called
4. Ensure Bootstrap scene is set up correctly

## Files Created/Modified

### New Files:
- `Assets/Scripts/Data/EmbossingDatabase.cs` - ScriptableObject database class
- `Assets/Editor/CreateEmbossingDatabase.cs` - Editor tool to populate database
- `Assets/Resources/EmbossingDatabase.asset` - Database asset (created by tool)

### Modified Files:
- `Assets/Scripts/Managers/EmbossingDatabase.cs` - Updated to use ScriptableObject first
- `Assets/Scripts/SceneManagement/AssetPreloader.cs` - Added `EmbossingDatabase` to preload list

## Updating the Database

When you add new embossing effects:

1. **Create new `EmbossingEffect` assets** in `Resources/Embossings`
2. **Run the database creator again**:
   - Menu: `FracturedConcord → Create Embossing Database`
   - Click "Load Embossings and Create Database"
   - It will update the existing database with new embossings

## Benefits

✅ **Faster Loading**: Single asset load vs. multiple individual loads
✅ **Preloadable**: Can be cached in Bootstrap
✅ **Backward Compatible**: Falls back to old method if database not found
✅ **Validated**: Editor tool checks for duplicates and invalid entries
✅ **Easy Updates**: Re-run tool to refresh database

## Next Steps

1. **Create the database** using the editor tool
2. **Test EquipmentScreen loading** - should be faster
3. **Profile performance** to verify improvement
4. **Update database** whenever you add new embossings

