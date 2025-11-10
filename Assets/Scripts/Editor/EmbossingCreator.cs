using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to quickly create embossing effects
/// </summary>
public class EmbossingCreator : EditorWindow
{
    private string embossingName = "of Unnamed";
    private string description = "";
    private EmbossingCategory category = EmbossingCategory.Damage;
    private EmbossingRarity rarity = EmbossingRarity.Common;
    private float manaCostMultiplier = 0.25f;
    private int minimumLevel = 1;
    private int minimumStrength = 0;
    private int minimumDexterity = 0;
    private int minimumIntelligence = 0;
    private EmbossingEffectType effectType = EmbossingEffectType.DamageMultiplier;
    private float effectValue = 0f;
    
    [MenuItem("Tools/Card System/Create Embossing")]
    public static void ShowWindow()
    {
        GetWindow<EmbossingCreator>("Create Embossing");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Create New Embossing", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        embossingName = EditorGUILayout.TextField("Name", embossingName);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
        
        EditorGUILayout.Space();
        category = (EmbossingCategory)EditorGUILayout.EnumPopup("Category", category);
        rarity = (EmbossingRarity)EditorGUILayout.EnumPopup("Rarity", rarity);
        
        EditorGUILayout.Space();
        GUILayout.Label("Mana Cost", EditorStyles.boldLabel);
        manaCostMultiplier = EditorGUILayout.Slider("Multiplier", manaCostMultiplier, 0f, 2f);
        EditorGUILayout.LabelField("", $"+{(manaCostMultiplier * 100):F0}% mana cost");
        
        EditorGUILayout.Space();
        GUILayout.Label("Requirements", EditorStyles.boldLabel);
        minimumLevel = EditorGUILayout.IntField("Min Level", minimumLevel);
        minimumStrength = EditorGUILayout.IntField("Min Strength", minimumStrength);
        minimumDexterity = EditorGUILayout.IntField("Min Dexterity", minimumDexterity);
        minimumIntelligence = EditorGUILayout.IntField("Min Intelligence", minimumIntelligence);
        
        EditorGUILayout.Space();
        GUILayout.Label("Effect", EditorStyles.boldLabel);
        effectType = (EmbossingEffectType)EditorGUILayout.EnumPopup("Effect Type", effectType);
        effectValue = EditorGUILayout.FloatField("Effect Value", effectValue);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Embossing", GUILayout.Height(40)))
        {
            CreateEmbossing();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Embossings will be created in Resources/Embossings/", MessageType.Info);
    }
    
    void CreateEmbossing()
    {
        string folderPath = "Assets/Resources/Embossings";
        
        // Create folder if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        // Create the embossing asset
        EmbossingEffect embossing = ScriptableObject.CreateInstance<EmbossingEffect>();
        embossing.embossingName = embossingName;
        embossing.description = description;
        embossing.category = category;
        embossing.rarity = rarity;
        embossing.manaCostMultiplier = manaCostMultiplier;
        embossing.minimumLevel = minimumLevel;
        embossing.minimumStrength = minimumStrength;
        embossing.minimumDexterity = minimumDexterity;
        embossing.minimumIntelligence = minimumIntelligence;
        embossing.effectType = effectType;
        embossing.effectValue = effectValue;
        
        // Generate safe file name
        string fileName = embossingName.Replace(" ", "_").Replace("of_", "");
        string assetPath = $"{folderPath}/{fileName}.asset";
        
        // Create asset
        AssetDatabase.CreateAsset(embossing, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = embossing;
        
        Debug.Log($"Created embossing: {embossingName} at {assetPath}");
    }
}

