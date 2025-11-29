using UnityEngine;

/// <summary>
/// Defines the hidden base modifiers that are automatically applied to enemies based on their rarity.
/// These are backend modifiers and don't count towards the visible modifier cap.
/// </summary>
public static class MonsterRarityModifiers
{
    /// <summary>
    /// Hidden base modifiers for Magic enemies
    /// </summary>
    public static class Magic
    {
        public const float ExperienceMultiplier = 2.5f; // 250% increased = 2.5x multiplier
        public const float LifeMultiplier = 1.48f; // 148% more = 2.48x total (1 + 1.48)
        public const float QuantityMultiplier = 6.0f; // 600% increased = 6x multiplier
        public const float RarityMultiplier = 2.0f; // 200% increased = 2x multiplier
        public const int ItemLevelBonus = 1; // Drops gear 1 level above
    }
    
    /// <summary>
    /// Hidden base modifiers for Rare enemies
    /// </summary>
    public static class Rare
    {
        public const float ExperienceMultiplier = 7.5f; // 750% increased = 7.5x multiplier
        public const float LifeMultiplier = 3.9f; // 390% more = 4.9x total (1 + 3.9)
        public const float QuantityMultiplier = 14.0f; // 1400% increased = 14x multiplier
        public const float RarityMultiplier = 10.0f; // 1000% increased = 10x multiplier
        public const float SizeMultiplier = 1.1f; // 10% increased size = 1.1x
        public const int ItemLevelBonus = 2; // Drops gear 2 levels above
    }
    
    /// <summary>
    /// Hidden base modifiers for Unique enemies
    /// </summary>
    public static class Unique
    {
        public const float ExperienceMultiplier = 4.5f; // 450% increased = 4.5x multiplier
        public const float LifeMultiplier = 6.98f; // 698% more = 7.98x total (1 + 6.98)
        public const float QuantityMultiplier = 28.5f; // 2850% increased = 28.5x multiplier
        public const float RarityMultiplier = 10.0f; // 1000% increased = 10x multiplier
    }
}

