using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Represents a connection between two boards
    /// </summary>
    [System.Serializable]
    public class BoardConnection
    {
        [Header("Connection Identity")]
        public string extensionPointId;    // Source extension point
        public string boardId;             // Connected board ID
        
        [Header("Board Transform")]
        public int boardRotation = 0;      // Rotation (0, 90, 180, 270)
        public BoardFlip boardFlip = BoardFlip.None;
        public Vector2 boardPosition;      // Position offset
        
        [Header("State")]
        public bool isActive = true;
        
        /// <summary>
        /// Create a new board connection
        /// </summary>
        public BoardConnection(string extensionPointId, string boardId)
        {
            this.extensionPointId = extensionPointId;
            this.boardId = boardId;
            this.boardRotation = 0;
            this.boardFlip = BoardFlip.None;
            this.boardPosition = Vector2.zero;
            this.isActive = true;
        }
        
        /// <summary>
        /// Get the rotation as a Quaternion
        /// </summary>
        public Quaternion GetRotation()
        {
            return Quaternion.Euler(0, 0, boardRotation);
        }
        
        /// <summary>
        /// Get the scale based on flip settings
        /// </summary>
        public Vector3 GetScale()
        {
            Vector3 scale = Vector3.one;
            
            switch (boardFlip)
            {
                case BoardFlip.Horizontal:
                    scale.x = -1;
                    break;
                case BoardFlip.Vertical:
                    scale.y = -1;
                    break;
                case BoardFlip.Both:
                    scale.x = -1;
                    scale.y = -1;
                    break;
            }
            
            return scale;
        }
        
        /// <summary>
        /// Check if this connection is valid
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(extensionPointId) && 
                   !string.IsNullOrEmpty(boardId) && 
                   isActive;
        }
    }
    
    /// <summary>
    /// Board flip options
    /// </summary>
    public enum BoardFlip
    {
        None,
        Horizontal,
        Vertical,
        Both
    }
}
