using UnityEngine;
using System.Collections.Generic;

namespace PassiveTree
{
    /// <summary>
    /// Defines the data for a passive tree node type
    /// </summary>
    [CreateAssetMenu(fileName = "New Passive Node", menuName = "Passive Tree/Node Data")]
    public class PassiveNodeData : ScriptableObject
    {
        [Header("Node Identity")]
        [SerializeField] private string _nodeName = "New Node";
        [SerializeField] private string _description = "Node description";
        [SerializeField] private NodeType _nodeType = NodeType.Travel;
        
        [Header("Visual")]
        [SerializeField] private Sprite _nodeSprite;
        [SerializeField] private Color _nodeColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private Color _selectedColor = Color.cyan;
        
        [Header("Gameplay")]
        [SerializeField] private bool _isUnlocked = false;
        [SerializeField] private bool _isStartNode = false;
        [SerializeField] private int _skillPointsCost = 1;
        [SerializeField] private int _maxRank = 1;
        [SerializeField] private string[] _connectedNodeIds;
        
        [Header("Effects")]
        [SerializeField] private string _effectDescription = "No effect";
        [SerializeField] private bool _hasSpecialEffect = false;
        [SerializeField] private Dictionary<string, float> _stats = new Dictionary<string, float>();
        
        // Properties
        public string NodeName => _nodeName;
        public string Description => _description;
        public NodeType NodeType => _nodeType;
        public Sprite NodeSprite => _nodeSprite;
        public Color NodeColor => _nodeColor;
        public Color HighlightColor => _highlightColor;
        public Color SelectedColor => _selectedColor;
        public bool IsUnlocked => _isUnlocked;
        public bool IsStartNode => _isStartNode;
        public int SkillPointsCost => _skillPointsCost;
        public string[] ConnectedNodeIds => _connectedNodeIds;
        public string EffectDescription => _effectDescription;
        public bool HasSpecialEffect => _hasSpecialEffect;
        
        // Additional properties for compatibility
        public int MaxRank => _maxRank;
        public int SkillPointCost => _skillPointsCost;
        
        /// <summary>
        /// Get the appropriate color based on node state
        /// </summary>
        public Color GetColorForState(NodeState state)
        {
            switch (state)
            {
                case NodeState.Normal:
                    return _nodeColor;
                case NodeState.Highlighted:
                    return _highlightColor;
                case NodeState.Selected:
                    return _selectedColor;
                case NodeState.Locked:
                    return Color.gray;
                default:
                    return _nodeColor;
            }
        }
        
        /// <summary>
        /// Check if this node can connect to another node
        /// </summary>
        public bool CanConnectTo(string targetNodeId)
        {
            if (_connectedNodeIds == null) return false;
            return System.Array.Exists(_connectedNodeIds, id => id == targetNodeId);
        }
        
        /// <summary>
        /// Get stats for this node
        /// </summary>
        public Dictionary<string, float> GetStats()
        {
            return _stats;
        }
        
        /// <summary>
        /// Create a simple node data instance for compatibility
        /// </summary>
        public static PassiveNodeData CreateSimpleNode(string nodeId, string nodeName, string description, NodeType nodeType, int cost, int maxRank, Dictionary<string, float> stats)
        {
            var nodeData = CreateInstance<PassiveNodeData>();
            
            // Use reflection to set private fields
            var nameField = typeof(PassiveNodeData).GetField("_nodeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descField = typeof(PassiveNodeData).GetField("_description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var typeField = typeof(PassiveNodeData).GetField("_nodeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var costField = typeof(PassiveNodeData).GetField("_skillPointsCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxRankField = typeof(PassiveNodeData).GetField("_maxRank", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var statsField = typeof(PassiveNodeData).GetField("_stats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (nameField != null) nameField.SetValue(nodeData, nodeName);
            if (descField != null) descField.SetValue(nodeData, description);
            if (typeField != null) typeField.SetValue(nodeData, nodeType);
            if (costField != null) costField.SetValue(nodeData, cost);
            if (maxRankField != null) maxRankField.SetValue(nodeData, maxRank);
            if (statsField != null) statsField.SetValue(nodeData, stats);
            
            return nodeData;
        }
    }
    
    /// <summary>
    /// Types of passive tree nodes
    /// </summary>
    public enum NodeType
    {
        Start,          // Starting node
        Travel,         // Basic travel node
        Extension,      // Extension point
        Notable,        // Notable passive
        Keystone,       // Major keystone
        Mastery,        // Mastery node
        Jewel,          // Jewel socket
        Cluster,        // Cluster jewel
        Main,           // Main node (legacy compatibility)
        Small           // Small node (legacy compatibility)
    }
    
    /// <summary>
    /// Current state of a node
    /// </summary>
    public enum NodeState
    {
        Normal,
        Highlighted,
        Selected,
        Locked,
        Unlocked
    }
}
