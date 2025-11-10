# Weapon CSV Importer Guide

## 🎯 Overview

The **Weapon CSV Importer** is an editor tool that allows you to bulk import weapons from a CSV file and automatically create `WeaponItem` ScriptableObjects with **full character stat integration**.

### ✨ **Latest Improvements**:
- **🎯 Accurate Stat Mapping**: Implicit modifiers now map directly to your character stats (e.g., "+(1-2) Wave card draw" → `cardsDrawnPerWave`)
- **🏷️ Smart Tagging**: Weapons get proper tags for weapon type, handedness, combat style, and effects  
- **🤝 Equipment Integration**: All modifiers connect properly to your `EquipmentManager` and `CharacterStatsData` system

## 📋 How to Use

### Step 1: Prepare Your CSV File

1. Place your CSV file in `Assets/Resources/CSV/` folder
2. The CSV should have the following format:

```csv
Name,Damage,Critical Strike Chance,Attacks,Requirements,Implicit 1,Implicit 2
Weathered Club,Physical Damage: 6-8,Critical Strike Chance: 5%,Attack Speed: 1.45,Requires 14 Str,10% reduced Enemy Stagger Threshold,
```

### Step 2: Open the Importer

1. In Unity, go to **Tools → Dexiled → Import Weapons from CSV**
2. A new window will appear with import options

### Step 3: Configure Import Settings

**CSV File**: Select your CSV file from `Assets/Resources/CSV/`

**Import Settings**:
- **Output Folder**: Where to save the created weapons (default: `Assets/Resources/Items/Weapons`)
- **Create Subfolders by Type**: Creates subfolders like `Maces/`, `Bows/`, etc.
- **Overwrite Existing Assets**: ⚠️ **IMPORTANT** - Leave UNCHECKED to preserve weapons you've already customized (added sprites, etc.)

**Weapon Type Filters**:
- **Filter by Weapon Type**: Enable to import only specific weapon types
  - Maces, Sceptres, Wands, Daggers, Claws, Bows, Axes, Swords, Staves
  - Use "Select All" / "Deselect All" for quick toggling
  - Each type shows the count: e.g., "Maces (26)"

**Handedness Filters**:
- **Filter by Handedness**: Enable to import only one-handed or two-handed weapons
  - One Handed (130 weapons)
  - Two Handed (24 weapons)

### Step 4: Preview Import

1. Click **"Preview Import"** to see what will be imported
2. Review the preview to ensure everything is parsed correctly
3. The preview shows the first 10 weapons with all their properties

### Step 5: Import Weapons

1. Click **"Import Weapons"** to create all weapon assets
2. Wait for the import to complete
3. A dialog will show you how many weapons were imported

## 📊 CSV Format

### Required Columns

1. **Name** - Weapon name (e.g., "Weathered Club")
2. **Damage** - Physical damage range (e.g., "Physical Damage: 6-8")
3. **Critical Strike Chance** - Base crit chance (e.g., "Critical Strike Chance: 5%")
4. **Attacks** - Attack speed (e.g., "Attack Speed: 1.45")
5. **Requirements** - Level and attribute requirements (e.g., "Requires Level 5, 26 Str")
6. **Implicit 1** - First implicit modifier (optional)
7. **Implicit 2** - Second implicit modifier (optional)

### Example Rows

```csv
Tribal Bludgeon,Physical Damage: 8-13,Critical Strike Chance: 5%,Attack Speed: 1.4,Requires Level 5, 26 Str,10% reduced Enemy Stagger Threshold,
Quartz Wand,Physical Damage: 12-23,Critical Strike Chance: 9%,Attack Speed: 1.45,Requires Level 18, 65 Int,Adds (2-3) to (4-7) Cold Damage to Spells and Attacks,
```

## 🔧 Automatic Weapon Type Detection

The importer automatically detects weapon types based on name keywords:

- **Bow**: Contains "bow"
- **Claw**: Contains "claw", "talon", "fang" (with claw)
- **Dagger**: Contains "dagger", "knife", "stiletto"
- **Mace**: Contains "mace", "club", "maul", "hammer", "mallet", "gavel"
- **Sceptre**: Contains "sceptre", "rod", "baton", "sekhem", "fetish", "idol"
- **Wand**: Contains "wand", "fang" (without claw)
- **Staff**: Contains "staff"
- **Sword**: Contains "sword", "blade"
- **Axe**: Contains "axe"

## 📝 Implicit Modifier Parsing

The importer can parse common implicit modifier patterns:

### Supported Patterns:

1. **Increased/Reduced Modifiers**
   - `"10% increased Elemental Damage"` → +10% Elemental Damage
   - `"10% reduced Enemy Stagger Threshold"` → -10% Enemy Stagger Threshold

2. **Flat Bonuses**
   - `"Grants 3 Life per Enemy Hit"` → +3 Life on Hit
   - `"Grants 6 Mana per Enemy Hit"` → +6 Mana on Hit

3. **Range Modifiers**
   - `"+(15-25)% to Global Critical Strike Multiplier"` → +15-25% Crit Multiplier

4. **Added Damage**
   - `"Adds (2-3) to (4-7) Cold Damage to Spells and Attacks"` → +2-3 to 4-7 Cold Damage

5. **Chance Modifiers**
   - `"4% Chance to Block Attack Damage"` → +4% Block Chance

6. **Penetration**
   - `"Damage Penetrates 4% Elemental Resistances"` → +4% Elemental Penetration

7. **Leech**
   - `"1.6% of Physical Attack Damage Leeched as Life"` → 1.6% Life Leech

## 🎨 Weapon Properties Set

For each weapon, the importer sets:

### Basic Properties:
- Item Name
- Description (auto-generated)
- Weapon Type (auto-detected)
- Item Type: Weapon
- Equipment Type: Weapon

### Damage:
- Min/Max Damage
- Primary Damage Type: Physical

### Stats:
- Attack Speed
- Critical Strike Chance
- Critical Strike Multiplier: 150% (default)

### Requirements:
- Required Level
- Required Strength
- Required Dexterity
- Required Intelligence

### Handedness:
- **Two-Handed**: Bows, Staves
- **One-Handed**: All other weapons

### Rarity:
- Normal (base rarity)

## 📁 Output Structure

With **Create Subfolders by Type** enabled:

```
Assets/Resources/Items/Weapons/
├── Maces/
│   ├── WeatheredClub.asset
│   ├── TribalBludgeon.asset
│   └── ...
├── Wands/
│   ├── WornWand.asset
│   ├── QuartzWand.asset
│   └── ...
├── Bows/
│   ├── CrudeBow.asset
│   └── ...
└── ...
```

## 🔍 Troubleshooting

### "No weapons found in CSV"
- Check that your CSV file has the correct format
- Ensure the header row is present
- Verify there are no extra blank lines

### "Weapon type not detected correctly"
- The weapon name should contain a keyword from the detection list
- You can manually change the weapon type after import

### "Implicit modifiers not parsing"
- Check the implicit description format matches one of the supported patterns
- You can manually add/edit implicit modifiers after import
- The description will still be preserved even if parsing fails

## 💡 Tips

1. **Preview First**: Always use the Preview feature before importing to check the data
2. **Avoid Overwriting Custom Weapons**: Keep "Overwrite Existing Assets" UNCHECKED if you've added sprites or customized weapons
3. **Import by Type**: Use weapon type filters to import only new weapon categories
4. **Import by Handedness**: Use handedness filters to import one-handed and two-handed weapons separately
5. **Check Weapon Types**: After import, spot-check a few weapons to ensure types are correct
6. **Manual Adjustments**: You can always manually edit the created assets in the Inspector
7. **Icons**: Add weapon icons to the created assets manually (icon field is left empty)

## 🔄 Example Workflows

### Workflow 1: Initial Import (All Weapons)
1. Open importer
2. Select CSV file
3. Leave filters disabled (import all)
4. Leave "Overwrite Existing" UNCHECKED
5. Import all weapons

### Workflow 2: Import Only Specific Types (e.g., Just Bows and Axes)
1. Open importer
2. Enable "Filter by Weapon Type"
3. Click "Deselect All"
4. Check only "Bows" and "Axes"
5. Preview to verify
6. Import selected types

### Workflow 3: Import Only Two-Handed Weapons
1. Open importer
2. Enable "Filter by Handedness"
3. Uncheck "One Handed"
4. Check "Two Handed"
5. Import (only bows and two-handed weapons)

### Workflow 4: Re-Import After Adding New Weapons to CSV (Avoid Overwriting)
1. Open importer
2. Keep "Overwrite Existing Assets" UNCHECKED ✓
3. Import - only NEW weapons will be created
4. Existing weapons with sprites are preserved!

## 🚀 Future Enhancements

Potential improvements for the importer:
- Support for weapon sprite/icon assignment
- Support for secondary damage types
- Import from multiple CSV files at once
- Custom weapon type mapping configuration
- More implicit modifier patterns
- Support for Excel files (.xlsx)

---

**Created by**: Agent AI  
**Last Updated**: October 25, 2025

