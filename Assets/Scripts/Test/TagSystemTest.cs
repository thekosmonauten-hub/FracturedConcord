using UnityEngine;
using System.Collections.Generic;

public class TagSystemTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestTagSystem();
        }
    }
    
    [ContextMenu("Test Tag System")]
    public void TestTagSystem()
    {
        Debug.Log("=== Testing Redesigned Tag System ===");
        
        // Test 1: Create different weapon types and show their tags
        TestWeaponTags();
        
        // Test 2: Show affix compatibility
        TestAffixCompatibility();
        
        // Test 3: Demonstrate flexibility
        TestDamageTypeFlexibility();
    }
    
    private void TestWeaponTags()
    {
        Debug.Log("\n--- Weapon Tag Examples ---");
        
        // Create test weapons
        var weapons = new Dictionary<string, WeaponItemType>
        {
            {"Sword", WeaponItemType.Sword},
            {"Bow", WeaponItemType.Bow},
            {"Staff", WeaponItemType.Staff},
            {"Wand", WeaponItemType.Wand}
        };
        
        foreach (var weapon in weapons)
        {
            var testWeapon = ScriptableObject.CreateInstance<WeaponItem>();
            testWeapon.itemName = $"Test {weapon.Key}";
            testWeapon.weaponType = weapon.Value;
            testWeapon.handedness = weapon.Value == WeaponItemType.Staff ? WeaponHandedness.TwoHanded : WeaponHandedness.OneHanded;
            
            // Simulate Auto-Set Tags
            var tags = GetExpectedTags(testWeapon);
            
            Debug.Log($"{weapon.Key}: {string.Join(", ", tags)}");
        }
    }
    
    private void TestAffixCompatibility()
    {
        Debug.Log("\n--- Affix Compatibility Examples ---");
        
        // Example weapon tags
        var swordTags = new List<string> {"weapon", "sword", "onehanded", "melee", "attack"};
        var bowTags = new List<string> {"weapon", "bow", "twohanded", "ranged", "attack"};
        var staffTags = new List<string> {"weapon", "staff", "twohanded", "spell", "spell"};
        
        // Example affix requirements
        var physicalAffix = new List<string> {"weapon", "attack"};
        var elementalAffix = new List<string> {"weapon"};
        var swordSpecificAffix = new List<string> {"weapon", "sword"};
        
        Debug.Log("Physical Damage Affix (requires: weapon, attack):");
        Debug.Log($"  Sword: {IsCompatible(swordTags, physicalAffix)}");
        Debug.Log($"  Bow: {IsCompatible(bowTags, physicalAffix)}");
        Debug.Log($"  Staff: {IsCompatible(staffTags, physicalAffix)}");
        
        Debug.Log("Elemental Damage Affix (requires: weapon):");
        Debug.Log($"  Sword: {IsCompatible(swordTags, elementalAffix)}");
        Debug.Log($"  Bow: {IsCompatible(bowTags, elementalAffix)}");
        Debug.Log($"  Staff: {IsCompatible(staffTags, elementalAffix)}");
        
        Debug.Log("Sword-Specific Affix (requires: weapon, sword):");
        Debug.Log($"  Sword: {IsCompatible(swordTags, swordSpecificAffix)}");
        Debug.Log($"  Bow: {IsCompatible(bowTags, swordSpecificAffix)}");
        Debug.Log($"  Staff: {IsCompatible(staffTags, swordSpecificAffix)}");
    }
    
    private void TestDamageTypeFlexibility()
    {
        Debug.Log("\n--- Damage Type Flexibility ---");
        
        var damageTypes = new string[] {"Physical", "Fire", "Cold", "Lightning", "Chaos"};
        var weaponTypes = new string[] {"Sword", "Bow", "Staff"};
        
        Debug.Log("All damage types can be applied to any weapon:");
        foreach (var weapon in weaponTypes)
        {
            Debug.Log($"\n{weapon} can have:");
            foreach (var damage in damageTypes)
            {
                Debug.Log($"  - {damage} Damage");
            }
        }
        
        Debug.Log("\nThis matches Path of Exile's system where any weapon can have any damage type!");
    }
    
    private List<string> GetExpectedTags(WeaponItem weapon)
    {
        var tags = new List<string> {"weapon"};
        
        // Add weapon type tag
        tags.Add(weapon.weaponType.ToString().ToLower());
        
        // Add handedness tag
        tags.Add(weapon.handedness.ToString().ToLower());
        
        // Add attack type tags
        switch (weapon.weaponType)
        {
            case WeaponItemType.Sword:
            case WeaponItemType.Axe:
            case WeaponItemType.Mace:
            case WeaponItemType.Dagger:
            case WeaponItemType.Claw:
            case WeaponItemType.RitualDagger:
                tags.Add("melee");
                break;
            case WeaponItemType.Bow:
            case WeaponItemType.Wand:
                tags.Add("ranged");
                break;
            case WeaponItemType.Staff:
            case WeaponItemType.Sceptre:
                tags.Add("spell");
                break;
        }
        
        // Add combat type tags
        switch (weapon.weaponType)
        {
            case WeaponItemType.Sword:
            case WeaponItemType.Axe:
            case WeaponItemType.Mace:
            case WeaponItemType.Dagger:
            case WeaponItemType.Claw:
            case WeaponItemType.RitualDagger:
            case WeaponItemType.Bow:
                tags.Add("attack");
                break;
            case WeaponItemType.Wand:
            case WeaponItemType.Staff:
            case WeaponItemType.Sceptre:
                tags.Add("spell");
                break;
        }
        
        return tags;
    }
    
    private bool IsCompatible(List<string> itemTags, List<string> requiredTags)
    {
        foreach (var requiredTag in requiredTags)
        {
            if (!itemTags.Contains(requiredTag))
                return false;
        }
        return true;
    }
}
