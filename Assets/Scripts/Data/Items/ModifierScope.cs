using System;

/// <summary>
/// Defines whether a modifier affects the item locally or the character globally.
/// This is a fundamental ARPG concept that determines where and how modifiers can roll.
/// </summary>
[Serializable]
public enum ModifierScope
{
    /// <summary>
    /// Local modifiers affect the base stats of the item they're on.
    /// Examples: "% increased Physical Damage" on weapons, "% increased Armour" on armor pieces.
    /// Can only roll on items that have the relevant base stat.
    /// </summary>
    Local,
    
    /// <summary>
    /// Global modifiers affect the character's stats directly.
    /// Examples: Critical Strike Multiplier, Resistances, Accuracy Rating.
    /// Can roll on any appropriate item type (typically jewelry and armor).
    /// </summary>
    Global
}







