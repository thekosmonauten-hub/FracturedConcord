using UnityEngine;

public class DualRangeTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestDualRangeSystem();
        }
    }
    
    [ContextMenu("Test Dual Range System")]
    public void TestDualRangeSystem()
    {
        Debug.Log("=== Dual Range Modifier Test ===");
        
        Debug.Log("🎯 DUAL RANGE MODIFIERS:");
        Debug.Log("• Polished: Adds (6-9) to (13-15) Physical Damage");
        Debug.Log("• Honed: Adds (8-12) to (17-20) Physical Damage");
        Debug.Log("• Gleaming: Adds (11-14) to (21-25) Physical Damage");
        Debug.Log("• Annealed: Adds (13-18) to (27-31) Physical Damage");
        Debug.Log("");
        
        Debug.Log("🔧 HOW IT WORKS:");
        Debug.Log("• Each range is rolled independently");
        Debug.Log("• First range: (6-9) rolls to a value like 7");
        Debug.Log("• Second range: (13-15) rolls to a value like 14");
        Debug.Log("• Final result: Adds 7 to 14 Physical Damage");
        Debug.Log("");
        
        Debug.Log("📊 SLIDER CONTROLS:");
        Debug.Log("• Min Value Slider: Controls the first range (6-9)");
        Debug.Log("• Max Value Slider: Controls the second range (13-15)");
        Debug.Log("• Randomize Both: Rolls both ranges independently");
        Debug.Log("• Range Display: Shows original ranges for reference");
        Debug.Log("");
        
        Debug.Log("💡 BENEFITS:");
        Debug.Log("• Accurate Path of Exile-style dual ranges");
        Debug.Log("• Independent rolling of min and max values");
        Debug.Log("• Precise control over both damage bounds");
        Debug.Log("• Maintains data integrity and range constraints");
        Debug.Log("");
        
        Debug.Log("📋 HOW TO TEST:");
        Debug.Log("1. Import dual-range affixes via bulk import");
        Debug.Log("2. Apply them to weapons");
        Debug.Log("3. Check that both ranges are rolled independently");
        Debug.Log("4. Use sliders to adjust both min and max values");
        Debug.Log("5. Verify that ranges are properly constrained");
    }
    
    [ContextMenu("Copy Dual Range Test Data")]
    public void CopyDualRangeTestData()
    {
        string testData = @"Prefix	Polished	21	Adds (6-9) to (13-15) Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Honed	29	Adds (8-12) to (17-20) Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Gleaming	36	Adds (11-14) to (21-25) Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Annealed	46	Adds (13-18) to (27-31) Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Razor-sharp	54	Adds (16-21) to (32-38) Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Tempered	65	Adds (19-25) to (39-45) Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Flaring	77	Adds (22-29) to (45-52) Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger";
        
        GUIUtility.systemCopyBuffer = testData;
        Debug.Log("Dual range test data copied to clipboard!");
        Debug.Log("Use this with the bulk import system to test dual-range modifiers.");
    }
    
    [ContextMenu("Show Dual Range Examples")]
    public void ShowDualRangeExamples()
    {
        Debug.Log("=== Dual Range Examples ===");
        Debug.Log("");
        Debug.Log("📊 PHYSICAL DAMAGE DUAL RANGES:");
        Debug.Log("• Glinting: 1 to (2-3) → Single range");
        Debug.Log("• Burnished: (4-5) to (8-9) → Dual range");
        Debug.Log("• Polished: (6-9) to (13-15) → Dual range");
        Debug.Log("• Honed: (8-12) to (17-20) → Dual range");
        Debug.Log("• Gleaming: (11-14) to (21-25) → Dual range");
        Debug.Log("• Annealed: (13-18) to (27-31) → Dual range");
        Debug.Log("• Razor-sharp: (16-21) to (32-38) → Dual range");
        Debug.Log("• Tempered: (19-25) to (39-45) → Dual range");
        Debug.Log("• Flaring: (22-29) to (45-52) → Dual range");
        Debug.Log("");
        Debug.Log("🎯 ROLLING EXAMPLES:");
        Debug.Log("• Polished (6-9) to (13-15):");
        Debug.Log("  - First roll: 7 (from 6-9)");
        Debug.Log("  - Second roll: 14 (from 13-15)");
        Debug.Log("  - Result: Adds 7 to 14 Physical Damage");
        Debug.Log("");
        Debug.Log("• Honed (8-12) to (17-20):");
        Debug.Log("  - First roll: 10 (from 8-12)");
        Debug.Log("  - Second roll: 18 (from 17-20)");
        Debug.Log("  - Result: Adds 10 to 18 Physical Damage");
    }
}
