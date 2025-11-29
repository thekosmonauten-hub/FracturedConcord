# Warrant Fusion in Town Scene - Setup Guide

## Quick Answer

**Yes, you can reuse the same `WarrantDatabase` asset**, but you'll need to create a new `WarrantLockerGrid` GameObject in the Town Scene. The `WarrantDatabase` is a ScriptableObject asset that can be shared, but `WarrantLockerGrid` is a MonoBehaviour component that needs to exist in each scene.

## Setup Options

### Option 1: Create WarrantLockerGrid in Town Scene (Recommended)

This gives you full drag-and-drop functionality:

1. **Create the Locker Grid:**
   - In Town Scene, create a new GameObject named `WarrantLockerGrid`
   - Add the `WarrantLockerGrid` component
   - Assign the same `WarrantDatabase` asset that you use in WarrantTree scene
   - Set up the grid container, item prefab, and scroll rect (same setup as WarrantTree)

2. **Wire Up Fusion UI:**
   - In `WarrantFusionUI` inspector, assign the `lockerGrid` field to your new `WarrantLockerGrid`
   - The fusion UI will auto-find it if not assigned, but it's better to assign it explicitly

3. **Inventory State:**
   - Each scene has its own inventory state (which warrants are in the locker)
   - If you want shared inventory across scenes, you'd need a persistent inventory system (future enhancement)

### Option 2: Use Without Visible Locker (Minimal Setup)

If you don't want a visible locker in Town Scene, the fusion will still work:

1. **Fusion UI Setup:**
   - Leave `lockerGrid` unassigned in `WarrantFusionUI`
   - Set `autoFindLockerGrid = false` if you don't want it to search
   - The fusion will still create the fused warrant
   - You'll just need to manually place warrants in slots (no drag-and-drop)

2. **Manual Warrant Selection:**
   - You could add a button to open a warrant selection panel
   - Or create warrants programmatically for testing

## Recommended Setup for Town Scene

### Step 1: Create WarrantLockerGrid

1. In Town Scene, create: `GameObject > UI > Panel` (or empty GameObject)
2. Name it: `WarrantLockerGrid`
3. Add component: `WarrantLockerGrid`
4. Configure:
   - `WarrantDatabase`: Assign the same asset from WarrantTree scene
   - `Grid Container`: Create a child GameObject with `GridLayoutGroup`
   - `Item Prefab`: Use the same prefab from WarrantTree scene
   - `Scroll Rect`: Optional, if you want scrolling

### Step 2: Wire Fusion UI

In `WarrantFusionUI` component:
- `warrantDatabase`: Assign the same WarrantDatabase asset
- `lockerGrid`: Assign your new WarrantLockerGrid GameObject
- `autoFindLockerGrid`: Can be true (will auto-find if not assigned)

### Step 3: Optional - Hide Locker UI

If you don't want the locker visible in Town Scene:
- Deactivate the locker panel GameObject
- Or set `autoPopulateFromDatabase = false` on WarrantLockerGrid
- The fusion UI can still reference it for adding results

## Code Changes Made

The fusion UI has been updated to:
- Auto-find `WarrantLockerGrid` if not assigned
- Work without a locker grid (with warnings)
- Handle missing locker gracefully

## Notes

- **WarrantDatabase (ScriptableObject)**: ✅ Can be shared across scenes
- **WarrantLockerGrid (MonoBehaviour)**: ❌ Needs separate instance per scene
- **Inventory State**: Currently per-scene (not persistent across scenes)

## Future Enhancement

If you want persistent warrant inventory across scenes, you could:
1. Create a `WarrantInventoryManager` singleton (DontDestroyOnLoad)
2. Store warrant inventory state there
3. Have each scene's `WarrantLockerGrid` sync with the manager

For now, the per-scene approach works fine for the fusion system.

