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
        
        Invoke("GetNumber", 0.5f);
    }
    public void GetNumber(){
        noPages = new bool[check.numberofPages];
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
