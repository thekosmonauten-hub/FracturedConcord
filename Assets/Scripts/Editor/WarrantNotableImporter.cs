using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Unity Editor tool for importing Notable affixes from JSON files into WarrantNotableDatabase.
/// Supports the JSON structure from WarrantDatabase.md (lines 233-490).
/// Menu: Tools > Warrants > Import Notable Affixes from JSON
/// </summary>
public class WarrantNotableImporter : EditorWindow
{
    private string jsonFilePath = "Assets/Documentation/WarrantNotables.json";
    private WarrantNotableDatabase targetDatabase;
    private Vector2 scrollPosition;
    private string importLog = "";
    private bool appendToExisting = true;
    
    [System.Serializable]
    private class NotableJSONEntry
    {
        public string name;
        public string[] stats;
        public string[] tags;
        public int weight;
    }
    
    [MenuItem("Tools/Warrants/Import Notable Affixes from JSON")]
    public static void ShowWindow()
    {
        var window = GetWindow<WarrantNotableImporter>("Notable Affix Importer");
        window.minSize = new Vector2(600, 700);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Import Notable affixes from JSON files into your WarrantNotableDatabase.\n" +
            "Expected format: Array of objects with 'name', 'stats', 'tags', and 'weight' fields.\n" +
            "Each Notable will be added as its own entry with modifiers nested within it.", 
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // File selection
        EditorGUILayout.BeginHorizontal();
        jsonFilePath = EditorGUILayout.TextField("JSON File Path:", jsonFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select JSON File", "Assets/Documentation", "json,md,txt");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    jsonFilePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    jsonFilePath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Database selection
        targetDatabase = (WarrantNotableDatabase)EditorGUILayout.ObjectField(
            "Target Database:", 
            targetDatabase, 
            typeof(WarrantNotableDatabase), 
            false
        );
        
        EditorGUILayout.Space();
        
        appendToExisting = EditorGUILayout.Toggle(
            "Append to Existing (don't clear Notable entries)", 
            appendToExisting
        );
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate JSON", GUILayout.Height(30)))
        {
            ValidateJSON();
        }
        if (GUILayout.Button("Import Notables", GUILayout.Height(30)))
        {
            ImportNotables();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Import log
        EditorGUILayout.LabelField("Import Log:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        EditorGUILayout.TextArea(importLog, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }
    
    private void ValidateJSON()
    {
        importLog = "Validating JSON...\n";
        
        if (!File.Exists(jsonFilePath))
        {
            LogError($"File not found: {jsonFilePath}");
            return;
        }
        
        try
        {
            string content = File.ReadAllText(jsonFilePath);
            
            // Try to extract JSON array from markdown file
            if (jsonFilePath.EndsWith(".md") || jsonFilePath.EndsWith(".txt"))
            {
                // Look for JSON array pattern
                var match = Regex.Match(content, @"\[[\s\S]*\]");
                if (match.Success)
                {
                    content = match.Value;
                    LogInfo("Extracted JSON array from markdown file.");
                }
                else
                {
                    LogError("Could not find JSON array in markdown file. Expected format: [ {...}, {...} ]");
                    return;
                }
            }
            
            // Parse JSON
            var entries = ParseJSONArray(content);
            if (entries == null || entries.Count == 0)
            {
                LogError("No valid Notable entries found in JSON.");
                return;
            }
            
            LogSuccess($"✓ Found {entries.Count} Notable entries");
            
            int totalStats = 0;
            foreach (var entry in entries)
            {
                if (entry.stats != null)
                    totalStats += entry.stats.Length;
            }
            
            LogInfo($"Total stat entries: {totalStats}");
            LogInfo($"Sample entry: {entries[0].name} ({entries[0].stats?.Length ?? 0} stats)");
        }
        catch (System.Exception e)
        {
            LogError($"Validation Failed: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void ImportNotables()
    {
        if (targetDatabase == null)
        {
            LogError("Please select a target WarrantNotableDatabase");
            return;
        }
        
        if (!File.Exists(jsonFilePath))
        {
            LogError($"File not found: {jsonFilePath}");
            return;
        }
        
        try
        {
            string content = File.ReadAllText(jsonFilePath);
            
            // Extract JSON array from markdown if needed
            if (jsonFilePath.EndsWith(".md") || jsonFilePath.EndsWith(".txt"))
            {
                var match = Regex.Match(content, @"\[[\s\S]*\]");
                if (match.Success)
                {
                    content = match.Value;
                }
                else
                {
                    LogError("Could not find JSON array in markdown file.");
                    return;
                }
            }
            
            var entries = ParseJSONArray(content);
            if (entries == null || entries.Count == 0)
            {
                LogError("No valid Notable entries found.");
                return;
            }
            
            importLog = $"Importing {entries.Count} Notables...\n\n";
            
            // Clear existing Notable entries if not appending
            if (!appendToExisting)
            {
                targetDatabase.ClearAll();
                LogInfo("Cleared existing Notable entries.");
            }
            
            int successCount = 0;
            int totalModifierCount = 0;
            
            foreach (var notableJson in entries)
            {
                if (string.IsNullOrWhiteSpace(notableJson.name))
                {
                    LogWarning($"Skipping Notable with empty name");
                    continue;
                }
                
                if (notableJson.stats == null || notableJson.stats.Length == 0)
                {
                    LogWarning($"Skipping Notable '{notableJson.name}' - no stats defined");
                    continue;
                }
                
                // Create a new Notable entry
                var notableEntry = new WarrantNotableDatabase.NotableEntry
                {
                    notableId = GenerateNotableId(notableJson.name),
                    displayName = notableJson.name,
                    description = "", // Can be populated from JSON if available
                    weight = notableJson.weight > 0 ? notableJson.weight : 100,
                    tags = notableJson.tags != null ? new List<string>(notableJson.tags) : new List<string>(),
                    modifiers = new List<WarrantNotableDatabase.NotableModifier>()
                };
                
                // Parse each stat string and add as a modifier
                int modifierCount = 0;
                foreach (var statString in notableJson.stats)
                {
                    if (string.IsNullOrWhiteSpace(statString))
                        continue;
                    
                    // Handle "and" in stat strings (e.g., "24% increased Evasion and Armour")
                    // Split into multiple modifiers
                    var statParts = SplitStatString(statString);
                    
                    foreach (var statPart in statParts)
                    {
                        var modifier = ParseStatStringToModifier(statPart);
                        if (modifier != null)
                        {
                            notableEntry.modifiers.Add(modifier);
                            modifierCount++;
                            totalModifierCount++;
                        }
                    }
                }
                
                if (modifierCount > 0)
                {
                    targetDatabase.AddNotable(notableEntry);
                    successCount++;
                    LogInfo($"✓ Imported '{notableJson.name}': {modifierCount} modifier entries");
                }
                else
                {
                    LogWarning($"✗ Failed to parse any stats for '{notableJson.name}'");
                }
            }
            
            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();
            
            LogSuccess($"\n✓ Import complete: {successCount}/{entries.Count} Notables, {totalModifierCount} total modifier entries");
        }
        catch (System.Exception e)
        {
            LogError($"Import Failed: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private string GenerateNotableId(string displayName)
    {
        // Convert display name to ID format (e.g., "Call to Arms" -> "call_to_arms")
        return displayName.ToLower()
            .Replace(" ", "_")
            .Replace("'", "")
            .Replace("-", "_")
            .Replace(".", "")
            .Trim();
    }
    
    private List<NotableJSONEntry> ParseJSONArray(string jsonContent)
    {
        // Simple JSON array parser (handles the format from WarrantDatabase.md)
        // For more complex JSON, consider using a proper JSON library
        
        var entries = new List<NotableJSONEntry>();
        
        // Remove whitespace and newlines for easier parsing
        jsonContent = jsonContent.Trim();
        if (!jsonContent.StartsWith("["))
        {
            throw new System.Exception("JSON content does not start with '['");
        }
        
        // Use Unity's JsonUtility with a wrapper class
        // Since JsonUtility doesn't support arrays directly, we'll parse manually
        // or use a simple regex-based approach
        
        // For now, use a simple approach: split by }, and parse each entry
        var entryMatches = Regex.Matches(jsonContent, @"\{[^}]+\}");
        
        foreach (Match match in entryMatches)
        {
            try
            {
                string entryJson = match.Value;
                var entry = JsonUtility.FromJson<NotableJSONEntry>(entryJson);
                if (entry != null && !string.IsNullOrWhiteSpace(entry.name))
                {
                    entries.Add(entry);
                }
            }
            catch (System.Exception e)
            {
                LogWarning($"Failed to parse entry: {match.Value.Substring(0, Mathf.Min(50, match.Value.Length))}... Error: {e.Message}");
            }
        }
        
        return entries;
    }
    
    private List<string> SplitStatString(string statString)
    {
        var results = new List<string>();
        
        // Check if stat string contains " and " (case-insensitive)
        if (Regex.IsMatch(statString, @"\s+and\s+", RegexOptions.IgnoreCase))
        {
            // Split by " and " but preserve the percentage/value prefix
            var parts = Regex.Split(statString, @"\s+and\s+", RegexOptions.IgnoreCase);
            
            if (parts.Length > 1)
            {
                // Extract the value prefix from the first part
                var valueMatch = Regex.Match(parts[0], @"([+-]?\d+(?:\.\d+)?\s*%)?\s*(.+)");
                string valuePrefix = valueMatch.Groups[1].Value;
                string firstStat = valueMatch.Groups[2].Value.Trim();
                
                // Add first stat with value
                if (!string.IsNullOrWhiteSpace(valuePrefix))
                {
                    results.Add($"{valuePrefix} {firstStat}");
                }
                else
                {
                    results.Add(firstStat);
                }
                
                // Add remaining stats (they may or may not have values)
                for (int i = 1; i < parts.Length; i++)
                {
                    string part = parts[i].Trim();
                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        // If the part doesn't start with a value, reuse the prefix
                        if (!Regex.IsMatch(part, @"^[+-]?\d"))
                        {
                            results.Add($"{valuePrefix} {part}");
                        }
                        else
                        {
                            results.Add(part);
                        }
                    }
                }
            }
            else
            {
                results.Add(statString);
            }
        }
        else
        {
            results.Add(statString);
        }
        
        return results;
    }
    
    private WarrantNotableDatabase.NotableModifier ParseStatStringToModifier(string statString)
    {
        if (string.IsNullOrWhiteSpace(statString))
            return null;
        
        statString = statString.Trim();
        
        // Parse patterns like:
        // "24% increased Evasion and Armour" -> multiple modifiers needed (handled by SplitStatString)
        // "8% increased Maximum Life" -> single modifier
        // "+20 Strength" -> flat stat
        // "Regenerate 0.5% Life per turn" -> special effect (may not map to stat)
        
        var modifier = new WarrantNotableDatabase.NotableModifier
        {
            socketOnly = true, // Notables are socket-only by default
            displayName = statString // Use the original string as display name
        };
        
        // Try to extract percentage value
        var percentMatch = Regex.Match(statString, @"([+-]?\d+(?:\.\d+)?)\s*%");
        if (percentMatch.Success)
        {
            float value = float.Parse(percentMatch.Groups[1].Value);
            modifier.value = value;
        }
        else
        {
            // Try flat value (e.g., "+20 Strength")
            var flatMatch = Regex.Match(statString, @"\+(\d+(?:\.\d+)?)\s+");
            if (flatMatch.Success)
            {
                float value = float.Parse(flatMatch.Groups[1].Value);
                modifier.value = value;
            }
            else
            {
                // Default: assume 0 value (will need manual adjustment)
                modifier.value = 0f;
                LogWarning($"Could not parse value from stat: {statString}");
            }
        }
        
        // Try to map stat string to statKey
        string statKey = MapStatStringToKey(statString);
        modifier.statKey = statKey;
        
        return modifier;
    }
    
    private string MapStatStringToKey(string statString)
    {
        // Map common stat descriptions to CharacterStatsData keys
        statString = statString.ToLower();
        
        // Critical Strike (check FIRST before weapon-specific damage)
        // Check for "crit" or "critical" first, then check modifiers
        if (statString.Contains("crit") || statString.Contains("critical"))
        {
            // Critical Strike Chance
            if (statString.Contains("chance"))
            {
                if (statString.Contains("spell") || statString.Contains("spell crit"))
                    return "increasedSpellCriticalChance";
                if (statString.Contains("dagger") || statString.Contains("daggers") || statString.Contains("with daggers"))
                    return "increasedCriticalChanceWithDaggers";
                if (statString.Contains("sword") || statString.Contains("swords") || statString.Contains("with swords"))
                    return "increasedCriticalChanceWithSwords";
                if (statString.Contains("cold") || statString.Contains("cold skills") || statString.Contains("with cold"))
                    return "increasedCriticalChanceWithColdSkills";
                if (statString.Contains("lightning") || statString.Contains("lightning cards") || statString.Contains("with lightning"))
                    return "increasedCriticalChanceWithLightningCards";
                if (statString.Contains("projectile") || statString.Contains("projectile cards") || statString.Contains("with projectile"))
                    return "increasedCriticalChanceWithProjectileCards";
                if (statString.Contains("prepared") || statString.Contains("prepared cards") || statString.Contains("on prepared"))
                    return "increasedCriticalChanceWithPreparedCards";
                if (statString.Contains("full life") || statString.Contains("enemies on full life") || statString.Contains("against enemies on full life") || (statString.Contains("against") && statString.Contains("full life")))
                    return "increasedCriticalChanceVsFullLife";
                if (statString.Contains("cursed") || statString.Contains("vs cursed") || statString.Contains("against cursed"))
                    return "increasedCriticalChanceVsCursed";
                return "increasedCriticalStrikeChance";
            }
            // Critical Strike Multiplier
            if (statString.Contains("multiplier") || statString.Contains("damage"))
            {
                if (statString.Contains("spell"))
                    return "increasedSpellCriticalMultiplier";
                if (statString.Contains("dagger") || statString.Contains("daggers") || statString.Contains("with daggers"))
                    return "increasedCriticalMultiplierWithDaggers";
                if (statString.Contains("sword") || statString.Contains("swords") || statString.Contains("with swords"))
                    return "increasedCriticalMultiplierWithSwords";
                if (statString.Contains("damage"))
                    return "increasedCriticalDamageMultiplier";
                return "increasedCriticalStrikeMultiplier";
            }
        }
        
        // Defense/Resource
        if (statString.Contains("evasion") && statString.Contains("armour"))
            return "evasionIncreased"; // Will need separate entries, but use one for now
        if (statString.Contains("maximum life") || statString.Contains("max life"))
            return "maxHealthIncreased";
        if (statString.Contains("maximum mana") || statString.Contains("max mana"))
            return "maxManaIncreased";
        if (statString.Contains("armor") || statString.Contains("armour"))
            return "armourIncreased";
        if (statString.Contains("evasion"))
            return "evasionIncreased";
        if (statString.Contains("energy shield"))
            return "energyShieldIncreased";
        
        // Damage types
        if (statString.Contains("physical damage"))
            return "increasedPhysicalDamage";
        if (statString.Contains("fire damage"))
            return "increasedFireDamage";
        if (statString.Contains("cold damage"))
            return "increasedColdDamage";
        if (statString.Contains("lightning damage"))
            return "increasedLightningDamage";
        if (statString.Contains("chaos damage"))
            return "increasedChaosDamage";
        if (statString.Contains("elemental damage"))
            return "increasedElementalDamage";
        if (statString.Contains("spell damage"))
            return "increasedSpellDamage";
        if (statString.Contains("attack damage"))
            return "increasedAttackDamage";
        if (statString.Contains("melee damage"))
            return "increasedMeleeDamage";
        if (statString.Contains("projectile damage"))
            return "increasedProjectileDamage";
        if (statString.Contains("area damage"))
            return "increasedAreaDamage";
        
        // Weapon types (damage only - crit already handled above)
        if (statString.Contains("dagger") && !statString.Contains("crit") && !statString.Contains("critical"))
        {
            if (statString.Contains("damage") || statString.Contains("increased"))
                return "increasedDaggerDamage";
            // If just "dagger" without damage, assume damage
            return "increasedDaggerDamage";
        }
        if (statString.Contains("axe"))
            return "increasedAxeDamage";
        if (statString.Contains("bow"))
            return "increasedBowDamage";
        if (statString.Contains("mace"))
            return "increasedMaceDamage";
        if (statString.Contains("sword"))
            return "increasedSwordDamage";
        if (statString.Contains("wand"))
            return "increasedWandDamage";
        if (statString.Contains("one-handed"))
            return "increasedOneHandedDamage";
        if (statString.Contains("two-handed"))
            return "increasedTwoHandedDamage";
        
        // Ailments
        if (statString.Contains("bleed"))
        {
            if (statString.Contains("duration"))
                return "increasedBleedDuration";
            return "increasedBleedMagnitude";
        }
        if (statString.Contains("poison"))
        {
            if (statString.Contains("duration"))
                return "increasedPoisonDuration";
            return "increasedPoisonDamage";
        }
        if (statString.Contains("ignite"))
        {
            if (statString.Contains("duration"))
                return "increasedIgniteDuration";
            return "increasedIgniteMagnitude";
        }
        if (statString.Contains("shock"))
        {
            if (statString.Contains("duration"))
                return "increasedShockDuration";
            return "increasedShockMagnitude";
        }
        if (statString.Contains("chill") || statString.Contains("freeze"))
        {
            if (statString.Contains("duration"))
                return "increasedChillDuration";
            return "increasedChillMagnitude";
        }
        
        
        // Speed
        if (statString.Contains("attack speed"))
            return "attackSpeed";
        if (statString.Contains("cast speed"))
            return "castSpeed";
        if (statString.Contains("movement speed") || (statString.Contains("increased") && statString.Contains("movement speed")) || (statString.Contains("+") && statString.Contains("movement speed")))
            return "movementSpeedIncreased";
        
        // Guard
        if (statString.Contains("guard effectiveness"))
        {
            if (statString.Contains("increased") || statString.Contains("%"))
                return "guardEffectivenessIncreased";
            return "guardEffectiveness";
        }
        if (statString.Contains("guard retention") || (statString.Contains("retain") && statString.Contains("guard")) || (statString.Contains("retain") && statString.Contains("more guard")) || (statString.Contains("retain") && statString.Contains("%") && statString.Contains("guard")))
            return "guardRetentionIncreased";
        if (statString.Contains("guard recovery") || (statString.Contains("increased") && statString.Contains("guard recovery")))
            return "guardRetentionIncreased"; // Guard recovery = retention
        if (statString.Contains("damage reduction") && statString.Contains("guard"))
            return "damageReductionWhileGuarding";
        if (statString.Contains("damage reduction while guarding") || (statString.Contains("+") && statString.Contains("damage reduction") && statString.Contains("guarding")))
            return "damageReductionWhileGuarding";
        if (statString.Contains("guard break") || statString.Contains("break guard") || (statString.Contains("chance") && statString.Contains("break guard")))
            return "guardBreakChance";
        
        // Block
        if (statString.Contains("block chance") || statString.Contains("increased block chance") || (statString.Contains("+") && statString.Contains("block chance")))
            return "blockChanceIncreased";
        if (statString.Contains("block effect") || statString.Contains("block effectiveness") || (statString.Contains("+") && statString.Contains("block effect")))
            return "blockEffectivenessIncreased";
        
        // Dodge
        if (statString.Contains("dodge chance") || statString.Contains("increased dodge chance") || (statString.Contains("+") && statString.Contains("dodge chance")))
            return "dodgeChanceIncreased";
        
        // Avoidance
        if (statString.Contains("critical strike avoidance") || statString.Contains("crit avoidance") || (statString.Contains("+") && statString.Contains("critical strike avoidance")))
            return "criticalStrikeAvoidance";
        if ((statString.Contains("debuff") || statString.Contains("debuffs")) && (statString.Contains("expire") || statString.Contains("faster")))
            return "debuffExpirationRateIncreased";
        if (statString.Contains("resist being debuffed") || statString.Contains("chance to resist") || (statString.Contains("+") && statString.Contains("chance to resist")))
            return "statusAvoidance"; // Use statusAvoidance for debuff resistance
        
        // Resistance
        if (statString.Contains("physical resistance") || (statString.Contains("+") && statString.Contains("physical resistance")) || (statString.Contains("increased") && statString.Contains("physical resistance")))
            return "physicalResistanceIncreased";
        if (statString.Contains("all elemental resistances") || statString.Contains("all elemental res") || statString.Contains("to all elemental resistances") || (statString.Contains("+") && statString.Contains("all elemental")) || (statString.Contains("+") && statString.Contains("to all elemental")))
            return "allElementalResistancesIncreased";
        if ((statString.Contains("penetration") || statString.Contains("penetrates")) && (statString.Contains("fire") || statString.Contains("fire resistance")))
            return "fireResistancePenetration";
        if (statString.Contains("damage penetrates") && (statString.Contains("fire") || statString.Contains("resistance") || statString.Contains("enemy fire")))
            return "fireResistancePenetration";
        
        // Regeneration & Recovery
        // Check for flat regeneration first (e.g., "regenerate X% life per turn")
        if (statString.Contains("regenerate") && statString.Contains("life"))
        {
            // Flat life regen per turn (e.g., "regenerate 0.5% life per turn")
            return "lifeRegeneration"; // Use base field for flat values
        }
        if (statString.Contains("life regeneration") || statString.Contains("life regen") || (statString.Contains("increased") && statString.Contains("life regeneration")))
            return "lifeRegenerationIncreased";
        if (statString.Contains("life recovery rate") || statString.Contains("life recovery") || statString.Contains("life recovery per turn") || (statString.Contains("+") && statString.Contains("life recovery")) || (statString.Contains("+") && statString.Contains("life recovery per turn")) || (statString.Contains("+") && statString.Contains("%") && statString.Contains("life recovery")))
            return "lifeRecoveryRateIncreased";
        if (statString.Contains("recovery") && !statString.Contains("life") && !statString.Contains("mana") && !statString.Contains("guard") && !statString.Contains("guard"))
            return "lifeRecoveryRateIncreased"; // Generic "recovery" likely means life recovery
        if (statString.Contains("mana regeneration") || statString.Contains("mana regen") || (statString.Contains("increased") && statString.Contains("mana regeneration")) || (statString.Contains("+") && statString.Contains("increased") && statString.Contains("mana regeneration")))
            return "manaRegenerationIncreased";
        
        // Life/Mana Steal
        if (statString.Contains("life steal") || statString.Contains("lifesteal") || statString.Contains("life leech"))
        {
            if (statString.Contains("increased"))
                return "lifeStealOnHitIncreased";
            if (statString.Contains("on hit") || statString.Contains("on hit"))
                return "lifeSteal";
            return "lifeSteal"; // Default to base lifeSteal
        }
        if (statString.Contains("life on kill") || statString.Contains("life on kill"))
            return "lifeOnKill";
        if (statString.Contains("mana leech") || statString.Contains("leech mana") || statString.Contains("chance to leech mana") || (statString.Contains("+") && statString.Contains("chance to leech mana")))
            return "manaLeechChance";
        
        // Damage over time
        if (statString.Contains("damage over time") || statString.Contains("dot"))
            return "increasedDamageOverTime";
        
        // Conditional Damage Modifiers
        if (statString.Contains("damage") && statString.Contains("cursed") && (statString.Contains("vs") || statString.Contains("against") || statString.Contains("with")))
            return "increasedDamageVsCursed";
        if (statString.Contains("damage vs cursed") || statString.Contains("damage against cursed") || statString.Contains("damage with cursed") || statString.Contains("increased damage with cursed"))
            return "increasedDamageVsCursed";
        if (statString.Contains("damage vs blocked") || statString.Contains("damage against blocked"))
            return "increasedDamageVsBlocked";
        if (statString.Contains("damage vs slowed") || statString.Contains("damage against slowed"))
            return "increasedDamageVsSlowed";
        if (statString.Contains("damage while in shadow") || statString.Contains("damage in shadow"))
            return "increasedDamageWhileInShadow";
        if (statString.Contains("damage on critical strikes") || statString.Contains("damage when you crit") || statString.Contains("damage on critical"))
            return "increasedDamageOnCriticalStrikes";
        if (statString.Contains("damage after discard") || statString.Contains("damage after discarding") || (statString.Contains("gain") && statString.Contains("damage") && statString.Contains("discard")))
            return "increasedDamageAfterDiscard";
        if ((statString.Contains("damage when below") || statString.Contains("damage below")) && statString.Contains("life"))
            return "increasedDamageWhenBelowLifeThreshold";
        if (statString.Contains("damage vs rare") || statString.Contains("damage vs unique") || statString.Contains("damage vs elite") || statString.Contains("more damage to rare"))
            return "increasedDamageVsRareAndUnique";
        if (statString.Contains("damage for each") && statString.Contains("attack") && (statString.Contains("consecutive") || statString.Contains("played consecutively")))
            return "increasedDamageWithConsecutiveAttacks";
        if (statString.Contains("every") && statString.Contains("attack") && (statString.Contains("3rd") || statString.Contains("4th") || statString.Contains("5th")))
            return "increasedDamageOnEveryNthAttack";
        if (statString.Contains("damage to marked") || statString.Contains("damage to marked enemies"))
            return "increasedDamageVsMarked";
        
        // Ailment & Status Effects
        if (statString.Contains("ailment duration") || (statString.Contains("+") && statString.Contains("ailment duration")))
            return "ailmentDurationIncreased";
        if (statString.Contains("ailment") && (statString.Contains("chance") || statString.Contains("apply")))
            return "ailmentApplicationChanceIncreased";
        if (statString.Contains("chance to apply ailments") || statString.Contains("chance to apply a random ailment") || (statString.Contains("+") && statString.Contains("chance to apply ailments")))
            return "ailmentApplicationChanceIncreased";
        if (statString.Contains("slow") && statString.Contains("chance"))
            return "slowChance";
        if (statString.Contains("freeze") && statString.Contains("chance") && !statString.Contains("duration"))
            return "freezeChance";
        if (statString.Contains("curse"))
        {
            if (statString.Contains("chance") || statString.Contains("apply"))
                return "curseApplicationChance";
            if (statString.Contains("duration") || statString.Contains("last") || statString.Contains("decay"))
                return "curseDurationIncreased";
            if (statString.Contains("effectiveness") || statString.Contains("effect"))
                return "curseEffectivenessIncreased";
        }
        if (statString.Contains("random ailment") || statString.Contains("random ailment chance") || statString.Contains("chance to apply a random ailment"))
            return "randomAilmentChance";
        if (statString.Contains("chill effectiveness") || statString.Contains("chill effect"))
            return "chillEffectivenessIncreased";
        if (statString.Contains("shock effectiveness") || statString.Contains("shock effect"))
            return "shockEffectivenessIncreased";
        
        // Damage Reduction
        if (statString.Contains("damage reduction"))
        {
            if (statString.Contains("spell") || statString.Contains("from spells") || statString.Contains("taken from spells"))
                return "damageReductionFromSpells";
            if (statString.Contains("stunned") || statString.Contains("stun") || statString.Contains("when stunned"))
                return "damageReductionWhenStunned";
            if (statString.Contains("physical") && statString.Contains("reduction"))
                return "physicalDamageReductionIncreased";
            if (statString.Contains("guarding") || statString.Contains("while guarding"))
                return "damageReductionWhileGuarding";
            return "damageReductionIncreased";
        }
        if (statString.Contains("physical reduction") || statString.Contains("increased physical reduction") || (statString.Contains("+") && statString.Contains("physical reduction")))
            return "physicalReductionIncreased";
        if (statString.Contains("less damage"))
            return "damageReductionIncreased";
        
        // Stun & Crowd Control
        if (statString.Contains("stun duration") || (statString.Contains("stun") && statString.Contains("duration")) || (statString.Contains("stun duration") && statString.Contains("enemies")))
            return "stunDurationIncreased";
        if (statString.Contains("stagger") && (statString.Contains("effectiveness") || statString.Contains("increased")))
            return "staggerEffectivenessIncreased";
        
        // Card System Stats
        if (statString.Contains("card draw") && statString.Contains("chance"))
            return "cardDrawChanceIncreased";
        if (statString.Contains("hand size") || statString.Contains("handsize") || (statString.Contains("+") && statString.Contains("hand") && statString.Contains("size")))
            return "handSizeIncreased";
        if (statString.Contains("mana cost") && (statString.Contains("efficiency") || statString.Contains("reduced") || statString.Contains("less") || statString.Contains("costs")))
            return "manaCostEfficiencyIncreased";
        if (statString.Contains("spell") && statString.Contains("mana cost") && (statString.Contains("reduced") || statString.Contains("less")))
            return "manaCostEfficiencyIncreased";
        if (statString.Contains("mana refund") || statString.Contains("refund mana") || statString.Contains("chance to refund mana") || (statString.Contains("+") && statString.Contains("chance to refund mana")))
            return "manaRefundChance";
        if (statString.Contains("card cycle") || statString.Contains("card draw speed") || statString.Contains("card cycle speed") || (statString.Contains("increased") && statString.Contains("card cycle")))
            return "cardCycleSpeedIncreased";
        if (statString.Contains("prepared card") && (statString.Contains("effectiveness") || statString.Contains("effect")) || (statString.Contains("effect of prepared")))
            return "preparedCardEffectivenessIncreased";
        if (statString.Contains("discard cost") && (statString.Contains("reduction") || statString.Contains("reduce")))
            return "discardCostReduction";
        if (statString.Contains("discard power") || statString.Contains("discard effect") || (statString.Contains("+") && statString.Contains("discard power")))
            return "discardPowerIncreased";
        if (statString.Contains("delayed card") && (statString.Contains("effectiveness") || statString.Contains("effect")) || (statString.Contains("increased") && statString.Contains("delayed card")))
            return "delayedCardEffectivenessIncreased";
        if (statString.Contains("skill card") && (statString.Contains("effectiveness") || statString.Contains("effect")) || (statString.Contains("skill effect")))
            return "skillCardEffectivenessIncreased";
        if (statString.Contains("spell power") || statString.Contains("spell effectiveness") || (statString.Contains("+") && statString.Contains("spell power")))
            return "spellPowerIncreased";
        if ((statString.Contains("echo card") && (statString.Contains("effectiveness") || statString.Contains("effect") || statString.Contains("increased"))) || (statString.Contains("echoed card") && statString.Contains("effectiveness")))
            return "echoCardEffectivenessIncreased";
        if (statString.Contains("chaos card") && (statString.Contains("effectiveness") || statString.Contains("increased")))
            return "spellPowerIncreased"; // Use spell power as placeholder
        if ((statString.Contains("projectile card") && statString.Contains("damage")) || (statString.Contains("+") && statString.Contains("projectile card") && statString.Contains("damage")))
            return "increasedProjectileDamage"; // Use projectile damage
        if ((statString.Contains("prepared") && statString.Contains("projectile") && statString.Contains("damage")) || (statString.Contains("+") && statString.Contains("prepared") && statString.Contains("projectile")))
            return "increasedProjectileDamage"; // Prepared projectile cards
        if ((statString.Contains("prepared") && statString.Contains("crit")) || (statString.Contains("+") && statString.Contains("crit") && statString.Contains("prepared")))
            return "increasedCriticalChanceWithPreparedCards";
        if ((statString.Contains("spell effect") && statString.Contains("ailment")) || (statString.Contains("increased") && statString.Contains("spell effect") && statString.Contains("ailment")))
            return "spellEffectVsAilmentIncreased";
        if ((statString.Contains("chain") && statString.Contains("chance")) || (statString.Contains("+") && statString.Contains("chain chance")))
            return "chainChance";
        if ((statString.Contains("life cost") && statString.Contains("efficiency")) || (statString.Contains("increased") && statString.Contains("life cost efficiency")))
            return "lifeCostEfficiencyIncreased";
        
        // Attributes (flat stats - these will need special handling)
        if (statString.Contains("strength"))
            return "strength"; // Flat stat
        if (statString.Contains("dexterity"))
            return "dexterity"; // Flat stat
        if (statString.Contains("intelligence"))
            return "intelligence"; // Flat stat
        
        // Default: use a generic key (will need manual mapping)
        LogWarning($"Could not map stat string to key: {statString}");
        return "unknown_stat";
    }
    
    private void LogInfo(string message) => importLog += $"[INFO] {message}\n";
    private void LogSuccess(string message) => importLog += $"[SUCCESS] {message}\n";
    private void LogWarning(string message) => importLog += $"[WARNING] {message}\n";
    private void LogError(string message) => importLog += $"[ERROR] {message}\n";
}

