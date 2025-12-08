# Item Name Generation - Setup Guide 🎮

**Date:** December 5, 2025  
**Status:** ✅ **READY TO USE**

---

## 🚀 **Quick Setup (3 Steps)**

### **Step 1: Create NameGenerationData Asset**
1. Open Unity Editor
2. Go to menu: **`Dexiled → Create Name Generation Data`**
3. Asset created at: `Assets/Resources/NameGenerationData.asset`
4. ✅ Check console for confirmation message

---

### **Step 2: Assign to LootManager**
1. Find `LootManager` in your scene or prefab
2. Inspector → **Name Generation** section
3. Drag `NameGenerationData.asset` into the **Name Generation Data** field
4. ✅ Save scene/prefab

---

### **Step 3: Assign to AreaLootManager**
1. Find `AreaLootManager` in your scene or prefab
2. Inspector → **Name Generation** section
3. Drag `NameGenerationData.asset` into the **Name Generation Data** field
4. ✅ Save scene/prefab

---

### **Step 4: (Optional) Assign to AreaLootTable Assets**
If you have specific `AreaLootTable` ScriptableObject assets:
1. Navigate to your `AreaLootTable` assets in Project window
2. Select each one
3. Inspector → **Name Generation** section
4. Drag `NameGenerationData.asset` into the **Name Generation Data** field
5. ✅ Repeat for all `AreaLootTable` assets

**Note:** If `AreaLootManager` has the field assigned, it will propagate to tables automatically in most cases.

---

## 📊 **Both Systems Integrated**

### **LootManager** (Legacy System)
- Handles basic loot generation
- Creates items from `ItemData`
- ✅ **Name generation integrated**

### **AreaLootManager + AreaLootTable** (Modern System)
- Area-based loot with level scaling
- Generates items based on area level
- ✅ **Name generation integrated**

Both systems will now generate proper Magic and Rare item names! 🎉

---

## 🧪 **Testing**

### **Test with LootManager:**
1. Use `SimpleItemGenerator` (if available)
2. OR play encounters and collect drops
3. Check console for: `[LootManager] Generated name for Magic item: '...'`

### **Test with AreaLootManager:**
1. Play encounters in areas with `AreaLootTable` configured
2. Collect item drops
3. Check console for: `[AreaLoot] Generated name for Magic item: '...'`

### **Verify in Tooltips:**
1. Hover over Magic or Rare items in Equipment Screen
2. Tooltip should show generated name:
   - **Magic:** `"Flaming Iron Sword of Dexterity"`
   - **Rare:** `"Forsaken Edge"`

---

## 📝 **Name Examples**

### **Magic Items (Affix-Based):**
```
Tainted Worn Hatchet of the Cat
Flaming Iron Sword
Rusty Axe of Dexterity
Iron Sword of the Bear
Frozen Mace
```

### **Rare Items (Random 2-Word):**
```
Forsaken Edge       (weapon)
Crimson Grasp       (gloves)
Twilight Stride     (boots)
Obsidian Crown      (helmet)
Phantom Bulwark     (shield)
Umbral Spiral       (ring)
Eldritch Pendant    (amulet)
```

---

## ⚙️ **Systems Overview**

### **Where Name Generation Happens:**

**LootManager.cs:**
```csharp
private void GenerateItemName(BaseItem item)
{
    if (item.rarity == ItemRarity.Magic || item.rarity == ItemRarity.Rare)
    {
        item.generatedName = ItemNameGenerator.GenerateItemName(item, nameGenerationData);
    }
}
```
- Called in: `CreateItemFromData()` after affixes are applied

**AreaLootTable.cs:**
```csharp
private void GenerateItemName(BaseItem item)
{
    if (item.rarity == ItemRarity.Magic || item.rarity == ItemRarity.Rare)
    {
        item.generatedName = ItemNameGenerator.GenerateItemName(item, nameGenerationData);
    }
}
```
- Called in: `ApplyRandomAffixes()` after affixes are generated
- Called in: `ApplyRandomAffixes(forcedRarity)` after affixes are generated

---

## 🔍 **Troubleshooting**

### **No Names Generated:**
- ✅ Check `NameGenerationData.asset` exists in `Assets/Resources/`
- ✅ Check `LootManager` has `nameGenerationData` assigned
- ✅ Check `AreaLootManager` has `nameGenerationData` assigned
- ✅ Check `AreaLootTable` assets have `nameGenerationData` assigned (if using specific tables)

### **Still Showing Old Format:**
- ✅ Ensure item rarity is Magic or Rare (not Normal or Unique)
- ✅ Check console for `"Generated name for..."` messages
- ✅ Try generating new items (old items may not have generated names)

### **Console Warnings:**
```
[ItemNameGenerator] NameGenerationData is null. Using fallback name.
```
- **Fix:** Assign `NameGenerationData.asset` to the manager that's generating items

---

## 📈 **Statistics**

- **Total Rare Prefixes:** 101
- **Total Suffixes per Category:** ~20 each
- **Total Categories:** 11 (weapons, helmets, armour, etc.)
- **Possible Rare Names:** **~2,020 unique names per item category!**

---

## ✅ **Checklist**

- ☐ Run `Dexiled → Create Name Generation Data` menu
- ☐ Assign to `LootManager`
- ☐ Assign to `AreaLootManager`
- ☐ (Optional) Assign to individual `AreaLootTable` assets
- ☐ Test with item generation
- ☐ Verify names appear in tooltips
- ☐ Enjoy ARPG-style item names! 🎮

---

**No linter errors!** 🎯

Both loot systems now support item name generation! 🚀

