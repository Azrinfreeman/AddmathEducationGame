using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TimerController : MonoBehaviour
{
    public static TimerController instance;

    private void Awake() {
        instance = this;
    }
    public bool isStopping;
    public float timer = 10.0f;
    TextMeshProUGUI text;
    public float minit;
    public float seconds;

    public bool isSwitch;

    [SerializeField]
    private AudioSource warningSFX;
    [Tooltip("Existing TimeUp overlay on the quiz Canvas.")]
    public Transform timeoutGameobject;
    [Tooltip("Existing results panel (Prompted (2)), separate from submit confirmation.")]
    public Transform prompted;
    // Start is called before the first frame update
    void Start()
    {
        
        text = transform.GetChild(2).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        // Decorative siblings must not change which dialog a timeout opens.
        var canvas = GetComponentInParent<Canvas>();
        if (!prompted && canvas) prompted = canvas.transform.Find("Prompted (2)");
        if (!timeoutGameobject && canvas) timeoutGameobject = canvas.transform.Find("TimeUp");
        var warning = GameObject.Find("warningSFX");
        if (!warningSFX && warning) warningSFX = warning.GetComponent<AudioSource>();
       
//        prompted = transform.parent.GetChild(0).GetComponent<GameObject>();
  //      timeoutGameobject = GameObject.Find("TimeUp").GetComponent<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {
        
        if (timer > 0)
        {
            if(!isStopping){
                timer -= Time.deltaTime;
                
            }
        }else{
            if(!isSwitch){
            if(QuizController.instance != null) QuizController.instance.CollectAndCheck();
            if (prompted) prompted.gameObject.SetActive(true);
            if (timeoutGameobject) timeoutGameobject.gameObject.SetActive(true);
            isSwitch = true;
            }
        }
        if (timer < 0) timer = 0;
        minit = Mathf.FloorToInt(timer/60);
        seconds = Mathf.FloorToInt(timer % 60);

        text.text = Format(timer);
        if(!isStopping && timer > 0 && minit <= 0 && seconds < 10){
            if(warningSFX && !warningSFX.isPlaying){
                warningSFX.Play();
            }
        }
    }

    private static string Format(float t)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(t));
        return (total / 60) + ":" + (total % 60).ToString("00");
    }
}
