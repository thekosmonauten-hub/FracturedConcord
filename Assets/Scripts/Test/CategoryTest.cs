using UnityEngine;

public class CategoryTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestCategoryStructure();
        }
    }
    
    [ContextMenu("Test Category Structure")]
    public void TestCategoryStructure()
    {
        if (AffixDatabase.Instance == null)
        {
            Debug.LogError("AffixDatabase not found! Please create it first.");
            return;
        }
        
        Debug.Log("=== Testing Category Structure ===");
        
        // Test weapon prefixes
        Debug.Log($"Weapon Prefix Categories: {AffixDatabase.Instance.weaponPrefixCategories.Count}");
        foreach (var category in AffixDatabase.Instance.weaponPrefixCategories)
        {
            Debug.Log($"  Category: {category.categoryName}");
            Debug.Log($"    Sub-categories: {category.subCategories.Count}");
            foreach (var subCategory in category.subCategories)
            {
                Debug.Log($"      Sub-category: {subCategory.subCategoryName} ({subCategory.affixes.Count} affixes)");
                foreach (var affix in subCategory.affixes)
                {
                    Debug.Log($"        Affix: {affix.name} - {affix.description}");
                }
            }
        }
        
        // Test getting all affixes
        var allPrefixes = AffixDatabase.Instance.GetRandomPrefix(ItemType.Weapon, 50, AffixTier.Tier1);
        if (allPrefixes != null)
        {
            Debug.Log($"Successfully retrieved random prefix: {allPrefixes.name}");
        }
        else
        {
            Debug.Log("No prefixes found!");
        }
    }
}
