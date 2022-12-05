using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightScale : MonoBehaviour
{

    public int weight;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "weight"){
            BallController.instance.Balls[transform.GetSiblingIndex()] = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.tag == "weight"){
            BallController.instance.Balls[transform.GetSiblingIndex()] = false;
          //  other.gameObject.GetComponent<ScaleCounterController>().startingWeight -= weight;
        }
    }
}
