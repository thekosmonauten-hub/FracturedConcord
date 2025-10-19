using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns the selected character's combat prefab under the PlayerPortrait GameObject.
/// Configure mappings in the Inspector to map class names to prefabs located in Assets/Prefab/Combat/Characters.
/// </summary>
public class CombatCharacterSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ClassPrefab
    {
        public string className; // e.g., "Marauder"
        public GameObject prefab; // prefab to spawn
    }

    [Header("Target Root (PlayerPortrait)")]
    public Transform playerPortraitRoot; // Assign the PlayerPortrait transform here

    [Header("Class → Prefab Mapping")]
    public List<ClassPrefab> classPrefabs = new List<ClassPrefab>();

    [Header("Options")]
    public bool clearExistingChildren = true;

    private void Start()
    {
        // Auto-find PlayerPortrait if not assigned
        if (playerPortraitRoot == null)
        {
            var t = transform.Find("PlayerPortrait");
            if (t == null)
            {
                var go = GameObject.Find("PlayerPortrait");
                playerPortraitRoot = go != null ? go.transform : null;
            }
        }

        if (playerPortraitRoot == null)
        {
            Debug.LogWarning("CombatCharacterSpawner: PlayerPortrait root not set or found.");
            return;
        }

        // Get current character class
        string className = GetCurrentClassName();
        if (string.IsNullOrWhiteSpace(className))
        {
            Debug.LogWarning("CombatCharacterSpawner: No character class available to spawn.");
            return;
        }

        // Resolve prefab and spawn
        var prefab = ResolvePrefabForClass(className);
        if (prefab == null)
        {
            Debug.LogWarning($"CombatCharacterSpawner: No prefab mapped for class '{className}'.");
            return;
        }

        Spawn(prefab);
    }

    private string GetCurrentClassName()
    {
        var cm = CharacterManager.Instance;
        if (cm != null && cm.HasCharacter())
        {
            return cm.GetCurrentCharacter().characterClass;
        }
        return string.Empty;
    }

    private GameObject ResolvePrefabForClass(string className)
    {
        if (classPrefabs != null)
        {
            for (int i = 0; i < classPrefabs.Count; i++)
            {
                var entry = classPrefabs[i];
                if (entry != null && entry.prefab != null && string.Equals(entry.className, className, System.StringComparison.OrdinalIgnoreCase))
                {
                    return entry.prefab;
                }
            }
        }
        return null;
    }

    private void Spawn(GameObject prefab)
    {
        if (clearExistingChildren)
        {
            for (int i = playerPortraitRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(playerPortraitRoot.GetChild(i).gameObject);
            }
        }

        var instance = Instantiate(prefab, playerPortraitRoot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }
}



