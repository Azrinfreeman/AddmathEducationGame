using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePraController : MonoBehaviour
{

    public static GamePraController instance;

    private void Awake() {
        instance = this;
    }
    public Transform[] bubbleAnswer;
    public Transform[] questions;

    public Transform EndPanel;
    public int level = 0;
    public int score = 0;

    public Rigidbody2D[] backgroundImage = new Rigidbody2D[2];
    // Start is called before the first frame update
    void Start()
    {
        bubbleAnswer = new Transform[transform.GetChild(4).transform.childCount];
        questions = new Transform[transform.GetChild(6).transform.childCount];
        EndPanel = transform.GetChild(13).GetComponent<Transform>();
        for(int i = 0; i < transform.GetChild(4).transform.childCount; i++){
            bubbleAnswer[i] = transform.GetChild(4).transform.GetChild(i).GetComponent<Transform>();
        }

        for(int i = 0 ; i < transform.GetChild(6).transform.childCount;i++){
            questions[i] = transform.GetChild(6).transform.GetChild(i).GetComponent<Transform>();
        }
    }

    public void ToNextLevel(){
        bubbleAnswer[level-1].gameObject.SetActive(false);
        questions[level-1].gameObject.SetActive(false);

        if(level < 10){
            
        bubbleAnswer[level].gameObject.SetActive(true);
        questions[level].gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(score < 0){
            score = 0;
        }
        if(level > 10){
            Destroy(backgroundImage[0].GetComponent<Rigidbody2D>());
            Destroy(backgroundImage[1].GetComponent<Rigidbody2D>());
            EndPanel.gameObject.SetActive(true);
            level = 10;
        }
    }
}
