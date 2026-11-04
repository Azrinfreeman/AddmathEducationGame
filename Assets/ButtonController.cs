using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isRight;
    public bool isLeft;
    public static ButtonController instance;

    private void Awake() {
        instance = this;
    }

    // pointerCurrentRaycast.gameObject is null when released off any UI element
    private static string HitName(PointerEventData e)
    {
        GameObject go = e.pointerPress;
        if (go == null) go = e.pointerCurrentRaycast.gameObject;
        if (go == null) go = e.pointerEnter;
        return go != null ? go.name : "";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        string n = HitName(eventData);
        if (n == "LeftArrow") isLeft = true;
        else if (n == "RightArrow") isRight = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // a finger lifted anywhere must always stop the rocket
        isLeft = false;
        isRight = false;
    }

    private void OnDisable()
    {
        isLeft = false;
        isRight = false;
    }
}
