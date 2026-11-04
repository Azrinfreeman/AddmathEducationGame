using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class RocketController : MonoBehaviour
{

    public Rigidbody2D rocket;
    public Transform fuel;
    public Vector3 move;
    public int playerSpeed = 3;
    

    // Start is called before the first frame update
    void Start()
    {
        rocket = GetComponent<Rigidbody2D>();
        fuel = GetComponent<Transform>();
    }


  



    public void MoveRight(){
      
            
        rocket.linearVelocity = new Vector2(10f * playerSpeed, rocket.linearVelocity.y );
        //rocket.Move(move * (Time.deltaTime * playerSpeed));
        
    }

    public void Stop(){
        rocket.linearVelocity = new Vector2(0f * playerSpeed, rocket.linearVelocity.y );
    }

    public void MoveLeft(){
        
            rocket.linearVelocity = new Vector2(-10f * playerSpeed, rocket.linearVelocity.y );
        
    }
    // Update is called once per frame
    void Update()
    {

        if(!ButtonController.instance.isRight && !ButtonController.instance.isLeft){
            Stop();
        }
        if(ButtonController.instance.isRight){
            MoveRight();
        }

        if(ButtonController.instance.isLeft){
            MoveLeft();
        }
    }
}
