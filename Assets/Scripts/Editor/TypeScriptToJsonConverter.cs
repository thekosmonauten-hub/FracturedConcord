using UnityEngine;
using UnityEditor;
using System.IO;

namespace PassiveTree
{
    /// <summary>
    /// Simple editor tool to convert TypeScript data to JSON
    /// Much easier than the complex ScriptableObject approach
    /// </summary>
    public class TypeScriptToJsonConverter : EditorWindow
    {
        private string typescriptData = "";
        private string outputPath = "Assets/Resources/PassiveTree/";
        private string fileName = "CoreBoardData.json";

        [MenuItem("Tools/Passive Tree/Convert TypeScript to JSON")]
        public static void ShowWindow()
        {
            GetWindow<TypeScriptToJsonConverter>("TypeScript to JSON Converter");
        }

        private void OnGUI()
        {
            GUILayout.Label("TypeScript to JSON Converter", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("TypeScript Data (paste from CoreBoard.ts):");
            typescriptData = EditorGUILayout.TextArea(typescriptData, GUILayout.Height(200));

            GUILayout.Space(10);

            GUILayout.Label("Output Settings:");
            outputPath = EditorGUILayout.TextField("Output Path:", outputPath);
            fileName = EditorGUILayout.TextField("File Name:", fileName);

            GUILayout.Space(10);

            if (GUILayout.Button("Convert to JSON"))
            {
                ConvertToJson();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Load Sample TypeScript Data"))
            {
                LoadSampleData();
            }
        }

        private void ConvertToJson()
        {
            if (string.IsNullOrEmpty(typescriptData))
            {
                EditorUtility.DisplayDialog("Error", "Please paste TypeScript data first!", "OK");
                return;
            }

            try
            {
                // Simple conversion - extract the JSON-like structure from TypeScript
                string jsonData = ExtractJsonFromTypeScript(typescriptData);
                
                // Ensure output directory exists
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                // Write JSON file
                string fullPath = Path.Combine(outputPath, fileName);
                File.WriteAllText(fullPath, jsonData);
                
                // Refresh Unity assets
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", $"JSON file created at: {fullPath}", "OK");
                Debug.Log($"[TypeScriptToJsonConverter] Created JSON file: {fullPath}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Conversion failed: {e.Message}", "OK");
                Debug.LogError($"[TypeScriptToJsonConverter] Conversion failed: {e.Message}");
            }
        }

        private string ExtractJsonFromTypeScript(string tsData)
        {
            // This is a simplified extraction - in a real implementation,
            // you'd want a more robust parser
            
            // Find the CORE_BOARD object
            int startIndex = tsData.IndexOf("export const CORE_BOARD: PassiveBoard = {");
            if (startIndex == -1)
            {
                throw new System.Exception("Could not find CORE_BOARD definition in TypeScript data");
            }

            // Find the matching closing brace
            int braceCount = 0;
            int endIndex = startIndex;
            bool inString = false;
            char prevChar = '\0';

            for (int i = startIndex; i < tsData.Length; i++)
            {
                char c = tsData[i];
                
                if (c == '"' && prevChar != '\\')
                {
                    inString = !inString;
                }
                else if (!inString)
                {
                    if (c == '{')
                    {
                        braceCount++;
                    }
                    else if (c == '}')
                    {
                        braceCount--;
                        if (braceCount == 0)
                        {
                            endIndex = i;
                            break;
                        }
                    }
                }
                
                prevChar = c;
            }

            // Extract the object content
            string objectContent = tsData.Substring(startIndex, endIndex - startIndex + 1);
            
            // Convert TypeScript syntax to JSON
            string jsonData = ConvertTypeScriptToJson(objectContent);
            
            return jsonData;
        }

        private string ConvertTypeScriptToJson(string tsObject)
        {
            // Simple conversions
            string json = tsObject;
            
            // Remove TypeScript-specific syntax
            json = json.Replace("export const CORE_BOARD: PassiveBoard = ", "");
            json = json.Replace("as unknown as PassiveBoard", "");
            
            // Convert single quotes to double quotes
            json = json.Replace("'", "\"");
            
            // Remove trailing commas (JSON doesn't allow them)
            json = System.Text.RegularExpressions.Regex.Replace(json, @",(\s*[}\]])", "$1");
            
            // Convert TypeScript property names to JSON format
            json = json.Replace("maxHealthIncrease", "maxHealthIncrease");
            json = json.Replace("maxEnergyShieldIncrease", "maxEnergyShieldIncrease");
            json = json.Replace("spellPowerIncrease", "spellPowerIncrease");
            json = json.Replace("armorIncrease", "armorIncrease");
            json = json.Replace("increasedEvasion", "increasedEvasion");
            json = json.Replace("increasedProjectileDamage", "increasedProjectileDamage");
            json = json.Replace("elementalResist", "elementalResist");
            
            return json;
        }

        private void LoadSampleData()
        {
            // Load the actual TypeScript file content
            string tsFilePath = "Assets/TypeScriptTranslations/ExtensionBoardsTS/CoreBoard.ts";
            
            if (File.Exists(tsFilePath))
            {
                typescriptData = File.ReadAllText(tsFilePath);
                EditorUtility.DisplayDialog("Success", "Loaded TypeScript data from CoreBoard.ts", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Could not find CoreBoard.ts file", "OK");
            }
        }
    }
}
