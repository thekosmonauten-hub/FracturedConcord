# Item Name Generation System - COMPLETE ✅

**Date:** December 5, 2025  
**Status:** ✅ **READY TO USE**

---

## 🎯 **What Was Implemented**

A comprehensive item name generation system that mimics classic ARPGs:

### **Magic Items (Blue) - Affix-Based Names**
Format: `"[Prefix] BaseName [of Suffix]"`

**Examples:**
- `"Tainted Worn Hatchet of the Cat"`
- `"Flaming Iron Sword"`
- `"Rusty Axe of Dexterity"`

**How it works:**
- Uses the actual affix names from the item
- Prefix appears before base name
- Suffix appears after with "of" prefix
- If only one affix, shows that one
- Example: `"+15% Fire Damage"` prefix named "Flaming" → `"Flaming Iron Sword"`

---

### **Rare Items (Gold) - Random 2-Word Names**
Format: `"[RandomPrefix] [RandomSuffix]"`

**Examples:**
- `"Forsaken Edge"` (weapon)
- `"Crimson Grasp"` (gloves)
- `"Twilight Stride"` (boots)
- `"Obsidian Crown"` (helmet)

**How it works:**
- Picks random prefix from 100+ word pool
- Picks random suffix from category-specific pool
- Suffix pool matches item type (weapons get weapon suffixes, gloves get glove suffixes, etc.)
- Results in snappy, evocative names like Path of Exile/Diablo

---

## 📁 **Files Created**

### **1. NameGenerationData.cs**
`Assets/Scripts/Data/Items/NameGenerationData.cs`

ScriptableObject that stores:
- 100+ rare prefixes (Abyssal, Forsaken, Crimson, etc.)
- Category-specific suffix pools:
  - Weapon Melee (Edge, Cleaver, Fang, etc.)
  - Weapon Ranged (Flight, Arrow, Piercer, etc.)
  - Weapon Caster (Focus, Channel, Spirit, etc.)
  - Helmets (Crown, Visage, Mask, etc.)
  - Body Armour (Cloak, Mail, Shell, etc.)
  - Gloves (Grasp, Clutch, Talons, etc.)
  - Boots (Stride, March, Tread, etc.)
  - Belts (Girdle, Chain, Bind, etc.)
  - Amulets (Charm, Pendant, Sigil, etc.)
  - Rings (Band, Loop, Circle, etc.)
  - Shields (Bulwark, Aegis, Ward, etc.)

**Methods:**
- `GetSuffixPoolForItem(BaseItem)` - Returns appropriate suffix list for item type

---

### **2. ItemNameGenerator.cs**
`Assets/Scripts/Data/Items/ItemNameGenerator.cs`

Static utility class that generates names:

**Methods:**
- `GenerateItemName(BaseItem, NameGenerationData)` - Main entry point
- `GenerateMagicItemName(BaseItem)` - Creates affix-based names
- `GenerateRareItemName(BaseItem, NameGenerationData)` - Creates random 2-word names
- `GenerateItemName(BaseItem, NameGenerationData, int seed)` - Seeded version for reproducibility

**Logic:**
```csharp
switch (item.rarity)
{
    case ItemRarity.Normal:
        return item.itemName; // "Iron Sword"
    
    case ItemRarity.Magic:
        return GenerateMagicItemName(item); // "Flaming Iron Sword of Dexterity"
    
    case ItemRarity.Rare:
        return GenerateRareItemName(item, nameData); // "Forsaken Edge"
    
    case ItemRarity.Unique:
        return item.itemName; // Use predefined unique name
}
```

---

### **3. CreateNameGenerationData.cs** (Editor Tool)
`Assets/Editor/CreateNameGenerationData.cs`

Editor utility to create the ScriptableObject asset pre-populated with all the names from your documentation.

**Usage:**
1. Open Unity
2. Click `Dexiled → Create Name Generation Data`
3. Asset created at: `Assets/Resources/NameGenerationData.asset`
4. Automatically populated with 100+ prefixes and all suffix pools

---

## 🔧 **Integration Points**

### **1. BaseItem.cs - Added Generated Name Field**

```csharp
[Header("Generated Name (Runtime)")]
[Tooltip("Auto-generated name for Magic/Rare items. Set at runtime.")]
public string generatedName = "";
```

**Updated `GetDisplayName()` method:**
```csharp
public virtual string GetDisplayName()
{
    // Use generated name if available (Magic/Rare items)
    if (!string.IsNullOrEmpty(generatedName))
    {
        return generatedName;
    }
    
    // Fallback to legacy format for items without generated names
    string qualityPrefix = quality > 0 ? $"Superior " : "";
    string rarityName = GetRarityName();
    string rarityPrefix = rarity != ItemRarity.Normal ? $"{rarityName} " : "";
    return rarityPrefix + qualityPrefix + itemName;
}
```

---

### **2. LootManager.cs - Auto-Generate Names**

**Added Fields:**
```csharp
[Header("Name Generation")]
[Tooltip("Data for generating Magic and Rare item names")]
public NameGenerationData nameGenerationData;
```

**Added Method:**
```csharp
private void GenerateItemName(BaseItem item)
{
    if (item == null) return;
    
    // Only generate names for Magic and Rare items
    if (item.rarity == ItemRarity.Magic || item.rarity == ItemRarity.Rare)
    {
        item.generatedName = ItemNameGenerator.GenerateItemName(item, nameGenerationData);
        Debug.Log($"[LootManager] Generated name for {item.rarity} item: '{item.generatedName}'");
    }
}
```

**Called after item creation:**
Items automatically get names when created by `LootManager`.

---

### **3. Tooltips - Display Generated Names**

Tooltips already call `item.GetDisplayName()`, so generated names appear automatically:
- `WeaponTooltipView.cs` ✅
- `EquipmentTooltipView.cs` ✅

No changes needed - the system is plug-and-play!

---

## 🎮 **Setup Instructions**

### **Step 1: Create NameGenerationData Asset**
1. Open Unity Editor
2. Go to menu: `Dexiled → Create Name Generation Data`
3. Asset created at: `Assets/Resources/NameGenerationData.asset`
4. Check console for confirmation:
   ```
   [NameGeneration] Created NameGenerationData asset
   [NameGeneration] Loaded 101 prefixes
   [NameGeneration] Ready to generate Magic and Rare item names!
   ```

---

### **Step 2: Assign to LootManager**
1. Find `LootManager` in your scene or prefab
2. Inspector → `Name Generation` section
3. Drag `NameGenerationData.asset` into the `Name Generation Data` field

---

### **Step 3: Test!**
1. Generate a Magic or Rare item (use `SimpleItemGenerator` or play encounters)
2. Check console for:
   ```
   [LootManager] Generated name for Magic item: 'Flaming Iron Sword of Dexterity'
   [LootManager] Generated name for Rare item: 'Forsaken Edge'
   ```
3. Hover item in Equipment Screen → Tooltip shows generated name!

---

## 📊 **Name Examples**

### **Magic Items (1-2 Affixes):**
```
Flaming Iron Sword
Rusty Axe of Dexterity
Tainted Worn Hatchet of the Cat
Iron Sword of Fire
Heavy Mace
```

### **Rare Items (3-6 Affixes):**
```
Forsaken Edge       (weapon)
Crimson Grasp       (gloves)
Twilight Stride     (boots)
Obsidian Crown      (helmet)
Phantom Bulwark     (shield)
Umbral Spiral       (ring)
Eldritch Pendant    (amulet)
Stormborn Mail      (body armour)
Voidforged Breaker  (weapon)
Nightfall Visage    (helmet)
```

---

## 🔍 **How It Works - Flow**

### **Item Creation:**
```
1. LootManager.CreateItemFromData()
   ├─ Creates BaseItem instance
   ├─ Adds affixes (via AffixDatabase)
   ├─ Sets rarity (Normal/Magic/Rare/Unique)
   └─ Calls GenerateItemName()
      ├─ If Normal → Use base name
      ├─ If Magic → Use affix names
      ├─ If Rare → Generate random 2-word name
      └─ Sets item.generatedName
```

### **Tooltip Display:**
```
1. Player hovers item
2. WeaponTooltipView.SetData(item)
3. nameLabel.text = item.GetDisplayName()
   └─ Returns item.generatedName (if set)
   └─ Otherwise returns legacy format
```

---

## ⚙️ **Customization**

### **Add More Prefixes:**
1. Open `NameGenerationData.asset`
2. Inspector → `Rare Prefixes` list
3. Add new words (e.g., "Ethereal", "Spectral", "Draconic")

### **Add More Suffixes:**
1. Open `NameGenerationData.asset`
2. Find the appropriate suffix pool (e.g., `Weapon Melee Suffixes`)
3. Add new words (e.g., "Slayer", "Decimator", "Ruiner")

### **Create Custom Suffix Pool:**
1. Edit `NameGenerationData.cs`
2. Add new `List<string>` field
3. Update `GetSuffixPoolForItem()` to return it for specific item types

---

## 🐛 **Troubleshooting**

### **Items Show Old Format Names:**
- ✅ Check `LootManager` has `nameGenerationData` assigned
- ✅ Check console for `"Generated name for..."` messages
- ✅ Verify item rarity is Magic or Rare (not Normal)

### **Names Are "Rare Iron Sword" Instead of Random:**
- ✅ `nameGenerationData` is null or not assigned
- ✅ Run `Dexiled → Create Name Generation Data` menu item
- ✅ Assign asset to LootManager

### **Magic Items Show Wrong Affixes:**
- ✅ Check affix `name` field in your affix ScriptableObjects
- ✅ Ensure affixes have proper names like "Flaming", "of Dexterity"
- ✅ Magic item format is `"[Prefix.name] BaseName [of Suffix.name]"`

---

## 📈 **Statistics**

- **Total Rare Prefixes:** 101
- **Weapon Melee Suffixes:** 20
- **Weapon Ranged Suffixes:** 20
- **Weapon Caster Suffixes:** 20
- **Helmet Suffixes:** 20
- **Body Armour Suffixes:** 20
- **Glove Suffixes:** 20
- **Boot Suffixes:** 20
- **Belt Suffixes:** 20
- **Amulet Suffixes:** 20
- **Ring Suffixes:** 20
- **Shield Suffixes:** 20

**Possible Rare Names:** 101 prefixes × ~20 suffixes per category = **~2,020 unique names per item category!**

---

## ✅ **Status: COMPLETE**

All systems implemented and ready to use! 🎉

**Next Steps:**
1. Run `Dexiled → Create Name Generation Data` in Unity
2. Assign asset to LootManager
3. Test with encounters or `SimpleItemGenerator`
4. Enjoy snappy ARPG-style item names!

---

**No linter errors!** 🎯

Magic and Rare items will now have proper ARPG-style names! 🎮

