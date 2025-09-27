using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Represents a single passive tree node on a tile
    /// </summary>
    public class PassiveNode : MonoBehaviour
    {
        [Header("Node Data")]
        [SerializeField] private PassiveNodeData _nodeData;
        [SerializeField] private string _nodeId;
        [SerializeField] private Vector3Int _gridPosition;
        
        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Tilemap _parentTilemap;
        
        [Header("State")]
        [SerializeField] private NodeState _currentState = NodeState.Normal;
        [SerializeField] private bool _isUnlocked = false;
        [SerializeField] private bool _isHighlighted = false;
        [SerializeField] private bool _isSelected = false;
        
        // Events
        public System.Action<PassiveNode> OnNodeClicked;
        public System.Action<PassiveNode> OnNodeSelected;
        public System.Action<PassiveNode> OnNodeUnlocked;
        
        // Properties
        public PassiveNodeData NodeData => _nodeData;
        public string NodeId => _nodeId;
        public Vector3Int GridPosition => _gridPosition;
        public NodeState CurrentState => _currentState;
        public bool IsUnlocked => _isUnlocked;
        public bool IsHighlighted => _isHighlighted;
        public bool IsSelected => _isSelected;
        
        // Legacy compatibility properties
        public string id => _nodeId;
        public string description => _nodeData?.Description ?? "";
        public Vector2Int position => new Vector2Int(_gridPosition.x, _gridPosition.y);
        public NodeType type => _nodeData?.NodeType ?? NodeType.Travel;
        public int cost => _nodeData?.SkillPointCost ?? 1;
        public int maxRank => _nodeData?.MaxRank ?? 1;
        public int currentRank => _isUnlocked ? 1 : 0;
        public Dictionary<string, float> stats => _nodeData?.GetStats() ?? new Dictionary<string, float>();
        
        // Additional properties for compatibility
        public bool CanAllocate => !_isUnlocked;
        public bool CanDeallocate => _isUnlocked;
        public string[] requirements => new string[0]; // TODO: Implement requirements
        

        
        /// <summary>
        /// Check if node is adjacent to allocated node (compatibility method)
        /// </summary>
        public bool IsAdjacentToAllocatedNode()
        {
            // Use the ExtensionBoardDataManager to check adjacency
            // The CanAllocatePosition method checks all boards for adjacency, so we don't need boardId
            if (ExtensionBoardDataManager.Instance != null)
            {
                return ExtensionBoardDataManager.Instance.CanAllocatePosition(new Vector2Int(GridPosition.x, GridPosition.y), null);
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if node is adjacent to allocated node with parameters (compatibility method)
        /// </summary>
        public bool IsAdjacentToAllocatedNode(string[] allocatedNodeIds, object board)
        {
            // Use the basic adjacency check
            return IsAdjacentToAllocatedNode();
        }
        
        /// <summary>
        /// Check if node is adjacent to allocated node with List<string> parameters (compatibility method)
        /// </summary>
        public bool IsAdjacentToAllocatedNode(System.Collections.Generic.List<string> allocatedNodeIds, object board)
        {
            // Use the basic adjacency check
            return IsAdjacentToAllocatedNode();
        }
        
        void Awake()
        {
            // Auto-find components if not assigned
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (_parentTilemap == null)
                _parentTilemap = GetComponentInParent<Tilemap>();
        }
        
        void Start()
        {
            // Initialize the node with its data
            if (_nodeData != null)
            {
                InitializeNode(_nodeData);
            }
        }
        
        /// <summary>
        /// Initialize the node with data
        /// </summary>
        public void InitializeNode(PassiveNodeData nodeData)
        {
            _nodeData = nodeData;
            _nodeId = nodeData.name;
            _isUnlocked = nodeData.IsUnlocked;
            
            // Set visual properties
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = nodeData.NodeSprite;
                _spriteRenderer.color = nodeData.GetColorForState(_currentState);
            }
            
            // Update state
            UpdateNodeState();
            
            Debug.Log($"Initialized node: {_nodeId} at position {_gridPosition}");
        }
        
        /// <summary>
        /// Set the grid position of this node
        /// </summary>
        public void SetGridPosition(Vector3Int position)
        {
            _gridPosition = position;
            
            // Only update transform if this node is actually instantiated as a GameObject
            if (transform != null && _parentTilemap != null)
            {
                transform.position = _parentTilemap.GetCellCenterWorld(position);
            }
        }
        
        /// <summary>
        /// Update the node's visual state
        /// </summary>
        public void UpdateNodeState()
        {
            if (_spriteRenderer == null || _nodeData == null) return;
            
            // Determine current state
            if (_isSelected)
                _currentState = NodeState.Selected;
            else if (_isHighlighted)
                _currentState = NodeState.Highlighted;
            else if (_isUnlocked)
                _currentState = NodeState.Unlocked;
            else
                _currentState = NodeState.Normal;
            
            // Apply color based on state
            _spriteRenderer.color = _nodeData.GetColorForState(_currentState);
        }
        
        /// <summary>
        /// Set the unlocked state of this node
        /// </summary>
        public void SetUnlocked(bool unlocked)
        {
            _isUnlocked = unlocked;
            UpdateNodeState();
            
            if (unlocked)
            {
                OnNodeUnlocked?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Set the highlighted state of this node
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            _isHighlighted = highlighted;
            UpdateNodeState();
        }
        
        /// <summary>
        /// Deallocate this node (set to locked state)
        /// </summary>
        public bool Deallocate()
        {
            if (_isUnlocked)
            {
                _isUnlocked = false;
                UpdateNodeState();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Set node ID (compatibility method)
        /// </summary>
        public void SetId(string newId)
        {
            _nodeId = newId;
        }
        
        /// <summary>
        /// Set node description (compatibility method)
        /// </summary>
        public void SetDescription(string newDescription)
        {
            // Note: This modifies the node data, not the node itself
            if (_nodeData != null)
            {
                // Use reflection to set private field
                var descField = typeof(PassiveNodeData).GetField("_description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (descField != null)
                    descField.SetValue(_nodeData, newDescription);
            }
        }
        
        /// <summary>
        /// Set node position (compatibility method)
        /// </summary>
        public void SetPosition(Vector2Int newPosition)
        {
            _gridPosition = new Vector3Int(newPosition.x, newPosition.y, 0);
        }
        
        /// <summary>
        /// Set node type (compatibility method)
        /// </summary>
        public void SetType(NodeType newType)
        {
            // Note: This modifies the node data, not the node itself
            if (_nodeData != null)
            {
                // Use reflection to set private field
                var typeField = typeof(PassiveNodeData).GetField("_nodeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (typeField != null)
                    typeField.SetValue(_nodeData, newType);
            }
        }
        
        /// <summary>
        /// Set node cost (compatibility method)
        /// </summary>
        public void SetCost(int newCost)
        {
            // Note: This modifies the node data, not the node itself
            if (_nodeData != null)
            {
                // Use reflection to set private field
                var costField = typeof(PassiveNodeData).GetField("_skillPointsCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (costField != null)
                    costField.SetValue(_nodeData, newCost);
            }
        }
        
        /// <summary>
        /// Set node max rank (compatibility method)
        /// </summary>
        public void SetMaxRank(int newMaxRank)
        {
            // Note: This modifies the node data, not the node itself
            if (_nodeData != null)
            {
                // Use reflection to set private field
                var maxRankField = typeof(PassiveNodeData).GetField("_maxRank", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (maxRankField != null)
                    maxRankField.SetValue(_nodeData, newMaxRank);
            }
        }
        
        /// <summary>
        /// Set node stats (compatibility method)
        /// </summary>
        public void SetStats(Dictionary<string, float> newStats)
        {
            // Note: This modifies the node data, not the node itself
            if (_nodeData != null)
            {
                // Use reflection to set private field
                var statsField = typeof(PassiveNodeData).GetField("_stats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (statsField != null)
                    statsField.SetValue(_nodeData, newStats);
            }
        }
        
        /// <summary>
        /// Set current rank (compatibility method)
        /// </summary>
        public void SetCurrentRank(int newRank)
        {
            _isUnlocked = (newRank > 0);
            UpdateNodeState();
        }
        
        /// <summary>
        /// Set the selected state of this node
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateNodeState();
            
            if (selected)
            {
                OnNodeSelected?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Handle mouse click on this node
        /// </summary>
        void OnMouseDown()
        {
            OnNodeClicked?.Invoke(this);
        }
        
        /// <summary>
        /// Handle mouse enter (highlight)
        /// </summary>
        void OnMouseEnter()
        {
            SetHighlighted(true);
        }
        
        /// <summary>
        /// Handle mouse exit (remove highlight)
        /// </summary>
        void OnMouseExit()
        {
            SetHighlighted(false);
        }
        
        /// <summary>
        /// Get the current stats from this node
        /// </summary>
        public Dictionary<string, float> GetCurrentStats()
        {
            if (_nodeData == null || !_isUnlocked)
                return new Dictionary<string, float>();
            
            return _nodeData.GetStats();
        }
        
        /// <summary>
        /// Check if this node can be unlocked
        /// </summary>
        public bool CanUnlock()
        {
            // Add your unlock logic here
            // For example, check if player has enough skill points
            return !_isUnlocked;
        }
        
        /// <summary>
        /// Unlock this node
        /// </summary>
        public bool Unlock()
        {
            if (!CanUnlock())
                return false;
            
            SetUnlocked(true);
            return true;
        }
        
        /// <summary>
        /// Get a string representation of this node
        /// </summary>
        public override string ToString()
        {
            return $"PassiveNode '{_nodeId}' at {_gridPosition} - {_currentState}";
        }
    }
}