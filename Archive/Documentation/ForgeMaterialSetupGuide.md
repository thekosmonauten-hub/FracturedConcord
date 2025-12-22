# Forge Material Setup Guide

## Overview
Forge materials use a database system similar to currencies. You need to:
1. Create a `ForgeMaterialDatabase` ScriptableObject
2. Assign sprites to each material type
3. Add `ForgeMaterialDisplayItem` component to your prefab
4. Wire up the references

---

## Step 1: Create ForgeMaterialDatabase

1. **Right-click** in Project window (e.g., `Assets/Resources/` folder)
2. Select **Create > Dexiled > Forge > Material Database**
3. Name it: `ForgeMaterialDatabase`
4. **IMPORTANT**: Place it in `Assets/Resources/` folder (or it won't be found at runtime)

---

## Step 2: Assign Sprites to Materials

1. Select the **ForgeMaterialDatabase** asset
2. In Inspector, you'll see **"Material Definitions"** list
3. Click **"+"** to add entries for each material type:
   - **Weapon Scraps** (WeaponScraps)
   - **Armour Scraps** (ArmourScraps)
   - **Effigy Splinters** (EffigySplinters)
   - **Warrant Shards** (WarrantShards)

4. For each entry:
   - **Material Type**: Select from dropdown (WeaponScraps, ArmourScraps, etc.)
   - **Display Name**: Enter display name (e.g., "Weapon Scraps")
   - **Description**: Enter description (optional, for tooltips)
   - **Material Sprite**: **Drag your sprite here** ⭐
   - **Display Color**: Optional tint color

5. **Alternative**: Right-click the database and select **"Initialize Default Materials"** to auto-create entries (then assign sprites manually)

---

## Step 3: Setup ForgeMaterial Prefab

### Option A: Using Your Existing Prefab

1. Select your **ForgeMaterial.prefab**
2. Add **`ForgeMaterialDisplayItem`** component to the root GameObject
3. In Inspector, find the component
4. **Right-click the component** → Select **"Auto-Assign References"**
   - This will try to find IconImage, NameText, CountText, and Background automatically
5. **Manually assign** any missing references:
   - **Material Icon**: Drag the `IconImage` GameObject (or `Content/IconImage`)
   - **Material Name Text**: Drag the `NameText` GameObject (or `Content/NameText`)
   - **Material Count Text**: Drag the `CountText` GameObject (or `Content/CountText`)
   - **Background Image**: Drag the `Background` GameObject (optional)

### Option B: Verify Prefab Structure

Your prefab should have this structure:
```
ForgeMaterial (root)
├── Background (Image)
└── Content (HorizontalLayoutGroup)
    ├── IconImage (Image) ← Material sprite goes here
    ├── NameText (TextMeshProUGUI) ← Material name
    └── CountText (TextMeshProUGUI) ← Quantity
```

---

## Step 4: Assign Prefab to ForgeMaterialDisplayUI

1. Select the GameObject with **`ForgeMaterialDisplayUI`** component
2. In Inspector, find **"Material Entry Prefab"** field
3. **Drag your `ForgeMaterial.prefab`** into this field

---

## Step 5: Test

1. **Play the scene**
2. Materials should display with:
   - ✅ Icons (if sprites assigned)
   - ✅ Names
   - ✅ Quantities

---

## Troubleshooting

### "Prefabs are not resolved"
- Make sure `ForgeMaterialDisplayItem` component is on the prefab
- Use **"Auto-Assign References"** context menu on the component
- Manually assign any missing UI references

### "No sprites showing"
- Check that `ForgeMaterialDatabase` is in `Resources/` folder
- Verify sprites are assigned in the database
- Check that `IconImage` reference is assigned in `ForgeMaterialDisplayItem`

### "Database not found"
- Ensure database is at: `Assets/Resources/ForgeMaterialDatabase.asset`
- Or create it and place it in Resources folder

---

## Quick Reference: Prefab Structure

```
ForgeMaterial (GameObject)
├── ForgeMaterialDisplayItem (Component) ← Add this!
│   ├── Material Icon: IconImage
│   ├── Material Name Text: NameText  
│   ├── Material Count Text: CountText
│   └── Background Image: Background
├── Background (Image)
└── Content (HorizontalLayoutGroup)
    ├── IconImage (Image)
    ├── NameText (TextMeshProUGUI)
    └── CountText (TextMeshProUGUI)
```

---

## Notes

- **You don't need to create currencies** - materials are separate from the currency system
- The database automatically creates default entries if missing
- Sprites can be assigned at any time - the UI will update when you refresh
- The `ForgeMaterialDisplayItem` component handles all the display logic automatically

