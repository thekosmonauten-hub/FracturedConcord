using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles populating the embossing tooltip prefab with runtime data (embossing info, requirements, effects, mana cost, etc.).
/// </summary>
public class EmbossingTooltipView : MonoBehaviour
{
    [Header("Embossing Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI elementText;
    [SerializeField] private TextMeshProUGUI requirementsText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;

    private void Awake()
    {
        CacheUIElements();
    }

    /// <summary>
    /// Populate the tooltip UI with the provided embossing information.
    /// </summary>
    public void SetData(EmbossingEffect embossing, Character character = null)
    {
        if (embossing == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureUIReferencesCached();

        PopulateHeader(embossing);
        PopulateDescription(embossing);
        PopulateInfo(embossing);
        PopulateRequirements(embossing, character);
        PopulateEffect(embossing);
        PopulateManaCost(embossing);
    }

    private void PopulateHeader(EmbossingEffect embossing)
    {
        if (titleText != null)
        {
            titleText.text = embossing.embossingName;
            titleText.color = embossing.GetRarityColor();
        }

        if (iconImage != null)
        {
            iconImage.sprite = embossing.embossingIcon;
            iconImage.enabled = embossing.embossingIcon != null;
        }
    }

    private void PopulateDescription(EmbossingEffect embossing)
    {
        if (descriptionText != null)
        {
            descriptionText.text = embossing.description;
        }
    }

    private void PopulateInfo(EmbossingEffect embossing)
    {
        if (categoryText != null)
        {
            categoryText.text = $"<b>Category:</b> {embossing.category}";
            categoryText.color = embossing.GetTypeColor();
        }

        if (rarityText != null)
        {
            rarityText.text = $"<b>Rarity:</b> {embossing.rarity}";
            rarityText.color = embossing.GetRarityColor();
        }

        if (elementText != null)
        {
            elementText.text = $"<b>Element:</b> {embossing.elementType}";
        }
    }

    private void PopulateRequirements(EmbossingEffect embossing, Character character)
    {
        if (requirementsText != null)
        {
            if (character != null)
            {
                requirementsText.text = embossing.GetRequirementsTextColored(character);
            }
            else
            {
                requirementsText.text = embossing.GetRequirementsText();
            }
        }
    }

    private void PopulateEffect(EmbossingEffect embossing)
    {
        if (effectText != null)
        {
            effectText.text = embossing.GetEffectDescription();
            effectText.color = new Color(0.5f, 1f, 0.5f); // Light green
        }
    }

    private void PopulateManaCost(EmbossingEffect embossing)
    {
        if (manaCostText != null)
        {
            string costText = $"+{(embossing.manaCostMultiplier * 100):F0}% mana cost";
            if (embossing.flatManaCostIncrease > 0)
            {
                costText += $" +{embossing.flatManaCostIncrease} flat";
            }
            manaCostText.text = costText;
            manaCostText.color = new Color(1f, 0.5f, 0.5f); // Light red
        }
    }

    private void CacheUIElements()
    {
        if (titleText == null)
        {
            var titleObj = transform.Find("Header/TitleText");
            if (titleObj == null) titleObj = transform.Find("TitleText");
            if (titleObj == null) titleObj = transform.Find("Title");
            titleText = titleObj ? titleObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (descriptionText == null)
        {
            var descObj = transform.Find("Content/DescriptionText");
            if (descObj == null) descObj = transform.Find("DescriptionText");
            if (descObj == null) descObj = transform.Find("Description");
            descriptionText = descObj ? descObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (categoryText == null)
        {
            var catObj = transform.Find("Content/CategoryLabel");
            if (catObj == null) catObj = transform.Find("CategoryLabel");
            if (catObj == null) catObj = transform.Find("Category");
            categoryText = catObj ? catObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (rarityText == null)
        {
            var rarityObj = transform.Find("Content/RarityLabel");
            if (rarityObj == null) rarityObj = transform.Find("RarityLabel");
            if (rarityObj == null) rarityObj = transform.Find("Rarity");
            rarityText = rarityObj ? rarityObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (elementText == null)
        {
            var elemObj = transform.Find("Content/ElementLabel");
            if (elemObj == null) elemObj = transform.Find("ElementLabel");
            if (elemObj == null) elemObj = transform.Find("Element");
            elementText = elemObj ? elemObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (requirementsText == null)
        {
            var reqObj = transform.Find("Content/RequirementsText");
            if (reqObj == null) reqObj = transform.Find("RequirementsText");
            if (reqObj == null) reqObj = transform.Find("Requirements");
            requirementsText = reqObj ? reqObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (effectText == null)
        {
            var effectObj = transform.Find("Content/EffectText");
            if (effectObj == null) effectObj = transform.Find("EffectText");
            if (effectObj == null) effectObj = transform.Find("Effect");
            effectText = effectObj ? effectObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (manaCostText == null)
        {
            var manaObj = transform.Find("Content/ManaCostText");
            if (manaObj == null) manaObj = transform.Find("ManaCostText");
            if (manaObj == null) manaObj = transform.Find("ManaCost");
            manaCostText = manaObj ? manaObj.GetComponent<TextMeshProUGUI>() : null;
        }

        if (iconImage == null)
        {
            var iconObj = transform.Find("Header/Icon");
            if (iconObj == null) iconObj = transform.Find("Icon");
            iconImage = iconObj ? iconObj.GetComponent<Image>() : null;
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    private void EnsureUIReferencesCached()
    {
        if (titleText == null || descriptionText == null)
        {
            CacheUIElements();
        }
    }
}

