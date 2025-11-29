using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Dexiled.Data.Items;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Component for individual vendor item slots in the vendor grid.
    /// Simple display with background, icon, price, and tooltip support.
    /// </summary>
    public class MazeVendorItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        public Image backgroundImage;
        public Image itemIcon;
        public TextMeshProUGUI itemPriceText;
        public GameObject soldOutOverlay; // Shows when item is purchased
        
        [Header("Selection")]
        [Tooltip("Visual indicator for selected state")]
        public GameObject selectionIndicator;
        
        private BaseItem item;
        private int price;
        private CurrencyType currencyType = CurrencyType.MandateFragment;
        private System.Action<MazeVendorItemSlot> onSelectedCallback;
        private bool isSelected = false;
        private bool isSoldOut = false;
        
        /// <summary>
        /// Initializes the vendor item slot with an item and price.
        /// </summary>
        public void Initialize(BaseItem item, int price, System.Action<MazeVendorItemSlot> onSelected, CurrencyType currency = CurrencyType.MandateFragment)
        {
            this.item = item;
            this.price = price;
            this.onSelectedCallback = onSelected;
            this.currencyType = currency;
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Updates the visual display of the item slot.
        /// </summary>
        private void UpdateDisplay()
        {
            if (item == null)
            {
                Debug.LogWarning("[MazeVendorItemSlot] Item is null! Cannot display.");
                return;
            }
            
            // Item icon
            if (itemIcon != null && item.itemIcon != null)
            {
                itemIcon.sprite = item.itemIcon;
                itemIcon.color = isSoldOut ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
            }
            
            // Item price
            if (itemPriceText != null)
            {
                itemPriceText.text = $"{price} {currencyType}";
            }
            
            // Background color based on rarity (optional visual feedback)
            if (backgroundImage != null)
            {
                Color rarityColor = GetRarityColor(item.GetCalculatedRarity());
                rarityColor.a = 0.3f; // Semi-transparent
                backgroundImage.color = rarityColor;
            }
            
            // Update selection indicator
            UpdateSelectionState();
        }
        
        /// <summary>
        /// Handles pointer click - selects the item.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isSoldOut) return;
            
            SetSelected(true);
            onSelectedCallback?.Invoke(this);
        }
        
        /// <summary>
        /// Handles pointer enter - shows tooltip using ItemTooltipManager.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item == null || isSoldOut) return;
            
            // Use the centralized ItemTooltipManager if available
            if (ItemTooltipManager.Instance != null)
            {
                ItemTooltipManager.Instance.ShowTooltipForPointer(item, eventData);
            }
            else
            {
                Debug.LogWarning("[MazeVendorItemSlot] ItemTooltipManager.Instance is null! Make sure ItemTooltipManager is in the scene.");
            }
        }
        
        /// <summary>
        /// Handles pointer exit - hides tooltip using ItemTooltipManager.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (ItemTooltipManager.Instance != null)
            {
                ItemTooltipManager.Instance.HideTooltip();
            }
        }
        
        /// <summary>
        /// Sets the selected state of this slot.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateSelectionState();
        }
        
        /// <summary>
        /// Updates the visual selection state.
        /// </summary>
        private void UpdateSelectionState()
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(isSelected);
            }
        }
        
        
        /// <summary>
        /// Marks the item as sold out.
        /// </summary>
        public void MarkAsSoldOut()
        {
            isSoldOut = true;
            
            if (soldOutOverlay != null)
            {
                soldOutOverlay.SetActive(true);
            }
            
            // Disable selection
            SetSelected(false);
            
            // Update display
            UpdateDisplay();
        }
        
        /// <summary>
        /// Gets the item associated with this slot.
        /// </summary>
        public BaseItem GetItem()
        {
            return item;
        }
        
        /// <summary>
        /// Gets the price of this item.
        /// </summary>
        public int GetPrice()
        {
            return price;
        }
        
        /// <summary>
        /// Gets the currency type for this item.
        /// </summary>
        public CurrencyType GetCurrencyType()
        {
            return currencyType;
        }
        
        /// <summary>
        /// Checks if this item is sold out.
        /// </summary>
        public bool IsSoldOut()
        {
            return isSoldOut;
        }
        
        /// <summary>
        /// Gets the color for a rarity level.
        /// </summary>
        private Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Normal:
                    return Color.white;
                case ItemRarity.Magic:
                    return new Color(0.2f, 0.6f, 1f); // Blue
                case ItemRarity.Rare:
                    return new Color(1f, 0.84f, 0f); // Gold
                case ItemRarity.Unique:
                    return new Color(1f, 0.5f, 0f); // Orange
                default:
                    return Color.white;
            }
        }
    }
}

