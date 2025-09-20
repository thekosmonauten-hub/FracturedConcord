using UnityEngine;
using TMPro;
using PassiveTree;

/// <summary>
/// Helper script to automatically set up PassiveTreeUI components
/// Creates UI elements and connects them to PassiveTreeManager
/// </summary>
public class PassiveTreeUISetupHelper : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createUIElements = true;
    [SerializeField] private bool connectToManager = true;
    
    [Header("UI Settings")]
    [SerializeField] private Vector2 uiPosition = new Vector2(10, 10);
    [SerializeField] private Vector2 uiSize = new Vector2(300, 200);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Color textColor = Color.white;
    
    [Header("References")]
    [SerializeField] private PassiveTreeUI passiveTreeUI;
    [SerializeField] private PassiveTreeManager treeManager;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPassiveTreeUI();
        }
    }
    
    /// <summary>
    /// Set up the PassiveTreeUI system
    /// </summary>
    [ContextMenu("Setup PassiveTreeUI")]
    public void SetupPassiveTreeUI()
    {
        Debug.Log("[PassiveTreeUISetupHelper] Setting up PassiveTreeUI...");
        
        // Find or create PassiveTreeUI
        if (passiveTreeUI == null)
        {
            passiveTreeUI = FindFirstObjectByType<PassiveTreeUI>();
        }
        
        if (passiveTreeUI == null && createUIElements)
        {
            passiveTreeUI = CreatePassiveTreeUI();
        }
        
        // Find PassiveTreeManager
        if (treeManager == null)
        {
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
        }
        
        // Connect them
        if (connectToManager && passiveTreeUI != null && treeManager != null)
        {
            treeManager.SetUIManager(passiveTreeUI);
            Debug.Log("[PassiveTreeUISetupHelper] Connected PassiveTreeUI to PassiveTreeManager");
        }
        
        Debug.Log($"[PassiveTreeUISetupHelper] Setup complete. UI: {passiveTreeUI != null}, Manager: {treeManager != null}");
    }
    
    /// <summary>
    /// Create a simple PassiveTreeUI with basic UI elements
    /// </summary>
    private PassiveTreeUI CreatePassiveTreeUI()
    {
        Debug.Log("[PassiveTreeUISetupHelper] Creating PassiveTreeUI...");
        
        // Create main UI GameObject
        GameObject uiObject = new GameObject("PassiveTreeUI");
        uiObject.transform.SetParent(transform);
        
        // Add Canvas if needed
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        uiObject.transform.SetParent(canvas.transform);
        
        // Add RectTransform
        RectTransform rectTransform = uiObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = uiPosition;
        rectTransform.sizeDelta = uiSize;
        
        // Add background image
        UnityEngine.UI.Image background = uiObject.AddComponent<UnityEngine.UI.Image>();
        background.color = backgroundColor;
        
        // Add PassiveTreeUI component
        PassiveTreeUI passiveTreeUI = uiObject.AddComponent<PassiveTreeUI>();
        
        // Create text elements
        CreateTextElement(uiObject, "CellInfoText", "Select a cell to view information", 16, new Vector2(0, 0), new Vector2(1, 0.3f));
        CreateTextElement(uiObject, "CellNameText", "Cell Name", 14, new Vector2(0, 0.3f), new Vector2(1, 0.2f));
        CreateTextElement(uiObject, "CellDescriptionText", "Cell Description", 12, new Vector2(0, 0.5f), new Vector2(1, 0.3f));
        CreateTextElement(uiObject, "CellTypeText", "Cell Type", 12, new Vector2(0, 0.8f), new Vector2(0.5f, 0.2f));
        CreateTextElement(uiObject, "CellCostText", "Cell Cost", 12, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.2f));
        
        Debug.Log("[PassiveTreeUISetupHelper] Created PassiveTreeUI with UI elements");
        return passiveTreeUI;
    }
    
    /// <summary>
    /// Create a text element for the UI
    /// </summary>
    private void CreateTextElement(GameObject parent, string name, string defaultText, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent.transform);
        
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = defaultText;
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.TopLeft;
        
        // Add some padding
        rectTransform.offsetMin = new Vector2(10, 5);
        rectTransform.offsetMax = new Vector2(-10, -5);
    }
    
    /// <summary>
    /// Quick fix: Just disable the warning by setting the UI manager to null
    /// </summary>
    [ContextMenu("Disable UI Warning (Quick Fix)")]
    public void DisableUIWarning()
    {
        if (treeManager == null)
        {
            treeManager = FindFirstObjectByType<PassiveTreeManager>();
        }
        
        if (treeManager != null)
        {
            // Set a dummy UI manager that does nothing
            treeManager.SetUIManager(null);
            Debug.Log("[PassiveTreeUISetupHelper] Disabled UI warning - PassiveTreeManager will not look for UI");
        }
    }
}

