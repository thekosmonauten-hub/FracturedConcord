# CharacterPanel Prefab Structure Guide (Legacy UI)

## Complete UI Hierarchy

```
CharacterPanel (Canvas)
├── Background (Image)
│   └── Background Image Component
│       - Color: Semi-transparent black (0, 0, 0, 0.8)
│       - Raycast Target: true
│
├── MainContainer (VerticalLayoutGroup)
│   ├── Header (HorizontalLayoutGroup)
│   │   ├── Title (TextMeshPro)
│   │   │   - Text: "Character Stats"
│   │   │   - Font Size: 24
│   │   │   - Color: White
│   │   │   - Alignment: Left
│   │   │
│   │   └── CloseButton (Button)
│   │       ├── Background (Image)
│   │       └── Text (TextMeshPro) - "X"
│   │
│   ├── ContentArea (ScrollRect)
│   │   ├── Viewport (Mask)
│   │   │   └── Content (VerticalLayoutGroup)
│   │   │       ├── CharacterInfoSection
│   │   │       ├── ResourceBarsSection
│   │   │       ├── AttributesSection
│   │   │       ├── CombatStatsSection
│   │   │       ├── DamageModifiersSection
│   │   │       ├── ResistancesSection
│   │   │       ├── DefenseStatsSection
│   │   │       ├── RecoveryStatsSection
│   │   │       ├── CombatMechanicsSection
│   │   │       ├── CardSystemSection
│   │   │       └── EquipmentSummarySection
│   │   │
│   │   ├── Scrollbar Vertical (Scrollbar)
│   │   └── Scrollbar Horizontal (Scrollbar)
│   │
│   └── Footer (HorizontalLayoutGroup)
│       ├── TotalPointsText (TextMeshPro)
│       ├── AllocatedPointsText (TextMeshPro)
│       └── AvailablePointsText (TextMeshPro)
```

## Section Details

### **CharacterInfoSection**
```
CharacterInfoSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Character Information"
├── CharacterPortrait (Image) - 64x64
├── CharacterName (TextMeshPro) - Font Size 18, Bold
├── CharacterClass (TextMeshPro) - Font Size 16, Italic
└── CharacterLevel (TextMeshPro) - Font Size 16
```

### **ResourceBarsSection**
```
ResourceBarsSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Resources"
├── HealthBar (Slider)
│   ├── Background (Image)
│   ├── Fill Area
│   │   └── Fill (Image) - Red color
│   └── HealthText (TextMeshPro)
├── EnergyShieldBar (Slider)
│   ├── Background (Image)
│   ├── Fill Area
│   │   └── Fill (Image) - Blue color
│   └── EnergyShieldText (TextMeshPro)
├── ManaBar (Slider)
│   ├── Background (Image)
│   ├── Fill Area
│   │   └── Fill (Image) - Purple color
│   └── ManaText (TextMeshPro)
└── RelianceBar (Slider)
    ├── Background (Image)
    ├── Fill Area
    │   └── Fill (Image) - Yellow color
    └── RelianceText (TextMeshPro)
```

### **AttributesSection**
```
AttributesSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Core Attributes"
├── StrengthRow (HorizontalLayoutGroup)
│   ├── StrengthLabel (TextMeshPro) - "Strength:"
│   └── StrengthValue (TextMeshPro) - Red color
├── DexterityRow (HorizontalLayoutGroup)
│   ├── DexterityLabel (TextMeshPro) - "Dexterity:"
│   └── DexterityValue (TextMeshPro) - Green color
└── IntelligenceRow (HorizontalLayoutGroup)
    ├── IntelligenceLabel (TextMeshPro) - "Intelligence:"
    └── IntelligenceValue (TextMeshPro) - Blue color
```

### **CombatStatsSection**
```
CombatStatsSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Combat Stats"
├── AttackPowerRow (HorizontalLayoutGroup)
├── DefenseRow (HorizontalLayoutGroup)
├── CriticalChanceRow (HorizontalLayoutGroup)
├── CriticalMultiplierRow (HorizontalLayoutGroup)
├── AccuracyRow (HorizontalLayoutGroup)
├── EvasionRow (HorizontalLayoutGroup)
├── BlockChanceRow (HorizontalLayoutGroup)
└── DodgeChanceRow (HorizontalLayoutGroup)
```

### **DamageModifiersSection**
```
DamageModifiersSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Damage Modifiers"
├── PhysicalDamageRow (HorizontalLayoutGroup)
├── FireDamageRow (HorizontalLayoutGroup)
├── ColdDamageRow (HorizontalLayoutGroup)
├── LightningDamageRow (HorizontalLayoutGroup)
├── ChaosDamageRow (HorizontalLayoutGroup)
├── ElementalDamageRow (HorizontalLayoutGroup)
├── SpellDamageRow (HorizontalLayoutGroup)
└── AttackDamageRow (HorizontalLayoutGroup)
```

### **ResistancesSection**
```
ResistancesSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Resistances"
├── PhysicalResistanceRow (HorizontalLayoutGroup)
├── FireResistanceRow (HorizontalLayoutGroup)
├── ColdResistanceRow (HorizontalLayoutGroup)
├── LightningResistanceRow (HorizontalLayoutGroup)
├── ChaosResistanceRow (HorizontalLayoutGroup)
├── ElementalResistanceRow (HorizontalLayoutGroup)
└── AllResistanceRow (HorizontalLayoutGroup)
```

### **DefenseStatsSection**
```
DefenseStatsSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Defense Stats"
├── ArmourRow (HorizontalLayoutGroup)
├── EnergyShieldRow (HorizontalLayoutGroup)
├── SpellDodgeChanceRow (HorizontalLayoutGroup)
└── SpellBlockChanceRow (HorizontalLayoutGroup)
```

### **RecoveryStatsSection**
```
RecoveryStatsSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Recovery Stats"
├── LifeRegenerationRow (HorizontalLayoutGroup)
├── EnergyShieldRegenerationRow (HorizontalLayoutGroup)
├── ManaRegenerationRow (HorizontalLayoutGroup)
├── RelianceRegenerationRow (HorizontalLayoutGroup)
├── LifeLeechRow (HorizontalLayoutGroup)
├── ManaLeechRow (HorizontalLayoutGroup)
└── EnergyShieldLeechRow (HorizontalLayoutGroup)
```

### **CombatMechanicsSection**
```
CombatMechanicsSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Combat Mechanics"
├── AttackSpeedRow (HorizontalLayoutGroup)
├── CastSpeedRow (HorizontalLayoutGroup)
├── MovementSpeedRow (HorizontalLayoutGroup)
├── AttackRangeRow (HorizontalLayoutGroup)
├── ProjectileSpeedRow (HorizontalLayoutGroup)
├── AreaOfEffectRow (HorizontalLayoutGroup)
├── SkillEffectDurationRow (HorizontalLayoutGroup)
└── StatusEffectDurationRow (HorizontalLayoutGroup)
```

### **CardSystemSection**
```
CardSystemSection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Card System"
├── CardsDrawnPerTurnRow (HorizontalLayoutGroup)
├── MaxHandSizeRow (HorizontalLayoutGroup)
├── DiscardPileSizeRow (HorizontalLayoutGroup)
├── ExhaustPileSizeRow (HorizontalLayoutGroup)
├── CardDrawChanceRow (HorizontalLayoutGroup)
├── CardRetentionChanceRow (HorizontalLayoutGroup)
└── CardUpgradeChanceRow (HorizontalLayoutGroup)
```

### **EquipmentSummarySection**
```
EquipmentSummarySection (VerticalLayoutGroup)
├── SectionHeader (TextMeshPro) - "Equipment Summary"
├── EquippedWeaponText (TextMeshPro)
├── EquippedArmorText (TextMeshPro)
├── EquippedAccessoriesText (TextMeshPro)
└── TotalEquipmentStatsText (TextMeshPro)
```

## Layout Group Settings

### **MainContainer (VerticalLayoutGroup)**
- Padding: Top=10, Bottom=10, Left=10, Right=10
- Spacing: 5
- Child Force Expand Width: true
- Child Force Expand Height: false
- Child Control Width: true
- Child Control Height: false

### **ContentArea (ScrollRect)**
- Viewport: Stretch to fill
- Content: VerticalLayoutGroup with proper spacing
- Scroll Sensitivity: 1
- Movement Type: Elastic
- Elasticity: 0.1
- Inertia: true
- Deceleration Rate: 0.135

### **Section Headers**
- Font Size: 16
- Font Style: Bold
- Color: Light Blue (0.7, 0.9, 1.0)
- Alignment: Left
- Padding: Top=5, Bottom=5

### **Stat Rows (HorizontalLayoutGroup)**
- Spacing: 10
- Child Force Expand Width: false
- Child Force Expand Height: true
- Child Control Width: true
- Child Control Height: true

## Color Scheme

- **Positive Stats**: Green (0.2, 0.8, 0.2)
- **Negative Stats**: Red (0.8, 0.2, 0.2)
- **Neutral Stats**: White (1.0, 1.0, 1.0)
- **Section Headers**: Light Blue (0.7, 0.9, 1.0)
- **Background**: Semi-transparent black (0, 0, 0, 0.8)

## Component Assignment

After creating the hierarchy, assign all UI elements to the `CharacterStatsController` script:

1. **Character Info References**
2. **Resource Bar References**
3. **Stat Section References**
4. **Equipment Summary References**

## Tips for Implementation

1. **Use Layout Groups** for automatic positioning
2. **Set proper anchors** for responsive design
3. **Use ContentSizeFitter** for dynamic sizing
4. **Implement proper scrolling** for large stat lists
5. **Add visual feedback** for stat changes
6. **Use consistent spacing** throughout the UI











