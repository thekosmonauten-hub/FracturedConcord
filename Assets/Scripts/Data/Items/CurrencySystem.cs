using System.Collections.Generic;
using UnityEngine;

namespace Dexiled.Data.Items
{
    [System.Serializable]
    public enum CurrencyType
    {
        // Orbs
        OrbOfGeneration,
        OrbOfInfusion,
        OrbOfPerfection,
        OrbOfPerpetuity,
        OrbOfRedundancy,
        OrbOfTheVoid,
        OrbOfMutation,
        OrbOfProliferation,
        OrbOfAmnesia,
        
        // Spirits
        FireSpirit,
        ColdSpirit,
        LightningSpirit,
        ChaosSpirit,
        PhysicalSpirit,
        LifeSpirit,
        DefenseSpirit,
        CritSpirit,
        DivineSpirit,
        
        // Seals
        TranspositionSeal,
        ChaosSeal,
        MemorySeal,
        InscriptionSeal,
        AdaptationSeal,
        CorrectionSeal,
        EtchingSeal,
        
        // Fragments (placeholder for future)
        Fragment1,
        Fragment2,
        Fragment3
    }

    [System.Serializable]
    public class CurrencyData
    {
        public CurrencyType currencyType;
        public string currencyName;
        public string description;
        public ItemRarity rarity;
        public Sprite currencySprite;
        public int quantity;
        
        // Target validation
        public bool canTargetCards;
        public bool canTargetEquipment;
        public ItemRarity[] validEquipmentRarities;
        public int maxAffixesForTarget; // For orbs that check affix count
        
        // Effect parameters
        public bool isCorruption; // For Void Orb
        public bool preservesLockedAffixes; // For Amnesia Orb
        
        // Slot position for consistent placement
        public int slotIndex;
    }

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
            
            // Orb of Generation
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfGeneration,
                currencyName = "Orb of Generation",
                description = "Allows you to generate a card",
                rarity = ItemRarity.Normal,
                canTargetCards = true,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 0
            });

            // Orb of Infusion
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfInfusion,
                currencyName = "Orb of Infusion",
                description = "Reforges Normal equipment, making it Magic and adds a random affix",
                rarity = ItemRarity.Normal,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Normal },
                maxAffixesForTarget = 0,
                slotIndex = 1
            });

            // Orb of Perfection
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfPerfection,
                currencyName = "Orb of Perfection",
                description = "Reforges Magic equipment making it Rare and adds a random affix",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic },
                maxAffixesForTarget = 0,
                slotIndex = 2
            });

            // Orb of Perpetuity
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfPerpetuity,
                currencyName = "Orb of Perpetuity",
                description = "Adds an affix to a Rare equipment",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 3
            });

            // Orb of Redundancy
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfRedundancy,
                currencyName = "Orb of Redundancy",
                description = "Removes an affix from selected Magic or Rare equipment",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 4
            });

            // Orb of the Void
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfTheVoid,
                currencyName = "Orb of the Void",
                description = "Corrupts the equipment or card unpredictably",
                rarity = ItemRarity.Magic,
                canTargetCards = true,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Normal, ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Unique },
                maxAffixesForTarget = 0,
                isCorruption = true,
                slotIndex = 5
            });

            // Orb of Mutation
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfMutation,
                currencyName = "Orb of Mutation",
                description = "Rerolls all affixes on a Magic item",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic },
                maxAffixesForTarget = 0,
                slotIndex = 6
            });

            // Orb of Proliferation
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfProliferation,
                currencyName = "Orb of Proliferation",
                description = "Adds an affix to a Magic item",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic },
                maxAffixesForTarget = 2,
                slotIndex = 7
            });

            // Orb of Amnesia
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.OrbOfAmnesia,
                currencyName = "Orb of Amnesia",
                description = "Removes all current affixes and sets the item to Normal (does not remove locked affixes)",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare, ItemRarity.Unique },
                maxAffixesForTarget = 0,
                preservesLockedAffixes = true,
                slotIndex = 8
            });

            // Spirit Orbs
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.FireSpirit,
                currencyName = "Fire Spirit",
                description = "Guarantees Fire-themed affix",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 0
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.ColdSpirit,
                currencyName = "Cold Spirit",
                description = "Guarantees Cold-themed affix",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 1
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.LightningSpirit,
                currencyName = "Lightning Spirit",
                description = "Guarantees Lightning-themed affix",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 2
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.ChaosSpirit,
                currencyName = "Chaos Spirit",
                description = "Guarantees Chaos-themed affix",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 3
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.PhysicalSpirit,
                currencyName = "Physical Spirit",
                description = "Guarantees Physical Damage affix",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 4
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.LifeSpirit,
                currencyName = "Life Spirit",
                description = "Guarantees Life affix",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 5
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.DefenseSpirit,
                currencyName = "Defense Spirit",
                description = "Guarantees Armor, Evasion or Energy Shield affix",
                rarity = ItemRarity.Magic,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 6
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.CritSpirit,
                currencyName = "Crit Spirit",
                description = "Guarantees Critical Strike affix",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 7
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.DivineSpirit,
                currencyName = "Divine Spirit",
                description = "Guarantees a mix of various affixes",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 8
            });

            // Seals
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.TranspositionSeal,
                currencyName = "Transposition Seal",
                description = "Transposes affixes between items",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 0
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.ChaosSeal,
                currencyName = "Chaos Seal",
                description = "Rerolls all affixes on Rare items",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Rare },
                maxAffixesForTarget = 0,
                slotIndex = 1
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.MemorySeal,
                currencyName = "Memory Seal",
                description = "Locks one random affix in place",
                rarity = ItemRarity.Rare,
                canTargetCards = false,
                canTargetEquipment = true,
                validEquipmentRarities = new ItemRarity[] { ItemRarity.Magic, ItemRarity.Rare },
                maxAffixesForTarget = 3,
                slotIndex = 2
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.InscriptionSeal,
                currencyName = "Inscription Seal",
                description = "Adds empty embossing slots to cards",
                rarity = ItemRarity.Magic,
                canTargetCards = true,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 3
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.AdaptationSeal,
                currencyName = "Adaptation Seal",
                description = "Allows the user to add an embossing effect to a card",
                rarity = ItemRarity.Rare,
                canTargetCards = true,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 4
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.CorrectionSeal,
                currencyName = "Correction Seal",
                description = "Removes embossing effects from cards",
                rarity = ItemRarity.Magic,
                canTargetCards = true,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 5
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.EtchingSeal,
                currencyName = "Etching Seal",
                description = "Allows the user to try to etch another affix onto a card",
                rarity = ItemRarity.Rare,
                canTargetCards = true,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 6
            });

            // Fragments (placeholder)
            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.Fragment1,
                currencyName = "Fragment 1",
                description = "Placeholder fragment",
                rarity = ItemRarity.Normal,
                canTargetCards = false,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 0
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.Fragment2,
                currencyName = "Fragment 2",
                description = "Placeholder fragment",
                rarity = ItemRarity.Normal,
                canTargetCards = false,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 1
            });

            currencies.Add(new CurrencyData
            {
                currencyType = CurrencyType.Fragment3,
                currencyName = "Fragment 3",
                description = "Placeholder fragment",
                rarity = ItemRarity.Normal,
                canTargetCards = false,
                canTargetEquipment = false,
                validEquipmentRarities = new ItemRarity[0],
                maxAffixesForTarget = 0,
                slotIndex = 2
            });
        }
    }

    public static class CurrencyEffects
    {
        public static bool CanUseCurrencyOnItem(CurrencyData currency, BaseItem item)
        {
            if (item == null) return false;

            // Check if currency can target equipment
            if (!currency.canTargetEquipment) return false;

            // Check if item rarity is valid for this currency
            bool validRarity = false;
            foreach (var rarity in currency.validEquipmentRarities)
            {
                if (item.rarity == rarity)
                {
                    validRarity = true;
                    break;
                }
            }
            if (!validRarity) return false;

            // Check affix count for specific orbs
            if (currency.maxAffixesForTarget > 0)
            {
                if (item.GetTotalAffixCount() >= currency.maxAffixesForTarget)
                    return false;
            }

            return true;
        }

        public static bool CanUseCurrencyOnCard(CurrencyData currency, object card) // Replace 'object' with actual card type
        {
            if (card == null) return false;
            return currency.canTargetCards;
        }

        public static void ApplyCurrencyEffect(CurrencyData currency, BaseItem item)
        {
            if (!CanUseCurrencyOnItem(currency, item)) return;

            switch (currency.currencyType)
            {
                case CurrencyType.OrbOfInfusion:
                    ApplyInfusionEffect(item);
                    break;
                case CurrencyType.OrbOfPerfection:
                    ApplyPerfectionEffect(item);
                    break;
                case CurrencyType.OrbOfPerpetuity:
                    ApplyPerpetuityEffect(item);
                    break;
                case CurrencyType.OrbOfRedundancy:
                    ApplyRedundancyEffect(item);
                    break;
                case CurrencyType.OrbOfTheVoid:
                    ApplyVoidEffect(item);
                    break;
                case CurrencyType.OrbOfMutation:
                    ApplyMutationEffect(item);
                    break;
                case CurrencyType.OrbOfProliferation:
                    ApplyProliferationEffect(item);
                    break;
                case CurrencyType.OrbOfAmnesia:
                    ApplyAmnesiaEffect(item);
                    break;
            }
        }

        private static void ApplyInfusionEffect(BaseItem item)
        {
            // Upgrade from Normal to Magic and add random affix
            item.rarity = ItemRarity.Magic;
            AddRandomAffix(item);
        }

        private static void ApplyPerfectionEffect(BaseItem item)
        {
            // Upgrade from Magic to Rare and add random affix
            item.rarity = ItemRarity.Rare;
            AddRandomAffix(item);
        }

        private static void ApplyPerpetuityEffect(BaseItem item)
        {
            // Add random affix to Rare item
            AddRandomAffix(item);
        }

        private static void ApplyRedundancyEffect(BaseItem item)
        {
            // Remove random affix
            int totalAffixes = item.GetTotalAffixCount();
            if (totalAffixes > 0)
            {
                // Randomly choose between prefix or suffix to remove
                if (UnityEngine.Random.Range(0, 2) == 0 && item.prefixes.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, item.prefixes.Count);
                    item.prefixes.RemoveAt(randomIndex);
                }
                else if (item.suffixes.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, item.suffixes.Count);
                    item.suffixes.RemoveAt(randomIndex);
                }
            }
        }

        private static void ApplyVoidEffect(BaseItem item)
        {
            // Corrupt the item unpredictably
            // This is a placeholder - implement corruption logic
            Debug.Log($"Corrupting {item.itemName} with Void Orb!");
        }

        private static void ApplyMutationEffect(BaseItem item)
        {
            // Reroll all affixes
            item.prefixes.Clear();
            item.suffixes.Clear();
            AddRandomAffix(item);
            AddRandomAffix(item);
        }

        private static void ApplyProliferationEffect(BaseItem item)
        {
            // Add affix if under limit
            if (item.GetTotalAffixCount() < 2)
            {
                AddRandomAffix(item);
            }
        }

        private static void ApplyAmnesiaEffect(BaseItem item)
        {
            // Remove all affixes and set to Normal, preserving locked ones
            // This is a placeholder - implement locked affix logic
            item.prefixes.Clear();
            item.suffixes.Clear();
            item.rarity = ItemRarity.Normal;
        }

        private static void AddRandomAffix(BaseItem item)
        {
            // Placeholder - implement random affix generation
            Debug.Log($"Adding random affix to {item.itemName}");
        }
    }
}
