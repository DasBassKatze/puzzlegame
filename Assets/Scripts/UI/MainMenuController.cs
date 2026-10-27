using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Главное меню: запуск игры, выбор уровня и сложности, своя картинка, загрузка, настройки, выход.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    private Transform canvasRoot;
    private GameObject mainPanel;
    private GameObject levelPanel;
    private GameObject customPanel;
    private SettingsView settingsView;
    private SaveSlotsView loadView;
    private ToastMessage toast;

    private Image customPreview;
    private string customImagePath;

    private readonly List<Button> difficultyButtons = new List<Button>();
    private readonly List<Text> bestLabels = new List<Text>();

    private int selectedGrid = 3;

    private void Awake()
    {
        SettingsManager.Load();
        UIFactory.EnsureEventSystem();
        selectedGrid = GameSession.GridSize;

        Canvas canvas = UIFactory.CreateCanvas("MenuCanvas", 0);
        canvasRoot = canvas.transform;
        Image background = UIFactory.CreateImage("Background", canvasRoot, UIFactory.Background);
        UIFactory.Stretch(background.rectTransform);

        BuildMainPanel();
        BuildLevelPanel();
        BuildCustomPanel();
        settingsView = new SettingsView(canvasRoot, () => ShowPanel(mainPanel));
        loadView = new SaveSlotsView(canvasRoot, "Загрузить игру", true, OnLoadSlot, () => ShowPanel(mainPanel));
        toast = ToastMessage.Create(canvasRoot);

        ShowPanel(mainPanel);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;
        if (!mainPanel.activeSelf || settingsView.IsOpen || loadView.IsOpen)
            ShowPanel(mainPanel);
    }

    private void ShowPanel(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        levelPanel.SetActive(panel == levelPanel);
        customPanel.SetActive(panel == customPanel);
        settingsView.Hide();
        loadView.Hide();
    }

    private void BuildMainPanel()
    {
        mainPanel = UIFactory.CreateRect("MainPanel", canvasRoot).gameObject;
        UIFactory.Stretch((RectTransform)mainPanel.transform);

        RectTransform column = UIFactory.CreateColumn("Buttons", mainPanel.transform, 24);
        UIFactory.Stretch(column);

        UIFactory.CreateSizedLabel(column, "ПАЗЛ", new Vector2(900, 150), 120, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFactory.CreateSizedLabel(column, "Собери картинку из кусочков", new Vector2(900, 60), 34, UIFactory.MutedText);
        UIFactory.CreateSizedLabel(column, "", new Vector2(10, 20), 10, UIFactory.TextColor);

        var size = new Vector2(440, 84);
        UIFactory.CreateButton(column, "Играть", size, () => { RefreshDifficulty(); ShowPanel(levelPanel); }, 34);
        UIFactory.CreateButton(column, "Своя картинка", size, OnCustomImage, 34);
        UIFactory.CreateButton(column, "Загрузить", size, () => loadView.Show(), 34);
        UIFactory.CreateButton(column, "Настройки", size, () => settingsView.Show(), 34);
        UIFactory.CreateButton(column, "Выход", size, Quit, 34);
    }

    private void BuildLevelPanel()
    {
        levelPanel = UIFactory.CreateRect("LevelPanel", canvasRoot).gameObject;
        UIFactory.Stretch((RectTransform)levelPanel.transform);

        Text title = UIFactory.CreateLabel(levelPanel.transform, "Выбор уровня", 56, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(1000, 90));

        RectTransform difficulty = CreateDifficultyRow(levelPanel.transform);
        UIFactory.Place(difficulty, new Vector2(0.5f, 1f), new Vector2(0, -180), new Vector2(1100, 70));

        RectTransform grid = UIFactory.CreateRect("Levels", levelPanel.transform);
        UIFactory.Place(grid, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1200, 680));
        var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(360, 320);
        layout.spacing = new Vector2(30, 30);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.childAlignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < LevelDatabase.Levels.Length; i++)
            CreateLevelTile(grid, i);

        Button back = UIFactory.CreateButton(levelPanel.transform, "Назад", new Vector2(360, 76), () => ShowPanel(mainPanel));
        UIFactory.Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(360, 76));
    }

    private RectTransform CreateDifficultyRow(Transform parent)
    {
        RectTransform row = UIFactory.CreateRow("Difficulty", parent, 20);
        for (int i = 0; i < LevelDatabase.GridSizes.Length; i++)
        {
            int grid = LevelDatabase.GridSizes[i];
            Button button = UIFactory.CreateButton(row, LevelDatabase.GridNames[i], new Vector2(340, 70), () =>
            {
                selectedGrid = grid;
                RefreshDifficulty();
            });
            difficultyButtons.Add(button);
        }
        return row;
    }

    private void CreateLevelTile(Transform parent, int levelId)
    {
        Texture2D texture = LevelDatabase.GetTexture(levelId);

        Button tile = UIFactory.CreateButton(parent, "", Vector2.zero, () => StartLevel(levelId));
        UIFactory.SetButtonColor(tile, UIFactory.PanelColor);
        Destroy(UIFactory.GetButtonLabel(tile).gameObject);

        Image thumbnail = UIFactory.CreateImage("Thumbnail", tile.transform, Color.white);
        UIFactory.Place(thumbnail.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -120), new Vector2(330, 220));
        thumbnail.preserveAspect = true;
        thumbnail.raycastTarget = false;
        if (texture != null)
            thumbnail.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Text name = UIFactory.CreateLabel(tile.transform, LevelDatabase.Levels[levelId].Name, 30, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFactory.Place(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 60), new Vector2(340, 40));

        Text best = UIFactory.CreateLabel(tile.transform, "", 24, UIFactory.MutedText);
        UIFactory.Place(best.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 24), new Vector2(340, 32));
        bestLabels.Add(best);
    }

    private void RefreshDifficulty()
    {
        for (int i = 0; i < difficultyButtons.Count; i++)
        {
            bool selected = LevelDatabase.GridSizes[i % LevelDatabase.GridSizes.Length] == selectedGrid;
            UIFactory.SetButtonColor(difficultyButtons[i], selected ? UIFactory.AccentDark : UIFactory.ButtonColor);
        }
        for (int i = 0; i < bestLabels.Count; i++)
        {
            int best = ScoreSystem.GetBest($"best_{i}_{selectedGrid}");
            bestLabels[i].text = best > 0 ? $"Рекорд: {best}" : "Рекорда пока нет";
        }
    }

    private void BuildCustomPanel()
    {
        customPanel = UIFactory.CreateRect("CustomPanel", canvasRoot).gameObject;
        UIFactory.Stretch((RectTransform)customPanel.transform);

        Text title = UIFactory.CreateLabel(customPanel.transform, "Своя картинка", 56, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(1000, 90));

        RectTransform difficulty = CreateDifficultyRow(customPanel.transform);
        UIFactory.Place(difficulty, new Vector2(0.5f, 1f), new Vector2(0, -180), new Vector2(1100, 70));

        customPreview = UIFactory.CreateImage("Preview", customPanel.transform, Color.white);
        UIFactory.Place(customPreview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(900, 560));
        customPreview.preserveAspect = true;

        RectTransform buttons = UIFactory.CreateRow("Buttons", customPanel.transform, 30);
        UIFactory.Place(buttons, new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(1200, 80));
        UIFactory.CreateButton(buttons, "Назад", new Vector2(300, 76), () => ShowPanel(mainPanel));
        UIFactory.CreateButton(buttons, "Другой файл", new Vector2(300, 76), OnCustomImage);
        UIFactory.CreateButton(buttons, "Начать", new Vector2(300, 76), StartCustom);
    }

    private void OnCustomImage()
    {
        string path = ImageLoader.PickImage();
        if (path == null)
        {
            toast.Show("Файл не выбран");
            return;
        }

        Texture2D texture = ImageLoader.LoadTexture(path);
        if (texture == null)
        {
            toast.Show("Не удалось открыть изображение. Выберите файл PNG или JPG");
            return;
        }

        customImagePath = path;
        customPreview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        RefreshDifficulty();
        ShowPanel(customPanel);
    }

    private void StartLevel(int levelId)
    {
        GameSession.StartLevel(levelId, selectedGrid);
        SceneLoader.Load(SceneLoader.GameScene);
    }

    private void StartCustom()
    {
        GameSession.StartCustom(customImagePath, selectedGrid);
        SceneLoader.Load(SceneLoader.GameScene);
    }

    private void OnLoadSlot(int slot)
    {
        SaveData data = SaveSystem.Load(slot);
        if (data == null)
        {
            toast.Show("Сохранение повреждено или отсутствует");
            return;
        }
        GameSession.StartFromSave(data);
        SceneLoader.Load(SceneLoader.GameScene);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
