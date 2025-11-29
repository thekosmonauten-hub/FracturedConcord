using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Editor utility to create and configure the Shop Database asset.
/// </summary>
public class CreateShopDatabase : EditorWindow
{
    private ShopDatabase shopDatabase;
    private Vector2 scrollPosition;
    private string outputPath = "Assets/Resources/ShopDatabase.asset";
    
    [MenuItem("Tools/Dexiled/Create Shop Database")]
    public static void ShowWindow()
    {
        GetWindow<CreateShopDatabase>("Shop Database Creator");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Shop Database Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool helps you create and configure the Shop Database.\n\n" +
            "The Shop Database maps shop IDs (used in dialogue actions) to their UI panels.\n" +
            "This allows dialogue to open shops without hardcoding shop-specific logic.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Load or create database
        EditorGUILayout.BeginHorizontal();
        shopDatabase = (ShopDatabase)EditorGUILayout.ObjectField(
            "Shop Database", 
            shopDatabase, 
            typeof(ShopDatabase), 
            false);
        
        if (GUILayout.Button("Load", GUILayout.Width(60)))
        {
            LoadExistingDatabase();
        }
        
        if (GUILayout.Button("Create New", GUILayout.Width(80)))
        {
            CreateNewDatabase();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (shopDatabase != null)
        {
            EditorGUILayout.LabelField("Shop Mappings", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Display existing mappings
            if (shopDatabase.shopMappings != null && shopDatabase.shopMappings.Count > 0)
            {
                for (int i = 0; i < shopDatabase.shopMappings.Count; i++)
                {
                    DrawShopMapping(i);
                    EditorGUILayout.Space();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No shop mappings defined. Click 'Add Default Shops' or add manually.", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Default Shops"))
            {
                AddDefaultShops();
            }
            
            if (GUILayout.Button("Add Empty Mapping"))
            {
                if (shopDatabase.shopMappings == null)
                    shopDatabase.shopMappings = new System.Collections.Generic.List<ShopDatabase.ShopMapping>();
                
                shopDatabase.shopMappings.Add(new ShopDatabase.ShopMapping
                {
                    shopId = "NewShop",
                    shopName = "New Shop",
                    accessMethod = ShopDatabase.PanelAccessMethod.MazeHubController,
                    panelFieldName = "vendorPanel"
                });
                EditorUtility.SetDirty(shopDatabase);
            }
            
            if (GUILayout.Button("Save"))
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Success", "Shop Database saved!", "OK");
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Load or create a Shop Database to begin.", MessageType.Warning);
            
            EditorGUILayout.Space();
            outputPath = EditorGUILayout.TextField("New Database Path:", outputPath);
            
            if (GUILayout.Button("Create New Database at Path"))
            {
                CreateDatabaseAtPath(outputPath);
            }
        }
    }
    
    private void LoadExistingDatabase()
    {
        string path = EditorUtility.OpenFilePanel("Load Shop Database", "Assets", "asset");
        if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
        {
            path = "Assets" + path.Substring(Application.dataPath.Length);
            shopDatabase = AssetDatabase.LoadAssetAtPath<ShopDatabase>(path);
            if (shopDatabase == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected file is not a ShopDatabase asset.", "OK");
            }
        }
    }
    
    private void CreateNewDatabase()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Shop Database",
            "ShopDatabase",
            "asset",
            "Choose where to save the Shop Database");
        
        if (!string.IsNullOrEmpty(path))
        {
            CreateDatabaseAtPath(path);
        }
    }
    
    private void CreateDatabaseAtPath(string path)
    {
        ShopDatabase newDatabase = ScriptableObject.CreateInstance<ShopDatabase>();
        newDatabase.shopMappings = new System.Collections.Generic.List<ShopDatabase.ShopMapping>();
        
        AssetDatabase.CreateAsset(newDatabase, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        shopDatabase = newDatabase;
        
        EditorUtility.DisplayDialog("Success", $"Shop Database created at:\n{path}", "OK");
        
        // Add default shops
        AddDefaultShops();
    }
    
    private void AddDefaultShops()
    {
        if (shopDatabase == null) return;
        
        if (shopDatabase.shopMappings == null)
            shopDatabase.shopMappings = new System.Collections.Generic.List<ShopDatabase.ShopMapping>();
        
        // Check if defaults already exist
        bool hasMazeVendor = shopDatabase.shopMappings.Any(m => 
            m != null && m.shopId != null && 
            (m.shopId.Equals("MazeVendor", System.StringComparison.OrdinalIgnoreCase) ||
             m.shopId.Equals("Blinket", System.StringComparison.OrdinalIgnoreCase)));
        
        if (!hasMazeVendor)
        {
            // Add Maze Vendor (Blinket) mapping
            shopDatabase.shopMappings.Add(new ShopDatabase.ShopMapping
            {
                shopId = "MazeVendor",
                shopName = "Blinket's Maze Vendor",
                accessMethod = ShopDatabase.PanelAccessMethod.MazeHubController,
                panelFieldName = "vendorPanel",
                gameObjectNames = new System.Collections.Generic.List<string> { "VendorPanel", "MazeVendorPanel", "BlinketShopPanel" }
            });
        }
        
        EditorUtility.SetDirty(shopDatabase);
        Debug.Log("[CreateShopDatabase] Added default shop mappings.");
    }
    
    private void DrawShopMapping(int index)
    {
        if (shopDatabase.shopMappings == null || index >= shopDatabase.shopMappings.Count)
            return;
        
        var mapping = shopDatabase.shopMappings[index];
        if (mapping == null) return;
        
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Shop #{index + 1}", EditorStyles.boldLabel, GUILayout.Width(80));
        
        if (GUILayout.Button("Remove", GUILayout.Width(60)))
        {
            shopDatabase.shopMappings.RemoveAt(index);
            EditorUtility.SetDirty(shopDatabase);
            return;
        }
        EditorGUILayout.EndHorizontal();
        
        mapping.shopId = EditorGUILayout.TextField("Shop ID:", mapping.shopId);
        mapping.shopName = EditorGUILayout.TextField("Shop Name:", mapping.shopName);
        
        EditorGUILayout.Space();
        mapping.accessMethod = (ShopDatabase.PanelAccessMethod)EditorGUILayout.EnumPopup("Access Method:", mapping.accessMethod);
        
        EditorGUILayout.Space();
        
        switch (mapping.accessMethod)
        {
            case ShopDatabase.PanelAccessMethod.MazeHubController:
                mapping.panelFieldName = EditorGUILayout.TextField("Panel Field Name:", mapping.panelFieldName);
                EditorGUILayout.HelpBox("Field name in MazeHubController (e.g., 'vendorPanel', 'forgePanel')", MessageType.Info);
                break;
                
            case ShopDatabase.PanelAccessMethod.DirectPanel:
                mapping.panelGameObject = (GameObject)EditorGUILayout.ObjectField(
                    "Panel GameObject:", 
                    mapping.panelGameObject, 
                    typeof(GameObject), 
                    false);
                EditorGUILayout.HelpBox("Direct reference to the panel GameObject", MessageType.Info);
                break;
                
            case ShopDatabase.PanelAccessMethod.ComponentType:
                mapping.componentTypeName = EditorGUILayout.TextField("Component Type Name:", mapping.componentTypeName);
                EditorGUILayout.HelpBox("Full type name (e.g., 'Dexiled.MazeSystem.MazeVendorUI')", MessageType.Info);
                break;
                
            case ShopDatabase.PanelAccessMethod.GameObjectName:
                EditorGUILayout.LabelField("GameObject Names (searches for any of these):");
                if (mapping.gameObjectNames == null)
                    mapping.gameObjectNames = new System.Collections.Generic.List<string>();
                
                for (int i = 0; i < mapping.gameObjectNames.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    mapping.gameObjectNames[i] = EditorGUILayout.TextField(mapping.gameObjectNames[i]);
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        mapping.gameObjectNames.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Add Name"))
                {
                    mapping.gameObjectNames.Add("");
                }
                
                EditorGUILayout.HelpBox("Searches for GameObjects with these names (tries each until one is found)", MessageType.Info);
                break;
        }
        
        EditorGUILayout.EndVertical();
        
        EditorUtility.SetDirty(shopDatabase);
    }
}

