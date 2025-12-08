using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Collects and applies warrant modifiers to character stats
/// </summary>
public static class WarrantModifierCollector
{
    /// <summary>
    /// Collect all active warrant modifiers from the warrant board and apply them to character
    /// </summary>
    public static void ApplyWarrantModifiersToCharacter(Character character, WarrantBoardStateController boardState, WarrantLockerGrid lockerGrid, WarrantBoardRuntimeGraph runtimeGraph)
    {
        if (character == null || boardState == null || lockerGrid == null || runtimeGraph == null)
        {
            Debug.LogWarning("[WarrantModifierCollector] Missing required references");
            return;
        }

        // Collect all active modifiers
        var collectedModifiers = CollectActiveWarrantModifiers(boardState, lockerGrid, runtimeGraph);
        
        // Apply to character's damage modifiers
        ApplyModifiersToCharacter(character, collectedModifiers);
        
        Debug.Log($"[WarrantModifierCollector] Applied {collectedModifiers.Count} warrant modifiers to character");
    }
    
    /// <summary>
    /// Collect all active warrant modifiers from the board
    /// </summary>
    public static List<WarrantModifier> CollectActiveWarrantModifiers(WarrantBoardStateController boardState, WarrantLockerGrid lockerGrid, WarrantBoardRuntimeGraph runtimeGraph)
    {
        var allModifiers = new List<WarrantModifier>();
        
        if (boardState == null || lockerGrid == null || runtimeGraph == null)
            return allModifiers;
        
        var activePage = boardState.ActivePage;
        if (activePage == null)
            return allModifiers;
        
        // Get all unlocked socket nodes with assigned warrants
        var socketNodes = runtimeGraph.Nodes.Values
            .Where(n => (n.NodeType == WarrantNodeType.Socket || n.NodeType == WarrantNodeType.SpecialSocket) 
                     && activePage.IsNodeUnlocked(n.Id));
        
        foreach (var socketNode in socketNodes)
        {
            string warrantId = boardState.GetWarrantAtNode(socketNode.Id);
            if (string.IsNullOrEmpty(warrantId))
                continue;
            
            WarrantDefinition warrant = lockerGrid.GetDefinition(warrantId);
            if (warrant == null)
                continue;
            
            // Collect modifiers from the warrant itself (notable + regular modifiers)
            CollectWarrantModifiers(warrant, socketNode, runtimeGraph, activePage, allModifiers, lockerGrid);
        }
        
        return allModifiers;
    }
    
    /// <summary>
    /// Collect modifiers from a warrant, including effect nodes within range
    /// </summary>
    private static void CollectWarrantModifiers(WarrantDefinition warrant, WarrantBoardRuntimeGraph.Node socketNode, 
        WarrantBoardRuntimeGraph runtimeGraph, WarrantBoardStateController.WarrantBoardPageData activePage, List<WarrantModifier> output, WarrantLockerGrid lockerGrid)
    {
        if (warrant == null || socketNode == null)
            return;
        
        // Add warrant's base modifiers (regular modifiers)
        if (warrant.modifiers != null)
        {
            foreach (var modifier in warrant.modifiers)
            {
                if (modifier != null)
                {
                    output.Add(modifier);
                }
            }
        }
        
        // Add notable modifiers - check both notable object and notableId
        // First check if warrant has a notable object (legacy)
        if (warrant.notable != null && warrant.notable.modifiers != null)
        {
            // WarrantNotableDefinition.modifiers is already List<WarrantModifier>, so add directly
            foreach (var notableMod in warrant.notable.modifiers)
            {
                if (notableMod != null)
                {
                    output.Add(notableMod);
                }
            }
        }
        // New system: Load notable from database using notableId
        else if (!string.IsNullOrWhiteSpace(warrant.notableId) && lockerGrid != null)
        {
            var notableDatabase = lockerGrid.GetNotableDatabase();
            if (notableDatabase != null)
            {
                var notableEntry = notableDatabase.GetById(warrant.notableId);
                if (notableEntry != null && notableEntry.modifiers != null)
                {
                    foreach (var notableMod in notableEntry.modifiers)
                    {
                        if (notableMod != null)
                        {
                            // Convert NotableModifier to WarrantModifier
                            var warrantMod = new WarrantModifier
                            {
                                modifierId = notableMod.statKey,
                                displayName = !string.IsNullOrWhiteSpace(notableMod.displayName) ? notableMod.displayName : notableMod.statKey,
                                operation = WarrantModifierOperation.Additive,
                                value = notableMod.value,
                                description = notableMod.displayName
                            };
                            output.Add(warrantMod);
                        }
                    }
                }
            }
        }
        
        // Find effect nodes within range and add their modifiers
        var effectNodes = FindEffectNodesWithinRange(socketNode, warrant.rangeDepth, runtimeGraph, activePage);
        
        foreach (var effectNode in effectNodes)
        {
            // Effect nodes get modifiers from the warrant's regular modifiers (NOT notables)
            // Notables are socket-only and should NOT be applied to effect nodes
            // For each effect node within range, add the warrant's regular modifiers again
            // This represents the "per effect node" bonus mentioned in the tooltip
            if (warrant.modifiers != null)
            {
                foreach (var modifier in warrant.modifiers)
                {
                    if (modifier != null)
                    {
                        // Add the same modifier again for this effect node
                        // This gives the "additional 8% per effect node" effect
                        output.Add(modifier);
                    }
                }
            }
            
            // NOTE: Notables are intentionally NOT added here - they are socket-only
            // Notables only apply to the socket node itself, not to effect nodes within range
        }
    }
    
    /// <summary>
    /// Find all effect nodes within range of a socket node
    /// </summary>
    private static List<WarrantBoardRuntimeGraph.Node> FindEffectNodesWithinRange(
        WarrantBoardRuntimeGraph.Node socketNode, int maxDepth, 
        WarrantBoardRuntimeGraph runtimeGraph, WarrantBoardStateController.WarrantBoardPageData activePage)
    {
        var result = new List<WarrantBoardRuntimeGraph.Node>();
        if (socketNode == null || maxDepth <= 0)
            return result;
        
        var visited = new HashSet<WarrantBoardRuntimeGraph.Node>();
        var queue = new Queue<(WarrantBoardRuntimeGraph.Node node, int depth)>();
        queue.Enqueue((socketNode, 0));
        visited.Add(socketNode);
        
        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();
            
            if (depth > maxDepth)
                continue;
            
            // If this is an effect node and it's unlocked, add it
            if (node.NodeType == WarrantNodeType.Effect && activePage.IsNodeUnlocked(node.Id))
            {
                if (!result.Contains(node))
                {
                    result.Add(node);
                }
            }
            
            // Explore neighbors
            foreach (var neighbor in node.Connections)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, depth + 1));
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Gather all warrants affecting an effect node
    /// </summary>
    private static List<WarrantDefinition> GatherAffectingWarrantsForNode(
        WarrantBoardRuntimeGraph.Node effectNode, WarrantBoardRuntimeGraph runtimeGraph,
        WarrantBoardStateController boardState, WarrantLockerGrid lockerGrid, WarrantBoardStateController.WarrantBoardPageData activePage)
    {
        var result = new List<WarrantDefinition>();
        if (effectNode == null)
            return result;
        
        // Find all socket nodes within range that have warrants
        var visited = new HashSet<WarrantBoardRuntimeGraph.Node>();
        var queue = new Queue<(WarrantBoardRuntimeGraph.Node node, int depth)>();
        queue.Enqueue((effectNode, 0));
        visited.Add(effectNode);
        
        const int MaxSearchDepth = 8;
        
        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();
            
            if (depth > MaxSearchDepth)
                continue;
            
            // If this is a socket node with a warrant, add it
            if ((node.NodeType == WarrantNodeType.Socket || node.NodeType == WarrantNodeType.SpecialSocket) 
                && activePage.IsNodeUnlocked(node.Id))
            {
                string warrantId = boardState.GetWarrantAtNode(node.Id);
                if (!string.IsNullOrEmpty(warrantId))
                {
                    WarrantDefinition warrant = lockerGrid.GetDefinition(warrantId);
                    if (warrant != null && !result.Contains(warrant))
                    {
                        // Check if warrant's range reaches this effect node
                        int distance = depth; // Distance from effect node to socket
                        if (distance <= warrant.rangeDepth)
                        {
                            result.Add(warrant);
                        }
                    }
                }
            }
            
            // Explore neighbors
            foreach (var neighbor in node.Connections)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, depth + 1));
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply collected modifiers to character's damage modifiers and stat modifiers
    /// </summary>
    private static void ApplyModifiersToCharacter(Character character, List<WarrantModifier> modifiers)
    {
        if (character == null || modifiers == null)
            return;
        
        var damageModifiers = character.GetDamageModifiers();
        if (damageModifiers == null)
            return;
        
        // Ensure warrantStatModifiers dictionary exists
        if (character.warrantStatModifiers == null)
        {
            character.warrantStatModifiers = new System.Collections.Generic.Dictionary<string, float>();
        }
        
        // Ensure warrantFlatModifiers dictionary exists
        if (character.warrantFlatModifiers == null)
        {
            character.warrantFlatModifiers = new System.Collections.Generic.Dictionary<string, float>();
        }
        
        // Group modifiers by ID to aggregate values
        var groupedModifiers = new System.Collections.Generic.Dictionary<string, float>();
        
        foreach (var modifier in modifiers)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.modifierId))
                continue;
            
            string key = modifier.modifierId.ToLowerInvariant();
            float value = modifier.value; // Value is in percentage form (8 = 8%)
            
            if (groupedModifiers.ContainsKey(key))
            {
                groupedModifiers[key] += value;
            }
            else
            {
                groupedModifiers[key] = value;
            }
        }
        
        int damageModifierCount = 0;
        int statModifierCount = 0;
        
        // Apply modifiers based on modifierId
        foreach (var kvp in groupedModifiers)
        {
            string modifierId = kvp.Key;
            float value = kvp.Value; // Percentage value (8 = 8%)
            
            // Map modifier IDs to appropriate stat systems
            string modIdLower = modifierId.ToLowerInvariant();
            bool applied = false;
            
            // ===== DAMAGE MODIFIERS (Applied to DamageModifiers lists) =====
            // These need to be converted to decimal (0.08) for DamageModifiers
            float decimalValue = value / 100f;
            
            if (modIdLower == "increasedphysicaldamage")
            {
                damageModifiers.increasedPhysicalDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            else if (modIdLower == "increasedfiredamage")
            {
                damageModifiers.increasedFireDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            else if (modIdLower == "increasedcolddamage")
            {
                damageModifiers.increasedColdDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            else if (modIdLower == "increasedlightningdamage")
            {
                damageModifiers.increasedLightningDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            else if (modIdLower == "increasedchaosdamage")
            {
                damageModifiers.increasedChaosDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            else if (modIdLower == "increasedelementaldamage")
            {
                // Apply to all elemental types
                damageModifiers.increasedFireDamage.Add(decimalValue);
                damageModifiers.increasedColdDamage.Add(decimalValue);
                damageModifiers.increasedLightningDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            else if (modIdLower == "increasedattackdamage")
            {
                damageModifiers.increasedAttackDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            else if (modIdLower == "increasedspelldamage")
            {
                damageModifiers.increasedSpellDamage.Add(decimalValue);
                applied = true;
                damageModifierCount++;
            }
            // ===== FLAT MODIFIERS (Applied to warrantFlatModifiers dictionary) =====
            // These are stored as flat values (40 = +40) and applied before percentage modifiers
            else if (modIdLower == "maxhealthflat" || modIdLower == "maxmanaflat")
            {
                // Store as flat value (40 = +40) - CharacterStatsData will apply before percentage modifiers
                character.warrantFlatModifiers[modifierId] = value;
                applied = true;
                statModifierCount++;
            }
            // ===== NON-DAMAGE STAT MODIFIERS (Applied to warrantStatModifiers dictionary) =====
            // These are stored as percentage values (8 = 8%) and applied to CharacterStatsData
            else if (modIdLower == "evasionincreased" || modIdLower == "maxhealthincreased" || 
                     modIdLower == "maxmanaincreased" || modIdLower == "energyshieldincreased" || 
                     modIdLower == "armourincreased" || modIdLower == "increasedprojectiledamage" ||
                     modIdLower == "increasedareadamage" || modIdLower == "increasedmeleedamage" ||
                     modIdLower == "increasedrangeddamage" || modIdLower == "increasedaxedamage" ||
                     modIdLower == "increasedbowdamage" || modIdLower == "increasedmacedamage" ||
                     modIdLower == "increasedsworddamage" || modIdLower == "increasedwanddamage" ||
                     modIdLower == "increaseddaggerdamage" || modIdLower == "increasedonehandeddamage" ||
                     modIdLower == "increasedtwohandeddamage" || modIdLower == "increasedignitemagnitude" ||
                     modIdLower == "increasedshockmagnitude" || modIdLower == "increasedchillmagnitude" ||
                     modIdLower == "increasedfreezemagnitude" || modIdLower == "increasedbleedmagnitude" ||
                     modIdLower == "increasedpoisonmagnitude" || modIdLower == "increaseddamageovertime" ||
                     modIdLower == "increasedpoisondamage" || modIdLower == "increasedpoisonduration" ||
                     modIdLower == "increaseddamagevschilled" || modIdLower == "increaseddamagevsshocked" ||
                     modIdLower == "increaseddamagevsignited" || modIdLower == "attackspeed" ||
                     modIdLower == "castspeed" || modIdLower == "statuseffectduration" ||
                     modIdLower == "aggressiongainincreased" || modIdLower == "focusgainincreased" ||
                     modIdLower == "guardeffectivenessincreased" || modIdLower == "lessdamagefromelites" ||
                     modIdLower == "statusavoidance")
            {
                // Store as percentage value (8 = 8%) - CharacterStatsData will handle it
                character.warrantStatModifiers[modifierId] = value;
                applied = true;
                statModifierCount++;
            }
            else
            {
                Debug.LogWarning($"[WarrantModifierCollector] Unknown modifier ID: {modifierId} (value: {value}%)");
            }
        }
        
        Debug.Log($"[WarrantModifierCollector] Applied {damageModifierCount} damage modifiers and {statModifierCount} stat modifiers to character");
    }
}

