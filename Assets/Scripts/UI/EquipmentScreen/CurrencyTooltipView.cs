using Dexiled.Data.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyTooltipView : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBackground;
    [SerializeField] private Image iconFrame;

    [Header("Details")]
    [SerializeField] private TextMeshProUGUI descriptionLabel;

    private bool cached;

    private void Awake()
    {
        CacheReferences();
    }

    public void SetData(CurrencyData currency)
    {
        if (currency == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureCached();
        gameObject.SetActive(true);

        if (nameLabel != null)
        {
            nameLabel.text = TooltipFormattingUtils.ColorizeByRarity(
                string.IsNullOrWhiteSpace(currency.currencyName) ? currency.currencyType.ToString() : currency.currencyName,
                currency.rarity);
        }

        if (iconImage != null)
        {
            iconImage.sprite = currency.currencySprite;
            iconImage.enabled = currency.currencySprite != null;
        }

        if (iconFrame != null)
        {
            if (ColorUtility.TryParseHtmlString(ItemRarityCalculator.GetRarityColor(currency.rarity), out var color))
            {
                iconFrame.color = color;
            }
        }

        if (descriptionLabel != null)
        {
            descriptionLabel.text = string.IsNullOrWhiteSpace(currency.description)
                ? "No description available."
                : currency.description;
        }

    }

    private void EnsureCached()
    {
        if (!cached)
        {
            CacheReferences();
        }
    }

    private void CacheReferences()
    {
        if (cached) return;
        cached = true;

        nameLabel ??= FindLabel("Header/CurrencyName");
        descriptionLabel ??= FindLabel("Content/CurrencyDetailsSection/CurrencyDescription");

        iconImage ??= FindImage("Content/CardContainer/CardIcon");
        iconBackground ??= FindImage("Content/CardContainer/IconBackground");
        iconFrame ??= FindImage("Content/CardContainer/ImageFrame");
    }

    private TextMeshProUGUI FindLabel(string path)
    {
        Transform t = transform.Find(path);
        return t ? t.GetComponent<TextMeshProUGUI>() : null;
    }

    private Image FindImage(string path)
    {
        Transform t = transform.Find(path);
        return t ? t.GetComponent<Image>() : null;
    }
}

