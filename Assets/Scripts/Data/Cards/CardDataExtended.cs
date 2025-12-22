using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Extended CardData with full combat features.
/// REPLACES the need for separate Card class.
/// 
/// Migration Strategy:
/// 1. Add this to your project
/// 2. Gradually move Card features here
/// 3. Update CombatDeckManager to use CardData directly
/// 4. Eliminate Card class conversion
/// </summary>
[CreateAssetMenu(fileName = "New Card Extended", menuName = "Dexiled/Cards/Card Data Extended")]
public class CardDataExtended : CardData
{
    [Header("Mana Cost (Skill Cards)")]
    [Tooltip("Percentage of max mana cost for Skill cards (e.g., 10 = 10% of maxMana). Only used if cardType is Skill. If set, playCost should be 0. Leave at 0 to use playCost as flat cost.")]
    public int percentageManaCost = 0;
    
    [Header("Combat Properties (Extended)")]
    public DamageType primaryDamageType = DamageType.Physical;
    public List<DamageType> additionalDamageTypes = new List<DamageType>();
    
    [Header("Weapon Scaling")]
    public bool scalesWithMeleeWeapon = false;
    public bool scalesWithProjectileWeapon = false;
    public bool scalesWithSpellWeapon = false;
    
    [Header("Area of Effect")]
    public bool isAoE = false;
    public int aoeTargets = 3;
    [Tooltip("Row scope for AoE targeting: BothRows (all) or SelectedRow (same row as target)")]
    public AoERowScope aoeRowScope = AoERowScope.BothRows;
    
    [Header("Multi-Hit")]
    [Tooltip("If enabled, this card will hit the same target multiple times")]
    public bool isMultiHit = false;
    [Tooltip("Number of times to hit the target (only used if isMultiHit is true)")]
    public int hitCount = 2;
    
    [Header("Attribute Scaling")]
    public AttributeScaling damageScaling = new AttributeScaling();
    public AttributeScaling guardScaling = new AttributeScaling();
    [Tooltip("Scaling for temporary evasion granted by Skill cards")]
    public AttributeScaling evasionScaling = new AttributeScaling();
    
    [Header("Evasion (Skill Cards)")]
    [Tooltip("Base evasion amount granted (for Skill cards that grant evasion)")]
    public float baseEvasion = 0f;
    [Tooltip("Duration in turns for evasion buff (-1 = rest of combat)")]
    public int evasionDuration = -1;
    
    [Header("Requirements")]
    public CardRequirements requirements = new CardRequirements();
    
    [Header("Card Effects")]
    public List<CardEffect> effects = new List<CardEffect>();
    
    [Header("Combo Settings")]
    [Tooltip("Enable/disable all combo interactions for this card asset")] 
    public bool enableCombo = true;
    
    [Header("Combo Properties (Extended)")]
    [Tooltip("Cards or types this card can combo with (comma-separated, or use card type dropdown)")]
    public string comboWith = "";
    [Tooltip("Combo description text shown in Additional Effects on combat card prefab")] 
    [TextArea(2,4)] public string comboDescription = "";
    
    [Header("Momentum Effects")]
    [Tooltip("Momentum effect description shown in Additional Effects on combat card prefab (e.g., 'Gain 1 Momentum', 'If you have 3+ Momentum: This card costs 0')")]
    [TextArea(2,4)] public string momentumEffectDescription = "";
    
    [Header("Combo Triggers")] 
    [Tooltip("Trigger combo after playing a card of this type (optional; overrides free text)")]
    public CardType comboWithCardType = CardType.Attack; 
    
    [Header("Combo Effect - Numeric Modifiers")] 
    public float comboAttackIncrease = 0f; 
    public float comboGuardIncrease = 0f; 
    public bool comboIsAoE = false; 
    public int comboManaRefund = 0; 
    
    [Header("Combo Effect - Ailments & Buffs")] 
    [Tooltip("Apply status effect to enemy/enemies when combo triggers")] 
    public string comboApplyAilment = ""; // legacy text (kept for back-compat)
    [Tooltip("List of buffs to apply to player when combo triggers")] 
    public List<string> comboBuffs = new List<string>();
    
    [Tooltip("If enabled, when this COMBO triggers it will consume the selected ailment on targets")] 
    public bool comboConsumeAilment = false;
    public AilmentId comboConsumeAilmentId = AilmentId.None;

    [Header("Combo Effect - Draw")]
    [Tooltip("Number of cards to draw when this combo triggers")] 
    public int comboDrawCards = 0;

    [Header("Combo Ailment (Per-Card)")]
    [Tooltip("Select an ailment to apply when combo triggers (per-card)")]
    public AilmentId comboAilment = AilmentId.None;
    [Tooltip("Portion of relevant damage to store/apply for the ailment (e.g., 0.3 for 30%)")] 
    public float comboAilmentPortion = 0f;
    [Tooltip("Base duration in turns for the ailment (e.g., 5 turns for Crumble)")] 
    public int comboAilmentDuration = 0;
    
    [Header("Consume Ailment (Per-Card)")]
    [Tooltip("If enabled, this card will consume the selected ailment on targets when played")]
    public bool consumeAilment = false;
    public AilmentId consumeAilmentId = AilmentId.None;
    
    [Header("Combo Effect - Scaling")] 
    public ComboScalingType comboScaling = ComboScalingType.None; 
    [Tooltip("Divisible scaling: e.g., 4 means STR/4 added")] 
    public float comboScalingDivisor = 1f; 
    
    [Header("Combo Logic")] 
    [Tooltip("Additive: adds to base effect; Instead: replaces base effect")] 
    public ComboLogicType comboLogic = ComboLogicType.Additive;
    
    [Header("Legacy Combo (Compatibility)")]
    [Tooltip("Legacy free-text combo effect string (kept for backward compatibility)")]
    public string comboEffect = "";
    [Tooltip("Legacy combo highlight type (e.g., 'specific', 'type', 'tag')")]
    public string comboHighlightType = "specific";
    
    [Header("Channeling Bonuses")]
    [Tooltip("Enable automatic bonuses when the player is channeling this card's group key")]
    public bool channelingBonusEnabled = false;
    [Tooltip("Minimum consecutive casts required before the bonus applies (default 2)")]
    [Min(1)] public int channelingMinStacks = 2;
    [Tooltip("Additional guard granted while channeling (added before percentages)")]
    public float channelingAdditionalGuard = 0f;
    [Tooltip("% increased damage while channeling (additive)")]
    public float channelingDamageIncreasedPercent = 0f;
    [Tooltip("% more damage while channeling (multiplicative)")]
    public float channelingDamageMorePercent = 0f;
    [Tooltip("% increased guard while channeling (additive)")]
    public float channelingGuardIncreasedPercent = 0f;
    [Tooltip("% more guard while channeling (multiplicative)")]
    public float channelingGuardMorePercent = 0f;
    
    [Header("Tags")]
    public List<string> tags = new List<string>();
    
    [Header("Embossing System")]
    // Note: embossingSlots is inherited from CardData base class, do not redeclare it here
    
    [Tooltip("List of applied embossing instances (includes level and XP). Persists with the card asset.")]
    public List<EmbossingInstance> appliedEmbossings = new List<EmbossingInstance>();
    
    [Header("Delayed Action")]
    [Tooltip("Number of turns to delay this card's execution (0 = play immediately). Used for Temporal Savant and similar effects.")]
    [Min(0)]
    public int delayTurns = 0;
    [Tooltip("If true, card is delayed (can be set by ascendancy or card effect)")]
    public bool isDelayed = false;
    
    [Header("Preparation System")]
    [Tooltip("Can this card be prepared for later unleashing?")]
    public bool canPrepare = false;
    
    [Tooltip("Maximum turns this card can be prepared (base value, modified by INT)")]
    public int maxPrepTurns = 3;
    
    [Tooltip("Damage/effect multiplier gained per turn prepared (e.g., 0.5 = +50% per turn)")]
    public float multiplierPerTurn = 0.5f;
    
    [Tooltip("Unleash condition: 'manual' (click), 'auto_on_max' (auto at max), 'triggered' (needs trigger card), 'decay' (auto with penalty)")]
    public string unleashCondition = "manual";
    
    [Tooltip("Unleash effect type: 'deal_stored_damage', 'apply_buffs', 'hybrid'")]
    public string unleashEffect = "deal_stored_damage";
    
    [Tooltip("Additional text describing the preparation/unleash effect")]
    [TextArea(2,3)]
    public string prepareDescription = "";
    
    [Header("Prepared Card Bonuses (Thief Class)")]
    [Tooltip("Base damage bonus per prepared card (for {PrepareDamage} variable). Set to 0 to disable.")]
    public float preparedCardDamageBase = 0f;
    
    [Tooltip("Attribute scaling for prepared card damage bonus (e.g., Dex/3). Leave all at 0 to disable scaling.")]
    public AttributeScaling preparedCardDamageScaling = new AttributeScaling();
    
    [Tooltip("Base poison bonus per prepared card (for {PreparePoison} variable). Set to 0 to disable.")]
    public int preparedCardPoisonBase = 0;
    
    [Tooltip("Base guard bonus per prepared card (for {PrepareGuard} variable). Set to 0 to disable.")]
    public float preparedCardGuardBase = 0f;
    
    [Tooltip("Attribute scaling for prepared card guard bonus (e.g., Dex/2). Leave all at 0 to disable scaling.")]
    public AttributeScaling preparedCardGuardScaling = new AttributeScaling();
    
    [Header("Consumed Card Bonuses (Thief Class)")]
    [Tooltip("Base damage bonus per consumed card (for {ConsumedDamage} variable). Set to 0 to disable.")]
    public float consumedCardDamageBase = 0f;
    
    [Tooltip("Attribute scaling for consumed card damage bonus (e.g., Dex/3). Leave all at 0 to disable scaling.")]
    public AttributeScaling consumedCardDamageScaling = new AttributeScaling();
    
    // IMPORTANT: Add all Card methods here
    
    /// <summary>
    /// Check if character can use this card
    /// </summary>
    public bool CanUseCard(Character character)
    {
        if (!requirements.MeetsRequirements(character)) return false;
        
        // Calculate actual mana cost (percentage for Skill cards if set, otherwise flat)
        int requiredMana = GetCurrentManaCost(null, character);
        if (requiredMana > 0 && character.mana < requiredMana) return false;
        
        return true;
    }
    
    /// <summary>
    /// Get detailed requirement check
    /// </summary>
    public (bool canUse, string reason) CheckCardUsage(Character character)
    {
        var (meetsReqs, reqReason) = requirements.CheckRequirements(character);
        if (!meetsReqs)
            return (false, reqReason);
        
        // Calculate actual mana cost (percentage for Skill cards if set, otherwise flat)
        int requiredMana = GetCurrentManaCost(null, character);
        if (requiredMana > 0 && character.mana < requiredMana)
            return (false, $"Requires {requiredMana} Mana (has {character.mana})");
        
        return (true, "Can use card");
    }
    
    /// <summary>
    /// Get weapon scaling damage
    /// </summary>
    public float GetWeaponScalingDamage(Character character)
    {
        float weaponDamage = 0f;
        
        if (scalesWithMeleeWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Melee);
        
        if (scalesWithProjectileWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Projectile);
        
        if (scalesWithSpellWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Spell);
        
        return weaponDamage;
    }
    
    /// <summary>
    /// Get dynamic description with actual calculated values
    /// </summary>
    public string GetDynamicDescription(Character character)
    {
        if (string.IsNullOrEmpty(description))
            return GetCardTooltip();
        
        string dynamicDesc = description;
        
		// Replace damage placeholders
		if (damage > 0)
		{
			// Base + attribute scaling + weapon scaling
			float baseWithScaling = damage;
			baseWithScaling += damageScaling.CalculateScalingBonus(character);
			baseWithScaling += GetWeaponScalingDamage(character);
			
			// Build a lightweight stats snapshot and damage context
			var statsData = new CharacterStatsData(character);
			// CardData.cardType is a string; runtime Card uses CardType enum.
			// Treat ScriptableObject as an Attack and detect "Spell" based on tags.
			bool hasSpellTag = tags != null && tags.Contains("Spell");
			
			var ctx = new DamageContext
			{
				damageType = primaryDamageType.ToString().ToLower(), // matches calculator expectations
				isAttack = string.Equals(cardType, "Attack", System.StringComparison.OrdinalIgnoreCase),
				isSpell = hasSpellTag,
				isProjectile = scalesWithProjectileWeapon,
				isArea = isAoE,
				isMelee = scalesWithMeleeWeapon,
				isRanged = scalesWithProjectileWeapon,
				isDot = false,
				weaponType = scalesWithMeleeWeapon ? "onehanded" : (scalesWithProjectileWeapon ? "bow" : (scalesWithSpellWeapon ? "wand" : "")),
				targetChilled = false,
				targetShocked = false,
				targetIgnited = false,
				targetIsElite = false
			};
			
			// Compute final card damage using the unified helpers
			var totals = StatAggregator.BuildTotals(statsData);
			float finalDamage = CardDamageUtility.ComputeCardDamage(baseWithScaling, statsData, totals, ctx);
			
			dynamicDesc = dynamicDesc.Replace("{damage}", finalDamage.ToString("F0"));
			dynamicDesc = dynamicDesc.Replace("{baseDamage}", damage.ToString());
		}
        
        // Replace guard placeholders for any card that grants block
        if (block > 0)
        {
            float totalGuard = block;
            totalGuard += guardScaling.CalculateScalingBonus(character);
            
            dynamicDesc = dynamicDesc.Replace("{guard}", totalGuard.ToString("F0"));
            dynamicDesc = dynamicDesc.Replace("{baseGuard}", block.ToString());
        }
        
        // Replace AoE placeholders
        if (isAoE)
        {
            dynamicDesc = dynamicDesc.Replace("{aoeTargets}", aoeTargets.ToString());
        }
        
        // Replace attribute placeholders
        if (character != null)
        {
            dynamicDesc = dynamicDesc.Replace("{str}", character.strength.ToString());
            dynamicDesc = dynamicDesc.Replace("{dex}", character.dexterity.ToString());
            dynamicDesc = dynamicDesc.Replace("{int}", character.intelligence.ToString());
            
            if (damageScaling.strengthScaling > 0)
            {
                float strBonus = character.strength * damageScaling.strengthScaling;
                dynamicDesc = dynamicDesc.Replace("{strBonus}", strBonus.ToString("F0"));
            }
            if (damageScaling.strengthDivisor > 0)
            {
                float strDivBonus = character.strength / damageScaling.strengthDivisor;
                dynamicDesc = dynamicDesc.Replace("{strDivisor}", strDivBonus.ToString("F0"));
            }
            
            if (damageScaling.dexterityScaling > 0)
            {
                float dexBonus = character.dexterity * damageScaling.dexterityScaling;
                dynamicDesc = dynamicDesc.Replace("{dexBonus}", dexBonus.ToString("F0"));
            }
            if (damageScaling.dexterityDivisor > 0)
            {
                float dexDivBonus = character.dexterity / damageScaling.dexterityDivisor;
                dynamicDesc = dynamicDesc.Replace("{dexDivisor}", dexDivBonus.ToString("F0"));
            }
            
            if (damageScaling.intelligenceScaling > 0)
            {
                float intBonus = character.intelligence * damageScaling.intelligenceScaling;
                dynamicDesc = dynamicDesc.Replace("{intBonus}", intBonus.ToString("F0"));
            }
            if (damageScaling.intelligenceDivisor > 0)
            {
                float intDivBonus = character.intelligence / damageScaling.intelligenceDivisor;
                dynamicDesc = dynamicDesc.Replace("{intDivisor}", intDivBonus.ToString("F0"));
            }
            
            if (guardScaling.strengthScaling > 0)
            {
                float guardStrBonus = character.strength * guardScaling.strengthScaling;
                dynamicDesc = dynamicDesc.Replace("{guardStrBonus}", guardStrBonus.ToString("F0"));
            }
            if (guardScaling.strengthDivisor > 0)
            {
                float guardStrDivBonus = character.strength / guardScaling.strengthDivisor;
                dynamicDesc = dynamicDesc.Replace("{guardStrDivisor}", guardStrDivBonus.ToString("F0"));
            }
            
            if (guardScaling.dexterityScaling > 0)
            {
                float guardDexBonus = character.dexterity * guardScaling.dexterityScaling;
                dynamicDesc = dynamicDesc.Replace("{guardDexBonus}", guardDexBonus.ToString("F0"));
            }
            if (guardScaling.dexterityDivisor > 0)
            {
                float guardDexDivBonus = character.dexterity / guardScaling.dexterityDivisor;
                dynamicDesc = dynamicDesc.Replace("{guardDexDivisor}", guardDexDivBonus.ToString("F0"));
            }
            
            if (guardScaling.intelligenceScaling > 0)
            {
                float guardIntBonus = character.intelligence * guardScaling.intelligenceScaling;
                dynamicDesc = dynamicDesc.Replace("{guardIntBonus}", guardIntBonus.ToString("F0"));
            }
            if (guardScaling.intelligenceDivisor > 0)
            {
                float guardIntDivBonus = character.intelligence / guardScaling.intelligenceDivisor;
                dynamicDesc = dynamicDesc.Replace("{guardIntDivisor}", guardIntDivBonus.ToString("F0"));
            }
            
            // Replace evasion placeholders (for Skill cards)
            // Check both flat evasion and percentage-based evasion
            bool hasEvasion = baseEvasion > 0 || evasionScaling.CalculateScalingBonus(character) > 0;
            bool isPercentageEvasion = description.ToLower().Contains("% increased evasion");
            
            if (hasEvasion || isPercentageEvasion)
            {
                if (isPercentageEvasion)
                {
                    // For percentage-based evasion, extract from description
                    var match = System.Text.RegularExpressions.Regex.Match(description, @"(\d+)%\s*increased\s*evasion", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success && float.TryParse(match.Groups[1].Value, out float percentage))
                    {
                        dynamicDesc = dynamicDesc.Replace("{evasion}", percentage.ToString("F0") + "%");
                    }
                }
                else
                {
                    // Flat evasion: base + scaling
                    float totalEvasion = baseEvasion + evasionScaling.CalculateScalingBonus(character);
                    dynamicDesc = dynamicDesc.Replace("{evasion}", totalEvasion.ToString("F0"));
                    dynamicDesc = dynamicDesc.Replace("{baseEvasion}", baseEvasion.ToString("F0"));
                }
                
                // Evasion scaling placeholders
                if (evasionScaling.dexterityScaling > 0)
                {
                    float evasionDexBonus = character.dexterity * evasionScaling.dexterityScaling;
                    dynamicDesc = dynamicDesc.Replace("{evasionDexBonus}", evasionDexBonus.ToString("F0"));
                }
                if (evasionScaling.dexterityDivisor > 0)
                {
                    float evasionDexDivBonus = character.dexterity / evasionScaling.dexterityDivisor;
                    dynamicDesc = dynamicDesc.Replace("{evasionDexDivisor}", evasionDexDivBonus.ToString("F0"));
                    // Also support common pattern: (+Dex/2) -> shows the divisor bonus
                    string dexDivPattern = "(+Dex/" + evasionScaling.dexterityDivisor + ")";
                    string dexDivReplacement = "(+{evasionDexDivisor})";
                    dynamicDesc = dynamicDesc.Replace(dexDivPattern, dexDivReplacement);
                }
            }
            
            if (scalesWithMeleeWeapon)
            {
                float weaponDamage = character.weapons.GetWeaponDamage(WeaponType.Melee);
                dynamicDesc = dynamicDesc.Replace("{weaponDamage}", weaponDamage.ToString("F0"));
            }
            if (scalesWithProjectileWeapon)
            {
                float weaponDamage = character.weapons.GetWeaponDamage(WeaponType.Projectile);
                dynamicDesc = dynamicDesc.Replace("{weaponDamage}", weaponDamage.ToString("F0"));
            }
            if (scalesWithSpellWeapon)
            {
                float weaponDamage = character.weapons.GetWeaponDamage(WeaponType.Spell);
                dynamicDesc = dynamicDesc.Replace("{weaponDamage}", weaponDamage.ToString("F0"));
            }
        }
        
        // Replace mana cost placeholder
        // For Skill cards with percentage cost, show percentage; otherwise show playCost
        string costDisplay = playCost.ToString();
        if (string.Equals(cardType, "Skill", System.StringComparison.OrdinalIgnoreCase) && percentageManaCost > 0)
        {
            if (character != null)
            {
                float percentageCost = percentageManaCost / 100.0f;
                int calculatedCost = Mathf.RoundToInt(character.maxMana * percentageCost);
                costDisplay = $"{percentageManaCost}% ({calculatedCost})";
            }
            else
            {
                costDisplay = $"{percentageManaCost}%";
            }
        }
        dynamicDesc = dynamicDesc.Replace("{manaCost}", costDisplay);
        dynamicDesc = dynamicDesc.Replace("{cost}", costDisplay);
        
        // Final fallback: ensure {aoeTargets} is always replaced even if isAoE wasn't set
        dynamicDesc = dynamicDesc.Replace("{aoeTargets}", (aoeTargets > 0 ? aoeTargets.ToString() : "1"));
        
        // Replace prepared card variables (Thief class cards)
        // These work both in and out of combat
        // IMPORTANT: Only show prepared card variables if:
        // 1. This card is a "Consume prepared" card (like Perfect Strike) - count ALL prepared cards
        // 2. This card has "If you have prepared cards" condition (like Ambush, Poisoned Blade) - count ALL prepared cards if any exist
        // 3. This card is currently prepared - count only this card (count = 1)
        // 4. Otherwise - don't show the variables (count = 0)
        
        bool isConsumePreparedCard = ThiefCardEffects.IsConsumePreparedCard(this);
        bool hasPreparedCardsCondition = ThiefCardEffects.HasPreparedCardsCondition(this);
        bool isThisCardPrepared = ThiefCardEffects.IsCardPrepared(this);
        
        int preparedCount = 0;
        if (isConsumePreparedCard)
        {
            // "Consume prepared" cards count ALL prepared cards
            preparedCount = ThiefCardEffects.GetPreparedCardCount();
        }
        else if (hasPreparedCardsCondition)
        {
            // Cards with "If you have prepared cards" condition count ALL prepared cards
            // (they check if ANY card is prepared and use the total count for bonuses)
            preparedCount = ThiefCardEffects.GetPreparedCardCount();
        }
        else if (isThisCardPrepared)
        {
            // Regular prepared cards only count themselves
            preparedCount = 1;
        }
        // else: preparedCount = 0 (card is not prepared, don't show variables)
        
        // {PrepareCount} - Number of prepared cards
        dynamicDesc = dynamicDesc.Replace("{PrepareCount}", preparedCount.ToString());
        
        // {PrepareDamage} - Damage bonus per prepared card
        // Reads from card's preparedCardDamageBase and preparedCardDamageScaling fields
        if (dynamicDesc.Contains("{PrepareDamage}"))
        {
            if (preparedCardDamageBase > 0f || preparedCardDamageScaling != null)
            {
                float damagePerCard = preparedCardDamageBase;
                if (preparedCardDamageScaling != null)
                {
                    damagePerCard += preparedCardDamageScaling.CalculateScalingBonus(character);
                }
                float totalPrepareDamage = damagePerCard * preparedCount;
                dynamicDesc = dynamicDesc.Replace("{PrepareDamage}", totalPrepareDamage.ToString("F0"));
            }
            else
            {
                // Fallback: hide the variable if not configured
                dynamicDesc = dynamicDesc.Replace("{PrepareDamage}", "0");
            }
        }
        
        // {PreparePoison} - Poison bonus per prepared card
        // Reads from card's preparedCardPoisonBase field
        if (dynamicDesc.Contains("{PreparePoison}"))
        {
            if (preparedCardPoisonBase > 0)
            {
                int totalPreparePoison = preparedCardPoisonBase * preparedCount;
                dynamicDesc = dynamicDesc.Replace("{PreparePoison}", totalPreparePoison.ToString());
            }
            else
            {
                // Fallback: hide the variable if not configured
                dynamicDesc = dynamicDesc.Replace("{PreparePoison}", "0");
            }
        }
        
        // {ConsumedDamage} - Damage bonus per consumed card
        // Reads from card's consumedCardDamageBase and consumedCardDamageScaling fields
        // Note: This assumes all prepared cards will be consumed (for Perfect Strike)
        // Only show for "Consume prepared" cards
        if (dynamicDesc.Contains("{ConsumedDamage}"))
        {
            if (isConsumePreparedCard && (consumedCardDamageBase > 0f || consumedCardDamageScaling != null))
            {
                float damagePerConsumedCard = consumedCardDamageBase;
                if (consumedCardDamageScaling != null)
                {
                    damagePerConsumedCard += consumedCardDamageScaling.CalculateScalingBonus(character);
                }
                // For consume cards, count all prepared cards (they will all be consumed)
                int allPreparedCount = ThiefCardEffects.GetPreparedCardCount();
                float totalConsumedDamage = damagePerConsumedCard * allPreparedCount;
                dynamicDesc = dynamicDesc.Replace("{ConsumedDamage}", totalConsumedDamage.ToString("F0"));
            }
            else
            {
                // Fallback: hide the variable if not configured or not a consume card
                dynamicDesc = dynamicDesc.Replace("{ConsumedDamage}", "0");
            }
        }
        
        // {PrepareGuard} - Guard bonus per prepared card
        // Reads from card's preparedCardGuardBase and preparedCardGuardScaling fields
        if (dynamicDesc.Contains("{PrepareGuard}"))
        {
            if (preparedCardGuardBase > 0f || preparedCardGuardScaling != null)
            {
                float guardPerCard = preparedCardGuardBase;
                if (preparedCardGuardScaling != null)
                {
                    guardPerCard += preparedCardGuardScaling.CalculateScalingBonus(character);
                }
                float totalPrepareGuard = guardPerCard * preparedCount;
                dynamicDesc = dynamicDesc.Replace("{PrepareGuard}", totalPrepareGuard.ToString("F0"));
            }
            else
            {
                // Fallback: hide the variable if not configured
                dynamicDesc = dynamicDesc.Replace("{PrepareGuard}", "0");
            }
        }
        
        return dynamicDesc;
    }
    
    /// <summary>
    /// Get dynamic momentum effect description replacing placeholders
    /// Supported placeholders:
    /// {momentum} => current momentum value
    /// {momentumGain} => momentum gained from this card
    /// {momentumSpent} => momentum spent by this card
    /// Also supports dynamic threshold checks
    /// </summary>
    public string GetDynamicMomentumDescription(Character character)
    {
        string desc = momentumEffectDescription ?? string.Empty;
        if (string.IsNullOrEmpty(desc)) return desc;
        
        if (character != null)
        {
            int currentMomentum = character.GetMomentum();
            desc = desc.Replace("{momentum}", currentMomentum.ToString());
            
            // Parse momentum gain from description if present
            int momentumGain = MomentumEffectParser.ParseGainMomentum(desc);
            if (momentumGain > 0)
            {
                desc = desc.Replace("{momentumGain}", momentumGain.ToString());
            }
            
            // Parse momentum spent from description if present
            int spendAmount = MomentumEffectParser.ParseSpendMomentum(desc);
            if (spendAmount == -1) // Spend all
            {
                desc = desc.Replace("{momentumSpent}", currentMomentum.ToString());
            }
            else if (spendAmount > 0)
            {
                desc = desc.Replace("{momentumSpent}", Mathf.Min(spendAmount, currentMomentum).ToString());
            }
        }
        
        return desc;
    }
    
    /// <summary>
    /// Get dynamic combo description replacing placeholders and STR/DEX/INT division patterns
    /// Supported placeholders:
    /// {comboDamage} => comboAttackIncrease + scaling
    /// {comboGuard} => comboGuardIncrease + scaling (if applicable)
    /// {manaRefund} => comboManaRefund
    /// Also replaces literal patterns like STR/4, DEX/3, INT/2 with computed values
    /// </summary>
    public string GetDynamicComboDescription(Character character)
    {
        string desc = comboDescription ?? string.Empty;
        if (string.IsNullOrEmpty(desc)) return desc;
        
        // Compute scaling bonus similar to ComboSystem
        float scalingBonus = 0f;
        if (character != null)
        {
            switch (comboScaling)
            {
                case ComboScalingType.Strength:
                    scalingBonus = character.strength; break;
                case ComboScalingType.Dexterity:
                    scalingBonus = character.dexterity; break;
                case ComboScalingType.Intelligence:
                    scalingBonus = character.intelligence; break;
                case ComboScalingType.Momentum:
                    scalingBonus = 0f; break; // hook when available
                case ComboScalingType.DiscardPower:
                    scalingBonus = 0f; break; // hook when available
            }
        }
        if (comboScalingDivisor != 0f && scalingBonus != 0f)
            scalingBonus = scalingBonus / comboScalingDivisor;
        else if (comboScalingDivisor != 0f && scalingBonus == 0f)
            scalingBonus = 0f;
        
        float comboDamageTotal = Mathf.Max(0f, comboAttackIncrease + scalingBonus);
        float comboGuardTotal = Mathf.Max(0f, comboGuardIncrease + ((comboScaling == ComboScalingType.Strength && comboScalingDivisor > 0) ? scalingBonus : 0f));
        
        // Replace explicit placeholders
        desc = desc.Replace("{comboDamage}", comboDamageTotal.ToString("F0"));
        desc = desc.Replace("{comboGuard}", comboGuardTotal.ToString("F0"));
        desc = desc.Replace("{manaRefund}", comboManaRefund.ToString());

        // Compute combo buff-specific placeholders (e.g., Bolster stacks)
        int bolsterStacks = 0;
        if (comboBuffs != null && comboBuffs.Exists(b => string.Equals(b, "Bolster", System.StringComparison.OrdinalIgnoreCase)))
        {
            int baseStacks = 1;
            int extraStacks = 0;
            if (character != null && comboScalingDivisor > 0f)
            {
                switch (comboScaling)
                {
                    case ComboScalingType.Strength:
                        extraStacks = Mathf.FloorToInt(character.strength / comboScalingDivisor);
                        break;
                    case ComboScalingType.Dexterity:
                        extraStacks = Mathf.FloorToInt(character.dexterity / comboScalingDivisor);
                        break;
                    case ComboScalingType.Intelligence:
                        extraStacks = Mathf.FloorToInt(character.intelligence / comboScalingDivisor);
                        break;
                }
            }
            bolsterStacks = Mathf.Clamp(baseStacks + extraStacks, 1, 10);
        }
        // Replace new placeholders for buffs
        desc = desc.Replace("{bolsterStacks}", bolsterStacks > 0 ? bolsterStacks.ToString() : "");
        // Generic {comboBuff}: if only Bolster is configured, map to total stacks for convenience
        if (desc.Contains("{comboBuff}"))
        {
            string value = bolsterStacks > 0 ? bolsterStacks.ToString() : "";
            desc = desc.Replace("{comboBuff}", value);
        }
        
        // Also support STR/4, DEX/3, INT/2 literal patterns
        if (character != null)
        {
            desc = Regex.Replace(desc, @"STR\/(\d+)", m => {
                if (int.TryParse(m.Groups[1].Value, out int d) && d != 0)
                    return Mathf.FloorToInt(character.strength / d).ToString();
                return "0";
            });
            desc = Regex.Replace(desc, @"DEX\/(\d+)", m => {
                if (int.TryParse(m.Groups[1].Value, out int d) && d != 0)
                    return Mathf.FloorToInt(character.dexterity / d).ToString();
                return "0";
            });
            desc = Regex.Replace(desc, @"INT\/(\d+)", m => {
                if (int.TryParse(m.Groups[1].Value, out int d) && d != 0)
                    return Mathf.FloorToInt(character.intelligence / d).ToString();
                return "0";
            });
        }
        
        return desc;
    }

    /// <summary>
    /// Get card tooltip
    /// </summary>
    public string GetCardTooltip()
    {
        string tooltip = $"{cardName}\n";
        tooltip += $"Type: {category}\n";
        // Show cost - percentage for Skill cards with percentageManaCost, otherwise flat
        if (string.Equals(cardType, "Skill", System.StringComparison.OrdinalIgnoreCase) && percentageManaCost > 0)
        {
            tooltip += $"Cost: {percentageManaCost}% of Max Mana\n";
        }
        else
        {
            tooltip += $"Cost: {playCost} Mana\n";
        }
        
        if (damage > 0)
        {
            tooltip += $"Damage: {damage}";
            if (damageScaling.strengthScaling > 0)
                tooltip += $" + {damageScaling.strengthScaling * 100}% STR";
            tooltip += "\n";
        }
        
        if (block > 0)
        {
            tooltip += $"Guard: {block}";
            if (guardScaling.strengthScaling > 0)
                tooltip += $" + {guardScaling.strengthScaling * 100}% STR";
            tooltip += "\n";
        }
        
        if (isAoE)
        {
            tooltip += $"AoE: Hits {aoeTargets} enemies\n";
        }
        
        return tooltip;
    }
    
    /// <summary>
    /// Get CardType enum from string cardType
    /// </summary>
    public CardType GetCardTypeEnum()
    {
        switch (cardType.ToLower())
        {
            case "attack": return CardType.Attack;
            case "guard": return CardType.Guard;
            case "skill": return CardType.Skill;
            case "power": return CardType.Power;
            case "aura": return CardType.Aura;
            default: return CardType.Attack;
        }
    }
    
    /// <summary>
    /// Get mana cost (alias for compatibility)
    /// </summary>
    public int GetManaCost()
    {
        return playCost;
    }
    
    /// <summary>
    /// Calculate the card's current mana cost with embossing modifiers
    /// Formula: (Base + N_embossings) × (1 + Σ Multipliers)
    /// For Skill cards: cost is percentage-based (playCost% of maxMana)
    /// For Attack cards (Weapon/Spell): cost is flat
    /// Note: This calculates based on a Card instance with applied embossings
    /// </summary>
    public int GetCurrentManaCost(Card card, Character character = null)
    {
        int baseCost = playCost;
        
        // Skill cards use percentage-based cost if percentageManaCost is set
        if (string.Equals(cardType, "Skill", System.StringComparison.OrdinalIgnoreCase) && character != null)
        {
            if (percentageManaCost > 0)
            {
                // percentageManaCost is a percentage (e.g., 10 = 10% of maxMana)
                // Use CeilToInt to round up (e.g., 4.5 -> 5, not 4)
                float percentageCost = percentageManaCost / 100.0f;
                baseCost = Mathf.CeilToInt(character.maxMana * percentageCost);
            }
            // If percentageManaCost is 0, use playCost as flat cost (backward compatibility)
        }
        
        if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
            return baseCost;
        
        // Use EmbossingDatabase if available
        if (EmbossingDatabase.Instance != null)
        {
            return EmbossingDatabase.Instance.CalculateCardManaCost(card, baseCost);
        }
        
        // Fallback calculation if database not available
        int embossingCount = card.appliedEmbossings.Count;
        return baseCost + embossingCount;
    }
    
    /// <summary>
    /// Get formatted mana cost display with embossing increase
    /// Shows increase if card has embossings: "5 (+3)"
    /// </summary>
    public string GetManaCostDisplay(Card card, Character character = null)
    {
        int currentCost = GetCurrentManaCost(card, character);
        
        // For Skill cards, show percentage if percentageManaCost is set
        if (string.Equals(cardType, "Skill", System.StringComparison.OrdinalIgnoreCase) && percentageManaCost > 0)
        {
            if (character != null)
            {
                float percentageCost = percentageManaCost / 100.0f;
                int calculatedCost = Mathf.RoundToInt(character.maxMana * percentageCost);
                if (currentCost == calculatedCost && (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0))
                    return $"{percentageManaCost}% ({calculatedCost})";
            }
            else
            {
                return $"{percentageManaCost}%";
            }
        }
        
        // For Attack/Guard cards or Skill cards with flat cost, use playCost
        int baseCost = playCost;
        
        if (currentCost == baseCost)
            return currentCost.ToString();
        
        // Show increase: "5 (+3)"
        int increase = currentCost - baseCost;
        return $"{currentCost} <color=#FF6B6B>(+{increase})</color>";
    }
    
    /// <summary>
    /// Get mana cost breakdown for tooltip
    /// Shows how embossings affect the mana cost
    /// </summary>
    public string GetManaCostBreakdown(Card card, Character character = null)
    {
        // For Skill cards with percentage cost, show percentage-based calculation
        if (string.Equals(cardType, "Skill", System.StringComparison.OrdinalIgnoreCase) && percentageManaCost > 0 && character != null)
        {
            float percentageCost = percentageManaCost / 100.0f;
            int calculatedCost = Mathf.RoundToInt(character.maxMana * percentageCost);
            
            if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
                return $"Mana Cost: {percentageManaCost}% of Max Mana ({calculatedCost})";
        }
        
        // For Attack/Guard cards or Skill cards with flat cost, use playCost
        int baseCost = playCost;
        
        if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
            return $"Mana Cost: {baseCost}";
        
        if (EmbossingDatabase.Instance != null)
        {
            return EmbossingDatabase.Instance.GetManaCostBreakdown(card, baseCost);
        }
        
        int finalCost = GetCurrentManaCost(card, character);
        return $"Base: {baseCost}\nWith {card.appliedEmbossings.Count} Embossings: {finalCost}";
    }
    
    /// <summary>
    /// Get base damage as float (for combat calculations)
    /// </summary>
    public float GetBaseDamage()
    {
        return (float)damage;
    }
    
    /// <summary>
    /// Get base guard as float (for combat calculations)
    /// </summary>
    public float GetBaseGuard()
    {
        return (float)block;
    }
    
    /// <summary>
    /// Convert this CardDataExtended to a Card object (for backward compatibility during migration)
    /// TEMPORARY - will be removed after full migration
    /// </summary>
    [System.Obsolete("Use CardDataExtended directly instead of converting to Card")]
    public Card ToCard()
    {
        var card = new Card
        {
            cardName = this.cardName,
            groupKey = this.groupKey,
            description = this.description,
            cardType = GetCardTypeEnum(),
            // For Skill cards with percentage cost, store the percentage value in manaCost
            // For other cards, use playCost
            manaCost = (string.Equals(cardType, "Skill", System.StringComparison.OrdinalIgnoreCase) && this.percentageManaCost > 0) 
                ? this.percentageManaCost 
                : this.playCost,
            baseDamage = (float)this.damage,
            baseGuard = (float)this.block,
            isMultiHit = this.isMultiHit,
            hitCount = this.hitCount,
            primaryDamageType = this.primaryDamageType,
            cardArt = this.cardImage,
            cardArtName = this.cardName,
            scalesWithMeleeWeapon = this.scalesWithMeleeWeapon,
            scalesWithProjectileWeapon = this.scalesWithProjectileWeapon,
            scalesWithSpellWeapon = this.scalesWithSpellWeapon,
            isAoE = this.isAoE,
            aoeTargets = this.aoeTargets,
            aoeRowScope = this.aoeRowScope,
            requirements = this.requirements,
            tags = this.tags != null ? new List<string>(this.tags) : new List<string>(),
            additionalDamageTypes = this.additionalDamageTypes != null ? new List<DamageType>(this.additionalDamageTypes) : new List<DamageType>(),
            damageScaling = this.damageScaling,
            guardScaling = this.guardScaling,
            effects = this.effects != null ? new List<CardEffect>(this.effects) : new List<CardEffect>(),
            comboWith = this.comboWith,
            comboDescription = this.comboDescription,
            // Note: momentumEffectDescription is only in CardDataExtended, not in Card class
            comboEffect = this.comboEffect,
            comboHighlightType = this.comboHighlightType,
            comboAilmentId = this.comboAilment,
            comboAilmentPortion = this.comboAilmentPortion,
            comboAilmentDuration = this.comboAilmentDuration,
            consumeAilmentEnabled = this.consumeAilment,
            consumeAilmentId = this.consumeAilmentId,
            channelingBonusEnabled = this.channelingBonusEnabled,
            channelingMinStacks = this.channelingMinStacks,
            channelingAdditionalGuard = this.channelingAdditionalGuard,
            channelingDamageIncreasedPercent = this.channelingDamageIncreasedPercent,
            channelingDamageMorePercent = this.channelingDamageMorePercent,
            channelingGuardIncreasedPercent = this.channelingGuardIncreasedPercent,
            channelingGuardMorePercent = this.channelingGuardMorePercent,
            embossingSlots = this.embossingSlots,
            appliedEmbossings = this.appliedEmbossings != null ? new List<EmbossingInstance>(this.appliedEmbossings) : new List<EmbossingInstance>(), // Copy embossings from CardDataExtended
            cardLevel = this.cardLevel,
            cardExperience = this.cardExperience,
            preparedCardDamageBase = this.preparedCardDamageBase,
            preparedCardDamageScaling = this.preparedCardDamageScaling,
            preparedCardPoisonBase = this.preparedCardPoisonBase,
            preparedCardGuardBase = this.preparedCardGuardBase,
            preparedCardGuardScaling = this.preparedCardGuardScaling,
            consumedCardDamageBase = this.consumedCardDamageBase,
            consumedCardDamageScaling = this.consumedCardDamageScaling
        };
        
        // Set source reference for preparation system
        card.sourceCardData = this;
        
        return card;
    }
}

public enum ComboScalingType
{
    None,
    Strength,
    Dexterity,
    Intelligence,
    Momentum,
    DiscardPower
}

public enum ComboLogicType
{
    Additive,
    Instead
}

public enum AilmentId
{
    None,
    Poison,
    Burn,
    Chill,
    Freeze,
    Stun,
    Vulnerable,
    Weak,
    Frail,
    Slow,
    Crumble,
    Curse
}

