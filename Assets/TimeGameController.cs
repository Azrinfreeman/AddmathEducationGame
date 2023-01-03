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
    // Start is called before the first frame update
    void Start()
    {
        
        text = transform.GetChild(2).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        prompted = transform.parent.transform.parent.GetChild(7).GetComponent<Animator>();
        timeoutGameobject = transform.parent.transform.parent.GetChild(8).GetComponent<Transform>();
        warningSFX = GameObject.Find("warningSFX").GetComponent<AudioSource>();
       
//        prompted = transform.parent.GetChild(0).GetComponent<GameObject>();
  //      timeoutGameobject = GameObject.Find("TimeUp").GetComponent<GameObject>();

    }

    IEnumerator OpenAndClose(){
        prompted.SetBool("isClosing",false);
            timeoutGameobject.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        prompted.SetBool("isClosing",true);
            timeoutGameobject.gameObject.SetActive(true);
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
            if(!isSwitch){
            Debug.Log(isSwitch);
            //QuizController.instance.CollectAndCheck();
            StartCoroutine(OpenAndClose());
            isSwitch = true;
            }
        }


        minit = Mathf.FloorToInt(timer/60);
        seconds = Mathf.FloorToInt(timer % 60);
        
        if(minit <=0 && seconds >=0 && seconds < 10){
        text.text = "0:0"+seconds.ToString();
        if(!isStopping){
            if(!warningSFX.isPlaying){

                warningSFX.Play(); 
                }
        }
                
            
        }else if (minit >= 1 && seconds < 10){
        text.text = minit+":0"+seconds.ToString();
        }else{
        text.text = minit+":"+seconds.ToString();
        }
    }
}
