using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CheckQuizNumber : MonoBehaviour
{
    public static CheckQuizNumber instance;

    private void Awake() {
        instance = this;
    }
    public int numberofQuizzes;
    
    public string quizNumber;
    public bool isNumber;
    public bool isIndicator;

    public string textLabel;
    TextMeshProUGUI text;
    public Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    public void setupPage(){
            quizNumber = transform.GetChild(0).name;
            numberofQuizzes =  transform.childCount;
            textLabel = quizNumber+ " out of "+ numberofQuizzes + " quiz";
        
            text = GameObject.Find("textLabel").GetComponent<TextMeshProUGUI>();
            slider = GameObject.Find("Slider").GetComponent<Slider>();
            text.text = textLabel;

            slider.minValue = 1;
            slider.maxValue = numberofQuizzes;

            slider.value = int.Parse(quizNumber);
        

    }

    // Update is called once per frame
    void Update()
    {
        setupPage();        
    }

}
