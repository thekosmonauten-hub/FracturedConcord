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
    
    [Header("Attribute Scaling")]
    public AttributeScaling damageScaling = new AttributeScaling();
    public AttributeScaling guardScaling = new AttributeScaling();
    
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
    
    // IMPORTANT: Add all Card methods here
    
    /// <summary>
    /// Check if character can use this card
    /// </summary>
    public bool CanUseCard(Character character)
    {
        if (!requirements.MeetsRequirements(character)) return false;
        if (playCost > 0 && character.mana < playCost) return false;
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
        
        if (playCost > 0 && character.mana < playCost)
            return (false, $"Requires {playCost} Mana (has {character.mana})");
        
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
            float totalDamage = damage;
            totalDamage += damageScaling.CalculateScalingBonus(character);
            totalDamage += GetWeaponScalingDamage(character);
            
            dynamicDesc = dynamicDesc.Replace("{damage}", totalDamage.ToString("F0"));
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
        dynamicDesc = dynamicDesc.Replace("{manaCost}", playCost.ToString());
        dynamicDesc = dynamicDesc.Replace("{cost}", playCost.ToString());
        
        // Final fallback: ensure {aoeTargets} is always replaced even if isAoE wasn't set
        dynamicDesc = dynamicDesc.Replace("{aoeTargets}", (aoeTargets > 0 ? aoeTargets.ToString() : "1"));
        
        return dynamicDesc;
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
        tooltip += $"Cost: {playCost} Mana\n";
        
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
    /// Note: This calculates based on a Card instance with applied embossings
    /// </summary>
    public int GetCurrentManaCost(Card card)
    {
        if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
            return playCost;
        
        // Use EmbossingDatabase if available
        if (EmbossingDatabase.Instance != null)
        {
            return EmbossingDatabase.Instance.CalculateCardManaCost(card, playCost);
        }
        
        // Fallback calculation if database not available
        int embossingCount = card.appliedEmbossings.Count;
        return playCost + embossingCount;
    }
    
    /// <summary>
    /// Get formatted mana cost display with embossing increase
    /// Shows increase if card has embossings: "5 (+3)"
    /// </summary>
    public string GetManaCostDisplay(Card card)
    {
        int currentCost = GetCurrentManaCost(card);
        
        if (currentCost == playCost)
            return currentCost.ToString();
        
        // Show increase: "5 (+3)"
        int increase = currentCost - playCost;
        return $"{currentCost} <color=#FF6B6B>(+{increase})</color>";
    }
    
    /// <summary>
    /// Get mana cost breakdown for tooltip
    /// Shows how embossings affect the mana cost
    /// </summary>
    public string GetManaCostBreakdown(Card card)
    {
        if (card == null || card.appliedEmbossings == null || card.appliedEmbossings.Count == 0)
            return $"Mana Cost: {playCost}";
        
        if (EmbossingDatabase.Instance != null)
        {
            return EmbossingDatabase.Instance.GetManaCostBreakdown(card, playCost);
        }
        
        int finalCost = GetCurrentManaCost(card);
        return $"Base: {playCost}\nWith {card.appliedEmbossings.Count} Embossings: {finalCost}";
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
            manaCost = this.playCost,
            baseDamage = (float)this.damage,
            baseGuard = (float)this.block,
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
            appliedEmbossings = new List<EmbossingInstance>(), // Start with empty embossings
            cardLevel = this.cardLevel,
            cardExperience = this.cardExperience
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

