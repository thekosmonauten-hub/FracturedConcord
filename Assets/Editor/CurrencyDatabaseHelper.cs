using UnityEngine;
using UnityEditor;
using Dexiled.Data.Items;

public class CurrencyDatabaseHelper : EditorWindow
{
    [MenuItem("Tools/Currency Database Helper")]
    public static void ShowWindow()
    {
        GetWindow<CurrencyDatabaseHelper>("Currency Database Helper");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Currency Database Helper", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Create/Recreate Currency Database", GUILayout.Height(30)))
        {
            CreateCurrencyDatabase();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Initialize Default Currencies", GUILayout.Height(30)))
        {
            InitializeDefaultCurrencies();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Check for Missing Sprites", GUILayout.Height(30)))
        {
            CheckForMissingSprites();
        }
    }

    private void CreateCurrencyDatabase()
    {
        // Check if CurrencyDatabase already exists
        CurrencyDatabase existingDB = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        
        if (existingDB != null)
        {
            if (EditorUtility.DisplayDialog("Currency Database Exists", 
                "A CurrencyDatabase already exists. Do you want to delete it and create a new one?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset("Assets/Resources/CurrencyDatabase.asset");
                AssetDatabase.Refresh();
            }
            else
            {
                return;
            }
        }

        // Create new CurrencyDatabase
        CurrencyDatabase newDB = CreateInstance<CurrencyDatabase>();
        
        // Ensure Resources folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Save the asset
        AssetDatabase.CreateAsset(newDB, "Assets/Resources/CurrencyDatabase.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Initialize with default currencies
        newDB.InitializeDefaultCurrencies();
        EditorUtility.SetDirty(newDB);
        AssetDatabase.SaveAssets();

        Debug.Log("Currency Database created successfully at Assets/Resources/CurrencyDatabase.asset");
        
        // Select the new asset
        Selection.activeObject = newDB;
        EditorGUIUtility.PingObject(newDB);
    }

    private void InitializeDefaultCurrencies()
    {
        CurrencyDatabase db = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        
        if (db == null)
        {
            EditorUtility.DisplayDialog("Error", "CurrencyDatabase not found. Please create it first.", "OK");
            return;
        }

        db.InitializeDefaultCurrencies();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        
        Debug.Log("Default currencies initialized successfully!");
    }

    private void CheckForMissingSprites()
    {
        CurrencyDatabase db = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
        
        if (db == null)
        {
            EditorUtility.DisplayDialog("Error", "CurrencyDatabase not found. Please create it first.", "OK");
            return;
        }

        int missingSprites = 0;
        foreach (var currency in db.currencies)
        {
            if (currency.currencySprite == null)
            {
                Debug.LogWarning($"Missing sprite for: {currency.currencyName}");
                missingSprites++;
            }
        }

        if (missingSprites == 0)
        {
            Debug.Log("All currencies have sprites assigned!");
        }
        else
        {
            Debug.LogWarning($"Found {missingSprites} currencies without sprites assigned.");
        }
    }
}
