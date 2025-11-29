using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles card hover animations using CombatAnimationManager.
/// Attach to card prefabs to enable hover effects.
/// Also raises hovered card to the top layer while hovering for visual clarity.
/// </summary>
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Ordering")]
    [SerializeField] private bool raiseBySibling = true; // Use sibling index for hover (safest method for cards in layouts)
    [SerializeField] private bool raiseByCanvas = false;   // Disabled - Canvas sorting causes raycast blocking issues
    [SerializeField] private bool stayWithinMask = false;  // when false, allows canvas override for hover (set true only if mask clipping required)
    [SerializeField] private bool forceRaiseOnHover = false; // Disabled to prevent Canvas-related issues
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
        bool shouldRaise = raiseByCanvas && (!stayWithinMask || forceRaiseOnHover);
        
        if (shouldRaise)
        {
            if (cardCanvas == null)
            {
                // Add a Canvas once to control sorting without affecting layout
                cardCanvas = gameObject.AddComponent<Canvas>();
                hadCanvas = false; // Track that we created this canvas
                originalOverrideSorting = false;
                originalSortingOrder = 0;
                Debug.Log($"[CardHover] {gameObject.name}: Added Canvas component for sorting");
                
                // CRITICAL: Ensure canvas doesn't block raycasts or interfere with pointer events
                var graphicRaycaster = gameObject.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (graphicRaycaster != null)
                {
                    // Remove GraphicRaycaster if it was auto-added (it blocks pointer events)
                    Destroy(graphicRaycaster);
                    Debug.Log($"[CardHover] {gameObject.name}: Removed GraphicRaycaster to prevent pointer blocking");
                }
                
                // ADDITIONAL FIX: Ensure the card's own graphics remain raycast targets
                // The card that's hovering MUST still receive raycasts
                var cardGraphics = gameObject.GetComponentsInChildren<Graphic>();
                foreach (var graphic in cardGraphics)
                {
                    // Keep raycastTarget TRUE so the card itself remains interactive
                    // Only child/preview elements should have it disabled
                    if (graphic.gameObject == gameObject || graphic.transform.parent == transform)
                    {
                        graphic.raycastTarget = true;
                    }
                }
                
                Debug.Log($"[CardHover] {gameObject.name}: Ensured card graphics remain raycast targets ({cardGraphics.Length} graphics checked)");
            }
            else if (!hadCanvas && cardCanvas != null)
            {
                // Canvas was added previously but we didn't track it - update tracking
                hadCanvas = false;
                originalOverrideSorting = false;
                originalSortingOrder = 0;
                
                // Also ensure raycasts work for this case
                var cardGraphics = gameObject.GetComponentsInChildren<Graphic>();
                foreach (var graphic in cardGraphics)
                {
                    if (graphic.gameObject == gameObject || graphic.transform.parent == transform)
                    {
                        graphic.raycastTarget = true;
                    }
                }
            }
            
            cardCanvas.overrideSorting = true;
            cardCanvas.sortingOrder = HoverSortingOrder;
            
            if (stayWithinMask && forceRaiseOnHover)
            {
                Debug.Log($"[CardHover] {gameObject.name}: Canvas sorting forced to {HoverSortingOrder} (forceRaiseOnHover=true)");
            }
            else
            {
                Debug.Log($"[CardHover] {gameObject.name}: Canvas sorting set to {HoverSortingOrder} (bring to front)");
            }
        }
        else if (raiseByCanvas && stayWithinMask && !forceRaiseOnHover)
        {
            Debug.LogWarning($"[CardHover] {gameObject.name}: Canvas sorting disabled (stayWithinMask=true, forceRaiseOnHover=false). Card won't come to front on hover.");
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
		Character character = GetCurrentCharacter();
		
		// Calculate actual damage value
		if (card.baseDamage > 0)
		{
			float totalDamage = card.baseDamage;
			if (character != null)
			{
				// Use DamageCalculator for accurate damage calculation
				totalDamage = DamageCalculator.CalculateCardDamage(card, character);
			}
			else if (card.damageScaling != null)
			{
				// Fallback: just add base + scaling bonus (without weapon/embossing)
				totalDamage += card.damageScaling.CalculateScalingBonus(null);
			}
			lines.Add($"Damage: {card.baseDamage:F0} → {totalDamage:F0} (with scaling)");
		}
		
		// Calculate actual guard value
		if (card.baseGuard > 0)
		{
			float totalGuard = card.baseGuard;
			if (character != null && card.guardScaling != null)
			{
				totalGuard += card.guardScaling.CalculateScalingBonus(character);
			}
			lines.Add($"Guard: {card.baseGuard:F0} → {totalGuard:F0} (with scaling)");
		}
		
		// Damage scaling details
		var dmgScale = BuildScalingString("Scaling", card.scalesWithMeleeWeapon, card.scalesWithProjectileWeapon, card.scalesWithSpellWeapon, card.damageScaling);
		if (!string.IsNullOrEmpty(dmgScale)) lines.Add(dmgScale);
		// Guard scaling details
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
		Character character = GetCurrentCharacter();
		
		// Calculate actual damage value
		if (ext.damage > 0)
		{
			float totalDamage = ext.damage;
			if (character != null)
			{
				// Convert CardDataExtended to Card for DamageCalculator
				Card tempCard = ext.ToCard();
				totalDamage = DamageCalculator.CalculateCardDamage(tempCard, character);
			}
			else if (ext.damageScaling != null)
			{
				// Fallback: just add base + scaling bonus (without weapon/embossing)
				totalDamage += ext.damageScaling.CalculateScalingBonus(null);
			}
			lines.Add($"Damage: {ext.damage:F0} → {totalDamage:F0} (with scaling)");
		}
		
		// Calculate actual guard value
		if (ext.block > 0)
		{
			float totalGuard = ext.block;
			if (character != null && ext.guardScaling != null)
			{
				totalGuard += ext.guardScaling.CalculateScalingBonus(character);
			}
			lines.Add($"Guard: {ext.block:F0} → {totalGuard:F0} (with scaling)");
		}
		
		// Damage scaling details (weapons + stats)
		var dmgScale = BuildScalingString("Scaling", ext.scalesWithMeleeWeapon, ext.scalesWithProjectileWeapon, ext.scalesWithSpellWeapon, ext.damageScaling);
		if (!string.IsNullOrEmpty(dmgScale)) lines.Add(dmgScale);
		// Guard scaling details
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
		// Momentum effects (shown in tooltip, not on card)
		if (character != null)
		{
			string momentumDesc = ext.GetDynamicMomentumDescription(character);
			if (string.IsNullOrWhiteSpace(momentumDesc))
			{
				momentumDesc = ext.momentumEffectDescription;
			}
			if (!string.IsNullOrWhiteSpace(momentumDesc))
			{
				lines.Add($"Momentum: {momentumDesc}");
			}
		}
		else if (!string.IsNullOrWhiteSpace(ext.momentumEffectDescription))
		{
			lines.Add($"Momentum: {ext.momentumEffectDescription}");
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
	
	/// <summary>
	/// Get the current character for tooltip calculations
	/// </summary>
	private Character GetCurrentCharacter()
	{
		// Try to get character from CharacterManager
		CharacterManager characterManager = CharacterManager.Instance;
		if (characterManager != null && characterManager.HasCharacter())
		{
			return characterManager.GetCurrentCharacter();
		}
		
		// Try to get character from CombatDeckManager
		CombatDeckManager deckManager = CombatDeckManager.Instance;
		if (deckManager != null)
		{
			return deckManager.GetCurrentCharacter();
		}
		
		return null;
	}

	private string BuildScalingString(string title, bool melee, bool proj, bool spell, AttributeScaling scaling)
	{
		if (scaling == null && !melee && !proj && !spell) return string.Empty;
		System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
		Character character = GetCurrentCharacter();
		
		if (melee) 
		{
			if (character != null)
			{
				float weaponDmg = character.weapons.GetWeaponDamage(WeaponType.Melee);
				parts.Add($"Melee weapon (+{weaponDmg:F0})");
			}
			else
			{
				parts.Add("Melee weapon");
			}
		}
		if (proj) 
		{
			if (character != null)
			{
				float weaponDmg = character.weapons.GetWeaponDamage(WeaponType.Projectile);
				parts.Add($"Projectile weapon (+{weaponDmg:F0})");
			}
			else
			{
				parts.Add("Projectile weapon");
			}
		}
		if (spell) 
		{
			if (character != null)
			{
				float weaponDmg = character.weapons.GetWeaponDamage(WeaponType.Spell);
				parts.Add($"Spell power (+{weaponDmg:F0})");
			}
			else
			{
				parts.Add("Spell power");
			}
		}
		if (scaling != null && character != null)
		{
			// Calculate actual scaling bonus values
			float scalingBonus = scaling.CalculateScalingBonus(character);
			if (scalingBonus > 0f)
			{
				// Build detailed breakdown
				System.Collections.Generic.List<string> statParts = new System.Collections.Generic.List<string>();
				if (scaling.strengthScaling > 0f)
				{
					float strBonus = character.strength * scaling.strengthScaling;
					statParts.Add($"STR: +{strBonus:F0}");
				}
				if (scaling.strengthDivisor > 0f)
				{
					float strDivBonus = character.strength / scaling.strengthDivisor;
					statParts.Add($"STR/{scaling.strengthDivisor:F0}: +{strDivBonus:F0}");
				}
				if (scaling.dexterityScaling > 0f)
				{
					float dexBonus = character.dexterity * scaling.dexterityScaling;
					statParts.Add($"DEX: +{dexBonus:F0}");
				}
				if (scaling.dexterityDivisor > 0f)
				{
					float dexDivBonus = character.dexterity / scaling.dexterityDivisor;
					statParts.Add($"DEX/{scaling.dexterityDivisor:F0}: +{dexDivBonus:F0}");
				}
				if (scaling.intelligenceScaling > 0f)
				{
					float intBonus = character.intelligence * scaling.intelligenceScaling;
					statParts.Add($"INT: +{intBonus:F0}");
				}
				if (scaling.intelligenceDivisor > 0f)
				{
					float intDivBonus = character.intelligence / scaling.intelligenceDivisor;
					statParts.Add($"INT/{scaling.intelligenceDivisor:F0}: +{intDivBonus:F0}");
				}
				
				if (statParts.Count > 0)
				{
					parts.Add($"Stats: {string.Join(", ", statParts)} (Total: +{scalingBonus:F0})");
				}
			}
		}
		else if (scaling != null)
		{
			// Fallback to format-only when character is not available
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
		case AilmentId.Chill:
			list.Add("Chill: Slows the target and reduces action speed");
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
			Debug.Log($"[CardHover] {gameObject.name}: Restored sibling index to {target}");
		}
		originalSiblingIndex = -1;

        // Restore canvas sorting
        if (cardCanvas != null)
		{
            if (hadCanvas)
            {
                // Canvas existed before we modified it - restore original settings
                cardCanvas.overrideSorting = originalOverrideSorting;
                cardCanvas.sortingOrder = originalSortingOrder;
                Debug.Log($"[CardHover] {gameObject.name}: Restored canvas to original (override={originalOverrideSorting}, order={originalSortingOrder})");
            }
            else
            {
                // We created this canvas - disable override sorting
                cardCanvas.overrideSorting = false;
                cardCanvas.sortingOrder = 0;
                Debug.Log($"[CardHover] {gameObject.name}: Disabled canvas override sorting (created canvas)");
            }
		}
	}
}

