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
    [SerializeField] private GameObject notificationPanel;
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
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure we have a Canvas
            if (notificationPanel != null)
            {
                Canvas canvas = notificationPanel.GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    // Create a Canvas if none exists
                    GameObject canvasObj = new GameObject("NotificationCanvas");
                    Canvas newCanvas = canvasObj.AddComponent<Canvas>();
                    newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    newCanvas.sortingOrder = 9999; // Very high to appear on top
                    canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    
                    notificationPanel.transform.SetParent(canvasObj.transform, false);
                    DontDestroyOnLoad(canvasObj);
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
        
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        // Auto-find NotificationPanel if not assigned
        if (notificationPanel == null)
        {
            Debug.Log("[NotificationManager] notificationPanel not assigned, searching for it in scene...");
            notificationPanel = GameObject.Find("NotificationPanel");
            if (notificationPanel == null)
            {
                // Try case-insensitive search
                GameObject[] allPanels = FindObjectsOfType<GameObject>();
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
                Debug.LogWarning("[NotificationManager] NotificationPanel not found in scene. Please assign it in the Inspector or create a GameObject named 'NotificationPanel'.");
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
                GameObject canvasObj = GameObject.Find("NotificationCanvas");
                if (canvasObj != null)
                {
                    Canvas foundCanvas = canvasObj.GetComponent<Canvas>();
                    if (foundCanvas != null)
                    {
                        Debug.Log($"[NotificationManager] Found NotificationCanvas: {canvasObj.name}");
                        // Ensure canvas is active
                        if (!canvasObj.activeSelf)
                        {
                            canvasObj.SetActive(true);
                        }
                        // Ensure high sort order
                        if (foundCanvas.sortingOrder < 9999)
                        {
                            foundCanvas.sortingOrder = 9999;
                            Debug.Log("[NotificationManager] Set NotificationCanvas sort order to 9999");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[NotificationManager] NotificationCanvas not found. Notification may not be visible.");
                }
            }
            else
            {
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
    
    /// <summary>
    /// Show a notification for a warrant pack reward
    /// </summary>
    public void ShowWarrantPackNotification(string packName, int warrantCount)
    {
        Debug.Log($"[NotificationManager] ShowWarrantPackNotification called: packName='{packName}', count={warrantCount}");
        string title = "Warrants Received!";
        string message = $"You received {warrantCount} warrant{(warrantCount != 1 ? "s" : "")} from {packName}!";
        Debug.Log($"[NotificationManager] Showing notification: '{title}' - '{message}'");
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
}

