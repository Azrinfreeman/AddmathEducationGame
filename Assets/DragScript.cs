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
    
    public bool isBall =false;
    private void Awake()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        colider = GetComponent<BoxCollider2D>();
        


    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
        Destroy(gameObject.GetComponent<Rigidbody2D>());
        
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
       
    }

     public void OnDrag(PointerEventData eventData)
    {

       
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        gameObject.AddComponent<Rigidbody2D>();
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
