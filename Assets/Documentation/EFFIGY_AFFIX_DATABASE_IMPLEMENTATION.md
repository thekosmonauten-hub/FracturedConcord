# Effigy Affix Database Implementation - Complete

## ✅ Implementation Summary

The Effigy system has been successfully updated to use a dedicated `EffigyAffixDatabase` with shape-specific affix categories instead of the generic `AffixDatabase` with 0.1x scaling.

---

## 📁 Files Created

### 1. **EffigyAffixDatabase.cs**
- **Location**: `Assets/Scripts/Data/Items/EffigyAffixDatabase.cs`
- **Purpose**: ScriptableObject database with shape-specific affix categories
- **Features**:
  - 9 shape categories (Cross, L-Shape, Line, S-Shape, Single, Square, Small L, T-Shape, Z-Shape)
  - Each shape has a unified affix category list (no prefix/suffix distinction)
  - Singleton pattern with `Instance` property
  - Helper methods: `GetAffixCategories()`, `GetAllAffixes()`

### 2. **CreateEffigyAffixDatabase.cs**
- **Location**: `Assets/Editor/CreateEffigyAffixDatabase.cs`
- **Purpose**: Unity menu item to create the database asset
- **Usage**: `Dexiled → Create Effigy Affix Database`
- **Creates**: `Assets/Resources/EffigyAffixDatabase.asset`

---

## 🔧 Files Modified

### 1. **EffigyAffixGenerator.cs**
- **Changes**:
  - Updated `RollAffixes()` to accept `EffigyAffixDatabase` instead of `AffixDatabase`
  - Simplified to use unified affix pool (no prefix/suffix distinction)
  - `BuildCombinedAffixPool()` now directly gets all affixes for the shape
  - Now uses `effigy.GetShapeCategory()` to select the correct affix pool
  - Removed dependency on weapon/armour/jewellery categories
  - All rolled affixes are stored in `effigy.prefixes` (suffixes remain empty)

### 2. **EffigyFactory.cs**
- **Changes**:
  - Updated `CreateInstance()` to accept `EffigyAffixDatabase` instead of `AffixDatabase`
  - Updated warning messages to reference `EffigyAffixDatabase`

### 3. **AreaLootTable.cs**
- **Changes**:
  - Updated to use `EffigyAffixDatabase.Instance` instead of `AffixDatabase.Instance`

### 4. **EffigyStorageUI.cs**
- **Changes**:
  - Updated to use `EffigyAffixDatabase.Instance` instead of `AffixDatabase.Instance`

---

## 🎯 Shape-Specific Affix Themes

Based on the implicit modifiers, each shape has a thematic focus:

| Shape | Implicit Theme | Suggested Affix Focus |
|-------|---------------|----------------------|
| **Cross** | All Attributes | Attributes, Hybrid stats, Multi-attribute bonuses |
| **L-Shape** | Life/Defense | Life bonuses, Life regeneration, Life recovery, Life leech |
| **Line** | Damage | Damage bonuses, Damage increased, Damage scaling, DoT |
| **S-Shape** | Evasion/Dodge | Evasion, Dodge chance, Movement speed, Evasion scaling |
| **Single** | Random/Adaptive | Small attribute bonuses, Generic stat boosts, Utility effects |
| **Square** | Guard/Defense | Guard bonuses, Guard effectiveness, Armour, Defense |
| **Small L** | Offensive Defense | Damage after guard, Counter-attack, Retaliation, Guard-to-damage |
| **T-Shape** | Buffs/Duration | Buff duration, Buff effectiveness, Status effect duration |
| **Z-Shape** | Ailments/Chaos | Ailment chance, Ailment damage, Chaos damage, DoT bonuses |

---

## 🚀 Next Steps

### 1. Create the Database Asset
1. In Unity, go to `Dexiled → Create Effigy Affix Database`
2. This creates `Assets/Resources/EffigyAffixDatabase.asset`
3. Select the asset in the Project window

### 2. Populate Affix Categories
For each shape, you need to:
1. Create `AffixCategory` objects (or use existing ones)
2. Add them to the shape's unified `affixCategories` list (e.g., `crossAffixCategories`)
3. Each category should contain `AffixSubCategory` objects
4. Each subcategory should contain `Affix` assets

**Important**: Effigies use a unified affix pool - there is no prefix/suffix distinction. Each shape has a single list of affix categories.

### 3. Design Affixes
- Create affix assets that match each shape's theme
- Affixes use their full values (no scaling) - design them at the appropriate power level
- All modifiers should use `ModifierScope.Global` (handled automatically)
- **Unified pool**: Effigies use a single affix pool per shape - no prefix/suffix distinction

### 4. Test
- Generate effigies and verify they use shape-specific affixes
- Check that affixes match the shape's theme
- Verify scaling and global scope are applied correctly

---

## 📝 Notes

- **No Scaling Factor**: Affixes use their full values - they should be designed at the appropriate power level for effigies from the start.
- **Global Scope**: All modifiers are automatically set to `ModifierScope.Global` so effigies affect the character globally.
- **Backward Compatibility**: Existing effigies in saves will need to be regenerated to use the new system.
- **Empty Categories**: If a shape has no affix categories, the generator will log a warning and skip affix generation for that shape.

---

## 🔍 Verification Checklist

- [x] `EffigyAffixDatabase.cs` created
- [x] `EffigyAffixGenerator` updated to use shape-specific pools
- [x] `EffigyFactory` updated to use new database
- [x] All call sites updated (`AreaLootTable`, `EffigyStorageUI`)
- [x] Unity menu item created
- [ ] Database asset created in Unity
- [ ] Affix categories populated for each shape
- [ ] Affixes tested in-game

---

## 📚 Related Documentation

- `EFFIGY_AFFIX_SYSTEM_ANALYSIS.md` - Original analysis and proposal
- `EFFIGY_SYSTEM_GUIDE.md` - General effigy system documentation
- **`STAT_KEY_REFERENCE.md`** - **Complete reference of all working stat keys** ⭐ **Use this when creating effigy affixes!**

