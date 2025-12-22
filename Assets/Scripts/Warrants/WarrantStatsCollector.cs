using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Collects and aggregates warrant modifiers by category for display in the Active Warrant Stats panel.
/// Groups modifiers by statKey and category (Damage, Defense, Attributes, etc.)
/// </summary>
public static class WarrantStatsCollector
{
    /// <summary>
    /// Represents a grouped warrant stat with its total value and contributing warrants
    /// </summary>
    public class WarrantStatEntry
    {
        public string statKey;
        public string displayName;
        public float totalValue;
        public int contributorCount;
        public string category;
        public bool isFlat;
        public List<string> contributingWarrants = new List<string>();
    }

    /// <summary>
    /// Collect all active warrant stats grouped by category
    /// </summary>
    public static Dictionary<string, List<WarrantStatEntry>> CollectActiveWarrantStats(
        WarrantBoardStateController boardState, 
        WarrantLockerGrid lockerGrid, 
        WarrantBoardRuntimeGraph runtimeGraph)
    {
        var result = new Dictionary<string, List<WarrantStatEntry>>();
        
        if (boardState == null || lockerGrid == null || runtimeGraph == null)
            return result;

        // Collect all modifiers from active warrants
        var allModifiers = WarrantModifierCollector.CollectActiveWarrantModifiers(boardState, lockerGrid, runtimeGraph);
        
        // Group by statKey and aggregate
        var groupedByStatKey = new Dictionary<string, WarrantStatEntry>();
        var warrantToStatKeys = new Dictionary<string, HashSet<string>>(); // Track which warrants contribute to which stats
        
        var activePage = boardState.ActivePage;
        if (activePage == null)
            return result;

        // Get all socket nodes with warrants to track contributing warrants
        var socketNodes = runtimeGraph.Nodes.Values
            .Where(n => (n.NodeType == WarrantNodeType.Socket || n.NodeType == WarrantNodeType.SpecialSocket) 
                     && activePage.IsNodeUnlocked(n.Id));

        foreach (var socketNode in socketNodes)
        {
            string warrantId = boardState.GetWarrantAtNode(socketNode.Id);
            if (string.IsNullOrEmpty(warrantId))
                continue;
            
            WarrantDefinition warrant = lockerGrid.GetDefinition(warrantId);
            if (warrant == null)
                continue;

            string warrantName = !string.IsNullOrWhiteSpace(warrant.displayName) 
                ? warrant.displayName 
                : warrantId;

            if (!warrantToStatKeys.ContainsKey(warrantName))
            {
                warrantToStatKeys[warrantName] = new HashSet<string>();
            }
        }

        // Process all modifiers
        foreach (var modifier in allModifiers)
        {
            if (modifier == null)
                continue;

            // Skip modifiers without modifierId (they can't be properly categorized)
            if (string.IsNullOrEmpty(modifier.modifierId))
                continue;

            // Skip socket-only modifiers (they're already applied to character, but we want to show effect node modifiers)
            if (modifier.modifierId.StartsWith("__SOCKET_ONLY__", System.StringComparison.OrdinalIgnoreCase))
                continue;

            string statKey = modifier.modifierId.ToLowerInvariant();
            
            // Determine if this is a flat stat
            bool isFlat = statKey.Contains("flat") || 
                         (!string.IsNullOrWhiteSpace(modifier.displayName) && 
                          !modifier.displayName.Contains("%") && 
                          modifier.displayName.StartsWith("+"));

            // Get or create entry
            if (!groupedByStatKey.TryGetValue(statKey, out var entry))
            {
                entry = new WarrantStatEntry
                {
                    statKey = modifier.modifierId, // Keep original case for display
                    displayName = !string.IsNullOrWhiteSpace(modifier.displayName) 
                        ? modifier.displayName 
                        : modifier.modifierId,
                    totalValue = 0f,
                    contributorCount = 0,
                    category = GetCategoryForStatKey(statKey),
                    isFlat = isFlat
                };
                groupedByStatKey[statKey] = entry;
            }

            // Aggregate value
            entry.totalValue += modifier.value;
            entry.contributorCount++;
        }

        // Group by category
        foreach (var entry in groupedByStatKey.Values)
        {
            if (!result.ContainsKey(entry.category))
            {
                result[entry.category] = new List<WarrantStatEntry>();
            }
            result[entry.category].Add(entry);
        }

        // Sort each category by statKey
        foreach (var category in result.Keys.ToList())
        {
            result[category] = result[category].OrderBy(e => e.statKey).ToList();
        }

        return result;
    }

    /// <summary>
    /// Determine the category for a stat key
    /// </summary>
    private static string GetCategoryForStatKey(string statKeyLower)
    {
        // Damage modifiers
        if (statKeyLower.Contains("damage") || 
            statKeyLower.Contains("increasedphysical") ||
            statKeyLower.Contains("increasedfire") ||
            statKeyLower.Contains("increasedcold") ||
            statKeyLower.Contains("increasedlightning") ||
            statKeyLower.Contains("increasedchaos") ||
            statKeyLower.Contains("increasedelemental") ||
            statKeyLower.Contains("increasedattack") ||
            statKeyLower.Contains("increasedspell") ||
            statKeyLower.Contains("increasedprojectile") ||
            statKeyLower.Contains("increasedarea") ||
            statKeyLower.Contains("increasedmelee") ||
            statKeyLower.Contains("increasedranged") ||
            statKeyLower.Contains("increasedaxe") ||
            statKeyLower.Contains("increasedbow") ||
            statKeyLower.Contains("increasedmace") ||
            statKeyLower.Contains("increasedsword") ||
            statKeyLower.Contains("increasedwand") ||
            statKeyLower.Contains("increaseddagger") ||
            statKeyLower.Contains("increasedonehanded") ||
            statKeyLower.Contains("increasedtwohanded") ||
            statKeyLower.Contains("increasedpoison"))
        {
            return "Damage";
        }

        // Defense modifiers
        if (statKeyLower.Contains("armour") ||
            statKeyLower.Contains("evasion") ||
            statKeyLower.Contains("energyshield") ||
            statKeyLower.Contains("maxhealth") ||
            statKeyLower.Contains("maxmana") ||
            statKeyLower.Contains("guard"))
        {
            return "Defense";
        }

        // Attributes
        if (statKeyLower == "strength" ||
            statKeyLower == "dexterity" ||
            statKeyLower == "intelligence")
        {
            return "Attributes";
        }

        // Ailments/Status Effects
        if (statKeyLower.Contains("ignite") ||
            statKeyLower.Contains("shock") ||
            statKeyLower.Contains("chill") ||
            statKeyLower.Contains("freeze") ||
            statKeyLower.Contains("bleed") ||
            statKeyLower.Contains("poison") ||
            statKeyLower.Contains("magnitude") ||
            statKeyLower.Contains("duration") ||
            statKeyLower.Contains("status"))
        {
            return "Ailments";
        }

        // Speed/Utility
        if (statKeyLower.Contains("speed") ||
            statKeyLower.Contains("cast") ||
            statKeyLower.Contains("attack") ||
            statKeyLower.Contains("aggression") ||
            statKeyLower.Contains("focus") ||
            statKeyLower.Contains("regeneration") ||
            statKeyLower.Contains("avoidance"))
        {
            return "Utility";
        }

        // Resistances
        if (statKeyLower.Contains("resistance") ||
            statKeyLower.Contains("resist"))
        {
            return "Resistances";
        }

        // Default category
        return "Other";
    }

    /// <summary>
    /// Format a stat value for display
    /// </summary>
    public static string FormatStatValue(WarrantStatEntry entry)
    {
        if (entry.isFlat)
        {
            return $"+{Mathf.RoundToInt(entry.totalValue)}";
        }
        else
        {
            return $"{Mathf.RoundToInt(entry.totalValue):+0;-0;0}%";
        }
    }
}

