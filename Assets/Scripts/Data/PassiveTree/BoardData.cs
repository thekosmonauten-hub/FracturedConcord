using UnityEngine;

namespace PassiveTree
{
    /// <summary>
    /// ScriptableObject that holds data for different board types
    /// Used for board selection UI and board creation
    /// </summary>
    [CreateAssetMenu(fileName = "New Board Data", menuName = "Passive Tree/Board Data")]
    public class BoardData : ScriptableObject
    {
        [Header("Board Information")]
        [SerializeField] private string boardName = "New Board";
        [SerializeField] private string boardDescription = "Description of the board";
        [SerializeField] private BoardTheme boardTheme = BoardTheme.General;
        
        [Header("Board Settings")]
        [SerializeField] private Vector2Int boardSize = new Vector2Int(7, 7);
        [SerializeField] private TextAsset jsonDataAsset;
        [SerializeField] private string jsonDataPath = "";
        
        [Header("Visual")]
        [SerializeField] private Sprite boardPreview;
        [SerializeField] private Color boardColor = Color.white;
        
        [Header("Gameplay")]
        [SerializeField] private bool isUnlocked = true;
        [SerializeField] private int requiredLevel = 1;
        
        // Properties
        public string BoardName => boardName;
        public string BoardDescription => boardDescription;
        public BoardTheme BoardTheme => boardTheme;
        public Vector2Int BoardSize => boardSize;
        public TextAsset JsonDataAsset => jsonDataAsset;
        public string JsonDataPath => jsonDataPath;
        public Sprite BoardPreview => boardPreview;
        public Color BoardColor => boardColor;
        public bool IsUnlocked => isUnlocked;
        public int RequiredLevel => requiredLevel;
        
        /// <summary>
        /// Check if this board can be selected at the given level
        /// </summary>
        public bool CanSelectAtLevel(int currentLevel)
        {
            return isUnlocked && currentLevel >= requiredLevel;
        }
        
        /// <summary>
        /// Validate that the board data is properly configured
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(boardName))
            {
                Debug.LogError($"[BoardData] Board name is empty for {name}");
                return false;
            }
            
            if (jsonDataAsset == null)
            {
                Debug.LogError($"[BoardData] No JSON data asset assigned for board '{boardName}'");
                return false;
            }
            
            if (boardSize.x <= 0 || boardSize.y <= 0)
            {
                Debug.LogError($"[BoardData] Invalid board size {boardSize} for board '{boardName}'");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get the JSON data as text
        /// </summary>
        public string GetJsonDataText()
        {
            if (jsonDataAsset == null)
            {
                Debug.LogError($"[BoardData] No JSON data asset assigned for board '{boardName}'");
                return null;
            }
            
            return jsonDataAsset.text;
        }
        
        /// <summary>
        /// Auto-assign JSON data asset based on the board name
        /// This method can be called from the inspector or editor scripts
        /// </summary>
        [ContextMenu("Auto-Assign JSON Data")]
        public void AutoAssignJsonData()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(boardName))
            {
                Debug.LogWarning($"[BoardData] Cannot auto-assign JSON data: board name is empty");
                return;
            }
            
            // Try to find JSON file based on board name
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{boardName} t:TextAsset");
            
            if (guids.Length > 0)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                jsonDataAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                jsonDataPath = assetPath;
                
                // Auto-extract board name from JSON
                ExtractBoardNameFromJson();
                
                Debug.Log($"[BoardData] Auto-assigned JSON data for '{boardName}': {assetPath}");
            }
            else
            {
                Debug.LogWarning($"[BoardData] No JSON file found for board '{boardName}'. Please assign manually.");
            }
#else
            Debug.LogWarning($"[BoardData] Auto-assign JSON data is only available in the Unity Editor");
#endif
        }
        
        /// <summary>
        /// Extract board name from the assigned JSON data
        /// </summary>
        [ContextMenu("Extract Board Name from JSON")]
        public void ExtractBoardNameFromJson()
        {
            if (jsonDataAsset == null)
            {
                Debug.LogWarning($"[BoardData] No JSON data asset assigned to extract name from");
                return;
            }
            
            try
            {
                // Parse JSON to extract the "name" field
                var jsonData = JsonUtility.FromJson<JsonBoardInfo>(jsonDataAsset.text);
                if (!string.IsNullOrEmpty(jsonData.name))
                {
                    boardName = jsonData.name;
                    Debug.Log($"[BoardData] Extracted board name from JSON: '{boardName}'");
                }
                else
                {
                    Debug.LogWarning($"[BoardData] No 'name' field found in JSON data");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BoardData] Failed to parse JSON data: {e.Message}");
            }
        }
        
        /// <summary>
        /// Simple JSON structure to extract board info
        /// </summary>
        [System.Serializable]
        private class JsonBoardInfo
        {
            public string id;
            public string name;
            public string description;
            public string theme;
        }
    }
}
