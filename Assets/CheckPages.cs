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
            if (page == previousPage && language == previousLanguage && numberofPages == transform.childCount) return;
            previousPage = page;
            previousLanguage = language;
            var layout = page.GetComponent<FunMath.UI.LessonPageLayout>();
            if (layout)
            {
                text = layout.progressLabel as TextMeshProUGUI;
                slider = layout.progressBar;
            }
            else
            {
                text = null;
                foreach (var label in page.GetComponentsInChildren<TextMeshProUGUI>(true))
                    if (label.name == "textLabel") { text = label; break; }
                slider = page.GetComponentInChildren<Slider>(true);
            }
            quizNumber = page.name;
            numberofPages =  transform.childCount;
            if(PlayerPrefs.GetInt("EnglishLang") == 1){
                textLabel = "Page " + quizNumber + " of " + numberofPages;
            }else{
                textLabel = "Halaman " + quizNumber + " daripada " + numberofPages;
        
            }
            
            if(text == null || slider == null) return;
            text.text = textLabel;

            slider.minValue = 0;
            slider.maxValue = numberofPages;

            int current;
            if(int.TryParse(quizNumber, out current)) slider.value = current;
        

    }

    // Update is called once per frame
    void Update()
    {
        setupPage();        
    }
}
