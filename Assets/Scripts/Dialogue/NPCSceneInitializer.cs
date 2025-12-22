using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Initializes NPC scenes by automatically starting dialogue when the scene loads.
/// Attach this to a GameObject in NPC-specific scenes (e.g., JoregShack).
/// </summary>
public class NPCSceneInitializer : MonoBehaviour
{
    [Header("NPC Configuration")]
    [Tooltip("The NPC ID to find and start dialogue with (e.g., 'peacekeeper_joreg')")]
    [SerializeField] private string npcId;
    
    [Tooltip("Alternative: Direct reference to the NPCInteractable component")]
    [SerializeField] private NPCInteractable npcReference;
    
    [Header("Initialization Settings")]
    [Tooltip("Delay before starting dialogue (allows scene to fully load)")]
    [SerializeField] private float startDelay = 0.5f;
    
    [Tooltip("Auto-start dialogue when scene loads")]
    [SerializeField] private bool autoStartDialogue = true;
    
    private void Start()
    {
        if (autoStartDialogue)
        {
            StartCoroutine(InitializeAfterDelay());
        }
    }
    
    private IEnumerator InitializeAfterDelay()
    {
        // Wait for scene to fully load
        yield return new WaitForSeconds(startDelay);
        
        // Find the NPC
        NPCInteractable npc = null;
        
        if (npcReference != null)
        {
            npc = npcReference;
            Debug.Log($"[NPCSceneInitializer] Using assigned NPC reference: {npc.GetNPCId()}");
        }
        else if (!string.IsNullOrEmpty(npcId))
        {
            npc = FindNPCById(npcId);
            if (npc != null)
            {
                Debug.Log($"[NPCSceneInitializer] Found NPC by ID: {npcId}");
            }
            else
            {
                Debug.LogWarning($"[NPCSceneInitializer] NPC with ID '{npcId}' not found in scene '{SceneManager.GetActiveScene().name}'. " +
                               $"Make sure the NPC GameObject has an NPCInteractable component with matching npcId.");
            }
        }
        else
        {
            Debug.LogWarning("[NPCSceneInitializer] No NPC ID or reference assigned. Cannot start dialogue.");
            yield break;
        }
        
        // Start dialogue if NPC was found
        if (npc != null)
        {
            // Ensure NPC and all parents are active
            ActivateNPCAndParents(npc);
            
            // Wait a frame for activation to process
            yield return null;
            
            // Start the dialogue
            npc.StartDialogue();
            Debug.Log($"[NPCSceneInitializer] Started dialogue with NPC '{npc.GetNPCId()}' in scene '{SceneManager.GetActiveScene().name}'.");
        }
    }
    
    /// <summary>
    /// Find NPC by ID (searches both active and inactive NPCs)
    /// </summary>
    private NPCInteractable FindNPCById(string id)
    {
        NPCInteractable[] allNPCs = FindObjectsByType<NPCInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (NPCInteractable npc in allNPCs)
        {
            if (npc != null && npc.GetNPCId() == id)
            {
                return npc;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Activate NPC and all its parent GameObjects
    /// </summary>
    private void ActivateNPCAndParents(NPCInteractable npc)
    {
        if (npc == null) return;
        
        Transform current = npc.transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
                Debug.Log($"[NPCSceneInitializer] Activated parent: {current.name}");
            }
            current = current.parent;
        }
    }
    
    /// <summary>
    /// Manually trigger dialogue start (can be called from buttons, etc.)
    /// </summary>
    public void StartDialogue()
    {
        if (npcReference != null)
        {
            npcReference.StartDialogue();
        }
        else if (!string.IsNullOrEmpty(npcId))
        {
            NPCInteractable npc = FindNPCById(npcId);
            if (npc != null)
            {
                ActivateNPCAndParents(npc);
                npc.StartDialogue();
            }
        }
    }
}

