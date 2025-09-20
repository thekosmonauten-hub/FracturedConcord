using UnityEngine;

public class UnifiedItemSystemTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    public bool testAffixGeneration = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestUnifiedItemSystem();
        }
    }
    
    [ContextMenu("Test Unified Item System")]
    public void TestUnifiedItemSystem()
    {
        Debug.Log("=== Testing Unified Item System ===");
        
        // Test 1: Create a weapon with the new structure
        TestWeaponStructure();
        
        // Test 2: Test affix generation
        if (testAffixGeneration)
        {
            TestAffixGeneration();
        }
        
        // Test 3: Test tooltip display
        TestTooltipDisplay();
    }
    
    private void TestWeaponStructure()
    {
        Debug.Log("--- Testing Weapon Structure ---");
        
        // Create a test weapon
        WeaponItem testWeapon = ScriptableObject.CreateInstance<WeaponItem>();
        testWeapon.itemName = "Test Sword";
        testWeapon.description = "A test sword for the unified system";
        testWeapon.weaponType = WeaponItemType.Sword;
        testWeapon.handedness = WeaponHandedness.OneHanded;
        testWeapon.minDamage = 10f;
        testWeapon.maxDamage = 15f;
        testWeapon.attackSpeed = 1.2f;
        testWeapon.criticalStrikeChance = 5f;
        testWeapon.primaryDamageType = DamageType.Physical;
        testWeapon.requiredLevel = 1;
        testWeapon.requiredStrength = 10;
        testWeapon.itemType = ItemType.Weapon;
        testWeapon.equipmentType = EquipmentType.MainHand;
        
        // Add implicit modifier (fixed, always present)
        Affix implicitAffix = new Affix("Sharp", "+10% increased Physical Damage", AffixType.Prefix, AffixTier.Tier9);
        AffixModifier implicitMod = new AffixModifier("PhysicalDamage", 10, 10, ModifierType.Increased);
        implicitMod.damageType = DamageType.Physical;
        implicitAffix.modifiers.Add(implicitMod);
        testWeapon.implicitModifiers.Add(implicitAffix);
        
        // Test base properties
        Debug.Log($"Weapon: {testWeapon.GetDisplayName()}");
        Debug.Log($"Base Damage: {testWeapon.minDamage}-{testWeapon.maxDamage}");
        Debug.Log($"Attack Speed: {testWeapon.attackSpeed}");
        Debug.Log($"Implicit Modifiers: {testWeapon.implicitModifiers.Count}");
        
        // Test total damage calculation
        float totalMinDamage = testWeapon.GetTotalMinDamage();
        float totalMaxDamage = testWeapon.GetTotalMaxDamage();
        Debug.Log($"Total Damage: {totalMinDamage}-{totalMaxDamage}");
        
        // Test description
        string description = testWeapon.GetFullDescription();
        Debug.Log($"Full Description:\n{description}");
        
        // Cleanup
        DestroyImmediate(testWeapon);
    }
    
    private void TestAffixGeneration()
    {
        Debug.Log("--- Testing Affix Generation ---");
        
        if (AffixDatabase.Instance == null)
        {
            Debug.LogError("AffixDatabase not found! Please create it first.");
            return;
        }
        
        // Create a test weapon
        WeaponItem testWeapon = ScriptableObject.CreateInstance<WeaponItem>();
        testWeapon.itemName = "Random Sword";
        testWeapon.weaponType = WeaponItemType.Sword;
        testWeapon.itemType = ItemType.Weapon;
        testWeapon.equipmentType = EquipmentType.MainHand;
        testWeapon.itemTags.Add("weapon");
        testWeapon.itemTags.Add("attack");
        testWeapon.itemTags.Add("physical");
        
        // Generate random affixes
        AffixDatabase.Instance.GenerateRandomAffixes(testWeapon, 50, 0.3f, 0.1f);
        
        Debug.Log($"Generated weapon: {testWeapon.GetDisplayName()}");
        Debug.Log($"Rarity: {testWeapon.GetCalculatedRarity()}");
        Debug.Log($"Prefixes: {testWeapon.prefixes.Count}");
        Debug.Log($"Suffixes: {testWeapon.suffixes.Count}");
        
        // Show generated affixes
        foreach (var prefix in testWeapon.prefixes)
        {
            Debug.Log($"  Prefix: {prefix.name} - {prefix.description}");
        }
        
        foreach (var suffix in testWeapon.suffixes)
        {
            Debug.Log($"  Suffix: {suffix.name} - {suffix.description}");
        }
        
        // Test total stats
        float physicalDamage = testWeapon.GetModifierValue("PhysicalDamage");
        Debug.Log($"Total Physical Damage Modifier: {physicalDamage}%");
        
        // Cleanup
        DestroyImmediate(testWeapon);
    }
    
    private void TestTooltipDisplay()
    {
        Debug.Log("--- Testing Tooltip Display ---");
        
        // Create a weapon with all types of modifiers
        WeaponItem testWeapon = ScriptableObject.CreateInstance<WeaponItem>();
        testWeapon.itemName = "Epic Test Sword";
        testWeapon.description = "A powerful test sword";
        testWeapon.weaponType = WeaponItemType.Sword;
        testWeapon.minDamage = 20f;
        testWeapon.maxDamage = 30f;
        testWeapon.attackSpeed = 1.5f;
        testWeapon.criticalStrikeChance = 8f;
        testWeapon.primaryDamageType = DamageType.Physical;
        testWeapon.requiredLevel = 10;
        testWeapon.requiredStrength = 25;
        testWeapon.quality = 5; // Superior quality
        testWeapon.itemType = ItemType.Weapon;
        testWeapon.equipmentType = EquipmentType.MainHand;
        
        // Add implicit modifier
        Affix implicitAffix = new Affix("Mighty", "+15% increased Physical Damage", AffixType.Prefix, AffixTier.Tier5);
        AffixModifier implicitMod = new AffixModifier("PhysicalDamage", 15, 15, ModifierType.Increased);
        implicitAffix.modifiers.Add(implicitMod);
        testWeapon.implicitModifiers.Add(implicitAffix);
        
        // Add random prefixes and suffixes
        if (AffixDatabase.Instance != null)
        {
            AffixDatabase.Instance.GenerateRandomAffixes(testWeapon, 20, 0.5f, 0.2f);
        }
        
        // Display the full tooltip
        string tooltip = testWeapon.GetFullDescription();
        Debug.Log($"Complete Tooltip:\n{tooltip}");
        
        // Test total stats
        Debug.Log($"Total Min Damage: {testWeapon.GetTotalMinDamage()}");
        Debug.Log($"Total Max Damage: {testWeapon.GetTotalMaxDamage()}");
        Debug.Log($"Total Attack Speed: {testWeapon.GetTotalAttackSpeed()}");
        Debug.Log($"Total Crit Chance: {testWeapon.GetTotalCriticalStrikeChance()}");
        
        // Cleanup
        DestroyImmediate(testWeapon);
    }
}
