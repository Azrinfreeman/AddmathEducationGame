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

    private void OnCollisionEnter2D(Collision2D other) {
       
        if(other.gameObject.tag == "weight"){
            //other.gameObject.GetComponent<ScaleCounterController>().startingWeight += weight;
        }
    }

    private void OnCollisionExit2D(Collision2D other) {
        
        if(other.gameObject.tag == "weight"){
          //  other.gameObject.GetComponent<ScaleCounterController>().startingWeight -= weight;
        }
    }
}
