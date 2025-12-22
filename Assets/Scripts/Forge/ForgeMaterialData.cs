using UnityEngine;
using System;

/// <summary>
/// Represents a material in the forge system
/// </summary>
[Serializable]
public class ForgeMaterialData
{
    public ForgeMaterialType materialType;
    public int quantity = 0;

    public ForgeMaterialData() { }

    public ForgeMaterialData(ForgeMaterialType type, int qty)
    {
        materialType = type;
        quantity = qty;
    }

    /// <summary>
    /// Get display name for the material
    /// </summary>
    public string GetDisplayName()
    {
        switch (materialType)
        {
            case ForgeMaterialType.WeaponScraps:
                return "Weapon Scraps";
            case ForgeMaterialType.ArmourScraps:
                return "Armour Scraps";
            case ForgeMaterialType.EffigySplinters:
                return "Effigy Splinters";
            case ForgeMaterialType.WarrantShards:
                return "Warrant Shards";
            default:
                return "Unknown Material";
        }
    }
}

