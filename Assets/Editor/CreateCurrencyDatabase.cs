using UnityEngine;
using UnityEditor;
using Dexiled.Data.Items;
using System.IO;

public class CreateCurrencyDatabase : EditorWindow
{
    private string databaseName = "CurrencyDatabase";
    private string savePath = "Assets/Resources/";
    
    [MenuItem("Tools/Dexiled/Create Currency Database")]
    public static void ShowWindow()
    {
        GetWindow<CreateCurrencyDatabase>("Create Currency Database");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Currency Database Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool creates a new CurrencyDatabase ScriptableObject with all currency types pre-configured.", MessageType.Info);
        EditorGUILayout.Space();
        
        databaseName = EditorGUILayout.TextField("Database Name:", databaseName);
        
        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField("Save Path:", savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Save Location", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                savePath = "Assets" + path.Substring(Application.dataPath.Length) + "/";
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Currency Database", GUILayout.Height(40)))
        {
            CreateDatabase();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Database + Assign to Resources", GUILayout.Height(40)))
        {
            savePath = "Assets/Resources/";
            databaseName = "CurrencyDatabase";
            CreateDatabase();
        }
    }
    
    private void CreateDatabase()
    {
        // Ensure save path exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        // Create the database
        CurrencyDatabase database = ScriptableObject.CreateInstance<CurrencyDatabase>();
        
        // Initialize with all currencies
        database.currencies.Clear();
        
        Debug.Log("Creating CurrencyDatabase with all currency types...");
        
        // ORBS
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfGeneration, 
            "Orb of Generation", 
            "Allows you to generate a card", 
            ItemRarity.Normal, 0));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfInfusion, 
            "Orb of Infusion", 
            "Reforges Normal equipment, making it Magic and adds a random affix", 
            ItemRarity.Normal, 1));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfPerfection, 
            "Orb of Perfection", 
            "Reforges Magic equipment making it Rare and adds a random affix", 
            ItemRarity.Magic, 2));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfPerpetuity, 
            "Orb of Perpetuity", 
            "Adds a random affix to Rare equipment", 
            ItemRarity.Magic, 3));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfRedundancy, 
            "Orb of Redundancy", 
            "Removes a random affix from an equipment", 
            ItemRarity.Normal, 4));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfTheVoid, 
            "Orb of the Void", 
            "Corrupts an item, adding powerful but unpredictable modifiers", 
            ItemRarity.Rare, 5));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfMutation, 
            "Orb of Mutation", 
            "Randomizes all affixes on a Magic equipment", 
            ItemRarity.Magic, 6));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfProliferation, 
            "Orb of Proliferation", 
            "Adds a random affix to a Magic equipment", 
            ItemRarity.Rare, 7));
            
        database.currencies.Add(CreateCurrency(CurrencyType.OrbOfAmnesia, 
            "Orb of Amnesia", 
            "Removes all affixes while preserving locked affixes", 
            ItemRarity.Magic, 8));
        
        // SPIRITS
        database.currencies.Add(CreateCurrency(CurrencyType.FireSpirit, 
            "Fire Spirit", 
            "Adds or rerolls fire damage affixes on equipment", 
            ItemRarity.Normal, 9));
            
        database.currencies.Add(CreateCurrency(CurrencyType.ColdSpirit, 
            "Cold Spirit", 
            "Adds or rerolls cold damage affixes on equipment", 
            ItemRarity.Normal, 10));
            
        database.currencies.Add(CreateCurrency(CurrencyType.LightningSpirit, 
            "Lightning Spirit", 
            "Adds or rerolls lightning damage affixes on equipment", 
            ItemRarity.Normal, 11));
            
        database.currencies.Add(CreateCurrency(CurrencyType.ChaosSpirit, 
            "Chaos Spirit", 
            "Adds or rerolls chaos damage affixes on equipment", 
            ItemRarity.Magic, 12));
            
        database.currencies.Add(CreateCurrency(CurrencyType.PhysicalSpirit, 
            "Physical Spirit", 
            "Adds or rerolls physical damage affixes on equipment", 
            ItemRarity.Normal, 13));
            
        database.currencies.Add(CreateCurrency(CurrencyType.LifeSpirit, 
            "Life Spirit", 
            "Adds or rerolls life affixes on equipment", 
            ItemRarity.Normal, 14));
            
        database.currencies.Add(CreateCurrency(CurrencyType.DefenseSpirit, 
            "Defense Spirit", 
            "Adds or rerolls defense affixes on equipment", 
            ItemRarity.Normal, 15));
            
        database.currencies.Add(CreateCurrency(CurrencyType.CritSpirit, 
            "Crit Spirit", 
            "Adds or rerolls critical strike affixes on equipment", 
            ItemRarity.Magic, 16));
            
        database.currencies.Add(CreateCurrency(CurrencyType.DivineSpirit, 
            "Divine Spirit", 
            "Rerolls the value of a random affix to maximum", 
            ItemRarity.Rare, 17));
        
        // SEALS
        database.currencies.Add(CreateCurrency(CurrencyType.TranspositionSeal, 
            "Transposition Seal", 
            "Swaps two affixes between two items", 
            ItemRarity.Magic, 18));
            
        database.currencies.Add(CreateCurrency(CurrencyType.ChaosSeal, 
            "Chaos Seal", 
            "Randomly shuffles all affixes on an item", 
            ItemRarity.Normal, 19));
            
        database.currencies.Add(CreateCurrency(CurrencyType.MemorySeal, 
            "Memory Seal", 
            "Saves the current state of an item for later restoration", 
            ItemRarity.Rare, 20));
            
        database.currencies.Add(CreateCurrency(CurrencyType.InscriptionSeal, 
            "Inscription Seal", 
            "Locks an affix preventing it from being changed", 
            ItemRarity.Magic, 21));
            
        database.currencies.Add(CreateCurrency(CurrencyType.AdaptationSeal, 
            "Adaptation Seal", 
            "Changes the type of an affix while keeping its tier", 
            ItemRarity.Magic, 22));
            
        database.currencies.Add(CreateCurrency(CurrencyType.CorrectionSeal, 
            "Correction Seal", 
            "Removes the lowest tier affix from an item", 
            ItemRarity.Normal, 23));
            
        database.currencies.Add(CreateCurrency(CurrencyType.EtchingSeal, 
            "Etching Seal", 
            "Improves the tier of a random affix by one", 
            ItemRarity.Rare, 24));
        
        // FRAGMENTS (Placeholders)
        database.currencies.Add(CreateCurrency(CurrencyType.Fragment1, 
            "Fragment 1", 
            "Mysterious fragment - purpose unknown", 
            ItemRarity.Normal, 25));
            
        database.currencies.Add(CreateCurrency(CurrencyType.Fragment2, 
            "Fragment 2", 
            "Mysterious fragment - purpose unknown", 
            ItemRarity.Normal, 26));
            
        database.currencies.Add(CreateCurrency(CurrencyType.Fragment3, 
            "Fragment 3", 
            "Mysterious fragment - purpose unknown", 
            ItemRarity.Normal, 27));
        
        // Save the asset
        string fullPath = savePath + databaseName + ".asset";
        AssetDatabase.CreateAsset(database, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = database;
        
        Debug.Log($"✓ Currency Database created at: {fullPath}");
        Debug.Log($"✓ Added {database.currencies.Count} currency types");
        
        EditorUtility.DisplayDialog("Success!", 
            $"Currency Database created successfully!\n\nLocation: {fullPath}\n\nCurrencies: {database.currencies.Count}", 
            "OK");
    }
    
    private CurrencyData CreateCurrency(CurrencyType type, string name, string desc, ItemRarity rarity, int slotIndex)
    {
        return new CurrencyData
        {
            currencyType = type,
            currencyName = name,
            description = desc,
            rarity = rarity,
            currencySprite = null, // Sprites can be assigned later
            quantity = 0,
            canTargetCards = false,
            canTargetEquipment = false,
            validEquipmentRarities = new ItemRarity[0],
            maxAffixesForTarget = 0,
            isCorruption = type == CurrencyType.OrbOfTheVoid,
            preservesLockedAffixes = type == CurrencyType.OrbOfAmnesia,
            slotIndex = slotIndex
        };
    }
}

