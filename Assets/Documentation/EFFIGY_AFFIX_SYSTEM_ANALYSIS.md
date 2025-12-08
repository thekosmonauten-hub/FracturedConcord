# Effigy Affix System Analysis & Proposal

## Current Implementation

### Current System Overview
The Effigy system currently uses the regular `AffixDatabase` with a **0.1x scaling factor** applied to all affix values. This means:
- Effigies pull from **Weapon**, **Armour**, and **Jewellery** affix categories
- All values are scaled down by 90% (multiplied by 0.1)
- No shape-specific affix pools exist
- All shapes use the same affix pool

**Note**: The new `EffigyAffixDatabase` system removes the scaling factor - affixes should be designed at full power for effigies.

### Current Files
1. **Effigy.cs** - Data structure with:
   - Shape definitions (Cross, L-Shape, Line, S-Shape, Single, Square, Small L, T-Shape, Z-Shape)
   - Element types (Fire, Cold, Lightning, Physical, Chaos)
   - Size tiers (Tiny, Medium, Large)
   - Affix lists (prefixes, suffixes, implicitModifiers)

2. **EffigyAffixGenerator.cs** - Current affix generation:
   - Uses `AffixDatabase` (weapon, armour, jewellery categories)
   - Applies `EffigyScalingFactor = 0.1f` to all values
   - Generates 4 explicit affixes (prefixes + suffixes)
   - Sets all modifiers to `ModifierScope.Global`

3. **EffigyImplicitLibrary.cs** - Shape-specific implicits:
   - **Cross**: +3/5/7 to all Attributes (Strength, Dexterity, Intelligence)
   - **L-Shape**: +4/6/8% increased Maximum Life
   - **Line**: +6/8/10% increased Damage
   - **S-Shape**: +6/8/10% increased Evasion, +3/4/5% Dodge Chance
   - **Single**: +1-3 to random Attribute
   - **Square**: +6/8/10% increased Guard Effectiveness
   - **Small L**: +10/15/20% increased Damage after Guarding or Attacking
   - **T-Shape**: +5/7/10% increased Buff Duration
   - **Z-Shape**: +5/7/10% chance to inflict Random Ailment on Hit

4. **EffigyFactory.cs** - Creates effigy instances and rolls affixes

### Current Issues
1. ❌ **No shape-specific affix pools** - All shapes use the same affixes
2. ❌ **Generic scaling** - 0.1x factor doesn't account for shape differences
3. ❌ **No thematic affixes** - Shapes don't have unique affix themes
4. ❌ **Limited customization** - Can't create shape-specific affix categories

---

## Proposed Solution: EffigyAffixDatabase

### New Structure

Create a dedicated `EffigyAffixDatabase` ScriptableObject with shape-specific affix categories:

```csharp
[CreateAssetMenu(fileName = "Effigy Affix Database", menuName = "Dexiled/Items/Effigy Affix Database")]
public class EffigyAffixDatabase : ScriptableObject
{
    [Header("Cross Shape Affixes")]
    public List<AffixCategory> crossPrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> crossSuffixCategories = new List<AffixCategory>();
    
    [Header("L-Shape Affixes")]
    public List<AffixCategory> lShapePrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> lShapeSuffixCategories = new List<AffixCategory>();
    
    [Header("Line Shape Affixes")]
    public List<AffixCategory> linePrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> lineSuffixCategories = new List<AffixCategory>();
    
    [Header("S-Shape Affixes")]
    public List<AffixCategory> sShapePrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> sShapeSuffixCategories = new List<AffixCategory>();
    
    [Header("Single Shape Affixes")]
    public List<AffixCategory> singlePrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> singleSuffixCategories = new List<AffixCategory>();
    
    [Header("Square Shape Affixes")]
    public List<AffixCategory> squarePrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> squareSuffixCategories = new List<AffixCategory>();
    
    [Header("Small L Shape Affixes")]
    public List<AffixCategory> smallLPrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> smallLSuffixCategories = new List<AffixCategory>();
    
    [Header("T-Shape Affixes")]
    public List<AffixCategory> tShapePrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> tShapeSuffixCategories = new List<AffixCategory>();
    
    [Header("Z-Shape Affixes")]
    public List<AffixCategory> zShapePrefixCategories = new List<AffixCategory>();
    public List<AffixCategory> zShapeSuffixCategories = new List<AffixCategory>();
}
```

### Shape-Specific Affix Themes (Proposed)

Based on the implicit modifiers, here are suggested affix themes for each shape:

#### **Cross** (All Attributes)
- **Prefixes**: Attribute bonuses (Strength, Dexterity, Intelligence), Hybrid stats
- **Suffixes**: Attribute scaling effects, Multi-attribute bonuses

#### **L-Shape** (Life/Defense)
- **Prefixes**: Life bonuses, Life regeneration, Max Life increased
- **Suffixes**: Life leech, Life on hit, Life recovery

#### **Line** (Damage)
- **Prefixes**: Damage bonuses (Physical, Elemental, Chaos), Damage increased
- **Suffixes**: Damage multipliers, Damage scaling, Damage over time

#### **S-Shape** (Evasion/Dodge)
- **Prefixes**: Evasion bonuses, Dodge chance, Movement speed
- **Suffixes**: Evasion increased, Dodge effectiveness, Evasion scaling

#### **Single** (Random/Adaptive)
- **Prefixes**: Small attribute bonuses, Generic stat boosts
- **Suffixes**: Utility effects, Minor bonuses

#### **Square** (Guard/Defense)
- **Prefixes**: Guard bonuses, Guard effectiveness, Armour bonuses
- **Suffixes**: Guard scaling, Guard regeneration, Defense bonuses

#### **Small L** (Offensive Defense)
- **Prefixes**: Damage after guard, Counter-attack bonuses, Retaliation damage
- **Suffixes**: Guard-to-damage conversion, Aggressive defense

#### **T-Shape** (Buffs/Duration)
- **Prefixes**: Buff duration, Buff effectiveness, Status effect duration
- **Suffixes**: Buff stacking, Buff scaling, Duration bonuses

#### **Z-Shape** (Ailments/Chaos)
- **Prefixes**: Ailment chance, Ailment damage, Chaos damage
- **Suffixes**: Ailment duration, Ailment effectiveness, DoT bonuses

---

## Implementation Plan

### Phase 1: Create EffigyAffixDatabase
1. Create `EffigyAffixDatabase.cs` ScriptableObject
2. Add shape-specific category lists
3. Create Unity menu item to create the database asset
4. Place in `Assets/Resources/EffigyAffixDatabase.asset`

### Phase 2: Update EffigyAffixGenerator
1. ✅ Replace `AffixDatabase` parameter with `EffigyAffixDatabase` - Completed
2. ✅ Remove prefix/suffix categories - Unified affix pool per shape - Completed
3. ✅ Remove 0.1x scaling factor - Completed (affixes use full values)
4. ✅ Use `effigy.GetShapeCategory()` to select the correct affix pool - Completed
5. ✅ Simplified database structure - Single `affixCategories` list per shape - Completed

### Phase 3: Update EffigyFactory
1. Update `CreateInstance()` to use `EffigyAffixDatabase` instead of `AffixDatabase`
2. Ensure proper database loading

### Phase 4: Create Affix Categories
1. Create affix categories for each shape
2. Design affixes that match the shape themes
3. Import/create affix assets for each category

---

## Files to Modify

1. **NEW**: `Assets/Scripts/Data/Items/EffigyAffixDatabase.cs`
2. **MODIFY**: `Assets/Scripts/Data/Items/EffigyAffixGenerator.cs`
3. **MODIFY**: `Assets/Scripts/Data/Items/EffigyFactory.cs`
4. **CHECK**: `Assets/Scripts/UI/EquipmentScreen/UnityUI/EquipmentScreenUI.cs` (may need to pass EffigyAffixDatabase)

---

## Next Steps

1. ✅ **Analysis Complete** - Current system documented
2. ✅ **Create EffigyAffixDatabase structure** - Completed
3. ✅ **Update EffigyAffixGenerator to use shape-specific pools** - Completed
4. ✅ **Remove 0.1x scaling factor** - Completed (affixes use full values)
5. ✅ **Remove prefix/suffix distinction** - Completed (effigies roll from unified pool)
6. ⏳ **Create initial affix categories for each shape**
7. ⏳ **Test affix generation with new system**

## 📚 Stat Key Reference

When creating affixes for Effigies, refer to **`STAT_KEY_REFERENCE.md`** for a complete list of all working stat keys. This document includes:
- All stat keys organized by category
- Value types (Flat, Increased, More)
- Conditional modifiers and their requirements
- Special behaviors and formulas
- Example values

**Key Points for Effigy Affixes:**
- Use `statName` field in `AffixModifier` with stat keys from the reference
- All modifiers are automatically set to `ModifierScope.Global` for effigies
- Affixes use their full values (no scaling)
- Stat keys are case-insensitive

