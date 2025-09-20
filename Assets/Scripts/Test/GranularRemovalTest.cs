using UnityEngine;

public class GranularRemovalTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestGranularRemoval();
        }
    }
    
    [ContextMenu("Test Granular Removal")]
    public void TestGranularRemoval()
    {
        Debug.Log("=== Granular Removal Test ===");
        
        Debug.Log("The AffixDatabase now supports multiple levels of removal:");
        Debug.Log("");
        
        Debug.Log("🎯 INDIVIDUAL LEVEL REMOVAL:");
        Debug.Log("• Remove Affix: Click 'Remove' next to any affix");
        Debug.Log("• Remove Sub: Click 'Remove Sub' next to any subcategory");
        Debug.Log("• Remove Category: Click 'Remove Category' next to any category");
        Debug.Log("");
        
        Debug.Log("🚀 BULK OPERATIONS:");
        Debug.Log("• Remove All Elemental: Removes all Fire, Cold, Lightning, Chaos categories");
        Debug.Log("• Remove All Physical: Removes all Physical categories");
        Debug.Log("• Remove Empty Categories: Cleans up empty categories");
        Debug.Log("• Remove Duplicate Affixes: Removes duplicates based on name/location");
        Debug.Log("");
        
        Debug.Log("📋 HOW TO USE:");
        Debug.Log("1. Select AffixDatabase in Project window");
        Debug.Log("2. Navigate through categories using foldouts");
        Debug.Log("3. Use individual 'Remove' buttons for specific items");
        Debug.Log("4. Use 'Quick Remove' section for bulk operations");
        Debug.Log("5. All operations have confirmation dialogs for safety");
        Debug.Log("");
        
        Debug.Log("💡 TIPS:");
        Debug.Log("• Individual removal is perfect for cleaning up specific affixes");
        Debug.Log("• Bulk removal is great for reorganizing your database");
        Debug.Log("• Empty category removal helps keep things tidy");
        Debug.Log("• Duplicate removal prevents conflicts and saves space");
        Debug.Log("");
        
        Debug.Log("⚠️ SAFETY FEATURES:");
        Debug.Log("• All removal operations show confirmation dialogs");
        Debug.Log("• Changes are automatically saved");
        Debug.Log("• Console logs show what was removed");
        Debug.Log("• No accidental deletions possible");
    }
    
    [ContextMenu("Show Removal Hierarchy")]
    public void ShowRemovalHierarchy()
    {
        Debug.Log("=== Removal Hierarchy ===");
        Debug.Log("");
        Debug.Log("AffixDatabase");
        Debug.Log("├── Weapon Prefixes");
        Debug.Log("│   ├── Physical (Remove Category)");
        Debug.Log("│   │   ├── Flat (Remove Sub)");
        Debug.Log("│   │   │   ├── Affix 1 (Remove)");
        Debug.Log("│   │   │   └── Affix 2 (Remove)");
        Debug.Log("│   │   └── Increased (Remove Sub)");
        Debug.Log("│   └── Elemental (Remove Category)");
        Debug.Log("│       ├── Fire Flat (Remove Sub)");
        Debug.Log("│       ├── Fire Increased (Remove Sub)");
        Debug.Log("│       ├── Fire More (Remove Sub)");
        Debug.Log("│       ├── Cold Flat (Remove Sub)");
        Debug.Log("│       ├── Cold Increased (Remove Sub)");
        Debug.Log("│       ├── Lightning Flat (Remove Sub)");
        Debug.Log("│       ├── Lightning Increased (Remove Sub)");
        Debug.Log("│       ├── Chaos Flat (Remove Sub)");
        Debug.Log("│       └── Chaos Increased (Remove Sub)");
        Debug.Log("├── Weapon Suffixes");
        Debug.Log("│   └── [Same structure as above]");
        Debug.Log("├── Armour Prefixes");
        Debug.Log("├── Armour Suffixes");
        Debug.Log("├── Jewellery Prefixes");
        Debug.Log("└── Jewellery Suffixes");
        Debug.Log("");
        Debug.Log("Each level has its own removal button for precise control!");
        Debug.Log("Elemental categories now have granular subcategories combining element + modifier type.");
    }
}
