using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Окно со слотами сохранений. В режиме загрузки пустые слоты недоступны.
/// </summary>
public class SaveSlotsView
{
    public bool IsOpen => root.activeSelf;

    private readonly GameObject root;
    private readonly bool loadMode;
    private readonly Button[] buttons = new Button[SaveSystem.SlotCount];

    public SaveSlotsView(Transform parent, string title, bool loadMode, Action<int> onSlotChosen, Action onBack)
    {
        this.loadMode = loadMode;
        root = UIFactory.CreateOverlay(loadMode ? "LoadPanel" : "SavePanel", parent);

        Image panel = UIFactory.CreateImage("Window", root.transform, UIFactory.PanelColor);
        UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 620));

        RectTransform column = UIFactory.CreateColumn("Content", panel.transform, 24);
        UIFactory.Stretch(column, 50, 40, 50, 40);

        UIFactory.CreateSizedLabel(column, title, new Vector2(1000, 70), 48, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);

        for (int i = 0; i < SaveSystem.SlotCount; i++)
        {
            int slot = i + 1;
            buttons[i] = UIFactory.CreateButton(column, "", new Vector2(1000, 80), () => onSlotChosen?.Invoke(slot), 28);
        }

        UIFactory.CreateButton(column, "Назад", new Vector2(360, 76), () => onBack?.Invoke());
        root.SetActive(false);
    }

    public void Refresh()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int slot = i + 1;
            UIFactory.GetButtonLabel(buttons[i]).text = SaveSystem.Describe(slot);
            buttons[i].interactable = !loadMode || SaveSystem.Exists(slot);
        }
    }

    public void Show()
    {
        Refresh();
        root.SetActive(true);
        root.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
