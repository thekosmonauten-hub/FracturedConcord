using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Dexiled.Data.Items;

/// <summary>
/// Component for individual card slots in the Seer vendor grid.
/// Displays a card with price and handles selection.
/// </summary>
public class SeerCardVendorSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image backgroundImage;
    public Image cardIcon;
    public TextMeshProUGUI cardPriceText;
    public TextMeshProUGUI cardNameText;
    public GameObject soldOutOverlay; // Shows when card is purchased
    
    [Header("Selection")]
    [Tooltip("Visual indicator for selected state")]
    public GameObject selectionIndicator;
    
    private CardDataExtended card;
    private int price;
    private CurrencyType currencyType;
    private System.Action<SeerCardVendorSlot> onSelectedCallback;
    private bool isSelected = false;
    private bool isSoldOut = false;
    
    /// <summary>
    /// Initializes the vendor card slot with a card and price.
    /// </summary>
    public void Initialize(CardDataExtended card, int price, CurrencyType currencyType, System.Action<SeerCardVendorSlot> onSelected)
    {
        this.card = card;
        this.price = price;
        this.currencyType = currencyType;
        this.onSelectedCallback = onSelected;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// Updates the visual display of the card slot.
    /// </summary>
    private void UpdateDisplay()
    {
        if (card == null)
        {
            Debug.LogWarning("[SeerCardVendorSlot] Card is null! Cannot display.");
            return;
        }
        
        // Card icon/image
        if (cardIcon != null && card.cardThumbnail != null)
        {
            cardIcon.sprite = card.cardThumbnail;
            cardIcon.color = isSoldOut ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
        }
        
        // Card name
        if (cardNameText != null)
        {
            cardNameText.text = card.cardName;
        }
        
        // Card price
        if (cardPriceText != null)
        {
            cardPriceText.text = $"{price} {currencyType}";
        }
        
        // Background color based on rarity (optional visual feedback)
        if (backgroundImage != null)
        {
            Color rarityColor = GetRarityColor(card.rarity);
            rarityColor.a = 0.3f; // Semi-transparent
            backgroundImage.color = rarityColor;
        }
        
        // Update selection indicator
        UpdateSelectionState();
        
        // Sold out overlay
        if (soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(isSoldOut);
        }
    }
    
    /// <summary>
    /// Handles pointer click - selects the card.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSoldOut) return;
        
        SetSelected(true);
        onSelectedCallback?.Invoke(this);
    }
    
    /// <summary>
    /// Handles pointer enter - shows card tooltip (if available).
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (card == null || isSoldOut) return;
        
        // TODO: Implement card tooltip display if card tooltip system exists
        // For now, we can add this later if needed
    }
    
    /// <summary>
    /// Handles pointer exit - hides card tooltip (if available).
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO: Hide card tooltip if implemented
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
    /// Marks the card as sold out.
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
    /// Gets the card associated with this slot.
    /// </summary>
    public CardDataExtended GetCard()
    {
        return card;
    }
    
    /// <summary>
    /// Gets the price of this card.
    /// </summary>
    public int GetPrice()
    {
        return price;
    }
    
    /// <summary>
    /// Gets the currency type for this card.
    /// </summary>
    public CurrencyType GetCurrencyType()
    {
        return currencyType;
    }
    
    /// <summary>
    /// Checks if this card is sold out.
    /// </summary>
    public bool IsSoldOut()
    {
        return isSoldOut;
    }
    
    /// <summary>
    /// Gets the color for a rarity level.
    /// </summary>
    private Color GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return Color.white;
            case CardRarity.Magic:
                return new Color(0.2f, 0.6f, 1f); // Blue
            case CardRarity.Rare:
                return new Color(1f, 0.84f, 0f); // Gold
            case CardRarity.Unique:
                return new Color(1f, 0.5f, 0f); // Orange
            default:
                return Color.white;
        }
    }
}

