using UnityEngine;
using System.Collections.Generic;

namespace Dexiled.Data.Items
{
    /// <summary>
    /// ScriptableObject database for rare item name generation
    /// Contains prefix pool and suffix pools categorized by item type
    /// </summary>
    [CreateAssetMenu(fileName = "NameGenerationData", menuName = "Dexiled/Items/Name Generation Data")]
    public class NameGenerationData : ScriptableObject
    {
        [Header("Rare Item Name Generation")]
        [Tooltip("Pool of prefixes for rare item names (e.g., 'Forsaken', 'Abyssal')")]
        public List<string> rarePrefixes = new List<string>();
        
        [Header("Suffix Pools by Category")]
        [Tooltip("Weapon suffixes (Swords, Axes, Maces, Daggers, Spears)")]
        public List<string> weaponMeleeSuffixes = new List<string>();
        
        [Tooltip("Bow & Projectile suffixes")]
        public List<string> weaponRangedSuffixes = new List<string>();
        
        [Tooltip("Staff & Wand suffixes")]
        public List<string> weaponCasterSuffixes = new List<string>();
        
        [Tooltip("Helmet suffixes")]
        public List<string> helmetSuffixes = new List<string>();
        
        [Tooltip("Body Armour suffixes")]
        public List<string> bodyArmourSuffixes = new List<string>();
        
        [Tooltip("Glove suffixes")]
        public List<string> gloveSuffixes = new List<string>();
        
        [Tooltip("Boot suffixes")]
        public List<string> bootSuffixes = new List<string>();
        
        [Tooltip("Belt suffixes")]
        public List<string> beltSuffixes = new List<string>();
        
        [Tooltip("Amulet suffixes")]
        public List<string> amuletSuffixes = new List<string>();
        
        [Tooltip("Ring suffixes")]
        public List<string> ringSuffixes = new List<string>();
        
        [Tooltip("Shield suffixes")]
        public List<string> shieldSuffixes = new List<string>();
        
        /// <summary>
        /// Get appropriate suffix list based on item slot and type
        /// </summary>
        public List<string> GetSuffixPoolForItem(BaseItem item)
        {
            if (item is WeaponItem weapon)
            {
                // Determine weapon category based on WeaponItemType enum
                switch (weapon.weaponType)
                {
                    case WeaponItemType.Sword:
                    case WeaponItemType.Axe:
                    case WeaponItemType.Mace:
                    case WeaponItemType.Dagger:
                    case WeaponItemType.RitualDagger:
                    case WeaponItemType.Claw:
                    case WeaponItemType.Sceptre:
                        return weaponMeleeSuffixes;
                    
                    case WeaponItemType.Bow:
                        return weaponRangedSuffixes;
                    
                    case WeaponItemType.Staff:
                    case WeaponItemType.Wand:
                        return weaponCasterSuffixes;
                    
                    default:
                        return weaponMeleeSuffixes;
                }
            }
            else if (item is Armour armour)
            {
                switch (armour.armourSlot)
                {
                    case ArmourSlot.Helmet:
                        return helmetSuffixes;
                    
                    case ArmourSlot.BodyArmour:
                        return bodyArmourSuffixes;
                    
                    case ArmourSlot.Gloves:
                        return gloveSuffixes;
                    
                    case ArmourSlot.Boots:
                        return bootSuffixes;
                    
                    case ArmourSlot.Shield:
                        return shieldSuffixes;
                    
                    default:
                        return bodyArmourSuffixes;
                }
            }
            else if (item is Jewellery jewellery)
            {
                switch (jewellery.jewelleryType)
                {
                    case JewelleryType.Amulet:
                        return amuletSuffixes;
                    
                    case JewelleryType.Ring:
                        return ringSuffixes;
                    
                    case JewelleryType.Belt:
                        return beltSuffixes;
                    
                    default:
                        return amuletSuffixes;
                }
            }
            
            // Fallback to weapon melee suffixes
            return weaponMeleeSuffixes;
        }
    }
}

