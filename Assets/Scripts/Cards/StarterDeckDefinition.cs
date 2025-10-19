using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StarterDeckDefinition", menuName = "Dexiled/Cards/Starter Deck Definition", order = 0)]
public class StarterDeckDefinition : ScriptableObject
{
	[Header("Class Binding (must match Character.characterClass exactly)")]
	public string characterClass = "Marauder";

	[System.Serializable]
	public class CardEntry
	{
		public CardDataExtended card;
		[Min(1)] public int count = 1;
	}

	[Header("Cards")]
	public List<CardEntry> cards = new List<CardEntry>();

	public List<CardDataExtended> BuildCardList()
	{
		var result = new List<CardDataExtended>();
		if (cards == null) return result;

		foreach (var entry in cards)
		{
			if (entry?.card == null || entry.count <= 0) continue;
			for (int i = 0; i < entry.count; i++)
			{
				result.Add(entry.card);
			}
		}
		return result;
	}

	public List<string> GetCardNames()
	{
		var names = new List<string>();
		if (cards == null) return names;

		foreach (var entry in cards)
		{
			if (entry?.card == null || entry.count <= 0) continue;
			for (int i = 0; i < entry.count; i++)
			{
				names.Add(entry.card.cardName);
			}
		}
		return names;
	}
}


