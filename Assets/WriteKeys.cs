using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WriteKeys : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    //Kuiz

    public void Kuiz0(){
        PlayerPrefs.SetInt("Kuiz0_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void Kuiz1(){
        PlayerPrefs.SetInt("Kuiz1_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void Kuiz2(){
        PlayerPrefs.SetInt("Kuiz2_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }



    //Game

    public void PraGame(){
        PlayerPrefs.SetInt("GamePra_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void GameLevel1(){
        PlayerPrefs.SetInt("GameLevel1_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void GameLevel2(){
        PlayerPrefs.SetInt("GameLevel2_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }

    //lessons
    public void Tahap2Level1(){
        PlayerPrefs.SetInt("Level1Tahap2_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void Tahap2Level2(){
        PlayerPrefs.SetInt("Level2Tahap2_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void Tahap2Level3(){
        PlayerPrefs.SetInt("Level3Tahap2_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    
    public void Tahap1Level1(){
        PlayerPrefs.SetInt("Level1Tahap1_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void Tahap1Level2(){
        PlayerPrefs.SetInt("Level2Tahap1_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void Tahap1Level3(){
        PlayerPrefs.SetInt("Level3Tahap1_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void Tahap1Level4(){
        PlayerPrefs.SetInt("Level4Tahap1_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }

    public void PrasekolahLevel1(){
        PlayerPrefs.SetInt("Level1Pra_"+PlayerPrefs.GetInt("PlayerID"), 1);
        
    }
    public void PrasekolahLevel2(){
        PlayerPrefs.SetInt("Level2Pra_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void PrasekolahLevel3(){
        PlayerPrefs.SetInt("Level3Pra_"+PlayerPrefs.GetInt("PlayerID"), 1);


    }

    public void menu1(){
        PlayerPrefs.SetInt("Menu1_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void menu2(){
        PlayerPrefs.SetInt("Menu2_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void menu3(){
        PlayerPrefs.SetInt("Menu3_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void menu4(){
        PlayerPrefs.SetInt("Menu4_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void menu5(){
        PlayerPrefs.SetInt("Menu5_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
    public void menu6(){
        PlayerPrefs.SetInt("Menu6_"+PlayerPrefs.GetInt("PlayerID"), 1);
    }
}
