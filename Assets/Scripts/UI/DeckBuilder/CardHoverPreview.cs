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








