using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Dexiled.Data.Items;

/// <summary>
/// Component for item selection buttons in the Forge crafting UI.
/// Displays item icon, name, equipment type, defense/damage values, and requirements.
/// </summary>
public class ForgeItemSelectionButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI equipmentTypeText;
    [SerializeField] private TextMeshProUGUI statsText; // Defense value for armour, damage for weapons
    [SerializeField] private TextMeshProUGUI requirementsText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI implicitText; // Implicit modifiers
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image selectedHighlight;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.5f, 0.7f, 0.5f, 1f);
    [SerializeField] private Color unselectableColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    
    private BaseItem itemData;
    private Button button;
    
    public System.Action<BaseItem> OnItemClicked;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnItemClicked?.Invoke(itemData));
        }
    }
    
    /// <summary>
    /// Initialize the button with item data
    /// </summary>
    public void SetItemData(BaseItem item, bool isSelected = false)
    {
        itemData = item;
        
        if (item == null)
        {
            Debug.LogWarning("[ForgeItemSelectionButton] Cannot set null item data.");
            return;
        }
        
        UpdateDisplay(isSelected);
    }
    
    /// <summary>
    /// Update the visual display based on item data
    /// </summary>
    private void UpdateDisplay(bool isSelected)
    {
        // Item Icon
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.itemIcon;
            itemIcon.enabled = itemData.itemIcon != null;
        }
        
        // Item Name
        if (itemNameText != null)
        {
            itemNameText.text = itemData.GetDisplayName();
            itemNameText.color = GetRarityColor(itemData.rarity);
        }
        
        // Equipment Type
        if (equipmentTypeText != null)
        {
            equipmentTypeText.text = GetEquipmentTypeString(itemData);
        }
        
        // Stats (Defense for armour, Damage for weapons)
        if (statsText != null)
        {
            statsText.text = GetStatsString(itemData);
        }
        
        // Requirements
        if (requirementsText != null)
        {
            requirementsText.text = GetRequirementsString(itemData);
        }
        
        // Level Requirement
        if (levelText != null)
        {
            levelText.text = $"Lv. {itemData.requiredLevel}";
        }
        
        // Implicit Modifiers
        if (implicitText != null)
        {
            implicitText.text = GetImplicitModifiersString(itemData);
        }
        
        // Visual State
        SetSelected(isSelected);
    }
    
    /// <summary>
    /// Set the selected state of this button
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
        
        if (selectedHighlight != null)
        {
            selectedHighlight.enabled = isSelected;
        }
        
        if (button != null)
        {
            button.interactable = true;
        }
    }
    
    /// <summary>
    /// Get equipment type as display string
    /// </summary>
    private string GetEquipmentTypeString(BaseItem item)
    {
        if (item is Armour armour)
        {
            return armour.armourSlot.ToString();
        }
        else if (item is WeaponItem weapon)
        {
            return weapon.weaponType.ToString();
        }
        else if (item is Jewellery jewellery)
        {
            return jewellery.jewelleryType.ToString();
        }
        
        return item.equipmentType.ToString();
    }
    
    /// <summary>
    /// Get stats string (defense for armour, damage for weapons)
    /// </summary>
    private string GetStatsString(BaseItem item)
    {
        if (item is Armour armour)
        {
            List<string> stats = new List<string>();
            if (armour.armour > 0) stats.Add($"Armour: {armour.armour:F0}");
            if (armour.evasion > 0) stats.Add($"Evasion: {armour.evasion:F0}");
            if (armour.energyShield > 0) stats.Add($"ES: {armour.energyShield:F0}");
            if (armour.ward > 0) stats.Add($"Ward: {armour.ward:F0}");
            
            return stats.Count > 0 ? string.Join(" | ", stats) : "";
        }
        else if (item is WeaponItem weapon)
        {
            int avgDamage = Mathf.CeilToInt((weapon.minDamage + weapon.maxDamage) / 2f);
            return $"Damage: {avgDamage}";
        }
        
        return "";
    }
    
    /// <summary>
    /// Get requirements string (attribute requirements)
    /// </summary>
    private string GetRequirementsString(BaseItem item)
    {
        List<string> reqs = new List<string>();
        
        if (item is Armour armour)
        {
            if (armour.requiredStrength > 0) reqs.Add($"Str: {armour.requiredStrength}");
            if (armour.requiredDexterity > 0) reqs.Add($"Dex: {armour.requiredDexterity}");
            if (armour.requiredIntelligence > 0) reqs.Add($"Int: {armour.requiredIntelligence}");
        }
        else if (item is WeaponItem weapon)
        {
            if (weapon.requiredStrength > 0) reqs.Add($"Str: {weapon.requiredStrength}");
            if (weapon.requiredDexterity > 0) reqs.Add($"Dex: {weapon.requiredDexterity}");
            if (weapon.requiredIntelligence > 0) reqs.Add($"Int: {weapon.requiredIntelligence}");
        }
        else if (item is Jewellery jewellery)
        {
            if (jewellery.requiredStrength > 0) reqs.Add($"Str: {jewellery.requiredStrength}");
            if (jewellery.requiredDexterity > 0) reqs.Add($"Dex: {jewellery.requiredDexterity}");
            if (jewellery.requiredIntelligence > 0) reqs.Add($"Int: {jewellery.requiredIntelligence}");
        }
        
        return reqs.Count > 0 ? string.Join(" / ", reqs) : "No Req.";
    }
    
    /// <summary>
    /// Get implicit modifiers string
    /// </summary>
    private string GetImplicitModifiersString(BaseItem item)
    {
        if (item == null || item.implicitModifiers == null || item.implicitModifiers.Count == 0)
        {
            return "";
        }
        
        List<string> implicitStrings = new List<string>();
        foreach (var affix in item.implicitModifiers)
        {
            if (affix != null && !string.IsNullOrEmpty(affix.description))
            {
                implicitStrings.Add(affix.description);
            }
        }
        
        // Join with separator, or show first one if space is limited
        if (implicitStrings.Count == 0)
        {
            return "";
        }
        else if (implicitStrings.Count == 1)
        {
            return implicitStrings[0];
        }
        else
        {
            // For multiple implicits, join with " | " separator
            return string.Join(" | ", implicitStrings);
        }
    }
    
    /// <summary>
    /// Get rarity color
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal:
                return Color.white;
            case ItemRarity.Magic:
                return new Color(0.4f, 0.6f, 1f); // Blue
            case ItemRarity.Rare:
                return new Color(1f, 0.84f, 0f); // Yellow/Gold
            case ItemRarity.Unique:
                return new Color(0.7f, 0.4f, 0.9f); // Purple
            default:
                return Color.white;
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("Auto-Assign References")]
    private void AutoAssignReferences()
    {
        // Try to find common child names
        if (itemIcon == null)
        {
            itemIcon = transform.Find("IconImage")?.GetComponent<Image>() ??
                       transform.Find("Icon")?.GetComponent<Image>();
        }
        
        if (itemNameText == null)
        {
            itemNameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (equipmentTypeText == null)
        {
            equipmentTypeText = transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>() ??
                                transform.Find("EquipmentType")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (statsText == null)
        {
            statsText = transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>() ??
                        transform.Find("Stats")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (requirementsText == null)
        {
            requirementsText = transform.Find("RequirementsText")?.GetComponent<TextMeshProUGUI>() ??
                               transform.Find("Requirements")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (levelText == null)
        {
            levelText = transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>() ??
                        transform.Find("Level")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (implicitText == null)
        {
            implicitText = transform.Find("ImplicitText")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Implicit")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("ImplicitModifiers")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        
        if (selectedHighlight == null)
        {
            selectedHighlight = transform.Find("SelectedHighlight")?.GetComponent<Image>();
        }
        
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}

