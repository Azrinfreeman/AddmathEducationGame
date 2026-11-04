using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DragScript : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{


    [SerializeField] private Canvas canvas;
    [SerializeField] private BoxCollider2D colider;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    public bool isBall =false;

    // Tap alternative to dragging: tap a piece to pick it up, then tap where it should go.
    private static DragScript selected;
    private bool tapMoving;
    private Vector3 restScale = Vector3.one;
    private const float SelectedScale = 1.12f;

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        colider = GetComponent<BoxCollider2D>();
        restScale = rectTransform.localScale;
    }

    private void OnDisable()
    {
        if (selected == this) Deselect();
        tapMoving = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (selected == this) Deselect();
        BeginHold();
    }

     public void OnDrag(PointerEventData eventData)
    {


        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndHold();
    }

    // A plain tap (not the end of a drag) selects this piece, or releases it if already selected.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || tapMoving) return;
        if (selected == this) { Deselect(); return; }
        if (selected) selected.Deselect();
        selected = this;
        rectTransform.localScale = restScale * SelectedScale;
        canvasGroup.alpha = .85f;
    }

    private void Deselect()
    {
        if (selected == this) selected = null;
        if (!rectTransform) return;
        rectTransform.localScale = restScale;
        if (!tapMoving && canvasGroup) canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        if (selected != this || tapMoving) return;
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;
        // Pressing the piece again is handled by OnPointerClick. Pressing a button cancels the pick-up.
        var hits = new List<RaycastResult>();
        var data = new PointerEventData(EventSystem.current) { position = pointer.position.ReadValue() };
        EventSystem.current.RaycastAll(data, hits);
        foreach (var hit in hits)
        {
            if (hit.gameObject.transform.IsChildOf(transform)) return;
            if (hit.gameObject.GetComponentInParent<Selectable>()) { Deselect(); return; }
        }
        StartCoroutine(TapMove(data.position));
    }

    private IEnumerator TapMove(Vector2 screenPoint)
    {
        tapMoving = true;
        rectTransform.localScale = restScale;
        BeginHold();
        yield return null; // the old Rigidbody2D is destroyed at the end of the frame
        var parent = (RectTransform)rectTransform.parent;
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, cam, out var local))
            rectTransform.localPosition = local;
        EndHold();
        tapMoving = false;
        selected = null;
    }

    private void BeginHold()
    {
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
        Destroy(gameObject.GetComponent<Rigidbody2D>());

        gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void EndHold()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        if (!gameObject.GetComponent<Rigidbody2D>()) gameObject.AddComponent<Rigidbody2D>();
        if(isBall){

        setRealisticGravity();
        }else {
            SetNoGravity();
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    public void SetNoGravity(){
            gameObject.GetComponent<Rigidbody2D>().mass = 1;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;

    }
    public void setRealisticGravity(){
        gameObject.GetComponent<Rigidbody2D>().mass = 120;
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 19.62f;
    }
}
