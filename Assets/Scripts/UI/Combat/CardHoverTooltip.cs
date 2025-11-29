using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows vertical tooltips to the left of a card when hovered in combat.
/// </summary>
public class CardHoverTooltip : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Enable/disable hover tooltips for this card instance")]
    public bool enableTooltips = true;
    [Tooltip("Offset (in pixels) from the card to the left where tooltips appear")]
    public Vector2 anchorOffset = new Vector2(-260f, 0f);
    [Tooltip("Vertical spacing between tooltip entries")]
    public float verticalSpacing = 6f;

    [Header("Prefab/Styling")]
    [Tooltip("Optional: Tooltip entry prefab. If not set, a simple runtime one is created.")]
    public GameObject tooltipEntryPrefab;
    
    [Header("Background Styling")]
    [Tooltip("Background color for tooltip entries")]
    public Color tooltipBackground = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Dark blue-gray, more opaque
    [Tooltip("Optional: Background sprite/material for tooltip entries")]
    public Sprite backgroundSprite;
    [Tooltip("Border color for tooltip entries")]
    public Color borderColor = new Color(0.8f, 0.6f, 0.2f, 1f); // Gold/amber
    [Tooltip("Border width in pixels")]
    public float borderWidth = 2f;
    [Tooltip("Corner radius for rounded tooltip entries (0 = square)")]
    public float cornerRadius = 8f;
    
    [Header("Text Styling")]
    [Tooltip("Text color for tooltip entries")]
    public Color tooltipText = new Color(0.95f, 0.95f, 0.95f, 1f); // Off-white
    [Tooltip("Font size for tooltip text")]
    public float tooltipFontSize = 15f;
    [Tooltip("Font style (Normal, Bold, Italic, BoldAndItalic)")]
    public FontStyles fontStyle = FontStyles.Normal;
    [Tooltip("Text outline/edge color for better visibility")]
    public Color textOutlineColor = new Color(0f, 0f, 0f, 0.8f);
    [Tooltip("Text outline width")]
    public float textOutlineWidth = 2f;
    
    [Header("Layout & Spacing")]
    [Tooltip("Minimum width for tooltip entries")]
    public float minWidth = 250f;
    [Tooltip("Minimum height for tooltip entries")]
    public float minHeight = 40f;
    [Tooltip("Padding inside tooltip entries (Left, Right, Top, Bottom)")]
    public Vector4 padding = new Vector4(12f, 12f, 8f, 8f);
    [Tooltip("Spacing between icon and text")]
    public float iconTextSpacing = 10f;
    
    [Header("Icon Settings")]
    [Tooltip("Optional icon sprite for each tooltip entry (e.g., ailment icon)")]
    public Sprite entryIconSprite;
    [Tooltip("Icon size for entries")]
    public Vector2 entryIconSize = new Vector2(24f, 24f);
    
    [Header("Shadow/Glow Effects")]
    [Tooltip("Enable shadow effect behind tooltip")]
    public bool enableShadow = true;
    [Tooltip("Shadow color")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);
    [Tooltip("Shadow offset")]
    public Vector2 shadowOffset = new Vector2(2f, -2f);
    [Tooltip("Shadow blur size")]
    public float shadowBlur = 4f;
    
    private RectTransform container;
    private bool initialized = false;

    private void EnsureContainer()
    {
        if (initialized) return;
        initialized = true;

        // Create container to the left of the card
        GameObject go = new GameObject("HoverTooltips", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        container = go.GetComponent<RectTransform>();
        container.anchorMin = new Vector2(0.5f, 0.5f);
        container.anchorMax = new Vector2(0.5f, 0.5f);
        container.pivot = new Vector2(1f, 0.5f); // right-align so it grows left of the card
        container.anchoredPosition = anchorOffset;

        var cg = container.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; // ensure tooltip never captures pointer

        var layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = verticalSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var fitter = container.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        container.gameObject.SetActive(false);
    }

    public void Show(string[] lines)
    {
        if (!enableTooltips || lines == null || lines.Length == 0) return;
        EnsureContainer();
        Clear();

        foreach (var line in lines)
        {
            CreateEntry(line);
        }
        container.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (container == null) return;
        container.gameObject.SetActive(false);
    }

    private void Clear()
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private void CreateEntry(string text)
    {
        GameObject entry = tooltipEntryPrefab;
        if (entry == null)
        {
            // Build an enhanced entry with better styling
            entry = new GameObject("TooltipEntry", typeof(RectTransform));
            var entryRect = entry.GetComponent<RectTransform>();
            
            // Add shadow effect if enabled
            if (enableShadow)
            {
                CreateShadow(entry);
            }
            
            // Background image with border
            var bg = entry.AddComponent<Image>();
            if (backgroundSprite != null)
            {
                bg.sprite = backgroundSprite;
                bg.type = Image.Type.Sliced; // Use 9-slice for rounded corners
            }
            else
            {
                // Create a simple colored background
                bg.color = tooltipBackground;
            }
            
            // Add border using Outline or create a border image
            if (borderWidth > 0f)
            {
                CreateBorder(entry, bg);
            }

            // Layout element for sizing
            var pad = entry.AddComponent<LayoutElement>();
            pad.minWidth = minWidth;
            pad.minHeight = minHeight;

            // Row container to hold icon + text
            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(entry.transform, false);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = Vector2.zero;
            rowRect.anchorMax = Vector2.one;
            rowRect.offsetMin = Vector2.zero;
            rowRect.offsetMax = Vector2.zero;
            
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = iconTextSpacing;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            rowLayout.padding = new RectOffset(
                Mathf.RoundToInt(padding.x), 
                Mathf.RoundToInt(padding.y), 
                Mathf.RoundToInt(padding.z), 
                Mathf.RoundToInt(padding.w)
            );

            // Optional icon
            if (entryIconSprite != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(row.transform, false);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = entryIconSprite;
                iconImg.preserveAspect = true;
                var iconLE = iconGO.AddComponent<LayoutElement>();
                iconLE.preferredWidth = entryIconSize.x;
                iconLE.preferredHeight = entryIconSize.y;
                iconLE.flexibleWidth = 0;
                iconLE.flexibleHeight = 0;
            }

            // Text with enhanced styling
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(row.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.fontSize = tooltipFontSize;
            tmp.color = tooltipText;
            tmp.fontStyle = fontStyle;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.text = text;
            
            // Add text outline for better visibility
            if (textOutlineWidth > 0f)
            {
                tmp.outlineWidth = textOutlineWidth;
                tmp.outlineColor = textOutlineColor;
            }

            var textFit = textGO.AddComponent<ContentSizeFitter>();
            textFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var entryLayout = entry.AddComponent<VerticalLayoutGroup>();
            entryLayout.padding = new RectOffset(0, 0, 0, 0); // Padding handled by row
            entryLayout.childAlignment = TextAnchor.MiddleLeft;
            entryLayout.childForceExpandWidth = true;
            entryLayout.childForceExpandHeight = false;
        }
        else
        {
            entry = Instantiate(tooltipEntryPrefab);
            // Try to set its child TMP text
            var tmp = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;
        }

        entry.transform.SetParent(container, false);
    }
    
    /// <summary>
    /// Create a shadow effect behind the tooltip entry
    /// </summary>
    private void CreateShadow(GameObject parent)
    {
        GameObject shadow = new GameObject("Shadow", typeof(RectTransform));
        shadow.transform.SetParent(parent.transform, false);
        shadow.transform.SetAsFirstSibling(); // Put shadow behind everything
        
        var shadowRect = shadow.GetComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = shadowOffset;
        shadowRect.offsetMax = shadowOffset;
        
        var shadowImg = shadow.AddComponent<Image>();
        shadowImg.color = shadowColor;
        shadowImg.raycastTarget = false;
        
        // Add blur effect if available (requires additional setup)
        // For now, just use a darker, offset background
    }
    
    /// <summary>
    /// Create a border around the tooltip entry
    /// </summary>
    private void CreateBorder(GameObject parent, Image background)
    {
        // Create border using Outline component on the background
        var outline = background.gameObject.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(borderWidth, -borderWidth);
        outline.useGraphicAlpha = true;
        
        // Alternative: Create a border image as a child (more control but more complex)
        // For now, using Outline component is simpler and works well
    }
}


