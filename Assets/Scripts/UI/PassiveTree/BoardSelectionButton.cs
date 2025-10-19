using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace PassiveTree.UI
{
    /// <summary>
    /// Button component for board selection with board data
    /// </summary>
    public class BoardSelectionButton : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image buttonImage;
        
        [Header("Board Data")]
        [SerializeField] private BoardData boardData;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<BoardData> onBoardSelected;
        
        private void Awake()
        {
            // Get button component if not assigned
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            
            // Get text component if not assigned
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
            }
            
            // Get image component if not assigned
            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
            }
            
            // Setup button click event
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        /// <summary>
        /// Set the board data for this button
        /// </summary>
        public void SetBoardData(BoardData data)
        {
            boardData = data;
            UpdateButtonDisplay();
        }
        
        /// <summary>
        /// Get the board data for this button
        /// </summary>
        public BoardData GetBoardData()
        {
            return boardData;
        }
        
        /// <summary>
        /// Update the button's visual display
        /// </summary>
        private void UpdateButtonDisplay()
        {
            if (boardData == null) return;
            
            // Update button text
            if (buttonText != null)
            {
                buttonText.text = boardData.BoardName;
            }
            
            // Update button image if available
            if (buttonImage != null && boardData.BoardPreview != null)
            {
                buttonImage.sprite = boardData.BoardPreview;
            }
        }
        
        /// <summary>
        /// Handle button click
        /// </summary>
        private void OnButtonClicked()
        {
            if (boardData == null)
            {
                Debug.LogWarning("[BoardSelectionButton] Button clicked but no board data assigned");
                return;
            }
            
            Debug.Log($"[BoardSelectionButton] Board selected: {boardData.BoardName}");
            
            // Invoke the selection event
            onBoardSelected?.Invoke(boardData);
        }
        
        /// <summary>
        /// Enable or disable the button
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
        
        /// <summary>
        /// Set the button's visual state
        /// </summary>
        public void SetButtonState(bool isSelected, bool isAvailable)
        {
            if (button == null) return;
            
            // Update button appearance based on state
            var colors = button.colors;
            
            if (isSelected)
            {
                colors.normalColor = Color.green;
                colors.highlightedColor = Color.green;
            }
            else if (!isAvailable)
            {
                colors.normalColor = Color.gray;
                colors.highlightedColor = Color.gray;
            }
            else
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.yellow;
            }
            
            button.colors = colors;
        }
        
        private void OnDestroy()
        {
            // Clean up button event
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}