using UnityEngine;
using UnityEditor;
using Dexiled.Data.Status;

/// <summary>
/// Editor tool to populate StatusDatabase with all status effect entries
/// </summary>
public class StatusDatabasePopulator : EditorWindow
{
    private StatusDatabase database;
    private Vector2 scrollPosition;
    
    [MenuItem("Dexiled/Populate Status Database")]
    public static void ShowWindow()
    {
        GetWindow<StatusDatabasePopulator>("Status Database Populator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Status Database Populator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Load database
        database = EditorGUILayout.ObjectField("Status Database", database, typeof(StatusDatabase), false) as StatusDatabase;
        
        if (database == null)
        {
            database = Resources.Load<StatusDatabase>("StatusDatabase");
            if (database == null)
            {
                EditorGUILayout.HelpBox("StatusDatabase not found in Resources. Please create one first.", MessageType.Warning);
                if (GUILayout.Button("Create Status Database"))
                {
                    CreateStatusDatabase();
                }
                return;
            }
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Populate All Status Effects", GUILayout.Height(30)))
        {
            PopulateDatabase();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Clear All Entries", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear All Entries", "Are you sure you want to clear all entries?", "Yes", "No"))
            {
                database.statusEffects.Clear();
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
            }
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("This will add/update entries for all status effect types. Existing entries will be updated if they match the effect type.", MessageType.Info);
    }
    
    private void CreateStatusDatabase()
    {
        StatusDatabase newDb = ScriptableObject.CreateInstance<StatusDatabase>();
        string path = "Assets/Resources/StatusDatabase.asset";
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        AssetDatabase.CreateAsset(newDb, path);
        AssetDatabase.SaveAssets();
        database = newDb;
        Debug.Log($"Created StatusDatabase at {path}");
    }
    
    private void PopulateDatabase()
    {
        if (database == null) return;
        
        // Clear existing entries
        database.statusEffects.Clear();
        
        // Add all status effect entries
        AddStatusEffect(StatusEffectType.Poison, "Poison", 
            "Deals chaos damage over time. 30% of (physical + chaos) damage per turn. Stacks independently. Base duration: 2 turns.",
            "Poison", new Color(0f, 0.8f, 0f), true, 0f, 2, 1f, DamageType.Chaos,
            "Poison causes the affected target to take chaos damage over time, at 30% of the combined physical and chaos damage of the hit that applied it per turn. Poison can stack multiple times on a single target.");
        
        AddStatusEffect(StatusEffectType.Burn, "Ignite",
            "Deals fire damage over time. 90% of fire damage per turn. Base duration: 4 turns.",
            "Ignite", Color.red, true, 0f, 4, 1f, DamageType.Fire,
            "Ignite causes the affected target to burn for 90% of the fire damage of the hit that applied it per turn. The base duration is 4 turns.");
        
        AddStatusEffect(StatusEffectType.Bleed, "Bleeding",
            "Deals physical damage over time. 70% of physical damage per turn. Only one instance can be active. Base duration: 5 turns.",
            "Bleed", new Color(0.7f, 0f, 0f), true, 0f, 5, 1f, DamageType.Physical,
            "Bleeding can only be inflicted by physical damage from attacks. Bleeding enemies take physical damage over time, at 70% of the physical damage of the hit that inflicted bleeding per turn, only one instance of bleeding can be active at once, but if a higher bleed is applied, the duration refreshes and the new bleed is applied. The base duration is 5 turns.");
        
        AddStatusEffect(StatusEffectType.ChaosDot, "Chaos DoT",
            "Deals chaos damage over time. Different from Poison, but both scale with Chaos damage.",
            "ChaosDot", new Color(0.5f, 0.2f, 0.6f), true, 0f, 3, 1f, DamageType.Chaos,
            "Chaos damage over time effect is applied by Chaos cards that specifically say they deal Chaos damage over time. This is different from Poison damage, but both scale with Chaos damage.");
        
        AddStatusEffect(StatusEffectType.Chill, "Chilled",
            "Slows energy gain up to 30% based on cold damage. Base duration: 2 turns.",
            "Chilled", new Color(0.6f, 0.9f, 1f), true, 0f, 2, 0f, DamageType.Cold,
            "Cold damage always inflicts chill. Chill slows all energy gain of the affected target up to 30%, based on the cold damage of the hit. The base duration is 2 turns.");
        
        AddStatusEffect(StatusEffectType.Freeze, "Frozen",
            "Prevents actions. Duration: 1-2 turns based on cold damage as percentage of max HP (2 turns if >= 10% of max HP).",
            "Chilled", Color.cyan, true, 0f, 1, 0f, DamageType.Cold,
            "Freeze prevents the affected target from taking actions for a duration based on the cold damage of the hit. The base minimum duration is 1 Turn and the base maximum duration is 2 Turns.");
        
        AddStatusEffect(StatusEffectType.Shocked, "Shocked",
            "Increases damage taken up to 50% based on lightning damage. Base duration: 2 turns.",
            "Shocked", new Color(1f, 0.8f, 0.2f), true, 0f, 2, 0f, DamageType.Lightning,
            "Shock causes the affected target to take up to 50% increased damage from all sources, based on the lightning damage of the hit. The base duration is 2 turns.");
        
        AddStatusEffect(StatusEffectType.Stun, "Stunned",
            "Cannot act for the duration. Base duration: 1 turn.",
            "Stunned", Color.yellow, true, 0f, 1, 0f, DamageType.Physical,
            "Stunned target cannot act for the duration of the stun, base duration is 1 turn.");
        
        AddStatusEffect(StatusEffectType.Stagger, "Stagger",
            "Cannot act for the duration. Base duration: 1 turn.",
            "Stunned", Color.yellow, true, 0f, 1, 0f, DamageType.Physical,
            "Stunned target cannot act for the duration of the stun, base duration is 1 turn.");
        
        AddStatusEffect(StatusEffectType.Vulnerable, "Vulnerability",
            "Target takes 20% more damage for the next instance of damage. Consumed after one damage instance.",
            "Vulnerability", Color.magenta, true, 0f, 1, 0f, DamageType.Physical,
            "Target affected by Vulnerability takes 20% more damage for the next instance of damage.");
        
        AddStatusEffect(StatusEffectType.Crumble, "Crumble",
            "Stores X% amount of Physical damage taken as a debuff. When the crumble debuff runs out, take that amount of damage.",
            "Crumble", new Color(0.6f, 0.5f, 0.8f), true, 0f, 5, 0f, DamageType.Physical,
            "Store X% amount of Physical damage taken as a debuff, when the crumble debuff runs out, take that amount of damage.");
        
        AddStatusEffect(StatusEffectType.Bolster, "Bolster",
            "Each instance reduces damage taken by 2% up to 20% at 10 stacks.",
            "Bolster", new Color(0.4f, 0.8f, 0.6f), false, 1f, 3, 0f, DamageType.Physical,
            "Each instance of Bolster reduces the damage taken by 2% up to 20% at 10 stacks.");
        
        AddStatusEffect(StatusEffectType.Strength, "TempStrength",
            "Increases or decreases the Strength stat for a duration or rest of combat.",
            "TempStrength", Color.red, false, 1f, 3, 0f, DamageType.Physical,
            "Increased or decreases the Strength stat for a duration or rest of combat.", true);
        
        AddStatusEffect(StatusEffectType.Dexterity, "TempDexterity",
            "Increases or decreases the Dexterity stat for a duration or rest of combat.",
            "TempAgility", Color.green, false, 1f, 3, 0f, DamageType.Physical,
            "Increased or decreases the Dexterity stat for a duration or rest of combat.", true);
        
        AddStatusEffect(StatusEffectType.Intelligence, "TempIntelligence",
            "Increases or decreases the Intelligence stat for a duration or rest of combat.",
            "TempIntelligence", Color.blue, false, 1f, 3, 0f, DamageType.Physical,
            "Increased or decreases the Intelligence stat for a duration or rest of combat.", true);
        
        AddStatusEffect(StatusEffectType.Shield, "Shielded",
            "Absorbs damage before health is reduced.",
            "Shielded", new Color(0.5f, 0.8f, 1f), false, 10f, 3, 0f, DamageType.Physical,
            "Shield absorbs damage before health is reduced.");
        
        AddStatusEffect(StatusEffectType.TempMaxMana, "TempMaxMana",
            "Temporarily increases maximum mana.",
            "TempMana", new Color(0.3f, 0.6f, 1f), false, 1f, 3, 0f, DamageType.Physical,
            "Temporarily increases maximum mana.");
        
        AddStatusEffect(StatusEffectType.TempEvasion, "TempEvasion",
            "Temporarily increases evasion.",
            "TempEvasion", new Color(0.3f, 0.85f, 0.6f), false, 10f, 3, 0f, DamageType.Physical,
            "Temporarily increases evasion.");
        
        AddStatusEffect(StatusEffectType.SpellPower, "SpellPower",
            "Increases spell damage multiplier.",
            "SpellPower", new Color(0.8f, 0.4f, 1f), false, 1f, 3, 0f, DamageType.Physical,
            "Increases spell damage multiplier.");
        
        // Save the asset
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"StatusDatabase populated with {database.statusEffects.Count} entries!");
        EditorUtility.DisplayDialog("Success", $"StatusDatabase populated with {database.statusEffects.Count} entries!\n\nRemember to assign sprites from your sprite sheet to each entry.", "OK");
    }
    
    private void AddStatusEffect(StatusEffectType type, string name, string description, 
        string iconName, Color color, bool isDebuff, float defaultMagnitude, int defaultDuration, 
        float tickInterval, DamageType damageType, string tooltip = "", bool usePositiveNegative = false)
    {
        StatusEffectData data = new StatusEffectData
        {
            effectType = type,
            effectName = name,
            description = description,
            effectColor = color,
            isDebuff = isDebuff,
            defaultMagnitude = defaultMagnitude,
            defaultDuration = defaultDuration,
            tickInterval = tickInterval,
            damageType = damageType,
            tooltipDescription = string.IsNullOrEmpty(tooltip) ? description : tooltip,
            usePositiveNegativeIcons = usePositiveNegative
        };
        
        // Try to load default sprite from Resources
        string spritePath = $"StatusEffectIcons/{iconName}";
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
        {
            data.iconSprite = sprite;
        }
        
        // If using positive/negative icons, try to load them
        if (usePositiveNegative)
        {
            string positivePath = $"StatusEffectIcons/{iconName}Positive";
            Sprite positiveSprite = Resources.Load<Sprite>(positivePath);
            if (positiveSprite != null)
            {
                data.positiveIconSprite = positiveSprite;
            }
            
            string negativePath = $"StatusEffectIcons/{iconName}Negative";
            Sprite negativeSprite = Resources.Load<Sprite>(negativePath);
            if (negativeSprite != null)
            {
                data.negativeIconSprite = negativeSprite;
            }
        }
        
        database.statusEffects.Add(data);
    }
}

