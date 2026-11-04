using UnityEngine;
using UnityEngine.UI;

public class GetMusic : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat("bgm", 1f);
    }
}
