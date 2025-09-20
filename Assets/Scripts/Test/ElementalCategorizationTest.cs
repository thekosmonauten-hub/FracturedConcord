using UnityEngine;
using System.Collections.Generic;

public class ElementalCategorizationTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestElementalCategorization();
        }
    }
    
    [ContextMenu("Test Elemental Categorization")]
    public void TestElementalCategorization()
    {
        Debug.Log("=== Elemental Categorization Test ===");
        
        // Test data that should be properly categorized
        var testData = new List<string>
        {
            "Prefix\tHumming\t3\tAdds 1 to (5-6) Lightning Damage\tDamage, Elemental, Lightning, Attack\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger",
            "Prefix\tBurning\t5\tAdds (2-4) to (6-8) Fire Damage\tDamage, Elemental, Fire, Attack\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger",
            "Prefix\tFrozen\t7\tAdds (3-5) to (7-9) Cold Damage\tDamage, Elemental, Cold, Attack\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger",
            "Prefix\tCorrupted\t10\tAdds (4-6) to (8-10) Chaos Damage\tDamage, Elemental, Chaos, Attack\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger",
            "Suffix\tof Lightning\t2\t(8-12)% increased Lightning Damage\tDamage, Elemental, Lightning\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger",
            "Suffix\tof Fire\t4\t(10-15)% increased Fire Damage\tDamage, Elemental, Fire\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger",
            "Suffix\tof Cold\t6\t(12-18)% increased Cold Damage\tDamage, Elemental, Cold\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger",
            "Suffix\tof Chaos\t8\t(15-20)% increased Chaos Damage\tDamage, Elemental, Chaos\tSword, Axe, Mace, Staff, Wand, Sceptre, Dagger"
        };
        
        Debug.Log("Expected Categorization (IMPROVED):");
        Debug.Log("Weapon Prefixes > Elemental > Lightning Flat: Humming");
        Debug.Log("Weapon Prefixes > Elemental > Fire Flat: Burning");
        Debug.Log("Weapon Prefixes > Elemental > Cold Flat: Frozen");
        Debug.Log("Weapon Prefixes > Elemental > Chaos Flat: Corrupted");
        Debug.Log("Weapon Suffixes > Elemental > Lightning Increased: of Lightning");
        Debug.Log("Weapon Suffixes > Elemental > Fire Increased: of Fire");
        Debug.Log("Weapon Suffixes > Elemental > Cold Increased: of Cold");
        Debug.Log("Weapon Suffixes > Elemental > Chaos Increased: of Chaos");
        
        Debug.Log("\n🎯 GRANULAR CATEGORIZATION:");
        Debug.Log("• Flat damage: 'Fire Flat', 'Cold Flat', 'Lightning Flat', 'Chaos Flat'");
        Debug.Log("• Increased damage: 'Fire Increased', 'Cold Increased', 'Lightning Increased', 'Chaos Increased'");
        Debug.Log("• More damage: 'Fire More', 'Cold More', etc.");
        Debug.Log("• Reduced damage: 'Fire Reduced', 'Cold Reduced', etc.");
        
        Debug.Log("\nTo test this:");
        Debug.Log("1. Copy the test data above to your clipboard");
        Debug.Log("2. Select AffixDatabase in the Project window");
        Debug.Log("3. Click 'Import Affixes from Clipboard'");
        Debug.Log("4. Check that elemental affixes are properly categorized with modifier types!");
        
        Debug.Log("\nNote: The system now combines elemental types with modifier types for precise categorization.");
        Debug.Log("This ensures Flat and Increased damage are properly separated within each element.");
    }
    
    [ContextMenu("Copy Test Data to Clipboard")]
    public void CopyTestDataToClipboard()
    {
        string testData = @"Prefix	Humming	3	Adds 1 to (5-6) Lightning Damage	Damage, Elemental, Lightning, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Burning	5	Adds (2-4) to (6-8) Fire Damage	Damage, Elemental, Fire, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Frozen	7	Adds (3-5) to (7-9) Cold Damage	Damage, Elemental, Cold, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Corrupted	10	Adds (4-6) to (8-10) Chaos Damage	Damage, Elemental, Chaos, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of Lightning	2	(8-12)% increased Lightning Damage	Damage, Elemental, Lightning	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of Fire	4	(10-15)% increased Fire Damage	Damage, Elemental, Fire	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of Cold	6	(12-18)% increased Cold Damage	Damage, Elemental, Cold	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of Chaos	8	(15-20)% increased Chaos Damage	Damage, Elemental, Chaos	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger";
        
        GUIUtility.systemCopyBuffer = testData;
        Debug.Log("Test data copied to clipboard! Use this with the bulk import system.");
    }
}
