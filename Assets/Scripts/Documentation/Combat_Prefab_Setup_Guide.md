# Combat Prefab Setup Guide
## Player and Enemy Prefab Creation

### Overview
This guide will help you create Player and Enemy prefabs for the combat system. These prefabs will display character/enemy stats, health, and combat information using the new `PlayerCombatDisplay` and `EnemyCombatDisplay` scripts.

---

## 🎯 Player Prefab Setup

### **Step 1: Create Player Prefab Structure**

1. **Create Empty GameObject** in the scene
   - Name it `PlayerCombatPrefab`
   - Add `PlayerCombatDisplay` script

2. **Add Canvas Group** (for easy visibility control)
   - Add `CanvasGroup` component
   - Set `Alpha` to 1
   - Set `Interactable` to true
   - Set `Blocks Raycasts` to true

### **Step 2: Create UI Hierarchy**

```
PlayerCombatPrefab
├── CharacterPortrait (Image)
├── CharacterName (TextMeshPro - Text)
├── HealthBar (Slider)
│   ├── Background (Image)
│   ├── Fill Area
│   │   └── Fill (Image) - Assign to healthFillImage
│   ├── EnergyShieldBar (Slider) - Overlay on top of health
│   │   ├── Background (Image) - Transparent
│   │   └── Fill Area
│   │       └── Fill (Image) - Assign to energyShieldFillImage
│   ├── GuardBar (Slider) - Overlay on top of health and energy shield
│   │   ├── Background (Image) - Transparent
│   │   └── Fill Area
│   │       └── Fill (Image) - Assign to guardFillImage
│   ├── HealthText (TextMeshPro - Text)
│   ├── EnergyShieldText (TextMeshPro - Text)
│   └── GuardText (TextMeshPro - Text)
└── ManaBar (Slider)
    ├── Background (Image)
    ├── Fill Area
    │   └── Fill (Image) - Assign to manaFillImage
    └── ManaText (TextMeshPro - Text)
```

### **Step 3: Configure UI Elements**

#### **Character Portrait**
- **Size**: 64x64 pixels
- **Position**: Top-left of the prefab
- **Image**: Assign character portrait sprite

#### **Character Name**
- **Font size**: 16, bold
- **Position**: Below or next to portrait

#### **Resource Bars**
- **HealthBar**: Red color, width 200px, height 20px
- **EnergyShieldBar**: Light blue with transparency, overlays on top of health bar
- **GuardBar**: Yellow with transparency, overlays on top of health and energy shield (limited to current health)
- **ManaBar**: Blue color, width 200px, height 20px

### **Step 4: Assign Script References**

In the `PlayerCombatDisplay` component:
- **Character Portrait**: Assign the CharacterPortrait Image
- **Character Name Text**: Assign CharacterName TextMeshPro
- **Health Slider**: Assign HealthBar Slider
- **Health Text**: Assign HealthText TextMeshPro
- **Health Fill Image**: Assign Fill Image from HealthBar
- **Energy Shield Slider**: Assign EnergyShieldBar Slider
- **Energy Shield Text**: Assign EnergyShieldText TextMeshPro
- **Energy Shield Fill Image**: Assign Fill Image from EnergyShieldBar
- **Mana Slider**: Assign ManaBar Slider
- **Mana Text**: Assign ManaText TextMeshPro
- **Mana Fill Image**: Assign Fill Image from ManaBar
- **Guard Slider**: Assign GuardBar Slider
- **Guard Text**: Assign GuardText TextMeshPro
- **Guard Fill Image**: Assign Fill Image from GuardBar

### **Step 5: Configure Colors**

Set the color values in the `PlayerCombatDisplay` script:
- **Health Color**: Red (255, 0, 0, 255)
- **Energy Shield Color**: Light Blue with transparency (128, 204, 255, 204)
- **Mana Color**: Blue (0, 0, 255, 255)
- **Guard Color**: Yellow with transparency (255, 255, 0, 179)

### **Step 6: Create Prefab**

1. Drag the configured `PlayerCombatPrefab` from the scene to the `Assets/Prefabs/` folder
2. Name it `PlayerCombatPrefab.prefab`
3. Delete the scene instance

---

## 🎯 Enemy Prefab Setup

### **Step 1: Create Enemy Prefab Structure**

1. **Create Empty GameObject** in the scene
   - Name it `EnemyCombatPrefab`
   - Add `EnemyCombatDisplay` script

2. **Add Canvas Group** (for easy visibility control)
   - Add `CanvasGroup` component
   - Set `Alpha` to 1
   - Set `Interactable` to true
   - Set `Blocks Raycasts` to true

### **Step 2: Create UI Hierarchy**

```
EnemyCombatPrefab
├── EnemyPortrait (Image)
├── EnemyInfo
│   ├── EnemyName (TextMeshPro - Text)
│   └── EnemyType (TextMeshPro - Text)
├── HealthBar (Slider)
│   ├── Background (Image)
│   ├── Fill Area
│   │   └── Fill (Image) - Assign to healthFillImage
│   └── HealthText (TextMeshPro - Text)
├── IntentContainer (GameObject)
│   ├── IntentIcon (Image)
│   ├── IntentText (TextMeshPro - Text)
│   └── IntentDamageText (TextMeshPro - Text)
└── StatusEffectsContainer (GameObject)
```

### **Step 3: Configure UI Elements**

#### **Enemy Portrait**
- **Size**: 64x64 pixels
- **Position**: Top-left of the prefab
- **Image**: Assign enemy portrait sprite

#### **Enemy Info Section**
- **EnemyName**: Font size 16, bold
- **EnemyType**: Font size 14, italic

#### **Health Bar**
- **Color**: Red
- **Width**: 200px
- **Height**: 20px

#### **Intent Container**
- **IntentIcon**: 32x32 pixels
- **IntentText**: Font size 14
- **IntentDamageText**: Font size 16, bold, red color

### **Step 4: Assign Script References**

In the `EnemyCombatDisplay` component:
- **Enemy Portrait**: Assign the EnemyPortrait Image
- **Enemy Name Text**: Assign EnemyName TextMeshPro
- **Enemy Type Text**: Assign EnemyType TextMeshPro
- **Health Slider**: Assign HealthBar Slider
- **Health Text**: Assign HealthText TextMeshPro
- **Health Fill Image**: Assign Fill Image from HealthBar
- **Intent Container**: Assign IntentContainer GameObject
- **Intent Icon**: Assign IntentIcon Image
- **Intent Text**: Assign IntentText TextMeshPro
- **Intent Damage Text**: Assign IntentDamageText TextMeshPro
- **Status Effects Container**: Assign StatusEffectsContainer GameObject

### **Step 5: Configure Colors and Icons**

Set the color values in the `EnemyCombatDisplay` script:
- **Health Color**: Red (255, 0, 0, 255)
- **Attack Intent Color**: Red (255, 0, 0, 255)
- **Defend Intent Color**: Blue (0, 0, 255, 255)

Assign intent icons:
- **Attack Icon**: Sword or weapon sprite
- **Defend Icon**: Shield sprite

### **Step 6: Create Prefab**

1. Drag the configured `EnemyCombatPrefab` from the scene to the `Assets/Prefabs/` folder
2. Name it `EnemyCombatPrefab.prefab`
3. Delete the scene instance

---

## 🎯 Combat Scene Integration

### **Step 1: Add Combat Display Manager**

1. **Create Empty GameObject** in the combat scene
   - Name it `CombatDisplayManager`
   - Add `CombatDisplayManager` script

2. **Configure Combat Display Manager**:
   - **Player Display**: Assign PlayerCombatPrefab instance
   - **Enemy Displays**: Assign EnemyCombatPrefab instances (up to 3)
   - **Max Enemies**: Set to 3
   - **Auto Start Combat**: Enable for testing
   - **Turn Delay**: Set to 1 second
   - **Create Test Enemies**: Enable for testing
   - **Test Enemy Count**: Set to 2

### **Step 2: Scene Layout**

```
CombatScene
├── Canvas (Main UI Canvas)
│   ├── PlayerArea (Left side)
│   │   └── PlayerCombatPrefab (Instance)
│   ├── EnemyArea (Right side)
│   │   ├── EnemyCombatPrefab (Instance 1)
│   │   ├── EnemyCombatPrefab (Instance 2)
│   │   └── EnemyCombatPrefab (Instance 3)
│   └── CardArea (Bottom)
│       └── SimpleCombatUI
└── CombatDisplayManager
```

### **Step 3: Positioning**

#### **Player Area**
- **Position**: Left side of screen
- **Anchor**: Middle-left
- **Offset**: (50, 0, 0)

#### **Enemy Area**
- **Position**: Right side of screen
- **Anchor**: Middle-right
- **Spacing**: 100 pixels between enemies
- **Offset**: (-50, 0, 0)

#### **Card Area**
- **Position**: Bottom of screen
- **Anchor**: Bottom-center
- **Height**: 300 pixels

---

## 🎯 Testing and Debugging

### **Player Prefab Testing**

1. **Test Damage**: Right-click PlayerCombatDisplay → Test Damage
2. **Test Heal**: Right-click PlayerCombatDisplay → Test Heal
3. **Test Mana Use**: Right-click PlayerCombatDisplay → Test Mana Use

### **Enemy Prefab Testing**

1. **Test Damage**: Right-click EnemyCombatDisplay → Test Damage
2. **Test Heal**: Right-click EnemyCombatDisplay → Test Heal
3. **Update Intent**: Right-click EnemyCombatDisplay → Update Intent
4. **Create Test Enemy**: Right-click EnemyCombatDisplay → Create Test Enemy

### **Combat Display Manager Testing**

1. **Start Combat**: Right-click CombatDisplayManager → Start Combat
2. **End Player Turn**: Right-click CombatDisplayManager → End Player Turn
3. **Test Player Attack**: Right-click CombatDisplayManager → Test Player Attack
4. **Test Player Heal**: Right-click CombatDisplayManager → Test Player Heal
5. **Create Test Enemies**: Right-click CombatDisplayManager → Create Test Enemies

---

## 🎯 Integration with Card System

### **Future Integration Points**

1. **Card Damage Integration**:
   ```csharp
   // In card effect system
   combatDisplayManager.PlayerAttackEnemy(targetEnemyIndex, cardDamage);
   ```

2. **Card Healing Integration**:
   ```csharp
   // In card effect system
   combatDisplayManager.PlayerHeal(healAmount);
   ```

3. **Turn Management**:
   ```csharp
   // When player finishes playing cards
   combatDisplayManager.EndPlayerTurn();
   ```

4. **Mana Cost Integration**:
   ```csharp
   // Check if player has enough mana for card
   if (characterManager.UseMana(cardCost))
   {
       // Play card
   }
   ```

---

## 🎯 Best Practices

### **Performance Optimization**

1. **Object Pooling**: Consider pooling enemy prefabs for multiple encounters
2. **Event-Driven Updates**: Use events to update UI only when needed
3. **Batch Updates**: Update multiple UI elements in single frame

### **Scalability**

1. **Modular Design**: Each prefab is self-contained
2. **Scriptable Objects**: Use ScriptableObjects for enemy data
3. **Configurable**: Easy to modify stats and appearance

### **Maintenance**

1. **Clear Naming**: Use descriptive names for all UI elements
2. **Documentation**: Keep this guide updated
3. **Version Control**: Track prefab changes in git

---

## 🎯 Troubleshooting

### **Common Issues**

1. **UI Not Updating**:
   - Check if CharacterManager exists in scene
   - Verify event subscriptions
   - Ensure prefab references are assigned

2. **Prefab Not Found**:
   - Check Resources folder path
   - Verify prefab name matches
   - Ensure prefab is in correct folder

3. **Script Errors**:
   - Check for missing using statements
   - Verify component references
   - Ensure proper inheritance

### **Debug Tools**

- Use context menu options for testing
- Check Console for error messages
- Use Debug.Log statements for tracking

---

*This guide should be updated as the combat system evolves.*
