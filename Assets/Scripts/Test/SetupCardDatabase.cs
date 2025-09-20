using UnityEngine;
using UnityEditor;
using System.Linq;

public class SetupCardDatabase : MonoBehaviour
{
    [ContextMenu("Setup Card Database")]
    public void InitializeCardDatabase()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // Create CardDatabase asset
        CardDatabase cardDatabase = ScriptableObject.CreateInstance<CardDatabase>();
        
        // Find all CardData assets in the project
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        cardDatabase.allCards.Clear();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (cardData != null)
            {
                cardDatabase.allCards.Add(cardData);
                Debug.Log($"Added card to database: {cardData.cardName}");
            }
        }
        
        // Auto-categorize the cards
        cardDatabase.CategorizeCards();
        
        // Save the database
        string databasePath = "Assets/Resources/CardDatabase.asset";
        AssetDatabase.CreateAsset(cardDatabase, databasePath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"CardDatabase created at {databasePath} with {cardDatabase.allCards.Count} cards");
        Debug.Log($"Categories: Attack={cardDatabase.attackCards.Count}, Skill={cardDatabase.skillCards.Count}, Power={cardDatabase.powerCards.Count}, Guard={cardDatabase.guardCards.Count}");
        Debug.Log($"Elements: Basic={cardDatabase.basicCards.Count}, Fire={cardDatabase.fireCards.Count}, Cold={cardDatabase.coldCards.Count}, Lightning={cardDatabase.lightningCards.Count}, Physical={cardDatabase.physicalCards.Count}, Chaos={cardDatabase.chaosCards.Count}");
        Debug.Log($"Rarities: Common={cardDatabase.commonCards.Count}, Magic={cardDatabase.magicCards.Count}, Rare={cardDatabase.rareCards.Count}, Unique={cardDatabase.uniqueCards.Count}");
    }
    
    [ContextMenu("Create CardVisualAssets")]
    public void CreateCardVisualAssets()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // Create CardVisualAssets asset
        CardVisualAssets visualAssets = ScriptableObject.CreateInstance<CardVisualAssets>();
        
        // Try to find and assign sprites from CardParts folder
        AssignCardSprites(visualAssets);
        
        // Save the assets
        string assetsPath = "Assets/Resources/CardVisualAssets.asset";
        AssetDatabase.CreateAsset(visualAssets, assetsPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"CardVisualAssets created at {assetsPath}");
    }
    
    private void AssignCardSprites(CardVisualAssets visualAssets)
    {
        // Find sprites in the CardParts folder
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Art/CardArt/CardParts" });
        
        foreach (string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            
            // Assign sprites based on filename
            AssignSpriteByName(visualAssets, sprite, fileName);
        }
    }
    
    private void AssignSpriteByName(CardVisualAssets visualAssets, Sprite sprite, string fileName)
    {
        // Card Element Frames
        if (fileName.Contains("BasicCard")) visualAssets.basicCard = sprite;
        else if (fileName.Contains("FireCard")) visualAssets.fireCard = sprite;
        else if (fileName.Contains("ColdCard")) visualAssets.coldCard = sprite;
        else if (fileName.Contains("LightningCard")) visualAssets.lightningCard = sprite;
        else if (fileName.Contains("PhysCard")) visualAssets.physCard = sprite;
        else if (fileName.Contains("ChaosCard")) visualAssets.chaosCard = sprite;
        else if (fileName.Contains("PowerCard")) visualAssets.powerCard = sprite;
        else if (fileName.Contains("GuardCard")) visualAssets.guardCard = sprite;
        else if (fileName.Contains("DiscardCard")) visualAssets.discardCard = sprite;
        
        // Cost Bubbles
        else if (fileName.Contains("BasicBubble")) visualAssets.basicBubble = sprite;
        else if (fileName.Contains("FireBubble")) visualAssets.fireBubble = sprite;
        else if (fileName.Contains("IceBubble")) visualAssets.coldBubble = sprite;
        else if (fileName.Contains("LightningBubble")) visualAssets.lightningBubble = sprite;
        else if (fileName.Contains("PhysBubble")) visualAssets.physBubble = sprite;
        else if (fileName.Contains("ChaosBubble")) visualAssets.chaosBubble = sprite;
        
        // Rarity Frames
        else if (fileName.Contains("CommonFrame")) visualAssets.commonFrame = sprite;
        else if (fileName.Contains("MagicFrame")) visualAssets.magicFrame = sprite;
        else if (fileName.Contains("RareFrame")) visualAssets.rareFrame = sprite;
        else if (fileName.Contains("UniqueFrame")) visualAssets.uniqueFrame = sprite;
        
        // Special Effects
        else if (fileName.Contains("Card_Highlight")) visualAssets.cardHighlight = sprite;
        else if (fileName.Contains("IceBubble")) visualAssets.iceBubble = sprite;
        
        Debug.Log($"Assigned sprite {fileName} to visual assets");
    }
}
