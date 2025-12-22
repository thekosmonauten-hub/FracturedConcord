using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// Editor tool to generate EmbossingModifierDefinition assets from "Embossing revisit.md" TSV data
/// Parses effect descriptions and creates appropriate modifier definitions
/// </summary>
public class EmbossingModifierGenerator : EditorWindow
{
    private TextAsset tsvFile;
    private string tsvFilePath = "Assets/Documentation/Embossing revisit.md";
    private string outputFolder = "Assets/Resources/EmbossingModifiers";
    private bool overwriteExisting = false;
    
    [MenuItem("Tools/Dexiled/Generate Embossing Modifiers from TSV")]
    public static void ShowWindow()
    {
        GetWindow<EmbossingModifierGenerator>("Embossing Modifier Generator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Embossing Modifier Generator", EditorStyles.boldLabel);
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
            string path = EditorUtility.OpenFilePanel("Select TSV/MD File", "Assets", "md,tsv");
            if (!string.IsNullOrEmpty(path))
            {
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
        EditorGUILayout.HelpBox("This tool generates EmbossingModifierDefinition assets based on the effects in the TSV file. Each embossing will get modifier definitions for its effects.", MessageType.Info);
        
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
            if (tsvFilePath.StartsWith("Assets/"))
            {
                string fullPath = System.IO.Path.Combine(Application.dataPath, tsvFilePath.Substring(7));
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
            Debug.LogError("[EmbossingModifierGenerator] TSV file is empty or has no data rows!");
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
                if (columns.Length < 4)
                    continue;
                
                string embossingName = columns[0].Trim();
                string levelStr = columns[1].Trim();
                string tagsStr = columns[2].Trim();
                string effect = columns[3].Trim();
                
                if (string.IsNullOrEmpty(embossingName))
                    continue;
                
                // Parse level (minimum level requirement)
                int minLevel = 1;
                if (!string.IsNullOrEmpty(levelStr) && int.TryParse(levelStr, out int parsedLevel))
                {
                    minLevel = parsedLevel;
                }
                
                // Parse tags (comma-separated)
                List<string> requiredTags = new List<string>();
                if (!string.IsNullOrEmpty(tagsStr))
                {
                    string[] tags = tagsStr.Split(',');
                    foreach (string tag in tags)
                    {
                        string trimmedTag = tag.Trim();
                        if (!string.IsNullOrEmpty(trimmedTag))
                        {
                            requiredTags.Add(trimmedTag);
                        }
                    }
                }
                
                // Generate embossing ID from name
                string embossingId = GenerateEmbossingId(embossingName);
                
                // Parse effects and create modifiers
                List<ModifierDefinition> modifiers = ParseEffect(embossingName, embossingId, effect, minLevel, requiredTags);
                
                foreach (var modDef in modifiers)
                {
                    if (CreateModifierAsset(embossingName, embossingId, modDef))
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
                Debug.LogError($"[EmbossingModifierGenerator] Error processing line {i + 1}: {e.Message}\n{e.StackTrace}");
                errorCount++;
            }
        }
        
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Generation Complete", 
            $"Generated: {generatedCount}\nSkipped: {skippedCount}\nErrors: {errorCount}", 
            "OK");
        
        Debug.Log($"[EmbossingModifierGenerator] Generation complete. Generated: {generatedCount}, Skipped: {skippedCount}, Errors: {errorCount}");
    }
    
    private string GenerateEmbossingId(string embossingName)
    {
        // Remove "of " prefix if present
        string id = embossingName.Replace("of ", "").Replace("Of ", "");
        // Convert to lowercase and replace spaces with underscores
        id = id.ToLower().Replace(" ", "_");
        // Remove special characters
        id = Regex.Replace(id, @"[^a-z0-9_]", "");
        return id;
    }
    
    /// <summary>
    /// Safely parse a float value from a string, returning 0 if parsing fails
    /// </summary>
    private float SafeParseFloat(string value, float defaultValue = 0f)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;
        
        if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        
        Debug.LogWarning($"[EmbossingModifierGenerator] Failed to parse float: '{value}', using default: {defaultValue}");
        return defaultValue;
    }
    
    private List<ModifierDefinition> ParseEffect(string embossingName, string embossingId, string effectText, int minLevel, List<string> requiredTags)
    {
        List<ModifierDefinition> modifiers = new List<ModifierDefinition>();
        
        if (string.IsNullOrEmpty(effectText))
            return modifiers;
        
        // Split by periods, commas, or semicolons to handle multiple effects
        string[] effects = Regex.Split(effectText, @"[.,;]+\s*");
        
        foreach (string effect in effects)
        {
            string trimmed = effect.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;
            
            ModifierDefinition modDef = ParseSingleEffect(embossingName, embossingId, trimmed, minLevel, requiredTags);
            if (modDef != null)
            {
                modifiers.Add(modDef);
            }
        }
        
        return modifiers;
    }
    
    private ModifierDefinition ParseSingleEffect(string embossingName, string embossingId, string effectText, int minLevel, List<string> requiredTags)
    {
        ModifierDefinition modDef = new ModifierDefinition
        {
            embossingName = embossingName,
            embossingId = embossingId,
            description = effectText,
            minLevel = minLevel,
            requiredTags = requiredTags
        };
        
        string effectLower = effectText.ToLower();
        
        // Status effect chances (Bleed, Poison, Ignite, Shock, Chill, Freeze, Blind, Vulnerability)
        // Check for "cannot apply" first and skip if found
        if (!Regex.IsMatch(effectLower, @"cannot\s+apply", RegexOptions.IgnoreCase))
        {
            if (Regex.IsMatch(effectLower, @"\d+%\s*chance\s*to\s*apply\s*(bleed|poison|ignite|shock|chill|freeze|blind|vulnerability)", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(effectLower, @"(?:attacks?|cards?|embossed\s+cards?)\s+apply\s*(bleed|poison|ignite|shock|chill|freeze|blind|vulnerability)", RegexOptions.IgnoreCase))
            {
                Match statusMatch = Regex.Match(effectLower, @"(\d+)%\s*chance\s*to\s*apply\s*(bleed|poison|ignite|shock|chill|freeze|blind|vulnerability)", RegexOptions.IgnoreCase);
                bool hasChance = statusMatch.Success;
                
                if (!hasChance)
                {
                    statusMatch = Regex.Match(effectLower, @"(?:attacks?|cards?|embossed\s+cards?)\s+apply\s*(bleed|poison|ignite|shock|chill|freeze|blind|vulnerability)", RegexOptions.IgnoreCase);
                }
                
                if (statusMatch.Success)
                {
                    modDef.actionType = ModifierActionType.ApplyStatusToEnemy;
                    modDef.eventType = ModifierEventType.OnDamageDealt;
                    
                    float chance = 1.0f; // Default to 100% if not specified
                    if (hasChance && statusMatch.Groups[1].Success && !string.IsNullOrEmpty(statusMatch.Groups[1].Value))
                    {
                        // Try to parse the chance value
                        if (float.TryParse(statusMatch.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsedChance))
                        {
                            chance = parsedChance / 100f;
                        }
                    }
                    
                    // Get status type - if hasChance, it's in Groups[2], otherwise Groups[1]
                    string statusType = hasChance && statusMatch.Groups.Count > 2 ? statusMatch.Groups[2].Value : statusMatch.Groups[1].Value;
                    modDef.parameters["chance"] = chance;
                    modDef.parameters["statusType"] = statusType;
                    return modDef; // Return early since we matched
                }
            }
        }
        // Flat damage additions
        else if (Regex.IsMatch(effectLower, @"adds?\s+\d+\s*(fire|cold|lightning|physical|chaos)\s*damage", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectLower, @"\d+\s*(fire|cold|lightning|physical|chaos)\s*damage", RegexOptions.IgnoreCase))
        {
            Match damageMatch = Regex.Match(effectLower, @"(?:adds?\s+)?(\d+)\s*(fire|cold|lightning|physical|chaos)\s*damage", RegexOptions.IgnoreCase);
            if (damageMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddFlatDamage;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["damage"] = SafeParseFloat(damageMatch.Groups[1].Value);
                modDef.parameters["damageType"] = damageMatch.Groups[2].Value;
            }
        }
        // Rolled damage (e.g., "1-14 Lightning damage, rolled each turn")
        else if (Regex.IsMatch(effectLower, @"\d+-\d+\s*(fire|cold|lightning|physical|chaos)\s*damage.*rolled", RegexOptions.IgnoreCase))
        {
            Match rangeMatch = Regex.Match(effectLower, @"(\d+)-(\d+)\s*(fire|cold|lightning|physical|chaos)\s*damage", RegexOptions.IgnoreCase);
            if (rangeMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddFlatDamage; // Will be rolled at runtime
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["minDamage"] = SafeParseFloat(rangeMatch.Groups[1].Value);
                modDef.parameters["maxDamage"] = SafeParseFloat(rangeMatch.Groups[2].Value);
                modDef.parameters["damageType"] = rangeMatch.Groups[3].Value;
                modDef.parameters["rolled"] = true;
            }
        }
        // Damage multipliers (more/increased)
        else if (Regex.IsMatch(effectLower, @"\d+%\s*(more|increased)\s*(fire|cold|lightning|physical|spell|elemental|melee|projectile|area|chaos)\s*damage", RegexOptions.IgnoreCase))
        {
            Match percentMatch = Regex.Match(effectLower, @"(\d+)%\s*(more|increased)\s*(fire|cold|lightning|physical|spell|elemental|melee|projectile|area|chaos)\s*damage", RegexOptions.IgnoreCase);
            if (percentMatch.Success)
            {
                float percent = SafeParseFloat(percentMatch.Groups[1].Value);
                string modifierType = percentMatch.Groups[2].Value;
                string damageType = percentMatch.Groups[3].Value;
                
                bool isMore = modifierType == "more";
                modDef.actionType = isMore ? ModifierActionType.AddDamageMorePercent : ModifierActionType.AddPercentDamage;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["percent"] = percent;
                modDef.parameters["damageType"] = damageType;
            }
        }
        // Extra damage conversions (e.g., "30% of Physical damage as extra Fire damage")
        else if (Regex.IsMatch(effectLower, @"\d+%\s*of\s*(physical|fire|cold|lightning)\s*damage\s*(as\s*extra|added\s*as\s*extra)\s*(fire|cold|lightning|chaos)", RegexOptions.IgnoreCase))
        {
            Match conversionMatch = Regex.Match(effectLower, @"(\d+)%\s*of\s*(physical|fire|cold|lightning)\s*damage\s*(?:as\s*extra|added\s*as\s*extra)\s*(fire|cold|lightning|chaos)", RegexOptions.IgnoreCase);
            if (conversionMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddElementalDamage;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["percent"] = SafeParseFloat(conversionMatch.Groups[1].Value);
                modDef.parameters["sourceType"] = conversionMatch.Groups[2].Value;
                modDef.parameters["targetType"] = conversionMatch.Groups[3].Value;
            }
        }
        // Conditional damage (e.g., "deal X% more damage to bleeding targets")
        else if (Regex.IsMatch(effectLower, @"deal\s+\d+%\s*(more|increased)\s*damage\s*to\s*(bleeding|ignited|chilled|frozen|shocked|poisoned)", RegexOptions.IgnoreCase))
        {
            Match condMatch = Regex.Match(effectLower, @"deal\s+(\d+)%\s*(more|increased)\s*damage\s*to\s*(bleeding|ignited|chilled|frozen|shocked|poisoned)", RegexOptions.IgnoreCase);
            if (condMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddDamageMorePercent;
                modDef.eventType = ModifierEventType.OnDamageDealt;
                modDef.parameters["percent"] = SafeParseFloat(condMatch.Groups[1].Value);
                modDef.parameters["conditionStatus"] = condMatch.Groups[3].Value;
            }
        }
        // Life steal / Life on hit
        else if (Regex.IsMatch(effectLower, @"gain\s+\d+\s*life\s*(when\s*attacking|on\s*hit)", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectLower, @"\d+%\s*of\s*(spell\s*)?damage\s*as\s*lifesteal", RegexOptions.IgnoreCase))
        {
            Match lifeMatch = Regex.Match(effectLower, @"gain\s+(\d+)\s*life", RegexOptions.IgnoreCase);
            if (lifeMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnDamageDealt;
                modDef.parameters["statName"] = "lifeOnHit";
                modDef.parameters["flat"] = SafeParseFloat(lifeMatch.Groups[1].Value);
            }
            else
            {
                Match lifestealMatch = Regex.Match(effectLower, @"(\d+)%\s*of\s*(?:spell\s*)?damage\s*as\s*lifesteal", RegexOptions.IgnoreCase);
                if (lifestealMatch.Success)
                {
                    modDef.actionType = ModifierActionType.ModifyStat;
                    modDef.eventType = ModifierEventType.OnDamageDealt;
                    modDef.parameters["statName"] = "lifeLeechPercent";
                    modDef.parameters["percent"] = SafeParseFloat(lifestealMatch.Groups[1].Value);
                }
            }
        }
        // Critical strike chance/multiplier
        else if (Regex.IsMatch(effectLower, @"\d+%\s*(?:to\s*)?critical\s*strike\s*(chance|multiplier)", RegexOptions.IgnoreCase))
        {
            Match critMatch = Regex.Match(effectLower, @"(\d+)%\s*(?:to\s*)?critical\s*strike\s*(chance|multiplier)", RegexOptions.IgnoreCase);
            if (critMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                string statType = critMatch.Groups[2].Value == "chance" ? "criticalStrikeChance" : "criticalStrikeMultiplier";
                modDef.parameters["statName"] = statType;
                modDef.parameters["percent"] = SafeParseFloat(critMatch.Groups[1].Value);
            }
        }
        // Mana cost reduction
        else if (Regex.IsMatch(effectLower, @"(?:reduced|cost)\s+\d+\s*(?:less\s*)?(?:base\s*)?mana", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectLower, @"\d+%\s*(?:reduced|less)\s*mana\s*cost", RegexOptions.IgnoreCase))
        {
            Match manaMatch = Regex.Match(effectLower, @"(?:reduced|cost)\s+(\d+)\s*(?:less\s*)?(?:base\s*)?mana", RegexOptions.IgnoreCase);
            if (manaMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["statName"] = "manaCostReduction";
                modDef.parameters["flat"] = SafeParseFloat(manaMatch.Groups[1].Value);
            }
            else
            {
                Match manaPercentMatch = Regex.Match(effectLower, @"(\d+)%\s*(?:reduced|less)\s*mana\s*cost", RegexOptions.IgnoreCase);
                if (manaPercentMatch.Success)
                {
                    modDef.actionType = ModifierActionType.ModifyStat;
                    modDef.eventType = ModifierEventType.OnCardPlayed;
                    modDef.parameters["statName"] = "manaCostReductionPercent";
                    modDef.parameters["percent"] = SafeParseFloat(manaPercentMatch.Groups[1].Value);
                }
            }
        }
        // Echo effects
        else if (Regex.IsMatch(effectLower, @"echo\s+(?:their\s*)?(?:effect\s*)?(?:with\s*)?\d+%\s*effect", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectLower, @"echo\s+\d+\s*times", RegexOptions.IgnoreCase))
        {
            Match echoMatch = Regex.Match(effectLower, @"echo\s+(?:their\s*)?(?:effect\s*)?(?:with\s*)?(\d+)%\s*effect", RegexOptions.IgnoreCase);
            if (echoMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddExtraHit;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["echoCount"] = 1;
                modDef.parameters["echoEffectPercent"] = SafeParseFloat(echoMatch.Groups[1].Value);
            }
            else
            {
                Match echoCountMatch = Regex.Match(effectLower, @"echo\s+(\d+)\s*times", RegexOptions.IgnoreCase);
                if (echoCountMatch.Success)
                {
                    modDef.actionType = ModifierActionType.AddExtraHit;
                    modDef.eventType = ModifierEventType.OnCardPlayed;
                    modDef.parameters["echoCount"] = int.Parse(echoCountMatch.Groups[1].Value);
                    modDef.parameters["echoEffectPercent"] = 100f; // Default to 100% if not specified
                }
            }
        }
        // AoE / Cleave effects
        else if (Regex.IsMatch(effectLower, @"hit\s+(?:adjacent\s*)?enemies?\s+for\s+\d+%\s*effect", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectLower, @"targets?\s+all\s+enemies", RegexOptions.IgnoreCase))
        {
            Match aoeMatch = Regex.Match(effectLower, @"hit\s+(?:adjacent\s*)?enemies?\s+for\s+(\d+)%\s*effect", RegexOptions.IgnoreCase);
            if (aoeMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddExtraHit;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["aoeEffectPercent"] = SafeParseFloat(aoeMatch.Groups[1].Value);
                modDef.parameters["targetsAllEnemies"] = true;
            }
            else if (Regex.IsMatch(effectLower, @"targets?\s+all\s+enemies", RegexOptions.IgnoreCase))
            {
                modDef.actionType = ModifierActionType.AddExtraHit;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["targetsAllEnemies"] = true;
                modDef.parameters["aoeEffectPercent"] = 100f;
            }
        }
        // Stagger / Guard effects
        else if (Regex.IsMatch(effectLower, @"\d+%\s*(?:increased|more)\s*stagger", RegexOptions.IgnoreCase))
        {
            Match staggerMatch = Regex.Match(effectLower, @"(\d+)%\s*(?:increased|more)\s*stagger", RegexOptions.IgnoreCase);
            if (staggerMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnDamageDealt;
                modDef.parameters["statName"] = "staggerMultiplier";
                modDef.parameters["percent"] = SafeParseFloat(staggerMatch.Groups[1].Value);
            }
        }
        // Stack gain effects (Momentum, Tolerance, Flow, etc.)
        else if (Regex.IsMatch(effectLower, @"gain\s+\d+\s*(momentum|tolerance|agitate|potential|flow|bolster)\s*stack", RegexOptions.IgnoreCase))
        {
            Match stackMatch = Regex.Match(effectLower, @"gain\s+(\d+)\s*(momentum|tolerance|agitate|potential|flow|bolster)\s*stack", RegexOptions.IgnoreCase);
            if (stackMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddStack;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["stackType"] = stackMatch.Groups[2].Value;
                modDef.parameters["stackCount"] = int.Parse(stackMatch.Groups[1].Value);
            }
        }
        // Preparation charges
        else if (Regex.IsMatch(effectLower, @"\d+%\s*(?:more|increased)\s*preparation\s*charges?", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectLower, @"prepared\s+for\s+\d+%\s*(?:more|longer)", RegexOptions.IgnoreCase))
        {
            Match prepMatch = Regex.Match(effectLower, @"(\d+)%\s*(?:more|increased)\s*preparation\s*charges?", RegexOptions.IgnoreCase);
            if (prepMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["statName"] = "preparationChargeMultiplier";
                modDef.parameters["percent"] = SafeParseFloat(prepMatch.Groups[1].Value);
            }
            else
            {
                Match prepDurationMatch = Regex.Match(effectLower, @"prepared\s+for\s+(\d+)%\s*(?:more|longer)", RegexOptions.IgnoreCase);
                if (prepDurationMatch.Success)
                {
                    modDef.actionType = ModifierActionType.ModifyStat;
                    modDef.eventType = ModifierEventType.OnCardPlayed;
                    modDef.parameters["statName"] = "preparationDurationMultiplier";
                    modDef.parameters["percent"] = SafeParseFloat(prepDurationMatch.Groups[1].Value);
                }
            }
        }
        // Penetration effects
        else if (Regex.IsMatch(effectLower, @"penetrate\s+\d+%\s*(?:enemy\s*)?(?:fire|cold|lightning|elemental)\s*resistance", RegexOptions.IgnoreCase))
        {
            Match penMatch = Regex.Match(effectLower, @"penetrate\s+(\d+)%\s*(?:enemy\s*)?(fire|cold|lightning|elemental)\s*resistance", RegexOptions.IgnoreCase);
            if (penMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnDamageDealt;
                modDef.parameters["statName"] = "resistancePenetration";
                modDef.parameters["percent"] = SafeParseFloat(penMatch.Groups[1].Value);
                modDef.parameters["resistanceType"] = penMatch.Groups[2].Value;
            }
        }
        // Ailment magnitude
        else if (Regex.IsMatch(effectLower, @"\d+%\s*(?:more|increased)\s*(?:ailment|chill|shock|ignite|poison|bleed)\s*magnitude", RegexOptions.IgnoreCase))
        {
            Match ailmentMatch = Regex.Match(effectLower, @"(\d+)%\s*(?:more|increased)\s*(?:ailment|chill|shock|ignite|poison|bleed)\s*magnitude", RegexOptions.IgnoreCase);
            if (ailmentMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnStatusApplied;
                modDef.parameters["statName"] = "ailmentMagnitude";
                modDef.parameters["percent"] = SafeParseFloat(ailmentMatch.Groups[1].Value);
            }
        }
        // Culling (instant kill at low HP)
        else if (Regex.IsMatch(effectLower, @"instantly\s*kill.*\d+%\s*or\s*less", RegexOptions.IgnoreCase))
        {
            Match cullMatch = Regex.Match(effectLower, @"(\d+)%\s*or\s*less", RegexOptions.IgnoreCase);
            if (cullMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnDamageDealt;
                modDef.parameters["statName"] = "cullingThreshold";
                modDef.parameters["percent"] = SafeParseFloat(cullMatch.Groups[1].Value) / 100f;
            }
        }
        // Ignore block
        else if (Regex.IsMatch(effectLower, @"ignore\s*(?:enemy\s*)?block", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.ModifyStat;
            modDef.eventType = ModifierEventType.OnDamageDealt;
            modDef.parameters["statName"] = "ignoreBlock";
            modDef.parameters["flat"] = 1f;
        }
        // Chain effects
        else if (Regex.IsMatch(effectLower, @"chain\s+to\s+\+\d+\s*(?:random\s*)?target", RegexOptions.IgnoreCase))
        {
            Match chainMatch = Regex.Match(effectLower, @"chain\s+to\s+\+(\d+)\s*(?:random\s*)?target", RegexOptions.IgnoreCase);
            if (chainMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddExtraHit;
                modDef.eventType = ModifierEventType.OnCardPlayed;
                modDef.parameters["chainCount"] = int.Parse(chainMatch.Groups[1].Value);
            }
        }
        // Status spread (Elemental Proliferation)
        else if (Regex.IsMatch(effectLower, @"spread\s+to\s+all\s+enemies", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.ApplyStatusToEnemy;
            modDef.eventType = ModifierEventType.OnStatusApplied;
            modDef.parameters["spreadToAllEnemies"] = true;
        }
        // Generic "more damage" when condition met
        else if (Regex.IsMatch(effectLower, @"deal\s+\d+%\s*more\s*damage\s*when", RegexOptions.IgnoreCase))
        {
            Match condDamageMatch = Regex.Match(effectLower, @"deal\s+(\d+)%\s*more\s*damage\s*when", RegexOptions.IgnoreCase);
            if (condDamageMatch.Success)
            {
                modDef.actionType = ModifierActionType.AddDamageMorePercent;
                modDef.eventType = ModifierEventType.OnDamageDealt;
                modDef.parameters["percent"] = SafeParseFloat(condDamageMatch.Groups[1].Value);
                modDef.needsManualConfig = true; // Condition needs manual setup
            }
        }
        // If no pattern matched, create a generic modifier that needs manual configuration
        if (modDef.actionType == ModifierActionType.AddStack && modDef.parameters.Count == 0)
        {
            modDef.actionType = ModifierActionType.ModifyStat;
            modDef.eventType = ModifierEventType.OnCardPlayed;
            modDef.parameters["customEffect"] = effectText;
            modDef.needsManualConfig = true;
        }
        
        return modDef;
    }
    
    private bool CreateModifierAsset(string embossingName, string embossingId, ModifierDefinition modDef)
    {
        // Create modifier ID
        string modifierId = SanitizeFileName(embossingId) + "_" + modDef.actionType.ToString();
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
            Debug.Log($"[EmbossingModifierGenerator] Skipping {modifierId} - asset already exists");
            return false;
        }
        
        // Create or load existing asset
        EmbossingModifierDefinition modifier = AssetDatabase.LoadAssetAtPath<EmbossingModifierDefinition>(assetPath);
        if (modifier == null)
        {
            modifier = ScriptableObject.CreateInstance<EmbossingModifierDefinition>();
        }
        
        // Populate modifier data
        modifier.modifierId = modifierId;
        modifier.linkedEmbossingId = embossingId;
        modifier.description = modDef.description;
        modifier.minLevel = modDef.minLevel;
        modifier.maxLevel = 0; // 0 = no max (scales to level 20 via _level20 parameters if needed)
        modifier.requiredCardTags = modDef.requiredTags;
        
        // Create ModifierEffect
        ModifierEffect effect = new ModifierEffect();
        effect.eventType = modDef.eventType;
        effect.conditions = new List<ModifierCondition>(); // Can be populated later if needed
        
        // Create ModifierAction
        ModifierAction action = new ModifierAction();
        action.actionType = modDef.actionType;
        action.executionOrder = 0;
        
        // Add parameters to action
        foreach (var kvp in modDef.parameters)
        {
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
        
        Debug.Log($"[EmbossingModifierGenerator] Created/Updated: {assetPath}");
        return true;
    }
    
    private string SanitizeFileName(string fileName)
    {
        return Regex.Replace(fileName, @"[<>:""/\\|?*]", "").Trim();
    }
    
    private class ModifierDefinition
    {
        public string embossingName;
        public string embossingId;
        public string description;
        public int minLevel = 1;
        public List<string> requiredTags = new List<string>();
        public ModifierEventType eventType = ModifierEventType.OnCardPlayed;
        public ModifierActionType actionType = ModifierActionType.AddStack; // Default placeholder
        public Dictionary<string, object> parameters = new Dictionary<string, object>();
        public bool needsManualConfig = false;
    }
}

