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

    // number of pathway colliders currently overlapped, so leaving one
    // collider while still inside another does not count as "outside"
    private int pathwayCount;

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("pathway")){
            pathwayCount++;
            isInside = true;
        }
        if(other.gameObject.CompareTag("triangle") && toTouchTriangle){
            isTouchingTriangle = true;
        }
        if(other.gameObject.CompareTag("circle") && toTouchCircle){
            isTouchingCircle = true;
        }
        if(other.gameObject.CompareTag("square") && toTouchSquare){
            isTouchingSquare = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.CompareTag("pathway")){
            pathwayCount = Mathf.Max(0, pathwayCount - 1);
            isInside = pathwayCount > 0;
        }
        if(other.gameObject.CompareTag("triangle") && toTouchTriangle){
            isTouchingTriangle = false;
        }
        if(other.gameObject.CompareTag("circle") && toTouchCircle){
            isTouchingCircle = false;
        }
        if(other.gameObject.CompareTag("square") && toTouchSquare){
            isTouchingSquare = false;
        }
    }
}
