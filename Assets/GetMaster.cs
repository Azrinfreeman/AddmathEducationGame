using UnityEngine;
using UnityEngine.UI;

public class GetMaster : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
    }
}
