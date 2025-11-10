using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Optional: Creates a large preview of the hovered card in a separate panel.
/// This avoids GridLayoutGroup conflicts.
/// </summary>
public class CardHoverPreview : MonoBehaviour
{
    [Header("Preview Settings")]
    [SerializeField] private RectTransform previewPanel;
    [SerializeField] private GameObject cardPreviewPrefab;
    [SerializeField] private Vector2 previewOffset = new Vector2(50, 0);
    [SerializeField] private float previewScale = 1.5f;
    
    private GameObject currentPreview;
    
    private void Awake()
    {
        // Ensure the preview panel itself doesn't block raycasts
        if (previewPanel != null)
        {
            // Disable all graphics on the panel itself
            var panelGraphics = previewPanel.GetComponents<Graphic>();
            foreach (var graphic in panelGraphics)
            {
                graphic.raycastTarget = false;
            }
            
            // Ensure CanvasGroup doesn't block if it exists
            var panelCanvasGroup = previewPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }
        }
    }
    
    /// <summary>
    /// Show an enlarged preview of the card.
    /// </summary>
    public void ShowPreview(DeckBuilderCardUI sourceCard, CardData cardData)
    {
        HidePreview();
        
        if (previewPanel == null || cardPreviewPrefab == null)
        {
            Debug.LogWarning("Preview panel or prefab not assigned!");
            return;
        }
        
        // Create preview
        currentPreview = Instantiate(cardPreviewPrefab, previewPanel);
        
        // Initialize with card data
        DeckBuilderCardUI previewUI = currentPreview.GetComponent<DeckBuilderCardUI>();
        if (previewUI != null)
        {
            previewUI.Initialize(cardData, null);
            // Disable interaction on preview
            previewUI.enabled = false;
        }
        
        // CRITICAL: Disable raycasting on ALL graphics in the preview to prevent blocking
        var allGraphics = currentPreview.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in allGraphics)
        {
            graphic.raycastTarget = false;
        }
        
        // Also disable any CanvasGroup to ensure no blocking
        var canvasGroups = currentPreview.GetComponentsInChildren<CanvasGroup>(true);
        foreach (var cg in canvasGroups)
        {
            cg.blocksRaycasts = false;
        }
        
        // Position near the source card
        RectTransform previewRect = currentPreview.GetComponent<RectTransform>();
        previewRect.localScale = Vector3.one * previewScale;
        
        // Position to the right of the hovered card
        Vector2 sourcePos = sourceCard.GetComponent<RectTransform>().position;
        previewRect.position = sourcePos + previewOffset;
        
        // Ensure it stays on screen
        ClampToScreen(previewRect);
    }
    
    /// <summary>
    /// Hide the current preview.
    /// </summary>
    public void HidePreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }
    
    /// <summary>
    /// Keep preview within screen bounds.
    /// </summary>
    private void ClampToScreen(RectTransform rect)
    {
        Vector3 pos = rect.position;
        Vector2 size = rect.rect.size * rect.localScale.x;
        
        // Clamp X
        if (pos.x + size.x > Screen.width)
        {
            pos.x = Screen.width - size.x;
        }
        if (pos.x < 0)
        {
            pos.x = 0;
        }
        
        // Clamp Y
        if (pos.y + size.y > Screen.height)
        {
            pos.y = Screen.height - size.y;
        }
        if (pos.y < 0)
        {
            pos.y = 0;
        }
        
        rect.position = pos;
    }
}








