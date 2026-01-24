using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;

    private bool isInitializing;

    private void OnEnable()
    {
        Initialize();
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void Initialize()
    {
        isInitializing = true;
        var settings = AudioSettingsManager.Instance;

        if (masterSlider != null)
            masterSlider.value = settings.MasterVolume;
        if (sfxSlider != null)
            sfxSlider.value = settings.SfxVolume;
        if (musicSlider != null)
            musicSlider.value = settings.MusicVolume;

        isInitializing = false;
    }

    private void Register()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(HandleMasterChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(HandleMusicChanged);
    }

    private void Unregister()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(HandleMasterChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(HandleMusicChanged);
    }

    private void HandleMasterChanged(float value)
    {
        if (isInitializing) return;
        AudioSettingsManager.Instance.SetMasterVolume(value);
    }

    private void HandleSfxChanged(float value)
    {
        if (isInitializing) return;
        AudioSettingsManager.Instance.SetSfxVolume(value);
    }

    private void HandleMusicChanged(float value)
    {
        if (isInitializing) return;
        AudioSettingsManager.Instance.SetMusicVolume(value);
    }
}
