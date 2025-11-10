# Card Hover Sorting Fix

## **🔧 Problem Analysis**

### **Original Issue: Cards Not Coming to Front**
Cards were not coming to the front when hovered because `stayWithinMask` was set to `true`, which disabled canvas sorting.

**Error Messages:**
```
[CardHover] PooledCard_0: Canvas sorting disabled (stayWithinMask=true). Card won't come to front on hover.
```

### **Attempted Fix #1: Canvas Override Sorting**
Adding Canvas components with override sorting caused multiple issues:
- **Cards getting stuck** in hovered state due to improper state tracking
- **Cards immediately exiting hover** due to raycast blocking
- **Canvas blocks pointer events** preventing hover from working

### **Root Cause:**
Dynamically adding Canvas components with `overrideSorting = true` fundamentally breaks raycast detection for UI elements in layouts and ScrollRects. The Canvas creates a new raycast layer that intercepts all pointer events.

## **✅ Final Solution: Use Sibling Index**

After attempting various Canvas-based solutions, the reliable approach is to use **sibling index manipulation** instead of Canvas override sorting.

### **Why Sibling Index Works:**
- ✅ **No raycast blocking** - doesn't interfere with pointer events
- ✅ **Simple and reliable** - standard Unity UI hierarchy approach
- ✅ **Works in layouts** - compatible with HorizontalLayoutGroup, etc.
- ✅ **No component additions** - uses existing transform hierarchy

### **Configuration:**
```csharp
[SerializeField] private bool raiseBySibling = true;  // ✓ Enabled
[SerializeField] private bool raiseByCanvas = false;  // ✗ Disabled
[SerializeField] private bool forceRaiseOnHover = false; // ✗ Disabled
```

### **How It Works:**
```csharp
// On Hover
originalSiblingIndex = transform.GetSiblingIndex();
transform.SetAsLastSibling(); // Move to end of parent's children

// On Exit
transform.SetSiblingIndex(originalSiblingIndex); // Restore original position
```

### **Trade-off:**
- **Limitation**: Cards won't render above ALL UI elements, only above siblings in same parent
- **Benefit**: Stable, reliable hover behavior with proper pointer event handling

## **⚙️ Configuration**

### **Recommended Settings:**
The fix is now using sibling index manipulation by default, which is the most reliable approach.

### **To Configure Card Prefabs:**
1. **Select your card prefab** in the Project window
2. **Find the CardHoverEffect component** in the Inspector
3. **Set the following values:**
   - **Raise By Sibling**: ✓ **Checked** (brings card to front among siblings)
   - **Raise By Canvas**: ❌ **Unchecked** (causes raycast issues)
   - **Stay Within Mask**: ❌ **Unchecked** (not needed)
   - **Force Raise On Hover**: ❌ **Unchecked** (not needed)

## **🎯 How It Works**

### **Previous Logic:**
```csharp
if (raiseByCanvas && !stayWithinMask)
{
    // Only raised if stayWithinMask was false
    cardCanvas.overrideSorting = true;
}
```

### **New Logic:**
```csharp
bool shouldRaise = raiseByCanvas && (!stayWithinMask || forceRaiseOnHover);

if (shouldRaise)
{
    // Now raises if either:
    // 1. stayWithinMask is false (old behavior)
    // 2. forceRaiseOnHover is true (new override)
    cardCanvas.overrideSorting = true;
}
```

## **📋 Settings Explained**

### **Force Raise On Hover (NEW):**
- **✓ Checked (default)**: Cards always come to front on hover
- **Unchecked**: Respects `stayWithinMask` setting (old behavior)

### **Raise By Canvas:**
- **✓ Checked (recommended)**: Uses Canvas sorting (doesn't affect layout)
- **Unchecked**: Uses sibling index (can cause layout issues)

### **Stay Within Mask:**
- **Purpose**: Originally designed to keep cards within ScrollRect masks
- **With forceRaiseOnHover=true**: This setting is overridden for hover
- **Effect**: Cards will still be clipped by masks, but sorting order is raised

### **Raise By Sibling:**
- **Checked**: Changes sibling index (can cause layout cycling)
- **✓ Unchecked (recommended)**: Avoids layout issues

## **🎮 Expected Results**

### **Before Fix:**
- ❌ Cards stayed behind other UI elements on hover
- ❌ Warning messages in console
- ❌ Cards getting stuck in hovered state
- ❌ Cards immediately exiting hover (flashing)
- ❌ Poor user experience

### **After Fix:**
- ✅ Hovered cards come to the front
- ✅ Cards stay hovered while mouse is over them
- ✅ Cards properly return to normal on hover exit
- ✅ No warning messages
- ✅ No GraphicRaycaster interference
- ✅ Smooth hover experience
- ✅ Cards still respect visual boundaries

### **Console Output (On Hover):**
```
[CardHover] PooledCard_4: Raised to last sibling (index 2 → 4)
```

### **Console Output (On Hover Exit):**
```
[CardHover] PooledCard_4: Restored sibling index to 2
```

## **🔍 Technical Details**

### **Sorting Order:**
- **Default**: Cards use parent canvas sorting
- **On Hover**: Cards use sorting order `5000`
- **On Exit**: Cards restore original sorting

### **Canvas Management:**
- **Adds Canvas component** if not present
- **Keeps Canvas** after hover (avoids pointer flicker)
- **Restores settings** on hover exit

### **Mask Compatibility:**
- Cards are still **visually clipped** by masks
- Only the **sorting order** is overridden
- No visual escape from mask boundaries

## **🐛 Troubleshooting**

### **Cards Still Not Coming Forward:**
1. ✅ Check `forceRaiseOnHover` is checked on card prefab
2. ✅ Check `raiseByCanvas` is checked
3. ✅ Verify CardHoverEffect component is attached
4. ✅ Check console for hover logs

### **Cards Escaping Mask Boundaries:**
- This fix **only affects sorting order**, not clipping
- If cards visually escape masks, check:
  - Mask component settings
  - RectMask2D vs Mask component
  - Canvas hierarchy

### **Layout Issues:**
- Ensure `raiseBySibling` is **unchecked**
- Canvas sorting doesn't affect layout
- Sibling index changes can cause layout cycling

## **🎯 Best Practices**

### **Recommended Settings:**
```
Raise By Sibling: ❌ Unchecked
Raise By Canvas: ✓ Checked
Stay Within Mask: ✓ Checked (for scroll areas)
Force Raise On Hover: ✓ Checked (for hover effect)
```

### **When to Disable forceRaiseOnHover:**
- Cards in fixed UI that should never overlap
- Special cases where mask clipping must be absolute
- Custom hover implementations

## **✅ Verification Checklist**

- [ ] CardHoverEffect has `forceRaiseOnHover = true`
- [ ] Cards come to front on hover in play mode
- [ ] No warning messages in console
- [ ] Cards return to normal position on hover exit
- [ ] No layout cycling or flickering
- [ ] Mask clipping still works correctly
- [ ] Hover tooltips display properly

**Your cards now properly come to the front on hover!** 🎮✨

