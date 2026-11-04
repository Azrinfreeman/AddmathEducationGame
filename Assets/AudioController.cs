using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AudioController : MonoBehaviour
{
    // Start is called before the first frame update

    public Animator animator;
   [SerializeField]
    private AudioSource soundSFX;
    //public bool isAudioPanel;
    void Start()
    {
        var click = GameObject.Find("clickSFX");
        if (click) soundSFX = click.GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     IEnumerator loadGame(string scene){

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(scene);
    }

    public void LogOut(){

        StartCoroutine(loadGame("LoginAddAccountPage"));
    }
    public void isShowing(){
        animator.SetBool("isClosing", false);
        var celebration = GetComponent<FunMath.UI.ResultCelebration>();
        if (celebration) celebration.Play();
        if (soundSFX) soundSFX.Play();
    }

    public void isHiding(){
        animator.SetBool("isClosing", true);
        if (soundSFX) soundSFX.Play();
    }
}
