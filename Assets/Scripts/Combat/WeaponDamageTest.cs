using UnityEngine;

public class WeaponDamageTest : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnStart = true;
    public bool showDetailedBreakdown = true;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    [ContextMenu("Run Weapon Damage Tests")]
    public void RunAllTests()
    {
        Debug.Log("=== WEAPON DAMAGE SYSTEM TEST ===\n");
        
        TestSteelAxeWeapon();
        TestCardDamageCalculation();
        TestMultipleWeapons();
        TestDamageBreakdown();
        
        Debug.Log("=== TEST COMPLETE ===");
    }
    
    void TestSteelAxeWeapon()
    {
        Debug.Log("--- TEST 1: Steel Axe Weapon Creation ---");
        
        // Create Steel Axe example
        Weapon steelAxe = CharacterWeapons.CreateSteelAxe();
        
        Debug.Log($"Weapon: {steelAxe.weaponName}");
        Debug.Log($"Type: {steelAxe.weaponType}");
        Debug.Log($"Base Damage: {steelAxe.GetBaseDamageRangeString()}");
        Debug.Log($"Total Damage: {steelAxe.GetDamageRangeString()}");
        Debug.Log($"Average Base Damage: {steelAxe.GetBaseDamage():F1}");
        Debug.Log($"Average Total Damage: {steelAxe.GetWeaponDamage():F1}");
        
        Debug.Log("\nAffixes:");
        foreach (WeaponAffix affix in steelAxe.affixes)
        {
            Debug.Log($"  - {affix.affixName}");
        }
        
        Debug.Log($"\nDamage Calculation:");
        Debug.Log($"  Base: 5-9 Physical");
        Debug.Log($"  Added: +6.5 Physical");
        Debug.Log($"  Subtotal: 11.5-15.5");
        Debug.Log($"  Increased: +21%");
        Debug.Log($"  Final: 12-18 Physical (rounded)");
        Debug.Log("");
    }
    
    void TestCardDamageCalculation()
    {
        Debug.Log("--- TEST 2: Card Damage Calculation ---");
        
        // Create a test character (Marauder)
        Character testCharacter = new Character("TestMarauder", "Marauder");
        Debug.Log($"Character: {testCharacter.characterName} ({testCharacter.characterClass})");
        Debug.Log($"Attributes: {testCharacter.strength} STR, {testCharacter.dexterity} DEX, {testCharacter.intelligence} INT");
        
        // Create a test card (Heavy Strike)
        Card heavyStrike = new Card
        {
            cardName = "Heavy Strike",
            baseDamage = 8f,
            scalesWithMeleeWeapon = true,
            damageScaling = new AttributeScaling { strengthScaling = 0.5f }
        };
        
        Debug.Log($"Card: {heavyStrike.cardName}");
        Debug.Log($"Base Damage: {heavyStrike.baseDamage}");
        Debug.Log($"Scales with: Melee Weapon");
        Debug.Log($"Attribute Scaling: +{heavyStrike.damageScaling.strengthScaling:F1} per Strength");
        
        // Calculate damage without weapon
        float damageWithoutWeapon = DamageCalculator.CalculateCardDamage(heavyStrike, testCharacter);
        Debug.Log($"\nDamage without weapon: {damageWithoutWeapon:F1}");
        
        // Create Steel Axe and calculate damage with weapon
        Weapon steelAxe = CharacterWeapons.CreateSteelAxe();
        float damageWithWeapon = DamageCalculator.CalculateCardDamage(heavyStrike, testCharacter, steelAxe);
        Debug.Log($"Damage with Steel Axe: {damageWithWeapon:F1}");
        Debug.Log($"Weapon contribution: +{damageWithWeapon - damageWithoutWeapon:F1}");
        Debug.Log("");
    }
    
    void TestMultipleWeapons()
    {
        Debug.Log("--- TEST 3: Multiple Weapon Types ---");
        
        Character testCharacter = new Character("TestCharacter", "Marauder");
        
        // Create different weapon types
        Weapon meleeWeapon = CharacterWeapons.CreateBasicWeapon("Iron Sword", WeaponType.Melee, 3f, 6f, DamageType.Physical);
        Weapon projectileWeapon = CharacterWeapons.CreateBasicWeapon("Short Bow", WeaponType.Projectile, 2f, 4f, DamageType.Physical);
        Weapon spellWeapon = CharacterWeapons.CreateBasicWeapon("Fire Staff", WeaponType.Spell, 4f, 8f, DamageType.Fire);
        
        Debug.Log($"Melee Weapon: {meleeWeapon.GetDamageRangeString()}");
        Debug.Log($"Projectile Weapon: {projectileWeapon.GetDamageRangeString()}");
        Debug.Log($"Spell Weapon: {spellWeapon.GetDamageRangeString()}");
        
        // Test different card types
        Card meleeCard = new Card { cardName = "Slash", baseDamage = 5f, scalesWithMeleeWeapon = true };
        Card projectileCard = new Card { cardName = "Arrow Shot", baseDamage = 4f, scalesWithProjectileWeapon = true };
        Card spellCard = new Card { cardName = "Fireball", baseDamage = 6f, scalesWithSpellWeapon = true };
        
        Debug.Log($"\nMelee Card with Melee Weapon: {DamageCalculator.CalculateCardDamage(meleeCard, testCharacter, meleeWeapon):F1}");
        Debug.Log($"Projectile Card with Bow: {DamageCalculator.CalculateCardDamage(projectileCard, testCharacter, projectileWeapon):F1}");
        Debug.Log($"Spell Card with Staff: {DamageCalculator.CalculateCardDamage(spellCard, testCharacter, spellWeapon):F1}");
        Debug.Log("");
    }
    
    void TestDamageBreakdown()
    {
        if (!showDetailedBreakdown) return;
        
        Debug.Log("--- TEST 4: Detailed Damage Breakdown ---");
        
        Character testCharacter = new Character("TestMarauder", "Marauder");
        Weapon steelAxe = CharacterWeapons.CreateSteelAxe();
        Card heavyStrike = new Card
        {
            cardName = "Heavy Strike",
            baseDamage = 8f,
            scalesWithMeleeWeapon = true,
            damageScaling = new AttributeScaling { strengthScaling = 0.5f }
        };
        
        float finalDamage = DamageCalculator.CalculateCardDamage(heavyStrike, testCharacter, steelAxe);
        
        Debug.Log($"Final Damage: {finalDamage:F1}");
        Debug.Log($"Breakdown:");
        Debug.Log($"  Card Base: {heavyStrike.baseDamage}");
        Debug.Log($"  Strength Scaling: +{testCharacter.strength * heavyStrike.damageScaling.strengthScaling:F1} ({testCharacter.strength} STR × {heavyStrike.damageScaling.strengthScaling:F1})");
        Debug.Log($"  Weapon Damage: +{steelAxe.GetWeaponDamage():F1} (total weapon damage)");
        Debug.Log($"  Subtotal: {heavyStrike.baseDamage + (testCharacter.strength * heavyStrike.damageScaling.strengthScaling) + steelAxe.GetWeaponDamage():F1}");
        Debug.Log($"  Character Increased Damage: +{testCharacter.increasedDamage:P0}");
        Debug.Log($"  Character More Damage: ×{testCharacter.moreDamage:F2}");
        Debug.Log($"  Final: {finalDamage:F1}");
        Debug.Log("");
    }
    
    // Button method for testing in editor
    [ContextMenu("Test Single Weapon")]
    public void TestSingleWeapon()
    {
        TestSteelAxeWeapon();
    }
}
