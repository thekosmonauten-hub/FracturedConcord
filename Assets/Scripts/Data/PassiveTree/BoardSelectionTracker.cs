using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PassiveTree
{
    /// <summary>
    /// Tracks selected boards to prevent duplicate selections and enforce tier-based selection rules
    /// Manages board selection state across the entire passive tree system
    /// </summary>
    public class BoardSelectionTracker : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool debugMode = true;
        
        // Track selected boards by tier and theme (using board names for actual created boards)
        private Dictionary<int, Dictionary<BoardTheme, string>> selectedBoardNamesByTier = new Dictionary<int, Dictionary<BoardTheme, string>>();
        
        // Track all selected board names for quick lookup
        private HashSet<string> allSelectedBoardNames = new HashSet<string>();
        
        // Track board data references for UI purposes
        private Dictionary<string, BoardData> boardNameToDataMap = new Dictionary<string, BoardData>();
        
        // Events
        public System.Action<BoardData, int, BoardTheme> OnBoardSelected;
        public System.Action<BoardData, int, BoardTheme> OnBoardDeselected;
        
        // Singleton pattern
        private static BoardSelectionTracker _instance;
        public static BoardSelectionTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BoardSelectionTracker>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("BoardSelectionTracker");
                        _instance = go.AddComponent<BoardSelectionTracker>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        void Awake()
        {
            // Ensure singleton
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeTracker();
        }
        
        /// <summary>
        /// Initialize the tracker with default values
        /// </summary>
        private void InitializeTracker()
        {
            selectedBoardNamesByTier.Clear();
            allSelectedBoardNames.Clear();
            boardNameToDataMap.Clear();
            
            if (debugMode)
            {
                Debug.Log("[BoardSelectionTracker] Initialized board selection tracker");
            }
        }
        
        /// <summary>
        /// Select a board for a specific tier and theme
        /// </summary>
        public bool SelectBoard(BoardData boardData, int tier, BoardTheme theme)
        {
            if (boardData == null)
            {
                Debug.LogError("[BoardSelectionTracker] Cannot select null board data");
                return false;
            }
            
            string boardName = boardData.BoardName;
            
            // Check if this theme is already selected for this tier
            if (IsThemeSelectedForTier(tier, theme))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[BoardSelectionTracker] Theme {theme} is already selected for tier {tier}");
                }
                return false;
            }
            
            // Check if this board is already selected
            if (allSelectedBoardNames.Contains(boardName))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[BoardSelectionTracker] Board {boardName} is already selected");
                }
                return false;
            }
            
            // Initialize tier dictionary if needed
            if (!selectedBoardNamesByTier.ContainsKey(tier))
            {
                selectedBoardNamesByTier[tier] = new Dictionary<BoardTheme, string>();
            }
            
            // Select the board
            selectedBoardNamesByTier[tier][theme] = boardName;
            allSelectedBoardNames.Add(boardName);
            boardNameToDataMap[boardName] = boardData;
            
            if (debugMode)
            {
                Debug.Log($"[BoardSelectionTracker] Selected board {boardName} for tier {tier}, theme {theme}");
            }
            
            OnBoardSelected?.Invoke(boardData, tier, theme);
            return true;
        }
        
        /// <summary>
        /// Deselect a board
        /// </summary>
        public bool DeselectBoard(BoardData boardData)
        {
            if (boardData == null)
            {
                Debug.LogError("[BoardSelectionTracker] Cannot deselect null board data");
                return false;
            }
            
            string boardName = boardData.BoardName;
            
            if (!allSelectedBoardNames.Contains(boardName))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[BoardSelectionTracker] Board {boardName} is not selected");
                }
                return false;
            }
            
            // Find and remove from tier tracking
            foreach (var tierPair in selectedBoardNamesByTier)
            {
                int tier = tierPair.Key;
                var themeDict = tierPair.Value;
                
                foreach (var themePair in themeDict.ToList())
                {
                    if (themePair.Value == boardName)
                    {
                        BoardTheme theme = themePair.Key;
                        themeDict.Remove(theme);
                        allSelectedBoardNames.Remove(boardName);
                        boardNameToDataMap.Remove(boardName);
                        
                        if (debugMode)
                        {
                            Debug.Log($"[BoardSelectionTracker] Deselected board {boardName} from tier {tier}, theme {theme}");
                        }
                        
                        OnBoardDeselected?.Invoke(boardData, tier, theme);
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a theme is already selected for a specific tier
        /// </summary>
        public bool IsThemeSelectedForTier(int tier, BoardTheme theme)
        {
            return selectedBoardNamesByTier.ContainsKey(tier) && 
                   selectedBoardNamesByTier[tier].ContainsKey(theme);
        }
        
        /// <summary>
        /// Check if a board is already selected
        /// </summary>
        public bool IsBoardSelected(BoardData boardData)
        {
            if (boardData == null) return false;
            return allSelectedBoardNames.Contains(boardData.BoardName);
        }
        
        /// <summary>
        /// Check if a board is already selected by name
        /// </summary>
        public bool IsBoardSelected(string boardName)
        {
            return allSelectedBoardNames.Contains(boardName);
        }
        
        /// <summary>
        /// Get the selected board for a specific tier and theme
        /// </summary>
        public BoardData GetSelectedBoard(int tier, BoardTheme theme)
        {
            if (selectedBoardNamesByTier.ContainsKey(tier) && 
                selectedBoardNamesByTier[tier].ContainsKey(theme))
            {
                string boardName = selectedBoardNamesByTier[tier][theme];
                if (boardNameToDataMap.ContainsKey(boardName))
                {
                    return boardNameToDataMap[boardName];
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get all selected boards for a specific tier
        /// </summary>
        public Dictionary<BoardTheme, BoardData> GetSelectedBoardsForTier(int tier)
        {
            var result = new Dictionary<BoardTheme, BoardData>();
            
            if (selectedBoardNamesByTier.ContainsKey(tier))
            {
                foreach (var themePair in selectedBoardNamesByTier[tier])
                {
                    string boardName = themePair.Value;
                    if (boardNameToDataMap.ContainsKey(boardName))
                    {
                        result[themePair.Key] = boardNameToDataMap[boardName];
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get all selected boards
        /// </summary>
        public HashSet<BoardData> GetAllSelectedBoards()
        {
            var result = new HashSet<BoardData>();
            foreach (string boardName in allSelectedBoardNames)
            {
                if (boardNameToDataMap.ContainsKey(boardName))
                {
                    result.Add(boardNameToDataMap[boardName]);
                }
            }
            return result;
        }
        
        /// <summary>
        /// Register a board when it's actually created in the scene
        /// This should be called when a board GameObject is instantiated
        /// </summary>
        public void RegisterCreatedBoard(string boardName, BoardData boardData, int tier, BoardTheme theme)
        {
            if (string.IsNullOrEmpty(boardName) || boardData == null)
            {
                Debug.LogError("[BoardSelectionTracker] Cannot register board with null name or data");
                return;
            }
            
            // Check if this theme is already selected for this tier
            if (IsThemeSelectedForTier(tier, theme))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[BoardSelectionTracker] Theme {theme} is already selected for tier {tier}. Cannot register duplicate board: {boardName}");
                }
                return;
            }
            
            // Update the board name to data mapping
            boardNameToDataMap[boardName] = boardData;
            
            // Mark as selected if not already
            if (!allSelectedBoardNames.Contains(boardName))
            {
                allSelectedBoardNames.Add(boardName);
                
                // Initialize tier dictionary if needed
                if (!selectedBoardNamesByTier.ContainsKey(tier))
                {
                    selectedBoardNamesByTier[tier] = new Dictionary<BoardTheme, string>();
                }
                
                selectedBoardNamesByTier[tier][theme] = boardName;
                
                if (debugMode)
                {
                    Debug.Log($"[BoardSelectionTracker] Registered created board: {boardName} for tier {tier}, theme {theme}");
                }
            }
        }
        
        /// <summary>
        /// Unregister a board when it's destroyed
        /// This should be called when a board GameObject is destroyed
        /// </summary>
        public void UnregisterBoard(string boardName)
        {
            if (string.IsNullOrEmpty(boardName))
            {
                Debug.LogError("[BoardSelectionTracker] Cannot unregister board with null name");
                return;
            }
            
            if (allSelectedBoardNames.Contains(boardName))
            {
                allSelectedBoardNames.Remove(boardName);
                boardNameToDataMap.Remove(boardName);
                
                // Remove from tier tracking
                foreach (var tierPair in selectedBoardNamesByTier)
                {
                    var themeDict = tierPair.Value;
                    foreach (var themePair in themeDict.ToList())
                    {
                        if (themePair.Value == boardName)
                        {
                            themeDict.Remove(themePair.Key);
                            break;
                        }
                    }
                }
                
                if (debugMode)
                {
                    Debug.Log($"[BoardSelectionTracker] Unregistered board: {boardName}");
                }
            }
        }
        
        /// <summary>
        /// Check if a board can be created (not duplicate theme for tier)
        /// </summary>
        public bool CanCreateBoard(string boardName, int tier, BoardTheme theme)
        {
            if (string.IsNullOrEmpty(boardName))
            {
                Debug.LogError("[BoardSelectionTracker] Cannot check board creation with null name");
                return false;
            }
            
            // Check if this theme is already selected for this tier
            if (IsThemeSelectedForTier(tier, theme))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[BoardSelectionTracker] Cannot create board '{boardName}' - Theme {theme} is already selected for tier {tier}");
                }
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get available boards for selection (filtered by tier and theme restrictions)
        /// </summary>
        public List<BoardData> GetAvailableBoards(List<BoardData> allBoards, int tier, BoardTheme theme)
        {
            return allBoards.Where(board => 
                board != null && 
                board.IsUnlocked && 
                !IsBoardSelected(board) && 
                !IsThemeSelectedForTier(tier, theme)
            ).ToList();
        }
        
        /// <summary>
        /// Get available boards for selection (any tier/theme)
        /// </summary>
        public List<BoardData> GetAvailableBoards(List<BoardData> allBoards)
        {
            return allBoards.Where(board => 
                board != null && 
                board.IsUnlocked && 
                !IsBoardSelected(board)
            ).ToList();
        }
        
        /// <summary>
        /// Clear all selections
        /// </summary>
        public void ClearAllSelections()
        {
            selectedBoardNamesByTier.Clear();
            allSelectedBoardNames.Clear();
            boardNameToDataMap.Clear();
            
            if (debugMode)
            {
                Debug.Log("[BoardSelectionTracker] Cleared all board selections");
            }
        }
        
        /// <summary>
        /// Get selection statistics
        /// </summary>
        public SelectionStatistics GetSelectionStatistics()
        {
            int totalSelected = allSelectedBoardNames.Count;
            int totalTiers = selectedBoardNamesByTier.Count;
            int totalThemes = selectedBoardNamesByTier.Values.Sum(tierDict => tierDict.Count);
            
            return new SelectionStatistics
            {
                TotalSelectedBoards = totalSelected,
                TotalTiers = totalTiers,
                TotalThemes = totalThemes,
                SelectedBoardsByTier = new Dictionary<int, int>(selectedBoardNamesByTier.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Count
                ))
            };
        }
        
        /// <summary>
        /// Debug method to print current selection state
        /// </summary>
        [ContextMenu("Debug Selection State")]
        public void DebugSelectionState()
        {
            Debug.Log("=== BOARD SELECTION TRACKER DEBUG ===");
            Debug.Log($"Total selected boards: {allSelectedBoardNames.Count}");
            Debug.Log($"Total tiers with selections: {selectedBoardNamesByTier.Count}");
            
            foreach (var tierPair in selectedBoardNamesByTier)
            {
                int tier = tierPair.Key;
                var themeDict = tierPair.Value;
                Debug.Log($"Tier {tier}: {themeDict.Count} themes selected");
                
                foreach (var themePair in themeDict)
                {
                    Debug.Log($"  - {themePair.Key}: {themePair.Value}");
                }
            }
            
            Debug.Log("=== END SELECTION DEBUG ===");
        }
        
        /// <summary>
        /// Selection statistics data structure
        /// </summary>
        [System.Serializable]
        public class SelectionStatistics
        {
            public int TotalSelectedBoards;
            public int TotalTiers;
            public int TotalThemes;
            public Dictionary<int, int> SelectedBoardsByTier;
        }
    }
}
