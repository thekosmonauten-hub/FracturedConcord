using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor tool to create/update Act 1, 2, and 3 encounter nodes based on ACT1-3 Node Data.md
/// </summary>
public class CreateActEncounters : EditorWindow
{
    [MenuItem("Dexiled/Create Act Encounters")]
    public static void ShowWindow()
    {
        GetWindow<CreateActEncounters>("Create Act Encounters");
    }

    private void OnGUI()
    {
        GUILayout.Label("Act Encounter Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Generate/Update Act 1 Encounters (15 nodes)", GUILayout.Height(30)))
        {
            CreateAct1Encounters();
        }

        if (GUILayout.Button("Generate/Update Act 2 Encounters (15 nodes)", GUILayout.Height(30)))
        {
            CreateAct2Encounters();
        }

        if (GUILayout.Button("Generate/Update Act 3 Encounters (10 nodes + final boss)", GUILayout.Height(30)))
        {
            CreateAct3Encounters();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Generate All Acts", GUILayout.Height(40)))
        {
            CreateAct1Encounters();
            CreateAct2Encounters();
            CreateAct3Encounters();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", "All encounter assets have been generated/updated!", "OK");
        }
    }

    private void CreateAct1Encounters()
    {
        string actFolder = "Assets/Resources/Encounters/Act1";
        EnsureFolderExists(actFolder);

        // Act 1 encounters from the document
        var act1Encounters = new List<EncounterInfo>
        {
            new EncounterInfo(1, "Where Night First Fell", "The First to Fall", 1),
            new EncounterInfo(2, "Camp Concordia", null, 2), // Town node (may not have boss combat)
            new EncounterInfo(3, "Whispering Orchard", "The Orchard-Bound Widow", 3),
            new EncounterInfo(4, "The Hollow Grainfields", "Husk Stalker", 4),
            new EncounterInfo(5, "The Splintered Bridge", "Bridge Warden Remnant", 5),
            new EncounterInfo(6, "Rotfall Creek", "The River-Sworn Rotmass", 6),
            new EncounterInfo(7, "The Sighing Woods", "Weeper-of-Bark", 7),
            new EncounterInfo(8, "Woe-Milestone Pass", "Entropic Traveler", 8),
            new EncounterInfo(9, "Blight-Tilled Meadow", "Fieldborn Aberrant", 9),
            new EncounterInfo(10, "The Thorned Palisade", "Bandit Reclaimer", 10),
            new EncounterInfo(11, "The Half-Lit Road", "The Lantern Wretch", 11),
            new EncounterInfo(12, "Asheslope Ridge", "Charred Homesteader", 12),
            new EncounterInfo(13, "The Folding Vale", "Concordial Echo-Beast", 13),
            new EncounterInfo(14, "Path of Failing Light", "Shadow Shepherd", 14),
            new EncounterInfo(15, "The Shattered Gate", "Gate Warden of Vassara", 15)
        };

        CreateEncountersForAct(act1Encounters, actFolder, 1);
        Debug.Log($"<color=green>Created/Updated {act1Encounters.Count} Act 1 encounters</color>");
    }

    private void CreateAct2Encounters()
    {
        string actFolder = "Assets/Resources/Encounters/Act2";
        EnsureFolderExists(actFolder);

        // Act 2 encounters from the document
        var act2Encounters = new List<EncounterInfo>
        {
            new EncounterInfo(1, "Outer Wards of Vassara", "Pale Sentry Construct", 16),
            new EncounterInfo(2, "The Silent Lecture Hall", "Dean of Dust", 17),
            new EncounterInfo(3, "The Hanging Archive", "Suspended Archivist", 18),
            new EncounterInfo(4, "Wreath-Tower of Concordial Study", "Law-Rooted Homunculus", 19),
            new EncounterInfo(5, "Court of Failed Petitions", "Petitioner's Shade", 20),
            new EncounterInfo(6, "Ruined Sealwright Annex", "Shardwright Drone", 21),
            new EncounterInfo(7, "The Grand Revision Circle", "The Overseer Husk", 22),
            new EncounterInfo(8, "The Broken Crown District", "Crownbearer's Echo", 23),
            new EncounterInfo(9, "Wall of the Unread Rulings", "The Grievance Wraith", 24),
            new EncounterInfo(10, "The Embered Senate", "Senator of Ash", 25),
            new EncounterInfo(11, "The Observatory of Fractures", "Starbreaking Acolyte", 26),
            new EncounterInfo(12, "Artery of Spellkiln Streets", "Kiln-Born Enforcer", 27),
            new EncounterInfo(13, "Crackling Forum of Echoes", "Echo-Tyrant", 28),
            new EncounterInfo(14, "The Splintered Ziggurat", "Ascendant Proctor", 29),
            new EncounterInfo(15, "Threshold of the Sundered Archive", "Guardian of Lost Articles", 30)
        };

        CreateEncountersForAct(act2Encounters, actFolder, 2);
        Debug.Log($"<color=green>Created/Updated {act2Encounters.Count} Act 2 encounters</color>");
    }

    private void CreateAct3Encounters()
    {
        string actFolder = "Assets/Resources/Encounters/Act3";
        EnsureFolderExists(actFolder);

        // Act 3 encounters from the document
        var act3Encounters = new List<EncounterInfo>
        {
            new EncounterInfo(1, "Hall of Forgotten Indictments", "The Indexer", 31),
            new EncounterInfo(2, "Atrium of Frayed Laws", "Law-Maw Aberrant", 32),
            new EncounterInfo(3, "Chamber of Waking Drafts", "Draftling Swarm", 33),
            new EncounterInfo(4, "Vault of Discarded Rulings", "Redacted Judge", 34),
            new EncounterInfo(5, "Gallery of Looping Footsteps", "Time-Locked Guardian", 35),
            new EncounterInfo(6, "Broken Thesis Labyrinth", "Thesis Revenant", 36),
            new EncounterInfo(7, "Chamber of Half-Truths", "The Two-Faced Edict", 37),
            new EncounterInfo(8, "Binding Floor of Lost Sealwrights", "Sealwright Echo", 38),
            new EncounterInfo(9, "Rift of the Unwritten", "Proto-Unwritten Spawn", 39),
            new EncounterInfo(10, "Archive Root", "Breach Herald", 40),
            new EncounterInfo(11, "Breachheart", "Eduard, The Unwritten Path", 41) // Final boss
        };

        CreateEncountersForAct(act3Encounters, actFolder, 3);
        Debug.Log($"<color=green>Created/Updated {act3Encounters.Count} Act 3 encounters</color>");
    }

    private void CreateEncountersForAct(List<EncounterInfo> encounters, string folderPath, int actNumber)
    {
        Dictionary<int, EncounterDataAsset> createdAssets = new Dictionary<int, EncounterDataAsset>();

        // First pass: Create all assets
        foreach (var encounter in encounters)
        {
            // Sanitize file name: remove spaces, hyphens, apostrophes, commas, and other special chars
            string sanitizedName = encounter.encounterName
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("'", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace(":", "");
            string fileName = $"{encounter.encounterID}.{sanitizedName}.asset";
            string assetPath = Path.Combine(folderPath, fileName);

            EncounterDataAsset asset;
            if (File.Exists(assetPath))
            {
                asset = AssetDatabase.LoadAssetAtPath<EncounterDataAsset>(assetPath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<EncounterDataAsset>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }
            }
            else
            {
                asset = ScriptableObject.CreateInstance<EncounterDataAsset>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            // Update asset properties - ensure encounterID and encounterName are always set
            // These are the critical fields that must be reflected correctly
            asset.encounterID = encounter.globalID;
            asset.encounterName = encounter.encounterName;
            asset.sceneName = "CombatScene";
            asset.actNumber = actNumber;
            asset.areaLevel = encounter.globalID;
            asset.totalWaves = 3; // Default 3 waves
            asset.maxEnemiesPerWave = 3;
            asset.randomizeEnemyCount = true;
            // uniqueEnemy will need to be assigned manually after boss EnemyData assets are created
            // prerequisiteEncounters will be set in second pass

            EditorUtility.SetDirty(asset);
            createdAssets[encounter.encounterID] = asset;
            
            // Log to confirm encounterID and encounterName are set correctly
            Debug.Log($"<color=cyan>[CreateActEncounters] Created/Updated: ID={asset.encounterID}, Name=\"{asset.encounterName}\", Act={asset.actNumber}, File={fileName}</color>");
        }

        // Second pass: Set up prerequisites (each encounter unlocks the next)
        for (int i = 0; i < encounters.Count; i++)
        {
            var current = encounters[i];
            var currentAsset = createdAssets[current.encounterID];

            List<EncounterDataAsset> prerequisites = new List<EncounterDataAsset>();

            // First encounter in act
            if (current.encounterID == 1)
            {
                if (actNumber > 1)
                {
                    // Act 2+ requires the final encounter of the previous act
                    int previousActLastGlobalID = (actNumber == 2) ? 15 : 30; // Act 1 ends at 15, Act 2 at 30
                    string previousActFolder = (actNumber == 2) ? "Assets/Resources/Encounters/Act1" : "Assets/Resources/Encounters/Act2";
                    
                    // Load all assets from previous act and find the one with matching global ID
                    var previousActGuids = AssetDatabase.FindAssets("t:EncounterDataAsset", new[] { previousActFolder });
                    foreach (var guid in previousActGuids)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<EncounterDataAsset>(AssetDatabase.GUIDToAssetPath(guid));
                        if (asset != null && asset.encounterID == previousActLastGlobalID)
                        {
                            prerequisites.Add(asset);
                            break;
                        }
                    }
                }
                // Act 1, first encounter has no prerequisites
            }
            else
            {
                // Each encounter requires the previous one in the same act
                var previous = encounters[i - 1];
                if (createdAssets.ContainsKey(previous.encounterID))
                {
                    prerequisites.Add(createdAssets[previous.encounterID]);
                }
            }

            currentAsset.prerequisiteEncounters = prerequisites.ToArray();
            EditorUtility.SetDirty(currentAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>[CreateActEncounters] Completed Act {actNumber}: Created/Updated {encounters.Count} encounters with IDs {encounters[0].globalID}-{encounters[encounters.Count-1].globalID}</color>");
    }

    private void EnsureFolderExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    private class EncounterInfo
    {
        public int encounterID; // ID within the act (1-15 for Act 1, etc.)
        public string encounterName;
        public string bossName;
        public int globalID; // Global ID across all acts (1-41)

        public EncounterInfo(int id, string name, string boss, int global)
        {
            encounterID = id;
            encounterName = name;
            bossName = boss;
            globalID = global;
        }
    }
}

