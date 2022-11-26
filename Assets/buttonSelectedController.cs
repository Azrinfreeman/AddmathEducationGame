using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class buttonSelectedController : MonoBehaviour
{
    public Button[] answers = new Button[4];
    public bool[] ispressed = new bool[4];
    // Start is called before the first frame update
    void Start()
    {
       for(int i =0; i < 4; i++){
        answers[i] = GameObject.Find("Answers").transform.GetChild(i).GetComponent<Button>();
        
       }
        answers[0].onClick.AddListener(() => selectedButton(0));
        answers[1].onClick.AddListener(() => selectedButton(1));
        answers[2].onClick.AddListener(() => selectedButton(2));
        answers[3].onClick.AddListener(() => selectedButton(3));

       
    }
    
    public void selectedButton(int index){
        ColorBlock colors;
        ColorBlock colors2;
        for(int i = 0; i <4; i++){
            colors = answers[i].colors;
            colors.normalColor = new Color32(255, 255, 255, 255);
            
            if(i!=index){
                ispressed[i] = false;
                answers[i].colors = colors;
            }
            
        }
        //Debug.Log(index + "index");
        colors2 = answers[index].colors;
        colors2.normalColor = new Color32(255,217,1,255);
        colors2.selectedColor = new Color32(255,217,1,255);
        answers[index].colors = colors2;
        ispressed[index] = true;
    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
}
