using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the display of Arcane Rebound bonus icons
/// </summary>
public static class ArcaneReboundIconManager
{
    private static Dictionary<Character, StatusEffect> activeBonusIcons = new Dictionary<Character, StatusEffect>();

    /// <summary>
    /// Shows an icon for the selected Arcane Rebound bonus.
    /// </summary>
    public static void ShowBonusIcon(Character character, string bonusType)
    {
        if (character == null) return;

        // Remove any existing icon first
        RemoveBonusIcon(character);

        string iconName = GetIconNameForBonusType(bonusType);
        if (string.IsNullOrEmpty(iconName))
        {
            Debug.LogWarning($"[ArcaneReboundIconManager] No icon name found for bonus type: {bonusType}");
            return;
        }

        // Create a temporary status effect for the icon
        StatusEffect bonusEffect = new StatusEffect(StatusEffectType.ArcaneReboundBonus, $"Arcane Rebound: {bonusType}", 0, 1, false);
        bonusEffect.iconName = $"ArcaneRebound/{iconName}"; // Path in Resources/StatusEffectIcons/ArcaneRebound/
        bonusEffect.description = $"Next attack gains {bonusType} bonus from Arcane Rebound.";
        bonusEffect.effectColor = GetColorForBonusType(bonusType);

        // Add to player's StatusEffectManager
        PlayerCombatDisplay playerDisplay = GameObject.FindFirstObjectByType<PlayerCombatDisplay>();
        if (playerDisplay != null)
        {
            StatusEffectManager statusManager = playerDisplay.GetStatusEffectManager();
            if (statusManager != null)
            {
                statusManager.AddStatusEffect(bonusEffect);
                activeBonusIcons[character] = bonusEffect;
                Debug.Log($"[ArcaneReboundIconManager] Displayed icon for Arcane Rebound bonus: {bonusType}");
            }
        }
    }

    /// <summary>
    /// Removes the active Arcane Rebound bonus icon.
    /// </summary>
    public static void RemoveBonusIcon(Character character)
    {
        if (character == null) return;

        if (activeBonusIcons.TryGetValue(character, out StatusEffect effect))
        {
            PlayerCombatDisplay playerDisplay = GameObject.FindFirstObjectByType<PlayerCombatDisplay>();
            if (playerDisplay != null)
            {
                StatusEffectManager statusManager = playerDisplay.GetStatusEffectManager();
                if (statusManager != null)
                {
                    statusManager.RemoveStatusEffect(effect);
                    activeBonusIcons.Remove(character);
                    Debug.Log($"[ArcaneReboundIconManager] Removed icon for Arcane Rebound bonus.");
                }
            }
        }
    }

    private static string GetIconNameForBonusType(string bonusType)
    {
        switch (bonusType)
        {
            case "damage": return "arcaneReboundMoreDamage";
            case "hits": return "arcaneReboundMultiHit";
            case "elemental": return "arcaneReboundEleDamage";
            default: return null;
        }
    }

    private static Color GetColorForBonusType(string bonusType)
    {
        switch (bonusType)
        {
            case "damage": return new Color(1f, 0.6f, 0.2f); // Orange
            case "hits": return new Color(0.2f, 0.8f, 1f); // Light Blue
            case "elemental": return new Color(0.8f, 0.4f, 1f); // Purple
            default: return Color.white;
        }
    }
}

