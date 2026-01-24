using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-75)]
public class MusicManager : MonoBehaviour
{
    [Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public MusicTrack track;
    }

    private static MusicManager instance;
    public static MusicManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MusicManager>();
                if (instance == null)
                {
                    var go = new GameObject("MusicManager");
                    instance = go.AddComponent<MusicManager>();
                }
            }
            return instance;
        }
    }

    [Header("Scene Music")]
    public List<SceneMusicEntry> sceneMusic = new List<SceneMusicEntry>();

    [Header("Playback")]
    [Range(0f, 2f)] public float fadeDuration = 0.75f;
    public bool dontDestroyOnLoad = true;

    private AudioSource activeSource;
    private AudioSource fadingSource;
    private MusicTrack currentTrack;

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

        activeSource = CreateSource("MusicSource_A");
        fadingSource = CreateSource("MusicSource_B");

        SceneManager.sceneLoaded += HandleSceneLoaded;

        var settings = AudioSettingsManager.Instance;
        if (settings != null)
            settings.OnVolumesChanged += HandleVolumesChanged;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            var settings = AudioSettingsManager.Instance;
            if (settings != null)
                settings.OnVolumesChanged -= HandleVolumesChanged;
        }
    }

    private AudioSource CreateSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        return source;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrWhiteSpace(scene.name))
            return;

        foreach (var entry in sceneMusic)
        {
            if (entry == null || entry.track == null)
                continue;
            if (string.Equals(entry.sceneName, scene.name, StringComparison.OrdinalIgnoreCase))
            {
                PlayTrack(entry.track);
                return;
            }
        }
    }

    private void HandleVolumesChanged(float master, float sfx, float music)
    {
        UpdateSourceVolumes(music);
    }

    public void PlayTrack(MusicTrack track)
    {
        if (track == null || track.clip == null)
            return;
        if (currentTrack == track)
            return;

        currentTrack = track;
        StartCrossfade(track);
    }

    public void Stop(float duration = 0.5f)
    {
        currentTrack = null;
        StartCrossfade(null, duration);
    }

    private void StartCrossfade(MusicTrack nextTrack, float? overrideFade = null)
    {
        float fade = overrideFade ?? fadeDuration;
        if (fade <= 0f)
        {
            ApplyImmediate(nextTrack);
            return;
        }

        var nextSource = fadingSource;
        var oldSource = activeSource;

        if (nextTrack != null)
        {
            nextSource.clip = nextTrack.clip;
            nextSource.loop = nextTrack.loop;
            nextSource.volume = 0f;
            nextSource.Play();
        }

        StartCoroutine(FadeRoutine(oldSource, nextSource, nextTrack, fade));
        activeSource = nextSource;
        fadingSource = oldSource;
    }

    private System.Collections.IEnumerator FadeRoutine(AudioSource from, AudioSource to, MusicTrack nextTrack, float duration)
    {
        float musicVolume = AudioSettingsManager.Instance != null ? AudioSettingsManager.Instance.MusicVolume : 1f;
        float targetVolume = nextTrack != null ? nextTrack.volume * musicVolume : 0f;

        float elapsed = 0f;
        float startFrom = from != null ? from.volume : 0f;
        float startTo = to != null ? to.volume : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (from != null)
                from.volume = Mathf.Lerp(startFrom, 0f, t);
            if (to != null)
                to.volume = Mathf.Lerp(startTo, targetVolume, t);

            yield return null;
        }

        if (from != null)
        {
            from.Stop();
            from.clip = null;
            from.volume = 0f;
        }

        if (to != null)
            to.volume = targetVolume;
    }

    private void ApplyImmediate(MusicTrack track)
    {
        float musicVolume = AudioSettingsManager.Instance != null ? AudioSettingsManager.Instance.MusicVolume : 1f;

        activeSource.Stop();
        activeSource.clip = track != null ? track.clip : null;
        activeSource.loop = track != null ? track.loop : true;
        activeSource.volume = track != null ? track.volume * musicVolume : 0f;

        if (track != null && track.clip != null)
            activeSource.Play();

        fadingSource.Stop();
        fadingSource.clip = null;
        fadingSource.volume = 0f;
    }

    private void UpdateSourceVolumes(float musicVolume)
    {
        if (activeSource != null && currentTrack != null)
            activeSource.volume = currentTrack.volume * musicVolume;
        if (fadingSource != null && fadingSource.isPlaying && currentTrack != null)
            fadingSource.volume = Mathf.Min(fadingSource.volume, currentTrack.volume * musicVolume);
    }

    public void PlayEncounterTrack(MusicTrack track)
    {
        PlayTrack(track);
    }
}
