using UnityEngine;

/// <summary>
/// JSON data structures for card import/export.
/// Shared between Editor tools and runtime loading.
/// </summary>

[System.Serializable]
public class JSONDeckFormat
{
    public string deckName;
    public string characterClass;
    public string description;
    public JSONCardEntry[] cards;
}

[System.Serializable]
public class JSONCardEntry
{
    public string cardName;
    public int count;
    public JSONCardFormat data;
}

[System.Serializable]
public class JSONCardFormat
{
    public string cardName;
    public string description;
    public string cardType;
    public int manaCost;
    public float baseDamage;
    public float baseGuard;
    public string primaryDamageType;
    
    // Visual Assets - Must be in same order as JSON
    [SerializeField] public string cardArtName = "";
    
    public JSONWeaponScaling weaponScaling;
    public JSONAoE aoe;
    public JSONRequirements requirements;
    public string[] tags;
    public string[] additionalDamageTypes;
    public JSONAttributeScaling damageScaling;
    public JSONAttributeScaling guardScaling;
    public JSONEffect[] effects;
    public string rarity;
    public string element;
    public string category;
}

[System.Serializable]
public class JSONWeaponScaling
{
    public bool scalesWithMeleeWeapon;
    public bool scalesWithProjectileWeapon;
    public bool scalesWithSpellWeapon;
}

[System.Serializable]
public class JSONAoE
{
    public bool isAoE;
    public int aoeTargets;
}

[System.Serializable]
public class JSONRequirements
{
    public int requiredStrength;
    public int requiredDexterity;
    public int requiredIntelligence;
    public int requiredLevel;
    public string[] requiredWeaponTypes;
}

[System.Serializable]
public class JSONAttributeScaling
{
    public float strengthScaling;
    public float dexterityScaling;
    public float intelligenceScaling;
}

[System.Serializable]
public class JSONEffect
{
    public string effectType;
    public string effectName;
    public string description;
    public float value;
    public int duration;
    public string damageType;
    public bool targetsSelf;
    public bool targetsEnemy;
    public bool targetsAllEnemies;
    public bool targetsAll;
    public string condition;
}

