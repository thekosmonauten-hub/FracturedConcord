# Class Selection Button - Playback Speed Control

Quick guide for adjusting video playback speed on class selection buttons.

---

## 🎯 Overview

**Feature:** Each class selection button can now have its own custom playback speed for the transition video.

**Use Cases:**
- Fast transitions (1.5x, 2x) for snappy feel
- Slow-motion transitions (0.5x, 0.75x) for dramatic effect
- Different speeds per class for variety

---

## ✅ How to Adjust Playback Speed

### **Method 1: Inspector (Recommended)**

1. **Open CharacterCreation scene**
2. **Select a class button** (e.g., `MarauderStartingDeck`)
3. **In Inspector → ClassSelectionButton component:**
   ```
   Transition Settings
   ├─ Target Scene Name: CharacterDisplayUI
   ├─ Custom Transition Video: [CharacterCreationSelectClass_2.mp4]
   └─ Playback Speed: 1.0  ← Adjust this!
   ```
4. **Adjust the slider:**
   - **0.1 - 0.9**: Slower than normal (slow-motion)
   - **1.0**: Normal speed (default)
   - **1.1 - 3.0**: Faster than normal (snappy)

5. **Save scene** (Ctrl+S)

---

## 🎨 Speed Examples

### **Normal Speed (1.0x)**
```
Playback Speed: 1.0
```
- Standard playback
- Video plays at original speed
- Good for: Default transitions

### **Fast Speed (1.5x)**
```
Playback Speed: 1.5
```
- 50% faster
- Video completes in 67% of original time
- Good for: Snappy, energetic transitions

### **Very Fast (2.0x)**
```
Playback Speed: 2.0
```
- 2x faster
- Video completes in half the time
- Good for: Quick transitions, impatient players

### **Slow Motion (0.5x)**
```
Playback Speed: 0.5
```
- Half speed
- Video takes twice as long
- Good for: Dramatic, cinematic transitions

### **Very Slow (0.25x)**
```
Playback Speed: 0.25
```
- Quarter speed
- Video takes 4x longer
- Good for: Epic, dramatic moments

---

## 🎯 Per-Class Speed Examples

You can set different speeds for each class:

```
WitchStartingDeck:
  Playback Speed: 1.5  (Fast, magical feel)

MarauderStartingDeck:
  Playback Speed: 1.0  (Normal, balanced)

RangerStartingDeck:
  Playback Speed: 0.75 (Slow, careful feel)

ThiefStartingDeck:
  Playback Speed: 2.0  (Very fast, quick)

ApostleStartingDeck:
  Playback Speed: 0.5  (Slow-motion, dramatic)

BrawlerStartingDeck:
  Playback Speed: 1.2  (Slightly fast, energetic)
```

---

## 🔧 Programmatic Control

### **Set Speed in Code:**

```csharp
// Get the button component
ClassSelectionButton button = GetComponent<ClassSelectionButton>();

// Set playback speed
button.SetPlaybackSpeed(1.5f);  // 1.5x speed

// Get current speed
float currentSpeed = button.GetPlaybackSpeed();
```

### **Dynamic Speed Based on Class:**

```csharp
void OnClassSelected(string className)
{
    ClassSelectionButton button = GetComponent<ClassSelectionButton>();
    
    // Set speed based on class
    switch (className)
    {
        case "Witch":
            button.SetPlaybackSpeed(1.5f);  // Fast
            break;
        case "Marauder":
            button.SetPlaybackSpeed(1.0f);  // Normal
            break;
        case "Ranger":
            button.SetPlaybackSpeed(0.75f); // Slow
            break;
    }
}
```

---

## 📊 Speed vs Duration

**Formula:**
```
Actual Duration = Original Duration / Playback Speed
```

**Examples:**

| Original Duration | Speed | Actual Duration |
|------------------|-------|-----------------|
| 5.0 seconds | 1.0x | 5.0 seconds |
| 5.0 seconds | 1.5x | 3.33 seconds |
| 5.0 seconds | 2.0x | 2.5 seconds |
| 5.0 seconds | 0.5x | 10.0 seconds |
| 5.0 seconds | 0.25x | 20.0 seconds |

---

## 🧪 Testing

### **Test Different Speeds:**

1. **Set Playback Speed to 1.5** on a class button
2. **Press Play**
3. **Click the class button**
4. **Watch:** Video should play 50% faster
5. **Check Console:**
   ```
   [ClassSelectionButton] Playing custom video: ... @ 1.5x speed
   ━━━ [VideoTransition] Playing: ... [FORWARD @ 1.5x] → ...
   ```

### **Compare Speeds:**

1. **Set Witch button to 1.5x**
2. **Set Marauder button to 0.5x**
3. **Test both** and compare the feel

---

## 🎨 Design Tips

### **When to Use Fast Speeds (1.5x - 2.0x):**
- ✅ Quick transitions (don't want to wait)
- ✅ Energetic classes (Thief, Brawler)
- ✅ Action-packed moments
- ✅ Players who skip cutscenes

### **When to Use Normal Speed (1.0x):**
- ✅ Default transitions
- ✅ Balanced feel
- ✅ Most classes

### **When to Use Slow Speeds (0.5x - 0.9x):**
- ✅ Dramatic moments
- ✅ Important class selections
- ✅ Cinematic feel
- ✅ Classes with weight (Marauder, Apostle)

---

## ⚙️ Technical Details

**Speed Range:**
- **Minimum:** 0.1x (very slow)
- **Maximum:** 3.0x (very fast)
- **Default:** 1.0x (normal)

**Clamping:**
- Values outside 0.1-3.0 are automatically clamped
- `SetPlaybackSpeed()` ensures valid range

**Reverse Transitions:**
- Cancel button uses 1.5x reverse (hardcoded)
- Forward transitions use button's `playbackSpeed` value

---

## 📋 Quick Setup Checklist

- [ ] Open CharacterCreation scene
- [ ] Select a class button
- [ ] Find ClassSelectionButton component
- [ ] Adjust Playback Speed slider
- [ ] Test in Play Mode
- [ ] Adjust as needed
- [ ] Save scene

---

## 🎯 Current Implementation

**✅ Implemented:**
- Playback speed field in Inspector (Range: 0.1-3.0)
- Speed passed to VideoTransitionManager
- Helper methods: `SetPlaybackSpeed()`, `GetPlaybackSpeed()`
- Debug logs show speed in Console

**✅ Ready to Use:**
- Adjust speed per button in Inspector
- Set speed programmatically in code
- Different speeds per class

---

**Last Updated:** 2024-12-19
**Status:** ✅ Fully Implemented - Ready to Use

