using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tests the Smart Affix Compatibility System to ensure no dead affixes are generated
/// </summary>
public class SmartCompatibilityTester : MonoBehaviour
{
    [Header("Test Settings")]
    public int testItemsPerType = 50;
    public bool showDetailedResults = true;
    public bool showOnlyFailures = false;
    
    [ContextMenu("Test Smart Compatibility")]
    public void TestSmartCompatibility()
    {
        Debug.Log("=== SMART AFFIX COMPATIBILITY TEST ===");
        
        if (AreaLootManager.Instance == null)
        {
            Debug.LogError("AreaLootManager.Instance is null! Ensure it exists in the scene.");
            return;
        }
        
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("ItemDatabase.Instance is null! Ensure ItemDatabase exists in Resources folder.");
            return;
        }
        
        TestArmorCompatibility();
        TestWeaponCompatibility();
        TestJewelryCompatibility();
        
        Debug.Log("=== COMPATIBILITY TEST COMPLETE ===");
    }
    
    private void TestArmorCompatibility()
    {
        Debug.Log("\n--- TESTING ARMOUR COMPATIBILITY ---");
        
        // Get all armour items from the database
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        List<Armour> armourItems = allItems.OfType<Armour>().ToList();
        
        if (armourItems.Count == 0)
        {
            Debug.LogWarning("No armour items found in ItemDatabase!");
            return;
        }
        
        int totalTests = 0;
        int passedTests = 0;
        int failedTests = 0;
        
        foreach (Armour armour in armourItems.Take(testItemsPerType))
        {
            // Generate affixes for this armour
            if (AffixDatabase_Modern.Instance != null)
            {
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(armour, 50, 0.8f, 0.4f);
                
                // Test each affix for compatibility
                foreach (var affix in armour.prefixes.Concat(armour.suffixes))
                {
                    totalTests++;
                    bool isCompatible = TestAffixCompatibility(affix, armour);
                    
                    if (isCompatible)
                    {
                        passedTests++;
                        if (showDetailedResults && !showOnlyFailures)
                        {
                            Debug.Log($"✅ PASS: '{affix.description}' on {armour.itemName} (A:{armour.armour} E:{armour.evasion} ES:{armour.energyShield})");
                        }
                    }
                    else
                    {
                        failedTests++;
                        Debug.LogError($"❌ FAIL: '{affix.description}' on {armour.itemName} (A:{armour.armour} E:{armour.evasion} ES:{armour.energyShield}) - DEAD AFFIX!");
                    }
                }
                
                // Clear affixes for next test
                armour.ClearAffixes();
            }
        }
        
        Debug.Log($"Armour Tests: {passedTests}/{totalTests} passed ({failedTests} dead affixes blocked)");
    }
    
    private void TestWeaponCompatibility()
    {
        Debug.Log("\n--- TESTING WEAPON COMPATIBILITY ---");
        
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        List<WeaponItem> weaponItems = allItems.OfType<WeaponItem>().ToList();
        
        if (weaponItems.Count == 0)
        {
            Debug.LogWarning("No weapon items found in ItemDatabase!");
            return;
        }
        
        int totalTests = 0;
        int passedTests = 0;
        int failedTests = 0;
        
        foreach (WeaponItem weapon in weaponItems.Take(testItemsPerType))
        {
            if (AffixDatabase_Modern.Instance != null)
            {
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(weapon, 50, 0.8f, 0.4f);
                
                foreach (var affix in weapon.prefixes.Concat(weapon.suffixes))
                {
                    totalTests++;
                    bool isCompatible = TestAffixCompatibility(affix, weapon);
                    
                    if (isCompatible)
                    {
                        passedTests++;
                        if (showDetailedResults && !showOnlyFailures)
                        {
                            Debug.Log($"✅ PASS: '{affix.description}' on {weapon.itemName} ({weapon.weaponType}, {weapon.handedness})");
                        }
                    }
                    else
                    {
                        failedTests++;
                        Debug.LogError($"❌ FAIL: '{affix.description}' on {weapon.itemName} ({weapon.weaponType}, {weapon.handedness}) - INCOMPATIBLE!");
                    }
                }
                
                weapon.ClearAffixes();
            }
        }
        
        Debug.Log($"Weapon Tests: {passedTests}/{totalTests} passed ({failedTests} incompatible blocked)");
    }
    
    private void TestJewelryCompatibility()
    {
        Debug.Log("\n--- TESTING JEWELRY COMPATIBILITY ---");
        
        // Note: Add jewelry testing when jewelry items are available in the database
        Debug.Log("Jewelry testing not implemented - no jewelry items in current database");
    }
    
    /// <summary>
    /// Tests if an affix makes sense on the given item
    /// </summary>
    private bool TestAffixCompatibility(Affix affix, BaseItem item)
    {
        // Check for obviously problematic combinations
        foreach (var modifier in affix.modifiers)
        {
            if (item is Armour armour)
            {
                // Energy Shield modifiers on non-ES armour = dead affix
                if (modifier.statName.ToLower().Contains("energyshield") && armour.energyShield <= 0)
                {
                    return false;
                }
                
                // Evasion modifiers on non-Evasion armour = dead affix  
                if (modifier.statName.ToLower().Contains("evasion") && armour.evasion <= 0)
                {
                    return false;
                }
                
                // Armour modifiers on non-Armour pieces = dead affix
                if (modifier.statName.ToLower().Contains("armour") && !modifier.statName.ToLower().Contains("energyshield") && armour.armour <= 0)
                {
                    return false;
                }
                
                // Block chance on non-shields = dead affix
                if (modifier.statName.ToLower().Contains("blockchance") && armour.armourSlot != ArmourSlot.Shield)
                {
                    return false;
                }
            }
            
            if (item is WeaponItem weapon)
            {
                // Cast speed on non-caster weapons = less useful (but not dead)
                if (modifier.statName.ToLower().Contains("castspeed"))
                {
                    if (weapon.weaponType != WeaponItemType.Staff && 
                        weapon.weaponType != WeaponItemType.Wand && 
                        weapon.weaponType != WeaponItemType.Sceptre)
                    {
                        // This is questionable but not necessarily dead
                        if (showDetailedResults)
                        {
                            Debug.LogWarning($"⚠️ QUESTIONABLE: Cast Speed on {weapon.weaponType} - may not be optimal");
                        }
                    }
                }
            }
        }
        
        return true; // Passed all compatibility checks
    }
    
    [ContextMenu("Test Specific Item Types")]
    public void TestSpecificItemTypes()
    {
        Debug.Log("\n--- TESTING SPECIFIC ITEM TYPES ---");
        
        // Test pure armour items
        TestPureArmorItems();
        
        // Test pure evasion items  
        TestPureEvasionItems();
        
        // Test energy shield items
        TestEnergyShieldItems();
        
        // Test hybrid items
        TestHybridItems();
    }
    
    private void TestPureArmorItems()
    {
        Debug.Log("\n🛡️ Testing Pure Armour Items:");
        
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        var pureArmourItems = allItems.OfType<Armour>()
            .Where(a => a.armour > 0 && a.evasion <= 0 && a.energyShield <= 0)
            .Take(5);
        
        foreach (Armour armour in pureArmourItems)
        {
            Debug.Log($"  • {armour.itemName}: Armour {armour.armour}, Evasion {armour.evasion}, ES {armour.energyShield}");
            
            // Generate and test affixes
            if (AffixDatabase_Modern.Instance != null)
            {
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(armour, 50, 1.0f, 0.5f);
                
                bool hasInvalidAffixes = false;
                foreach (var affix in armour.prefixes.Concat(armour.suffixes))
                {
                    foreach (var modifier in affix.modifiers)
                    {
                        if (modifier.statName.ToLower().Contains("evasion") || modifier.statName.ToLower().Contains("energyshield"))
                        {
                            Debug.LogError($"    ❌ INVALID: {affix.description} - would be dead on pure armour!");
                            hasInvalidAffixes = true;
                        }
                    }
                }
                
                if (!hasInvalidAffixes)
                {
                    Debug.Log($"    ✅ All affixes compatible with pure armour base");
                }
                
                armour.ClearAffixes();
            }
        }
    }
    
    private void TestPureEvasionItems()
    {
        Debug.Log("\n🏃 Testing Pure Evasion Items:");
        
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        var pureEvasionItems = allItems.OfType<Armour>()
            .Where(a => a.evasion > 0 && a.armour <= 0 && a.energyShield <= 0)
            .Take(5);
        
        foreach (Armour armour in pureEvasionItems)
        {
            Debug.Log($"  • {armour.itemName}: Armour {armour.armour}, Evasion {armour.evasion}, ES {armour.energyShield}");
            
            if (AffixDatabase_Modern.Instance != null)
            {
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(armour, 50, 1.0f, 0.5f);
                
                bool hasInvalidAffixes = false;
                foreach (var affix in armour.prefixes.Concat(armour.suffixes))
                {
                    foreach (var modifier in affix.modifiers)
                    {
                        if (modifier.statName.ToLower().Contains("armour") && !modifier.statName.ToLower().Contains("energyshield"))
                        {
                            Debug.LogError($"    ❌ INVALID: {affix.description} - would be dead on pure evasion!");
                            hasInvalidAffixes = true;
                        }
                    }
                }
                
                if (!hasInvalidAffixes)
                {
                    Debug.Log($"    ✅ All affixes compatible with pure evasion base");
                }
                
                armour.ClearAffixes();
            }
        }
    }
    
    private void TestEnergyShieldItems()
    {
        Debug.Log("\n⚡ Testing Energy Shield Items:");
        
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        var energyShieldItems = allItems.OfType<Armour>()
            .Where(a => a.energyShield > 0)
            .Take(5);
        
        foreach (Armour armour in energyShieldItems)
        {
            Debug.Log($"  • {armour.itemName}: Armour {armour.armour}, Evasion {armour.evasion}, ES {armour.energyShield}");
            
            if (AffixDatabase_Modern.Instance != null)
            {
                AffixDatabase_Modern.Instance.GenerateRandomAffixes(armour, 50, 1.0f, 0.5f);
                
                bool foundEsAffix = false;
                foreach (var affix in armour.prefixes.Concat(armour.suffixes))
                {
                    foreach (var modifier in affix.modifiers)
                    {
                        if (modifier.statName.ToLower().Contains("energyshield"))
                        {
                            foundEsAffix = true;
                            Debug.Log($"    ✅ GOOD: {affix.description} - meaningful on ES base!");
                            break;
                        }
                    }
                    if (foundEsAffix) break;
                }
                
                armour.ClearAffixes();
            }
        }
    }
    
    private void TestHybridItems()
    {
        Debug.Log("\n🔀 Testing Hybrid Items:");
        
        List<BaseItem> allItems = ItemDatabase.Instance.GetAllItems();
        var hybridItems = allItems.OfType<Armour>()
            .Where(a => (a.armour > 0 ? 1 : 0) + (a.evasion > 0 ? 1 : 0) + (a.energyShield > 0 ? 1 : 0) >= 2)
            .Take(5);
        
        foreach (Armour armour in hybridItems)
        {
            Debug.Log($"  • {armour.itemName}: Armour {armour.armour}, Evasion {armour.evasion}, ES {armour.energyShield} (HYBRID)");
        }
    }
}
