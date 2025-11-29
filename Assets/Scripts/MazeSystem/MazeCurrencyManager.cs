using UnityEngine;
using System.Collections.Generic;
using Dexiled.Data.Items;

namespace Dexiled.MazeSystem
{
    /// <summary>
    /// Manages maze-specific currency for the Maze Hub.
    /// Integrates with the main currency system but tracks maze-specific balances.
    /// </summary>
    public class MazeCurrencyManager : MonoBehaviour
    {
        private static MazeCurrencyManager _instance;
        public static MazeCurrencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MazeCurrencyManager>();
                }
                return _instance;
            }
        }
        
        [Header("Currency Storage")]
        [Tooltip("Maze-specific currency balances")]
        private Dictionary<CurrencyType, int> mazeCurrencies = new Dictionary<CurrencyType, int>();
        
        private CurrencyDatabase currencyDatabase;
        private LootManager lootManager;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeCurrency();
        }
        
        private void InitializeCurrency()
        {
            // Load currency database
            currencyDatabase = Resources.Load<CurrencyDatabase>("CurrencyDatabase");
            
            // Get reference to LootManager for currency operations
            lootManager = LootManager.Instance;
            
            // Load saved maze currencies from PlayerPrefs or CharacterManager
            LoadMazeCurrencies();
        }
        
        /// <summary>
        /// Gets the current amount of a specific currency.
        /// </summary>
        public int GetCurrencyAmount(CurrencyType currencyType)
        {
            if (mazeCurrencies.ContainsKey(currencyType))
            {
                return mazeCurrencies[currencyType];
            }
            
            // Fallback to main currency system if available
            if (lootManager != null)
            {
                // Try to get from LootManager's currency system
                // Note: This depends on LootManager having public access to currencies
                // If not, we'll need to add that functionality
                return 0; // Placeholder - implement based on LootManager's currency access
            }
            
            return 0;
        }
        
        /// <summary>
        /// Adds currency to the maze currency balance.
        /// </summary>
        public void AddCurrency(CurrencyType currencyType, int amount)
        {
            if (amount <= 0) return;
            
            if (mazeCurrencies.ContainsKey(currencyType))
            {
                mazeCurrencies[currencyType] += amount;
            }
            else
            {
                mazeCurrencies[currencyType] = amount;
            }
            
            SaveMazeCurrencies();
            
            Debug.Log($"[MazeCurrencyManager] Added {amount} {currencyType}. Total: {mazeCurrencies[currencyType]}");
        }
        
        /// <summary>
        /// Spends currency from the maze currency balance.
        /// </summary>
        public bool SpendCurrency(CurrencyType currencyType, int amount)
        {
            if (amount <= 0) return true;
            
            int currentAmount = GetCurrencyAmount(currencyType);
            if (currentAmount < amount)
            {
                Debug.LogWarning($"[MazeCurrencyManager] Insufficient currency: Have {currentAmount}, need {amount}");
                return false;
            }
            
            if (mazeCurrencies.ContainsKey(currencyType))
            {
                mazeCurrencies[currencyType] -= amount;
            }
            else
            {
                mazeCurrencies[currencyType] = -amount; // Shouldn't happen, but handle it
            }
            
            SaveMazeCurrencies();
            
            Debug.Log($"[MazeCurrencyManager] Spent {amount} {currencyType}. Remaining: {mazeCurrencies[currencyType]}");
            return true;
        }
        
        /// <summary>
        /// Checks if the player has enough of a specific currency.
        /// </summary>
        public bool HasCurrency(CurrencyType currencyType, int amount)
        {
            return GetCurrencyAmount(currencyType) >= amount;
        }
        
        /// <summary>
        /// Loads maze currencies from saved data.
        /// </summary>
        private void LoadMazeCurrencies()
        {
            // Load from PlayerPrefs (simple implementation)
            // In a full implementation, this would load from CharacterManager or save file
            
            // Initialize all currency types to 0 if not found
            foreach (CurrencyType currencyType in System.Enum.GetValues(typeof(CurrencyType)))
            {
                string key = $"MazeCurrency_{currencyType}";
                int amount = PlayerPrefs.GetInt(key, 0);
                if (amount > 0)
                {
                    mazeCurrencies[currencyType] = amount;
                }
            }
            
            Debug.Log($"[MazeCurrencyManager] Loaded {mazeCurrencies.Count} currency type(s)");
        }
        
        /// <summary>
        /// Saves maze currencies to persistent storage.
        /// </summary>
        private void SaveMazeCurrencies()
        {
            foreach (var kvp in mazeCurrencies)
            {
                string key = $"MazeCurrency_{kvp.Key}";
                PlayerPrefs.SetInt(key, kvp.Value);
            }
            
            PlayerPrefs.Save();
        }
    }
}


