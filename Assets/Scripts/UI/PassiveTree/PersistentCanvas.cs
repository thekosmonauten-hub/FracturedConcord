using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Makes a Canvas persistent across scene loads
    /// </summary>
    public class PersistentCanvas : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log($"[PersistentCanvas] Awake called for {gameObject.name}");
            // Make this canvas persistent across scene loads (only if it's a root object)
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log($"[PersistentCanvas] Made {gameObject.name} persistent");
            }
            else
            {
                Debug.LogWarning($"[PersistentCanvas] Cannot use DontDestroyOnLoad on {gameObject.name} - it's not a root GameObject. Parent: {transform.parent.name}");
            }
        }
        
        void OnDestroy()
        {
            Debug.LogError($"[PersistentCanvas] {gameObject.name} is being destroyed! This should not happen.");
            Debug.LogError($"[PersistentCanvas] Stack trace: {System.Environment.StackTrace}");
        }
        
        void OnDisable()
        {
            Debug.LogWarning($"[PersistentCanvas] {gameObject.name} is being disabled");
        }
    }
}
