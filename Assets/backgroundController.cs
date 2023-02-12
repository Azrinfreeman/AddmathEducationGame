using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundController : MonoBehaviour
{
   
    public bool isToNextLevel;
    
    public Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "imageBorder"){
            transform.localPosition = new Vector2(transform.localPosition.x, 10079);
            rb.velocity = new Vector2(0.0f, -555.0f);
            if(isToNextLevel){
                 
                
                //GamePraController.instance.level++;
                GamePraController.instance.ToNextLevel();
                GamePraController.instance.level++;
            }else{
               //rb.velocity = new Vector2(0.0f, 355.0f);
            }
        }
    }
}
