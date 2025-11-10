using UnityEngine;
using System.Linq;

public class AffixTagChecker : MonoBehaviour
{
    [ContextMenu("Check Affix Tags")]
    public void CheckAffixTags()
    {
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
        Debug.Log("<color=cyan><b>AFFIX TAG CHECKER</b></color>");
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
        
        if (AffixDatabase_Modern.Instance == null)
        {
            Debug.LogError("❌ AffixDatabase_Modern.Instance not found!");
            return;
        }
        
        Debug.Log("✅ AffixDatabase found");
        Debug.Log("");
        
        // Check a sample of armor prefixes
        Debug.Log("<color=yellow><b>CHECKING ARMOR PREFIXES:</b></color>");
        int sampleCount = 0;
        foreach (var category in AffixDatabase_Modern.Instance.armourPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                foreach (var affix in subCategory.affixes)
                {
                    if (sampleCount < 5) // Show first 5
                    {
                        string tagsStr = affix.compatibleTags != null && affix.compatibleTags.Count > 0
                            ? string.Join(", ", affix.compatibleTags)
                            : "<color=red>NO TAGS!</color>";
                        
                        Debug.Log($"  • {affix.name}");
                        Debug.Log($"    Tags: [{tagsStr}]");
                        Debug.Log($"    Stat: {affix.description}");
                        sampleCount++;
                    }
                }
            }
        }
        
        Debug.Log("");
        
        // Check a sample of armor suffixes
        Debug.Log("<color=yellow><b>CHECKING ARMOR SUFFIXES:</b></color>");
        sampleCount = 0;
        foreach (var category in AffixDatabase_Modern.Instance.armourSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                foreach (var affix in subCategory.affixes)
                {
                    if (sampleCount < 5) // Show first 5
                    {
                        string tagsStr = affix.compatibleTags != null && affix.compatibleTags.Count > 0
                            ? string.Join(", ", affix.compatibleTags)
                            : "<color=red>NO TAGS!</color>";
                        
                        Debug.Log($"  • {affix.name}");
                        Debug.Log($"    Tags: [{tagsStr}]");
                        Debug.Log($"    Stat: {affix.description}");
                        sampleCount++;
                    }
                }
            }
        }
        
        Debug.Log("");
        
        // Count affixes with NO tags
        int noTagsCount = 0;
        int totalCount = 0;
        
        foreach (var category in AffixDatabase_Modern.Instance.armourPrefixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                foreach (var affix in subCategory.affixes)
                {
                    totalCount++;
                    if (affix.compatibleTags == null || affix.compatibleTags.Count == 0)
                    {
                        noTagsCount++;
                    }
                }
            }
        }
        
        foreach (var category in AffixDatabase_Modern.Instance.armourSuffixCategories)
        {
            foreach (var subCategory in category.subCategories)
            {
                foreach (var affix in subCategory.affixes)
                {
                    totalCount++;
                    if (affix.compatibleTags == null || affix.compatibleTags.Count == 0)
                    {
                        noTagsCount++;
                    }
                }
            }
        }
        
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
        
        if (noTagsCount > 0)
        {
            Debug.LogError($"❌ <b>{noTagsCount} out of {totalCount}</b> armor affixes have NO TAGS!");
            Debug.LogError("   This is why affixes aren't rolling on items!");
            Debug.LogError("   <b>Solution: CLEAR and RE-IMPORT affixes with the updated importer!</b>");
        }
        else
        {
            Debug.Log($"✅ All {totalCount} armor affixes have tags!");
        }
        
        Debug.Log("<color=cyan>═══════════════════════════════════════════════════════</color>");
    }
}

