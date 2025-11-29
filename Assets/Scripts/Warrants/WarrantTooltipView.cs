using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple, data-driven tooltip view for warrants. It supports an arbitrary number of
/// sections/lines so we can render anywhere from 1 to 8+ modifiers without creating
/// new prefabs for each variant.
/// </summary>
public class WarrantTooltipView : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private TextMeshProUGUI subtitleLabel;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backdropImage;

    [Header("Layout References")]
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private TextMeshProUGUI lineTemplate;
    
    [Header("Notable Display (Optional)")]
    [Tooltip("If assigned, Notable data will be displayed here instead of in the sections. Leave empty to use sections.")]
    [SerializeField] private GameObject notableContainer;
    [SerializeField] private TextMeshProUGUI notableNameLabel;
    [SerializeField] private TextMeshProUGUI notableDescriptionLabel;
    [SerializeField] private RectTransform notableModifierContainer;
    [SerializeField] private TextMeshProUGUI notableModifierTemplate;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color magicColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color rareColor = new Color(1f, 0.9f, 0.4f);
    [SerializeField] private Color uniqueColor = new Color(1f, 0.55f, 0.15f);

    private readonly List<TextMeshProUGUI> spawnedLines = new List<TextMeshProUGUI>();

    private void Awake()
    {
        EnsureRuntimeLayout();
    }

    public void SetData(WarrantTooltipData data)
    {
        EnsureRuntimeLayout();

        if (titleLabel != null)
        {
            titleLabel.text = data?.title ?? string.Empty;
            titleLabel.color = GetRarityColor(data?.rarity ?? WarrantRarity.Common);
        }

        if (subtitleLabel != null)
        {
            bool hasSubtitle = data != null && !string.IsNullOrWhiteSpace(data.subtitle);
            subtitleLabel.gameObject.SetActive(hasSubtitle);
            if (hasSubtitle)
            {
                subtitleLabel.text = data.subtitle;
            }
        }

        if (iconImage != null)
        {
            iconImage.enabled = data != null && data.icon != null;
            iconImage.sprite = data?.icon;
        }

        RebuildNotableDisplay(data);
        RebuildLines(data);
    }
    
    private void RebuildNotableDisplay(WarrantTooltipData data)
    {
        // If Notable container is not assigned, Notable will be shown in sections (default behavior)
        if (notableContainer == null)
            return;
        
        bool hasNotable = data != null && 
                         (!string.IsNullOrWhiteSpace(data.notableDisplayName) || 
                          data.notableModifierNames.Count > 0);
        
        notableContainer.SetActive(hasNotable);
        
        if (!hasNotable)
            return;
        
        // Display Notable name
        if (notableNameLabel != null)
        {
            notableNameLabel.text = !string.IsNullOrWhiteSpace(data.notableDisplayName) 
                ? $"Notable: {data.notableDisplayName}" 
                : "Notable";
            notableNameLabel.gameObject.SetActive(true);
        }
        
        // Display Notable description
        if (notableDescriptionLabel != null)
        {
            bool hasDescription = !string.IsNullOrWhiteSpace(data.notableDescription);
            notableDescriptionLabel.gameObject.SetActive(hasDescription);
            if (hasDescription)
            {
                notableDescriptionLabel.text = data.notableDescription;
            }
        }
        
        // Display Notable modifier names
        if (notableModifierContainer != null && notableModifierTemplate != null)
        {
            // Clear existing modifier labels
            for (int i = notableModifierContainer.childCount - 1; i >= 0; i--)
            {
                var child = notableModifierContainer.GetChild(i);
                if (child != null && child.gameObject != notableModifierTemplate.gameObject)
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Create modifier labels
            foreach (var modifierName in data.notableModifierNames)
            {
                if (string.IsNullOrWhiteSpace(modifierName))
                    continue;
                
                var modifierLabel = Instantiate(notableModifierTemplate, notableModifierContainer);
                modifierLabel.gameObject.SetActive(true);
                modifierLabel.text = modifierName;
            }
        }
    }

    private void RebuildLines(WarrantTooltipData data)
    {
        foreach (var line in spawnedLines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        spawnedLines.Clear();

        if (data == null)
        {
            CreateLine(new WarrantTooltipLine { text = "No data available." });
            return;
        }

        // Find Notable section and Regular Modifiers section
        WarrantTooltipSection notableSection = null;
        WarrantTooltipSection modifiersSection = null;
        
        if (data.sections != null)
        {
            foreach (var section in data.sections)
            {
                if (section == null) continue;
                
                // Check if this is the Notable section
                if (!string.IsNullOrWhiteSpace(section.header) && 
                    (section.header.Contains("Notable") || section.header.Contains("NOTABLE")))
                {
                    notableSection = section;
                }
                // Check if this is the regular Modifiers section
                else if (!string.IsNullOrWhiteSpace(section.header) && 
                         (section.header.Equals("Modifiers", System.StringComparison.OrdinalIgnoreCase) ||
                          section.header.Equals("MODIFIERS", System.StringComparison.OrdinalIgnoreCase)))
                {
                    modifiersSection = section;
                }
            }
        }

        // Build the layout in the requested order:
        // 1. Warrant Name(s) - already shown in titleLabel
        // 2. Separator (if Notable exists)
        // 3. Notable name + Notable modifiers
        // 4. Separator (if Notable exists)
        // 5. Regular affixes/modifiers

        bool hasNotable = notableSection != null && 
                         (notableSection.lines.Count > 0 || 
                          !string.IsNullOrWhiteSpace(data.notableDisplayName) ||
                          data.notableModifierNames.Count > 0);

        // Separator before Notable (if Notable exists)
        if (hasNotable)
        {
            CreateSeparatorLine();
        }

        // Notable section
        if (hasNotable)
        {
            // Notable name
            string notableName = !string.IsNullOrWhiteSpace(data.notableDisplayName) 
                ? data.notableDisplayName 
                : "Notable";
            CreateLine(new WarrantTooltipLine { text = notableName, emphasize = true });

            // Notable modifiers - use notableModifierNames if available, otherwise use section lines
            if (data.notableModifierNames.Count > 0)
            {
                foreach (var modifierName in data.notableModifierNames)
                {
                    if (!string.IsNullOrWhiteSpace(modifierName))
                    {
                        CreateLine(new WarrantTooltipLine { text = modifierName, color = Color.white });
                    }
                }
            }
            else if (notableSection != null && notableSection.lines.Count > 0)
            {
                foreach (var line in notableSection.lines)
                {
                    // Skip the description line if it's just a concatenation of modifier names
                    if (line.text != data.notableDescription || string.IsNullOrWhiteSpace(data.notableDescription))
                    {
                        CreateLine(line);
                    }
                }
            }
        }

        // Separator after Notable (if Notable exists)
        if (hasNotable)
        {
            CreateSeparatorLine();
        }

        // Regular modifiers/affixes section
        if (modifiersSection != null && modifiersSection.lines.Count > 0)
        {
            foreach (var line in modifiersSection.lines)
            {
                CreateLine(line);
            }
        }
        else if (data.sections != null)
        {
            // Fallback: show any other sections that aren't Notable
            foreach (var section in data.sections)
            {
                if (section == null || section == notableSection || section == modifiersSection)
                    continue;

                bool hasHeader = !string.IsNullOrWhiteSpace(section.header);
                if (hasHeader)
                {
                    CreateLine(new WarrantTooltipLine { text = section.header, emphasize = true });
                }

                if (section.lines != null && section.lines.Count > 0)
                {
                    foreach (var line in section.lines)
                    {
                        CreateLine(line);
                    }
                }
            }
        }

        // If no content at all
        if (spawnedLines.Count == 0)
        {
            CreateLine(new WarrantTooltipLine { text = "No modifiers assigned yet." });
        }
    }

    private void CreateSeparatorLine()
    {
        CreateLine(new WarrantTooltipLine { text = "-------------------", color = new Color(0.5f, 0.5f, 0.5f, 0.6f) });
    }

    private void CreateLine(WarrantTooltipLine lineData)
    {
        if (lineContainer == null)
            return;

        var line = Instantiate(lineTemplate, lineContainer);
        line.gameObject.SetActive(true);
        line.text = lineData?.text ?? string.Empty;
        line.fontStyle = lineData?.emphasize == true ? FontStyles.SmallCaps | FontStyles.Bold : FontStyles.Normal;
        line.color = lineData != null ? lineData.color : Color.white;
        line.alpha = lineData?.emphasize == true ? 0.95f : 0.85f;
        spawnedLines.Add(line);
    }

    private void EnsureRuntimeLayout()
    {
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        if (backdropImage == null)
        {
            backdropImage = gameObject.GetComponent<Image>();
            if (backdropImage == null)
            {
                backdropImage = gameObject.AddComponent<Image>();
            }
            backdropImage.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);
        }

        if (lineContainer == null)
        {
            var containerGO = new GameObject("ModifierContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            containerGO.transform.SetParent(transform, false);
            lineContainer = containerGO.GetComponent<RectTransform>();

            var layout = containerGO.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 4f;
            layout.padding = new RectOffset(12, 12, 48, 12);

            var fitter = containerGO.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        if (lineTemplate == null)
        {
            var templateGO = CreateText("LineTemplate", 16);
            templateGO.transform.SetParent(lineContainer, false);
            lineTemplate = templateGO.GetComponent<TextMeshProUGUI>();
            lineTemplate.gameObject.SetActive(false);
        }

        if (titleLabel == null)
        {
            titleLabel = CreateText("Title", 20, FontStyles.Bold);
            titleLabel.transform.SetParent(transform, false);
            titleLabel.rectTransform.anchoredPosition = new Vector2(12, -12);
        }

        if (subtitleLabel == null)
        {
            subtitleLabel = CreateText("Subtitle", 16, FontStyles.Italic);
            subtitleLabel.transform.SetParent(transform, false);
            subtitleLabel.rectTransform.anchoredPosition = new Vector2(12, -34);
        }

        if (iconImage == null)
        {
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGO.transform.SetParent(transform, false);
            iconImage = iconGO.GetComponent<Image>();
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(1, 1);
            iconRect.anchorMax = new Vector2(1, 1);
            iconRect.pivot = new Vector2(1, 1);
            iconRect.sizeDelta = new Vector2(48, 48);
            iconRect.anchoredPosition = new Vector2(-12, -12);
        }
    }

    private TextMeshProUGUI CreateText(string name, int fontSize, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.enableWordWrapping = true;
        text.color = Color.white;
        return text;
    }

    private Color GetRarityColor(WarrantRarity rarity)
    {
        return rarity switch
        {
            WarrantRarity.Magic => magicColor,
            WarrantRarity.Rare => rareColor,
            WarrantRarity.Unique => uniqueColor,
            _ => commonColor
        };
    }
}

