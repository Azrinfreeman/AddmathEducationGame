using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class loadScoreGame1 : MonoBehaviour
{
    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        if(Game1Controller.instance.score < 0 ){
            Game1Controller.instance.score = 0;
            text.text = Game1Controller.instance.score+" / "+ Game1Controller.instance.toCorrect;
        }else{
            text.text = Game1Controller.instance.score+" / "+ Game1Controller.instance.toCorrect;
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
