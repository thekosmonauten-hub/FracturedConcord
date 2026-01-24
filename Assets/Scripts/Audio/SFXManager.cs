using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-80)]
public class SFXManager : MonoBehaviour
{
    private static SFXManager instance;
    public static SFXManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SFXManager>();
                if (instance == null)
                {
                    var go = new GameObject("SFXManager");
                    instance = go.AddComponent<SFXManager>();
                }
            }
            return instance;
        }
    }

    [Header("Pool Settings")]
    [Min(1)] public int initialPoolSize = 8;
    public bool dontDestroyOnLoad = true;

    private readonly List<AudioSource> pool = new List<AudioSource>();
    private readonly Dictionary<SoundEvent, float> lastPlayTimes = new Dictionary<SoundEvent, float>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        WarmPool();
    }

    private void WarmPool()
    {
        for (int i = pool.Count; i < initialPoolSize; i++)
        {
            pool.Add(CreateSource());
        }
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject("SFXSource");
        go.transform.SetParent(transform, false);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        return source;
    }

    private AudioSource GetSource()
    {
        foreach (var source in pool)
        {
            if (!source.isPlaying)
                return source;
        }
        var created = CreateSource();
        pool.Add(created);
        return created;
    }

    public void Play(SoundEvent soundEvent, Vector3? position = null)
    {
        if (soundEvent == null)
            return;

        if (soundEvent.minInterval > 0f &&
            lastPlayTimes.TryGetValue(soundEvent, out float lastTime) &&
            Time.time - lastTime < soundEvent.minInterval)
        {
            return;
        }

        var clip = soundEvent.GetRandomClip();
        if (clip == null)
            return;

        var source = GetSource();
        source.clip = clip;
        float sfxMultiplier = AudioSettingsManager.Instance != null ? AudioSettingsManager.Instance.SfxVolume : 1f;
        source.volume = soundEvent.GetRandomVolume() * sfxMultiplier;
        source.pitch = soundEvent.GetRandomPitch();
        source.spatialBlend = soundEvent.spatialBlend;
        source.outputAudioMixerGroup = soundEvent.outputMixerGroup;

        if (position.HasValue)
            source.transform.position = position.Value;

        source.Play();
        lastPlayTimes[soundEvent] = Time.time;

        StartCoroutine(ReleaseAfterPlay(source, clip.length, source.pitch));
    }

    private IEnumerator ReleaseAfterPlay(AudioSource source, float clipLength, float pitch)
    {
        float duration = clipLength / Mathf.Max(0.01f, Mathf.Abs(pitch));
        yield return new WaitForSeconds(duration);
        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }
    }
}
