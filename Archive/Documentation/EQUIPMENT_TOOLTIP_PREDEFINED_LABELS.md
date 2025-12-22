# Equipment Tooltip - Pre-defined Stat Labels

**Date:** December 5, 2025  
**Status:** ✅ **COMPLETE**

---

## 🎯 **What Was Changed**

Replaced the dynamic `baseStatLabels` list approach with **pre-defined SerializeField labels** for specific stats:

### **Pre-defined Labels:**
1. **Quality** - `qualityLabel`
2. **Armour** - `armourLabel`
3. **Energy Shield** - `energyShieldLabel`
4. **Evasion** - `evasionLabel`
5. **Item Type** - `itemTypeLabel`

---

## 📁 **Files Modified**

### **EquipmentTooltipView.cs**
`Assets/Scripts/UI/EquipmentScreen/EquipmentTooltipView.cs`

**Added Fields:**
```csharp
[Header("Pre-defined Stat Labels")]
[SerializeField] private TextMeshProUGUI qualityLabel;
[SerializeField] private TextMeshProUGUI armourLabel;
[SerializeField] private TextMeshProUGUI energyShieldLabel;
[SerializeField] private TextMeshProUGUI evasionLabel;
[SerializeField] private TextMeshProUGUI itemTypeLabel;
```

**New Methods:**
- `ResetPredefinedLabels()` - Hides all pre-defined labels
- `SetItemTypeLabel(BaseItem)` - Sets item type text
- `SetQualityLabel(BaseItem)` - Sets quality text
- `ApplyOtherBaseStats(BaseItem)` - Handles non-predefined stats

**Modified Methods:**
- `ApplyBaseStats(BaseItem)` - Now uses pre-defined labels
- `ApplyDefenceValues(Armour)` - Uses pre-defined labels for Armour/Evasion/Energy Shield
- `ApplyBaseStats(ItemData)` - Uses pre-defined labels for ItemData
- `CacheUIElements()` - Auto-finds pre-defined labels by name

---

## 🎮 **Setup in Unity**

### **Step 1: Create Label GameObjects**

In your `EquipmentTooltip` prefab, under `Content/BaseItemStats` (or `Content/BaseWeaponStats`), create these GameObjects:

1. **QualityLabel** - TextMeshProUGUI component
2. **ArmourLabel** - TextMeshProUGUI component
3. **EnergyShieldLabel** - TextMeshProUGUI component
4. **EvasionLabel** - TextMeshProUGUI component
5. **ItemTypeLabel** - TextMeshProUGUI component

### **Step 2: Assign in Inspector**

1. Select the `EquipmentTooltipView` component
2. Expand **"Pre-defined Stat Labels"** section
3. Drag each label GameObject to its corresponding field:
   - `Quality Label` → QualityLabel GameObject
   - `Armour Label` → ArmourLabel GameObject
   - `Energy Shield Label` → EnergyShieldLabel GameObject
   - `Evasion Label` → EvasionLabel GameObject
   - `Item Type Label` → ItemTypeLabel GameObject

**OR** leave them empty - the code will auto-find them by name!

---

## 🔄 **How It Works**

### **Label Population:**

**Quality Label:**
- Shows: `"Quality: +{value}%"`
- Hidden if: `item.quality <= 0`

**Armour Label:**
- Shows: `"Armour: {value}"`
- Only for Armour items
- Hidden if: `armour.armour <= 0`

**Energy Shield Label:**
- Shows: `"Energy Shield: {value}"`
- Only for Armour items
- Hidden if: `armour.energyShield <= 0`

**Evasion Label:**
- Shows: `"Evasion: {value}"`
- Only for Armour items
- Hidden if: `armour.evasion <= 0`

**Item Type Label:**
- Shows: `"{Slot} - {Type}"` (e.g., "Helmet - Armour", "Ring - Jewellery")
- Or: `"Tags: {tag1, tag2}"` for generic items
- Always shown if item has type information

---

## 📊 **Label Behavior**

### **For Armour Items:**
```
Item Type Label: "Helmet - Armour"
Armour Label: "Armour: 150" (if > 0)
Evasion Label: "Evasion: 200" (if > 0)
Energy Shield Label: "Energy Shield: 50" (if > 0)
Quality Label: "Quality: +20%" (if > 0)
```

### **For Other Items:**
```
Item Type Label: "Ring - Jewellery" (or "Tags: ...")
Quality Label: "Quality: +15%" (if > 0)
Other stats → Generic baseStatLabels
```

---

## 🎨 **Auto-Discovery**

The code automatically finds labels by name if not assigned in Inspector:

**Search Paths:**
1. `Content/BaseItemStats/{LabelName}`
2. `Content/BaseWeaponStats/{LabelName}` (fallback)

**Label Names:**
- `QualityLabel`
- `ArmourLabel`
- `EnergyShieldLabel`
- `EvasionLabel`
- `ItemTypeLabel`

---

## ⚠️ **Important Notes**

### **Backwards Compatibility:**
- Generic `baseStatLabels` still work for other stats
- Pre-defined labels are **in addition to**, not replacement of, generic labels
- If a pre-defined label is not found, it's simply hidden (no errors)

### **Label Visibility:**
- Labels are **automatically hidden** if:
  - The stat value is 0 or negative
  - The item type doesn't have that stat
  - The label GameObject is null

### **Order Matters:**
- Pre-defined labels can be positioned anywhere in the UI hierarchy
- They don't need to be in a specific order
- The code finds them by name, not position

---

## 🧪 **Testing**

### **Test Armour Item:**
1. Hover over an Armour item
2. ✅ Item Type Label shows "Helmet - Armour" (or similar)
3. ✅ Armour Label shows if armour > 0
4. ✅ Evasion Label shows if evasion > 0
5. ✅ Energy Shield Label shows if energyShield > 0
6. ✅ Quality Label shows if quality > 0

### **Test Jewellery:**
1. Hover over a Ring/Amulet/Belt
2. ✅ Item Type Label shows "Ring - Jewellery" (or similar)
3. ✅ Quality Label shows if quality > 0
4. ✅ Other stats in generic labels

### **Test Item Without Stats:**
1. Hover over item with no quality/defense
2. ✅ Pre-defined labels are hidden
3. ✅ Only Item Type Label shows (if applicable)

---

## ✅ **Status: COMPLETE**

**No linter errors!** 🎯

Pre-defined labels are now:
- ✅ Separate SerializeField references
- ✅ Auto-discovered by name
- ✅ Properly shown/hidden based on values
- ✅ Backwards compatible with generic labels

---

## 🚀 **Next Steps (Optional)**

### **Add More Pre-defined Labels:**
If you want more specific labels, add them to:
- `EquipmentTooltipView.cs` - Add SerializeField
- `CacheUIElements()` - Add auto-discovery
- `ApplyBaseStats()` - Add population logic

### **Visual Styling:**
- Style each label differently (colors, fonts)
- Add icons next to labels
- Group labels visually

---

**Ready to use!** 🎮

Just create the label GameObjects in your prefab and assign them (or let auto-discovery find them)! 🚀

