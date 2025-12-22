# Forge System Documentation

## Overview
The Forge is a town facility where players can:
1. **Salvage** items (Equipment/Effigies/Warrants) into base materials
2. **Craft** new items using those materials

## Core Components

### 1. Material System
- **ForgeMaterialType**: Enum defining material types
  - `WeaponScraps` - From weapons
  - `ArmourScraps` - From armour and accessories
  - `EffigySplinters` - From effigies
  - `WarrantShards` - From warrants

- **ForgeMaterialData**: Data structure storing material type and quantity
- **ForgeMaterialManager**: Static utility for managing materials in character inventory

### 2. Salvage System
- **ForgeSalvageSystem**: Handles destroying items and converting them to materials
- **Material Calculation**:
  - Base: 1 material
  - Rarity multiplier: Normal (1x), Magic (1.5x), Rare (2.5x), Unique (4x)
  - Level scaling: +10% per item level
  - Quality bonus: +5% per quality point
  - Minimum: Always gives at least 1 material

### 3. Crafting System
- **CraftingRecipe**: ScriptableObject defining recipes
  - Required materials
  - Item type to craft
  - Equipment type (optional)
  - Specific item or random generation
  - Item level range
  - Target rarity

- **ForgeCraftingSystem**: Handles crafting items from materials
  - Validates material requirements
  - Consumes materials
  - Generates items (specific or random)
  - Applies affixes based on rarity
  - Sets appropriate item level

### 4. Character Integration
- Added `forgeMaterials` list to `Character` class
- Materials persist with character save data

## Usage Examples

### Salvaging an Item
```csharp
BaseItem itemToSalvage = // ... get item
Character character = CharacterManager.Instance.GetCurrentCharacter();

bool success = ForgeSalvageSystem.SalvageItem(itemToSalvage, character);
// Materials are automatically added to character.forgeMaterials
```

### Checking Materials
```csharp
Character character = CharacterManager.Instance.GetCurrentCharacter();
int weaponScraps = ForgeMaterialManager.GetMaterialQuantity(character, ForgeMaterialType.WeaponScraps);
```

### Crafting an Item
```csharp
CraftingRecipe recipe = // ... load recipe
Character character = CharacterManager.Instance.GetCurrentCharacter();

BaseItem craftedItem = ForgeCraftingSystem.CraftItem(recipe, character);
if (craftedItem != null)
{
    // Add to inventory
    CharacterManager.Instance.inventoryItems.Add(craftedItem);
}
```

## Creating Recipes

1. Create a new `CraftingRecipe` ScriptableObject
2. Set required materials (e.g., 5 Weapon Scraps, 3 Armour Scraps)
3. Choose item type (Weapon, Armour, Accessory)
4. Set crafting options:
   - `craftRandomItem = true` for random generation
   - `minItemLevel` and `maxItemLevel` for level range
   - `craftedRarity` for target rarity

## Next Steps (UI Implementation)

The core systems are complete, but you'll need to create:

1. **ForgeUI Scene/Manager**:
   - Salvage panel (drag items to salvage)
   - Material display (show current materials)
   - Crafting panel (show available recipes)
   - Recipe selection and crafting button

2. **Recipe Database**:
   - Create ScriptableObject recipes for common items
   - Store in Resources folder for easy access

3. **Integration**:
   - Add Forge button to town UI
   - Load Forge scene when accessed
   - Save materials when leaving Forge

## Material Yield Examples

- **Normal Level 1 Weapon**: 1 Weapon Scrap
- **Magic Level 10 Weapon**: ~2-3 Weapon Scraps
- **Rare Level 20 Weapon**: ~5-6 Weapon Scraps
- **Unique Level 30 Weapon**: ~12-15 Weapon Scraps

## Notes

- Materials are stored per-character
- Salvaging is permanent (items are destroyed)
- Crafted items are level-appropriate for the character
- Affixes are automatically applied based on rarity

