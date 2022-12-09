using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stickmanProperty : MonoBehaviour
{
    public bool isInside;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
  private void OnTriggerStay2D(Collider2D other) {
    if(other.gameObject.tag == "pathway"){
        isInside = true;
        Debug.Log("inside");
    }
  }

  private void OnTriggerExit2D(Collider2D other) {
    if(other.gameObject.tag == "pathway"){
        isInside = false;
        Debug.Log("exit");
    }
  }
}
