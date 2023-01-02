using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TimeGameController : MonoBehaviour
{
      public float timer = 10.0f;
    TextMeshProUGUI text;
    public float minit;
    public float seconds;

    public bool isSwitch;

    [SerializeField]
    private AudioSource warningSFX;
    public Transform timeoutGameobject;
    public Transform prompted;
    // Start is called before the first frame update
    void Start()
    {
        
        text = transform.GetChild(2).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        prompted = transform.parent.transform.parent.GetChild(7).GetComponent<Transform>();
        timeoutGameobject = transform.parent.transform.parent.GetChild(8).GetComponent<Transform>();
        warningSFX = GameObject.Find("warningSFX").GetComponent<AudioSource>();
       
//        prompted = transform.parent.GetChild(0).GetComponent<GameObject>();
  //      timeoutGameobject = GameObject.Find("TimeUp").GetComponent<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {
        
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }else{
            if(!isSwitch){
            Debug.Log(isSwitch);
            //QuizController.instance.CollectAndCheck();
            prompted.gameObject.SetActive(true);
            timeoutGameobject.gameObject.SetActive(true);
            isSwitch = true;
            }
        }


        minit = Mathf.FloorToInt(timer/60);
        seconds = Mathf.FloorToInt(timer % 60);
        
        if(minit <=0 && seconds >=0 && seconds < 10){
        text.text = "0:0"+seconds.ToString();
            if(!warningSFX.isPlaying){

            warningSFX.Play(); 
            }
        }else if (minit >= 1 && seconds < 10){
        text.text = minit+":0"+seconds.ToString();
        }else{
        text.text = minit+":"+seconds.ToString();
        }
    }
}
