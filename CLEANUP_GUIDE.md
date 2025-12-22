# Project Cleanup Guide

This guide helps you safely clean up and restructure your Unity project without breaking anything.

## ⚠️ IMPORTANT: Before You Start

1. **Commit your current work to git** - Make sure everything is saved and committed
2. **Create a backup** - Better safe than sorry!
3. **Test your game** - Make sure everything works before cleanup
4. **Work incrementally** - Clean up one category at a time and test

---

## 🗑️ Safe to Delete (High Confidence)

### 1. `Assets/_Recovery/` Folder
**Status**: ✅ **SAFE TO DELETE**
- These are Unity's auto-generated recovery scene files
- Created when Unity crashes or encounters errors
- Not referenced anywhere in your code
- Contains 25 duplicate scene files (0.unity, 0 (1).unity, etc.)

**Action**: Delete the entire `Assets/_Recovery/` folder

### 2. `Assets/AI/` Folder
**Status**: ✅ **SAFE TO DELETE**
- Currently empty
- No references found in codebase

**Action**: Delete the `Assets/AI/` folder

### 3. LeanTween Example/Test Files
**Status**: ✅ **SAFE TO DELETE** (if you're not learning from them)
- `Assets/LeanTween/Examples/` - Demo scenes and scripts
- `Assets/LeanTween/Testing/` - Test files
- `Assets/LeanTween/Documentation/` - HTML docs (if you prefer online docs)

**Action**: Keep the `Framework/` folder, delete Examples/Testing/Documentation if not needed

---

## ⚠️ Review Before Deleting (Medium Confidence)

### 1. Unused Scenes
These scenes are NOT in your build settings:

- `Assets/Scenes/EquipmentScreen_Old.unity` - Old version?
- `Assets/Scenes/EquipmentScreen_New.unity` - Check if this is used
- `Assets/Scenes/FxScene.unity` - Test scene?
- `Assets/Scenes/cardPrefabCreationScene.unity` - Utility scene?
- `Assets/Scenes/StatPanelRuntimeTest.unity` - Test scene
- `Assets/Scenes/MazeHub.unity` - Not in build settings

**Action**: 
1. Open each scene in Unity
2. Check if they're referenced by scripts
3. Delete if truly unused

### 2. External Assets
`Assets/00 External Assets/` contains:
- UI Layout Grid
- UnityChromakey-master

**Action**: 
1. Search for references: `grep -r "UI Layout Grid\|UnityChromakey" Assets/Scripts/`
2. If no references found, check if used in scenes
3. Delete if unused

---

## 🔍 How to Verify Assets Are Unused

### Method 1: Unity's Built-in Tools
1. Right-click on an asset in Project window
2. Select "Find References in Scene" (if in a scene)
3. Or use "Select Dependencies" to see what uses it

### Method 2: Use the Cleanup Helper Tool
1. Open Unity Editor
2. Go to `Tools > Cleanup Helper`
3. Click "Scan for Unused Scenes" and "Find Empty Folders"
4. Review the results

### Method 3: Search in Code
```bash
# Search for asset references in scripts
grep -r "AssetName" Assets/Scripts/
```

---

## 📁 Suggested Folder Structure

After cleanup, consider organizing like this:

```
Assets/
├── Art/                    # All art assets
│   ├── Sprites/
│   ├── Textures/
│   └── Materials/
├── Audio/                  # Sound effects and music
├── Prefabs/                # All prefabs
├── Scenes/                  # Game scenes
├── Scripts/                 # All C# scripts
│   ├── Core/               # Core game systems
│   ├── UI/                 # UI scripts
│   ├── Combat/             # Combat system
│   └── ...
├── Resources/              # Resources folder (keep organized)
├── ScriptableObjects/       # ScriptableObject assets
├── Settings/               # Project settings
└── ThirdParty/             # External plugins/assets
    ├── LeanTween/
    └── TextMesh Pro/
```

---

## 🛠️ Step-by-Step Cleanup Process

### Phase 1: Safe Deletions (Do First)
1. ✅ Delete `Assets/_Recovery/`
2. ✅ Delete `Assets/AI/` (empty folder)
3. ✅ Delete `Assets/LeanTween/Examples/` (if not needed)
4. ✅ Delete `Assets/LeanTween/Testing/` (if not needed)

### Phase 2: Review and Delete
1. 🔍 Review unused scenes (use Cleanup Helper tool)
2. 🔍 Check external assets for references
3. 🔍 Delete confirmed unused items

### Phase 3: Reorganization (Optional)
1. Create new folder structure
2. Move assets to appropriate folders
3. Unity will update all references automatically
4. Test thoroughly after moving

---

## ✅ After Cleanup Checklist

- [ ] Game builds successfully
- [ ] All scenes load correctly
- [ ] No missing reference errors in console
- [ ] All UI elements work
- [ ] Combat system works
- [ ] Save/load system works
- [ ] Commit changes to git

---

## 🆘 If Something Breaks

1. **Don't panic!** Unity keeps references by GUID, so moving files is usually safe
2. Check the Console for missing reference errors
3. Use `git checkout` to restore deleted files if needed
4. Re-import assets if references break: `Assets > Reimport All`

---

## 💡 Pro Tips

1. **Use Unity's "Find References"** - Right-click asset > Find References
2. **Check .meta files** - They contain GUID references Unity uses
3. **Test incrementally** - Don't delete everything at once
4. **Keep a log** - Note what you delete in case you need to restore
5. **Use version control** - Commit after each successful cleanup phase

