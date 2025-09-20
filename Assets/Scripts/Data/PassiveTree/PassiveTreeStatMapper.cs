using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Maps JSON stat names to Unity stat names for the passive tree system
    /// </summary>
    public static class PassiveTreeStatMapper
    {
        private static readonly Dictionary<string, string> statNameMapping = new Dictionary<string, string>
        {
            // Core attributes
            { "strength", "strength" },
            { "dexterity", "dexterity" },
            { "intelligence", "intelligence" },
            
            // Health and resources
            { "maxHealthIncrease", "maxhealth" },
            { "maxEnergyShieldIncrease", "maxenergyshield" },
            { "maxEnergyShield", "maxenergyshield" },
            
            // Combat stats
            { "armor", "armor" },
            { "armorIncrease", "armor" },
            { "evasion", "evasion" },
            { "increasedEvasion", "evasion" },
            { "elementalResist", "elementalresist" },
            { "accuracy", "accuracy" },
            
            // Damage stats
            { "spellPowerIncrease", "spellpower" },
            { "increasedProjectileDamage", "projectiledamage" },
            
            // Fire-specific stats
            { "fireIncrease", "firedamage" },
            { "fire", "fireresist" },
            { "chanceToIgnite", "ignitechance" },
            { "addedPhysicalAsFire", "physicalasfire" },
            { "increasedIgniteMagnitude", "ignitemagnitude" },
            { "addedFireAsCold", "fireascold" },
        };
        
        /// <summary>
        /// Maps a JSON stat name to a Unity stat name
        /// </summary>
        /// <param name="jsonStatName">The stat name from JSON</param>
        /// <returns>The corresponding Unity stat name, or the original name if no mapping exists</returns>
        public static string MapStatName(string jsonStatName)
        {
            if (string.IsNullOrEmpty(jsonStatName))
                return jsonStatName;
                
            // Try exact match first
            if (statNameMapping.ContainsKey(jsonStatName))
                return statNameMapping[jsonStatName];
                
            // Try case-insensitive match
            foreach (var kvp in statNameMapping)
            {
                if (string.Equals(kvp.Key, jsonStatName, System.StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
            
            // If no mapping found, return the original name (will be stored in equipmentStats)
            return jsonStatName;
        }
        
        /// <summary>
        /// Gets all available stat mappings
        /// </summary>
        /// <returns>Dictionary of JSON stat names to Unity stat names</returns>
        public static Dictionary<string, string> GetAllMappings()
        {
            return new Dictionary<string, string>(statNameMapping);
        }
        
        /// <summary>
        /// Validates stat mappings and returns list of unmapped stats
        /// </summary>
        /// <param name="allStats">List of all stat strings to validate</param>
        /// <returns>List of unmapped stat names</returns>
        public static List<string> ValidateStatMappings(List<string> allStats)
        {
            var unmappedStats = new List<string>();
            
            foreach (string statString in allStats)
            {
                if (string.IsNullOrEmpty(statString))
                    continue;
                    
                // Extract stat name from "statName: value" format
                string statName = statString.Split(':')[0].Trim();
                
                // Check if this stat is mapped
                if (!statNameMapping.ContainsKey(statName))
                {
                    // Check case-insensitive
                    bool found = false;
                    foreach (var kvp in statNameMapping)
                    {
                        if (string.Equals(kvp.Key, statName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    
                    if (!found)
                    {
                        unmappedStats.Add(statName);
                    }
                }
            }
            
            return unmappedStats;
        }
        
        /// <summary>
        /// Converts stat format from TypeScript to Unity format
        /// </summary>
        /// <param name="stats">List of stat strings in TypeScript format</param>
        /// <returns>List of stat strings in Unity format</returns>
        public static List<string> ConvertStatFormat(List<string> stats)
        {
            var convertedStats = new List<string>();
            
            foreach (string stat in stats)
            {
                if (string.IsNullOrEmpty(stat))
                    continue;
                    
                // Map the stat name to Unity format
                string mappedStatName = MapStatName(stat);
                convertedStats.Add(mappedStatName);
            }
            
            return convertedStats;
        }
    }
}