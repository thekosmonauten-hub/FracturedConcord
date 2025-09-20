using UnityEngine;

public class AffixTest : MonoBehaviour
{
    [Header("Test Settings")]
    public WeaponItem testWeapon;
    public int itemLevel = 50;
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestAffixGeneration();
        }
    }
    
    [ContextMenu("Test Affix Generation")]
    public void TestAffixGeneration()
    {
        if (testWeapon == null)
        {
            Debug.LogError("Please assign a test weapon!");
            return;
        }
        
        if (AffixDatabase.Instance == null)
        {
            Debug.LogError("AffixDatabase not found! Please create it first.");
            return;
        }
        
        Debug.Log($"Testing affix generation for {testWeapon.itemName} (Level {itemLevel})");
        
        // Generate random affixes
        AffixDatabase.Instance.GenerateRandomAffixes(testWeapon, itemLevel, 0.3f, 0.1f);
        
        // Display results
        Debug.Log($"Generated {testWeapon.GetDisplayName()} ({testWeapon.rarity})");
        Debug.Log($"Prefixes: {testWeapon.prefixes.Count}, Suffixes: {testWeapon.suffixes.Count}");
        
        foreach (var prefix in testWeapon.prefixes)
        {
            Debug.Log($"Prefix: {prefix.name} - {prefix.description}");
        }
        
        foreach (var suffix in testWeapon.suffixes)
        {
            Debug.Log($"Suffix: {suffix.name} - {suffix.description}");
        }
    }
    
    [ContextMenu("Test Specific Affix")]
    public void TestSpecificAffix()
    {
        if (AffixDatabase.Instance == null)
        {
            Debug.LogError("AffixDatabase not found!");
            return;
        }
        
        // Test getting a specific affix
        Affix prefix = AffixDatabase.Instance.GetRandomPrefix(ItemType.Weapon, itemLevel, AffixTier.Tier1);
        if (prefix != null)
        {
            Debug.Log($"Random prefix: {prefix.name} - {prefix.description}");
        }
        else
        {
            Debug.Log("No suitable prefix found!");
        }
    }
}
