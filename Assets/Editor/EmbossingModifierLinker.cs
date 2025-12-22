using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor tool to auto-link EmbossingModifierDefinition assets to their corresponding EmbossingEffect assets
/// Finds all modifiers with matching linkedEmbossingId and populates the modifierIds list
/// </summary>
public class EmbossingModifierLinker : EditorWindow
{
    private string embossingFolder = "Assets/Resources/Embossings";
    private string modifierFolder = "Assets/Resources/EmbossingModifiers";
    private bool overwriteExisting = false;
    
    [MenuItem("Tools/Dexiled/Link Embossing Modifiers to Embossing Assets")]
    public static void ShowWindow()
    {
        GetWindow<EmbossingModifierLinker>("Embossing Modifier Linker");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Embossing Modifier Linker", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Folder Paths", EditorStyles.boldLabel);
        embossingFolder = EditorGUILayout.TextField("Embossing Assets Folder", embossingFolder);
        modifierFolder = EditorGUILayout.TextField("Modifier Assets Folder", modifierFolder);
        
        EditorGUILayout.Space();
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Links", overwriteExisting);
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This tool finds all EmbossingModifierDefinition assets and links them to their corresponding EmbossingEffect assets based on linkedEmbossingId.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Link All Modifiers"))
        {
            LinkAllModifiers();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Link Single Embossing"))
        {
            LinkSingleEmbossing();
        }
    }
    
    private void LinkAllModifiers()
    {
        // Load all modifier definitions
        string[] modifierGuids = AssetDatabase.FindAssets("t:EmbossingModifierDefinition", new[] { modifierFolder });
        
        if (modifierGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Modifiers Found", $"No EmbossingModifierDefinition assets found in {modifierFolder}", "OK");
            return;
        }
        
        // Group modifiers by linkedEmbossingId
        Dictionary<string, List<string>> modifiersByEmbossingId = new Dictionary<string, List<string>>();
        
        foreach (string guid in modifierGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EmbossingModifierDefinition modifier = AssetDatabase.LoadAssetAtPath<EmbossingModifierDefinition>(path);
            
            if (modifier != null && !string.IsNullOrEmpty(modifier.linkedEmbossingId))
            {
                if (!modifiersByEmbossingId.ContainsKey(modifier.linkedEmbossingId))
                {
                    modifiersByEmbossingId[modifier.linkedEmbossingId] = new List<string>();
                }
                modifiersByEmbossingId[modifier.linkedEmbossingId].Add(modifier.modifierId);
            }
        }
        
        Debug.Log($"[EmbossingModifierLinker] Found {modifiersByEmbossingId.Count} unique embossing IDs with modifiers");
        
        // Load all embossing assets
        string[] embossingGuids = AssetDatabase.FindAssets("t:EmbossingEffect", new[] { embossingFolder });
        
        if (embossingGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Embossings Found", $"No EmbossingEffect assets found in {embossingFolder}", "OK");
            return;
        }
        
        int linkedCount = 0;
        int skippedCount = 0;
        int notFoundCount = 0;
        
        foreach (string guid in embossingGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EmbossingEffect embossing = AssetDatabase.LoadAssetAtPath<EmbossingEffect>(path);
            
            if (embossing == null)
                continue;
            
            // Generate embossing ID if not set
            if (string.IsNullOrEmpty(embossing.embossingId))
            {
                embossing.embossingId = GenerateEmbossingId(embossing.embossingName);
            }
            
            // Find matching modifiers
            if (modifiersByEmbossingId.ContainsKey(embossing.embossingId))
            {
                List<string> modifierIds = modifiersByEmbossingId[embossing.embossingId];
                
                // Check if we should overwrite
                if (embossing.modifierIds != null && embossing.modifierIds.Count > 0 && !overwriteExisting)
                {
                    Debug.Log($"[EmbossingModifierLinker] Skipping {embossing.embossingName} - already has {embossing.modifierIds.Count} modifier IDs (use 'Overwrite Existing Links' to replace)");
                    skippedCount++;
                    continue;
                }
                
                // Link modifiers
                embossing.modifierIds = new List<string>(modifierIds);
                EditorUtility.SetDirty(embossing);
                
                Debug.Log($"[EmbossingModifierLinker] Linked {modifierIds.Count} modifier(s) to {embossing.embossingName} ({embossing.embossingId}): {string.Join(", ", modifierIds)}");
                linkedCount++;
            }
            else
            {
                Debug.LogWarning($"[EmbossingModifierLinker] No modifiers found for embossing: {embossing.embossingName} (ID: {embossing.embossingId})");
                notFoundCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Linking Complete", 
            $"Linked: {linkedCount}\nSkipped: {skippedCount}\nNo Modifiers Found: {notFoundCount}", 
            "OK");
        
        Debug.Log($"[EmbossingModifierLinker] Linking complete. Linked: {linkedCount}, Skipped: {skippedCount}, Not Found: {notFoundCount}");
    }
    
    private void LinkSingleEmbossing()
    {
        // Let user select an embossing asset
        string path = EditorUtility.OpenFilePanel("Select Embossing Asset", embossingFolder, "asset");
        
        if (string.IsNullOrEmpty(path))
            return;
        
        // Convert to relative path if needed
        if (path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
        }
        
        EmbossingEffect embossing = AssetDatabase.LoadAssetAtPath<EmbossingEffect>(path);
        
        if (embossing == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected file is not an EmbossingEffect asset", "OK");
            return;
        }
        
        // Generate embossing ID if not set
        if (string.IsNullOrEmpty(embossing.embossingId))
        {
            embossing.embossingId = GenerateEmbossingId(embossing.embossingName);
        }
        
        // Find all modifiers with matching linkedEmbossingId
        string[] modifierGuids = AssetDatabase.FindAssets("t:EmbossingModifierDefinition", new[] { modifierFolder });
        List<string> modifierIds = new List<string>();
        
        foreach (string guid in modifierGuids)
        {
            string modifierPath = AssetDatabase.GUIDToAssetPath(guid);
            EmbossingModifierDefinition modifier = AssetDatabase.LoadAssetAtPath<EmbossingModifierDefinition>(modifierPath);
            
            if (modifier != null && modifier.linkedEmbossingId == embossing.embossingId)
            {
                modifierIds.Add(modifier.modifierId);
            }
        }
        
        if (modifierIds.Count == 0)
        {
            EditorUtility.DisplayDialog("No Modifiers Found", $"No modifiers found for embossing '{embossing.embossingName}' (ID: {embossing.embossingId})", "OK");
            return;
        }
        
        // Link modifiers
        embossing.modifierIds = new List<string>(modifierIds);
        EditorUtility.SetDirty(embossing);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Linked", $"Linked {modifierIds.Count} modifier(s) to {embossing.embossingName}:\n{string.Join("\n", modifierIds)}", "OK");
        Debug.Log($"[EmbossingModifierLinker] Linked {modifierIds.Count} modifier(s) to {embossing.embossingName}: {string.Join(", ", modifierIds)}");
    }
    
    private string GenerateEmbossingId(string embossingName)
    {
        if (string.IsNullOrEmpty(embossingName))
            return "";
        
        // Remove "of " prefix if present
        string id = embossingName.Replace("of ", "").Replace("Of ", "");
        // Convert to lowercase and replace spaces with underscores
        id = id.ToLower().Replace(" ", "_");
        // Remove special characters
        id = System.Text.RegularExpressions.Regex.Replace(id, @"[^a-z0-9_]", "");
        return id;
    }
}

