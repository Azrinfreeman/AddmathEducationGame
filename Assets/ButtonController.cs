using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

  public bool isRight;
  public bool isLeft;

  public static ButtonController instance;

  private void Awake() {
    instance = this;
  }
    // Start is called before the first frame update
    void Start()
    {
      
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
        if(eventData.pointerCurrentRaycast.gameObject.name == "LeftArrow"){
            isLeft = true;
        }else if (eventData.pointerCurrentRaycast.gameObject.name == "RightArrow"){
            isRight = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData){
        Debug.Log("up: " + eventData.pointerCurrentRaycast.gameObject.name);
         if(eventData.pointerCurrentRaycast.gameObject.name == "LeftArrow"){
            isLeft = false;
        }else if (eventData.pointerCurrentRaycast.gameObject.name == "RightArrow"){
            isRight = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
