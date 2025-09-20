using UnityEngine;
using Dexiled.Data.Items;

public class DamageCalculationTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestOnStart = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            TestRustedSwordCalculation();
        }
    }
    
    [ContextMenu("Test Rusted Sword Calculation")]
    public void TestRustedSwordCalculation()
    {
        Debug.Log("=== Testing Rusted Sword Damage Calculation ===");
        
        // Create a Rusted Sword with base damage 8
        WeaponItem rustedSword = ScriptableObject.CreateInstance<WeaponItem>();
        rustedSword.itemName = "Rusted Sword";
        rustedSword.description = "A rusty but functional sword";
        rustedSword.weaponType = WeaponItemType.Sword;
        rustedSword.handedness = WeaponHandedness.OneHanded;
        rustedSword.minDamage = 8f;
        rustedSword.maxDamage = 8f; // Single value for simplicity
        rustedSword.attackSpeed = 1.0f;
        rustedSword.criticalStrikeChance = 5f;
        rustedSword.primaryDamageType = DamageType.Physical;
        rustedSword.itemType = ItemType.Weapon;
        rustedSword.equipmentType = EquipmentType.MainHand;
        
        Debug.Log($"Original base damage: {rustedSword.GetOriginalBaseDamage()}");
        
        // Add affix: "+6 Physical damage"
        Affix addedDamageAffix = new Affix("Added Physical", "Adds 6 Physical Damage", AffixType.Prefix, AffixTier.Tier1);
        AffixModifier addedMod = new AffixModifier("PhysicalDamage", 6, 6, ModifierType.Flat);
        addedMod.damageType = DamageType.Physical;
        addedMod.scope = ModifierScope.Local;
        addedDamageAffix.modifiers.Add(addedMod);
        rustedSword.AddPrefix(addedDamageAffix);
        
        // Add affix: "40% increased Physical damage"
        Affix increasedDamageAffix = new Affix("Increased Physical", "40% increased Physical Damage", AffixType.Suffix, AffixTier.Tier1);
        AffixModifier increasedMod = new AffixModifier("IncreasedPhysicalDamage", 40, 40, ModifierType.Increased);
        increasedMod.damageType = DamageType.Physical;
        increasedMod.scope = ModifierScope.Local;
        increasedDamageAffix.modifiers.Add(increasedMod);
        rustedSword.AddSuffix(increasedDamageAffix);
        
        // Calculate expected result: (8 + 6) * 1.4 = 19.6, rounded up to 20
        float expectedDamage = Mathf.Ceil((8f + 6f) * 1.4f);
        float actualDamage = rustedSword.GetCalculatedTotalDamage();
        
        Debug.Log($"Expected damage: {expectedDamage}");
        Debug.Log($"Actual calculated damage: {actualDamage}");
        Debug.Log($"Calculation correct: {Mathf.Approximately(expectedDamage, actualDamage)}");
        
        // Show the full description
        string description = rustedSword.GetFullDescription();
        Debug.Log($"Full item description:\n{description}");
        
        // Test individual values
        Debug.Log($"Total Min Damage: {rustedSword.GetTotalMinDamage()}");
        Debug.Log($"Total Max Damage: {rustedSword.GetTotalMaxDamage()}");
        Debug.Log($"Average Damage: {rustedSword.GetCalculatedTotalDamage()}");
        
        // Test modifier values
        Debug.Log($"Added Physical Damage: {rustedSword.GetModifierValue("PhysicalDamage")}");
        Debug.Log($"Increased Physical Damage: {rustedSword.GetModifierValue("IncreasedPhysicalDamage")}");
        
        Debug.Log("=== Test Complete ===");
    }
    
    [ContextMenu("Test Different Weapon")]
    public void TestDifferentWeapon()
    {
        Debug.Log("=== Testing Different Weapon ===");
        
        // Create a Steel Axe with base damage 10-15
        WeaponItem steelAxe = ScriptableObject.CreateInstance<WeaponItem>();
        steelAxe.itemName = "Steel Axe";
        steelAxe.description = "A well-crafted steel axe";
        steelAxe.weaponType = WeaponItemType.Axe;
        steelAxe.handedness = WeaponHandedness.OneHanded;
        steelAxe.minDamage = 10f;
        steelAxe.maxDamage = 15f;
        steelAxe.attackSpeed = 1.2f;
        steelAxe.criticalStrikeChance = 5f;
        steelAxe.primaryDamageType = DamageType.Physical;
        steelAxe.itemType = ItemType.Weapon;
        steelAxe.equipmentType = EquipmentType.MainHand;
        
        Debug.Log($"Original base damage: {steelAxe.GetOriginalBaseDamage()}");
        
        // Add affix: "+5 Physical damage"
        Affix addedDamageAffix = new Affix("Added Physical", "Adds 5 Physical Damage", AffixType.Prefix, AffixTier.Tier1);
        AffixModifier addedMod = new AffixModifier("PhysicalDamage", 5, 5, ModifierType.Flat);
        addedMod.damageType = DamageType.Physical;
        addedMod.scope = ModifierScope.Local;
        addedDamageAffix.modifiers.Add(addedMod);
        steelAxe.AddPrefix(addedDamageAffix);
        
        // Add affix: "30% increased Physical damage"
        Affix increasedDamageAffix = new Affix("Increased Physical", "30% increased Physical Damage", AffixType.Suffix, AffixTier.Tier1);
        AffixModifier increasedMod = new AffixModifier("IncreasedPhysicalDamage", 30, 30, ModifierType.Increased);
        increasedMod.damageType = DamageType.Physical;
        increasedMod.scope = ModifierScope.Local;
        increasedDamageAffix.modifiers.Add(increasedMod);
        steelAxe.AddSuffix(increasedDamageAffix);
        
        // Calculate expected result: (12.5 + 5) * 1.3 = 22.75, rounded up to 23
        float expectedDamage = Mathf.Ceil((12.5f + 5f) * 1.3f);
        float actualDamage = steelAxe.GetCalculatedTotalDamage();
        
        Debug.Log($"Expected damage: {expectedDamage}");
        Debug.Log($"Actual calculated damage: {actualDamage}");
        Debug.Log($"Calculation correct: {Mathf.Approximately(expectedDamage, actualDamage)}");
        
        // Show the full description
        string description = steelAxe.GetFullDescription();
        Debug.Log($"Full item description:\n{description}");
        
        Debug.Log("=== Test Complete ===");
    }
}


