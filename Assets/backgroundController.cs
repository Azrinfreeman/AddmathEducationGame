using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundController : MonoBehaviour
{
    public bool isToNextLevel;
    int level;
    // Start is called before the first frame update
    void Start()
    {
        level =  1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "imageBorder"){
            transform.localPosition = new Vector2(transform.localPosition.x, 10079);

            if(isToNextLevel){
                level++;
            }
        }
    }
}
