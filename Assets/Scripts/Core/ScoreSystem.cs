using UnityEngine;

/// <summary>
/// Подсчёт очков: награда за верно поставленный фрагмент, штрафы за ошибки и подсказки,
/// бонус за скорость сборки и хранение рекордов.
/// </summary>
public class ScoreSystem
{
    public const int CorrectPoints = 10;
    public const int MistakePenalty = 2;
    public const int PreviewHintCost = 5;
    public const int PlaceHintCost = 15;
    public const int BonusPerPiece = 20;

    public int Score { get; private set; }
    public int Mistakes { get; private set; }
    public int HintsUsed { get; private set; }

    public void Restore(int score, int mistakes, int hintsUsed)
    {
        Score = score;
        Mistakes = mistakes;
        HintsUsed = hintsUsed;
    }

    public void AddCorrect()
    {
        Score += CorrectPoints;
    }

    public void AddMistake()
    {
        Mistakes++;
        Score = Mathf.Max(0, Score - MistakePenalty);
    }

    public void SpendHint(int cost)
    {
        HintsUsed++;
        Score = Mathf.Max(0, Score - cost);
    }

    /// <summary>Бонус уменьшается на 1 очко за каждую секунду сборки.</summary>
    public static int TimeBonus(int gridSize, float elapsed)
    {
        return Mathf.Max(0, gridSize * gridSize * BonusPerPiece - Mathf.FloorToInt(elapsed));
    }

    public static int GetBest(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>Сохраняет результат, если он лучше рекорда. Возвращает true для нового рекорда.</summary>
    public static bool SubmitBest(string key, int score)
    {
        if (score <= GetBest(key))
            return false;
        PlayerPrefs.SetInt(key, score);
        PlayerPrefs.Save();
        return true;
    }
}
