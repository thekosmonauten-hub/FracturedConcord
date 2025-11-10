using UnityEngine;

/// <summary>
/// Singleton that persists selected class data between scenes.
/// Use this to pass class selection from CharacterCreation to CharacterDisplayUI.
/// </summary>
public class ClassSelectionData : MonoBehaviour
{
    private static ClassSelectionData _instance;
    public static ClassSelectionData Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ClassSelectionData");
                _instance = go.AddComponent<ClassSelectionData>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    void Awake()
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

    // Selected class data
    public string SelectedClass { get; private set; }
    public string CharacterName { get; set; }
    
    /// <summary>
    /// Set the selected class (call from CharacterCreation scene)
    /// </summary>
    public void SetSelectedClass(string className)
    {
        SelectedClass = className;
        Debug.Log($"[ClassSelectionData] Selected class: {className}");
    }
    
    /// <summary>
    /// Check if a class has been selected
    /// </summary>
    public bool HasSelectedClass()
    {
        return !string.IsNullOrEmpty(SelectedClass);
    }
    
    /// <summary>
    /// Clear selection (optional - for restarting)
    /// </summary>
    public void Clear()
    {
        SelectedClass = null;
        CharacterName = null;
        Debug.Log("[ClassSelectionData] Cleared selection");
    }
}


