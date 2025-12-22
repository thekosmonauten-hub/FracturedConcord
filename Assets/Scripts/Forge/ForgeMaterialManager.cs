using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages forge materials for characters
/// </summary>
public static class ForgeMaterialManager
{
    /// <summary>
    /// Get material quantity for a character
    /// </summary>
    public static int GetMaterialQuantity(Character character, ForgeMaterialType materialType)
    {
        if (character == null || character.forgeMaterials == null)
        {
            return 0;
        }

        var material = character.forgeMaterials.FirstOrDefault(m => m.materialType == materialType);
        return material != null ? material.quantity : 0;
    }

    /// <summary>
    /// Add materials to character inventory
    /// </summary>
    public static void AddMaterials(Character character, ForgeMaterialType materialType, int quantity)
    {
        if (character == null)
        {
            Debug.LogWarning("[ForgeMaterialManager] Cannot add materials: character is null.");
            return;
        }

        if (character.forgeMaterials == null)
        {
            character.forgeMaterials = new List<ForgeMaterialData>();
        }

        var existingMaterial = character.forgeMaterials.FirstOrDefault(m => m.materialType == materialType);
        if (existingMaterial != null)
        {
            existingMaterial.quantity += quantity;
        }
        else
        {
            character.forgeMaterials.Add(new ForgeMaterialData(materialType, quantity));
        }

        Debug.Log($"[ForgeMaterialManager] Added {quantity} {materialType} to {character.characterName}. Total: {GetMaterialQuantity(character, materialType)}");
    }

    /// <summary>
    /// Remove materials from character inventory
    /// </summary>
    public static bool RemoveMaterials(Character character, ForgeMaterialType materialType, int quantity)
    {
        if (character == null || character.forgeMaterials == null)
        {
            return false;
        }

        var existingMaterial = character.forgeMaterials.FirstOrDefault(m => m.materialType == materialType);
        if (existingMaterial == null || existingMaterial.quantity < quantity)
        {
            return false;
        }

        existingMaterial.quantity -= quantity;
        if (existingMaterial.quantity <= 0)
        {
            character.forgeMaterials.Remove(existingMaterial);
        }

        return true;
    }

    /// <summary>
    /// Check if character has enough materials
    /// </summary>
    public static bool HasMaterials(Character character, Dictionary<ForgeMaterialType, int> requiredMaterials)
    {
        if (character == null || requiredMaterials == null)
        {
            return false;
        }

        foreach (var requirement in requiredMaterials)
        {
            if (GetMaterialQuantity(character, requirement.Key) < requirement.Value)
            {
                return false;
            }
        }

        return true;
    }
}

