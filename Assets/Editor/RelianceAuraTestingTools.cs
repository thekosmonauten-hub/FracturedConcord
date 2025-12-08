using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tools for testing Reliance Auras in-game.
/// Provides quick access to unlock, activate, and level up auras.
/// </summary>
public class RelianceAuraTestingTools : EditorWindow
{
    private Vector2 scrollPosition;
    private bool unlockAllAuras = true;
    private bool activateAllAuras = false;
    private bool setAllToLevel20 = false;
    private int customLevel = 1;
    private bool useCustomLevel = false;
    
    [MenuItem("Tools/Dexiled/Reliance Aura Testing Tools")]
    public static void ShowWindow()
    {
        GetWindow<RelianceAuraTestingTools>("Aura Testing Tools");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Reliance Aura Testing Tools", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        CharacterManager characterManager = CharacterManager.Instance;
        if (characterManager == null || !characterManager.HasCharacter())
        {
            EditorGUILayout.HelpBox("No character loaded. Please load a character first.", MessageType.Warning);
            return;
        }
        
        Character character = characterManager.GetCurrentCharacter();
        if (character == null)
        {
            EditorGUILayout.HelpBox("Character is null.", MessageType.Error);
            return;
        }
        
        // Character Info
        EditorGUILayout.LabelField("Character:", character.characterName);
        EditorGUILayout.LabelField("Owned Auras:", character.ownedRelianceAuras?.Count.ToString() ?? "0");
        EditorGUILayout.LabelField("Active Auras:", character.activeRelianceAuras?.Count.ToString() ?? "0");
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        
        // Options
        unlockAllAuras = EditorGUILayout.Toggle("Unlock All Auras", unlockAllAuras);
        activateAllAuras = EditorGUILayout.Toggle("Activate All Auras", activateAllAuras);
        
        GUILayout.Space(5);
        useCustomLevel = EditorGUILayout.Toggle("Set Custom Level", useCustomLevel);
        if (useCustomLevel)
        {
            customLevel = EditorGUILayout.IntSlider("Level", customLevel, 1, 20);
        }
        else
        {
            setAllToLevel20 = EditorGUILayout.Toggle("Set All to Level 20", setAllToLevel20);
        }
        
        GUILayout.Space(10);
        
        // Action Buttons
        if (GUILayout.Button("Apply Changes", GUILayout.Height(30)))
        {
            ApplyChanges(character);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Unlock All Auras Only", GUILayout.Height(25)))
        {
            UnlockAllAuras(character);
        }
        
        if (GUILayout.Button("Activate All Owned Auras", GUILayout.Height(25)))
        {
            ActivateAllOwnedAuras(character);
        }
        
        if (GUILayout.Button("Deactivate All Auras", GUILayout.Height(25)))
        {
            DeactivateAllAuras(character);
        }
        
        if (GUILayout.Button("Give 10,000 XP to All Active Auras", GUILayout.Height(25)))
        {
            GiveExperienceToActiveAuras(character, 10000);
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        
        // Aura List
        EditorGUILayout.LabelField("Aura Status:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        if (database != null)
        {
            List<RelianceAura> allAuras = database.GetAllAuras();
            AuraExperienceManager expManager = AuraExperienceManager.Instance;
            
            foreach (RelianceAura aura in allAuras)
            {
                if (aura == null) continue;
                
                bool isOwned = character.ownedRelianceAuras != null && 
                              character.ownedRelianceAuras.Contains(aura.auraName);
                bool isActive = character.activeRelianceAuras != null && 
                               character.activeRelianceAuras.Contains(aura.auraName);
                
                int level = 1;
                int experience = 0;
                if (expManager != null)
                {
                    level = expManager.GetAuraLevel(aura.auraName);
                    experience = expManager.GetAuraExperience(aura.auraName);
                }
                
                EditorGUILayout.BeginHorizontal();
                
                // Status indicators
                string status = "";
                if (isActive) status += "[ACTIVE] ";
                if (isOwned) status += "[OWNED] ";
                if (!isOwned) status += "[LOCKED] ";
                
                EditorGUILayout.LabelField($"{status}{aura.auraName}", GUILayout.Width(250));
                EditorGUILayout.LabelField($"Lv.{level} ({experience} XP)", GUILayout.Width(100));
                
                // Quick actions
                if (isOwned && !isActive)
                {
                    if (GUILayout.Button("Activate", GUILayout.Width(70)))
                    {
                        ActivateAura(character, aura);
                    }
                }
                else if (isActive)
                {
                    if (GUILayout.Button("Deactivate", GUILayout.Width(70)))
                    {
                        DeactivateAura(character, aura);
                    }
                }
                
                if (GUILayout.Button("+1000 XP", GUILayout.Width(70)))
                {
                    GiveExperienceToAura(aura.auraName, 1000);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("RelianceAuraDatabase not found. Make sure it exists in the scene.", MessageType.Warning);
        }
        
        EditorGUILayout.EndScrollView();
        
        GUILayout.Space(10);
        
        // Save button
        if (GUILayout.Button("Save Character", GUILayout.Height(25)))
        {
            characterManager.SaveCharacter();
            EditorUtility.DisplayDialog("Saved", "Character saved successfully!", "OK");
        }
    }
    
    private void ApplyChanges(Character character)
    {
        if (unlockAllAuras)
        {
            UnlockAllAuras(character);
        }
        
        if (activateAllAuras)
        {
            ActivateAllOwnedAuras(character);
        }
        
        // Set levels
        AuraExperienceManager expManager = AuraExperienceManager.Instance;
        if (expManager != null)
        {
            int targetLevel = useCustomLevel ? customLevel : (setAllToLevel20 ? 20 : 1);
            
            RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
            if (database != null)
            {
                List<RelianceAura> allAuras = database.GetAllAuras();
                foreach (RelianceAura aura in allAuras)
                {
                    if (aura == null) continue;
                    
                    // Calculate XP needed to reach target level
                    AuraExperienceData data = expManager.GetAuraExperienceData(aura.auraName);
                    int currentLevel = data.level;
                    
                    if (currentLevel < targetLevel)
                    {
                        // Give enough XP to reach target level
                        int totalXPNeeded = 0;
                        for (int level = currentLevel; level < targetLevel; level++)
                        {
                            data.level = level;
                            totalXPNeeded += data.GetRequiredExperienceForNextLevel();
                        }
                        
                        // Add a bit extra to ensure we reach the level
                        expManager.AddAuraExperience(aura.auraName, totalXPNeeded + 100);
                    }
                }
            }
        }
        
        EditorUtility.DisplayDialog("Applied", "Changes applied successfully!", "OK");
        Repaint();
    }
    
    private void UnlockAllAuras(Character character)
    {
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        if (database == null)
        {
            EditorUtility.DisplayDialog("Error", "RelianceAuraDatabase not found!", "OK");
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
        
        Debug.Log($"[Aura Testing] Unlocked {unlockedCount} auras for {character.characterName}");
        EditorUtility.DisplayDialog("Unlocked", $"Unlocked {unlockedCount} auras!", "OK");
        Repaint();
    }
    
    private void ActivateAllOwnedAuras(Character character)
    {
        if (character.ownedRelianceAuras == null || character.ownedRelianceAuras.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "No owned auras to activate.", "OK");
            return;
        }
        
        if (character.activeRelianceAuras == null)
        {
            character.activeRelianceAuras = new List<string>();
        }
        
        int activatedCount = 0;
        int totalRelianceCost = 0;
        
        RelianceAuraDatabase database = RelianceAuraDatabase.Instance;
        
        foreach (string auraName in character.ownedRelianceAuras)
        {
            if (!character.activeRelianceAuras.Contains(auraName))
            {
                RelianceAura aura = database != null ? database.GetAura(auraName) : null;
                if (aura != null)
                {
                    totalRelianceCost += aura.relianceCost;
                    
                    // Check if character has enough reliance (for testing, we'll allow it anyway)
                    if (totalRelianceCost <= character.maxReliance)
                    {
                        character.activeRelianceAuras.Add(auraName);
                        activatedCount++;
                    }
                }
            }
        }
        
        Debug.Log($"[Aura Testing] Activated {activatedCount} auras (Total Reliance Cost: {totalRelianceCost})");
        EditorUtility.DisplayDialog("Activated", $"Activated {activatedCount} auras!\nTotal Reliance Cost: {totalRelianceCost}", "OK");
        Repaint();
    }
    
    private void DeactivateAllAuras(Character character)
    {
        if (character.activeRelianceAuras == null || character.activeRelianceAuras.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "No active auras to deactivate.", "OK");
            return;
        }
        
        int count = character.activeRelianceAuras.Count;
        character.activeRelianceAuras.Clear();
        
        Debug.Log($"[Aura Testing] Deactivated {count} auras");
        EditorUtility.DisplayDialog("Deactivated", $"Deactivated {count} auras!", "OK");
        Repaint();
    }
    
    private void ActivateAura(Character character, RelianceAura aura)
    {
        if (character.activeRelianceAuras == null)
        {
            character.activeRelianceAuras = new List<string>();
        }
        
        if (!character.activeRelianceAuras.Contains(aura.auraName))
        {
            character.activeRelianceAuras.Add(aura.auraName);
            Debug.Log($"[Aura Testing] Activated {aura.auraName}");
            Repaint();
        }
    }
    
    private void DeactivateAura(Character character, RelianceAura aura)
    {
        if (character.activeRelianceAuras != null && character.activeRelianceAuras.Remove(aura.auraName))
        {
            Debug.Log($"[Aura Testing] Deactivated {aura.auraName}");
            Repaint();
        }
    }
    
    private void GiveExperienceToActiveAuras(Character character, int amount)
    {
        if (character.activeRelianceAuras == null || character.activeRelianceAuras.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "No active auras to give experience to.", "OK");
            return;
        }
        
        AuraExperienceManager expManager = AuraExperienceManager.Instance;
        if (expManager == null)
        {
            EditorUtility.DisplayDialog("Error", "AuraExperienceManager not found!", "OK");
            return;
        }
        
        int count = 0;
        foreach (string auraName in character.activeRelianceAuras)
        {
            expManager.AddAuraExperience(auraName, amount);
            count++;
        }
        
        Debug.Log($"[Aura Testing] Gave {amount} XP to {count} active auras");
        EditorUtility.DisplayDialog("Experience", $"Gave {amount} XP to {count} auras!", "OK");
        Repaint();
    }
    
    private void GiveExperienceToAura(string auraName, int amount)
    {
        AuraExperienceManager expManager = AuraExperienceManager.Instance;
        if (expManager == null)
        {
            EditorUtility.DisplayDialog("Error", "AuraExperienceManager not found!", "OK");
            return;
        }
        
        expManager.AddAuraExperience(auraName, amount);
        Debug.Log($"[Aura Testing] Gave {amount} XP to {auraName}");
        Repaint();
    }
}

