using UnityEngine;
using UnityEngine.UI;

public class EncounterSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(3, 5)]
    public string setupInstructions = 
        "1. Add this script to a GameObject in your MainGameUI scene\n" +
        "2. Call SetupEncounterButtons() to create test buttons\n" +
        "3. Make sure EncounterManager is in the scene\n" +
        "4. Test the button functionality";
    
    [Header("Button Creation")]
    public Transform buttonParent;
    public GameObject buttonPrefab;
    
    [ContextMenu("Setup Encounter Buttons")]
    public void SetupEncounterButtons()
    {
        if (EncounterManager.Instance == null)
        {
            Debug.LogError("EncounterManager not found! Please add it to the scene first.");
            return;
        }
        
        if (buttonParent == null)
        {
            buttonParent = transform;
        }
        
        // Create buttons for each encounter
        foreach (EncounterData encounter in EncounterManager.Instance.act1Encounters)
        {
            CreateEncounterButton(encounter);
        }
        
        Debug.Log("Encounter buttons created successfully!");
    }
    
    private void CreateEncounterButton(EncounterData encounter)
    {
        GameObject buttonGO;
        
        if (buttonPrefab != null)
        {
            buttonGO = Instantiate(buttonPrefab, buttonParent);
        }
        else
        {
            // Create a simple button if no prefab is provided
            buttonGO = new GameObject($"Encounter_{encounter.encounterID}_{encounter.encounterName}");
            buttonGO.transform.SetParent(buttonParent);
            
            // Add required components
            Image image = buttonGO.AddComponent<Image>();
            Button button = buttonGO.AddComponent<Button>();
            
            // Create text child
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            Text text = textGO.AddComponent<Text>();
            text.text = encounter.encounterName;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            
            // Set up RectTransform for text
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        // Add EncounterButton component
        EncounterButton encounterButton = buttonGO.GetComponent<EncounterButton>();
        if (encounterButton == null)
        {
            encounterButton = buttonGO.AddComponent<EncounterButton>();
        }
        
        // Set encounter data
        encounterButton.encounterID = encounter.encounterID;
        encounterButton.encounterName = encounter.encounterName;
        
        Debug.Log($"Created button for encounter: {encounter.encounterName}");
    }
}
