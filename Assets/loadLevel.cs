using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class loadLevel : MonoBehaviour
{
    public static loadLevel instance;

    private void Awake() {
        instance = this;
    } 
    private TextMeshProUGUI text;
    

    public bool isScore;
    // Start is called before the first frame update
    void Start()
    {
    
            
        text = GetComponent<TextMeshProUGUI>();    
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isScore){
            text.text = "Tahap "+ GamePraController.instance.level;
        }else {
            text.text = "Markah ="+ GamePraController.instance.score;
        }
        
        if(GamePraController.instance.score >=120){
            PlayerPrefs.SetInt("PraGameKuiz_"+PlayerPrefs.GetInt("PlayerID"), 1);
        }
    }
}
