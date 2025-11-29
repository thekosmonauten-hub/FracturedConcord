using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Wires MainGameUI navigation bar buttons to panels/scenes.
/// Finds buttons by name if not assigned.
/// </summary>
public class MainGameNavController : MonoBehaviour
{
	[Header("Optional direct references (UGUI Buttons)")]
	public Button CharacterStatsButton;
	public Button EquipmentButton;
	// PassiveTreeButton removed - PassiveTree system is legacy
	public Button DeckManagerButton;
	public Button WarrantsTreeButton;
	public Button CampButton;

	[Header("Scene Names")]
	public string equipmentSceneName = "EquipmentScreen_New";
	// passiveTreeSceneName removed - PassiveTree system is legacy
	public string deckBuilderSceneName = "DeckBuilderScene";
	public string warrantTreeSceneName = "WarrantTree";

	[Header("UI")]
	public UIManager uiManager; // for CharacterStats toggle

	private void Awake()
	{
		// Try to find UIManager if not assigned
		if (uiManager == null)
		{
			uiManager = FindFirstObjectByType<UIManager>();
		}
	}

	private void Start()
	{
		FindButtonsIfMissing();
		WireHandlers();
		// Update CampButton availability when scene loads
		UpdateCampButtonAvailability();
	}

	private void FindButtonsIfMissing()
	{
		if (CharacterStatsButton == null)
		{
			var go = GameObject.Find("CharacterStatsButton");
			if (go != null) CharacterStatsButton = go.GetComponent<Button>();
		}
		if (EquipmentButton == null)
		{
			var go = GameObject.Find("EquipmentButton");
			if (go != null) EquipmentButton = go.GetComponent<Button>();
		}
		// PassiveTreeButton finding removed - PassiveTree system is legacy
		if (DeckManagerButton == null)
		{
			var go = GameObject.Find("DeckManagerButton");
			if (go != null) DeckManagerButton = go.GetComponent<Button>();
		}
		if (WarrantsTreeButton == null)
		{
			var go = GameObject.Find("WarrantsTreeButton");
			if (go != null) WarrantsTreeButton = go.GetComponent<Button>();
		}
		if (CampButton == null)
		{
			var go = GameObject.Find("CampButton");
			if (go != null) CampButton = go.GetComponent<Button>();
		}
	}

	private void WireHandlers()
	{
		if (CharacterStatsButton != null)
		{
			CharacterStatsButton.onClick.RemoveAllListeners();
			CharacterStatsButton.onClick.AddListener(OnCharacterStatsClicked);
		}
		if (EquipmentButton != null)
		{
			EquipmentButton.onClick.RemoveAllListeners();
			EquipmentButton.onClick.AddListener(() => LoadSceneSafe(equipmentSceneName));
		}
		// PassiveTreeButton wiring removed - PassiveTree system is legacy
		if (DeckManagerButton != null)
		{
			DeckManagerButton.onClick.RemoveAllListeners();
			DeckManagerButton.onClick.AddListener(() => LoadSceneSafe(deckBuilderSceneName));
		}
		if (WarrantsTreeButton != null)
		{
			WarrantsTreeButton.onClick.RemoveAllListeners();
			WarrantsTreeButton.onClick.AddListener(() => LoadSceneSafe(warrantTreeSceneName));
		}
		if (CampButton != null)
		{
			CampButton.onClick.RemoveAllListeners();
			CampButton.onClick.AddListener(OnCampButtonClicked);
			UpdateCampButtonAvailability();
		}
	}

	private void OnCharacterStatsClicked()
	{
		if (uiManager != null)
		{
			uiManager.ToggleCharacterStats();
		}
		else
		{
			Debug.LogWarning("MainGameNavController: UIManager not found to toggle CharacterStatsPanel.");
		}
	}

	private void OnCampButtonClicked()
	{
		// Camp button functionality - can be extended later
		Debug.Log("CampButton clicked - Camp functionality to be implemented");
	}

	private void UpdateCampButtonAvailability()
	{
		if (CampButton == null) return;

		// Check if Encounter 2 (Camp Concordia) has been entered
		bool campAvailable = false;
		var character = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
		if (character != null)
		{
			campAvailable = character.HasEnteredEncounter(2); // Encounter 2 is Camp Concordia
		}

		CampButton.interactable = campAvailable;
		CampButton.gameObject.SetActive(campAvailable);
	}

	private void LoadSceneSafe(string sceneName)
	{
		if (string.IsNullOrWhiteSpace(sceneName))
		{
			Debug.LogWarning("MainGameNavController: Scene name is empty.");
			return;
		}
		SceneManager.LoadScene(sceneName);
	}
}


