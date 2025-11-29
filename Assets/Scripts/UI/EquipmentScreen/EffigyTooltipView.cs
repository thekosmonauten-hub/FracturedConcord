using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds effigy data to the effigy tooltip prefab.
/// </summary>
public class EffigyTooltipView : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBackground;
    [SerializeField] private Image iconFrame;

    [Header("Stats")]
    [SerializeField] private List<TextMeshProUGUI> baseStatLabels = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI requirementsLabel;

    [Header("Affixes")]
    [SerializeField] private TextMeshProUGUI implicitLabel;
    [SerializeField] private List<TextMeshProUGUI> explicitLabels = new List<TextMeshProUGUI>();

    private bool cached;

    private void Awake()
    {
        CacheUIElements();
    }

    public void SetData(Effigy effigy)
    {
        if (effigy == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureUIReferencesCached();
        gameObject.SetActive(true);

        ItemRarity rarity = effigy.GetCalculatedRarity();
        string displayName = !string.IsNullOrEmpty(effigy.displayAlias)
            ? effigy.displayAlias
            : (!string.IsNullOrEmpty(effigy.effigyName) ? effigy.effigyName : effigy.GetDisplayName());

        if (nameLabel != null)
        {
            nameLabel.text = TooltipFormattingUtils.ColorizeByRarity(displayName, rarity);
        }

        if (iconImage != null)
        {
            Sprite sprite = effigy.icon != null ? effigy.icon : effigy.itemIcon;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        if (iconFrame != null)
        {
            iconFrame.color = effigy.GetElementColor();
        }

        ApplyBaseStats(effigy);
        ApplyRequirements(effigy);
        ApplyAffixes(implicitLabel, effigy.implicitModifiers);

        var explicitAffixes = new List<Affix>();
        if (effigy.prefixes != null)
            explicitAffixes.AddRange(effigy.prefixes);
        if (effigy.suffixes != null)
            explicitAffixes.AddRange(effigy.suffixes);

        SetAffixTexts(explicitLabels, explicitAffixes);
    }

    private void ApplyBaseStats(Effigy effigy)
    {
        var lines = new List<string>
        {
            $"Element: {effigy.element}",
            $"Shape: {effigy.GetShapeCategory()} ({effigy.shapeWidth}x{effigy.shapeHeight}, {effigy.GetCellCount()} cells)",
            $"Tier: {effigy.sizeTier}"
        };

        if (!string.IsNullOrWhiteSpace(effigy.description))
        {
            lines.Add(effigy.description);
        }

        for (int i = 0; i < baseStatLabels.Count; i++)
        {
            var label = baseStatLabels[i];
            if (label == null)
                continue;

            if (i < lines.Count)
            {
                label.text = lines[i];
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }
        }
    }

    private void ApplyRequirements(Effigy effigy)
    {
        if (requirementsLabel == null)
            return;

        string text = TooltipFormattingUtils.FormatRequirements(effigy.requiredLevel, 0, 0, 0);
        requirementsLabel.text = $"Requirements:\n{text}";
        requirementsLabel.gameObject.SetActive(true);
    }

    private void ApplyAffixes(TextMeshProUGUI target, List<Affix> affixes)
    {
        if (target == null)
            return;

        string text = BuildAffixBlock(affixes);
        bool hasAffix = !string.IsNullOrEmpty(text);
        target.text = hasAffix ? text : "None";
        target.gameObject.SetActive(true);
    }

    private string BuildAffixBlock(List<Affix> affixes)
    {
        if (affixes == null || affixes.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        for (int i = 0; i < affixes.Count; i++)
        {
            string formatted = TooltipFormattingUtils.FormatAffix(affixes[i]);
            if (string.IsNullOrEmpty(formatted))
                continue;

            if (builder.Length > 0)
                builder.Append('\n');

            builder.Append(formatted);
        }

        return builder.ToString();
    }

    private void SetAffixTexts(IEnumerable<TextMeshProUGUI> targets, IList<Affix> affixes)
    {
        if (targets == null)
            return;

        int index = 0;
        foreach (var label in targets)
        {
            if (label == null)
                continue;

            if (affixes != null && index < affixes.Count)
            {
                string text = TooltipFormattingUtils.FormatAffix(affixes[index]);
                label.text = string.IsNullOrEmpty(text) ? "—" : text;
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }

            index++;
        }
    }

    private void CacheUIElements()
    {
        if (cached)
            return;

        cached = true;

        nameLabel ??= FindLabel("Header/EffigyName");
        iconImage ??= FindImage("Content/IconContainer/WeaponIcon");
        iconBackground ??= FindImage("Content/IconContainer/IconBackground");
        iconFrame ??= FindImage("Content/IconContainer/ImageFrame");

        var statsContainer = transform.Find("Content/BaseWeaponStats");
        baseStatLabels ??= new List<TextMeshProUGUI>();
        baseStatLabels.Clear();

        if (statsContainer != null)
        {
            foreach (Transform child in statsContainer)
            {
                var label = child.GetComponent<TextMeshProUGUI>();
                if (label == null)
                    continue;

                if (child.name == "Requirements")
                {
                    requirementsLabel ??= label;
                }
                else
                {
                    baseStatLabels.Add(label);
                }
            }
        }

        implicitLabel ??= FindLabel("Content/AffixContainer/Implicit/ImplicitModifider");

        if (explicitLabels == null || explicitLabels.Count == 0)
        {
            explicitLabels = CollectChildLabels("Content/AffixContainer/Affixes", "Affix");
        }
    }

    private void EnsureUIReferencesCached()
    {
        if (!cached)
        {
            CacheUIElements();
        }
    }

    private TextMeshProUGUI FindLabel(string path)
    {
        Transform target = transform.Find(path);
        return target ? target.GetComponent<TextMeshProUGUI>() : null;
    }

    private Image FindImage(string path)
    {
        Transform target = transform.Find(path);
        return target ? target.GetComponent<Image>() : null;
    }

    private List<TextMeshProUGUI> CollectChildLabels(string path, string namePrefix = null, int startIndex = 0)
    {
        var list = new List<TextMeshProUGUI>();
        Transform parent = transform.Find(path);
        if (parent == null)
            return list;

        int currentIndex = 0;
        foreach (Transform child in parent)
        {
            if (!string.IsNullOrEmpty(namePrefix) && !child.name.StartsWith(namePrefix))
                continue;

            if (currentIndex++ < startIndex)
                continue;

            var label = child.GetComponent<TextMeshProUGUI>();
            if (label != null)
                list.Add(label);
        }

        return list;
    }
}


