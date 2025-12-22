# Forge Granular Recipes Guide

This guide explains how to create granular crafting recipes that filter items by specific types, allowing players to craft exactly what they want.

## Overview

The crafting recipe system now supports multiple levels of filtering, allowing you to create very specific recipes. For example:
- **Weapon Recipes**: Filter by weapon type (Sword, Axe, Bow, etc.) and handedness (OneHanded, TwoHanded)
- **Armour Recipes**: Filter by slot (Helmet, BodyArmour, Gloves, Boots), armour type (Cloth, Leather, Plate, etc.), and defense type (Armour, Evasion, Energy Shield, Ward)

## Recipe Filter Options

### Base Filters (All Item Types)

- **Item Type**: Weapon, Armour, Accessory
- **Equipment Type**: MainHand, OffHand, etc. (optional)
- **Level Range**: `minItemLevel` to `maxItemLevel` (or -1 for unlimited)

### Weapon-Specific Filters

- **Filter By Weapon Type**: Enable to filter by specific weapon types
  - Options: Axe, Bow, Claw, Dagger, RitualDagger, Mace, Sceptre, Staff, Sword, Wand
- **Filter By Weapon Handedness**: Enable to filter by one-handed or two-handed
  - Options: OneHanded, TwoHanded

### Armour-Specific Filters

- **Filter By Armour Slot**: Enable to filter by equipment slot
  - Options: Helmet, BodyArmour, Gloves, Boots, Shield
- **Filter By Defense Type**: Enable to filter by defense stat
  - Options: Armour, Evasion, Energy Shield, Ward
  - Matches items where the selected defense stat > 0
  - **Hybrid Defense Bases**: Items with multiple defense types (e.g., Armour/Evasion, Armour/Energy Shield, Evasion/Energy Shield) will appear in multiple recipe categories. For example, an item with both Armour and Evasion will show up in both "Armour" and "Evasion" filtered recipes.

## Example Recipe Setups

### Example 1: One-Handed Swords

```
Item Type: Weapon
Filter By Equipment Type: ✓ (MainHand)
Filter By Weapon Type: ✓ (Sword)
Filter By Weapon Handedness: ✓ (OneHanded)
Level Range: 1-100
```

This recipe will show all one-handed swords that the player can craft at their level.

### Example 2: Body Armour (Armour Defense)

```
Item Type: Armour
Filter By Equipment Type: ✓ (BodyArmour)
Filter By Armour Slot: ✓ (BodyArmour)
Filter By Defense Type: ✓ (Armour)
Level Range: 1-100
```

This recipe will show all body armours that primarily use Armour defense.

### Example 3: Helmet (Energy Shield)

```
Item Type: Armour
Filter By Armour Slot: ✓ (Helmet)
Filter By Defense Type: ✓ (EnergyShield)
Level Range: 1-100
```

This recipe will show all helmets that primarily use Energy Shield defense.

### Example 4: Two-Handed Bows

```
Item Type: Weapon
Filter By Weapon Type: ✓ (Bow)
Filter By Weapon Handedness: ✓ (TwoHanded)
Level Range: 1-100
```

This recipe will show all two-handed bows.

## Creating a Complete Recipe Set

To give players options for all item types, create recipes like this:

### Weapon Recipes (10 recipes - one per weapon type)
1. **Axe Recipe** - Filter by Weapon Type: Axe
2. **Bow Recipe** - Filter by Weapon Type: Bow
3. **Claw Recipe** - Filter by Weapon Type: Claw
4. **Dagger Recipe** - Filter by Weapon Type: Dagger
5. **Mace Recipe** - Filter by Weapon Type: Mace
6. **Sceptre Recipe** - Filter by Weapon Type: Sceptre
7. **Staff Recipe** - Filter by Weapon Type: Staff
8. **Sword Recipe** - Filter by Weapon Type: Sword
9. **Wand Recipe** - Filter by Weapon Type: Wand
10. **Ritual Dagger Recipe** - Filter by Weapon Type: RitualDagger

### Armour Recipes (40+ recipes - combinations of slot, type, and defense)

**By Slot (5 recipes):**
- Helmet Recipe
- Body Armour Recipe
- Gloves Recipe
- Boots Recipe
- Shield Recipe

**By Slot + Defense Type (20 recipes - 5 slots × 4 defense types):**
- Helmet (Armour Defense)
- Helmet (Evasion Defense)
- Helmet (Energy Shield)
- Helmet (Ward)
- Body Armour (Armour Defense)
- Body Armour (Evasion Defense)
- etc.

## Recommended Approach

For a balanced crafting system, create recipes at these granularity levels:

### Level 1: Basic Categories (5-10 recipes)
- Weapon Recipe (all weapons)
- Armour Recipe (all armour)
- Accessory Recipe (all jewellery)
- Optional: Separate by equipment slot

### Level 2: Type-Based (15-20 recipes)
- One recipe per weapon type (10 recipes)
- One recipe per armour slot (5 recipes)
- Or combine: One recipe per armour slot + defense type (20 recipes)

### Level 3: Full Granularity (30-50 recipes)
- One recipe per weapon type + handedness
- One recipe per armour slot + defense type
- Maximum player choice, but more recipes to manage

## Tips

1. **Start Simple**: Begin with broad recipes (all weapons, all armour) and add granularity as needed
2. **Use Descriptive Names**: Name recipes clearly (e.g., "Plate Body Armour - Armour Defense")
3. **Level Ranges**: Set appropriate `minItemLevel` and `maxItemLevel` for each recipe
4. **Material Costs**: Consider varying material costs based on item rarity/type
5. **Test Filtering**: Verify that recipes show the expected items at different character levels

## Filter Logic

Filters work with AND logic - all enabled filters must match:
- If you enable "Filter By Weapon Type: Sword" AND "Filter By Weapon Handedness: OneHanded"
- The recipe will only show items that are BOTH a Sword AND OneHanded

Filters are optional - if a filter is disabled, it doesn't restrict the results.

## Dynamic Item Selection

Even with granular filters, recipes still support dynamic item selection:
- Items are filtered by player level
- Player can choose which specific item to craft from the eligible list
- Items are sorted by required level (lowest first)

This means one recipe can still offer multiple item options, but they'll all match your specified filters.

