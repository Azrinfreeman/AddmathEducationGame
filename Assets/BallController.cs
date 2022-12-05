using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static BallController instance;

    public bool[] Balls;
    public int num;


    private void Awake() {
        instance = this;
    }

   
    // Start is called before the first frame update
    void Start()
    {
       num = transform.GetChild(2).transform.childCount;
       Balls = new bool[num];
    }

    
    // Update is called once per frame
    void Update()
    {
    

    }
}
