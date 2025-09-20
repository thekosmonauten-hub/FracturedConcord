using UnityEngine;

[System.Serializable]
public class DamageStats
{
    [Header("Resistances")]
    public float physicalResistance = 0f; // Armor (flat reduction)
    public float fireResistance = 0f;     // Percentage reduction
    public float coldResistance = 0f;     // Percentage reduction
    public float lightningResistance = 0f; // Percentage reduction
    public float chaosResistance = 0f;    // Percentage reduction
    
    // Get resistance for a specific damage type
    public float GetResistance(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                return physicalResistance;
            case DamageType.Fire:
                return fireResistance;
            case DamageType.Cold:
                return coldResistance;
            case DamageType.Lightning:
                return lightningResistance;
            case DamageType.Chaos:
                return chaosResistance;
            default:
                return 0f;
        }
    }
    
    // Set resistance for a specific damage type
    public void SetResistance(DamageType damageType, float value)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                physicalResistance = value;
                break;
            case DamageType.Fire:
                fireResistance = value;
                break;
            case DamageType.Cold:
                coldResistance = value;
                break;
            case DamageType.Lightning:
                lightningResistance = value;
                break;
            case DamageType.Chaos:
                chaosResistance = value;
                break;
        }
    }
    
    // Add resistance for a specific damage type
    public void AddResistance(DamageType damageType, float value)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                physicalResistance += value;
                break;
            case DamageType.Fire:
                fireResistance += value;
                break;
            case DamageType.Cold:
                coldResistance += value;
                break;
            case DamageType.Lightning:
                lightningResistance += value;
                break;
            case DamageType.Chaos:
                chaosResistance += value;
                break;
        }
    }
}
