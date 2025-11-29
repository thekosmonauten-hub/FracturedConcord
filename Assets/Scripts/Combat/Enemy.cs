using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Enemy
{
    [Header("Basic Information")]
    public string enemyName;
    public int maxHealth;
    public int currentHealth;
    public int baseDamage;
    public float accuracyRating = 100f; // for hit chance vs player evasion
    public float evasionRating = 0f; // reserved for future
    public EnemyRarity rarity = EnemyRarity.Normal;
    
    [Header("Combat Stats")]
    public float criticalChance = 0.05f; // 5% base
    public float criticalMultiplier = 1.5f;
    
    [Header("Energy System")]
    public bool usesEnergy = false;
    public float maxEnergy = 0f;
    public float currentEnergy = 0f;
    public float energyRegenPerTurn = 0f;
    public float chillDrainPerMagnitude = 1f;
    public float slowDrainPerMagnitude = 1.5f;
    
    [Header("Stagger System")]
    public float staggerThreshold = 100f; // Amount of stagger needed to trigger stun
    public float currentStagger = 0f; // Current stagger meter value
    public float staggerDecayPerTurn = 0f; // How much stagger decays per turn (0 = no decay)
    
    [Header("Guard System")]
    public float currentGuard = 0f; // Current guard amount
    public float maxGuard = 0f; // Maximum guard (based on max health)
    public float guardPersistenceFraction = 0.25f; // Fraction of guard retained between turns
    public float defendGuardPercent = 0.1f; // Percentage of max health gained as guard when defending (default 10%)
    
    [Header("AI Behavior")]
    public EnemyIntent currentIntent;
    public int intentDamage;
    
    [Header("Delayed Actions")]
    [Tooltip("Actions queued for future turns (for Time-Lagged modifier, etc.)")]
    public System.Collections.Generic.List<DelayedAction> delayedActions = new System.Collections.Generic.List<DelayedAction>();
    
    [Header("Monster Modifiers")]
    [Tooltip("Visible modifiers applied to this enemy (Magic: 1, Rare: 2-4)")]
    public List<MonsterModifier> modifiers = new List<MonsterModifier>();
    
    [Header("Hidden Rarity Stats")]
    [Tooltip("Experience multiplier from rarity (hidden)")]
    public float experienceMultiplier = 1f;
    [Tooltip("Item quantity multiplier from rarity (hidden)")]
    public float quantityMultiplier = 1f;
    [Tooltip("Item rarity multiplier from rarity (hidden)")]
    public float rarityMultiplier = 1f;
    [Tooltip("Item level bonus from rarity (hidden)")]
    public int itemLevelBonus = 0;
    [Tooltip("Size multiplier from rarity (hidden, for Rare enemies)")]
    public float sizeMultiplier = 1f;
    
    [System.Serializable]
    private class StackEntry
    {
        public StackType type;
        public int baseMaxStacks = 10;
        public int bonusMaxStacks = 0;
        public int currentStacks = 0;
    }

    [System.Serializable]
    private class EnemyStackCollection
    {
        [SerializeField] private List<StackEntry> stackEntries = new List<StackEntry>();
        private readonly Dictionary<StackType, StackEntry> lookup = new Dictionary<StackType, StackEntry>();
        private Action<StackType, int> onStacksChanged;

        public void SetChangeCallback(Action<StackType, int> callback)
        {
            onStacksChanged = callback;
        }

        private void EnsureStacks()
        {
            if (lookup.Count == 0)
            {
                foreach (var entry in stackEntries)
                {
                    if (entry != null && !lookup.ContainsKey(entry.type))
                    {
                        lookup.Add(entry.type, entry);
                    }
                }

                foreach (StackType type in System.Enum.GetValues(typeof(StackType)))
                {
                    if (!lookup.ContainsKey(type))
                    {
                        var entry = new StackEntry { type = type };
                        lookup.Add(type, entry);
                        stackEntries.Add(entry);
                    }
                }
            }
        }

        public void Reset()
        {
            EnsureStacks();
            foreach (var entry in lookup.Values)
            {
                int previous = entry.currentStacks;
                entry.currentStacks = 0;
                entry.bonusMaxStacks = 0;
                if (previous != entry.currentStacks)
                {
                    Notify(entry.type, entry);
                }
            }
        }

        private StackEntry GetEntry(StackType type)
        {
            EnsureStacks();
            return lookup[type];
        }

        private int GetMaxStacks(StackEntry entry) => entry.baseMaxStacks + entry.bonusMaxStacks;

        public int GetStacks(StackType type) => GetEntry(type).currentStacks;

        public void SetBonusMaxStacks(StackType type, int bonus)
        {
            var entry = GetEntry(type);
            int previous = entry.currentStacks;
            entry.bonusMaxStacks = Mathf.Max(0, bonus);
            entry.currentStacks = Mathf.Min(entry.currentStacks, GetMaxStacks(entry));
            if (entry.currentStacks != previous)
            {
                Notify(type, entry);
            }
        }

        public void AddStacks(StackType type, int amount)
        {
            var entry = GetEntry(type);
            int maxStacks = GetMaxStacks(entry);
            int previous = entry.currentStacks;
            entry.currentStacks = Mathf.Clamp(previous + Mathf.Abs(amount), 0, maxStacks);
            if (entry.currentStacks != previous)
            {
                Notify(type, entry);
            }
        }

        public void RemoveStacks(StackType type, int amount)
        {
            var entry = GetEntry(type);
            int previous = entry.currentStacks;
            entry.currentStacks = Mathf.Max(0, entry.currentStacks - Mathf.Abs(amount));
            if (entry.currentStacks != previous)
            {
                Notify(type, entry);
            }
        }

        public void ClearStacks(StackType type)
        {
            var entry = GetEntry(type);
            if (entry.currentStacks == 0) return;
            entry.currentStacks = 0;
            Notify(type, entry);
        }

        public float GetDamageMoreMultiplier()
        {
            int stacks = GetStacks(StackType.Agitate);
            return 1f + (0.02f * stacks);
        }

        public (float attack, float cast, float move) GetSpeedBonuses()
        {
            float percent = 2f * GetStacks(StackType.Agitate);
            return (percent, percent, percent);
        }

        public float GetToleranceDamageMultiplier()
        {
            float reduction = 0.03f * GetStacks(StackType.Tolerance);
            return Mathf.Clamp01(1f - reduction);
        }

        public float GetCritChanceBonus()
        {
            return 2f * GetStacks(StackType.Potential);
        }

        public float GetCritMultiplierBonus()
        {
            return 1f + (0.02f * GetStacks(StackType.Potential));
        }

        private void Notify(StackType type, StackEntry entry)
        {
            onStacksChanged?.Invoke(type, entry.currentStacks);
        }
    }

    [SerializeField]
    private EnemyStackCollection stackCollection = new EnemyStackCollection();
    private bool stackCallbackRegistered;
    private float bolsterStacks;
    private float strengthStacks;
    private float dexterityStacks;
    private float intelligenceStacks;

    private const float BolsterDamageReductionPerStack = 0.02f;
    private const float BolsterStacksCap = 10f;
    private const float StrengthDamagePerStack = 0.04f;
    private const float DexterityAccuracyPerStack = 0.02f;
    private const float IntelligenceCritChancePerStack = 0.02f;
    private const float IntelligenceCritMultiplierPerStack = 0.02f;

    public event Action<StackType, int> OnStacksChanged;
    public event Action<float, float> OnEnergyChanged;

    public Enemy()
    {
        EnsureStackCallback();
        stackCollection.Reset();
        ResetBuffs();
    }

    public Enemy(string name, int health, int damage)
        : this()
    {
        enemyName = name;
        maxHealth = health;
        currentHealth = health;
        baseDamage = damage;
        
        // Set initial intent
        SetIntent();
    }

    /// <summary>
    /// Apply rarity scaling including hidden base modifiers and visible monster modifiers.
    /// </summary>
    public void ApplyRarityScaling(EnemyRarity r)
    {
        rarity = r;
        modifiers.Clear(); // Clear any existing modifiers
        
        // Initialize hidden stats to defaults
        experienceMultiplier = 1f;
        quantityMultiplier = 1f;
        rarityMultiplier = 1f;
        itemLevelBonus = 0;
        sizeMultiplier = 1f;
        
        switch (rarity)
        {
            case EnemyRarity.Magic:
                ApplyMagicRarityModifiers();
                break;
            case EnemyRarity.Rare:
                ApplyRareRarityModifiers();
                break;
            case EnemyRarity.Unique:
                ApplyUniqueRarityModifiers();
                break;
            case EnemyRarity.Normal:
            default:
                // Normal enemies have no modifiers
                break;
        }
        
        // Apply modifier stat changes after all modifiers are assigned
        ApplyModifierStats();
    }
    
    private void ApplyMagicRarityModifiers()
    {
        // Apply hidden base modifiers
        experienceMultiplier = MonsterRarityModifiers.Magic.ExperienceMultiplier;
        quantityMultiplier = MonsterRarityModifiers.Magic.QuantityMultiplier;
        rarityMultiplier = MonsterRarityModifiers.Magic.RarityMultiplier;
        itemLevelBonus = MonsterRarityModifiers.Magic.ItemLevelBonus;
        
        // Apply hidden life multiplier (148% more = 2.48x total)
        float lifeMultiplier = MonsterRarityModifiers.Magic.LifeMultiplier;
        maxHealth = Mathf.CeilToInt(maxHealth * (1f + lifeMultiplier));
        currentHealth = maxHealth;
        UpdateMaxGuard();
        
        // Roll 1 visible modifier
        RollVisibleModifiers(1);
        
        Debug.Log($"[Monster Rarity] Applied Magic rarity to {enemyName}: {modifiers.Count} modifier(s), {experienceMultiplier}x XP, {quantityMultiplier}x Quantity");
    }
    
    private void ApplyRareRarityModifiers()
    {
        // Apply hidden base modifiers
        experienceMultiplier = MonsterRarityModifiers.Rare.ExperienceMultiplier;
        quantityMultiplier = MonsterRarityModifiers.Rare.QuantityMultiplier;
        rarityMultiplier = MonsterRarityModifiers.Rare.RarityMultiplier;
        itemLevelBonus = MonsterRarityModifiers.Rare.ItemLevelBonus;
        sizeMultiplier = MonsterRarityModifiers.Rare.SizeMultiplier;
        
        // Apply hidden life multiplier (390% more = 4.9x total)
        float lifeMultiplier = MonsterRarityModifiers.Rare.LifeMultiplier;
        maxHealth = Mathf.CeilToInt(maxHealth * (1f + lifeMultiplier));
        currentHealth = maxHealth;
        UpdateMaxGuard();
        
        // Roll 2-4 visible modifiers
        int modifierCount = UnityEngine.Random.Range(2, 5); // 2, 3, or 4
        RollVisibleModifiers(modifierCount);
        
        Debug.Log($"[Monster Rarity] Applied Rare rarity to {enemyName}: {modifiers.Count} modifier(s), {experienceMultiplier}x XP, {quantityMultiplier}x Quantity");
    }
    
    private void ApplyUniqueRarityModifiers()
    {
        // Apply hidden base modifiers
        experienceMultiplier = MonsterRarityModifiers.Unique.ExperienceMultiplier;
        quantityMultiplier = MonsterRarityModifiers.Unique.QuantityMultiplier;
        rarityMultiplier = MonsterRarityModifiers.Unique.RarityMultiplier;
        // Unique enemies don't have item level bonus or size multiplier
        
        // Apply hidden life multiplier (698% more = 7.98x total)
        float lifeMultiplier = MonsterRarityModifiers.Unique.LifeMultiplier;
        maxHealth = Mathf.CeilToInt(maxHealth * (1f + lifeMultiplier));
        currentHealth = maxHealth;
        UpdateMaxGuard();
        
        // Unique enemies typically don't get random modifiers (they're scripted)
        // But we can add them if needed in the future
        
        Debug.Log($"[Monster Rarity] Applied Unique rarity to {enemyName}: {experienceMultiplier}x XP, {quantityMultiplier}x Quantity");
    }
    
    private void RollVisibleModifiers(int count)
    {
        var database = MonsterModifierDatabase.Instance;
        if (database == null)
        {
            Debug.LogWarning("[Monster Rarity] MonsterModifierDatabase not found! Cannot roll modifiers.");
            return;
        }
        
        // Use weighted selection for modifiers
        modifiers = database.GetRandomModifiersWeighted(count);
        
        if (modifiers.Count > 0)
        {
            List<string> modifierNames = new List<string>();
            foreach (var mod in modifiers)
            {
                if (mod != null)
                    modifierNames.Add(mod.modifierName);
            }
            Debug.Log($"[Monster Rarity] Rolled {modifiers.Count} modifier(s) for {enemyName}: {string.Join(", ", modifierNames)}");
        }
    }
    
    private void ApplyModifierStats()
    {
        if (modifiers == null || modifiers.Count == 0)
            return;
        
        float totalHealthMultiplier = 1f;
        float totalDamageMultiplier = 1f;
        float totalAccuracyMultiplier = 1f;
        float totalEvasionMultiplier = 1f;
        float totalCritChanceMultiplier = 1f;
        float totalCritMultiplierMultiplier = 1f;
        
        // Accumulate loot multipliers (multiplicative - stack with rarity base multipliers)
        float totalQuantityMultiplier = 1f;
        float totalRarityMultiplier = 1f;
        
        foreach (var modifier in modifiers)
        {
            if (modifier == null) continue;
            
            // Accumulate stat multipliers (additive for percentages)
            if (modifier.healthMultiplier > 0f)
                totalHealthMultiplier += modifier.healthMultiplier / 100f;
            if (modifier.damageMultiplier > 0f)
                totalDamageMultiplier += modifier.damageMultiplier / 100f;
            if (modifier.accuracyMultiplier > 0f)
                totalAccuracyMultiplier += modifier.accuracyMultiplier / 100f;
            if (modifier.evasionMultiplier > 0f)
                totalEvasionMultiplier += modifier.evasionMultiplier / 100f;
            if (modifier.critChanceMultiplier > 0f)
                totalCritChanceMultiplier += modifier.critChanceMultiplier / 100f;
            if (modifier.critMultiplierMultiplier > 0f)
                totalCritMultiplierMultiplier += modifier.critMultiplierMultiplier / 100f;
            
            // Accumulate loot multipliers (multiplicative stacking)
            if (modifier.quantityMultiplier > 1f)
                totalQuantityMultiplier *= modifier.quantityMultiplier;
            if (modifier.rarityMultiplier > 1f)
                totalRarityMultiplier *= modifier.rarityMultiplier;
        }
        
        // Apply accumulated loot multipliers to enemy's base multipliers
        quantityMultiplier *= totalQuantityMultiplier;
        rarityMultiplier *= totalRarityMultiplier;
        
        // Apply accumulated multipliers
        if (totalHealthMultiplier != 1f)
        {
            maxHealth = Mathf.CeilToInt(maxHealth * totalHealthMultiplier);
            currentHealth = maxHealth;
            UpdateMaxGuard();
        }
        
        if (totalDamageMultiplier != 1f)
        {
            baseDamage = Mathf.CeilToInt(baseDamage * totalDamageMultiplier);
        }
        
        if (totalAccuracyMultiplier != 1f)
        {
            accuracyRating *= totalAccuracyMultiplier;
        }
        
        if (totalEvasionMultiplier != 1f)
        {
            evasionRating *= totalEvasionMultiplier;
        }
        
        if (totalCritChanceMultiplier != 1f)
        {
            criticalChance *= totalCritChanceMultiplier;
            criticalChance = Mathf.Clamp01(criticalChance); // Cap at 100%
        }
        
        if (totalCritMultiplierMultiplier != 1f)
        {
            criticalMultiplier *= totalCritMultiplierMultiplier;
        }
    }
    
    public void TakeDamage(float damage, bool ignoreGuardArmor = false)
    {
        float adjustedDamage = damage * stackCollection.GetToleranceDamageMultiplier();
        
        // Apply guard first (if not ignoring guard/armor)
        if (!ignoreGuardArmor && currentGuard > 0f)
        {
            if (currentGuard >= adjustedDamage)
            {
                currentGuard -= adjustedDamage;
                adjustedDamage = 0;
            }
            else
            {
                adjustedDamage -= currentGuard;
                currentGuard = 0;
            }
        }
        
        // Apply Bolster reduction (if guard didn't fully block)
        if (!ignoreGuardArmor && adjustedDamage > 0f && bolsterStacks > 0f)
        {
            float reduction = Mathf.Clamp01(bolsterStacks * BolsterDamageReductionPerStack);
            adjustedDamage *= 1f - reduction;
        }
        
        currentHealth = Mathf.Max(0, currentHealth - Mathf.RoundToInt(adjustedDamage));
        
        if (currentHealth <= 0)
        {
            Debug.Log($"{enemyName} has been defeated!");
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
    
    public int GetAttackDamage()
    {
        // Accuracy vs Player evasion (area level parity assumed elsewhere)
        var cm = CharacterManager.Instance;
        var player = cm != null ? cm.GetCurrentCharacter() : null;
        if (player != null)
        {
            float playerEvasion = Mathf.Max(0f, player.baseEvasionRating) * (1f + Mathf.Max(0f, player.increasedEvasion));
            // Area parity: if area level > player level, scale evasion down slightly; if lower, scale up slightly
            int areaLevel = EncounterManager.Instance != null ? EncounterManager.Instance.GetCurrentAreaLevel() : player.level;
            float parity = Mathf.Clamp((float)player.level / Mathf.Max(1, areaLevel), 0.5f, 1.5f);
            playerEvasion *= parity;
            float effectiveAccuracy = accuracyRating * (1f + Mathf.Max(0f, dexterityStacks) * DexterityAccuracyPerStack);
            float chanceToHit = effectiveAccuracy / (effectiveAccuracy + Mathf.Max(1f, playerEvasion));
            if (UnityEngine.Random.value >= Mathf.Clamp01(chanceToHit))
            {
                return 0; // Miss
            }
        }

        // Check for critical hit
        float critChance = Mathf.Clamp01(criticalChance
                                         + (stackCollection.GetCritChanceBonus() * 0.01f)
                                         + Mathf.Max(0f, intelligenceStacks) * IntelligenceCritChancePerStack);
        bool isCritical = UnityEngine.Random.Range(0f, 1f) < critChance;
        float damage = baseDamage * stackCollection.GetDamageMoreMultiplier();
        damage *= 1f + Mathf.Max(0f, strengthStacks) * StrengthDamagePerStack;
        
        // Apply damage bonus from delayed actions (Time-Lagged modifier)
        float delayedActionBonus = ModifierEffectHandler.GetDelayedActionDamageBonus(this);
        if (delayedActionBonus > 0f)
        {
            damage *= 1f + (delayedActionBonus / 100f);
        }
        
        if (isCritical)
        {
            // Check for modifier effects that reduce/no extra crit damage (for player taking damage from enemy)
            // Note: This is for when the ENEMY crits the PLAYER, not when player crits enemy
            // The player's crit damage reduction would be handled in Character.TakeDamage or CombatManager
            float critMultBonus = 1f + Mathf.Max(0f, intelligenceStacks) * IntelligenceCritMultiplierPerStack;
            damage *= criticalMultiplier * stackCollection.GetCritMultiplierBonus() * critMultBonus;
            Debug.Log($"{enemyName} landed a critical hit!");
        }
        
        return Mathf.RoundToInt(damage);
    }
    
    public void SetIntent()
    {
        // Energy costs for actions
        const float attackEnergyCost = 5f;
        const float defendEnergyCost = 15f;
        
        // If enemy uses energy, check if they have enough for actions
        if (usesEnergy)
        {
            // If not enough energy for either action, regenerate and try to attack next turn
            if (currentEnergy < attackEnergyCost)
            {
                currentIntent = EnemyIntent.Defend; // Can't act, will just wait
                intentDamage = 0;
                return;
            }
            
            // Prefer attack if we have energy for it, otherwise defend if we have energy for that
            float random = UnityEngine.Random.Range(0f, 1f);
            
            if (random < 0.7f && currentEnergy >= attackEnergyCost) // 70% chance to attack if we have energy
            {
                currentIntent = EnemyIntent.Attack;
                intentDamage = GetAttackDamage();
            }
            else if (currentEnergy >= defendEnergyCost) // Defend if we have energy
            {
                currentIntent = EnemyIntent.Defend;
                intentDamage = 0;
            }
            else if (currentEnergy >= attackEnergyCost) // Fallback to attack if we only have energy for that
            {
                currentIntent = EnemyIntent.Attack;
                intentDamage = GetAttackDamage();
            }
            else // Not enough energy for anything
            {
                currentIntent = EnemyIntent.Defend; // Wait/defend
                intentDamage = 0;
            }
        }
        else
        {
            // No energy system - use simple random choice
            float random = UnityEngine.Random.Range(0f, 1f);
            
            if (random < 0.8f) // 80% chance to attack
            {
                currentIntent = EnemyIntent.Attack;
                intentDamage = GetAttackDamage();
            }
            else // 20% chance to defend
            {
                currentIntent = EnemyIntent.Defend;
                intentDamage = 0;
            }
        }
    }
    
    public string GetIntentDescription()
    {
        switch (currentIntent)
        {
            case EnemyIntent.Attack:
                return $"Attack ({intentDamage})";
            case EnemyIntent.Defend:
                return "Defend";
            default:
                return "Unknown";
        }
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public void ConfigureEnergy(float maxValue, float regenPerTurn, float chillDrainScale = 1f, float slowDrainScale = 1.5f)
    {
        maxEnergy = Mathf.Max(0f, maxValue);
        energyRegenPerTurn = Mathf.Max(0f, regenPerTurn);
        chillDrainPerMagnitude = Mathf.Max(0f, chillDrainScale);
        slowDrainPerMagnitude = Mathf.Max(0f, slowDrainScale);
        usesEnergy = maxEnergy > 0f;
        currentEnergy = usesEnergy ? maxEnergy : 0f;
        RaiseEnergyChanged();
    }
    
    public void RegenerateEnergy(float amount)
    {
        if (!usesEnergy || amount <= 0f) return;
        float newEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
        if (!Mathf.Approximately(newEnergy, currentEnergy))
        {
            currentEnergy = newEnergy;
            RaiseEnergyChanged();
        }
    }
    
    public bool DrainEnergy(float amount, string source = null)
    {
        if (!usesEnergy || amount <= 0f) return false;
        float previousEnergy = currentEnergy;
        float newEnergy = Mathf.Clamp(currentEnergy - amount, 0f, maxEnergy);
        currentEnergy = newEnergy;
        // Always raise the event if energy changed (even slightly)
        if (!Mathf.Approximately(newEnergy, previousEnergy))
        {
            RaiseEnergyChanged();
            Debug.Log($"[EnemyEnergy] {enemyName} drained {amount:F1} energy ({source}). {previousEnergy:F1} -> {currentEnergy:F1}/{maxEnergy:F1}");
            return true;
        }
        return false;
    }
    
    public void ApplyEnergyDrainFromStatus(StatusEffectType effectType, float magnitude)
    {
        if (!usesEnergy || magnitude <= 0f) return;
        float drainAmount = 0f;
        switch (effectType)
        {
            case StatusEffectType.Chill:
                drainAmount = magnitude * Mathf.Max(0f, chillDrainPerMagnitude);
                break;
            case StatusEffectType.Slow:
                drainAmount = magnitude * Mathf.Max(0f, slowDrainPerMagnitude);
                break;
        }
        if (drainAmount > 0f)
        {
            DrainEnergy(drainAmount, effectType.ToString());
        }
    }
    
    public float GetEnergyPercent()
    {
        if (!usesEnergy || maxEnergy <= 0f) return 0f;
        return Mathf.Clamp01(currentEnergy / maxEnergy);
    }
    
    private void RaiseEnergyChanged()
    {
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    #region Guard System
    
    /// <summary>
    /// Add guard to this enemy (capped at max health)
    /// </summary>
    public void AddGuard(float guardAmount)
    {
        // Guard cannot exceed max health
        float newGuard = currentGuard + guardAmount;
        currentGuard = Mathf.Min(newGuard, maxHealth);
        maxGuard = maxHealth; // Update max guard to match max health
        
        Debug.Log($"{enemyName} gained {guardAmount} guard. Total guard: {currentGuard}/{maxGuard}");
    }
    
    /// <summary>
    /// Update max guard based on max health (call this when max health changes)
    /// </summary>
    public void UpdateMaxGuard()
    {
        maxGuard = maxHealth;
        // Ensure current guard doesn't exceed new max
        if (currentGuard > maxGuard)
        {
            currentGuard = maxGuard;
        }
    }
    
    /// <summary>
    /// Decay guard at the start of turn (retain a fraction)
    /// </summary>
    public void DecayGuard()
    {
        if (currentGuard > 0f)
        {
            float retention = Mathf.Clamp01(guardPersistenceFraction);
            float decayedGuard = currentGuard * retention;
            if (!Mathf.Approximately(decayedGuard, currentGuard))
            {
                Debug.Log($"[Guard] {enemyName} guard decayed from {currentGuard:F2} to {decayedGuard:F2} (retention {retention:P0})");
                currentGuard = Mathf.Max(0f, decayedGuard);
            }
        }
    }
    
    #endregion
    
    #region Stagger System
    
    /// <summary>
    /// Add stagger to this enemy. Returns true if stagger threshold was reached and enemy should be stunned.
    /// </summary>
    public bool AddStagger(float amount, float staggerEffectiveness = 1f)
    {
        if (amount <= 0f) return false;
        
        float effectiveStagger = amount * staggerEffectiveness;
        float previousStagger = currentStagger;
        currentStagger += effectiveStagger;
        
        Debug.Log($"[Stagger] {enemyName} gained {effectiveStagger:F1} stagger ({previousStagger:F1} -> {currentStagger:F1}/{staggerThreshold:F1})");
        
        // Check if stagger threshold was reached
        if (previousStagger < staggerThreshold && currentStagger >= staggerThreshold)
        {
            Debug.Log($"[Stagger] {enemyName} reached stagger threshold! ({currentStagger:F1}/{staggerThreshold:F1})");
            return true; // Signal that stagger threshold was reached
        }
        
        return false;
    }
    
    /// <summary>
    /// Reduce stagger meter (decay over time)
    /// </summary>
    public void DecayStagger()
    {
        if (staggerDecayPerTurn > 0f && currentStagger > 0f)
        {
            float previousStagger = currentStagger;
            currentStagger = Mathf.Max(0f, currentStagger - staggerDecayPerTurn);
            
            if (!Mathf.Approximately(previousStagger, currentStagger))
            {
                Debug.Log($"[Stagger] {enemyName} stagger decayed ({previousStagger:F1} -> {currentStagger:F1})");
            }
        }
    }
    
    /// <summary>
    /// Reset stagger meter (called when enemy is stunned)
    /// </summary>
    public void ResetStagger()
    {
        if (currentStagger > 0f)
        {
            Debug.Log($"[Stagger] {enemyName} stagger reset ({currentStagger:F1} -> 0)");
            currentStagger = 0f;
        }
    }
    
    /// <summary>
    /// Get stagger percentage (0-1)
    /// </summary>
    public float GetStaggerPercentage()
    {
        if (staggerThreshold <= 0f) return 0f;
        return Mathf.Clamp01(currentStagger / staggerThreshold);
    }
    
    /// <summary>
    /// Check if enemy is currently staggered (has Stun status effect)
    /// </summary>
    public bool IsStaggered()
    {
        // This will be checked via StatusEffectManager in combat
        // For now, return false - the actual check happens in combat system
        return false;
    }
    
    #endregion

    #region Stack API

    public void ResetStacks()
    {
        EnsureStackCallback();
        stackCollection.Reset();
        ResetBuffs();
    }

    public int GetStacks(StackType type) => stackCollection.GetStacks(type);

    public void AddStacks(StackType type, int amount)
    {
        EnsureStackCallback();
        stackCollection.AddStacks(type, amount);
    }

    public void RemoveStacks(StackType type, int amount)
    {
        EnsureStackCallback();
        stackCollection.RemoveStacks(type, amount);
    }

    public void ClearStacks(StackType type)
    {
        EnsureStackCallback();
        stackCollection.ClearStacks(type);
    }

    public void SetBonusMaxStacks(StackType type, int bonus)
    {
        EnsureStackCallback();
        stackCollection.SetBonusMaxStacks(type, bonus);
    }

    public float GetAgitateDamageMultiplier() => stackCollection.GetDamageMoreMultiplier();

    public (float attack, float cast, float move) GetStackSpeedBonuses() => stackCollection.GetSpeedBonuses();

    public float GetToleranceDamageMultiplier() => stackCollection.GetToleranceDamageMultiplier();

    public float GetPotentialCritChanceBonus() => stackCollection.GetCritChanceBonus();

    public float GetPotentialCritMultiplierBonus() => stackCollection.GetCritMultiplierBonus();

    #endregion

    #region Buff Helpers

    private void ResetBuffs()
    {
        bolsterStacks = 0f;
        strengthStacks = 0f;
        dexterityStacks = 0f;
        intelligenceStacks = 0f;
    }

    public void ModifyBolsterStacks(float delta)
    {
        bolsterStacks = Mathf.Clamp(bolsterStacks + delta, 0f, BolsterStacksCap);
    }

    public float GetBolsterStacks() => bolsterStacks;

    public void ModifyStrengthStacks(float delta)
    {
        strengthStacks = Mathf.Max(0f, strengthStacks + delta);
    }

    public float GetStrengthStacks() => strengthStacks;

    public void ModifyDexterityStacks(float delta)
    {
        dexterityStacks = Mathf.Max(0f, dexterityStacks + delta);
    }

    public float GetDexterityStacks() => dexterityStacks;

    public void ModifyIntelligenceStacks(float delta)
    {
        intelligenceStacks = Mathf.Max(0f, intelligenceStacks + delta);
    }

    public float GetIntelligenceStacks() => intelligenceStacks;

    #endregion

    private void EnsureStackCallback()
    {
        if (stackCallbackRegistered) return;
        stackCollection.SetChangeCallback(OnStackValueChanged);
        stackCallbackRegistered = true;
    }

    private void OnStackValueChanged(StackType type, int value)
    {
        OnStacksChanged?.Invoke(type, value);
    }
}

public enum EnemyIntent
{
    Attack,
    Defend
}
