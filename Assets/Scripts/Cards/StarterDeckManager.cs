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
}
