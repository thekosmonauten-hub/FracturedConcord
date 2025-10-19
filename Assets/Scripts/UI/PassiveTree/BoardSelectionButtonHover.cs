using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PassiveTree.UI
{
    /// <summary>
    /// Handles hover events for board selection buttons to show board summary
    /// </summary>
    public class BoardSelectionButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private BoardSummaryWindow summaryWindow;
        [SerializeField] private BoardData boardData;
        
        [Header("Hover Settings")]
        [SerializeField] private float hoverDelay = 0.5f; // Delay before showing summary
        [SerializeField] private bool showSummaryOnHover = true;
        
        private float hoverStartTime;
        private bool isHovering = false;
        private Coroutine hoverCoroutine;
        
        private void Start()
        {
            // Auto-find summary window if not assigned
            if (summaryWindow == null)
            {
                summaryWindow = FindObjectOfType<BoardSummaryWindow>();
                if (summaryWindow == null)
                {
                    Debug.LogWarning("[BoardSelectionButtonHover] No BoardSummaryWindow found in scene");
                }
            }
            
            // Get board data from button if not assigned
            if (boardData == null)
            {
                var button = GetComponent<Button>();
                if (button != null)
                {
                    // Try to get board data from button's onClick event or attached component
                    var boardButton = GetComponent<BoardSelectionButton>();
                    if (boardButton != null)
                    {
                        boardData = boardButton.GetBoardData();
                    }
                }
            }
        }
        
        /// <summary>
        /// Set the board data for this button
        /// </summary>
        public void SetBoardData(BoardData data)
        {
            boardData = data;
        }
        
        /// <summary>
        /// Handle mouse enter event
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!showSummaryOnHover || boardData == null || summaryWindow == null)
                return;
            
            isHovering = true;
            hoverStartTime = Time.time;
            
            // Start hover delay coroutine
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(ShowSummaryAfterDelay());
            
            Debug.Log($"[BoardSelectionButtonHover] Mouse entered button for {boardData.BoardName}");
        }
        
        /// <summary>
        /// Handle mouse exit event
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isHovering)
                return;
            
            isHovering = false;
            
            // Stop hover coroutine
            if (hoverCoroutine != null)
            {
                StopCoroutine(hoverCoroutine);
                hoverCoroutine = null;
            }
            
            // Hide summary if it's showing
            if (summaryWindow != null && summaryWindow.IsVisible)
            {
                summaryWindow.HideBoardSummary();
            }
            
            Debug.Log($"[BoardSelectionButtonHover] Mouse exited button for {(boardData != null ? boardData.BoardName : "unknown")}");
        }
        
        /// <summary>
        /// Show summary after hover delay
        /// </summary>
        private System.Collections.IEnumerator ShowSummaryAfterDelay()
        {
            yield return new WaitForSeconds(hoverDelay);
            
            // Only show if still hovering
            if (isHovering && boardData != null && summaryWindow != null)
            {
                summaryWindow.ShowBoardSummary(boardData);
                Debug.Log($"[BoardSelectionButtonHover] Showing summary for {boardData.BoardName}");
            }
        }
        
        /// <summary>
        /// Force show summary (for testing)
        /// </summary>
        [ContextMenu("Test Show Summary")]
        public void TestShowSummary()
        {
            if (boardData != null && summaryWindow != null)
            {
                summaryWindow.ShowBoardSummary(boardData);
            }
        }
        
        /// <summary>
        /// Force hide summary (for testing)
        /// </summary>
        [ContextMenu("Test Hide Summary")]
        public void TestHideSummary()
        {
            if (summaryWindow != null)
            {
                summaryWindow.HideBoardSummary();
            }
        }
    }
}
