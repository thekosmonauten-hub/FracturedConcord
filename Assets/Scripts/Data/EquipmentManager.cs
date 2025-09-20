using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EquipmentManager : MonoBehaviour
{
    [Header("Equipment Data")]
    public EquipmentData currentEquipment = new EquipmentData();
    
    [Header("Character Reference")]
    public Character currentCharacter;
    
    [Header("Equipment Stats")]
    public Dictionary<string, float> totalEquipmentStats = new Dictionary<string, float>();
    
    // Singleton pattern
    private static EquipmentManager _instance;
    public static EquipmentManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EquipmentManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("EquipmentManager");
                    _instance = go.AddComponent<EquipmentManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Find CharacterManager if not assigned
            if (currentCharacter == null)
            {
                CharacterManager charManager = FindFirstObjectByType<CharacterManager>();
                if (charManager != null)
                {
                    currentCharacter = charManager.GetCurrentCharacter();
                }
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Get character reference
        if (currentCharacter == null)
        {
            CharacterManager charManager = FindFirstObjectByType<CharacterManager>();
            if (charManager != null)
            {
                currentCharacter = charManager.GetCurrentCharacter();
            }
        }
        
        // Load equipment data
        LoadEquipmentData();
        
        // Apply equipment stats to character
        ApplyEquipmentStats();
    }
    
    // Equip an item
    public bool EquipItem(ItemData item)
    {
        if (item == null) return false;
        
        // Check if the item can be equipped in the specified slot
        EquipmentType targetSlot = item.equipmentType;
        ItemData currentlyEquipped = GetEquippedItem(targetSlot);
        
        // Unequip current item if any
        if (currentlyEquipped != null)
        {
            UnequipItem(targetSlot);
        }
        
        // Equip the new item
        SetEquippedItem(targetSlot, item);
        
        // Recalculate and apply equipment stats
        CalculateTotalEquipmentStats();
        ApplyEquipmentStats();
        
        // Save equipment data
        SaveEquipmentData();
        
        Debug.Log($"Equipped {item.itemName} in {targetSlot} slot");
        return true;
    }
    
    // Unequip an item
    public ItemData UnequipItem(EquipmentType slot)
    {
        ItemData unequippedItem = GetEquippedItem(slot);
        if (unequippedItem != null)
        {
            SetEquippedItem(slot, null);
            
            // Recalculate and apply equipment stats
            CalculateTotalEquipmentStats();
            ApplyEquipmentStats();
            
            // Save equipment data
            SaveEquipmentData();
            
            Debug.Log($"Unequipped {unequippedItem.itemName} from {slot} slot");
        }
        
        return unequippedItem;
    }
    
    // Get equipped item for a slot
    public ItemData GetEquippedItem(EquipmentType slot)
    {
        switch (slot)
        {
            case EquipmentType.Helmet:
                return currentEquipment.helmet;
            case EquipmentType.Amulet:
                return currentEquipment.amulet;
            case EquipmentType.MainHand:
                return currentEquipment.mainHand;
            case EquipmentType.BodyArmour:
                return currentEquipment.bodyArmour;
            case EquipmentType.OffHand:
                return currentEquipment.offHand;
            case EquipmentType.Gloves:
                return currentEquipment.gloves;
            case EquipmentType.LeftRing:
                return currentEquipment.leftRing;
            case EquipmentType.RightRing:
                return currentEquipment.rightRing;
            case EquipmentType.Belt:
                return currentEquipment.belt;
            case EquipmentType.Boots:
                return currentEquipment.boots;
            default:
                return null;
        }
    }
    
    // Set equipped item for a slot
    private void SetEquippedItem(EquipmentType slot, ItemData item)
    {
        switch (slot)
        {
            case EquipmentType.Helmet:
                currentEquipment.helmet = item;
                break;
            case EquipmentType.Amulet:
                currentEquipment.amulet = item;
                break;
            case EquipmentType.MainHand:
                currentEquipment.mainHand = item;
                break;
            case EquipmentType.BodyArmour:
                currentEquipment.bodyArmour = item;
                break;
            case EquipmentType.OffHand:
                currentEquipment.offHand = item;
                break;
            case EquipmentType.Gloves:
                currentEquipment.gloves = item;
                break;
            case EquipmentType.LeftRing:
                currentEquipment.leftRing = item;
                break;
            case EquipmentType.RightRing:
                currentEquipment.rightRing = item;
                break;
            case EquipmentType.Belt:
                currentEquipment.belt = item;
                break;
            case EquipmentType.Boots:
                currentEquipment.boots = item;
                break;
        }
    }
    
    // Calculate total stats from all equipped items
    public void CalculateTotalEquipmentStats()
    {
        totalEquipmentStats.Clear();
        
        // Process all equipment slots
        EquipmentType[] allSlots = {
            EquipmentType.Helmet, EquipmentType.Amulet, EquipmentType.MainHand,
            EquipmentType.BodyArmour, EquipmentType.OffHand, EquipmentType.Gloves,
            EquipmentType.LeftRing, EquipmentType.RightRing, EquipmentType.Belt, EquipmentType.Boots
        };
        
        foreach (EquipmentType slot in allSlots)
        {
            ItemData item = GetEquippedItem(slot);
            if (item != null && item.stats != null)
            {
                foreach (var stat in item.stats)
                {
                    if (totalEquipmentStats.ContainsKey(stat.Key))
                    {
                        totalEquipmentStats[stat.Key] += stat.Value;
                    }
                    else
                    {
                        totalEquipmentStats[stat.Key] = stat.Value;
                    }
                }
            }
        }
        
        Debug.Log($"Total equipment stats calculated: {totalEquipmentStats.Count} stats");
    }
    
    // Apply equipment stats to character
    public void ApplyEquipmentStats()
    {
        if (currentCharacter == null) return;
        
        // Reset character stats to base values
        currentCharacter.CalculateDerivedStats();
        
        // Apply equipment modifiers
        DamageModifiers equipmentModifiers = new DamageModifiers();
        
        // Process all equipment stats
        foreach (var stat in totalEquipmentStats)
        {
            ApplyStatToCharacter(stat.Key, stat.Value, equipmentModifiers);
        }
        
        // Apply damage modifiers to character
        currentCharacter.ApplyEquipmentModifiers(equipmentModifiers);
        
        Debug.Log($"Applied equipment stats to character: {totalEquipmentStats.Count} stats");
    }
    
    // Apply a single stat to the character
    private void ApplyStatToCharacter(string statName, float value, DamageModifiers equipmentModifiers)
    {
        // Handle different stat types
        switch (statName)
        {
            // Damage stats
            case "PhysicalDamage":
                equipmentModifiers.addedPhysicalDamage += value;
                break;
            case "FireDamage":
                equipmentModifiers.addedFireDamage += value;
                break;
            case "ColdDamage":
                equipmentModifiers.addedColdDamage += value;
                break;
            case "LightningDamage":
                equipmentModifiers.addedLightningDamage += value;
                break;
            case "ChaosDamage":
                equipmentModifiers.addedChaosDamage += value;
                break;
                
            // Increased damage stats
            case "IncreasedPhysicalDamage":
                equipmentModifiers.increasedPhysicalDamage.Add(value);
                break;
            case "IncreasedFireDamage":
                equipmentModifiers.increasedFireDamage.Add(value);
                break;
            case "IncreasedColdDamage":
                equipmentModifiers.increasedColdDamage.Add(value);
                break;
            case "IncreasedLightningDamage":
                equipmentModifiers.increasedLightningDamage.Add(value);
                break;
            case "IncreasedChaosDamage":
                equipmentModifiers.increasedChaosDamage.Add(value);
                break;
                
            // Defense stats
            case "Armour":
                currentCharacter.damageStats.physicalResistance += value;
                break;
            case "Evasion":
                // Add evasion to character stats
                break;
            case "EnergyShield":
                // Add energy shield to character stats
                break;
                
            // Attribute stats
            case "Strength":
                currentCharacter.strength += (int)value;
                break;
            case "Dexterity":
                currentCharacter.dexterity += (int)value;
                break;
            case "Intelligence":
                currentCharacter.intelligence += (int)value;
                break;
                
            // Critical stats
            case "CriticalStrikeChance":
                equipmentModifiers.criticalStrikeChance += value;
                break;
            case "CriticalStrikeMultiplier":
                equipmentModifiers.criticalStrikeMultiplier += value;
                break;
                
            // Other stats
            case "AttackSpeed":
                // Add attack speed to character stats
                break;
            case "MovementSpeed":
                // Add movement speed to character stats
                break;
        }
    }
    
    // Save equipment data to PlayerPrefs
    public void SaveEquipmentData()
    {
        if (currentCharacter == null) return;
        
        string prefix = $"Equipment_{currentCharacter.characterName}_";
        
        // Save equipped items (simplified - just save item names for now)
        SaveEquippedItem(prefix, "Helmet", currentEquipment.helmet);
        SaveEquippedItem(prefix, "Amulet", currentEquipment.amulet);
        SaveEquippedItem(prefix, "MainHand", currentEquipment.mainHand);
        SaveEquippedItem(prefix, "BodyArmour", currentEquipment.bodyArmour);
        SaveEquippedItem(prefix, "OffHand", currentEquipment.offHand);
        SaveEquippedItem(prefix, "Gloves", currentEquipment.gloves);
        SaveEquippedItem(prefix, "LeftRing", currentEquipment.leftRing);
        SaveEquippedItem(prefix, "RightRing", currentEquipment.rightRing);
        SaveEquippedItem(prefix, "Belt", currentEquipment.belt);
        SaveEquippedItem(prefix, "Boots", currentEquipment.boots);
        
        Debug.Log($"Saved equipment data for {currentCharacter.characterName}");
    }
    
    // Load equipment data from PlayerPrefs
    public void LoadEquipmentData()
    {
        if (currentCharacter == null) return;
        
        string prefix = $"Equipment_{currentCharacter.characterName}_";
        
        // Load equipped items (simplified - just load item names for now)
        currentEquipment.helmet = LoadEquippedItem(prefix, "Helmet");
        currentEquipment.amulet = LoadEquippedItem(prefix, "Amulet");
        currentEquipment.mainHand = LoadEquippedItem(prefix, "MainHand");
        currentEquipment.bodyArmour = LoadEquippedItem(prefix, "BodyArmour");
        currentEquipment.offHand = LoadEquippedItem(prefix, "OffHand");
        currentEquipment.gloves = LoadEquippedItem(prefix, "Gloves");
        currentEquipment.leftRing = LoadEquippedItem(prefix, "LeftRing");
        currentEquipment.rightRing = LoadEquippedItem(prefix, "RightRing");
        currentEquipment.belt = LoadEquippedItem(prefix, "Belt");
        currentEquipment.boots = LoadEquippedItem(prefix, "Boots");
        
        Debug.Log($"Loaded equipment data for {currentCharacter.characterName}");
    }
    
    // Helper method to save equipped item
    private void SaveEquippedItem(string prefix, string slotName, ItemData item)
    {
        if (item != null)
        {
            PlayerPrefs.SetString(prefix + slotName, item.itemName);
        }
        else
        {
            PlayerPrefs.DeleteKey(prefix + slotName);
        }
    }
    
    // Helper method to load equipped item
    private ItemData LoadEquippedItem(string prefix, string slotName)
    {
        string itemName = PlayerPrefs.GetString(prefix + slotName, "");
        if (string.IsNullOrEmpty(itemName)) return null;
        
        // Try to find the item in the database
        // This is a simplified version - in a full implementation, you'd want to save/load the full item data
        return null; // For now, return null to avoid complexity
    }
    
    // Get total equipment stats for display
    public Dictionary<string, float> GetTotalEquipmentStats()
    {
        return new Dictionary<string, float>(totalEquipmentStats);
    }
    
    // Get equipment summary for character info
    public string GetEquipmentSummary()
    {
        int equippedCount = 0;
        if (currentEquipment.helmet != null) equippedCount++;
        if (currentEquipment.amulet != null) equippedCount++;
        if (currentEquipment.mainHand != null) equippedCount++;
        if (currentEquipment.bodyArmour != null) equippedCount++;
        if (currentEquipment.offHand != null) equippedCount++;
        if (currentEquipment.gloves != null) equippedCount++;
        if (currentEquipment.leftRing != null) equippedCount++;
        if (currentEquipment.rightRing != null) equippedCount++;
        if (currentEquipment.belt != null) equippedCount++;
        if (currentEquipment.boots != null) equippedCount++;
        
        return $"Equipped Items: {equippedCount}/10";
    }
}
