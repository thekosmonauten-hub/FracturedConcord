# Armor CSV Importer Guide

## 🎯 Overview

The **Armor CSV Importer** is an editor tool that allows you to bulk import armor from a CSV file and automatically create `Armour` ScriptableObjects with **full character stat integration**.

### ✨ **Key Features**:
- **🎯 Accurate Stat Mapping**: Implicit modifiers now map directly to your character stats (e.g., "-1 Card draw" → `cardsDrawnPerTurn`)
- **🏷️ Smart Tagging**: Armor gets proper tags for slot type, defense type, and effects  
- **🤝 Equipment Integration**: All modifiers connect properly to your `EquipmentManager` and `CharacterStatsData` system
- **🛡️ Defense Type Detection**: Automatically categorizes as Pure Armour, Pure Evasion, Pure Energy Shield, or Hybrid
- **📁 Flexible Organization**: Optional subfolders by slot type and defense type

## 📋 CSV Format

Your CSV should have exactly **8 columns**:

```csv
Name,Defence,Defence 2,Defence 3,Requirements,Implicit 1,Implicit 2,Item Type
Dreamquest Slippers,Energy Shield: 50–57,,,Requires Level 80, 124 Int,-1 Card draw,Select (1-2) additional Card to Discard,Boots
Sacrificial Garb,Armour: 329–378,Evasion Rating: 329–378,Energy Shield: 67–77,Requires Level 72, 66 Str, 66 Dex, 66 Int,+1 to Level of all Void Cards,,Body Armour
```

### 📊 Defense Columns (Defence, Defence 2, Defence 3)

Each defense column can contain one of:
- `Armour: 50-60` or `Armour: 55`
- `Evasion Rating: 100-120` or `Evasion Rating: 110`  
- `Energy Shield: 25-30` or `Energy Shield: 28`
- *(empty)* - No defense value

### 🎽 Item Types

Map to armor slots:
- `Helmet` → Helmet slot
- `Body Armour` → Body Armour slot  
- `Gloves` → Gloves slot
- `Boots` → Boots slot
- `Shield` → Shield slot (OffHand equipment type)

### 📋 Requirements Format

- `"Requires Level 20, 30 Str, 25 Dex"`
- `"requires: Level 23, 28 Str, 28 Dex"` (lowercase/colon variations supported)

### ⚡ Implicit Modifiers

Examples of supported patterns:
- **Card System**: `-1 Card draw`, `+2 Card draw` → `cardsDrawnPerTurn`
- **Card Selection**: `Select (1-2) additional Card to Discard` → `additionalCardDiscardSelection`
- **Card Levels**: `+1 to Level of all Void Cards` → `voidCardLevelBonus`
- **Resistances**: `+(10-15)% to all Resistances` → `allResistance`
- **Stats**: `10% increased Movement Speed` → `movementSpeed`

## 📋 How to Use

### Step 1: Prepare Your CSV File

1. Place your CSV file in `Assets/Resources/CSV/` folder
2. Ensure it follows the 8-column format above
3. Use quotes around requirements if they contain commas

### Step 2: Open the Importer

1. In Unity, go to **Tools → Dexiled → Import Armor from CSV**
2. A new window will appear with import options

### Step 3: Configure Import Settings

**CSV File**: Select your CSV file from `Assets/Resources/CSV/`

**Output Settings**:
- **Output Folder**: Where to save the created armor (default: `Assets/Resources/Items/Armour`)
- **Create Subfolders by Slot**: Creates subfolders like `Helmets/`, `Boots/`, etc.
- **Separate by Slot Type**: Further organizes by defense type (`Armour/`, `Evasion/`, `Hybrid/`, etc.)
- **Overwrite Existing Assets**: ⚠️ **IMPORTANT** - Leave UNCHECKED to preserve armor you've already customized

**Armor Slot Filters**:
- **Filter by Armor Slot**: Enable to import only specific armor slots
  - Helmets, Body Armour, Gloves, Boots, Shields
  - Use "Select All" / "Deselect All" for quick toggling
  - Each slot shows the count: e.g., "Helmets (15)"

**Defense Type Filters**:
- **Filter by Defense Type**: Enable to import only specific defense types
  - Pure Armour: Only has Armour defense
  - Pure Evasion: Only has Evasion Rating defense  
  - Pure Energy Shield: Only has Energy Shield defense
  - Hybrid Defense: Has multiple defense types

### Step 4: Preview Import

1. Click **"Preview Import"** to see what will be imported
2. Review the preview to ensure everything is parsed correctly
3. The preview shows:
   - Total count and filtered count
   - Breakdown by slot type
   - Breakdown by defense type
   - First 5 items with full details

### Step 5: Import Armor

1. Click **"Import Armor"** to create the assets
2. Assets will be created in the specified output folder
3. Existing assets will be skipped (if overwrite is disabled)

## 📁 Folder Organization Examples

### Basic Organization (Create Subfolders = ✅)
```
Items/Armour/
├── Helmets/
│   ├── Wolf Pelt.asset
│   └── Visored Sallet.asset
├── Boots/
│   └── Dreamquest Slippers.asset
└── Body Armour/
    └── Sacrificial Garb.asset
```

### Advanced Organization (+ Separate by Slot Type = ✅)
```
Items/Armour/
├── Helmets/
│   ├── Evasion/
│   │   └── Wolf Pelt.asset
│   └── Hybrid/  
│       └── Visored Sallet.asset
├── Boots/
│   └── EnergyShield/
│       └── Dreamquest Slippers.asset
└── Body Armour/
    └── Hybrid/
        └── Sacrificial Garb.asset
```

## 🏷️ Tags Applied

The importer automatically adds comprehensive tags to each armor piece:

**Base Tags:**
- `"armour"`, `"defence"` - All armor
- Slot type: `"helmet"`, `"bodyarmour"`, `"gloves"`, `"boots"`, `"shield"`
- Armor type: `"cloth"`, `"leather"`, `"plate"`, `"chain"`, etc.

**Defense Type Tags:**
- `"armor"` - Has Armour defense
- `"evasion"` - Has Evasion Rating defense
- `"energyshield"` - Has Energy Shield defense
- `"hybrid"` - Has multiple defense types

**Requirement Tags:**
- `"strength"`, `"dexterity"`, `"intelligence"` - Based on requirements

**Effect Tags (from implicit modifiers):**
- `"carddraw"` - Card draw effects
- `"cardlevel"` - Card level bonuses
- `"discard"` - Card discard effects
- `"void"` - Void card effects
- `"resistance"` - Resistance bonuses

## 📊 Comprehensive Stat Mapping

The importer correctly maps CSV implicit modifiers to your actual character stat system:

### Card System Examples:
- `"-1 Card draw"` → `cardsDrawnPerTurn` stat (reduces cards drawn)
- `"Select (1-2) additional Card to Discard"` → `additionalCardDiscardSelection` stat
- `"+1 to Level of all Void Cards"` → `voidCardLevelBonus` stat

### Defense Examples:
- `"+(10-15)% to all Resistances"` → `allResistance` stat
- `"10% increased Movement Speed"` → `movementSpeed` stat
- `"+15 to Strength"` → `strength` stat

**✅ All modifiers now connect properly to your equipment system!**

## 🚀 Tips for Best Results

### Avoid Duplicates/Overwrites:
1. **Always keep "Overwrite Existing Assets" unchecked** if you've customized any armor (added sprites, etc.)
2. Use **Slot Filters** to import only specific types you need
3. Use **Defense Type Filters** to focus on specific defense categories

### Organize Your Collection:
1. Enable **"Create Subfolders by Slot"** for better organization
2. Enable **"Separate by Slot Type"** if you have many items and want fine-grained organization
3. Use the **Preview** feature to verify parsing before importing

### Workflow Example:
```
1. Import Helmets first → Check results → ✅
2. Import Body Armour → Check results → ✅  
3. Import Boots and Gloves → Final review → ✅
4. All armor now has proper stats and connects to character system! 🎯
```

## 🔧 Troubleshooting

**"Invalid CSV line (insufficient columns)"**
- Ensure your CSV has exactly 8 columns
- Check for missing commas or extra commas

**"Could not parse armor implicit modifier"**
- Check that implicit modifiers follow supported patterns
- Complex modifiers may need custom pattern additions

**"No armor pieces match the current filters"**
- Check your filter settings
- Use "Preview Import" to see what's being filtered out

**Missing defense values**
- Ensure defense strings follow the format: `"Armour: 50-60"` or `"Energy Shield: 25"`
- Check for typos in "Armour", "Evasion Rating", "Energy Shield"

---

The armor importer is now fully integrated with your game's stat and equipment systems! 🛡️








