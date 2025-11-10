using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Test tool to force enemy sprites to be visible
/// </summary>
public class EnemyVisibilityTester : EditorWindow
{
    [MenuItem("Tools/Dexiled/Test Enemy Visibility")]
    public static void ShowWindow()
    {
        GetWindow<EnemyVisibilityTester>("Enemy Visibility Tester");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Enemy Visibility Tester", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool tests different approaches to make enemy sprites visible.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Force All Enemies to Center Screen", GUILayout.Height(40)))
        {
            ForceEnemiesToCenter();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Set All Enemies to Test Sprite", GUILayout.Height(40)))
        {
            SetTestSprites();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Make All Enemies Bright Red", GUILayout.Height(40)))
        {
            MakeEnemiesBrightRed();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Reset All Enemy Positions", GUILayout.Height(40)))
        {
            ResetPositions();
        }
    }
    
    private void ForceEnemiesToCenter()
    {
        var displays = GameObject.FindObjectsOfType<EnemyCombatDisplay>();
        
        if (displays.Length == 0)
        {
            Debug.LogWarning("[Visibility Test] No EnemyCombatDisplay components found!");
            return;
        }
        
        Debug.Log($"[Visibility Test] Moving {displays.Length} enemies to center screen...");
        
        foreach (var display in displays)
        {
            var portrait = display.GetType().GetField("enemyPortrait", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(display) as Image;
            
            if (portrait != null)
            {
                // Move to center of screen
                portrait.rectTransform.anchoredPosition = Vector2.zero;
                portrait.rectTransform.sizeDelta = new Vector2(200, 200); // Make it big
                
                // Ensure it's visible
                portrait.color = Color.white;
                portrait.enabled = true;
                portrait.gameObject.SetActive(true);
                
                Debug.Log($"[Visibility Test] Moved {display.gameObject.name} to center");
            }
        }
        
        EditorUtility.DisplayDialog("Test Complete", 
            $"Moved {displays.Length} enemies to center screen.\n\n" +
            "Check if you can see them now!", 
            "OK");
    }
    
    private void SetTestSprites()
    {
        var displays = GameObject.FindObjectsOfType<EnemyCombatDisplay>();
        
        if (displays.Length == 0)
        {
            Debug.LogWarning("[Test Sprite] No EnemyCombatDisplay components found!");
            return;
        }
        
        // Try to find a simple test sprite
        Sprite testSprite = Resources.Load<Sprite>("UI/DefaultSprite");
        if (testSprite == null)
        {
            // Create a simple colored sprite
            testSprite = CreateColoredSprite();
        }
        
        Debug.Log($"[Test Sprite] Setting test sprites for {displays.Length} enemies...");
        
        foreach (var display in displays)
        {
            var portrait = display.GetType().GetField("enemyPortrait", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(display) as Image;
            
            if (portrait != null && testSprite != null)
            {
                portrait.sprite = testSprite;
                portrait.color = Color.white;
                portrait.enabled = true;
                
                Debug.Log($"[Test Sprite] Set test sprite for {display.gameObject.name}");
            }
        }
        
        EditorUtility.DisplayDialog("Test Complete", 
            $"Set test sprites for {displays.Length} enemies.\n\n" +
            "Check if you can see them now!", 
            "OK");
    }
    
    private void MakeEnemiesBrightRed()
    {
        var displays = GameObject.FindObjectsOfType<EnemyCombatDisplay>();
        
        if (displays.Length == 0)
        {
            Debug.LogWarning("[Bright Red] No EnemyCombatDisplay components found!");
            return;
        }
        
        Debug.Log($"[Bright Red] Making {displays.Length} enemies bright red...");
        
        foreach (var display in displays)
        {
            var portrait = display.GetType().GetField("enemyPortrait", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(display) as Image;
            
            if (portrait != null)
            {
                portrait.color = Color.red;
                portrait.enabled = true;
                portrait.gameObject.SetActive(true);
                
                Debug.Log($"[Bright Red] Made {display.gameObject.name} bright red");
            }
        }
        
        EditorUtility.DisplayDialog("Test Complete", 
            $"Made {displays.Length} enemies bright red.\n\n" +
            "Check if you can see red squares now!", 
            "OK");
    }
    
    private void ResetPositions()
    {
        var displays = GameObject.FindObjectsOfType<EnemyCombatDisplay>();
        
        if (displays.Length == 0)
        {
            Debug.LogWarning("[Reset] No EnemyCombatDisplay components found!");
            return;
        }
        
        Debug.Log($"[Reset] Resetting positions for {displays.Length} enemies...");
        
        foreach (var display in displays)
        {
            var portrait = display.GetType().GetField("enemyPortrait", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(display) as Image;
            
            if (portrait != null)
            {
                // Reset to default position (you may need to adjust this)
                portrait.rectTransform.anchoredPosition = new Vector2(-200, 0);
                portrait.color = Color.white;
                
                Debug.Log($"[Reset] Reset position for {display.gameObject.name}");
            }
        }
        
        EditorUtility.DisplayDialog("Reset Complete", 
            $"Reset positions for {displays.Length} enemies.", 
            "OK");
    }
    
    private Sprite CreateColoredSprite()
    {
        // Create a simple 64x64 red square
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.red;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
    }
}












