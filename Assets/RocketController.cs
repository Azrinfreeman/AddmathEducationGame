using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class RocketController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public Rigidbody2D rocket;
    public Transform fuel;
    public Vector3 move;
    public int playerSpeed = 3;
    public bool buttonPressed;
    // Start is called before the first frame update
    void Start()
    {
        rocket = GetComponent<Rigidbody2D>();
        fuel = GetComponent<Transform>();
    }


 
public void OnPointerDown(PointerEventData eventData){
     buttonPressed = true;
}
 
public void OnPointerUp(PointerEventData eventData){
    buttonPressed = false;
}



    public void MoveRight(){
        if(buttonPressed){
            
        rocket.velocity = new Vector2(10f * playerSpeed, rocket.velocity.y );
        //rocket.Move(move * (Time.deltaTime * playerSpeed));
        }
    }


    public void MoveLeft(){
        if(buttonPressed){
            rocket.velocity = new Vector2(-10f * playerSpeed, rocket.velocity.y );
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(buttonPressed){
            
        }
    }
}
