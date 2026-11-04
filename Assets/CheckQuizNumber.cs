using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CheckQuizNumber : MonoBehaviour
{
    public static CheckQuizNumber instance;

    private void Awake() {
        instance = this;
        numberofQuizzes = transform.childCount;
    }
    public int numberofQuizzes;

    public string quizNumber;
    public bool isNumber;
    public bool isIndicator;

    public string textLabel;
    TextMeshProUGUI text;
    public Slider slider;
    private Transform previousPage;
    private int previousLanguage = -1;
    // Start is called before the first frame update
    void Start()
    {

    }


    public void setupPage(){
            if(transform.childCount == 0) return;
            var page = transform.GetChild(0);
            int language = PlayerPrefs.GetInt("EnglishLang", 0);
            if (page == previousPage && language == previousLanguage && numberofQuizzes == transform.childCount) return;
            previousPage = page;
            previousLanguage = language;
            text = page.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(label => label.name == "textLabel");
            slider = page.GetComponentInChildren<Slider>(true);
            quizNumber = page.name;
            numberofQuizzes =  transform.childCount;
            if(PlayerPrefs.GetInt("EnglishLang") == 1){
                textLabel = "Question " + quizNumber + " of " + numberofQuizzes;
            }else{
                textLabel = quizNumber+ " daripada "+ numberofQuizzes + " kuiz";
            }


            if(text == null || slider == null) return;
            text.text = textLabel;

            slider.minValue = 1;
            slider.maxValue = numberofQuizzes;

            int current;
            if(int.TryParse(quizNumber, out current)) slider.value = current;


    }

    // Update is called once per frame
    void Update()
    {
        setupPage();
    }

}
