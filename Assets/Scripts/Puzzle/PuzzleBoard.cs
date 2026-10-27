using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Игровое поле: нарезает изображение на фрагменты, перемешивает их на панели
/// и проверяет, на своё ли место положен каждый фрагмент.
/// </summary>
public class PuzzleBoard : MonoBehaviour
{
    /// <summary>Доля размера ячейки, на которой фрагмент «примагничивается» к своему месту.</summary>
    private const float SnapFactor = 0.35f;

    public event Action<PuzzlePiece> PiecePlaced;
    public event Action Mistake;
    public event Action Completed;

    public bool InputEnabled { get; set; } = true;
    public int GridSize { get; private set; }
    public int PlacedCount { get; private set; }
    public IReadOnlyList<PuzzlePiece> Pieces => pieces;
    public Image Preview { get; private set; }

    private RectTransform playArea;
    private RectTransform piecesRoot;
    private Vector2 boardCenter;
    private Vector2 boardSize;
    private Vector2 cellSize;
    private Rect trayRect;
    private readonly List<PuzzlePiece> pieces = new List<PuzzlePiece>();
    private readonly List<Image> cells = new List<Image>();

    /// <param name="area">Игровая область, в координатах которой задаются поле и панель.</param>
    /// <param name="boardArea">Максимальная область под поле; поле вписывается в неё с сохранением пропорций.</param>
    /// <param name="tray">Область панели, на которой изначально лежат фрагменты.</param>
    public void Build(RectTransform area, Texture2D texture, int gridSize, Rect boardArea, Rect tray)
    {
        playArea = area;
        GridSize = gridSize;
        trayRect = tray;

        float aspect = texture.width / (float)texture.height;
        Vector2 size = boardArea.size;
        if (size.x / size.y > aspect)
            size.x = size.y * aspect;
        else
            size.y = size.x / aspect;
        boardSize = size;
        boardCenter = boardArea.center;
        cellSize = boardSize / gridSize;

        Image frame = UIFactory.CreateImage("Board", playArea, new Color32(14, 18, 30, 255));
        UIFactory.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), boardCenter, boardSize + new Vector2(12, 12));

        RectTransform cellsRoot = UIFactory.CreateRect("Cells", playArea);
        UIFactory.Stretch(cellsRoot);
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                Image cell = UIFactory.CreateImage($"Cell {row}-{col}", cellsRoot, Color.white);
                UIFactory.Place(cell.rectTransform, new Vector2(0.5f, 0.5f), CellCenter(row, col), cellSize - new Vector2(2, 2));
                cell.raycastTarget = false;
                cells.Add(cell);
            }
        }
        SetGridVisible(SettingsManager.ShowGrid);

        Preview = UIFactory.CreateImage("Preview", playArea, new Color(1, 1, 1, 0.55f));
        UIFactory.Place(Preview.rectTransform, new Vector2(0.5f, 0.5f), boardCenter, boardSize);
        Preview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        Preview.raycastTarget = false;
        Preview.gameObject.SetActive(false);

        piecesRoot = UIFactory.CreateRect("Pieces", playArea);
        UIFactory.Stretch(piecesRoot);
        CreatePieces(texture);
    }

    private void CreatePieces(Texture2D texture)
    {
        float pieceWidth = texture.width / (float)GridSize;
        float pieceHeight = texture.height / (float)GridSize;

        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                // Строки поля считаются сверху вниз, а координаты текстуры — снизу вверх.
                var rect = new Rect(col * pieceWidth, (GridSize - 1 - row) * pieceHeight, pieceWidth, pieceHeight);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);

                RectTransform rt = UIFactory.CreateRect($"Piece {row}-{col}", piecesRoot);
                var piece = rt.gameObject.AddComponent<PuzzlePiece>();
                piece.Init(this, row * GridSize + col, CellCenter(row, col), sprite, cellSize);
                pieces.Add(piece);
            }
        }
    }

    public Vector2 CellCenter(int row, int col)
    {
        return boardCenter + new Vector2(
            -boardSize.x / 2f + cellSize.x * (col + 0.5f),
            boardSize.y / 2f - cellSize.y * (row + 0.5f));
    }

    /// <summary>Раскладывает фрагменты в случайных точках панели в случайном порядке.</summary>
    public void Shuffle()
    {
        var order = new List<PuzzlePiece>(pieces);
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        float halfW = Mathf.Min(cellSize.x / 2f, trayRect.width / 2f);
        float halfH = Mathf.Min(cellSize.y / 2f, trayRect.height / 2f);
        foreach (PuzzlePiece piece in order)
        {
            piece.Rect.anchoredPosition = new Vector2(
                UnityEngine.Random.Range(trayRect.xMin + halfW, trayRect.xMax - halfW),
                UnityEngine.Random.Range(trayRect.yMin + halfH, trayRect.yMax - halfH));
            piece.Rect.SetAsLastSibling();
        }
    }

    public bool ScreenToLocal(PointerEventData eventData, out Vector2 local)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            playArea, eventData.position, eventData.pressEventCamera, out local);
    }

    public Vector2 ClampToPlayArea(Vector2 position, Vector2 size)
    {
        Rect r = playArea.rect;
        position.x = Mathf.Clamp(position.x, r.xMin + size.x / 2f, r.xMax - size.x / 2f);
        position.y = Mathf.Clamp(position.y, r.yMin + size.y / 2f, r.yMax - size.y / 2f);
        return position;
    }

    /// <summary>
    /// Проверка отпущенного фрагмента. Если он рядом со своей ячейкой, он фиксируется.
    /// Если он положен на поле в чужую ячейку, это ошибка и фрагмент возвращается назад.
    /// Вне поля фрагмент остаётся там, где его отпустили.
    /// </summary>
    public void OnPieceDropped(PuzzlePiece piece, Vector2 dragStart)
    {
        if (!InputEnabled)
        {
            piece.MoveTo(dragStart);
            return;
        }

        Vector2 position = piece.Rect.anchoredPosition;
        var boardRect = new Rect(boardCenter - boardSize / 2f, boardSize);
        if (!boardRect.Contains(position))
            return;

        float snapDistance = Mathf.Min(cellSize.x, cellSize.y) * SnapFactor;
        if (Vector2.Distance(position, piece.CorrectPosition) <= snapDistance)
        {
            PlacePiece(piece, true);
        }
        else
        {
            piece.MoveTo(dragStart, 0.2f);
            Mistake?.Invoke();
        }
    }

    /// <param name="byPlayer">true, если фрагмент поставил игрок (за это начисляются очки), false — подсказка или загрузка.</param>
    public void PlacePiece(PuzzlePiece piece, bool byPlayer, bool animate = true)
    {
        if (piece.IsPlaced)
            return;
        piece.Lock(animate);
        piece.Rect.SetAsFirstSibling();
        PlacedCount++;

        if (byPlayer)
            PiecePlaced?.Invoke(piece);
        if (PlacedCount == pieces.Count)
            Completed?.Invoke();
    }

    public PuzzlePiece GetRandomUnplaced()
    {
        var free = pieces.FindAll(p => !p.IsPlaced);
        if (free.Count == 0)
            return null;
        return free[UnityEngine.Random.Range(0, free.Count)];
    }

    public void SetGridVisible(bool visible)
    {
        Color color = visible ? new Color(1, 1, 1, 0.07f) : new Color(1, 1, 1, 0f);
        foreach (Image cell in cells)
            cell.color = color;
    }

    public List<PieceState> GetState()
    {
        var states = new List<PieceState>();
        foreach (PuzzlePiece piece in pieces)
        {
            Vector2 p = piece.Rect.anchoredPosition;
            states.Add(new PieceState { index = piece.Index, x = p.x, y = p.y, placed = piece.IsPlaced });
        }
        return states;
    }

    public void ApplyState(List<PieceState> states)
    {
        foreach (PieceState state in states)
        {
            if (state.index < 0 || state.index >= pieces.Count)
                continue;
            PuzzlePiece piece = pieces[state.index];
            if (state.placed)
                PlacePiece(piece, false, false);
            else
                piece.Rect.anchoredPosition = new Vector2(state.x, state.y);
        }
    }
}
