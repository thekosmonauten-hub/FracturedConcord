# Class Splash Art Setup

How to display base class splash art on CharacterDisplayUI.

---

## 🎯 Overview

Display the base class's splash art (Marauder, Witch, etc.) alongside the Ascendancy options.

**Your splash art location:**
`Assets/Art/CharCreation/SplashArt/Class/ClassSplashArt.png`

---

## ✅ Setup Options

### **Option 1: Manual Override** ⭐ **RECOMMENDED** (No file moving needed)

1. **Open CharacterDisplayUI scene**
2. **Select `CharacterDisplayController` GameObject**
3. **In Inspector → Class Splash Art:**
   ```
   Class Splash Art Image: [Drag Image component to display splash art]
   ```

4. **Set up overrides:**
   ```
   Class Splash Art Overrides
   ├─ Size: 6 (one per class)
   ├─ Element 0
   │   ├─ Class Name: "Marauder"
   │   └─ Splash Art: [Drag Assets/Art/CharCreation/SplashArt/Class/MarauderSplashArt.png]
   ├─ Element 1
   │   ├─ Class Name: "Witch"
   │   └─ Splash Art: [Drag WitchSplashArt.png]
   ├─ Element 2
   │   ├─ Class Name: "Ranger"
   │   └─ Splash Art: [Drag RangerSplashArt.png]
   ├─ Element 3
   │   ├─ Class Name: "Thief"
   │   └─ Splash Art: [Drag ThiefSplashArt.png]
   ├─ Element 4
   │   ├─ Class Name: "Apostle"
   │   └─ Splash Art: [Drag ApostleSplashArt.png]
   └─ Element 5
       ├─ Class Name: "Brawler"
       └─ Splash Art: [Drag BrawlerSplashArt.png]
   ```

5. **Save scene**
6. **Done!** No file moving required.

---

### **Option 2: Resources Folder** (For automatic loading)

1. **Move or copy splash art files to Resources:**
   ```
   From: Assets/Art/CharCreation/SplashArt/Class/
   To:   Assets/Resources/Art/CharCreation/SplashArt/Class/
   ```

2. **Rename files to match pattern:**
   ```
   MarauderSplashArt.png
   WitchSplashArt.png
   RangerSplashArt.png
   ThiefSplashArt.png
   ApostleSplashArt.png
   BrawlerSplashArt.png
   ```

3. **In CharacterDisplayController Inspector:**
   ```
   Class Splash Art
   ├─ Class Splash Art Image: [Drag Image component]
   └─ Class Splash Art Resource Path: "Art/CharCreation/SplashArt/Class"
   ```

4. **Leave overrides empty** (auto-loads from Resources)

---

## 🎨 Where to Display Class Splash Art

### **Recommended Locations:**

1. **Behind Ascendancy buttons** (as background)
2. **On the opposite page** (book layout)
3. **At the top** of the page (header)
4. **Center of page** with Ascendancy buttons around it

### **Example Setup:**

```
CharacterDisplayUI
└── Background
    └── RightPage
        ├── ClassSplashArt (Image) ← Add this
        │   └── Display large class art
        └── AscendancySection
            ├── Ascendancy1 (Button)
            ├── Ascendancy2 (Button)
            └── Ascendancy3 (Button)
```

---

## 🔧 Create Class Splash Art Image

1. **In CharacterDisplayUI scene**
2. **Navigate to where you want the splash art**
3. **Right-click → UI → Image**
4. **Name:** `ClassSplashArt`
5. **Configure:**
   ```
   Rect Transform:
   ├─ Size: 400x600 (or your desired size)
   └─ Position: Center or top of page
   
   Image:
   ├─ Source Image: [Leave empty - set at runtime]
   ├─ Color: White
   ├─ Preserve Aspect: ✅ (recommended)
   └─ Raycast Target: ❌ (no interaction needed)
   ```

6. **Drag this Image** to:
   ```
   CharacterDisplayController → Class Splash Art → Class Splash Art Image
   ```

---

## 🧪 Testing

### **With Manual Overrides:**

1. **Setup overrides** in Inspector (6 classes)
2. **Press Play**
3. **Select Marauder**
4. **Check Console:**
   ```
   [CharacterDisplayController] Using override splash art for Marauder
   ✓ Set class splash art for Marauder: MarauderSplashArt
   ```
5. **Verify:** Class splash art appears on screen

### **With Resources Folder:**

1. **Move files to Resources**
2. **Set Resource Path** in Inspector
3. **Press Play**
4. **Select Marauder**
5. **Check Console:**
   ```
   [CharacterDisplayController] Loaded class splash art from Resources: Art/CharCreation/SplashArt/Class/MarauderSplashArt
   ✓ Set class splash art for Marauder: MarauderSplashArt
   ```

---

## 📊 File Naming Patterns

The system tries these naming patterns automatically:

1. `{Class}SplashArt` → `MarauderSplashArt.png`
2. `{Class}` → `Marauder.png`
3. `Class{Class}SplashArt` → `ClassMarauderSplashArt.png`
4. `{class}` → `marauder.png` (lowercase)

**Your current files:** `ClassSplashArt.png`

**Rename to:**
- `MarauderSplashArt.png`
- `WitchSplashArt.png`
- `RangerSplashArt.png`
- `ThiefSplashArt.png`
- `ApostleSplashArt.png`
- `BrawlerSplashArt.png`

**OR** just use manual overrides and keep current names!

---

## 💡 Recommended Approach

**Use Manual Overrides because:**
- ✅ No file moving/renaming needed
- ✅ Works with current file structure
- ✅ Easy to set up in Inspector
- ✅ More control

**Just drag your 6 splash art files to the override slots!**

---

## 🎨 Visual Design Tips

### **Layout Idea 1: Split Page**
```
Left Page:                Right Page:
┌─────────────┐          ┌─────────────┐
│   CLASS     │          │ ASCENDANCY  │
│  SPLASH     │          │   OPTIONS   │
│    ART      │          │  [Button 1] │
│             │          │  [Button 2] │
│  (Marauder) │          │  [Button 3] │
└─────────────┘          └─────────────┘
```

### **Layout Idea 2: Background**
```
┌─────────────────────────┐
│   CLASS SPLASH ART      │
│   (Faded/transparent)   │
│                         │
│  ┌───┐ ┌───┐ ┌───┐    │
│  │ 1 │ │ 2 │ │ 3 │    │
│  └───┘ └───┘ └───┘    │
│  Ascendancy Buttons    │
└─────────────────────────┘
```

### **Layout Idea 3: Header**
```
┌─────────────────────────┐
│   [Class Splash Art]    │
│      (Small, top)       │
├─────────────────────────┤
│  ASCENDANCY OPTIONS     │
│  ┌───────┐              │
│  │   1   │              │
│  ├───────┤              │
│  │   2   │              │
│  ├───────┤              │
│  │   3   │              │
│  └───────┘              │
└─────────────────────────┘
```

---

## 📋 Quick Setup Checklist

- [ ] Create Image GameObject for class splash art
- [ ] Position it on the page
- [ ] Assign to CharacterDisplayController → Class Splash Art Image
- [ ] Setup manual overrides (6 classes)
- [ ] Drag each class's splash art file
- [ ] Test with Marauder
- [ ] Verify splash art displays

---

## 🔧 Alternative: Direct File Path (No Resources)

If you don't want to move files OR use overrides, use direct loading:

Since your files are already in the project (not in Resources), **Manual Overrides is the easiest solution**.

---

**Last Updated:** 2024-12-19
**Status:** ✅ Implemented - Use Manual Overrides for Easy Setup


