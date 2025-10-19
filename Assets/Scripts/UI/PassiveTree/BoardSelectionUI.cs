using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Manages the board selection UI that appears when clicking extension points
    /// </summary>
    public class BoardSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Transform boardButtonContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button backgroundBlocker; // Background panel to block clicks
        [SerializeField] private UnityEngine.UI.ScrollRect scrollRect; // Reference to your Scroll Rect
        [SerializeField] private GameObject boardsContainer; // Reference to the BoardsContainer GameObject
        
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
                Debug.LogWarning($"[BoardSelectionUI] Cannot use DontDestroyOnLoad on {gameObject.name} - it's not a root GameObject. Parent: {transform.parent.name}");
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
        /// Show the board selection UI for a specific extension point
        /// </summary>
        public void ShowBoardSelection(ExtensionPoint extensionPoint, Vector2Int gridPosition)
        {
            ShowBoardSelection(extensionPoint, gridPosition, boardTier);
        }
        
        /// <summary>
        /// Show the board selection UI for a specific extension point with tier
        /// </summary>
        public void ShowBoardSelection(ExtensionPoint extensionPoint, Vector2Int gridPosition, int tier)
        {
            Debug.Log($"[BoardSelectionUI] Showing board selection for extension point at {extensionPoint.position}, tier {tier}");
            
            currentExtensionPoint = extensionPoint;
            targetGridPosition = gridPosition;
            boardTier = tier;
            
            // Update UI text
            if (titleText != null)
                titleText.text = $"Select Board Type (Tier {tier})";
            else if (showDebugInfo)
                Debug.LogWarning($"[BoardSelectionUI] titleText is null! Please assign titleText in the inspector.");
                
            if (descriptionText != null)
                descriptionText.text = $"Choose a board to create at position {gridPosition} (Tier {tier})";
            else if (showDebugInfo)
                Debug.LogWarning($"[BoardSelectionUI] descriptionText is null! Please assign descriptionText in the inspector.");
            
            // Clear existing buttons
            ClearBoardButtons();
            
            // Create buttons for available boards
            CreateBoardButtons();
            
        // Disable passive tree to prevent click-through
        Debug.Log("[BoardSelectionUI] 🎯 ShowBoardSelection called - about to disable passive tree");
        DisablePassiveTree();
            
            // Create background blocker if needed
            CreateBackgroundBlocker();
            
            // Show the panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                if (showDebugInfo)
                {
                    Debug.Log($"[BoardSelectionUI] Panel activated: {selectionPanel.activeInHierarchy}");
                }
            }
            else if (showDebugInfo)
            {
                Debug.LogError($"[BoardSelectionUI] selectionPanel is null - cannot show UI!");
            }
        }
        
        /// <summary>
        /// Hide the board selection UI
        /// </summary>
        public void HideBoardSelection()
        {
            if (selectionPanel != null)
                selectionPanel.SetActive(false);
                
        // Re-enable passive tree
        EnablePassiveTree();
                
            currentExtensionPoint = null;
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
        /// Disable passive tree interaction to prevent click-through
        /// </summary>
        private void DisablePassiveTree()
        {
            Debug.Log("[BoardSelectionUI] 🔒 Starting to disable passive tree...");
            
            // Method 1: Try to find BoardsContainer if not assigned
            if (boardsContainer == null)
            {
                boardsContainer = GameObject.Find("BoardsContainer");
                if (boardsContainer != null)
                {
                    Debug.Log($"[BoardSelectionUI] Found BoardsContainer: {boardsContainer.name}");
                }
                else
                {
                    Debug.LogWarning("[BoardSelectionUI] ❌ BoardsContainer not found! Searching for alternatives...");
                }
            }
            
            // Method 1: Disable the entire BoardsContainer
            if (boardsContainer != null)
            {
                boardsContainer.SetActive(false);
                Debug.Log($"[BoardSelectionUI] ✅ Disabled BoardsContainer: {boardsContainer.name}");
            }
            else
            {
                Debug.LogWarning("[BoardSelectionUI] ❌ BoardsContainer is null, cannot disable it");
            }
            
            // Method 2: Disable all passive tree cells/buttons as backup
            CellController[] allCells = FindObjectsOfType<CellController>();
            Debug.Log($"[BoardSelectionUI] Found {allCells.Length} CellController components");
            foreach (CellController cell in allCells)
            {
                cell.enabled = false; // Disable the cell controller
            }
            
            // Method 3: Disable all colliders on passive tree objects
            Collider[] allColliders = FindObjectsOfType<Collider>();
            int disabledColliders = 0;
            foreach (Collider collider in allColliders)
            {
                // Only disable colliders that are part of passive tree (not UI)
                if (collider.gameObject.name.Contains("Board") || 
                    collider.gameObject.name.Contains("Cell") ||
                    collider.gameObject.name.Contains("Node"))
                {
                    collider.enabled = false;
                    disabledColliders++;
                }
            }
            
            Debug.Log($"[BoardSelectionUI] ✅ Disabled {allCells.Length} cells and {disabledColliders} colliders to prevent click-through");
        }
        
        /// <summary>
        /// Re-enable passive tree interaction
        /// </summary>
        private void EnablePassiveTree()
        {
            Debug.Log("[BoardSelectionUI] 🔓 Starting to re-enable passive tree...");
            
            // Method 1: Re-enable the entire BoardsContainer
            if (boardsContainer != null)
            {
                boardsContainer.SetActive(true);
                Debug.Log($"[BoardSelectionUI] ✅ Re-enabled BoardsContainer: {boardsContainer.name}");
            }
            else
            {
                Debug.LogWarning("[BoardSelectionUI] ❌ BoardsContainer is null, cannot re-enable it");
            }
            
            // Method 2: Re-enable all passive tree cells/buttons
            CellController[] allCells = FindObjectsOfType<CellController>();
            Debug.Log($"[BoardSelectionUI] Found {allCells.Length} CellController components to re-enable");
            foreach (CellController cell in allCells)
            {
                cell.enabled = true; // Re-enable the cell controller
            }
            
            // Method 3: Re-enable all colliders on passive tree objects
            Collider[] allColliders = FindObjectsOfType<Collider>();
            int enabledColliders = 0;
            foreach (Collider collider in allColliders)
            {
                // Only re-enable colliders that are part of passive tree (not UI)
                if (collider.gameObject.name.Contains("Board") || 
                    collider.gameObject.name.Contains("Cell") ||
                    collider.gameObject.name.Contains("Node"))
                {
                    collider.enabled = true;
                    enabledColliders++;
                }
            }
            
            Debug.Log($"[BoardSelectionUI] ✅ Re-enabled BoardsContainer, {allCells.Length} cells, and {enabledColliders} colliders");
        }
        
        /// <summary>
        /// Test method to manually disable passive tree (for debugging)
        /// </summary>
        [ContextMenu("Test Disable Passive Tree")]
        public void TestDisablePassiveTree()
        {
            Debug.Log("[BoardSelectionUI] 🧪 Testing disable passive tree...");
            DisablePassiveTree();
        }
        
        /// <summary>
        /// Test method to manually enable passive tree (for debugging)
        /// </summary>
        [ContextMenu("Test Enable Passive Tree")]
        public void TestEnablePassiveTree()
        {
            Debug.Log("[BoardSelectionUI] 🧪 Testing enable passive tree...");
            EnablePassiveTree();
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
            
            Debug.Log("[BoardSelectionUI] Created background blocker panel");
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
        /// Create buttons for all available boards
        /// </summary>
        private void CreateBoardButtons()
        {
            if (boardButtonContainer == null || boardButtonPrefab == null) 
            {
                Debug.LogError($"[BoardSelectionUI] Cannot create buttons - missing references!");
                return;
            }
            
            // Get the board selection tracker
            var tracker = BoardSelectionTracker.Instance;
            if (tracker == null)
            {
                Debug.LogError($"[BoardSelectionUI] BoardSelectionTracker not found!");
                return;
            }
            
            // Filter available boards using the tracker - check each board individually
            var selectableBoards = new List<BoardData>();
            
            foreach (var boardData in availableBoards)
            {
                if (boardData == null || !boardData.IsUnlocked) continue;
                
                // Check if this specific board is already selected
                if (tracker.IsBoardSelected(boardData)) continue;
                
                // Check if this theme is already selected for this tier
                if (tracker.IsThemeSelectedForTier(boardTier, boardData.BoardTheme)) continue;
                
                selectableBoards.Add(boardData);
            }
            
            Debug.Log($"[BoardSelectionUI] Creating {selectableBoards.Count} board selection buttons for tier {boardTier}");
            Debug.Log($"[BoardSelectionUI] Filtered out {availableBoards.Count - selectableBoards.Count} unavailable boards");
            
            foreach (var boardData in selectableBoards)
            {
                CreateBoardButton(boardData);
            }
            
            // If no boards are available, show a message
            if (selectableBoards.Count == 0)
            {
                CreateNoBoardsMessage();
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
                Debug.LogError($"[BoardSelectionUI] Board button prefab doesn't have a Button component!");
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
        }
        
        /// <summary>
        /// Handle board selection
        /// </summary>
        private void SelectBoard(BoardData boardData)
        {
            Debug.Log($"[BoardSelectionUI] Board selected: {boardData.BoardName}");
            
            // Register the board selection with the tracker
            var tracker = BoardSelectionTracker.Instance;
            if (tracker != null)
            {
                bool success = tracker.SelectBoard(boardData, boardTier, boardData.BoardTheme);
                if (!success)
                {
                    Debug.LogWarning($"[BoardSelectionUI] Failed to register board selection: {boardData.BoardName}");
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
            
            // Filter out extension boards (exclude core board)
            foreach (BoardData boardData in allBoardData)
            {
                if (boardData != null && !boardData.BoardName.ToLower().Contains("core"))
                {
                    AddAvailableBoard(boardData);
                }
            }
            
            Debug.Log($"[BoardSelectionUI] Found {availableBoards.Count} available boards");
        }
        
        /// <summary>
        /// Add a board to the available boards list
        /// </summary>
        public void AddAvailableBoard(BoardData boardData)
        {
            if (boardData != null && !availableBoards.Contains(boardData))
            {
                availableBoards.Add(boardData);
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
                    Debug.Log($"[BoardSelectionUI] Removed board from available list: {boardData.BoardName}");
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
                messageText.text = "No boards available for this tier";
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
                Debug.Log($"[BoardSelectionUI] No boards available for tier {boardTier}");
            }
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
                Debug.Log($"[BoardSelectionUI] Board selected: {boardData.BoardName} for tier {tier}, theme {theme}");
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
                Debug.Log($"[BoardSelectionUI] Board deselected: {boardData.BoardName} for tier {tier}, theme {theme}");
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
                Debug.Log($"[BoardSelectionUI] Refreshing board selection UI");
            }
            
            // Clear existing buttons
            ClearBoardButtons();
            
            // Create new buttons with updated filtering
            CreateBoardButtons();
        }
    }
}
