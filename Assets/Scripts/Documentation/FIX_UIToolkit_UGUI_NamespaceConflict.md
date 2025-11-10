# Fix: UI Toolkit + UGUI Namespace Conflict

## Problem

When using both **UI Toolkit** (`UnityEngine.UIElements`) and **UGUI** (`UnityEngine.UI`) in the same script, you'll encounter namespace conflicts because both have classes with the same names:

### Common Conflicts
- `Button` (exists in both namespaces)
- `Image` (exists in both namespaces)
- `Toggle` (exists in both namespaces)
- `Slider` (exists in both namespaces)

### Error Example
```
CS0104: 'Button' is an ambiguous reference between 
'UnityEngine.UI.Button' and 'UnityEngine.UIElements.Button'
```

---

## Solution: Use Namespace Aliases

### Method 1: Type Aliases (Recommended)
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Create aliases for conflicting types
using UIToolkitButton = UnityEngine.UIElements.Button;
using UGUIButton = UnityEngine.UI.Button;
using UIToolkitImage = UnityEngine.UIElements.Image;
using UGUIImage = UnityEngine.UI.Image;

public class MyController : MonoBehaviour
{
    // Now you can use the aliases
    private UIToolkitButton myUIToolkitButton;  // UI Toolkit button
    private UGUIButton myUGUIButton;             // UGUI button
    private UIToolkitImage myUIToolkitImage;     // UI Toolkit image
    private UGUIImage myUGUIImage;               // UGUI image
}
```

### Method 2: Full Namespace Qualification
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MyController : MonoBehaviour
{
    // Explicitly specify which namespace
    private UnityEngine.UIElements.Button myUIToolkitButton;
    private UnityEngine.UI.Button myUGUIButton;
}
```

**Pros of Method 1 (Aliases):**
- Cleaner code
- Easier to read
- Less typing
- Clear intent (UIToolkit vs UGUI)

**Pros of Method 2 (Full Qualification):**
- No extra using statements
- Explicit namespace reference

---

## CharacterCreationController Implementation

### Applied Fix
```csharp
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Aliases to resolve conflicts
using UIToolkitButton = UnityEngine.UIElements.Button;
using UGUIButton = UnityEngine.UI.Button;

public class CharacterCreationController : MonoBehaviour
{
    // UI Toolkit buttons (from UXML)
    private UIToolkitButton createCharacterButton;
    private UIToolkitButton backButton;
    private UIToolkitButton witchButton;
    private UIToolkitButton marauderButton;
    private UIToolkitButton rangerButton;
    private UIToolkitButton thiefButton;
    private UIToolkitButton apostleButton;
    private UIToolkitButton brawlerButton;
    
    // If we ever need UGUI buttons:
    // private UGUIButton myUGUIButton;
}
```

---

## When to Use Which?

### Use UI Toolkit When:
- ✅ Building main menus and UI screens
- ✅ Creating layout-heavy interfaces
- ✅ Need responsive/flexible layouts
- ✅ Want better performance for large lists
- ✅ Working with UXML/USS files

### Use UGUI When:
- ✅ Need world space UI (health bars, name plates)
- ✅ Complex visual effects with shaders
- ✅ Legacy systems already using UGUI
- ✅ Prefab-based UI components
- ✅ Camera-space rendering needed

### Use Hybrid Approach When:
- ✅ Need layout from UI Toolkit + visuals from UGUI
- ✅ Reusing existing UGUI prefabs (like cards)
- ✅ Want flexibility to mix both systems

---

## Common Namespace Conflicts Reference

### Button
```csharp
using UIToolkitButton = UnityEngine.UIElements.Button;
using UGUIButton = UnityEngine.UI.Button;
```

### Image
```csharp
using UIToolkitImage = UnityEngine.UIElements.Image;
using UGUIImage = UnityEngine.UI.Image;
```

### Toggle
```csharp
using UIToolkitToggle = UnityEngine.UIElements.Toggle;
using UGUIToggle = UnityEngine.UI.Toggle;
```

### Slider
```csharp
using UIToolkitSlider = UnityEngine.UIElements.Slider;
using UGUISlider = UnityEngine.UI.Slider;
```

### ScrollView
```csharp
using UIToolkitScrollView = UnityEngine.UIElements.ScrollView;
using UGUIScrollView = UnityEngine.UI.ScrollRect;  // Note: different name!
```

---

## Best Practices

### 1. Use Descriptive Aliases
```csharp
// ✅ GOOD - Clear which UI system
using UIToolkitButton = UnityEngine.UIElements.Button;
using UGUIButton = UnityEngine.UI.Button;

// ❌ BAD - Confusing abbreviations
using UIB = UnityEngine.UIElements.Button;
using B2 = UnityEngine.UI.Button;
```

### 2. Place Aliases at Top
```csharp
// ✅ GOOD - At top with other using statements
using UnityEngine;
using UnityEngine.UIElements;
using UIToolkitButton = UnityEngine.UIElements.Button;

// ❌ BAD - Scattered throughout file
using UnityEngine;
// ... 50 lines of code ...
using UIToolkitButton = UnityEngine.UIElements.Button;
```

### 3. Document Why You're Using Both
```csharp
/// <summary>
/// Hybrid UI controller using:
/// - UI Toolkit for main layout and deck list
/// - UGUI for card prefab rendering on hover
/// </summary>
public class CharacterCreationController : MonoBehaviour
{
    // ...
}
```

### 4. Avoid Mixing When Possible
```csharp
// If you can avoid using both, do so!
// Only use hybrid when it provides real benefits
```

---

## Migration Tips

### If You Need to Add UGUI Components Later
```csharp
// Already have this
using UIToolkitButton = UnityEngine.UIElements.Button;

// Just add more aliases as needed
using UGUIButton = UnityEngine.UI.Button;
using UGUIImage = UnityEngine.UI.Image;
using UGUIToggle = UnityEngine.UI.Toggle;
```

### If You Need to Remove UI Toolkit Later
```csharp
// Remove the UI Toolkit using
// using UnityEngine.UIElements;

// Remove the aliases
// using UIToolkitButton = UnityEngine.UIElements.Button;

// Change all UIToolkitButton back to Button
// Unity will now use UnityEngine.UI.Button by default
```

---

## Related Issues

### Issue: "Canvas is an ambiguous reference"
**Fix**: No conflict! `Canvas` only exists in `UnityEngine`

### Issue: "RectTransform is an ambiguous reference"
**Fix**: No conflict! `RectTransform` only exists in `UnityEngine`

### Issue: "TextField is an ambiguous reference"
**Fix**: No conflict! `TextField` only exists in `UnityEngine.UIElements`

---

## Summary

**Problem**: Namespace conflicts when mixing UI Toolkit and UGUI

**Solution**: Use type aliases to disambiguate conflicting types

**Result**: Clean, readable code that uses both UI systems seamlessly

```csharp
// ✅ Perfect solution
using UIToolkitButton = UnityEngine.UIElements.Button;
using UGUIButton = UnityEngine.UI.Button;

// Now you can use both!
private UIToolkitButton myUIButton;
private UGUIButton myGameObjectButton;
```

---

**Remember**: This is a common issue when working with hybrid UI systems. Always use aliases for clarity! 🎯












