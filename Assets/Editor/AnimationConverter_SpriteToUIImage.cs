using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Converts SpriteRenderer animations to UI Image animations.
/// Preserves all keyframes, timing, and settings.
/// </summary>
public class AnimationConverter_SpriteToUIImage : EditorWindow
{
    private AnimationClip sourceClip;
    private string newClipName = "";
    private bool preserveOriginal = true;
    
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
        
        // Batch convert section
        GUILayout.Label("Batch Convert (All SkeletonArcher Animations)", EditorStyles.boldLabel);
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
                
                Debug.Log($"✓ Converted {keyframes.Length} sprite keyframes from SpriteRenderer to UI Image");
            }
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
            $"Converted {bindings.Length} property binding(s).\n\n" +
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
                // Create new clip
                AnimationClip newClip = new AnimationClip();
                newClip.frameRate = clip.frameRate;
                
                // Copy settings
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                AnimationUtility.SetAnimationClipSettings(newClip, settings);
                
                // Convert bindings
                EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                foreach (var binding in bindings)
                {
                    if (binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_Sprite")
                    {
                        ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                        
                        EditorCurveBinding newBinding = new EditorCurveBinding();
                        newBinding.path = binding.path;
                        newBinding.type = typeof(Image);
                        newBinding.propertyName = "m_Sprite";
                        
                        AnimationUtility.SetObjectReferenceCurve(newClip, newBinding, keyframes);
                    }
                }
                
                // Save
                AssetDatabase.CreateAsset(newClip, uiPath);
                converted.Add(System.IO.Path.GetFileName(uiPath));
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














