using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime helper for testing Reliance Auras in-game.
/// Can be attached to a GameObject or called from code.
/// </summary>
public class AuraTestingHelper : MonoBehaviour
{
    [Header("Quick Actions")]
    [Tooltip("Unlock all auras for the current character")]
    public bool unlockAllAuras = false;
    
    [Tooltip("Activate all owned auras")]
    public bool activateAllAuras = false;
    
    [Tooltip("Give experience to all active auras")]
    public bool giveExperienceToActive = false;
    
    [Tooltip("Amount of experience to give")]
    public int experienceAmount = 10000;
    
    [Header("Single Aura Testing")]
    [Tooltip("Aura name to test")]
    public string testAuraName = "";
    
    [Tooltip("Give experience to test aura")]
    public bool giveExperienceToTestAura = false;
    
    [Tooltip("Activate test aura")]
    public bool activateTestAura = false;
    
    [Tooltip("Deactivate test aura")]
    public bool deactivateTestAura = false;
    
    void Update()
    {
        // Quick actions (triggered when bools are set to true)
        if (unlockAllAuras)
        {
            unlockAllAuras = false;
            UnlockAllAuras();
        }
        
        if (activateAllAuras)
        {
            activateAllAuras = false;
            ActivateAllOwnedAuras();
        }
        
        if (giveExperienceToActive)
        {
            giveExperienceToActive = false;
            GiveExperienceToActiveAuras(experienceAmount);
        }
        
        // Test aura actions
        if (!string.IsNullOrEmpty(testAuraName))
        {
            if (giveExperienceToTestAura)
            {
                giveExperienceToTestAura = false;
                GiveExperienceToAura(testAuraName, experienceAmount);
            }
            
            if (activateTestAura)
            {
                activateTestAura = false;
                ActivateAura(testAuraName);
            }
            
            if (deactivateTestAura)
            {
                deactivateTestAura = false;
                DeactivateAura(testAuraName);
            }
        }
    }
    
    /// <summary>
    /// Unlock all auras for the current character
    /// </summary>
    [ContextMenu("Unlock All Auras")]
    public void UnlockAllAuras()
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[AuraTestingHelper] No character loaded!");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("[AuraTestingHelper] Character is null!");
            return;
        }
        
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        if (database == null)
        {
            Debug.LogWarning("[AuraTestingHelper] RelianceAuraDatabase not found!");
            return;
        }
        
        if (character.ownedRelianceAuras == null)
        {
            character.ownedRelianceAuras = new List<string>();
        }
        
        List<RelianceAura> allAuras = database.GetAllAuras();
        int unlockedCount = 0;
        
        foreach (RelianceAura aura in allAuras)
        {
            if (aura != null && !character.ownedRelianceAuras.Contains(aura.auraName))
            {
                character.ownedRelianceAuras.Add(aura.auraName);
                unlockedCount++;
            }
        }
        
        Debug.Log($"[AuraTestingHelper] Unlocked {unlockedCount} auras for {character.characterName}");
    }
    
    /// <summary>
    /// Activate all owned auras
    /// </summary>
    [ContextMenu("Activate All Owned Auras")]
    public void ActivateAllOwnedAuras()
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[AuraTestingHelper] No character loaded!");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null || character.ownedRelianceAuras == null || character.ownedRelianceAuras.Count == 0)
        {
            Debug.LogWarning("[AuraTestingHelper] No owned auras to activate!");
            return;
        }
        
        if (character.activeRelianceAuras == null)
        {
            character.activeRelianceAuras = new List<string>();
        }
        
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        int activatedCount = 0;
        
        foreach (string auraName in character.ownedRelianceAuras)
        {
            if (!character.activeRelianceAuras.Contains(auraName))
            {
                character.activeRelianceAuras.Add(auraName);
                activatedCount++;
            }
        }
        
        Debug.Log($"[AuraTestingHelper] Activated {activatedCount} auras");
    }
    
    /// <summary>
    /// Give experience to all active auras
    /// </summary>
    [ContextMenu("Give Experience to Active Auras")]
    public void GiveExperienceToActiveAuras(int amount)
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[AuraTestingHelper] No character loaded!");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null || character.activeRelianceAuras == null || character.activeRelianceAuras.Count == 0)
        {
            Debug.LogWarning("[AuraTestingHelper] No active auras!");
            return;
        }
        
        AuraExperienceManager expManager = AuraExperienceManager.Instance;
        if (expManager == null)
        {
            Debug.LogWarning("[AuraTestingHelper] AuraExperienceManager not found!");
            return;
        }
        
        foreach (string auraName in character.activeRelianceAuras)
        {
            expManager.AddAuraExperience(auraName, amount);
        }
        
        Debug.Log($"[AuraTestingHelper] Gave {amount} XP to {character.activeRelianceAuras.Count} active auras");
    }
    
    /// <summary>
    /// Give experience to a specific aura
    /// </summary>
    public void GiveExperienceToAura(string auraName, int amount)
    {
        AuraExperienceManager expManager = AuraExperienceManager.Instance;
        if (expManager == null)
        {
            Debug.LogWarning("[AuraTestingHelper] AuraExperienceManager not found!");
            return;
        }
        
        expManager.AddAuraExperience(auraName, amount);
        int level = expManager.GetAuraLevel(auraName);
        Debug.Log($"[AuraTestingHelper] Gave {amount} XP to {auraName} (now level {level})");
    }
    
    /// <summary>
    /// Activate a specific aura
    /// </summary>
    public void ActivateAura(string auraName)
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[AuraTestingHelper] No character loaded!");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null)
        {
            Debug.LogWarning("[AuraTestingHelper] Character is null!");
            return;
        }
        
        if (character.activeRelianceAuras == null)
        {
            character.activeRelianceAuras = new List<string>();
        }
        
        if (!character.activeRelianceAuras.Contains(auraName))
        {
            character.activeRelianceAuras.Add(auraName);
            Debug.Log($"[AuraTestingHelper] Activated {auraName}");
        }
    }
    
    /// <summary>
    /// Deactivate a specific aura
    /// </summary>
    public void DeactivateAura(string auraName)
    {
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            Debug.LogWarning("[AuraTestingHelper] No character loaded!");
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null || character.activeRelianceAuras == null)
        {
            return;
        }
        
        if (character.activeRelianceAuras.Remove(auraName))
        {
            Debug.Log($"[AuraTestingHelper] Deactivated {auraName}");
        }
    }
    
    /// <summary>
    /// Set an aura to a specific level (gives enough XP to reach that level)
    /// </summary>
    [ContextMenu("Set Test Aura to Level 20")]
    public void SetTestAuraToLevel20()
    {
        if (string.IsNullOrEmpty(testAuraName))
        {
            Debug.LogWarning("[AuraTestingHelper] No test aura name set!");
            return;
        }
        
        SetAuraToLevel(testAuraName, 20);
    }
    
    /// <summary>
    /// Set an aura to a specific level
    /// </summary>
    public void SetAuraToLevel(string auraName, int targetLevel)
    {
        AuraExperienceManager expManager = AuraExperienceManager.Instance;
        if (expManager == null)
        {
            Debug.LogWarning("[AuraTestingHelper] AuraExperienceManager not found!");
            return;
        }
        
        AuraExperienceData data = expManager.GetAuraExperienceData(auraName);
        int currentLevel = data.level;
        
        if (currentLevel >= targetLevel)
        {
            Debug.Log($"[AuraTestingHelper] {auraName} is already level {currentLevel} (target: {targetLevel})");
            return;
        }
        
        // Calculate total XP needed
        int totalXPNeeded = 0;
        for (int level = currentLevel; level < targetLevel; level++)
        {
            data.level = level;
            totalXPNeeded += data.GetRequiredExperienceForNextLevel();
        }
        
        // Add extra to ensure we reach the level
        expManager.AddAuraExperience(auraName, totalXPNeeded + 100);
        
        int newLevel = expManager.GetAuraLevel(auraName);
        Debug.Log($"[AuraTestingHelper] Set {auraName} to level {newLevel} (target was {targetLevel})");
    }
}

