using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Debug tool to diagnose enemy display visibility issues
/// </summary>
public class EnemyDisplayDebugger : EditorWindow
{
    [MenuItem("Tools/Dexiled/Debug Enemy Displays")]
    public static void ShowWindow()
    {
        GetWindow<EnemyDisplayDebugger>("Enemy Display Debugger");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Enemy Display Debugger", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool checks all enemy displays in the active scene for visibility issues.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Check All Enemy Displays", GUILayout.Height(40)))
        {
            CheckAllDisplays();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Fix All Portrait Visibility Issues", GUILayout.Height(40)))
        {
            FixAllPortraits();
        }
    }
    
    private void CheckAllDisplays()
    {
        var displays = GameObject.FindObjectsOfType<EnemyCombatDisplay>();
        
        if (displays.Length == 0)
        {
            Debug.LogWarning("[Debug] No EnemyCombatDisplay components found in scene!");
            return;
        }
        
        Debug.Log($"[Debug] Found {displays.Length} enemy displays. Checking...");
        
        foreach (var display in displays)
        {
            Debug.Log($"\n===== {display.gameObject.name} =====");
            
            // Check GameObject active
            Debug.Log($"  GameObject Active: {display.gameObject.activeInHierarchy}");
            
            // Check portrait
            var portrait = display.GetType().GetField("enemyPortrait", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(display) as Image;
            
            if (portrait != null)
            {
                Debug.Log($"  Portrait Found: {portrait.gameObject.name}");
                Debug.Log($"    - Active: {portrait.gameObject.activeInHierarchy}");
                Debug.Log($"    - Enabled: {portrait.enabled}");
                Debug.Log($"    - Sprite: {(portrait.sprite != null ? portrait.sprite.name : "NULL")}");
                Debug.Log($"    - Color: {portrait.color}");
                Debug.Log($"    - Alpha: {portrait.color.a}");
                Debug.Log($"    - RaycastTarget: {portrait.raycastTarget}");
                
                // Check Canvas Group
                var cg = portrait.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    Debug.Log($"    - CanvasGroup Alpha: {cg.alpha}");
                    Debug.Log($"    - CanvasGroup BlocksRaycasts: {cg.blocksRaycasts}");
                }
                
                // Check sorting
                Canvas canvas = portrait.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    Debug.Log($"    - Canvas Sort Order: {canvas.sortingOrder}");
                    Debug.Log($"    - Canvas RenderMode: {canvas.renderMode}");
                }
                
                // Check sibling index
                Debug.Log($"    - Sibling Index: {portrait.transform.GetSiblingIndex()} / {portrait.transform.parent.childCount - 1}");
            }
            else
            {
                Debug.LogError($"  Portrait: NOT FOUND!");
            }
        }
    }
    
    private void FixAllPortraits()
    {
        var displays = GameObject.FindObjectsOfType<EnemyCombatDisplay>();
        
        if (displays.Length == 0)
        {
            Debug.LogWarning("[Fix] No EnemyCombatDisplay components found in scene!");
            return;
        }
        
        int fixedCount = 0;
        
        foreach (var display in displays)
        {
            var portrait = display.GetType().GetField("enemyPortrait", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(display) as Image;
            
            if (portrait != null)
            {
                // Ensure active
                if (!portrait.gameObject.activeSelf)
                {
                    portrait.gameObject.SetActive(true);
                    Debug.Log($"[Fix] Activated portrait for {display.gameObject.name}");
                    fixedCount++;
                }
                
                // Ensure enabled
                if (!portrait.enabled)
                {
                    portrait.enabled = true;
                    Debug.Log($"[Fix] Enabled portrait Image for {display.gameObject.name}");
                    fixedCount++;
                }
                
                // Ensure fully opaque
                if (portrait.color.a < 1f)
                {
                    portrait.color = new Color(portrait.color.r, portrait.color.g, portrait.color.b, 1f);
                    Debug.Log($"[Fix] Set portrait alpha to 1.0 for {display.gameObject.name}");
                    fixedCount++;
                }
                
                // Check CanvasGroup
                var cg = portrait.GetComponent<CanvasGroup>();
                if (cg != null && cg.alpha < 1f)
                {
                    cg.alpha = 1f;
                    Debug.Log($"[Fix] Set CanvasGroup alpha to 1.0 for {display.gameObject.name}");
                    fixedCount++;
                }
                
                // Set as last sibling
                portrait.transform.SetAsLastSibling();
                
                EditorUtility.SetDirty(portrait);
                EditorUtility.SetDirty(display);
            }
        }
        
        Debug.Log($"[Fix] Fixed {fixedCount} portrait visibility issues across {displays.Length} displays");
        
        EditorUtility.DisplayDialog("Fix Complete", 
            $"Processed {displays.Length} enemy displays.\n\n" +
            $"Applied {fixedCount} fixes.\n\n" +
            "Check console for details.", 
            "OK");
    }
}













