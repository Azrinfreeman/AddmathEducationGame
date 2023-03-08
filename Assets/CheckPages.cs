using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckPages : MonoBehaviour
{
    public int numberofPages;
    
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
            numberofPages =  transform.childCount;
            textLabel = quizNumber+ " daripada "+ numberofPages + " muka surat";
        
            text = GameObject.Find("textLabel").GetComponent<TextMeshProUGUI>();
            slider = GameObject.Find("Slider").GetComponent<Slider>();
            text.text = textLabel;

            slider.minValue = 1;
            slider.maxValue = numberofPages;

            slider.value = int.Parse(quizNumber);
        

    }

    // Update is called once per frame
    void Update()
    {
        setupPage();        
    }
}
