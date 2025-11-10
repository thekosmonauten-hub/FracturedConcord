using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages navigation between multiple panel groups
/// Controls both Display Window panels and Dynamic Area panels simultaneously
/// Example: Equipment button shows Equipment Display + Equipment Dynamic Area
/// </summary>
public class PanelNavigationController : MonoBehaviour
{
    [System.Serializable]
    public class NavigationItem
    {
        [Tooltip("Navigation button for this panel set")]
        public Button navigationButton;
        
        [Tooltip("Panel in the Display Window (e.g., EquipmentNavDisplay)")]
        public GameObject displayPanel;
        
        [Tooltip("Panel in the Dynamic Area (e.g., Equipment child of DynamicArea)")]
        public GameObject dynamicPanel;
        
        [Header("Visual States (Optional)")]
        [Tooltip("Sprite for active/selected button state")]
        public Sprite activeButtonSprite;
        
        [Tooltip("Sprite for inactive/unselected button state")]
        public Sprite inactiveButtonSprite;
        
        [Tooltip("Color for active button (used with or without sprites)")]
        public Color activeButtonColor = Color.white;
        
        [Tooltip("Color for inactive button (used with or without sprites)")]
        public Color inactiveButtonColor = new Color(0.7f, 0.7f, 0.7f);
    }
    
    [Header("Navigation Items")]
    [SerializeField] private List<NavigationItem> navigationItems = new List<NavigationItem>();
    
    [Header("Settings")]
    [SerializeField] private int startingIndex = 0;
    [SerializeField] private bool useSpriteSwapping = false;
    
    [Header("Optional Transitions")]
    [SerializeField] private bool enableFadeTransitions = false;
    [SerializeField] private float fadeDuration = 0.2f;
    
    private int currentIndex = 0;
    
    void Start()
    {
        SetupNavigation();
        
        // Show starting panel
        if (startingIndex >= 0 && startingIndex < navigationItems.Count)
        {
            SwitchToPanel(startingIndex, false); // No animation on start
        }
    }
    
    void SetupNavigation()
    {
        for (int i = 0; i < navigationItems.Count; i++)
        {
            NavigationItem item = navigationItems[i];
            
            if (item.navigationButton == null)
            {
                Debug.LogWarning($"[PanelNavigationController] Navigation item {i} has no button assigned!");
                continue;
            }
            
            // Add click listener
            int capturedIndex = i;
            item.navigationButton.onClick.AddListener(() => SwitchToPanel(capturedIndex));
            
            // Set initial button visuals
            UpdateButtonVisuals(item, false);
        }
        
        Debug.Log($"[PanelNavigationController] Setup {navigationItems.Count} navigation items");
    }
    
    public void SwitchToPanel(int index, bool animate = true)
    {
        if (index < 0 || index >= navigationItems.Count)
        {
            Debug.LogWarning($"[PanelNavigationController] Invalid panel index: {index}");
            return;
        }
        
        if (index == currentIndex)
        {
            Debug.Log($"[PanelNavigationController] Already on panel {index}");
            return;
        }
        
        NavigationItem previousItem = navigationItems[currentIndex];
        NavigationItem newItem = navigationItems[index];
        
        // Deactivate previous panels
        if (animate && enableFadeTransitions)
        {
            // Fade out previous, fade in new
            if (previousItem.displayPanel != null)
                StartCoroutine(FadeOutPanel(previousItem.displayPanel));
            if (previousItem.dynamicPanel != null)
                StartCoroutine(FadeOutPanel(previousItem.dynamicPanel));
            
            if (newItem.displayPanel != null)
                StartCoroutine(FadeInPanel(newItem.displayPanel));
            if (newItem.dynamicPanel != null)
                StartCoroutine(FadeInPanel(newItem.dynamicPanel));
        }
        else
        {
            // Instant switch
            if (previousItem.displayPanel != null)
                previousItem.displayPanel.SetActive(false);
            if (previousItem.dynamicPanel != null)
                previousItem.dynamicPanel.SetActive(false);
            
            if (newItem.displayPanel != null)
                newItem.displayPanel.SetActive(true);
            if (newItem.dynamicPanel != null)
                newItem.dynamicPanel.SetActive(true);
        }
        
        // Update button visuals
        UpdateButtonVisuals(previousItem, false);
        UpdateButtonVisuals(newItem, true);
        
        currentIndex = index;
        
        Debug.Log($"[PanelNavigationController] Switched to panel: {index}");
    }
    
    void UpdateButtonVisuals(NavigationItem item, bool isActive)
    {
        if (item.navigationButton == null) return;
        
        Image buttonImage = item.navigationButton.GetComponent<Image>();
        
        // Sprite swapping (if enabled and sprites are assigned)
        if (useSpriteSwapping && buttonImage != null)
        {
            if (isActive && item.activeButtonSprite != null)
            {
                buttonImage.sprite = item.activeButtonSprite;
            }
            else if (!isActive && item.inactiveButtonSprite != null)
            {
                buttonImage.sprite = item.inactiveButtonSprite;
            }
        }
        
        // Color tinting (always applied)
        if (buttonImage != null)
        {
            buttonImage.color = isActive ? item.activeButtonColor : item.inactiveButtonColor;
        }
        else
        {
            // Fallback to ColorBlock if no Image component
            ColorBlock colors = item.navigationButton.colors;
            colors.normalColor = isActive ? item.activeButtonColor : item.inactiveButtonColor;
            item.navigationButton.colors = colors;
        }
    }
    
    System.Collections.IEnumerator FadeOutPanel(GameObject panel)
    {
        if (panel == null) yield break;
        
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        panel.SetActive(false);
    }
    
    System.Collections.IEnumerator FadeInPanel(GameObject panel)
    {
        if (panel == null) yield break;
        
        panel.SetActive(true);
        
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Get the currently active navigation index
    /// </summary>
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    
    /// <summary>
    /// Get the currently active navigation item
    /// </summary>
    public NavigationItem GetCurrentItem()
    {
        if (currentIndex >= 0 && currentIndex < navigationItems.Count)
            return navigationItems[currentIndex];
        return null;
    }
    
    /// <summary>
    /// Switch to next panel (cycles)
    /// </summary>
    public void NextPanel()
    {
        int nextIndex = (currentIndex + 1) % navigationItems.Count;
        SwitchToPanel(nextIndex);
    }
    
    /// <summary>
    /// Switch to previous panel (cycles)
    /// </summary>
    public void PreviousPanel()
    {
        int prevIndex = currentIndex - 1;
        if (prevIndex < 0)
            prevIndex = navigationItems.Count - 1;
        SwitchToPanel(prevIndex);
    }
    
    /// <summary>
    /// Programmatically add a navigation item at runtime
    /// </summary>
    public void AddNavigationItem(Button button, GameObject displayPanel, GameObject dynamicPanel)
    {
        NavigationItem newItem = new NavigationItem
        {
            navigationButton = button,
            displayPanel = displayPanel,
            dynamicPanel = dynamicPanel
        };
        
        navigationItems.Add(newItem);
        
        // Setup the new button
        int index = navigationItems.Count - 1;
        button.onClick.AddListener(() => SwitchToPanel(index));
        UpdateButtonVisuals(newItem, false);
        
        Debug.Log($"[PanelNavigationController] Added navigation item at index {index}");
    }
    
    /// <summary>
    /// Remove all click listeners (useful for cleanup)
    /// </summary>
    void OnDestroy()
    {
        foreach (var item in navigationItems)
        {
            if (item.navigationButton != null)
            {
                item.navigationButton.onClick.RemoveAllListeners();
            }
        }
    }
}

