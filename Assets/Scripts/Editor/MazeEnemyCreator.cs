using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor tool to create all Maze of Broken Clauses enemy assets.
/// Creates folder structure and all EnemyData/EnemyAbility assets.
/// </summary>
public class MazeEnemyCreator : EditorWindow
{
    private string basePath = "Assets/Resources/Enemies/Maze of Broken Clauses";
    
    [MenuItem("Tools/Maze Enemies/Create All Maze Enemies")]
    public static void ShowWindow()
    {
        GetWindow<MazeEnemyCreator>("Maze Enemy Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Maze of Broken Clauses - Enemy Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        basePath = EditorGUILayout.TextField("Base Path:", basePath);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create All Maze Enemies", GUILayout.Height(30)))
        {
            CreateAllEnemies();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("This will create:\n" +
            "• 10 EnemyData ScriptableObjects\n" +
            "• All ability folders and EnemyAbility assets\n" +
            "• DamageEffect and StatusEffectEffect assets\n" +
            "• Proper folder structure matching your pattern", MessageType.Info);
    }
    
    private void CreateAllEnemies()
    {
        // Ensure base directory exists
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        
        // Create all enemies
        CreateMandateFragmenter();
        CreateLawTetheredSentinel();
        CreateNullGeometryHound();
        CreateComplianceWarden();
        CreateRunesealRemnant();
        CreateScholarOfFirstError();
        CreateArchivesWanderer();
        CreateBrokenLecturer();
        CreatePenitentRewriter();
        CreateBlindSpeculator();
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "All maze enemies created successfully!", "OK");
    }
    
    #region LAW CONSTRUCTS (1-5)
    
    private void CreateMandateFragmenter()
    {
        string enemyName = "Mandate Fragmenter";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        // Create EnemyData
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "A drifting mass of broken stone segments held together by glowing law-runes. Sometimes pauses mid-animation as if waiting for an instruction that no longer exists.";
        enemy.tier = EnemyTier.Normal;
        enemy.category = EnemyCategory.Tank;
        enemy.minHealth = 45;
        enemy.maxHealth = 60;
        enemy.baseDamage = 8;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.7f; // More defensive
        enemy.minGoldDrop = 8;
        enemy.maxGoldDrop = 15;
        enemy.cardDropChance = 0.12f;
        
        // Create abilities - Save effects first, then reference them in abilities
        var clauseSlamDamage = SaveEffectAsset(CreateDamageEffect("ClauseSlam_Damage", DamageType.Physical, 12, 0f), 
            Path.Combine(abilitiesFolder, "Clause Slam_Damage.asset"));
        var clauseSlamCostIncrease = SaveEffectAsset(CreateStatusEffect("ClauseSlam_CostIncrease", StatusEffectType.Vulnerable, 1f, 1, true), 
            Path.Combine(abilitiesFolder, "Clause Slam_CostIncrease.asset"));
        
        var clauseSlam = CreateEnemyAbility("Clause Slam", "ClauseSlam", AbilityTrigger.OnAttack, 2, 0, true, AbilityTarget.Player, 0);
        clauseSlam.effects = new List<AbilityEffect> { clauseSlamDamage, clauseSlamCostIncrease };
        SaveAsset(clauseSlam, Path.Combine(abilitiesFolder, "Clause Slam.asset"));
        
        var runicWeightStack = SaveStackAdjustmentWithDefinition("RunicWeight_Stack", 0, 0, 0, 0f, 0f, 0f,
            Path.Combine(abilitiesFolder, "Runic Weight_Stack.asset"),
            Path.Combine(abilitiesFolder, "Runic Weight_Definition.asset"));
        
        var runicWeight = CreateEnemyAbility("Runic Weight", "RunicWeight", AbilityTrigger.OnTurnStart, 3, 1, false, AbilityTarget.Player, 0);
        runicWeight.effects = new List<AbilityEffect> { runicWeightStack };
        SaveAsset(runicWeight, Path.Combine(abilitiesFolder, "Runic Weight.asset"));
        
        var sentenceLockDebuff = SaveEffectAsset(CreateStatusEffect("SentenceLock_Debuff", StatusEffectType.Weak, 0.5f, 1, true),
            Path.Combine(abilitiesFolder, "Sentence Lock_Debuff.asset"));
        
        var sentenceLock = CreateEnemyAbility("Sentence Lock", "SentenceLock", AbilityTrigger.PhaseGate, 4, 2, false, AbilityTarget.Player, 0);
        sentenceLock.phaseThreshold = 0.5f; // Triggers at 50% HP
        sentenceLock.effects = new List<AbilityEffect> { sentenceLockDebuff };
        SaveAsset(sentenceLock, Path.Combine(abilitiesFolder, "Sentence Lock.asset"));
        
        enemy.abilities = new List<EnemyAbility> { clauseSlam, runicWeight, sentenceLock };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreateLawTetheredSentinel()
    {
        string enemyName = "Law-Tethered Sentinel";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "A humanoid statue held upright by cords of radiant text that hum softly. Flickering text occasionally whispers old legal language from the Empire of Vassara.";
        enemy.tier = EnemyTier.Normal;
        enemy.category = EnemyCategory.Tank;
        enemy.minHealth = 50;
        enemy.maxHealth = 70;
        enemy.baseDamage = 6;
        enemy.baseArmor = 3;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.5f; // Balanced
        enemy.minGoldDrop = 10;
        enemy.maxGoldDrop = 18;
        enemy.cardDropChance = 0.15f;
        
        string barrierStackPath = Path.Combine(abilitiesFolder, "Interdict Barrier_Stack.asset");
        string barrierDefPath = Path.Combine(abilitiesFolder, "Interdict Barrier_Definition.asset");
        var barrierStack = SaveStackAdjustmentWithDefinition("InterdictBarrier_Stack", 0, 5, 0, 0f, 0f, 0f, barrierStackPath, barrierDefPath);
        
        var interdictBarrier = CreateEnemyAbility("Interdict Barrier", "InterdictBarrier", AbilityTrigger.OnTurnStart, 4, 0, false, AbilityTarget.Self, 0);
        interdictBarrier.effects = new List<AbilityEffect> { barrierStack };
        SaveAsset(interdictBarrier, Path.Combine(abilitiesFolder, "Interdict Barrier.asset"));
        
        string wordbindDebuffPath = Path.Combine(abilitiesFolder, "Wordbind_Debuff.asset");
        var wordbindDebuff = SaveEffectAsset(CreateStatusEffect("Wordbind_Debuff", StatusEffectType.Weak, 0.5f, 2, true), wordbindDebuffPath);
        
        var wordbind = CreateEnemyAbility("Wordbind", "Wordbind", AbilityTrigger.OnAttack, 2, 0, false, AbilityTarget.Player, 0);
        wordbind.effects = new List<AbilityEffect> { wordbindDebuff };
        SaveAsset(wordbind, Path.Combine(abilitiesFolder, "Wordbind.asset"));
        
        string restoreHealPath = Path.Combine(abilitiesFolder, "Restore Protocol_Heal.asset");
        var restoreHeal = SaveEffectAsset(CreateStatusEffect("RestoreProtocol_Heal", StatusEffectType.Regeneration, 5f, 1, false), restoreHealPath);
        
        var restoreProtocol = CreateEnemyAbility("Restore Protocol", "RestoreProtocol", AbilityTrigger.OnTurnStart, 5, 2, false, AbilityTarget.Self, 0);
        restoreProtocol.effects = new List<AbilityEffect> { restoreHeal };
        SaveAsset(restoreProtocol, Path.Combine(abilitiesFolder, "Restore Protocol.asset"));
        
        enemy.abilities = new List<EnemyAbility> { interdictBarrier, wordbind, restoreProtocol };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreateNullGeometryHound()
    {
        string enemyName = "Null-Geometry Hound";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "An angular beast of floating polyhedrons, like a wolf built from broken geometry. Leaves no footprints—its feet momentarily phase through the floor.";
        enemy.tier = EnemyTier.Normal;
        enemy.category = EnemyCategory.Melee;
        enemy.minHealth = 30;
        enemy.maxHealth = 45;
        enemy.baseDamage = 10;
        enemy.evasionRating = 25f; // High evasion
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.9f;
        enemy.minGoldDrop = 7;
        enemy.maxGoldDrop = 14;
        enemy.cardDropChance = 0.1f;
        
        var sidestepEvasion = SaveEffectAsset(CreateStatusEffect("SidestepEvasion_Buff", StatusEffectType.Evasion, 0.5f, 2, false),
            Path.Combine(abilitiesFolder, "Sidestep Reality_Buff.asset"));
        
        var sidestepReality = CreateEnemyAbility("Sidestep Reality", "SidestepReality", AbilityTrigger.OnTurnStart, 3, 0, false, AbilityTarget.Self, 0);
        sidestepReality.effects = new List<AbilityEffect> { sidestepEvasion };
        SaveAsset(sidestepReality, Path.Combine(abilitiesFolder, "Sidestep Reality.asset"));
        
        var pounceDamage = SaveEffectAsset(CreateDamageEffect("FracturedPounce_Damage", DamageType.Physical, 8, 0.05f),
            Path.Combine(abilitiesFolder, "Fractured Pounce_Damage.asset"));
        
        var fracturedPounce = CreateEnemyAbility("Fractured Pounce", "FracturedPounce", AbilityTrigger.OnAttack, 2, 0, true, AbilityTarget.Player, 0);
        fracturedPounce.effects = new List<AbilityEffect> { pounceDamage };
        SaveAsset(fracturedPounce, Path.Combine(abilitiesFolder, "Fractured Pounce.asset"));
        
        var biteDot = SaveEffectAsset(CreateStatusEffect("EntropicBite_DoT", StatusEffectType.ChaosDot, 3f, 3, true),
            Path.Combine(abilitiesFolder, "Entropic Bite_DoT.asset"));
        
        var entropicBite = CreateEnemyAbility("Entropic Bite", "EntropicBite", AbilityTrigger.OnAttack, 3, 0, false, AbilityTarget.Player, 0);
        entropicBite.effects = new List<AbilityEffect> { biteDot };
        SaveAsset(entropicBite, Path.Combine(abilitiesFolder, "Entropic Bite.asset"));
        
        enemy.abilities = new List<EnemyAbility> { sidestepReality, fracturedPounce, entropicBite };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreateComplianceWarden()
    {
        string enemyName = "Compliance Warden";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "A towering construct shaped like a judge's gavel fused with an armored golem. Takes a deep 'breath' before attacks—except it has no lungs.";
        enemy.tier = EnemyTier.Elite;
        enemy.category = EnemyCategory.Tank;
        enemy.minHealth = 70;
        enemy.maxHealth = 90;
        enemy.baseDamage = 12;
        enemy.baseArmor = 5;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.6f;
        enemy.minGoldDrop = 15;
        enemy.maxGoldDrop = 25;
        enemy.cardDropChance = 0.2f;
        
        var patternViolation = CreateEnemyAbility("Pattern Violation", "PatternViolation", AbilityTrigger.OnDamaged, 0, 0, false, AbilityTarget.Player, 0);
        // Note: Pattern detection would need custom logic - for now, just retaliation damage
        var violationDamage = CreateDamageEffect("PatternViolation_Damage", DamageType.Physical, 8, 0f);
        patternViolation.effects = new List<AbilityEffect> { violationDamage };
        SaveAsset(patternViolation, Path.Combine(abilitiesFolder, "Pattern Violation.asset"));
        SaveAsset(violationDamage, Path.Combine(abilitiesFolder, "Pattern Violation_Damage.asset"));
        
        var edictCrush = CreateEnemyAbility("Edict Crush", "EdictCrush", AbilityTrigger.OnAttack, 3, 1, true, AbilityTarget.AllPlayers, 0);
        var crushDamage = CreateDamageEffect("EdictCrush_Damage", DamageType.Physical, 15, 0f);
        edictCrush.effects = new List<AbilityEffect> { crushDamage };
        SaveAsset(edictCrush, Path.Combine(abilitiesFolder, "Edict Crush.asset"));
        SaveAsset(crushDamage, Path.Combine(abilitiesFolder, "Edict Crush_Damage.asset"));
        
        var oversightProtocol = CreateEnemyAbility("Oversight Protocol", "OversightProtocol", AbilityTrigger.OnTurnStart, 4, 1, false, AbilityTarget.Player, 0);
        // Note: Card reveal/alteration would need custom logic - for now, apply a debuff
        var oversightDebuff = CreateStatusEffect("OversightProtocol_Debuff", StatusEffectType.Vulnerable, 0.3f, 2, true);
        oversightProtocol.effects = new List<AbilityEffect> { oversightDebuff };
        SaveAsset(oversightProtocol, Path.Combine(abilitiesFolder, "Oversight Protocol.asset"));
        SaveAsset(oversightDebuff, Path.Combine(abilitiesFolder, "Oversight Protocol_Debuff.asset"));
        
        enemy.abilities = new List<EnemyAbility> { patternViolation, edictCrush, oversightProtocol };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreateRunesealRemnant()
    {
        string enemyName = "Runeseal Remnant";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "A floating seal-wheel with rotating rings of copper and gold. If you freeze the animation, you can see half-faded imprints of ancient Sealwright hands.";
        enemy.tier = EnemyTier.Normal;
        enemy.category = EnemyCategory.Support;
        enemy.minHealth = 35;
        enemy.maxHealth = 50;
        enemy.baseDamage = 5;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.4f; // More support-focused
        enemy.minGoldDrop = 9;
        enemy.maxGoldDrop = 16;
        enemy.cardDropChance = 0.12f;
        
        var chronoRotate = CreateEnemyAbility("Chrono-Rotate", "ChronoRotate", AbilityTrigger.OnTurnStart, 5, 1, false, AbilityTarget.Player, 0);
        // Note: Deck scrambling would need custom logic - for now, apply a debuff
        var chronoDebuff = CreateStatusEffect("ChronoRotate_Debuff", StatusEffectType.Slow, 0.5f, 2, true);
        chronoRotate.effects = new List<AbilityEffect> { chronoDebuff };
        SaveAsset(chronoRotate, Path.Combine(abilitiesFolder, "Chrono-Rotate.asset"));
        SaveAsset(chronoDebuff, Path.Combine(abilitiesFolder, "Chrono-Rotate_Debuff.asset"));
        
        string sealflareStackPath = Path.Combine(abilitiesFolder, "Sealflare_Buff.asset");
        string sealflareDefPath = Path.Combine(abilitiesFolder, "Sealflare_Definition.asset");
        var sealflareBuff = SaveStackAdjustmentWithDefinition("Sealflare_Buff", 3, 0, 0, 0f, 0f, 0f, sealflareStackPath, sealflareDefPath);
        
        var sealflare = CreateEnemyAbility("Sealflare", "Sealflare", AbilityTrigger.OnTurnStart, 4, 0, false, AbilityTarget.Self, 0);
        sealflare.effects = new List<AbilityEffect> { sealflareBuff };
        SaveAsset(sealflare, Path.Combine(abilitiesFolder, "Sealflare.asset"));
        
        var lockspin = CreateEnemyAbility("Lockspin", "Lockspin", AbilityTrigger.PhaseGate, 6, 3, false, AbilityTarget.Player, 0);
        lockspin.phaseThreshold = 0.3f; // Triggers at 30% HP
        // Note: Disable drawing would need custom logic - for now, apply Slow (reduced action speed)
        var lockspinDebuff = CreateStatusEffect("Lockspin_Debuff", StatusEffectType.Slow, 1f, 1, true);
        lockspin.effects = new List<AbilityEffect> { lockspinDebuff };
        SaveAsset(lockspin, Path.Combine(abilitiesFolder, "Lockspin.asset"));
        SaveAsset(lockspinDebuff, Path.Combine(abilitiesFolder, "Lockspin_Debuff.asset"));
        
        enemy.abilities = new List<EnemyAbility> { chronoRotate, sealflare, lockspin };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    #endregion
    
    #region LOST SCHOLARS (6-10)
    
    private void CreateScholarOfFirstError()
    {
        string enemyName = "Scholar of the First Error";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "His face repeats three different expressions, out of sync, as if stuck between timelines. Speaks entire sentences in reverse before correcting himself.";
        enemy.tier = EnemyTier.Elite;
        enemy.category = EnemyCategory.Caster;
        enemy.minHealth = 40;
        enemy.maxHealth = 60;
        enemy.baseDamage = 9;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.8f;
        enemy.minGoldDrop = 12;
        enemy.maxGoldDrop = 20;
        enemy.cardDropChance = 0.18f;
        
        var echoedCast = CreateEnemyAbility("Echoed Cast", "EchoedCast", AbilityTrigger.OnAttack, 2, 0, false, AbilityTarget.Player, 0);
        var echoDamage = CreateDamageEffect("EchoedCast_Damage", DamageType.Chaos, 10, 0f);
        echoedCast.effects = new List<AbilityEffect> { echoDamage };
        SaveAsset(echoedCast, Path.Combine(abilitiesFolder, "Echoed Cast.asset"));
        SaveAsset(echoDamage, Path.Combine(abilitiesFolder, "Echoed Cast_Damage.asset"));
        
        var misalignedThesis = CreateEnemyAbility("Misaligned Thesis", "MisalignedThesis", AbilityTrigger.OnTurnStart, 3, 1, false, AbilityTarget.Player, 0);
        // Note: Self-targeting would need custom logic - for now, apply Weak (reduced damage)
        var misalignedDebuff = CreateStatusEffect("MisalignedThesis_Debuff", StatusEffectType.Weak, 0.4f, 2, true);
        misalignedThesis.effects = new List<AbilityEffect> { misalignedDebuff };
        SaveAsset(misalignedThesis, Path.Combine(abilitiesFolder, "Misaligned Thesis.asset"));
        SaveAsset(misalignedDebuff, Path.Combine(abilitiesFolder, "Misaligned Thesis_Debuff.asset"));
        
        var reboundSurge = CreateEnemyAbility("Rebound Surge", "ReboundSurge", AbilityTrigger.OnDamaged, 0, 0, false, AbilityTarget.Player, 0);
        var surgeDamage = CreateDamageEffect("ReboundSurge_Damage", DamageType.Chaos, 6, 0f);
        reboundSurge.effects = new List<AbilityEffect> { surgeDamage };
        SaveAsset(reboundSurge, Path.Combine(abilitiesFolder, "Rebound Surge.asset"));
        SaveAsset(surgeDamage, Path.Combine(abilitiesFolder, "Rebound Surge_Damage.asset"));
        
        enemy.abilities = new List<EnemyAbility> { echoedCast, misalignedThesis, reboundSurge };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreateArchivesWanderer()
    {
        string enemyName = "Archives-Wanderer";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "Carries the last pages of a treatise that was never finished. The pages burn from the edges inward. Sometimes stops fighting to jot invisible notes in the air.";
        enemy.tier = EnemyTier.Elite;
        enemy.category = EnemyCategory.Caster;
        enemy.minHealth = 50;
        enemy.maxHealth = 70;
        enemy.baseDamage = 8;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.7f;
        enemy.minGoldDrop = 14;
        enemy.maxGoldDrop = 22;
        enemy.cardDropChance = 0.2f;
        
        string insightStackPath = Path.Combine(abilitiesFolder, "Accumulated Insight_Stack.asset");
        string insightDefPath = Path.Combine(abilitiesFolder, "Accumulated Insight_Definition.asset");
        var insightStack = SaveStackAdjustmentWithDefinition("AccumulatedInsight_Stack", 2, 0, 0, 0f, 0f, 0f, insightStackPath, insightDefPath);
        
        var accumulatedInsight = CreateEnemyAbility("Accumulated Insight", "AccumulatedInsight", AbilityTrigger.OnTurnStart, 0, 0, false, AbilityTarget.Self, 0);
        accumulatedInsight.effects = new List<AbilityEffect> { insightStack };
        SaveAsset(accumulatedInsight, Path.Combine(abilitiesFolder, "Accumulated Insight.asset"));
        
        var inkTorrent = CreateEnemyAbility("Ink Torrent", "InkTorrent", AbilityTrigger.OnAttack, 3, 0, true, AbilityTarget.Player, 0);
        var torrentDamage = CreateDamageEffect("InkTorrent_Damage", DamageType.Chaos, 12, 0f);
        inkTorrent.effects = new List<AbilityEffect> { torrentDamage };
        SaveAsset(inkTorrent, Path.Combine(abilitiesFolder, "Ink Torrent.asset"));
        SaveAsset(torrentDamage, Path.Combine(abilitiesFolder, "Ink Torrent_Damage.asset"));
        
        var finalAnnotation = CreateEnemyAbility("Final Annotation", "FinalAnnotation", AbilityTrigger.OnDeath, 0, 0, false, AbilityTarget.Player, 0);
        var annotationDamage = CreateDamageEffect("FinalAnnotation_Damage", DamageType.Chaos, 20, 0.1f); // 20 + 10% max HP
        finalAnnotation.effects = new List<AbilityEffect> { annotationDamage };
        SaveAsset(finalAnnotation, Path.Combine(abilitiesFolder, "Final Annotation.asset"));
        SaveAsset(annotationDamage, Path.Combine(abilitiesFolder, "Final Annotation_Damage.asset"));
        
        enemy.abilities = new List<EnemyAbility> { accumulatedInsight, inkTorrent, finalAnnotation };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreateBrokenLecturer()
    {
        string enemyName = "The Broken Lecturer";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "A transparent silhouette flickering between bodies, endlessly teaching a lesson no one can hear. Mouth moves constantly but produces no sound—yet players feel lectured.";
        enemy.tier = EnemyTier.Elite;
        enemy.category = EnemyCategory.Support;
        enemy.minHealth = 45;
        enemy.maxHealth = 65;
        enemy.baseDamage = 7;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.5f;
        enemy.minGoldDrop = 13;
        enemy.maxGoldDrop = 21;
        enemy.cardDropChance = 0.18f;
        
        var thesisDrain = CreateEnemyAbility("Thesis Drain", "ThesisDrain", AbilityTrigger.OnAttack, 2, 0, false, AbilityTarget.Player, 0);
        var drainDebuff = CreateStatusEffect("ThesisDrain_Debuff", StatusEffectType.Weak, 0.5f, 3, true); // Weak = reduced damage
        thesisDrain.effects = new List<AbilityEffect> { drainDebuff };
        SaveAsset(thesisDrain, Path.Combine(abilitiesFolder, "Thesis Drain.asset"));
        SaveAsset(drainDebuff, Path.Combine(abilitiesFolder, "Thesis Drain_Debuff.asset"));
        
        var invertedLesson = CreateEnemyAbility("Inverted Lesson", "InvertedLesson", AbilityTrigger.OnTurnStart, 4, 1, false, AbilityTarget.Player, 0);
        // Note: Buff inversion would need custom logic - for now, apply Vulnerable (take more damage)
        var invertedDebuff = CreateStatusEffect("InvertedLesson_Debuff", StatusEffectType.Vulnerable, 0.5f, 2, true);
        invertedLesson.effects = new List<AbilityEffect> { invertedDebuff };
        SaveAsset(invertedLesson, Path.Combine(abilitiesFolder, "Inverted Lesson.asset"));
        SaveAsset(invertedDebuff, Path.Combine(abilitiesFolder, "Inverted Lesson_Debuff.asset"));
        
        var lecturePulse = CreateEnemyAbility("Lecture Pulse", "LecturePulse", AbilityTrigger.OnAttack, 2, 0, true, AbilityTarget.AllPlayers, 0);
        var pulseDamage = CreateDamageEffect("LecturePulse_Damage", DamageType.Chaos, 5, 0f);
        lecturePulse.effects = new List<AbilityEffect> { pulseDamage };
        SaveAsset(lecturePulse, Path.Combine(abilitiesFolder, "Lecture Pulse.asset"));
        SaveAsset(pulseDamage, Path.Combine(abilitiesFolder, "Lecture Pulse_Damage.asset"));
        
        enemy.abilities = new List<EnemyAbility> { thesisDrain, invertedLesson, lecturePulse };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreatePenitentRewriter()
    {
        string enemyName = "The Penitent Rewriter";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "A scholar who tried to 'fix' the Law after the Shattering and scarred himself with glyphs. Mutters 'still wrong… still wrong…' during every action.";
        enemy.tier = EnemyTier.Elite;
        enemy.category = EnemyCategory.Caster;
        enemy.minHealth = 55;
        enemy.maxHealth = 75;
        enemy.baseDamage = 11;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.85f;
        enemy.minGoldDrop = 16;
        enemy.maxGoldDrop = 24;
        enemy.cardDropChance = 0.22f;
        
        var correctionStrike = CreateEnemyAbility("Correction Strike", "CorrectionStrike", AbilityTrigger.OnAttack, 2, 0, true, AbilityTarget.Player, 0);
        var strikeDamage = CreateDamageEffect("CorrectionStrike_Damage", DamageType.Chaos, 18, 0f);
        correctionStrike.effects = new List<AbilityEffect> { strikeDamage };
        // Note: Self-damage (5 damage) would need custom implementation - DamageEffect targets are set by EnemyAbility
        SaveAsset(correctionStrike, Path.Combine(abilitiesFolder, "Correction Strike.asset"));
        SaveAsset(strikeDamage, Path.Combine(abilitiesFolder, "Correction Strike_Damage.asset"));
        
        var revokePattern = CreateEnemyAbility("Revoke Pattern", "RevokePattern", AbilityTrigger.OnTurnStart, 3, 1, false, AbilityTarget.Player, 0);
        // Note: Status removal would need custom logic - for now, apply a strong debuff
        var revokeDebuff = CreateStatusEffect("RevokePattern_Debuff", StatusEffectType.Vulnerable, 0.6f, 2, true);
        revokePattern.effects = new List<AbilityEffect> { revokeDebuff };
        SaveAsset(revokePattern, Path.Combine(abilitiesFolder, "Revoke Pattern.asset"));
        SaveAsset(revokeDebuff, Path.Combine(abilitiesFolder, "Revoke Pattern_Debuff.asset"));
        
        var burningScript = CreateEnemyAbility("Burning Script", "BurningScript", AbilityTrigger.OnAttack, 3, 0, false, AbilityTarget.Player, 0);
        var burningDot = CreateStatusEffect("BurningScript_DoT", StatusEffectType.Burn, 4f, 3, true);
        burningScript.effects = new List<AbilityEffect> { burningDot };
        SaveAsset(burningScript, Path.Combine(abilitiesFolder, "Burning Script.asset"));
        SaveAsset(burningDot, Path.Combine(abilitiesFolder, "Burning Script_DoT.asset"));
        
        enemy.abilities = new List<EnemyAbility> { correctionStrike, revokePattern, burningScript };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    private void CreateBlindSpeculator()
    {
        string enemyName = "The Blind Speculator";
        string folder = Path.Combine(basePath, enemyName);
        string abilitiesFolder = Path.Combine(folder, "Abilities", "Enemy");
        
        EnsureDirectory(abilitiesFolder);
        
        EnemyData enemy = ScriptableObject.CreateInstance<EnemyData>();
        enemy.enemyName = enemyName;
        enemy.description = "Eyes burned out by glimpsing the Entropic Beyond—perhaps the Seer's dark mirror. Turns its head toward the player despite having no sense of sight.";
        enemy.tier = EnemyTier.Miniboss;
        enemy.category = EnemyCategory.Caster;
        enemy.minHealth = 80;
        enemy.maxHealth = 100;
        enemy.baseDamage = 10;
        enemy.aiPattern = EnemyAIPattern.Aggressive;
        enemy.attackPreference = 0.75f;
        enemy.minGoldDrop = 20;
        enemy.maxGoldDrop = 30;
        enemy.cardDropChance = 0.25f;
        
        var futureLeak = CreateEnemyAbility("Future Leak", "FutureLeak", AbilityTrigger.OnTurnStart, 4, 0, false, AbilityTarget.Player, 0);
        // Note: Card reveal would need custom logic - for now, apply a debuff
        var leakDebuff = CreateStatusEffect("FutureLeak_Debuff", StatusEffectType.Vulnerable, 0.3f, 2, true);
        futureLeak.effects = new List<AbilityEffect> { leakDebuff };
        SaveAsset(futureLeak, Path.Combine(abilitiesFolder, "Future Leak.asset"));
        SaveAsset(leakDebuff, Path.Combine(abilitiesFolder, "Future Leak_Debuff.asset"));
        
        var speculativeStrike = CreateEnemyAbility("Speculative Strike", "SpeculativeStrike", AbilityTrigger.OnAttack, 2, 0, true, AbilityTarget.Player, 0);
        var specDamage = CreateDamageEffect("SpeculativeStrike_Damage", DamageType.Chaos, 15, 0.08f); // 15 + 8% max HP
        speculativeStrike.effects = new List<AbilityEffect> { specDamage };
        SaveAsset(speculativeStrike, Path.Combine(abilitiesFolder, "Speculative Strike.asset"));
        SaveAsset(specDamage, Path.Combine(abilitiesFolder, "Speculative Strike_Damage.asset"));
        
        var horizonShriek = CreateEnemyAbility("Horizon Shriek", "HorizonShriek", AbilityTrigger.OnTurnStart, 5, 2, false, AbilityTarget.Player, 0);
        // Note: Card discard would need custom logic - for now, apply Stun (skip turn)
        var shriekDebuff = CreateStatusEffect("HorizonShriek_Debuff", StatusEffectType.Stun, 1f, 1, true);
        horizonShriek.effects = new List<AbilityEffect> { shriekDebuff };
        SaveAsset(horizonShriek, Path.Combine(abilitiesFolder, "Horizon Shriek.asset"));
        SaveAsset(shriekDebuff, Path.Combine(abilitiesFolder, "Horizon Shriek_Debuff.asset"));
        
        enemy.abilities = new List<EnemyAbility> { futureLeak, speculativeStrike, horizonShriek };
        
        SaveAsset(enemy, Path.Combine(folder, $"{enemyName}.asset"));
    }
    
    #endregion
    
    #region Helper Methods
    
    private void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    private EnemyAbility CreateEnemyAbility(string displayName, string id, AbilityTrigger trigger, int cooldown, int initialCooldown, bool consumesTurn, AbilityTarget target, float energyCost)
    {
        EnemyAbility ability = ScriptableObject.CreateInstance<EnemyAbility>();
        ability.displayName = displayName;
        ability.id = id;
        ability.trigger = trigger;
        ability.cooldownTurns = cooldown;
        ability.initialCooldown = initialCooldown;
        ability.consumesTurn = consumesTurn;
        ability.target = target;
        ability.energyCost = energyCost;
        return ability;
    }
    
    private DamageEffect CreateDamageEffect(string name, DamageType damageType, int flatDamage, float percentOfMaxHp)
    {
        DamageEffect effect = ScriptableObject.CreateInstance<DamageEffect>();
        effect.name = name;
        effect.damageType = damageType;
        effect.flatDamage = flatDamage;
        effect.percentOfMaxHp = percentOfMaxHp;
        return effect;
    }
    
    private StatusEffectEffect CreateStatusEffect(string name, StatusEffectType statusType, float magnitude, int duration, bool isDebuff)
    {
        StatusEffectEffect effect = ScriptableObject.CreateInstance<StatusEffectEffect>();
        effect.name = name;
        effect.statusType = statusType;
        effect.magnitude = magnitude;
        effect.durationTurns = duration;
        effect.isDebuff = isDebuff;
        return effect;
    }
    
    private StackAdjustmentEffect CreateStackAdjustment(string name, int agitate, int tolerance, int potential, float agitatePercent, float tolerancePercent, float potentialPercent)
    {
        StackAdjustmentEffect effect = ScriptableObject.CreateInstance<StackAdjustmentEffect>();
        effect.name = name;
        
        // Create StackAdjustmentDefinition
        StackAdjustmentDefinition adjustment = CreateStackAdjustmentDefinition(agitate, tolerance, potential, agitatePercent, tolerancePercent, potentialPercent);
        
        effect.adjustment = adjustment;
        return effect;
    }
    
    /// <summary>
    /// Creates a StackAdjustmentDefinition with the specified values.
    /// </summary>
    private StackAdjustmentDefinition CreateStackAdjustmentDefinition(int agitate, int tolerance, int potential, 
        float agitatePercent, float tolerancePercent, float potentialPercent)
    {
        StackAdjustmentDefinition adjustment = ScriptableObject.CreateInstance<StackAdjustmentDefinition>();
        adjustment.agitateStacks = agitate;
        adjustment.toleranceStacks = tolerance;
        adjustment.potentialStacks = potential;
        adjustment.agitateIncreasedPercent = agitatePercent;
        adjustment.toleranceIncreasedPercent = tolerancePercent;
        adjustment.potentialIncreasedPercent = potentialPercent;
        return adjustment;
    }
    
    /// <summary>
    /// Saves a StackAdjustmentEffect with its definition, ensuring proper references.
    /// </summary>
    private StackAdjustmentEffect SaveStackAdjustmentWithDefinition(string effectName, int agitate, int tolerance, int potential,
        float agitatePercent, float tolerancePercent, float potentialPercent, string effectPath, string definitionPath)
    {
        // Save definition first and get asset reference
        var definition = CreateStackAdjustmentDefinition(agitate, tolerance, potential, 
            agitatePercent, tolerancePercent, potentialPercent);
        var savedDefinition = SaveStackAdjustmentAsset(definition, definitionPath);
        
        // Create effect with saved definition reference
        var effect = ScriptableObject.CreateInstance<StackAdjustmentEffect>();
        effect.name = effectName;
        effect.adjustment = savedDefinition;
        
        // Save and reload effect to get proper asset reference
        return SaveEffectAsset(effect, effectPath);
    }
    
    private void SaveAsset(Object asset, string path)
    {
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }
    
    /// <summary>
    /// Saves an effect asset and returns a proper asset reference.
    /// This ensures the ability can properly reference the effect.
    /// </summary>
    private T SaveEffectAsset<T>(T effect, string path) where T : AbilityEffect
    {
        if (effect == null) return null;
        
        // Ensure directory exists
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        AssetDatabase.CreateAsset(effect, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Reload as asset to get proper reference
        T loaded = AssetDatabase.LoadAssetAtPath<T>(path);
        if (loaded == null)
        {
            Debug.LogError($"[MazeEnemyCreator] Failed to load effect asset at {path}");
        }
        return loaded;
    }
    
    /// <summary>
    /// Saves a stack adjustment definition asset and returns a proper asset reference.
    /// </summary>
    private StackAdjustmentDefinition SaveStackAdjustmentAsset(StackAdjustmentDefinition adjustment, string path)
    {
        if (adjustment == null) return null;
        
        // Ensure directory exists
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        AssetDatabase.CreateAsset(adjustment, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Reload as asset to get proper reference
        return AssetDatabase.LoadAssetAtPath<StackAdjustmentDefinition>(path);
    }
    
    #endregion
}

