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
      
            
        rocket.velocity = new Vector2(10f * playerSpeed, rocket.velocity.y );
        //rocket.Move(move * (Time.deltaTime * playerSpeed));
        
    }

    public void Stop(){
        rocket.velocity = new Vector2(0f * playerSpeed, rocket.velocity.y );
    }

    public void MoveLeft(){
        
            rocket.velocity = new Vector2(-10f * playerSpeed, rocket.velocity.y );
        
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
