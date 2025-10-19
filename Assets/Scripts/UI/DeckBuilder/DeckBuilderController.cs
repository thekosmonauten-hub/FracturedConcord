using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Deck builder controller that lists only the character's obtained cards.
/// Requires a CardFactory prefab setup and a parent Transform to place cards under.
/// </summary>
public class DeckBuilderController : MonoBehaviour
{
	[Header("UI Targets")]
	public Transform collectionGridParent;

	private Character current;
	private CardDatabase db;

	private void Start()
	{
		current = CharacterManager.Instance != null ? CharacterManager.Instance.GetCurrentCharacter() : null;
		db = CardDatabase.Instance;
        RebuildCollection();

        // Subscribe to card unlock events to keep collection in sync
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCardUnlocked += HandleCardUnlocked;
        }
	}

    private void OnDestroy()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.OnCardUnlocked -= HandleCardUnlocked;
        }
    }

    private void HandleCardUnlocked(string cardName)
    {
        // Refresh collection when new cards are obtained
        RebuildCollection();
    }

	public void RebuildCollection()
	{
		if (collectionGridParent == null)
		{
			Debug.LogWarning("DeckBuilderController: collectionGridParent is not assigned.");
			return;
		}

		// Clear old items
		for (int i = collectionGridParent.childCount - 1; i >= 0; i--)
		{
			Destroy(collectionGridParent.GetChild(i).gameObject);
		}

		if (current == null || db == null)
		{
			return;
		}

		// Debug path: show all if hasAllCards
		if (current.deckData != null && current.deckData.hasAllCards)
		{
			foreach (var c in db.allCards)
			{
				CardFactory.CreateCard(c, collectionGridParent);
			}
			return;
		}

		// Show only obtained cards by name
		HashSet<string> ownedNames = new HashSet<string>(current.deckData != null ? current.deckData.unlockedCards : new List<string>());
		List<CardData> owned = db.allCards.Where(c => c != null && ownedNames.Contains(c.cardName)).ToList();

		foreach (var c in owned)
		{
			CardFactory.CreateCard(c, collectionGridParent);
		}
	}
}


