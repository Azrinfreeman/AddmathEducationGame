using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
  
    [SerializeField] private Canvas canvas;
    [SerializeField] private BoxCollider2D colider;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public bool A;
    public bool T;
    public bool C;
    public bool G;

    [Header("Game 4")]
    public bool Met;
    public bool Leu;
    public bool Val;
    public bool Pro;
    public bool Ala;

    public bool bridge;

    public bool firstTime;


    [Header("6 slots")]
    public List<GameObject> slots;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        colider = GetComponent<BoxCollider2D>();



    }

    public void OnBeginDrag(PointerEventData eventData)
    {
       if (eventData.pointerDrag.tag == "bridge")
        {
            Debug.Log("asfasf");
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SetActive(false);
            }
        }
        //  Debug.Log("OnBeginDrag");
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;


       
    }

    public void OnDrag(PointerEventData eventData)
    {

       
        //Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerDrag.tag == "bridge")
        {

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SetActive(true);
            }
        }
        //   Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        


    }

    public void OnPointerDown(PointerEventData eventData)
    {
   //    Debug.Log("OnPointerDown");
    }

    
}



