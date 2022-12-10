using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stickmanProperty : MonoBehaviour
{
    public static stickmanProperty instance;

    private void Awake() {
      instance = this;
    }
    public bool isInside;

  
  public bool toTouchTriangle;
    public bool isTouchingTriangle;
    public bool toTouchCircle;
    public bool isTouchingCircle;
    public bool toTouchSquare;
    public bool isTouchingSquare;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
  private void OnTriggerEnter2D(Collider2D other) {
    if(other.gameObject.tag == "pathway"){
        isInside = true;
        Debug.Log("enter2d");
    }

    if(other.gameObject.tag == "triangle"){
      Debug.Log("triangle");
      if(toTouchTriangle){
        
      isTouchingTriangle = true;
      }
    }

    if(other.gameObject.tag == "circle"){
      if (toTouchCircle){
        isTouchingCircle = true;
      }
    }

    if(other.gameObject.tag == "square"){
      if (toTouchSquare){
        isTouchingSquare = true;
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other) {
    if(other.gameObject.tag == "pathway"){
        isInside = false;
        Debug.Log("exit2d");
    }
  }
}
