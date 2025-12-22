using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Helper tool to identify potentially unused assets in the project.
/// Use this to safely identify what can be deleted before restructuring.
/// </summary>
public class CleanupHelper : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> unusedScenes = new List<string>();
    private List<string> emptyFolders = new List<string>();
    
    [MenuItem("Tools/Cleanup Helper")]
    public static void ShowWindow()
    {
        GetWindow<CleanupHelper>("Cleanup Helper");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Project Cleanup Helper", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool helps identify potentially unused assets. " +
            "Always verify in Unity before deleting anything!",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Scan for Unused Scenes", GUILayout.Height(30)))
        {
            ScanUnusedScenes();
        }
        
        if (GUILayout.Button("Find Empty Folders", GUILayout.Height(30)))
        {
            FindEmptyFolders();
        }
        
        GUILayout.Space(10);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (unusedScenes.Count > 0)
        {
            GUILayout.Label($"Unused Scenes ({unusedScenes.Count}):", EditorStyles.boldLabel);
            foreach (var scene in unusedScenes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(scene);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(scene);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
        }
        
        if (emptyFolders.Count > 0)
        {
            GUILayout.Label($"Empty Folders ({emptyFolders.Count}):", EditorStyles.boldLabel);
            foreach (var folder in emptyFolders)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(folder);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(folder);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Tip: Use Unity's built-in 'Find References in Scene' feature " +
            "to verify if assets are actually used before deleting.",
            MessageType.Info);
    }
    
    private void ScanUnusedScenes()
    {
        unusedScenes.Clear();
        
        // Get all scenes in build settings
        var buildScenes = EditorBuildSettings.scenes
            .Select(s => s.path)
            .ToHashSet();
        
        // Get all .unity files
        var allScenes = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
            .Where(s => s.Replace("\\", "/").StartsWith("Assets/"))
            .ToList();
        
        foreach (var scene in allScenes)
        {
            if (!buildScenes.Contains(scene))
            {
                unusedScenes.Add(scene);
            }
        }
        
        Debug.Log($"Found {unusedScenes.Count} scenes not in build settings.");
    }
    
    private void FindEmptyFolders()
    {
        emptyFolders.Clear();
        
        var assetFolders = Directory.GetDirectories("Assets", "*", SearchOption.AllDirectories)
            .Where(d => d.Replace("\\", "/").StartsWith("Assets/"))
            .ToList();
        
        foreach (var folder in assetFolders)
        {
            var files = Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly)
                .Where(f => !f.EndsWith(".meta"))
                .ToList();
            
            var subdirs = Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly);
            
            if (files.Count == 0 && subdirs.Length == 0)
            {
                emptyFolders.Add(folder.Replace("\\", "/"));
            }
        }
        
        Debug.Log($"Found {emptyFolders.Count} empty folders.");
    }
}

