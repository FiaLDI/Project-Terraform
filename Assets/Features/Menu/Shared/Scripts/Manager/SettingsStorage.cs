using UnityEngine;

public static class SettingsStorage
{
    private const string MASTER_VOL = "settings.masterVolume";
    private const string MUSIC_VOL  = "settings.musicVolume";
    private const string SFX_VOL    = "settings.sfxVolume";

    private const string RESOLUTION = "settings.resolutionIndex";
    private const string FULLSCREEN = "settings.fullscreen";
    private const string QUALITY    = "settings.quality";
    private const string VSYNC      = "settings.vsync";

    private const string SENS       = "settings.mouseSensitivity";

    // -------- PROPERTIES ----------
    public static float MasterVolume   { get => PlayerPrefs.GetFloat(MASTER_VOL, 1f); set => PlayerPrefs.SetFloat(MASTER_VOL, value); }
    public static float MusicVolume    { get => PlayerPrefs.GetFloat(MUSIC_VOL, 1f);  set => PlayerPrefs.SetFloat(MUSIC_VOL, value); }
    public static float SFXVolume      { get => PlayerPrefs.GetFloat(SFX_VOL, 1f);    set => PlayerPrefs.SetFloat(SFX_VOL, value); }

    public static int ResolutionIndex  { get => PlayerPrefs.GetInt(RESOLUTION, 0);    set => PlayerPrefs.SetInt(RESOLUTION, value); }
    public static bool Fullscreen      { get => PlayerPrefs.GetInt(FULLSCREEN, 1)==1; set => PlayerPrefs.SetInt(FULLSCREEN, value?1:0); }
    public static int Quality          { get => PlayerPrefs.GetInt(QUALITY, 2);       set => PlayerPrefs.SetInt(QUALITY, value); }
    public static bool VSync           { get => PlayerPrefs.GetInt(VSYNC, 1)==1;      set => PlayerPrefs.SetInt(VSYNC, value?1:0 ); }

    public static float Sensitivity    { get => PlayerPrefs.GetFloat(SENS, 1f);       set => PlayerPrefs.SetFloat(SENS, value); }

    // -------- SAVE / RESET ----------
    public static void Save() => PlayerPrefs.Save();

    public static void ResetToDefaults()
    {
        MasterVolume = 1f;
        MusicVolume  = 1f;
        SFXVolume    = 1f;

        Fullscreen   = true;
        VSync        = true;
        Quality      = 2;
        ResolutionIndex = 0;

        Sensitivity  = 1f;

        Save();
    }
}
