using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class LoadScore : MonoBehaviour
{
    public QuizController quiz;

    private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        quiz = GameObject.Find("QuizController").GetComponent<QuizController>();
        text = GetComponent<TextMeshProUGUI>();

        text.text = "SCORE: "+quiz.score.ToString() +" / 10";    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
