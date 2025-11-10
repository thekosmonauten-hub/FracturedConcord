using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Editor utility to bulk-assign card sprites to CardDataExtended assets.
/// Matches sprites by name (e.g., "Strike.png" → "Strike.asset")
/// </summary>
public class CardSpriteAssigner : EditorWindow
{
    [Header("Configuration")]
    [Tooltip("Folder containing CardDataExtended assets")]
    private string cardAssetsPath = "Assets/Resources/Cards";
    
    [Tooltip("Folder containing card sprite images")]
    private string spritesPath = "Assets/Art/CardArt/CardSprites";
    
    [Tooltip("Only assign if card currently has no sprite")]
    private bool onlyAssignIfEmpty = true;
    
    [Tooltip("Show detailed logs")]
    private bool showLogs = true;
    
    private Vector2 scrollPosition;
    private int cardsFound = 0;
    private int spritesFound = 0;
    private int cardsUpdated = 0;
    
    [MenuItem("Tools/Card Sprite Assigner")]
    public static void ShowWindow()
    {
        GetWindow<CardSpriteAssigner>("Card Sprite Assigner");
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Card Sprite Auto-Assigner", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Configuration
        cardAssetsPath = EditorGUILayout.TextField("Cards Path:", cardAssetsPath);
        spritesPath = EditorGUILayout.TextField("Sprites Path:", spritesPath);
        onlyAssignIfEmpty = EditorGUILayout.Toggle("Only Assign If Empty:", onlyAssignIfEmpty);
        showLogs = EditorGUILayout.Toggle("Show Logs:", showLogs);
        
        EditorGUILayout.Space();
        
        // Info display
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Cards Found: {cardsFound}");
        EditorGUILayout.LabelField($"Sprites Found: {spritesFound}");
        EditorGUILayout.LabelField($"Cards Updated: {cardsUpdated}");
        
        EditorGUILayout.Space();
        
        // Actions
        if (GUILayout.Button("Scan Folders", GUILayout.Height(30)))
        {
            ScanFolders();
        }
        
        if (GUILayout.Button("Assign Sprites", GUILayout.Height(40)))
        {
            AssignSprites();
        }
        
        EditorGUILayout.Space();
        
        // Instructions
        EditorGUILayout.HelpBox(
            "Instructions:\n" +
            "1. Set 'Cards Path' to folder containing CardDataExtended assets\n" +
            "2. Set 'Sprites Path' to folder containing card sprite images\n" +
            "3. Click 'Scan Folders' to see how many cards/sprites were found\n" +
            "4. Click 'Assign Sprites' to auto-assign sprites by matching names\n\n" +
            "Example:\n" +
            "Strike.asset ← Strike.png\n" +
            "Bash.asset ← Bash.png",
            MessageType.Info
        );
        
        EditorGUILayout.EndScrollView();
    }
    
    void ScanFolders()
    {
        // Find all CardDataExtended assets
        string[] cardGuids = AssetDatabase.FindAssets("t:CardDataExtended", new[] { cardAssetsPath });
        cardsFound = cardGuids.Length;
        
        // Find all sprite assets
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spritesPath });
        spritesFound = spriteGuids.Length;
        
        if (showLogs)
        {
            Debug.Log($"[CardSpriteAssigner] Found {cardsFound} cards in '{cardAssetsPath}'");
            Debug.Log($"[CardSpriteAssigner] Found {spritesFound} sprites in '{spritesPath}'");
        }
    }
    
    void AssignSprites()
    {
        cardsUpdated = 0;
        
        // Find all CardDataExtended assets
        string[] cardGuids = AssetDatabase.FindAssets("t:CardDataExtended", new[] { cardAssetsPath });
        cardsFound = cardGuids.Length;
        
        // Find all sprite assets
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spritesPath });
        spritesFound = spriteGuids.Length;
        
        // Create a lookup dictionary: sprite name → sprite
        var spriteLookup = spriteGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
            .Where(sprite => sprite != null)
            .ToDictionary(sprite => sprite.name, sprite => sprite);
        
        if (showLogs)
        {
            Debug.Log($"[CardSpriteAssigner] Starting sprite assignment...");
            Debug.Log($"[CardSpriteAssigner] Found {cardsFound} cards and {spriteLookup.Count} sprites");
        }
        
        // Assign sprites to cards
        foreach (string guid in cardGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardDataExtended card = AssetDatabase.LoadAssetAtPath<CardDataExtended>(path);
            
            if (card == null) continue;
            
            // Skip if card already has a sprite (if option is enabled)
            if (onlyAssignIfEmpty && card.cardImage != null)
            {
                if (showLogs)
                {
                    Debug.Log($"[CardSpriteAssigner] Skipped '{card.cardName}' (already has sprite)");
                }
                continue;
            }
            
            // Try to find matching sprite by card name
            string cardName = card.cardName;
            
            // Try exact match first
            if (spriteLookup.TryGetValue(cardName, out Sprite sprite))
            {
                card.cardImage = sprite;
                EditorUtility.SetDirty(card);
                cardsUpdated++;
                
                if (showLogs)
                {
                    Debug.Log($"[CardSpriteAssigner] ✓ Assigned '{sprite.name}' to '{card.cardName}'");
                }
            }
            // Try normalized match (remove spaces, lowercase)
            else
            {
                string normalizedCardName = cardName.Replace(" ", "").ToLower();
                var match = spriteLookup.FirstOrDefault(kvp => 
                    kvp.Key.Replace(" ", "").ToLower() == normalizedCardName);
                
                if (match.Value != null)
                {
                    card.cardImage = match.Value;
                    EditorUtility.SetDirty(card);
                    cardsUpdated++;
                    
                    if (showLogs)
                    {
                        Debug.Log($"[CardSpriteAssigner] ✓ Assigned '{match.Value.name}' to '{card.cardName}' (normalized match)");
                    }
                }
                else
                {
                    if (showLogs)
                    {
                        Debug.LogWarning($"[CardSpriteAssigner] ✗ No sprite found for '{card.cardName}'");
                    }
                }
            }
        }
        
        // Save changes
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>[CardSpriteAssigner] ✓ Complete! Updated {cardsUpdated} cards</color>");
        
        EditorUtility.DisplayDialog(
            "Sprite Assignment Complete",
            $"Successfully assigned sprites to {cardsUpdated} cards!\n\n" +
            $"Cards found: {cardsFound}\n" +
            $"Sprites found: {spritesFound}\n" +
            $"Cards updated: {cardsUpdated}",
            "OK"
        );
    }
}


