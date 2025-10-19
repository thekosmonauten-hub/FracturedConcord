using UnityEngine;

[System.Serializable]
public class DeckStatistics
{
    [Header("Card Counts")]
    public int totalCards;
    public int uniqueCards; // Number of unique cards in deck
    public int attackCards;
    public int guardCards;
    public int skillCards;
    public int powerCards;
    public int auraCards;
    
    [Header("Mana Information")]
    public float averageManaCost;
    public int totalManaCost;
    
    // Get percentage of each card type
    public float GetAttackCardPercentage()
    {
        return totalCards > 0 ? (float)attackCards / totalCards * 100f : 0f;
    }
    
    public float GetGuardCardPercentage()
    {
        return totalCards > 0 ? (float)guardCards / totalCards * 100f : 0f;
    }
    
    public float GetSkillCardPercentage()
    {
        return totalCards > 0 ? (float)skillCards / totalCards * 100f : 0f;
    }
    
    public float GetPowerCardPercentage()
    {
        return totalCards > 0 ? (float)powerCards / totalCards * 100f : 0f;
    }
    
    public float GetAuraCardPercentage()
    {
        return totalCards > 0 ? (float)auraCards / totalCards * 100f : 0f;
    }
    
    // Get deck summary as string
    public string GetDeckSummary()
    {
        return $"Total Cards: {totalCards}\n" +
               $"Attack: {attackCards} ({GetAttackCardPercentage():F1}%)\n" +
               $"Guard: {guardCards} ({GetGuardCardPercentage():F1}%)\n" +
               $"Skill: {skillCards} ({GetSkillCardPercentage():F1}%)\n" +
               $"Power: {powerCards} ({GetPowerCardPercentage():F1}%)\n" +
               $"Aura: {auraCards} ({GetAuraCardPercentage():F1}%)\n" +
               $"Average Mana Cost: {averageManaCost:F1}";
    }
}
