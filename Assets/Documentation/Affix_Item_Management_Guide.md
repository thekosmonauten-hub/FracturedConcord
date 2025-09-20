# Affix and Item Management Guide

## Overview
This guide covers the comprehensive affix and item management system, including the hierarchical affix organization, custom editors, and the redesigned tag system.

## Table of Contents
1. [AffixDatabaseEditor](#affixdatabaseeditor)
2. [ItemAffixEditor](#itemaffixeditor)
3. [ItemDatabaseEditor](#itemdatabaseeditor)
4. [Redesigned Tag System](#redesigned-tag-system)
5. [Quick Start Guide](#quick-start-guide)
6. [Troubleshooting](#troubleshooting)

## AffixDatabaseEditor

### Features
- **Hierarchical Navigation**: Drill down through categories and sub-categories
- **Direct Editing**: Modify affix properties directly in the inspector
- **Management Tools**: Add, remove, and duplicate affixes
- **Statistics Display**: View affix counts and organization
- **Bulk Import**: Import affixes from clipboard using tab-separated format

### Usage
1. Select the `AffixDatabase` asset in the Project window
2. Use the hierarchical foldouts to navigate categories
3. **Individual Removal**: Click "Remove" buttons next to specific items
4. **Bulk Removal**: Use the "Quick Remove" section for common operations
5. Edit affix properties directly in the inspector
6. Use management buttons for bulk operations
7. Use bulk import for adding multiple affixes at once

### Buttons
- **Add Physical Damage Affixes**: Creates organized physical damage affixes
- **Add Elemental Damage Affixes**: Creates organized elemental damage affixes
- **Import Affixes from Clipboard**: Bulk import from tab-separated data
- **Show Import Format**: Displays the expected format for bulk import
- **Remove All Elemental**: Removes all Elemental categories from all affix types
- **Remove All Physical**: Removes all Physical categories from all affix types
- **Remove Empty Categories**: Removes categories that have no subcategories
- **Remove Duplicate Affixes**: Removes duplicate affixes based on name and location
- **Clear All [Type]**: Removes all affixes of a specific type

### Granular Removal Options
The inspector now provides multiple levels of removal control:

#### **Individual Level**
- **Remove Affix**: Remove a specific affix from its subcategory
- **Remove Sub**: Remove an entire subcategory and all its affixes
- **Remove Category**: Remove an entire category and all its contents

#### **Bulk Operations**
- **Remove All Elemental**: Quickly remove all Elemental categories (Fire, Cold, Lightning, Chaos)
- **Remove All Physical**: Quickly remove all Physical categories
- **Remove Empty Categories**: Clean up categories that have no content
- **Remove Duplicate Affixes**: Remove duplicate affixes that have the same name and location

### Bulk Import System

The bulk import system allows you to quickly add multiple affixes by copying tab-separated data from a spreadsheet or text file.

#### Import Format
```
Affix Slot	Name	Item Level	Stat	Tags	Weapon Types
Suffix	of Skill	1	(5-7)% increased Attack Speed	Attack, Speed	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Humming	3	Adds 1 to (5-6) Lightning Damage	Damage, Elemental, Lightning, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
```

#### Format Rules
- **Tab-separated columns** (copy from Excel/Google Sheets)
- **Affix Slot**: Prefix or Suffix
- **Name**: Affix name
- **Item Level**: Required item level (number)
- **Stat**: The stat description
- **Tags**: Comma-separated tags
- **Weapon Types**: Comma-separated weapon types

#### Supported Weapon Types
- Sword, Axe, Mace, Dagger, Claw, RitualDagger, Bow, Wand, Staff, Sceptre

#### Supported Tags
- weapon, attack, melee, ranged, spell, damage, elemental, physical, fire, cold, lightning, chaos, speed, etc.

#### How to Use Bulk Import
1. **Prepare your data** in the correct format (tab-separated)
2. **Copy the data** to your clipboard
3. **Select AffixDatabase** in the Project window
4. **Click "Import Affixes from Clipboard"**
5. **Review results** in the popup dialog

#### Automatic Features
- **Tier Assignment**: Automatically determines affix tier based on item level
- **Modifier Parsing**: Parses stat descriptions to create proper AffixModifier objects
- **Category Organization**: Automatically organizes affixes into appropriate categories
- **Elemental Subcategorization**: Properly separates Fire, Cold, Lightning, and Chaos damage into granular subcategories (e.g., "Fire Flat", "Fire Increased", "Cold Flat", "Cold Increased")
- **Tag Management**: Sets compatibility tags based on weapon types and additional tags
- **Duplicate Detection**: Automatically skips affixes that already exist in the database
- **Error Handling**: Provides detailed feedback on import success/failures and skipped duplicates

#### Example Data
You can use the `AffixImportTest` script to get example data:
1. Add `AffixImportTest` component to any GameObject
2. Right-click the component and select "Copy Example Data to Clipboard"
3. Use the copied data with the import system

## ItemAffixEditor

### Features
- **Quick Affix Generation**: Generate random affixes with one click
- **Manual Affix Selection**: Browse and select specific affixes
- **Current Affix Management**: View and remove existing affixes
- **Tag Management**: Auto-set and manage item tags

### Usage
1. Select any `BaseItem` or derived item (Weapon, Armour, etc.)
2. Scroll down to "Affix Management" section
3. Use "Auto-Set Tags" to configure item compatibility
4. Generate random affixes or manually select specific ones

### Sections
- **Quick Affix Generation**: Buttons for random generation
- **Item Tags Management**: Display and manage item tags
- **Manual Affix Selection**: Hierarchical affix browser
- **Manual Affix Creation**: Create custom affixes with manual value control
- **Current Affixes**: View existing affixes with removal options and value sliders

## **OneHand and TwoHand Tag System**

### **Overview**
The system now supports OneHand and TwoHand tags to ensure that affixes are only compatible with weapons of the appropriate handedness. This prevents inappropriate affixes from being applied to the wrong weapon types.

### **How Handedness Tags Work**

#### **1. Weapon Tag Assignment**
Weapons automatically receive handedness tags based on their `WeaponHandedness`:
- **One-handed weapons**: Get `"onehanded"` tag
- **Two-handed weapons**: Get `"twohanded"` tag

#### **2. Affix Tag Requirements**
Affixes can specify handedness requirements:
- **OneHand affixes**: Require `"onehanded"` tag
- **TwoHand affixes**: Require `"twohanded"` tag

#### **3. Compatibility Logic**
The system checks compatibility with special handling for handedness:
```csharp
// OneHand affixes only work with one-handed weapons
if (hasOneHandRequirement && !item.itemTags.Contains("onehanded"))
    return false;

// TwoHand affixes only work with two-handed weapons  
if (hasTwoHandRequirement && !item.itemTags.Contains("twohanded"))
    return false;
```

### **Weapon Handedness Examples**

#### **One-Handed Weapons**
- **Sword**: `weapon, sword, onehanded, melee, attack`
- **Axe**: `weapon, axe, onehanded, melee, attack`
- **Mace**: `weapon, mace, onehanded, melee, attack`
- **Dagger**: `weapon, dagger, onehanded, melee, attack`
- **Claw**: `weapon, claw, onehanded, melee, attack`
- **Ritual Dagger**: `weapon, ritualdagger, onehanded, melee, attack`

#### **Two-Handed Weapons**
- **Staff**: `weapon, staff, twohanded, spell, spell`
- **Bow**: `weapon, bow, twohanded, ranged, attack`
- **Two-Handed Sword**: `weapon, sword, twohanded, melee, attack`
- **Two-Handed Axe**: `weapon, axe, twohanded, melee, attack`

### **Affix Compatibility Examples**

#### **OneHand Affixes**
- **Required Tags**: `[weapon, attack, OneHand]`
- **Compatible With**: One-handed weapons only
- **Category Structure**: `OneHand/Physical Flat`, `OneHand/Fire Increased`, etc.
- **Examples**:
  - "Adds (3-5) to (8-10) Physical Damage" (OneHand)
  - "(10-15)% increased Attack Speed" (OneHand)

#### **TwoHand Affixes**
- **Required Tags**: `[weapon, attack, TwoHand]`
- **Compatible With**: Two-handed weapons only
- **Category Structure**: `TwoHand/Physical Flat`, `TwoHand/Fire Increased`, etc.
- **Examples**:
  - "Adds (6-8) to (12-15) Physical Damage" (TwoHand)
  - "(15-20)% increased Attack Speed" (TwoHand)

### **Categorization Structure**

The system now creates handedness-based categories for better organization:

```
Weapon Prefixes:
├── OneHand
│   ├── Physical Flat
│   ├── Physical Increased
│   ├── Fire Flat
│   ├── Fire Increased
│   ├── Cold Flat
│   ├── Cold Increased
│   ├── Lightning Flat
│   ├── Lightning Increased
│   ├── Chaos Flat
│   └── Chaos Increased
├── TwoHand
│   ├── Physical Flat
│   ├── Physical Increased
│   ├── Fire Flat
│   ├── Fire Increased
│   ├── Cold Flat
│   ├── Cold Increased
│   ├── Lightning Flat
│   ├── Lightning Increased
│   ├── Chaos Flat
│   └── Chaos Increased
├── Physical (Universal)
├── Elemental (Universal)
└── [Other categories]
```

**Benefits of Handedness Categorization:**
- **📁 Clear Organization**: Separate categories for one-handed and two-handed affixes
- **🎯 Easy Selection**: Quickly find affixes for specific weapon types
- **🔍 Better Filtering**: Filter by handedness when selecting affixes
- **📊 Logical Structure**: Handedness → Damage Type → Modifier Type hierarchy

### **Compatibility Matrix**

| Affix Type | One-Handed Weapon | Two-Handed Weapon |
|------------|-------------------|-------------------|
| OneHand    | ✅ Compatible     | ❌ Incompatible   |
| TwoHand    | ❌ Incompatible   | ✅ Compatible     |
| Universal  | ✅ Compatible     | ✅ Compatible     |

### **Benefits**
- **🎯 Precision**: Prevents inappropriate affixes on wrong weapon types
- **⚖️ Balance**: Maintains game balance and logic
- **🔒 Safety**: Clear separation between one-handed and two-handed affixes
- **🤖 Automation**: Automatic compatibility checking in the editor
- **📊 Clarity**: Clear feedback when affixes are incompatible

### **Testing Handedness Compatibility**
1. **Create One-Handed Weapon**: Create a Sword or Dagger
2. **Test OneHand Affix**: Should be compatible
3. **Test TwoHand Affix**: Should show "No compatible affixes"
4. **Create Two-Handed Weapon**: Create a Staff or Bow
5. **Test TwoHand Affix**: Should be compatible
6. **Test OneHand Affix**: Should show "No compatible affixes"

### **Bulk Import Format**
When importing affixes with handedness requirements, include the appropriate handedness value:
```
Prefix	OneHanded	15	Adds (3-5) to (8-10) Physical Damage	OneHand	Damage, Physical, Attack	Sword, Axe, Mace, Dagger, Claw, RitualDagger	Local
Prefix	TwoHanded	20	Adds (6-8) to (12-15) Physical Damage	TwoHand	Damage, Physical, Attack	Staff, Bow	Local
```

### **New Import Format**
The bulk import system now uses a dedicated Handedness field:

**Format**: `Affix Slot | Name | Item Level | Stat | Handedness | Tags | Weapon Types | Scope`

**Handedness Values**:
- **Both**: Compatible with both one-handed and two-handed weapons
- **OneHand**: Only compatible with one-handed weapons  
- **TwoHand**: Only compatible with two-handed weapons

**Example**:
```
Prefix	OneHandPhysical	15	Adds (3-5) to (8-10) Physical Damage	OneHand	Damage, Physical, Attack	Sword, Axe, Mace	Local
Prefix	TwoHandPhysical	25	Adds (6-8) to (12-15) Physical Damage	TwoHand	Damage, Physical, Attack	Staff, Bow	Local
```

**Benefits**:
- **🎯 Cleaner Separation**: Handedness is now a dedicated field, not mixed with tags
- **🔄 Duplicate Prevention**: Same affix name can exist for different handedness (not considered duplicates)
- **📋 Explicit Format**: More clear and structured import format
- **📊 Better Organization**: Improved categorization in the database

## **Dual-Range Modifiers**

### **Overview**
Some affixes have dual ranges that need to be rolled independently, such as "Adds (6-9) to (13-15) Physical Damage". The system now supports these complex modifiers with proper independent rolling.

### **How Dual-Range Modifiers Work**

#### **1. Structure**
- **First Range**: The minimum damage range (e.g., 6-9)
- **Second Range**: The maximum damage range (e.g., 13-15)
- **Independent Rolling**: Each range is rolled separately
- **Final Result**: "Adds 7 to 14 Physical Damage" (example)

#### **2. Parsing**
The bulk import system automatically detects dual-range patterns:
- **Pattern**: `Adds (6-9) to (13-15) Physical Damage`
- **Detection**: Regex pattern `\((\d+)-(\d+)\)\s+to\s+\((\d+)-(\d+)\)`
- **Extraction**: Four values (firstMin, firstMax, secondMin, secondMax)

#### **3. Rolling System**
```csharp
// Each range is rolled independently
float firstValue = Random.Range(firstMin, firstMax + 1);
float secondValue = Random.Range(secondMin, secondMax + 1);
// Result: Adds firstValue to secondValue Damage
```

#### **4. Slider Controls**
Dual-range modifiers have separate controls:
- **Min Value Slider**: Controls the first range (6-9)
- **Max Value Slider**: Controls the second range (13-15)
- **Randomize Both**: Rolls both ranges independently
- **Range Display**: Shows original ranges for reference

### **Examples of Dual-Range Modifiers**

#### **Physical Damage Affixes**
- **Polished**: Adds (6-9) to (13-15) Physical Damage
- **Honed**: Adds (8-12) to (17-20) Physical Damage
- **Gleaming**: Adds (11-14) to (21-25) Physical Damage
- **Annealed**: Adds (13-18) to (27-31) Physical Damage
- **Razor-sharp**: Adds (16-21) to (32-38) Physical Damage
- **Tempered**: Adds (19-25) to (39-45) Physical Damage
- **Flaring**: Adds (22-29) to (45-52) Physical Damage

#### **Rolling Examples**
- **Polished (6-9) to (13-15)**:
  - First roll: 7 (from 6-9)
  - Second roll: 14 (from 13-15)
  - Result: Adds 7 to 14 Physical Damage

- **Honed (8-12) to (17-20)**:
  - First roll: 10 (from 8-12)
  - Second roll: 18 (from 17-20)
  - Result: Adds 10 to 18 Physical Damage

### **Benefits**
- **🎯 Accuracy**: True Path of Exile-style dual ranges
- **🎲 Independent Rolling**: Each range rolls separately
- **🎛️ Precise Control**: Separate sliders for each range
- **📊 Data Integrity**: Maintains range constraints
- **🔄 Reproducibility**: Seeded rolling for consistent results

### **Testing Dual-Range Modifiers**
1. **Import Test Data**: Use the `DualRangeTest` component to copy test data
2. **Bulk Import**: Import dual-range affixes via the bulk import system
3. **Apply to Items**: Add dual-range affixes to weapons
4. **Verify Rolling**: Check that both ranges roll independently
5. **Test Sliders**: Use separate sliders to adjust both ranges
6. **Validate Constraints**: Ensure ranges are properly constrained

## **Manual Affix Value Control**

### Overview
The ItemAffixEditor now includes **slider controls** that allow you to manually adjust affix values within their ranges. This is perfect for fine-tuning items during development or creating specific test scenarios.

### Features

#### **1. Value Sliders for Existing Affixes**
When you have affixes applied to an item, each modifier displays:
- **Current Value**: Shows the exact rolled value
- **Manual Value Slider**: Adjust the value within the affix's original range (e.g., 40-49% for Heavy affix)
- **Random Button**: Re-roll the value randomly within the original range
- **Original Range Display**: Shows the exact original range from the affix template

**Important**: The slider is constrained to the affix's original range. For example, a "Heavy" affix with 40-49% increased Physical Damage will only allow you to select values between 40 and 49, not outside that range.

#### **2. Manual Affix Creation**
Create custom affixes from scratch with full control:
- **Affix Type**: Prefix or Suffix
- **Affix Name**: Custom name for the affix
- **Stat Name**: The stat this affix modifies
- **Modifier Type**: Flat, Increased, More, Reduced, Less
- **Scope**: Local or Global
- **Damage Type**: Physical, Fire, Cold, Lightning, Chaos, None
- **Value Range**: Set the minimum and maximum possible values
- **Manual Value Slider**: Choose the exact value within the range
- **Randomize Button**: Randomly select a value within the range

### How to Use

#### **Adjusting Existing Affix Values**
1. Select any item with affixes in the Project window
2. Scroll down to "Current Affixes" section
3. Find the affix you want to adjust
4. Use the slider to set the desired value
5. The value updates immediately and is saved

#### **Creating Custom Affixes**
1. Select any item in the Project window
2. Scroll down to "Manual Affix Creation" section
3. Fill in the affix details:
   - **Affix Name**: e.g., "Custom Damage"
   - **Stat Name**: e.g., "PhysicalDamage"
   - **Modifier Type**: e.g., "Increased"
   - **Scope**: e.g., "Local"
   - **Value Range**: e.g., 10 to 20
   - **Manual Value**: Use slider to select 15
4. Click "Create Custom Affix"
5. The affix is added to the item with your selected value

### Example Workflow

#### **Creating a Perfect Weapon**
1. **Create a weapon** with base stats
2. **Add affixes** from the database (they get random values)
3. **Use sliders** to adjust values to your desired numbers
4. **Create custom affixes** for specific effects you want
5. **Fine-tune** until the weapon is perfect

#### **Testing Different Scenarios**
1. **Create multiple copies** of the same weapon
2. **Adjust affix values** to test different power levels
3. **Compare results** in combat or tooltips
4. **Find the perfect balance** for your game

### Benefits

#### **🎯 Precise Control**
- Set exact values instead of relying on random rolls
- Perfect for testing and balancing
- Create consistent items for comparison

#### **🔧 Development Tools**
- Quickly prototype different item configurations
- Test edge cases and extreme values
- Create reference items for documentation

#### **🎮 Game Design**
- Fine-tune item progression
- Create specific test scenarios
- Balance items without re-rolling multiple times

### Tips

#### **For Testing**
- Use the "Random" button to quickly test different values
- Create items with minimum and maximum values to test ranges
- Use custom affixes to test new mechanics

#### **For Balancing**
- Start with database affixes for realistic values
- Use sliders to make small adjustments
- Create custom affixes for unique effects

#### **For Development**
- Save multiple versions of items with different values
- Use descriptive names for custom affixes
- Document the reasoning behind specific values

## ItemDatabaseEditor

### Features
- **Auto-Population**: Scan Resources/Items folder for new items
- **Category Management**: Organize items by type
- **Statistics**: View database statistics and rarity breakdown
- **Item Management**: Select and remove items

### Usage
1. Select the `ItemDatabase` asset
2. Use "Scan" buttons to populate from Resources/Items
3. View organized item lists by category
4. Use statistics to monitor database health

## Redesigned Tag System

### Overview
The tag system has been redesigned to work more like Path of Exile, where:
- **Weapons have weapon-specific tags** (type, handedness, attack type)
- **Affixes specify which weapon types they can apply to**
- **All damage types can be applied to any weapon** (as long as the weapon type is compatible)

## Modifier Scope System

### Overview
The modifier scope system distinguishes between **Local** and **Global** modifiers, just like Path of Exile. This affects how damage calculations work and how modifiers interact with each other.

### Local Modifiers (🔧)
Local modifiers affect the **item's base properties** and are applied first in damage calculations.

**Characteristics:**
- **Unconditional** modifiers that affect innate item properties
- **Hand-specific** modifiers (e.g., "with this Weapon")
- **Weapon hit effects** unrelated to numeric damage
- **Applied to item base stats** before global modifiers

**Examples:**
- `Adds 5-10 Physical Damage` (affects weapon base damage)
- `20% increased Attack Speed` (affects weapon attack speed)
- `15% increased Critical Strike Chance` (affects weapon crit chance)
- `Adds 3-6 Fire Damage` (affects weapon base damage)

### Global Modifiers (🌍)
Global modifiers affect **character stats** and are applied after local modifiers.

**Characteristics:**
- **Conditional** modifiers (e.g., "to Attacks", "while wielding")
- **Character-wide** effects
- **Applied to total stats** after local modifiers
- **Never modify item base stats** directly

**Examples:**
- `30% increased Physical Damage to Attacks` (from rings/charms)
- `25% more Attack Speed` (from skills/passives)
- `20% increased Critical Strike Multiplier` (from passives)
- `15% increased Movement Speed` (from items/skills)

### Damage Calculation Order
1. **Apply LOCAL modifiers** to item base stats
2. **Apply GLOBAL modifiers** to character total stats
3. **Calculate final damage** using PoE-style formula

**Example Calculation:**
```
Base Sword: 10-15 Physical Damage

LOCAL modifiers (applied to weapon):
+ Adds 5-10 Physical Damage
+ 20% increased Physical Damage
= Weapon base: 18-30 Physical Damage

GLOBAL modifiers (applied to character):
+ 30% increased Physical Damage (from ring)
+ 25% more Physical Damage (from skill)
= Final damage: 18-30 * 1.3 * 1.25 = 29.25-48.75
```

### Automatic Scope Detection
The bulk import system automatically determines the correct scope:

**Local Scope (🔧):**
- Added damage modifiers (`Adds X to Y Damage`)
- Increased damage modifiers on weapons (`X% increased Physical Damage`)
- Attack speed modifiers (`X% increased Attack Speed`)
- Critical strike modifiers (`X% increased Critical Strike Chance`)

**Global Scope (🌍):**
- "More" multipliers (`X% more Damage`)
- "Reduced/Less" modifiers (`X% reduced Damage`)
- Conditional modifiers (`X% increased Damage to Attacks`)
- Character-wide effects

### Benefits
1. **🎯 Accurate Calculations**: Matches Path of Exile's damage calculation system
2. **📊 Clear Separation**: Distinguishes between item and character effects
3. **🔧 Proper Interactions**: Local modifiers affect base stats, global modifiers affect totals
4. **🌍 Scalable System**: Easy to add new modifier types with appropriate scopes
5. **📝 Visual Feedback**: Color-coded scope indicators in the inspector

### Usage in Inspector
- **Green indicators** (🔧) show LOCAL modifiers
- **Blue indicators** (🌍) show GLOBAL modifiers
- **Scope dropdown** allows manual adjustment
- **Automatic detection** during bulk import

## Affix Rolling System

### Overview
The affix rolling system ensures that each affix applied to an item has a **specific value** rather than a range. This is crucial for accurate damage calculations and proper Path of Exile-style item generation.

### How It Works

#### **1. Template Affixes (Database)**
Affixes stored in the `AffixDatabase` contain **ranges** of possible values:
```
"Squire's" Prefix: 15-19% increased Physical Damage
"Glinting" Prefix: Adds 1-3 Physical Damage
```

#### **2. Rolling Process**
When an affix is applied to an item, it gets "rolled" to a specific value:
```
Template: 15-19% increased Physical Damage
Rolled: 17.3% increased Physical Damage
```

#### **3. Actual Values**
The rolled affix contains actual numbers, not ranges:
```
Before rolling: minValue=15, maxValue=19
After rolling: minValue=17.3, maxValue=17.3
```

### Benefits

#### **🎯 Accurate Damage Calculations**
- Each affix has a specific value, not a range
- Damage calculations use actual numbers
- No ambiguity in final damage values

#### **🎲 Random but Reproducible**
- Each item gets unique affix values
- Same seed produces same results
- Perfect for save/load systems

#### **📊 Clear Item Display**
- Tooltips show exact values
- No confusing ranges in item descriptions
- Players see exactly what they have

#### **🔧 Proper PoE Mechanics**
- Matches Path of Exile's affix system
- Each item is unique when generated
- Supports item trading and comparison

### Implementation Details

#### **Automatic Rolling**
When you add an affix to an item (via `AddPrefix()` or `AddSuffix()`), the system automatically:
1. **Generates a rolled version** of the affix
2. **Selects random values** within the ranges
3. **Applies the rolled affix** to the item

#### **Seeded Rolling**
For reproducible results (useful for testing or save systems):
```csharp
// Random roll
Affix rolledAffix = originalAffix.GenerateRolledAffix();

// Seeded roll (same seed = same result)
Affix rolledAffix = originalAffix.GenerateRolledAffix(12345);
```

#### **Value Calculation**
The `GetModifierValue()` method now uses the actual rolled values:
```csharp
// Uses the actual rolled value (minValue and maxValue are the same after rolling)
totalValue += modifier.minValue;
```

### Example Workflow

#### **1. Create Template Affix**
```csharp
Affix template = new Affix("Squire's", "15-19% increased Physical Damage", AffixType.Prefix, AffixTier.Tier1);
AffixModifier modifier = new AffixModifier("PhysicalDamage", 15f, 19f, ModifierType.Increased, ModifierScope.Local);
template.modifiers.Add(modifier);
```

#### **2. Apply to Item**
```csharp
// This automatically rolls the affix
weapon.AddPrefix(template);
```

#### **3. Result**
The weapon now has a specific value like `17.3% increased Physical Damage` instead of the range `15-19%`.

### Testing the System

Use the `AffixRollingTest` script to see the rolling system in action:
1. **Add `AffixRollingTest`** component to any GameObject
2. **Right-click** and select "Test Affix Rolling"
3. **Observe** how the same affix produces different values each time
4. **Test seeded rolling** for reproducible results

### Tooltip Display

The updated tooltip system shows:
- **Exact rolled values** instead of ranges
- **Scope indicators** (🔧 Local, 🌍 Global)
- **Clear formatting** for different modifier types

**Example:**
```
Squire's: 🔧 +17.3% increased PhysicalDamage
Glinting: 🔧 +2.1 Physical Damage
```

### Affix Compatibility
Affixes now use more flexible tag requirements:

**Physical Damage Affixes:**
- Required tags: `["weapon", "attack"]`
- Can apply to: Any weapon that can attack (all melee, bows)

**Elemental Damage Affixes:**
- Required tags: `["weapon"]`
- Can apply to: Any weapon type

**Weapon-Specific Affixes:**
- Can require specific weapon types like `["weapon", "sword"]` or `["weapon", "melee"]`

### Benefits of New System
1. **Flexibility**: All damage types can be applied to any weapon
2. **Realism**: Matches Path of Exile's system where any weapon can have any damage type
3. **Scalability**: Easy to add new weapon types and affixes
4. **Clarity**: Clear separation between weapon properties and affix requirements

### Example: Sword with Elemental Damage
A sword with "Auto-Set Tags" will have:
- `["weapon", "sword", "onehanded", "melee", "attack"]`

This makes it compatible with:
- Physical damage affixes (requires `["weapon", "attack"]`)
- Fire damage affixes (requires `["weapon"]`)
- Cold damage affixes (requires `["weapon"]`)
- Lightning damage affixes (requires `["weapon"]`)
- Chaos damage affixes (requires `["weapon"]`)
- Sword-specific affixes (requires `["weapon", "sword"]`)

## Quick Start Guide

### Setting Up a New Weapon
1. Create a new `WeaponItem` asset
2. Set the weapon type and handedness
3. Select the weapon in the Project window
4. Scroll down to "Affix Management"
5. Click **"Auto-Set Tags"** to configure compatibility
6. Use "Generate Random" or "Manual Selection" to add affixes

### Adding New Affixes
1. Select the `AffixDatabase` asset
2. Use "Add Physical Damage Affixes" or "Add Elemental Damage Affixes"
3. Navigate through the hierarchical structure to edit specific affixes
4. Modify tags, values, and descriptions as needed

### Populating Item Database
1. Organize item assets in `Assets/Resources/Items/[Category]/`
2. Select the `ItemDatabase` asset
3. Click "Scan and Populate from Resources/Items"
4. Verify items appear in the appropriate categories

## Troubleshooting

### "No compatible affixes found"
**Cause**: Item doesn't have the required tags
**Solution**: Click "Auto-Set Tags" in the ItemAffixEditor

### Affixes not appearing in categories
**Cause**: AffixDatabase not populated
**Solution**: Use "Add Physical Damage Affixes" or "Add Elemental Damage Affixes" buttons

### Items not appearing in database
**Cause**: Items not in Resources/Items folder
**Solution**: Move items to `Assets/Resources/Items/[Category]/` and scan

### Compilation errors
**Cause**: Missing using statements or duplicate definitions
**Solution**: Check for missing `using UnityEditor;` or duplicate enum definitions

### Tags not updating
**Cause**: Changes not saved
**Solution**: Use `EditorUtility.SetDirty()` or click "Auto-Set Tags" again
