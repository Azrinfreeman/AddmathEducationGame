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
        soundSFX = GameObject.Find("clickSFX").GetComponent<AudioSource>();
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
        soundSFX.Play();
    }

    public void isHiding(){
        animator.SetBool("isClosing", true);
        soundSFX.Play();
    }
}
