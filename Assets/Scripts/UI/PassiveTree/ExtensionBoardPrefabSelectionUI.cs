using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Manages the board selection UI specifically for extension board prefabs
    /// Shows pre-configured prefabs instead of BoardData assets
    /// </summary>
    public class ExtensionBoardPrefabSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Transform boardButtonContainer;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Board Button Prefab")]
        [SerializeField] private GameObject boardButtonPrefab;
        
        [Header("Available Board Prefabs")]
        [SerializeField] private List<ExtensionBoardPrefabData> availableBoardPrefabs = new List<ExtensionBoardPrefabData>();
        
        [Header("Settings")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Events
        public System.Action<GameObject> OnBoardPrefabSelected;
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
                
            // Load available board prefabs from the ExtensionBoardGenerator
            LoadBoardPrefabsFromGenerator();
        }
        
        /// <summary>
        /// Load board prefabs from the ExtensionBoardGenerator
        /// </summary>
        private void LoadBoardPrefabsFromGenerator()
        {
            // Find the ExtensionBoardGenerator in the scene
            ExtensionBoardGenerator generator = FindFirstObjectByType<ExtensionBoardGenerator>();
            if (generator == null)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("[ExtensionBoardPrefabSelectionUI] No ExtensionBoardGenerator found in scene!");
                }
                return;
            }
            
            // Get available board prefabs from the generator
            ExtensionBoardPrefabData[] generatorPrefabs = generator.GetAvailableBoardPrefabs();
            
            if (generatorPrefabs == null || generatorPrefabs.Length == 0)
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning("[ExtensionBoardPrefabSelectionUI] No board prefabs found in ExtensionBoardGenerator!");
                }
                return;
            }
            
            // Clear existing prefabs and add new ones
            availableBoardPrefabs.Clear();
            foreach (var prefabData in generatorPrefabs)
            {
                if (prefabData != null)
                {
                    availableBoardPrefabs.Add(prefabData);
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Loaded {availableBoardPrefabs.Count} board prefabs from generator");
            }
        }
        
        /// <summary>
        /// Show the board selection UI for a specific extension point on an extension board
        /// </summary>
        public void ShowBoardSelection(ExtensionPoint extensionPoint, Vector2Int gridPosition, string currentBoardName)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Showing board selection for extension point at {extensionPoint.position}");
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Current board: {currentBoardName}, Target position: {gridPosition}");
            }
            
            currentExtensionPoint = extensionPoint;
            targetGridPosition = gridPosition;
            this.currentBoardName = currentBoardName;
            
            // Update UI text
            if (titleText != null)
                titleText.text = "Select Extension Board";
            else if (showDebugInfo)
                Debug.LogWarning($"[ExtensionBoardPrefabSelectionUI] titleText is null! Please assign titleText in the inspector.");
                
            if (descriptionText != null)
                descriptionText.text = $"Choose a new extension board to create from {currentBoardName}";
            else if (showDebugInfo)
                Debug.LogWarning($"[ExtensionBoardPrefabSelectionUI] descriptionText is null! Please assign descriptionText in the inspector.");
            
            // Clear existing buttons
            ClearBoardButtons();
            
            // Create buttons for available board prefabs (filtered to exclude current board)
            CreateBoardButtons();
            
            // Show the panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardPrefabSelectionUI] Panel activated: {selectionPanel.activeInHierarchy}");
                }
            }
            else if (showDebugInfo)
            {
                Debug.LogError($"[ExtensionBoardPrefabSelectionUI] selectionPanel is null - cannot show UI!");
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
        /// Create buttons for available board prefabs (filtered to exclude current board and duplicates)
        /// </summary>
        private void CreateBoardButtons()
        {
            if (boardButtonContainer == null || boardButtonPrefab == null) 
            {
                Debug.LogError($"[ExtensionBoardPrefabSelectionUI] Cannot create buttons - missing references!");
                return;
            }
            
            // Filter available board prefabs to exclude:
            // 1. The current board (to prevent duplicates)
            // 2. Invalid or null prefabs
            var selectableBoards = availableBoardPrefabs.Where(prefabData => 
                prefabData != null &&
                prefabData.boardPrefab != null &&
                !prefabData.boardName.Equals(currentBoardName, System.StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Creating {selectableBoards.Count} board selection buttons");
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Current board '{currentBoardName}' excluded from selection");
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Available boards: {string.Join(", ", selectableBoards.Select(b => b.boardName))}");
            }
            
            foreach (var prefabData in selectableBoards)
            {
                CreateBoardButton(prefabData);
            }
            
            // If no boards are available, show a message
            if (selectableBoards.Count == 0)
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
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] No additional boards available for selection");
            }
        }
        
        /// <summary>
        /// Create a button for a specific board prefab
        /// </summary>
        private void CreateBoardButton(ExtensionBoardPrefabData prefabData)
        {
            if (boardButtonPrefab == null) return;
            
            // Instantiate button
            GameObject buttonObj = Instantiate(boardButtonPrefab, boardButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            if (button == null)
            {
                Debug.LogError($"[ExtensionBoardPrefabSelectionUI] Board button prefab doesn't have a Button component!");
                Destroy(buttonObj);
                return;
            }
            
            // Setup button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = prefabData.boardName;
            }
            
            // Setup button image/preview
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null && prefabData.boardPreview != null)
            {
                buttonImage.sprite = prefabData.boardPreview;
            }
            
            // Setup button color and layout
            if (buttonImage != null)
            {
                buttonImage.color = prefabData.boardColor;
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
            button.onClick.AddListener(() => SelectBoardPrefab(prefabData));
            
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Created button for board: {prefabData.boardName}");
            }
        }
        
        /// <summary>
        /// Handle board prefab selection
        /// </summary>
        private void SelectBoardPrefab(ExtensionBoardPrefabData prefabData)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[ExtensionBoardPrefabSelectionUI] Board prefab selected: {prefabData.boardName} for extension point at {currentExtensionPoint?.position}");
            }
            
            // Hide the UI
            HideBoardSelection();
            
            // Trigger the selection event with the prefab
            OnBoardPrefabSelected?.Invoke(prefabData.boardPrefab);
        }
        
        /// <summary>
        /// Add a board prefab to the available boards list
        /// </summary>
        public void AddAvailableBoardPrefab(ExtensionBoardPrefabData prefabData)
        {
            if (prefabData != null && !availableBoardPrefabs.Contains(prefabData))
            {
                availableBoardPrefabs.Add(prefabData);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardPrefabSelectionUI] Added board prefab to available list: {prefabData.boardName}");
                }
            }
        }
        
        /// <summary>
        /// Remove a board prefab from the available boards list
        /// </summary>
        public void RemoveAvailableBoardPrefab(ExtensionBoardPrefabData prefabData)
        {
            if (prefabData != null && availableBoardPrefabs.Contains(prefabData))
            {
                availableBoardPrefabs.Remove(prefabData);
                
                if (showDebugInfo)
                {
                    Debug.Log($"[ExtensionBoardPrefabSelectionUI] Removed board prefab from available list: {prefabData.boardName}");
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
    }
}

