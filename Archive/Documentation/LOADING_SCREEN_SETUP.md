# Loading Screen Setup Guide

## Overview

The Bootstrap scene is the perfect place for a loading screen since it loads first and stays loaded. This guide shows you how to set up a loading screen with a progress bar that displays during bootstrap initialization.

## Quick Setup (5 minutes)

### Step 1: Create Loading Screen UI in Bootstrap Scene

1. **Open BootstrapScene.unity**

2. **Create Canvas**:
   - Right-click Hierarchy → UI → Canvas
   - Name it `LoadingScreenCanvas`
   - Set **Render Mode** to `Screen Space - Overlay`
   - Set **Sort Order** to `9999` (ensures it's on top)

3. **Create Loading Panel**:
   - Right-click `LoadingScreenCanvas` → UI → Panel
   - Name it `LoadingPanel`
   - Set background color to black (or your preferred color)
   - Stretch to fill screen (Anchor: Stretch-Stretch)

4. **Create Progress Bar**:
   - Right-click `LoadingPanel` → UI → Image
   - Name it `ProgressBarBackground`
   - Set color to dark gray
   - Set width: `600`, height: `30`
   - Center it (Anchor: Middle-Center, Pos Y: `-50`)
   
   - Right-click `ProgressBarBackground` → UI → Image
   - Name it `ProgressBarFill`
   - Set color to your accent color (e.g., blue, green)
   - Set **Image Type** to `Filled`
   - Set **Fill Method** to `Horizontal`
   - Set **Fill Origin** to `Left`
   - Stretch to fill parent (Anchor: Stretch-Stretch)
   - Set **Pixels Per Unit Multiplier** to `1`

5. **Create Progress Text**:
   - Right-click `LoadingPanel` → UI → Text - TextMeshPro
   - Name it `ProgressText`
   - Set text: `Loading... 0%`
   - Center it (Anchor: Middle-Center, Pos Y: `-100`)
   - Set font size: `24`
   - Set alignment: Center

6. **Create Status Text**:
   - Right-click `LoadingPanel` → UI → Text - TextMeshPro
   - Name it `StatusText`
   - Set text: `Initializing...`
   - Center it (Anchor: Middle-Center, Pos Y: `50`)
   - Set font size: `18`
   - Set alignment: Center

7. **Create Loading Spinner (Optional)**:
   - Right-click `LoadingPanel` → UI → Image
   - Name it `LoadingSpinner`
   - Add a spinning icon/sprite
   - Center it (Anchor: Middle-Center, Pos Y: `150`)
   - Set width/height: `64x64`

### Step 2: Add LoadingScreenManager Component

1. **Create Manager GameObject**:
   - Right-click Hierarchy → Create Empty
   - Name it `LoadingScreenManager`
   - Add Component → `LoadingScreenManager`

2. **Assign UI References**:
   - **Loading Canvas**: Drag `LoadingScreenCanvas`
   - **Loading Panel**: Drag `LoadingPanel`
   - **Progress Bar Fill**: Drag `ProgressBarFill`
   - **Progress Text**: Drag `ProgressText`
   - **Status Text**: Drag `StatusText`
   - **Loading Spinner**: Drag `LoadingSpinner` (if created)

3. **Configure Settings**:
   - **Show On Start**: ✅ Checked
   - **Progress Smooth Speed**: `2` (adjust for smoother/faster animation)

### Step 3: Test

1. **Play the game**
2. **You should see**:
   - Loading screen appears immediately
   - Progress bar animates from 0% to 100%
   - Status messages update during loading
   - Loading screen fades out when complete

## UI Hierarchy Structure

```
LoadingScreenCanvas (Canvas)
└── LoadingPanel (Panel - Black background)
    ├── ProgressBarBackground (Image - Dark gray)
    │   └── ProgressBarFill (Image - Accent color, Filled)
    ├── ProgressText (TextMeshPro - "Loading... 0%")
    ├── StatusText (TextMeshPro - "Initializing...")
    └── LoadingSpinner (Image - Optional spinning icon)
```

## Progress Stages

The loading screen automatically updates progress during bootstrap:

- **0-10%**: Initializing systems
- **10-30%**: Loading managers
- **30-70%**: Preloading assets (if enabled)
- **70-90%**: Preparing game
- **90-100%**: Loading initial scene
- **100%**: Complete (brief pause, then fade out)

## Customization

### Change Colors

1. **Background**: Select `LoadingPanel` → Image → Color
2. **Progress Bar**: Select `ProgressBarFill` → Image → Color
3. **Text**: Select text components → Color

### Change Fonts

1. Select text components
2. Change **Font Asset** in TextMeshPro settings
3. Adjust **Font Size** as needed

### Add Logo/Art

1. Add Image component to `LoadingPanel`
2. Assign your logo sprite
3. Position above progress bar

### Custom Status Messages

The status messages are automatically set by `BootstrapManager`, but you can customize them:

```csharp
// In BootstrapManager.cs, modify SetStatus() calls:
LoadingScreenManager.Instance.SetStatus("Your custom message");
```

## Advanced: Animated Spinner

To make the spinner rotate:

1. Create a script `LoadingSpinner.cs`:
```csharp
using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f;
    
    private void Update()
    {
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }
}
```

2. Add to `LoadingSpinner` GameObject
3. Set **Rotation Speed** (degrees per second)

## Troubleshooting

### Loading Screen Not Showing?
- Check that `LoadingScreenManager` component is in Bootstrap scene
- Verify `Show On Start` is checked
- Ensure Canvas is active and has high Sort Order

### Progress Bar Not Moving?
- Check that `ProgressBarFill` is assigned
- Verify `ProgressBarFill` has **Image Type** set to `Filled`
- Check that `Fill Method` is `Horizontal`

### Text Not Updating?
- Verify TextMeshPro components are assigned
- Check that text components are active
- Ensure font asset is assigned

### Loading Screen Stays Visible?
- Check that `BootstrapManager` calls `Hide()` after loading
- Verify scene transition completes successfully

## Integration with Bootstrap

The `BootstrapManager` automatically:
- Shows loading screen at start
- Updates progress during initialization
- Updates status messages
- Hides loading screen when complete

No additional code needed! Just set up the UI and assign references.

## Files Reference

- **LoadingScreenManager**: `Assets/Scripts/UI/LoadingScreenManager.cs`
- **BootstrapManager**: `Assets/Scripts/SceneManagement/BootstrapManager.cs` (updated)

## Next Steps

1. **Customize Design**: Match your game's art style
2. **Add Logo**: Include your game logo
3. **Add Tips**: Show loading tips during initialization
4. **Add Animations**: Animate UI elements for polish

