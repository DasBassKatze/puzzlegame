using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Короткое сообщение внизу экрана, которое само исчезает через пару секунд (работает и на паузе).
/// </summary>
public class ToastMessage : MonoBehaviour
{
    private Text text;
    private Coroutine routine;

    public static ToastMessage Create(Transform parent)
    {
        Text label = UIFactory.CreateToast(parent);
        var toast = label.gameObject.AddComponent<ToastMessage>();
        toast.text = label;
        return toast;
    }

    public void Show(string message, float duration = 2f)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        text.text = message;
        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        gameObject.SetActive(false);
        routine = null;
    }
}
