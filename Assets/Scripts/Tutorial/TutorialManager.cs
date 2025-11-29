using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages tutorial sequences, highlighting UI elements and displaying tutorial text.
/// Persists across scenes to support cross-scene tutorials.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TutorialManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("TutorialManager");
                    _instance = go.AddComponent<TutorialManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject highlightOverlayPrefab;
    
    [Header("Settings")]
    [SerializeField] private float highlightPulseSpeed = 2f;
    [SerializeField] private float highlightPulseMinAlpha = 0.2f;
    [SerializeField] private float highlightPulseMaxAlpha = 0.5f;
    
    // Current tutorial state
    private TutorialData currentTutorial;
    public string currentTutorialId { get; private set; }
    private int currentStepIndex = -1;
    private TutorialStep currentStep;
    private GameObject currentHighlight;
    private List<GameObject> currentHighlights = new List<GameObject>(); // Support multiple highlights
    private Coroutine highlightPulseCoroutine;
    private List<Coroutine> highlightPulseCoroutines = new List<Coroutine>(); // Support multiple pulse coroutines
    private bool isTutorialActive = false;
    
    // Tutorial state persistence
    private string pendingTutorialId;
    private bool shouldResumeOnSceneLoad = false;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad only works on root GameObjects
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning($"[TutorialManager] Cannot use DontDestroyOnLoad on {gameObject.name} - it's not a root GameObject. Parent: {transform.parent.name}");
                // Move to root to make it persistent
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeUI();
        MakeTutorialPanelPersistent();
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void InitializeUI()
    {
        // Auto-find UI if not assigned
        if (tutorialPanel == null)
        {
            // Try to find tutorial panel in scene by name
            var panel = GameObject.Find("TutorialPanel");
            if (panel == null)
            {
                // Try searching in all canvases
                Canvas[] canvases = FindObjectsOfType<Canvas>(true);
                foreach (var canvas in canvases)
                {
                    Transform found = canvas.transform.Find("TutorialPanel");
                    if (found == null)
                    {
                        // Recursive search
                        found = FindChildRecursive(canvas.transform, "TutorialPanel");
                    }
                    if (found != null)
                    {
                        panel = found.gameObject;
                        break;
                    }
                }
            }
            if (panel != null)
            {
                tutorialPanel = panel;
                Debug.Log($"[TutorialManager] Found TutorialPanel: {panel.name} in scene");
            }
            else
            {
                Debug.LogWarning("[TutorialManager] TutorialPanel not found in scene. Tutorial UI will not be available.");
            }
        }
        
        // Get UI references from TutorialPanel
        if (tutorialPanel != null)
        {
            if (tutorialText == null)
            {
                tutorialText = tutorialPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (tutorialText == null)
                {
                    Debug.LogWarning("[TutorialManager] TutorialText (TextMeshProUGUI) not found in TutorialPanel.");
                }
            }
            
            if (nextButton == null)
            {
                // Find all buttons and look for "Next" button specifically
                Button[] buttons = tutorialPanel.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    // Check if button text contains "Next" or if it's not the skip button
                    var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null && buttonText.text.Contains("Next"))
                    {
                        nextButton = button;
                        break;
                    }
                    // If no text match, use first button that's not the skip button
                    if (nextButton == null && button != skipButton)
                    {
                        nextButton = button;
                    }
                }
                
                // Fallback: use first button if still null
                if (nextButton == null && buttons.Length > 0)
                {
                    nextButton = buttons[0];
                }
            }
            
            // Find skip button if not assigned
            if (skipButton == null)
            {
                Button[] buttons = tutorialPanel.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null && (buttonText.text.Contains("Skip") || buttonText.text.Contains("Close")))
                    {
                        skipButton = button;
                        break;
                    }
                }
            }
        }
        
        // Re-wire button listeners (remove old ones first to avoid duplicates)
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipTutorial);
        }
        
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextStep);
        }
        
        // Hide tutorial UI initially (unless tutorial is active)
        if (tutorialPanel != null && !isTutorialActive)
        {
            tutorialPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Make the TutorialPanel persistent across scenes by moving it to DontDestroyOnLoad
    /// This method makes the Canvas (TutorialCanvas) persistent, not just the panel
    /// </summary>
    private void MakeTutorialPanelPersistent()
    {
        if (tutorialPanel == null)
        {
            Debug.LogWarning("[TutorialManager] Cannot make TutorialPanel persistent - it's null. Will try again on scene load.");
            return;
        }
        
        // Find the parent Canvas (should be TutorialCanvas)
        Canvas panelCanvas = tutorialPanel.GetComponentInParent<Canvas>();
        
        if (panelCanvas == null)
        {
            // Try to find TutorialCanvas by name
            GameObject tutorialCanvasObj = GameObject.Find("TutorialCanvas");
            if (tutorialCanvasObj != null)
            {
                panelCanvas = tutorialCanvasObj.GetComponent<Canvas>();
                if (panelCanvas == null)
                {
                    panelCanvas = tutorialCanvasObj.AddComponent<Canvas>();
                }
                // Move TutorialPanel to TutorialCanvas if it's not already there
                if (tutorialPanel.transform.parent != tutorialCanvasObj.transform)
                {
                    tutorialPanel.transform.SetParent(tutorialCanvasObj.transform, false);
                }
            }
        }
        
        if (panelCanvas == null)
        {
            // If TutorialPanel itself is a Canvas, use it
            Canvas canvas = tutorialPanel.GetComponent<Canvas>();
            if (canvas != null)
            {
                panelCanvas = canvas;
            }
            else
            {
                // Create a Canvas for it if it doesn't have one
                GameObject canvasObj = new GameObject("TutorialCanvas");
                panelCanvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                // Move TutorialPanel to be a child of the new Canvas
                tutorialPanel.transform.SetParent(canvasObj.transform, false);
                Debug.Log("[TutorialManager] Created TutorialCanvas for TutorialPanel.");
            }
        }
        
        // Configure Canvas to ensure it overlays everything properly
        if (panelCanvas != null)
        {
            ConfigureTutorialCanvas(panelCanvas);
            
            // Make the Canvas persistent (not just the panel)
            // Check if Canvas is already persistent
            if (panelCanvas.gameObject.scene.name == "DontDestroyOnLoad")
            {
                Debug.Log($"[TutorialManager] TutorialCanvas '{panelCanvas.name}' is already persistent.");
                return;
            }
            
            // Ensure Canvas is a root GameObject (required for DontDestroyOnLoad)
            if (panelCanvas.transform.parent != null)
            {
                panelCanvas.transform.SetParent(null);
                Debug.Log($"[TutorialManager] Moved TutorialCanvas '{panelCanvas.name}' to root for persistence.");
            }
            
            // Make the Canvas persistent
            DontDestroyOnLoad(panelCanvas.gameObject);
            Debug.Log($"[TutorialManager] Made TutorialCanvas '{panelCanvas.name}' persistent (TutorialPanel will persist with it).");
        }
        else
        {
            Debug.LogError("[TutorialManager] Could not find or create Canvas for TutorialPanel! Panel may not be visible.");
        }
    }
    
    /// <summary>
    /// Configure TutorialCanvas with proper settings to overlay all content
    /// </summary>
    private void ConfigureTutorialCanvas(Canvas canvas)
    {
        if (canvas == null) return;
        
        // Ensure Screen Space Overlay mode for proper positioning
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Very high sorting order
        canvas.overrideSorting = true; // Ensure sorting order is respected
        
        // Ensure no world camera is set (for Screen Space Overlay)
        canvas.worldCamera = null;
        
        // Configure CanvasScaler if it exists
        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        }
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Ensure GraphicRaycaster exists for interaction
        if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Ensure TutorialPanel fills the entire screen
        if (tutorialPanel != null)
        {
            RectTransform panelRect = tutorialPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.localPosition = Vector3.zero;
                panelRect.localScale = Vector3.one;
            }
        }
        
        Debug.Log($"[TutorialManager] Configured TutorialCanvas: RenderMode={canvas.renderMode}, SortingOrder={canvas.sortingOrder}, OverrideSorting={canvas.overrideSorting}");
    }
    
    /// <summary>
    /// Recursively search for a child by name
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[TutorialManager] Scene loaded: {scene.name}");
        
        // Always ensure TutorialPanel exists and is properly set up
        EnsureTutorialPanelExists();
        
        // Resume tutorial if one was pending
        if (shouldResumeOnSceneLoad && !string.IsNullOrEmpty(pendingTutorialId))
        {
            Debug.Log($"[TutorialManager] Resuming tutorial '{pendingTutorialId}' from step {currentStepIndex} in scene {scene.name}");
            
            // Ensure TutorialManager GameObject is active (needed for coroutines)
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            
            StartCoroutine(ResumeTutorialAfterSceneLoad());
        }
        else if (isTutorialActive && currentTutorial != null)
        {
            // Tutorial is already active - just update UI references and show current step
            Debug.Log($"[TutorialManager] Tutorial '{currentTutorialId}' is already active, updating UI references for scene {scene.name}");
            
            // Ensure TutorialPanel is active
            EnsureTutorialPanelExists();
            if (tutorialPanel != null && !tutorialPanel.activeSelf)
            {
                tutorialPanel.SetActive(true);
                ActivateParentChain(tutorialPanel);
            }
            
            ShowStep(currentStepIndex);
        }
    }
    
    /// <summary>
    /// Ensure TutorialPanel exists and is properly initialized
    /// </summary>
    private void EnsureTutorialPanelExists()
    {
        // Check if TutorialPanel's Canvas is persistent and valid
        Canvas panelCanvas = tutorialPanel != null ? tutorialPanel.GetComponentInParent<Canvas>() : null;
        bool isPersistent = panelCanvas != null && panelCanvas.gameObject.scene.name == "DontDestroyOnLoad";
        
        if (tutorialPanel != null && isPersistent)
        {
            Debug.Log("[TutorialManager] TutorialPanel and TutorialCanvas are persistent - references should be intact.");
            // Ensure UI references are still valid
            RefreshUIReferences();
            return;
        }
        
        // TutorialPanel or its Canvas is not persistent or doesn't exist - try to find it
        if (tutorialPanel == null || !isPersistent)
        {
            Debug.Log("[TutorialManager] TutorialPanel or TutorialCanvas not found or not persistent. Searching for it...");
            
            // Clear old UI references
            tutorialPanel = null;
            tutorialText = null;
            nextButton = null;
            skipButton = null;
            
            // Try to find TutorialPanel in the new scene
            InitializeUI();
            
            // If still not found, create it
            if (tutorialPanel == null)
            {
                Debug.LogWarning("[TutorialManager] TutorialPanel not found in scene. Creating a new one.");
                CreateTutorialPanel();
            }
            
            // Make the Canvas persistent for future scene transitions
            MakeTutorialPanelPersistent();
        }
    }
    
    /// <summary>
    /// Refresh UI references from existing TutorialPanel
    /// </summary>
    private void RefreshUIReferences()
    {
        if (tutorialPanel == null) return;
        
        if (tutorialText == null)
        {
            tutorialText = tutorialPanel.GetComponentInChildren<TextMeshProUGUI>();
        }
        if (nextButton == null)
        {
            Button[] buttons = tutorialPanel.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && buttonText.text.Contains("Next"))
                {
                    nextButton = button;
                    break;
                }
            }
            // Fallback: use first button if no "Next" button found
            if (nextButton == null && buttons.Length > 0)
            {
                nextButton = buttons[0];
            }
        }
        if (skipButton == null)
        {
            Button[] buttons = tutorialPanel.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && (buttonText.text.Contains("Skip") || buttonText.text.Contains("Close")))
                {
                    skipButton = button;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Create a basic TutorialPanel if one doesn't exist
    /// </summary>
    private void CreateTutorialPanel()
    {
        // Create Canvas for TutorialPanel
        GameObject canvasObj = new GameObject("TutorialCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Configure the canvas (will be done by ConfigureTutorialCanvas)
        
        // Create TutorialPanel
        tutorialPanel = new GameObject("TutorialPanel");
        tutorialPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = tutorialPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.localPosition = Vector3.zero;
        panelRect.localScale = Vector3.one;
        
        // Add background
        UnityEngine.UI.Image bgImage = tutorialPanel.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black background
        
        // Create TutorialText
        GameObject textObj = new GameObject("TutorialText");
        textObj.transform.SetParent(tutorialPanel.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.1f);
        textRect.anchorMax = new Vector2(0.9f, 0.4f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        tutorialText = textObj.AddComponent<TextMeshProUGUI>();
        tutorialText.text = "Tutorial Text";
        tutorialText.fontSize = 24;
        tutorialText.color = Color.white;
        tutorialText.alignment = TextAlignmentOptions.Center;
        
        // Create NextButton
        GameObject nextBtnObj = new GameObject("NextButton");
        nextBtnObj.transform.SetParent(tutorialPanel.transform, false);
        RectTransform nextBtnRect = nextBtnObj.AddComponent<RectTransform>();
        nextBtnRect.anchorMin = new Vector2(0.7f, 0.05f);
        nextBtnRect.anchorMax = new Vector2(0.9f, 0.15f);
        nextBtnRect.offsetMin = Vector2.zero;
        nextBtnRect.offsetMax = Vector2.zero;
        
        nextButton = nextBtnObj.AddComponent<Button>();
        UnityEngine.UI.Image nextBtnImage = nextBtnObj.AddComponent<UnityEngine.UI.Image>();
        nextBtnImage.color = new Color(0.2f, 0.6f, 0.2f);
        
        GameObject nextBtnTextObj = new GameObject("Text");
        nextBtnTextObj.transform.SetParent(nextBtnObj.transform, false);
        RectTransform nextBtnTextRect = nextBtnTextObj.AddComponent<RectTransform>();
        nextBtnTextRect.anchorMin = Vector2.zero;
        nextBtnTextRect.anchorMax = Vector2.one;
        nextBtnTextRect.offsetMin = Vector2.zero;
        nextBtnTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI nextBtnText = nextBtnTextObj.AddComponent<TextMeshProUGUI>();
        nextBtnText.text = "Next";
        nextBtnText.fontSize = 20;
        nextBtnText.color = Color.white;
        nextBtnText.alignment = TextAlignmentOptions.Center;
        
        // Wire button listener
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextStep);
        
        // Configure the canvas after creating everything
        ConfigureTutorialCanvas(canvas);
        
        Debug.Log("[TutorialManager] Created TutorialPanel with basic UI elements.");
    }
    
    private IEnumerator ResumeTutorialAfterSceneLoad()
    {
        // Wait a few frames for scene to fully initialize
        yield return null;
        yield return null;
        yield return null;
        
        // Ensure TutorialPanel exists and is active
        EnsureTutorialPanelExists();
        
        if (tutorialPanel != null)
        {
            // Ensure Canvas is properly configured for positioning
            Canvas panelCanvas = tutorialPanel.GetComponentInParent<Canvas>();
            if (panelCanvas != null)
            {
                ConfigureTutorialCanvas(panelCanvas);
            }
            
            // Reset TutorialPanel positioning to fill screen
            RectTransform panelRect = tutorialPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.localPosition = Vector3.zero;
                panelRect.localScale = Vector3.one;
            }
            
            tutorialPanel.SetActive(true);
            ActivateParentChain(tutorialPanel);
            Debug.Log("[TutorialManager] TutorialPanel activated for tutorial resumption.");
        }
        else
        {
            Debug.LogError("[TutorialManager] TutorialPanel is still null after EnsureTutorialPanelExists! Cannot resume tutorial.");
            shouldResumeOnSceneLoad = false;
            pendingTutorialId = null;
            yield break;
        }
        
        // Load and start the tutorial
        var tutorial = LoadTutorialData(pendingTutorialId);
        if (tutorial != null)
        {
            StartTutorial(tutorial, currentStepIndex);
        }
        else
        {
            Debug.LogError($"[TutorialManager] Failed to load tutorial '{pendingTutorialId}' after scene transition.");
        }
        
        shouldResumeOnSceneLoad = false;
        pendingTutorialId = null;
    }
    
    /// <summary>
    /// Start a tutorial sequence. Can be called from dialogue actions or directly.
    /// </summary>
    public void StartTutorial(string tutorialId, int startStepIndex = 0)
    {
        var tutorial = LoadTutorialData(tutorialId);
        if (tutorial != null)
        {
            StartTutorial(tutorial, startStepIndex);
        }
        else
        {
            Debug.LogError($"[TutorialManager] Tutorial '{tutorialId}' not found!");
        }
    }
    
    /// <summary>
    /// Start a tutorial sequence from TutorialData asset.
    /// </summary>
    public void StartTutorial(TutorialData tutorial, int startStepIndex = 0)
    {
        if (tutorial == null || tutorial.steps == null || tutorial.steps.Count == 0)
        {
            Debug.LogWarning("[TutorialManager] Cannot start null or empty tutorial.");
            return;
        }
        
        currentTutorial = tutorial;
        currentTutorialId = tutorial.tutorialId;
        currentStepIndex = Mathf.Clamp(startStepIndex, 0, tutorial.steps.Count - 1);
        isTutorialActive = true;
        
        // Ensure TutorialManager GameObject is active (needed for coroutines)
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.Log("[TutorialManager] Activated TutorialManager GameObject for tutorial.");
        }
        
        // Ensure TutorialPanel exists before showing tutorial
        EnsureTutorialPanelExists();
        
        // Show tutorial UI - activate TutorialPanel and ensure it's visible
        if (tutorialPanel != null)
        {
            // Ensure Canvas is properly configured for positioning
            Canvas panelCanvas = tutorialPanel.GetComponentInParent<Canvas>();
            if (panelCanvas != null)
            {
                ConfigureTutorialCanvas(panelCanvas);
            }
            
            // Reset TutorialPanel positioning to fill screen
            RectTransform panelRect = tutorialPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.localPosition = Vector3.zero;
                panelRect.localScale = Vector3.one;
            }
            
            tutorialPanel.SetActive(true);
            // Ensure parent chain is active
            ActivateParentChain(tutorialPanel);
            Debug.Log("[TutorialManager] Activated TutorialPanel for tutorial display.");
        }
        else
        {
            Debug.LogError("[TutorialManager] TutorialPanel is null after EnsureTutorialPanelExists! Cannot display tutorial. Creating fallback panel...");
            CreateTutorialPanel();
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
                ActivateParentChain(tutorialPanel);
            }
        }
        
        // Show/hide skip button
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(tutorial.canSkip);
        }
        
        // Start first step
        ShowStep(currentStepIndex);
    }
    
    /// <summary>
    /// Show a specific tutorial step
    /// </summary>
    private void ShowStep(int stepIndex)
    {
        if (currentTutorial == null || stepIndex < 0 || stepIndex >= currentTutorial.steps.Count)
        {
            EndTutorial();
            return;
        }
        
        // Clear previous step
        ClearCurrentStep();
        
        currentStep = currentTutorial.steps[stepIndex];
        currentStepIndex = stepIndex;
        
        // Display tutorial text
        if (tutorialText != null)
        {
            tutorialText.text = currentStep.tutorialText ?? "";
        }
        
        // Execute onStart action
        ExecuteAction(currentStep.onStartAction);
        
        // Find and highlight target object(s)
        List<GameObject> targets = FindTargetObjects(currentStep);
        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                HighlightObject(target, currentStep.highlightColor, currentStep.usePulseAnimation);
            }
        }
        
        // Handle step completion based on type
        switch (currentStep.completionType)
        {
            case TutorialStep.CompletionType.ClickTarget:
                SetupClickTarget(currentStep);
                if (nextButton != null) nextButton.gameObject.SetActive(false);
                break;
                
            case TutorialStep.CompletionType.WaitForTime:
                StartCoroutine(WaitForTimeCoroutine(currentStep.waitTime));
                if (nextButton != null) nextButton.gameObject.SetActive(false);
                break;
                
            case TutorialStep.CompletionType.WaitForCondition:
                StartCoroutine(WaitForConditionCoroutine(currentStep.conditionName));
                if (nextButton != null) nextButton.gameObject.SetActive(false);
                break;
                
            case TutorialStep.CompletionType.Manual:
                if (nextButton != null) nextButton.gameObject.SetActive(true);
                break;
        }
        
        Debug.Log($"[TutorialManager] Showing tutorial step {stepIndex + 1}/{currentTutorial.steps.Count}: {currentStep.stepId}");
    }
    
    /// <summary>
    /// Find the target GameObject(s) for highlighting
    /// Supports both single targetObjectPath and multiple targetObjectPaths
    /// </summary>
    private List<GameObject> FindTargetObjects(TutorialStep step)
    {
        List<GameObject> targets = new List<GameObject>();
        
        // Check if multiple targets are specified
        if (step.targetObjectPaths != null && step.targetObjectPaths.Length > 0)
        {
            // Use the array of paths
            foreach (string path in step.targetObjectPaths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    GameObject found = FindGameObjectByPath(path);
                    if (found != null)
                    {
                        targets.Add(found);
                    }
                    else
                    {
                        Debug.LogWarning($"[TutorialManager] Could not find target object: {path}");
                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(step.targetObjectPath))
        {
            // Check if single path contains semicolons (multiple paths separated by semicolon)
            if (step.targetObjectPath.Contains(";"))
            {
                string[] paths = step.targetObjectPath.Split(';');
                foreach (string path in paths)
                {
                    string trimmedPath = path.Trim();
                    if (!string.IsNullOrEmpty(trimmedPath))
                    {
                        GameObject found = FindGameObjectByPath(trimmedPath);
                        if (found != null)
                        {
                            targets.Add(found);
                        }
                        else
                        {
                            Debug.LogWarning($"[TutorialManager] Could not find target object: {trimmedPath}");
                        }
                    }
                }
            }
            else
            {
                // Single target
                GameObject found = FindGameObjectByPath(step.targetObjectPath);
                if (found != null)
                {
                    targets.Add(found);
                }
                else
                {
                    Debug.LogWarning($"[TutorialManager] Could not find target object: {step.targetObjectPath}");
                }
            }
        }
        
        return targets;
    }
    
    /// <summary>
    /// Recursively search for a child GameObject by name
    /// </summary>
    private GameObject FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
            
            GameObject found = FindChildByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
    
    /// <summary>
    /// Highlight a GameObject with an overlay
    /// </summary>
    private void HighlightObject(GameObject target, Color color, bool pulse)
    {
        if (target == null) return;
        
        GameObject highlight = null;
        
        // Create highlight overlay
        if (highlightOverlayPrefab != null)
        {
            highlight = Instantiate(highlightOverlayPrefab, target.transform);
        }
        else
        {
            // Create a simple highlight overlay
            highlight = new GameObject("TutorialHighlight");
            highlight.transform.SetParent(target.transform, false);
            
            RectTransform rect = highlight.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image image = highlight.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }
        
        // Store highlight for cleanup
        currentHighlights.Add(highlight);
        
        // Keep single highlight reference for backward compatibility
        if (currentHighlight == null)
        {
            currentHighlight = highlight;
        }
        
        // Start pulse animation if requested
        if (pulse && highlight != null)
        {
            Coroutine pulseCoroutine = StartCoroutine(PulseHighlightCoroutine(highlight, color));
            highlightPulseCoroutines.Add(pulseCoroutine);
            
            // Keep single coroutine reference for backward compatibility
            if (highlightPulseCoroutine == null)
            {
                highlightPulseCoroutine = pulseCoroutine;
            }
        }
    }
    
    /// <summary>
    /// Pulse animation for highlight
    /// </summary>
    private IEnumerator PulseHighlightCoroutine(GameObject highlight, Color baseColor)
    {
        Image image = highlight.GetComponent<Image>();
        if (image == null) yield break;
        
        float time = 0f;
        while (highlight != null && image != null)
        {
            time += Time.deltaTime * highlightPulseSpeed;
            float alpha = Mathf.Lerp(highlightPulseMinAlpha, highlightPulseMaxAlpha, (Mathf.Sin(time) + 1f) / 2f);
            Color color = baseColor;
            color.a = alpha;
            image.color = color;
            yield return null;
        }
    }
    
    /// <summary>
    /// Setup click target for step completion
    /// </summary>
    private HashSet<GameObject> clickedTargets = new HashSet<GameObject>(); // Track which targets have been clicked
    private List<GameObject> currentClickTargets = new List<GameObject>(); // Current step's click targets
    
    private void SetupClickTarget(TutorialStep step)
    {
        clickedTargets.Clear();
        currentClickTargets.Clear();
        
        // Find click target(s) by path - support comma-separated paths
        string pathToFind = !string.IsNullOrEmpty(step.clickTargetPath) 
            ? step.clickTargetPath 
            : step.targetObjectPath; // Fall back to target object if no click target specified
        
        if (!string.IsNullOrEmpty(pathToFind))
        {
            // Check if path contains commas (multiple targets)
            if (pathToFind.Contains(","))
            {
                // Split by comma and find each target
                string[] paths = pathToFind.Split(',');
                foreach (string path in paths)
                {
                    string trimmedPath = path.Trim();
                    if (!string.IsNullOrEmpty(trimmedPath))
                    {
                        GameObject target = FindGameObjectByPath(trimmedPath);
                        if (target != null)
                        {
                            currentClickTargets.Add(target);
                            SetupClickHandlerForTarget(target);
                        }
                        else
                        {
                            Debug.LogWarning($"[TutorialManager] Could not find click target: {trimmedPath}");
                        }
                    }
                }
            }
            else
            {
                // Single target
                GameObject clickTarget = FindGameObjectByPath(pathToFind);
                if (clickTarget != null)
                {
                    currentClickTargets.Add(clickTarget);
                    SetupClickHandlerForTarget(clickTarget);
                }
                else
                {
                    Debug.LogWarning($"[TutorialManager] Could not find click target: {pathToFind}");
                }
            }
        }
        
        if (currentClickTargets.Count > 0)
        {
            Debug.Log($"[TutorialManager] Set up {currentClickTargets.Count} click target(s) for tutorial step. All must be clicked to proceed.");
        }
    }
    
    private void SetupClickHandlerForTarget(GameObject target)
    {
        if (target == null) return;
        
        // Add a temporary click handler
        var button = target.GetComponent<Button>();
        if (button == null)
        {
            button = target.AddComponent<Button>();
        }
        
        // IMPORTANT: Don't remove original onClick listeners!
        // Instead, add our completion handler as an additional listener
        // This preserves the button's original functionality (e.g., opening WarrantLockerGrid)
        button.onClick.AddListener(() =>
        {
            OnClickTargetClicked(target);
        });
        
        Debug.Log($"[TutorialManager] Added tutorial click handler to '{target.name}'. Original button functionality will still execute.");
    }
    
    private void OnClickTargetClicked(GameObject target)
    {
        if (target == null || !currentClickTargets.Contains(target))
            return;
        
        // Mark this target as clicked
        if (!clickedTargets.Contains(target))
        {
            clickedTargets.Add(target);
            Debug.Log($"[TutorialManager] Clicked target '{target.name}' ({clickedTargets.Count}/{currentClickTargets.Count} targets clicked)");
            
            // Visual feedback could be added here (e.g., change color, add checkmark)
        }
        
        // Check if all targets have been clicked
        if (clickedTargets.Count >= currentClickTargets.Count)
        {
            Debug.Log($"[TutorialManager] All {currentClickTargets.Count} click target(s) have been clicked. Proceeding to next step.");
            // Complete the tutorial step after a short delay to allow button's original functionality to execute
            StartCoroutine(CompleteStepAfterDelay(0.15f));
        }
    }
    
    /// <summary>
    /// Coroutine to complete the tutorial step after a delay
    /// This allows button's original functionality (e.g., DOTween animations) to start before completing
    /// </summary>
    private IEnumerator CompleteStepAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteCurrentStep();
    }
    
    /// <summary>
    /// Find a GameObject by path (reusable method)
    /// Supports:
    /// - GameObject name: "MyObject"
    /// - Hierarchical path: "Canvas/Panel/Button"
    /// - Component property: "WarrantNodeView:Anchor" (finds WarrantNodeView with NodeId="Anchor")
    /// </summary>
    private GameObject FindGameObjectByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        
        // Check if this is a component property search (format: "ComponentName:PropertyValue")
        if (path.Contains(":"))
        {
            var parts = path.Split(':');
            if (parts.Length == 2)
            {
                string componentName = parts[0].Trim();
                string propertyValue = parts[1].Trim();
                
                return FindByComponentProperty(componentName, propertyValue);
            }
        }
        
        // Try to find by name first (searches entire scene)
        GameObject found = GameObject.Find(path);
        if (found != null) return found;
        
        // Try to find by path (e.g., "Canvas/Panel/Button")
        // Split by '/' and search hierarchically
        var pathParts = path.Split('/');
        if (pathParts.Length > 1)
        {
            // Start from root and traverse
            GameObject current = GameObject.Find(pathParts[0]);
            if (current != null)
            {
                for (int i = 1; i < pathParts.Length; i++)
                {
                    Transform child = current.transform.Find(pathParts[i]);
                    if (child != null)
                    {
                        current = child.gameObject;
                    }
                    else
                    {
                        // Try searching in children
                        current = FindChildByName(current.transform, pathParts[i]);
                        if (current == null) break;
                    }
                }
                if (current != null) return current;
            }
        }
        else if (pathParts.Length == 1)
        {
            // Single name - try to find by name
            found = GameObject.Find(pathParts[0]);
            if (found != null) return found;
        }
        
        // Last resort: search all GameObjects in scene (slower)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.name == path)
            {
                return obj;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Find a GameObject by component property value
    /// </summary>
    private GameObject FindByComponentProperty(string componentName, string propertyValue)
    {
        // Special handling for common component types
        switch (componentName)
        {
            case "WarrantNodeView":
            case "WarrantSocketView":
                // Find WarrantNodeView with matching NodeId
                WarrantNodeView[] nodeViews = FindObjectsOfType<WarrantNodeView>();
                foreach (var nodeView in nodeViews)
                {
                    if (nodeView.NodeId == propertyValue)
                    {
                        return nodeView.gameObject;
                    }
                }
                break;
                
            default:
                // Generic component search using reflection (slower but more flexible)
                return FindByComponentPropertyGeneric(componentName, propertyValue);
        }
        
        return null;
    }
    
    /// <summary>
    /// Generic component property search using reflection
    /// </summary>
    private GameObject FindByComponentPropertyGeneric(string componentName, string propertyValue)
    {
        // Try to find the component type
        System.Type componentType = System.Type.GetType(componentName);
        if (componentType == null)
        {
            // Try with namespace
            componentType = System.Type.GetType($"UnityEngine.{componentName}") ??
                          System.Type.GetType($"{componentName}, Assembly-CSharp");
        }
        
        if (componentType == null)
        {
            Debug.LogWarning($"[TutorialManager] Component type '{componentName}' not found.");
            return null;
        }
        
        // Find all components of this type
        // Search all GameObjects and check for component (simpler and more reliable)
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            var component = obj.GetComponent(componentType);
            if (component != null)
            {
                // Try to get a property named "NodeId", "Id", or the component name + "Id"
                var nodeIdProperty = componentType.GetProperty("NodeId") ??
                                   componentType.GetProperty("Id") ??
                                   componentType.GetProperty($"{componentName}Id");
                
                if (nodeIdProperty != null)
                {
                    var value = nodeIdProperty.GetValue(component);
                    if (value != null && value.ToString() == propertyValue)
                    {
                        return obj;
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Wait for time coroutine
    /// </summary>
    private IEnumerator WaitForTimeCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        CompleteCurrentStep();
    }
    
    /// <summary>
    /// Wait for condition coroutine
    /// </summary>
    private IEnumerator WaitForConditionCoroutine(string conditionName)
    {
        // TODO: Implement custom condition checking
        // For now, just wait a bit
        yield return new WaitForSeconds(1f);
        CompleteCurrentStep();
    }
    
    /// <summary>
    /// Complete the current step and move to next
    /// </summary>
    public void NextStep()
    {
        if (!isTutorialActive || currentStep == null)
            return;
        
        CompleteCurrentStep();
    }
    
    private void CompleteCurrentStep()
    {
        if (currentStep == null) return;
        
        // Execute onComplete action
        ExecuteAction(currentStep.onCompleteAction);
        
        // Move to next step
        currentStepIndex++;
        if (currentStepIndex < currentTutorial.steps.Count)
        {
            ShowStep(currentStepIndex);
        }
        else
        {
            EndTutorial();
        }
    }
    
    /// <summary>
    /// Clear current step visuals
    /// </summary>
    private void ClearCurrentStep()
    {
        // Stop all pulse coroutines
        if (highlightPulseCoroutine != null)
        {
            StopCoroutine(highlightPulseCoroutine);
            highlightPulseCoroutine = null;
        }
        
        foreach (Coroutine coroutine in highlightPulseCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        highlightPulseCoroutines.Clear();
        
        // Destroy all highlights
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
        
        foreach (GameObject highlight in currentHighlights)
        {
            if (highlight != null)
            {
                Destroy(highlight);
            }
        }
        currentHighlights.Clear();
    }
    
    /// <summary>
    /// End the current tutorial
    /// </summary>
    public void EndTutorial()
    {
        ClearCurrentStep();
        
        // Mark tutorial as completed in Character
        if (currentTutorial != null && !string.IsNullOrEmpty(currentTutorial.tutorialId))
        {
            var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
            if (character != null)
            {
                character.MarkTutorialCompleted(currentTutorial.tutorialId);
                CharacterManager.Instance.SaveCharacter(); // Persist the completion
                Debug.Log($"[TutorialManager] Marked tutorial '{currentTutorial.tutorialId}' as completed for character '{character.characterName}'");
            }
        }
        
        // Execute tutorial completion action if set
        if (currentTutorial != null && currentTutorial.onTutorialComplete != null)
        {
            ExecuteAction(currentTutorial.onTutorialComplete);
        }
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        currentTutorial = null;
        currentTutorialId = null;
        currentStep = null;
        currentStepIndex = -1;
        isTutorialActive = false;
        
        Debug.Log("[TutorialManager] Tutorial ended.");
    }
    
    /// <summary>
    /// Skip the current tutorial
    /// </summary>
    public void SkipTutorial()
    {
        if (currentTutorial != null && currentTutorial.canSkip)
        {
            EndTutorial();
        }
    }
    
    /// <summary>
    /// Prepare to resume tutorial after scene transition
    /// </summary>
    public void PrepareTutorialForSceneTransition(string tutorialId, int currentStep)
    {
        pendingTutorialId = tutorialId;
        currentStepIndex = currentStep;
        shouldResumeOnSceneLoad = true;
        
        Debug.Log($"[TutorialManager] Prepared tutorial '{tutorialId}' for scene transition (will resume from step {currentStep})");
        
        // Ensure TutorialPanel will be active when we resume
        // (It will be activated in OnSceneLoaded before starting the coroutine)
    }
    
    /// <summary>
    /// Execute a tutorial action
    /// </summary>
    private void ExecuteAction(TutorialAction action)
    {
        if (action == null || action.actionType == TutorialAction.ActionType.None)
            return;
        
        switch (action.actionType)
        {
            case TutorialAction.ActionType.ShowPanel:
                ShowPanelFromTutorial(action.actionValue);
                break;
                
            case TutorialAction.ActionType.HidePanel:
                var hidePanel = GameObject.Find(action.actionValue);
                if (hidePanel != null) hidePanel.SetActive(false);
                break;
                
            case TutorialAction.ActionType.EnableObject:
                var enableObj = GameObject.Find(action.actionValue);
                if (enableObj != null) enableObj.SetActive(true);
                break;
                
            case TutorialAction.ActionType.DisableObject:
                var disableObj = GameObject.Find(action.actionValue);
                if (disableObj != null) disableObj.SetActive(false);
                break;
                
            case TutorialAction.ActionType.TransitionScene:
                TransitionToScene(action.actionValue);
                break;
                
            case TutorialAction.ActionType.StartDialogue:
                StartDialogueFromTutorial(action.actionValue);
                break;
                
            case TutorialAction.ActionType.Custom:
                HandleCustomAction(action.actionValue);
                break;
        }
    }
    
    /// <summary>
    /// Transition to a scene (used by tutorial actions)
    /// </summary>
    private void TransitionToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[TutorialManager] Scene name is empty for TransitionScene action.");
            return;
        }
        
        // Use TransitionManager if available, otherwise use SceneManager directly
        TransitionManager transitionManager = FindFirstObjectByType<TransitionManager>();
        if (transitionManager != null)
        {
            transitionManager.TransitionToScene(sceneName);
            Debug.Log($"[TutorialManager] Transitioning to scene '{sceneName}' via TransitionManager.");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            Debug.Log($"[TutorialManager] Loading scene '{sceneName}' directly (no TransitionManager found).");
        }
    }
    
    /// <summary>
    /// Start a dialogue from tutorial action
    /// </summary>
    private void StartDialogueFromTutorial(string dialogueIdOrNpcId)
    {
        if (string.IsNullOrEmpty(dialogueIdOrNpcId))
        {
            Debug.LogWarning("[TutorialManager] Dialogue ID or NPC ID is empty for StartDialogue action.");
            return;
        }
        
        // Try to find DialogueManager
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogWarning("[TutorialManager] DialogueManager not found. Cannot start dialogue.");
            return;
        }
        
        // Try to find NPC by ID first
        NPCInteractable npc = FindNPCById(dialogueIdOrNpcId);
        if (npc != null)
        {
            // Use the NPC's StartDialogue method which handles dialogueData internally
            npc.StartDialogue();
            Debug.Log($"[TutorialManager] Started dialogue with NPC '{dialogueIdOrNpcId}'.");
        }
        else
        {
            // Try to load dialogue by ID from Resources
            DialogueData dialogue = Resources.Load<DialogueData>($"Dialogues/{dialogueIdOrNpcId}");
            if (dialogue != null)
            {
                dialogueManager.StartDialogue(dialogue);
                Debug.Log($"[TutorialManager] Started dialogue '{dialogueIdOrNpcId}' from Resources.");
            }
            else
            {
                Debug.LogWarning($"[TutorialManager] Could not find NPC or dialogue with ID '{dialogueIdOrNpcId}'.");
            }
        }
    }
    
    /// <summary>
    /// Find NPC by ID
    /// </summary>
    private NPCInteractable FindNPCById(string npcId)
    {
        NPCInteractable[] allNPCs = FindObjectsOfType<NPCInteractable>();
        foreach (NPCInteractable npc in allNPCs)
        {
            if (npc != null && npc.GetNPCId() == npcId)
            {
                return npc;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Show a panel from tutorial action (supports WarrantFusionPanel, WarrantLockerPanel, etc.)
    /// </summary>
    private void ShowPanelFromTutorial(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
        {
            Debug.LogWarning("[TutorialManager] Panel name is empty for ShowPanel action.");
            return;
        }
        
        Debug.Log($"[TutorialManager] ShowPanel action: {panelName}");
        
        // Handle specific panel types
        switch (panelName.ToLower())
        {
            case "warrantfusion":
            case "warrantfusionpanel":
            case "fusion":
                // Try to find WarrantFusionUI component
                WarrantFusionUI fusionUI = FindFirstObjectByType<WarrantFusionUI>();
                if (fusionUI != null)
                {
                    fusionUI.ShowPanel();
                    Debug.Log("[TutorialManager] Opened WarrantFusionPanel via WarrantFusionUI.");
                    return;
                }
                
                // Fallback: Try to find by GameObject name
                GameObject fusionPanel = GameObject.Find("WarrantFusionPanel");
                if (fusionPanel == null)
                {
                    fusionPanel = GameObject.Find("WarrantFusion");
                }
                
                if (fusionPanel != null)
                {
                    fusionPanel.SetActive(true);
                    Debug.Log("[TutorialManager] Opened WarrantFusionPanel by GameObject name.");
                    return;
                }
                
                Debug.LogWarning("[TutorialManager] Could not find WarrantFusionPanel. Make sure it exists in the scene with WarrantFusionUI component or is named 'WarrantFusionPanel'.");
                break;
            
            case "warrantlocker":
            case "locker":
                // Try to find WarrantLockerPanelManager
                WarrantLockerPanelManager lockerManager = FindFirstObjectByType<WarrantLockerPanelManager>();
                if (lockerManager != null)
                {
                    lockerManager.ShowPanel();
                    Debug.Log("[TutorialManager] Opened WarrantLockerPanel via WarrantLockerPanelManager.");
                    return;
                }
                
                // Fallback: Try to find WarrantLockerGrid directly
                WarrantLockerGrid lockerGrid = FindFirstObjectByType<WarrantLockerGrid>();
                if (lockerGrid != null && lockerGrid.gameObject != null)
                {
                    lockerGrid.gameObject.SetActive(true);
                    Debug.Log("[TutorialManager] Opened WarrantLockerPanel by activating WarrantLockerGrid.");
                    return;
                }
                
                Debug.LogWarning("[TutorialManager] Could not find WarrantLockerPanel.");
                break;
            
            default:
                // Generic: Try to find by GameObject name
                GameObject panelObj = GameObject.Find(panelName);
                if (panelObj != null)
                {
                    panelObj.SetActive(true);
                    Debug.Log($"[TutorialManager] Found and activated panel '{panelName}' by name.");
                }
                else
                {
                    Debug.LogWarning($"[TutorialManager] Could not find panel '{panelName}'. Make sure it exists in the scene.");
                }
                break;
        }
    }
    
    /// <summary>
    /// Handle custom tutorial actions
    /// </summary>
    private void HandleCustomAction(string actionValue)
    {
        if (string.IsNullOrEmpty(actionValue))
        {
            Debug.Log("[TutorialManager] Custom action with empty value.");
            return;
        }
        
        // Handle known custom actions
        switch (actionValue)
        {
            case "transition_to_warrant_tree":
                // This is handled by the dialogue system, but we can log it
                Debug.Log("[TutorialManager] Custom action: transition_to_warrant_tree (handled by dialogue system)");
                break;
                
            default:
                Debug.Log($"[TutorialManager] Custom action: {actionValue}");
                break;
        }
    }
    
    /// <summary>
    /// Load tutorial data from Resources
    /// </summary>
    private TutorialData LoadTutorialData(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
        {
            Debug.LogError("[TutorialManager] Cannot load tutorial with empty ID.");
            return null;
        }
        
        Debug.Log($"[TutorialManager] Loading tutorial '{tutorialId}' from Resources...");
        
        // Try to load from Resources/Tutorials/
        var tutorial = Resources.Load<TutorialData>($"Tutorials/{tutorialId}");
        if (tutorial == null)
        {
            Debug.LogWarning($"[TutorialManager] Tutorial not found at 'Tutorials/{tutorialId}', trying without path...");
            // Try without path
            tutorial = Resources.Load<TutorialData>(tutorialId);
        }
        
        if (tutorial == null)
        {
            Debug.LogError($"[TutorialManager] Tutorial '{tutorialId}' not found in Resources! " +
                          $"Make sure the tutorial asset is located at 'Assets/Resources/Tutorials/{tutorialId}.asset' " +
                          $"(filename must match tutorialId).");
        }
        else
        {
            Debug.Log($"[TutorialManager] Successfully loaded tutorial '{tutorialId}': {tutorial.tutorialName} ({tutorial.steps?.Count ?? 0} steps)");
        }
        
        return tutorial;
    }
    
    public bool IsTutorialActive => isTutorialActive;
    public int CurrentStepIndex => currentStepIndex;
    
    /// <summary>
    /// Activate a GameObject and all its parent GameObjects up to the root
    /// </summary>
    private void ActivateParentChain(GameObject obj)
    {
        if (obj == null) return;
        
        Transform current = obj.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }
            current = current.parent;
        }
    }
}

