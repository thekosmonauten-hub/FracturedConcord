using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Unity UI version of equipment slot
/// Handles equipment slot display, interaction, and events
/// </summary>
public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler, 
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image backgroundImage;
    public Image itemIconImage;
    public TextMeshProUGUI slotLabel;
    public TextMeshProUGUI itemNameLabel;
    
    [Header("Settings")]
    public EquipmentType slotType;
    public Color emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color occupiedColor = new Color(0.2f, 0.3f, 0.4f, 0.9f);
    public Color hoverColor = new Color(0.3f, 0.4f, 0.5f, 1f);
    
    public event Action<EquipmentType> OnSlotClicked;
    public event Action<EquipmentType, Vector2> OnSlotHovered;
    
    private ItemData equippedItem = null;
    
    public void Initialize(EquipmentType type, string labelText)
    {
        slotType = type;
        if (slotLabel != null)
            slotLabel.text = labelText;
        
        UpdateVisual();
    }
    
    public void SetEquippedItem(ItemData item)
    {
        equippedItem = item;
        UpdateVisual();
    }
    
    public ItemData GetEquippedItem()
    {
        return equippedItem;
    }
    
    private void UpdateVisual()
    {
        if (equippedItem != null)
        {
            backgroundImage.color = occupiedColor;
            
            if (itemIconImage != null && equippedItem.itemSprite != null)
            {
                itemIconImage.sprite = equippedItem.itemSprite;
                itemIconImage.enabled = true;
            }
            
            if (itemNameLabel != null)
            {
                itemNameLabel.text = equippedItem.itemName;
                itemNameLabel.enabled = true;
            }
        }
        else
        {
            backgroundImage.color = emptyColor;
            
            if (itemIconImage != null)
                itemIconImage.enabled = false;
            
            if (itemNameLabel != null)
                itemNameLabel.enabled = false;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(slotType);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (equippedItem != null)
        {
            OnSlotHovered?.Invoke(slotType, eventData.position);
        }
        
        // Visual feedback
        Color originalColor = backgroundImage.color;
        backgroundImage.color = hoverColor;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateVisual(); // Restore original color
        if (ItemTooltipManager.Instance != null)
        {
            ItemTooltipManager.Instance.HideTooltip();
        }
    }
}

