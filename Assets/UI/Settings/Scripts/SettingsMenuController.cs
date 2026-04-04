using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsMenuController : MonoBehaviour
{
    private Resolution[] resolutions;
    [Header("Audio")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle vsyncToggle;

    [Header("Controls")]
    public Slider sensitivitySlider;

    private void Start()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        resolutionDropdown.value = SettingsStorage.ResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string>
        {
            "Exclusive Fullscreen",
            "Borderless",
            "Windowed"
        });
        LoadSettings();
    }

    public void ApplySettings()
    {
        SettingsStorage.MasterVolume = masterSlider.value;
        SettingsStorage.MusicVolume  = musicSlider.value;
        SettingsStorage.SFXVolume    = sfxSlider.value;

        SettingsStorage.ResolutionIndex = resolutionDropdown.value;
        SettingsStorage.ScreenMode      = screenModeDropdown.value;
        SettingsStorage.Quality         = qualityDropdown.value;
        SettingsStorage.VSync           = vsyncToggle.isOn;

        SettingsStorage.Sensitivity     = sensitivitySlider.value;

        ApplyGraphics();

        Debug.Log("<color=cyan>[SETTINGS] APPLIED</color>");
    }

    private void ApplyGraphics()
    {
        Resolution res = resolutions[SettingsStorage.ResolutionIndex];

        FullScreenMode mode = FullScreenMode.FullScreenWindow;

        switch (SettingsStorage.ScreenMode)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.FullScreenWindow; break;
            case 2: mode = FullScreenMode.Windowed; break;
        }

        Screen.SetResolution(res.width, res.height, mode);

        QualitySettings.SetQualityLevel(SettingsStorage.Quality);

        QualitySettings.vSyncCount = SettingsStorage.VSync ? 1 : 0;
    }

    private void LoadSettings()
    {
        masterSlider.value = SettingsStorage.MasterVolume;
        musicSlider.value  = SettingsStorage.MusicVolume;
        sfxSlider.value    = SettingsStorage.SFXVolume;

        resolutionDropdown.value = SettingsStorage.ResolutionIndex;
        screenModeDropdown.value = SettingsStorage.ScreenMode;

        qualityDropdown.value = SettingsStorage.Quality;
        vsyncToggle.isOn      = SettingsStorage.VSync;

        sensitivitySlider.value = SettingsStorage.Sensitivity;
    }

    public void LoadSettingsUI() {
        LoadSettings();
    }
}
