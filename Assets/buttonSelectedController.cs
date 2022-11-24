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
        answers[i] = GameObject.Find("Answers").transform.GetChild(i).GetComponent<Button>();
        answers[i].onClick.AddListener(selectedButton(1));
       }

       
    }

    public void selectedButton(int index){
        ColorBlock colors;
        ColorBlock colors2;
        for(int i = 0; i <4; i++){
            colors = answers[i].colors;
            colors.normalColor = new Color32(255, 255, 255, 255);
            
            if(i!=index){
                answers[i].colors = colors;
            }
            
        }
        colors2 = answers[index].colors;
        colors2.normalColor = new Color32(255,217,1,255);
        colors2.selectedColor = new Color32(255,217,1,255);
        answers[index].colors = colors2;
    }
    public void selectedButtonB(){
       ColorBlock colors;
        ColorBlock colors2;
        for(int i = 0; i <4; i++){
            colors = answers[i].colors;
            colors.normalColor = new Color32(255, 255, 255, 255);
            
            if(i!=1){
                answers[i].colors = colors;
            }
            
        }
        colors2 = answers[1].colors;
        colors2.normalColor = new Color32(255,217,1,255);
        colors2.selectedColor = new Color32(255,217,1,255);
        answers[1].colors = colors2;
    }
    public void selectedButtonC(){
        ColorBlock colors;
        ColorBlock colors2;
        for(int i = 0; i <4; i++){
            colors = answers[i].colors;
            colors.normalColor = new Color32(255, 255, 255, 255);
            
            if(i!=2){
                answers[i].colors = colors;
            }
            
        }
        colors2 = answers[2].colors;
        colors2.normalColor = new Color32(255,217,1,255);
        colors2.selectedColor = new Color32(255,217,1,255);
        answers[2].colors = colors2;
    }

    public void selectedButtonD(){
    ColorBlock colors;
        ColorBlock colors2;
        for(int i = 0; i <4; i++){
            colors = answers[i].colors;
            colors.normalColor = new Color32(255, 255, 255, 255);
            
            if(i!=3){
                answers[i].colors = colors;
            }
            
        }
        colors2 = answers[3].colors;
        colors2.normalColor = new Color32(255,217,1,255);
        colors2.selectedColor = new Color32(255,217,1,255);
        answers[3].colors = colors2;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
