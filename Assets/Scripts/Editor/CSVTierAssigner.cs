using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

/// <summary>
/// Utility to automatically assign tiers to CSV entries based on their power levels
/// </summary>
public class CSVTierAssigner : EditorWindow
{
    private string csvFilePath = "Assets/Scripts/Documentation/Comprehensive_Mods.csv";
    
    [MenuItem("Dexiled/Auto-Assign CSV Tiers")]
    public static void ShowWindow()
    {
        CSVTierAssigner window = GetWindow<CSVTierAssigner>("CSV Tier Assigner");
        window.minSize = new Vector2(400, 300);
    }
    
    void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("CSV Tier Auto-Assigner", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool automatically assigns tier and minimum level to CSV entries that don't have them.\n" +
            "Tiers are assigned based on the power level of the mod values.",
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        // File Selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("CSV File:", GUILayout.Width(70));
        csvFilePath = EditorGUILayout.TextField(csvFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "Assets", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvFilePath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Auto-Assign Tiers", GUILayout.Height(30)))
        {
            AutoAssignTiers();
        }
        
        if (GUILayout.Button("Preview Missing Tiers", GUILayout.Height(30)))
        {
            PreviewMissingTiers();
        }
    }
    
    private void AutoAssignTiers()
    {
        if (!File.Exists(csvFilePath))
        {
            EditorUtility.DisplayDialog("Error", $"CSV file not found: {csvFilePath}", "OK");
            return;
        }
        
        string[] lines = File.ReadAllLines(csvFilePath);
        List<string> updatedLines = new List<string>();
        
        bool isFirstLine = true;
        int updatedCount = 0;
        
        foreach (string line in lines)
        {
            if (isFirstLine)
            {
                updatedLines.Add(line); // Keep header as-is
                isFirstLine = false;
                continue;
            }
            
            if (string.IsNullOrEmpty(line.Trim()) || line.Trim().StartsWith("#"))
            {
                updatedLines.Add(line); // Keep comments and empty lines
                continue;
            }
            
            string updatedLine = ProcessLine(line, out bool wasUpdated);
            updatedLines.Add(updatedLine);
            if (wasUpdated) updatedCount++;
        }
        
        // Write back to file
        File.WriteAllLines(csvFilePath, updatedLines);
        AssetDatabase.Refresh();
        
        Debug.Log($"Auto-assigned tiers to {updatedCount} entries in {csvFilePath}");
        EditorUtility.DisplayDialog("Complete", $"Auto-assigned tiers to {updatedCount} entries", "OK");
    }
    
    private void PreviewMissingTiers()
    {
        if (!File.Exists(csvFilePath))
        {
            EditorUtility.DisplayDialog("Error", $"CSV file not found: {csvFilePath}", "OK");
            return;
        }
        
        string[] lines = File.ReadAllLines(csvFilePath);
        int missingCount = 0;
        
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line.Trim()) || line.Trim().StartsWith("#") || line.Contains("Category,Prefix"))
                continue;
            
            string[] columns = ParseCSVLine(line);
            if (columns.Length >= 10)
            {
                // Already has tier and level
                continue;
            }
            
            missingCount++;
            if (missingCount <= 10) // Show first 10
            {
                Debug.Log($"Missing tier: {line}");
            }
        }
        
        Debug.Log($"Found {missingCount} entries without tier assignments");
    }
    
    private string ProcessLine(string line, out bool wasUpdated)
    {
        wasUpdated = false;
        string[] columns = ParseCSVLine(line);
        
        // Check if already has tier and level (columns 8 and 9)
        if (columns.Length >= 10)
        {
            return line; // Already has tier and level
        }
        
        if (columns.Length < 8)
        {
            return line; // Not enough columns to process
        }
        
        try
        {
            // Parse min and max values
            float minValue = float.Parse(columns[4].Trim());
            float maxValue = float.Parse(columns[5].Trim());
            string statText = columns[3].Trim();
            string category = columns[0].Trim();
            
            // Determine tier and level based on category and values
            (int tier, int minLevel) = DetermineTierAndLevel(category, statText, minValue, maxValue);
            
            // Add tier and level to the line
            string updatedLine = line + $",{tier},{minLevel}";
            wasUpdated = true;
            
            return updatedLine;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to process line: {line}. Error: {e.Message}");
            return line;
        }
    }
    
    private (int tier, int minLevel) DetermineTierAndLevel(string category, string statText, float minValue, float maxValue)
    {
        float avgValue = (minValue + maxValue) / 2f;
        string lowerCategory = category.ToLower();
        string lowerStat = statText.ToLower();
        
        // Category-specific tier assignment
        if (lowerCategory.Contains("strength") || lowerCategory.Contains("dexterity") || lowerCategory.Contains("intelligence"))
        {
            // Attributes: 5-9 = T9, 10-19 = T6, 20-29 = T3, 30+ = T1
            if (avgValue <= 7) return (9, 1);
            if (avgValue <= 14) return (6, 30);
            if (avgValue <= 24) return (3, 60);
            return (1, 80);
        }
        
        if (lowerCategory.Contains("life"))
        {
            // Life: 15 = T8, 25 = T7, 35 = T6, 45 = T5, 55 = T4, 70 = T2, 90 = T1
            if (avgValue <= 15) return (8, 10);
            if (avgValue <= 25) return (7, 20);
            if (avgValue <= 35) return (6, 30);
            if (avgValue <= 45) return (5, 40);
            if (avgValue <= 55) return (4, 50);
            if (avgValue <= 70) return (2, 70);
            return (1, 80);
        }
        
        if (lowerCategory.Contains("mana"))
        {
            // Mana: similar progression to life but smaller values
            if (avgValue <= 7) return (9, 1);
            if (avgValue <= 12) return (8, 10);
            if (avgValue <= 17) return (7, 20);
            if (avgValue <= 22) return (6, 30);
            if (avgValue <= 27) return (4, 50);
            return (2, 70);
        }
        
        if (lowerCategory.Contains("physical damage") && !lowerStat.Contains("%"))
        {
            // Flat physical damage: 5 = T9, 10 = T8, 15 = T7, 20 = T6, 25 = T5, 35 = T4, 45 = T3, 50+ = T2
            if (avgValue <= 5) return (9, 1);
            if (avgValue <= 10) return (8, 10);
            if (avgValue <= 15) return (7, 20);
            if (avgValue <= 20) return (6, 30);
            if (avgValue <= 25) return (5, 40);
            if (avgValue <= 35) return (4, 50);
            if (avgValue <= 45) return (3, 60);
            return (2, 70);
        }
        
        if (lowerStat.Contains("% increased") || lowerStat.Contains("increased %"))
        {
            // Percentage increases: 20% = T8, 30% = T7, 40% = T6, 50% = T4, 60+ = T2
            if (avgValue <= 20) return (8, 10);
            if (avgValue <= 30) return (7, 20);
            if (avgValue <= 40) return (6, 30);
            if (avgValue <= 50) return (4, 50);
            return (2, 70);
        }
        
        if (lowerCategory.Contains("elemental") || lowerCategory.Contains("fire") || lowerCategory.Contains("cold") || 
            lowerCategory.Contains("lightning") || lowerCategory.Contains("chaos"))
        {
            // Elemental damage: similar to physical but slightly lower
            if (avgValue <= 3) return (9, 1);
            if (avgValue <= 7) return (8, 10);
            if (avgValue <= 12) return (6, 30);
            if (avgValue <= 20) return (4, 50);
            return (2, 70);
        }
        
        if (lowerCategory.Contains("resistance"))
        {
            // Resistances: 10% = T8, 15% = T7, 20% = T6, 25% = T4, 30+ = T2
            if (avgValue <= 10) return (8, 10);
            if (avgValue <= 15) return (7, 20);
            if (avgValue <= 20) return (6, 30);
            if (avgValue <= 25) return (4, 50);
            return (2, 70);
        }
        
        if (lowerCategory.Contains("speed"))
        {
            // Attack/Cast speed: 5% = T8, 8% = T7, 11% = T6, 14% = T5, 17% = T4
            if (avgValue <= 5) return (8, 10);
            if (avgValue <= 8) return (7, 20);
            if (avgValue <= 11) return (6, 30);
            if (avgValue <= 14) return (5, 40);
            return (4, 50);
        }
        
        if (lowerCategory.Contains("critical"))
        {
            // Critical: 10% = T8, 15% = T7, 20% = T6, 25% = T5, 30+ = T3
            if (avgValue <= 10) return (8, 10);
            if (avgValue <= 15) return (7, 20);
            if (avgValue <= 20) return (6, 30);
            if (avgValue <= 25) return (5, 40);
            return (3, 60);
        }
        
        // Default tier assignment based on average value
        if (avgValue <= 5) return (9, 1);
        if (avgValue <= 10) return (8, 10);
        if (avgValue <= 15) return (7, 20);
        if (avgValue <= 20) return (6, 30);
        if (avgValue <= 30) return (5, 40);
        if (avgValue <= 40) return (4, 50);
        if (avgValue <= 50) return (3, 60);
        if (avgValue <= 70) return (2, 70);
        return (1, 80);
    }
    
    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        fields.Add(currentField); // Add the last field
        return fields.ToArray();
    }
}
