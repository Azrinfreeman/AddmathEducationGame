using System.Collections;
using UnityEngine;

public class ButtonChoiceController : MonoBehaviour
{
    public Animator animator;
    public bool isCorrect;
    private bool hasHit; // ignore double taps so the score is counted once

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    IEnumerator ending(){
        yield return new WaitForSeconds(2.5f);
        if(isCorrect){
            Game1Controller.instance.toCorrect++;
            Game1Controller.instance.score++;
        }else{
            Game1Controller.instance.score--;
        }
        animator.SetBool("isEnding", true);
    }

    public void hitting(){
        if(hasHit) return;
        hasHit = true;
        animator.SetBool("isHitting", true);
        StartCoroutine(ending());
    }
}
