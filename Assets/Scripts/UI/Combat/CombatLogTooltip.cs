using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dexiled.Data.Items;

/// <summary>
/// Tooltip that shows detailed information about loot when hovering over combat log entries
/// </summary>
public class CombatLogTooltip : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitle;
    public TextMeshProUGUI tooltipDescription;
    public Image tooltipIcon;
    public RectTransform tooltipRectTransform;
    
    [Header("Colors")]
    public Color currencyColor = new Color(1f, 0.84f, 0f);
    public Color itemColor = new Color(0.8f, 0.8f, 1f);
    public Color experienceColor = new Color(0.5f, 1f, 0.5f);
    
    private CurrencyDatabase currencyDatabase;
    
    private void Start()
    {
        // Load currency database
        currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        
        // Hide by default
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    public void ShowLootTooltip(LootReward loot, Vector2 position)
    {
        if (tooltipPanel == null)
            return;
        
        tooltipPanel.SetActive(true);
        
        // Set title and description based on loot type
        switch (loot.rewardType)
        {
            case RewardType.Currency:
                ShowCurrencyTooltip(loot.currencyType);
                break;
                
            case RewardType.Item:
                ShowItemTooltip(loot.itemData);
                break;
                
            case RewardType.Experience:
                ShowExperienceTooltip(loot.experienceAmount);
                break;
                
            case RewardType.Card:
                ShowCardTooltip(loot.cardName);
                break;
        }
        
        // Position tooltip near cursor
        if (tooltipRectTransform != null)
        {
            tooltipRectTransform.position = position + new Vector2(10, 10);
        }
    }
    
    private void ShowCurrencyTooltip(CurrencyType currencyType)
    {
        if (currencyDatabase != null)
        {
            var currencyData = currencyDatabase.GetCurrency(currencyType);
            if (currencyData != null)
            {
                if (tooltipTitle != null)
                {
                    tooltipTitle.text = currencyData.currencyName;
                    tooltipTitle.color = GetRarityColor(currencyData.rarity);
                }
                
                if (tooltipDescription != null)
                {
                    tooltipDescription.text = currencyData.description;
                }
                
                if (tooltipIcon != null && currencyData.currencySprite != null)
                {
                    tooltipIcon.sprite = currencyData.currencySprite;
                    tooltipIcon.enabled = true;
                }
                else if (tooltipIcon != null)
                {
                    tooltipIcon.enabled = false;
                }
            }
        }
    }
    
    private void ShowItemTooltip(ItemData itemData)
    {
        if (itemData == null)
            return;
        
        if (tooltipTitle != null)
        {
            tooltipTitle.text = itemData.itemName;
            tooltipTitle.color = GetRarityColor(itemData.rarity);
        }
        
        if (tooltipDescription != null)
        {
            // Build item description
            string desc = $"{itemData.itemType} - Level {itemData.level}\n\n";
            
            // Add weapon stats
            if (itemData.baseDamageMin > 0 || itemData.baseDamageMax > 0)
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
                    int rolledBase = Mathf.RoundToInt(rolledBaseDamage);
                    desc += $"Damage: {rolledBase}\n";
                }
                else
                {
                    // Fallback: Show range
                    desc += $"Damage: {itemData.baseDamageMin}-{itemData.baseDamageMax}\n";
                }
            }
            
            // Add armor stats
            if (itemData.baseArmour > 0)
                desc += $"Armour: {itemData.baseArmour}\n";
            if (itemData.baseEvasion > 0)
                desc += $"Evasion: {itemData.baseEvasion}\n";
            if (itemData.baseEnergyShield > 0)
                desc += $"Energy Shield: {itemData.baseEnergyShield}\n";
            
            // Add requirements
            if (itemData.requiredStrength > 0 || itemData.requiredDexterity > 0 || itemData.requiredIntelligence > 0)
            {
                desc += "\nRequirements:\n";
                if (itemData.requiredStrength > 0) desc += $"Strength: {itemData.requiredStrength}\n";
                if (itemData.requiredDexterity > 0) desc += $"Dexterity: {itemData.requiredDexterity}\n";
                if (itemData.requiredIntelligence > 0) desc += $"Intelligence: {itemData.requiredIntelligence}\n";
            }
            
            tooltipDescription.text = desc;
        }
        
        if (tooltipIcon != null && itemData.itemSprite != null)
        {
            tooltipIcon.sprite = itemData.itemSprite;
            tooltipIcon.enabled = true;
        }
        else if (tooltipIcon != null)
        {
            tooltipIcon.enabled = false;
        }
    }
    
    private void ShowExperienceTooltip(int amount)
    {
        if (tooltipTitle != null)
        {
            tooltipTitle.text = "Experience Gained";
            tooltipTitle.color = experienceColor;
        }
        
        if (tooltipDescription != null)
        {
            tooltipDescription.text = $"+{amount} Experience\n\nGained from defeating enemies.";
        }
        
        if (tooltipIcon != null)
        {
            tooltipIcon.enabled = false;
        }
    }
    
    private void ShowCardTooltip(string cardName)
    {
        if (tooltipTitle != null)
        {
            tooltipTitle.text = cardName;
            tooltipTitle.color = new Color(0.9f, 0.7f, 1f);
        }
        
        if (tooltipDescription != null)
        {
            tooltipDescription.text = "A new card has been added to your collection!";
        }
        
        if (tooltipIcon != null)
        {
            tooltipIcon.enabled = false;
        }
    }
    
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
    
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal:
                return Color.white;
            case ItemRarity.Magic:
                return new Color(0.4f, 0.4f, 1f); // Blue
            case ItemRarity.Rare:
                return new Color(1f, 1f, 0.4f); // Yellow
            case ItemRarity.Unique:
                return new Color(1f, 0.5f, 0f); // Orange
            default:
                return Color.white;
        }
    }
}













