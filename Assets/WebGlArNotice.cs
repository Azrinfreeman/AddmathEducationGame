using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shown instead of an AR lesson when the game runs in a web browser (AR needs the phone app).
/// Tap anywhere on the card to close it.
/// </summary>
public static class WebGlArNotice
{
    private static GameObject current;

    public static void Show()
    {
        if (current != null) return;

        bool english = PlayerPrefs.GetInt("EnglishLang", 0) == 1;
        string message = english
            ? "AR lessons need the Android app.\nPlease install the app to try them."
            : "Pelajaran AR memerlukan aplikasi Android.\nSila pasang aplikasi untuk mencubanya.";

        current = new GameObject("WebGlArNotice", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = current.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = current.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        Image dim = MakeImage("Dim", current.transform, new Color(0, 0, 0, 0.55f));
        Stretch(dim.rectTransform);
        Button close = dim.gameObject.AddComponent<Button>();
        close.onClick.AddListener(() => { Object.Destroy(current); current = null; });

        Image card = MakeImage("Card", dim.transform, new Color(1f, 0.9725f, 0.9059f, 1f)); // cream
        card.rectTransform.anchorMin = card.rectTransform.anchorMax = card.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        card.rectTransform.sizeDelta = new Vector2(760, 360);

        GameObject textObject = new GameObject("Text", typeof(Text));
        textObject.transform.SetParent(card.transform, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = message;
        text.fontSize = 36;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.1255f, 0.2275f, 0.2784f, 1f); // ink
        text.raycastTarget = false;
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(30, 30);
        text.rectTransform.offsetMax = new Vector2(-30, -30);
    }

    private static Image MakeImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
