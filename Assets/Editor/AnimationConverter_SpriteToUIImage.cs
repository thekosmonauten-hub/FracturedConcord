using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Converts SpriteRenderer animations to UI Image animations.
/// Preserves all keyframes, timing, and settings.
/// Supports single and bulk conversion.
/// </summary>
public class AnimationConverter_SpriteToUIImage : EditorWindow
{
    private AnimationClip sourceClip;
    private string newClipName = "";
    private bool preserveOriginal = true;
    
    // Bulk conversion
    private Vector2 scrollPosition;
    private List<string> selectedFolders = new List<string>();
    private List<string> foundAnimations = new List<string>();
    private Dictionary<string, bool> animationToggleStates = new Dictionary<string, bool>();
    
    [MenuItem("Tools/Dexiled/Convert Animation: SpriteRenderer → UI Image")]
    public static void ShowWindow()
    {
        GetWindow<AnimationConverter_SpriteToUIImage>("Animation Converter");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Convert SpriteRenderer Animation to UI Image", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool converts animations that target SpriteRenderer (classID 212) " +
            "to target UI Image component instead (classID 114).\n\n" +
            "Perfect for fixing animations that don't work with UI-based systems!",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        // Source clip selection
        sourceClip = (AnimationClip)EditorGUILayout.ObjectField(
            "Source Animation Clip",
            sourceClip,
            typeof(AnimationClip),
            false
        );
        
        if (sourceClip != null)
        {
            // Auto-generate name
            if (string.IsNullOrEmpty(newClipName))
            {
                newClipName = sourceClip.name + "_UI";
            }
            
            EditorGUILayout.Space();
            newClipName = EditorGUILayout.TextField("New Clip Name", newClipName);
            preserveOriginal = EditorGUILayout.Toggle("Keep Original Clip", preserveOriginal);
            
            EditorGUILayout.Space();
            
            // Show source clip info
            EditorGUILayout.LabelField("Source Clip Info:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  Duration:", sourceClip.length.ToString("F2") + " seconds");
            EditorGUILayout.LabelField("  Frame Rate:", sourceClip.frameRate.ToString());
            EditorGUILayout.LabelField("  Loop Time:", sourceClip.isLooping.ToString());
            
            EditorGUILayout.Space();
            
            // Convert button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Convert Animation", GUILayout.Height(40)))
            {
                ConvertAnimation();
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Select an animation clip that uses SpriteRenderer.\n" +
                "Usually found in: Assets/Art/Enemies/[EnemyName]/",
                MessageType.Warning
            );
        }
        
        GUILayout.Space(20);
        
        // Bulk conversion section
        DrawBulkConversionSection();
        
        GUILayout.Space(20);
        
        // Legacy batch convert section (keep for backward compatibility)
        GUILayout.Label("Legacy: Batch Convert SkeletonArcher", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Quick fix: Converts all SkeletonArcher animations at once!",
            MessageType.Info
        );
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Convert All SkeletonArcher Animations", GUILayout.Height(30)))
        {
            BatchConvertSkeletonArcher();
        }
        GUI.backgroundColor = Color.white;
    }
    
    private void DrawBulkConversionSection()
    {
        GUILayout.Label("Bulk Conversion", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select folders to scan for animation clips. All .anim files that target SpriteRenderer will be converted.\n" +
            "Converted files will be named: [OriginalName]_UI.anim and placed in the same folder as the source.",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        // Folder selection
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Folder to Scan", GUILayout.Height(25)))
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder to Scan", "Assets", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                // Convert to relative path
                string relativePath = "Assets" + folderPath.Replace(Application.dataPath, "").Replace('\\', '/');
                if (Directory.Exists(relativePath) && !selectedFolders.Contains(relativePath))
                {
                    selectedFolders.Add(relativePath);
                    ScanFolderForAnimations(relativePath);
                }
            }
        }
        
        if (selectedFolders.Count > 0)
        {
            if (GUILayout.Button("Clear All Folders", GUILayout.Height(25), GUILayout.Width(120)))
            {
                selectedFolders.Clear();
                foundAnimations.Clear();
                animationToggleStates.Clear();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Display selected folders
        if (selectedFolders.Count > 0)
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField($"Selected Folders: {selectedFolders.Count}", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            for (int i = selectedFolders.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(selectedFolders[i], GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    string folder = selectedFolders[i];
                    selectedFolders.RemoveAt(i);
                    // Remove animations from this folder
                    foundAnimations.RemoveAll(path => path.StartsWith(folder));
                    // Re-scan if needed
                    if (selectedFolders.Count > 0)
                    {
                        RefreshAnimationList();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("Refresh Animation List", GUILayout.Height(25)))
            {
                RefreshAnimationList();
            }
        }
        
        // Display found animations
        if (foundAnimations.Count > 0)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField($"Found {foundAnimations.Count} Animation Clip(s)", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", GUILayout.Width(80)))
            {
                foreach (string path in foundAnimations)
                {
                    animationToggleStates[path] = true;
                }
            }
            if (GUILayout.Button("Deselect All", GUILayout.Width(80)))
            {
                foreach (string path in foundAnimations)
                {
                    animationToggleStates[path] = false;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (string animPath in foundAnimations)
            {
                if (!animationToggleStates.ContainsKey(animPath))
                {
                    animationToggleStates[animPath] = true; // Default to selected
                }
                
                EditorGUILayout.BeginHorizontal();
                animationToggleStates[animPath] = EditorGUILayout.Toggle(animationToggleStates[animPath], GUILayout.Width(20));
                
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
                if (clip != null)
                {
                    EditorGUILayout.LabelField(Path.GetFileName(animPath), GUILayout.ExpandWidth(true));
                    
                    // Check if already converted
                    string uiPath = animPath.Replace(".anim", "_UI.anim");
                    if (File.Exists(uiPath))
                    {
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField("(Already converted)", GUILayout.Width(120));
                        GUI.color = Color.white;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            
            GUILayout.Space(10);
            
            // Bulk convert button
            int selectedCount = animationToggleStates.Values.Count(v => v);
            EditorGUILayout.LabelField($"Selected: {selectedCount} / {foundAnimations.Count}");
            
            GUI.backgroundColor = selectedCount > 0 ? Color.green : Color.gray;
            EditorGUI.BeginDisabledGroup(selectedCount == 0);
            if (GUILayout.Button($"Convert {selectedCount} Selected Animation(s)", GUILayout.Height(40)))
            {
                BulkConvertSelected();
            }
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = Color.white;
        }
    }
    
    private void ScanFolderForAnimations(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return;
        
        // Find all .anim files in this folder (including subfolders)
        string[] animGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folderPath });
        
        foreach (string guid in animGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".anim"))
            {
                // Check if this animation targets SpriteRenderer
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null && IsSpriteRendererAnimation(clip))
                {
                    if (!foundAnimations.Contains(path))
                    {
                        foundAnimations.Add(path);
                        if (!animationToggleStates.ContainsKey(path))
                        {
                            animationToggleStates[path] = true; // Default to selected
                        }
                    }
                }
            }
        }
        
        // Count how many actually target SpriteRenderer
        int spriteRendererCount = 0;
        foreach (string path in foundAnimations)
        {
            if (path.StartsWith(folderPath))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null && IsSpriteRendererAnimation(clip))
                {
                    spriteRendererCount++;
                }
            }
        }
        
        Debug.Log($"Scanned {folderPath}: Found {animGuids.Length} animation clip(s), {spriteRendererCount} targeting SpriteRenderer");
    }
    
    private bool IsSpriteRendererAnimation(AnimationClip clip)
    {
        EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var binding in bindings)
        {
            if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
            {
                return true;
            }
        }
        return false;
    }
    
    private void RefreshAnimationList()
    {
        foundAnimations.Clear();
        animationToggleStates.Clear();
        
        foreach (string folder in selectedFolders)
        {
            ScanFolderForAnimations(folder);
        }
    }
    
    private void BulkConvertSelected()
    {
        List<string> toConvert = foundAnimations.Where(path => animationToggleStates.ContainsKey(path) && animationToggleStates[path]).ToList();
        
        if (toConvert.Count == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select at least one animation to convert.", "OK");
            return;
        }
        
        if (!EditorUtility.DisplayDialog(
            "Confirm Bulk Conversion",
            $"Convert {toConvert.Count} animation clip(s)?\n\n" +
            "This will create new _UI versions in the same folders.\n" +
            "Original files will be preserved.",
            "Convert",
            "Cancel"))
        {
            return;
        }
        
        List<string> converted = new List<string>();
        List<string> skipped = new List<string>();
        List<string> errors = new List<string>();
        
        foreach (string animPath in toConvert)
        {
            // Check if already converted
            string uiPath = animPath.Replace(".anim", "_UI.anim");
            if (File.Exists(uiPath))
            {
                skipped.Add(Path.GetFileName(animPath) + " (already converted)");
                continue;
            }
            
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            if (clip == null)
            {
                skipped.Add(Path.GetFileName(animPath) + " (not found)");
                continue;
            }
            
            try
            {
                // Convert the animation
                AnimationClip newClip = ConvertClip(clip);
                
                if (newClip != null)
                {
                    // Save in same folder with _UI suffix
                    AssetDatabase.CreateAsset(newClip, uiPath);
                    converted.Add(Path.GetFileName(uiPath));
                    Debug.Log($"✓ Converted: {Path.GetFileName(animPath)} → {Path.GetFileName(uiPath)}");
                }
                else
                {
                    skipped.Add(Path.GetFileName(animPath) + " (not a SpriteRenderer animation)");
                }
            }
            catch (System.Exception e)
            {
                errors.Add(Path.GetFileName(animPath) + " (" + e.Message + ")");
                Debug.LogError($"✗ Failed to convert {animPath}: {e.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Report results
        string report = $"Bulk Conversion Complete!\n\n";
        
        if (converted.Count > 0)
        {
            report += $"✓ Successfully converted {converted.Count} animation(s):\n";
            foreach (string name in converted.Take(10)) // Show first 10
            {
                report += $"  • {name}\n";
            }
            if (converted.Count > 10)
            {
                report += $"  ... and {converted.Count - 10} more\n";
            }
        }
        
        if (skipped.Count > 0)
        {
            report += $"\n⚠ Skipped {skipped.Count} animation(s):\n";
            foreach (string name in skipped.Take(5)) // Show first 5
            {
                report += $"  • {name}\n";
            }
            if (skipped.Count > 5)
            {
                report += $"  ... and {skipped.Count - 5} more\n";
            }
        }
        
        if (errors.Count > 0)
        {
            report += $"\n✗ Errors ({errors.Count}):\n";
            foreach (string error in errors)
            {
                report += $"  • {error}\n";
            }
        }
        
        Debug.Log($"<color=cyan>{report}</color>");
        EditorUtility.DisplayDialog("Bulk Conversion Complete!", report, "OK");
        
        // Refresh the list to show newly converted files
        RefreshAnimationList();
    }
    
    private AnimationClip ConvertClip(AnimationClip sourceClip)
    {
        if (sourceClip == null)
            return null;
        
        // Check if it's a SpriteRenderer animation
        EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(sourceClip);
        bool foundSpriteBinding = false;
        
        foreach (var binding in bindings)
        {
            if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
            {
                foundSpriteBinding = true;
                break;
            }
        }
        
        if (!foundSpriteBinding)
        {
            return null; // Not a SpriteRenderer animation
        }
        
        // Create new clip
        AnimationClip newClip = new AnimationClip();
        newClip.frameRate = sourceClip.frameRate;
        
        // Copy animation clip settings
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(sourceClip);
        AnimationUtility.SetAnimationClipSettings(newClip, settings);
        
        // Copy events
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(sourceClip);
        AnimationUtility.SetAnimationEvents(newClip, events);
        
        // Convert bindings
        foreach (var binding in bindings)
        {
            if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
            {
                // Get original keyframes
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(sourceClip, binding);
                
                // Create new binding for UI Image
                EditorCurveBinding newBinding = new EditorCurveBinding();
                newBinding.path = binding.path;
                newBinding.type = typeof(Image); // Change to Image instead of SpriteRenderer
                newBinding.propertyName = "m_Sprite"; // Same property name
                
                // Set keyframes on new clip
                AnimationUtility.SetObjectReferenceCurve(newClip, newBinding, keyframes);
            }
        }
        
        return newClip;
    }
    
    private void ConvertAnimation()
    {
        if (sourceClip == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a source animation clip!", "OK");
            return;
        }
        
        // Get all sprite bindings
        EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(sourceClip);
        
        bool foundSpriteBinding = false;
        foreach (var binding in bindings)
        {
            // Look for SpriteRenderer.sprite bindings (type 212)
            if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
            {
                foundSpriteBinding = true;
                break;
            }
        }
        
        if (!foundSpriteBinding)
        {
            EditorUtility.DisplayDialog(
                "Not a SpriteRenderer Animation",
                "This animation doesn't target SpriteRenderer.m_Sprite.\n" +
                "It might already be a UI Image animation or use different properties.",
                "OK"
            );
            return;
        }
        
        // Use the shared ConvertClip method
        AnimationClip newClip = ConvertClip(sourceClip);
        
        if (newClip == null)
        {
            EditorUtility.DisplayDialog(
                "Not a SpriteRenderer Animation",
                "This animation doesn't target SpriteRenderer.m_Sprite.\n" +
                "It might already be a UI Image animation or use different properties.",
                "OK"
            );
            return;
        }
        
        // Save new clip
        string sourcePath = AssetDatabase.GetAssetPath(sourceClip);
        string directory = System.IO.Path.GetDirectoryName(sourcePath);
        string newPath = System.IO.Path.Combine(directory, newClipName + ".anim");
        
        AssetDatabase.CreateAsset(newClip, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select new clip
        Selection.activeObject = newClip;
        EditorGUIUtility.PingObject(newClip);
        
        Debug.Log($"<color=green>✓ Successfully created UI Image animation: {newPath}</color>");
        
        EditorUtility.DisplayDialog(
            "Conversion Complete!",
            $"Created: {newClipName}.anim\n\n" +
            "Don't forget to assign the new clip to your Animator Controller!",
            "OK"
        );
    }
    
    private void BatchConvertSkeletonArcher()
    {
        string[] animPaths = new string[]
        {
            "Assets/Art/Enemies/SkeletonArcher/SkeletonArcher_Idle.anim",
            "Assets/Art/Enemies/SkeletonArcher/SkeletonArcher_Attack.anim",
            "Assets/Art/Enemies/SkeletonArcher/SkeletonArcher_Hit.anim"
        };
        
        List<string> converted = new List<string>();
        List<string> skipped = new List<string>();
        
        foreach (string path in animPaths)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                skipped.Add(System.IO.Path.GetFileName(path) + " (not found)");
                continue;
            }
            
            // Check if already converted
            string uiPath = path.Replace(".anim", "_UI.anim");
            if (System.IO.File.Exists(uiPath))
            {
                skipped.Add(System.IO.Path.GetFileName(path) + " (already exists)");
                continue;
            }
            
            // Convert
            sourceClip = clip;
            newClipName = clip.name + "_UI";
            
            try
            {
                // Use the shared ConvertClip method
                AnimationClip newClip = ConvertClip(clip);
                
                if (newClip != null)
                {
                    // Save
                    AssetDatabase.CreateAsset(newClip, uiPath);
                    converted.Add(System.IO.Path.GetFileName(uiPath));
                }
                else
                {
                    skipped.Add(System.IO.Path.GetFileName(path) + " (not a SpriteRenderer animation)");
                }
            }
            catch (System.Exception e)
            {
                skipped.Add(System.IO.Path.GetFileName(path) + " (error: " + e.Message + ")");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Report
        string report = "Batch Conversion Complete!\n\n";
        
        if (converted.Count > 0)
        {
            report += $"✓ Converted {converted.Count} animation(s):\n";
            foreach (string name in converted)
            {
                report += $"  • {name}\n";
            }
        }
        
        if (skipped.Count > 0)
        {
            report += $"\n⚠ Skipped {skipped.Count} animation(s):\n";
            foreach (string name in skipped)
            {
                report += $"  • {name}\n";
            }
        }
        
        report += "\nNext Step: Update SkeletonArcher_Controller to use the new _UI clips!";
        
        Debug.Log($"<color=cyan>{report}</color>");
        EditorUtility.DisplayDialog("Batch Conversion Complete!", report, "OK");
        
        // Try to open the folder
        EditorUtility.RevealInFinder("Assets/Art/Enemies/SkeletonArcher/");
    }
}














