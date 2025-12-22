using UnityEngine;
using Dexiled.Data.Items;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles crafting items from materials
/// </summary>
public static class ForgeCraftingSystem
{
    /// <summary>
    /// Craft an item from a recipe
    /// </summary>
    public static BaseItem CraftItem(CraftingRecipe recipe, Character character)
    {
        if (recipe == null || character == null)
        {
            Debug.LogWarning("[ForgeCraftingSystem] Cannot craft: recipe or character is null.");
            return null;
        }

        // Check if character has required materials
        if (!recipe.CanCraft(character))
        {
            Debug.LogWarning($"[ForgeCraftingSystem] Character does not have required materials for {recipe.recipeName}");
            return null;
        }

        // Consume materials
        foreach (var requirement in recipe.requiredMaterials)
        {
            if (!ForgeMaterialManager.RemoveMaterials(character, requirement.materialType, requirement.quantity))
            {
                Debug.LogError($"[ForgeCraftingSystem] Failed to remove {requirement.quantity} {requirement.materialType}");
                return null;
            }
        }

        // Generate the item
        BaseItem craftedItem = null;

        // Priority 1: Use selected item if set (from dynamic item selection)
        if (recipe.selectedItemToCraft != null)
        {
            craftedItem = CreateItemCopy(recipe.selectedItemToCraft);
            // Keep the original requiredLevel from the item
        }
        // Priority 2: Use specific item if set and not crafting random
        else if (recipe.specificItem != null && !recipe.craftRandomItem)
        {
            craftedItem = CreateItemCopy(recipe.specificItem);
        }
        // Priority 3: Generate random item
        else
        {
            craftedItem = GenerateRandomItem(recipe, character);
        }

        if (craftedItem == null)
        {
            Debug.LogError($"[ForgeCraftingSystem] Failed to generate item for recipe {recipe.recipeName}");
            // Refund materials
            foreach (var requirement in recipe.requiredMaterials)
            {
                ForgeMaterialManager.AddMaterials(character, requirement.materialType, requirement.quantity);
            }
            return null;
        }

        // Set item level (use item's original requiredLevel if we selected a specific item, otherwise calculate)
        if (recipe.selectedItemToCraft != null || (recipe.specificItem != null && !recipe.craftRandomItem))
        {
            // Use the item's original requiredLevel
            craftedItem.itemLevel = craftedItem.requiredLevel;
        }
        else
        {
            // For random items, calculate appropriate level
            int itemLevel = CalculateItemLevel(recipe, character);
            craftedItem.requiredLevel = itemLevel;
            craftedItem.itemLevel = itemLevel;
        }
        
        craftedItem.rarity = recipe.craftedRarity;

        // Apply affixes based on rarity
        ApplyAffixesForRarity(craftedItem, recipe.craftedRarity);

        Debug.Log($"[ForgeCraftingSystem] Crafted {craftedItem.GetDisplayName()} (Level {craftedItem.requiredLevel}, {recipe.craftedRarity})");

        return craftedItem;
    }

    /// <summary>
    /// Generate a random item based on recipe
    /// </summary>
    private static BaseItem GenerateRandomItem(CraftingRecipe recipe, Character character)
    {
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[ForgeCraftingSystem] ItemDatabase.Instance is null!");
            return null;
        }

        int itemLevel = CalculateItemLevel(recipe, character);
        List<BaseItem> eligibleItems = new List<BaseItem>();

        // Get eligible items from database
        switch (recipe.itemType)
        {
            case ItemType.Weapon:
                var weapons = ItemDatabase.Instance.GetWeaponsByLevel(recipe.minItemLevel, itemLevel);
                eligibleItems = weapons
                    .Where(w => w != null && 
                                (!recipe.filterByEquipmentType || w.equipmentType == recipe.equipmentType) &&
                                (!recipe.filterByWeaponType || w.weaponType == recipe.weaponType) &&
                                (!recipe.filterByWeaponHandedness || w.handedness == recipe.weaponHandedness))
                    .Cast<BaseItem>().ToList();
                break;
            case ItemType.Armour:
                if (ItemDatabase.Instance.armour == null)
                {
                    Debug.LogError("[ForgeCraftingSystem] ItemDatabase.Instance.armour is null!");
                    break;
                }
                eligibleItems = ItemDatabase.Instance.armour
                    .Where(a => a != null && 
                                a.requiredLevel >= recipe.minItemLevel && a.requiredLevel <= itemLevel &&
                                (!recipe.filterByEquipmentType || a.equipmentType == recipe.equipmentType) &&
                                (!recipe.filterByArmourSlot || a.armourSlot == recipe.armourSlot) &&
                                (!recipe.filterByDefenseType || MatchesDefenseType(a, recipe)))
                    .Cast<BaseItem>().ToList();
                break;
            case ItemType.Accessory:
                eligibleItems = ItemDatabase.Instance.jewellery
                    .Where(j => j != null && 
                                j.requiredLevel >= recipe.minItemLevel && j.requiredLevel <= itemLevel &&
                                (!recipe.filterByEquipmentType || j.equipmentType == recipe.equipmentType))
                    .Cast<BaseItem>().ToList();
                break;
        }

        if (eligibleItems.Count == 0)
        {
            Debug.LogWarning($"[ForgeCraftingSystem] No eligible items found for type {recipe.itemType} at level {itemLevel}");
            return null;
        }

        // Select random item
        BaseItem baseItem = eligibleItems[Random.Range(0, eligibleItems.Count)];
        return CreateItemCopy(baseItem);
    }

    /// <summary>
    /// Calculate appropriate item level for crafted item
    /// </summary>
    private static int CalculateItemLevel(CraftingRecipe recipe, Character character)
    {
        int maxLevel = recipe.maxItemLevel > 0 ? recipe.maxItemLevel : character.level;
        int itemLevel = Mathf.Clamp(character.level, recipe.minItemLevel, maxLevel);
        return itemLevel;
    }

    /// <summary>
    /// Create a copy of an item (for crafting)
    /// </summary>
    private static BaseItem CreateItemCopy(BaseItem original)
    {
        if (original == null) return null;

        // Create a ScriptableObject instance (runtime copy)
        BaseItem copy = ScriptableObject.CreateInstance(original.GetType()) as BaseItem;
        if (copy == null) return null;

        // Copy basic properties
        copy.itemName = original.itemName;
        copy.description = original.description;
        copy.itemIcon = original.itemIcon;
        copy.itemType = original.itemType;
        copy.equipmentType = original.equipmentType;
        copy.requiredLevel = original.requiredLevel;
        copy.itemLevel = original.itemLevel;
        copy.quality = original.quality;
        copy.itemTags = new List<string>(original.itemTags);

        // Copy implicit modifiers
        copy.implicitModifiers = new List<Affix>(original.implicitModifiers);

        // Note: Prefixes and suffixes will be applied by ApplyAffixesForRarity

        return copy;
    }

    /// <summary>
    /// Apply affixes based on item rarity
    /// </summary>
    private static void ApplyAffixesForRarity(BaseItem item, ItemRarity rarity)
    {
        if (item == null) return;

        // Clear existing random affixes
        item.prefixes = new List<Affix>();
        item.suffixes = new List<Affix>();

        // Use AffixDatabase_Modern if available
        if (AffixDatabase_Modern.Instance != null)
        {
            // Calculate magic/rare chances based on target rarity
            float magicChance = rarity == ItemRarity.Magic ? 1f : (rarity == ItemRarity.Rare ? 0.5f : 0f);
            float rareChance = rarity == ItemRarity.Rare ? 1f : 0f;

            AffixDatabase_Modern.Instance.GenerateRandomAffixes(item, item.itemLevel, magicChance, rareChance);
            
            // Generate display name for Magic/Rare items
            GenerateItemName(item);
        }
        else if (AffixDatabase.Instance != null)
        {
            // Fallback to old AffixDatabase using public method
            AffixDatabase.Instance.GenerateRandomAffixes(item, item.itemLevel, rarity);
        }
        else
        {
            Debug.LogWarning("[ForgeCraftingSystem] No AffixDatabase available, cannot apply affixes.");
        }
    }

    /// <summary>
    /// Generate display name for Magic/Rare items (similar to AreaLootTable)
    /// </summary>
    private static void GenerateItemName(BaseItem item)
    {
        if (item == null || item.rarity == ItemRarity.Normal || item.rarity == ItemRarity.Unique)
        {
            return; // Normal and Unique items use their base name
        }

        // Generate name from prefixes/suffixes (similar to AreaLootTable logic)
        // This is a simplified version - you may want to use the full name generation from AreaLootTable
        string name = item.itemName;
        if (item.prefixes.Count > 0 || item.suffixes.Count > 0)
        {
            // Use the first prefix/suffix for name generation
            // Full implementation would use ItemNameGenerator
            name = item.itemName; // Placeholder - would need full name generation system
        }
        item.generatedName = name;
    }
    
    /// <summary>
    /// Check if armour matches the specified defense type
    /// Handles both single and hybrid defense bases
    /// </summary>
    private static bool MatchesDefenseType(Armour armour, CraftingRecipe recipe)
    {
        if (!recipe.filterByDefenseType) return true;
        
        switch (recipe.defenseType)
        {
            // Single defense types - must have this defense and no other primary defenses
            case DefenseType.Armour:
                return armour.armour > 0 && armour.evasion == 0 && armour.energyShield == 0 && armour.ward == 0;
            case DefenseType.Evasion:
                return armour.evasion > 0 && armour.armour == 0 && armour.energyShield == 0 && armour.ward == 0;
            case DefenseType.EnergyShield:
                return armour.energyShield > 0 && armour.armour == 0 && armour.evasion == 0 && armour.ward == 0;
            case DefenseType.Ward:
                return armour.ward > 0 && armour.armour == 0 && armour.evasion == 0 && armour.energyShield == 0;
            
            // Hybrid defense types - must have both specified defenses
            case DefenseType.ArmourEvasion:
                return armour.armour > 0 && armour.evasion > 0;
            case DefenseType.ArmourEnergyShield:
                return armour.armour > 0 && armour.energyShield > 0;
            case DefenseType.ArmourWard:
                return armour.armour > 0 && armour.ward > 0;
            case DefenseType.EvasionEnergyShield:
                return armour.evasion > 0 && armour.energyShield > 0;
            case DefenseType.EvasionWard:
                return armour.evasion > 0 && armour.ward > 0;
            case DefenseType.EnergyShieldWard:
                return armour.energyShield > 0 && armour.ward > 0;
            
            default:
                return true;
        }
    }
}

