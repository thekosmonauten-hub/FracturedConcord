# Unity Concepts Quick Reference Guide

## Core Unity Concepts

### 1. **GameObjects and Components**
- **GameObject**: Container for components in Unity
- **Component**: Scripts, Renderers, Colliders, etc. that give GameObjects functionality
- **Transform**: Every GameObject has a Transform component (position, rotation, scale)

### 2. **MonoBehaviour**
- Base class for Unity scripts
- Provides lifecycle methods: `Start()`, `Update()`, `Awake()`, `OnDestroy()`
- Must be attached to a GameObject to work

### 3. **Scene Management**
```csharp
using UnityEngine.SceneManagement;

// Load scene by name
SceneManager.LoadScene("SceneName");

// Load scene by build index
SceneManager.LoadScene(0);

// Check if scene is loaded
SceneManager.GetActiveScene().name == "SceneName";
```

### 4. **Singleton Pattern**
```csharp
public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

### 5. **DontDestroyOnLoad**
- Keeps GameObject alive during scene changes
- Use for persistent managers
- Be careful with memory usage

### 6. **UI System**

#### Button Events
```csharp
public Button myButton;

private void Start()
{
    myButton.onClick.AddListener(OnButtonClick);
}

private void OnButtonClick()
{
    Debug.Log("Button clicked!");
}

private void OnDestroy()
{
    myButton.onClick.RemoveListener(OnButtonClick);
}
```

#### Inspector References
```csharp
[Header("UI References")]
public Button myButton;
public Text myText;
public GameObject myPanel;
```

### 7. **Serialization**
```csharp
[System.Serializable]
public class MyData
{
    public string name;
    public int value;
}

// Shows in Inspector
public MyData data;
```

### 8. **Coroutines**
```csharp
private IEnumerator MyCoroutine()
{
    yield return new WaitForSeconds(1f);
    Debug.Log("1 second passed");
}

// Start coroutine
StartCoroutine(MyCoroutine());
```

### 9. **Prefabs**
- Reusable GameObject templates
- Create: Drag GameObject from Hierarchy to Project window
- Instantiate: `Instantiate(prefab, position, rotation)`

### 10. **Tags and Layers**
- **Tags**: Identify GameObjects by type
- **Layers**: Control rendering and physics interactions

## Common Patterns

### 1. **Manager Pattern**
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("References")]
    public UIManager uiManager;
    public AudioManager audioManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

### 2. **Event System**
```csharp
public class EventManager : MonoBehaviour
{
    public static event System.Action OnGameStart;
    public static event System.Action<int> OnScoreChanged;
    
    public static void TriggerGameStart()
    {
        OnGameStart?.Invoke();
    }
}
```

### 3. **Object Pooling**
```csharp
public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 10;
    
    private List<GameObject> pool = new List<GameObject>();
    
    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }
    
    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        return null;
    }
}
```

## Best Practices

### 1. **Performance**
- Use Object Pooling for frequently created/destroyed objects
- Cache component references in Start()
- Use Coroutines instead of Update() for timed events
- Minimize GameObject.Find() calls

### 2. **Memory Management**
- Always remove event listeners in OnDestroy()
- Use null checks before accessing components
- Dispose of resources properly

### 3. **Code Organization**
- Use namespaces for large projects
- Group related functionality in folders
- Use consistent naming conventions
- Document complex logic

### 4. **Debugging**
```csharp
// Debug logging
Debug.Log("Info message");
Debug.LogWarning("Warning message");
Debug.LogError("Error message");

// Conditional compilation
#if UNITY_EDITOR
    Debug.Log("Editor-only code");
#endif
```

## Common Issues and Solutions

### 1. **Null Reference Exceptions**
```csharp
// Always check for null
if (myComponent != null)
{
    myComponent.DoSomething();
}
```

### 2. **Scene Loading Issues**
- Ensure scenes are added to Build Settings
- Check scene names match exactly
- Use SceneManager.sceneLoaded event for post-load setup

### 3. **UI Not Responding**
- Check if Button component is attached
- Verify event listeners are added in Start()
- Ensure UI is in correct Canvas

### 4. **Scripts Not Working**
- Check if script is attached to GameObject
- Verify script inherits from MonoBehaviour
- Look for compilation errors in Console

## Unity Editor Tips

### 1. **Inspector Organization**
```csharp
[Header("Settings")]
public int setting1;

[Space(10)]
[Header("References")]
public GameObject reference1;
```

### 2. **Custom Inspector**
```csharp
#if UNITY_EDITOR
[CustomEditor(typeof(MyScript))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (GUILayout.Button("Custom Action"))
        {
            // Custom editor functionality
        }
    }
}
#endif
```

### 3. **Context Menu**
```csharp
[ContextMenu("Custom Action")]
private void CustomAction()
{
    Debug.Log("Context menu action");
}
```
