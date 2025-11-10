using UnityEngine;
using System.Linq;

/// <summary>
/// Quick checker to see if weapons have proper tags
/// </summary>
public class WeaponTagChecker : MonoBehaviour
{
    [ContextMenu("Check Weapon Tags")]
    public void CheckWeaponTags()
    {
        Debug.Log("<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=cyan><b>CHECKING WEAPON TAGS</b></color>");
        Debug.Log("<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        var allItems = ItemDatabase.Instance?.GetAllItems();
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogError("No items in database!");
            return;
        }
        
        var weapons = allItems.Where(i => i.itemType == ItemType.Weapon).Take(10).ToList();
        
        Debug.Log($"Checking first {weapons.Count} weapons:\n");
        
        int withTags = 0;
        int withoutTags = 0;
        
        foreach (var weapon in weapons)
        {
            if (weapon.itemTags == null || weapon.itemTags.Count == 0)
            {
                Debug.LogError($"❌ <b>{weapon.itemName}</b> - NO TAGS!");
                withoutTags++;
            }
            else
            {
                Debug.Log($"✅ <b>{weapon.itemName}</b>");
                Debug.Log($"   Tags: <color=cyan>{string.Join(", ", weapon.itemTags)}</color>");
                withTags++;
            }
        }
        
        Debug.Log($"\n<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<b>SUMMARY:</b>");
        Debug.Log($"  Weapons WITH tags: <color=green>{withTags}</color>");
        Debug.Log($"  Weapons WITHOUT tags: <color=red>{withoutTags}</color>");
        
        if (withoutTags > 0)
        {
            Debug.LogError($"\n❌ <b>PROBLEM FOUND:</b> {withoutTags} weapons have NO TAGS!");
            Debug.LogError("   This is why affixes can't be applied to weapons!");
            Debug.LogError("\n<b>FIX:</b>");
            Debug.LogError("   1. Re-import weapons using the Weapon CSV Importer");
            Debug.LogError("   2. The updated importer applies tags automatically");
            Debug.LogError("   3. Or manually add tags to existing weapons");
        }
        else
        {
            Debug.Log("\n✅ <color=green>All weapons have tags!</color>");
            Debug.Log("   Problem must be elsewhere - check affix compatible tags.");
        }
        
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
    }
    
    [ContextMenu("Check Affix Compatible Tags (Weapon)")]
    public void CheckAffixCompatibleTags()
    {
        Debug.Log("<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log("<color=cyan><b>CHECKING AFFIX COMPATIBLE TAGS (WEAPONS)</b></color>");
        Debug.Log("<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
        
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogError("AffixDatabase not found!");
            return;
        }
        
        // Get first 10 weapon prefixes
        var weaponPrefixes = AffixDatabase_Modern.Instance.weaponPrefixCategories
            .SelectMany(c => c.GetAllAffixes())
            .Take(10)
            .ToList();
            
        Debug.Log($"Checking first {weaponPrefixes.Count} weapon prefixes:\n");
        
        int withTags = 0;
        int withoutTags = 0;
        
        foreach (var affix in weaponPrefixes)
        {
            if (affix.compatibleTags == null || affix.compatibleTags.Count == 0)
            {
                Debug.LogWarning($"⚠️ <b>{affix.name}</b> - NO compatible tags!");
                withoutTags++;
            }
            else
            {
                Debug.Log($"✅ <b>{affix.name}</b>");
                Debug.Log($"   Compatible Tags: <color=cyan>{string.Join(", ", affix.compatibleTags)}</color>");
                withTags++;
            }
        }
        
        Debug.Log($"\n<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<b>SUMMARY:</b>");
        Debug.Log($"  Affixes WITH compatible tags: <color=green>{withTags}</color>");
        Debug.Log($"  Affixes WITHOUT compatible tags: <color=yellow>{withoutTags}</color>");
        Debug.Log($"<color=cyan>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
    }
}

