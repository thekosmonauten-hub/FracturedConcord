using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Auto-generates encounter nodes on the WorldMap from EncounterManager data.
/// Can be used at runtime or in editor to populate the world map with encounter buttons.
/// </summary>
public class WorldMapNodeGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [Tooltip("Prefab to instantiate for each encounter node. Should have EncounterButton component.")]
    [SerializeField] private GameObject encounterNodePrefab;
    
    [Tooltip("Parent transform to place generated nodes under. If null, uses this transform.")]
    [SerializeField] private Transform nodeParent;
    
    [Tooltip("If true, clears existing nodes before generating new ones.")]
    [SerializeField] private bool clearExistingNodes = true;
    
    [Tooltip("If true, generates nodes on Start(). Otherwise, call GenerateNodes() manually.")]
    [SerializeField] private bool generateOnStart = true;
    
    [Tooltip("If true, only generates unlocked encounters. Otherwise generates all.")]
    [SerializeField] private bool onlyUnlockedEncounters = false;
    
    [Header("Layout Settings")]
    [Tooltip("Grid layout: number of columns")]
    [SerializeField] private int gridColumns = 5;
    
    [Tooltip("Spacing between nodes")]
    [SerializeField] private Vector2 nodeSpacing = new Vector2(200f, 200f);
    
    [Tooltip("Starting position offset")]
    [SerializeField] private Vector2 startOffset = Vector2.zero;
    
    [Header("Debug")]
    [SerializeField] private bool verboseDebug = true;

    private List<GameObject> generatedNodes = new List<GameObject>();

    private void Start()
    {
        if (generateOnStart)
        {
            // Wait a frame to ensure EncounterManager is initialized
            StartCoroutine(GenerateNodesDelayed());
        }
    }

    private System.Collections.IEnumerator GenerateNodesDelayed()
    {
        yield return null; // Wait one frame
        
        // Ensure EncounterManager is initialized
        if (EncounterManager.Instance == null)
        {
            Debug.LogWarning("[WorldMapNodeGenerator] EncounterManager.Instance is null. Waiting...");
            yield return new WaitForSeconds(0.1f);
        }
        
        // Ensure encounter graph is built
        if (EncounterManager.Instance != null)
        {
            var nodes = EncounterManager.Instance.GetEncounterNodes();
            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogWarning("[WorldMapNodeGenerator] No encounter nodes found. EncounterManager may need initialization.");
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        GenerateNodes();
    }

    /// <summary>
    /// Generates encounter nodes from EncounterManager data.
    /// </summary>
    [ContextMenu("Generate Encounter Nodes")]
    public void GenerateNodes()
    {
        if (clearExistingNodes)
        {
            ClearExistingNodes();
        }

        EncounterManager encounterManager = EncounterManager.Instance;
        if (encounterManager == null)
        {
            Debug.LogError("[WorldMapNodeGenerator] EncounterManager.Instance is null! Cannot generate nodes.");
            return;
        }

        var encounterNodes = encounterManager.GetEncounterNodes();
        if (encounterNodes == null || encounterNodes.Count == 0)
        {
            Debug.LogWarning("[WorldMapNodeGenerator] No encounters found in EncounterManager. Make sure encounters are loaded from Resources.");
            return;
        }

        if (encounterNodePrefab == null)
        {
            Debug.LogError("[WorldMapNodeGenerator] Encounter node prefab is not assigned!");
            return;
        }

        Transform parent = nodeParent != null ? nodeParent : transform;
        int nodeIndex = 0;

        foreach (var kvp in encounterNodes)
        {
            EncounterData encounter = kvp.Value;
            if (encounter == null) continue;

            // Skip if only generating unlocked encounters and this one is locked
            if (onlyUnlockedEncounters && !encounter.isUnlocked)
            {
                if (verboseDebug)
                {
                    Debug.Log($"[WorldMapNodeGenerator] Skipping locked encounter: {encounter.encounterName} (ID: {encounter.encounterID})");
                }
                continue;
            }

            // Create node
            GameObject nodeGO = Instantiate(encounterNodePrefab, parent);
            nodeGO.name = $"EncounterNode_{encounter.encounterID}_{encounter.encounterName}";

            // Set up EncounterButton component
            EncounterButton encounterButton = nodeGO.GetComponent<EncounterButton>();
            if (encounterButton == null)
            {
                encounterButton = nodeGO.AddComponent<EncounterButton>();
            }

            // Configure the button
            encounterButton.encounterID = encounter.encounterID;
            encounterButton.encounterName = encounter.encounterName;
            encounterButton.areaLevel = encounter.areaLevel;
            encounterButton.sceneName = encounter.sceneName;
            encounterButton.autoSyncEncounterData = true; // Enable auto-sync to keep data current

            // Position the node in a grid layout
            RectTransform rectTransform = nodeGO.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                int row = nodeIndex / gridColumns;
                int col = nodeIndex % gridColumns;
                Vector2 position = startOffset + new Vector2(
                    col * nodeSpacing.x,
                    -row * nodeSpacing.y
                );
                rectTransform.anchoredPosition = position;
            }

            generatedNodes.Add(nodeGO);
            nodeIndex++;

            if (verboseDebug)
            {
                Debug.Log($"[WorldMapNodeGenerator] Generated node for: {encounter.encounterName} (ID: {encounter.encounterID}, Unlocked: {encounter.isUnlocked})");
            }
        }

        Debug.Log($"[WorldMapNodeGenerator] Generated {generatedNodes.Count} encounter nodes.");
    }

    /// <summary>
    /// Clears all generated nodes.
    /// </summary>
    [ContextMenu("Clear Generated Nodes")]
    public void ClearExistingNodes()
    {
        foreach (GameObject node in generatedNodes)
        {
            if (node != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(node);
                }
                else
#endif
                {
                    Destroy(node);
                }
            }
        }
        generatedNodes.Clear();
        Debug.Log("[WorldMapNodeGenerator] Cleared all generated nodes.");
    }

    /// <summary>
    /// Refreshes all generated nodes (useful after encounter progression changes).
    /// </summary>
    [ContextMenu("Refresh Nodes")]
    public void RefreshNodes()
    {
        foreach (GameObject node in generatedNodes)
        {
            if (node != null)
            {
                EncounterButton button = node.GetComponent<EncounterButton>();
                if (button != null)
                {
                    button.RefreshVisualsFromOutside();
                }
            }
        }
        Debug.Log("[WorldMapNodeGenerator] Refreshed all node visuals.");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: Generates nodes from Resources without requiring runtime initialization.
    /// </summary>
    [ContextMenu("Generate Nodes (Editor Mode)")]
    public void GenerateNodesEditor()
    {
        // Load encounters directly from Resources
        var act1Assets = Resources.LoadAll<EncounterDataAsset>("Encounters/Act1");
        var act2Assets = Resources.LoadAll<EncounterDataAsset>("Encounters/Act2");
        var act3Assets = Resources.LoadAll<EncounterDataAsset>("Encounters/Act3");
        var act4Assets = Resources.LoadAll<EncounterDataAsset>("Encounters/Act4");

        if (clearExistingNodes)
        {
            ClearExistingNodes();
        }

        if (encounterNodePrefab == null)
        {
            Debug.LogError("[WorldMapNodeGenerator] Encounter node prefab is not assigned!");
            return;
        }

        Transform parent = nodeParent != null ? nodeParent : transform;
        int nodeIndex = 0;
        List<EncounterDataAsset> allAssets = new List<EncounterDataAsset>();
        
        if (act1Assets != null) allAssets.AddRange(act1Assets);
        if (act2Assets != null) allAssets.AddRange(act2Assets);
        if (act3Assets != null) allAssets.AddRange(act3Assets);
        if (act4Assets != null) allAssets.AddRange(act4Assets);

        // Sort by encounter ID
        allAssets.Sort((a, b) => a.encounterID.CompareTo(b.encounterID));

        foreach (var asset in allAssets)
        {
            if (asset == null) continue;

            // Create node
            GameObject nodeGO = UnityEditor.PrefabUtility.InstantiatePrefab(encounterNodePrefab, parent) as GameObject;
            if (nodeGO == null)
            {
                nodeGO = Instantiate(encounterNodePrefab, parent);
            }
            
            nodeGO.name = $"EncounterNode_{asset.encounterID}_{asset.encounterName}";

            // Set up EncounterButton component
            EncounterButton encounterButton = nodeGO.GetComponent<EncounterButton>();
            if (encounterButton == null)
            {
                encounterButton = nodeGO.AddComponent<EncounterButton>();
            }

            // Configure the button
            encounterButton.encounterID = asset.encounterID;
            encounterButton.encounterName = asset.encounterName;
            encounterButton.areaLevel = asset.areaLevel;
            encounterButton.sceneName = asset.sceneName;
            encounterButton.encounterAsset = asset; // Assign the asset reference
            encounterButton.autoSyncEncounterData = true;

            // Position the node
            RectTransform rectTransform = nodeGO.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                int row = nodeIndex / gridColumns;
                int col = nodeIndex % gridColumns;
                Vector2 position = startOffset + new Vector2(
                    col * nodeSpacing.x,
                    -row * nodeSpacing.y
                );
                rectTransform.anchoredPosition = position;
            }

            generatedNodes.Add(nodeGO);
            nodeIndex++;

            UnityEditor.Undo.RegisterCreatedObjectUndo(nodeGO, "Generate Encounter Node");
        }

        Debug.Log($"[WorldMapNodeGenerator] Generated {generatedNodes.Count} encounter nodes in editor mode.");
    }
#endif
}

