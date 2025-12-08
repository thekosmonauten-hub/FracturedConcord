using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
    private WeaponItem currentWeapon;
    private ItemData currentItemData;
    private bool lastAltState = false;

    private void Awake()
    {
        CacheUIElements();
    }
    
    private void Update()
    {
        // Check if ALT key state changed while tooltip is active
        if (gameObject.activeSelf && (currentWeapon != null || currentItemData != null))
        {
            bool currentAltState = IsAltKeyHeld();
            
            if (currentAltState != lastAltState)
            {
                lastAltState = currentAltState;
                
                Debug.Log($"<color=yellow>[WeaponTooltip] ALT state changed: {currentAltState} (showRanges: {currentAltState})</color>");
                
                // Refresh tooltip with new ALT state
                if (currentWeapon != null)
                {
                    SetData(currentWeapon);
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

    public void SetData(WeaponItem weapon)
    {
        if (weapon == null)
        {
            gameObject.SetActive(false);
            currentWeapon = null;
            currentItemData = null;
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);
        
        // Cache current weapon for ALT-key updates
        currentWeapon = weapon;
        currentItemData = null;
        
        // Check if ALT key is held (show ranges instead of rolled values)
        bool showRanges = IsAltKeyHeld();
        lastAltState = showRanges;

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
            if (showRanges)
            {
                // ALT held: Show breakdown with base and total (separate lines)
                if (weapon.rolledBaseDamage > 0f)
                {
                    int rolledBase = Mathf.RoundToInt(weapon.rolledBaseDamage);
                    int rolledTotal = Mathf.RoundToInt(weapon.GetTotalMinDamage());
                    damageLabel.text = $"Dmg: {rolledBase} base\n     ({rolledTotal} total)";
                }
                else
                {
                    // No rolled value, show range
                    float baseMin = weapon.minDamage;
                    float baseMax = weapon.maxDamage;
                    float totalMin = weapon.GetTotalMinDamage();
                    float totalMax = weapon.GetTotalMaxDamage();
                    damageLabel.text = $"Dmg: {baseMin:F0}-{baseMax:F0} base\n     ({totalMin:F0}-{totalMax:F0} total)";
                }
            }
            else
            {
                // Normal: Only show total
                int rolledTotal = Mathf.RoundToInt(weapon.GetTotalMinDamage());
                damageLabel.text = $"Dmg: {rolledTotal}";
            }
        }

        if (attackSpeedLabel != null)
        {
            float totalSpeed = weapon.GetTotalAttackSpeed();
            
            if (showRanges)
            {
                // ALT held: Show base and total (separate lines)
                attackSpeedLabel.text = $"AS: {weapon.attackSpeed:F2} base\n    ({totalSpeed:F2} total)";
            }
            else
            {
                // Normal: Only show total
                attackSpeedLabel.text = $"AS: {totalSpeed:F2}";
            }
        }

        if (critChanceLabel != null)
        {
            float totalCrit = weapon.GetTotalCriticalStrikeChance();
            
            if (showRanges)
            {
                // ALT held: Show base and total (separate lines)
                critChanceLabel.text = $"Crit: {weapon.criticalStrikeChance:F1}% base\n      ({totalCrit:F1}% total)";
            }
            else
            {
                // Normal: Only show total
                critChanceLabel.text = $"Crit: {totalCrit:F1}%";
            }
        }

        if (weaponTypeLabel != null)
        {
            // Format handedness and weapon type as compact notation
            string handedness = weapon.handedness == WeaponHandedness.OneHanded ? "1H" : "2H";
            string weaponType = FormatWeaponType(weapon.weaponType);
            weaponTypeLabel.text = $"{handedness}-{weaponType}";
        }

        if (requirementsLabel != null)
        {
            string requirements = TooltipFormattingUtils.FormatRequirementsCompact(
                weapon.requiredLevel,
                weapon.requiredStrength,
                weapon.requiredDexterity,
                weapon.requiredIntelligence);

            requirementsLabel.text = requirements;
            requirementsLabel.gameObject.SetActive(true);
        }

        if (implicitLabel != null)
        {
            string text = BuildAffixBlock(weapon.implicitModifiers, showRanges, isImplicit: true);
            bool hasImplicit = !string.IsNullOrEmpty(text);
            
            if (hasImplicit)
            {
                implicitLabel.text = text;
                implicitLabel.color = Color.white; // Base color, text has inline color tags
                implicitLabel.gameObject.SetActive(true);
            }
            else
            {
                // Hide label instead of showing "None"
                implicitLabel.gameObject.SetActive(false);
            }
        }

        SetAffixTexts(prefixLabels, weapon.prefixes, showRanges);
        SetAffixTexts(suffixLabels, weapon.suffixes, showRanges);
    }

    public void SetData(ItemData itemData)
    {
        if (itemData == null)
        {
            gameObject.SetActive(false);
            currentWeapon = null;
            currentItemData = null;
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
            currentWeapon = null;
            currentItemData = null;
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);
        
        // Cache current item data for ALT-key updates
        currentItemData = itemData;
        currentWeapon = null;
        
        // Check if ALT key is held
        bool showRanges = IsAltKeyHeld();
        lastAltState = showRanges;

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
            // Check if source item has rolled damage
            bool hasRolledDamage = false;
            float rolledBaseDamage = 0f;
            
            if (itemData.sourceItem is WeaponItem weaponSource)
            {
                hasRolledDamage = weaponSource.rolledBaseDamage > 0f;
                rolledBaseDamage = weaponSource.rolledBaseDamage;
            }
            
            float baseMin = itemData.baseDamageMin;
            float baseMax = itemData.baseDamageMax;
            float totalMin = itemData.GetTotalMinDamage();
            float totalMax = itemData.GetTotalMaxDamage();
            
            if (baseMin > 0f || baseMax > 0f || totalMin > 0f || totalMax > 0f)
            {
                if (showRanges)
                {
                    // ALT held: Show breakdown (separate lines)
                    if (hasRolledDamage)
                    {
                        int rolledBase = Mathf.RoundToInt(rolledBaseDamage);
                        int rolledTotal = Mathf.RoundToInt(totalMin);
                        damageLabel.text = $"Dmg: {rolledBase} base\n     ({rolledTotal} total)";
                    }
                    else
                    {
                        damageLabel.text = $"Dmg: {baseMin:F0}-{baseMax:F0} base\n     ({totalMin:F0}-{totalMax:F0} total)";
                    }
                }
                else
                {
                    // Normal: Only show total
                    int rolledTotal = Mathf.RoundToInt(totalMin);
                    damageLabel.text = $"Dmg: {rolledTotal}";
                }
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
                if (showRanges)
                {
                    // ALT held: Show base (separate line)
                    attackSpeedLabel.text = $"AS: {itemData.attackSpeed:F2} base";
                }
                else
                {
                    // Normal: Only show value
                    attackSpeedLabel.text = $"AS: {itemData.attackSpeed:F2}";
                }
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
                if (showRanges)
                {
                    // ALT held: Show base (separate line)
                    critChanceLabel.text = $"Crit: {itemData.criticalStrikeChance:F1}% base";
                }
                else
                {
                    // Normal: Only show value
                    critChanceLabel.text = $"Crit: {itemData.criticalStrikeChance:F1}%";
                }
                critChanceLabel.gameObject.SetActive(true);
            }
            else
            {
                critChanceLabel.gameObject.SetActive(false);
            }
        }

        if (weaponTypeLabel != null)
        {
            // Format as compact notation (e.g., "1H-Axe", "2H-Sword")
            string type = itemData.equipmentType.ToString();
            weaponTypeLabel.text = type;
            weaponTypeLabel.gameObject.SetActive(true);
        }

        if (requirementsLabel != null)
        {
            string requirements = TooltipFormattingUtils.FormatRequirementsCompact(
                itemData.requiredLevel,
                itemData.requiredStrength,
                itemData.requiredDexterity,
                itemData.requiredIntelligence);

            requirementsLabel.text = requirements;
            requirementsLabel.gameObject.SetActive(true);
        }

        ApplyAffixStrings(implicitLabel, itemData.implicitModifiers);
        SetAffixTexts(prefixLabels, itemData.prefixModifiers);
        SetAffixTexts(suffixLabels, itemData.suffixModifiers);
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
            // Debug: Log every frame when tooltip is active (remove this later)
            if (gameObject.activeSelf && (currentWeapon != null || currentItemData != null))
            {
                // Only log when state changes to avoid spam
                if (isPressed != lastAltState)
                {
                    Debug.Log($"<color=cyan>[WeaponTooltip] Input System ALT detected: {isPressed}</color>");
                }
            }
            return isPressed;
        }
        Debug.LogWarning("[WeaponTooltip] Keyboard.current is null!");
        return false;
#else
        return UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt);
#endif
    }
    
    /// <summary>
    /// Format weapon type to compact notation (e.g., Axe, Sword, Bow)
    /// </summary>
    private string FormatWeaponType(WeaponItemType weaponType)
    {
        return weaponType.ToString(); // Already short: Axe, Sword, Bow, etc.
    }
}


