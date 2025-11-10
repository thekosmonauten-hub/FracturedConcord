# Currency Display System Setup Guide

**Created:** November 1, 2025  
**System:** Modular currency display with three categorized sections  
**Location:** `EquipmentScreen_New`

---

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [UI Hierarchy Setup](#ui-hierarchy-setup)
4. [Component Configuration](#component-configuration)
5. [Testing & Validation](#testing--validation)
6. [Troubleshooting](#troubleshooting)
7. [Advanced Usage](#advanced-usage)

---

## Overview

The Currency Display System organizes all game currencies from `CurrencyDatabase.asset` into three categorized sections:

- **Orbs Section**: Crafting orbs (0-8) - Primary crafting currencies
- **Spirits Section**: Elemental/stat spirits (9-17) - Element and stat-specific currencies
- **Seals & Fragments Section**: Seals (18-24) + Fragments (25-27) - Special currencies and collectibles

### Key Features
- ✅ Automatic currency spawning from database
- ✅ Dynamic quantity updates
- ✅ Rarity-based visual theming
- ✅ Modular section architecture
- ✅ Performance-optimized updates
- ✅ Editor auto-assignment helpers

---

## Architecture

```
CurrencyManager (Main Controller)
    ├── CurrencyDatabase (Data Source)
    │
    ├── OrbsSection (CurrencySectionController)
    │   └── Container (Horizontal Layout Group)
    │       ├── CurrencyPrefab Instance 1
    │       ├── CurrencyPrefab Instance 2
    │       └── ... (dynamically spawned)
    │
    ├── SpiritsSection (CurrencySectionController)
    │   └── Container (Horizontal Layout Group)
    │       └── ... (dynamically spawned)
    │
    └── FragmentsSection (Seals & Fragments - CurrencySectionController)
        └── Container (Horizontal Layout Group)
            └── ... (dynamically spawned)
```

### Component Responsibilities

| Component | Purpose |
|-----------|---------|
| **CurrencyManager** | Main controller - initializes sections, coordinates updates |
| **CurrencySectionController** | Manages one category - spawns/updates currency items |
| **CurrencyDisplayItem** | Individual currency display - shows icon and quantity |
| **CurrencyDatabase** | Data source - stores all currency definitions |

---

## UI Hierarchy Setup

### Step 1: Create the Root CurrencyDisplay Object

1. In your `EquipmentScreen_New` hierarchy, create a new empty GameObject
2. Name it: **`CurrencyDisplay`**
3. Add Component: **CurrencyManager** script
4. Set RectTransform anchors as needed for your layout

**Recommended Settings:**
```
RectTransform:
  - Anchors: Top-stretch (X: 0-1, Y: 1-1)
  - Pivot: (0.5, 1)
  - Height: 150-200px (adjust based on your needs)
```

---

### Step 2: Create Orbs Section

1. **Create Section Root:**
   - Right-click `CurrencyDisplay` → Create Empty
   - Name: **`OrbsSection`**
   - Add Component: **CurrencySectionController**

2. **Set Category:**
   - In Inspector, set **Category** dropdown to: `Orbs`

3. **Create Header (Optional but recommended):**
   - Right-click `OrbsSection` → UI → Text - TextMeshPro
   - Name: **`Header`**
   - Set text: "CURRENCY"
   - Font Size: 16-18
   - Alignment: Center
   - Color: Your theme color

4. **Create Currency Container:**
   - Right-click `OrbsSection` → Create Empty
   - Name: **`Container`**
   - Add Component: **Horizontal Layout Group**
   
   **Horizontal Layout Group Settings:**
   ```
   Padding: Left=10, Right=10, Top=5, Bottom=5
   Spacing: 15
   Child Alignment: Middle Center
   Child Controls Size: ✅ Width, ✅ Height
   Child Force Expand: ❌ Width, ❌ Height
   ```

5. **Configure CurrencySectionController:**
   - **Currency Container**: Drag the `Container` object here
   - **Currency Prefab**: Drag `CurrencyPrefab.prefab` here
   - **Hide When Empty**: ☐ (leave unchecked unless desired)

---

### Step 3: Create Spirits Section

Repeat Step 2 with these changes:
- Name: **`SpiritsSection`**
- Category: **`Spirits`**
- Header text: "SPIRITS"

---

### Step 4: Create Seals & Fragments Section

Repeat Step 2 with these changes:
- Name: **`FragmentsSection`** (or **`SealsFragmentsSection`**)
- Category: **`Fragments`**
- Header text: "SEALS & FRAGMENTS"

---

### Step 5: Configure the CurrencyPrefab

1. **Select `CurrencyPrefab.prefab`** (in Assets/Prefab/EquipmentScreen/)
2. **Add Component: CurrencyDisplayItem** script

3. **Assign References:**
   - **Currency Icon**: Drag the `Image` child object
   - **Currency Count Text**: Drag the `CurrencyCount` child object
   - **Background Image**: Drag the `Background` child object

4. **Quick Setup:** Right-click component → **"Auto-Assign References"**
   - This will automatically find and assign the children

5. **Visual Settings (Optional):**
   ```
   Normal Rarity Color: RGB(200, 200, 200)
   Magic Rarity Color:  RGB(75, 125, 255)
   Rare Rarity Color:   RGB(255, 200, 50)
   ```

---

### Step 6: Connect Everything to CurrencyManager

1. Select the **`CurrencyDisplay`** root object
2. In the **CurrencyManager** component:
   - **Currency Database**: Drag `CurrencyDatabase.asset` (from Resources folder)
   - **Orbs Section**: Drag the `OrbsSection` object
   - **Spirits Section**: Drag the `SpiritsSection` object
   - **Fragments Section**: Drag the `FragmentsSection` object
   - **Initialize On Start**: ✅ (checked)

3. **Quick Setup:** Right-click CurrencyManager component → **"Auto-Assign Sections"**
   - This will find and assign sections automatically

4. **Load Database:** Right-click → **"Load Database from Resources"**
   - Automatically loads CurrencyDatabase.asset

---

## Component Configuration

### CurrencyManager Settings

| Setting | Description | Recommended Value |
|---------|-------------|-------------------|
| Currency Database | Reference to CurrencyDatabase.asset | Auto-load from Resources |
| Orbs Section | CurrencySectionController for orbs | Auto-assign |
| Spirits Section | CurrencySectionController for spirits | Auto-assign |
| Fragments Section | CurrencySectionController for fragments | Auto-assign |
| Initialize On Start | Auto-initialize on Start() | ✅ Checked |

### CurrencySectionController Settings

| Setting | Description | Value |
|---------|-------------|-------|
| Category | Which currency group to display | Orbs / Spirits / Fragments |
| Currency Container | Parent transform for spawned items | The Container child object |
| Currency Prefab | Template for currency items | CurrencyPrefab.prefab |
| Hide When Empty | Hide section if no currencies | ☐ Usually unchecked |
| Section Root Object | Object to hide/show | Leave empty unless needed |

### CurrencyDisplayItem Settings

| Setting | Description |
|---------|-------------|
| Currency Icon | Image component for the currency sprite |
| Currency Count Text | TextMeshProUGUI for quantity display |
| Background Image | Optional image for rarity color tinting |
| Normal/Magic/Rare Colors | Visual theme colors by rarity |

---

## Testing & Validation

### Quick Test Checklist

1. **Play Mode Test:**
   ```
   ✅ Enter Play Mode
   ✅ Check that all three sections appear
   ✅ Verify currencies are organized correctly:
      - Orbs section: Orbs (0-8)
      - Spirits section: All spirits (9-17)
      - Seals & Fragments section: Seals (18-24) + Fragments (25-27)
   ✅ Check that icons display correctly
   ✅ Verify quantity text shows "0" initially
   ```

2. **Console Check:**
   ```
   Expected logs:
   - "[CurrencyManager] Currency system initialized successfully!"
   - "[CurrencySectionController] Orbs section initialized with X items"
   - "[CurrencySectionController] Spirits section initialized with X items"
   - "[CurrencySectionController] Fragments section initialized with X items"
   ```

3. **Visual Validation:**
   - Currencies should be laid out horizontally
   - Icons should match CurrencyDatabase sprites
   - Text should be readable
   - Spacing should look clean

---

## Troubleshooting

### Issue: Currencies Not Appearing

**Possible Causes:**
1. CurrencyDatabase not assigned
   - **Fix:** Select CurrencyManager → Right-click → "Load Database from Resources"

2. Section controllers not assigned
   - **Fix:** Select CurrencyManager → Right-click → "Auto-Assign Sections"

3. Container not assigned in section
   - **Fix:** Select each section → Right-click CurrencySectionController → "Auto-Assign Container"

4. CurrencyPrefab missing CurrencyDisplayItem component
   - **Fix:** Open prefab → Add CurrencyDisplayItem component

---

### Issue: Icons Not Showing

**Possible Causes:**
1. CurrencyDisplayItem references not assigned
   - **Fix:** Open CurrencyPrefab → Right-click CurrencyDisplayItem → "Auto-Assign References"

2. CurrencyDatabase sprites are null
   - **Fix:** Open CurrencyDatabase.asset → Assign sprites to each currency

3. Image component disabled
   - **Fix:** Check that the Image child object is active

---

### Issue: Wrong Currencies in Wrong Sections

**Check Category Assignment:**
- Orbs Section → Category should be `Orbs`
- Spirits Section → Category should be `Spirits`
- Fragments Section → Category should be `Fragments`

**Category Mapping:**
```csharp
Orbs:      Types 0-8 (Orbs only)
Spirits:   Types 9-17 (All Spirits)
Fragments: Types 18-27 (Seals + Fragments)
```

---

### Issue: Layout Issues / Overlapping

**Check Horizontal Layout Group:**
```
✅ Spacing: 10-20 pixels
✅ Child Controls Size: Both checked
✅ Child Force Expand: Both unchecked
✅ Child Alignment: Middle Center
```

**Check Container RectTransform:**
- Should stretch to fit parent
- Anchors: 0-1 (stretch both X and Y)

---

## Advanced Usage

### Dynamically Update Currency Quantities

```csharp
// Get reference to CurrencyManager
CurrencyManager currencyManager = FindObjectOfType<CurrencyManager>();

// Update a specific currency
currencyManager.UpdateCurrency(CurrencyType.OrbOfGeneration, 150);

// Or refresh all displays
currencyManager.Refresh();
```

### Integrate with Game Systems

```csharp
public class CurrencyRewardSystem : MonoBehaviour
{
    [SerializeField] private CurrencyManager currencyManager;

    public void GiveCurrency(CurrencyType type, int amount)
    {
        // Update database
        CurrencyDatabase db = currencyManager.GetDatabase();
        CurrencyData currency = db.GetCurrency(type);
        
        if (currency != null)
        {
            currency.quantity += amount;
            
            // Update display
            currencyManager.UpdateCurrency(type, currency.quantity);
        }
    }
}
```

### Custom Filtering Logic

If you need to modify which currencies appear in each section, edit the `IsCurrencyInCategory()` method in `CurrencySectionController.cs`:

```csharp
private bool IsCurrencyInCategory(CurrencyType type)
{
    int typeIndex = (int)type;

    switch (category)
    {
        case CurrencyCategory.Orbs:
            // Customize your logic here
            return (typeIndex >= 0 && typeIndex <= 8) || 
                   (typeIndex >= 18 && typeIndex <= 24);
        
        // ... other cases
    }
}
```

### Add Tooltips (Future Enhancement)

To add hover tooltips showing currency descriptions:

1. Add a component to CurrencyPrefab that implements `IPointerEnterHandler`, `IPointerExitHandler`
2. Display a tooltip panel with `currency.description`
3. Use Unity's Event System for mouse detection

---

## Layout Variations

### Vertical Stacking (Alternative)

If you prefer sections stacked vertically:

1. Add **Vertical Layout Group** to the root `CurrencyDisplay`
2. Configure:
   ```
   Spacing: 20
   Child Alignment: Upper Center
   Child Controls Size: ✅ Both
   Child Force Expand: ❌ Both
   ```

### Grid Layout (High Currency Count)

For many currencies in one section:

1. Replace Horizontal Layout Group with **Grid Layout Group**
2. Configure:
   ```
   Cell Size: 60x60
   Spacing: 10x10
   Constraint: Fixed Column Count = 10
   ```

---

## Performance Notes

- **Efficient Updates:** Use `UpdateCurrency()` for single changes instead of `Refresh()`
- **Pooling:** Not needed unless you're dynamically adding/removing currency types
- **Batch Updates:** If updating multiple currencies, consider batching then calling `Refresh()` once

---

## Integration with EquipmentScreen_New

### Recommended Placement

```
EquipmentScreen_New
├── CardCarousel (your existing carousel)
├── EquipmentSlots (your equipment display)
└── CurrencyDisplay (new - add at top or bottom)
    ├── OrbsSection
    ├── SpiritsSection
    └── FragmentsSection
```

### Anchoring Suggestions

**Top Placement:**
```
Anchors: (0, 1) to (1, 1)  [Top-stretch]
Position Y: -10 to -160
```

**Bottom Placement:**
```
Anchors: (0, 0) to (1, 0)  [Bottom-stretch]
Position Y: 10 to 160
```

---

## Development Log

### Version 1.0 - November 1, 2025
- ✅ Created modular currency display system
- ✅ Three-section categorization:
  - Orbs (0-8): Crafting orbs
  - Spirits (9-17): Elemental/stat spirits
  - Seals & Fragments (18-27): Seals + Fragments
- ✅ Dynamic prefab instantiation
- ✅ Rarity-based visual theming
- ✅ Editor auto-assignment helpers
- ✅ Comprehensive documentation

### Future Enhancements
- [ ] Tooltip system on hover
- [ ] Currency gain/loss animations
- [ ] Sound effects for currency changes
- [ ] Filtering/sorting options
- [ ] Search functionality for many currencies

---

## Quick Reference

### Initialization Flow
```
1. CurrencyManager.Start()
2. → Initialize()
3.   → ValidateSetup()
4.   → Load CurrencyDatabase
5.   → orbsSection.InitializeSection()
6.     → GetCurrenciesForCategory()
7.     → SpawnCurrencyItem() for each
8.       → CurrencyDisplayItem.Initialize()
9.         → UpdateDisplay()
```

### Update Flow
```
UpdateCurrency(type, quantity)
  → Update in CurrencyDatabase
  → Determine section
  → section.UpdateCurrencyQuantity()
    → item.UpdateQuantity()
      → Update TextMeshProUGUI
```

---

## Support & Questions

For issues or questions about this system:
1. Check the Troubleshooting section above
2. Verify all references are assigned (use auto-assign context menus)
3. Check Unity Console for error messages
4. Review the Architecture section for component relationships

---

**End of Guide**

