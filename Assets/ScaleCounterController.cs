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

    private void OnCollisionEnter2D(Collision2D other) {

        if(other.gameObject.tag == "ball"){

        startingWeight += other.gameObject.GetComponent<WeightScale>().weight;
       
        }
        
    }

    private void OnCollisionStay2D(Collision2D other) {
        
    }
    private void OnCollisionExit2D(Collision2D other) {

        if(other.gameObject.tag == "ball"){

        startingWeight -= other.gameObject.GetComponent<WeightScale>().weight;
        text.text = startingWeight.ToString() + "Kg";
        }
        
    }


    
}
