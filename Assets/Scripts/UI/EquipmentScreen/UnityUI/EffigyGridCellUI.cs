using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Unity UI version of effigy grid cell
/// Handles individual cell interactions for drag & drop
/// </summary>
public class EffigyGridCellUI : MonoBehaviour, 
    IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image backgroundImage;
    
    [Header("Settings")]
    public int cellX;
    public int cellY;
    
    [Header("Colors")]
    public Color normalColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    public Color borderColor = new Color(0.3f, 0.3f, 0.3f);
    
    public event Action<int, int, PointerEventData> OnCellMouseDown;
    public event Action<int, int> OnCellMouseEnter;
    public event Action<int, int, PointerEventData> OnCellMouseUp;
    
    private Outline outline;
    
    void Awake()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        // Add outline component for borders
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();
        
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(1, 1);
        
        // Set initial color
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnCellMouseDown?.Invoke(cellX, cellY, eventData);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnCellMouseEnter?.Invoke(cellX, cellY);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        OnCellMouseUp?.Invoke(cellX, cellY, eventData);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Drag started (can be used for additional feedback)
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Dragging (can be used for drag preview)
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Drag ended
    }
    
    public void SetHighlight(Color highlightColor)
    {
        if (backgroundImage != null)
            backgroundImage.color = highlightColor;
        
        // Make border more visible during highlight
        if (outline != null)
        {
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, 2);
        }
    }
    
    public void ClearHighlight()
    {
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
        
        // Restore normal border
        if (outline != null)
        {
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(1, 1);
        }
    }
}

