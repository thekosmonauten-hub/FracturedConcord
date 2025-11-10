# Equipment Manager Setup Guide

**Created:** November 1, 2025  
**System:** Equipment management with stat calculations for Unity UI  
**Location:** `EquipmentScreen_New`

---

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [UI Hierarchy Setup](#ui-hierarchy-setup)
4. [Component Configuration](#component-configuration)
5. [Integration with EquipmentManager](#integration-with-equipmentmanager)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

---

## Overview

The **EquipmentManagerUI** system manages the 10 equipment slots in your Unity UI and integrates with the existing `EquipmentManager` to calculate and apply stats to your character. It bridges the gap between your ScriptableObject items (WeaponItem, Armour, etc.) and the runtime stat calculation system.

### Key Features
- ✅ **10 Equipment Slots**: Helmet, BodyArmour, Belt, Gloves, Boots, Ring1, Ring2, Amulet, Weapon, Offhand
- ✅ **Stat Calculations**: Automatically calculates stats from equipped items
- ✅ **Character Integration**: Applies stats to CharacterStats via EquipmentManager
- ✅ **Requirement Checking**: Validates level and attribute requirements
- ✅ **Visual Updates**: Updates slot displays when items change
- ✅ **ScriptableObject Support**: Works with your BaseItem SO system

---

## Architecture

```
EquipmentManagerUI (Unity UI Controller)
    ├── EquipmentManager (Singleton - Stat Calculations)
    │   ├── CalculateTotalEquipmentStats()
    │   └── ApplyEquipmentStats()
    │
    ├── Character (Player Character)
    │   └── ApplyEquipmentModifiers()
    │
    └── EquipmentSlotUI Components (10 slots)
        ├── Helmet
        ├── BodyArmour
        ├── Belt
        ├── Gloves
        ├── Boots
        ├── Ring1 (LeftRing)
        ├── Ring2 (RightRing)
        ├── Amulet
        ├── Weapon (MainHand)
        └── Offhand (OffHand)
```

### Data Flow

```
1. User equips BaseItem ScriptableObject
   → EquipmentManagerUI.EquipItem(BaseItem)
   
2. Convert BaseItem to ItemData
   → ConvertToItemData(BaseItem)
   → Extracts stats from affixes
   
3. Update slot display
   → EquipmentSlotUI.SetEquippedItem(ItemData)
   
4. Calculate total stats
   → ConvertAffixesToStats()
   → Aggregate all equipped items
   
5. Apply to character
   → equipmentManager.ApplyEquipmentStats()
   → Character.ApplyEquipmentModifiers()
```

---

## UI Hierarchy Setup

### Step 1: Add EquipmentManagerUI Component

1. In your `EquipmentScreen_New` hierarchy, find the `EquipmentNavDisplay` GameObject
2. **Add Component** → **EquipmentManagerUI** script
3. The component should be at the root of your equipment display area

**Recommended Structure:**
```
EquipmentNavDisplay
  ├── Helmet (EquipmentSlotUI component)
  ├── BodyArmour (EquipmentSlotUI component)
  ├── Belt (EquipmentSlotUI component)
  ├── Gloves (EquipmentSlotUI component)
  ├── Boots (EquipmentSlotUI component)
  ├── Ring1 (EquipmentSlotUI component)
  ├── Ring2 (EquipmentSlotUI component)
  ├── Amulet (EquipmentSlotUI component)
  ├── Weapon (EquipmentSlotUI component)
  ├── Offhand (EquipmentSlotUI component)
  └── EquipmentManagerUI (Main controller)
```

---

### Step 2: Configure Each EquipmentSlotUI

For each of your 10 GameObjects (Helmet, BodyArmour, etc.):

1. **Select the GameObject** (e.g., "Helmet")
2. **Add Component**: `EquipmentSlotUI` (if not already present)
3. **Configure the component:**

**EquipmentSlotUI Settings:**
```
Slot Type: Helmet (drop-down selection)
Slot Label: "Helmet" (or whatever you want displayed)

References:
  - Background Image: Drag the background Image component
  - Item Icon Image: Drag the icon Image component  
  - Slot Label: Drag the label TextMeshPro component (optional)
  - Item Name Label: Drag the item name TextMeshPro component (optional)
  
Visual Settings:
  - Empty Color: Dark gray for empty slots
  - Occupied Color: Slightly brighter for occupied slots
  - Hover Color: Highlight color on hover
```

4. **Repeat for all 10 slots** with appropriate Slot Type

---

### Step 3: Configure EquipmentManagerUI

1. **Select the GameObject** with the EquipmentManagerUI component
2. **In Inspector, configure:**

**Slot References:**
- **Helmet Slot**: Drag the "Helmet" GameObject
- **BodyArmour Slot**: Drag the "BodyArmour" GameObject
- **Belt Slot**: Drag the "Belt" GameObject
- **Gloves Slot**: Drag the "Gloves" GameObject
- **Boots Slot**: Drag the "Boots" GameObject
- **Ring1 Slot**: Drag the "Ring1" GameObject
- **Ring2 Slot**: Drag the "Ring2" GameObject
- **Amulet Slot**: Drag the "Amulet" GameObject
- **Weapon Slot**: Drag the "Weapon" GameObject
- **Offhand Slot**: Drag the "Offhand" GameObject

**System References:**
- **Equipment Manager**: ⚠️ **Leave empty in Edit Mode** (will auto-find EquipmentManager.Instance at runtime)
- **Current Character**: Leave empty (auto-finds from CharacterManager at runtime)

**Settings:**
- **Initialize On Start**: ✅ Checked

3. **Quick Setup:** Right-click EquipmentManagerUI component → **"Auto-Assign Slots"**
   - Automatically finds and assigns all slot references

4. **Important:** Do NOT use "Find EquipmentManager" in Edit Mode
   - The EquipmentManager is a singleton from another scene
   - It will be automatically found when you enter Play Mode
   - **Leave the Equipment Manager field empty in Edit Mode**

---

## Component Configuration

### EquipmentManagerUI Settings

| Setting | Description | Required |
|---------|-------------|----------|
| **Slot References** | All 10 equipment slot objects | ✅ Yes |
| **Equipment Manager** | EquipmentManager.Instance reference | Auto-found |
| **Current Character** | Character instance | Auto-found |
| **Initialize On Start** | Auto-initialize on Start() | Recommended ✅ |

### EquipmentSlotUI Settings

| Setting | Description | Value |
|---------|-------------|-------|
| **Slot Type** | Which equipment type this is | Helmet/BodyArmour/etc |
| **Slot Label** | Text label for the slot | Optional |
| **Background Image** | Image component for background | Required |
| **Item Icon Image** | Image for equipped item icon | Optional |
| **Item Name Label** | Text for item name | Optional |
| **Empty Color** | Background color when empty | Dark gray |
| **Occupied Color** | Background color when occupied | Lighter |
| **Hover Color** | Background color on hover | Highlight |

---

## Integration with EquipmentManager

The `EquipmentManagerUI` integrates seamlessly with your existing `EquipmentManager` singleton:

### How Stats Are Calculated

1. **Base Item Properties**
   - Weapons: minDamage, maxDamage, crit chance, attack speed
   - Armour: armour value, evasion, energy shield

2. **Affix Modifiers**
   - Implicit modifiers (always present)
   - Prefixes (random affixes)
   - Suffixes (random affixes)

3. **Stat Aggregation**
   - All equipped items' stats are summed
   - Handles dual-range damage affixes
   - Applies Local vs Global scope correctly

4. **Character Application**
   - Reset to base character stats
   - Apply all equipment modifiers
   - Update DerivedStats

### Supported Stat Types

**Damage Stats:**
- `PhysicalDamage`, `FireDamage`, `ColdDamage`, `LightningDamage`, `ChaosDamage`
- `IncreasedPhysicalDamage`, `IncreasedFireDamage`, etc.
- `MorePhysicalDamage`, `MoreFireDamage`, etc.

**Defense Stats:**
- `Armour`, `Evasion`, `EnergyShield`
- Resistance stats (via DamageStats)

**Attributes:**
- `Strength`, `Dexterity`, `Intelligence`

**Critical Stats:**
- `CriticalStrikeChance`, `CriticalStrikeMultiplier`

**Other:**
- Any custom stat from your AffixModifiers

---

## Testing

### Quick Test Checklist

1. **Play Mode Test:**
   ```
   ✅ Enter Play Mode
   ✅ Check Console for initialization logs:
      - "[EquipmentManagerUI] Equipment manager initialized successfully"
      - "[EquipmentManagerUI] Built slot map with 10 slots"
   ✅ Verify all 10 slots appear on screen
   ✅ Check slots are initially empty
   ```

2. **Equipment Test (Add manually via Console):**
   ```csharp
   // Get a test item from ItemDatabase
   var db = ItemDatabase.Instance;
   var testWeapon = db.weapons[0]; // Get first weapon
   
   // Get the EquipmentManagerUI
   var manager = FindObjectOfType<EquipmentManagerUI>();
   
   // Equip the item
   manager.EquipItem(testWeapon);
   
   ✅ Check that slot displays the weapon
   ✅ Check that character stats updated
   ```

3. **Stat Calculation Test:**
   - Equip multiple items
   - Open CharacterStats panel
   - Verify stats are correctly aggregated

---

## Troubleshooting

### Issue: Slots Not Found

**Console Error:**
```
[EquipmentManagerUI] Built slot map with 0 slots
```

**Possible Causes:**
1. Slot GameObjects not assigned
2. EquipmentSlotUI components missing
3. Slot Type not set correctly

**Fix:**
1. Use **"Auto-Assign Slots"** context menu
2. Verify each GameObject has EquipmentSlotUI component
3. Check that Slot Type is set to correct EquipmentType

---

### Issue: Character Not Found

**Console Error:**
```
CharacterManager not found. Stats will not be applied.
```

**Possible Causes:**
1. No CharacterManager in scene
2. No current character loaded

**Fix:**
1. Verify CharacterManager exists in scene
2. Ensure a character is selected/loaded

---

### Issue: EquipmentManager Not Found

**Console Error:**
```
[EquipmentManagerUI] EquipmentManager.Instance not found!
```

**Possible Causes:**
1. EquipmentManager not in scene
2. EquipmentManager not set up as singleton
3. Trying to access singleton in Edit Mode

**Fix:**
1. **In Edit Mode:** Leave equipmentManager field **empty**
   - It will auto-find at runtime
2. **In Play Mode:** It will automatically find EquipmentManager.Instance
3. EquipmentManager will auto-create if missing (DontDestroyOnLoad)

---

### Issue: DontDestroyOnLoad Error in Edit Mode

**Console Error:**
```
InvalidOperationException: The following game object is invoking the DontDestroyOnLoad method: EquipmentManager.
Notice that DontDestroyOnLoad can only be used in play mode...
```

**Cause:**
- Trying to use "Find EquipmentManager" context menu in Edit Mode
- EquipmentManager is a singleton that uses DontDestroyOnLoad

**Fix:**
1. **DO NOT** use "Find EquipmentManager" in Edit Mode
2. **Leave the equipmentManager field empty** in Inspector
3. The system will automatically find it when you enter Play Mode
4. This is expected Unity behavior for cross-scene singletons

---

### Issue: Stats Not Updating

**Check:**
1. Verify equipmentManager.ApplyEquipmentStats() is being called
2. Check Console for stat calculation logs
3. Verify character is set in EquipmentManager

**Debug:**
Add this to EquipmentManagerUI.Start():
```csharp
Debug.Log($"Character: {currentCharacter != null}");
Debug.Log($"EquipmentManager: {equipmentManager != null}");
```

---

## Advanced Usage

### Programmatically Equip Items

```csharp
// Get reference to EquipmentManagerUI
EquipmentManagerUI manager = FindObjectOfType<EquipmentManagerUI>();

// Get item from database
ItemDatabase db = ItemDatabase.Instance;
WeaponItem sword = db.weapons[0];

// Equip it
bool success = manager.EquipItem(sword);

if (success)
{
    Debug.Log("Item equipped successfully!");
}
```

### Listen for Equipment Changes

```csharp
public class MyCustomSystem : MonoBehaviour
{
    private void Start()
    {
        EquipmentManagerUI manager = FindObjectOfType<EquipmentManagerUI>();
        
        // You can listen to individual slot events
        foreach (EquipmentSlotUI slot in GetAllSlots())
        {
            slot.OnSlotClicked += (type) => {
                Debug.Log($"Slot {type} was clicked");
            };
        }
    }
}
```

### Manual Stat Calculation

```csharp
// Force recalculate stats without changing equipment
EquipmentManagerUI manager = FindObjectOfType<EquipmentManagerUI>();
manager.CalculateAndApplyStats();
```

---

## Integration with CardCarousel

Your EquipmentScreen_New likely has both:
- **CurrencyDisplay** (from previous setup)
- **EquipmentNavDisplay** (current setup)

They work independently:
```
EquipmentScreen_New
├── CardCarousel (your existing carousel)
├── CurrencyDisplay (currencies - previously set up)
│   ├── OrbsSection
│   ├── SpiritsSection
│   └── FragmentsSection
└── EquipmentNavDisplay (equipment - current setup)
    ├── EquipmentManagerUI (main controller)
    ├── Helmet
    ├── BodyArmour
    ├── ... (all 10 slots)
```

---

## Development Log

### Version 1.0 - November 1, 2025
- ✅ Created EquipmentManagerUI for Unity UI system
- ✅ Integrated with existing EquipmentManager singleton
- ✅ Support for 10 equipment slots
- ✅ BaseItem ScriptableObject conversion system
- ✅ Affix-to-stats calculation pipeline
- ✅ Character stat application
- ✅ Requirement validation
- ✅ Auto-assignment helpers
- ✅ Comprehensive documentation

### Future Enhancements
- [ ] Drag & drop support between inventory and equipment
- [ ] Tooltip system for equipped items
- [ ] Comparison tooltips (old vs new item)
- [ ] Affix preview on hover
- [ ] Stat change animations
- [ ] Equipment loadout saving/loading

---

## Quick Reference

### Initialization Flow
```
1. EquipmentManagerUI.Start()
2. → Initialize()
3.   → Find EquipmentManager.Instance
4.   → Find Character from CharacterManager
5.   → BuildSlotMap()
6.   → SubscribeToSlotEvents()
7.   → LoadSavedEquipment()
```

### Equip Flow
```
1. EquipItem(BaseItem)
2. → ValidateRequirements()
3. → ConvertToItemData()
4.   → Extract base properties
5.   → ConvertAffixesToStats()
6.   → Build stats dictionary
7. → Update slot display
8. → CalculateAndApplyStats()
9.   → Aggregate all equipped stats
10.  → equipmentManager.ApplyEquipmentStats()
11.     → Character.ApplyEquipmentModifiers()
```

---

**End of Guide**

