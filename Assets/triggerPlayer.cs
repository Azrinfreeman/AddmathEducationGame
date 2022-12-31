using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerPlayer : MonoBehaviour
{
    public bool isCorrect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "Player"){
            if(isCorrect){

                GamePraController.instance.score +=10;
            }else{
                GamePraController.instance.score -=10;
            }
            Destroy(gameObject);
        }
    }
}
