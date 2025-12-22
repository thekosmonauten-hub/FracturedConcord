# Reverse Video Transition Setup

This guide explains how to set up reverse video transitions (e.g., for "Back" or "Cancel" buttons).

---

## 🎯 Overview

**Feature:** Play transition videos in reverse at custom speeds when going back to a previous scene.

**Example Use Case:**
- User clicks class button → video plays forward → loads CharacterDisplayUI
- User clicks "Cancel" button → **same video plays in reverse at 1.5x speed** → returns to CharacterCreation

---

## ✅ Two Ways to Set Up Reverse Transitions

### **Method 1: Using Code (Current Implementation)**

The `CharacterDisplayController.OnBackClicked()` method already implements this:

```csharp
void OnBackClicked()
{
    VideoTransitionManager transitionManager = VideoTransitionManager.Instance;
    
    if (transitionManager != null)
    {
        transitionManager.PlayTransitionAndLoadScene(
            "CharacterCreation",    // Scene to load
            null,                    // Use default video
            1.5f,                    // 1.5x speed
            true                     // Play in reverse
        );
    }
    
    classSelectionData.Clear();
}
```

**Pros:**
- ✅ Most control
- ✅ Can add custom logic (clearing data, etc.)
- ✅ Already implemented for CharacterDisplayUI

**Cons:**
- ❌ Requires editing script for each button

---

### **Method 2: Using ReverseTransitionButton Component (Inspector-Friendly)**

For future "Back" buttons, you can use the helper component:

1. **Select the button GameObject** (e.g., "BackButton", "CancelButton")
2. **Add Component** → Search "Reverse Transition Button"
3. **Configure in Inspector:**
   ```
   Transition Settings
   ├─ Target Scene Name: CharacterCreation
   ├─ Custom Transition Video: [Optional - leave empty for default]
   └─ Playback Speed: 1.5
   
   Debug
   └─ Show Debug Logs: ✅ (for testing)
   ```
4. **Done!** The button will automatically play reverse transitions.

**Pros:**
- ✅ No code editing needed
- ✅ Inspector-friendly
- ✅ Reusable for any button

**Cons:**
- ❌ Less flexible than code

---

## 🎬 How Reverse Playback Works

### **Technical Details:**

Unity's `VideoPlayer` supports reverse playback using **negative playback speed**:

```csharp
// Forward playback
videoPlayer.playbackSpeed = 1.5f;   // 1.5x speed forward

// Reverse playback
videoPlayer.playbackSpeed = -1.5f;  // 1.5x speed backward
```

**Implementation Steps:**

1. **Prepare the video** normally
2. **Seek to the end** of the video:
   ```csharp
   videoPlayer.time = videoClip.length - 0.01; // Start at end
   ```
3. **Set negative playback speed**:
   ```csharp
   videoPlayer.playbackSpeed = -1.5f; // Reverse at 1.5x
   ```
4. **Play** and wait for video to reach time 0

**Duration Adjustment:**

When playing at 1.5x speed, the video takes **67% of original time**:
```
Original duration: 3.0 seconds
At 1.5x speed: 3.0 / 1.5 = 2.0 seconds
```

---

## 🎨 Customization Options

### **Different Speeds:**

```csharp
// Slower reverse (dramatic exit)
transitionManager.PlayTransitionAndLoadScene(sceneName, null, 0.5f, true);

// Normal speed reverse
transitionManager.PlayTransitionAndLoadScene(sceneName, null, 1.0f, true);

// Fast reverse (snappy)
transitionManager.PlayTransitionAndLoadScene(sceneName, null, 2.0f, true);
```

### **Different Videos for Forward/Reverse:**

```csharp
// Forward: Use Video A
classButton.customTransitionVideo = videoA;

// Reverse: Use Video B
transitionManager.PlayTransitionAndLoadScene(sceneName, videoB, 1.5f, true);
```

### **Same Video, Different Speeds:**

```csharp
// Forward at normal speed
transitionManager.PlayTransitionAndLoadScene("CharacterDisplayUI", video, 1.0f, false);

// Reverse at 2x speed
transitionManager.PlayTransitionAndLoadScene("CharacterCreation", video, 2.0f, true);
```

---

## 🧪 Testing

### **Test Reverse Transition:**

1. **Start from MainMenu**
2. Click "Start Journey"
3. Select a class (forward transition plays)
4. In CharacterDisplayUI, click "Cancel/Back"
5. **Verify:**
   - Same video plays in reverse
   - Plays at 1.5x speed (should be faster)
   - Returns to CharacterCreation scene

### **Expected Console Output:**

```
[CharacterDisplayController] Back button clicked - playing reverse transition
━━━ [VideoTransition] Playing: CharacterCreationSelectClass_2 [REVERSE @ -1.5x] → CharacterCreation ━━━
[VideoTransition] Video prepared (speed: -1.5x), starting playback...
[VideoTransition] Seeking to end of video (2.98s) for reverse playback
[VideoTransition] Video finished after 2.01s
[VideoTransition] Loading scene: CharacterCreation
```

**Note:** The actual playback time should be shorter than the video's length due to 1.5x speed.

---

## 🔧 VideoTransitionManager API

### **Method Signature:**

```csharp
public void PlayTransitionAndLoadScene(
    string sceneName,       // Scene to load after transition
    VideoClip customVideo,  // Optional custom video (null = use default)
    float playbackSpeed,    // Speed multiplier (1.0 = normal, 1.5 = faster)
    bool playInReverse      // If true, plays video backward
)
```

### **Overloads:**

```csharp
// Simple (forward, normal speed)
PlayTransitionAndLoadScene("NextScene");

// With custom video (forward, normal speed)
PlayTransitionAndLoadScene("NextScene", myVideo);

// With speed and direction control (full control)
PlayTransitionAndLoadScene("NextScene", myVideo, 1.5f, true);
```

---

## 🎯 Use Cases

### **1. Back/Cancel Buttons (Current)**
```csharp
// User cancels character creation
transitionManager.PlayTransitionAndLoadScene("CharacterCreation", null, 1.5f, true);
```

### **2. Fast Exit Transitions**
```csharp
// Quick exit from menu (2x speed)
transitionManager.PlayTransitionAndLoadScene("MainMenu", null, 2.0f, true);
```

### **3. Dramatic Slow-Motion Reverse**
```csharp
// Slow-mo reverse for emphasis (0.5x speed)
transitionManager.PlayTransitionAndLoadScene("PreviousScene", null, 0.5f, true);
```

### **4. Different Forward/Reverse Videos**
```csharp
// Forward: Door opening
transitionManager.PlayTransitionAndLoadScene("Town", doorOpenVideo, 1.0f, false);

// Reverse: Door closing (different video)
transitionManager.PlayTransitionAndLoadScene("WorldMap", doorCloseVideo, 1.0f, false);
```

---

## 🐛 Troubleshooting

### ❌ "Video plays forward instead of reverse"

**Fix:**
- Verify `playInReverse` parameter is `true`
- Check Console for: `[VideoTransition] Playing: ... [REVERSE @ -1.5x]`
- If it shows positive speed, the parameter isn't being passed correctly

### ❌ "Reverse video is too slow/fast"

**Fix:**
- Adjust `playbackSpeed` parameter
- `1.0` = normal reverse speed
- `1.5` = 1.5x faster (recommended for back buttons)
- `2.0` = 2x faster (very snappy)
- `0.5` = half speed (slow-motion)

### ❌ "Video doesn't play at all"

**Fix:**
1. Check if VideoTransitionManager exists in scene
2. Verify default video is assigned in Inspector
3. Enable debug logs: `Show Debug Logs = ✅`
4. Check Console for error messages

### ❌ "Reverse video stutters or jumps"

**Possible causes:**
- Video codec doesn't support reverse playback well
- Try re-encoding video with a different codec
- H.264 generally works well with Unity's VideoPlayer

---

## 📊 Reverse Transition Flow

```
User clicks Back/Cancel button
    ↓
OnBackClicked() / OnButtonClicked()
    ↓
VideoTransitionManager.PlayTransitionAndLoadScene(
    sceneName, video, 1.5f, true
)
    ↓
VideoTransitionCoroutine starts
    ↓
Set playbackSpeed = -1.5 (negative = reverse)
    ↓
Prepare video
    ↓
Seek to end of video
    ↓
Play() → video plays backward
    ↓
Wait for video.time to reach 0
    ↓
Load target scene
```

---

## ✨ Benefits

**User Experience:**
- ✅ Smooth, cinematic back navigation
- ✅ Visual consistency (same video forward/backward)
- ✅ Faster reverse = snappier feel for "Cancel" actions
- ✅ Professional polish

**Technical:**
- ✅ No need for separate "reverse" video files
- ✅ Single video file used for both directions
- ✅ Saves storage space
- ✅ Easy to implement

---

## 📋 Quick Setup Checklist

For CharacterDisplayUI "Cancel" button:
- [x] VideoTransitionManager supports reverse playback
- [x] CharacterDisplayController.OnBackClicked() uses reverse transition
- [x] Playback speed set to 1.5x
- [ ] Test: Click Cancel and verify reverse playback

For new back buttons (future):
- [ ] Add ReverseTransitionButton component to button
- [ ] Set Target Scene Name
- [ ] Set Playback Speed (default: 1.5)
- [ ] Test reverse transition

---

## 🎯 Current Implementation Status

**✅ Implemented:**
- VideoTransitionManager supports reverse playback
- Speed control (any multiplier)
- Direction control (forward/reverse)
- Duration adjustment for speed
- Proper waiting for reverse playback to complete
- CharacterDisplayUI "Cancel" button uses reverse transition at 1.5x

**✅ Ready to Use:**
- ReverseTransitionButton component for Inspector setup
- Full API for programmatic control
- Works with default or custom videos

**🧪 Ready to Test:**
- Click Cancel button in CharacterDisplayUI
- Should play transition in reverse at 1.5x speed

---

**Last Updated:** 2024-12-19
**Status:** ✅ Fully Implemented - Ready to Test

