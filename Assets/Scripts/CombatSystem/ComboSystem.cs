using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages card combo detection and visual indicators
/// </summary>
public class ComboSystem : MonoBehaviour
{
	public static ComboSystem Instance { get; private set; }
	
	[Header("Combo Settings")]
	public float comboHighlightDuration = 2f;
	public Color comboHighlightColor = Color.yellow;
	public float comboIndicatorScale = 1.2f;
	
	[Header("Combo History")]
	public int maxComboHistory = 5; // Remember last 5 cards played
	
	// Events
	public System.Action<Card, Card> OnComboDetected;
	public System.Action<Card> OnComboHighlight;
	
	// Combo state
	private List<Card> comboHistory = new List<Card>();
	private List<Card> highlightedCards = new List<Card>();
	
	// New: Track last played info for asset-driven combos
	private CardType? lastPlayedCardType = null;
	private string lastPlayedCardName = null;
	private string lastPlayedGroupKey = null;
	
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
	}
	
	/// <summary>
	/// Register last played card type/name (call after successfully playing a card)
	/// </summary>
	public void RegisterLastPlayed(CardType cardType, string cardName, string groupKey = null)
	{
		lastPlayedCardType = cardType;
		lastPlayedCardName = cardName;
		lastPlayedGroupKey = groupKey;
	}
	
	public CardType? GetLastPlayedType()
	{
		return lastPlayedCardType;
	}
	
	public string GetLastPlayedName()
	{
		return lastPlayedCardName;
	}

	public string GetLastPlayedGroupKey()
	{
		return lastPlayedGroupKey;
	}
	
	/// <summary>
	/// Build a combo application from the current CardDataExtended and player state.
	/// Returns null if no combo should trigger given the last played card info.
	/// </summary>
	public ComboApplication BuildComboApplication(CardDataExtended cardData, Character player)
	{
		if (cardData == null) return null;
		// Respect per-card combo toggle
		if (!cardData.enableCombo) return null;
		
		// Determine if combo triggers
		bool triggers = false;
		if (lastPlayedCardType.HasValue)
		{
			// Prefer explicit dropdown type
			if (cardData.comboWithCardType == lastPlayedCardType.Value)
			{
				triggers = true;
			}
			else if (!string.IsNullOrEmpty(cardData.comboWith))
			{
				// Fallback: free text matching
				if (cardData.comboWith.IndexOf(lastPlayedCardType.Value.ToString(), System.StringComparison.OrdinalIgnoreCase) >= 0)
					triggers = true;
				if (!triggers && !string.IsNullOrEmpty(lastPlayedCardName) && cardData.comboWith.IndexOf(lastPlayedCardName, System.StringComparison.OrdinalIgnoreCase) >= 0)
					triggers = true;
				if (!triggers && !string.IsNullOrEmpty(lastPlayedGroupKey) && cardData.comboWith.IndexOf(lastPlayedGroupKey, System.StringComparison.OrdinalIgnoreCase) >= 0)
					triggers = true;
			}
		}
		if (!triggers) return null;
		
		// Compute scaling bonus
		float scalingBonus = 0f;
		switch (cardData.comboScaling)
		{
			case ComboScalingType.Strength:
				scalingBonus = player != null ? player.strength : 0f; break;
			case ComboScalingType.Dexterity:
				scalingBonus = player != null ? player.dexterity : 0f; break;
			case ComboScalingType.Intelligence:
				scalingBonus = player != null ? player.intelligence : 0f; break;
			case ComboScalingType.Momentum:
				// TODO: hook into momentum system if available
				scalingBonus = 0f; break;
			case ComboScalingType.DiscardPower:
				// TODO: hook into discard power system if available
				scalingBonus = 0f; break;
		}
		if (cardData.comboScalingDivisor != 0f)
			scalingBonus = scalingBonus / cardData.comboScalingDivisor;
		
		// Build application
		var app = new ComboApplication
		{
			logic = cardData.comboLogic,
			attackIncrease = cardData.comboAttackIncrease + scalingBonus,
			guardIncrease = cardData.comboGuardIncrease + ((cardData.comboScaling == ComboScalingType.Strength && cardData.comboScalingDivisor > 0) ? scalingBonus : 0f),
			isAoEOverride = cardData.comboIsAoE,
			manaRefund = Mathf.Max(0, cardData.comboManaRefund),
			ailmentId = string.IsNullOrEmpty(cardData.comboApplyAilment) ? null : cardData.comboApplyAilment,
			buffIds = cardData.comboBuffs != null ? new List<string>(cardData.comboBuffs) : new List<string>(),
			comboDescription = cardData.comboDescription,
			comboAilmentId = cardData.comboAilment,
			comboAilmentPortion = cardData.comboAilmentPortion,
			comboAilmentDuration = cardData.comboAilmentDuration,
			comboDrawCards = Mathf.Max(0, cardData.comboDrawCards)
		};
		return app;
	}
	
	public class ComboApplication
	{
		public ComboLogicType logic;
		public float attackIncrease;
		public float guardIncrease;
		public bool isAoEOverride;
		public int manaRefund;
		public string ailmentId; // legacy string id
		public List<string> buffIds;
		public string comboDescription;
		
		// New: structured ailment fields
		public AilmentId comboAilmentId = AilmentId.None;
		public float comboAilmentPortion = 0f;
		public int comboAilmentDuration = 0;
			public int comboDrawCards = 0;
	}
	
	/// <summary>
	/// Called when a card is played to check for combos (legacy Card flow)
	/// </summary>
	public void OnCardPlayed(Card playedCard)
	{
		if (playedCard == null) return;
		
		// Add to combo history
		comboHistory.Add(playedCard);
		
		// Trim history if too long
		if (comboHistory.Count > maxComboHistory)
		{
			comboHistory.RemoveAt(0);
		}
		
		// Check for combos with the previously played card
		if (comboHistory.Count >= 2)
		{
			Card previousCard = comboHistory[comboHistory.Count - 2];
			CheckForCombo(previousCard, playedCard);
		}
		
		// Check for combos with other cards in history
		CheckForDelayedCombos(playedCard);
		
		Debug.Log($"<color=cyan>Combo System: Card {playedCard.cardName} played. History: {comboHistory.Count} cards</color>");
	}
	
	/// <summary>
	/// Check if two cards can combo
	/// </summary>
	private void CheckForCombo(Card card1, Card card2)
	{
		if (CanCombo(card1, card2))
		{
			Debug.Log($"<color=yellow>COMBO DETECTED: {card1.cardName} + {card2.cardName}</color>");
			
			// Trigger combo effect
			ProcessComboEffect(card1, card2);
			
			// Highlight the cards
			HighlightComboCards(card1, card2);
			
			// Trigger event
			OnComboDetected?.Invoke(card1, card2);
		}
	}
	
	/// <summary>
	/// Check if two cards can combo based on their combo properties
	/// </summary>
	private bool CanCombo(Card card1, Card card2)
	{
		// Check if card2 has combo properties
		if (string.IsNullOrEmpty(card2.comboWith)) return false;
		
		// Check for specific card combo
		if (card2.comboWith.Contains(card1.cardName))
		{
			return true;
		}
		
		// Check for type-based combo
		if (card2.comboWith.Contains(card1.cardType.ToString()))
		{
			return true;
		}
		
		// Check for tag-based combo
		if (card2.comboWith.Contains("Tag"))
		{
			// Check if cards share any tags
			return HasSharedTags(card1, card2);
		}
		
		return false;
	}
	
	private bool HasSharedTags(Card card1, Card card2)
	{
		if (card1.tags == null || card2.tags == null) return false;
		
		foreach (string tag1 in card1.tags)
		{
			foreach (string tag2 in card2.tags)
			{
				if (tag1 == tag2)
				{
					return true;
				}
			}
		}
		
		return false;
	}
	
	private void ProcessComboEffect(Card card1, Card card2)
	{
		// Legacy placeholder
		if (!string.IsNullOrEmpty(card2.comboEffect))
		{
			Debug.Log($"<color=green>Applying combo effect: {card2.comboEffect}</color>");
			ApplyComboEffect(card1, card2, card2.comboEffect);
		}
	}
	
	private void ApplyComboEffect(Card card1, Card card2, string effectString)
	{
		if (effectString.Contains("damage"))
		{
			Debug.Log("Combo effect: Increased damage!");
		}
		else if (effectString.Contains("draw"))
		{
			Debug.Log("Combo effect: Draw extra cards!");
		}
		else if (effectString.Contains("mana"))
		{
			Debug.Log("Combo effect: Gain extra mana!");
		}
		else if (effectString.Contains("guard"))
		{
			Debug.Log("Combo effect: Gain guard!");
		}
	}
	
	private void HighlightComboCards(Card card1, Card card2)
	{
		GameObject card1Visual = FindCardVisual(card1);
		GameObject card2Visual = FindCardVisual(card2);
		
		if (card1Visual != null)
		{
			HighlightCard(card1Visual);
		}
		
		if (card2Visual != null)
		{
			HighlightCard(card2Visual);
		}
	}
	
	private GameObject FindCardVisual(Card card)
	{
		CombatDeckManager deckManager = CombatDeckManager.Instance;
		if (deckManager != null)
		{
			var handVisuals = deckManager.GetHandVisuals();
			if (handVisuals != null)
			{
				foreach (GameObject visual in handVisuals)
				{
					if (visual != null)
					{
						return visual;
					}
				}
			}
		}
		
		return null;
	}
	
	private void HighlightCard(GameObject cardVisual)
	{
		if (cardVisual == null) return;
		
		if (!highlightedCards.Contains(GetCardFromVisual(cardVisual)))
		{
			highlightedCards.Add(GetCardFromVisual(cardVisual));
		}
		
		StartCoroutine(HighlightCardCoroutine(cardVisual));
		
		Card card = GetCardFromVisual(cardVisual);
		if (card != null)
		{
			OnComboHighlight?.Invoke(card);
		}
	}
	
	private Card GetCardFromVisual(GameObject cardVisual)
	{
		return null;
	}
	
	private System.Collections.IEnumerator HighlightCardCoroutine(GameObject cardVisual)
	{
		if (cardVisual == null) yield break;
		
		Vector3 originalScale = cardVisual.transform.localScale;
		cardVisual.transform.localScale = originalScale * comboIndicatorScale;
		yield return new WaitForSeconds(comboHighlightDuration);
		cardVisual.transform.localScale = originalScale;
	}
	
	private void CheckForDelayedCombos(Card playedCard)
	{
		for (int i = 0; i < comboHistory.Count - 1; i++)
		{
			Card historyCard = comboHistory[i];
			if (CanCombo(historyCard, playedCard))
			{
				Debug.Log($"<color=yellow>DELAYED COMBO DETECTED: {historyCard.cardName} + {playedCard.cardName}</color>");
				ProcessComboEffect(historyCard, playedCard);
				HighlightComboCards(historyCard, playedCard);
				OnComboDetected?.Invoke(historyCard, playedCard);
			}
		}
	}
	
	public bool CanComboWithLastPlayed(Card handCard)
	{
		if (comboHistory.Count == 0 || handCard == null) return false;
		
		Card lastPlayed = comboHistory[comboHistory.Count - 1];
		return CanCombo(lastPlayed, handCard);
	}
	
	public List<Card> GetComboableCards(List<Card> hand)
	{
		List<Card> comboableCards = new List<Card>();
		
		foreach (Card card in hand)
		{
			if (CanComboWithLastPlayed(card))
			{
				comboableCards.Add(card);
			}
		}
		
		return comboableCards;
	}
	
	public void ClearComboHistory()
	{
		comboHistory.Clear();
		highlightedCards.Clear();
		lastPlayedCardType = null;
		lastPlayedCardName = null;
		lastPlayedGroupKey = null;
		Debug.Log("Combo history cleared");
	}
	
	public List<Card> GetComboHistory()
	{
		return new List<Card>(comboHistory);
	}
	
	public Card GetLastPlayedCard()
	{
		return comboHistory.Count > 0 ? comboHistory[comboHistory.Count - 1] : null;
	}
	
	public bool HasComboPotential(Card card)
	{
		return !string.IsNullOrEmpty(card.comboWith);
	}
	
	public string GetComboDescription(Card card)
	{
		if (string.IsNullOrEmpty(card.comboDescription)) return "";
		return card.comboDescription;
	}
	
	[ContextMenu("Test Combo System")]
	public void TestComboSystem()
	{
		Debug.Log("=== COMBO SYSTEM TEST ===");
		Debug.Log($"Combo history count: {comboHistory.Count}");
		Debug.Log($"Highlighted cards count: {highlightedCards.Count}");
		
		if (comboHistory.Count > 0)
		{
			Debug.Log($"Last played card: {comboHistory[comboHistory.Count - 1].cardName}");
		}
		else
		{
			Debug.Log("No cards in combo history");
		}
	}
	
	[ContextMenu("Clear Combo History")]
	public void DebugClearComboHistory()
	{
		ClearComboHistory();
		Debug.Log("Combo history cleared via debug method");
	}
	
	[ContextMenu("Show Combo History")]
	public void DebugShowComboHistory()
	{
		Debug.Log("=== COMBO HISTORY ===");
		for (int i = 0; i < comboHistory.Count; i++)
		{
			Card card = comboHistory[i];
			Debug.Log($"{i + 1}. {card.cardName} (Combo: {card.comboWith})");
		}
	}
}
