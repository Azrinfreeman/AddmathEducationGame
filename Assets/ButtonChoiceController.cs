using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonChoiceController : MonoBehaviour
{
    public Animator animator;
    public bool isCorrect;
    // Start is called before the first frame update
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
        animator.SetBool("isHitting", true);
        StartCoroutine(ending());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
