using UnityEngine;
using System.Linq;

/// <summary>
/// Test helper for Ascendancy features - provides context menu methods for debugging
/// </summary>
public class AscendancyTestHelper : MonoBehaviour
{
    [Header("Test Settings")]
    public string testAscendancyName = "Inquisitor";
    public int testPoints = 1;
    [Tooltip("Passive name to unlock (e.g., 'Tolerance', 'Arcane Rebound', 'Tolerance_Start')")]
    public string passiveNameToUnlock = "Tolerance";
    
    /// <summary>
    /// Assign an Ascendancy to the current character
    /// Uses testAscendancyName field if set, otherwise falls back to first Ascendancy for character's class
    /// </summary>
    [ContextMenu("Assign Ascendancy to Character")]
    public void AssignAscendancyToCharacter()
    {
        var charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter())
        {
            Debug.LogError("[AscendancyTestHelper] No character found!");
            return;
        }
        
        var character = charManager.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogError("[AscendancyTestHelper] Character is null!");
            return;
        }
        
        // Get AscendancyDatabase
        var ascendancyDB = AscendancyDatabase.Instance;
        if (ascendancyDB == null)
        {
            Debug.LogError("[AscendancyTestHelper] AscendancyDatabase not found!");
            return;
        }
        
        // If character doesn't have an Ascendancy, assign one
        if (character.ascendancyProgress == null || string.IsNullOrEmpty(character.ascendancyProgress.selectedAscendancy))
        {
            AscendancyData ascendancyToAssign = null;
            
            // First, try to find Ascendancy by testAscendancyName (if set)
            if (!string.IsNullOrEmpty(testAscendancyName))
            {
                ascendancyToAssign = ascendancyDB.GetAscendancy(testAscendancyName);
                if (ascendancyToAssign == null)
                {
                    // Try case-insensitive search
                    var allAscendancies = ascendancyDB.GetAllAscendancies();
                    if (allAscendancies != null)
                    {
                        ascendancyToAssign = allAscendancies.FirstOrDefault(a => 
                            a != null && (
                                a.ascendancyName.Equals(testAscendancyName, System.StringComparison.OrdinalIgnoreCase) ||
                                a.ascendancyName.Contains(testAscendancyName, System.StringComparison.OrdinalIgnoreCase) ||
                                testAscendancyName.Contains(a.ascendancyName, System.StringComparison.OrdinalIgnoreCase)
                            ));
                    }
                }
                
                if (ascendancyToAssign != null)
                {
                    Debug.Log($"[AscendancyTestHelper] Found Ascendancy by name: {ascendancyToAssign.ascendancyName} (searched for: {testAscendancyName})");
                }
                else
                {
                    Debug.LogWarning($"[AscendancyTestHelper] Ascendancy '{testAscendancyName}' not found. Falling back to first Ascendancy for class '{character.characterClass}'");
                }
            }
            
            // Fallback: Get first Ascendancy for this character's class
            if (ascendancyToAssign == null)
            {
                var classAscendancies = ascendancyDB.GetAscendanciesForClass(character.characterClass);
                if (classAscendancies != null && classAscendancies.Count > 0)
                {
                    ascendancyToAssign = classAscendancies[0];
                    Debug.Log($"[AscendancyTestHelper] Using first Ascendancy for class '{character.characterClass}': {ascendancyToAssign.ascendancyName}");
                }
            }
            
            if (ascendancyToAssign != null)
            {
                if (character.ascendancyProgress == null)
                {
                    character.ascendancyProgress = new CharacterAscendancyProgress();
                }
                // Use ChooseAscendancy to auto-allocate starter node
                character.ascendancyProgress.ChooseAscendancy(ascendancyToAssign.ascendancyName, ascendancyToAssign);
                Debug.Log($"[AscendancyTestHelper] ✓ Assigned Ascendancy: {ascendancyToAssign.ascendancyName} to {character.characterName} (starter node auto-allocated)");
            }
            else
            {
                Debug.LogError($"[AscendancyTestHelper] No Ascendancies found! (Searched for: '{testAscendancyName}', Class: '{character.characterClass}')");
            }
        }
        else
        {
            Debug.Log($"[AscendancyTestHelper] Character already has Ascendancy: {character.ascendancyProgress.selectedAscendancy}");
        }
    }
    
    /// <summary>
    /// Give Ascendancy points to the current character
    /// </summary>
    [ContextMenu("Give Ascendancy Points")]
    public void GiveAscendancyPoints()
    {
        var charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter())
        {
            Debug.LogError("[AscendancyTestHelper] No character found!");
            return;
        }
        
        var character = charManager.GetCurrentCharacter();
        if (character == null || character.ascendancyProgress == null)
        {
            Debug.LogError("[AscendancyTestHelper] Character or ascendancyProgress is null! Assign an Ascendancy first.");
            return;
        }
        
        character.ascendancyProgress.availableAscendancyPoints += testPoints;
        Debug.Log($"[AscendancyTestHelper] Gave {testPoints} Ascendancy points. Total: {character.ascendancyProgress.availableAscendancyPoints}");
    }
    
    /// <summary>
    /// Show current Ascendancy status
    /// </summary>
    [ContextMenu("Show Ascendancy Status")]
    public void ShowAscendancyStatus()
    {
        var charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter())
        {
            Debug.LogError("[AscendancyTestHelper] No character found!");
            return;
        }
        
        var character = charManager.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogError("[AscendancyTestHelper] Character is null!");
            return;
        }
        
        if (character.ascendancyProgress == null)
        {
            Debug.Log("[AscendancyTestHelper] Character has no Ascendancy assigned.");
            return;
        }
        
        Debug.Log($"=== Ascendancy Status for {character.characterName} ===");
        Debug.Log($"Selected Ascendancy: {character.ascendancyProgress.selectedAscendancy}");
        Debug.Log($"Available Points: {character.ascendancyProgress.availableAscendancyPoints}");
        Debug.Log($"Unlocked Passives: {character.ascendancyProgress.unlockedPassives?.Count ?? 0}");
        
        if (character.ascendancyProgress.unlockedPassives != null && character.ascendancyProgress.unlockedPassives.Count > 0)
        {
            Debug.Log("Unlocked Passives:");
            foreach (var passive in character.ascendancyProgress.unlockedPassives)
            {
                Debug.Log($"  - {passive}");
            }
        }
    }
    
    /// <summary>
    /// Unlock a passive by name (uses passiveNameToUnlock field)
    /// </summary>
    [ContextMenu("Unlock Passive By Name")]
    public void UnlockPassiveByName()
    {
        if (string.IsNullOrEmpty(passiveNameToUnlock))
        {
            Debug.LogError("[AscendancyTestHelper] passiveNameToUnlock is empty! Set it in the Inspector.");
            return;
        }
        
        UnlockPassiveByName(passiveNameToUnlock);
    }
    
    /// <summary>
    /// Unlock a passive by name (flexible - searches for partial matches)
    /// </summary>
    public void UnlockPassiveByName(string passiveName)
    {
        if (string.IsNullOrEmpty(passiveName))
        {
            Debug.LogError("[AscendancyTestHelper] Passive name cannot be empty!");
            return;
        }
        
        var charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter())
        {
            Debug.LogError("[AscendancyTestHelper] No character found!");
            return;
        }
        
        var character = charManager.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogError("[AscendancyTestHelper] Character is null!");
            return;
        }
        
        // Ensure character has an Ascendancy
        if (character.ascendancyProgress == null || string.IsNullOrEmpty(character.ascendancyProgress.selectedAscendancy))
        {
            AssignAscendancyToCharacter();
        }
        
        if (character.ascendancyProgress == null)
        {
            Debug.LogError("[AscendancyTestHelper] Could not assign Ascendancy!");
            return;
        }
        
        // Get AscendancyDatabase
        var ascendancyDB = AscendancyDatabase.Instance;
        if (ascendancyDB == null)
        {
            Debug.LogError("[AscendancyTestHelper] AscendancyDatabase not found!");
            return;
        }
        
        // Get the Ascendancy data
        var ascendancy = ascendancyDB.GetAscendancy(character.ascendancyProgress.selectedAscendancy);
        if (ascendancy == null)
        {
            Debug.LogError($"[AscendancyTestHelper] Ascendancy not found: {character.ascendancyProgress.selectedAscendancy}");
            return;
        }
        
        // Try to find the passive by name (flexible matching)
        AscendancyPassive foundPassive = null;
        
        // First try exact match
        foundPassive = ascendancy.FindPassiveByName(passiveName);
        
        // If not found, try partial match (contains)
        if (foundPassive == null && ascendancy.passiveAbilities != null)
        {
            foundPassive = ascendancy.passiveAbilities.FirstOrDefault(p => 
                p != null && (
                    p.name.Equals(passiveName, System.StringComparison.OrdinalIgnoreCase) ||
                    p.name.Contains(passiveName, System.StringComparison.OrdinalIgnoreCase) ||
                    passiveName.Contains(p.name, System.StringComparison.OrdinalIgnoreCase)
                ));
        }
        
        if (foundPassive == null)
        {
            Debug.LogError($"[AscendancyTestHelper] Passive '{passiveName}' not found in Ascendancy '{ascendancy.ascendancyName}'!");
            Debug.Log($"Available passives: {string.Join(", ", ascendancy.passiveAbilities?.Select(p => p?.name).Where(n => !string.IsNullOrEmpty(n)) ?? new string[0])}");
            return;
        }
        
        // Check if already unlocked
        if (character.ascendancyProgress.IsPassiveUnlocked(foundPassive.name))
        {
            Debug.Log($"[AscendancyTestHelper] Passive '{foundPassive.name}' is already unlocked!");
            return;
        }
        
        // Ensure we have enough points (give points if needed)
        if (character.ascendancyProgress.availableAscendancyPoints < foundPassive.pointCost)
        {
            int needed = foundPassive.pointCost - character.ascendancyProgress.availableAscendancyPoints;
            character.ascendancyProgress.availableAscendancyPoints += needed;
            Debug.Log($"[AscendancyTestHelper] Auto-granted {needed} points to unlock '{foundPassive.name}'");
        }
        
        // Unlock the passive
        bool unlocked = character.ascendancyProgress.UnlockPassive(foundPassive, ascendancy);
        if (unlocked)
        {
            Debug.Log($"<color=green>[AscendancyTestHelper] ✓ Unlocked: {foundPassive.name}</color>");
            
            // Note: Modifier activation will be handled automatically when combat starts
            // or when the AscendancyModifierHandler loads active modifiers
            Debug.Log($"[AscendancyTestHelper] Modifier will activate when combat starts or handler loads");
        }
        else
        {
            Debug.LogWarning($"[AscendancyTestHelper] Failed to unlock: {foundPassive.name}");
        }
    }
    
    /// <summary>
    /// Unlock the Tolerance passive (for testing - legacy method)
    /// </summary>
    [ContextMenu("Unlock Tolerance Passive")]
    public void UnlockTolerancePassive()
    {
        UnlockPassiveByName("Tolerance");
    }
    
    /// <summary>
    /// Clear all Ascendancy data from the character
    /// </summary>
    [ContextMenu("Clear Ascendancy Data")]
    public void ClearAscendancyData()
    {
        var charManager = CharacterManager.Instance;
        if (charManager == null || !charManager.HasCharacter())
        {
            Debug.LogError("[AscendancyTestHelper] No character found!");
            return;
        }
        
        var character = charManager.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogError("[AscendancyTestHelper] Character is null!");
            return;
        }
        
        character.ascendancyProgress = new CharacterAscendancyProgress();
        Debug.Log("[AscendancyTestHelper] Cleared all Ascendancy data from character");
    }
}

