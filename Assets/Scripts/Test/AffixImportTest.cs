using UnityEngine;

public class AffixImportTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestAffixImport();
        }
    }
    
    [ContextMenu("Test Affix Import")]
    public void TestAffixImport()
    {
        Debug.Log("=== Testing Affix Import System ===");
        
        // Example data that can be copied to clipboard
        string exampleData = @"Affix Slot	Name	Item Level	Stat	Tags	Weapon Types
Suffix	of Skill	1	(5-7)% increased Attack Speed	Attack, Speed	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Humming	3	Adds 1 to (5-6) Lightning Damage	Damage, Elemental, Lightning, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of the Bear	5	(8-12)% increased Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Dagger, Claw
Prefix	Burning	8	Adds (2-4) to (6-8) Fire Damage	Damage, Elemental, Fire, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of the Eagle	12	(10-15)% increased Critical Strike Chance	Attack, Critical	Sword, Axe, Mace, Dagger, Claw, Bow
Prefix	Frozen	15	Adds (3-5) to (7-10) Cold Damage	Damage, Elemental, Cold, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of the Viper	18	(12-18)% increased Chaos Damage	Damage, Chaos, Attack	Sword, Axe, Mace, Dagger, Claw, Bow
Prefix	Thunder	22	Adds (4-6) to (8-12) Lightning Damage	Damage, Elemental, Lightning, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger";
        
        Debug.Log("Example affix data for copy-paste:");
        Debug.Log(exampleData);
        
        Debug.Log("\nTo use this system:");
        Debug.Log("1. Copy the example data above to your clipboard");
        Debug.Log("2. Select the AffixDatabase asset in the Project window");
        Debug.Log("3. Click 'Import Affixes from Clipboard' in the inspector");
        Debug.Log("4. The system will automatically parse and create the affixes");
        
        Debug.Log("\nFeatures of the import system:");
        Debug.Log("- Automatically determines affix tier based on item level");
        Debug.Log("- Parses stat descriptions to create proper modifiers");
        Debug.Log("- Organizes affixes into categories and sub-categories");
        Debug.Log("- Sets appropriate tags for compatibility");
        Debug.Log("- Supports all damage types and weapon types");
    }
    
    [ContextMenu("Copy Example Data to Clipboard")]
    public void CopyExampleDataToClipboard()
    {
        string exampleData = @"Affix Slot	Name	Item Level	Stat	Tags	Weapon Types
Suffix	of Skill	1	(5-7)% increased Attack Speed	Attack, Speed	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Prefix	Humming	3	Adds 1 to (5-6) Lightning Damage	Damage, Elemental, Lightning, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of the Bear	5	(8-12)% increased Physical Damage	Damage, Physical, Attack	Sword, Axe, Mace, Dagger, Claw
Prefix	Burning	8	Adds (2-4) to (6-8) Fire Damage	Damage, Elemental, Fire, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of the Eagle	12	(10-15)% increased Critical Strike Chance	Attack, Critical	Sword, Axe, Mace, Dagger, Claw, Bow
Prefix	Frozen	15	Adds (3-5) to (7-10) Cold Damage	Damage, Elemental, Cold, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger
Suffix	of the Viper	18	(12-18)% increased Chaos Damage	Damage, Chaos, Attack	Sword, Axe, Mace, Dagger, Claw, Bow
Prefix	Thunder	22	Adds (4-6) to (8-12) Lightning Damage	Damage, Elemental, Lightning, Attack	Sword, Axe, Mace, Staff, Wand, Sceptre, Dagger";
        
        GUIUtility.systemCopyBuffer = exampleData;
        Debug.Log("Example affix data copied to clipboard! You can now paste it into the import system.");
    }
}
