# Video Transition Troubleshooting Guide

Quick guide to fix common video transition issues.

---

## 🚨 Issue: Reverse Transition Not Playing

**Symptom:** Clicking "Cancel" button just loads the scene without playing video in reverse.

### **Fix 1: Check Console for Error Messages** ⭐ **START HERE**

1. **Open Console** (Window → General → Console)
2. **Click Cancel button**
3. **Look for these messages:**

**✅ SUCCESS - Should see:**
```
[VideoTransitionManager] ✓ Stored video for reverse: CharacterCreationSelectClass_2
[CharacterDisplayController] Back button clicked - playing reverse transition
[VideoTransitionManager] Using last used video: CharacterCreationSelectClass_2
━━━ [VideoTransition] Playing: CharacterCreationSelectClass_2 [REVERSE @ -1.5x] → CharacterCreation ━━━
```

**❌ ERROR - If you see:**
```
[VideoTransitionManager] ❌ NO VIDEO CLIP AVAILABLE!
  - customVideo: NULL
  - lastUsedVideo: NULL
  - transitionVideo: NULL
Loading scene directly without transition.
```

**This means no video was assigned anywhere!**

---

### **Fix 2: Assign Video to Class Selection Button** ⭐ **MOST COMMON FIX**

The forward transition needs a video assigned:

1. **In CharacterCreation scene:**
   - Select a class button (e.g., `MarauderStartingDeck`)
2. **Find `ClassSelectionButton` component** in Inspector
3. **Assign the video:**
   ```
   Transition Settings
   └─ Custom Transition Video: [CharacterCreationSelectClass_2.mp4]
   ```
4. **Save scene** (Ctrl+S)
5. **Test:** Select class → then click Cancel

**Why this works:**
- When you click the class button, VideoTransitionManager stores the video
- When you click Cancel, it uses the stored video in reverse
- If no video was stored, reverse transition fails

---

### **Fix 3: Assign Default Video to VideoTransitionManager**

If you want ALL transitions to use the same default video:

1. **Find `VideoTransitionManager` GameObject:**
   - It's a DontDestroyOnLoad singleton
   - May be in MainMenu scene or CharacterCreation scene
   - Search Hierarchy: "VideoTransitionManager"
2. **Select it** and find the component
3. **Assign default video:**
   ```
   Video Settings
   └─ Transition Video: [CharacterCreationSelectClass_2.mp4]
   ```
4. **Save scene**

**Why this works:**
- Provides a fallback video if no custom video is assigned
- Used when lastUsedVideo is null

---

## 🔍 Diagnostic Steps

### **Step 1: Verify VideoTransitionManager Exists**

1. **Start Play Mode**
2. **Open Hierarchy**
3. **Look for GameObject:** `VideoTransitionManager`
4. **Check if it says:** `(DontDestroyOnLoad)` next to it

**If missing:**
- VideoTransitionManager wasn't created
- Check if MainMenu scene has it
- Check if CharacterCreation scene loads it

---

### **Step 2: Check Video Assignment**

**In CharacterCreation scene:**

1. **Select a class button** (e.g., `WitchStartingDeck`)
2. **Check Inspector → ClassSelectionButton:**
   - `Custom Transition Video`: Should have a video assigned
3. **If empty:** Assign `CharacterCreationSelectClass_2.mp4`

**OR in VideoTransitionManager:**

1. **Select VideoTransitionManager** in Hierarchy
2. **Check Inspector:**
   - `Transition Video`: Should have a video assigned

**At least ONE of these must be assigned!**

---

### **Step 3: Test Forward Transition**

1. **Press Play**
2. **Click a class button**
3. **Watch Console:**
   ```
   [ClassSelectionButton] Playing custom video: CharacterCreationSelectClass_2
   ✓ Stored video for reverse: CharacterCreationSelectClass_2
   ━━━ [VideoTransition] Playing: ... [FORWARD @ 1x] → CharacterDisplayUI ━━━
   ```

**If forward transition works:**
- Video is assigned correctly
- VideoTransitionManager stored it
- Reverse should work

**If forward transition doesn't work:**
- Video isn't assigned
- Go back to Fix 2

---

### **Step 4: Test Reverse Transition**

After successful forward transition:

1. **In CharacterDisplayUI scene, click "Cancel"**
2. **Watch Console:**
   ```
   [CharacterDisplayController] Back button clicked - playing reverse transition
   [VideoTransitionManager] Using last used video: CharacterCreationSelectClass_2
   ━━━ [VideoTransition] Playing: ... [REVERSE @ -1.5x] → CharacterCreation ━━━
   ```

**If reverse works:**
- ✅ Success! You're done!

**If reverse fails:**
- Continue to Fix 4

---

### **Fix 4: Enable Debug Logs**

1. **Select class button** in CharacterCreation
2. **ClassSelectionButton component:**
   ```
   Debug
   └─ Show Debug Logs: ✅
   ```
3. **Select VideoTransitionManager**
4. *(No debug toggle needed - logs always on)*
5. **Test again** and read Console output

---

### **Fix 5: Check Scene Build Settings**

1. **File → Build Settings**
2. **Verify scenes are added:**
   ```
   ✅ MainMenu
   ✅ CharacterCreation
   ✅ CharacterDisplayUI
   ```
3. **If missing:** Click "Add Open Scenes" while scene is open

---

## 🎯 Common Mistakes

### ❌ **Mistake 1: No Video Assigned Anywhere**

**Problem:**
- No video on class button
- No default video on VideoTransitionManager

**Fix:**
- Assign video to EITHER class button OR VideoTransitionManager

---

### ❌ **Mistake 2: VideoTransitionManager Missing**

**Problem:**
- No VideoTransitionManager in scene
- Not created from MainMenu

**Fix:**
- Ensure MainMenu scene has VideoTransitionManager
- It should persist via DontDestroyOnLoad

---

### ❌ **Mistake 3: Wrong Video File Type**

**Problem:**
- Video file isn't recognized by Unity
- VideoClip shows as null in Inspector

**Fix:**
- Use supported formats: .mp4, .mov, .webm
- Re-import video asset

---

### ❌ **Mistake 4: Scene Names Mismatch**

**Problem:**
- Target scene name is wrong
- Scene doesn't exist or is named differently

**Fix:**
- Check exact scene name in Project window
- Update `targetSceneName` to match exactly (case-sensitive)

---

## 🧪 Test Procedure

### **Complete Test Flow:**

1. **Start from MainMenu**
2. Click "Start Journey" → CharacterCreation
3. Click a class button (watch for video forward)
4. Should load CharacterDisplayUI
5. Click "Cancel" button
6. **Video should play in REVERSE at 1.5x speed**
7. Returns to CharacterCreation

### **Expected Console Output:**

```
[MainMenu] Start Journey clicked
[VideoTransition] Playing: ... [FORWARD @ 1x] → CharacterCreation
✓ Stored video for reverse: CharacterCreationSelectClass_2

[ClassSelectionButton] Marauder button clicked
[VideoTransition] Playing: CharacterCreationSelectClass_2 [FORWARD @ 1x] → CharacterDisplayUI
✓ Stored video for reverse: CharacterCreationSelectClass_2

[CharacterDisplayController] Back button clicked - playing reverse transition
[VideoTransitionManager] Using last used video: CharacterCreationSelectClass_2
━━━ [VideoTransition] Playing: CharacterCreationSelectClass_2 [REVERSE @ -1.5x] → CharacterCreation ━━━
[VideoTransition] Video prepared (speed: -1.5x), starting playback...
[VideoTransition] Seeking to end of video (2.98s) for reverse playback
[VideoTransition] Video finished after 2.01s
```

---

## 🔧 Quick Fixes Summary

| Issue | Quick Fix |
|-------|-----------|
| No reverse transition | Assign video to class button's `Custom Transition Video` field |
| No forward transition | Same - assign video to class button |
| Both transitions fail | Assign default video to VideoTransitionManager |
| Video plays but not reverse | Check console - might be codec issue |
| Scene loads instantly | VideoTransitionManager missing or no video assigned |

---

## 📝 Checklist

Before reporting a bug, check:

- [ ] Video assigned to class button OR VideoTransitionManager
- [ ] Forward transition works (class button → CharacterDisplayUI)
- [ ] Console shows "✓ Stored video for reverse"
- [ ] VideoTransitionManager exists in Hierarchy
- [ ] Scenes added to Build Settings
- [ ] Video file is .mp4, .mov, or .webm
- [ ] No errors in Console

---

## 💡 Pro Tips

**Tip 1:** Always test forward transition first
- If forward works, reverse should work automatically

**Tip 2:** Use custom video per button
- Different classes can have different transition videos

**Tip 3:** Use default video for consistency
- All transitions use the same video

**Tip 4:** Check Console immediately
- New detailed error messages tell you exactly what's wrong

---

**Last Updated:** 2024-12-19
**Status:** Enhanced with lastUsedVideo memory

