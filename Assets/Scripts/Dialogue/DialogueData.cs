using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dexiled/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Info")]
    public string dialogueId;
    public string dialogueName;
    public string npcId; // Links to NPC (e.g., "seer", "questgiver")
    
    [Header("Speaker Portrait")]
    [Tooltip("Default speaker portrait used for all dialogue nodes. Individual nodes can override this with their own portrait.")]
    public Sprite defaultSpeakerPortrait;
    
    [Header("Dialogue Tree")]
    public string startNodeId; // Entry point
    public List<DialogueNode> nodes = new List<DialogueNode>();
    
    private Dictionary<string, DialogueNode> nodeLookup;
    
    public DialogueNode GetNode(string nodeId)
    {
        if (nodeLookup == null)
        {
            BuildLookup();
        }
        
        nodeLookup.TryGetValue(nodeId, out var node);
        return node;
    }
    
    public DialogueNode GetStartNode()
    {
        if (string.IsNullOrEmpty(startNodeId))
            return nodes.Count > 0 ? nodes[0] : null;
        
        return GetNode(startNodeId);
    }
    
    private void BuildLookup()
    {
        nodeLookup = new Dictionary<string, DialogueNode>();
        foreach (var node in nodes)
        {
            if (node != null && !string.IsNullOrEmpty(node.nodeId))
            {
                nodeLookup[node.nodeId] = node;
            }
        }
    }
    
    private void OnValidate()
    {
        nodeLookup = null; // Rebuild on next access
    }
}


