using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor utility to add Maximum Mana affixes to Jewellery categories.
/// Creates Tier1-Tier10 affixes with proper scaling.
/// </summary>
public class JewelleryMaxManaAffixAdder : EditorWindow
{
    private AffixDatabase_Modern database;
    private Vector2 scrollPosition;
    
    [MenuItem("Dexiled/Add Jewellery Max Mana Affixes")]
    public static void ShowWindow()
    {
        GetWindow<JewelleryMaxManaAffixAdder>("Add Jewellery Max Mana Affixes");
    }
    
    private void OnEnable()
    {
        database = Resources.Load<AffixDatabase_Modern>("AffixDatabase");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Add Maximum Mana Affixes to Jewellery", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        database = (AffixDatabase_Modern)EditorGUILayout.ObjectField(
            "Affix Database",
            database,
            typeof(AffixDatabase_Modern),
            false
        );
        
        if (database == null)
        {
            database = Resources.Load<AffixDatabase_Modern>("AffixDatabase");
            if (database == null)
            {
                EditorGUILayout.HelpBox(
                    "AffixDatabase_Modern not found! Make sure it's in Resources folder and named 'AffixDatabase.asset'",
                    MessageType.Warning
                );
                return;
            }
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This will add Maximum Mana affixes (Tier1-Tier10) to Jewellery Prefixes.\n" +
            "Tier9: 1 max mana\n" +
            "Tier1: 15-20 max mana\n" +
            "Values scale appropriately between tiers.",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add Max Mana Affixes to Jewellery", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm",
                "This will add Maximum Mana affixes to Jewellery Prefixes. Continue?",
                "Add Affixes",
                "Cancel"))
            {
                AddMaxManaAffixes();
            }
        }
        
        GUILayout.Space(10);
        
        // Show preview of what will be added
        EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        var tiers = new[]
        {
            (AffixTier.Tier10, 1f, 1f, "Mana-Touched", 1),
            (AffixTier.Tier9, 1f, 1f, "Mana-Blessed", 1),
            (AffixTier.Tier8, 2f, 3f, "Mana-Infused", 10),
            (AffixTier.Tier7, 3f, 5f, "Mana-Flowing", 15),
            (AffixTier.Tier6, 4f, 7f, "Mana-Rich", 20),
            (AffixTier.Tier5, 6f, 9f, "Mana-Weaver", 30),
            (AffixTier.Tier4, 8f, 12f, "Mana-Keeper", 40),
            (AffixTier.Tier3, 10f, 14f, "Mana-Sage", 50),
            (AffixTier.Tier2, 12f, 17f, "Mana-Master", 60),
            (AffixTier.Tier1, 15f, 20f, "Mana-Transcendent", 65)
        };
        
        foreach (var (tier, min, max, name, minLevel) in tiers)
        {
            EditorGUILayout.LabelField($"{name} ({tier}): +{min}-{max} Max Mana (Requires Level {minLevel})");
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void AddMaxManaAffixes()
    {
        if (database == null)
        {
            Debug.LogError("[JewelleryMaxManaAffixAdder] Database is null!");
            return;
        }
        
        Undo.RecordObject(database, "Add Jewellery Max Mana Affixes");
        
        // Get or create "Maximum Mana" category
        AffixCategory category = GetOrCreateCategory(database.jewelleryPrefixCategories, "Maximum Mana");
        AffixSubCategory subCategory = GetOrCreateSubCategory(category, "Maximum Mana");
        
        // Define tier progression: Tier10 (lowest) to Tier1 (highest)
        var tierData = new[]
        {
            (AffixTier.Tier10, 1f, 1f, "Mana-Touched", 1),
            (AffixTier.Tier9, 1f, 1f, "Mana-Blessed", 1),
            (AffixTier.Tier8, 2f, 3f, "Mana-Infused", 2),
            (AffixTier.Tier7, 3f, 5f, "Mana-Flowing", 3),
            (AffixTier.Tier6, 4f, 7f, "Mana-Rich", 4),
            (AffixTier.Tier5, 6f, 9f, "Mana-Weaver", 5),
            (AffixTier.Tier4, 8f, 12f, "Mana-Keeper", 8),
            (AffixTier.Tier3, 10f, 14f, "Mana-Sage", 10),
            (AffixTier.Tier2, 12f, 17f, "Mana-Master", 12),
            (AffixTier.Tier1, 15f, 20f, "Mana-Transcendent", 15)
        };
        
        int addedCount = 0;
        
        foreach (var (tier, min, max, name, minLevel) in tierData)
        {
            // Check if this affix already exists
            bool exists = subCategory.affixes.Any(a => 
                a != null && 
                a.name == name && 
                a.tier == tier &&
                a.modifiers.Any(m => m != null && m.statName == "maxMana")
            );
            
            if (exists)
            {
                Debug.Log($"[JewelleryMaxManaAffixAdder] Skipping {name} ({tier}) - already exists");
                continue;
            }
            
            // Create affix
            Affix affix = new Affix(name, $"+{min}-{max} to Maximum Mana", AffixType.Prefix, tier);
            affix.minLevel = minLevel;
            affix.weight = 100f;
            
            // Add compatible tags for jewellery
            affix.compatibleTags = new List<string> { "jewellery", "accessory", "amulet", "ring", "belt" };
            
            // Create modifier
            AffixModifier modifier = new AffixModifier(
                "maxMana",
                min,
                max,
                ModifierType.Flat,
                ModifierScope.Global
            );
            
            affix.modifiers.Add(modifier);
            subCategory.affixes.Add(affix);
            addedCount++;
            
            Debug.Log($"[JewelleryMaxManaAffixAdder] Added {name} ({tier}): +{min}-{max} Max Mana");
        }
        
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>[JewelleryMaxManaAffixAdder] Successfully added {addedCount} Maximum Mana affixes to Jewellery Prefixes!</color>");
        
        EditorUtility.DisplayDialog(
            "Success",
            $"Added {addedCount} Maximum Mana affixes to Jewellery Prefixes.\n\n" +
            $"Tier9: 1 max mana\n" +
            $"Tier1: 15-20 max mana\n" +
            $"All tiers filled!",
            "OK"
        );
    }
    
    private AffixCategory GetOrCreateCategory(List<AffixCategory> categories, string categoryName)
    {
        var category = categories.Find(c => c != null && c.categoryName == categoryName);
        if (category == null)
        {
            category = new AffixCategory(categoryName);
            categories.Add(category);
        }
        return category;
    }
    
    private AffixSubCategory GetOrCreateSubCategory(AffixCategory category, string subCategoryName)
    {
        var subCategory = category.subCategories.Find(s => s != null && s.subCategoryName == subCategoryName);
        if (subCategory == null)
        {
            subCategory = new AffixSubCategory(subCategoryName);
            category.subCategories.Add(subCategory);
        }
        return subCategory;
    }
}

