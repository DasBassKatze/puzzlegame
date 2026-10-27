/// <summary>
/// Параметры текущей партии, которые передаются из меню в игровую сцену.
/// </summary>
public static class GameSession
{
    public const int CustomLevelId = -1;

    public static int LevelId = 0;
    public static int GridSize = 3;
    public static string CustomImagePath;
    public static SaveData PendingLoad;

    public static bool IsCustom => LevelId == CustomLevelId;

    public static string LevelName => LevelDatabase.GetName(LevelId);

    public static string BestScoreKey =>
        IsCustom ? $"best_custom_{GridSize}" : $"best_{LevelId}_{GridSize}";

    public static void StartLevel(int levelId, int gridSize)
    {
        LevelId = levelId;
        GridSize = gridSize;
        CustomImagePath = null;
        PendingLoad = null;
    }

    public static void StartCustom(string imagePath, int gridSize)
    {
        LevelId = CustomLevelId;
        GridSize = gridSize;
        CustomImagePath = imagePath;
        PendingLoad = null;
    }

    public static void StartFromSave(SaveData data)
    {
        LevelId = data.levelId;
        GridSize = data.gridSize;
        CustomImagePath = data.levelId == CustomLevelId ? SaveSystem.GetImagePath(data) : null;
        PendingLoad = data;
    }
}
