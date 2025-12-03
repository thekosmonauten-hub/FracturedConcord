using UnityEngine;

[CreateAssetMenu(fileName = "CardVisualAssets", menuName = "Dexiled/Cards/Card Visual Assets")]
public class CardVisualAssets : ScriptableObject
{
    [Header("Card Element Frames")]
    public Sprite basicCard;
    public Sprite fireCard;
    public Sprite coldCard;
    public Sprite lightningCard;
    public Sprite physCard;
    public Sprite chaosCard;
    public Sprite powerCard;
    public Sprite guardCard;
    public Sprite discardCard;
    
    [Header("Cost Bubbles")]
    public Sprite basicBubble;
    public Sprite fireBubble;
    public Sprite coldBubble;
    public Sprite lightningBubble;
    public Sprite physBubble;
    public Sprite chaosBubble;
    
    [Header("Rarity Frames")]
    public Sprite commonFrame;
    public Sprite magicFrame;
    public Sprite rareFrame;
    public Sprite uniqueFrame;
    
    [Header("Special Effects")]
    public Sprite cardHighlight;
    public Sprite iceBubble;
    public Sprite temporalCard; // Special background for Temporal cards (Temporal Savant)
    
    [Header("Category Icons")]
    public Sprite attackIcon;
    public Sprite guardIcon;
    public Sprite skillIcon;
    public Sprite powerIcon;
    public Sprite auraIcon;
}
