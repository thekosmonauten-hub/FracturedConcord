using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to generate sample embossing effects based on the design document
/// </summary>
public class EmbossingSampleGenerator : EditorWindow
{
    [MenuItem("Tools/Card System/Generate Sample Embossings")]
    public static void GenerateSampleEmbossings()
    {
        string folderPath = "Assets/Resources/Embossings";
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        int count = 0;
        
        // 🔥 DAMAGE EMBOSSINGS
        count += CreateEmbossing("of Ferocity", 
            "+35% more Physical Damage\nDeal no Elemental damage", 
            EmbossingCategory.Damage, EmbossingRarity.Rare,
            0.35f, EmbossingEffectType.PhysicalDamageMultiplier, 0.35f,
            10, 50, 0, 0);
            
        count += CreateEmbossing("of the Inferno", 
            "Converts 50% Physical to Fire Damage", 
            EmbossingCategory.Conversion, EmbossingRarity.Uncommon,
            0.25f, EmbossingEffectType.PhysicalToFireConversion, 0.5f,
            5, 30, 0, 0);
            
        count += CreateEmbossing("of Amplification", 
            "+25% more Spell Damage", 
            EmbossingCategory.Damage, EmbossingRarity.Common,
            0.25f, EmbossingEffectType.SpellDamageMultiplier, 0.25f,
            3, 0, 0, 40);
            
        count += CreateEmbossing("of Momentum", 
            "+7% more damage per hit this turn", 
            EmbossingCategory.Damage, EmbossingRarity.Uncommon,
            0.30f, EmbossingEffectType.ComboScaling, 0.07f,
            8, 35, 0, 0);
            
        count += CreateEmbossing("of Impact", 
            "+1 AoE radius (hits one additional enemy)", 
            EmbossingCategory.Damage, EmbossingRarity.Rare,
            0.30f, EmbossingEffectType.FlatDamageBonus, 1f,
            12, 45, 0, 0);
            
        count += CreateEmbossing("of Annihilation", 
            "15% chance to deal double damage", 
            EmbossingCategory.Damage, EmbossingRarity.Rare,
            0.25f, EmbossingEffectType.ConditionalDamage, 2f,
            10, 40, 0, 0, 0.15f);
        
        // 🧠 SCALING EMBOSSINGS
        count += CreateEmbossing("of Focus", 
            "+15% scaling from primary stat", 
            EmbossingCategory.Scaling, EmbossingRarity.Common,
            0.20f, EmbossingEffectType.DamageMultiplier, 0.15f,
            5, 0, 0, 0);
            
        count += CreateEmbossing("of Efficiency", 
            "-20% mana cost", 
            EmbossingCategory.Scaling, EmbossingRarity.Uncommon,
            0.15f, EmbossingEffectType.ManaCostReduction, 0.20f,
            6, 0, 0, 30);
            
        count += CreateEmbossing("of Power", 
            "+3% more damage per 50 Strength", 
            EmbossingCategory.Scaling, EmbossingRarity.Uncommon,
            0.20f, EmbossingEffectType.StrengthScaling, 0.03f,
            8, 40, 0, 0);
            
        count += CreateEmbossing("of Precision", 
            "+1% crit chance per 50 Dexterity", 
            EmbossingCategory.Scaling, EmbossingRarity.Uncommon,
            0.20f, EmbossingEffectType.CriticalChance, 0.01f,
            8, 0, 40, 0);
            
        count += CreateEmbossing("of Calculation", 
            "+10% increased Card effect per 50 Intelligence", 
            EmbossingCategory.Scaling, EmbossingRarity.Uncommon,
            0.20f, EmbossingEffectType.IntelligenceScaling, 0.10f,
            8, 0, 0, 40);
        
        // 💎 UTILITY EMBOSSINGS
        count += CreateEmbossing("of Recovery", 
            "Gain 3 Life on Hit", 
            EmbossingCategory.Utility, EmbossingRarity.Common,
            0.20f, EmbossingEffectType.LifeOnHit, 3f,
            3, 0, 0, 0);
            
        count += CreateEmbossing("of Leeching", 
            "10% of damage dealt restored as Life", 
            EmbossingCategory.Utility, EmbossingRarity.Uncommon,
            0.25f, EmbossingEffectType.LifeLeech, 0.10f,
            8, 0, 0, 0);
            
        count += CreateEmbossing("of Channeling", 
            "If cast consecutively, +20% more spell damage per cast\nResets when another skill is used", 
            EmbossingCategory.Utility, EmbossingRarity.Rare,
            0.35f, EmbossingEffectType.CustomEffect, 0.20f,
            15, 0, 0, 50);
            
        count += CreateEmbossing("of the Echo", 
            "Repeat the card once at 50% damage", 
            EmbossingCategory.Utility, EmbossingRarity.Epic,
            0.40f, EmbossingEffectType.CardDuplication, 0.50f,
            18, 0, 0, 60);
            
        count += CreateEmbossing("of Preparation", 
            "If prepared for 2 turns, double its effect", 
            EmbossingCategory.Utility, EmbossingRarity.Rare,
            0.30f, EmbossingEffectType.ConditionalDamage, 2f,
            12, 0, 0, 45);
        
        // 🛡️ DEFENSIVE EMBOSSINGS
        count += CreateEmbossing("of Endurance", 
            "Gain +1 Guard when played", 
            EmbossingCategory.Defensive, EmbossingRarity.Common,
            0.20f, EmbossingEffectType.GuardOnPlay, 1f,
            3, 30, 0, 0);
            
        count += CreateEmbossing("of Fortification", 
            "Gain Bolster stacks on hit", 
            EmbossingCategory.Defensive, EmbossingRarity.Uncommon,
            0.25f, EmbossingEffectType.CustomEffect, 1f,
            10, 45, 0, 0);
            
        count += CreateEmbossing("of the Bastion", 
            "+20% Guard effectiveness", 
            EmbossingCategory.Defensive, EmbossingRarity.Rare,
            0.30f, EmbossingEffectType.GuardEffectiveness, 0.20f,
            15, 50, 0, 0);
            
        count += CreateEmbossing("of Reflection", 
            "Reflect 10% of damage taken", 
            EmbossingCategory.Defensive, EmbossingRarity.Uncommon,
            0.25f, EmbossingEffectType.DamageReflection, 0.10f,
            8, 35, 0, 0);
        
        // ⚡ COMBO EMBOSSINGS
        count += CreateEmbossing("of the Gambit", 
            "If discarded, returns next turn with +50% damage", 
            EmbossingCategory.Combo, EmbossingRarity.Rare,
            0.30f, EmbossingEffectType.CustomEffect, 0.50f,
            12, 0, 40, 0);
            
        count += CreateEmbossing("of Flow", 
            "+15% more damage per previous skill played", 
            EmbossingCategory.Combo, EmbossingRarity.Uncommon,
            0.25f, EmbossingEffectType.ComboScaling, 0.15f,
            10, 0, 35, 0);
            
        count += CreateEmbossing("of Crescendo", 
            "Each repeat increases damage by 25%, resets on turn end", 
            EmbossingCategory.Combo, EmbossingRarity.Rare,
            0.35f, EmbossingEffectType.ComboScaling, 0.25f,
            14, 0, 45, 0);
        
        // 🌌 AILMENT EMBOSSINGS
        count += CreateEmbossing("of Cruelty", 
            "15% chance to Bleed on hit", 
            EmbossingCategory.Ailment, EmbossingRarity.Common,
            0.20f, EmbossingEffectType.ApplyBleed, 0.15f,
            5, 25, 0, 0);
            
        count += CreateEmbossing("of Immolation", 
            "15% chance to Ignite on hit", 
            EmbossingCategory.Ailment, EmbossingRarity.Common,
            0.20f, EmbossingEffectType.ApplyIgnite, 0.15f,
            5, 0, 0, 25);
            
        count += CreateEmbossing("of Sepsis", 
            "15% chance to Poison on hit", 
            EmbossingCategory.Ailment, EmbossingRarity.Common,
            0.20f, EmbossingEffectType.ApplyPoison, 0.15f,
            5, 0, 30, 0);
            
        count += CreateEmbossing("of Conduction", 
            "15% chance to Shock on hit", 
            EmbossingCategory.Ailment, EmbossingRarity.Common,
            0.20f, EmbossingEffectType.ApplyShock, 0.15f,
            5, 0, 0, 30);
            
        count += CreateEmbossing("of Brittle Frost", 
            "15% chance to Chill or Freeze", 
            EmbossingCategory.Ailment, EmbossingRarity.Uncommon,
            0.25f, EmbossingEffectType.ApplyFreeze, 0.15f,
            8, 0, 0, 35);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[EmbossingSampleGenerator] Created {count} sample embossings in {folderPath}");
        EditorUtility.DisplayDialog("Embossings Created", $"Successfully created {count} sample embossings!", "OK");
    }
    
    private static int CreateEmbossing(string name, string desc, EmbossingCategory category, EmbossingRarity rarity,
        float manaMult, EmbossingEffectType effectType, float effectValue,
        int minLevel = 1, int minStr = 0, int minDex = 0, int minInt = 0, float secondaryValue = 0f)
    {
        string folderPath = "Assets/Resources/Embossings";
        string fileName = name.Replace(" ", "_").Replace("of_", "").Replace("the_", "");
        string assetPath = $"{folderPath}/{fileName}.asset";
        
        // Skip if already exists
        if (File.Exists(assetPath))
        {
            Debug.Log($"[EmbossingSampleGenerator] Skipping existing: {name}");
            return 0;
        }
        
        EmbossingEffect embossing = ScriptableObject.CreateInstance<EmbossingEffect>();
        embossing.embossingName = name;
        embossing.description = desc;
        embossing.category = category;
        embossing.rarity = rarity;
        embossing.manaCostMultiplier = manaMult;
        embossing.effectType = effectType;
        embossing.effectValue = effectValue;
        embossing.secondaryEffectValue = secondaryValue;
        embossing.minimumLevel = minLevel;
        embossing.minimumStrength = minStr;
        embossing.minimumDexterity = minDex;
        embossing.minimumIntelligence = minInt;
        
        AssetDatabase.CreateAsset(embossing, assetPath);
        Debug.Log($"[EmbossingSampleGenerator] Created: {name}");
        
        return 1;
    }
}

