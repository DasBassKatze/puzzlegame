/// <summary>
/// Игровой таймер: считает время сборки, пока партия не на паузе.
/// </summary>
public class PuzzleTimer
{
    public float Elapsed { get; set; }
    public bool Running { get; set; }

    public void Tick(float deltaTime)
    {
        if (Running)
            Elapsed += deltaTime;
    }

    public static string Format(float seconds)
    {
        int total = (int)seconds;
        return $"{total / 60:00}:{total % 60:00}";
    }
}
