using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays CardData (ScriptableObject) on a card prefab.
/// Simpler version for the CardData system.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardDataVisualizer : MonoBehaviour
{
	[Header("Card Elements")]
	[SerializeField] private Text cardNameText;
	[SerializeField] private Text cardCostText;
	[SerializeField] private Text cardDescriptionText;
	[SerializeField] private Text cardValueText; // Damage or Block
	[SerializeField] private Text cardTypeText;
	[SerializeField] private Image cardBackground;
	[SerializeField] private Image cardBorder;
	[SerializeField] private Image rarityGlow;
	[SerializeField] private Image categoryIcon; // NEW: category symbol
	[SerializeField] private CardVisualAssets visualAssets; // NEW: reference to sprites
	
	private CardData currentCardData;
	
	/// <summary>
	/// Set card data and update visuals
	/// </summary>
	public void SetCardData(CardData cardData)
	{
		currentCardData = cardData;
		UpdateVisuals();
	}
	
	/// <summary>
	/// Update all visual elements
	/// </summary>
	public void UpdateVisuals()
	{
		if (currentCardData == null) return;
		
		// Update texts
		if (cardNameText != null)
			cardNameText.text = currentCardData.cardName;
		
		if (cardCostText != null)
			cardCostText.text = currentCardData.playCost.ToString();
		
		if (cardTypeText != null)
			cardTypeText.text = currentCardData.category.ToString();
		
		if (cardDescriptionText != null)
			cardDescriptionText.text = currentCardData.description;
		
		// Update damage/block value
		if (cardValueText != null)
		{
			if (currentCardData.damage > 0)
			{
				cardValueText.text = currentCardData.damage.ToString();
				cardValueText.color = GetDamageColor();
			}
			else if (currentCardData.block > 0)
			{
				cardValueText.text = currentCardData.block.ToString();
				cardValueText.color = Color.cyan;
			}
			else
			{
				cardValueText.text = "";
			}
		}
		
		// Update colors
		UpdateColors();
		
		// Update category icon
		UpdateCategoryIcon();
	}
	
	/// <summary>
	/// Update card colors based on category and element
	/// </summary>
	private void UpdateColors()
	{
		Color backgroundColor = GetCategoryColor(currentCardData.category);
		Color borderColor = GetElementColor(currentCardData.element);
		Color glowColor = GetRarityColor(currentCardData.rarity);
		
		if (cardBackground != null)
			cardBackground.color = backgroundColor;
		
		if (cardBorder != null)
			cardBorder.color = borderColor;
		
		if (rarityGlow != null)
		{
			rarityGlow.color = glowColor;
			rarityGlow.enabled = currentCardData.rarity != CardRarity.Common;
		}
	}
	
	private void UpdateCategoryIcon()
	{
		if (categoryIcon == null || visualAssets == null) return;
		Sprite sprite = null;
		switch (currentCardData.category)
		{
			case CardCategory.Attack: sprite = visualAssets.attackIcon; break;
			case CardCategory.Guard: sprite = visualAssets.guardIcon; break;
			case CardCategory.Skill: sprite = visualAssets.skillIcon; break;
			case CardCategory.Power: sprite = visualAssets.powerIcon; break;
			default: sprite = null; break;
		}
		categoryIcon.sprite = sprite;
		categoryIcon.enabled = sprite != null;
	}
	
	private Color GetCategoryColor(CardCategory category)
	{
		switch (category)
		{
			case CardCategory.Attack:
				return new Color(0.8f, 0.3f, 0.3f, 0.9f); // Red
			case CardCategory.Guard:
				return new Color(0.3f, 0.6f, 0.8f, 0.9f); // Blue
			case CardCategory.Skill:
				return new Color(0.3f, 0.8f, 0.3f, 0.9f); // Green
			case CardCategory.Power:
				return new Color(0.8f, 0.6f, 0.2f, 0.9f); // Gold
			default:
				return new Color(0.5f, 0.5f, 0.5f, 0.9f); // Gray
		}
	}
	
	private Color GetElementColor(CardElement element)
	{
		switch (element)
		{
			case CardElement.Fire:
				return new Color(1f, 0.4f, 1f, 1f); // Orange
			case CardElement.Cold:
				return new Color(0.3f, 0.7f, 1f, 1f); // Light Blue
			case CardElement.Lightning:
				return new Color(0.7f, 0.7f, 1f, 1f); // Purple-Blue
			case CardElement.Physical:
				return new Color(0.7f, 0.7f, 0.7f, 1f); // Gray
			case CardElement.Chaos:
				return new Color(0.8f, 0.2f, 0.8f, 1f); // Magenta
			default:
				return Color.white;
		}
	}
	
	private Color GetRarityColor(CardRarity rarity)
	{
		switch (rarity)
		{
			case CardRarity.Common:
				return new Color(0.7f, 0.7f, 0.7f, 0f); // Transparent
			case CardRarity.Magic:
				return new Color(0.3f, 0.5f, 1f, 0.3f); // Blue glow
			case CardRarity.Rare:
				return new Color(1f, 0.8f, 0.2f, 0.3f); // Gold glow
			case CardRarity.Unique:
				return new Color(1f, 0.5f, 0f, 0.4f); // Orange glow
			default:
				return new Color(0, 0, 0, 0);
		}
	}
	
	private Color GetDamageColor()
	{
		switch (currentCardData.element)
		{
			case CardElement.Fire:
				return new Color(1f, 0.5f, 0f);
			case CardElement.Cold:
				return new Color(0.5f, 0.8f, 1f);
			case CardElement.Lightning:
				return new Color(0.8f, 0.8f, 1f);
			default:
				return new Color(1f, 0.3f, 0.3f); // Red for physical
		}
	}
	
	/// <summary>
	/// Auto-find UI elements if not assigned
	/// </summary>
	private void Awake()
	{
		if (cardNameText == null)
			cardNameText = transform.Find("CardName")?.GetComponent<Text>();
		
		if (cardCostText == null)
			cardCostText = transform.Find("Cost")?.GetComponent<Text>();
		
		if (cardDescriptionText == null)
			cardDescriptionText = transform.Find("Description")?.GetComponent<Text>();
		
		if (cardValueText == null)
			cardValueText = transform.Find("Value")?.GetComponent<Text>();
		
		if (cardTypeText == null)
			cardTypeText = transform.Find("Type")?.GetComponent<Text>();
		
		if (cardBackground == null)
			cardBackground = transform.Find("Background")?.GetComponent<Image>();
		
		if (cardBorder == null)
			cardBorder = GetComponent<Image>();
		
		if (rarityGlow == null)
			rarityGlow = transform.Find("RarityGlow")?.GetComponent<Image>();
		
		if (categoryIcon == null)
			categoryIcon = transform.Find("CategoryIcon")?.GetComponent<Image>();
	}
	
	/// <summary>
	/// Get the current card data
	/// </summary>
	public CardData GetCardData()
	{
		return currentCardData;
	}
}

