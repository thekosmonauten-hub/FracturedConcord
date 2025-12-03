using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stores a choice node's selected sub-node index
/// </summary>
[System.Serializable]
public class ChoiceNodeSelection
{
    public string nodeName = "";
    public int selectedSubNodeIndex = -1; // -1 = no selection
    
    public ChoiceNodeSelection(string name, int index)
    {
        nodeName = name;
        selectedSubNodeIndex = index;
    }
}

/// <summary>
/// Tracks a character's Ascendancy progression:
/// - Which Ascendancy they've chosen
/// - Which passives they've unlocked
/// - Available Ascendancy points
/// </summary>
[System.Serializable]
public class CharacterAscendancyProgress
{
    [Header("Ascendancy Selection")]
    public string selectedAscendancy = ""; // Empty = not chosen yet
    public bool ascendancyUnlocked = false;
    
    [Header("Point Progression")]
    public int totalAscendancyPoints = 0;      // Total points earned
    public int spentAscendancyPoints = 0;      // Points spent on passives
    public int availableAscendancyPoints = 0;  // Points available to spend
    
    [Header("Unlocked Passives")]
    public List<string> unlockedPassives = new List<string>();
    
    [Header("Choice Node Selections")]
    [Tooltip("Stores selected sub-node indices for choice nodes. Use GetSelectedSubNodeIndex/SetSelectedSubNodeIndex methods.")]
    public List<ChoiceNodeSelection> choiceNodeSelections = new List<ChoiceNodeSelection>();
    
    [Header("Signature Card")]
    public bool signatureCardUnlocked = false;
    
    /// <summary>
    /// Choose an Ascendancy (can only be done once)
    /// Automatically allocates the starter node if it has unlockedByDefault
    /// </summary>
    public bool ChooseAscendancy(string ascendancyName, AscendancyData ascendancyData = null)
    {
        if (!string.IsNullOrEmpty(selectedAscendancy))
        {
            Debug.LogWarning($"[AscendancyProgress] Already chose Ascendancy: {selectedAscendancy}");
            return false;
        }
        
        selectedAscendancy = ascendancyName;
        ascendancyUnlocked = true;
        signatureCardUnlocked = true; // Signature card unlocks immediately
        
        Debug.Log($"[AscendancyProgress] Chose Ascendancy: {ascendancyName}");
        
        // Auto-allocate starter node if it exists and has unlockedByDefault
        if (ascendancyData != null)
        {
            AscendancyPassive startNode = ascendancyData.GetStartNode();
            if (startNode != null && startNode.unlockedByDefault)
            {
                // Add starter node to unlocked passives (it's free, no point cost)
                if (!unlockedPassives.Contains(startNode.name))
                {
                    unlockedPassives.Add(startNode.name);
                    Debug.Log($"[AscendancyProgress] Auto-allocated starter node: {startNode.name}");
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Award Ascendancy points (from completing challenges/quests)
    /// </summary>
    public void AwardPoints(int points)
    {
        totalAscendancyPoints += points;
        availableAscendancyPoints += points;
        Debug.Log($"[AscendancyProgress] Awarded {points} Ascendancy Points (Total: {totalAscendancyPoints}, Available: {availableAscendancyPoints})");
    }
    
    /// <summary>
    /// Unlock a passive ability
    /// </summary>
    public bool UnlockPassive(AscendancyPassive passive, AscendancyData ascendancy)
    {
        if (passive == null)
        {
            Debug.LogError("[AscendancyProgress] Cannot unlock null passive!");
            return false;
        }
        
        // Check if already unlocked
        if (unlockedPassives.Contains(passive.name))
        {
            Debug.LogWarning($"[AscendancyProgress] Passive already unlocked: {passive.name}");
            return false;
        }
        
        // Check point cost
        if (availableAscendancyPoints < passive.pointCost)
        {
            Debug.LogWarning($"[AscendancyProgress] Not enough points! Need {passive.pointCost}, have {availableAscendancyPoints}");
            return false;
        }
        
        // Check prerequisites (accounting for unlockedByDefault nodes and choice node subnode selection)
        if (passive.prerequisitePassives != null && passive.prerequisitePassives.Count > 0)
        {
            if (passive.requireAllPrerequisites)
            {
                foreach (string prereq in passive.prerequisitePassives)
                {
                    // Check if prerequisite is unlocked in progression OR is unlockedByDefault
                    bool prereqUnlocked = unlockedPassives.Contains(prereq);
                    
                    // If not in progression, check if it's a start node with unlockedByDefault
                    if (!prereqUnlocked && ascendancy != null)
                    {
                        AscendancyPassive prereqPassive = ascendancy.FindPassiveByName(prereq);
                        if (prereqPassive != null && prereqPassive.unlockedByDefault)
                        {
                            prereqUnlocked = true;
                        }
                    }
                    
                    // If prerequisite is a choice node, it must have a selected subnode
                    if (prereqUnlocked && ascendancy != null)
                    {
                        AscendancyPassive prereqPassive = ascendancy.FindPassiveByName(prereq);
                        if (prereqPassive != null && prereqPassive.isChoiceNode)
                        {
                            int selectedIndex = GetSelectedSubNodeIndex(prereq);
                            if (selectedIndex < 0) // No subnode selected
                            {
                                Debug.LogWarning($"[AscendancyProgress] Prerequisite choice node '{prereq}' is unlocked but has no subnode selected. Cannot unlock '{passive.name}' until a subnode is selected.");
                                return false;
                            }
                        }
                    }
                    
                    if (!prereqUnlocked)
                    {
                        Debug.LogWarning($"[AscendancyProgress] Missing prerequisite: {prereq}");
                        return false;
                    }
                }
            }
            else
            {
                bool anyUnlocked = false;
                foreach (string prereq in passive.prerequisitePassives)
                {
                    // Check if prerequisite is unlocked in progression OR is unlockedByDefault
                    bool prereqUnlocked = unlockedPassives.Contains(prereq);
                    
                    // If not in progression, check if it's a start node with unlockedByDefault
                    if (!prereqUnlocked && ascendancy != null)
                    {
                        AscendancyPassive prereqPassive = ascendancy.FindPassiveByName(prereq);
                        if (prereqPassive != null && prereqPassive.unlockedByDefault)
                        {
                            prereqUnlocked = true;
                        }
                    }
                    
                    // If prerequisite is a choice node, it must have a selected subnode
                    if (prereqUnlocked && ascendancy != null)
                    {
                        AscendancyPassive prereqPassive = ascendancy.FindPassiveByName(prereq);
                        if (prereqPassive != null && prereqPassive.isChoiceNode)
                        {
                            int selectedIndex = GetSelectedSubNodeIndex(prereq);
                            if (selectedIndex < 0) // No subnode selected
                            {
                                // This prerequisite doesn't count as "unlocked" for choice nodes without selection
                                prereqUnlocked = false;
                            }
                        }
                    }
                    
                    if (prereqUnlocked)
                    {
                        anyUnlocked = true;
                        break;
                    }
                }

                if (!anyUnlocked)
                {
                    Debug.LogWarning($"[AscendancyProgress] Requires any prerequisite unlocked: {string.Join(", ", passive.prerequisitePassives)}");
                    return false;
                }
            }
        }
        
        // Check node group (mutually exclusive nodes)
        if (ascendancy != null && !string.IsNullOrEmpty(passive.nodeGroup))
        {
            if (ascendancy.IsAnyNodeInGroupUnlocked(passive.nodeGroup, this))
            {
                // Find which node in the group is already unlocked
                var nodesInGroup = ascendancy.GetNodesInGroup(passive.nodeGroup);
                foreach (var nodeInGroup in nodesInGroup)
                {
                    if (unlockedPassives.Contains(nodeInGroup.name))
                    {
                        Debug.LogWarning($"[AscendancyProgress] Cannot unlock {passive.name} - node {nodeInGroup.name} in group '{passive.nodeGroup}' is already unlocked. Only one node per group can be unlocked.");
                        return false;
                    }
                }
            }
        }
        
        // Unlock passive
        unlockedPassives.Add(passive.name);
        spentAscendancyPoints += passive.pointCost;
        availableAscendancyPoints -= passive.pointCost;
        
        // Initialize choice node selection state (no sub-node selected yet)
        if (passive.isChoiceNode)
        {
            if (GetSelectedSubNodeIndex(passive.name) == -2) // -2 = not found
            {
                choiceNodeSelections.Add(new ChoiceNodeSelection(passive.name, -1)); // -1 = no selection, show sub-nodes
            }
        }
        
        Debug.Log($"[AscendancyProgress] Unlocked passive: {passive.name} (Cost: {passive.pointCost}, Remaining: {availableAscendancyPoints})");
        return true;
    }
    
    /// <summary>
    /// Select a sub-node for a choice node
    /// </summary>
    public bool SelectSubNode(string choiceNodeName, int subNodeIndex, AscendancyData ascendancy)
    {
        if (ascendancy == null || string.IsNullOrEmpty(choiceNodeName))
        {
            Debug.LogError("[AscendancyProgress] Cannot select sub-node - invalid parameters");
            return false;
        }
        
        // Find the choice node
        AscendancyPassive choiceNode = ascendancy.FindPassiveByName(choiceNodeName);
        if (choiceNode == null || !choiceNode.isChoiceNode)
        {
            Debug.LogError($"[AscendancyProgress] Node '{choiceNodeName}' is not a choice node!");
            return false;
        }
        
        // Check if choice node is unlocked
        if (!unlockedPassives.Contains(choiceNodeName))
        {
            Debug.LogWarning($"[AscendancyProgress] Cannot select sub-node - choice node '{choiceNodeName}' is not unlocked!");
            return false;
        }
        
        // Validate sub-node index
        if (choiceNode.subNodes == null || subNodeIndex < 0 || subNodeIndex >= choiceNode.subNodes.Count)
        {
            Debug.LogError($"[AscendancyProgress] Invalid sub-node index {subNodeIndex} for choice node '{choiceNodeName}'");
            return false;
        }
        
        // Store selection
        int existingIndex = choiceNodeSelections.FindIndex(s => s.nodeName == choiceNodeName);
        if (existingIndex >= 0)
        {
            choiceNodeSelections[existingIndex].selectedSubNodeIndex = subNodeIndex;
        }
        else
        {
            choiceNodeSelections.Add(new ChoiceNodeSelection(choiceNodeName, subNodeIndex));
        }
        choiceNode.selectedSubNodeIndex = subNodeIndex;
        
        Debug.Log($"[AscendancyProgress] Selected sub-node {subNodeIndex} ('{choiceNode.subNodes[subNodeIndex].name}') for choice node '{choiceNodeName}'");
        return true;
    }
    
    /// <summary>
    /// Get selected sub-node index for a choice node (-1 if no selection, -2 if not found)
    /// </summary>
    public int GetSelectedSubNodeIndex(string choiceNodeName)
    {
        var selection = choiceNodeSelections.FirstOrDefault(s => s.nodeName == choiceNodeName);
        if (selection != null)
        {
            return selection.selectedSubNodeIndex;
        }
        return -2; // Not found
    }
    
    /// <summary>
    /// Clear sub-node selection (return to sub-node display state)
    /// </summary>
    public void ClearSubNodeSelection(string choiceNodeName)
    {
        var selection = choiceNodeSelections.FirstOrDefault(s => s.nodeName == choiceNodeName);
        if (selection != null)
        {
            selection.selectedSubNodeIndex = -1;
            Debug.Log($"[AscendancyProgress] Cleared sub-node selection for '{choiceNodeName}'");
        }
    }
    
    /// <summary>
    /// Check if a passive is unlocked
    /// </summary>
    public bool IsPassiveUnlocked(string passiveName)
    {
        return unlockedPassives.Contains(passiveName);
    }
    
    /// <summary>
    /// Get number of unlocked passives
    /// </summary>
    public int GetUnlockedPassiveCount()
    {
        return unlockedPassives.Count;
    }
    
    /// <summary>
    /// Refund all points (for testing or respec)
    /// </summary>
    public void RefundAllPoints()
    {
        availableAscendancyPoints = totalAscendancyPoints;
        spentAscendancyPoints = 0;
        unlockedPassives.Clear();
        choiceNodeSelections.Clear(); // Clear all sub-node selections
        Debug.Log($"[AscendancyProgress] Refunded all points. Available: {availableAscendancyPoints}");
    }
    
    /// <summary>
    /// Refund a specific passive (for respec)
    /// </summary>
    public bool RefundPassive(string passiveName, AscendancyData ascendancy)
    {
        if (!unlockedPassives.Contains(passiveName))
        {
            Debug.LogWarning($"[AscendancyProgress] Cannot refund '{passiveName}' - not unlocked");
            return false;
        }
        
        // Find passive to get point cost
        AscendancyPassive passive = ascendancy?.FindPassiveByName(passiveName);
        if (passive != null)
        {
            availableAscendancyPoints += passive.pointCost;
            spentAscendancyPoints -= passive.pointCost;
        }
        
        unlockedPassives.Remove(passiveName);
        
        // Clear sub-node selection if this was a choice node
        if (passive != null && passive.isChoiceNode)
        {
            ClearSubNodeSelection(passiveName);
        }
        
        Debug.Log($"[AscendancyProgress] Refunded passive: {passiveName}");
        return true;
    }
    
    /// <summary>
    /// Get summary of progression
    /// </summary>
    public string GetProgressSummary()
    {
        if (!ascendancyUnlocked)
            return "No Ascendancy chosen";
        
        return $"{selectedAscendancy}\n" +
               $"Points: {spentAscendancyPoints}/{totalAscendancyPoints} spent ({availableAscendancyPoints} available)\n" +
               $"Passives: {unlockedPassives.Count} unlocked";
    }
}

