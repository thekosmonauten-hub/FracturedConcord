using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace PassiveTree.UI
{
    /// <summary>
    /// Helper script to automatically create and setup the Stats Summary Panel
    /// </summary>
    public class StatsSummaryPanelSetupHelper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool createOnRightSide = true;
        [SerializeField] private Vector2 panelSize = new Vector2(300, 600);
        [SerializeField] private Vector2 buttonSize = new Vector2(120, 40);
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        [Header("Runtime Behavior")]
        [SerializeField] private bool destroyAfterSetup = false;
        [SerializeField] private bool hideAfterSetup = true;

        [ContextMenu("Setup Stats Summary Panel")]
        public void SetupStatsSummaryPanel()
        {
            // 1. Create the main Canvas for the Stats Panel
            GameObject canvasGO = new GameObject("StatsSummaryCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // Higher than tooltip
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
            canvasGO.transform.SetParent(transform); // Parent to this helper for organization

            // 2. Create the Stats Summary Panel component
            GameObject statsPanelGO = new GameObject("StatsSummaryPanel");
            statsPanelGO.transform.SetParent(canvasGO.transform);
            RectTransform statsRect = statsPanelGO.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(1, 1);
            statsRect.anchorMax = new Vector2(1, 1);
            statsRect.pivot = new Vector2(1, 1);
            statsRect.sizeDelta = panelSize;
            statsRect.anchoredPosition = new Vector2(-10, -10); // Top-right corner
            StatsSummaryPanel statsPanel = statsPanelGO.AddComponent<StatsSummaryPanel>();

            // Add background image
            Image panelImage = statsPanelGO.AddComponent<Image>();
            panelImage.color = panelColor;

            // 3. Create Toggle Button
            GameObject toggleButtonGO = new GameObject("ToggleButton");
            toggleButtonGO.transform.SetParent(canvasGO.transform);
            RectTransform toggleRect = toggleButtonGO.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1, 1);
            toggleRect.anchorMax = new Vector2(1, 1);
            toggleRect.pivot = new Vector2(1, 1);
            toggleRect.sizeDelta = buttonSize;
            toggleRect.anchoredPosition = new Vector2(-10, -panelSize.y - 20); // Below the panel
            Button toggleButton = toggleButtonGO.AddComponent<Button>();
            Image buttonImage = toggleButtonGO.AddComponent<Image>();
            buttonImage.color = buttonColor;
            statsPanel.toggleButton = toggleButton;

            // Add button text
            GameObject buttonTextGO = new GameObject("Text (TMP)");
            buttonTextGO.transform.SetParent(toggleButtonGO.transform);
            RectTransform buttonTextRect = buttonTextGO.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Show Stats";
            buttonText.color = Color.white;
            buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            statsPanel.toggleButtonText = buttonText;

            // 4. Create Stats Panel Content
            GameObject statsContentGO = new GameObject("StatsContent");
            statsContentGO.transform.SetParent(statsPanelGO.transform);
            RectTransform contentRect = statsContentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(10, 10);
            contentRect.offsetMax = new Vector2(-10, -10);

            // Add ScrollRect
            ScrollRect scrollRect = statsContentGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            statsPanel.statsScrollRect = scrollRect;

            // Add Content for ScrollRect
            GameObject scrollContentGO = new GameObject("Content");
            scrollContentGO.transform.SetParent(statsContentGO.transform);
            RectTransform scrollContentRect = scrollContentGO.AddComponent<RectTransform>();
            scrollContentRect.anchorMin = new Vector2(0, 1);
            scrollContentRect.anchorMax = new Vector2(1, 1);
            scrollContentRect.pivot = new Vector2(0.5f, 1);
            scrollContentRect.sizeDelta = new Vector2(0, 0);
            scrollRect.content = scrollContentRect;

            // Add ContentSizeFitter
            ContentSizeFitter contentFitter = scrollContentGO.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add VerticalLayoutGroup
            VerticalLayoutGroup layoutGroup = scrollContentGO.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.spacing = 5;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);

            // 5. Create Title Text
            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(scrollContentGO.transform);
            RectTransform titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 30);
            TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "Passive Tree Stats";
            titleText.color = Color.white;
            titleText.fontSize = 24;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            statsPanel.titleText = titleText;

            // 6. Create Stats Content Text
            GameObject statsTextGO = new GameObject("StatsText");
            statsTextGO.transform.SetParent(scrollContentGO.transform);
            RectTransform statsTextRect = statsTextGO.AddComponent<RectTransform>();
            statsTextRect.sizeDelta = new Vector2(0, 500); // Will be adjusted by ContentSizeFitter
            TextMeshProUGUI statsText = statsTextGO.AddComponent<TextMeshProUGUI>();
            statsText.text = "No stats allocated";
            statsText.color = Color.white;
            statsText.fontSize = 18;
            statsText.alignment = TextAlignmentOptions.Left;
            statsText.enableWordWrapping = true;
            statsText.overflowMode = TextOverflowModes.Overflow;
            statsPanel.statsContentText = statsText;

            // 7. Assign the main panel to the component
            statsPanel.statsPanel = statsPanelGO;

            // 8. Initially hide the panel but keep it active
            statsPanelGO.SetActive(false);
            
            // 9. Ensure the panel persists and is properly configured
            // Make sure the panel is not destroyed on load
            DontDestroyOnLoad(statsPanelGO);
            
            // Ensure the StatsSummaryPanel component is properly initialized
            if (statsPanel != null)
            {
                // Force initialization
                statsPanel.enabled = true;
                statsPanel.gameObject.SetActive(true);
                statsPanel.gameObject.SetActive(false); // Hide but keep active
            }

            // 9. Handle runtime behavior
            if (hideAfterSetup)
            {
                gameObject.SetActive(false);
                Debug.Log("[StatsSummaryPanelSetupHelper] Helper hidden after setup");
            }
            
            if (destroyAfterSetup)
            {
                DestroyImmediate(gameObject);
                Debug.Log("[StatsSummaryPanelSetupHelper] Helper destroyed after setup");
            }

            Debug.Log("[StatsSummaryPanelSetupHelper] ✅ Stats Summary Panel Setup Complete!");
            Debug.Log("[StatsSummaryPanelSetupHelper] Panel created in top-right corner with toggle button");
        }

        /// <summary>
        /// Setup stats panel without destroying the helper
        /// </summary>
        [ContextMenu("Setup Persistent Stats Panel")]
        public void SetupPersistentStatsPanel()
        {
            // Temporarily disable destruction
            bool originalDestroyAfterSetup = destroyAfterSetup;
            bool originalHideAfterSetup = hideAfterSetup;
            
            destroyAfterSetup = false;
            hideAfterSetup = false;
            
            // Run the setup
            SetupStatsSummaryPanel();
            
            // Restore original settings
            destroyAfterSetup = originalDestroyAfterSetup;
            hideAfterSetup = originalHideAfterSetup;
            
            Debug.Log("[StatsSummaryPanelSetupHelper] ✅ Persistent Stats Panel Setup Complete!");
        }

        /// <summary>
        /// Test the stats panel with sample data
        /// </summary>
        [ContextMenu("Test Stats Panel")]
        public void TestStatsPanel()
        {
            StatsSummaryPanel statsPanel = FindObjectOfType<StatsSummaryPanel>();
            if (statsPanel != null)
            {
                statsPanel.ShowPanel();
                statsPanel.TestStatsDisplay();
                Debug.Log("[StatsSummaryPanelSetupHelper] ✅ Testing stats panel with sample data");
            }
            else
            {
                Debug.LogWarning("[StatsSummaryPanelSetupHelper] No StatsSummaryPanel found. Run 'Setup Stats Summary Panel' first.");
            }
        }

        /// <summary>
        /// Create a simple stats panel for testing
        /// </summary>
        [ContextMenu("Create Simple Stats Panel")]
        public void CreateSimpleStatsPanel()
        {
            // Create a simple version for quick testing
            GameObject simplePanel = new GameObject("SimpleStatsPanel");
            simplePanel.transform.SetParent(transform);
            
            // Add Canvas
            Canvas canvas = simplePanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            simplePanel.AddComponent<CanvasScaler>();
            simplePanel.AddComponent<GraphicRaycaster>();
            
            // Add StatsSummaryPanel component
            StatsSummaryPanel statsPanel = simplePanel.AddComponent<StatsSummaryPanel>();
            
            // Create basic UI elements
            GameObject panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(simplePanel.transform);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 1);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.pivot = new Vector2(1, 1);
            panelRect.sizeDelta = new Vector2(300, 400);
            panelRect.anchoredPosition = new Vector2(-10, -10);
            panelGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            statsPanel.statsPanel = panelGO;
            
            // Add text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(panelGO.transform);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "Stats will appear here";
            text.color = Color.white;
            text.fontSize = 18;
            text.enableWordWrapping = true;
            statsPanel.statsContentText = text;
            
            // Add toggle button
            GameObject buttonGO = new GameObject("ToggleButton");
            buttonGO.transform.SetParent(simplePanel.transform);
            RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 1);
            buttonRect.anchorMax = new Vector2(1, 1);
            buttonRect.pivot = new Vector2(1, 1);
            buttonRect.sizeDelta = new Vector2(120, 40);
            buttonRect.anchoredPosition = new Vector2(-10, -420);
            Button button = buttonGO.AddComponent<Button>();
            buttonGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            statsPanel.toggleButton = button;
            
            // Add button text
            GameObject buttonTextGO = new GameObject("Text");
            buttonTextGO.transform.SetParent(buttonGO.transform);
            RectTransform buttonTextRect = buttonTextGO.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Show Stats";
            buttonText.color = Color.white;
            buttonText.fontSize = 16;
            buttonText.alignment = TextAlignmentOptions.Center;
            statsPanel.toggleButtonText = buttonText;
            
            // Initially hide
            panelGO.SetActive(false);
            
            Debug.Log("[StatsSummaryPanelSetupHelper] ✅ Simple Stats Panel Created!");
        }
    }
}
