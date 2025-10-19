# Deck Builder Input Field Update

## 🎯 What Changed

**Deck Name** and **Deck Description** are now **editable input fields** instead of static text displays!

### **Before:**
- Deck name was read-only text
- Player couldn't rename decks
- Description was static

### **After:**
- ✅ Click deck name to edit
- ✅ Type new name before saving
- ✅ Auto-validates and sanitizes names
- ✅ Prevents duplicates and invalid characters
- ✅ Description is also editable

---

## 🔧 Unity Scene Updates Required

You need to **replace TextMeshProUGUI with TMP_InputField** in your scene:

### **Step 1: Update Deck Name Field** (2 minutes)

1. Open your **DeckBuilder** scene
2. Find the **DeckNameText** GameObject (in Deck Info Panel)
3. **Remove Component** → Remove **TextMeshProUGUI**
4. **Add Component** → **TMP_InputField**
5. Configure the Input Field:
   - **Text Component**: Drag the child **Text** GameObject here
   - **Character Limit**: 50
   - **Content Type**: Standard
   - **Line Type**: Single Line
6. **Visual Setup** (optional but recommended):
   - Add a **Image** component (background)
   - Set color to semi-transparent (e.g., `rgba(40, 40, 40, 180)`)
   - Set **Transition**: Color Tint on the Input Field

### **Step 2: Update Deck Description Field** (2 minutes)

1. Find the **DeckDescriptionText** GameObject
2. **Remove Component** → Remove **TextMeshProUGUI**
3. **Add Component** → **TMP_InputField**
4. Configure the Input Field:
   - **Text Component**: Drag the child **Text** GameObject here
   - **Character Limit**: 200
   - **Content Type**: Standard
   - **Line Type**: Multi Line Submit (allows Enter key)
5. **Visual Setup**:
   - Add **Image** background
   - Make it taller to accommodate multiple lines

### **Step 3: Reassign References in DeckBuilderUI**

1. Select the **DeckBuilderUI** GameObject
2. In the **DeckBuilderUI** component:
   - **Deck Name Input Field**: Drag the renamed DeckNameInputField
   - **Deck Description Input Field**: Drag DeckDescriptionInputField
3. **Remove old references** (if they still exist):
   - Old: Deck Name Text (remove)
   - Old: Deck Description Text (remove)

---

## 📝 Example Hierarchy

### **Before:**
```
DeckPanel
├── DeckInfoHeader
│   ├── DeckNameText (TextMeshProUGUI) ❌
│   ├── DeckSizeText (TextMeshProUGUI) ✅ Keep as is
│   ├── DeckDescriptionText (TextMeshProUGUI) ❌
│   └── DeckValidityIndicator (Image) ✅ Keep as is
```

### **After:**
```
DeckPanel
├── DeckInfoHeader
│   ├── DeckNameInputField (Image + TMP_InputField) ✅ NEW
│   │   └── Text (TextMeshProUGUI child)
│   ├── DeckSizeText (TextMeshProUGUI) ✅ Keep as is
│   ├── DeckDescriptionInputField (Image + TMP_InputField) ✅ NEW
│   │   └── Text (TextMeshProUGUI child)
│   └── DeckValidityIndicator (Image) ✅ Keep as is
```

---

## 🎨 Recommended Input Field Styling

### **Deck Name Input Field:**

**RectTransform:**
- Width: 400px
- Height: 50px
- Anchor: Top-Center

**Image (Background):**
- Color: `rgba(40, 40, 40, 180)` - Dark semi-transparent
- Sprite: UI-Default or custom panel sprite

**TMP_InputField:**
- Font Size: 24
- Font: Bold
- Alignment: Center/Left
- Text Color: White
- Placeholder Text: "Enter Deck Name..."
- Placeholder Color: `rgba(255, 255, 255, 100)` - Faded white

---

### **Deck Description Input Field:**

**RectTransform:**
- Width: 400px
- Height: 100px
- Anchor: Top-Center

**Image (Background):**
- Color: `rgba(30, 30, 30, 150)` - Slightly darker
- Sprite: UI-Default

**TMP_InputField:**
- Font Size: 16
- Font: Regular
- Alignment: Top-Left
- Text Color: Light Gray `rgba(220, 220, 220, 255)`
- Placeholder Text: "Add deck description..."
- Line Type: Multi Line Submit

---

## 🚀 New Features

### **1. Real-Time Validation**

When player types a name and presses Enter or clicks away:
- ✅ **Checks for empty names** → Reverts to previous name
- ✅ **Checks for duplicates** → Shows error message
- ✅ **Sanitizes invalid characters** → Removes `/\:*?"<>|`
- ✅ **Limits length** → Max 50 characters

### **2. Auto-Sanitization**

Invalid characters are automatically removed:
```
Player types: "My Deck/Build #1"
Saved as:     "My Deck Build 1"
```

### **3. Duplicate Prevention**

```
Player types: "Berserker Build"
Error: "A deck named 'Berserker Build' already exists!"
→ Name reverts to original
```

### **4. File System Safety**

All names are safe for Windows, Mac, and Linux file systems:
- No special characters
- No leading/trailing spaces
- Proper length limits

---

## 🎮 Player Workflow

### **Old Workflow:**
1. Build deck
2. Click "New Deck" → Creates "New Deck"
3. Click "Save" → Saves as "New Deck"
4. No way to rename!

### **New Workflow:**
1. Build deck
2. **Click deck name field**
3. **Type custom name**: "Berserker Build"
4. Press Enter (validates name)
5. Click "Save" → Saves as "Berserker Build" ✅

---

## 🐛 Troubleshooting

### **Issue: Input field not appearing**

**Fix:** Ensure you added the **Image** component for the background. TMP_InputField requires a Graphic component.

### **Issue: Can't type in the field**

**Fix:** 
1. Check **Interactable** is enabled on TMP_InputField
2. Ensure there's an **EventSystem** in the scene
3. Check the **Text Component** reference is assigned

### **Issue: Placeholder text not showing**

**Fix:**
1. Create a child GameObject named "Placeholder"
2. Add TextMeshProUGUI component
3. Assign to **Placeholder** field in TMP_InputField
4. Set color to faded white: `rgba(255, 255, 255, 100)`

### **Issue: Text doesn't update when changing decks**

**Fix:** This is handled automatically by `UpdateDeckInfoDisplay()`. If it's not working, check the reference is assigned in DeckBuilderUI inspector.

---

## ✅ Testing Checklist

- [ ] Click deck name field → Cursor appears, can type
- [ ] Type new name, press Enter → Name updates
- [ ] Type duplicate name → Shows error, reverts
- [ ] Type empty name → Shows error, reverts
- [ ] Type name with `/` → Automatically removed
- [ ] Save deck → Saves with custom name
- [ ] Load different deck → Name field updates
- [ ] Description field also editable → Works

---

## 📚 Code Reference

### **Key Methods Added:**

```csharp
// Called when player finishes editing name
private void OnDeckNameChanged(string newName)

// Called when player finishes editing description
private void OnDeckDescriptionChanged(string newDescription)

// Removes invalid file system characters
private string SanitizeDeckName(string name)
```

### **Validation Rules:**

- **Min Length**: 1 character (after sanitization)
- **Max Length**: 50 characters
- **Invalid Characters**: `/ \ : * ? " < > |` (removed automatically)
- **Duplicates**: Checked against all existing decks
- **Empty**: Defaults to "Unnamed Deck"

---

## 🎉 Benefits

✅ **Better UX** - Players can name decks intuitively  
✅ **No File Errors** - Names are always file-system safe  
✅ **Duplicate Prevention** - Avoids conflicts  
✅ **Real-Time Feedback** - Instant validation messages  
✅ **Editable Descriptions** - Add deck notes/strategies  

---

Your Deck Builder now has professional-grade deck naming! 🃏✨








