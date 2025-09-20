using UnityEngine;
using UnityEditor;

namespace PassiveTree
{
    /// <summary>
    /// Custom editor for BoardData ScriptableObject
    /// Provides automated JSON data assignment and validation
    /// </summary>
    [CustomEditor(typeof(BoardData))]
    public class BoardDataEditor : Editor
    {
        private BoardData boardData;
        
        void OnEnable()
        {
            boardData = (BoardData)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("JSON Data Management", EditorStyles.boldLabel);
            
            // Show current JSON data status
            if (boardData.JsonDataAsset != null)
            {
                EditorGUILayout.HelpBox($"JSON Data: {boardData.JsonDataAsset.name}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("No JSON data assigned", MessageType.Warning);
            }
            
            EditorGUILayout.BeginHorizontal();
            
            // Auto-assign JSON data button
            if (GUILayout.Button("Auto-Assign JSON Data"))
            {
                boardData.AutoAssignJsonData();
                EditorUtility.SetDirty(boardData);
            }
            
            // Extract board name from JSON button
            if (GUILayout.Button("Extract Name from JSON"))
            {
                boardData.ExtractBoardNameFromJson();
                EditorUtility.SetDirty(boardData);
            }
            
            // Validate board data button
            if (GUILayout.Button("Validate Board Data"))
            {
                bool isValid = boardData.IsValid();
                if (isValid)
                {
                    EditorUtility.DisplayDialog("Validation", "Board data is valid!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Validation", "Board data has issues. Check console for details.", "OK");
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Auto-assign all boards button
            EditorGUILayout.Space();
            if (GUILayout.Button("Auto-Assign JSON Data for All Boards"))
            {
                AutoAssignAllBoards();
            }
            
            // Extract names from all boards button
            if (GUILayout.Button("Extract Names from All Boards"))
            {
                ExtractNamesFromAllBoards();
            }
            
            // Create sample JSON files button
            EditorGUILayout.Space();
            if (GUILayout.Button("Create Sample JSON Files"))
            {
                CreateSampleJsonFiles();
            }
        }
        
        /// <summary>
        /// Auto-assign JSON data for all BoardData assets in the project
        /// </summary>
        private void AutoAssignAllBoards()
        {
            string[] guids = AssetDatabase.FindAssets("t:BoardData");
            int assignedCount = 0;
            int totalCount = guids.Length;
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BoardData board = AssetDatabase.LoadAssetAtPath<BoardData>(assetPath);
                
                if (board != null)
                {
                    board.AutoAssignJsonData();
                    EditorUtility.SetDirty(board);
                    assignedCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Auto-Assign Complete", 
                $"Processed {totalCount} boards, assigned JSON data to {assignedCount} boards.", "OK");
        }
        
        /// <summary>
        /// Extract board names from JSON data for all BoardData assets
        /// </summary>
        private void ExtractNamesFromAllBoards()
        {
            string[] guids = AssetDatabase.FindAssets("t:BoardData");
            int extractedCount = 0;
            int totalCount = guids.Length;
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BoardData board = AssetDatabase.LoadAssetAtPath<BoardData>(assetPath);
                
                if (board != null && board.JsonDataAsset != null)
                {
                    board.ExtractBoardNameFromJson();
                    EditorUtility.SetDirty(board);
                    extractedCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Name Extraction Complete", 
                $"Processed {totalCount} boards, extracted names from {extractedCount} boards.", "OK");
        }
        
        /// <summary>
        /// Create sample JSON files for common board types
        /// </summary>
        private void CreateSampleJsonFiles()
        {
            string[] boardTypes = { "Fire", "Ice", "Lightning", "Chaos", "Physical", "Life" };
            
            foreach (string boardType in boardTypes)
            {
                CreateSampleJsonFile(boardType);
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Sample Files Created", 
                $"Created sample JSON files for {boardTypes.Length} board types in Assets/Resources/PassiveTree/", "OK");
        }
        
        /// <summary>
        /// Create a sample JSON file for a specific board type
        /// </summary>
        private void CreateSampleJsonFile(string boardType)
        {
            string fileName = $"{boardType}BoardData.json";
            string folderPath = "Assets/Resources/PassiveTree/";
            string filePath = folderPath + fileName;
            
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/PassiveTree"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "PassiveTree");
            }
            
            // Create sample JSON content
            string jsonContent = CreateSampleJsonContent(boardType);
            
            // Write file
            System.IO.File.WriteAllText(filePath, jsonContent);
            AssetDatabase.ImportAsset(filePath);
            
            Debug.Log($"[BoardDataEditor] Created sample JSON file: {filePath}");
        }
        
        /// <summary>
        /// Create sample JSON content for a board type
        /// </summary>
        private string CreateSampleJsonContent(string boardType)
        {
            return $@"{{
    ""id"": ""{boardType.ToLower()}_board"",
    ""name"": ""{boardType} Board"",
    ""description"": ""A board focused on {boardType.ToLower()} themed passives"",
    ""theme"": ""{boardType}"",
    ""size"": {{
        ""rows"": 7,
        ""columns"": 7
    }},
    ""nodes"": [
        [
            {{
                ""id"": ""Cell_0_0"",
                ""name"": ""{boardType} Mastery"",
                ""description"": ""+10 {boardType} Damage"",
                ""position"": {{ ""row"": 0, ""column"": 0 }},
                ""type"": ""small"",
                ""stats"": {{ ""{boardType.ToLower()}_damage"": 10 }},
                ""maxRank"": 1,
                ""currentRank"": 0,
                ""cost"": 1,
                ""prerequisites"": []
            }},
            null,
            null,
            null,
            null,
            null,
            null
        ],
        [
            null,
            null,
            null,
            null,
            null,
            null,
            null
        ],
        [
            null,
            null,
            null,
            null,
            null,
            null,
            null
        ],
        [
            null,
            null,
            null,
            {{
                ""id"": ""Cell_3_3"",
                ""name"": ""{boardType} Keystone"",
                ""description"": ""Powerful {boardType.ToLower()} keystone passive"",
                ""position"": {{ ""row"": 3, ""column"": 3 }},
                ""type"": ""keystone"",
                ""stats"": {{ ""{boardType.ToLower()}_power"": 50 }},
                ""maxRank"": 1,
                ""currentRank"": 0,
                ""cost"": 1,
                ""prerequisites"": []
            }},
            null,
            null,
            null
        ],
        [
            null,
            null,
            null,
            null,
            null,
            null,
            null
        ],
        [
            null,
            null,
            null,
            null,
            null,
            null,
            null
        ],
        [
            null,
            null,
            null,
            null,
            null,
            null,
            null
        ]
    ]
}}";
        }
    }
}
