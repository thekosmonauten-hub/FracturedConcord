using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom editor for Effigy ScriptableObjects with visual shape editor
/// </summary>
[CustomEditor(typeof(Effigy))]
[CanEditMultipleObjects]
public class EffigyShapeEditor : Editor
{
    private const int CELL_SIZE = 20;
    private const int MAX_GRID_SIZE = 6;
    
    private bool showShapeEditor = true;
    private bool showPresets = true;
    private bool setPickupPointMode = false;
    
    public override void OnInspectorGUI()
    {
        Effigy effigy = (Effigy)target;
        
        // Draw default inspector fields
        serializedObject.Update();
        
        // Draw description field manually to avoid serialization conflicts
        SerializedProperty descProp = serializedObject.FindProperty("description");
        if (descProp != null)
        {
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            descProp.stringValue = EditorGUILayout.TextArea(descProp.stringValue, GUILayout.Height(60));
        }
        
        // Draw requiredLevel field manually
        SerializedProperty reqLevelProp = serializedObject.FindProperty("requiredLevel");
        if (reqLevelProp != null)
        {
            EditorGUILayout.LabelField("Required Level");
            reqLevelProp.intValue = EditorGUILayout.IntField(reqLevelProp.intValue);
        }
        
        // Draw all other fields except the problematic ones
        DrawPropertiesExcluding(serializedObject, "description", "requiredLevel", "m_Script");
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(10);
        
        // Sprite Generation Button
        EditorGUILayout.LabelField("Sprite Generation", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate Sprite from Shape", GUILayout.Height(30)))
        {
            GenerateEffigySprite(effigy);
        }
        EditorGUILayout.HelpBox("Click to generate a sprite based on the effigy's shape, element color, and rarity.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Shape Editor", EditorStyles.boldLabel);
        
        // Toggle shape editor
        showShapeEditor = EditorGUILayout.Foldout(showShapeEditor, "Visual Shape Editor", true);
        if (showShapeEditor)
        {
            DrawShapeEditor(effigy);
        }
        
        EditorGUILayout.Space(5);
        
        // Preset shapes
        showPresets = EditorGUILayout.Foldout(showPresets, "Preset Shapes", true);
        if (showPresets)
        {
            DrawPresetButtons(effigy);
        }
        
        // Preview
        EditorGUILayout.Space(5);
        DrawShapePreview(effigy);
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(effigy);
        }
    }
    
    /// <summary>
    /// Draw the visual grid editor
    /// </summary>
    private void DrawShapeEditor(Effigy effigy)
    {
        EditorGUILayout.BeginVertical("box");
        
        // Grid size controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Width:", GUILayout.Width(50));
        int newWidth = EditorGUILayout.IntSlider(effigy.shapeWidth, 1, MAX_GRID_SIZE);
        if (newWidth != effigy.shapeWidth)
        {
            effigy.shapeWidth = newWidth;
            UpdateShapeMask(effigy);
        }
        
        EditorGUILayout.LabelField("Height:", GUILayout.Width(50));
        int newHeight = EditorGUILayout.IntSlider(effigy.shapeHeight, 1, MAX_GRID_SIZE);
        if (newHeight != effigy.shapeHeight)
        {
            effigy.shapeHeight = newHeight;
            UpdateShapeMask(effigy);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Ghost pickup point controls
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Ghost Pickup Point:", GUILayout.Width(140));
        Vector2Int pickupPoint = effigy.ghostPickupPoint;
        if (pickupPoint.x < 0 || pickupPoint.y < 0)
        {
            EditorGUILayout.LabelField("Auto (center of occupied cells)", EditorStyles.helpBox);
        }
        else
        {
            EditorGUILayout.LabelField($"Cell ({pickupPoint.x}, {pickupPoint.y})", EditorStyles.helpBox);
        }
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            effigy.ghostPickupPoint = new Vector2Int(-1, -1);
            EditorUtility.SetDirty(effigy);
        }
        EditorGUILayout.EndHorizontal();
        
        // Toggle for "Set Pickup Point" mode
        EditorGUILayout.BeginHorizontal();
        setPickupPointMode = EditorGUILayout.Toggle("Set Pickup Point Mode", setPickupPointMode);
        if (setPickupPointMode)
        {
            EditorGUILayout.HelpBox("Click any cell below to set it as the pickup point", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();
        
        if (!setPickupPointMode)
        {
            EditorGUILayout.HelpBox("Enable 'Set Pickup Point Mode' above, then click a cell to set it as the ghost pickup point.", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        
        // Visual grid
        if (effigy.shapeMask != null && effigy.shapeMask.Length == effigy.shapeWidth * effigy.shapeHeight)
        {
            // Draw grid with clickable cells
            for (int y = effigy.shapeHeight - 1; y >= 0; y--) // Top to bottom
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < effigy.shapeWidth; x++)
                {
                    int index = y * effigy.shapeWidth + x;
                    bool isOccupied = effigy.shapeMask[index];
                    bool isPickupPoint = effigy.ghostPickupPoint.x == x && effigy.ghostPickupPoint.y == y;
                    
                    // Draw button
                    Color originalColor = GUI.color;
                    if (isPickupPoint)
                    {
                        // Highlight pickup point with yellow/orange
                        GUI.color = new Color(1f, 0.8f, 0.2f); // Orange-yellow
                    }
                    else
                    {
                        GUI.color = isOccupied ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                    }
                    
                    Rect buttonRect = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE, GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE));
                    
                    if (GUI.Button(buttonRect, isPickupPoint ? "★" : ""))
                    {
                        if (setPickupPointMode)
                        {
                            // In "Set Pickup Point" mode: set the pickup point
                            effigy.ghostPickupPoint = new Vector2Int(x, y);
                            EditorUtility.SetDirty(effigy);
                            UnityEditor.AssetDatabase.SaveAssets();
                            Debug.Log($"[EffigyShapeEditor] ✓ Set ghost pickup point for '{effigy.name}' to cell ({x}, {y})");
                            GUI.changed = true;
                            Repaint();
                        }
                        else
                        {
                            // Normal mode: toggle cell
                            effigy.shapeMask[index] = !effigy.shapeMask[index];
                            UpdateSizeTier(effigy);
                        }
                    }
                    
                    GUI.color = originalColor;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Shape mask size mismatch! Click 'Fix Shape Mask' to correct.", MessageType.Warning);
            if (GUILayout.Button("Fix Shape Mask"))
            {
                UpdateShapeMask(effigy);
            }
        }
        
        EditorGUILayout.Space(5);
        
        // Utility buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All"))
        {
            ClearShape(effigy);
        }
        if (GUILayout.Button("Fill All"))
        {
            FillShape(effigy);
        }
        if (GUILayout.Button("Rotate 90°"))
        {
            RotateShape(effigy);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// Draw preset shape buttons
    /// </summary>
    private void DrawPresetButtons(Effigy effigy)
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.LabelField("Single & Line", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Single (1)")) { ApplyPreset(effigy, PresetShapes.Single); }
        if (GUILayout.Button("Line H (1x2)")) { ApplyPreset(effigy, PresetShapes.LineHorizontal); }
        if (GUILayout.Button("Line V (2x1)")) { ApplyPreset(effigy, PresetShapes.LineVertical); }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("L Shapes", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("L Top-Right")) { ApplyPreset(effigy, PresetShapes.L_TopRight); }
        if (GUILayout.Button("L Top-Left")) { ApplyPreset(effigy, PresetShapes.L_TopLeft); }
        if (GUILayout.Button("L Bottom-Right")) { ApplyPreset(effigy, PresetShapes.L_BottomRight); }
        if (GUILayout.Button("L Bottom-Left")) { ApplyPreset(effigy, PresetShapes.L_BottomLeft); }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("T Shapes", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("T Up")) { ApplyPreset(effigy, PresetShapes.T_Up); }
        if (GUILayout.Button("T Down")) { ApplyPreset(effigy, PresetShapes.T_Down); }
        if (GUILayout.Button("T Left")) { ApplyPreset(effigy, PresetShapes.T_Left); }
        if (GUILayout.Button("T Right")) { ApplyPreset(effigy, PresetShapes.T_Right); }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Z & S Shapes", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Z Shape")) { ApplyPreset(effigy, PresetShapes.Z_Shape); }
        if (GUILayout.Button("S Shape")) { ApplyPreset(effigy, PresetShapes.S_Shape); }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Large Shapes", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cross (+)")) { ApplyPreset(effigy, PresetShapes.Cross); }
        if (GUILayout.Button("Square (2x2)")) { ApplyPreset(effigy, PresetShapes.Square); }
        if (GUILayout.Button("Big L")) { ApplyPreset(effigy, PresetShapes.BigL); }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// Draw shape preview and info
    /// </summary>
    private void DrawShapePreview(Effigy effigy)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Shape Info", EditorStyles.boldLabel);
        
        int cellCount = effigy.GetCellCount();
        EditorGUILayout.LabelField($"Occupied Cells: {cellCount}");
        EditorGUILayout.LabelField($"Size Tier: {effigy.sizeTier}");
        
        // Text preview
        string preview = GetShapePreviewText(effigy);
        EditorGUILayout.HelpBox(preview, MessageType.None);
        
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// Get ASCII preview of the shape
    /// </summary>
    private string GetShapePreviewText(Effigy effigy)
    {
        if (effigy.shapeMask == null || effigy.shapeMask.Length == 0)
            return "No shape defined";
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        for (int y = effigy.shapeHeight - 1; y >= 0; y--)
        {
            sb.Append("│");
            for (int x = 0; x < effigy.shapeWidth; x++)
            {
                int index = y * effigy.shapeWidth + x;
                sb.Append(effigy.shapeMask[index] ? "█" : "░");
            }
            sb.AppendLine("│");
        }
        
        return sb.ToString();
    }
    
    private void UpdateShapeMask(Effigy effigy)
    {
        int expectedSize = effigy.shapeWidth * effigy.shapeHeight;
        bool[] newMask = new bool[expectedSize];
        
        if (effigy.shapeMask != null)
        {
            for (int i = 0; i < Mathf.Min(effigy.shapeMask.Length, expectedSize); i++)
            {
                newMask[i] = effigy.shapeMask[i];
            }
        }
        
        effigy.shapeMask = newMask;
        UpdateSizeTier(effigy);
    }
    
    private void ClearShape(Effigy effigy)
    {
        if (effigy.shapeMask != null)
        {
            for (int i = 0; i < effigy.shapeMask.Length; i++)
            {
                effigy.shapeMask[i] = false;
            }
        }
        UpdateSizeTier(effigy);
    }
    
    private void FillShape(Effigy effigy)
    {
        if (effigy.shapeMask != null)
        {
            for (int i = 0; i < effigy.shapeMask.Length; i++)
            {
                effigy.shapeMask[i] = true;
            }
        }
        UpdateSizeTier(effigy);
    }
    
    private void UpdateSizeTier(Effigy effigy)
    {
        effigy.UpdateSizeTier();
        EditorUtility.SetDirty(effigy);
    }
    
    private void RotateShape(Effigy effigy)
    {
        if (effigy.shapeMask == null || effigy.shapeMask.Length == 0)
            return;
        
        // Create rotated mask
        int newWidth = effigy.shapeHeight;
        int newHeight = effigy.shapeWidth;
        bool[] rotatedMask = new bool[newWidth * newHeight];
        
        // Rotate 90 degrees clockwise
        for (int y = 0; y < effigy.shapeHeight; y++)
        {
            for (int x = 0; x < effigy.shapeWidth; x++)
            {
                int oldIndex = y * effigy.shapeWidth + x;
                int newX = effigy.shapeHeight - 1 - y;
                int newY = x;
                int newIndex = newY * newWidth + newX;
                
                rotatedMask[newIndex] = effigy.shapeMask[oldIndex];
            }
        }
        
        effigy.shapeWidth = newWidth;
        effigy.shapeHeight = newHeight;
        effigy.shapeMask = rotatedMask;
        UpdateSizeTier(effigy);
    }
    
    private void ApplyPreset(Effigy effigy, bool[,] preset)
    {
        effigy.shapeHeight = preset.GetLength(0);
        effigy.shapeWidth = preset.GetLength(1);
        effigy.shapeMask = new bool[effigy.shapeWidth * effigy.shapeHeight];
        
        for (int y = 0; y < effigy.shapeHeight; y++)
        {
            for (int x = 0; x < effigy.shapeWidth; x++)
            {
                int index = y * effigy.shapeWidth + x;
                effigy.shapeMask[index] = preset[y, x];
            }
        }
        
        UpdateSizeTier(effigy);
    }
    
    /// <summary>
    /// Preset Tetris-like shapes (row-major: [y, x])
    /// </summary>
    private static class PresetShapes
    {
        public static bool[,] Single = new bool[,] {
            { true }
        };
        
        public static bool[,] LineHorizontal = new bool[,] {
            { true, true }
        };
        
        public static bool[,] LineVertical = new bool[,] {
            { true },
            { true }
        };
        
        public static bool[,] L_TopRight = new bool[,] {
            { true, true },
            { false, true }
        };
        
        public static bool[,] L_TopLeft = new bool[,] {
            { true, true },
            { true, false }
        };
        
        public static bool[,] L_BottomRight = new bool[,] {
            { false, true },
            { true, true }
        };
        
        public static bool[,] L_BottomLeft = new bool[,] {
            { true, false },
            { true, true }
        };
        
        public static bool[,] T_Up = new bool[,] {
            { true, true, true },
            { false, true, false }
        };
        
        public static bool[,] T_Down = new bool[,] {
            { false, true, false },
            { true, true, true }
        };
        
        public static bool[,] T_Left = new bool[,] {
            { true, false },
            { true, true },
            { true, false }
        };
        
        public static bool[,] T_Right = new bool[,] {
            { false, true },
            { true, true },
            { false, true }
        };
        
        public static bool[,] Z_Shape = new bool[,] {
            { true, true, false },
            { false, true, true }
        };
        
        public static bool[,] S_Shape = new bool[,] {
            { false, true, true },
            { true, true, false }
        };
        
        public static bool[,] Cross = new bool[,] {
            { false, true, false },
            { true, true, true },
            { false, true, false }
        };
        
        public static bool[,] Square = new bool[,] {
            { true, true },
            { true, true }
        };
        
        public static bool[,] BigL = new bool[,] {
            { true, false },
            { true, false },
            { true, true }
        };
    }
    
    /// <summary>
    /// Generate a sprite from the effigy's shapeMask
    /// </summary>
    private void GenerateEffigySprite(Effigy effigy)
    {
        if (effigy.shapeMask == null || effigy.shapeMask.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Shape mask is empty. Please define a shape first.", "OK");
            return;
        }
        
        // Calculate sprite dimensions (with padding)
        const int CELL_PIXELS = 32; // Size of each grid cell in pixels
        const int PADDING = 8; // Padding around the sprite
        int spriteWidth = effigy.shapeWidth * CELL_PIXELS + (PADDING * 2);
        int spriteHeight = effigy.shapeHeight * CELL_PIXELS + (PADDING * 2);
        
        // Create texture
        Texture2D texture = new Texture2D(spriteWidth, spriteHeight, TextureFormat.RGBA32, false);
        
        // Fill with transparent background
        Color[] transparentPixels = new Color[spriteWidth * spriteHeight];
        for (int i = 0; i < transparentPixels.Length; i++)
        {
            transparentPixels[i] = new Color(0, 0, 0, 0);
        }
        texture.SetPixels(transparentPixels);
        
        // Get element color and rarity brightness
        Color elementColor = effigy.GetElementColor();
        float rarityBrightness = GetRarityBrightness(effigy.rarity);
        Color baseColor = elementColor * rarityBrightness;
        
        // Draw each cell
        for (int y = 0; y < effigy.shapeHeight; y++)
        {
            for (int x = 0; x < effigy.shapeWidth; x++)
            {
                if (effigy.IsCellOccupied(x, y))
                {
                    int pixelX = PADDING + (x * CELL_PIXELS);
                    int pixelY = spriteHeight - (PADDING + ((y + 1) * CELL_PIXELS)); // Flip Y for texture coords
                    
                    // Draw cell with border
                    DrawCell(texture, pixelX, pixelY, CELL_PIXELS, baseColor, effigy.rarity);
                }
            }
        }
        
        texture.Apply();
        
        // Save to file
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Effigy Sprite",
            $"{effigy.effigyName}_Sprite",
            "png",
            "Choose where to save the generated sprite"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            // Encode to PNG
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            
            // Refresh asset database
            AssetDatabase.Refresh();
            
            // Configure texture importer
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            
            // Load and assign sprite
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                effigy.icon = sprite;
                EditorUtility.SetDirty(effigy);
                AssetDatabase.SaveAssets();
                
                EditorUtility.DisplayDialog(
                    "Success!",
                    $"Generated sprite for {effigy.effigyName}!\n\n" +
                    $"Sprite saved to: {path}\n\n" +
                    "The sprite has been assigned to the effigy's icon field.",
                    "OK"
                );
            }
        }
        
        // Cleanup
        DestroyImmediate(texture);
    }
    
    /// <summary>
    /// Draw a single cell with border and shading
    /// </summary>
    private void DrawCell(Texture2D texture, int startX, int startY, int cellSize, Color baseColor, ItemRarity rarity)
    {
        Color borderColor = GetRarityBorderColor(rarity);
        const int BORDER_WIDTH = 2;
        
        for (int y = 0; y < cellSize; y++)
        {
            for (int x = 0; x < cellSize; x++)
            {
                int pixelX = startX + x;
                int pixelY = startY + y;
                
                // Skip out of bounds
                if (pixelX < 0 || pixelX >= texture.width || pixelY < 0 || pixelY >= texture.height)
                    continue;
                
                // Determine if this pixel is border or fill
                bool isBorder = x < BORDER_WIDTH || x >= cellSize - BORDER_WIDTH ||
                               y < BORDER_WIDTH || y >= cellSize - BORDER_WIDTH;
                
                // Apply shading (slight gradient from top-left to bottom-right)
                float shadeFactor = 1f;
                if (!isBorder)
                {
                    float normX = (float)x / cellSize;
                    float normY = (float)y / cellSize;
                    shadeFactor = 0.85f + (normX * 0.1f) + (normY * 0.1f); // Slight brightness variation
                }
                
                Color pixelColor = isBorder ? borderColor : (baseColor * shadeFactor);
                texture.SetPixel(pixelX, pixelY, pixelColor);
            }
        }
    }
    
    /// <summary>
    /// Get rarity border color
    /// </summary>
    private Color GetRarityBorderColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal:
                return new Color(0.7f, 0.7f, 0.7f); // Grey
            case ItemRarity.Magic:
                return new Color(0.3f, 0.6f, 1f); // Blue
            case ItemRarity.Rare:
                return new Color(1f, 0.8f, 0.2f); // Gold
            case ItemRarity.Unique:
                return new Color(1f, 0.5f, 0f); // Orange
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Get rarity brightness modifier
    /// </summary>
    private float GetRarityBrightness(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Normal:
                return 0.6f;
            case ItemRarity.Magic:
                return 0.8f;
            case ItemRarity.Rare:
                return 1.0f;
            case ItemRarity.Unique:
                return 1.2f;
            default:
                return 1f;
        }
    }
}

