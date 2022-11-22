using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CheckQuizNumber : MonoBehaviour
{
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

            slider.minValue = int.Parse(quizNumber);
            slider.maxValue = numberofQuizzes;

            slider.value = slider.minValue;

    }

    // Update is called once per frame
    void Update()
    {
        setupPage();        
    }

}
