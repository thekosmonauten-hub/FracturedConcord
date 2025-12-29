# Bootstrap Scene - Quick Setup Guide

## 🚀 Quick Start (15 minutes)

Follow these steps to set up a Bootstrap scene for maximum performance.

---

## Step 1: Create Bootstrap Scene

1. **File → New Scene → Basic (Scene)**
2. **Save as**: `Assets/Scenes/BootstrapScene.unity`
3. **Keep it empty** (just managers, no UI or game objects)

---

## Step 2: Add BootstrapManager

1. **In BootstrapScene**:
   - Right-click Hierarchy → Create Empty
   - Name: `BootstrapManager`
   - Add Component → `BootstrapManager`
   - Set **Initial Scene Name** to `"MainMenu"` (or your starting scene)

---

## Step 3: Add Core Managers (Auto-Create Method - Recommended)

Most managers auto-create themselves, but you can manually add them for explicit control:

**Option A: Let Managers Auto-Create** ✅ (Easiest)
- Managers will create themselves when first accessed
- Just ensure the scripts exist in your project
- They'll be created automatically with `DontDestroyOnLoad`

**Option B: Manual Setup** (More Control)
- Create empty GameObjects for each manager
- Add the manager component
- Configure settings

**Managers to Consider Adding:**
- `AssetPreloader` (recommended - add this one)
- `SceneInitializationManager` (recommended - add this one)
- `TransitionManager` (if you want it in Bootstrap)
- `CharacterManager` (auto-creates)
- `EquipmentManager` (auto-creates)
- `EncounterManager` (auto-creates)
- Others as needed

---

## Step 4: Update Build Settings

1. **File → Build Settings**
2. **Drag `BootstrapScene.unity` to the TOP** (index 0)
3. **Order should be**:
   ```
   0. BootstrapScene ← MUST BE FIRST!
   1. MainMenu
   2. CharacterCreation
   3. MainGameUI
   ... (other scenes)
   ```
4. **Save Build Settings**

---

## Step 5: Test

1. **Play the game**
2. **Check Console**:
   ```
   [BootstrapManager] Starting bootstrap initialization...
   [AssetPreloader] Starting asset preload...
   [BootstrapManager] Bootstrap ready, loading initial scene: MainMenu
   [TransitionManager] Loading scene additively: MainMenu
   ```
3. **Navigate between scenes** - should be faster!

---

## ✅ What You Get

- **Faster Scene Loads**: Only content scenes load, not managers
- **Persistent Managers**: No recreation overhead
- **Smoother Transitions**: Additive loading prevents freezing
- **Better Performance**: Especially on low-end PCs

---

## 🔧 Troubleshooting

### Bootstrap Not Loading First?
- Check Build Settings - BootstrapScene must be at index 0
- Verify scene is enabled in Build Settings

### Managers Being Destroyed?
- Managers should use `DontDestroyOnLoad` (most already do)
- Check that managers are in Bootstrap or auto-create properly

### Still Using Single Scene Loading?
- `TransitionManager` automatically detects Bootstrap and uses additive loading
- If Bootstrap isn't detected, it falls back to single loading (backward compatible)

---

## 📋 Setup Checklist

- [ ] Created `BootstrapScene.unity`
- [ ] Added `BootstrapManager` component
- [ ] Set initial scene name in BootstrapManager
- [ ] Added `AssetPreloader` to Bootstrap scene (recommended)
- [ ] Added `SceneInitializationManager` to Bootstrap scene (recommended)
- [ ] Set BootstrapScene as first in Build Settings
- [ ] Tested scene transitions
- [ ] Verified managers persist
- [ ] Checked performance improvements

---

## 🎯 Expected Results

**Before Bootstrap:**
- Scene load: 2-5 seconds
- Managers recreated each time
- Heavy initialization on transitions

**After Bootstrap:**
- Scene load: 0.5-2 seconds
- Managers persist (no recreation)
- Minimal initialization overhead

---

## 📚 Full Documentation

See `BOOTSTRAP_SCENE_SETUP_GUIDE.md` for:
- Detailed architecture explanation
- Manual manager setup
- Advanced configuration
- UI separation pattern
- Complete migration guide

