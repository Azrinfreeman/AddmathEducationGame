using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoController : MonoBehaviour
{
    public Transform[] input;
    public int numInput;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(transform.GetChild(1).transform.name);
        //numInput = transform.GetChild(1).name;
        //input = new Transform[numInput];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
