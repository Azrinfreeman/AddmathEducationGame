using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class TimeGameController : MonoBehaviour
{

    public static TimeGameController instance;

    private void Awake() {
        instance = this;
    }
      public float timer = 10.0f;
    TextMeshProUGUI text;
    public float minit;
    public float seconds;

    public bool isSwitch;
    public bool isStopping;

    [SerializeField]
    private AudioSource warningSFX;
    public Transform timeoutGameobject;
    public Animator prompted;
    public event System.Action SecondsChanged;
    private int displayedSeconds = -1;
    // Start is called before the first frame update
    void Start()
    {
        
        text = transform.GetChild(2).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        // Explicit scene references avoid opening unrelated panels when siblings change.
        var canvas = GetComponentInParent<Canvas>();
        if (!prompted && canvas) prompted = canvas.transform.Find("againPanel")?.GetComponent<Animator>();
        if (!timeoutGameobject && canvas) timeoutGameobject = canvas.transform.Find("TimeUp");
        var warning = GameObject.Find("warningSFX");
        if (!warningSFX && warning) warningSFX = warning.GetComponent<AudioSource>();
       
//        prompted = transform.parent.GetChild(0).GetComponent<GameObject>();
  //      timeoutGameobject = GameObject.Find("TimeUp").GetComponent<GameObject>();

    }

    IEnumerator OpenAndClose(){
        if (prompted) prompted.SetBool("isClosing",false);
        if (timeoutGameobject) timeoutGameobject.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        if (prompted) prompted.SetBool("isClosing",true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            bool won = Game1Controller.instance != null && Game1Controller.instance.isEnded;
            if(!isSwitch && !won){
            //QuizController.instance.CollectAndCheck();
            StartCoroutine(OpenAndClose());
            isSwitch = true;
            }
        }


        if (timer < 0) timer = 0;
        minit = Mathf.FloorToInt(timer/60);
        seconds = Mathf.FloorToInt(timer % 60);

        text.text = Format(timer);
        int wholeSeconds = Mathf.Max(0, Mathf.CeilToInt(timer));
        if (displayedSeconds != wholeSeconds)
        {
            displayedSeconds = wholeSeconds;
            SecondsChanged?.Invoke();
        }
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
