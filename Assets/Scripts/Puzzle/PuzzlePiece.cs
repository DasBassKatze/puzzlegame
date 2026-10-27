using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Фрагмент изображения. Перетаскивается мышью и фиксируется, когда его кладут на своё место.
/// </summary>
public class PuzzlePiece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Index { get; private set; }
    public Vector2 CorrectPosition { get; private set; }
    public bool IsPlaced { get; private set; }
    public RectTransform Rect { get; private set; }

    private PuzzleBoard board;
    private Image image;
    private Shadow shadow;
    private Vector2 dragOffset;
    private Vector2 dragStart;
    private bool dragging;
    private Coroutine moveRoutine;

    public void Init(PuzzleBoard owner, int index, Vector2 correctPosition, Sprite sprite, Vector2 size)
    {
        board = owner;
        Index = index;
        CorrectPosition = correctPosition;
        Rect = (RectTransform)transform;
        Rect.anchorMin = Rect.anchorMax = new Vector2(0.5f, 0.5f);
        Rect.sizeDelta = size;

        image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        shadow = gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.6f);
        shadow.effectDistance = new Vector2(4, -4);
    }

    private bool CanDrag => !IsPlaced && board.InputEnabled;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanDrag)
            Rect.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        dragging = true;
        dragStart = Rect.anchoredPosition;
        if (board.ScreenToLocal(eventData, out Vector2 pointer))
            dragOffset = Rect.anchoredPosition - pointer;
        transform.localScale = Vector3.one * 1.05f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || !board.InputEnabled)
            return;
        if (board.ScreenToLocal(eventData, out Vector2 pointer))
            Rect.anchoredPosition = board.ClampToPlayArea(pointer + dragOffset, Rect.sizeDelta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;
        dragging = false;
        transform.localScale = Vector3.one;
        board.OnPieceDropped(this, dragStart);
    }

    /// <summary>Плавно перемещает фрагмент в указанную точку.</summary>
    public void MoveTo(Vector2 target, float duration = 0.15f)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine(target, duration));
    }

    private IEnumerator MoveRoutine(Vector2 target, float duration)
    {
        Vector2 from = Rect.anchoredPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            Rect.anchoredPosition = Vector2.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        Rect.anchoredPosition = target;
        moveRoutine = null;
    }

    /// <summary>Фиксирует фрагмент на правильном месте: после этого его нельзя двигать.</summary>
    public void Lock(bool animate)
    {
        IsPlaced = true;
        dragging = false;
        transform.localScale = Vector3.one;
        image.raycastTarget = false;
        shadow.enabled = false;
        if (animate)
            MoveTo(CorrectPosition);
        else
            Rect.anchoredPosition = CorrectPosition;
    }
}
