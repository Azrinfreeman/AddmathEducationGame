using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ScaleCounterController : MonoBehaviour
{

    
    public int startingWeight =0;
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        startingWeight = 0;
        text = transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = startingWeight.ToString() + " Kg ";
    }

   private void OnTriggerEnter2D(Collider2D other) {
     if(other.gameObject.tag == "item"){

        startingWeight += other.gameObject.GetComponent<WeightScale>().weight;
        BallController.instance.inContainer[other.gameObject.transform.GetSiblingIndex()]=true;
        //other.gameObject.GetComponent<WeightScale>().inContainer = true;

        //other.gameObject.GetComponent<BoxCollider2D>().enabled = false;
       
        }
   }

    private void OnTriggerStay2D(Collider2D other) {
        
    }
    private void OnTriggerExit2D(Collider2D other) {
         if(other.gameObject.tag == "item"){

        startingWeight -= other.gameObject.GetComponent<WeightScale>().weight;
        text.text = startingWeight.ToString() + "Kg";
        }
        
    }


}
