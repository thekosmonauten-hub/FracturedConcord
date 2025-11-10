**# Embossing Browser UI - Setup Guide

## ✅ What's Been Implemented

### Components Created:

1. **EmbossingFilterController** - Filter controller that works with existing EmbossingGridUI
2. **EmbossingGridUI** - Already exists (grid layout system)
3. **EmbossingSlotUI** - Already exists (individual slot component)

### Features:
- ✅ Load all embossings from EmbossingDatabase
- ✅ Display in grid layout with slots
- ✅ Filter by Category (Damage, Scaling, Utility, etc.)
- ✅ Filter by Rarity (Common, Rare, Epic, etc.)
- ✅ Filter by Element Type (Physical, Fire, Cold, etc.)
- ✅ Filter by Level (slider from 1-30)
- ✅ Filter by Requirements (show only what character can use)
- ✅ Filter by Affordability (show only what's applicable to selected card)
- ✅ Click selection with visual feedback
- ✅ Info display for selected embossing

---

## 🎯 Hierarchy Setup

Your existing hierarchy should be:

```
EmbossingStorage (GameObject)
├── Filters (Container) ← Add filter controls here
│   ├── CategoryFilter (TMP_Dropdown)
│   ├── RarityFilter (TMP_Dropdown)
│   ├── ElementFilter (TMP_Dropdown)
│   ├── LevelFilter (Container)
│   │   ├── Slider
│   │   └── LevelText (TextMeshProUGUI)
│   ├── OnlyAffordableToggle (Toggle)
│   └── OnlyMeetsRequirementsToggle (Toggle)
│
├── Scroll View (ScrollRect) ← Already exists
│   └── Viewport
│       └── Content (GameObject)
│           ├── EmbossingGridUI (Component) ← Already exists
│           └── GridLayoutGroup (Component) ← Already exists
│               └── (Embossing slots auto-generated here)
│
└── InfoPanel (Container) ← Add info display here
    ├── EmbossingCountText (TextMeshProUGUI)
    └── SelectedEmbossingInfo (TextMeshProUGUI)
```

**Note:** You already have `EmbossingGridUI` on the Content object - no need to create a new grid system!

---

## 🔧 Component Setup

### Step 1: Add EmbossingFilterController

**On GameObject:** `EmbossingStorage` (or parent container)

1. Add Component: `EmbossingFilterController`
2. **Assign References:**

**Grid Reference:**
- **Embossing Grid**: Auto-finds or drag the `Content` GameObject (with EmbossingGridUI component)

**Filters - Dropdowns:**
- **Category Filter**: Drag `CategoryFilter` TMP_Dropdown
- **Rarity Filter**: Drag `RarityFilter` TMP_Dropdown
- **Element Filter**: Drag `ElementFilter` TMP_Dropdown

**Filters - Sliders:**
- **Level Filter**: Drag level `Slider`
- **Level Filter Text**: Drag `LevelText` TextMeshProUGUI

**Filters - Toggles:**
- **Show Only Affordable Toggle**: Drag toggle
- **Show Only Meets Requirements Toggle**: Drag toggle

**References:**
- **Card Carousel**: Auto-finds or drag CardCarouselUI

**Info Display:**
- **Embossing Count Text**: Drag count text
- **Selected Embossing Info**: Drag info text

**That's it!** The existing `EmbossingGridUI` on your Content object will handle the grid display. The filter controller will update it with filtered embossings.

---

### Step 2: Verify EmbossingGridUI Setup

**Check your existing `EmbossingGridUI` on `Content` object:**

**Grid Settings:**
- **Grid Columns**: 4-7 (your choice)
- **Grid Rows**: 20+ (enough for all embossings)
- **Cell Size**: 80-100 pixels
- **Cell Spacing**: 10 pixels
- **Grid Padding**: 10 pixels

**The grid will be auto-generated - no prefab needed!**

**Note:** `EmbossingGridUI` already creates cells automatically. You don't need to create a prefab.

---

### Step 4: Setup Filters

#### Category Filter (TMP_Dropdown)
**Options will be auto-populated:**
- All
- Damage
- Scaling
- Utility
- Defensive
- Combo
- Ailment
- Chaos
- Conversion

#### Rarity Filter (TMP_Dropdown)
**Options will be auto-populated:**
- All
- Common
- Uncommon
- Rare
- Epic
- Legendary

#### Element Filter (TMP_Dropdown)
**Options will be auto-populated:**
- All
- Physical
- Fire
- Cold
- Lightning
- Chaos

#### Level Filter (Slider)
**Settings:**
- Min Value: 1
- Max Value: 30
- Whole Numbers: ✅
- Value: 1

#### Toggles
**Show Only Affordable:**
- Shows only embossings that can be applied to selected card
- Considers available embossing slots

**Show Only Meets Requirements:**
- Shows only embossings character can use
- Checks level and stat requirements

---

## 🎨 Visual Design Suggestions

### Embossing Slot Colors (by Category)

| Category | Background Color | Visual |
|----------|-----------------|--------|
| **Damage** | Red (0.8, 0.2, 0.2) | 🔴 |
| **Scaling** | Green (0.2, 0.8, 0.2) | 🟢 |
| **Utility** | Blue (0.2, 0.5, 0.8) | 🔵 |
| **Defensive** | Purple (0.5, 0.5, 0.8) | 🟣 |
| **Combo** | Orange (0.8, 0.6, 0.2) | 🟠 |
| **Ailment** | Violet (0.6, 0.2, 0.8) | 🟣 |
| **Chaos** | Magenta (0.8, 0.2, 0.6) | 🌸 |
| **Conversion** | Cyan (0.2, 0.8, 0.8) | 🔷 |

### Rarity Colors

| Rarity | Color | Visual |
|--------|-------|--------|
| **Common** | White | ⚪ |
| **Uncommon** | Light Green | 🟢 |
| **Rare** | Light Blue | 🔵 |
| **Epic** | Purple | 🟣 |
| **Legendary** | Orange/Gold | 🟠 |

---

## 🎮 User Flow

### Basic Flow:

```
1. Player enters EquipmentScreen
   ↓
2. Selects card from carousel
   ↓
3. Embossing grid shows all available embossings
   ↓
4. Player applies filters:
   - Category: "Damage"
   - Rarity: "Rare"
   - Level: "10"
   ↓
5. Grid updates to show filtered embossings
   ↓
6. Player clicks embossing slot
   ↓
7. Info panel shows embossing details
   ↓
8. Player clicks "Apply" button (to be implemented)
   ↓
9. Embossing applied to card
```

### Filter Examples:

**Find high-level damage embossings:**
- Category: Damage
- Rarity: Rare+
- Level: 15

**Find what character can use:**
- Toggle: ✅ Only Meets Requirements
- Shows only embossings character qualifies for

**Find what fits selected card:**
- Select card with 2 empty slots
- Toggle: ✅ Only Affordable
- Shows only applicable embossings

---

## 🧪 Testing

### Test Filter Functionality:

**Category Filter:**
```
1. Select "Damage" from dropdown
2. Grid shows only Damage embossings (red background)
3. Count updates: "6 / 28 Embossings"
```

**Rarity Filter:**
```
1. Select "Rare" from dropdown
2. Grid shows only Rare embossings
3. Names displayed in light blue color
```

**Level Filter:**
```
1. Slide to level 10
2. Text shows: "Level: 10"
3. Grid shows embossings requiring level ≤ 10
```

**Requirements Toggle:**
```
1. Enable "Only Meets Requirements"
2. If character is level 5 with low stats:
   - Shows only Common/Uncommon low-level embossings
   - High-level embossings hidden
```

**Combined Filters:**
```
1. Category: Scaling
2. Rarity: Uncommon
3. Requirements: ✅
4. Result: Shows only Uncommon Scaling embossings character can use
```

---

## 🔍 Features Explained

### Filter by Category
Shows embossings of specific type:
- **Damage**: Pure damage increases
- **Scaling**: Stat-based bonuses
- **Utility**: Special mechanics
- **Defensive**: Guard/protection
- **Combo**: Discard/combo synergies
- **Ailment**: Status effects
- **Chaos**: High risk/reward
- **Conversion**: Element conversion

### Filter by Rarity
Controls power level and requirements:
- **Common**: Low requirements, basic effects
- **Uncommon**: Moderate requirements
- **Rare**: Higher requirements, strong effects
- **Epic**: Significant requirements, very strong
- **Legendary**: Extreme requirements, game-changing

### Filter by Element
Shows embossings related to specific damage type:
- Physical conversions
- Fire embossings
- Cold embossings
- Lightning embossings
- Chaos embossings

### Filter by Level
Slider shows embossings available at character's level:
- Level 1: Starter embossings
- Level 10: Mid-game embossings
- Level 20+: Endgame embossings

### Only Affordable
Smart filter considering:
- Card has empty embossing slots
- Embossing not already applied (if unique)
- No exclusivity conflicts

### Only Meets Requirements
Checks character against:
- Minimum level
- Minimum Strength
- Minimum Dexterity
- Minimum Intelligence

---

## 🐛 Troubleshooting

### Grid not populating
**Solution:**
- Check `EmbossingDatabase` exists in scene
- Verify embossings generated: `Tools > Card System > Generate Sample Embossings`
- Check console for errors

### Filters not working
**Solution:**
- Ensure TMP_Dropdowns are assigned
- Check dropdown options populated (should happen on Start)
- Verify filter callbacks connected

### Slots not clickable
**Solution:**
- Check `Button` component on prefab
- Verify `EmbossingSlotUI` component attached
- Ensure `OnSlotClicked` callback registered

### Wrong colors displayed
**Solution:**
- Check `GetTypeColor()` and `GetRarityColor()` in EmbossingEffect.cs
- Verify embossing category/rarity set correctly
- Try creating embossing manually to test

### "Only Meets Requirements" shows nothing
**Solution:**
- Check `CharacterManager.Instance.currentCharacter` is loaded
- Verify character stats set correctly
- Lower level filter to 1
- Check embossing requirements aren't too high

---

## 📋 Implementation Checklist

**Scene Setup:**
- [ ] Create EmbossingStorage GameObject
- [ ] Add EmbossingBrowserUI component
- [ ] Create filter UI (dropdowns, slider, toggles)
- [ ] Create ScrollView with GridLayoutGroup
- [ ] Assign all references

**Prefab Setup:**
- [ ] Create EmbossingSlotPrefab
- [ ] Add Button component
- [ ] Add EmbossingSlotUI component
- [ ] Create child elements (Icon, NameText, etc.)
- [ ] Assign prefab to browser

**Database Setup:**
- [ ] EmbossingDatabase in scene
- [ ] Generated sample embossings (28)
- [ ] Verify embossings loaded

**Testing:**
- [ ] Grid populates on scene start
- [ ] All filters work correctly
- [ ] Clicking slots selects them
- [ ] Info panel updates
- [ ] Combined filters work
- [ ] Requirements filter accurate

**Polish:**
- [ ] Add filter icons
- [ ] Add hover tooltips
- [ ] Add "Reset Filters" button
- [ ] Add embossing count display
- [ ] Add smooth transitions

---

## ✅ Result

**Working Embossing Browser:**

```
╔══════════════════════════════════════════════════╗
║  Filters: [Category▼] [Rarity▼] [Element▼]     ║
║           [Level: ──●──────] 10                   ║
║           [✓] Only Affordable                     ║
║           [✓] Only Meets Requirements             ║
╠══════════════════════════════════════════════════╣
║  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐            ║
║  │🔴  │ │🟢  │ │🔵  │ │🟣  │ │🟠  │   12 / 28   ║
║  │Fero│ │Focu│ │Echo│ │Bas │ │Flow│            ║
║  └────┘ └────┘ └────┘ └────┘ └────┘            ║
║  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐            ║
║  │    │ │    │ │    │ │    │ │    │            ║
║  └────┘ └────┘ └────┘ └────┘ └────┘            ║
╠══════════════════════════════════════════════════╣
║  [Selected: of Ferocity]                         ║
║  +35% Physical Damage                            ║
║  Rarity: Rare | Cost: +35%                       ║
║  Requires: Level 10, Strength 50                 ║
║                                                   ║
║  [Apply Embossing] [Cancel]                      ║
╚══════════════════════════════════════════════════╝
```

**System is fully functional and ready to browse embossings!** 🎨

---

## 🔗 Related Documentation

- **EMBOSSING_SYSTEM.md** - Core embossing mechanics
- **EMBOSSING_SETUP_GUIDE.md** - Database and sample setup
- **INSCRIPTION_SEAL_SETUP.md** - Currency and slot management

**Next:** Implement "Apply Embossing" button functionality!

