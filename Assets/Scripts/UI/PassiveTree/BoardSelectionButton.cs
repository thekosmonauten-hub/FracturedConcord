using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PassiveTree
{
    /// <summary>
    /// Component for board selection buttons in the board selection UI
    /// </summary>
    public class BoardSelectionButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Image boardPreviewImage;
        [SerializeField] private TextMeshProUGUI boardNameText;
        [SerializeField] private TextMeshProUGUI boardDescriptionText;
        [SerializeField] private Image backgroundImage;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.cyan;
        [SerializeField] private Color selectedColor = Color.green;
        
        // Current board data
        private BoardData currentBoardData;
        
        // Events
        public System.Action<BoardData> OnBoardSelected;
        
        void Awake()
        {
            // Get components if not assigned
            if (button == null)
                button = GetComponent<Button>();
                
            if (boardPreviewImage == null)
                boardPreviewImage = GetComponentInChildren<Image>();
                
            if (boardNameText == null)
                boardNameText = GetComponentInChildren<TextMeshProUGUI>();
                
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            // Setup button events
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        /// <summary>
        /// Setup the button with board data
        /// </summary>
        public void SetupButton(BoardData boardData)
        {
            currentBoardData = boardData;
            
            if (boardData == null) return;
            
            // Set board name
            if (boardNameText != null)
                boardNameText.text = boardData.BoardName;
            
            // Set board description
            if (boardDescriptionText != null)
                boardDescriptionText.text = boardData.BoardDescription;
            
            // Set board preview image
            if (boardPreviewImage != null && boardData.BoardPreview != null)
                boardPreviewImage.sprite = boardData.BoardPreview;
            
            // Set background color
            if (backgroundImage != null)
                backgroundImage.color = boardData.BoardColor;
            
            // Enable/disable button based on unlock status
            if (button != null)
                button.interactable = boardData.IsUnlocked;
        }
        
        /// <summary>
        /// Handle button click
        /// </summary>
        private void OnButtonClicked()
        {
            if (currentBoardData != null)
            {
                OnBoardSelected?.Invoke(currentBoardData);
            }
        }
        
        /// <summary>
        /// Set the button's visual state
        /// </summary>
        public void SetButtonState(bool isSelected, bool isHovered = false)
        {
            if (backgroundImage == null) return;
            
            if (isSelected)
                backgroundImage.color = selectedColor;
            else if (isHovered)
                backgroundImage.color = hoverColor;
            else
                backgroundImage.color = normalColor;
        }
        
        /// <summary>
        /// Get the current board data
        /// </summary>
        public BoardData GetBoardData()
        {
            return currentBoardData;
        }
    }
}

