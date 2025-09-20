using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Content")]
    public string title = "";
    public string content = "";
    
    [Header("Tooltip Settings")]
    public float delay = 0.5f;
    public Vector2 offset = new Vector2(10f, 10f);
    
    private bool isHovering = false;
    private float hoverTimer = 0f;
    private GameObject tooltipInstance;
    
    private void Update()
    {
        if (isHovering)
        {
            hoverTimer += Time.deltaTime;
            
            if (hoverTimer >= delay && tooltipInstance == null)
            {
                ShowTooltip();
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        hoverTimer = 0f;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        hoverTimer = 0f;
        HideTooltip();
    }
    
    private void ShowTooltip()
    {
        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(content))
            return;
        
        // Create tooltip GameObject
        tooltipInstance = new GameObject("Tooltip");
        tooltipInstance.transform.SetParent(transform.root);
        
        // Add Canvas if needed
        Canvas canvas = tooltipInstance.GetComponent<Canvas>();
        if (canvas == null)
            canvas = tooltipInstance.AddComponent<Canvas>();
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        // Add CanvasScaler
        var scaler = tooltipInstance.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null)
            scaler = tooltipInstance.AddComponent<UnityEngine.UI.CanvasScaler>();
        
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster
        var raycaster = tooltipInstance.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster == null)
            raycaster = tooltipInstance.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create background
        var background = new GameObject("Background");
        background.transform.SetParent(tooltipInstance.transform);
        
        var backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.zero;
        backgroundRect.pivot = Vector2.zero;
        
        // Create content
        var contentObj = new GameObject("Content");
        contentObj.transform.SetParent(background.transform);
        
        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(10f, 10f);
        contentRect.offsetMax = new Vector2(-10f, -10f);
        
        // Create title text
        if (!string.IsNullOrEmpty(title))
        {
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(contentObj.transform);
            
            var titleText = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 16;
            titleText.fontStyle = TMPro.FontStyles.Bold;
            titleText.color = Color.white;
            
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = new Vector2(0, -20f);
        }
        
        // Create content text
        if (!string.IsNullOrEmpty(content))
        {
            var contentTextObj = new GameObject("ContentText");
            contentTextObj.transform.SetParent(contentObj.transform);
            
            var contentText = contentTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            contentText.text = content;
            contentText.fontSize = 14;
            contentText.color = Color.white;
#pragma warning disable CS0618 // Type or member is obsolete
            contentText.enableWordWrapping = true; // Using the working property for this Unity version
#pragma warning restore CS0618 // Type or member is obsolete
        }
        
        // Position tooltip
        Vector2 mousePos = Input.mousePosition;
        Vector2 tooltipPos = mousePos + offset;
        
        var tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        tooltipRect.anchoredPosition = tooltipPos;
        
        // Size tooltip to content
        var contentSizeFitter = background.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        contentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        
        // Force layout update
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);
    }
    
    private void HideTooltip()
    {
        if (tooltipInstance != null)
        {
            DestroyImmediate(tooltipInstance);
            tooltipInstance = null;
        }
    }
    
    private void OnDestroy()
    {
        HideTooltip();
    }
    
    private void OnDisable()
    {
        HideTooltip();
    }
}

