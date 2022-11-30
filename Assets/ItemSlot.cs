/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler {
    [Header("Indicator")]
    public GameObject indicator; 
    [Header("Variables")]
    public bool A;
    public bool T;
    public bool C;
    public bool G;
    [SerializeField] BoxCollider2D itemInsertedCol;
    [SerializeField] bool isItemInserted;

    [Header("Game4")]
    public bool Met;
    public bool Leu;
    public bool Val;
    public bool Pro;
    public bool Ala;

    public bool bridge;

    public int num;
    public int gameNum;
    public List<bool> isAlready;
    //public DragScript drag;
    private AudioSource slotInsertedSound;

    public ScoreController scoreController;
    private void Start()
    {
        slotInsertedSound = GameObject.Find("SlotInserted").GetComponent<AudioSource>();
        itemInsertedCol = GetComponent<BoxCollider2D>();
    }

  
    public void OnDrop(PointerEventData eventData) {
        Debug.Log("OnDrop");
        
        

        if (eventData.pointerDrag != null)
        {
           

                eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
                
                if (!slotInsertedSound.isPlaying)
                {
                    slotInsertedSound.Play();
                    indicator.SetActive(true); 
            }

            if (eventData.pointerDrag.tag == "DNA A (Temp)")
            {
                eventData.pointerDrag.tag = "DNA A";
            } else if (eventData.pointerDrag.tag == "DNA T (Temp)")
            {
                eventData.pointerDrag.tag = "DNA T";
            }
            else if (eventData.pointerDrag.tag == "DNA C (Temp)")
            {
                eventData.pointerDrag.tag = "DNA C";
            }
            else if (eventData.pointerDrag.tag == "DNA G (Temp)")
            {
                eventData.pointerDrag.tag = "DNA G";
            }
            else if (eventData.pointerDrag.tag == "Met (Temp)")
            {
                eventData.pointerDrag.tag = "Met";
            }
            else if (eventData.pointerDrag.tag == "Leu (Temp)")
            {
                eventData.pointerDrag.tag = "Leu";
            }
            else if (eventData.pointerDrag.tag == "Pro (Temp)")
            {
                eventData.pointerDrag.tag = "Pro";
            }
            else if (eventData.pointerDrag.tag == "Val (Temp)")
            {
                eventData.pointerDrag.tag = "Val";
            }
            else if (eventData.pointerDrag.tag == "Ala (Temp)")
            {
                eventData.pointerDrag.tag = "Ala";
            } else if (eventData.pointerDrag.tag == "bridge (temp)")
            {
                eventData.pointerDrag.tag = "bridge";
            }
            else if (eventData.pointerDrag.tag == "bridge 2 (temp)")
            {
                eventData.pointerDrag.tag = "bridge 2";
            }



            if (gameNum == 1)
            {


                if (num == 1 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().A);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[0] = true;
                }
                else if (num == 15 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 15");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[14] = true;

                }
                else if (num == 17 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 17");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[16] = true;
                }
                else if (num == 18 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[17] = true;
                }



                // DNA T

                else if (num == 2 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //  Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[1] = true;
                }
                else if (num == 5 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //  Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[4] = true;
                }
                else if (num == 8 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[7] = true;
                }
                else if (num == 12 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[11] = true;
                }
                else if (num == 16 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[15] = true;
                }

                //// DNA G
                ///
                else if (num == 3 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[2] = true;
                }
                else if (num == 7 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[6] = true;
                }
                else if (num == 9 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[8] = true;
                }
                else if (num == 13 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[12] = true;
                }


                ////DNA C
                ///
                else if (num == 4 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[3] = true;
                }
                else if (num == 6 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[5] = true;
                }
                else if (num == 10 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[9] = true;
                }
                else if (num == 11 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[10] = true;
                }

                else if (num == 14 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[13] = true;
                }




            }else if (gameNum == 2)
            {
                Debug.Log("Game Num 2 ");

                if (num == 2 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().A);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[1] = true;
                }
                else if (num == 5 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 15");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[4] = true;

                }
                else if (num == 8 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 17");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[7] = true;
                }
                else if (num == 12 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[11] = true;
                }
                else if (num == 16 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[15] = true;
                }


                // DNA T

                else if (num == 1 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //  Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[0] = true;
                }
                else if (num == 15 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //  Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[14] = true;
                }
                else if (num == 17 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[16] = true;
                }
                else if (num == 18 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[17] = true;
                }
                
                //// DNA G
                ///
                else if (num == 4 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[3] = true;
                }
                else if (num == 6 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[5] = true;
                }
                else if (num == 10 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[9] = true;
                }
                else if (num == 11 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[10] = true;
                }
                else if (num == 14 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[13] = true;
                }


                ////DNA C
                ///
                else if (num == 3 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[2] = true;
                }
                else if (num == 7 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[6] = true;
                }
                else if (num == 9 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[8] = true;
                }
                else if (num == 13 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[12] = true;
                }

            }
            else if (gameNum == 3)
            {
                Debug.Log("Game Num 2 ");

                if (num == 1 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().A);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[0] = true;
                }
                else if (num == 15 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 15");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[14] = true;

                }
                else if (num == 18 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 17");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[17] = true;
                }
                else if (num == 17 && A && eventData.pointerDrag.GetComponent<DragScript>().A)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[16] = true;
                }
               

                // DNA T

                else if (num == 2 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //  Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[1] = true;
                }
                else if (num == 5 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //  Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[4] = true;
                }
                else if (num == 8 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[7] = true;
                }
                else if (num == 12 && T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[11] = true;
                }
                else if (num == 16 & T && eventData.pointerDrag.GetComponent<DragScript>().T)
                {
                    //Debug.Log("statement tru num 1");
                    Debug.Log(eventData.pointerDrag.GetComponent<DragScript>().T);
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[15] = true;
                }

                //// DNA G
                ///
                else if (num == 3 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[2] = true;
                }
                else if (num == 7 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[6] = true;
                }
                else if (num == 9 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[8] = true;
                }
                else if (num == 13 && G && eventData.pointerDrag.GetComponent<DragScript>().G)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[12] = true;
                }
               

                ////DNA C
                ///
                else if (num == 4 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[3] = true;
                }
                else if (num == 6 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[5] = true;
                }
                else if (num == 10 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[9] = true;
                }
                else if (num == 11 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[10] = true;
                }
                else if (num == 14 && C && eventData.pointerDrag.GetComponent<DragScript>().C)
                {
                    Debug.Log("statement tru num 18");
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[13] = true;
                }

            }
            else if (gameNum == 4)
            {

                if (bridge && eventData.pointerDrag.GetComponent<DragScript>().bridge)
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[5] = true;
                }

                if (num == 1 && Met && eventData.pointerDrag.GetComponent<DragScript>().Met)
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[0] = true;
                }
                else if (num == 2 && Leu && eventData.pointerDrag.GetComponent<DragScript>().Leu)
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[1] = true;
                }
                else if (num == 3 && Val && eventData.pointerDrag.GetComponent<DragScript>().Val)
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[2] = true;
                }
                else if (num == 4 && Pro && eventData.pointerDrag.GetComponent<DragScript>().Pro)
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[3] = true;
                }
                else if (num == 5 && Ala && eventData.pointerDrag.GetComponent<DragScript>().Ala)
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[4] = true;
                }
                else if (num == 6)
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    scoreController.DNASlot[5] = true;
                }

                else if (num == 11 && eventData.pointerDrag.tag == "bridge")
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    CheckingBridge.Instance.isBridge[0] = true;
                }
                else if (num == 12 && eventData.pointerDrag.tag == "bridge")
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    CheckingBridge.Instance.isBridge[1] = true;
                }
                else if (num == 13 && eventData.pointerDrag.tag == "bridge")
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    CheckingBridge.Instance.isBridge[2] = true;
                }
                else if (num == 14 && eventData.pointerDrag.tag == "bridge")
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    CheckingBridge.Instance.isBridge[3] = true;
                }else if (num == 15 && eventData.pointerDrag.tag == "bridge")
                {
                    eventData.pointerDrag.GetComponent<DragScript>().firstTime = true;
                    CheckingBridge.Instance.isBridge[4] = true;
                }


            }

        }
        else
        {
            Debug.Log("else stateme");
        }
    }

}
