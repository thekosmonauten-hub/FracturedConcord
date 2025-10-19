using UnityEngine;
using TMPro;

/// <summary>
/// Populates the TMP texts on the CharacterStatsButton with current character data.
/// Expects three TMP objects as children named exactly:
/// - CharacterName
/// - CharacterClass
/// - CharacterLevel
/// </summary>
public class CharacterStatsButtonBinder : MonoBehaviour
{
	[Header("Optional references (auto-found if null)")]
	public TextMeshProUGUI characterNameText;
	public TextMeshProUGUI characterClassText;
	public TextMeshProUGUI characterLevelText;

	private CharacterManager characterManager;

	private void Awake()
	{
		characterManager = CharacterManager.Instance;
		FindTextsIfMissing();
	}

	private void OnEnable()
	{
		Subscribe();
		Refresh();
	}

	private void OnDisable()
	{
		Unsubscribe();
	}

	private void FindTextsIfMissing()
	{
		if (characterNameText == null)
		{
			var t = transform.Find("CharacterName");
			if (t != null) characterNameText = t.GetComponent<TextMeshProUGUI>();
		}
		if (characterClassText == null)
		{
			var t = transform.Find("CharacterClass");
			if (t != null) characterClassText = t.GetComponent<TextMeshProUGUI>();
		}
		if (characterLevelText == null)
		{
			var t = transform.Find("CharacterLevel");
			if (t != null) characterLevelText = t.GetComponent<TextMeshProUGUI>();
		}
	}

	private void Subscribe()
	{
		if (characterManager == null) return;
		characterManager.OnCharacterLoaded += OnCharacterChanged;
		characterManager.OnCharacterLevelUp += OnCharacterChanged;
		characterManager.OnExperienceGained += OnCharacterChanged;
	}

	private void Unsubscribe()
	{
		if (characterManager == null) return;
		characterManager.OnCharacterLoaded -= OnCharacterChanged;
		characterManager.OnCharacterLevelUp -= OnCharacterChanged;
		characterManager.OnExperienceGained -= OnCharacterChanged;
	}

	private void OnCharacterChanged(Character _)
	{
		Refresh();
	}

	public void Refresh()
	{
		var c = characterManager != null ? characterManager.GetCurrentCharacter() : null;
		if (c == null)
		{
			SetTexts("—", "—", "—");
			return;
		}
		SetTexts(c.characterName, c.characterClass, $"Lv {c.level}");
	}

	private void SetTexts(string name, string cls, string lvl)
	{
		if (characterNameText != null) characterNameText.text = name;
		if (characterClassText != null) characterClassText.text = cls;
		if (characterLevelText != null) characterLevelText.text = lvl;
	}
}


