using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldMapNode : MonoBehaviour
{
    [Header("Encounter Settings")]
    public int encounterID = 2; // Wretched Shore is encounter 2
    public string encounterName = "The Wretched Shore";

    public void LoadCombatScene()
    {
        // Use the new encounter system
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.StartEncounter(encounterID);
        }
        else
        {
            // Fallback to direct scene loading
            SceneManager.LoadScene("CombatScene");
        }
    }
}
