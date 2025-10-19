using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CreateSampleCards : MonoBehaviour
{
    [ContextMenu("Create Sample Cards")]
    public void GenerateSampleCards()
    {
        CreateCard("Strike", "Attack", 1, "Deal 6 damage.", CardRarity.Common, CardElement.Basic, CardCategory.Attack, 6, 0);
        CreateCard("Defend", "Skill", 1, "Gain 5 block.", CardRarity.Common, CardElement.Basic, CardCategory.Guard, 0, 5);
        CreateCard("Fireball", "Attack", 2, "Deal 12 fire damage.", CardRarity.Magic, CardElement.Fire, CardCategory.Attack, 12, 0);
        CreateCard("Ice Shield", "Skill", 1, "Gain 8 block. Apply 2 frost.", CardRarity.Magic, CardElement.Cold, CardCategory.Guard, 0, 8);
        CreateCard("Lightning Bolt", "Attack", 1, "Deal 8 lightning damage. Draw 1 card.", CardRarity.Magic, CardElement.Lightning, CardCategory.Attack, 8, 0);
        CreateCard("Heavy Strike", "Attack", 2, "Deal 15 physical damage.", CardRarity.Common, CardElement.Physical, CardCategory.Attack, 15, 0);
        CreateCard("Chaos Blast", "Attack", 3, "Deal 20 chaos damage to all enemies.", CardRarity.Rare, CardElement.Chaos, CardCategory.Attack, 20, 0);
        CreateCard("Power Up", "Power", 2, "Gain 2 strength.", CardRarity.Magic, CardElement.Basic, CardCategory.Power, 0, 0);
        CreateCard("Dual Strike", "Attack", 1, "Deal 4 damage twice.", CardRarity.Magic, CardElement.Physical, CardCategory.Attack, 4, 0, false, true, "", "Deal 4 damage again.");
        CreateCard("Desperate Strike", "Attack", 0, "Deal 8 damage. If discarded, deal 12 damage instead.", CardRarity.Rare, CardElement.Physical, CardCategory.Attack, 8, 0, true, false, "Deal 12 damage instead.");
        
        Debug.Log("Sample cards created! Check the Assets/SampleCards folder.");
    }
    
    private void CreateCard(string name, string type, int cost, string description, CardRarity rarity, CardElement element, CardCategory category, int damage, int block, bool isDiscard = false, bool isDual = false, string discardEffect = "", string dualEffect = "")
    {
        // Create the card asset
        CardData cardData = ScriptableObject.CreateInstance<CardData>();
        
        // Set basic information
        cardData.cardName = name;
        cardData.cardType = type;
        cardData.playCost = cost;
        cardData.description = description;
        
        // Set properties
        cardData.rarity = rarity;
        cardData.element = element;
        cardData.category = category;
        
        // Set effects
        cardData.damage = damage;
        cardData.block = block;
        cardData.isDiscardCard = isDiscard;
        cardData.isDualWield = isDual;
        cardData.ifDiscardedEffect = discardEffect;
        cardData.dualWieldEffect = dualEffect;
        
        #if UNITY_EDITOR
        // Create the folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/SampleCards"))
        {
            AssetDatabase.CreateFolder("Assets", "SampleCards");
        }
        // Save the asset
        string assetPath = $"Assets/SampleCards/{name}.asset";
        AssetDatabase.CreateAsset(cardData, assetPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created card: {name} at {assetPath}");
        #else
        Debug.LogWarning("CreateCard asset creation is editor-only.");
        #endif
    }
}
