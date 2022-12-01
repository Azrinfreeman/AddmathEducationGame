using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragScript : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{


    [SerializeField] private Canvas canvas;
    [SerializeField] private BoxCollider2D colider;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    

    private void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        colider = GetComponent<BoxCollider2D>();
        


    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    
          Debug.Log("OnBeginDrag");
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
        Destroy(gameObject.GetComponent<Rigidbody2D>());
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
       
    }

     public void OnDrag(PointerEventData eventData)
    {

       
        Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       
           Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        gameObject.AddComponent<Rigidbody2D>();
        setRealisticGravity();
        gameObject.GetComponent<BoxCollider2D>().enabled = true;

    }

    public void setRealisticGravity(){
        gameObject.GetComponent<Rigidbody2D>().mass = 120;
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 19.62f;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
