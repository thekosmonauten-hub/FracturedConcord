using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor tool to find unused sprites and assign them to embossings with duplicate sprites
/// </summary>
public class EmbossingSpriteAssigner : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, List<string>> assignments = new Dictionary<string, List<string>>();
    
    [MenuItem("Tools/Dexiled/Assign Unused Sprites to Embossings")]
    public static void ShowWindow()
    {
        GetWindow<EmbossingSpriteAssigner>("Embossing Sprite Assigner");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Embossing Sprite Assigner", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool finds unused sprites from the Embossings sprite sheets and assigns them to embossings that currently have duplicate sprites.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find Unused Sprites & Assign"))
        {
            FindAndAssignUnusedSprites();
        }
        
        EditorGUILayout.Space();
        
        if (assignments.Count > 0)
        {
            EditorGUILayout.LabelField("Proposed Assignments:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            foreach (var assignment in assignments)
            {
                EditorGUILayout.LabelField($"{assignment.Key}:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                foreach (string spriteName in assignment.Value)
                {
                    EditorGUILayout.LabelField($"  → {spriteName}");
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Apply Assignments"))
            {
                ApplyAssignments();
            }
        }
    }
    
    private void FindAndAssignUnusedSprites()
    {
        assignments.Clear();
        
        // Load all sprites from the sprite sheets
        List<Sprite> allSprites = new List<Sprite>();
        
        // Load sprites from each sprite sheet
        string[] spriteSheetPaths = {
            "Assets/Art/UI/Embossings/Embossings_1-Sheet.aseprite",
            "Assets/Art/UI/Embossings/Embossings_2.aseprite",
            "Assets/Art/UI/Embossings/Embossings_3.aseprite",
            "Assets/Art/UI/Embossings/Embossings_4.png",
            "Assets/Art/UI/Embossings/Embossings_5.png",
            "Assets/Art/UI/Embossings/Embossings_6.png"
        };
        
        foreach (string sheetPath in spriteSheetPaths)
        {
            // Load all sprites from this texture/sprite sheet
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
            if (sprites.Length > 0)
            {
                allSprites.AddRange(sprites);
                Debug.Log($"[EmbossingSpriteAssigner] Loaded {sprites.Length} sprites from {sheetPath}");
            }
            else
            {
                Debug.LogWarning($"[EmbossingSpriteAssigner] No sprites found in {sheetPath}");
            }
        }
        
        Debug.Log($"[EmbossingSpriteAssigner] Total sprites found: {allSprites.Count}");
        
        // Get all currently assigned sprites
        string[] embossingGuids = AssetDatabase.FindAssets("t:EmbossingEffect", new[] { "Assets/Resources/Embossings" });
        HashSet<Sprite> usedSprites = new HashSet<Sprite>();
        Dictionary<string, Sprite> embossingToSprite = new Dictionary<string, Sprite>();
        
        foreach (string guid in embossingGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EmbossingEffect embossing = AssetDatabase.LoadAssetAtPath<EmbossingEffect>(path);
            
            if (embossing != null && embossing.embossingIcon != null)
            {
                usedSprites.Add(embossing.embossingIcon);
                embossingToSprite[embossing.embossingName] = embossing.embossingIcon;
            }
        }
        
        Debug.Log($"[EmbossingSpriteAssigner] Currently used sprites: {usedSprites.Count}");
        
        // Find unused sprites
        List<Sprite> unusedSprites = allSprites.Where(s => s != null && !usedSprites.Contains(s)).ToList();
        Debug.Log($"[EmbossingSpriteAssigner] Unused sprites: {unusedSprites.Count}");
        
        // Target embossings that need new sprites (those with duplicates)
        List<string> targetEmbossings = new List<string>
        {
            "of Resilience",
            "of Deadly Ailments",
            "of Flame Infusion",
            "of Power Charges",
            "of Slower Projectiles",
            "of Unbound Ailments"
        };
        
        // Assign unused sprites to target embossings
        int unusedIndex = 0;
        foreach (string embossingName in targetEmbossings)
        {
            if (unusedIndex < unusedSprites.Count)
            {
                if (!assignments.ContainsKey(embossingName))
                {
                    assignments[embossingName] = new List<string>();
                }
                assignments[embossingName].Add(unusedSprites[unusedIndex].name);
                unusedIndex++;
            }
            else
            {
                Debug.LogWarning($"[EmbossingSpriteAssigner] Not enough unused sprites! Need {targetEmbossings.Count}, have {unusedSprites.Count}");
                break;
            }
        }
        
        if (assignments.Count > 0)
        {
            Debug.Log($"[EmbossingSpriteAssigner] Found {assignments.Count} assignments to make");
        }
        else
        {
            EditorUtility.DisplayDialog("No Assignments", "Could not find enough unused sprites to assign.", "OK");
        }
    }
    
    private void ApplyAssignments()
    {
        if (assignments.Count == 0)
        {
            EditorUtility.DisplayDialog("No Assignments", "No assignments to apply. Click 'Find Unused Sprites & Assign' first.", "OK");
            return;
        }
        
        int assignedCount = 0;
        int notFoundCount = 0;
        
        foreach (var assignment in assignments)
        {
            string embossingName = assignment.Key;
            string spriteName = assignment.Value[0]; // Use first suggested sprite
            
            // Find the embossing asset
            string[] guids = AssetDatabase.FindAssets("t:EmbossingEffect", new[] { "Assets/Resources/Embossings" });
            EmbossingEffect embossing = null;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EmbossingEffect e = AssetDatabase.LoadAssetAtPath<EmbossingEffect>(path);
                if (e != null && e.embossingName == embossingName)
                {
                    embossing = e;
                    break;
                }
            }
            
            if (embossing == null)
            {
                Debug.LogWarning($"[EmbossingSpriteAssigner] Could not find embossing: {embossingName}");
                notFoundCount++;
                continue;
            }
            
            // Find the sprite by name
            Sprite targetSprite = null;
            string[] spriteSheetPaths = {
                "Assets/Art/UI/Embossings/Embossings_1-Sheet.aseprite",
                "Assets/Art/UI/Embossings/Embossings_2.aseprite",
                "Assets/Art/UI/Embossings/Embossings_3.aseprite",
                "Assets/Art/UI/Embossings/Embossings_4.png",
                "Assets/Art/UI/Embossings/Embossings_5.png",
                "Assets/Art/UI/Embossings/Embossings_6.png"
            };
            
            foreach (string sheetPath in spriteSheetPaths)
            {
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
                targetSprite = sprites.FirstOrDefault(s => s != null && s.name == spriteName);
                if (targetSprite != null)
                    break;
            }
            
            if (targetSprite == null)
            {
                Debug.LogWarning($"[EmbossingSpriteAssigner] Could not find sprite: {spriteName}");
                notFoundCount++;
                continue;
            }
            
            // Assign the sprite
            embossing.embossingIcon = targetSprite;
            EditorUtility.SetDirty(embossing);
            assignedCount++;
            
            Debug.Log($"[EmbossingSpriteAssigner] Assigned sprite '{spriteName}' to '{embossingName}'");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Assignment Complete", 
            $"Assigned: {assignedCount}\nNot Found: {notFoundCount}", 
            "OK");
        
        Debug.Log($"[EmbossingSpriteAssigner] Assignment complete. Assigned: {assignedCount}, Not Found: {notFoundCount}");
        
        // Clear assignments after applying
        assignments.Clear();
    }
}

