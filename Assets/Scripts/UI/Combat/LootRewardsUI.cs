using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Handles the Area Cleared panel and loot summary shown after combat victory.
/// </summary>
public class LootRewardsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject areaClearedPanel;
    [SerializeField, Tooltip("Delay (seconds) before the panel appears after victory.")] private float showDelaySeconds = 1.1f;

    [Header("Summary UI")]
    [SerializeField] private TextMeshProUGUI victoryLabel;
    [SerializeField] private TextMeshProUGUI experienceLabel;
    [SerializeField] private TextMeshProUGUI emptyLootLabel;
    [SerializeField] private Transform lootSummaryContainer;
    [SerializeField] private GameObject lootSummaryItemPrefab;

    [Header("Navigation")]
    [SerializeField] private Button continueButton;
    [SerializeField] private string worldMapSceneName = "MainGameUI";

    private LootDropResult currentLoot;
    private bool rewardsApplied;
    private readonly List<GameObject> spawnedLootEntries = new List<GameObject>();
    private CombatDisplayManager combatDisplayManager;

    private void Awake()
    {
        combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();

        if (areaClearedPanel == null)
        {
            // First, check if LootRewardsUI is on the AreaClearedPanel GameObject itself
            if (gameObject.name == "AreaClearedPanel" || gameObject.name.Contains("AreaCleared"))
            {
                areaClearedPanel = gameObject;
                Debug.Log($"[LootRewardsUI] LootRewardsUI is on AreaClearedPanel GameObject itself: {gameObject.name}");
            }
            else
            {
                // Try to find as child of this transform
                var panelTransform = transform.Find("AreaClearedPanel");
                if (panelTransform != null)
                {
                    areaClearedPanel = panelTransform.gameObject;
                    Debug.Log("[LootRewardsUI] Found AreaClearedPanel as child of LootRewardsUI");
                }
                else
                {
                    // Try to find in scene root (standalone GameObject)
                    GameObject foundPanel = GameObject.Find("AreaClearedPanel");
                    if (foundPanel != null)
                    {
                        areaClearedPanel = foundPanel;
                        Debug.Log("[LootRewardsUI] Found AreaClearedPanel as standalone GameObject in scene");
                    }
                    else
                    {
                        // Try to get from CombatSceneManager.victoryUI (if it's set to AreaClearedPanel)
                        CombatSceneManager sceneManager = FindFirstObjectByType<CombatSceneManager>();
                        if (sceneManager != null && sceneManager.victoryUI != null)
                        {
                            areaClearedPanel = sceneManager.victoryUI;
                            Debug.Log("[LootRewardsUI] Found AreaClearedPanel via CombatSceneManager.victoryUI");
                        }
                        else
                        {
                            Debug.LogError("[LootRewardsUI] AreaClearedPanel not found! Searched as child, standalone GameObject, and CombatSceneManager.victoryUI. Please assign it in the Inspector or ensure it exists in the scene.");
                        }
                    }
                }
            }
        }

        if (lootSummaryContainer == null && areaClearedPanel != null)
        {
            var summary = areaClearedPanel.transform.Find("LootSummary");
            if (summary != null)
            {
                lootSummaryContainer = summary;
            }
        }

        if (continueButton == null && areaClearedPanel != null)
        {
            var continueTransform = areaClearedPanel.transform.Find("ContinueButton");
            if (continueTransform != null)
            {
                continueButton = continueTransform.GetComponent<Button>();
            }
        }
    }

    private void Start()
    {
        // Ensure subscription happens even if OnEnable was called before CombatDisplayManager existed
        if (combatDisplayManager == null)
        {
            combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        }
        
        if (combatDisplayManager != null)
        {
            // Unsubscribe first to avoid duplicate subscriptions
            combatDisplayManager.OnCombatStateChanged -= HandleCombatStateChanged;
            combatDisplayManager.OnCombatStateChanged += HandleCombatStateChanged;
            Debug.Log("[LootRewardsUI] Subscribed to OnCombatStateChanged in Start()");
            
            // Check if we're already in victory state (in case event was fired before we subscribed)
            CheckCurrentCombatState();
        }
        else
        {
            Debug.LogWarning("[LootRewardsUI] CombatDisplayManager still not found in Start()");
        }
    }

    private bool isInitialized = false;
    
    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        // Ensure we have a reference to CombatDisplayManager
        if (combatDisplayManager == null)
        {
            combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        }

        if (combatDisplayManager != null)
        {
            combatDisplayManager.OnCombatStateChanged += HandleCombatStateChanged;
            Debug.Log("[LootRewardsUI] Subscribed to OnCombatStateChanged event");
            
            // Check if we're already in victory state (we're being activated to show the panel)
            if (combatDisplayManager.currentState == CombatDisplayManager.CombatState.Victory)
            {
                Debug.Log("[LootRewardsUI] OnEnable called during victory - checking if panel should be shown");
                // Don't hide the panel - we're being activated to show it
                // The ShowPanel() call will happen from PresentAreaClearedPanel()
                return;
            }
        }
        else
        {
            Debug.LogWarning("[LootRewardsUI] CombatDisplayManager not found in OnEnable - will retry when combat ends");
        }

            // Only hide panel on initial enable (when combat hasn't ended yet)
            // Use a flag to track if this is the first time OnEnable is called
            // BUT: If we're in victory state, don't hide - we're being activated to show the panel
            if (!isInitialized && combatDisplayManager.currentState != CombatDisplayManager.CombatState.Victory)
            {
                HidePanel();
                isInitialized = true;
            }
            else if (combatDisplayManager.currentState == CombatDisplayManager.CombatState.Victory)
            {
                Debug.Log("[LootRewardsUI] OnEnable called during victory - keeping panel active");
                // Don't hide - we're being activated to show the panel
                isInitialized = true; // Mark as initialized so we don't hide on next enable
            }
            else
            {
                Debug.Log("[LootRewardsUI] OnEnable called after initialization - not hiding panel (may be showing it)");
            }
    }

    private void OnDisable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        if (combatDisplayManager != null)
        {
            combatDisplayManager.OnCombatStateChanged -= HandleCombatStateChanged;
        }
    }

    private void HandleCombatStateChanged(CombatDisplayManager.CombatState state)
    {
        Debug.Log($"[LootRewardsUI] Combat state changed to: {state}");
        if (state == CombatDisplayManager.CombatState.Victory)
        {
            Debug.Log($"[LootRewardsUI] Victory detected! Will show AreaClearedPanel in {showDelaySeconds} seconds");
            Invoke(nameof(PresentAreaClearedPanel), showDelaySeconds);
        }
    }
    
    /// <summary>
    /// Check current combat state and show panel if already in victory state
    /// This is useful if we subscribe to the event after it was already fired
    /// </summary>
    private void CheckCurrentCombatState()
    {
        if (combatDisplayManager == null)
        {
            combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
        }
        
        if (combatDisplayManager != null && combatDisplayManager.currentState == CombatDisplayManager.CombatState.Victory)
        {
            Debug.Log("[LootRewardsUI] Already in Victory state - showing AreaClearedPanel immediately");
            PresentAreaClearedPanel();
        }
    }

    /// <summary>
    /// Public method to show the AreaClearedPanel - can be called directly if event subscription fails
    /// </summary>
    public void ShowAreaClearedPanel()
    {
        PresentAreaClearedPanel();
    }

    private void PresentAreaClearedPanel()
    {
        Debug.Log("[LootRewardsUI] PresentAreaClearedPanel() called");
        
        if (combatDisplayManager == null)
        {
            combatDisplayManager = FindFirstObjectByType<CombatDisplayManager>();
            if (combatDisplayManager == null)
            {
                Debug.LogError("[LootRewardsUI] CombatDisplayManager not found; cannot present loot.");
                return;
            }
        }

        currentLoot = combatDisplayManager.GetPendingLoot();
        rewardsApplied = false;

        RefreshSummaryUI(currentLoot);
        ShowPanel();
        
        Debug.Log("[LootRewardsUI] AreaClearedPanel should now be visible");
    }

    private void RefreshSummaryUI(LootDropResult loot)
    {
        ClearSpawnedLootEntries();

        if (victoryLabel != null)
        {
            victoryLabel.text = "AREA CLEARED";
        }

        if (experienceLabel != null)
        {
            int totalExp = loot != null ? loot.totalExperience : 0;
            experienceLabel.text = totalExp > 0 ? $"+{totalExp} Experience" : string.Empty;
        }

        List<LootReward> itemRewards = loot?.rewards
            .Where(r => r.rewardType == RewardType.Item || r.rewardType == RewardType.Effigy)
            .ToList() ?? new List<LootReward>();

        bool hasItems = itemRewards.Count > 0;

        if (lootSummaryContainer != null && hasItems)
        {
            foreach (var reward in itemRewards)
            {
                spawnedLootEntries.Add(CreateLootSummaryEntry(reward));
            }
        }

        if (emptyLootLabel != null)
        {
            emptyLootLabel.gameObject.SetActive(!hasItems);
            if (!hasItems)
            {
                emptyLootLabel.text = "No items found this encounter.";
            }
        }
    }

    private GameObject CreateLootSummaryEntry(LootReward reward)
    {
        GameObject entry;
        if (lootSummaryItemPrefab != null)
        {
            entry = Instantiate(lootSummaryItemPrefab, lootSummaryContainer);
        }
        else
        {
            entry = new GameObject("LootSummaryItem");
            entry.transform.SetParent(lootSummaryContainer, false);
            entry.AddComponent<HorizontalLayoutGroup>();
        }

        Image icon = entry.GetComponentInChildren<Image>(includeInactive: true);
        if (icon != null)
        {
            Sprite sprite = reward.rewardType == RewardType.Effigy
                ? reward.effigyInstance != null ? reward.effigyInstance.icon : null
                : reward.GetIcon();

            if (sprite != null)
            {
                icon.sprite = sprite;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }
        }

        TextMeshProUGUI label = entry.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        if (label != null)
        {
            label.gameObject.SetActive(true);
            label.text = reward.GetDisplayName();
        }

        return entry;
    }

    private void ClearSpawnedLootEntries()
    {
        foreach (var entry in spawnedLootEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        spawnedLootEntries.Clear();
    }

    private void OnContinueClicked()
    {
        if (!rewardsApplied && combatDisplayManager != null)
        {
            combatDisplayManager.ApplyPendingLoot();
            rewardsApplied = true;
        }

        HidePanel();
        LoadWorldMap();
    }

    private void ShowPanel()
    {
        // Try to find panel if not already set
        if (areaClearedPanel == null)
        {
            // If LootRewardsUI is on the AreaClearedPanel GameObject itself, use this GameObject
            if (gameObject.name == "AreaClearedPanel" || gameObject.name.Contains("AreaCleared"))
            {
                areaClearedPanel = gameObject;
                Debug.Log("[LootRewardsUI] LootRewardsUI is on AreaClearedPanel GameObject - using this GameObject");
            }
            else
            {
                // Try to find as child of this transform
                var panelTransform = transform.Find("AreaClearedPanel");
                if (panelTransform != null)
                {
                    areaClearedPanel = panelTransform.gameObject;
                    Debug.Log("[LootRewardsUI] Found AreaClearedPanel as child of LootRewardsUI");
                }
                else
                {
                    // Try to find in scene root (standalone GameObject)
                    GameObject foundPanel = GameObject.Find("AreaClearedPanel");
                    if (foundPanel != null)
                    {
                        areaClearedPanel = foundPanel;
                        Debug.Log("[LootRewardsUI] Found AreaClearedPanel as standalone GameObject in scene");
                    }
                    else
                    {
                        // Try to get from CombatSceneManager.victoryUI (if it's set to AreaClearedPanel)
                        CombatSceneManager sceneManager = FindFirstObjectByType<CombatSceneManager>();
                        if (sceneManager != null && sceneManager.victoryUI != null)
                        {
                            areaClearedPanel = sceneManager.victoryUI;
                            Debug.Log("[LootRewardsUI] Found AreaClearedPanel via CombatSceneManager.victoryUI");
                        }
                    }
                }
            }
        }
        
        if (areaClearedPanel != null)
        {
            // Ensure parent is active first (if panel has a parent)
            if (areaClearedPanel.transform.parent != null && !areaClearedPanel.transform.parent.gameObject.activeInHierarchy)
            {
                areaClearedPanel.transform.parent.gameObject.SetActive(true);
                Debug.Log($"[LootRewardsUI] Activated parent GameObject: {areaClearedPanel.transform.parent.name}");
            }
            
            // If the panel is this GameObject, we're already active (OnEnable was called)
            // Just ensure it stays active and log the state
            if (areaClearedPanel == gameObject)
            {
                // We're already active (OnEnable was called), just ensure we stay active
                if (!areaClearedPanel.activeSelf)
                {
                    areaClearedPanel.SetActive(true);
                }
                Debug.Log($"[LootRewardsUI] AreaClearedPanel (this GameObject) is active: {areaClearedPanel.activeInHierarchy} (ActiveSelf: {areaClearedPanel.activeSelf})");
            }
            else
            {
                areaClearedPanel.SetActive(true);
                Debug.Log($"[LootRewardsUI] AreaClearedPanel activated: {areaClearedPanel.name} (Active: {areaClearedPanel.activeInHierarchy}, ActiveSelf: {areaClearedPanel.activeSelf})");
            }
            
            // Double-check: If still inactive, there might be a parent issue
            if (!areaClearedPanel.activeInHierarchy)
            {
                Debug.LogWarning($"[LootRewardsUI] AreaClearedPanel is still inactive after SetActive(true)! Parent: {(areaClearedPanel.transform.parent != null ? areaClearedPanel.transform.parent.name : "None")}, Parent Active: {(areaClearedPanel.transform.parent != null ? areaClearedPanel.transform.parent.gameObject.activeInHierarchy.ToString() : "N/A")}");
                
                // Try activating all parents up the hierarchy
                Transform current = areaClearedPanel.transform;
                while (current != null && current.parent != null)
                {
                    current = current.parent;
                    if (!current.gameObject.activeInHierarchy)
                    {
                        current.gameObject.SetActive(true);
                        Debug.Log($"[LootRewardsUI] Activated parent in hierarchy: {current.name}");
                    }
                }
                
                // Try activating again
                areaClearedPanel.SetActive(true);
                Debug.Log($"[LootRewardsUI] Retried activation. Final state: Active={areaClearedPanel.activeInHierarchy}, ActiveSelf={areaClearedPanel.activeSelf}");
            }
        }
        else
        {
            Debug.LogError("[LootRewardsUI] areaClearedPanel is null! Cannot show panel. Check if AreaClearedPanel GameObject exists in scene.");
        }
    }

    private void HidePanel()
    {
        if (areaClearedPanel != null)
        {
            areaClearedPanel.SetActive(false);
        }
    }

    private void LoadWorldMap()
    {
        if (string.IsNullOrWhiteSpace(worldMapSceneName))
        {
            Debug.LogWarning("[LootRewardsUI] worldMapSceneName not set; staying in current scene.");
            return;
        }

        SceneManager.LoadScene(worldMapSceneName);
    }
}
