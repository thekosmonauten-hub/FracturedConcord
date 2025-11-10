using UnityEngine;
using System.Linq;

public class ArmorAffixCompatibilityDiagnostic : MonoBehaviour
{
    [ContextMenu("Diagnose Armor Affix Compatibility")]
    public void DiagnoseArmorCompatibility()
    {
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan><b>ARMOR AFFIX COMPATIBILITY DIAGNOSTIC</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>\n");
        
        // Check AffixDatabase
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogError("❌ AffixDatabase_Modern.Instance is NULL!");
            return;
        }
        
        Debug.Log("✅ AffixDatabase_Modern.Instance found\n");
        
        // Get a sample armor item
        var allArmor = Resources.LoadAll<Armour>("Armor");
        if (allArmor.Length == 0)
        {
            Debug.LogError("❌ No armor found in Resources/Armor!");
            return;
        }
        
        // Find a specific armor to test
        Armour testArmor = allArmor.FirstOrDefault(a => a.itemName == "Golden Plate");
        if (testArmor == null)
        {
            testArmor = allArmor[0]; // Use first armor if Golden Plate not found
        }
        
        Debug.Log($"<color=yellow><b>TEST ARMOR:</b></color> {testArmor.itemName}");
        Debug.Log($"  Item Type: {testArmor.itemType}");
        Debug.Log($"  Armor Slot: {testArmor.armourSlot}");
        Debug.Log($"  Armor Type: {testArmor.armourType}");
        Debug.Log($"  Base Stats: Armour={testArmor.armour}, Evasion={testArmor.evasion}, ES={testArmor.energyShield}");
        
        if (testArmor.itemTags != null && testArmor.itemTags.Count > 0)
        {
            Debug.Log($"  <color=green>Item Tags: [{string.Join(", ", testArmor.itemTags)}]</color>");
        }
        else
        {
            Debug.LogError("  ❌ <b>ARMOR HAS NO TAGS!</b>");
            Debug.LogError("     This is why affixes can't match!");
        }
        
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>CHECKING ARMOR SUFFIXES IN DATABASE:</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        // Check first 10 armor suffixes and their tags
        int suffixCount = 0;
        int suffixesWithTags = 0;
        int suffixesWithoutTags = 0;
        
        foreach (var category in AffixDatabase_Modern.Instance.armourSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                foreach (var affix in subCategory.affixes)
                {
                    if (suffixCount < 10) // Show first 10
                    {
                        bool hasTags = affix.compatibleTags != null && affix.compatibleTags.Count > 0;
                        
                        if (hasTags)
                        {
                            Debug.Log($"  ✅ {affix.name}");
                            Debug.Log($"     Description: {affix.description}");
                            Debug.Log($"     Compatible Tags: [{string.Join(", ", affix.compatibleTags)}]");
                            suffixesWithTags++;
                        }
                        else
                        {
                            Debug.LogError($"  ❌ {affix.name} - <b>NO TAGS!</b>");
                            Debug.LogError($"     Description: {affix.description}");
                            suffixesWithoutTags++;
                        }
                        
                        suffixCount++;
                    }
                }
            }
        }
        
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>SUFFIX TAG SUMMARY:</b></color>");
        Debug.Log($"  Suffixes WITH tags: <color=green>{suffixesWithTags}</color> out of {suffixCount} checked");
        Debug.Log($"  Suffixes WITHOUT tags: <color=red>{suffixesWithoutTags}</color> out of {suffixCount} checked");
        
        // Count ALL armor suffixes
        int totalSuffixes = 0;
        int totalWithoutTags = 0;
        
        foreach (var category in AffixDatabase_Modern.Instance.armourSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                foreach (var affix in subCategory.affixes)
                {
                    totalSuffixes++;
                    if (affix.compatibleTags == null || affix.compatibleTags.Count == 0)
                    {
                        totalWithoutTags++;
                    }
                }
            }
        }
        
        Debug.Log($"\n  <b>TOTAL ARMOR SUFFIXES: {totalSuffixes}</b>");
        Debug.Log($"  <b>Suffixes without tags: <color=red>{totalWithoutTags}</color></b>");
        
        if (totalWithoutTags > 0)
        {
            Debug.LogError("\n❌ <b>PROBLEM IDENTIFIED:</b>");
            Debug.LogError($"   {totalWithoutTags} armor suffixes have NO TAGS!");
            Debug.LogError("   This is why suffixes can't roll on armor!");
            Debug.LogError("\n<b>SOLUTION:</b>");
            Debug.LogError("   1. Unity Menu → Dexiled → Import Affixes from CSV");
            Debug.LogError("   2. Select: Comprehensive_Mods.csv");
            Debug.LogError("   3. Select: AffixDatabase.asset (the modern one)");
            Debug.LogError("   4. Click: Import Affixes");
            Debug.LogError("   5. This will populate all affix tags!");
        }
        else
        {
            Debug.Log("\n✅ <color=green>All armor suffixes have tags!</color>");
        }
        
        Debug.Log("\n<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan><b>DIAGNOSTIC COMPLETE</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
    }
}








