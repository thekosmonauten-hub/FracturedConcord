using System.Collections.Generic;
using UnityEngine;

namespace Dexiled.Data.Items
{
    [CreateAssetMenu(fileName = "CurrencyDatabase", menuName = "Dexiled/Currency Database")]
    public class CurrencyDatabase : ScriptableObject
    {
        public List<CurrencyData> currencies = new List<CurrencyData>();

        public CurrencyData GetCurrency(CurrencyType type)
        {
            return currencies.Find(c => c.currencyType == type);
        }
        
        public CurrencyData GetCurrencyBySlotIndex(int slotIndex, string tabName)
        {
            switch (tabName)
            {
                case "Orbs":
                    return currencies.Find(c => c.slotIndex == slotIndex && IsOrbCurrency(c.currencyType));
                case "Spirits":
                    return currencies.Find(c => c.slotIndex == slotIndex && IsSpiritCurrency(c.currencyType));
                case "Seals":
                    return currencies.Find(c => c.slotIndex == slotIndex && IsSealCurrency(c.currencyType));
                case "Fragments":
                    return currencies.Find(c => c.slotIndex == slotIndex && IsFragmentCurrency(c.currencyType));
                default:
                    return null;
            }
        }
        
        private bool IsOrbCurrency(CurrencyType currencyType)
        {
            return currencyType >= CurrencyType.OrbOfGeneration && currencyType <= CurrencyType.OrbOfAmnesia;
        }
        
        private bool IsSpiritCurrency(CurrencyType currencyType)
        {
            return currencyType >= CurrencyType.FireSpirit && currencyType <= CurrencyType.DivineSpirit;
        }
        
        private bool IsSealCurrency(CurrencyType currencyType)
        {
            return currencyType >= CurrencyType.TranspositionSeal && currencyType <= CurrencyType.EtchingSeal;
        }
        
        private bool IsFragmentCurrency(CurrencyType currencyType)
        {
            return currencyType >= CurrencyType.Fragment1 && currencyType <= CurrencyType.Fragment3;
        }
        
        // Helper method to assign placeholder sprites to currencies
        public void AssignPlaceholderSprites()
        {
            foreach (var currency in currencies)
            {
                if (currency.currencySprite == null)
                {
                    // You can load placeholder sprites from Resources folder
                    // For now, we'll leave them null and the UI will show text placeholders
                    Debug.Log($"Currency {currency.currencyName} needs a placeholder sprite assigned");
                }
            }
        }
        
        // Helper method to get currency by name
        public CurrencyData GetCurrencyByName(string currencyName)
        {
            return currencies.Find(c => c.currencyName == currencyName);
        }
        
        // Helper method to assign sprite to currency by name
        public void AssignSpriteToCurrency(string currencyName, Sprite sprite)
        {
            CurrencyData currency = GetCurrencyByName(currencyName);
            if (currency != null)
            {
                currency.currencySprite = sprite;
                Debug.Log($"Assigned sprite to {currencyName}");
            }
            else
            {
                Debug.LogWarning($"Currency {currencyName} not found");
            }
        }

        public void InitializeDefaultCurrencies()
        {
            currencies.Clear();
            
            // This method is here for reference but the CreateCurrencyDatabase tool should be used instead
            Debug.LogWarning("Use Tools > Dexiled > Create Currency Database to create a properly configured database");
        }
    }
}













