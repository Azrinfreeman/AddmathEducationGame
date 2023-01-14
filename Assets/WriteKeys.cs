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

    public void Tahap2Level1(){
        PlayerPrefs.SetInt("Level1Tahap2", 1);
    }
    public void Tahap2Level2(){
        PlayerPrefs.SetInt("Level2Tahap2", 1);
    }
    public void Tahap2Level3(){
        PlayerPrefs.SetInt("Level3Tahap2", 1);
    }
    
    public void Tahap1Level1(){
        PlayerPrefs.SetInt("Level1Tahap1", 1);
    }
    public void Tahap1Level2(){
        PlayerPrefs.SetInt("Level2Tahap1", 1);
    }
    public void Tahap1Level3(){
        PlayerPrefs.SetInt("Level3Tahap1", 1);
    }
    public void Tahap1Level4(){
        PlayerPrefs.SetInt("Level4Tahap1", 1);
    }

    public void PrasekolahLevel1(){
        PlayerPrefs.SetInt("Level1Pra", 1);
        
    }
    public void PrasekolahLevel2(){
        PlayerPrefs.SetInt("Level2Pra", 1);
    }
    public void PrasekolahLevel3(){
        PlayerPrefs.SetInt("Level3Pra", 1);
    }
}
