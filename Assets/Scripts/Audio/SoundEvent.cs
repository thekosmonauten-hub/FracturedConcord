using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SoundEvent", menuName = "Dexiled/Audio/Sound Event")]
public class SoundEvent : ScriptableObject
{
    [Header("Clips")]
    public AudioClip[] clips;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float volumeVariance = 0.1f;

    [Header("Pitch")]
    [Range(-3f, 3f)] public float pitch = 1f;
    [Range(0f, 1f)] public float pitchVariance = 0.08f;

    [Header("Playback")]
    [Tooltip("Minimum seconds between plays for this event.")]
    [Min(0f)] public float minInterval = 0f;
    [Range(0f, 1f)] public float spatialBlend = 0f;
    public AudioMixerGroup outputMixerGroup;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;
        int index = Random.Range(0, clips.Length);
        return clips[index];
    }

    public float GetRandomVolume()
    {
        float variance = Random.Range(-volumeVariance, volumeVariance);
        return Mathf.Clamp01(volume + variance);
    }

    public float GetRandomPitch()
    {
        float variance = Random.Range(-pitchVariance, pitchVariance);
        return Mathf.Clamp(pitch + variance, -3f, 3f);
    }
}
