using UnityEngine;

[System.Serializable]
public class EncounterData
{
    public int encounterID;
    public string encounterName;
    public string sceneName;
    public bool isUnlocked = true;
    public bool isCompleted = false;
    
    public EncounterData(int id, string name, string scene)
    {
        encounterID = id;
        encounterName = name;
        sceneName = scene;
    }
}
