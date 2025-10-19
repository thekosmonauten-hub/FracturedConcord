using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Manages the board selection UI specifically for extension board extension points
    /// Filters out duplicate boards and provides a tailored experience for extension board selection
    /// </summary>
    public class ExtensionBoardSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Transform boardButtonContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button backgroundBlocker; // Background panel to block clicks
        
        [Header("Board Button Prefab")]
        [SerializeField] private GameObject boardButtonPrefab;
        
        [Header("Board Data")]
        [SerializeField] private List<BoardData> availableBoards = new List<BoardData>();
        
        [Header("Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private int boardTier = 1; // Default tier for board selection
        
        // Events
        public System.Action<BoardData> OnBoardSelected;
        public System.Action OnSelectionCancelled;
        
        // Current context
        private ExtensionPoint currentExtensionPoint;
        private Vector2Int targetGridPosition;
        private string currentBoardName; // The name of the board this extension point belongs to
        
        void Awake()
        {
            // Hide the panel initially
            if (selectionPanel != null)
                selectionPanel.SetActive(false);
                
            // Setup close button
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSelection);
                
            // Setup background blocker to prevent click-through
            if (backgroundBlocker != null)
                backgroundBlocker.onClick.AddListener(CloseSelection);
                
            // Make this GameObject persistent to prevent destruction (only if it's a root object)
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning($"[ExtensionBoardSelectionUI] Cannot use DontDestroyOnLoad on {gameObject.name} - it's not a root GameObject. Parent: {transform.parent.name}");
            }
            
            // Auto-populate available boards
            AutoPopulateAvailableBoards();
            
            // Subscribe to board selection events to refresh UI
            SubscribeToBoardSelectionEvents();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            UnsubscribeFromBoardSelectionEvents();
        }
        
        /// <summary>
        /// Show the board selection UI for a specific extension point on an extension board
        /// </summary>
        public void ShowBoardSelection(ExtensionPoint extensionPoint, Vector2Int gridPosition, string currentBoardName)
        {
            ShowBoardSelection(extensionPoint, gridPosition, currentBoardName, boardTier);
        }
        
        /// <summary>
        /// Show the board selection UI for a specific extension point on an extension board with tier
        /// </summary>
        public void ShowBoardSelection(ExtensionPoint extensionPoint, Vector2Int gridPosition, string currentBoardName, int tier)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Showing board selection for extension point at {extensionPoint.position}");
                Debug.Log($"[ExtensionBoardSelectionUI] Current board: {currentBoardName}, Target position: {gridPosition}, Tier: {tier}");
            }
            
            currentExtensionPoint = extensionPoint;
            targetGridPosition = gridPosition;
            this.currentBoardName = currentBoardName;
            boardTier = tier;
            
            // Update UI text
            if (titleText != null)
                titleText.text = $"Select Extension Board (Tier {tier})";
            else if (showDebugInfo)
                Debug.LogWarning($"[ExtensionBoardSelectionUI] titleText is null! Please assign titleText in the inspector.");
                
            if (descriptionText != null)
                descriptionText.text = $"Choose a new extension board to create from {currentBoardName} (Tier {tier})";
            else if (showDebugInfo)
                Debug.LogWarning($"[ExtensionBoardSelectionUI] descriptionText is null! Please assign descriptionText in the inspector.");
            
            // Clear existing buttons
            ClearBoardButtons();
            
            // Create buttons for available boards (filtered to exclude current board)
            CreateBoardButtons();
            
            // Create background blocker if needed
            CreateBackgroundBlocker();
            
            // Show the panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardSelectionUI] Panel activated: {selectionPanel.activeInHierarchy}");
                }
            }
            else if (showDebugInfo)
            {
                Debug.LogError($"[ExtensionBoardSelectionUI] selectionPanel is null - cannot show UI!");
            }
        }
        
        /// <summary>
        /// Hide the board selection UI
        /// </summary>
        public void HideBoardSelection()
        {
            if (selectionPanel != null)
                selectionPanel.SetActive(false);
                
            currentExtensionPoint = null;
            currentBoardName = null;
        }
        
        /// <summary>
        /// Close the selection (same as hide but triggers cancel event)
        /// </summary>
        public void CloseSelection()
        {
            HideBoardSelection();
            OnSelectionCancelled?.Invoke();
        }
        
        /// <summary>
        /// Create a background blocker panel if it doesn't exist
        /// </summary>
        private void CreateBackgroundBlocker()
        {
            if (backgroundBlocker != null) return; // Already exists
            
            // Create a new GameObject for the background
            GameObject blockerObject = new GameObject("BackgroundBlocker");
            blockerObject.transform.SetParent(transform, false);
            
            // Add Image component for visual background
            var image = blockerObject.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
            
            // Add Button component for click handling
            backgroundBlocker = blockerObject.AddComponent<Button>();
            
            // Set up RectTransform to cover full screen
            var rectTransform = blockerObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Make sure it's behind the selection panel
            blockerObject.transform.SetAsFirstSibling();
            
            // Setup the click listener
            backgroundBlocker.onClick.AddListener(CloseSelection);
            
            Debug.Log("[ExtensionBoardSelectionUI] Created background blocker panel");
        }
        
        /// <summary>
        /// Clear all existing board buttons
        /// </summary>
        private void ClearBoardButtons()
        {
            if (boardButtonContainer == null) return;
            
            foreach (Transform child in boardButtonContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
        
        /// <summary>
        /// Create buttons for available boards (filtered to exclude current board and duplicates)
        /// </summary>
        private void CreateBoardButtons()
        {
            if (boardButtonContainer == null || boardButtonPrefab == null) 
            {
                Debug.LogError($"[ExtensionBoardSelectionUI] Cannot create buttons - missing references!");
                return;
            }
            
            // Get the board selection tracker
            var tracker = BoardSelectionTracker.Instance;
            if (tracker == null)
            {
                Debug.LogError($"[ExtensionBoardSelectionUI] BoardSelectionTracker not found!");
                return;
            }
            
            // Filter available boards using the tracker and additional filters
            var finalSelectableBoards = new List<BoardData>();
            
            foreach (var board in availableBoards)
            {
                if (board == null || !board.IsUnlocked) continue;
                
                // Skip core boards
                if (board.BoardName.ToLower().Contains("core")) continue;
                
                // Skip current board
                if (board.BoardName.Equals(currentBoardName, System.StringComparison.OrdinalIgnoreCase)) continue;
                
                // Check if this specific board is already selected
                if (tracker.IsBoardSelected(board)) continue;
                
                // Check if this theme is already selected for this tier
                if (tracker.IsThemeSelectedForTier(boardTier, board.BoardTheme)) continue;
                
                finalSelectableBoards.Add(board);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Creating {finalSelectableBoards.Count} board selection buttons for tier {boardTier}");
                Debug.Log($"[ExtensionBoardSelectionUI] Current board '{currentBoardName}' excluded from selection");
                Debug.Log($"[ExtensionBoardSelectionUI] Available boards: {string.Join(", ", finalSelectableBoards.Select(b => b.BoardName))}");
            }
            
            foreach (var boardData in finalSelectableBoards)
            {
                CreateBoardButton(boardData);
            }
            
            // If no boards are available, show a message
            if (finalSelectableBoards.Count == 0)
            {
                CreateNoBoardsMessage();
            }
        }
        
        /// <summary>
        /// Create a message when no boards are available
        /// </summary>
        private void CreateNoBoardsMessage()
        {
            if (boardButtonPrefab == null) return;
            
            // Create a disabled button to show the message
            GameObject messageObj = Instantiate(boardButtonPrefab, boardButtonContainer);
            Button button = messageObj.GetComponent<Button>();
            
            if (button != null)
            {
                button.interactable = false;
            }
            
            // Setup message text
            TextMeshProUGUI messageText = messageObj.GetComponentInChildren<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = "No additional boards available";
                messageText.color = Color.gray;
            }
            
            // Setup button appearance
            Image buttonImage = messageObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Semi-transparent gray
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] No additional boards available for selection");
            }
        }
        
        /// <summary>
        /// Create a button for a specific board
        /// </summary>
        private void CreateBoardButton(BoardData boardData)
        {
            if (boardButtonPrefab == null) return;
            
            // Instantiate button
            GameObject buttonObj = Instantiate(boardButtonPrefab, boardButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            if (button == null)
            {
                Debug.LogError($"[ExtensionBoardSelectionUI] Board button prefab doesn't have a Button component!");
                Destroy(buttonObj);
                return;
            }
            
            // Setup button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = boardData.BoardName;
            }
            
            // Setup button image/preview
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null && boardData.BoardPreview != null)
            {
                buttonImage.sprite = boardData.BoardPreview;
            }
            
            // Setup button color and layout
            if (buttonImage != null)
            {
                buttonImage.color = boardData.BoardColor;
            }
            
            // Ensure proper layout by setting RectTransform properties
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // Set anchor to top-left for proper grid layout
                buttonRect.anchorMin = new Vector2(0, 1);
                buttonRect.anchorMax = new Vector2(0, 1);
                buttonRect.pivot = new Vector2(0, 1);
                
                // Set size
                buttonRect.sizeDelta = new Vector2(150, 50);
                
                // Position will be handled by the layout component
            }
            
            // Setup button click event
            button.onClick.AddListener(() => SelectBoard(boardData));
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Created button for board: {boardData.BoardName}");
            }
        }
        
        /// <summary>
        /// Handle board selection
        /// </summary>
        private void SelectBoard(BoardData boardData)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Board selected: {boardData.BoardName} for extension point at {currentExtensionPoint?.position}");
            }
            
            // Register the board selection with the tracker
            var tracker = BoardSelectionTracker.Instance;
            if (tracker != null)
            {
                bool success = tracker.SelectBoard(boardData, boardTier, boardData.BoardTheme);
                if (!success)
                {
                    Debug.LogWarning($"[ExtensionBoardSelectionUI] Failed to register board selection: {boardData.BoardName}");
                    return;
                }
            }
            
            // Hide the UI
            HideBoardSelection();
            
            // Trigger the selection event
            OnBoardSelected?.Invoke(boardData);
        }
        
        /// <summary>
        /// Auto-populate available boards from the project
        /// </summary>
        private void AutoPopulateAvailableBoards()
        {
            // Find all BoardData assets in the project
            BoardData[] allBoardData = Resources.FindObjectsOfTypeAll<BoardData>();
            
            // Filter to include only extension boards (exclude core board)
            foreach (BoardData boardData in allBoardData)
            {
                if (boardData != null && !boardData.BoardName.ToLower().Contains("core"))
                {
                    AddAvailableBoard(boardData);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Found {availableBoards.Count} available extension boards");
            }
        }
        
        /// <summary>
        /// Add a board to the available boards list
        /// </summary>
        public void AddAvailableBoard(BoardData boardData)
        {
            if (boardData != null && !availableBoards.Contains(boardData))
            {
                availableBoards.Add(boardData);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardSelectionUI] Added board to available list: {boardData.BoardName}");
                }
            }
        }
        
        /// <summary>
        /// Remove a board from the available boards list
        /// </summary>
        public void RemoveAvailableBoard(BoardData boardData)
        {
            if (boardData != null && availableBoards.Contains(boardData))
            {
                availableBoards.Remove(boardData);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardSelectionUI] Removed board from available list: {boardData.BoardName}");
                }
            }
        }
        
        /// <summary>
        /// Get the current extension point
        /// </summary>
        public ExtensionPoint GetCurrentExtensionPoint()
        {
            return currentExtensionPoint;
        }
        
        /// <summary>
        /// Get the target grid position
        /// </summary>
        public Vector2Int GetTargetGridPosition()
        {
            return targetGridPosition;
        }
        
        /// <summary>
        /// Get the current board name
        /// </summary>
        public string GetCurrentBoardName()
        {
            return currentBoardName;
        }
        
        /// <summary>
        /// Subscribe to board selection events
        /// </summary>
        private void SubscribeToBoardSelectionEvents()
        {
            var tracker = BoardSelectionTracker.Instance;
            if (tracker != null)
            {
                tracker.OnBoardSelected += HandleBoardSelected;
                tracker.OnBoardDeselected += HandleBoardDeselected;
            }
        }
        
        /// <summary>
        /// Unsubscribe from board selection events
        /// </summary>
        private void UnsubscribeFromBoardSelectionEvents()
        {
            var tracker = BoardSelectionTracker.Instance;
            if (tracker != null)
            {
                tracker.OnBoardSelected -= HandleBoardSelected;
                tracker.OnBoardDeselected -= HandleBoardDeselected;
            }
        }
        
        /// <summary>
        /// Handle board selection event
        /// </summary>
        private void HandleBoardSelected(BoardData boardData, int tier, BoardTheme theme)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Board selected: {boardData.BoardName} for tier {tier}, theme {theme}");
            }
            
            // Refresh the UI if it's currently visible
            if (selectionPanel != null && selectionPanel.activeInHierarchy)
            {
                RefreshBoardSelection();
            }
        }
        
        /// <summary>
        /// Handle board deselection event
        /// </summary>
        private void HandleBoardDeselected(BoardData boardData, int tier, BoardTheme theme)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Board deselected: {boardData.BoardName} for tier {tier}, theme {theme}");
            }
            
            // Refresh the UI if it's currently visible
            if (selectionPanel != null && selectionPanel.activeInHierarchy)
            {
                RefreshBoardSelection();
            }
        }
        
        /// <summary>
        /// Refresh the board selection UI
        /// </summary>
        public void RefreshBoardSelection()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardSelectionUI] Refreshing board selection UI");
            }
            
            // Clear existing buttons
            ClearBoardButtons();
            
            // Create new buttons with updated filtering
            CreateBoardButtons();
        }
    }
}
