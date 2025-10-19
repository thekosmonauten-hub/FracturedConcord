using UnityEngine;
using System.Collections.Generic;

public class StarterDeckManager : MonoBehaviour
{
    private static StarterDeckManager _instance;
    public static StarterDeckManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<StarterDeckManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("StarterDeckManager");
                    _instance = go.AddComponent<StarterDeckManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Starter Deck Assets")]
    public MarauderStarterDeck marauderStarterDeck;
    public RangerStarterDeck rangerStarterDeck;
    public WitchStarterDeck witchStarterDeck;
    public BrawlerStarterDeck brawlerStarterDeck;
    public ThiefStarterDeck thiefStarterDeck;
    public ApostleStarterDeck apostleStarterDeck;
    
    private Dictionary<string, Deck> starterDecks = new Dictionary<string, Deck>();

	[Header("Starter Deck Definitions (Optional) - CardDataExtended only")]
	public List<StarterDeckDefinition> starterDeckDefinitions = new List<StarterDeckDefinition>();

	[Header("Resources Loading (Optional)")]
	public bool loadDefinitionsFromResources = true;
	public string definitionsResourcesPath = "StarterDecks";
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStarterDecks();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeStarterDecks()
    {
        // Create starter decks for each class
        if (marauderStarterDeck != null)
            starterDecks["Marauder"] = marauderStarterDeck.CreateMarauderStarterDeck();
            
        if (rangerStarterDeck != null)
            starterDecks["Ranger"] = rangerStarterDeck.CreateRangerStarterDeck();
            
        if (witchStarterDeck != null)
            starterDecks["Witch"] = witchStarterDeck.CreateWitchStarterDeck();
            
        if (brawlerStarterDeck != null)
            starterDecks["Brawler"] = brawlerStarterDeck.CreateBrawlerStarterDeck();
            
        if (thiefStarterDeck != null)
            starterDecks["Thief"] = thiefStarterDeck.CreateThiefStarterDeck();
            
        if (apostleStarterDeck != null)
            starterDecks["Apostle"] = apostleStarterDeck.CreateApostleStarterDeck();

		// (Optional) Load StarterDeckDefinition entries for CardDataExtended-only flow
		if (starterDeckDefinitions != null)
		{
			foreach (var def in starterDeckDefinitions)
			{
				if (def == null || string.IsNullOrWhiteSpace(def.characterClass)) continue;
				// No legacy Deck population here; definitions are used to initialize Character.deckData
			}
		}

		if (loadDefinitionsFromResources && !string.IsNullOrEmpty(definitionsResourcesPath))
		{
			var defs = Resources.LoadAll<StarterDeckDefinition>(definitionsResourcesPath);
			foreach (var def in defs)
			{
				if (def == null || string.IsNullOrWhiteSpace(def.characterClass)) continue;
				// As above, kept for deckData initialization time
			}
		}
    }
    
    // Get starter deck for a character class
    public Deck GetStarterDeck(string characterClass)
    {
        if (starterDecks.ContainsKey(characterClass))
        {
            return starterDecks[characterClass].GetCopy();
        }
        
        Debug.LogWarning($"No starter deck found for class: {characterClass}");
        return null;
    }
    
    // Get all available character classes
    public List<string> GetAvailableClasses()
    {
        return new List<string>(starterDecks.Keys);
    }
    
    // Check if a class has a starter deck
    public bool HasStarterDeck(string characterClass)
    {
        return starterDecks.ContainsKey(characterClass);
    }
    
    // Get deck statistics for a class
    public DeckStatistics GetDeckStatistics(string characterClass)
    {
        if (starterDecks.ContainsKey(characterClass))
        {
            return starterDecks[characterClass].GetDeckStatistics();
        }
        
        return null;
    }
    
    // Get all starter deck statistics
    public Dictionary<string, DeckStatistics> GetAllDeckStatistics()
    {
        Dictionary<string, DeckStatistics> stats = new Dictionary<string, DeckStatistics>();
        
        foreach (var kvp in starterDecks)
        {
            stats[kvp.Key] = kvp.Value.GetDeckStatistics();
        }
        
        return stats;
    }

	/// <summary>
	/// Initialize Character.deckData from StarterDeckDefinition for the character's class.
	/// Does not use legacy Card/Deck. Pure CardDataExtended-based.
	/// </summary>
	public void AssignStarterToCharacterDeckData(Character character)
	{
		if (character == null) return;

		StarterDeckDefinition chosen = null;
		if (starterDeckDefinitions != null && starterDeckDefinitions.Count > 0)
		{
			chosen = starterDeckDefinitions.Find(d => d != null &&
				string.Equals(d.characterClass, character.characterClass, System.StringComparison.OrdinalIgnoreCase));
		}

		if (chosen == null && loadDefinitionsFromResources && !string.IsNullOrEmpty(definitionsResourcesPath))
		{
			var defs = Resources.LoadAll<StarterDeckDefinition>(definitionsResourcesPath);
			foreach (var d in defs)
			{
				if (d != null && string.Equals(d.characterClass, character.characterClass, System.StringComparison.OrdinalIgnoreCase))
				{
					chosen = d; break;
				}
			}
		}

		if (chosen == null)
		{
			Debug.LogWarning($"StarterDeckManager: No StarterDeckDefinition found for '{character.characterClass}'.");
			return;
		}

		var names = chosen.GetCardNames();
		character.deckData.InitializeStarterCollection(names);

		var defaultDeckName = $"{character.characterClass} Starter";
		if (!character.deckData.HasDeck(defaultDeckName))
		{
			character.deckData.AddDeck(defaultDeckName);
		}
		character.deckData.SetActiveDeck(defaultDeckName);

		Debug.Log($"StarterDeckManager: Initialized starter collection for {character.characterName} ({character.characterClass}) with {names.Count} cards.");
	}

	/// <summary>
	/// Retrieve the StarterDeckDefinition for a given class, checking assigned list then Resources.
	/// Returns null if none found.
	/// </summary>
	public StarterDeckDefinition GetDefinitionForClass(string className)
	{
		if (string.IsNullOrWhiteSpace(className)) return null;
		StarterDeckDefinition chosen = null;
		if (starterDeckDefinitions != null && starterDeckDefinitions.Count > 0)
		{
			chosen = starterDeckDefinitions.Find(d => d != null &&
				string.Equals(d.characterClass, className, System.StringComparison.OrdinalIgnoreCase));
		}
		if (chosen == null && loadDefinitionsFromResources && !string.IsNullOrEmpty(definitionsResourcesPath))
		{
			var defs = Resources.LoadAll<StarterDeckDefinition>(definitionsResourcesPath);
			foreach (var d in defs)
			{
				if (d != null && string.Equals(d.characterClass, className, System.StringComparison.OrdinalIgnoreCase))
				{
					chosen = d; break;
				}
			}
		}
		return chosen;
	}
}
