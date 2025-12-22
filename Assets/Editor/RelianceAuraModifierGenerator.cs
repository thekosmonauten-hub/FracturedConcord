using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Editor tool to generate RelianceAuraModifierDefinition assets from RelianceAura TSV data
/// Parses effect descriptions and creates appropriate modifier definitions
/// </summary>
public class RelianceAuraModifierGenerator : EditorWindow
{
    private TextAsset tsvFile;
    private string tsvFilePath = "Assets/Documentation/RelianceAuras.tsv";
    private string outputFolder = "Assets/Resources/RelianceAuraModifiers";
    private bool overwriteExisting = false;
    
    [MenuItem("Tools/Dexiled/Generate Reliance Aura Modifiers from TSV")]
    public static void ShowWindow()
    {
        GetWindow<RelianceAuraModifierGenerator>("Reliance Aura Modifier Generator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Reliance Aura Modifier Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("TSV File Selection", EditorStyles.boldLabel);
        tsvFile = (TextAsset)EditorGUILayout.ObjectField("TSV File (TextAsset)", tsvFile, typeof(TextAsset), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("OR", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("TSV File Path (Alternative)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        tsvFilePath = EditorGUILayout.TextField("File Path", tsvFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select TSV File", "Assets", "tsv");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path if it's within the project
                if (path.StartsWith(Application.dataPath))
                {
                    tsvFilePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    tsvFilePath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This tool generates RelianceAuraModifierDefinition assets based on the effects in the TSV file. Each aura will get modifier definitions for its effects.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Modifiers"))
        {
            if (tsvFile == null && string.IsNullOrEmpty(tsvFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a TSV file or provide a file path.", "OK");
                return;
            }
            
            GenerateModifiers();
        }
    }
    
    private void GenerateModifiers()
    {
        string tsvContent = null;
        
        // Try to get content from TextAsset first
        if (tsvFile != null)
        {
            tsvContent = tsvFile.text;
        }
        // Otherwise, try to read from file path
        else if (!string.IsNullOrEmpty(tsvFilePath))
        {
            // Check if it's a relative path (starts with Assets/)
            if (tsvFilePath.StartsWith("Assets/"))
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, tsvFilePath.Substring(7)); // Remove "Assets/"
                if (System.IO.File.Exists(fullPath))
                {
                    tsvContent = System.IO.File.ReadAllText(fullPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", $"File not found: {fullPath}", "OK");
                    return;
                }
            }
            else if (System.IO.File.Exists(tsvFilePath))
            {
                tsvContent = System.IO.File.ReadAllText(tsvFilePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"File not found: {tsvFilePath}", "OK");
                return;
            }
        }
        
        if (string.IsNullOrEmpty(tsvContent))
        {
            EditorUtility.DisplayDialog("Error", "Could not read TSV file. Please check the file path or select a TextAsset.", "OK");
            return;
        }
        
        string[] lines = tsvContent.Split('\n');
        if (lines.Length < 2)
        {
            Debug.LogError("[RelianceAuraModifierGenerator] TSV file is empty or has no data rows!");
            return;
        }
        
        // Ensure output folder exists
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        
        int generatedCount = 0;
        int skippedCount = 0;
        int errorCount = 0;
        
        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            
            try
            {
                string[] columns = line.Split('\t');
                if (columns.Length < 6)
                    continue;
                
                string category = columns[0].Trim();
                string auraName = columns[1].Trim();
                string effectLevel1 = columns[3].Trim();
                string effectLevel20 = columns[4].Trim();
                
                // Get scaling type (column 12, index 12, if it exists)
                // Columns: 0=Category, 1=Name, 2=Desc, 3=Effect1, 4=Effect20, 5=Reliance, 6=ModIDs, 7=Emboss, 8=ReqLevel, 9=Unlock, 10=Icon, 11=Color, 12=Scaling
                string scalingType = columns.Length > 12 ? columns[12].Trim().ToLower() : "linear";
                if (string.IsNullOrEmpty(scalingType))
                    scalingType = "linear";
                
                if (string.IsNullOrEmpty(auraName))
                    continue;
                
                // Generate modifiers for this aura
                List<ModifierDefinition> modifiers = ParseEffects(auraName, effectLevel1, effectLevel20, scalingType);
                
                foreach (var modDef in modifiers)
                {
                    if (CreateModifierAsset(auraName, modDef))
                    {
                        generatedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RelianceAuraModifierGenerator] Error processing line {i + 1}: {e.Message}");
                errorCount++;
            }
        }
        
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Generation Complete", 
            $"Generated: {generatedCount}\nSkipped: {skippedCount}\nErrors: {errorCount}", 
            "OK");
        
        Debug.Log($"[RelianceAuraModifierGenerator] Generation complete. Generated: {generatedCount}, Skipped: {skippedCount}, Errors: {errorCount}");
    }
    
    private List<ModifierDefinition> ParseEffects(string auraName, string effectLevel1, string effectLevel20, string scalingType = "linear")
    {
        List<ModifierDefinition> modifiers = new List<ModifierDefinition>();
        
        // Parse both levels and merge them into single modifiers with scaling
        List<ModifierDefinition> level1Mods = new List<ModifierDefinition>();
        List<ModifierDefinition> level20Mods = new List<ModifierDefinition>();
        
        ParseEffect(auraName, effectLevel1, level1Mods, 1);
        
        // Only parse level 20 if scaling is enabled
        if (scalingType != "none")
        {
            ParseEffect(auraName, effectLevel20, level20Mods, 20);
            Debug.Log($"[RelianceAuraModifierGenerator] Parsed level 20 effects for {auraName} (scaling type: {scalingType}). Found {level20Mods.Count} level 20 modifiers.");
        }
        else
        {
            Debug.Log($"[RelianceAuraModifierGenerator] Skipping level 20 parsing for {auraName} (scaling type: {scalingType})");
        }
        
        // Merge modifiers: match by action type and merge parameters with _level20 suffix
        Dictionary<string, ModifierDefinition> mergedMods = new Dictionary<string, ModifierDefinition>();
        
        // First, add all level 1 modifiers
        foreach (var mod in level1Mods)
        {
            string key = GetModifierKey(mod);
            mergedMods[key] = mod;
            Debug.Log($"[RelianceAuraModifierGenerator] Added level 1 modifier: {key} with {mod.parameters.Count} parameters");
        }
        
        // Then, merge level 20 parameters into matching modifiers (only if scaling is enabled)
        if (scalingType != "none")
        {
            foreach (var mod20 in level20Mods)
            {
                string key = GetModifierKey(mod20);
                if (mergedMods.ContainsKey(key))
                {
                    // Merge level 20 parameters with _level20 suffix
                    var mod1 = mergedMods[key];
                    foreach (var kvp in mod20.parameters)
                    {
                        // Skip if this parameter already exists (don't overwrite)
                        if (!mod1.parameters.ContainsKey(kvp.Key))
                        {
                            // If level 1 doesn't have this parameter, set it to the level 20 value
                            // (for effects that only appear at higher levels)
                            mod1.parameters[kvp.Key] = kvp.Value;
                        }
                        else
                        {
                            // Add level 20 value with _level20 suffix for scaling
                            string level20Key = kvp.Key + "_level20";
                            mod1.parameters[level20Key] = kvp.Value;
                            Debug.Log($"[RelianceAuraModifierGenerator] Added scaling parameter: {level20Key} = {kvp.Value} (from {kvp.Key})");
                        }
                    }
                }
                else
                {
                    // New modifier only at level 20 - add it but mark parameters as level 20
                    // This handles cases where an effect only appears at level 20
                    var newMod = new ModifierDefinition
                    {
                        auraName = mod20.auraName,
                        level = 1, // Set to 1 so it's active from start
                        description = mod20.description,
                        actionType = mod20.actionType,
                        eventType = mod20.eventType,
                        parameters = new Dictionary<string, object>()
                    };
                    
                    // Copy parameters with _level20 suffix (they'll scale from 0 to this value)
                    foreach (var kvp in mod20.parameters)
                    {
                        newMod.parameters[kvp.Key + "_level20"] = kvp.Value;
                        // Also set base to 0 or a minimal value
                        if (kvp.Value is float)
                            newMod.parameters[kvp.Key] = 0f;
                        else if (kvp.Value is int)
                            newMod.parameters[kvp.Key] = 0;
                        else
                            newMod.parameters[kvp.Key] = kvp.Value;
                    }
                    
                    mergedMods[key] = newMod;
                }
            }
        }
        
        modifiers.AddRange(mergedMods.Values);
        
        // Debug: Log summary of merged modifiers
        Debug.Log($"[RelianceAuraModifierGenerator] Merged {modifiers.Count} modifier(s) for {auraName}:");
        foreach (var mod in modifiers)
        {
            int level20Count = 0;
            foreach (var key in mod.parameters.Keys)
            {
                if (key.EndsWith("_level20"))
                    level20Count++;
            }
            Debug.Log($"  - {mod.actionType}: {mod.parameters.Count} total parameters ({level20Count} with _level20 suffix)");
        }
        
        return modifiers;
    }
    
    private string GetModifierKey(ModifierDefinition mod)
    {
        // Create a unique key based on action type and primary parameter
        string key = mod.actionType.ToString();
        if (mod.parameters.ContainsKey("damageType"))
            key += "_" + mod.parameters["damageType"].ToString();
        else if (mod.parameters.ContainsKey("statName"))
            key += "_" + mod.parameters["statName"].ToString();
        else if (mod.parameters.ContainsKey("statusType"))
            key += "_" + mod.parameters["statusType"].ToString();
        return key;
    }
    
    private void ParseEffect(string auraName, string effectText, List<ModifierDefinition> modifiers, int level)
    {
        if (string.IsNullOrEmpty(effectText))
            return;
        
        // Split by comma, semicolon, or period to handle multiple effects
        // But be careful with decimals - only split on periods that are followed by space or end of string
        string[] effects = effectText.Split(new char[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Also split by periods, but only if they're followed by a space (not decimals)
        List<string> allEffects = new List<string>();
        foreach (string effect in effects)
        {
            // Split by period followed by space or end of string
            string[] periodSplit = Regex.Split(effect, @"\.\s+");
            foreach (string part in periodSplit)
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    allEffects.Add(trimmed);
                }
            }
        }
        effects = allEffects.ToArray();
        
        foreach (string effect in effects)
        {
            string trimmed = effect.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;
            
            ModifierDefinition modDef = ParseSingleEffect(auraName, trimmed, level);
            if (modDef != null)
            {
                modifiers.Add(modDef);
            }
        }
    }
    
    private ModifierDefinition ParseSingleEffect(string auraName, string effectText, int level)
    {
        ModifierDefinition modDef = new ModifierDefinition
        {
            auraName = auraName,
            level = level,
            description = effectText
        };
        
        // Parse different effect types
        effectText = effectText.ToLower();
        
        // Flat damage additions (may include chance to ignite/shock/etc)
        if (Regex.IsMatch(effectText, @"\d+\s*(flat\s*)?(fire|cold|lightning|physical|chaos)\s*damage"))
        {
            modDef.actionType = ModifierActionType.AddFlatDamage;
            modDef.eventType = ModifierEventType.OnCombatStart; // Auras are persistent
            
            // Extract damage value and type
            Match damageMatch = Regex.Match(effectText, @"(\d+)\s*(flat\s*)?(fire|cold|lightning|physical|chaos)");
            if (damageMatch.Success)
            {
                modDef.parameters["damage"] = float.Parse(damageMatch.Groups[1].Value);
                modDef.parameters["damageType"] = damageMatch.Groups[3].Value;
            }
            
            // Check for chance to apply status (e.g., "15% chance to ignite")
            Match chanceMatch = Regex.Match(effectText, @"(\d+)%\s*chance\s*to\s*(ignite|shock|chill|poison|freeze)", RegexOptions.IgnoreCase);
            if (chanceMatch.Success)
            {
                float chance = float.Parse(chanceMatch.Groups[1].Value) / 100f;
                modDef.parameters["statusChance"] = chance;
                modDef.parameters["statusType"] = chanceMatch.Groups[2].Value;
            }
        }
        // Percentage damage increases (including "Gain +X%" and "+X% more" patterns)
        else if (Regex.IsMatch(effectText, @"(gain\s+\+|\+)?\d+%\s*(increased|more)\s*(fire|cold|lightning|physical|spell|elemental)\s*damage", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"gain\s+\+(\d+)%\s*(fire|cold|lightning|physical|spell|elemental)\s*damage", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"\+(\d+)%\s*more\s*(fire|cold|lightning|physical|spell|elemental)\s*damage", RegexOptions.IgnoreCase))
        {
            Match percentMatch = Regex.Match(effectText, @"(?:gain\s+\+|\+)?(\d+)%\s*(increased|more)?\s*(fire|cold|lightning|physical|spell|elemental)", RegexOptions.IgnoreCase);
            if (percentMatch.Success)
            {
                float percent = float.Parse(percentMatch.Groups[1].Value);
                string modifierType = percentMatch.Groups[2].Success ? percentMatch.Groups[2].Value : "more"; // Default to "more" if not specified
                string damageType = percentMatch.Groups[3].Value;
                
                bool isMore = modifierType == "more" || string.IsNullOrEmpty(modifierType);
                modDef.actionType = isMore ? ModifierActionType.AddDamageMorePercent : ModifierActionType.AddPercentDamage;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["percent"] = percent;
                modDef.parameters["damageType"] = damageType;
            }
        }
        // Extra damage as conversion
        else if (Regex.IsMatch(effectText, @"\d+%\s*of\s*(physical|fire|cold|lightning)\s*damage\s*as\s*extra\s*(fire|cold|lightning|chaos)"))
        {
            Match conversionMatch = Regex.Match(effectText, @"(\d+)%\s*of\s*(physical|fire|cold|lightning)\s*damage\s*as\s*extra\s*(fire|cold|lightning|chaos)");
            if (conversionMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddElementalDamage;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["percent"] = float.Parse(conversionMatch.Groups[1].Value);
                modDef.parameters["sourceType"] = conversionMatch.Groups[2].Value;
                modDef.parameters["targetType"] = conversionMatch.Groups[3].Value;
            }
        }
        // Status effect magnitude increases (handles "More" and "Increased")
        else if (Regex.IsMatch(effectText, @"\d+%\s*(more|increased)\s*(ignite|chill|shock|poison|ailment)\s*magnitude", RegexOptions.IgnoreCase))
        {
            Match statusMatch = Regex.Match(effectText, @"(\d+)%\s*(more|increased)\s*(ignite|chill|shock|poison|ailment)\s*magnitude", RegexOptions.IgnoreCase);
            if (statusMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = statusMatch.Groups[3].Value + "Magnitude";
                modDef.parameters["percent"] = float.Parse(statusMatch.Groups[1].Value);
                modDef.parameters["modifierType"] = statusMatch.Groups[2].Value.ToLower(); // "more" or "increased"
            }
        }
        // Status effect chance (including "always" or "100% chance")
        else if (Regex.IsMatch(effectText, @"(always|100%\s*chance|\d+%\s*chance)\s*to\s*(shock|chill|ignite|poison|freeze)", RegexOptions.IgnoreCase))
        {
            Match chanceMatch = Regex.Match(effectText, @"(always|100%|(\d+)%)\s*chance\s*to\s*(shock|chill|ignite|poison|freeze)", RegexOptions.IgnoreCase);
            if (chanceMatch.Success)
            {
                modDef.actionType = ModifierActionType.ApplyStatus;
                modDef.eventType = ModifierEventType.OnDamageDealt;
                
                // Parse chance value
                float chance = 1.0f; // Default to 100% for "always"
                if (chanceMatch.Groups[1].Value.ToLower() == "always" || chanceMatch.Groups[1].Value == "100%")
                {
                    chance = 1.0f;
                }
                else if (chanceMatch.Groups[2].Success && !string.IsNullOrEmpty(chanceMatch.Groups[2].Value))
                {
                    chance = float.Parse(chanceMatch.Groups[2].Value) / 100f;
                }
                
                modDef.parameters["chance"] = chance;
                modDef.parameters["statusEffectType"] = chanceMatch.Groups[3].Value;
            }
        }
        // Speed increases
        else if (Regex.IsMatch(effectText, @"\d+%\s*increased\s*(attack|cast|movement)\s*speed"))
        {
            Match speedMatch = Regex.Match(effectText, @"(\d+)%\s*increased\s*(attack|cast|movement)\s*speed");
            if (speedMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = speedMatch.Groups[2].Value + "Speed";
                modDef.parameters["percent"] = float.Parse(speedMatch.Groups[1].Value);
            }
        }
        // Critical strike
        else if (Regex.IsMatch(effectText, @"\d+%\s*(increased\s*)?(critical\s*strike\s*chance|critical\s*strike\s*multiplier)"))
        {
            Match critMatch = Regex.Match(effectText, @"(\d+)%\s*(increased\s*)?(critical\s*strike\s*chance|critical\s*strike\s*multiplier)");
            if (critMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                string statType = critMatch.Groups[3].Value.Replace(" ", "");
                modDef.parameters["statName"] = statType;
                modDef.parameters["percent"] = float.Parse(critMatch.Groups[1].Value);
            }
        }
        // Defense stats (armour, evasion, energy shield)
        else if (Regex.IsMatch(effectText, @"\d+\s*(armour|armor|evasion|energy\s*shield)"))
        {
            Match defenseMatch = Regex.Match(effectText, @"(\d+)\s*(armour|armor|evasion|energy\s*shield)");
            if (defenseMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                string statName = defenseMatch.Groups[2].Value.Replace(" ", "");
                if (statName == "armor") statName = "armour";
                modDef.parameters["statName"] = statName;
                modDef.parameters["flat"] = float.Parse(defenseMatch.Groups[1].Value);
            }
        }
        // Resistances
        else if (Regex.IsMatch(effectText, @"\d+%\s*(all\s*)?resistance"))
        {
            Match resistMatch = Regex.Match(effectText, @"(\d+)%\s*(all\s*)?resistance");
            if (resistMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = "allResistance";
                modDef.parameters["percent"] = float.Parse(resistMatch.Groups[1].Value);
            }
        }
        // Mana regeneration
        else if (Regex.IsMatch(effectText, @"\d+\s*mana\s*regeneration"))
        {
            Match manaMatch = Regex.Match(effectText, @"(\d+)\s*mana\s*regeneration");
            if (manaMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = "manaRecoveryPerTurn";
                modDef.parameters["flat"] = float.Parse(manaMatch.Groups[1].Value);
            }
        }
        // Life regeneration
        else if (Regex.IsMatch(effectText, @"\d+\s*life\s*regeneration"))
        {
            Match lifeMatch = Regex.Match(effectText, @"(\d+)\s*life\s*regeneration");
            if (lifeMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = "lifeRegeneration";
                modDef.parameters["flat"] = float.Parse(lifeMatch.Groups[1].Value);
            }
        }
        // Card draw
        else if (Regex.IsMatch(effectText, @"\d+\s*card\s*draw"))
        {
            Match drawMatch = Regex.Match(effectText, @"(\d+)\s*card\s*draw");
            if (drawMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = "cardsDrawnPerTurn";
                modDef.parameters["flat"] = float.Parse(drawMatch.Groups[1].Value);
            }
        }
        // Preparation charges
        else if (Regex.IsMatch(effectText, @"\d+\s*preparation\s*charge"))
        {
            Match prepMatch = Regex.Match(effectText, @"(\d+)\s*preparation\s*charge");
            if (prepMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = "preparationChargeCap";
                modDef.parameters["flat"] = float.Parse(prepMatch.Groups[1].Value);
            }
        }
        // Discard power
        else if (Regex.IsMatch(effectText, @"\d+\s*discard\s*power"))
        {
            Match discardMatch = Regex.Match(effectText, @"(\d+)\s*discard\s*power");
            if (discardMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = "discardPower";
                modDef.parameters["flat"] = float.Parse(discardMatch.Groups[1].Value);
            }
        }
        // COMPLEX EFFECTS - Use new Reliance Aura action types
        
        // Spread Status on Kill (Emberwake Echo, Law of Ember)
        else if (Regex.IsMatch(effectText, @"spread\s+(ignite|shock|chill|poison|burn)\s+to\s+(\d+)\s+random\s+enemy", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"enemies\s+you\s+(ignite|kill|shock).*spread\s+(ignite|shock)", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.SpreadStatusOnKill;
            modDef.eventType = ModifierEventType.OnEnemyKilled;
            
            // Extract status type
            Match statusMatch = Regex.Match(effectText, @"spread\s+(ignite|shock|chill|poison|burn)", RegexOptions.IgnoreCase);
            if (!statusMatch.Success)
                statusMatch = Regex.Match(effectText, @"(ignite|shock|chill|poison|burn)", RegexOptions.IgnoreCase);
            
            string statusType = statusMatch.Success ? statusMatch.Groups[1].Value : "Ignite";
            modDef.parameters["statusType"] = statusType;
            
            // Extract spread count
            Match countMatch = Regex.Match(effectText, @"(\d+)\s+random\s+enemy", RegexOptions.IgnoreCase);
            int spreadCount = countMatch.Success ? int.Parse(countMatch.Groups[1].Value) : 1;
            modDef.parameters["spreadCount"] = spreadCount;
        }
        
        // Shatter on Kill (Law of Permafrost)
        else if (Regex.IsMatch(effectText, @"shatter.*deal\s+damage.*(\d+)%\s*of\s*max\s*life", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"killing\s+a\s+(chilled|frozen)\s+enemy.*shatter", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.ShatterOnKill;
            modDef.eventType = ModifierEventType.OnEnemyKilled;
            
            // Extract damage percent
            Match damageMatch = Regex.Match(effectText, @"(\d+)%\s*of\s*max\s*life", RegexOptions.IgnoreCase);
            float damagePercent = damageMatch.Success ? float.Parse(damageMatch.Groups[1].Value) / 100f : 0.03f;
            modDef.parameters["damagePercent"] = damagePercent;
        }
        
        // Cast Spell on Kill (Law of Tempest) - handles strike count
        else if (Regex.IsMatch(effectText, @"killing\s+a\s+shocked\s+enemy.*(lightning|cast)", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"lightning\s+bolt.*(\d+)\s+lightning\s+damage", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.CastSpellOnKill;
            modDef.eventType = ModifierEventType.OnEnemyKilled;
            
            // Extract damage
            Match damageMatch = Regex.Match(effectText, @"(\d+)\s+lightning\s+damage", RegexOptions.IgnoreCase);
            float damage = damageMatch.Success ? float.Parse(damageMatch.Groups[1].Value) : 80f;
            modDef.parameters["damage"] = damage;
            modDef.parameters["spellName"] = "Lightning Bolt";
            
            // Extract strike count (e.g., "strikes once" or "strikes 5 times")
            Match strikeMatch = Regex.Match(effectText, @"strikes\s+(\d+)\s+times?", RegexOptions.IgnoreCase);
            if (strikeMatch.Success)
            {
                modDef.parameters["strikeCount"] = int.Parse(strikeMatch.Groups[1].Value);
            }
            else if (Regex.IsMatch(effectText, @"strikes\s+once", RegexOptions.IgnoreCase))
            {
                modDef.parameters["strikeCount"] = 1;
            }
        }
        
        // Apply Crumble (Law of Force - part 1) - now with chance
        else if (Regex.IsMatch(effectText, @"(chance\s+to\s+apply\s+crumble|apply\s+crumble)", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"enemies\s+you\s+hit\s+gain\s+crumble", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.ApplyCrumble;
            modDef.eventType = ModifierEventType.OnDamageDealtToEnemy;
            
            // Extract chance if present
            Match chanceMatch = Regex.Match(effectText, @"(\d+)%\s*chance\s*to\s*apply\s+crumble", RegexOptions.IgnoreCase);
            if (chanceMatch.Success)
            {
                float chance = float.Parse(chanceMatch.Groups[1].Value) / 100f;
                modDef.parameters["chance"] = chance;
            }
            else
            {
                // Default to 100% if not specified
                modDef.parameters["chance"] = 1.0f;
            }
        }
        
        // Deal Crumble Damage (Law of Force - part 2)
        else if (Regex.IsMatch(effectText, @"crumble.*deal.*(\d+)%\s*of\s*stored", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"hitting\s+an\s+enemy\s+affected\s+by\s+crumble", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.DealCrumbleDamage;
            modDef.eventType = ModifierEventType.OnDamageDealtToEnemy;
            
            // Extract damage percent
            Match damageMatch = Regex.Match(effectText, @"(\d+)%\s*of\s*(stored|crumble)", RegexOptions.IgnoreCase);
            float damagePercent = damageMatch.Success ? float.Parse(damageMatch.Groups[1].Value) / 100f : 0.10f;
            modDef.parameters["damagePercent"] = damagePercent;
        }
        
        // Add Poison Stacks (Law of Pale Venin) - with chance
        else if (Regex.IsMatch(effectText, @"\+(\d+)\s+poison\s+stacks", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"additional\s+stack.*poison", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"chance\s+to\s+poison.*\+(\d+)\s+poison", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.AddPoisonStacks;
            modDef.eventType = ModifierEventType.OnStatusApplied;
            
            // Extract stack count
            Match stackMatch = Regex.Match(effectText, @"\+(\d+)\s+poison\s+stacks", RegexOptions.IgnoreCase);
            int stacks = stackMatch.Success ? int.Parse(stackMatch.Groups[1].Value) : 1;
            modDef.parameters["additionalStacks"] = stacks;
            
            // Extract chance if present
            Match chanceMatch = Regex.Match(effectText, @"(\d+)%\s*chance\s*to\s*poison", RegexOptions.IgnoreCase);
            if (chanceMatch.Success)
            {
                float chance = float.Parse(chanceMatch.Groups[1].Value) / 100f;
                modDef.parameters["chance"] = chance;
            }
        }
        
        // Roll Damage Per Turn (Tempest Flux)
        else if (Regex.IsMatch(effectText, @"rolled\s+each\s+turn", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"new\s+value\s+each\s+turn", RegexOptions.IgnoreCase) ||
                 (Regex.IsMatch(effectText, @"(\d+)-(\d+)", RegexOptions.IgnoreCase) && 
                  Regex.IsMatch(effectText, @"(fire|cold|lightning|physical|chaos)\s+damage.*turn", RegexOptions.IgnoreCase)))
        {
            modDef.actionType = ModifierActionType.RollDamagePerTurn;
            modDef.eventType = ModifierEventType.OnTurnStart;
            
            // Extract damage range
            Match rangeMatch = Regex.Match(effectText, @"(\d+)-(\d+)\s+flat\s+(fire|cold|lightning|physical|chaos)", RegexOptions.IgnoreCase);
            if (rangeMatch.Success)
            {
                modDef.parameters["minDamage"] = float.Parse(rangeMatch.Groups[1].Value);
                modDef.parameters["maxDamage"] = float.Parse(rangeMatch.Groups[2].Value);
                modDef.parameters["damageType"] = rangeMatch.Groups[3].Value;
            }
            else
            {
                // Fallback: try to extract from "1-140 flat Lightning damage"
                rangeMatch = Regex.Match(effectText, @"(\d+)-(\d+).*?(fire|cold|lightning|physical|chaos)", RegexOptions.IgnoreCase);
                if (rangeMatch.Success)
                {
                    modDef.parameters["minDamage"] = float.Parse(rangeMatch.Groups[1].Value);
                    modDef.parameters["maxDamage"] = float.Parse(rangeMatch.Groups[2].Value);
                    modDef.parameters["damageType"] = rangeMatch.Groups[3].Value;
                }
            }
        }
        
        // Stack Damage Per Turn (Iron Ascent)
        else if (Regex.IsMatch(effectText, @"(\d+)%\s+more\s+(physical|fire|cold|lightning)\s+damage\s+per\s+turn", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"damage\s+to\s+increase\s+over", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"(\d+)%\s+more.*per\s+turn.*(\d+)\s+turns", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.StackDamagePerTurn;
            modDef.eventType = ModifierEventType.OnTurnStart;
            
            // Extract percent per turn
            Match percentMatch = Regex.Match(effectText, @"(\d+)%\s+more\s+(physical|fire|cold|lightning)", RegexOptions.IgnoreCase);
            if (percentMatch.Success)
            {
                modDef.parameters["percentPerTurn"] = float.Parse(percentMatch.Groups[1].Value);
                modDef.parameters["damageType"] = percentMatch.Groups[2].Value;
            }
            
            // Extract max turns
            Match turnMatch = Regex.Match(effectText, @"(\d+)\s+turns", RegexOptions.IgnoreCase);
            int maxTurns = turnMatch.Success ? int.Parse(turnMatch.Groups[1].Value) : 10;
            modDef.parameters["maxTurns"] = maxTurns;
        }
        
        // Modify Ailment Duration and Effect (Woundweaver Rite)
        else if (Regex.IsMatch(effectText, @"increased\s+ailment\s+duration.*reduced.*effect", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"(\d+)%\s+increased\s+ailment\s+duration.*(\d+)%\s+reduced", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.ModifyAilmentDurationAndEffect;
            modDef.eventType = ModifierEventType.OnCombatStart;
            
            // Extract duration multiplier (100% increased = 2x)
            Match durationMatch = Regex.Match(effectText, @"(\d+)%\s+increased\s+ailment\s+duration", RegexOptions.IgnoreCase);
            float durationPercent = durationMatch.Success ? float.Parse(durationMatch.Groups[1].Value) : 100f;
            modDef.parameters["durationMultiplier"] = 1f + (durationPercent / 100f);
            
            // Extract effect reduction (50% reduced = 0.5x)
            Match effectMatch = Regex.Match(effectText, @"(\d+)%\s+reduced\s+ailment\s+effect", RegexOptions.IgnoreCase);
            float effectPercent = effectMatch.Success ? float.Parse(effectMatch.Groups[1].Value) : 50f;
            modDef.parameters["effectReduction"] = 1f - (effectPercent / 100f);
        }
        
        // Scale Discard Power (Echo of the Unburdened)
        else if (Regex.IsMatch(effectText, @"discard\s+power.*discarded", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"(\d+)\s+discard\s+power\s+per\s+discarded", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.ScaleDiscardPower;
            modDef.eventType = ModifierEventType.OnCombatStart; // Will be updated on discard
            
            // Extract power per discard
            Match powerMatch = Regex.Match(effectText, @"(\d+)\s+discard\s+power\s+per", RegexOptions.IgnoreCase);
            float powerPerDiscard = powerMatch.Success ? float.Parse(powerMatch.Groups[1].Value) : 1f;
            modDef.parameters["powerPerDiscard"] = powerPerDiscard;
        }
        // Percentage modifiers with "more" or "reduced"
        else if (Regex.IsMatch(effectText, @"\d+%\s*(more|reduced|increased)\s*"))
        {
            Match moreMatch = Regex.Match(effectText, @"(\d+)%\s*(more|reduced|increased)\s*(.+)");
            if (moreMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["percent"] = float.Parse(moreMatch.Groups[1].Value);
                modDef.parameters["modifierType"] = moreMatch.Groups[2].Value;
                modDef.parameters["statName"] = moreMatch.Groups[3].Value.Trim();
            }
        }
        
        // If no pattern matched, create a generic modifier
        if (modDef.actionType == ModifierActionType.AddStack && modDef.parameters.Count == 0)
        {
            modDef.actionType = ModifierActionType.ModifyStat;
            modDef.eventType = ModifierEventType.OnCombatStart;
            modDef.parameters["customEffect"] = effectText;
            modDef.needsManualConfig = true;
        }
        
        return modDef;
    }
    
    private bool CreateModifierAsset(string auraName, ModifierDefinition modDef)
    {
        // Create modifier ID (without level suffix since we merge level 1 and 20 into one asset)
        string modifierId = SanitizeFileName(auraName) + "_" + modDef.actionType.ToString();
        if (modDef.parameters.ContainsKey("damageType"))
        {
            modifierId += "_" + modDef.parameters["damageType"].ToString();
        }
        else if (modDef.parameters.ContainsKey("statName"))
        {
            modifierId += "_" + modDef.parameters["statName"].ToString();
        }
        else if (modDef.parameters.ContainsKey("statusType"))
        {
            modifierId += "_" + modDef.parameters["statusType"].ToString();
        }
        
        string assetName = modifierId;
        string assetPath = Path.Combine(outputFolder, $"{assetName}.asset");
        
        // Check if asset already exists
        if (File.Exists(assetPath) && !overwriteExisting)
        {
            Debug.Log($"[RelianceAuraModifierGenerator] Skipping {modifierId} - asset already exists");
            return false;
        }
        
        // Create or load existing asset
        RelianceAuraModifierDefinition modifier = AssetDatabase.LoadAssetAtPath<RelianceAuraModifierDefinition>(assetPath);
        if (modifier == null)
        {
            modifier = ScriptableObject.CreateInstance<RelianceAuraModifierDefinition>();
        }
        
        // Populate modifier data
        modifier.modifierId = modifierId;
        modifier.linkedAuraName = auraName;
        modifier.description = modDef.description;
        modifier.minLevel = 1; // Always starts at level 1
        modifier.maxLevel = 0; // 0 = no max (scales to level 20 via _level20 parameters)
        
        // Log if this modifier has scaling parameters
        bool hasScaling = false;
        foreach (var kvp in modDef.parameters)
        {
            if (kvp.Key.EndsWith("_level20"))
            {
                hasScaling = true;
                break;
            }
        }
        if (hasScaling)
        {
            Debug.Log($"[RelianceAuraModifierGenerator] Created scaling modifier: {modifierId} (has _level20 parameters)");
        }
        
        // Create ModifierEffect
        ModifierEffect effect = new ModifierEffect();
        effect.eventType = modDef.eventType;
        effect.priority = 0;
        
        // Create ModifierAction
        ModifierAction action = new ModifierAction();
        action.actionType = modDef.actionType;
        action.executionOrder = 0;
        
        // Add parameters to action (including _level20 parameters for scaling)
        Debug.Log($"[RelianceAuraModifierGenerator] Adding {modDef.parameters.Count} parameters to modifier {modifierId}:");
        foreach (var kvp in modDef.parameters)
        {
            Debug.Log($"  - {kvp.Key} = {kvp.Value} (type: {kvp.Value.GetType().Name})");
            var entry = new SerializableParameterDictionary.ParameterEntry
            {
                key = kvp.Key,
                value = new SerializableParameterDictionary.ParameterValue()
            };
            entry.value.SetValue(kvp.Value);
            action.parameterDict.entries.Add(entry);
        }
        
        effect.actions = new List<ModifierAction> { action };
        modifier.effects = new List<ModifierEffect> { effect };
        
        // Add tag if needs manual config
        if (modDef.needsManualConfig)
        {
            if (modifier.tags == null)
                modifier.tags = new List<string>();
            if (!modifier.tags.Contains("NeedsManualConfig"))
                modifier.tags.Add("NeedsManualConfig");
        }
        
        // Save asset
        if (File.Exists(assetPath))
        {
            EditorUtility.SetDirty(modifier);
        }
        else
        {
            AssetDatabase.CreateAsset(modifier, assetPath);
        }
        
        Debug.Log($"[RelianceAuraModifierGenerator] Created/Updated: {assetPath}");
        return true;
    }
    
    private string SanitizeFileName(string fileName)
    {
        return Regex.Replace(fileName, @"[<>:""/\\|?*]", "").Trim();
    }
    
    private class ModifierDefinition
    {
        public string auraName;
        public int level;
        public string description;
        public ModifierEventType eventType = ModifierEventType.OnCombatStart;
        public ModifierActionType actionType = ModifierActionType.AddStack; // Default placeholder
        public Dictionary<string, object> parameters = new Dictionary<string, object>();
        public bool needsManualConfig = false;
    }
}

