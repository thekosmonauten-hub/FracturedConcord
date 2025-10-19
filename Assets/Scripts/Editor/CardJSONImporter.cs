using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Unity Editor tool for importing cards from JSON files.
/// Supports both single card imports and full starter deck imports.
/// Menu: Tools > Cards > Import Cards from JSON
/// </summary>
public class CardJSONImporter : EditorWindow
{
    private string jsonFilePath = "Assets/Resources/CardJSON/MarauderStarterDeck.json";
    private CardDatabase targetDatabase;
    private Vector2 scrollPosition;
    private string importLog = "";
    private bool createScriptableObjects = true;
    private string scriptableObjectPath = "Assets/Resources/Cards/";
    
    [MenuItem("Tools/Cards/Import Cards from JSON")]
    public static void ShowWindow()
    {
        var window = GetWindow<CardJSONImporter>("Card JSON Importer");
        window.minSize = new Vector2(500, 600);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Card JSON Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Import cards from JSON files into your CardDatabase.\n" +
            "Supports both single card files and starter deck collections.", 
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // File selection
        EditorGUILayout.BeginHorizontal();
        jsonFilePath = EditorGUILayout.TextField("JSON File Path:", jsonFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select JSON File", "Assets/Resources/CardJSON", "json");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative Unity path
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
        targetDatabase = (CardDatabase)EditorGUILayout.ObjectField(
            "Target Database:", 
            targetDatabase, 
            typeof(CardDatabase), 
            false
        );
        
        EditorGUILayout.Space();
        
        // ScriptableObject creation options
        createScriptableObjects = EditorGUILayout.Toggle(
            "Create ScriptableObjects", 
            createScriptableObjects
        );
        
        if (createScriptableObjects)
        {
            EditorGUI.indentLevel++;
            scriptableObjectPath = EditorGUILayout.TextField(
                "Save Path:", 
                scriptableObjectPath
            );
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Import buttons
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = !string.IsNullOrEmpty(jsonFilePath) && File.Exists(jsonFilePath);
        if (GUILayout.Button("Import Cards", GUILayout.Height(30)))
        {
            ImportCards();
        }
        
        if (GUILayout.Button("Validate JSON", GUILayout.Height(30)))
        {
            ValidateJSON();
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Quick load buttons
        EditorGUILayout.LabelField("Quick Load:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Marauder Deck"))
        {
            jsonFilePath = "Assets/Resources/CardJSON/MarauderStarterDeck.json";
        }
        
        if (GUILayout.Button("Witch Deck"))
        {
            jsonFilePath = "Assets/Resources/CardJSON/WitchStarterDeck.json";
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Import log
        EditorGUILayout.LabelField("Import Log:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        EditorGUILayout.TextArea(importLog, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        
        if (GUILayout.Button("Clear Log"))
        {
            importLog = "";
        }
    }
    
    private void ValidateJSON()
    {
        if (!File.Exists(jsonFilePath))
        {
            LogError($"File not found: {jsonFilePath}");
            return;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            
            // Try to determine if it's a deck or single card
            if (jsonContent.Contains("\"deckName\"") && jsonContent.Contains("\"cards\""))
            {
                var deckData = JsonUtility.FromJson<JSONDeckFormat>(jsonContent);
                LogSuccess($"✓ Valid deck JSON: {deckData.deckName}");
                LogInfo($"  - Character Class: {deckData.characterClass}");
                LogInfo($"  - Total Card Types: {deckData.cards.Length}");
                
                int totalCards = 0;
                foreach (var cardEntry in deckData.cards)
                {
                    totalCards += cardEntry.count;
                }
                LogInfo($"  - Total Cards: {totalCards}");
            }
            else
            {
                var cardData = JsonUtility.FromJson<JSONCardFormat>(jsonContent);
                LogSuccess($"✓ Valid single card JSON: {cardData.cardName}");
                LogInfo($"  - Type: {cardData.cardType}");
                LogInfo($"  - Mana Cost: {cardData.manaCost}");
            }
        }
        catch (System.Exception e)
        {
            LogError($"JSON Validation Failed: {e.Message}");
        }
    }
    
    private void ImportCards()
    {
        if (!File.Exists(jsonFilePath))
        {
            LogError($"File not found: {jsonFilePath}");
            return;
        }
        
        if (targetDatabase == null && !createScriptableObjects)
        {
            LogError("Please select a target database or enable ScriptableObject creation");
            return;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            
            // Determine if it's a deck or single card
            if (jsonContent.Contains("\"deckName\"") && jsonContent.Contains("\"cards\""))
            {
                ImportDeck(jsonContent);
            }
            else
            {
                ImportSingleCard(jsonContent);
            }
            
            if (targetDatabase != null)
            {
                EditorUtility.SetDirty(targetDatabase);
                AssetDatabase.SaveAssets();
                LogSuccess("✓ Database saved");
            }
            
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            LogError($"Import Failed: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void ImportDeck(string jsonContent)
    {
        var deckData = JsonUtility.FromJson<JSONDeckFormat>(jsonContent);
        
        LogInfo($"Importing deck: {deckData.deckName}");
        LogInfo($"Class: {deckData.characterClass}");
        LogInfo($"Description: {deckData.description}");
        LogInfo("---");
        
        int successCount = 0;
        int totalCards = 0;
        
        foreach (var cardEntry in deckData.cards)
        {
            LogInfo($"Importing: {cardEntry.data.cardName} (x{cardEntry.count})");
            
            Card card = ConvertJSONToCard(cardEntry.data);
            
            if (card != null)
            {
                // Create ScriptableObject if enabled
                if (createScriptableObjects)
                {
                    CreateCardDataScriptableObject(card, deckData.characterClass);
                }
                
                // Add to database if specified
                if (targetDatabase != null)
                {
                    // Note: Database expects CardData, not Card
                    // You may need to convert Card to CardData or use a different approach
                    LogWarning($"  ⚠ Card '{card.cardName}' created but not added to database (Card -> CardData conversion needed)");
                }
                
                successCount++;
                totalCards += cardEntry.count;
            }
            else
            {
                LogError($"  ✗ Failed to import: {cardEntry.data.cardName}");
            }
        }
        
        LogSuccess($"✓ Imported {successCount}/{deckData.cards.Length} card types ({totalCards} total cards)");
    }
    
    private void ImportSingleCard(string jsonContent)
    {
        var cardData = JsonUtility.FromJson<JSONCardFormat>(jsonContent);
        
        LogInfo($"Importing single card: {cardData.cardName}");
        
        Card card = ConvertJSONToCard(cardData);
        
        if (card != null)
        {
            if (createScriptableObjects)
            {
                CreateCardDataScriptableObject(card, "Generic");
            }
            
            LogSuccess($"✓ Imported: {card.cardName}");
        }
        else
        {
            LogError($"✗ Failed to import: {cardData.cardName}");
        }
    }
    
    private Card ConvertJSONToCard(JSONCardFormat jsonCard)
    {
        Card card = new Card
        {
            cardName = jsonCard.cardName,
            description = jsonCard.description,
            cardType = ParseCardType(jsonCard.cardType),
            manaCost = jsonCard.manaCost,
            baseDamage = jsonCard.baseDamage,
            baseGuard = jsonCard.baseGuard,
            primaryDamageType = ParseDamageType(jsonCard.primaryDamageType),
            
            // Weapon scaling
            scalesWithMeleeWeapon = jsonCard.weaponScaling.scalesWithMeleeWeapon,
            scalesWithProjectileWeapon = jsonCard.weaponScaling.scalesWithProjectileWeapon,
            scalesWithSpellWeapon = jsonCard.weaponScaling.scalesWithSpellWeapon,
            
            // AoE
            isAoE = jsonCard.aoe.isAoE,
            aoeTargets = jsonCard.aoe.aoeTargets,
            
            // Requirements
            requirements = new CardRequirements
            {
                requiredStrength = jsonCard.requirements.requiredStrength,
                requiredDexterity = jsonCard.requirements.requiredDexterity,
                requiredIntelligence = jsonCard.requirements.requiredIntelligence,
                requiredLevel = jsonCard.requirements.requiredLevel,
                requiredWeaponTypes = jsonCard.requirements.requiredWeaponTypes
                    .Select(t => ParseWeaponType(t))
                    .ToList()
            },
            
            // Tags
            tags = new List<string>(jsonCard.tags),
            
            // Additional damage types
            additionalDamageTypes = jsonCard.additionalDamageTypes
                .Select(t => ParseDamageType(t))
                .ToList(),
            
            // Scaling
            damageScaling = new AttributeScaling
            {
                strengthScaling = jsonCard.damageScaling.strengthScaling,
                dexterityScaling = jsonCard.damageScaling.dexterityScaling,
                intelligenceScaling = jsonCard.damageScaling.intelligenceScaling
            },
            
            guardScaling = new AttributeScaling
            {
                strengthScaling = jsonCard.guardScaling.strengthScaling,
                dexterityScaling = jsonCard.guardScaling.dexterityScaling,
                intelligenceScaling = jsonCard.guardScaling.intelligenceScaling
            },
            
            // Effects
            effects = jsonCard.effects.Select(e => new CardEffect
            {
                effectType = ParseEffectType(e.effectType),
                effectName = e.effectName,
                description = e.description,
                value = e.value,
                duration = e.duration,
                damageType = ParseDamageType(e.damageType),
                targetsSelf = e.targetsSelf,
                targetsEnemy = e.targetsEnemy,
                targetsAllEnemies = e.targetsAllEnemies,
                targetsAll = e.targetsAll,
                condition = e.condition
            }).ToList()
        };
        
        return card;
    }
    
    private void CreateCardDataScriptableObject(Card card, string characterClass)
    {
        // Ensure directory exists
        if (!Directory.Exists(scriptableObjectPath))
        {
            Directory.CreateDirectory(scriptableObjectPath);
        }
        
        string className = characterClass.Replace(" ", "");
        string classFolder = $"{scriptableObjectPath}{className}/";
        
        if (!Directory.Exists(classFolder))
        {
            Directory.CreateDirectory(classFolder);
        }
        
        // Create CardData ScriptableObject
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        cardData.cardName = card.cardName;
        cardData.cardType = card.cardType.ToString();
        cardData.playCost = card.manaCost;
        cardData.description = card.description;
        cardData.rarity = CardRarity.Common; // Default, can be customized
        cardData.element = ParseCardElement(card.primaryDamageType);
        cardData.category = ParseCardCategory(card.cardType);
        cardData.damage = (int)card.baseDamage;
        cardData.block = (int)card.baseGuard;
        
        // Create asset
        string assetPath = $"{classFolder}{card.cardName.Replace(" ", "")}.asset";
        AssetDatabase.CreateAsset(cardData, assetPath);
        
        LogSuccess($"  ✓ Created ScriptableObject: {assetPath}");
    }
    
    // Parsing helpers
    private CardType ParseCardType(string type)
    {
        return System.Enum.TryParse<CardType>(type, true, out var result) ? result : CardType.Attack;
    }
    
    private DamageType ParseDamageType(string type)
    {
        return System.Enum.TryParse<DamageType>(type, true, out var result) ? result : DamageType.Physical;
    }
    
    private WeaponType ParseWeaponType(string type)
    {
        return System.Enum.TryParse<WeaponType>(type, true, out var result) ? result : WeaponType.Melee;
    }
    
    private EffectType ParseEffectType(string type)
    {
        return System.Enum.TryParse<EffectType>(type, true, out var result) ? result : EffectType.Damage;
    }
    
    private CardElement ParseCardElement(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Fire: return CardElement.Fire;
            case DamageType.Cold: return CardElement.Cold;
            case DamageType.Lightning: return CardElement.Lightning;
            case DamageType.Physical: return CardElement.Physical;
            case DamageType.Chaos: return CardElement.Chaos;
            default: return CardElement.Basic;
        }
    }
    
    private CardCategory ParseCardCategory(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Attack: return CardCategory.Attack;
            case CardType.Guard: return CardCategory.Guard;
            case CardType.Skill: return CardCategory.Skill;
            case CardType.Power: return CardCategory.Power;
            default: return CardCategory.Attack;
        }
    }
    
    // Logging helpers
    private void LogInfo(string message)
    {
        importLog += $"[INFO] {message}\n";
        Debug.Log(message);
    }
    
    private void LogSuccess(string message)
    {
        importLog += $"<color=green>[SUCCESS]</color> {message}\n";
        Debug.Log($"<color=green>{message}</color>");
    }
    
    private void LogWarning(string message)
    {
        importLog += $"<color=yellow>[WARNING]</color> {message}\n";
        Debug.LogWarning(message);
    }
    
    private void LogError(string message)
    {
        importLog += $"<color=red>[ERROR]</color> {message}\n";
        Debug.LogError(message);
    }
}

// JSON Data Structures are now in CardJSONFormat.cs (shared between Editor and Runtime)

