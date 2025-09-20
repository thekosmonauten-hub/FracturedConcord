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
        
        [Header("Board Button Prefab")]
        [SerializeField] private GameObject boardButtonPrefab;
        
        [Header("Board Data")]
        [SerializeField] private List<BoardData> availableBoards = new List<BoardData>();
        
        [Header("Settings")]
        [SerializeField] private bool showDebugInfo = true;
        
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
        }
        
        /// <summary>
        /// Show the board selection UI for a specific extension point
        /// </summary>
        public void ShowBoardSelection(ExtensionPoint extensionPoint, Vector2Int gridPosition)
        {
            Debug.Log($"[BoardSelectionUI] Showing board selection for extension point at {extensionPoint.position}");
            
            currentExtensionPoint = extensionPoint;
            targetGridPosition = gridPosition;
            
            // Update UI text
            if (titleText != null)
                titleText.text = "Select Board Type";
            else if (showDebugInfo)
                Debug.LogWarning($"[BoardSelectionUI] titleText is null! Please assign titleText in the inspector.");
                
            if (descriptionText != null)
                descriptionText.text = $"Choose a board to create at position {gridPosition}";
            else if (showDebugInfo)
                Debug.LogWarning($"[BoardSelectionUI] descriptionText is null! Please assign descriptionText in the inspector.");
            
            // Clear existing buttons
            ClearBoardButtons();
            
            // Create buttons for available boards
            CreateBoardButtons();
            
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
            
            // Filter available boards (for now, show all unlocked boards)
            var selectableBoards = availableBoards.Where(board => board.IsUnlocked).ToList();
            
            Debug.Log($"[BoardSelectionUI] Creating {selectableBoards.Count} board selection buttons");
            
            foreach (var boardData in selectableBoards)
            {
                CreateBoardButton(boardData);
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
    }
}
