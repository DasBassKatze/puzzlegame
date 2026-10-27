using System;
using UnityEngine;

/// <summary>
/// Настройки игры. Хранятся между запусками в PlayerPrefs.
/// </summary>
public static class SettingsManager
{
    private const string VolumeKey = "settings_volume";
    private const string FullscreenKey = "settings_fullscreen";
    private const string ResolutionKey = "settings_resolution";
    private const string GridKey = "settings_grid";

    public static readonly Vector2Int[] Resolutions =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
    };

    public static float Volume { get; private set; } = 0.8f;
    public static bool Fullscreen { get; private set; } = true;
    public static int ResolutionIndex { get; private set; } = 3;
    public static bool ShowGrid { get; private set; } = true;

    /// <summary>Вызывается при изменении настроек, влияющих на игровое поле.</summary>
    public static event Action Changed;

    private static bool loaded;

    public static void Load()
    {
        if (loaded)
            return;
        loaded = true;

        Volume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        ResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt(ResolutionKey, 3), 0, Resolutions.Length - 1);
        ShowGrid = PlayerPrefs.GetInt(GridKey, 1) == 1;

        AudioListener.volume = Volume;
        if (PlayerPrefs.HasKey(ResolutionKey))
            ApplyScreen();
    }

    public static void SetVolume(float value)
    {
        Volume = Mathf.Clamp01(value);
        AudioListener.volume = Volume;
        PlayerPrefs.SetFloat(VolumeKey, Volume);
        PlayerPrefs.Save();
    }

    public static void SetFullscreen(bool value)
    {
        Fullscreen = value;
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        PlayerPrefs.SetInt(ResolutionKey, ResolutionIndex);
        PlayerPrefs.Save();
        ApplyScreen();
    }

    public static void SetResolution(int index)
    {
        ResolutionIndex = (index + Resolutions.Length) % Resolutions.Length;
        PlayerPrefs.SetInt(ResolutionKey, ResolutionIndex);
        PlayerPrefs.Save();
        ApplyScreen();
    }

    public static void SetShowGrid(bool value)
    {
        ShowGrid = value;
        PlayerPrefs.SetInt(GridKey, value ? 1 : 0);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static string ResolutionName(int index)
    {
        Vector2Int r = Resolutions[index];
        return $"{r.x}×{r.y}";
    }

    private static void ApplyScreen()
    {
#if !UNITY_EDITOR
        Vector2Int r = Resolutions[ResolutionIndex];
        Screen.SetResolution(r.x, r.y, Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
#endif
    }
}
