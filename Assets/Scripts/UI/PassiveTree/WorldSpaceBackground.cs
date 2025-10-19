using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Creates a world space background for the passive tree.
    /// Use this instead of Canvas-based backgrounds in world space scenes.
    /// </summary>
    public class WorldSpaceBackground : MonoBehaviour
    {
        [Header("Background Settings")]
        [SerializeField] private bool createBackgroundOnStart = true;
        [SerializeField] private BackgroundType backgroundType = BackgroundType.Sprite;
        
        [Header("Sprite Background")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.08f, 1f); // Dark blue-grey
        [SerializeField] private Vector2 backgroundSize = new Vector2(50f, 50f); // World units
        [SerializeField] private float backgroundZ = 10f; // Z distance from camera (positive = behind nodes)
        
        [Header("Quad Background")]
        [SerializeField] private Material quadMaterial;
        [SerializeField] private Texture2D quadTexture;
        
        [Header("Position")]
        [SerializeField] private Vector3 backgroundPosition = Vector3.zero;
        
        [Header("References")]
        private GameObject backgroundObject;
        
        public enum BackgroundType
        {
            Sprite,      // Uses SpriteRenderer (simple, recommended)
            Quad,        // Uses 3D quad mesh (more control)
            Plane        // Uses Unity plane primitive
        }
        
        void Start()
        {
            if (createBackgroundOnStart)
            {
                CreateBackground();
            }
        }
        
        /// <summary>
        /// Create the world space background
        /// </summary>
        [ContextMenu("Create Background")]
        public void CreateBackground()
        {
            // Remove existing background if present
            if (backgroundObject != null)
            {
                DestroyImmediate(backgroundObject);
            }
            
            switch (backgroundType)
            {
                case BackgroundType.Sprite:
                    CreateSpriteBackground();
                    break;
                case BackgroundType.Quad:
                    CreateQuadBackground();
                    break;
                case BackgroundType.Plane:
                    CreatePlaneBackground();
                    break;
            }
            
            Debug.Log($"[WorldSpaceBackground] Created {backgroundType} background at position {backgroundPosition}");
        }
        
        /// <summary>
        /// Create a sprite-based background (simple and efficient)
        /// </summary>
        private void CreateSpriteBackground()
        {
            backgroundObject = new GameObject("PassiveTreeBackground_Sprite");
            backgroundObject.transform.SetParent(transform);
            backgroundObject.transform.localPosition = new Vector3(backgroundPosition.x, backgroundPosition.y, backgroundZ);
            backgroundObject.transform.localRotation = Quaternion.identity;
            
            // Add SpriteRenderer
            SpriteRenderer spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            
            if (backgroundSprite != null)
            {
                spriteRenderer.sprite = backgroundSprite;
                spriteRenderer.color = Color.white; // Use sprite's colors
            }
            else
            {
                // Create a simple white square sprite if none provided
                spriteRenderer.sprite = CreateDefaultSquareSprite();
                spriteRenderer.color = backgroundColor;
            }
            
            // Scale to desired size
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = backgroundSize;
            
            // Set sorting order (lower than nodes)
            spriteRenderer.sortingOrder = -100;
            
            Debug.Log($"[WorldSpaceBackground] Created sprite background with size {backgroundSize}");
        }
        
        /// <summary>
        /// Create a quad-based background (more control, supports custom materials)
        /// </summary>
        private void CreateQuadBackground()
        {
            backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundObject.name = "PassiveTreeBackground_Quad";
            backgroundObject.transform.SetParent(transform);
            backgroundObject.transform.localPosition = new Vector3(backgroundPosition.x, backgroundPosition.y, backgroundZ);
            backgroundObject.transform.localRotation = Quaternion.identity;
            backgroundObject.transform.localScale = new Vector3(backgroundSize.x, backgroundSize.y, 1f);
            
            // Remove collider (not needed for background)
            Collider collider = backgroundObject.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            // Set material
            Renderer renderer = backgroundObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (quadMaterial != null)
                {
                    renderer.material = quadMaterial;
                }
                else if (quadTexture != null)
                {
                    // Create a simple material with the texture
                    Material mat = new Material(Shader.Find("Sprites/Default"));
                    mat.mainTexture = quadTexture;
                    renderer.material = mat;
                }
                else
                {
                    // Use default material with solid color
                    Material mat = new Material(Shader.Find("Sprites/Default"));
                    mat.color = backgroundColor;
                    renderer.material = mat;
                }
            }
            
            Debug.Log($"[WorldSpaceBackground] Created quad background with size {backgroundSize}");
        }
        
        /// <summary>
        /// Create a plane-based background (largest, suitable for very big backgrounds)
        /// </summary>
        private void CreatePlaneBackground()
        {
            backgroundObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            backgroundObject.name = "PassiveTreeBackground_Plane";
            backgroundObject.transform.SetParent(transform);
            backgroundObject.transform.localPosition = new Vector3(backgroundPosition.x, backgroundPosition.y, backgroundZ);
            backgroundObject.transform.localRotation = Quaternion.Euler(90, 0, 0); // Rotate to face camera
            backgroundObject.transform.localScale = new Vector3(backgroundSize.x / 10f, 1f, backgroundSize.y / 10f); // Plane is 10x10 by default
            
            // Remove collider
            Collider collider = backgroundObject.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            // Set material
            Renderer renderer = backgroundObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (quadMaterial != null)
                {
                    renderer.material = quadMaterial;
                }
                else
                {
                    Material mat = new Material(Shader.Find("Sprites/Default"));
                    mat.color = backgroundColor;
                    renderer.material = mat;
                }
            }
            
            Debug.Log($"[WorldSpaceBackground] Created plane background with size {backgroundSize}");
        }
        
        /// <summary>
        /// Create a default white square sprite for fallback
        /// </summary>
        private Sprite CreateDefaultSquareSprite()
        {
            // Create a 1x1 white texture
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            return sprite;
        }
        
        /// <summary>
        /// Update background color at runtime
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            
            if (backgroundObject != null)
            {
                SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = color;
                }
                else
                {
                    Renderer renderer = backgroundObject.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.color = color;
                    }
                }
                
                Debug.Log($"[WorldSpaceBackground] Background color updated to {color}");
            }
        }
        
        /// <summary>
        /// Update background sprite at runtime
        /// </summary>
        public void SetBackgroundSprite(Sprite sprite)
        {
            backgroundSprite = sprite;
            
            if (backgroundObject != null)
            {
                SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = sprite;
                    Debug.Log($"[WorldSpaceBackground] Background sprite updated");
                }
            }
        }
        
        /// <summary>
        /// Update background size at runtime
        /// </summary>
        public void SetBackgroundSize(Vector2 size)
        {
            backgroundSize = size;
            
            if (backgroundObject != null)
            {
                SpriteRenderer spriteRenderer = backgroundObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.size = size;
                }
                else
                {
                    // Update transform scale for quad/plane
                    if (backgroundType == BackgroundType.Quad)
                    {
                        backgroundObject.transform.localScale = new Vector3(size.x, size.y, 1f);
                    }
                    else if (backgroundType == BackgroundType.Plane)
                    {
                        backgroundObject.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);
                    }
                }
                
                Debug.Log($"[WorldSpaceBackground] Background size updated to {size}");
            }
        }
        
        /// <summary>
        /// Remove the background
        /// </summary>
        [ContextMenu("Remove Background")]
        public void RemoveBackground()
        {
            if (backgroundObject != null)
            {
                DestroyImmediate(backgroundObject);
                backgroundObject = null;
                Debug.Log("[WorldSpaceBackground] Background removed");
            }
        }
    }
}



