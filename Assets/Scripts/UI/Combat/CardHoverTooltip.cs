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
    [Tooltip("Background color for tooltip entries")]
    public Color tooltipBackground = new Color(0f, 0f, 0f, 0.65f);
    [Tooltip("Text color for tooltip entries")]
    public Color tooltipText = Color.white;
    [Tooltip("Font size for tooltip text")]
    public float tooltipFontSize = 14f;
    [Tooltip("Optional icon sprite for each tooltip entry (e.g., ailment icon)")]
    public Sprite entryIconSprite;
    [Tooltip("Icon size for entries")]
    public Vector2 entryIconSize = new Vector2(22f, 22f);
    
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
            // Build a simple entry
            entry = new GameObject("TooltipEntry", typeof(RectTransform));
            var bg = entry.AddComponent<Image>();
            bg.color = tooltipBackground;

            var pad = entry.AddComponent<LayoutElement>();
            pad.minWidth = 220f;
            pad.minHeight = 36f;

            // Row container to hold icon + text
            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(entry.transform, false);
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            // Optional icon
            if (entryIconSprite != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(row.transform, false);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = entryIconSprite;
                var iconLE = iconGO.AddComponent<LayoutElement>();
                iconLE.preferredWidth = entryIconSize.x;
                iconLE.preferredHeight = entryIconSize.y;
            }

            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(entry.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.enableWordWrapping = true;
            tmp.fontSize = tooltipFontSize;
            tmp.color = tooltipText;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.text = text;

            var textFit = textGO.AddComponent<ContentSizeFitter>();
            textFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var entryLayout = entry.AddComponent<VerticalLayoutGroup>();
            entryLayout.padding = new RectOffset(10, 10, 6, 6);
            entryLayout.childAlignment = TextAnchor.MiddleLeft;
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
}


