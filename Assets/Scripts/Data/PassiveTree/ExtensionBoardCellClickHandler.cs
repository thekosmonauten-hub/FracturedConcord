using UnityEngine;
using UnityEngine.EventSystems;

namespace PassiveTree
{
    /// <summary>
    /// Handles cell clicks for extension board cells
    /// Bridges the gap between CellController and ExtensionBoardController
    /// </summary>
    public class ExtensionBoardCellClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private ExtensionBoardController boardController;
        private Vector2Int cellPosition;
        
        /// <summary>
        /// Initialize the click handler
        /// </summary>
        public void Initialize(ExtensionBoardController controller, Vector2Int position)
        {
            boardController = controller;
            cellPosition = position;
        }
        
        /// <summary>
        /// Handle pointer click events
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (boardController != null)
            {
                boardController.OnCellClicked(cellPosition);
            }
        }
    }
}
