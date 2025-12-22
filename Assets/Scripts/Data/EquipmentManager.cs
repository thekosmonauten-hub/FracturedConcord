using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EquipmentManager : MonoBehaviour
{
    [Header("Equipment Data")]
    public EquipmentData currentEquipment = new EquipmentData();
    
    [Header("Equipment Stats")]
    public Dictionary<string, float> totalEquipmentStats = new Dictionary<string, float>();
    private Dictionary<string, float> effigyStats = new Dictionary<string, float>();
    
    /// <summary>
    /// Always get current character from CharacterManager (never cache!)
    /// </summary>
    private Character CurrentCharacter => CharacterManager.Instance?.GetCurrentCharacter();
    
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
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Load equipment data and apply stats
        // (Always gets current character from CharacterManager)
        LoadEquipmentData();
        ApplyEquipmentStats();
    }
    
    // Equip an item
    public bool EquipItem(BaseItem item, EquipmentType? targetSlotOverride = null)
    {
        if (item == null) return false;
        
        // Determine target slot: use override if provided, otherwise use item's equipmentType
        // Special case: 1-handed weapons can be equipped in either MainHand or OffHand
        EquipmentType targetSlot;
        if (targetSlotOverride.HasValue)
        {
            targetSlot = targetSlotOverride.Value;
        }
        else if (item is WeaponItem weapon && weapon.handedness == WeaponHandedness.OneHanded)
        {
            // Default 1-handed weapons to MainHand if no override specified
            targetSlot = EquipmentType.MainHand;
        }
        else
        {
            targetSlot = item.equipmentType;
        }
        
        // Validate that the item can actually go in this slot
        if (!CanItemBeEquippedInSlot(item, targetSlot))
        {
            Debug.LogWarning($"[EquipmentManager] Cannot equip {item.itemName} in {targetSlot} slot!");
            return false;
        }
        
        BaseItem currentlyEquipped = GetEquippedItem(targetSlot);
        
        // Unequip current item if any and return it to inventory
        if (currentlyEquipped != null)
        {
            BaseItem unequippedItem = UnequipItem(targetSlot);
            
            // Add the old item back to inventory
            var charManager = CharacterManager.Instance;
            if (charManager != null && unequippedItem != null)
            {
                charManager.inventoryItems.Add(unequippedItem);
                charManager.OnItemAdded?.Invoke(unequippedItem);
                Debug.Log($"[EquipmentManager] Returned {unequippedItem.itemName} to inventory (replaced by {item.itemName})");
            }
        }
        
        // Equip the new item
        SetEquippedItem(targetSlot, item);
        
        // Recalculate and apply equipment stats
        CalculateTotalEquipmentStats();
        ApplyEquipmentStats();
        
        // Update Character.weapons for damage scaling
        UpdateCharacterWeaponReferences();
        
        // Save equipment data
        SaveEquipmentData();
        
        Debug.Log($"Equipped {item.itemName} in {targetSlot} slot");
        return true;
    }
    
    // Unequip an item
    public BaseItem UnequipItem(EquipmentType slot)
    {
        BaseItem unequippedItem = GetEquippedItem(slot);
        if (unequippedItem != null)
        {
            SetEquippedItem(slot, null);
            
            // Special rule: If MainHand is unequipped and OffHand has a weapon, move it to MainHand
            if (slot == EquipmentType.MainHand)
            {
                BaseItem offHandItem = GetEquippedItem(EquipmentType.OffHand);
                if (offHandItem != null && offHandItem is WeaponItem)
                {
                    Debug.Log($"[EquipmentManager] MainHand unequipped. Moving {offHandItem.itemName} from OffHand to MainHand.");
                    
                    // Move offhand weapon to main hand
                    SetEquippedItem(EquipmentType.OffHand, null);
                    SetEquippedItem(EquipmentType.MainHand, offHandItem);
                    
                    Debug.Log($"[EquipmentManager] ✅ {offHandItem.itemName} moved from OffHand to MainHand");
                }
            }
            
            // Recalculate and apply equipment stats
            CalculateTotalEquipmentStats();
            ApplyEquipmentStats();
            
            // Update Character.weapons for damage scaling
            UpdateCharacterWeaponReferences();
            
            // Save equipment data
            SaveEquipmentData();
            
            Debug.Log($"Unequipped {unequippedItem.itemName} from {slot} slot");
        }
        
        return unequippedItem;
    }
    
    // Get equipped item for a slot
    public BaseItem GetEquippedItem(EquipmentType slot)
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
    private void SetEquippedItem(EquipmentType slot, BaseItem item)
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
        effigyStats.Clear();
        
        // Process all equipment slots
        EquipmentType[] allSlots = {
            EquipmentType.Helmet, EquipmentType.Amulet, EquipmentType.MainHand,
            EquipmentType.BodyArmour, EquipmentType.OffHand, EquipmentType.Gloves,
            EquipmentType.LeftRing, EquipmentType.RightRing, EquipmentType.Belt, EquipmentType.Boots
        };
        
        foreach (EquipmentType slot in allSlots)
        {
            BaseItem item = GetEquippedItem(slot);
            if (item != null)
            {
                // Get all modifier stats from BaseItem
                Dictionary<string, float> itemStats = GetItemStats(item);
                foreach (var stat in itemStats)
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
        
        // Process equipped effigies
        CalculateEffigyStats();
        
        // Merge effigy stats into total equipment stats
        foreach (var stat in effigyStats)
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
        
        Debug.Log($"Total equipment stats calculated: {totalEquipmentStats.Count} stats (including {effigyStats.Count} from effigies)");
    }
    
    /// <summary>
    /// Calculate stats from all equipped effigies
    /// </summary>
    private void CalculateEffigyStats()
    {
        effigyStats.Clear();
        
        Character character = CurrentCharacter;
        if (character == null || character.equippedEffigies == null)
        {
            Debug.Log("[EquipmentManager] No character or equipped effigies to calculate stats from.");
            return;
        }
        
        Debug.Log($"[EquipmentManager] Calculating stats for {character.equippedEffigies.Count} equipped effigies.");
        
        foreach (Effigy effigy in character.equippedEffigies)
        {
            if (effigy == null)
            {
                Debug.LogWarning("[EquipmentManager] Encountered null effigy in equippedEffigies list. Skipping.");
                continue;
            }
            
            Dictionary<string, float> effigyStatValues = GetEffigyStats(effigy);
            Debug.Log($"[EquipmentManager] Effigy '{effigy.effigyName}' contributes {effigyStatValues.Count} stats:");
            foreach (var stat in effigyStatValues)
            {
                Debug.Log($"  - {stat.Key}: {stat.Value}");
                if (effigyStats.ContainsKey(stat.Key))
                {
                    effigyStats[stat.Key] += stat.Value;
                }
                else
                {
                    effigyStats[stat.Key] = stat.Value;
                }
            }
        }
        Debug.Log($"[EquipmentManager] Total effigy stats aggregated: {effigyStats.Count} stats.");
    }
    
    /// <summary>
    /// Get all stats from a single effigy (implicit + rolled affixes)
    /// </summary>
    public Dictionary<string, float> GetEffigyStats(Effigy effigy)
    {
        Dictionary<string, float> stats = new Dictionary<string, float>();
        
        if (effigy == null) return stats;
        
        // Process implicit modifiers
        if (effigy.implicitModifiers != null)
        {
            foreach (Affix implicitAffix in effigy.implicitModifiers)
            {
                if (implicitAffix?.modifiers == null) continue;
                foreach (AffixModifier modifier in implicitAffix.modifiers)
                {
                    float value = modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                    if (stats.ContainsKey(modifier.statName))
                        stats[modifier.statName] += value;
                    else
                        stats[modifier.statName] = value;
                }
            }
        }
        
        // Process rolled prefixes
        if (effigy.prefixes != null)
        {
            foreach (Affix prefix in effigy.prefixes)
            {
                if (prefix?.modifiers == null) continue;
                foreach (AffixModifier modifier in prefix.modifiers)
                {
                    float value = modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                    if (stats.ContainsKey(modifier.statName))
                        stats[modifier.statName] += value;
                    else
                        stats[modifier.statName] = value;
                }
            }
        }
        
        // Process rolled suffixes
        if (effigy.suffixes != null)
        {
            foreach (Affix suffix in effigy.suffixes)
            {
                if (suffix?.modifiers == null) continue;
                foreach (AffixModifier modifier in suffix.modifiers)
                {
                    float value = modifier.isRolled ? modifier.rolledValue : modifier.minValue;
                    if (stats.ContainsKey(modifier.statName))
                        stats[modifier.statName] += value;
                    else
                        stats[modifier.statName] = value;
                }
            }
        }
        
        return stats;
    }
    
    /// <summary>
    /// Helper method to get all stats from a BaseItem
    /// </summary>
    private Dictionary<string, float> GetItemStats(BaseItem item)
    {
        Dictionary<string, float> stats = new Dictionary<string, float>();
        
        if (item == null) return stats;
        
        // Process all affixes (implicit, prefixes, suffixes)
        List<Affix> allAffixes = new List<Affix>();
        allAffixes.AddRange(item.implicitModifiers);
        allAffixes.AddRange(item.prefixes);
        allAffixes.AddRange(item.suffixes);
        
        foreach (Affix affix in allAffixes)
        {
            foreach (AffixModifier modifier in affix.modifiers)
            {
                float value = modifier.minValue; // Use rolled value
                
                if (stats.ContainsKey(modifier.statName))
                    stats[modifier.statName] += value;
                else
                    stats[modifier.statName] = value;
            }
        }
        
        return stats;
    }
    
    /// <summary>
    /// Update Character.weapons with equipped weapon data
    /// This is critical for card damage scaling!
    /// </summary>
    private void UpdateCharacterWeaponReferences()
    {
        Character character = CurrentCharacter;
        if (character == null)
        {
            Debug.LogWarning("[EquipmentManager] No current character to update weapons!");
            return;
        }
        
        // Get main hand and off hand
        BaseItem mainHand = GetEquippedItem(EquipmentType.MainHand);
        BaseItem offHand = GetEquippedItem(EquipmentType.OffHand);
        
        // Clear existing weapons
        character.weapons.meleeWeapon = null;
        character.weapons.projectileWeapon = null;
        character.weapons.spellWeapon = null;
        
        // Assign main hand weapon
        if (mainHand is WeaponItem mainWeapon)
        {
            Weapon weaponData = ConvertWeaponItemToWeapon(mainWeapon);
            AssignWeaponByType(character, mainWeapon.weaponType, weaponData);
            Debug.Log($"[EquipmentManager] Updated Character.weapons: MainHand = {mainWeapon.itemName} ({mainWeapon.weaponType})");
        }
        
        // Assign off hand weapon (if dual wielding)
        if (offHand is WeaponItem offWeapon)
        {
            Weapon weaponData = ConvertWeaponItemToWeapon(offWeapon);
            AssignWeaponByType(character, offWeapon.weaponType, weaponData);
            Debug.Log($"[EquipmentManager] Updated Character.weapons: OffHand = {offWeapon.itemName} ({offWeapon.weaponType})");
        }
        
        Debug.Log($"[EquipmentManager] ✅ Character weapon data synced for damage scaling!");
    }
    
    private void AssignWeaponByType(Character character, WeaponItemType weaponType, Weapon weaponData)
    {
        switch (weaponType)
        {
            case WeaponItemType.Sword:
            case WeaponItemType.Axe:
            case WeaponItemType.Mace:
            case WeaponItemType.Dagger:
            case WeaponItemType.Claw:
            case WeaponItemType.RitualDagger:
                character.weapons.meleeWeapon = weaponData;
                break;
            case WeaponItemType.Bow:
                character.weapons.projectileWeapon = weaponData;
                break;
            case WeaponItemType.Wand:
            case WeaponItemType.Staff:
            case WeaponItemType.Sceptre:
                character.weapons.spellWeapon = weaponData;
                break;
        }
    }
    
    private Weapon ConvertWeaponItemToWeapon(WeaponItem weaponItem)
    {
        Weapon weapon = new Weapon
        {
            weaponName = weaponItem.itemName,
            weaponType = ConvertWeaponItemTypeToWeaponType(weaponItem.weaponType),
            weaponItemType = weaponItem.weaponType, // Store original weapon item type for weapon-type modifiers
            attackSpeed = weaponItem.attackSpeed,
            baseDamageMin = weaponItem.minDamage,
            baseDamageMax = weaponItem.maxDamage,
            rolledBaseDamage = weaponItem.rolledBaseDamage, // Transfer rolled value for card scaling
            baseDamageType = weaponItem.primaryDamageType
        };
        
        // Calculate total damage including affixes
        weapon.CalculateTotalDamage();
        
        Debug.Log($"[EquipmentManager] Converted {weaponItem.itemName}: Base range {weapon.baseDamageMin}-{weapon.baseDamageMax}, Rolled: {weapon.rolledBaseDamage:F1}");
        
        return weapon;
    }
    
    private WeaponType ConvertWeaponItemTypeToWeaponType(WeaponItemType itemType)
    {
        switch (itemType)
        {
            case WeaponItemType.Sword:
            case WeaponItemType.Axe:
            case WeaponItemType.Mace:
            case WeaponItemType.Dagger:
            case WeaponItemType.Claw:
            case WeaponItemType.RitualDagger:
                return WeaponType.Melee;
            case WeaponItemType.Bow:
                return WeaponType.Projectile;
            case WeaponItemType.Wand:
            case WeaponItemType.Staff:
            case WeaponItemType.Sceptre:
                return WeaponType.Spell;
            default:
                return WeaponType.Melee;
        }
    }
    
    // Apply equipment stats to character
    public void ApplyEquipmentStats()
    {
        Character character = CurrentCharacter;
        if (character == null)
        {
            Debug.LogWarning("[EquipmentManager] No current character to apply equipment stats!");
            return;
        }
        
        // Reset character stats to base values
        character.CalculateDerivedStats();
        
        // Reset equipment-accumulated stats (these are added with +=, so need to be reset)
        character.baseArmourFromItems = 0f;
        character.baseEvasionFromItems = 0f;
        character.baseEnergyShieldFromItems = 0f;
        if (character.warrantStatModifiers != null)
        {
            character.warrantStatModifiers.Clear();
        }
        character.increasedDamage = 0f;
        character.increasedEvasion = 0f;
        
        // Apply equipment modifiers
        DamageModifiers equipmentModifiers = new DamageModifiers();

        // totalEquipmentStats already includes effigy stats (merged in CalculateTotalEquipmentStats)
        // So we can use it directly without merging again
        Dictionary<string, float> combinedStats = new Dictionary<string, float>(totalEquipmentStats);
        
        // Process all equipment stats
        foreach (var stat in combinedStats)
        {
            ApplyStatToCharacter(character, stat.Key, stat.Value, equipmentModifiers);
        }
        
        // Apply damage modifiers to character
        character.ApplyEquipmentModifiers(equipmentModifiers);
        
        Debug.Log($"[EquipmentManager] Applied equipment stats to {character.characterName}: {combinedStats.Count} stats");
    }

    public void SetEffigyStats(Dictionary<string, float> stats)
    {
        effigyStats = stats ?? new Dictionary<string, float>();
        ApplyEquipmentStats();
    }
    
    // Apply a single stat to the character
    private void ApplyStatToCharacter(Character character, string statName, float value, DamageModifiers equipmentModifiers)
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
                
            // Defense stats - track base values separately for scaling with increased modifiers
            case "Armour":
                character.baseArmourFromItems += value;
                // Also add to physical resistance (legacy support)
                character.damageStats.physicalResistance += value;
                break;
            case "Evasion":
                character.baseEvasionFromItems += value;
                break;
            case "EnergyShield":
                character.baseEnergyShieldFromItems += value;
                break;
                
            // Attribute stats
            case "Strength":
                character.strength += (int)value;
                break;
            case "Dexterity":
                character.dexterity += (int)value;
                break;
            case "Intelligence":
                character.intelligence += (int)value;
                break;
            case "AllAttributes":
                // Single modifier that applies to all three attributes equally
                character.strength += (int)value;
                character.dexterity += (int)value;
                character.intelligence += (int)value;
                Debug.Log($"[EquipmentManager] Added {value:F0} to all attributes (Strength, Dexterity, Intelligence)");
                break;
            case "IncreasedMaxLifePercent":
                character.maxHealth = Mathf.RoundToInt(character.maxHealth * (1f + value / 100f));
                character.currentHealth = Mathf.Min(character.currentHealth, character.maxHealth);
                character.UpdateMaxGuard();
                break;
            case "maxHealth": // Flat max health (from effigies)
                character.maxHealth += Mathf.RoundToInt(value);
                character.currentHealth = Mathf.Min(character.currentHealth, character.maxHealth);
                character.UpdateMaxGuard();
                Debug.Log($"[EquipmentManager] Added {value:F0} flat max health. New max: {character.maxHealth}");
                break;
            case "lifeRegeneration":
                if (character.warrantStatModifiers == null)
                {
                    character.warrantStatModifiers = new Dictionary<string, float>();
                }
                if (character.warrantStatModifiers.ContainsKey("lifeRegeneration"))
                {
                    character.warrantStatModifiers["lifeRegeneration"] += value;
                }
                else
                {
                    character.warrantStatModifiers["lifeRegeneration"] = value;
                }
                Debug.Log($"[EquipmentManager] Added {value:F0} life regeneration per turn. Total: {character.warrantStatModifiers["lifeRegeneration"]:F0}");
                break;
            case "IncreasedDamagePercent":
                character.increasedDamage += value / 100f;
                break;
            case "IncreasedEvasionPercent":
                character.increasedEvasion += value / 100f;
                break;
            case "DodgeChancePercent":
                character.dodgeChance += value;
                break;
            case "GuardEffectivenessPercent":
                character.guardEffectivenessPercent += value;
                break;
            case "BuffDurationPercent":
                character.buffDurationIncreasedPercent += value;
                break;
            case "RandomAilmentChancePercent":
                character.randomAilmentChancePercent += value;
                break;
            case "DamageAfterGuardPercent":
                character.increasedDamageAfterGuardPercent += value;
                character.increasedDamage += value / 100f;
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
        Character character = CurrentCharacter;
        if (character == null)
        {
            Debug.LogWarning("[EquipmentManager] No current character to save equipment data!");
            return;
        }
        
        string prefix = $"Equipment_{character.characterName}_";
        
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
        
        Debug.Log($"[EquipmentManager] Saved equipment data for {character.characterName}");
    }
    
    // Load equipment data from PlayerPrefs
    public void LoadEquipmentData()
    {
        Character character = CurrentCharacter;
        if (character == null)
        {
            Debug.LogWarning("[EquipmentManager] No current character to load equipment data!");
            return;
        }
        
        string prefix = $"Equipment_{character.characterName}_";
        
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
        
        Debug.Log($"[EquipmentManager] Loaded equipment data for {character.characterName}");
    }
    
    // Helper method to save equipped item
    private void SaveEquippedItem(string prefix, string slotName, BaseItem item)
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
    private BaseItem LoadEquippedItem(string prefix, string slotName)
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
    
    /// <summary>
    /// Check if an item can be equipped in a specific slot
    /// Special case: 1-handed weapons can go in both MainHand and OffHand
    /// Rule: Cannot equip in OffHand if MainHand is empty (must equip MainHand first)
    /// </summary>
    public bool CanItemBeEquippedInSlot(BaseItem item, EquipmentType slot)
    {
        if (item == null) return false;
        
        // Special case: 1-handed weapons can be equipped in both MainHand and OffHand
        if (item is WeaponItem weapon && weapon.handedness == WeaponHandedness.OneHanded)
        {
            // Rule: Cannot equip in OffHand if MainHand is empty
            if (slot == EquipmentType.OffHand)
            {
                BaseItem mainHandItem = GetEquippedItem(EquipmentType.MainHand);
                if (mainHandItem == null)
                {
                    Debug.LogWarning($"[EquipmentManager] Cannot equip {item.itemName} in OffHand: MainHand must be equipped first!");
                    return false;
                }
            }
            return slot == EquipmentType.MainHand || slot == EquipmentType.OffHand;
        }
        
        // Default: Check if equipment types match
        return item.equipmentType == slot;
    }
}
