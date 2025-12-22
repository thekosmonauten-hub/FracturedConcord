using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Collects and applies warrant modifiers to character stats
/// </summary>
public static class WarrantModifierCollector
{
    /// <summary>
    /// Lightweight method to apply warrant modifiers from saved state without requiring WarrantScene components.
    /// This can be called early (e.g., on character load) to ensure modifiers are applied before combat.
    /// </summary>
    public static void ApplyWarrantModifiersFromSavedState(Character character)
    {
        if (character == null)
        {
            Debug.LogWarning("[WarrantModifierCollector] Cannot apply modifiers: character is null");
            return;
        }

        // Get warrant board state JSON from saved data
        string stateJson = null;
        var warrantsManager = WarrantsManager.Instance;
        if (warrantsManager != null)
        {
            stateJson = warrantsManager.GetWarrantBoardStateJson();
        }
        
        // Fallback: Try to load from CharacterData
        if (string.IsNullOrEmpty(stateJson))
        {
            var charManager = CharacterManager.Instance;
            if (charManager != null)
            {
                var saveSystem = CharacterSaveSystem.Instance;
                if (saveSystem != null)
                {
                    CharacterData characterData = saveSystem.GetCharacter(character.characterName);
                    if (characterData != null && !string.IsNullOrEmpty(characterData.warrantBoardStateJson))
                    {
                        stateJson = characterData.warrantBoardStateJson;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(stateJson))
        {
            Debug.Log("[WarrantModifierCollector] No warrant board state found. Character may not have any warrants socketed yet.");
            return;
        }

        // Parse the JSON state
        WarrantBoardStateController.WarrantBoardSaveData saveData = null;
        try
        {
            saveData = JsonUtility.FromJson<WarrantBoardStateController.WarrantBoardSaveData>(stateJson);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[WarrantModifierCollector] Failed to parse warrant board state JSON: {e.Message}");
            return;
        }

        if (saveData == null || saveData.pages == null || saveData.pages.Count == 0)
        {
            Debug.Log("[WarrantModifierCollector] Warrant board state has no pages.");
            return;
        }

        // Get active page
        int activePageIndex = Mathf.Clamp(saveData.activePageIndex, 0, saveData.pages.Count - 1);
        var activePage = saveData.pages[activePageIndex];
        if (activePage == null)
        {
            Debug.LogWarning("[WarrantModifierCollector] Active page is null.");
            return;
        }

        // Rebuild unlocked node cache after deserialization
        activePage.RebuildUnlockedCache();

        // Load warrant database
        WarrantDatabase warrantDatabase = Resources.Load<WarrantDatabase>("WarrantDatabase");
        if (warrantDatabase == null)
        {
            Debug.LogWarning("[WarrantModifierCollector] WarrantDatabase not found in Resources. Cannot load warrant definitions.");
            return;
        }

        // Load notable database
        WarrantNotableDatabase notableDatabase = Resources.Load<WarrantNotableDatabase>("WarrantNotableDatabase");
        if (notableDatabase == null)
        {
            Debug.LogWarning("[WarrantModifierCollector] WarrantNotableDatabase not found in Resources. Notable modifiers may not be applied.");
        }

        // Collect modifiers from socketed warrants
        var allModifiers = new List<WarrantModifier>();

        // Iterate through all assignments in the active page
        if (activePage.SocketAssignments != null)
        {
            foreach (var assignment in activePage.SocketAssignments)
            {
                if (assignment == null)
                    continue;

                // Only process if the node is unlocked
                if (!activePage.IsNodeUnlocked(assignment.nodeId))
                    continue;

                string warrantId = assignment.warrantId;
                if (string.IsNullOrEmpty(warrantId))
                    continue;

                // Get warrant definition from character's owned warrants or database
                WarrantDefinition warrant = GetWarrantDefinition(warrantId, character, warrantDatabase);
                if (warrant == null)
                {
                    Debug.LogWarning($"[WarrantModifierCollector] Warrant '{warrantId}' not found in character's owned warrants or database.");
                    continue;
                }

                // Collect modifiers from this warrant
                CollectWarrantModifiersLightweight(warrant, allModifiers, notableDatabase);
            }
        }

        // Apply modifiers to character
        ApplyModifiersToCharacter(character, allModifiers);
        Debug.Log($"[WarrantModifierCollector] Applied {allModifiers.Count} warrant modifiers from saved state to character.");
    }

    /// <summary>
    /// Get warrant definition from character's owned warrants or database
    /// </summary>
    private static WarrantDefinition GetWarrantDefinition(string warrantId, Character character, WarrantDatabase warrantDatabase)
    {
        // First, try to find in character's owned warrants
        if (character.ownedWarrants != null)
        {
            foreach (var warrantData in character.ownedWarrants)
            {
                if (warrantData != null && warrantData.warrantId == warrantId)
                {
                    // Convert WarrantInstanceData to WarrantDefinition
                    WarrantDefinition warrant = warrantData.ToWarrantDefinition(warrantDatabase, null);
                    if (warrant != null)
                        return warrant;
                }
            }
        }

        // Fallback: Try to get from database (this won't have instance-specific data like iconIndex)
        return warrantDatabase != null ? warrantDatabase.GetById(warrantId) : null;
    }

    /// <summary>
    /// Collect modifiers from a warrant (lightweight version, no runtime graph needed)
    /// </summary>
    private static void CollectWarrantModifiersLightweight(WarrantDefinition warrant, List<WarrantModifier> output, WarrantNotableDatabase notableDatabase)
    {
        if (warrant == null)
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

        // Add notable modifiers
        if (warrant.notable != null && warrant.notable.modifiers != null)
        {
            foreach (var notableMod in warrant.notable.modifiers)
            {
                if (notableMod != null)
                {
                    output.Add(notableMod);
                }
            }
        }
        // New system: Load notable from database using notableId
        else if (!string.IsNullOrWhiteSpace(warrant.notableId) && notableDatabase != null)
        {
            var notableEntry = notableDatabase.GetById(warrant.notableId);
            if (notableEntry != null && notableEntry.modifiers != null)
            {
                foreach (var notableMod in notableEntry.modifiers)
                {
                    if (notableMod != null && !string.IsNullOrWhiteSpace(notableMod.statKey))
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
                    Debug.Log($"[WarrantModifierCollector] Loading Notable '{warrant.notableId}' with {notableEntry.modifiers.Count} modifiers");
                    foreach (var notableMod in notableEntry.modifiers)
                    {
                        if (notableMod != null && !string.IsNullOrWhiteSpace(notableMod.statKey))
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
                            Debug.Log($"[WarrantModifierCollector] Added Notable modifier: statKey='{notableMod.statKey}', value={notableMod.value}, displayName='{notableMod.displayName}'");
                        }
                        else
                        {
                            Debug.LogWarning($"[WarrantModifierCollector] Notable modifier is null or has empty statKey for Notable '{warrant.notableId}'");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[WarrantModifierCollector] Notable entry '{warrant.notableId}' not found or has no modifiers");
                }
            }
            else
            {
                Debug.LogWarning($"[WarrantModifierCollector] NotableDatabase is null, cannot load Notable '{warrant.notableId}'");
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
            float value = kvp.Value; // Percentage value (8 = 8%) or flat value depending on modifier type
            
            // Strip __socket_only__ prefix if present (used in Notable database for clarity)
            string cleanModifierId = modifierId;
            if (modifierId.StartsWith("__socket_only__", System.StringComparison.OrdinalIgnoreCase))
            {
                cleanModifierId = modifierId.Substring("__socket_only__".Length);
                Debug.Log($"[WarrantModifierCollector] Stripped '__socket_only__' prefix from '{modifierId}' -> '{cleanModifierId}'");
            }
            
            // Map modifier IDs to appropriate stat systems
            string modIdLower = cleanModifierId.ToLowerInvariant();
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
            // ===== ATTRIBUTE MODIFIERS (Applied directly to character attributes) =====
            // These are flat values (60 = +60 strength), not percentages
            else if (modIdLower == "strength")
            {
                // Apply as flat value to character strength
                character.strength += Mathf.RoundToInt(value);
                applied = true;
                statModifierCount++;
                Debug.Log($"[WarrantModifierCollector] Applied {value} flat strength to character (new total: {character.strength})");
            }
            else if (modIdLower == "dexterity")
            {
                // Apply as flat value to character dexterity
                character.dexterity += Mathf.RoundToInt(value);
                applied = true;
                statModifierCount++;
                Debug.Log($"[WarrantModifierCollector] Applied {value} flat dexterity to character (new total: {character.dexterity})");
            }
            else if (modIdLower == "intelligence")
            {
                // Apply as flat value to character intelligence
                character.intelligence += Mathf.RoundToInt(value);
                applied = true;
                statModifierCount++;
                Debug.Log($"[WarrantModifierCollector] Applied {value} flat intelligence to character (new total: {character.intelligence})");
            }
            // ===== LIFE REGENERATION MODIFIERS =====
            // These can be flat (1.5 = +1.5 per turn) or percentage (1.5% = 1.5% increased)
            else if (modIdLower == "liferegeneration")
            {
                // Store as flat value - CharacterStatsData will apply it
                // Note: If the value is meant to be percentage, it should be "liferegenerationincreased"
                character.warrantStatModifiers["lifeRegeneration"] = value;
                applied = true;
                statModifierCount++;
                Debug.Log($"[WarrantModifierCollector] Applied {value} life regeneration to character");
            }
            else if (modIdLower == "liferegenerationincreased")
            {
                // Store as percentage value (1.5 = 1.5% increased)
                character.warrantStatModifiers["lifeRegenerationIncreased"] = value;
                applied = true;
                statModifierCount++;
                Debug.Log($"[WarrantModifierCollector] Applied {value}% increased life regeneration to character");
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
                // Use cleanModifierId to store (without __socket_only__ prefix)
                character.warrantStatModifiers[cleanModifierId] = value;
                applied = true;
                statModifierCount++;
            }
            else
            {
                // Check if this might be a Notable modifier that wasn't recognized
                Debug.LogWarning($"[WarrantModifierCollector] Unknown modifier ID: '{cleanModifierId}' (original: '{modifierId}', value: {value}%). " +
                    $"This modifier will not be applied to character stats. " +
                    $"Check if the statKey in WarrantNotableDatabase matches the expected modifier IDs.");
            }
        }
        
        Debug.Log($"[WarrantModifierCollector] Applied {damageModifierCount} damage modifiers and {statModifierCount} stat modifiers to character");
    }
}

