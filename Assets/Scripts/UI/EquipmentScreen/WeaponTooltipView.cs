using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponTooltipView : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBackground;
    [SerializeField] private Image iconFrame;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI damageLabel;
    [SerializeField] private TextMeshProUGUI attackSpeedLabel;
    [SerializeField] private TextMeshProUGUI critChanceLabel;
    [SerializeField] private TextMeshProUGUI weaponTypeLabel;
    [SerializeField] private TextMeshProUGUI requirementsLabel;

    [Header("Affixes")]
    [SerializeField] private TextMeshProUGUI implicitLabel;
    [SerializeField] private List<TextMeshProUGUI> prefixLabels = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> suffixLabels = new List<TextMeshProUGUI>();

    private bool cached;

    private void Awake()
    {
        CacheUIElements();
    }

    public void SetData(WeaponItem weapon)
    {
        if (weapon == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);

        ItemRarity rarity = weapon.GetCalculatedRarity();
        string displayName = weapon.GetDisplayName();

        if (nameLabel != null)
        {
            nameLabel.text = TooltipFormattingUtils.ColorizeByRarity(displayName, rarity);
        }

        if (iconImage != null)
        {
            iconImage.sprite = weapon.itemIcon;
            iconImage.enabled = weapon.itemIcon != null;
        }

        if (iconFrame != null)
        {
            if (ColorUtility.TryParseHtmlString(ItemRarityCalculator.GetRarityColor(rarity), out Color rarityColor))
            {
                iconFrame.color = rarityColor;
            }
        }

        if (damageLabel != null)
        {
            float baseMin = weapon.minDamage;
            float baseMax = weapon.maxDamage;
            float totalMin = weapon.GetTotalMinDamage();
            float totalMax = weapon.GetTotalMaxDamage();
            damageLabel.text = $"Damage: {baseMin:F0}-{baseMax:F0}  (Total {totalMin:F0}-{totalMax:F0})";
        }

        if (attackSpeedLabel != null)
        {
            float totalSpeed = weapon.GetTotalAttackSpeed();
            attackSpeedLabel.text = $"Attack Speed: {weapon.attackSpeed:F2} aps  (Total {totalSpeed:F2})";
        }

        if (critChanceLabel != null)
        {
            float totalCrit = weapon.GetTotalCriticalStrikeChance();
            critChanceLabel.text = $"Critical Chance: {weapon.criticalStrikeChance:F1}%  (Total {totalCrit:F1}%)";
        }

        if (weaponTypeLabel != null)
        {
            weaponTypeLabel.text = $"{weapon.handedness} {weapon.weaponType}";
        }

        if (requirementsLabel != null)
        {
            string requirements = TooltipFormattingUtils.FormatRequirements(
                weapon.requiredLevel,
                weapon.requiredStrength,
                weapon.requiredDexterity,
                weapon.requiredIntelligence);

            requirementsLabel.text = $"Requirements:\n{requirements}";
            requirementsLabel.gameObject.SetActive(true);
        }

        if (implicitLabel != null)
        {
            string text = BuildAffixBlock(weapon.implicitModifiers);
            bool hasImplicit = !string.IsNullOrEmpty(text);
            implicitLabel.text = hasImplicit ? text : "None";
            implicitLabel.gameObject.SetActive(true);
        }

        SetAffixTexts(prefixLabels, weapon.prefixes);
        SetAffixTexts(suffixLabels, weapon.suffixes);
    }

    public void SetData(ItemData itemData)
    {
        if (itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (itemData.sourceItem is WeaponItem weapon)
        {
            SetData(weapon);
            return;
        }

        if (itemData.itemType != ItemType.Weapon)
        {
            var equipmentView = GetComponent<EquipmentTooltipView>();
            if (equipmentView != null)
            {
                equipmentView.SetData(itemData);
            }
            else
            {
                gameObject.SetActive(false);
            }
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);

        ItemRarity rarity = itemData.rarity;
        string displayName = string.IsNullOrWhiteSpace(itemData.itemName) ? "Unnamed Weapon" : itemData.itemName;

        if (nameLabel != null)
        {
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

        if (damageLabel != null)
        {
            float baseMin = itemData.baseDamageMin;
            float baseMax = itemData.baseDamageMax;
            float totalMin = itemData.GetTotalMinDamage();
            float totalMax = itemData.GetTotalMaxDamage();
            if (baseMin > 0f || baseMax > 0f || totalMin > 0f || totalMax > 0f)
            {
                damageLabel.text = $"Damage: {baseMin:F0}-{baseMax:F0}  (Total {totalMin:F0}-{totalMax:F0})";
                damageLabel.gameObject.SetActive(true);
            }
            else
            {
                damageLabel.gameObject.SetActive(false);
            }
        }

        if (attackSpeedLabel != null)
        {
            if (itemData.attackSpeed > 0f)
            {
                attackSpeedLabel.text = $"Attack Speed: {itemData.attackSpeed:F2} aps";
                attackSpeedLabel.gameObject.SetActive(true);
            }
            else
            {
                attackSpeedLabel.gameObject.SetActive(false);
            }
        }

        if (critChanceLabel != null)
        {
            if (itemData.criticalStrikeChance > 0f)
            {
                critChanceLabel.text = $"Critical Chance: {itemData.criticalStrikeChance:F1}%";
                critChanceLabel.gameObject.SetActive(true);
            }
            else
            {
                critChanceLabel.gameObject.SetActive(false);
            }
        }

        if (weaponTypeLabel != null)
        {
            weaponTypeLabel.text = itemData.equipmentType.ToString();
            weaponTypeLabel.gameObject.SetActive(true);
        }

        if (requirementsLabel != null)
        {
            string requirements = TooltipFormattingUtils.FormatRequirements(
                itemData.requiredLevel,
                itemData.requiredStrength,
                itemData.requiredDexterity,
                itemData.requiredIntelligence);

            bool hasRequirements = !string.IsNullOrWhiteSpace(requirements);
            requirementsLabel.text = hasRequirements ? $"Requirements:\n{requirements}" : string.Empty;
            requirementsLabel.gameObject.SetActive(hasRequirements);
        }

        ApplyAffixStrings(implicitLabel, itemData.implicitModifiers);
        SetAffixTexts(prefixLabels, itemData.prefixModifiers);
        SetAffixTexts(suffixLabels, itemData.suffixModifiers);
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

    private void ApplyAffixStrings(TextMeshProUGUI target, List<string> affixLines)
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

    private void CacheUIElements()
    {
        if (cached)
            return;

        cached = true;

        nameLabel ??= FindLabel("Header/WeaponName");
        iconImage ??= FindImage("Content/IconContainer/WeaponIcon");
        iconBackground ??= FindImage("Content/IconContainer/IconBackground");
        iconFrame ??= FindImage("Content/IconContainer/ImageFrame");

        var statsContainer = transform.Find("Content/BaseWeaponStats");
        if (statsContainer != null)
        {
            foreach (Transform child in statsContainer)
            {
                var label = child.GetComponent<TextMeshProUGUI>();
                if (label == null)
                    continue;

                switch (child.name)
                {
                    case "AttackDamage":
                        damageLabel ??= label;
                        break;
                    case "AttackSpeed":
                        attackSpeedLabel ??= label;
                        break;
                    case "CritChance":
                        critChanceLabel ??= label;
                        break;
                    case "WeaponType":
                        weaponTypeLabel ??= label;
                        break;
                    case "Requirements":
                        requirementsLabel ??= label;
                        break;
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
}


