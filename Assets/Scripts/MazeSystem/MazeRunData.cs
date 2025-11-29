using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Represents the entire maze run state (persists across floors).
    /// </summary>
    [System.Serializable]
    public class MazeRunData
    {
        public string runId;
        public int seed;
        public int currentFloor = 1;
        public int totalFloors = 8;
        public List<MazeFloor> floors = new List<MazeFloor>();
        public bool isActive = false;
        public bool isCompleted = false;
        public bool isFailed = false;
        
        // Run modifiers (shrines, curses, etc.)
        public List<MazeRunModifier> runModifiers = new List<MazeRunModifier>();
        
        // Boss progression tracking
        public Dictionary<int, bool> bossEncounters = new Dictionary<int, bool>(); // Floor -> encountered
        
        public MazeRunData(int runSeed, int totalFloorCount)
        {
            runId = Guid.NewGuid().ToString();
            seed = runSeed;
            totalFloors = totalFloorCount;
            currentFloor = 1;
            floors = new List<MazeFloor>();
            runModifiers = new List<MazeRunModifier>();
            bossEncounters = new Dictionary<int, bool>();
        }
        
        public MazeFloor GetCurrentFloor()
        {
            if (currentFloor > 0 && currentFloor <= floors.Count)
                return floors[currentFloor - 1];
            return null;
        }
        
        public void AddRunModifier(MazeRunModifier modifier)
        {
            runModifiers.Add(modifier);
        }
        
        public bool HasModifier(string modifierId)
        {
            return runModifiers.Exists(m => m.modifierId == modifierId);
        }
        
        public void MarkBossEncountered(int floor)
        {
            bossEncounters[floor] = true;
        }
        
        public bool WasBossEncountered(int floor)
        {
            return bossEncounters.TryGetValue(floor, out bool encountered) && encountered;
        }
    }
    
    /// <summary>
    /// A run-wide modifier (shrine buff, curse, etc.)
    /// </summary>
    [System.Serializable]
    public class MazeRunModifier
    {
        public string modifierId;
        public string displayName;
        public string description;
        public float value; // Numerical value if applicable
        public ModifierType type;
        public Sprite icon; // Optional icon for UI
        
        public MazeRunModifier(string id, string name, string desc, ModifierType modType, float val = 0f)
        {
            modifierId = id;
            displayName = name;
            description = desc;
            type = modType;
            value = val;
        }
    }
    
    /// <summary>
    /// Types of run modifiers.
    /// </summary>
    public enum ModifierType
    {
        Shrine,     // Positive buff (e.g., Cohesion, Echoes)
        Curse,      // Negative effect (e.g., Entropy, Time-bending)
        Special     // Unique effects
    }
}

