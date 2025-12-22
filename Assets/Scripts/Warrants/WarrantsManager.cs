using UnityEngine;

/// <summary>
/// Singleton manager for warrant board state persistence across scenes.
/// Stores the warrant board state JSON so it can be saved to CharacterData
/// even when the Warrant Scene is unloaded.
/// </summary>
public class WarrantsManager : MonoBehaviour
{
    private static WarrantsManager _instance;
    public static WarrantsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<WarrantsManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("WarrantsManager");
                    _instance = go.AddComponent<WarrantsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Warrant Board State")]
    [Tooltip("The serialized warrant board state JSON. Updated by WarrantBoardStateController when state changes.")]
    [SerializeField] private string warrantBoardStateJson = "";
    
    /// <summary>
    /// Gets the current warrant board state JSON.
    /// Returns empty string if no state has been saved.
    /// </summary>
    public string GetWarrantBoardStateJson()
    {
        return warrantBoardStateJson ?? "";
    }
    
    /// <summary>
    /// Sets the warrant board state JSON.
    /// Called by WarrantBoardStateController when state changes.
    /// </summary>
    public void SetWarrantBoardStateJson(string json)
    {
        warrantBoardStateJson = json ?? "";
        Debug.Log($"[WarrantsManager] Updated warrant board state JSON ({warrantBoardStateJson.Length} chars)");
    }
    
    /// <summary>
    /// Clears the warrant board state.
    /// Called when switching characters or resetting state.
    /// </summary>
    public void ClearWarrantBoardState()
    {
        warrantBoardStateJson = "";
        Debug.Log("[WarrantsManager] Cleared warrant board state");
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}

