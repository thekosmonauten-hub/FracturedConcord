Unity Scene Loading – Best Practices Guide
Core Principle

Scenes should load data, not logic.
Game flow should persist across scenes.

Long load times almost always come from doing too much work inside scenes.

1. Split Your Game into Persistent vs Swappable Layers
❌ Common Anti-Pattern

Each scene:

Initializes systems

Loads assets

Builds UI

Registers events

Result:

Long load times

Memory spikes

State bugs

✅ Correct Pattern: Bootstrap + Additive Scenes
1️⃣ Bootstrap Scene (Loads Once)

This scene is loaded at game start and never unloaded.

Contains:

GameManager

SaveManager

AudioManager

Card Database

Asset References

Action Queue

Player State

Bootstrap (DontDestroyOnLoad)

2️⃣ Content Scenes (Loaded Additively)

Examples:

Combat

Inventory

Vendor

Ascendancy

Map

Bootstrap
+ CombatScene
+ UIScene


Only these get loaded/unloaded.

2. Use Additive Scene Loading (Always)
❌ Avoid
SceneManager.LoadScene("Combat");


This blocks the main thread.

✅ Use
SceneManager.LoadSceneAsync("Combat", LoadSceneMode.Additive);


Then explicitly unload:

SceneManager.UnloadSceneAsync("Inventory");

3. Loading Pipeline (The Right Way)
Recommended Flow
Fade Out
→ Load Scene Async (Additive)
→ Wait for activation
→ Inject Data
→ Initialize Scene
→ Fade In
→ Unload Old Scene

Example Loader
IEnumerator LoadScene(string sceneName)
{
    yield return FadeOut();

    var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    op.allowSceneActivation = false;

    while (op.progress < 0.9f)
        yield return null;

    InjectGameState(sceneName);

    op.allowSceneActivation = true;
    yield return op;

    yield return FadeIn();
}

4. Separate “Loading” from “Initialization”
❌ Wrong

Load scene

Instantiate enemies

Generate deck

Build UI

Load audio
All in Start() or Awake()

✅ Correct Pattern
Scene Lifecycle Interface
public interface ISceneInitializable
{
    IEnumerator Initialize();
}


Loader calls:

yield return sceneInitializer.Initialize();


This:

Allows progress bars

Avoids frame spikes

Makes scene load deterministic

5. Asset Loading Strategy (Huge Performance Gain)
Use Addressables or Explicit Preloads
❌ Bad

Load textures, sprites, VFX in Start()

Rely on Resources folder

✅ Good

Preload assets during transitions

Keep references in a central database

await Addressables.LoadAssetAsync<GameObject>("CardFX");

Card Games Tip

Preload all card art at startup
They’re reused constantly.

6. UI Should Be Its Own Scene
Why?

UI is expensive

UI rebuilds cause stalls

UI logic shouldn’t reset

Structure
Bootstrap
+ UIScene (persistent)
+ ContentScene (swaps)


UI listens to state changes — not scenes.

7. Loading Screens Should Do Real Work
❌ Fake Loading Bars
progress += Time.deltaTime;

✅ Real Progress

Scene loading progress

Asset load progress

Initialization steps

Example:

progress = (sceneLoad + assetLoad + initSteps) / totalSteps;

8. Avoid Heavy Work in Awake / Start
Bad Operations to Avoid in Scene Scripts

LINQ allocations

File IO

JSON parsing

Massive instantiation

Card generation

Enemy stat calculation

Move these to:

Bootstrap preload

Async coroutines

Background threads (if safe)

9. Object Pool Everything

Especially in:

Combat scenes

Card visuals

Status effects

Damage numbers

Pooling reduces:

GC spikes

Scene load time

Frame drops

10. Scene Size Guidelines
Ideal Scene Contents

Empty GameObjects

Markers

Light logic

References

Not Ideal

200+ prefabs

All enemies pre-placed

Every VFX preloaded

11. Debugging Load Time (Must Do)
Enable Unity Profiler

Watch for:

GC Alloc

Instantiate

Serialization

Asset loading

Red Flags

200ms+ spikes during load

Repeated allocations per transition


12. Golden Rule

A scene should never decide what the game state is.
It should only display and interact with the state.

13. Final Checklist

✔ Additive scene loading
✔ Persistent bootstrap scene
✔ Async initialization
✔ No heavy logic in Awake/Start
✔ Asset preloading
✔ UI separated
✔ Object pooling
✔ Real loading feedback