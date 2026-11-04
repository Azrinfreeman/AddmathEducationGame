using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessonController : MonoBehaviour
{
    public CheckPages check;
    public bool[] noPages;
    // Start is called before the first frame update
    void Start()
    {
        
        check = GetComponent<CheckPages>();
        
        GetNumber();
    }
    public void GetNumber(){
        if (!check) check = GetComponent<CheckPages>();
        noPages = new bool[transform.childCount];
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void WriteKeyTahap1_2(){
        PlayerPrefs.SetInt("Tahap1_2", 1);
    }
    public void WriteKeyTahap1_3(){
        PlayerPrefs.SetInt("Tahap1_3", 1);
    }
    public void WriteKeyTahap1_4(){
        PlayerPrefs.SetInt("Tahap1_4", 1);
    }
}
