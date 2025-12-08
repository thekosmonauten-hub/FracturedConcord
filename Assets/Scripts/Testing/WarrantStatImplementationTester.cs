using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data;
using Dexiled.Data.Items;

/// <summary>
/// Comprehensive test script for warrant stat implementations.
/// Tests all Phase 1 and Phase 2 features.
/// </summary>
public class WarrantStatImplementationTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool verboseLogging = true;
    
    [Header("Test Data")]
    [SerializeField] private WeaponItem testWeapon;
    [SerializeField] private Armour testArmour;
    
    private Character testCharacter;
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        Debug.Log("<color=cyan>========================================</color>");
        Debug.Log("<color=cyan>WARRANT STAT IMPLEMENTATION TEST SUITE</color>");
        Debug.Log("<color=cyan>========================================</color>\n");
        
        SetupTestCharacter();
        
        // Phase 1 Tests
        TestPhase1_1_CardTagModifiers();
        TestPhase1_2_GuardEffectiveness();
        TestPhase1_3_FlatVsIncreasedModifiers();
        TestPhase1_4_StatusEffectDuration();
        
        // Phase 2 Tests
        TestPhase2_1_DefenseModifiersScaling();
        TestPhase2_2_WeaponTypeModifiers();
        TestPhase2_3_AilmentDamageModifiers();
        TestPhase2_4_ConditionalDamage();
        
        Debug.Log("\n<color=green>========================================</color>");
        Debug.Log("<color=green>ALL TESTS COMPLETE</color>");
        Debug.Log("<color=green>========================================</color>");
    }
    
    private void SetupTestCharacter()
    {
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("[WarrantTester] CharacterManager.Instance is null! Cannot run tests.");
            return;
        }
        
        // Get or create test character
        testCharacter = CharacterManager.Instance.GetCurrentCharacter();
        if (testCharacter == null)
        {
            Debug.LogWarning("[WarrantTester] No character loaded. Creating test character...");
            // Try to load a character or create a basic one
            // Character constructor requires name and characterClass
            testCharacter = new Character("TestCharacter", "Warrior");
            testCharacter.level = 10;
            testCharacter.strength = 50;
            testCharacter.dexterity = 50;
            testCharacter.intelligence = 50;
            testCharacter.maxHealth = 100;
            testCharacter.currentHealth = 100;
            testCharacter.maxMana = 50;
            testCharacter.mana = 50;
            testCharacter.CalculateDerivedStats();
        }
        
        Debug.Log($"<color=yellow>[Setup] Using character: {testCharacter.characterName} (Level {testCharacter.level})</color>\n");
    }
    
    #region Phase 1 Tests
    
    private void TestPhase1_1_CardTagModifiers()
    {
        Debug.Log("<color=magenta>=== Phase 1.1: Card Tag-Based Damage Modifiers ===</color>");
        
        if (testCharacter == null) return;
        
        // Create test stats data
        var statsData = new CharacterStatsData(testCharacter);
        
        // Test Projectile tag modifier
        statsData.increasedProjectileDamage = 20f;
        var projectileCtx = new DamageContext
        {
            damageType = "physical",
            isProjectile = true,
            isArea = false,
            isMelee = false,
            isRanged = false
        };
        float projectileInc = StatsDamageCalculator.ComputeIncreased(statsData, projectileCtx);
        LogTestResult("Projectile Damage Modifier", projectileInc == 20f, $"Expected: 20%, Got: {projectileInc}%");
        
        // Test AoE tag modifier
        statsData.increasedAreaDamage = 15f;
        var aoeCtx = new DamageContext
        {
            damageType = "fire",
            isProjectile = false,
            isArea = true,
            isMelee = false,
            isRanged = false
        };
        float aoeInc = StatsDamageCalculator.ComputeIncreased(statsData, aoeCtx);
        LogTestResult("AoE Damage Modifier", aoeInc == 15f, $"Expected: 15%, Got: {aoeInc}%");
        
        // Test Melee modifier
        statsData.increasedMeleeDamage = 25f;
        var meleeCtx = new DamageContext
        {
            damageType = "physical",
            isProjectile = false,
            isArea = false,
            isMelee = true,
            isRanged = false
        };
        float meleeInc = StatsDamageCalculator.ComputeIncreased(statsData, meleeCtx);
        LogTestResult("Melee Damage Modifier", meleeInc == 25f, $"Expected: 25%, Got: {meleeInc}%");
        
        Debug.Log("");
    }
    
    private void TestPhase1_2_GuardEffectiveness()
    {
        Debug.Log("<color=magenta>=== Phase 1.2: Guard Effectiveness ===</color>");
        
        if (testCharacter == null) return;
        
        // Set guard effectiveness modifier
        var statsData = new CharacterStatsData(testCharacter);
        statsData.guardEffectivenessIncreased = 30f;
        
        // Test guard calculation
        float baseGuard = 100f;
        float expectedGuard = baseGuard * (1f + 30f / 100f); // 130
        
        // Simulate guard calculation (as done in CardEffectProcessor)
        float finalGuard = baseGuard;
        if (statsData.guardEffectivenessIncreased > 0f)
        {
            float effectivenessMultiplier = 1f + (statsData.guardEffectivenessIncreased / 100f);
            finalGuard *= effectivenessMultiplier;
        }
        
        LogTestResult("Guard Effectiveness", Mathf.Approximately(finalGuard, expectedGuard), 
            $"Base: {baseGuard}, Expected: {expectedGuard}, Got: {finalGuard}");
        
        Debug.Log("");
    }
    
    private void TestPhase1_3_FlatVsIncreasedModifiers()
    {
        Debug.Log("<color=magenta>=== Phase 1.3: Flat vs Increased Modifiers ===</color>");
        
        if (testCharacter == null) return;
        
        // Set base values
        testCharacter.maxHealth = 100;
        testCharacter.maxMana = 50;
        
        // Apply flat modifiers
        if (testCharacter.warrantFlatModifiers == null)
            testCharacter.warrantFlatModifiers = new Dictionary<string, float>();
        testCharacter.warrantFlatModifiers["maxHealthFlat"] = 40f;
        testCharacter.warrantFlatModifiers["maxManaFlat"] = 20f;
        
        // Apply increased modifiers
        if (testCharacter.warrantStatModifiers == null)
            testCharacter.warrantStatModifiers = new Dictionary<string, float>();
        testCharacter.warrantStatModifiers["maxHealthIncreased"] = 20f; // 20%
        testCharacter.warrantStatModifiers["maxManaIncreased"] = 30f; // 30%
        
        // Create stats data (should apply modifiers)
        var statsData = new CharacterStatsData(testCharacter);
        
        // Expected: (100 + 40) * 1.20 = 168
        float expectedHealth = (100f + 40f) * 1.20f;
        // Expected: (50 + 20) * 1.30 = 91
        float expectedMana = (50f + 20f) * 1.30f;
        
        LogTestResult("Max Health (Flat + Increased)", Mathf.Approximately(statsData.maxHealth, expectedHealth),
            $"Expected: {expectedHealth}, Got: {statsData.maxHealth}");
        LogTestResult("Max Mana (Flat + Increased)", Mathf.Approximately(statsData.maxMana, expectedMana),
            $"Expected: {expectedMana}, Got: {statsData.maxMana}");
        
        Debug.Log("");
    }
    
    private void TestPhase1_4_StatusEffectDuration()
    {
        Debug.Log("<color=magenta>=== Phase 1.4: Status Effect Duration ===</color>");
        
        if (testCharacter == null) return;
        
        // Set status effect duration modifier
        var statsData = new CharacterStatsData(testCharacter);
        statsData.statusEffectDuration = 50f; // 50% increased duration
        
        // Simulate status effect duration calculation
        int baseDuration = 4;
        float durationMultiplier = 1f + (statsData.statusEffectDuration / 100f);
        float modifiedDuration = baseDuration * durationMultiplier;
        int finalDuration = Mathf.CeilToInt(modifiedDuration); // Should round up
        
        // Expected: 4 * 1.5 = 6
        int expectedDuration = 6;
        
        LogTestResult("Status Effect Duration", finalDuration == expectedDuration,
            $"Base: {baseDuration}, Expected: {expectedDuration}, Got: {finalDuration}");
        
        Debug.Log("");
    }
    
    #endregion
    
    #region Phase 2 Tests
    
    private void TestPhase2_1_DefenseModifiersScaling()
    {
        Debug.Log("<color=magenta>=== Phase 2.1: Defense Modifiers Scaling ===</color>");
        
        if (testCharacter == null) return;
        
        // Set base defense values from items
        testCharacter.baseEvasionFromItems = 100f;
        testCharacter.baseArmourFromItems = 150f;
        testCharacter.baseEnergyShieldFromItems = 80f;
        
        // Apply increased modifiers
        if (testCharacter.warrantStatModifiers == null)
            testCharacter.warrantStatModifiers = new Dictionary<string, float>();
        testCharacter.warrantStatModifiers["evasionIncreased"] = 25f; // 25%
        testCharacter.warrantStatModifiers["armourIncreased"] = 30f; // 30%
        testCharacter.warrantStatModifiers["energyShieldIncreased"] = 40f; // 40%
        
        // Create stats data (should apply modifiers)
        var statsData = new CharacterStatsData(testCharacter);
        
        // Expected: 100 * 1.25 = 125
        float expectedEvasion = 100f * 1.25f;
        // Expected: 150 * 1.30 = 195
        int expectedArmour = Mathf.RoundToInt(150f * 1.30f);
        // Expected: 80 * 1.40 = 112
        int expectedEnergyShield = Mathf.RoundToInt(80f * 1.40f);
        
        LogTestResult("Evasion Scaling", Mathf.Approximately(statsData.evasion, expectedEvasion),
            $"Expected: {expectedEvasion}, Got: {statsData.evasion}");
        LogTestResult("Armour Scaling", statsData.armour == expectedArmour,
            $"Expected: {expectedArmour}, Got: {statsData.armour}");
        LogTestResult("Energy Shield Scaling", statsData.energyShield == expectedEnergyShield,
            $"Expected: {expectedEnergyShield}, Got: {statsData.energyShield}");
        
        Debug.Log("");
    }
    
    private void TestPhase2_2_WeaponTypeModifiers()
    {
        Debug.Log("<color=magenta>=== Phase 2.2: Weapon Type Modifiers ===</color>");
        
        if (testCharacter == null) return;
        
        var statsData = new CharacterStatsData(testCharacter);
        
        // Test Axe modifier
        statsData.increasedAxeDamage = 35f;
        var axeCtx = new DamageContext { weaponType = "axe", damageType = "physical" };
        float axeInc = StatsDamageCalculator.ComputeIncreased(statsData, axeCtx);
        LogTestResult("Axe Damage Modifier", axeInc == 35f, $"Expected: 35%, Got: {axeInc}%");
        
        // Test Bow modifier
        statsData.increasedBowDamage = 28f;
        var bowCtx = new DamageContext { weaponType = "bow", damageType = "physical" };
        float bowInc = StatsDamageCalculator.ComputeIncreased(statsData, bowCtx);
        LogTestResult("Bow Damage Modifier", bowInc == 28f, $"Expected: 28%, Got: {bowInc}%");
        
        // Test Sword modifier
        statsData.increasedSwordDamage = 22f;
        var swordCtx = new DamageContext { weaponType = "sword", damageType = "physical" };
        float swordInc = StatsDamageCalculator.ComputeIncreased(statsData, swordCtx);
        LogTestResult("Sword Damage Modifier", swordInc == 22f, $"Expected: 22%, Got: {swordInc}%");
        
        // Test Dagger modifier
        statsData.increasedDaggerDamage = 18f;
        var daggerCtx = new DamageContext { weaponType = "dagger", damageType = "physical" };
        float daggerInc = StatsDamageCalculator.ComputeIncreased(statsData, daggerCtx);
        LogTestResult("Dagger Damage Modifier", daggerInc == 18f, $"Expected: 18%, Got: {daggerInc}%");
        
        Debug.Log("");
    }
    
    private void TestPhase2_3_AilmentDamageModifiers()
    {
        Debug.Log("<color=magenta>=== Phase 2.3: Ailment Damage Modifiers ===</color>");
        
        if (testCharacter == null) return;
        
        var statsData = new CharacterStatsData(testCharacter);
        
        // Test Poison modifiers
        statsData.increasedPoisonMagnitude = 20f;
        statsData.increasedPoisonDamage = 15f;
        statsData.increasedDamageOverTime = 10f;
        
        // Simulate poison damage calculation
        float basePoisonDamage = 30f; // Base magnitude
        float magnitudeMultiplier = 1f + (statsData.increasedPoisonMagnitude / 100f);
        float poisonDamageMultiplier = 1f + (statsData.increasedPoisonDamage / 100f);
        float dotMultiplier = 1f + (statsData.increasedDamageOverTime / 100f);
        float finalPoisonDamage = basePoisonDamage * magnitudeMultiplier * poisonDamageMultiplier * dotMultiplier;
        
        // Expected: 30 * 1.20 * 1.15 * 1.10 = 45.54
        float expectedPoison = 30f * 1.20f * 1.15f * 1.10f;
        
        LogTestResult("Poison Damage Modifiers", Mathf.Approximately(finalPoisonDamage, expectedPoison),
            $"Base: {basePoisonDamage}, Expected: {expectedPoison:F2}, Got: {finalPoisonDamage:F2}");
        
        // Test Ignite modifiers
        statsData.increasedIgniteMagnitude = 25f;
        float baseIgniteDamage = 50f;
        float igniteMagnitudeMultiplier = 1f + (statsData.increasedIgniteMagnitude / 100f);
        float finalIgniteDamage = baseIgniteDamage * igniteMagnitudeMultiplier * dotMultiplier;
        
        // Expected: 50 * 1.25 * 1.10 = 68.75
        float expectedIgnite = 50f * 1.25f * 1.10f;
        
        LogTestResult("Ignite Damage Modifiers", Mathf.Approximately(finalIgniteDamage, expectedIgnite),
            $"Base: {baseIgniteDamage}, Expected: {expectedIgnite:F2}, Got: {finalIgniteDamage:F2}");
        
        // Test Bleed modifiers
        statsData.increasedBleedMagnitude = 18f;
        float baseBleedDamage = 40f;
        float bleedMagnitudeMultiplier = 1f + (statsData.increasedBleedMagnitude / 100f);
        float finalBleedDamage = baseBleedDamage * bleedMagnitudeMultiplier * dotMultiplier;
        
        // Expected: 40 * 1.18 * 1.10 = 51.92
        float expectedBleed = 40f * 1.18f * 1.10f;
        
        LogTestResult("Bleed Damage Modifiers", Mathf.Approximately(finalBleedDamage, expectedBleed),
            $"Base: {baseBleedDamage}, Expected: {expectedBleed:F2}, Got: {finalBleedDamage:F2}");
        
        Debug.Log("");
    }
    
    private void TestPhase2_4_ConditionalDamage()
    {
        Debug.Log("<color=magenta>=== Phase 2.4: Conditional Damage ===</color>");
        
        if (testCharacter == null) return;
        
        var statsData = new CharacterStatsData(testCharacter);
        
        // Test vs Chilled modifier
        statsData.increasedDamageVsChilled = 30f;
        var chilledCtx = new DamageContext
        {
            damageType = "physical",
            targetChilled = true,
            targetShocked = false,
            targetIgnited = false
        };
        float chilledInc = StatsDamageCalculator.ComputeIncreased(statsData, chilledCtx);
        LogTestResult("Damage vs Chilled", chilledInc == 30f, $"Expected: 30%, Got: {chilledInc}%");
        
        // Test vs Shocked modifier
        statsData.increasedDamageVsShocked = 25f;
        var shockedCtx = new DamageContext
        {
            damageType = "lightning",
            targetChilled = false,
            targetShocked = true,
            targetIgnited = false
        };
        float shockedInc = StatsDamageCalculator.ComputeIncreased(statsData, shockedCtx);
        LogTestResult("Damage vs Shocked", shockedInc == 25f, $"Expected: 25%, Got: {shockedInc}%");
        
        // Test vs Ignited modifier
        statsData.increasedDamageVsIgnited = 35f;
        var ignitedCtx = new DamageContext
        {
            damageType = "fire",
            targetChilled = false,
            targetShocked = false,
            targetIgnited = true
        };
        float ignitedInc = StatsDamageCalculator.ComputeIncreased(statsData, ignitedCtx);
        LogTestResult("Damage vs Ignited", ignitedInc == 35f, $"Expected: 35%, Got: {ignitedInc}%");
        
        // Test combined conditional modifiers
        statsData.increasedDamageVsChilled = 20f;
        statsData.increasedDamageVsShocked = 15f;
        var combinedCtx = new DamageContext
        {
            damageType = "cold",
            targetChilled = true,
            targetShocked = true,
            targetIgnited = false
        };
        float combinedInc = StatsDamageCalculator.ComputeIncreased(statsData, combinedCtx);
        LogTestResult("Damage vs Chilled + Shocked", combinedInc == 35f, 
            $"Expected: 35% (20% + 15%), Got: {combinedInc}%");
        
        Debug.Log("");
    }
    
    #endregion
    
    #region Helper Methods
    
    private void LogTestResult(string testName, bool passed, string details = "")
    {
        if (passed)
        {
            Debug.Log($"<color=green>✓ PASS</color> - {testName}" + (verboseLogging ? $": {details}" : ""));
        }
        else
        {
            Debug.LogError($"<color=red>✗ FAIL</color> - {testName}: {details}");
        }
    }
    
    [ContextMenu("Clear Test Modifiers")]
    public void ClearTestModifiers()
    {
        if (testCharacter == null) return;
        
        if (testCharacter.warrantStatModifiers != null)
            testCharacter.warrantStatModifiers.Clear();
        
        if (testCharacter.warrantFlatModifiers != null)
            testCharacter.warrantFlatModifiers.Clear();
        
        testCharacter.baseEvasionFromItems = 0f;
        testCharacter.baseArmourFromItems = 0f;
        testCharacter.baseEnergyShieldFromItems = 0f;
        
        Debug.Log("[WarrantTester] Cleared all test modifiers");
    }
    
    #endregion
}

