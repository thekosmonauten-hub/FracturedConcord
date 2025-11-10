# Ascendancy System Setup Guide

Complete guide for setting up class-specific Ascendancy options in CharacterDisplayUI.

---

## 🎯 Overview

**Feature:** Display 3 Ascendancy classes for each base class on the CharacterDisplayUI.

**What are Ascendancies?**
- Advanced specializations for each base class
- Permanent upgrades unlocked during gameplay
- Each base class has 3 unique Ascendancy options
- Provide passive bonuses, new abilities, and playstyle modifications

**Example:**
- **Witch** → Occultist, Elementalist, Necromancer
- **Marauder** → Juggernaut, Berserker, Chieftain
- **Ranger** → Deadeye, Raider, Pathfinder

---

## ✅ Setup Steps

### **Step 1: Create Ascendancy Assets**

1. **In Project window, navigate to** `Assets/Resources/Ascendancies`
   - If folder doesn't exist, create it: Right-click → Create → Folder

2. **Create an Ascendancy asset:**
   - Right-click in folder → Create → Dexiled → Ascendancy Data
   - Name it: `WitchOccultist` (or similar)

3. **Configure the Ascendancy:**
   ```
   Basic Info
   ├─ Ascendancy Name: "Occultist"
   ├─ Base Class: "Witch"
   └─ Tagline: "Master of Curses and Chaos"
   
   Visual Assets
   ├─ Splash Art: [Drag your splash art image]
   ├─ Icon: [Drag your icon]
   └─ Theme Color: [Purple/dark colors for Occultist]
   
   Description:
   "The Occultist specializes in curses, chaos damage, and 
   energy shield manipulation..."
   
   Playstyle Keywords:
   - "Damage over Time"
   - "Curses"
   - "Energy Shield"
   
   Bonuses:
   + [Add] → Bonus Type: "Passive", Description: "+20% Chaos Damage"
   + [Add] → Bonus Type: "Skill", Description: "Curses can affect hexproof enemies"
   + [Add] → Bonus Type: "Stat", Description: "+50 Maximum Energy Shield"
   
   Unlock Requirements
   ├─ Required Level: 15
   └─ Unlock Requirement: "Complete the Labyrinth"
   
   Progression
   ├─ Max Ascendancy Points: 8
   └─ Ascendancy Tree Nodes: [Add node IDs if using passive tree]
   ```

4. **Repeat for all 3 Ascendancies per class** (18 total for 6 classes)

---

### **Step 2: Setup AscendancyDatabase**

1. **Create AscendancyDatabase GameObject:**
   - In Hierarchy (any scene, preferably MainMenu or persistent scene)
   - Right-click → Create Empty
   - Name: `AscendancyDatabase`

2. **Add component:**
   - Add Component → Scripts → AscendancyDatabase

3. **Configure in Inspector:**
   ```
   Settings
   ├─ Load From Resources: ✅
   └─ Resources Path: "Ascendancies"
   ```

4. **Make it persistent (optional):**
   - The script handles `DontDestroyOnLoad` automatically
   - It will persist across scenes like a singleton

---

### **Step 3: Setup CharacterDisplayUI Scene**

1. **Open CharacterDisplayUI scene**

2. **Find/Create Ascendancy button GameObjects:**
   - You mentioned: `Ascendancy1`, `Ascendancy2`, `Ascendancy3`
   - These should be UI buttons with Image components

3. **For EACH Ascendancy button:**
   
   **Required children (auto-created by AscendancyButton if missing):**
   ```
   Ascendancy1 (Button)
   ├─ SplashArt (Image) ← Full splash art background
   ├─ Icon (Image) ← Small icon overlay
   ├─ Name (TextMeshPro) ← Ascendancy name
   ├─ Tagline (TextMeshPro) ← Tagline/subtitle
   ├─ BackgroundOverlay (Image) ← Theme color overlay
   └─ LockedOverlay (GameObject) ← Shown when locked
       └─ LockReasonText (TextMeshPro) ← "Unlocks at Level 15"
   ```

4. **Add AscendancyButton component** (optional - will be added automatically):
   - Select `Ascendancy1`
   - Add Component → Scripts → AscendancyButton
   - **Assign references:**
     ```
     UI References
     ├─ Splash Art Image: [Drag SplashArt Image]
     ├─ Icon Image: [Drag Icon Image]
     ├─ Name Text: [Drag Name TextMeshPro]
     ├─ Tagline Text: [Drag Tagline TextMeshPro]
     └─ Background Overlay: [Drag BackgroundOverlay Image]
     
     Lock State
     ├─ Locked Overlay: [Drag LockedOverlay GameObject]
     └─ Lock Reason Text: [Drag LockReasonText]
     ```

5. **Repeat for Ascendancy2 and Ascendancy3**

---

### **Step 4: Wire Up CharacterDisplayController**

1. **Select CharacterDisplayController GameObject** in scene

2. **In Inspector → CharacterDisplayController component:**
   ```
   Ascendancy Display
   ├─ Ascendancy1 Button: [Drag Ascendancy1 GameObject]
   ├─ Ascendancy2 Button: [Drag Ascendancy2 GameObject]
   └─ Ascendancy3 Button: [Drag Ascendancy3 GameObject]
   ```

3. **Save scene** (Ctrl+S)

---

## 🧪 Testing

### **Test with Marauder:**

1. **Create 3 Ascendancy assets** for Marauder:
   - `MarauderJuggernaut` (Base Class: "Marauder")
   - `MarauderBerserker` (Base Class: "Marauder")
   - `MarauderChieftain` (Base Class: "Marauder")

2. **Place in** `Assets/Resources/Ascendancies/`

3. **Press Play**

4. **Navigate:** MainMenu → CharacterCreation → Select Marauder → CharacterDisplayUI

5. **Verify:**
   - ✅ 3 Ascendancy buttons appear
   - ✅ Each shows correct splash art
   - ✅ Each shows correct name and tagline
   - ✅ All are locked (with lock overlay)
   - ✅ Lock reason says "Unlocks at Level 15" (or your set level)

6. **Click an Ascendancy:**
   - Check Console for debug output
   - Should log Ascendancy info (bonuses, description, etc.)

---

## 📊 Ascendancy Asset Structure

### **Folder Structure:**

```
Assets/
└── Resources/
    └── Ascendancies/
        ├── WitchOccultist.asset
        ├── WitchElementalist.asset
        ├── WitchNecromancer.asset
        ├── MarauderJuggernaut.asset
        ├── MarauderBerserker.asset
        ├── MarauderChieftain.asset
        ├── RangerDeadeye.asset
        ├── RangerRaider.asset
        ├── RangerPathfinder.asset
        ├── ThiefAssassin.asset
        ├── ThiefTrickster.asset
        ├── ThiefSaboteur.asset
        ├── ApostleGuardian.asset
        ├── ApostleHierophant.asset
        ├── ApostleInquisitor.asset
        ├── BrawlerChampion.asset
        ├── BrawlerGladiator.asset
        └── BrawlerSlayer.asset
```

### **Naming Convention:**
```
{BaseClass}{AscendancyName}.asset
```

---

## 🎨 Creating Splash Art

### **Splash Art Specs:**

- **Resolution:** 512x512 or higher (recommend 1024x1024)
- **Aspect Ratio:** Square (1:1) or portrait (3:4)
- **Format:** PNG (with transparency) or JPG
- **Style:** Should match game's art style
- **Content:** Character portrait, thematic background

### **Icon Specs:**

- **Resolution:** 128x128 or 256x256
- **Aspect Ratio:** Square (1:1)
- **Format:** PNG with transparency
- **Style:** Simplified version of splash art or unique symbol

---

## 🔧 Customization

### **Locked vs Unlocked State:**

Currently, all Ascendancies are shown as **locked** in CharacterDisplayUI (since this is character creation).

**To show unlocked Ascendancies** (for in-game character screen):

```csharp
// In CharacterDisplayController.cs DisplayAscendancies()
// Change this line:
ascButton.Initialize(ascendancies[i], locked: true, lockReason: $"Unlocks at Level {ascendancies[i].requiredLevel}");

// To:
bool isUnlocked = CheckIfPlayerUnlockedAscendancy(ascendancies[i]);
ascButton.Initialize(ascendancies[i], locked: !isUnlocked, lockReason: isUnlocked ? "" : $"Unlocks at Level {ascendancies[i].requiredLevel}");
```

### **Different Number of Ascendancies:**

If a class has fewer than 3 Ascendancies:
- Unused buttons are automatically hidden
- No errors will occur

If a class has more than 3:
- Only first 3 are shown
- Add more buttons to display all

---

## 💡 Advanced Features (Future)

### **Detailed Info Panel:**

When clicking an Ascendancy, you can show a detailed panel:

```csharp
void OnAscendancyClicked(AscendancyData ascendancy)
{
    // Show panel with:
    // - Full description
    // - List of all bonuses
    // - Passive tree nodes
    // - "Select This Ascendancy" button
    ascendancyDetailPanel.Show(ascendancy);
}
```

### **Ascendancy Selection:**

For in-game Ascendancy selection:

```csharp
void SelectAscendancy(AscendancyData ascendancy)
{
    // Save to CharacterManager
    CharacterManager.Instance.SetAscendancy(ascendancy.ascendancyName);
    
    // Apply bonuses
    ApplyAscendancyBonuses(ascendancy);
    
    // Unlock Ascendancy passive tree
    PassiveTreeManager.Instance.UnlockAscendancyTree(ascendancy);
}
```

### **Dynamic Unlock Requirements:**

```csharp
bool CheckUnlockRequirement(AscendancyData ascendancy)
{
    Character character = CharacterManager.Instance.GetCurrentCharacter();
    
    // Check level
    if (character.level < ascendancy.requiredLevel)
        return false;
    
    // Check quest completion
    if (!string.IsNullOrEmpty(ascendancy.unlockRequirement))
    {
        return QuestManager.Instance.IsQuestCompleted(ascendancy.unlockRequirement);
    }
    
    return true;
}
```

---

## 🐛 Troubleshooting

### ❌ **"No Ascendancies found for class: Marauder"**

**Fix:**
1. Check Ascendancy assets have correct `Base Class` field
2. Must match exactly: "Marauder" (case-sensitive)
3. Check assets are in `Assets/Resources/Ascendancies/` folder

---

### ❌ **Ascendancy buttons don't appear**

**Fix:**
1. Check `Ascendancy1Button`, `Ascendancy2Button`, `Ascendancy3Button` assigned in Inspector
2. Check GameObjects are active in scene
3. Enable Test Mode in CharacterDisplayController
4. Check Console for errors

---

### ❌ **Splash art doesn't show**

**Fix:**
1. Check `Splash Art` field in AscendancyData asset is assigned
2. Check image import settings: Texture Type = "Sprite (2D and UI)"
3. Check SplashArt Image component on button has correct anchors

---

### ❌ **AscendancyDatabase not found**

**Fix:**
1. Create AscendancyDatabase GameObject in scene
2. Add AscendancyDatabase component
3. Set `Load From Resources = ✅`
4. Set `Resources Path = "Ascendancies"`

---

## 📋 Quick Setup Checklist

- [ ] Create `Assets/Resources/Ascendancies/` folder
- [ ] Create 3 AscendancyData assets per class (18 total)
- [ ] Assign splash art and icons to each asset
- [ ] Set Base Class field correctly
- [ ] Add bonuses and descriptions
- [ ] Create AscendancyDatabase GameObject
- [ ] Assign Ascendancy button GameObjects in CharacterDisplayController
- [ ] Test with each class

---

**Last Updated:** 2024-12-19
**Status:** ✅ Fully Implemented - Ready to Setup Assets

