using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EquipmentTooltipView : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBackground;
    [SerializeField] private Image iconFrame;

    [Header("Stats")]
    [SerializeField] private List<TextMeshProUGUI> baseStatLabels = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI requirementsLabel;
    
    [Header("Pre-defined Stat Labels")]
    [SerializeField] private TextMeshProUGUI qualityLabel;
    [SerializeField] private TextMeshProUGUI armourLabel;
    [SerializeField] private TextMeshProUGUI energyShieldLabel;
    [SerializeField] private TextMeshProUGUI evasionLabel;
    [SerializeField] private TextMeshProUGUI itemTypeLabel;
    
    // Specific defense value labels (if they exist in prefab)
    private List<TextMeshProUGUI> defenceValueLabels = new List<TextMeshProUGUI>();

    [Header("Affixes")]
    [SerializeField] private TextMeshProUGUI implicitLabel;
    [SerializeField] private List<TextMeshProUGUI> prefixLabels = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> suffixLabels = new List<TextMeshProUGUI>();

    private bool cached;
    private BaseItem currentItem;
    private ItemData currentItemData;
    private bool lastAltState = false;

    private void Awake()
    {
        CacheUIElements();
    }
    
    private void Update()
    {
        // Check if ALT key state changed while tooltip is active
        if (gameObject.activeSelf && (currentItem != null || currentItemData != null))
        {
            bool currentAltState = IsAltKeyHeld();
            
            if (currentAltState != lastAltState)
            {
                lastAltState = currentAltState;
                
                Debug.Log($"<color=yellow>[EquipmentTooltip] ALT state changed: {currentAltState} (showRanges: {currentAltState})</color>");
                
                // Refresh tooltip with new ALT state
                if (currentItem != null)
                {
                    SetData(currentItem);
                }
                else if (currentItemData != null)
                {
                    SetData(currentItemData);
                }
                
                // Force canvas update
                Canvas.ForceUpdateCanvases();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }
        }
    }

    public void SetData(BaseItem item)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            currentItem = null;
            currentItemData = null;
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);
        
        // Cache current item for ALT-key updates
        currentItem = item;
        currentItemData = null;
        
        // Check if ALT key is held (show ranges instead of rolled values)
        bool showRanges = IsAltKeyHeld();
        lastAltState = showRanges;

        ItemRarity rarity = item.GetCalculatedRarity();

        if (nameLabel != null)
        {
            string displayName = item.GetDisplayName();
            nameLabel.text = TooltipFormattingUtils.ColorizeByRarity(displayName, rarity);
        }

        if (iconImage != null)
        {
            iconImage.sprite = item.itemIcon;
            iconImage.enabled = item.itemIcon != null;
        }

        if (iconFrame != null)
        {
            if (ColorUtility.TryParseHtmlString(ItemRarityCalculator.GetRarityColor(rarity), out Color rarityColor))
            {
                iconFrame.color = rarityColor;
            }
        }

        ApplyBaseStats(item);
        ApplyRequirements(item);
        ApplyAffixes(implicitLabel, item.implicitModifiers, showRanges);
        SetAffixTexts(prefixLabels, item.prefixes, showRanges);
        SetAffixTexts(suffixLabels, item.suffixes, showRanges);
    }

    public void SetData(ItemData itemData)
    {
        if (itemData == null)
        {
            gameObject.SetActive(false);
            currentItem = null;
            currentItemData = null;
            return;
        }

        if (itemData.sourceItem != null)
        {
            SetData(itemData.sourceItem);
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);
        
        // Cache current item data for ALT-key updates
        currentItemData = itemData;
        currentItem = null;
        
        // Check if ALT key is held
        bool showRanges = IsAltKeyHeld();
        lastAltState = showRanges;

        ItemRarity rarity = itemData.rarity;

        if (nameLabel != null)
        {
            string displayName = string.IsNullOrWhiteSpace(itemData.itemName) ? "Unnamed Item" : itemData.itemName;
            nameLabel.text = TooltipFormattingUtils.ColorizeByRarity(displayName, rarity);
        }

        if (iconImage != null)
        {
            iconImage.sprite = itemData.itemSprite;
            iconImage.enabled = itemData.itemSprite != null;
        }

        if (iconFrame != null)
        {
            if (ColorUtility.TryParseHtmlString(ItemRarityCalculator.GetRarityColor(rarity), out Color rarityColor))
            {
                iconFrame.color = rarityColor;
            }
        }

        ApplyBaseStats(itemData);
        ApplyRequirements(itemData);
        ApplyAffixes(implicitLabel, itemData.implicitModifiers);
        SetAffixTexts(prefixLabels, itemData.prefixModifiers);
        SetAffixTexts(suffixLabels, itemData.suffixModifiers);
    }

    private void ApplyBaseStats(BaseItem item)
    {
        // Reset all pre-defined labels
        ResetPredefinedLabels();
        
        // Set Item Type
        SetItemTypeLabel(item);
        
        // Handle Armour items specifically for defense values
        if (item is Armour armour)
        {
            ApplyDefenceValues(armour);
        }
        else
        {
            // For non-armour items, populate other stats
            ApplyOtherBaseStats(item);
        }
        
        // Set Quality (applies to all items)
        SetQualityLabel(item);
    }
    
    /// <summary>
    /// Reset all pre-defined stat labels to hidden/empty
    /// </summary>
    private void ResetPredefinedLabels()
    {
        if (qualityLabel != null)
        {
            qualityLabel.text = string.Empty;
            qualityLabel.gameObject.SetActive(false);
        }
        if (armourLabel != null)
        {
            armourLabel.text = string.Empty;
            armourLabel.gameObject.SetActive(false);
        }
        if (energyShieldLabel != null)
        {
            energyShieldLabel.text = string.Empty;
            energyShieldLabel.gameObject.SetActive(false);
        }
        if (evasionLabel != null)
        {
            evasionLabel.text = string.Empty;
            evasionLabel.gameObject.SetActive(false);
        }
        if (itemTypeLabel != null)
        {
            itemTypeLabel.text = string.Empty;
            itemTypeLabel.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set the Item Type label based on item type
    /// </summary>
    private void SetItemTypeLabel(BaseItem item)
    {
        if (itemTypeLabel == null)
            return;
            
        string typeText = string.Empty;
        
        switch (item)
        {
            case Armour armour:
                typeText = $"{armour.armourSlot} - {armour.armourType}";
                break;
            case OffHandEquipment offHand:
                typeText = $"{offHand.offHandSlot} - {offHand.offHandType}";
                break;
            case Jewellery jewellery:
                typeText = $"{jewellery.jewellerySlot} - {jewellery.jewelleryType}";
                break;
            default:
                if (item.itemTags != null && item.itemTags.Count > 0)
                {
                    typeText = $"Tags: {string.Join(", ", item.itemTags)}";
                }
                break;
        }
        
        if (!string.IsNullOrWhiteSpace(typeText))
        {
            itemTypeLabel.text = typeText;
            itemTypeLabel.gameObject.SetActive(true);
        }
        else
        {
            itemTypeLabel.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set the Quality label
    /// </summary>
    private void SetQualityLabel(BaseItem item)
    {
        if (qualityLabel == null)
            return;
            
        if (item.quality > 0)
        {
            qualityLabel.text = $"Quality: +{item.quality}%";
            qualityLabel.gameObject.SetActive(true);
        }
        else
        {
            qualityLabel.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Apply other base stats (non-predefined) to generic labels
    /// </summary>
    private void ApplyOtherBaseStats(BaseItem item)
    {
        var lines = new List<string>();
        
        // Add description if exists
        if (!string.IsNullOrWhiteSpace(item.description))
        {
            lines.Add(item.description);
        }
        
        // Add item-specific stats that aren't in predefined labels
        switch (item)
        {
            case OffHandEquipment offHand:
                if (offHand.defence > 0f) lines.Add($"Defence: {offHand.defence}");
                if (offHand.blockChance > 0f) lines.Add($"Block Chance: {offHand.blockChance:F1}%");
                if (offHand.blockValue > 0f) lines.Add($"Block Value: {offHand.blockValue}");
                if (offHand.attackSpeed != 1f) lines.Add($"Attack Speed: {offHand.attackSpeed:F2}");
                if (offHand.criticalStrikeChance != 0f) lines.Add($"Critical Chance: {offHand.criticalStrikeChance:F1}%");
                break;
            case Jewellery jewellery:
                if (jewellery.life > 0f) lines.Add($"+{jewellery.life} to Maximum Life");
                if (jewellery.mana > 0f) lines.Add($"+{jewellery.mana} to Maximum Mana");
                if (jewellery.energyShield > 0f) lines.Add($"+{jewellery.energyShield} to Energy Shield");
                if (jewellery.ward > 0f) lines.Add($"+{jewellery.ward} to Ward");
                break;
            case Armour armour:
                // Penalties for armour
                if (armour.movementSpeedPenalty != 0f) lines.Add($"Movement Speed: {FormatSignedPercent(-armour.movementSpeedPenalty)}");
                if (armour.attackSpeedPenalty != 0f) lines.Add($"Attack Speed: {FormatSignedPercent(-armour.attackSpeedPenalty)}");
                break;
        }
        
        // Populate generic base stat labels
        for (int i = 0; i < baseStatLabels.Count; i++)
        {
            var label = baseStatLabels[i];
            if (label == null)
                continue;

            // Skip defence value labels - they're handled separately
            if (defenceValueLabels.Contains(label))
                continue;

            if (i < lines.Count)
            {
                label.text = lines[i];
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }
    }
    
    private void ApplyDefenceValues(Armour armour)
    {
        // Set pre-defined defense labels
        if (armourLabel != null)
        {
            if (armour.armour > 0f)
            {
                armourLabel.text = $"Armour: {armour.armour:F0}";
                armourLabel.gameObject.SetActive(true);
            }
            else
            {
                armourLabel.gameObject.SetActive(false);
            }
        }
        
        if (evasionLabel != null)
        {
            if (armour.evasion > 0f)
            {
                evasionLabel.text = $"Evasion: {armour.evasion:F0}";
                evasionLabel.gameObject.SetActive(true);
            }
            else
            {
                evasionLabel.gameObject.SetActive(false);
            }
        }
        
        if (energyShieldLabel != null)
        {
            if (armour.energyShield > 0f)
            {
                energyShieldLabel.text = $"Energy Shield: {armour.energyShield:F0}";
                energyShieldLabel.gameObject.SetActive(true);
            }
            else
            {
                energyShieldLabel.gameObject.SetActive(false);
            }
        }
        
        // Handle Ward in generic labels (not pre-defined)
        var lines = new List<string>();
        if (armour.ward > 0f) lines.Add($"Ward: {armour.ward:F0}");
        
        // Add penalties
        if (armour.movementSpeedPenalty != 0f) lines.Add($"Movement Speed: {FormatSignedPercent(-armour.movementSpeedPenalty)}");
        if (armour.attackSpeedPenalty != 0f) lines.Add($"Attack Speed: {FormatSignedPercent(-armour.attackSpeedPenalty)}");
        
        // Populate generic base stat labels with remaining stats
        for (int i = 0; i < baseStatLabels.Count; i++)
        {
            var label = baseStatLabels[i];
            if (label == null)
                continue;

            // Skip defence value labels - they're handled separately
            if (defenceValueLabels.Contains(label))
                continue;

            if (i < lines.Count)
            {
                label.text = lines[i];
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }
    }

    private void ApplyBaseStats(ItemData itemData)
    {
        // Reset all pre-defined labels
        ResetPredefinedLabels();
        
        // Set Item Type
        if (itemTypeLabel != null)
        {
            itemTypeLabel.text = itemData.equipmentType.ToString();
            itemTypeLabel.gameObject.SetActive(true);
        }
        
        // Handle item-specific stats
        switch (itemData.itemType)
        {
            case ItemType.Armour:
                if (armourLabel != null && itemData.baseArmour > 0f)
                {
                    armourLabel.text = $"Armour: {itemData.baseArmour:F0}";
                    armourLabel.gameObject.SetActive(true);
                }
                if (evasionLabel != null && itemData.baseEvasion > 0f)
                {
                    evasionLabel.text = $"Evasion: {itemData.baseEvasion:F0}";
                    evasionLabel.gameObject.SetActive(true);
                }
                if (energyShieldLabel != null && itemData.baseEnergyShield > 0f)
                {
                    energyShieldLabel.text = $"Energy Shield: {itemData.baseEnergyShield:F0}";
                    energyShieldLabel.gameObject.SetActive(true);
                }
                break;
        }
        
        // Set Quality if available
        // Note: ItemData doesn't have quality field, so this might not apply
        // But we'll leave it for consistency
        
        // Populate other stats in generic labels
        var lines = new List<string>();
        
        if (itemData.itemType == ItemType.Weapon)
        {
            // Check if source item has rolled damage
            bool hasRolledDamage = false;
            float rolledBaseDamage = 0f;
            
            if (itemData.sourceItem is WeaponItem weaponSource)
            {
                hasRolledDamage = weaponSource.rolledBaseDamage > 0f;
                rolledBaseDamage = weaponSource.rolledBaseDamage;
            }
            
            if (hasRolledDamage)
            {
                lines.Add($"Base Damage: {rolledBaseDamage:F0}");
                int rolledTotal = Mathf.RoundToInt(itemData.GetTotalMinDamage());
                lines.Add($"Total Damage: {rolledTotal}");
            }
            else
            {
                if (itemData.baseDamageMin > 0f || itemData.baseDamageMax > 0f)
                    lines.Add($"Base Damage: {itemData.baseDamageMin:F0}-{itemData.baseDamageMax:F0}");

                float totalMin = itemData.GetTotalMinDamage();
                float totalMax = itemData.GetTotalMaxDamage();
                if (totalMin > 0f || totalMax > 0f)
                    lines.Add($"Total Damage: {totalMin:F0}-{totalMax:F0}");
            }

            if (itemData.attackSpeed > 0f)
                lines.Add($"Attack Speed: {itemData.attackSpeed:F2}");

            if (itemData.criticalStrikeChance > 0f)
                lines.Add($"Critical Strike Chance: {itemData.criticalStrikeChance:F1}%");
        }
        
        if (itemData.stats != null && itemData.stats.Count > 0)
        {
            foreach (var kvp in itemData.stats)
            {
                lines.Add($"{kvp.Key}: {kvp.Value}");
            }
        }
        
        // Populate generic labels
        for (int i = 0; i < baseStatLabels.Count; i++)
        {
            var label = baseStatLabels[i];
            if (label == null)
                continue;

            if (i < lines.Count)
            {
                label.text = lines[i];
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }
    }

    private List<string> BuildBaseStatLines(BaseItem item)
    {
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.description))
        {
            lines.Add(item.description);
        }

        switch (item)
        {
            case Armour armour:
                lines.Add($"{armour.armourSlot} - {armour.armourType}");
                if (armour.armour > 0f) lines.Add($"Armour: {armour.armour}");
                if (armour.evasion > 0f) lines.Add($"Evasion: {armour.evasion}");
                if (armour.energyShield > 0f) lines.Add($"Energy Shield: {armour.energyShield}");
                if (armour.ward > 0f) lines.Add($"Ward: {armour.ward}");
                if (armour.movementSpeedPenalty != 0f) lines.Add($"Movement Speed: {FormatSignedPercent(-armour.movementSpeedPenalty)}");
                if (armour.attackSpeedPenalty != 0f) lines.Add($"Attack Speed: {FormatSignedPercent(-armour.attackSpeedPenalty)}");
                break;

            case OffHandEquipment offHand:
                lines.Add($"{offHand.offHandSlot} - {offHand.offHandType}");
                if (offHand.defence > 0f) lines.Add($"Defence: {offHand.defence}");
                if (offHand.blockChance > 0f) lines.Add($"Block Chance: {offHand.blockChance:F1}%");
                if (offHand.blockValue > 0f) lines.Add($"Block Value: {offHand.blockValue}");
                if (offHand.attackSpeed != 1f) lines.Add($"Attack Speed: {offHand.attackSpeed:F2}");
                if (offHand.criticalStrikeChance != 0f) lines.Add($"Critical Chance: {offHand.criticalStrikeChance:F1}%");
                break;

            case Jewellery jewellery:
                lines.Add($"{jewellery.jewellerySlot} - {jewellery.jewelleryType}");
                if (jewellery.life > 0f) lines.Add($"+{jewellery.life} to Maximum Life");
                if (jewellery.mana > 0f) lines.Add($"+{jewellery.mana} to Maximum Mana");
                if (jewellery.energyShield > 0f) lines.Add($"+{jewellery.energyShield} to Energy Shield");
                if (jewellery.ward > 0f) lines.Add($"+{jewellery.ward} to Ward");
                break;

            default:
                if (item.itemTags != null && item.itemTags.Count > 0)
                {
                    lines.Add($"Tags: {string.Join(", ", item.itemTags)}");
                }
                break;
        }

        if (item.quality > 0)
        {
            lines.Add($"Quality: +{item.quality}%");
        }

        return lines;
    }

    private List<string> BuildBaseStatLines(ItemData itemData)
    {
        var lines = new List<string>();

        lines.Add(itemData.equipmentType.ToString());

        switch (itemData.itemType)
        {
            case ItemType.Weapon:
                // Check if source item has rolled damage
                bool hasRolledDamage = false;
                float rolledBaseDamage = 0f;
                
                if (itemData.sourceItem is WeaponItem weaponSource)
                {
                    hasRolledDamage = weaponSource.rolledBaseDamage > 0f;
                    rolledBaseDamage = weaponSource.rolledBaseDamage;
                }
                
                if (hasRolledDamage)
                {
                    // Show rolled base damage (single value)
                    lines.Add($"Base Damage: {rolledBaseDamage:F0}");
                    
                    // Total damage (single value, affixes are rolled too)
                    int rolledTotal = Mathf.RoundToInt(itemData.GetTotalMinDamage());
                    lines.Add($"Total Damage: {rolledTotal}");
                }
                else
                {
                    // Fallback: Show range (for items without rolled damage)
                    if (itemData.baseDamageMin > 0f || itemData.baseDamageMax > 0f)
                        lines.Add($"Base Damage: {itemData.baseDamageMin:F0}-{itemData.baseDamageMax:F0}");

                    float totalMin = itemData.GetTotalMinDamage();
                    float totalMax = itemData.GetTotalMaxDamage();
                    if (totalMin > 0f || totalMax > 0f)
                        lines.Add($"Total Damage: {totalMin:F0}-{totalMax:F0}");
                }

                if (itemData.attackSpeed > 0f)
                    lines.Add($"Attack Speed: {itemData.attackSpeed:F2}");

                if (itemData.criticalStrikeChance > 0f)
                    lines.Add($"Critical Strike Chance: {itemData.criticalStrikeChance:F1}%");
                break;

            case ItemType.Armour:
                if (itemData.baseArmour > 0f) lines.Add($"Armour: {itemData.baseArmour:F0}");
                if (itemData.baseEvasion > 0f) lines.Add($"Evasion: {itemData.baseEvasion:F0}");
                if (itemData.baseEnergyShield > 0f) lines.Add($"Energy Shield: {itemData.baseEnergyShield:F0}");
                break;

            default:
                break;
        }

        if (itemData.stats != null && itemData.stats.Count > 0)
        {
            foreach (var kvp in itemData.stats)
            {
                lines.Add($"{kvp.Key}: {kvp.Value}");
            }
        }

        return lines;
    }

    private void ApplyRequirements(BaseItem item)
    {
        if (requirementsLabel == null)
            return;

        int requiredStrength = 0;
        int requiredDexterity = 0;
        int requiredIntelligence = 0;
        int requiredLevel = item.requiredLevel;

        switch (item)
        {
            case Armour armour:
                requiredStrength = armour.requiredStrength;
                requiredDexterity = armour.requiredDexterity;
                requiredIntelligence = armour.requiredIntelligence;
                // requiredLevel already set from item.requiredLevel above
                
                // Check implicit modifiers for requirements (some items have requirements in implicits)
                if (item.implicitModifiers != null)
                {
                    foreach (var implicitModifier in item.implicitModifiers)
                    {
                        if (implicitModifier != null && !string.IsNullOrEmpty(implicitModifier.description))
                        {
                            // Check if this is a requirements implicit (e.g., "Requires Level 66, 159 Dex")
                            string desc = implicitModifier.description.ToLower();
                            if (desc.Contains("requires level"))
                            {
                                // Try to extract level requirement (handles "Requires Level 66" or "Level 66")
                                var levelMatch = Regex.Match(desc, @"level\s+(\d+)", RegexOptions.IgnoreCase);
                                if (levelMatch.Success && int.TryParse(levelMatch.Groups[1].Value, out int level))
                                {
                                    requiredLevel = Mathf.Max(requiredLevel, level);
                                }
                                
                                // Try to extract attribute requirements (handles "159 Dex", "159 Dexterity", etc.)
                                var strMatch = Regex.Match(desc, @"(\d+)\s+(?:str|strength)", RegexOptions.IgnoreCase);
                                if (strMatch.Success && int.TryParse(strMatch.Groups[1].Value, out int str))
                                {
                                    requiredStrength = Mathf.Max(requiredStrength, str);
                                }
                                
                                var dexMatch = Regex.Match(desc, @"(\d+)\s+(?:dex|dexterity)", RegexOptions.IgnoreCase);
                                if (dexMatch.Success && int.TryParse(dexMatch.Groups[1].Value, out int dex))
                                {
                                    requiredDexterity = Mathf.Max(requiredDexterity, dex);
                                }
                                
                                var intMatch = Regex.Match(desc, @"(\d+)\s+(?:int|intelligence)", RegexOptions.IgnoreCase);
                                if (intMatch.Success && int.TryParse(intMatch.Groups[1].Value, out int intel))
                                {
                                    requiredIntelligence = Mathf.Max(requiredIntelligence, intel);
                                }
                            }
                        }
                    }
                }
                break;
            case OffHandEquipment offHand:
                requiredStrength = offHand.requiredStrength;
                requiredDexterity = offHand.requiredDexterity;
                requiredIntelligence = offHand.requiredIntelligence;
                requiredLevel = offHand.requiredLevel;
                break;
            case Jewellery jewellery:
                requiredStrength = jewellery.requiredStrength;
                requiredDexterity = jewellery.requiredDexterity;
                requiredIntelligence = jewellery.requiredIntelligence;
                requiredLevel = jewellery.requiredLevel;
                break;
        }

        string text = TooltipFormattingUtils.FormatRequirements(requiredLevel, requiredStrength, requiredDexterity, requiredIntelligence);
        bool hasRequirements = !string.IsNullOrWhiteSpace(text);
        requirementsLabel.text = hasRequirements ? $"Requirements:\n{text}" : string.Empty;
        requirementsLabel.gameObject.SetActive(hasRequirements);
    }

    private void ApplyRequirements(ItemData itemData)
    {
        if (requirementsLabel == null)
            return;

        string text = TooltipFormattingUtils.FormatRequirements(
            itemData.requiredLevel,
            itemData.requiredStrength,
            itemData.requiredDexterity,
            itemData.requiredIntelligence);

        bool hasRequirements = !string.IsNullOrWhiteSpace(text);
        requirementsLabel.text = hasRequirements ? $"Requirements:\n{text}" : string.Empty;
        requirementsLabel.gameObject.SetActive(hasRequirements);
    }

    private void ApplyAffixes(TextMeshProUGUI target, List<Affix> affixes, bool showRanges = false)
    {
        if (target == null)
            return;

        // Filter out requirement implicits - they should be shown in Requirements section instead
        List<Affix> filteredAffixes = new List<Affix>();
        if (affixes != null)
        {
            foreach (var affix in affixes)
            {
                if (affix != null && !string.IsNullOrEmpty(affix.description))
                {
                    string desc = affix.description.ToLower();
                    // Skip requirement implicits (they're handled in Requirements section)
                    if (desc.Contains("requires level") || desc.Contains("requires ") && 
                        (desc.Contains("str") || desc.Contains("dex") || desc.Contains("int")))
                    {
                        continue; // Skip this implicit - it's shown in Requirements
                    }
                }
                filteredAffixes.Add(affix);
            }
        }

        string text = BuildAffixBlock(filteredAffixes, showRanges, isImplicit: true);
        bool hasAffix = !string.IsNullOrEmpty(text);
        
        if (hasAffix)
        {
            target.text = text;
            target.color = Color.white; // Base color, text has inline color tags
            target.gameObject.SetActive(true);
        }
        else
        {
            // Hide instead of showing "None"
            target.gameObject.SetActive(false);
        }
    }

    private void ApplyAffixes(TextMeshProUGUI target, List<string> affixLines, bool showRanges = false)
    {
        if (target == null)
            return;

        if (affixLines == null || affixLines.Count == 0)
        {
            // Hide instead of showing "None"
            target.gameObject.SetActive(false);
            return;
        }
        
        // Note: String affixes don't have rolling info, display as-is
        // This is for ItemData compatibility where affixes are already strings

        target.text = string.Join("\n", affixLines);
        target.color = Color.white; // Base color
        target.gameObject.SetActive(true);
    }

    private string BuildAffixBlock(List<Affix> affixes, bool showRanges = false, bool isImplicit = false)
    {
        if (affixes == null || affixes.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        for (int i = 0; i < affixes.Count; i++)
        {
            string formatted = isImplicit
                ? TooltipFormattingUtils.FormatImplicit(affixes[i], showRanges)
                : TooltipFormattingUtils.FormatAffix(affixes[i], showRanges);
                
            if (string.IsNullOrEmpty(formatted))
                continue;

            if (builder.Length > 0)
                builder.Append('\n');

            builder.Append(formatted);
        }

        return builder.ToString();
    }

    private void SetAffixTexts(IEnumerable<TextMeshProUGUI> targets, IList<Affix> affixes, bool showRanges = false)
    {
        if (targets == null)
            return;

        int index = 0;
        foreach (var label in targets)
        {
            if (label == null)
                continue;

            if (affixes != null && index < affixes.Count)
            {
                string text = TooltipFormattingUtils.FormatAffix(affixes[index], showRanges);
                label.text = string.IsNullOrEmpty(text) ? "—" : text;
                label.color = Color.white; // Base color, text has inline color tags
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }

            index++;
        }
    }

    private void SetAffixTexts(IEnumerable<TextMeshProUGUI> targets, IList<string> affixLines)
    {
        if (targets == null)
            return;

        int index = 0;
        foreach (var label in targets)
        {
            if (label == null)
                continue;

            if (affixLines != null && index < affixLines.Count && !string.IsNullOrWhiteSpace(affixLines[index]))
            {
                label.text = affixLines[index];
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }

            index++;
        }
    }

    private void CacheUIElements()
    {
        if (cached)
            return;

        cached = true;

        nameLabel ??= FindLabel("Header/EquipmentName");
        iconImage ??= FindImage("Content/IconContainer/WeaponIcon");
        iconBackground ??= FindImage("Content/IconContainer/IconBackground");
        iconFrame ??= FindImage("Content/IconContainer/ImageFrame");

        var statsContainer = transform.Find("Content/BaseItemStats");
        if (statsContainer == null)
        {
            // Fallback to BaseWeaponStats for backwards compatibility
            statsContainer = transform.Find("Content/BaseWeaponStats");
        }
        
        // Find pre-defined stat labels
        qualityLabel ??= FindLabel("Content/BaseItemStats/QualityLabel");
        if (qualityLabel == null)
            qualityLabel = FindLabel("Content/BaseWeaponStats/QualityLabel");
            
        armourLabel ??= FindLabel("Content/BaseItemStats/ArmourLabel");
        if (armourLabel == null)
            armourLabel = FindLabel("Content/BaseWeaponStats/ArmourLabel");
            
        energyShieldLabel ??= FindLabel("Content/BaseItemStats/EnergyShieldLabel");
        if (energyShieldLabel == null)
            energyShieldLabel = FindLabel("Content/BaseWeaponStats/EnergyShieldLabel");
            
        evasionLabel ??= FindLabel("Content/BaseItemStats/EvasionLabel");
        if (evasionLabel == null)
            evasionLabel = FindLabel("Content/BaseWeaponStats/EvasionLabel");
            
        itemTypeLabel ??= FindLabel("Content/BaseItemStats/ItemTypeLabel");
        if (itemTypeLabel == null)
            itemTypeLabel = FindLabel("Content/BaseWeaponStats/ItemTypeLabel");
        
        baseStatLabels ??= new List<TextMeshProUGUI>();
        baseStatLabels.Clear();
        defenceValueLabels.Clear();

        if (statsContainer != null)
        {
            foreach (Transform child in statsContainer)
            {
                var label = child.GetComponent<TextMeshProUGUI>();
                if (label == null)
                    continue;

                string childName = child.name;
                
                // Skip pre-defined labels - they're handled separately
                if (childName == "QualityLabel" || childName == "ArmourLabel" || 
                    childName == "EnergyShieldLabel" || childName == "EvasionLabel" || 
                    childName == "ItemTypeLabel")
                {
                    continue;
                }
                
                if (childName == "Requirements")
                {
                    requirementsLabel ??= label;
                }
                else if (childName.StartsWith("DefenceValue", System.StringComparison.OrdinalIgnoreCase))
                {
                    // This is a defence value label - handle separately
                    defenceValueLabels.Add(label);
                    // Also add to baseStatLabels but we'll skip it in ApplyBaseStats for armour
                    baseStatLabels.Add(label);
                }
                else
                {
                    baseStatLabels.Add(label);
                }
            }
        }

        implicitLabel ??= FindLabel("Content/AffixContainer/Implicit/ImplicitModifider");

        if (prefixLabels == null || prefixLabels.Count == 0)
        {
            prefixLabels = CollectChildLabels("Content/AffixContainer/Prefixes");
        }

        if (suffixLabels == null || suffixLabels.Count == 0)
        {
            suffixLabels = CollectChildLabels("Content/AffixContainer/Suffixes");
        }
    }

    private void EnsureUIReferencesCached()
    {
        if (!cached)
        {
            CacheUIElements();
        }
    }

    private TextMeshProUGUI FindLabel(string path)
    {
        Transform target = transform.Find(path);
        return target ? target.GetComponent<TextMeshProUGUI>() : null;
    }

    private Image FindImage(string path)
    {
        Transform target = transform.Find(path);
        return target ? target.GetComponent<Image>() : null;
    }

    private List<TextMeshProUGUI> CollectChildLabels(string path)
    {
        var list = new List<TextMeshProUGUI>();
        Transform parent = transform.Find(path);
        if (parent == null)
            return list;

        foreach (Transform child in parent)
        {
            var label = child.GetComponent<TextMeshProUGUI>();
            if (label != null)
                list.Add(label);
        }

        return list;
    }

    private string FormatSignedPercent(float value)
    {
        return value.ToString("+0;-0", CultureInfo.InvariantCulture) + "%";
    }
    
    /// <summary>
    /// Check if ALT key is held (supports both old and new Input System)
    /// </summary>
    private bool IsAltKeyHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            bool isPressed = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
            // Debug: Log when state changes
            if (gameObject.activeSelf && (currentItem != null || currentItemData != null))
            {
                if (isPressed != lastAltState)
                {
                    Debug.Log($"<color=cyan>[EquipmentTooltip] Input System ALT detected: {isPressed}</color>");
                }
            }
            return isPressed;
        }
        Debug.LogWarning("[EquipmentTooltip] Keyboard.current is null!");
        return false;
#else
        return UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt);
#endif
    }
}


