using UnityEngine;
using UnityEditor;

namespace PassiveTree
{
    public class FixColliderSizes : EditorWindow
    {
        [MenuItem("Tools/Passive Tree/Fix Collider Sizes")]
        public static void ShowWindow()
        {
            GetWindow<FixColliderSizes>("Fix Collider Sizes");
        }

        [SerializeField] private float colliderSize = 1.0f;
        [SerializeField] private bool useSpriteSize = true;

        void OnGUI()
        {
            GUILayout.Label("Fix Collider Sizes", EditorStyles.boldLabel);
            GUILayout.Space(10);

            useSpriteSize = EditorGUILayout.Toggle("Use Sprite Size", useSpriteSize);
            
            if (!useSpriteSize)
            {
                colliderSize = EditorGUILayout.FloatField("Collider Size", colliderSize);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Fix All Cell Colliders"))
            {
                FixAllColliders();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Debug Collider Sizes"))
            {
                DebugColliderSizes();
            }
        }

        private void FixAllColliders()
        {
            Debug.Log("=== FIXING COLLIDER SIZES ===");

            // Find all CellController components
            CellController[] cellControllers = FindObjectsOfType<CellController>();
            Debug.Log($"Found {cellControllers.Length} CellController components");

            int fixedCount = 0;

            foreach (CellController cell in cellControllers)
            {
                Collider2D collider = cell.GetComponent<Collider2D>();
                if (collider == null)
                {
                    Debug.LogWarning($"Cell {cell.GridPosition} has no Collider2D component!");
                    continue;
                }

                // Get the current bounds
                Bounds currentBounds = collider.bounds;
                Debug.Log($"Cell {cell.GridPosition}: Current bounds = {currentBounds}");

                if (useSpriteSize)
                {
                    // Use sprite size
                    SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                        Debug.Log($"  - Sprite size: {spriteSize}");

                        if (collider is BoxCollider2D boxCollider)
                        {
                            boxCollider.size = spriteSize;
                            Debug.Log($"  - Set BoxCollider2D size to: {spriteSize}");
                        }
                        else if (collider is CircleCollider2D circleCollider)
                        {
                            circleCollider.radius = Mathf.Max(spriteSize.x, spriteSize.y) * 0.5f;
                            Debug.Log($"  - Set CircleCollider2D radius to: {circleCollider.radius}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"  - No sprite found for cell {cell.GridPosition}, using default size");
                        SetDefaultColliderSize(collider);
                    }
                }
                else
                {
                    // Use custom size
                    SetCustomColliderSize(collider, colliderSize);
                }

                // Verify the fix
                Bounds newBounds = collider.bounds;
                Debug.Log($"  - New bounds: {newBounds}");
                Debug.Log($"  - Extents: {newBounds.extents}");

                fixedCount++;
            }

            Debug.Log($"=== FIXED {fixedCount} COLLIDERS ===");
        }

        private void SetDefaultColliderSize(Collider2D collider)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                boxCollider.size = Vector2.one;
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = 0.5f;
            }
        }

        private void SetCustomColliderSize(Collider2D collider, float size)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                boxCollider.size = Vector2.one * size;
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = size * 0.5f;
            }
        }

        private void DebugColliderSizes()
        {
            Debug.Log("=== DEBUGGING COLLIDER SIZES ===");

            CellController[] cellControllers = FindObjectsOfType<CellController>();
            
            foreach (CellController cell in cellControllers)
            {
                Collider2D collider = cell.GetComponent<Collider2D>();
                if (collider != null)
                {
                    Bounds bounds = collider.bounds;
                    Debug.Log($"Cell {cell.GridPosition}: Bounds = {bounds}, Extents = {bounds.extents}");
                    
                    if (collider is BoxCollider2D boxCollider)
                    {
                        Debug.Log($"  - BoxCollider2D size: {boxCollider.size}");
                    }
                    else if (collider is CircleCollider2D circleCollider)
                    {
                        Debug.Log($"  - CircleCollider2D radius: {circleCollider.radius}");
                    }
                }
            }

            Debug.Log("=== END COLLIDER DEBUG ===");
        }
    }
}
