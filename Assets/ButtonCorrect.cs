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
        if(!int.TryParse(this.transform.parent.transform.parent.name, out no)) Debug.LogWarning("ButtonCorrect: page name is not numeric: " + transform.parent.parent.name);
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

       bool[] quiz = QuizController.instance.noQuiz;
       if(quiz == null || no < 1 || no > quiz.Length) return;
       quiz[no-1] = selectedButtonController.ispressed[siblingno];
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
