using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game1Controller : MonoBehaviour
{
    public static Game1Controller instance;

    private void Awake() {
        instance = this;
    }
    public bool[]answers;
    public Transform[] answerTrans;

    public int correctAnswers;
    public int toCorrect;
    public bool isEnded;
    public bool isEnding;

    public Transform winPanel;
    // Start is called before the first frame update
    void Start()
    {
        isEnding = false;
        answers = new bool[transform.childCount];
        answerTrans = new Transform[transform.childCount];
        winPanel = GameObject.Find("winPanel").GetComponent<Transform>();
        for(int i=0; i<transform.childCount; i++){
            answerTrans[i] = transform.GetChild(i);
        }
        
        
    }

  
    IEnumerator showWin(){
        yield return new WaitForSeconds(1.5f);
        winPanel.GetComponent<Animator>().SetBool("isClosing",false);
    }
    // Update is called once per frame
    void Update()
    {
        if(!isEnded){
            
            if(toCorrect == correctAnswers){
                isEnded = true;
            }

        }

        if(isEnded && !isEnding){
            isEnding = true;
            StartCoroutine(showWin());
        }
    }
}
