using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to create MonsterModifier assets quickly
/// </summary>
public class MonsterModifierCreator : EditorWindow
{
    private string modifierName = "New Modifier";
    private string description = "Modifier description";
    private Color modifierColor = Color.yellow;
    
    // Stat modifications
    private float healthMultiplier = 0f;
    private float damageMultiplier = 0f;
    private float accuracyMultiplier = 0f;
    private float evasionMultiplier = 0f;
    private float critChanceMultiplier = 0f;
    private float critMultiplierMultiplier = 0f;
    
    // Resistances
    private float physicalResistance = 0f;
    private float fireResistance = 0f;
    private float coldResistance = 0f;
    private float lightningResistance = 0f;
    private float chaosResistance = 0f;
    
    // Special effects
    private bool regeneratesHealth = false;
    private float healthRegenPerTurn = 0f;
    private bool isHasted = false;
    private bool isArmored = false;
    private bool reflectsDamage = false;
    private float reflectPercentage = 0f;
    
    private Vector2 scrollPosition;
    
    [MenuItem("Dexiled/Create Monster Modifier")]
    public static void ShowWindow()
    {
        GetWindow<MonsterModifierCreator>("Monster Modifier Creator");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Monster Modifier Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        modifierName = EditorGUILayout.TextField("Modifier Name", modifierName);
        description = EditorGUILayout.TextField("Description", description);
        modifierColor = EditorGUILayout.ColorField("Modifier Color", modifierColor);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stat Modifications (%)", EditorStyles.boldLabel);
        healthMultiplier = EditorGUILayout.FloatField("Health Multiplier", healthMultiplier);
        damageMultiplier = EditorGUILayout.FloatField("Damage Multiplier", damageMultiplier);
        accuracyMultiplier = EditorGUILayout.FloatField("Accuracy Multiplier", accuracyMultiplier);
        evasionMultiplier = EditorGUILayout.FloatField("Evasion Multiplier", evasionMultiplier);
        critChanceMultiplier = EditorGUILayout.FloatField("Crit Chance Multiplier", critChanceMultiplier);
        critMultiplierMultiplier = EditorGUILayout.FloatField("Crit Multiplier Multiplier", critMultiplierMultiplier);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Resistances (%)", EditorStyles.boldLabel);
        physicalResistance = EditorGUILayout.FloatField("Physical Resistance", physicalResistance);
        fireResistance = EditorGUILayout.FloatField("Fire Resistance", fireResistance);
        coldResistance = EditorGUILayout.FloatField("Cold Resistance", coldResistance);
        lightningResistance = EditorGUILayout.FloatField("Lightning Resistance", lightningResistance);
        chaosResistance = EditorGUILayout.FloatField("Chaos Resistance", chaosResistance);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Special Effects", EditorStyles.boldLabel);
        regeneratesHealth = EditorGUILayout.Toggle("Regenerates Health", regeneratesHealth);
        if (regeneratesHealth)
        {
            healthRegenPerTurn = EditorGUILayout.FloatField("Health Regen Per Turn", healthRegenPerTurn);
        }
        isHasted = EditorGUILayout.Toggle("Is Hasted", isHasted);
        isArmored = EditorGUILayout.Toggle("Is Armored", isArmored);
        reflectsDamage = EditorGUILayout.Toggle("Reflects Damage", reflectsDamage);
        if (reflectsDamage)
        {
            reflectPercentage = EditorGUILayout.FloatField("Reflect Percentage", reflectPercentage);
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Modifier Asset", GUILayout.Height(30)))
        {
            CreateModifierAsset();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Common Modifier Presets:", MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Hasted"))
        {
            SetPresetHasted();
        }
        if (GUILayout.Button("Armored"))
        {
            SetPresetArmored();
        }
        if (GUILayout.Button("Resistant"))
        {
            SetPresetResistant();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CreateModifierAsset()
    {
        if (string.IsNullOrEmpty(modifierName))
        {
            EditorUtility.DisplayDialog("Error", "Modifier name cannot be empty!", "OK");
            return;
        }
        
        MonsterModifier modifier = ScriptableObject.CreateInstance<MonsterModifier>();
        modifier.modifierName = modifierName;
        modifier.description = description;
        modifier.modifierColor = modifierColor;
        
        modifier.healthMultiplier = healthMultiplier;
        modifier.damageMultiplier = damageMultiplier;
        modifier.accuracyMultiplier = accuracyMultiplier;
        modifier.evasionMultiplier = evasionMultiplier;
        modifier.critChanceMultiplier = critChanceMultiplier;
        modifier.critMultiplierMultiplier = critMultiplierMultiplier;
        
        modifier.physicalResistance = physicalResistance;
        modifier.fireResistance = fireResistance;
        modifier.coldResistance = coldResistance;
        modifier.lightningResistance = lightningResistance;
        modifier.chaosResistance = chaosResistance;
        
        modifier.regeneratesHealth = regeneratesHealth;
        modifier.healthRegenPerTurn = healthRegenPerTurn;
        modifier.isHasted = isHasted;
        modifier.isArmored = isArmored;
        modifier.reflectsDamage = reflectsDamage;
        modifier.reflectPercentage = reflectPercentage;
        
        string path = "Assets/Resources/MonsterModifiers";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        string assetPath = $"{path}/{modifierName}.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(modifier, assetPath);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = modifier;
        
        Debug.Log($"Created MonsterModifier: {assetPath}");
    }
    
    private void SetPresetHasted()
    {
        modifierName = "Hasted";
        description = "This enemy moves and acts faster.";
        modifierColor = Color.cyan;
        isHasted = true;
        accuracyMultiplier = 20f;
        evasionMultiplier = 15f;
    }
    
    private void SetPresetArmored()
    {
        modifierName = "Armored";
        description = "This enemy has increased defenses.";
        modifierColor = Color.gray;
        isArmored = true;
        healthMultiplier = 30f;
        physicalResistance = 25f;
    }
    
    private void SetPresetResistant()
    {
        modifierName = "Resistant";
        description = "This enemy resists elemental damage.";
        modifierColor = Color.magenta;
        fireResistance = 30f;
        coldResistance = 30f;
        lightningResistance = 30f;
    }
}

