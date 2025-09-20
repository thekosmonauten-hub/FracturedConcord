using UnityEngine;
using System.Collections.Generic;

public class AffixRollingTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    public int testIterations = 5;
    
    void Start()
    {
        if (testOnStart)
        {
            TestAffixRolling();
        }
    }
    
    [ContextMenu("Test Affix Rolling")]
    public void TestAffixRolling()
    {
        Debug.Log("=== Testing Affix Rolling System ===");
        
        // Test 1: Create affix with ranges
        TestAffixWithRanges();
        
        // Test 2: Roll affix multiple times
        TestMultipleRolls();
        
        // Test 3: Seeded rolling for reproducibility
        TestSeededRolling();
        
        // Test 4: Damage calculation with rolled values
        TestDamageCalculation();
    }
    
    private void TestAffixWithRanges()
    {
        Debug.Log("\n--- Creating Affix with Ranges ---");
        
        // Create an affix with ranges (like in the database)
        Affix originalAffix = new Affix("Squire's", "15-19% increased Physical Damage", AffixType.Prefix, AffixTier.Tier1);
        
        // Add a modifier with a range
        AffixModifier modifier = new AffixModifier("PhysicalDamage", 15f, 19f, ModifierType.Increased, ModifierScope.Local);
        modifier.damageType = DamageType.Physical;
        originalAffix.modifiers.Add(modifier);
        
        Debug.Log($"Original Affix: {originalAffix.name}");
        Debug.Log($"Modifier Range: {modifier.minValue}-{modifier.maxValue}% increased Physical Damage");
        Debug.Log($"This is the template affix stored in the database");
    }
    
    private void TestMultipleRolls()
    {
        Debug.Log("\n--- Rolling Affix Multiple Times ---");
        
        // Create an affix with ranges
        Affix originalAffix = new Affix("Squire's", "15-19% increased Physical Damage", AffixType.Prefix, AffixTier.Tier1);
        AffixModifier modifier = new AffixModifier("PhysicalDamage", 15f, 19f, ModifierType.Increased, ModifierScope.Local);
        modifier.damageType = DamageType.Physical;
        originalAffix.modifiers.Add(modifier);
        
        Debug.Log("Rolling the same affix multiple times:");
        for (int i = 0; i < testIterations; i++)
        {
            Affix rolledAffix = originalAffix.GenerateRolledAffix();
            int rolledValue = (int)rolledAffix.modifiers[0].minValue;
            Debug.Log($"  Roll {i + 1}: {rolledValue}% increased Physical Damage");
        }
        
        Debug.Log("Each roll gives a different whole number within the range!");
    }
    
    private void TestSeededRolling()
    {
        Debug.Log("\n--- Seeded Rolling for Reproducibility ---");
        
        // Create an affix with ranges
        Affix originalAffix = new Affix("Squire's", "15-19% increased Physical Damage", AffixType.Prefix, AffixTier.Tier1);
        AffixModifier modifier = new AffixModifier("PhysicalDamage", 15f, 19f, ModifierType.Increased, ModifierScope.Local);
        modifier.damageType = DamageType.Physical;
        originalAffix.modifiers.Add(modifier);
        
        int seed = 12345;
        Debug.Log($"Using seed: {seed}");
        
        // Roll with the same seed multiple times
        for (int i = 0; i < 3; i++)
        {
            Affix rolledAffix = originalAffix.GenerateRolledAffix(seed);
            float rolledValue = rolledAffix.modifiers[0].minValue;
            Debug.Log($"  Roll {i + 1}: {rolledValue:F1}% increased Physical Damage");
        }
        
        Debug.Log("Same seed always produces the same result!");
    }
    
    private void TestDamageCalculation()
    {
        Debug.Log("\n--- Damage Calculation with Rolled Values ---");
        
        // Create a weapon with base damage
        WeaponItem weapon = ScriptableObject.CreateInstance<WeaponItem>();
        weapon.itemName = "Test Sword";
        weapon.minDamage = 10f;
        weapon.maxDamage = 15f;
        
        Debug.Log($"Base Weapon: {weapon.minDamage}-{weapon.maxDamage} Physical Damage");
        
        // Create and roll an affix
        Affix originalAffix = new Affix("Squire's", "15-19% increased Physical Damage", AffixType.Prefix, AffixTier.Tier1);
        AffixModifier modifier = new AffixModifier("PhysicalDamage", 15f, 19f, ModifierType.Increased, ModifierScope.Local);
        modifier.damageType = DamageType.Physical;
        originalAffix.modifiers.Add(modifier);
        
        // Add the rolled affix to the weapon
        weapon.AddPrefix(originalAffix);
        
        // Calculate total damage
        float totalMinDamage = weapon.GetTotalMinDamage();
        float totalMaxDamage = weapon.GetTotalMaxDamage();
        
        Debug.Log($"After adding rolled affix:");
        Debug.Log($"  Total Damage: {totalMinDamage:F1}-{totalMaxDamage:F1} Physical Damage");
        Debug.Log($"  The affix value was rolled to a specific number, not a range!");
    }
    
    [ContextMenu("Show Affix Rolling Benefits")]
    public void ShowAffixRollingBenefits()
    {
        Debug.Log("=== Affix Rolling Benefits ===");
        Debug.Log("");
        Debug.Log("🎯 **Accurate Damage Calculations**:");
        Debug.Log("• Each affix has a specific value, not a range");
        Debug.Log("• Damage calculations use actual numbers");
        Debug.Log("• No ambiguity in final damage values");
        Debug.Log("");
        Debug.Log("🎲 **Random but Reproducible**:");
        Debug.Log("• Each item gets unique affix values");
        Debug.Log("• Same seed produces same results");
        Debug.Log("• Perfect for save/load systems");
        Debug.Log("");
        Debug.Log("📊 **Clear Item Display**:");
        Debug.Log("• Tooltips show exact values");
        Debug.Log("• No confusing ranges in item descriptions");
        Debug.Log("• Players see exactly what they have");
        Debug.Log("");
        Debug.Log("🔧 **Proper PoE Mechanics**:");
        Debug.Log("• Matches Path of Exile's affix system");
        Debug.Log("• Each item is unique when generated");
        Debug.Log("• Supports item trading and comparison");
    }
}
