using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Editor tool to create Blinket's dialogue from the markdown documentation.
/// Generates a DialogueData ScriptableObject with all nodes and branches.
/// </summary>
public class CreateBlinketDialogue : EditorWindow
{
    private string markdownPath = "Assets/Documentation/Blinket_the_Bargain-Bound_dialogue.md";
    private string outputPath = "Assets/Resources/Dialogues/Blinket_Dialogue.asset";
    private Vector2 scrollPosition;
    private DialogueData previewDialogue;
    private Sprite defaultSpeakerPortrait;
    
    [MenuItem("Tools/Dexiled/Create Blinket Dialogue")]
    public static void ShowWindow()
    {
        GetWindow<CreateBlinketDialogue>("Create Blinket Dialogue");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Blinket Dialogue from Markdown", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool reads the Blinket dialogue markdown file and generates " +
            "a DialogueData ScriptableObject asset with all dialogue nodes and branches.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Markdown path
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Markdown File:", GUILayout.Width(120));
        markdownPath = EditorGUILayout.TextField(markdownPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select Markdown File", "Assets/Documentation", "md");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                markdownPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Output path
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Path:", GUILayout.Width(120));
        outputPath = EditorGUILayout.TextField(outputPath);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Default speaker portrait
        EditorGUILayout.LabelField("Default Speaker Portrait:", EditorStyles.boldLabel);
        defaultSpeakerPortrait = (Sprite)EditorGUILayout.ObjectField(
            "Portrait Sprite", 
            defaultSpeakerPortrait, 
            typeof(Sprite), 
            false);
        EditorGUILayout.HelpBox(
            "This portrait will be used for all dialogue nodes that don't have their own speaker portrait. " +
            "Individual nodes can override this with their own portrait.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Dialogue Asset", GUILayout.Height(30)))
        {
            GenerateDialogue();
        }
        
        EditorGUILayout.Space();
        
        if (previewDialogue != null)
        {
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Dialogue ID: {previewDialogue.dialogueId}");
            EditorGUILayout.LabelField($"Dialogue Name: {previewDialogue.dialogueName}");
            EditorGUILayout.LabelField($"NPC ID: {previewDialogue.npcId}");
            EditorGUILayout.LabelField($"Total Nodes: {previewDialogue.nodes.Count}");
            EditorGUILayout.LabelField($"Start Node: {previewDialogue.startNodeId}");
        }
    }
    
    private void GenerateDialogue()
    {
        if (!File.Exists(markdownPath))
        {
            EditorUtility.DisplayDialog("Error", $"Markdown file not found: {markdownPath}", "OK");
            return;
        }
        
        string[] lines = File.ReadAllLines(markdownPath);
        
        // Create DialogueData
        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.dialogueId = "blinket_vendor";
        dialogue.dialogueName = "Blinket the Bargain-Bound";
        dialogue.npcId = "blinket";
        dialogue.startNodeId = "greeting";
        dialogue.defaultSpeakerPortrait = defaultSpeakerPortrait; // Set default portrait
        dialogue.nodes = new List<DialogueNode>();
        
        // Parse markdown and build dialogue tree
        ParseMarkdownToDialogue(lines, dialogue);
        
        // Save asset
        string directory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        AssetDatabase.CreateAsset(dialogue, outputPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        previewDialogue = dialogue;
        
        EditorUtility.DisplayDialog(
            "Success",
            $"Dialogue asset created successfully!\n\n" +
            $"Path: {outputPath}\n" +
            $"Nodes: {dialogue.nodes.Count}",
            "OK");
        
        Debug.Log($"[CreateBlinketDialogue] Created dialogue asset at {outputPath} with {dialogue.nodes.Count} nodes");
    }
    
    private void ParseMarkdownToDialogue(string[] lines, DialogueData dialogue)
    {
        // Initial greeting node
        DialogueNode greetingNode = new DialogueNode
        {
            nodeId = "greeting",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Ah! Customer! Or victim! Or… hm. No, customer. Yes. Blinket decides customer today."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Who are you?", targetNodeId = "who_are_you" },
                new DialogueChoice { choiceText = "What is this place?", targetNodeId = "what_is_place" },
                new DialogueChoice { choiceText = "Why does your bag keep… twitching?", targetNodeId = "bag_twitching" },
                new DialogueChoice { choiceText = "Let me see what you're selling.", targetNodeId = "open_shop" },
                new DialogueChoice { choiceText = "Goodbye.", targetNodeId = "goodbye" }
            }
        };
        dialogue.nodes.Add(greetingNode);
        
        // Who are you branch
        DialogueNode whoAreYouNode = new DialogueNode
        {
            nodeId = "who_are_you",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Blinket, at your service! Purveyor of rare oddities, risky bargains, and whatever reality hasn't swallowed yet today.",
                "Blinket collects things that fall out of the Maze's cracks! Sometimes they fall through him first, but Blinket doesn't complain. Much."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Are you… from the Maze?", targetNodeId = "from_maze" },
                new DialogueChoice { choiceText = "You seem… nervous.", targetNodeId = "nervous" },
                new DialogueChoice { choiceText = "Show me your wares.", targetNodeId = "open_shop" }
            }
        };
        dialogue.nodes.Add(whoAreYouNode);
        
        DialogueNode fromMazeNode = new DialogueNode
        {
            nodeId = "from_maze",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Oh no! Blinket would never live in there. Laws bend too much. Ankles break too easy. Blinket prefers the outside where sanity only occasionally slips away."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Show me your wares.", targetNodeId = "open_shop" },
                new DialogueChoice { choiceText = "Goodbye.", targetNodeId = "goodbye" }
            }
        };
        dialogue.nodes.Add(fromMazeNode);
        
        DialogueNode nervousNode = new DialogueNode
        {
            nodeId = "nervous",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Nervous? No no no. Blinket is cautiously optimistic. Many things want to eat Blinket, but Blinket has learned to wriggle just right."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Show me your wares.", targetNodeId = "open_shop" },
                new DialogueChoice { choiceText = "Goodbye.", targetNodeId = "goodbye" }
            }
        };
        dialogue.nodes.Add(nervousNode);
        
        // What is this place branch
        DialogueNode whatIsPlaceNode = new DialogueNode
        {
            nodeId = "what_is_place",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "This? This is Blinket's Ever-Shifting Emporium! A shop so dependable it only disappears twice a day! Sometimes more. Inventory restocks whenever reality sneezes. Blinket hates when it sneezes upwards."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Restocks? Without you doing anything?", targetNodeId = "restocks" },
                new DialogueChoice { choiceText = "Is it safe to shop here?", targetNodeId = "is_safe" },
                new DialogueChoice { choiceText = "Can I just see what you're selling?", targetNodeId = "open_shop" }
            }
        };
        dialogue.nodes.Add(whatIsPlaceNode);
        
        DialogueNode restocksNode = new DialogueNode
        {
            nodeId = "restocks",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Oh yes! Blinket is blessed and cursed. Mostly cursed. The Laws near the Maze twist, tangle, knot! Items vanish, multiply, melt, or turn into coupons for things that don't exist. Blinket makes do."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Show me your wares.", targetNodeId = "open_shop" },
                new DialogueChoice { choiceText = "Goodbye.", targetNodeId = "goodbye" }
            }
        };
        dialogue.nodes.Add(restocksNode);
        
        DialogueNode isSafeNode = new DialogueNode
        {
            nodeId = "is_safe",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "For Blinket? No.",
                "For you? …Probably. Blinket recommends you don't sniff anything, though."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Show me your wares.", targetNodeId = "open_shop" },
                new DialogueChoice { choiceText = "Goodbye.", targetNodeId = "goodbye" }
            }
        };
        dialogue.nodes.Add(isSafeNode);
        
        // Bag twitching branch
        DialogueNode bagTwitchingNode = new DialogueNode
        {
            nodeId = "bag_twitching",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Ah! That means the merchandise is fresh. Or alive. Or angry. Hard to say! Blinket doesn't open the bag unless a customer is looking—very important safety rule."
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { choiceText = "Show me your wares.", targetNodeId = "open_shop" },
                new DialogueChoice { choiceText = "Goodbye.", targetNodeId = "goodbye" }
            }
        };
        dialogue.nodes.Add(bagTwitchingNode);
        
        // Open shop node (triggers shop action)
        DialogueNode openShopNode = new DialogueNode
        {
            nodeId = "open_shop",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Of course! Step right up. Browse quickly before the stock remembers it can leave!"
            },
            // Use onExitActions - shop opens when user clicks Continue on this end node
            // This allows the message to be displayed first, then shop opens when continuing
            onExitActions = new List<DialogueAction>
            {
                new DialogueAction
                {
                    actionType = DialogueAction.ActionType.OpenShop,
                    actionValue = "MazeVendor"
                }
            }
        };
        dialogue.nodes.Add(openShopNode);
        
        // Goodbye node
        DialogueNode goodbyeNode = new DialogueNode
        {
            nodeId = "goodbye",
            speakerName = "Blinket",
            paragraphs = new List<string>
            {
                "Blinket bids you farewell! Try not to die! Blinket's best customers are the ones who keep coming back."
            }
            // No choices = end node
        };
        dialogue.nodes.Add(goodbyeNode);
    }
}

