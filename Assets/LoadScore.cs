using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LoadScore : MonoBehaviour
{
    public QuizController quiz;

    public bool pra;
    public bool t1;
    public bool t2;
    private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        quiz = GameObject.Find("QuizController").GetComponent<QuizController>();
        text = GetComponent<TextMeshProUGUI>();

        text.text = "SCORE: "+quiz.score.ToString() +" / " + CheckQuizNumber.instance.numberofQuizzes;

        if(quiz.score == CheckQuizNumber.instance.numberofQuizzes){
            if(pra){

            }else if (t1){

            }else if (t2){

            }
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
