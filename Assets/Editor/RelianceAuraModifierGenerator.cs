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
        
        tsvFile = (TextAsset)EditorGUILayout.ObjectField("TSV File", tsvFile, typeof(TextAsset), false);
        
        EditorGUILayout.Space();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This tool generates RelianceAuraModifierDefinition assets based on the effects in the TSV file. Each aura will get modifier definitions for its effects.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Modifiers"))
        {
            if (tsvFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a TSV file.", "OK");
                return;
            }
            
            GenerateModifiers();
        }
    }
    
    private void GenerateModifiers()
    {
        if (tsvFile == null)
        {
            Debug.LogError("[RelianceAuraModifierGenerator] No TSV file selected!");
            return;
        }
        
        string[] lines = tsvFile.text.Split('\n');
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
                
                if (string.IsNullOrEmpty(auraName))
                    continue;
                
                // Generate modifiers for this aura
                List<ModifierDefinition> modifiers = ParseEffects(auraName, effectLevel1, effectLevel20);
                
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
    
    private List<ModifierDefinition> ParseEffects(string auraName, string effectLevel1, string effectLevel20)
    {
        List<ModifierDefinition> modifiers = new List<ModifierDefinition>();
        
        // Parse Level 1 effect (primary effect)
        ParseEffect(auraName, effectLevel1, modifiers, 1);
        
        // Parse Level 20 effect (may have additional effects)
        ParseEffect(auraName, effectLevel20, modifiers, 20);
        
        return modifiers;
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
        
        // Flat damage additions
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
        }
        // Percentage damage increases (including "Gain +X%" patterns)
        else if (Regex.IsMatch(effectText, @"(gain\s+\+)?\d+%\s*(increased|more)\s*(fire|cold|lightning|physical|spell|elemental)\s*damage", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"gain\s+\+(\d+)%\s*(fire|cold|lightning|physical|spell|elemental)\s*damage", RegexOptions.IgnoreCase))
        {
            Match percentMatch = Regex.Match(effectText, @"(?:gain\s+\+)?(\d+)%\s*(increased|more)?\s*(fire|cold|lightning|physical|spell|elemental)", RegexOptions.IgnoreCase);
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
        // Status effect magnitude increases
        else if (Regex.IsMatch(effectText, @"\d+%\s*increased\s*(ignite|chill|shock|poison|ailment)\s*magnitude"))
        {
            Match statusMatch = Regex.Match(effectText, @"(\d+)%\s*increased\s*(ignite|chill|shock|poison|ailment)\s*magnitude");
            if (statusMatch.Success)
            {
                modDef.actionType = ModifierActionType.ModifyStat;
                modDef.eventType = ModifierEventType.OnCombatStart;
                modDef.parameters["statName"] = statusMatch.Groups[2].Value + "Magnitude";
                modDef.parameters["percent"] = float.Parse(statusMatch.Groups[1].Value);
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
        
        // Cast Spell on Kill (Law of Tempest)
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
        }
        
        // Apply Crumble (Law of Force - part 1)
        else if (Regex.IsMatch(effectText, @"enemies\s+you\s+hit\s+gain\s+crumble", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.ApplyCrumble;
            modDef.eventType = ModifierEventType.OnDamageDealtToEnemy;
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
        
        // Add Poison Stacks (Law of Pale Venin)
        else if (Regex.IsMatch(effectText, @"\+(\d+)\s+poison\s+stacks", RegexOptions.IgnoreCase) ||
                 Regex.IsMatch(effectText, @"additional\s+stack.*poison", RegexOptions.IgnoreCase))
        {
            modDef.actionType = ModifierActionType.AddPoisonStacks;
            modDef.eventType = ModifierEventType.OnStatusApplied;
            
            // Extract stack count
            Match stackMatch = Regex.Match(effectText, @"\+(\d+)\s+poison\s+stacks", RegexOptions.IgnoreCase);
            int stacks = stackMatch.Success ? int.Parse(stackMatch.Groups[1].Value) : 1;
            modDef.parameters["additionalStacks"] = stacks;
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
        // Create modifier ID
        string modifierId = SanitizeFileName(auraName) + "_" + modDef.actionType.ToString() + "_L" + modDef.level;
        if (modDef.parameters.ContainsKey("damageType"))
        {
            modifierId += "_" + modDef.parameters["damageType"].ToString();
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
        modifier.minLevel = modDef.level == 1 ? 1 : modDef.level;
        modifier.maxLevel = modDef.level == 20 ? 0 : modDef.level; // 0 = no max
        
        // Create ModifierEffect
        ModifierEffect effect = new ModifierEffect();
        effect.eventType = modDef.eventType;
        effect.priority = 0;
        
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

