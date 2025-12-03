using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CreateAct1Enemies : EditorWindow
{
    [MenuItem("Dexiled/Create Act 1 Enemies")]
    public static void ShowWindow()
    {
        GetWindow<CreateAct1Enemies>("Create Act 1 Enemies");
    }

    private void OnGUI()
    {
        GUILayout.Label("Act 1 Enemy Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create All Act 1 Enemies", GUILayout.Height(30)))
        {
            CreateAllEnemies();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("This will create 30 enemy assets organized by area. " +
            "Each enemy will have appropriate stats and flavorful abilities matching their descriptions.", MessageType.Info);
    }

    private void CreateAllEnemies()
    {
        string basePath = "Assets/Resources/Enemies/Act1";
        
        // Ensure base directory exists
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Enemies", "Act1");
        }

        // Define all enemies with their data
        var enemies = GetEnemyDefinitions();
        
        int created = 0;
        foreach (var enemyDef in enemies)
        {
            CreateEnemyAsset(enemyDef, basePath);
            created++;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Created {created} Act 1 enemy assets!");
        EditorUtility.DisplayDialog("Success", $"Created {created} enemy assets in {basePath}", "OK");
    }

    private List<EnemyDefinition> GetEnemyDefinitions()
    {
        return new List<EnemyDefinition>
        {
            // Area 1: Where Night First Fell
            new EnemyDefinition
            {
                name = "Gloom-Touched Peasant",
                description = "A once-normal villager with faint dark veins glowing under the skin. Moves erratically and swings tools with unnatural force.",
                area = "WhereNightFirstFell",
                healthMin = 35, healthMax = 45,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Erratic Swing",
                abilityDescription = "Attacks with unpredictable force, dealing extra damage"
            },
            new EnemyDefinition
            {
                name = "Shuddering Crow",
                description = "A crow with patchy feathers and one luminous eye. Its wings jitter unnaturally as if flickering between moments.",
                area = "WhereNightFirstFell",
                healthMin = 25, healthMax = 35,
                damage = 5,
                category = EnemyCategory.Ranged,
                aiPattern = EnemyAIPattern.Balanced,
                abilityName = "Temporal Flicker",
                abilityDescription = "Gains evasion when damaged"
            },
            
            // Area 3: Whispering Orchard
            new EnemyDefinition
            {
                name = "Rotblush Picker",
                description = "A farmhand-like figure with sap-stained hands and swollen fruitlike growths on their arms.",
                area = "WhisperingOrchard",
                healthMin = 40, healthMax = 50,
                damage = 7,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Sap Strike",
                abilityDescription = "Applies Slow on attack"
            },
            new EnemyDefinition
            {
                name = "Sap-Drained Husk",
                description = "A thin, bark-textured humanoid whose skin is drying and cracking as if becoming part tree.",
                area = "WhisperingOrchard",
                healthMin = 30, healthMax = 40,
                damage = 5,
                category = EnemyCategory.Tank,
                aiPattern = EnemyAIPattern.Defensive,
                abilityName = "Bark Armor",
                abilityDescription = "Gains guard when defending"
            },
            
            // Area 4: The Hollow Grainfields
            new EnemyDefinition
            {
                name = "Wheat-Stalker Hound",
                description = "A lean hunting dog with stalks of grain growing from its back, eyes pale and distant.",
                area = "HollowGrainfields",
                healthMin = 35, healthMax = 45,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Pack Hunter",
                abilityDescription = "Deals more damage if other enemies are present"
            },
            new EnemyDefinition
            {
                name = "Grainwisp Vermin",
                description = "Small field rodents trailing drifting golden dust; their bodies twitch with erratic energy.",
                area = "HollowGrainfields",
                healthMin = 20, healthMax = 30,
                damage = 4,
                category = EnemyCategory.Swarm,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Dust Cloud",
                abilityDescription = "Applies Blind on attack"
            },
            
            // Area 5: The Splintered Bridge
            new EnemyDefinition
            {
                name = "Bridge-Gnawer",
                description = "A humanoid with splintered wooden shards protruding from their limbs, as if fused to the collapsing bridge.",
                area = "SplinteredBridge",
                healthMin = 40, healthMax = 50,
                damage = 7,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Splinter Burst",
                abilityDescription = "Deals damage to player when taking damage"
            },
            new EnemyDefinition
            {
                name = "River Siltling",
                description = "Small mud-coated creatures leaving trails of wet silt, with faint glowing eyes beneath the muck.",
                area = "SplinteredBridge",
                healthMin = 25, healthMax = 35,
                damage = 5,
                category = EnemyCategory.Swarm,
                aiPattern = EnemyAIPattern.Balanced,
                abilityName = "Mud Trap",
                abilityDescription = "Applies Slow when defending"
            },
            
            // Area 6: Rotfall Creek
            new EnemyDefinition
            {
                name = "Creek-Swollen Corpse",
                description = "A waterlogged undead that bloats and leaks murky creekwater with each step.",
                area = "RotfallCreek",
                healthMin = 45, healthMax = 55,
                damage = 6,
                category = EnemyCategory.Tank,
                aiPattern = EnemyAIPattern.Defensive,
                abilityName = "Bloat",
                abilityDescription = "Gains max health when damaged"
            },
            new EnemyDefinition
            {
                name = "Reeking Marshspawn",
                description = "Amphibious pests formed from mud and decayed plant matter, hopping and spitting filth.",
                area = "RotfallCreek",
                healthMin = 30, healthMax = 40,
                damage = 5,
                category = EnemyCategory.Swarm,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Filth Spit",
                abilityDescription = "Applies Poison on attack"
            },
            
            // Area 7: The Sighing Woods
            new EnemyDefinition
            {
                name = "Wind-Swayed Shade",
                description = "Vague humanoid shapes made of drifting mist and old cloth scraps caught on branches.",
                area = "SighingWoods",
                healthMin = 30, healthMax = 40,
                damage = 5,
                category = EnemyCategory.Caster,
                aiPattern = EnemyAIPattern.Tactical,
                abilityName = "Mist Form",
                abilityDescription = "Gains dodge chance when damaged"
            },
            new EnemyDefinition
            {
                name = "Bramble Skitterer",
                description = "A small forest beast with thorny brambles tangled in its fur.",
                area = "SighingWoods",
                healthMin = 35, healthMax = 45,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Thorny Defense",
                abilityDescription = "Deals damage to attacker when taking damage"
            },
            
            // Area 8: Woe-Milestone Pass
            new EnemyDefinition
            {
                name = "Stone-Loom Walker",
                description = "A humanoid with patches of rough stone crust forming on shoulders and arms.",
                area = "WoeMilestonePass",
                healthMin = 40, healthMax = 50,
                damage = 6,
                category = EnemyCategory.Tank,
                aiPattern = EnemyAIPattern.Defensive,
                abilityName = "Stone Skin",
                abilityDescription = "Gains physical resistance"
            },
            new EnemyDefinition
            {
                name = "Gravel-Toothed Scavenger",
                description = "A hunched creature that gnashes with stone-like teeth and spits pebbles.",
                area = "WoeMilestonePass",
                healthMin = 30, healthMax = 40,
                damage = 5,
                category = EnemyCategory.Ranged,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Pebble Spit",
                abilityDescription = "Ranged attack that applies Stagger"
            },
            
            // Area 9: Blight-Tilled Meadow
            new EnemyDefinition
            {
                name = "Meadow Blightsow",
                description = "A corrupted boar with fungal blooms on its back and pale spores puffing with each step.",
                area = "BlightTilledMeadow",
                healthMin = 45, healthMax = 55,
                damage = 7,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Spore Cloud",
                abilityDescription = "Applies Poison to player on attack"
            },
            new EnemyDefinition
            {
                name = "Fevered Fieldhand",
                description = "A sickly worker clutching tools, with faint glowing blotches forming under the skin.",
                area = "BlightTilledMeadow",
                healthMin = 35, healthMax = 45,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Balanced,
                abilityName = "Fevered Strike",
                abilityDescription = "Deals extra damage when health is low"
            },
            
            // Area 10: The Thorned Palisade
            new EnemyDefinition
            {
                name = "Thorned Lurker",
                description = "A humanoid partially wrapped in bramble-like bindings that act like living restraints.",
                area = "ThornedPalisade",
                healthMin = 40, healthMax = 50,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Defensive,
                abilityName = "Bramble Bind",
                abilityDescription = "Applies Slow to player on attack"
            },
            new EnemyDefinition
            {
                name = "Palisade Skewerling",
                description = "Small imp-like creatures with wooden stakes for arms, quick and stabbing.",
                area = "ThornedPalisade",
                healthMin = 25, healthMax = 35,
                damage = 5,
                category = EnemyCategory.Swarm,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Quick Stab",
                abilityDescription = "Attacks twice in quick succession"
            },
            
            // Area 11: The Half-Lit Road
            new EnemyDefinition
            {
                name = "Shadelamp Drifter",
                description = "A wandering figure with a lantern that flickers between natural and arcane light.",
                area = "HalfLitRoad",
                healthMin = 35, healthMax = 45,
                damage = 6,
                category = EnemyCategory.Caster,
                aiPattern = EnemyAIPattern.Tactical,
                abilityName = "Flickering Light",
                abilityDescription = "Applies Blind to player"
            },
            new EnemyDefinition
            {
                name = "Half-Lit Runner",
                description = "A roadside bandit whose shadow doesn't match their movements, creating eerie mismatches.",
                area = "HalfLitRoad",
                healthMin = 30, healthMax = 40,
                damage = 5,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Balanced,
                abilityName = "Shadow Mismatch",
                abilityDescription = "Gains evasion when attacking"
            },
            
            // Area 12: Asheslope Ridge
            new EnemyDefinition
            {
                name = "Ash-Bleeder",
                description = "A humanoid covered in soot; when struck, ash bursts from cracks in their skin.",
                area = "AsheslopeRidge",
                healthMin = 40, healthMax = 50,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Ash Burst",
                abilityDescription = "Deals fire damage to player when taking damage"
            },
            new EnemyDefinition
            {
                name = "Cinder Scuttlebeast",
                description = "A small canine-like creature with ember-like spots on its fur.",
                area = "AsheslopeRidge",
                healthMin = 30, healthMax = 40,
                damage = 5,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Ember Bite",
                abilityDescription = "Applies Ignite on attack"
            },
            
            // Area 13: The Folding Vale
            new EnemyDefinition
            {
                name = "Fold-Shift Stalker",
                description = "A creature whose limbs bend at slightly wrong angles, as if space around it is folding.",
                area = "FoldingVale",
                healthMin = 35, healthMax = 45,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Tactical,
                abilityName = "Spatial Fold",
                abilityDescription = "Teleports and gains evasion"
            },
            new EnemyDefinition
            {
                name = "Vale Fragment",
                description = "A fluttering construct-like being made from paper-thin pieces of shifting wood and cloth.",
                area = "FoldingVale",
                healthMin = 25, healthMax = 35,
                damage = 4,
                category = EnemyCategory.Swarm,
                aiPattern = EnemyAIPattern.Balanced,
                abilityName = "Fragment Split",
                abilityDescription = "Spawns additional fragments when damaged"
            },
            
            // Area 14: Path of Failing Light
            new EnemyDefinition
            {
                name = "Gloom-Bound Watcher",
                description = "A tall figure with eyes dimmed to a faint violet glow, slowly scanning the path.",
                area = "PathOfFailingLight",
                healthMin = 40, healthMax = 50,
                damage = 6,
                category = EnemyCategory.Caster,
                aiPattern = EnemyAIPattern.Tactical,
                abilityName = "Gloom Gaze",
                abilityDescription = "Applies Weak to player"
            },
            new EnemyDefinition
            {
                name = "Lumen-Drip Beast",
                description = "A creature whose body leaks faint blue-white luminescence that drips like liquid.",
                area = "PathOfFailingLight",
                healthMin = 35, healthMax = 45,
                damage = 5,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Balanced,
                abilityName = "Luminous Leak",
                abilityDescription = "Heals when taking damage"
            },
            
            // Area 15: The Shattered Gate
            new EnemyDefinition
            {
                name = "Shard-Marked Soldier",
                description = "A humanoid with faint crystal-like fissures spreading across their arms and torso.",
                area = "ShatteredGate",
                healthMin = 45, healthMax = 55,
                damage = 7,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Crystal Shards",
                abilityDescription = "Deals extra damage and applies Bleed"
            },
            new EnemyDefinition
            {
                name = "Fractured Warg",
                description = "A wolf-like beast with jagged shard growths around the jaw and shoulders.",
                area = "ShatteredGate",
                healthMin = 40, healthMax = 50,
                damage = 6,
                category = EnemyCategory.Melee,
                aiPattern = EnemyAIPattern.Aggressive,
                abilityName = "Shard Bite",
                abilityDescription = "Applies Bleed on attack"
            }
        };
    }

    private void CreateEnemyAsset(EnemyDefinition def, string basePath)
    {
        // Create area folder if needed
        string areaPath = $"{basePath}/{def.area}";
        if (!AssetDatabase.IsValidFolder(areaPath))
        {
            string parentPath = basePath;
            string folderName = def.area;
            AssetDatabase.CreateFolder(parentPath, folderName);
        }

        // Create EnemyData
        EnemyData enemyData = ScriptableObject.CreateInstance<EnemyData>();
        enemyData.enemyName = def.name;
        enemyData.description = def.description;
        enemyData.tier = EnemyTier.Normal;
        enemyData.rarity = EnemyRarity.Normal;
        enemyData.category = def.category;
        enemyData.minHealth = def.healthMin;
        enemyData.maxHealth = def.healthMax;
        enemyData.baseDamage = def.damage;
        enemyData.aiPattern = def.aiPattern;
        enemyData.attackPreference = 0.75f; // 75% attack preference
        enemyData.enableEnergy = true;
        enemyData.baseMaxEnergy = 100f;
        enemyData.energyRegenPerTurn = 10f;
        enemyData.staggerThreshold = 100f;
        enemyData.defendGuardPercent = 0.1f;
        enemyData.displayScale = 1f;
        enemyData.basePanelHeight = 280f;
        enemyData.minGoldDrop = 5;
        enemyData.maxGoldDrop = 15;
        enemyData.experienceReward = 10;
        enemyData.cardDropChance = 0.1f;

        // Create ability if specified
        if (!string.IsNullOrEmpty(def.abilityName))
        {
            EnemyAbility ability = CreateAbility(def, areaPath);
            if (ability != null)
            {
                enemyData.abilities = new List<EnemyAbility> { ability };
            }
        }

        // Save asset
        string assetPath = $"{areaPath}/{def.name.Replace(" ", "").Replace("-", "")}.asset";
        AssetDatabase.CreateAsset(enemyData, assetPath);
    }

    private EnemyAbility CreateAbility(EnemyDefinition def, string areaPath)
    {
        EnemyAbility ability = ScriptableObject.CreateInstance<EnemyAbility>();
        ability.id = $"{def.name.Replace(" ", "").Replace("-", "")}_Ability";
        ability.displayName = def.abilityName;
        ability.trigger = AbilityTrigger.OnTurnStart;
        ability.cooldownTurns = 2;
        ability.initialCooldown = 1;
        ability.consumesTurn = false; // Most abilities don't consume turn for common enemies
        ability.target = AbilityTarget.Player;
        ability.energyCost = 0f; // Common enemies use basic energy system

        // Create and save the ability asset first
        string abilityPath = $"{areaPath}/{def.name.Replace(" ", "").Replace("-", "")}_Ability.asset";
        AssetDatabase.CreateAsset(ability, abilityPath);
        AssetDatabase.SaveAssets();

        // Create appropriate effect based on ability description
        AbilityEffect effect = CreateAbilityEffect(def);
        if (effect != null)
        {
            // Add effect as sub-asset to the ability
            AssetDatabase.AddObjectToAsset(effect, ability);
            ability.effects = new List<AbilityEffect> { effect };
            
            // Save again after adding the effect
            EditorUtility.SetDirty(ability);
            AssetDatabase.SaveAssets();
        }
        
        return ability;
    }

    private AbilityEffect CreateAbilityEffect(EnemyDefinition def)
    {
        // Create effects based on ability description keywords
        string desc = def.abilityDescription.ToLower();
        
        if (desc.Contains("damage") || desc.Contains("strike") || desc.Contains("attack"))
        {
            DamageEffect damage = ScriptableObject.CreateInstance<DamageEffect>();
            damage.damageType = DamageType.Physical;
            damage.flatDamage = def.damage + 2; // Slightly more than base attack
            return damage;
        }
        else if (desc.Contains("poison"))
        {
            StatusEffectEffect status = ScriptableObject.CreateInstance<StatusEffectEffect>();
            status.statusType = StatusEffectType.Poison;
            status.magnitude = 2f;
            status.durationTurns = 2;
            status.isDebuff = true;
            return status;
        }
        else if (desc.Contains("slow"))
        {
            StatusEffectEffect status = ScriptableObject.CreateInstance<StatusEffectEffect>();
            status.statusType = StatusEffectType.Slow;
            status.magnitude = 1f;
            status.durationTurns = 2;
            status.isDebuff = true;
            return status;
        }
        else if (desc.Contains("bleed"))
        {
            StatusEffectEffect status = ScriptableObject.CreateInstance<StatusEffectEffect>();
            status.statusType = StatusEffectType.Bleed;
            status.magnitude = 1f;
            status.durationTurns = 2;
            status.isDebuff = true;
            return status;
        }
        else if (desc.Contains("ignite") || desc.Contains("burn"))
        {
            StatusEffectEffect status = ScriptableObject.CreateInstance<StatusEffectEffect>();
            status.statusType = StatusEffectType.Burn;
            status.magnitude = 2f;
            status.durationTurns = 2;
            status.isDebuff = true;
            return status;
        }
        else if (desc.Contains("weak"))
        {
            StatusEffectEffect status = ScriptableObject.CreateInstance<StatusEffectEffect>();
            status.statusType = StatusEffectType.Weak;
            status.magnitude = 1f;
            status.durationTurns = 2;
            status.isDebuff = true;
            return status;
        }
        else if (desc.Contains("blind") || desc.Contains("evasion") || desc.Contains("dodge"))
        {
            // Blind not available, use Weak instead (reduces accuracy)
            StatusEffectEffect status = ScriptableObject.CreateInstance<StatusEffectEffect>();
            status.statusType = StatusEffectType.Weak;
            status.magnitude = 1f;
            status.durationTurns = 1;
            status.isDebuff = true;
            return status;
        }
        
        // Default: simple damage
        DamageEffect defaultDamage = ScriptableObject.CreateInstance<DamageEffect>();
        defaultDamage.damageType = DamageType.Physical;
        defaultDamage.flatDamage = def.damage;
        return defaultDamage;
    }

    private class EnemyDefinition
    {
        public string name;
        public string description;
        public string area;
        public int healthMin;
        public int healthMax;
        public int damage;
        public EnemyCategory category;
        public EnemyAIPattern aiPattern;
        public string abilityName;
        public string abilityDescription;
    }
}

