# Audio Setup (SFX + Volume Sliders)

This project includes a lightweight SFX system and global volume settings. You can wire this later without code changes.

## 1) Sound Events (SFX)

Create `SoundEvent` assets:
- Project window → Right‑click → Create → Dexiled → Audio → Sound Event
- Assign one or more clips
- Optional tuning: volume, volume variance, pitch, pitch variance, min interval, spatial blend, mixer group

`SoundEvent` assets are assigned to gameplay components via inspector fields (e.g., `EnemyCombatDisplay.attackSfx`).

## 2) SFX Playback

`SFXManager` plays `SoundEvent` clips with random pitch/volume variance:
- Auto‑creates itself on first use
- Pools AudioSources
- Respects SFX volume (see Volume Settings below)

## 2.5) Music Playback (Looping + Scene Tracks)

`MusicManager` supports looping tracks, crossfades, and scene‑based music.

How it works:
- Auto‑creates itself on first use
- Keeps playing across scene loads (`DontDestroyOnLoad`)
- Can auto‑switch based on scene name list
- Uses `MusicVolume` from `AudioSettingsManager`

Setup:
1. Create `MusicTrack` assets (Create → Dexiled → Audio → Music Track)
2. Assign `clip`, `volume`, and `loop`
3. Add a `MusicManager` in a bootstrap scene (optional)
4. Populate `sceneMusic` list with scene name → track entries

Manual use:
- `MusicManager.Instance.PlayTrack(track)`
- `MusicManager.Instance.Stop()`

## 3) Global Volume Settings

`AudioSettingsManager` stores three volumes:
- Master (applies to `AudioListener.volume`)
- SFX (multiplies `SFXManager` playback volume)
- Music (stored for future BGM system)

Settings are saved to `PlayerPrefs` and persist across sessions.

## 4) UI Sliders (Optional)

To add volume sliders later:
1. Create sliders for Master / SFX / Music
2. Add `AudioSettingsUI` to the settings panel
3. Assign the slider references in the inspector

## 5) Files

- `Assets/Scripts/Audio/SoundEvent.cs`
- `Assets/Scripts/Audio/SFXManager.cs`
- `Assets/Scripts/Audio/MusicTrack.cs`
- `Assets/Scripts/Audio/MusicManager.cs`
- `Assets/Scripts/Audio/AudioSettingsManager.cs`
- `Assets/Scripts/Audio/AudioSettingsUI.cs`

## 6) Notes

- No audio mixer is required, but `SoundEvent` supports an optional `AudioMixerGroup`.
- A dedicated Music system is not implemented yet; `MusicVolume` is stored for future use.
