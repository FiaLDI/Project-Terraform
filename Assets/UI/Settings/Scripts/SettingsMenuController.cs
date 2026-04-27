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

        List<Resolution> uniqueResolutions = new List<Resolution>();
        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string key = resolutions[i].width + "x" + resolutions[i].height;

            if (seen.Add(key))
            {
                uniqueResolutions.Add(resolutions[i]);
            }
        }

        uniqueResolutions.Sort((a, b) =>
            (b.width * b.height).CompareTo(a.width * a.height));

        resolutions = uniqueResolutions.ToArray();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            int w = resolutions[i].width;
            int h = resolutions[i].height;

            string aspect = GetAspectRatio(w, h);

            bool isCurrent =
                w == Screen.currentResolution.width &&
                h == Screen.currentResolution.height;

            string label = $"{w} x {h} ({aspect})";

            if (isCurrent)
            {
                label += " <color=#00FFAA>(Current)</color>";
            }

            options.Add(label);
        }

        resolutionDropdown.AddOptions(options);

        if (SettingsStorage.ResolutionIndex < 0 || SettingsStorage.ResolutionIndex >= resolutions.Length)
        {
            SettingsStorage.ResolutionIndex = GetCurrentResolutionIndex();
        }

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

    private string GetAspectRatio(int width, int height)
    {
        int gcd = GCD(width, height);
        int w = width / gcd;
        int h = height / gcd;

        return $"{w}:{h}";
    }

    private int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
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
        musicSlider.value = SettingsStorage.MusicVolume;
        sfxSlider.value = SettingsStorage.SFXVolume;

        if (SettingsStorage.ResolutionIndex < 0 || SettingsStorage.ResolutionIndex >= resolutions.Length)
        {
            SettingsStorage.ResolutionIndex = GetCurrentResolutionIndex();
        }

        resolutionDropdown.value = SettingsStorage.ResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        screenModeDropdown.value = SettingsStorage.ScreenMode;
        qualityDropdown.value = SettingsStorage.Quality;
        vsyncToggle.isOn = SettingsStorage.VSync;

        sensitivitySlider.value = SettingsStorage.Sensitivity;
    }

    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }

        return 0;
    }

    public void LoadSettingsUI() {
        LoadSettings();
    }
}
