using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// UGUI back button handler for PassiveTreeScene.
/// Saves character and passive tree (if available) before loading MainGameUI.
/// Finds a Button named "BackButton" if not assigned.
/// </summary>
public class PassiveTreeBackButton : MonoBehaviour
{
	[Header("Optional reference (auto-found by name if null)")]
	public Button backButton;

	[Header("Target Scene")]
	public string worldMapSceneName = "MainGameUI";

	private void Awake()
	{
		if (backButton == null)
		{
			var go = GameObject.Find("BackButton");
			if (go != null) backButton = go.GetComponent<Button>();
		}
	}

	private void OnEnable()
	{
		if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
	}

	private void OnDisable()
	{
		if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
	}

	private void Update()
	{
#if ENABLE_INPUT_SYSTEM
		if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
		{
			OnBackClicked();
		}
#elif ENABLE_LEGACY_INPUT_MANAGER
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			OnBackClicked();
		}
#endif
	}

	private void OnBackClicked()
	{
		TrySaveCharacter();
		TrySavePassiveTree();
		LoadWorldMap();
	}

	private void TrySaveCharacter()
	{
		var cm = CharacterManager.Instance;
		if (cm != null && cm.HasCharacter())
		{
			cm.SaveCharacter();
		}
	}

	private void TrySavePassiveTree()
	{
		// Save via reflection to avoid direct compile dependency on PassiveTreeManager
		try
		{
			var asmList = AppDomain.CurrentDomain.GetAssemblies();
			var ptType = asmList.SelectMany(a => a.GetTypes())
				.FirstOrDefault(t => string.Equals(t.Name, "PassiveTreeManager", StringComparison.Ordinal));
			if (ptType == null) return;

			var instances = FindObjectsOfType(ptType);
			if (instances == null || instances.Length == 0) return;

			var method = ptType.GetMethod("SavePassiveTreeState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (method == null) return;

			// Call on the first instance
			method.Invoke(instances[0], null);
		}
		catch (Exception)
		{
			// Swallow: optional system
		}
	}

	private void LoadWorldMap()
	{
		if (string.IsNullOrWhiteSpace(worldMapSceneName))
		{
			Debug.LogWarning("PassiveTreeBackButton: worldMapSceneName is empty.");
			return;
		}
		SceneManager.LoadScene(worldMapSceneName);
	}
}


