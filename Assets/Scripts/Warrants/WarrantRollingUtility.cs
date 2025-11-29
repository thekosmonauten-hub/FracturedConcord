using UnityEngine;

/// <summary>
/// Utility class for rolling warrants from blueprints and adding them to the player's locker.
/// Used for quest rewards, drops, and testing.
/// </summary>
public static class WarrantRollingUtility
{
    /// <summary>
    /// Rolls a warrant instance from a blueprint and adds it to the player's locker.
    /// Rarity is automatically determined by affix count:
    /// - 1 Notable + 1 affix = Common
    /// - 1 Notable + 2 affixes = Magic
    /// - 1 Notable + 3 affixes = Rare
    /// - 1 Notable + 4 affixes = Rare
    /// </summary>
    /// <param name="blueprint">The blueprint warrant to roll from (must have isBlueprint = true)</param>
    /// <param name="warrantDatabase">The WarrantDatabase containing the blueprint and affix database</param>
    /// <param name="lockerGrid">The WarrantLockerGrid to add the rolled warrant to</param>
    /// <param name="minAffixes">Minimum number of affixes to roll (default: 1)</param>
    /// <param name="maxAffixes">Maximum number of affixes to roll (default: 3)</param>
    /// <returns>The rolled warrant instance, or null if rolling failed</returns>
    public static WarrantDefinition RollAndAddToLocker(
        WarrantDefinition blueprint,
        WarrantDatabase warrantDatabase,
        WarrantLockerGrid lockerGrid,
        int minAffixes = 1,
        int maxAffixes = 3)
    {
        if (blueprint == null)
        {
            Debug.LogWarning("[WarrantRollingUtility] Cannot roll from null blueprint.");
            return null;
        }

        if (warrantDatabase == null)
        {
            Debug.LogWarning("[WarrantRollingUtility] WarrantDatabase is null. Cannot roll warrant.");
            return null;
        }

        if (lockerGrid == null)
        {
            Debug.LogWarning("[WarrantRollingUtility] WarrantLockerGrid is null. Cannot add warrant to locker.");
            return null;
        }

        // Roll the warrant instance from the blueprint (rarity is automatically calculated from affix count)
        WarrantDefinition rolledInstance = warrantDatabase.CreateInstanceFromBlueprint(blueprint, minAffixes, maxAffixes);
        
        if (rolledInstance == null)
        {
            Debug.LogWarning($"[WarrantRollingUtility] Failed to roll warrant from blueprint '{blueprint.warrantId}'.");
            return null;
        }

        // Add to locker
        lockerGrid.AddWarrantInstance(rolledInstance);

        Debug.Log($"[WarrantRollingUtility] Successfully rolled and added warrant '{rolledInstance.warrantId}' from blueprint '{blueprint.warrantId}' to locker.");
        
        return rolledInstance;
    }

    /// <summary>
    /// Rolls a warrant instance from a blueprint by ID and adds it to the player's locker.
    /// Rarity is automatically determined by affix count (see RollAndAddToLocker for rules).
    /// </summary>
    /// <param name="blueprintId">The ID of the blueprint warrant to roll from</param>
    /// <param name="warrantDatabase">The WarrantDatabase containing the blueprint</param>
    /// <param name="lockerGrid">The WarrantLockerGrid to add the rolled warrant to</param>
    /// <param name="minAffixes">Minimum number of affixes to roll (default: 1)</param>
    /// <param name="maxAffixes">Maximum number of affixes to roll (default: 3)</param>
    /// <returns>The rolled warrant instance, or null if rolling failed</returns>
    public static WarrantDefinition RollAndAddToLockerById(
        string blueprintId,
        WarrantDatabase warrantDatabase,
        WarrantLockerGrid lockerGrid,
        int minAffixes = 1,
        int maxAffixes = 3)
    {
        if (string.IsNullOrEmpty(blueprintId))
        {
            Debug.LogWarning("[WarrantRollingUtility] Cannot roll from empty blueprint ID.");
            return null;
        }

        if (warrantDatabase == null)
        {
            Debug.LogWarning("[WarrantRollingUtility] WarrantDatabase is null. Cannot roll warrant.");
            return null;
        }

        WarrantDefinition blueprint = warrantDatabase.GetById(blueprintId);
        if (blueprint == null)
        {
            Debug.LogWarning($"[WarrantRollingUtility] Blueprint with ID '{blueprintId}' not found in database.");
            return null;
        }

        return RollAndAddToLocker(blueprint, warrantDatabase, lockerGrid, minAffixes, maxAffixes);
    }
}

