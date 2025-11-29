using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Unity Editor tool to quickly create circular health/mana bars.
/// Menu: Tools > Combat UI > Create Circular Bar
/// </summary>
public class CircularBarCreator : EditorWindow
{
    private BarType barType = BarType.Health;
    private string barName = "CircularHealthBar";
    private float barSize = 100f;
    private bool addBackground = true;
    private bool addBorder = true;
    private bool addText = true;
    private bool addGlow = false;
    private FillOrigin fillOrigin = FillOrigin.Top;
    
    [MenuItem("Tools/Combat UI/Create Circular Bar")]
    public static void ShowWindow()
    {
        var window = GetWindow<CircularBarCreator>("Circular Bar Creator");
        window.minSize = new Vector2(400, 500);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Circular Bar Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Quickly create circular health/mana bars with all components configured.\n" +
            "Creates layered UI with background, fill, border, and text.", 
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Configuration
        barType = (BarType)EditorGUILayout.EnumPopup("Bar Type:", barType);
        barName = EditorGUILayout.TextField("Bar Name:", barName);
        barSize = EditorGUILayout.Slider("Bar Size:", barSize, 50f, 200f);
        fillOrigin = (FillOrigin)EditorGUILayout.EnumPopup("Fill Origin:", fillOrigin);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Components:", EditorStyles.boldLabel);
        addBackground = EditorGUILayout.Toggle("Add Background", addBackground);
        addBorder = EditorGUILayout.Toggle("Add Border", addBorder);
        addText = EditorGUILayout.Toggle("Add Text Display", addText);
        addGlow = EditorGUILayout.Toggle("Add Glow Effect", addGlow);
        
        EditorGUILayout.Space();
        
        // Preview
        EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
        string preview = GeneratePreview();
        EditorGUILayout.HelpBox(preview, MessageType.None);
        
        EditorGUILayout.Space();
        
        // Create button
        if (GUILayout.Button("Create Circular Bar", GUILayout.Height(40)))
        {
            CreateCircularBar();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Created bar will be added to the currently selected GameObject in the hierarchy.\n" +
            "If nothing is selected, it will be created under the active Canvas.", 
            MessageType.Info
        );
    }
    
    private string GeneratePreview()
    {
        string preview = $"Will create: {barName}\n";
        preview += $"Type: {barType}\n";
        preview += $"Size: {barSize}x{barSize}\n";
        preview += $"Drains from: {fillOrigin}\n\n";
        preview += "Hierarchy:\n";
        preview += $"└── {barName}\n";
        if (addGlow) preview += "    ├── Glow (Image)\n";
        if (addBackground) preview += "    ├── Background (Image)\n";
        preview += "    ├── Fill (Image + CircularHealthBar)\n";
        if (addBorder) preview += "    ├── Border (Image)\n";
        if (addText) preview += "    └── Text (Text)\n";
        
        return preview;
    }
    
    private void CreateCircularBar()
    {
        // Get or find canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene! Create a Canvas first.", "OK");
            return;
        }
        
        // Get parent (selected object or canvas)
        Transform parent = Selection.activeTransform;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            parent = canvas.transform;
        }
        
        // Create root GameObject
        GameObject root = new GameObject(barName);
        root.transform.SetParent(parent, false);
        
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(barSize, barSize);
        
        // Add layers
        if (addGlow)
        {
            CreateGlow(root.transform);
        }
        
        if (addBackground)
        {
            CreateBackground(root.transform);
        }
        
        GameObject fillObj = CreateFill(root.transform);
        
        if (addBorder)
        {
            CreateBorder(root.transform);
        }
        
        if (addText)
        {
            CreateText(root.transform);
        }
        
        // Select the created object
        Selection.activeGameObject = root;
        
        EditorUtility.DisplayDialog(
            "Success!", 
            $"Created {barName}!\n\nSelect the Fill object to configure colors and gradient.", 
            "OK"
        );
        
        Debug.Log($"Created circular bar: {barName}");
    }
    
    private void CreateGlow(Transform parent)
    {
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(parent, false);
        
        RectTransform rect = glow.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barSize * 1.3f, barSize * 1.3f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = glow.AddComponent<Image>();
        image.sprite = CreateCircleSprite();
        Color glowColor = barType == BarType.Health ? new Color(0f, 0.8f, 0.4f, 0.3f) : new Color(0.3f, 0.5f, 1f, 0.3f);
        image.color = glowColor;
    }
    
    private void CreateBackground(Transform parent)
    {
        GameObject background = new GameObject("Background");
        background.transform.SetParent(parent, false);
        
        RectTransform rect = background.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barSize, barSize);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = background.AddComponent<Image>();
        image.sprite = CreateCircleSprite();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    }
    
    private GameObject CreateFill(Transform parent)
    {
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(parent, false);
        
        RectTransform rect = fill.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barSize * 0.9f, barSize * 0.9f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = fill.AddComponent<Image>();
        image.sprite = CreateCircleSprite();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = (int)fillOrigin;
        image.fillClockwise = true;
        image.fillAmount = 1f;
        
        Color fillColor = barType == BarType.Health ? new Color(0f, 0.8f, 0.4f) : new Color(0.3f, 0.5f, 1f);
        image.color = fillColor;
        
        // Add CircularHealthBar component
        CircularHealthBar circularBar = fill.AddComponent<CircularHealthBar>();
        // Component will auto-configure itself
        
        return fill;
    }
    
    private void CreateBorder(Transform parent)
    {
        GameObject border = new GameObject("Border");
        border.transform.SetParent(parent, false);
        
        RectTransform rect = border.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barSize, barSize);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = border.AddComponent<Image>();
        image.sprite = CreateCircleSprite();
        image.color = Color.white;
        
        // Add outline component for ring effect
        Outline outline = border.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
    }
    
    private void CreateText(Transform parent)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(barSize * 0.8f, barSize * 0.8f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Text text = textObj.AddComponent<Text>();
        text.text = barType == BarType.Health ? "100/100" : "10/10";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = Mathf.RoundToInt(barSize * 0.15f);
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        // Add outline for readability
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, 1);
    }
    
    private Sprite CreateCircleSprite()
    {
        // Try to find a circle sprite in resources
        Sprite circle = Resources.Load<Sprite>("UI/Circle");
        
        if (circle == null)
        {
            // Use Unity's built-in circle sprite
            circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }
        
        return circle;
    }
}

