using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

namespace Dexiled.UI.EquipmentScreen
{
    /// <summary>
    /// Equipment Manager for Unity UI system.
    /// Manages equipment slots, stat calculations, and integrates with CharacterStats.
    /// Designed to work with BaseItem ScriptableObjects (WeaponItem, Armour, etc.).
    /// </summary>
    public class EquipmentManagerUI : MonoBehaviour
    {
        [Header("Slot References")]
        [SerializeField] private EquipmentSlotUI helmetSlot;
        [SerializeField] private EquipmentSlotUI bodyArmourSlot;
        [SerializeField] private EquipmentSlotUI beltSlot;
        [SerializeField] private EquipmentSlotUI glovesSlot;
        [SerializeField] private EquipmentSlotUI bootsSlot;
        [SerializeField] private EquipmentSlotUI ring1Slot;
        [SerializeField] private EquipmentSlotUI ring2Slot;
        [SerializeField] private EquipmentSlotUI amuletSlot;
        [SerializeField] private EquipmentSlotUI weaponSlot;
        [SerializeField] private EquipmentSlotUI offhandSlot;

        [Header("System References")]
        [SerializeField] private EquipmentManager equipmentManager;
        [SerializeField] private Character currentCharacter;

        [Header("Settings")]
        [SerializeField] private bool initializeOnStart = true;

        private Dictionary<EquipmentType, EquipmentSlotUI> slotMap = new Dictionary<EquipmentType, EquipmentSlotUI>();

        #region Unity Lifecycle

        private void Start()
        {
            if (initializeOnStart)
            {
                Initialize();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the equipment manager and all slot references.
        /// </summary>
        public void Initialize()
        {
            // Get EquipmentManager instance if not assigned
            if (equipmentManager == null)
            {
                equipmentManager = EquipmentManager.Instance;
                if (equipmentManager == null)
                {
                    Debug.LogError("[EquipmentManagerUI] EquipmentManager.Instance not found!");
                    return;
                }
            }

            // Get Character reference
            if (currentCharacter == null)
            {
                CharacterManager charManager = FindFirstObjectByType<CharacterManager>();
                if (charManager != null)
                {
                    currentCharacter = charManager.GetCurrentCharacter();
                    equipmentManager.currentCharacter = currentCharacter;
                }
                else
                {
                    Debug.LogWarning("[EquipmentManagerUI] CharacterManager not found. Stats will not be applied.");
                }
            }

            // Build slot mapping
            BuildSlotMap();

            // Subscribe to slot events
            SubscribeToSlotEvents();

            // Load any saved equipment and update displays
            LoadSavedEquipment();

            Debug.Log("[EquipmentManagerUI] Equipment manager initialized successfully");
        }

        /// <summary>
        /// Equips a BaseItem (ScriptableObject) to the appropriate slot.
        /// Converts BaseItem to stat calculations and applies to character.
        /// </summary>
        public bool EquipItem(BaseItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[EquipmentManagerUI] Cannot equip null item");
                return false;
            }

            EquipmentType slotType = item.equipmentType;

            // Verify slot exists
            if (!slotMap.ContainsKey(slotType))
            {
                Debug.LogError($"[EquipmentManagerUI] No slot mapped for EquipmentType: {slotType}");
                return false;
            }

            // Check if character can equip this item
            if (!CanEquipItem(item))
            {
                Debug.LogWarning($"[EquipmentManagerUI] {currentCharacter.characterName} cannot equip {item.itemName}");
                return false;
            }

            // Get the slot UI component
            EquipmentSlotUI slotUI = slotMap[slotType];

            // Check if there's already an item equipped
            BaseItem previouslyEquipped = GetEquippedBaseItem(slotType);
            if (previouslyEquipped != null)
            {
                UnequipItem(slotType);
            }

            // Update the slot's visual display
            slotUI.SetEquippedItem(ConvertToItemData(item));

            // Calculate stats from the BaseItem and apply to character
            CalculateAndApplyStats();

            Debug.Log($"[EquipmentManagerUI] Equipped {item.itemName} in {slotType} slot");
            return true;
        }

        /// <summary>
        /// Unequips an item from a slot.
        /// </summary>
        public bool UnequipItem(EquipmentType slotType)
        {
            if (!slotMap.ContainsKey(slotType))
            {
                Debug.LogError($"[EquipmentManagerUI] No slot mapped for EquipmentType: {slotType}");
                return false;
            }

            EquipmentSlotUI slotUI = slotMap[slotType];
            BaseItem equippedItem = GetEquippedBaseItem(slotType);

            if (equippedItem != null)
            {
                // Clear the slot display
                slotUI.SetEquippedItem(null);

                // Recalculate stats without this item
                CalculateAndApplyStats();

                Debug.Log($"[EquipmentManagerUI] Unequipped {equippedItem.itemName} from {slotType} slot");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the BaseItem equipped in a specific slot (for compatibility).
        /// </summary>
        public BaseItem GetEquippedBaseItem(EquipmentType slotType)
        {
            if (!slotMap.ContainsKey(slotType))
                return null;

            EquipmentSlotUI slotUI = slotMap[slotType];
            ItemData itemData = slotUI.GetEquippedItem();

            // TODO: Convert ItemData back to BaseItem if needed
            // For now, returns null as we're primarily working with BaseItems
            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds the mapping from EquipmentType to EquipmentSlotUI components.
        /// </summary>
        private void BuildSlotMap()
        {
            slotMap.Clear();

            if (helmetSlot != null)
                slotMap[EquipmentType.Helmet] = helmetSlot;

            if (bodyArmourSlot != null)
                slotMap[EquipmentType.BodyArmour] = bodyArmourSlot;

            if (beltSlot != null)
                slotMap[EquipmentType.Belt] = beltSlot;

            if (glovesSlot != null)
                slotMap[EquipmentType.Gloves] = glovesSlot;

            if (bootsSlot != null)
                slotMap[EquipmentType.Boots] = bootsSlot;

            if (ring1Slot != null)
                slotMap[EquipmentType.LeftRing] = ring1Slot;

            if (ring2Slot != null)
                slotMap[EquipmentType.RightRing] = ring2Slot;

            if (amuletSlot != null)
                slotMap[EquipmentType.Amulet] = amuletSlot;

            if (weaponSlot != null)
                slotMap[EquipmentType.MainHand] = weaponSlot;

            if (offhandSlot != null)
                slotMap[EquipmentType.OffHand] = offhandSlot;

            Debug.Log($"[EquipmentManagerUI] Built slot map with {slotMap.Count} slots");
        }

        /// <summary>
        /// Subscribes to slot click and hover events.
        /// </summary>
        private void SubscribeToSlotEvents()
        {
            foreach (var kvp in slotMap)
            {
                kvp.Value.OnSlotClicked += (type) => OnSlotClicked(type);
                kvp.Value.OnSlotHovered += (type, pos) => OnSlotHovered(type, pos);
            }
        }

        /// <summary>
        /// Handles slot click events.
        /// </summary>
        private void OnSlotClicked(EquipmentType slotType)
        {
            Debug.Log($"[EquipmentManagerUI] Slot clicked: {slotType}");
            // TODO: Implement drag & drop or item selection logic
        }

        /// <summary>
        /// Handles slot hover events.
        /// </summary>
        private void OnSlotHovered(EquipmentType slotType, Vector2 position)
        {
            if (ItemTooltipManager.Instance == null)
                return;

            if (!slotMap.TryGetValue(slotType, out var slot) || slot == null)
            {
                ItemTooltipManager.Instance.HideTooltip();
                return;
            }

            ItemData equipped = slot.GetEquippedItem();
            if (equipped == null)
            {
                ItemTooltipManager.Instance.HideTooltip();
                return;
            }

            ItemTooltipManager.Instance.ShowEquipmentTooltip(equipped, position);
        }

        /// <summary>
        /// Checks if the character can equip an item (level and attribute requirements).
        /// </summary>
        private bool CanEquipItem(BaseItem item)
        {
            if (currentCharacter == null || item == null)
                return false;

            // Check level requirement
            if (currentCharacter.level < item.requiredLevel)
                return false;

            // Check attribute requirements based on item type
            if (item is WeaponItem weapon)
            {
                if (currentCharacter.strength < weapon.requiredStrength ||
                    currentCharacter.dexterity < weapon.requiredDexterity ||
                    currentCharacter.intelligence < weapon.requiredIntelligence)
                    return false;
            }
            else if (item is Armour armor)
            {
                if (currentCharacter.strength < armor.requiredStrength ||
                    currentCharacter.dexterity < armor.requiredDexterity ||
                    currentCharacter.intelligence < armor.requiredIntelligence)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates total stats from all equipped items and applies them to character.
        /// </summary>
        private void CalculateAndApplyStats()
        {
            if (equipmentManager == null || currentCharacter == null)
                return;

            // Collect all equipped items
            Dictionary<string, float> totalStats = new Dictionary<string, float>();

            foreach (var kvp in slotMap)
            {
                ItemData equipped = kvp.Value.GetEquippedItem();
                if (equipped != null && equipped.stats != null)
                {
                    foreach (var stat in equipped.stats)
                    {
                        if (totalStats.ContainsKey(stat.Key))
                            totalStats[stat.Key] += stat.Value;
                        else
                            totalStats[stat.Key] = stat.Value;
                    }
                }
            }

            // Apply stats through EquipmentManager
            equipmentManager.totalEquipmentStats = totalStats;
            equipmentManager.ApplyEquipmentStats();

            Debug.Log($"[EquipmentManagerUI] Calculated and applied {totalStats.Count} stats");
        }

        /// <summary>
        /// Loads any saved equipment from EquipmentManager and displays it.
        /// </summary>
        private void LoadSavedEquipment()
        {
            if (equipmentManager == null)
                return;

            // Load equipment from the old system if it exists
            equipmentManager.LoadEquipmentData();

            // Update visual displays for each slot
            foreach (var kvp in slotMap)
            {
                ItemData savedItem = equipmentManager.GetEquippedItem(kvp.Key);
                if (savedItem != null)
                {
                    kvp.Value.SetEquippedItem(savedItem);
                }
            }
        }

        /// <summary>
        /// Converts a BaseItem ScriptableObject to ItemData for compatibility.
        /// </summary>
        private ItemData ConvertToItemData(BaseItem baseItem)
        {
            if (baseItem == null) return null;

            ItemData itemData = new ItemData
            {
                itemName = baseItem.itemName,
                itemType = baseItem.itemType,
                equipmentType = baseItem.equipmentType,
                rarity = baseItem.GetCalculatedRarity(),
                level = baseItem.requiredLevel,
                itemSprite = baseItem.itemIcon,
                requiredLevel = baseItem.requiredLevel,
                sourceItem = baseItem
            };

            // Convert weapon-specific properties
            if (baseItem is WeaponItem weapon)
            {
                itemData.baseDamageMin = weapon.minDamage;
                itemData.baseDamageMax = weapon.maxDamage;
                itemData.criticalStrikeChance = weapon.criticalStrikeChance;
                itemData.attackSpeed = weapon.attackSpeed;
                itemData.requiredStrength = weapon.requiredStrength;
                itemData.requiredDexterity = weapon.requiredDexterity;
                itemData.requiredIntelligence = weapon.requiredIntelligence;
            }

            // Convert armor-specific properties
            if (baseItem is Armour armor)
            {
                itemData.baseArmour = armor.armour;
                itemData.baseEvasion = armor.evasion;
                itemData.baseEnergyShield = armor.energyShield;
                itemData.requiredStrength = armor.requiredStrength;
                itemData.requiredDexterity = armor.requiredDexterity;
                itemData.requiredIntelligence = armor.requiredIntelligence;
            }

            // Convert affixes to stat dictionary
            itemData.stats = ConvertAffixesToStats(baseItem);

            // Convert modifiers to string lists (for tooltips)
            ConvertAffixesToStrings(baseItem, itemData);

            return itemData;
        }

        /// <summary>
        /// Converts affixes from BaseItem to a stats dictionary for EquipmentManager.
        /// </summary>
        private Dictionary<string, float> ConvertAffixesToStats(BaseItem baseItem)
        {
            Dictionary<string, float> stats = new Dictionary<string, float>();

            // Add base item properties as stats
            if (baseItem is WeaponItem weapon)
            {
                // Weapon damage is handled separately via DamageModifiers, but we can store it
                stats["WeaponMinDamage"] = weapon.minDamage;
                stats["WeaponMaxDamage"] = weapon.maxDamage;
                stats["WeaponCritChance"] = weapon.criticalStrikeChance;
                stats["WeaponAttackSpeed"] = weapon.attackSpeed;
            }
            else if (baseItem is Armour armor)
            {
                stats["Armour"] = armor.armour;
                stats["Evasion"] = armor.evasion;
                stats["EnergyShield"] = armor.energyShield;
            }

            // Process all affixes (implicit, prefixes, suffixes)
            List<Affix> allAffixes = new List<Affix>();
            allAffixes.AddRange(baseItem.implicitModifiers);
            allAffixes.AddRange(baseItem.prefixes);
            allAffixes.AddRange(baseItem.suffixes);

            foreach (Affix affix in allAffixes)
            {
                foreach (AffixModifier modifier in affix.modifiers)
                {
                    // Get the actual value from the modifier
                    float value = GetModifierValue(modifier);

                    if (!string.IsNullOrEmpty(modifier.statName))
                    {
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
        /// Gets the actual value from an AffixModifier.
        /// </summary>
        private float GetModifierValue(AffixModifier modifier)
        {
            if (modifier.isDualRange)
            {
                // For dual-range, use the average of both ranges
                return (modifier.rolledFirstValue + modifier.rolledSecondValue) / 2f;
            }
            else
            {
                // Use the rolled value or minValue as fallback
                return modifier.minValue;
            }
        }

        /// <summary>
        /// Converts affixes to string lists for tooltip display.
        /// </summary>
        private void ConvertAffixesToStrings(BaseItem baseItem, ItemData itemData)
        {
            foreach (Affix affix in baseItem.implicitModifiers)
            {
                if (!string.IsNullOrEmpty(affix.description))
                    itemData.implicitModifiers.Add(affix.description);
            }

            foreach (Affix affix in baseItem.prefixes)
            {
                if (!string.IsNullOrEmpty(affix.description))
                    itemData.prefixModifiers.Add(affix.description);
            }

            foreach (Affix affix in baseItem.suffixes)
            {
                if (!string.IsNullOrEmpty(affix.description))
                    itemData.suffixModifiers.Add(affix.description);
            }
        }

        #endregion

        #region Editor Helpers
#if UNITY_EDITOR
        /// <summary>
        /// Auto-assigns slot references from children.
        /// </summary>
        [ContextMenu("Auto-Assign Slots")]
        private void AutoAssignSlots()
        {
            EquipmentSlotUI[] allSlots = GetComponentsInChildren<EquipmentSlotUI>(true);

            foreach (var slot in allSlots)
            {
                if (slot.slotType == EquipmentType.Helmet && helmetSlot == null)
                {
                    helmetSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned helmetSlot");
                }
                else if (slot.slotType == EquipmentType.BodyArmour && bodyArmourSlot == null)
                {
                    bodyArmourSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned bodyArmourSlot");
                }
                else if (slot.slotType == EquipmentType.Belt && beltSlot == null)
                {
                    beltSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned beltSlot");
                }
                else if (slot.slotType == EquipmentType.Gloves && glovesSlot == null)
                {
                    glovesSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned glovesSlot");
                }
                else if (slot.slotType == EquipmentType.Boots && bootsSlot == null)
                {
                    bootsSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned bootsSlot");
                }
                else if (slot.slotType == EquipmentType.LeftRing && ring1Slot == null)
                {
                    ring1Slot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned ring1Slot");
                }
                else if (slot.slotType == EquipmentType.RightRing && ring2Slot == null)
                {
                    ring2Slot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned ring2Slot");
                }
                else if (slot.slotType == EquipmentType.Amulet && amuletSlot == null)
                {
                    amuletSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned amuletSlot");
                }
                else if (slot.slotType == EquipmentType.MainHand && weaponSlot == null)
                {
                    weaponSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned weaponSlot");
                }
                else if (slot.slotType == EquipmentType.OffHand && offhandSlot == null)
                {
                    offhandSlot = slot;
                    Debug.Log("[EquipmentManagerUI] Auto-assigned offhandSlot");
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Finds and assigns EquipmentManager instance (PLAY MODE ONLY).
        /// In Edit Mode, leave the equipmentManager field empty - it will be found at runtime.
        /// </summary>
        [ContextMenu("Find EquipmentManager (Play Mode Only)")]
        private void FindEquipmentManager()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[EquipmentManagerUI] EquipmentManager can only be found in Play Mode. Leave the field empty - it will auto-find at runtime.");
                return;
            }

            equipmentManager = EquipmentManager.Instance;
            if (equipmentManager != null)
            {
                Debug.Log("[EquipmentManagerUI] Found EquipmentManager instance");
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogWarning("[EquipmentManagerUI] EquipmentManager.Instance not found in scene!");
            }
        }
#endif
        #endregion
    }
}

