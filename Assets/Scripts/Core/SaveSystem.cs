using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Состояние одного фрагмента в сохранении.
/// </summary>
[Serializable]
public class PieceState
{
    public int index;
    public float x;
    public float y;
    public bool placed;
}

/// <summary>
/// Данные сохранённой партии. Сериализуются в JSON.
/// </summary>
[Serializable]
public class SaveData
{
    public int levelId;
    public int gridSize;
    public string customImageFile;
    public float elapsed;
    public int score;
    public int mistakes;
    public int hintsUsed;
    public string savedAt;
    public List<PieceState> pieces = new List<PieceState>();
}

/// <summary>
/// Сохранение и загрузка партий в три слота в папке Application.persistentDataPath/saves.
/// </summary>
public static class SaveSystem
{
    public const int SlotCount = 3;

    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "saves");

    private static string GetSlotPath(int slot) => Path.Combine(SaveDirectory, $"slot{slot}.json");

    public static bool Exists(int slot) => File.Exists(GetSlotPath(slot));

    public static void Save(int slot, SaveData data, string customImageSource)
    {
        Directory.CreateDirectory(SaveDirectory);

        if (data.levelId == GameSession.CustomLevelId && !string.IsNullOrEmpty(customImageSource))
        {
            // Своя картинка копируется рядом с сохранением, чтобы игра загрузилась,
            // даже если исходный файл переместят или удалят.
            string imageName = $"slot{slot}_image.dat";
            string destination = Path.Combine(SaveDirectory, imageName);
            if (Path.GetFullPath(customImageSource) != Path.GetFullPath(destination))
                File.Copy(customImageSource, destination, true);
            data.customImageFile = imageName;
        }

        data.savedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        File.WriteAllText(GetSlotPath(slot), JsonUtility.ToJson(data, true));
    }

    public static SaveData Load(int slot)
    {
        if (!Exists(slot))
            return null;
        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(GetSlotPath(slot)));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Не удалось прочитать слот {slot}: {e.Message}");
            return null;
        }
    }

    public static string GetImagePath(SaveData data)
    {
        if (string.IsNullOrEmpty(data.customImageFile))
            return null;
        return Path.Combine(SaveDirectory, data.customImageFile);
    }

    public static string Describe(int slot)
    {
        SaveData data = Load(slot);
        if (data == null)
            return $"Слот {slot}: пусто";
        string name = LevelDatabase.GetName(data.levelId);
        return $"Слот {slot}: {name}, {data.gridSize}×{data.gridSize}, " +
               $"{PuzzleTimer.Format(data.elapsed)}, {data.score} очк., {data.savedAt}";
    }
}
