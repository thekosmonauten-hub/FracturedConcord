using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Unity UI version of inventory slot
/// Handles individual slot behavior, visuals, and events
/// Supports both click-to-equip and drag-and-drop
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, 
    IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image background;
    public Image itemIcon;
    public TextMeshProUGUI itemLabel;
    
    [Header("Colors")]
    public Color normalColor = new Color(0.12f, 0.14f, 0.16f);
    public Color hoverColor = new Color(0.2f, 0.22f, 0.24f);
    public Color occupiedColor = new Color(0.3f, 0.4f, 0.5f);
    public Color selectedColor = new Color(0.9f, 0.7f, 0.2f); // Gold/yellow for selection
    
    private bool hasSprite = false; // Track if background has a sprite
    private bool isSelected = false;
    
    public event Action OnSlotClicked;
    public event Action OnSlotHovered;
    public event Action<int, int> OnDragStarted; // (x, y)
    public event Action<int, int, Vector2> OnDragging; // (x, y, screenPosition)
    public event Action<int, int> OnDragEnded; // (x, y)
    
    private int posX;
    private int posY;
    private bool isOccupied = false;
    private Color currentColor; // Track current color for proper restoration
    private BaseItem currentItem; // Store reference to current item
    
    void Start()
    {
        // Check if background has a sprite
        hasSprite = (background != null && background.sprite != null);
        
        // Initialize to normal color
        currentColor = normalColor;
        
        if (background != null)
        {
            // If background has a sprite, Unity multiplies: sprite color × Image.color
            // If sprite is dark/colored, result will be darker
            // Solution: Use white sprite OR remove sprite and use solid color
            if (hasSprite)
            {
                // Set color to white so sprite shows at full brightness
                // Then apply our tint (but this still multiplies with sprite)
                // BEST: Replace sprite with white UI sprite in prefab
                background.color = normalColor;
            }
            else
            {
                // No sprite = solid color background (this works perfectly)
                background.color = normalColor;
            }
        }
    }
    
    public void SetPosition(int x, int y)
    {
        posX = x;
        posY = y;
    }
    
    public void SetOccupied(bool occupied, Sprite icon = null, string itemName = null, BaseItem item = null)
    {
        isOccupied = occupied;
        currentItem = item;
        
        if (occupied)
        {
            currentColor = occupiedColor;
            // Apply color based on whether sprite exists
            if (hasSprite)
                background.color = Color.white * occupiedColor;
            else
                background.color = occupiedColor;
            
            if (icon != null && itemIcon != null)
            {
                itemIcon.sprite = icon;
                itemIcon.gameObject.SetActive(true); // Show GameObject
            }
            
            if (!string.IsNullOrEmpty(itemName) && itemLabel != null)
            {
                itemLabel.text = itemName;
                itemLabel.gameObject.SetActive(true); // Show GameObject
            }
        }
        else
        {
            currentColor = normalColor;
            // Apply color based on whether sprite exists
            if (hasSprite)
                background.color = Color.white * normalColor;
            else
                background.color = normalColor;
            
            if (itemIcon != null)
                itemIcon.gameObject.SetActive(false); // Hide GameObject
            
            if (itemLabel != null)
                itemLabel.gameObject.SetActive(false); // Hide GameObject
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOccupied)
        {
            // Apply hover color based on whether sprite exists
            if (hasSprite)
                background.color = Color.white * hoverColor;
            else
                background.color = hoverColor;
        }
        OnSlotHovered?.Invoke();
    }
    
    /// <summary>
    /// Set selected state (visual feedback)
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            // Highlight with selection color
            if (hasSprite)
                background.color = Color.white * selectedColor;
            else
                background.color = selectedColor;
        }
        else
        {
            // Restore normal colors
            if (isOccupied)
            {
                if (hasSprite)
                    background.color = Color.white * occupiedColor;
                else
                    background.color = occupiedColor;
            }
            else
            {
                if (hasSprite)
                    background.color = Color.white * normalColor;
                else
                    background.color = normalColor;
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Don't change color if selected
        if (isSelected) return;
        
        // Restore to current state color (normalColor or occupiedColor)
        if (background != null)
        {
            if (hasSprite)
                background.color = Color.white * currentColor;
            else
                background.color = currentColor;
        }
    }
    
    // ====================
    // DRAG & DROP HANDLERS
    // ====================
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only allow dragging occupied slots
        if (!isOccupied || currentItem == null)
            return;
        
        Debug.Log($"[InventorySlotUI] Begin drag: ({posX}, {posY})");
        
        // Notify parent
        OnDragStarted?.Invoke(posX, posY);
        
        // Create drag visual
        DragVisualHelper.Instance?.BeginDrag(currentItem, itemIcon != null ? itemIcon.sprite : null);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isOccupied || currentItem == null)
            return;
        
        // Update drag visual position
        DragVisualHelper.Instance?.UpdateDragPosition(eventData.position);
        
        // Notify parent for potential drop zone highlighting
        OnDragging?.Invoke(posX, posY, eventData.position);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isOccupied || currentItem == null)
            return;
        
        Debug.Log($"[InventorySlotUI] End drag: ({posX}, {posY})");
        
        // End drag visual
        DragVisualHelper.Instance?.EndDrag();
        
        // Notify parent
        OnDragEnded?.Invoke(posX, posY);
    }
    
    /// <summary>
    /// Get current item in this slot (for drag operations)
    /// </summary>
    public BaseItem GetCurrentItem()
    {
        return currentItem;
    }
    
    /// <summary>
    /// Get slot position
    /// </summary>
    public (int x, int y) GetPosition()
    {
        return (posX, posY);
    }
}


