using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GetSFX : MonoBehaviour
{
    
    public Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat ("sfx");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
