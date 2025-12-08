using UnityEngine;
using UnityEditor;
using Dexiled.Data.Items;
using System.Collections.Generic;

/// <summary>
/// Editor utility to create and populate NameGenerationData ScriptableObject
/// </summary>
public class CreateNameGenerationData : EditorWindow
{
    [MenuItem("Dexiled/Create Name Generation Data")]
    public static void CreateData()
    {
        // Create the asset
        NameGenerationData data = ScriptableObject.CreateInstance<NameGenerationData>();
        
        // Populate rare prefixes from the documentation
        data.rarePrefixes = new List<string>
        {
            "Abyssal", "Ancient", "Arcane", "Ashen", "Astral", "Baneful", "Blighted", "Bloodsoaked",
            "Boundless", "Broken", "Brutal", "Cerulean", "Chthonic", "Clouded", "Corrupted", "Cosmic",
            "Crimson", "Cursed", "Darkened", "Dawnforge", "Deadlight", "Devious", "Dire", "Dreadful",
            "Ebony", "Eldritch", "Emberforged", "Eternal", "Fallen", "Feral", "Fiery", "Forsaken",
            "Fractured", "Frozen", "Gilded", "Glimmering", "Grim", "Harmonious", "Haunted", "Hollow",
            "Honored", "Howling", "Infernal", "Ironbound", "Lurid", "Malevolent", "Midnight", "Mistwalk",
            "Molten", "Moonlit", "Mystic", "Netherbound", "Nightfall", "Obsidian", "Ominous", "Paradox",
            "Phantom", "Polar", "Primeval", "Prismatic", "Radiant", "Ravenous", "Relentless", "Restless",
            "Rimeborn", "Runic", "Sable", "Sacred", "Scarlet", "Shadowed", "Shattered", "Shimmering",
            "Silvered", "Sinister", "Sorrowful", "Soulbound", "Spellwoven", "Spiraled", "Stormborn",
            "Tempestuous", "Thorned", "Thunderous", "Twilight", "Umbral", "Unholy", "Venerable", "Verdant",
            "Vicious", "Vile", "Voidforged", "Whispering", "Wicked", "Wildheart", "Withered", "Wraithbound",
            "Zealous", "Zenith", "Zephyrborn", "Blazewrought", "Starforged"
        };
        
        // Weapon Melee Suffixes (Swords, Axes, Maces, Daggers, Spears)
        data.weaponMeleeSuffixes = new List<string>
        {
            "Edge", "Cleaver", "Splitter", "Bite", "Carver", "Rend", "Gash", "Fang", "Reaver",
            "Wound", "Chopper", "Razor", "Breaker", "Hewer", "Slash", "Spike", "Tear", "Gouge",
            "Blades", "Crescent"
        };
        
        // Bows & Projectiles
        data.weaponRangedSuffixes = new List<string>
        {
            "Flight", "Striker", "Piercer", "Quill", "Arrow", "Feather", "Wingspan", "Windbite",
            "Bolt", "Skewer", "Barrage", "Seeker", "Gale", "Arc", "Shot", "Longstring", "Branch",
            "Shadeflight", "Hawkeye", "Twine"
        };
        
        // Staves & Wands
        data.weaponCasterSuffixes = new List<string>
        {
            "Channel", "Focus", "Spire", "Conduit", "Rod", "Whisper", "Glyph", "Spirit", "Script",
            "Echo", "Pulse", "Chant", "Weaver", "Augury", "Brand", "Tether", "Spark", "Omen",
            "Ember", "Refrain"
        };
        
        // Helmets
        data.helmetSuffixes = new List<string>
        {
            "Crown", "Visage", "Mask", "Gaze", "Brow", "Hood", "Casque", "Helm", "Veil", "Horn",
            "Mantle", "Circlet", "Shadowcap", "Faceguard", "Headwrap", "Warden", "Skullguard",
            "Warhelm", "Mindshell", "Dreadhelm"
        };
        
        // Body Armour
        data.bodyArmourSuffixes = new List<string>
        {
            "Cloak", "Mail", "Plate", "Harness", "Shell", "Carapace", "Shroud", "Raiment", "Mantle",
            "Garb", "Vestment", "Wrap", "Aegis", "Coat", "Jacket", "Hauberk", "Frame", "Bulwark",
            "Hide", "Coil"
        };
        
        // Gloves
        data.gloveSuffixes = new List<string>
        {
            "Hold", "Grasp", "Clutch", "Reach", "Palm", "Fingers", "Talons", "Knuckles", "Grip",
            "Touch", "Bind", "Embrace", "Fistwrap", "Gauntlet", "Claw", "Rivet", "Mesh",
            "Handguard", "Skin", "Links"
        };
        
        // Boots
        data.bootSuffixes = new List<string>
        {
            "Tread", "Stride", "March", "Tracks", "Pace", "Greaves", "Footfall", "Steps", "Trail",
            "Riverrun", "Path", "Lurch", "Sprint", "Stamp", "Soles", "Treadway", "Hollowstep",
            "Stormstep", "Trample", "Voyager"
        };
        
        // Belts
        data.beltSuffixes = new List<string>
        {
            "Girdle", "Chain", "Cord", "Bind", "Wrap", "Strap", "Loop", "Ringlet", "Lash",
            "Cincher", "Links", "Clasp", "Gripline", "Knot", "Reins", "Span", "Beltline",
            "Binder", "Rib", "Twist"
        };
        
        // Amulets
        data.amuletSuffixes = new List<string>
        {
            "Beads", "Charm", "Pendant", "Sigil", "Bond", "Locket", "Emblem", "Mark", "Fragment",
            "Whisper", "Idol", "Icon", "Rune", "Focus", "Trace", "Totem", "Halo", "Eye", "Glimmer", "Sign"
        };
        
        // Rings
        data.ringSuffixes = new List<string>
        {
            "Twirl", "Band", "Loop", "Circle", "Turn", "Whorl", "Gyre", "Spiral", "Coil", "Circlet",
            "Knot", "Loopstone", "Bindstone", "Halo", "Orbit", "Wreath", "Cycle", "Dial", "Echo", "Oath"
        };
        
        // Shields
        data.shieldSuffixes = new List<string>
        {
            "Bulwark", "Wall", "Aegis", "Ward", "Guard", "Barrier", "Holdfast", "Keep", "Rampart",
            "Bastion", "Front", "Palisade", "Defender", "Platewall", "Oathguard", "Shieldmark",
            "Safeguard", "Protector", "Vow", "Sentinel"
        };
        
        // Save the asset
        string path = "Assets/Resources/NameGenerationData.asset";
        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;
        
        Debug.Log($"<color=green>[NameGeneration] Created NameGenerationData asset at: {path}</color>");
        Debug.Log($"<color=green>[NameGeneration] Loaded {data.rarePrefixes.Count} prefixes</color>");
        Debug.Log($"<color=green>[NameGeneration] Ready to generate Magic and Rare item names!</color>");
    }
}

