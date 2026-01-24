using System;
using UnityEngine;

public class AudioSettingsManager : MonoBehaviour
{
    private static AudioSettingsManager instance;
    public static AudioSettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioSettingsManager>();
                if (instance == null)
                {
                    var go = new GameObject("AudioSettingsManager");
                    instance = go.AddComponent<AudioSettingsManager>();
                }
            }
            return instance;
        }
    }

    public event Action<float, float, float> OnVolumesChanged;

    private const string MasterKey = "Audio_MasterVolume";
    private const string SfxKey = "Audio_SfxVolume";
    private const string MusicKey = "Audio_MusicVolume";

    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
        Apply();
    }

    public float MasterVolume => masterVolume;
    public float SfxVolume => sfxVolume;
    public float MusicVolume => musicVolume;

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        Apply();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        Apply();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        Apply();
    }

    private void Apply()
    {
        AudioListener.volume = masterVolume;
        Save();
        OnVolumesChanged?.Invoke(masterVolume, sfxVolume, musicVolume);
    }

    private void Load()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterKey, masterVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxKey, sfxVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicKey, musicVolume);
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(MasterKey, masterVolume);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.Save();
    }
}
