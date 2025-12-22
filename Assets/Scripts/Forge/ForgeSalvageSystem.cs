using UnityEngine;
using Dexiled.Data.Items;
using System.Collections.Generic;

/// <summary>
/// Handles salvaging items into materials
/// </summary>
public static class ForgeSalvageSystem
{
    /// <summary>
    /// Calculate materials gained from salvaging an item
    /// </summary>
    public static ForgeMaterialData CalculateSalvageMaterials(BaseItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[ForgeSalvageSystem] Cannot salvage null item.");
            return null;
        }

        ForgeMaterialType materialType = GetMaterialTypeForItem(item);
        int quantity = CalculateMaterialQuantity(item);

        return new ForgeMaterialData(materialType, quantity);
    }

    /// <summary>
    /// Get the material type for a given item
    /// </summary>
    private static ForgeMaterialType GetMaterialTypeForItem(BaseItem item)
    {
        switch (item.itemType)
        {
            case ItemType.Weapon:
                return ForgeMaterialType.WeaponScraps;
            case ItemType.Armour:
                return ForgeMaterialType.ArmourScraps;
            case ItemType.Accessory:
                return ForgeMaterialType.ArmourScraps; // Accessories also give armour scraps
            case ItemType.Effigy:
                return ForgeMaterialType.EffigySplinters;
            case ItemType.Warrant:
                return ForgeMaterialType.WarrantShards;
            default:
                Debug.LogWarning($"[ForgeSalvageSystem] Unknown item type: {item.itemType}. Defaulting to Weapon Scraps.");
                return ForgeMaterialType.WeaponScraps;
        }
    }

    /// <summary>
    /// Calculate how many materials to give based on item rarity and level
    /// </summary>
    private static int CalculateMaterialQuantity(BaseItem item)
    {
        int baseQuantity = 1;

        // Rarity multiplier
        float rarityMultiplier = 1f;
        switch (item.rarity)
        {
            case ItemRarity.Normal:
                rarityMultiplier = 1f;
                break;
            case ItemRarity.Magic:
                rarityMultiplier = 1.5f;
                break;
            case ItemRarity.Rare:
                rarityMultiplier = 2.5f;
                break;
            case ItemRarity.Unique:
                rarityMultiplier = 4f;
                break;
        }

        // Level scaling (higher level items give more materials)
        float levelMultiplier = 1f + (item.requiredLevel * 0.1f);

        // Quality bonus
        float qualityMultiplier = 1f + (item.quality * 0.05f);

        int totalQuantity = Mathf.RoundToInt(baseQuantity * rarityMultiplier * levelMultiplier * qualityMultiplier);
        return Mathf.Max(1, totalQuantity); // Always give at least 1
    }

    /// <summary>
    /// Salvage an item and add materials to character inventory
    /// </summary>
    public static bool SalvageItem(BaseItem item, Character character)
    {
        if (item == null || character == null)
        {
            Debug.LogWarning("[ForgeSalvageSystem] Cannot salvage: item or character is null.");
            return false;
        }

        ForgeMaterialData materials = CalculateSalvageMaterials(item);
        if (materials == null)
        {
            return false;
        }

        // Add materials to character
        ForgeMaterialManager.AddMaterials(character, materials.materialType, materials.quantity);

        Debug.Log($"[ForgeSalvageSystem] Salvaged {item.GetDisplayName()} into {materials.quantity} {materials.GetDisplayName()}");

        return true;
    }
}

