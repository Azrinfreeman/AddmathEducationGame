using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static BallController instance;

    private void Awake() {
        instance = this;
    }

    public int ballCount;
 
    public bool[] inContainer;
    // Start is called before the first frame update
    void Start()
    {
        ballCount = gameObject.transform.GetChild(2).gameObject.transform.childCount;
        inContainer = new bool[ballCount];
        
    }

    // Update is called once per frame
    void Update()
    {
       

    }
}
