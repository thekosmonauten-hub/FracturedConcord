using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Owns multiple warrant board "pages" (loadouts) for a single character and exposes
/// helper APIs for assigning/removing warrants plus save/load helpers.
/// </summary>
public class WarrantBoardStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WarrantBoardGraphDefinition graphDefinition;

    [Header("Page Settings")]
    [SerializeField, Min(1)] private int defaultPageCount = 3;
    [SerializeField] private string pageNamePrefix = "Loadout";
    [SerializeField] private bool autoCreatePagesOnAwake = true;

    [Header("Persistence")]
    [SerializeField] private string playerPrefsKey = "WarrantBoardState";

    [Header("Debug/Testing")]
    [Tooltip("If enabled, nodes can be unlocked without spending skill points. Useful for testing.")]
    [SerializeField] private bool unlimitedSkillPoints = false;

    [SerializeField] private List<WarrantBoardPageData> pages = new List<WarrantBoardPageData>();
    [SerializeField] private int activePageIndex;

    private readonly HashSet<string> validSocketIds = new HashSet<string>();

    /// <summary>
    /// Event fired when a warrant is assigned or removed from the board.
    /// Subscribers can refresh their displays when warrants change.
    /// </summary>
    public event Action OnWarrantChanged;

    public IReadOnlyList<WarrantBoardPageData> Pages => pages;
    public int ActivePageIndex => Mathf.Clamp(activePageIndex, 0, pages.Count > 0 ? pages.Count - 1 : 0);
    public WarrantBoardPageData ActivePage => pages.Count == 0 ? null : pages[ActivePageIndex];
    public bool IsUnlimitedSkillPointsMode => unlimitedSkillPoints;

    private void Awake()
    {
        CacheValidSockets();
        if (autoCreatePagesOnAwake && pages.Count == 0)
        {
            EnsurePageCount(defaultPageCount);
        }
        ClampActiveIndex();
        
        // Try to load warrant board state from Character save data
        LoadFromCharacterData();
        
        // Sync initial state to WarrantsManager
        SyncToWarrantsManager();
    }
    
    private void Start()
    {
        // Defer heavy initialization to prevent blocking scene load
        StartCoroutine(DeferredStart());
    }
    
    /// <summary>
    /// Defer heavy initialization to spread across frames.
    /// </summary>
    private System.Collections.IEnumerator DeferredStart()
    {
        // Wait a frame to ensure all components are initialized
        yield return null;
        
        // Refresh character warrant modifiers after Start() to ensure all components are initialized
        // This ensures warrants from locker grid are loaded and board state is ready
        RefreshCharacterWarrantModifiers();
    }
    
    /// <summary>
    /// Load warrant board state from Character save data if available
    /// </summary>
    private void LoadFromCharacterData()
    {
        // First, try to load from WarrantsManager (most up-to-date, persists across scenes)
        var warrantsManager = WarrantsManager.Instance;
        if (warrantsManager != null)
        {
            string stateJson = warrantsManager.GetWarrantBoardStateJson();
            if (!string.IsNullOrEmpty(stateJson))
            {
                LoadFromJson(stateJson);
                Debug.Log($"[WarrantBoardStateController] Loaded warrant board state from WarrantsManager ({stateJson.Length} chars)");
                return;
            }
        }
        
        // Fallback: Try to load from Character save data
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            Character character = charManager.GetCurrentCharacter();
            if (character != null)
            {
                // Get CharacterData from save system to access warrant board state JSON
                var saveSystem = CharacterSaveSystem.Instance;
                if (saveSystem != null)
                {
                    CharacterData characterData = saveSystem.GetCharacter(character.characterName);
                    if (characterData != null && !string.IsNullOrEmpty(characterData.warrantBoardStateJson))
                    {
                        LoadFromJson(characterData.warrantBoardStateJson);
                        // Also sync to WarrantsManager for next time
                        if (warrantsManager != null)
                        {
                            warrantsManager.SetWarrantBoardStateJson(characterData.warrantBoardStateJson);
                        }
                        Debug.Log($"[WarrantBoardStateController] Loaded warrant board state from Character save data ({characterData.warrantBoardStateJson.Length} chars)");
                        return;
                    }
                }
            }
        }
        
        // Final fallback: Try loading from PlayerPrefs (per-character key)
        LoadFromPlayerPrefs();
    }

    private void OnValidate()
    {
        CacheValidSockets();
        ClampActiveIndex();
    }
    
    private void OnDestroy()
    {
        // Sync state to WarrantsManager before scene is destroyed
        // This ensures the state is persisted even if Character.ToCharacterData() is called
        // after the scene is unloaded (where FindFirstObjectByType won't find us)
        SyncToWarrantsManager();
        
        // Also save to PlayerPrefs as backup
        SaveToPlayerPrefs();
        
        // Force save character to ensure state is persisted
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            charManager.SaveCharacter();
            Debug.Log("[WarrantBoardStateController] Saved character on scene destroy to persist warrant board state");
        }
        
        Debug.Log("[WarrantBoardStateController] Synced warrant board state to WarrantsManager and PlayerPrefs on scene destroy");
    }
    
    private void OnDisable()
    {
        // Also sync on disable (in case OnDestroy isn't called in some scenarios)
        // This is a safety net to ensure state is saved
        SyncToWarrantsManager();
    }
    
    /// <summary>
    /// Syncs the current warrant board state to WarrantsManager singleton.
    /// This ensures the state persists across scene changes.
    /// </summary>
    private void SyncToWarrantsManager()
    {
        var warrantsManager = WarrantsManager.Instance;
        if (warrantsManager != null)
        {
            string json = ToJson(false);
            warrantsManager.SetWarrantBoardStateJson(json);
            
            // Debug: Log unlocked nodes count for verification
            if (ActivePage != null)
            {
                int unlockedCount = ActivePage.UnlockedNodeCount;
                Debug.Log($"[WarrantBoardStateController] Synced state to WarrantsManager: {unlockedCount} unlocked nodes on active page");
            }
        }
        else
        {
            Debug.LogWarning("[WarrantBoardStateController] WarrantsManager.Instance is null - cannot sync state!");
        }
    }

    /// <summary>
    /// Ensures there are at least the requested number of board pages (loadouts).
    /// </summary>
    public void EnsurePageCount(int targetCount)
    {
        targetCount = Mathf.Max(1, targetCount);
        while (pages.Count < targetCount)
        {
            pages.Add(CreatePageData(pages.Count));
        }
        ClampActiveIndex();
    }

    /// <summary>
    /// Switches the active page index.
    /// </summary>
    public bool SetActivePage(int index)
    {
        if (index < 0 || index >= pages.Count)
            return false;

        if (activePageIndex == index)
            return true;

        activePageIndex = index;
        
        // Notify subscribers that the active page changed (which affects displayed warrants)
        OnWarrantChanged?.Invoke();
        
        return true;
    }

    /// <summary>
    /// Assigns a warrant (by definition ID) to a specific node on the active page.
    /// Only works if the socket node is unlocked.
    /// </summary>
    public bool TryAssignWarrant(string nodeId, string warrantId)
    {
        if (!IsSocketNode(nodeId) || ActivePage == null)
            return false;

        // Can only assign to unlocked socket nodes
        if (!ActivePage.IsNodeUnlocked(nodeId))
            return false;

        ActivePage.SetAssignment(nodeId, warrantId);
        
        // Sync state to WarrantsManager
        SyncToWarrantsManager();
        
        // Refresh character warrant modifiers
        RefreshCharacterWarrantModifiers();
        
        // Notify subscribers that a warrant was changed
        OnWarrantChanged?.Invoke();
        
        // Save character to persist warrant board state
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            charManager.SaveCharacter();
        }
        
        return true;
    }

    /// <summary>
    /// Removes any warrant assigned to the node on the active page.
    /// </summary>
    public bool TryRemoveWarrant(string nodeId)
    {
        if (ActivePage == null)
            return false;

        bool removed = ActivePage.RemoveAssignment(nodeId);
        
        if (removed)
        {
            // Sync state to WarrantsManager
            SyncToWarrantsManager();
            
            // Refresh character warrant modifiers
            RefreshCharacterWarrantModifiers();
            
            // Notify subscribers that a warrant was changed
            OnWarrantChanged?.Invoke();
            
            // Save character to persist warrant board state
            var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
            if (charManager != null && charManager.HasCharacter())
            {
                charManager.SaveCharacter();
            }
        }
        
        return removed;
    }

    public string GetWarrantAtNode(string nodeId)
    {
        return ActivePage == null ? null : ActivePage.GetWarrant(nodeId);
    }

    public void ClearActivePage()
    {
        ActivePage?.ClearAssignments();
    }

    /// <summary>
    /// Serializes the current pages/active slot to JSON for storage.
    /// </summary>
    public string ToJson(bool prettyPrint = false)
    {
        var data = new WarrantBoardSaveData
        {
            activePageIndex = ActivePageIndex,
            pages = pages.Select(p => p.Clone()).ToList()
        };
        return JsonUtility.ToJson(data, prettyPrint);
    }

    /// <summary>
    /// Restores the controller from a JSON string produced by <see cref="ToJson"/>.
    /// </summary>
    public void LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        var data = JsonUtility.FromJson<WarrantBoardSaveData>(json);
        if (data == null)
            return;

        pages = data.pages ?? new List<WarrantBoardPageData>();
        activePageIndex = data.activePageIndex;
        ClampActiveIndex();
        
        // IMPORTANT: Rebuild cache for all pages after loading
        // The cache might be stale after deserialization
        foreach (var page in pages)
        {
            if (page != null)
            {
                // Force cache rebuild after deserialization
                page.RebuildUnlockedCache();
            }
        }
        
        // Debug: Log loaded state
        if (ActivePage != null)
        {
            int unlockedCount = ActivePage.UnlockedNodeCount;
            Debug.Log($"[WarrantBoardStateController] Loaded state: {unlockedCount} unlocked nodes on active page (page {activePageIndex + 1}/{pages.Count})");
            
            // Debug: Log the actual unlocked node IDs for verification
            if (unlockedCount > 0)
            {
                var unlockedIds = string.Join(", ", ActivePage.UnlockedNodeIds);
                Debug.Log($"[WarrantBoardStateController] Unlocked node IDs: [{unlockedIds}]");
            }
        }
        
        // Sync loaded state to WarrantsManager
        SyncToWarrantsManager();
        
        // Refresh character warrant modifiers after loading state
        // This ensures stats from socketed warrants are applied to the character
        RefreshCharacterWarrantModifiers();
        
        // Note: Effect views and socket views will be refreshed when graph is built (via OnGraphBuilt callback)
        // This ensures they refresh after all views are created and configured
    }
    
    /// <summary>
    /// Called by WarrantBoardGraphBuilder when the graph is finished building.
    /// Refreshes all effect views and socket views to sync their state with the loaded board state.
    /// </summary>
    public void OnGraphBuilt()
    {
        RefreshAllEffectViews();
        RefreshAllSocketViews();
        
        // Refresh character warrant modifiers after graph is built and socket views are synced
        // This ensures stats from socketed warrants are applied to the character
        RefreshCharacterWarrantModifiers();
    }
    
    /// <summary>
    /// Refreshes all WarrantEffectView components in the scene to sync their lock state.
    /// Called after graph is built to ensure all views are created and configured.
    /// </summary>
    private void RefreshAllEffectViews()
    {
        // Find all WarrantEffectView components and refresh their lock state
        WarrantEffectView[] effectViews = FindObjectsByType<WarrantEffectView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int refreshedCount = 0;
        foreach (var effectView in effectViews)
        {
            if (effectView != null)
            {
                // Sync lock state (effectView should already have boardState reference from Configure)
                effectView.SyncLockState();
                refreshedCount++;
            }
        }
        if (refreshedCount > 0)
        {
            Debug.Log($"[WarrantBoardStateController] Refreshed {refreshedCount} effect views after graph built");
        }
    }
    
    /// <summary>
    /// Refreshes all WarrantSocketView components in the scene to sync their assigned warrants.
    /// Called after graph is built to ensure socketed warrants are displayed correctly.
    /// </summary>
    private void RefreshAllSocketViews()
    {
        // Find all WarrantSocketView components and refresh their assigned warrants
        WarrantSocketView[] socketViews = FindObjectsByType<WarrantSocketView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int refreshedCount = 0;
        int assignedCount = 0;
        foreach (var socketView in socketViews)
        {
            if (socketView != null)
            {
                // Sync assigned warrant from board state (socketView should already have boardState reference from Configure)
                socketView.SyncFromState();
                refreshedCount++;
                if (!string.IsNullOrEmpty(socketView.WarrantId))
                {
                    assignedCount++;
                }
            }
        }
        if (refreshedCount > 0)
        {
            Debug.Log($"[WarrantBoardStateController] Refreshed {refreshedCount} socket views after graph built ({assignedCount} with assigned warrants)");
        }
    }

    [ContextMenu("Save To PlayerPrefs")]
    public void SaveToPlayerPrefs()
    {
        if (string.IsNullOrEmpty(playerPrefsKey))
            return;

        // Use per-character key if character is loaded
        string key = GetPerCharacterKey();
        PlayerPrefs.SetString(key, ToJson(false));
        PlayerPrefs.Save();
        Debug.Log($"[WarrantBoardStateController] Saved to PlayerPrefs with key: {key}");
    }

    [ContextMenu("Load From PlayerPrefs")]
    public void LoadFromPlayerPrefs()
    {
        string key = GetPerCharacterKey();
        if (!PlayerPrefs.HasKey(key))
        {
            // Fallback to old global key for backward compatibility
            if (!string.IsNullOrEmpty(playerPrefsKey) && PlayerPrefs.HasKey(playerPrefsKey))
            {
                LoadFromJson(PlayerPrefs.GetString(playerPrefsKey));
                Debug.Log($"[WarrantBoardStateController] Loaded from legacy PlayerPrefs key: {playerPrefsKey}");
                return;
            }
            return;
        }

        LoadFromJson(PlayerPrefs.GetString(key));
        Debug.Log($"[WarrantBoardStateController] Loaded from PlayerPrefs with key: {key}");
    }
    
    /// <summary>
    /// Get per-character PlayerPrefs key for warrant board state
    /// </summary>
    private string GetPerCharacterKey()
    {
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            Character character = charManager.GetCurrentCharacter();
            if (character != null)
            {
                return $"{playerPrefsKey}_{character.characterName}";
            }
        }
        // Fallback to global key if no character loaded
        return playerPrefsKey;
    }

    public WarrantBoardPageData DuplicateActivePage(string newDisplayName = null)
    {
        if (ActivePage == null)
            return null;

        var duplicate = ActivePage.Clone(Guid.NewGuid().ToString("N"), newDisplayName ?? $"{pageNamePrefix} {pages.Count + 1}");
        pages.Add(duplicate);
        activePageIndex = pages.Count - 1;
        return duplicate;
    }

    /// <summary>
    /// Checks if a node can be unlocked (has adjacent unlocked node and enough skill points).
    /// </summary>
    public bool CanUnlockNode(string nodeId, int availableSkillPoints, WarrantBoardRuntimeGraph runtimeGraph)
    {
        if (ActivePage == null || string.IsNullOrEmpty(nodeId))
            return false;

        // Already unlocked?
        if (ActivePage.IsNodeUnlocked(nodeId))
            return false;

        // Need at least 1 skill point (unless debug mode is enabled)
        if (!unlimitedSkillPoints && availableSkillPoints < 1)
            return false;

        // Anchor is always unlocked (handled in constructor)
        if (nodeId == "Anchor")
            return false;

        // Check if any adjacent node is unlocked
        if (runtimeGraph == null || !runtimeGraph.Nodes.TryGetValue(nodeId, out var targetNode))
            return false;

        // Check all connected nodes
        foreach (var connection in targetNode.Connections)
        {
            if (ActivePage.IsNodeUnlocked(connection.Id))
                return true; // Found an adjacent unlocked node
        }

        // Also check reverse connections (bidirectional edges)
        foreach (var kvp in runtimeGraph.Nodes)
        {
            if (kvp.Value.Connections.Contains(targetNode) && ActivePage.IsNodeUnlocked(kvp.Key))
                return true; // Found a node that connects to this one
        }

        return false; // No adjacent unlocked nodes found
    }

    /// <summary>
    /// Attempts to unlock a node. Requires adjacent unlocked node and 1 skill point.
    /// </summary>
    public bool TryUnlockNode(string nodeId, WarrantBoardRuntimeGraph runtimeGraph, ref int skillPoints)
    {
        if (!CanUnlockNode(nodeId, skillPoints, runtimeGraph))
            return false;

        if (ActivePage == null)
            return false;

        if (ActivePage.UnlockNode(nodeId))
        {
            // Only consume skill points if not in debug mode
            if (!unlimitedSkillPoints)
            {
                skillPoints -= 1;
            }
            
            Debug.Log($"[WarrantBoardStateController] ✅ Unlocked node '{nodeId}'. Active page now has {ActivePage.UnlockedNodeCount} unlocked nodes");
            
            // Sync state to WarrantsManager FIRST (important: unlocks effect nodes)
            // This must happen before SaveCharacter() so WarrantsManager has the latest state
            SyncToWarrantsManager();
            
            // Refresh character warrant modifiers (unlocking nodes may affect effect nodes)
            RefreshCharacterWarrantModifiers();
            
            // Save character to persist warrant board state
            // Character.ToCharacterData() will read from WarrantsManager, which now has the latest state
            var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
            if (charManager != null && charManager.HasCharacter())
            {
                charManager.SaveCharacter();
                Debug.Log($"[WarrantBoardStateController] Saved character after unlocking node '{nodeId}'");
            }
            
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Refresh character warrant modifiers after board changes
    /// </summary>
    private void RefreshCharacterWarrantModifiers()
    {
        var charManager = CharacterManager.Instance ?? FindFirstObjectByType<CharacterManager>();
        if (charManager != null && charManager.HasCharacter())
        {
            Character character = charManager.GetCurrentCharacter();
            if (character != null)
            {
                character.RefreshWarrantModifiers();
            }
        }
    }

    /// <summary>
    /// Checks if a node is unlocked on the active page.
    /// </summary>
    public bool IsNodeUnlocked(string nodeId)
    {
        if (ActivePage == null)
        {
            Debug.LogWarning($"[WarrantBoardStateController] IsNodeUnlocked('{nodeId}'): ActivePage is null");
            return false;
        }
        
        bool unlocked = ActivePage.IsNodeUnlocked(nodeId);
        
        // Debug logging for first few nodes to track state
        if (nodeId.Contains("effect") && (nodeId.Contains("effect1") || nodeId.Contains("effect2") || nodeId.Contains("effect3")))
        {
            Debug.Log($"[WarrantBoardStateController] IsNodeUnlocked('{nodeId}'): {unlocked} (ActivePage: {ActivePageIndex + 1}/{pages.Count}, UnlockedCount: {ActivePage.UnlockedNodeCount})");
        }
        
        return unlocked;
    }

    private void CacheValidSockets()
    {
        validSocketIds.Clear();
        if (graphDefinition == null)
            return;

        foreach (var node in graphDefinition.Nodes)
        {
            if (node == null)
                continue;

            if (node.nodeType == WarrantNodeType.Socket || node.nodeType == WarrantNodeType.SpecialSocket)
            {
                validSocketIds.Add(node.id);
            }
        }
    }

    private void ClampActiveIndex()
    {
        if (pages.Count == 0)
        {
            activePageIndex = 0;
            return;
        }

        activePageIndex = Mathf.Clamp(activePageIndex, 0, pages.Count - 1);
    }

    private bool IsSocketNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
            return false;

        if (validSocketIds.Count == 0)
        {
            CacheValidSockets();
        }

        return validSocketIds.Contains(nodeId);
    }

    private WarrantBoardPageData CreatePageData(int index)
    {
        var id = Guid.NewGuid().ToString("N");
        var displayName = $"{pageNamePrefix} {index + 1}";
        return new WarrantBoardPageData(id, displayName);
    }

    [Serializable]
    public class WarrantBoardSaveData
    {
        public int activePageIndex;
        public List<WarrantBoardPageData> pages = new List<WarrantBoardPageData>();
    }

    [Serializable]
    public class WarrantBoardPageData
    {
        [SerializeField] private string pageId;
        [SerializeField] private string displayName;
        [SerializeField] private List<WarrantSocketAssignment> socketAssignments = new List<WarrantSocketAssignment>();
        [SerializeField] private List<string> unlockedNodeIds = new List<string>();

        private HashSet<string> unlockedSetCache;

        public string PageId => pageId;
        public string DisplayName => displayName;
        public IReadOnlyList<WarrantSocketAssignment> SocketAssignments => socketAssignments;
        public int UnlockedNodeCount => unlockedNodeIds?.Count ?? 0;
        public IReadOnlyList<string> UnlockedNodeIds => unlockedNodeIds ?? new List<string>();
        
        public bool IsNodeUnlocked(string nodeId)
        {
            EnsureUnlockedSetCache();
            return unlockedSetCache.Contains(nodeId);
        }

        public IReadOnlyCollection<string> GetUnlockedNodeIds()
        {
            EnsureUnlockedSetCache();
            return unlockedSetCache.ToList();
        }

        private void EnsureUnlockedSetCache()
        {
            if (unlockedSetCache == null)
            {
                unlockedSetCache = new HashSet<string>(unlockedNodeIds ?? new List<string>());
            }
        }
        
        /// <summary>
        /// Forces a rebuild of the unlocked node cache.
        /// Call this after deserialization to ensure cache is up to date.
        /// </summary>
        public void RebuildUnlockedCache()
        {
            unlockedSetCache = null;
            EnsureUnlockedSetCache();
        }

        public bool UnlockNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return false;

            EnsureUnlockedSetCache();
            if (unlockedSetCache.Contains(nodeId))
                return false; // Already unlocked

            unlockedNodeIds.Add(nodeId);
            unlockedSetCache.Add(nodeId);
            return true;
        }

        public WarrantBoardPageData()
            : this(Guid.NewGuid().ToString("N"), "Loadout")
        {
        }

        public WarrantBoardPageData(string id, string display)
        {
            pageId = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString("N") : id;
            displayName = string.IsNullOrEmpty(display) ? "Loadout" : display;
            
            // Anchor node is always unlocked by default
            UnlockNode("Anchor");
        }

        public void SetDisplayName(string value)
        {
            displayName = string.IsNullOrEmpty(value) ? displayName : value;
        }

        public void SetAssignment(string nodeId, string warrantId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return;

            var assignment = socketAssignments.FirstOrDefault(a => a.nodeId == nodeId);
            if (string.IsNullOrEmpty(warrantId))
            {
                if (assignment != null)
                {
                    socketAssignments.Remove(assignment);
                }
                return;
            }

            if (assignment == null)
            {
                assignment = new WarrantSocketAssignment { nodeId = nodeId };
                socketAssignments.Add(assignment);
            }
            assignment.warrantId = warrantId;
        }

        public bool RemoveAssignment(string nodeId)
        {
            var assignment = socketAssignments.FirstOrDefault(a => a.nodeId == nodeId);
            if (assignment == null)
                return false;

            socketAssignments.Remove(assignment);
            return true;
        }

        public string GetWarrant(string nodeId)
        {
            var assignment = socketAssignments.FirstOrDefault(a => a.nodeId == nodeId);
            return assignment?.warrantId;
        }

        public void ClearAssignments()
        {
            socketAssignments.Clear();
        }

        public WarrantBoardPageData Clone(string newId = null, string newDisplayName = null)
        {
            var clone = new WarrantBoardPageData(newId ?? pageId, newDisplayName ?? displayName);
            foreach (var assignment in socketAssignments)
            {
                clone.socketAssignments.Add(new WarrantSocketAssignment
                {
                    nodeId = assignment.nodeId,
                    warrantId = assignment.warrantId
                });
            }
            
            // Clone unlocked nodes
            EnsureUnlockedSetCache();
            foreach (var nodeId in unlockedNodeIds)
            {
                clone.UnlockNode(nodeId);
            }
            
            return clone;
        }
    }

    [Serializable]
    public class WarrantSocketAssignment
    {
        public string nodeId;
        public string warrantId;
    }
}

