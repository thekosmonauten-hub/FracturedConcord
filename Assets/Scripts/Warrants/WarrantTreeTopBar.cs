using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top bar for the Warrant Tree: shows available points, a Confirm button,
/// and page toggles for switching warrant board pages.
/// </summary>
public class WarrantTreeTopBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI availablePointsLabel;
    [SerializeField] private Button confirmButton;

    [Header("Page Toggles")]
    [SerializeField] private ToggleGroup pageToggleGroup;
    [SerializeField] private Toggle pageTogglePrefab;
    [SerializeField] private Transform pageToggleContainer;

    [Header("References")]
    [SerializeField] private WarrantBoardStateController boardState;
    [SerializeField] private WarrantBoardGraphBuilder graphBuilder;

    private readonly List<Toggle> pageToggles = new List<Toggle>();
    private Character characterRef;

    private void Awake()
    {
        if (boardState == null)
        {
            boardState = FindFirstObjectByType<WarrantBoardStateController>();
        }

        if (graphBuilder == null)
        {
            graphBuilder = FindFirstObjectByType<WarrantBoardGraphBuilder>();
        }
    }

    private void Start()
    {
        ResolveCharacterReference();
        BuildPageToggles();
        WireConfirmButton();
        RefreshAvailablePointsLabel();
    }

    private void Update()
    {
        RefreshAvailablePointsLabel();
    }

    private void ResolveCharacterReference()
    {
        if (characterRef != null)
            return;

        var charManager = FindFirstObjectByType<CharacterManager>();
        characterRef = charManager != null ? charManager.GetCurrentCharacter() : null;
    }

    private void RefreshAvailablePointsLabel()
    {
        ResolveCharacterReference();

        // Check if unlimited points mode is enabled
        bool unlimitedMode = boardState != null && boardState.IsUnlimitedSkillPointsMode;

        int points = characterRef != null ? characterRef.skillPoints : 0;
        bool hasPointsToSpend = points > 0 || unlimitedMode;

        if (availablePointsLabel != null)
        {
            if (unlimitedMode)
            {
                availablePointsLabel.text = "Available Points: ∞ (Unlimited Mode)";
            }
            else
            {
                availablePointsLabel.text = $"Available Points: {points}";
            }
            availablePointsLabel.gameObject.SetActive(hasPointsToSpend);
        }

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(hasPointsToSpend);
        }
    }

    private void WireConfirmButton()
    {
        if (confirmButton == null)
            return;

        confirmButton.onClick.RemoveListener(OnConfirmPressed);
        confirmButton.onClick.AddListener(OnConfirmPressed);
    }

    private void OnConfirmPressed()
    {
        // Save character
        var charManager = FindFirstObjectByType<CharacterManager>();
        charManager?.SaveCharacter();

        // Persist board state
        boardState?.SaveToPlayerPrefs();
    }

    private void BuildPageToggles()
    {
        if (pageTogglePrefab == null || pageToggleContainer == null || boardState == null)
            return;

        // Clear old toggles
        foreach (var t in pageToggles)
        {
            if (t != null)
            {
                t.onValueChanged.RemoveAllListeners();
                Destroy(t.gameObject);
            }
        }
        pageToggles.Clear();

        var pages = boardState.Pages;
        if (pages == null || pages.Count == 0)
            return;

        for (int i = 0; i < pages.Count; i++)
        {
            int pageIndex = i;
            var toggleInstance = Instantiate(pageTogglePrefab, pageToggleContainer);
            toggleInstance.gameObject.SetActive(true);

            if (pageToggleGroup != null)
            {
                toggleInstance.group = pageToggleGroup;
            }

            // Label the toggle with page index or display name
            var label = toggleInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string display = !string.IsNullOrEmpty(pages[i].DisplayName)
                    ? pages[i].DisplayName
                    : $"Page {i + 1}";
                label.text = display;
            }

            toggleInstance.isOn = (pageIndex == boardState.ActivePageIndex);
            toggleInstance.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    OnPageToggleSelected(pageIndex);
                }
            });

            pageToggles.Add(toggleInstance);
        }
    }

    private void OnPageToggleSelected(int pageIndex)
    {
        if (boardState == null)
            return;

        if (boardState.SetActivePage(pageIndex))
        {
            // Rebuild visuals so sockets / effects bind against the new active page
            graphBuilder?.BuildGraph();
        }
    }
}


