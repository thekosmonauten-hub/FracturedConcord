using UnityEngine;
using System.Collections.Generic;

public enum CardType
{
    Attack,
    Guard,
    Skill,
    Power,
    Aura
}

[System.Serializable]
public class AttributeScaling
{
    [Tooltip("Multiplicative scaling applied per point of the attribute.")]
    public float strengthScaling = 0f;
    public float dexterityScaling = 0f;
    public float intelligenceScaling = 0f;

    [Tooltip("Additive scaling applied as attribute/divisor (e.g. 15 -> STAT/15).")]
    public float strengthDivisor = 0f;
    public float dexterityDivisor = 0f;
    public float intelligenceDivisor = 0f;
    
    public float CalculateScalingBonus(Character character)
    {
        float bonus = 0f;
        if (character == null)
            return bonus;

        bonus += character.strength * strengthScaling;
        bonus += character.dexterity * dexterityScaling;
        bonus += character.intelligence * intelligenceScaling;

        if (strengthDivisor > 0f)
            bonus += character.strength / strengthDivisor;
        if (dexterityDivisor > 0f)
            bonus += character.dexterity / dexterityDivisor;
        if (intelligenceDivisor > 0f)
            bonus += character.intelligence / intelligenceDivisor;
        return bonus;
    }
}

[System.Serializable]
public class Card
{
    [Header("Card Properties")]
    public string cardName;
    
    [Tooltip("Group key for card variants. Cards with the same groupKey are treated as the same card type for embossing/upgrades. Defaults to cardName if empty.")]
    public string groupKey = "";
    
    public string description; // Static description template
    public CardType cardType;
    public int manaCost;
    public float baseDamage;
    public float baseGuard;
    public DamageType primaryDamageType = DamageType.Physical;
    
    [Header("Source Reference")]
    [System.NonSerialized] // Don't serialize this to avoid circular references
    public CardDataExtended sourceCardData; // Reference to the CardDataExtended that created this Card
    
    [Header("Visual Assets")]
    public Sprite cardArt; // Card artwork sprite
    public string cardArtName; // Name of sprite to load from Resources (for JSON cards)
    
    [Header("Weapon Scaling")]
    public bool scalesWithMeleeWeapon = false;
    public bool scalesWithProjectileWeapon = false;
    public bool scalesWithSpellWeapon = false;
    
    [Header("Area of Effect")]
    public bool isAoE = false; // Area of Effect - hits multiple enemies
    public int aoeTargets = 3; // Number of enemies hit (default to all 3)
    public AoERowScope aoeRowScope = AoERowScope.BothRows; // Row scope for AoE
    
    [Header("Multi-Hit")]
    public bool isMultiHit = false; // If true, hits the same target multiple times
    public int hitCount = 2; // Number of times to hit (only used if isMultiHit is true)
    
    [Header("Requirements")]
    public CardRequirements requirements = new CardRequirements();
    
    [Header("Card Tags")]
    public List<string> tags = new List<string>();
    
    [Header("Additional Properties")]
    public List<DamageType> additionalDamageTypes = new List<DamageType>();
    
    [Header("Attribute Scaling")]
    public AttributeScaling damageScaling = new AttributeScaling();
    public AttributeScaling guardScaling = new AttributeScaling(); // For Guard cards
    
    [Header("Card Effects")]
    public List<CardEffect> effects = new List<CardEffect>();
    
    [Header("Combo Properties")]
    [Tooltip("Cards or types this card can combo with (comma-separated)")]
    public string comboWith = "";
    [Tooltip("Description of what happens when this card combos")]
    public string comboDescription = "";
    [Tooltip("Effect that triggers when this card combos")]
    public string comboEffect = "";
    [Tooltip("Type of combo highlighting: 'specific', 'type', 'tag'")]
    public string comboHighlightType = "specific";
    
    [Header("Combo Ailment (Runtime)")]
    public AilmentId comboAilmentId = AilmentId.None;
    public float comboAilmentPortion = 0f;
    public int comboAilmentDuration = 0;
    
    [Header("Consume Ailment (Runtime)")]
    public bool consumeAilmentEnabled = false;
    public AilmentId consumeAilmentId = AilmentId.None;
    
    [System.NonSerialized] public bool channelingActive;
    [System.NonSerialized] public int channelingStacks;
    [System.NonSerialized] public bool channelingStartedThisCast;
    [System.NonSerialized] public bool channelingStoppedThisCast;
    [System.NonSerialized] public string channelingGroupKey = string.Empty;
    public bool channelingBonusEnabled;
    public int channelingMinStacks = 2;
    public float channelingAdditionalGuard = 0f;
    public float channelingDamageIncreasedPercent = 0f;
    public float channelingDamageMorePercent = 0f;
    public float channelingGuardIncreasedPercent = 0f;
    public float channelingGuardMorePercent = 0f;
    [System.NonSerialized] public bool channelingBonusApplied;
    
    [Header("Embossing System")]
    [Tooltip("Number of embossing slots this card has")]
    public int embossingSlots = 1;
    
    [Tooltip("List of applied embossing instances (includes level and XP)")]
    public List<EmbossingInstance> appliedEmbossings = new List<EmbossingInstance>();
    
    [Header("Card Leveling System")]
    [Tooltip("Current level of this card (1-20). Tracked per groupKey in CardExperienceManager.")]
    public int cardLevel = 1;
    
    [Tooltip("Current experience points for this card")]
    public int cardExperience = 0;
    
    /// <summary>
    /// Get the group key for this card. Returns cardName if groupKey is not set.
    /// Used for grouping card variants (base, upgraded, foil, etc.) together.
    /// </summary>
    public string GetGroupKey()
    {
        return string.IsNullOrEmpty(groupKey) ? cardName : groupKey;
    }
    
    /// <summary>
    /// Check if this card has any empty embossing slots
    /// </summary>
    public bool HasEmptyEmbossingSlot()
    {
        if (embossingSlots <= 0) return false;
        if (appliedEmbossings == null) return embossingSlots > 0;
        return appliedEmbossings.Count < embossingSlots;
    }
    
    /// <summary>
    /// Get the number of empty embossing slots
    /// </summary>
    public int GetEmptySlotCount()
    {
        if (appliedEmbossings == null) return embossingSlots;
        return Mathf.Max(0, embossingSlots - appliedEmbossings.Count);
    }
    
    /// <summary>
    /// Get the number of filled embossing slots
    /// </summary>
    public int GetFilledSlotCount()
    {
        if (appliedEmbossings == null) return 0;
        return appliedEmbossings.Count;
    }
    
    /// <summary>
    /// Check if a card can have embossing applied
    /// </summary>
    public bool CanEmboss()
    {
        return HasEmptyEmbossingSlot();
    }
    
    /// <summary>
    /// Add experience to all embossings on this card.
    /// Returns true if any embossing leveled up.
    /// </summary>
    public bool AddEmbossingExperience(int amount)
    {
        if (appliedEmbossings == null || appliedEmbossings.Count == 0) return false;
        
        bool anyLeveledUp = false;
        foreach (EmbossingInstance embossing in appliedEmbossings)
        {
            if (embossing != null && embossing.AddExperience(amount))
            {
                anyLeveledUp = true;
            }
        }
        
        return anyLeveledUp;
    }
    
    /// <summary>
    /// Get the combined embossing bonus multiplier.
    /// Averages all embossing multipliers.
    /// </summary>
    public float GetEmbossingBonusMultiplier()
    {
        if (appliedEmbossings == null || appliedEmbossings.Count == 0) return 1.0f;
        
        float totalMultiplier = 0f;
        int count = 0;
        
        foreach (EmbossingInstance embossing in appliedEmbossings)
        {
            if (embossing != null)
            {
                totalMultiplier += embossing.GetLevelBonusMultiplier();
                count++;
            }
        }
        
        return count > 0 ? totalMultiplier / count : 1.0f;
    }
    
    /// <summary>
    /// Get the total combined multiplier from card level + embossings.
    /// </summary>
    public float GetTotalBonusMultiplier()
    {
        float cardBonus = GetLevelBonusMultiplier();
        float embossingBonus = GetEmbossingBonusMultiplier();
        
        // Combine multiplicatively: (1 + card%) * (1 + embossing%)
        return cardBonus * embossingBonus;
    }
    
    /// <summary>
    /// Calculate the card's current mana cost with embossing modifiers
    /// Formula: (Base + N_embossings) × (1 + Σ Multipliers)
    /// </summary>
    public int GetCurrentManaCost()
    {
        if (appliedEmbossings == null || appliedEmbossings.Count == 0)
            return manaCost;
        
        // Use EmbossingDatabase if available
        if (EmbossingDatabase.Instance != null)
        {
            return EmbossingDatabase.Instance.CalculateCardManaCost(this, manaCost);
        }
        
        // Fallback calculation if database not available
        int embossingCount = appliedEmbossings.Count;
        
        // Note: This fallback won't have access to embossing definitions
        // In production, EmbossingDatabase should always be available
        
        return manaCost + embossingCount;
    }
    
    /// <summary>
    /// Get formatted mana cost display (shows increase if embossed)
    /// </summary>
    public string GetManaCostDisplay()
    {
        int currentCost = GetCurrentManaCost();
        
        if (currentCost == manaCost)
            return currentCost.ToString();
        
        // Show increase: "5 (+3)"
        int increase = currentCost - manaCost;
        return $"{currentCost} <color=#FF6B6B>(+{increase})</color>";
    }
    
    /// <summary>
    /// Calculate the damage/effect bonus multiplier based on card level.
    /// Max level (20) provides +10% bonus (1.10x multiplier).
    /// </summary>
    public float GetLevelBonusMultiplier()
    {
        // Level 1 = 1.00x (no bonus)
        // Level 20 = 1.10x (+10% bonus)
        // Linear scaling: ~0.5263% per level
        return 1.0f + ((cardLevel - 1) * 0.005263f);
    }
    
    /// <summary>
    /// Get experience required for next level.
    /// Uses exponential curve for progression.
    /// </summary>
    public int GetRequiredExperienceForNextLevel()
    {
        if (cardLevel >= 20) return 0; // Max level
        
        // Base XP: 100
        // Scaling: 1.15x per level (exponential growth)
        return Mathf.RoundToInt(100f * Mathf.Pow(1.15f, cardLevel - 1));
    }
    
    /// <summary>
    /// Check if card is ready to level up.
    /// </summary>
    public bool CanLevelUp()
    {
        return cardLevel < 20 && cardExperience >= GetRequiredExperienceForNextLevel();
    }
    
    /// <summary>
    /// Add experience to this card and handle level ups.
    /// Also gives experience to all applied embossings.
    /// Returns true if card or any embossing leveled up.
    /// </summary>
    public bool AddExperience(int amount, bool shareWithEmbossings = true)
    {
        if (cardLevel >= 20 && (!shareWithEmbossings || appliedEmbossings == null || appliedEmbossings.Count == 0))
        {
            return false; // Nothing to level up
        }
        
        bool anyLevelUp = false;
        
        // Add experience to card
        if (cardLevel < 20)
        {
            cardExperience += amount;
            
            // Check for level ups (handle multiple levels if enough XP)
            while (CanLevelUp())
            {
                int requiredXP = GetRequiredExperienceForNextLevel();
                cardExperience -= requiredXP;
                cardLevel++;
                anyLevelUp = true;
                
                Debug.Log($"[Card Level Up] {cardName} reached level {cardLevel}! Bonus: {GetLevelBonusMultiplier():P1}");
            }
        }
        
        // Also give experience to embossings
        if (shareWithEmbossings && AddEmbossingExperience(amount))
        {
            anyLevelUp = true;
        }
        
        return anyLevelUp;
    }
    
    // Check if character can use this card
    public bool CanUseCard(Character character)
    {
        // Check requirements
        if (!requirements.MeetsRequirements(character)) return false;
        
        // Check mana cost
        if (manaCost > 0 && character.mana < manaCost) return false;
        
        return true;
    }
    
    // Get detailed requirement check
    public (bool canUse, string reason) CheckCardUsage(Character character)
    {
        var (meetsReqs, reqReason) = requirements.CheckRequirements(character);
        if (!meetsReqs)
            return (false, reqReason);
        
        if (manaCost > 0 && character.mana < manaCost)
            return (false, $"Requires {manaCost} Mana (has {character.mana})");
        
        return (true, "Can use card");
    }
    
    // Get weapon scaling damage for attack cards
    public float GetWeaponScalingDamage(Character character)
    {
        if (cardType != CardType.Attack) return 0f;
        
        float weaponDamage = 0f;
        
        if (scalesWithMeleeWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Melee);
        
        if (scalesWithProjectileWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Projectile);
        
        if (scalesWithSpellWeapon)
            weaponDamage += character.weapons.GetWeaponDamage(WeaponType.Spell);
        
        return weaponDamage;
    }
    
    // Get dynamic description with actual calculated values
    public string GetDynamicDescription(Character character)
    {
        if (string.IsNullOrEmpty(description))
            return GetCardTooltip();
        
        string dynamicDesc = description;
        
        // Replace damage placeholders with actual calculated values
        if (cardType == CardType.Attack && baseDamage > 0)
        {
            float totalDamage = DamageCalculator.CalculateCardDamage(this, character, character.weapons.meleeWeapon);
            
            // Debug logging to identify the issue
            Debug.Log($"<color=yellow>Card {cardName} Damage Calculation:</color>");
            Debug.Log($"  Base Damage: {baseDamage}");
            Debug.Log($"  Character: {(character != null ? character.characterName : "NULL")}");
            Debug.Log($"  Melee Weapon: {(character?.weapons?.meleeWeapon != null ? character.weapons.meleeWeapon.weaponName : "NULL")}");
            Debug.Log($"  Scales with Melee: {scalesWithMeleeWeapon}");
            Debug.Log($"  Strength: {character?.strength ?? 0}");
            Debug.Log($"  Strength Scaling: {damageScaling.strengthScaling}");
            Debug.Log($"  Total Damage: {totalDamage}");
            
            dynamicDesc = dynamicDesc.Replace("{damage}", totalDamage.ToString("F0"));
            dynamicDesc = dynamicDesc.Replace("{baseDamage}", baseDamage.ToString("F0"));
        }
        
        // Replace guard placeholders with actual calculated values
        if (cardType == CardType.Guard && baseGuard > 0)
        {
            float totalGuard = DamageCalculator.CalculateGuardAmount(this, character);
            dynamicDesc = dynamicDesc.Replace("{guard}", totalGuard.ToString("F0"));
            dynamicDesc = dynamicDesc.Replace("{baseGuard}", baseGuard.ToString("F0"));
        }
        
        // Replace attribute scaling placeholders
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
        
        // Replace weapon damage placeholders
        if (scalesWithMeleeWeapon)
        {
            float weaponDamage = character.weapons.GetWeaponDamage(WeaponType.Melee);
            dynamicDesc = dynamicDesc.Replace("{weaponDamage}", weaponDamage.ToString("F0"));
        }
        
        // Replace AoE target count
        if (isAoE)
        {
            dynamicDesc = dynamicDesc.Replace("{aoeTargets}", aoeTargets.ToString());
        }
        
        // Replace effect-based scaling placeholders
        if (effects != null && effects.Count > 0)
        {
            foreach (var effect in effects)
            {
                // Handle status effect scaling with Strength (like Vulnerability)
                if (effect.effectType == EffectType.ApplyStatus && effect.effectName == "Vulnerability")
                {
                    // Calculate total Vulnerability: base value + 1 per 15 Strength
                    float baseVulnerability = effect.value;
                    float strengthBonus = Mathf.Floor(character.strength / 15f);
                    float totalVulnerability = baseVulnerability + strengthBonus;
                    
                    dynamicDesc = dynamicDesc.Replace("{vulnerability}", totalVulnerability.ToString("F0"));
                    dynamicDesc = dynamicDesc.Replace("{baseVulnerability}", baseVulnerability.ToString("F0"));
                    dynamicDesc = dynamicDesc.Replace("{vulnerabilityBonus}", strengthBonus.ToString("F0"));
                }
                
                // Add more effect-based placeholders here as needed
                // Example: {poison}, {burn}, {stun}, etc.
            }
        }
        
        return dynamicDesc;
    }
    
    // Get card tooltip with requirements and scaling info
    public string GetCardTooltip()
    {
        string tooltip = $"{cardName}\n";
        tooltip += $"Type: {cardType}\n";
        tooltip += $"Cost: {manaCost} Mana\n";
        
        if (baseDamage > 0)
        {
            tooltip += $"Damage: {baseDamage}";
            if (damageScaling.strengthScaling > 0)
                tooltip += $" + {damageScaling.strengthScaling * 100}% STR";
            if (damageScaling.strengthDivisor > 0)
                tooltip += $" + STR/{damageScaling.strengthDivisor}";
            if (damageScaling.dexterityScaling > 0)
                tooltip += $" + {damageScaling.dexterityScaling * 100}% DEX";
            if (damageScaling.dexterityDivisor > 0)
                tooltip += $" + DEX/{damageScaling.dexterityDivisor}";
            if (damageScaling.intelligenceScaling > 0)
                tooltip += $" + {damageScaling.intelligenceScaling * 100}% INT";
            if (damageScaling.intelligenceDivisor > 0)
                tooltip += $" + INT/{damageScaling.intelligenceDivisor}";
            tooltip += "\n";
        }
        
        if (baseGuard > 0)
        {
            tooltip += $"Guard: {baseGuard}";
            if (guardScaling.strengthScaling > 0)
                tooltip += $" + {guardScaling.strengthScaling * 100}% STR";
            if (guardScaling.strengthDivisor > 0)
                tooltip += $" + STR/{guardScaling.strengthDivisor}";
            if (guardScaling.dexterityScaling > 0)
                tooltip += $" + {guardScaling.dexterityScaling * 100}% DEX";
            if (guardScaling.dexterityDivisor > 0)
                tooltip += $" + DEX/{guardScaling.dexterityDivisor}";
            if (guardScaling.intelligenceScaling > 0)
                tooltip += $" + {guardScaling.intelligenceScaling * 100}% INT";
            if (guardScaling.intelligenceDivisor > 0)
                tooltip += $" + INT/{guardScaling.intelligenceDivisor}";
            tooltip += "\n";
        }
        
        if (isAoE)
        {
            tooltip += $"AoE: Hits {aoeTargets} enemies\n";
        }
        
        if (scalesWithMeleeWeapon)
            tooltip += "Scales with Melee Weapon\n";
        if (scalesWithProjectileWeapon)
            tooltip += "Scales with Projectile Weapon\n";
        if (scalesWithSpellWeapon)
            tooltip += "Scales with Spell Weapon\n";
        
        return tooltip;
    }
}
