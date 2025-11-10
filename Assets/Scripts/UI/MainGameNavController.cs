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
	public Button PassiveTreeButton;
	public Button DeckManagerButton;

	[Header("Scene Names")]
	public string equipmentSceneName = "EquipmentScreen_New";
	public string passiveTreeSceneName = "PassiveTreeScene";
	public string deckBuilderSceneName = "DeckBuilderScene";

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
		if (PassiveTreeButton == null)
		{
			var go = GameObject.Find("PassiveTreeButton");
			if (go != null) PassiveTreeButton = go.GetComponent<Button>();
		}
		if (DeckManagerButton == null)
		{
			var go = GameObject.Find("DeckManagerButton");
			if (go != null) DeckManagerButton = go.GetComponent<Button>();
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
		if (PassiveTreeButton != null)
		{
			PassiveTreeButton.onClick.RemoveAllListeners();
			PassiveTreeButton.onClick.AddListener(() => LoadSceneSafe(passiveTreeSceneName));
		}
		if (DeckManagerButton != null)
		{
			DeckManagerButton.onClick.RemoveAllListeners();
			DeckManagerButton.onClick.AddListener(() => LoadSceneSafe(deckBuilderSceneName));
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


