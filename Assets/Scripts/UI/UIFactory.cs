using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Создание элементов интерфейса из кода: холст, панели, надписи, кнопки, ползунки, переключатели.
/// Все экраны игры собираются через этот класс, поэтому оформление везде одинаковое.
/// </summary>
public static class UIFactory
{
    public static readonly Color Background = new Color32(24, 30, 48, 255);
    public static readonly Color PanelColor = new Color32(36, 45, 70, 255);
    public static readonly Color ButtonColor = new Color32(56, 70, 108, 255);
    public static readonly Color AccentColor = new Color32(86, 140, 255, 255);
    public static readonly Color AccentDark = new Color32(60, 105, 210, 255);
    public static readonly Color TextColor = new Color32(235, 238, 245, 255);
    public static readonly Color MutedText = new Color32(160, 170, 195, 255);
    public static readonly Color Overlay = new Color(0f, 0f, 0f, 0.72f);

    private static Font font;

    public static Font Font
    {
        get
        {
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    /// <summary>Растягивает элемент на всего родителя с отступами.</summary>
    public static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    /// <summary>Размещает элемент заданного размера относительно точки привязки.</summary>
    public static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    public static Image CreateImage(string name, Transform parent, Color color)
    {
        RectTransform rt = CreateRect(name, parent);
        var image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    /// <summary>Затемняющая подложка на весь экран, перекрывающая нажатия на элементы под ней.</summary>
    public static GameObject CreateOverlay(string name, Transform parent)
    {
        Image overlay = CreateImage(name, parent, Overlay);
        Stretch(overlay.rectTransform);
        return overlay.gameObject;
    }

    public static Text CreateLabel(Transform parent, string text, int size, Color color,
        TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
    {
        RectTransform rt = CreateRect("Text", parent);
        var label = rt.gameObject.AddComponent<Text>();
        label.font = Font;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = alignment;
        label.text = text;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        return label;
    }

    public static Button CreateButton(Transform parent, string text, Vector2 size, UnityAction onClick, int fontSize = 30)
    {
        Image image = CreateImage("Button " + text, parent, Color.white);
        image.rectTransform.sizeDelta = size;

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        SetButtonColor(button, ButtonColor);

        Text label = CreateLabel(image.transform, text, fontSize, TextColor);
        Stretch(label.rectTransform, 10, 0, 10, 0);

        button.onClick.AddListener(AudioManager.PlayClick);
        if (onClick != null)
            button.onClick.AddListener(onClick);
        return button;
    }

    public static void SetButtonColor(Button button, Color normal)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = AccentColor;
        colors.selectedColor = normal;
        colors.pressedColor = AccentDark;
        colors.disabledColor = new Color(normal.r, normal.g, normal.b, 0.4f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
    }

    public static Text GetButtonLabel(Button button)
    {
        return button.GetComponentInChildren<Text>();
    }

    public static RectTransform CreateColumn(string name, Transform parent, float spacing)
    {
        RectTransform rt = CreateRect(name, parent);
        var layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return rt;
    }

    public static RectTransform CreateRow(string name, Transform parent, float spacing)
    {
        RectTransform rt = CreateRect(name, parent);
        var layout = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return rt;
    }

    /// <summary>Надпись фиксированного размера для использования внутри строки или колонки.</summary>
    public static Text CreateSizedLabel(Transform parent, string text, Vector2 size, int fontSize,
        Color color, TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
    {
        Text label = CreateLabel(parent, text, fontSize, color, alignment, style);
        label.rectTransform.sizeDelta = size;
        return label;
    }

    public static Slider CreateSlider(Transform parent, Vector2 size, float value, UnityAction<float> onChange)
    {
        RectTransform root = CreateRect("Slider", parent);
        root.sizeDelta = size;
        var slider = root.gameObject.AddComponent<Slider>();

        float barInset = size.y * 0.38f;
        Image background = CreateImage("Background", root, Background);
        Stretch(background.rectTransform, 0, barInset, 0, barInset);

        RectTransform fillArea = CreateRect("Fill Area", root);
        Stretch(fillArea, 10, barInset, 10, barInset);
        Image fill = CreateImage("Fill", fillArea, AccentColor);
        Stretch(fill.rectTransform);

        RectTransform handleArea = CreateRect("Handle Slide Area", root);
        Stretch(handleArea, 12, 0, 12, 0);
        Image handle = CreateImage("Handle", handleArea, TextColor);
        handle.rectTransform.anchorMin = new Vector2(0, 0);
        handle.rectTransform.anchorMax = new Vector2(0, 1);
        handle.rectTransform.sizeDelta = new Vector2(24, 0);

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        if (onChange != null)
            slider.onValueChanged.AddListener(onChange);
        return slider;
    }

    public static Toggle CreateToggle(Transform parent, string text, Vector2 size, bool value, UnityAction<bool> onChange)
    {
        RectTransform root = CreateRect("Toggle " + text, parent);
        root.sizeDelta = size;
        var toggle = root.gameObject.AddComponent<Toggle>();

        Image box = CreateImage("Box", root, Background);
        Place(box.rectTransform, new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(44, 44));
        Image check = CreateImage("Check", box.transform, AccentColor);
        Stretch(check.rectTransform, 8, 8, 8, 8);

        Text label = CreateLabel(root, text, 28, TextColor, TextAnchor.MiddleLeft);
        Stretch(label.rectTransform, 64, 0, 0, 0);

        toggle.targetGraphic = box;
        toggle.graphic = check;
        toggle.isOn = value;
        toggle.onValueChanged.AddListener(_ => AudioManager.PlayClick());
        if (onChange != null)
            toggle.onValueChanged.AddListener(onChange);
        return toggle;
    }

    /// <summary>Тост: короткое сообщение внизу экрана.</summary>
    public static Text CreateToast(Transform parent)
    {
        Text toast = CreateLabel(parent, "", 30, TextColor);
        toast.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        toast.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        toast.rectTransform.pivot = new Vector2(0.5f, 0f);
        toast.rectTransform.anchoredPosition = new Vector2(0, 30);
        toast.rectTransform.sizeDelta = new Vector2(1400, 50);
        var shadow = toast.gameObject.AddComponent<Shadow>();
        shadow.effectDistance = new Vector2(2, -2);
        toast.gameObject.SetActive(false);
        return toast;
    }
}
