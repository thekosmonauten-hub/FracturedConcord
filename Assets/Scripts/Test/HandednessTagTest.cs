using UnityEngine;

public class HandednessTagTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestHandednessTagSystem();
        }
    }
    
    [ContextMenu("Test Handedness Tag System")]
    public void TestHandednessTagSystem()
    {
        Debug.Log("=== Handedness Tag Compatibility Test ===");
        
        Debug.Log("🎯 ONE HAND VS TWO HAND AFFIXES:");
        Debug.Log("• OneHand affixes: Only compatible with one-handed weapons");
        Debug.Log("• TwoHand affixes: Only compatible with two-handed weapons");
        Debug.Log("");
        
        Debug.Log("🔧 HOW IT WORKS:");
        Debug.Log("• One-handed weapons get 'onehanded' tag");
        Debug.Log("• Two-handed weapons get 'twohanded' tag");
        Debug.Log("• OneHand affixes require 'onehanded' tag");
        Debug.Log("• TwoHand affixes require 'twohanded' tag");
        Debug.Log("");
        
        Debug.Log("📊 WEAPON HANDEDNESS EXAMPLES:");
        Debug.Log("• One-Handed: Sword, Axe, Mace, Dagger, Claw, Ritual Dagger");
        Debug.Log("• Two-Handed: Staff, Bow, Two-Handed Sword, Two-Handed Axe");
        Debug.Log("");
        
        Debug.Log("🎯 AFFIX COMPATIBILITY:");
        Debug.Log("• OneHand affix + One-handed weapon = ✅ Compatible");
        Debug.Log("• OneHand affix + Two-handed weapon = ❌ Incompatible");
        Debug.Log("• TwoHand affix + One-handed weapon = ❌ Incompatible");
        Debug.Log("• TwoHand affix + Two-handed weapon = ✅ Compatible");
        Debug.Log("");
        
        Debug.Log("💡 BENEFITS:");
        Debug.Log("• Prevents inappropriate affixes on wrong weapon types");
        Debug.Log("• Maintains game balance and logic");
        Debug.Log("• Clear separation between one-handed and two-handed affixes");
        Debug.Log("• Automatic compatibility checking in the editor");
        Debug.Log("");
        
        Debug.Log("📋 HOW TO TEST:");
        Debug.Log("1. Create a one-handed weapon (e.g., Sword)");
        Debug.Log("2. Try to add OneHand affix → Should work");
        Debug.Log("3. Try to add TwoHand affix → Should show 'No compatible affixes'");
        Debug.Log("4. Create a two-handed weapon (e.g., Staff)");
        Debug.Log("5. Try to add TwoHand affix → Should work");
        Debug.Log("6. Try to add OneHand affix → Should show 'No compatible affixes'");
    }
    
    [ContextMenu("Show Tag Examples")]
    public void ShowTagExamples()
    {
        Debug.Log("=== Handedness Tag Examples ===");
        Debug.Log("");
        Debug.Log("🏷️ WEAPON TAGS:");
        Debug.Log("• One-Handed Sword: weapon, sword, onehanded, melee, attack");
        Debug.Log("• Two-Handed Staff: weapon, staff, twohanded, spell, spell");
        Debug.Log("• One-Handed Dagger: weapon, dagger, onehanded, melee, attack");
        Debug.Log("• Two-Handed Bow: weapon, bow, twohanded, ranged, attack");
        Debug.Log("");
        Debug.Log("🎯 AFFIX REQUIRED TAGS:");
        Debug.Log("• OneHand Physical Damage: [weapon, attack, OneHand]");
        Debug.Log("• TwoHand Physical Damage: [weapon, attack, TwoHand]");
        Debug.Log("• OneHand Elemental Damage: [weapon, OneHand]");
        Debug.Log("• TwoHand Elemental Damage: [weapon, TwoHand]");
        Debug.Log("");
        Debug.Log("📁 NEW CATEGORIZATION STRUCTURE:");
        Debug.Log("Weapon Prefixes:");
        Debug.Log("├── OneHand");
        Debug.Log("│   ├── Physical Flat");
        Debug.Log("│   ├── Physical Increased");
        Debug.Log("│   ├── Fire Flat");
        Debug.Log("│   ├── Fire Increased");
        Debug.Log("│   └── [Other damage types]");
        Debug.Log("├── TwoHand");
        Debug.Log("│   ├── Physical Flat");
        Debug.Log("│   ├── Physical Increased");
        Debug.Log("│   ├── Fire Flat");
        Debug.Log("│   ├── Fire Increased");
        Debug.Log("│   └── [Other damage types]");
        Debug.Log("├── Physical (Universal)");
        Debug.Log("├── Elemental (Universal)");
        Debug.Log("└── [Other categories]");
        Debug.Log("");
        Debug.Log("✅ COMPATIBILITY MATRIX:");
        Debug.Log("OneHand Affix:");
        Debug.Log("  • One-handed weapon → ✅ Compatible");
        Debug.Log("  • Two-handed weapon → ❌ Incompatible");
        Debug.Log("");
        Debug.Log("TwoHand Affix:");
        Debug.Log("  • One-handed weapon → ❌ Incompatible");
        Debug.Log("  • Two-handed weapon → ✅ Compatible");
    }
    
    [ContextMenu("Copy Test Affix Data")]
    public void CopyTestAffixData()
    {
        string testData = @"Prefix	OneHanded	15	Adds (3-5) to (8-10) Physical Damage	OneHand	Damage, Physical, Attack	Sword, Axe, Mace, Dagger, Claw, RitualDagger	Local
Prefix	TwoHanded	20	Adds (6-8) to (12-15) Physical Damage	TwoHand	Damage, Physical, Attack	Staff, Bow	Local
Suffix	of OneHanded	12	(10-15)% increased Attack Speed	OneHand	Speed, Attack	Sword, Axe, Mace, Dagger, Claw, RitualDagger	Local
Suffix	of TwoHanded	18	(15-20)% increased Attack Speed	TwoHand	Speed, Attack	Staff, Bow	Local";
        
        GUIUtility.systemCopyBuffer = testData;
        Debug.Log("Handedness test affix data copied to clipboard!");
        Debug.Log("Use this with the bulk import system to test OneHand/TwoHand compatibility.");
    }
    
    [ContextMenu("Copy Categorization Test Data")]
    public void CopyCategorizationTestData()
    {
        string testData = @"Prefix	OneHandPhysical	15	Adds (3-5) to (8-10) Physical Damage	OneHand	Damage, Physical, Attack	Sword, Axe, Mace, Dagger, Claw, RitualDagger	Local
Prefix	OneHandFire	18	Adds (2-4) to (6-8) Fire Damage	OneHand	Damage, Fire, Attack	Sword, Axe, Mace, Dagger, Claw, RitualDagger	Local
Prefix	OneHandCold	16	Adds (2-4) to (5-7) Cold Damage	OneHand	Damage, Cold, Attack	Sword, Axe, Mace, Dagger, Claw, RitualDagger	Local
Prefix	TwoHandPhysical	25	Adds (6-8) to (12-15) Physical Damage	TwoHand	Damage, Physical, Attack	Staff, Bow	Local
Prefix	TwoHandFire	28	Adds (4-6) to (9-12) Fire Damage	TwoHand	Damage, Fire, Attack	Staff, Bow	Local
Prefix	TwoHandCold	26	Adds (4-6) to (8-11) Cold Damage	TwoHand	Damage, Cold, Attack	Staff, Bow	Local
Suffix	OneHandSpeed	12	(10-15)% increased Attack Speed	OneHand	Speed, Attack	Sword, Axe, Mace, Dagger, Claw, RitualDagger	Local
Suffix	TwoHandSpeed	18	(15-20)% increased Attack Speed	TwoHand	Speed, Attack	Staff, Bow	Local";
        
        GUIUtility.systemCopyBuffer = testData;
        Debug.Log("Categorization test data copied to clipboard!");
        Debug.Log("This will create OneHand and TwoHand categories with proper subcategories.");
        Debug.Log("Expected structure:");
        Debug.Log("• OneHand/Physical Flat");
        Debug.Log("• OneHand/Fire Flat");
        Debug.Log("• OneHand/Cold Flat");
        Debug.Log("• TwoHand/Physical Flat");
        Debug.Log("• TwoHand/Fire Flat");
        Debug.Log("• TwoHand/Cold Flat");
    }
    
    [ContextMenu("Show New Import Format")]
    public void ShowNewImportFormat()
    {
        Debug.Log("=== New Import Format ===");
        Debug.Log("");
        Debug.Log("📋 FORMAT: Affix Slot | Name | Item Level | Stat | Handedness | Tags | Weapon Types | Scope");
        Debug.Log("");
        Debug.Log("🎯 HANDEDNESS VALUES:");
        Debug.Log("• Both: Compatible with both one-handed and two-handed weapons");
        Debug.Log("• OneHand: Only compatible with one-handed weapons");
        Debug.Log("• TwoHand: Only compatible with two-handed weapons");
        Debug.Log("");
        Debug.Log("📝 EXAMPLE:");
        Debug.Log("Prefix | OneHandPhysical | 15 | Adds (3-5) to (8-10) Physical Damage | OneHand | Damage, Physical, Attack | Sword, Axe, Mace | Local");
        Debug.Log("");
        Debug.Log("💡 BENEFITS:");
        Debug.Log("• Cleaner separation of handedness from tags");
        Debug.Log("• Same affix name can exist for different handedness");
        Debug.Log("• More explicit and clear import format");
        Debug.Log("• Better organization in the database");
    }
}
