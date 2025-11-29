using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility script to fix missing references in EnemyAbility assets.
/// Scans all EnemyAbility assets and fixes their effect references.
/// </summary>
public class MazeEnemyCreator_FixReferences : EditorWindow
{
    private string mazeEnemiesPath = "Assets/Resources/Enemies/Maze of Broken Clauses";
    
    [MenuItem("Tools/Maze Enemies/Fix Ability References")]
    public static void ShowWindow()
    {
        GetWindow<MazeEnemyCreator_FixReferences>("Fix Ability References");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Fix EnemyAbility References", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        mazeEnemiesPath = EditorGUILayout.TextField("Enemies Path:", mazeEnemiesPath);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Fix All Ability References", GUILayout.Height(30)))
        {
            FixAllReferences();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("This will:\n" +
            "• Scan all EnemyAbility assets in maze enemy folders\n" +
            "• Find corresponding effect assets in the same folder\n" +
            "• Fix missing references by loading proper asset references", MessageType.Info);
    }
    
    private void FixAllReferences()
    {
        if (!Directory.Exists(mazeEnemiesPath))
        {
            EditorUtility.DisplayDialog("Error", $"Path not found: {mazeEnemiesPath}", "OK");
            return;
        }
        
        // Find all EnemyAbility assets
        string[] abilityGuids = AssetDatabase.FindAssets("t:EnemyAbility", new[] { mazeEnemiesPath });
        
        int fixedCount = 0;
        int totalCount = 0;
        
        foreach (string guid in abilityGuids)
        {
            totalCount++;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyAbility ability = AssetDatabase.LoadAssetAtPath<EnemyAbility>(path);
            
            if (ability == null) continue;
            
            bool wasModified = false;
            string folder = Path.GetDirectoryName(path);
            
            // Fix effects list
            if (ability.effects != null && ability.effects.Count > 0)
            {
                List<AbilityEffect> fixedEffects = new List<AbilityEffect>();
                
                foreach (var effect in ability.effects)
                {
                    if (effect == null)
                    {
                        // Effect reference is missing - try to find it in the folder
                        // Note: This is a fallback - proper fix requires regenerating with SaveEffectAsset
                        continue;
                    }
                    
                    // Check if it's a proper asset reference
                    string effectPath = AssetDatabase.GetAssetPath(effect);
                    if (!string.IsNullOrEmpty(effectPath))
                    {
                        // Reload as proper asset reference
                        AbilityEffect reloadedEffect = AssetDatabase.LoadAssetAtPath<AbilityEffect>(effectPath);
                        if (reloadedEffect != null)
                        {
                            fixedEffects.Add(reloadedEffect);
                            if (reloadedEffect != effect)
                                wasModified = true;
                        }
                        else
                        {
                            fixedEffects.Add(effect);
                        }
                    }
                    else
                    {
                        // Missing reference - try to find by name in folder
                        string effectName = effect.name;
                        string[] effectFiles = Directory.GetFiles(folder, "*.asset", SearchOption.AllDirectories);
                        bool found = false;
                        
                        foreach (string effectFile in effectFiles)
                        {
                            string relativePath = effectFile.Replace(Application.dataPath, "Assets");
                            AbilityEffect foundEffect = AssetDatabase.LoadAssetAtPath<AbilityEffect>(relativePath);
                            if (foundEffect != null && foundEffect.name == effectName)
                            {
                                fixedEffects.Add(foundEffect);
                                found = true;
                                wasModified = true;
                                break;
                            }
                        }
                        
                        if (!found)
                        {
                            Debug.LogWarning($"[Fix References] Could not find effect '{effectName}' for ability '{ability.displayName}' at {path}");
                        }
                    }
                }
                
                if (wasModified && fixedEffects.Count > 0)
                {
                    ability.effects = fixedEffects;
                    EditorUtility.SetDirty(ability);
                    fixedCount++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Complete", 
            $"Fixed {fixedCount} out of {totalCount} abilities.\n\n" +
            $"If some references are still missing, you may need to regenerate the abilities using the Creator tool.", 
            "OK");
    }
}

