using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Item Database", menuName = "Dexiled/Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("Weapons")]
    public List<WeaponItem> weapons = new List<WeaponItem>();
    
    [Header("Armour")]
    public List<Armour> armour = new List<Armour>();
    
    [Header("Jewellery")]
    public List<Jewellery> jewellery = new List<Jewellery>();
    
    [Header("Off-Hand Equipment")]
    public List<OffHandEquipment> offHandEquipment = new List<OffHandEquipment>();
    
    [Header("Consumables")]
    public List<Consumable> consumables = new List<Consumable>();
    
    // Singleton instance
    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                if (_instance == null)
                {
                    Debug.LogError("ItemDatabase not found in Resources folder!");
                }
            }
            return _instance;
        }
    }
    
    // Get all items of a specific type
    public List<BaseItem> GetAllItems()
    {
        List<BaseItem> allItems = new List<BaseItem>();
        allItems.AddRange(weapons.Cast<BaseItem>());
        allItems.AddRange(armour.Cast<BaseItem>());
        allItems.AddRange(jewellery.Cast<BaseItem>());
        allItems.AddRange(offHandEquipment.Cast<BaseItem>());
        allItems.AddRange(consumables.Cast<BaseItem>());
        return allItems;
    }
    
    // Get weapons by type
    public List<WeaponItem> GetWeaponsByType(WeaponItemType weaponType)
    {
        return weapons.Where(w => w.weaponType == weaponType).ToList();
    }
    
    // Get weapons by rarity
    public List<WeaponItem> GetWeaponsByRarity(ItemRarity rarity)
    {
        return weapons.Where(w => w.rarity == rarity).ToList();
    }
    
    // Get weapons by level requirement
    public List<WeaponItem> GetWeaponsByLevel(int minLevel, int maxLevel)
    {
        return weapons.Where(w => w.requiredLevel >= minLevel && w.requiredLevel <= maxLevel).ToList();
    }
    
    // Get armour by slot
    public List<Armour> GetArmourBySlot(ArmourSlot slot)
    {
        return armour.Where(a => a.armourSlot == slot).ToList();
    }
    
    // Get jewellery by type
    public List<Jewellery> GetJewelleryByType(JewelleryType jewelleryType)
    {
        return jewellery.Where(j => j.jewelleryType == jewelleryType).ToList();
    }
    
    // Get consumables by type
    public List<Consumable> GetConsumablesByType(ConsumableType consumableType)
    {
        return consumables.Where(c => c.consumableType == consumableType).ToList();
    }
    
    // Get items suitable for a character
    public List<BaseItem> GetItemsForCharacter(Character character)
    {
        List<BaseItem> suitableItems = new List<BaseItem>();
        
        // Check weapons
        foreach (var weapon in weapons)
        {
            if (weapon.CanBeEquippedBy(character))
            {
                suitableItems.Add(weapon);
            }
        }
        
        // Check armour
        foreach (var armourPiece in armour)
        {
            if (armourPiece.requiredLevel <= character.level)
            {
                suitableItems.Add(armourPiece);
            }
        }
        
        // Check jewellery
        foreach (var jewelleryPiece in jewellery)
        {
            if (jewelleryPiece.requiredLevel <= character.level)
            {
                suitableItems.Add(jewelleryPiece);
            }
        }
        
        return suitableItems;
    }
    
    // Get random weapon
    public WeaponItem GetRandomWeapon()
    {
        if (weapons.Count == 0) return null;
        return weapons[Random.Range(0, weapons.Count)];
    }
    
    // Get random armour
    public Armour GetRandomArmour()
    {
        if (armour.Count == 0) return null;
        return armour[Random.Range(0, armour.Count)];
    }
    
    // Get random jewellery
    public Jewellery GetRandomJewellery()
    {
        if (jewellery.Count == 0) return null;
        return jewellery[Random.Range(0, jewellery.Count)];
    }
    
    // Get random consumable
    public Consumable GetRandomConsumable()
    {
        if (consumables.Count == 0) return null;
        return consumables[Random.Range(0, consumables.Count)];
    }
    
    // Get random off-hand equipment
    public OffHandEquipment GetRandomOffHandEquipment()
    {
        if (offHandEquipment.Count == 0) return null;
        return offHandEquipment[Random.Range(0, offHandEquipment.Count)];
    }
    
    // Auto-populate from Resources/Items folder structure
    [ContextMenu("Scan and Populate from Resources/Items")]
    public void ScanAndPopulateFromResources()
    {
        Debug.Log("Scanning Resources/Items folder for new items...");
        
        int totalFound = 0;
        int totalAdded = 0;
        
        // Scan for weapons
        var weaponAssets = Resources.LoadAll<WeaponItem>("Items/Weapons");
        totalFound += weaponAssets.Length;
        foreach (var weapon in weaponAssets)
        {
            if (!weapons.Contains(weapon))
            {
                weapons.Add(weapon);
                totalAdded++;
                Debug.Log($"Added weapon: {weapon.itemName}");
            }
        }
        
        // Scan for armour
        var armourAssets = Resources.LoadAll<Armour>("Items/Armour");
        totalFound += armourAssets.Length;
        foreach (var armourPiece in armourAssets)
        {
            if (!armour.Contains(armourPiece))
            {
                armour.Add(armourPiece);
                totalAdded++;
                Debug.Log($"Added armour: {armourPiece.itemName}");
            }
        }
        
        // Scan for jewellery
        var jewelleryAssets = Resources.LoadAll<Jewellery>("Items/Jewellery");
        totalFound += jewelleryAssets.Length;
        foreach (var jewelleryPiece in jewelleryAssets)
        {
            if (!jewellery.Contains(jewelleryPiece))
            {
                jewellery.Add(jewelleryPiece);
                totalAdded++;
                Debug.Log($"Added jewellery: {jewelleryPiece.itemName}");
            }
        }
        
        // Scan for off-hand equipment
        var offHandAssets = Resources.LoadAll<OffHandEquipment>("Items/OffHand");
        totalFound += offHandAssets.Length;
        foreach (var offHand in offHandAssets)
        {
            if (!offHandEquipment.Contains(offHand))
            {
                offHandEquipment.Add(offHand);
                totalAdded++;
                Debug.Log($"Added off-hand equipment: {offHand.itemName}");
            }
        }
        
        // Scan for consumables
        var consumableAssets = Resources.LoadAll<Consumable>("Items/Consumables");
        totalFound += consumableAssets.Length;
        foreach (var consumable in consumableAssets)
        {
            if (!consumables.Contains(consumable))
            {
                consumables.Add(consumable);
                totalAdded++;
                Debug.Log($"Added consumable: {consumable.itemName}");
            }
        }
        
        Debug.Log($"Scan complete! Found {totalFound} items, added {totalAdded} new items to database.");
    }
    
    // Scan specific category
    [ContextMenu("Scan Weapons Only")]
    public void ScanWeaponsOnly()
    {
        Debug.Log("Scanning for new weapons...");
        var weaponAssets = Resources.LoadAll<WeaponItem>("Items/Weapons");
        int added = 0;
        
        foreach (var weapon in weaponAssets)
        {
            if (!weapons.Contains(weapon))
            {
                weapons.Add(weapon);
                added++;
                Debug.Log($"Added weapon: {weapon.itemName}");
            }
        }
        
        Debug.Log($"Added {added} new weapons to database.");
    }
    
    [ContextMenu("Scan Armour Only")]
    public void ScanArmourOnly()
    {
        Debug.Log("Scanning for new armour...");
        var armourAssets = Resources.LoadAll<Armour>("Items/Armour");
        int added = 0;
        
        foreach (var armourPiece in armourAssets)
        {
            if (!armour.Contains(armourPiece))
            {
                armour.Add(armourPiece);
                added++;
                Debug.Log($"Added armour: {armourPiece.itemName}");
            }
        }
        
        Debug.Log($"Added {added} new armour pieces to database.");
    }
    
    [ContextMenu("Scan Jewellery Only")]
    public void ScanJewelleryOnly()
    {
        Debug.Log("Scanning for new jewellery...");
        var jewelleryAssets = Resources.LoadAll<Jewellery>("Items/Jewellery");
        int added = 0;
        
        foreach (var jewelleryPiece in jewelleryAssets)
        {
            if (!jewellery.Contains(jewelleryPiece))
            {
                jewellery.Add(jewelleryPiece);
                added++;
                Debug.Log($"Added jewellery: {jewelleryPiece.itemName}");
            }
        }
        
        Debug.Log($"Added {added} new jewellery pieces to database.");
    }
    
    [ContextMenu("Scan Off-Hand Only")]
    public void ScanOffHandOnly()
    {
        Debug.Log("Scanning for new off-hand equipment...");
        var offHandAssets = Resources.LoadAll<OffHandEquipment>("Items/OffHand");
        int added = 0;
        
        foreach (var offHand in offHandAssets)
        {
            if (!offHandEquipment.Contains(offHand))
            {
                offHandEquipment.Add(offHand);
                added++;
                Debug.Log($"Added off-hand equipment: {offHand.itemName}");
            }
        }
        
        Debug.Log($"Added {added} new off-hand equipment pieces to database.");
    }
    
    [ContextMenu("Scan Consumables Only")]
    public void ScanConsumablesOnly()
    {
        Debug.Log("Scanning for new consumables...");
        var consumableAssets = Resources.LoadAll<Consumable>("Items/Consumables");
        int added = 0;
        
        foreach (var consumable in consumableAssets)
        {
            if (!consumables.Contains(consumable))
            {
                consumables.Add(consumable);
                added++;
                Debug.Log($"Added consumable: {consumable.itemName}");
            }
        }
        
        Debug.Log($"Added {added} new consumables to database.");
    }
    
    // Clear all items
    [ContextMenu("Clear All Items")]
    public void ClearAllItems()
    {
        #if UNITY_EDITOR
        if (EditorUtility.DisplayDialog("Clear All Items", 
            "Are you sure you want to clear all items from the database?", "Yes", "No"))
        {
            weapons.Clear();
            armour.Clear();
            jewellery.Clear();
            offHandEquipment.Clear();
            consumables.Clear();
            Debug.Log("Cleared all items from database.");
        }
        #else
        Debug.LogWarning("ClearAllItems dialog is editor-only.");
        #endif
    }
    
    // Get database statistics
    public string GetDatabaseStatistics()
    {
        return $"Weapons: {weapons.Count}, Armour: {armour.Count}, Jewellery: {jewellery.Count}, Off-Hand: {offHandEquipment.Count}, Consumables: {consumables.Count}";
    }
}
