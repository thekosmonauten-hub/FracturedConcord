using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles card hover animations using CombatAnimationManager.
/// Attach to card prefabs to enable hover effects.
/// Also raises hovered card to the top layer while hovering for visual clarity.
/// </summary>
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Ordering")]
    [SerializeField] private bool raiseBySibling = false; // disable to avoid layout cycling
    [SerializeField] private bool raiseByCanvas = true;   // prefer canvas sorting for hover
    [SerializeField] private bool stayWithinMask = false;  // when false, allows canvas override for hover (set true only if mask clipping required)
	[HideInInspector]
	public CombatAnimationManager animationManager;
	
	private bool isHovering = false;
	private Vector3 originalPosition;
	private bool hasStoredPosition = false;
	private Vector3 baseScale; // Base scale of the card (from CardRuntimeManager)
	private bool hasStoredScale = false;
	
	// Render order handling
	private int originalSiblingIndex = -1;
	private Canvas cardCanvas;
	private bool hadCanvas = false;
	private bool originalOverrideSorting = false;
	private int originalSortingOrder = 0;
	private const int HoverSortingOrder = 5000; // large number to ensure on-top within canvas layer
    // We keep any created canvas rather than adding/removing to avoid pointer flicker
		
		// Optional: Tooltip support
		private CardHoverTooltip hoverTooltip;
	
	private void Start()
	{
		if (animationManager == null)
		{
			animationManager = CombatAnimationManager.Instance;
		}
		cardCanvas = GetComponent<Canvas>();
		if (cardCanvas != null)
		{
			hadCanvas = true;
			originalOverrideSorting = cardCanvas.overrideSorting;
			originalSortingOrder = cardCanvas.sortingOrder;
		}
		hoverTooltip = GetComponent<CardHoverTooltip>();
	}
	
	/// <summary>
	/// Call this after the card has been positioned to store its rest position.
	/// CardRuntimeManager should call this after repositioning cards.
	/// </summary>
	public void StoreOriginalPosition()
	{
		originalPosition = transform.localPosition;
		hasStoredPosition = true;
		
		// Also store the scale if not already stored
		if (!hasStoredScale)
		{
			StoreBaseScale();
		}
	}
	
	/// <summary>
	/// Store the card's base scale (for relative hover scaling).
	/// </summary>
	public void StoreBaseScale()
	{
		baseScale = transform.localScale;
		hasStoredScale = true;
	}
	
	/// <summary>
	/// Get the stored original position (or current if not stored yet).
	/// </summary>
	public Vector3 GetOriginalPosition()
	{
		if (!hasStoredPosition)
		{
			StoreOriginalPosition();
		}
		return originalPosition;
	}
	
	/// <summary>
	/// Get the card's base scale (for relative hover scaling).
	/// </summary>
	public Vector3 GetBaseScale()
	{
		if (!hasStoredScale)
		{
			StoreBaseScale();
		}
		return baseScale;
	}
	
	public void OnPointerEnter(PointerEventData eventData)
	{
		// IMPORTANT: Don't process hover if component is disabled
		if (!enabled) return;
		
        // Guard against re-entrancy while already hovering
        if (isHovering) return;

		if (!hasStoredPosition)
		{
			StoreOriginalPosition();
		}
		
		// Raise render order
		RaiseToTopLayer();
		
		if (animationManager != null)
		{
			animationManager.AnimateCardHover(gameObject, originalPosition, true);
			isHovering = true;
		}
		
		// Show tooltips if available
		if (hoverTooltip != null)
		{
			string[] lines = BuildTooltipLines();
			hoverTooltip.Show(lines);
		}
	}
	
	public void OnPointerExit(PointerEventData eventData)
	{
		// IMPORTANT: Don't process hover if component is disabled
		if (!enabled) return;
		
        if (!isHovering) return;

		if (animationManager != null)
		{
			animationManager.AnimateCardHover(gameObject, originalPosition, false);
			isHovering = false;
		}
		
		// Restore original order
		RestoreLayerOrder();
		
		// Hide tooltips if present
		if (hoverTooltip != null)
		{
			hoverTooltip.Hide();
		}
	}
	
	private void OnDisable()
	{
		// Reset hover state when disabled
        if (isHovering)
        {
            if (animationManager != null)
            {
                animationManager.AnimateCardHover(gameObject, originalPosition, false);
            }
            isHovering = false;
        }
		// Ensure order restored if disabled while hovering
		RestoreLayerOrder();
		// Hide tooltips if present
		if (hoverTooltip != null)
		{
			hoverTooltip.Hide();
		}
	}
	
	private void RaiseToTopLayer()
	{
        // Optionally bring to top within parent (can cause layout re-ordering)
        if (raiseBySibling && transform.parent != null)
        {
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
            Debug.Log($"[CardHover] {gameObject.name}: Raised to last sibling (index {originalSiblingIndex} → {transform.GetSiblingIndex()})");
        }

        // Prefer raising via canvas sorting which doesn't alter layout order
        if (raiseByCanvas && !stayWithinMask)
        {
            if (cardCanvas == null)
            {
                // Add a Canvas once to control sorting without affecting layout
                cardCanvas = gameObject.AddComponent<Canvas>();
                Debug.Log($"[CardHover] {gameObject.name}: Added Canvas component for sorting");
            }
            cardCanvas.overrideSorting = true;
            cardCanvas.sortingOrder = HoverSortingOrder;
            Debug.Log($"[CardHover] {gameObject.name}: Canvas sorting set to {HoverSortingOrder} (bring to front)");
        }
        else if (raiseByCanvas && stayWithinMask)
        {
            Debug.LogWarning($"[CardHover] {gameObject.name}: Canvas sorting disabled (stayWithinMask=true). Card won't come to front on hover.");
        }
	}

	private string[] BuildTooltipLines()
	{
		// Attempt to pull details from CombatCardAdapter → Card or CardData/Extended
		CombatCardAdapter adapter = GetComponent<CombatCardAdapter>();
		if (adapter != null)
		{
			var card = adapter.GetCard();
			if (card != null)
			{
				return BuildLinesFromCard(card);
			}
			// If no Card (when using CardDataExtended path), try to read from DeckBuilderCardUI
			DeckBuilderCardUI deckUI = GetComponent<DeckBuilderCardUI>();
			if (deckUI != null)
			{
				var cardData = deckUI.GetCardData();
				if (cardData is CardDataExtended ext)
				{
					return BuildLinesFromExtended(ext);
				}
			}
		}
		return System.Array.Empty<string>();
	}

	private string[] BuildLinesFromCard(Card card)
	{
		var lines = new System.Collections.Generic.List<string>();
		// Damage scaling
		var dmgScale = BuildScalingString("Scaling", card.scalesWithMeleeWeapon, card.scalesWithProjectileWeapon, card.scalesWithSpellWeapon, card.damageScaling);
		if (!string.IsNullOrEmpty(dmgScale)) lines.Add(dmgScale);
		// Guard scaling
		var grdScale = BuildScalingString("Guard Scaling", false, false, false, card.guardScaling);
		if (!string.IsNullOrEmpty(grdScale)) lines.Add(grdScale);
		// Ailments
		lines.AddRange(BuildAilmentLines(card.comboAilmentId, card.comboAilmentPortion, card.comboAilmentDuration));
		// Buffs on legacy Card via effects
		if (card.effects != null)
		{
			foreach (var eff in card.effects)
			{
				if (eff != null && eff.effectType == EffectType.ApplyStatus && string.Equals(eff.effectName, "Bolster", System.StringComparison.OrdinalIgnoreCase))
				{
					int dur = eff.duration > 0 ? eff.duration : 2;
					lines.Add($"Bolster: 2% less damage taken per stack ({dur} turns)");
				}
			}
		}
		// Combo requirement (legacy Card uses free-text comboWith)
		if (!string.IsNullOrWhiteSpace(card.comboWith))
		{
			lines.Add($"Combo activates after: {card.comboWith}");
		}
		// AoE
		if (card.isAoE) lines.Add($"AoE: Hits {Mathf.Max(1, card.aoeTargets)} enemies");
		return lines.ToArray();
	}

	private string[] BuildLinesFromExtended(CardDataExtended ext)
	{
		var lines = new System.Collections.Generic.List<string>();
		// Damage scaling (weapons + stats)
		var dmgScale = BuildScalingString("Scaling", ext.scalesWithMeleeWeapon, ext.scalesWithProjectileWeapon, ext.scalesWithSpellWeapon, ext.damageScaling);
		if (!string.IsNullOrEmpty(dmgScale)) lines.Add(dmgScale);
		// Guard scaling
		var grdScale = BuildScalingString("Guard Scaling", false, false, false, ext.guardScaling);
		if (!string.IsNullOrEmpty(grdScale)) lines.Add(grdScale);
		// Ailments from asset-defined combo/apply
		lines.AddRange(BuildAilmentLines(ext.comboAilment, ext.comboAilmentPortion, ext.comboAilmentDuration));
		// Buffs: Bolster via comboBuffs or effects
		if (ext.comboBuffs != null)
		{
			foreach (var buff in ext.comboBuffs)
			{
				if (!string.IsNullOrEmpty(buff) && string.Equals(buff, "Bolster", System.StringComparison.OrdinalIgnoreCase))
				{
					lines.Add("Bolster: 2% less damage taken per stack (2 turns)");
				}
			}
		}
		if (ext.effects != null)
		{
			foreach (var eff in ext.effects)
			{
				if (eff != null && eff.effectType == EffectType.ApplyStatus && string.Equals(eff.effectName, "Bolster", System.StringComparison.OrdinalIgnoreCase))
				{
					int dur = eff.duration > 0 ? eff.duration : 2;
					lines.Add($"Bolster: 2% less damage taken per stack ({dur} turns)");
				}
			}
		}
		// Combo requirement (typed)
		if (ext.enableCombo)
		{
			// Prefer enum-driven requirement; fallback to free-text comboWith
			string req = null;
			if (!string.IsNullOrEmpty(ext.comboWith)) req = ext.comboWith;
			if (req == null || req.Trim().Length == 0)
			{
				req = ext.comboWithCardType.ToString();
			}
			if (!string.IsNullOrWhiteSpace(req))
			{
				lines.Add($"Combo activates after: {req}");
			}
		}
		// AoE
		if (ext.isAoE) lines.Add($"AoE: Hits {Mathf.Max(1, ext.aoeTargets)} enemies");
		return lines.ToArray();
	}

	private string BuildScalingString(string title, bool melee, bool proj, bool spell, AttributeScaling scaling)
	{
		if (scaling == null && !melee && !proj && !spell) return string.Empty;
		System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
		if (melee) parts.Add("Melee weapon");
		if (proj) parts.Add("Projectile weapon");
		if (spell) parts.Add("Spell power");
		if (scaling != null)
		{
			if (scaling.strengthScaling > 0f) parts.Add(FormatStatScaling("STR", scaling.strengthScaling));
			if (scaling.dexterityScaling > 0f) parts.Add(FormatStatScaling("DEX", scaling.dexterityScaling));
			if (scaling.intelligenceScaling > 0f) parts.Add(FormatStatScaling("INT", scaling.intelligenceScaling));
		}
		if (parts.Count == 0) return string.Empty;
		return $"{title}: {string.Join(" and ", parts)}";
	}

	private string FormatStatScaling(string stat, float scale)
	{
		if (scale <= 0f) return string.Empty;
		float recip = 1f / scale;
		float rounded = Mathf.Round(recip);
		if (Mathf.Abs(recip - rounded) < 0.05f && rounded >= 1f && rounded <= 10f)
		{
			return $"{stat}/{rounded:0}";
		}
		return $"{stat} x {scale:0.##}";
	}

	private System.Collections.Generic.IEnumerable<string> BuildAilmentLines(AilmentId id, float portion, int duration)
	{
		var list = new System.Collections.Generic.List<string>();
		switch (id)
		{
			case AilmentId.Crumble:
				if (portion > 0f)
				{
					int dur = duration > 0 ? duration : 5;
					list.Add($"Crumble: Stores {portion:P0} of physical damage for {dur} turns (consumed by Shout)");
				}
				break;
			case AilmentId.Poison:
				list.Add("Poison: Deals damage over time each turn");
				break;
			case AilmentId.Burn:
				list.Add("Burn: Deals fire damage over time");
				break;
			case AilmentId.Freeze:
				list.Add("Freeze: Skips next action");
				break;
			case AilmentId.Stun:
				list.Add("Stun: Cannot act for a short duration");
				break;
			case AilmentId.Vulnerable:
				list.Add("Vulnerable: Takes increased damage");
				break;
			case AilmentId.Weak:
				list.Add("Weak: Deals reduced damage");
				break;
			case AilmentId.Frail:
				list.Add("Frail: Takes increased physical damage");
				break;
			case AilmentId.Slow:
				list.Add("Slow: Reduced action speed");
				break;
			case AilmentId.Curse:
				list.Add("Curse: Various negative effects");
				break;
		}
		return list;
	}
	
	private void RestoreLayerOrder()
	{
        // Restore sibling index if valid
        if (raiseBySibling && originalSiblingIndex >= 0 && transform.parent != null)
		{
			int maxIndex = transform.parent.childCount - 1;
			int target = Mathf.Clamp(originalSiblingIndex, 0, maxIndex);
			transform.SetSiblingIndex(target);
		}
		originalSiblingIndex = -1;

        // Restore canvas sorting
        if (cardCanvas != null)
		{
            if (hadCanvas)
            {
                cardCanvas.overrideSorting = originalOverrideSorting;
                cardCanvas.sortingOrder = originalSortingOrder;
            }
            else
            {
                // If we created a canvas, only use it when not constrained by mask
                cardCanvas.overrideSorting = false;
                cardCanvas.sortingOrder = 0;
            }
		}
	}
}

