using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles populating the card tooltip prefab with runtime data (card stats, embossings, etc.).
/// </summary>
public class CardTooltipView : MonoBehaviour
{
    [Header("Card Elements")]
    [SerializeField] private TextMeshProUGUI cardDetailsLabel;
    [SerializeField] private Image cardIconImage;
    [SerializeField] private Transform embossingContainer;

    private readonly List<EmbossingRow> embossingRows = new List<EmbossingRow>();

    private void Awake()
    {
        CacheUIElements();
    }

    /// <summary>
    /// Populate the tooltip UI with the provided card information.
    /// </summary>
    public void SetData(Card card, Character character)
    {
        if (card == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureUIReferencesCached();

        PopulateHeader(card, character);
        PopulateEmbossings(card, character);
    }

    private void PopulateHeader(Card card, Character character)
    {
        if (cardDetailsLabel != null)
        {
            string manaDisplay;
            string manaBreakdown;

            if (card.sourceCardData != null)
            {
                manaDisplay = card.sourceCardData.GetManaCostDisplay(card);
                manaBreakdown = card.sourceCardData.GetManaCostBreakdown(card);
            }
            else
            {
                manaDisplay = card.GetManaCostDisplay();
                if (EmbossingDatabase.Instance != null)
                {
                    manaBreakdown = EmbossingDatabase.Instance.GetManaCostBreakdown(card, card.manaCost);
                }
                else
                {
                    manaBreakdown = $"Mana Cost: {card.GetCurrentManaCost()}";
                }
            }

            string dynamicDescription = string.Empty;
            if (card.sourceCardData != null)
            {
                dynamicDescription = card.sourceCardData.GetDynamicDescription(character);
            }
            else if (!string.IsNullOrWhiteSpace(card.description))
            {
                dynamicDescription = card.GetDynamicDescription(character);
            }

            int filled = card.GetFilledSlotCount();
            string embossingInfo = $"Embossings: {filled}/{card.embossingSlots}";

            cardDetailsLabel.text =
                $"{card.cardName}\n" +
                $"Type: {card.cardType}\n" +
                $"Mana: {manaDisplay}\n" +
                $"{manaBreakdown}\n" +
                $"{embossingInfo}\n\n" +
                $"{dynamicDescription}";
        }

        if (cardIconImage != null)
        {
            cardIconImage.sprite = card.cardArt;
            cardIconImage.enabled = card.cardArt != null;
        }
    }

    private void PopulateEmbossings(Card card, Character character)
    {
        List<EmbossingInstance> embossings = card.appliedEmbossings ?? new List<EmbossingInstance>();

        for (int i = 0; i < embossingRows.Count; i++)
        {
            bool hasData = i < embossings.Count;
            var row = embossingRows[i];
            if (row.Root != null)
            {
                row.Root.SetActive(hasData);
            }

            if (!hasData)
            {
                continue;
            }

            EmbossingInstance instance = embossings[i];
            EmbossingEffect effect = EmbossingDatabase.Instance != null
                ? EmbossingDatabase.Instance.GetEmbossing(instance.embossingId)
                : null;

            if (row.Icon != null)
            {
                row.Icon.sprite = effect?.embossingIcon;
                row.Icon.color = effect != null ? effect.embossingColor : Color.white;
                row.Icon.enabled = row.Icon.sprite != null;
            }

            if (row.Description != null)
            {
                string title = effect != null ? effect.embossingName : instance.embossingId;
                string effectDescription = effect != null ? effect.GetEffectDescription() : "Unknown effect";
                string requirements = effect != null ? effect.GetRequirementsTextColored(character) : string.Empty;

                row.Description.text =
                    $"{title} (Lv {instance.level})\n" +
                    $"{effectDescription}\n" +
                    $"{requirements}".TrimEnd();
            }
        }
    }

    private void CacheUIElements()
    {
        if (cardDetailsLabel == null)
        {
            var header = transform.Find("Header/CardDetails");
            cardDetailsLabel = header ? header.GetComponent<TextMeshProUGUI>() : null;
        }

        if (cardIconImage == null)
        {
            var icon = transform.Find("Content/CardContainer/CardIcon");
            cardIconImage = icon ? icon.GetComponent<Image>() : null;
        }

        if (embossingContainer == null)
        {
            var container = transform.Find("Content/EmbossingContainer");
            embossingContainer = container;
        }

        embossingRows.Clear();
        if (embossingContainer != null)
        {
            foreach (Transform child in embossingContainer)
            {
                var icon = child.Find("EmbossingIcon")?.GetComponent<Image>();
                var description = child.Find("EmbossingDescription")?.GetComponent<TextMeshProUGUI>();
                embossingRows.Add(new EmbossingRow(child.gameObject, icon, description));
            }
        }
    }

    private void EnsureUIReferencesCached()
    {
        if (cardDetailsLabel == null || cardIconImage == null || embossingRows.Count == 0)
        {
            CacheUIElements();
        }
    }

    private readonly struct EmbossingRow
    {
        public GameObject Root { get; }
        public Image Icon { get; }
        public TextMeshProUGUI Description { get; }

        public EmbossingRow(GameObject root, Image icon, TextMeshProUGUI description)
        {
            Root = root;
            Icon = icon;
            Description = description;
        }
    }
}


