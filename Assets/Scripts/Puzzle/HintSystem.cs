using System.Collections;
using UnityEngine;

/// <summary>
/// Подсказки: показ исходного изображения поверх поля и установка случайного фрагмента на место.
/// </summary>
public class HintSystem
{
    private const float PreviewDuration = 3f;

    private readonly PuzzleBoard board;
    private readonly ScoreSystem score;
    private readonly MonoBehaviour host;
    private bool previewActive;

    public HintSystem(PuzzleBoard board, ScoreSystem score, MonoBehaviour host)
    {
        this.board = board;
        this.score = score;
        this.host = host;
    }

    public bool ShowPreview()
    {
        if (previewActive)
            return false;
        score.SpendHint(ScoreSystem.PreviewHintCost);
        host.StartCoroutine(PreviewRoutine());
        return true;
    }

    private IEnumerator PreviewRoutine()
    {
        previewActive = true;
        board.Preview.gameObject.SetActive(true);
        board.Preview.rectTransform.SetAsLastSibling();
        yield return new WaitForSeconds(PreviewDuration);
        board.Preview.gameObject.SetActive(false);
        previewActive = false;
    }

    public bool PlaceRandomPiece()
    {
        PuzzlePiece piece = board.GetRandomUnplaced();
        if (piece == null)
            return false;
        score.SpendHint(ScoreSystem.PlaceHintCost);
        board.PlacePiece(piece, false);
        return true;
    }
}
