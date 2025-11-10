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
}
