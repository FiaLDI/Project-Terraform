using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public TMP_Dropdown qualityDropdown;
    public Toggle vsyncToggle;

    [Header("Controls")]
    public Slider sensitivitySlider;

    private void Start()
    {
        LoadSettings();
    }

    public void ApplySettings()
    {
        SettingsStorage.MasterVolume = masterSlider.value;
        SettingsStorage.MusicVolume  = musicSlider.value;
        SettingsStorage.SFXVolume    = sfxSlider.value;

        SettingsStorage.ResolutionIndex = resolutionDropdown.value;
        SettingsStorage.Fullscreen      = fullscreenToggle.isOn;
        SettingsStorage.Quality         = qualityDropdown.value;
        SettingsStorage.VSync           = vsyncToggle.isOn;

        SettingsStorage.Sensitivity     = sensitivitySlider.value;

        Debug.Log("<color=cyan>[SETTINGS] SAVED</color>");
    }

    private void LoadSettings()
    {
        masterSlider.value = SettingsStorage.MasterVolume;
        musicSlider.value  = SettingsStorage.MusicVolume;
        sfxSlider.value    = SettingsStorage.SFXVolume;

        resolutionDropdown.value = SettingsStorage.ResolutionIndex;
        fullscreenToggle.isOn    = SettingsStorage.Fullscreen;
        qualityDropdown.value    = SettingsStorage.Quality;
        vsyncToggle.isOn         = SettingsStorage.VSync;

        sensitivitySlider.value  = SettingsStorage.Sensitivity;
    }
}
