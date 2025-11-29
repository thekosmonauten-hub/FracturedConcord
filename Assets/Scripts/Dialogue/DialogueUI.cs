using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying dialogue. Uses Unity's legacy UI system (uGUI).
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    
    [Header("Dialogue Display")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image speakerPortrait;
    
    [Header("Paragraph GameObjects (Preferred)")]
    [Tooltip("Paragraph GameObjects (Paragraph1, Paragraph2, etc.). If provided, these will be used instead of dialogueText.")]
    [SerializeField] private TextMeshProUGUI[] paragraphTexts = new TextMeshProUGUI[10];
    
    [Header("Legacy Single Text Field")]
    [Tooltip("Legacy single dialogue text field. Only used if paragraphTexts array is empty.")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    [Header("Choices")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    
    [Header("Navigation")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backButton;
    
    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f; // Seconds per character
    [SerializeField] private bool useTypewriterEffect = false; // Disabled for now - text appears instantly
    [SerializeField] private bool splitByParagraphs = true;
    [Tooltip("Character(s) used to split dialogue into paragraphs. Default: double newline (\\n\\n)")]
    [SerializeField] private string paragraphDelimiter = "\n\n";
    
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    private Coroutine typewriterCoroutineOnManager; // Track coroutine started on DialogueManager
    private List<Button> currentChoiceButtons = new List<Button>();
    
    // Access current node and paragraphs from DialogueManager (singleton, persists across instances)
    private DialogueNode currentNode => DialogueManager.Instance != null ? DialogueManager.Instance.CurrentNode : null;
    private List<string> currentParagraphs => DialogueManager.Instance != null ? DialogueManager.Instance.currentParagraphs : new List<string>();
    private int currentParagraphIndex
    {
        get => DialogueManager.Instance != null ? DialogueManager.Instance.currentParagraphIndex : 0;
        set { if (DialogueManager.Instance != null) DialogueManager.Instance.currentParagraphIndex = value; }
    }
    
    // Store the resolved scene instance of dialoguePanel (not prefab asset)
    private GameObject resolvedDialoguePanel;
    
    // Store resolved scene instances of paragraph texts (not prefab assets)
    private TextMeshProUGUI[] resolvedParagraphTexts;
    
    private void Awake()
    {
        InitializeUI();
        EnsureDialoguePanelInScene();
        
        // Ensure buttons are properly set up (in case they're on a prefab instance)
        // Always re-add listeners to ensure they work even if the component is on a prefab
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
            Debug.Log("[DialogueUI] Initialized Continue button listener in Awake");
        }
        else
        {
            Debug.LogWarning("[DialogueUI] Continue button is null! Cannot add listener.");
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
            Debug.Log("[DialogueUI] Initialized Close button listener in Awake");
        }
        else
        {
            Debug.LogWarning("[DialogueUI] Close button is null! Cannot add listener.");
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
            Debug.Log("[DialogueUI] Initialized Back button listener in Awake");
        }
        else
        {
            Debug.LogWarning("[DialogueUI] Back button is null! Cannot add listener.");
        }
    }
    
    private void Start()
    {
        // Only deactivate panel if no dialogue is currently active
        // This prevents deactivating it if NPCInteractable already activated it
        if (dialoguePanel != null && (DialogueManager.Instance == null || DialogueManager.Instance.CurrentNode == null))
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Ensure DialoguePanel is in the scene (not in DontDestroyOnLoad)
    /// This prevents it from disappearing from Scene view
    /// Also ensures it has its own Canvas (DialogueCanvas) if needed
    /// </summary>
    private void EnsureDialoguePanelInScene()
    {
        if (dialoguePanel == null) return;
        
        // Check if DialoguePanel or its Canvas is in DontDestroyOnLoad
        Canvas parentCanvas = dialoguePanel.GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.gameObject.scene.name == "DontDestroyOnLoad")
        {
            Debug.LogWarning($"[DialogueUI] DialoguePanel's Canvas '{parentCanvas.name}' is in DontDestroyOnLoad. This will hide it from Scene view.");
            Debug.LogWarning($"[DialogueUI] SOLUTION: Create a dedicated 'DialogueCanvas' in the scene and move DialoguePanel under it. Do NOT make DialogueCanvas persistent.");
        }
        
        // Check if DialoguePanel itself is in DontDestroyOnLoad
        if (dialoguePanel.scene.name == "DontDestroyOnLoad")
        {
            Debug.LogWarning($"[DialogueUI] DialoguePanel '{dialoguePanel.name}' is in DontDestroyOnLoad. This will hide it from Scene view.");
            Debug.LogWarning($"[DialogueUI] SOLUTION: Ensure DialoguePanel exists in the scene hierarchy under a non-persistent Canvas (e.g., DialogueCanvas).");
        }
        
        // If DialoguePanel exists but is not under a Canvas, try to find or create DialogueCanvas
        if (dialoguePanel != null && dialoguePanel.scene.name != "DontDestroyOnLoad")
        {
            Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.gameObject.scene.name == "DontDestroyOnLoad")
            {
                // Try to find DialogueCanvas in the scene
                GameObject dialogueCanvasObj = GameObject.Find("DialogueCanvas");
                if (dialogueCanvasObj == null)
                {
                    Debug.LogWarning($"[DialogueUI] DialoguePanel is not under a Canvas. Please create a 'DialogueCanvas' in the scene and move DialoguePanel under it.");
                }
                else
                {
                    Canvas dialogueCanvas = dialogueCanvasObj.GetComponent<Canvas>();
                    if (dialogueCanvas == null)
                    {
                        dialogueCanvas = dialogueCanvasObj.AddComponent<Canvas>();
                    }
                    
                    // Move DialoguePanel to DialogueCanvas if it's not already there
                    if (dialoguePanel.transform.parent != dialogueCanvasObj.transform)
                    {
                        dialoguePanel.transform.SetParent(dialogueCanvasObj.transform, false);
                        Debug.Log($"[DialogueUI] Moved DialoguePanel to DialogueCanvas.");
                    }
                    
                    // Ensure DialogueCanvas is properly configured
                    ConfigureDialogueCanvas(dialogueCanvas);
                }
            }
            else if (canvas != null)
            {
                // Ensure the Canvas is properly configured
                ConfigureDialogueCanvas(canvas);
            }
        }
    }
    
    /// <summary>
    /// Configure DialogueCanvas with proper settings
    /// </summary>
    private void ConfigureDialogueCanvas(Canvas canvas)
    {
        if (canvas == null) return;
        
        // Ensure it's NOT persistent
        if (canvas.gameObject.scene.name == "DontDestroyOnLoad")
        {
            Debug.LogError($"[DialogueUI] DialogueCanvas '{canvas.name}' is in DontDestroyOnLoad! This will hide it from Scene view. Please remove any PersistentCanvas component or DontDestroyOnLoad calls.");
            return;
        }
        
        // Configure Canvas settings
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // High enough to appear above game UI, but not too high
        
        // Ensure CanvasScaler exists
        if (canvas.GetComponent<UnityEngine.UI.CanvasScaler>() == null)
        {
            var scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        // Ensure GraphicRaycaster exists
        if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // PersistentCanvas removal code removed - PassiveTree system is legacy
        // Note: PersistentCanvas was part of PassiveTree system which has been removed
        
        Debug.Log($"[DialogueUI] Configured DialogueCanvas '{canvas.name}' (Scene: {canvas.gameObject.scene.name}, SortingOrder: {canvas.sortingOrder})");
    }
    
    private void InitializeUI()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }
    
    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }
    
    public void DisplayNode(DialogueNode node)
    {
        if (node == null)
            return;
        
        // Clear all paragraph text immediately to prevent showing previous dialogue
        ClearAllParagraphText();
        
        // currentNode is now accessed via DialogueManager, no need to store locally
        
        // IMPORTANT: Activate DialoguePanel immediately (before coroutine)
        // This ensures it's visible even if Start() hasn't run yet or runs later
        if (dialoguePanel != null)
        {
            if (!dialoguePanel.activeSelf)
            {
                dialoguePanel.SetActive(true);
                Debug.Log("[DialogueUI] Activated DialoguePanel immediately in DisplayNode()");
            }
            
            // CRITICAL: Set CanvasGroup alpha to 1 to make it visible
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 1f;
                panelCanvasGroup.blocksRaycasts = true;
                panelCanvasGroup.interactable = true;
                Debug.Log("[DialogueUI] Set CanvasGroup alpha to 1, blocksRaycasts=true, interactable=true");
            }
            else
            {
                // Try to get CanvasGroup from dialoguePanel
                CanvasGroup cg = dialoguePanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.blocksRaycasts = true;
                    cg.interactable = true;
                    Debug.Log("[DialogueUI] Found and configured CanvasGroup on DialoguePanel");
                }
                else
                {
                    Debug.LogWarning("[DialogueUI] No CanvasGroup found on DialoguePanel! Panel may be invisible.");
                }
            }
        }
        
        // Simple approach: activate both the panel and this GameObject
        GameObject targetPanel = dialoguePanel != null ? dialoguePanel : gameObject;
        
        // Ensure we're using the scene instance, not a prefab asset or DontDestroyOnLoad instance
        if (targetPanel != null)
        {
            // If dialoguePanel is a prefab asset (not in scene), try to find the scene instance
            // Prefab assets have an empty or null scene name
            if (string.IsNullOrEmpty(targetPanel.scene.name) || targetPanel.scene.name == "DontDestroyOnLoad")
            {
                string reason = string.IsNullOrEmpty(targetPanel.scene.name) ? "prefab asset" : "DontDestroyOnLoad";
                Debug.LogWarning($"[DialogueUI] dialoguePanel appears to be a {reason}, not a scene instance. Searching for scene instance...");
                
                // Find ALL GameObjects with this name in the scene (excluding DontDestroyOnLoad)
                GameObject[] allPanels = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                GameObject sceneInstance = null;
                
                // Look for one that's actually under a Canvas in an active scene
                foreach (GameObject obj in allPanels)
                {
                    if (obj.name == targetPanel.name && !string.IsNullOrEmpty(obj.scene.name) && obj.scene.name != "DontDestroyOnLoad")
                    {
                        // Check if this instance is under a Canvas
                        Canvas canvas = obj.GetComponentInParent<Canvas>(true);
                        if (canvas != null && canvas.gameObject.scene.name != "DontDestroyOnLoad")
                        {
                            sceneInstance = obj;
                            Debug.Log($"[DialogueUI] Found scene instance under Canvas '{canvas.name}' in scene '{obj.scene.name}': {obj.name}");
                            break;
                        }
                    }
                }
                
                // If no Canvas-parented instance found, use any scene instance (excluding DontDestroyOnLoad)
                if (sceneInstance == null)
                {
                    foreach (GameObject obj in allPanels)
                    {
                        if (obj.name == targetPanel.name && obj.scene.name != "DontDestroyOnLoad" && obj.scene.isLoaded)
                        {
                            sceneInstance = obj;
                            Debug.LogWarning($"[DialogueUI] Found scene instance '{sceneInstance.name}' in scene '{obj.scene.name}' but it's not under a Canvas! This may cause issues.");
                            break;
                        }
                    }
                }
                
                if (sceneInstance != null)
                {
                    targetPanel = sceneInstance;
                }
                else
                {
                    Debug.LogError($"[DialogueUI] Could not find scene instance of '{targetPanel.name}' in any active scene. Please ensure DialoguePanel exists in the scene hierarchy under a Canvas.");
                    return;
                }
            }
            
            // Verify the targetPanel is actually under a Canvas
            Canvas parentCanvas = targetPanel.GetComponentInParent<Canvas>(true);
            if (parentCanvas == null)
            {
                Debug.LogError($"[DialogueUI] DialoguePanel '{targetPanel.name}' is not under a Canvas! Please parent it under TownCanvas → Menus → DialoguePanel in the scene hierarchy.");
                return;
            }
            
            // Store the resolved scene instance for use in coroutine
            resolvedDialoguePanel = targetPanel;
            
            // Activate entire parent chain from root to targetPanel
            ActivateParentChain(targetPanel);
            
            // Ensure the panel itself is active
            if (!targetPanel.activeSelf)
            {
                targetPanel.SetActive(true);
                Debug.Log($"[DialogueUI] Activated resolved DialoguePanel '{targetPanel.name}' in DisplayNode()");
            }
            
            // CRITICAL: Set CanvasGroup alpha to 1 to make it visible
            CanvasGroup cg = targetPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
                Debug.Log($"[DialogueUI] Set CanvasGroup alpha to 1 on resolved DialoguePanel '{targetPanel.name}'");
            }
        }
        else
        {
            // If targetPanel is null, use gameObject
            resolvedDialoguePanel = gameObject;
        }
        
        // Resolve paragraph GameObjects from prefab to scene instances
        ResolveParagraphGameObjects();
        
        // Also ensure this GameObject (with DialogueUI component) is active
        if (resolvedDialoguePanel != gameObject)
        {
            ActivateParentChain(gameObject);
        }
        
        // Always use DialogueManager to start coroutine (it's always active)
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartCoroutine(DisplayNodeSimple(node));
        }
        else
        {
            Debug.LogError("[DialogueUI] DialogueManager.Instance is null! Cannot display dialogue.");
        }
    }
    
    /// <summary>
    /// Resolves paragraph GameObjects from prefab references to scene instances
    /// </summary>
    private void ResolveParagraphGameObjects()
    {
        Debug.Log($"[DialogueUI] ResolveParagraphGameObjects: Starting resolution...");
        Debug.Log($"[DialogueUI] paragraphTexts is null: {paragraphTexts == null}, Length: {(paragraphTexts != null ? paragraphTexts.Length : 0)}");
        
        // If paragraphTexts is not assigned, try to auto-find paragraph GameObjects
        if (paragraphTexts == null || paragraphTexts.Length == 0 || AllParagraphTextsAreNull())
        {
            Debug.LogWarning("[DialogueUI] ResolveParagraphGameObjects: paragraphTexts array is null, empty, or all null! Attempting to auto-find paragraph GameObjects...");
            AutoFindParagraphGameObjects();
        }
        
        if (paragraphTexts == null || paragraphTexts.Length == 0)
        {
            Debug.LogWarning("[DialogueUI] ResolveParagraphGameObjects: paragraphTexts array is still null or empty after auto-find!");
            resolvedParagraphTexts = paragraphTexts;
            return;
        }
        
        resolvedParagraphTexts = new TextMeshProUGUI[paragraphTexts.Length];
        
        // Use the resolved dialogue panel to search for paragraph GameObjects
        GameObject searchRoot = resolvedDialoguePanel != null ? resolvedDialoguePanel : dialoguePanel;
        if (searchRoot == null)
        {
            searchRoot = gameObject;
        }
        
        Debug.Log($"[DialogueUI] ResolveParagraphGameObjects: Searching under '{searchRoot.name}' (scene: {searchRoot.scene.name})");
        
        // Also try searching all TextMeshProUGUI components in the scene as a fallback
        TextMeshProUGUI[] allTextComponents = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[DialogueUI] Found {allTextComponents.Length} TextMeshProUGUI components in scene");
        
        for (int i = 0; i < paragraphTexts.Length; i++)
        {
            if (paragraphTexts[i] == null)
            {
                Debug.LogWarning($"[DialogueUI] ResolveParagraphGameObjects: paragraphTexts[{i}] is null in Inspector!");
                
                // Try to find by expected name (Paragraph1, Paragraph2, etc.)
                string expectedName = $"Paragraph{i + 1}";
                Debug.Log($"[DialogueUI] Attempting to find '{expectedName}' by name...");
                
                // First try recursive search under dialogue panel
                Transform foundTransform = FindChildRecursive(searchRoot.transform, expectedName);
                if (foundTransform != null)
                {
                    TextMeshProUGUI found = foundTransform.GetComponent<TextMeshProUGUI>();
                    if (found != null)
                    {
                        resolvedParagraphTexts[i] = found;
                        Debug.Log($"[DialogueUI] Found '{expectedName}' via recursive search: {found.gameObject.name}");
                        continue;
                    }
                }
                
                // Fallback: search all TextMeshProUGUI components
                foreach (TextMeshProUGUI textComp in allTextComponents)
                {
                    if (textComp.gameObject.name == expectedName && !string.IsNullOrEmpty(textComp.gameObject.scene.name))
                    {
                        resolvedParagraphTexts[i] = textComp;
                        Debug.Log($"[DialogueUI] Found '{expectedName}' via scene search: {textComp.gameObject.name}");
                        break;
                    }
                }
                
                if (resolvedParagraphTexts[i] == null)
                {
                    Debug.LogWarning($"[DialogueUI] Could not find '{expectedName}' anywhere in scene!");
                }
                
                continue;
            }
            
            // Check if this is a prefab asset (not a scene instance)
            if (string.IsNullOrEmpty(paragraphTexts[i].gameObject.scene.name))
            {
                // This is a prefab asset, search for scene instance
                string paragraphName = paragraphTexts[i].gameObject.name;
                Debug.Log($"[DialogueUI] Resolving paragraph GameObject '{paragraphName}' from prefab to scene instance...");
                
                // Search recursively under the dialogue panel
                Transform foundTransform = FindChildRecursive(searchRoot.transform, paragraphName);
                if (foundTransform != null)
                {
                    TextMeshProUGUI found = foundTransform.GetComponent<TextMeshProUGUI>();
                    if (found != null && !string.IsNullOrEmpty(found.gameObject.scene.name))
                    {
                        resolvedParagraphTexts[i] = found;
                        Debug.Log($"[DialogueUI] Found scene instance via recursive search: {found.gameObject.name}");
                        continue;
                    }
                }
                
                // Fallback: search all TextMeshProUGUI components
                foreach (TextMeshProUGUI textComp in allTextComponents)
                {
                    if (textComp.gameObject.name == paragraphName && !string.IsNullOrEmpty(textComp.gameObject.scene.name))
                    {
                        resolvedParagraphTexts[i] = textComp;
                        Debug.Log($"[DialogueUI] Found scene instance via scene search: {textComp.gameObject.name}");
                        break;
                    }
                }
                
                if (resolvedParagraphTexts[i] == null)
                {
                    Debug.LogWarning($"[DialogueUI] Could not find scene instance of '{paragraphName}'. Make sure it exists under '{searchRoot.name}' in the scene hierarchy.");
                }
            }
            else
            {
                // Already a scene instance
                resolvedParagraphTexts[i] = paragraphTexts[i];
                Debug.Log($"[DialogueUI] paragraphTexts[{i}] is already a scene instance: {paragraphTexts[i].gameObject.name}");
            }
        }
        
        // Log final resolution results
        int resolvedCount = 0;
        for (int i = 0; i < resolvedParagraphTexts.Length; i++)
        {
            if (resolvedParagraphTexts[i] != null)
                resolvedCount++;
        }
        Debug.Log($"[DialogueUI] ResolveParagraphGameObjects: Resolved {resolvedCount}/{resolvedParagraphTexts.Length} paragraph GameObjects");
    }
    
    /// <summary>
    /// Check if all paragraphTexts are null
    /// </summary>
    private bool AllParagraphTextsAreNull()
    {
        if (paragraphTexts == null) return true;
        foreach (var text in paragraphTexts)
        {
            if (text != null) return false;
        }
        return true;
    }
    
    /// <summary>
    /// Auto-find paragraph GameObjects by searching for "Paragraph1", "Paragraph2", etc.
    /// </summary>
    private void AutoFindParagraphGameObjects()
    {
        GameObject searchRoot = resolvedDialoguePanel != null ? resolvedDialoguePanel : dialoguePanel;
        if (searchRoot == null)
        {
            searchRoot = gameObject;
        }
        
        Debug.Log($"[DialogueUI] AutoFindParagraphGameObjects: Searching under '{searchRoot.name}' for paragraph GameObjects...");
        
        // Search for Paragraph1, Paragraph2, etc. up to Paragraph10
        List<TextMeshProUGUI> foundParagraphs = new List<TextMeshProUGUI>();
        
        for (int i = 1; i <= 10; i++)
        {
            string paragraphName = $"Paragraph{i}";
            Transform found = FindChildRecursive(searchRoot.transform, paragraphName);
            
            if (found != null)
            {
                TextMeshProUGUI textComponent = found.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    foundParagraphs.Add(textComponent);
                    Debug.Log($"[DialogueUI] Found paragraph GameObject: '{paragraphName}'");
                }
                else
                {
                    Debug.LogWarning($"[DialogueUI] Found GameObject '{paragraphName}' but it doesn't have a TextMeshProUGUI component!");
                }
            }
        }
        
        if (foundParagraphs.Count > 0)
        {
            // Update the paragraphTexts array
            paragraphTexts = foundParagraphs.ToArray();
            Debug.Log($"[DialogueUI] AutoFindParagraphGameObjects: Found {foundParagraphs.Count} paragraph GameObjects!");
        }
        else
        {
            Debug.LogError($"[DialogueUI] AutoFindParagraphGameObjects: Could not find any paragraph GameObjects (Paragraph1, Paragraph2, etc.) under '{searchRoot.name}'!");
            Debug.LogError($"[DialogueUI] Please ensure your DialoguePanel has child GameObjects named 'Paragraph1', 'Paragraph2', etc. with TextMeshProUGUI components.");
        }
    }
    
    /// <summary>
    /// Helper method to recursively find a child by name
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent == null) return null;
        
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            
            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        
        return null;
    }
    
    /// <summary>
    /// Activates a GameObject and all its parents up to the root
    /// </summary>
    private void ActivateParentChain(GameObject obj)
    {
        if (obj == null) return;
        
        // Build parent chain from root to immediate parent
        List<Transform> parentChain = new List<Transform>();
        Transform current = obj.transform.parent;
        while (current != null)
        {
            parentChain.Insert(0, current); // Insert at front to maintain root-to-child order
            current = current.parent;
        }
        
        // Activate parents from root to immediate parent (must be done in order)
        foreach (Transform parent in parentChain)
        {
            if (!parent.gameObject.activeSelf)
            {
                parent.gameObject.SetActive(true);
            }
        }
        
        // Finally, activate the target GameObject itself
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
        }
    }
    
    /// <summary>
    /// Simple coroutine to display dialogue node. Always started from DialogueManager.
    /// </summary>
    private System.Collections.IEnumerator DisplayNodeSimple(DialogueNode node)
    {
        // Use the resolved scene instance, not the prefab asset reference
        GameObject targetPanel = resolvedDialoguePanel != null ? resolvedDialoguePanel : gameObject;
        
        if (targetPanel == null)
        {
            Debug.LogError("[DialogueUI] targetPanel is null! Cannot display dialogue.");
            yield break;
        }
        
        // Step 1: Find and activate Canvas FIRST (before checking panel)
        // IMPORTANT: Search in active scene first, not DontDestroyOnLoad
        Canvas canvas = null;
        
        // First try: Find Canvas in the same scene as targetPanel (but exclude DontDestroyOnLoad)
        if (targetPanel.scene.name != "DontDestroyOnLoad")
        {
            canvas = targetPanel.GetComponentInParent<Canvas>(true);
            if (canvas != null && canvas.gameObject.scene.name == "DontDestroyOnLoad")
            {
                // Canvas is in DontDestroyOnLoad, but panel is not - this is a problem
                Debug.LogWarning($"[DialogueUI] DialoguePanel '{targetPanel.name}' is in scene '{targetPanel.scene.name}' but Canvas '{canvas.name}' is in DontDestroyOnLoad. Searching for scene Canvas...");
                canvas = null;
            }
        }
        
        // If not found or in wrong scene, search in active scene
        if (canvas == null)
        {
            // Search all Canvas objects in the current active scene (excluding DontDestroyOnLoad)
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                if (c.gameObject.scene.name != "DontDestroyOnLoad" && c.gameObject.scene.isLoaded)
                {
                    canvas = c;
                    Debug.Log($"[DialogueUI] Found Canvas in active scene: {canvas.name}");
                    break;
                }
            }
        }
        
        // Manual walk up the hierarchy to find Canvas (if still not found)
        if (canvas == null)
        {
            Transform current = targetPanel.transform;
            while (current != null)
            {
                canvas = current.GetComponent<Canvas>();
                if (canvas != null && canvas.gameObject.scene.name != "DontDestroyOnLoad") break;
                current = current.parent;
            }
        }
        
        if (canvas != null)
        {
            // Activate Canvas and all parents
            ActivateParentChain(canvas.gameObject);
            Debug.Log($"[DialogueUI] Found Canvas: {canvas.name} (scene: {canvas.gameObject.scene.name}, activeSelf: {canvas.gameObject.activeSelf}, activeInHierarchy: {canvas.gameObject.activeInHierarchy})");
        }
        else
        {
            Debug.LogError("[DialogueUI] NO CANVAS FOUND IN ACTIVE SCENE! Cannot display dialogue. Please ensure DialoguePanel is under a Canvas in the active scene.");
            yield break;
        }
        
        // Step 2: Activate entire parent chain from Canvas down to panel
        ActivateParentChain(targetPanel);
        if (targetPanel != gameObject)
        {
            ActivateParentChain(gameObject);
        }
        
        // Step 3: Wait for activation to propagate
        yield return null;
        yield return null;
        yield return null;
        
        // Step 4: Final verification
        if (!targetPanel.activeInHierarchy)
        {
            // Log detailed debug info
            System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();
            debugInfo.AppendLine($"[DialogueUI] DialoguePanel still not active in hierarchy after activation!");
            debugInfo.AppendLine($"  Target Panel: {targetPanel.name}");
            debugInfo.AppendLine($"  activeSelf: {targetPanel.activeSelf}");
            debugInfo.AppendLine($"  activeInHierarchy: {targetPanel.activeInHierarchy}");
            
            // Walk up the parent chain and log each level
            Transform current = targetPanel.transform;
            int depth = 0;
            while (current != null && depth < 10)
            {
                debugInfo.AppendLine($"  [{depth}] {current.name} - activeSelf: {current.gameObject.activeSelf}, activeInHierarchy: {current.gameObject.activeInHierarchy}");
                current = current.parent;
                depth++;
            }
            
            Debug.LogError(debugInfo.ToString());
            Debug.LogError($"[DialogueUI] Please check the Unity Hierarchy. DialoguePanel must be a child of an active Canvas.");
            yield break;
        }
        
        Debug.Log($"[DialogueUI] DialoguePanel is active and ready! (activeSelf: {targetPanel.activeSelf}, activeInHierarchy: {targetPanel.activeInHierarchy})");
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;
        }
        
        // Reset typing state when starting new dialogue
        isTyping = false;
        if (typewriterCoroutine != null)
        {
            if (gameObject.activeInHierarchy)
            {
                StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = null;
        }
        if (typewriterCoroutineOnManager != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StopCoroutine(typewriterCoroutineOnManager);
            typewriterCoroutineOnManager = null;
        }
        
        // Update speaker info - ensure it's always updated when displaying a node
        if (speakerNameText != null)
        {
            string newSpeakerName = node.speakerName ?? "Unknown";
            speakerNameText.text = newSpeakerName;
            
            // Ensure speaker name GameObject is active and visible
            if (!speakerNameText.gameObject.activeSelf)
            {
                speakerNameText.gameObject.SetActive(true);
                ActivateParentChain(speakerNameText.gameObject);
            }
            
            // Force text update
            speakerNameText.ForceMeshUpdate();
            
            Debug.Log($"[DialogueUI] Updated speaker name to: '{newSpeakerName}' for node '{node.nodeId}'");
        }
        else
        {
            Debug.LogWarning("[DialogueUI] speakerNameText is null! Cannot display speaker name.");
        }
        
        if (speakerPortrait != null)
        {
            // Use node's portrait if available, otherwise fall back to dialogue's default portrait
            Sprite portraitToUse = null;
            
            if (node.speakerPortrait != null)
            {
                // Node has its own portrait - use it
                portraitToUse = node.speakerPortrait;
            }
            else if (DialogueManager.Instance != null && DialogueManager.Instance.CurrentDialogue != null)
            {
                // Fall back to dialogue's default portrait
                portraitToUse = DialogueManager.Instance.CurrentDialogue.defaultSpeakerPortrait;
            }
            
            speakerPortrait.sprite = portraitToUse;
            speakerPortrait.enabled = portraitToUse != null;
            
            // Ensure speaker portrait GameObject is active
            if (speakerPortrait.sprite != null && !speakerPortrait.gameObject.activeSelf)
            {
                speakerPortrait.gameObject.SetActive(true);
                ActivateParentChain(speakerPortrait.gameObject);
            }
        }
        
        // Use paragraphs list if available, otherwise fall back to dialogueText
        if (node.paragraphs != null && node.paragraphs.Count > 0)
        {
            Debug.Log($"[DialogueUI] DisplayNode: Node '{node.nodeId}' has {node.paragraphs.Count} paragraphs");
            for (int i = 0; i < node.paragraphs.Count; i++)
            {
                Debug.Log($"[DialogueUI] Paragraph {i + 1}: '{node.paragraphs[i].Substring(0, Mathf.Min(50, node.paragraphs[i].Length))}...'");
            }
            
            // CRITICAL: Clear all paragraph text FIRST, before loading new text
            // This prevents flickering when the same paragraph index is reused in different nodes
            ClearAllParagraphText();
            
            // Use new paragraphs system (PREFERRED)
            DialogueManager.Instance.currentParagraphs = new List<string>(node.paragraphs);
            DialogueManager.Instance.currentParagraphIndex = 0;
            
            Debug.Log($"[DialogueUI] DisplayNode: Set currentParagraphs to {DialogueManager.Instance.currentParagraphs.Count} paragraphs, index = 0");
            
            // Ensure paragraph GameObjects are resolved before preloading
            ResolveParagraphGameObjects();
            
            // Pre-load all paragraphs into their GameObjects (text is already cleared above)
            PreloadParagraphs(node.paragraphs);
            
            // Ensure continue button is ready before showing first paragraph
            // (ShowCurrentParagraph will set it properly based on whether there are more paragraphs)
            // Always show continue button when displaying a new node (unless it's an end node)
            if (continueButton != null && node.paragraphs.Count > 0)
            {
                // Show continue button for any non-end node (even if it has choices)
                bool shouldShow = !node.IsEndNode;
                continueButton.gameObject.SetActive(shouldShow);
                continueButton.interactable = shouldShow;
                Debug.Log($"[DialogueUI] DisplayNode: Continue button set to active={shouldShow}, interactable={shouldShow}");
            }
            else if (continueButton == null)
            {
                Debug.LogError("[DialogueUI] DisplayNode: Continue button is null! Buttons won't work.");
            }
            
            ShowCurrentParagraph();
        }
        else if (!string.IsNullOrEmpty(node.dialogueText))
        {
            // Fallback to legacy dialogueText field (for backward compatibility)
            if (splitByParagraphs)
            {
                SplitIntoParagraphs(node.dialogueText);
                DialogueManager.Instance.currentParagraphIndex = 0;
                
                // Pre-load all paragraphs into their GameObjects
                PreloadParagraphs(DialogueManager.Instance.currentParagraphs);
                
                ShowCurrentParagraph();
            }
            else
            {
                // Display all dialogue text at once
                if (dialogueText != null)
                {
                    if (useTypewriterEffect)
                    {
                        StartTypewriter(node.dialogueText);
                    }
                    else
                    {
                        dialogueText.text = node.dialogueText ?? "";
                        isTyping = false;
                    }
                }
                ShowChoicesOrContinue(node);
            }
        }
        else
        {
            Debug.LogWarning($"[DialogueUI] Node '{node.nodeId}' has no paragraphs and no dialogueText!");
        }
        
        // Show/hide close button (always available)
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(true);
        }
        
        // Show/hide back button
        if (backButton != null)
        {
            backButton.gameObject.SetActive(DialogueManager.Instance != null && 
                                           DialogueManager.Instance.DialogueHistory.Count > 0);
        }
    }
    
    private void StartTypewriter(string text)
    {
        Debug.Log($"[DialogueUI] StartTypewriter called. Text length: {text?.Length ?? 0}, isTyping: {isTyping}, gameObject.activeInHierarchy: {gameObject.activeInHierarchy}");
        
        // Stop any existing typewriter coroutines
        if (typewriterCoroutine != null)
        {
            Debug.Log("[DialogueUI] Stopping existing typewriterCoroutine on gameObject");
            if (gameObject.activeInHierarchy)
            {
                StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = null;
        }
        
        if (typewriterCoroutineOnManager != null && DialogueManager.Instance != null)
        {
            Debug.Log("[DialogueUI] Stopping existing typewriterCoroutineOnManager");
            DialogueManager.Instance.StopCoroutine(typewriterCoroutineOnManager);
            typewriterCoroutineOnManager = null;
        }
        
        // Prevent starting if already typing (safety check)
        if (isTyping)
        {
            Debug.LogWarning("[DialogueUI] Typewriter already running! Skipping duplicate start.");
            return;
        }
        
        isTyping = true;
        
        // Start coroutine on this GameObject if active, otherwise use DialogueManager
        if (gameObject.activeInHierarchy)
        {
            Debug.Log("[DialogueUI] Starting typewriter coroutine on gameObject");
            typewriterCoroutine = StartCoroutine(TypewriterCoroutine(text));
        }
        else if (DialogueManager.Instance != null)
        {
            Debug.Log("[DialogueUI] Starting typewriter coroutine on DialogueManager.Instance");
            typewriterCoroutineOnManager = DialogueManager.Instance.StartCoroutine(TypewriterCoroutine(text));
        }
        else
        {
            Debug.LogError("[DialogueUI] Cannot start typewriter! gameObject is inactive and DialogueManager.Instance is null!");
            isTyping = false;
        }
    }
    
    private IEnumerator TypewriterCoroutine(string text)
    {
        Debug.Log($"[DialogueUI] TypewriterCoroutine ENTERED. Text length: {text?.Length ?? 0}, dialogueText null: {dialogueText == null}");
        
        if (dialogueText == null)
        {
            Debug.LogError("[DialogueUI] dialogueText is null! Cannot run typewriter.");
            isTyping = false;
            yield break;
        }
        
        // Debug: Log what text we're about to type
        Debug.Log($"[DialogueUI] Typewriter starting with text length: {text?.Length ?? 0}");
        if (!string.IsNullOrEmpty(text) && text.Length > 0)
        {
            int previewLength = Mathf.Min(50, text.Length);
            Debug.Log($"[DialogueUI] Typewriter text preview: {text.Substring(0, previewLength)}");
        }
        
        // Wait one frame to ensure everything is set up
        Debug.Log("[DialogueUI] Typewriter waiting one frame...");
        yield return null;
        Debug.Log("[DialogueUI] Typewriter frame wait complete, starting character loop...");
        
        // Simple string-building approach - build text character by character
        // This works even if the GameObject isn't fully active in hierarchy
        dialogueText.text = ""; // Start with empty text
        
        StringBuilder displayText = new StringBuilder();
        
        Debug.Log($"[DialogueUI] Typewriter: Starting to build text character by character ({text.Length} chars)");
        
        // Build text character by character
        for (int i = 0; i < text.Length; i++)
        {
            if (dialogueText == null)
            {
                Debug.LogError($"[DialogueUI] dialogueText became null during typewriter at char {i}!");
                break;
            }
            
            char c = text[i];
            displayText.Append(c);
            
            // Set the text - use SetText for TextMeshPro
            dialogueText.SetText(displayText.ToString());
            
            // Force update
            dialogueText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            
            // Debug: Log first few and every 10th character
            if (i < 5 || i % 10 == 0)
            {
                Debug.Log($"[DialogueUI] Typewriter: Added character {i + 1}/{text.Length} ('{c}'), text now: '{displayText.ToString().Substring(0, Mathf.Min(20, displayText.Length))}...'");
            }
            
            // Wait before adding next character
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        // Ensure final text is set
        if (dialogueText != null)
        {
            dialogueText.SetText(text);
            dialogueText.ForceMeshUpdate();
        }
        
        Debug.Log($"[DialogueUI] Typewriter finished. Final text length: {displayText.Length}, expected: {text.Length}");
        
        isTyping = false;
        typewriterCoroutine = null;
        typewriterCoroutineOnManager = null;
    }
    
    private void ShowChoices(List<DialogueChoice> choices)
    {
        // Clear existing choices first
        ClearChoices();
        
        // Validate inputs
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("[DialogueUI] Choice button prefab is null! Cannot create choice buttons.");
            return;
        }
        
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("[DialogueUI] ShowChoices: choices list is null or empty!");
            return;
        }
        
        // Check for EventSystem (required for UI interactions)
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.LogError("[DialogueUI] No EventSystem found! Buttons will not be clickable. Please add an EventSystem to the scene.");
        }
        
        // Find scene instance of choicesContainer if it's a prefab
        Transform containerToUse = choicesContainer;
        if (choicesContainer != null)
        {
            // Check if this is a prefab asset (not a scene instance)
            if (string.IsNullOrEmpty(choicesContainer.gameObject.scene.name))
            {
                Debug.LogWarning("[DialogueUI] choicesContainer appears to be a prefab asset, searching for scene instance...");
                // Search recursively under dialogue panel
                Transform found = FindChildRecursive(resolvedDialoguePanel?.transform ?? transform, choicesContainer.name);
                if (found != null)
                {
                    containerToUse = found;
                    Debug.Log($"[DialogueUI] Found scene instance: {found.name}");
                }
            }
        }
        
        if (containerToUse == null)
        {
            Debug.LogError("[DialogueUI] Could not find choicesContainer in scene! Cannot display choices.");
            return;
        }
        
        // Ensure container is active and visible
        ActivateParentChain(containerToUse.gameObject);
        containerToUse.gameObject.SetActive(true);
        
        currentChoiceButtons.Clear();
        
        // Build list of valid choice indices (for condition filtering)
        List<int> validChoiceIndices = new List<int>();
        
        Debug.Log($"[DialogueUI] ShowChoices called with {choices.Count} total choices");
        Debug.Log($"[DialogueUI] Current node: {currentNode?.nodeId ?? "null"}");
        
        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];
            bool conditionMet = DialogueManager.Instance != null && 
                               DialogueManager.Instance.EvaluateChoiceCondition(choice);
            
            Debug.Log($"[DialogueUI] Choice[{i}]: '{choice.choiceText}' -> '{choice.targetNodeId}', Condition met: {conditionMet}");
            
            // Check condition - only show choices that meet their conditions
            if (conditionMet)
            {
                validChoiceIndices.Add(i);
                Debug.Log($"[DialogueUI]   -> Adding to validChoiceIndices (index: {i})");
            }
            else
            {
                Debug.Log($"[DialogueUI]   -> Skipping (condition not met)");
            }
        }
        
        if (validChoiceIndices.Count == 0)
        {
            Debug.LogWarning("[DialogueUI] No valid choices to display after condition filtering.");
            return;
        }
        
        Debug.Log($"[DialogueUI] Valid choice indices: [{string.Join(", ", validChoiceIndices)}]");
        
        // Create buttons only for valid choices
        Debug.Log($"[DialogueUI] Creating {validChoiceIndices.Count} choice buttons...");
        for (int idx = 0; idx < validChoiceIndices.Count; idx++)
        {
            // Get the original index from the choices list (before filtering)
            int originalChoiceIndex = validChoiceIndices[idx];
            var choice = choices[originalChoiceIndex];
            
            Debug.Log($"[DialogueUI] Creating button {idx} for choice {originalChoiceIndex}: '{choice.choiceText}'");
            
            // Instantiate button
            GameObject choiceObj = Instantiate(choiceButtonPrefab, containerToUse);
            if (choiceObj == null)
            {
                Debug.LogError($"[DialogueUI] Failed to instantiate choice button prefab!");
                continue;
            }
            
            // Get Button component (should exist on prefab root)
            Button choiceButton = choiceObj.GetComponent<Button>();
            if (choiceButton == null)
            {
                Debug.LogError($"[DialogueUI] Choice prefab '{choiceObj.name}' has no Button component on root! Cannot create clickable button.");
                Destroy(choiceObj);
                continue;
            }
            
            // Set choice text
            TextMeshProUGUI choiceText = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            if (choiceText != null)
            {
                choiceText.text = choice.choiceText;
            }
            else
            {
                Debug.LogWarning($"[DialogueUI] Choice button has no TextMeshProUGUI component for text!");
            }
            
            // Ensure button has a target graphic for raycasting
            // Button needs an Image or Graphic component to be clickable
            if (choiceButton.targetGraphic == null)
            {
                Debug.LogWarning($"[DialogueUI] Button '{choiceObj.name}' has no targetGraphic set! Searching for Graphic component...");
                
                // Try to find an Image component on the button or its children
                UnityEngine.UI.Image buttonImage = choiceObj.GetComponent<UnityEngine.UI.Image>();
                if (buttonImage == null)
                {
                    buttonImage = choiceObj.GetComponentInChildren<UnityEngine.UI.Image>();
                }
                
                if (buttonImage == null)
                {
                    // Try to find any Graphic component
                    UnityEngine.UI.Graphic graphic = choiceObj.GetComponent<UnityEngine.UI.Graphic>();
                    if (graphic == null)
                    {
                        graphic = choiceObj.GetComponentInChildren<UnityEngine.UI.Graphic>();
                    }
                    
                    if (graphic != null)
                    {
                        choiceButton.targetGraphic = graphic;
                        Debug.Log($"[DialogueUI] Set button targetGraphic to {graphic.GetType().Name} on '{graphic.gameObject.name}'");
                    }
                    else
                    {
                        // Add an Image component if none exists (for raycasting)
                        buttonImage = choiceObj.AddComponent<UnityEngine.UI.Image>();
                        buttonImage.color = new Color(1f, 1f, 1f, 0f); // Transparent
                        choiceButton.targetGraphic = buttonImage;
                        Debug.Log($"[DialogueUI] Added transparent Image component for raycasting on root GameObject");
                    }
                }
                else
                {
                    choiceButton.targetGraphic = buttonImage;
                    Debug.Log($"[DialogueUI] Set button targetGraphic to existing Image component on '{buttonImage.gameObject.name}'");
                }
            }
            else
            {
                Debug.Log($"[DialogueUI] Button already has targetGraphic: {choiceButton.targetGraphic.GetType().Name} on '{choiceButton.targetGraphic.gameObject.name}'");
            }
            
            // Ensure target graphic has raycastTarget enabled
            if (choiceButton.targetGraphic != null)
            {
                choiceButton.targetGraphic.raycastTarget = true;
                
                // Ensure the target graphic's GameObject is active
                if (!choiceButton.targetGraphic.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"[DialogueUI] Target graphic GameObject '{choiceButton.targetGraphic.gameObject.name}' is not active! Activating...");
                    choiceButton.targetGraphic.gameObject.SetActive(true);
                    ActivateParentChain(choiceButton.targetGraphic.gameObject);
                }
                
                Debug.Log($"[DialogueUI] Target graphic raycastTarget enabled: {choiceButton.targetGraphic.raycastTarget}, " +
                         $"Active: {choiceButton.targetGraphic.gameObject.activeSelf}, " +
                         $"ActiveInHierarchy: {choiceButton.targetGraphic.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogError($"[DialogueUI] Button '{choiceObj.name}' still has no targetGraphic after setup! Button clicks will NOT work!");
            }
            
            // Ensure button is active, visible, and interactable
            choiceButton.gameObject.SetActive(true);
            choiceButton.interactable = true;
            choiceButton.enabled = true;
            
            // Ensure parent chain is active
            ActivateParentChain(choiceButton.gameObject);
            
            // Verify EventSystem exists (required for button clicks)
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                Debug.LogError($"[DialogueUI] EventSystem not found! Button clicks will not work. Please add an EventSystem to the scene.");
            }
            
            // Verify GraphicRaycaster exists (required for UI raycasting)
            Canvas canvas = choiceButton.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                UnityEngine.UI.GraphicRaycaster raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    Debug.LogWarning($"[DialogueUI] Canvas '{canvas.name}' has no GraphicRaycaster! Button clicks may not work.");
                }
                
                // Check for CanvasGroup that might block raycasts
                CanvasGroup canvasGroup = choiceButton.GetComponentInParent<CanvasGroup>();
                if (canvasGroup != null && !canvasGroup.blocksRaycasts)
                {
                    Debug.LogWarning($"[DialogueUI] CanvasGroup '{canvasGroup.name}' has blocksRaycasts=false! This may prevent button clicks.");
                }
            }
            
            // Remove any existing listeners first
            choiceButton.onClick.RemoveAllListeners();
            
            // Test: Add a simple test listener to verify onClick works at all
            choiceButton.onClick.AddListener(() => {
                Debug.Log($"[DialogueUI] TEST: Button onClick fired for '{choice.choiceText}'");
            });
            
            // Wire up click handler - use originalChoiceIndex so DialogueManager can find the right choice
            int capturedChoiceIndex = originalChoiceIndex; // Capture for closure
            string capturedChoiceText = choice.choiceText; // Capture for logging
            string capturedTargetNodeId = choice.targetNodeId; // Capture for verification
            
            choiceButton.onClick.AddListener(() => {
                Debug.Log($"[DialogueUI] ===== CHOICE BUTTON CLICKED! ===== ");
                Debug.Log($"[DialogueUI] Button index in loop: {idx}");
                Debug.Log($"[DialogueUI] Original choice index (from validChoiceIndices): {capturedChoiceIndex}");
                Debug.Log($"[DialogueUI] Choice text: '{capturedChoiceText}'");
                Debug.Log($"[DialogueUI] Expected target node: '{capturedTargetNodeId}'");
                Debug.Log($"[DialogueUI] Current node ID: {currentNode?.nodeId ?? "null"}");
                Debug.Log($"[DialogueUI] Current node has {currentNode?.choices?.Count ?? 0} choices");
                if (currentNode?.choices != null)
                {
                    for (int i = 0; i < currentNode.choices.Count; i++)
                    {
                        Debug.Log($"[DialogueUI]   Choice[{i}]: '{currentNode.choices[i].choiceText}' -> '{currentNode.choices[i].targetNodeId}'");
                    }
                }
                OnChoiceSelected(capturedChoiceIndex);
            });
            
            // Also add IPointerClickHandler as a backup
            var clickHandler = choiceObj.GetComponent<ChoiceButtonClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = choiceObj.AddComponent<ChoiceButtonClickHandler>();
            }
            clickHandler.Initialize(capturedChoiceIndex, this);
            
            currentChoiceButtons.Add(choiceButton);
            
            Debug.Log($"[DialogueUI] Button {idx} created for choice '{choice.choiceText}' (index {capturedChoiceIndex}). " +
                     $"Active: {choiceButton.gameObject.activeSelf}, ActiveInHierarchy: {choiceButton.gameObject.activeInHierarchy}, " +
                     $"Interactable: {choiceButton.interactable}, Enabled: {choiceButton.enabled}, " +
                     $"TargetGraphic: {(choiceButton.targetGraphic != null ? choiceButton.targetGraphic.GetType().Name : "NULL")}");
        }
        
        // Force canvas update to ensure buttons are visible
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"[DialogueUI] Successfully created {currentChoiceButtons.Count} choice buttons.");
    }
    
    private void ClearChoices()
    {
        // Remove listeners before destroying
        foreach (var button in currentChoiceButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                
                // Also remove ChoiceButtonClickHandler to prevent stale handlers
                var clickHandler = button.GetComponent<ChoiceButtonClickHandler>();
                if (clickHandler != null)
                {
                    // Clear the handler's reference to prevent it from calling old callbacks
                    clickHandler.Initialize(-1, null); // Invalidate it
                    Destroy(clickHandler);
                }
            }
        }
        currentChoiceButtons.Clear();
        
        // Find the actual container to use (scene instance)
        Transform containerToUse = choicesContainer;
        if (choicesContainer != null && string.IsNullOrEmpty(choicesContainer.gameObject.scene.name))
        {
            // It's a prefab, find scene instance
            Transform found = FindChildRecursive(resolvedDialoguePanel?.transform ?? transform, choicesContainer.name);
            if (found != null)
            {
                containerToUse = found;
            }
        }
        
        if (containerToUse == null)
            return;
        
        // Destroy all child buttons (including any stale ones)
        // We need to create a list first because we're modifying the collection during iteration
        List<Transform> childrenToDestroy = new List<Transform>();
        foreach (Transform child in containerToUse)
        {
            if (child != null)
            {
                childrenToDestroy.Add(child);
            }
        }
        
        foreach (var child in childrenToDestroy)
        {
            if (child != null)
            {
                // Remove any ChoiceButtonClickHandler before destroying
                var clickHandler = child.GetComponent<ChoiceButtonClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.Initialize(-1, null); // Invalidate it
                }
                Destroy(child.gameObject);
            }
        }
    }
    
    public void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"[DialogueUI] ========== OnChoiceSelected CALLED ==========");
        Debug.Log($"[DialogueUI] Received choiceIndex: {choiceIndex}");
        
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[DialogueUI] DialogueManager.Instance is null! Cannot select choice.");
            return;
        }
        
        if (currentNode == null)
        {
            Debug.LogError("[DialogueUI] currentNode is null! Cannot select choice.");
            return;
        }
        
        // Verify the choice index is valid for the CURRENT node (before navigation)
        if (currentNode.choices == null || choiceIndex < 0 || choiceIndex >= currentNode.choices.Count)
        {
            Debug.LogError($"[DialogueUI] Invalid choice index {choiceIndex}! Current node '{currentNode.nodeId}' has {currentNode.choices?.Count ?? 0} choices.");
            Debug.LogError($"[DialogueUI] This might be a stale button click from a previous node. Ignoring.");
            return;
        }
        
        // Log which choice we're selecting
        var selectedChoice = currentNode.choices[choiceIndex];
        Debug.Log($"[DialogueUI] Selecting choice[{choiceIndex}]: '{selectedChoice.choiceText}'");
        Debug.Log($"[DialogueUI] Choice target node ID: '{selectedChoice.targetNodeId}'");
        
        // IMPORTANT: Clear choices IMMEDIATELY and make all buttons non-interactable
        // This prevents stale buttons from firing again after node transition
        foreach (var button in currentChoiceButtons)
        {
            if (button != null)
            {
                button.interactable = false; // Disable immediately
                button.onClick.RemoveAllListeners(); // Remove listeners
                
                // Invalidate ChoiceButtonClickHandler
                var clickHandler = button.GetComponent<ChoiceButtonClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.Initialize(-1, null);
                }
            }
        }
        
        // Now clear all choices completely
        ClearChoices();
        
        // Reset paragraph index when a choice is made (for next dialogue node)
        DialogueManager.Instance.currentParagraphIndex = 0;
        
        // IMPORTANT: Activate continue button when a choice is selected
        // This ensures the continue button is visible when the new node is displayed
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
            // Ensure parent chain is active
            ActivateParentChain(continueButton.gameObject);
            Debug.Log("[DialogueUI] Activated continue button after choice selection");
        }
        
        // Deactivate all paragraph GameObjects to clear current display
        TextMeshProUGUI[] textsToUse = resolvedParagraphTexts != null ? resolvedParagraphTexts : paragraphTexts;
        if (textsToUse != null && textsToUse.Length > 0)
        {
            for (int i = 0; i < textsToUse.Length; i++)
            {
                if (textsToUse[i] != null)
                {
                    textsToUse[i].gameObject.SetActive(false);
                }
            }
        }
        
        // Tell DialogueManager to advance to the next node based on choice
        Debug.Log($"[DialogueUI] Calling DialogueManager.SelectChoice({choiceIndex}) to advance conversation...");
        DialogueManager.Instance.SelectChoice(choiceIndex);
    }
    
    private void SplitIntoParagraphs(string text)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[DialogueUI] DialogueManager.Instance is null! Cannot split paragraphs.");
            return;
        }
        
        Debug.Log($"[DialogueUI] SplitIntoParagraphs: Clearing currentParagraphs (had {DialogueManager.Instance.currentParagraphs.Count} items)");
        DialogueManager.Instance.currentParagraphs.Clear();
        
        if (string.IsNullOrEmpty(text))
        {
            DialogueManager.Instance.currentParagraphs.Add("");
            return;
        }
        
        // Handle both literal "\n\n" strings and actual newline characters
        // First, replace literal "\n" with actual newlines if needed
        string processedText = text;
        // Check for literal backslash-n (could be stored as "\\n" in string or shown as "\n" in inspector)
        if (text.Contains("\\n"))
        {
            Debug.Log("[DialogueUI] Found literal \\n in text, converting to actual newlines");
            processedText = text.Replace("\\n", "\n");
            Debug.Log($"[DialogueUI] After replacement, text contains actual newlines: {processedText.Contains("\n")}");
        }
        
        // Also handle the case where paragraphDelimiter might need to be adjusted
        // If we're looking for "\n\n" but the text has been processed, make sure delimiter matches
        string delimiterToUse = paragraphDelimiter;
        if (paragraphDelimiter == "\\n\\n" && processedText.Contains("\n"))
        {
            delimiterToUse = "\n\n";
            Debug.Log("[DialogueUI] Adjusted delimiter from '\\n\\n' to actual newlines");
        }
        
        // Split by paragraph delimiter (default: double newline)
        string[] paragraphs = processedText.Split(new[] { delimiterToUse }, System.StringSplitOptions.None);
        Debug.Log($"[DialogueUI] Split text into {paragraphs.Length} parts using delimiter (length: {delimiterToUse.Length})");
        
        // Also split by single newlines if they're separated by blank lines
        List<string> finalParagraphs = new List<string>();
        foreach (var para in paragraphs)
        {
            // Trim and add non-empty paragraphs
            var trimmed = para.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                finalParagraphs.Add(trimmed);
            }
        }
        
        // If no paragraphs found, use the whole text
        if (finalParagraphs.Count == 0)
        {
            Debug.Log("[DialogueUI] No paragraphs found after splitting, using whole text");
            DialogueManager.Instance.currentParagraphs.Add(processedText);
        }
        else
        {
            Debug.Log($"[DialogueUI] Found {finalParagraphs.Count} paragraphs after trimming");
            for (int i = 0; i < finalParagraphs.Count; i++)
            {
                Debug.Log($"[DialogueUI] Paragraph {i}: '{finalParagraphs[i].Substring(0, Mathf.Min(50, finalParagraphs[i].Length))}...'");
            }
            DialogueManager.Instance.currentParagraphs.AddRange(finalParagraphs);
            Debug.Log($"[DialogueUI] SplitIntoParagraphs: currentParagraphs now has {DialogueManager.Instance.currentParagraphs.Count} items");
        }
    }
    
    /// <summary>
    /// <summary>
    /// Clear all paragraph text from paragraph GameObjects immediately and synchronously
    /// This prevents flickering when transitioning between dialogue nodes
    /// </summary>
    private void ClearAllParagraphText()
    {
        // Use resolved paragraph texts if available, otherwise fall back to original array
        TextMeshProUGUI[] textsToUse = resolvedParagraphTexts != null ? resolvedParagraphTexts : paragraphTexts;
        
        if (textsToUse != null && textsToUse.Length > 0)
        {
            for (int i = 0; i < textsToUse.Length; i++)
            {
                if (textsToUse[i] != null)
                {
                    // Clear text immediately
                    textsToUse[i].text = "";
                    // Force mesh update to clear visual text immediately
                    textsToUse[i].ForceMeshUpdate();
                    // Deactivate GameObject
                    textsToUse[i].gameObject.SetActive(false);
                }
            }
            // Force canvas update to ensure changes are visible immediately
            Canvas.ForceUpdateCanvases();
            Debug.Log("[DialogueUI] Cleared all paragraph text from paragraph GameObjects (synchronously)");
        }
        else
        {
            // Fallback: try to find paragraph GameObjects by name if array isn't set up
            GameObject searchRoot = resolvedDialoguePanel != null ? resolvedDialoguePanel : dialoguePanel;
            if (searchRoot == null) searchRoot = gameObject;
            
            for (int i = 1; i <= 10; i++)
            {
                string paragraphName = $"Paragraph{i}";
                Transform found = FindChildRecursive(searchRoot.transform, paragraphName);
                if (found != null)
                {
                    TextMeshProUGUI textComp = found.GetComponent<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = "";
                        textComp.ForceMeshUpdate();
                        textComp.gameObject.SetActive(false);
                    }
                }
            }
            Canvas.ForceUpdateCanvases();
            Debug.Log("[DialogueUI] Cleared paragraph text via fallback search");
        }
    }
    
    /// <summary>
    /// Pre-loads all paragraphs from the node into their respective paragraph GameObjects.
    /// This is called once when DisplayNode is called, so we only need to activate/deactivate GameObjects later.
    /// </summary>
    private void PreloadParagraphs(List<string> paragraphs)
    {
        if (paragraphs == null || paragraphs.Count == 0)
        {
            Debug.LogWarning("[DialogueUI] PreloadParagraphs: paragraphs list is null or empty!");
            return;
        }
        
        // Use resolved paragraph texts (scene instances) if available, otherwise fall back to original array
        TextMeshProUGUI[] textsToUse = resolvedParagraphTexts != null ? resolvedParagraphTexts : paragraphTexts;
        
        if (textsToUse == null || textsToUse.Length == 0)
        {
            Debug.LogWarning("[DialogueUI] PreloadParagraphs: paragraphTexts array is null or empty! Make sure you've assigned paragraph GameObjects in the Inspector.");
            return;
        }
        
        Debug.Log($"[DialogueUI] PreloadParagraphs: Loading {paragraphs.Count} paragraphs into GameObjects...");
        
        // IMPORTANT: Clear all paragraph text FIRST before loading new text
        // This prevents flickering when the same paragraph index is used in different nodes
        for (int i = 0; i < textsToUse.Length; i++)
        {
            if (textsToUse[i] != null)
            {
                textsToUse[i].text = "";
                textsToUse[i].gameObject.SetActive(false);
            }
        }
        Canvas.ForceUpdateCanvases(); // Force immediate visual update
        
        // Pre-load all paragraphs into their GameObjects
        for (int i = 0; i < paragraphs.Count && i < textsToUse.Length; i++)
        {
            if (textsToUse[i] != null)
            {
                // Trim any leading/trailing whitespace from the paragraph text
                string trimmedParagraph = paragraphs[i].Trim();
                
                // Ensure text is set
                textsToUse[i].text = trimmedParagraph;
                
                // Ensure text color is visible
                if (textsToUse[i].color.a < 1f)
                {
                    Color textColor = textsToUse[i].color;
                    textColor.a = 1f;
                    textsToUse[i].color = textColor;
                    Debug.Log($"[DialogueUI] PreloadParagraphs: Fixed text color alpha for paragraph {i + 1}");
                }
                
                // Force text update
                textsToUse[i].ForceMeshUpdate();
                
                // Keep GameObject inactive for now - will be activated by ShowCurrentParagraph
                textsToUse[i].gameObject.SetActive(false);
                
                Debug.Log($"[DialogueUI] Preloaded paragraph {i + 1} into GameObject '{textsToUse[i].gameObject.name}': '{trimmedParagraph.Substring(0, Mathf.Min(50, trimmedParagraph.Length))}...' (text length: {trimmedParagraph.Length})");
            }
            else
            {
                Debug.LogWarning($"[DialogueUI] PreloadParagraphs: paragraphTexts[{i}] is null! Make sure all paragraph GameObjects are assigned in the Inspector.");
            }
        }
        
        // Force canvas update after preloading to ensure all text is set
        Canvas.ForceUpdateCanvases();
        
        // Deactivate any remaining paragraph GameObjects that weren't used
        for (int i = paragraphs.Count; i < textsToUse.Length; i++)
        {
            if (textsToUse[i] != null)
            {
                textsToUse[i].gameObject.SetActive(false);
                textsToUse[i].text = ""; // Clear unused paragraph GameObjects
            }
        }
        
        Debug.Log($"[DialogueUI] PreloadParagraphs: Finished loading {paragraphs.Count} paragraphs.");
    }
    
    private void ShowCurrentParagraph()
    {
        // Prevent duplicate calls while typewriter is running
        if (isTyping)
        {
            Debug.LogWarning("[DialogueUI] ShowCurrentParagraph called while typewriter is already running. Ignoring duplicate call.");
            return;
        }
        
        if (currentParagraphs == null || currentParagraphs.Count == 0)
        {
            Debug.LogWarning("[DialogueUI] ShowCurrentParagraph: currentParagraphs is null or empty!");
            ShowChoicesOrContinue(currentNode);
            return;
        }
        
        if (currentParagraphIndex >= currentParagraphs.Count)
        {
            ShowChoicesOrContinue(currentNode);
            return;
        }
        
        // Use paragraph GameObjects if available (PREFERRED METHOD)
        // Text is already pre-loaded, we just need to activate/deactivate
        // Use resolved paragraph texts (scene instances) if available
        TextMeshProUGUI[] textsToUse = resolvedParagraphTexts != null ? resolvedParagraphTexts : paragraphTexts;
        
        if (textsToUse != null && textsToUse.Length > 0 && currentParagraphIndex < textsToUse.Length)
        {
            // CRITICAL: Deactivate all paragraph GameObjects first, but DON'T clear text yet
            // We'll clear unused paragraphs after activating the current one
            for (int i = 0; i < textsToUse.Length; i++)
            {
                if (textsToUse[i] != null)
                {
                    textsToUse[i].gameObject.SetActive(false);
                }
            }
            
            // Activate current paragraph (text is already pre-loaded from PreloadParagraphs)
            if (textsToUse[currentParagraphIndex] != null)
            {
                TextMeshProUGUI currentParagraphText = textsToUse[currentParagraphIndex];
                
                // Read text from currentParagraphs list to ensure we have the correct text
                string paragraphText = "";
                if (currentParagraphs != null && currentParagraphIndex < currentParagraphs.Count)
                {
                    paragraphText = currentParagraphs[currentParagraphIndex].Trim();
                }
                else
                {
                    Debug.LogWarning($"[DialogueUI] ShowCurrentParagraph: currentParagraphs[{currentParagraphIndex}] is null or out of range!");
                }
                
                // If text is empty, try to read from the GameObject (it should have been preloaded)
                if (string.IsNullOrEmpty(paragraphText))
                {
                    paragraphText = currentParagraphText.text?.Trim() ?? "";
                }
                
                Debug.Log($"[DialogueUI] ShowCurrentParagraph: Activating paragraph {currentParagraphIndex + 1}, text length: {paragraphText?.Length ?? 0}");
                
                // Ensure text is set (from preload or currentParagraphs)
                currentParagraphText.text = paragraphText;
                
                // Activate the GameObject
                currentParagraphText.gameObject.SetActive(true);
                
                // Ensure parent chain is active
                ActivateParentChain(currentParagraphText.gameObject);
                
                // Ensure text is visible (check color alpha)
                if (currentParagraphText.color.a < 1f)
                {
                    Color textColor = currentParagraphText.color;
                    textColor.a = 1f;
                    currentParagraphText.color = textColor;
                    Debug.Log($"[DialogueUI] ShowCurrentParagraph: Fixed text color alpha from {currentParagraphText.color.a} to 1.0");
                }
                
                // Now clear unused paragraphs (those beyond the current node's paragraph count)
                if (currentParagraphs != null)
                {
                    for (int i = currentParagraphs.Count; i < textsToUse.Length; i++)
                    {
                        if (textsToUse[i] != null)
                        {
                            textsToUse[i].text = "";
                            textsToUse[i].gameObject.SetActive(false);
                        }
                    }
                }
                
                // Trigger typewriter effect if the component exists
                typewriterUI typewriter = currentParagraphText.GetComponent<typewriterUI>();
                if (typewriter != null)
                {
                    // Manually trigger the typewriter effect since Start() only runs once
                    typewriter.StartTypewriterEffect();
                }
                
                // Force update
                currentParagraphText.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
                
                Debug.Log($"[DialogueUI] ShowCurrentParagraph: Paragraph {currentParagraphIndex + 1} activated and visible. Text: '{paragraphText.Substring(0, Mathf.Min(50, paragraphText.Length))}...'");
            }
            else
            {
                Debug.LogWarning($"[DialogueUI] Paragraph {currentParagraphIndex + 1} GameObject is null!");
            }
        }
        // Fallback to legacy single dialogueText field
        else if (dialogueText != null)
        {
            string paragraph = currentParagraphs[currentParagraphIndex];
            // Ensure the GameObject is active
            if (!dialogueText.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[DialogueUI] dialogueText GameObject is not active in hierarchy! Activating...");
                dialogueText.gameObject.SetActive(true);
                // Activate parent chain
                ActivateParentChain(dialogueText.gameObject);
            }
            
            // Always set text directly - typewriter is disabled
            dialogueText.text = paragraph;
            dialogueText.maxVisibleCharacters = -1; // Show all characters
            
            // Force TextMeshPro to update the mesh
            dialogueText.ForceMeshUpdate();
            dialogueText.ComputeMarginSize();
            
            // Ensure text is visible (check color alpha)
            if (dialogueText.color.a < 1f)
            {
                Debug.LogWarning($"[DialogueUI] dialogueText color alpha is {dialogueText.color.a}, setting to 1.0");
                Color textColor = dialogueText.color;
                textColor.a = 1f;
                dialogueText.color = textColor;
            }
            
            // Check CanvasGroup on parents (might be blocking visibility)
            CanvasGroup parentCanvasGroup = dialogueText.GetComponentInParent<CanvasGroup>();
            if (parentCanvasGroup != null)
            {
                if (parentCanvasGroup.alpha < 1f)
                {
                    Debug.LogWarning($"[DialogueUI] Parent CanvasGroup alpha is {parentCanvasGroup.alpha}, setting to 1.0");
                    parentCanvasGroup.alpha = 1f;
                }
                if (!parentCanvasGroup.interactable)
                {
                    Debug.LogWarning("[DialogueUI] Parent CanvasGroup is not interactable, enabling...");
                    parentCanvasGroup.interactable = true;
                }
                if (!parentCanvasGroup.blocksRaycasts)
                {
                    Debug.LogWarning("[DialogueUI] Parent CanvasGroup blocksRaycasts is false, enabling...");
                    parentCanvasGroup.blocksRaycasts = true;
                }
            }
            
            // Check RectTransform bounds
            RectTransform rectTransform = dialogueText.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Debug.Log($"[DialogueUI] RectTransform: anchoredPosition={rectTransform.anchoredPosition}, sizeDelta={rectTransform.sizeDelta}, rect={rectTransform.rect}");
                if (rectTransform.rect.width <= 0 || rectTransform.rect.height <= 0)
                {
                    Debug.LogWarning("[DialogueUI] RectTransform has zero or negative size! This might prevent text from rendering.");
                }
            }
            
            // Force Canvas update
            Canvas.ForceUpdateCanvases();
            
            // CRITICAL: Wait a frame and then verify/force the text to be visible
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartCoroutine(WaitAndVerifyTextAndForceVisible(paragraph));
            }
            
            isTyping = false;
            Debug.Log($"[DialogueUI] Set dialogue text directly (typewriter disabled). Text length: {paragraph.Length}");
            Debug.Log($"[DialogueUI] dialogueText.text after setting: '{dialogueText.text.Substring(0, Mathf.Min(100, dialogueText.text.Length))}...'");
            Debug.Log($"[DialogueUI] dialogueText GameObject active: {dialogueText.gameObject.activeSelf}, activeInHierarchy: {dialogueText.gameObject.activeInHierarchy}");
            Debug.Log($"[DialogueUI] dialogueText enabled: {dialogueText.enabled}, color: {dialogueText.color}, alpha: {dialogueText.color.a}");
        }
        else
        {
            Debug.LogError("[DialogueUI] dialogueText is null! Cannot display paragraph.");
        }
        
        // Show/hide continue button based on whether there are more paragraphs
        bool hasMoreParagraphs = currentParagraphIndex < currentParagraphs.Count - 1;
        bool hasChoices = currentNode != null && currentNode.HasChoices;
        
        Debug.Log($"[DialogueUI] ShowCurrentParagraph: index={currentParagraphIndex}/{currentParagraphs.Count}, hasMoreParagraphs={hasMoreParagraphs}, hasChoices={hasChoices}");
        
        if (continueButton != null)
        {
            // Show continue button if:
            // 1. There are more paragraphs to show (always show "Next" button), OR
            // 2. This is the last paragraph and it's not an end node (show "Continue" button), OR
            // 3. This is an end node but has exit actions (user needs to click Continue to trigger actions)
            bool hasExitActions = false;
            if (currentNode != null)
            {
                // Check if node has exit actions (either single or list)
                hasExitActions = (currentNode.onExitAction != null && currentNode.onExitAction.actionType != DialogueAction.ActionType.None) ||
                                 (currentNode.onExitActions != null && currentNode.onExitActions.Count > 0 && 
                                  currentNode.onExitActions.Any(a => a != null && a.actionType != DialogueAction.ActionType.None));
            }
            
            bool shouldShowContinue = hasMoreParagraphs || 
                                     (currentNode != null && !currentNode.IsEndNode) ||
                                     (currentNode != null && currentNode.IsEndNode && hasExitActions);
            
            // Always ensure button is set properly
            continueButton.gameObject.SetActive(shouldShowContinue);
            continueButton.interactable = shouldShowContinue; // Ensure button is clickable
            
            Debug.Log($"[DialogueUI] Continue button active: {shouldShowContinue}, interactable: {continueButton.interactable}, " +
                     $"(hasMoreParagraphs={hasMoreParagraphs}, hasChoices={hasChoices}, isEndNode={currentNode?.IsEndNode}, hasExitActions={hasExitActions})");
            
            // Update button text
            var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = hasMoreParagraphs ? "Next" : "Continue";
            }
            
            // Ensure button GameObject and all parents are active
            if (shouldShowContinue)
            {
                ActivateParentChain(continueButton.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("[DialogueUI] Continue button is null!");
        }
        
        // Hide choices until ALL paragraphs are shown
        if (hasChoices)
        {
            if (!hasMoreParagraphs)
            {
                // Last paragraph - show choices AFTER ensuring text is rendered
                Debug.Log("[DialogueUI] Last paragraph reached, will show choices after text renders...");
                
                // Wait a frame to ensure text is fully rendered, then show choices
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartCoroutine(ShowChoicesAfterTextRenders(currentNode));
                }
                else
                {
                    // Fallback if DialogueManager is null
                    ShowChoicesOrContinue(currentNode);
                }
            }
            else
            {
                // Still more paragraphs - hide choices until all are shown
                Debug.Log("[DialogueUI] More paragraphs remaining, hiding choices...");
                ClearChoices();
            }
        }
        else
        {
            // No choices, just clear any existing ones
            ClearChoices();
        }
    }
    
    private void ShowChoicesOrContinue(DialogueNode node)
    {
        if (node == null)
            return;
        
        // Ensure current paragraph is visible before showing choices
        // Use resolved paragraph texts (scene instances) if available
        TextMeshProUGUI[] textsToUse = resolvedParagraphTexts != null ? resolvedParagraphTexts : paragraphTexts;
        
        if (textsToUse != null && textsToUse.Length > 0 && currentParagraphIndex < textsToUse.Length)
        {
            if (textsToUse[currentParagraphIndex] != null)
            {
                TextMeshProUGUI currentParagraph = textsToUse[currentParagraphIndex];
                if (!currentParagraph.gameObject.activeInHierarchy)
                {
                    currentParagraph.gameObject.SetActive(true);
                    ActivateParentChain(currentParagraph.gameObject);
                }
                currentParagraph.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases();
            }
        }
        else if (dialogueText != null)
        {
            if (!dialogueText.gameObject.activeInHierarchy)
            {
                dialogueText.gameObject.SetActive(true);
                ActivateParentChain(dialogueText.gameObject);
            }
            dialogueText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
        }
        
        // Display choices or continue button
        // NOTE: Only call this when ALL paragraphs have been shown
        // If there are still paragraphs to show, the continue button should remain visible
        if (node.HasChoices)
        {
            // Only show choices if we're on the last paragraph
            // Otherwise, ShowCurrentParagraph will handle showing the continue button
            bool isLastParagraph = DialogueManager.Instance != null && 
                                   DialogueManager.Instance.currentParagraphIndex >= currentParagraphs.Count - 1;
            
            if (isLastParagraph)
            {
                ShowChoices(node.choices);
                // Hide continue button when choices are shown (user must select a choice)
                if (continueButton != null) continueButton.gameObject.SetActive(false);
            }
            else
            {
                // Still have paragraphs to show - don't show choices yet, keep continue button visible
                ClearChoices();
                // ShowCurrentParagraph will handle the continue button visibility
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(true);
                    continueButton.interactable = true;
                }
            }
            
            // Reset paragraph index when showing choices (for next dialogue node)
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.currentParagraphIndex = 0;
            }
        }
        else
        {
            ClearChoices();
            if (continueButton != null)
            {
                // Show continue button for non-end nodes
                bool shouldShow = !node.IsEndNode;
                continueButton.gameObject.SetActive(shouldShow);
                continueButton.interactable = shouldShow; // Ensure it's clickable
                var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Continue";
                }
            }
        }
    }
    
    private void OnContinueClicked()
    {
        if (isTyping)
        {
            // Skip typewriter effect
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            if (dialogueText != null && currentParagraphs != null && currentParagraphIndex < currentParagraphs.Count)
            {
                dialogueText.text = currentParagraphs[currentParagraphIndex];
            }
            isTyping = false;
            return;
        }
        
        // DIRECT APPROACH: Manually switch paragraph GameObjects
        // Use resolved paragraph texts (scene instances) if available
        TextMeshProUGUI[] textsToUse = resolvedParagraphTexts != null ? resolvedParagraphTexts : paragraphTexts;
        
        if (textsToUse != null && textsToUse.Length > 0 && 
            currentParagraphs != null && currentParagraphs.Count > 0)
        {
            // Check if there are more paragraphs to show
            if (currentParagraphIndex < currentParagraphs.Count - 1)
            {
                // Deactivate current paragraph GameObject
                if (currentParagraphIndex >= 0 && currentParagraphIndex < textsToUse.Length && 
                    textsToUse[currentParagraphIndex] != null)
                {
                    textsToUse[currentParagraphIndex].gameObject.SetActive(false);
                }
                
                // Increment to next paragraph
                currentParagraphIndex++;
                
                // Activate next paragraph GameObject
                if (currentParagraphIndex < textsToUse.Length && 
                    textsToUse[currentParagraphIndex] != null)
                {
                    textsToUse[currentParagraphIndex].gameObject.SetActive(true);
                    ActivateParentChain(textsToUse[currentParagraphIndex].gameObject);
                    textsToUse[currentParagraphIndex].ForceMeshUpdate();
                    Canvas.ForceUpdateCanvases();
                    
                    // Update continue button text
                    if (continueButton != null)
                    {
                        bool hasMoreParagraphs = currentParagraphIndex < currentParagraphs.Count - 1;
                        var buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = hasMoreParagraphs ? "Next" : "Continue";
                        }
                    }
                    
                    return; // Exit early - we've shown the next paragraph
                }
            }
        }
        
        // Fallback to original logic if paragraph GameObjects aren't available
        if (splitByParagraphs)
        {
            if (currentParagraphs == null || currentParagraphs.Count == 0)
            {
                Debug.LogWarning("[DialogueUI] OnContinueClicked: currentParagraphs is null or empty!");
                // Recovery: Re-load paragraphs from currentNode
                if (currentNode != null)
                {
                    if (currentNode.paragraphs != null && currentNode.paragraphs.Count > 0)
                    {
                        DialogueManager.Instance.currentParagraphs = new List<string>(currentNode.paragraphs);
                        DialogueManager.Instance.currentParagraphIndex = 0;
                        PreloadParagraphs(DialogueManager.Instance.currentParagraphs);
                        if (DialogueManager.Instance.currentParagraphs.Count > 0)
                        {
                            ShowCurrentParagraph();
                            return;
                        }
                    }
                    else if (!string.IsNullOrEmpty(currentNode.dialogueText))
                    {
                        SplitIntoParagraphs(currentNode.dialogueText);
                        DialogueManager.Instance.currentParagraphIndex = 0;
                        PreloadParagraphs(DialogueManager.Instance.currentParagraphs);
                        if (DialogueManager.Instance.currentParagraphs.Count > 0)
                        {
                            ShowCurrentParagraph();
                            return;
                        }
                    }
                }
            }
            else if (currentParagraphIndex < currentParagraphs.Count - 1)
            {
                // Show next paragraph
                currentParagraphIndex++;
                ShowCurrentParagraph();
                return;
            }
        }
        
        // No more paragraphs - show choices if node has them, otherwise continue to next node
        if (currentNode != null && currentNode.HasChoices)
        {
            ShowChoicesOrContinue(currentNode);
        }
        else
        {
            // Clear all paragraph text before moving to next node
            ClearAllParagraphText();
            
            // No choices, continue to next node or end dialogue
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Continue();
            }
        }
    }
    
    private void OnCloseClicked()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.EndDialogue();
        }
    }
    
    private void OnBackClicked()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.GoBack();
        }
    }
    
    
    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.interactable = false;
        }
        
        // currentNode and currentParagraphs are now managed by DialogueManager
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.currentParagraphs.Clear();
            DialogueManager.Instance.currentParagraphIndex = 0;
        }
        ClearChoices();
        
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        isTyping = false;
    }
    
    private System.Collections.IEnumerator WaitAndVerifyText()
    {
        yield return null; // Wait one frame
        
        if (dialogueText != null)
        {
            // Force another update after frame
            dialogueText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            
            // Check if text is actually visible
            if (string.IsNullOrEmpty(dialogueText.text))
            {
                Debug.LogError("[DialogueUI] After frame wait: dialogueText.text is empty!");
            }
            else
            {
                Debug.Log($"[DialogueUI] After frame wait: dialogueText.text length={dialogueText.text.Length}, visible: {dialogueText.gameObject.activeInHierarchy}");
            }
        }
    }
    
    private System.Collections.IEnumerator WaitAndVerifyTextAndForceVisible(string expectedText)
    {
        yield return null; // Wait one frame
        
        if (dialogueText != null)
        {
            // Ensure GameObject is active
            if (!dialogueText.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[DialogueUI] dialogueText not active in hierarchy after frame wait! Activating...");
                dialogueText.gameObject.SetActive(true);
                ActivateParentChain(dialogueText.gameObject);
            }
            
            // Verify text is still set correctly
            if (dialogueText.text != expectedText)
            {
                Debug.LogWarning($"[DialogueUI] Text mismatch after frame wait! Expected: '{expectedText.Substring(0, Mathf.Min(50, expectedText.Length))}...', Got: '{dialogueText.text.Substring(0, Mathf.Min(50, dialogueText.text.Length))}...'");
                // Restore the text
                dialogueText.text = expectedText;
            }
            
            // Force multiple updates to ensure rendering
            dialogueText.ForceMeshUpdate();
            dialogueText.ComputeMarginSize();
            Canvas.ForceUpdateCanvases();
            
            // Wait another frame
            yield return null;
            
            // Force one more update
            dialogueText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            
        }
    }
    
    private System.Collections.IEnumerator EnsureTextVisibleAfterChoices()
    {
        yield return null; // Wait one frame for choices to be instantiated
        
        if (dialogueText != null)
        {
            // Ensure GameObject is still active
            if (!dialogueText.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[DialogueUI] dialogueText became inactive after choices! Reactivating...");
                dialogueText.gameObject.SetActive(true);
                ActivateParentChain(dialogueText.gameObject);
            }
            
            // Force multiple updates to ensure text is rendered
            dialogueText.ForceMeshUpdate();
            dialogueText.ComputeMarginSize();
            Canvas.ForceUpdateCanvases();
            
            // Wait another frame
            yield return null;
            
            // Final force update
            dialogueText.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            
        }
    }
    
    private System.Collections.IEnumerator ShowChoicesAfterTextRenders(DialogueNode node)
    {
        // Wait one frame to ensure current paragraph is fully rendered
        yield return null;
        
        // Ensure current paragraph is visible
        // Use resolved paragraph texts (scene instances) if available
        TextMeshProUGUI[] textsToUse = resolvedParagraphTexts != null ? resolvedParagraphTexts : paragraphTexts;
        
        if (textsToUse != null && textsToUse.Length > 0 && currentParagraphIndex < textsToUse.Length)
        {
            if (textsToUse[currentParagraphIndex] != null)
            {
                TextMeshProUGUI currentParagraph = textsToUse[currentParagraphIndex];
                currentParagraph.gameObject.SetActive(true);
                ActivateParentChain(currentParagraph.gameObject);
                currentParagraph.ForceMeshUpdate();
                currentParagraph.ComputeMarginSize();
                Canvas.ForceUpdateCanvases();
            }
        }
        else if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);
            ActivateParentChain(dialogueText.gameObject);
            dialogueText.ForceMeshUpdate();
            dialogueText.ComputeMarginSize();
            Canvas.ForceUpdateCanvases();
        }
        
        // Wait another frame
        yield return null;
        
        // Now show choices
        ShowChoicesOrContinue(node);
    }
}

// Helper component to handle button clicks via IPointerClickHandler
public class ChoiceButtonClickHandler : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    private int choiceIndex;
    private DialogueUI dialogueUI;
    
    public void Initialize(int index, DialogueUI ui)
    {
        choiceIndex = index;
        dialogueUI = ui;
        Debug.Log($"[ChoiceButtonClickHandler] Initialized with choiceIndex={choiceIndex}");
    }
    
    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Debug.Log($"[ChoiceButtonClickHandler] ===== OnPointerClick FIRED! ===== Choice index: {choiceIndex}, GameObject: {gameObject.name}");
        
        // Check if this handler is invalid (was cleared)
        if (choiceIndex < 0 || dialogueUI == null)
        {
            Debug.LogWarning($"[ChoiceButtonClickHandler] Handler invalid or cleared (choiceIndex: {choiceIndex}). Ignoring click.");
            return;
        }
        
        // Verify we have a valid dialogueUI reference
        if (dialogueUI == null)
        {
            Debug.LogError("[ChoiceButtonClickHandler] dialogueUI is null! Cannot process choice.");
            return;
        }
        
        // Verify the button is still valid and hasn't been destroyed
        if (gameObject == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[ChoiceButtonClickHandler] Button GameObject is null or inactive. Ignoring click.");
            return;
        }
        
        // Verify we have a valid choice index
        Debug.Log($"[ChoiceButtonClickHandler] Calling DialogueUI.OnChoiceSelected({choiceIndex})...");
        dialogueUI.OnChoiceSelected(choiceIndex);
    }
    
    // Also handle other pointer events as backup
    public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Debug.Log($"[ChoiceButtonClickHandler] OnPointerDown detected on button {gameObject.name}");
    }
    
    public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Debug.Log($"[ChoiceButtonClickHandler] OnPointerUp detected on button {gameObject.name}");
    }
}

