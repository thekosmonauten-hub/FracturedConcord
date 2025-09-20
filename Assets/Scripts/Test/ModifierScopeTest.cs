using UnityEngine;
using System.Collections.Generic;

public class ModifierScopeTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestModifierScopes();
        }
    }
    
    [ContextMenu("Test Modifier Scopes")]
    public void TestModifierScopes()
    {
        Debug.Log("=== Testing Modifier Scope System ===");
        
        // Test 1: Local vs Global modifiers
        TestLocalVsGlobalModifiers();
        
        // Test 2: Scope determination examples
        TestScopeDetermination();
        
        // Test 3: Damage calculation implications
        TestDamageCalculationImplications();
    }
    
    private void TestLocalVsGlobalModifiers()
    {
        Debug.Log("\n--- Local vs Global Modifiers ---");
        
        // Example local modifiers (affect weapon base properties)
        var localModifiers = new List<string>
        {
            "Adds 5-10 Physical Damage",
            "20% increased Attack Speed", 
            "15% increased Critical Strike Chance",
            "Adds 3-6 Fire Damage",
            "Adds 2-4 Lightning Damage"
        };
        
        Debug.Log("LOCAL Modifiers (affect item base properties):");
        foreach (var modifier in localModifiers)
        {
            Debug.Log($"  🔧 {modifier}");
        }
        
        // Example global modifiers (affect character stats)
        var globalModifiers = new List<string>
        {
            "30% increased Physical Damage (from ring)",
            "25% more Attack Speed (from skill)",
            "20% increased Critical Strike Multiplier",
            "15% increased Movement Speed",
            "10% increased Experience Gain"
        };
        
        Debug.Log("\nGLOBAL Modifiers (affect character stats):");
        foreach (var modifier in globalModifiers)
        {
            Debug.Log($"  🌍 {modifier}");
        }
    }
    
    private void TestScopeDetermination()
    {
        Debug.Log("\n--- Scope Determination Examples ---");
        
        var testCases = new Dictionary<string, ModifierScope>
        {
            {"Adds 5-10 Physical Damage", ModifierScope.Local},
            {"20% increased Physical Damage", ModifierScope.Local},
            {"15% increased Attack Speed", ModifierScope.Local},
            {"Adds 3-6 Fire Damage", ModifierScope.Local},
            {"30% increased Physical Damage to Attacks", ModifierScope.Global},
            {"25% more Attack Speed", ModifierScope.Global},
            {"20% increased Critical Strike Multiplier", ModifierScope.Global},
            {"15% increased Movement Speed", ModifierScope.Global}
        };
        
        foreach (var testCase in testCases)
        {
            string scopeText = testCase.Value == ModifierScope.Local ? "🔧 LOCAL" : "🌍 GLOBAL";
            Debug.Log($"{scopeText}: {testCase.Key}");
        }
    }
    
    private void TestDamageCalculationImplications()
    {
        Debug.Log("\n--- Damage Calculation Implications ---");
        
        Debug.Log("Example: Sword with local and global modifiers");
        Debug.Log("Base Sword: 10-15 Physical Damage");
        Debug.Log("");
        
        Debug.Log("LOCAL modifiers (applied to weapon base):");
        Debug.Log("  + Adds 5-10 Physical Damage (Local)");
        Debug.Log("  + 20% increased Physical Damage (Local)");
        Debug.Log("  Weapon base becomes: 18-30 Physical Damage");
        Debug.Log("");
        
        Debug.Log("GLOBAL modifiers (applied to character):");
        Debug.Log("  + 30% increased Physical Damage (Global)");
        Debug.Log("  + 25% more Physical Damage (Global)");
        Debug.Log("  Final damage: 18-30 * 1.3 * 1.25 = 29.25-48.75");
        Debug.Log("");
        
        Debug.Log("Key Points:");
        Debug.Log("• Local modifiers affect the weapon's base damage");
        Debug.Log("• Global modifiers affect the character's total damage");
        Debug.Log("• Local modifiers are applied first, then global modifiers");
        Debug.Log("• This creates the PoE-style damage calculation system");
    }
    
    [ContextMenu("Show Scope Rules")]
    public void ShowScopeRules()
    {
        Debug.Log("=== Modifier Scope Rules ===");
        Debug.Log("");
        Debug.Log("LOCAL Modifiers (affect item base properties):");
        Debug.Log("• Unconditional modifiers that affect innate item properties");
        Debug.Log("• Hand-specific modifiers (\"with this Weapon\")");
        Debug.Log("• Weapon hit effects unrelated to numeric damage");
        Debug.Log("• Examples: Added damage, increased attack speed, accuracy");
        Debug.Log("");
        Debug.Log("GLOBAL Modifiers (affect character stats):");
        Debug.Log("• All conditional modifiers");
        Debug.Log("• Modifiers that don't affect item base stats");
        Debug.Log("• Modifiers that state \"Global\"");
        Debug.Log("• Examples: Increased damage from rings, more multipliers");
        Debug.Log("");
        Debug.Log("Calculation Order:");
        Debug.Log("1. Apply LOCAL modifiers to item base stats");
        Debug.Log("2. Apply GLOBAL modifiers to character total stats");
        Debug.Log("3. This creates the PoE-style damage calculation system");
    }
}
