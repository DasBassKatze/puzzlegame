using UnityEngine;

/// <summary>
/// Описание одного встроенного уровня: название и путь к картинке в папке Resources.
/// </summary>
public class LevelInfo
{
    public string Name;
    public string Resource;

    public LevelInfo(string name, string resource)
    {
        Name = name;
        Resource = resource;
    }
}

/// <summary>
/// Набор базовых уровней и доступных размеров сетки.
/// </summary>
public static class LevelDatabase
{
    public static readonly LevelInfo[] Levels =
    {
        new LevelInfo("Горы", "Levels/level1"),
        new LevelInfo("Море", "Levels/level2"),
        new LevelInfo("Ночной город", "Levels/level3"),
        new LevelInfo("Цветы", "Levels/level4"),
        new LevelInfo("Космос", "Levels/level5"),
        new LevelInfo("Осенний лес", "Levels/level6"),
    };

    public static readonly int[] GridSizes = { 3, 4, 5 };

    public static readonly string[] GridNames = { "Лёгкий 3×3", "Средний 4×4", "Сложный 5×5" };

    public static Texture2D GetTexture(int levelId)
    {
        if (levelId < 0 || levelId >= Levels.Length)
            return null;
        return Resources.Load<Texture2D>(Levels[levelId].Resource);
    }

    public static string GetName(int levelId)
    {
        if (levelId >= 0 && levelId < Levels.Length)
            return Levels[levelId].Name;
        return "Своя картинка";
    }
}
