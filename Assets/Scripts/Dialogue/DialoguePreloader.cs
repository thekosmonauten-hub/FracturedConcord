using UnityEngine;

/// <summary>
/// Preloads dialogue data when a panel is activated to ensure dialogue is ready before interaction.
/// Attach this to the PeacekeepersFactionPanel or the button that activates it.
/// </summary>
public class DialoguePreloader : MonoBehaviour
{
    [Header("NPC to Preload")]
    [Tooltip("The NPCInteractable component for the NPC whose dialogue should be preloaded")]
    [SerializeField] private NPCInteractable npcToPreload;
    
    [Tooltip("NPC ID to search for if npcToPreload is not assigned (e.g., 'peacekeeper_joreg')")]
    [SerializeField] private string npcIdToFind = "peacekeeper_joreg";
    
    [Header("Auto-find Settings")]
    [Tooltip("If true, will search for NPCInteractable in children when panel is activated")]
    [SerializeField] private bool autoFindNPC = true;
    
    private void OnEnable()
    {
        // Preload dialogue when this GameObject (panel) is activated
        PreloadDialogue();
    }
    
    /// <summary>
    /// Preloads the dialogue for the assigned NPC
    /// </summary>
    public void PreloadDialogue()
    {
        NPCInteractable npc = GetNPCInteractable();
        
        if (npc == null)
        {
            Debug.LogWarning($"[DialoguePreloader] Could not find NPC to preload dialogue for. NPC ID: {npcIdToFind}");
            return;
        }
        
        if (npc.GetDialogueData() == null)
        {
            Debug.LogWarning($"[DialoguePreloader] NPC '{npc.GetNPCName()}' ({npc.GetNPCId()}) has no dialogue data assigned!");
            return;
        }
        
        // Validate dialogue data by checking if it can be loaded
        var dialogueData = npc.GetDialogueData();
        if (dialogueData != null && dialogueData.nodes != null && dialogueData.nodes.Count > 0)
        {
            // Get the start node to validate it exists
            var startNode = dialogueData.GetStartNode();
            if (startNode != null)
            {
                Debug.Log($"[DialoguePreloader] ✓ Preloaded dialogue for '{npc.GetNPCName()}' - Start node: '{startNode.nodeId}'");
            }
            else
            {
                Debug.LogWarning($"[DialoguePreloader] Dialogue for '{npc.GetNPCName()}' has no valid start node!");
            }
        }
        else
        {
            Debug.LogWarning($"[DialoguePreloader] Dialogue data for '{npc.GetNPCName()}' is invalid or empty!");
        }
    }
    
    /// <summary>
    /// Gets the NPCInteractable component to preload
    /// </summary>
    private NPCInteractable GetNPCInteractable()
    {
        // Use assigned reference if available
        if (npcToPreload != null)
        {
            return npcToPreload;
        }
        
        // Auto-find if enabled
        if (autoFindNPC)
        {
            // Search in children first
            NPCInteractable found = GetComponentInChildren<NPCInteractable>();
            if (found != null && (string.IsNullOrEmpty(npcIdToFind) || found.GetNPCId() == npcIdToFind))
            {
                return found;
            }
            
            // Search in scene if not found in children
            NPCInteractable[] allNPCs = FindObjectsByType<NPCInteractable>(FindObjectsSortMode.None);
            foreach (var npc in allNPCs)
            {
                if (npc != null && npc.GetNPCId() == npcIdToFind)
                {
                    return npc;
                }
            }
        }
        
        return null;
    }
}



