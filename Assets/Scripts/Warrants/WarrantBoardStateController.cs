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
    }

    private void OnValidate()
    {
        CacheValidSockets();
        ClampActiveIndex();
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
        
        // Refresh character warrant modifiers
        RefreshCharacterWarrantModifiers();
        
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
            // Refresh character warrant modifiers
            RefreshCharacterWarrantModifiers();
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
    }

    [ContextMenu("Save To PlayerPrefs")]
    public void SaveToPlayerPrefs()
    {
        if (string.IsNullOrEmpty(playerPrefsKey))
            return;

        PlayerPrefs.SetString(playerPrefsKey, ToJson(false));
        PlayerPrefs.Save();
    }

    [ContextMenu("Load From PlayerPrefs")]
    public void LoadFromPlayerPrefs()
    {
        if (string.IsNullOrEmpty(playerPrefsKey) || !PlayerPrefs.HasKey(playerPrefsKey))
            return;

        LoadFromJson(PlayerPrefs.GetString(playerPrefsKey));
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
            
            // Refresh character warrant modifiers (unlocking nodes may affect effect nodes)
            RefreshCharacterWarrantModifiers();
            
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
        return ActivePage != null && ActivePage.IsNodeUnlocked(nodeId);
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
                unlockedSetCache = new HashSet<string>(unlockedNodeIds);
            }
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

