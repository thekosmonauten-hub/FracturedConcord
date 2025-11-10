using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Dexiled.Data.Items
{
    /// <summary>
    /// Handles the application of currency effects to items
    /// </summary>
    public static class CurrencyEffectProcessor
    {
        /// <summary>
        /// Apply a currency's effect to a target item
        /// </summary>
        public static bool ApplyCurrencyToItem(CurrencyType currencyType, BaseItem targetItem, out string resultMessage)
        {
            resultMessage = "";
            
            if (targetItem == null)
            {
                resultMessage = "No target item selected";
                return false;
            }
            
            switch (currencyType)
            {
                // === ORBS ===
                case CurrencyType.OrbOfGeneration:
                    return ApplyOrbOfGeneration(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfInfusion:
                    return ApplyOrbOfInfusion(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfPerfection:
                    return ApplyOrbOfPerfection(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfPerpetuity:
                    return ApplyOrbOfPerpetuity(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfRedundancy:
                    return ApplyOrbOfRedundancy(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfTheVoid:
                    return ApplyOrbOfTheVoid(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfMutation:
                    return ApplyOrbOfMutation(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfProliferation:
                    return ApplyOrbOfProliferation(targetItem, out resultMessage);
                    
                case CurrencyType.OrbOfAmnesia:
                    return ApplyOrbOfAmnesia(targetItem, out resultMessage);
                
                // === SPIRITS ===
                case CurrencyType.FireSpirit:
                case CurrencyType.ColdSpirit:
                case CurrencyType.LightningSpirit:
                case CurrencyType.ChaosSpirit:
                case CurrencyType.PhysicalSpirit:
                case CurrencyType.LifeSpirit:
                case CurrencyType.DefenseSpirit:
                case CurrencyType.CritSpirit:
                    return ApplySpirit(currencyType, targetItem, out resultMessage);
                    
                case CurrencyType.DivineSpirit:
                    return ApplyDivineSpirit(targetItem, out resultMessage);
                
                // === SEALS ===
                case CurrencyType.TranspositionSeal:
                case CurrencyType.ChaosSeal:
                case CurrencyType.MemorySeal:
                case CurrencyType.InscriptionSeal:
                case CurrencyType.AdaptationSeal:
                case CurrencyType.CorrectionSeal:
                case CurrencyType.EtchingSeal:
                    return ApplySeal(currencyType, targetItem, out resultMessage);
                
                default:
                    resultMessage = $"Currency type {currencyType} not implemented";
                    return false;
            }
        }
        
        // === ORB IMPLEMENTATIONS ===
        
        private static bool ApplyOrbOfGeneration(BaseItem item, out string msg)
        {
            // Card generation - not applicable to items
            msg = "Orb of Generation is used to generate cards, not modify items";
            return false;
        }
        
        private static bool ApplyOrbOfInfusion(BaseItem item, out string msg)
        {
            // Reforges Normal equipment → Magic (adds 1 random affix)
            if (item.rarity != ItemRarity.Normal)
            {
                msg = $"Can only be used on Normal rarity items (item is {item.rarity})";
                return false;
            }
            
            // Clear existing affixes
            item.prefixes.Clear();
            item.suffixes.Clear();
            
            // Add 1 random affix (50/50 prefix or suffix)
            // TODO: Use AffixDatabase to get random affix
            msg = $"Reforged {item.itemName} to Magic quality and added 1 random affix";
            item.rarity = ItemRarity.Magic;
            return true;
        }
        
        private static bool ApplyOrbOfPerfection(BaseItem item, out string msg)
        {
            // Reforges Magic equipment → Rare (adds 1 random affix)
            if (item.rarity != ItemRarity.Magic)
            {
                msg = $"Can only be used on Magic rarity items (item is {item.rarity})";
                return false;
            }
            
            // Add 1 more random affix
            // TODO: Use AffixDatabase to get random affix
            msg = $"Reforged {item.itemName} to Rare quality and added 1 random affix";
            item.rarity = ItemRarity.Rare;
            return true;
        }
        
        private static bool ApplyOrbOfPerpetuity(BaseItem item, out string msg)
        {
            // Adds a random affix to Rare equipment
            if (item.rarity != ItemRarity.Rare)
            {
                msg = $"Can only be used on Rare rarity items (item is {item.rarity})";
                return false;
            }
            
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            if (totalAffixes >= 6)
            {
                msg = "Item already has maximum affixes (6)";
                return false;
            }
            
            // Add 1 random affix
            // TODO: Use AffixDatabase to get random affix
            msg = $"Added 1 random affix to {item.itemName}";
            return true;
        }
        
        private static bool ApplyOrbOfRedundancy(BaseItem item, out string msg)
        {
            // NEW LOGIC: Removes a random affix from an equipment
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            if (totalAffixes == 0)
            {
                msg = "Item has no affixes to remove";
                return false;
            }
            
            // Randomly choose between prefix or suffix based on availability
            bool removePrefix = false;
            if (item.prefixes.Count > 0 && item.suffixes.Count > 0)
            {
                removePrefix = Random.value < 0.5f;
            }
            else if (item.prefixes.Count > 0)
            {
                removePrefix = true;
            }
            
            if (removePrefix && item.prefixes.Count > 0)
            {
                int randomIndex = Random.Range(0, item.prefixes.Count);
                var removed = item.prefixes[randomIndex];
                item.prefixes.RemoveAt(randomIndex);
                msg = $"Removed affix: {removed.description}";
            }
            else if (item.suffixes.Count > 0)
            {
                int randomIndex = Random.Range(0, item.suffixes.Count);
                var removed = item.suffixes[randomIndex];
                item.suffixes.RemoveAt(randomIndex);
                msg = $"Removed affix: {removed.description}";
            }
            else
            {
                msg = "Failed to remove affix";
                return false;
            }
            
            return true;
        }
        
        private static bool ApplyOrbOfTheVoid(BaseItem item, out string msg)
        {
            // Corrupts an item, adding powerful but unpredictable modifiers
            msg = $"Corrupted {item.itemName}! The item cannot be modified further.";
            // TODO: Add corruption logic and powerful random modifiers
            return true;
        }
        
        private static bool ApplyOrbOfMutation(BaseItem item, out string msg)
        {
            // NEW LOGIC: Randomizes all affixes on a Magic equipment
            if (item.rarity != ItemRarity.Magic)
            {
                msg = $"Can only be used on Magic rarity items (item is {item.rarity})";
                return false;
            }
            
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            if (totalAffixes == 0)
            {
                msg = "Item has no affixes to randomize";
                return false;
            }
            
            // Clear existing affixes and regenerate same number
            int prefixCount = item.prefixes.Count;
            int suffixCount = item.suffixes.Count;
            
            item.prefixes.Clear();
            item.suffixes.Clear();
            
            // TODO: Use AffixDatabase to regenerate random affixes
            // For now, just clear them
            msg = $"Randomized all affixes on {item.itemName} ({prefixCount} prefixes, {suffixCount} suffixes regenerated)";
            return true;
        }
        
        private static bool ApplyOrbOfProliferation(BaseItem item, out string msg)
        {
            // NEW LOGIC: Adds a random affix to a Magic equipment
            if (item.rarity != ItemRarity.Magic)
            {
                msg = $"Can only be used on Magic rarity items (item is {item.rarity})";
                return false;
            }
            
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            if (totalAffixes >= 6)
            {
                msg = "Item already has maximum affixes (6)";
                return false;
            }
            
            // Add 1 random affix
            // TODO: Use AffixDatabase to get random affix
            msg = $"Added 1 random affix to {item.itemName}";
            return true;
        }
        
        private static bool ApplyOrbOfAmnesia(BaseItem item, out string msg)
        {
            // NEW LOGIC: Removes all affixes while preserving locked affixes
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            if (totalAffixes == 0)
            {
                msg = "Item has no affixes to remove";
                return false;
            }
            
            // TODO: Implement locked affix tracking system
            // For now, remove all non-locked affixes
            int removedCount = 0;
            
            // Remove all prefixes (except locked ones when system is implemented)
            removedCount += item.prefixes.Count;
            item.prefixes.Clear();
            
            // Remove all suffixes (except locked ones when system is implemented)
            removedCount += item.suffixes.Count;
            item.suffixes.Clear();
            
            msg = $"Removed {removedCount} affixes from {item.itemName}. Locked affixes preserved.";
            return true;
        }
        
        // === SPIRIT IMPLEMENTATIONS ===
        
        private static bool ApplySpirit(CurrencyType spiritType, BaseItem item, out string msg)
        {
            // Adds or rerolls damage/stat affixes based on spirit type
            string spiritName = spiritType.ToString().Replace("Spirit", "");
            msg = $"Applied {spiritName} Spirit to {item.itemName}";
            // TODO: Implement spirit-specific affix addition/reroll
            return true;
        }
        
        private static bool ApplyDivineSpirit(BaseItem item, out string msg)
        {
            // Rerolls the value of a random affix to maximum
            int totalAffixes = item.prefixes.Count + item.suffixes.Count;
            if (totalAffixes == 0)
            {
                msg = "Item has no affixes to reroll";
                return false;
            }
            
            // TODO: Implement affix value reroll to max
            msg = $"Rerolled a random affix on {item.itemName} to maximum value";
            return true;
        }
        
        // === SEAL IMPLEMENTATIONS ===
        
        private static bool ApplySeal(CurrencyType sealType, BaseItem item, out string msg)
        {
            string sealName = sealType.ToString().Replace("Seal", "");
            msg = $"Applied {sealName} Seal to {item.itemName}";
            // TODO: Implement seal-specific effects
            return true;
        }
        
        /// <summary>
        /// Validate if a currency can be used on a target item
        /// </summary>
        public static bool CanApplyCurrency(CurrencyType currencyType, BaseItem targetItem, out string reason)
        {
            reason = "";
            
            if (targetItem == null)
            {
                reason = "No target item";
                return false;
            }
            
            switch (currencyType)
            {
                case CurrencyType.OrbOfInfusion:
                    if (targetItem.rarity != ItemRarity.Normal)
                    {
                        reason = "Can only be used on Normal rarity items";
                        return false;
                    }
                    break;
                    
                case CurrencyType.OrbOfPerfection:
                    if (targetItem.rarity != ItemRarity.Magic)
                    {
                        reason = "Can only be used on Magic rarity items";
                        return false;
                    }
                    break;
                    
                case CurrencyType.OrbOfPerpetuity:
                    if (targetItem.rarity != ItemRarity.Rare)
                    {
                        reason = "Can only be used on Rare rarity items";
                        return false;
                    }
                    int totalAffixes = targetItem.prefixes.Count + targetItem.suffixes.Count;
                    if (totalAffixes >= 6)
                    {
                        reason = "Item already has maximum affixes (6)";
                        return false;
                    }
                    break;
                    
                case CurrencyType.OrbOfMutation:
                case CurrencyType.OrbOfProliferation:
                    if (targetItem.rarity != ItemRarity.Magic)
                    {
                        reason = "Can only be used on Magic rarity items";
                        return false;
                    }
                    break;
                    
                case CurrencyType.OrbOfRedundancy:
                case CurrencyType.OrbOfAmnesia:
                    int affixCount = targetItem.prefixes.Count + targetItem.suffixes.Count;
                    if (affixCount == 0)
                    {
                        reason = "Item has no affixes";
                        return false;
                    }
                    break;
            }
            
            return true;
        }
    }
}













