using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Управление игровой сценой: строит интерфейс и поле, ведёт таймер и очки,
/// обрабатывает паузу, подсказки, сохранение и окончание игры.
/// </summary>
public class GameController : MonoBehaviour
{
    private const float TopBarHeight = 100f;

    private static readonly Rect BoardArea = new Rect(-860f, -430f, 860f, 860f);
    private static readonly Rect TrayArea = new Rect(20f, -450f, 900f, 900f);

    private readonly PuzzleTimer timer = new PuzzleTimer();
    private readonly ScoreSystem score = new ScoreSystem();

    private PuzzleBoard board;
    private HintSystem hints;
    private Transform canvasRoot;
    private Text timeText;
    private Text scoreText;
    private GameObject pausePanel;
    private GameObject resultPanel;
    private Text resultText;
    private SettingsView settingsView;
    private SaveSlotsView saveView;
    private ToastMessage toast;

    private bool paused;
    private bool finished;

    private void Start()
    {
        SettingsManager.Load();
        UIFactory.EnsureEventSystem();

        Texture2D texture = GameSession.IsCustom
            ? ImageLoader.LoadTexture(GameSession.CustomImagePath)
            : LevelDatabase.GetTexture(GameSession.LevelId);
        if (texture == null)
        {
            Debug.LogWarning("Изображение уровня не найдено, возврат в меню");
            SceneLoader.Load(SceneLoader.MenuScene);
            return;
        }

        BuildInterface(texture);
        hints = new HintSystem(board, score, this);

        board.PiecePlaced += OnPiecePlaced;
        board.Mistake += OnMistake;
        board.Completed += OnCompleted;
        SettingsManager.Changed += OnSettingsChanged;

        SaveData save = GameSession.PendingLoad;
        GameSession.PendingLoad = null;
        if (save != null)
        {
            timer.Elapsed = save.elapsed;
            score.Restore(save.score, save.mistakes, save.hintsUsed);
            board.ApplyState(save.pieces);
        }
        else
        {
            board.Shuffle();
        }

        if (!finished)
            timer.Running = true;
        UpdateHud();
    }

    private void OnDestroy()
    {
        SettingsManager.Changed -= OnSettingsChanged;
    }

    private void Update()
    {
        if (board == null)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
            HandleEscape();
        timer.Tick(Time.deltaTime);
        UpdateHud();
    }

    private void UpdateHud()
    {
        timeText.text = "Время: " + PuzzleTimer.Format(timer.Elapsed);
        scoreText.text = "Очки: " + score.Score;
    }

    // ---------- Интерфейс ----------

    private void BuildInterface(Texture2D texture)
    {
        Canvas canvas = UIFactory.CreateCanvas("GameCanvas", 0);
        canvasRoot = canvas.transform;
        Image background = UIFactory.CreateImage("Background", canvasRoot, UIFactory.Background);
        UIFactory.Stretch(background.rectTransform);

        BuildTopBar();

        RectTransform playArea = UIFactory.CreateRect("PlayArea", canvasRoot);
        UIFactory.Stretch(playArea, 0, 0, 0, TopBarHeight);

        Image tray = UIFactory.CreateImage("Tray", playArea, UIFactory.PanelColor);
        UIFactory.Place(tray.rectTransform, new Vector2(0.5f, 0.5f), TrayArea.center, TrayArea.size);
        tray.raycastTarget = false;

        board = playArea.gameObject.AddComponent<PuzzleBoard>();
        board.Build(playArea, texture, GameSession.GridSize, BoardArea, TrayArea);

        BuildPausePanel();
        settingsView = new SettingsView(canvasRoot, ShowPauseMenu);
        saveView = new SaveSlotsView(canvasRoot, "Сохранить игру", false, SaveToSlot, ShowPauseMenu);
        BuildResultPanel();
        toast = ToastMessage.Create(canvasRoot);
    }

    private void BuildTopBar()
    {
        Image bar = UIFactory.CreateImage("TopBar", canvasRoot, UIFactory.PanelColor);
        RectTransform rt = bar.rectTransform;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, TopBarHeight);

        string title = $"{GameSession.LevelName}  {GameSession.GridSize}×{GameSession.GridSize}";
        Text titleText = UIFactory.CreateLabel(bar.transform, title, 34, UIFactory.TextColor, TextAnchor.MiddleLeft, FontStyle.Bold);
        SetHorizontalBand(titleText.rectTransform, 0f, 0.28f, 40);

        timeText = UIFactory.CreateLabel(bar.transform, "", 34, UIFactory.TextColor, TextAnchor.MiddleLeft);
        SetHorizontalBand(timeText.rectTransform, 0.28f, 0.42f, 0);

        scoreText = UIFactory.CreateLabel(bar.transform, "", 34, UIFactory.TextColor, TextAnchor.MiddleLeft);
        SetHorizontalBand(scoreText.rectTransform, 0.42f, 0.54f, 0);

        var right = new Vector2(1f, 0.5f);
        Button pause = UIFactory.CreateButton(bar.transform, "Пауза", new Vector2(180, 64), Pause);
        UIFactory.Place((RectTransform)pause.transform, right, new Vector2(-120, 0), new Vector2(180, 64));
        Button place = UIFactory.CreateButton(bar.transform, $"Подсказка (−{ScoreSystem.PlaceHintCost})", new Vector2(280, 64), OnPlaceHint, 28);
        UIFactory.Place((RectTransform)place.transform, right, new Vector2(-370, 0), new Vector2(280, 64));
        Button preview = UIFactory.CreateButton(bar.transform, $"Картинка (−{ScoreSystem.PreviewHintCost})", new Vector2(280, 64), OnPreviewHint, 28);
        UIFactory.Place((RectTransform)preview.transform, right, new Vector2(-670, 0), new Vector2(280, 64));
    }

    private static void SetHorizontalBand(RectTransform rt, float from, float to, float leftPadding)
    {
        rt.anchorMin = new Vector2(from, 0);
        rt.anchorMax = new Vector2(to, 1);
        rt.offsetMin = new Vector2(leftPadding, 0);
        rt.offsetMax = Vector2.zero;
    }

    private void BuildPausePanel()
    {
        pausePanel = UIFactory.CreateOverlay("PausePanel", canvasRoot);
        RectTransform column = UIFactory.CreateColumn("Buttons", pausePanel.transform, 24);
        UIFactory.Stretch(column);

        UIFactory.CreateSizedLabel(column, "Пауза", new Vector2(600, 110), 72, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        var size = new Vector2(440, 84);
        UIFactory.CreateButton(column, "Продолжить", size, Resume, 34);
        UIFactory.CreateButton(column, "Сохранить", size, () => { pausePanel.SetActive(false); saveView.Show(); }, 34);
        UIFactory.CreateButton(column, "Настройки", size, () => { pausePanel.SetActive(false); settingsView.Show(); }, 34);
        UIFactory.CreateButton(column, "Выйти в меню", size, ExitToMenu, 34);
        pausePanel.SetActive(false);
    }

    private void BuildResultPanel()
    {
        resultPanel = UIFactory.CreateOverlay("ResultPanel", canvasRoot);
        Image window = UIFactory.CreateImage("Window", resultPanel.transform, UIFactory.PanelColor);
        UIFactory.Place(window.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 680));

        RectTransform column = UIFactory.CreateColumn("Content", window.transform, 24);
        UIFactory.Stretch(column, 40, 40, 40, 40);
        UIFactory.CreateSizedLabel(column, "Картинка собрана!", new Vector2(720, 90), 56, UIFactory.TextColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        resultText = UIFactory.CreateSizedLabel(column, "", new Vector2(720, 320), 32, UIFactory.TextColor);

        RectTransform buttons = UIFactory.CreateRow("Buttons", column, 30);
        buttons.sizeDelta = new Vector2(720, 84);
        UIFactory.CreateButton(buttons, "Играть снова", new Vector2(320, 80), PlayAgain, 32);
        UIFactory.CreateButton(buttons, "В меню", new Vector2(320, 80), ExitToMenu, 32);
        resultPanel.SetActive(false);
    }

    // ---------- Пауза ----------

    private void HandleEscape()
    {
        if (finished)
            return;
        if (settingsView.IsOpen || saveView.IsOpen)
            ShowPauseMenu();
        else if (paused)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        if (finished)
            return;
        paused = true;
        Time.timeScale = 0f;
        timer.Running = false;
        board.InputEnabled = false;
        ShowPauseMenu();
    }

    private void ShowPauseMenu()
    {
        settingsView.Hide();
        saveView.Hide();
        pausePanel.SetActive(true);
        pausePanel.transform.SetAsLastSibling();
    }

    private void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        timer.Running = true;
        board.InputEnabled = true;
        pausePanel.SetActive(false);
        settingsView.Hide();
        saveView.Hide();
    }

    private void OnSettingsChanged()
    {
        if (!finished)
            board.SetGridVisible(SettingsManager.ShowGrid);
    }

    // ---------- Игровые события ----------

    private void OnPiecePlaced(PuzzlePiece piece)
    {
        score.AddCorrect();
        AudioManager.PlaySnap();
    }

    private void OnMistake()
    {
        score.AddMistake();
        AudioManager.PlayError();
        toast.Show($"Этот фрагмент не отсюда (−{ScoreSystem.MistakePenalty})", 1.2f);
    }

    private void OnPreviewHint()
    {
        if (paused || finished)
            return;
        if (hints.ShowPreview())
            toast.Show($"Подсказка: исходная картинка (−{ScoreSystem.PreviewHintCost})", 1.5f);
    }

    private void OnPlaceHint()
    {
        if (paused || finished)
            return;
        if (hints.PlaceRandomPiece())
        {
            AudioManager.PlaySnap();
            toast.Show($"Подсказка: фрагмент поставлен на место (−{ScoreSystem.PlaceHintCost})", 1.5f);
        }
    }

    private void OnCompleted()
    {
        finished = true;
        timer.Running = false;
        board.InputEnabled = false;
        board.SetGridVisible(false);

        int bonus = ScoreSystem.TimeBonus(GameSession.GridSize, timer.Elapsed);
        int total = score.Score + bonus;
        string key = GameSession.BestScoreKey;
        bool record = ScoreSystem.SubmitBest(key, total);

        resultText.text =
            $"Время: {PuzzleTimer.Format(timer.Elapsed)}\n" +
            $"Очки за сборку: {score.Score}\n" +
            $"Бонус за скорость: +{bonus}\n" +
            $"Ошибки: {score.Mistakes}    Подсказки: {score.HintsUsed}\n\n" +
            $"Итого: {total}\n" +
            (record ? "Новый рекорд!" : $"Рекорд: {ScoreSystem.GetBest(key)}");

        StartCoroutine(ShowResultDelayed());
    }

    private IEnumerator ShowResultDelayed()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        AudioManager.PlayWin();
        resultPanel.SetActive(true);
        resultPanel.transform.SetAsLastSibling();
    }

    // ---------- Сохранение и выход ----------

    private void SaveToSlot(int slot)
    {
        var data = new SaveData
        {
            levelId = GameSession.LevelId,
            gridSize = GameSession.GridSize,
            elapsed = timer.Elapsed,
            score = score.Score,
            mistakes = score.Mistakes,
            hintsUsed = score.HintsUsed,
            pieces = board.GetState(),
        };

        try
        {
            SaveSystem.Save(slot, data, GameSession.CustomImagePath);
            toast.Show($"Игра сохранена в слот {slot}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            toast.Show("Не удалось сохранить игру");
        }
        ShowPauseMenu();
    }

    private void PlayAgain()
    {
        GameSession.PendingLoad = null;
        SceneLoader.Load(SceneLoader.GameScene);
    }

    private void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.Load(SceneLoader.MenuScene);
    }
}
