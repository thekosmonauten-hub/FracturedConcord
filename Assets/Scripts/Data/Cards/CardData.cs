using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Dexiled/Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Information")]
    public string cardName = "Card Name";
    public string cardType = "Attack";
    public int playCost = 1;
    [TextArea(3, 5)]
    public string description = "Card description";
    
    [Header("Card Properties")]
    public CardRarity rarity = CardRarity.Common;
    public CardElement element = CardElement.Basic;
    public CardCategory category = CardCategory.Attack;
    
    [Header("Visual Assets")]
    public Sprite cardImage;
    public Sprite elementFrame;
    public Sprite costBubble;
    public Sprite rarityFrame;
    
    [Header("Card Effects")]
    public int damage = 0;
    public int block = 0;
    public bool isDiscardCard = false;
    public bool isDualWield = false;
    
    [Header("Special Effects")]
    [TextArea(2, 3)]
    public string ifDiscardedEffect = "";
    [TextArea(2, 3)]
    public string dualWieldEffect = "";
    
    public string GetDisplayName()
    {
        return cardName;
    }
    
    public string GetFullDescription()
    {
        string fullDesc = description;
        
        if (damage > 0)
        {
            fullDesc += $"\nDeal {damage} damage.";
        }
        
        if (block > 0)
        {
            fullDesc += $"\nGain {block} block.";
        }
        
        if (!string.IsNullOrEmpty(ifDiscardedEffect))
        {
            fullDesc += $"\nIf discarded: {ifDiscardedEffect}";
        }
        
        if (!string.IsNullOrEmpty(dualWieldEffect))
        {
            fullDesc += $"\nDual wield: {dualWieldEffect}";
        }
        
        return fullDesc;
    }
}

public enum CardRarity
{
    Common,
    Magic,
    Rare,
    Unique
}

public enum CardElement
{
    Basic,
    Fire,
    Cold,
    Lightning,
    Physical,
    Chaos
}

public enum CardCategory
{
    Attack,
    Skill,
    Power,
    Guard
}
