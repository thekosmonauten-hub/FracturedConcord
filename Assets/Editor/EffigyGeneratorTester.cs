using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor window for generating Effigies with rolled affixes for testing purposes
/// </summary>
public class EffigyGeneratorTester : EditorWindow
{
    private Effigy selectedBlueprint = null;
    private int itemLevel = 29;
    private ItemRarity targetRarity = ItemRarity.Rare;
    private int? seed = null;
    private bool useSeed = false;
    
    private Effigy generatedEffigy = null;
    private Vector2 scrollPosition;
    
    private bool addToCharacterStorage = true;
    
    // Shape category selection
    private EffigyShapeCategory selectedShapeCategory = EffigyShapeCategory.Unknown; // Unknown = Random
    private bool useRandomShape = true;
    
    // Cached effigy blueprints grouped by shape category
    private Dictionary<EffigyShapeCategory, List<Effigy>> effigiesByShape = new Dictionary<EffigyShapeCategory, List<Effigy>>();
    private bool blueprintsLoaded = false;
    
    [MenuItem("Dexiled/Effigy Generator Tester")]
    public static void ShowWindow()
    {
        GetWindow<EffigyGeneratorTester>("Effigy Generator");
    }
    
    private void OnEnable()
    {
        // Load effigy blueprints
        LoadEffigyBlueprints();
    }
    
    /// <summary>
    /// Get the current character from CharacterManager (always fresh, never cached)
    /// </summary>
    private Character GetCurrentCharacter()
    {
        if (!Application.isPlaying)
            return null;
            
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager != null && characterManager.HasCharacter())
        {
            return characterManager.GetCurrentCharacter();
        }
        return null;
    }
    
    /// <summary>
    /// Load all effigy blueprints from Resources and group them by shape category
    /// </summary>
    private void LoadEffigyBlueprints()
    {
        effigiesByShape.Clear();
        
        // Load all effigies from Resources/Items/Effigies
        Effigy[] allEffigies = Resources.LoadAll<Effigy>("Items/Effigies");
        
        foreach (Effigy effigy in allEffigies)
        {
            if (effigy == null) continue;
            
            EffigyShapeCategory category = effigy.GetShapeCategory();
            
            if (!effigiesByShape.ContainsKey(category))
            {
                effigiesByShape[category] = new List<Effigy>();
            }
            
            effigiesByShape[category].Add(effigy);
        }
        
        blueprintsLoaded = true;
        
        // Log summary
        Debug.Log($"[EffigyGeneratorTester] Loaded {allEffigies.Length} effigy blueprints:");
        foreach (var kvp in effigiesByShape.OrderBy(x => x.Key.ToString()))
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value.Count} blueprints");
        }
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Effigy Generator Tester", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // Reload blueprints button
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reload Blueprints", GUILayout.Width(150)))
        {
            LoadEffigyBlueprints();
        }
        
        if (blueprintsLoaded)
        {
            int totalBlueprints = effigiesByShape.Values.Sum(list => list.Count);
            EditorGUILayout.LabelField($"Loaded: {totalBlueprints} blueprints", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Shape category selection
        useRandomShape = EditorGUILayout.Toggle("Random Shape Category", useRandomShape);
        
        if (!useRandomShape)
        {
            // Show dropdown for specific shape category
            selectedShapeCategory = (EffigyShapeCategory)EditorGUILayout.EnumPopup(
                "Shape Category",
                selectedShapeCategory == EffigyShapeCategory.Unknown ? EffigyShapeCategory.Cross : selectedShapeCategory
            );
            
            if (selectedShapeCategory == EffigyShapeCategory.Unknown)
            {
                selectedShapeCategory = EffigyShapeCategory.Cross;
            }
            
            // Show available blueprints for selected category
            if (effigiesByShape.ContainsKey(selectedShapeCategory))
            {
                int count = effigiesByShape[selectedShapeCategory].Count;
                EditorGUILayout.LabelField($"Available blueprints: {count}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox($"No blueprints found for shape: {selectedShapeCategory}", MessageType.Warning);
            }
        }
        else
        {
            // Show summary of available shapes
            EditorGUILayout.LabelField("Available Shapes:", EditorStyles.miniLabel);
            var availableShapes = effigiesByShape.Keys.Where(k => k != EffigyShapeCategory.Unknown).OrderBy(k => k.ToString());
            string shapesList = string.Join(", ", availableShapes);
            EditorGUILayout.LabelField(shapesList, EditorStyles.miniLabel);
        }
        
        EditorGUILayout.Space(5);
        
        // Optional: Manual blueprint override (for testing specific blueprints)
        EditorGUILayout.LabelField("(Optional) Manual Blueprint Override:", EditorStyles.miniLabel);
        selectedBlueprint = (Effigy)EditorGUILayout.ObjectField(
            "Blueprint Override",
            selectedBlueprint,
            typeof(Effigy),
            false
        );
        
        EditorGUILayout.Space(5);
        
        // Item level
        itemLevel = EditorGUILayout.IntField("Item Level", itemLevel);
        itemLevel = Mathf.Clamp(itemLevel, 1, 100);
        
        // Rarity selection
        targetRarity = (ItemRarity)EditorGUILayout.EnumPopup("Target Rarity", targetRarity);
        
        EditorGUILayout.Space(5);
        
        // Seed option
        useSeed = EditorGUILayout.Toggle("Use Seed", useSeed);
        if (useSeed)
        {
            int seedValue = seed.HasValue ? seed.Value : System.Environment.TickCount;
            seedValue = EditorGUILayout.IntField("Seed", seedValue);
            seed = seedValue;
        }
        else
        {
            seed = null;
        }
        
        EditorGUILayout.Space(10);
        
        // Generate button
        if (GUILayout.Button("Generate Effigy", GUILayout.Height(30)))
        {
            GenerateEffigy();
        }
        
        EditorGUILayout.Space(10);
        
        // Character storage option
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            addToCharacterStorage = EditorGUILayout.Toggle("Add to Character Storage", addToCharacterStorage);
            
            Character currentChar = GetCurrentCharacter();
            if (currentChar != null)
            {
                EditorGUILayout.LabelField($"Character: {currentChar.characterName}", GUILayout.Width(200));
            }
            else
            {
                EditorGUILayout.LabelField("No character loaded", GUILayout.Width(200));
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to add generated effigies to character storage", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        // Display generated effigy info
        if (generatedEffigy != null)
        {
            EditorGUILayout.LabelField("Generated Effigy:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField($"Name: {generatedEffigy.effigyName}");
            EditorGUILayout.LabelField($"Rarity: {generatedEffigy.rarity}");
            EditorGUILayout.LabelField($"Shape: {generatedEffigy.GetShapeCategory()}");
            EditorGUILayout.LabelField($"Element: {generatedEffigy.element}");
            EditorGUILayout.LabelField($"Item Level: {itemLevel}");
            
            EditorGUILayout.Space(5);
            
            // Implicit modifiers
            if (generatedEffigy.implicitModifiers != null && generatedEffigy.implicitModifiers.Count > 0)
            {
                EditorGUILayout.LabelField("Implicit Modifiers:", EditorStyles.boldLabel);
                foreach (var implicitAffix in generatedEffigy.implicitModifiers)
                {
                    if (implicitAffix != null)
                    {
                        EditorGUILayout.LabelField($"  • {implicitAffix.description}");
                    }
                }
            }
            
            EditorGUILayout.Space(5);
            
            // Rolled affixes
            int affixCount = 0;
            if (generatedEffigy.prefixes != null) affixCount += generatedEffigy.prefixes.Count;
            if (generatedEffigy.suffixes != null) affixCount += generatedEffigy.suffixes.Count;
            
            EditorGUILayout.LabelField($"Rolled Affixes ({affixCount}):", EditorStyles.boldLabel);
            
            if (generatedEffigy.prefixes != null && generatedEffigy.prefixes.Count > 0)
            {
                foreach (var affix in generatedEffigy.prefixes)
                {
                    if (affix != null)
                    {
                        EditorGUILayout.LabelField($"  Prefix: {affix.name}");
                        EditorGUILayout.LabelField($"    {affix.description}");
                        if (affix.modifiers != null)
                        {
                            foreach (var mod in affix.modifiers)
                            {
                                if (mod != null)
                                {
                                    string valueStr = mod.isRolled ? mod.rolledValue.ToString("F2") : $"{mod.minValue}-{mod.maxValue}";
                                    EditorGUILayout.LabelField($"      {mod.statName}: {valueStr}");
                                }
                            }
                        }
                    }
                }
            }
            
            if (generatedEffigy.suffixes != null && generatedEffigy.suffixes.Count > 0)
            {
                foreach (var affix in generatedEffigy.suffixes)
                {
                    if (affix != null)
                    {
                        EditorGUILayout.LabelField($"  Suffix: {affix.name}");
                        EditorGUILayout.LabelField($"    {affix.description}");
                        if (affix.modifiers != null)
                        {
                            foreach (var mod in affix.modifiers)
                            {
                                if (mod != null)
                                {
                                    string valueStr = mod.isRolled ? mod.rolledValue.ToString("F2") : $"{mod.minValue}-{mod.maxValue}";
                                    EditorGUILayout.LabelField($"      {mod.statName}: {valueStr}");
                                }
                            }
                        }
                    }
                }
            }
            
            EditorGUILayout.Space(5);
            
            // Stat summary
            EditorGUILayout.LabelField("Stat Summary:", EditorStyles.boldLabel);
            Dictionary<string, float> stats = EquipmentManager.Instance != null 
                ? EquipmentManager.Instance.GetEffigyStats(generatedEffigy) 
                : new Dictionary<string, float>();
            
            if (stats.Count > 0)
            {
                foreach (var stat in stats.OrderBy(s => s.Key))
                {
                    EditorGUILayout.LabelField($"  {stat.Key}: {stat.Value:F2}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("  (No stats calculated)");
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    private void GenerateEffigy()
    {
        // Reload blueprints if needed
        if (!blueprintsLoaded)
        {
            LoadEffigyBlueprints();
        }
        
        Effigy blueprintToUse = null;
        
        // Use manual override if provided
        if (selectedBlueprint != null)
        {
            blueprintToUse = selectedBlueprint;
            Debug.Log($"[EffigyGeneratorTester] Using manual blueprint override: {blueprintToUse.effigyName}");
        }
        else
        {
            // Select blueprint based on shape category selection
            if (useRandomShape)
            {
                // Random shape category
                blueprintToUse = GetRandomBlueprint();
            }
            else
            {
                // Specific shape category
                blueprintToUse = GetRandomBlueprintFromCategory(selectedShapeCategory);
            }
            
            if (blueprintToUse == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    $"No blueprints available for shape category: {selectedShapeCategory}. " +
                    "Make sure effigies are loaded in Resources/Items/Effigies and have valid shape categories.", 
                    "OK");
                Debug.LogError($"[EffigyGeneratorTester] No blueprint found for shape: {selectedShapeCategory}");
                return;
            }
        }
        
        EffigyAffixDatabase database = EffigyAffixDatabase.Instance;
        if (database == null)
        {
            EditorUtility.DisplayDialog("Error", "EffigyAffixDatabase.Instance is null! Make sure it exists in Resources.", "OK");
            Debug.LogError("[EffigyGeneratorTester] EffigyAffixDatabase.Instance is null!");
            return;
        }
        
        Debug.Log($"[EffigyGeneratorTester] Generating effigy from blueprint: {blueprintToUse.effigyName}");
        Debug.Log($"[EffigyGeneratorTester] Shape Category: {blueprintToUse.GetShapeCategory()}");
        Debug.Log($"[EffigyGeneratorTester] Item Level: {itemLevel}, Rarity: {targetRarity}, Seed: {seed}");
        
        // Create instance using EffigyFactory
        // itemLevel determines which affix tiers can roll (affixes with minLevel <= itemLevel)
        generatedEffigy = EffigyFactory.CreateInstance(blueprintToUse, database, targetRarity, seed, itemLevel);
        
        if (generatedEffigy != null)
        {
            Debug.Log($"[EffigyGeneratorTester] ✓ Successfully generated: {generatedEffigy.effigyName}");
            Debug.Log($"[EffigyGeneratorTester] Rarity: {generatedEffigy.rarity}");
            Debug.Log($"[EffigyGeneratorTester] Shape: {generatedEffigy.GetShapeCategory()}");
            
            int prefixCount = generatedEffigy.prefixes != null ? generatedEffigy.prefixes.Count : 0;
            int suffixCount = generatedEffigy.suffixes != null ? generatedEffigy.suffixes.Count : 0;
            Debug.Log($"[EffigyGeneratorTester] Affixes: {prefixCount} prefixes, {suffixCount} suffixes");
            
            if (addToCharacterStorage)
            {
                AddEffigyToCharacterStorage(generatedEffigy);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to generate effigy. Check console for details.", "OK");
        }
    }
    
    private void AddEffigyToCharacterStorage(Effigy effigy)
    {
        // Always get fresh character reference from CharacterManager
        Character character = GetCurrentCharacter();
        
        if (character == null)
        {
            EditorUtility.DisplayDialog("Error", "No character loaded. Please load a character in Play Mode to add effigies to storage.", "OK");
            Debug.LogWarning("[EffigyGeneratorTester] No character loaded. Cannot add effigy to storage.");
            return;
        }
        
        if (character.ownedEffigies == null)
        {
            character.ownedEffigies = new List<Effigy>();
        }
        
        // Check for duplicates by name (simple check for testing purposes)
        if (character.ownedEffigies.Any(e => e != null && e.effigyName == effigy.effigyName))
        {
            Debug.LogWarning($"[EffigyGeneratorTester] Effigy '{effigy.effigyName}' already exists in character storage. Adding anyway (duplicate).");
        }
        
        character.ownedEffigies.Add(effigy);
        Debug.Log($"[EffigyGeneratorTester] Added effigy to character.ownedEffigies. Count is now: {character.ownedEffigies.Count}");
        
        // Verify the effigy was added by checking CharacterManager's character
        Character verifyChar = CharacterManager.Instance?.GetCurrentCharacter();
        if (verifyChar != null)
        {
            Debug.Log($"[EffigyGeneratorTester] Verification: CharacterManager's character has {verifyChar.ownedEffigies?.Count ?? 0} effigies");
            Debug.Log($"[EffigyGeneratorTester] Character references match: {character == verifyChar}");
        }
        
        // Refresh the effigy storage UI if it exists
        if (Application.isPlaying)
        {
            EffigyStorageUI storageUI = Object.FindObjectOfType<EffigyStorageUI>();
            if (storageUI != null)
            {
                // Force refresh by getting character again
                Character refreshChar = CharacterManager.Instance?.GetCurrentCharacter();
                if (refreshChar != null && refreshChar.ownedEffigies != null)
                {
                    Debug.Log($"[EffigyGeneratorTester] Before refresh: CharacterManager character has {refreshChar.ownedEffigies.Count} effigies");
                }
                
                storageUI.LoadEffigiesFromResources();
                Debug.Log("[EffigyGeneratorTester] Refreshed EffigyStorageUI to show new effigy");
            }
            else
            {
                Debug.LogWarning("[EffigyGeneratorTester] EffigyStorageUI not found in scene. Effigy added to character but may not appear in UI until scene reload.");
            }
        }
        
        EditorUtility.DisplayDialog("Effigy Added", $"Successfully added '{effigy.effigyName}' to {character.characterName}'s effigy storage.\nCharacter now has {character.ownedEffigies.Count} effigies.", "OK");
        Debug.Log($"[EffigyGeneratorTester] ✓ Added '{effigy.effigyName}' to {character.characterName}'s effigy storage");
        Debug.Log($"[EffigyGeneratorTester] Character now has {character.ownedEffigies.Count} effigies in storage");
        
        // Character data is managed by CharacterManager and will be saved through its system.
        // No need for EditorUtility.SetDirty for non-Unity objects.
        if (Application.isPlaying)
        {
            Debug.Log("[EffigyGeneratorTester] Character will be saved on next save cycle");
        }
        else
        {
            Debug.Log("[EffigyGeneratorTester] Note: Character changes will be saved when you enter play mode and save");
        }
    }
    
    /// <summary>
    /// Get a random blueprint from all available shapes
    /// </summary>
    private Effigy GetRandomBlueprint()
    {
        // Collect all blueprints (excluding Unknown category)
        List<Effigy> allBlueprints = new List<Effigy>();
        foreach (var kvp in effigiesByShape)
        {
            if (kvp.Key != EffigyShapeCategory.Unknown && kvp.Value.Count > 0)
            {
                allBlueprints.AddRange(kvp.Value);
            }
        }
        
        if (allBlueprints.Count == 0)
        {
            Debug.LogError("[EffigyGeneratorTester] No effigy blueprints loaded!");
            return null;
        }
        
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        Effigy selected = allBlueprints[rng.Next(allBlueprints.Count)];
        
        Debug.Log($"[EffigyGeneratorTester] Randomly selected blueprint: {selected.effigyName} (Shape: {selected.GetShapeCategory()})");
        return selected;
    }
    
    /// <summary>
    /// Get a random blueprint from a specific shape category
    /// </summary>
    private Effigy GetRandomBlueprintFromCategory(EffigyShapeCategory category)
    {
        if (!effigiesByShape.ContainsKey(category) || effigiesByShape[category].Count == 0)
        {
            Debug.LogError($"[EffigyGeneratorTester] No blueprints available for shape category: {category}");
            return null;
        }
        
        var blueprints = effigiesByShape[category];
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        Effigy selected = blueprints[rng.Next(blueprints.Count)];
        
        Debug.Log($"[EffigyGeneratorTester] Selected blueprint from {category}: {selected.effigyName}");
        return selected;
    }
}

