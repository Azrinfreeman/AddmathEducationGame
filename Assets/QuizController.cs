using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizController : MonoBehaviour
{
    public static QuizController instance;
    public int score;
    private void Awake() {
        instance = this;
    }
    public CheckQuizNumber check;
    public bool[] noQuiz;
    // Start is called before the first frame update
    void Start()
    {
        check = GetComponent<CheckQuizNumber>();
        
        GetNumber();

        Debug.Log("score: "+PlayerPrefs.GetInt("QuizScoreEasy"));
        
    }

    public void GetNumber(){
        if(check == null) check = GetComponent<CheckQuizNumber>();
        noQuiz = new bool[Mathf.Max(0, check.numberofQuizzes)];
    }

    public void CollectAndCheck(){
        score = 0;
        if(noQuiz == null) return;
        for(int i = 0; i< noQuiz.Length; i++){
            if(noQuiz[i]){
                score++;
                Debug.Log(score);
            }


        }
    }

    public void WriteKeyEasy(){
        PlayerPrefs.SetInt("QuizScoreEasy", score);
    }

    public void WriteKeyMedium(){
        PlayerPrefs.SetInt("QuizScoreMedium", score);
    }

    public void WriteKeyHard(){
        PlayerPrefs.SetInt("QuizScoreHard", score);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
