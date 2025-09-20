using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }
    
    [Header("Encounter Data")]
    public List<EncounterData> act1Encounters = new List<EncounterData>();
    
    [Header("Current Encounter")]
    public int currentEncounterID = 0;
    
    private void Awake()
    {
        // Singleton pattern - only one instance should exist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEncounters();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeEncounters()
    {
        // Initialize Act 1 encounters
        act1Encounters.Clear();
        act1Encounters.Add(new EncounterData(1, "Where Night First Fell", "CombatScene"));
        act1Encounters.Add(new EncounterData(2, "The Wretched Shore", "CombatScene"));
        act1Encounters.Add(new EncounterData(3, "Tidal Island", "CombatScene"));
        // Add more encounters as needed...
    }
    
    public void StartEncounter(int encounterID)
    {
        EncounterData encounter = GetEncounterByID(encounterID);
        if (encounter != null && encounter.isUnlocked)
        {
            currentEncounterID = encounterID;
            SceneManager.LoadScene(encounter.sceneName);
        }
        else
        {
            Debug.LogWarning($"Encounter {encounterID} is not available or unlocked!");
        }
    }
    
    public EncounterData GetEncounterByID(int encounterID)
    {
        return act1Encounters.Find(e => e.encounterID == encounterID);
    }
    
    public EncounterData GetCurrentEncounter()
    {
        return GetEncounterByID(currentEncounterID);
    }
    
    public void CompleteCurrentEncounter()
    {
        EncounterData encounter = GetCurrentEncounter();
        if (encounter != null)
        {
            encounter.isCompleted = true;
            // Unlock next encounter if it exists
            EncounterData nextEncounter = GetEncounterByID(currentEncounterID + 1);
            if (nextEncounter != null)
            {
                nextEncounter.isUnlocked = true;
            }
        }
    }
    
    public void ReturnToMainUI()
    {
        SceneManager.LoadScene("MainGameUI");
    }
}
