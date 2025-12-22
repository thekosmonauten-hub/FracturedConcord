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
    [SerializeField] private TextMeshProUGUI cardNameLabel;
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
        // Populate card name separately
        if (cardNameLabel != null)
        {
            cardNameLabel.text = card.cardName;
        }
        
        if (cardDetailsLabel != null)
        {
            string manaDisplay;
            string manaBreakdown;

            if (card.sourceCardData != null)
            {
                manaDisplay = card.sourceCardData.GetManaCostDisplay(card, character);
                manaBreakdown = card.sourceCardData.GetManaCostBreakdown(card, character);
            }
            else
            {
                manaDisplay = card.GetManaCostDisplay(character);
                if (EmbossingDatabase.Instance != null)
                {
                    // For Skill cards, calculate base cost first
                    int baseCost = card.manaCost;
                    if (card.cardType == CardType.Skill && character != null)
                    {
                        float percentageCost = baseCost / 100.0f;
                        baseCost = Mathf.RoundToInt(character.maxMana * percentageCost);
                    }
                    manaBreakdown = EmbossingDatabase.Instance.GetManaCostBreakdown(card, baseCost);
                }
                else
                {
                    manaBreakdown = $"Mana Cost: {card.GetCurrentManaCost(character)}";
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
            
            // Add damage breakdown if card has embossings
            if (card.cardType == CardType.Attack && card.baseDamage > 0 && 
                card.appliedEmbossings != null && card.appliedEmbossings.Count > 0)
            {
                var breakdown = CardStatCalculator.CalculateCardDamage(card, character);
                if (breakdown.embossingFlatBonus > 0 || breakdown.embossingMultiplier > 0 || breakdown.embossingAddedDamage.Count > 0)
                {
                    // Extract breakdown from description if it exists, or add it
                    if (!dynamicDescription.Contains("Damage Breakdown:"))
                    {
                        dynamicDescription += $"\n\n<color=cyan>Damage Breakdown:</color>\n{breakdown.breakdownText}";
                    }
                }
            }

            int filled = card.GetFilledSlotCount();
            string embossingInfo = $"Embossings: {filled}/{card.embossingSlots}";
            
            // Build card tags string
            string tagsText = string.Empty;
            if (card.tags != null && card.tags.Count > 0)
            {
                tagsText = $"Tags: {string.Join(", ", card.tags)}";
            }

            cardDetailsLabel.text =
                $"Type: {card.cardType}\n" +
                $"Mana: {manaDisplay}\n" +
                $"{manaBreakdown}\n" +
                $"{embossingInfo}";
            
            // Add tags if present
            if (!string.IsNullOrEmpty(tagsText))
            {
                cardDetailsLabel.text += $"\n{tagsText}";
            }
            
            // Add description
            if (!string.IsNullOrEmpty(dynamicDescription))
            {
                cardDetailsLabel.text += $"\n\n{dynamicDescription}";
            }
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
            
            // Ensure database exists
            if (EmbossingDatabase.Instance == null)
            {
                EmbossingDatabase.EnsureInstance();
            }
            
            EmbossingEffect effect = EmbossingDatabase.Instance != null
                ? EmbossingDatabase.Instance.GetEmbossing(instance.embossingId)
                : null;

            if (effect == null)
            {
                Debug.LogWarning($"[CardTooltipView] Embossing not found in database: '{instance.embossingId}' for card '{card.cardName}'. Instance is {(EmbossingDatabase.Instance == null ? "NULL" : "EXISTS")}");
            }

            if (row.Icon != null)
            {
                if (effect != null && effect.embossingIcon != null)
                {
                    row.Icon.sprite = effect.embossingIcon;
                    row.Icon.color = effect.embossingColor;
                    row.Icon.enabled = true;
                }
                else
                {
                    row.Icon.sprite = null;
                    row.Icon.enabled = false;
                    if (effect != null)
                    {
                        Debug.LogWarning($"[CardTooltipView] Embossing '{effect.embossingName}' (ID: {instance.embossingId}) has no icon assigned");
                    }
                }
            }

            if (row.Description != null)
            {
                string title = effect != null ? effect.embossingName : instance.embossingId;
                string effectDescription = "Unknown effect";
                
                if (effect != null)
                {
                    string desc = effect.GetEffectDescription();
                    if (!string.IsNullOrEmpty(desc))
                    {
                        effectDescription = desc;
                    }
                    else if (!string.IsNullOrEmpty(effect.description))
                    {
                        // Fallback to description field if GetEffectDescription() returns empty
                        effectDescription = effect.description;
                    }
                    else
                    {
                        Debug.LogWarning($"[CardTooltipView] Embossing '{effect.embossingName}' (ID: {instance.embossingId}) has no description");
                    }
                }

                row.Description.text =
                    $"{title} (Lv {instance.level})\n" +
                    $"{effectDescription}";
            }
        }
    }

    private void CacheUIElements()
    {
        if (cardNameLabel == null)
        {
            // Try multiple possible paths for card name
            var namePaths = new[]
            {
                "Header/CardName",
                "CardName",
                "Header/Title",
                "Title"
            };
            
            foreach (var path in namePaths)
            {
                var nameObj = transform.Find(path);
                if (nameObj != null)
                {
                    cardNameLabel = nameObj.GetComponent<TextMeshProUGUI>();
                    if (cardNameLabel != null)
                        break;
                }
            }
        }
        
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
        if (cardNameLabel == null || cardDetailsLabel == null || cardIconImage == null || embossingRows.Count == 0)
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


