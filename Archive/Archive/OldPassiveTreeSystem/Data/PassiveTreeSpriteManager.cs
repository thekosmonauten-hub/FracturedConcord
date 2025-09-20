using System.Collections.Generic;
using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// Manages sprite assets for the passive tree system
    /// Handles BoardContainer and Cell sprites with theme-based organization
    /// </summary>
    [CreateAssetMenu(fileName = "PassiveTreeSpriteManager", menuName = "Dexiled/Passive Tree/Sprite Manager")]
    public class PassiveTreeSpriteManager : ScriptableObject
    {
        [Header("Board Container Sprites")]
        [SerializeField] private Sprite defaultBoardContainer;
        [SerializeField] private List<ThemeBoardSprites> themeBoardSprites = new List<ThemeBoardSprites>();
        
        [Header("Cell Sprites")]
        [SerializeField] private Sprite defaultCellSprite;
        [SerializeField] private List<ThemeCellSprites> themeCellSprites = new List<ThemeCellSprites>();
        
        [Header("Node Type Sprites")]
        [SerializeField] private Sprite normalNodeSprite;
        [SerializeField] private Sprite keystoneNodeSprite;
        [SerializeField] private Sprite notableNodeSprite;
        [SerializeField] private Sprite smallNodeSprite;
        
        /// <summary>
        /// Get the appropriate board container sprite for a theme
        /// </summary>
        public Sprite GetBoardContainerSprite(BoardTheme theme)
        {
            // Look for theme-specific sprite
            foreach (var themeSprites in themeBoardSprites)
            {
                if (themeSprites.theme == theme && themeSprites.boardContainerSprite != null)
                {
                    return themeSprites.boardContainerSprite;
                }
            }
            
            // Return default if no theme-specific sprite found
            if (defaultBoardContainer == null)
            {
                Debug.LogWarning($"[PassiveTreeSpriteManager] No default board container sprite assigned!");
            }
            return defaultBoardContainer;
        }
        
        /// <summary>
        /// Get the appropriate cell sprite for a theme
        /// </summary>
        public Sprite GetCellSprite(BoardTheme theme)
        {
            // Look for theme-specific sprite
            foreach (var themeSprites in themeCellSprites)
            {
                if (themeSprites.theme == theme && themeSprites.cellSprite != null)
                {
                    return themeSprites.cellSprite;
                }
            }
            
            // Return default if no theme-specific sprite found
            return defaultCellSprite;
        }
        
        /// <summary>
        /// Get the appropriate node sprite based on node type
        /// </summary>
        public Sprite GetNodeSprite(NodeType nodeType)
        {
            Sprite result = null;
            
            switch (nodeType)
            {
                case NodeType.Keystone:
                    result = keystoneNodeSprite != null ? keystoneNodeSprite : defaultCellSprite;
                    break;
                case NodeType.Notable:
                    result = notableNodeSprite != null ? notableNodeSprite : defaultCellSprite;
                    break;
                case NodeType.Small:
                    result = smallNodeSprite != null ? smallNodeSprite : defaultCellSprite;
                    break;
                case NodeType.Main:
                case NodeType.Travel:
                case NodeType.Extension:
                    result = normalNodeSprite != null ? normalNodeSprite : defaultCellSprite;
                    break;
                default:
                    result = normalNodeSprite != null ? normalNodeSprite : defaultCellSprite;
                    break;
            }
            
            if (result == null)
            {
                Debug.LogWarning($"[PassiveTreeSpriteManager] No sprite found for {nodeType} node type!");
            }
            
            return result;
        }
        
        /// <summary>
        /// Add a new theme board sprite configuration
        /// </summary>
        public void AddThemeBoardSprite(BoardTheme theme, Sprite boardContainerSprite)
        {
            // Check if theme already exists
            for (int i = 0; i < themeBoardSprites.Count; i++)
            {
                if (themeBoardSprites[i].theme == theme)
                {
                    themeBoardSprites[i].boardContainerSprite = boardContainerSprite;
                    return;
                }
            }
            
            // Add new theme
            themeBoardSprites.Add(new ThemeBoardSprites
            {
                theme = theme,
                boardContainerSprite = boardContainerSprite
            });
        }
        
        /// <summary>
        /// Add a new theme cell sprite configuration
        /// </summary>
        public void AddThemeCellSprite(BoardTheme theme, Sprite cellSprite)
        {
            // Check if theme already exists
            for (int i = 0; i < themeCellSprites.Count; i++)
            {
                if (themeCellSprites[i].theme == theme)
                {
                    themeCellSprites[i].cellSprite = cellSprite;
                    return;
                }
            }
            
            // Add new theme
            themeCellSprites.Add(new ThemeCellSprites
            {
                theme = theme,
                cellSprite = cellSprite
            });
        }
        
        /// <summary>
        /// Get all available themes that have custom sprites
        /// </summary>
        public List<BoardTheme> GetAvailableThemes()
        {
            var themes = new List<BoardTheme>();
            
            foreach (var themeSprites in themeBoardSprites)
            {
                if (themeSprites.boardContainerSprite != null)
                {
                    themes.Add(themeSprites.theme);
                }
            }
            
            return themes;
        }
        
        /// <summary>
        /// Log the current sprite configuration for debugging
        /// </summary>
        public void LogSpriteConfiguration()
        {
            Debug.Log($"[PassiveTreeSpriteManager] '{name}' Configuration:");
            Debug.Log($"  Default Board Container: {(defaultBoardContainer != null ? defaultBoardContainer.name : "NULL")}");
            Debug.Log($"  Default Cell Sprite: {(defaultCellSprite != null ? defaultCellSprite.name : "NULL")}");
            Debug.Log($"  Normal Node Sprite: {(normalNodeSprite != null ? normalNodeSprite.name : "NULL")}");
            Debug.Log($"  Keystone Node Sprite: {(keystoneNodeSprite != null ? keystoneNodeSprite.name : "NULL")}");
            Debug.Log($"  Notable Node Sprite: {(notableNodeSprite != null ? notableNodeSprite.name : "NULL")}");
            Debug.Log($"  Small Node Sprite: {(smallNodeSprite != null ? smallNodeSprite.name : "NULL")}");
            
            Debug.Log($"  Theme-Specific Board Sprites: {themeBoardSprites.Count}");
            foreach (var themeSprite in themeBoardSprites)
            {
                Debug.Log($"    {themeSprite.theme}: {(themeSprite.boardContainerSprite != null ? themeSprite.boardContainerSprite.name : "NULL")}");
            }
            
            Debug.Log($"  Theme-Specific Cell Sprites: {themeCellSprites.Count}");
            foreach (var themeSprite in themeCellSprites)
            {
                Debug.Log($"    {themeSprite.theme}: {(themeSprite.cellSprite != null ? themeSprite.cellSprite.name : "NULL")}");
            }
        }
        
        [ContextMenu("Test Sprite Assets")]
        public void TestSpriteAssets()
        {
            Debug.Log("=== Sprite Asset Test ===");
            
            // Test Small Node Sprite (which should be Basic_Cell)
            if (smallNodeSprite != null)
            {
                Debug.Log($"Small Node Sprite: {smallNodeSprite.name}");
                Debug.Log($"  - Size: {smallNodeSprite.rect.size}");
                Debug.Log($"  - PixelsPerUnit: {smallNodeSprite.pixelsPerUnit}");
                Debug.Log($"  - Texture: {(smallNodeSprite.texture != null ? smallNodeSprite.texture.name : "NULL")}");
                
                if (smallNodeSprite.texture != null)
                {
                    Debug.Log($"  - Texture Format: {smallNodeSprite.texture.format}");
                    Debug.Log($"  - Texture Size: {smallNodeSprite.texture.width}x{smallNodeSprite.texture.height}");
                    Debug.Log($"  - Texture Filter Mode: {smallNodeSprite.texture.filterMode}");
                    Debug.Log($"  - Texture Wrap Mode: {smallNodeSprite.texture.wrapMode}");
                }
            }
            else
            {
                Debug.LogWarning("Small Node Sprite is NULL!");
            }
            
            // Test Default Cell Sprite
            if (defaultCellSprite != null)
            {
                Debug.Log($"Default Cell Sprite: {defaultCellSprite.name}");
                Debug.Log($"  - Size: {defaultCellSprite.rect.size}");
                Debug.Log($"  - PixelsPerUnit: {defaultCellSprite.pixelsPerUnit}");
                Debug.Log($"  - Texture: {(defaultCellSprite.texture != null ? defaultCellSprite.texture.name : "NULL")}");
            }
            else
            {
                Debug.LogWarning("Default Cell Sprite is NULL!");
            }
        }
    }
    
    /// <summary>
    /// Container for theme-specific board sprites
    /// </summary>
    [System.Serializable]
    public class ThemeBoardSprites
    {
        public BoardTheme theme;
        public Sprite boardContainerSprite;
    }
    
    /// <summary>
    /// Container for theme-specific cell sprites
    /// </summary>
    [System.Serializable]
    public class ThemeCellSprites
    {
        public BoardTheme theme;
        public Sprite cellSprite;
    }
}
