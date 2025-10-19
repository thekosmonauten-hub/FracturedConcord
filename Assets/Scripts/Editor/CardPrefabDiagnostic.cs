using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Diagnostic tool to check card prefab setup for card art display
/// </summary>
public class CardPrefabDiagnostic : EditorWindow
{
    private GameObject cardPrefab;

    [MenuItem("Dexiled/Debug/Card Prefab Diagnostic")]
    public static void ShowWindow()
    {
        GetWindow<CardPrefabDiagnostic>("Card Prefab Diagnostic");
    }

    private void OnGUI()
    {
        GUILayout.Label("Card Prefab Art Diagnostic", EditorStyles.boldLabel);
        GUILayout.Space(10);

        cardPrefab = (GameObject)EditorGUILayout.ObjectField("Card Prefab", cardPrefab, typeof(GameObject), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Analyze Prefab"))
        {
            if (cardPrefab != null)
            {
                AnalyzePrefab();
            }
            else
            {
                Debug.LogError("Please assign a card prefab first!");
            }
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Find Combat Card Prefab"))
        {
            FindCombatCardPrefab();
        }
    }

    private void AnalyzePrefab()
    {
        Debug.Log($"<color=cyan>===== ANALYZING CARD PREFAB: {cardPrefab.name} =====</color>");

        // Check for CardVisualizer
        CardVisualizer visualizer = cardPrefab.GetComponent<CardVisualizer>();
        if (visualizer != null)
        {
            Debug.Log($"<color=green>✓ CardVisualizer component found!</color>");
        }
        else
        {
            Debug.LogError($"<color=red>✗ CardVisualizer component MISSING! Add it to the prefab root.</color>");
        }

        // Check for all Image components
        Image[] allImages = cardPrefab.GetComponentsInChildren<Image>(true);
        Debug.Log($"\n<color=yellow>Found {allImages.Length} Image components:</color>");

        foreach (Image img in allImages)
        {
            Debug.Log($"  - {img.gameObject.name} (GameObject path: {GetGameObjectPath(img.transform)})");
        }

        // Check for specific card art names
        Transform cardImageTransform = cardPrefab.transform.Find("Card Image");
        if (cardImageTransform != null)
        {
            Image cardImage = cardImageTransform.GetComponent<Image>();
            if (cardImage != null)
            {
                Debug.Log($"<color=green>✓ 'Card Image' GameObject found with Image component!</color>");
                Debug.Log($"  Current sprite: {(cardImage.sprite != null ? cardImage.sprite.name : "None")}");
            }
            else
            {
                Debug.LogWarning($"<color=orange>⚠ 'Card Image' GameObject exists but has no Image component!</color>");
            }
        }
        else
        {
            Debug.LogWarning($"<color=orange>⚠ 'Card Image' GameObject not found in prefab!</color>");
            Debug.Log($"  CardVisualizer will look for: Card Image, CardImage, CardArt, Art, Image, Artwork");
        }

        // List all children
        Debug.Log($"\n<color=yellow>Prefab hierarchy:</color>");
        LogHierarchy(cardPrefab.transform, 0);

        Debug.Log($"\n<color=cyan>===== ANALYSIS COMPLETE =====</color>");
    }

    private void LogHierarchy(Transform parent, int indent)
    {
        string indentStr = new string(' ', indent * 2);
        Debug.Log($"{indentStr}- {parent.name}");

        foreach (Transform child in parent)
        {
            LogHierarchy(child, indent + 1);
        }
    }

    private string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    private void FindCombatCardPrefab()
    {
        // Try to find AnimatedCombatUI in scene
        AnimatedCombatUI animatedUI = FindFirstObjectByType<AnimatedCombatUI>();
        if (animatedUI != null)
        {
            SerializedObject so = new SerializedObject(animatedUI);
            SerializedProperty prefabProperty = so.FindProperty("cardPrefab");

            if (prefabProperty != null && prefabProperty.objectReferenceValue != null)
            {
                cardPrefab = prefabProperty.objectReferenceValue as GameObject;
                Debug.Log($"<color=green>✓ Found card prefab from AnimatedCombatUI: {cardPrefab.name}</color>");
                Repaint();
            }
            else
            {
                Debug.LogWarning("AnimatedCombatUI found but cardPrefab is not assigned!");
            }
        }
        else
        {
            Debug.LogWarning("AnimatedCombatUI not found in scene. Make sure Combat Scene is open.");
        }
    }
}
#endif




