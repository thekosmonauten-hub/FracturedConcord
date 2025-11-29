using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    
    // Specific defense value labels (if they exist in prefab)
    private List<TextMeshProUGUI> defenceValueLabels = new List<TextMeshProUGUI>();

    [Header("Affixes")]
    [SerializeField] private TextMeshProUGUI implicitLabel;
    [SerializeField] private List<TextMeshProUGUI> prefixLabels = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> suffixLabels = new List<TextMeshProUGUI>();

    private bool cached;

    private void Awake()
    {
        CacheUIElements();
    }

    public void SetData(BaseItem item)
    {
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);

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
        ApplyAffixes(implicitLabel, item.implicitModifiers);
        SetAffixTexts(prefixLabels, item.prefixes);
        SetAffixTexts(suffixLabels, item.suffixes);
    }

    public void SetData(ItemData itemData)
    {
        if (itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (itemData.sourceItem != null)
        {
            SetData(itemData.sourceItem);
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);

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
        // Handle Armour items specifically for defense values
        if (item is Armour armour)
        {
            ApplyDefenceValues(armour);
        }
        else
        {
            // For non-armour items, use the generic approach
            var lines = BuildBaseStatLines(item);

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
    }
    
    private void ApplyDefenceValues(Armour armour)
    {
        // Build list of defense values that actually exist
        List<string> defenceValues = new List<string>();
        
        if (armour.armour > 0f) defenceValues.Add($"{armour.armour:F0} Armour");
        if (armour.evasion > 0f) defenceValues.Add($"{armour.evasion:F0} Evasion");
        if (armour.energyShield > 0f) defenceValues.Add($"{armour.energyShield:F0} Energy Shield");
        if (armour.ward > 0f) defenceValues.Add($"{armour.ward:F0} Ward");
        
        // Populate defence value labels
        for (int i = 0; i < defenceValueLabels.Count; i++)
        {
            var label = defenceValueLabels[i];
            if (label == null)
                continue;
                
            if (i < defenceValues.Count)
            {
                label.text = defenceValues[i];
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }
        
        // Handle other base stat labels (description, slot/type, penalties, quality)
        var lines = BuildBaseStatLines(armour);
        // Remove defense values from lines since we handled them separately
        lines.RemoveAll(line => line.Contains("Armour:") || line.Contains("Evasion:") || 
                                line.Contains("Energy Shield:") || line.Contains("Ward:"));
        
        int lineIndex = 0;
        foreach (var label in baseStatLabels)
        {
            if (label == null)
                continue;
                
            // Skip defence value labels - already handled
            if (defenceValueLabels.Contains(label))
                continue;
                
            if (lineIndex < lines.Count)
            {
                label.text = lines[lineIndex];
                label.gameObject.SetActive(true);
                lineIndex++;
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }
    }

    private void ApplyBaseStats(ItemData itemData)
    {
        var lines = BuildBaseStatLines(itemData);

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
                if (itemData.baseDamageMin > 0f || itemData.baseDamageMax > 0f)
                    lines.Add($"Base Damage: {itemData.baseDamageMin:F0}-{itemData.baseDamageMax:F0}");

                float totalMin = itemData.GetTotalMinDamage();
                float totalMax = itemData.GetTotalMaxDamage();
                if (totalMin > 0f || totalMax > 0f)
                    lines.Add($"Total Damage: {totalMin:F0}-{totalMax:F0}");

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

    private void ApplyAffixes(TextMeshProUGUI target, List<Affix> affixes)
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

        string text = BuildAffixBlock(filteredAffixes);
        bool hasAffix = !string.IsNullOrEmpty(text);
        target.text = hasAffix ? text : "None";
        target.gameObject.SetActive(true);
    }

    private void ApplyAffixes(TextMeshProUGUI target, List<string> affixLines)
    {
        if (target == null)
            return;

        if (affixLines == null || affixLines.Count == 0)
        {
            target.text = "None";
            target.gameObject.SetActive(true);
            return;
        }

        target.text = string.Join("\n", affixLines);
        target.gameObject.SetActive(true);
    }

    private string BuildAffixBlock(List<Affix> affixes)
    {
        if (affixes == null || affixes.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        for (int i = 0; i < affixes.Count; i++)
        {
            string formatted = TooltipFormattingUtils.FormatAffix(affixes[i]);
            if (string.IsNullOrEmpty(formatted))
                continue;

            if (builder.Length > 0)
                builder.Append('\n');

            builder.Append(formatted);
        }

        return builder.ToString();
    }

    private void SetAffixTexts(IEnumerable<TextMeshProUGUI> targets, IList<Affix> affixes)
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
                string text = TooltipFormattingUtils.FormatAffix(affixes[index]);
                label.text = string.IsNullOrEmpty(text) ? "—" : text;
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
}


