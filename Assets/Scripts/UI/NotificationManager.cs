using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Singleton manager for displaying notifications/popups to the player
/// </summary>
public class NotificationManager : MonoBehaviour
{
    private static NotificationManager _instance;
    public static NotificationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<NotificationManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("NotificationManager");
                    _instance = go.AddComponent<NotificationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Notification UI")]
    [SerializeField] private GameObject notificationPanelPrefab; // Prefab to instantiate (recommended)
    [SerializeField] private GameObject notificationPanel; // Direct reference (if not using prefab)
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    
    [Header("Settings")]
    [SerializeField] private float autoCloseDelay = 3f; // Auto-close after this many seconds (0 = no auto-close)
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    private CanvasGroup panelCanvasGroup;
    private Coroutine autoCloseCoroutine;
    private Coroutine fadeCoroutine;
    private static GameObject notificationCanvasInstance; // Track the singleton canvas instance
    private bool isInitialized = false;
    private static bool canvasCreated = false; // Track if canvas has been created
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure we have a Canvas - but check if one already exists first
            if (notificationPanel != null)
            {
                Canvas canvas = notificationPanel.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    // Check if NotificationCanvas already exists in the scene (prevent duplicates)
                    if (notificationCanvasInstance == null && !canvasCreated)
                    {
                        // First, search for existing NotificationCanvas in the scene
                        GameObject existingCanvas = GameObject.Find("NotificationCanvas");
                        if (existingCanvas == null)
                        {
                            // Also search using FindObjectsByType to catch inactive ones
                            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                            foreach (Canvas c in allCanvases)
                            {
                                if (c.gameObject.name == "NotificationCanvas")
                                {
                                    existingCanvas = c.gameObject;
                                    break;
                                }
                            }
                        }
                        
                        if (existingCanvas != null)
                        {
                            notificationCanvasInstance = existingCanvas;
                            Canvas existingCanvasComponent = existingCanvas.GetComponent<Canvas>();
                            if (existingCanvasComponent != null)
                            {
                                // Use existing canvas
                                notificationPanel.transform.SetParent(existingCanvas.transform, false);
                                DontDestroyOnLoad(existingCanvas);
                                canvasCreated = true;
                                Debug.Log("[NotificationManager] Found existing NotificationCanvas, reusing it.");
                            }
                        }
                        else if (!canvasCreated)
                        {
                            // Create a Canvas if none exists (only once)
                            GameObject canvasObj = new GameObject("NotificationCanvas");
                            Canvas newCanvas = canvasObj.AddComponent<Canvas>();
                            newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                            newCanvas.sortingOrder = 9999; // Very high to appear on top
                            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                            
                            notificationPanel.transform.SetParent(canvasObj.transform, false);
                            DontDestroyOnLoad(canvasObj);
                            notificationCanvasInstance = canvasObj;
                            canvasCreated = true;
                            Debug.Log("[NotificationManager] Created new NotificationCanvas.");
                        }
                    }
                    else if (notificationCanvasInstance != null)
                    {
                        // Canvas instance already exists, use it
                        notificationPanel.transform.SetParent(notificationCanvasInstance.transform, false);
                        Debug.Log("[NotificationManager] Reusing existing NotificationCanvas instance.");
                    }
                    else if (canvasCreated)
                    {
                        // Canvas was created by another instance, find it
                        GameObject existingCanvas = GameObject.Find("NotificationCanvas");
                        if (existingCanvas != null)
                        {
                            notificationCanvasInstance = existingCanvas;
                            notificationPanel.transform.SetParent(existingCanvas.transform, false);
                            Debug.Log("[NotificationManager] Canvas was already created, found and reusing it.");
                        }
                    }
                }
                else
                {
                    // Panel already has a canvas, track it
                    notificationCanvasInstance = canvas.gameObject;
                    DontDestroyOnLoad(canvas.gameObject);
                }
                
                panelCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                }
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Only initialize UI once
        if (!isInitialized)
        {
            InitializeUI();
            isInitialized = true;
        }
    }
    
    private void InitializeUI()
    {
        // Prevent multiple initializations
        if (isInitialized)
        {
            Debug.Log("[NotificationManager] InitializeUI called but already initialized. Skipping.");
            return;
        }
        
        // PRIORITY: If prefab is assigned, always use it (even if notificationPanel is already assigned)
        // This ensures the prefab takes precedence over any scene-based panel
        if (notificationPanelPrefab != null)
        {
            Debug.Log("[NotificationManager] Prefab assigned - instantiating NotificationPanel from prefab (overriding any scene-based panel)...");
            
            // If there's an existing panel reference, clear it so we use the prefab instead
            if (notificationPanel != null)
            {
                Debug.LogWarning($"[NotificationManager] Found existing NotificationPanel in scene ('{notificationPanel.name}'), but prefab is assigned. Using prefab instead. Clear the 'Notification Panel' field in Inspector if you want to use the prefab exclusively.");
            }
            
            notificationPanel = Instantiate(notificationPanelPrefab);
            notificationPanel.name = "NotificationPanel"; // Remove (Clone) suffix
            
            // Ensure it's under the NotificationCanvas
            GameObject canvasObj = GetOrCreateNotificationCanvas();
            notificationPanel.transform.SetParent(canvasObj.transform, false);
            
            // Get UI references from the instantiated prefab
            titleText = notificationPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (titleText != null && titleText.name.ToLower().Contains("title"))
            {
                Debug.Log($"[NotificationManager] Found TitleText in prefab: {titleText.name}");
            }
            else
            {
                // Try to find by searching all text components
                TextMeshProUGUI[] allTexts = notificationPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var text in allTexts)
                {
                    if (text.name.ToLower().Contains("title"))
                    {
                        titleText = text;
                        Debug.Log($"[NotificationManager] Found TitleText by name: {text.name}");
                        break;
                    }
                }
            }
            
            messageText = notificationPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (messageText != null && messageText != titleText && (messageText.name.ToLower().Contains("message") || messageText.name.ToLower().Contains("body")))
            {
                Debug.Log($"[NotificationManager] Found MessageText in prefab: {messageText.name}");
            }
            else
            {
                TextMeshProUGUI[] allTexts = notificationPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var text in allTexts)
                {
                    if (text != titleText && (text.name.ToLower().Contains("message") || text.name.ToLower().Contains("body")))
                    {
                        messageText = text;
                        Debug.Log($"[NotificationManager] Found MessageText by name: {text.name}");
                        break;
                    }
                }
            }
            
            closeButton = notificationPanel.GetComponentInChildren<Button>(true);
            if (closeButton != null)
            {
                Debug.Log($"[NotificationManager] Found CloseButton in prefab: {closeButton.name}");
            }
            
            Debug.Log($"[NotificationManager] Successfully instantiated NotificationPanel from prefab.");
        }
        // Auto-find NotificationPanel if not assigned and no prefab
        else if (notificationPanel == null)
        {
            Debug.Log("[NotificationManager] notificationPanel not assigned, searching for it in scene...");
            notificationPanel = GameObject.Find("NotificationPanel");
            if (notificationPanel == null)
            {
                // Try case-insensitive search
                GameObject[] allPanels = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allPanels)
                {
                    if (obj.name.ToLower().Contains("notificationpanel"))
                    {
                        notificationPanel = obj;
                        Debug.Log($"[NotificationManager] Found NotificationPanel by name search: {obj.name}");
                        break;
                    }
                }
            }
            
            if (notificationPanel == null)
            {
                Debug.LogWarning("[NotificationManager] NotificationPanel not found in scene. Creating a basic notification panel...");
                CreateNotificationPanel();
            }
            else
            {
                Debug.Log($"[NotificationManager] Auto-found NotificationPanel: {notificationPanel.name}");
            }
        }
        
        // Ensure NotificationPanel has Canvas
        if (notificationPanel != null)
        {
            Canvas canvas = notificationPanel.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[NotificationManager] NotificationPanel is not under a Canvas. Searching for NotificationCanvas...");
                
                // Check if we already have a tracked canvas instance
                if (notificationCanvasInstance != null && notificationCanvasInstance.activeInHierarchy)
                {
                    // Use the tracked instance
                    notificationPanel.transform.SetParent(notificationCanvasInstance.transform, false);
                    canvas = notificationCanvasInstance.GetComponent<Canvas>();
                    Debug.Log($"[NotificationManager] Using tracked NotificationCanvas instance: {notificationCanvasInstance.name}");
                }
                else
                {
                    // Search for existing NotificationCanvas in scene
                    GameObject[] allCanvases = GameObject.FindGameObjectsWithTag("Untagged"); // Find all objects
                    GameObject canvasObj = null;
                    foreach (GameObject obj in allCanvases)
                    {
                        if (obj.name == "NotificationCanvas")
                        {
                            canvasObj = obj;
                            break;
                        }
                    }
                    
                    // Also try FindFirstObjectByType as fallback
                    if (canvasObj == null)
                    {
                        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                        foreach (Canvas c in canvases)
                        {
                            if (c.gameObject.name == "NotificationCanvas")
                            {
                                canvasObj = c.gameObject;
                                break;
                            }
                        }
                    }
                    
                    if (canvasObj != null)
                    {
                        Canvas foundCanvas = canvasObj.GetComponent<Canvas>();
                        if (foundCanvas != null)
                        {
                            notificationCanvasInstance = canvasObj;
                            notificationPanel.transform.SetParent(canvasObj.transform, false);
                            canvas = foundCanvas;
                            DontDestroyOnLoad(canvasObj);
                            Debug.Log($"[NotificationManager] Found existing NotificationCanvas: {canvasObj.name}, reusing it.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[NotificationManager] NotificationCanvas not found. Canvas should have been created in Awake().");
                    }
                }
            }
            
            // Update canvas reference if we found one
            if (canvas != null)
            {
                notificationCanvasInstance = canvas.gameObject;
                Debug.Log($"[NotificationManager] NotificationPanel is under Canvas: {canvas.name}, sort order: {canvas.sortingOrder}");
                // Ensure canvas is active and has high sort order
                if (!canvas.gameObject.activeSelf)
                {
                    canvas.gameObject.SetActive(true);
                    Debug.Log($"[NotificationManager] Activated Canvas: {canvas.name}");
                }
                if (canvas.sortingOrder < 9999)
                {
                    canvas.sortingOrder = 9999;
                    Debug.Log($"[NotificationManager] Set Canvas '{canvas.name}' sort order to 9999");
                }
            }
            
            // Ensure CanvasGroup exists
            panelCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                Debug.Log("[NotificationManager] Added CanvasGroup to NotificationPanel");
            }
        }
        
        // Auto-find text elements if not assigned
        if (notificationPanel != null)
        {
            if (titleText == null)
            {
                titleText = notificationPanel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (titleText != null && titleText.name.ToLower().Contains("title"))
                {
                    Debug.Log($"[NotificationManager] Auto-found TitleText: {titleText.name}");
                }
                else
                {
                    // Search for any text with "title" in name
                    TextMeshProUGUI[] allTexts = notificationPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var text in allTexts)
                    {
                        if (text.name.ToLower().Contains("title"))
                        {
                            titleText = text;
                            Debug.Log($"[NotificationManager] Auto-found TitleText by name: {text.name}");
                            break;
                        }
                    }
                }
            }
            
            if (messageText == null)
            {
                TextMeshProUGUI[] allTexts = notificationPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var text in allTexts)
                {
                    if (text != titleText && (text.name.ToLower().Contains("message") || text.name.ToLower().Contains("body")))
                    {
                        messageText = text;
                        Debug.Log($"[NotificationManager] Auto-found MessageText: {text.name}");
                        break;
                    }
                }
            }
            
            if (closeButton == null)
            {
                closeButton = notificationPanel.GetComponentInChildren<Button>(true);
                if (closeButton != null)
                {
                    Debug.Log($"[NotificationManager] Auto-found CloseButton: {closeButton.name}");
                }
            }
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseNotification);
        }
        
        // Hide notification panel initially
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
            }
            Debug.Log("[NotificationManager] NotificationPanel initialized and hidden");
        }
    }
    
    /// <summary>
    /// Show a notification popup with title and message
    /// </summary>
    public void ShowNotification(string title, string message, bool autoClose = true)
    {
        Debug.Log($"[NotificationManager] ShowNotification called: title='{title}', message='{message}', autoClose={autoClose}");
        
        // Ensure we're initialized before showing notifications
        if (!isInitialized)
        {
            InitializeUI();
            isInitialized = true;
        }
        
        if (notificationPanel == null)
        {
            Debug.LogWarning("[NotificationManager] Notification panel is not assigned. Cannot show notification. " +
                           "Please assign the notificationPanel GameObject in the Inspector.");
            return;
        }
        
        Debug.Log($"[NotificationManager] Notification panel found: {notificationPanel.name}, active: {notificationPanel.activeSelf}");
        
        // Stop any existing auto-close or fade coroutines
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // Update text
        if (titleText != null)
        {
            titleText.text = title ?? "Notification";
        }
        
        if (messageText != null)
        {
            messageText.text = message ?? "";
        }
        
        // Ensure Canvas is active
        Canvas canvas = notificationPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (!canvas.gameObject.activeSelf)
            {
                canvas.gameObject.SetActive(true);
                Debug.Log($"[NotificationManager] Activated Canvas: {canvas.name}");
            }
            
            // Ensure high sort order
            if (canvas.sortingOrder < 9999)
            {
                canvas.sortingOrder = 9999;
                Debug.Log($"[NotificationManager] Set Canvas '{canvas.name}' sort order to 9999");
            }
        }
        
        // Ensure parent chain is active
        ActivateParentChain(notificationPanel);
        
        // Show the panel
        notificationPanel.SetActive(true);
        
        Debug.Log($"[NotificationManager] NotificationPanel activated: activeSelf={notificationPanel.activeSelf}, activeInHierarchy={notificationPanel.activeInHierarchy}");
        
        // Fade in
        if (panelCanvasGroup != null)
        {
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        
        // Auto-close if enabled
        if (autoClose && autoCloseDelay > 0)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }
    
    private static string lastNotificationKey = ""; // Track last notification to prevent duplicates
    private static float lastNotificationTime = 0f;
    private const float NOTIFICATION_COOLDOWN = 2f; // Prevent duplicate notifications within 2 seconds (increased from 0.5s)
    
    /// <summary>
    /// Show a notification for a warrant pack reward
    /// </summary>
    public void ShowWarrantPackNotification(string packName, int warrantCount)
    {
        Debug.Log($"[NotificationManager] ShowWarrantPackNotification called: packName='{packName}', count={warrantCount}");
        
        // Create a unique key for this notification to prevent duplicates
        string notificationKey = $"warrant_{packName}_{warrantCount}";
        float currentTime = Time.time;
        
        // Prevent duplicate notifications (same notification within cooldown period)
        // Only block if it's the EXACT same notification within the cooldown window
        if (!string.IsNullOrEmpty(lastNotificationKey) && 
            lastNotificationKey == notificationKey && 
            (currentTime - lastNotificationTime) < NOTIFICATION_COOLDOWN)
        {
            Debug.LogWarning($"[NotificationManager] Duplicate warrant notification prevented: '{packName}' (count: {warrantCount}). Last shown {currentTime - lastNotificationTime:F2}s ago.");
            return;
        }
        
        lastNotificationKey = notificationKey;
        lastNotificationTime = currentTime;
        
        string title = "Warrants Received!";
        string message = $"You received {warrantCount} warrant{(warrantCount != 1 ? "s" : "")} from {packName}!";
        Debug.Log($"[NotificationManager] Showing notification: '{title}' - '{message}'");
        
        // Ensure notification panel exists before showing
        if (notificationPanel == null)
        {
            Debug.LogWarning("[NotificationManager] notificationPanel is null in ShowWarrantPackNotification. Attempting to initialize...");
            if (!isInitialized)
            {
                InitializeUI();
                isInitialized = true;
            }
            
            if (notificationPanel == null)
            {
                Debug.LogError("[NotificationManager] notificationPanel is still null after initialization. Cannot show notification.");
                return;
            }
        }
        
        ShowNotification(title, message, autoClose: true);
    }
    
    /// <summary>
    /// Close the notification
    /// </summary>
    public void CloseNotification()
    {
        if (notificationPanel == null || !notificationPanel.activeSelf)
            return;
        
        // Stop auto-close
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // Fade out and hide
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeOutAndHide());
    }
    
    private IEnumerator FadeIn()
    {
        if (panelCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        panelCanvasGroup.alpha = 1f;
        fadeCoroutine = null;
    }
    
    private IEnumerator FadeOutAndHide()
    {
        if (panelCanvasGroup == null)
        {
            notificationPanel.SetActive(false);
            yield break;
        }
        
        float elapsed = 0f;
        float startAlpha = panelCanvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 0f;
        notificationPanel.SetActive(false);
        fadeCoroutine = null;
    }
    
    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseNotification();
        autoCloseCoroutine = null;
    }
    
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
                Debug.Log($"[NotificationManager] Activated parent: {current.name}");
            }
            current = current.parent;
        }
    }
    
    /// <summary>
    /// Get or create the NotificationCanvas
    /// </summary>
    private GameObject GetOrCreateNotificationCanvas()
    {
        GameObject canvasObj = null;
        if (notificationCanvasInstance != null)
        {
            canvasObj = notificationCanvasInstance;
        }
        else
        {
            canvasObj = GameObject.Find("NotificationCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("NotificationCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                DontDestroyOnLoad(canvasObj);
                notificationCanvasInstance = canvasObj;
                canvasCreated = true;
                Debug.Log("[NotificationManager] Created NotificationCanvas for notification panel.");
            }
            else
            {
                notificationCanvasInstance = canvasObj;
            }
        }
        return canvasObj;
    }
    
    /// <summary>
    /// Create a basic notification panel if one doesn't exist
    /// </summary>
    private void CreateNotificationPanel()
    {
        // Ensure we have a canvas first
        GameObject canvasObj = GetOrCreateNotificationCanvas();
        
        // Create the notification panel
        notificationPanel = new GameObject("NotificationPanel");
        notificationPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = notificationPanel.AddComponent<RectTransform>();
        // Center the panel on screen with proper size
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600, 200); // Fixed size: 600x200 pixels
        panelRect.anchoredPosition = Vector2.zero; // Center on screen
        
        // Add background
        UnityEngine.UI.Image bgImage = notificationPanel.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Add CanvasGroup for fade effects
        panelCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        panelCanvasGroup.alpha = 0f;
        
        // Create title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(notificationPanel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.6f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(10, 10);
        titleRect.offsetMax = new Vector2(-10, -10);
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Notification";
        titleText.fontSize = 28;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = TMPro.FontStyles.Bold;
        titleText.enableWordWrapping = true;
        
        // Create message text
        GameObject messageObj = new GameObject("MessageText");
        messageObj.transform.SetParent(notificationPanel.transform, false);
        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0f, 0.25f);
        messageRect.anchorMax = new Vector2(1f, 0.6f);
        messageRect.offsetMin = new Vector2(10, 10);
        messageRect.offsetMax = new Vector2(-10, -10);
        messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "";
        messageText.fontSize = 20;
        messageText.color = Color.white;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.enableWordWrapping = true;
        
        // Create close button
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(notificationPanel.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.7f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0.25f);
        buttonRect.offsetMin = new Vector2(10, 10);
        buttonRect.offsetMax = new Vector2(-10, -10);
        closeButton = buttonObj.AddComponent<Button>();
        UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f);
        
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Close";
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        closeButton.onClick.AddListener(CloseNotification);
        
        notificationPanel.SetActive(false);
        Debug.Log("[NotificationManager] Created basic NotificationPanel with UI elements.");
    }
}

