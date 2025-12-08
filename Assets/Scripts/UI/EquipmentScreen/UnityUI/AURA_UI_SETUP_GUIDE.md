# Aura UI Setup Guide
## Step-by-Step Instructions for Aura Locker System

**Purpose:** Display owned auras in a grid and show active auras with their effects

---

## 🎯 What You're Building

An Aura Locker system with two main areas:
1. **Owned Auras Grid** (`DynamicArea/Auras/AuraContent`) - Grid showing all owned auras that can be activated
2. **Active Auras Display** (`AuraNavDisplay/ActiveAuras`) - Shows all currently active auras and their effects

---

## 🏗️ Hierarchy Structure

```
EquipmentScreenCanvas
├── DisplayWindow
│   └── AuraNavDisplay (NEW - Panel for aura navigation)
│       ├── ActiveAuras (Container for active aura entries)
│       │   ├── Header (Text: "Active Auras")
│       │   ├── Summary (Text: "Active Auras: X, Total Cost: Y")
│       │   └── ScrollView (for active aura entries)
│       │       └── Content
│       │           └── (ActiveAuraButtonUI instances created dynamically)
│       └── EffectDisplay (Container for effect display)
│           ├── Header (Text: "Active Aura Effects")
│           └── ScrollView (for effect entries)
│               └── Content
│                   └── (Effect entries created dynamically)
│
└── DynamicArea
    └── Auras (NEW - Panel for aura content)
        └── AuraContent (Attach AuraStorageUI.cs here)
            └── ScrollView (for owned auras grid)
                └── Viewport
                    └── Content (GridLayoutGroup - slots created dynamically)
                        └── (AuraSlotUI instances created automatically)
```

---

## 🛠️ Step 1: Create AuraNavDisplay Panel

1. **In DisplayWindow** (where EquipmentNavDisplay, EffigyNavDisplay, etc. are):
   - Right-click → Create Empty
   - Name it: `AuraNavDisplay`
   - Add `RectTransform` component (should be automatic)
   - Position it at the same location as other NavDisplay panels (they overlap)

2. **Create ActiveAuras Container:**
   - Right-click on `AuraNavDisplay` → UI → Panel
   - Name it: `ActiveAuras`
   - Add `VerticalLayoutGroup` component
   - Set spacing: 10

3. **Add Header:**
   - Right-click on `ActiveAuras` → UI → Text - TextMeshPro
   - Name it: `Header`
   - Text: "Active Auras"
   - Font Size: 18
   - Font Style: Bold

4. **Add Horizontal Layout for Aura Button Icons:**
   - Right-click on `ActiveAuras` → UI → Panel
   - Name it: `AuraButtonsContainer`
   - Add `HorizontalLayoutGroup` component:
     - Spacing: 10
     - Child Control Width: ☐ (unchecked)
     - Child Control Height: ☐ (unchecked)
   - This is where active aura button icons will appear (created dynamically)

6. **Create EffectDisplay Container:**
   - Right-click on `AuraNavDisplay` → UI → Panel
   - Name it: `EffectDisplay`
   - Add `VerticalLayoutGroup` component
   - Set spacing: 10

7. **Add EffectDisplay Header:**
   - Right-click on `EffectDisplay` → UI → Text - TextMeshPro
   - Name it: `Header`
   - Text: "Active Aura Effects"
   - Font Size: 18
   - Font Style: Bold

8. **Add ScrollView for Effects:**
   - Right-click on `EffectDisplay` → UI → Scroll View
   - Name it: `EffectsScrollView`
   - Configure:
     - Horizontal: ☐ (unchecked)
     - Vertical: ✓ (checked)
   - In the `Content` child:
     - Add `VerticalLayoutGroup`
     - Set spacing: 5
     - Set `ContentSizeFitter`:
       - Horizontal: Unconstrained
       - Vertical: Preferred Size

---

## 🛠️ Step 2: Create DynamicArea/Auras Panel

1. **In DynamicArea** (where Equipment, Effigy, Character panels are):
   - Right-click → Create Empty
   - Name it: `Auras`
   - Add `RectTransform` component

2. **Create RelianceDisplay Header:**
   - Right-click on `Auras` → UI → Panel
   - Name it: `RelianceHeader`
   - Add `HorizontalLayoutGroup` component
   - Set spacing: 10
   
   - **Add "Reliance" Text:**
     - Right-click on `RelianceHeader` → UI → Text - TextMeshPro
     - Name it: `RelianceLabel`
     - Text: "Reliance"
     - Font Size: 16
     - Font Style: Bold
   
   - **Add Reliance Counter:**
     - Right-click on `RelianceHeader` → UI → Text - TextMeshPro
     - Name it: `RelianceCounter`
     - Text: "200/200"
     - Font Size: 14
   
   - **Add RelianceDisplay Image (Filled):**
     - Right-click on `RelianceHeader` → UI → Image
     - Name it: `RelianceDisplay`
     - Set Image Type: **Filled**
     - Set Fill Method: Horizontal (or Vertical, depending on design)
     - Set Fill Amount: 1.0 (will be bound to currentReliance/maxReliance)
     - Assign a sprite/color for the fill

3. **Create AuraContent:**
   - Right-click on `Auras` → UI → Scroll View
   - Name it: `AuraContent`
   - Configure:
     - Horizontal: ☐ (unchecked)
     - Vertical: ✓ (checked)
   - In the `Content` child (inside Viewport):
     - Add `GridLayoutGroup` (will be configured by script)
     - Add `ContentSizeFitter`:
       - Horizontal: Unconstrained
       - Vertical: Preferred Size

---

## 🛠️ Step 3: Attach Scripts

1. **AuraStorageUI:**
   - Select `AuraContent` (the ScrollView)
   - Add Component → `AuraStorageUI`
   - Assign:
     - `Grid Container`: The `Content` child (inside Viewport)
     - `Slot Prefab`: (Optional - can create dynamically)
     - `Grid Columns`: 4
     - `Grid Rows`: 10
     - `Cell Size`: 100
     - `Cell Spacing`: 10
     - `Grid Padding`: 10

2. **ActiveAurasDisplay:**
   - Select `AuraNavDisplay`
   - Add Component → `ActiveAurasDisplay`
   - Assign:
     - `Active Auras Container`: The `AuraButtonsContainer` (horizontal layout)
     - `Aura Button Prefab`: (Optional - can create dynamically)
     - `Effect Display Container`: The `EffectDisplay` panel
     - `Header Text`: (Optional header text)

3. **RelianceDisplay:**
   - Select `RelianceHeader` (or the GameObject with RelianceDisplay image)
   - Add Component → `RelianceDisplay`
   - Assign:
     - `Reliance Text`: The "Reliance" label text (optional)
     - `Reliance Counter Text`: The counter text showing "Current/Max"
     - `Reliance Fill Image`: The `RelianceDisplay` Image component (Filled type)

4. **AuraManagerUI:**
   - Select `AuraNavDisplay` (or create a manager GameObject)
   - Add Component → `AuraManagerUI`
   - Assign:
     - `Aura Storage`: The `AuraStorageUI` component
     - `Active Auras Display`: The `ActiveAurasDisplay` component

---

## 🛠️ Step 4: Add to PanelNavigationController

1. **Find PanelNavigationController** (usually on NavigationBar)
2. **Add new NavigationItem:**
   - `Navigation Button`: Create/assign an "Auras" button
   - `Display Panel`: Drag `AuraNavDisplay` here
   - `Dynamic Panel`: Drag `DynamicArea/Auras` here
   - Set button colors/sprites as desired

---

## 🎨 Step 5: Create Aura Slot Prefab (Optional)

If you want a custom prefab for aura slots:

1. **Create Prefab:**
   - Right-click in Project → Create → UI → Panel
   - Name it: `AuraSlotPrefab`

2. **Structure:**
   ```
   AuraSlotPrefab
   ├── Background (Image)
   ├── Icon (Image)
   ├── Name (TextMeshProUGUI)
   ├── ActiveIndicator (GameObject - optional visual)
   └── LockedIndicator (GameObject - optional visual)
   ```

3. **Add Component:**
   - Add `AuraSlotUI` component
   - Assign all UI references

4. **Assign to AuraStorageUI:**
   - In `AuraStorageUI` component, assign this prefab to `Slot Prefab`

---

## ✅ Testing

1. **Ensure RelianceAuraDatabase is set up:**
   - Create a GameObject with `RelianceAuraDatabase` component
   - Set `Resources Path` to "RelianceAuras"
   - Ensure auras are in `Resources/RelianceAuras/` folder

2. **Ensure Character has owned auras:**
   - In CharacterManager or Character data, add some aura names to `ownedRelianceAuras` list

3. **Test activation:**
   - Click on an owned aura in the grid
   - It should appear in ActiveAurasDisplay
   - Effects should appear in EffectDisplay

---

## 📝 Notes

- **ActiveAuras Display:**
  - Uses horizontal layout with button icons
  - Clicking an aura button selects it and shows its details in EffectDisplay
  - Clicking the same button again deselects it (shows all effects)
  - When no aura is selected, EffectDisplay shows all active aura effects

- **RelianceDisplay:**
  - Updates automatically based on character's current/max reliance
  - Fill image type must be set to "Filled" in the Image component
  - Fill amount is calculated as: currentReliance / maxReliance

- **General:**
  - The system automatically loads auras from `RelianceAuraDatabase`
  - Auras must be in the character's `ownedRelianceAuras` list to be activatable
  - Active auras are stored in character's `activeRelianceAuras` list
  - Reliance cost checking is a TODO - currently auras activate without cost validation

