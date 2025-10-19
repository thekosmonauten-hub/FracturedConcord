using UnityEngine;
using UnityEditor;

/// <summary>
/// Generates perfect circle sprites for circular health bars.
/// Menu: Tools > Combat UI > Generate Circle Sprite
/// </summary>
public class CircleSpriteGenerator : EditorWindow
{
    private int textureSize = 512;
    private Color circleColor = Color.white;
    private bool antiAliasing = true;
    private string savePath = "Assets/Art/UI/";
    private string spriteName = "CircleSprite";
    
    [MenuItem("Tools/Combat UI/Generate Circle Sprite")]
    public static void ShowWindow()
    {
        var window = GetWindow<CircleSpriteGenerator>("Circle Sprite Generator");
        window.minSize = new Vector2(400, 350);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Circle Sprite Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Generate perfect circle sprites for circular health/mana bars.\n" +
            "Creates a high-quality PNG with transparency.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Configuration
        textureSize = EditorGUILayout.IntSlider("Texture Size:", textureSize, 128, 1024);
        circleColor = EditorGUILayout.ColorField("Circle Color:", circleColor);
        antiAliasing = EditorGUILayout.Toggle("Anti-Aliasing:", antiAliasing);
        
        EditorGUILayout.Space();
        
        spriteName = EditorGUILayout.TextField("Sprite Name:", spriteName);
        savePath = EditorGUILayout.TextField("Save Path:", savePath);
        
        EditorGUILayout.Space();
        
        // Info
        EditorGUILayout.HelpBox(
            $"Will create: {spriteName}.png\n" +
            $"Size: {textureSize}x{textureSize}\n" +
            $"Location: {savePath}",
            MessageType.None
        );
        
        EditorGUILayout.Space();
        
        // Create button
        if (GUILayout.Button("Generate Circle Sprite", GUILayout.Height(40)))
        {
            GenerateCircleSprite();
        }
        
        EditorGUILayout.Space();
        
        // Quick presets
        EditorGUILayout.LabelField("Quick Presets:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("White Circle"))
        {
            circleColor = Color.white;
            spriteName = "WhiteCircle";
        }
        
        if (GUILayout.Button("Health Ring"))
        {
            circleColor = new Color(0f, 0.8f, 0.4f);
            spriteName = "HealthCircle";
        }
        
        if (GUILayout.Button("Mana Ring"))
        {
            circleColor = new Color(0.3f, 0.5f, 1f);
            spriteName = "ManaCircle";
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void GenerateCircleSprite()
    {
        // Ensure directory exists
        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }
        
        // Create texture
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        
        // Draw circle
        float center = textureSize / 2f;
        float radius = center - 2f; // Leave 2px border for AA
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                Color pixelColor;
                
                if (antiAliasing)
                {
                    // Anti-aliased edges
                    float edgeSoftness = 2f;
                    float alpha = 1f - Mathf.Clamp01((distance - radius + edgeSoftness) / edgeSoftness);
                    pixelColor = new Color(circleColor.r, circleColor.g, circleColor.b, alpha);
                }
                else
                {
                    // Hard edges
                    if (distance <= radius)
                    {
                        pixelColor = circleColor;
                    }
                    else
                    {
                        pixelColor = new Color(0, 0, 0, 0); // Transparent
                    }
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        
        // Save to PNG
        byte[] bytes = texture.EncodeToPNG();
        string fullPath = $"{savePath}{spriteName}.png";
        System.IO.File.WriteAllBytes(fullPath, bytes);
        
        // Refresh and import
        AssetDatabase.Refresh();
        
        // Configure texture importer
        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        
        // Select the created sprite
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
        Selection.activeObject = sprite;
        EditorGUIUtility.PingObject(sprite);
        
        EditorUtility.DisplayDialog(
            "Success!",
            $"Created circle sprite: {spriteName}.png\n\n" +
            $"Location: {fullPath}\n\n" +
            "The sprite is now selected in the Project window.\n" +
            "Drag it to your Image component's Source Image field!",
            "OK"
        );
        
        Debug.Log($"<color=green>Created circle sprite:</color> {fullPath}");
    }
}

