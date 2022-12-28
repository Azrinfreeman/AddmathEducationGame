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
    public int level;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();    
        level = 1;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Tahap "+ level;
    }
}
