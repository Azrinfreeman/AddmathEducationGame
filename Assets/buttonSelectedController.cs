using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class buttonSelectedController : MonoBehaviour
{
    public Button[] answers = new Button[4];
    // Start is called before the first frame update
    void Start()
    {
       for(int i =0; i < 4; i++){
        answers[i] = transform.GetChild(i).GetComponent<Button>();
       }
    }

    public void selectedButtonA(){
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
