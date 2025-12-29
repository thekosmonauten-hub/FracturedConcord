# Finding EffigyGridUI Grid Prefab Field

## The Field Exists!

The `EffigyGridUI` component **does have** a `Grid Prefab` field. Here's how to find it:

## Step-by-Step: Finding EffigyGridUI Grid Prefab Field

### Step 1: Locate EffigyGrid GameObject

1. **Open EquipmentScreen scene**
2. **In Hierarchy**, search for:
   - "Effigy" or "EffigyGrid"
   - Look for GameObject with `EffigyGridUI` component
3. **Select the GameObject**

### Step 2: Find the Component

1. **In Inspector**, scroll to find `EffigyGridUI` component
2. **Expand the component** (click the arrow if collapsed)
3. **Look for "References" section** (Header)

### Step 3: Find Grid Prefab Field

In the **References** section, you should see:

```
[References]
├── Cell Prefab (GameObject) ← For individual cells (old method)
├── Grid Prefab (GameObject) ← THIS IS WHAT YOU NEED! ✅
├── Grid Container (Transform)
└── Effigy Storage (EffigyStorageUI)
```

**Field Name**: `Grid Prefab`  
**Type**: GameObject  
**Tooltip**: "Complete grid prefab with all cells already set up (FASTER - recommended)"

---

## If You Still Can't See It

### Solution 1: Reimport the Script

1. **In Project window**, find: `Assets/Scripts/UI/EquipmentScreen/UnityUI/EffigyGridUI.cs`
2. **Right-click** the file
3. **Select**: "Reimport"
4. **Wait** for Unity to recompile
5. **Check Inspector again**

### Solution 2: Check Script Compilation

1. **Check Console** for compilation errors
2. **Fix any errors** in `EffigyGridUI.cs`
3. **Wait** for Unity to recompile
4. **Check Inspector again**

### Solution 3: Verify Field Exists in Code

The field should be at **line 28** in `EffigyGridUI.cs`:

```csharp
[Tooltip("Complete grid prefab with all cells already set up (FASTER - recommended)")]
[SerializeField] private GameObject gridPrefab;
```

If this line exists, the field **will** appear in Inspector.

### Solution 4: Check Inspector Mode

1. **In Inspector**, check the **3-dot menu** (top right)
2. **Make sure** "Normal" mode is selected (not Debug)
3. **Try collapsing and expanding** the component

---

## Visual Guide

### What You Should See in Inspector:

```
EffigyGridUI (Script)
├── Grid Settings
│   ├── Cell Size: 60
│   └── Cell Spacing: 2
│
├── References  ← LOOK HERE!
│   ├── Cell Prefab: [Empty or assigned]
│   ├── Grid Prefab: [Empty] ← ASSIGN HERE! ✅
│   ├── Grid Container: [Assigned]
│   └── Effigy Storage: [Assigned]
│
└── Colors
    ├── Empty Cell Color
    └── Valid Placement Color
```

---

## Quick Assignment Steps

1. **Select EffigyGrid GameObject** in Hierarchy
2. **In Inspector**, find `EffigyGridUI` component
3. **Expand "References" section**
4. **Find "Grid Prefab" field** (should be second field)
5. **Drag** `EffigyGridPrefab_6x20.prefab` to the field
6. **Leave "Cell Prefab" empty** (not needed with grid prefab)

---

## Verification

After assigning:

1. **Check Console** when scene loads
2. **Should see**: `"[EffigyGridUI] Loaded 120 cells from prefab grid (6x20) - FAST PATH"`
3. **Should NOT see**: `"[EffigyGridUI] Generated ... progressively - SLOW PATH"`

If you see "FAST PATH", it's working!

---

## Still Having Issues?

If the field truly doesn't appear:

1. **Check Unity version** - Make sure you're using a recent version
2. **Check for script errors** - Fix any compilation errors
3. **Try restarting Unity** - Sometimes Inspector needs a refresh
4. **Check if field is private** - It should be `[SerializeField]` (which it is)

The field **definitely exists** in the code, so it should appear in Inspector. If it doesn't, it's likely a Unity Inspector refresh issue.

