using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ArmorLevelDiagnostic : MonoBehaviour
{
    [Header("Test Settings")]
    public int testAreaLevel = 45;
    
    [ContextMenu("Diagnose Armor Level Distribution")]
    public void DiagnoseArmorLevels()
    {
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log($"<color=cyan><b>ARMOR LEVEL DISTRIBUTION - Area Level {testAreaLevel}</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>\n");
        
        // Calculate valid level range for this area
        int minLevel = Mathf.Max(1, testAreaLevel - 25);
        int maxLevel = testAreaLevel + 2;
        
        Debug.Log($"<color=yellow>Valid item level range for Area Level {testAreaLevel}: {minLevel}-{maxLevel}</color>\n");
        
        // Load all armor
        var allArmor = Resources.LoadAll<Armour>("Armor");
        
        if (allArmor.Length == 0)
        {
            Debug.LogError("❌ No armor found in Resources/Armor!");
            return;
        }
        
        Debug.Log($"✅ Total armor items in database: <color=green>{allArmor.Length}</color>");
        
        // Filter by level range
        var eligibleArmor = allArmor.Where(a => a.requiredLevel >= minLevel && a.requiredLevel <= maxLevel).ToList();
        
        Debug.Log($"✅ Armor items in valid level range ({minLevel}-{maxLevel}): <color=green>{eligibleArmor.Count}</color>\n");
        
        if (eligibleArmor.Count == 0)
        {
            Debug.LogError($"❌ <b>NO ARMOR ITEMS IN RANGE {minLevel}-{maxLevel}!</b>");
            Debug.LogError("   This is why generation is failing!");
            
            // Show closest items
            var belowRange = allArmor.Where(a => a.requiredLevel < minLevel).OrderByDescending(a => a.requiredLevel).Take(5);
            var aboveRange = allArmor.Where(a => a.requiredLevel > maxLevel).OrderBy(a => a.requiredLevel).Take(5);
            
            Debug.Log("\n<color=yellow>Closest armor BELOW range:</color>");
            foreach (var armor in belowRange)
            {
                Debug.Log($"  Level {armor.requiredLevel}: {armor.itemName}");
            }
            
            Debug.Log("\n<color=yellow>Closest armor ABOVE range:</color>");
            foreach (var armor in aboveRange)
            {
                Debug.Log($"  Level {armor.requiredLevel}: {armor.itemName}");
            }
            
            return;
        }
        
        // Show distribution by armor slot
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>ARMOR DISTRIBUTION BY SLOT:</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        var groupedBySlot = eligibleArmor.GroupBy(a => a.armourSlot);
        
        foreach (var group in groupedBySlot.OrderBy(g => g.Key))
        {
            Debug.Log($"<color=cyan><b>{group.Key}:</b></color> {group.Count()} items");
            
            // Show first 5 items per slot
            foreach (var armor in group.Take(5))
            {
                string tagsInfo = armor.itemTags != null && armor.itemTags.Count > 0 
                    ? $"[{string.Join(", ", armor.itemTags)}]" 
                    : "<color=red>[NO TAGS]</color>";
                    
                Debug.Log($"  Level {armor.requiredLevel}: {armor.itemName} {tagsInfo}");
            }
            
            if (group.Count() > 5)
            {
                Debug.Log($"  ... and {group.Count() - 5} more");
            }
            
            Debug.Log("");
        }
        
        // Show distribution by armor type (Armour/Evasion/ES base)
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>ARMOR DISTRIBUTION BY TYPE:</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        var groupedByType = eligibleArmor.GroupBy(a => a.armourType);
        
        foreach (var group in groupedByType.OrderBy(g => g.Key))
        {
            Debug.Log($"<color=cyan><b>{group.Key}:</b></color> {group.Count()} items");
        }
        
        // Check for items without tags
        Debug.Log("\n<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=yellow><b>TAG STATUS:</b></color>");
        Debug.Log("<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        int withTags = eligibleArmor.Count(a => a.itemTags != null && a.itemTags.Count > 0);
        int withoutTags = eligibleArmor.Count - withTags;
        
        Debug.Log($"Armor WITH tags: <color=green>{withTags}</color> / {eligibleArmor.Count}");
        Debug.Log($"Armor WITHOUT tags: <color=red>{withoutTags}</color> / {eligibleArmor.Count}");
        
        if (withoutTags > 0)
        {
            Debug.LogWarning($"\n⚠️ {withoutTags} armor items have no tags!");
            Debug.LogWarning("   Affixes won't be able to roll on these items!");
            Debug.LogWarning("   Solution: Re-import armor using the ArmorCSVImporter");
        }
        
        Debug.Log("\n<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan><b>DIAGNOSTIC COMPLETE</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
    }
}







