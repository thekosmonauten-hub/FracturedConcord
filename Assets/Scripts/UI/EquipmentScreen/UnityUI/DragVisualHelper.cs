using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual representation of items being dragged
/// Shows a ghost image that follows the cursor
/// </summary>
public class DragVisualHelper : MonoBehaviour
{
    public static DragVisualHelper Instance { get; private set; }
    
    [Header("Drag Visual Settings")]
    [SerializeField] private Canvas dragCanvas;
    [SerializeField] private GameObject dragVisualPrefab;
    [SerializeField] private float dragAlpha = 0.7f;
    
    private GameObject currentDragVisual;
    private Image dragImage;
    private BaseItem draggedItem;
    private bool isDragging = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        // Find drag canvas if not assigned
        if (dragCanvas == null)
        {
            dragCanvas = GetComponentInParent<Canvas>();
            if (dragCanvas == null)
            {
                // Try to find any canvas in scene
                dragCanvas = FindFirstObjectByType<Canvas>();
            }
        }
    }
    
    /// <summary>
    /// Begin dragging an item
    /// </summary>
    public void BeginDrag(BaseItem item, Sprite itemSprite)
    {
        if (item == null) return;
        
        draggedItem = item;
        isDragging = true;
        
        // Create drag visual
        if (dragVisualPrefab != null && dragCanvas != null)
        {
            currentDragVisual = Instantiate(dragVisualPrefab, dragCanvas.transform);
            dragImage = currentDragVisual.GetComponent<Image>();
            
            if (dragImage != null && itemSprite != null)
            {
                dragImage.sprite = itemSprite;
                
                // Set alpha for ghost effect
                Color color = dragImage.color;
                color.a = dragAlpha;
                dragImage.color = color;
                
                // Disable raycast so it doesn't block other UI elements
                dragImage.raycastTarget = false;
            }
            
            currentDragVisual.transform.SetAsLastSibling(); // Render on top
        }
        else
        {
            // Fallback: Create simple drag visual
            GameObject dragObj = new GameObject("DragVisual");
            dragObj.transform.SetParent(dragCanvas != null ? dragCanvas.transform : transform);
            
            dragImage = dragObj.AddComponent<Image>();
            if (itemSprite != null)
            {
                dragImage.sprite = itemSprite;
            }
            
            Color color = dragImage.color;
            color.a = dragAlpha;
            dragImage.color = color;
            dragImage.raycastTarget = false;
            
            currentDragVisual = dragObj;
            currentDragVisual.transform.SetAsLastSibling();
        }
        
        Debug.Log($"[DragVisualHelper] Begin drag: {item.GetDisplayName()}");
    }
    
    /// <summary>
    /// Update drag visual position to follow cursor
    /// </summary>
    public void UpdateDragPosition(Vector2 screenPosition)
    {
        if (!isDragging || currentDragVisual == null) return;
        
        // Convert screen position to canvas position
        if (dragCanvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragCanvas.transform as RectTransform,
                screenPosition,
                dragCanvas.worldCamera,
                out localPoint
            );
            
            currentDragVisual.transform.localPosition = localPoint;
        }
        else
        {
            currentDragVisual.transform.position = screenPosition;
        }
    }
    
    /// <summary>
    /// End drag operation and destroy visual
    /// </summary>
    public void EndDrag()
    {
        isDragging = false;
        draggedItem = null;
        
        if (currentDragVisual != null)
        {
            Destroy(currentDragVisual);
            currentDragVisual = null;
            dragImage = null;
        }
        
        Debug.Log($"[DragVisualHelper] End drag");
    }
    
    /// <summary>
    /// Get the item currently being dragged
    /// </summary>
    public BaseItem GetDraggedItem()
    {
        return draggedItem;
    }
    
    /// <summary>
    /// Check if currently dragging
    /// </summary>
    public bool IsDragging()
    {
        return isDragging;
    }
}


