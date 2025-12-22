using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor tool to find duplicate sprites assigned to Embossing assets
/// </summary>
public class EmbossingDuplicateSpriteChecker : EditorWindow
{
    [MenuItem("Tools/Dexiled/Check Embossing Duplicate Sprites")]
    public static void ShowWindow()
    {
        GetWindow<EmbossingDuplicateSpriteChecker>("Embossing Duplicate Sprite Checker");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Embossing Duplicate Sprite Checker", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool checks all EmbossingEffect assets for duplicate sprite assignments.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Check for Duplicates"))
        {
            CheckForDuplicates();
        }
    }
    
    private void CheckForDuplicates()
    {
        // Find all EmbossingEffect assets
        string[] guids = AssetDatabase.FindAssets("t:EmbossingEffect", new[] { "Assets/Resources/Embossings" });
        
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Assets Found", "No EmbossingEffect assets found in Assets/Resources/Embossings", "OK");
            return;
        }
        
        // Dictionary to track sprite -> list of embossings
        Dictionary<Sprite, List<string>> spriteToEmbossings = new Dictionary<Sprite, List<string>>();
        int nullCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EmbossingEffect embossing = AssetDatabase.LoadAssetAtPath<EmbossingEffect>(path);
            
            if (embossing == null)
                continue;
            
            Sprite sprite = embossing.embossingIcon;
            
            if (sprite == null)
            {
                nullCount++;
                continue;
            }
            
            if (!spriteToEmbossings.ContainsKey(sprite))
            {
                spriteToEmbossings[sprite] = new List<string>();
            }
            
            spriteToEmbossings[sprite].Add(embossing.embossingName);
        }
        
        // Find duplicates (sprites used by more than one embossing)
        var duplicates = spriteToEmbossings.Where(kvp => kvp.Value.Count > 1).ToList();
        
        // Build report
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine($"Total Embossings: {guids.Length}");
        report.AppendLine($"Embossings with no sprite: {nullCount}");
        report.AppendLine($"Unique sprites: {spriteToEmbossings.Count}");
        report.AppendLine($"Duplicate sprites: {duplicates.Count}");
        report.AppendLine();
        
        if (duplicates.Count > 0)
        {
            report.AppendLine("=== DUPLICATE SPRITES ===");
            report.AppendLine();
            
            foreach (var duplicate in duplicates.OrderByDescending(d => d.Value.Count))
            {
                string spriteName = duplicate.Key != null ? duplicate.Key.name : "NULL";
                report.AppendLine($"Sprite: {spriteName} (used by {duplicate.Value.Count} embossings)");
                foreach (string embossingName in duplicate.Value)
                {
                    report.AppendLine($"  - {embossingName}");
                }
                report.AppendLine();
            }
        }
        else
        {
            report.AppendLine("✓ No duplicate sprites found!");
        }
        
        // Show report
        Debug.Log($"[EmbossingDuplicateSpriteChecker]\n{report.ToString()}");
        
        // Also show in a scrollable window
        ShowReportWindow(report.ToString(), duplicates);
    }
    
    private void ShowReportWindow(string report, List<System.Collections.Generic.KeyValuePair<Sprite, List<string>>> duplicates)
    {
        EditorWindow reportWindow = GetWindow<DuplicateReportWindow>("Duplicate Sprite Report");
        reportWindow.minSize = new Vector2(500, 400);
        ((DuplicateReportWindow)reportWindow).SetReport(report, duplicates);
    }
}

/// <summary>
/// Window to display the duplicate sprite report
/// </summary>
public class DuplicateReportWindow : EditorWindow
{
    private string reportText = "";
    private Vector2 scrollPosition;
    private List<System.Collections.Generic.KeyValuePair<Sprite, List<string>>> duplicates;
    
    public void SetReport(string report, List<System.Collections.Generic.KeyValuePair<Sprite, List<string>>> duplicatesList)
    {
        reportText = report;
        duplicates = duplicatesList;
    }
    
    void OnGUI()
    {
        GUILayout.Label("Duplicate Sprite Report", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(reportText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space();
        
        if (duplicates != null && duplicates.Count > 0)
        {
            EditorGUILayout.HelpBox($"Found {duplicates.Count} duplicate sprite(s). Check the console for full details.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("No duplicates found!", MessageType.Info);
        }
    }
}

