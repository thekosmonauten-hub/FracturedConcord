using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Tool to migrate CardData assets to CardDataExtended.
/// Eliminates circular conversion by creating CardDataExtended assets.
/// </summary>
public class CardDataMigrationTool : EditorWindow
{
    private List<CardData> cardsToMigrate = new List<CardData>();
    private Vector2 scrollPosition;
    private bool showExtendedOnly = false;
    private bool showNonExtendedOnly = false;
    
    [MenuItem("Tools/Cards/Migrate to CardDataExtended")]
    public static void ShowWindow()
    {
        var window = GetWindow<CardDataMigrationTool>("Card Migration");
        window.minSize = new Vector2(600, 400);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("CardDataExtended Migration Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool migrates CardData assets to CardDataExtended.\n\n" +
            "CardDataExtended includes all CardData features PLUS combat features:\n" +
            "• Weapon scaling (melee, projectile, spell)\n" +
            "• AoE targeting\n" +
            "• Attribute scaling (STR/DEX/INT)\n" +
            "• Effects system\n" +
            "• Combo properties\n" +
            "• Requirements",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find All CardData Assets", GUILayout.Height(30)))
        {
            FindCardDataAssets();
        }
        
        showExtendedOnly = GUILayout.Toggle(showExtendedOnly, "Show Extended Only");
        showNonExtendedOnly = GUILayout.Toggle(showNonExtendedOnly, "Show Non-Extended Only");
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (cardsToMigrate.Count > 0)
        {
            int extendedCount = 0;
            int regularCount = 0;
            
            foreach (var card in cardsToMigrate)
            {
                if (card is CardDataExtended)
                    extendedCount++;
                else
                    regularCount++;
            }
            
            EditorGUILayout.LabelField($"Found {cardsToMigrate.Count} CardData assets:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"✓ Already Extended: {extendedCount}");
            EditorGUILayout.LabelField($"⚠ Needs Migration: {regularCount}");
            
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));
            
            foreach (CardData card in cardsToMigrate)
            {
                bool isExtended = card is CardDataExtended;
                
                // Filter logic
                if (showExtendedOnly && !isExtended) continue;
                if (showNonExtendedOnly && isExtended) continue;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(card, typeof(CardData), false);
                
                if (isExtended)
                {
                    GUI.color = Color.green;
                    GUILayout.Label("✓ Extended", GUILayout.Width(100));
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("⚠ Migrate", GUILayout.Width(100));
                    GUI.color = Color.white;
                    
                    if (GUILayout.Button("Migrate →", GUILayout.Width(80)))
                    {
                        MigrateCard(card);
                        FindCardDataAssets(); // Refresh list
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            if (regularCount > 0)
            {
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button($"Migrate All {regularCount} Cards", GUILayout.Height(40)))
                {
                    MigrateAllCards();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.color = Color.green;
                EditorGUILayout.HelpBox("✓ All cards are already CardDataExtended!", MessageType.Info);
                GUI.color = Color.white;
            }
        }
    }
    
    private void FindCardDataAssets()
    {
        cardsToMigrate.Clear();
        
        // Find all CardData assets
        string[] guids = AssetDatabase.FindAssets("t:CardData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null)
            {
                cardsToMigrate.Add(card);
            }
        }
        
        Debug.Log($"Found {cardsToMigrate.Count} CardData assets");
    }
    
    private void MigrateCard(CardData oldCard)
    {
        // Create new CardDataExtended asset
        CardDataExtended newCard = ScriptableObject.CreateInstance<CardDataExtended>();
        
        // Copy all CardData fields
        CopyCardDataFields(oldCard, newCard);
        
        // Set default combat properties based on card data
        SetDefaultCombatProperties(newCard);
        
        // Get original path and create new one
        string oldPath = AssetDatabase.GetAssetPath(oldCard);
        string directory = Path.GetDirectoryName(oldPath);
        string fileName = Path.GetFileNameWithoutExtension(oldPath);
        string newPath = Path.Combine(directory, fileName + "_Extended.asset");
        
        // Check if file already exists
        if (File.Exists(newPath))
        {
            if (!EditorUtility.DisplayDialog(
                "File Exists",
                $"{fileName}_Extended.asset already exists. Overwrite?",
                "Overwrite",
                "Cancel"))
            {
                return;
            }
        }
        
        // Create the new asset
        AssetDatabase.CreateAsset(newCard, newPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"✓ Migrated: {oldCard.cardName} → {newPath}");
        EditorUtility.DisplayDialog(
            "Migration Complete",
            $"Migrated {oldCard.cardName} to CardDataExtended!\n\nNew file: {Path.GetFileName(newPath)}",
            "OK"
        );
    }
    
    private void MigrateAllCards()
    {
        int migratedCount = 0;
        int skippedCount = 0;
        List<string> errors = new List<string>();
        
        foreach (CardData oldCard in cardsToMigrate)
        {
            // Skip if already extended
            if (oldCard is CardDataExtended)
            {
                skippedCount++;
                continue;
            }
            
            try
            {
                // Create new CardDataExtended asset
                CardDataExtended newCard = ScriptableObject.CreateInstance<CardDataExtended>();
                
                // Copy all CardData fields
                CopyCardDataFields(oldCard, newCard);
                
                // Set default combat properties
                SetDefaultCombatProperties(newCard);
                
                // Get original path and create new one
                string oldPath = AssetDatabase.GetAssetPath(oldCard);
                string directory = Path.GetDirectoryName(oldPath);
                string fileName = Path.GetFileNameWithoutExtension(oldPath);
                string newPath = Path.Combine(directory, fileName + "_Extended.asset");
                
                // Create the new asset
                AssetDatabase.CreateAsset(newCard, newPath);
                
                Debug.Log($"✓ Migrated: {oldCard.cardName} → {newPath}");
                migratedCount++;
            }
            catch (System.Exception e)
            {
                errors.Add($"{oldCard.cardName}: {e.Message}");
                Debug.LogError($"Failed to migrate {oldCard.cardName}: {e.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Show results
        string message = $"Migrated {migratedCount} cards to CardDataExtended.\n" +
                        $"Skipped {skippedCount} cards (already extended).\n\n" +
                        $"New assets created with '_Extended' suffix.\n";
        
        if (errors.Count > 0)
        {
            message += $"\n{errors.Count} errors occurred:\n" + string.Join("\n", errors);
        }
        else
        {
            message += "\n✓ Review and delete old CardData assets when ready.";
        }
        
        EditorUtility.DisplayDialog("Migration Complete", message, "OK");
        
        // Refresh the list
        FindCardDataAssets();
    }
    
    private void CopyCardDataFields(CardData source, CardDataExtended target)
    {
        target.cardName = source.cardName;
        target.cardType = source.cardType;
        target.playCost = source.playCost;
        target.description = source.description;
        target.rarity = source.rarity;
        target.element = source.element;
        target.category = source.category;
        target.cardImage = source.cardImage;
        target.elementFrame = source.elementFrame;
        target.costBubble = source.costBubble;
        target.rarityFrame = source.rarityFrame;
        target.damage = source.damage;
        target.block = source.block;
        target.isDiscardCard = source.isDiscardCard;
        target.isDualWield = source.isDualWield;
        target.ifDiscardedEffect = source.ifDiscardedEffect;
        target.dualWieldEffect = source.dualWieldEffect;
    }
    
    private void SetDefaultCombatProperties(CardDataExtended card)
    {
        // Set primaryDamageType based on element
        card.primaryDamageType = GetDamageTypeFromElement(card.element);
        
        // Set weapon scaling based on category and element
        if (card.category == CardCategory.Attack)
        {
            if (card.element == CardElement.Physical)
            {
                card.scalesWithMeleeWeapon = true;
            }
            else if (card.element == CardElement.Fire || card.element == CardElement.Cold || 
                     card.element == CardElement.Lightning || card.element == CardElement.Chaos)
            {
                card.scalesWithSpellWeapon = true;
            }
        }
        
        // Default AoE settings
        card.isAoE = false;
        card.aoeTargets = 1;
        
        // Initialize attribute scaling
        card.damageScaling = new AttributeScaling();
        if (card.category == CardCategory.Attack)
        {
            // Default STR scaling for physical attacks
            if (card.element == CardElement.Physical)
            {
                card.damageScaling.strengthScaling = 1.0f;
            }
            // Default INT scaling for spell attacks
            else if (card.element == CardElement.Fire || card.element == CardElement.Cold || 
                     card.element == CardElement.Lightning || card.element == CardElement.Chaos)
            {
                card.damageScaling.intelligenceScaling = 1.0f;
            }
        }
        
        card.guardScaling = new AttributeScaling();
        if (card.category == CardCategory.Guard)
        {
            card.guardScaling.strengthScaling = 0.5f; // Guards scale with STR
        }
        
        // Initialize empty collections
        card.requirements = new CardRequirements();
        card.effects = new List<CardEffect>();
        card.tags = new List<string> { card.category.ToString(), card.element.ToString() };
        card.additionalDamageTypes = new List<DamageType>();
        
        // Initialize combo properties
        card.comboWith = "";
        card.comboDescription = "";
        card.comboEffect = "";
        card.comboHighlightType = "type";
    }
    
    private DamageType GetDamageTypeFromElement(CardElement element)
    {
        switch (element)
        {
            case CardElement.Fire: return DamageType.Fire;
            case CardElement.Cold: return DamageType.Cold;
            case CardElement.Lightning: return DamageType.Lightning;
            case CardElement.Physical: return DamageType.Physical;
            case CardElement.Chaos: return DamageType.Chaos;
            default: return DamageType.Physical;
        }
    }
}



