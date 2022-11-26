using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonCorrect : MonoBehaviour
{
    public Button correctButton;
    public buttonSelectedController selectedButtonController;
    private int no;
    private int siblingno;
    // Start is called before the first frame update
    void Start()
    {
        correctButton = GetComponent<Button>();
        no = int.Parse(this.transform.parent.transform.parent.name);
        siblingno = transform.GetSiblingIndex();
        selectedButtonController = gameObject.transform.parent.GetComponent<buttonSelectedController>();
        //Debug.Log(transform.GetSiblingIndex());

        Invoke("StartLater", 0.2f);
    }

    void StartLater(){
           correctButton.onClick.AddListener(CheckButtonCondition);
        for(int i= 0; i<4; i++){
            
            if(selectedButtonController.answers[i].GetComponent<ButtonCorrect>() == null){
               // Debug.Log(selectedButtonController.answers[i].name);
                selectedButtonController.answers[i].onClick.AddListener(CheckButtonCondition);
            }
        }
    }

    public void CheckButtonCondition(){

       if(selectedButtonController.ispressed[siblingno] && selectedButtonController.answers[siblingno]){

       QuizController.instance.noQuiz[no-1] = true; 
       }else {
       QuizController.instance.noQuiz[no-1] = false; 
        
       }
    }
    public void changeColor(){
        ColorBlock color;
        color = correctButton.colors;
        color.normalColor = new Color32(113,255,0, 255);
        correctButton.colors = color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
