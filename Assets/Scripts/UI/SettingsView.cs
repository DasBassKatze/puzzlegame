using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Окно настроек. Одно и то же окно используется в главном меню и в меню паузы.
/// </summary>
public class SettingsView
{
    public bool IsOpen => root.activeSelf;

    private readonly GameObject root;
    private readonly Slider volumeSlider;
    private readonly Toggle fullscreenToggle;
    private readonly Toggle gridToggle;
    private readonly Text resolutionLabel;

    public SettingsView(Transform parent, Action onBack)
    {
        root = UIFactory.CreateOverlay("SettingsPanel", parent);

        Image panel = UIFactory.CreateImage("Window", root.transform, UIFactory.PanelColor);
        UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 700));

        RectTransform column = UIFactory.CreateColumn("Content", panel.transform, 28);
        UIFactory.Stretch(column, 60, 40, 60, 40);

        UIFactory.CreateSizedLabel(column, "Настройки", new Vector2(700, 70), 48, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);

        RectTransform volumeRow = UIFactory.CreateRow("Volume", column, 20);
        volumeRow.sizeDelta = new Vector2(700, 60);
        UIFactory.CreateSizedLabel(volumeRow, "Громкость", new Vector2(240, 60), 30, UIFactory.TextColor, TextAnchor.MiddleLeft);
        volumeSlider = UIFactory.CreateSlider(volumeRow, new Vector2(440, 50), SettingsManager.Volume, SettingsManager.SetVolume);

        RectTransform resolutionRow = UIFactory.CreateRow("Resolution", column, 20);
        resolutionRow.sizeDelta = new Vector2(700, 64);
        UIFactory.CreateSizedLabel(resolutionRow, "Разрешение", new Vector2(240, 64), 30, UIFactory.TextColor, TextAnchor.MiddleLeft);
        UIFactory.CreateButton(resolutionRow, "<", new Vector2(64, 64), () => ChangeResolution(-1));
        resolutionLabel = UIFactory.CreateSizedLabel(resolutionRow, "", new Vector2(272, 64), 30, UIFactory.TextColor);
        UIFactory.CreateButton(resolutionRow, ">", new Vector2(64, 64), () => ChangeResolution(1));

        fullscreenToggle = UIFactory.CreateToggle(column, "Полноэкранный режим", new Vector2(700, 56),
            SettingsManager.Fullscreen, SettingsManager.SetFullscreen);
        gridToggle = UIFactory.CreateToggle(column, "Показывать сетку на поле", new Vector2(700, 56),
            SettingsManager.ShowGrid, SettingsManager.SetShowGrid);

        UIFactory.CreateButton(column, "Назад", new Vector2(360, 76), () => onBack?.Invoke());

        root.SetActive(false);
    }

    private void ChangeResolution(int step)
    {
        SettingsManager.SetResolution(SettingsManager.ResolutionIndex + step);
        resolutionLabel.text = SettingsManager.ResolutionName(SettingsManager.ResolutionIndex);
    }

    public void Show()
    {
        volumeSlider.SetValueWithoutNotify(SettingsManager.Volume);
        fullscreenToggle.SetIsOnWithoutNotify(SettingsManager.Fullscreen);
        gridToggle.SetIsOnWithoutNotify(SettingsManager.ShowGrid);
        resolutionLabel.text = SettingsManager.ResolutionName(SettingsManager.ResolutionIndex);
        root.SetActive(true);
        root.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
