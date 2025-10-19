using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Unity Editor tool to quickly create card prefabs.
/// Menu: Tools > Combat UI > Create Card Prefab
/// </summary>
public class CardPrefabCreator : EditorWindow
{
    private string cardName = "CardPrefab";
    private Vector2 cardSize = new Vector2(120, 180);
    private bool useCardDataFormat = true;
    
    [MenuItem("Tools/Combat UI/Create Card Prefab")]
    public static void ShowWindow()
    {
        var window = GetWindow<CardPrefabCreator>("Card Prefab Creator");
        window.minSize = new Vector2(400, 400);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Card Prefab Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Creates a complete card prefab with all necessary components.\n" +
            "Includes visual elements, scripts, and is ready for pooling.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Configuration
        cardName = EditorGUILayout.TextField("Prefab Name:", cardName);
        cardSize = EditorGUILayout.Vector2Field("Card Size:", cardSize);
        useCardDataFormat = EditorGUILayout.Toggle("Use CardData Format:", useCardDataFormat);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            useCardDataFormat ?
            "Will use CardData (ScriptableObject) - simpler system" :
            "Will use Card class - full featured system",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Preview
        EditorGUILayout.LabelField("Will Create:", EditorStyles.boldLabel);
        string preview = GeneratePreview();
        EditorGUILayout.TextArea(preview, GUILayout.Height(200));
        
        EditorGUILayout.Space();
        
        // Create button
        if (GUILayout.Button("Create Card Prefab", GUILayout.Height(40)))
        {
            CreateCardPrefab();
        }
    }
    
    private string GeneratePreview()
    {
        string preview = $"{cardName}\n";
        preview += $"├── Background (Image - colored by card type)\n";
        preview += $"├── Border (Image - colored by element)\n";
        preview += $"├── RarityGlow (Image - for rare cards)\n";
        preview += $"├── CardName (Text - top)\n";
        preview += $"├── Cost (Text - top right corner)\n";
        preview += $"├── Type (Text - below name)\n";
        preview += $"├── Value (Text - center, large damage/block)\n";
        preview += $"└── Description (Text - bottom)\n\n";
        preview += $"Components:\n";
        preview += useCardDataFormat ? 
            $"- CardDataVisualizer\n" : 
            $"- CardVisualizer\n";
        preview += $"- CardHoverEffect\n";
        preview += $"- Button\n";
        preview += $"- CanvasGroup\n\n";
        preview += $"Size: {cardSize.x}x{cardSize.y}\n";
        preview += $"Ready for: Object pooling, animations, runtime use";
        
        return preview;
    }
    
    private void CreateCardPrefab()
    {
        // Create root GameObject
        GameObject card = new GameObject(cardName);
        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = cardSize;
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Add components
        card.AddComponent<CanvasGroup>(); // For fading
        
        // Create visual hierarchy
        CreateBackground(card.transform);
        CreateBorder(card.transform);
        CreateRarityGlow(card.transform);
        CreateCardName(card.transform);
        CreateCost(card.transform);
        CreateType(card.transform);
        CreateValue(card.transform);
        CreateDescription(card.transform);
        
        // Add interaction components
        Button button = card.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        
        CardHoverEffect hover = card.AddComponent<CardHoverEffect>();
        
        // Add visualizer
        if (useCardDataFormat)
        {
            card.AddComponent<CardDataVisualizer>();
        }
        else
        {
            card.AddComponent<CardVisualizer>();
        }
        
        // Save as prefab
        string prefabPath = $"Assets/Prefab/{cardName}.prefab";
        
        // Ensure directory exists
        if (!System.IO.Directory.Exists("Assets/Prefab"))
        {
            System.IO.Directory.CreateDirectory("Assets/Prefab");
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, prefabPath);
        DestroyImmediate(card);
        
        // Select the prefab
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        
        EditorUtility.DisplayDialog(
            "Success!",
            $"Created card prefab: {cardName}\n\n" +
            $"Location: {prefabPath}\n\n" +
            "The prefab is now selected.\n" +
            "Assign it to CardRuntimeManager's Card Prefab field!",
            "OK"
        );
        
        Debug.Log($"<color=green>Created card prefab:</color> {prefabPath}");
    }
    
    private void CreateBackground(Transform parent)
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(parent, false);
        
        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image image = bg.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
    }
    
    private void CreateBorder(Transform parent)
    {
        GameObject border = new GameObject("Border");
        border.transform.SetParent(parent, false);
        
        RectTransform rect = border.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image image = border.AddComponent<Image>();
        image.color = Color.white;
        
        Outline outline = border.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
    }
    
    private void CreateRarityGlow(Transform parent)
    {
        GameObject glow = new GameObject("RarityGlow");
        glow.transform.SetParent(parent, false);
        
        RectTransform rect = glow.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-10, -10);
        rect.offsetMax = new Vector2(10, 10);
        
        Image image = glow.AddComponent<Image>();
        image.color = new Color(1f, 0.8f, 0f, 0.2f);
        image.enabled = false; // Hidden by default, shown for rare cards
    }
    
    private void CreateCardName(Transform parent)
    {
        GameObject nameObj = new GameObject("CardName");
        nameObj.transform.SetParent(parent, false);
        
        RectTransform rect = nameObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -10);
        rect.sizeDelta = new Vector2(-20, 30);
        
        Text text = nameObj.AddComponent<Text>();
        text.text = "Card Name";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        
        AddTextOutline(nameObj);
    }
    
    private void CreateCost(Transform parent)
    {
        GameObject costObj = new GameObject("Cost");
        costObj.transform.SetParent(parent, false);
        
        RectTransform rect = costObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-5, -5);
        rect.sizeDelta = new Vector2(30, 30);
        
        Text text = costObj.AddComponent<Text>();
        text.text = "1";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = Color.cyan;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        
        AddTextOutline(costObj);
    }
    
    private void CreateType(Transform parent)
    {
        GameObject typeObj = new GameObject("Type");
        typeObj.transform.SetParent(parent, false);
        
        RectTransform rect = typeObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -45);
        rect.sizeDelta = new Vector2(-20, 20);
        
        Text text = typeObj.AddComponent<Text>();
        text.text = "Attack";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 12;
        text.color = Color.yellow;
        text.alignment = TextAnchor.MiddleCenter;
        
        AddTextOutline(typeObj);
    }
    
    private void CreateValue(Transform parent)
    {
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(parent, false);
        
        RectTransform rect = valueObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(60, 60);
        
        Text text = valueObj.AddComponent<Text>();
        text.text = "10";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.color = new Color(1f, 0.3f, 0.3f);
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        
        AddTextOutline(valueObj);
    }
    
    private void CreateDescription(Transform parent)
    {
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(parent, false);
        
        RectTransform rect = descObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 10);
        rect.sizeDelta = new Vector2(-20, 50);
        
        Text text = descObj.AddComponent<Text>();
        text.text = "Card description text goes here";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 10;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        
        AddTextOutline(descObj);
    }
    
    private void AddTextOutline(GameObject textObj)
    {
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, 1);
    }
}

