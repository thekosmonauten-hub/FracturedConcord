using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PassiveTree.UI
{
    /// <summary>
    /// Helper script to automatically set up board selection UI with hover detection
    /// </summary>
    public class BoardSelectionUISetup : MonoBehaviour
    {
        [Header("Setup References")]
        [SerializeField] private BoardPositioningManager boardPositioningManager;
        [SerializeField] private BoardSummaryWindow boardSummaryWindow;
        [SerializeField] private Transform boardButtonContainer;
        [SerializeField] private GameObject boardButtonPrefab;
        
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool addHoverDetection = true;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupBoardSelectionUI();
            }
        }
        
        /// <summary>
        /// Set up the board selection UI with hover detection
        /// </summary>
        [ContextMenu("Setup Board Selection UI")]
        public void SetupBoardSelectionUI()
        {
            Debug.Log("[BoardSelectionUISetup] Setting up board selection UI...");
            
            // Auto-find components if not assigned
            if (boardPositioningManager == null)
            {
                boardPositioningManager = FindObjectOfType<BoardPositioningManager>();
            }
            
            if (boardSummaryWindow == null)
            {
                boardSummaryWindow = FindObjectOfType<BoardSummaryWindow>();
            }
            
            if (boardButtonContainer == null)
            {
                boardButtonContainer = transform;
            }
            
            // Create board summary window if it doesn't exist
            if (boardSummaryWindow == null)
            {
                CreateBoardSummaryWindow();
            }
            
            Debug.Log("[BoardSelectionUISetup] ✅ Board selection UI setup complete");
        }
        
        /// <summary>
        /// Create a board summary window if one doesn't exist
        /// </summary>
        private void CreateBoardSummaryWindow()
        {
            // Create a simple board summary window
            GameObject summaryWindowGO = new GameObject("BoardSummaryWindow");
            summaryWindowGO.transform.SetParent(transform);
            
            // Add the BoardSummaryWindow component
            boardSummaryWindow = summaryWindowGO.AddComponent<BoardSummaryWindow>();
            
            // Create a simple UI panel
            GameObject panel = new GameObject("SummaryPanel");
            panel.transform.SetParent(summaryWindowGO.transform);
            
            // Add Canvas and CanvasScaler for UI
            Canvas canvas = summaryWindowGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // High sorting order to appear on top
            
            CanvasScaler scaler = summaryWindowGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster
            summaryWindowGO.AddComponent<GraphicRaycaster>();
            
            // Create a simple panel with text
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
            
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 300);
            panelRect.anchoredPosition = new Vector2(200, 0); // Position to the right
            
            // Add text components
            GameObject boardNameGO = new GameObject("BoardNameText");
            boardNameGO.transform.SetParent(panel.transform);
            TextMeshProUGUI boardNameText = boardNameGO.AddComponent<TextMeshProUGUI>();
            boardNameText.text = "Board Name";
            boardNameText.fontSize = 18;
            boardNameText.color = Color.white;
            
            RectTransform boardNameRect = boardNameGO.GetComponent<RectTransform>();
            boardNameRect.anchorMin = new Vector2(0, 1);
            boardNameRect.anchorMax = new Vector2(1, 1);
            boardNameRect.sizeDelta = new Vector2(0, 30);
            boardNameRect.anchoredPosition = new Vector2(0, -15);
            
            // Add stats text
            GameObject statsGO = new GameObject("StatsText");
            statsGO.transform.SetParent(panel.transform);
            TextMeshProUGUI statsText = statsGO.AddComponent<TextMeshProUGUI>();
            statsText.text = "Stats will appear here...";
            statsText.fontSize = 14;
            statsText.color = Color.yellow;
            
            RectTransform statsRect = statsGO.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0.5f);
            statsRect.anchorMax = new Vector2(1, 1);
            statsRect.sizeDelta = new Vector2(-20, -40);
            statsRect.anchoredPosition = new Vector2(0, -20);
            
            // Add notables text
            GameObject notablesGO = new GameObject("NotablesText");
            notablesGO.transform.SetParent(panel.transform);
            TextMeshProUGUI notablesText = notablesGO.AddComponent<TextMeshProUGUI>();
            notablesText.text = "Notables will appear here...";
            notablesText.fontSize = 12;
            notablesText.color = Color.cyan;
            
            RectTransform notablesRect = notablesGO.GetComponent<RectTransform>();
            notablesRect.anchorMin = new Vector2(0, 0);
            statsRect.anchorMax = new Vector2(1, 0.5f);
            notablesRect.sizeDelta = new Vector2(-20, -20);
            notablesRect.anchoredPosition = new Vector2(0, 20);
            
            Debug.Log("[BoardSelectionUISetup] ✅ Created BoardSummaryWindow with basic UI");
        }
        
        /// <summary>
        /// Add hover detection to a board button
        /// </summary>
        public void AddHoverDetectionToButton(GameObject buttonGO, BoardData boardData)
        {
            if (buttonGO == null || boardData == null) return;
            
            // Add BoardSelectionButton component
            BoardSelectionButton selectionButton = buttonGO.GetComponent<BoardSelectionButton>();
            if (selectionButton == null)
            {
                selectionButton = buttonGO.AddComponent<BoardSelectionButton>();
            }
            selectionButton.SetBoardData(boardData);
            
            // Add hover detection
            if (addHoverDetection)
            {
                BoardSelectionButtonHover hover = buttonGO.GetComponent<BoardSelectionButtonHover>();
                if (hover == null)
                {
                    hover = buttonGO.AddComponent<BoardSelectionButtonHover>();
                }
                hover.SetBoardData(boardData);
            }
            
            Debug.Log($"[BoardSelectionUISetup] ✅ Added hover detection to button for {boardData.BoardName}");
        }
        
        /// <summary>
        /// Test the board summary with sample data
        /// </summary>
        [ContextMenu("Test Board Summary")]
        public void TestBoardSummary()
        {
            if (boardSummaryWindow == null)
            {
                Debug.LogWarning("[BoardSelectionUISetup] No BoardSummaryWindow found for testing");
                return;
            }
            
            // Create sample board data (Note: BoardData is a ScriptableObject, so we can't create it like this)
            // Instead, we'll use a mock approach or find an existing BoardData asset
            BoardData sampleBoard = ScriptableObject.CreateInstance<BoardData>();
            if (sampleBoard != null)
            {
                // We can't directly set the private fields, so this is just for testing
                Debug.Log("[BoardSelectionUISetup] Note: BoardData is a ScriptableObject - use existing assets for testing");
            }
            
            boardSummaryWindow.ShowBoardSummary(sampleBoard);
            Debug.Log("[BoardSelectionUISetup] ✅ Testing board summary with sample data");
        }
    }
}
